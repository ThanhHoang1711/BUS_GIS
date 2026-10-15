using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace XeBus_GIS.UI.Controls
{
    public enum KieuThongBao { ThongTin, ThanhCong, CanhBao, Loi }

    /// <summary>
    /// Thông báo nhanh không chặn (toast) ở góc dưới phải cửa sổ chính: hiện mờ dần vào, tự đóng sau ~3,5 s,
    /// di chuột lên thì giữ lại, click để đóng. Thay MessageBox cho các thông báo thông thường.
    /// </summary>
    public sealed class Toast : Form
    {
        private const int ThoiGianHienMs = 3500;
        private static readonly List<Toast> _dangHien = new List<Toast>();

        private readonly string _noiDung;
        private readonly KieuThongBao _kieu;
        private readonly Timer _timer = new Timer { Interval = 15 };
        private readonly Form _chu;
        private int _conLaiMs = ThoiGianHienMs;
        private bool _dangDong;

        /// <summary>Hiện toast gắn với cửa sổ gốc (không phải hộp thoại modal sắp đóng) của owner.</summary>
        public static void Hien(IWin32Window owner, string noiDung, KieuThongBao kieu)
        {
            var chu = CuaSoGoc(owner);
            if (chu == null || !chu.IsHandleCreated || !chu.Visible)
            {
                // chưa có cửa sổ nào để gắn (vd lúc khởi động) – dùng hộp thoại
                MessageBox.Show(owner, noiDung, "Thông báo", MessageBoxButtons.OK,
                    kieu == KieuThongBao.Loi ? MessageBoxIcon.Error : MessageBoxIcon.Information);
                return;
            }
            var t = new Toast(chu, noiDung, kieu);
            t.Show(chu);
        }

        /// <summary>Đi ngược chuỗi Owner qua các hộp thoại modal để toast không bị đóng theo hộp thoại.</summary>
        private static Form CuaSoGoc(IWin32Window owner)
        {
            var f = (owner as Control)?.FindForm() ?? Form.ActiveForm;
            while (f != null && f.Owner != null && (f.Modal || f is Toast)) f = f.Owner;
            return f;
        }

        private Toast(Form chu, string noiDung, KieuThongBao kieu)
        {
            _chu = chu;
            _noiDung = noiDung;
            _kieu = kieu;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            BackColor = Theme.Surface;
            Opacity = 0;
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            Font = Theme.FontThuong;

            int rong = UiHelper.Dp(360);
            var kt = TextRenderer.MeasureText(noiDung, Theme.FontThuong, new Size(rong - UiHelper.Dp(64), 0), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            Size = new Size(rong, Math.Max(UiHelper.Dp(52), kt.Height + UiHelper.Dp(28)));

            _timer.Tick += Tick;
            Click += (s, e) => BatDauDong();
            MouseEnter += (s, e) => _conLaiMs = Math.Max(_conLaiMs, 1500);
            _chu.Move += ChuThayDoi;
            _chu.Resize += ChuThayDoi;
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE – không cướp focus của bản đồ / ô nhập
                cp.ClassStyle |= 0x00020000; // CS_DROPSHADOW
                return cp;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _dangHien.Add(this);
            XepViTri();
            _timer.Start();
        }

        private void ChuThayDoi(object sender, EventArgs e) => XepViTri();

        /// <summary>Xếp chồng các toast từ góc dưới phải cửa sổ chính (trên thanh trạng thái).</summary>
        private static void XepViTri()
        {
            int y = -1;
            for (int i = _dangHien.Count - 1; i >= 0; i--)
            {
                var t = _dangHien[i];
                if (t.IsDisposed || t._chu.IsDisposed) continue;
                var vung = t._chu.RectangleToScreen(t._chu.ClientRectangle);
                if (y < 0) y = vung.Bottom - UiHelper.Dp(40);
                y -= t.Height;
                t.Location = new Point(vung.Right - t.Width - UiHelper.Dp(20), y);
                y -= UiHelper.Dp(8);
            }
        }

        private void Tick(object sender, EventArgs e)
        {
            if (_dangDong)
            {
                Opacity -= 0.12;
                if (Opacity <= 0.02) Close();
                return;
            }
            if (Opacity < 1) Opacity = Math.Min(1, Opacity + 0.15);
            if (ClientRectangle.Contains(PointToClient(Cursor.Position))) return; // đang di chuột lên – giữ lại
            _conLaiMs -= _timer.Interval;
            if (_conLaiMs <= 0) BatDauDong();
        }

        private void BatDauDong() => _dangDong = true;

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer.Stop();
            _timer.Dispose();
            _chu.Move -= ChuThayDoi;
            _chu.Resize -= ChuThayDoi;
            _dangHien.Remove(this);
            XepViTri();
            base.OnFormClosed(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color mau, nenIcon;
            string icon;
            switch (_kieu)
            {
                case KieuThongBao.ThanhCong: mau = Theme.Success; Theme.MauTrangThai("Thành công", out nenIcon, out _); icon = Icons.ThanhCong; break;
                case KieuThongBao.CanhBao: mau = Theme.WarningText; Theme.MauTrangThai("Chờ duyệt", out nenIcon, out _); icon = Icons.CanhBao; break;
                case KieuThongBao.Loi: mau = Theme.Danger; Theme.MauTrangThai("Thất bại", out nenIcon, out _); icon = Icons.Loi; break;
                default: mau = Theme.Accent; nenIcon = Theme.AccentSoft; icon = Icons.Thong_Tin; break;
            }
            using (var pen = new Pen(Theme.Border))
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            using (var br = new SolidBrush(mau))
                g.FillRectangle(br, 0, 0, UiHelper.Dp(4), Height);

            int o = UiHelper.Dp(28);
            var rIcon = new Rectangle(UiHelper.Dp(16), (Height - o) / 2, o, o);
            using (var br = new SolidBrush(nenIcon))
                g.FillEllipse(br, rIcon);
            using (var f = Theme.FontIcon(10f))
                TextRenderer.DrawText(g, icon, f, rIcon, mau, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            var rChu = new Rectangle(UiHelper.Dp(54), UiHelper.Dp(12), Width - UiHelper.Dp(64), Height - UiHelper.Dp(24));
            TextRenderer.DrawText(g, _noiDung, Theme.FontThuong, rChu, Theme.Text,
                TextFormatFlags.WordBreak | TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _timer.Dispose();
            base.Dispose(disposing);
        }
    }
}
