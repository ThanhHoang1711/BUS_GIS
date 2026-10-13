using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using AxMapWinGIS;
using MapWinGIS;
using XeBus_GIS.Data;
using XeBus_GIS.UI;

namespace XeBus_GIS.Map
{
    /// <summary>Thông tin một lớp bản đồ (shapefile trong bộ nhớ, nạp từ PostGIS).</summary>
    public class LopBanDo
    {
        public string Khoa;
        public string Ten;
        public ShpfileType Loai;
        public string Sql;
        public int Handle = -1;
        public Shapefile Sf;
        public bool LaLopTam;
        /// <summary>mã đối tượng -> chỉ số shape</summary>
        public readonly Dictionary<string, int> ChiSo = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        public Action<Shapefile> KieuDang;
    }

    /// <summary>
    /// Quản lý bản đồ MapWinGIS: hệ Web Mercator + nền OSM, các lớp nghiệp vụ nạp từ PostGIS dưới dạng
    /// shapefile trong bộ nhớ (WKT EPSG:3857), chọn/highlight/zoom theo mã đối tượng.
    /// Mọi thao tác COM phải chạy trên luồng giao diện (STA).
    /// </summary>
    public class MapManager
    {
        public const string RanhGioi = "RANHGIOI";
        public const string PhuongXa = "PHUONG";
        public const string KhuDanCu = "KDC";
        public const string CongTrinh = "CONGTRINH";
        public const string DoanDuong = "DUONG";
        public const string Tuyen = "TUYEN";
        public const string Tram = "TRAM";

        public AxMap Map { get; }
        private readonly Dictionary<string, LopBanDo> _lop = new Dictionary<string, LopBanDo>();

        /// <summary>Danh sách lớp nghiệp vụ theo thứ tự vẽ (dưới -> trên).</summary>
        public IEnumerable<LopBanDo> CacLop => _lop.Values.Where(l => !l.LaLopTam);

        public MapManager(AxMap map)
        {
            Map = map;
        }

