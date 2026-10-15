using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using XeBus_GIS.Data;
using XeBus_GIS.UI;

namespace XeBus_GIS.Forms
{
    /// <summary>
    /// Đăng ký tài khoản (trong cột phải của trang đăng nhập): tạo người dùng nhóm Khách hàng ở trạng thái
    /// "Chờ duyệt" – quản trị viên duyệt (đổi trạng thái / nhóm) trong màn hình Quản trị.
    /// </summary>
    public partial class UcDangKy : UserControl
    {
        /// <summary>Người dùng bấm "Đăng nhập" để quay lại.</summary>
        public event Action QuayLai;

        /// <summary>Đăng ký thành công (tên đăng nhập vừa tạo).</summary>
        public event Action<string> DaGui;

        public UcDangKy()
        {
            InitializeComponent();
        }

        /// <summary>Áp giao diện + gắn sự kiện (gọi từ trang đăng nhập; tách khỏi ctor để VS Designer mở được).</summary>
        public void KhoiTao()
        {
            BackColor = Theme.Surface;
            lblTieuDe.Font = new Font("Segoe UI Semibold", 20f);
            lblTieuDe.ForeColor = Theme.Primary;
            UiHelper.ChuPhu(lblHuongDan);
            foreach (var l in new[] { lblHoTen, lblDonVi, lblTenDN, lblMatKhau, lblNhapLai }) UiHelper.NhanTruong(l);
            UiHelper.O(txtHoTen, txtDonVi, txtTenDN, txtMatKhau, txtNhapLai);
            var f = new Font("Segoe UI", 11.25f);
            foreach (var t in new[] { txtHoTen, txtDonVi, txtTenDN, txtMatKhau, txtNhapLai })
            {
                t.Font = f;
                UiHelper.KhungO(t, Theme.DnNhan);
            }
            UiHelper.NutChinh(btnGui, Theme.DnNhan, Theme.DnNhanHover, Theme.DnNhanNhan);
            btnGui.Font = new Font("Segoe UI Semibold", 10.5f);
            lblLoi.ForeColor = Theme.Danger;
            lnkQuayLai.LinkColor = Theme.DnNhan;
            lnkQuayLai.ActiveLinkColor = Theme.DnNhanHover;
            lnkQuayLai.Cursor = Cursors.Hand;
            lnkQuayLai.UseMnemonic = false;
            // chỉ gạch chân phần "Đăng nhập"
            lnkQuayLai.LinkArea = new LinkArea(lnkQuayLai.Text.IndexOf("Đăng nhập", StringComparison.Ordinal), "Đăng nhập".Length);

            btnGui.Click += async (s, e) => await GuiAsync();
            lnkQuayLai.LinkClicked += (s, e) => QuayLai?.Invoke();
            foreach (var t in new[] { txtHoTen, txtDonVi, txtTenDN, txtMatKhau, txtNhapLai })
            {
                t.TextChanged += (s, e) => lblLoi.Visible = false;
                t.KeyDown += async (s, e) =>
                {
                    if (e.KeyCode != Keys.Enter) return;
                    e.SuppressKeyPress = true;
                    await GuiAsync();
                };
            }
        }

        /// <summary>Xoá nội dung cũ và đặt con trỏ vào ô đầu.</summary>
        public void BatDau()
        {
            foreach (var t in new[] { txtHoTen, txtDonVi, txtTenDN, txtMatKhau, txtNhapLai }) t.Clear();
            lblLoi.Visible = false;
            txtHoTen.Focus();
        }

        private void BaoLoi(string noiDung, Control o)
        {
            lblLoi.Text = noiDung;
            lblLoi.Visible = true;
            o?.Focus();
        }

        private async Task GuiAsync()
        {
            if (!btnGui.Enabled) return;
            if (string.IsNullOrWhiteSpace(txtHoTen.Text)) { BaoLoi("Vui lòng nhập họ và tên.", txtHoTen); return; }
            if (string.IsNullOrWhiteSpace(txtTenDN.Text)) { BaoLoi("Vui lòng nhập tên đăng nhập.", txtTenDN); return; }
            if (txtMatKhau.Text.Length < 6) { BaoLoi("Mật khẩu tối thiểu 6 ký tự.", txtMatKhau); return; }
            if (txtMatKhau.Text != txtNhapLai.Text) { BaoLoi("Hai lần nhập mật khẩu không khớp.", txtNhapLai); return; }

            string loi = null;
            string ten = txtTenDN.Text.Trim(), hoTen = txtHoTen.Text, donVi = txtDonVi.Text, mk = txtMatKhau.Text;
            bool ok = await UiHelper.ChayAsync(btnGui, async () =>
                loi = await Task.Run(() => NguoiDungRepository.DangKy(ten, hoTen, donVi, mk)), "Đang gửi…");
            if (!ok) return;
            if (loi != null)
            {
                BaoLoi(loi, loi.StartsWith("Tên đăng nhập") ? txtTenDN : loi.StartsWith("Mật khẩu") ? (Control)txtMatKhau : txtHoTen);
                return;
            }
            DaGui?.Invoke(ten);
        }
    }
}
