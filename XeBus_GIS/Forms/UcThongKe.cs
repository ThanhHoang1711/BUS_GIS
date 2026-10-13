using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using ShpfileType = MapWinGIS.ShpfileType;
using XeBus_GIS.Data;
using XeBus_GIS.Map;
using XeBus_GIS.Security;
using XeBus_GIS.UI;
using XeBus_GIS.UI.Controls;

namespace XeBus_GIS.Forms
{
    /// <summary>
    /// Quy trình 4 – Thống kê trạm xe buýt:
    /// 4.1 Số lượng trạm theo khu vực · 4.2 Trạm theo tuyến · 4.3 Phạm vi phục vụ · 4.4 Xuất file báo cáo.
    /// </summary>
    public partial class UcThongKe : UserControl, IModule
    {
        private const string LopKhuVuc = "TK_KHUVUC";
        private const string LopPhucVu = "TK_PHUCVU";

        // 5 lớp màu số trạm: 0 trạm (đỏ nhạt – vùng trống) → nhiều trạm (xanh đậm); 4 lớp % dân được phục vụ
        private static readonly Color[] MauSoTram = Theme.ThangMauSoTram;
        private static readonly Color[] MauPhucVu = Theme.ThangMauPhucVu;

        private KhungChuGiai _cgKV, _cgPV;

        private readonly BoiCanh _bc;
        private Chart _chartKV, _chartTU, _chartPV;
        private bool _daNap;
        private string _tab = "KV";
        private DataTable _dtPhuong;     // cache theo phường (theo bán kính _bkPhuong)
        private double _bkPhuong;
        private DataRow _tongQuan;

        /// <summary>Dùng cho VS Designer.</summary>
        public UcThongKe()
        {
            InitializeComponent();
        }

        public UcThongKe(BoiCanh boiCanh) : this()
        {
            _bc = boiCanh;
            ApDungGiaoDien();
            PhanQuyen();
            GanSuKien();
        }

        // =====================================================================
        // Giao diện
        // =====================================================================
        private void ApDungGiaoDien()
        {
            BackColor = Theme.Surface;
            foreach (var p in new[] { pnlKV, pnlTU, pnlPV }) p.BackColor = Theme.Surface;
            lblTieuDe.Font = Theme.FontTieuDe;
            lblTieuDe.ForeColor = Theme.Primary;
            foreach (var l in new[] { lblKpi1, lblKpi2, lblKpi3 })
            {
                l.Font = Theme.FontDam;
                l.BackColor = Theme.SurfaceAlt;
                l.ForeColor = Theme.Primary;
            }
            UiHelper.NhanTruong(lblDonVi);
            UiHelper.NhanTruong(lblBanKinh);
            foreach (var b in new[] { btnThongKeKV, btnThongKeTU, btnThongKePV, btnXuat }) UiHelper.NutChinh(b);
            UiHelper.NutPhu(btnChonHet);
            foreach (var b in new[] { btnTabKhuVuc, btnTabTuyen, btnTabPhamVi }) UiHelper.Tab(b);
            UiHelper.GanIcon(btnXuat, Icons.Xuat);
            UiHelper.O(cboDonVi, nudBanKinh);
            UiHelper.NhanTrangThai(dgvTU, "Trạng thái");
            UiHelper.TrangThaiRong(dgvKV, "Chọn đơn vị thống kê rồi bấm Thống kê.");
            UiHelper.TrangThaiRong(dgvTU, "Chọn tuyến rồi bấm Thống kê.");
            UiHelper.TrangThaiRong(dgvPV, "Chọn bán kính phục vụ rồi bấm Thống kê.");
            foreach (var g in new[] { dgvKV, dgvTU, dgvPV }) UiHelper.Luoi(g);
            clbTuyen.BorderStyle = BorderStyle.FixedSingle;
            clbTuyen.Font = Theme.FontNhan;
            clbTuyen.Cursor = Cursors.Hand;
            cboDonVi.Items.AddRange(new object[] { "Phường/xã", "Lưới ô vuông 1 km", "Lưới ô vuông 2 km" });
            cboDonVi.SelectedIndex = 0;

            _chartKV = TaoChart(pnlChartKV, "Số trạm", SeriesChartType.Bar);
            _chartTU = TaoChart(pnlChartTU, "Số trạm", SeriesChartType.Column);
            _chartPV = TaoChart(pnlChartPV, "% có trạm trong bán kính", SeriesChartType.Bar);
            // chú giải thang màu trên bản đồ (hiện sau khi thống kê)
            _cgKV = new KhungChuGiai { Dock = DockStyle.Bottom, Visible = false };
            pnlChartKV.Controls.Add(_cgKV);
            _cgPV = new KhungChuGiai { Dock = DockStyle.Bottom, Visible = false };
            _cgPV.Dat(new ChuGiai("< 25%", MauPhucVu[0], KieuKyHieu.Vung), new ChuGiai("25–50%", MauPhucVu[1], KieuKyHieu.Vung),
                      new ChuGiai("50–75%", MauPhucVu[2], KieuKyHieu.Vung), new ChuGiai("≥ 75%", MauPhucVu[3], KieuKyHieu.Vung),
                      new ChuGiai("Chưa có dữ liệu KDC", Theme.Border, KieuKyHieu.Vung),
                      new ChuGiai("Ranh vùng phục vụ", Theme.Primary, KieuKyHieu.Duong));
            pnlChartPV.Controls.Add(_cgPV);
            HienTab("KV");
        }

