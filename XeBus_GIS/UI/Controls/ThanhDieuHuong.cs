using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace XeBus_GIS.UI.Controls
{
    /// <summary>Một mục trên thanh điều hướng.</summary>
    public class MucDieuHuong
    {
        public string Khoa;
        public string Ten;
        public string Icon;
        /// <summary>Tên nhóm hiển thị phía trên mục (null = cùng nhóm với mục trước).</summary>
        public string Nhom;
        public bool Hien = true;
        internal Rectangle Vung;
    }

    /// <summary>
    /// Thanh điều hướng bên trái (tự vẽ): thương hiệu, các nhóm mục có icon, mục đang chọn nền xanh nhạt + vạch trái,
    /// hover / focus bàn phím (↑ ↓ Enter), thu gọn 220 ↔ 56 px có chuyển động ngắn, tooltip khi thu gọn.
    /// </summary>
    public class ThanhDieuHuong : Control
    {
        private readonly List<MucDieuHuong> _muc = new List<MucDieuHuong>();
        private readonly ToolTip _tip = new ToolTip { InitialDelay = 250 };
        private readonly Timer _timer = new Timer { Interval = 15 };
        private string _khoaChon;
        private int _hover = -1;       // chỉ số mục đang hover; -2 = nút thu gọn
        private int _focus = -1;       // chỉ số mục focus bằng bàn phím
        private bool _focusBanPhim;
        private bool _thuGon;
        private int _rongDich;
        private string _tipDangHien;

        public int RongMo { get; set; } = UiHelper.Dp(220);
        public int RongThu { get; set; } = UiHelper.Dp(56);

        private int CaoThuongHieu => UiHelper.Dp(64);
        private int CaoMuc => UiHelper.Dp(40);
        private int CaoNhom => UiHelper.Dp(30);
        private int CaoNutThu => UiHelper.Dp(44);

        /// <summary>Người dùng chọn một mục (khoá).</summary>
        public event Action<string> MucDuocChon;

        /// <summary>true khi bắt đầu chuyển động thu/mở, false khi kết thúc (form chính khoá vẽ lại bản đồ).</summary>
        public event Action<bool> DangChuyenDong;

        public string TieuDe { get; set; } = "XeBus GIS";
        public string PhuDe { get; set; } = "Quản lý trạm xe buýt";

        public ThanhDieuHuong()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.Selectable, true);
            BackColor = Theme.Surface;
            Width = RongMo;
            TabStop = true;
            _timer.Tick += Tick;
        }

        public IList<MucDieuHuong> CacMuc => _muc;

        public MucDieuHuong Them(string khoa, string ten, string icon, string nhom = null)
        {
            var m = new MucDieuHuong { Khoa = khoa, Ten = ten, Icon = icon, Nhom = nhom };
            _muc.Add(m);
            Invalidate();
            return m;
        }

        public void DatHien(string khoa, bool hien)
        {
            var m = _muc.FirstOrDefault(x => x.Khoa == khoa);
            if (m != null) { m.Hien = hien; Invalidate(); }
        }

        public string KhoaDangChon
        {
            get => _khoaChon;
            set { _khoaChon = value; Invalidate(); }
        }

        public bool ThuGon
        {
            get => _thuGon;
            set
            {
                if (_thuGon == value) return;
                _thuGon = value;
                _rongDich = value ? RongThu : RongMo;
                if (!_timer.Enabled) DangChuyenDong?.Invoke(true);
                _timer.Start();
            }
        }

        private void Tick(object sender, EventArgs e)
        {
            int conLai = _rongDich - Width;
            if (Math.Abs(conLai) <= 2)
            {
                Width = _rongDich;
                _timer.Stop();
                Invalidate();
                DangChuyenDong?.Invoke(false);
                return;
            }
            // ease-out: mỗi bước đi 40% quãng còn lại (~120 ms)
            Width += (int)Math.Round(conLai * 0.4);
        }

        // =====================================================================
        // Bố cục
        // =====================================================================
        private bool DangGon => Width < (RongMo + RongThu) / 2;

        private void TinhBoCuc()
        {
            int y = CaoThuongHieu + UiHelper.Dp(8);
            string nhomTruoc = null;
            foreach (var m in _muc)
            {
                if (!m.Hien) { m.Vung = Rectangle.Empty; continue; }
                if (m.Nhom != null && m.Nhom != nhomTruoc)
                {
                    y += DangGon ? UiHelper.Dp(12) : CaoNhom;
                    nhomTruoc = m.Nhom;
                }
                m.Vung = new Rectangle(UiHelper.Dp(8), y, Width - UiHelper.Dp(16), CaoMuc);
                y += CaoMuc + UiHelper.Dp(2);
            }
        }

        private Rectangle VungNutThu => new Rectangle(UiHelper.Dp(8), Height - CaoNutThu - UiHelper.Dp(8), Width - UiHelper.Dp(16), CaoNutThu);

        private List<MucDieuHuong> MucHien => _muc.Where(m => m.Hien).ToList();

        // =====================================================================
        // Vẽ
        // =====================================================================
        protected override void OnPaint(PaintEventArgs e)
        {
            TinhBoCuc();
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            bool gon = DangGon;

            // viền phải
            using (var pen = new Pen(Theme.Border))
                g.DrawLine(pen, Width - 1, 0, Width - 1, Height);

            // thương hiệu: ô vuông bo góc màu Accent chứa icon xe buýt
            int o = UiHelper.Dp(34);
            var rLogo = new Rectangle(UiHelper.Dp(11), (CaoThuongHieu - o) / 2, o, o);
            using (var path = UiHelper.DuongBoGoc(rLogo, UiHelper.Dp(8)))
            using (var br = new SolidBrush(Theme.Accent))
                g.FillPath(br, path);
            using (var f = Theme.FontIcon(14f))
                TextRenderer.DrawText(g, Icons.Bus, f, rLogo, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            if (!gon)
            {
                int x = rLogo.Right + UiHelper.Dp(10);
                TextRenderer.DrawText(g, TieuDe, Theme.FontAppTitle, new Point(x, rLogo.Top - UiHelper.Dp(3)), Theme.Primary, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, PhuDe, Theme.FontNho, new Point(x, rLogo.Top + UiHelper.Dp(19)), Theme.Muted, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
            using (var pen = new Pen(Theme.Border))
                g.DrawLine(pen, UiHelper.Dp(12), CaoThuongHieu, Width - UiHelper.Dp(12), CaoThuongHieu);

            // nhóm + mục
            string nhomTruoc = null;
            var hien = MucHien;
            for (int i = 0; i < hien.Count; i++)
            {
                var m = hien[i];
                if (m.Nhom != null && m.Nhom != nhomTruoc)
                {
                    nhomTruoc = m.Nhom;
                    if (gon)
                    {
                        using (var pen = new Pen(Theme.Border))
                            g.DrawLine(pen, UiHelper.Dp(16), m.Vung.Top - UiHelper.Dp(7), Width - UiHelper.Dp(16), m.Vung.Top - UiHelper.Dp(7));
                    }
                    else
                    {
                        TextRenderer.DrawText(g, m.Nhom.ToUpperInvariant(), Theme.FontNho,
                            new Rectangle(UiHelper.Dp(20), m.Vung.Top - CaoNhom, Width - UiHelper.Dp(28), CaoNhom - UiHelper.Dp(4)),
                            Theme.Disabled, TextFormatFlags.Bottom | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                    }
                }
                VeMuc(g, m, _muc.IndexOf(m), gon);
            }

            // nút thu gọn
            var rThu = VungNutThu;
            if (_hover == -2)
                using (var path = UiHelper.DuongBoGoc(rThu, Theme.BoGoc))
                using (var br = new SolidBrush(Theme.SurfaceHover))
                    g.FillPath(br, path);
            using (var pen = new Pen(Theme.Border))
                g.DrawLine(pen, UiHelper.Dp(12), rThu.Top - UiHelper.Dp(8), Width - UiHelper.Dp(12), rThu.Top - UiHelper.Dp(8));
            var rIconThu = new Rectangle(rThu.X, rThu.Y, RongThu - UiHelper.Dp(16), rThu.Height);
            using (var f = Theme.FontIcon(11f))
                TextRenderer.DrawText(g, gon ? Icons.Phai : Icons.Trai, f, rIconThu, Theme.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            if (!gon)
                TextRenderer.DrawText(g, "Thu gọn", Theme.FontNhan, new Rectangle(rIconThu.Right, rThu.Y, rThu.Width - rIconThu.Width, rThu.Height),
                    Theme.Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix);
        }

        private void VeMuc(Graphics g, MucDieuHuong m, int chiSo, bool gon)
        {
            bool chon = m.Khoa == _khoaChon;
            bool hover = chiSo == _hover;
            var r = m.Vung;

            if (chon || hover)
                using (var path = UiHelper.DuongBoGoc(r, Theme.BoGoc))
                using (var br = new SolidBrush(chon ? Theme.AccentSoft : Theme.SurfaceHover))
                    g.FillPath(br, path);
            if (chon)
                using (var br = new SolidBrush(Theme.Accent))
                    g.FillRectangle(br, r.X, r.Y + UiHelper.Dp(9), UiHelper.Dp(3), r.Height - UiHelper.Dp(18));
            if (Focused && _focusBanPhim && chiSo == _focus)
                using (var path = UiHelper.DuongBoGoc(new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1, r.Height - 1), Theme.BoGoc))
                using (var pen = new Pen(Theme.Accent, 1.5f))
                    g.DrawPath(pen, path);

            var mau = chon ? Theme.Accent : Theme.Secondary;
            var rIcon = new Rectangle(r.X, r.Y, RongThu - UiHelper.Dp(16), r.Height);
            using (var f = Theme.FontIcon(12f))
                TextRenderer.DrawText(g, m.Icon, f, rIcon, mau, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            if (!gon)
                TextRenderer.DrawText(g, m.Ten, chon ? Theme.FontDam : Theme.FontThuong,
                    new Rectangle(rIcon.Right, r.Y, r.Width - rIcon.Width - UiHelper.Dp(4), r.Height), chon ? Theme.Accent : Theme.Text,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        // =====================================================================
        // Chuột / bàn phím
        // =====================================================================
        private int MucTaiDiem(Point p)
        {
            TinhBoCuc();
            if (VungNutThu.Contains(p)) return -2;
            for (int i = 0; i < _muc.Count; i++)
                if (_muc[i].Hien && _muc[i].Vung.Contains(p)) return i;
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int h = MucTaiDiem(e.Location);
            if (h == _hover) return;
            _hover = h;
            Cursor = h == -1 ? Cursors.Default : Cursors.Hand;
            string tip = null;
            if (DangGon && h >= 0) tip = _muc[h].Ten;
            else if (h == -2) tip = DangGon ? "Mở rộng menu (Ctrl+B)" : "Thu gọn menu (Ctrl+B)";
            if (tip != _tipDangHien)
            {
                _tipDangHien = tip;
                _tip.SetToolTip(this, tip);
            }
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = -1;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _focusBanPhim = false;
            base.OnMouseDown(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left) return;
            int h = MucTaiDiem(e.Location);
            if (h == -2) ThuGon = !ThuGon;
            else if (h >= 0) Chon(_muc[h]);
        }

        private void Chon(MucDieuHuong m)
        {
            _focus = _muc.IndexOf(m);
            MucDuocChon?.Invoke(m.Khoa);
        }

        protected override bool IsInputKey(Keys keyData) =>
            keyData == Keys.Up || keyData == Keys.Down || base.IsInputKey(keyData);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            var hien = MucHien;
            if (hien.Count == 0) return;
            int vt = _focus >= 0 ? hien.IndexOf(_muc[_focus]) : hien.FindIndex(m => m.Khoa == _khoaChon);
            if (e.KeyCode == Keys.Down) vt = Math.Min(hien.Count - 1, vt + 1);
            else if (e.KeyCode == Keys.Up) vt = Math.Max(0, vt - 1);
            else if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) && vt >= 0) { Chon(hien[vt]); return; }
            else return;
            _focusBanPhim = true;
            _focus = _muc.IndexOf(hien[Math.Max(0, vt)]);
            Invalidate();
            e.Handled = true;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            if (Control.MouseButtons == MouseButtons.None)
            {
                _focusBanPhim = true;
                if (_focus < 0) _focus = _muc.FindIndex(m => m.Khoa == _khoaChon);
            }
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
                _tip.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
