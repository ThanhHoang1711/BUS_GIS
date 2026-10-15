namespace XeBus_GIS.Forms
{
    partial class UcPhanTich
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
            this.pnlVung = new System.Windows.Forms.Panel();
            this.lblHdrVung = new System.Windows.Forms.Label();
            this.rdoToanTP = new System.Windows.Forms.RadioButton();
            this.rdoPhuong = new System.Windows.Forms.RadioButton();
            this.cboPhuong = new System.Windows.Forms.ComboBox();
            this.rdoVeVung = new System.Windows.Forms.RadioButton();
            this.btnVe = new System.Windows.Forms.Button();
            this.lblVung = new System.Windows.Forms.Label();
            this.pnlTieuChi = new System.Windows.Forms.Panel();
            this.lblHdrTieuChi = new System.Windows.Forms.Label();
            this.lblBanKinh = new System.Windows.Forms.Label();
            this.nudBanKinh = new System.Windows.Forms.NumericUpDown();
            this.lblNguong = new System.Windows.Forms.Label();
            this.nudNguong = new System.Windows.Forms.NumericUpDown();
            this.lblGiaiThich = new System.Windows.Forms.Label();
            this.btnPhanTich = new System.Windows.Forms.Button();
            this.pnlKetQua = new System.Windows.Forms.Panel();
            this.lblHdrKetQua = new System.Windows.Forms.Label();
            this.kpiDan = new XeBus_GIS.UI.Controls.TheKpi();
            this.kpiKhu = new XeBus_GIS.UI.Controls.TheKpi();
            this.kpiTyLe = new XeBus_GIS.UI.Controls.TheKpi();
            this.chuGiai = new XeBus_GIS.UI.Controls.KhungChuGiai();
            this.chkVungPV = new System.Windows.Forms.CheckBox();
            this.dgvThieu = new System.Windows.Forms.DataGridView();
            this.pnlNut = new System.Windows.Forms.Panel();
            this.btnDeXuat = new System.Windows.Forms.Button();
            this.btnXoa = new System.Windows.Forms.Button();
            this.pnlVung.SuspendLayout();
            this.pnlTieuChi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBanKinh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNguong)).BeginInit();
            this.pnlKetQua.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvThieu)).BeginInit();
            this.pnlNut.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTieuDe
            //
            this.lblTieuDe.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Size = new System.Drawing.Size(370, 42);
            this.lblTieuDe.Text = "PHÂN TÍCH KHU VỰC THIẾU TRẠM";
            this.lblTieuDe.Visible = false;
            //
            // pnlVung
            //
            this.pnlVung.Controls.Add(this.lblHdrVung);
            this.pnlVung.Controls.Add(this.rdoToanTP);
            this.pnlVung.Controls.Add(this.rdoPhuong);
            this.pnlVung.Controls.Add(this.cboPhuong);
            this.pnlVung.Controls.Add(this.rdoVeVung);
            this.pnlVung.Controls.Add(this.btnVe);
            this.pnlVung.Controls.Add(this.lblVung);
            this.pnlVung.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlVung.Name = "pnlVung";
            this.pnlVung.Size = new System.Drawing.Size(370, 150);
            //
            // lblHdrVung
            //
            this.lblHdrVung.AutoSize = true;
            this.lblHdrVung.Location = new System.Drawing.Point(12, 8);
            this.lblHdrVung.Name = "lblHdrVung";
            this.lblHdrVung.Text = "1. Vùng phân tích";
            //
            // rdoToanTP
            //
            this.rdoToanTP.AutoSize = true;
            this.rdoToanTP.Location = new System.Drawing.Point(14, 34);
            this.rdoToanTP.Name = "rdoToanTP";
            this.rdoToanTP.TabIndex = 0;
            this.rdoToanTP.Text = "Toàn thành phố";
            //
            // rdoPhuong
            //
            this.rdoPhuong.AutoSize = true;
            this.rdoPhuong.Checked = true;
            this.rdoPhuong.Location = new System.Drawing.Point(14, 64);
            this.rdoPhuong.Name = "rdoPhuong";
            this.rdoPhuong.TabIndex = 1;
            this.rdoPhuong.TabStop = true;
            this.rdoPhuong.Text = "Phường/xã";
            //
            // cboPhuong
            //
            this.cboPhuong.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboPhuong.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboPhuong.Location = new System.Drawing.Point(124, 62);
            this.cboPhuong.Name = "cboPhuong";
            this.cboPhuong.Size = new System.Drawing.Size(232, 25);
            this.cboPhuong.TabIndex = 2;
            //
            // rdoVeVung
            //
            this.rdoVeVung.AutoSize = true;
            this.rdoVeVung.Location = new System.Drawing.Point(14, 96);
            this.rdoVeVung.Name = "rdoVeVung";
            this.rdoVeVung.TabIndex = 3;
            this.rdoVeVung.Text = "Vùng tự vẽ";
            //
            // btnVe
            //
            this.btnVe.Location = new System.Drawing.Point(124, 92);
            this.btnVe.Name = "btnVe";
            this.btnVe.Size = new System.Drawing.Size(232, 32);
            this.btnVe.TabIndex = 4;
            this.btnVe.Text = "Vẽ vùng trên bản đồ";
            //
            // lblVung
            //
            this.lblVung.Location = new System.Drawing.Point(12, 126);
            this.lblVung.Name = "lblVung";
            this.lblVung.Size = new System.Drawing.Size(344, 20);
            //
            // pnlTieuChi
            //
            this.pnlTieuChi.Controls.Add(this.lblHdrTieuChi);
            this.pnlTieuChi.Controls.Add(this.lblBanKinh);
            this.pnlTieuChi.Controls.Add(this.nudBanKinh);
            this.pnlTieuChi.Controls.Add(this.lblNguong);
            this.pnlTieuChi.Controls.Add(this.nudNguong);
            this.pnlTieuChi.Controls.Add(this.lblGiaiThich);
            this.pnlTieuChi.Controls.Add(this.btnPhanTich);
            this.pnlTieuChi.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTieuChi.Name = "pnlTieuChi";
            this.pnlTieuChi.Size = new System.Drawing.Size(370, 200);
            //
            // lblHdrTieuChi
            //
            this.lblHdrTieuChi.AutoSize = true;
            this.lblHdrTieuChi.Location = new System.Drawing.Point(12, 8);
            this.lblHdrTieuChi.Name = "lblHdrTieuChi";
            this.lblHdrTieuChi.Text = "2. Tiêu chí";
            //
            // lblBanKinh
            //
            this.lblBanKinh.AutoSize = true;
            this.lblBanKinh.Location = new System.Drawing.Point(12, 34);
            this.lblBanKinh.Name = "lblBanKinh";
            this.lblBanKinh.Text = "Bán kính phục vụ (m)";
            //
            // nudBanKinh
            //
            this.nudBanKinh.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            this.nudBanKinh.Location = new System.Drawing.Point(12, 54);
            this.nudBanKinh.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            this.nudBanKinh.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudBanKinh.Name = "nudBanKinh";
            this.nudBanKinh.Size = new System.Drawing.Size(166, 25);
            this.nudBanKinh.TabIndex = 5;
            this.nudBanKinh.Value = new decimal(new int[] { 500, 0, 0, 0 });
            //
            // lblNguong
            //
            this.lblNguong.AutoSize = true;
            this.lblNguong.Location = new System.Drawing.Point(190, 34);
            this.lblNguong.Name = "lblNguong";
            this.lblNguong.Text = "Ngưỡng phục vụ (%)";
            //
            // nudNguong
            //
            this.nudNguong.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            this.nudNguong.Location = new System.Drawing.Point(190, 54);
            this.nudNguong.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            this.nudNguong.Name = "nudNguong";
            this.nudNguong.Size = new System.Drawing.Size(166, 25);
            this.nudNguong.TabIndex = 6;
            this.nudNguong.Value = new decimal(new int[] { 50, 0, 0, 0 });
            //
            // lblGiaiThich
            //
            this.lblGiaiThich.Location = new System.Drawing.Point(12, 88);
            this.lblGiaiThich.Name = "lblGiaiThich";
            this.lblGiaiThich.Size = new System.Drawing.Size(344, 62);
            //
            // btnPhanTich
            //
            this.btnPhanTich.Location = new System.Drawing.Point(12, 156);
            this.btnPhanTich.Name = "btnPhanTich";
            this.btnPhanTich.Size = new System.Drawing.Size(344, 36);
            this.btnPhanTich.TabIndex = 7;
            this.btnPhanTich.Text = "Phân tích";
            //
            // pnlKetQua
            //
            this.pnlKetQua.Controls.Add(this.lblHdrKetQua);
            this.pnlKetQua.Controls.Add(this.kpiDan);
            this.pnlKetQua.Controls.Add(this.kpiKhu);
            this.pnlKetQua.Controls.Add(this.kpiTyLe);
            this.pnlKetQua.Controls.Add(this.chuGiai);
            this.pnlKetQua.Controls.Add(this.chkVungPV);
            this.pnlKetQua.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlKetQua.Name = "pnlKetQua";
            this.pnlKetQua.Size = new System.Drawing.Size(370, 176);
            //
            // lblHdrKetQua
            //
            this.lblHdrKetQua.AutoSize = true;
            this.lblHdrKetQua.Location = new System.Drawing.Point(12, 8);
            this.lblHdrKetQua.Name = "lblHdrKetQua";
            this.lblHdrKetQua.Text = "3. Kết quả";
            //
            // kpi
            //
            this.kpiDan.Location = new System.Drawing.Point(12, 32);
            this.kpiDan.Name = "kpiDan";
            this.kpiDan.Nhan = "Chưa có trạm";
            this.kpiDan.Size = new System.Drawing.Size(110, 84);
            this.kpiKhu.Location = new System.Drawing.Point(129, 32);
            this.kpiKhu.Name = "kpiKhu";
            this.kpiKhu.Nhan = "Khu thiếu trạm";
            this.kpiKhu.Size = new System.Drawing.Size(110, 84);
            this.kpiTyLe.Location = new System.Drawing.Point(246, 32);
            this.kpiTyLe.Name = "kpiTyLe";
            this.kpiTyLe.Nhan = "Dân được PV";
            this.kpiTyLe.Size = new System.Drawing.Size(110, 84);
            //
            // chuGiai
            //
            this.chuGiai.Location = new System.Drawing.Point(12, 124);
            this.chuGiai.Name = "chuGiai";
            this.chuGiai.Size = new System.Drawing.Size(344, 22);
            //
            // chkVungPV
            //
            this.chkVungPV.AutoSize = true;
            this.chkVungPV.Checked = true;
            this.chkVungPV.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkVungPV.Location = new System.Drawing.Point(14, 146);
            this.chkVungPV.Name = "chkVungPV";
            this.chkVungPV.TabIndex = 8;
            this.chkVungPV.Text = "Hiện vùng đã được phục vụ";
            //
            // dgvThieu
            //
            this.dgvThieu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvThieu.Name = "dgvThieu";
            this.dgvThieu.TabIndex = 9;
            //
            // pnlNut
            //
            this.pnlNut.Controls.Add(this.btnDeXuat);
            this.pnlNut.Controls.Add(this.btnXoa);
            this.pnlNut.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlNut.Name = "pnlNut";
            this.pnlNut.Size = new System.Drawing.Size(370, 54);
            //
            // btnDeXuat
            //
            this.btnDeXuat.Location = new System.Drawing.Point(12, 10);
            this.btnDeXuat.Name = "btnDeXuat";
            this.btnDeXuat.Size = new System.Drawing.Size(236, 34);
            this.btnDeXuat.TabIndex = 10;
            this.btnDeXuat.Text = "Đề xuất trạm cho vùng này";
            //
            // btnXoa
            //
            this.btnXoa.Location = new System.Drawing.Point(256, 10);
            this.btnXoa.Name = "btnXoa";
            this.btnXoa.Size = new System.Drawing.Size(100, 34);
            this.btnXoa.TabIndex = 11;
            this.btnXoa.Text = "Xoá kết quả";
            //
            // UcPhanTich
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;   // co giãn DPI bằng UiHelper.CoGianDpi (xem ctor)
            this.Controls.Add(this.dgvThieu);
            this.Controls.Add(this.pnlNut);
            this.Controls.Add(this.pnlKetQua);
            this.Controls.Add(this.pnlTieuChi);
            this.Controls.Add(this.pnlVung);
            this.Controls.Add(this.lblTieuDe);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Name = "UcPhanTich";
            this.Size = new System.Drawing.Size(370, 760);
            this.pnlVung.ResumeLayout(false);
            this.pnlVung.PerformLayout();
            this.pnlTieuChi.ResumeLayout(false);
            this.pnlTieuChi.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBanKinh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNguong)).EndInit();
            this.pnlKetQua.ResumeLayout(false);
            this.pnlKetQua.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvThieu)).EndInit();
            this.pnlNut.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTieuDe;
        private System.Windows.Forms.Panel pnlVung;
        private System.Windows.Forms.Label lblHdrVung;
        private System.Windows.Forms.RadioButton rdoToanTP;
        private System.Windows.Forms.RadioButton rdoPhuong;
        private System.Windows.Forms.ComboBox cboPhuong;
        private System.Windows.Forms.RadioButton rdoVeVung;
        private System.Windows.Forms.Button btnVe;
        private System.Windows.Forms.Label lblVung;
        private System.Windows.Forms.Panel pnlTieuChi;
        private System.Windows.Forms.Label lblHdrTieuChi;
        private System.Windows.Forms.Label lblBanKinh;
        private System.Windows.Forms.NumericUpDown nudBanKinh;
        private System.Windows.Forms.Label lblNguong;
        private System.Windows.Forms.NumericUpDown nudNguong;
        private System.Windows.Forms.Label lblGiaiThich;
        private System.Windows.Forms.Button btnPhanTich;
        private System.Windows.Forms.Panel pnlKetQua;
        private System.Windows.Forms.Label lblHdrKetQua;
        private XeBus_GIS.UI.Controls.TheKpi kpiDan;
        private XeBus_GIS.UI.Controls.TheKpi kpiKhu;
        private XeBus_GIS.UI.Controls.TheKpi kpiTyLe;
        private XeBus_GIS.UI.Controls.KhungChuGiai chuGiai;
        private System.Windows.Forms.CheckBox chkVungPV;
        private System.Windows.Forms.DataGridView dgvThieu;
        private System.Windows.Forms.Panel pnlNut;
        private System.Windows.Forms.Button btnDeXuat;
        private System.Windows.Forms.Button btnXoa;
    }
}
