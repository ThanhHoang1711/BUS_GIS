using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WfColor = System.Drawing.Color;

namespace XeBus_GIS.UI.DangNhap
{
    /// <summary>
    /// Bảng màu + chữ của trang đăng nhập, lấy từ <see cref="Theme"/> (sáng/tối). Tông than chì (Theme.DnNhan) – không
    /// xanh ngọc ở trang này (người dùng yêu cầu); hổ phách chỉ cho cảnh báo (Caps Lock).
    /// </summary>
    internal static class MauDn
    {
        public static Color W(WfColor c, byte? a = null) => Color.FromArgb(a ?? c.A, c.R, c.G, c.B);
        public static SolidColorBrush B(Color c) => new SolidColorBrush(c);   // KHÔNG freeze – còn animate màu

        public static Color Nen => W(Theme.Surface);
        public static Color NenO => Theme.Toi ? W(Theme.SurfaceAlt) : Colors.White;
        public static Color Chu => W(Theme.Text);
        public static Color ChuPhu => W(Theme.Muted);
        public static Color Vien => W(Theme.BorderStrong);
        public static Color VienHover => Theme.Toi ? Color.FromRgb(0x6B, 0x78, 0x90) : Color.FromRgb(0x94, 0xA3, 0xB8);
        public static Color Nhan => W(Theme.DnNhan);                 // nút chính, viền focus
        public static Color NhanHover => W(Theme.DnNhanHover);
        public static Color NhanNhan => W(Theme.DnNhanNhan);
        public static Color ChuTrenNhan => Theme.Toi ? Color.FromRgb(0x11, 0x18, 0x27) : Colors.White;
        public static Color VongFocus => W(Theme.DnNhan, (byte)(Theme.Toi ? 0x55 : 0x33));
        public static Color Loi => W(Theme.Danger);
        public static Color ThanhCong => W(Theme.Success);
        public static Color CanhBao => W(Theme.WarningText);
        public static Color Phu => W(Theme.DnPhu);                    // lớp phủ trên video

        public static readonly FontFamily ChuTieuDe = new FontFamily("Segoe UI Variable Display, Segoe UI");
        public static readonly FontFamily ChuThuong = new FontFamily("Segoe UI Variable Text, Segoe UI");
        public static FontFamily ChuIcon => new FontFamily(Theme.TenFontIcon);

        public static TextBlock Chu_(string text, double co, FontWeight dam, Color mau, FontFamily f = null)
        {
            var t = new TextBlock
            {
                Text = text,
                FontSize = co,
                FontWeight = dam,
                FontFamily = f ?? ChuThuong,
                Foreground = new SolidColorBrush(mau),
                TextWrapping = TextWrapping.Wrap,
            };
            TextOptions.SetTextFormattingMode(t, TextFormattingMode.Ideal);
            return t;
        }

        public static TextBlock Icon(string glyph, double co, Color mau) => new TextBlock
        {
            Text = glyph,
            FontFamily = ChuIcon,
            FontSize = co,
            Foreground = new SolidColorBrush(mau),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
    }

    /// <summary>Glyph Segoe Fluent Icons dùng riêng ở trang đăng nhập.</summary>
    internal static class IconDn
    {
        public const string QuayLai = "";
        public const string KhoaMo = "";
        public const string NguoiDung = "";
        public const string DauTich = "";
        public const string ThongTin = "";
        public const string CanhBao = "";
        public const string Loi = "";
        public const string Mat = "";
        public const string AnMat = "";
        public const string CoQuan = "";
        public const string MuiTenPhai = "";
    }

    // =====================================================================================================
    /// <summary>
    /// Ô nhập có nhãn nổi: nhãn nằm giữa ô khi trống, thu nhỏ lên mép trên khi focus / có chữ (150 ms). Viền + vòng focus
    /// 3 px đổi màu mượt. Dòng gợi ý / lỗi bên dưới luôn chiếm chỗ cố định ⇒ không giật bố cục khi báo lỗi.
    /// Mật khẩu: nút hiện/ẩn trong ô, cảnh báo Caps Lock.
    /// </summary>
    internal class ONhap : StackPanel
    {
        private readonly Border _vong;          // vòng focus ngoài
        private readonly Border _o;             // khung ô
        private readonly TextBlock _nhan;
        private readonly ScaleTransform _nhanCo = new ScaleTransform(1, 1);
        private readonly TranslateTransform _nhanDich = new TranslateTransform();
        private readonly TextBox _tb;
        private readonly PasswordBox _pb;
        private readonly Button _nutMat;
        private readonly TextBlock _dongIcon, _dongChu;
        private readonly StackPanel _dong;
        private readonly SolidColorBrush _brVien, _brVong, _brNhan, _brDong;
        private string _goiY = "";
        private bool _coLoi, _hienMk;

