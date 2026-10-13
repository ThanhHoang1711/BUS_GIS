namespace XeBus_GIS.Forms
{
    partial class UcThongKe
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
            this.lblTieuDe = new System.Windows.Forms.Label();
            this.pnlTab = new System.Windows.Forms.Panel();
            this.btnTabKhuVuc = new System.Windows.Forms.Button();
            this.btnTabTuyen = new System.Windows.Forms.Button();
            this.btnTabPhamVi = new System.Windows.Forms.Button();
            this.pnlKpi = new System.Windows.Forms.Panel();
            this.lblKpi1 = new System.Windows.Forms.Label();
            this.lblKpi2 = new System.Windows.Forms.Label();
            this.lblKpi3 = new System.Windows.Forms.Label();
            this.pnlNoiDung = new System.Windows.Forms.Panel();
            this.pnlKV = new System.Windows.Forms.Panel();
            this.lblDonVi = new System.Windows.Forms.Label();
            this.cboDonVi = new System.Windows.Forms.ComboBox();
            this.btnThongKeKV = new System.Windows.Forms.Button();
            this.pnlChartKV = new System.Windows.Forms.Panel();
            this.dgvKV = new System.Windows.Forms.DataGridView();
            this.pnlTU = new System.Windows.Forms.Panel();
            this.clbTuyen = new System.Windows.Forms.CheckedListBox();
            this.btnThongKeTU = new System.Windows.Forms.Button();
            this.btnChonHet = new System.Windows.Forms.Button();
            this.pnlChartTU = new System.Windows.Forms.Panel();
            this.dgvTU = new System.Windows.Forms.DataGridView();
            this.pnlPV = new System.Windows.Forms.Panel();
            this.lblBanKinh = new System.Windows.Forms.Label();
            this.nudBanKinh = new System.Windows.Forms.NumericUpDown();
            this.btnThongKePV = new System.Windows.Forms.Button();
            this.pnlChartPV = new System.Windows.Forms.Panel();
            this.dgvPV = new System.Windows.Forms.DataGridView();
            this.pnlNut = new System.Windows.Forms.Panel();
            this.btnXuat = new System.Windows.Forms.Button();
            this.pnlTab.SuspendLayout();
            this.pnlKpi.SuspendLayout();
            this.pnlNoiDung.SuspendLayout();
            this.pnlKV.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKV)).BeginInit();
            this.pnlTU.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTU)).BeginInit();
            this.pnlPV.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBanKinh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPV)).BeginInit();
            this.pnlNut.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTieuDe
            //
            this.lblTieuDe.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblTieuDe.Size = new System.Drawing.Size(380, 42);
            this.lblTieuDe.Text = "THỐNG KÊ TRẠM XE BUÝT";
            this.lblTieuDe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlTab
            //
            this.pnlTab.Controls.Add(this.btnTabKhuVuc);
            this.pnlTab.Controls.Add(this.btnTabTuyen);
            this.pnlTab.Controls.Add(this.btnTabPhamVi);
            this.pnlTab.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTab.Name = "pnlTab";
            this.pnlTab.Size = new System.Drawing.Size(380, 42);
            this.btnTabKhuVuc.Location = new System.Drawing.Point(12, 4);
            this.btnTabKhuVuc.Name = "btnTabKhuVuc";
            this.btnTabKhuVuc.Size = new System.Drawing.Size(116, 34);
            this.btnTabKhuVuc.Text = "Theo khu vực";
            this.btnTabTuyen.Location = new System.Drawing.Point(130, 4);
            this.btnTabTuyen.Name = "btnTabTuyen";
            this.btnTabTuyen.Size = new System.Drawing.Size(116, 34);
            this.btnTabTuyen.Text = "Theo tuyến";
            this.btnTabPhamVi.Location = new System.Drawing.Point(248, 4);
            this.btnTabPhamVi.Name = "btnTabPhamVi";
            this.btnTabPhamVi.Size = new System.Drawing.Size(116, 34);
            this.btnTabPhamVi.Text = "Phạm vi phục vụ";
            //
            // pnlKpi
            //
            this.pnlKpi.Controls.Add(this.lblKpi1);
            this.pnlKpi.Controls.Add(this.lblKpi2);
            this.pnlKpi.Controls.Add(this.lblKpi3);
            this.pnlKpi.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlKpi.Name = "pnlKpi";
            this.pnlKpi.Size = new System.Drawing.Size(380, 78);
            this.lblKpi1.Location = new System.Drawing.Point(12, 6);
            this.lblKpi1.Name = "lblKpi1";
            this.lblKpi1.Size = new System.Drawing.Size(114, 66);
            this.lblKpi1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblKpi2.Location = new System.Drawing.Point(132, 6);
            this.lblKpi2.Name = "lblKpi2";
            this.lblKpi2.Size = new System.Drawing.Size(114, 66);
            this.lblKpi2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblKpi3.Location = new System.Drawing.Point(252, 6);
            this.lblKpi3.Name = "lblKpi3";
            this.lblKpi3.Size = new System.Drawing.Size(112, 66);
            this.lblKpi3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // pnlNoiDung
            //
            this.pnlNoiDung.Controls.Add(this.pnlKV);
            this.pnlNoiDung.Controls.Add(this.pnlTU);
            this.pnlNoiDung.Controls.Add(this.pnlPV);
            this.pnlNoiDung.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNoiDung.Name = "pnlNoiDung";
            this.pnlNoiDung.Size = new System.Drawing.Size(380, 490);
            //
            // ===== pnlKV – theo khu vực =====
            //
            this.pnlKV.Controls.Add(this.lblDonVi);
            this.pnlKV.Controls.Add(this.cboDonVi);
            this.pnlKV.Controls.Add(this.btnThongKeKV);
            this.pnlKV.Controls.Add(this.pnlChartKV);
            this.pnlKV.Controls.Add(this.dgvKV);
            this.pnlKV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlKV.Name = "pnlKV";
            this.pnlKV.Size = new System.Drawing.Size(380, 490);
            this.lblDonVi.AutoSize = true;
            this.lblDonVi.Location = new System.Drawing.Point(12, 6);
            this.lblDonVi.Name = "lblDonVi";
            this.lblDonVi.Text = "Đơn vị khu vực";
            this.cboDonVi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDonVi.Location = new System.Drawing.Point(12, 26);
            this.cboDonVi.Name = "cboDonVi";
            this.cboDonVi.Size = new System.Drawing.Size(206, 25);
            this.cboDonVi.TabIndex = 0;
            this.btnThongKeKV.Location = new System.Drawing.Point(226, 23);
            this.btnThongKeKV.Name = "btnThongKeKV";
            this.btnThongKeKV.Size = new System.Drawing.Size(138, 31);
            this.btnThongKeKV.TabIndex = 1;
            this.btnThongKeKV.Text = "Thống kê";
            this.pnlChartKV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlChartKV.Location = new System.Drawing.Point(12, 60);
            this.pnlChartKV.Name = "pnlChartKV";
            this.pnlChartKV.Size = new System.Drawing.Size(356, 190);
            this.dgvKV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvKV.Location = new System.Drawing.Point(12, 256);
            this.dgvKV.Name = "dgvKV";
            this.dgvKV.Size = new System.Drawing.Size(356, 228);
            this.dgvKV.TabIndex = 2;
            //
            // ===== pnlTU – theo tuyến =====
            //
            this.pnlTU.Controls.Add(this.clbTuyen);
            this.pnlTU.Controls.Add(this.btnThongKeTU);
            this.pnlTU.Controls.Add(this.btnChonHet);
            this.pnlTU.Controls.Add(this.pnlChartTU);
            this.pnlTU.Controls.Add(this.dgvTU);
            this.pnlTU.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTU.Name = "pnlTU";
            this.pnlTU.Size = new System.Drawing.Size(380, 490);
            this.clbTuyen.CheckOnClick = true;
            this.clbTuyen.Location = new System.Drawing.Point(12, 6);
            this.clbTuyen.Name = "clbTuyen";
            this.clbTuyen.Size = new System.Drawing.Size(240, 94);
            this.clbTuyen.TabIndex = 0;
            this.btnThongKeTU.Location = new System.Drawing.Point(260, 6);
            this.btnThongKeTU.Name = "btnThongKeTU";
            this.btnThongKeTU.Size = new System.Drawing.Size(104, 40);
            this.btnThongKeTU.TabIndex = 1;
            this.btnThongKeTU.Text = "Thống kê";
            this.btnChonHet.Location = new System.Drawing.Point(260, 52);
            this.btnChonHet.Name = "btnChonHet";
            this.btnChonHet.Size = new System.Drawing.Size(104, 32);
            this.btnChonHet.TabIndex = 2;
            this.btnChonHet.Text = "Chọn tất cả";
            this.pnlChartTU.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlChartTU.Location = new System.Drawing.Point(12, 106);
            this.pnlChartTU.Name = "pnlChartTU";
            this.pnlChartTU.Size = new System.Drawing.Size(356, 160);
            this.dgvTU.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvTU.Location = new System.Drawing.Point(12, 272);
            this.dgvTU.Name = "dgvTU";
            this.dgvTU.Size = new System.Drawing.Size(356, 212);
            this.dgvTU.TabIndex = 3;
            //
            // ===== pnlPV – phạm vi phục vụ =====
            //
            this.pnlPV.Controls.Add(this.lblBanKinh);
            this.pnlPV.Controls.Add(this.nudBanKinh);
            this.pnlPV.Controls.Add(this.btnThongKePV);
            this.pnlPV.Controls.Add(this.pnlChartPV);
            this.pnlPV.Controls.Add(this.dgvPV);
            this.pnlPV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPV.Name = "pnlPV";
            this.pnlPV.Size = new System.Drawing.Size(380, 490);
            this.lblBanKinh.AutoSize = true;
            this.lblBanKinh.Location = new System.Drawing.Point(12, 6);
            this.lblBanKinh.Name = "lblBanKinh";
            this.lblBanKinh.Text = "Bán kính phục vụ của trạm (m)";
            this.nudBanKinh.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            this.nudBanKinh.Location = new System.Drawing.Point(12, 26);
            this.nudBanKinh.Maximum = new decimal(new int[] { 1500, 0, 0, 0 });
            this.nudBanKinh.Minimum = new decimal(new int[] { 200, 0, 0, 0 });
            this.nudBanKinh.Name = "nudBanKinh";
            this.nudBanKinh.Size = new System.Drawing.Size(206, 25);
            this.nudBanKinh.TabIndex = 0;
            this.nudBanKinh.Value = new decimal(new int[] { 500, 0, 0, 0 });
            this.btnThongKePV.Location = new System.Drawing.Point(226, 23);
            this.btnThongKePV.Name = "btnThongKePV";
            this.btnThongKePV.Size = new System.Drawing.Size(138, 31);
            this.btnThongKePV.TabIndex = 1;
            this.btnThongKePV.Text = "Thống kê";
            this.pnlChartPV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlChartPV.Location = new System.Drawing.Point(12, 60);
            this.pnlChartPV.Name = "pnlChartPV";
            this.pnlChartPV.Size = new System.Drawing.Size(356, 200);
            this.dgvPV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvPV.Location = new System.Drawing.Point(12, 266);
            this.dgvPV.Name = "dgvPV";
            this.dgvPV.Size = new System.Drawing.Size(356, 218);
            this.dgvPV.TabIndex = 2;
            //
            // pnlNut
            //
            this.pnlNut.Controls.Add(this.btnXuat);
            this.pnlNut.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlNut.Name = "pnlNut";
            this.pnlNut.Size = new System.Drawing.Size(380, 56);
            this.btnXuat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnXuat.Location = new System.Drawing.Point(12, 10);
            this.btnXuat.Name = "btnXuat";
            this.btnXuat.Size = new System.Drawing.Size(356, 36);
            this.btnXuat.TabIndex = 9;
            this.btnXuat.Text = "Xuất báo cáo Excel";
            //
            // UcThongKe
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlNoiDung);
            this.Controls.Add(this.pnlNut);
            this.Controls.Add(this.pnlKpi);
            this.Controls.Add(this.pnlTab);
            this.Controls.Add(this.lblTieuDe);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Name = "UcThongKe";
            this.Size = new System.Drawing.Size(380, 708);
            this.pnlTab.ResumeLayout(false);
            this.pnlKpi.ResumeLayout(false);
            this.pnlNoiDung.ResumeLayout(false);
            this.pnlKV.ResumeLayout(false);
            this.pnlKV.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKV)).EndInit();
            this.pnlTU.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTU)).EndInit();
            this.pnlPV.ResumeLayout(false);
            this.pnlPV.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBanKinh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPV)).EndInit();
            this.pnlNut.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTieuDe;
        private System.Windows.Forms.Panel pnlTab;
        private System.Windows.Forms.Button btnTabKhuVuc;
        private System.Windows.Forms.Button btnTabTuyen;
        private System.Windows.Forms.Button btnTabPhamVi;
        private System.Windows.Forms.Panel pnlKpi;
        private System.Windows.Forms.Label lblKpi1;
        private System.Windows.Forms.Label lblKpi2;
        private System.Windows.Forms.Label lblKpi3;
        private System.Windows.Forms.Panel pnlNoiDung;
        private System.Windows.Forms.Panel pnlKV;
        private System.Windows.Forms.Label lblDonVi;
        private System.Windows.Forms.ComboBox cboDonVi;
        private System.Windows.Forms.Button btnThongKeKV;
        private System.Windows.Forms.Panel pnlChartKV;
        private System.Windows.Forms.DataGridView dgvKV;
        private System.Windows.Forms.Panel pnlTU;
        private System.Windows.Forms.CheckedListBox clbTuyen;
        private System.Windows.Forms.Button btnThongKeTU;
        private System.Windows.Forms.Button btnChonHet;
        private System.Windows.Forms.Panel pnlChartTU;
        private System.Windows.Forms.DataGridView dgvTU;
        private System.Windows.Forms.Panel pnlPV;
        private System.Windows.Forms.Label lblBanKinh;
        private System.Windows.Forms.NumericUpDown nudBanKinh;
        private System.Windows.Forms.Button btnThongKePV;
        private System.Windows.Forms.Panel pnlChartPV;
        private System.Windows.Forms.DataGridView dgvPV;
        private System.Windows.Forms.Panel pnlNut;
        private System.Windows.Forms.Button btnXuat;
    }
}
