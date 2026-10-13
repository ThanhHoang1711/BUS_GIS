using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using XeBus_GIS.Data;
using XeBus_GIS.UI;

namespace XeBus_GIS.Forms
{
    /// <summary>Quy trình 2.4 – Cập nhật thông tin thuộc tính của tuyến (tên, mô tả, trạng thái).</summary>
    public partial class FrmCapNhatTuyen : Form
    {
        private readonly string _ma;

        /// <summary>Dùng cho VS Designer.</summary>
        public FrmCapNhatTuyen()
        {
            InitializeComponent();
        }

        public FrmCapNhatTuyen(DataRow tuyen) : this()
        {
            _ma = Db.Str(tuyen, "matuyen");
            BackColor = Theme.Surface;
            lblTieuDe.Font = Theme.FontTieuDe;
            lblTieuDe.ForeColor = Theme.Primary;
            foreach (var l in new[] { lblMa, lblTrangThai, lblTen, lblMoTa })
                UiHelper.NhanTruong(l);
            lblLoiTen.ForeColor = Theme.Danger;
            lblLoiTen.Font = Theme.FontNhan;
            lblThongTin.ForeColor = Theme.Muted;
            lblThongTin.Font = Theme.FontNhan;
            txtMa.Font = Theme.FontSo;
            txtMa.BackColor = Theme.SurfaceAlt;
            UiHelper.NutChinh(btnLuu);
            UiHelper.NutPhu(btnHuy);

            cboTrangThai.Items.AddRange(TuyenRepository.TrangThaiTuyen);
            txtMa.Text = _ma;
            txtTen.Text = Db.Str(tuyen, "tentuyen");
            txtMoTa.Text = Db.Str(tuyen, "mota");
            cboTrangThai.SelectedItem = Db.Str(tuyen, "trangthai");
            if (cboTrangThai.SelectedIndex < 0) cboTrangThai.SelectedIndex = 0;
            lblThongTin.Text = $"Tạo ngày {Db.Str(tuyen, "ngaytao")} bởi {Db.Str(tuyen, "tenntao") ?? "—"}" +
                               (Db.Str(tuyen, "ngaycapnhat") != null ? $" · cập nhật lần cuối {Db.Str(tuyen, "ngaycapnhat")}" : "");

            txtTen.TextChanged += (s, e) => lblLoiTen.Visible = false;
            btnLuu.Click += async (s, e) => await LuuAsync();
        }

        private async Task LuuAsync()
        {
            var ten = txtTen.Text.Trim();
            if (ten.Length == 0)
            {
                lblLoiTen.Text = "Tên tuyến không được để trống.";
                lblLoiTen.Visible = true;
                txtTen.Focus();
                return;
            }
            string moTa = txtMoTa.Text, tt = (string)cboTrangThai.SelectedItem;
            if (!await UiHelper.ChayAsync(btnLuu, () => Task.Run(() => TuyenRepository.CapNhat(_ma, ten, moTa, tt)), "Đang lưu…"))
                return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
