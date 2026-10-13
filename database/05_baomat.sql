-- =====================================================================
-- 05_baomat.sql – Nhóm người dùng, quyền, phân quyền, tài khoản mẫu (mục 3.1, 3.2.1)
-- Mật khẩu lưu dạng PBKDF2-HMAC-SHA256: "<số vòng lặp>:<salt base64>:<hash base64>"
-- (khớp Security/PasswordHasher.cs – Rfc2898DeriveBytes, SHA256, 10000 vòng, salt 16 byte, hash 32 byte)
--
-- TÀI KHOẢN MẪU (đổi mật khẩu sau khi đăng nhập lần đầu):
--   admin  / Admin@123   – Quản trị viên
--   quanly / Quanly@123  – Người quản lý
--   khach  / Khach@123   – Khách hàng
-- =====================================================================
BEGIN;

-- B11. Nhóm người dùng (id cố định để code tham chiếu)
INSERT INTO xebus.n_ngdung (idnhom, tennhom, mota) VALUES
    (1, 'Người quản lý', 'Bộ phận quản lý: quản lý trạm, tuyến, đề xuất trạm mới, thống kê'),
    (2, 'Khách hàng',    'Bộ phận khách hàng: xem và tra cứu trạm, tuyến'),
    (3, 'Quản trị viên', 'Quản trị hệ thống: người dùng, phân quyền, lịch sử truy cập, sao lưu')
ON CONFLICT (idnhom) DO NOTHING;
SELECT setval(pg_get_serial_sequence('xebus.n_ngdung', 'idnhom'), greatest(3, (SELECT max(idnhom) FROM xebus.n_ngdung)));

-- B12. Quyền: mỗi chức năng (bảng 3.1.1) × loại thao tác chính
INSERT INTO xebus.quyen (tenquyen, machucnang, loaithaotac, mota) VALUES
    -- Quản lý trạm
    ('Xem thông tin trạm',                   'TRAM_XEM',        'XEM',  'Quy trình 1.1'),
    ('Tra cứu trạm theo thuộc tính',         'TRAM_TRACUU_TT',  'XEM',  'Quy trình 1.2'),
    ('Tra cứu trạm theo không gian',         'TRAM_TRACUU_KG',  'XEM',  'Quy trình 1.3'),
    ('Cập nhật thông tin thuộc tính trạm',   'TRAM_CAPNHAT',    'SUA',  'Quy trình 1.4'),
    -- Quản lý tuyến
    ('Xem thông tin tuyến',                  'TUYEN_XEM',       'XEM',  'Quy trình 2.1'),
    ('Tra cứu tuyến theo thuộc tính',        'TUYEN_TRACUU_TT', 'XEM',  'Quy trình 2.2'),
    ('Tra cứu tuyến theo không gian',        'TUYEN_TRACUU_KG', 'XEM',  'Quy trình 2.3'),
    ('Cập nhật thông tin thuộc tính tuyến',  'TUYEN_CAPNHAT',   'SUA',  'Quy trình 2.4'),
    ('Gán trạm vào tuyến xe buýt',           'TUYEN_GANTRAM',   'SUA',  'Quy trình 2.5'),
    -- Đề xuất trạm mới
    ('Chọn vùng cần phân tích',              'DX_CHONVUNG',     'XEM',  'Quy trình 3.1'),
    ('Xác định các khu vực thiếu trạm',      'DX_THIEUTRAM',    'XEM',  'Quy trình 3.2'),
    ('Xác định vị trí ứng viên trạm',        'DX_UNGVIEN',      'XEM',  'Quy trình 3.3'),
    ('So sánh mức độ tối ưu các ứng viên',   'DX_SOSANH',       'XEM',  'Quy trình 3.4'),
    ('Đề xuất vị trí trạm xe tối ưu',        'DX_DEXUAT',       'THEM', 'Quy trình 3.5'),
    -- Thống kê
    ('Thống kê số lượng trạm theo khu vực',  'TK_KHUVUC',       'XEM',  'Quy trình 4.1'),
    ('Thống kê trạm theo tuyến xe buýt',     'TK_TUYEN',        'XEM',  'Quy trình 4.2'),
    ('Thống kê phạm vi phục vụ của trạm',    'TK_PHAMVI',       'XEM',  'Quy trình 4.3'),
    ('Xuất file báo cáo',                    'TK_XUATFILE',     'XEM',  'Quy trình 4.4'),
    -- Quản trị
    ('Thêm mới người dùng nghiệp vụ',        'QT_NGUOIDUNG',    'THEM', 'Quản trị 01'),
    ('Chỉnh sửa thông tin người dùng',       'QT_NGUOIDUNG',    'SUA',  'Quản trị 02'),
    ('Xóa người dùng nghiệp vụ',             'QT_NGUOIDUNG',    'XOA',  'Quản trị 03'),
    ('Phân quyền người dùng',                'QT_PHANQUYEN',    'SUA',  'Quản trị 04'),
    ('Xem lịch sử truy cập',                 'QT_LICHSU',       'XEM',  'Quản trị 05'),
    ('Sao lưu dữ liệu hệ thống',             'QT_SAOLUU',       'THEM', 'Quản trị 06')
