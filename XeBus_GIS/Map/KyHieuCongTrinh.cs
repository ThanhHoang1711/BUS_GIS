using System.Drawing;
using System.Linq;
using XeBus_GIS.UI;

namespace XeBus_GIS.Map
{
    /// <summary>
    /// Ký hiệu từng loại công trình (mã loaicongtrinh): hình tròn màu nhóm + icon Segoe Fluent riêng
    /// (khác ô vuông bo góc của trạm). Dùng cho lớp Công trình trên bản đồ và chú giải.
    /// </summary>
    public static class KyHieuCongTrinh
    {
        public class Loai
        {
            public string Ma, Ten, Glyph;
            public Color Mau;
            public Loai(string ma, string ten, string glyph, Color mau) { Ma = ma; Ten = ten; Glyph = glyph; Mau = mau; }
        }

        public static readonly Loai[] CacLoai =
        {
            new Loai("TH", "Trường học", "", Theme.MapCtGiaoDuc),          // mũ tốt nghiệp
            new Loai("DH", "Đại học - cao đẳng", "", Theme.MapCtGiaoDuc),
            new Loai("BV", "Bệnh viện", "", Theme.MapCtYTe),               // nhịp tim
            new Loai("YT", "Phòng khám - y tế", "", Theme.MapCtYTe),
            new Loai("TT", "TTTM - siêu thị", "", Theme.MapCtThuongMai),   // xe đẩy
            new Loai("BL", "Chợ", "", Theme.MapCtThuongMai),               // túi mua hàng
            new Loai("TM", "Thương mại - dịch vụ", "", Theme.MapCtThuongMai),
            new Loai("CN", "Khu công nghiệp", "", Theme.MapCtCongNghiep),  // cụm nhà xưởng
            new Loai("HC", "Hành chính - công cộng", "", Theme.MapCtHanhChinh), // toà nhà cột
            new Loai("DL", "Văn hoá - du lịch", "", Theme.MapCongTrinh),   // ngôi sao
            new Loai("GT", "Thể thao - vui chơi", "", Theme.MapCtCongVien), // người chạy
            new Loai("CV", "Công viên", "", Theme.MapCtCongVien),          // lá cây
            new Loai("LT", "Lưu trú", "", Theme.MapCongTrinh),             // ngôi nhà
        };

        public static Loai Tim(string ma) => CacLoai.FirstOrDefault(l => l.Ma == ma);

        /// <summary>Nhóm hiển thị trong chú giải (các loại cùng icon + màu gộp một dòng).</summary>
        public static readonly (string ten, string glyph, Color mau)[] ChuGiai =
        {
            ("Trường học, đại học", "", Theme.MapCtGiaoDuc),
            ("Bệnh viện, y tế", "", Theme.MapCtYTe),
            ("TTTM, siêu thị", "", Theme.MapCtThuongMai),
            ("Chợ, thương mại", "", Theme.MapCtThuongMai),
            ("Khu công nghiệp", "", Theme.MapCtCongNghiep),
            ("Hành chính - công cộng", "", Theme.MapCtHanhChinh),
            ("Văn hoá - du lịch", "", Theme.MapCongTrinh),
            ("Thể thao - vui chơi", "", Theme.MapCtCongVien),
            ("Công viên", "", Theme.MapCtCongVien),
            ("Lưu trú", "", Theme.MapCongTrinh),
        };
    }
}
