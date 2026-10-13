-- =====================================================================
-- 04_tuyen_mau.sql – Mạng định tuyến (pgRouting) + hàm dựng tuyến + 5 tuyến mẫu
-- Mạng được dựng từ xebus.doanduong (bản sao của public.giaothong) – không đụng public.
-- =====================================================================
BEGIN;

-- ---------------------------------------------------------------------
-- 1. Mạng định tuyến: đỉnh + cạnh (dựng lại mỗi khi chạy script)
-- ---------------------------------------------------------------------
DROP TABLE IF EXISTS xebus.mang_canh;
DROP TABLE IF EXISTS xebus.mang_dinh;

CREATE TABLE xebus.mang_dinh (
    id   bigserial PRIMARY KEY,
    geom geometry(Point, 4326) NOT NULL UNIQUE
);
COMMENT ON TABLE xebus.mang_dinh IS 'Đỉnh mạng định tuyến (sinh từ doanduong, sai số bắt điểm ~2 m)';

CREATE TABLE xebus.mang_canh (
    id           bigserial PRIMARY KEY,
    madoan       varchar(10) REFERENCES xebus.doanduong(madoan) ON DELETE CASCADE,
    source       bigint,
    target       bigint,
    cost         double precision,  -- mét
    reverse_cost double precision,  -- mét (đường 2 chiều)
    geom         geometry(LineString, 4326) NOT NULL
);
COMMENT ON TABLE xebus.mang_canh IS 'Cạnh mạng định tuyến (pgRouting) sinh từ doanduong';

INSERT INTO xebus.mang_canh (madoan, cost, reverse_cost, geom)
SELECT d.madoan, ST_Length(p.geom::geography), ST_Length(p.geom::geography), p.geom
FROM xebus.doanduong d, LATERAL ST_Dump(d.geom) p
WHERE ST_NPoints(p.geom) >= 2;

INSERT INTO xebus.mang_dinh (geom)
SELECT DISTINCT ST_SnapToGrid(pt, 0.00002)
FROM (SELECT ST_StartPoint(geom) pt FROM xebus.mang_canh
      UNION ALL SELECT ST_EndPoint(geom) FROM xebus.mang_canh) a;

UPDATE xebus.mang_canh c SET
    source = (SELECT id FROM xebus.mang_dinh v WHERE v.geom = ST_SnapToGrid(ST_StartPoint(c.geom), 0.00002)),
    target = (SELECT id FROM xebus.mang_dinh v WHERE v.geom = ST_SnapToGrid(ST_EndPoint(c.geom), 0.00002));

CREATE INDEX ix_mang_dinh_geom   ON xebus.mang_dinh USING gist (geom);
CREATE INDEX ix_mang_canh_geom   ON xebus.mang_canh USING gist (geom);
CREATE INDEX ix_mang_canh_source ON xebus.mang_canh (source);
CREATE INDEX ix_mang_canh_target ON xebus.mang_canh (target);

-- ---------------------------------------------------------------------
-- 2. Đỉnh mạng gần một điểm nhất
-- ---------------------------------------------------------------------
CREATE OR REPLACE FUNCTION xebus.fn_dinh_gan_nhat(p_diem geometry)
RETURNS bigint LANGUAGE sql STABLE AS $$
    SELECT v.id FROM xebus.mang_dinh v ORDER BY v.geom <-> p_diem LIMIT 1;
$$;

-- ---------------------------------------------------------------------
-- 3. Đường đi ngắn nhất giữa 2 điểm: trả về các cạnh theo thứ tự, đã xoay đúng chiều đi
-- ---------------------------------------------------------------------
CREATE OR REPLACE FUNCTION xebus.fn_duong_di(p_tu geometry, p_den geometry)
RETURNS TABLE (seq int, madoan varchar, geom geometry(LineString, 4326))
LANGUAGE sql STABLE AS $$
    SELECT r.seq,
           c.madoan,
           (CASE WHEN r.node = c.source THEN c.geom ELSE ST_Reverse(c.geom) END)::geometry(LineString, 4326)
    FROM pgr_dijkstra(
            'SELECT id, source, target, cost, reverse_cost FROM xebus.mang_canh',
            xebus.fn_dinh_gan_nhat(p_tu), xebus.fn_dinh_gan_nhat(p_den), true) r
    JOIN xebus.mang_canh c ON c.id = r.edge
    ORDER BY r.seq;
$$;

-- ---------------------------------------------------------------------
-- 4. Dựng lại hình học tuyến từ danh sách trạm (tuyen_tram theo thutu).
--    Nối từng cặp trạm liên tiếp theo mạng đường; nếu không tìm được đường thì nối thẳng (fallback).
--    Trả về số đoạn phải nối thẳng.
-- ---------------------------------------------------------------------
CREATE OR REPLACE FUNCTION xebus.fn_dung_hinh_tuyen(p_matuyen varchar)
RETURNS int LANGUAGE plpgsql AS $$
DECLARE
    r        record;
    v_truoc  geometry;
    v_doan   geometry;
    v_tatca  geometry[] := '{}';
    v_thang  int := 0;
