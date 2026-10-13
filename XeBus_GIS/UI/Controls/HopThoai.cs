using System;
using System.Drawing;
using System.Windows.Forms;

namespace XeBus_GIS.UI.Controls
{
    public enum KieuHopThoai { ThongTin, CauHoi, NguyHiem, Loi }

    /// <summary>
    /// Hộp thoại theo design system (thay MessageBox): icon màu theo loại, tiêu đề, nội dung, nút chính / nút huỷ.
    /// Esc = huỷ; Enter = nút được focus (mặc định là nút an toàn – Huỷ – với hộp thoại nguy hiểm).
    /// </summary>
    public sealed class HopThoai : Form
    {
        private readonly KieuHopThoai _kieu;

        public static DialogResult Hien(IWin32Window owner, string tieuDe, string noiDung, KieuHopThoai kieu, string nutChinh, string nutHuy)
        {
            using (var f = new HopThoai(tieuDe, noiDung, kieu, nutChinh, nutHuy))
            {
                var chu = owner ?? Form.ActiveForm;
                if (chu == null) f.StartPosition = FormStartPosition.CenterScreen;
                return chu == null ? f.ShowDialog() : f.ShowDialog(chu);
            }
        }

        private HopThoai(string tieuDe, string noiDung, KieuHopThoai kieu, string nutChinh, string nutHuy)
        {
            _kieu = kieu;
            Text = tieuDe;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Theme.Surface;
            Font = Theme.FontThuong;
            AutoScaleMode = AutoScaleMode.None;

            int rong = UiHelper.Dp(440), le = UiHelper.Dp(24), cotChu = UiHelper.Dp(72);
            var ktNoiDung = TextRenderer.MeasureText(noiDung ?? "", Theme.FontThuong,
                new Size(rong - cotChu - le, 0), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);

            var lblIcon = new Label
            {
                Text = ChonIcon(kieu, out var mau),
                Font = Theme.FontIcon(18f),
                ForeColor = mau,
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds = new Rectangle(le, le, UiHelper.Dp(36), UiHelper.Dp(36)),
            };
            var lblTieuDe = new Label
            {
                Text = tieuDe,
                Font = Theme.FontTieuDe,
                ForeColor = Theme.Primary,
                AutoSize = false,
                Bounds = new Rectangle(cotChu, le - UiHelper.Dp(2), rong - cotChu - le, UiHelper.Dp(26)),
            };
            var lblNoiDung = new Label
            {
                Text = noiDung,
                Font = Theme.FontThuong,
                ForeColor = Theme.Text,
                AutoSize = false,
                Bounds = new Rectangle(cotChu, lblTieuDe.Bottom + UiHelper.Dp(6), rong - cotChu - le, ktNoiDung.Height + UiHelper.Dp(4)),
            };

            var pnlNut = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = UiHelper.Dp(60),
                BackColor = Theme.SurfaceAlt,
            };
            pnlNut.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.Border)) e.Graphics.DrawLine(pen, 0, 0, pnlNut.Width, 0);
            };

            var btnChinh = new Button { Text = nutChinh, DialogResult = DialogResult.OK, Height = UiHelper.Dp(34) };
            if (kieu == KieuHopThoai.NguyHiem) UiHelper.NutNguyHiemChinh(btnChinh); else UiHelper.NutChinh(btnChinh);
            btnChinh.Width = Math.Max(UiHelper.Dp(96), TextRenderer.MeasureText(nutChinh, Theme.FontDam).Width + UiHelper.Dp(32));
            int x = rong - le - btnChinh.Width;
            btnChinh.Location = new Point(x, UiHelper.Dp(13));
            pnlNut.Controls.Add(btnChinh);
            AcceptButton = btnChinh;

            if (!string.IsNullOrEmpty(nutHuy))
            {
                var btnHuy = new Button { Text = nutHuy, DialogResult = DialogResult.Cancel, Height = UiHelper.Dp(34) };
                UiHelper.NutPhu(btnHuy);
                btnHuy.Width = Math.Max(UiHelper.Dp(88), TextRenderer.MeasureText(nutHuy, Theme.FontThuong).Width + UiHelper.Dp(32));
                btnHuy.Location = new Point(x - UiHelper.Dp(8) - btnHuy.Width, UiHelper.Dp(13));
                pnlNut.Controls.Add(btnHuy);
                CancelButton = btnHuy;
                // thao tác nguy hiểm: focus mặc định vào nút an toàn
                if (kieu == KieuHopThoai.NguyHiem) Shown += (s, e) => btnHuy.Focus();
            }
            else
            {
                CancelButton = btnChinh;
            }

            Controls.Add(lblIcon);
            Controls.Add(lblTieuDe);
            Controls.Add(lblNoiDung);
            Controls.Add(pnlNut);
            ClientSize = new Size(rong, lblNoiDung.Bottom + le + pnlNut.Height);
        }

        private static string ChonIcon(KieuHopThoai kieu, out Color mau)
        {
            switch (kieu)
            {
                case KieuHopThoai.CauHoi: mau = Theme.Accent; return Icons.Thong_Tin;
                case KieuHopThoai.NguyHiem: mau = Theme.Danger; return Icons.CanhBao;
                case KieuHopThoai.Loi: mau = Theme.Danger; return Icons.Loi;
                default: mau = Theme.Accent; return Icons.Thong_Tin;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_kieu == KieuHopThoai.Loi) System.Media.SystemSounds.Hand.Play();
        }
    }
}