        // =====================================================================
        // Khởi tạo
        // =====================================================================
        public void KhoiTao()
        {
            Map.Projection = tkMapProjection.PROJECTION_GOOGLE_MERCATOR;
            var nen = ConfigurationManager.AppSettings["NenBanDo"] ?? "OpenStreetMap";
            Map.TileProvider = nen.Equals("None", StringComparison.OrdinalIgnoreCase)
                ? tkTileProvider.ProviderNone : tkTileProvider.OpenStreetMap;
            Map.ShowVersionNumber = false;
            Map.ShowRedrawTime = false;
            Map.ScalebarVisible = true;
            Map.ShowCoordinates = tkCoordinatesDisplay.cdmNone;
            Map.ShowZoomBar = true;
            Map.SendMouseDown = true;
            Map.SendMouseMove = true;
            Map.CursorMode = tkCursorMode.cmPan;
            Map.BackColor = Theme.Background;
            ToanCanh();

            DangKy(RanhGioi, "Ranh giới Hà Nội", ShpfileType.SHP_POLYGON, BanDoRepository.SqlRanhGioi, sf =>
            {
                var o = sf.DefaultDrawingOptions;
                o.FillVisible = false;
                o.LineColor = Theme.Ole(Theme.MapRanhGioi);
                o.LineWidth = 2;
                o.LineStipple = tkDashStyle.dsDash;
            });
            DangKy(PhuongXa, "Ranh giới phường/xã", ShpfileType.SHP_POLYGON, BanDoRepository.SqlPhuongXa, sf =>
            {
                var o = sf.DefaultDrawingOptions;
                o.FillVisible = false;
                o.LineColor = Theme.Ole(Theme.MapPhuongXa);
                o.LineWidth = 1;
                o.LineStipple = tkDashStyle.dsDot;
            });
            DangKy(KhuDanCu, "Khu dân cư", ShpfileType.SHP_POLYGON, BanDoRepository.SqlKhuDanCu, sf =>
            {
                var o = sf.DefaultDrawingOptions;
                o.FillColor = Theme.Ole(Theme.MapKhuDanCu);
                o.FillTransparency = 110;
                o.LineColor = Theme.Ole(Color.FromArgb(0xD9, 0x9A, 0x06));
                o.LineWidth = 1;
            });
            DangKy(CongTrinh, "Công trình", ShpfileType.SHP_POINT, BanDoRepository.SqlCongTrinh, sf =>
            {
                KieuDiem(sf.DefaultDrawingOptions, Theme.MapCongTrinh, 6, tkDefaultPointSymbol.dpsDiamond);
                ThemLoaiDiem(sf, "Y tế", "YT", Theme.MapCtYTe, 8, tkDefaultPointSymbol.dpsCross);
                ThemLoaiDiem(sf, "Giáo dục", "GD", Theme.MapCtGiaoDuc, 7, tkDefaultPointSymbol.dpsTriangleUp);
                ThemLoaiDiem(sf, "Thương mại - chợ", "TM", Theme.MapCtThuongMai, 6, tkDefaultPointSymbol.dpsDiamond);
                ThemLoaiDiem(sf, "Khu công nghiệp", "CN", Theme.MapCtCongNghiep, 9, tkDefaultPointSymbol.dpsSquare);
            });
            DangKy(DoanDuong, "Đoạn đường", ShpfileType.SHP_POLYLINE, BanDoRepository.SqlDoanDuong, sf =>
            {
                var o = sf.DefaultDrawingOptions;
                o.LineColor = Theme.Ole(Theme.MapDuong);
                o.LineWidth = 1;
            });
            DangKy(Tuyen, "Tuyến xe buýt", ShpfileType.SHP_POLYLINE, BanDoRepository.SqlTuyen, sf =>
            {
                sf.DefaultDrawingOptions.LineWidth = 3;
                sf.DefaultDrawingOptions.LineColor = Theme.Ole(Theme.MauTuyen[0]);
                for (int i = 0; i < Theme.MauTuyen.Length; i++)
                {
                    var cat = sf.Categories.Add("Màu " + i);
                    cat.Expression = "[kieu] = \"" + i + "\"";
                    cat.DrawingOptions.LineColor = Theme.Ole(Theme.MauTuyen[i]);
                    cat.DrawingOptions.LineWidth = 3;
                }
            });
            DangKy(Tram, "Trạm xe buýt", ShpfileType.SHP_POINT, BanDoRepository.SqlTram, sf =>
            {
                KieuDiem(sf.DefaultDrawingOptions, Theme.Accent, 8, tkDefaultPointSymbol.dpsCircle);
                ThemLoaiDiem(sf, "Bến xe", "BX", Theme.MapBenXe, 11, tkDefaultPointSymbol.dpsSquare);
                ThemLoaiDiem(sf, "Đề xuất", "DX", Theme.MapTramDeXuat, 10, tkDefaultPointSymbol.dpsCircle);
                ThemLoaiDiem(sf, "Ngừng hoạt động", "NG", Theme.MapTramNgung, 7, tkDefaultPointSymbol.dpsCircle);
            });
        }

        private static void KieuDiem(ShapeDrawingOptions o, Color mau, float coDiem, tkDefaultPointSymbol kieu)
        {
            o.SetDefaultPointSymbol(kieu);
            o.PointSize = coDiem;
            o.FillColor = Theme.Ole(mau);
            o.LineColor = Theme.Ole(Color.White);
            o.LineWidth = 1.5f;
            o.LineVisible = true;
        }

        private static void ThemLoaiDiem(Shapefile sf, string ten, string kieu, Color mau, float coDiem, tkDefaultPointSymbol hinh)
        {
            var cat = sf.Categories.Add(ten);
            cat.Expression = "[kieu] = \"" + kieu + "\"";
            KieuDiem(cat.DrawingOptions, mau, coDiem, hinh);
        }

