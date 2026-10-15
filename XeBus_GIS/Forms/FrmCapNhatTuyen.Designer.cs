namespace XeBus_GIS.Forms
{
    partial class FrmCapNhatTuyen
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
            this.lblTieuDe = new System.Windows.Forms.Label();
            this.lblMa = new System.Windows.Forms.Label();
            this.txtMa = new System.Windows.Forms.TextBox();
            this.lblTrangThai = new System.Windows.Forms.Label();
            this.cboTrangThai = new System.Windows.Forms.ComboBox();
            this.lblTen = new System.Windows.Forms.Label();
            this.txtTen = new System.Windows.Forms.TextBox();
            this.lblLoiTen = new System.Windows.Forms.Label();
            this.lblMoTa = new System.Windows.Forms.Label();
            this.txtMoTa = new System.Windows.Forms.TextBox();
            this.lblThongTin = new System.Windows.Forms.Label();
            this.btnLuu = new System.Windows.Forms.Button();
            this.btnHuy = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblTieuDe
            //
            this.lblTieuDe.AutoSize = true;
            this.lblTieuDe.Location = new System.Drawing.Point(20, 16);
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Text = "Cập nhật thông tin thuộc tính tuyến";
            //
            // lblMa
            //
            this.lblMa.AutoSize = true;
            this.lblMa.Location = new System.Drawing.Point(20, 56);
            this.lblMa.Name = "lblMa";
            this.lblMa.Text = "Mã tuyến";
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
            this.lblTen.Text = "Tên tuyến *";
            //
            // txtTen
            //
            this.txtTen.Location = new System.Drawing.Point(20, 132);
            this.txtTen.MaxLength = 100;
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
            // lblMoTa
            //
            this.lblMoTa.AutoSize = true;
            this.lblMoTa.Location = new System.Drawing.Point(20, 184);
            this.lblMoTa.Name = "lblMoTa";
            this.lblMoTa.Text = "Mô tả (lộ trình, ghi chú) – tối đa 500 ký tự";
            //
            // txtMoTa
            //
            this.txtMoTa.Location = new System.Drawing.Point(20, 204);
            this.txtMoTa.MaxLength = 500;
            this.txtMoTa.Multiline = true;
            this.txtMoTa.Name = "txtMoTa";
            this.txtMoTa.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMoTa.Size = new System.Drawing.Size(420, 96);
            this.txtMoTa.TabIndex = 2;
            //
            // lblThongTin
            //
            this.lblThongTin.Location = new System.Drawing.Point(20, 310);
            this.lblThongTin.Name = "lblThongTin";
            this.lblThongTin.Size = new System.Drawing.Size(420, 40);
            //
            // btnLuu
            //
            this.btnLuu.Location = new System.Drawing.Point(240, 360);
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.Size = new System.Drawing.Size(200, 38);
            this.btnLuu.TabIndex = 3;
            this.btnLuu.Text = "Lưu thay đổi";
            //
            // btnHuy
            //
            this.btnHuy.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnHuy.Location = new System.Drawing.Point(120, 360);
            this.btnHuy.Name = "btnHuy";
            this.btnHuy.Size = new System.Drawing.Size(110, 38);
            this.btnHuy.TabIndex = 4;
            this.btnHuy.Text = "Huỷ";
            //
            // FrmCapNhatTuyen
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;   // co giãn DPI bằng UiHelper.CoGianDpi (xem ctor)
            this.CancelButton = this.btnHuy;
            this.ClientSize = new System.Drawing.Size(462, 418);
            this.Controls.Add(this.lblTieuDe);
            this.Controls.Add(this.lblMa);
            this.Controls.Add(this.txtMa);
            this.Controls.Add(this.lblTrangThai);
            this.Controls.Add(this.cboTrangThai);
            this.Controls.Add(this.lblTen);
            this.Controls.Add(this.txtTen);
            this.Controls.Add(this.lblLoiTen);
            this.Controls.Add(this.lblMoTa);
            this.Controls.Add(this.txtMoTa);
            this.Controls.Add(this.lblThongTin);
            this.Controls.Add(this.btnLuu);
            this.Controls.Add(this.btnHuy);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmCapNhatTuyen";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cập nhật tuyến xe buýt";
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
        private System.Windows.Forms.Label lblMoTa;
        private System.Windows.Forms.TextBox txtMoTa;
        private System.Windows.Forms.Label lblThongTin;
        private System.Windows.Forms.Button btnLuu;
        private System.Windows.Forms.Button btnHuy;
    }
}
