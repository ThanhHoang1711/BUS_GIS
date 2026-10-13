using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace XeBus_GIS.Data
{
    /// <summary>
    /// Quy trình 1 – Quản lý trạm xe buýt: tra cứu thuộc tính / không gian, đọc và cập nhật thuộc tính trạm (B06).
    /// Toạ độ không gian từ bản đồ truyền vào theo EPSG:3857; CSDL lưu EPSG:4326.
    /// </summary>
    public static class TramRepository
    {
        public static readonly string[] TrangThaiTram = { "Hoạt động", "Đề xuất", "Ngừng hoạt động" };

        /// <summary>Các cột kết quả tra cứu (giống nhau cho mọi kiểu tra cứu).</summary>
        private const string CotKetQua = @"
            t.matram   AS ""Mã"",
            t.tentram  AS ""Tên trạm"",
            l.tenloaitram AS ""Loại"",
            t.trangthai AS ""Trạng thái"",
            p.tenphuong AS ""Phường/xã"",
            (SELECT string_agg(tt.matuyen, ', ' ORDER BY tt.matuyen) FROM xebus.tuyen_tram tt WHERE tt.matram = t.matram) AS ""Tuyến""";

        private const string NguonKetQua = @"
            FROM xebus.trambuyt t
            LEFT JOIN xebus.loaitram l ON l.maloaitram = t.maloaitram
            LEFT JOIN xebus.phuongxa p ON p.maphuong = t.maphuong";

        // ------------------------------------------------------------------
        // Danh mục cho combobox
        // ------------------------------------------------------------------
        public static DataTable LoaiTram() =>
            Db.Query("SELECT maloaitram AS ma, tenloaitram AS ten FROM xebus.loaitram ORDER BY tenloaitram");

        public static DataTable Tuyen() =>
            Db.Query("SELECT matuyen AS ma, matuyen || ' - ' || tentuyen AS ten FROM xebus.tuyenbuyt ORDER BY matuyen");

        public static DataTable PhuongXa() =>
            Db.Query("SELECT maphuong AS ma, tenphuong AS ten FROM xebus.phuongxa ORDER BY tenphuong");

        public static DataTable PhuongTien() =>
            Db.Query("SELECT maphuongtien AS ma, maphuongtien || ' - ' || bienso || ' (' || loaiphuongtien || ')' AS ten FROM xebus.phuongtien ORDER BY maphuongtien");

        // ------------------------------------------------------------------
        // 1.2 Tra cứu theo thuộc tính
        // ------------------------------------------------------------------
        /// <summary>Tham số null/rỗng = bỏ qua điều kiện đó.</summary>
        public static DataTable TraCuuThuocTinh(string ten, string maLoai, string trangThai, string maTuyen, string maPhuong)
        {
            var sql = new StringBuilder("SELECT ").Append(CotKetQua).Append(NguonKetQua).Append(" WHERE true");
            var thamSo = new List<object>();
            if (!string.IsNullOrWhiteSpace(ten))
            {
                sql.Append(" AND (xebus.unaccent_vn(t.tentram) LIKE '%' || xebus.unaccent_vn(@ten) || '%' OR t.matram ILIKE @ten)");
                thamSo.AddRange(new object[] { "@ten", ten.Trim() });
            }
            if (!string.IsNullOrEmpty(maLoai))
            {
                sql.Append(" AND t.maloaitram = @loai");
                thamSo.AddRange(new object[] { "@loai", maLoai });
            }
            if (!string.IsNullOrEmpty(trangThai))
            {
                sql.Append(" AND t.trangthai = @tt");
                thamSo.AddRange(new object[] { "@tt", trangThai });
            }
            if (!string.IsNullOrEmpty(maTuyen))
            {
                sql.Append(" AND (t.matuyen = @tuyen OR EXISTS (SELECT 1 FROM xebus.tuyen_tram x WHERE x.matram = t.matram AND x.matuyen = @tuyen))");
                thamSo.AddRange(new object[] { "@tuyen", maTuyen });
            }
            if (!string.IsNullOrEmpty(maPhuong))
            {
                sql.Append(" AND t.maphuong = @phuong");
                thamSo.AddRange(new object[] { "@phuong", maPhuong });
            }
            sql.Append(" ORDER BY t.tentram");
            return Db.Query(sql.ToString(), thamSo.ToArray());
        }

        // ------------------------------------------------------------------
        // 1.3 Tra cứu theo không gian
        // ------------------------------------------------------------------
        /// <summary>Trạm nằm trong đa giác vẽ trên bản đồ (WKT EPSG:3857).</summary>
        public static DataTable TraCuuTrongVung(string wkt3857) =>
            Db.Query("SELECT " + CotKetQua + NguonKetQua +
                     " WHERE ST_Intersects(t.geom, ST_Transform(ST_GeomFromText(@wkt, 3857), 4326)) ORDER BY t.tentram",
                     "@wkt", wkt3857);

        /// <summary>Trạm trong bán kính r (m) quanh điểm (x, y EPSG:3857), kèm khoảng cách, sắp gần -> xa.</summary>
        public static DataTable TraCuuBanKinh(double x, double y, double banKinhM) =>
            Db.Query("SELECT " + CotKetQua + @",
                         round(ST_Distance(t.geom::geography, d.g::geography))::int AS ""Khoảng cách (m)""" +
                     NguonKetQua + @"
                     CROSS JOIN (SELECT ST_Transform(ST_SetSRID(ST_MakePoint(@x, @y), 3857), 4326) AS g) d
                     WHERE ST_DWithin(t.geom::geography, d.g::geography, @r)
                     ORDER BY 7",
                     "@x", x, "@y", y, "@r", banKinhM);

        /// <summary>Phường/xã chứa điểm click (x, y EPSG:3857) – trả (maphuong, tenphuong) hoặc null.</summary>
        public static DataRow PhuongTaiDiem(double x, double y)
        {
            var dt = Db.Query(@"SELECT maphuong, tenphuong FROM xebus.phuongxa
                                WHERE ST_Intersects(geom, ST_Transform(ST_SetSRID(ST_MakePoint(@x, @y), 3857), 4326)) LIMIT 1",
                              "@x", x, "@y", y);
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        /// <summary>Hình vòng tròn bán kính r quanh điểm – WKT EPSG:3857 để vẽ lớp tạm.</summary>
        public static string WktVongTron(double x, double y, double banKinhM) =>
            Convert.ToString(Db.Scalar(@"SELECT ST_AsText(ST_Transform(
                                            ST_Buffer(ST_Transform(ST_SetSRID(ST_MakePoint(@x, @y), 3857), 4326)::geography, @r, 'quad_segs=16')::geometry,
                                            3857))", "@x", x, "@y", y, "@r", banKinhM));

        /// <summary>Ranh giới phường – WKT EPSG:3857.</summary>
        public static string WktPhuong(string maPhuong) =>
            Convert.ToString(Db.Scalar("SELECT ST_AsText(ST_Transform(geom, 3857)) FROM xebus.phuongxa WHERE maphuong = @m",
                                       "@m", maPhuong));

        // ------------------------------------------------------------------
        // 1.4 Cập nhật thuộc tính trạm
        // ------------------------------------------------------------------
        public static DataRow Lay(string maTram)
        {
            var dt = Db.Query(@"SELECT matram, tentram, maloaitram, matuyen, maphuongtien, trangthai, vido, kinhdo, madoan, maphuong
                                FROM xebus.trambuyt WHERE matram = @m", "@m", maTram);
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        /// <summary>Kiểm tra toạ độ (WGS84) nằm trong ranh giới Hà Nội.</summary>
        public static bool TrongHaNoi(double kinhDo, double viDo) =>
            Convert.ToBoolean(Db.Scalar(
                "SELECT EXISTS (SELECT 1 FROM public.hanoi WHERE ST_Intersects(geom, ST_SetSRID(ST_MakePoint(@x, @y), 4326)))",
                "@x", kinhDo, "@y", viDo));

        /// <summary>
        /// Cập nhật thuộc tính trạm. Nếu vị trí đổi: tự gán lại đoạn đường gần nhất (≤100 m), phường/xã,
        /// và tính lại chi tiết phục vụ (B06.1/B06.2). ngaycapnhat do trigger ghi.
        /// </summary>
        public static void CapNhat(string maTram, string ten, string maLoai, string maTuyen, string maPhuongTien,
                                   string trangThai, double kinhDo, double viDo)
        {
            Db.Transaction((conn, tx) =>
            {
                Db.Execute(conn, tx, @"
                    UPDATE xebus.trambuyt t SET
                        tentram = @ten, maloaitram = @loai, matuyen = NULLIF(@tuyen, ''),
                        maphuongtien = NULLIF(@pt, ''), trangthai = @tt,
                        vido = round(@y::numeric, 7), kinhdo = round(@x::numeric, 7),
                        geom = ST_SetSRID(ST_MakePoint(@x, @y), 4326),
                        maphuong = (SELECT p.maphuong FROM xebus.phuongxa p
                                    WHERE ST_Intersects(p.geom, ST_SetSRID(ST_MakePoint(@x, @y), 4326)) LIMIT 1),
                        madoan = (SELECT d.madoan FROM xebus.doanduong d
                                  WHERE ST_DWithin(d.geom::geography, ST_SetSRID(ST_MakePoint(@x, @y), 4326)::geography, 100)
                                  ORDER BY d.geom <-> ST_SetSRID(ST_MakePoint(@x, @y), 4326) LIMIT 1)
                    WHERE t.matram = @ma",
                    "@ten", ten.Trim(), "@loai", maLoai, "@tuyen", maTuyen ?? "", "@pt", maPhuongTien ?? "",
                    "@tt", trangThai, "@x", kinhDo, "@y", viDo, "@ma", maTram);
                Db.Execute(conn, tx, "SELECT xebus.fn_tinh_ctpv(500, @ma)", "@ma", maTram);
            });
        }
    }
}
