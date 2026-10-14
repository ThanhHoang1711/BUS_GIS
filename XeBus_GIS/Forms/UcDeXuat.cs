using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using ShpfileType = MapWinGIS.ShpfileType;
using tkDashStyle = MapWinGIS.tkDashStyle;
using XeBus_GIS.Data;
using XeBus_GIS.Map;
using XeBus_GIS.Security;
using XeBus_GIS.UI;
using XeBus_GIS.UI.Controls;

namespace XeBus_GIS.Forms
{
    /// <summary>
    /// Quy trình 3 – Đề xuất trạm xe mới (wizard 5 bước):
    /// 3.1 Chọn vùng · 3.2 Khu vực thiếu trạm · 3.3 Ứng viên · 3.4 So sánh · 3.5 Đề xuất (tạo trạm trạng thái "Đề xuất").
    /// </summary>
    public partial class UcDeXuat : UserControl, IModule
    {
        private const string LopVung = "DX_VUNG";
        private const string LopThieu = "DX_THIEU";
        private const string LopUngVien = "DX_UNGVIEN";
        private const string LopVong = "DX_VONG";

        private enum DangCho { KhongCo, VeVung, DanhGiaDiem }

        private readonly BoiCanh _bc;
        private Chart _chart;
        private ThanhBuoc _thanhBuoc;
        private int _buocXaNhat = 1;     // bước xa nhất đã tới với kết quả hiện tại
        private int _buoc = 1;
        private bool _daNap;
        private DangCho _dangCho = DangCho.KhongCo;

        // kết quả phân tích
        private string _vungVe;          // WKT 3857 vùng người dùng vẽ
        private string _vung;            // vùng đang phân tích (null = toàn TP)
        private string _tenVung;
        private double _banKinh;
        private DataTable _thieu;        // khu vực thiếu trạm
        private DataTable _soSanh;       // ứng viên + vị trí tự chọn (cột "#" kiểu chuỗi)
        private DataRow _chon;           // ứng viên được chọn để đề xuất
        private int _soTuChon;

        /// <summary>Dùng cho VS Designer.</summary>
        public UcDeXuat()
        {
            InitializeComponent();
        }

        public UcDeXuat(BoiCanh boiCanh) : this()
        {
            _bc = boiCanh;
            ApDungGiaoDien();
            TaoBieuDo();
            GanSuKien();
            DenBuoc(1);
            TaoTab();
        }

        // =====================================================================
        // Hai tab: "Đề xuất theo vùng" (wizard 5 bước – Người quản lý) | "Chọn vị trí đặt trạm" (đa tiêu chí – Quản trị viên)
        // =====================================================================
        private UcChonViTri _chonViTri;
        private Panel _pnlTheoVung, _pnlTab;
        private Button _tabTheoVung, _tabChonViTri;
        private bool _oChonViTri;
        private bool _dangHien;

        private bool CoTheoVung => Session.Co(Quyen.DxChonVung);

        private void TaoTab()
        {
            if (!Session.Co(Quyen.DxChonViTri)) return;   // chỉ có wizard như cũ
            SuspendLayout();
            // gom toàn bộ nội dung wizard vào một panel (giữ nguyên thứ tự dock)
            var cu = Controls.Cast<Control>().ToArray();
            Controls.Clear();
            _pnlTheoVung = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface };
            _pnlTheoVung.Controls.AddRange(cu);
            _chonViTri = new UcChonViTri(_bc) { Dock = DockStyle.Fill };
            Controls.Add(_chonViTri);
            Controls.Add(_pnlTheoVung);
            if (CoTheoVung)
            {
                _pnlTab = new Panel { Dock = DockStyle.Top, Height = UiHelper.Dp(40), BackColor = Theme.Surface, Padding = new Padding(UiHelper.Dp(6), 0, UiHelper.Dp(6), 0) };
                _tabTheoVung = new Button { Text = "Theo vùng (5 bước)", Dock = DockStyle.Left };
                _tabChonViTri = new Button { Text = "Chọn vị trí đặt trạm", Dock = DockStyle.Left };
                foreach (var b in new[] { _tabTheoVung, _tabChonViTri })
                {
                    UiHelper.Tab(b);
                    b.Width = TextRenderer.MeasureText(b.Text, Theme.FontDam).Width + UiHelper.Dp(24);
                }
                _pnlTab.Controls.Add(_tabChonViTri);
                _pnlTab.Controls.Add(_tabTheoVung);
                UiHelper.DuongKe(_pnlTab, 0);
                Controls.Add(_pnlTab);
                _tabTheoVung.Click += (s, e) => ChonTab(false);
                _tabChonViTri.Click += (s, e) => ChonTab(true);
            }
            ResumeLayout();
            DatTab(!CoTheoVung);
        }

        private void DatTab(bool chonViTri)
        {
            _oChonViTri = chonViTri;
            if (_pnlTheoVung == null) return;
            _pnlTheoVung.Visible = !chonViTri;
            _chonViTri.Visible = chonViTri;
            if (_pnlTab != null)
            {
                UiHelper.ChonTab(_tabTheoVung, !chonViTri);
                UiHelper.ChonTab(_tabChonViTri, chonViTri);
            }
        }

