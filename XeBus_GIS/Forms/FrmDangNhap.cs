using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using XeBus_GIS.Data;
using XeBus_GIS.UI;

namespace XeBus_GIS.Forms
{
    /// <summary>Đăng nhập (B13) – ghi lịch sử đăng nhập (B14). DialogResult.OK khi đăng nhập thành công.</summary>
    public partial class FrmDangNhap : Form
    {
        public FrmDangNhap()
        {
            InitializeComponent();
            ApDungGiaoDien();

            btnDangNhap.Click += async (s, e) => await DangNhapAsync();
            lnkKhach.LinkClicked += async (s, e) => await VaoVoiTuCachKhachAsync();
            lnkDangKy.LinkClicked += (s, e) => HienDangKy(true);
            ucDangKy.QuayLai += () => HienDangKy(false);
            ucDangKy.DaGui += tenDN =>
            {
                HienDangKy(false);
                txtTenDN.Text = tenDN;
                txtMatKhau.Clear();
                BaoThanhCong("Đã gửi đăng ký. Bạn đăng nhập được sau khi quản trị viên duyệt tài khoản.");
            };
            btnHienMK.Click += (s, e) =>
            {
                txtMatKhau.UseSystemPasswordChar = !txtMatKhau.UseSystemPasswordChar;
                btnHienMK.Text = txtMatKhau.UseSystemPasswordChar ? Icons.Mat : Icons.AnMat;
                txtMatKhau.Focus();
            };
            txtTenDN.TextChanged += (s, e) => lblLoi.Visible = false;
            txtMatKhau.TextChanged += (s, e) => lblLoi.Visible = false;
            Shown += async (s, e) => await KiemTraKetNoiAsync();
        }

        private void ApDungGiaoDien()
        {
            BackColor = Theme.Surface;

            // bên trái: hình nền động (render Blender từ dữ liệu CSDLKG) + giới thiệu hệ thống
            nenVideo.TieuDe = "XeBus GIS";
            nenVideo.Slogan = "Mạng lưới trạm xe buýt Hà Nội trên một bản đồ";
            nenVideo.MoTa = "Quản lý trạm và tuyến, tìm khu dân cư chưa được phục vụ và đề xuất vị trí trạm mới "
                          + "dựa trên dân số, công trình thu hút chuyến đi.";
            nenVideo.TinhNang = TinhNang;
            nenVideo.ChuThichNguon = "Hình nền: mô hình 3D dựng từ dữ liệu CSDLKG (nhà, mặt nước, tuyến, trạm) · © OpenStreetMap contributors";
            nenVideo.Nap(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "nen_dangnhap.mp4"));

            // bên phải: cột đăng nhập, khối form luôn căn giữa
            pnlForm.BackColor = Theme.Surface;
            pnlKhung.BackColor = Theme.Surface;
            pnlForm.Width = UiHelper.Dp(480);
            pnlForm.Resize += (s, e) => CanGiuaKhung();
            ucDangKy.KhoiTao();
            CanGiuaKhung();

            lblDangNhap.Font = new Font("Segoe UI Semibold", 20f);
            lblDangNhap.ForeColor = Theme.Primary;
            UiHelper.ChuPhu(lblHuongDan);
            UiHelper.O(txtTenDN, txtMatKhau);
            txtTenDN.Font = txtMatKhau.Font = new Font("Segoe UI", 11.25f);
            UiHelper.NhanTruong(lblTenDN);
            UiHelper.NhanTruong(lblMatKhau);
            // ô nhập có khung riêng; nút hiện mật khẩu nằm TRONG khung ô mật khẩu ⇒ hai ô dài bằng nhau
            UiHelper.NutIcon(btnHienMK, Icons.Mat, "Hiện / ẩn mật khẩu", 11f);
            txtMatKhau.Width = txtTenDN.Width;
            UiHelper.KhungO(txtTenDN, Theme.DnNhan);
            UiHelper.KhungO(txtMatKhau, Theme.DnNhan, btnHienMK);

