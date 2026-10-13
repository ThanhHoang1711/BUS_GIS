namespace XeBus_GIS.Forms
{
    partial class FrmCapNhatTram
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblTieuDe = new System.Windows.Forms.Label();
            this.lblMa = new System.Windows.Forms.Label();
            this.txtMa = new System.Windows.Forms.TextBox();
            this.lblTrangThai = new System.Windows.Forms.Label();
            this.cboTrangThai = new System.Windows.Forms.ComboBox();
            this.lblTen = new System.Windows.Forms.Label();
            this.txtTen = new System.Windows.Forms.TextBox();
            this.lblLoiTen = new System.Windows.Forms.Label();
            this.lblLoai = new System.Windows.Forms.Label();
            this.cboLoai = new System.Windows.Forms.ComboBox();
            this.lblTuyen = new System.Windows.Forms.Label();
            this.cboTuyen = new System.Windows.Forms.ComboBox();
            this.lblPhuongTien = new System.Windows.Forms.Label();
            this.cboPhuongTien = new System.Windows.Forms.ComboBox();
            this.lblViTri = new System.Windows.Forms.Label();
            this.lblViDo = new System.Windows.Forms.Label();
            this.nudViDo = new System.Windows.Forms.NumericUpDown();
            this.lblKinhDo = new System.Windows.Forms.Label();
            this.nudKinhDo = new System.Windows.Forms.NumericUpDown();
            this.btnChonViTri = new System.Windows.Forms.Button();
            this.lblLoiViTri = new System.Windows.Forms.Label();
            this.btnLuu = new System.Windows.Forms.Button();
            this.btnHuy = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudViDo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKinhDo)).BeginInit();
            this.SuspendLayout();
            //
            // lblTieuDe
            //
            this.lblTieuDe.AutoSize = true;
            this.lblTieuDe.Location = new System.Drawing.Point(20, 16);
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Text = "Cập nhật thông tin thuộc tính trạm";
            //
            // lblMa
            //
            this.lblMa.AutoSize = true;
            this.lblMa.Location = new System.Drawing.Point(20, 56);
            this.lblMa.Name = "lblMa";
            this.lblMa.Text = "Mã trạm";
            //
            // txtMa
            //
            this.txtMa.Location = new System.Drawing.Point(20, 76);
            this.txtMa.Name = "txtMa";
            this.txtMa.ReadOnly = true;
            this.txtMa.Size = new System.Drawing.Size(200, 25);
            this.txtMa.TabStop = false;
            //
            // lblTrangThai
            //
            this.lblTrangThai.AutoSize = true;
            this.lblTrangThai.Location = new System.Drawing.Point(240, 56);
            this.lblTrangThai.Name = "lblTrangThai";
            this.lblTrangThai.Text = "Trạng thái *";
            //
            // cboTrangThai
            //
            this.cboTrangThai.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTrangThai.Location = new System.Drawing.Point(240, 76);
            this.cboTrangThai.Name = "cboTrangThai";
            this.cboTrangThai.Size = new System.Drawing.Size(200, 25);
            this.cboTrangThai.TabIndex = 1;
            //
            // lblTen
            //
            this.lblTen.AutoSize = true;
            this.lblTen.Location = new System.Drawing.Point(20, 112);
            this.lblTen.Name = "lblTen";
            this.lblTen.Text = "Tên trạm *";
            //
            // txtTen
            //
            this.txtTen.Location = new System.Drawing.Point(20, 132);
            this.txtTen.MaxLength = 150;
            this.txtTen.Name = "txtTen";
            this.txtTen.Size = new System.Drawing.Size(420, 25);
            this.txtTen.TabIndex = 0;
            //
            // lblLoiTen
            //
            this.lblLoiTen.AutoSize = true;
            this.lblLoiTen.Location = new System.Drawing.Point(20, 160);
            this.lblLoiTen.Name = "lblLoiTen";
            this.lblLoiTen.Visible = false;
            //
            // lblLoai
            //
            this.lblLoai.AutoSize = true;
            this.lblLoai.Location = new System.Drawing.Point(20, 184);
            this.lblLoai.Name = "lblLoai";
            this.lblLoai.Text = "Loại trạm *";
            //
            // cboLoai
            //
            this.cboLoai.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLoai.Location = new System.Drawing.Point(20, 204);
            this.cboLoai.Name = "cboLoai";
            this.cboLoai.Size = new System.Drawing.Size(200, 25);
            this.cboLoai.TabIndex = 2;
            //
            // lblTuyen
            //
            this.lblTuyen.AutoSize = true;
            this.lblTuyen.Location = new System.Drawing.Point(240, 184);
            this.lblTuyen.Name = "lblTuyen";
            this.lblTuyen.Text = "Tuyến chính";
            //
            // cboTuyen
            //
            this.cboTuyen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTuyen.Location = new System.Drawing.Point(240, 204);
            this.cboTuyen.Name = "cboTuyen";
            this.cboTuyen.Size = new System.Drawing.Size(200, 25);
            this.cboTuyen.TabIndex = 3;
            //
            // lblPhuongTien
            //
            this.lblPhuongTien.AutoSize = true;
            this.lblPhuongTien.Location = new System.Drawing.Point(20, 240);
            this.lblPhuongTien.Name = "lblPhuongTien";
            this.lblPhuongTien.Text = "Phương tiện phục vụ";
            //
            // cboPhuongTien
            //
            this.cboPhuongTien.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPhuongTien.Location = new System.Drawing.Point(20, 260);
            this.cboPhuongTien.Name = "cboPhuongTien";
            this.cboPhuongTien.Size = new System.Drawing.Size(420, 25);
            this.cboPhuongTien.TabIndex = 4;
            //
            // lblViTri
            //
            this.lblViTri.AutoSize = true;
            this.lblViTri.Location = new System.Drawing.Point(20, 302);
            this.lblViTri.Name = "lblViTri";
            this.lblViTri.Text = "Vị trí (WGS84)";
            //
            // lblViDo
            //
            this.lblViDo.AutoSize = true;
            this.lblViDo.Location = new System.Drawing.Point(20, 328);
            this.lblViDo.Name = "lblViDo";
            this.lblViDo.Text = "Vĩ độ";
            //
            // nudViDo
            //
            this.nudViDo.DecimalPlaces = 7;
            this.nudViDo.Increment = new decimal(new int[] { 1, 0, 0, 262144 });
            this.nudViDo.Location = new System.Drawing.Point(20, 348);
            this.nudViDo.Maximum = new decimal(new int[] { 22, 0, 0, 0 });
            this.nudViDo.Minimum = new decimal(new int[] { 20, 0, 0, 0 });
            this.nudViDo.Name = "nudViDo";
            this.nudViDo.Size = new System.Drawing.Size(130, 25);
            this.nudViDo.TabIndex = 5;
            this.nudViDo.Value = new decimal(new int[] { 21, 0, 0, 0 });
            //
            // lblKinhDo
            //
            this.lblKinhDo.AutoSize = true;
            this.lblKinhDo.Location = new System.Drawing.Point(160, 328);
            this.lblKinhDo.Name = "lblKinhDo";
            this.lblKinhDo.Text = "Kinh độ";
            //
            // nudKinhDo
            //
            this.nudKinhDo.DecimalPlaces = 7;
            this.nudKinhDo.Increment = new decimal(new int[] { 1, 0, 0, 262144 });
            this.nudKinhDo.Location = new System.Drawing.Point(160, 348);
            this.nudKinhDo.Maximum = new decimal(new int[] { 107, 0, 0, 0 });
            this.nudKinhDo.Minimum = new decimal(new int[] { 105, 0, 0, 0 });
            this.nudKinhDo.Name = "nudKinhDo";
            this.nudKinhDo.Size = new System.Drawing.Size(130, 25);
            this.nudKinhDo.TabIndex = 6;
            this.nudKinhDo.Value = new decimal(new int[] { 1058, 0, 0, 65536 });
            //
            // btnChonViTri
            //
            this.btnChonViTri.Location = new System.Drawing.Point(300, 344);
            this.btnChonViTri.Name = "btnChonViTri";
            this.btnChonViTri.Size = new System.Drawing.Size(140, 32);
            this.btnChonViTri.TabIndex = 7;
            this.btnChonViTri.Text = "Chọn trên bản đồ";
            //
            // lblLoiViTri
            //
            this.lblLoiViTri.AutoSize = true;
            this.lblLoiViTri.Location = new System.Drawing.Point(20, 378);
            this.lblLoiViTri.Name = "lblLoiViTri";
            this.lblLoiViTri.Visible = false;
            //
            // btnLuu
            //
            this.btnLuu.Location = new System.Drawing.Point(240, 412);
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.Size = new System.Drawing.Size(200, 38);
            this.btnLuu.TabIndex = 8;
            this.btnLuu.Text = "Lưu thay đổi";
            //
            // btnHuy
            //
            this.btnHuy.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnHuy.Location = new System.Drawing.Point(120, 412);
            this.btnHuy.Name = "btnHuy";
            this.btnHuy.Size = new System.Drawing.Size(110, 38);
            this.btnHuy.TabIndex = 9;
            this.btnHuy.Text = "Huỷ";
            //
            // FrmCapNhatTram
            //
            this.AcceptButton = this.btnLuu;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnHuy;
            this.ClientSize = new System.Drawing.Size(462, 470);
            this.Controls.Add(this.lblTieuDe);
            this.Controls.Add(this.lblMa);
            this.Controls.Add(this.txtMa);
            this.Controls.Add(this.lblTrangThai);
            this.Controls.Add(this.cboTrangThai);
            this.Controls.Add(this.lblTen);
            this.Controls.Add(this.txtTen);
            this.Controls.Add(this.lblLoiTen);
            this.Controls.Add(this.lblLoai);
            this.Controls.Add(this.cboLoai);
            this.Controls.Add(this.lblTuyen);
            this.Controls.Add(this.cboTuyen);
            this.Controls.Add(this.lblPhuongTien);
            this.Controls.Add(this.cboPhuongTien);
            this.Controls.Add(this.lblViTri);
            this.Controls.Add(this.lblViDo);
            this.Controls.Add(this.nudViDo);
            this.Controls.Add(this.lblKinhDo);
            this.Controls.Add(this.nudKinhDo);
            this.Controls.Add(this.btnChonViTri);
            this.Controls.Add(this.lblLoiViTri);
            this.Controls.Add(this.btnLuu);
            this.Controls.Add(this.btnHuy);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmCapNhatTram";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cập nhật trạm xe buýt";
            ((System.ComponentModel.ISupportInitialize)(this.nudViDo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudKinhDo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTieuDe;
        private System.Windows.Forms.Label lblMa;
        private System.Windows.Forms.TextBox txtMa;
        private System.Windows.Forms.Label lblTrangThai;
        private System.Windows.Forms.ComboBox cboTrangThai;
        private System.Windows.Forms.Label lblTen;
        private System.Windows.Forms.TextBox txtTen;
        private System.Windows.Forms.Label lblLoiTen;
        private System.Windows.Forms.Label lblLoai;
        private System.Windows.Forms.ComboBox cboLoai;
        private System.Windows.Forms.Label lblTuyen;
        private System.Windows.Forms.ComboBox cboTuyen;
        private System.Windows.Forms.Label lblPhuongTien;
        private System.Windows.Forms.ComboBox cboPhuongTien;
        private System.Windows.Forms.Label lblViTri;
        private System.Windows.Forms.Label lblViDo;
        private System.Windows.Forms.NumericUpDown nudViDo;
        private System.Windows.Forms.Label lblKinhDo;
        private System.Windows.Forms.NumericUpDown nudKinhDo;
        private System.Windows.Forms.Button btnChonViTri;
        private System.Windows.Forms.Label lblLoiViTri;
        private System.Windows.Forms.Button btnLuu;
        private System.Windows.Forms.Button btnHuy;
    }
}
