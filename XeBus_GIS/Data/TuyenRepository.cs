using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace XeBus_GIS.Data
{
    /// <summary>
    /// Quy trình 2 – Quản lý tuyến xe buýt (B01 tuyenbuyt, bảng bổ sung tuyen_tram).
    /// Theo phân quyền Word: Người quản lý được E,R trên tuyến (không tạo/xoá tuyến), gán trạm qua tuyen_tram.
    /// </summary>
    public static class TuyenRepository
    {
        public static readonly string[] TrangThaiTuyen = { "Hoạt động", "Tạm dừng", "Ngừng hoạt động" };

        private const string CotKetQua = @"
            t.matuyen  AS ""Mã"",
            t.tentuyen AS ""Tên tuyến"",
            t.trangthai AS ""Trạng thái"",
            (SELECT count(*) FROM xebus.tuyen_tram x WHERE x.matuyen = t.matuyen)::int AS ""Số trạm"",
            round((ST_Length(t.geom::geography) / 1000)::numeric, 1) AS ""Dài (km)""";

        // ------------------------------------------------------------------
        // 2.2 Tra cứu theo thuộc tính
        // ------------------------------------------------------------------
        /// <param name="tuKhoa">mã hoặc tên tuyến (không dấu)</param>
        /// <param name="quaTram">tên trạm mà tuyến đi qua (không dấu)</param>
        /// <param name="maPhuong">tuyến có trạm trong phường/xã này</param>
        public static DataTable TraCuuThuocTinh(string tuKhoa, string trangThai, string quaTram, string maPhuong)
        {
            var sql = new StringBuilder("SELECT ").Append(CotKetQua).Append(" FROM xebus.tuyenbuyt t WHERE true");
            var ts = new List<object>();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                sql.Append(" AND (t.matuyen ILIKE @tk OR xebus.unaccent_vn(t.tentuyen) LIKE '%' || xebus.unaccent_vn(@tk) || '%'" +
                           " OR xebus.unaccent_vn(coalesce(t.mota, '')) LIKE '%' || xebus.unaccent_vn(@tk) || '%')");
                ts.AddRange(new object[] { "@tk", tuKhoa.Trim() });
            }
            if (!string.IsNullOrEmpty(trangThai))
            {
                sql.Append(" AND t.trangthai = @tt");
                ts.AddRange(new object[] { "@tt", trangThai });
            }
            if (!string.IsNullOrWhiteSpace(quaTram))
            {
                sql.Append(@" AND EXISTS (SELECT 1 FROM xebus.tuyen_tram x JOIN xebus.trambuyt s ON s.matram = x.matram
                                          WHERE x.matuyen = t.matuyen
                                            AND (s.matram ILIKE @tram OR xebus.unaccent_vn(s.tentram) LIKE '%' || xebus.unaccent_vn(@tram) || '%'))");
                ts.AddRange(new object[] { "@tram", quaTram.Trim() });
            }
            if (!string.IsNullOrEmpty(maPhuong))
            {
                sql.Append(@" AND EXISTS (SELECT 1 FROM xebus.tuyen_tram x JOIN xebus.trambuyt s ON s.matram = x.matram
                                          WHERE x.matuyen = t.matuyen AND s.maphuong = @phuong)");
                ts.AddRange(new object[] { "@phuong", maPhuong });
            }
            sql.Append(" ORDER BY t.matuyen");
            return Db.Query(sql.ToString(), ts.ToArray());
        }

        // ------------------------------------------------------------------
        // 2.3 Tra cứu theo không gian
        // ------------------------------------------------------------------
        /// <summary>Tuyến đi qua vùng vẽ (WKT EPSG:3857).</summary>
        public static DataTable TraCuuQuaVung(string wkt3857) =>
            Db.Query("SELECT " + CotKetQua + @" FROM xebus.tuyenbuyt t
                     WHERE t.geom IS NOT NULL AND ST_Intersects(t.geom, ST_Transform(ST_GeomFromText(@wkt, 3857), 4326))
                     ORDER BY t.matuyen", "@wkt", wkt3857);

        /// <summary>Tuyến đi gần điểm (x, y EPSG:3857) trong bán kính r mét, kèm khoảng cách.</summary>
        public static DataTable TraCuuGanDiem(double x, double y, double banKinhM) =>
            Db.Query("SELECT " + CotKetQua + @",
                         round(ST_Distance(t.geom::geography, d.g::geography))::int AS ""Cách (m)""
                     FROM xebus.tuyenbuyt t
                     CROSS JOIN (SELECT ST_Transform(ST_SetSRID(ST_MakePoint(@x, @y), 3857), 4326) AS g) d
                     WHERE t.geom IS NOT NULL AND ST_DWithin(t.geom::geography, d.g::geography, @r)
                     ORDER BY 6", "@x", x, "@y", y, "@r", banKinhM);

        // ------------------------------------------------------------------
        // 2.1 Xem thông tin tuyến
        // ------------------------------------------------------------------
        public static DataRow Lay(string maTuyen)
        {
            var dt = Db.Query(@"SELECT matuyen, tentuyen, mota, trangthai, to_char(ngaytao, 'DD/MM/YYYY') AS ngaytao,
                                       to_char(ngaycapnhat, 'DD/MM/YYYY HH24:MI') AS ngaycapnhat, tenntao,
                                       round((ST_Length(geom::geography) / 1000)::numeric, 2) AS chieudai
                                FROM xebus.tuyenbuyt WHERE matuyen = @m", "@m", maTuyen);
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        /// <summary>Danh sách trạm trên tuyến theo thứ tự, kèm toạ độ 3857 để vẽ nhãn số thứ tự.</summary>
        public static DataTable TramTrenTuyen(string maTuyen) =>
            Db.Query(@"SELECT x.thutu AS ""#"", s.matram AS ""Mã"", s.tentram AS ""Tên trạm"", p.tenphuong AS ""Phường/xã"",
                              ST_AsText(ST_Transform(s.geom, 3857)) AS wkt
                       FROM xebus.tuyen_tram x
                       JOIN xebus.trambuyt s ON s.matram = x.matram
                       LEFT JOIN xebus.phuongxa p ON p.maphuong = s.maphuong
                       WHERE x.matuyen = @m ORDER BY x.thutu", "@m", maTuyen);

        /// <summary>Thông tin các trạm theo danh sách mã (giữ đúng thứ tự truyền vào), có wkt EPSG:3857.</summary>
        public static DataTable ThongTinTram(string[] danhSachMa) =>
            Db.Query(@"SELECT s.matram AS ""Mã"", s.tentram AS ""Tên trạm"", p.tenphuong AS ""Phường/xã"",
                              ST_AsText(ST_Transform(s.geom, 3857)) AS wkt
                       FROM unnest(@ds::text[]) WITH ORDINALITY u(ma, stt)
                       JOIN xebus.trambuyt s ON s.matram = u.ma
                       LEFT JOIN xebus.phuongxa p ON p.maphuong = s.maphuong
                       ORDER BY u.stt", "@ds", danhSachMa);

        // ------------------------------------------------------------------
        // 2.4 Cập nhật thuộc tính tuyến
        // ------------------------------------------------------------------
        public static void CapNhat(string maTuyen, string ten, string moTa, string trangThai) =>
            Db.Execute(@"UPDATE xebus.tuyenbuyt SET tentuyen = @ten, mota = NULLIF(@mota, ''), trangthai = @tt
                         WHERE matuyen = @m",
                       "@ten", ten.Trim(), "@mota", (moTa ?? "").Trim(), "@tt", trangThai, "@m", maTuyen);

        // ------------------------------------------------------------------
        // 2.5 Gán trạm vào tuyến
        // ------------------------------------------------------------------
        /// <summary>
        /// Ghi lại toàn bộ danh sách trạm của tuyến theo thứ tự (1..n) trong một transaction.
        /// Đồng bộ cột trambuyt.matuyen (tuyến chính theo thiết kế Word):
        ///   - trạm mới gán chưa có tuyến chính => nhận tuyến này;
        ///   - trạm bị gỡ khỏi tuyến mà tuyến chính đang là tuyến này => chuyển sang tuyến khác nó thuộc (hoặc NULL).
        /// dungLaiHinh = true: dựng lại đường đi tuyến theo mạng đường (fn_dung_hinh_tuyen).
        /// Trả về số đoạn phải nối thẳng do không tìm được đường (-1 nếu không dựng lại).
        /// </summary>
        public static int LuuDanhSachTram(string maTuyen, IList<string> danhSachTram, bool dungLaiHinh)
        {
            int noiThang = -1;
            Db.Transaction((conn, tx) =>
            {
                Db.Execute(conn, tx, "DELETE FROM xebus.tuyen_tram WHERE matuyen = @m", "@m", maTuyen);
                for (int i = 0; i < danhSachTram.Count; i++)
                    Db.Execute(conn, tx, "INSERT INTO xebus.tuyen_tram (matuyen, matram, thutu) VALUES (@m, @t, @i)",
                               "@m", maTuyen, "@t", danhSachTram[i], "@i", (short)(i + 1));

                Db.Execute(conn, tx, @"
                    UPDATE xebus.trambuyt s SET matuyen = @m
                    WHERE s.matuyen IS NULL AND s.matram IN (SELECT matram FROM xebus.tuyen_tram WHERE matuyen = @m)", "@m", maTuyen);
                Db.Execute(conn, tx, @"
                    UPDATE xebus.trambuyt s SET matuyen = (SELECT min(x.matuyen) FROM xebus.tuyen_tram x WHERE x.matram = s.matram)
                    WHERE s.matuyen = @m AND NOT EXISTS (SELECT 1 FROM xebus.tuyen_tram x WHERE x.matram = s.matram AND x.matuyen = @m)",
                    "@m", maTuyen);

                if (dungLaiHinh)
                {
                    using (var cmd = new Npgsql.NpgsqlCommand("SELECT xebus.fn_dung_hinh_tuyen(@m)", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@m", maTuyen);
                        cmd.CommandTimeout = 300;
                        noiThang = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            });
            return noiThang;
        }
    }
}
