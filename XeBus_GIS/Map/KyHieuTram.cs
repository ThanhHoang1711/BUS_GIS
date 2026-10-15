using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using XeBus_GIS.UI;

namespace XeBus_GIS.Map
{
    /// <summary>
    /// Ký hiệu trạm hình xe buýt (giống logo app: ô bo góc màu loại trạm + xe buýt trắng) cho MapWinGIS,
    /// cỡ thay đổi theo tỷ lệ bản đồ. Ảnh được vẽ sẵn đúng cỡ điểm ảnh (không phóng to/thu nhỏ khi vẽ ⇒ luôn nét),
    /// lưu PNG trong %LOCALAPPDATA%\XeBus_GIS\kyhieu rồi mở bằng MapWinGIS.Image.
    /// </summary>
    public static class KyHieuTram
    {
        /// <summary>
        /// Cỡ ký hiệu (px) theo mẫu số tỷ lệ; 0 = vẽ chấm tròn (toàn thành phố có hơn 1.000 trạm,
        /// biểu tượng sẽ chồng lên nhau).
        /// </summary>
        public static int CoTheoTyLe(double mauSoTyLe)
        {
            if (mauSoTyLe <= 0 || mauSoTyLe > 120000) return 0;
            if (mauSoTyLe > 50000) return UiHelper.Dp(14);
            if (mauSoTyLe > 20000) return UiHelper.Dp(18);
            if (mauSoTyLe > 8000) return UiHelper.Dp(24);
            return UiHelper.Dp(30);
        }

        /// <summary>Cỡ ký hiệu công trình (px) theo tỷ lệ; 0 = chấm nhỏ (hơn 2.000 công trình – thu nhỏ sẽ chồng chéo).</summary>
        public static int CoCongTrinhTheoTyLe(double mauSoTyLe)
        {
            if (mauSoTyLe <= 0 || mauSoTyLe > 60000) return 0;
            if (mauSoTyLe > 25000) return UiHelper.Dp(15);
            if (mauSoTyLe > 10000) return UiHelper.Dp(19);
            return UiHelper.Dp(24);
        }

        private static readonly Dictionary<string, MapWinGIS.Image> _dem = new Dictionary<string, MapWinGIS.Image>();

        /// <summary>Ảnh ký hiệu cho MapWinGIS (dùng lại nếu đã tạo).</summary>
        public static MapWinGIS.Image Anh(Color nen, Color chu, int px, string glyph = null, bool tron = false)
        {
            glyph = glyph ?? Icons.Bus;
            string khoa = $"{nen.ToArgb():X8}_{chu.ToArgb():X8}_{px}_{(int)glyph[0]:X4}{(tron ? "_t" : "")}";
            if (_dem.TryGetValue(khoa, out var img)) return img;

            string thuMuc = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XeBus_GIS", "kyhieu");
            Directory.CreateDirectory(thuMuc);
            string tep = Path.Combine(thuMuc, "xebus_" + khoa + ".png");
            using (var bmp = Ve(nen, chu, px, glyph, tron))
                bmp.Save(tep, ImageFormat.Png);

            img = new MapWinGIS.Image();
            if (!img.Open(tep, MapWinGIS.ImageType.USE_FILE_EXTENSION, true, null)) return null;
            _dem[khoa] = img;
            return img;
        }

        /// <summary>
        /// Vẽ ký hiệu: ô bo góc (trạm) hoặc hình tròn (công trình) màu nền, viền trắng mảnh (tách khỏi nền bản đồ),
        /// icon ở giữa (mặc định xe buýt).
        /// </summary>
        public static Bitmap Ve(Color nen, Color chu, int px, string glyph = null, bool tron = false)
        {
            glyph = glyph ?? Icons.Bus;
            var bmp = new Bitmap(px, px, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                float vien = Math.Max(1f, px / 14f);
                var r = new RectangleF(vien / 2, vien / 2, px - vien - 0.5f, px - vien - 0.5f);
                if (tron)
                {
                    using (var br = new SolidBrush(nen)) g.FillEllipse(br, r);
                    using (var pen = new Pen(Color.White, vien)) g.DrawEllipse(pen, r);
                }
                else
                    using (var path = UiHelper.DuongBoGoc(r, Math.Max(2, (int)(px * 0.24f))))
                    {
                        using (var br = new SolidBrush(nen)) g.FillPath(br, path);
                        using (var pen = new Pen(Color.White, vien)) g.DrawPath(pen, path);
                    }
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                using (var f = new Font(Theme.TenFontIcon, px * (tron ? 0.48f : 0.5f), GraphicsUnit.Pixel))
                using (var br = new SolidBrush(chu))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(glyph, f, br, new RectangleF(0, px * 0.02f, px, px), sf);
            }
            return bmp;
        }
    }
}