BEGIN
    FOR r IN SELECT t.geom FROM xebus.tuyen_tram tt JOIN xebus.trambuyt t ON t.matram = tt.matram
             WHERE tt.matuyen = p_matuyen ORDER BY tt.thutu
    LOOP
        IF v_truoc IS NOT NULL THEN
            SELECT ST_LineMerge(ST_Collect(d.geom ORDER BY d.seq)) INTO v_doan
            FROM xebus.fn_duong_di(v_truoc, r.geom) d;
            IF v_doan IS NULL OR ST_IsEmpty(v_doan) THEN
                v_doan  := ST_MakeLine(v_truoc, r.geom);
                v_thang := v_thang + 1;
            ELSE
                -- nối đoạn trạm -> đỉnh mạng ở hai đầu cho liền mạch
                v_tatca := v_tatca || ST_MakeLine(v_truoc, ST_ClosestPoint(v_doan, v_truoc))
                                   || ST_MakeLine(ST_ClosestPoint(v_doan, r.geom), r.geom);
            END IF;
            v_tatca := v_tatca || v_doan;
        END IF;
        v_truoc := r.geom;
    END LOOP;

    UPDATE xebus.tuyenbuyt
    SET geom = CASE WHEN array_length(v_tatca, 1) > 0
                    THEN ST_Multi(ST_LineMerge(ST_Collect(v_tatca)))::geometry(MultiLineString, 4326)
                    END
    WHERE matuyen = p_matuyen;
    RETURN v_thang;
END $$;

-- ---------------------------------------------------------------------
-- 5. Tạo tuyến mẫu: tìm đường ngắn nhất từ trạm đầu -> trạm cuối,
--    lấy các trạm nằm trong p_dem m quanh đường theo thứ tự đi qua,
--    bỏ trạm cách trạm đã chọn trước đó < p_cach m (tránh lấy cả 2 bên đường).
-- ---------------------------------------------------------------------
CREATE OR REPLACE FUNCTION xebus.fn_tao_tuyen_mau(
    p_matuyen varchar, p_tentuyen varchar, p_mota varchar,
    p_tram_dau varchar, p_tram_cuoi varchar,
    p_dem double precision DEFAULT 30, p_cach double precision DEFAULT 250)
RETURNS int LANGUAGE plpgsql AS $$
DECLARE
    v_dau   geometry := (SELECT geom FROM xebus.trambuyt WHERE matram = p_tram_dau);
    v_cuoi  geometry := (SELECT geom FROM xebus.trambuyt WHERE matram = p_tram_cuoi);
    r       record;
    v_truoc geometry;
    v_thutu int := 0;
BEGIN
    IF v_dau IS NULL OR v_cuoi IS NULL THEN
        RAISE NOTICE 'Tuyến %: không tìm thấy trạm đầu/cuối', p_matuyen;
        RETURN 0;
    END IF;
    IF EXISTS (SELECT 1 FROM xebus.tuyenbuyt WHERE matuyen = p_matuyen) THEN
        RAISE NOTICE 'Tuyến % đã tồn tại – bỏ qua', p_matuyen;
        RETURN 0;
    END IF;

    INSERT INTO xebus.tuyenbuyt (matuyen, tentuyen, mota, trangthai, tenntao)
    VALUES (p_matuyen, p_tentuyen, p_mota, 'Hoạt động', 'Dữ liệu mẫu');

    CREATE TEMP TABLE IF NOT EXISTS tmp_duong (seq int, madoan varchar, geom geometry) ON COMMIT DROP;
    TRUNCATE tmp_duong;
    INSERT INTO tmp_duong SELECT * FROM xebus.fn_duong_di(v_dau, v_cuoi);

    FOR r IN
        WITH ung AS (
            SELECT t.matram, t.geom,
                   (SELECT d.seq + ST_LineLocatePoint(d.geom, t.geom)
                    FROM tmp_duong d ORDER BY d.geom <-> t.geom LIMIT 1) AS vitri
            FROM xebus.trambuyt t
            WHERE t.matram IN (p_tram_dau, p_tram_cuoi)
               OR EXISTS (SELECT 1 FROM tmp_duong d WHERE ST_DWithin(d.geom::geography, t.geom::geography, p_dem))
        )
        SELECT matram, geom FROM ung
        ORDER BY CASE WHEN matram = p_tram_dau THEN -1 WHEN matram = p_tram_cuoi THEN 1e9 ELSE vitri END
    LOOP
        IF v_truoc IS NULL OR r.matram = p_tram_cuoi
           OR NOT ST_DWithin(v_truoc::geography, r.geom::geography, p_cach) THEN
            v_thutu := v_thutu + 1;
            INSERT INTO xebus.tuyen_tram (matuyen, matram, thutu) VALUES (p_matuyen, r.matram, v_thutu);
            v_truoc := r.geom;
        END IF;
    END LOOP;

    -- tuyến chính của trạm (cột matuyen theo thiết kế Word) – chỉ gán nếu trạm chưa có tuyến
    UPDATE xebus.trambuyt t SET matuyen = p_matuyen
    WHERE t.matuyen IS NULL AND t.matram IN (SELECT matram FROM xebus.tuyen_tram WHERE matuyen = p_matuyen);

    -- đoạn đường tuyến đi qua (thứ tự theo đường đi) – chỉ gán nếu đoạn chưa thuộc tuyến nào
    UPDATE xebus.doanduong dd SET matuyen = p_matuyen, thutu = s.thutu
    FROM (SELECT madoan, row_number() OVER (ORDER BY min(seq))::smallint AS thutu
          FROM tmp_duong GROUP BY madoan) s
    WHERE dd.madoan = s.madoan AND dd.matuyen IS NULL;

    -- hình học tuyến = đường đi ngắn nhất đầu -> cuối (fn_dung_hinh_tuyen dùng khi người dùng gán lại trạm)
    UPDATE xebus.tuyenbuyt SET geom =
        (SELECT ST_Multi(ST_LineMerge(ST_Collect(geom ORDER BY seq)))::geometry(MultiLineString, 4326) FROM tmp_duong)
    WHERE matuyen = p_matuyen;
    IF (SELECT geom FROM xebus.tuyenbuyt WHERE matuyen = p_matuyen) IS NULL THEN
        PERFORM xebus.fn_dung_hinh_tuyen(p_matuyen);
    END IF;
    RETURN v_thutu;
