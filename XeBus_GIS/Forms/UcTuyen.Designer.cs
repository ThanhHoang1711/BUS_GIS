namespace XeBus_GIS.Forms
{
    partial class UcTuyen
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
            this.pnlTraCuu = new System.Windows.Forms.Panel();
            this.lblHdrTraCuu = new System.Windows.Forms.Label();
            this.lblTuKhoa = new System.Windows.Forms.Label();
            this.txtTuKhoa = new System.Windows.Forms.TextBox();
            this.lblTrangThai = new System.Windows.Forms.Label();
            this.cboTrangThai = new System.Windows.Forms.ComboBox();
            this.lblQuaTram = new System.Windows.Forms.Label();
            this.txtQuaTram = new System.Windows.Forms.TextBox();
            this.btnTim = new System.Windows.Forms.Button();
            this.pnlKhongGian = new System.Windows.Forms.Panel();
            this.lblHdrKhongGian = new System.Windows.Forms.Label();
            this.btnQuaVung = new System.Windows.Forms.Button();
            this.btnGanDiem = new System.Windows.Forms.Button();
            this.nudBanKinh = new System.Windows.Forms.NumericUpDown();
            this.split = new System.Windows.Forms.SplitContainer();
            this.dgvTuyen = new System.Windows.Forms.DataGridView();
            this.lblDanhSach = new System.Windows.Forms.Label();
            this.dgvTram = new System.Windows.Forms.DataGridView();
            this.lblThongTinTuyen = new System.Windows.Forms.Label();
            this.lblChiTiet = new System.Windows.Forms.Label();
            this.pnlNut = new System.Windows.Forms.Panel();
            this.btnSua = new System.Windows.Forms.Button();
            this.btnGanTram = new System.Windows.Forms.Button();
            this.pnlTraCuu.SuspendLayout();
            this.pnlKhongGian.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBanKinh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.split)).BeginInit();
            this.split.Panel1.SuspendLayout();
            this.split.Panel2.SuspendLayout();
            this.split.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTuyen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTram)).BeginInit();
            this.pnlNut.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTieuDe
            //
            this.lblTieuDe.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTieuDe.Name = "lblTieuDe";
            this.lblTieuDe.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblTieuDe.Size = new System.Drawing.Size(380, 42);
            this.lblTieuDe.Text = "QUẢN LÝ TUYẾN XE BUÝT";
            this.lblTieuDe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlTraCuu
            //
            this.pnlTraCuu.Controls.Add(this.lblHdrTraCuu);
            this.pnlTraCuu.Controls.Add(this.lblTuKhoa);
            this.pnlTraCuu.Controls.Add(this.txtTuKhoa);
            this.pnlTraCuu.Controls.Add(this.lblTrangThai);
            this.pnlTraCuu.Controls.Add(this.cboTrangThai);
            this.pnlTraCuu.Controls.Add(this.lblQuaTram);
            this.pnlTraCuu.Controls.Add(this.txtQuaTram);
            this.pnlTraCuu.Controls.Add(this.btnTim);
            this.pnlTraCuu.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTraCuu.Name = "pnlTraCuu";
            this.pnlTraCuu.Size = new System.Drawing.Size(380, 142);
            //
            // lblHdrTraCuu
            //
            this.lblHdrTraCuu.AutoSize = true;
            this.lblHdrTraCuu.Location = new System.Drawing.Point(12, 4);
            this.lblHdrTraCuu.Name = "lblHdrTraCuu";
            this.lblHdrTraCuu.Text = "Tra cứu theo thuộc tính";
            //
            // lblTuKhoa
            //
            this.lblTuKhoa.AutoSize = true;
            this.lblTuKhoa.Location = new System.Drawing.Point(12, 30);
            this.lblTuKhoa.Name = "lblTuKhoa";
            this.lblTuKhoa.Text = "Mã / tên tuyến";
            //
            // txtTuKhoa
            //
            this.txtTuKhoa.Location = new System.Drawing.Point(12, 50);
            this.txtTuKhoa.Name = "txtTuKhoa";
            this.txtTuKhoa.Size = new System.Drawing.Size(220, 25);
            this.txtTuKhoa.TabIndex = 0;
            //
            // lblTrangThai
            //
            this.lblTrangThai.AutoSize = true;
            this.lblTrangThai.Location = new System.Drawing.Point(240, 30);
            this.lblTrangThai.Name = "lblTrangThai";
            this.lblTrangThai.Text = "Trạng thái";
            //
            // cboTrangThai
            //
            this.cboTrangThai.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTrangThai.Location = new System.Drawing.Point(240, 50);
            this.cboTrangThai.Name = "cboTrangThai";
            this.cboTrangThai.Size = new System.Drawing.Size(124, 25);
            this.cboTrangThai.TabIndex = 1;
            //
            // lblQuaTram
            //
            this.lblQuaTram.AutoSize = true;
            this.lblQuaTram.Location = new System.Drawing.Point(12, 82);
            this.lblQuaTram.Name = "lblQuaTram";
            this.lblQuaTram.Text = "Đi qua trạm (tên hoặc mã)";
            //
            // txtQuaTram
            //
            this.txtQuaTram.Location = new System.Drawing.Point(12, 102);
            this.txtQuaTram.Name = "txtQuaTram";
            this.txtQuaTram.Size = new System.Drawing.Size(220, 25);
            this.txtQuaTram.TabIndex = 2;
            //
            // btnTim
            //
            this.btnTim.Location = new System.Drawing.Point(240, 99);
            this.btnTim.Name = "btnTim";
            this.btnTim.Size = new System.Drawing.Size(124, 31);
            this.btnTim.TabIndex = 3;
            this.btnTim.Text = "Tìm kiếm";
            //
            // pnlKhongGian
            //
            this.pnlKhongGian.Controls.Add(this.lblHdrKhongGian);
            this.pnlKhongGian.Controls.Add(this.btnQuaVung);
            this.pnlKhongGian.Controls.Add(this.btnGanDiem);
            this.pnlKhongGian.Controls.Add(this.nudBanKinh);
            this.pnlKhongGian.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlKhongGian.Name = "pnlKhongGian";
            this.pnlKhongGian.Size = new System.Drawing.Size(380, 82);
            //
            // lblHdrKhongGian
            //
            this.lblHdrKhongGian.AutoSize = true;
            this.lblHdrKhongGian.Location = new System.Drawing.Point(12, 6);
            this.lblHdrKhongGian.Name = "lblHdrKhongGian";
            this.lblHdrKhongGian.Text = "Tra cứu theo không gian";
            //
            // btnQuaVung
            //
            this.btnQuaVung.Location = new System.Drawing.Point(12, 34);
            this.btnQuaVung.Name = "btnQuaVung";
            this.btnQuaVung.Size = new System.Drawing.Size(170, 34);
            this.btnQuaVung.TabIndex = 4;
            this.btnQuaVung.Text = "Đi qua vùng vẽ";
            //
            // btnGanDiem
            //
            this.btnGanDiem.Location = new System.Drawing.Point(194, 34);
            this.btnGanDiem.Name = "btnGanDiem";
            this.btnGanDiem.Size = new System.Drawing.Size(104, 34);
            this.btnGanDiem.TabIndex = 5;
            this.btnGanDiem.Text = "Gần điểm";
            //
            // nudBanKinh
            //
            this.nudBanKinh.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudBanKinh.Location = new System.Drawing.Point(304, 39);
            this.nudBanKinh.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            this.nudBanKinh.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
            this.nudBanKinh.Name = "nudBanKinh";
            this.nudBanKinh.Size = new System.Drawing.Size(60, 25);
            this.nudBanKinh.TabIndex = 6;
            this.nudBanKinh.Value = new decimal(new int[] { 300, 0, 0, 0 });
            //
            // split
            //
            this.split.Dock = System.Windows.Forms.DockStyle.Fill;
            this.split.Name = "split";
            this.split.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.split.Size = new System.Drawing.Size(380, 440);
            this.split.SplitterDistance = 170;
            this.split.SplitterWidth = 6;
            this.split.TabIndex = 7;
            //
            // split.Panel1
            //
            this.split.Panel1.Controls.Add(this.dgvTuyen);
            this.split.Panel1.Controls.Add(this.lblDanhSach);
            //
            // split.Panel2
            //
            this.split.Panel2.Controls.Add(this.dgvTram);
            this.split.Panel2.Controls.Add(this.lblThongTinTuyen);
            this.split.Panel2.Controls.Add(this.lblChiTiet);
            //
            // dgvTuyen
            //
            this.dgvTuyen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTuyen.Name = "dgvTuyen";
            this.dgvTuyen.TabIndex = 8;
            //
            // lblDanhSach
            //
            this.lblDanhSach.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDanhSach.Name = "lblDanhSach";
            this.lblDanhSach.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblDanhSach.Size = new System.Drawing.Size(380, 30);
            this.lblDanhSach.Text = "Danh sách tuyến";
            this.lblDanhSach.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // dgvTram
            //
            this.dgvTram.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTram.Name = "dgvTram";
            this.dgvTram.TabIndex = 9;
            //
            // lblThongTinTuyen
            //
            this.lblThongTinTuyen.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblThongTinTuyen.Name = "lblThongTinTuyen";
            this.lblThongTinTuyen.Padding = new System.Windows.Forms.Padding(12, 0, 8, 4);
            this.lblThongTinTuyen.Size = new System.Drawing.Size(380, 58);
            //
            // lblChiTiet
            //
            this.lblChiTiet.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblChiTiet.Name = "lblChiTiet";
            this.lblChiTiet.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblChiTiet.Size = new System.Drawing.Size(380, 30);
            this.lblChiTiet.Text = "Chi tiết tuyến";
            this.lblChiTiet.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlNut
            //
            this.pnlNut.Controls.Add(this.btnSua);
            this.pnlNut.Controls.Add(this.btnGanTram);
            this.pnlNut.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlNut.Name = "pnlNut";
            this.pnlNut.Size = new System.Drawing.Size(380, 54);
            //
            // btnSua
            //
            this.btnSua.Location = new System.Drawing.Point(12, 10);
            this.btnSua.Name = "btnSua";
            this.btnSua.Size = new System.Drawing.Size(170, 34);
            this.btnSua.TabIndex = 10;
            this.btnSua.Text = "Sửa thuộc tính";
            //
            // btnGanTram
            //
            this.btnGanTram.Location = new System.Drawing.Point(194, 10);
            this.btnGanTram.Name = "btnGanTram";
            this.btnGanTram.Size = new System.Drawing.Size(170, 34);
            this.btnGanTram.TabIndex = 11;
            this.btnGanTram.Text = "Gán trạm vào tuyến";
            //
            // UcTuyen
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;   // co giãn DPI bằng UiHelper.CoGianDpi (xem ctor)
            this.Controls.Add(this.split);
            this.Controls.Add(this.pnlNut);
            this.Controls.Add(this.pnlKhongGian);
            this.Controls.Add(this.pnlTraCuu);
            this.Controls.Add(this.lblTieuDe);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Name = "UcTuyen";
            this.Size = new System.Drawing.Size(380, 760);
            this.pnlTraCuu.ResumeLayout(false);
            this.pnlTraCuu.PerformLayout();
            this.pnlKhongGian.ResumeLayout(false);
            this.pnlKhongGian.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBanKinh)).EndInit();
            this.split.Panel1.ResumeLayout(false);
            this.split.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.split)).EndInit();
            this.split.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTuyen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTram)).EndInit();
            this.pnlNut.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTieuDe;
        private System.Windows.Forms.Panel pnlTraCuu;
        private System.Windows.Forms.Label lblHdrTraCuu;
        private System.Windows.Forms.Label lblTuKhoa;
        private System.Windows.Forms.TextBox txtTuKhoa;
        private System.Windows.Forms.Label lblTrangThai;
        private System.Windows.Forms.ComboBox cboTrangThai;
        private System.Windows.Forms.Label lblQuaTram;
        private System.Windows.Forms.TextBox txtQuaTram;
        private System.Windows.Forms.Button btnTim;
        private System.Windows.Forms.Panel pnlKhongGian;
        private System.Windows.Forms.Label lblHdrKhongGian;
        private System.Windows.Forms.Button btnQuaVung;
        private System.Windows.Forms.Button btnGanDiem;
        private System.Windows.Forms.NumericUpDown nudBanKinh;
        private System.Windows.Forms.SplitContainer split;
        private System.Windows.Forms.DataGridView dgvTuyen;
        private System.Windows.Forms.Label lblDanhSach;
        private System.Windows.Forms.DataGridView dgvTram;
        private System.Windows.Forms.Label lblThongTinTuyen;
        private System.Windows.Forms.Label lblChiTiet;
        private System.Windows.Forms.Panel pnlNut;
        private System.Windows.Forms.Button btnSua;
        private System.Windows.Forms.Button btnGanTram;
    }
}
