-- =====================================================================
-- 09_phuongxa_congtrinh.sql – Nạp phường/xã và công trình từ shapefile (data/shp) vào xebus
--
-- ĐIỀU KIỆN: đã nạp shapefile vào bảng tạm (chạy trong thư mục D:\XeBus_GIS):
--   shp2pgsql -c -s 4326 -W UTF-8 -I data\shp\phuongxa.shp  xebus.tmp_phuongxa  | psql -d CSDLKG
--   shp2pgsql -c -s 4326 -W UTF-8 -I data\shp\congtrinh.shp xebus.tmp_congtrinh | psql -d CSDLKG
--
-- Nội dung:
--   1. phuongxa  <- tmp_phuongxa (126 xã/phường Hà Nội sau sắp xếp 2025, có dân số + diện tích).
--      Cấp quận/huyện đã bãi bỏ => maquan để NULL, bảng quanhuyen không dùng.
--   2. Gán maphuong cho trambuyt, khudancu, doanduong theo vị trí.
--   3. Dân số khu dân cư: phân bổ dân số thật của phường cho các KDC trong phường theo diện tích
--      (phương pháp dasymetric). Vẫn là ƯỚC TÍNH ở cấp khu, nhưng tổng theo phường khớp số liệu thật.
--   4. congtrinh: chuyển sang dạng ĐIỂM (đúng thiết kế Word: viDo/kinhDo); nạp các POI thu hút
--      chuyến đi từ tmp_congtrinh + khu công nghiệp (điểm đại diện của sudungdata.industrial).
--   5. Tính lại B06.1/B06.2 (bán kính 500 m), xoá bảng tạm.
-- Chỉ ghi vào schema xebus. Idempotent.
-- =====================================================================
BEGIN;

DO $$
BEGIN
    IF to_regclass('xebus.tmp_phuongxa') IS NULL OR to_regclass('xebus.tmp_congtrinh') IS NULL THEN
        RAISE EXCEPTION 'Chưa nạp shapefile vào xebus.tmp_phuongxa / xebus.tmp_congtrinh (xem đầu file)';
    END IF;
END $$;

-- ---------------------------------------------------------------------
-- 1. Phường / xã
-- ---------------------------------------------------------------------
ALTER TABLE xebus.phuongxa ADD COLUMN IF NOT EXISTS danso       integer;        -- bổ sung: dân số (shapefile)
ALTER TABLE xebus.phuongxa ADD COLUMN IF NOT EXISTS dientichkm2 numeric(10,2);  -- bổ sung: diện tích (km²)
ALTER TABLE xebus.phuongxa ADD COLUMN IF NOT EXISTS ghichu      text;           -- bổ sung: đơn vị cũ sáp nhập
COMMENT ON COLUMN xebus.phuongxa.maquan IS 'Không dùng – cấp quận/huyện đã bãi bỏ (2025)';

INSERT INTO xebus.phuongxa (maphuong, maquan, tenphuong, danso, dientichkm2, ghichu, geom)
SELECT trim(t.maxa), NULL, trim(t.tenxa),
       NULLIF(regexp_replace(t.danso, '[^0-9]', '', 'g'), '')::int,
       NULLIF(t.dientich, '')::numeric(10,2),
       NULLIF(trim(t.ghichu), ''),
       ST_Multi(ST_MakeValid(t.geom))::geometry(MultiPolygon, 4326)
FROM xebus.tmp_phuongxa t
ON CONFLICT (maphuong) DO UPDATE
   SET tenphuong = EXCLUDED.tenphuong, danso = EXCLUDED.danso, dientichkm2 = EXCLUDED.dientichkm2,
       ghichu = EXCLUDED.ghichu, geom = EXCLUDED.geom;

CREATE INDEX IF NOT EXISTS ix_phuongxa_utm ON xebus.phuongxa USING gist (ST_Transform(geom, 32648));

-- ---------------------------------------------------------------------
-- 2. Gán phường theo vị trí (tắt trigger để không đổi ngaycapnhat của trạm)
-- ---------------------------------------------------------------------
ALTER TABLE xebus.trambuyt DISABLE TRIGGER trg_trambuyt_diem;
UPDATE xebus.trambuyt t SET maphuong = p.maphuong
FROM xebus.phuongxa p
WHERE ST_Intersects(p.geom, t.geom) AND t.maphuong IS DISTINCT FROM p.maphuong;
ALTER TABLE xebus.trambuyt ENABLE TRIGGER trg_trambuyt_diem;

UPDATE xebus.khudancu k SET maphuong = p.maphuong
FROM xebus.phuongxa p
WHERE ST_Intersects(p.geom, ST_PointOnSurface(k.geom)) AND k.maphuong IS DISTINCT FROM p.maphuong;

UPDATE xebus.doanduong d SET maphuong = p.maphuong
FROM xebus.phuongxa p
WHERE ST_Intersects(p.geom, ST_LineInterpolatePoint(ST_GeometryN(d.geom, 1), 0.5))
  AND d.maphuong IS DISTINCT FROM p.maphuong;

