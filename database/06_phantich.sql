-- =====================================================================
-- 06_phantich.sql – Hàm phân tích không gian (Đề xuất trạm mới, Thống kê)
-- Tất cả trong schema xebus. Vùng phân tích (p_vung) là geometry SRID 4326; NULL = toàn bộ dữ liệu.
-- Tính toán khoảng cách/diện tích trên hệ phẳng UTM 48N (EPSG:32648, đơn vị mét – chính xác cho Hà Nội),
-- kết quả hình học trả về SRID 4326.
-- =====================================================================
BEGIN;

-- Xoá phiên bản cũ của các hàm (chỉ hàm thuộc xebus) để đổi được kiểu trả về
DROP FUNCTION IF EXISTS xebus.fn_vung_phucvu(double precision, geometry);
DROP FUNCTION IF EXISTS xebus.fn_phucvu_kdc(geometry, double precision);
DROP FUNCTION IF EXISTS xebus.fn_khuvuc_thieutram(geometry, double precision, double precision);
DROP FUNCTION IF EXISTS xebus.fn_danhgia_diem(geometry, double precision, geometry);
DROP FUNCTION IF EXISTS xebus.fn_ungvien(geometry, double precision, double precision, double precision, integer);
DROP FUNCTION IF EXISTS xebus.fn_thongke_phamvi(double precision, geometry);
DROP FUNCTION IF EXISTS xebus.fn_danhgia_diem(geometry, double precision);
DROP FUNCTION IF EXISTS xebus.fn_ungvien(geometry, double precision, double precision, double precision, integer, double precision);
DROP FUNCTION IF EXISTS xebus.fn_thongke_luoi(double precision, geometry);

-- Index hàm trên toạ độ UTM để ST_DWithin theo mét dùng được index
CREATE INDEX IF NOT EXISTS ix_trambuyt_utm  ON xebus.trambuyt  USING gist (ST_Transform(geom, 32648));
CREATE INDEX IF NOT EXISTS ix_khudancu_utm  ON xebus.khudancu  USING gist (ST_Transform(geom, 32648));
CREATE INDEX IF NOT EXISTS ix_congtrinh_utm ON xebus.congtrinh USING gist (ST_Transform(geom, 32648));
CREATE INDEX IF NOT EXISTS ix_doanduong_utm ON xebus.doanduong USING gist (ST_Transform(geom, 32648));

-- ---------------------------------------------------------------------
-- 0. Vùng phục vụ (hợp các vòng tròn bán kính r quanh trạm đang hoạt động / đề xuất) – để hiển thị
-- ---------------------------------------------------------------------
CREATE FUNCTION xebus.fn_vung_phucvu(p_bankinh double precision DEFAULT 500, p_vung geometry DEFAULT NULL)
RETURNS geometry LANGUAGE sql STABLE AS $$
    SELECT ST_Transform(ST_Union(ST_Buffer(ST_Transform(t.geom, 32648), p_bankinh, 8)), 4326)
    FROM xebus.trambuyt t
    WHERE t.trangthai <> 'Ngừng hoạt động'
      AND (p_vung IS NULL OR ST_DWithin(ST_Transform(t.geom, 32648), ST_Transform(p_vung, 32648), p_bankinh));
$$;

