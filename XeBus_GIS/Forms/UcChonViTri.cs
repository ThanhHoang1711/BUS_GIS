using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShpfileType = MapWinGIS.ShpfileType;
using tkDashStyle = MapWinGIS.tkDashStyle;
using XeBus_GIS.Data;
using XeBus_GIS.Map;
using XeBus_GIS.Security;
using XeBus_GIS.UI;
using XeBus_GIS.UI.Controls;

namespace XeBus_GIS.Forms
{
    /// <summary>
    /// Tab "Chọn vị trí đặt trạm" trong màn hình Đề xuất (chỉ nhóm Quản trị viên – quyền DX_CHONVITRI).
    /// Người dùng chọn khu vực → CSDL rải điểm ứng viên dọc đường và chấm 5 tiêu chí không gian (xebus.fn_chon_vitri):
    /// dân cư chưa có trạm · công trình thu hút · khoảng cách trạm hiện có · giao thông · tiếp cận an toàn.
    /// Điểm tổng = Σ trọng số × tiêu chí tính ngay tại đây khi kéo thanh trọng số (không gọi lại CSDL).
    /// Trực quan hoá: đường tô màu theo điểm tổng hoặc từng tiêu chí + lớp ngữ cảnh của tiêu chí đó, bảng xếp hạng có
    /// thanh đóng góp từng tiêu chí, biểu đồ radar và bảng giải thích số liệu của vị trí đang chọn (khung dưới bản đồ).
    /// </summary>
    public partial class UcChonViTri : UserControl
    {
        private const string LopDan = "CV_DAN", LopPv = "CV_PV", LopNut = "CV_NUT", LopVung = "CV_VUNG",
                             LopDoan = "CV_DOAN", LopDiem = "CV_DIEM", LopVong = "CV_VONG", LopTia = "CV_TIA",
                             LopTop = "CV_TOP", LopChon = "CV_CHON";
        private static readonly string[] TatCaLop = { LopDan, LopPv, LopNut, LopVung, LopDoan, LopDiem, LopVong, LopTia, LopTop, LopChon };

        private const double DienTichToiDa = 120;   // km² – khu vực lớn hơn nên chia nhỏ (thời gian tính + đọc kết quả)

        private static readonly string[] TenTC = { "Nhu cầu dân cư", "Công trình thu hút", "Xa trạm hiện có", "Giao thông", "Tiếp cận an toàn" };
        private static readonly string[] TenNgan = { "Dân cư", "Công trình", "Trạm hiện có", "Giao thông", "Tiếp cận" };
        private static readonly string[] IconTC = { "", "", Icons.Bus, "", "" };
        private static readonly string[] MoTaTC =
        {
            "Dân chưa có trạm trong bán kính phục vụ (ở gần tính đủ, ở xa tính một nửa)",
            "Trường, bệnh viện, chợ, KCN… trong bán kính; gần và chưa có trạm được ưu tiên",
            "Lấp khoảng trống: cách trạm hiện có < 150 m = 0 điểm, ≥ 400 m = tối đa",
            "Cấp đường (quốc lộ > tỉnh lộ > đường đô thị) và có tuyến buýt đi qua",
            "Không đặt sát nút giao: < 25 m = 0 điểm, ≥ 75 m = tối đa",
        };
        private static readonly (string ten, int[] w)[] MauTrongSo =
        {
            ("Cân bằng", new[] { 7, 5, 4, 2, 2 }),
            ("Ưu tiên dân cư", new[] { 10, 3, 4, 2, 2 }),
            ("Ưu tiên công trình", new[] { 4, 10, 3, 2, 2 }),
            ("Lấp khoảng trống", new[] { 6, 3, 10, 2, 2 }),
        };
        private static readonly int[] NguongMatDo = { 2000, 5000, 10000, 20000 };   // người/km² – 5 lớp màu dân cư

        /// <summary>Một điểm ứng viên đã chấm.</summary>
        private class ViTri
        {
            public int Id;
            public string MaDoan, TenDuong, LoaiDuong, Nguon, TenPhuong, CtMoTa, TramGan, Tuyen, WktDiem, WktDoan;
            public double Lon, Lat, DiemCt, Diem;
            public int Dan, DanGan, SoCt, SoCtMoi;
            public int? KcTram, KcNut;
            public double[] C = new double[5];
            public double[] DongGop = new double[5];   // điểm đóng góp của từng tiêu chí (tổng = Diem)
            public int Hang;                           // thứ hạng trong danh sách đề xuất (0 = không nằm trong)
            public bool TrenDuong => Nguon == "Đường";
            public string TenViTri => string.IsNullOrWhiteSpace(TenDuong) ? (TrenDuong ? "Đường chưa có tên" : "Khu dân cư") : TenDuong;
        }

        private enum DangCho { KhongCo, VeVung, ChonDiem }

        private readonly BoiCanh _bc;
        private bool _daNap, _dangHien;
        private DangCho _dangCho = DangCho.KhongCo;

        private string _vungVe;            // WKT 3857 vùng người dùng vẽ
        private string _vung;              // khu vực đã phân tích
        private string _tenVung;
        private double _banKinh, _buoc;
        private List<ViTri> _ds = new List<ViTri>();
        private List<ViTri> _top = new List<ViTri>();
        private ViTri _chon;
        private double[] _tbKhuVuc = new double[5];
        private DataTable _lopDan, _lopPv, _lopNut;

        // cột trái
        private ThanhTrongSo[] _ts;
        private NhomChip _chipMau, _chipCheDo;
        private Label _lblGoiYTrongSo;
        private ThanhThangMau _thang;
        private Label _lblGiaiThich;
        private KhungChuGiai _chuGiai;
        private bool _dangDatMau;
        private readonly Timer _hoan = new Timer { Interval = 180 };

        // khung dưới bản đồ
        private Panel _khungDuoi;
        private Button _tabTop, _tabDuong, _btnChonTrenBanDo;
        private DataGridView _dgvTop, _dgvDuong;
        private BieuDoRadar _radar;
        private BangTieuChi _bang;
        private Label _lblChon, _lblKetQua;
        private TextBox _txtTen;
        private Button _btnTao;

        /// <summary>Dùng cho VS Designer.</summary>
        public UcChonViTri()
        {
            InitializeComponent();
        }

        public UcChonViTri(BoiCanh boiCanh) : this()
        {
            _bc = boiCanh;
            ApDungGiaoDien();
            TaoTrongSo();
            TaoHienThi();
            TaoKhungDuoi();
            GanSuKien();
            CapNhatMoTaVung();
            DatCheDoHienThi(0, veLai: false);
        }

        // =====================================================================
        // Giao diện – cột trái
        // =====================================================================
        private void ApDungGiaoDien()
        {
            BackColor = Theme.Surface;
            foreach (var p in new Control[] { pnlCuon, pnlKhuVuc, pnlTrongSo, pnlNut, pnlHienThi }) p.BackColor = Theme.Surface;
            foreach (var l in new[] { lblH1, lblH2, lblH3 }) UiHelper.TieuDeKhuVuc(l);
            foreach (var r in new[] { rdoPhuong, rdoVe }) { r.ForeColor = Theme.Text; r.Cursor = Cursors.Hand; }
            foreach (var l in new[] { lblBanKinh, lblSoDX, lblCach }) UiHelper.NhanTruong(l);
            foreach (var l in new[] { lblVung, lblTomTat })
            {
                l.Font = Theme.FontNhan;
                l.ForeColor = Theme.Muted;
                l.UseMnemonic = false;
            }
            UiHelper.NutPhu(btnVe);
            UiHelper.GanIcon(btnVe, Icons.VeVung);
            UiHelper.NutChinh(btnPhanTich);
            UiHelper.GanIcon(btnPhanTich, Icons.PhanTich);
            UiHelper.O(cboPhuong, nudBanKinh, nudSoDX, nudCach);
            UiHelper.TipCho(btnVe, "Click trái thêm đỉnh, double-click hoặc click phải để kết thúc");
            UiHelper.TipCho(nudCach, "Hai vị trí đề xuất không gần nhau hơn khoảng này (tránh đề xuất trùng vùng phục vụ)");
            UiHelper.DuongKe(pnlTrongSo);
            UiHelper.DuongKe(pnlHienThi);
            UiHelper.CoGianNgang(pnlKhuVuc);
            UiHelper.CoGianNgang(pnlNut);
        }

