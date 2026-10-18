using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace XeBus_GIS.UI.DangNhap
{
    /// <summary>
    /// Chuyển động dùng chung cho trang đăng nhập (WPF – diễn hoạt bằng GPU trên Opacity/Transform).
    /// Vào: ease-out; ra: ease-in; vi tương tác 150 ms, chuyển trang 280–320 ms. Người dùng tắt hiệu ứng của Windows
    /// ⇒ không trượt / rung / đếm số, chỉ đổi trạng thái tức thì hoặc mờ dần rất ngắn.
    /// </summary>
    internal static class HieuUng
    {
        /// <summary>Có được dùng chuyển động không (Windows: "Animation effects" + hiệu ứng giao diện).</summary>
        public static bool ChoPhep
        {
            get
            {
                if (Environment.GetEnvironmentVariable("XEBUS_KHONG_HIEU_UNG") == "1") return false;
                try { return SystemParameters.ClientAreaAnimation && System.Windows.Forms.SystemInformation.UIEffectsEnabled; }
                catch { return true; }
            }
        }

        public static readonly IEasingFunction Ra = new CubicEase { EasingMode = EasingMode.EaseOut };     // phần tử đi vào
        public static readonly IEasingFunction Vao = new CubicEase { EasingMode = EasingMode.EaseIn };     // phần tử đi ra
        public static readonly IEasingFunction Muot = new CubicEase { EasingMode = EasingMode.EaseInOut };

        private static TranslateTransform Dich(UIElement e)
        {
            if (e.RenderTransform is TranslateTransform t && !t.IsFrozen) return t;
            if (e.RenderTransform is TransformGroup g)
                foreach (var x in g.Children) if (x is TranslateTransform tt && !tt.IsFrozen) return tt;
            var moi = new TranslateTransform();
            e.RenderTransform = moi;
            return moi;
        }

        private static DoubleAnimation Anim(double? tu, double den, int ms, IEasingFunction ease, int treMs = 0) =>
            new DoubleAnimation
            {
                From = tu,
                To = den,
                Duration = TimeSpan.FromMilliseconds(ms),
                BeginTime = TimeSpan.FromMilliseconds(treMs),
                EasingFunction = ease,
                FillBehavior = FillBehavior.HoldEnd,
            };

        /// <summary>Hiện phần tử: mờ → rõ + trượt từ (dx, dy) về 0.</summary>
        public static void Hien(UIElement e, double dx = 0, double dy = 16, int ms = 320, int treMs = 0, Action xong = null)
        {
            e.Visibility = Visibility.Visible;
            var t = Dich(e);
            if (!ChoPhep)
            {
                e.BeginAnimation(UIElement.OpacityProperty, null);
                t.BeginAnimation(TranslateTransform.XProperty, null);
                t.BeginAnimation(TranslateTransform.YProperty, null);
                e.Opacity = 1; t.X = 0; t.Y = 0;
                xong?.Invoke();
                return;
            }
            var a = Anim(0, 1, ms, Ra, treMs);
            if (xong != null) a.Completed += (s, ev) => xong();
            e.BeginAnimation(UIElement.OpacityProperty, a);
            t.BeginAnimation(TranslateTransform.XProperty, Anim(dx, 0, ms, Ra, treMs));
            t.BeginAnimation(TranslateTransform.YProperty, Anim(dy, 0, ms, Ra, treMs));
        }

        /// <summary>Ẩn phần tử: rõ → mờ + trượt tới (dx, dy); xong thì Collapsed.</summary>
        public static void An(UIElement e, double dx = 0, double dy = 0, int ms = 220, Action xong = null)
        {
            var t = Dich(e);
            if (!ChoPhep)
            {
                e.BeginAnimation(UIElement.OpacityProperty, null);
                e.Opacity = 0;
                e.Visibility = Visibility.Collapsed;
                xong?.Invoke();
                return;
            }
            var a = Anim(null, 0, ms, Vao);
            a.Completed += (s, ev) =>
            {
                // chỉ thu gọn nếu không có hiệu ứng "hiện" nào chen vào trong lúc ẩn
                if (e.Opacity <= 0.01) e.Visibility = Visibility.Collapsed;
                xong?.Invoke();
            };
            e.BeginAnimation(UIElement.OpacityProperty, a);
            t.BeginAnimation(TranslateTransform.XProperty, Anim(null, dx, ms, Vao));
            t.BeginAnimation(TranslateTransform.YProperty, Anim(null, dy, ms, Vao));
        }

        /// <summary>Rung ngang nhẹ (báo nhập sai) – 300 ms.</summary>
        public static void Rung(UIElement e)
        {
            if (!ChoPhep) return;
            var t = Dich(e);
            var k = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(320) };
            double[] x = { 0, -7, 6, -4, 3, -1, 0 };
            for (int i = 0; i < x.Length; i++)
                k.KeyFrames.Add(new EasingDoubleKeyFrame(x[i], KeyTime.FromPercent(i / (double)(x.Length - 1))));
            t.BeginAnimation(TranslateTransform.XProperty, k);
        }

        /// <summary>Đổi màu brush (brush không được Freeze).</summary>
        public static void DoiMau(SolidColorBrush b, Color den, int ms = 150)
        {
            if (b == null || b.IsFrozen) return;
            if (!ChoPhep || ms <= 0) { b.BeginAnimation(SolidColorBrush.ColorProperty, null); b.Color = den; return; }
            b.BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation(den, TimeSpan.FromMilliseconds(ms)) { EasingFunction = Ra, FillBehavior = FillBehavior.HoldEnd });
        }

        /// <summary>Animate một DependencyProperty kiểu double (vd Width, Height, ScaleX).</summary>
        public static void So(IAnimatable dt, DependencyProperty p, double den, int ms = 150, IEasingFunction ease = null)
        {
            if (!ChoPhep || ms <= 0)
            {
                dt.BeginAnimation(p, null);
                if (dt is DependencyObject d) d.SetValue(p, den);
                return;
            }
            dt.BeginAnimation(p, new DoubleAnimation(den, TimeSpan.FromMilliseconds(ms)) { EasingFunction = ease ?? Ra, FillBehavior = FillBehavior.HoldEnd });
        }

        /// <summary>Đếm số từ 0 lên giá trị thật (cập nhật theo nhịp vẽ màn hình, ease-out).</summary>
        public static void DemSo(System.Windows.Controls.TextBlock tb, long den, int ms = 900, int treMs = 0, string dinhDang = "N0")
        {
            var vn = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
            if (!ChoPhep) { tb.Text = den.ToString(dinhDang, vn); return; }
            tb.Text = 0L.ToString(dinhDang, vn);
            var sw = Stopwatch.StartNew();
            EventHandler buoc = null;
            buoc = (s, e) =>
            {
                double t = (sw.ElapsedMilliseconds - treMs) / (double)ms;
                if (t < 0) return;
                if (t >= 1)
                {
                    tb.Text = den.ToString(dinhDang, vn);
                    CompositionTarget.Rendering -= buoc;
                    return;
                }
                double k = 1 - Math.Pow(1 - t, 3);   // ease-out bậc 3
                tb.Text = ((long)Math.Round(den * k)).ToString(dinhDang, vn);
            };
            CompositionTarget.Rendering += buoc;
        }
    }
}
