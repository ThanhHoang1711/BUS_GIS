namespace XeBus_GIS.Forms
{
    partial class UcQuanTri
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
            this.tab = new System.Windows.Forms.TabControl();
            this.tpNguoiDung = new System.Windows.Forms.TabPage();
            this.lblTim = new System.Windows.Forms.Label();
            this.txtTim = new System.Windows.Forms.TextBox();
            this.btnTimND = new System.Windows.Forms.Button();
            this.dgvND = new System.Windows.Forms.DataGridView();
            this.pnlSua = new System.Windows.Forms.Panel();
            this.lblHdrSua = new System.Windows.Forms.Label();
            this.lblTenDN = new System.Windows.Forms.Label();
            this.txtTenDN = new System.Windows.Forms.TextBox();
            this.lblHoTen = new System.Windows.Forms.Label();
            this.txtHoTen = new System.Windows.Forms.TextBox();
            this.lblChucVu = new System.Windows.Forms.Label();
            this.txtChucVu = new System.Windows.Forms.TextBox();
            this.lblDonVi = new System.Windows.Forms.Label();
            this.txtDonVi = new System.Windows.Forms.TextBox();
            this.lblNhom = new System.Windows.Forms.Label();
            this.cboNhom = new System.Windows.Forms.ComboBox();
            this.lblTrangThaiND = new System.Windows.Forms.Label();
            this.cboTrangThaiND = new System.Windows.Forms.ComboBox();
            this.lblMatKhau = new System.Windows.Forms.Label();
            this.txtMatKhau = new System.Windows.Forms.TextBox();
            this.lblXacNhan = new System.Windows.Forms.Label();
            this.txtXacNhan = new System.Windows.Forms.TextBox();
            this.lblGhiChuMK = new System.Windows.Forms.Label();
            this.lblLoiND = new System.Windows.Forms.Label();
            this.btnMoi = new System.Windows.Forms.Button();
            this.btnLuuND = new System.Windows.Forms.Button();
            this.btnDatLaiMK = new System.Windows.Forms.Button();
            this.btnXoaND = new System.Windows.Forms.Button();
            this.tpPhanQuyen = new System.Windows.Forms.TabPage();
            this.lblNhomHdr = new System.Windows.Forms.Label();
            this.lstNhom = new System.Windows.Forms.ListBox();
            this.lblMoTaNhom = new System.Windows.Forms.Label();
            this.dgvQuyen = new System.Windows.Forms.DataGridView();
            this.btnLuuQuyen = new System.Windows.Forms.Button();
            this.btnChonTatCaQ = new System.Windows.Forms.Button();
            this.lblQuyenGhiChu = new System.Windows.Forms.Label();
            this.tpLichSu = new System.Windows.Forms.TabPage();
            this.lblTu = new System.Windows.Forms.Label();
            this.dtpTu = new System.Windows.Forms.DateTimePicker();
            this.lblDen = new System.Windows.Forms.Label();
            this.dtpDen = new System.Windows.Forms.DateTimePicker();
            this.lblTenDNLs = new System.Windows.Forms.Label();
            this.txtTenDNLs = new System.Windows.Forms.TextBox();
            this.lblKetQuaLs = new System.Windows.Forms.Label();
            this.cboKetQuaLs = new System.Windows.Forms.ComboBox();
            this.btnLoc = new System.Windows.Forms.Button();
            this.lblTomTatLs = new System.Windows.Forms.Label();
            this.dgvLichSu = new System.Windows.Forms.DataGridView();
            this.tpSaoLuu = new System.Windows.Forms.TabPage();
            this.lblSaoLuuMoTa = new System.Windows.Forms.Label();
            this.btnSaoLuu = new System.Windows.Forms.Button();
            this.btnMoThuMuc = new System.Windows.Forms.Button();
            this.lblSaoLuuKQ = new System.Windows.Forms.Label();
            this.dgvSaoLuu = new System.Windows.Forms.DataGridView();
            this.tab.SuspendLayout();
            this.tpNguoiDung.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvND)).BeginInit();
            this.pnlSua.SuspendLayout();
            this.tpPhanQuyen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvQuyen)).BeginInit();
            this.tpLichSu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLichSu)).BeginInit();
            this.tpSaoLuu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSaoLuu)).BeginInit();
            this.SuspendLayout();
            //
            // lblTieuDe
            //
            this.lblTieuDe.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.lblTieuDe.Size = new System.Drawing.Size(1100, 48);
            this.lblTieuDe.Text = "QUẢN TRỊ HỆ THỐNG";
            this.lblTieuDe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // tab
            //
            this.tab.Controls.Add(this.tpNguoiDung);
            this.tab.Controls.Add(this.tpPhanQuyen);
            this.tab.Controls.Add(this.tpLichSu);
            this.tab.Controls.Add(this.tpSaoLuu);
            this.tab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tab.Name = "tab";
            this.tab.Padding = new System.Drawing.Point(16, 6);
            this.tab.SelectedIndex = 0;
            this.tab.Size = new System.Drawing.Size(1100, 632);
            //
            // ================= tpNguoiDung =================
            //
            this.tpNguoiDung.Controls.Add(this.lblTim);
            this.tpNguoiDung.Controls.Add(this.txtTim);
            this.tpNguoiDung.Controls.Add(this.btnTimND);
            this.tpNguoiDung.Controls.Add(this.dgvND);
            this.tpNguoiDung.Controls.Add(this.pnlSua);
            this.tpNguoiDung.Name = "tpNguoiDung";
            this.tpNguoiDung.Size = new System.Drawing.Size(1092, 596);
            this.tpNguoiDung.Text = "Người dùng";
            this.lblTim.AutoSize = true;
            this.lblTim.Location = new System.Drawing.Point(12, 10);
            this.lblTim.Name = "lblTim";
            this.lblTim.Text = "Tìm người dùng (tên đăng nhập, họ tên, đơn vị)";
            this.txtTim.Location = new System.Drawing.Point(12, 30);
            this.txtTim.Name = "txtTim";
            this.txtTim.Size = new System.Drawing.Size(300, 25);
            this.txtTim.TabIndex = 0;
            this.btnTimND.Location = new System.Drawing.Point(320, 27);
            this.btnTimND.Name = "btnTimND";
            this.btnTimND.Size = new System.Drawing.Size(90, 31);
            this.btnTimND.TabIndex = 1;
            this.btnTimND.Text = "Tìm";
            this.dgvND.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvND.Location = new System.Drawing.Point(12, 66);
            this.dgvND.Name = "dgvND";
            this.dgvND.Size = new System.Drawing.Size(690, 518);
            this.dgvND.TabIndex = 2;
            //
            // pnlSua
            //
            this.pnlSua.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlSua.Controls.Add(this.lblHdrSua);
            this.pnlSua.Controls.Add(this.lblTenDN);
            this.pnlSua.Controls.Add(this.txtTenDN);
            this.pnlSua.Controls.Add(this.lblHoTen);
            this.pnlSua.Controls.Add(this.txtHoTen);
            this.pnlSua.Controls.Add(this.lblChucVu);
            this.pnlSua.Controls.Add(this.txtChucVu);
            this.pnlSua.Controls.Add(this.lblDonVi);
            this.pnlSua.Controls.Add(this.txtDonVi);
            this.pnlSua.Controls.Add(this.lblNhom);
            this.pnlSua.Controls.Add(this.cboNhom);
            this.pnlSua.Controls.Add(this.lblTrangThaiND);
            this.pnlSua.Controls.Add(this.cboTrangThaiND);
            this.pnlSua.Controls.Add(this.lblMatKhau);
            this.pnlSua.Controls.Add(this.txtMatKhau);
            this.pnlSua.Controls.Add(this.lblXacNhan);
            this.pnlSua.Controls.Add(this.txtXacNhan);
            this.pnlSua.Controls.Add(this.lblGhiChuMK);
            this.pnlSua.Controls.Add(this.lblLoiND);
            this.pnlSua.Controls.Add(this.btnMoi);
            this.pnlSua.Controls.Add(this.btnLuuND);
            this.pnlSua.Controls.Add(this.btnDatLaiMK);
            this.pnlSua.Controls.Add(this.btnXoaND);
            this.pnlSua.Location = new System.Drawing.Point(716, 10);
            this.pnlSua.Name = "pnlSua";
            this.pnlSua.Size = new System.Drawing.Size(362, 574);
            this.lblHdrSua.AutoSize = true;
            this.lblHdrSua.Location = new System.Drawing.Point(0, 4);
            this.lblHdrSua.Name = "lblHdrSua";
            this.lblHdrSua.Text = "Thêm người dùng mới";
            this.lblTenDN.AutoSize = true;
            this.lblTenDN.Location = new System.Drawing.Point(0, 40);
            this.lblTenDN.Name = "lblTenDN";
            this.lblTenDN.Text = "Tên đăng nhập *";
            this.txtTenDN.Location = new System.Drawing.Point(0, 60);
            this.txtTenDN.MaxLength = 50;
            this.txtTenDN.Name = "txtTenDN";
            this.txtTenDN.Size = new System.Drawing.Size(350, 25);
            this.txtTenDN.TabIndex = 10;
            this.lblHoTen.AutoSize = true;
            this.lblHoTen.Location = new System.Drawing.Point(0, 96);
            this.lblHoTen.Name = "lblHoTen";
            this.lblHoTen.Text = "Họ tên *";
            this.txtHoTen.Location = new System.Drawing.Point(0, 116);
            this.txtHoTen.MaxLength = 100;
            this.txtHoTen.Name = "txtHoTen";
            this.txtHoTen.Size = new System.Drawing.Size(350, 25);
            this.txtHoTen.TabIndex = 11;
            this.lblChucVu.AutoSize = true;
            this.lblChucVu.Location = new System.Drawing.Point(0, 152);
            this.lblChucVu.Name = "lblChucVu";
            this.lblChucVu.Text = "Chức vụ";
            this.txtChucVu.Location = new System.Drawing.Point(0, 172);
            this.txtChucVu.MaxLength = 100;
            this.txtChucVu.Name = "txtChucVu";
            this.txtChucVu.Size = new System.Drawing.Size(170, 25);
            this.txtChucVu.TabIndex = 12;
            this.lblDonVi.AutoSize = true;
            this.lblDonVi.Location = new System.Drawing.Point(180, 152);
            this.lblDonVi.Name = "lblDonVi";
            this.lblDonVi.Text = "Đơn vị";
            this.txtDonVi.Location = new System.Drawing.Point(180, 172);
            this.txtDonVi.MaxLength = 100;
            this.txtDonVi.Name = "txtDonVi";
            this.txtDonVi.Size = new System.Drawing.Size(170, 25);
            this.txtDonVi.TabIndex = 13;
            this.lblNhom.AutoSize = true;
            this.lblNhom.Location = new System.Drawing.Point(0, 208);
            this.lblNhom.Name = "lblNhom";
            this.lblNhom.Text = "Nhóm người dùng *";
            this.cboNhom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNhom.Location = new System.Drawing.Point(0, 228);
            this.cboNhom.Name = "cboNhom";
            this.cboNhom.Size = new System.Drawing.Size(170, 25);
            this.cboNhom.TabIndex = 14;
            this.lblTrangThaiND.AutoSize = true;
            this.lblTrangThaiND.Location = new System.Drawing.Point(180, 208);
            this.lblTrangThaiND.Name = "lblTrangThaiND";
            this.lblTrangThaiND.Text = "Trạng thái *";
            this.cboTrangThaiND.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTrangThaiND.Location = new System.Drawing.Point(180, 228);
            this.cboTrangThaiND.Name = "cboTrangThaiND";
            this.cboTrangThaiND.Size = new System.Drawing.Size(170, 25);
            this.cboTrangThaiND.TabIndex = 15;
            this.lblMatKhau.AutoSize = true;
            this.lblMatKhau.Location = new System.Drawing.Point(0, 264);
            this.lblMatKhau.Name = "lblMatKhau";
            this.lblMatKhau.Text = "Mật khẩu";
            this.txtMatKhau.Location = new System.Drawing.Point(0, 284);
            this.txtMatKhau.Name = "txtMatKhau";
            this.txtMatKhau.Size = new System.Drawing.Size(170, 25);
            this.txtMatKhau.TabIndex = 16;
            this.txtMatKhau.UseSystemPasswordChar = true;
            this.lblXacNhan.AutoSize = true;
            this.lblXacNhan.Location = new System.Drawing.Point(180, 264);
            this.lblXacNhan.Name = "lblXacNhan";
            this.lblXacNhan.Text = "Nhập lại mật khẩu";
            this.txtXacNhan.Location = new System.Drawing.Point(180, 284);
            this.txtXacNhan.Name = "txtXacNhan";
            this.txtXacNhan.Size = new System.Drawing.Size(170, 25);
            this.txtXacNhan.TabIndex = 17;
            this.txtXacNhan.UseSystemPasswordChar = true;
            this.lblGhiChuMK.Location = new System.Drawing.Point(0, 314);
            this.lblGhiChuMK.Name = "lblGhiChuMK";
            this.lblGhiChuMK.Size = new System.Drawing.Size(350, 36);
            this.lblGhiChuMK.Text = "Bắt buộc khi thêm mới (tối thiểu 6 ký tự). Khi sửa: chỉ dùng cho nút \"Đặt lại mật khẩu\".";
            this.lblLoiND.Location = new System.Drawing.Point(0, 352);
            this.lblLoiND.Name = "lblLoiND";
            this.lblLoiND.Size = new System.Drawing.Size(350, 40);
            this.lblLoiND.Visible = false;
            this.btnMoi.Location = new System.Drawing.Point(0, 400);
            this.btnMoi.Name = "btnMoi";
            this.btnMoi.Size = new System.Drawing.Size(110, 36);
            this.btnMoi.TabIndex = 19;
            this.btnMoi.Text = "Thêm mới";
            this.btnLuuND.Location = new System.Drawing.Point(118, 400);
            this.btnLuuND.Name = "btnLuuND";
            this.btnLuuND.Size = new System.Drawing.Size(232, 36);
            this.btnLuuND.TabIndex = 18;
            this.btnLuuND.Text = "Lưu";
            this.btnDatLaiMK.Location = new System.Drawing.Point(0, 444);
            this.btnDatLaiMK.Name = "btnDatLaiMK";
            this.btnDatLaiMK.Size = new System.Drawing.Size(170, 34);
            this.btnDatLaiMK.TabIndex = 20;
            this.btnDatLaiMK.Text = "Đặt lại mật khẩu";
            this.btnXoaND.Location = new System.Drawing.Point(180, 444);
            this.btnXoaND.Name = "btnXoaND";
            this.btnXoaND.Size = new System.Drawing.Size(170, 34);
            this.btnXoaND.TabIndex = 21;
            this.btnXoaND.Text = "Xoá người dùng";
            //
            // ================= tpPhanQuyen =================
            //
            this.tpPhanQuyen.Controls.Add(this.lblNhomHdr);
            this.tpPhanQuyen.Controls.Add(this.lstNhom);
            this.tpPhanQuyen.Controls.Add(this.lblMoTaNhom);
            this.tpPhanQuyen.Controls.Add(this.dgvQuyen);
            this.tpPhanQuyen.Controls.Add(this.btnLuuQuyen);
            this.tpPhanQuyen.Controls.Add(this.btnChonTatCaQ);
            this.tpPhanQuyen.Controls.Add(this.lblQuyenGhiChu);
            this.tpPhanQuyen.Name = "tpPhanQuyen";
            this.tpPhanQuyen.Size = new System.Drawing.Size(1092, 596);
            this.tpPhanQuyen.Text = "Nhóm & phân quyền";
            this.lblNhomHdr.AutoSize = true;
            this.lblNhomHdr.Location = new System.Drawing.Point(12, 10);
            this.lblNhomHdr.Name = "lblNhomHdr";
            this.lblNhomHdr.Text = "Nhóm người dùng";
            this.lstNhom.IntegralHeight = false;
            this.lstNhom.ItemHeight = 17;
            this.lstNhom.Location = new System.Drawing.Point(12, 36);
            this.lstNhom.Name = "lstNhom";
            this.lstNhom.Size = new System.Drawing.Size(260, 200);
            this.lstNhom.TabIndex = 0;
            this.lblMoTaNhom.Location = new System.Drawing.Point(12, 244);
            this.lblMoTaNhom.Name = "lblMoTaNhom";
            this.lblMoTaNhom.Size = new System.Drawing.Size(260, 110);
            this.dgvQuyen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvQuyen.Location = new System.Drawing.Point(286, 36);
            this.dgvQuyen.Name = "dgvQuyen";
            this.dgvQuyen.Size = new System.Drawing.Size(792, 504);
            this.dgvQuyen.TabIndex = 1;
            this.btnLuuQuyen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLuuQuyen.Location = new System.Drawing.Point(286, 548);
            this.btnLuuQuyen.Name = "btnLuuQuyen";
            this.btnLuuQuyen.Size = new System.Drawing.Size(200, 36);
            this.btnLuuQuyen.TabIndex = 2;
            this.btnLuuQuyen.Text = "Lưu phân quyền";
            this.btnChonTatCaQ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnChonTatCaQ.Location = new System.Drawing.Point(494, 548);
            this.btnChonTatCaQ.Name = "btnChonTatCaQ";
            this.btnChonTatCaQ.Size = new System.Drawing.Size(140, 36);
            this.btnChonTatCaQ.TabIndex = 3;
            this.btnChonTatCaQ.Text = "Chọn / bỏ tất cả";
            this.lblQuyenGhiChu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblQuyenGhiChu.Location = new System.Drawing.Point(644, 548);
            this.lblQuyenGhiChu.Name = "lblQuyenGhiChu";
            this.lblQuyenGhiChu.Size = new System.Drawing.Size(434, 36);
            this.lblQuyenGhiChu.Text = "Thay đổi quyền có hiệu lực ở lần đăng nhập tiếp theo của người dùng.";
            this.lblQuyenGhiChu.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // ================= tpLichSu =================
            //
            this.tpLichSu.Controls.Add(this.lblTu);
            this.tpLichSu.Controls.Add(this.dtpTu);
            this.tpLichSu.Controls.Add(this.lblDen);
            this.tpLichSu.Controls.Add(this.dtpDen);
            this.tpLichSu.Controls.Add(this.lblTenDNLs);
            this.tpLichSu.Controls.Add(this.txtTenDNLs);
            this.tpLichSu.Controls.Add(this.lblKetQuaLs);
            this.tpLichSu.Controls.Add(this.cboKetQuaLs);
            this.tpLichSu.Controls.Add(this.btnLoc);
            this.tpLichSu.Controls.Add(this.lblTomTatLs);
            this.tpLichSu.Controls.Add(this.dgvLichSu);
            this.tpLichSu.Name = "tpLichSu";
            this.tpLichSu.Size = new System.Drawing.Size(1092, 596);
            this.tpLichSu.Text = "Lịch sử truy cập";
            this.lblTu.AutoSize = true;
            this.lblTu.Location = new System.Drawing.Point(12, 10);
            this.lblTu.Name = "lblTu";
            this.lblTu.Text = "Từ ngày";
            this.dtpTu.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpTu.CustomFormat = "dd/MM/yyyy";
            this.dtpTu.Location = new System.Drawing.Point(12, 30);
            this.dtpTu.Name = "dtpTu";
            this.dtpTu.Size = new System.Drawing.Size(150, 25);
            this.dtpTu.TabIndex = 0;
            this.lblDen.AutoSize = true;
            this.lblDen.Location = new System.Drawing.Point(172, 10);
            this.lblDen.Name = "lblDen";
            this.lblDen.Text = "Đến ngày";
            this.dtpDen.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpDen.CustomFormat = "dd/MM/yyyy";
            this.dtpDen.Location = new System.Drawing.Point(172, 30);
            this.dtpDen.Name = "dtpDen";
            this.dtpDen.Size = new System.Drawing.Size(150, 25);
            this.dtpDen.TabIndex = 1;
            this.lblTenDNLs.AutoSize = true;
            this.lblTenDNLs.Location = new System.Drawing.Point(332, 10);
            this.lblTenDNLs.Name = "lblTenDNLs";
            this.lblTenDNLs.Text = "Tên đăng nhập";
            this.txtTenDNLs.Location = new System.Drawing.Point(332, 30);
            this.txtTenDNLs.Name = "txtTenDNLs";
            this.txtTenDNLs.Size = new System.Drawing.Size(180, 25);
            this.txtTenDNLs.TabIndex = 2;
            this.lblKetQuaLs.AutoSize = true;
            this.lblKetQuaLs.Location = new System.Drawing.Point(522, 10);
            this.lblKetQuaLs.Name = "lblKetQuaLs";
            this.lblKetQuaLs.Text = "Kết quả";
            this.cboKetQuaLs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboKetQuaLs.Location = new System.Drawing.Point(522, 30);
            this.cboKetQuaLs.Name = "cboKetQuaLs";
            this.cboKetQuaLs.Size = new System.Drawing.Size(170, 25);
            this.cboKetQuaLs.TabIndex = 3;
            this.btnLoc.Location = new System.Drawing.Point(702, 27);
            this.btnLoc.Name = "btnLoc";
            this.btnLoc.Size = new System.Drawing.Size(120, 31);
            this.btnLoc.TabIndex = 4;
            this.btnLoc.Text = "Lọc";
            this.lblTomTatLs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTomTatLs.Location = new System.Drawing.Point(836, 28);
            this.lblTomTatLs.Name = "lblTomTatLs";
            this.lblTomTatLs.Size = new System.Drawing.Size(242, 30);
            this.lblTomTatLs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.dgvLichSu.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvLichSu.Location = new System.Drawing.Point(12, 68);
            this.dgvLichSu.Name = "dgvLichSu";
            this.dgvLichSu.Size = new System.Drawing.Size(1066, 516);
            this.dgvLichSu.TabIndex = 5;
            //
            // ================= tpSaoLuu =================
            //
            this.tpSaoLuu.Controls.Add(this.lblSaoLuuMoTa);
            this.tpSaoLuu.Controls.Add(this.btnSaoLuu);
            this.tpSaoLuu.Controls.Add(this.btnMoThuMuc);
            this.tpSaoLuu.Controls.Add(this.lblSaoLuuKQ);
            this.tpSaoLuu.Controls.Add(this.dgvSaoLuu);
            this.tpSaoLuu.Name = "tpSaoLuu";
            this.tpSaoLuu.Size = new System.Drawing.Size(1092, 596);
            this.tpSaoLuu.Text = "Sao lưu dữ liệu";
            this.lblSaoLuuMoTa.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSaoLuuMoTa.Location = new System.Drawing.Point(12, 10);
            this.lblSaoLuuMoTa.Name = "lblSaoLuuMoTa";
            this.lblSaoLuuMoTa.Size = new System.Drawing.Size(1066, 62);
            this.btnSaoLuu.Location = new System.Drawing.Point(12, 80);
            this.btnSaoLuu.Name = "btnSaoLuu";
            this.btnSaoLuu.Size = new System.Drawing.Size(220, 38);
            this.btnSaoLuu.TabIndex = 0;
            this.btnSaoLuu.Text = "Sao lưu ngay";
            this.btnMoThuMuc.Location = new System.Drawing.Point(240, 80);
            this.btnMoThuMuc.Name = "btnMoThuMuc";
            this.btnMoThuMuc.Size = new System.Drawing.Size(180, 38);
            this.btnMoThuMuc.TabIndex = 1;
            this.btnMoThuMuc.Text = "Mở thư mục sao lưu";
            this.lblSaoLuuKQ.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSaoLuuKQ.Location = new System.Drawing.Point(430, 80);
            this.lblSaoLuuKQ.Name = "lblSaoLuuKQ";
            this.lblSaoLuuKQ.Size = new System.Drawing.Size(648, 38);
            this.lblSaoLuuKQ.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.dgvSaoLuu.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSaoLuu.Location = new System.Drawing.Point(12, 130);
            this.dgvSaoLuu.Name = "dgvSaoLuu";
            this.dgvSaoLuu.Size = new System.Drawing.Size(1066, 454);
            this.dgvSaoLuu.TabIndex = 2;
            //
            // UcQuanTri
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Size = new System.Drawing.Size(1100, 680);
            this.Controls.Add(this.tab);
            this.Controls.Add(this.lblTieuDe);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Name = "UcQuanTri";
            this.tab.ResumeLayout(false);
            this.tpNguoiDung.ResumeLayout(false);
            this.tpNguoiDung.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvND)).EndInit();
            this.pnlSua.ResumeLayout(false);
            this.pnlSua.PerformLayout();
            this.tpPhanQuyen.ResumeLayout(false);
            this.tpPhanQuyen.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvQuyen)).EndInit();
            this.tpLichSu.ResumeLayout(false);
            this.tpLichSu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLichSu)).EndInit();
            this.tpSaoLuu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSaoLuu)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTieuDe;
        private System.Windows.Forms.TabControl tab;
        private System.Windows.Forms.TabPage tpNguoiDung;
        private System.Windows.Forms.Label lblTim;
        private System.Windows.Forms.TextBox txtTim;
        private System.Windows.Forms.Button btnTimND;
        private System.Windows.Forms.DataGridView dgvND;
        private System.Windows.Forms.Panel pnlSua;
        private System.Windows.Forms.Label lblHdrSua;
        private System.Windows.Forms.Label lblTenDN;
        private System.Windows.Forms.TextBox txtTenDN;
        private System.Windows.Forms.Label lblHoTen;
        private System.Windows.Forms.TextBox txtHoTen;
        private System.Windows.Forms.Label lblChucVu;
        private System.Windows.Forms.TextBox txtChucVu;
        private System.Windows.Forms.Label lblDonVi;
        private System.Windows.Forms.TextBox txtDonVi;
        private System.Windows.Forms.Label lblNhom;
        private System.Windows.Forms.ComboBox cboNhom;
        private System.Windows.Forms.Label lblTrangThaiND;
        private System.Windows.Forms.ComboBox cboTrangThaiND;
        private System.Windows.Forms.Label lblMatKhau;
        private System.Windows.Forms.TextBox txtMatKhau;
        private System.Windows.Forms.Label lblXacNhan;
        private System.Windows.Forms.TextBox txtXacNhan;
        private System.Windows.Forms.Label lblGhiChuMK;
        private System.Windows.Forms.Label lblLoiND;
        private System.Windows.Forms.Button btnMoi;
        private System.Windows.Forms.Button btnLuuND;
        private System.Windows.Forms.Button btnDatLaiMK;
        private System.Windows.Forms.Button btnXoaND;
        private System.Windows.Forms.TabPage tpPhanQuyen;
        private System.Windows.Forms.Label lblNhomHdr;
        private System.Windows.Forms.ListBox lstNhom;
        private System.Windows.Forms.Label lblMoTaNhom;
        private System.Windows.Forms.DataGridView dgvQuyen;
        private System.Windows.Forms.Button btnLuuQuyen;
        private System.Windows.Forms.Button btnChonTatCaQ;
        private System.Windows.Forms.Label lblQuyenGhiChu;
        private System.Windows.Forms.TabPage tpLichSu;
        private System.Windows.Forms.Label lblTu;
        private System.Windows.Forms.DateTimePicker dtpTu;
        private System.Windows.Forms.Label lblDen;
        private System.Windows.Forms.DateTimePicker dtpDen;
        private System.Windows.Forms.Label lblTenDNLs;
        private System.Windows.Forms.TextBox txtTenDNLs;
        private System.Windows.Forms.Label lblKetQuaLs;
        private System.Windows.Forms.ComboBox cboKetQuaLs;
        private System.Windows.Forms.Button btnLoc;
        private System.Windows.Forms.Label lblTomTatLs;
        private System.Windows.Forms.DataGridView dgvLichSu;
        private System.Windows.Forms.TabPage tpSaoLuu;
        private System.Windows.Forms.Label lblSaoLuuMoTa;
        private System.Windows.Forms.Button btnSaoLuu;
        private System.Windows.Forms.Button btnMoThuMuc;
        private System.Windows.Forms.Label lblSaoLuuKQ;
        private System.Windows.Forms.DataGridView dgvSaoLuu;
    }
}
