using System;
using System.Data;

namespace XeBus_GIS.Data
{
    /// <summary>
    /// Công cụ "Chọn vị trí đặt trạm" (đa tiêu chí – database/13_chon_vitri.sql). Khu vực truyền dạng WKT EPSG:3857
    /// (từ bản đồ); hình học trả về cột "wkt" EPSG:3857 để vẽ lớp tạm. Chỉ đọc CSDL.
    /// </summary>
    public static class ChonViTriRepository
    {
        private const string Vung = "ST_Transform(ST_GeomFromText(@vung::text, 3857), 4326)";

        /// <summary>
        /// Ứng viên dọc đường trong khu vực + 5 tiêu chí chuẩn hoá 0..1 (c_dan, c_ct, c_tram, c_gt, c_tc) và số liệu gốc.
        /// buoc = khoảng cách giữa các điểm ứng viên dọc đường (m).
        /// </summary>
        public static DataTable ChamDiem(string vung3857, double banKinh, double buoc)
        {
            DataTable kq = null;
            using (var conn = Db.Open())
            using (var cmd = new Npgsql.NpgsqlCommand(
                "SELECT * FROM xebus.fn_chon_vitri(" + Vung + ", @r, @buoc)", conn))
            {
                cmd.CommandTimeout = 300;
                cmd.Parameters.AddWithValue("@vung", vung3857);
                cmd.Parameters.AddWithValue("@r", banKinh);
                cmd.Parameters.AddWithValue("@buoc", buoc);
                using (var rd = cmd.ExecuteReader())
                {
                    kq = new DataTable();
                    kq.Load(rd);
                }
            }
            return kq;
        }

        /// <summary>
        /// Mảnh dân cư chưa có trạm phục vụ trong khu vực (cắt theo khu vực, đã đơn giản hoá) – cột ma, md (người/km²), wkt.
        /// </summary>
        public static DataTable DanCuChuaPhucVu(string vung3857, double banKinh) =>
            Db.Query(@"WITH v AS (SELECT ST_Transform(" + Vung + @", 32648) AS gu)
                       SELECT 'D' || row_number() OVER () AS ma, round((x.md * 1e6)::numeric) AS md,
                              ST_AsText(ST_Transform(ST_SimplifyPreserveTopology(x.g, 3), 3857)) AS wkt
                       FROM (SELECT d.md, (ST_Dump(ST_CollectionExtract(ST_Intersection(d.gu, v.gu), 3))).geom AS g
                             FROM (SELECT f.makhu, f.md, ST_Union(f.gu) AS gu       -- gộp lại các mảnh đã chia nhỏ
                                   FROM xebus.fn_dancu_chua_pv(" + Vung + @", @r) f GROUP BY f.makhu, f.md) d, v
                             WHERE ST_Intersects(d.gu, v.gu)) x
                       WHERE ST_Area(x.g) > 50",
                     "@vung", vung3857, "@r", banKinh);

        /// <summary>Nút giao (đỉnh mạng đường nối ≥ 3 cạnh) trong khu vực – cột ma, wkt.</summary>
        public static DataTable NutGiao(string vung3857) =>
            Db.Query(@"SELECT 'N' || d.id AS ma, ST_AsText(ST_Transform(d.geom, 3857)) AS wkt
                       FROM xebus.mang_dinh d
                       WHERE ST_Intersects(d.geom, ST_Buffer(" + Vung + @"::geography, 100)::geometry)
                         AND (SELECT count(*) FROM xebus.mang_canh c WHERE c.source = d.id OR c.target = d.id) >= 3",
                     "@vung", vung3857);

        /// <summary>
        /// Công trình trong bán kính r quanh điểm (kinh độ, vĩ độ): tia nối điểm → công trình (cột wkt LINESTRING 3857),
        /// kieu = "1" công trình chưa có trạm phục vụ, "0" đã có.
        /// </summary>
        public static DataTable CongTrinhQuanh(double kinhDo, double viDo, double banKinh) =>
            Db.Query(@"WITH p AS (SELECT ST_SetSRID(ST_MakePoint(@x, @y), 4326) AS g)
                       SELECT c.macongtrinh AS ma, c.tencongtrinh AS ten, l.tenloaicongtrinh AS loai,
                              round(ST_Distance(c.geom::geography, p.g::geography))::int AS kc,
                              CASE WHEN EXISTS (SELECT 1 FROM xebus.trambuyt s
                                                WHERE s.trangthai <> 'Ngừng hoạt động'
                                                  AND ST_DWithin(s.geom::geography, c.geom::geography, @r))
                                   THEN '0' ELSE '1' END AS kieu,
                              ST_AsText(ST_Transform(ST_MakeLine(p.g, c.geom), 3857)) AS wkt
                       FROM xebus.congtrinh c JOIN xebus.loaicongtrinh l USING (maloaicongtrinh), p
                       WHERE c.trangthai <> 'Ngừng hoạt động' AND ST_DWithin(c.geom::geography, p.g::geography, @r)
                       ORDER BY kc",
                     "@x", kinhDo, "@y", viDo, "@r", banKinh);
    }
}
