using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace XeBus_GIS.UI.Controls
{
    /// <summary>
    /// Chú giải ngắn gọn đặt trong module (vd "● Ứng viên  ● Trạm hiện có  ▭ Khu thiếu trạm"):
    /// các mục xếp nối tiếp, tự xuống dòng theo chiều rộng; chiều cao tự tính.
    /// </summary>
    public class KhungChuGiai : Control
    {
        private List<ChuGiai> _muc = new List<ChuGiai>();

        public KhungChuGiai()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Theme.Surface;
            Height = UiHelper.Dp(24);
        }

        public void Dat(params ChuGiai[] muc)
        {
            _muc = muc.ToList();
            TinhChieuCao();
            Parent?.PerformLayout();
            Invalidate();
        }

        private int CaoDong => UiHelper.Dp(22);

        private IEnumerable<(ChuGiai muc, Rectangle vung)> BoCuc()
        {
            int x = 0, y = 0;
            foreach (var m in _muc)
            {
                int w = UiHelper.Dp(22) + TextRenderer.MeasureText(m.Ten, Theme.FontNhan).Width + UiHelper.Dp(10);
                if (x > 0 && x + w > Width) { x = 0; y += CaoDong; }
                yield return (m, new Rectangle(x, y, w, CaoDong));
                x += w;
            }
        }

        private void TinhChieuCao()
        {
            int cao = BoCuc().Select(b => b.vung.Bottom).DefaultIfEmpty(CaoDong).Max();
            if (Height == cao) return;
            Height = cao;
            // control Dock: đổi chiều cao trong lúc cha đang bố cục thì phải bố cục lại cha
            if (Dock != DockStyle.None && Parent != null && Parent.IsHandleCreated) Parent.BeginInvoke((System.Action)(() => Parent?.PerformLayout()));
        }

        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);
            TinhChieuCao();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            foreach (var (m, r) in BoCuc())
            {
                PanelLopBanDo.VeKyHieu(g, m, new Rectangle(r.X, r.Y + (r.Height - UiHelper.Dp(14)) / 2, UiHelper.Dp(16), UiHelper.Dp(14)));
                TextRenderer.DrawText(g, m.Ten, Theme.FontNhan, new Rectangle(r.X + UiHelper.Dp(22), r.Y, r.Width - UiHelper.Dp(22), r.Height),
                    Theme.Secondary, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix);
            }
        }
    }
}
