namespace XeBus_GIS.Forms
{
    partial class FrmChonViTri
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
            this.lblMoTa = new System.Windows.Forms.Label();
            this.cboPhuong = new System.Windows.Forms.ComboBox();
            this.lblViDo = new System.Windows.Forms.Label();
            this.txtViDo = new System.Windows.Forms.TextBox();
            this.lblKinhDo = new System.Windows.Forms.Label();
            this.txtKinhDo = new System.Windows.Forms.TextBox();
            this.lblLoi = new System.Windows.Forms.Label();
            this.btnHuy = new System.Windows.Forms.Button();
            this.btnDongY = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblTieuDe
            //
            this.lblTieuDe.AutoSize = true;
            this.lblTieuDe.Location = new System.Drawing.Point(20, 18);
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Text = "Chọn vị trí";
            //
            // lblMoTa
            //
            this.lblMoTa.Location = new System.Drawing.Point(20, 46);
            this.lblMoTa.Name = "lblMoTa";
            this.lblMoTa.Size = new System.Drawing.Size(380, 38);
            this.lblMoTa.Text = "";
            //
            // cboPhuong
            //
            this.cboPhuong.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboPhuong.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboPhuong.Location = new System.Drawing.Point(20, 92);
            this.cboPhuong.Name = "cboPhuong";
            this.cboPhuong.Size = new System.Drawing.Size(380, 25);
            this.cboPhuong.TabIndex = 0;
            //
            // lblViDo
            //
            this.lblViDo.AutoSize = true;
            this.lblViDo.Location = new System.Drawing.Point(20, 92);
            this.lblViDo.Name = "lblViDo";
            this.lblViDo.Text = "Vĩ độ (VD 21.0285)";
            //
            // txtViDo
            //
            this.txtViDo.Location = new System.Drawing.Point(20, 114);
            this.txtViDo.Name = "txtViDo";
            this.txtViDo.Size = new System.Drawing.Size(180, 25);
            this.txtViDo.TabIndex = 1;
            //
            // lblKinhDo
            //
            this.lblKinhDo.AutoSize = true;
            this.lblKinhDo.Location = new System.Drawing.Point(220, 92);
            this.lblKinhDo.Name = "lblKinhDo";
            this.lblKinhDo.Text = "Kinh độ (VD 105.8542)";
            //
            // txtKinhDo
            //
            this.txtKinhDo.Location = new System.Drawing.Point(220, 114);
            this.txtKinhDo.Name = "txtKinhDo";
            this.txtKinhDo.Size = new System.Drawing.Size(180, 25);
            this.txtKinhDo.TabIndex = 2;
            //
            // lblLoi
            //
            this.lblLoi.Location = new System.Drawing.Point(20, 148);
            this.lblLoi.Name = "lblLoi";
            this.lblLoi.Size = new System.Drawing.Size(380, 20);
            this.lblLoi.Text = "";
            //
            // btnHuy
            //
            this.btnHuy.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnHuy.Location = new System.Drawing.Point(196, 178);
            this.btnHuy.Name = "btnHuy";
            this.btnHuy.Size = new System.Drawing.Size(96, 34);
            this.btnHuy.TabIndex = 4;
            this.btnHuy.Text = "Huỷ";
            //
            // btnDongY
            //
            this.btnDongY.Location = new System.Drawing.Point(304, 178);
            this.btnDongY.Name = "btnDongY";
            this.btnDongY.Size = new System.Drawing.Size(96, 34);
            this.btnDongY.TabIndex = 3;
            this.btnDongY.Text = "Đồng ý";
            //
            // FrmChonViTri
            //
            this.AcceptButton = this.btnDongY;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnHuy;
            this.ClientSize = new System.Drawing.Size(420, 230);
            this.Controls.Add(this.lblTieuDe);
            this.Controls.Add(this.lblMoTa);
            this.Controls.Add(this.cboPhuong);
            this.Controls.Add(this.lblViDo);
            this.Controls.Add(this.txtViDo);
            this.Controls.Add(this.lblKinhDo);
            this.Controls.Add(this.txtKinhDo);
            this.Controls.Add(this.lblLoi);
            this.Controls.Add(this.btnHuy);
            this.Controls.Add(this.btnDongY);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmChonViTri";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Vị trí của tôi";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTieuDe;
        private System.Windows.Forms.Label lblMoTa;
        private System.Windows.Forms.ComboBox cboPhuong;
        private System.Windows.Forms.Label lblViDo;
        private System.Windows.Forms.TextBox txtViDo;
        private System.Windows.Forms.Label lblKinhDo;
        private System.Windows.Forms.TextBox txtKinhDo;
        private System.Windows.Forms.Label lblLoi;
        private System.Windows.Forms.Button btnHuy;
        private System.Windows.Forms.Button btnDongY;
    }
}