        public bool LaMatKhau { get; }
        public event Action DoiNoiDung;

        public ONhap(string nhan, bool matKhau = false, string goiY = "")
        {
            LaMatKhau = matKhau;
            _goiY = goiY ?? "";
            _brVien = MauDn.B(MauDn.Vien);
            _brVong = MauDn.B(Colors.Transparent);
            _brNhan = MauDn.B(MauDn.ChuPhu);
            _brDong = MauDn.B(MauDn.ChuPhu);

            var luoi = new Grid();
            luoi.ColumnDefinitions.Add(new ColumnDefinition());
            luoi.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _nhan = MauDn.Chu_(nhan, 15, FontWeights.Normal, MauDn.ChuPhu);
            _nhan.Foreground = _brNhan;
            _nhan.TextWrapping = TextWrapping.NoWrap;
            _nhan.IsHitTestVisible = false;
            _nhan.VerticalAlignment = VerticalAlignment.Center;
            _nhan.Margin = new Thickness(14, 0, 0, 0);
            _nhan.RenderTransformOrigin = new Point(0, 0.5);
            _nhan.RenderTransform = new TransformGroup { Children = { _nhanCo, _nhanDich } };

            var kieuO = new Thickness(13, 19, 8, 3);
            if (matKhau)
            {
                _pb = new PasswordBox();
                KieuNhap(_pb, kieuO);
                _pb.PasswordChanged += (s, e) => { DongBoMk(true); KhiDoi(); };
                _tb = new TextBox { Visibility = Visibility.Collapsed };
                KieuNhap(_tb, kieuO);
                _tb.TextChanged += (s, e) => { if (_hienMk) { DongBoMk(false); KhiDoi(); } };
                luoi.Children.Add(_pb);
                luoi.Children.Add(_tb);
                AutomationProperties.SetName(_pb, nhan);
                AutomationProperties.SetName(_tb, nhan);

                _nutMat = new NutIcon(IconDn.Mat, "Hiện mật khẩu") { Margin = new Thickness(0, 0, 6, 0) };
                _nutMat.Click += (s, e) => DoiHienMk();
                Grid.SetColumn(_nutMat, 1);
                luoi.Children.Add(_nutMat);
                foreach (UIElement x in new UIElement[] { _pb, _tb })
                {
                    x.PreviewKeyDown += (s, e) => KiemCapsLock();
                    x.KeyUp += (s, e) => KiemCapsLock();
                }
            }
            else
            {
                _tb = new TextBox();
                KieuNhap(_tb, kieuO);
                _tb.TextChanged += (s, e) => KhiDoi();
                luoi.Children.Add(_tb);
                AutomationProperties.SetName(_tb, nhan);
            }
            Grid.SetColumnSpan(_nhan, 1);
            luoi.Children.Add(_nhan);

            _o = new Border
            {
                Height = 52,
                CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(1.5),
                BorderBrush = _brVien,
                Background = MauDn.B(MauDn.NenO),
                Child = luoi,
                Cursor = Cursors.IBeam,
            };
            _o.MouseLeftButtonDown += (s, e) => { DatFocus(); e.Handled = true; };
            _o.MouseEnter += (s, e) => { if (!CoFocus && !_coLoi) HieuUng.DoiMau(_brVien, MauDn.VienHover); };
            _o.MouseLeave += (s, e) => { if (!CoFocus && !_coLoi) HieuUng.DoiMau(_brVien, MauDn.Vien); };

            _vong = new Border
            {
                CornerRadius = new CornerRadius(13),
                BorderThickness = new Thickness(3),
                BorderBrush = _brVong,
                Margin = new Thickness(-3),
                Child = _o,
            };
            Children.Add(_vong);

            // dòng gợi ý / lỗi (giữ chỗ 22 px)
            _dongIcon = MauDn.Icon("", 12, MauDn.ChuPhu);
            _dongIcon.Foreground = _brDong;
            _dongIcon.Margin = new Thickness(0, 1, 6, 0);
            _dongIcon.Visibility = Visibility.Collapsed;
            _dongChu = MauDn.Chu_(_goiY, 12.5, FontWeights.Normal, MauDn.ChuPhu);
            _dongChu.Foreground = _brDong;
            _dong = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(4, 6, 0, 0), MinHeight = 20 };
            _dong.Children.Add(_dongIcon);
            _dong.Children.Add(_dongChu);
            Children.Add(_dong);

