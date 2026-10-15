namespace XeBus_GIS.Forms
{
    partial class FrmDangNhap
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
            this.nenVideo = new XeBus_GIS.UI.Controls.NenVideo();
            this.pnlForm = new System.Windows.Forms.Panel();
            this.pnlKhung = new System.Windows.Forms.Panel();
            this.lblDangNhap = new System.Windows.Forms.Label();
            this.lblHuongDan = new System.Windows.Forms.Label();
            this.lblTenDN = new System.Windows.Forms.Label();
            this.txtTenDN = new System.Windows.Forms.TextBox();
            this.lblMatKhau = new System.Windows.Forms.Label();
            this.txtMatKhau = new System.Windows.Forms.TextBox();
            this.btnHienMK = new System.Windows.Forms.Button();
            this.lblLoi = new System.Windows.Forms.Label();
            this.btnDangNhap = new System.Windows.Forms.Button();
            this.lnkKhach = new System.Windows.Forms.LinkLabel();
            this.lblKetNoi = new System.Windows.Forms.Label();
            this.lnkDangKy = new System.Windows.Forms.LinkLabel();
            this.ucDangKy = new XeBus_GIS.Forms.UcDangKy();
            this.pnlForm.SuspendLayout();
            this.pnlKhung.SuspendLayout();
            this.SuspendLayout();
            //
            // nenVideo (hình nền động + giới thiệu hệ thống, bên trái)
            //
            this.nenVideo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nenVideo.Name = "nenVideo";
            this.nenVideo.TabIndex = 0;
            this.nenVideo.TabStop = false;
            //
            // pnlForm (cột đăng nhập bên phải)
            //
            this.pnlForm.AutoScroll = true;
            this.pnlForm.Controls.Add(this.pnlKhung);
            this.pnlForm.Controls.Add(this.ucDangKy);
            this.pnlForm.Controls.Add(this.lblKetNoi);
            this.pnlForm.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlForm.Name = "pnlForm";
            this.pnlForm.Size = new System.Drawing.Size(480, 800);
            this.pnlForm.TabIndex = 1;
            //
            // pnlKhung (khối form, luôn căn giữa cột phải)
            //
            this.pnlKhung.Controls.Add(this.lblDangNhap);
            this.pnlKhung.Controls.Add(this.lblHuongDan);
            this.pnlKhung.Controls.Add(this.lblTenDN);
            this.pnlKhung.Controls.Add(this.txtTenDN);
            this.pnlKhung.Controls.Add(this.lblMatKhau);
            this.pnlKhung.Controls.Add(this.txtMatKhau);
            this.pnlKhung.Controls.Add(this.btnHienMK);
            this.pnlKhung.Controls.Add(this.lblLoi);
            this.pnlKhung.Controls.Add(this.btnDangNhap);
            this.pnlKhung.Controls.Add(this.lnkKhach);
            this.pnlKhung.Controls.Add(this.lnkDangKy);
            this.pnlKhung.Location = new System.Drawing.Point(60, 170);
            this.pnlKhung.Name = "pnlKhung";
            this.pnlKhung.Size = new System.Drawing.Size(360, 404);
            this.pnlKhung.TabIndex = 0;
            //
            // lblDangNhap
            //
            this.lblDangNhap.AutoSize = true;
            this.lblDangNhap.Location = new System.Drawing.Point(-3, 0);
            this.lblDangNhap.Name = "lblDangNhap";
            this.lblDangNhap.Text = "Đăng nhập";
            //
            // lblHuongDan
            //
            this.lblHuongDan.Location = new System.Drawing.Point(0, 46);
            this.lblHuongDan.Name = "lblHuongDan";
            this.lblHuongDan.Size = new System.Drawing.Size(360, 22);
            this.lblHuongDan.Text = "Dùng tài khoản do quản trị viên cấp.";
            //
            // lblTenDN
            //
            this.lblTenDN.AutoSize = true;
            this.lblTenDN.Location = new System.Drawing.Point(0, 96);
            this.lblTenDN.Name = "lblTenDN";
            this.lblTenDN.TabIndex = 1;
            this.lblTenDN.Text = "Tên đăng nhập";
            //
            // txtTenDN
            //
            this.txtTenDN.Location = new System.Drawing.Point(0, 118);
            this.txtTenDN.Name = "txtTenDN";
            this.txtTenDN.Size = new System.Drawing.Size(360, 27);
            this.txtTenDN.TabIndex = 2;
            //
            // lblMatKhau
            //
            this.lblMatKhau.AutoSize = true;
            this.lblMatKhau.Location = new System.Drawing.Point(0, 170);
            this.lblMatKhau.Name = "lblMatKhau";
            this.lblMatKhau.TabIndex = 3;
            this.lblMatKhau.Text = "Mật khẩu";
            //
            // txtMatKhau
            //
            this.txtMatKhau.Location = new System.Drawing.Point(0, 192);
            this.txtMatKhau.Name = "txtMatKhau";
            this.txtMatKhau.Size = new System.Drawing.Size(360, 27);
            this.txtMatKhau.TabIndex = 4;
            this.txtMatKhau.UseSystemPasswordChar = true;
            //
            // btnHienMK
            //
            this.btnHienMK.Location = new System.Drawing.Point(0, 192);
            this.btnHienMK.Name = "btnHienMK";
            this.btnHienMK.Size = new System.Drawing.Size(36, 27);
            this.btnHienMK.TabIndex = 5;
            this.btnHienMK.TabStop = false;
            //
            // lblLoi
            //
            this.lblLoi.Location = new System.Drawing.Point(0, 236);
            this.lblLoi.Name = "lblLoi";
            this.lblLoi.Size = new System.Drawing.Size(360, 40);
            this.lblLoi.TabIndex = 6;
            this.lblLoi.Visible = false;
            //
            // btnDangNhap
            //
            this.btnDangNhap.Location = new System.Drawing.Point(0, 282);
            this.btnDangNhap.Name = "btnDangNhap";
            this.btnDangNhap.Size = new System.Drawing.Size(360, 46);
            this.btnDangNhap.TabIndex = 7;
            this.btnDangNhap.Text = "Đăng nhập";
            //
            // lnkKhach
            //
            this.lnkKhach.Location = new System.Drawing.Point(0, 342);
            this.lnkKhach.Name = "lnkKhach";
            this.lnkKhach.Size = new System.Drawing.Size(360, 24);
            this.lnkKhach.TabIndex = 8;
            this.lnkKhach.TabStop = true;
            this.lnkKhach.Text = "Tiếp tục với tư cách khách (chỉ xem, tra cứu)";
            this.lnkKhach.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lnkDangKy
            //
            this.lnkDangKy.Location = new System.Drawing.Point(0, 374);
            this.lnkDangKy.Name = "lnkDangKy";
            this.lnkDangKy.Size = new System.Drawing.Size(360, 24);
            this.lnkDangKy.TabIndex = 9;
            this.lnkDangKy.TabStop = true;
            this.lnkDangKy.Text = "Chưa có tài khoản? Đăng ký";
            this.lnkDangKy.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // ucDangKy (đăng ký – ẩn cho tới khi bấm "Đăng ký")
            //
            this.ucDangKy.Location = new System.Drawing.Point(60, 130);
            this.ucDangKy.Name = "ucDangKy";
            this.ucDangKy.Size = new System.Drawing.Size(360, 564);
            this.ucDangKy.TabIndex = 1;
            this.ucDangKy.Visible = false;
            //
            // lblKetNoi
            //
            this.lblKetNoi.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblKetNoi.Name = "lblKetNoi";
            this.lblKetNoi.Size = new System.Drawing.Size(480, 48);
            this.lblKetNoi.TabIndex = 10;
            this.lblKetNoi.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // FrmDangNhap
            //
            this.AcceptButton = this.btnDangNhap;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;   // co giãn DPI bằng UiHelper.CoGianDpi (xem ctor)
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.Controls.Add(this.nenVideo);
            this.Controls.Add(this.pnlForm);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.MinimumSize = new System.Drawing.Size(1024, 680);
            this.Name = "FrmDangNhap";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Đăng nhập – XeBus GIS";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.pnlForm.ResumeLayout(false);
            this.pnlKhung.ResumeLayout(false);
            this.pnlKhung.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private XeBus_GIS.UI.Controls.NenVideo nenVideo;
        private System.Windows.Forms.Panel pnlForm;
        private System.Windows.Forms.Panel pnlKhung;
        private System.Windows.Forms.Label lblDangNhap;
        private System.Windows.Forms.Label lblHuongDan;
        private System.Windows.Forms.Label lblTenDN;
        private System.Windows.Forms.TextBox txtTenDN;
        private System.Windows.Forms.Label lblMatKhau;
        private System.Windows.Forms.TextBox txtMatKhau;
        private System.Windows.Forms.Button btnHienMK;
        private System.Windows.Forms.Label lblLoi;
        private System.Windows.Forms.Button btnDangNhap;
        private System.Windows.Forms.LinkLabel lnkKhach;
        private System.Windows.Forms.Label lblKetNoi;
        private System.Windows.Forms.LinkLabel lnkDangKy;
        private XeBus_GIS.Forms.UcDangKy ucDangKy;
    }
}
