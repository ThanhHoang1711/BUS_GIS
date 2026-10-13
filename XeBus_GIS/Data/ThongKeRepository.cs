using System;
using System.Data;

namespace XeBus_GIS.Data
{
    /// <summary>Quy trình 4 – Thống kê trạm xe buýt (theo phường/xã, theo tuyến, phạm vi phục vụ).</summary>
    public static class ThongKeRepository
    {
        /// <summary>Chỉ tiêu tổng quan toàn thành phố.</summary>
        public static DataRow TongQuan(double banKinh)
        {
            var dt = Db.Query(@"SELECT f.*,
                                       (SELECT count(*) FROM xebus.tuyenbuyt WHERE trangthai = 'Hoạt động')::int AS so_tuyen,
                                       (SELECT count(*) FROM xebus.trambuyt WHERE trangthai = 'Đề xuất')::int AS so_dexuat,
                                       (SELECT count(*) FROM xebus.phuongxa p WHERE NOT EXISTS (
                                            SELECT 1 FROM xebus.trambuyt t WHERE t.maphuong = p.maphuong AND t.trangthai <> 'Ngừng hoạt động'))::int AS phuong_khong_tram
                                FROM xebus.fn_thongke_phamvi(@r, NULL) f", "@r", banKinh);
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        // ------------------------------------------------------------------
        // 4.1 Số lượng trạm theo khu vực
        // ------------------------------------------------------------------
        /// <summary>Thống kê theo phường/xã (kèm phạm vi phục vụ r) – có cột ma, wkt (3857).</summary>
        public static DataTable TheoPhuong(double banKinh) =>
            Db.Query(@"SELECT f.maphuong AS ma, f.tenphuong AS ""Phường/xã"", f.danso AS ""Dân số"",
                              f.dientich_km2 AS ""Diện tích (km²)"", f.so_tram AS ""Số trạm"",
                              f.so_tram_dexuat AS ""Trạm đề xuất"", f.tram_tren_km2 AS ""Trạm/km²"",
                              f.dan_tren_tram AS ""Dân/trạm"", f.so_tuyen AS ""Số tuyến"",
                              f.dan_kdc AS ""Dân KDC (ước tính)"", f.dan_kdc_pv AS ""Dân KDC được PV"",
                              f.tyle_pv AS ""% dân được PV"", f.so_ct AS ""Công trình"", f.so_ct_pv AS ""CT được PV"",
                              ST_AsText(ST_Transform(ST_SimplifyPreserveTopology(p.geom, 0.0001), 3857)) AS wkt
                       FROM xebus.fn_thongke_phuong(@r) f JOIN xebus.phuongxa p ON p.maphuong = f.maphuong
                       ORDER BY f.so_tram DESC, f.tenphuong", "@r", banKinh);

        /// <summary>Thống kê theo lưới ô vuông (kích thước m) – có cột ma, wkt.</summary>
        public static DataTable TheoLuoi(double kichThuocM) =>
            Db.Query(@"SELECT o_id AS ma, so_tram AS ""Số trạm"", ST_AsText(ST_Transform(geom, 3857)) AS wkt
                       FROM xebus.fn_thongke_luoi(@k, NULL) ORDER BY so_tram DESC", "@k", kichThuocM);

        // ------------------------------------------------------------------
        // 4.2 Trạm theo tuyến
        // ------------------------------------------------------------------
        public static DataTable DanhSachTuyen() =>
            Db.Query(@"SELECT matuyen AS ma, matuyen || ' - ' || tentuyen AS ten, tentuyen, trangthai, so_tram, chieudai_km
                       FROM xebus.v_thongke_tuyen ORDER BY matuyen");

        public static DataTable TomTatTuyen(string[] maTuyen) =>
            Db.Query(@"SELECT v.matuyen AS ""Mã tuyến"", v.tentuyen AS ""Tên tuyến"", v.trangthai AS ""Trạng thái"",
                              v.so_tram AS ""Số trạm"", v.chieudai_km AS ""Chiều dài (km)"",
                              CASE WHEN v.so_tram > 1 THEN round((v.chieudai_km * 1000 / (v.so_tram - 1))::numeric) END AS ""Cự ly TB giữa trạm (m)"",
                              (SELECT count(DISTINCT s.maphuong) FROM xebus.tuyen_tram x JOIN xebus.trambuyt s ON s.matram = x.matram
                                WHERE x.matuyen = v.matuyen) AS ""Số phường/xã đi qua""
                       FROM xebus.v_thongke_tuyen v WHERE v.matuyen = ANY(@ds) ORDER BY v.matuyen", "@ds", maTuyen);

        public static DataTable TramTheoTuyen(string[] maTuyen) =>
            Db.Query(@"SELECT x.matuyen AS ""Tuyến"", x.thutu AS ""#"", s.matram AS ""Mã trạm"", s.tentram AS ""Tên trạm"",
                              l.tenloaitram AS ""Loại"", s.trangthai AS ""Trạng thái"", p.tenphuong AS ""Phường/xã""
                       FROM xebus.tuyen_tram x
                       JOIN xebus.trambuyt s ON s.matram = x.matram
                       LEFT JOIN xebus.loaitram l ON l.maloaitram = s.maloaitram
                       LEFT JOIN xebus.phuongxa p ON p.maphuong = s.maphuong
                       WHERE x.matuyen = ANY(@ds) ORDER BY x.matuyen, x.thutu", "@ds", maTuyen);

        // ------------------------------------------------------------------
        // 4.3 Phạm vi phục vụ
        // ------------------------------------------------------------------
        public static DataTable CongTrinhPhucVu(double banKinh) =>
            Db.Query(@"SELECT tenloai AS ""Loại công trình"", so_ct AS ""Số công trình"", so_ct_pv AS ""Có trạm trong bán kính"",
                              tyle_pv AS ""% được phục vụ""
                       FROM xebus.fn_thongke_ct_phucvu(@r)", "@r", banKinh);

        /// <summary>Vùng phục vụ toàn TP (hợp các vòng tròn bán kính r quanh trạm) – từng polygon, cột ma + wkt 3857.</summary>
        public static DataTable VungPhucVu(double banKinh) => DeXuatRepository.VungPhucVu(null, banKinh);
    }
}
