-- =====================================================================
-- 02_danhmuc.sql – Dữ liệu danh mục: loại trạm, loại công trình, phương tiện mẫu
-- Chỉ ghi vào schema xebus. Idempotent (ON CONFLICT DO NOTHING).
-- =====================================================================
BEGIN;

INSERT INTO xebus.loaitram (maloaitram, tenloaitram, mota) VALUES
    ('TD', 'Trạm dừng',              'Điểm dừng đón trả khách dọc tuyến (OSM: bus_stop)'),
    ('BX', 'Bến xe / điểm trung chuyển', 'Bến xe, điểm đầu cuối hoặc trung chuyển (OSM: bus_station)'),
    ('NC', 'Nhà chờ',                'Trạm dừng có nhà chờ có mái che')
ON CONFLICT (maloaitram) DO NOTHING;

-- trongso: trọng số ưu tiên khi đánh giá ứng viên trạm (mục 3 – Đề xuất trạm mới)
INSERT INTO xebus.loaicongtrinh (maloaicongtrinh, tenloaicongtrinh, mota, trongso) VALUES
    ('TM', 'Thương mại - dịch vụ', 'Trung tâm thương mại, văn phòng (OSM landuse=commercial)', 1.50),
    ('BL', 'Chợ - bán lẻ',         'Chợ, siêu thị, khu bán lẻ (OSM landuse=retail)',          1.50),
    ('CN', 'Khu công nghiệp',      'Khu/cụm công nghiệp, nhà máy (OSM landuse=industrial)',   2.00),
    ('CV', 'Công viên',            'Công viên, vườn hoa (OSM leisure/landuse=park)',          1.00),
    ('GT', 'Khu vui chơi - thể thao', 'Sân thể thao, khu vui chơi (OSM recreation_ground)',   1.00),
    ('TH', 'Trường học',           'Trường học các cấp, đại học (nhập bổ sung)',              2.00),
    ('BV', 'Bệnh viện - y tế',     'Bệnh viện, trạm y tế (nhập bổ sung)',                     2.00)
ON CONFLICT (maloaicongtrinh) DO NOTHING;

INSERT INTO xebus.phuongtien (maphuongtien, bienso, loaiphuongtien, succhua, trangthai) VALUES
    ('PT001', '29B-100.01', 'Xe buýt thường 80 chỗ',   80, 'Hoạt động'),
    ('PT002', '29B-100.02', 'Xe buýt thường 80 chỗ',   80, 'Hoạt động'),
    ('PT003', '29B-200.15', 'Xe buýt trung 60 chỗ',    60, 'Hoạt động'),
    ('PT004', '29B-300.27', 'Xe buýt điện VinBus',     70, 'Hoạt động'),
    ('PT005', '29B-300.28', 'Xe buýt điện VinBus',     70, 'Bảo dưỡng'),
    ('PT006', '29B-400.33', 'Xe buýt nhanh BRT',       90, 'Hoạt động')
ON CONFLICT (maphuongtien) DO NOTHING;

COMMIT;
