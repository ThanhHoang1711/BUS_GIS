-- =====================================================================
-- 03_chuyen_dulieu.sql – Sao chép dữ liệu gốc (schema public) sang các bảng xebus
-- Bảng public CHỈ được SELECT. Không có lệnh nào ghi vào public.
-- Idempotent: mỗi khối chỉ chèn những bản ghi nguồn chưa được chép (theo osm_id / nguon_gid).
-- =====================================================================
BEGIN;

-- ---------------------------------------------------------------------
-- 1. public.giaothong -> xebus.doanduong (SRID gốc = 0 nhưng là lon/lat => gán 4326)
--    chieudaikm theo geography; tgdichuyen giả định vận tốc xe buýt 20 km/h
-- ---------------------------------------------------------------------
WITH moi AS (
    SELECT g.gid, g.ten, g.tendaydu, g.loaiduongb, ST_Multi(ST_SetSRID(g.geom, 4326)) AS geom
    FROM public.giaothong g
    WHERE g.geom IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM xebus.doanduong d WHERE d.nguon_gid = g.gid)
), so_cu AS (
    SELECT coalesce(max(substring(madoan FROM 3)::int), 0) AS n FROM xebus.doanduong WHERE madoan ~ '^DD[0-9]+$'
)
INSERT INTO xebus.doanduong (madoan, tenduong, loaiduong, chieudaikm, tgdichuyen, trangthai, nguon_gid, geom)
SELECT 'DD' || lpad((so_cu.n + row_number() OVER (ORDER BY m.gid))::text, 4, '0'),
       left(coalesce(nullif(trim(m.ten), ''), nullif(split_part(m.tendaydu, ',', 1), ''), 'Đường chưa đặt tên'), 100),
       CASE m.loaiduongb WHEN 1 THEN 'Quốc lộ' WHEN 2 THEN 'Tỉnh lộ' WHEN 3 THEN 'Đường đô thị / huyện'
                         WHEN 4 THEN 'Đường xã' ELSE 'Khác' END,
       round((ST_Length(m.geom::geography) / 1000)::numeric, 3),
       round((ST_Length(m.geom::geography) / 1000 / 20 * 60)::numeric, 2),
       'Hoạt động',
       m.gid,
       m.geom::geometry(MultiLineString, 4326)
FROM moi m CROSS JOIN so_cu;

-- ---------------------------------------------------------------------
-- 2. public.tramxebus -> xebus.trambuyt
--    matram TR0001.. theo thứ tự gid; loại TD (bus_stop) / BX (bus_station)
--    madoan = đoạn đường gần nhất trong phạm vi 100 m
-- ---------------------------------------------------------------------
WITH moi AS (
    SELECT t.*
    FROM public.tramxebus t
    WHERE t.geom IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM xebus.trambuyt x WHERE x.osm_id = t.osm_id)
), so_cu AS (
    SELECT coalesce(max(substring(matram FROM 3)::int), 0) AS n FROM xebus.trambuyt WHERE matram ~ '^TR[0-9]+$'
)
INSERT INTO xebus.trambuyt (matram, maloaitram, madoan, tentram, vido, kinhdo, trangthai, ngaytao, tenntao, osm_id, geom)
SELECT 'TR' || lpad((so_cu.n + row_number() OVER (ORDER BY m.gid))::text, 4, '0'),
       CASE WHEN m.fclass = 'bus_station' THEN 'BX' ELSE 'TD' END,
       (SELECT d.madoan FROM xebus.doanduong d
         WHERE ST_DWithin(d.geom::geography, ST_SetSRID(m.geom, 4326)::geography, 100)
         ORDER BY d.geom <-> ST_SetSRID(m.geom, 4326) LIMIT 1),
       coalesce(nullif(trim(m.name), ''), 'Trạm chưa đặt tên #' || m.gid),
       round(ST_Y(m.geom)::numeric, 7),
       round(ST_X(m.geom)::numeric, 7),
       'Hoạt động',
       now(),
       'Nhập từ OSM (public.tramxebus)',
       m.osm_id,
       ST_SetSRID(m.geom, 4326)::geometry(Point, 4326)
FROM moi m CROSS JOIN so_cu;

