using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapWinGIS;
using XeBus_GIS.Data;
using XeBus_GIS.Map;
using XeBus_GIS.Security;
using XeBus_GIS.UI;

namespace XeBus_GIS.Forms
{
    /// <summary>
    /// Quy trình 1 – Quản lý trạm xe buýt:
    /// 1.1 Xem thông tin trạm · 1.2 Tra cứu theo thuộc tính · 1.3 Tra cứu theo không gian · 1.4 Cập nhật thuộc tính.
    /// </summary>
    public partial class UcTram : UserControl, IModule
    {
        private const string LopTraCuu = "TRAM_TRACUU";

        private enum DangCho { KhongCo, VeVung, BanKinh, Phuong, ViTriSua }

        private readonly BoiCanh _bc;
        private bool _daNapDanhMuc;
        private Panel _bangDuoi;     // bảng kết quả đặt ở khung dưới bản đồ (FrmMain)
        private DangCho _dangCho = DangCho.KhongCo;
        private FrmCapNhatTram _formSuaDangMo;   // giữ giá trị đang sửa khi người dùng chọn lại vị trí trên bản đồ

        /// <summary>Dùng cho VS Designer.</summary>
        public UcTram()
        {
            InitializeComponent();
        }

        public UcTram(BoiCanh boiCanh) : this()
        {
            _bc = boiCanh;
            ApDungGiaoDien();
            PhanQuyen();
            GanSuKien();
        }

        // =====================================================================
        // Khởi tạo
        // =====================================================================
        private void ApDungGiaoDien()
        {
            BackColor = Theme.Surface;
            lblTieuDe.Font = Theme.FontTieuDe;
            lblTieuDe.ForeColor = Theme.Primary;
            foreach (var l in new[] { lblHdrThuocTinh, lblHdrKhongGian }) UiHelper.TieuDeKhuVuc(l);
            foreach (var l in new[] { lblTen, lblLoai, lblTrangThai, lblTuyen, lblPhuong, lblBanKinh })
                UiHelper.NhanTruong(l);
            UiHelper.O(txtTen, cboLoai, cboTrangThai, cboTuyen, cboPhuong, nudBanKinh);
            UiHelper.NutChinh(btnTim);
            UiHelper.NutPhu(btnDatLai);
            UiHelper.NutPhu(btnVeVung);
            UiHelper.NutPhu(btnTheoPhuong);
            UiHelper.NutPhu(btnBanKinh);
            UiHelper.GanIcon(btnVeVung, Icons.VeVung);
            UiHelper.GanIcon(btnTheoPhuong, Icons.BanDo);
            UiHelper.GanIcon(btnBanKinh, Icons.BanKinh);
            UiHelper.NutChinh(btnSua);
            UiHelper.NutPhu(btnHuyKetQua);
            UiHelper.TipCho(btnVeVung, "Vẽ đa giác trên bản đồ: click trái thêm đỉnh, double-click hoặc click phải để kết thúc");
            UiHelper.TipCho(btnTheoPhuong, "Click vào một phường/xã trên bản đồ để lấy các trạm trong phường đó");
            UiHelper.TipCho(btnBanKinh, "Click một điểm trên bản đồ để lấy các trạm trong bán kính đã chọn");
            UiHelper.TipCho(btnSua, "Sửa thuộc tính trạm đang chọn (hoặc double-click dòng)");
            UiHelper.DuongKe(pnlThuocTinh);
            UiHelper.DuongKe(pnlKhongGian);
            UiHelper.ChuPhu(lblKetQua);
            UiHelper.Luoi(dgvKetQua);
            UiHelper.NhanTrangThai(dgvKetQua, "Trạng thái");
            UiHelper.TrangThaiRong(dgvKetQua, "Chưa có kết quả.\nNhập điều kiện rồi bấm Tìm kiếm, hoặc tra cứu theo vùng / phường / bán kính trên bản đồ.");
            foreach (var p in new Control[] { pnlThuocTinh, pnlKhongGian }) UiHelper.CoGianNgang(p);
            UiHelper.KhuVucThuGon(pnlThuocTinh, lblHdrThuocTinh);
            UiHelper.KhuVucThuGon(pnlKhongGian, lblHdrKhongGian);
            TaoBangDuoi();
        }

