using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace XeBus_GIS.UI.Controls
{
    /// <summary>
    /// Biểu đồ radar tự vẽ (lấp đầy khung, không bị co nhỏ như Chart Radar): n trục 0–100, lưới 4 vòng,
    /// đa giác "tham chiếu" nét đứt (vd trung bình khu vực) và đa giác giá trị tô màu; nhãn trục kèm giá trị.
    /// </summary>
    public class BieuDoRadar : Control
    {
        private string[] _nhan = new string[0];
        private Color[] _mauTruc = new Color[0];
        private double[] _giaTri, _thamChieu;

        public Color MauGiaTri { get; set; } = Theme.Accent;
        public string ChuThichGiaTri { get; set; } = "Vị trí đang xem";
        public string ChuThichThamChieu { get; set; } = "TB khu vực";

        public BieuDoRadar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Theme.Surface;
        }

        public void DatTruc(string[] nhan, Color[] mau)
        {
            _nhan = nhan ?? new string[0];
            _mauTruc = mau ?? new Color[_nhan.Length];
            Invalidate();
        }

        /// <summary>Giá trị 0..100 (null = không vẽ).</summary>
        public void DatGiaTri(double[] giaTri, double[] thamChieu)
        {
            _giaTri = giaTri;
            _thamChieu = thamChieu;
            Invalidate();
        }

        private PointF Diem(PointF tam, float r, int i, double v)
        {
            double goc = -Math.PI / 2 + 2 * Math.PI * i / _nhan.Length;
            float k = (float)(Math.Max(0, Math.Min(100, v)) / 100.0) * r;
            return new PointF(tam.X + (float)Math.Cos(goc) * k, tam.Y + (float)Math.Sin(goc) * k);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int n = _nhan.Length;
            if (n < 3) return;
            int hChuThich = UiHelper.Dp(20);
            int leNgang = UiHelper.Dp(44), leDoc = UiHelper.Dp(18);
            float r = Math.Max(10, Math.Min((Width - 2 * leNgang) / 2f, (Height - hChuThich - 2 * leDoc) / 2f));
            var tam = new PointF(Width / 2f, leDoc + (Height - hChuThich - 2 * leDoc) / 2f + UiHelper.Dp(2));

            // lưới
            using (var pen = new Pen(Theme.Border))
            {
                for (int vong = 1; vong <= 4; vong++)
                    g.DrawPolygon(pen, Enumerable.Range(0, n).Select(i => Diem(tam, r, i, vong * 25)).ToArray());
                for (int i = 0; i < n; i++) g.DrawLine(pen, tam, Diem(tam, r, i, 100));
            }

            if (_thamChieu != null)
                using (var pen = new Pen(Theme.Disabled, UiHelper.Dp(2)) { DashStyle = DashStyle.Dash })
                    g.DrawPolygon(pen, Enumerable.Range(0, n).Select(i => Diem(tam, r, i, _thamChieu[i])).ToArray());

            if (_giaTri != null)
            {
                var dg = Enumerable.Range(0, n).Select(i => Diem(tam, r, i, _giaTri[i])).ToArray();
                using (var br = new SolidBrush(Color.FromArgb(70, MauGiaTri))) g.FillPolygon(br, dg);
                using (var pen = new Pen(MauGiaTri, UiHelper.Dp(2))) g.DrawPolygon(pen, dg);
                int d = UiHelper.Dp(8);
                for (int i = 0; i < n; i++)
                    using (var br = new SolidBrush(i < _mauTruc.Length ? _mauTruc[i] : MauGiaTri))
                    using (var pen = new Pen(Color.White, 1.5f))
                    {
                        var o = new RectangleF(dg[i].X - d / 2f, dg[i].Y - d / 2f, d, d);
                        g.FillEllipse(br, o);
                        g.DrawEllipse(pen, o);
                    }
            }

            // nhãn trục (+ giá trị)
            for (int i = 0; i < n; i++)
            {
                var p = Diem(tam, r + UiHelper.Dp(6), i, 100);
                string chu = _nhan[i] + (_giaTri != null ? $" {_giaTri[i]:0}" : "");
                var sz = TextRenderer.MeasureText(chu, Theme.FontNho);
                float x = p.X - sz.Width / 2f, y = p.Y - sz.Height / 2f;
                double goc = -Math.PI / 2 + 2 * Math.PI * i / n;
                if (Math.Cos(goc) > 0.3) x = p.X;
                else if (Math.Cos(goc) < -0.3) x = p.X - sz.Width;
                if (Math.Sin(goc) < -0.9) y = p.Y - sz.Height;
                x = Math.Max(0, Math.Min(Width - sz.Width, x));
                TextRenderer.DrawText(g, chu, Theme.FontNho, new Point((int)x, (int)y), Theme.Secondary, TextFormatFlags.NoPrefix);
            }

            // chú thích
            int yCt = Height - hChuThich + UiHelper.Dp(4), xCt = UiHelper.Dp(8);
            using (var br = new SolidBrush(Color.FromArgb(120, MauGiaTri))) g.FillRectangle(br, xCt, yCt + UiHelper.Dp(4), UiHelper.Dp(12), UiHelper.Dp(8));
            xCt += UiHelper.Dp(16);
            TextRenderer.DrawText(g, ChuThichGiaTri, Theme.FontNho, new Point(xCt, yCt), Theme.Muted, TextFormatFlags.NoPrefix);
            xCt += TextRenderer.MeasureText(ChuThichGiaTri, Theme.FontNho).Width + UiHelper.Dp(10);
            using (var pen = new Pen(Theme.Disabled, UiHelper.Dp(2)) { DashStyle = DashStyle.Dash })
                g.DrawLine(pen, xCt, yCt + UiHelper.Dp(8), xCt + UiHelper.Dp(14), yCt + UiHelper.Dp(8));
            TextRenderer.DrawText(g, ChuThichThamChieu, Theme.FontNho, new Point(xCt + UiHelper.Dp(18), yCt), Theme.Muted, TextFormatFlags.NoPrefix);
        }
    }
}
