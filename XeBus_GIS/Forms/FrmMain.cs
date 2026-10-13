using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AxMapWinGIS;
using XeBus_GIS.Data;
using XeBus_GIS.Map;
using XeBus_GIS.Security;
using XeBus_GIS.UI;
using XeBus_GIS.UI.Controls;

namespace XeBus_GIS.Forms
{
    /// <summary>
    /// Form chính: thanh điều hướng trái | tiêu đề trang (breadcrumb, tìm nhanh, tài khoản) |
    /// thanh công cụ bản đồ | module trái ⇔ bản đồ ⇔ panel phải (Thông tin / Lớp &amp; chú giải) | thanh trạng thái.
    /// Màn hình không cần bản đồ (Tổng quan, Quản trị) hiển thị toàn trang.
    /// </summary>
    public partial class FrmMain : Form
    {
        private AxMap _axMap;
        private MapManager _mm;
        private MapTools _tools;
        private BoiCanh _boiCanh;

        private readonly Dictionary<string, Control> _modules = new Dictionary<string, Control>();
        private string _moduleHienTai;
        private bool _anPanelPhai;
        private bool _tabThongTin = true;

        /// <summary>Thông tin hiển thị của mỗi màn hình.</summary>
        private class TrangInfo
        {
            public string Ten, Duong, Icon, Nhom;
            public bool ToanTrang;   // không có bản đồ
            public bool CoModule;    // có panel module bên trái
        }

        private static readonly Dictionary<string, TrangInfo> Trang = new Dictionary<string, TrangInfo>
        {
            ["TONGQUAN"] = new TrangInfo { Ten = "Tổng quan", Duong = "Hệ thống", Icon = Icons.TongQuan, Nhom = "Chung", ToanTrang = true },
            ["BANDO"] = new TrangInfo { Ten = "Bản đồ GIS", Duong = "Chung", Icon = Icons.BanDo, Nhom = null },
            ["TRAM"] = new TrangInfo { Ten = "Quản lý trạm xe buýt", Duong = "Quản lý", Icon = Icons.Tram, Nhom = "Quản lý", CoModule = true },
            ["TUYEN"] = new TrangInfo { Ten = "Quản lý tuyến xe buýt", Duong = "Quản lý", Icon = Icons.Tuyen, Nhom = null, CoModule = true },
            ["PHANTICH"] = new TrangInfo { Ten = "Phân tích khu vực thiếu trạm", Duong = "Phân tích", Icon = Icons.PhanTich, Nhom = "Phân tích", CoModule = true },
            ["DEXUAT"] = new TrangInfo { Ten = "Đề xuất vị trí trạm mới", Duong = "Phân tích", Icon = Icons.DeXuat, Nhom = null, CoModule = true },
            ["THONGKE"] = new TrangInfo { Ten = "Thống kê và báo cáo", Duong = "Phân tích", Icon = Icons.ThongKe, Nhom = null, CoModule = true },
            ["QUANTRI"] = new TrangInfo { Ten = "Quản trị hệ thống", Duong = "Hệ thống", Icon = Icons.QuanTri, Nhom = "Hệ thống", ToanTrang = true },
        };

        private static readonly Dictionary<string, string> TenMenu = new Dictionary<string, string>
        {
            ["TONGQUAN"] = "Tổng quan",
            ["BANDO"] = "Bản đồ GIS",
            ["TRAM"] = "Trạm xe buýt",
            ["TUYEN"] = "Tuyến xe buýt",
            ["PHANTICH"] = "Khu vực thiếu trạm",
            ["DEXUAT"] = "Đề xuất trạm mới",
            ["THONGKE"] = "Thống kê, báo cáo",
            ["QUANTRI"] = "Quản trị",
        };

        /// <summary>true nếu người dùng chọn Đăng xuất (Program sẽ mở lại form đăng nhập).</summary>
        public bool DaDangXuat { get; private set; }

        public FrmMain()
        {
            InitializeComponent();
            TaoBanDo();
            ApDungGiaoDien();
            TaoMenu();
            TaoDanhSachLop();
            GanSuKien();
        }

        // =====================================================================
        // Khởi tạo
        // =====================================================================
        private void TaoBanDo()
        {
            _axMap = new AxMap();
            ((ISupportInitialize)_axMap).BeginInit();
            _axMap.Dock = DockStyle.Fill;
            _axMap.Name = "axMap";
            pnlMap.Controls.Add(_axMap);
            ((ISupportInitialize)_axMap).EndInit();
        }

        private void ApDungGiaoDien()
        {
            BackColor = Theme.Background;
            pnlNoiDung.BackColor = Theme.Background;
            pnlTrang.BackColor = Theme.Background;

            // Tiêu đề trang
            pnlPageHeader.BackColor = Theme.Surface;
            pnlPageHeader.Paint += VeDuongDay;
            pnlPageHeader.Padding = new Padding(pnlPageHeader.Padding.Left, 0, pnlPageHeader.Padding.Right, 1);
            lblTrang.Font = Theme.FontTrang;
            lblTrang.ForeColor = Theme.Primary;
            lblBreadcrumb.Font = Theme.FontNhan;
            lblBreadcrumb.ForeColor = Theme.Muted;
            pnlHeaderPhai.BackColor = Theme.Surface;

            // Ô tìm nhanh
            pnlTim.BackColor = Theme.SurfaceAlt;
            pnlTim.Paint += (s, e) =>
            {
                using (var pen = new Pen(txtTimNhanh.Focused ? Theme.Accent : Theme.BorderStrong))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlTim.Width - 1, pnlTim.Height - 1);
            };
            txtTimNhanh.BackColor = Theme.SurfaceAlt;
            txtTimNhanh.Font = Theme.FontThuong;
            txtTimNhanh.ForeColor = Theme.Text;
            lblIconTim.Text = Icons.TimKiem;
            lblIconTim.Font = Theme.FontIcon(10f);
            lblIconTim.ForeColor = Theme.Muted;
            DatGoiYTim(true);
            UiHelper.TipCho(txtTimNhanh, "Tìm trạm, tuyến, phường/xã theo tên hoặc mã (Ctrl+F)");
            lstTimNhanh.Font = Theme.FontThuong;
            lstTimNhanh.ItemHeight = UiHelper.Dp(44);