        private void DangKy(string khoa, string ten, ShpfileType loai, string sql, Action<Shapefile> kieuDang)
        {
            _lop[khoa] = new LopBanDo { Khoa = khoa, Ten = ten, Loai = loai, Sql = sql, KieuDang = kieuDang };
        }

        // =====================================================================
        // Nạp dữ liệu
        // =====================================================================
        /// <summary>Nạp tất cả lớp nghiệp vụ (đọc DB ở luồng nền, dựng shapefile ở luồng giao diện).</summary>
        public async Task NapTatCaAsync(IEnumerable<string> lopAn = null)
        {
            var an = new HashSet<string>(lopAn ?? Enumerable.Empty<string>());
            var lopNghiepVu = CacLop.ToList();
            var duLieu = await Task.Run(() => lopNghiepVu.ToDictionary(l => l.Khoa, l => BanDoRepository.Lay(l.Sql)));
            foreach (var l in lopNghiepVu)
            {
                DungLop(l, duLieu[l.Khoa]);
                Map.set_LayerVisible(l.Handle, !an.Contains(l.Khoa));
            }
            Map.Redraw();
        }

        /// <summary>Nạp lại một lớp sau khi dữ liệu thay đổi (giữ nguyên thứ tự lớp).</summary>
        public async Task NapLaiAsync(string khoa)
        {
            var l = _lop[khoa];
            var dt = await Task.Run(() => BanDoRepository.Lay(l.Sql));
            DungLop(l, dt);
            Map.Redraw();
        }

        /// <summary>Dựng (hoặc làm mới) shapefile của lớp từ DataTable có cột wkt (EPSG:3857) và ma.</summary>
        private void DungLop(LopBanDo l, DataTable dt)
        {
            bool moi = l.Sf == null;
            if (moi)
            {
                l.Sf = new Shapefile();
                if (!l.Sf.CreateNewWithShapeID("", l.Loai))
                    throw new InvalidOperationException("Không tạo được lớp " + l.Ten + ": " + l.Sf.get_ErrorMsg(l.Sf.LastErrorCode));
                l.Sf.GeoProjection.ImportFromEPSG(3857);
                // mặc định MapWinGIS bỏ qua ký hiệu điểm trùng vị trí với điểm đã vẽ ở lớp khác (vd trạm đánh số
                // trên tuyến, ứng viên trùng trạm) – lớp nghiệp vụ và lớp tạm đều phải được vẽ đầy đủ
                l.Sf.CollisionMode = tkCollisionMode.AllowCollisions;
                foreach (DataColumn c in dt.Columns)
                {
                    if (c.ColumnName == "wkt") continue;
                    l.Sf.EditAddField(c.ColumnName, FieldType.STRING_FIELD, 0, 254);
                }
                l.Sf.SelectionAppearance = tkSelectionAppearance.saDrawingOptions;
                var sel = l.Sf.SelectionDrawingOptions;
                sel.FillColor = Theme.Ole(Theme.MapChon);
                sel.LineColor = Theme.Ole(Theme.MapChonVien);
                sel.FillTransparency = 160;
                if (l.Loai == ShpfileType.SHP_POINT)
                {
                    KieuDiem(sel, Theme.MapChon, 13, tkDefaultPointSymbol.dpsCircle);
                    sel.LineColor = Theme.Ole(Theme.Primary);
                    sel.LineWidth = 2;
                }
                else if (l.Loai == ShpfileType.SHP_POLYLINE)
                {
                    sel.LineColor = Theme.Ole(Theme.MapChonDuong);
                    sel.LineWidth = 6;
                }
                else
                {
                    sel.LineWidth = l.Loai == ShpfileType.SHP_POLYLINE ? 5 : 3;
                }
                l.KieuDang?.Invoke(l.Sf);
            }
            else
            {
                l.Sf.EditClear();
            }

            l.ChiSo.Clear();
            var cot = dt.Columns.Cast<DataColumn>().Where(c => c.ColumnName != "wkt").ToList();
            var chiSoTruong = cot.ToDictionary(c => c.ColumnName, c => l.Sf.get_FieldIndexByName(c.ColumnName));
            foreach (DataRow r in dt.Rows)
            {
                var wkt = r["wkt"] as string;
                if (string.IsNullOrEmpty(wkt)) continue;
                var shp = new Shape();
                if (!shp.ImportFromWKT(wkt)) continue;
                int idx = l.Sf.NumShapes;
                if (!l.Sf.EditInsertShape(shp, ref idx)) continue;
                foreach (var c in cot)
                    l.Sf.EditCellValue(chiSoTruong[c.ColumnName], idx, r[c] == DBNull.Value ? "" : Convert.ToString(r[c]));
                var ma = Convert.ToString(r["ma"]);
                if (!string.IsNullOrEmpty(ma)) l.ChiSo[ma] = idx;
            }
            if (l.Sf.Categories.Count > 0) l.Sf.Categories.ApplyExpressions();

            if (moi)
            {
                l.Handle = Map.AddLayer(l.Sf, true);
                Map.set_LayerName(l.Handle, l.Ten);
            }
        }

