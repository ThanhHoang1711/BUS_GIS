using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WfColor = System.Drawing.Color;

namespace XeBus_GIS.UI.Controls
{
    /// <summary>
    /// Vùng hình nền động của trang đăng nhập: phát lặp video MP4 (H.264, render bằng Blender từ dữ liệu CSDLKG –
    /// tools/blender/dung_canh.py → dong_goi_video.ps1) bằng WPF MediaElement (giải mã/phóng to bằng GPU ⇒ mượt, nét
    /// ở mọi tỉ lệ DPI), lớp phủ than chì và phần giới thiệu vẽ cùng trong WPF nên không bị "đè" như ActiveX.
    /// Ảnh tĩnh (cùng tên, đuôi .jpg) hiện trong lúc mở video, khi Windows không phát được (thiếu Media Feature Pack)
    /// hoặc khi người dùng tắt hiệu ứng của Windows. Tạm dừng khi cửa sổ thu nhỏ.
    /// </summary>
    public class NenVideo : System.Windows.Forms.Control
    {
        private readonly ElementHost _host;
        private readonly Grid _goc = new Grid { ClipToBounds = true };
        private readonly MediaElement _video = new MediaElement { Stretch = Stretch.UniformToFill, IsMuted = true, Opacity = 0 };
        private readonly Image _anhTinh = new Image { Stretch = Stretch.UniformToFill };
        private MediaClock _dongHo;