        private void TaoTrongSo()
        {
            _lblGoiYTrongSo = new Label
            {
                Text = "Kéo thanh để đổi mức quan trọng – điểm tổng và bản đồ cập nhật ngay, không cần phân tích lại.",
                UseMnemonic = false,
            };
            UiHelper.ChuPhu(_lblGoiYTrongSo);
            _chipMau = new NhomChip();
            _chipMau.Dat(MauTrongSo.Select(m => m.ten).Concat(new[] { "Tuỳ chỉnh" }).ToArray());
            _ts = new ThanhTrongSo[5];
            for (int i = 0; i < 5; i++)
            {
                _ts[i] = new ThanhTrongSo
                {
                    TenTieuChi = TenTC[i],
                    MoTa = MoTaTC[i],
                    Icon = IconTC[i],
                    Mau = Theme.MauTieuChi[i],
                };
                UiHelper.TipCho(_ts[i], TenTC[i] + ": " + MoTaTC[i]);
            }
            pnlTrongSo.Controls.Add(_lblGoiYTrongSo);
            pnlTrongSo.Controls.Add(_chipMau);
            pnlTrongSo.Controls.AddRange(_ts);
            pnlTrongSo.Resize += (s, e) => BoCucTrongSo();
            DatMauTrongSo(0);
            BoCucTrongSo();
        }

        private void BoCucTrongSo()
        {
            int le = UiHelper.Dp(12), w = Math.Max(UiHelper.Dp(120), pnlTrongSo.ClientSize.Width - 2 * le);
            int y = lblH2.Bottom + UiHelper.Dp(4);
            int hGoiY = TextRenderer.MeasureText(_lblGoiYTrongSo.Text, _lblGoiYTrongSo.Font, new Size(w, 0), TextFormatFlags.WordBreak).Height;
            _lblGoiYTrongSo.SetBounds(le, y, w, hGoiY + UiHelper.Dp(2));
            y += hGoiY + UiHelper.Dp(8);
            _chipMau.SetBounds(le, y, w, _chipMau.Height);
            _chipMau.Width = w;   // NhomChip tự tính lại chiều cao theo chiều rộng
            y = _chipMau.Bottom + UiHelper.Dp(10);
            foreach (var t in _ts)
            {
                t.SetBounds(le, y, w, UiHelper.Dp(74));
                y += t.Height + UiHelper.Dp(4);
            }
            int cao = y + UiHelper.Dp(4);
            if (pnlTrongSo.Height != cao) pnlTrongSo.Height = cao;
        }

        private void TaoHienThi()
        {
            _chipCheDo = new NhomChip();
            _chipCheDo.Dat(new[] { "Tổng điểm" }.Concat(TenNgan).ToArray(),
                           new Color?[] { null }.Concat(Theme.MauTieuChi.Select(c => (Color?)c)).ToArray());
            _thang = new ThanhThangMau();
            _lblGiaiThich = new Label { UseMnemonic = false };
            UiHelper.ChuPhu(_lblGiaiThich);
            _chuGiai = new KhungChuGiai();
            pnlHienThi.Controls.Add(_chipCheDo);
            pnlHienThi.Controls.Add(_thang);
            pnlHienThi.Controls.Add(_lblGiaiThich);
            pnlHienThi.Controls.Add(_chuGiai);
            pnlHienThi.Resize += (s, e) => BoCucHienThi();
            BoCucHienThi();
        }

        private void BoCucHienThi()
        {
            int le = UiHelper.Dp(12), w = Math.Max(UiHelper.Dp(120), pnlHienThi.ClientSize.Width - 2 * le);
            int y = lblH3.Bottom + UiHelper.Dp(8);
            _chipCheDo.SetBounds(le, y, w, _chipCheDo.Height);
            _chipCheDo.Width = w;
            y = _chipCheDo.Bottom + UiHelper.Dp(10);
            _thang.SetBounds(le, y, w, UiHelper.Dp(50));
            y = _thang.Bottom + UiHelper.Dp(4);
            int h = TextRenderer.MeasureText(_lblGiaiThich.Text, _lblGiaiThich.Font, new Size(w, 0), TextFormatFlags.WordBreak).Height;
            _lblGiaiThich.SetBounds(le, y, w, h + UiHelper.Dp(2));
            y = _lblGiaiThich.Bottom + UiHelper.Dp(6);
            _chuGiai.SetBounds(le, y, w, _chuGiai.Height);
            _chuGiai.Width = w;
            int cao = _chuGiai.Bottom + UiHelper.Dp(16);
            if (pnlHienThi.Height != cao) pnlHienThi.Height = cao;
        }

