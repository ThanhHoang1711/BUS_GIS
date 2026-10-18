using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace XeBus_GIS.UI.DangNhap
{
    /// <summary>Màn hình sau khi gửi đăng ký: vòng tròn + dấu tích vẽ dần, nhắc chờ quản trị viên duyệt.</summary>
    internal class ManThanhCong : StackPanel
    {
        public event Action<string> VeDangNhap;

        private readonly Ellipse _vong;
        private readonly Path _tich;
        private readonly TextBlock _moTa;
        private readonly NutDn _nut;
        private string _ten;

        public ManThanhCong()
        {
            var mau = MauDn.ThanhCong;
            var bieuTuong = new Grid { Width = 76, Height = 76, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 28) };
            bieuTuong.Children.Add(new Ellipse { Fill = new SolidColorBrush(Color.FromArgb((byte)(Theme.Toi ? 0x30 : 0x1C), mau.R, mau.G, mau.B)) });
            _vong = new Ellipse
            {
                Stroke = new SolidColorBrush(mau),
                StrokeThickness = 3,
                StrokeDashArray = new DoubleCollection { 80, 80 },   // chu vi ≈ π·(76−3)/3 (đơn vị = độ dày nét)
                StrokeDashCap = PenLineCap.Round,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(-90),
            };
            bieuTuong.Children.Add(_vong);
            _tich = new Path
            {
                Data = Geometry.Parse("M 24,39 L 34,49 L 53,29"),
                Stroke = new SolidColorBrush(mau),
                StrokeThickness = 4,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeDashArray = new DoubleCollection { 12, 12 },   // độ dài nét tích ≈ 41 px / 4
            };
            bieuTuong.Children.Add(_tich);
            Children.Add(bieuTuong);

            Children.Add(MauDn.Chu_("Đã gửi yêu cầu đăng ký", 28, FontWeights.SemiBold, MauDn.Chu, MauDn.ChuTieuDe));
            _moTa = MauDn.Chu_("", 15, FontWeights.Normal, MauDn.ChuPhu);
            _moTa.Margin = new Thickness(0, 10, 0, 0);
            _moTa.LineHeight = 23;
            Children.Add(_moTa);

            var buoc = new StackPanel { Margin = new Thickness(0, 24, 0, 28) };
            buoc.Children.Add(DongBuoc("1", "Quản trị viên xem và duyệt tài khoản của bạn."));
            buoc.Children.Add(DongBuoc("2", "Sau khi được duyệt, đăng nhập bằng tên và mật khẩu vừa tạo."));
            Children.Add(buoc);

            _nut = new NutDn("Về trang đăng nhập", true, IconDn.MuiTenPhai);
            _nut.Click += (s, e) => VeDangNhap?.Invoke(_ten);
            Children.Add(_nut);
        }

        private static UIElement DongBuoc(string so, string chu)
        {
            var g = new Grid { Margin = new Thickness(0, 0, 0, 12) };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
            g.ColumnDefinitions.Add(new ColumnDefinition());
            var o = new Border
            {
                Width = 24, Height = 24, CornerRadius = new CornerRadius(12), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(MauDn.W(Theme.Text, 0x10)),
                Child = MauDn.Chu_(so, 12.5, FontWeights.SemiBold, MauDn.Chu),
            };
            ((TextBlock)o.Child).HorizontalAlignment = HorizontalAlignment.Center;
            ((TextBlock)o.Child).VerticalAlignment = VerticalAlignment.Center;
            g.Children.Add(o);
            var t = MauDn.Chu_(chu, 14.5, FontWeights.Normal, MauDn.Chu);
            t.Margin = new Thickness(0, 2, 0, 0);
            Grid.SetColumn(t, 1);
            g.Children.Add(t);
            return g;
        }

        /// <summary>Hiện cho tài khoản vừa đăng ký và chạy hiệu ứng vẽ dấu tích.</summary>
        public void Hien(string ten)
        {
            _ten = ten;
            _moTa.Inlines.Clear();
            _moTa.Inlines.Add(new Run("Tài khoản "));
            _moTa.Inlines.Add(new Bold(new Run(ten)) { Foreground = new SolidColorBrush(MauDn.Chu) });
            _moTa.Inlines.Add(new Run(" đang chờ quản trị viên duyệt. Bạn chưa đăng nhập được cho tới khi tài khoản được duyệt."));

            if (!HieuUng.ChoPhep) { _vong.StrokeDashOffset = 0; _tich.StrokeDashOffset = 0; return; }
            _vong.BeginAnimation(Shape.StrokeDashOffsetProperty,
                new DoubleAnimation(80, 0, TimeSpan.FromMilliseconds(520)) { EasingFunction = HieuUng.Ra, BeginTime = TimeSpan.FromMilliseconds(120) });
            _tich.StrokeDashOffset = 12;
            _tich.BeginAnimation(Shape.StrokeDashOffsetProperty,
                new DoubleAnimation(12, 0, TimeSpan.FromMilliseconds(320)) { EasingFunction = HieuUng.Ra, BeginTime = TimeSpan.FromMilliseconds(560) });
        }

        public void DatFocusDau() => _nut.Focus();
    }
}