        /// <summary>
        /// Bảng kết quả nằm ngang trong khung dưới bản đồ (đủ rộng để đọc tên trạm, tuyến, phường/xã)
        /// thay vì cột hẹp bên trái: hàng tiêu đề (số kết quả · Sửa · Xoá kết quả) + lưới.
        /// Cột trái chỉ còn các điều kiện tra cứu.
        /// </summary>
        private void TaoBangDuoi()
        {
            _bangDuoi = new Panel { BackColor = Theme.Surface };
            var hang = new Panel { Dock = DockStyle.Top, Height = UiHelper.Dp(46), BackColor = Theme.Surface, Padding = new Padding(UiHelper.Dp(12), UiHelper.Dp(8), UiHelper.Dp(12), UiHelper.Dp(8)) };
            Controls.Remove(dgvKetQua);
            Controls.Remove(pnlNut);
            Controls.Remove(pnlKetQua);

            btnHuyKetQua.Dock = DockStyle.Right;
            btnSua.Dock = DockStyle.Right;
            btnHuyKetQua.Width = UiHelper.Dp(120);
            btnSua.Width = UiHelper.Dp(170);
            var cach = new Panel { Dock = DockStyle.Right, Width = UiHelper.Dp(8), BackColor = Theme.Surface };
            lblKetQua.Dock = DockStyle.Fill;
            lblKetQua.Font = Theme.FontDam;
            lblKetQua.AutoEllipsis = true;
            lblKetQua.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            hang.Controls.Add(lblKetQua);
            hang.Controls.Add(btnSua);
            hang.Controls.Add(cach);
            hang.Controls.Add(btnHuyKetQua);
            UiHelper.DuongKe(hang);

            dgvKetQua.Dock = DockStyle.Fill;
            _bangDuoi.Controls.Add(dgvKetQua);
            _bangDuoi.Controls.Add(hang);

            // cột trái: gợi ý kết quả nằm ở bảng dưới bản đồ
            lblKetQua.Text = "Kết quả";
            var goiY = new System.Windows.Forms.Label
            {
                Dock = DockStyle.Top,
                Height = UiHelper.Dp(44),
                Padding = new Padding(UiHelper.Dp(12), 0, UiHelper.Dp(12), 0),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Text = "Kết quả tra cứu hiển thị ở bảng phía dưới bản đồ.",
                UseMnemonic = false,
            };
            UiHelper.ChuPhu(goiY);
            Controls.Add(goiY);
            // dock Top theo thứ tự: tiêu đề · thuộc tính · không gian · gợi ý
            foreach (var p in new Control[] { lblTieuDe, pnlThuocTinh, pnlKhongGian, goiY }) p.BringToFront();
        }


        private void PhanQuyen()
        {
            pnlThuocTinh.Enabled = Session.Co(Quyen.TramTraCuuThuocTinh);
            pnlKhongGian.Enabled = Session.Co(Quyen.TramTraCuuKhongGian);
            btnSua.Visible = Session.Co(Quyen.TramCapNhat);
        }

        private void GanSuKien()
        {
            btnTim.Click += async (s, e) => await TimTheoThuocTinhAsync();
            txtTen.KeyDown += async (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                await TimTheoThuocTinhAsync();
            };
            btnDatLai.Click += (s, e) => DatLai();
            btnVeVung.Click += (s, e) => BatDauCho(DangCho.VeVung, CongCu.VeVung, "Vẽ vùng tra cứu: click trái thêm đỉnh, double-click / click phải để kết thúc.");
            btnBanKinh.Click += (s, e) => BatDauCho(DangCho.BanKinh, CongCu.ChonDiem, $"Click một điểm trên bản đồ – tìm trạm trong bán kính {nudBanKinh.Value:N0} m.");
            btnTheoPhuong.Click += (s, e) => BatDauCho(DangCho.Phuong, CongCu.ChonDiem, "Click vào phường/xã cần tra cứu.");
            btnHuyKetQua.Click += (s, e) => XoaKetQua();
            btnSua.Click += async (s, e) => await MoFormSuaAsync(MaTramDangChon());

            dgvKetQua.SelectionChanged += (s, e) =>
            {
                var ma = MaTramDangChon();
                if (ma == null || !dgvKetQua.Focused) return;
                _bc.BanDo.ZoomToi(MapManager.Tram, ma);
                _bc.HienThongTin("TRAM", ma);
            };
            dgvKetQua.CellDoubleClick += async (s, e) =>
            {
                if (e.RowIndex >= 0 && btnSua.Visible) await MoFormSuaAsync(MaTramDangChon());
            };

            _bc.CongCu.VeVungXong += async wkt => await KhiVeVungXongAsync(wkt);
            _bc.CongCu.ChonDiemXong += async (x, y) => await KhiChonDiemXongAsync(x, y);
            _bc.DoiTuongDuocChon += KhiChonTrenBanDo;
        }

