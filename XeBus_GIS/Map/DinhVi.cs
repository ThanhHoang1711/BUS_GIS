using System;
using System.Data;
using System.Device.Location;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using XeBus_GIS.Data;
using XeBus_GIS.UI;

namespace XeBus_GIS.Map
{
    /// <summary>Vị trí người dùng (chỉ giữ trong bộ nhớ; toạ độ GPS/Windows không được lưu xuống đĩa).</summary>
    public class ViTriNguoiDung
    {
        public double ViDo, KinhDo;
        public double? DoChinhXacM;
        /// <summary>"Dịch vụ định vị Windows" | "Phường/xã đã chọn" | "Toạ độ nhập tay".</summary>
        public string Nguon;
        public string TenPhuong;
        public string MaPhuong;

        public string ToaDo => string.Format(CultureInfo.InvariantCulture, "{0:0.00000}, {1:0.00000}", ViDo, KinhDo);
    }

    /// <summary>
    /// Định vị cho phần mềm desktop – theo thứ tự người dùng chọn, KHÔNG tự động:
    /// 1. Dịch vụ định vị của Windows (System.Device.Location – GPS/Wi-Fi/… tuỳ máy; cần bật Location trong Settings,
    ///    lấy MỘT lần, không theo dõi liên tục);
    /// 2. Chọn phường/xã (lấy điểm đại diện từ CSDL cục bộ – không gọi dịch vụ ngoài);
    /// 3. Nhập toạ độ.
    /// Không dùng định vị theo IP. Tên phường/xã chứa điểm tra trong xebus.phuongxa (cục bộ).
    /// </summary>
    public static class DinhVi
    {
        /// <summary>Phạm vi hợp lệ (bao quanh Hà Nội, nới rộng) – toạ độ ngoài vùng này vẫn dùng được nhưng không có tên phường.</summary>
        public static bool TrongVietNam(double viDo, double kinhDo) => viDo >= 8 && viDo <= 24 && kinhDo >= 102 && kinhDo <= 110;

        /// <summary>
        /// Lấy vị trí hiện tại từ dịch vụ định vị Windows (một lần, có timeout). Ném InvalidOperationException với lời
        /// giải thích dễ hiểu khi bị từ chối / tắt dịch vụ / không có tín hiệu.
        /// </summary>
        public static Task<ViTriNguoiDung> LayTuWindowsAsync(int timeoutMs = 12000) => Task.Run(() =>
        {
            using (var w = new GeoCoordinateWatcher(GeoPositionAccuracy.Default))
            {
                var xong = new ManualResetEventSlim(false);
                GeoCoordinate kq = null;
                w.PositionChanged += (s, e) =>
                {
                    if (e.Position.Location != null && !e.Position.Location.IsUnknown) { kq = e.Position.Location; xong.Set(); }
                };
                w.StatusChanged += (s, e) => { if (e.Status == GeoPositionStatus.Disabled) xong.Set(); };
                if (!w.TryStart(false, TimeSpan.FromMilliseconds(timeoutMs)))
                    throw new InvalidOperationException("Dịch vụ định vị Windows không phản hồi.");
                if (w.Permission == GeoPositionPermission.Denied)
                    throw new InvalidOperationException("Windows chưa cho phép ứng dụng dùng vị trí. Bật trong Settings › Privacy & security › Location " +
                                                        "(mục \"Let desktop apps access your location\"), hoặc chọn phường/xã thủ công.");
                if (w.Status == GeoPositionStatus.Disabled)
                    throw new InvalidOperationException("Dịch vụ định vị (Location) của Windows đang tắt. Bật trong Settings › Privacy & security › Location, " +
                                                        "hoặc chọn phường/xã thủ công.");
                if (!w.Position.Location.IsUnknown) kq = w.Position.Location;
                else xong.Wait(timeoutMs);
                w.Stop();
                if (kq == null || kq.IsUnknown)
                    throw new InvalidOperationException("Không xác định được vị trí (máy không có GPS/Wi-Fi định vị hoặc chưa có tín hiệu). Hãy chọn phường/xã thủ công.");
                var v = new ViTriNguoiDung
                {
                    ViDo = kq.Latitude, KinhDo = kq.Longitude,
                    DoChinhXacM = double.IsNaN(kq.HorizontalAccuracy) ? (double?)null : kq.HorizontalAccuracy,
                    Nguon = "Dịch vụ định vị Windows",
                };
                GanPhuong(v);
                return v;
            }
        });

        /// <summary>Điểm đại diện của phường/xã (ST_PointOnSurface) – từ CSDL cục bộ.</summary>
        public static ViTriNguoiDung TuPhuong(string maPhuong)
        {
            var dt = Db.Query(@"SELECT maphuong, tenphuong, ST_Y(ST_PointOnSurface(geom)) AS vido, ST_X(ST_PointOnSurface(geom)) AS kinhdo
                                FROM xebus.phuongxa WHERE maphuong = @m", "@m", maPhuong);
            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new ViTriNguoiDung
            {
                ViDo = Convert.ToDouble(r["vido"]), KinhDo = Convert.ToDouble(r["kinhdo"]),
                Nguon = "Phường/xã đã chọn", MaPhuong = Convert.ToString(r["maphuong"]), TenPhuong = Convert.ToString(r["tenphuong"]),
            };
        }

        public static ViTriNguoiDung TuToaDo(double viDo, double kinhDo)
        {
            var v = new ViTriNguoiDung { ViDo = viDo, KinhDo = kinhDo, Nguon = "Toạ độ nhập tay" };
            GanPhuong(v);
            return v;
        }

        /// <summary>Tên phường/xã chứa điểm (null nếu ngoài Hà Nội hoặc lỗi CSDL).</summary>
        private static void GanPhuong(ViTriNguoiDung v)
        {
            try
            {
                var dt = Db.Query(@"SELECT maphuong, tenphuong FROM xebus.phuongxa
                                    WHERE ST_Intersects(geom, ST_SetSRID(ST_MakePoint(@x, @y), 4326)) LIMIT 1",
                                  "@x", v.KinhDo, "@y", v.ViDo);
                if (dt.Rows.Count > 0)
                {
                    v.MaPhuong = Convert.ToString(dt.Rows[0]["maphuong"]);
                    v.TenPhuong = Convert.ToString(dt.Rows[0]["tenphuong"]);
                }
            }
            catch { /* chỉ là nhãn hiển thị */ }
        }

        /// <summary>Chuỗi lưu lựa chọn thủ công (không lưu vị trí lấy từ Windows).</summary>
        public static string ChuoiLuu(ViTriNguoiDung v)
        {
            if (v == null) return null;
            if (v.Nguon == "Phường/xã đã chọn") return "phuong:" + v.MaPhuong;
            if (v.Nguon == "Toạ độ nhập tay") return string.Format(CultureInfo.InvariantCulture, "toado:{0},{1}", v.ViDo, v.KinhDo);
            return null;
        }

        /// <summary>Khôi phục lựa chọn đã lưu (gọi trong luồng nền – có truy vấn CSDL).</summary>
        public static ViTriNguoiDung KhoiPhuc(string chuoi)
        {
            if (string.IsNullOrEmpty(chuoi)) return null;
            try
            {
                if (chuoi.StartsWith("phuong:")) return TuPhuong(chuoi.Substring(7));
                if (chuoi.StartsWith("toado:"))
                {
                    var p = chuoi.Substring(6).Split(',');
                    return TuToaDo(double.Parse(p[0], CultureInfo.InvariantCulture), double.Parse(p[1], CultureInfo.InvariantCulture));
                }
            }
            catch { }
            return null;
        }
    }
}
