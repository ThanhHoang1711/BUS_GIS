using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XeBus_GIS.Map
{
    /// <summary>
    /// Máy chủ ảnh nền (tile) cục bộ cho MapWinGIS.
    /// Lý do: MapWinGIS 4.9 chỉ tải tile qua http, trong khi các máy chủ OSM hiện chỉ phục vụ https; ngoài ra một số
    /// mạng không phân giải được openstreetmap.org. App mở cổng 127.0.0.1:&lt;ngẫu nhiên&gt;, MapWinGIS gọi
    /// http://127.0.0.1:port/{z}/{x}/{y}.png, máy chủ này tải tile từ nguồn https (User-Agent đúng chính sách OSM),
    /// làm nhạt màu (tuỳ chọn) để lớp GIS nổi rõ, lưu đệm xuống đĩa (%LOCALAPPDATA%\XeBus_GIS\tiles) để vùng đã xem
    /// vẫn hiện khi mất mạng.
    /// </summary>
    public sealed class MayChuNenCucBo : IDisposable
    {
        private const string UserAgent = "XeBus_GIS/1.0 (do an GIS tram xe buyt Ha Noi; WinForms MapWinGIS)";

        private readonly TcpListener _nghe;
        private readonly string[] _nguon;
        private readonly float _baoHoa;
        private readonly string _thuMucDem;
        private readonly CancellationTokenSource _huy = new CancellationTokenSource();
        private readonly SemaphoreSlim _gioiHan = new SemaphoreSlim(6); // không dồn quá nhiều request tới máy chủ nguồn

        public int Cong { get; }

        /// <summary>Mẫu URL cho MapWinGIS (cú pháp {zoom}/{x}/{y} của TileProviders.Add).</summary>
        public string UrlChoMapWinGis => $"http://127.0.0.1:{Cong}/{{zoom}}/{{x}}/{{y}}.png";

        /// <param name="nguon">các URL nguồn có {z} {x} {y} – thử lần lượt, nguồn sau là dự phòng khi nguồn trước lỗi</param>
        /// <param name="tenDem">tên thư mục đệm (theo nguồn + kiểu)</param>
        /// <param name="baoHoa">độ bão hoà màu giữ lại (1 = màu gốc; nhỏ hơn = nhạt hơn, kèm nâng sáng nhẹ)</param>
        public MayChuNenCucBo(string[] nguon, string tenDem, float baoHoa)
        {
            _nguon = nguon;
            _baoHoa = baoHoa;
            _thuMucDem = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XeBus_GIS", "tiles", tenDem);
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = Math.Max(ServicePointManager.DefaultConnectionLimit, 8);

            _nghe = new TcpListener(IPAddress.Loopback, 0);
            _nghe.Start();
            Cong = ((IPEndPoint)_nghe.LocalEndpoint).Port;
            Task.Run(VongNhanAsync);
        }

        private async Task VongNhanAsync()
        {
            while (!_huy.IsCancellationRequested)
            {
                TcpClient kh;
                try { kh = await _nghe.AcceptTcpClientAsync().ConfigureAwait(false); }
                catch { if (_huy.IsCancellationRequested) return; continue; }
                var _ = Task.Run(() => XuLyAsync(kh));
            }
        }

        private async Task XuLyAsync(TcpClient kh)
        {
            using (kh)
            {
                try
                {
                    kh.ReceiveTimeout = 10000;
                    var luong = kh.GetStream();
                    string dongDau = DocDongDau(luong);
                    // "GET /12/3255/1804.png HTTP/1.1"
                    var phan = (dongDau ?? "").Split(' ');
                    if (phan.Length < 2 || !TachToaDo(phan[1], out int z, out int x, out int y))
                    {
                        GhiPhanHoi(luong, 404, null);
                        return;
                    }
                    var anh = await LayTileAsync(z, x, y).ConfigureAwait(false);
                    GhiPhanHoi(luong, anh == null ? 404 : 200, anh);
                }
                catch { /* client đóng sớm / lỗi mạng – bỏ qua */ }
            }
        }

        /// <summary>Đọc dòng đầu của request rồi bỏ phần header còn lại (tới dòng trống).</summary>
        private static string DocDongDau(Stream s)
        {
            var sb = new StringBuilder();
            string dau = null;
            int b;
            while ((b = s.ReadByte()) >= 0)
            {
                if (b == 13) continue;      // CR
                if (b == 10)                // LF
                {
                    var dong = sb.ToString();
                    sb.Clear();
                    if (dau == null) dau = dong;
                    else if (dong.Length == 0) break; // hết header
                    continue;
                }
                if (sb.Length < 4096) sb.Append((char)b);
            }
            return dau;
        }

        private static bool TachToaDo(string duongDan, out int z, out int x, out int y)
        {
            z = x = y = 0;
            var p = duongDan.Trim('/').Split('/');
            if (p.Length != 3) return false;
            var cuoi = p[2];
            int cham = cuoi.IndexOf('.');
            if (cham > 0) cuoi = cuoi.Substring(0, cham);
            return int.TryParse(p[0], out z) && int.TryParse(p[1], out x) && int.TryParse(cuoi, out y)
                && z >= 0 && z <= 19 && x >= 0 && y >= 0 && x < (1 << z) && y < (1 << z);
        }

        private static void GhiPhanHoi(Stream s, int ma, byte[] noiDung)
        {
            var dau = Encoding.ASCII.GetBytes(
                $"HTTP/1.1 {ma} {(ma == 200 ? "OK" : "Not Found")}\r\n" +
                (ma == 200 ? "Content-Type: image/png\r\nCache-Control: max-age=604800\r\n" : "") +
                $"Content-Length: {noiDung?.Length ?? 0}\r\nConnection: close\r\n\r\n");
            s.Write(dau, 0, dau.Length);
            if (noiDung != null) s.Write(noiDung, 0, noiDung.Length);
            s.Flush();
        }

        private async Task<byte[]> LayTileAsync(int z, int x, int y)
        {
            string tep = Path.Combine(_thuMucDem, z.ToString(), x.ToString(), y + ".png");
            if (File.Exists(tep))
            {
                try { return File.ReadAllBytes(tep); } catch { }
            }

            await _gioiHan.WaitAsync(_huy.Token).ConfigureAwait(false);
            try
            {
                foreach (var mau in _nguon)
                {
                    var goc = await TaiAsync(mau.Replace("{z}", z.ToString()).Replace("{x}", x.ToString()).Replace("{y}", y.ToString())).ConfigureAwait(false);
                    if (goc == null) continue; // nguồn này lỗi – thử nguồn dự phòng

                    var kq = _baoHoa < 0.999f ? LamNhat(goc, _baoHoa) : goc;
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(tep));
                        File.WriteAllBytes(tep, kq);
                    }
                    catch { /* không ghi được đệm thì vẫn trả ảnh */ }
                    return kq;
                }
                return null; // mất mạng / mọi nguồn lỗi – MapWinGIS để trống ô đó
            }
            finally
            {
                _gioiHan.Release();
            }
        }

        private static async Task<byte[]> TaiAsync(string url)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.UserAgent = UserAgent;
                req.Timeout = 12000;
                req.ReadWriteTimeout = 12000;
                req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                using (var res = (HttpWebResponse)await req.GetResponseAsync().ConfigureAwait(false))
                using (var rs = res.GetResponseStream())
                using (var ms = new MemoryStream())
                {
                    if (res.StatusCode != HttpStatusCode.OK || rs == null || !(res.ContentType ?? "").StartsWith("image/")) return null;
                    await rs.CopyToAsync(ms).ConfigureAwait(false);
                    return ms.ToArray();
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Giảm bão hoà và kéo sáng nhẹ – nền nhạt để ký hiệu GIS nổi rõ.</summary>
        private static byte[] LamNhat(byte[] png, float s)
        {
            using (var vao = new MemoryStream(png))
            using (var anh = Image.FromStream(vao))
            using (var bmp = new Bitmap(anh.Width, anh.Height, PixelFormat.Format32bppArgb))
            {
                const float lr = 0.3086f, lg = 0.6094f, lb = 0.0820f;
                float a = (1 - s) * lr, b = (1 - s) * lg, c = (1 - s) * lb;
                float k = 0.88f + 0.12f * s, sang = 0.12f * (1 - s); // nén tương phản + nâng sáng theo mức làm nhạt
                var cm = new ColorMatrix(new[]
                {
                    new[] { (a + s) * k, a * k,       a * k,       0f, 0f },
                    new[] { b * k,       (b + s) * k, b * k,       0f, 0f },
                    new[] { c * k,       c * k,       (c + s) * k, 0f, 0f },
                    new[] { 0f,          0f,          0f,          1f, 0f },
                    new[] { sang,        sang,        sang,        0f, 1f },
                });
                using (var g = Graphics.FromImage(bmp))
                using (var ia = new ImageAttributes())
                {
                    ia.SetColorMatrix(cm);
                    g.DrawImage(anh, new Rectangle(0, 0, anh.Width, anh.Height), 0, 0, anh.Width, anh.Height, GraphicsUnit.Pixel, ia);
                }
                using (var ra = new MemoryStream())
                {
                    bmp.Save(ra, ImageFormat.Png);
                    return ra.ToArray();
                }
            }
        }

        public void Dispose()
        {
            _huy.Cancel();
            try { _nghe.Stop(); } catch { }
        }
    }
}
