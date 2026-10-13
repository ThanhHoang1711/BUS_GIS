using System.Drawing;
using System.Drawing.Text;
using System.Linq;

namespace XeBus_GIS.UI
{
    /// <summary>
    /// Design tokens (docs/UI_DESIGN.md) – phong cách Modern GIS Desktop · Clean Enterprise · Light Urban Analytics.
    /// KHÔNG hard-code màu/font trong form – luôn lấy từ đây.
    /// </summary>
    public static class Theme
    {
        // ---- Màu giao diện ----
        public static readonly Color Primary = ColorTranslator.FromHtml("#172033");       // chữ tiêu đề / nhấn đậm
        public static readonly Color Secondary = ColorTranslator.FromHtml("#334155");     // chữ nút phụ
        public static readonly Color Accent = ColorTranslator.FromHtml("#2563EB");        // hành động chính
        public static readonly Color AccentHover = ColorTranslator.FromHtml("#1D4ED8");
        public static readonly Color AccentPressed = ColorTranslator.FromHtml("#1E40AF");
        public static readonly Color AccentSoft = ColorTranslator.FromHtml("#EAF1FE");    // nền mục đang chọn
        public static readonly Color AccentSoftHover = ColorTranslator.FromHtml("#DCE7FD");
        public static readonly Color Accent2 = ColorTranslator.FromHtml("#06B6D4");       // vùng phục vụ / biểu đồ phụ
        public static readonly Color Highlight = ColorTranslator.FromHtml("#F97316");     // trạm đề xuất / ứng viên
        public static readonly Color Success = ColorTranslator.FromHtml("#16A34A");
        public static readonly Color Warning = ColorTranslator.FromHtml("#F59E0B");
        public static readonly Color WarningText = ColorTranslator.FromHtml("#B45309");
        public static readonly Color Danger = ColorTranslator.FromHtml("#DC2626");
        public static readonly Color DangerHover = ColorTranslator.FromHtml("#B91C1C");
        public static readonly Color Background = ColorTranslator.FromHtml("#F5F7FB");
        public static readonly Color Surface = Color.White;
        public static readonly Color SurfaceAlt = ColorTranslator.FromHtml("#F8FAFC");    // header lưới, vùng phụ
        public static readonly Color Border = ColorTranslator.FromHtml("#E2E8F0");
        public static readonly Color BorderStrong = ColorTranslator.FromHtml("#CBD5E1");
        public static readonly Color Text = ColorTranslator.FromHtml("#172033");
        public static readonly Color Muted = ColorTranslator.FromHtml("#64748B");
        public static readonly Color Disabled = ColorTranslator.FromHtml("#94A3B8");
        public static readonly Color RowAlt = ColorTranslator.FromHtml("#FAFBFD");
        public static readonly Color RowSelect = AccentSoft;
        public static readonly Color HeaderHover = ColorTranslator.FromHtml("#F1F5F9");
        public static readonly Color SurfaceHover = ColorTranslator.FromHtml("#F1F5F9");

        // ---- Màu ký hiệu bản đồ (mang nghĩa nghiệp vụ – giữ ổn định) ----
        public static readonly Color MapTram = Accent;
        public static readonly Color MapDuong = ColorTranslator.FromHtml("#94A3B8");
        public static readonly Color MapKhuDanCu = ColorTranslator.FromHtml("#FDE68A");
        public static readonly Color MapCongTrinh = ColorTranslator.FromHtml("#7C3AED");     // công trình khác
        public static readonly Color MapCtYTe = ColorTranslator.FromHtml("#DC2626");         // bệnh viện, y tế
        public static readonly Color MapCtGiaoDuc = ColorTranslator.FromHtml("#16A34A");     // trường học, đại học
        public static readonly Color MapCtThuongMai = ColorTranslator.FromHtml("#D97706");   // TTTM, siêu thị, chợ
        public static readonly Color MapCtCongNghiep = ColorTranslator.FromHtml("#78350F");  // khu công nghiệp
        public static readonly Color MapPhuongXa = ColorTranslator.FromHtml("#64748B");
        public static readonly Color MapRanhGioi = ColorTranslator.FromHtml("#172033");
        public static readonly Color MapBenXe = ColorTranslator.FromHtml("#172033");
        public static readonly Color MapTramDeXuat = Highlight;
        public static readonly Color MapTramNgung = ColorTranslator.FromHtml("#9CA3AF");
        public static readonly Color MapThieuTram = ColorTranslator.FromHtml("#F87171");     // KDC thiếu trạm
        public static readonly Color MapVungPhucVu = Accent2;
        /// <summary>Đối tượng đang chọn trên bản đồ (vàng – khác màu cam của trạm đề xuất).</summary>
        public static readonly Color MapChon = ColorTranslator.FromHtml("#FACC15");
        public static readonly Color MapChonVien = ColorTranslator.FromHtml("#A16207");
        public static readonly Color MapChonDuong = ColorTranslator.FromHtml("#EAB308");   // đường/tuyến đang chọn