        private void ChonTab(bool chonViTri)
        {
            if (chonViTri == _oChonViTri) return;
            if (_dangHien)
            {
                if (chonViTri) NgungTheoVung(); else _chonViTri.NgungKichHoat();
            }
            DatTab(chonViTri);
            if (_dangHien)
            {
                if (chonViTri) _chonViTri.KichHoat(); else KichHoatTheoVung();
            }
        }

        // =====================================================================
        // Giao diện
        // =====================================================================
        private void ApDungGiaoDien()
        {
            BackColor = Theme.Surface;
            lblTieuDe.Font = Theme.FontTieuDe;
            lblTieuDe.ForeColor = Theme.Primary;
            foreach (var p in new[] { pnlB1, pnlB2, pnlB3, pnlB4, pnlB5 }) p.BackColor = Theme.Surface;
            foreach (var l in new[] { lblH1, lblH2, lblH3, lblH4, lblH5 }) UiHelper.TieuDeKhuVuc(l);

            // thanh bước tự vẽ thay 5 nhãn bước cũ
            foreach (var l in new[] { lblB1, lblB2, lblB3, lblB4, lblB5 }) l.Visible = false;
            _thanhBuoc = new ThanhBuoc
            {
                Dock = DockStyle.Fill,
                CacBuoc = new[] { "Vùng", "Thiếu trạm", "Ứng viên", "So sánh", "Đề xuất" },
            };
            pnlBuoc.Controls.Add(_thanhBuoc);
            pnlBuoc.Height = UiHelper.Dp(60);
            pnlBuoc.BackColor = Theme.Surface;
            UiHelper.DuongKe(pnlBuoc, 0);

            // chú giải bản đồ ở bước 2 và 3 – lưới bên dưới dời xuống nhường chỗ
            var cg2 = new KhungChuGiai { Location = new Point(lblGhiChu2.Left, lblGhiChu2.Bottom + UiHelper.Dp(2)), Width = lblGhiChu2.Width };
            cg2.Dat(new ChuGiai("Khu dân cư thiếu trạm", Theme.Danger, KieuKyHieu.Vung),
                    new ChuGiai("Vùng phân tích", Theme.Accent, KieuKyHieu.DuongDut));
            pnlB2.Controls.Add(cg2);
            DoiXuong(dgvThieu, cg2.Bottom + UiHelper.Dp(6));
            var cg3 = new KhungChuGiai { Location = new Point(lblGhiChu3.Left, lblGhiChu3.Bottom + UiHelper.Dp(2)), Width = lblGhiChu3.Width };
            cg3.Dat(new ChuGiai("Ứng viên (số = thứ tự chọn)", Theme.Highlight, KieuKyHieu.Tron),
                    new ChuGiai("Trạm hiện có", Theme.MapTram, KieuKyHieu.Tron),
                    new ChuGiai("Vùng phục vụ ứng viên", Theme.Success, KieuKyHieu.Vung));
            pnlB3.Controls.Add(cg3);
            DoiXuong(dgvUngVien, cg3.Bottom + UiHelper.Dp(6));
            foreach (var r in new[] { rdoVeVung, rdoPhuong, rdoToanTP }) { r.ForeColor = Theme.Text; r.Cursor = Cursors.Hand; }
            chkHienLop.ForeColor = Theme.Text;
            chkHienLop.Cursor = Cursors.Hand;
            lblThamSo.Font = Theme.FontDam;
            lblThamSo.ForeColor = Theme.Text;
            foreach (var l in new[] { lblBanKinh, lblCach, lblNguong, lblToiDa, lblTen, lblLoai, lblTuyen })
                UiHelper.NhanTruong(l);
            foreach (var l in new[] { lblVung, lblGhiChu2, lblGhiChu3 })
            {
                l.Font = Theme.FontNhan;
                l.ForeColor = Theme.Muted;
            }
            foreach (var l in new[] { lblKpi1, lblKpi2, lblKpi3 })
            {
                l.Font = Theme.FontDam;
                l.BackColor = Theme.SurfaceAlt;
                l.ForeColor = Theme.Primary;
            }
            lblChon.Font = Theme.FontThuong;
            lblChon.ForeColor = Theme.Text;
            lblChon.BackColor = Theme.AccentSoft;
            lblChon.Padding = new Padding(8);
            lblLoi.ForeColor = Theme.Danger;
            lblLoi.Font = Theme.FontNhan;
            lblKetQua.Font = Theme.FontThuong;

            UiHelper.NutPhu(btnVe);
            UiHelper.NutPhu(btnDanhGia);
            UiHelper.NutChinh(btnTao);
            UiHelper.NutPhu(btnQuayLai);
            UiHelper.NutChinh(btnTiep);
            UiHelper.Luoi(dgvThieu);
            UiHelper.Luoi(dgvUngVien);
            UiHelper.Luoi(dgvSoSanh);
            UiHelper.TipCho(btnVe, "Click trái thêm đỉnh, double-click hoặc click phải để kết thúc");
            UiHelper.TipCho(nudNguong, "Khu dân cư có tỷ lệ diện tích nằm trong bán kính phục vụ thấp hơn ngưỡng này được coi là thiếu trạm");
            UiHelper.TipCho(btnDanhGia, "Click một vị trí bất kỳ trên bản đồ để tính chỉ tiêu và so sánh với các ứng viên");
            UiHelper.GanIcon(btnVe, Icons.VeVung);
            UiHelper.GanIcon(btnDanhGia, Icons.BanKinh);
            UiHelper.O(cboPhuong, nudBanKinh, nudCach, nudNguong, nudToiDa, txtTen, cboLoai, cboTuyen);
            UiHelper.TrangThaiRong(dgvThieu, "Không có khu dân cư thiếu trạm trong vùng với tiêu chí hiện tại.");
            UiHelper.TrangThaiRong(dgvUngVien, "Không tìm được vị trí ứng viên – vùng đã được phục vụ đủ hoặc không còn vị trí phù hợp.");
            pnlDieuHuong.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.Border)) e.Graphics.DrawLine(pen, 0, 0, pnlDieuHuong.Width, 0);
            };
            foreach (var p in new Control[] { pnlB1, pnlB2, pnlB3, pnlB4, pnlB5, pnlDieuHuong }) UiHelper.CoGianNgang(p);
        }

        /// <summary>Dời control (đã neo 4 phía) xuống vị trí y, giữ nguyên mép dưới.</summary>
        private static void DoiXuong(Control c, int y)
        {
            int day = c.Bottom;
            c.Top = y;
            c.Height = Math.Max(UiHelper.Dp(60), day - y);
        }

        private void TaoBieuDo()
        {
            _chart = new Chart { Dock = DockStyle.Fill, BackColor = Theme.Surface };
            var area = new ChartArea("a") { BackColor = Theme.Surface };
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.LabelStyle.Font = Theme.FontNhan;
            area.AxisX.LabelStyle.ForeColor = Theme.Muted;
            area.AxisX.LineColor = Theme.Border;
            area.AxisX.Interval = 1;
            area.AxisY.MajorGrid.LineColor = Theme.Border;
            area.AxisY.LabelStyle.Font = Theme.FontNhan;
            area.AxisY.LabelStyle.ForeColor = Theme.Muted;
            area.AxisY.LineColor = Theme.Border;
            area.AxisY.Title = "Tổng điểm";
            area.AxisY.TitleFont = Theme.FontNhan;
            area.AxisY.TitleForeColor = Theme.Muted;
            _chart.ChartAreas.Add(area);
            var lg = new Legend("l") { Docking = Docking.Top, Font = Theme.FontNhan, ForeColor = Theme.Muted, BackColor = Theme.Surface };
            _chart.Legends.Add(lg);
            _chart.Series.Add(new Series("Điểm dân số chưa phục vụ") { ChartType = SeriesChartType.StackedBar, Color = Theme.Accent, Legend = "l" });
            _chart.Series.Add(new Series("Điểm công trình chưa phục vụ") { ChartType = SeriesChartType.StackedBar, Color = Theme.Warning, Legend = "l" });
            UiHelper.BieuDo(_chart);
            area.AxisY.Title = "Tổng điểm (3 điểm / 1.000 dân + Σ trọng số công trình)";
            foreach (var s in _chart.Series) { s["PointWidth"] = "0.6"; s.BorderWidth = 0; }
            pnlBieuDo.Controls.Add(_chart);
        }

        private void DenBuoc(int buoc)
        {
            _buoc = buoc;
            var panels = new[] { pnlB1, pnlB2, pnlB3, pnlB4, pnlB5 };
            for (int i = 0; i < 5; i++) panels[i].Visible = i + 1 == buoc;
            // thanh bước: đánh dấu xong các bước đã đi qua với kết quả hiện tại
            _buocXaNhat = Math.Max(_buocXaNhat, buoc);
            _thanhBuoc.HienTai = buoc - 1;
            _thanhBuoc.DatLaiTienDo(_buocXaNhat - 1);
            panels[buoc - 1].BringToFront();
            btnQuayLai.Enabled = buoc > 1;
            switch (buoc)
            {
                case 1: btnTiep.Text = "Phân tích khu vực thiếu trạm"; break;
                case 2: btnTiep.Text = "Tìm vị trí ứng viên"; break;
                case 3: btnTiep.Text = "So sánh ứng viên"; break;
                case 4: btnTiep.Text = "Đề xuất ứng viên đã chọn"; break;
                case 5: btnTiep.Text = "Phân tích vùng mới"; break;
            }
            btnTiep.Enabled = true;
        }

        // =====================================================================
        // Sự kiện
        // =====================================================================
        private void GanSuKien()
        {
            btnTiep.Click += async (s, e) => await TiepAsync();
            btnQuayLai.Click += (s, e) => { if (_buoc > 1) DenBuoc(_buoc - 1); };
            _thanhBuoc.BuocDuocChon += i =>
            {
                int b = i + 1;
                // chỉ đi tới bước đã tới trước đó và còn dữ liệu
                if (b <= _buocXaNhat && (b == 1 || (b == 2 && _thieu != null) || (b == 3 && _soSanh != null) ||
                    (b == 4 && _soSanh != null) || (b == 5 && _chon != null)))
                {
                    if (b == 4) HienSoSanh();
                    if (b == 5) ChuanBiDeXuat();
                    DenBuoc(b);
                }
            };
            btnVe.Click += (s, e) =>
            {
                rdoVeVung.Checked = true;
                _dangCho = DangCho.VeVung;
                _bc.CongCu.Dat(CongCu.VeVung);
                _bc.TrangThai("Vẽ vùng phân tích: click trái thêm đỉnh, double-click / click phải để kết thúc.", false);
            };
            cboPhuong.SelectionChangeCommitted += (s, e) => rdoPhuong.Checked = true;
            cboPhuong.TextUpdate += (s, e) => rdoPhuong.Checked = true;
            btnDanhGia.Click += (s, e) =>
            {
                _dangCho = DangCho.DanhGiaDiem;
                _bc.CongCu.Dat(CongCu.ChonDiem);
                _bc.TrangThai("Click vị trí muốn đánh giá trên bản đồ.", false);
            };
            dgvUngVien.SelectionChanged += (s, e) => { if (dgvUngVien.Focused) ChonUngVien(dgvUngVien); };
            dgvSoSanh.SelectionChanged += (s, e) => { if (dgvSoSanh.Focused) ChonUngVien(dgvSoSanh); };
            btnTao.Click += async (s, e) => await TaoTramAsync();
            txtTen.TextChanged += (s, e) => lblLoi.Visible = false;

            _bc.CongCu.VeVungXong += async wkt => await KhiVeVungXongAsync(wkt);
            _bc.CongCu.ChonDiemXong += async (x, y) => await KhiChonDiemAsync(x, y);
        }

        // =====================================================================
        // IModule
        // =====================================================================
        public void KichHoat()
        {
            _dangHien = true;
            if (_oChonViTri) _chonViTri.KichHoat();
            else KichHoatTheoVung();
        }

        public void NgungKichHoat()
        {
            _dangHien = false;
            if (_chonViTri != null) _chonViTri.NgungKichHoat();
            NgungTheoVung();
        }

        private async void KichHoatTheoVung()
        {
            if (!_daNap)
            {
                try
                {
                    var phuong = await Task.Run(() => TramRepository.PhuongXa());
                    var loai = await Task.Run(() => TramRepository.LoaiTram());
                    var tuyen = await Task.Run(() => TramRepository.Tuyen());
                    UcTram.GanCombo(cboPhuong, phuong, null);
                    cboPhuong.SelectedIndex = -1;
                    UcTram.GanCombo(cboLoai, loai, null);
                    cboLoai.SelectedValue = "TD";
                    UcTram.GanCombo(cboTuyen, tuyen, "(Chưa gán tuyến)");
                    _daNap = true;
                    _datVungCho?.Invoke();
                    _datVungCho = null;
                }
                catch (Exception ex)
                {
                    _bc.TrangThai("Lỗi nạp danh mục: " + ex.GetBaseException().Message, true);
                }
                CapNhatMoTaVung();
            }
            else
            {
                VeLaiBanDo();
            }
            if (!Session.Co(Quyen.DxDeXuat))
            {
                btnTao.Enabled = false;
                lblLoi.Text = "Tài khoản không có quyền tạo trạm đề xuất.";
                lblLoi.Visible = true;
            }
        }

        private void NgungTheoVung()
        {
            if (_dangCho != DangCho.KhongCo)
            {
                _dangCho = DangCho.KhongCo;
                _bc.CongCu.Dat(CongCu.ChonDoiTuong);
            }
            foreach (var l in new[] { LopVung, LopThieu, LopUngVien, LopVong }) _bc.BanDo.XoaLopTam(l);
        }

        private Action _datVungCho;   // vùng nhận từ màn hình Phân tích khi danh mục chưa nạp xong

        /// <summary>
        /// Nhận vùng và tiêu chí từ màn hình Phân tích khu vực thiếu trạm (maPhuong, hoặc wktVe 3857, hoặc cả hai null = toàn TP),
        /// quay về bước 1 để người dùng xem lại rồi bấm phân tích.
        /// </summary>
        public void DatVung(string maPhuong, string wktVe, double banKinh, double nguong)
        {
            if (_pnlTab != null) ChonTab(false);   // vùng từ màn hình Phân tích dùng cho wizard theo vùng
            Action dat = () =>
            {
                nudBanKinh.Value = Math.Max(nudBanKinh.Minimum, Math.Min(nudBanKinh.Maximum, (decimal)banKinh));
                nudNguong.Value = Math.Max(nudNguong.Minimum, Math.Min(nudNguong.Maximum, (decimal)Math.Round(nguong * 100)));
                if (maPhuong != null)
                {
                    rdoPhuong.Checked = true;
                    cboPhuong.SelectedValue = maPhuong;
                }
                else if (wktVe != null)
                {
                    rdoVeVung.Checked = true;
                    _vungVe = wktVe;
                    VeVung(wktVe);
                }
                else rdoToanTP.Checked = true;
                foreach (var l in new[] { LopThieu, LopUngVien, LopVong }) _bc.BanDo.XoaLopTam(l);
                _thieu = null; _soSanh = null; _chon = null;
                _buocXaNhat = 1;
                CapNhatMoTaVung();
                DenBuoc(1);
                _bc.TrangThai("Đã nhận vùng và tiêu chí từ màn hình Phân tích – bấm \"" + btnTiep.Text + "\" để tiếp tục.", false);
            };
            if (_daNap) dat(); else _datVungCho = dat;
        }

        // =====================================================================
        // Điều hướng
        // =====================================================================
        private async Task TiepAsync()
        {
            switch (_buoc)
            {
                case 1: await PhanTichThieuTramAsync(); break;
                case 2: await TimUngVienAsync(); break;
                case 3: HienSoSanh(); DenBuoc(4); break;
                case 4:
                    if (_chon == null) { UiHelper.CanhBao(FindForm(), "Chọn một ứng viên trong bảng so sánh (hoặc trên biểu đồ) để đề xuất."); return; }
                    ChuanBiDeXuat();
                    DenBuoc(5);
                    break;
                case 5:
                    foreach (var l in new[] { LopThieu, LopUngVien, LopVong }) _bc.BanDo.XoaLopTam(l);
                    _thieu = null; _soSanh = null; _chon = null;
                    _buocXaNhat = 1;
                    DenBuoc(1);
                    break;
            }
        }

        // =====================================================================
        // Bước 1 – Chọn vùng
        // =====================================================================
        private async Task KhiVeVungXongAsync(string wkt)
        {
            if (_dangCho != DangCho.VeVung) return;
            _dangCho = DangCho.KhongCo;
            _vungVe = wkt;
            rdoVeVung.Checked = true;
            VeVung(wkt);
            try
            {
                var dt = await Task.Run(() => DeXuatRepository.DienTichKm2(wkt));
                CapNhatMoTaVung(dt);
            }
            catch { CapNhatMoTaVung(); }
        }

        private void CapNhatMoTaVung(double? dienTich = null)
        {
            lblVung.Text = _vungVe == null
                ? "Chưa vẽ vùng. Chọn một cách xác định vùng phân tích ở trên."
                : $"Đã vẽ vùng phân tích{(dienTich.HasValue ? $" – diện tích {dienTich:N2} km²" : "")}.";
        }

        private void VeVung(string wkt)
        {
            if (wkt == null) { _bc.BanDo.XoaLopTam(LopVung); return; }
            var t = BangWkt(wkt);
            _bc.BanDo.HienLopTam(LopVung, "Vùng phân tích", ShpfileType.SHP_POLYGON, t, sf =>
            {
                MapManager.KieuVung(Theme.Accent, 20)(sf);
                sf.DefaultDrawingOptions.LineWidth = 2.5f;
                sf.DefaultDrawingOptions.LineStipple = tkDashStyle.dsDash;
            });
        }

        private static DataTable BangWkt(string wkt)
        {
            var t = new DataTable();
            t.Columns.Add("ma");
            t.Columns.Add("wkt");
            t.Rows.Add("VUNG", wkt);
            return t;
        }

        // =====================================================================
        // Bước 2 – Khu vực thiếu trạm
        // =====================================================================
        private async Task PhanTichThieuTramAsync()
        {
            // xác định vùng
            string vung, ten;
            if (rdoVeVung.Checked)
            {
                if (_vungVe == null) { UiHelper.CanhBao(FindForm(), "Hãy bấm \"Vẽ vùng\" và khoanh vùng cần phân tích trên bản đồ."); return; }
                vung = _vungVe;
                ten = "vùng đã vẽ";
            }
            else if (rdoPhuong.Checked)
            {
                var ma = cboPhuong.SelectedValue as string;
                if (string.IsNullOrEmpty(ma)) { UiHelper.CanhBao(FindForm(), "Chọn phường/xã cần phân tích."); cboPhuong.Focus(); return; }
                DataRow p = null;
                if (!await UiHelper.ChayAsync(btnTiep, async () => p = await Task.Run(() => DeXuatRepository.VungPhuong(ma)), "Đang đọc ranh giới…")) return;
                vung = Db.Str(p, "wkt");
                ten = Db.Str(p, "tenphuong");
            }
            else
            {
                vung = null;
                ten = "toàn thành phố";
            }

            double r = (double)nudBanKinh.Value, nguong = (double)nudNguong.Value / 100.0;
            DataRow tq = null;
            DataTable thieu = null;
            if (!await UiHelper.ChayAsync(btnTiep, () => Task.Run(() =>
                {
                    tq = DeXuatRepository.TongQuan(vung, r);
                    thieu = DeXuatRepository.KhuVucThieuTram(vung, r, nguong);
                }), "Đang phân tích…"))
                return;

            _vung = vung;
            _tenVung = ten;
            _banKinh = r;
            _thieu = thieu;
            _soSanh = null;
            _chon = null;
            _bc.BanDo.XoaLopTam(LopUngVien);
            _bc.BanDo.XoaLopTam(LopVong);

            // KPI
            long danChua = thieu.Rows.Cast<DataRow>().Sum(x => Convert.ToInt64(x["Dân chưa PV"]));
            lblKpi1.Text = $"{danChua:N0}\nngười chưa có trạm";
            lblKpi2.Text = $"{thieu.Rows.Count:N0}\nkhu thiếu trạm";
            var tyLe = tq == null || tq["tyle_pv"] == DBNull.Value ? "—" : $"{Convert.ToDouble(tq["tyle_pv"]):0.#}%";
            lblKpi3.Text = $"{tyLe}\ndân được phục vụ";
            lblKpi1.ForeColor = danChua > 0 ? Theme.Danger : Theme.Success;
            lblH2.Text = $"Bước 2 · Khu vực thiếu trạm – {ten}";

            dgvThieu.DataSource = thieu;
            foreach (var c in new[] { "ma", "wkt", "Dân số" })
                if (dgvThieu.Columns.Contains(c)) dgvThieu.Columns[c].Visible = false;
            if (dgvThieu.Columns.Contains("Khu dân cư")) dgvThieu.Columns["Khu dân cư"].FillWeight = 80;
            if (dgvThieu.Columns.Contains("Phường/xã")) dgvThieu.Columns["Phường/xã"].Visible = vung == null;
            UiHelper.CanPhaiCotSo(dgvThieu);

            if (chkHienLop.Checked)
            {
                _bc.BanDo.HienLop(MapManager.KhuDanCu, true);
                _bc.BanDo.HienLop(MapManager.CongTrinh, true);
            }
            VeLaiBanDo(zoom: true);
            _bc.TrangThai($"Phân tích {ten}: {thieu.Rows.Count} khu thiếu trạm, {danChua:N0} người chưa có trạm trong {r:N0} m.", false);
            _buocXaNhat = 2;
            DenBuoc(2);
        }

        // =====================================================================
        // Bước 3 – Ứng viên
        // =====================================================================
        private async Task TimUngVienAsync()
        {
            double cach = (double)nudCach.Value, nguong = (double)nudNguong.Value / 100.0;
            int toiDa = (int)nudToiDa.Value;
            DataTable uv = null;
            if (!await UiHelper.ChayAsync(btnTiep, async () =>
                    uv = await Task.Run(() => DeXuatRepository.UngVien(_vung, _banKinh, cach, toiDa, nguong)),
                    "Đang tìm ứng viên…"))
                return;

            _soSanh = TaoBangSoSanh(uv);
            _soTuChon = 0;
            _chon = null;
            dgvUngVien.DataSource = _soSanh;
            DinhDangLuoiUngVien(dgvUngVien, rutGon: true);
            lblH3.Text = $"Bước 3 · {_soSanh.Rows.Count} vị trí ứng viên – {_tenVung}";
            VeLaiBanDo(zoom: false);
            _bc.TrangThai(_soSanh.Rows.Count == 0
                ? "Không tìm được ứng viên: vùng đã được phục vụ đủ hoặc không có vị trí phù hợp."
                : $"Tìm được {_soSanh.Rows.Count} ứng viên (điểm cam đánh số trên bản đồ).", false);
            _buocXaNhat = 3;
            DenBuoc(3);
        }

        /// <summary>Bảng so sánh: cột "#" kiểu chuỗi để thêm được vị trí tự chọn (TC1, TC2…).</summary>
        private static DataTable TaoBangSoSanh(DataTable uv)
        {
            var t = new DataTable();
            t.Columns.Add("#");
            t.Columns.Add("Vị trí");
            t.Columns.Add("Đường / khu");
            t.Columns.Add("Phường/xã");
            t.Columns.Add("Dân PV thêm", typeof(int));
            t.Columns.Add("CT PV thêm", typeof(int));
            t.Columns.Add("Điểm CT", typeof(decimal));
            t.Columns.Add("Tổng điểm", typeof(decimal));
            t.Columns.Add("Cách trạm cũ (m)", typeof(int));
            t.Columns.Add("kinhdo", typeof(double));
            t.Columns.Add("vido", typeof(double));
            t.Columns.Add("wkt");
            foreach (DataRow r in uv.Rows)
                t.Rows.Add(Convert.ToString(r["#"]), r["Vị trí"], r["Đường / khu"], r["Phường/xã"],
                           r["Dân PV thêm"], r["CT PV thêm"], r["Điểm CT"], r["Tổng điểm"], r["Cách trạm cũ (m)"],
                           Convert.ToDouble(r["kinhdo"]), Convert.ToDouble(r["vido"]), r["wkt"]);
            return t;
        }

        private static void DinhDangLuoiUngVien(DataGridView g, bool rutGon)
        {
            foreach (var c in new[] { "kinhdo", "vido", "wkt" })
                if (g.Columns.Contains(c)) g.Columns[c].Visible = false;
            UiHelper.CotVuaDu(g, "#", "Dân PV thêm", "CT PV thêm", "Tổng điểm");
            g.Columns["Đường / khu"].FillWeight = 100;
            g.Columns["Vị trí"].Visible = !rutGon;
            g.Columns["Phường/xã"].Visible = !rutGon;
            g.Columns["Điểm CT"].Visible = !rutGon;
            g.Columns["Cách trạm cũ (m)"].Visible = !rutGon;
            g.Columns["Dân PV thêm"].HeaderText = "Dân +";
            g.Columns["CT PV thêm"].HeaderText = "CT +";
            g.Columns["Tổng điểm"].HeaderText = "Điểm";
            g.Columns["Tổng điểm"].DefaultCellStyle.Font = Theme.FontDam;
            g.Columns["Dân PV thêm"].DefaultCellStyle.Format = "N0";
            if (!rutGon)
            {
                g.Columns["Cách trạm cũ (m)"].HeaderText = "Cách trạm (m)";
                g.Columns["Phường/xã"].FillWeight = 50;
            }
            UiHelper.CanPhaiCotSo(g);
            foreach (DataGridViewColumn c in g.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
            g.ClearSelection();
        }

        // =====================================================================
        // Bước 4 – So sánh
        // =====================================================================
        private void HienSoSanh()
        {
            if (_soSanh == null) return;
            var view = _soSanh.DefaultView;
            view.Sort = "[Tổng điểm] DESC";
            dgvSoSanh.DataSource = view;
            DinhDangLuoiUngVien(dgvSoSanh, rutGon: false);
            VeBieuDo();
            lblH4.Text = $"Bước 4 · So sánh {_soSanh.Rows.Count} phương án – {_tenVung}";
            if (_chon != null) ChonDongTheoMa(dgvSoSanh, Convert.ToString(_chon["#"]));
        }

        private void VeBieuDo()
        {
            var sDan = _chart.Series[0];
            var sCt = _chart.Series[1];
            sDan.Points.Clear();
            sCt.Points.Clear();
            // StackedBar vẽ từ dưới lên => đảo thứ tự để phương án điểm cao nằm trên cùng
            var dong = _soSanh.Rows.Cast<DataRow>().OrderBy(r => Convert.ToDecimal(r["Tổng điểm"])).ToList();
            foreach (var r in dong)
            {
                var nhan = "#" + r["#"];
                double dan = 3.0 * Convert.ToInt32(r["Dân PV thêm"]) / 1000.0;
                double ct = Convert.ToDouble(r["Điểm CT"]);
                int i = sDan.Points.AddXY(nhan, Math.Round(dan, 2));
                sDan.Points[i].ToolTip = $"{nhan}: {Convert.ToInt32(r["Dân PV thêm"]):N0} dân chưa phục vụ → {dan:0.##} điểm";
                int j = sCt.Points.AddXY(nhan, Math.Round(ct, 2));
                sCt.Points[j].ToolTip = $"{nhan}: {r["CT PV thêm"]} công trình chưa phục vụ → {ct:0.##} điểm";
                if (_chon != null && Convert.ToString(_chon["#"]) == Convert.ToString(r["#"]))
                {
                    sDan.Points[i].Color = Theme.Highlight;
                    sCt.Points[j].Color = ControlPaint.Light(Theme.Highlight, 0.5f);
                }
            }
            _chart.ChartAreas[0].RecalculateAxesScale();
        }

        private async Task KhiChonDiemAsync(double x, double y)
        {
            if (_dangCho != DangCho.DanhGiaDiem) return;
            _dangCho = DangCho.KhongCo;
            DataRow d = null;
            if (!await UiHelper.ChayAsync(btnDanhGia, async () => d = await Task.Run(() => DeXuatRepository.DanhGiaDiem(x, y, _banKinh)),
                    "Đang đánh giá…"))
                return;
            if (d == null) return;
            if (d["dat_duoc"] != DBNull.Value && !Convert.ToBoolean(d["dat_duoc"]))
                UiHelper.CanhBao(FindForm(), "Lưu ý: vị trí này nằm trên mặt nước (sông/hồ) – không đặt trạm được. Vẫn thêm vào bảng để so sánh.");

            _soTuChon++;
            var ma = "TC" + _soTuChon;
            string wkt = $"POINT({x.ToString(System.Globalization.CultureInfo.InvariantCulture)} {y.ToString(System.Globalization.CultureInfo.InvariantCulture)})";
            _soSanh.Rows.Add(ma, "Tự chọn", Db.Str(d, "tenduong") ?? "", Db.Str(d, "tenphuong") ?? "",
                             Convert.ToInt32(d["dan_moi"]), Convert.ToInt32(d["so_ct_moi"]),
                             Convert.ToDecimal(d["diem_ct"]), Convert.ToDecimal(d["tong_diem"]),
                             d["kc_tram_gan_nhat"] == DBNull.Value ? 0 : Convert.ToInt32(d["kc_tram_gan_nhat"]),
                             Convert.ToDouble(d["kinhdo"]), Convert.ToDouble(d["vido"]), wkt);
            HienSoSanh();
            VeLaiBanDo(zoom: false);
            ChonDongTheoMa(dgvSoSanh, ma);
            ChonUngVien(dgvSoSanh);
            _bc.TrangThai($"Vị trí tự chọn {ma}: {Convert.ToInt32(d["dan_moi"]):N0} dân và {d["so_ct_moi"]} công trình chưa có trạm được phục vụ thêm – tổng điểm {d["tong_diem"]}.", false);
        }

        private static void ChonDongTheoMa(DataGridView g, string ma)
        {
            foreach (DataGridViewRow row in g.Rows)
            {
                if (Convert.ToString(row.Cells["#"].Value) != ma) continue;
                g.CurrentCell = row.Cells["#"];
                row.Selected = true;
                return;
            }
        }

        private async void ChonUngVien(DataGridView g)
        {
            if (g.CurrentRow == null || _soSanh == null) return;
            var ma = Convert.ToString(g.CurrentRow.Cells["#"].Value);
            _chon = _soSanh.Rows.Cast<DataRow>().FirstOrDefault(r => Convert.ToString(r["#"]) == ma);
            if (_chon == null) return;
            if (_buoc == 4) VeBieuDo();
            var l = _bc.BanDo.Lop(LopUngVien);
            if (l != null) _bc.BanDo.Chon(LopUngVien, new[] { ma }, zoom: false);
            _bc.BanDo.ZoomToi(LopUngVien, ma);
            try
            {
                double lon = Convert.ToDouble(_chon["kinhdo"]), lat = Convert.ToDouble(_chon["vido"]);
                var vong = await Task.Run(() => DeXuatRepository.WktVongPhucVu(lon, lat, _banKinh));
                var t = BangWkt(vong);
                _bc.BanDo.HienLopTam(LopVong, "Vùng phục vụ ứng viên", ShpfileType.SHP_POLYGON, t, MapManager.KieuVung(Theme.Success, 55));
                _bc.BanDo.Map.MoveLayerTop(_bc.BanDo.Map.get_LayerPosition(_bc.BanDo.Lop(LopUngVien).Handle));
                _bc.BanDo.Map.Redraw();
            }
            catch { /* chỉ là hiển thị phụ */ }
        }

        // =====================================================================
        // Bước 5 – Đề xuất
        // =====================================================================
        private void ChuanBiDeXuat()
        {
            var r = _chon;
            lblChon.Text =
                $"Phương án #{r["#"]} – {r["Vị trí"]}: {r["Đường / khu"]}\n" +
                $"{r["Phường/xã"]} · toạ độ {Convert.ToDouble(r["vido"]):0.000000}, {Convert.ToDouble(r["kinhdo"]):0.000000}\n" +
                $"Phục vụ thêm {Convert.ToInt32(r["Dân PV thêm"]):N0} dân (ước tính) và {r["CT PV thêm"]} công trình\n" +
                $"Tổng điểm {r["Tổng điểm"]} · cách trạm hiện có {r["Cách trạm cũ (m)"]} m";
            var tenGoiY = Convert.ToString(r["Đường / khu"]);
            txtTen.Text = "Trạm đề xuất " + (string.IsNullOrWhiteSpace(tenGoiY) ? "#" + r["#"] : tenGoiY);
            lblKetQua.Text = "";
            lblLoi.Visible = !Session.Co(Quyen.DxDeXuat);
            btnTao.Enabled = Session.Co(Quyen.DxDeXuat);
        }

        private async Task TaoTramAsync()
        {
            if (_chon == null || !Session.Co(Quyen.DxDeXuat)) return;
            var ten = txtTen.Text.Trim();
            if (ten.Length == 0)
            {
                lblLoi.Text = "Tên trạm không được để trống.";
                lblLoi.Visible = true;
                txtTen.Focus();
                return;
            }
            if (!UiHelper.XacNhan(FindForm(), $"Tạo trạm \"{ten}\" với trạng thái \"Đề xuất\" tại vị trí phương án #{_chon["#"]}?")) return;

            double lon = Convert.ToDouble(_chon["kinhdo"]), lat = Convert.ToDouble(_chon["vido"]);
            string loai = cboLoai.SelectedValue as string ?? "TD", tuyen = cboTuyen.SelectedValue as string;
            string maMoi = null;
            if (!await UiHelper.ChayAsync(btnTao, async () => maMoi = await Task.Run(() =>
                    DeXuatRepository.TaoTramDeXuat(lon, lat, ten, loai, tuyen, Session.IdNguoiDung,
                                                   Session.TenNguoiDung ?? Session.TenDangNhap, _banKinh)),
                    "Đang tạo trạm…"))
                return;

            lblKetQua.ForeColor = Theme.Success;
            lblKetQua.Text = $"Đã tạo trạm {maMoi} – \"{ten}\" (trạng thái Đề xuất).\n" +
                             "Trạm hiển thị màu cam trên bản đồ; có thể chỉnh sửa ở tab Trạm.\n" +
                             "Kết quả phân tích cũ chưa tính trạm mới – bấm \"Phân tích vùng mới\" để chạy lại.";
            btnTao.Enabled = false;
            UiHelper.ThanhCong(FindForm(), $"Đã tạo trạm đề xuất {maMoi}.");
            await _bc.BanDo.NapLaiAsync(MapManager.Tram);
            _bc.BanDo.Chon(MapManager.Tram, new[] { maMoi }, zoom: true);
            _bc.HienThongTin("TRAM", maMoi);
            _bc.TrangThai($"Đã tạo trạm đề xuất {maMoi}.", false);
        }

        // =====================================================================
        // Bản đồ
        // =====================================================================
        private void VeLaiBanDo(bool zoom = false)
        {
            VeVung(_vung);
            if (_thieu != null && _thieu.Rows.Count > 0)
            {
                _bc.BanDo.HienLopTam(LopThieu, "Khu vực thiếu trạm", ShpfileType.SHP_POLYGON, BangTam(_thieu, "ma"),
                                     MapManager.KieuVung(Theme.Danger, 110));
            }
            else _bc.BanDo.XoaLopTam(LopThieu);

            if (_soSanh != null && _soSanh.Rows.Count > 0)
            {
                _bc.BanDo.HienLopTam(LopUngVien, "Ứng viên trạm", ShpfileType.SHP_POINT, BangTam(_soSanh, "#"),
                                     MapManager.KieuDiemTam(Theme.Highlight, 13));
                _bc.BanDo.DatNhan(LopUngVien, "[ma]", Theme.Primary);
            }
            else _bc.BanDo.XoaLopTam(LopUngVien);

            if (zoom)
            {
                var l = _bc.BanDo.Lop(_vung != null ? LopVung : LopThieu);
                if (l != null) _bc.BanDo.Map.ZoomToLayer(l.Handle);
                else _bc.BanDo.ToanCanh();
            }
        }

        private static DataTable BangTam(DataTable nguon, string cotMa)
        {
            var t = new DataTable();
            t.Columns.Add("ma");
            t.Columns.Add("wkt");
            foreach (DataRow r in nguon.Rows) t.Rows.Add(Convert.ToString(r[cotMa]), Convert.ToString(r["wkt"]));
            return t;
        }
    }
}
