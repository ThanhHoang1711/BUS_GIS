using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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
    /// Phân tích khu vực thiếu trạm (Quy trình 3.2 tách thành màn hình riêng): chọn vùng → tiêu chí → kết quả
    /// (KPI, danh sách khu dân cư thiếu trạm, lớp bản đồ có chú giải). Dùng đúng hàm CSDL fn_khuvuc_thieutram
    /// như bước 2 của màn hình Đề xuất; có thể chuyển vùng sang Đề xuất để tìm vị trí trạm mới.
    /// </summary>
    public partial class UcPhanTich : UserControl, IModule
    {
        private const string LopVung = "PT_VUNG";
        private const string LopThieu = "PT_THIEU";
        private const string LopPhucVu = "PT_PHUCVU";

        private readonly BoiCanh _bc;
        private Action<bool> _moVung, _moTieuChi;
        private bool _daNap, _dangChoVe;
        private string _vungVe;            // WKT 3857 vùng người dùng vẽ
        private string _vung, _tenVung;    // vùng của kết quả hiện tại (null = toàn TP)
        private string _maPhuong;          // phường của kết quả hiện tại (nếu phân tích theo phường)
        private double _banKinh, _nguong;
        private DataTable _thieu;
        private DataTable _phucVu;

        /// <summary>Dùng cho VS Designer.</summary>
        public UcPhanTich()
        {
            InitializeComponent();
        }

        public UcPhanTich(BoiCanh boiCanh) : this()
        {
            _bc = boiCanh;
            ApDungGiaoDien();
            GanSuKien();
            HienKetQuaRong();
        }

        // =====================================================================
        // Khởi tạo
        // =====================================================================
        private void ApDungGiaoDien()
        {
            BackColor = Theme.Surface;
            foreach (var l in new[] { lblHdrVung, lblHdrTieuChi, lblHdrKetQua }) UiHelper.TieuDeKhuVuc(l);
            foreach (var l in new[] { lblBanKinh, lblNguong }) UiHelper.NhanTruong(l);
            UiHelper.ChuPhu(lblVung);
            UiHelper.ChuPhu(lblGiaiThich);
            foreach (var r in new[] { rdoToanTP, rdoPhuong, rdoVeVung }) { r.ForeColor = Theme.Text; r.Cursor = Cursors.Hand; }
            chkVungPV.ForeColor = Theme.Text;
            chkVungPV.Cursor = Cursors.Hand;
            UiHelper.O(cboPhuong, nudBanKinh, nudNguong);
            UiHelper.NutPhu(btnVe);
            UiHelper.GanIcon(btnVe, Icons.VeVung);
            UiHelper.NutChinh(btnPhanTich);
            UiHelper.NutPhu(btnDeXuat);
            UiHelper.GanIcon(btnDeXuat, Icons.DeXuat);
            UiHelper.NutPhu(btnXoa);
            UiHelper.TipCho(btnVe, "Click trái thêm đỉnh, double-click hoặc click phải để kết thúc vùng");
            UiHelper.TipCho(nudBanKinh, "Khoảng cách đi bộ tới trạm được coi là được phục vụ");
            UiHelper.TipCho(nudNguong, "Khu dân cư có phần diện tích nằm trong bán kính phục vụ thấp hơn ngưỡng này được coi là thiếu trạm");
            UiHelper.TipCho(btnDeXuat, "Mở màn hình Đề xuất vị trí trạm mới với cùng vùng và tiêu chí");

            var fontKpi = new Font("Segoe UI Semibold", 14f);
            foreach (var k in new[] { kpiDan, kpiKhu, kpiTyLe }) k.FontGiaTri = fontKpi;
            kpiDan.MauNhan = Theme.Danger;
            kpiTyLe.MauNhan = Theme.Accent2;

            chuGiai.Dat(new ChuGiai("Khu dân cư thiếu trạm", Theme.MapThieuTram, KieuKyHieu.Vung),
                        new ChuGiai("Vùng đã được phục vụ", Theme.MapVungPhucVu, KieuKyHieu.Vung),
                        new ChuGiai("Trạm hiện có", Theme.MapTram, KieuKyHieu.Tron),
                        new ChuGiai("Ranh giới vùng", Theme.Accent, KieuKyHieu.DuongDut));

            UiHelper.Luoi(dgvThieu);
            UiHelper.TrangThaiRong(dgvThieu, "Chưa có kết quả.\nChọn vùng, kiểm tra tiêu chí rồi bấm Phân tích.");
            foreach (var p in new[] { pnlVung, pnlTieuChi }) UiHelper.DuongKe(p);
            pnlNut.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.Border)) e.Graphics.DrawLine(pen, 0, 0, pnlNut.Width, 0);
            };
            foreach (var p in new Control[] { pnlVung, pnlTieuChi, pnlKetQua, pnlNut }) UiHelper.CoGianNgang(p);
            _moVung = UiHelper.KhuVucThuGon(pnlVung, lblHdrVung);
            _moTieuChi = UiHelper.KhuVucThuGon(pnlTieuChi, lblHdrTieuChi);
            // chú giải tự xuống dòng theo chiều rộng – đẩy ô đánh dấu và chiều cao khu kết quả theo
            chuGiai.Resize += (s, e) => XepKhuKetQua();
            XepKhuKetQua();
            CapNhatGiaiThich();

            bool coDeXuat = Session.Co(Quyen.DxUngVien) || Session.Co(Quyen.DxDeXuat);
            btnDeXuat.Visible = coDeXuat;
            if (!coDeXuat) btnXoa.Left = btnDeXuat.Left;
        }

        private void XepKhuKetQua()
        {
            chkVungPV.Top = chuGiai.Bottom + UiHelper.Dp(4);
            pnlKetQua.Height = chkVungPV.Bottom + UiHelper.Dp(10);
        }

        private void GanSuKien()
        {
            btnPhanTich.Click += async (s, e) => await PhanTichAsync();
            btnXoa.Click += (s, e) => XoaKetQua();
            btnDeXuat.Click += (s, e) => ChuyenDeXuat();
            btnVe.Click += (s, e) =>
            {
                rdoVeVung.Checked = true;
                _dangChoVe = true;
                _bc.CongCu.Dat(CongCu.VeVung);
                _bc.TrangThai("Vẽ vùng phân tích: click trái thêm đỉnh, double-click / click phải để kết thúc.", false);
            };
            cboPhuong.SelectionChangeCommitted += (s, e) => rdoPhuong.Checked = true;
            cboPhuong.TextUpdate += (s, e) => rdoPhuong.Checked = true;
            foreach (var r in new[] { rdoToanTP, rdoPhuong, rdoVeVung }) r.CheckedChanged += (s, e) => CapNhatMoTaVung();
            nudBanKinh.ValueChanged += (s, e) => CapNhatGiaiThich();
            nudNguong.ValueChanged += (s, e) => CapNhatGiaiThich();
            chkVungPV.CheckedChanged += async (s, e) => await VeVungPhucVuAsync();
            dgvThieu.SelectionChanged += (s, e) =>
            {
                if (!dgvThieu.Focused || dgvThieu.CurrentRow == null || !dgvThieu.Columns.Contains("ma")) return;
                var ma = Convert.ToString(dgvThieu.CurrentRow.Cells["ma"].Value);
                _bc.BanDo.Chon(LopThieu, new[] { ma }, zoom: false);
                _bc.BanDo.ZoomToi(LopThieu, ma);
                _bc.HienThongTin("KDC", ma);
            };
            _bc.CongCu.VeVungXong += async wkt => await KhiVeVungXongAsync(wkt);
        }

        // =====================================================================
        // IModule
        // =====================================================================
        public async void KichHoat()
        {
            if (!_daNap)
            {
                try
                {
                    var phuong = await Task.Run(() => TramRepository.PhuongXa());
                    UcTram.GanCombo(cboPhuong, phuong, null);
                    cboPhuong.SelectedIndex = -1;
                    _daNap = true;
                }
                catch (Exception ex)
                {
                    _bc.TrangThai("Lỗi nạp danh mục: " + ex.GetBaseException().Message, true);
                }
                CapNhatMoTaVung();
            }
            else
            {
                await VeLaiBanDoAsync(zoom: false);
            }
        }

        public void NgungKichHoat()
        {
            if (_dangChoVe)
            {
                _dangChoVe = false;
                _bc.CongCu.Dat(MapTools.MacDinh);
            }
            foreach (var l in new[] { LopVung, LopThieu, LopPhucVu }) _bc.BanDo.XoaLopTam(l);
        }

        // =====================================================================
        // Vùng / tiêu chí
        // =====================================================================
        private async Task KhiVeVungXongAsync(string wkt)
        {
            if (!_dangChoVe) return;
            _dangChoVe = false;
            _vungVe = wkt;
            rdoVeVung.Checked = true;
            VeRanhGioi(wkt);
            try
            {
                var dt = await Task.Run(() => DeXuatRepository.DienTichKm2(wkt));
                lblVung.Text = $"Đã vẽ vùng – diện tích {dt:N2} km². Bấm Phân tích.";
            }
            catch { CapNhatMoTaVung(); }
        }

        private void CapNhatMoTaVung()
        {
            if (rdoToanTP.Checked) lblVung.Text = "Toàn bộ 126 phường/xã của Hà Nội (khoảng 5 giây).";
            else if (rdoPhuong.Checked) lblVung.Text = "Chọn hoặc gõ tên phường/xã (không dấu cũng được).";
            else lblVung.Text = _vungVe == null ? "Chưa vẽ vùng – bấm \"Vẽ vùng trên bản đồ\"." : "Đã vẽ vùng. Bấm Phân tích.";
        }

        private void CapNhatGiaiThich()
        {
            lblGiaiThich.Text =
                $"Khu dân cư được coi là thiếu trạm khi dưới {nudNguong.Value:N0}% diện tích của nó nằm trong " +
                $"{nudBanKinh.Value:N0} m quanh một trạm đang hoạt động. Dân số khu dân cư là số ước tính " +
                "(phân bổ dân số phường theo diện tích).";
        }

        // =====================================================================
        // Phân tích
        // =====================================================================
        private async Task PhanTichAsync()
        {
            string vung, ten, maPhuong = null;
            if (rdoVeVung.Checked)
            {
                if (_vungVe == null) { UiHelper.CanhBao(FindForm(), "Hãy bấm \"Vẽ vùng trên bản đồ\" và khoanh vùng cần phân tích."); return; }
                vung = _vungVe;
                ten = "vùng đã vẽ";
            }
            else if (rdoPhuong.Checked)
            {
                maPhuong = cboPhuong.SelectedValue as string;
                if (string.IsNullOrEmpty(maPhuong)) { UiHelper.CanhBao(FindForm(), "Chọn phường/xã cần phân tích."); cboPhuong.Focus(); return; }
                DataRow p = null;
                var ma = maPhuong;
                if (!await UiHelper.ChayAsync(btnPhanTich, async () => p = await Task.Run(() => DeXuatRepository.VungPhuong(ma)), "Đang đọc ranh giới…")) return;
                if (p == null) return;
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
            DataTable phucVu = null;
            bool layPhucVu = chkVungPV.Checked;
            if (!await UiHelper.ChayAsync(btnPhanTich, () => Task.Run(() =>
                {
                    tq = DeXuatRepository.TongQuan(vung, r);
                    thieu = DeXuatRepository.KhuVucThieuTram(vung, r, nguong);
                    if (layPhucVu) phucVu = DeXuatRepository.VungPhucVu(vung, r);
                }), "Đang phân tích…"))
                return;

            _vung = vung;
            _tenVung = ten;
            _maPhuong = maPhuong;
            _banKinh = r;
            _nguong = nguong;
            _thieu = thieu;
            _phucVu = phucVu;

            long danChua = thieu.Rows.Cast<DataRow>().Sum(x => Convert.ToInt64(x["Dân chưa PV"]));
            kpiDan.Dat(danChua.ToString("N0"), "người (ước tính)", danChua > 0 ? Theme.Danger : Theme.Success);
            kpiKhu.Dat(thieu.Rows.Count.ToString("N0"), "khu dân cư");
            kpiTyLe.Dat(tq == null || tq["tyle_pv"] == DBNull.Value ? null : $"{Convert.ToDouble(tq["tyle_pv"]):0.#}%",
                        $"trong {r:N0} m");
            lblHdrKetQua.Text = "3. Kết quả – " + ten;

            dgvThieu.DataSource = thieu;
            foreach (var c in new[] { "ma", "wkt", "Dân số" })
                if (dgvThieu.Columns.Contains(c)) dgvThieu.Columns[c].Visible = false;
            if (dgvThieu.Columns.Contains("Khu dân cư")) dgvThieu.Columns["Khu dân cư"].FillWeight = 120;
            if (dgvThieu.Columns.Contains("Phường/xã")) dgvThieu.Columns["Phường/xã"].Visible = vung == null || maPhuong == null;
            if (dgvThieu.Columns.Contains("Dân chưa PV")) dgvThieu.Columns["Dân chưa PV"].DefaultCellStyle.Format = "N0";
            if (dgvThieu.Columns.Contains("Trạm gần nhất (m)")) dgvThieu.Columns["Trạm gần nhất (m)"].HeaderText = "Trạm gần";
            if (dgvThieu.Columns.Contains("% phục vụ")) dgvThieu.Columns["% phục vụ"].HeaderText = "% PV";
            if (dgvThieu.Columns.Contains("Dân chưa PV")) dgvThieu.Columns["Dân chưa PV"].HeaderText = "Chưa PV";
            UiHelper.CotVuaDu(dgvThieu, "% phục vụ", "Dân chưa PV", "Trạm gần nhất (m)");
            UiHelper.CanPhaiCotSo(dgvThieu);
            dgvThieu.ClearSelection();
            UiHelper.DatChuRong(dgvThieu, $"Không có khu dân cư nào thiếu trạm trong {ten} với tiêu chí hiện tại.");
            btnDeXuat.Enabled = true;
            // thu gọn phần nhập để lưới kết quả có chỗ (click tiêu đề để mở lại)
            _moVung?.Invoke(false);
            _moTieuChi?.Invoke(false);

            await VeLaiBanDoAsync(zoom: true);
            _bc.TrangThai($"Phân tích {ten}: {thieu.Rows.Count} khu dân cư thiếu trạm, khoảng {danChua:N0} người chưa có trạm trong {r:N0} m.", false);
        }

        private void HienKetQuaRong()
        {
            kpiDan.Dat(null, "người (ước tính)");
            kpiKhu.Dat(null, "khu dân cư");
            kpiTyLe.Dat(null, "trong bán kính");
            lblHdrKetQua.Text = "3. Kết quả";
            btnDeXuat.Enabled = false;
        }

        private void XoaKetQua()
        {
            _thieu = null;
            _vung = null;
            _phucVu = null;
            dgvThieu.DataSource = null;
            UiHelper.DatChuRong(dgvThieu, "Chưa có kết quả.\nChọn vùng, kiểm tra tiêu chí rồi bấm Phân tích.");
            HienKetQuaRong();
            foreach (var l in new[] { LopThieu, LopPhucVu }) _bc.BanDo.XoaLopTam(l);
            VeRanhGioi(rdoVeVung.Checked ? _vungVe : null);
        }

        /// <summary>Mở màn hình Đề xuất với cùng vùng và tiêu chí.</summary>
        private void ChuyenDeXuat()
        {
            if (_thieu == null) return;
            var uc = _bc.MoModule?.Invoke("DEXUAT") as UcDeXuat;
            uc?.DatVung(_maPhuong, _maPhuong == null ? _vung : null, _banKinh, _nguong);
        }

        // =====================================================================
        // Bản đồ
        // =====================================================================
        private void VeRanhGioi(string wkt)
        {
            if (wkt == null) { _bc.BanDo.XoaLopTam(LopVung); return; }
            _bc.BanDo.HienLopTam(LopVung, "Vùng phân tích", ShpfileType.SHP_POLYGON, BangWkt("VUNG", wkt), sf =>
            {
                MapManager.KieuVung(Theme.Accent, 0)(sf);
                sf.DefaultDrawingOptions.FillVisible = false;
                sf.DefaultDrawingOptions.LineWidth = 2.5f;
                sf.DefaultDrawingOptions.LineStipple = tkDashStyle.dsDash;
            });
        }

        private async Task VeVungPhucVuAsync()
        {
            if (!chkVungPV.Checked) { _bc.BanDo.XoaLopTam(LopPhucVu); return; }
            if (_thieu == null) return;
            if (_phucVu == null)
            {
                string vung = _vung;
                double r = _banKinh;
                try { _phucVu = await Task.Run(() => DeXuatRepository.VungPhucVu(vung, r)); }
                catch (Exception ex) { _bc.TrangThai("Lỗi tính vùng phục vụ: " + ex.GetBaseException().Message, true); return; }
            }
            if (_phucVu.Rows.Count == 0) { _bc.BanDo.XoaLopTam(LopPhucVu); return; }
            _bc.BanDo.HienLopTam(LopPhucVu, "Vùng đã được phục vụ", ShpfileType.SHP_POLYGON, _phucVu,
                                 MapManager.KieuVung(Theme.MapVungPhucVu, 45));
            // vùng phục vụ nằm dưới khu thiếu trạm và ranh giới
            foreach (var k in new[] { LopThieu, LopVung })
            {
                var l = _bc.BanDo.Lop(k);
                if (l != null) _bc.BanDo.Map.MoveLayerTop(_bc.BanDo.Map.get_LayerPosition(l.Handle));
            }
            _bc.BanDo.Map.Redraw();
        }

        private async Task VeLaiBanDoAsync(bool zoom)
        {
            VeRanhGioi(_vung ?? (rdoVeVung.Checked ? _vungVe : null));
            if (_thieu != null && _thieu.Rows.Count > 0)
            {
                var t = new DataTable();
                t.Columns.Add("ma");
                t.Columns.Add("wkt");
                foreach (DataRow r in _thieu.Rows) t.Rows.Add(Convert.ToString(r["ma"]), Convert.ToString(r["wkt"]));
                _bc.BanDo.HienLopTam(LopThieu, "Khu dân cư thiếu trạm", ShpfileType.SHP_POLYGON, t, sf =>
                {
                    MapManager.KieuVung(Theme.MapThieuTram, 130)(sf);
                    sf.DefaultDrawingOptions.LineColor = Theme.Ole(Theme.Danger);
                });
            }
            else _bc.BanDo.XoaLopTam(LopThieu);
            if (_thieu != null) await VeVungPhucVuAsync();

            if (!zoom) return;
            var lop = _bc.BanDo.Lop(_vung != null ? LopVung : LopThieu);
            if (lop != null) _bc.BanDo.Map.ZoomToLayer(lop.Handle);
            else _bc.BanDo.ToanCanh();
        }

        private static DataTable BangWkt(string ma, string wkt)
        {
            var t = new DataTable();
            t.Columns.Add("ma");
            t.Columns.Add("wkt");
            t.Rows.Add(ma, wkt);
            return t;
        }
    }
}