        /// <summary>Bảng 8 màu phân biệt cho các tuyến.</summary>
        public static readonly Color[] MauTuyen =
        {
            ColorTranslator.FromHtml("#2563EB"), ColorTranslator.FromHtml("#DC2626"),
            ColorTranslator.FromHtml("#16A34A"), ColorTranslator.FromHtml("#9333EA"),
            ColorTranslator.FromHtml("#EA580C"), ColorTranslator.FromHtml("#0891B2"),
            ColorTranslator.FromHtml("#DB2777"), ColorTranslator.FromHtml("#65A30D"),
        };

        /// <summary>Thang màu số trạm (0 = đỏ, rồi xanh nhạt → đậm).</summary>
        public static readonly Color[] ThangMauSoTram =
        {
            ColorTranslator.FromHtml("#FCA5A5"), ColorTranslator.FromHtml("#BFDBFE"), ColorTranslator.FromHtml("#60A5FA"),
            ColorTranslator.FromHtml("#2563EB"), ColorTranslator.FromHtml("#1E3A8A"),
        };

        /// <summary>Thang màu % dân được phục vụ (thấp = đỏ → cao = xanh lá).</summary>
        public static readonly Color[] ThangMauPhucVu =
        {
            Danger, ColorTranslator.FromHtml("#F97316"), ColorTranslator.FromHtml("#FACC15"), Success,
        };

        /// <summary>Màu (nền, chữ) cho nhãn trạng thái trong lưới.</summary>
        public static void MauTrangThai(string trangThai, out Color nen, out Color chu)
        {
            switch ((trangThai ?? "").Trim())
            {
                case "Hoạt động":
                    nen = ColorTranslator.FromHtml("#DCFCE7"); chu = ColorTranslator.FromHtml("#15803D"); break;
                case "Đề xuất":
                    nen = ColorTranslator.FromHtml("#FFEDD5"); chu = ColorTranslator.FromHtml("#C2410C"); break;
                case "Tạm dừng":
                    nen = ColorTranslator.FromHtml("#FEF3C7"); chu = WarningText; break;
                case "Ngừng hoạt động":
                case "Khóa":
                case "Thất bại":
                    nen = ColorTranslator.FromHtml("#FEE2E2"); chu = ColorTranslator.FromHtml("#B91C1C"); break;
                case "Thành công":
                    nen = ColorTranslator.FromHtml("#DCFCE7"); chu = ColorTranslator.FromHtml("#15803D"); break;
                default:
                    nen = ColorTranslator.FromHtml("#F1F5F9"); chu = Muted; break;
            }
        }

        // ---- Khoảng cách (bội số 4) ----
        public const int Sp1 = 4, Sp2 = 8, Sp3 = 12, Sp4 = 16, Sp5 = 24, Sp6 = 32;
        public const int BoGoc = 6;

        // ---- Font ----
        public const string FontChinh = "Segoe UI";
        public static readonly Font FontAppTitle = new Font("Segoe UI Semibold", 12f);
        public static readonly Font FontTrang = new Font("Segoe UI Semibold", 15f);
        public static readonly Font FontTieuDe = new Font("Segoe UI Semibold", 11.25f);
        public static readonly Font FontThuong = new Font(FontChinh, 9.75f);
        public static readonly Font FontNhan = new Font(FontChinh, 9f);
        public static readonly Font FontNho = new Font(FontChinh, 8.25f);
        public static readonly Font FontDam = new Font("Segoe UI Semibold", 9.75f);
        public static readonly Font FontKpi = new Font("Segoe UI Semibold", 20f);
        public static readonly Font FontSo = new Font("Consolas", 9.75f);

        private static string _fontIcon;

        /// <summary>Font icon: Segoe Fluent Icons (Win11) hoặc Segoe MDL2 Assets (Win10).</summary>
        public static string TenFontIcon
        {
            get
            {
                if (_fontIcon == null)
                {
                    using (var fc = new InstalledFontCollection())
                    {
                        var ten = fc.Families.Select(f => f.Name).ToList();
                        _fontIcon = ten.Contains("Segoe Fluent Icons") ? "Segoe Fluent Icons"
                                  : ten.Contains("Segoe MDL2 Assets") ? "Segoe MDL2 Assets"
                                  : FontChinh;
                    }
                }
                return _fontIcon;
            }
        }

        public static Font FontIcon(float size) => new Font(TenFontIcon, size);

        /// <summary>Đổi Color sang uint màu OLE (BGR) dùng cho MapWinGIS.</summary>
        public static uint Ole(Color c) => (uint)ColorTranslator.ToOle(c);
    }
}
