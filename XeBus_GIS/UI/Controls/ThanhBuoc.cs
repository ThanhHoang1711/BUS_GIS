using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace XeBus_GIS.UI.Controls
{
    /// <summary>
    /// Thanh bước (stepper) cho quy trình nhiều bước: bước đã xong có dấu tích xanh, bước hiện tại màu Accent,
    /// bước chưa tới màu xám. Click vào bước đã xong / đang mở để quay lại (nếu <see cref="ChoPhepQuayLai"/>).
    /// </summary>
    public class ThanhBuoc : Control
    {
        private string[] _buoc = { "Bước 1", "Bước 2", "Bước 3" };
        private int _hienTai;      // 0-based
        private int _xaNhat;       // bước xa nhất đã mở được
        private int _hover = -1;
        private static readonly Font FontBuocMo = new Font("Segoe UI Semibold", 8.25f);

        public event Action<int> BuocDuocChon;

        public bool ChoPhepQuayLai { get; set; } = true;

        public ThanhBuoc()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Theme.Surface;
            Height = UiHelper.Dp(56);
        }

        public string[] CacBuoc
        {
            get => _buoc;
            set { _buoc = value ?? new string[0]; Invalidate(); }
        }

        /// <summary>Bước hiện tại (0-based).</summary>
        public int HienTai
        {
            get => _hienTai;
            set
            {
                _hienTai = Math.Max(0, Math.Min(_buoc.Length - 1, value));
                _xaNhat = Math.Max(_xaNhat, _hienTai);
                Invalidate();
            }
        }

        /// <summary>Đặt lại tiến độ (vd khi đổi vùng phân tích – các bước sau không còn hợp lệ).</summary>
        public void DatLaiTienDo(int xaNhat)
        {
            _xaNhat = Math.Max(_hienTai, xaNhat);
            Invalidate();
        }

        private Rectangle VungBuoc(int i)
        {
            int n = Math.Max(1, _buoc.Length);
            int w = Width / n;
            return new Rectangle(i * w, 0, w, Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int d = UiHelper.Dp(24), yTam = UiHelper.Dp(18);

            for (int i = 0; i < _buoc.Length; i++)
            {
                var r = VungBuoc(i);
                int cx = r.X + r.Width / 2;

                // đường nối với bước sau
                if (i < _buoc.Length - 1)
                {
                    var r2 = VungBuoc(i + 1);
                    using (var pen = new Pen(i < _xaNhat ? Theme.Accent : Theme.Border, 2))
                        g.DrawLine(pen, cx + d / 2 + UiHelper.Dp(4), yTam, r2.X + r2.Width / 2 - d / 2 - UiHelper.Dp(4), yTam);
                }

                bool xong = i < _hienTai || (i <= _xaNhat && i != _hienTai);
                bool dangMo = i == _hienTai;
                var o = new Rectangle(cx - d / 2, yTam - d / 2, d, d);
                if (dangMo)
                {
                    using (var br = new SolidBrush(Theme.Accent)) g.FillEllipse(br, o);
                    TextRenderer.DrawText(g, (i + 1).ToString(), Theme.FontDam, o, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
                else if (xong)
                {
                    using (var br = new SolidBrush(_hover == i ? Theme.AccentSoftHover : Theme.AccentSoft)) g.FillEllipse(br, o);
                    using (var f = Theme.FontIcon(9f))
                        TextRenderer.DrawText(g, Icons.ThanhCong, f, o, Theme.Accent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
                else
                {
                    using (var pen = new Pen(Theme.BorderStrong, 1.5f)) g.DrawEllipse(pen, o);
                    TextRenderer.DrawText(g, (i + 1).ToString(), Theme.FontNhan, o, Theme.Disabled, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }

                var rChu = new Rectangle(r.X + 2, yTam + d / 2 + UiHelper.Dp(3), r.Width - 4, Height - yTam - d / 2 - UiHelper.Dp(3));
                TextRenderer.DrawText(g, _buoc[i], dangMo ? FontBuocMo : Theme.FontNho, rChu,
                    dangMo ? Theme.Accent : xong ? Theme.Text : Theme.Disabled,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }

        private int BuocTaiDiem(Point p)
        {
            for (int i = 0; i < _buoc.Length; i++)
                if (VungBuoc(i).Contains(p)) return i;
            return -1;
        }

        private bool ChonDuoc(int i) => ChoPhepQuayLai && i >= 0 && i <= _xaNhat && i != _hienTai;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int h = BuocTaiDiem(e.Location);
            Cursor = ChonDuoc(h) ? Cursors.Hand : Cursors.Default;
            if (h != _hover) { _hover = h; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = -1;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int i = BuocTaiDiem(e.Location);
            if (ChonDuoc(i)) BuocDuocChon?.Invoke(i);
        }
    }
}
