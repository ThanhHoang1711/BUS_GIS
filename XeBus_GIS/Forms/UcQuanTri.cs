using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using XeBus_GIS.Data;
using XeBus_GIS.Security;
using XeBus_GIS.UI;

namespace XeBus_GIS.Forms
{
    /// <summary>
    /// Quản trị hệ thống (nhóm Quản trị viên – mục 3.1.1):
    /// 01–03 Thêm/sửa/xoá người dùng · 04 Phân quyền · 05 Lịch sử truy cập · 06 Sao lưu dữ liệu.
    /// </summary>
    public partial class UcQuanTri : UserControl, IModule
    {
        private int? _idDangSua;       // null = đang thêm mới
        private bool _dangNapQuyen;

        private bool _daNap;

        /// <summary>Dùng cho VS Designer.</summary>
        public UcQuanTri()
        {
            InitializeComponent();
        }

        public UcQuanTri(BoiCanh boiCanh) : this()
        {
            ApDungGiaoDien();
            PhanQuyen();
            TaoThanhTab();
            GanSuKien();
        }

        // =====================================================================
        // IModule – nằm trong cửa sổ chính (màn hình toàn trang, không bản đồ)
        // =====================================================================
        public async void KichHoat()
        {
            await NhacChoDuyetAsync();
            if (_daNap) return;
            _daNap = true;
            await NapBanDauAsync();
        }

        public void NgungKichHoat() { }

        /// <summary>Nhắc quản trị viên khi có tài khoản tự đăng ký đang chờ duyệt.</summary>
        private async Task NhacChoDuyetAsync()
        {
            try
            {
                int so = await Task.Run(() => QuanTriRepository.SoChoDuyet());
                if (so > 0)
                    UiHelper.CanhBao(this, $"Có {so} tài khoản tự đăng ký đang chờ duyệt. Chọn tài khoản, sửa trạng thái thành \"Hoạt động\" (và nhóm nếu cần).");
            }
            catch { /* chỉ là nhắc nhở */ }
        }

        /// <summary>Thay dải tab hệ thống bằng các tab gạch chân thống nhất với các màn hình khác.</summary>
        private void TaoThanhTab()
        {
            tab.Appearance = TabAppearance.FlatButtons;
            tab.ItemSize = new System.Drawing.Size(0, 1);
            tab.SizeMode = TabSizeMode.Fixed;
            var pnl = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = UiHelper.Dp(44),
                BackColor = Theme.Surface,
                WrapContents = false,
                Padding = new Padding(UiHelper.Dp(8), 0, 0, 0),
            };
            pnl.Paint += (s, e) =>
            {
                using (var pen = new System.Drawing.Pen(Theme.Border)) e.Graphics.DrawLine(pen, 0, pnl.Height - 1, pnl.Width, pnl.Height - 1);
            };
            var nut = new System.Collections.Generic.List<Button>();
            foreach (TabPage tp in tab.TabPages)
            {
                var b = new Button { Text = tp.Text, Height = UiHelper.Dp(44), Margin = new Padding(0), Tag = tp, AutoSize = false, UseMnemonic = false };
                b.Width = TextRenderer.MeasureText(tp.Text, Theme.FontDam).Width + UiHelper.Dp(32);
                UiHelper.Tab(b);
                b.Click += (s, e) =>
                {
                    tab.SelectedTab = (TabPage)b.Tag;
                    foreach (var x in nut) UiHelper.ChonTab(x, x == b);
                };
                nut.Add(b);
                pnl.Controls.Add(b);
            }
            if (nut.Count > 0) UiHelper.ChonTab(nut[0], true);
            Controls.Add(pnl);
            pnl.BringToFront();
            tab.BringToFront();   // tab (Fill) phải nằm trên cùng z-order để dock sau thanh tab
        }

