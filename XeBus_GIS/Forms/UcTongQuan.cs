using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using XeBus_GIS.Data;
using XeBus_GIS.Map;
using XeBus_GIS.Security;
using XeBus_GIS.UI;

namespace XeBus_GIS.Forms
{
    /// <summary>
    /// Màn hình Tổng quan: chỉ tiêu chính của mạng lưới trạm, phường/xã nhiều trạm, phường/xã cần chú ý,
    /// mức phục vụ công trình và các trạm cập nhật gần đây. Mọi số liệu đọc trực tiếp từ CSDL.
    /// </summary>
    public partial class UcTongQuan : UserControl, IModule
    {
        private readonly BoiCanh _bc;
        private DateTime _lanNapCuoi = DateTime.MinValue;
        private bool _dangNap;

        /// <summary>Dùng cho VS Designer.</summary>
        public UcTongQuan()
        {
            InitializeComponent();
            UI.UiHelper.CoGianDpi(this);   // co giãn theo DPI ngay – trước khi code dựng thêm control bằng UiHelper.Dp()
        }

        public UcTongQuan(BoiCanh boiCanh) : this()
        {
            _bc = boiCanh;
            ApDungGiaoDien();
            GanSuKien();
        }

        // =====================================================================
        // Khởi tạo
        // =====================================================================
        private void ApDungGiaoDien()
        {
            BackColor = Theme.Background;
            tlpChinh.BackColor = Theme.Background;
            pnlDau.BackColor = Theme.Background;
            UiHelper.ChuPhu(lblMoTa);
            UiHelper.ChuPhu(lblCapNhat);
            UiHelper.NutPhu(btnLamMoi);
            btnLamMoi.Margin = new Padding(0, 6, 0, 6);

            foreach (var p in new[] { pnlBieuDo, pnlChuY, pnlGanDay, pnlCongTrinh })
            {
                p.BackColor = Theme.Surface;
                UiHelper.VienMong(p);
            }
            foreach (var l in new[] { lblBieuDo, lblChuY, lblGanDay, lblCongTrinh }) UiHelper.TieuDe(l);
            foreach (var l in new[] { lblBieuDo, lblChuY, lblGanDay, lblCongTrinh }) l.AutoSize = false;
            foreach (var l in new[] { lblBieuDoPhu, lblChuYPhu, lblGanDayPhu, lblCongTrinhPhu }) UiHelper.ChuPhu(l);
            pnlChuYNut.BackColor = Theme.Surface;
            UiHelper.NutPhu(btnPhanTich);
            btnPhanTich.Visible = Session.CoNhom("DX_");

            kpiTram.ChuThich = "trong CSDL";
            kpiHoatDong.ChuThich = "trạng thái Hoạt động";
            kpiDeXuat.ChuThich = "chờ xem xét";
            kpiDeXuat.MauNhan = Theme.Highlight;
            kpiTuyen.ChuThich = "trạng thái Hoạt động";
            kpiPhuong.ChuThich = "trên tổng số phường/xã";
            kpiPhucVu.ChuThich = "dân số KDC là ước tính";
            kpiPhucVu.MauNhan = Theme.Accent2;

            foreach (var g in new[] { dgvChuY, dgvGanDay })
            {
                UiHelper.Luoi(g);
                g.BorderStyle = BorderStyle.None;
            }
            UiHelper.NhanTrangThai(dgvGanDay, "Trạng thái");
            UiHelper.TrangThaiRong(dgvChuY, "Đang tải…");
            UiHelper.TrangThaiRong(dgvGanDay, "Đang tải…");

            UiHelper.BieuDo(chartPhuong);
            UiHelper.BieuDo(chartCongTrinh);
        }

