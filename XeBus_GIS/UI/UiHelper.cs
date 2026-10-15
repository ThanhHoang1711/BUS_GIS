using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using XeBus_GIS.UI.Controls;

namespace XeBus_GIS.UI
{
    /// <summary>Áp dụng design system cho control WinForms + tiện ích thông báo / chạy tác vụ nền.</summary>
    public static class UiHelper
    {
        private static readonly ToolTip _tip = new ToolTip { InitialDelay = 350, ReshowDelay = 100 };

        /// <summary>Form chính đăng ký để bật/tắt thanh tiến trình khi có tác vụ nền (ChayAsync).</summary>
        public static Action<bool> TienTrinh;
        private static int _soTacVu;

        // =====================================================================
        // DPI
        // =====================================================================
        private static float _heSoDpi = -1;

        /// <summary>Hệ số DPI so với 96 (1.0 = 100%).</summary>
        public static float HeSoDpi
        {
            get
            {
                if (_heSoDpi < 0)
                {
                    try { using (var g = Graphics.FromHwnd(IntPtr.Zero)) _heSoDpi = g.DpiX / 96f; }
                    catch { _heSoDpi = 1f; }
                }
                return _heSoDpi;
            }
        }

        /// <summary>Đổi kích thước thiết kế (px ở 96 DPI) sang px thực tế.</summary>
        public static int Dp(int px) => (int)Math.Round(px * HeSoDpi);

        private static readonly ConditionalWeakTable<Control, object> _daCoGianDpi = new ConditionalWeakTable<Control, object>();

        /// <summary>
        /// Co giãn form/UserControl dựng bằng Designer theo DPI – gọi NGAY sau InitializeComponent() (trước khi code dựng thêm
        /// control bằng Dp()). Designer đặt AutoScaleMode.None + toạ độ thiết kế ở 96 DPI.
        /// Không dùng auto-scale của WinForms: khi control gốc có Font khác mặc định (mọi Designer ở đây đặt Segoe UI 9.75),
        /// WinForms scale Label/LinkLabel/ListBox HAI lần (×2,25 ở 150%), còn chế độ Font thì không scale control con nào
        /// ⇒ chữ đè nhau trên màn hình 125–150%. Control tự lo DPI (UI.Controls dựng bằng Dp, UserControl đã tự co giãn)
        /// chỉ được nhân vị trí/kích thước, không đi vào bên trong để tránh nhân hai lần.
        /// </summary>
        public static void CoGianDpi(Control goc)
        {
            _daCoGianDpi.GetValue(goc, _ => new object());
            float k = HeSoDpi;
            if (Math.Abs(k - 1f) < 0.01f) return;

            var tach = new List<Tuple<Control, Control, int>>();
            TimControlTuLoDpi(goc, goc, tach);
            goc.SuspendLayout();
            try
            {
                foreach (var t in tach) t.Item1.Controls.Remove(t.Item2);
                goc.Scale(new SizeF(k, k));
                tach.Sort((a, b) => a.Item3.CompareTo(b.Item3));
                foreach (var t in tach)
                {
                    var c = t.Item2;
                    c.SetBounds((int)Math.Round(c.Left * k), (int)Math.Round(c.Top * k),
                                (int)Math.Round(c.Width * k), (int)Math.Round(c.Height * k));
                    var m = c.Margin;
                    c.Margin = new Padding((int)Math.Round(m.Left * k), (int)Math.Round(m.Top * k), (int)Math.Round(m.Right * k), (int)Math.Round(m.Bottom * k));
                    t.Item1.Controls.Add(c);
                    t.Item1.Controls.SetChildIndex(c, Math.Min(t.Item3, t.Item1.Controls.Count - 1));
                }
            }
            finally { goc.ResumeLayout(true); }
        }

        private static void TimControlTuLoDpi(Control goc, Control cha, List<Tuple<Control, Control, int>> ds)
        {
            foreach (Control c in cha.Controls)
            {
                bool tuLo = _daCoGianDpi.TryGetValue(c, out _) || c.GetType().Namespace == typeof(ThanhDieuHuong).Namespace;
                if (tuLo) ds.Add(Tuple.Create(cha, c, cha.Controls.GetChildIndex(c)));
                else TimControlTuLoDpi(goc, c, ds);
            }
        }

        // =====================================================================
        // Nút – trạng thái hover / pressed / focus / disabled thống nhất
        // =====================================================================
        private enum KieuNut { Chinh, Phu, NguyHiem, Icon, NguyHiemChinh }

        /// <summary>Trạng thái gốc của nút để khôi phục sau disabled / focus.</summary>
        private class TrangThaiNut
        {
            public KieuNut Kieu;
            public Color Nen, Chu, Vien;
            public int DoDayVien;
            public bool DangBat;
        }

        private static readonly ConditionalWeakTable<Button, TrangThaiNut> _nut = new ConditionalWeakTable<Button, TrangThaiNut>();