END $$;

-- ---------------------------------------------------------------------
-- 6. Tuyến mẫu (tên mô phỏng, trạm đầu/cuối là bến xe / điểm trung chuyển có thật trong dữ liệu OSM)
--    Mỗi tuyến một hướng chính, hạn chế chồng lấn. Chạy lại script sẽ xoá và tạo lại các tuyến mẫu
--    (chỉ tuyến có tenntao = 'Dữ liệu mẫu'; tuyến người dùng tạo qua ứng dụng được giữ nguyên).
-- ---------------------------------------------------------------------
UPDATE xebus.trambuyt  SET matuyen = NULL
 WHERE matuyen IN (SELECT matuyen FROM xebus.tuyenbuyt WHERE tenntao = 'Dữ liệu mẫu');
UPDATE xebus.doanduong SET matuyen = NULL, thutu = NULL
 WHERE matuyen IN (SELECT matuyen FROM xebus.tuyenbuyt WHERE tenntao = 'Dữ liệu mẫu');
DELETE FROM xebus.tuyenbuyt WHERE tenntao = 'Dữ liệu mẫu';   -- tuyen_tram xoá theo (ON DELETE CASCADE)

SELECT 'M01' AS tuyen, xebus.fn_tao_tuyen_mau('M01', 'Giáp Bát - Nhổn',
       'Tuyến mẫu hướng Nam - Tây Bắc: BX Giáp Bát - Cầu Giấy - Hồ Tùng Mậu - Điểm trung chuyển Nhổn', 'TR0049', 'TR0410');
SELECT 'M02', xebus.fn_tao_tuyen_mau('M02', 'Hà Đông - Nguyễn Trãi',
       'Tuyến mẫu hướng Tây Nam: Bưu điện Hà Đông - Quang Trung - Nguyễn Trãi', 'TR0210', 'TR0122');
SELECT 'M03', xebus.fn_tao_tuyen_mau('M03', 'Mỹ Đình - Long Biên',
       'Tuyến mẫu hướng Tây - Đông: BX Mỹ Đình - trung tâm - Điểm trung chuyển Long Biên', 'TR0140', 'TR0289');
SELECT 'M04', xebus.fn_tao_tuyen_mau('M04', 'Long Biên - Gia Lâm',
       'Tuyến mẫu hướng Đông: Long Biên - Nguyễn Văn Linh - Nguyễn Đức Thuận', 'TR0289', 'TR0433');
SELECT 'M05', xebus.fn_tao_tuyen_mau('M05', 'Giáp Bát - Ngọc Hồi',
       'Tuyến mẫu hướng Nam: BX Giáp Bát - BX Nước Ngầm - Văn Điển - Ngọc Hồi (QL1A)', 'TR0049', 'TR0966');
SELECT 'M06', xebus.fn_tao_tuyen_mau('M06', 'Long Biên - Đông Anh',
       'Tuyến mẫu hướng Bắc: Điểm trung chuyển Long Biên - qua sông Hồng - Đông Anh', 'TR0289', 'TR0603');

COMMIT;