-- ---------------------------------------------------------------------
-- 1. Tính lại bảng chi tiết phục vụ (B06.1, B06.2)
--    p_matram NULL = tính cho tất cả trạm; ngược lại chỉ cho 1 trạm
-- ---------------------------------------------------------------------
CREATE OR REPLACE FUNCTION xebus.fn_tinh_ctpv(p_bankinh double precision DEFAULT 500, p_matram varchar DEFAULT NULL)
RETURNS TABLE (so_kdc bigint, so_congtrinh bigint) LANGUAGE plpgsql AS $$
BEGIN
    DELETE FROM xebus.ctpvkhudancu  WHERE p_matram IS NULL OR matram = p_matram;
    DELETE FROM xebus.ctpvcongtrinh WHERE p_matram IS NULL OR matram = p_matram;

    INSERT INTO xebus.ctpvkhudancu (matram, makhu, khoangcachm)
    SELECT t.matram, k.makhu, round(ST_Distance(ST_Transform(t.geom, 32648), ST_Transform(k.geom, 32648))::numeric, 2)
    FROM xebus.trambuyt t
    JOIN xebus.khudancu k ON ST_DWithin(ST_Transform(t.geom, 32648), ST_Transform(k.geom, 32648), p_bankinh)
    WHERE (p_matram IS NULL OR t.matram = p_matram) AND t.trangthai <> 'Ngừng hoạt động';

    INSERT INTO xebus.ctpvcongtrinh (matram, macongtrinh, khoangcachm)
    SELECT t.matram, c.macongtrinh, round(ST_Distance(ST_Transform(t.geom, 32648), ST_Transform(c.geom, 32648))::numeric, 2)
    FROM xebus.trambuyt t
    JOIN xebus.congtrinh c ON ST_DWithin(ST_Transform(t.geom, 32648), ST_Transform(c.geom, 32648), p_bankinh)
    WHERE (p_matram IS NULL OR t.matram = p_matram) AND t.trangthai <> 'Ngừng hoạt động';

    RETURN QUERY SELECT (SELECT count(*) FROM xebus.ctpvkhudancu), (SELECT count(*) FROM xebus.ctpvcongtrinh);
END $$;

-- ---------------------------------------------------------------------
-- 2. Mức độ phục vụ của từng khu dân cư (lõi cho Quy trình 3.2 và 4.3)
--    Với mỗi KDC: hợp vòng phục vụ của các trạm gần nó (cục bộ => nhanh), phần chưa phục vụ,
--    tỷ lệ diện tích được phục vụ, dân số chưa phục vụ (giả định dân phân bố đều trong khu).
--    chua_pv_utm: phần chưa phục vụ (SRID 32648) – dùng nội bộ cho chấm điểm ứng viên.
-- ---------------------------------------------------------------------
CREATE FUNCTION xebus.fn_phucvu_kdc(p_vung geometry DEFAULT NULL, p_bankinh double precision DEFAULT 500)
RETURNS TABLE (makhu varchar, tenkhu varchar, danso int, dientich_m2 double precision,
               tyle_phucvu double precision, chua_pv_utm geometry)
LANGUAGE sql STABLE AS $$
    WITH kdc AS (
        SELECT k.makhu, k.tenkhu, coalesce(k.danso, 0) AS danso, ST_Transform(k.geom, 32648) AS gu
        FROM xebus.khudancu k
        WHERE k.trangthai <> 'Ngừng hoạt động'
          AND (p_vung IS NULL OR ST_Intersects(k.geom, p_vung))
    ), tinh AS (
        SELECT k.*,
               (SELECT ST_Union(ST_Buffer(ST_Transform(t.geom, 32648), p_bankinh, 8))
                  FROM xebus.trambuyt t
                 WHERE t.trangthai <> 'Ngừng hoạt động'
                   AND ST_DWithin(ST_Transform(t.geom, 32648), k.gu, p_bankinh)) AS pv
        FROM kdc k
    ), hieu AS (
        SELECT t.makhu, t.tenkhu, t.danso, ST_Area(t.gu) AS dt,
               CASE WHEN t.pv IS NULL THEN t.gu
                    ELSE ST_CollectionExtract(ST_Difference(t.gu, t.pv), 3) END AS chua
        FROM tinh t
    )
    SELECT h.makhu, h.tenkhu, h.danso, h.dt,
           CASE WHEN h.dt = 0 THEN 1.0 ELSE greatest(0, 1 - ST_Area(h.chua) / h.dt) END,
           h.chua
    FROM hieu h;
$$;

-- ---------------------------------------------------------------------
-- 3. Khu vực thiếu trạm (Quy trình 3.2): các KDC có tỷ lệ phục vụ < p_nguong
--    geom = phần diện tích chưa được phục vụ (SRID 4326) để tô đỏ trên bản đồ
-- ---------------------------------------------------------------------
CREATE FUNCTION xebus.fn_khuvuc_thieutram(
    p_vung geometry DEFAULT NULL, p_bankinh double precision DEFAULT 500, p_nguong double precision DEFAULT 0.8)
RETURNS TABLE (makhu varchar, tenkhu varchar, danso int, tyle_phucvu numeric, danso_chua_pv int,
               kc_tram_gan_nhat numeric, geom geometry)
