using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace XeBus_GIS.UI.Controls
{
    /// <summary>
    /// Thẻ thông tin đối tượng ở panel phải (thay lưới 2 cột): đầu thẻ (ký hiệu, tên, mã · loại, nhãn trạng thái),
    /// thanh thao tác (Phóng tới / Sửa / Sao chép toạ độ), chip "Tuyến đi qua" (click mở tuyến) và danh sách thuộc tính.
    /// Dữ liệu lấy nguyên từ BanDoRepository.ThongTin (một dòng, tên cột là nhãn hiển thị) – không thêm truy vấn.
    /// </summary>
    public class TheDoiTuong : Panel
    {
        public event Action<string, string> PhongToi;   // (loai, ma)
        public event Action<string, string> Sua;        // (loai, ma)
        public event Action<string> MoTuyen;            // matuyen

        private readonly DauThe _dau;
        private readonly FlowLayoutPanel _thaoTac, _tuyen;
        private readonly Label _lblTuyen;
        private readonly DanhSachThuocTinh _ds;
        private readonly Button _btnPhongToi, _btnSua, _btnCopy;
        private string _loai, _ma, _toaDo;

        /// <summary>Có cho phép nút Sửa (theo quyền) với loại đối tượng – FrmMain gán.</summary>
        public Func<string, bool> ChoPhepSua { get; set; } = _ => false;
        /// <summary>Ký hiệu đầu thẻ (vẽ sẵn theo loại + giá trị) – FrmMain gán (dùng KyHieuTram/KyHieuCongTrinh, màu tuyến).</summary>
        public Func<string, DataRow, Bitmap> KyHieu { get; set; }
        /// <summary>Màu chấm của chip tuyến.</summary>
        public Func<string, Color?> MauTuyen { get; set; }

        public TheDoiTuong()
        {
            AutoScroll = true;
            BackColor = Theme.Surface;
            Padding = new Padding(0, 0, UiHelper.Dp(2), UiHelper.Dp(8));

            _dau = new DauThe();
            _thaoTac = new FlowLayoutPanel { WrapContents = true, Padding = new Padding(0, UiHelper.Dp(6), 0, UiHelper.Dp(6)), BackColor = Theme.Surface };
            _btnPhongToi = NutNho("Phóng tới", Icons.ZoomIn);
            _btnSua = NutNho("Sửa", Icons.Sua);
            _btnCopy = NutNho("Sao chép toạ độ", "");
            _thaoTac.Controls.AddRange(new Control[] { _btnPhongToi, _btnSua, _btnCopy });
            _lblTuyen = new Label { Height = UiHelper.Dp(24), Text = "Tuyến đi qua", TextAlign = ContentAlignment.BottomLeft, UseMnemonic = false };
            UiHelper.TieuDeKhuVuc(_lblTuyen);
            _tuyen = new FlowLayoutPanel { WrapContents = true, Padding = new Padding(0, UiHelper.Dp(4), 0, UiHelper.Dp(4)), BackColor = Theme.Surface };
            _ds = new DanhSachThuocTinh();
            Controls.AddRange(new Control[] { _dau, _thaoTac, _lblTuyen, _tuyen, _ds });
            _dau.Resize += (s, e) => PerformLayout();
            _ds.Resize += (s, e) => PerformLayout();

            _btnPhongToi.Click += (s, e) => { if (_ma != null) PhongToi?.Invoke(_loai, _ma); };
            _btnSua.Click += (s, e) => { if (_ma != null) Sua?.Invoke(_loai, _ma); };
            _btnCopy.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(_toaDo)) return;
                try { Clipboard.SetText(_toaDo); UiHelper.ThanhCong(FindForm(), "Đã sao chép toạ độ " + _toaDo); } catch { }
            };
            Hien(null, null, null, null);
        }

        /// <summary>Xếp dọc: đầu thẻ · thao tác · tuyến đi qua · thuộc tính (tự tính chiều cao, cuộn khi dài).</summary>
        protected override void OnLayout(LayoutEventArgs e)
        {
            int w = Math.Max(UiHelper.Dp(120), ClientSize.Width - Padding.Right), y = AutoScrollPosition.Y;
            foreach (Control c in new Control[] { _dau, _thaoTac, _lblTuyen, _tuyen, _ds })
            {
                if (c == null || !c.Visible) continue;
                int h = c is FlowLayoutPanel f ? f.GetPreferredSize(new Size(w, 0)).Height : c.Height;
                c.SetBounds(0, y, w, h);
                y += c.Height;
            }
            AutoScrollMinSize = new Size(0, y - AutoScrollPosition.Y + Padding.Bottom);
            base.OnLayout(e);
        }

        private Button NutNho(string chu, string glyph)
        {
            var b = new Button { Text = chu, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Height = UiHelper.Dp(30), Margin = new Padding(0, 0, UiHelper.Dp(6), UiHelper.Dp(6)), Padding = new Padding(UiHelper.Dp(4), 0, UiHelper.Dp(6), 0) };
            UiHelper.NutPhu(b);
            b.Font = Theme.FontNhan;
            UiHelper.GanIcon(b, glyph, 9f);
            return b;
        }

        /// <summary>
        /// Hiện đối tượng. dt = kết quả BanDoRepository.ThongTin (một dòng); tenLoai = "trạm", "công trình"…
        /// null ⇒ trạng thái rỗng.
        /// </summary>
        public void Hien(string loai, string ma, string tenLoai, DataTable dt)
        {
            SuspendLayout();
            _loai = loai;
            _ma = ma;
            _tuyen.Controls.Clear();
            var r = dt != null && dt.Rows.Count > 0 ? dt.Rows[0] : null;
            if (r == null)
            {
                _dau.Dat(null, "Chưa chọn đối tượng",
                    "Click một trạm (hoặc công trình khi lớp Công trình đang bật) trên bản đồ, hoặc dùng ô tìm nhanh (Ctrl+F).", null, null);
                _thaoTac.Visible = _lblTuyen.Visible = _tuyen.Visible = false;
                _ds.Dat(new List<(string, string)>());
                _toaDo = null;
                ResumeLayout();
                return;
            }

            var cot = dt.Columns.Cast<DataColumn>().ToList();
            string Gt(DataColumn c) => r[c] == DBNull.Value ? null : Convert.ToString(r[c], CultureInfo.CurrentCulture);
            // tên: cột đầu tiên có chữ "Tên"; mã: cột đầu tiên có chữ "Mã"
            var cTen = cot.FirstOrDefault(c => c.ColumnName.StartsWith("Tên", StringComparison.OrdinalIgnoreCase));
            var cMa = cot.FirstOrDefault(c => c.ColumnName.StartsWith("Mã", StringComparison.OrdinalIgnoreCase));
            var cTrangThai = cot.FirstOrDefault(c => c.ColumnName == "Trạng thái");
            var cLoai = cot.FirstOrDefault(c => c.ColumnName == "Loại trạm" || c.ColumnName == "Loại" || c.ColumnName == "Loại đường");
            var cVi = cot.FirstOrDefault(c => c.ColumnName == "Vĩ độ");
            var cKinh = cot.FirstOrDefault(c => c.ColumnName == "Kinh độ");
            var cTuyen = cot.FirstOrDefault(c => c.ColumnName == "Các tuyến");

            string ten = cTen != null ? Gt(cTen) : ma;
            string phu = string.Join(" · ", new[] { Viet(tenLoai), cMa != null ? Gt(cMa) : ma, cLoai != null ? Gt(cLoai) : null }
                                                .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct());
            _dau.Dat(KyHieu?.Invoke(loai, r), ten ?? ma, phu, cTrangThai != null ? Gt(cTrangThai) : null, null);

            _toaDo = cVi != null && cKinh != null && r[cVi] != DBNull.Value && r[cKinh] != DBNull.Value
                ? string.Format(CultureInfo.InvariantCulture, "{0:0.000000}, {1:0.000000}", Convert.ToDouble(r[cVi]), Convert.ToDouble(r[cKinh]))
                : null;
            _btnCopy.Visible = _toaDo != null;
            _btnSua.Visible = ChoPhepSua(loai);
            _thaoTac.Visible = true;

            var dsTuyen = cTuyen != null && Gt(cTuyen) != null
                ? Gt(cTuyen).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.Length > 0).ToList()
                : new List<string>();
            _lblTuyen.Visible = _tuyen.Visible = dsTuyen.Count > 0;
            foreach (var mt in dsTuyen) _tuyen.Controls.Add(TaoChipTuyen(mt));

            var bo = new HashSet<DataColumn> { cTen, cMa, cTrangThai, cTuyen, cVi, cKinh };
            var dong = cot.Where(c => !bo.Contains(c)).Select(c => (c.ColumnName, Gt(c) ?? "—")).ToList();
            if (_toaDo != null) dong.Add(("Toạ độ (vĩ, kinh)", _toaDo));
            _ds.Dat(dong);
            ResumeLayout();
            AutoScrollPosition = Point.Empty;
        }

        private static string Viet(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);

        private Control TaoChipTuyen(string ma)
        {
            var mau = MauTuyen?.Invoke(ma) ?? Theme.Accent;
            var b = new Button { Text = ma, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Height = UiHelper.Dp(28), Margin = new Padding(0, 0, UiHelper.Dp(6), UiHelper.Dp(6)), Padding = new Padding(UiHelper.Dp(14), 0, UiHelper.Dp(4), 0) };
            UiHelper.NutPhu(b);
            b.Font = Theme.FontDam;
            b.Paint += (s, e) =>
            {
                int d = UiHelper.Dp(8);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new SolidBrush(mau)) e.Graphics.FillEllipse(br, UiHelper.Dp(8), (b.Height - d) / 2, d, d);
            };
            UiHelper.TipCho(b, "Mở tuyến " + ma + " ở màn hình Tuyến xe buýt");
            b.Click += (s, e) => MoTuyen?.Invoke(ma);
            return b;
        }

        // =====================================================================
        // Đầu thẻ: ký hiệu · tên (2 dòng) · dòng phụ · nhãn trạng thái
        // =====================================================================
        private class DauThe : Control
        {
            private Bitmap _kh;
            private string _ten = "", _phu = "", _trangThai;

            public DauThe()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
                BackColor = Theme.Surface;
                Height = UiHelper.Dp(84);
            }

            public void Dat(Bitmap kh, string ten, string phu, string trangThai, object _)
            {
                _kh?.Dispose();
                _kh = kh;
                _ten = ten ?? "";
                _phu = phu ?? "";
                _trangThai = trangThai;
                TinhCao();
                Invalidate();
            }

            private int LeTrai => _kh != null ? UiHelper.Dp(48) : 0;

            private void TinhCao()
            {
                int w = Math.Max(UiHelper.Dp(80), Width - LeTrai);
                int hTen = TextRenderer.MeasureText(_ten, Theme.FontTieuDe, new Size(w, 0), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height;
                int hPhu = TextRenderer.MeasureText(_phu, Theme.FontNhan, new Size(w, 0), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height;
                int cao = UiHelper.Dp(8) + Math.Min(hTen, UiHelper.Dp(48)) + UiHelper.Dp(2) + hPhu + (_trangThai != null ? UiHelper.Dp(30) : UiHelper.Dp(8));
                cao = Math.Max(cao, _kh != null ? UiHelper.Dp(56) : 0);
                if (Height != cao) Height = cao;
            }

            protected override void OnResize(EventArgs e) { base.OnResize(e); TinhCao(); }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int y = UiHelper.Dp(8);
                if (_kh != null) g.DrawImage(_kh, 0, y, UiHelper.Dp(36), UiHelper.Dp(36));
                int x = LeTrai, w = Math.Max(10, Width - x);
                int hTen = Math.Min(TextRenderer.MeasureText(_ten, Theme.FontTieuDe, new Size(w, 0), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height, UiHelper.Dp(48));
                TextRenderer.DrawText(g, _ten, Theme.FontTieuDe, new Rectangle(x, y, w, hTen), Theme.Primary,
                    TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                y += hTen + UiHelper.Dp(2);
                int hPhu = TextRenderer.MeasureText(_phu, Theme.FontNhan, new Size(w, 0), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height;
                TextRenderer.DrawText(g, _phu, Theme.FontNhan, new Rectangle(x, y, w, hPhu), Theme.Muted, TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
                y += hPhu + UiHelper.Dp(6);
                if (_trangThai != null)
                {
                    Theme.MauTrangThai(_trangThai, out var nen, out var chu);
                    var kt = TextRenderer.MeasureText(_trangThai, Theme.FontNho);
                    var r = new Rectangle(x, y, kt.Width + UiHelper.Dp(12), UiHelper.Dp(20));
                    using (var path = UiHelper.DuongBoGoc(r, r.Height / 2f))
                    using (var br = new SolidBrush(nen)) g.FillPath(br, path);
                    TextRenderer.DrawText(g, _trangThai, Theme.FontNho, r, chu, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
            }
        }

        // =====================================================================
        // Danh sách thuộc tính: nhãn mờ bên trái · giá trị bên phải (tự xuống dòng)
        // =====================================================================
        private class DanhSachThuocTinh : Control
        {
            private List<(string nhan, string gt)> _dong = new List<(string, string)>();

            public DanhSachThuocTinh()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
                BackColor = Theme.Surface;
            }

            public void Dat(List<(string, string)> dong)
            {
                _dong = dong ?? new List<(string, string)>();
                TinhCao();
                Invalidate();
            }

            private int CotNhan => Math.Min(UiHelper.Dp(130), (int)(Width * 0.42));

            private IEnumerable<(string nhan, string gt, int y, int h)> BoCuc()
            {
                int y = UiHelper.Dp(4), wGt = Math.Max(10, Width - CotNhan - UiHelper.Dp(8));
                foreach (var (nhan, gt) in _dong)
                {
                    int h1 = TextRenderer.MeasureText(nhan, Theme.FontNhan, new Size(CotNhan, 0), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height;
                    int h2 = TextRenderer.MeasureText(gt, Theme.FontThuong, new Size(wGt, 0), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height;
                    int h = Math.Max(h1, h2) + UiHelper.Dp(10);
                    yield return (nhan, gt, y, h);
                    y += h;
                }
            }

            private void TinhCao()
            {
                int cao = BoCuc().Select(b => b.y + b.h).DefaultIfEmpty(0).Max() + UiHelper.Dp(4);
                if (Height != cao) Height = cao;
            }

            protected override void OnResize(EventArgs e) { base.OnResize(e); TinhCao(); }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                int wGt = Math.Max(10, Width - CotNhan - UiHelper.Dp(8));
                using (var pen = new Pen(Theme.Border))
                    foreach (var (nhan, gt, y, h) in BoCuc())
                    {
                        TextRenderer.DrawText(g, nhan, Theme.FontNhan, new Rectangle(0, y + UiHelper.Dp(5), CotNhan, h), Theme.Muted,
                            TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
                        TextRenderer.DrawText(g, gt, Theme.FontThuong, new Rectangle(CotNhan + UiHelper.Dp(8), y + UiHelper.Dp(5), wGt, h), Theme.Text,
                            TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
                        g.DrawLine(pen, 0, y + h - 1, Width, y + h - 1);
                    }
            }
        }
    }
}
