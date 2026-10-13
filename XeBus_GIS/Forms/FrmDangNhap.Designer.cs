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
            this.pnlBrand = new System.Windows.Forms.Panel();
            this.lblLogo = new System.Windows.Forms.Label();
            this.lblTenApp = new System.Windows.Forms.Label();
            this.lblMoTa = new System.Windows.Forms.Label();
            this.lblTinhNang = new System.Windows.Forms.Label();
            this.lblNguon = new System.Windows.Forms.Label();
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
            this.pnlBrand.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlBrand (dải nhận diện bên trái)
            //
            this.pnlBrand.Controls.Add(this.lblLogo);
            this.pnlBrand.Controls.Add(this.lblTenApp);
            this.pnlBrand.Controls.Add(this.lblMoTa);
            this.pnlBrand.Controls.Add(this.lblTinhNang);
            this.pnlBrand.Controls.Add(this.lblNguon);
            this.pnlBrand.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlBrand.Name = "pnlBrand";
            this.pnlBrand.Size = new System.Drawing.Size(320, 480);
            this.pnlBrand.TabIndex = 0;
            //
            // lblLogo
            //
            this.lblLogo.Location = new System.Drawing.Point(32, 40);
            this.lblLogo.Name = "lblLogo";
            this.lblLogo.Size = new System.Drawing.Size(52, 52);
            this.lblLogo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblTenApp
            //
            this.lblTenApp.Location = new System.Drawing.Point(32, 108);
            this.lblTenApp.Name = "lblTenApp";
            this.lblTenApp.Size = new System.Drawing.Size(260, 34);
            this.lblTenApp.Text = "XeBus GIS";
            //
            // lblMoTa
            //
            this.lblMoTa.Location = new System.Drawing.Point(32, 144);
            this.lblMoTa.Name = "lblMoTa";
            this.lblMoTa.Size = new System.Drawing.Size(260, 64);
            this.lblMoTa.Text = "Hệ thống GIS hỗ trợ quản lý, phân tích và thiết lập trạm xe buýt – Thành phố Hà Nội";
            //
            // lblTinhNang
            //
            this.lblTinhNang.Location = new System.Drawing.Point(32, 224);
            this.lblTinhNang.Name = "lblTinhNang";
            this.lblTinhNang.Size = new System.Drawing.Size(260, 150);
            //
            // lblNguon
            //
            this.lblNguon.Location = new System.Drawing.Point(32, 426);
            this.lblNguon.Name = "lblNguon";
            this.lblNguon.Size = new System.Drawing.Size(260, 36);
            this.lblNguon.Text = "Nhóm 5 · Dữ liệu: CSDLKG (PostgreSQL/PostGIS)";
            //
            // lblDangNhap
            //
            this.lblDangNhap.AutoSize = true;
            this.lblDangNhap.Location = new System.Drawing.Point(360, 40);
            this.lblDangNhap.Name = "lblDangNhap";
            this.lblDangNhap.Text = "Đăng nhập";
            //
            // lblHuongDan
            //
            this.lblHuongDan.Location = new System.Drawing.Point(361, 78);
            this.lblHuongDan.Name = "lblHuongDan";
            this.lblHuongDan.Size = new System.Drawing.Size(320, 22);
            this.lblHuongDan.Text = "Dùng tài khoản do quản trị viên cấp.";
            //
            // lblTenDN
            //
            this.lblTenDN.AutoSize = true;
            this.lblTenDN.Location = new System.Drawing.Point(360, 120);
            this.lblTenDN.Name = "lblTenDN";
            this.lblTenDN.TabIndex = 3;
            this.lblTenDN.Text = "Tên đăng nhập";
            //
            // txtTenDN
            //
            this.txtTenDN.Location = new System.Drawing.Point(360, 142);
            this.txtTenDN.Name = "txtTenDN";
            this.txtTenDN.Size = new System.Drawing.Size(320, 25);
            this.txtTenDN.TabIndex = 4;
            //
            // lblMatKhau
            //
            this.lblMatKhau.AutoSize = true;
            this.lblMatKhau.Location = new System.Drawing.Point(360, 184);
            this.lblMatKhau.Name = "lblMatKhau";
            this.lblMatKhau.TabIndex = 5;
            this.lblMatKhau.Text = "Mật khẩu";
            //
            // txtMatKhau
            //
            this.txtMatKhau.Location = new System.Drawing.Point(360, 206);
            this.txtMatKhau.Name = "txtMatKhau";
            this.txtMatKhau.Size = new System.Drawing.Size(282, 25);
            this.txtMatKhau.TabIndex = 6;
            this.txtMatKhau.UseSystemPasswordChar = true;
            //
            // btnHienMK
            //
            this.btnHienMK.Location = new System.Drawing.Point(646, 205);
            this.btnHienMK.Name = "btnHienMK";
            this.btnHienMK.Size = new System.Drawing.Size(34, 27);
            this.btnHienMK.TabIndex = 7;
            this.btnHienMK.TabStop = false;
            //
            // lblLoi
            //
            this.lblLoi.Location = new System.Drawing.Point(360, 240);
            this.lblLoi.Name = "lblLoi";
            this.lblLoi.Size = new System.Drawing.Size(320, 40);
            this.lblLoi.TabIndex = 8;
            this.lblLoi.Visible = false;
            //
            // btnDangNhap
            //
            this.btnDangNhap.Location = new System.Drawing.Point(360, 286);
            this.btnDangNhap.Name = "btnDangNhap";
            this.btnDangNhap.Size = new System.Drawing.Size(320, 42);
            this.btnDangNhap.TabIndex = 9;
            this.btnDangNhap.Text = "Đăng nhập";
            //
            // lnkKhach
            //
            this.lnkKhach.Location = new System.Drawing.Point(360, 340);
            this.lnkKhach.Name = "lnkKhach";
            this.lnkKhach.Size = new System.Drawing.Size(320, 24);
            this.lnkKhach.TabIndex = 10;
            this.lnkKhach.TabStop = true;
            this.lnkKhach.Text = "Tiếp tục với tư cách khách (chỉ xem, tra cứu)";
            this.lnkKhach.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblKetNoi
            //
            this.lblKetNoi.Location = new System.Drawing.Point(360, 430);
            this.lblKetNoi.Name = "lblKetNoi";
            this.lblKetNoi.Size = new System.Drawing.Size(320, 22);
            this.lblKetNoi.TabIndex = 11;
            this.lblKetNoi.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // FrmDangNhap
            //
            this.AcceptButton = this.btnDangNhap;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(720, 480);
            this.Controls.Add(this.lblDangNhap);
            this.Controls.Add(this.lblHuongDan);
            this.Controls.Add(this.lblTenDN);
            this.Controls.Add(this.txtTenDN);
            this.Controls.Add(this.lblMatKhau);
            this.Controls.Add(this.txtMatKhau);
            this.Controls.Add(this.btnHienMK);
            this.Controls.Add(this.lblLoi);
            this.Controls.Add(this.btnDangNhap);
            this.Controls.Add(this.lnkKhach);
            this.Controls.Add(this.lblKetNoi);
            this.Controls.Add(this.pnlBrand);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmDangNhap";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Đăng nhập - XeBus GIS";
            this.pnlBrand.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel pnlBrand;
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.Label lblTenApp;
        private System.Windows.Forms.Label lblMoTa;
        private System.Windows.Forms.Label lblTinhNang;
        private System.Windows.Forms.Label lblNguon;
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
    }
}