        // =====================================================================
        // IModule
        // =====================================================================
        public async void KichHoat()
        {
            _bc.HienBangDuoi?.Invoke(_bangDuoi);
            if (_daNapDanhMuc) return;
            try
            {
                var loai = await Task.Run(() => TramRepository.LoaiTram());
                var tuyen = await Task.Run(() => TramRepository.Tuyen());
                var phuong = await Task.Run(() => TramRepository.PhuongXa());
                GanCombo(cboLoai, loai, "(Tất cả loại)");
                GanCombo(cboTuyen, tuyen, "(Tất cả tuyến)");
                GanCombo(cboPhuong, phuong, "(Tất cả phường/xã)");
                cboTrangThai.Items.Clear();
                cboTrangThai.Items.Add("(Tất cả)");
                cboTrangThai.Items.AddRange(TramRepository.TrangThaiTram);
                cboTrangThai.SelectedIndex = 0;
                _daNapDanhMuc = true;
            }
            catch (Exception ex)
            {
                _bc.TrangThai("Lỗi nạp danh mục: " + ex.GetBaseException().Message, true);
            }
        }

        public void NgungKichHoat()
        {
            _bc.AnBangDuoi?.Invoke(_bangDuoi);
            if (_dangCho != DangCho.KhongCo)
            {
                _dangCho = DangCho.KhongCo;
                _bc.CongCu.Dat(CongCu.ChonDoiTuong);
            }
            _bc.BanDo.XoaLopTam(LopTraCuu);
        }

        internal static void GanCombo(ComboBox cbo, DataTable dt, string dongDau)
        {
            var bang = dt.Copy();
            if (dongDau != null)
            {
                var r = bang.NewRow();
                r["ma"] = "";
                r["ten"] = dongDau;
                bang.Rows.InsertAt(r, 0);
            }
            cbo.DisplayMember = "ten";
            cbo.ValueMember = "ma";
            cbo.DataSource = bang;
            cbo.SelectedIndex = 0;
        }

        private static string GiaTri(ComboBox cbo)
        {
            var v = cbo.SelectedValue as string;
            return string.IsNullOrEmpty(v) ? null : v;
        }

        // =====================================================================
        // 1.2 Tra cứu theo thuộc tính
        // =====================================================================
        private async Task TimTheoThuocTinhAsync()
        {
            string ten = txtTen.Text, loai = GiaTri(cboLoai), tuyen = GiaTri(cboTuyen), phuong = GiaTri(cboPhuong);
            string tt = cboTrangThai.SelectedIndex > 0 ? (string)cboTrangThai.SelectedItem : null;
            DataTable kq = null;
            if (await UiHelper.ChayAsync(btnTim, async () =>
                    kq = await Task.Run(() => TramRepository.TraCuuThuocTinh(ten, loai, tt, tuyen, phuong)), "Đang tìm…"))
            {
                _bc.BanDo.XoaLopTam(LopTraCuu);
                HienKetQua(kq, "theo thuộc tính");
            }
        }

        private void DatLai()
        {
            txtTen.Clear();
            foreach (var c in new[] { cboLoai, cboTrangThai, cboTuyen, cboPhuong })
                if (c.Items.Count > 0) c.SelectedIndex = 0;
            XoaKetQua();
        }

        // =====================================================================
        // 1.3 Tra cứu theo không gian
        // =====================================================================
        private void BatDauCho(DangCho cho, CongCu congCu, string goiY)
        {
            _dangCho = cho;
            _bc.CongCu.Dat(congCu);
            _bc.TrangThai(goiY, false);
        }

        private async Task KhiVeVungXongAsync(string wkt)
        {
            if (_dangCho != DangCho.VeVung) return;
            _dangCho = DangCho.KhongCo;
            await ChayTraCuuKhongGian(btnVeVung, wkt,
                () => TramRepository.TraCuuTrongVung(wkt), "trong vùng đã vẽ");
        }