        private static Chart TaoChart(Panel cha, string tieuDeTrucY, SeriesChartType kieu)
        {
            var c = new Chart { Dock = DockStyle.Fill, BackColor = Theme.Surface };
            var a = new ChartArea("a") { BackColor = Theme.Surface };
            a.AxisX.MajorGrid.Enabled = false;
            a.AxisX.Interval = 1;
            a.AxisX.LabelStyle.Font = Theme.FontNhan;
            a.AxisX.LabelStyle.ForeColor = Theme.Muted;
            a.AxisX.LineColor = Theme.Border;
            a.AxisY.MajorGrid.LineColor = Theme.Border;
            a.AxisY.LabelStyle.Font = Theme.FontNhan;
            a.AxisY.LabelStyle.ForeColor = Theme.Muted;
            a.AxisY.LineColor = Theme.Border;
            a.AxisY.Title = tieuDeTrucY;
            a.AxisY.TitleFont = Theme.FontNhan;
            a.AxisY.TitleForeColor = Theme.Muted;
            c.ChartAreas.Add(a);
            c.Series.Add(new Series("s") { ChartType = kieu, Color = Theme.Accent, IsValueShownAsLabel = true, Font = Theme.FontNho, LabelForeColor = Theme.Secondary });
            c.Series[0]["PointWidth"] = "0.65";
            UiHelper.BieuDo(c);
            a.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            cha.Controls.Add(c);
            return c;
        }

        private void PhanQuyen()
        {
            btnTabKhuVuc.Visible = Session.Co(Quyen.TkKhuVuc);
            btnTabTuyen.Visible = Session.Co(Quyen.TkTuyen);
            btnTabPhamVi.Visible = Session.Co(Quyen.TkPhamVi);
            pnlNut.Visible = Session.Co(Quyen.TkXuatFile);
        }

        private void HienTab(string tab)
        {
            _tab = tab;
            pnlKV.Visible = tab == "KV";
            pnlTU.Visible = tab == "TU";
            pnlPV.Visible = tab == "PV";
            (tab == "KV" ? pnlKV : tab == "TU" ? pnlTU : pnlPV).BringToFront();
            foreach (var kv in new[] { (btnTabKhuVuc, "KV"), (btnTabTuyen, "TU"), (btnTabPhamVi, "PV") })
                UiHelper.ChonTab(kv.Item1, kv.Item2 == tab);
        }

