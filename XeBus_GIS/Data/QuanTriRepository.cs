using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using Npgsql;
using XeBus_GIS.Security;

namespace XeBus_GIS.Data
{
    /// <summary>
    /// Chức năng Quản trị viên (mục 3.1.1): quản lý người dùng nghiệp vụ, phân quyền, lịch sử truy cập, sao lưu.
    /// Bảng: n_ngdung (B11), quyen_nnd (B11.1), quyen (B12), nguoidung (B13), ls_dnhap (B14), saoluu (B15).
    /// </summary>
    public static class QuanTriRepository
    {
        public static readonly string[] TrangThaiNguoiDung = { "Hoạt động", "Khóa", NguoiDungRepository.TrangThaiChoDuyet };

        /// <summary>Số tài khoản tự đăng ký đang chờ duyệt.</summary>
        public static int SoChoDuyet() =>
            Convert.ToInt32(Db.Scalar("SELECT count(*) FROM xebus.nguoidung WHERE trangthai = @tt", "@tt", NguoiDungRepository.TrangThaiChoDuyet));

        // ------------------------------------------------------------------
        // Người dùng
        // ------------------------------------------------------------------
        public static DataTable DanhSachNguoiDung(string tuKhoa) =>
            Db.Query(@"SELECT n.idndung AS id, n.tendnhap AS ""Tên đăng nhập"", n.tenndung AS ""Họ tên"",
                              g.tennhom AS ""Nhóm"", n.chucvu AS ""Chức vụ"", n.donvi AS ""Đơn vị"", n.trangthai AS ""Trạng thái"",
                              (SELECT to_char(max(l.tgdangnhap), 'DD/MM/YYYY HH24:MI') FROM xebus.ls_dnhap l
                                WHERE l.idndung = n.idndung AND l.ketqua = 'Thành công') AS ""Đăng nhập gần nhất"",
                              n.idnhom
                       FROM xebus.nguoidung n JOIN xebus.n_ngdung g ON g.idnhom = n.idnhom
                       WHERE @tk = '' OR xebus.unaccent_vn(n.tendnhap || ' ' || n.tenndung || ' ' || coalesce(n.donvi, ''))
                                         LIKE '%' || xebus.unaccent_vn(@tk) || '%'
                       ORDER BY n.idnhom DESC, n.tendnhap", "@tk", (tuKhoa ?? "").Trim());

        public static DataRow LayNguoiDung(int id)
        {
            var dt = Db.Query("SELECT idndung, tendnhap, tenndung, chucvu, donvi, idnhom, trangthai FROM xebus.nguoidung WHERE idndung = @id", "@id", id);
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        public static DataTable Nhom() =>
            Db.Query(@"SELECT g.idnhom AS ma, g.tennhom AS ten, g.mota,
                              (SELECT count(*) FROM xebus.nguoidung n WHERE n.idnhom = g.idnhom)::int AS so_nguoi,
                              (SELECT count(*) FROM xebus.quyen_nnd q WHERE q.idnhom = g.idnhom)::int AS so_quyen
                       FROM xebus.n_ngdung g ORDER BY g.idnhom");

        public static bool TenDangNhapDaCo(string tenDN, int? truId = null) =>
            Convert.ToBoolean(Db.Scalar(
                "SELECT EXISTS (SELECT 1 FROM xebus.nguoidung WHERE lower(tendnhap) = lower(@u) AND idndung <> coalesce(@id, -1))",
                "@u", tenDN.Trim(), "@id", (object)truId));

        /// <summary>01 – Thêm mới người dùng nghiệp vụ. Trả về id.</summary>
        public static int Them(string tenDN, string hoTen, string chucVu, string donVi, int idNhom, string trangThai, string matKhau) =>
            Convert.ToInt32(Db.Scalar(@"INSERT INTO xebus.nguoidung (tendnhap, tenndung, chucvu, donvi, idnhom, trangthai, matkhau)
                                        VALUES (@u, @ten, NULLIF(@cv, ''), NULLIF(@dv, ''), @nhom, @tt, @mk) RETURNING idndung",
                "@u", tenDN.Trim(), "@ten", hoTen.Trim(), "@cv", (chucVu ?? "").Trim(), "@dv", (donVi ?? "").Trim(),
                "@nhom", idNhom, "@tt", trangThai, "@mk", PasswordHasher.Hash(matKhau)));

        /// <summary>02 – Chỉnh sửa thông tin người dùng (không đổi mật khẩu).</summary>
        public static void Sua(int id, string tenDN, string hoTen, string chucVu, string donVi, int idNhom, string trangThai)
        {
            KiemTraConQuanTri(id, idNhom, trangThai);
            Db.Execute(@"UPDATE xebus.nguoidung SET tendnhap = @u, tenndung = @ten, chucvu = NULLIF(@cv, ''), donvi = NULLIF(@dv, ''),
                                                    idnhom = @nhom, trangthai = @tt
                         WHERE idndung = @id",
                "@u", tenDN.Trim(), "@ten", hoTen.Trim(), "@cv", (chucVu ?? "").Trim(), "@dv", (donVi ?? "").Trim(),
                "@nhom", idNhom, "@tt", trangThai, "@id", id);
        }

        public static void DatLaiMatKhau(int id, string matKhauMoi) =>
            Db.Execute("UPDATE xebus.nguoidung SET matkhau = @mk WHERE idndung = @id", "@mk", PasswordHasher.Hash(matKhauMoi), "@id", id);

        /// <summary>03 – Xoá người dùng. Lịch sử đăng nhập / sao lưu giữ lại (idndung => NULL).</summary>
        public static void Xoa(int id)
        {
            if (Session.IdNguoiDung == id) throw new InvalidOperationException("Không thể xoá tài khoản đang đăng nhập.");
            KiemTraConQuanTri(id, null, null);
            Db.Execute("DELETE FROM xebus.nguoidung WHERE idndung = @id", "@id", id);
        }

        /// <summary>Luôn phải còn ít nhất một Quản trị viên đang hoạt động.</summary>
        private static void KiemTraConQuanTri(int id, int? nhomMoi, string trangThaiMoi)
        {
            bool vanLaQtvHoatDong = nhomMoi == Quyen.NhomQuanTri && trangThaiMoi == "Hoạt động";
            if (vanLaQtvHoatDong) return;
            var conLai = Convert.ToInt32(Db.Scalar(@"SELECT count(*) FROM xebus.nguoidung
                                                     WHERE idnhom = @qtv AND trangthai = 'Hoạt động' AND idndung <> @id",
                                                   "@qtv", Quyen.NhomQuanTri, "@id", id));
            var laQtv = Convert.ToBoolean(Db.Scalar("SELECT EXISTS (SELECT 1 FROM xebus.nguoidung WHERE idndung = @id AND idnhom = @qtv AND trangthai = 'Hoạt động')",
                                                    "@id", id, "@qtv", Quyen.NhomQuanTri));
            if (laQtv && conLai == 0)
                throw new InvalidOperationException("Đây là Quản trị viên hoạt động cuối cùng – không thể xoá, khoá hoặc chuyển nhóm.");
        }

        // ------------------------------------------------------------------
        // 04 – Phân quyền
        // ------------------------------------------------------------------
        public static DataTable QuyenCuaNhom(int idNhom) =>
            Db.Query(@"SELECT q.idquyen, EXISTS (SELECT 1 FROM xebus.quyen_nnd x WHERE x.idnhom = @n AND x.idquyen = q.idquyen) AS ""Cho phép"",
                              CASE split_part(q.machucnang, '_', 1)
                                   WHEN 'TRAM' THEN 'Quản lý trạm' WHEN 'TUYEN' THEN 'Quản lý tuyến'
                                   WHEN 'DX' THEN 'Đề xuất trạm mới' WHEN 'TK' THEN 'Thống kê' WHEN 'QT' THEN 'Quản trị' END AS ""Nhóm chức năng"",
                              q.tenquyen AS ""Chức năng"",
                              CASE q.loaithaotac WHEN 'XEM' THEN 'Xem' WHEN 'THEM' THEN 'Thêm' WHEN 'SUA' THEN 'Sửa' WHEN 'XOA' THEN 'Xoá' END AS ""Thao tác"",
                              q.machucnang AS ""Mã""
                       FROM xebus.quyen q
                       ORDER BY CASE split_part(q.machucnang, '_', 1) WHEN 'TRAM' THEN 1 WHEN 'TUYEN' THEN 2 WHEN 'DX' THEN 3 WHEN 'TK' THEN 4 ELSE 5 END,
                                q.idquyen", "@n", idNhom);

        public static void LuuQuyenNhom(int idNhom, IList<int> danhSachIdQuyen)
        {
            Db.Transaction((conn, tx) =>
            {
                Db.Execute(conn, tx, "DELETE FROM xebus.quyen_nnd WHERE idnhom = @n", "@n", idNhom);
                foreach (var q in danhSachIdQuyen)
                    Db.Execute(conn, tx, "INSERT INTO xebus.quyen_nnd (idnhom, idquyen) VALUES (@n, @q)", "@n", idNhom, "@q", q);
                if (idNhom == Quyen.NhomQuanTri)
                {
                    using (var cmd = new NpgsqlCommand(@"SELECT EXISTS (SELECT 1 FROM xebus.quyen_nnd x JOIN xebus.quyen q USING (idquyen)
                                                         WHERE x.idnhom = @n AND q.machucnang = 'QT_PHANQUYEN')", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@n", idNhom);
                        if (!(bool)cmd.ExecuteScalar())
                            throw new InvalidOperationException("Nhóm Quản trị viên phải giữ quyền \"Phân quyền người dùng\" (tránh tự khoá hệ thống).");
                    }
                }
            });
        }

        // ------------------------------------------------------------------
        // 05 – Lịch sử truy cập
        // ------------------------------------------------------------------
        public static DataTable LichSu(DateTime tuNgay, DateTime denNgay, string tenDN, string ketQua) =>
            Db.Query(@"SELECT to_char(l.tgdangnhap, 'DD/MM/YYYY HH24:MI:SS') AS ""Đăng nhập"",
                              to_char(l.tgdangxuat, 'DD/MM/YYYY HH24:MI:SS') AS ""Đăng xuất"",
                              CASE WHEN l.tgdangxuat IS NOT NULL
                                   THEN to_char(l.tgdangxuat - l.tgdangnhap, 'HH24:MI:SS') END AS ""Thời lượng"",
                              coalesce(n.tendnhap, l.tendnhap) AS ""Tên đăng nhập"", n.tenndung AS ""Họ tên"",
                              g.tennhom AS ""Nhóm"", l.ketqua AS ""Kết quả"", l.diachiip AS ""Địa chỉ IP"", l.tenthietbi AS ""Thiết bị""
                       FROM xebus.ls_dnhap l
                       LEFT JOIN xebus.nguoidung n ON n.idndung = l.idndung
                       LEFT JOIN xebus.n_ngdung g ON g.idnhom = n.idnhom
                       WHERE l.tgdangnhap >= @tu AND l.tgdangnhap < @den + interval '1 day'
                         AND (@u = '' OR coalesce(n.tendnhap, l.tendnhap, '') ILIKE '%' || @u || '%')
                         AND (@kq = '' OR l.ketqua = @kq)
                       ORDER BY l.tgdangnhap DESC
                       LIMIT 2000",
                "@tu", tuNgay.Date, "@den", denNgay.Date, "@u", (tenDN ?? "").Trim(), "@kq", ketQua ?? "");

        // ------------------------------------------------------------------
        // 06 – Sao lưu dữ liệu hệ thống (pg_dump schema xebus)
        // ------------------------------------------------------------------
        public static DataTable DanhSachSaoLuu() =>
            Db.Query(@"SELECT to_char(s.thoigian, 'DD/MM/YYYY HH24:MI:SS') AS ""Thời gian"", s.loaisaoluu AS ""Loại"",
                              n.tendnhap AS ""Người thực hiện"", round(s.kichthuoc / 1024.0, 1) AS ""Kích thước (KB)"",
                              s.filepath AS ""Đường dẫn""
                       FROM xebus.saoluu s LEFT JOIN xebus.nguoidung n ON n.idndung = s.idndung
                       ORDER BY s.thoigian DESC");

        public static string ThuMucSaoLuu => ConfigurationManager.AppSettings["BackupFolder"] ?? @"D:\XeBus_GIS\database\backup";
        public static string ThuMucPgBin => ConfigurationManager.AppSettings["PgBinPath"] ?? @"C:\Program Files\PostgreSQL\17\bin";

        /// <summary>
        /// Sao lưu schema xebus (dữ liệu nghiệp vụ + bảo mật của hệ thống) bằng pg_dump định dạng custom.
        /// Ghi nhận vào bảng saoluu. Trả về đường dẫn file.
        /// </summary>
        public static string SaoLuu(int? idNguoiDung, string loai = "Thủ công")
        {
            var pgDump = Path.Combine(ThuMucPgBin, "pg_dump.exe");
            if (!File.Exists(pgDump)) throw new FileNotFoundException("Không tìm thấy pg_dump.exe – kiểm tra PgBinPath trong App.config.", pgDump);
            Directory.CreateDirectory(ThuMucSaoLuu);
            var file = Path.Combine(ThuMucSaoLuu, $"xebus_{DateTime.Now:yyyyMMdd_HHmmss}.backup");

            var cs = new NpgsqlConnectionStringBuilder(Db.ConnectionString);
            var psi = new ProcessStartInfo(pgDump,
                $"-h \"{cs.Host}\" -p {cs.Port} -U \"{cs.Username}\" -d \"{cs.Database}\" -n xebus -Fc -f \"{file}\"")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            psi.EnvironmentVariables["PGPASSWORD"] = cs.Password ?? "";
            using (var p = Process.Start(psi))
            {
                var loi = p.StandardError.ReadToEnd();
                p.WaitForExit(10 * 60 * 1000);
                if (p.ExitCode != 0 || !File.Exists(file))
                    throw new InvalidOperationException("pg_dump lỗi: " + loi.Trim());
            }
            var kichThuoc = new FileInfo(file).Length;
            Db.Execute(@"INSERT INTO xebus.saoluu (idndung, filepath, kichthuoc, loaisaoluu) VALUES (@id, @f, @k, @l)",
                       "@id", (object)idNguoiDung, "@f", file, "@k", kichThuoc, "@l", loai);
            return file;
        }
    }
}