LANGUAGE sql STABLE AS $$
    SELECT p.makhu, p.tenkhu, p.danso,
           round(p.tyle_phucvu::numeric, 3),
           round(p.danso * (1 - p.tyle_phucvu))::int,
           (SELECT round(ST_Distance(ST_Transform(s.geom, 32648), ST_Transform(k.geom, 32648))::numeric, 0)
              FROM xebus.trambuyt s WHERE s.trangthai <> 'Ngừng hoạt động'
              ORDER BY s.geom <-> k.geom LIMIT 1),
           ST_Multi(ST_Transform(p.chua_pv_utm, 4326))
    FROM xebus.fn_phucvu_kdc(p_vung, p_bankinh) p
    JOIN xebus.khudancu k ON k.makhu = p.makhu
    WHERE p.tyle_phucvu < p_nguong AND NOT ST_IsEmpty(p.chua_pv_utm)
    ORDER BY round(p.danso * (1 - p.tyle_phucvu)) DESC;
$$;

-- ---------------------------------------------------------------------
-- 4. Đánh giá một vị trí bất kỳ (người dùng tự chọn điểm trên bản đồ)
--    dan_moi   : dân số chưa được phục vụ sẽ được phục vụ thêm nếu đặt trạm tại điểm
--    so_kdc    : số khu dân cư trong bán kính
--    so_ct     : số công trình trong bán kính
--    diem_ct   : Σ trongso(loại công trình) × mucdouutien
--    tong_diem : dan_moi / 1000 + diem_ct  (chỉ tiêu tổng hợp để xếp hạng)
-- ---------------------------------------------------------------------
CREATE FUNCTION xebus.fn_danhgia_diem(p_diem geometry, p_bankinh double precision DEFAULT 500)
RETURNS TABLE (dan_moi int, so_kdc int, so_ct int, diem_ct numeric, tong_diem numeric, kc_tram_gan_nhat numeric)
LANGUAGE sql STABLE AS $$
    WITH vt AS (
        SELECT ST_Transform(p_diem, 32648) AS du,
               ST_Buffer(ST_Transform(p_diem, 32648), p_bankinh, 8) AS vong
    ), kdc AS (
        SELECT p.* FROM xebus.fn_phucvu_kdc(ST_Transform((SELECT vong FROM vt), 4326), p_bankinh) p
    ), dan AS (
        SELECT coalesce(sum(k.danso * ST_Area(ST_Intersection(k.chua_pv_utm, vt.vong)) / nullif(k.dientich_m2, 0)), 0) AS d
        FROM kdc k, vt WHERE NOT ST_IsEmpty(k.chua_pv_utm) AND ST_Intersects(k.chua_pv_utm, vt.vong)
    ), ct AS (
        SELECT count(*) AS n, coalesce(sum(l.trongso * coalesce(c.mucdouutien, 1)), 0) AS d
        FROM xebus.congtrinh c JOIN xebus.loaicongtrinh l USING (maloaicongtrinh), vt
        WHERE ST_DWithin(ST_Transform(c.geom, 32648), vt.du, p_bankinh)
    )
    SELECT round(dan.d)::int,
           (SELECT count(*)::int FROM kdc),
           ct.n::int,
           round(ct.d, 2),
           round((dan.d / 1000 + ct.d)::numeric, 2),
           (SELECT round(ST_Distance(ST_Transform(s.geom, 32648), vt.du)::numeric, 0)
              FROM xebus.trambuyt s, vt WHERE s.trangthai <> 'Ngừng hoạt động'
              ORDER BY ST_Transform(s.geom, 32648) <-> vt.du LIMIT 1)
    FROM dan, ct;
$$;

-- ---------------------------------------------------------------------
-- 5. Ứng viên trạm (Quy trình 3.3 + 3.4)
--    - Xác định KDC thiếu trạm trong vùng (tỷ lệ phục vụ < p_nguong)
--    - Rải điểm dọc đoạn đường gần các khu đó, cách nhau p_buoc m
--    - Loại điểm cách trạm hiện có < p_cach_toithieu m
--    - Chấm điểm, chọn tối đa p_toida ứng viên tốt nhất, các ứng viên cách nhau ≥ p_cach_toithieu
-- ---------------------------------------------------------------------
CREATE FUNCTION xebus.fn_ungvien(
    p_vung geometry DEFAULT NULL,
    p_bankinh double precision DEFAULT 500,
    p_cach_toithieu double precision DEFAULT 300,
    p_buoc double precision DEFAULT 150,
    p_toida int DEFAULT 10,
    p_nguong double precision DEFAULT 0.8)