        private void GanSuKien()
        {
            btnTabKhuVuc.Click += (s, e) => HienTab("KV");
            btnTabTuyen.Click += (s, e) => HienTab("TU");
            btnTabPhamVi.Click += (s, e) => HienTab("PV");
            btnThongKeKV.Click += async (s, e) => await ThongKeKhuVucAsync();
            btnThongKeTU.Click += async (s, e) => await ThongKeTuyenAsync();
            btnThongKePV.Click += async (s, e) => await ThongKePhamViAsync();
            btnChonHet.Click += (s, e) =>
            {
                bool het = clbTuyen.CheckedItems.Count < clbTuyen.Items.Count;
                for (int i = 0; i < clbTuyen.Items.Count; i++) clbTuyen.SetItemChecked(i, het);
                btnChonHet.Text = het ? "Bỏ chọn" : "Chọn tất cả";
            };
            btnXuat.Click += async (s, e) => await XuatBaoCaoAsync();
            dgvKV.SelectionChanged += (s, e) =>
            {
                if (!dgvKV.Focused || dgvKV.CurrentRow == null || !dgvKV.Columns.Contains("ma")) return;
                var ma = Convert.ToString(dgvKV.CurrentRow.Cells["ma"].Value);
                _bc.BanDo.Chon(LopKhuVuc, new[] { ma }, zoom: true);
                if (cboDonVi.SelectedIndex == 0) _bc.HienThongTin("PHUONG", ma);
            };
            dgvPV.SelectionChanged += (s, e) =>
            {
                if (!dgvPV.Focused || dgvPV.CurrentRow == null || !dgvPV.Columns.Contains("ma")) return;
                var ma = Convert.ToString(dgvPV.CurrentRow.Cells["ma"].Value);
                _bc.BanDo.Chon(LopKhuVuc, new[] { ma }, zoom: true);
                _bc.HienThongTin("PHUONG", ma);
            };
            dgvTU.SelectionChanged += (s, e) =>
            {
                if (!dgvTU.Focused || dgvTU.CurrentRow == null || !dgvTU.Columns.Contains("Mã trạm")) return;
                var ma = Convert.ToString(dgvTU.CurrentRow.Cells["Mã trạm"].Value);
                _bc.BanDo.ZoomToi(MapManager.Tram, ma);
                _bc.HienThongTin("TRAM", ma);
            };
        }

        // =====================================================================
        // IModule
        // =====================================================================
        public async void KichHoat()
        {
            if (_daNap) return;
            _daNap = true;
            try
            {
                DataTable tuyen = null;
                await Task.Run(() =>
                {
                    _tongQuan = ThongKeRepository.TongQuan(500);
                    tuyen = ThongKeRepository.DanhSachTuyen();
                });
                HienKpiChung();
                clbTuyen.Items.Clear();
                foreach (DataRow r in tuyen.Rows)
                    clbTuyen.Items.Add(new MucTuyen(Convert.ToString(r["ma"]), Convert.ToString(r["ten"])), true);
                btnChonHet.Text = "Bỏ chọn";
            }
            catch (Exception ex)
            {
                _bc.TrangThai("Lỗi nạp thống kê: " + ex.GetBaseException().Message, true);
            }
        }

        public void NgungKichHoat()
        {
            _bc.BanDo.XoaLopTam(LopKhuVuc);
            _bc.BanDo.XoaLopTam(LopPhucVu);
        }

        private sealed class MucTuyen
        {
            public string Ma { get; }
            private readonly string _ten;
            public MucTuyen(string ma, string ten) { Ma = ma; _ten = ten; }
            public override string ToString() => _ten;
        }

        private void HienKpiChung()
        {
            if (_tongQuan == null) return;
            Kpi(lblKpi1, Convert.ToInt32(_tongQuan["so_tram"]).ToString("N0"), "trạm đang khai thác");
            Kpi(lblKpi2, Convert.ToInt32(_tongQuan["so_tuyen"]).ToString("N0"), "tuyến hoạt động");
            Kpi(lblKpi3, PhanTram(_tongQuan["tyle_pv"]), "dân KDC trong 500 m");
        }

        private static void Kpi(Label l, string so, string moTa, Color? mau = null)
        {
            l.Text = so + "\n" + moTa;
            l.ForeColor = mau ?? Theme.Primary;
        }