            // Nút tài khoản
            btnUser.FlatStyle = FlatStyle.Flat;
            btnUser.FlatAppearance.BorderSize = 0;
            btnUser.FlatAppearance.MouseOverBackColor = Theme.SurfaceHover;
            btnUser.FlatAppearance.MouseDownBackColor = Theme.Border;
            btnUser.BackColor = Theme.Surface;
            btnUser.Cursor = Cursors.Hand;
            btnUser.Text = "";
            btnUser.Paint += VeNutUser;
            mnuUser.Font = Theme.FontThuong;
            mnuDangXuat.Image = null;

            // Thanh công cụ bản đồ
            pnlToolbar.BackColor = Theme.Surface;
            pnlToolbar.Paint += VeDuongDay;
            flpCongCu.BackColor = Theme.Surface;
            flpCongCu.Width = UiHelper.Dp(450);
            UiHelper.NutIcon(btnChon, Icons.Identify, "Chọn đối tượng – xem thông tin");
            UiHelper.NutIcon(btnKeo, Icons.Pan, "Kéo bản đồ");
            UiHelper.NutIcon(btnPhongTo, Icons.ZoomIn, "Phóng to (click hoặc kéo khung)");
            UiHelper.NutIcon(btnThuNho, Icons.ZoomOut, "Thu nhỏ");
            UiHelper.NutIcon(btnToanCanh, Icons.FullExtent, "Toàn cảnh Hà Nội");
            UiHelper.NutIcon(btnDoDai, Icons.DoDai, "Đo khoảng cách – click các điểm, double-click để kết thúc");
            UiHelper.NutIcon(btnDoDienTich, Icons.DoDienTich, "Đo diện tích – click các đỉnh, double-click để kết thúc");
            UiHelper.NutIcon(btnXoaChon, Icons.XoaChon, "Bỏ chọn, xoá kết quả trên bản đồ (Esc huỷ thao tác đang vẽ)");
            UiHelper.NutIcon(btnLamMoi, Icons.LamMoi, "Nạp lại dữ liệu bản đồ (F5)");
            UiHelper.NutIcon(btnPanelPhai, Icons.PanelPhai, "Ẩn/hiện panel Thông tin – Lớp bản đồ");
            foreach (var s in new[] { sep1, sep2, sep3 }) s.BackColor = Theme.Border;
            lblGoiY.ForeColor = Theme.Muted;
            lblGoiY.Font = Theme.FontNhan;