-- ---------------------------------------------------------------------
-- 3. public.sudungdata (fclass = residential) -> xebus.khudancu
--    DÂN SỐ LÀ ƯỚC TÍNH (chưa có số liệu thực):
--      danso = diện tích(km²) × 8 000 người/km²  +  số nhà OSM nằm trong khu × 20 người
--    (dữ liệu nhà OSM thưa nên cộng thêm thành phần theo diện tích). Có thể sửa lại sau.
-- ---------------------------------------------------------------------
WITH moi AS (
    SELECT s.gid, s.name, ST_Multi(s.geom) AS geom,
           ST_Area(s.geom::geography) / 1e6 AS dt_km2,
           (SELECT count(*) FROM public.nha h WHERE ST_Intersects(h.geom, s.geom)) AS so_nha
    FROM public.sudungdata s
    WHERE s.fclass = 'residential' AND s.geom IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM xebus.khudancu k WHERE k.nguon_gid = s.gid)
), tinh AS (
    SELECT m.*, round(m.dt_km2 * 8000 + m.so_nha * 20)::int AS danso FROM moi m
), so_cu AS (
    SELECT coalesce(max(substring(makhu FROM 4)::int), 0) AS n FROM xebus.khudancu WHERE makhu ~ '^KDC[0-9]+$'
)
INSERT INTO xebus.khudancu (makhu, tenkhu, vido, kinhdo, danso, matdodancu, trangthai, nguon_gid, geom)
SELECT 'KDC' || lpad((so_cu.n + row_number() OVER (ORDER BY t.gid))::text, 4, '0'),
       left(coalesce(nullif(trim(t.name), ''), 'Khu dân cư #' || t.gid), 150),
       round(ST_Y(ST_PointOnSurface(t.geom))::numeric, 7),
       round(ST_X(ST_PointOnSurface(t.geom))::numeric, 7),
       t.danso,
       CASE WHEN t.dt_km2 > 0 THEN round((t.danso / t.dt_km2)::numeric, 2) END,
       'Hoạt động',
       t.gid,
       t.geom::geometry(MultiPolygon, 4326)
FROM tinh t CROSS JOIN so_cu;

-- ---------------------------------------------------------------------
-- 4. public.sudungdata (commercial/retail/industrial/park/recreation_ground) -> xebus.congtrinh
--    quymo = ước tính số người sử dụng theo diện tích (m² / hệ số theo loại)
--    mucdouutien: 1 thấp .. 3 cao
-- ---------------------------------------------------------------------
WITH loai AS (
    SELECT * FROM (VALUES
        ('commercial',        'TM', 'Khu thương mại',     20.0, 2::smallint),
        ('retail',            'BL', 'Khu bán lẻ',         20.0, 2::smallint),
        ('industrial',        'CN', 'Khu công nghiệp',    80.0, 3::smallint),
        ('park',              'CV', 'Công viên',         300.0, 1::smallint),
        ('recreation_ground', 'GT', 'Khu vui chơi',      150.0, 1::smallint)
    ) v(fclass, maloai, tenmacdinh, m2_moi_nguoi, uutien)
), moi AS (
    SELECT s.gid, s.name, l.maloai, l.tenmacdinh, l.m2_moi_nguoi, l.uutien, ST_Multi(s.geom) AS geom
    FROM public.sudungdata s JOIN loai l ON l.fclass = s.fclass
    WHERE s.geom IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM xebus.congtrinh c WHERE c.nguon_gid = s.gid)
), so_cu AS (
    SELECT coalesce(max(substring(macongtrinh FROM 3)::int), 0) AS n FROM xebus.congtrinh WHERE macongtrinh ~ '^CT[0-9]+$'
)
INSERT INTO xebus.congtrinh (macongtrinh, maloaicongtrinh, tencongtrinh, vido, kinhdo, quymo, mucdouutien, trangthai, nguon_gid, geom)
SELECT 'CT' || lpad((so_cu.n + row_number() OVER (ORDER BY m.gid))::text, 4, '0'),
       m.maloai,
       left(coalesce(nullif(trim(m.name), ''), m.tenmacdinh || ' #' || m.gid), 150),
       round(ST_Y(ST_PointOnSurface(m.geom))::numeric, 7),
       round(ST_X(ST_PointOnSurface(m.geom))::numeric, 7),
       greatest(1, round(ST_Area(m.geom::geography) / m.m2_moi_nguoi))::int,
       m.uutien,
       'Hoạt động',
       m.gid,
       m.geom::geometry(MultiPolygon, 4326)
FROM moi m CROSS JOIN so_cu;

COMMIT;