        // =====================================================================
        // Giao diện + phân quyền
        // =====================================================================
        private void ApDungGiaoDien()
        {
            BackColor = Theme.Surface;
            lblTieuDe.Visible = false;   // tiêu đề trang hiển thị ở đầu cửa sổ chính
            tab.Font = Theme.FontThuong;
            foreach (TabPage tp in tab.TabPages) { tp.BackColor = Theme.Surface; tp.Font = Theme.FontThuong; tp.Padding = new Padding(UiHelper.Dp(8)); }
            UiHelper.NhanTrangThai(dgvND, "Trạng thái");
            UiHelper.NhanTrangThai(dgvLichSu, "Kết quả");
            UiHelper.TrangThaiRong(dgvND, "Không có người dùng phù hợp.");
            UiHelper.TrangThaiRong(dgvLichSu, "Không có lượt đăng nhập nào trong khoảng thời gian đã chọn.");
            UiHelper.TrangThaiRong(dgvSaoLuu, "Chưa có bản sao lưu nào.");
            pnlSua.BackColor = Theme.SurfaceAlt;
            UiHelper.VienMong(pnlSua);

            foreach (var l in new[] { lblTim, lblTenDN, lblHoTen, lblChucVu, lblDonVi, lblNhom, lblTrangThaiND, lblMatKhau, lblXacNhan,
                                      lblTu, lblDen, lblTenDNLs, lblKetQuaLs })
                UiHelper.NhanTruong(l);
            foreach (var l in new[] { lblHdrSua, lblNhomHdr })
            {
                l.Font = Theme.FontTieuDe;
                l.ForeColor = Theme.Primary;
            }
            foreach (var l in new[] { lblGhiChuMK, lblMoTaNhom, lblQuyenGhiChu, lblSaoLuuMoTa, lblTomTatLs })
            {
                l.Font = Theme.FontNhan;
                l.ForeColor = Theme.Muted;
            }
            lblLoiND.ForeColor = Theme.Danger;
            lblLoiND.Font = Theme.FontNhan;
            lblSaoLuuKQ.Font = Theme.FontThuong;

            UiHelper.NutChinh(btnTimND);
            UiHelper.NutPhu(btnMoi);
            UiHelper.NutChinh(btnLuuND);
            UiHelper.NutPhu(btnDatLaiMK);
            UiHelper.NutNguyHiem(btnXoaND);
            UiHelper.NutChinh(btnLuuQuyen);
            UiHelper.NutPhu(btnChonTatCaQ);
            UiHelper.NutChinh(btnLoc);
            UiHelper.NutChinh(btnSaoLuu);
            UiHelper.NutPhu(btnMoThuMuc);
            foreach (var g in new[] { dgvND, dgvQuyen, dgvLichSu, dgvSaoLuu }) UiHelper.Luoi(g);
            lstNhom.Font = Theme.FontThuong;
            lstNhom.BorderStyle = BorderStyle.FixedSingle;
            lstNhom.Cursor = Cursors.Hand;

            cboTrangThaiND.Items.AddRange(QuanTriRepository.TrangThaiNguoiDung);
            cboKetQuaLs.Items.AddRange(new object[] { "(Tất cả)", NguoiDungRepository.KetQuaThanhCong, NguoiDungRepository.KetQuaSaiMatKhau,
                                                      NguoiDungRepository.KetQuaKhongTonTai, NguoiDungRepository.KetQuaBiKhoa });
            cboKetQuaLs.SelectedIndex = 0;
            dtpTu.Value = DateTime.Today.AddDays(-30);
            dtpDen.Value = DateTime.Today;

            lblSaoLuuMoTa.Text =
                "Sao lưu toàn bộ dữ liệu của hệ thống (schema xebus: trạm, tuyến, phường/xã, khu dân cư, công trình, người dùng, phân quyền, lịch sử…) " +
                "bằng pg_dump (định dạng custom, phục hồi bằng pg_restore).\n" +
                $"Thư mục lưu: {QuanTriRepository.ThuMucSaoLuu}   ·   pg_dump: {QuanTriRepository.ThuMucPgBin}";
        }

        private void PhanQuyen()
        {
            if (!Session.CoNhom("QT_NGUOIDUNG")) tab.TabPages.Remove(tpNguoiDung);
            if (!Session.Co(Quyen.QtPhanQuyen)) tab.TabPages.Remove(tpPhanQuyen);
            if (!Session.Co(Quyen.QtLichSu)) tab.TabPages.Remove(tpLichSu);
            if (!Session.Co(Quyen.QtSaoLuu)) tab.TabPages.Remove(tpSaoLuu);
            btnMoi.Enabled = Session.Co(Quyen.QtNguoiDung, Quyen.Them);
            btnDatLaiMK.Enabled = Session.Co(Quyen.QtNguoiDung, Quyen.Sua);
            btnXoaND.Enabled = Session.Co(Quyen.QtNguoiDung, Quyen.Xoa);
        }