        private void GanSuKien()
        {
            btnLamMoi.Click += async (s, e) => await NapAsync(true);
            btnPhanTich.Click += (s, e) => _bc.MoModule?.Invoke("PHANTICH");
            dgvChuY.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                XemTrenBanDo("PHUONG", Convert.ToString(dgvChuY.Rows[e.RowIndex].Cells["ma"].Value));
            };
            dgvGanDay.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                XemTrenBanDo("TRAM", Convert.ToString(dgvGanDay.Rows[e.RowIndex].Cells["Mã"].Value));
            };
            chartPhuong.MouseMove += (s, e) =>
            {
                var hit = chartPhuong.HitTest(e.X, e.Y);
                chartPhuong.Cursor = hit.ChartElementType == ChartElementType.DataPoint ? Cursors.Hand : Cursors.Default;
            };
            chartPhuong.MouseClick += (s, e) =>
            {
                var hit = chartPhuong.HitTest(e.X, e.Y);
                if (hit.ChartElementType == ChartElementType.DataPoint && hit.Object is DataPoint p && p.Tag is string ma)
                    XemTrenBanDo("PHUONG", ma);
            };
        }

        /// <summary>Chuyển sang màn hình Bản đồ, chọn và phóng tới đối tượng.</summary>
        private void XemTrenBanDo(string loai, string ma)
        {
            if (string.IsNullOrEmpty(ma) || _bc.MoModule == null) return;
            _bc.MoModule("BANDO");
            string lop = loai == "TRAM" ? MapManager.Tram : MapManager.PhuongXa;
            // chờ bản đồ hiện lại và khôi phục khung nhìn rồi mới phóng tới đối tượng
            BeginInvoke((Action)(() =>
            {
                if (!_bc.BanDo.LopDangHien(lop)) _bc.BanDo.HienLop(lop, true);
                _bc.BanDo.BoChonTatCa();
                _bc.BanDo.Chon(lop, new[] { ma }, zoom: true);
                _bc.HienThongTin(loai, ma);
            }));
        }

        // =====================================================================
        // IModule
        // =====================================================================
        public async void KichHoat()
        {
            // số liệu trong 2 phút gần nhất thì giữ nguyên (bấm Làm mới để nạp lại ngay)
            if ((DateTime.Now - _lanNapCuoi).TotalMinutes < 2) return;
            await NapAsync(false);
        }

        public void NgungKichHoat() { }

        // =====================================================================
        // Nạp số liệu
        // =====================================================================
        private async Task NapAsync(bool boQuaLoi)
        {
            if (_dangNap) return;
            _dangNap = true;
            lblCapNhat.Text = "Đang tải số liệu…";
            lblCapNhat.ForeColor = Theme.Muted;
            TongQuanRepository.SoLieu s = null;
            bool ok = await UiHelper.ChayAsync(btnLamMoi, async () => s = await Task.Run(() => TongQuanRepository.Lay()), "Đang tải…");
            _dangNap = false;
            if (!ok || s == null)
            {
                lblCapNhat.Text = "Không tải được số liệu – bấm Làm mới để thử lại";
                lblCapNhat.ForeColor = Theme.Danger;
                UiHelper.DatChuRong(dgvChuY, "Không tải được số liệu.");
                UiHelper.DatChuRong(dgvGanDay, "Không tải được số liệu.");
                return;
            }
            _lanNapCuoi = DateTime.Now;
            HienThi(s);
            lblCapNhat.Text = "Cập nhật lúc " + _lanNapCuoi.ToString("HH:mm:ss");
        }

        private void HienThi(TongQuanRepository.SoLieu s)
        {
            var tq = s.TongQuan;
            int hoatDong = tq == null ? 0 : Convert.ToInt32(tq["so_tram"]);
            int deXuat = tq == null ? 0 : Convert.ToInt32(tq["so_dexuat"]);
            int khongTram = tq == null ? 0 : Convert.ToInt32(tq["phuong_khong_tram"]);

            kpiTram.Dat(s.TongSoTram.ToString("N0"));
            kpiHoatDong.Dat(hoatDong.ToString("N0"),
                s.TongSoTram > 0 ? $"{hoatDong * 100.0 / s.TongSoTram:0.#}% tổng số trạm" : null);
            kpiDeXuat.Dat(deXuat.ToString("N0"), deXuat > 0 ? "chờ xem xét" : "chưa có trạm đề xuất");
            kpiTuyen.Dat(tq == null ? null : Convert.ToInt32(tq["so_tuyen"]).ToString("N0"));
            kpiPhuong.Dat(khongTram.ToString("N0"), $"trên {s.SoPhuongXa} phường/xã", khongTram > 0 ? Theme.Danger : Theme.Success);
            kpiPhucVu.Dat(tq == null || tq["tyle_pv"] == DBNull.Value ? null : Convert.ToDouble(tq["tyle_pv"]).ToString("0.#") + "%");

            VeBieuDoPhuong(s.TopPhuong);
            VeBieuDoCongTrinh(s.CongTrinhPhucVu);

            dgvChuY.DataSource = s.PhuongCanChuY;
            if (dgvChuY.Columns.Contains("ma")) dgvChuY.Columns["ma"].Visible = false;
            UiHelper.CanPhaiCotSo(dgvChuY);
            if (dgvChuY.Columns.Contains("Phường/xã")) dgvChuY.Columns["Phường/xã"].FillWeight = 200;
            if (dgvChuY.Columns.Contains("Trạm")) dgvChuY.Columns["Trạm"].FillWeight = 55;
            if (dgvChuY.Columns.Contains("% dân PV")) dgvChuY.Columns["% dân PV"].DefaultCellStyle.Format = "0.#";
            if (dgvChuY.Columns.Contains("Dân chưa PV")) dgvChuY.Columns["Dân chưa PV"].DefaultCellStyle.Format = "N0";
            foreach (DataGridViewRow r in dgvChuY.Rows)
                if (Convert.ToInt32(r.Cells["Trạm"].Value) == 0)
                    r.Cells["Trạm"].Style.ForeColor = Theme.Danger;
            UiHelper.DatChuRong(dgvChuY, "Không có phường/xã nào dưới ngưỡng – mọi phường/xã đều có trạm và ≥ 30% dân KDC được phục vụ.");

            dgvGanDay.DataSource = s.CapNhatGanDay;
            foreach (var cot in new[] { "Mã", "Cập nhật" })
                if (dgvGanDay.Columns.Contains(cot)) dgvGanDay.Columns[cot].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            if (dgvGanDay.Columns.Contains("Trạng thái"))
            {
                dgvGanDay.Columns["Trạng thái"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvGanDay.Columns["Trạng thái"].Width = UiHelper.Dp(118);
            }
            if (dgvGanDay.Columns.Contains("Tên trạm")) dgvGanDay.Columns["Tên trạm"].FillWeight = 190;
            if (dgvGanDay.Columns.Contains("Phường/xã")) dgvGanDay.Columns["Phường/xã"].FillWeight = 120;

            UiHelper.DatChuRong(dgvGanDay, "Chưa có trạm nào.");
        }

        private void VeBieuDoPhuong(DataTable dt)
        {
            chartPhuong.Series.Clear();
            var sr = chartPhuong.Series.Add("Số trạm");
            sr.ChartType = SeriesChartType.Bar;
            sr.Color = Theme.Accent;
            sr["PointWidth"] = "0.65";
            sr.IsValueShownAsLabel = true;
            sr.LabelForeColor = Theme.Secondary;
            sr.Font = Theme.FontNho;
            sr.ToolTip = "#VALX: #VAL trạm";
            // biểu đồ thanh ngang vẽ từ dưới lên – đảo thứ tự để phường nhiều trạm nhất ở trên cùng
            foreach (DataRow r in dt.Rows.Cast<DataRow>().Reverse())
            {
                int i = sr.Points.AddXY(RutGon(Convert.ToString(r["ten"])), Convert.ToInt32(r["so_tram"]));
                sr.Points[i].Tag = Convert.ToString(r["ma"]);
            }
            var a = chartPhuong.ChartAreas[0];
            a.AxisY.Title = "Số trạm";
            a.AxisY.LabelStyle.Format = "N0";
            a.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
        }

        private void VeBieuDoCongTrinh(DataTable dt)
        {
            chartCongTrinh.Series.Clear();
            var sr = chartCongTrinh.Series.Add("% được phục vụ");
            sr.ChartType = SeriesChartType.Bar;
            sr.Color = Theme.Accent2;
            sr["PointWidth"] = "0.65";
            sr.IsValueShownAsLabel = true;
            sr.LabelFormat = "0'%'";
            sr.LabelForeColor = Theme.Secondary;
            sr.Font = Theme.FontNho;
            foreach (DataRow r in dt.Rows.Cast<DataRow>().OrderBy(r => Convert.ToDouble(r["% được phục vụ"])))
            {
                int i = sr.Points.AddXY(Convert.ToString(r["Loại công trình"]), Convert.ToDouble(r["% được phục vụ"]));
                sr.Points[i].ToolTip = $"{r["Loại công trình"]}: {r["Có trạm trong bán kính"]}/{r["Số công trình"]} công trình có trạm trong 500 m";
                // dưới 50% tô màu cảnh báo để dễ nhận ra
                if (Convert.ToDouble(r["% được phục vụ"]) < 50) sr.Points[i].Color = Theme.Warning;
            }
            var a = chartCongTrinh.ChartAreas[0];
            a.AxisY.Minimum = 0;
            a.AxisY.Maximum = 100;
            a.AxisY.Interval = 25;
            a.AxisY.Title = "% công trình có trạm trong 500 m";
            a.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
        }

        /// <summary>Bỏ tiền tố "Phường"/"Xã" để nhãn trục gọn hơn (tên đầy đủ vẫn ở tooltip).</summary>
        private static string RutGon(string ten)
        {
            if (ten == null) return "";
            if (ten.StartsWith("Phường ")) return ten.Substring(7);
            if (ten.StartsWith("Xã ")) return "Xã " + ten.Substring(3);
            return ten;
        }
    }
}
