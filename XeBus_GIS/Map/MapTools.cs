using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AxMapWinGIS;
using MapWinGIS;
using XeBus_GIS.UI;

namespace XeBus_GIS.Map
{
    public enum CongCu
    {
        Keo,        // kéo bản đồ (pan)
        PhongTo,
        ThuNho,
        ChonDoiTuong, // click xem thông tin đối tượng
        VeVung,     // vẽ đa giác: click trái thêm đỉnh, double-click / click phải kết thúc
        ChonDiem,   // click 1 điểm (tra cứu theo bán kính, chọn vị trí trạm...)
        DoDai,      // đo khoảng cách (công cụ đo của MapWinGIS – trắc địa trên ellipsoid)
        DoDienTich  // đo diện tích
    }

    /// <summary>
    /// Xử lý thao tác chuột trên bản đồ. Toạ độ phát ra theo EPSG:3857 (mét Web Mercator).
    /// </summary>
    public class MapTools
    {
        private readonly MapManager _mm;
        private AxMap Map => _mm.Map;
        private readonly List<double[]> _dinh = new List<double[]>();
        private int _hVe = -1;

        public CongCu HienTai { get; private set; } = CongCu.Keo;

        /// <summary>Click chọn đối tượng: (x, y, dung sai mét) theo EPSG:3857.</summary>
        public event Action<double, double, double> ClickChonDoiTuong;
        /// <summary>Vẽ vùng xong: WKT POLYGON theo EPSG:3857.</summary>
        public event Action<string> VeVungXong;
        /// <summary>Chọn điểm xong: (x, y) theo EPSG:3857.</summary>
        public event Action<double, double> ChonDiemXong;
        /// <summary>Di chuột: (kinh độ, vĩ độ).</summary>
        public event Action<double, double> ToaDoThayDoi;
        /// <summary>Đổi công cụ.</summary>
        public event Action<CongCu> CongCuThayDoi;

        public MapTools(MapManager mm)
        {
            _mm = mm;
            Map.MouseDownEvent += Map_MouseDownEvent;
            Map.MouseMoveEvent += Map_MouseMoveEvent;
            Map.DblClick += Map_DblClick;
        }

        public void Dat(CongCu cc)
        {
            HuyVe();
            if ((HienTai == CongCu.DoDai || HienTai == CongCu.DoDienTich) && cc != HienTai)
                Map.Measuring.Clear();
            HienTai = cc;
            switch (cc)
            {
                case CongCu.Keo: Map.CursorMode = tkCursorMode.cmPan; break;
                case CongCu.PhongTo: Map.CursorMode = tkCursorMode.cmZoomIn; break;
                case CongCu.ThuNho: Map.CursorMode = tkCursorMode.cmZoomOut; break;
                case CongCu.ChonDoiTuong:
                    Map.CursorMode = tkCursorMode.cmNone;
                    Map.MapCursor = tkCursor.crsrHand;
                    break;
                case CongCu.VeVung:
                case CongCu.ChonDiem:
                    Map.CursorMode = tkCursorMode.cmNone;
                    Map.MapCursor = tkCursor.crsrCross;
                    break;
                case CongCu.DoDai:
                case CongCu.DoDienTich:
                    var m = Map.Measuring;
                    m.Clear();
                    m.MeasuringType = cc == CongCu.DoDai ? tkMeasuringType.MeasureDistance : tkMeasuringType.MeasureArea;
                    m.LengthUnits = tkLengthDisplayMode.ldmMetric;
                    m.AreaUnits = tkAreaDisplayMode.admHectars;
                    m.ShowTotalLength = true;
                    m.LineColor = Theme.Ole(Theme.Highlight);
                    m.FillColor = Theme.Ole(Theme.Highlight);
                    m.FillTransparency = 60;
                    Map.CursorMode = tkCursorMode.cmMeasure;
                    break;
            }
            CongCuThayDoi?.Invoke(cc);
        }

        private void Map_MouseDownEvent(object sender, _DMapEvents_MouseDownEvent e)
        {
            _mm.PixelSangProj(e.x, e.y, out double x, out double y);
            bool trai = e.button == 1;
            bool phai = e.button == 2;

            switch (HienTai)
            {
                case CongCu.ChonDoiTuong:
                    if (trai) ClickChonDoiTuong?.Invoke(x, y, _mm.DungSaiChonM());
                    break;

                case CongCu.ChonDiem:
                    if (trai)
                    {
                        Dat(CongCu.ChonDoiTuong);
                        ChonDiemXong?.Invoke(x, y);
                    }
                    break;

                case CongCu.VeVung:
                    if (trai)
                    {
                        _dinh.Add(new[] { x, y });
                        VeLaiPhacThao();
                    }
                    else if (phai)
                    {
                        KetThucVe();
                    }
                    break;
            }
        }

        private void Map_DblClick(object sender, EventArgs e)
        {
            if (HienTai == CongCu.VeVung) KetThucVe();
        }

        private void Map_MouseMoveEvent(object sender, _DMapEvents_MouseMoveEvent e)
        {
            if (ToaDoThayDoi == null) return;
            _mm.PixelSangProj(e.x, e.y, out double x, out double y);
            _mm.ProjSangDo(x, y, out double lon, out double lat);
            ToaDoThayDoi(lon, lat);
        }

        private void VeLaiPhacThao()
        {
            if (_hVe >= 0) Map.ClearDrawing(_hVe);
            _hVe = Map.NewDrawing(tkDrawReferenceList.dlSpatiallyReferencedList);
            uint mau = Theme.Ole(Theme.Highlight);
            for (int i = 0; i < _dinh.Count; i++)
            {
                Map.DrawPointEx(_hVe, _dinh[i][0], _dinh[i][1], 7, mau);
                if (i > 0) Map.DrawLineEx(_hVe, _dinh[i - 1][0], _dinh[i - 1][1], _dinh[i][0], _dinh[i][1], 2, mau);
            }
            if (_dinh.Count > 2)
                Map.DrawLineEx(_hVe, _dinh[_dinh.Count - 1][0], _dinh[_dinh.Count - 1][1], _dinh[0][0], _dinh[0][1], 1, mau);
        }

        private void KetThucVe()
        {
            // bỏ các đỉnh trùng nhau (double-click sinh thêm đỉnh)
            double tol = _mm.MetTrenPixel() * 3;
            var dinh = new List<double[]>();
            foreach (var d in _dinh)
                if (dinh.Count == 0 || Math.Abs(dinh.Last()[0] - d[0]) > tol || Math.Abs(dinh.Last()[1] - d[1]) > tol)
                    dinh.Add(d);

            if (dinh.Count < 3) return; // chưa đủ đỉnh – tiếp tục vẽ

            var inv = CultureInfo.InvariantCulture;
            var sb = new StringBuilder("POLYGON((");
            foreach (var d in dinh) sb.Append(d[0].ToString("R", inv)).Append(' ').Append(d[1].ToString("R", inv)).Append(',');
            sb.Append(dinh[0][0].ToString("R", inv)).Append(' ').Append(dinh[0][1].ToString("R", inv)).Append("))");

            var wkt = sb.ToString();
            Dat(CongCu.ChonDoiTuong);
            VeVungXong?.Invoke(wkt);
        }

        /// <summary>Huỷ phác thảo đang vẽ.</summary>
        public void HuyVe()
        {
            _dinh.Clear();
            if (_hVe >= 0)
            {
                Map.ClearDrawing(_hVe);
                _hVe = -1;
                Map.Redraw();
            }
        }
    }
}