        // =====================================================================
        // Giao diện – khung dưới bản đồ: xếp hạng | radar | giải thích + tạo trạm
        // =====================================================================
        private void TaoKhungDuoi()
        {
            _khungDuoi = new Panel { BackColor = Theme.Surface };
            var bang = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Theme.Surface };
            bang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 41));
            bang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26));
            bang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            bang.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _khungDuoi.Controls.Add(bang);

            // --- cột 1: xếp hạng
            var c1 = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0), BackColor = Theme.Surface };
            var hang1 = new Panel { Dock = DockStyle.Top, Height = UiHelper.Dp(40), BackColor = Theme.Surface, Padding = new Padding(UiHelper.Dp(8), UiHelper.Dp(4), UiHelper.Dp(8), 0) };
            _tabTop = new Button { Text = "Vị trí đề xuất tốt nhất", Dock = DockStyle.Left, Width = UiHelper.Dp(180) };
            _tabDuong = new Button { Text = "Xếp hạng tuyến đường", Dock = DockStyle.Left, Width = UiHelper.Dp(170) };
            _btnChonTrenBanDo = new Button { Text = "Chọn trên bản đồ", Dock = DockStyle.Right, Width = UiHelper.Dp(150) };
            UiHelper.Tab(_tabTop);
            UiHelper.Tab(_tabDuong);
            UiHelper.NutPhu(_btnChonTrenBanDo);
            UiHelper.GanIcon(_btnChonTrenBanDo, Icons.Identify);
            UiHelper.TipCho(_btnChonTrenBanDo, "Click một đoạn đường đã tô màu trên bản đồ để xem điểm của vị trí gần nhất");
            hang1.Controls.Add(_tabDuong);
            hang1.Controls.Add(_tabTop);
            hang1.Controls.Add(_btnChonTrenBanDo);
            UiHelper.DuongKe(hang1, 0);
            _dgvTop = TaoLuoi();
            _dgvDuong = TaoLuoi();
            _dgvDuong.Visible = false;
            UiHelper.TrangThaiRong(_dgvTop, "Chưa có kết quả – chọn khu vực rồi bấm \"Phân tích khu vực\".");
            UiHelper.TrangThaiRong(_dgvDuong, "Chưa có kết quả.");
            c1.Controls.Add(_dgvTop);
            c1.Controls.Add(_dgvDuong);
            c1.Controls.Add(hang1);
            bang.Controls.Add(c1, 0, 0);

            // --- cột 2: radar
            var c2 = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0), BackColor = Theme.Surface };
            c2.Paint += (s, e) => { using (var pen = new Pen(Theme.Border)) e.Graphics.DrawLine(pen, 0, 0, 0, c2.Height); };
            var tieuDe2 = TaoTieuDe("Hồ sơ tiêu chí");
            UiHelper.TipCho(tieuDe2, "Vùng màu: vị trí đang xem · nét đứt: trung bình các vị trí ứng viên trong khu vực (thang 0–100)");
            TaoRadar();
            c2.Controls.Add(_radar);
            c2.Controls.Add(tieuDe2);
            bang.Controls.Add(c2, 1, 0);

            // --- cột 3: giải thích + tạo trạm
            var c3 = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0), BackColor = Theme.Surface };
            c3.Paint += (s, e) => { using (var pen = new Pen(Theme.Border)) e.Graphics.DrawLine(pen, 0, 0, 0, c3.Height); };
            _lblChon = TaoTieuDe("Vì sao vị trí này?");
            _lblChon.AutoEllipsis = true;
            var cuon = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Theme.Surface, Padding = new Padding(UiHelper.Dp(12), UiHelper.Dp(4), UiHelper.Dp(12), UiHelper.Dp(4)) };
            _bang = new BangTieuChi { Dock = DockStyle.Top };
            cuon.Controls.Add(_bang);
            var day = new Panel { Dock = DockStyle.Bottom, Height = UiHelper.Dp(86), BackColor = Theme.Surface, Padding = new Padding(UiHelper.Dp(12), UiHelper.Dp(8), UiHelper.Dp(12), UiHelper.Dp(4)) };
            day.Paint += (s, e) => { using (var pen = new Pen(Theme.Border)) e.Graphics.DrawLine(pen, 0, 0, day.Width, 0); };
            _txtTen = new TextBox { Dock = DockStyle.Top };
            _btnTao = new Button { Text = "Tạo trạm đề xuất", Dock = DockStyle.Right, Width = UiHelper.Dp(156) };
            UiHelper.NutChinh(_btnTao);
            UiHelper.GanIcon(_btnTao, Icons.Them);
            UiHelper.O(_txtTen);
            var hangNut = new Panel { Dock = DockStyle.Top, Height = UiHelper.Dp(40), BackColor = Theme.Surface, Padding = new Padding(0, UiHelper.Dp(6), 0, 0) };
            _lblKetQua = new Label { Dock = DockStyle.Fill, UseMnemonic = false, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
            UiHelper.ChuPhu(_lblKetQua);
            hangNut.Controls.Add(_lblKetQua);
            hangNut.Controls.Add(new Panel { Dock = DockStyle.Right, Width = UiHelper.Dp(8), BackColor = Theme.Surface });
            hangNut.Controls.Add(_btnTao);
            day.Controls.Add(hangNut);
            day.Controls.Add(_txtTen);
            UiHelper.TipCho(_txtTen, "Tên trạm đề xuất sẽ tạo");
            c3.Controls.Add(cuon);
            c3.Controls.Add(day);
            c3.Controls.Add(_lblChon);
            bang.Controls.Add(c3, 2, 0);

            ChonTabDuoi(true);
            DatChonTrong();
        }

        private static Label TaoTieuDe(string chu)
        {
            var l = new Label
            {
                Dock = DockStyle.Top,
                Height = UiHelper.Dp(40),
                Padding = new Padding(UiHelper.Dp(12), 0, UiHelper.Dp(12), 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = chu,
                UseMnemonic = false,
                Font = Theme.FontDam,
                ForeColor = Theme.Primary,
                BackColor = Theme.Surface,
            };
            l.Paint += (s, e) => { using (var pen = new Pen(Theme.Border)) e.Graphics.DrawLine(pen, 0, l.Height - 1, l.Width, l.Height - 1); };
            return l;
        }

        private static DataGridView TaoLuoi()
        {
            var g = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.Luoi(g);
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            return g;
        }

        private void TaoRadar()
        {
            _radar = new BieuDoRadar { Dock = DockStyle.Fill };
            _radar.DatTruc(TenNgan, Theme.MauTieuChi);
            VeRadar();
        }

        // =====================================================================
        // Sự kiện
        // =====================================================================
        private void GanSuKien()
        {
            btnVe.Click += (s, e) =>
            {
                rdoVe.Checked = true;
                _dangCho = DangCho.VeVung;
                _bc.CongCu.Dat(CongCu.VeVung);
                _bc.TrangThai("Khoanh khu vực muốn đặt trạm: click trái thêm đỉnh, double-click / click phải để kết thúc.", false);
            };
            cboPhuong.SelectionChangeCommitted += (s, e) => rdoPhuong.Checked = true;
            cboPhuong.TextUpdate += (s, e) => rdoPhuong.Checked = true;
            rdoPhuong.CheckedChanged += (s, e) => CapNhatMoTaVung();
            btnPhanTich.Click += async (s, e) => await PhanTichAsync();

            _chipMau.ChonDoi += i => { if (!_dangDatMau && i < MauTrongSo.Length) DatMauTrongSo(i); };
            foreach (var t in _ts) t.GiaTriDoi += _ => KhiDoiTrongSo();
            _hoan.Tick += (s, e) => { _hoan.Stop(); CapNhatKetQua(giuChon: true); };
            nudSoDX.ValueChanged += (s, e) => { if (_ds.Count > 0) CapNhatKetQua(giuChon: true); };
            nudCach.ValueChanged += (s, e) => { if (_ds.Count > 0) CapNhatKetQua(giuChon: true); };
            _chipCheDo.ChonDoi += i => DatCheDoHienThi(i, veLai: true);

            _tabTop.Click += (s, e) => ChonTabDuoi(true);
            _tabDuong.Click += (s, e) => ChonTabDuoi(false);
            _dgvTop.CellClick += (s, e) => { if (e.RowIndex >= 0) ChonTuLuoiTop(); };
            _dgvTop.SelectionChanged += (s, e) => { if (_dgvTop.Focused) ChonTuLuoiTop(); };
            _dgvDuong.CellClick += (s, e) => { if (e.RowIndex >= 0) ChonTuLuoiDuong(); };
            _dgvDuong.SelectionChanged += (s, e) => { if (_dgvDuong.Focused) ChonTuLuoiDuong(); };
            _dgvTop.CellPainting += VeOLuoiTop;
            _dgvDuong.CellPainting += VeOLuoiDuong;
            _btnChonTrenBanDo.Click += (s, e) =>
            {
                if (_ds.Count == 0) { UiHelper.CanhBao(FindForm(), "Hãy phân tích một khu vực trước."); return; }
                _dangCho = DangCho.ChonDiem;
                _bc.CongCu.Dat(CongCu.ChonDiem);
                _bc.TrangThai("Click một vị trí trên đoạn đường đã tô màu để xem điểm.", false);
            };
            _btnTao.Click += async (s, e) => await TaoTramAsync();

            _bc.CongCu.VeVungXong += async wkt => await KhiVeVungXongAsync(wkt);
            _bc.CongCu.ChonDiemXong += (x, y) => KhiChonDiem(x, y);
        }

        // =====================================================================
        // Vòng đời (do UcDeXuat gọi khi mở/ẩn tab)
        // =====================================================================
        public async void KichHoat()
        {
            _dangHien = true;
            if (!_daNap)
            {
                try
                {
                    var phuong = await Task.Run(() => TramRepository.PhuongXa());
                    UcTram.GanCombo(cboPhuong, phuong, null);
                    cboPhuong.SelectedIndex = -1;
                    _daNap = true;
                }
                catch (Exception ex)
                {
                    _bc.TrangThai("Lỗi nạp danh sách phường/xã: " + ex.GetBaseException().Message, true);
                }
            }
            if (!_dangHien) return;
            _bc.HienBangDuoi?.Invoke(_khungDuoi);
            VeBanDo();
            if (!Session.Co(Quyen.DxChonViTri)) _btnTao.Enabled = false;
        }

        public void NgungKichHoat()
        {
            _dangHien = false;
            _hoan.Stop();
            if (_dangCho != DangCho.KhongCo)
            {
                _dangCho = DangCho.KhongCo;
                _bc.CongCu.Dat(CongCu.ChonDoiTuong);
            }
            _bc.AnBangDuoi?.Invoke(_khungDuoi);
            foreach (var l in TatCaLop) _bc.BanDo.XoaLopTam(l);
        }

        // =====================================================================
        // Khu vực
        // =====================================================================
        private async Task KhiVeVungXongAsync(string wkt)
        {
            if (_dangCho != DangCho.VeVung) return;
            _dangCho = DangCho.KhongCo;
            _vungVe = wkt;
            rdoVe.Checked = true;
            VeVung(wkt);
            try
            {
                var dt = await Task.Run(() => DeXuatRepository.DienTichKm2(wkt));
                CapNhatMoTaVung(dt);
            }
            catch { CapNhatMoTaVung(); }
        }

        private void CapNhatMoTaVung(double? dienTich = null)
        {
            if (rdoPhuong.Checked)
                lblVung.Text = "Chọn phường/xã rồi bấm \"Phân tích khu vực\". Các điểm ứng viên được rải dọc mọi đoạn đường trong phường.";
            else
                lblVung.Text = _vungVe == null
                    ? "Bấm \"Vẽ vùng\" và khoanh khu vực muốn đặt trạm (nên ≤ " + DienTichToiDa.ToString("N0") + " km²)."
                    : $"Đã khoanh vùng{(dienTich.HasValue ? $" – diện tích {dienTich:N2} km²" : "")}.";
        }

        private void VeVung(string wkt)
        {
            if (wkt == null) { _bc.BanDo.XoaLopTam(LopVung); return; }
            _bc.BanDo.HienLopTam(LopVung, "Khu vực chọn vị trí", ShpfileType.SHP_POLYGON, BangWkt("VUNG", wkt), sf =>
            {
                MapManager.KieuVung(Theme.Accent, 12)(sf);
                sf.DefaultDrawingOptions.LineWidth = 2.5f;
                sf.DefaultDrawingOptions.LineStipple = tkDashStyle.dsDash;
            });
        }

        // =====================================================================
        // Phân tích
        // =====================================================================
        private async Task PhanTichAsync()
        {
            string vung, ten;
            double dienTich;
            if (rdoVe.Checked)
            {
                if (_vungVe == null) { UiHelper.CanhBao(FindForm(), "Hãy bấm \"Vẽ vùng\" và khoanh khu vực muốn đặt trạm trên bản đồ."); return; }
                vung = _vungVe;
                ten = "khu vực đã khoanh";
                dienTich = await Task.Run(() => DeXuatRepository.DienTichKm2(vung));
            }
            else
            {
                var ma = cboPhuong.SelectedValue as string;
                if (string.IsNullOrEmpty(ma)) { UiHelper.CanhBao(FindForm(), "Chọn phường/xã muốn đặt trạm."); cboPhuong.Focus(); return; }
                DataRow p = null;
                if (!await UiHelper.ChayAsync(btnPhanTich, async () => p = await Task.Run(() => DeXuatRepository.VungPhuong(ma)), "Đang đọc ranh giới…")) return;
                if (p == null) return;
                vung = Db.Str(p, "wkt");
                ten = Db.Str(p, "tenphuong");
                dienTich = Convert.ToDouble(p["dientich"]);
            }
            if (dienTich > DienTichToiDa)
            {
                UiHelper.CanhBao(FindForm(), $"Khu vực rộng {dienTich:N0} km² – hãy khoanh vùng nhỏ hơn {DienTichToiDa:N0} km² để kết quả chi tiết và nhanh.");
                return;
            }

            double r = (double)nudBanKinh.Value;
            double buoc = dienTich <= 15 ? 50 : 100;      // khu vực lớn: thưa điểm hơn để giữ tốc độ
            DataTable kq = null, dan = null, pv = null, nut = null;
            if (!await UiHelper.ChayAsync(btnPhanTich, () => Task.Run(() =>
                {
                    kq = ChonViTriRepository.ChamDiem(vung, r, buoc);
                    dan = ChonViTriRepository.DanCuChuaPhucVu(vung, r);
                    pv = DeXuatRepository.VungPhucVu(vung, r);
                    nut = ChonViTriRepository.NutGiao(vung);
                }), "Đang chấm điểm…"))
                return;

            _vung = vung;
            _tenVung = ten;
            _banKinh = r;
            _buoc = buoc;
            _ds = kq.Rows.Cast<DataRow>().Select(DocViTri).ToList();
            _lopDan = BangLop();
            foreach (DataRow d in dan.Rows)
                _lopDan.Rows.Add(Convert.ToString(d["ma"]), LopMatDo(Convert.ToDouble(d["md"])).ToString(CultureInfo.InvariantCulture), Convert.ToString(d["wkt"]));
            _lopPv = pv;
            _lopNut = nut;
            _chon = null;
            for (int i = 0; i < 5; i++) _tbKhuVuc[i] = _ds.Count == 0 ? 0 : _ds.Average(v => v.C[i]);
            _lblKetQua.Text = "";

            if (_ds.Count == 0)
            {
                lblTomTat.Text = "Không có đoạn đường đặt được trạm trong khu vực (mạng đường chỉ gồm trục chính) và không có khu dân cư thiếu trạm.";
                lblTomTat.ForeColor = Theme.WarningText;
            }
            CapNhatKetQua(giuChon: false);
            VeBanDo(zoom: true);
            if (_top.Count > 0) ChonViTri(_top[0], zoom: false);
            _bc.TrangThai($"Đã chấm {_ds.Count:N0} vị trí ứng viên trong {ten} (bán kính {r:N0} m).", false);
        }

        private static ViTri DocViTri(DataRow r)
        {
            var v = new ViTri
            {
                Id = Convert.ToInt32(r["id"]),
                MaDoan = Db.Str(r, "madoan"),
                TenDuong = Db.Str(r, "tenduong"),
                LoaiDuong = Db.Str(r, "loaiduong"),
                Nguon = Db.Str(r, "nguon"),
                TenPhuong = Db.Str(r, "tenphuong"),
                CtMoTa = Db.Str(r, "ct_mota"),
                TramGan = Db.Str(r, "tram_gan"),
                Tuyen = Db.Str(r, "tuyen"),
                WktDiem = Db.Str(r, "wkt_diem"),
                WktDoan = Db.Str(r, "wkt_doan"),
                Lon = Convert.ToDouble(r["kinhdo"]),
                Lat = Convert.ToDouble(r["vido"]),
                Dan = Db.Int(r, "dan") ?? 0,
                DanGan = Db.Int(r, "dan_gan") ?? 0,
                SoCt = Db.Int(r, "so_ct") ?? 0,
                SoCtMoi = Db.Int(r, "so_ct_moi") ?? 0,
                DiemCt = Db.Dbl(r, "diem_ct") ?? 0,
                KcTram = Db.Int(r, "kc_tram"),
                KcNut = Db.Int(r, "kc_nut"),
            };
            var cot = new[] { "c_dan", "c_ct", "c_tram", "c_gt", "c_tc" };
            for (int i = 0; i < 5; i++) v.C[i] = Db.Dbl(r, cot[i]) ?? 0;
            return v;
        }

        private static int LopMatDo(double nguoiKm2)
        {
            int i = 0;
            while (i < NguongMatDo.Length && nguoiKm2 >= NguongMatDo[i]) i++;
            return i;
        }

        // =====================================================================
        // Trọng số + điểm tổng
        // =====================================================================
        private void DatMauTrongSo(int i)
        {
            _dangDatMau = true;
            for (int k = 0; k < 5; k++) _ts[k].GiaTri = MauTrongSo[i].w[k];
            _chipMau.Chon = i;
            _dangDatMau = false;
            KhiDoiTrongSo();
        }

        private void KhiDoiTrongSo()
        {
            var w = TrongSo();
            for (int k = 0; k < 5; k++) _ts[k].TyTrong = $"{w[k] * 100:0}%";
            if (!_dangDatMau)
            {
                _dangDatMau = true;
                int mau = Array.FindIndex(MauTrongSo, m => m.w.SequenceEqual(_ts.Select(t => t.GiaTri)));
                _chipMau.Chon = mau >= 0 ? mau : MauTrongSo.Length;
                _dangDatMau = false;
                if (_ds.Count > 0) { _hoan.Stop(); _hoan.Start(); }
            }
        }

        /// <summary>Trọng số chuẩn hoá (tổng = 1). Tất cả bằng 0 ⇒ chia đều.</summary>
        private double[] TrongSo()
        {
            double tong = _ts.Sum(t => t.GiaTri);
            return _ts.Select(t => tong <= 0 ? 0.2 : t.GiaTri / tong).ToArray();
        }

        /// <summary>Tính lại điểm tổng, chọn danh sách đề xuất, xếp hạng đường, vẽ lại bảng + bản đồ.</summary>
        private void CapNhatKetQua(bool giuChon)
        {
            var w = TrongSo();
            foreach (var v in _ds)
            {
                for (int k = 0; k < 5; k++) v.DongGop[k] = 100 * w[k] * v.C[k];
                v.Diem = v.DongGop.Sum();
                v.Hang = 0;
            }

            // đề xuất: điểm cao nhất, các vị trí cách nhau ≥ nudCach (tránh trùng vùng phục vụ)
            double cach = (double)nudCach.Value;
            int soDx = (int)nudSoDX.Value;
            _top = new List<ViTri>();
            foreach (var v in _ds.OrderByDescending(x => x.Diem).ThenBy(x => x.Id))
            {
                if (_top.Count >= soDx || v.Diem <= 0) break;
                if (_top.Any(t => KhoangCachM(t, v) < cach)) continue;
                _top.Add(v);
                v.Hang = _top.Count;
            }

            HienLuoiTop();
            HienLuoiDuong();
            if (_ds.Count > 0)
            {
                var tot = _top.FirstOrDefault();
                lblTomTat.ForeColor = Theme.Muted;
                lblTomTat.Text = $"{_ds.Count:N0} vị trí ứng viên đã chấm trong {_tenVung}" +
                                 (tot != null ? $" · tốt nhất {tot.Diem:0} điểm ({tot.TenViTri})." : ".");
            }
            if (!giuChon || _chon == null) _chon = _top.FirstOrDefault();
            if (_dangHien) VeBanDo();
            CapNhatChiTiet();
            ChonDongTop();
        }

        private static double KhoangCachM(ViTri a, ViTri b)
        {
            const double R = 6371000;
            double p1 = a.Lat * Math.PI / 180, p2 = b.Lat * Math.PI / 180;
            double dp = p2 - p1, dl = (b.Lon - a.Lon) * Math.PI / 180;
            double h = Math.Sin(dp / 2) * Math.Sin(dp / 2) + Math.Cos(p1) * Math.Cos(p2) * Math.Sin(dl / 2) * Math.Sin(dl / 2);
            return 2 * R * Math.Asin(Math.Sqrt(h));
        }

        // =====================================================================
        // Bảng xếp hạng
        // =====================================================================
        private void ChonTabDuoi(bool top)
        {
            UiHelper.ChonTab(_tabTop, top);
            UiHelper.ChonTab(_tabDuong, !top);
            _dgvTop.Visible = top;
            _dgvDuong.Visible = !top;
        }

        private void HienLuoiTop()
        {
            var t = new DataTable();
            t.Columns.Add("#");
            t.Columns.Add("id", typeof(int));
            t.Columns.Add("Vị trí");
            t.Columns.Add("Điểm", typeof(double));
            t.Columns.Add("Đóng góp từng tiêu chí");
            t.Columns.Add("Dân chưa có trạm", typeof(int));
            t.Columns.Add("Cách trạm (m)", typeof(int));
            foreach (var v in _top)
                t.Rows.Add(v.Hang.ToString(), v.Id, v.TenViTri, Math.Round(v.Diem, 1), "", v.Dan,
                           v.KcTram.HasValue ? (object)v.KcTram.Value : DBNull.Value);
            _dgvTop.DataSource = t;
            var g = _dgvTop;
            g.Columns["id"].Visible = false;
            UiHelper.CotVuaDu(g, "#", "Điểm", "Dân chưa có trạm", "Cách trạm (m)");
            g.Columns["Vị trí"].FillWeight = 100;
            g.Columns["Vị trí"].MinimumWidth = UiHelper.Dp(110);
            g.Columns["Đóng góp từng tiêu chí"].FillWeight = 100;
            g.Columns["Đóng góp từng tiêu chí"].MinimumWidth = UiHelper.Dp(90);
            g.Columns["Đóng góp từng tiêu chí"].HeaderText = "Đóng góp tiêu chí";
            g.Columns["Điểm"].DefaultCellStyle.Format = "0";
            g.Columns["Điểm"].DefaultCellStyle.Font = Theme.FontDam;
            g.Columns["Dân chưa có trạm"].DefaultCellStyle.Format = "N0";
            g.Columns["Dân chưa có trạm"].HeaderText = "Dân +";
            g.Columns["Cách trạm (m)"].HeaderText = "Cách trạm";
            g.Columns["Cách trạm (m)"].DefaultCellStyle.Format = "N0";
            UiHelper.CanPhaiCotSo(g);
            foreach (DataGridViewColumn c in g.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
            foreach (DataGridViewRow row in g.Rows)
            {
                var v = TimTheoId(Convert.ToInt32(row.Cells["id"].Value));
                if (v == null) continue;
                row.Cells["Vị trí"].ToolTipText = v.TenViTri + (v.TenPhuong != null ? " – " + v.TenPhuong : "") +
                                                 (v.Tuyen != null ? "\nTuyến đi qua: " + v.Tuyen : "");
                row.Cells["Đóng góp từng tiêu chí"].ToolTipText = string.Join("\n",
                    Enumerable.Range(0, 5).Select(k => $"{TenNgan[k]}: {v.DongGop[k]:0.#} điểm (tiêu chí {v.C[k] * 100:0}/100)"));
            }
            g.ClearSelection();
        }

        private void HienLuoiDuong()
        {
            var t = new DataTable();
            t.Columns.Add("id", typeof(int));
            t.Columns.Add("Đường");
            t.Columns.Add("Loại");
            t.Columns.Add("Điểm cao nhất", typeof(double));
            t.Columns.Add("Điểm TB", typeof(double));
            t.Columns.Add("Mức phù hợp dọc đường");
            t.Columns.Add("Đoạn ≥ 70 điểm (m)", typeof(int));
            var nhom = _ds.Where(v => v.TrenDuong)
                          .GroupBy(v => string.IsNullOrWhiteSpace(v.TenDuong) ? "(" + v.MaDoan + ")" : v.TenDuong)
                          .Select(g => new
                          {
                              Ten = g.Key,
                              Loai = g.Select(v => v.LoaiDuong).FirstOrDefault(x => x != null) ?? "",
                              TotNhat = g.OrderByDescending(v => v.Diem).First(),
                              Tb = g.Average(v => v.Diem),
                              DaiTot = (int)Math.Round(g.Count(v => v.Diem >= 70) * _buoc),
                          })
                          .OrderByDescending(x => x.TotNhat.Diem);
            foreach (var x in nhom)
                t.Rows.Add(x.TotNhat.Id, x.Ten, x.Loai, Math.Round(x.TotNhat.Diem, 1), Math.Round(x.Tb, 1), "", x.DaiTot);
            _dgvDuong.DataSource = t;
            var gr = _dgvDuong;
            gr.Columns["id"].Visible = false;
            UiHelper.CotVuaDu(gr, "Điểm cao nhất", "Điểm TB", "Đoạn ≥ 70 điểm (m)");
            gr.Columns["Đường"].FillWeight = 90;
            gr.Columns["Đường"].MinimumWidth = UiHelper.Dp(110);
            gr.Columns["Loại"].FillWeight = 50;
            gr.Columns["Mức phù hợp dọc đường"].FillWeight = 100;
            gr.Columns["Mức phù hợp dọc đường"].MinimumWidth = UiHelper.Dp(90);
            gr.Columns["Điểm cao nhất"].HeaderText = "Cao nhất";
            gr.Columns["Đoạn ≥ 70 điểm (m)"].HeaderText = "Dài ≥ 70 điểm";
            foreach (var c in new[] { "Điểm cao nhất", "Điểm TB" }) gr.Columns[c].DefaultCellStyle.Format = "0";
            gr.Columns["Điểm cao nhất"].DefaultCellStyle.Font = Theme.FontDam;
            gr.Columns["Đoạn ≥ 70 điểm (m)"].DefaultCellStyle.Format = "N0";
            UiHelper.CanPhaiCotSo(gr);
            foreach (DataGridViewColumn c in gr.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
            gr.ClearSelection();
        }

        private ViTri TimTheoId(int id) => _ds.FirstOrDefault(v => v.Id == id);

        /// <summary>Ô "Đóng góp" = thanh chồng 5 màu (độ dài = điểm, cả ô = 100 điểm); ô "Điểm" = chấm màu theo thang phù hợp.</summary>
        private void VeOLuoiTop(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var ten = _dgvTop.Columns[e.ColumnIndex].Name;
            if (ten != "Đóng góp từng tiêu chí" && ten != "Điểm") return;
            var v = TimTheoId(Convert.ToInt32(_dgvTop.Rows[e.RowIndex].Cells["id"].Value));
            if (v == null) return;
            e.PaintBackground(e.CellBounds, (e.State & DataGridViewElementStates.Selected) != 0);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var o = e.CellBounds;
            if (ten == "Điểm")
            {
                int d = UiHelper.Dp(8);
                using (var br = new SolidBrush(MauPhuHop(v.Diem / 100))) g.FillEllipse(br, o.X + UiHelper.Dp(8), o.Y + (o.Height - d) / 2, d, d);
                TextRenderer.DrawText(g, v.Diem.ToString("0"), Theme.FontDam, new Rectangle(o.X + UiHelper.Dp(18), o.Y, o.Width - UiHelper.Dp(22), o.Height),
                    Theme.Text, TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
            else VeThanhDongGop(g, v.DongGop, Rectangle.Inflate(o, -UiHelper.Dp(8), -UiHelper.Dp(8)));
            e.Handled = true;
        }

        private void VeOLuoiDuong(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _dgvDuong.Columns[e.ColumnIndex].Name != "Mức phù hợp dọc đường") return;
            var ten = Convert.ToString(_dgvDuong.Rows[e.RowIndex].Cells["Đường"].Value);
            var diem = _ds.Where(v => v.TrenDuong && (string.IsNullOrWhiteSpace(v.TenDuong) ? "(" + v.MaDoan + ")" : v.TenDuong) == ten)
                          .OrderBy(v => v.MaDoan).ThenBy(v => v.Id).Select(v => v.Diem).ToList();
            e.PaintBackground(e.CellBounds, (e.State & DataGridViewElementStates.Selected) != 0);
            // dải màu: mỗi điểm ứng viên dọc đường là một vạch màu theo điểm tổng
            var r = Rectangle.Inflate(e.CellBounds, -UiHelper.Dp(8), -UiHelper.Dp(9));
            if (diem.Count > 0 && r.Width > 4)
            {
                float w = r.Width / (float)diem.Count;
                for (int i = 0; i < diem.Count; i++)
                    using (var br = new SolidBrush(MauPhuHop(diem[i] / 100)))
                        e.Graphics.FillRectangle(br, r.X + i * w, r.Y, Math.Max(1, w), r.Height);
            }
            e.Handled = true;
        }

        private static void VeThanhDongGop(Graphics g, double[] dongGop, Rectangle r)
        {
            using (var path = UiHelper.DuongBoGoc(r, UiHelper.Dp(3)))
            using (var br = new SolidBrush(Theme.Border)) g.FillPath(br, path);
            float x = r.X;
            var clip = g.Clip;
            using (var path = UiHelper.DuongBoGoc(r, UiHelper.Dp(3))) g.SetClip(path);
            for (int k = 0; k < 5; k++)
            {
                float w = (float)(r.Width * dongGop[k] / 100);
                if (w <= 0) continue;
                using (var br = new SolidBrush(Theme.MauTieuChi[k])) g.FillRectangle(br, x, r.Y, w, r.Height);
                x += w;
            }
            g.Clip = clip;
        }

        private static Color MauPhuHop(double v)
        {
            var thang = Theme.ThangMauPhuHop;
            return thang[LopPhuHop(v)];
        }

        private static int LopPhuHop(double v) => Math.Max(0, Math.Min(Theme.ThangMauPhuHop.Length - 1, (int)Math.Floor(v * Theme.ThangMauPhuHop.Length)));

        private void ChonTuLuoiTop()
        {
            if (_dgvTop.CurrentRow == null) return;
            var v = TimTheoId(Convert.ToInt32(_dgvTop.CurrentRow.Cells["id"].Value));
            if (v != null && v != _chon) ChonViTri(v, zoom: true);
        }

        private void ChonTuLuoiDuong()
        {
            if (_dgvDuong.CurrentRow == null) return;
            var v = TimTheoId(Convert.ToInt32(_dgvDuong.CurrentRow.Cells["id"].Value));
            if (v != null && v != _chon) ChonViTri(v, zoom: true);
        }

        private void ChonDongTop()
        {
            if (_chon == null) return;
            foreach (DataGridViewRow row in _dgvTop.Rows)
            {
                if (Convert.ToInt32(row.Cells["id"].Value) != _chon.Id) continue;
                _dgvTop.CurrentCell = row.Cells["Vị trí"];
                row.Selected = true;
                return;
            }
            _dgvTop.ClearSelection();
        }

        // =====================================================================
        // Vị trí đang chọn: radar + giải thích + lớp tia công trình / vòng phục vụ
        // =====================================================================
        private void KhiChonDiem(double x, double y)
        {
            if (_dangCho != DangCho.ChonDiem) return;
            _dangCho = DangCho.KhongCo;
            _bc.CongCu.Dat(CongCu.ChonDoiTuong);
            _bc.BanDo.ProjSangDo(x, y, out double lon, out double lat);
            var p = new ViTri { Lon = lon, Lat = lat };
            var gan = _ds.OrderBy(v => KhoangCachM(v, p)).FirstOrDefault();
            if (gan == null || KhoangCachM(gan, p) > Math.Max(150, _buoc * 2))
            {
                UiHelper.CanhBao(FindForm(), "Không có vị trí ứng viên gần điểm vừa click – hãy click lên đoạn đường đã tô màu trong khu vực.");
                return;
            }
            ChonViTri(gan, zoom: false);
        }

        private async void ChonViTri(ViTri v, bool zoom)
        {
            _chon = v;
            CapNhatChiTiet();
            ChonDongTop();
            if (!_dangHien || v == null) return;
            _bc.BanDo.HienLopTam(LopChon, "Vị trí đang xem", ShpfileType.SHP_POINT, BangWkt("C", v.WktDiem),
                                 MapManager.KieuDiemTam(Theme.MapChon, 17));
            try
            {
                string vong = null;
                DataTable tia = null;
                double r = _banKinh;
                await Task.Run(() =>
                {
                    vong = DeXuatRepository.WktVongPhucVu(v.Lon, v.Lat, r);
                    tia = ChonViTriRepository.CongTrinhQuanh(v.Lon, v.Lat, r);
                });
                if (_chon != v || !_dangHien) return;
                _bc.BanDo.HienLopTam(LopVong, "Bán kính phục vụ vị trí đang xem", ShpfileType.SHP_POLYGON, BangWkt("V", vong),
                                     sf =>
                                     {
                                         MapManager.KieuVung(Theme.Success, 35)(sf);
                                         sf.DefaultDrawingOptions.LineWidth = 2;
                                     });
                if (tia.Rows.Count > 0)
                    _bc.BanDo.HienLopTam(LopTia, "Công trình trong bán kính", ShpfileType.SHP_POLYLINE, tia,
                                         MapManager.KieuDuongPhanLop(new[] { Theme.Disabled, Theme.MauTieuChi[1] }, 1.6f));
                else _bc.BanDo.XoaLopTam(LopTia);
                DuaLenTren(LopTop, LopChon);
                if (zoom) _bc.BanDo.ZoomToi(LopVong, "V");
                _bc.BanDo.Map.Redraw();
            }
            catch (Exception ex)
            {
                _bc.TrangThai("Không vẽ được vùng phục vụ của vị trí: " + ex.GetBaseException().Message, true);
            }
        }

        private void DuaLenTren(params string[] lop)
        {
            foreach (var k in lop)
            {
                var l = _bc.BanDo.Lop(k);
                if (l != null) _bc.BanDo.Map.MoveLayerTop(_bc.BanDo.Map.get_LayerPosition(l.Handle));
            }
        }

        private void DatChonTrong()
        {
            _lblChon.Text = "Vì sao vị trí này?";
            _bang.Dat(null);
            _txtTen.Text = "";
            _btnTao.Enabled = false;
            VeRadar();
        }

        private void CapNhatChiTiet()
        {
            var v = _chon;
            if (v == null) { DatChonTrong(); return; }
            string hang = v.Hang > 0 ? $"#{v.Hang} · " : "";
            _lblChon.Text = $"{hang}{v.Diem:0} điểm · {v.TenViTri}{(v.TenPhuong != null ? ", " + v.TenPhuong : "")}";
            UiHelper.TipCho(_lblChon, _lblChon.Text);
            int r = (int)_banKinh;
            _bang.Dat(new[]
            {
                new DongTieuChi
                {
                    Ten = TenTC[0], Icon = IconTC[0], Mau = Theme.MauTieuChi[0], GiaTri = v.C[0],
                    SoLieu = $"{v.Dan:N0} người chưa có trạm trong {r:N0} m",
                    GhiChu = $"{v.DanGan:N0} người trong {r / 2:N0} m · dân số ước tính",
                },
                new DongTieuChi
                {
                    Ten = TenTC[1], Icon = IconTC[1], Mau = Theme.MauTieuChi[1], GiaTri = v.C[1],
                    SoLieu = v.SoCt == 0 ? $"Không có công trình thu hút trong {r:N0} m" : $"{v.SoCt} công trình, {v.SoCtMoi} chưa có trạm phục vụ",
                    GhiChu = v.CtMoTa,
                },
                new DongTieuChi
                {
                    Ten = TenTC[2], Icon = IconTC[2], Mau = Theme.MauTieuChi[2], GiaTri = v.C[2],
                    SoLieu = v.KcTram.HasValue ? $"Trạm gần nhất cách {v.KcTram:N0} m" : "Không có trạm hiện có",
                    GhiChu = v.TramGan,
                },
                new DongTieuChi
                {
                    Ten = TenTC[3], Icon = IconTC[3], Mau = Theme.MauTieuChi[3], GiaTri = v.C[3],
                    SoLieu = v.TrenDuong ? $"{v.TenViTri} · {v.LoaiDuong ?? "đường"}" : "Trong khu dân cư – mạng đường chưa có đường tiếp cận",
                    GhiChu = v.Tuyen != null ? $"Tuyến đi qua: {v.Tuyen} – có thể gán trạm vào tuyến" : "Chưa có tuyến buýt đi qua đoạn này",
                },
                new DongTieuChi
                {
                    Ten = TenTC[4], Icon = IconTC[4], Mau = Theme.MauTieuChi[4], GiaTri = v.C[4],
                    SoLieu = !v.TrenDuong ? "Chưa xác định (vị trí trong khu dân cư)"
                           : v.KcNut.HasValue ? $"Cách nút giao gần nhất {v.KcNut:N0} m" : "Không có nút giao trong 200 m",
                    GhiChu = v.TrenDuong && v.KcNut.HasValue && v.KcNut < 25 ? "Quá sát nút giao – nên dời vị trí dọc đường" : null,
                },
            });
            _txtTen.Text = "Trạm đề xuất " + v.TenViTri;
            _btnTao.Enabled = Session.Co(Quyen.DxChonViTri);
            VeRadar();
        }

        private void VeRadar()
        {
            if (_radar == null) return;
            _radar.DatGiaTri(_chon?.C.Select(c => Math.Round(c * 100)).ToArray(),
                             _ds.Count > 0 ? _tbKhuVuc.Select(c => Math.Round(c * 100)).ToArray() : null);
        }

        // =====================================================================
        // Bản đồ
        // =====================================================================
        private int _cheDo;

        private void DatCheDoHienThi(int cheDo, bool veLai)
        {
            _cheDo = cheDo;
            string tieuDe = cheDo == 0 ? "Điểm tổng của từng đoạn đường" : "Điểm tiêu chí " + TenTC[cheDo - 1].ToLowerInvariant();
            _thang.Dat(tieuDe, Theme.ThangMauPhuHop, "Kém (0)", "Tốt (100)");
            var deXuat = new ChuGiai("Vị trí đề xuất (số = thứ hạng)", Theme.Highlight, KieuKyHieu.Tron);
            var chon = new ChuGiai("Vị trí đang xem", Theme.MapChon, KieuKyHieu.Tron);
            switch (cheDo)
            {
                case 0:
                    _lblGiaiThich.Text = "Mỗi đoạn đường ngắn (≈ " + (_buoc > 0 ? _buoc : 50).ToString("N0") +
                                         " m) tô theo điểm tổng có trọng số. Vòng xanh lá: bán kính phục vụ của vị trí đang xem.";
                    _chuGiai.Dat(deXuat, chon, new ChuGiai("Bán kính phục vụ", Theme.Success, KieuKyHieu.Vung));
                    break;
                case 1:
                    _lblGiaiThich.Text = "Nền hồng: mật độ dân CHƯA có trạm phục vụ (đậm = đông). Trong khu dân cư dùng dân số ước tính, " +
                                         "ngoài khu dân cư rải đều phần dân số thật còn lại của phường/xã.";
                    _chuGiai.Dat(new ChuGiai("< 2 nghìn", Theme.ThangMauDanCu[0], KieuKyHieu.Vung),
                                 new ChuGiai("2–5", Theme.ThangMauDanCu[1], KieuKyHieu.Vung),
                                 new ChuGiai("5–10", Theme.ThangMauDanCu[2], KieuKyHieu.Vung),
                                 new ChuGiai("10–20", Theme.ThangMauDanCu[3], KieuKyHieu.Vung),
                                 new ChuGiai("> 20 nghìn người/km²", Theme.ThangMauDanCu[4], KieuKyHieu.Vung), deXuat);
                    break;
                case 2:
                    _lblGiaiThich.Text = "Bật lớp Công trình. Tia nối từ vị trí đang xem tới các công trình trong bán kính: " +
                                         "cam = chưa có trạm phục vụ (được tính đủ điểm), xám = đã có trạm (tính 30%).";
                    _chuGiai.Dat(new ChuGiai("Công trình chưa có trạm", Theme.MauTieuChi[1], KieuKyHieu.Duong),
                                 new ChuGiai("Đã có trạm", Theme.Disabled, KieuKyHieu.Duong), deXuat, chon);
                    break;
                case 3:
                    _lblGiaiThich.Text = "Vùng xanh nhạt: đã nằm trong bán kính phục vụ của trạm hiện có. " +
                                         "Đoạn đường ngoài vùng này (xa trạm ≥ 400 m) được điểm tối đa.";
                    _chuGiai.Dat(new ChuGiai("Đã có trạm phục vụ", Theme.Accent2, KieuKyHieu.Vung),
                                 new ChuGiai("Trạm hiện có", Theme.MapTram, KieuKyHieu.XeBus), deXuat);
                    break;
                case 4:
                    _lblGiaiThich.Text = "Bật lớp Tuyến xe buýt. Quốc lộ > tỉnh lộ > đường đô thị; đoạn có tuyến buýt đi qua được cộng điểm " +
                                         "(gán trạm mới vào tuyến ngay). Dữ liệu không có lưu lượng xe – dùng cấp đường thay thế.";
                    _chuGiai.Dat(new ChuGiai("Tuyến xe buýt", Theme.MauTieuChi[3], KieuKyHieu.Duong), deXuat);
                    break;
                case 5:
                    _lblGiaiThich.Text = "Chấm đen: nút giao (giao của ≥ 3 đoạn đường trong mạng đường). " +
                                         "Trạm đặt sát nút giao gây ùn tắc và mất an toàn – nên cách ≥ 75 m.";
                    _chuGiai.Dat(new ChuGiai("Nút giao", Theme.Primary, KieuKyHieu.Tron), deXuat);
                    break;
            }
            BoCucHienThi();
            if (cheDo == 2) _bc?.BanDo.HienLop(MapManager.CongTrinh, true);
            if (cheDo == 4) _bc?.BanDo.HienLop(MapManager.Tuyen, true);
            if (veLai && _dangHien) VeBanDo();
        }

        private double GiaTriTheoCheDo(ViTri v) => _cheDo == 0 ? v.Diem / 100 : v.C[_cheDo - 1];

        private void VeBanDo(bool zoom = false)
        {
            if (_bc == null) return;
            var m = _bc.BanDo;
            // lớp ngữ cảnh theo tiêu chí đang xem (nằm dưới)
            if (_cheDo == 1 && _lopDan != null && _lopDan.Rows.Count > 0)
                m.HienLopTam(LopDan, "Dân chưa có trạm phục vụ", ShpfileType.SHP_POLYGON, _lopDan, MapManager.KieuPhanLop(Theme.ThangMauDanCu, 150));
            else m.XoaLopTam(LopDan);
            if (_cheDo == 3 && _lopPv != null && _lopPv.Rows.Count > 0)
                m.HienLopTam(LopPv, "Vùng đã có trạm phục vụ", ShpfileType.SHP_POLYGON, _lopPv, MapManager.KieuVung(Theme.Accent2, 60));
            else m.XoaLopTam(LopPv);
            if (_cheDo == 5 && _lopNut != null && _lopNut.Rows.Count > 0)
                m.HienLopTam(LopNut, "Nút giao", ShpfileType.SHP_POINT, _lopNut, MapManager.KieuDiemTam(Theme.Primary, 9));
            else m.XoaLopTam(LopNut);

            VeVung(_ds.Count > 0 ? _vung : (rdoVe.Checked ? _vungVe : null));

            if (_ds.Count > 0)
            {
                var doan = BangLop();
                var diem = BangLop();
                foreach (var v in _ds)
                {
                    var kieu = LopPhuHop(GiaTriTheoCheDo(v)).ToString(CultureInfo.InvariantCulture);
                    if (v.WktDoan != null) doan.Rows.Add(v.Id.ToString(), kieu, v.WktDoan);
                    else diem.Rows.Add(v.Id.ToString(), kieu, v.WktDiem);
                }
                if (doan.Rows.Count > 0)
                    m.HienLopTam(LopDoan, "Mức phù hợp đặt trạm (đoạn đường)", ShpfileType.SHP_POLYLINE, doan, MapManager.KieuDuongPhanLop(Theme.ThangMauPhuHop, 6));
                else m.XoaLopTam(LopDoan);
                if (diem.Rows.Count > 0)
                    m.HienLopTam(LopDiem, "Mức phù hợp đặt trạm (trong khu dân cư)", ShpfileType.SHP_POINT, diem, MapManager.KieuDiemPhanLop(Theme.ThangMauPhuHop, 11));
                else m.XoaLopTam(LopDiem);
            }
            else
            {
                m.XoaLopTam(LopDoan);
                m.XoaLopTam(LopDiem);
            }

            // vị trí đang xem (vòng + tia vẽ lại khi chọn) và đề xuất trên cùng
            if (_chon != null && _ds.Contains(_chon))
            {
                DuaLenTren(LopVong, LopTia);
                m.HienLopTam(LopChon, "Vị trí đang xem", ShpfileType.SHP_POINT, BangWkt("C", _chon.WktDiem), MapManager.KieuDiemTam(Theme.MapChon, 17));
            }
            else foreach (var l in new[] { LopChon, LopVong, LopTia }) m.XoaLopTam(l);
            if (_top.Count > 0)
            {
                var t = new DataTable();
                t.Columns.Add("ma");
                t.Columns.Add("wkt");
                foreach (var v in _top) t.Rows.Add(v.Hang.ToString(), v.WktDiem);
                m.HienLopTam(LopTop, "Vị trí đề xuất", ShpfileType.SHP_POINT, t, MapManager.KieuDiemTam(Theme.Highlight, 15));
                m.DatNhan(LopTop, "[ma]", Theme.Primary);
            }
            else m.XoaLopTam(LopTop);
            DuaLenTren(LopNut, LopTop, LopChon);

            if (zoom)
            {
                var l = m.Lop(LopVung);
                if (l != null) m.Map.ZoomToLayer(l.Handle);
            }
            m.Map.Redraw();
        }

        private static DataTable BangLop()
        {
            var t = new DataTable();
            t.Columns.Add("ma");
            t.Columns.Add("kieu");
            t.Columns.Add("wkt");
            return t;
        }

        private static DataTable BangWkt(string ma, string wkt)
        {
            var t = new DataTable();
            t.Columns.Add("ma");
            t.Columns.Add("wkt");
            t.Rows.Add(ma, wkt);
            return t;
        }

        // =====================================================================
        // Tạo trạm đề xuất
        // =====================================================================
        private async Task TaoTramAsync()
        {
            var v = _chon;
            if (v == null || !Session.Co(Quyen.DxChonViTri)) return;
            var ten = _txtTen.Text.Trim();
            if (ten.Length == 0)
            {
                _lblKetQua.ForeColor = Theme.Danger;
                _lblKetQua.Text = "Tên trạm không được để trống.";
                _txtTen.Focus();
                return;
            }
            if (!UiHelper.XacNhan(FindForm(), $"Tạo trạm \"{ten}\" (trạng thái \"Đề xuất\") tại vị trí {v.Diem:0} điểm trên {v.TenViTri}?")) return;
            string maMoi = null;
            double r = _banKinh;
            if (!await UiHelper.ChayAsync(_btnTao, async () => maMoi = await Task.Run(() =>
                    DeXuatRepository.TaoTramDeXuat(v.Lon, v.Lat, ten, "TD", null, Session.IdNguoiDung,
                                                   Session.TenNguoiDung ?? Session.TenDangNhap, r)),
                    "Đang tạo…"))
                return;
            _lblKetQua.ForeColor = Theme.Success;
            _lblKetQua.Text = $"Đã tạo {maMoi} (Đề xuất)." + (v.Tuyen != null ? $" Có thể gán vào tuyến {v.Tuyen} ở màn hình Tuyến." : "") +
                              " Bấm \"Phân tích khu vực\" để chấm lại với trạm mới.";
            UiHelper.TipCho(_lblKetQua, _lblKetQua.Text);
            UiHelper.ThanhCong(FindForm(), $"Đã tạo trạm đề xuất {maMoi}.");
            await _bc.BanDo.NapLaiAsync(MapManager.Tram);
            _bc.TrangThai($"Đã tạo trạm đề xuất {maMoi} – \"{ten}\".", false);
        }
    }
}