RETURNS TABLE (stt int, kinhdo numeric, vido numeric, madoan varchar, tenduong varchar,
               dan_moi int, so_kdc int, so_ct int, diem_ct numeric, tong_diem numeric, geom geometry)
LANGUAGE plpgsql AS $$
#variable_conflict use_column
DECLARE
    r       record;
    v_chon  geometry[] := '{}';
    v_stt   int := 0;
    v_trung boolean;
    g       geometry;
BEGIN
    DROP TABLE IF EXISTS pg_temp.tmp_thieu;
    CREATE TEMP TABLE tmp_thieu AS
        SELECT p.makhu, p.danso, p.dientich_m2, p.chua_pv_utm AS gu
        FROM xebus.fn_phucvu_kdc(p_vung, p_bankinh) p
        WHERE p.tyle_phucvu < p_nguong AND NOT ST_IsEmpty(p.chua_pv_utm);
    CREATE INDEX ON tmp_thieu USING gist (gu);
    ANALYZE tmp_thieu;

    IF NOT EXISTS (SELECT 1 FROM tmp_thieu) THEN
        RETURN;
    END IF;

    -- điểm rải dọc đường (UTM)
    DROP TABLE IF EXISTS pg_temp.tmp_diem;
    CREATE TEMP TABLE tmp_diem AS
    WITH duong AS (
        SELECT d.madoan, d.tenduong, (ST_Dump(ST_Transform(d.geom, 32648))).geom AS g
        FROM xebus.doanduong d
        WHERE d.trangthai <> 'Ngừng hoạt động'
          AND (p_vung IS NULL OR ST_Intersects(d.geom, p_vung))
          AND EXISTS (SELECT 1 FROM tmp_thieu t WHERE ST_DWithin(t.gu, ST_Transform(d.geom, 32648), p_bankinh))
    ), diem AS (
        SELECT du.madoan, du.tenduong, pt.geom AS gu
        FROM duong du
        CROSS JOIN LATERAL ST_Dump(
            CASE WHEN ST_Length(du.g) <= p_buoc THEN ST_LineInterpolatePoint(du.g, 0.5)
                 ELSE ST_LineInterpolatePoints(du.g, p_buoc / ST_Length(du.g)) END) pt
        WHERE ST_Length(du.g) > 0
    )
    SELECT dm.madoan, dm.tenduong, dm.gu, ST_Buffer(dm.gu, p_bankinh, 8) AS vong
    FROM diem dm
    WHERE (p_vung IS NULL OR ST_Intersects(dm.gu, ST_Transform(p_vung, 32648)))
      AND NOT EXISTS (SELECT 1 FROM xebus.trambuyt s
                      WHERE s.trangthai <> 'Ngừng hoạt động'
                        AND ST_DWithin(ST_Transform(s.geom, 32648), dm.gu, p_cach_toithieu))
      AND EXISTS (SELECT 1 FROM tmp_thieu t WHERE ST_DWithin(t.gu, dm.gu, p_bankinh));

    FOR r IN
        WITH cham AS (
            SELECT d.madoan, d.tenduong, d.gu, d.vong,
                   coalesce((SELECT sum(t.danso * ST_Area(ST_Intersection(t.gu, d.vong)) / nullif(t.dientich_m2, 0))
                             FROM tmp_thieu t WHERE ST_Intersects(t.gu, d.vong)), 0) AS dan
            FROM tmp_diem d
        )
        SELECT c.madoan, c.tenduong, c.gu, round(c.dan)::int AS dan_moi, ct.n AS so_ct, ct.d AS diem_ct,
               (SELECT count(*)::int FROM xebus.khudancu k
                 WHERE ST_DWithin(ST_Transform(k.geom, 32648), c.gu, p_bankinh)) AS so_kdc,
               round((c.dan / 1000.0 + ct.d)::numeric, 2) AS tong
        FROM cham c
        CROSS JOIN LATERAL (
            SELECT count(*)::int AS n, coalesce(sum(l.trongso * coalesce(cg.mucdouutien, 1)), 0)::numeric AS d
            FROM xebus.congtrinh cg JOIN xebus.loaicongtrinh l USING (maloaicongtrinh)
            WHERE ST_DWithin(ST_Transform(cg.geom, 32648), c.gu, p_bankinh)) ct
        WHERE c.dan >= 1
        ORDER BY 8 DESC
    LOOP
        v_trung := false;
        FOREACH g IN ARRAY v_chon LOOP
            IF ST_DWithin(g, r.gu, p_cach_toithieu) THEN v_trung := true; EXIT; END IF;
        END LOOP;
        CONTINUE WHEN v_trung;

        v_chon := v_chon || r.gu;
        v_stt  := v_stt + 1;
        g := ST_Transform(r.gu, 4326);
        stt := v_stt; kinhdo := round(ST_X(g)::numeric, 7); vido := round(ST_Y(g)::numeric, 7);
        madoan := r.madoan; tenduong := r.tenduong; dan_moi := r.dan_moi; so_kdc := r.so_kdc;
        so_ct := r.so_ct; diem_ct := round(r.diem_ct, 2); tong_diem := r.tong; geom := g;
        RETURN NEXT;
        EXIT WHEN v_stt >= p_toida;
    END LOOP;
