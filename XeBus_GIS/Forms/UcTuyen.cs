using System;
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
    /// Quy trình 2 – Quản lý tuyến xe buýt:
    /// 2.1 Xem thông tin tuyến · 2.2 Tra cứu thuộc tính · 2.3 Tra cứu không gian · 2.4 Cập nhật thuộc tính · 2.5 Gán trạm vào tuyến.
    /// </summary>
    public partial class UcTuyen : UserControl, IModule
    {
        internal const string LopTramTuyen = "TUYEN_TRAM";
        private const string LopTraCuu = "TUYEN_TRACUU";

        private enum DangCho { KhongCo, VeVung, GanDiem }

        private readonly BoiCanh _bc;
        private bool _daNap;
        private DangCho _dangCho = DangCho.KhongCo;
        private string _tuyenDangXem;
        private FrmGanTram _formGan;

        /// <summary>Dùng cho VS Designer.</summary>
        public UcTuyen()
        {
            InitializeComponent();
            UI.UiHelper.CoGianDpi(this);   // co giãn theo DPI ngay – trước khi code dựng thêm control bằng UiHelper.Dp()
        }

        public UcTuyen(BoiCanh boiCanh) : this()
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
            foreach (var l in new[] { lblHdrTraCuu, lblHdrKhongGian }) UiHelper.TieuDeKhuVuc(l);
            foreach (var l in new[] { lblTuKhoa, lblTrangThai, lblQuaTram })
                UiHelper.NhanTruong(l);
            UiHelper.O(txtTuKhoa, cboTrangThai, txtQuaTram, nudBanKinh);
            foreach (var l in new[] { lblDanhSach, lblChiTiet })
            {
                UiHelper.TieuDeKhuVuc(l);
                l.BackColor = Theme.SurfaceAlt;
            }
            lblThongTinTuyen.Font = Theme.FontNhan;
            lblThongTinTuyen.ForeColor = Theme.Muted;
            UiHelper.NutChinh(btnTim);
            UiHelper.NutPhu(btnQuaVung);
            UiHelper.NutPhu(btnGanDiem);
            UiHelper.GanIcon(btnQuaVung, Icons.VeVung);
            UiHelper.GanIcon(btnGanDiem, Icons.BanKinh);
            UiHelper.NutPhu(btnSua);
            UiHelper.NutChinh(btnGanTram);
            UiHelper.DuongKe(pnlTraCuu);
            UiHelper.DuongKe(pnlKhongGian);
            pnlNut.Paint += (s, e) =>
            {
                using (var pen = new System.Drawing.Pen(Theme.Border)) e.Graphics.DrawLine(pen, 0, 0, pnlNut.Width, 0);
            };
            foreach (var p in new Control[] { pnlTraCuu, pnlKhongGian, pnlNut }) UiHelper.CoGianNgang(p);
            UiHelper.KhuVucThuGon(pnlTraCuu, lblHdrTraCuu);
            UiHelper.KhuVucThuGon(pnlKhongGian, lblHdrKhongGian);
            UiHelper.NhanTrangThai(dgvTuyen, "Trạng thái");
            UiHelper.NhanTrangThai(dgvTram, "Trạng thái");
            UiHelper.TrangThaiRong(dgvTuyen, "Không có tuyến phù hợp với điều kiện tra cứu.");
            UiHelper.TrangThaiRong(dgvTram, "Chọn một tuyến để xem danh sách trạm theo thứ tự.");
            dgvTuyen.CellPainting += VeMauTuyen;
            UiHelper.TipCho(btnQuaVung, "Vẽ đa giác trên bản đồ – lấy các tuyến đi qua vùng");
            UiHelper.TipCho(btnGanDiem, "Click một điểm – lấy các tuyến đi gần điểm trong bán kính (m) bên cạnh");
            UiHelper.TipCho(nudBanKinh, "Bán kính tìm tuyến gần điểm (m)");
            UiHelper.Luoi(dgvTuyen);
            UiHelper.Luoi(dgvTram);
            split.BackColor = Theme.Border;
            split.Panel1.BackColor = Theme.Surface;
            split.Panel2.BackColor = Theme.Surface;
            cboTrangThai.Items.Add("(Tất cả)");
            cboTrangThai.Items.AddRange(TuyenRepository.TrangThaiTuyen);
            cboTrangThai.SelectedIndex = 0;
            HienChiTiet(null, null);
        }

        private void PhanQuyen()
        {
            pnlTraCuu.Enabled = Session.Co(Quyen.TuyenTraCuuThuocTinh);
            pnlKhongGian.Enabled = Session.Co(Quyen.TuyenTraCuuKhongGian);
            btnSua.Visible = Session.Co(Quyen.TuyenCapNhat);
            btnGanTram.Visible = Session.Co(Quyen.TuyenGanTram);
            if (!btnSua.Visible && !btnGanTram.Visible) pnlNut.Visible = false;
        }

        private void GanSuKien()
        {
            btnTim.Click += async (s, e) => await TimAsync();
            foreach (var t in new[] { txtTuKhoa, txtQuaTram })
                t.KeyDown += async (s, e) =>
                {
                    if (e.KeyCode != Keys.Enter) return;
                    e.SuppressKeyPress = true;
                    await TimAsync();
                };
            btnQuaVung.Click += (s, e) => BatDauCho(DangCho.VeVung, CongCu.VeVung,
                "Vẽ vùng: click trái thêm đỉnh, double-click / click phải để kết thúc – tìm tuyến đi qua vùng.");
            btnGanDiem.Click += (s, e) => BatDauCho(DangCho.GanDiem, CongCu.ChonDiem,
                $"Click một điểm – tìm tuyến đi trong phạm vi {nudBanKinh.Value:N0} m.");
            btnSua.Click += async (s, e) => await SuaAsync();
            btnGanTram.Click += async (s, e) => await MoGanTramAsync();

            dgvTuyen.SelectionChanged += async (s, e) =>
            {
                var ma = MaDangChon(dgvTuyen);
                if (ma != null && ma != _tuyenDangXem && dgvTuyen.Focused) await XemTuyenAsync(ma);
            };
            dgvTram.SelectionChanged += (s, e) =>
            {
                var ma = MaDangChon(dgvTram);
                if (ma == null || !dgvTram.Focused) return;
                _bc.BanDo.ZoomToi(MapManager.Tram, ma);
                _bc.HienThongTin("TRAM", ma);
            };

            _bc.CongCu.VeVungXong += async wkt => await KhiVeVungXongAsync(wkt);
            _bc.CongCu.ChonDiemXong += async (x, y) => await KhiChonDiemXongAsync(x, y);
            _bc.DoiTuongDuocChon += async (loai, ma) =>
            {
                if (!Visible) return;
                if (loai == "TUYEN") await XemTuyenAsync(ma, chonTrongLuoi: true);
                else if (loai == "TRAM" && _formGan != null && !_formGan.IsDisposed) await _formGan.ThemTramAsync(ma);
            };
        }

        // =====================================================================
        // IModule
        // =====================================================================
        public async void KichHoat()
        {
            if (!_daNap)
            {
                _daNap = true;
                await TimAsync();
            }
            else if (_tuyenDangXem != null)
            {
                await XemTuyenAsync(_tuyenDangXem, zoom: false);
            }
        }

        public void NgungKichHoat()
        {
            if (_dangCho != DangCho.KhongCo)
            {
                _dangCho = DangCho.KhongCo;
                _bc.CongCu.Dat(MapTools.MacDinh);
            }
            if (_formGan != null && !_formGan.IsDisposed) _formGan.Close();
            _bc.BanDo.XoaLopTam(LopTramTuyen);
            _bc.BanDo.XoaLopTam(LopTraCuu);
        }

        /// <summary>Ô màu nhỏ trước mã tuyến – cùng màu với đường tuyến trên bản đồ.</summary>
        private void VeMauTuyen(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || dgvTuyen.Columns[e.ColumnIndex].Name != "Mã") return;
            var mau = _bc?.BanDo.MauTuyen(Convert.ToString(e.Value));
            e.PaintBackground(e.CellBounds, (e.State & DataGridViewElementStates.Selected) != 0);
            int o = UiHelper.Dp(10);
            var r = new System.Drawing.Rectangle(e.CellBounds.X + UiHelper.Dp(8), e.CellBounds.Y + (e.CellBounds.Height - o) / 2, o, o);
            using (var br = new System.Drawing.SolidBrush(mau ?? Theme.Disabled))
                e.Graphics.FillRectangle(br, r);
            var rChu = new System.Drawing.Rectangle(r.Right + UiHelper.Dp(6), e.CellBounds.Y, e.CellBounds.Right - r.Right - UiHelper.Dp(8), e.CellBounds.Height);
            TextRenderer.DrawText(e.Graphics, Convert.ToString(e.Value), e.CellStyle.Font, rChu, e.CellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
            e.Handled = true;
        }

        private static string MaDangChon(DataGridView g)
        {
            if (g.CurrentRow == null || !g.Columns.Contains("Mã")) return null;
            return Convert.ToString(g.CurrentRow.Cells["Mã"].Value);
        }

        // =====================================================================
        // 2.2 / 2.3 Tra cứu
        // =====================================================================
        private async Task TimAsync()
        {
            string tk = txtTuKhoa.Text, qua = txtQuaTram.Text;
            string tt = cboTrangThai.SelectedIndex > 0 ? (string)cboTrangThai.SelectedItem : null;
            DataTable kq = null;
            if (await UiHelper.ChayAsync(btnTim, async () =>
                    kq = await Task.Run(() => TuyenRepository.TraCuuThuocTinh(tk, tt, qua, null)), "Đang tìm…"))
            {
                _bc.BanDo.XoaLopTam(LopTraCuu);
                bool coDieuKien = !string.IsNullOrWhiteSpace(tk) || !string.IsNullOrWhiteSpace(qua) || tt != null;
                HienDanhSach(kq, coDieuKien ? "theo điều kiện" : null);
            }
        }

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
            DataTable kq = null;
            if (!await UiHelper.ChayAsync(btnQuaVung, async () => kq = await Task.Run(() => TuyenRepository.TraCuuQuaVung(wkt)), "Đang tra cứu…"))
                return;
            HienVungTraCuu(wkt);
            HienDanhSach(kq, "đi qua vùng đã vẽ");
        }

        private async Task KhiChonDiemXongAsync(double x, double y)
        {
            if (_dangCho != DangCho.GanDiem) return;
            _dangCho = DangCho.KhongCo;
            double r = (double)nudBanKinh.Value;
            DataTable kq = null;
            string wkt = null;
            if (!await UiHelper.ChayAsync(btnGanDiem, async () => kq = await Task.Run(() =>
                {
                    wkt = TramRepository.WktVongTron(x, y, r);
                    return TuyenRepository.TraCuuGanDiem(x, y, r);
                }), "Đang tra cứu…"))
                return;
            HienVungTraCuu(wkt);
            HienDanhSach(kq, $"trong {r:N0} m quanh điểm");
        }

        private void HienVungTraCuu(string wkt)
        {
            var vung = new DataTable();
            vung.Columns.Add("ma");
            vung.Columns.Add("wkt");
            vung.Rows.Add("VUNG", wkt);
            _bc.BanDo.HienLopTam(LopTraCuu, "Vùng tra cứu tuyến", ShpfileType.SHP_POLYGON, vung, MapManager.KieuVung(Theme.Accent, 45));
        }

        private void HienDanhSach(DataTable kq, string moTa)
        {
            dgvTuyen.DataSource = kq;
            if (dgvTuyen.Columns.Contains("Mã")) dgvTuyen.Columns["Mã"].FillWeight = 26;
            if (dgvTuyen.Columns.Contains("Tên tuyến")) dgvTuyen.Columns["Tên tuyến"].FillWeight = 80;
            if (dgvTuyen.Columns.Contains("Trạng thái")) dgvTuyen.Columns["Trạng thái"].Visible = false;
            if (dgvTuyen.Columns.Contains("Số trạm")) { dgvTuyen.Columns["Số trạm"].HeaderText = "Trạm"; dgvTuyen.Columns["Số trạm"].FillWeight = 20; }
            if (dgvTuyen.Columns.Contains("Dài (km)")) { dgvTuyen.Columns["Dài (km)"].HeaderText = "km"; dgvTuyen.Columns["Dài (km)"].FillWeight = 18; }
            if (dgvTuyen.Columns.Contains("Cách (m)")) { dgvTuyen.Columns["Cách (m)"].HeaderText = "m"; dgvTuyen.Columns["Cách (m)"].FillWeight = 18; }
            UiHelper.CanPhaiCotSo(dgvTuyen);
            dgvTuyen.ClearSelection();

            var ma = kq.Rows.Cast<DataRow>().Select(r => Convert.ToString(r["Mã"])).ToList();
            lblDanhSach.Text = moTa == null ? $"Danh sách tuyến ({ma.Count})" : $"Kết quả: {ma.Count} tuyến {moTa}";
            lblDanhSach.ForeColor = ma.Count == 0 ? Theme.Danger : Theme.Text;
            if (moTa != null)
            {
                _bc.BanDo.Chon(MapManager.Tuyen, ma, zoom: ma.Count > 0);
                _bc.TrangThai(ma.Count == 0 ? "Không có tuyến phù hợp." : $"Tìm thấy {ma.Count} tuyến – đã tô sáng trên bản đồ.", false);
            }
        }

        // =====================================================================
        // 2.1 Xem thông tin tuyến
        // =====================================================================
        /// <summary>Xem một tuyến từ nơi khác (chip tuyến trên thẻ thông tin trạm).</summary>
        public Task XemTuyenTuNgoaiAsync(string maTuyen) => XemTuyenAsync(maTuyen, zoom: true, chonTrongLuoi: true);

        private async Task XemTuyenAsync(string maTuyen, bool zoom = true, bool chonTrongLuoi = false)
        {
            DataRow tuyen = null;
            DataTable tram = null;
            try
            {
                await Task.Run(() =>
                {
                    tuyen = TuyenRepository.Lay(maTuyen);
                    tram = TuyenRepository.TramTrenTuyen(maTuyen);
                });
            }
            catch (Exception ex)
            {
                _bc.TrangThai("Lỗi đọc tuyến: " + ex.GetBaseException().Message, true);
                return;
            }
            if (tuyen == null) return;
            _tuyenDangXem = maTuyen;
            HienChiTiet(tuyen, tram);

            if (chonTrongLuoi)
                foreach (DataGridViewRow row in dgvTuyen.Rows)
                    if (Convert.ToString(row.Cells["Mã"].Value) == maTuyen) { dgvTuyen.CurrentCell = row.Cells["Mã"]; break; }

            // bản đồ: tô sáng tuyến + đánh số thứ tự trạm
            if (!_bc.BanDo.LopDangHien(MapManager.Tuyen)) _bc.BanDo.HienLop(MapManager.Tuyen, true);
            _bc.BanDo.Chon(MapManager.Tuyen, new[] { maTuyen }, zoom);
            VeTramTheoThuTu(tram);
            _bc.HienThongTin("TUYEN", maTuyen);
        }

        /// <summary>Vẽ lớp tạm các trạm của tuyến với nhãn số thứ tự (dùng cả khi xem trước lúc gán trạm).</summary>
        internal void VeTramTheoThuTu(DataTable tram)
        {
            var diem = new DataTable();
            diem.Columns.Add("ma");
            diem.Columns.Add("thutu");
            diem.Columns.Add("wkt");
            int i = 0;
            foreach (DataRow r in tram.Rows)
                diem.Rows.Add(Convert.ToString(r["Mã"]), (++i).ToString(), Convert.ToString(r["wkt"]));
            _bc.BanDo.HienLopTam(LopTramTuyen, "Trạm trên tuyến", ShpfileType.SHP_POINT, diem,
                                 MapManager.KieuDiemTam(Theme.Highlight, 11));
            _bc.BanDo.DatNhan(LopTramTuyen, "[thutu]", Theme.Primary);
        }

        private void HienChiTiet(DataRow tuyen, DataTable tram)
        {
            if (tuyen == null)
            {
                lblChiTiet.Text = "Chi tiết tuyến";
                lblThongTinTuyen.Text = "Chọn một tuyến trong danh sách hoặc click tuyến trên bản đồ.";
                dgvTram.DataSource = null;
                btnSua.Enabled = btnGanTram.Enabled = false;
                return;
            }
            lblChiTiet.Text = $"Tuyến {Db.Str(tuyen, "matuyen")} · {Db.Str(tuyen, "tentuyen")}";
            lblThongTinTuyen.Text =
                $"{Db.Str(tuyen, "trangthai")} · {tram.Rows.Count} trạm · {Db.Str(tuyen, "chieudai") ?? "—"} km" +
                $"{Environment.NewLine}{Db.Str(tuyen, "mota") ?? "(chưa có mô tả)"}";
            dgvTram.DataSource = tram;
            if (dgvTram.Columns.Contains("wkt")) dgvTram.Columns["wkt"].Visible = false;
            UiHelper.CotVuaDu(dgvTram, "#", "Mã");
            if (dgvTram.Columns.Contains("Tên trạm")) dgvTram.Columns["Tên trạm"].FillWeight = 70;
            if (dgvTram.Columns.Contains("Phường/xã")) dgvTram.Columns["Phường/xã"].Visible = false;
            UiHelper.CanPhaiCotSo(dgvTram);
            dgvTram.ClearSelection();
            btnSua.Enabled = btnGanTram.Enabled = true;
        }

        // =====================================================================
        // 2.4 Cập nhật thuộc tính tuyến
        // =====================================================================
        private async Task SuaAsync()
        {
            if (_tuyenDangXem == null || !Session.Co(Quyen.TuyenCapNhat)) return;
            DataRow tuyen = null;
            if (!await UiHelper.ChayAsync(btnSua, async () => tuyen = await Task.Run(() => TuyenRepository.Lay(_tuyenDangXem)), "Đang mở…"))
                return;
            using (var f = new FrmCapNhatTuyen(tuyen))
            {
                if (f.ShowDialog(FindForm()) != DialogResult.OK) return;
            }
            _bc.TrangThai("Đã cập nhật tuyến " + _tuyenDangXem + ".", false);
            await _bc.BanDo.NapLaiAsync(MapManager.Tuyen);
            await LamMoiSauThayDoiAsync();
        }

        private async Task LamMoiSauThayDoiAsync()
        {
            var ma = _tuyenDangXem;
            await TimAsync();
            if (ma != null) await XemTuyenAsync(ma, zoom: false, chonTrongLuoi: true);
        }

        // =====================================================================
        // 2.5 Gán trạm vào tuyến (cửa sổ không chặn – vẫn click được trạm trên bản đồ)
        // =====================================================================
        private async Task MoGanTramAsync()
        {
            if (_tuyenDangXem == null || !Session.Co(Quyen.TuyenGanTram)) return;
            if (_formGan != null && !_formGan.IsDisposed)
            {
                _formGan.Activate();
                return;
            }
            DataRow tuyen = null;
            DataTable tram = null;
            if (!await UiHelper.ChayAsync(btnGanTram, () => Task.Run(() =>
                {
                    tuyen = TuyenRepository.Lay(_tuyenDangXem);
                    tram = TuyenRepository.TramTrenTuyen(_tuyenDangXem);
                }), "Đang mở…"))
                return;

            _formGan = new FrmGanTram(tuyen, tram, VeTramTheoThuTu);
            _formGan.DaLuu += async () =>
            {
                await _bc.BanDo.NapLaiAsync(MapManager.Tuyen);
                await LamMoiSauThayDoiAsync();
            };
            _formGan.FormClosed += async (s, e) =>
            {
                _formGan = null;
                if (_bc.CongCu.HienTai == CongCu.ChonDoiTuong) _bc.CongCu.Dat(MapTools.MacDinh);
                if (_tuyenDangXem != null && Visible) await XemTuyenAsync(_tuyenDangXem, zoom: false);
            };
            var main = FindForm();
            _formGan.StartPosition = FormStartPosition.Manual;
            _formGan.Location = new System.Drawing.Point(main.Right - _formGan.Width - 320, main.Top + 130);
            _formGan.Show(main);
            _bc.CongCu.Dat(CongCu.ChonDoiTuong);
            _bc.TrangThai("Gán trạm: click trạm trên bản đồ để thêm vào cuối tuyến, hoặc tìm trong cửa sổ Gán trạm.", false);
        }
    }
}