-- ---------------------------------------------------------------------
-- 3. Dân số KDC = Σ theo phường min( dân số phường × diện tích(KDC ∩ phường) / tổng diện tích KDC trong phường,
--                                     diện tích(KDC ∩ phường) × mật độ trần )
--    Mật độ trần: phường 40 000 người/km², xã 15 000 người/km² (OSM chỉ vẽ một phần khu ở nên
--    không giới hạn sẽ dồn toàn bộ dân phường vào vài khu nhỏ => mật độ phi thực tế).
-- ---------------------------------------------------------------------
WITH giao AS (
    SELECT k.makhu, p.maphuong, p.danso, p.tenphuong,
           ST_Area(ST_Intersection(ST_Transform(k.geom, 32648), ST_Transform(p.geom, 32648))) AS dt
    FROM xebus.khudancu k
    JOIN xebus.phuongxa p ON ST_Intersects(k.geom, p.geom)
), tong AS (
    SELECT maphuong, sum(dt) AS dt_kdc FROM giao GROUP BY maphuong
), phan_bo AS (
    SELECT g.makhu,
           sum(least(g.danso * g.dt / NULLIF(t.dt_kdc, 0),
                     g.dt / 1e6 * CASE WHEN g.tenphuong LIKE 'Phường%' THEN 40000 ELSE 15000 END)) AS dan
    FROM giao g JOIN tong t USING (maphuong)
    GROUP BY g.makhu
)
UPDATE xebus.khudancu k
SET danso      = round(pb.dan)::int,
    matdodancu = round((pb.dan / NULLIF(ST_Area(ST_Transform(k.geom, 32648)) / 1e6, 0))::numeric, 2)
FROM phan_bo pb
WHERE pb.makhu = k.makhu;

-- ---------------------------------------------------------------------
-- 4. Công trình dạng điểm
-- ---------------------------------------------------------------------
ALTER TABLE xebus.congtrinh ADD COLUMN IF NOT EXISTS osm_id varchar(12);  -- bổ sung: truy vết POI
ALTER TABLE xebus.congtrinh ADD COLUMN IF NOT EXISTS nguon  varchar(30);  -- bổ sung: nguồn dữ liệu

DO $$
BEGIN
    IF (SELECT format_type(atttypid, atttypmod) FROM pg_attribute
        WHERE attrelid = 'xebus.congtrinh'::regclass AND attname = 'geom') <> 'geometry(Point,4326)' THEN
        -- dữ liệu cũ là vùng sử dụng đất (bản sao từ public.sudungdata) – thay bằng điểm
        DELETE FROM xebus.congtrinh;
        ALTER TABLE xebus.congtrinh ALTER COLUMN geom TYPE geometry(Point, 4326) USING ST_PointOnSurface(geom);
    END IF;
END $$;

INSERT INTO xebus.loaicongtrinh (maloaicongtrinh, tenloaicongtrinh, mota, trongso) VALUES
    ('TH', 'Trường học',               'Trường mầm non, phổ thông (OSM school, kindergarten)',            2.00),
    ('DH', 'Đại học - cao đẳng',       'Trường đại học, cao đẳng (OSM university, college)',              2.50),
    ('BV', 'Bệnh viện',                'Bệnh viện (OSM hospital)',                                        2.50),
    ('YT', 'Phòng khám - y tế',        'Phòng khám, nha khoa, bác sĩ (OSM clinic, doctors, dentist)',     1.00),
    ('TT', 'TTTM - siêu thị',          'Trung tâm thương mại, cửa hàng bách hoá, siêu thị',               1.50),
    ('BL', 'Chợ',                      'Chợ dân sinh (OSM marketplace)',                                  1.50),
    ('HC', 'Hành chính - công cộng',   'UBND, công an, bưu điện, nhà văn hoá, thư viện, đại sứ quán…',    1.50),
    ('DL', 'Văn hoá - du lịch',        'Bảo tàng, nhà hát, rạp chiếu phim, di tích, điểm tham quan',      1.00),
    ('GT', 'Thể thao - vui chơi',      'Sân vận động, trung tâm thể thao, bể bơi, sân chơi',              1.00),
    ('CV', 'Công viên',                'Công viên, vườn hoa',                                             1.00),
    ('LT', 'Lưu trú',                  'Khách sạn, nhà nghỉ, hostel',                                     0.50),
    ('CN', 'Khu công nghiệp',          'Khu/cụm công nghiệp (điểm đại diện vùng OSM landuse=industrial)', 2.00)
ON CONFLICT (maloaicongtrinh) DO UPDATE
   SET tenloaicongtrinh = EXCLUDED.tenloaicongtrinh, mota = EXCLUDED.mota, trongso = EXCLUDED.trongso;