        /// <summary>Nút hành động chính (nền Accent, chữ trắng).</summary>
        public static void NutChinh(Button b)
        {
            NutCoBan(b);
            b.Font = Theme.FontDam;
            b.FlatAppearance.MouseOverBackColor = Theme.AccentHover;
            b.FlatAppearance.MouseDownBackColor = Theme.AccentPressed;
            GanTrangThai(b, KieuNut.Chinh, Theme.Accent, Theme.ChuTrenAccent, Theme.Accent, 0);
            ThemeManager.DangKy(b, "nut", () => NutChinh(b));
        }

        /// <summary>Nút chính với bộ màu riêng (vd trang đăng nhập dùng tông than chì thay Accent).</summary>
        public static void NutChinh(Button b, Color nen, Color nenHover, Color nenNhan)
        {
            NutCoBan(b);
            b.Font = Theme.FontDam;
            b.FlatAppearance.MouseOverBackColor = nenHover;
            b.FlatAppearance.MouseDownBackColor = nenNhan;
            // chữ trắng trên nền tối, chữ than chì trên nền sáng
            var chu = nen.GetBrightness() > 0.6f ? ColorTranslator.FromHtml("#111827") : Color.White;
            GanTrangThai(b, KieuNut.Chinh, nen, chu, nen, 0);
        }

        /// <summary>
        /// Bọc TextBox trong khung tự vẽ cao 38 px (viền mảnh, đổi màu khi đang nhập), tuỳ chọn một nút nằm trong khung
        /// ở mép phải (vd hiện mật khẩu) – các ô có/không có nút đều dài bằng nhau. Khung thay vị trí của TextBox.
        /// </summary>
        public static Panel KhungO(TextBox t, Color mauFocus, Control nutPhai = null)
        {
            var cha = t.Parent;
            int le = Dp(10), cao = Dp(38);
            var k = new Panel
            {
                Bounds = new Rectangle(t.Left, t.Top, t.Width, cao),
                BackColor = Theme.Surface,
                Anchor = t.Anchor,
                TabIndex = t.TabIndex,
                Cursor = Cursors.IBeam,
            };
            int chiSo = cha.Controls.GetChildIndex(t);
            cha.Controls.Add(k);
            cha.Controls.SetChildIndex(k, chiSo);
            t.BorderStyle = BorderStyle.None;
            t.Parent = k;
            int rongNut = 0;
            if (nutPhai != null)
            {
                rongNut = Dp(34);
                nutPhai.Parent = k;
                nutPhai.Bounds = new Rectangle(k.Width - rongNut - Dp(3), Dp(3), rongNut, cao - Dp(6));
                nutPhai.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                nutPhai.BackColor = Theme.Surface;
            }
            t.Bounds = new Rectangle(le, (cao - t.Height) / 2, k.Width - le - (rongNut > 0 ? rongNut + Dp(6) : le), t.Height);
            t.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            bool dangNhap = false;
            t.GotFocus += (s, e) => { dangNhap = true; k.Invalidate(); };
            t.LostFocus += (s, e) => { dangNhap = false; k.Invalidate(); };
            k.Click += (s, e) => t.Focus();
            k.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new RectangleF(0.5f, 0.5f, k.Width - 1.5f, k.Height - 1.5f);
                using (var path = DuongBoGoc(r, Dp(6)))
                using (var pen = new Pen(dangNhap ? mauFocus : Theme.BorderStrong, dangNhap ? 1.6f : 1f))
                    g.DrawPath(pen, path);
                g.SmoothingMode = SmoothingMode.None;
            };
            return k;
        }

        /// <summary>Nút phụ (nền trắng, viền).</summary>
        public static void NutPhu(Button b)
        {
            NutCoBan(b);
            b.Font = Theme.FontThuong;
            b.FlatAppearance.MouseOverBackColor = Theme.SurfaceHover;
            b.FlatAppearance.MouseDownBackColor = Theme.Border;
            GanTrangThai(b, KieuNut.Phu, Theme.Surface, Theme.Secondary, Theme.BorderStrong, 1);
            ThemeManager.DangKy(b, "nut", () => NutPhu(b));
        }

        /// <summary>Nút nguy hiểm dạng phụ (xoá) – chữ đỏ, viền.</summary>
        public static void NutNguyHiem(Button b)
        {
            NutCoBan(b);
            b.Font = Theme.FontThuong;
            b.FlatAppearance.MouseOverBackColor = Theme.DangerSoft;
            b.FlatAppearance.MouseDownBackColor = Theme.DangerSoftPressed;
            GanTrangThai(b, KieuNut.NguyHiem, Theme.Surface, Theme.Danger, Theme.DangerBorder, 1);
            ThemeManager.DangKy(b, "nut", () => NutNguyHiem(b));
        }

        /// <summary>Nút nguy hiểm dạng chính (xác nhận xoá trong hộp thoại).</summary>
        public static void NutNguyHiemChinh(Button b)
        {
            NutCoBan(b);
            b.Font = Theme.FontDam;
            b.FlatAppearance.MouseOverBackColor = Theme.DangerHover;
            b.FlatAppearance.MouseDownBackColor = Theme.DangerHover;
            GanTrangThai(b, KieuNut.NguyHiemChinh, Theme.Danger, Theme.Toi ? Theme.ChuTrenAccent : Color.White, Theme.Danger, 0);
            ThemeManager.DangKy(b, "nut", () => NutNguyHiemChinh(b));
        }

