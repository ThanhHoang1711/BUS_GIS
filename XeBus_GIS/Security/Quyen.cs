namespace XeBus_GIS.Security
{
    /// <summary>Mã chức năng – khớp cột quyen.machucnang (database/05_baomat.sql).</summary>
    public static class Quyen
    {
        // Quản lý trạm
        public const string TramXem = "TRAM_XEM";
        public const string TramTraCuuThuocTinh = "TRAM_TRACUU_TT";
        public const string TramTraCuuKhongGian = "TRAM_TRACUU_KG";
        public const string TramCapNhat = "TRAM_CAPNHAT";

        // Quản lý tuyến
        public const string TuyenXem = "TUYEN_XEM";
        public const string TuyenTraCuuThuocTinh = "TUYEN_TRACUU_TT";
        public const string TuyenTraCuuKhongGian = "TUYEN_TRACUU_KG";
        public const string TuyenCapNhat = "TUYEN_CAPNHAT";
        public const string TuyenGanTram = "TUYEN_GANTRAM";

        // Đề xuất trạm mới
        public const string DxChonVung = "DX_CHONVUNG";
        public const string DxThieuTram = "DX_THIEUTRAM";
        public const string DxUngVien = "DX_UNGVIEN";
        public const string DxSoSanh = "DX_SOSANH";
        public const string DxDeXuat = "DX_DEXUAT";
        /// <summary>Chọn vị trí đặt trạm theo đa tiêu chí (database/13_chon_vitri.sql) – chỉ nhóm Quản trị viên.</summary>
        public const string DxChonViTri = "DX_CHONVITRI";

        // Thống kê
        public const string TkKhuVuc = "TK_KHUVUC";
        public const string TkTuyen = "TK_TUYEN";
        public const string TkPhamVi = "TK_PHAMVI";
        public const string TkXuatFile = "TK_XUATFILE";

        // Quản trị
        public const string QtNguoiDung = "QT_NGUOIDUNG";
        public const string QtPhanQuyen = "QT_PHANQUYEN";
        public const string QtLichSu = "QT_LICHSU";
        public const string QtSaoLuu = "QT_SAOLUU";

        // Loại thao tác (quyen.loaithaotac)
        public const string Xem = "XEM";
        public const string Them = "THEM";
        public const string Sua = "SUA";
        public const string Xoa = "XOA";

        // Nhóm người dùng (n_ngdung.idnhom)
        public const int NhomQuanLy = 1;
        public const int NhomKhachHang = 2;
        public const int NhomQuanTri = 3;
    }
}
