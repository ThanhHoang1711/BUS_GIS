using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace XeBus_GIS.UI.DangNhap
{
    /// <summary>
    /// Trang đăng nhập / đăng ký (toàn bộ là WPF dựng bằng code, đặt trong ElementHost của FrmDangNhap):
    ///  • trái (~60%): video nền lặp (Blender – tools/blender) + lớp phủ than chì + giới thiệu + số liệu thật đếm lên;
    ///  • phải: thẻ form – Đăng nhập ⇄ Đăng ký ⇄ Đã gửi đăng ký chuyển bằng trượt ngang + mờ dần (280 ms).
    /// Cửa sổ hẹp (&lt; 960 DIP) ⇒ ẩn cột video. WPF tự co giãn theo DPI; diễn hoạt chạy trên GPU.
    /// </summary>
    internal class TrangDangNhap : Grid
    {
        public readonly FormDangNhap DangNhap = new FormDangNhap();
        public readonly FormDangKy DangKy = new FormDangKy();
        public readonly ManThanhCong ThanhCong = new ManThanhCong();

        private readonly ColumnDefinition _cotTrai = new ColumnDefinition();
        private readonly ColumnDefinition _cotPhai = new ColumnDefinition { Width = new GridLength(520) };
        private readonly Grid _trai = new Grid { ClipToBounds = true };
        private readonly MediaElement _video = new MediaElement { Stretch = Stretch.UniformToFill, IsMuted = true, Opacity = 0 };
        private readonly Image _anhTinh = new Image { Stretch = Stretch.UniformToFill };
        private MediaClock _dongHo;
        private readonly Grid _noiDungTrai = new Grid();
        private readonly TextBlock _tieuDe, _moTa;
        private readonly TextBlock[] _so = new TextBlock[3];
        private readonly StackPanel _hangSo = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 36, 0, 0), Opacity = 0 };
        private readonly Grid _the = new Grid { MaxWidth = 400 };
        private readonly PillTrangThai _pill = new PillTrangThai();
        private UIElement _hienTai;
        private readonly UIElement[] _vaoTrai;

        public TrangDangNhap()
        {
            Background = new SolidColorBrush(MauDn.Nen);
            UseLayoutRounding = true;
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
            ColumnDefinitions.Add(_cotTrai);
            ColumnDefinitions.Add(_cotPhai);

            // ------------------------------------------------------------ cột trái: video + giới thiệu
            var phu = MauDn.Phu;
            _trai.Background = new SolidColorBrush(phu);
            _trai.Children.Add(_anhTinh);
            _trai.Children.Add(_video);
            // lớp phủ: đậm ở mép trái (chữ) → trong suốt ở 55% chiều rộng; thêm dải đáy + dải mảnh trên cùng cho logo
            _trai.Children.Add(new System.Windows.Shapes.Rectangle
            {
                Fill = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(200, phu.R, phu.G, phu.B), 0),
                    new GradientStop(Color.FromArgb(120, phu.R, phu.G, phu.B), 0.32),
                    new GradientStop(Color.FromArgb(0, phu.R, phu.G, phu.B), 0.62),
                }, 0),
                IsHitTestVisible = false,
            });
            _trai.Children.Add(new System.Windows.Shapes.Rectangle
            {
                Fill = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(120, phu.R, phu.G, phu.B), 0),
                    new GradientStop(Color.FromArgb(0, phu.R, phu.G, phu.B), 0.18),
                    new GradientStop(Color.FromArgb(0, phu.R, phu.G, phu.B), 0.50),
                    new GradientStop(Color.FromArgb(215, phu.R, phu.G, phu.B), 1),
                }, 90),
                IsHitTestVisible = false,
            });

            _noiDungTrai.Margin = new Thickness(56, 40, 56, 48);
            _noiDungTrai.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _noiDungTrai.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            _noiDungTrai.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _noiDungTrai.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var logo = new StackPanel { Orientation = Orientation.Horizontal, Opacity = 0 };
            logo.Children.Add(new Border
            {
                Width = 44, Height = 44, CornerRadius = new CornerRadius(12), Background = Brushes.White,
                Child = MauDn.Icon(Icons.Bus, 22, Color.FromRgb(0x11, 0x18, 0x27)),
            });
            var tenApp = new StackPanel { Margin = new Thickness(14, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            tenApp.Children.Add(MauDn.Chu_("XeBus GIS", 19, FontWeights.SemiBold, Colors.White, MauDn.ChuTieuDe));
            var phuDe = MauDn.Chu_("Hệ thống GIS quản lý trạm xe buýt · Hà Nội", 13, FontWeights.Normal, Color.FromArgb(0xD9, 0xFF, 0xFF, 0xFF));
            tenApp.Children.Add(phuDe);
            logo.Children.Add(tenApp);
            _noiDungTrai.Children.Add(logo);

            var gioiThieu = new StackPanel { MaxWidth = 620, HorizontalAlignment = HorizontalAlignment.Left };
            _tieuDe = MauDn.Chu_("Mạng lưới xe buýt Hà Nội,\nquản lý trên một bản đồ", 42, FontWeights.SemiBold, Colors.White, MauDn.ChuTieuDe);
            _tieuDe.LineHeight = 50;
            _tieuDe.Opacity = 0;
            gioiThieu.Children.Add(_tieuDe);
            _moTa = MauDn.Chu_("Quản lý trạm và tuyến, tìm khu dân cư chưa được phục vụ và đề xuất vị trí trạm mới từ dữ liệu dân số, công trình thu hút chuyến đi.",
                               17, FontWeights.Normal, Color.FromArgb(0xE6, 0xFF, 0xFF, 0xFF));
            _moTa.LineHeight = 27;
            _moTa.Margin = new Thickness(0, 18, 0, 0);
            _moTa.Opacity = 0;
            gioiThieu.Children.Add(_moTa);
            string[] nhanSo = { "trạm đang khai thác", "tuyến buýt", "phường / xã" };
            for (int i = 0; i < 3; i++)
            {
                var o = new StackPanel { Margin = new Thickness(0, 0, 44, 0) };
                _so[i] = MauDn.Chu_("–", 34, FontWeights.SemiBold, Colors.White, MauDn.ChuTieuDe);
                o.Children.Add(_so[i]);
                o.Children.Add(MauDn.Chu_(nhanSo[i], 13.5, FontWeights.Normal, Color.FromArgb(0xD0, 0xFF, 0xFF, 0xFF)));
                _hangSo.Children.Add(o);
            }
            gioiThieu.Children.Add(_hangSo);
            Grid.SetRow(gioiThieu, 2);
            _noiDungTrai.Children.Add(gioiThieu);

            _trai.Children.Add(_noiDungTrai);
            Children.Add(_trai);
            _vaoTrai = new UIElement[] { logo, _tieuDe, _moTa, _hangSo };

            // ------------------------------------------------------------ cột phải: thẻ form
            var phai = new Grid { Background = new SolidColorBrush(MauDn.Nen) };
            phai.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            phai.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var cuon = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Focusable = false };
            var khung = new Grid { Margin = new Thickness(48, 40, 48, 24) };
            _the.VerticalAlignment = VerticalAlignment.Center;
            foreach (var v in new FrameworkElement[] { DangNhap, DangKy, ThanhCong })
            {
                // mỗi màn căn giữa riêng ⇒ khi hai màn khác chiều cao chồng lên nhau lúc chuyển, màn cũ không bị nhảy
                v.VerticalAlignment = VerticalAlignment.Center;
                v.Visibility = Visibility.Collapsed;
                v.Opacity = 0;
                _the.Children.Add(v);
            }
            khung.Children.Add(_the);
            cuon.Content = khung;
            phai.Children.Add(cuon);

            var chan = new StackPanel { Margin = new Thickness(24, 0, 24, 22) };
            chan.Children.Add(_pill);
            var banQuyen = MauDn.Chu_("XeBus GIS · Hệ thống quản lý trạm xe buýt Hà Nội", 12, FontWeights.Normal, MauDn.ChuPhu);
            banQuyen.HorizontalAlignment = HorizontalAlignment.Center;
            banQuyen.Margin = new Thickness(0, 10, 0, 0);
            chan.Children.Add(banQuyen);
            Grid.SetRow(chan, 1);
            phai.Children.Add(chan);
            // đường viền mảnh giữa video và form
            phai.Children.Add(new Border { Width = 1, HorizontalAlignment = HorizontalAlignment.Left, Background = new SolidColorBrush(MauDn.W(Theme.Border)) });
            Grid.SetColumn(phai, 1);
            Children.Add(phai);

            // ------------------------------------------------------------ chuyển màn
            DangNhap.MoDangKy += () => { DangKy.BatDau(); ChuyenSang(DangKy, true); };
            DangKy.QuayLai += () => ChuyenSang(DangNhap, false);
            DangKy.DaGui += ten => { ThanhCong.Hien(ten); ChuyenSang(ThanhCong, true); };
            ThanhCong.VeDangNhap += ten =>
            {
                DangNhap.TenDangNhap = ten;
                DangNhap.XoaMatKhau();
                DangNhap.BaoThanhCong("Đã gửi đăng ký. Bạn đăng nhập được sau khi quản trị viên duyệt tài khoản.");
                ChuyenSang(DangNhap, false);
            };

            SizeChanged += (s, e) => BoCuc(e.NewSize);
            Loaded += (s, e) => VaoTrang();
        }

        // ------------------------------------------------------------------ bố cục theo bề rộng
        private void BoCuc(Size co)
        {
            bool hep = co.Width < 960;
            _trai.Visibility = hep ? Visibility.Collapsed : Visibility.Visible;
            _cotTrai.Width = hep ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
            // cột phải ≈ 40% (440–560 DIP); hẹp thì chiếm hết
            _cotPhai.Width = hep ? new GridLength(1, GridUnitType.Star) : new GridLength(Math.Max(440, Math.Min(560, co.Width * 0.4)));
            double rongTrai = co.Width - _cotPhai.Width.Value;
            bool vua = rongTrai < 760;
            _tieuDe.FontSize = vua ? 34 : 42;
            _tieuDe.LineHeight = vua ? 41 : 50;
            _moTa.FontSize = vua ? 15.5 : 17;
            _noiDungTrai.Margin = vua ? new Thickness(40, 32, 40, 36) : new Thickness(56, 40, 56, 48);
        }

        // ------------------------------------------------------------------ hiệu ứng vào trang
        private void VaoTrang()
        {
            for (int i = 0; i < _vaoTrai.Length; i++) HieuUng.Hien(_vaoTrai[i], 0, 16, 420, 150 + i * 80);
            DangNhap.Visibility = Visibility.Visible;
            _hienTai = DangNhap;
            HieuUng.Hien(DangNhap, 0, 24, 360, 220, () => DatFocus(DangNhap));
        }

        /// <summary>Chuyển giữa Đăng nhập / Đăng ký / Thành công (tiến = trượt từ phải sang).</summary>
        public void ChuyenSang(UIElement moi, bool tien)
        {
            if (_hienTai == moi) return;
            var cu = _hienTai;
            _hienTai = moi;
            if (cu != null)
            {
                cu.IsHitTestVisible = false;
                HieuUng.An(cu, tien ? -32 : 32, 0, 150, () => cu.IsHitTestVisible = true);
            }
            moi.IsHitTestVisible = true;
            HieuUng.Hien(moi, tien ? 40 : -40, 0, 280, cu != null ? 110 : 0, () => DatFocus(moi));
        }

        private void DatFocus(UIElement v)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                if (_hienTai != v) return;
                if (v is FormDangNhap a) a.DatFocusDau();
                else if (v is FormDangKy b) b.DatFocusDau();
                else if (v is ManThanhCong c) c.DatFocusDau();
            }));
        }

        // ------------------------------------------------------------------ dữ liệu
        public void DatSoLieu(long tram, long tuyen, long phuong)
        {
            long[] gt = { tram, tuyen, phuong };
            for (int i = 0; i < 3; i++) HieuUng.DemSo(_so[i], gt[i], 900, i * 120);
        }

        /// <summary>trangThai: 0 = đang kết nối, 1 = đã kết nối, 2 = lỗi.</summary>
        public void DatKetNoi(int trangThai, string chu)
        {
            _pill.Dat(chu, trangThai == 1 ? MauDn.ThanhCong : trangThai == 2 ? MauDn.Loi : (Color?)null);
            DangNhap.DatKetNoi(trangThai == 1);
        }

        /// <summary>Mờ toàn trang rồi gọi xong (đăng nhập thành công).</summary>
        public void Thoat(Action xong)
        {
            if (!HieuUng.ChoPhep) { xong(); return; }
            var a = new DoubleAnimation(0, TimeSpan.FromMilliseconds(260)) { BeginTime = TimeSpan.FromMilliseconds(260), EasingFunction = HieuUng.Vao };
            a.Completed += (s, e) => xong();
            BeginAnimation(OpacityProperty, a);
        }

        // ------------------------------------------------------------------ video nền
        /// <summary>Nạp video mp4 + ảnh tĩnh cùng tên (.jpg). Tắt hiệu ứng Windows ⇒ chỉ ảnh tĩnh.</summary>
        public void NapVideo(string duongDanVideo)
        {
            var anh = Path.ChangeExtension(duongDanVideo, ".jpg");
            if (File.Exists(anh))
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
                catch { /* không có ảnh thì nền than chì */ }
            }
            if (!File.Exists(duongDanVideo) || !HieuUng.ChoPhep) return;
            try
            {
                var tl = new MediaTimeline(new Uri(duongDanVideo, UriKind.Absolute)) { RepeatBehavior = RepeatBehavior.Forever };
                _dongHo = tl.CreateClock();
                _video.MediaOpened += (s, e) =>
                    _video.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(700)) { EasingFunction = HieuUng.Ra });
                _video.MediaFailed += (s, e) => _video.Visibility = Visibility.Collapsed;   // giữ ảnh tĩnh
                _video.Clock = _dongHo;
            }
            catch { _video.Visibility = Visibility.Collapsed; }
        }

        private bool _dangDung;

        /// <summary>Tạm dừng video khi cửa sổ thu nhỏ, phát tiếp khi hiện lại.</summary>
        public void TamDung(bool dung)
        {
            if (_dongHo == null || dung == _dangDung) return;
            _dangDung = dung;
            if (dung) _dongHo.Controller.Pause(); else _dongHo.Controller.Resume();
        }

        public void Dung()
        {
            try { _dongHo?.Controller.Stop(); } catch { }
            _video.Clock = null;
        }

        /// <summary>Thời điểm đang phát (kiểm tra video chạy).</summary>
        public TimeSpan? ThoiGianVideo => _dongHo?.CurrentTime;
    }
}