        private static string PhanTram(object v) => v == null || v == DBNull.Value ? "—" : $"{Convert.ToDouble(v):0.#}%";

        // =====================================================================
        // 4.1 Số lượng trạm theo khu vực
        // =====================================================================
        private async Task ThongKeKhuVucAsync()
        {
            int donVi = cboDonVi.SelectedIndex;
            DataTable dt = null;
            if (!await UiHelper.ChayAsync(btnThongKeKV, async () => dt = await Task.Run(() =>
                    donVi == 0 ? LayTheoPhuong(500) : ThongKeRepository.TheoLuoi(donVi == 1 ? 1000 : 2000)), "Đang thống kê…"))
                return;

            // bảng
            var hien = dt.Copy();
            if (donVi == 0)
            {
                foreach (var c in hien.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList())
                    if (!new[] { "ma", "wkt", "Phường/xã", "Số trạm", "Trạm/km²", "Dân/trạm", "Dân số" }.Contains(c)) hien.Columns.Remove(c);
            }
            dgvKV.DataSource = hien;
            foreach (var c in new[] { "ma", "wkt" }) if (dgvKV.Columns.Contains(c)) dgvKV.Columns[c].Visible = donVi != 0 && c == "ma";
            if (dgvKV.Columns.Contains("ma")) dgvKV.Columns["ma"].HeaderText = "Ô lưới";
            if (dgvKV.Columns.Contains("Phường/xã")) dgvKV.Columns["Phường/xã"].FillWeight = 100;
            UiHelper.CotVuaDu(dgvKV, "Dân số", "Số trạm", "Trạm/km²", "Dân/trạm");
            if (dgvKV.Columns.Contains("Trạm/km²")) dgvKV.Columns["Trạm/km²"].HeaderText = "/km²";
            if (dgvKV.Columns.Contains("Dân số")) dgvKV.Columns["Dân số"].DefaultCellStyle.Format = "N0";
            if (dgvKV.Columns.Contains("Dân/trạm")) dgvKV.Columns["Dân/trạm"].DefaultCellStyle.Format = "N0";
            UiHelper.CanPhaiCotSo(dgvKV);
            dgvKV.ClearSelection();

            // biểu đồ
            var s = _chartKV.Series[0];
            s.Points.Clear();
            s.Color = Theme.Accent;
            if (donVi == 0)
            {
                _chartKV.ChartAreas[0].AxisY.Title = "Số trạm – 10 phường/xã nhiều trạm nhất";
                foreach (var r in dt.Rows.Cast<DataRow>().Take(10).Reverse())
                    s.Points.AddXY(RutGon(Convert.ToString(r["Phường/xã"])), Convert.ToInt32(r["Số trạm"]));
            }
            else
            {
                _chartKV.ChartAreas[0].AxisY.Title = "Số ô lưới theo số trạm";
                var nhom = new[] { ("0 trạm", 0, 0), ("1", 1, 1), ("2–3", 2, 3), ("4–7", 4, 7), ("≥ 8", 8, int.MaxValue) };
                foreach (var g in nhom.Reverse())
                    s.Points.AddXY(g.Item1, dt.Rows.Cast<DataRow>().Count(r => { int n = Convert.ToInt32(r["Số trạm"]); return n >= g.Item2 && n <= g.Item3; }));
            }
            _chartKV.ChartAreas[0].RecalculateAxesScale();

            // bản đồ: tô màu phân lớp theo số trạm
            int[] nguong = donVi == 0 ? new[] { 1, 6, 16, 31 } : new[] { 1, 2, 4, 8 };
            var lop = BangPhanLop(dt, r => PhanLop(Convert.ToInt32(r["Số trạm"]), nguong));
            _bc.BanDo.XoaLopTam(LopPhucVu);
            _bc.BanDo.HienLopTam(LopKhuVuc, "Thống kê số trạm", ShpfileType.SHP_POLYGON, lop, MapManager.KieuPhanLop(MauSoTram, 150));
            _bc.BanDo.Map.ZoomToLayer(_bc.BanDo.Lop(LopKhuVuc).Handle);

            var nhanLop = donVi == 0 ? new[] { "0 trạm", "1–5", "6–15", "16–30", "> 30" } : new[] { "0 trạm", "1", "2–3", "4–7", "≥ 8" };
            _cgKV.Dat(nhanLop.Select((n, i) => new ChuGiai(n, MauSoTram[i], KieuKyHieu.Vung)).ToArray());
            _cgKV.Visible = true;

            int khong = dt.Rows.Cast<DataRow>().Count(r => Convert.ToInt32(r["Số trạm"]) == 0);
            var tenDv = donVi == 0 ? "phường/xã" : "ô lưới";
            Kpi(lblKpi1, dt.Rows.Count.ToString("N0"), tenDv);
            Kpi(lblKpi2, khong.ToString("N0"), tenDv + " không có trạm", khong > 0 ? Theme.Danger : Theme.Success);
            Kpi(lblKpi3, dt.Rows.Cast<DataRow>().Sum(r => Convert.ToInt32(r["Số trạm"])).ToString("N0"), "trạm (hoạt động)");
            _bc.TrangThai($"Thống kê theo {tenDv}: đỏ nhạt = không có trạm, xanh càng đậm càng nhiều trạm "
                          + (donVi == 0 ? "(0 · 1–5 · 6–15 · 16–30 · >30)." : "(0 · 1 · 2–3 · 4–7 · ≥8)."), false);
        }

