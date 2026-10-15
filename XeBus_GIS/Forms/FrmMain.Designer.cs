namespace XeBus_GIS.Forms
{
    partial class FrmMain
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
            this.navBar = new XeBus_GIS.UI.Controls.ThanhDieuHuong();
            this.pnlNoiDung = new System.Windows.Forms.Panel();
            this.pnlLamViec = new System.Windows.Forms.Panel();
            this.pnlTrang = new System.Windows.Forms.Panel();
            this.pnlMap = new System.Windows.Forms.Panel();
            this.splPhai = new System.Windows.Forms.Splitter();
            this.pnlPhai = new System.Windows.Forms.Panel();
            this.pnlTabThongTin = new System.Windows.Forms.Panel();
            this.dgvThongTin = new System.Windows.Forms.DataGridView();
            this.lblThongTin = new System.Windows.Forms.Label();
            this.pnlTabLop = new System.Windows.Forms.Panel();
            this.panelLop = new XeBus_GIS.UI.Controls.PanelLopBanDo();
            this.pnlPhaiTab = new System.Windows.Forms.Panel();
            this.btnThuPhai = new System.Windows.Forms.Button();
            this.btnTabLop = new System.Windows.Forms.Button();
            this.btnTabThongTin = new System.Windows.Forms.Button();
            this.splTrai = new System.Windows.Forms.Splitter();
            this.pnlTrai = new System.Windows.Forms.Panel();
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.lblGoiY = new System.Windows.Forms.Label();
            this.btnPanelPhai = new System.Windows.Forms.Button();
            this.flpCongCu = new System.Windows.Forms.FlowLayoutPanel();
            this.btnChon = new System.Windows.Forms.Button();
            this.btnKeo = new System.Windows.Forms.Button();
            this.sep1 = new System.Windows.Forms.Label();
            this.btnPhongTo = new System.Windows.Forms.Button();
            this.btnThuNho = new System.Windows.Forms.Button();
            this.btnToanCanh = new System.Windows.Forms.Button();
            this.sep2 = new System.Windows.Forms.Label();
            this.btnDoDai = new System.Windows.Forms.Button();
            this.btnDoDienTich = new System.Windows.Forms.Button();
            this.sep3 = new System.Windows.Forms.Label();
            this.btnXoaChon = new System.Windows.Forms.Button();
            this.btnLamMoi = new System.Windows.Forms.Button();
            this.pnlPageHeader = new System.Windows.Forms.Panel();
            this.lblTrang = new System.Windows.Forms.Label();
            this.lblBreadcrumb = new System.Windows.Forms.Label();
            this.pnlHeaderPhai = new System.Windows.Forms.Panel();
            this.pnlTim = new System.Windows.Forms.Panel();
            this.txtTimNhanh = new System.Windows.Forms.TextBox();
            this.lblIconTim = new System.Windows.Forms.Label();
            this.btnUser = new System.Windows.Forms.Button();
            this.mnuUser = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuThongTinTk = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSep = new System.Windows.Forms.ToolStripSeparator();
            this.mnuDangXuat = new System.Windows.Forms.ToolStripMenuItem();
            this.lstTimNhanh = new System.Windows.Forms.ListBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.slToaDo = new System.Windows.Forms.ToolStripStatusLabel();
            this.slTyLe = new System.Windows.Forms.ToolStripStatusLabel();
            this.slHeToaDo = new System.Windows.Forms.ToolStripStatusLabel();
            this.slThongBao = new System.Windows.Forms.ToolStripStatusLabel();
            this.slProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.slDb = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlNoiDung.SuspendLayout();
            this.pnlLamViec.SuspendLayout();
            this.pnlPhai.SuspendLayout();
            this.pnlTabThongTin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvThongTin)).BeginInit();
            this.pnlTabLop.SuspendLayout();
            this.pnlPhaiTab.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.flpCongCu.SuspendLayout();
            this.pnlPageHeader.SuspendLayout();
            this.pnlHeaderPhai.SuspendLayout();
            this.pnlTim.SuspendLayout();
            this.mnuUser.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // navBar
            // 
            this.navBar.Dock = System.Windows.Forms.DockStyle.Left;
            this.navBar.KhoaDangChon = null;
            this.navBar.Location = new System.Drawing.Point(0, 0);
            this.navBar.Name = "navBar";
            this.navBar.PhuDe = "Quản lý trạm xe buýt";
            this.navBar.RongMo = 220;
            this.navBar.RongThu = 56;
            this.navBar.Size = new System.Drawing.Size(220, 746);
            this.navBar.TabIndex = 0;
            this.navBar.ThuGon = false;
            this.navBar.TieuDe = "XeBus GIS";
            // 
            // pnlNoiDung
            // 
            this.pnlNoiDung.Controls.Add(this.pnlLamViec);
            this.pnlNoiDung.Controls.Add(this.pnlToolbar);
            this.pnlNoiDung.Controls.Add(this.pnlPageHeader);
            this.pnlNoiDung.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNoiDung.Location = new System.Drawing.Point(220, 0);
            this.pnlNoiDung.Name = "pnlNoiDung";
            this.pnlNoiDung.Size = new System.Drawing.Size(1146, 746);
            this.pnlNoiDung.TabIndex = 1;
            // 
            // pnlLamViec
            // 
            this.pnlLamViec.Controls.Add(this.pnlTrang);
            this.pnlLamViec.Controls.Add(this.pnlMap);
            this.pnlLamViec.Controls.Add(this.splPhai);
            this.pnlLamViec.Controls.Add(this.pnlPhai);
            this.pnlLamViec.Controls.Add(this.splTrai);
            this.pnlLamViec.Controls.Add(this.pnlTrai);
            this.pnlLamViec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLamViec.Location = new System.Drawing.Point(0, 104);
            this.pnlLamViec.Name = "pnlLamViec";
            this.pnlLamViec.Size = new System.Drawing.Size(1146, 642);
            this.pnlLamViec.TabIndex = 2;
            // 
            // pnlTrang
            // 
            this.pnlTrang.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTrang.Location = new System.Drawing.Point(374, 0);
            this.pnlTrang.Name = "pnlTrang";
            this.pnlTrang.Size = new System.Drawing.Size(478, 642);
            this.pnlTrang.TabIndex = 0;
            this.pnlTrang.Visible = false;
            // 
            // pnlMap
            // 
            this.pnlMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMap.Location = new System.Drawing.Point(374, 0);
            this.pnlMap.Name = "pnlMap";
            this.pnlMap.Size = new System.Drawing.Size(478, 642);
            this.pnlMap.TabIndex = 4;
            // 
            // splPhai
            // 
            this.splPhai.Dock = System.Windows.Forms.DockStyle.Right;
            this.splPhai.Location = new System.Drawing.Point(852, 0);
            this.splPhai.MinExtra = 300;
            this.splPhai.MinSize = 240;
            this.splPhai.Name = "splPhai";
            this.splPhai.Size = new System.Drawing.Size(4, 642);
            this.splPhai.TabIndex = 5;
            this.splPhai.TabStop = false;
            // 
            // pnlPhai
            // 
            this.pnlPhai.Controls.Add(this.pnlTabThongTin);
            this.pnlPhai.Controls.Add(this.pnlTabLop);
            this.pnlPhai.Controls.Add(this.pnlPhaiTab);
            this.pnlPhai.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlPhai.Location = new System.Drawing.Point(856, 0);
            this.pnlPhai.Name = "pnlPhai";
            this.pnlPhai.Size = new System.Drawing.Size(290, 642);
            this.pnlPhai.TabIndex = 3;
            // 
            // pnlTabThongTin
            // 
            this.pnlTabThongTin.Controls.Add(this.dgvThongTin);
            this.pnlTabThongTin.Controls.Add(this.lblThongTin);
            this.pnlTabThongTin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTabThongTin.Location = new System.Drawing.Point(0, 40);
            this.pnlTabThongTin.Name = "pnlTabThongTin";
            this.pnlTabThongTin.Padding = new System.Windows.Forms.Padding(12, 4, 8, 8);
            this.pnlTabThongTin.Size = new System.Drawing.Size(290, 602);
            this.pnlTabThongTin.TabIndex = 0;
            // 
            // dgvThongTin
            // 
            this.dgvThongTin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvThongTin.Location = new System.Drawing.Point(12, 36);
            this.dgvThongTin.Name = "dgvThongTin";
            this.dgvThongTin.Size = new System.Drawing.Size(270, 558);
            this.dgvThongTin.TabIndex = 1;
            // 
            // lblThongTin
            // 
            this.lblThongTin.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblThongTin.Location = new System.Drawing.Point(12, 4);
            this.lblThongTin.Name = "lblThongTin";
            this.lblThongTin.Size = new System.Drawing.Size(270, 32);
            this.lblThongTin.TabIndex = 2;
            this.lblThongTin.Text = "Thông tin đối tượng";
            this.lblThongTin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlTabLop
            // 
            this.pnlTabLop.Controls.Add(this.panelLop);
            this.pnlTabLop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTabLop.Location = new System.Drawing.Point(0, 40);
            this.pnlTabLop.Name = "pnlTabLop";
            this.pnlTabLop.Padding = new System.Windows.Forms.Padding(8, 4, 4, 8);
            this.pnlTabLop.Size = new System.Drawing.Size(290, 602);
            this.pnlTabLop.TabIndex = 1;
            this.pnlTabLop.Visible = false;
            // 
            // panelLop
            // 
            this.panelLop.AutoScroll = true;
            this.panelLop.BackColor = System.Drawing.Color.White;
            this.panelLop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLop.Location = new System.Drawing.Point(8, 4);
            this.panelLop.Name = "panelLop";
            this.panelLop.Size = new System.Drawing.Size(278, 590);
            this.panelLop.TabIndex = 0;
            // 
            // pnlPhaiTab
            // 
            this.pnlPhaiTab.Controls.Add(this.btnThuPhai);
            this.pnlPhaiTab.Controls.Add(this.btnTabLop);
            this.pnlPhaiTab.Controls.Add(this.btnTabThongTin);
            this.pnlPhaiTab.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlPhaiTab.Location = new System.Drawing.Point(0, 0);
            this.pnlPhaiTab.Name = "pnlPhaiTab";
            this.pnlPhaiTab.Padding = new System.Windows.Forms.Padding(8, 0, 4, 0);
            this.pnlPhaiTab.Size = new System.Drawing.Size(290, 40);
            this.pnlPhaiTab.TabIndex = 2;
            // 
            // btnThuPhai
            // 
            this.btnThuPhai.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnThuPhai.Location = new System.Drawing.Point(250, 0);
            this.btnThuPhai.Name = "btnThuPhai";
            this.btnThuPhai.Size = new System.Drawing.Size(36, 40);
            this.btnThuPhai.TabIndex = 0;
            // 
            // btnTabLop
            // 
            this.btnTabLop.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnTabLop.Location = new System.Drawing.Point(104, 0);
            this.btnTabLop.Name = "btnTabLop";
            this.btnTabLop.Size = new System.Drawing.Size(124, 40);
            this.btnTabLop.TabIndex = 1;
            this.btnTabLop.Text = "Lớp && chú giải";
            // 
            // btnTabThongTin
            // 
            this.btnTabThongTin.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnTabThongTin.Location = new System.Drawing.Point(8, 0);
            this.btnTabThongTin.Name = "btnTabThongTin";
            this.btnTabThongTin.Size = new System.Drawing.Size(96, 40);
            this.btnTabThongTin.TabIndex = 2;
            this.btnTabThongTin.Text = "Thông tin";
            // 
            // splTrai
            // 
            this.splTrai.Location = new System.Drawing.Point(370, 0);
            this.splTrai.MinExtra = 300;
            this.splTrai.MinSize = 320;
            this.splTrai.Name = "splTrai";
            this.splTrai.Size = new System.Drawing.Size(4, 642);
            this.splTrai.TabIndex = 6;
            this.splTrai.TabStop = false;
            // 
            // pnlTrai
            // 
            this.pnlTrai.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlTrai.Location = new System.Drawing.Point(0, 0);
            this.pnlTrai.Name = "pnlTrai";
            this.pnlTrai.Size = new System.Drawing.Size(370, 642);
            this.pnlTrai.TabIndex = 2;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.Controls.Add(this.lblGoiY);
            this.pnlToolbar.Controls.Add(this.btnPanelPhai);
            this.pnlToolbar.Controls.Add(this.flpCongCu);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 60);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(1146, 44);
            this.pnlToolbar.TabIndex = 1;
            // 
            // lblGoiY
            // 
            this.lblGoiY.AutoEllipsis = true;
            this.lblGoiY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGoiY.Location = new System.Drawing.Point(420, 0);
            this.lblGoiY.Name = "lblGoiY";
            this.lblGoiY.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblGoiY.Size = new System.Drawing.Size(682, 44);
            this.lblGoiY.TabIndex = 0;
            this.lblGoiY.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnPanelPhai
            // 
            this.btnPanelPhai.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnPanelPhai.Location = new System.Drawing.Point(1102, 0);
            this.btnPanelPhai.Name = "btnPanelPhai";
            this.btnPanelPhai.Size = new System.Drawing.Size(44, 44);
            this.btnPanelPhai.TabIndex = 1;
            // 
            // flpCongCu
            // 
            this.flpCongCu.Controls.Add(this.btnChon);
            this.flpCongCu.Controls.Add(this.btnKeo);
            this.flpCongCu.Controls.Add(this.sep1);
            this.flpCongCu.Controls.Add(this.btnPhongTo);
            this.flpCongCu.Controls.Add(this.btnThuNho);
            this.flpCongCu.Controls.Add(this.btnToanCanh);
            this.flpCongCu.Controls.Add(this.sep2);
            this.flpCongCu.Controls.Add(this.btnDoDai);
            this.flpCongCu.Controls.Add(this.btnDoDienTich);
            this.flpCongCu.Controls.Add(this.sep3);
            this.flpCongCu.Controls.Add(this.btnXoaChon);
            this.flpCongCu.Controls.Add(this.btnLamMoi);
            this.flpCongCu.Dock = System.Windows.Forms.DockStyle.Left;
            this.flpCongCu.Location = new System.Drawing.Point(0, 0);
            this.flpCongCu.Name = "flpCongCu";
            this.flpCongCu.Padding = new System.Windows.Forms.Padding(10, 5, 0, 0);
            this.flpCongCu.Size = new System.Drawing.Size(420, 44);
            this.flpCongCu.TabIndex = 2;
            this.flpCongCu.WrapContents = false;
            // 
            // btnChon
            // 
            this.btnChon.Location = new System.Drawing.Point(11, 5);
            this.btnChon.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.btnChon.Name = "btnChon";
            this.btnChon.Size = new System.Drawing.Size(34, 34);
            this.btnChon.TabIndex = 0;
            // 
            // btnKeo
            // 
            this.btnKeo.Location = new System.Drawing.Point(47, 5);
            this.btnKeo.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.btnKeo.Name = "btnKeo";
            this.btnKeo.Size = new System.Drawing.Size(34, 34);
            this.btnKeo.TabIndex = 1;
            // 
            // sep1
            // 
            this.sep1.Location = new System.Drawing.Point(90, 11);
            this.sep1.Margin = new System.Windows.Forms.Padding(8, 6, 8, 0);
            this.sep1.Name = "sep1";
            this.sep1.Size = new System.Drawing.Size(1, 22);
            this.sep1.TabIndex = 2;
            // 
            // btnPhongTo
            // 
            this.btnPhongTo.Location = new System.Drawing.Point(100, 5);
            this.btnPhongTo.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.btnPhongTo.Name = "btnPhongTo";
            this.btnPhongTo.Size = new System.Drawing.Size(34, 34);
            this.btnPhongTo.TabIndex = 3;
            // 
            // btnThuNho
            // 
            this.btnThuNho.Location = new System.Drawing.Point(136, 5);
            this.btnThuNho.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.btnThuNho.Name = "btnThuNho";
            this.btnThuNho.Size = new System.Drawing.Size(34, 34);
            this.btnThuNho.TabIndex = 4;
            // 
            // btnToanCanh
            // 
            this.btnToanCanh.Location = new System.Drawing.Point(172, 5);
            this.btnToanCanh.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.btnToanCanh.Name = "btnToanCanh";
            this.btnToanCanh.Size = new System.Drawing.Size(34, 34);
            this.btnToanCanh.TabIndex = 5;
            // 
            // sep2
            // 
            this.sep2.Location = new System.Drawing.Point(215, 11);
            this.sep2.Margin = new System.Windows.Forms.Padding(8, 6, 8, 0);
            this.sep2.Name = "sep2";
            this.sep2.Size = new System.Drawing.Size(1, 22);
            this.sep2.TabIndex = 6;
            // 
            // btnDoDai
            // 
            this.btnDoDai.Location = new System.Drawing.Point(225, 5);
            this.btnDoDai.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.btnDoDai.Name = "btnDoDai";
            this.btnDoDai.Size = new System.Drawing.Size(34, 34);
            this.btnDoDai.TabIndex = 7;
            // 
            // btnDoDienTich
            // 
            this.btnDoDienTich.Location = new System.Drawing.Point(261, 5);
            this.btnDoDienTich.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.btnDoDienTich.Name = "btnDoDienTich";
            this.btnDoDienTich.Size = new System.Drawing.Size(34, 34);
            this.btnDoDienTich.TabIndex = 8;
            // 
            // sep3
            // 
            this.sep3.Location = new System.Drawing.Point(304, 11);
            this.sep3.Margin = new System.Windows.Forms.Padding(8, 6, 8, 0);
            this.sep3.Name = "sep3";
            this.sep3.Size = new System.Drawing.Size(1, 22);
            this.sep3.TabIndex = 9;
            // 
            // btnXoaChon
            // 
            this.btnXoaChon.Location = new System.Drawing.Point(314, 5);
            this.btnXoaChon.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.btnXoaChon.Name = "btnXoaChon";
            this.btnXoaChon.Size = new System.Drawing.Size(34, 34);
            this.btnXoaChon.TabIndex = 10;
            // 
            // btnLamMoi
            // 
            this.btnLamMoi.Location = new System.Drawing.Point(350, 5);
            this.btnLamMoi.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.btnLamMoi.Name = "btnLamMoi";
            this.btnLamMoi.Size = new System.Drawing.Size(34, 34);
            this.btnLamMoi.TabIndex = 11;
            // 
            // pnlPageHeader
            // 
            this.pnlPageHeader.Controls.Add(this.lblTrang);
            this.pnlPageHeader.Controls.Add(this.lblBreadcrumb);
            this.pnlPageHeader.Controls.Add(this.pnlHeaderPhai);
            this.pnlPageHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlPageHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlPageHeader.Name = "pnlPageHeader";
            this.pnlPageHeader.Padding = new System.Windows.Forms.Padding(20, 0, 12, 0);
            this.pnlPageHeader.Size = new System.Drawing.Size(1146, 60);
            this.pnlPageHeader.TabIndex = 0;
            // 
            // lblTrang
            // 
            this.lblTrang.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTrang.Location = new System.Drawing.Point(20, 24);
            this.lblTrang.Name = "lblTrang";
            this.lblTrang.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblTrang.Size = new System.Drawing.Size(554, 36);
            this.lblTrang.TabIndex = 0;
            this.lblTrang.Text = "Tổng quan";
            // 
            // lblBreadcrumb
            // 
            this.lblBreadcrumb.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblBreadcrumb.Location = new System.Drawing.Point(20, 0);
            this.lblBreadcrumb.Name = "lblBreadcrumb";
            this.lblBreadcrumb.Size = new System.Drawing.Size(554, 24);
            this.lblBreadcrumb.TabIndex = 1;
            this.lblBreadcrumb.Text = "Hệ thống";
            this.lblBreadcrumb.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // pnlHeaderPhai
            // 
            this.pnlHeaderPhai.Controls.Add(this.pnlTim);
            this.pnlHeaderPhai.Controls.Add(this.btnUser);
            this.pnlHeaderPhai.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlHeaderPhai.Location = new System.Drawing.Point(574, 0);
            this.pnlHeaderPhai.Name = "pnlHeaderPhai";
            this.pnlHeaderPhai.Size = new System.Drawing.Size(560, 60);
            this.pnlHeaderPhai.TabIndex = 2;
            // 
            // pnlTim
            // 
            this.pnlTim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTim.Controls.Add(this.txtTimNhanh);
            this.pnlTim.Controls.Add(this.lblIconTim);
            this.pnlTim.Location = new System.Drawing.Point(8, 13);
            this.pnlTim.Name = "pnlTim";
            this.pnlTim.Padding = new System.Windows.Forms.Padding(1);
            this.pnlTim.Size = new System.Drawing.Size(320, 34);
            this.pnlTim.TabIndex = 0;
            // 
            // txtTimNhanh
            // 
            this.txtTimNhanh.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTimNhanh.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTimNhanh.Location = new System.Drawing.Point(34, 8);
            this.txtTimNhanh.Name = "txtTimNhanh";
            this.txtTimNhanh.Size = new System.Drawing.Size(278, 18);
            this.txtTimNhanh.TabIndex = 0;
            // 
            // lblIconTim
            // 
            this.lblIconTim.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblIconTim.Location = new System.Drawing.Point(1, 1);
            this.lblIconTim.Name = "lblIconTim";
            this.lblIconTim.Size = new System.Drawing.Size(32, 32);
            this.lblIconTim.TabIndex = 1;
            this.lblIconTim.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnUser
            // 
            this.btnUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUser.Location = new System.Drawing.Point(340, 13);
            this.btnUser.Name = "btnUser";
            this.btnUser.Size = new System.Drawing.Size(212, 34);
            this.btnUser.TabIndex = 1;
            this.btnUser.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mnuUser
            // 
            this.mnuUser.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuThongTinTk,
            this.mnuSep,
            this.mnuDangXuat});
            this.mnuUser.Name = "mnuUser";
            this.mnuUser.Size = new System.Drawing.Size(128, 54);
            // 
            // mnuThongTinTk
            // 
            this.mnuThongTinTk.Enabled = false;
            this.mnuThongTinTk.Name = "mnuThongTinTk";
            this.mnuThongTinTk.Size = new System.Drawing.Size(127, 22);
            this.mnuThongTinTk.Text = "Tài khoản";
            // 
            // mnuSep
            // 
            this.mnuSep.Name = "mnuSep";
            this.mnuSep.Size = new System.Drawing.Size(124, 6);
            // 
            // mnuDangXuat
            // 
            this.mnuDangXuat.Name = "mnuDangXuat";
            this.mnuDangXuat.Size = new System.Drawing.Size(127, 22);
            this.mnuDangXuat.Text = "Đăng xuất";
            // 
            // lstTimNhanh
            // 
            this.lstTimNhanh.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstTimNhanh.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstTimNhanh.IntegralHeight = false;
            this.lstTimNhanh.ItemHeight = 44;
            this.lstTimNhanh.Location = new System.Drawing.Point(0, 0);
            this.lstTimNhanh.Name = "lstTimNhanh";
            this.lstTimNhanh.Size = new System.Drawing.Size(360, 200);
            this.lstTimNhanh.TabIndex = 1;
            this.lstTimNhanh.TabStop = false;
            this.lstTimNhanh.Visible = false;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slToaDo,
            this.slTyLe,
            this.slHeToaDo,
            this.slThongBao,
            this.slProgress,
            this.slDb});
            this.statusStrip.Location = new System.Drawing.Point(0, 746);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1366, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 5;
            // 
            // slToaDo
            // 
            this.slToaDo.AutoSize = false;
            this.slToaDo.Name = "slToaDo";
            this.slToaDo.Size = new System.Drawing.Size(250, 17);
            this.slToaDo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // slTyLe
            // 
            this.slTyLe.AutoSize = false;
            this.slTyLe.Name = "slTyLe";
            this.slTyLe.Size = new System.Drawing.Size(130, 17);
            this.slTyLe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // slHeToaDo
            // 
            this.slHeToaDo.Name = "slHeToaDo";
            this.slHeToaDo.Size = new System.Drawing.Size(110, 17);
            this.slHeToaDo.Text = "WGS 84 · EPSG:4326";
            // 
            // slThongBao
            // 
            this.slThongBao.Name = "slThongBao";
            this.slThongBao.Size = new System.Drawing.Size(812, 17);
            this.slThongBao.Spring = true;
            this.slThongBao.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // slProgress
            // 
            this.slProgress.Name = "slProgress";
            this.slProgress.Size = new System.Drawing.Size(120, 16);
            this.slProgress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.slProgress.Visible = false;
            // 
            // slDb
            // 
            this.slDb.Name = "slDb";
            this.slDb.Size = new System.Drawing.Size(49, 17);
            this.slDb.Text = "CSDLKG";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;   // co giãn DPI bằng UiHelper.CoGianDpi (xem ctor)
            this.ClientSize = new System.Drawing.Size(1366, 768);
            this.Controls.Add(this.lstTimNhanh);
            this.Controls.Add(this.pnlNoiDung);
            this.Controls.Add(this.navBar);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1100, 650);
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "XeBus GIS - Hệ thống GIS hỗ trợ quản lý, phân tích, thiết lập trạm xe buýt";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.pnlNoiDung.ResumeLayout(false);
            this.pnlLamViec.ResumeLayout(false);
            this.pnlPhai.ResumeLayout(false);
            this.pnlTabThongTin.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvThongTin)).EndInit();
            this.pnlTabLop.ResumeLayout(false);
            this.pnlPhaiTab.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.flpCongCu.ResumeLayout(false);
            this.pnlPageHeader.ResumeLayout(false);
            this.pnlHeaderPhai.ResumeLayout(false);
            this.pnlTim.ResumeLayout(false);
            this.pnlTim.PerformLayout();
            this.mnuUser.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private XeBus_GIS.UI.Controls.ThanhDieuHuong navBar;
        private System.Windows.Forms.Panel pnlNoiDung;
        private System.Windows.Forms.Panel pnlLamViec;
        private System.Windows.Forms.Panel pnlTrang;
        private System.Windows.Forms.Panel pnlMap;
        private System.Windows.Forms.Splitter splPhai;
        private System.Windows.Forms.Panel pnlPhai;
        private System.Windows.Forms.Panel pnlTabThongTin;
        private System.Windows.Forms.DataGridView dgvThongTin;
        private System.Windows.Forms.Label lblThongTin;
        private System.Windows.Forms.Panel pnlTabLop;
        private XeBus_GIS.UI.Controls.PanelLopBanDo panelLop;
        private System.Windows.Forms.Panel pnlPhaiTab;
        private System.Windows.Forms.Button btnTabThongTin;
        private System.Windows.Forms.Button btnTabLop;
        private System.Windows.Forms.Button btnThuPhai;
        private System.Windows.Forms.Splitter splTrai;
        private System.Windows.Forms.Panel pnlTrai;
        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Label lblGoiY;
        private System.Windows.Forms.Button btnPanelPhai;
        private System.Windows.Forms.FlowLayoutPanel flpCongCu;
        private System.Windows.Forms.Button btnChon;
        private System.Windows.Forms.Button btnKeo;
        private System.Windows.Forms.Label sep1;
        private System.Windows.Forms.Button btnPhongTo;
        private System.Windows.Forms.Button btnThuNho;
        private System.Windows.Forms.Button btnToanCanh;
        private System.Windows.Forms.Label sep2;
        private System.Windows.Forms.Button btnDoDai;
        private System.Windows.Forms.Button btnDoDienTich;
        private System.Windows.Forms.Label sep3;
        private System.Windows.Forms.Button btnXoaChon;
        private System.Windows.Forms.Button btnLamMoi;
        private System.Windows.Forms.Panel pnlPageHeader;
        private System.Windows.Forms.Label lblTrang;
        private System.Windows.Forms.Label lblBreadcrumb;
        private System.Windows.Forms.Panel pnlHeaderPhai;
        private System.Windows.Forms.Panel pnlTim;
        private System.Windows.Forms.TextBox txtTimNhanh;
        private System.Windows.Forms.Label lblIconTim;
        private System.Windows.Forms.Button btnUser;
        private System.Windows.Forms.ContextMenuStrip mnuUser;
        private System.Windows.Forms.ToolStripMenuItem mnuThongTinTk;
        private System.Windows.Forms.ToolStripSeparator mnuSep;
        private System.Windows.Forms.ToolStripMenuItem mnuDangXuat;
        private System.Windows.Forms.ListBox lstTimNhanh;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel slToaDo;
        private System.Windows.Forms.ToolStripStatusLabel slTyLe;
        private System.Windows.Forms.ToolStripStatusLabel slHeToaDo;
        private System.Windows.Forms.ToolStripStatusLabel slThongBao;
        private System.Windows.Forms.ToolStripProgressBar slProgress;
        private System.Windows.Forms.ToolStripStatusLabel slDb;
    }
}