        // =====================================================================
        // Lớp tạm (kết quả phân tích, vùng vẽ, ứng viên)
        // =====================================================================
        /// <summary>
        /// Hiển thị kết quả lên lớp tạm. dt phải có cột wkt (EPSG:3857) và ma.
        /// Lớp tạm luôn nằm trên cùng; gọi lại với cùng khoá sẽ thay nội dung.
        /// </summary>
        public void HienLopTam(string khoa, string ten, ShpfileType loai, DataTable dt, Action<Shapefile> kieuDang)
        {
            // luôn tạo lại: kiểu dáng/phân loại màu có thể khác lần trước (vd cùng lớp nhưng đổi thang màu)
            if (_lop.TryGetValue(khoa, out var cu) && cu.LaLopTam && cu.Handle >= 0)
            {
                Map.RemoveLayer(cu.Handle);
                _lop.Remove(khoa);
            }
            var l = new LopBanDo { Khoa = khoa, Ten = ten, Loai = loai, LaLopTam = true, KieuDang = kieuDang };
            _lop[khoa] = l;
            DungLop(l, dt);
            Map.set_LayerVisible(l.Handle, true);
            Map.MoveLayerTop(Map.get_LayerPosition(l.Handle));
            Map.Redraw();
        }

        public void XoaLopTam(string khoa)
        {
            if (_lop.TryGetValue(khoa, out var l) && l.LaLopTam)
            {
                if (l.Handle >= 0) Map.RemoveLayer(l.Handle);
                _lop.Remove(khoa);
                Map.Redraw();
            }
        }

        public void XoaTatCaLopTam()
        {
            foreach (var k in _lop.Values.Where(x => x.LaLopTam).Select(x => x.Khoa).ToList())
                XoaLopTam(k);
        }

        /// <summary>Gắn nhãn cho lớp (vd "[thutu]"): chữ trắng đậm trên nền tròn màu, đặt ở góc trên phải điểm.</summary>
        public void DatNhan(string khoa, string bieuThuc, Color nen)
        {
            var l = Lop(khoa);
            if (l?.Sf == null) return;
            var lb = l.Sf.Labels;
            lb.Generate(bieuThuc, tkLabelPositioning.lpCenter, false);
            lb.FontName = Theme.FontChinh;
            lb.FontSize = 8;
            lb.FontBold = true;
            lb.FontColor = Theme.Ole(Color.White);
            lb.FrameVisible = true;
            lb.FrameType = tkLabelFrameType.lfRoundedRectangle;
            lb.FrameBackColor = Theme.Ole(nen);
            lb.FrameOutlineColor = Theme.Ole(Color.White);
            lb.Alignment = tkLabelAlignment.laTopRight;
            lb.OffsetX = 4;
            lb.OffsetY = -4;
            lb.AvoidCollisions = false;
            lb.Visible = true;
            Map.Redraw();
        }

