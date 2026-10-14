using System.Data;

namespace XeBus_GIS.Data
{
    /// <summary>
    /// Truy vấn dữ liệu hiển thị bản đồ. Hình học trả về cột "wkt" ở hệ Web Mercator (EPSG:3857)
    /// để khớp với bản đồ MapWinGIS (PROJECTION_GOOGLE_MERCATOR + nền OSM).
    /// </summary>
    public static class BanDoRepository
    {
        public const string SqlRanhGioi =
            @"SELECT 'HN' AS ma, tentinh AS ten, ST_AsText(ST_Transform(ST_Simplify(geom, 0.0002), 3857)) AS wkt
              FROM public.hanoi";

        public const string SqlDoanDuong =
            @"SELECT madoan AS ma, tenduong AS ten, loaiduong, matuyen,
                     ST_AsText(ST_Transform(geom, 3857)) AS wkt
              FROM xebus.doanduong";

        public const string SqlKhuDanCu =
            @"SELECT makhu AS ma, tenkhu AS ten, danso,
                     ST_AsText(ST_Transform(ST_SimplifyPreserveTopology(geom, 0.00002), 3857)) AS wkt
              FROM xebus.khudancu";

        public const string SqlPhuongXa =
            @"SELECT maphuong AS ma, tenphuong AS ten, danso,
                     ST_AsText(ST_Transform(ST_SimplifyPreserveTopology(geom, 0.0001), 3857)) AS wkt
              FROM xebus.phuongxa";

        /// <summary>Công trình dạng điểm; kieu = mã loại công trình (TH, DH, BV… – ký hiệu riêng từng loại, Map/KyHieuCongTrinh).</summary>
        public const string SqlCongTrinh =
            @"SELECT c.macongtrinh AS ma, c.tencongtrinh AS ten, l.tenloaicongtrinh AS loai,
                     coalesce(c.maloaicongtrinh, 'KH') AS kieu,
                     ST_AsText(ST_Transform(c.geom, 3857)) AS wkt
              FROM xebus.congtrinh c LEFT JOIN xebus.loaicongtrinh l USING (maloaicongtrinh)";

        public const string SqlTuyen =
            @"SELECT matuyen AS ma, tentuyen AS ten, trangthai,
                     ((row_number() OVER (ORDER BY matuyen) - 1) % 8)::text AS kieu,
                     ST_AsText(ST_Transform(geom, 3857)) AS wkt
              FROM xebus.tuyenbuyt WHERE geom IS NOT NULL ORDER BY matuyen";

        public const string SqlTram =
            @"SELECT matram AS ma, tentram AS ten, maloaitram, trangthai, matuyen,
                     CASE WHEN trangthai = 'Đề xuất' THEN 'DX'
                          WHEN trangthai = 'Ngừng hoạt động' THEN 'NG'
                          WHEN maloaitram = 'BX' THEN 'BX' ELSE 'TD' END AS kieu,
                     ST_AsText(ST_Transform(geom, 3857)) AS wkt
              FROM xebus.trambuyt";

        public static DataTable Lay(string sql) => Db.Query(sql);

