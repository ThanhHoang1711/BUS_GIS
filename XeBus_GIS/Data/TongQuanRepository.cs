using System.Data;

namespace XeBus_GIS.Data
{
    /// <summary>Số liệu màn hình Tổng quan – chỉ đọc, lấy trực tiếp từ CSDL (không có số liệu dựng sẵn).</summary>
    public static class TongQuanRepository
    {
        public const double BanKinhMacDinh = 500;

        /// <summary>Toàn bộ số liệu của màn hình (đọc một lượt ở luồng nền).</summary>
        public class SoLieu
        {
            /// <summary>Hàng tổng quan của <see cref="ThongKeRepository.TongQuan"/> (so_tram, so_tuyen, so_dexuat, tyle_pv, phuong_khong_tram…).</summary>
            public DataRow TongQuan;
            public int TongSoTram;
            public int SoPhuongXa;
            public DataTable TopPhuong;
            public DataTable PhuongCanChuY;
            public DataTable CongTrinhPhucVu;
            public DataTable CapNhatGanDay;
        }

        public static SoLieu Lay()
        {
            return new SoLieu
            {
                TongQuan = ThongKeRepository.TongQuan(BanKinhMacDinh),
                TongSoTram = System.Convert.ToInt32(Db.Scalar("SELECT count(*) FROM xebus.trambuyt")),
                SoPhuongXa = System.Convert.ToInt32(Db.Scalar("SELECT count(*) FROM xebus.phuongxa")),
                TopPhuong = Db.Query(@"SELECT maphuong AS ma, tenphuong AS ten, so_tram
                                       FROM xebus.fn_thongke_phuong(@r)
                                       ORDER BY so_tram DESC, tenphuong LIMIT 12", "@r", BanKinhMacDinh),
                // chưa có trạm, hoặc dưới 30% dân KDC trong bán kính – xếp theo số dân KDC chưa được phục vụ
                PhuongCanChuY = Db.Query(@"SELECT maphuong AS ma, tenphuong AS ""Phường/xã"", so_tram AS ""Trạm"",
                                                  tyle_pv AS ""% dân PV"",
                                                  (dan_kdc - dan_kdc_pv) AS ""Dân chưa PV""
                                           FROM xebus.fn_thongke_phuong(@r)
                                           WHERE so_tram = 0 OR coalesce(tyle_pv, 0) < 30
                                           ORDER BY (so_tram = 0) DESC, (dan_kdc - dan_kdc_pv) DESC NULLS LAST
                                           LIMIT 15", "@r", BanKinhMacDinh),
                CongTrinhPhucVu = ThongKeRepository.CongTrinhPhucVu(BanKinhMacDinh),
                CapNhatGanDay = Db.Query(@"SELECT t.matram AS ""Mã"", t.tentram AS ""Tên trạm"", t.trangthai AS ""Trạng thái"",
                                                  p.tenphuong AS ""Phường/xã"",
                                                  to_char(coalesce(t.ngaycapnhat, t.ngaytao), 'DD/MM/YYYY HH24:MI') AS ""Cập nhật""
                                           FROM xebus.trambuyt t LEFT JOIN xebus.phuongxa p ON p.maphuong = t.maphuong
                                           ORDER BY coalesce(t.ngaycapnhat, t.ngaytao) DESC NULLS LAST, t.matram DESC
                                           LIMIT 8"),
            };
        }
    }
}