        /// <summary>
        /// Tô màu phân lớp (choropleth): cột "kieu" của lớp tạm mang giá trị "0".."n-1" ứng với mau[i];
        /// giá trị khác (vd "x" – không có dữ liệu) dùng màu xám.
        /// </summary>
        public static Action<Shapefile> KieuPhanLop(Color[] mau, int doDuc = 170) => sf =>
        {
            var o = sf.DefaultDrawingOptions;
            o.FillColor = Theme.Ole(Theme.Border);
            o.FillTransparency = doDuc;
            o.LineColor = Theme.Ole(Color.White);
            o.LineWidth = 1;
            for (int i = 0; i < mau.Length; i++)
            {
                var cat = sf.Categories.Add("Lớp " + i);
                cat.Expression = "[kieu] = \"" + i + "\"";
                cat.DrawingOptions.FillColor = Theme.Ole(mau[i]);
                cat.DrawingOptions.FillTransparency = doDuc;
                cat.DrawingOptions.LineColor = Theme.Ole(Color.White);
                cat.DrawingOptions.LineWidth = 1;
            }
        };

        /// <summary>Chụp ảnh khung nhìn bản đồ hiện tại ra file PNG.</summary>
        public void ChupAnh(string duongDan)
        {
            int w = Math.Max(400, Map.Width), h = Math.Max(300, Map.Height);
            var ext = (Extents)Map.Extents;
            try
            {
                using (var bmp = new Bitmap(w, h))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.White);
                        var hdc = g.GetHdc();
                        try { Map.SnapShotToDC(hdc, ext, w); }
                        finally { g.ReleaseHdc(hdc); }
                    }
                    bmp.Save(duongDan, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch
            {
                var img = Map.SnapShot(ext) as MapWinGIS.Image;
                img?.Save(duongDan, false, ImageType.PNG_FILE, null);
            }
        }

        /// <summary>Kiểu dáng nhanh cho lớp tạm vùng (tô màu bán trong suốt).</summary>
        public static Action<Shapefile> KieuVung(Color mau, int doTrongSuot = 90) => sf =>
        {
            var o = sf.DefaultDrawingOptions;
            o.FillColor = Theme.Ole(mau);
            o.FillTransparency = doTrongSuot;
            o.LineColor = Theme.Ole(mau);
            o.LineWidth = 1.5f;
        };

        public static Action<Shapefile> KieuDiemTam(Color mau, float coDiem = 11) => sf =>
            KieuDiem(sf.DefaultDrawingOptions, mau, coDiem, tkDefaultPointSymbol.dpsCircle);

        // =====================================================================
        // Hiển thị / chọn / zoom
        // =====================================================================
        public LopBanDo Lop(string khoa) => _lop.TryGetValue(khoa, out var l) ? l : null;

        /// <summary>Giá trị một trường của đối tượng (theo mã) trong lớp đã nạp; null nếu không có.</summary>
        public string GiaTriTruong(string khoa, string ma, string truong)
        {
            var l = Lop(khoa);
            if (l?.Sf == null || ma == null || !l.ChiSo.TryGetValue(ma, out int i)) return null;
            int f = l.Sf.get_FieldIndexByName(truong);
            return f < 0 ? null : Convert.ToString(l.Sf.get_CellValue(f, i));
        }

        /// <summary>Màu hiển thị của tuyến trên bản đồ (khớp bảng màu <see cref="Theme.MauTuyen"/>).</summary>
        public Color? MauTuyen(string maTuyen) =>
            int.TryParse(GiaTriTruong(Tuyen, maTuyen, "kieu"), out int k) && k >= 0 && k < Theme.MauTuyen.Length
                ? Theme.MauTuyen[k] : (Color?)null;