        private DataTable LayTheoPhuong(double banKinh)
        {
            if (_dtPhuong == null || Math.Abs(_bkPhuong - banKinh) > 0.1)
            {
                _dtPhuong = ThongKeRepository.TheoPhuong(banKinh);
                _bkPhuong = banKinh;
            }
            return _dtPhuong;
        }

        private static int PhanLop(int giaTri, int[] nguong)
        {
            int lop = 0;
            foreach (var n in nguong) if (giaTri >= n) lop++;
            return lop;
        }

        private static DataTable BangPhanLop(DataTable dt, Func<DataRow, int?> layLop)
        {
            var t = new DataTable();
            t.Columns.Add("ma");
            t.Columns.Add("kieu");
            t.Columns.Add("wkt");
            foreach (DataRow r in dt.Rows)
            {
                var l = layLop(r);
                t.Rows.Add(Convert.ToString(r["ma"]), l.HasValue ? l.Value.ToString() : "x", Convert.ToString(r["wkt"]));
            }
            return t;
        }

        private static string RutGon(string tenPhuong) =>
            tenPhuong.Replace("Phường ", "P. ").Replace("Xã ", "X. ");

        // =====================================================================
        // 4.2 Trạm theo tuyến
        // =====================================================================
        private async Task ThongKeTuyenAsync()
        {
            var ma = clbTuyen.CheckedItems.Cast<MucTuyen>().Select(m => m.Ma).ToArray();
            if (ma.Length == 0) { UiHelper.CanhBao(FindForm(), "Chọn ít nhất một tuyến."); return; }
            DataTable tom = null, tram = null;
            if (!await UiHelper.ChayAsync(btnThongKeTU, () => Task.Run(() =>
                {
                    tom = ThongKeRepository.TomTatTuyen(ma);
                    tram = ThongKeRepository.TramTheoTuyen(ma);
                }), "Đang thống kê…"))
                return;

            var s = _chartTU.Series[0];
            s.Points.Clear();
            foreach (DataRow r in tom.Rows)
            {
                int i = s.Points.AddXY(Convert.ToString(r["Mã tuyến"]), Convert.ToInt32(r["Số trạm"]));
                s.Points[i].Color = Theme.MauTuyen[i % Theme.MauTuyen.Length];
                s.Points[i].ToolTip = $"{r["Tên tuyến"]}: {r["Số trạm"]} trạm, {r["Chiều dài (km)"]} km, cự ly TB {r["Cự ly TB giữa trạm (m)"]} m";
            }
            _chartTU.ChartAreas[0].RecalculateAxesScale();

            dgvTU.DataSource = tram;
            foreach (var c in new[] { "Loại", "Phường/xã" }) if (dgvTU.Columns.Contains(c)) dgvTU.Columns[c].Visible = false;
            if (dgvTU.Columns.Contains("Tuyến")) dgvTU.Columns["Tuyến"].FillWeight = 20;
            if (dgvTU.Columns.Contains("#")) dgvTU.Columns["#"].FillWeight = 12;
            if (dgvTU.Columns.Contains("Mã trạm")) dgvTU.Columns["Mã trạm"].FillWeight = 24;
            if (dgvTU.Columns.Contains("Tên trạm")) dgvTU.Columns["Tên trạm"].FillWeight = 70;
            if (dgvTU.Columns.Contains("Trạng thái")) dgvTU.Columns["Trạng thái"].Visible = false;
            UiHelper.CanPhaiCotSo(dgvTU);
            dgvTU.ClearSelection();

            int tongTram = tom.Rows.Cast<DataRow>().Sum(r => Convert.ToInt32(r["Số trạm"]));
            int tramKhac = tram.Rows.Cast<DataRow>().Select(r => Convert.ToString(r["Mã trạm"])).Distinct().Count();
            double km = tom.Rows.Cast<DataRow>().Sum(r => r["Chiều dài (km)"] == DBNull.Value ? 0 : Convert.ToDouble(r["Chiều dài (km)"]));
            Kpi(lblKpi1, ma.Length.ToString(), "tuyến được chọn");
            Kpi(lblKpi2, tramKhac.ToString("N0"), $"trạm ({tongTram} lượt dừng)");
            Kpi(lblKpi3, km.ToString("N1"), "km tổng chiều dài");

            _bc.BanDo.XoaLopTam(LopKhuVuc);
            _bc.BanDo.XoaLopTam(LopPhucVu);
            _bc.BanDo.HienLop(MapManager.Tuyen, true);
            _bc.BanDo.Chon(MapManager.Tuyen, ma, zoom: true);
            _bc.BanDo.Chon(MapManager.Tram, tram.Rows.Cast<DataRow>().Select(r => Convert.ToString(r["Mã trạm"])), zoom: false);
            _bc.TrangThai($"Thống kê {ma.Length} tuyến: {tramKhac} trạm phục vụ.", false);
        }

