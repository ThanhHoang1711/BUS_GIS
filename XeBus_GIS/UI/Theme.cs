using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;

namespace XeBus_GIS.UI
{
    /// <summary>
    /// Design tokens (docs/UI_DESIGN.md) – Enterprise GIS Desktop, hai chế độ sáng / tối.
    /// KHÔNG hard-code màu/font trong form – luôn lấy từ đây. Token giao diện là thuộc tính đổi được khi chuyển chế độ
    /// (<see cref="ApDung"/>; áp lên control đang mở do <see cref="ThemeManager"/> làm). Màu ký hiệu bản đồ mang nghĩa
    /// nghiệp vụ giữ nguyên ở cả hai chế độ (trừ nét ranh giới/đường cần đổi để nhìn được trên nền tối).
    /// </summary>
    public static class Theme
    {
        /// <summary>Đang ở chế độ tối.</summary>
        public static bool Toi { get; private set; }

        // ---- Màu giao diện (đổi theo chế độ) ----
        public static Color Primary { get; private set; }         // chữ tiêu đề / nhấn đậm
        public static Color Secondary { get; private set; }       // chữ nút phụ
        public static Color Accent { get; private set; }          // hành động chính (teal)
        public static Color AccentHover { get; private set; }
        public static Color AccentPressed { get; private set; }
        /// <summary>Chữ/icon đặt trên nền Accent (trắng ở chế độ sáng, gần đen ở chế độ tối vì Accent sáng).</summary>
        public static Color ChuTrenAccent { get; private set; }
        public static Color AccentSoft { get; private set; }      // nền mục đang chọn
        public static Color AccentSoftHover { get; private set; }
        public static Color Accent2 { get; private set; }         // vùng phục vụ / biểu đồ phụ
        public static Color Success { get; private set; }
        public static Color Warning { get; private set; }
        public static Color WarningText { get; private set; }
        public static Color Danger { get; private set; }
        public static Color DangerHover { get; private set; }
        public static Color DangerSoft { get; private set; }      // nền hover nút xoá
        public static Color DangerSoftPressed { get; private set; }
        public static Color DangerBorder { get; private set; }
        public static Color Background { get; private set; }
        public static Color Surface { get; private set; }
        public static Color SurfaceAlt { get; private set; }      // header lưới, vùng phụ
        public static Color SurfaceHover { get; private set; }
        public static Color Border { get; private set; }
        public static Color BorderStrong { get; private set; }
        public static Color Text { get; private set; }
        public static Color Muted { get; private set; }
        public static Color Disabled { get; private set; }
        public static Color RowAlt { get; private set; }
        public static Color RowSelect { get; private set; }
        public static Color RowHover { get; private set; }
        public static Color HeaderHover { get; private set; }
        // ---- Thanh bên (tông nền riêng, đậm hơn vùng làm việc một bậc) ----
        public static Color Sidebar { get; private set; }
        public static Color SidebarHover { get; private set; }
        public static Color SidebarChon { get; private set; }

        // ---- Nét bản đồ đổi theo nền (ranh giới, đường, viền phường) ----
        public static Color MapDuong { get; private set; }
        public static Color MapPhuongXa { get; private set; }
        public static Color MapRanhGioi { get; private set; }

        // Highlight không đổi: màu cam của trạm đề xuất/ứng viên mang nghĩa nghiệp vụ
        public static readonly Color Highlight = ColorTranslator.FromHtml("#F97316");

        // ---- Trang đăng nhập / đăng ký: tông than chì trung tính (người dùng chọn bỏ xanh ngọc ở trang này) ----
        public static readonly Color DnNhan = ColorTranslator.FromHtml("#111827");        // nút chính, logo, liên kết
        public static readonly Color DnNhanHover = ColorTranslator.FromHtml("#1F2937");
        public static readonly Color DnNhanNhan = ColorTranslator.FromHtml("#030712");
        public static readonly Color DnPhu = Color.FromArgb(17, 24, 39);                  // lớp phủ trên video nền

