using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using XeBus_GIS.Security;

namespace XeBus_GIS.Data
{
    /// <summary>Đăng nhập / đăng xuất, ghi lịch sử đăng nhập (B13, B14) và nạp quyền (B11, B11.1, B12).</summary>
    public static class NguoiDungRepository
    {
        public const string KetQuaThanhCong = "Thành công";
        public const string KetQuaSaiMatKhau = "Sai mật khẩu";
        public const string KetQuaKhongTonTai = "Không tồn tại";
        public const string KetQuaBiKhoa = "Tài khoản bị khóa";

        /// <summary>Trạng thái tài khoản tự đăng ký, chờ quản trị viên duyệt (đổi sang "Hoạt động" trong màn hình Quản trị).</summary>
        public const string TrangThaiChoDuyet = "Chờ duyệt";

        /// <summary>
        /// Đăng ký tài khoản: tạo người dùng nhóm Khách hàng ở trạng thái "Chờ duyệt" – chưa đăng nhập được cho tới khi
        /// quản trị viên duyệt (phân quyền vẫn do QTV quyết định, đúng thiết kế mục 3.1). Trả về null nếu thành công,
        /// ngược lại là thông báo lỗi.
        /// </summary>
        public static string DangKy(string tenDangNhap, string hoTen, string donVi, string matKhau)
        {
            tenDangNhap = (tenDangNhap ?? "").Trim();
            hoTen = (hoTen ?? "").Trim();
            if (hoTen.Length == 0) return "Vui lòng nhập họ tên.";
            if (!System.Text.RegularExpressions.Regex.IsMatch(tenDangNhap, @"^[A-Za-z0-9_.]{3,50}$"))
                return "Tên đăng nhập 3–50 ký tự, chỉ gồm chữ không dấu, số, dấu _ hoặc dấu chấm.";
            if ((matKhau ?? "").Length < 6) return "Mật khẩu tối thiểu 6 ký tự.";
            if (QuanTriRepository.TenDangNhapDaCo(tenDangNhap)) return "Tên đăng nhập đã có người dùng.";
            try
            {
                Db.Execute(@"INSERT INTO xebus.nguoidung (tendnhap, tenndung, donvi, idnhom, trangthai, matkhau)
                             VALUES (@u, @ten, NULLIF(@dv, ''),
                                     (SELECT idnhom FROM xebus.n_ngdung WHERE tennhom = 'Khách hàng'), @tt, @mk)",
                    "@u", tenDangNhap, "@ten", hoTen, "@dv", (donVi ?? "").Trim(), "@tt", TrangThaiChoDuyet,
                    "@mk", PasswordHasher.Hash(matKhau));
                return null;
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
            {
                return "Tên đăng nhập đã có người dùng.";
            }
        }

        /// <summary>Đăng nhập bằng tài khoản. Trả về null nếu thành công, ngược lại là thông báo lỗi.</summary>
        public static string DangNhap(string tenDangNhap, string matKhau)
        {
            tenDangNhap = (tenDangNhap ?? "").Trim();
            var dt = Db.Query(@"SELECT n.idndung, n.tenndung, n.matkhau, n.idnhom, n.trangthai, g.tennhom
                                FROM xebus.nguoidung n JOIN xebus.n_ngdung g ON g.idnhom = n.idnhom
                                WHERE lower(n.tendnhap) = lower(@u)", "@u", tenDangNhap);

            if (dt.Rows.Count == 0)
            {
                GhiLichSu(null, tenDangNhap, KetQuaKhongTonTai);
                return "Tên đăng nhập hoặc mật khẩu không đúng.";
            }

            var r = dt.Rows[0];
            int id = Convert.ToInt32(r["idndung"]);
            if (!PasswordHasher.Verify(matKhau, Db.Str(r, "matkhau")))
            {
                GhiLichSu(id, tenDangNhap, KetQuaSaiMatKhau);
                return "Tên đăng nhập hoặc mật khẩu không đúng.";
            }
            if (Db.Str(r, "trangthai") == TrangThaiChoDuyet)
            {
                GhiLichSu(id, tenDangNhap, KetQuaBiKhoa);
                return "Tài khoản đang chờ quản trị viên duyệt.";
            }
            if (Db.Str(r, "trangthai") != "Hoạt động")
            {
                GhiLichSu(id, tenDangNhap, KetQuaBiKhoa);
                return "Tài khoản đang bị khóa. Liên hệ quản trị viên.";
            }

            int idNhom = Convert.ToInt32(r["idnhom"]);
            Session.BatDau(id, tenDangNhap, Db.Str(r, "tenndung"), idNhom, Db.Str(r, "tennhom"), NapQuyen(idNhom));
            Session.IdLichSuDangNhap = GhiLichSu(id, tenDangNhap, KetQuaThanhCong);
            return null;
        }

        /// <summary>Vào hệ thống với tư cách khách (nhóm Khách hàng, không cần tài khoản).</summary>
        public static void DangNhapKhach()
        {
            var tenNhom = Convert.ToString(Db.Scalar("SELECT tennhom FROM xebus.n_ngdung WHERE idnhom = @id", "@id", Quyen.NhomKhachHang));
            Session.BatDau(null, "khach_vang_lai", "Khách", Quyen.NhomKhachHang, tenNhom, NapQuyen(Quyen.NhomKhachHang));
            Session.IdLichSuDangNhap = GhiLichSu(null, "(khách vãng lai)", KetQuaThanhCong);
        }

        public static void DangXuat()
        {
            try
            {
                if (Session.IdLichSuDangNhap.HasValue)
                    Db.Execute("UPDATE xebus.ls_dnhap SET tgdangxuat = now() WHERE idlsdn = @id", "@id", Session.IdLichSuDangNhap.Value);
            }
            catch { /* không chặn việc đăng xuất khi mất kết nối */ }
            Session.KetThuc();
        }

        public static List<KeyValuePair<string, string>> NapQuyen(int idNhom)
        {
            var dt = Db.Query(@"SELECT q.machucnang, q.loaithaotac
                                FROM xebus.quyen_nnd qn JOIN xebus.quyen q ON q.idquyen = qn.idquyen
                                WHERE qn.idnhom = @n", "@n", idNhom);
            return dt.Rows.Cast<DataRow>()
                     .Select(r => new KeyValuePair<string, string>(Db.Str(r, "machucnang"), Db.Str(r, "loaithaotac")))
                     .ToList();
        }

        private static long? GhiLichSu(int? idNguoiDung, string tenDangNhap, string ketQua)
        {
            try
            {
                var id = Db.Scalar(@"INSERT INTO xebus.ls_dnhap (idndung, tendnhap, diachiip, tenthietbi, ketqua)
                                     VALUES (@id, @u, @ip, @may, @kq) RETURNING idlsdn",
                                   "@id", (object)idNguoiDung, "@u", tenDangNhap, "@ip", DiaChiIp(),
                                   "@may", Environment.MachineName, "@kq", ketQua);
                return id == null ? (long?)null : Convert.ToInt64(id);
            }
            catch
            {
                return null;
            }
        }

        private static string DiaChiIp()
        {
            try
            {
                var ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                            .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                return ip?.ToString() ?? "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }
    }
}
