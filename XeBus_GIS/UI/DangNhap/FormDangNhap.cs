using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace XeBus_GIS.UI.DangNhap
{
    /// <summary>
    /// Form đăng nhập (WPF). Chỉ lo giao diện + luồng thao tác; nghiệp vụ do FrmDangNhap cấp qua delegate
    /// (<see cref="DangNhap"/>, <see cref="VaoKhach"/>) – trả null nếu thành công, ngược lại là thông báo lỗi.
    /// </summary>
    internal class FormDangNhap : StackPanel
    {
        public Func<string, string, Task<string>> DangNhap;
        public Func<Task<string>> VaoKhach;
        public event Action<string, bool> DaDangNhap;   // (tên đăng nhập | null nếu khách, ghi nhớ tên)
        public event Action MoDangKy;

        private readonly ONhap _ten, _mk;
        private readonly HopKiem _ghiNho;
        private readonly NutDn _nutDangNhap, _nutKhach;
        private readonly LienKet _lnkDangKy;
        private readonly ThongBaoDn _thongBao = new ThongBaoDn { Margin = new Thickness(0, 0, 0, 20) };
        private bool _coKetNoi;

        public FormDangNhap()
        {
            var tieuDe = MauDn.Chu_("Đăng nhập", 28, FontWeights.SemiBold, MauDn.Chu, MauDn.ChuTieuDe);
            Children.Add(tieuDe);
            var phu = MauDn.Chu_("Dùng tài khoản do quản trị viên cấp để vào hệ thống.", 14.5, FontWeights.Normal, MauDn.ChuPhu);
            phu.Margin = new Thickness(0, 6, 0, 28);
            Children.Add(phu);

            Children.Add(_thongBao);

            _ten = new ONhap("Tên đăng nhập") { Margin = new Thickness(0, 0, 0, 6) };
            _ten.DoDaiToiDa = 50;
            Children.Add(_ten);
            _mk = new ONhap("Mật khẩu", true) { Margin = new Thickness(0, 0, 0, 6) };
            _mk.DoDaiToiDa = 128;
            Children.Add(_mk);

            _ghiNho = new HopKiem("Ghi nhớ tên đăng nhập") { Margin = new Thickness(2, 0, 0, 24) };
            Children.Add(_ghiNho);

            _nutDangNhap = new NutDn("Đăng nhập", true, IconDn.MuiTenPhai);
            _nutDangNhap.Click += async (s, e) => await GuiAsync();
            Children.Add(_nutDangNhap);

            // "hoặc"
            var hoac = new Grid { Margin = new Thickness(0, 20, 0, 20) };
            hoac.ColumnDefinitions.Add(new ColumnDefinition());
            hoac.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            hoac.ColumnDefinitions.Add(new ColumnDefinition());
            var vach1 = new Border { Height = 1, Background = new SolidColorBrush(MauDn.W(Theme.Border)), VerticalAlignment = VerticalAlignment.Center };
            var vach2 = new Border { Height = 1, Background = new SolidColorBrush(MauDn.W(Theme.Border)), VerticalAlignment = VerticalAlignment.Center };
            var tHoac = MauDn.Chu_("hoặc", 13, FontWeights.Normal, MauDn.ChuPhu);
            tHoac.Margin = new Thickness(14, 0, 14, 2);
            Grid.SetColumn(tHoac, 1);
            Grid.SetColumn(vach2, 2);
            hoac.Children.Add(vach1);
            hoac.Children.Add(tHoac);
            hoac.Children.Add(vach2);
            Children.Add(hoac);

            _nutKhach = new NutDn("Tiếp tục với tư cách khách", false);
            _nutKhach.ToolTip = "Chỉ xem và tra cứu trạm, tuyến – không cần tài khoản";
            _nutKhach.Click += async (s, e) => await KhachAsync();
            Children.Add(_nutKhach);

            var hangDk = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 28, 0, 0) };
            var tDk = MauDn.Chu_("Chưa có tài khoản?", 14, FontWeights.Normal, MauDn.ChuPhu);
            tDk.Margin = new Thickness(0, 0, 6, 0);
            hangDk.Children.Add(tDk);
            _lnkDangKy = new LienKet("Đăng ký");
            _lnkDangKy.Click += (s, e) => MoDangKy?.Invoke();
            hangDk.Children.Add(_lnkDangKy);
            Children.Add(hangDk);

            // Enter trong ô ⇒ đăng nhập; ô tên trống mà Enter ⇒ sang ô mật khẩu
            PreviewKeyDown += async (s, e) =>
            {
                if (e.Key != Key.Enter || e.OriginalSource is Button) return;
                e.Handled = true;
                if (_ten.IsKeyboardFocusWithin && _ten.Text.Trim().Length > 0 && _mk.Text.Length == 0) { _mk.DatFocus(); return; }
                await GuiAsync();
            };
            DatKetNoi(false);
        }

        public string TenDangNhap { get => _ten.Text.Trim(); set => _ten.Text = value; }
        public bool GhiNho { get => _ghiNho.IsChecked == true; set => _ghiNho.IsChecked = value; }

        /// <summary>Đặt con trỏ: ô tên nếu trống, ngược lại ô mật khẩu.</summary>
        public void DatFocusDau()
        {
            if (_ten.Text.Trim().Length == 0) _ten.DatFocus(); else _mk.DatFocus();
        }

        /// <summary>Có kết nối CSDL hay chưa – chưa có thì khoá nút đăng nhập / khách.</summary>
        public void DatKetNoi(bool ok)
        {
            _coKetNoi = ok;
            _nutDangNhap.IsEnabled = _nutKhach.IsEnabled = ok;
            _lnkDangKy.IsEnabled = ok;
        }

        public void BaoLoi(string chu) => _thongBao.Hien(chu, ThongBaoDn.Loai.Loi);
        public void BaoThanhCong(string chu) => _thongBao.Hien(chu, ThongBaoDn.Loai.ThanhCong);
        public void XoaThongBao() => _thongBao.An();

        public void XoaMatKhau() => _mk.Text = "";

        private bool _dangGui;

        private async Task GuiAsync()
        {
            if (_dangGui || !_coKetNoi || DangNhap == null) return;
            _thongBao.An();
            var ten = _ten.Text.Trim();
            var mk = _mk.Text;
            if (ten.Length == 0) { _ten.DatLoi("Nhập tên đăng nhập.", true); _ten.DatFocus(); return; }
            if (mk.Length == 0) { _mk.DatLoi("Nhập mật khẩu.", true); _mk.DatFocus(); return; }

            _dangGui = true;
            _nutDangNhap.DangCho = true;
            _nutKhach.IsEnabled = false;
            string loi;
            try { loi = await DangNhap(ten, mk); }
            catch (Exception ex) { loi = ex.GetBaseException().Message; }
            _dangGui = false;
            _nutDangNhap.DangCho = false;
            _nutKhach.IsEnabled = _coKetNoi;

            if (loi == null)
            {
                _nutDangNhap.HienXong("Đã đăng nhập");
                DaDangNhap?.Invoke(ten, GhiNho);
                return;
            }
            // sai tên/mật khẩu: báo ngay dưới ô mật khẩu (không nói rõ sai phần nào – tránh dò tài khoản)
            if (loi.IndexOf("không đúng", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _mk.DatLoi(loi, true);
                _mk.ChonHet();
                _mk.DatFocus();
            }
            else
            {
                BaoLoi(loi);
                HieuUng.Rung(_thongBao);
            }
        }

        private async Task KhachAsync()
        {
            if (_dangGui || !_coKetNoi || VaoKhach == null) return;
            _dangGui = true;
            _nutKhach.DangCho = true;
            _nutDangNhap.IsEnabled = false;
            string loi;
            try { loi = await VaoKhach(); }
            catch (Exception ex) { loi = ex.GetBaseException().Message; }
            _dangGui = false;
            _nutKhach.DangCho = false;
            _nutDangNhap.IsEnabled = _coKetNoi;
            if (loi == null) { DaDangNhap?.Invoke(null, false); return; }
            BaoLoi(loi);
        }
    }
}
