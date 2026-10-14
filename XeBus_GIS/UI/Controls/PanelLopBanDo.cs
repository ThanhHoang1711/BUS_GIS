using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace XeBus_GIS.UI.Controls
{
    public enum KieuKyHieu { Tron, Vuong, TamGiac, Thoi, ChuThap, Duong, DuongDut, DuongCham, Vung, XeBus, IconTron }

    /// <summary>Một mục chú giải (ký hiệu + tên).</summary>
    public class ChuGiai
    {
        public string Ten;
        public Color Mau;
        public KieuKyHieu Kieu;
        /// <summary>Icon (Segoe Fluent) cho kiểu IconTron – ký hiệu công trình.</summary>
        public string Glyph;
        public ChuGiai(string ten, Color mau, KieuKyHieu kieu) { Ten = ten; Mau = mau; Kieu = kieu; }
        public ChuGiai(string ten, Color mau, string glyph) { Ten = ten; Mau = mau; Kieu = KieuKyHieu.IconTron; Glyph = glyph; }
    }

    /// <summary>Một lớp trong danh sách: bật/tắt, ký hiệu, số đối tượng / trạng thái tải, chú giải con.</summary>
    public class DongLop
    {
        public string Khoa;
        public string Ten;
        public string Nhom;
        public bool Hien = true;
        public ChuGiai KyHieu;
        public List<ChuGiai> ChuGiai = new List<ChuGiai>();
        public bool MoChuGiai = true;
        /// <summary>null = bình thường; "Đang tải…", "Lỗi tải"… hiển thị thay số đối tượng.</summary>
        public string TrangThai;
        public int SoDoiTuong = -1;
        internal Rectangle VungDong, VungO, VungMui;
    }

    /// <summary>
    /// Panel "Lớp &amp; chú giải": lớp bản đồ nhóm theo chủ đề, mỗi lớp có ô bật/tắt, ký hiệu, số đối tượng
    /// (hoặc trạng thái đang tải / lỗi) và chú giải các loại ký hiệu bên dưới (thu/mở được).
    /// </summary>
    public class PanelLopBanDo : Panel
    {
        private readonly List<DongLop> _lop = new List<DongLop>();
        private readonly VungVe _ve;

        /// <summary>Người dùng bật/tắt một lớp (khoá, hiện).</summary>
        public event Action<string, bool> LopThayDoi;

        public PanelLopBanDo()
        {
            AutoScroll = true;
            BackColor = Theme.Surface;
            _ve = new VungVe(this) { Dock = DockStyle.Top };
            Controls.Add(_ve);
        }

        public DongLop Them(string khoa, string ten, string nhom, ChuGiai kyHieu, params ChuGiai[] chuGiai)
        {
            var d = new DongLop { Khoa = khoa, Ten = ten, Nhom = nhom, KyHieu = kyHieu, ChuGiai = chuGiai.ToList() };
            _lop.Add(d);
            _ve.CapNhat();
            return d;
        }

        public DongLop Lop(string khoa) => _lop.FirstOrDefault(l => l.Khoa == khoa);

        public IEnumerable<DongLop> CacLop => _lop;

        public void DatTrangThai(string khoa, string trangThai, int soDoiTuong = -1)
        {
            var l = Lop(khoa);
            if (l == null) return;
            l.TrangThai = trangThai;
            if (soDoiTuong >= 0) l.SoDoiTuong = soDoiTuong;
            _ve.Invalidate();
        }

        public void DatHien(string khoa, bool hien)
        {
            var l = Lop(khoa);
            if (l == null) return;
            l.Hien = hien;
            _ve.Invalidate();
        }

        /// <summary>Đặt trạng thái cho mọi lớp (vd "Đang tải…" khi nạp lại bản đồ).</summary>
        public void DatTrangThaiTatCa(string trangThai)
        {
            foreach (var l in _lop) l.TrangThai = trangThai;
            _ve.Invalidate();
        }

        /// <summary>Vùng vẽ bên trong (chiều cao theo nội dung để Panel cha cuộn).</summary>
        private class VungVe : Control
        {
            private readonly PanelLopBanDo _cha;
            private DongLop _hover;

            private static readonly Font FontNhom = new Font("Segoe UI Semibold", 8.25f);

            public VungVe(PanelLopBanDo cha)
            {
                _cha = cha;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
                BackColor = Theme.Surface;
            }

            private int CaoNhom => UiHelper.Dp(30);
            private int CaoLop => UiHelper.Dp(32);
            private int CaoChuGiai => UiHelper.Dp(22);

            /// <summary>Tính lại vị trí các dòng và chiều cao tổng.</summary>
            public void CapNhat()
            {
                int y = UiHelper.Dp(4);
                string nhomTruoc = null;
                foreach (var l in _cha._lop)
                {
                    if (l.Nhom != nhomTruoc) { y += CaoNhom; nhomTruoc = l.Nhom; }
                    l.VungDong = new Rectangle(0, y, Width, CaoLop);
                    l.VungMui = l.ChuGiai.Count > 0 ? new Rectangle(UiHelper.Dp(2), y, UiHelper.Dp(20), CaoLop) : Rectangle.Empty;
                    l.VungO = new Rectangle(UiHelper.Dp(24), y + (CaoLop - UiHelper.Dp(16)) / 2, UiHelper.Dp(16), UiHelper.Dp(16));
                    y += CaoLop;
                    if (l.MoChuGiai) y += l.ChuGiai.Count * CaoChuGiai;
                }
                Height = y + UiHelper.Dp(12);
                Invalidate();
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                foreach (var l in _cha._lop) l.VungDong.Width = Width;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                string nhomTruoc = null;
                foreach (var l in _cha._lop)
                {
                    if (l.Nhom != nhomTruoc)
                    {
                        nhomTruoc = l.Nhom;
                        TextRenderer.DrawText(g, l.Nhom.ToUpperInvariant(), FontNhom,
                            new Rectangle(UiHelper.Dp(6), l.VungDong.Top - CaoNhom, Width - UiHelper.Dp(12), CaoNhom - UiHelper.Dp(4)),
                            Theme.Disabled, TextFormatFlags.Bottom | TextFormatFlags.Left | TextFormatFlags.NoPrefix);
                    }
                    VeDong(g, l);
                }
            }

            private void VeDong(Graphics g, DongLop l)
            {
                var r = l.VungDong;
                if (l == _hover)
                    using (var path = UiHelper.DuongBoGoc(new Rectangle(r.X + 1, r.Y + 1, r.Width - 2, r.Height - 2), Theme.BoGoc))
                    using (var br = new SolidBrush(Theme.SurfaceHover))
                        g.FillPath(br, path);

                // mũi tên thu/mở chú giải
                if (l.ChuGiai.Count > 0)
                    using (var f = Theme.FontIcon(7f))
                        TextRenderer.DrawText(g, l.MoChuGiai ? Icons.Xuong : Icons.Phai, f, l.VungMui, Theme.Muted,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

                // ô đánh dấu
                var o = l.VungO;
                using (var path = UiHelper.DuongBoGoc(o, UiHelper.Dp(3)))
                {
                    if (l.Hien)
                    {
                        using (var br = new SolidBrush(Theme.Accent)) g.FillPath(br, path);
                        using (var f = Theme.FontIcon(8f))
                            TextRenderer.DrawText(g, Icons.ThanhCong, f, o, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                    }
                    else
                    {
                        using (var br = new SolidBrush(Theme.Surface)) g.FillPath(br, path);
                        using (var pen = new Pen(Theme.BorderStrong, 1.5f)) g.DrawPath(pen, path);
                    }
                }

                // ký hiệu
                var rKh = new Rectangle(o.Right + UiHelper.Dp(10), r.Y + (r.Height - UiHelper.Dp(16)) / 2, UiHelper.Dp(18), UiHelper.Dp(16));
                if (l.KyHieu != null) VeKyHieu(g, l.KyHieu, rKh, l.Hien);

                // tên + số đối tượng / trạng thái
                string phai = l.TrangThai ?? (l.SoDoiTuong >= 0 ? l.SoDoiTuong.ToString("N0") : "");
                var mauPhai = l.TrangThai == null ? Theme.Muted : l.TrangThai.StartsWith("Lỗi") ? Theme.Danger : Theme.Accent;
                int rongPhai = string.IsNullOrEmpty(phai) ? 0 : TextRenderer.MeasureText(phai, Theme.FontNho).Width + UiHelper.Dp(4);
                var rTen = new Rectangle(rKh.Right + UiHelper.Dp(8), r.Y, r.Right - rKh.Right - UiHelper.Dp(16) - rongPhai, r.Height);
                TextRenderer.DrawText(g, l.Ten, Theme.FontThuong, rTen, l.Hien ? Theme.Text : Theme.Muted,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                if (rongPhai > 0)
                    TextRenderer.DrawText(g, phai, Theme.FontNho, new Rectangle(r.Right - rongPhai - UiHelper.Dp(8), r.Y, rongPhai, r.Height), mauPhai,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.NoPrefix);

                // chú giải con
                if (!l.MoChuGiai) return;
                int y = r.Bottom;
                foreach (var cg in l.ChuGiai)
                {
                    var rk = new Rectangle(rKh.X + UiHelper.Dp(4), y + (CaoChuGiai - UiHelper.Dp(14)) / 2, UiHelper.Dp(14), UiHelper.Dp(14));
                    VeKyHieu(g, cg, rk, l.Hien);
                    TextRenderer.DrawText(g, cg.Ten, Theme.FontNhan,
                        new Rectangle(rk.Right + UiHelper.Dp(10), y, Width - rk.Right - UiHelper.Dp(16), CaoChuGiai),
                        l.Hien ? Theme.Secondary : Theme.Disabled, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                    y += CaoChuGiai;
                }
            }

            /// <summary>Vẽ ký hiệu chú giải giống ký hiệu trên bản đồ.</summary>
            internal static void VeKyHieu(Graphics g, ChuGiai k, Rectangle r, bool sang)
            {
                var mau = sang ? k.Mau : Color.FromArgb(110, k.Mau);
                int s = Math.Min(r.Width, r.Height);
                var o = new Rectangle(r.X + (r.Width - s) / 2, r.Y + (r.Height - s) / 2, s, s);
                int cy = r.Y + r.Height / 2;
                using (var br = new SolidBrush(mau))
                using (var vien = new Pen(Color.White, 1.2f))
                {
                    switch (k.Kieu)
                    {
                        case KieuKyHieu.Tron:
                            o.Inflate(-2, -2);
                            g.FillEllipse(br, o);
                            g.DrawEllipse(vien, o);
                            break;
                        case KieuKyHieu.Vuong:
                            o.Inflate(-2, -2);
                            g.FillRectangle(br, o);
                            break;
                        case KieuKyHieu.TamGiac:
                            g.FillPolygon(br, new[] { new Point(o.X + s / 2, o.Y + 1), new Point(o.Right - 1, o.Bottom - 2), new Point(o.X + 1, o.Bottom - 2) });
                            break;
                        case KieuKyHieu.Thoi:
                            g.FillPolygon(br, new[] { new Point(o.X + s / 2, o.Y + 1), new Point(o.Right - 1, o.Y + s / 2), new Point(o.X + s / 2, o.Bottom - 1), new Point(o.X + 1, o.Y + s / 2) });
                            break;
                        case KieuKyHieu.ChuThap:
                            int t = Math.Max(3, s / 3);
                            g.FillRectangle(br, o.X + (s - t) / 2, o.Y + 1, t, s - 2);
                            g.FillRectangle(br, o.X + 1, o.Y + (s - t) / 2, s - 2, t);
                            break;
                        case KieuKyHieu.Duong:
                        case KieuKyHieu.DuongDut:
                        case KieuKyHieu.DuongCham:
                            using (var pen = new Pen(mau, k.Kieu == KieuKyHieu.Duong ? 3f : 1.6f))
                            {
                                if (k.Kieu == KieuKyHieu.DuongDut) pen.DashStyle = DashStyle.Dash;
                                if (k.Kieu == KieuKyHieu.DuongCham) pen.DashStyle = DashStyle.Dot;
                                g.DrawLine(pen, r.X, cy, r.Right, cy);
                            }
                            break;
                        case KieuKyHieu.XeBus:
                            // giống ký hiệu trạm trên bản đồ khi phóng to (Map.KyHieuTram)
                            using (var bmp = XeBus_GIS.Map.KyHieuTram.Ve(mau, Color.White, s))
                                g.DrawImage(bmp, o.X, o.Y, s, s);
                            break;
                        case KieuKyHieu.IconTron:
                            // giống ký hiệu công trình trên bản đồ (Map.KyHieuCongTrinh)
                            using (var bmp = XeBus_GIS.Map.KyHieuTram.Ve(mau, Color.White, s, k.Glyph, tron: true))
                                g.DrawImage(bmp, o.X, o.Y, s, s);
                            break;
                        case KieuKyHieu.Vung:
                            var rv = new Rectangle(r.X + 1, r.Y + 2, r.Width - 2, r.Height - 4);
                            using (var brv = new SolidBrush(Color.FromArgb(sang ? 150 : 70, k.Mau)))
                                g.FillRectangle(brv, rv);
                            using (var pen = new Pen(Color.FromArgb(sang ? 255 : 110, ControlPaint.Dark(k.Mau, 0.1f))))
                                g.DrawRectangle(pen, rv);
                            break;
                    }
                }
            }

            private DongLop DongTaiDiem(Point p) => _cha._lop.FirstOrDefault(l => l.VungDong.Contains(p));

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                var h = DongTaiDiem(e.Location);
                Cursor = h != null ? Cursors.Hand : Cursors.Default;
                if (h != _hover) { _hover = h; Invalidate(); }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                _hover = null;
                Invalidate();
            }

            protected override void OnMouseClick(MouseEventArgs e)
            {
                base.OnMouseClick(e);
                var l = DongTaiDiem(e.Location);
                if (l == null || e.Button != MouseButtons.Left) return;
                if (!l.VungMui.IsEmpty && e.X < l.VungO.Left)
                {
                    l.MoChuGiai = !l.MoChuGiai;
                    CapNhat();
                    return;
                }
                l.Hien = !l.Hien;
                Invalidate();
                _cha.LopThayDoi?.Invoke(l.Khoa, l.Hien);
            }
        }

        /// <summary>Vẽ một ký hiệu chú giải (dùng lại ở các module, vd chú giải kết quả phân tích).</summary>
        public static void VeKyHieu(Graphics g, ChuGiai k, Rectangle r) => VungVe.VeKyHieu(g, k, r, true);

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            _ve?.CapNhat();
        }
    }
}
