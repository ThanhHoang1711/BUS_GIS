using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml.Linq;

namespace XeBus_GIS.Data
{
    /// <summary>
    /// Cấu hình ứng dụng = <c>App.config</c> (nằm trong repo, KHÔNG chứa mật khẩu) + <c>App.local.config</c> riêng từng máy
    /// (đã có trong .gitignore – không bao giờ bị commit) ghi đè lên. Mỗi người chép <c>App.local.config.example</c> thành
    /// <c>App.local.config</c> rồi điền mật khẩu PostgreSQL của máy mình.
    /// File riêng được tìm cạnh exe trước, rồi lần lên các thư mục cha (bin\Debug → thư mục project) để chạy từ Visual Studio
    /// không cần chép file vào bin.
    /// </summary>
    public static class CauHinh
    {
        public const string TenFileRieng = "App.local.config";
        /// <summary>Mật khẩu giữ chỗ trong App.config / file mẫu – còn giá trị này nghĩa là chưa cấu hình.</summary>
        public const string MatKhauGiuCho = "MAT_KHAU_POSTGRES";

        private static readonly object _khoa = new object();
        private static bool _daNap;
        private static string _duongDan;
        private static readonly Dictionary<string, string> _chuoiKetNoi = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> _caiDat = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Đường dẫn App.local.config đang dùng (null nếu không có).</summary>
        public static string DuongDanFileRieng { get { Nap(); return _duongDan; } }

        /// <summary>Chuỗi kết nối theo tên: App.local.config trước, không có thì App.config.</summary>
        public static string ChuoiKetNoi(string ten)
        {
            Nap();
            if (_chuoiKetNoi.TryGetValue(ten, out var cs) && !string.IsNullOrWhiteSpace(cs)) return cs;
            return ConfigurationManager.ConnectionStrings[ten]?.ConnectionString;
        }

        /// <summary>Giá trị appSettings: App.local.config trước, không có thì App.config.</summary>
        public static string Doc(string khoa)
        {
            Nap();
            if (_caiDat.TryGetValue(khoa, out var v)) return v;
            return ConfigurationManager.AppSettings[khoa];
        }

        private static void Nap()
        {
            if (_daNap) return;
            lock (_khoa)
            {
                if (_daNap) return;
                _duongDan = TimFileRieng();
                if (_duongDan != null)
                {
                    XDocument x;
                    try { x = XDocument.Load(_duongDan); }
                    catch (Exception ex)
                    {
                        throw new ConfigurationErrorsException($"File {TenFileRieng} không đúng định dạng XML ({_duongDan}): {ex.Message}", ex);
                    }
                    var goc = x.Root;
                    if (goc != null)
                    {
                        foreach (var e in goc.Elements("connectionStrings").Elements("add"))
                        {
                            var ten = (string)e.Attribute("name");
                            if (!string.IsNullOrEmpty(ten)) _chuoiKetNoi[ten] = (string)e.Attribute("connectionString");
                        }
                        foreach (var e in goc.Elements("appSettings").Elements("add"))
                        {
                            var k = (string)e.Attribute("key");
                            if (!string.IsNullOrEmpty(k)) _caiDat[k] = (string)e.Attribute("value");
                        }
                    }
                }
                _daNap = true;
            }
        }

        private static string TimFileRieng()
        {
            try
            {
                var thuMuc = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                for (int i = 0; i < 4 && thuMuc != null; i++, thuMuc = thuMuc.Parent)
                {
                    var p = Path.Combine(thuMuc.FullName, TenFileRieng);
                    if (File.Exists(p)) return p;
                }
            }
            catch { /* không đọc được thư mục – coi như không có file riêng */ }
            return null;
        }
    }
}