-- Ánh xạ fclass POI -> loại, quy mô ƯỚC TÍNH (người/ngày hoặc số chỗ), mức ưu tiên 1..3
CREATE TEMP TABLE tmp_anhxa (fclass text PRIMARY KEY, maloai text, quymo int, uutien smallint) ON COMMIT DROP;
INSERT INTO tmp_anhxa VALUES
    ('school','TH',800,2), ('kindergarten','TH',200,2),
    ('university','DH',10000,3), ('college','DH',3000,3),
    ('hospital','BV',500,3),
    ('clinic','YT',50,2), ('doctors','YT',20,1), ('dentist','YT',10,1),
    ('mall','TT',5000,3), ('department_store','TT',1000,2), ('supermarket','TT',500,2),
    ('market_place','BL',2000,3),
    ('town_hall','HC',200,2), ('police','HC',50,1), ('post_office','HC',50,1), ('community_centre','HC',100,1),
    ('library','HC',100,1), ('embassy','HC',50,1), ('fire_station','HC',50,1),
    ('museum','DL',500,2), ('theatre','DL',300,2), ('cinema','DL',500,2), ('attraction','DL',300,1),
    ('monument','DL',100,1), ('memorial','DL',100,1), ('arts_centre','DL',200,1), ('archaeological','DL',100,1),
    ('castle','DL',200,1),
    ('stadium','GT',10000,3), ('sports_centre','GT',500,2), ('swimming_pool','GT',200,1), ('pitch','GT',100,1),
    ('playground','GT',100,1),
    ('park','CV',500,1),
    ('hotel','LT',100,1), ('hostel','LT',30,1), ('guesthouse','LT',20,1), ('motel','LT',20,1);

-- 4a. POI từ shapefile
WITH moi AS (
    SELECT t.gid, t.osm_id, t.name, a.maloai, a.quymo, a.uutien, t.geom
    FROM xebus.tmp_congtrinh t JOIN tmp_anhxa a ON a.fclass = t.fclass
    WHERE t.geom IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM xebus.congtrinh c WHERE c.nguon = 'shp_congtrinh' AND c.osm_id = t.osm_id)
), so_cu AS (
    SELECT coalesce(max(substring(macongtrinh FROM 3)::int), 0) AS n FROM xebus.congtrinh WHERE macongtrinh ~ '^CT[0-9]+$'
)
INSERT INTO xebus.congtrinh (macongtrinh, maloaicongtrinh, tencongtrinh, vido, kinhdo, quymo, mucdouutien,
                             trangthai, nguon_gid, osm_id, nguon, geom)
SELECT 'CT' || lpad((so_cu.n + row_number() OVER (ORDER BY m.maloai, m.gid))::text, 5, '0'),
       m.maloai,
       left(coalesce(nullif(trim(m.name), ''), l.tenloaicongtrinh || ' #' || m.osm_id), 150),
       round(ST_Y(m.geom)::numeric, 7), round(ST_X(m.geom)::numeric, 7),
       m.quymo, m.uutien, 'Hoạt động', m.gid, m.osm_id, 'shp_congtrinh',
       m.geom::geometry(Point, 4326)
FROM moi m CROSS JOIN so_cu JOIN xebus.loaicongtrinh l ON l.maloaicongtrinh = m.maloai;

-- 4b. Khu công nghiệp (điểm đại diện) từ public.sudungdata – chỉ SELECT
WITH moi AS (
    SELECT s.gid, s.name, ST_PointOnSurface(s.geom) AS geom, ST_Area(s.geom::geography) AS dt
    FROM public.sudungdata s
    WHERE s.fclass = 'industrial' AND s.geom IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM xebus.congtrinh c WHERE c.nguon = 'sudungdata' AND c.nguon_gid = s.gid)
), so_cu AS (
    SELECT coalesce(max(substring(macongtrinh FROM 3)::int), 0) AS n FROM xebus.congtrinh WHERE macongtrinh ~ '^CT[0-9]+$'
)
INSERT INTO xebus.congtrinh (macongtrinh, maloaicongtrinh, tencongtrinh, vido, kinhdo, quymo, mucdouutien,
                             trangthai, nguon_gid, nguon, geom)
SELECT 'CT' || lpad((so_cu.n + row_number() OVER (ORDER BY m.gid))::text, 5, '0'),
       'CN',
       left(coalesce(nullif(trim(m.name), ''), 'Khu công nghiệp #' || m.gid), 150),
       round(ST_Y(m.geom)::numeric, 7), round(ST_X(m.geom)::numeric, 7),
       greatest(50, round(m.dt / 80))::int, 3::smallint, 'Hoạt động', m.gid, 'sudungdata',
       m.geom::geometry(Point, 4326)
FROM moi m CROSS JOIN so_cu;

UPDATE xebus.congtrinh c SET maphuong = p.maphuong
FROM xebus.phuongxa p
WHERE ST_Intersects(p.geom, c.geom) AND c.maphuong IS DISTINCT FROM p.maphuong;

-- ---------------------------------------------------------------------
-- 5. Tính lại chi tiết phục vụ, xoá bảng tạm
-- ---------------------------------------------------------------------
SELECT * FROM xebus.fn_tinh_ctpv(500);

DROP TABLE xebus.tmp_phuongxa;
DROP TABLE xebus.tmp_congtrinh;

COMMIT;
