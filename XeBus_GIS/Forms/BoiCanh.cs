using System;
using XeBus_GIS.Map;

namespace XeBus_GIS.Forms
{
    /// <summary>
    /// Ngữ cảnh dùng chung giữa form chính và các module (UserControl ở panel trái):
    /// truy cập bản đồ, công cụ chuột, panel thông tin, thanh trạng thái.
    /// </summary>
    public class BoiCanh
    {
        public MapManager BanDo { get; }
        public MapTools CongCu { get; }

        /// <summary>Hiển thị thuộc tính đối tượng (loai: TRAM/TUYEN/DOAN/CONGTRINH/KDC, mã) ở panel phải.</summary>
        public Action<string, string> HienThongTin { get; }

        /// <summary>Ghi thanh trạng thái (nội dung, loi = true nếu là lỗi).</summary>
        public Action<string, bool> TrangThai { get; }

        /// <summary>
        /// Chuyển sang màn hình khác (khoá: TONGQUAN, BANDO, TRAM, TUYEN, PHANTICH, DEXUAT, THONGKE, QUANTRI);
        /// trả về module của màn hình đó (null nếu không có quyền / là màn hình bản đồ).
        /// </summary>
        public Func<string, System.Windows.Forms.Control> MoModule { get; set; }

        /// <summary>
        /// Đặt một khối nội dung (thường là bảng kết quả) vào khung ngang dưới bản đồ và hiện khung đó
        /// (khung kéo giãn được bằng thanh chia). Module gọi trong KichHoat.
        /// </summary>
        public Action<System.Windows.Forms.Control> HienBangDuoi { get; set; }

        /// <summary>Ẩn khung dưới bản đồ nếu nó đang chứa khối nội dung này. Module gọi trong NgungKichHoat.</summary>
        public Action<System.Windows.Forms.Control> AnBangDuoi { get; set; }

        /// <summary>Phát ra khi người dùng click chọn một đối tượng trên bản đồ (loai, ma).</summary>
        public event Action<string, string> DoiTuongDuocChon;

        public BoiCanh(MapManager banDo, MapTools congCu, Action<string, string> hienThongTin, Action<string, bool> trangThai)
        {
            BanDo = banDo;
            CongCu = congCu;
            HienThongTin = hienThongTin;
            TrangThai = trangThai;
        }

        internal void PhatChon(string loai, string ma) => DoiTuongDuocChon?.Invoke(loai, ma);
    }

    /// <summary>Module chức năng đặt ở panel trái của form chính.</summary>
    public interface IModule
    {
        /// <summary>Gọi khi module được hiển thị (lần đầu hoặc chuyển tab quay lại).</summary>
        void KichHoat();

        /// <summary>Gọi khi chuyển sang module khác – dọn lớp tạm, huỷ công cụ đang dùng.</summary>
        void NgungKichHoat();
    }
}
