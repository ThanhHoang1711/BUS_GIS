using System;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using Npgsql;

namespace XeBus_GIS.Data
{
    /// <summary>
    /// Truy cập PostgreSQL/PostGIS (DB CSDLKG). Mọi câu lệnh dùng tham số @ten – không nối chuỗi dữ liệu người dùng.
    /// Tham số truyền theo cặp: Db.Query(sql, "@ma", ma, "@ten", ten).
    /// </summary>
    public static class Db
    {
        private static string _connStr;

        public static string ConnectionString
        {
            get
            {
                if (_connStr == null)
                {
                    // App.local.config (riêng máy, không commit) ghi đè App.config – xem Data/CauHinh.cs
                    var cs = CauHinh.ChuoiKetNoi("CSDLKG");
                    if (string.IsNullOrWhiteSpace(cs))
                        throw new ConfigurationErrorsException("Thiếu chuỗi kết nối 'CSDLKG' (App.config / App.local.config).");
                    string mk;
                    try { mk = new NpgsqlConnectionStringBuilder(cs).Password; }
                    catch (Exception ex) { throw new ConfigurationErrorsException("Chuỗi kết nối 'CSDLKG' sai định dạng: " + ex.Message, ex); }
                    if (string.IsNullOrEmpty(mk) || mk.Trim('<', '>') == CauHinh.MatKhauGiuCho)
                        throw new ConfigurationErrorsException(CauHinh.DuongDanFileRieng == null
                            ? "Chưa đặt mật khẩu CSDL: chép App.local.config.example thành App.local.config, điền mật khẩu."
                            : "Chưa điền mật khẩu CSDL trong App.local.config (" + CauHinh.DuongDanFileRieng + ").");
                    _connStr = cs;
                }
                return _connStr;
            }
        }

        public static NpgsqlConnection Open()
        {
            var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        private static NpgsqlCommand TaoLenh(NpgsqlConnection conn, string sql, object[] thamSo, NpgsqlTransaction tx = null)
        {
            var cmd = new NpgsqlCommand(sql, conn, tx);
            if (thamSo != null)
            {
                if (thamSo.Length % 2 != 0) throw new ArgumentException("Tham số phải theo cặp tên/giá trị");
                for (int i = 0; i < thamSo.Length; i += 2)
                    cmd.Parameters.AddWithValue((string)thamSo[i], thamSo[i + 1] ?? DBNull.Value);
            }
            return cmd;
        }

        public static DataTable Query(string sql, params object[] thamSo)
        {
            using (var conn = Open())
            using (var cmd = TaoLenh(conn, sql, thamSo))
            using (var rd = cmd.ExecuteReader())
            {
                var dt = new DataTable();
                dt.Load(rd);
                return dt;
            }
        }

        public static int Execute(string sql, params object[] thamSo)
        {
            using (var conn = Open())
            using (var cmd = TaoLenh(conn, sql, thamSo))
                return cmd.ExecuteNonQuery();
        }

        public static object Scalar(string sql, params object[] thamSo)
        {
            using (var conn = Open())
            using (var cmd = TaoLenh(conn, sql, thamSo))
            {
                var v = cmd.ExecuteScalar();
                return v == DBNull.Value ? null : v;
            }
        }

        /// <summary>Chạy nhiều lệnh trong 1 transaction; lỗi => rollback toàn bộ.</summary>
        public static void Transaction(Action<NpgsqlConnection, NpgsqlTransaction> congViec)
        {
            using (var conn = Open())
            using (var tx = conn.BeginTransaction())
            {
                congViec(conn, tx);
                tx.Commit();
            }
        }

        public static int Execute(NpgsqlConnection conn, NpgsqlTransaction tx, string sql, params object[] thamSo)
        {
            using (var cmd = TaoLenh(conn, sql, thamSo, tx))
                return cmd.ExecuteNonQuery();
        }

        public static Task<DataTable> QueryAsync(string sql, params object[] thamSo)
            => Task.Run(() => Query(sql, thamSo));

        public static Task<object> ScalarAsync(string sql, params object[] thamSo)
            => Task.Run(() => Scalar(sql, thamSo));

        public static Task<int> ExecuteAsync(string sql, params object[] thamSo)
            => Task.Run(() => Execute(sql, thamSo));

        /// <summary>Kiểm tra kết nối + schema xebus đã được tạo.</summary>
        public static bool KiemTraKetNoi(out string loi)
        {
            try
            {
                var n = Convert.ToInt32(Scalar("SELECT count(*) FROM pg_namespace WHERE nspname = 'xebus'"));
                loi = n == 1 ? null : "Chưa có schema xebus – hãy chạy các script trong thư mục database.";
                return n == 1;
            }
            catch (Exception ex)
            {
                loi = ex.Message;
                return false;
            }
        }

        // --- tiện ích đọc giá trị từ DataRow ---
        public static string Str(DataRow r, string cot) => r[cot] == DBNull.Value ? null : Convert.ToString(r[cot]);
        public static int? Int(DataRow r, string cot) => r[cot] == DBNull.Value ? (int?)null : Convert.ToInt32(r[cot]);
        public static double? Dbl(DataRow r, string cot) => r[cot] == DBNull.Value ? (double?)null : Convert.ToDouble(r[cot]);
    }
}
