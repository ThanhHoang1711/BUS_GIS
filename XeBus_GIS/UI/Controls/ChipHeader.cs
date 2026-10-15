using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using XeBus_GIS.Data;

namespace XeBus_GIS.UI.Controls
{
    /// <summary>
    /// Chip nhỏ trên header (thời tiết, vị trí): nền trong suốt, hover nhẹ, focus bàn phím; tự tính chiều rộng theo nội dung.
    /// Lớp con tự vẽ biểu tượng ở ô bên trái.
    /// </summary>
    public abstract class ChipHeader : Control
    {
        private bool _hover, _nhan;
        protected string Chu = "", ChuPhu = "";
        /// <summary>Thu gọn (màn hình hẹp): chỉ còn biểu tượng (+ chữ chính nếu ngắn).</summary>
        public bool RutGon { get; set; }

        protected ChipHeader()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint | ControlStyles.Selectable, true);
            Height = UiHelper.Dp(34);
            Cursor = Cursors.Hand;
            TabStop = true;
        }

        protected int O => UiHelper.Dp(22);

        /// <summary>Chiều rộng cần theo nội dung hiện tại.</summary>
        public int RongCan
        {
            get
            {
                int w = UiHelper.Dp(8) + O + UiHelper.Dp(6);
                if (Chu.Length > 0 && !(RutGon && ChiIconKhiGon)) w += TextRenderer.MeasureText(Chu, Theme.FontDam).Width;
                if (!RutGon && ChuPhu.Length > 0) w += UiHelper.Dp(4) + TextRenderer.MeasureText(ChuPhu, Theme.FontNhan).Width;
                return w + UiHelper.Dp(8);
            }
        }

        /// <summary>Khi thu gọn chỉ hiện biểu tượng (không cả chữ chính).</summary>
        protected virtual bool ChiIconKhiGon => false;

        protected abstract void VeIcon(Graphics g, Rectangle o);

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (_hover || _nhan || (Focused && ShowFocusCues))
                using (var path = UiHelper.DuongBoGoc(new RectangleF(0.5f, 0.5f, Width - 1, Height - 1), UiHelper.Dp(6)))
                {
                    using (var br = new SolidBrush(_nhan ? Theme.Border : Theme.SurfaceHover)) g.FillPath(br, path);
                    if (Focused && ShowFocusCues) using (var pen = new Pen(Theme.Accent, 1.5f)) g.DrawPath(pen, path);
                }
            var o = new Rectangle(UiHelper.Dp(8), (Height - O) / 2, O, O);
            VeIcon(g, o);
            int x = o.Right + UiHelper.Dp(6);
            if (Chu.Length > 0 && !(RutGon && ChiIconKhiGon))
            {
                TextRenderer.DrawText(g, Chu, Theme.FontDam, new Rectangle(x, 0, Width - x, Height), Theme.Text,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                x += TextRenderer.MeasureText(Chu, Theme.FontDam).Width + UiHelper.Dp(4);
            }
            if (!RutGon && ChuPhu.Length > 0 && x < Width)
                TextRenderer.DrawText(g, ChuPhu, Theme.FontNhan, new Rectangle(x, 0, Width - x - UiHelper.Dp(4), Height), Theme.Muted,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hover = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hover = false; _nhan = false; Invalidate(); }
        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); _nhan = true; Invalidate(); }
        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); _nhan = false; Invalidate(); }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
        protected override bool IsInputKey(Keys keyData) => keyData == Keys.Enter || keyData == Keys.Space || base.IsInputKey(keyData);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) { OnClick(EventArgs.Empty); e.Handled = true; }
        }
    }

    /// <summary>Chip thời tiết hiện tại: biểu tượng (tự vẽ) · nhiệt độ · mô tả. Chưa có dữ liệu ⇒ "—°".</summary>
    public class ChipThoiTiet : ChipHeader
    {
        private DuLieuThoiTiet _d;
        private bool _dangTai;

        public ChipThoiTiet() { Dat(null, false); }

        public DuLieuThoiTiet DuLieu => _d;

        public void Dat(DuLieuThoiTiet d, bool dangTai)
        {
            _d = d;
            _dangTai = dangTai;
            if (d == null)
            {
                Chu = "—°";
                ChuPhu = dangTai ? "Đang tải…" : "Chưa có dữ liệu";
            }
            else
            {
                Chu = Math.Round(d.NhietDo).ToString("0") + "°";
                ChuPhu = ThoiTietService.MoTa(d.MaWmo, d.BanNgay).moTa + (d.Cu ? " (cũ)" : "");
            }
            Invalidate();
        }

        protected override void VeIcon(Graphics g, Rectangle o)
        {
            var kieu = _d == null ? ThoiTietService.KieuIcon.May : ThoiTietService.MoTa(_d.MaWmo, _d.BanNgay).icon;
            var mauMay = Theme.Toi ? Color.FromArgb(0xCB, 0xD5, 0xE1) : Color.FromArgb(0x94, 0xA3, 0xB8);
            var mauNang = Color.FromArgb(0xF5, 0x9E, 0x0B);
            var mauMua = Color.FromArgb(0x0E, 0xA5, 0xE9);
            float s = o.Width;
            if (_d == null) { using (var pen = new Pen(Theme.Disabled, 1.5f)) VeMay(g, o, pen, null); return; }
            switch (kieu)
            {
                case ThoiTietService.KieuIcon.Nang: VeMatTroi(g, new RectangleF(o.X + s * 0.15f, o.Y + s * 0.15f, s * 0.7f, s * 0.7f), mauNang); break;
                case ThoiTietService.KieuIcon.Trang:
                    using (var br = new SolidBrush(Color.FromArgb(0xFC, 0xD3, 0x4D)))
                    using (var path = new GraphicsPath())
                    {
                        path.AddEllipse(o.X + s * 0.18f, o.Y + s * 0.15f, s * 0.64f, s * 0.64f);
                        using (var cat = new GraphicsPath())
                        {
                            cat.AddEllipse(o.X + s * 0.38f, o.Y + s * 0.05f, s * 0.6f, s * 0.6f);
                            using (var rg = new Region(path)) { rg.Exclude(cat); g.FillRegion(br, rg); }
                        }
                    }
                    break;
                case ThoiTietService.KieuIcon.ItMay:
                    VeMatTroi(g, new RectangleF(o.X + s * 0.38f, o.Y, s * 0.6f, s * 0.6f), mauNang);
                    using (var br = new SolidBrush(mauMay)) VeMay(g, new Rectangle(o.X, o.Y + (int)(s * 0.25f), (int)(s * 0.85f), (int)(s * 0.7f)), null, br);
                    break;
                case ThoiTietService.KieuIcon.SuongMu:
                    using (var pen = new Pen(mauMay, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        for (int i = 0; i < 3; i++) g.DrawLine(pen, o.X + s * 0.1f, o.Y + s * (0.3f + 0.2f * i), o.Right - s * 0.1f, o.Y + s * (0.3f + 0.2f * i));
                    break;
                default:
                    using (var br = new SolidBrush(mauMay)) VeMay(g, new Rectangle(o.X, o.Y, o.Width, (int)(s * 0.68f)), null, br);
                    if (kieu == ThoiTietService.KieuIcon.Mua || kieu == ThoiTietService.KieuIcon.MuaRao || kieu == ThoiTietService.KieuIcon.MuaPhun)
                        using (var pen = new Pen(mauMua, kieu == ThoiTietService.KieuIcon.MuaPhun ? 1.2f : 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                            for (int i = 0; i < 3; i++)
                            {
                                float x = o.X + s * (0.28f + 0.22f * i);
                                g.DrawLine(pen, x, o.Y + s * 0.74f, x - s * 0.08f, o.Y + s * 0.96f);
                            }
                    if (kieu == ThoiTietService.KieuIcon.Dong)
                        using (var br = new SolidBrush(mauNang))
                            g.FillPolygon(br, new[]
                            {
                                new PointF(o.X + s * 0.55f, o.Y + s * 0.55f), new PointF(o.X + s * 0.38f, o.Y + s * 0.8f),
                                new PointF(o.X + s * 0.5f, o.Y + s * 0.8f), new PointF(o.X + s * 0.42f, o.Y + s), new PointF(o.X + s * 0.66f, o.Y + s * 0.72f),
                                new PointF(o.X + s * 0.54f, o.Y + s * 0.72f),
                            });
                    if (kieu == ThoiTietService.KieuIcon.Tuyet)
                        using (var br = new SolidBrush(mauMua))
                            for (int i = 0; i < 3; i++) g.FillEllipse(br, o.X + s * (0.25f + 0.22f * i), o.Y + s * 0.8f, s * 0.1f, s * 0.1f);
                    break;
            }
        }

        private static void VeMatTroi(Graphics g, RectangleF r, Color mau)
        {
            float cx = r.X + r.Width / 2, cy = r.Y + r.Height / 2, rr = r.Width * 0.27f;
            using (var br = new SolidBrush(mau)) g.FillEllipse(br, cx - rr, cy - rr, rr * 2, rr * 2);
            using (var pen = new Pen(mau, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                for (int i = 0; i < 8; i++)
                {
                    double a = i * Math.PI / 4;
                    g.DrawLine(pen, cx + (float)Math.Cos(a) * rr * 1.45f, cy + (float)Math.Sin(a) * rr * 1.45f,
                                    cx + (float)Math.Cos(a) * r.Width / 2, cy + (float)Math.Sin(a) * r.Height / 2);
                }
        }

        /// <summary>Đám mây (3 hình tròn + đáy), vẽ viền hoặc tô.</summary>
        private static void VeMay(Graphics g, Rectangle o, Pen vien, Brush to)
        {
            float w = o.Width, h = o.Height;
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(o.X + w * 0.05f, o.Y + h * 0.45f, w * 0.4f, h * 0.45f);
                path.AddEllipse(o.X + w * 0.25f, o.Y + h * 0.15f, w * 0.5f, h * 0.6f);
                path.AddEllipse(o.X + w * 0.55f, o.Y + h * 0.4f, w * 0.4f, h * 0.5f);
                path.AddRectangle(new RectangleF(o.X + w * 0.25f, o.Y + h * 0.6f, w * 0.5f, h * 0.3f));
                using (var rg = new Region(path))
                {
                    if (to != null) g.FillRegion(to, rg);
                    else if (vien != null) g.DrawPath(vien, path);
                }
            }
        }
    }

    /// <summary>Chip vị trí người dùng: ghim + tên phường/xã (hoặc "Chưa định vị" / "Đang xác định…").</summary>
    public class ChipViTri : ChipHeader
    {
        private bool _co, _dangTim;

        public ChipViTri() { Dat(null, false); }

        protected override bool ChiIconKhiGon => true;

        public void Dat(string ten, bool dangTim)
        {
            _co = ten != null;
            _dangTim = dangTim;
            Chu = dangTim ? "Đang xác định…" : ten ?? "Chưa định vị";
            ChuPhu = "";
            Invalidate();
        }

        protected override void VeIcon(Graphics g, Rectangle o)
        {
            var mau = _co ? Theme.Accent : _dangTim ? Theme.Warning : Theme.Muted;
            using (var f = Theme.FontIcon(11f))
                TextRenderer.DrawText(g, _co ? "" : "", f, o, mau, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }
}
