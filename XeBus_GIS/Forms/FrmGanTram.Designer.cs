namespace XeBus_GIS.Forms
{
    partial class FrmGanTram
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
            this.lblGoiY = new System.Windows.Forms.Label();
            this.lblNguon = new System.Windows.Forms.Label();
            this.txtTim = new System.Windows.Forms.TextBox();
            this.btnTim = new System.Windows.Forms.Button();
            this.dgvNguon = new System.Windows.Forms.DataGridView();
            this.btnThem = new System.Windows.Forms.Button();
            this.btnBo = new System.Windows.Forms.Button();
            this.lblDich = new System.Windows.Forms.Label();
            this.dgvDich = new System.Windows.Forms.DataGridView();
            this.btnLen = new System.Windows.Forms.Button();
            this.btnXuong = new System.Windows.Forms.Button();
            this.chkDungLai = new System.Windows.Forms.CheckBox();
            this.lblThayDoi = new System.Windows.Forms.Label();
            this.btnHuy = new System.Windows.Forms.Button();
            this.btnLuu = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNguon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDich)).BeginInit();
            this.SuspendLayout();
            //
            // lblTieuDe
            //
            this.lblTieuDe.AutoSize = true;
            this.lblTieuDe.Location = new System.Drawing.Point(20, 14);
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Text = "Gán trạm vào tuyến";
            //
            // lblGoiY
            //
            this.lblGoiY.Location = new System.Drawing.Point(20, 42);
            this.lblGoiY.Name = "lblGoiY";
            this.lblGoiY.Size = new System.Drawing.Size(820, 22);
            this.lblGoiY.Text = "Click trạm trên bản đồ để thêm vào cuối tuyến, hoặc tìm ở danh sách bên trái. Dùng nút lên/xuống để sắp xếp thứ tự.";
            //
            // lblNguon
            //
            this.lblNguon.AutoSize = true;
            this.lblNguon.Location = new System.Drawing.Point(20, 72);
            this.lblNguon.Name = "lblNguon";
            this.lblNguon.Text = "Tìm trạm (tên / mã, không dấu)";
            //
            // txtTim
            //
            this.txtTim.Location = new System.Drawing.Point(20, 94);
            this.txtTim.Name = "txtTim";
            this.txtTim.Size = new System.Drawing.Size(262, 25);
            this.txtTim.TabIndex = 0;
            //
            // btnTim
            //
            this.btnTim.Location = new System.Drawing.Point(288, 91);
            this.btnTim.Name = "btnTim";
            this.btnTim.Size = new System.Drawing.Size(92, 31);
            this.btnTim.TabIndex = 1;
            this.btnTim.Text = "Tìm";
            //
            // dgvNguon
            //
            this.dgvNguon.Location = new System.Drawing.Point(20, 130);
            this.dgvNguon.Name = "dgvNguon";
            this.dgvNguon.Size = new System.Drawing.Size(360, 336);
            this.dgvNguon.TabIndex = 2;
            //
            // btnThem
            //
            this.btnThem.Location = new System.Drawing.Point(394, 220);
            this.btnThem.Name = "btnThem";
            this.btnThem.Size = new System.Drawing.Size(52, 42);
            this.btnThem.TabIndex = 3;
            //
            // btnBo
            //
            this.btnBo.Location = new System.Drawing.Point(394, 270);
            this.btnBo.Name = "btnBo";
            this.btnBo.Size = new System.Drawing.Size(52, 42);
            this.btnBo.TabIndex = 4;
            //
            // lblDich
            //
            this.lblDich.AutoSize = true;
            this.lblDich.Location = new System.Drawing.Point(460, 72);
            this.lblDich.Name = "lblDich";
            this.lblDich.Text = "Trạm trên tuyến";
            //
            // dgvDich
            //
            this.dgvDich.Location = new System.Drawing.Point(460, 94);
            this.dgvDich.Name = "dgvDich";
            this.dgvDich.Size = new System.Drawing.Size(330, 372);
            this.dgvDich.TabIndex = 5;
            //
            // btnLen
            //
            this.btnLen.Location = new System.Drawing.Point(798, 220);
            this.btnLen.Name = "btnLen";
            this.btnLen.Size = new System.Drawing.Size(42, 42);
            this.btnLen.TabIndex = 6;
            //
            // btnXuong
            //
            this.btnXuong.Location = new System.Drawing.Point(798, 270);
            this.btnXuong.Name = "btnXuong";
            this.btnXuong.Size = new System.Drawing.Size(42, 42);
            this.btnXuong.TabIndex = 7;
            //
            // chkDungLai
            //
            this.chkDungLai.AutoSize = true;
            this.chkDungLai.Checked = true;
            this.chkDungLai.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDungLai.Location = new System.Drawing.Point(20, 482);
            this.chkDungLai.Name = "chkDungLai";
            this.chkDungLai.TabIndex = 8;
            this.chkDungLai.Text = "Dựng lại đường đi của tuyến theo mạng đường khi lưu";
            //
            // lblThayDoi
            //
            this.lblThayDoi.Location = new System.Drawing.Point(20, 508);
            this.lblThayDoi.Name = "lblThayDoi";
            this.lblThayDoi.Size = new System.Drawing.Size(500, 22);
            //
            // btnHuy
            //
            this.btnHuy.Location = new System.Drawing.Point(560, 492);
            this.btnHuy.Name = "btnHuy";
            this.btnHuy.Size = new System.Drawing.Size(110, 38);
            this.btnHuy.TabIndex = 10;
            this.btnHuy.Text = "Đóng";
            //
            // btnLuu
            //
            this.btnLuu.Location = new System.Drawing.Point(680, 492);
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.Size = new System.Drawing.Size(160, 38);
            this.btnLuu.TabIndex = 9;
            this.btnLuu.Text = "Lưu danh sách trạm";
            //
            // FrmGanTram
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(860, 548);
            this.Controls.Add(this.lblTieuDe);
            this.Controls.Add(this.lblGoiY);
            this.Controls.Add(this.lblNguon);
            this.Controls.Add(this.txtTim);
            this.Controls.Add(this.btnTim);
            this.Controls.Add(this.dgvNguon);
            this.Controls.Add(this.btnThem);
            this.Controls.Add(this.btnBo);
            this.Controls.Add(this.lblDich);
            this.Controls.Add(this.dgvDich);
            this.Controls.Add(this.btnLen);
            this.Controls.Add(this.btnXuong);
            this.Controls.Add(this.chkDungLai);
            this.Controls.Add(this.lblThayDoi);
            this.Controls.Add(this.btnHuy);
            this.Controls.Add(this.btnLuu);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmGanTram";
            this.ShowInTaskbar = false;
            this.Text = "Gán trạm vào tuyến";
            ((System.ComponentModel.ISupportInitialize)(this.dgvNguon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDich)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTieuDe;
        private System.Windows.Forms.Label lblGoiY;
        private System.Windows.Forms.Label lblNguon;
        private System.Windows.Forms.TextBox txtTim;
        private System.Windows.Forms.Button btnTim;
        private System.Windows.Forms.DataGridView dgvNguon;
        private System.Windows.Forms.Button btnThem;
        private System.Windows.Forms.Button btnBo;
        private System.Windows.Forms.Label lblDich;
        private System.Windows.Forms.DataGridView dgvDich;
        private System.Windows.Forms.Button btnLen;
        private System.Windows.Forms.Button btnXuong;
        private System.Windows.Forms.CheckBox chkDungLai;
        private System.Windows.Forms.Label lblThayDoi;
        private System.Windows.Forms.Button btnHuy;
        private System.Windows.Forms.Button btnLuu;
    }
}