        private async Task KhiChonDiemXongAsync(double x, double y)
        {
            var cho = _dangCho;
            _dangCho = DangCho.KhongCo;
            switch (cho)
            {
                case DangCho.BanKinh:
                {
                    double r = (double)nudBanKinh.Value;
                    string wkt = null;
                    await ChayTraCuuKhongGian(btnBanKinh, null, () =>
                    {
                        wkt = TramRepository.WktVongTron(x, y, r);
                        return TramRepository.TraCuuBanKinh(x, y, r);
                    }, $"trong bán kính {r:N0} m", () => wkt);
                    break;
                }
                case DangCho.Phuong:
                {
                    DataRow phuong = null;
                    string wkt = null;
                    await ChayTraCuuKhongGian(btnTheoPhuong, null, () =>
                    {
                        phuong = TramRepository.PhuongTaiDiem(x, y);
                        if (phuong == null) return null;
                        wkt = TramRepository.WktPhuong(Db.Str(phuong, "maphuong"));
                        return TramRepository.TraCuuThuocTinh(null, null, null, null, Db.Str(phuong, "maphuong"));
                    }, null, () => wkt, () => phuong == null ? null : "thuộc " + Db.Str(phuong, "tenphuong"));
                    if (phuong == null) _bc.TrangThai("Điểm click nằm ngoài các phường/xã.", true);
                    break;
                }
                case DangCho.ViTriSua:
                {
                    _bc.BanDo.ProjSangDo(x, y, out double lon, out double lat);
                    var f = _formSuaDangMo;
                    _formSuaDangMo = null;
                    if (f != null)
                    {
                        f.DatViTri(lon, lat);
                        await HienFormSuaAsync(f);
                    }
                    break;
                }
            }
        }

        /// <param name="wktVung">vùng tra cứu (EPSG:3857) để vẽ lớp tạm; hoặc lấy sau qua layWkt</param>
        private async Task ChayTraCuuKhongGian(Button nut, string wktVung, Func<DataTable> truyVan, string moTa,
                                               Func<string> layWkt = null, Func<string> layMoTa = null)
        {
            DataTable kq = null;
            if (!await UiHelper.ChayAsync(nut, async () => kq = await Task.Run(truyVan), "Đang tra cứu…")) return;
            var wkt = wktVung ?? layWkt?.Invoke();
            if (!string.IsNullOrEmpty(wkt))
            {
                var vung = new DataTable();
                vung.Columns.Add("ma");
                vung.Columns.Add("wkt");
                vung.Rows.Add("VUNG", wkt);
                _bc.BanDo.HienLopTam(LopTraCuu, "Vùng tra cứu trạm", ShpfileType.SHP_POLYGON, vung,
                                     MapManager.KieuVung(Theme.Accent, 45));
            }
            if (kq != null) HienKetQua(kq, layMoTa?.Invoke() ?? moTa);
        }

        // =====================================================================
        // Kết quả
        // =====================================================================
        private void HienKetQua(DataTable kq, string moTa)
        {
            dgvKetQua.DataSource = kq;
            UiHelper.CotVuaDu(dgvKetQua, "Mã");
            UiHelper.CotCoDinh(dgvKetQua, "Trạng thái", 104);
            // bảng nằm ngang dưới bản đồ – đủ rộng để hiện đủ các cột
            if (dgvKetQua.Columns.Contains("Tên trạm")) dgvKetQua.Columns["Tên trạm"].FillWeight = 100;
            if (dgvKetQua.Columns.Contains("Loại")) dgvKetQua.Columns["Loại"].FillWeight = 34;
            if (dgvKetQua.Columns.Contains("Phường/xã")) dgvKetQua.Columns["Phường/xã"].FillWeight = 46;
            if (dgvKetQua.Columns.Contains("Tuyến")) dgvKetQua.Columns["Tuyến"].FillWeight = 40;
            if (dgvKetQua.Columns.Contains("Khoảng cách (m)"))
            {
                dgvKetQua.Columns["Khoảng cách (m)"].FillWeight = 26;
            }
            UiHelper.CanPhaiCotSo(dgvKetQua);
            dgvKetQua.ClearSelection();

            var ma = kq.Rows.Cast<DataRow>().Select(r => Convert.ToString(r["Mã"])).ToList();
            lblKetQua.Text = $"Kết quả: {ma.Count:N0} trạm {moTa}";
            lblKetQua.ForeColor = ma.Count == 0 ? Theme.Danger : Theme.Text;
            if (!_bc.BanDo.LopDangHien(MapManager.Tram)) _bc.BanDo.HienLop(MapManager.Tram, true);
            _bc.BanDo.Chon(MapManager.Tram, ma, zoom: ma.Count > 0);
            _bc.TrangThai(ma.Count == 0 ? "Không tìm thấy trạm phù hợp." : $"Tìm thấy {ma.Count:N0} trạm – đã tô sáng trên bản đồ.", false);
        }