        private readonly TextBlock _tbTieuDe = Chu(19, FontWeights.SemiBold);
        private readonly TextBlock _tbSlogan = Chu(36, FontWeights.SemiBold);
        private readonly TextBlock _tbMoTa = Chu(15.5, FontWeights.Normal, 0.85);
        private readonly StackPanel _spTinhNang = new StackPanel { Margin = new Thickness(0, 26, 0, 0) };
        private readonly Border _bdSoLieu = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromArgb(70, 255, 255, 255)),
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(0, 14, 0, 0),
            Margin = new Thickness(0, 10, 0, 0),
            Visibility = Visibility.Collapsed,
        };
        private readonly TextBlock _tbSoLieu = Chu(15, FontWeights.SemiBold);
        private readonly TextBlock _tbNguon = Chu(11.5, FontWeights.Normal, 0.6);

        public NenVideo()
        {
            BackColor = WfColor.FromArgb(17, 24, 39);
            _host = new ElementHost { Dock = System.Windows.Forms.DockStyle.Fill, BackColorTransparent = false };
            _host.Child = DungNoiDung();
            Controls.Add(_host);
        }

        // ------------------------------------------------------------------ thuộc tính nội dung
        public string TieuDe { get => _tbTieuDe.Text; set => _tbTieuDe.Text = value; }
        public string Slogan { get => _tbSlogan.Text; set => _tbSlogan.Text = value; }
        public string MoTa { get => _tbMoTa.Text; set => _tbMoTa.Text = value; }
        public string ChuThichNguon { get => _tbNguon.Text; set => _tbNguon.Text = value; }

        /// <summary>Số liệu thật (vd "1.258 trạm · 6 tuyến · 126 phường/xã"); rỗng = ẩn dòng này.</summary>
        public string SoLieu
        {
            get => _tbSoLieu.Text;
            set
            {
                _tbSoLieu.Text = value ?? "";
                _bdSoLieu.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private (string icon, string ten, string moTa)[] _tinhNang = new (string, string, string)[0];
        public (string icon, string ten, string moTa)[] TinhNang
        {
            get => _tinhNang;
            set
            {
                _tinhNang = value ?? new (string, string, string)[0];
                _spTinhNang.Children.Clear();
                foreach (var (icon, ten, moTa) in _tinhNang) _spTinhNang.Children.Add(DongTinhNang(icon, ten, moTa));
            }
        }

        // ------------------------------------------------------------------ dựng giao diện WPF
        private static TextBlock Chu(double co, FontWeight dam, double doDuc = 1)
        {
            var t = new TextBlock
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = co,
                FontWeight = dam,
                Foreground = Brushes.White,
                Opacity = doDuc,
                TextWrapping = TextWrapping.Wrap,
            };
            TextOptions.SetTextFormattingMode(t, TextFormattingMode.Display);
            return t;
        }

        private static FontFamily FontIcon => new FontFamily(Theme.TenFontIcon);

        private static Color Wpf(WfColor c, byte? a = null) => Color.FromArgb(a ?? c.A, c.R, c.G, c.B);

        private UIElement DungNoiDung()
        {
            var phu = Wpf(Theme.DnPhu, 255);
            _goc.Background = new SolidColorBrush(phu);

            // 1. ảnh tĩnh + video (phủ kín, căn giữa)
            _goc.Children.Add(_anhTinh);
            _goc.Children.Add(_video);

            // 2. lớp phủ than chì: đậm ở mép trái + đáy (nơi đặt chữ), nhạt dần sang phải; dải mờ ở trên cho logo
            _goc.Children.Add(new System.Windows.Shapes.Rectangle
            {
                Fill = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(205, phu.R, phu.G, phu.B), 0),
                    new GradientStop(Color.FromArgb(120, phu.R, phu.G, phu.B), 0.45),
                    new GradientStop(Color.FromArgb(0, phu.R, phu.G, phu.B), 0.85),
                }, 0),
            });
            _goc.Children.Add(new System.Windows.Shapes.Rectangle
            {
                Fill = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(110, phu.R, phu.G, phu.B), 0),
                    new GradientStop(Color.FromArgb(0, phu.R, phu.G, phu.B), 0.16),
                    new GradientStop(Color.FromArgb(0, phu.R, phu.G, phu.B), 0.45),
                    new GradientStop(Color.FromArgb(190, phu.R, phu.G, phu.B), 1),
                }, 90),
            });

            // 3. nội dung
            var noiDung = new Grid { Margin = new Thickness(56, 36, 56, 28) };
            noiDung.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            noiDung.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            noiDung.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            noiDung.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // logo + tên
            var hangLogo = new StackPanel { Orientation = Orientation.Horizontal };
            var logo = new Border
            {
                Width = 42, Height = 42, CornerRadius = new CornerRadius(10), Background = Brushes.White,
                Child = new TextBlock
                {
                    Text = Icons.Bus, FontFamily = FontIcon, FontSize = 21,
                    Foreground = new SolidColorBrush(Wpf(Theme.DnNhan)),
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                },
            };
            hangLogo.Children.Add(logo);
            _tbTieuDe.Margin = new Thickness(12, 0, 0, 0);
            _tbTieuDe.VerticalAlignment = VerticalAlignment.Center;
            hangLogo.Children.Add(_tbTieuDe);
            Grid.SetRow(hangLogo, 0);
            noiDung.Children.Add(hangLogo);

            // khối giới thiệu (đáy trái, rộng tối đa 640)
            var gioiThieu = new StackPanel { MaxWidth = 640, HorizontalAlignment = HorizontalAlignment.Left };
            _tbSlogan.LineHeight = 44;
            gioiThieu.Children.Add(_tbSlogan);
            _tbMoTa.Margin = new Thickness(0, 14, 0, 0);
            _tbMoTa.LineHeight = 23;
            gioiThieu.Children.Add(_tbMoTa);
            gioiThieu.Children.Add(_spTinhNang);
            _bdSoLieu.Child = _tbSoLieu;
            gioiThieu.Children.Add(_bdSoLieu);
            Grid.SetRow(gioiThieu, 2);
            noiDung.Children.Add(gioiThieu);

            _tbNguon.Margin = new Thickness(0, 26, 0, 0);
            _tbNguon.TextTrimming = TextTrimming.CharacterEllipsis;
            _tbNguon.TextWrapping = TextWrapping.NoWrap;
            Grid.SetRow(_tbNguon, 3);
            noiDung.Children.Add(_tbNguon);
            _goc.Children.Add(noiDung);

            // cỡ chữ tiêu đề theo bề rộng vùng
            _goc.SizeChanged += (s, e) =>
            {
                bool hep = e.NewSize.Width < 820;
                _tbSlogan.FontSize = hep ? 28 : 36;
                _tbSlogan.LineHeight = hep ? 35 : 44;
                noiDung.Margin = hep ? new Thickness(36, 28, 36, 22) : new Thickness(56, 36, 56, 28);
            };
            return _goc;
        }

        private static UIElement DongTinhNang(string icon, string ten, string moTa)
        {
            var hang = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            hang.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(52) });
            hang.ColumnDefinitions.Add(new ColumnDefinition());
            var o = new Border
            {
                Width = 38, Height = 38, CornerRadius = new CornerRadius(9), HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(70, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = icon, FontFamily = FontIcon, FontSize = 17, Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                },
            };
            hang.Children.Add(o);
            var chu = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            chu.Children.Add(Chu(14.5, FontWeights.SemiBold));
            ((TextBlock)chu.Children[0]).Text = ten;
            var tbMoTa = Chu(12.5, FontWeights.Normal, 0.8);
            tbMoTa.Text = moTa;
            tbMoTa.Margin = new Thickness(0, 2, 0, 0);
            chu.Children.Add(tbMoTa);
            Grid.SetColumn(chu, 1);
            hang.Children.Add(chu);
            return hang;
        }

        // ------------------------------------------------------------------ phát video
        /// <summary>
        /// Nạp video (mp4) và ảnh tĩnh cùng tên (.jpg). Trả false nếu thiếu cả hai (khi đó chỉ có nền than chì).
        /// </summary>
        public bool Nap(string duongDanVideo)
        {
            var anh = Path.ChangeExtension(duongDanVideo, ".jpg");
            bool coAnh = File.Exists(anh);
            if (coAnh)
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(anh, UriKind.Absolute);
                    bmp.EndInit();
                    bmp.Freeze();
                    _anhTinh.Source = bmp;
                }
                catch { coAnh = false; }
            }

            if (!File.Exists(duongDanVideo)) return coAnh;
            // người dùng tắt hiệu ứng của Windows ⇒ chỉ ảnh tĩnh
            if (!System.Windows.Forms.SystemInformation.UIEffectsEnabled) return true;

            try
            {
                var tl = new MediaTimeline(new Uri(duongDanVideo, UriKind.Absolute)) { RepeatBehavior = RepeatBehavior.Forever };
                _dongHo = tl.CreateClock();
                _video.MediaOpened += (s, e) =>
                    _video.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(400)));
                _video.MediaFailed += (s, e) => _video.Visibility = Visibility.Collapsed; // giữ ảnh tĩnh
                _video.Clock = _dongHo;
                CapNhatPhat();
                return true;
            }
            catch
            {
                _video.Visibility = Visibility.Collapsed;
                return coAnh;
            }
        }

        private bool _dangDung;

        /// <summary>Tạm dừng khi cửa sổ thu nhỏ / control ẩn, phát tiếp khi hiện lại.</summary>
        private void CapNhatPhat()
        {
            if (_dongHo == null) return;
            var f = FindForm();
            bool phat = Visible && (f == null || f.WindowState != System.Windows.Forms.FormWindowState.Minimized);
            if (!phat && !_dangDung) { _dongHo.Controller.Pause(); _dangDung = true; }
            else if (phat && _dangDung) { _dongHo.Controller.Resume(); _dangDung = false; }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var f = FindForm();
            if (f != null) f.Resize += (s, ev) => CapNhatPhat();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            CapNhatPhat();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { _dongHo?.Controller.Stop(); } catch { }
                _video.Clock = null;
                _host.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