        /// <summary>Nút chỉ có icon (toolbar) – bắt buộc có tooltip mô tả.</summary>
        public static void NutIcon(Button b, string glyph, string moTa, float coChu = 12f)
        {
            NutCoBan(b);
            b.Text = glyph;
            b.Font = Theme.FontIcon(coChu);
            b.FlatAppearance.MouseOverBackColor = Theme.SurfaceHover;
            b.FlatAppearance.MouseDownBackColor = Theme.Border;
            b.AccessibleName = moTa;
            _tip.SetToolTip(b, moTa);
            var nen = b.Parent != null && b.Parent.BackColor.ToArgb() == Theme.Background.ToArgb() ? Theme.Background : Theme.Surface;
            GanTrangThai(b, KieuNut.Icon, nen, Theme.Secondary, nen, 0);
            ThemeManager.DangKy(b, "nut", () => NutIcon(b, glyph, moTa, coChu));
        }

        /// <summary>Đánh dấu nút công cụ đang bật (nền xanh nhạt, icon xanh).</summary>
        public static void DatTrangThaiBat(Button b, bool bat)
        {
            if (_nut.TryGetValue(b, out var t)) t.DangBat = bat;
            ApMau(b);
        }

        private static void NutCoBan(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
            b.TextAlign = ContentAlignment.MiddleCenter;
        }

        private static void GanTrangThai(Button b, KieuNut kieu, Color nen, Color chu, Color vien, int doDay)
        {
            bool moi = !_nut.TryGetValue(b, out var t);
            if (moi)
            {
                t = new TrangThaiNut();
                _nut.Add(b, t);
                b.EnabledChanged += (s, e) => ApMau(b);
                b.GotFocus += (s, e) =>
                {
                    // chỉ hiện vòng focus khi focus bằng bàn phím (Tab) – click chuột thì không
                    if (Control.MouseButtons == MouseButtons.None) VeVongFocus(b, true);
                };
                b.LostFocus += (s, e) => VeVongFocus(b, false);
                b.MouseDown += (s, e) => VeVongFocus(b, false);
            }
            t.Kieu = kieu;
            t.Nen = nen;
            t.Chu = chu;
            t.Vien = vien;
            t.DoDayVien = doDay;
            ApMau(b);
        }

        private static void ApMau(Button b)
        {
            if (!_nut.TryGetValue(b, out var t)) return;
            if (!b.Enabled)
            {
                bool chinh = t.Kieu == KieuNut.Chinh || t.Kieu == KieuNut.NguyHiemChinh;
                b.BackColor = chinh ? Theme.SurfaceHover : (t.Kieu == KieuNut.Icon ? t.Nen : Theme.SurfaceAlt);
                b.ForeColor = Theme.Disabled;
                b.FlatAppearance.BorderColor = chinh || t.DoDayVien > 0 ? Theme.Border : b.BackColor;
                b.FlatAppearance.BorderSize = chinh ? 1 : t.DoDayVien;
                b.Cursor = Cursors.Default;
                return;
            }
            b.Cursor = Cursors.Hand;
            if (t.DangBat)
            {
                b.BackColor = Theme.AccentSoft;
                b.ForeColor = Theme.Accent;
                b.FlatAppearance.MouseOverBackColor = Theme.AccentSoftHover;
            }
            else
            {
                b.BackColor = t.Nen;
                b.ForeColor = t.Chu;
                if (t.Kieu == KieuNut.Icon) b.FlatAppearance.MouseOverBackColor = Theme.SurfaceHover;
            }
            b.FlatAppearance.BorderColor = t.Vien;
            b.FlatAppearance.BorderSize = t.DoDayVien;
        }

        private static void VeVongFocus(Button b, bool hien)
        {
            if (!_nut.TryGetValue(b, out var t) || !b.Enabled) return;
            if (hien)
            {
                b.FlatAppearance.BorderSize = 2;
                b.FlatAppearance.BorderColor = t.Kieu == KieuNut.Chinh ? Theme.AccentPressed
                    : t.Kieu == KieuNut.NguyHiemChinh ? Theme.DangerHover : Theme.Accent;
            }
            else
            {
                b.FlatAppearance.BorderSize = t.DoDayVien;
                b.FlatAppearance.BorderColor = t.Vien;
            }
        }

        public static void TipCho(Control c, string moTa) => _tip.SetToolTip(c, moTa);

        private static readonly ConditionalWeakTable<Button, bool[]> _tab = new ConditionalWeakTable<Button, bool[]>();

