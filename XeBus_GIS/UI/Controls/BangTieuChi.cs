using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace XeBus_GIS.UI.Controls
{
    /// <summary>Một dòng của BangTieuChi.</summary>
    public class DongTieuChi
    {
        public string Ten;
        public string Icon;
        public Color Mau;
        /// <summary>Điểm tiêu chí 0..1 (null = không áp dụng).</summary>
        public double? GiaTri;
        /// <summary>Số liệu chính (vd "1.240 người chưa có trạm trong 500 m").</summary>
        public string SoLieu;
        /// <summary>Ghi chú phụ (vd "820 người trong 250 m").</summary>
        public string GhiChu;
    }

    /// <summary>
    /// Bảng giải thích tiêu chí của một vị trí: mỗi dòng = ô màu có icon · tên · điểm /100 · thanh điểm · số liệu thật.
    /// Chiều cao tự tính theo số dòng (đặt trong panel AutoScroll).
    /// </summary>
    public class BangTieuChi : Control
    {
        private List<DongTieuChi> _dong = new List<DongTieuChi>();
        private string _rong = "Chọn một vị trí trong bảng hoặc trên bản đồ để xem vì sao vị trí đó được chấm điểm như vậy.";

        public BangTieuChi()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Theme.Surface;
            Height = UiHelper.Dp(60);
        }

        public string ChuRong
        {
            get => _rong;
            set { _rong = value ?? ""; Invalidate(); }
        }

        public void Dat(IEnumerable<DongTieuChi> dong)
        {
            _dong = dong?.ToList() ?? new List<DongTieuChi>();
            Height = _dong.Count == 0 ? UiHelper.Dp(60) : _dong.Count * CaoDong;
            Invalidate();
        }

        private int CaoDong => UiHelper.Dp(76);

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (_dong.Count == 0)
            {
                TextRenderer.DrawText(g, _rong, Theme.FontNhan, new Rectangle(0, 0, Width, Height), Theme.Muted,
                    TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
                return;
            }
            int s = UiHelper.Dp(22), le = UiHelper.Dp(30);
            for (int i = 0; i < _dong.Count; i++)
            {
                var d = _dong[i];
                int y = i * CaoDong;
                var o = new Rectangle(0, y + UiHelper.Dp(2), s, s);
                using (var path = UiHelper.DuongBoGoc(o, UiHelper.Dp(5)))
                using (var br = new SolidBrush(d.Mau)) g.FillPath(br, path);
                using (var f = Theme.FontIcon(9f))
                    TextRenderer.DrawText(g, d.Icon, f, o, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

                string diem = d.GiaTri.HasValue ? $"{d.GiaTri.Value * 100:0}/100" : "—";
                int wDiem = TextRenderer.MeasureText(diem, Theme.FontDam).Width;
                TextRenderer.DrawText(g, d.Ten, Theme.FontDam, new Rectangle(le, y, Width - le - wDiem - UiHelper.Dp(6), UiHelper.Dp(26)), Theme.Text,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, diem, Theme.FontDam, new Rectangle(Width - wDiem - 2, y, wDiem + 2, UiHelper.Dp(26)), d.Mau,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.NoPrefix);

                // thanh điểm hết chiều rộng dưới tên
                int h = UiHelper.Dp(6);
                var nen = new RectangleF(le, y + UiHelper.Dp(27), Width - le - 2, h);
                using (var path = UiHelper.DuongBoGoc(nen, h / 2f))
                using (var br = new SolidBrush(Theme.Border)) g.FillPath(br, path);
                if (d.GiaTri.HasValue && d.GiaTri.Value > 0.005)
                {
                    var day = new RectangleF(nen.X, nen.Y, (float)(nen.Width * System.Math.Min(1, d.GiaTri.Value)), h);
                    using (var path = UiHelper.DuongBoGoc(day, h / 2f))
                    using (var br = new SolidBrush(d.Mau)) g.FillPath(br, path);
                }

                TextRenderer.DrawText(g, d.SoLieu ?? "", Theme.FontNhan, new Rectangle(le, y + UiHelper.Dp(37), Width - le, UiHelper.Dp(18)), Theme.Secondary,
                    TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                if (!string.IsNullOrEmpty(d.GhiChu))
                    TextRenderer.DrawText(g, d.GhiChu, Theme.FontNho, new Rectangle(le, y + UiHelper.Dp(55), Width - le, UiHelper.Dp(17)), Theme.Muted,
                        TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                if (i < _dong.Count - 1)
                    using (var pen = new Pen(Theme.Border)) g.DrawLine(pen, le, y + CaoDong - 2, Width, y + CaoDong - 2);
            }
        }
    }
}