        // ---- Màu ký hiệu bản đồ (mang nghĩa nghiệp vụ – giữ ổn định ở cả hai chế độ) ----
        public static readonly Color MapTram = ColorTranslator.FromHtml("#0F766E");
        public static readonly Color MapKhuDanCu = ColorTranslator.FromHtml("#FDE68A");
        public static readonly Color MapCongTrinh = ColorTranslator.FromHtml("#7C3AED");     // công trình khác
        public static readonly Color MapCtYTe = ColorTranslator.FromHtml("#DC2626");         // bệnh viện, y tế
        public static readonly Color MapCtGiaoDuc = ColorTranslator.FromHtml("#16A34A");     // trường học, đại học
        public static readonly Color MapCtThuongMai = ColorTranslator.FromHtml("#D97706");   // TTTM, siêu thị, chợ
        public static readonly Color MapCtCongNghiep = ColorTranslator.FromHtml("#78350F");  // khu công nghiệp
        public static readonly Color MapCtHanhChinh = ColorTranslator.FromHtml("#334155");   // hành chính - công cộng
        public static readonly Color MapCtCongVien = ColorTranslator.FromHtml("#4D7C0F");    // công viên, thể thao
        public static readonly Color MapBenXe = ColorTranslator.FromHtml("#172033");
        public static readonly Color MapTramDeXuat = Highlight;
        public static readonly Color MapTramNgung = ColorTranslator.FromHtml("#9CA3AF");
        public static readonly Color MapThieuTram = ColorTranslator.FromHtml("#F87171");     // KDC thiếu trạm
        public static readonly Color MapVungPhucVu = ColorTranslator.FromHtml("#0EA5E9");
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
            ColorTranslator.FromHtml("#DC2626"), ColorTranslator.FromHtml("#F97316"), ColorTranslator.FromHtml("#FACC15"),
            ColorTranslator.FromHtml("#16A34A"),
        };

        // ---- Công cụ "Chọn vị trí đặt trạm" (đa tiêu chí) ----
        /// <summary>Màu 5 tiêu chí: dân cư · công trình · trạm hiện có · giao thông · tiếp cận an toàn.</summary>
        public static readonly Color[] MauTieuChi =
        {
            ColorTranslator.FromHtml("#BE123C"), ColorTranslator.FromHtml("#D97706"), ColorTranslator.FromHtml("#0EA5E9"),
            ColorTranslator.FromHtml("#4F46E5"), ColorTranslator.FromHtml("#0F766E"),
        };

        /// <summary>Thang mức phù hợp 0 → 1 (kém đỏ → tốt xanh lá, 5 lớp).</summary>
        public static readonly Color[] ThangMauPhuHop =
        {
            ColorTranslator.FromHtml("#D7191C"), ColorTranslator.FromHtml("#F48C47"), ColorTranslator.FromHtml("#E9C74A"),
            ColorTranslator.FromHtml("#8CC65B"), ColorTranslator.FromHtml("#1A9641"),
        };

        /// <summary>Thang mật độ dân CHƯA có trạm phục vụ (nhạt → đậm, cùng tông màu tiêu chí dân cư).</summary>
        public static readonly Color[] ThangMauDanCu =
        {
            ColorTranslator.FromHtml("#FFE4E6"), ColorTranslator.FromHtml("#FDA4AF"), ColorTranslator.FromHtml("#FB7185"),
            ColorTranslator.FromHtml("#E11D48"), ColorTranslator.FromHtml("#881337"),
        };

        /// <summary>Màu (nền, chữ) cho nhãn trạng thái trong lưới.</summary>
        public static void MauTrangThai(string trangThai, out Color nen, out Color chu)
        {
            string n, c;
            switch ((trangThai ?? "").Trim())
            {
                case "Hoạt động":
                case "Thành công":
                    n = Toi ? "#12352A" : "#DCFCE7"; c = Toi ? "#4ADE80" : "#15803D"; break;
                case "Đề xuất":
                    n = Toi ? "#3D2614" : "#FFEDD5"; c = Toi ? "#FDBA74" : "#C2410C"; break;
                case "Chờ duyệt":
                case "Tạm dừng":
                    n = Toi ? "#3A2E12" : "#FEF3C7"; c = Toi ? "#FCD34D" : "#B45309"; break;
                case "Ngừng hoạt động":
                case "Khóa":
                case "Thất bại":
                    n = Toi ? "#3B1D22" : "#FEE2E2"; c = Toi ? "#FCA5A5" : "#B91C1C"; break;
                default:
                    nen = Toi ? ColorTranslator.FromHtml("#2A3446") : ColorTranslator.FromHtml("#F1F5F9");
                    chu = Muted;
                    return;
            }
            nen = ColorTranslator.FromHtml(n);
            chu = ColorTranslator.FromHtml(c);
        }

