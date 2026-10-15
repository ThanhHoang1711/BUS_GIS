namespace XeBus_GIS.Forms
{
    partial class UcChonViTri
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
            this.pnlCuon = new System.Windows.Forms.Panel();
            this.pnlHienThi = new System.Windows.Forms.Panel();
            this.lblH3 = new System.Windows.Forms.Label();
            this.pnlNut = new System.Windows.Forms.Panel();
            this.btnPhanTich = new System.Windows.Forms.Button();
            this.lblTomTat = new System.Windows.Forms.Label();
            this.pnlTrongSo = new System.Windows.Forms.Panel();
            this.lblH2 = new System.Windows.Forms.Label();
            this.pnlKhuVuc = new System.Windows.Forms.Panel();
            this.lblH1 = new System.Windows.Forms.Label();
            this.rdoPhuong = new System.Windows.Forms.RadioButton();
            this.cboPhuong = new System.Windows.Forms.ComboBox();
            this.rdoVe = new System.Windows.Forms.RadioButton();
            this.btnVe = new System.Windows.Forms.Button();
            this.lblVung = new System.Windows.Forms.Label();
            this.lblBanKinh = new System.Windows.Forms.Label();
            this.nudBanKinh = new System.Windows.Forms.NumericUpDown();
            this.lblSoDX = new System.Windows.Forms.Label();
            this.nudSoDX = new System.Windows.Forms.NumericUpDown();
            this.lblCach = new System.Windows.Forms.Label();
            this.nudCach = new System.Windows.Forms.NumericUpDown();
            this.pnlCuon.SuspendLayout();
            this.pnlHienThi.SuspendLayout();
            this.pnlNut.SuspendLayout();
            this.pnlTrongSo.SuspendLayout();
            this.pnlKhuVuc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBanKinh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSoDX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCach)).BeginInit();
            this.SuspendLayout();
            //
            // pnlCuon
            //
            this.pnlCuon.AutoScroll = true;
            this.pnlCuon.Controls.Add(this.pnlHienThi);
            this.pnlCuon.Controls.Add(this.pnlNut);
            this.pnlCuon.Controls.Add(this.pnlTrongSo);
            this.pnlCuon.Controls.Add(this.pnlKhuVuc);
            this.pnlCuon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCuon.Location = new System.Drawing.Point(0, 0);
            this.pnlCuon.Name = "pnlCuon";
            this.pnlCuon.Size = new System.Drawing.Size(380, 760);
            this.pnlCuon.TabIndex = 0;
            //
            // pnlHienThi
            //
            this.pnlHienThi.Controls.Add(this.lblH3);
            this.pnlHienThi.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHienThi.Location = new System.Drawing.Point(0, 646);
            this.pnlHienThi.Name = "pnlHienThi";
            this.pnlHienThi.Size = new System.Drawing.Size(363, 200);
            this.pnlHienThi.TabIndex = 3;
            //
            // lblH3
            //
            this.lblH3.AutoSize = true;
            this.lblH3.Location = new System.Drawing.Point(12, 10);
            this.lblH3.Name = "lblH3";
            this.lblH3.Text = "3 · Xem từng tiêu chí trên bản đồ";
            //
            // pnlNut
            //
            this.pnlNut.Controls.Add(this.btnPhanTich);
            this.pnlNut.Controls.Add(this.lblTomTat);
            this.pnlNut.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlNut.Location = new System.Drawing.Point(0, 554);
            this.pnlNut.Name = "pnlNut";
            this.pnlNut.Size = new System.Drawing.Size(363, 92);
            this.pnlNut.TabIndex = 2;
            //
            // btnPhanTich
            //
            this.btnPhanTich.Location = new System.Drawing.Point(12, 8);
            this.btnPhanTich.Name = "btnPhanTich";
            this.btnPhanTich.Size = new System.Drawing.Size(339, 36);
            this.btnPhanTich.TabIndex = 0;
            this.btnPhanTich.Text = "Phân tích khu vực";
            //
            // lblTomTat
            //
            this.lblTomTat.Location = new System.Drawing.Point(12, 50);
            this.lblTomTat.Name = "lblTomTat";
            this.lblTomTat.Size = new System.Drawing.Size(339, 38);
            this.lblTomTat.Text = "";
            //
            // pnlTrongSo
            //
            this.pnlTrongSo.Controls.Add(this.lblH2);
            this.pnlTrongSo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTrongSo.Location = new System.Drawing.Point(0, 266);
            this.pnlTrongSo.Name = "pnlTrongSo";
            this.pnlTrongSo.Size = new System.Drawing.Size(363, 288);
            this.pnlTrongSo.TabIndex = 1;
            //
            // lblH2
            //
            this.lblH2.AutoSize = true;
            this.lblH2.Location = new System.Drawing.Point(12, 10);
            this.lblH2.Name = "lblH2";
            this.lblH2.Text = "2 · Tiêu chí và mức quan trọng";
            //
            // pnlKhuVuc
            //
            this.pnlKhuVuc.Controls.Add(this.lblH1);
            this.pnlKhuVuc.Controls.Add(this.rdoPhuong);
            this.pnlKhuVuc.Controls.Add(this.cboPhuong);
            this.pnlKhuVuc.Controls.Add(this.rdoVe);
            this.pnlKhuVuc.Controls.Add(this.btnVe);
            this.pnlKhuVuc.Controls.Add(this.lblVung);
            this.pnlKhuVuc.Controls.Add(this.lblBanKinh);
            this.pnlKhuVuc.Controls.Add(this.nudBanKinh);
            this.pnlKhuVuc.Controls.Add(this.lblSoDX);
            this.pnlKhuVuc.Controls.Add(this.nudSoDX);
            this.pnlKhuVuc.Controls.Add(this.lblCach);
            this.pnlKhuVuc.Controls.Add(this.nudCach);
            this.pnlKhuVuc.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlKhuVuc.Location = new System.Drawing.Point(0, 0);
            this.pnlKhuVuc.Name = "pnlKhuVuc";
            this.pnlKhuVuc.Size = new System.Drawing.Size(363, 266);
            this.pnlKhuVuc.TabIndex = 0;
            //
            // lblH1
            //
            this.lblH1.AutoSize = true;
            this.lblH1.Location = new System.Drawing.Point(12, 10);
            this.lblH1.Name = "lblH1";
            this.lblH1.Text = "1 · Khu vực muốn đặt trạm";
            //
            // rdoPhuong
            //
            this.rdoPhuong.AutoSize = true;
            this.rdoPhuong.Checked = true;
            this.rdoPhuong.Location = new System.Drawing.Point(12, 40);
            this.rdoPhuong.Name = "rdoPhuong";
            this.rdoPhuong.TabIndex = 0;
            this.rdoPhuong.TabStop = true;
            this.rdoPhuong.Text = "Một phường/xã";
            //
            // cboPhuong
            //
            this.cboPhuong.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboPhuong.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboPhuong.Location = new System.Drawing.Point(32, 64);
            this.cboPhuong.Name = "cboPhuong";
            this.cboPhuong.Size = new System.Drawing.Size(319, 25);
            this.cboPhuong.TabIndex = 1;
            //
            // rdoVe
            //
            this.rdoVe.AutoSize = true;
            this.rdoVe.Location = new System.Drawing.Point(12, 100);
            this.rdoVe.Name = "rdoVe";
            this.rdoVe.TabIndex = 2;
            this.rdoVe.Text = "Khoanh vùng trên bản đồ";
            //
            // btnVe
            //
            this.btnVe.Location = new System.Drawing.Point(237, 95);
            this.btnVe.Name = "btnVe";
            this.btnVe.Size = new System.Drawing.Size(114, 30);
            this.btnVe.TabIndex = 3;
            this.btnVe.Text = "Vẽ vùng";
            //
            // lblVung
            //
            this.lblVung.Location = new System.Drawing.Point(32, 128);
            this.lblVung.Name = "lblVung";
            this.lblVung.Size = new System.Drawing.Size(319, 34);
            this.lblVung.Text = "";
            //
            // lblBanKinh
            //
            this.lblBanKinh.AutoSize = true;
            this.lblBanKinh.Location = new System.Drawing.Point(12, 170);
            this.lblBanKinh.Name = "lblBanKinh";
            this.lblBanKinh.Text = "Bán kính phục vụ của trạm (m)";
            //
            // nudBanKinh
            //
            this.nudBanKinh.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            this.nudBanKinh.Location = new System.Drawing.Point(261, 166);
            this.nudBanKinh.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.nudBanKinh.Minimum = new decimal(new int[] { 200, 0, 0, 0 });
            this.nudBanKinh.Name = "nudBanKinh";
            this.nudBanKinh.Size = new System.Drawing.Size(90, 25);
            this.nudBanKinh.TabIndex = 4;
            this.nudBanKinh.Value = new decimal(new int[] { 500, 0, 0, 0 });
            //
            // lblSoDX
            //
            this.lblSoDX.AutoSize = true;
            this.lblSoDX.Location = new System.Drawing.Point(12, 202);
            this.lblSoDX.Name = "lblSoDX";
            this.lblSoDX.Text = "Số vị trí đề xuất";
            //
            // nudSoDX
            //
            this.nudSoDX.Location = new System.Drawing.Point(261, 198);
            this.nudSoDX.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            this.nudSoDX.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudSoDX.Name = "nudSoDX";
            this.nudSoDX.Size = new System.Drawing.Size(90, 25);
            this.nudSoDX.TabIndex = 5;
            this.nudSoDX.Value = new decimal(new int[] { 5, 0, 0, 0 });
            //
            // lblCach
            //
            this.lblCach.AutoSize = true;
            this.lblCach.Location = new System.Drawing.Point(12, 234);
            this.lblCach.Name = "lblCach";
            this.lblCach.Text = "Các vị trí cách nhau tối thiểu (m)";
            //
            // nudCach
            //
            this.nudCach.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            this.nudCach.Location = new System.Drawing.Point(261, 230);
            this.nudCach.Maximum = new decimal(new int[] { 1500, 0, 0, 0 });
            this.nudCach.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudCach.Name = "nudCach";
            this.nudCach.Size = new System.Drawing.Size(90, 25);
            this.nudCach.TabIndex = 6;
            this.nudCach.Value = new decimal(new int[] { 400, 0, 0, 0 });
            //
            // UcChonViTri
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlCuon);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Name = "UcChonViTri";
            this.Size = new System.Drawing.Size(380, 760);
            this.pnlCuon.ResumeLayout(false);
            this.pnlHienThi.ResumeLayout(false);
            this.pnlHienThi.PerformLayout();
            this.pnlNut.ResumeLayout(false);
            this.pnlTrongSo.ResumeLayout(false);
            this.pnlTrongSo.PerformLayout();
            this.pnlKhuVuc.ResumeLayout(false);
            this.pnlKhuVuc.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBanKinh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSoDX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCach)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlCuon;
        private System.Windows.Forms.Panel pnlKhuVuc;
        private System.Windows.Forms.Label lblH1;
        private System.Windows.Forms.RadioButton rdoPhuong;
        private System.Windows.Forms.ComboBox cboPhuong;
        private System.Windows.Forms.RadioButton rdoVe;
        private System.Windows.Forms.Button btnVe;
        private System.Windows.Forms.Label lblVung;
        private System.Windows.Forms.Label lblBanKinh;
        private System.Windows.Forms.NumericUpDown nudBanKinh;
        private System.Windows.Forms.Label lblSoDX;
        private System.Windows.Forms.NumericUpDown nudSoDX;
        private System.Windows.Forms.Label lblCach;
        private System.Windows.Forms.NumericUpDown nudCach;
        private System.Windows.Forms.Panel pnlTrongSo;
        private System.Windows.Forms.Label lblH2;
        private System.Windows.Forms.Panel pnlNut;
        private System.Windows.Forms.Button btnPhanTich;
        private System.Windows.Forms.Label lblTomTat;
        private System.Windows.Forms.Panel pnlHienThi;
        private System.Windows.Forms.Label lblH3;
    }
}