        /// <summary>Nút dạng tab gạch chân (tab con trong module): chữ xám, tab đang chọn chữ xanh + gạch chân.</summary>
        public static void Tab(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Theme.SurfaceHover;
            b.FlatAppearance.MouseDownBackColor = Theme.Border;
            b.BackColor = Theme.Surface;
            b.UseVisualStyleBackColor = false;
            b.Cursor = Cursors.Hand;
            if (_tab.TryGetValue(b, out var daCo)) { ChonTab(b, daCo[0]); return; }
            _tab.Add(b, new[] { false });
            ThemeManager.DangKy(b, "tab", () => Tab(b));
            b.Paint += (s, e) =>
            {
                if (!_tab.TryGetValue(b, out var chon) || !chon[0]) return;
                using (var br = new SolidBrush(Theme.Accent))
                    e.Graphics.FillRectangle(br, Dp(6), b.Height - Dp(3), b.Width - Dp(12), Dp(3));
            };
            ChonTab(b, false);
        }

        public static void ChonTab(Button b, bool chon)
        {
            if (_tab.TryGetValue(b, out var st)) st[0] = chon;
            b.ForeColor = chon ? Theme.Accent : Theme.Muted;
            b.Font = chon ? Theme.FontDam : Theme.FontThuong;
            b.Invalidate();
        }

        private static readonly Dictionary<string, Bitmap> _anhIcon = new Dictionary<string, Bitmap>();