        // =====================================================================
        // 4.3 Phạm vi phục vụ của trạm
        // =====================================================================
        private async Task ThongKePhamViAsync()
        {
            double r = (double)nudBanKinh.Value;
            DataRow tq = null;
            DataTable phuong = null, ct = null;
            DataTable vung = null;
            if (!await UiHelper.ChayAsync(btnThongKePV, () => Task.Run(() =>
                {
                    tq = ThongKeRepository.TongQuan(r);
                    phuong = LayTheoPhuong(r);
                    ct = ThongKeRepository.CongTrinhPhucVu(r);
                    vung = ThongKeRepository.VungPhucVu(r);
                }), "Đang tính phạm vi…"))
                return;

            Kpi(lblKpi1, PhanTram(tq["tyle_pv"]), $"dân KDC trong {r:N0} m", Theme.Accent);
            Kpi(lblKpi2, $"{Convert.ToInt32(tq["so_ct_pv"]) * 100.0 / Math.Max(1, Convert.ToInt32(tq["so_ct"])):0.#}%", "công trình có trạm gần");
            Kpi(lblKpi3, Convert.ToInt32(tq["phuong_khong_tram"]).ToString(), "phường/xã chưa có trạm",
                Convert.ToInt32(tq["phuong_khong_tram"]) > 0 ? Theme.Danger : Theme.Success);

            // biểu đồ % công trình được phục vụ theo loại
            var s = _chartPV.Series[0];
            s.Points.Clear();
            foreach (DataRow x in ct.Rows.Cast<DataRow>().OrderBy(x => Convert.ToDouble(x["% được phục vụ"])))
            {
                int i = s.Points.AddXY(Convert.ToString(x["Loại công trình"]), Convert.ToDouble(x["% được phục vụ"]));
                double v = Convert.ToDouble(x["% được phục vụ"]);
                s.Points[i].Color = v < 50 ? Theme.Danger : v < 70 ? Theme.Warning : Theme.Success;
                s.Points[i].ToolTip = $"{x["Loại công trình"]}: {x["Có trạm trong bán kính"]}/{x["Số công trình"]}";
            }
            _chartPV.ChartAreas[0].AxisY.Maximum = 100;
            _chartPV.ChartAreas[0].RecalculateAxesScale();

            // bảng phường – xếp tỷ lệ thấp lên trước
            var hien = phuong.Copy();
            foreach (var c in hien.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList())
                if (!new[] { "ma", "wkt", "Phường/xã", "Số trạm", "% dân được PV", "Dân KDC (ước tính)", "Công trình", "CT được PV" }.Contains(c))
                    hien.Columns.Remove(c);
            // xếp tỷ lệ thấp lên trước; phường chưa có dữ liệu KDC xuống cuối
            hien.Columns.Add("_k", typeof(double));
            foreach (DataRow x in hien.Rows) x["_k"] = x["% dân được PV"] == DBNull.Value ? 1000.0 : Convert.ToDouble(x["% dân được PV"]);
            hien.DefaultView.Sort = "_k ASC";
            dgvPV.DataSource = hien.DefaultView;
            foreach (var c in new[] { "ma", "wkt", "_k", "Công trình", "CT được PV" }) if (dgvPV.Columns.Contains(c)) dgvPV.Columns[c].Visible = false;
            if (dgvPV.Columns.Contains("Phường/xã")) dgvPV.Columns["Phường/xã"].FillWeight = 100;
            UiHelper.CotVuaDu(dgvPV, "Số trạm", "% dân được PV", "Dân KDC (ước tính)");
            if (dgvPV.Columns.Contains("Dân KDC (ước tính)")) { dgvPV.Columns["Dân KDC (ước tính)"].HeaderText = "Dân KDC"; dgvPV.Columns["Dân KDC (ước tính)"].DefaultCellStyle.Format = "N0"; }
            if (dgvPV.Columns.Contains("% dân được PV")) dgvPV.Columns["% dân được PV"].HeaderText = "% PV";
            UiHelper.CanPhaiCotSo(dgvPV);
            dgvPV.ClearSelection();

            // bản đồ: phường tô theo % dân được phục vụ + vùng phục vụ
            var lop = BangPhanLop(phuong, x =>
            {
                if (x["% dân được PV"] == DBNull.Value) return null;
                double v = Convert.ToDouble(x["% dân được PV"]);
                return v < 25 ? 0 : v < 50 ? 1 : v < 75 ? 2 : 3;
            });
            _bc.BanDo.HienLopTam(LopKhuVuc, "Tỷ lệ dân được phục vụ", ShpfileType.SHP_POLYGON, lop, MapManager.KieuPhanLop(MauPhucVu, 110));
            if (vung != null && vung.Rows.Count > 0)
            {
                _bc.BanDo.HienLopTam(LopPhucVu, $"Vùng phục vụ {r:N0} m", ShpfileType.SHP_POLYGON, vung, sf =>
                {
                    var o = sf.DefaultDrawingOptions;
                    o.FillVisible = false;
                    o.LineColor = Theme.Ole(Theme.Primary);
                    o.LineWidth = 1.5f;
                });
            }
            _cgPV.Visible = true;
            _bc.BanDo.ToanCanh();
            _bc.TrangThai($"Phạm vi {r:N0} m: phường tô đỏ < 25% · cam < 50% · vàng < 75% · xanh ≥ 75% dân KDC có trạm gần; xám = chưa có dữ liệu KDC. Viền đen = vùng phục vụ.", false);
        }

