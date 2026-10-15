namespace XeBus_GIS.Forms
{
    partial class UcTongQuan
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            this.tlpChinh = new System.Windows.Forms.TableLayoutPanel();
            this.pnlDau = new System.Windows.Forms.Panel();
            this.lblMoTa = new System.Windows.Forms.Label();
            this.lblCapNhat = new System.Windows.Forms.Label();
            this.btnLamMoi = new System.Windows.Forms.Button();
            this.tlpKpi = new System.Windows.Forms.TableLayoutPanel();
            this.kpiTram = new XeBus_GIS.UI.Controls.TheKpi();
            this.kpiHoatDong = new XeBus_GIS.UI.Controls.TheKpi();
            this.kpiDeXuat = new XeBus_GIS.UI.Controls.TheKpi();
            this.kpiTuyen = new XeBus_GIS.UI.Controls.TheKpi();
            this.kpiPhuong = new XeBus_GIS.UI.Controls.TheKpi();
            this.kpiPhucVu = new XeBus_GIS.UI.Controls.TheKpi();
            this.tlpGiua = new System.Windows.Forms.TableLayoutPanel();
            this.pnlBieuDo = new System.Windows.Forms.Panel();
            this.chartPhuong = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lblBieuDoPhu = new System.Windows.Forms.Label();
            this.lblBieuDo = new System.Windows.Forms.Label();
            this.pnlChuY = new System.Windows.Forms.Panel();
            this.dgvChuY = new System.Windows.Forms.DataGridView();
            this.pnlChuYNut = new System.Windows.Forms.Panel();
            this.btnPhanTich = new System.Windows.Forms.Button();
            this.lblChuYPhu = new System.Windows.Forms.Label();
            this.lblChuY = new System.Windows.Forms.Label();
            this.tlpDuoi = new System.Windows.Forms.TableLayoutPanel();
            this.pnlGanDay = new System.Windows.Forms.Panel();
            this.dgvGanDay = new System.Windows.Forms.DataGridView();
            this.lblGanDayPhu = new System.Windows.Forms.Label();
            this.lblGanDay = new System.Windows.Forms.Label();
            this.pnlCongTrinh = new System.Windows.Forms.Panel();
            this.chartCongTrinh = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lblCongTrinhPhu = new System.Windows.Forms.Label();
            this.lblCongTrinh = new System.Windows.Forms.Label();
            this.tlpChinh.SuspendLayout();
            this.pnlDau.SuspendLayout();
            this.tlpKpi.SuspendLayout();
            this.tlpGiua.SuspendLayout();
            this.pnlBieuDo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartPhuong)).BeginInit();
            this.pnlChuY.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChuY)).BeginInit();
            this.pnlChuYNut.SuspendLayout();
            this.tlpDuoi.SuspendLayout();
            this.pnlGanDay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGanDay)).BeginInit();
            this.pnlCongTrinh.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartCongTrinh)).BeginInit();
            this.SuspendLayout();
            //
            // tlpChinh
            //
            this.tlpChinh.ColumnCount = 1;
            this.tlpChinh.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpChinh.Controls.Add(this.pnlDau, 0, 0);
            this.tlpChinh.Controls.Add(this.tlpKpi, 0, 1);
            this.tlpChinh.Controls.Add(this.tlpGiua, 0, 2);
            this.tlpChinh.Controls.Add(this.tlpDuoi, 0, 3);
            this.tlpChinh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpChinh.Name = "tlpChinh";
            this.tlpChinh.Padding = new System.Windows.Forms.Padding(20, 12, 20, 20);
            this.tlpChinh.RowCount = 4;
            this.tlpChinh.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tlpChinh.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.tlpChinh.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tlpChinh.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tlpChinh.Size = new System.Drawing.Size(1140, 700);
            this.tlpChinh.TabIndex = 0;
            //
            // pnlDau
            //
            this.pnlDau.Controls.Add(this.lblMoTa);
            this.pnlDau.Controls.Add(this.lblCapNhat);
            this.pnlDau.Controls.Add(this.btnLamMoi);
            this.pnlDau.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDau.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.pnlDau.Name = "pnlDau";
            //
            // lblMoTa
            //
            this.lblMoTa.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMoTa.Name = "lblMoTa";
            this.lblMoTa.Text = "Tình hình mạng lưới trạm xe buýt Hà Nội – số liệu đọc trực tiếp từ CSDLKG, bán kính phục vụ 500 m.";
            this.lblMoTa.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblCapNhat
            //
            this.lblCapNhat.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblCapNhat.Name = "lblCapNhat";
            this.lblCapNhat.Size = new System.Drawing.Size(200, 44);
            this.lblCapNhat.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // btnLamMoi
            //
            this.btnLamMoi.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnLamMoi.Name = "btnLamMoi";
            this.btnLamMoi.Size = new System.Drawing.Size(100, 44);
            this.btnLamMoi.TabIndex = 0;
            this.btnLamMoi.Text = "Làm mới";
            //
            // tlpKpi
            //
            this.tlpKpi.ColumnCount = 6;
            this.tlpKpi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66F));
            this.tlpKpi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66F));
            this.tlpKpi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66F));
            this.tlpKpi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66F));
            this.tlpKpi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66F));
            this.tlpKpi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.7F));
            this.tlpKpi.Controls.Add(this.kpiTram, 0, 0);
            this.tlpKpi.Controls.Add(this.kpiHoatDong, 1, 0);
            this.tlpKpi.Controls.Add(this.kpiDeXuat, 2, 0);
            this.tlpKpi.Controls.Add(this.kpiTuyen, 3, 0);
            this.tlpKpi.Controls.Add(this.kpiPhuong, 4, 0);
            this.tlpKpi.Controls.Add(this.kpiPhucVu, 5, 0);
            this.tlpKpi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpKpi.Margin = new System.Windows.Forms.Padding(0);
            this.tlpKpi.Name = "tlpKpi";
            this.tlpKpi.RowCount = 1;
            this.tlpKpi.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            //
            // kpi cards
            //
            this.kpiTram.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiTram.Margin = new System.Windows.Forms.Padding(4, 4, 4, 12);
            this.kpiTram.Name = "kpiTram";
            this.kpiTram.Nhan = "Tổng số trạm";
            this.kpiHoatDong.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiHoatDong.Margin = new System.Windows.Forms.Padding(4, 4, 4, 12);
            this.kpiHoatDong.Name = "kpiHoatDong";
            this.kpiHoatDong.Nhan = "Trạm đang hoạt động";
            this.kpiDeXuat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiDeXuat.Margin = new System.Windows.Forms.Padding(4, 4, 4, 12);
            this.kpiDeXuat.Name = "kpiDeXuat";
            this.kpiDeXuat.Nhan = "Trạm đề xuất";
            this.kpiTuyen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiTuyen.Margin = new System.Windows.Forms.Padding(4, 4, 4, 12);
            this.kpiTuyen.Name = "kpiTuyen";
            this.kpiTuyen.Nhan = "Tuyến đang hoạt động";
            this.kpiPhuong.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiPhuong.Margin = new System.Windows.Forms.Padding(4, 4, 4, 12);
            this.kpiPhuong.Name = "kpiPhuong";
            this.kpiPhuong.Nhan = "Phường/xã chưa có trạm";
            this.kpiPhucVu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiPhucVu.Margin = new System.Windows.Forms.Padding(4, 4, 4, 12);
            this.kpiPhucVu.Name = "kpiPhucVu";
            this.kpiPhucVu.Nhan = "Dân KDC trong 500 m";
            //
            // tlpGiua
            //
            this.tlpGiua.ColumnCount = 2;
            this.tlpGiua.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tlpGiua.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tlpGiua.Controls.Add(this.pnlBieuDo, 0, 0);
            this.tlpGiua.Controls.Add(this.pnlChuY, 1, 0);
            this.tlpGiua.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGiua.Margin = new System.Windows.Forms.Padding(0);
            this.tlpGiua.Name = "tlpGiua";
            this.tlpGiua.RowCount = 1;
            this.tlpGiua.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            //
            // pnlBieuDo
            //
            this.pnlBieuDo.Controls.Add(this.chartPhuong);
            this.pnlBieuDo.Controls.Add(this.lblBieuDoPhu);
            this.pnlBieuDo.Controls.Add(this.lblBieuDo);
            this.pnlBieuDo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBieuDo.Margin = new System.Windows.Forms.Padding(4, 4, 6, 6);
            this.pnlBieuDo.Name = "pnlBieuDo";
            this.pnlBieuDo.Padding = new System.Windows.Forms.Padding(16, 12, 16, 8);
            //
            // chartPhuong
            //
            chartArea1.Name = "ChartArea1";
            this.chartPhuong.ChartAreas.Add(chartArea1);
            this.chartPhuong.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartPhuong.Name = "chartPhuong";
            //
            // lblBieuDoPhu
            //
            this.lblBieuDoPhu.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblBieuDoPhu.Name = "lblBieuDoPhu";
            this.lblBieuDoPhu.Size = new System.Drawing.Size(300, 22);
            this.lblBieuDoPhu.Text = "12 phường/xã có nhiều trạm nhất · đơn vị: trạm (không tính trạm ngừng hoạt động)";
            //
            // lblBieuDo
            //
            this.lblBieuDo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblBieuDo.Name = "lblBieuDo";
            this.lblBieuDo.Size = new System.Drawing.Size(300, 26);
            this.lblBieuDo.Text = "Số trạm theo phường/xã";
            //
            // pnlChuY
            //
            this.pnlChuY.Controls.Add(this.dgvChuY);
            this.pnlChuY.Controls.Add(this.pnlChuYNut);
            this.pnlChuY.Controls.Add(this.lblChuYPhu);
            this.pnlChuY.Controls.Add(this.lblChuY);
            this.pnlChuY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlChuY.Margin = new System.Windows.Forms.Padding(6, 4, 4, 6);
            this.pnlChuY.Name = "pnlChuY";
            this.pnlChuY.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            //
            // dgvChuY
            //
            this.dgvChuY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvChuY.Name = "dgvChuY";
            this.dgvChuY.TabIndex = 1;
            //
            // pnlChuYNut
            //
            this.pnlChuYNut.Controls.Add(this.btnPhanTich);
            this.pnlChuYNut.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlChuYNut.Name = "pnlChuYNut";
            this.pnlChuYNut.Size = new System.Drawing.Size(300, 44);
            //
            // btnPhanTich
            //
            this.btnPhanTich.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPhanTich.Location = new System.Drawing.Point(80, 10);
            this.btnPhanTich.Name = "btnPhanTich";
            this.btnPhanTich.Size = new System.Drawing.Size(220, 34);
            this.btnPhanTich.TabIndex = 2;
            this.btnPhanTich.Text = "Phân tích khu vực thiếu trạm";
            //
            // lblChuYPhu
            //
            this.lblChuYPhu.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblChuYPhu.Name = "lblChuYPhu";
            this.lblChuYPhu.Size = new System.Drawing.Size(300, 30);
            this.lblChuYPhu.Text = "Chưa có trạm, hoặc dưới 30% dân khu dân cư có trạm trong 500 m. Double-click để xem trên bản đồ.";
            //
            // lblChuY
            //
            this.lblChuY.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblChuY.Name = "lblChuY";
            this.lblChuY.Size = new System.Drawing.Size(300, 26);
            this.lblChuY.Text = "Phường/xã cần chú ý";
            //
            // tlpDuoi
            //
            this.tlpDuoi.ColumnCount = 2;
            this.tlpDuoi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tlpDuoi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tlpDuoi.Controls.Add(this.pnlGanDay, 0, 0);
            this.tlpDuoi.Controls.Add(this.pnlCongTrinh, 1, 0);
            this.tlpDuoi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDuoi.Margin = new System.Windows.Forms.Padding(0);
            this.tlpDuoi.Name = "tlpDuoi";
            this.tlpDuoi.RowCount = 1;
            this.tlpDuoi.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            //
            // pnlGanDay
            //
            this.pnlGanDay.Controls.Add(this.dgvGanDay);
            this.pnlGanDay.Controls.Add(this.lblGanDayPhu);
            this.pnlGanDay.Controls.Add(this.lblGanDay);
            this.pnlGanDay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGanDay.Margin = new System.Windows.Forms.Padding(4, 6, 6, 4);
            this.pnlGanDay.Name = "pnlGanDay";
            this.pnlGanDay.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            //
            // dgvGanDay
            //
            this.dgvGanDay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvGanDay.Name = "dgvGanDay";
            this.dgvGanDay.TabIndex = 3;
            //
            // lblGanDayPhu
            //
            this.lblGanDayPhu.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblGanDayPhu.Name = "lblGanDayPhu";
            this.lblGanDayPhu.Size = new System.Drawing.Size(300, 24);
            this.lblGanDayPhu.Text = "8 trạm được tạo hoặc sửa gần nhất. Double-click để xem trên bản đồ.";
            //
            // lblGanDay
            //
            this.lblGanDay.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblGanDay.Name = "lblGanDay";
            this.lblGanDay.Size = new System.Drawing.Size(300, 26);
            this.lblGanDay.Text = "Trạm cập nhật gần đây";
            //
            // pnlCongTrinh
            //
            this.pnlCongTrinh.Controls.Add(this.chartCongTrinh);
            this.pnlCongTrinh.Controls.Add(this.lblCongTrinhPhu);
            this.pnlCongTrinh.Controls.Add(this.lblCongTrinh);
            this.pnlCongTrinh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCongTrinh.Margin = new System.Windows.Forms.Padding(6, 6, 4, 4);
            this.pnlCongTrinh.Name = "pnlCongTrinh";
            this.pnlCongTrinh.Padding = new System.Windows.Forms.Padding(16, 12, 16, 8);
            //
            // chartCongTrinh
            //
            chartArea2.Name = "ChartArea1";
            this.chartCongTrinh.ChartAreas.Add(chartArea2);
            this.chartCongTrinh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartCongTrinh.Name = "chartCongTrinh";
            //
            // lblCongTrinhPhu
            //
            this.lblCongTrinhPhu.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCongTrinhPhu.Name = "lblCongTrinhPhu";
            this.lblCongTrinhPhu.Size = new System.Drawing.Size(300, 22);
            this.lblCongTrinhPhu.Text = "% công trình có trạm trong 500 m, theo loại";
            //
            // lblCongTrinh
            //
            this.lblCongTrinh.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCongTrinh.Name = "lblCongTrinh";
            this.lblCongTrinh.Size = new System.Drawing.Size(300, 26);
            this.lblCongTrinh.Text = "Công trình được phục vụ";
            //
            // UcTongQuan
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;   // co giãn DPI bằng UiHelper.CoGianDpi (xem ctor)
            this.AutoScroll = true;
            this.AutoScrollMinSize = new System.Drawing.Size(960, 860);
            this.Controls.Add(this.tlpChinh);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Name = "UcTongQuan";
            this.Size = new System.Drawing.Size(1140, 700);
            this.tlpChinh.ResumeLayout(false);
            this.pnlDau.ResumeLayout(false);
            this.tlpKpi.ResumeLayout(false);
            this.tlpGiua.ResumeLayout(false);
            this.pnlBieuDo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartPhuong)).EndInit();
            this.pnlChuY.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvChuY)).EndInit();
            this.pnlChuYNut.ResumeLayout(false);
            this.tlpDuoi.ResumeLayout(false);
            this.pnlGanDay.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvGanDay)).EndInit();
            this.pnlCongTrinh.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartCongTrinh)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpChinh;
        private System.Windows.Forms.Panel pnlDau;
        private System.Windows.Forms.Label lblMoTa;
        private System.Windows.Forms.Label lblCapNhat;
        private System.Windows.Forms.Button btnLamMoi;
        private System.Windows.Forms.TableLayoutPanel tlpKpi;
        private XeBus_GIS.UI.Controls.TheKpi kpiTram;
        private XeBus_GIS.UI.Controls.TheKpi kpiHoatDong;
        private XeBus_GIS.UI.Controls.TheKpi kpiDeXuat;
        private XeBus_GIS.UI.Controls.TheKpi kpiTuyen;
        private XeBus_GIS.UI.Controls.TheKpi kpiPhuong;
        private XeBus_GIS.UI.Controls.TheKpi kpiPhucVu;
        private System.Windows.Forms.TableLayoutPanel tlpGiua;
        private System.Windows.Forms.Panel pnlBieuDo;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartPhuong;
        private System.Windows.Forms.Label lblBieuDoPhu;
        private System.Windows.Forms.Label lblBieuDo;
        private System.Windows.Forms.Panel pnlChuY;
        private System.Windows.Forms.DataGridView dgvChuY;
        private System.Windows.Forms.Panel pnlChuYNut;
        private System.Windows.Forms.Button btnPhanTich;
        private System.Windows.Forms.Label lblChuYPhu;
        private System.Windows.Forms.Label lblChuY;
        private System.Windows.Forms.TableLayoutPanel tlpDuoi;
        private System.Windows.Forms.Panel pnlGanDay;
        private System.Windows.Forms.DataGridView dgvGanDay;
        private System.Windows.Forms.Label lblGanDayPhu;
        private System.Windows.Forms.Label lblGanDay;
        private System.Windows.Forms.Panel pnlCongTrinh;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartCongTrinh;
        private System.Windows.Forms.Label lblCongTrinhPhu;
        private System.Windows.Forms.Label lblCongTrinh;
    }
}
