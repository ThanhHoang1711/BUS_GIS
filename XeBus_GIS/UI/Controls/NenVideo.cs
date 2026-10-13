using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace XeBus_GIS.UI.Controls
{
    /// <summary>
    /// Vùng hình nền động của trang đăng nhập: phát lặp chuỗi khung hình JPEG (render bằng Blender từ dữ liệu CSDLKG –
    /// tools/blender/dung_canh.py) rồi vẽ lớp phủ tối + nội dung giới thiệu hệ thống lên trên.
    /// Tệp khung hình: Assets\nen_dangnhap.bin (định dạng "XBV1" · fps · số khung · [độ dài · byte JPEG]…).
    /// Thiếu tệp → nền gradient tĩnh. Windows tắt hiệu ứng (UIEffects) → đứng yên ở khung đầu.
    /// Khung chỉ được giải mã khi cần vẽ (giữ byte JPEG trong RAM, không giữ bitmap) để tốn ít bộ nhớ.
    /// </summary>
    public class NenVideo : Control
    {
        private readonly List<byte[]> _khung = new List<byte[]>();
        private readonly Timer _timer = new Timer();
        private int _chiSo;
        private Image _anh;

        public string TieuDe { get; set; } = "XeBus GIS";
        public string Slogan { get; set; } = "";
        public string MoTa { get; set; } = "";
        /// <summary>Số liệu thật (vd "1.258 trạm · 6 tuyến · 126 phường/xã"); rỗng = không vẽ.</summary>
        public string SoLieu { get => _soLieu; set { _soLieu = value; Invalidate(); } }
        private string _soLieu = "";
        public string ChuThichNguon { get; set; } = "";
        public (string icon, string ten, string moTa)[] TinhNang { get; set; } = new (string, string, string)[0];

        public NenVideo()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = ColorTranslator.FromHtml("#08131B");
            _timer.Tick += (s, e) =>
            {
                if (_khung.Count == 0) return;
                _chiSo = (_chiSo + 1) % _khung.Count;
                GiaiMa();
                Invalidate();
            };
        }

        /// <summary>Nạp tệp khung hình; trả false nếu không có/lỗi (khi đó vẽ nền tĩnh).</summary>
        public bool Nap(string duongDan)
        {
            try
            {
                if (!File.Exists(duongDan)) return false;
                using (var br = new BinaryReader(File.OpenRead(duongDan)))
                {
                    if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "XBV1") return false;
                    int fps = br.ReadInt32(), so = br.ReadInt32();
                    for (int i = 0; i < so; i++) _khung.Add(br.ReadBytes(br.ReadInt32()));
                    _timer.Interval = Math.Max(15, 1000 / Math.Max(1, fps));
                }
                _chiSo = 0;
                GiaiMa();
                CapNhatPhat();
                return _khung.Count > 0;
            }
            catch
            {
                _khung.Clear();
                return false;
            }
        }

        private void GiaiMa()
        {
            if (_khung.Count == 0) return;
            var cu = _anh;
            try
            {
                using (var ms = new MemoryStream(_khung[_chiSo]))
                using (var tam = Image.FromStream(ms))
                    _anh = new Bitmap(tam);
            }
            catch { return; }
            cu?.Dispose();
        }

        /// <summary>Chỉ phát khi đang hiển thị, cửa sổ không thu nhỏ và Windows bật hiệu ứng.</summary>
        private void CapNhatPhat()
        {
            var f = FindForm();
            bool phat = _khung.Count > 1 && Visible && IsHandleCreated
                        && SystemInformation.UIEffectsEnabled
                        && (f == null || f.WindowState != FormWindowState.Minimized);
            if (phat && !_timer.Enabled) _timer.Start();
            else if (!phat && _timer.Enabled) _timer.Stop();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var f = FindForm();
            if (f != null) f.Resize += (s, ev) => CapNhatPhat();
            CapNhatPhat();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            CapNhatPhat();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var r = ClientRectangle;
            if (r.Width <= 0 || r.Height <= 0) return;

            // 1. hình nền: phủ kín (cover), căn giữa
            if (_anh != null)
            {
                float tl = Math.Max((float)r.Width / _anh.Width, (float)r.Height / _anh.Height);
                int w = (int)Math.Ceiling(_anh.Width * tl), h = (int)Math.Ceiling(_anh.Height * tl);
                g.InterpolationMode = InterpolationMode.Bilinear;
                g.DrawImage(_anh, new Rectangle((r.Width - w) / 2, (r.Height - h) / 2, w, h));
            }
            else
            {
                using (var br = new LinearGradientBrush(r, ColorTranslator.FromHtml("#0B2530"), ColorTranslator.FromHtml("#071018"), 60f))
                    g.FillRectangle(br, r);
            }

            // 2. lớp phủ tối để chữ trắng đủ tương phản (đậm ở trái-dưới nơi đặt chữ)
            using (var br = new LinearGradientBrush(r, Color.FromArgb(205, 6, 14, 20), Color.FromArgb(70, 6, 14, 20), 0f))
                g.FillRectangle(br, r);
            using (var br = new LinearGradientBrush(new Rectangle(0, r.Height / 2, r.Width, r.Height / 2 + 1), Color.FromArgb(0, 6, 14, 20), Color.FromArgb(170, 6, 14, 20), 90f))
                g.FillRectangle(br, 0, r.Height / 2, r.Width, r.Height - r.Height / 2);

            VeNoiDung(g, r);
        }

        private void VeNoiDung(Graphics g, Rectangle r)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            int le = Math.Max(UiHelper.Dp(40), r.Width / 14);
            int rong = Math.Min(r.Width - 2 * le, UiHelper.Dp(620));
            var trang = Color.White;
            var phu = Color.FromArgb(205, 226, 232, 240);
            var flags = TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.Left;

            // logo + tên ở góc trên
            int o = UiHelper.Dp(40), y = UiHelper.Dp(36);
            var rLogo = new Rectangle(le, y, o, o);
            using (var path = UiHelper.DuongBoGoc(rLogo, UiHelper.Dp(9)))
            using (var br = new SolidBrush(Theme.Accent))
                g.FillPath(br, path);
            using (var f = Theme.FontIcon(16f))
                TextRenderer.DrawText(g, Icons.Bus, f, rLogo, trang, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            using (var f = new Font("Segoe UI Semibold", 14f))
                TextRenderer.DrawText(g, TieuDe, f, new Rectangle(rLogo.Right + UiHelper.Dp(12), y, rong, o), trang,
                    TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

            // khối giới thiệu ở nửa dưới
            using (var fTieuDe = new Font("Segoe UI Semibold", r.Width > UiHelper.Dp(900) ? 28f : 22f))
            using (var fMoTa = new Font("Segoe UI", 11.25f))
            using (var fTen = new Font("Segoe UI Semibold", 10.5f))
            using (var fIcon = Theme.FontIcon(15f))
            {
                int hSlogan = TextRenderer.MeasureText(g, Slogan, fTieuDe, new Size(rong, 0), flags).Height;
                int hMoTa = TextRenderer.MeasureText(g, MoTa, fMoTa, new Size(rong, 0), flags).Height;
                int hTinhNang = TinhNang.Length * UiHelper.Dp(52);
                int hSoLieu = string.IsNullOrEmpty(SoLieu) ? 0 : UiHelper.Dp(44);
                int tong = hSlogan + UiHelper.Dp(14) + hMoTa + UiHelper.Dp(28) + hTinhNang + hSoLieu;
                y = Math.Max(rLogo.Bottom + UiHelper.Dp(40), r.Height - UiHelper.Dp(72) - tong);

                TextRenderer.DrawText(g, Slogan, fTieuDe, new Rectangle(le, y, rong, hSlogan), trang, flags);
                y += hSlogan + UiHelper.Dp(14);
                TextRenderer.DrawText(g, MoTa, fMoTa, new Rectangle(le, y, rong, hMoTa), phu, flags);
                y += hMoTa + UiHelper.Dp(28);

                foreach (var (icon, ten, moTa) in TinhNang)
                {
                    var rIcon = new Rectangle(le, y, UiHelper.Dp(36), UiHelper.Dp(36));
                    using (var path = UiHelper.DuongBoGoc(rIcon, UiHelper.Dp(8)))
                    using (var br = new SolidBrush(Color.FromArgb(60, 45, 212, 191)))
                        g.FillPath(br, path);
                    TextRenderer.DrawText(g, icon, fIcon, rIcon, ColorTranslator.FromHtml("#5EEAD4"),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                    int x = rIcon.Right + UiHelper.Dp(14);
                    TextRenderer.DrawText(g, ten, fTen, new Rectangle(x, y - UiHelper.Dp(1), rong - x + le, UiHelper.Dp(20)), trang, TextFormatFlags.NoPrefix | TextFormatFlags.Left);
                    TextRenderer.DrawText(g, moTa, Theme.FontNhan, new Rectangle(x, y + UiHelper.Dp(19), rong - x + le, UiHelper.Dp(20)), phu,
                        TextFormatFlags.NoPrefix | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
                    y += UiHelper.Dp(52);
                }

                if (hSoLieu > 0)
                {
                    y += UiHelper.Dp(4);
                    using (var pen = new Pen(Color.FromArgb(70, 255, 255, 255)))
                        g.DrawLine(pen, le, y, le + rong, y);
                    TextRenderer.DrawText(g, SoLieu, fTen, new Rectangle(le, y + UiHelper.Dp(12), rong, UiHelper.Dp(24)), trang, TextFormatFlags.NoPrefix | TextFormatFlags.Left);
                }
            }

            if (!string.IsNullOrEmpty(ChuThichNguon))
                TextRenderer.DrawText(g, ChuThichNguon, Theme.FontNho, new Rectangle(le, r.Height - UiHelper.Dp(36), r.Width - 2 * le, UiHelper.Dp(20)),
                    Color.FromArgb(150, 226, 232, 240), TextFormatFlags.NoPrefix | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
                _anh?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
