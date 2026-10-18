using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XeBus_GIS.UI
{
    /// <summary>
    /// Lựa chọn giao diện của người dùng trên máy này (chế độ sáng/tối, bản đồ nền, vị trí tự chọn…):
    /// tệp key=value <c>%LOCALAPPDATA%\XeBus_GIS\caidat.txt</c>. Không lưu dữ liệu nghiệp vụ, mật khẩu hay toạ độ GPS.
    /// Lỗi đọc/ghi bị bỏ qua – app vẫn chạy với giá trị mặc định.
    /// </summary>
    public static class CaiDatNguoiDung
    {
        public const string KhoaTheme = "theme";          // "toi" | "sang"
        public const string KhoaNen = "nen";              // khoá MapManager.CacNen
        public const string KhoaViTri = "vitri";          // "phuong:<mã>" | "toado:<vĩ độ>,<kinh độ>" (do người dùng chọn/nhập)
        public const string KhoaViTriThoiTiet = "vitri_thoitiet"; // "1" = cho dùng vị trí đã chọn để lấy thời tiết
        public const string KhoaRongPanelPhai = "rong_panel_phai";
        public const string KhoaTenDangNhap = "ten_dn_nho";  // tên đăng nhập khi người dùng chọn "Ghi nhớ" (KHÔNG lưu mật khẩu)

        public static string ThuMuc => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XeBus_GIS");
        private static string Tep => Path.Combine(ThuMuc, "caidat.txt");

        private static Dictionary<string, string> _gt;

        private static Dictionary<string, string> GiaTri
        {
            get
            {
                if (_gt != null) return _gt;
                _gt = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    if (File.Exists(Tep))
                        foreach (var dong in File.ReadAllLines(Tep))
                        {
                            int i = dong.IndexOf('=');
                            if (i > 0) _gt[dong.Substring(0, i).Trim()] = dong.Substring(i + 1).Trim();
                        }
                    // tương thích bản trước: lựa chọn nền lưu riêng ở nen.txt
                    var nenCu = Path.Combine(ThuMuc, "nen.txt");
                    if (!_gt.ContainsKey(KhoaNen) && File.Exists(nenCu)) _gt[KhoaNen] = File.ReadAllText(nenCu).Trim();
                }
                catch { /* dùng mặc định */ }
                return _gt;
            }
        }

        public static string Doc(string khoa, string macDinh = null) =>
            GiaTri.TryGetValue(khoa, out var v) && v.Length > 0 ? v : macDinh;

        public static void Ghi(string khoa, string giaTri)
        {
            if (giaTri == null) GiaTri.Remove(khoa);
            else GiaTri[khoa] = giaTri.Replace("\r", "").Replace("\n", "");
            try
            {
                Directory.CreateDirectory(ThuMuc);
                File.WriteAllLines(Tep, GiaTri.Select(kv => kv.Key + "=" + kv.Value));
            }
            catch { /* chỉ là ghi nhớ lựa chọn */ }
        }
    }
}
