using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace XeBus_GIS.UI.Controls
{
    /// <summary>
    /// Một tiêu chí + thanh kéo mức quan trọng (0–10): ô màu có icon · tên · tỷ trọng (%) · thanh kéo · mô tả ngắn.
    /// Kéo chuột, click trên thanh hoặc phím ←/→ (Home/End) để đổi. Tỷ trọng % do nơi dùng tính và gán (TyTrong).
    /// </summary>
    public class ThanhTrongSo : Control
    {
        private int _giaTri = 5;
        private bool _keo;
        private bool _hover;
        private string _tyTrong = "";

        public event Action<ThanhTrongSo> GiaTriDoi;

        public string TenTieuChi { get; set; } = "Tiêu chí";
        public string MoTa { get; set; } = "";
        public string Icon { get; set; } = Icons.Thong_Tin;
        public Color Mau { get; set; } = Theme.Accent;
        public int ToiDa { get; set; } = 10;

        public ThanhTrongSo()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint | ControlStyles.Selectable, true);
            BackColor = Theme.Surface;
            Height = UiHelper.Dp(74);
            TabStop = true;
            Cursor = Cursors.Hand;
        }

        public int GiaTri
        {
            get => _giaTri;
            set
            {
                int v = Math.Max(0, Math.Min(ToiDa, value));
                if (v == _giaTri) return;
                _giaTri = v;
                Invalidate();
                GiaTriDoi?.Invoke(this);
            }
        }

        /// <summary>Chữ hiển thị bên phải tên (vd "35%").</summary>
        public string TyTrong
        {
            get => _tyTrong;
            set { _tyTrong = value ?? ""; Invalidate(); }
        }

        private int Le => UiHelper.Dp(32);
        private Rectangle VungThanh => new Rectangle(Le + UiHelper.Dp(7), UiHelper.Dp(28), Math.Max(10, Width - Le - UiHelper.Dp(14)), UiHelper.Dp(14));

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int s = UiHelper.Dp(24);

            // ô màu + icon
            var o = new Rectangle(0, UiHelper.Dp(2), s, s);
            using (var path = UiHelper.DuongBoGoc(o, UiHelper.Dp(6)))
            using (var br = new SolidBrush(Enabled ? Mau : Theme.Disabled))
                g.FillPath(br, path);
            using (var f = Theme.FontIcon(10f))
                TextRenderer.DrawText(g, Icon, f, o, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            // tên + tỷ trọng
            var wTy = TextRenderer.MeasureText(_tyTrong, Theme.FontDam).Width;
            TextRenderer.DrawText(g, TenTieuChi, Theme.FontDam, new Rectangle(Le, 0, Width - Le - wTy - UiHelper.Dp(4), UiHelper.Dp(28)),
                Theme.Text, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, _tyTrong, Theme.FontDam, new Rectangle(Width - wTy - 2, 0, wTy + 2, UiHelper.Dp(28)),
                _giaTri == 0 ? Theme.Disabled : Mau, TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.NoPrefix);

            // thanh kéo
            var t = VungThanh;
            int yTam = t.Y + t.Height / 2, h = UiHelper.Dp(4);
            var nen = new RectangleF(t.X, yTam - h / 2f, t.Width, h);
            using (var path = UiHelper.DuongBoGoc(nen, h / 2f))
            using (var br = new SolidBrush(Theme.Border))
                g.FillPath(br, path);
            float x = t.X + t.Width * _giaTri / (float)ToiDa;
            if (_giaTri > 0)
            {
                var day = new RectangleF(t.X, yTam - h / 2f, x - t.X, h);
                using (var path = UiHelper.DuongBoGoc(day, h / 2f))
                using (var br = new SolidBrush(Enabled ? Mau : Theme.Disabled))
                    g.FillPath(br, path);
            }
            // vạch chia
            using (var br = new SolidBrush(Theme.BorderStrong))
                for (int i = 1; i < ToiDa; i++)
                {
                    float xi = t.X + t.Width * i / (float)ToiDa;
                    if (Math.Abs(xi - x) > UiHelper.Dp(6)) g.FillEllipse(br, xi - 1.5f, yTam - 1.5f, 3, 3);
                }
            int d = UiHelper.Dp(_hover || _keo || Focused ? 16 : 14);
            var num = new RectangleF(x - d / 2f, yTam - d / 2f, d, d);
            using (var br = new SolidBrush(Color.White)) g.FillEllipse(br, num);
            using (var pen = new Pen(Enabled ? Mau : Theme.Disabled, UiHelper.Dp(2))) g.DrawEllipse(pen, num);
            if (Focused && ShowFocusCues)
                using (var pen = new Pen(Color.FromArgb(70, Mau), UiHelper.Dp(3)))
                    g.DrawEllipse(pen, RectangleF.Inflate(num, UiHelper.Dp(3), UiHelper.Dp(3)));

            // mô tả
            TextRenderer.DrawText(g, MoTa, Theme.FontNho, new Rectangle(Le, t.Bottom + UiHelper.Dp(1), Width - Le, Height - t.Bottom - UiHelper.Dp(1)),
                Theme.Muted, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private void DatTheoChuot(int x)
        {
            var t = VungThanh;
            GiaTri = (int)Math.Round((x - t.X) * ToiDa / (double)t.Width);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.Button != MouseButtons.Left) return;
            var t = VungThanh;
            if (e.Y < t.Y - UiHelper.Dp(6) || e.Y > t.Bottom + UiHelper.Dp(6)) return;
            _keo = true;
            Capture = true;
            DatTheoChuot(e.X);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_keo) DatTheoChuot(e.X);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _keo = false;
            Capture = false;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hover = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hover = false; Invalidate(); }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override bool IsInputKey(Keys keyData) =>
            keyData == Keys.Left || keyData == Keys.Right || base.IsInputKey(keyData);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Left: case Keys.Down: GiaTri--; e.Handled = true; break;
                case Keys.Right: case Keys.Up: GiaTri++; e.Handled = true; break;
                case Keys.Home: GiaTri = 0; e.Handled = true; break;
                case Keys.End: GiaTri = ToiDa; e.Handled = true; break;
            }
        }
    }
}