            foreach (UIElement x in _pb != null ? new UIElement[] { _pb, _tb } : new UIElement[] { _tb })
            {
                x.GotKeyboardFocus += (s, e) => CapNhatTrangThai(true);
                x.LostKeyboardFocus += (s, e) => CapNhatTrangThai(true);
            }
            Loaded += (s, e) => CapNhatTrangThai(false);
        }

        private static void KieuNhap(Control c, Thickness p)
        {
            c.Background = Brushes.Transparent;
            c.BorderThickness = new Thickness(0);
            c.Padding = p;
            c.FontFamily = MauDn.ChuThuong;
            c.FontSize = 15;
            c.Foreground = new SolidColorBrush(MauDn.Chu);
            c.VerticalContentAlignment = VerticalAlignment.Center;
            c.FocusVisualStyle = null;
            if (c is TextBox t) t.CaretBrush = new SolidColorBrush(MauDn.Chu);
            if (c is PasswordBox p2) p2.CaretBrush = new SolidColorBrush(MauDn.Chu);
        }

        private bool CoFocus => (_pb != null && _pb.IsKeyboardFocusWithin) || _tb.IsKeyboardFocusWithin;
        private bool CoChu => Text.Length > 0;

        public string Text
        {
            get => LaMatKhau ? (_hienMk ? _tb.Text : _pb.Password) : _tb.Text;
            set
            {
                if (LaMatKhau) { _pb.Password = value ?? ""; _tb.Text = value ?? ""; }
                else _tb.Text = value ?? "";
                CapNhatTrangThai(false);
            }
        }

        public void DatFocus()
        {
            var x = LaMatKhau && !_hienMk ? (Control)_pb : _tb;
            x.Focus();
            Keyboard.Focus(x);
            if (x is TextBox t) t.CaretIndex = t.Text.Length;
        }

        public void ChonHet()
        {
            if (LaMatKhau && !_hienMk) _pb.SelectAll(); else _tb.SelectAll();
        }

        public int DoDaiToiDa { set { _tb.MaxLength = value; if (_pb != null) _pb.MaxLength = value; } }

        private void KhiDoi()
        {
            if (_coLoi) DatLoi(null);
            CapNhatTrangThai(true);
            DoiNoiDung?.Invoke();
        }

        private bool _dongBo;
        private void DongBoMk(bool tuPb)
        {
            if (_dongBo) return;
            _dongBo = true;
            if (tuPb) _tb.Text = _pb.Password; else _pb.Password = _tb.Text;
            _dongBo = false;
        }

        private void DoiHienMk()
        {
            _hienMk = !_hienMk;
            DongBoMk(!_hienMk);
            _tb.Visibility = _hienMk ? Visibility.Visible : Visibility.Collapsed;
            _pb.Visibility = _hienMk ? Visibility.Collapsed : Visibility.Visible;
            ((NutIcon)_nutMat).Glyph = _hienMk ? IconDn.AnMat : IconDn.Mat;
            AutomationProperties.SetName(_nutMat, _hienMk ? "Ẩn mật khẩu" : "Hiện mật khẩu");
            _nutMat.ToolTip = _hienMk ? "Ẩn mật khẩu" : "Hiện mật khẩu";
            DatFocus();
        }