END $$;

-- ---------------------------------------------------------------------
-- 6. Tạo trạm đề xuất (Quy trình 3.5) – trả về mã trạm mới
-- ---------------------------------------------------------------------
CREATE OR REPLACE FUNCTION xebus.fn_tao_tram_dexuat(
    p_kinhdo numeric, p_vido numeric, p_tentram varchar, p_maloaitram varchar DEFAULT 'TD',
    p_matuyen varchar DEFAULT NULL, p_idntao int DEFAULT NULL, p_tenntao varchar DEFAULT NULL,
    p_bankinh double precision DEFAULT 500)
RETURNS varchar LANGUAGE plpgsql AS $$
DECLARE
    v_ma   varchar := xebus.fn_ma_tiep('trambuyt', 'matram', 'TR');
    v_diem geometry := ST_SetSRID(ST_MakePoint(p_kinhdo, p_vido), 4326);
BEGIN
    INSERT INTO xebus.trambuyt (matram, matuyen, maloaitram, madoan, tentram, vido, kinhdo, trangthai,
                                idntao, tenntao, maphuong, geom)
    VALUES (v_ma, p_matuyen, p_maloaitram,
            (SELECT d.madoan FROM xebus.doanduong d ORDER BY d.geom <-> v_diem LIMIT 1),
            p_tentram, p_vido, p_kinhdo, 'Đề xuất', p_idntao, p_tenntao,
            (SELECT x.maphuong FROM xebus.phuongxa x WHERE ST_Contains(x.geom, v_diem) LIMIT 1),
            v_diem);
    PERFORM xebus.fn_tinh_ctpv(p_bankinh, v_ma);
    RETURN v_ma;
END $$;

-- ---------------------------------------------------------------------
-- 7. Thống kê
-- ---------------------------------------------------------------------
-- 7.1 Số trạm theo tuyến (Quy trình 4.2)
CREATE OR REPLACE VIEW xebus.v_thongke_tuyen AS
SELECT t.matuyen, t.tentuyen, t.trangthai,
       count(tt.matram)                                           AS so_tram,
       round((ST_Length(t.geom::geography) / 1000)::numeric, 2)   AS chieudai_km
FROM xebus.tuyenbuyt t
LEFT JOIN xebus.tuyen_tram tt ON tt.matuyen = t.matuyen
GROUP BY t.matuyen, t.tentuyen, t.trangthai, t.geom;

-- 7.2 Số trạm theo phường / quận (Quy trình 4.1) – dùng khi đã import ranh giới
CREATE OR REPLACE VIEW xebus.v_thongke_phuong AS
SELECT p.maphuong, p.tenphuong, q.maquan, q.tenquan,
       count(t.matram) AS so_tram,
       round((ST_Area(p.geom::geography) / 1e6)::numeric, 2) AS dientich_km2,
       p.geom
FROM xebus.phuongxa p
LEFT JOIN xebus.quanhuyen q ON q.maquan = p.maquan
LEFT JOIN xebus.trambuyt t ON ST_Contains(p.geom, t.geom) AND t.trangthai <> 'Ngừng hoạt động'
GROUP BY p.maphuong, p.tenphuong, q.maquan, q.tenquan, p.geom;