        // =====================================================================
        // Hai bảng màu
        // =====================================================================
        private static readonly Dictionary<string, string> BangSang = new Dictionary<string, string>
        {
            ["Primary"] = "#0F172A", ["Secondary"] = "#334155",
            ["Accent"] = "#0F766E", ["AccentHover"] = "#115E59", ["AccentPressed"] = "#134E4A", ["ChuTrenAccent"] = "#FFFFFF",
            ["AccentSoft"] = "#E6F4F1", ["AccentSoftHover"] = "#D1EBE6", ["Accent2"] = "#0EA5E9",
            ["Success"] = "#16A34A", ["Warning"] = "#F59E0B", ["WarningText"] = "#B45309",
            ["Danger"] = "#DC2626", ["DangerHover"] = "#B91C1C", ["DangerSoft"] = "#FEF2F2", ["DangerSoftPressed"] = "#FEE2E2", ["DangerBorder"] = "#FECACA",
            ["Background"] = "#F4F7FB", ["Surface"] = "#FFFFFF", ["SurfaceAlt"] = "#F8FAFB", ["SurfaceHover"] = "#F1F4F7",
            ["Border"] = "#E2E7EC", ["BorderStrong"] = "#CAD2DA",
            ["Text"] = "#0F172A", ["Muted"] = "#475569", ["Disabled"] = "#94A3B8",
            ["RowAlt"] = "#FAFBFC", ["RowSelect"] = "#E6F4F1", ["RowHover"] = "#F3F6F9", ["HeaderHover"] = "#F1F4F7",
            ["Sidebar"] = "#EEF2F5", ["SidebarHover"] = "#E2E8ED", ["SidebarChon"] = "#DCEFEA",
            ["MapDuong"] = "#94A3B8", ["MapPhuongXa"] = "#64748B", ["MapRanhGioi"] = "#172033",
        };

        private static readonly Dictionary<string, string> BangToi = new Dictionary<string, string>
        {
            ["Primary"] = "#F1F5F9", ["Secondary"] = "#CBD5E1",
            ["Accent"] = "#2DD4BF", ["AccentHover"] = "#5EEAD4", ["AccentPressed"] = "#14B8A6", ["ChuTrenAccent"] = "#0B1F1C",
            ["AccentSoft"] = "#17393A", ["AccentSoftHover"] = "#1D4746", ["Accent2"] = "#38BDF8",
            ["Success"] = "#22C55E", ["Warning"] = "#F59E0B", ["WarningText"] = "#FBBF24",
            ["Danger"] = "#F87171", ["DangerHover"] = "#EF4444", ["DangerSoft"] = "#3B1D22", ["DangerSoftPressed"] = "#4A2228", ["DangerBorder"] = "#7F1D1D",
            ["Background"] = "#171C26", ["Surface"] = "#202735", ["SurfaceAlt"] = "#252D3D", ["SurfaceHover"] = "#2A3446",
            ["Border"] = "#354155", ["BorderStrong"] = "#46536A",
            ["Text"] = "#E7EDF5", ["Muted"] = "#A5B1C2", ["Disabled"] = "#6B778A",
            ["RowAlt"] = "#232B3A", ["RowSelect"] = "#17393A", ["RowHover"] = "#28324A", ["HeaderHover"] = "#2A3446",
            ["Sidebar"] = "#1B2130", ["SidebarHover"] = "#262F40", ["SidebarChon"] = "#17393A",
            ["MapDuong"] = "#64748B", ["MapPhuongXa"] = "#94A3B8", ["MapRanhGioi"] = "#E2E8F0",
        };

        static Theme() { ApDung(false); }

        /// <summary>
        /// Gán lại toàn bộ token giao diện theo chế độ. Chỉ đổi giá trị – áp lên control đang mở dùng
        /// <see cref="ThemeManager.DoiCheDo"/>.
        /// </summary>
        public static void ApDung(bool toi)
        {
            Toi = toi;
            var b = toi ? BangToi : BangSang;
            foreach (var p in typeof(Theme).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
                if (p.PropertyType == typeof(Color) && b.TryGetValue(p.Name, out var hex))
                    p.SetValue(null, ColorTranslator.FromHtml(hex));
        }

        /// <summary>Ảnh chụp giá trị hiện tại của mọi token giao diện (tên → màu) – để ánh xạ màu cũ sang màu mới.</summary>
        public static Dictionary<string, Color> ChupBang()
        {
            var kq = new Dictionary<string, Color>();
            foreach (var p in typeof(Theme).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
                if (p.PropertyType == typeof(Color)) kq[p.Name] = (Color)p.GetValue(null);
            return kq;
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
