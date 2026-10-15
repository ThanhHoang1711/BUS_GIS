namespace XeBus_GIS.Forms
{
    partial class UcTram
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
            this.components = new System.ComponentModel.Container();
            this.lblTieuDe = new System.Windows.Forms.Label();
            this.pnlThuocTinh = new System.Windows.Forms.Panel();
            this.lblHdrThuocTinh = new System.Windows.Forms.Label();
            this.lblTen = new System.Windows.Forms.Label();
            this.txtTen = new System.Windows.Forms.TextBox();
            this.lblLoai = new System.Windows.Forms.Label();
            this.cboLoai = new System.Windows.Forms.ComboBox();
            this.lblTrangThai = new System.Windows.Forms.Label();
            this.cboTrangThai = new System.Windows.Forms.ComboBox();
            this.lblTuyen = new System.Windows.Forms.Label();
            this.cboTuyen = new System.Windows.Forms.ComboBox();
            this.lblPhuong = new System.Windows.Forms.Label();
            this.cboPhuong = new System.Windows.Forms.ComboBox();
            this.btnTim = new System.Windows.Forms.Button();
            this.btnDatLai = new System.Windows.Forms.Button();
            this.pnlKhongGian = new System.Windows.Forms.Panel();
            this.lblHdrKhongGian = new System.Windows.Forms.Label();
            this.btnVeVung = new System.Windows.Forms.Button();
            this.btnTheoPhuong = new System.Windows.Forms.Button();
            this.lblBanKinh = new System.Windows.Forms.Label();
            this.nudBanKinh = new System.Windows.Forms.NumericUpDown();
            this.btnBanKinh = new System.Windows.Forms.Button();
            this.pnlKetQua = new System.Windows.Forms.Panel();
            this.lblKetQua = new System.Windows.Forms.Label();
            this.dgvKetQua = new System.Windows.Forms.DataGridView();
            this.pnlNut = new System.Windows.Forms.Panel();
            this.btnSua = new System.Windows.Forms.Button();
            this.btnHuyKetQua = new System.Windows.Forms.Button();
            this.pnlThuocTinh.SuspendLayout();
            this.pnlKhongGian.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBanKinh)).BeginInit();
            this.pnlKetQua.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKetQua)).BeginInit();
            this.pnlNut.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTieuDe
            //
            this.lblTieuDe.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblTieuDe.Size = new System.Drawing.Size(380, 42);
            this.lblTieuDe.Text = "QUẢN LÝ TRẠM XE BUÝT";
            this.lblTieuDe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlThuocTinh
            //
            this.pnlThuocTinh.Controls.Add(this.lblHdrThuocTinh);
            this.pnlThuocTinh.Controls.Add(this.lblTen);
            this.pnlThuocTinh.Controls.Add(this.txtTen);
            this.pnlThuocTinh.Controls.Add(this.lblLoai);
            this.pnlThuocTinh.Controls.Add(this.cboLoai);
            this.pnlThuocTinh.Controls.Add(this.lblTrangThai);
            this.pnlThuocTinh.Controls.Add(this.cboTrangThai);
            this.pnlThuocTinh.Controls.Add(this.lblTuyen);
            this.pnlThuocTinh.Controls.Add(this.cboTuyen);
            this.pnlThuocTinh.Controls.Add(this.lblPhuong);
            this.pnlThuocTinh.Controls.Add(this.cboPhuong);
            this.pnlThuocTinh.Controls.Add(this.btnTim);
            this.pnlThuocTinh.Controls.Add(this.btnDatLai);
            this.pnlThuocTinh.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlThuocTinh.Name = "pnlThuocTinh";
            this.pnlThuocTinh.Size = new System.Drawing.Size(380, 232);
            //
            // lblHdrThuocTinh
            //
            this.lblHdrThuocTinh.AutoSize = true;
            this.lblHdrThuocTinh.Location = new System.Drawing.Point(12, 6);
            this.lblHdrThuocTinh.Name = "lblHdrThuocTinh";
            this.lblHdrThuocTinh.Text = "Tra cứu theo thuộc tính";
            //
            // lblTen
            //
            this.lblTen.AutoSize = true;
            this.lblTen.Location = new System.Drawing.Point(12, 34);
            this.lblTen.Name = "lblTen";
            this.lblTen.Text = "Tên hoặc mã trạm (gõ không dấu cũng được)";
            //
            // txtTen
            //
            this.txtTen.Location = new System.Drawing.Point(12, 54);
            this.txtTen.Name = "txtTen";
            this.txtTen.Size = new System.Drawing.Size(352, 25);
            this.txtTen.TabIndex = 0;
            //
            // lblLoai
            //
            this.lblLoai.AutoSize = true;
            this.lblLoai.Location = new System.Drawing.Point(12, 86);
            this.lblLoai.Name = "lblLoai";
            this.lblLoai.Text = "Loại trạm";
            //
            // cboLoai
            //
            this.cboLoai.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLoai.Location = new System.Drawing.Point(12, 106);
            this.cboLoai.Name = "cboLoai";
            this.cboLoai.Size = new System.Drawing.Size(170, 25);
            this.cboLoai.TabIndex = 1;
            //
            // lblTrangThai
            //
            this.lblTrangThai.AutoSize = true;
            this.lblTrangThai.Location = new System.Drawing.Point(194, 86);
            this.lblTrangThai.Name = "lblTrangThai";
            this.lblTrangThai.Text = "Trạng thái";
            //
            // cboTrangThai
            //
            this.cboTrangThai.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTrangThai.Location = new System.Drawing.Point(194, 106);
            this.cboTrangThai.Name = "cboTrangThai";
            this.cboTrangThai.Size = new System.Drawing.Size(170, 25);
            this.cboTrangThai.TabIndex = 2;
            //
            // lblTuyen
            //
            this.lblTuyen.AutoSize = true;
            this.lblTuyen.Location = new System.Drawing.Point(12, 138);
            this.lblTuyen.Name = "lblTuyen";
            this.lblTuyen.Text = "Tuyến";
            //
            // cboTuyen
            //
            this.cboTuyen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTuyen.Location = new System.Drawing.Point(12, 158);
            this.cboTuyen.Name = "cboTuyen";
            this.cboTuyen.Size = new System.Drawing.Size(170, 25);
            this.cboTuyen.TabIndex = 3;
            //
            // lblPhuong
            //
            this.lblPhuong.AutoSize = true;
            this.lblPhuong.Location = new System.Drawing.Point(194, 138);
            this.lblPhuong.Name = "lblPhuong";
            this.lblPhuong.Text = "Phường/xã";
            //
            // cboPhuong
            //
            this.cboPhuong.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboPhuong.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboPhuong.Location = new System.Drawing.Point(194, 158);
            this.cboPhuong.Name = "cboPhuong";
            this.cboPhuong.Size = new System.Drawing.Size(170, 25);
            this.cboPhuong.TabIndex = 4;
            //
            // btnTim
            //
            this.btnTim.Location = new System.Drawing.Point(12, 192);
            this.btnTim.Name = "btnTim";
            this.btnTim.Size = new System.Drawing.Size(240, 34);
            this.btnTim.TabIndex = 5;
            this.btnTim.Text = "Tìm kiếm";
            //
            // btnDatLai
            //
            this.btnDatLai.Location = new System.Drawing.Point(260, 192);
            this.btnDatLai.Name = "btnDatLai";
            this.btnDatLai.Size = new System.Drawing.Size(104, 34);
            this.btnDatLai.TabIndex = 6;
            this.btnDatLai.Text = "Đặt lại";
            //
            // pnlKhongGian
            //
            this.pnlKhongGian.Controls.Add(this.lblHdrKhongGian);
            this.pnlKhongGian.Controls.Add(this.btnVeVung);
            this.pnlKhongGian.Controls.Add(this.btnTheoPhuong);
            this.pnlKhongGian.Controls.Add(this.lblBanKinh);
            this.pnlKhongGian.Controls.Add(this.nudBanKinh);
            this.pnlKhongGian.Controls.Add(this.btnBanKinh);
            this.pnlKhongGian.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlKhongGian.Name = "pnlKhongGian";
            this.pnlKhongGian.Size = new System.Drawing.Size(380, 124);
            //
            // lblHdrKhongGian
            //
            this.lblHdrKhongGian.AutoSize = true;
            this.lblHdrKhongGian.Location = new System.Drawing.Point(12, 8);
            this.lblHdrKhongGian.Name = "lblHdrKhongGian";
            this.lblHdrKhongGian.Text = "Tra cứu theo không gian";
            //
            // btnVeVung
            //
            this.btnVeVung.Location = new System.Drawing.Point(12, 36);
            this.btnVeVung.Name = "btnVeVung";
            this.btnVeVung.Size = new System.Drawing.Size(170, 34);
            this.btnVeVung.TabIndex = 7;
            this.btnVeVung.Text = "Vẽ vùng";
            //
            // btnTheoPhuong
            //
            this.btnTheoPhuong.Location = new System.Drawing.Point(194, 36);
            this.btnTheoPhuong.Name = "btnTheoPhuong";
            this.btnTheoPhuong.Size = new System.Drawing.Size(170, 34);
            this.btnTheoPhuong.TabIndex = 8;
            this.btnTheoPhuong.Text = "Chọn phường/xã";
            //
            // lblBanKinh
            //
            this.lblBanKinh.AutoSize = true;
            this.lblBanKinh.Location = new System.Drawing.Point(12, 86);
            this.lblBanKinh.Name = "lblBanKinh";
            this.lblBanKinh.Text = "Bán kính (m)";
            //
            // nudBanKinh
            //
            this.nudBanKinh.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudBanKinh.Location = new System.Drawing.Point(100, 82);
            this.nudBanKinh.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            this.nudBanKinh.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
            this.nudBanKinh.Name = "nudBanKinh";
            this.nudBanKinh.Size = new System.Drawing.Size(82, 25);
            this.nudBanKinh.TabIndex = 9;
            this.nudBanKinh.Value = new decimal(new int[] { 500, 0, 0, 0 });
            //
            // btnBanKinh
            //
            this.btnBanKinh.Location = new System.Drawing.Point(194, 78);
            this.btnBanKinh.Name = "btnBanKinh";
            this.btnBanKinh.Size = new System.Drawing.Size(170, 34);
            this.btnBanKinh.TabIndex = 10;
            this.btnBanKinh.Text = "Quanh điểm click";
            //
            // pnlKetQua
            //
            this.pnlKetQua.Controls.Add(this.lblKetQua);
            this.pnlKetQua.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlKetQua.Name = "pnlKetQua";
            this.pnlKetQua.Size = new System.Drawing.Size(380, 34);
            //
            // lblKetQua
            //
            this.lblKetQua.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblKetQua.Name = "lblKetQua";
            this.lblKetQua.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblKetQua.Text = "Kết quả";
            this.lblKetQua.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // dgvKetQua
            //
            this.dgvKetQua.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvKetQua.Name = "dgvKetQua";
            this.dgvKetQua.TabIndex = 11;
            //
            // pnlNut
            //
            this.pnlNut.Controls.Add(this.btnSua);
            this.pnlNut.Controls.Add(this.btnHuyKetQua);
            this.pnlNut.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlNut.Name = "pnlNut";
            this.pnlNut.Size = new System.Drawing.Size(380, 54);
            //
            // btnSua
            //
            this.btnSua.Location = new System.Drawing.Point(12, 10);
            this.btnSua.Name = "btnSua";
            this.btnSua.Size = new System.Drawing.Size(240, 34);
            this.btnSua.TabIndex = 12;
            this.btnSua.Text = "Sửa thuộc tính trạm";
            //
            // btnHuyKetQua
            //
            this.btnHuyKetQua.Location = new System.Drawing.Point(260, 10);
            this.btnHuyKetQua.Name = "btnHuyKetQua";
            this.btnHuyKetQua.Size = new System.Drawing.Size(104, 34);
            this.btnHuyKetQua.TabIndex = 13;
            this.btnHuyKetQua.Text = "Xoá kết quả";
            //
            // UcTram
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;   // co giãn DPI bằng UiHelper.CoGianDpi (xem ctor)
            this.Controls.Add(this.dgvKetQua);
            this.Controls.Add(this.pnlNut);
            this.Controls.Add(this.pnlKetQua);
            this.Controls.Add(this.pnlKhongGian);
            this.Controls.Add(this.pnlThuocTinh);
            this.Controls.Add(this.lblTieuDe);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Name = "UcTram";
            this.Size = new System.Drawing.Size(380, 760);
            this.pnlThuocTinh.ResumeLayout(false);
            this.pnlThuocTinh.PerformLayout();
            this.pnlKhongGian.ResumeLayout(false);
            this.pnlKhongGian.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBanKinh)).EndInit();
            this.pnlKetQua.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvKetQua)).EndInit();
            this.pnlNut.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTieuDe;
        private System.Windows.Forms.Panel pnlThuocTinh;
        private System.Windows.Forms.Label lblHdrThuocTinh;
        private System.Windows.Forms.Label lblTen;
        private System.Windows.Forms.TextBox txtTen;
        private System.Windows.Forms.Label lblLoai;
        private System.Windows.Forms.ComboBox cboLoai;
        private System.Windows.Forms.Label lblTrangThai;
        private System.Windows.Forms.ComboBox cboTrangThai;
        private System.Windows.Forms.Label lblTuyen;
        private System.Windows.Forms.ComboBox cboTuyen;
        private System.Windows.Forms.Label lblPhuong;
        private System.Windows.Forms.ComboBox cboPhuong;
        private System.Windows.Forms.Button btnTim;
        private System.Windows.Forms.Button btnDatLai;
        private System.Windows.Forms.Panel pnlKhongGian;
        private System.Windows.Forms.Label lblHdrKhongGian;
        private System.Windows.Forms.Button btnVeVung;
        private System.Windows.Forms.Button btnTheoPhuong;
        private System.Windows.Forms.Label lblBanKinh;
        private System.Windows.Forms.NumericUpDown nudBanKinh;
        private System.Windows.Forms.Button btnBanKinh;
        private System.Windows.Forms.Panel pnlKetQua;
        private System.Windows.Forms.Label lblKetQua;
        private System.Windows.Forms.DataGridView dgvKetQua;
        private System.Windows.Forms.Panel pnlNut;
        private System.Windows.Forms.Button btnSua;
        private System.Windows.Forms.Button btnHuyKetQua;
    }
}