CREATE OR REPLACE VIEW xebus.v_thongke_quan AS
SELECT q.maquan, q.tenquan,
       count(t.matram) AS so_tram,
       round((ST_Area(q.geom::geography) / 1e6)::numeric, 2) AS dientich_km2,
       q.geom
FROM xebus.quanhuyen q
LEFT JOIN xebus.trambuyt t ON ST_Contains(q.geom, t.geom) AND t.trangthai <> 'Ngừng hoạt động'
GROUP BY q.maquan, q.tenquan, q.geom;

-- 7.3 Số trạm theo lưới ô vuông (thay thế khi chưa có ranh giới hành chính)
CREATE FUNCTION xebus.fn_thongke_luoi(p_kichthuoc double precision DEFAULT 2000, p_vung geometry DEFAULT NULL)
RETURNS TABLE (o_id text, so_tram int, geom geometry) LANGUAGE sql STABLE AS $$
    WITH ranh AS MATERIALIZED (
        SELECT ST_Simplify(ST_Transform(coalesce(p_vung, (SELECT ST_Union(h.geom) FROM public.hanoi h)), 32648), 50) AS g
    ), o AS (
        SELECT q.i, q.j, q.geom
        FROM ranh, LATERAL ST_SquareGrid(p_kichthuoc, ranh.g) q
        WHERE ST_Intersects(q.geom, ranh.g)
    ), dem AS (   -- chỉ số ô của ST_SquareGrid = floor(toạ độ / kích thước)
        SELECT floor(ST_X(ST_Transform(t.geom, 32648)) / p_kichthuoc)::int AS i,
               floor(ST_Y(ST_Transform(t.geom, 32648)) / p_kichthuoc)::int AS j,
               count(*)::int AS n
        FROM xebus.trambuyt t
        WHERE t.trangthai <> 'Ngừng hoạt động'
        GROUP BY 1, 2
    )
    SELECT o.i || '_' || o.j, coalesce(dem.n, 0), ST_Transform(o.geom, 4326)
    FROM o LEFT JOIN dem ON dem.i = o.i AND dem.j = o.j;
$$;

-- 7.4 Phạm vi phục vụ (Quy trình 4.3)
CREATE FUNCTION xebus.fn_thongke_phamvi(p_bankinh double precision DEFAULT 500, p_vung geometry DEFAULT NULL)
RETURNS TABLE (so_tram int, so_kdc int, tong_danso bigint, danso_pv bigint, tyle_pv numeric,
               so_ct int, so_ct_pv int)
LANGUAGE sql STABLE AS $$
    WITH kdc AS (SELECT * FROM xebus.fn_phucvu_kdc(p_vung, p_bankinh)),
    ct AS (
        SELECT c.macongtrinh,
               EXISTS (SELECT 1 FROM xebus.trambuyt t
                        WHERE t.trangthai <> 'Ngừng hoạt động'
                          AND ST_DWithin(ST_Transform(t.geom, 32648), ST_Transform(c.geom, 32648), p_bankinh)) AS pv
        FROM xebus.congtrinh c
        WHERE p_vung IS NULL OR ST_Intersects(c.geom, p_vung)
    )
    SELECT (SELECT count(*)::int FROM xebus.trambuyt t
             WHERE t.trangthai <> 'Ngừng hoạt động' AND (p_vung IS NULL OR ST_Intersects(t.geom, p_vung))),
           (SELECT count(*)::int FROM kdc),
           (SELECT coalesce(sum(danso), 0)::bigint FROM kdc),
           (SELECT coalesce(round(sum(danso * tyle_phucvu)), 0)::bigint FROM kdc),
           (SELECT round((sum(danso * tyle_phucvu) / nullif(sum(danso), 0) * 100)::numeric, 1) FROM kdc),
           (SELECT count(*)::int FROM ct),
           (SELECT count(*)::int FROM ct WHERE pv);
$$;

-- ---------------------------------------------------------------------
-- 8. Tính chi tiết phục vụ (bán kính 500 m)
-- ---------------------------------------------------------------------
SELECT * FROM xebus.fn_tinh_ctpv(500);

COMMIT;