            // Panel trái (module) / phải (thông tin, lớp)
            pnlTrai.BackColor = Theme.Surface;
            pnlTrai.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.Border)) e.Graphics.DrawLine(pen, pnlTrai.Width - 1, 0, pnlTrai.Width - 1, pnlTrai.Height);
            };
            splTrai.BackColor = Theme.Background;
            splPhai.BackColor = Theme.Background;
            pnlPhai.BackColor = Theme.Surface;
            pnlPhai.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.Border)) e.Graphics.DrawLine(pen, 0, 0, 0, pnlPhai.Height);
            };
            pnlPhaiTab.BackColor = Theme.Surface;
            pnlPhaiTab.Paint += VeDuongDay;
            foreach (var b in new[] { btnTabThongTin, btnTabLop })
            {
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseOverBackColor = Theme.Surface;
                b.FlatAppearance.MouseDownBackColor = Theme.Surface;
                b.BackColor = Theme.Surface;
                b.Cursor = Cursors.Hand;
                b.Paint += VeGachTab;
            }
            UiHelper.NutIcon(btnThuPhai, Icons.Phai, "Ẩn panel", 9f);
            pnlTabThongTin.BackColor = Theme.Surface;
            pnlTabLop.BackColor = Theme.Surface;
            UiHelper.TieuDeKhuVuc(lblThongTin);
            UiHelper.Luoi(dgvThongTin);
            dgvThongTin.ColumnHeadersVisible = false;
            dgvThongTin.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvThongTin.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvThongTin.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvThongTin.AlternatingRowsDefaultCellStyle.BackColor = Theme.Surface;
            dgvThongTin.DefaultCellStyle.SelectionBackColor = Theme.SurfaceHover;
            UiHelper.TrangThaiRong(dgvThongTin, "Chọn công cụ Chọn đối tượng rồi click vào trạm, tuyến, đường, phường/xã… trên bản đồ để xem thuộc tính.");
            DatTabPhai(true);

            // Thanh trạng thái
            statusStrip.BackColor = Theme.Surface;
            statusStrip.Font = Theme.FontNhan;
            statusStrip.Renderer = new ToolStripProfessionalRenderer(new MauStatusStrip());
            slToaDo.Font = Theme.FontSo;
            slToaDo.Text = "Kinh độ —   Vĩ độ —";
            slToaDo.ForeColor = Theme.Muted;
            slTyLe.ForeColor = Theme.Muted;
            slHeToaDo.ForeColor = Theme.Muted;
            slHeToaDo.ToolTipText = "CSDL lưu EPSG:4326 (WGS 84); bản đồ hiển thị Web Mercator (EPSG:3857); toạ độ con trỏ là kinh/vĩ độ WGS 84";
            slDb.ForeColor = Theme.Muted;
        }

        /// <summary>Đường kẻ mảnh ở đáy các thanh ngang.</summary>
        private static void VeDuongDay(object sender, PaintEventArgs e)
        {
            var c = (Control)sender;
            using (var pen = new Pen(Theme.Border)) e.Graphics.DrawLine(pen, 0, c.Height - 1, c.Width, c.Height - 1);
        }

        /// <summary>Gạch chân màu Accent dưới tab đang chọn của panel phải.</summary>
        private void VeGachTab(object sender, PaintEventArgs e)
        {
            var b = (Button)sender;
            bool chon = (b == btnTabThongTin) == _tabThongTin;
            if (!chon) return;
            using (var br = new SolidBrush(Theme.Accent))
                e.Graphics.FillRectangle(br, UiHelper.Dp(10), b.Height - UiHelper.Dp(3), b.Width - UiHelper.Dp(20), UiHelper.Dp(3));
        }

        private void VeNutUser(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var b = btnUser;
            string ten = Session.TenNguoiDung ?? Session.TenDangNhap ?? "Khách";
            string nhom = Session.TenNhom ?? "";
            int d = UiHelper.Dp(28);
            using (var f = Theme.FontIcon(8f))
            {
                int rongChu = Math.Max(TextRenderer.MeasureText(ten, Theme.FontDam).Width, TextRenderer.MeasureText(nhom, Theme.FontNho).Width);
                int x0 = Math.Max(UiHelper.Dp(4), b.Width - UiHelper.Dp(24) - rongChu - d - UiHelper.Dp(10));
                var rAvatar = new Rectangle(x0, (b.Height - d) / 2, d, d);
                using (var br = new SolidBrush(Theme.AccentSoft)) g.FillEllipse(br, rAvatar);
                string chuDau = ten.Trim().Split(' ').Last().Substring(0, 1).ToUpperInvariant();
                TextRenderer.DrawText(g, chuDau, Theme.FontDam, rAvatar, Theme.Accent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                int xChu = rAvatar.Right + UiHelper.Dp(8);
                int wChu = b.Width - xChu - UiHelper.Dp(24);
                TextRenderer.DrawText(g, ten, Theme.FontDam, new Rectangle(xChu, UiHelper.Dp(1), wChu, b.Height / 2), Theme.Text,
                    TextFormatFlags.Bottom | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, nhom, Theme.FontNho, new Rectangle(xChu, b.Height / 2 + UiHelper.Dp(1), wChu, b.Height / 2), Theme.Muted,
                    TextFormatFlags.Top | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, Icons.MuiTenXuong, f, new Rectangle(b.Width - UiHelper.Dp(22), 0, UiHelper.Dp(18), b.Height), Theme.Muted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
        }

        private void TaoMenu()
        {
            foreach (var kv in Trang)
                navBar.Them(kv.Key, TenMenu[kv.Key], kv.Value.Icon, kv.Value.Nhom);
            // gom nhóm: Bản đồ thuộc nhóm "Chung" cùng Tổng quan
            navBar.CacMuc.First(m => m.Khoa == "BANDO").Nhom = "Chung";
            navBar.CacMuc.First(m => m.Khoa == "TUYEN").Nhom = "Quản lý";
            navBar.CacMuc.First(m => m.Khoa == "DEXUAT").Nhom = "Phân tích";
            navBar.CacMuc.First(m => m.Khoa == "THONGKE").Nhom = "Phân tích";
        }

        /// <summary>Danh sách lớp + chú giải (ký hiệu khớp với MapManager.KhoiTao).</summary>
        private void TaoDanhSachLop()
        {
            const string gt = "Giao thông", dc = "Dân cư & công trình", rg = "Ranh giới hành chính";
            panelLop.Them(MapManager.Tram, "Trạm xe buýt", gt, new ChuGiai("", Theme.MapTram, KieuKyHieu.Tron),
                new ChuGiai("Trạm dừng", Theme.MapTram, KieuKyHieu.Tron),
                new ChuGiai("Bến xe / điểm trung chuyển", Theme.MapBenXe, KieuKyHieu.Vuong),
                new ChuGiai("Trạm đề xuất", Theme.MapTramDeXuat, KieuKyHieu.Tron),
                new ChuGiai("Ngừng hoạt động", Theme.MapTramNgung, KieuKyHieu.Tron));
            panelLop.Them(MapManager.Tuyen, "Tuyến xe buýt", gt, new ChuGiai("", Theme.MauTuyen[0], KieuKyHieu.Duong));
            panelLop.Them(MapManager.DoanDuong, "Đoạn đường", gt, new ChuGiai("", Theme.MapDuong, KieuKyHieu.Duong));
            panelLop.Them(MapManager.CongTrinh, "Công trình", dc, new ChuGiai("", Theme.MapCongTrinh, KieuKyHieu.Thoi),
                new ChuGiai("Y tế", Theme.MapCtYTe, KieuKyHieu.ChuThap),
                new ChuGiai("Giáo dục", Theme.MapCtGiaoDuc, KieuKyHieu.TamGiac),
                new ChuGiai("Thương mại, chợ", Theme.MapCtThuongMai, KieuKyHieu.Thoi),
                new ChuGiai("Khu công nghiệp", Theme.MapCtCongNghiep, KieuKyHieu.Vuong),
                new ChuGiai("Công trình khác", Theme.MapCongTrinh, KieuKyHieu.Thoi));
            panelLop.Them(MapManager.KhuDanCu, "Khu dân cư", dc, new ChuGiai("", Theme.MapKhuDanCu, KieuKyHieu.Vung));
            panelLop.Them(MapManager.PhuongXa, "Phường/xã", rg, new ChuGiai("", Theme.MapPhuongXa, KieuKyHieu.DuongCham));
            panelLop.Them(MapManager.RanhGioi, "Ranh giới Hà Nội", rg, new ChuGiai("", Theme.MapRanhGioi, KieuKyHieu.DuongDut));
            panelLop.Lop(MapManager.CongTrinh).MoChuGiai = false;
            // mặc định ẩn lớp nặng / dày đặc
            panelLop.DatHien(MapManager.KhuDanCu, false);
            panelLop.DatHien(MapManager.CongTrinh, false);
        }

        private void GanSuKien()
        {
            navBar.MucDuocChon += khoa => ChonModule(khoa);
            navBar.DangChuyenDong += dang =>
            {
                // khoá vẽ lại bản đồ trong lúc thanh điều hướng co giãn để chuyển động mượt
                if (_mm == null) return;
                try { _axMap.LockWindow(dang ? MapWinGIS.tkLockMode.lmLock : MapWinGIS.tkLockMode.lmUnlock); } catch { }
            };

            btnUser.Click += (s, e) => mnuUser.Show(btnUser, new Point(btnUser.Width - mnuUser.Width, btnUser.Height + 2));
            mnuDangXuat.Click += (s, e) => DangXuat();

            btnKeo.Click += (s, e) => _tools.Dat(CongCu.Keo);
            btnPhongTo.Click += (s, e) => _tools.Dat(CongCu.PhongTo);
            btnThuNho.Click += (s, e) => _tools.Dat(CongCu.ThuNho);
            btnChon.Click += (s, e) => _tools.Dat(CongCu.ChonDoiTuong);
            btnDoDai.Click += (s, e) => _tools.Dat(CongCu.DoDai);
            btnDoDienTich.Click += (s, e) => _tools.Dat(CongCu.DoDienTich);
            btnToanCanh.Click += (s, e) => _mm.ToanCanh();
            btnXoaChon.Click += (s, e) =>
            {
                _tools.HuyVe();
                _mm.BoChonTatCa();
                _mm.XoaTatCaLopTam();
                dgvThongTin.DataSource = null;
                lblThongTin.Text = "Thông tin đối tượng";
            };
            btnLamMoi.Click += async (s, e) => await UiHelper.ChayAsync(btnLamMoi, NapBanDoAsync);
            btnPanelPhai.Click += (s, e) => DatPanelPhai(!_anPanelPhai);
            btnThuPhai.Click += (s, e) => DatPanelPhai(true);
            btnTabThongTin.Click += (s, e) => DatTabPhai(true);
            btnTabLop.Click += (s, e) => DatTabPhai(false);

            panelLop.LopThayDoi += (khoa, hien) => _mm?.HienLop(khoa, hien);

            // tìm nhanh
            _timerTim.Tick += async (s, e) => { _timerTim.Stop(); await TimNhanhAsync(); };
            txtTimNhanh.TextChanged += (s, e) =>
            {
                if (_dangDatGoiY) return;
                _timerTim.Stop();
                if (txtTimNhanh.Text.Trim().Length >= 2) _timerTim.Start(); else AnGoiY();
            };
            txtTimNhanh.KeyDown += TxtTimNhanh_KeyDown;
            txtTimNhanh.GotFocus += (s, e) => { DatGoiYTim(false); pnlTim.Invalidate(); };
            txtTimNhanh.LostFocus += (s, e) =>
            {
                pnlTim.Invalidate();
                BeginInvoke((Action)(() =>
                {
                    if (!lstTimNhanh.Focused && !txtTimNhanh.Focused) { AnGoiY(); DatGoiYTim(true); }
                }));
            };
            lblIconTim.Click += (s, e) => txtTimNhanh.Focus();
            pnlTim.Click += (s, e) => txtTimNhanh.Focus();
            lstTimNhanh.DrawItem += VeGoiY;
            lstTimNhanh.MouseClick += (s, e) =>
            {
                int i = lstTimNhanh.IndexFromPoint(e.Location);
                if (i >= 0) ChonKetQuaTim((KetQuaTim)lstTimNhanh.Items[i]);
            };
            lstTimNhanh.MouseMove += (s, e) =>
            {
                int i = lstTimNhanh.IndexFromPoint(e.Location);
                if (i >= 0 && i != lstTimNhanh.SelectedIndex) lstTimNhanh.SelectedIndex = i;
            };

            UiHelper.TienTrinh = dang => slProgress.Visible = dang;

            Shown += async (s, e) => await KhoiDongAsync();
            FormClosing += (s, e) =>
            {
                if (!DaDangXuat) NguoiDungRepository.DangXuat();
                UiHelper.TienTrinh = null;
            };
        }

        private async Task KhoiDongAsync()
        {
            PhanQuyenMenu();
            btnUser.Invalidate();
            mnuThongTinTk.Text = (Session.TenNguoiDung ?? Session.TenDangNhap) + " – " + Session.TenNhom;

            // bản đồ phải đang hiển thị khi khởi tạo ActiveX
            _mm = new MapManager(_axMap);
            _mm.KhoiTao();
            _mm.HienThiThayDoi += (khoa, hien) => panelLop.DatHien(khoa, hien);
            _tools = new MapTools(_mm);
            _tools.ToaDoThayDoi += (lon, lat) => slToaDo.Text = $"Kinh độ {lon:0.00000}   Vĩ độ {lat:0.00000}";
            _tools.CongCuThayDoi += CapNhatNutCongCu;
            _tools.ClickChonDoiTuong += async (x, y, tol) => await ChonTaiDiemAsync(x, y, tol);
            _axMap.ExtentsChanged += (s, e) => slTyLe.Text = "Tỷ lệ 1:" + _axMap.CurrentScale.ToString("N0");
            _boiCanh = new BoiCanh(_mm, _tools, HienThongTin, TrangThai) { MoModule = ChonModule };
            _tools.Dat(CongCu.ChonDoiTuong);

            // màn hình laptop: thu gọn menu để dành chỗ cho bản đồ (người dùng mở lại bằng nút / Ctrl+B)
            if (Screen.FromControl(this).WorkingArea.Width < UiHelper.Dp(1600)) navBar.ThuGon = true;
            ChonModule("TONGQUAN");
            await UiHelper.ChayAsync(btnLamMoi, NapBanDoAsync);
        }

        private async Task NapBanDoAsync()
        {
            TrangThai("Đang nạp dữ liệu bản đồ từ CSDLKG…", false);
            panelLop.DatTrangThaiTatCa("Đang tải…");
            slDb.ForeColor = Theme.Muted;
            slDb.Text = "CSDLKG";
            try
            {
                var dangAn = panelLop.CacLop.Where(l => !l.Hien).Select(l => l.Khoa).ToArray();
                await _mm.NapTatCaAsync(dangAn);
            }
            catch
            {
                panelLop.DatTrangThaiTatCa("Lỗi tải");
                slDb.ForeColor = Theme.Danger;
                slDb.Text = "● Mất kết nối CSDLKG";
                throw;
            }

            foreach (var l in panelLop.CacLop)
            {
                var lop = _mm.Lop(l.Khoa);
                panelLop.DatTrangThai(l.Khoa, lop?.Sf == null ? "Lỗi tải" : null, lop?.ChiSo.Count ?? 0);
                panelLop.DatHien(l.Khoa, _mm.LopDangHien(l.Khoa));
            }

            int soTram = _mm.Lop(MapManager.Tram)?.ChiSo.Count ?? 0;
            int soTuyen = _mm.Lop(MapManager.Tuyen)?.ChiSo.Count ?? 0;
            TrangThai($"Đã nạp {soTram:N0} trạm, {soTuyen} tuyến.", false);
            slDb.ForeColor = Theme.Success;
            slDb.Text = "● CSDLKG";
        }

        // =====================================================================
        // Điều hướng / module
        // =====================================================================
        private void PhanQuyenMenu()
        {
            navBar.DatHien("TRAM", Session.Co(Quyen.TramXem));
            navBar.DatHien("TUYEN", Session.Co(Quyen.TuyenXem));
            navBar.DatHien("PHANTICH", Session.Co(Quyen.DxThieuTram));
            navBar.DatHien("DEXUAT", Session.CoNhom("DX_"));
            navBar.DatHien("THONGKE", Session.CoNhom("TK_"));
            navBar.DatHien("QUANTRI", Session.CoNhom("QT_"));
        }

        /// <summary>Chuyển sang màn hình (khoá trong <see cref="Trang"/>). Trả về module (null với Bản đồ).</summary>
        public Control ChonModule(string khoa)
        {
            if (!Trang.TryGetValue(khoa, out var info)) return null;
            var muc = navBar.CacMuc.FirstOrDefault(m => m.Khoa == khoa);
            if (muc != null && !muc.Hien) return null; // không có quyền

            navBar.KhoaDangChon = khoa;
            lblTrang.Text = info.Ten;
            lblBreadcrumb.Text = "XeBus GIS  /  " + info.Duong + "  /  " + TenMenu[khoa];

            if (_moduleHienTai == khoa) return _modules.TryGetValue(khoa, out var htai) ? htai : null;
            if (_moduleHienTai != null && _modules.TryGetValue(_moduleHienTai, out var cu))
            {
                (cu as IModule)?.NgungKichHoat();
                cu.Visible = false;
            }
            _moduleHienTai = khoa;

            SuspendLayout();
            DatCheDoToanTrang(info.ToanTrang);
            pnlTrai.Visible = splTrai.Visible = info.CoModule;

            Control uc = null;
            if (info.ToanTrang || info.CoModule)
            {
                if (!_modules.TryGetValue(khoa, out uc))
                {
                    uc = TaoModule(khoa);
                    uc.Dock = DockStyle.Fill;
                    (info.ToanTrang ? pnlTrang : pnlTrai).Controls.Add(uc);
                    _modules[khoa] = uc;
                    // tiêu đề đã hiển thị ở đầu trang – ẩn tiêu đề riêng của module
                    foreach (var lbl in uc.Controls.Find("lblTieuDe", true)) lbl.Visible = false;
                }
                uc.Visible = true;
                uc.BringToFront();
            }
            ResumeLayout(true);
            (uc as IModule)?.KichHoat();
            return uc;
        }

        private Control TaoModule(string khoa)
        {
            switch (khoa)
            {
                case "TONGQUAN": return new UcTongQuan(_boiCanh);
                case "TRAM": return new UcTram(_boiCanh);
                case "TUYEN": return new UcTuyen(_boiCanh);
                case "PHANTICH": return new UcPhanTich(_boiCanh);
                case "DEXUAT": return new UcDeXuat(_boiCanh);
                case "THONGKE": return new UcThongKe(_boiCanh);
                case "QUANTRI": return new UcQuanTri(_boiCanh);
                default: return new UcDangPhatTrien(khoa, "");
            }
        }

        private MapWinGIS.Extents _phamViLuu;

        /// <summary>Màn hình toàn trang (không bản đồ) hoặc màn hình bản đồ.</summary>
        private void DatCheDoToanTrang(bool toanTrang)
        {
            bool dangCoBanDo = pnlMap.Visible;
            // ẩn bản đồ làm control co về 0 – lưu phạm vi để khôi phục đúng khung nhìn khi quay lại
            if (toanTrang && dangCoBanDo && _mm != null) _phamViLuu = _axMap.Extents as MapWinGIS.Extents;
            if (!toanTrang && !dangCoBanDo && _phamViLuu != null)
            {
                var pv = _phamViLuu;
                BeginInvoke((Action)(() => { try { _axMap.Extents = pv; } catch { } }));
            }
            pnlTrang.Visible = toanTrang;
            pnlToolbar.Visible = !toanTrang;
            pnlMap.Visible = !toanTrang;
            pnlPhai.Visible = splPhai.Visible = !toanTrang && !_anPanelPhai;
            if (toanTrang) { pnlTrai.Visible = splTrai.Visible = false; AnGoiY(); }
            btnPanelPhai.Visible = !toanTrang;
        }

        private void DatPanelPhai(bool an)
        {
            _anPanelPhai = an;
            bool coBanDo = _moduleHienTai == null || !Trang[_moduleHienTai].ToanTrang;
            pnlPhai.Visible = splPhai.Visible = !an && coBanDo;
            UiHelper.DatTrangThaiBat(btnPanelPhai, !an);
        }

        private void DatTabPhai(bool thongTin)
        {
            _tabThongTin = thongTin;
            pnlTabThongTin.Visible = thongTin;
            pnlTabLop.Visible = !thongTin;
            btnTabThongTin.ForeColor = thongTin ? Theme.Accent : Theme.Muted;
            btnTabLop.ForeColor = thongTin ? Theme.Muted : Theme.Accent;
            btnTabThongTin.Font = thongTin ? Theme.FontDam : Theme.FontThuong;
            btnTabLop.Font = thongTin ? Theme.FontThuong : Theme.FontDam;
            btnTabThongTin.Invalidate();
            btnTabLop.Invalidate();
            UiHelper.DatTrangThaiBat(btnPanelPhai, !_anPanelPhai);
        }

        // =====================================================================
        // Phím tắt
        // =====================================================================
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.F:
                    txtTimNhanh.Focus();
                    txtTimNhanh.SelectAll();
                    return true;
                case Keys.Control | Keys.B:
                    navBar.ThuGon = !navBar.ThuGon;
                    return true;
                case Keys.F5:
                    if (_mm != null && pnlMap.Visible && btnLamMoi.Enabled) btnLamMoi.PerformClick();
                    return true;
                case Keys.Escape:
                    if (lstTimNhanh.Visible) { AnGoiY(); return true; }
                    if (_tools != null && pnlMap.Visible && _tools.HienTai != CongCu.ChonDoiTuong && _tools.HienTai != CongCu.Keo)
                    {
                        _tools.Dat(CongCu.ChonDoiTuong);
                        TrangThai("Đã huỷ thao tác trên bản đồ.", false);
                        return true;
                    }
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // =====================================================================
        // Tìm nhanh
        // =====================================================================
        private readonly Timer _timerTim = new Timer { Interval = 300 };
        private int _lanTim;
        private bool _dangDatGoiY;
        private const string GoiYTim = "Tìm trạm, tuyến, phường/xã…";

        private class KetQuaTim
        {
            public string Loai, Ma, Ten, Phu;
        }

        /// <summary>Chữ gợi ý mờ trong ô tìm kiếm khi trống (TextBox WinForms không có placeholder).</summary>
        private void DatGoiYTim(bool hien)
        {
            _dangDatGoiY = true;
            if (hien && string.IsNullOrEmpty(txtTimNhanh.Text))
            {
                txtTimNhanh.Text = GoiYTim;
                txtTimNhanh.ForeColor = Theme.Disabled;
            }
            else if (!hien && txtTimNhanh.Text == GoiYTim && txtTimNhanh.ForeColor == Theme.Disabled)
            {
                txtTimNhanh.Text = "";
                txtTimNhanh.ForeColor = Theme.Text;
            }
            _dangDatGoiY = false;
        }

        private async Task TimNhanhAsync()
        {
            string tu = txtTimNhanh.Text.Trim();
            if (tu.Length < 2 || tu == GoiYTim) return;
            int lan = ++_lanTim;
            try
            {
                var dt = await Task.Run(() => BanDoRepository.TimNhanh(tu));
                if (lan != _lanTim || !txtTimNhanh.Focused) return; // đã gõ tiếp / rời ô tìm
                lstTimNhanh.Items.Clear();
                foreach (DataRow r in dt.Rows)
                    lstTimNhanh.Items.Add(new KetQuaTim { Loai = Db.Str(r, "loai"), Ma = Db.Str(r, "ma"), Ten = Db.Str(r, "ten"), Phu = Db.Str(r, "phu") });
                if (lstTimNhanh.Items.Count == 0)
                    lstTimNhanh.Items.Add(new KetQuaTim { Loai = "", Ten = "Không tìm thấy kết quả", Phu = "Thử tên khác, mã trạm (TR0049) hoặc mã tuyến (M01)" });
                HienGoiY();
            }
            catch (Exception ex)
            {
                TrangThai("Lỗi tìm kiếm: " + ex.GetBaseException().Message, true);
            }
        }

        private void HienGoiY()
        {
            var p = PointToClient(pnlTim.PointToScreen(new Point(0, pnlTim.Height + UiHelper.Dp(4))));
            int rong = Math.Max(pnlTim.Width, UiHelper.Dp(360));
            p.X = Math.Min(p.X, ClientSize.Width - rong - UiHelper.Dp(8));
            lstTimNhanh.Bounds = new Rectangle(p, new Size(rong, Math.Min(8, lstTimNhanh.Items.Count) * lstTimNhanh.ItemHeight + 2));
            lstTimNhanh.Visible = true;
            lstTimNhanh.BringToFront();
            if (lstTimNhanh.Items.Count > 0 && ((KetQuaTim)lstTimNhanh.Items[0]).Loai != "") lstTimNhanh.SelectedIndex = 0;
        }

        private void AnGoiY()
        {
            _lanTim++;
            lstTimNhanh.Visible = false;
        }

        private void TxtTimNhanh_KeyDown(object sender, KeyEventArgs e)
        {
            if (!lstTimNhanh.Visible)
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _timerTim.Stop(); _ = TimNhanhAsync(); }
                return;
            }
            int n = lstTimNhanh.Items.Count;
            switch (e.KeyCode)
            {
                case Keys.Down:
                    lstTimNhanh.SelectedIndex = Math.Min(n - 1, lstTimNhanh.SelectedIndex + 1);
                    e.Handled = true;
                    break;
                case Keys.Up:
                    lstTimNhanh.SelectedIndex = Math.Max(0, lstTimNhanh.SelectedIndex - 1);
                    e.Handled = true;
                    break;
                case Keys.Enter:
                    e.SuppressKeyPress = true;
                    if (lstTimNhanh.SelectedItem is KetQuaTim k) ChonKetQuaTim(k);
                    break;
            }
        }

        private static readonly Dictionary<string, string> LopTheoLoai = new Dictionary<string, string>
        {
            ["TRAM"] = MapManager.Tram,
            ["TUYEN"] = MapManager.Tuyen,
            ["DOAN"] = MapManager.DoanDuong,
            ["CONGTRINH"] = MapManager.CongTrinh,
            ["KDC"] = MapManager.KhuDanCu,
            ["PHUONG"] = MapManager.PhuongXa,
        };

        private void ChonKetQuaTim(KetQuaTim k)
        {
            if (string.IsNullOrEmpty(k.Loai)) return;
            AnGoiY();
            _dangDatGoiY = true;
            txtTimNhanh.Text = k.Ten;
            _dangDatGoiY = false;
            if (_mm == null || !LopTheoLoai.TryGetValue(k.Loai, out var khoaLop)) return;

            // đang ở màn hình không có bản đồ thì chuyển sang Bản đồ GIS
            if (_moduleHienTai != null && Trang[_moduleHienTai].ToanTrang) ChonModule("BANDO");
            if (!_mm.LopDangHien(khoaLop)) _mm.HienLop(khoaLop, true);
            _mm.BoChonTatCa();
            _mm.Chon(khoaLop, new[] { k.Ma }, zoom: true);
            HienThongTin(k.Loai, k.Ma);
            _boiCanh.PhatChon(k.Loai, k.Ma);
            _axMap.Focus();
        }

        private void VeGoiY(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var k = (KetQuaTim)lstTimNhanh.Items[e.Index];
            bool chon = (e.State & DrawItemState.Selected) != 0 && k.Loai != "";
            using (var br = new SolidBrush(chon ? Theme.AccentSoft : Theme.Surface))
                e.Graphics.FillRectangle(br, e.Bounds);
            var r = e.Bounds;
            string icon = k.Loai == "TRAM" ? Icons.Tram : k.Loai == "TUYEN" ? Icons.Tuyen : k.Loai == "PHUONG" ? Icons.BanDo : Icons.TimKiem;
            using (var f = Theme.FontIcon(11f))
                TextRenderer.DrawText(e.Graphics, icon, f, new Rectangle(r.X, r.Y, UiHelper.Dp(40), r.Height),
                    k.Loai == "" ? Theme.Disabled : Theme.Accent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            int x = r.X + UiHelper.Dp(40), w = r.Width - UiHelper.Dp(48);
            TextRenderer.DrawText(e.Graphics, k.Ten, k.Loai == "" ? Theme.FontThuong : Theme.FontDam,
                new Rectangle(x, r.Y + UiHelper.Dp(4), w, r.Height / 2), Theme.Text, TextFormatFlags.Left | TextFormatFlags.Bottom | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(e.Graphics, k.Phu, Theme.FontNho,
                new Rectangle(x, r.Y + r.Height / 2 + UiHelper.Dp(2), w, r.Height / 2 - UiHelper.Dp(4)), Theme.Muted, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        // =====================================================================
        // Bản đồ: chọn đối tượng, thông tin
        // =====================================================================
        private void CapNhatNutCongCu(CongCu cc)
        {
            UiHelper.DatTrangThaiBat(btnKeo, cc == CongCu.Keo);
            UiHelper.DatTrangThaiBat(btnPhongTo, cc == CongCu.PhongTo);
            UiHelper.DatTrangThaiBat(btnThuNho, cc == CongCu.ThuNho);
            UiHelper.DatTrangThaiBat(btnChon, cc == CongCu.ChonDoiTuong);
            UiHelper.DatTrangThaiBat(btnDoDai, cc == CongCu.DoDai);
            UiHelper.DatTrangThaiBat(btnDoDienTich, cc == CongCu.DoDienTich);
            switch (cc)
            {
                case CongCu.ChonDoiTuong: lblGoiY.Text = "Click vào trạm, tuyến, đường, phường/xã… để xem thông tin. Lăn chuột để phóng to/thu nhỏ."; break;
                case CongCu.Keo: lblGoiY.Text = "Giữ chuột trái và kéo để di chuyển bản đồ."; break;
                case CongCu.PhongTo: lblGoiY.Text = "Click hoặc kéo khung để phóng to."; break;
                case CongCu.ThuNho: lblGoiY.Text = "Click để thu nhỏ."; break;
                case CongCu.VeVung: lblGoiY.Text = "Click trái để thêm đỉnh; double-click hoặc click phải để kết thúc vùng. Esc để huỷ."; break;
                case CongCu.ChonDiem: lblGoiY.Text = "Click một điểm trên bản đồ. Esc để huỷ."; break;
                case CongCu.DoDai: lblGoiY.Text = "Đo khoảng cách: click các điểm, double-click để kết thúc. Esc để thoát."; break;
                case CongCu.DoDienTich: lblGoiY.Text = "Đo diện tích: click các đỉnh, double-click để kết thúc. Esc để thoát."; break;
            }
        }

        private async Task ChonTaiDiemAsync(double x, double y, double tol)
        {
            try
            {
                bool coCt = _mm.LopDangHien(MapManager.CongTrinh), coKdc = _mm.LopDangHien(MapManager.KhuDanCu);
                var r = await Task.Run(() => BanDoRepository.TimTaiDiem(x, y, tol, coCt, coKdc));
                _mm.BoChonTatCa();
                if (r == null)
                {
                    dgvThongTin.DataSource = null;
                    lblThongTin.Text = "Thông tin đối tượng";
                    TrangThai("Không có đối tượng tại vị trí click.", false);
                    return;
                }
                string loai = Convert.ToString(r["loai"]), ma = Convert.ToString(r["ma"]);
                if (LopTheoLoai.TryGetValue(loai, out var khoaLop) && _mm.LopDangHien(khoaLop))
                    _mm.Chon(khoaLop, new[] { ma }, zoom: false);
                HienThongTin(loai, ma);
                _boiCanh.PhatChon(loai, ma);
            }
            catch (Exception ex)
            {
                TrangThai("Lỗi tra cứu: " + ex.GetBaseException().Message, true);
            }
        }

        /// <summary>Hiển thị thuộc tính đối tượng ở panel phải (bảng 2 cột Thuộc tính / Giá trị).</summary>
        public async void HienThongTin(string loai, string ma)
        {
            try
            {
                if (_anPanelPhai) DatPanelPhai(false);
                if (!_tabThongTin) DatTabPhai(true);
                lblThongTin.Text = "Đang đọc thông tin…";
                var dt = await Task.Run(() => BanDoRepository.ThongTin(loai, ma));
                var bang = new DataTable();
                bang.Columns.Add("Thuộc tính");
                bang.Columns.Add("Giá trị");
                if (dt.Rows.Count > 0)
                    foreach (DataColumn c in dt.Columns)
                        bang.Rows.Add(c.ColumnName, dt.Rows[0][c] == DBNull.Value ? "—" : Convert.ToString(dt.Rows[0][c]));
                dgvThongTin.DataSource = bang;
                dgvThongTin.Columns[0].FillWeight = 42;
                dgvThongTin.Columns[0].DefaultCellStyle.ForeColor = Theme.Muted;
                dgvThongTin.Columns[0].DefaultCellStyle.Font = Theme.FontNhan;
                dgvThongTin.Columns[1].FillWeight = 58;
                dgvThongTin.Columns[1].DefaultCellStyle.Font = Theme.FontDam;
                lblThongTin.Text = "Thông tin " + TenLoai(loai);
            }
            catch (Exception ex)
            {
                lblThongTin.Text = "Thông tin đối tượng";
                TrangThai("Lỗi đọc thông tin: " + ex.GetBaseException().Message, true);
            }
        }

        private static string TenLoai(string loai)
        {
            switch (loai)
            {
                case "TRAM": return "trạm";
                case "TUYEN": return "tuyến";
                case "DOAN": return "đoạn đường";
                case "CONGTRINH": return "công trình";
                case "KDC": return "khu dân cư";
                case "PHUONG": return "phường/xã";
                default: return "đối tượng";
            }
        }

        public void TrangThai(string noiDung, bool loi)
        {
            slThongBao.Text = noiDung;
            slThongBao.ForeColor = loi ? Theme.Danger : Theme.Text;
        }

        // =====================================================================
        // Đăng xuất
        // =====================================================================
        private void DangXuat()
        {
            if (!UiHelper.XacNhan(this, "Bạn có muốn đăng xuất khỏi hệ thống?")) return;
            NguoiDungRepository.DangXuat();
            DaDangXuat = true;
            Close();
        }

        /// <summary>Màu thanh trạng thái: nền trắng, viền trên mảnh.</summary>
        private class MauStatusStrip : ProfessionalColorTable
        {
            public override Color StatusStripGradientBegin => Theme.Surface;
            public override Color StatusStripGradientEnd => Theme.Surface;
            public override Color ToolStripBorder => Theme.Border;
            public override Color MenuItemSelected => Theme.AccentSoft;
            public override Color MenuItemBorder => Theme.AccentSoft;
        }
    }
}
