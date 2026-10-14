using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace XeBus_GIS.UI.Controls
{
    /// <summary>
    /// Nhóm nút chọn một (chip): mỗi chip có chấm màu tuỳ chọn + chữ; chip đang chọn nền AccentSoft, viền Accent.
    /// Tự xuống dòng theo chiều rộng, chiều cao tự tính. Phím ←/→ để đổi.
    /// </summary>
    public class NhomChip : Control
    {
        private string[] _muc = new string[0];
        private Color?[] _mau = new Color?[0];
        private int _chon;
        private int _hover = -1;

        public event Action<int> ChonDoi;

        public NhomChip()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint | ControlStyles.Selectable, true);
            BackColor = Theme.Surface;
            TabStop = true;
            Height = UiHelper.Dp(30);
        }

        public void Dat(string[] muc, Color?[] mau = null)
        {
            _muc = muc ?? new string[0];
            _mau = mau ?? new Color?[_muc.Length];
            _chon = Math.Min(_chon, Math.Max(0, _muc.Length - 1));
            TinhChieuCao();
            Invalidate();
        }

        public int Chon
        {
            get => _chon;
            set
            {
                int v = Math.Max(0, Math.Min(_muc.Length - 1, value));
                if (v == _chon) return;
                _chon = v;
                Invalidate();
                ChonDoi?.Invoke(v);
            }
        }

        private int CaoChip => UiHelper.Dp(26);
        private int Cach => UiHelper.Dp(6);

        private List<Rectangle> BoCuc()
        {
            var ds = new List<Rectangle>();
            int x = 0, y = 0;
            for (int i = 0; i < _muc.Length; i++)
            {
                int w = TextRenderer.MeasureText(_muc[i], Theme.FontNhan).Width + UiHelper.Dp(_mau[i].HasValue ? 30 : 20);
                if (x > 0 && x + w > Width) { x = 0; y += CaoChip + Cach; }
                ds.Add(new Rectangle(x, y, w, CaoChip));
                x += w + Cach;
            }
            return ds;
        }

        private void TinhChieuCao()
        {
            int cao = BoCuc().Select(r => r.Bottom).DefaultIfEmpty(CaoChip).Max() + 1;
            if (Height != cao) Height = cao;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            TinhChieuCao();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var bc = BoCuc();
            for (int i = 0; i < bc.Count; i++)
            {
                var r = bc[i];
                bool chon = i == _chon;
                using (var path = UiHelper.DuongBoGoc(new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1, r.Height - 1), r.Height / 2f))
                {
                    using (var br = new SolidBrush(chon ? Theme.AccentSoft : i == _hover ? Theme.SurfaceHover : Theme.Surface))
                        g.FillPath(br, path);
                    using (var pen = new Pen(chon ? Theme.Accent : Theme.Border, chon ? 1.5f : 1f))
                        g.DrawPath(pen, path);
                }
                int x = r.X + UiHelper.Dp(10);
                if (_mau[i].HasValue)
                {
                    int d = UiHelper.Dp(8);
                    using (var br = new SolidBrush(_mau[i].Value)) g.FillEllipse(br, x, r.Y + (r.Height - d) / 2, d, d);
                    x += UiHelper.Dp(12);
                }
                TextRenderer.DrawText(g, _muc[i], chon ? Theme.FontDam : Theme.FontNhan, new Rectangle(x, r.Y, r.Right - x - UiHelper.Dp(6), r.Height),
                    chon ? Theme.Accent : Theme.Secondary, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix);
                if (chon && Focused && ShowFocusCues)
                    using (var pen = new Pen(Color.FromArgb(90, Theme.Accent), 2f))
                    using (var path = UiHelper.DuongBoGoc(RectangleF.Inflate(r, 2, 2), r.Height / 2f + 2))
                        g.DrawPath(pen, path);
            }
        }

        private int ChipTai(Point p)
        {
            var bc = BoCuc();
            for (int i = 0; i < bc.Count; i++) if (bc[i].Contains(p)) return i;
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int h = ChipTai(e.Location);
            Cursor = h >= 0 ? Cursors.Hand : Cursors.Default;
            if (h != _hover) { _hover = h; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hover = -1; Invalidate(); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            Focus();
            int i = ChipTai(e.Location);
            if (i >= 0) Chon = i;
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
        protected override bool IsInputKey(Keys keyData) => keyData == Keys.Left || keyData == Keys.Right || base.IsInputKey(keyData);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Left) { Chon--; e.Handled = true; }
            else if (e.KeyCode == Keys.Right) { Chon++; e.Handled = true; }
        }
    }
}
