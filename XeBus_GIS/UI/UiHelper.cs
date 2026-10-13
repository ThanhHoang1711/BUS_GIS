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
            GanTrangThai(b, KieuNut.Chinh, Theme.Accent, Color.White, Theme.Accent, 0);
        }

        /// <summary>Nút phụ (nền trắng, viền).</summary>
        public static void NutPhu(Button b)
        {
            NutCoBan(b);
            b.Font = Theme.FontThuong;
            b.FlatAppearance.MouseOverBackColor = Theme.SurfaceHover;
            b.FlatAppearance.MouseDownBackColor = Theme.Border;
            GanTrangThai(b, KieuNut.Phu, Theme.Surface, Theme.Secondary, Theme.BorderStrong, 1);
        }

        /// <summary>Nút nguy hiểm dạng phụ (xoá) – chữ đỏ, viền.</summary>
        public static void NutNguyHiem(Button b)
        {
            NutCoBan(b);
            b.Font = Theme.FontThuong;
            b.FlatAppearance.MouseOverBackColor = ColorTranslator.FromHtml("#FEF2F2");
            b.FlatAppearance.MouseDownBackColor = ColorTranslator.FromHtml("#FEE2E2");
            GanTrangThai(b, KieuNut.NguyHiem, Theme.Surface, Theme.Danger, ColorTranslator.FromHtml("#FECACA"), 1);
        }

        /// <summary>Nút nguy hiểm dạng chính (xác nhận xoá trong hộp thoại).</summary>
        public static void NutNguyHiemChinh(Button b)
        {
            NutCoBan(b);
            b.Font = Theme.FontDam;
            b.FlatAppearance.MouseOverBackColor = Theme.DangerHover;
            b.FlatAppearance.MouseDownBackColor = Theme.DangerHover;
            GanTrangThai(b, KieuNut.NguyHiemChinh, Theme.Danger, Color.White, Theme.Danger, 0);
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
            GanTrangThai(b, KieuNut.Icon, b.Parent?.BackColor == Theme.Background ? Theme.Background : Theme.Surface,
                Theme.Secondary, Theme.Surface, 0);
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
            if (_tab.TryGetValue(b, out _)) return;
            _tab.Add(b, new[] { false });
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
                c.ForeColor = Theme.Text;
                if (c is TextBox t) t.BorderStyle = BorderStyle.FixedSingle;
                if (c is ComboBox cb) cb.FlatStyle = FlatStyle.System;
            }
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
            g.BackgroundColor = Theme.Surface;
            g.BorderStyle = BorderStyle.None;
            g.GridColor = Theme.Border;
            g.EnableHeadersVisualStyles = false;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            g.ColumnHeadersDefaultCellStyle.BackColor = Theme.SurfaceAlt;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Muted;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9f);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.SurfaceAlt;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Theme.Muted;
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            g.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            g.ColumnHeadersHeight = Dp(32);
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.DefaultCellStyle.Font = Theme.FontThuong;
            g.DefaultCellStyle.ForeColor = Theme.Text;
            g.DefaultCellStyle.BackColor = Theme.Surface;
            g.DefaultCellStyle.SelectionBackColor = Theme.RowSelect;
            g.DefaultCellStyle.SelectionForeColor = Theme.Text;
            g.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            g.AlternatingRowsDefaultCellStyle.BackColor = Theme.RowAlt;
            g.AlternatingRowsDefaultCellStyle.SelectionBackColor = Theme.RowSelect;
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
            c.BackColor = Theme.Surface;
            c.BorderlineWidth = 0;
            c.AntiAliasing = System.Windows.Forms.DataVisualization.Charting.AntiAliasingStyles.All;
            c.TextAntiAliasingQuality = System.Windows.Forms.DataVisualization.Charting.TextAntiAliasingQuality.High;
            foreach (var a in c.ChartAreas)
            {
                a.BackColor = Theme.Surface;
                foreach (var ax in new[] { a.AxisX, a.AxisY })
                {
                    ax.LineColor = Theme.Border;
                    ax.MajorGrid.LineColor = Theme.Border;
                    ax.MajorTickMark.LineColor = Theme.Border;
                    ax.LabelStyle.ForeColor = Theme.Muted;
                    ax.LabelStyle.Font = Theme.FontNho;
                    ax.TitleForeColor = Theme.Muted;
                    ax.TitleFont = Theme.FontNho;
                }
                a.AxisX.MajorGrid.Enabled = false;
                a.AxisX.MajorTickMark.Enabled = false;
                a.AxisX.Interval = 1;
            }
            foreach (var l in c.Legends)
            {
                l.BackColor = Theme.Surface;
                l.ForeColor = Theme.Secondary;
                l.Font = Theme.FontNho;
            }
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