        /// <summary>Vẽ glyph icon thành ảnh (để đặt trước chữ của nút). Ảnh được dùng lại theo glyph + màu + cỡ.</summary>
        public static Bitmap AnhIcon(string glyph, Color mau, float coChu = 10f)
        {
            string khoa = glyph + "|" + mau.ToArgb() + "|" + coChu;
            if (_anhIcon.TryGetValue(khoa, out var b)) return b;
            int s = Dp((int)Math.Round(coChu * 1.8f));
            b = new Bitmap(s, s);
            using (var g = Graphics.FromImage(b))
            using (var f = Theme.FontIcon(coChu))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                TextRenderer.DrawText(g, glyph, f, new Rectangle(0, 0, s, s), mau, Color.Transparent,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
            _anhIcon[khoa] = b;
            return b;
        }

        /// <summary>Thêm icon nhỏ trước chữ của nút (màu theo màu chữ hiện tại của nút).</summary>
        public static void GanIcon(Button b, string glyph, float coChu = 10f)
        {
            ThemeManager.DangKy(b, "icon", () => b.Image = AnhIcon(glyph, b.Enabled ? b.ForeColor : Theme.Disabled, coChu));
            b.Image = AnhIcon(glyph, b.ForeColor, coChu);
            b.ImageAlign = ContentAlignment.MiddleCenter;
            b.TextImageRelation = TextImageRelation.ImageBeforeText;
        }

        // =====================================================================
        // Ô nhập / nhãn / tiêu đề
        // =====================================================================
        public static void NhanTruong(Label l)
        {
            l.Font = Theme.FontNhan;
            l.ForeColor = Theme.Muted;
            l.AutoSize = true;
        }

        public static void TieuDe(Label l)
        {
            l.Font = Theme.FontTieuDe;
            l.ForeColor = Theme.Primary;
            l.AutoSize = true;
        }

        /// <summary>Tiêu đề một khu vực trong module (vd "Tra cứu theo thuộc tính").</summary>
        public static void TieuDeKhuVuc(Label l)
        {
            l.Font = Theme.FontDam;
            l.ForeColor = Theme.Primary;
        }

        /// <summary>Chữ phụ / chú thích (đếm kết quả, giải thích ngắn).</summary>
        public static void ChuPhu(Label l)
        {
            l.Font = Theme.FontNhan;
            l.ForeColor = Theme.Muted;
        }

        /// <summary>Chuẩn hoá ô nhập (font, màu).</summary>
        public static void O(params Control[] cs)
        {
            foreach (var c in cs)
            {
                c.Font = Theme.FontThuong;
                if (c is TextBox t) t.BorderStyle = BorderStyle.FixedSingle;
                MauO(c);
                ThemeManager.DangKy(c, "o", () => MauO(c));
            }
        }

        /// <summary>
        /// Áp kiểu ô nhập cho mọi TextBox / ComboBox / NumericUpDown trong cây control chưa được chuẩn hoá
        /// (bỏ qua ô không viền – vd ô nằm trong khung tự vẽ KhungO hoặc ô tìm nhanh).
        /// </summary>
        public static void ChuanHoaO(Control goc)
        {
            foreach (Control c in goc.Controls)
            {
                if ((c is TextBox t && t.BorderStyle != BorderStyle.None) || c is ComboBox || c is NumericUpDown)
                {
                    var f = c.Font;
                    O(c);
                    if (f.Size != c.Font.Size) c.Font = f;   // giữ cỡ chữ thiết kế sẵn (vd ô to hơn)
                }
                else if (!(c is DataGridView)) ChuanHoaO(c);
            }
        }

        /// <summary>
        /// Màu ô nhập theo chế độ. ComboBox: sáng dùng kiểu hệ thống (gọn, quen mắt), tối phải dùng Flat vì kiểu hệ thống
        /// luôn vẽ nền trắng.
        /// </summary>
        private static void MauO(Control c)
        {
            c.ForeColor = Theme.Text;
            if (c is TextBox tb && !tb.Multiline) VienTextBox(tb, Theme.Toi);
            if (c is ComboBox cb)
            {
                cb.FlatStyle = Theme.Toi ? FlatStyle.Flat : FlatStyle.System;
                cb.BackColor = Theme.Surface;
            }
            else if (c is TextBox || c is NumericUpDown) c.BackColor = Theme.Surface;
        }

        private static readonly ConditionalWeakTable<TextBox, object> _oTuVe = new ConditionalWeakTable<TextBox, object>();
        private static readonly ConditionalWeakTable<Control, object> _chaVeVien = new ConditionalWeakTable<Control, object>();

        /// <summary>
        /// Viền ô nhập: chế độ sáng dùng viền hệ thống (FixedSingle); chế độ tối viền hệ thống luôn sáng chói ⇒ bỏ viền
        /// và để control cha vẽ khung mảnh theo theme (đổi màu khi ô đang nhập).
        /// </summary>
        private static void VienTextBox(TextBox t, bool tuVe)
        {
            if (!tuVe)
            {
                if (_oTuVe.TryGetValue(t, out _)) { _oTuVe.Remove(t); t.BorderStyle = BorderStyle.FixedSingle; t.Parent?.Invalidate(); }
                return;
            }
            if (_oTuVe.TryGetValue(t, out _)) return;
            _oTuVe.Add(t, null);
            t.BorderStyle = BorderStyle.None;
            var cha = t.Parent;
            if (cha == null) return;
            t.GotFocus += (s, e) => t.Parent?.Invalidate();
            t.LostFocus += (s, e) => t.Parent?.Invalidate();
            t.VisibleChanged += (s, e) => t.Parent?.Invalidate();
            if (_chaVeVien.TryGetValue(cha, out _)) { cha.Invalidate(); return; }
            _chaVeVien.Add(cha, null);
            cha.Paint += (s, e) =>
            {
                foreach (Control con in cha.Controls)
                {
                    if (!(con is TextBox x) || !x.Visible || !_oTuVe.TryGetValue(x, out _)) continue;
                    var r = Rectangle.Inflate(x.Bounds, Dp(2), Dp(2));
                    using (var pen = new Pen(x.Focused ? Theme.Accent : Theme.BorderStrong))
                        e.Graphics.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
                }
            };
            cha.Invalidate();
        }

        /// <summary>Kẻ một đường mảnh ở đáy panel (phân tách các khu vực).</summary>
        public static void DuongKe(Panel p, int le = 12)
        {
            p.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.Border))
                    e.Graphics.DrawLine(pen, le, p.Height - 1, p.Width - le, p.Height - 1);
            };
        }

        /// <summary>Kẻ viền mảnh quanh panel (thay BorderStyle.FixedSingle màu hệ thống).</summary>
        public static void VienMong(Panel p)
        {
            p.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };
        }

        /// <summary>
        /// Co giãn ngang các control đặt vị trí tuyệt đối trong khung (thiết kế cho chiều rộng hiện tại):
        /// khi khung rộng/hẹp lại (kéo thanh chia), vị trí X và chiều rộng được nhân theo tỉ lệ nên bố cục
        /// hai cột vẫn thẳng hàng. Nhãn AutoSize chỉ dời vị trí. Control Dock không bị ảnh hưởng.
        /// </summary>
        public static void CoGianNgang(Control khung)
        {
            int w0 = khung.Width, le = Dp(12);
            if (w0 <= 2 * le) return;
            var goc = new Dictionary<Control, Rectangle>();
            foreach (Control c in khung.Controls)
                if (c.Dock == DockStyle.None && (c.Anchor & AnchorStyles.Right) == 0) goc[c] = c.Bounds;
            khung.Resize += (s, e) =>
            {
                if (khung.Width <= 2 * le) return;
                float k = (khung.Width - 2f * le) / (w0 - 2f * le);
                foreach (var kv in goc)
                {
                    var b = kv.Value;
                    int x = le + (int)Math.Round((b.X - le) * k);
                    bool coDinhRong = kv.Key is Label l && l.AutoSize || kv.Key is CheckBox || kv.Key is RadioButton;
                    int w = coDinhRong ? b.Width : Math.Max(20, (int)Math.Round(b.Width * k));
                    kv.Key.SetBounds(x, 0, w, 0, BoundsSpecified.X | BoundsSpecified.Width);
                }
            };
        }

        /// <summary>
        /// Biến panel (Dock Top) thành khu vực thu/mở được: click tiêu đề (hoặc mũi tên bên phải) để ẩn nội dung,
        /// dành chỗ cho lưới kết quả bên dưới.
        /// </summary>
        public static Action<bool> KhuVucThuGon(Panel p, Label tieuDe, bool moBanDau = true)
        {
            int caoMo = p.Height;
            var mui = new Label
            {
                AutoSize = false,
                Size = new Size(Dp(28), Math.Max(Dp(22), tieuDe.Height)),
                Font = Theme.FontIcon(8f),
                ForeColor = Theme.Muted,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            mui.Location = new Point(p.Width - mui.Width - Dp(8), tieuDe.Top + (tieuDe.Height - mui.Height) / 2);
            p.Controls.Add(mui);
            mui.BringToFront();
            tieuDe.Cursor = Cursors.Hand;

            bool mo = true;
            var daAn = new List<Control>();   // control bị ẩn khi thu gọn (giữ nguyên control vốn đã ẩn do phân quyền)
            void Dat(bool m)
            {
                if (m == mo && daAn.Count == 0 && m) { mui.Text = Icons.Len; return; }
                mo = m;
                mui.Text = m ? Icons.Len : Icons.Xuong;
                if (m)
                {
                    foreach (var c in daAn) c.Visible = true;
                    daAn.Clear();
                }
                else
                {
                    foreach (Control c in p.Controls)
                        if (c != tieuDe && c != mui && DatHien(c)) { daAn.Add(c); c.Visible = false; }
                }
                p.Height = m ? caoMo : tieuDe.Bottom + Dp(10);
                _tip.SetToolTip(mui, m ? "Thu gọn" : "Mở rộng");
                _tip.SetToolTip(tieuDe, m ? "Click để thu gọn" : "Click để mở rộng");
            }
            tieuDe.Click += (s, e) => Dat(!mo);
            mui.Click += (s, e) => Dat(!mo);
            Dat(moBanDau);
            return Dat;
        }

        private static readonly System.Reflection.MethodInfo _getState =
            typeof(Control).GetMethod("GetState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                                      null, new[] { typeof(int) }, null);

        /// <summary>
        /// Control có được đặt Visible = true hay không (bản thân nó) – khác với thuộc tính Visible vốn trả false
        /// khi control cha chưa hiển thị.
        /// </summary>
        public static bool DatHien(Control c)
        {
            try { return _getState != null ? (bool)_getState.Invoke(c, new object[] { 0x2 }) : c.Visible; }
            catch { return c.Visible; }
        }

        /// <summary>Đường bo góc dùng cho các control tự vẽ.</summary>
        public static GraphicsPath DuongBoGoc(RectangleF r, float banKinh)
        {
            var p = new GraphicsPath();
            float d = Math.Min(banKinh * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        // =====================================================================
        // Lưới dữ liệu
        // =====================================================================
        public static void Luoi(DataGridView g)
        {
            g.BorderStyle = BorderStyle.None;
            g.EnableHeadersVisualStyles = false;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9f);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            g.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            g.ColumnHeadersHeight = Dp(32);
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.DefaultCellStyle.Font = Theme.FontThuong;
            g.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            MauLuoi(g);
            ThemeManager.DangKy(g, "luoi", () => { MauLuoi(g); g.Invalidate(); });
            HoverHang(g);
            g.RowTemplate.Height = Dp(28);
            g.RowHeadersVisible = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.MultiSelect = false;
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            BatDoubleBuffer(g);
        }

        /// <summary>Màu của lưới theo chế độ hiện tại (gọi lại khi đổi sáng/tối).</summary>
        private static void MauLuoi(DataGridView g)
        {
            g.BackgroundColor = Theme.Surface;
            g.GridColor = Theme.Border;
            g.ColumnHeadersDefaultCellStyle.BackColor = Theme.SurfaceAlt;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Muted;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.SurfaceAlt;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Theme.Muted;
            g.DefaultCellStyle.ForeColor = Theme.Text;
            g.DefaultCellStyle.BackColor = Theme.Surface;
            g.DefaultCellStyle.SelectionBackColor = Theme.RowSelect;
            g.DefaultCellStyle.SelectionForeColor = Theme.Text;
            g.AlternatingRowsDefaultCellStyle.BackColor = Theme.RowAlt;
            g.AlternatingRowsDefaultCellStyle.ForeColor = Theme.Text;
            g.AlternatingRowsDefaultCellStyle.SelectionBackColor = Theme.RowSelect;
            g.AlternatingRowsDefaultCellStyle.SelectionForeColor = Theme.Text;
        }

        private static readonly ConditionalWeakTable<DataGridView, int[]> _hangHover = new ConditionalWeakTable<DataGridView, int[]>();

        /// <summary>Tô nhẹ hàng đang rê chuột (không đổi lựa chọn).</summary>
        private static void HoverHang(DataGridView g)
        {
            if (_hangHover.TryGetValue(g, out _)) return;
            var h = new[] { -1 };
            _hangHover.Add(g, h);
            void Dat(int r)
            {
                if (r == h[0]) return;
                int cu = h[0];
                h[0] = r;
                if (cu >= 0 && cu < g.Rows.Count) g.InvalidateRow(cu);
                if (r >= 0 && r < g.Rows.Count) g.InvalidateRow(r);
            }
            g.CellMouseEnter += (s, e) => Dat(e.RowIndex);
            g.MouseLeave += (s, e) => Dat(-1);
            g.RowPrePaint += (s, e) =>
            {
                if (e.RowIndex != h[0] || e.RowIndex < 0 || e.RowIndex >= g.Rows.Count || g.Rows[e.RowIndex].Selected) return;
                using (var br = new SolidBrush(Theme.RowHover)) e.Graphics.FillRectangle(br, e.RowBounds);
                e.PaintParts &= ~DataGridViewPaintParts.Background;
            };
        }

        /// <summary>Bật double buffering cho lưới (giảm nháy khi cuộn) – thuộc tính protected nên dùng reflection.</summary>
        private static void BatDoubleBuffer(DataGridView g)
        {
            typeof(DataGridView).GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(g, true, null);
        }

        /// <summary>Các cột ngắn (mã, ngày…) rộng vừa nội dung – không bị cắt "TR0…".</summary>
        public static void CotVuaDu(DataGridView g, params string[] cot)
        {
            foreach (var c in cot)
                if (g.Columns.Contains(c)) g.Columns[c].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }

        /// <summary>Cột chiều rộng cố định (px ở 96 DPI) – dùng cho cột nhãn trạng thái.</summary>
        public static void CotCoDinh(DataGridView g, string cot, int rong)
        {
            if (!g.Columns.Contains(cot)) return;
            g.Columns[cot].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            g.Columns[cot].Width = Dp(rong);
        }

        /// <summary>Căn phải các cột số sau khi gán DataSource.</summary>
        public static void CanPhaiCotSo(DataGridView g)
        {
            foreach (DataGridViewColumn c in g.Columns)
            {
                var t = c.ValueType;
                if (t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(decimal) || t == typeof(double) || t == typeof(float))
                    c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        /// <summary>
        /// Vẽ giá trị các cột trạng thái thành nhãn màu bo góc (Hoạt động / Đề xuất / Ngừng hoạt động / Khóa…).
        /// tenCot so với Name, DataPropertyName hoặc HeaderText của cột.
        /// </summary>
        public static void NhanTrangThai(DataGridView g, params string[] tenCot)
        {
            var ds = new HashSet<string>(tenCot, StringComparer.OrdinalIgnoreCase);
            g.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                var cot = g.Columns[e.ColumnIndex];
                if (!ds.Contains(cot.Name) && !ds.Contains(cot.DataPropertyName ?? "") && !ds.Contains(cot.HeaderText ?? "")) return;
                var chu = Convert.ToString(e.Value);
                if (string.IsNullOrEmpty(chu)) return;

                e.PaintBackground(e.CellBounds, true);
                Theme.MauTrangThai(chu, out var nen, out var mauChu);
                var gr = e.Graphics;
                var smCu = gr.SmoothingMode;   // Graphics dùng chung cho cả lưới – phải trả lại trạng thái cũ
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                var kt = TextRenderer.MeasureText(gr, chu, Theme.FontNho, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                int cao = Math.Min(e.CellBounds.Height - 6, Dp(20));
                int rong = Math.Min(kt.Width + Dp(14), e.CellBounds.Width - Dp(10));
                var r = new Rectangle(e.CellBounds.X + Dp(6), e.CellBounds.Y + (e.CellBounds.Height - cao) / 2, rong, cao);
                using (var path = DuongBoGoc(r, cao / 2f))
                using (var br = new SolidBrush(nen))
                    gr.FillPath(br, path);
                TextRenderer.DrawText(gr, chu, Theme.FontNho, r, mauChu,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                gr.SmoothingMode = smCu;
                e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
                e.Handled = true;
            };
        }

        private static readonly ConditionalWeakTable<DataGridView, string[]> _chuRong = new ConditionalWeakTable<DataGridView, string[]>();

        /// <summary>
        /// Khi lưới không có dòng nào: vẽ thông báo mờ ở giữa (trạng thái rỗng). Đổi nội dung bằng <see cref="DatChuRong"/>.
        /// </summary>
        public static void TrangThaiRong(DataGridView g, string chu)
        {
            if (_chuRong.TryGetValue(g, out var cu)) { cu[0] = chu; g.Invalidate(); return; }
            _chuRong.Add(g, new[] { chu });
            g.Paint += (s, e) =>
            {
                if (g.Rows.Count > 0 || !_chuRong.TryGetValue(g, out var nd) || string.IsNullOrEmpty(nd[0])) return;
                int top = g.ColumnHeadersVisible ? g.ColumnHeadersHeight : 0;
                var r = new Rectangle(Dp(16), top + Dp(16), Math.Max(10, g.Width - Dp(32)), Math.Max(10, g.Height - top - Dp(32)));
                TextRenderer.DrawText(e.Graphics, nd[0], Theme.FontNhan, r, Theme.Muted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            };
            g.RowsAdded += (s, e) => g.Invalidate();
            g.RowsRemoved += (s, e) => g.Invalidate();
            g.DataBindingComplete += (s, e) => g.Invalidate();
        }

        public static void DatChuRong(DataGridView g, string chu) => TrangThaiRong(g, chu);

        // =====================================================================
        // Biểu đồ
        // =====================================================================
        /// <summary>Kiểu dáng biểu đồ thống nhất: nền trắng, lưới mảnh, chữ Muted, không viền 3D.</summary>
        public static void BieuDo(System.Windows.Forms.DataVisualization.Charting.Chart c)
        {
            c.BorderlineWidth = 0;
            c.AntiAliasing = System.Windows.Forms.DataVisualization.Charting.AntiAliasingStyles.All;
            c.TextAntiAliasingQuality = System.Windows.Forms.DataVisualization.Charting.TextAntiAliasingQuality.High;
            foreach (var a in c.ChartAreas)
            {
                foreach (var ax in new[] { a.AxisX, a.AxisY })
                {
                    ax.LabelStyle.Font = Theme.FontNho;
                    ax.TitleFont = Theme.FontNho;
                }
                a.AxisX.MajorGrid.Enabled = false;
                a.AxisX.MajorTickMark.Enabled = false;
                a.AxisX.Interval = 1;
            }
            foreach (var l in c.Legends) l.Font = Theme.FontNho;
            MauBieuDo(c);
            ThemeManager.DangKy(c, "bieudo", () => MauBieuDo(c));
        }

        /// <summary>Màu nền/trục/chữ của biểu đồ theo chế độ hiện tại (màu series giữ nguyên – mang nghĩa dữ liệu).</summary>
        public static void MauBieuDo(System.Windows.Forms.DataVisualization.Charting.Chart c)
        {
            c.BackColor = Theme.Surface;
            foreach (var a in c.ChartAreas)
            {
                a.BackColor = Theme.Surface;
                foreach (var ax in new[] { a.AxisX, a.AxisY })
                {
                    ax.LineColor = Theme.Border;
                    ax.MajorGrid.LineColor = Theme.Border;
                    ax.MajorTickMark.LineColor = Theme.Border;
                    ax.LabelStyle.ForeColor = Theme.Muted;
                    ax.TitleForeColor = Theme.Muted;
                }
            }
            foreach (var l in c.Legends)
            {
                l.BackColor = Theme.Surface;
                l.ForeColor = Theme.Secondary;
            }
            foreach (var t in c.Titles) t.ForeColor = Theme.Primary;
            foreach (var s in c.Series) s.LabelForeColor = Theme.Secondary;
        }

        // =====================================================================
        // Thông báo
        // =====================================================================
        /// <summary>Lỗi: hộp thoại (chặn) – người dùng cần đọc và xử lý.</summary>
        public static void Loi(IWin32Window owner, string noiDung) =>
            HopThoai.Hien(owner, "Không thực hiện được", noiDung, KieuHopThoai.Loi, "Đóng", null);

        /// <summary>Thông báo thường: toast không chặn ở góc dưới phải cửa sổ chính.</summary>
        public static void ThongBao(IWin32Window owner, string noiDung) =>
            Toast.Hien(owner, noiDung, KieuThongBao.ThongTin);

        /// <summary>Thông báo thành công (toast màu xanh lá).</summary>
        public static void ThanhCong(IWin32Window owner, string noiDung) =>
            Toast.Hien(owner, noiDung, KieuThongBao.ThanhCong);

        /// <summary>Cảnh báo nhẹ (toast màu vàng) – thiếu điều kiện, chưa chọn đối tượng…</summary>
        public static void CanhBao(IWin32Window owner, string noiDung) =>
            Toast.Hien(owner, noiDung, KieuThongBao.CanhBao);

        public static bool XacNhan(IWin32Window owner, string noiDung, string nutDongY = "Đồng ý", string nutHuy = "Huỷ") =>
            HopThoai.Hien(owner, "Xác nhận", noiDung, KieuHopThoai.CauHoi, nutDongY, nutHuy) == DialogResult.OK;

        /// <summary>Xác nhận thao tác không hoàn tác được (xoá…) – nút chính màu đỏ.</summary>
        public static bool XacNhanNguyHiem(IWin32Window owner, string noiDung, string nutDongY = "Xoá") =>
            HopThoai.Hien(owner, "Xác nhận", noiDung, KieuHopThoai.NguyHiem, nutDongY, "Huỷ") == DialogResult.OK;

        // =====================================================================
        // Tác vụ nền
        // =====================================================================
        /// <summary>
        /// Chạy tác vụ DB: khoá nút + đổi chữ "Đang xử lý…" + con trỏ chờ + thanh tiến trình form chính;
        /// lỗi => hộp thoại lỗi. Trả về true nếu thành công.
        /// </summary>
        public static async Task<bool> ChayAsync(Control nut, Func<Task> congViec, string chuDangChay = "Đang xử lý…")
        {
            var form = nut?.FindForm();
            string chuCu = nut?.Text;
            bool laIcon = nut != null && nut.Font.Name == Theme.TenFontIcon;
            try
            {
                if (nut != null)
                {
                    nut.Enabled = false;
                    if (!laIcon) nut.Text = chuDangChay;
                }
                if (form != null) form.UseWaitCursor = true;
                if (_soTacVu++ == 0) TienTrinh?.Invoke(true);
                await congViec();
                return true;
            }
            catch (Exception ex)
            {
                Loi(form, ex.GetBaseException().Message);
                return false;
            }
            finally
            {
                if (--_soTacVu == 0) TienTrinh?.Invoke(false);
                if (nut != null)
                {
                    nut.Enabled = true;
                    if (!laIcon) nut.Text = chuCu;
                }
                if (form != null) form.UseWaitCursor = false;
            }
        }
    }
}
