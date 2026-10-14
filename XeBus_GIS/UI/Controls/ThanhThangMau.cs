using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace XeBus_GIS.UI.Controls
{
    /// <summary>
    /// Chú giải thang màu liên tục chia lớp: [tiêu đề] / ô màu nối tiếp / nhãn hai đầu (vd "Kém" … "Tốt").
    /// </summary>
    public class ThanhThangMau : Control
    {
        private Color[] _mau = Theme.ThangMauPhuHop;
        private string _tieuDe = "", _trai = "", _phai = "";

        public ThanhThangMau()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Theme.Surface;
            Height = UiHelper.Dp(50);
        }

        public void Dat(string tieuDe, Color[] mau, string trai, string phai)
        {
            _tieuDe = tieuDe ?? "";
            _mau = mau ?? Theme.ThangMauPhuHop;
            _trai = trai ?? "";
            _phai = phai ?? "";
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int y = 0;
            if (_tieuDe.Length > 0)
            {
                TextRenderer.DrawText(g, _tieuDe, Theme.FontNhan, new Rectangle(0, 0, Width, UiHelper.Dp(18)), Theme.Secondary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                y = UiHelper.Dp(20);
            }
            int h = UiHelper.Dp(10), n = _mau.Length;
            float w = Width / (float)n;
            using (var path = UiHelper.DuongBoGoc(new RectangleF(0, y, Width - 1, h), h / 2f))
            {
                var clip = g.Clip;
                g.SetClip(path);
                for (int i = 0; i < n; i++)
                    using (var br = new SolidBrush(_mau[i])) g.FillRectangle(br, i * w, y, w + 1, h);
                g.Clip = clip;
            }
            var rChu = new Rectangle(0, y + h + UiHelper.Dp(2), Width, UiHelper.Dp(16));
            TextRenderer.DrawText(g, _trai, Theme.FontNho, rChu, Theme.Muted, TextFormatFlags.Left | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, _phai, Theme.FontNho, rChu, Theme.Muted, TextFormatFlags.Right | TextFormatFlags.NoPrefix);
        }
    }
}
