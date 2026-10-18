using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using XeBus_GIS.Data;
using XeBus_GIS.UI;
using XeBus_GIS.UI.DangNhap;

namespace XeBus_GIS.Forms
{
    /// <summary>
    /// Đăng nhập (B13) – ghi lịch sử đăng nhập (B14) – và đăng ký tài khoản chờ duyệt. Giao diện là trang WPF
    /// <see cref="TrangDangNhap"/> (video nền Blender + form có hiệu ứng); form này chỉ cấp nghiệp vụ cho trang.
    /// DialogResult.OK khi đăng nhập thành công (tài khoản hoặc khách).
    /// </summary>
    public partial class FrmDangNhap : Form
    {
        private readonly TrangDangNhap _trang;
        private bool _daXong;

        public FrmDangNhap()
        {
            InitializeComponent();
            UI.UiHelper.CoGianDpi(this);   // co giãn theo DPI ngay – trước khi code dựng thêm control bằng UiHelper.Dp()
            BackColor = Theme.Surface;
            ThemeManager.TieuDeToi(this);

            _trang = new TrangDangNhap();
            host.BackColorTransparent = false;
            host.BackColor = Theme.Surface;
            host.Child = _trang;
            _trang.NapVideo(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "nen_dangnhap.mp4"));

            // nghiệp vụ cho các form WPF
            var dn = _trang.DangNhap;
            dn.TenDangNhap = CaiDatNguoiDung.Doc(CaiDatNguoiDung.KhoaTenDangNhap) ?? "";
            dn.GhiNho = dn.TenDangNhap.Length > 0;
            dn.DangNhap = (ten, mk) => Task.Run(() => NguoiDungRepository.DangNhap(ten, mk));
            dn.VaoKhach = () => Task.Run(() => { NguoiDungRepository.DangNhapKhach(); return (string)null; });
            dn.DaDangNhap += (ten, ghiNho) =>
            {
                if (ten != null) CaiDatNguoiDung.Ghi(CaiDatNguoiDung.KhoaTenDangNhap, ghiNho ? ten : null);
                _daXong = true;
                _trang.Thoat(() => { DialogResult = DialogResult.OK; Close(); });
            };
            var dk = _trang.DangKy;
            dk.TenDaCo = ten => Task.Run(() => QuanTriRepository.TenDangNhapDaCo(ten));
            dk.DangKy = (ten, hoTen, donVi, mk) => Task.Run(() => NguoiDungRepository.DangKy(ten, hoTen, donVi, mk));

            Resize += (s, e) => _trang.TamDung(WindowState == FormWindowState.Minimized);
            Shown += async (s, e) => await KiemTraKetNoiAsync();
            FormClosed += (s, e) => { try { _trang.Dung(); } catch { } };
            FormClosing += (s, e) => { if (!_daXong && DialogResult != DialogResult.OK) DialogResult = DialogResult.Cancel; };
        }

        /// <summary>Trang WPF (dùng cho bộ tự kiểm tra giao diện).</summary>
        internal TrangDangNhap Trang => _trang;

        private async Task KiemTraKetNoiAsync()
        {
            _trang.DatKetNoi(0, "Đang kết nối CSDL CSDLKG…");
            string loi = null;
            bool ok = await Task.Run(() => Db.KiemTraKetNoi(out loi));
            _trang.DatKetNoi(ok ? 1 : 2, ok ? "Đã kết nối CSDL CSDLKG" : "Không kết nối được CSDL");
            if (!ok)
            {
                _trang.DangNhap.BaoLoi("Không kết nối được CSDL: " + loi);
                return;
            }
            await NapSoLieuAsync();
        }

        /// <summary>Số liệu thật của mạng lưới (chỉ đọc) – đếm lên ở cột giới thiệu.</summary>
        private async Task NapSoLieuAsync()
        {
            try
            {
                var dt = await Db.QueryAsync(@"SELECT (SELECT count(*) FROM xebus.trambuyt WHERE trangthai <> 'Ngừng hoạt động') AS tram,
                                                      (SELECT count(*) FROM xebus.tuyenbuyt) AS tuyen,
                                                      (SELECT count(*) FROM xebus.phuongxa) AS phuong");
                var r = dt.Rows[0];
                _trang.DatSoLieu(Convert.ToInt64(r["tram"]), Convert.ToInt64(r["tuyen"]), Convert.ToInt64(r["phuong"]));
            }
            catch { /* không có số liệu thì giữ dấu "–" */ }
        }
    }
}
