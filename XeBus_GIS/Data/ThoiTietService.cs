using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using XeBus_GIS.UI;

namespace XeBus_GIS.Data
{
    /// <summary>Thời tiết hiện tại (một lần đọc từ Open-Meteo).</summary>
    public class DuLieuThoiTiet
    {
        public double NhietDo;           // °C
        public int DoAm;                 // %
        public int MaWmo;                // mã thời tiết WMO
        public bool BanNgay;
        public double Gio;               // km/h
        public DateTime ThoiDiem;        // giờ quan trắc (giờ Hà Nội)
        public DateTime LayLuc;          // lúc ứng dụng lấy về
        public string ViTri;             // tên vị trí hiển thị
        /// <summary>Dữ liệu đọc từ đệm (lần lấy gần nhất), chưa làm mới được.</summary>
        public bool Cu;
    }

    /// <summary>
    /// Thời tiết hiện tại cho header – nguồn Open-Meteo (https://open-meteo.com, miễn phí cho mục đích phi thương mại,
    /// không cần API key; ghi nguồn ở tooltip). Gọi trong luồng nền, timeout 8 s; lưu kết quả gần nhất vào
    /// %LOCALAPPDATA%\XeBus_GIS\thoitiet.txt để mở app lần sau hiện ngay. Không tạo số liệu giả: lỗi ⇒ null / dữ liệu cũ.
    /// Mặc định dùng toạ độ trung tâm Hà Nội (cố định, không phải vị trí cá nhân); chỉ dùng vị trí người dùng khi họ cho
    /// phép, toạ độ được làm tròn 0,01° (~1 km) trước khi gửi.
    /// </summary>
    public static class ThoiTietService
    {
        public const double ViDoHaNoi = 21.0285, KinhDoHaNoi = 105.8542;
        public const string NguonDuLieu = "Open-Meteo.com";

        private const string Url =
            "https://api.open-meteo.com/v1/forecast?latitude={0}&longitude={1}" +
            "&current=temperature_2m,relative_humidity_2m,weather_code,is_day,wind_speed_10m&timezone=Asia%2FBangkok";

        private static string TepDem => Path.Combine(CaiDatNguoiDung.ThuMuc, "thoitiet.txt");