        /// <summary>
        /// Tìm đối tượng gần điểm click nhất trong dung sai (x, y, dungSai đều theo mét của EPSG:3857).
        /// Ưu tiên: trạm -> công trình -> tuyến. Trả về (loai, ma) hoặc null.
        /// </summary>
        /// <param name="coTram">lớp trạm đang hiển thị</param>
        /// <param name="coCongTrinh">lớp công trình đang hiển thị (ẩn thì bỏ qua khi tra)</param>
        /// <param name="coTuyen">cho chọn tuyến (chỉ ở màn hình Tuyến xe buýt)</param>
        public static DataRow TimTaiDiem(double x, double y, double dungSai, bool coTram = true, bool coCongTrinh = true, bool coTuyen = false)
        {
            // Chỉ chọn đối tượng điểm (trạm luôn được ưu tiên; công trình khi lớp đang bật) – click trượt không còn
            // rơi vào đường, khu dân cư hay phường/xã. Tuyến chỉ chọn được khi đang ở màn hình Tuyến xe buýt.
            const string sql = @"
WITH p AS (SELECT ST_SetSRID(ST_MakePoint(@x, @y), 3857) AS m)
SELECT * FROM (
    (SELECT 1 AS uu_tien, 'TRAM' AS loai, t.matram AS ma, ST_Distance(ST_Transform(t.geom, 3857), p.m) AS kc
       FROM xebus.trambuyt t, p WHERE @tram AND ST_DWithin(ST_Transform(t.geom, 3857), p.m, @tol)
      ORDER BY 4 LIMIT 1)
    UNION ALL
    (SELECT 2, 'CONGTRINH', c.macongtrinh, ST_Distance(ST_Transform(c.geom, 3857), p.m)
       FROM xebus.congtrinh c, p WHERE @ct AND ST_DWithin(ST_Transform(c.geom, 3857), p.m, @tol)
      ORDER BY 4 LIMIT 1)
    UNION ALL
    (SELECT 3, 'TUYEN', t.matuyen, ST_Distance(ST_Transform(t.geom, 3857), p.m)
       FROM xebus.tuyenbuyt t, p WHERE @tuyen AND t.geom IS NOT NULL AND ST_DWithin(ST_Transform(t.geom, 3857), p.m, @tol * 0.6)
      ORDER BY 4 LIMIT 1)
) a ORDER BY uu_tien, kc LIMIT 1";
            var dt = Db.Query(sql, "@x", x, "@y", y, "@tol", dungSai, "@tram", coTram, "@ct", coCongTrinh, "@tuyen", coTuyen);
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        /// <summary>
        /// Tìm nhanh (ô tìm kiếm ở đầu trang): trạm, tuyến, phường/xã theo tên hoặc mã, không phân biệt dấu.
        /// Trả về loai (TRAM/TUYEN/PHUONG), ma, ten, phu (dòng mô tả phụ) – tối đa <paramref name="toiDa"/> dòng.
        /// </summary>
        public static DataTable TimNhanh(string tuKhoa, int toiDa = 12)
        {
            const string sql = @"
WITH k AS (SELECT xebus.unaccent_vn(@q) AS q, @q AS goc)
SELECT loai, ma, ten, phu FROM (
    (SELECT 1 AS nhom, 'TUYEN' AS loai, t.matuyen AS ma, t.tentuyen AS ten,
            'Tuyến · ' || t.matuyen || ' · ' || coalesce(t.trangthai, '') AS phu,
            (xebus.unaccent_vn(t.tentuyen) LIKE k.q || '%' OR t.matuyen ILIKE k.goc) AS dau
       FROM xebus.tuyenbuyt t, k
      WHERE xebus.unaccent_vn(t.tentuyen) LIKE '%' || k.q || '%' OR t.matuyen ILIKE k.goc || '%'
      LIMIT 5)
    UNION ALL
    (SELECT 2, 'PHUONG', x.maphuong, x.tenphuong, 'Phường/xã · ' || coalesce(to_char(x.danso, 'FM999G999G999'), '?') || ' dân',
            xebus.unaccent_vn(x.tenphuong) LIKE k.q || '%' OR xebus.unaccent_vn(x.tenphuong) LIKE '% ' || k.q || '%'
       FROM xebus.phuongxa x, k
      WHERE xebus.unaccent_vn(x.tenphuong) LIKE '%' || k.q || '%'
      LIMIT 5)
    UNION ALL
    (SELECT 3, 'TRAM', t.matram, t.tentram,
            'Trạm · ' || t.matram || coalesce(' · ' || p.tenphuong, ''),
            xebus.unaccent_vn(t.tentram) LIKE k.q || '%' OR t.matram ILIKE k.goc
       FROM xebus.trambuyt t LEFT JOIN xebus.phuongxa p ON p.maphuong = t.maphuong, k
      WHERE xebus.unaccent_vn(t.tentram) LIKE '%' || k.q || '%' OR t.matram ILIKE k.goc || '%'
      ORDER BY 6 DESC, t.tentram
      LIMIT @n)
) a ORDER BY dau DESC, nhom, ten LIMIT @n";
            return Db.Query(sql, "@q", tuKhoa.Trim(), "@n", toiDa);
        }

        /// <summary>Thuộc tính chi tiết (tên cột tiếng Việt) để hiển thị ở panel Thông tin.</summary>
        public static DataTable ThongTin(string loai, string ma)
        {
            switch (loai)
            {
                case "TRAM":
                    return Db.Query(@"SELECT t.matram AS ""Mã trạm"", t.tentram AS ""Tên trạm"", l.tenloaitram AS ""Loại trạm"",
                                             t.trangthai AS ""Trạng thái"", t.matuyen AS ""Tuyến chính"",
                                             (SELECT string_agg(tt.matuyen, ', ' ORDER BY tt.matuyen) FROM xebus.tuyen_tram tt WHERE tt.matram = t.matram) AS ""Các tuyến"",
                                             d.tenduong AS ""Đoạn đường"", p.tenphuong AS ""Phường/xã"",
                                             t.vido AS ""Vĩ độ"", t.kinhdo AS ""Kinh độ"",
                                             to_char(t.ngaytao, 'DD/MM/YYYY HH24:MI') AS ""Ngày tạo"",
                                             to_char(t.ngaycapnhat, 'DD/MM/YYYY HH24:MI') AS ""Ngày cập nhật"",
                                             t.tenntao AS ""Người tạo""
                                      FROM xebus.trambuyt t
                                      LEFT JOIN xebus.loaitram l ON l.maloaitram = t.maloaitram
                                      LEFT JOIN xebus.doanduong d ON d.madoan = t.madoan
                                      LEFT JOIN xebus.phuongxa p ON p.maphuong = t.maphuong
                                      WHERE t.matram = @ma", "@ma", ma);
                case "TUYEN":
                    return Db.Query(@"SELECT t.matuyen AS ""Mã tuyến"", t.tentuyen AS ""Tên tuyến"", t.mota AS ""Mô tả"",
                                             t.trangthai AS ""Trạng thái"",
                                             (SELECT count(*) FROM xebus.tuyen_tram tt WHERE tt.matuyen = t.matuyen) AS ""Số trạm"",
                                             round((ST_Length(t.geom::geography) / 1000)::numeric, 2) AS ""Chiều dài (km)"",
                                             to_char(t.ngaytao, 'DD/MM/YYYY') AS ""Ngày tạo"", t.tenntao AS ""Người tạo""
                                      FROM xebus.tuyenbuyt t WHERE t.matuyen = @ma", "@ma", ma);
                case "DOAN":
                    return Db.Query(@"SELECT madoan AS ""Mã đoạn"", tenduong AS ""Tên đường"", loaiduong AS ""Loại đường"",
                                             chieudaikm AS ""Chiều dài (km)"", tgdichuyen AS ""Thời gian (phút)"",
                                             matuyen AS ""Thuộc tuyến"", trangthai AS ""Trạng thái""
                                      FROM xebus.doanduong WHERE madoan = @ma", "@ma", ma);
                case "CONGTRINH":
                    return Db.Query(@"SELECT c.macongtrinh AS ""Mã công trình"", c.tencongtrinh AS ""Tên"", l.tenloaicongtrinh AS ""Loại"",
                                             p.tenphuong AS ""Phường/xã"",
                                             c.quymo AS ""Quy mô (ước tính)"", c.mucdouutien AS ""Mức ưu tiên"",
                                             (SELECT count(*) FROM xebus.ctpvcongtrinh x WHERE x.macongtrinh = c.macongtrinh) AS ""Số trạm trong 500 m"",
                                             (SELECT round(min(x.khoangcachm)) FROM xebus.ctpvcongtrinh x WHERE x.macongtrinh = c.macongtrinh) AS ""Trạm gần nhất (m)"",
                                             c.vido AS ""Vĩ độ"", c.kinhdo AS ""Kinh độ""
                                      FROM xebus.congtrinh c
                                      LEFT JOIN xebus.loaicongtrinh l USING (maloaicongtrinh)
                                      LEFT JOIN xebus.phuongxa p ON p.maphuong = c.maphuong
                                      WHERE c.macongtrinh = @ma", "@ma", ma);
                case "PHUONG":
                    return Db.Query(@"SELECT x.maphuong AS ""Mã phường/xã"", x.tenphuong AS ""Tên"",
                                             x.danso AS ""Dân số"", x.dientichkm2 AS ""Diện tích (km²)"",
                                             round(x.danso / NULLIF(x.dientichkm2, 0)) AS ""Mật độ (người/km²)"",
                                             (SELECT count(*) FROM xebus.trambuyt t WHERE t.maphuong = x.maphuong AND t.trangthai <> 'Ngừng hoạt động') AS ""Số trạm"",
                                             (SELECT count(*) FROM xebus.congtrinh c WHERE c.maphuong = x.maphuong) AS ""Số công trình"",
                                             x.ghichu AS ""Sáp nhập từ""
                                      FROM xebus.phuongxa x WHERE x.maphuong = @ma", "@ma", ma);
                case "KDC":
                    return Db.Query(@"SELECT k.makhu AS ""Mã khu"", k.tenkhu AS ""Tên khu"",
                                             (SELECT p.tenphuong FROM xebus.phuongxa p WHERE p.maphuong = k.maphuong) AS ""Phường/xã"",
                                             k.danso AS ""Dân số (ước tính)"",
                                             k.matdodancu AS ""Mật độ (người/km²)"",
                                             round((ST_Area(k.geom::geography) / 1e6)::numeric, 3) AS ""Diện tích (km²)"",
                                             (SELECT count(*) FROM xebus.ctpvkhudancu x WHERE x.makhu = k.makhu) AS ""Số trạm phục vụ""
                                      FROM xebus.khudancu k WHERE k.makhu = @ma", "@ma", ma);
                default:
                    return new DataTable();
            }
        }
    }
}
