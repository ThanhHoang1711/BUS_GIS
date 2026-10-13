using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using XeBus_GIS.Data;
using XeBus_GIS.UI;

namespace XeBus_GIS.Forms
{
    /// <summary>
    /// Quy trình 1.4 – Cập nhật thông tin thuộc tính trạm.
    /// DialogResult: OK = đã lưu; Retry = người dùng muốn chọn lại vị trí trên bản đồ (form được mở lại sau đó).
    /// </summary>
    public partial class FrmCapNhatTram : Form
    {
        public string MaTram { get; }

        /// <summary>Dùng cho VS Designer.</summary>
        public FrmCapNhatTram()
        {
            InitializeComponent();
        }

        public FrmCapNhatTram(DataRow tram, DataTable loaiTram, DataTable tuyen, DataTable phuongTien) : this()
        {
            MaTram = Db.Str(tram, "matram");
            ApDungGiaoDien();

            UcTram.GanCombo(cboLoai, loaiTram, null);
            UcTram.GanCombo(cboTuyen, tuyen, "(Không có)");
            UcTram.GanCombo(cboPhuongTien, phuongTien, "(Không có)");
            cboTrangThai.Items.AddRange(TramRepository.TrangThaiTram);

            txtMa.Text = MaTram;
            txtTen.Text = Db.Str(tram, "tentram");
            cboLoai.SelectedValue = Db.Str(tram, "maloaitram") ?? "TD";
            cboTuyen.SelectedValue = Db.Str(tram, "matuyen") ?? "";
            cboPhuongTien.SelectedValue = Db.Str(tram, "maphuongtien") ?? "";
            cboTrangThai.SelectedItem = Db.Str(tram, "trangthai");
            if (cboTrangThai.SelectedIndex < 0) cboTrangThai.SelectedIndex = 0;
            DatViTri(Db.Dbl(tram, "kinhdo") ?? 105.85, Db.Dbl(tram, "vido") ?? 21.03);

            txtTen.TextChanged += (s, e) => AnLoi(lblLoiTen);
            nudViDo.ValueChanged += (s, e) => AnLoi(lblLoiViTri);
            nudKinhDo.ValueChanged += (s, e) => AnLoi(lblLoiViTri);
            btnChonViTri.Click += (s, e) => { DialogResult = DialogResult.Retry; Hide(); };
            btnLuu.Click += async (s, e) => await LuuAsync();
        }

        private void ApDungGiaoDien()
        {
            BackColor = Theme.Surface;
            lblTieuDe.Font = Theme.FontTieuDe;
            lblTieuDe.ForeColor = Theme.Primary;
            foreach (var l in new[] { lblMa, lblTrangThai, lblTen, lblLoai, lblTuyen, lblPhuongTien, lblViDo, lblKinhDo })
                UiHelper.NhanTruong(l);
            UiHelper.TieuDeKhuVuc(lblViTri);
            txtMa.Font = Theme.FontSo;
            txtMa.BackColor = Theme.SurfaceAlt;
            foreach (var l in new[] { lblLoiTen, lblLoiViTri })
            {
                l.ForeColor = Theme.Danger;
                l.Font = Theme.FontNhan;
            }
            UiHelper.NutChinh(btnLuu);
            UiHelper.NutPhu(btnHuy);
            UiHelper.NutPhu(btnChonViTri);
            UiHelper.TipCho(btnChonViTri, "Đóng tạm hộp thoại để click vị trí mới trên bản đồ");
        }

        /// <summary>Đặt toạ độ (WGS84) – gọi khi người dùng chọn lại vị trí trên bản đồ.</summary>
        public void DatViTri(double kinhDo, double viDo)
        {
            nudKinhDo.Value = Kep(nudKinhDo, (decimal)Math.Round(kinhDo, 7));
            nudViDo.Value = Kep(nudViDo, (decimal)Math.Round(viDo, 7));
        }

        private static decimal Kep(NumericUpDown n, decimal v) => Math.Max(n.Minimum, Math.Min(n.Maximum, v));

        private static void BaoLoi(Label l, string noiDung)
        {
            l.Text = noiDung;
            l.Visible = true;
        }

        private static void AnLoi(Label l) => l.Visible = false;

        private async Task LuuAsync()
        {
            // --- kiểm tra hợp lệ ---
            var ten = txtTen.Text.Trim();
            bool hopLe = true;
            if (ten.Length == 0) { BaoLoi(lblLoiTen, "Tên trạm không được để trống."); hopLe = false; }
            else if (ten.Length > 150) { BaoLoi(lblLoiTen, "Tên trạm tối đa 150 ký tự."); hopLe = false; }
            if (cboLoai.SelectedValue == null) { UiHelper.CanhBao(this, "Chọn loại trạm."); hopLe = false; }
            if (!hopLe) { txtTen.Focus(); return; }

            double x = (double)nudKinhDo.Value, y = (double)nudViDo.Value;
            string loai = (string)cboLoai.SelectedValue, tuyen = cboTuyen.SelectedValue as string,
                   pt = cboPhuongTien.SelectedValue as string, tt = (string)cboTrangThai.SelectedItem;

            bool trongHaNoi = true;
            bool ok = await UiHelper.ChayAsync(btnLuu, () => Task.Run(() =>
            {
                trongHaNoi = TramRepository.TrongHaNoi(x, y);
                if (trongHaNoi) TramRepository.CapNhat(MaTram, ten, loai, tuyen, pt, tt, x, y);
            }), "Đang lưu…");
            if (!ok) return;
            if (!trongHaNoi)
            {
                BaoLoi(lblLoiViTri, "Vị trí nằm ngoài ranh giới Hà Nội.");
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