        private void GanSuKien()
        {
            // Người dùng
            btnTimND.Click += async (s, e) => await NapNguoiDungAsync();
            txtTim.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await NapNguoiDungAsync(); } };
            dgvND.SelectionChanged += async (s, e) => { if (dgvND.Focused) await ChonNguoiDungAsync(); };
            btnMoi.Click += (s, e) => CheDoThemMoi();
            btnLuuND.Click += async (s, e) => await LuuNguoiDungAsync();
            btnDatLaiMK.Click += async (s, e) => await DatLaiMatKhauAsync();
            btnXoaND.Click += async (s, e) => await XoaNguoiDungAsync();
            foreach (var t in new[] { txtTenDN, txtHoTen, txtMatKhau, txtXacNhan }) t.TextChanged += (s, e) => lblLoiND.Visible = false;

            // Phân quyền
            lstNhom.SelectedIndexChanged += async (s, e) => await NapQuyenNhomAsync();
            btnLuuQuyen.Click += async (s, e) => await LuuQuyenAsync();
            btnChonTatCaQ.Click += (s, e) =>
            {
                if (!(dgvQuyen.DataSource is DataTable dt)) return;
                bool tatCa = dt.Rows.Cast<DataRow>().Any(r => !(bool)r["Cho phép"]);
                foreach (DataRow r in dt.Rows) r["Cho phép"] = tatCa;
            };
            dgvQuyen.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvQuyen.IsCurrentCellDirty) dgvQuyen.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            // Lịch sử
            btnLoc.Click += async (s, e) => await NapLichSuAsync();
            dgvLichSu.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || !dgvLichSu.Columns.Contains("Kết quả")) return;
                var kq = Convert.ToString(dgvLichSu.Rows[e.RowIndex].Cells["Kết quả"].Value);
                if (kq != NguoiDungRepository.KetQuaThanhCong) e.CellStyle.ForeColor = Theme.Danger;
            };

            // Sao lưu
            btnSaoLuu.Click += async (s, e) => await SaoLuuAsync();
            btnMoThuMuc.Click += (s, e) =>
            {
                try
                {
                    System.IO.Directory.CreateDirectory(QuanTriRepository.ThuMucSaoLuu);
                    Process.Start("explorer.exe", "\"" + QuanTriRepository.ThuMucSaoLuu + "\"");
                }
                catch (Exception ex) { UiHelper.Loi(this, ex.Message); }
            };
        }

        private async Task NapBanDauAsync()
        {
            try
            {
                var nhom = await Task.Run(() => QuanTriRepository.Nhom());
                cboNhom.DisplayMember = "ten";
                cboNhom.ValueMember = "ma";
                cboNhom.DataSource = nhom.Copy();
                lstNhom.DisplayMember = "ten";
                lstNhom.ValueMember = "ma";
                _dangNapQuyen = true;
                lstNhom.DataSource = nhom;
                _dangNapQuyen = false;
                CheDoThemMoi();
                if (tab.TabPages.Contains(tpNguoiDung)) await NapNguoiDungAsync();
                if (tab.TabPages.Contains(tpPhanQuyen)) await NapQuyenNhomAsync();
                if (tab.TabPages.Contains(tpLichSu)) await NapLichSuAsync();
                if (tab.TabPages.Contains(tpSaoLuu)) await NapSaoLuuAsync();
            }
            catch (Exception ex)
            {
                UiHelper.Loi(this, ex.GetBaseException().Message);
            }
        }

        // =====================================================================
        // 01–03 Người dùng
        // =====================================================================
        private async Task NapNguoiDungAsync()
        {
            var tk = txtTim.Text;
            DataTable dt = null;
            if (!await UiHelper.ChayAsync(btnTimND, async () => dt = await Task.Run(() => QuanTriRepository.DanhSachNguoiDung(tk)), "…")) return;
            dgvND.DataSource = dt;
            foreach (var c in new[] { "id", "idnhom" }) if (dgvND.Columns.Contains(c)) dgvND.Columns[c].Visible = false;
            dgvND.ClearSelection();
        }

        private async Task ChonNguoiDungAsync()
        {
            if (dgvND.CurrentRow == null) return;
            int id = Convert.ToInt32(dgvND.CurrentRow.Cells["id"].Value);
            DataRow r;
            try { r = await Task.Run(() => QuanTriRepository.LayNguoiDung(id)); }
            catch (Exception ex) { UiHelper.Loi(this, ex.GetBaseException().Message); return; }
            if (r == null) return;
            _idDangSua = id;
            lblHdrSua.Text = "Sửa người dùng #" + id;
            txtTenDN.Text = Db.Str(r, "tendnhap");
            txtHoTen.Text = Db.Str(r, "tenndung");
            txtChucVu.Text = Db.Str(r, "chucvu");
            txtDonVi.Text = Db.Str(r, "donvi");
            cboNhom.SelectedValue = Convert.ToInt32(r["idnhom"]);
            cboTrangThaiND.SelectedItem = Db.Str(r, "trangthai");
            if (cboTrangThaiND.SelectedIndex < 0) cboTrangThaiND.SelectedIndex = 0;
            txtMatKhau.Clear();
            txtXacNhan.Clear();
            lblLoiND.Visible = false;
            btnLuuND.Enabled = Session.Co(Quyen.QtNguoiDung, Quyen.Sua);
            btnDatLaiMK.Enabled = Session.Co(Quyen.QtNguoiDung, Quyen.Sua);
            btnXoaND.Enabled = Session.Co(Quyen.QtNguoiDung, Quyen.Xoa) && id != Session.IdNguoiDung;
        }

        private void CheDoThemMoi()
        {
            _idDangSua = null;
            lblHdrSua.Text = "Thêm người dùng mới";
            foreach (var t in new[] { txtTenDN, txtHoTen, txtChucVu, txtDonVi, txtMatKhau, txtXacNhan }) t.Clear();
            if (cboNhom.Items.Count > 0) cboNhom.SelectedValue = Quyen.NhomQuanLy;
            cboTrangThaiND.SelectedIndex = 0;
            lblLoiND.Visible = false;
            btnLuuND.Enabled = Session.Co(Quyen.QtNguoiDung, Quyen.Them);
            btnDatLaiMK.Enabled = false;
            btnXoaND.Enabled = false;
            dgvND.ClearSelection();
            txtTenDN.Focus();
        }

        private void BaoLoi(string noiDung)
        {
            lblLoiND.Text = noiDung;
            lblLoiND.Visible = true;
        }

        private string KiemTraMatKhau()
        {
            if (txtMatKhau.Text.Length < 6) return "Mật khẩu tối thiểu 6 ký tự.";
            if (txtMatKhau.Text != txtXacNhan.Text) return "Mật khẩu nhập lại không khớp.";
            return null;
        }

        private async Task LuuNguoiDungAsync()
        {
            var tenDN = txtTenDN.Text.Trim();
            var hoTen = txtHoTen.Text.Trim();
            if (!Regex.IsMatch(tenDN, @"^[A-Za-z0-9_.]{3,50}$")) { BaoLoi("Tên đăng nhập 3–50 ký tự, chỉ gồm chữ không dấu, số, dấu _ hoặc dấu chấm."); txtTenDN.Focus(); return; }
            if (hoTen.Length == 0) { BaoLoi("Họ tên không được để trống."); txtHoTen.Focus(); return; }
            if (cboNhom.SelectedValue == null) { BaoLoi("Chọn nhóm người dùng."); return; }
            if (_idDangSua == null)
            {
                var loiMk = KiemTraMatKhau();
                if (loiMk != null) { BaoLoi(loiMk); txtMatKhau.Focus(); return; }
            }

            int? id = _idDangSua;
            int nhom = Convert.ToInt32(cboNhom.SelectedValue);
            string cv = txtChucVu.Text, dv = txtDonVi.Text, tt = (string)cboTrangThaiND.SelectedItem, mk = txtMatKhau.Text;
            bool trung = false;
            int idMoi = 0;
            if (!await UiHelper.ChayAsync(btnLuuND, () => Task.Run(() =>
                {
                    trung = QuanTriRepository.TenDangNhapDaCo(tenDN, id);
                    if (trung) return;
                    if (id == null) idMoi = QuanTriRepository.Them(tenDN, hoTen, cv, dv, nhom, tt, mk);
                    else QuanTriRepository.Sua(id.Value, tenDN, hoTen, cv, dv, nhom, tt);
                }), "Đang lưu…"))
                return;
            if (trung) { BaoLoi($"Tên đăng nhập \"{tenDN}\" đã tồn tại."); txtTenDN.Focus(); return; }

            UiHelper.ThanhCong(this, id == null ? $"Đã thêm người dùng \"{tenDN}\"." : $"Đã cập nhật người dùng \"{tenDN}\".");
            await NapNguoiDungAsync();
            if (id == null) CheDoThemMoi();
            if (id != null && id == Session.IdNguoiDung)
                UiHelper.ThongBao(this, "Bạn vừa sửa tài khoản của chính mình – thay đổi nhóm/quyền có hiệu lực ở lần đăng nhập sau.");
        }

        private async Task DatLaiMatKhauAsync()
        {
            if (_idDangSua == null) return;
            var loi = KiemTraMatKhau();
            if (loi != null) { BaoLoi(loi + " (nhập mật khẩu mới vào 2 ô Mật khẩu)"); txtMatKhau.Focus(); return; }
            if (!UiHelper.XacNhan(this, $"Đặt lại mật khẩu cho \"{txtTenDN.Text}\"?")) return;
            int id = _idDangSua.Value;
            var mk = txtMatKhau.Text;
            if (!await UiHelper.ChayAsync(btnDatLaiMK, () => Task.Run(() => QuanTriRepository.DatLaiMatKhau(id, mk)), "Đang lưu…")) return;
            txtMatKhau.Clear();
            txtXacNhan.Clear();
            UiHelper.ThanhCong(this, "Đã đặt lại mật khẩu.");
        }

        private async Task XoaNguoiDungAsync()
        {
            if (_idDangSua == null) return;
            if (!UiHelper.XacNhanNguyHiem(this, $"Xoá người dùng \"{txtTenDN.Text}\"?\nThao tác không hoàn tác được. Lịch sử đăng nhập của người này vẫn được giữ lại.")) return;
            int id = _idDangSua.Value;
            if (!await UiHelper.ChayAsync(btnXoaND, () => Task.Run(() => QuanTriRepository.Xoa(id)), "Đang xoá…")) return;
            CheDoThemMoi();
            await NapNguoiDungAsync();
        }

        // =====================================================================
        // 04 Phân quyền
        // =====================================================================
        private async Task NapQuyenNhomAsync()
        {
            if (_dangNapQuyen || !(lstNhom.SelectedItem is DataRowView v)) return;
            int idNhom = Convert.ToInt32(v["ma"]);
            lblMoTaNhom.Text = $"{v["mota"]}\n\n{v["so_nguoi"]} người dùng · {v["so_quyen"]} quyền được gán.";
            DataTable dt;
            try { dt = await Task.Run(() => QuanTriRepository.QuyenCuaNhom(idNhom)); }
            catch (Exception ex) { UiHelper.Loi(this, ex.GetBaseException().Message); return; }
            dgvQuyen.DataSource = dt;
            dgvQuyen.ReadOnly = !Session.Co(Quyen.QtPhanQuyen);
            foreach (DataGridViewColumn c in dgvQuyen.Columns) c.ReadOnly = c.Name != "Cho phép";
            dgvQuyen.Columns["idquyen"].Visible = false;
            dgvQuyen.Columns["Cho phép"].FillWeight = 16;
            dgvQuyen.Columns["Nhóm chức năng"].FillWeight = 34;
            dgvQuyen.Columns["Chức năng"].FillWeight = 80;
            dgvQuyen.Columns["Thao tác"].FillWeight = 18;
            dgvQuyen.Columns["Mã"].FillWeight = 34;
            dgvQuyen.Columns["Mã"].DefaultCellStyle.Font = Theme.FontSo;
            dgvQuyen.Columns["Mã"].DefaultCellStyle.ForeColor = Theme.Muted;
            dgvQuyen.SelectionMode = DataGridViewSelectionMode.CellSelect;
            btnLuuQuyen.Enabled = btnChonTatCaQ.Enabled = Session.Co(Quyen.QtPhanQuyen);
        }

        private async Task LuuQuyenAsync()
        {
            if (!(lstNhom.SelectedItem is DataRowView v) || !(dgvQuyen.DataSource is DataTable dt)) return;
            dgvQuyen.EndEdit();
            int idNhom = Convert.ToInt32(v["ma"]);
            var ds = dt.Rows.Cast<DataRow>().Where(r => (bool)r["Cho phép"]).Select(r => Convert.ToInt32(r["idquyen"])).ToList();
            if (!UiHelper.XacNhan(this, $"Lưu {ds.Count} quyền cho nhóm \"{v["ten"]}\"?")) return;
            if (!await UiHelper.ChayAsync(btnLuuQuyen, () => Task.Run(() => QuanTriRepository.LuuQuyenNhom(idNhom, ds)), "Đang lưu…")) return;
            UiHelper.ThanhCong(this, "Đã lưu phân quyền. Người dùng của nhóm cần đăng nhập lại để áp dụng.");
            var chon = lstNhom.SelectedIndex;
            var nhom = await Task.Run(() => QuanTriRepository.Nhom());
            _dangNapQuyen = true;
            lstNhom.DataSource = nhom;
            _dangNapQuyen = false;
            lstNhom.SelectedIndex = chon;
            await NapQuyenNhomAsync();
        }

        // =====================================================================
        // 05 Lịch sử truy cập
        // =====================================================================
        private async Task NapLichSuAsync()
        {
            if (dtpTu.Value.Date > dtpDen.Value.Date) { UiHelper.CanhBao(this, "Từ ngày phải trước Đến ngày."); return; }
            DateTime tu = dtpTu.Value, den = dtpDen.Value;
            string u = txtTenDNLs.Text, kq = cboKetQuaLs.SelectedIndex > 0 ? (string)cboKetQuaLs.SelectedItem : "";
            DataTable dt = null;
            if (!await UiHelper.ChayAsync(btnLoc, async () => dt = await Task.Run(() => QuanTriRepository.LichSu(tu, den, u, kq)), "…")) return;
            dgvLichSu.DataSource = dt;
            int thanhCong = dt.Rows.Cast<DataRow>().Count(r => Convert.ToString(r["Kết quả"]) == NguoiDungRepository.KetQuaThanhCong);
            lblTomTatLs.Text = $"{dt.Rows.Count} lượt · {thanhCong} thành công · {dt.Rows.Count - thanhCong} thất bại";
            lblTomTatLs.ForeColor = dt.Rows.Count - thanhCong > 0 ? Theme.Danger : Theme.Muted;
        }

        // =====================================================================
        // 06 Sao lưu
        // =====================================================================
        private async Task NapSaoLuuAsync()
        {
            try
            {
                var dt = await Task.Run(() => QuanTriRepository.DanhSachSaoLuu());
                dgvSaoLuu.DataSource = dt;
                if (dgvSaoLuu.Columns.Contains("Đường dẫn")) dgvSaoLuu.Columns["Đường dẫn"].FillWeight = 200;
                UiHelper.CanPhaiCotSo(dgvSaoLuu);
            }
            catch (Exception ex) { UiHelper.Loi(this, ex.GetBaseException().Message); }
        }

        private async Task SaoLuuAsync()
        {
            if (!UiHelper.XacNhan(this, "Tạo bản sao lưu dữ liệu hệ thống ngay bây giờ?")) return;
            string file = null;
            if (!await UiHelper.ChayAsync(btnSaoLuu, async () => file = await Task.Run(() => QuanTriRepository.SaoLuu(Session.IdNguoiDung)),
                    "Đang sao lưu…"))
            {
                lblSaoLuuKQ.ForeColor = Theme.Danger;
                lblSaoLuuKQ.Text = "Sao lưu thất bại.";
                return;
            }
            lblSaoLuuKQ.ForeColor = Theme.Success;
            lblSaoLuuKQ.Text = "Đã sao lưu: " + file;
            UiHelper.ThanhCong(this, "Đã tạo bản sao lưu.");
            await NapSaoLuuAsync();
        }
    }
}
