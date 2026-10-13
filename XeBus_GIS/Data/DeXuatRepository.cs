using System;
using System.Data;

namespace XeBus_GIS.Data
{
    /// <summary>
    /// Quy trình 3 – Đề xuất trạm xe mới. Vùng phân tích truyền dạng WKT EPSG:3857 (từ bản đồ);
    /// null = toàn thành phố. Kết quả hình học trả về cột "wkt" EPSG:3857 để vẽ lớp tạm.
    /// </summary>
    public static class DeXuatRepository
    {
        private const string Vung = "ST_Transform(ST_GeomFromText(@vung::text, 3857), 4326)";

        /// <summary>Ranh giới phường – WKT 3857 và diện tích km².</summary>
        public static DataRow VungPhuong(string maPhuong)
        {
            var dt = Db.Query(@"SELECT tenphuong, ST_AsText(ST_Transform(geom, 3857)) AS wkt,
                                       round((ST_Area(geom::geography) / 1e6)::numeric, 2) AS dientich
                                FROM xebus.phuongxa WHERE maphuong = @m", "@m", maPhuong);
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        /// <summary>Diện tích (km²) của vùng vẽ (WKT 3857).</summary>
        public static double DienTichKm2(string wkt3857) =>
            Convert.ToDouble(Db.Scalar("SELECT ST_Area(" + Vung + "::geography) / 1e6", "@vung", wkt3857));

        /// <summary>Tổng quan phục vụ trong vùng: số trạm, dân số, % dân được phục vụ, công trình…</summary>
        public static DataRow TongQuan(string vung3857, double banKinh)
        {
            var dt = Db.Query("SELECT * FROM xebus.fn_thongke_phamvi(@r, " + Vung + ")",
                              "@r", banKinh, "@vung", (object)vung3857);
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        /// <summary>
        /// Vùng phục vụ (hợp vòng bán kính r quanh các trạm) trong vùng phân tích (null = toàn TP), tách thành từng polygon
        /// (cột ma, wkt EPSG:3857) – MapWinGIS nhập một MultiPolygon lớn nhiều lỗ bằng WKT bị nối sai các phần.
        /// </summary>
        public static DataTable VungPhucVu(string vung3857, double banKinh) =>
            Db.Query(@"SELECT 'PV' || d.path[1] AS ma, ST_AsText(ST_Transform(d.geom, 3857)) AS wkt
                       FROM ST_Dump(ST_SimplifyPreserveTopology(xebus.fn_vung_phucvu(@r, " + Vung + @"), 0.00005)) d
                       WHERE NOT ST_IsEmpty(d.geom)",
                     "@r", banKinh, "@vung", (object)vung3857);

        /// <summary>3.2 – Khu vực thiếu trạm (phần chưa được phục vụ của từng khu dân cư).</summary>
        public static DataTable KhuVucThieuTram(string vung3857, double banKinh, double nguong) =>
            Db.Query(@"SELECT k.makhu AS ma, k.tenkhu AS ""Khu dân cư"", p.tenphuong AS ""Phường/xã"",
                              k.danso AS ""Dân số"", round(k.tyle_phucvu * 100)::int AS ""% phục vụ"",
                              k.danso_chua_pv AS ""Dân chưa PV"", k.kc_tram_gan_nhat::int AS ""Trạm gần nhất (m)"",
                              ST_AsText(ST_Transform(k.geom, 3857)) AS wkt
                       FROM xebus.fn_khuvuc_thieutram(" + Vung + @", @r, @ng) k
                       LEFT JOIN xebus.khudancu kd ON kd.makhu = k.makhu
                       LEFT JOIN xebus.phuongxa p ON p.maphuong = kd.maphuong
                       ORDER BY k.danso_chua_pv DESC",
                     "@vung", (object)vung3857, "@r", banKinh, "@ng", nguong);

        /// <summary>3.3 + 3.4 – Ứng viên trạm (đã chấm điểm, chọn tham lam theo phủ tối đa).</summary>
        public static DataTable UngVien(string vung3857, double banKinh, double cachToiThieu, int toiDa, double nguong)
        {
            // chạy trong transaction riêng: hàm dùng bảng tạm (temp table)
            DataTable kq = null;
            Db.Transaction((conn, tx) =>
            {
                using (var cmd = new Npgsql.NpgsqlCommand(@"
                    SELECT u.stt AS ""#"", u.nguon AS ""Vị trí"", coalesce(u.tenduong, '') AS ""Đường / khu"",
                           u.tenphuong AS ""Phường/xã"", u.dan_moi AS ""Dân PV thêm"", u.so_kdc AS ""Số KDC"",
                           u.so_ct_moi AS ""CT PV thêm"", u.diem_ct AS ""Điểm CT"", u.tong_diem AS ""Tổng điểm"",
                           u.kc_tram_gan_nhat AS ""Cách trạm cũ (m)"",
                           u.kinhdo, u.vido, u.madoan, u.maphuong,
                           ST_AsText(ST_Transform(u.geom, 3857)) AS wkt
                    FROM xebus.fn_ungvien(" + Vung + @", @r, @cach, @buoc, @toida, @ng) u
                    ORDER BY u.stt", conn, tx))
                {
                    cmd.CommandTimeout = 600;
                    cmd.Parameters.AddWithValue("@vung", (object)vung3857 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@r", banKinh);
                    cmd.Parameters.AddWithValue("@cach", cachToiThieu);
                    cmd.Parameters.AddWithValue("@buoc", vung3857 == null ? 200.0 : 150.0);
                    cmd.Parameters.AddWithValue("@toida", toiDa);
                    cmd.Parameters.AddWithValue("@ng", nguong);
                    using (var rd = cmd.ExecuteReader())
                    {
                        kq = new DataTable();
                        kq.Load(rd);
                    }
                }
            });
            return kq;
        }

        /// <summary>Đánh giá vị trí người dùng tự chọn (x, y EPSG:3857) – cùng công thức với ứng viên.</summary>
        public static DataRow DanhGiaDiem(double x, double y, double banKinh)
        {
            var dt = Db.Query(@"SELECT d.*, ST_X(p.g) AS kinhdo, ST_Y(p.g) AS vido
                                FROM (SELECT ST_Transform(ST_SetSRID(ST_MakePoint(@x, @y), 3857), 4326) AS g) p,
                                     xebus.fn_danhgia_diem(p.g, @r) d",
                              "@x", x, "@y", y, "@r", banKinh);
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        /// <summary>Vòng phục vụ bán kính r quanh điểm (kinh độ, vĩ độ) – WKT 3857.</summary>
        public static string WktVongPhucVu(double kinhDo, double viDo, double banKinh) =>
            Convert.ToString(Db.Scalar(@"SELECT ST_AsText(ST_Transform(
                                            ST_Buffer(ST_SetSRID(ST_MakePoint(@x, @y), 4326)::geography, @r, 'quad_segs=16')::geometry, 3857))",
                                       "@x", kinhDo, "@y", viDo, "@r", banKinh));

        /// <summary>3.5 – Tạo trạm đề xuất (trạng thái "Đề xuất"), trả về mã trạm mới.</summary>
        public static string TaoTramDeXuat(double kinhDo, double viDo, string ten, string maLoai, string maTuyen,
                                           int? idNguoiTao, string tenNguoiTao, double banKinh) =>
            Convert.ToString(Db.Scalar(
                "SELECT xebus.fn_tao_tram_dexuat(@x::numeric, @y::numeric, @ten, @loai, NULLIF(@tuyen, ''), @id, @tennd, @r)",
                "@x", kinhDo, "@y", viDo, "@ten", ten.Trim(), "@loai", maLoai, "@tuyen", maTuyen ?? "",
                "@id", (object)idNguoiTao, "@tennd", tenNguoiTao, "@r", banKinh));
    }
}
