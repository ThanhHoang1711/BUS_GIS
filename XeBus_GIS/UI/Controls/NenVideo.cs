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
    /// Khung chỉ được giải mã khi cần vẽ (giữ byte JPEG trong RAM, chỉ 2 bitmap đang dùng) để tốn ít bộ nhớ.
    /// Tệp lưu ít khung/giây (12) cho nhẹ; khi phát, hai khung liền nhau được hoà trộn theo thời gian
    /// (vẽ ~30 lần/giây) nên chuyển động vẫn mượt.
    /// </summary>
    public class NenVideo : Control
    {
        private readonly List<byte[]> _khung = new List<byte[]>();
        private readonly Timer _timer = new Timer { Interval = 33 };
        private readonly System.Diagnostics.Stopwatch _dongHo = new System.Diagnostics.Stopwatch();
        private int _fps = 12;
        private int _chiSo = -1;          // khung A đang giải mã
        private Image _anh, _anhSau;      // khung A và khung kế tiếp B
        private float _tron;              // 0..1: tỉ lệ hoà khung B lên khung A

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
                double vt = _dongHo.Elapsed.TotalSeconds * _fps;
                int a = (int)Math.Floor(vt) % _khung.Count;
                _tron = (float)(vt - Math.Floor(vt));
                if (a != _chiSo) DatKhung(a);
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
                    _fps = Math.Max(1, fps);
                }
                DatKhung(0);
                CapNhatPhat();
                return _khung.Count > 0;
            }
            catch
            {
                _khung.Clear();
                return false;
            }
        }

        private Image GiaiMa(int chiSo)
        {
            try
            {
                using (var ms = new MemoryStream(_khung[chiSo]))
                using (var tam = Image.FromStream(ms))
                    return new Bitmap(tam);
            }
            catch { return null; }
        }

        /// <summary>Đặt khung A = chiSo, B = khung kế (tái dùng bitmap B cũ khi đi tới tuần tự).</summary>
        private void DatKhung(int chiSo)
        {
            if (_khung.Count == 0) return;
            int sau = (chiSo + 1) % _khung.Count;
            Image moiA, moiB;
            if (_chiSo >= 0 && chiSo == (_chiSo + 1) % _khung.Count && _anhSau != null)
            {
                moiA = _anhSau;
                _anh?.Dispose();
            }
            else
            {
                moiA = GiaiMa(chiSo);
                _anh?.Dispose();
                _anhSau?.Dispose();
            }
            moiB = _khung.Count > 1 ? GiaiMa(sau) : null;
            _anh = moiA;
            _anhSau = moiB;
            _chiSo = chiSo;
        }

        /// <summary>Chỉ phát khi đang hiển thị, cửa sổ không thu nhỏ và Windows bật hiệu ứng.</summary>
        private void CapNhatPhat()
        {
            var f = FindForm();
            bool phat = _khung.Count > 1 && Visible && IsHandleCreated
                        && SystemInformation.UIEffectsEnabled
                        && (f == null || f.WindowState != FormWindowState.Minimized);
            if (phat && !_timer.Enabled) { _dongHo.Start(); _timer.Start(); }
            else if (!phat && _timer.Enabled) { _timer.Stop(); _dongHo.Stop(); }
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
                g.PixelOffsetMode = PixelOffsetMode.Half;
                var dich = new Rectangle((r.Width - w) / 2, (r.Height - h) / 2, w, h);
                g.DrawImage(_anh, dich);
                if (_anhSau != null && _tron > 0.02f && _timer.Enabled)
                    using (var ia = new ImageAttributes())
                    {
                        ia.SetColorMatrix(new ColorMatrix { Matrix33 = _tron });
                        g.DrawImage(_anhSau, dich, 0, 0, _anhSau.Width, _anhSau.Height, GraphicsUnit.Pixel, ia);
                    }
                g.PixelOffsetMode = PixelOffsetMode.Default;
            }
            else
            {
                using (var br = new LinearGradientBrush(r, ColorTranslator.FromHtml("#0B2530"), ColorTranslator.FromHtml("#071018"), 60f))
                    g.FillRectangle(br, r);
            }

            // 2. lớp phủ than chì (trung tính) để chữ trắng đủ tương phản: đậm ở mép trái + đáy (nơi đặt chữ),
            //    nhạt dần sang phải để vẫn thấy rõ sa bàn
            var p = Theme.DnPhu;
            var rTrai = new Rectangle(0, 0, Math.Max(1, r.Width * 4 / 5), r.Height);
            using (var br = new LinearGradientBrush(rTrai, Color.FromArgb(205, p), Color.FromArgb(0, p), 0f))
                g.FillRectangle(br, rTrai);
            var rDuoi = new Rectangle(0, r.Height * 2 / 5, r.Width, r.Height - r.Height * 2 / 5 + 1);
            using (var br = new LinearGradientBrush(rDuoi, Color.FromArgb(0, p), Color.FromArgb(185, p), 90f))
                g.FillRectangle(br, rDuoi);
            using (var br = new LinearGradientBrush(new Rectangle(0, 0, r.Width, UiHelper.Dp(120)), Color.FromArgb(120, p), Color.FromArgb(0, p), 90f))
                g.FillRectangle(br, 0, 0, r.Width, UiHelper.Dp(120));

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
            using (var br = new SolidBrush(trang))
                g.FillPath(br, path);
            using (var f = Theme.FontIcon(16f))
                TextRenderer.DrawText(g, Icons.Bus, f, rLogo, Theme.DnNhan, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
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
                    using (var br = new SolidBrush(Color.FromArgb(38, 255, 255, 255)))
                    using (var pen = new Pen(Color.FromArgb(60, 255, 255, 255)))
                    {
                        g.FillPath(br, path);
                        g.DrawPath(pen, path);
                    }
                    TextRenderer.DrawText(g, icon, fIcon, rIcon, trang,
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
                _anhSau?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