        // =====================================================================
        // 4.4 Xuất file báo cáo
        // =====================================================================
        private async Task XuatBaoCaoAsync()
        {
            if (!Session.Co(Quyen.TkXuatFile)) return;
            string duongDan;
            using (var dlg = new SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = $"BaoCao_TramXeBuyt_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                Title = "Lưu báo cáo thống kê"
            })
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                duongDan = dlg.FileName;
            }

            double r = (double)nudBanKinh.Value;
            var anh = Path.Combine(Path.GetTempPath(), $"xebus_bando_{Guid.NewGuid():N}.png");
            try { _bc.BanDo.ChupAnh(anh); } catch { anh = null; }
            var nguoiLap = (Session.TenNguoiDung ?? Session.TenDangNhap) + " (" + Session.TenNhom + ")";

            bool ok = await UiHelper.ChayAsync(btnXuat, () => Task.Run(() =>
            {
                var tq = ThongKeRepository.TongQuan(r);
                var phuong = LayTheoPhuong(r);
                var dsTuyen = ThongKeRepository.DanhSachTuyen().Rows.Cast<DataRow>().Select(x => Convert.ToString(x["ma"])).ToArray();
                var tongQuan = new List<KeyValuePair<string, string>>
                {
                    Kv("Bán kính phục vụ của trạm", $"{r:N0} m"),
                    Kv("Số trạm đang khai thác", Convert.ToInt32(tq["so_tram"]).ToString("N0")),
                    Kv("Số trạm đề xuất", Convert.ToInt32(tq["so_dexuat"]).ToString("N0")),
                    Kv("Số tuyến hoạt động", Convert.ToInt32(tq["so_tuyen"]).ToString("N0")),
                    Kv("Số phường/xã", phuong.Rows.Count.ToString()),
                    Kv("Phường/xã chưa có trạm", Convert.ToInt32(tq["phuong_khong_tram"]).ToString()),
                    Kv("Dân số các phường/xã", phuong.Rows.Cast<DataRow>().Sum(x => x["Dân số"] == DBNull.Value ? 0L : Convert.ToInt64(x["Dân số"])).ToString("N0")),
                    Kv("Dân số khu dân cư (ước tính)", Convert.ToInt64(tq["tong_danso"]).ToString("N0")),
                    Kv($"Dân KDC có trạm trong {r:N0} m", Convert.ToInt64(tq["danso_pv"]).ToString("N0")),
                    Kv("Tỷ lệ dân KDC được phục vụ", PhanTram(tq["tyle_pv"])),
                    Kv("Số công trình", Convert.ToInt32(tq["so_ct"]).ToString("N0")),
                    Kv($"Công trình có trạm trong {r:N0} m", Convert.ToInt32(tq["so_ct_pv"]).ToString("N0")),
                };
                var bang = new List<KeyValuePair<string, DataTable>>
                {
                    new KeyValuePair<string, DataTable>("Theo phường-xã", phuong),
                    new KeyValuePair<string, DataTable>("Theo tuyến", ThongKeRepository.TomTatTuyen(dsTuyen)),
                    new KeyValuePair<string, DataTable>("Trạm theo tuyến", ThongKeRepository.TramTheoTuyen(dsTuyen)),
                    new KeyValuePair<string, DataTable>("Công trình được phục vụ", ThongKeRepository.CongTrinhPhucVu(r)),
                };
                BaoCaoExcel.Xuat(duongDan, "BÁO CÁO THỐNG KÊ TRẠM XE BUÝT", nguoiLap, tongQuan, bang, anh);
            }), "Đang xuất báo cáo…");

            if (anh != null) try { File.Delete(anh); } catch { /* bỏ qua */ }
            if (!ok) return;
            _bc.TrangThai("Đã xuất báo cáo: " + duongDan, false);
            if (UiHelper.XacNhan(FindForm(), "Đã xuất báo cáo:\n" + duongDan + "\n\nMở file ngay?", "Mở file", "Để sau"))
                try { Process.Start(new ProcessStartInfo(duongDan) { UseShellExecute = true }); } catch (Exception ex) { UiHelper.Loi(FindForm(), ex.Message); }
        }

        private static KeyValuePair<string, string> Kv(string k, string v) => new KeyValuePair<string, string>(k, v);
    }
}
