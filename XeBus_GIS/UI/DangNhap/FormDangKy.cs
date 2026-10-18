using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace XeBus_GIS.UI.DangNhap
{
    /// <summary>
    /// Form đăng ký (WPF): tạo tài khoản nhóm Khách hàng, trạng thái "Chờ duyệt" (quản trị viên duyệt trong màn hình
    /// Quản trị). Kiểm tra ngay khi gõ: tên đăng nhập (định dạng + còn trống, debounce 400 ms), độ mạnh mật khẩu,
    /// nhập lại khớp. Nghiệp vụ do FrmDangNhap cấp qua delegate.
    /// </summary>
    internal class FormDangKy : StackPanel
    {
        public Func<string, Task<bool>> TenDaCo;                                  // true = đã có người dùng
        public Func<string, string, string, string, Task<string>> DangKy;        // (tên, họ tên, đơn vị, mk) → null | lỗi
        public event Action QuayLai;
        public event Action<string> DaGui;

        private static readonly Regex DinhDangTen = new Regex(@"^[A-Za-z0-9_.]{3,50}$");
        private readonly ONhap _hoTen, _donVi, _ten, _mk, _nhapLai;
        private readonly DoManhMatKhau _doManh = new DoManhMatKhau { Margin = new Thickness(0, -2, 0, 14) };
        private readonly NutDn _nutGui;
        private readonly ThongBaoDn _thongBao = new ThongBaoDn { Margin = new Thickness(0, 0, 0, 16) };
        private readonly DispatcherTimer _hen = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        private int _lanKiem;
        private bool? _tenTrong;   // null = chưa biết

        public FormDangKy()
        {
            var quayLai = new NutIcon(IconDn.QuayLai, "Quay lại đăng nhập (Esc)", 15) { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(-10, 0, 0, 10) };
            quayLai.Click += (s, e) => QuayLai?.Invoke();
            Children.Add(quayLai);

            Children.Add(MauDn.Chu_("Tạo tài khoản", 28, FontWeights.SemiBold, MauDn.Chu, MauDn.ChuTieuDe));
            var phu = MauDn.Chu_("Tài khoản mới thuộc nhóm Khách hàng và cần quản trị viên duyệt trước khi đăng nhập.", 14.5, FontWeights.Normal, MauDn.ChuPhu);
            phu.Margin = new Thickness(0, 6, 0, 24);
            Children.Add(phu);
            Children.Add(_thongBao);

            _hoTen = new ONhap("Họ và tên") { Margin = new Thickness(0, 0, 0, 4) };
            _hoTen.DoDaiToiDa = 100;
            Children.Add(_hoTen);
            _donVi = new ONhap("Đơn vị / cơ quan (không bắt buộc)") { Margin = new Thickness(0, 0, 0, 4) };
            _donVi.DoDaiToiDa = 150;
            Children.Add(_donVi);
            _ten = new ONhap("Tên đăng nhập", false, "3–50 ký tự: chữ không dấu, số, dấu _ hoặc dấu chấm.") { Margin = new Thickness(0, 0, 0, 4) };
            _ten.DoDaiToiDa = 50;
            Children.Add(_ten);
            _mk = new ONhap("Mật khẩu", true) { Margin = new Thickness(0, 0, 0, 0) };
            _mk.DoDaiToiDa = 128;
            Children.Add(_mk);
            Children.Add(_doManh);
            _nhapLai = new ONhap("Nhập lại mật khẩu", true) { Margin = new Thickness(0, 0, 0, 12) };
            _nhapLai.DoDaiToiDa = 128;
            Children.Add(_nhapLai);

            _nutGui = new NutDn("Gửi đăng ký", true, IconDn.MuiTenPhai);
            _nutGui.Click += async (s, e) => await GuiAsync();
            Children.Add(_nutGui);

            var hang = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 24, 0, 0) };
            var t = MauDn.Chu_("Đã có tài khoản?", 14, FontWeights.Normal, MauDn.ChuPhu);
            t.Margin = new Thickness(0, 0, 6, 0);
            hang.Children.Add(t);
            var lnk = new LienKet("Đăng nhập");
            lnk.Click += (s, e) => QuayLai?.Invoke();
            hang.Children.Add(lnk);
            Children.Add(hang);

            // kiểm tra trực tiếp
            _ten.DoiNoiDung += () =>
            {
                _hen.Stop();
                _tenTrong = null;
                _lanKiem++;
                var ten = _ten.Text.Trim();
                if (ten.Length == 0) { _ten.DatOk(null); return; }
                if (!DinhDangTen.IsMatch(ten))
                {
                    if (ten.Length >= 3 || Regex.IsMatch(ten, @"[^A-Za-z0-9_.]")) _ten.DatLoi(ThongBaoDinhDang(ten));
                    return;
                }
                _hen.Start();
            };
            _hen.Tick += async (s, e) => { _hen.Stop(); await KiemTenAsync(); };
            _mk.DoiNoiDung += () =>
            {
                _doManh.DatMatKhau(_mk.Text);
                if (_nhapLai.Text.Length > 0) KiemNhapLai();
            };
            _nhapLai.DoiNoiDung += KiemNhapLai;
            _doManh.DatMatKhau("");

            PreviewKeyDown += async (s, e) =>
            {
                if (e.Key == Key.Escape) { e.Handled = true; QuayLai?.Invoke(); return; }
                if (e.Key != Key.Enter || e.OriginalSource is Button) return;
                e.Handled = true;
                await GuiAsync();
            };
        }

        private static string ThongBaoDinhDang(string ten)
        {
            if (ten.Length < 3) return "Tên đăng nhập cần ít nhất 3 ký tự.";
            if (Regex.IsMatch(ten, @"\s")) return "Tên đăng nhập không được có khoảng trắng.";
            return "Chỉ dùng chữ không dấu, số, dấu _ hoặc dấu chấm.";
        }

        private async Task<bool> KiemTenAsync()
        {
            var ten = _ten.Text.Trim();
            if (!DinhDangTen.IsMatch(ten) || TenDaCo == null) return false;
            int lan = ++_lanKiem;
            bool daCo;
            try { daCo = await TenDaCo(ten); }
            catch { return true; }   // không kiểm được thì để máy chủ kiểm lúc gửi
            if (lan != _lanKiem) return _tenTrong == true;   // người dùng đã gõ tiếp – bỏ kết quả cũ
            _tenTrong = !daCo;
            if (daCo) _ten.DatLoi("Tên đăng nhập này đã có người dùng.");
            else _ten.DatOk("Tên đăng nhập dùng được.");
            return !daCo;
        }

        private void KiemNhapLai()
        {
            if (_nhapLai.Text.Length == 0) { _nhapLai.DatOk(null); return; }
            if (_nhapLai.Text == _mk.Text) _nhapLai.DatOk("Mật khẩu khớp.");
            else if (_nhapLai.Text.Length >= _mk.Text.Length) _nhapLai.DatLoi("Hai lần nhập chưa khớp.");
            else _nhapLai.DatOk(null);
        }

        /// <summary>Xoá nội dung cũ và đặt con trỏ vào ô đầu.</summary>
        public void BatDau()
        {
            foreach (var o in new[] { _hoTen, _donVi, _ten, _mk, _nhapLai }) { o.Text = ""; o.DatLoi(null); }
            _tenTrong = null;
            _thongBao.An();
            _doManh.DatMatKhau("");
            _nutGui.DangCho = false;
        }

        public void DatFocusDau() => _hoTen.DatFocus();

        private bool _dangGui;

        private async Task GuiAsync()
        {
            if (_dangGui || DangKy == null) return;
            _thongBao.An();
            var hoTen = _hoTen.Text.Trim();
            var ten = _ten.Text.Trim();
            ONhap oLoi = null;
            if (hoTen.Length == 0) { _hoTen.DatLoi("Nhập họ và tên.", true); oLoi = oLoi ?? _hoTen; }
            if (!DinhDangTen.IsMatch(ten)) { _ten.DatLoi(ten.Length == 0 ? "Nhập tên đăng nhập." : ThongBaoDinhDang(ten), true); oLoi = oLoi ?? _ten; }
            if (_mk.Text.Length < 6) { _mk.DatLoi("Mật khẩu tối thiểu 6 ký tự.", true); oLoi = oLoi ?? _mk; }
            else if (_nhapLai.Text != _mk.Text) { _nhapLai.DatLoi("Hai lần nhập chưa khớp.", true); oLoi = oLoi ?? _nhapLai; }
            if (oLoi != null) { oLoi.DatFocus(); return; }

            _dangGui = true;
            _nutGui.DangCho = true;
            string loi;
            try
            {
                if (_tenTrong == null && !await KiemTenAsync() && _tenTrong == false) loi = "Tên đăng nhập này đã có người dùng.";
                else loi = await DangKy(ten, hoTen, _donVi.Text.Trim(), _mk.Text);
            }
            catch (Exception ex) { loi = ex.GetBaseException().Message; }
            _dangGui = false;
            _nutGui.DangCho = false;

            if (loi == null) { DaGui?.Invoke(ten); return; }
            if (loi.StartsWith("Tên đăng nhập", StringComparison.Ordinal)) { _ten.DatLoi(loi, true); _ten.DatFocus(); }
            else if (loi.StartsWith("Mật khẩu", StringComparison.Ordinal)) { _mk.DatLoi(loi, true); _mk.DatFocus(); }
            else if (loi.Contains("họ tên")) { _hoTen.DatLoi(loi, true); _hoTen.DatFocus(); }
            else { _thongBao.Hien(loi, ThongBaoDn.Loai.Loi); HieuUng.Rung(_thongBao); }
        }
    }
}