        private void XoaKetQua()
        {
            dgvKetQua.DataSource = null;
            lblKetQua.Text = "Kết quả";
            lblKetQua.ForeColor = Theme.Text;
            _bc.BanDo.XoaLopTam(LopTraCuu);
            _bc.BanDo.BoChonTatCa();
        }

        private string MaTramDangChon()
        {
            if (dgvKetQua.CurrentRow == null || !dgvKetQua.Columns.Contains("Mã")) return null;
            return Convert.ToString(dgvKetQua.CurrentRow.Cells["Mã"].Value);
        }

        /// <summary>1.1 Xem thông tin: click trạm trên bản đồ thì chọn dòng tương ứng trong kết quả (nếu có).</summary>
        private void KhiChonTrenBanDo(string loai, string ma)
        {
            if (!Visible || loai != "TRAM" || dgvKetQua.DataSource == null) return;
            foreach (DataGridViewRow row in dgvKetQua.Rows)
            {
                if (Convert.ToString(row.Cells["Mã"].Value) != ma) continue;
                dgvKetQua.CurrentCell = row.Cells["Mã"];
                return;
            }
        }

        // =====================================================================
        // 1.4 Cập nhật thuộc tính trạm
        // =====================================================================
        /// <summary>Mở hộp thoại sửa trạm từ nơi khác (thẻ thông tin ở panel phải).</summary>
        public Task SuaTramAsync(string maTram) => MoFormSuaAsync(maTram);

        private async Task MoFormSuaAsync(string maTram)
        {
            if (!Session.Co(Quyen.TramCapNhat)) return;
            if (string.IsNullOrEmpty(maTram))
            {
                UiHelper.ThongBao(FindForm(), "Hãy chọn một trạm trong danh sách kết quả (hoặc click trạm trên bản đồ rồi tìm theo mã).");
                return;
            }
            DataRow tram = null;
            DataTable loai = null, tuyen = null, phuongTien = null;
            if (!await UiHelper.ChayAsync(btnSua, () => Task.Run(() =>
                {
                    tram = TramRepository.Lay(maTram);
                    loai = TramRepository.LoaiTram();
                    tuyen = TramRepository.Tuyen();
                    phuongTien = TramRepository.PhuongTien();
                }), "Đang mở…")) return;
            if (tram == null)
            {
                UiHelper.Loi(FindForm(), "Không tìm thấy trạm " + maTram);
                return;
            }
            await HienFormSuaAsync(new FrmCapNhatTram(tram, loai, tuyen, phuongTien));
        }

        private async Task HienFormSuaAsync(FrmCapNhatTram f)
        {
            var kq = f.ShowDialog(FindForm());
            if (kq == DialogResult.Retry)
            {
                // người dùng muốn chọn lại vị trí trên bản đồ
                _formSuaDangMo = f;
                BatDauCho(DangCho.ViTriSua, CongCu.ChonDiem, "Click vị trí mới của trạm " + f.MaTram + " trên bản đồ.");
                return;
            }
            if (kq != DialogResult.OK)
            {
                f.Dispose();
                return;
            }

            var ma = f.MaTram;
            f.Dispose();
            _bc.TrangThai("Đã cập nhật trạm " + ma + ".", false);
            UiHelper.ThanhCong(FindForm(), "Đã lưu thay đổi của trạm " + ma + ".");
            await _bc.BanDo.NapLaiAsync(MapManager.Tram);
            _bc.BanDo.Chon(MapManager.Tram, new[] { ma }, zoom: true);
            _bc.HienThongTin("TRAM", ma);
            // cập nhật dòng trong lưới kết quả
            if (dgvKetQua.DataSource is DataTable dt)
            {
                var r = dt.Rows.Cast<DataRow>().FirstOrDefault(x => Convert.ToString(x["Mã"]) == ma);
                var moi = await Task.Run(() => TramRepository.TraCuuThuocTinh(ma, null, null, null, null));
                var rm = moi.Rows.Cast<DataRow>().FirstOrDefault(x => Convert.ToString(x["Mã"]) == ma);
                if (r != null && rm != null)
                    foreach (DataColumn c in moi.Columns)
                        if (dt.Columns.Contains(c.ColumnName)) r[c.ColumnName] = rm[c];
            }
        }
    }
}