        /// <summary>Lấy thời tiết hiện tại (đồng bộ – gọi trong Task.Run). Trả null nếu lỗi mạng / dữ liệu không hợp lệ.</summary>
        public static DuLieuThoiTiet Lay(double viDo, double kinhDo, string tenViTri)
        {
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                var url = string.Format(CultureInfo.InvariantCulture, Url, Math.Round(viDo, 2), Math.Round(kinhDo, 2));
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = 8000;
                req.ReadWriteTimeout = 8000;
                req.UserAgent = "XeBus_GIS/1.0 (do an GIS tram xe buyt Ha Noi)";
                req.AutomaticDecompression = DecompressionMethods.GZip;   // không nhận deflate: .NET Framework giải nén lỗi với máy chủ này
                string json;
                using (var res = (HttpWebResponse)req.GetResponse())
                using (var sr = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                    json = sr.ReadToEnd();
                var d = Doc(json);
                if (d == null) return null;
                d.ViTri = tenViTri;
                d.LayLuc = DateTime.Now;
                LuuDem(d);
                return d;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Đọc khối "current" của JSON Open-Meteo (không cần thư viện JSON – chỉ vài số).</summary>
        private static DuLieuThoiTiet Doc(string json)
        {
            int i = json.IndexOf("\"current\":{", StringComparison.Ordinal);
            if (i < 0) return null;
            int j = json.IndexOf('}', i);
            string cur = json.Substring(i, j - i);
            double So(string ten)
            {
                var m = Regex.Match(cur, "\"" + ten + "\":(-?[0-9.]+)");
                if (!m.Success) throw new FormatException(ten);
                return double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            }
            try
            {
                var t = Regex.Match(cur, "\"time\":\"([^\"]+)\"");
                return new DuLieuThoiTiet
                {
                    NhietDo = So("temperature_2m"),
                    DoAm = (int)So("relative_humidity_2m"),
                    MaWmo = (int)So("weather_code"),
                    BanNgay = So("is_day") > 0,
                    Gio = So("wind_speed_10m"),
                    ThoiDiem = t.Success && DateTime.TryParse(t.Groups[1].Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var tg) ? tg : DateTime.Now,
                };
            }
            catch { return null; }
        }

        private static void LuuDem(DuLieuThoiTiet d)
        {
            try
            {
                Directory.CreateDirectory(CaiDatNguoiDung.ThuMuc);
                File.WriteAllText(TepDem, string.Join("\n",
                    d.NhietDo.ToString(CultureInfo.InvariantCulture), d.DoAm, d.MaWmo, d.BanNgay ? 1 : 0,
                    d.Gio.ToString(CultureInfo.InvariantCulture), d.ThoiDiem.ToString("o"), d.LayLuc.ToString("o"), d.ViTri ?? ""));
            }
            catch { /* chỉ là đệm */ }
        }

        /// <summary>Kết quả lần lấy gần nhất (đánh dấu Cu = true) – null nếu chưa từng lấy được.</summary>
        public static DuLieuThoiTiet DocDem()
        {
            try
            {
                if (!File.Exists(TepDem)) return null;
                var p = File.ReadAllText(TepDem).Split('\n');
                return new DuLieuThoiTiet
                {
                    NhietDo = double.Parse(p[0], CultureInfo.InvariantCulture),
                    DoAm = int.Parse(p[1]),
                    MaWmo = int.Parse(p[2]),
                    BanNgay = p[3] == "1",
                    Gio = double.Parse(p[4], CultureInfo.InvariantCulture),
                    ThoiDiem = DateTime.Parse(p[5], null, DateTimeStyles.RoundtripKind),
                    LayLuc = DateTime.Parse(p[6], null, DateTimeStyles.RoundtripKind),
                    ViTri = p.Length > 7 ? p[7] : "",
                    Cu = true,
                };
            }
            catch { return null; }
        }

        /// <summary>Kiểu biểu tượng thời tiết (control tự vẽ).</summary>
        public enum KieuIcon { Nang, Trang, ItMay, May, SuongMu, MuaPhun, Mua, MuaRao, Dong, Tuyet }

        /// <summary>Mô tả tiếng Việt + kiểu biểu tượng theo mã WMO (https://open-meteo.com/en/docs – WMO Weather interpretation codes).</summary>
        public static (string moTa, KieuIcon icon) MoTa(int ma, bool banNgay)
        {
            switch (ma)
            {
                case 0: return ("Trời quang", banNgay ? KieuIcon.Nang : KieuIcon.Trang);
                case 1: return ("Ít mây", KieuIcon.ItMay);
                case 2: return ("Có mây", KieuIcon.ItMay);
                case 3: return ("Nhiều mây", KieuIcon.May);
                case 45: case 48: return ("Sương mù", KieuIcon.SuongMu);
                case 51: case 53: case 55: case 56: case 57: return ("Mưa phùn", KieuIcon.MuaPhun);
                case 61: return ("Mưa nhỏ", KieuIcon.Mua);
                case 63: return ("Mưa vừa", KieuIcon.Mua);
                case 65: case 66: case 67: return ("Mưa to", KieuIcon.Mua);
                case 71: case 73: case 75: case 77: case 85: case 86: return ("Tuyết", KieuIcon.Tuyet);
                case 80: case 81: return ("Mưa rào", KieuIcon.MuaRao);
                case 82: return ("Mưa rào mạnh", KieuIcon.MuaRao);
                case 95: return ("Dông", KieuIcon.Dong);
                case 96: case 99: return ("Dông kèm mưa đá", KieuIcon.Dong);
                default: return ("Không rõ", KieuIcon.May);
            }
        }
    }
}