        /// <summary>Nhãn nổi + màu viền theo trạng thái focus / có chữ / lỗi.</summary>
        private void CapNhatTrangThai(bool coHieuUng)
        {
            bool len = CoFocus || CoChu;
            int ms = coHieuUng ? 150 : 0;
            HieuUng.So(_nhanCo, ScaleTransform.ScaleXProperty, len ? 0.8 : 1, ms);
            HieuUng.So(_nhanCo, ScaleTransform.ScaleYProperty, len ? 0.8 : 1, ms);
            HieuUng.So(_nhanDich, TranslateTransform.YProperty, len ? -10 : 0, ms);
            HieuUng.DoiMau(_brNhan, _coLoi ? MauDn.Loi : CoFocus ? MauDn.Nhan : MauDn.ChuPhu, ms);
            HieuUng.DoiMau(_brVien, _coLoi ? MauDn.Loi : CoFocus ? MauDn.Nhan : MauDn.Vien, ms);
            HieuUng.DoiMau(_brVong, CoFocus ? (_coLoi ? MauDn.W(Theme.Danger, 0x33) : MauDn.VongFocus) : Colors.Transparent, ms);
            if (LaMatKhau) KiemCapsLock();
        }

        private bool _capsHien;
        private void KiemCapsLock()
        {
            if (!LaMatKhau || _coLoi) return;
            bool caps = CoFocus && Keyboard.IsKeyToggled(Key.CapsLock);
            if (caps == _capsHien) return;
            _capsHien = caps;
            if (caps) HienDong(IconDn.CanhBao, "Caps Lock đang bật", MauDn.CanhBao);
            else HienDong(null, _goiY, MauDn.ChuPhu);
        }

        private void HienDong(string icon, string chu, Color mau)
        {
            _dongIcon.Text = icon ?? "";
            _dongIcon.Visibility = string.IsNullOrEmpty(icon) ? Visibility.Collapsed : Visibility.Visible;
            _dongChu.Text = chu ?? "";
            HieuUng.DoiMau(_brDong, mau, 150);
            if (!string.IsNullOrEmpty(chu)) HieuUng.Hien(_dong, 0, -4, 180);
        }

        /// <summary>Báo lỗi dưới ô (null = xoá lỗi, trở lại gợi ý).</summary>
        public void DatLoi(string loi, bool rung = false)
        {
            _coLoi = !string.IsNullOrEmpty(loi);
            _capsHien = false;
            if (_coLoi) HienDong(IconDn.Loi, loi, MauDn.Loi);
            else HienDong(null, _goiY, MauDn.ChuPhu);
            CapNhatTrangThai(true);
            if (rung && _coLoi) HieuUng.Rung(_vong);
        }

        /// <summary>Thông báo tốt dưới ô (vd "Tên đăng nhập dùng được").</summary>
        public void DatOk(string chu)
        {
            if (_coLoi) return;
            if (string.IsNullOrEmpty(chu)) HienDong(null, _goiY, MauDn.ChuPhu);
            else HienDong(IconDn.DauTich, chu, MauDn.ThanhCong);
        }

        public string GoiY { set { _goiY = value ?? ""; if (!_coLoi && !_capsHien) HienDong(null, _goiY, MauDn.ChuPhu); } }
    }

    // =====================================================================================================
    /// <summary>Nút WPF tự vẽ (template tối giản) – vẫn là Button nên có bàn phím, UI Automation, Click.</summary>
    internal abstract class NutCoBan : Button
    {
        protected NutCoBan()
        {
            var tpl = new ControlTemplate(typeof(Button)) { VisualTree = new FrameworkElementFactory(typeof(ContentPresenter)) };
            Template = tpl;
            FocusVisualStyle = null;
            Cursor = Cursors.Hand;
            Background = Brushes.Transparent;
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            IsEnabledChanged += (s, e) => CapNhat(true);
        }

        protected abstract void CapNhat(bool coHieuUng);