ON CONFLICT (machucnang, loaithaotac) DO NOTHING;

-- B11.1. Gán quyền cho nhóm
-- Người quản lý: toàn bộ chức năng nghiệp vụ (18)
INSERT INTO xebus.quyen_nnd (idnhom, idquyen)
SELECT 1, idquyen FROM xebus.quyen
WHERE machucnang ~ '^(TRAM|TUYEN|DX|TK)_'
ON CONFLICT DO NOTHING;

-- Khách hàng: 6 chức năng xem / tra cứu
INSERT INTO xebus.quyen_nnd (idnhom, idquyen)
SELECT 2, idquyen FROM xebus.quyen
WHERE machucnang IN ('TRAM_XEM','TRAM_TRACUU_TT','TRAM_TRACUU_KG','TUYEN_XEM','TUYEN_TRACUU_TT','TUYEN_TRACUU_KG')
ON CONFLICT DO NOTHING;

-- Quản trị viên: chức năng quản trị + đọc dữ liệu (R trên mọi bảng theo mục 3.1.2)
INSERT INTO xebus.quyen_nnd (idnhom, idquyen)
SELECT 3, idquyen FROM xebus.quyen
WHERE machucnang ~ '^QT_'
   OR machucnang IN ('TRAM_XEM','TRAM_TRACUU_TT','TRAM_TRACUU_KG','TUYEN_XEM','TUYEN_TRACUU_TT','TUYEN_TRACUU_KG')
ON CONFLICT DO NOTHING;

-- B13. Tài khoản mẫu
INSERT INTO xebus.nguoidung (tenndung, chucvu, donvi, tendnhap, matkhau, idnhom, trangthai) VALUES
    ('Quản trị hệ thống', 'Quản trị viên', 'Phòng CNTT',
     'admin',  '10000:iISkUREax4yqDrQ4y6peyQ==:xOtS7YvvsF8s2+JwpvI3tuTIPirpzEk4d6or6Nr95l8=', 3, 'Hoạt động'),
    ('Nguyễn Văn Quản', 'Chuyên viên quản lý mạng lưới', 'Bộ phận quản lý',
     'quanly', '10000:0W3syQruqMjwEgmC9fQegQ==:SPQUFCen+Hb00lDSNXkCRDQcfBbyMU6wrLp0u/TW3Gc=', 1, 'Hoạt động'),
    ('Khách hàng', NULL, 'Bộ phận khách hàng',
     'khach',  '10000:tWyvgXO5C7+luUFXqwBm2g==:BnLjco4DLsfOdlxQN7aMz06m5Bougp7bg3Ph5ZADpSU=', 2, 'Hoạt động')
ON CONFLICT (tendnhap) DO NOTHING;

COMMIT;
