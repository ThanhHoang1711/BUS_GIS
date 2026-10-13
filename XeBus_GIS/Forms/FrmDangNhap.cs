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
            btnHienMK.Click += (s, e) =>
            {
                txtMatKhau.UseSystemPasswordChar = !txtMatKhau.UseSystemPasswordChar;
                UiHelper.DatTrangThaiBat(btnHienMK, !txtMatKhau.UseSystemPasswordChar);
            };
            txtTenDN.TextChanged += (s, e) => lblLoi.Visible = false;
            txtMatKhau.TextChanged += (s, e) => lblLoi.Visible = false;
            Shown += async (s, e) => await KiemTraKetNoiAsync();
        }

        private void ApDungGiaoDien()
        {
            BackColor = Theme.Surface;

            // dải nhận diện bên trái
            pnlBrand.BackColor = Theme.AccentSoft;
            lblLogo.Text = Icons.Bus;
            lblLogo.Font = Theme.FontIcon(22f);
            lblLogo.ForeColor = Color.White;
            lblLogo.BackColor = Theme.Accent;
            lblTenApp.Font = new Font("Segoe UI Semibold", 18f);
            lblTenApp.ForeColor = Theme.Primary;
            lblMoTa.Font = Theme.FontThuong;
            lblMoTa.ForeColor = Theme.Secondary;
            lblNguon.Font = Theme.FontNho;
            lblNguon.ForeColor = Theme.Muted;
            lblTinhNang.Paint += VeTinhNang;

            // cột đăng nhập
            lblDangNhap.Font = Theme.FontTrang;
            lblDangNhap.ForeColor = Theme.Primary;
            UiHelper.ChuPhu(lblHuongDan);
            UiHelper.O(txtTenDN, txtMatKhau);
            UiHelper.NhanTruong(lblTenDN);
            UiHelper.NhanTruong(lblMatKhau);
            UiHelper.NutIcon(btnHienMK, Icons.Mat, "Hiện / ẩn mật khẩu", 11f);
            btnHienMK.FlatAppearance.BorderSize = 1;
            btnHienMK.FlatAppearance.BorderColor = Theme.Border;
            UiHelper.NutChinh(btnDangNhap);
            lblLoi.ForeColor = Theme.Danger;
            lnkKhach.LinkColor = Theme.Accent;
            lnkKhach.ActiveLinkColor = Theme.AccentHover;
            lnkKhach.Cursor = Cursors.Hand;
            lblKetNoi.Font = Theme.FontNhan;
            lblKetNoi.ForeColor = Theme.Muted;
        }

        /// <summary>Ba nhóm chức năng chính của hệ thống (đúng với các màn hình đã có).</summary>
        private static readonly (string icon, string ten, string moTa)[] TinhNang =
        {
            (Icons.Tram, "Quản lý trạm, tuyến", "Tra cứu theo thuộc tính và trên bản đồ"),
            (Icons.PhanTich, "Phân tích thiếu trạm", "Khu dân cư nằm ngoài bán kính phục vụ"),
            (Icons.DeXuat, "Đề xuất trạm mới", "Chấm điểm theo dân số, công trình"),
        };

        private void VeTinhNang(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            int y = 0, cao = UiHelper.Dp(48);
            using (var f = Theme.FontIcon(13f))
                foreach (var (icon, ten, moTa) in TinhNang)
                {
                    TextRenderer.DrawText(g, icon, f, new Rectangle(0, y, UiHelper.Dp(28), UiHelper.Dp(24)), Theme.Accent,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                    TextRenderer.DrawText(g, ten, Theme.FontDam, new Point(UiHelper.Dp(32), y + UiHelper.Dp(2)), Theme.Primary);
                    TextRenderer.DrawText(g, moTa, Theme.FontNhan, new Rectangle(UiHelper.Dp(32), y + UiHelper.Dp(22), lblTinhNang.Width - UiHelper.Dp(32), UiHelper.Dp(20)),
                        Theme.Muted, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                    y += cao;
                }
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
            else txtTenDN.Focus();
        }

        private void BaoLoi(string noiDung)
        {
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
