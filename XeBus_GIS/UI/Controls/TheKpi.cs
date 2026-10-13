using System;
using System.Drawing;
using System.Windows.Forms;

namespace XeBus_GIS.UI.Controls
{
    /// <summary>
    /// Ô số liệu: nhãn (chữ phụ) · giá trị lớn · chú thích nhỏ. Vạch màu bên trái thể hiện ý nghĩa
    /// (Accent = trung tính, Danger = cần chú ý…). Giá trị chưa có thì hiện "—".
    /// </summary>
    public class TheKpi : Control
    {
        private string _nhan = "Chỉ số", _giaTri = "—", _chuThich = "";
        private Color _mauNhan = Theme.Accent;

        public TheKpi()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Theme.Surface;
            Size = new Size(UiHelper.Dp(180), UiHelper.Dp(88));
        }

        public string Nhan { get => _nhan; set { _nhan = value; Invalidate(); } }
        public string GiaTri { get => _giaTri; set { _giaTri = string.IsNullOrEmpty(value) ? "—" : value; Invalidate(); } }
        public string ChuThich { get => _chuThich; set { _chuThich = value; Invalidate(); } }
        public Color MauNhan { get => _mauNhan; set { _mauNhan = value; Invalidate(); } }

        private Font _fontGiaTri;
        /// <summary>Font của giá trị (mặc định <see cref="Theme.FontKpi"/>; ô hẹp dùng cỡ nhỏ hơn).</summary>
        [System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Font FontGiaTri { get => _fontGiaTri ?? Theme.FontKpi; set { _fontGiaTri = value; Invalidate(); } }

        public void Dat(string giaTri, string chuThich = null, Color? mau = null)
        {
            _giaTri = string.IsNullOrEmpty(giaTri) ? "—" : giaTri;
            if (chuThich != null) _chuThich = chuThich;
            if (mau.HasValue) _mauNhan = mau.Value;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            using (var pen = new Pen(Theme.Border))
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            using (var br = new SolidBrush(_mauNhan))
                g.FillRectangle(br, 0, 0, UiHelper.Dp(3), Height);

            int x = UiHelper.Dp(14), w = Width - x - UiHelper.Dp(8);
            TextRenderer.DrawText(g, _nhan, Theme.FontNhan, new Rectangle(x, UiHelper.Dp(10), w, UiHelper.Dp(18)), Theme.Muted,
                TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            // vùng giá trị: giữa nhãn và chú thích (co theo chiều cao ô)
            int yGt = UiHelper.Dp(28), cuoi = Height - (string.IsNullOrEmpty(_chuThich) ? UiHelper.Dp(8) : UiHelper.Dp(26));
            TextRenderer.DrawText(g, _giaTri, FontGiaTri, new Rectangle(x - UiHelper.Dp(2), yGt, w, Math.Max(UiHelper.Dp(16), cuoi - yGt)), Theme.Primary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if (!string.IsNullOrEmpty(_chuThich))
                TextRenderer.DrawText(g, _chuThich, Theme.FontNho, new Rectangle(x, Height - UiHelper.Dp(24), w, UiHelper.Dp(18)), Theme.Muted,
                    TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }
}