        protected override void OnMouseEnter(MouseEventArgs e) { base.OnMouseEnter(e); CapNhat(true); }
        protected override void OnMouseLeave(MouseEventArgs e) { base.OnMouseLeave(e); CapNhat(true); }
        protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e) { base.OnIsPressedChanged(e); CapNhat(true); }
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e) { base.OnGotKeyboardFocus(e); CapNhat(true); }
        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e) { base.OnLostKeyboardFocus(e); CapNhat(true); }

        /// <summary>Focus do bàn phím (Tab) – chuột bấm thì không vẽ vòng focus.</summary>
        protected bool FocusBanPhim => IsKeyboardFocused && InputManager.Current.MostRecentInputDevice is KeyboardDevice;
    }

    /// <summary>Nút chính (cao 48) / nút viền: hover 150 ms, nhấn thu 0,98, đang chờ → spinner, xong → dấu tích.</summary>
    internal class NutDn : NutCoBan
    {
        private readonly bool _chinh;
        private readonly Border _vong, _nen;
        private readonly SolidColorBrush _brNen, _brVien, _brChu, _brVong;
        private readonly ScaleTransform _co = new ScaleTransform(1, 1);
        private readonly TextBlock _chu, _icon;
        private readonly Grid _quay;
        private readonly RotateTransform _xoay = new RotateTransform();
        private string _chuGoc;
        private bool _dangCho;

        public NutDn(string chu, bool chinh = true, string iconPhai = null)
        {
            _chinh = chinh;
            _chuGoc = chu;
            _brNen = MauDn.B(chinh ? MauDn.Nhan : Colors.Transparent);
            _brVien = MauDn.B(chinh ? MauDn.Nhan : MauDn.Vien);
            _brChu = MauDn.B(chinh ? MauDn.ChuTrenNhan : MauDn.Chu);
            _brVong = MauDn.B(Colors.Transparent);

            _chu = MauDn.Chu_(chu, 15, FontWeights.SemiBold, chinh ? MauDn.ChuTrenNhan : MauDn.Chu);
            _chu.Foreground = _brChu;
            _chu.TextWrapping = TextWrapping.NoWrap;
            _chu.VerticalAlignment = VerticalAlignment.Center;
            _icon = MauDn.Icon(iconPhai ?? "", 13, chinh ? MauDn.ChuTrenNhan : MauDn.Chu);
            _icon.Foreground = _brChu;
            _icon.Margin = new Thickness(10, 1, 0, 0);
            _icon.Visibility = string.IsNullOrEmpty(iconPhai) ? Visibility.Collapsed : Visibility.Visible;
            var hang = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            hang.Children.Add(_chu);
            hang.Children.Add(_icon);

            // vòng xoay chờ (cung 270°)
            _quay = new Grid { Width = 20, Height = 20, Visibility = Visibility.Collapsed, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            _quay.Children.Add(new Ellipse { Stroke = _brChu, StrokeThickness = 2.4, Opacity = 0.25 });
            _quay.Children.Add(new Path
            {
                Stroke = _brChu,
                StrokeThickness = 2.4,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Data = Geometry.Parse("M 10,1.2 A 8.8,8.8 0 0 1 18.8,10"),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = _xoay,
            });

            var noiDung = new Grid();
            noiDung.Children.Add(hang);
            noiDung.Children.Add(_quay);
            _nen = new Border
            {
                Height = 48,
                CornerRadius = new CornerRadius(10),
                Background = _brNen,
                BorderBrush = _brVien,
                BorderThickness = new Thickness(chinh ? 0 : 1.5),
                Child = noiDung,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = _co,
            };
            _vong = new Border { CornerRadius = new CornerRadius(13), BorderThickness = new Thickness(3), BorderBrush = _brVong, Margin = new Thickness(-3), Child = _nen };
            Content = _vong;
            AutomationProperties.SetName(this, chu);
        }

        public string Chu { get => _chuGoc; set { _chuGoc = value; if (!_dangCho) _chu.Text = value; AutomationProperties.SetName(this, value); } }

        /// <summary>Bật/tắt trạng thái chờ (spinner, khoá bấm).</summary>
        public bool DangCho
        {
            get => _dangCho;
            set
            {
                _dangCho = value;
                IsHitTestVisible = !value;
                _quay.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                ((FrameworkElement)_chu.Parent).Visibility = value ? Visibility.Hidden : Visibility.Visible;
                if (value)
                    _xoay.BeginAnimation(RotateTransform.AngleProperty,
                        new DoubleAnimation(0, 360, TimeSpan.FromMilliseconds(800)) { RepeatBehavior = RepeatBehavior.Forever });
                else _xoay.BeginAnimation(RotateTransform.AngleProperty, null);
            }
        }

        /// <summary>Hiện dấu tích ngắn (đăng nhập thành công).</summary>
        public void HienXong(string chu)
        {
            DangCho = false;
            _chu.Text = chu;
            _icon.Text = IconDn.DauTich;
            _icon.Visibility = Visibility.Visible;
            HieuUng.DoiMau(_brNen, MauDn.ThanhCong, 200);
            HieuUng.DoiMau(_brVien, MauDn.ThanhCong, 200);
            HieuUng.DoiMau(_brChu, Colors.White, 200);
        }

        protected override void CapNhat(bool coHieuUng)
        {
            if (_brNen == null) return;
            int ms = coHieuUng ? 150 : 0;
            bool tat = !IsEnabled;
            Opacity = tat ? 0.5 : 1;
            if (_chinh) HieuUng.DoiMau(_brNen, IsPressed ? MauDn.NhanNhan : IsMouseOver ? MauDn.NhanHover : MauDn.Nhan, ms);
            else
            {
                HieuUng.DoiMau(_brNen, IsPressed ? MauDn.W(Theme.Text, 0x1A) : IsMouseOver ? MauDn.W(Theme.Text, 0x0D) : Colors.Transparent, ms);
                HieuUng.DoiMau(_brVien, IsMouseOver ? MauDn.VienHover : MauDn.Vien, ms);
            }
            HieuUng.So(_co, ScaleTransform.ScaleXProperty, IsPressed ? 0.98 : 1, 100);
            HieuUng.So(_co, ScaleTransform.ScaleYProperty, IsPressed ? 0.98 : 1, 100);
            HieuUng.DoiMau(_brVong, FocusBanPhim ? MauDn.VongFocus : Colors.Transparent, ms);
        }
    }

    /// <summary>Nút chỉ có icon (hiện mật khẩu, quay lại) – vùng bấm 40×40.</summary>
    internal class NutIcon : NutCoBan
    {
        private readonly Border _nen;
        private readonly SolidColorBrush _brNen;
        private readonly TextBlock _icon;

        public NutIcon(string glyph, string moTa, double co = 16)
        {
            _brNen = MauDn.B(Colors.Transparent);
            _icon = MauDn.Icon(glyph, co, MauDn.ChuPhu);
            _nen = new Border { Width = 40, Height = 40, CornerRadius = new CornerRadius(8), Background = _brNen, Child = _icon };
            Content = _nen;
            VerticalAlignment = VerticalAlignment.Center;
            ToolTip = moTa;
            AutomationProperties.SetName(this, moTa);
            Focusable = true;
        }

        public string Glyph { set => _icon.Text = value; }

        protected override void CapNhat(bool coHieuUng)
        {
            if (_brNen == null) return;
            HieuUng.DoiMau(_brNen, IsPressed ? MauDn.W(Theme.Text, 0x22) : (IsMouseOver || FocusBanPhim) ? MauDn.W(Theme.Text, 0x12) : Colors.Transparent, coHieuUng ? 150 : 0);
        }
    }

    /// <summary>Liên kết chữ (gạch chân khi hover / focus bàn phím).</summary>
    internal class LienKet : NutCoBan
    {
        private readonly TextBlock _chu;
        private readonly SolidColorBrush _br;

        public LienKet(string chu, double co = 14, bool dam = true)
        {
            _br = MauDn.B(MauDn.Nhan);
            _chu = MauDn.Chu_(chu, co, dam ? FontWeights.SemiBold : FontWeights.Normal, MauDn.Nhan);
            _chu.Foreground = _br;
            _chu.TextWrapping = TextWrapping.NoWrap;
            Content = _chu;
            HorizontalAlignment = HorizontalAlignment.Left;
            Padding = new Thickness(0);
            AutomationProperties.SetName(this, chu);
        }

        protected override void CapNhat(bool coHieuUng)
        {
            if (_chu == null) return;
            _chu.TextDecorations = IsMouseOver || FocusBanPhim ? TextDecorations.Underline : null;
            HieuUng.DoiMau(_br, IsPressed ? MauDn.NhanNhan : MauDn.Nhan, coHieuUng ? 100 : 0);
            Opacity = IsEnabled ? 1 : 0.5;
        }
    }

    /// <summary>Hộp kiểm tự vẽ (ô bo góc 18 px, dấu tích hiện mờ dần).</summary>
    internal class HopKiem : CheckBox
    {
        private readonly Border _o;
        private readonly TextBlock _tich;
        private readonly SolidColorBrush _brNen, _brVien;

        public HopKiem(string chu)
        {
            Template = new ControlTemplate(typeof(CheckBox)) { VisualTree = new FrameworkElementFactory(typeof(ContentPresenter)) };
            FocusVisualStyle = null;
            Cursor = Cursors.Hand;
            _brNen = MauDn.B(Colors.Transparent);
            _brVien = MauDn.B(MauDn.Vien);
            _tich = MauDn.Icon(IconDn.DauTich, 11, MauDn.ChuTrenNhan);
            _tich.Opacity = 0;
            _o = new Border { Width = 18, Height = 18, CornerRadius = new CornerRadius(5), BorderThickness = new Thickness(1.5), BorderBrush = _brVien, Background = _brNen, Child = _tich };
            var hang = new StackPanel { Orientation = Orientation.Horizontal };
            hang.Children.Add(_o);
            var t = MauDn.Chu_(chu, 14, FontWeights.Normal, MauDn.Chu);
            t.Margin = new Thickness(10, 0, 0, 1);
            t.VerticalAlignment = VerticalAlignment.Center;
            hang.Children.Add(t);
            Content = hang;
            AutomationProperties.SetName(this, chu);
            Checked += (s, e) => CapNhat();
            Unchecked += (s, e) => CapNhat();
            MouseEnter += (s, e) => CapNhat();
            MouseLeave += (s, e) => CapNhat();
            GotKeyboardFocus += (s, e) => CapNhat();
            LostKeyboardFocus += (s, e) => CapNhat();
        }

        private void CapNhat()
        {
            bool bat = IsChecked == true;
            HieuUng.DoiMau(_brNen, bat ? MauDn.Nhan : Colors.Transparent);
            HieuUng.DoiMau(_brVien, bat ? MauDn.Nhan : (IsMouseOver || IsKeyboardFocused) ? MauDn.VienHover : MauDn.Vien);
            HieuUng.So(_tich, OpacityProperty, bat ? 1 : 0);
            _o.Effect = null;
        }
    }

    /// <summary>Thanh độ mạnh mật khẩu 4 nấc + nhãn.</summary>
    internal class DoManhMatKhau : StackPanel
    {
        private readonly SolidColorBrush[] _nac = new SolidColorBrush[4];
        private readonly TextBlock _nhan;

        public DoManhMatKhau()
        {
            var hang = new Grid { Height = 4, Margin = new Thickness(4, 0, 4, 0) };
            for (int i = 0; i < 4; i++)
            {
                hang.ColumnDefinitions.Add(new ColumnDefinition());
                _nac[i] = MauDn.B(MauDn.Vien);
                var r = new Border { CornerRadius = new CornerRadius(2), Background = _nac[i], Margin = new Thickness(i == 0 ? 0 : 3, 0, i == 3 ? 0 : 3, 0) };
                Grid.SetColumn(r, i);
                hang.Children.Add(r);
            }
            Children.Add(hang);
            _nhan = MauDn.Chu_("", 12.5, FontWeights.Normal, MauDn.ChuPhu);
            _nhan.Margin = new Thickness(4, 6, 0, 0);
            Children.Add(_nhan);
        }

        /// <summary>0 = trống, 1 yếu … 4 rất mạnh.</summary>
        public static int Cham(string mk)
        {
            if (string.IsNullOrEmpty(mk)) return 0;
            int d = 0;
            if (mk.Length >= 6) d++;
            if (mk.Length >= 10) d++;
            int loai = 0;
            bool thuong = false, hoa = false, so = false, ky = false;
            foreach (var c in mk) { if (char.IsLower(c)) thuong = true; else if (char.IsUpper(c)) hoa = true; else if (char.IsDigit(c)) so = true; else ky = true; }
            loai = (thuong ? 1 : 0) + (hoa ? 1 : 0) + (so ? 1 : 0) + (ky ? 1 : 0);
            if (loai >= 2) d++;
            if (loai >= 3) d++;
            if (mk.Length < 6) d = Math.Min(d, 1);
            return Math.Max(1, Math.Min(4, d));
        }

        public void DatMatKhau(string mk)
        {
            int d = Cham(mk);
            Color[] mau = { MauDn.Vien, MauDn.Loi, MauDn.CanhBao, Color.FromRgb(0x65, 0xA3, 0x0D), MauDn.ThanhCong };
            string[] chu = { "Tối thiểu 6 ký tự – nên có chữ hoa, số hoặc ký tự đặc biệt.", "Yếu", "Trung bình", "Khá mạnh", "Mạnh" };
            for (int i = 0; i < 4; i++) HieuUng.DoiMau(_nac[i], i < d ? mau[d] : MauDn.Vien, 200);
            _nhan.Text = d == 0 ? chu[0] : "Độ mạnh mật khẩu: " + chu[d];
        }
    }

    /// <summary>Thông báo trong form (lỗi kết nối, đăng ký xong…): khung màu nhạt + icon.</summary>
    internal class ThongBaoDn : Border
    {
        private readonly TextBlock _icon, _chu;

        public ThongBaoDn()
        {
            CornerRadius = new CornerRadius(10);
            Padding = new Thickness(14, 11, 14, 11);
            BorderThickness = new Thickness(1);
            Visibility = Visibility.Collapsed;
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition());
            _icon = MauDn.Icon("", 15, MauDn.Loi);
            _icon.VerticalAlignment = VerticalAlignment.Top;
            _icon.Margin = new Thickness(0, 2, 10, 0);
            _chu = MauDn.Chu_("", 13.5, FontWeights.Normal, MauDn.Chu);
            Grid.SetColumn(_chu, 1);
            g.Children.Add(_icon);
            g.Children.Add(_chu);
            Child = g;
            AutomationProperties.SetLiveSetting(this, AutomationLiveSetting.Assertive);
        }

        public enum Loai { Loi, ThanhCong, ThongTin }

        public void Hien(string chu, Loai loai)
        {
            if (string.IsNullOrEmpty(chu)) { An(); return; }
            var mau = loai == Loai.Loi ? MauDn.Loi : loai == Loai.ThanhCong ? MauDn.ThanhCong : MauDn.Nhan;
            _icon.Text = loai == Loai.Loi ? IconDn.Loi : loai == Loai.ThanhCong ? IconDn.DauTich : IconDn.ThongTin;
            _icon.Foreground = new SolidColorBrush(mau);
            _chu.Text = chu;
            Background = new SolidColorBrush(Color.FromArgb((byte)(Theme.Toi ? 0x26 : 0x14), mau.R, mau.G, mau.B));
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x55, mau.R, mau.G, mau.B));
            AutomationProperties.SetName(this, chu);
            HieuUng.Hien(this, 0, -6, 220);
        }

        public void An() { if (Visibility == Visibility.Visible) HieuUng.An(this, 0, -4, 160); }
    }

    /// <summary>Pill trạng thái kết nối CSDL (chấm màu + chữ).</summary>
    internal class PillTrangThai : Border
    {
        private readonly Ellipse _cham;
        private readonly TextBlock _chu;

        public PillTrangThai()
        {
            CornerRadius = new CornerRadius(14);
            Padding = new Thickness(10, 5, 12, 5);
            Background = new SolidColorBrush(MauDn.W(Theme.Text, 0x0B));
            HorizontalAlignment = HorizontalAlignment.Center;
            var h = new StackPanel { Orientation = Orientation.Horizontal };
            _cham = new Ellipse { Width = 8, Height = 8, Fill = new SolidColorBrush(MauDn.ChuPhu), VerticalAlignment = VerticalAlignment.Center };
            _chu = MauDn.Chu_("", 12.5, FontWeights.Normal, MauDn.ChuPhu);
            _chu.Margin = new Thickness(8, 0, 0, 1);
            _chu.TextWrapping = TextWrapping.NoWrap;
            h.Children.Add(_cham);
            h.Children.Add(_chu);
            Child = h;
        }

        public void Dat(string chu, Color? mau)
        {
            _chu.Text = chu;
            _cham.Fill = new SolidColorBrush(mau ?? MauDn.ChuPhu);
            AutomationProperties.SetName(this, chu);
        }
    }
}
