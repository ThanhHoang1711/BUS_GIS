using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using XeBus_GIS.Map;
using XeBus_GIS.UI;

namespace XeBus_GIS.Forms
{
    /// <summary>
    /// Chọn vị trí thủ công cho header (không cần dịch vụ định vị): chọn phường/xã (điểm đại diện từ CSDL cục bộ)
    /// hoặc nhập toạ độ WGS 84.
    /// </summary>
    public partial class FrmChonViTri : Form
    {
        public enum CheDo { PhuongXa, ToaDo }

        private readonly CheDo _cheDo;

        /// <summary>Kết quả: mã phường (chế độ PhuongXa).</summary>
        public string MaPhuong { get; private set; }
        /// <summary>Kết quả: toạ độ (chế độ ToaDo).</summary>
        public double ViDo { get; private set; }
        public double KinhDo { get; private set; }

        /// <summary>Dùng cho VS Designer.</summary>
        public FrmChonViTri()
        {
            InitializeComponent();
        }

        public FrmChonViTri(CheDo cheDo, DataTable phuong, ViTriNguoiDung hienTai) : this()
        {
            _cheDo = cheDo;
            BackColor = Theme.Surface;
            ForeColor = Theme.Text;
            UiHelper.TieuDe(lblTieuDe);
            UiHelper.ChuPhu(lblMoTa);
            UiHelper.NhanTruong(lblViDo);
            UiHelper.NhanTruong(lblKinhDo);
            lblLoi.ForeColor = Theme.Danger;
            lblLoi.Font = Theme.FontNhan;
            UiHelper.NutChinh(btnDongY);
            UiHelper.NutPhu(btnHuy);
            UiHelper.O(cboPhuong, txtViDo, txtKinhDo);
            ThemeManager.TieuDeToi(this);

            bool laPhuong = cheDo == CheDo.PhuongXa;
            cboPhuong.Visible = laPhuong;
            foreach (Control c in new Control[] { lblViDo, txtViDo, lblKinhDo, txtKinhDo }) c.Visible = !laPhuong;
            if (laPhuong)
            {
                lblTieuDe.Text = "Chọn phường/xã của bạn";
                lblMoTa.Text = "Dùng điểm đại diện của phường/xã làm vị trí (lấy từ CSDL trên máy, không gửi ra ngoài).";
                UcTram.GanCombo(cboPhuong, phuong, null);
                if (hienTai?.MaPhuong != null) cboPhuong.SelectedValue = hienTai.MaPhuong;
                else cboPhuong.SelectedIndex = -1;
            }
            else
            {
                lblTieuDe.Text = "Nhập toạ độ";
                lblMoTa.Text = "Toạ độ WGS 84 dạng số thập phân (dấu chấm). Vị trí chỉ dùng trong ứng dụng.";
                if (hienTai != null)
                {
                    txtViDo.Text = hienTai.ViDo.ToString("0.000000", CultureInfo.InvariantCulture);
                    txtKinhDo.Text = hienTai.KinhDo.ToString("0.000000", CultureInfo.InvariantCulture);
                }
            }
            btnDongY.Click += (s, e) => DongY();
        }

        private void DongY()
        {
            lblLoi.Text = "";
            if (_cheDo == CheDo.PhuongXa)
            {
                MaPhuong = cboPhuong.SelectedValue as string;
                if (string.IsNullOrEmpty(MaPhuong)) { lblLoi.Text = "Hãy chọn một phường/xã trong danh sách."; cboPhuong.Focus(); return; }
            }
            else
            {
                bool ok1 = double.TryParse(txtViDo.Text.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double vi);
                bool ok2 = double.TryParse(txtKinhDo.Text.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double kinh);
                if (!ok1 || !ok2) { lblLoi.Text = "Vĩ độ / kinh độ phải là số (vd 21.0285 và 105.8542)."; return; }
                if (!DinhVi.TrongVietNam(vi, kinh)) { lblLoi.Text = "Toạ độ nằm ngoài phạm vi Việt Nam – kiểm tra lại (vĩ độ 8–24, kinh độ 102–110)."; return; }
                ViDo = vi;
                KinhDo = kinh;
            }
            DialogResult = DialogResult.OK;
        }
    }
}