        /// <summary>Lớp nghiệp vụ được bật/tắt (từ bất kỳ đâu) – panel Lớp bản đồ đồng bộ theo.</summary>
        public event Action<string, bool> HienThiThayDoi;

        public void HienLop(string khoa, bool hien)
        {
            var l = Lop(khoa);
            if (l == null || l.Handle < 0) return;
            Map.set_LayerVisible(l.Handle, hien);
            Map.Redraw();
            if (!l.LaLopTam) HienThiThayDoi?.Invoke(khoa, hien);
        }

        public bool LopDangHien(string khoa)
        {
            var l = Lop(khoa);
            return l != null && l.Handle >= 0 && Map.get_LayerVisible(l.Handle);
        }

        /// <summary>Highlight các đối tượng theo mã; zoom tới chúng nếu yêu cầu.</summary>
        public int Chon(string khoa, IEnumerable<string> danhSachMa, bool zoom = true)
        {
            var l = Lop(khoa);
            if (l?.Sf == null) return 0;
            l.Sf.SelectNone();
            int n = 0;
            foreach (var ma in danhSachMa)
                if (ma != null && l.ChiSo.TryGetValue(ma, out int i)) { l.Sf.set_ShapeSelected(i, true); n++; }
            if (n > 0 && zoom)
            {
                if (n == 1 && l.Loai == ShpfileType.SHP_POINT) ZoomToiDiem(l.Sf.get_Shape(l.ChiSo[danhSachMa.First(m => m != null && l.ChiSo.ContainsKey(m))]), 600);
                else Map.ZoomToSelected(l.Handle);
            }
            Map.Redraw();
            return n;
        }

        public void BoChonTatCa()
        {
            foreach (var l in _lop.Values.Where(x => x.Sf != null)) l.Sf.SelectNone();
            Map.Redraw();
        }

        public void ZoomToi(string khoa, string ma)
        {
            var l = Lop(khoa);
            if (l?.Sf == null || !l.ChiSo.TryGetValue(ma, out int i)) return;
            if (l.Loai == ShpfileType.SHP_POINT) ZoomToiDiem(l.Sf.get_Shape(i), 600);
            else Map.ZoomToShape(l.Handle, i);
        }

        private void ZoomToiDiem(Shape shp, double banKinhM)
        {
            var p = shp.Center;
            var e = new Extents();
            e.SetBounds(p.x - banKinhM, p.y - banKinhM, 0, p.x + banKinhM, p.y + banKinhM, 0);
            Map.Extents = e;
        }

        /// <summary>Zoom về toàn Hà Nội.</summary>
        public void ToanCanh()
        {
            var e = new Extents();
            e.SetBounds(105.28, 20.55, 0, 106.03, 21.40, 0);
            Map.SetGeographicExtents(e);
        }

        // =====================================================================
        // Toạ độ
        // =====================================================================
        public void PixelSangProj(double px, double py, out double x, out double y)
        {
            x = y = 0;
            Map.PixelToProj(px, py, ref x, ref y);
        }

        public void ProjSangDo(double x, double y, out double lon, out double lat)
        {
            lon = lat = 0;
            Map.ProjToDegrees(x, y, ref lon, ref lat);
        }

        public void ProjSangPixel(double x, double y, out double px, out double py)
        {
            px = py = 0;
            Map.ProjToPixel(x, y, ref px, ref py);
        }

        /// <summary>Số mét (EPSG:3857) trên một pixel màn hình ở mức zoom hiện tại.</summary>
        public double MetTrenPixel()
        {
            PixelSangProj(0, 0, out double x1, out _);
            PixelSangProj(100, 0, out double x2, out _);
            return Math.Abs(x2 - x1) / 100.0;
        }
    }
}