            // trang đăng nhập dùng tông than chì (không xanh ngọc)
            UiHelper.NutChinh(btnDangNhap, Theme.DnNhan, Theme.DnNhanHover, Theme.DnNhanNhan);
            btnDangNhap.Font = new Font("Segoe UI Semibold", 10.5f);
            lblLoi.ForeColor = Theme.Danger;
            foreach (var l in new[] { lnkKhach, lnkDangKy })
            {
                l.LinkColor = Theme.DnNhan;
                l.ActiveLinkColor = Theme.DnNhanHover;
                l.Cursor = Cursors.Hand;
                l.UseMnemonic = false;
            }
            lnkDangKy.LinkArea = new LinkArea(lnkDangKy.Text.IndexOf("Đăng ký", StringComparison.Ordinal), "Đăng ký".Length);
            lblKetNoi.Font = Theme.FontNhan;
            lblKetNoi.ForeColor = Theme.Muted;
            pnlForm.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.Border)) e.Graphics.DrawLine(pen, 0, 0, 0, pnlForm.Height);
            };
        }

        private void CanGiuaKhung()
        {
            foreach (Control k in new Control[] { pnlKhung, ucDangKy })
            {
                k.Left = Math.Max(UiHelper.Dp(24), (pnlForm.ClientSize.Width - k.Width) / 2);
                k.Top = Math.Max(UiHelper.Dp(24), (pnlForm.ClientSize.Height - lblKetNoi.Height - k.Height) / 2);
            }
        }

        /// <summary>Chuyển cột phải giữa đăng nhập và đăng ký.</summary>
        private void HienDangKy(bool dangKy)
        {
            pnlKhung.Visible = !dangKy;
            ucDangKy.Visible = dangKy;
            AcceptButton = dangKy ? null : btnDangNhap;
            Text = (dangKy ? "Đăng ký" : "Đăng nhập") + " – XeBus GIS";
            lblLoi.Visible = false;
            CanGiuaKhung();
            if (dangKy) ucDangKy.BatDau();
            else txtTenDN.Focus();
        }

        /// <summary>Ba nhóm chức năng chính của hệ thống (đúng với các màn hình đã có).</summary>
        private static readonly (string icon, string ten, string moTa)[] TinhNang =
        {
            (Icons.Tram, "Quản lý trạm, tuyến", "Tra cứu theo thuộc tính và trực tiếp trên bản đồ"),
            (Icons.PhanTich, "Phân tích thiếu trạm", "Khu dân cư nằm ngoài bán kính phục vụ của trạm"),
            (Icons.DeXuat, "Đề xuất trạm mới", "Chấm điểm vị trí theo dân số và công trình"),
        };

        /// <summary>Số liệu thật của mạng lưới (chỉ đọc) – hiện dưới phần giới thiệu.</summary>
        private async Task NapSoLieuAsync()
        {
            try
            {
                var dt = await Db.QueryAsync(@"SELECT (SELECT count(*) FROM xebus.trambuyt WHERE trangthai <> 'Ngừng hoạt động') AS tram,
                                                      (SELECT count(*) FROM xebus.tuyenbuyt) AS tuyen,
                                                      (SELECT count(*) FROM xebus.phuongxa) AS phuong");
                var r = dt.Rows[0];
                nenVideo.SoLieu = $"{Convert.ToInt64(r["tram"]):N0} trạm đang khai thác  ·  {Convert.ToInt64(r["tuyen"])} tuyến  ·  {Convert.ToInt64(r["phuong"])} phường/xã";
            }
            catch { /* không có số liệu thì bỏ qua dòng này */ }
        }

        private async Task KiemTraKetNoiAsync()
        {
            lblKetNoi.Text = "Đang kết nối CSDL CSDLKG…";
            string loi = null;
            bool ok = await Task.Run(() => Db.KiemTraKetNoi(out loi));
            lblKetNoi.ForeColor = ok ? Theme.Success : Theme.Danger;
            lblKetNoi.Text = ok ? "● Đã kết nối CSDL CSDLKG" : "● Không kết nối được CSDL";
            btnDangNhap.Enabled = lnkKhach.Enabled = ok;
            if (!ok) BaoLoi("Lỗi kết nối: " + loi);
            else
            {
                txtTenDN.Focus();
                await NapSoLieuAsync();
            }
        }

        private void BaoLoi(string noiDung)
        {
            lblLoi.ForeColor = Theme.Danger;
            lblLoi.Text = noiDung;
            lblLoi.Visible = true;
        }

        private void BaoThanhCong(string noiDung)
        {
            lblLoi.ForeColor = Theme.Success;
            lblLoi.Text = noiDung;
            lblLoi.Visible = true;
        }

        private async Task DangNhapAsync()
        {
            if (string.IsNullOrWhiteSpace(txtTenDN.Text)) { BaoLoi("Vui lòng nhập tên đăng nhập."); txtTenDN.Focus(); return; }
            if (string.IsNullOrEmpty(txtMatKhau.Text)) { BaoLoi("Vui lòng nhập mật khẩu."); txtMatKhau.Focus(); return; }

            string loi = null;
            string ten = txtTenDN.Text, mk = txtMatKhau.Text;
            bool ok = await UiHelper.ChayAsync(btnDangNhap, async () =>
                loi = await Task.Run(() => NguoiDungRepository.DangNhap(ten, mk)), "Đang đăng nhập…");
            if (!ok) return;
            if (loi != null)
            {
                BaoLoi(loi);
                txtMatKhau.SelectAll();
                txtMatKhau.Focus();
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private async Task VaoVoiTuCachKhachAsync()
        {
            bool ok = await UiHelper.ChayAsync(btnDangNhap, () => Task.Run(() => NguoiDungRepository.DangNhapKhach()));
            if (!ok) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
