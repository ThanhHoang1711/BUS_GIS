-- =====================================================================
-- 11_dexuat_v2.sql – Thuật toán đề xuất trạm mới (Quy trình 3.3 – 3.5), phiên bản 2
-- Thay thế fn_ungvien / fn_danhgia_diem của 06_phantich.sql. Chỉ trong schema xebus;
-- public.thuyhea chỉ được SELECT (loại vị trí trên mặt nước = trên cầu).
--
-- Cải tiến so với v1:
--   1. Chỉ tính dân số và công trình CHƯA được phục vụ (không có trạm trong bán kính r).
--   2. Điểm tổng hợp: tong_diem = 3 × dan_moi / 1000 + Σ trongso × mucdouutien (công trình chưa phục vụ).
--      (1 000 dân mới ≈ 3 điểm; 1 bệnh viện ≈ 7,5 điểm; 1 trường học ≈ 4 điểm; 1 khách sạn ≈ 0,5 điểm.)
--   3. Chọn tham lam theo bài toán phủ tối đa (maximal covering): mỗi lần chọn ứng viên tốt nhất rồi trừ
--      phần dân số / công trình nó đã phủ, chấm lại các ứng viên còn lại.
--   4. Loại vị trí không đặt được trạm: hầm chui, cầu vượt, đường dẫn lên cầu, điểm nằm trên mặt nước.
--   5. Nếu khu dân cư thiếu trạm không có đường trong mạng đường đủ gần, đặt ứng viên tại điểm đại diện
--      của phần chưa được phục vụ (nguon = 'Khu dân cư' – cần khảo sát đường tiếp cận thực tế).
-- =====================================================================
BEGIN;

DROP FUNCTION IF EXISTS xebus.fn_ungvien(geometry, double precision, double precision, double precision, integer, double precision);
DROP FUNCTION IF EXISTS xebus.fn_danhgia_diem(geometry, double precision);

-- ---------------------------------------------------------------------
-- Vị trí có thể đặt trạm? (không phải hầm/cầu vượt/đường dẫn cầu, không trên mặt nước)
-- ---------------------------------------------------------------------
CREATE OR REPLACE FUNCTION xebus.fn_duong_dat_duoc_tram(p_tenduong text)
RETURNS boolean LANGUAGE sql IMMUTABLE AS $$
    SELECT p_tenduong IS NULL
        OR xebus.unaccent_vn(p_tenduong) !~ '^(ham |cau vuot|duong dan cau|di cau|doc cau)';
$$;

CREATE OR REPLACE FUNCTION xebus.fn_tren_mat_nuoc(p_diem geometry)
RETURNS boolean LANGUAGE sql STABLE AS $$
    SELECT EXISTS (SELECT 1 FROM public.thuyhea w
                   WHERE w.fclass IN ('water', 'riverbank', 'reservoir') AND ST_Intersects(w.geom, p_diem));
$$;

-- ---------------------------------------------------------------------
-- Ứng viên trạm mới
-- ---------------------------------------------------------------------
CREATE FUNCTION xebus.fn_ungvien(
    p_vung geometry DEFAULT NULL,
    p_bankinh double precision DEFAULT 500,
    p_cach_toithieu double precision DEFAULT 300,
    p_buoc double precision DEFAULT 150,
    p_toida int DEFAULT 10,
    p_nguong double precision DEFAULT 0.8)
RETURNS TABLE (stt int, kinhdo numeric, vido numeric, madoan varchar, tenduong varchar, nguon text,
               maphuong varchar, tenphuong varchar,
               dan_moi int, so_kdc int, so_ct_moi int, diem_ct numeric, tong_diem numeric,
               kc_tram_gan_nhat int, geom geometry)
LANGUAGE plpgsql AS $$
#variable_conflict use_column
DECLARE
    v_vung_utm geometry := CASE WHEN p_vung IS NULL THEN NULL ELSE ST_Transform(p_vung, 32648) END;
    v_stt   int := 0;
    r       record;
    v_vong  geometry;
    g4326   geometry;
BEGIN
    -- 1. Phần diện tích KDC chưa được phục vụ (UTM)
    DROP TABLE IF EXISTS pg_temp.uv_thieu;
    CREATE TEMP TABLE uv_thieu AS
        SELECT p.makhu, p.danso, p.dientich_m2, p.chua_pv_utm AS gu
        FROM xebus.fn_phucvu_kdc(p_vung, p_bankinh) p
        WHERE p.tyle_phucvu < p_nguong AND NOT ST_IsEmpty(p.chua_pv_utm) AND p.danso > 0;
    CREATE INDEX ON uv_thieu USING gist (gu);
    ANALYZE uv_thieu;

    -- 2. Công trình chưa được phục vụ (trong vùng mở rộng r)
    DROP TABLE IF EXISTS pg_temp.uv_ct;
    CREATE TEMP TABLE uv_ct AS
        SELECT c.macongtrinh, ST_Transform(c.geom, 32648) AS gu,
               (l.trongso * coalesce(c.mucdouutien, 1))::numeric AS diem
        FROM xebus.congtrinh c JOIN xebus.loaicongtrinh l USING (maloaicongtrinh)
        WHERE c.trangthai <> 'Ngừng hoạt động'
          AND (v_vung_utm IS NULL OR ST_DWithin(ST_Transform(c.geom, 32648), v_vung_utm, p_bankinh))
          AND NOT EXISTS (SELECT 1 FROM xebus.trambuyt s
                          WHERE s.trangthai <> 'Ngừng hoạt động'
                            AND ST_DWithin(ST_Transform(s.geom, 32648), ST_Transform(c.geom, 32648), p_bankinh));
    CREATE INDEX ON uv_ct USING gist (gu);
    ANALYZE uv_ct;

    IF NOT EXISTS (SELECT 1 FROM uv_thieu) AND NOT EXISTS (SELECT 1 FROM uv_ct) THEN
        RETURN;
    END IF;

    -- 3. Điểm ứng viên
    DROP TABLE IF EXISTS pg_temp.uv_diem;
    CREATE TEMP TABLE uv_diem (id serial PRIMARY KEY, madoan varchar, tenduong varchar, nguon text, gu geometry);

    -- 3a. Rải dọc đoạn đường gần nơi thiếu phục vụ
    INSERT INTO uv_diem (madoan, tenduong, nguon, gu)
    SELECT du.madoan, du.tenduong, 'Đường', pt.geom
    FROM (
        SELECT d.madoan, d.tenduong, (ST_Dump(ST_Transform(d.geom, 32648))).geom AS g
        FROM xebus.doanduong d
        WHERE d.trangthai <> 'Ngừng hoạt động'
          AND xebus.fn_duong_dat_duoc_tram(d.tenduong)
          AND (v_vung_utm IS NULL OR ST_Intersects(ST_Transform(d.geom, 32648), v_vung_utm))
          AND (EXISTS (SELECT 1 FROM uv_thieu t WHERE ST_DWithin(t.gu, ST_Transform(d.geom, 32648), p_bankinh))
               OR EXISTS (SELECT 1 FROM uv_ct c WHERE ST_DWithin(c.gu, ST_Transform(d.geom, 32648), p_bankinh)))
    ) du
    CROSS JOIN LATERAL ST_Dump(
        CASE WHEN ST_Length(du.g) <= p_buoc THEN ST_LineInterpolatePoint(du.g, 0.5)
             ELSE ST_LineInterpolatePoints(du.g, p_buoc / ST_Length(du.g)) END) pt
    WHERE ST_Length(du.g) > 0
      AND (v_vung_utm IS NULL OR ST_Intersects(pt.geom, v_vung_utm));

    -- 3b. Fallback: KDC thiếu trạm không có điểm đường nào trong bán kính
    INSERT INTO uv_diem (madoan, tenduong, nguon, gu)
    SELECT NULL, k.tenkhu, 'Khu dân cư',
           ST_PointOnSurface((SELECT d.geom FROM ST_Dump(x.phan) d ORDER BY ST_Area(d.geom) DESC LIMIT 1))
    FROM uv_thieu t
    JOIN xebus.khudancu k ON k.makhu = t.makhu
    CROSS JOIN LATERAL (   -- chỉ lấy phần nằm trong vùng phân tích
        SELECT CASE WHEN v_vung_utm IS NULL THEN t.gu
                    ELSE ST_CollectionExtract(ST_Intersection(t.gu, v_vung_utm), 3) END AS phan) x
    WHERE NOT ST_IsEmpty(x.phan)
      AND NOT EXISTS (SELECT 1 FROM uv_diem u WHERE ST_DWithin(u.gu, t.gu, p_bankinh));

    -- loại điểm quá gần trạm hiện có hoặc nằm trên mặt nước (trên cầu)
    DELETE FROM uv_diem u
    WHERE EXISTS (SELECT 1 FROM xebus.trambuyt s
                  WHERE s.trangthai <> 'Ngừng hoạt động'
                    AND ST_DWithin(ST_Transform(s.geom, 32648), u.gu, p_cach_toithieu))
       OR xebus.fn_tren_mat_nuoc(ST_Transform(u.gu, 4326));
    CREATE INDEX ON uv_diem USING gist (gu);
    ANALYZE uv_diem;

    -- 4. Chọn tham lam theo phủ tối đa
    LOOP
        EXIT WHEN v_stt >= p_toida;

        SELECT d.id, d.madoan, d.tenduong, d.nguon, d.gu,
               coalesce(dm.dan, 0) AS dan,
               coalesce(ct.n, 0) AS so_ct, coalesce(ct.diem, 0) AS diem_ct,
               round((3 * coalesce(dm.dan, 0) / 1000.0 + coalesce(ct.diem, 0))::numeric, 2) AS tong
        INTO r
        FROM uv_diem d
        LEFT JOIN LATERAL (
            SELECT sum(t.danso * ST_Area(ST_Intersection(t.gu, ST_Buffer(d.gu, p_bankinh, 8))) / NULLIF(t.dientich_m2, 0)) AS dan
            FROM uv_thieu t WHERE ST_DWithin(t.gu, d.gu, p_bankinh)) dm ON true
        LEFT JOIN LATERAL (
            SELECT count(*)::int AS n, sum(c.diem) AS diem
            FROM uv_ct c WHERE ST_DWithin(c.gu, d.gu, p_bankinh)) ct ON true
        ORDER BY 9 DESC, d.id
        LIMIT 1;

        EXIT WHEN NOT FOUND OR r.tong <= 0.05;

        v_stt := v_stt + 1;
        v_vong := ST_Buffer(r.gu, p_bankinh, 16);
        g4326 := ST_Transform(r.gu, 4326);

        stt := v_stt;
        kinhdo := round(ST_X(g4326)::numeric, 7);
        vido := round(ST_Y(g4326)::numeric, 7);
        madoan := r.madoan;
        tenduong := r.tenduong;
        nguon := r.nguon;
        SELECT x.maphuong, x.tenphuong INTO maphuong, tenphuong
        FROM xebus.phuongxa x WHERE ST_Intersects(x.geom, g4326) LIMIT 1;
        dan_moi := round(r.dan)::int;
        so_kdc := (SELECT count(*)::int FROM xebus.khudancu k WHERE ST_DWithin(ST_Transform(k.geom, 32648), r.gu, p_bankinh));
        so_ct_moi := r.so_ct;
        diem_ct := round(r.diem_ct, 2);
        tong_diem := r.tong;
        kc_tram_gan_nhat := (SELECT round(ST_Distance(ST_Transform(s.geom, 32648), r.gu))::int
                             FROM xebus.trambuyt s WHERE s.trangthai <> 'Ngừng hoạt động'
                             ORDER BY s.geom <-> g4326 LIMIT 1);
        geom := g4326;
        RETURN NEXT;

        -- trừ phần đã phủ, loại ứng viên quá gần ứng viên vừa chọn
        UPDATE uv_thieu t SET gu = ST_CollectionExtract(ST_Difference(t.gu, v_vong), 3)
        WHERE ST_Intersects(t.gu, v_vong);
        DELETE FROM uv_thieu t WHERE ST_IsEmpty(t.gu);
        DELETE FROM uv_ct c WHERE ST_DWithin(c.gu, r.gu, p_bankinh);
        DELETE FROM uv_diem d WHERE ST_DWithin(d.gu, r.gu, p_cach_toithieu);
    END LOOP;
END $$;

COMMENT ON FUNCTION xebus.fn_ungvien IS
'Đề xuất ứng viên trạm (v2): phủ tối đa tham lam; tong_diem = 3×dan_moi/1000 + Σ trongso×mucdouutien của công trình chưa phục vụ';

-- ---------------------------------------------------------------------
-- Đánh giá một vị trí bất kỳ do người dùng chọn (cùng công thức với fn_ungvien)
-- ---------------------------------------------------------------------
CREATE FUNCTION xebus.fn_danhgia_diem(p_diem geometry, p_bankinh double precision DEFAULT 500)
RETURNS TABLE (maphuong varchar, tenphuong varchar, madoan varchar, tenduong varchar,
               dan_moi int, so_kdc int, so_ct_moi int, diem_ct numeric, tong_diem numeric,
               kc_tram_gan_nhat int, dat_duoc boolean)
LANGUAGE sql STABLE AS $$
    WITH vt AS (
        SELECT ST_Transform(p_diem, 32648) AS du,
               ST_Buffer(ST_Transform(p_diem, 32648), p_bankinh, 16) AS vong
    ), kdc AS (
        SELECT p.* FROM xebus.fn_phucvu_kdc(ST_Transform((SELECT vong FROM vt), 4326), p_bankinh) p
    ), dan AS (
        SELECT coalesce(sum(k.danso * ST_Area(ST_Intersection(k.chua_pv_utm, vt.vong)) / NULLIF(k.dientich_m2, 0)), 0) AS d
        FROM kdc k, vt
        WHERE NOT ST_IsEmpty(k.chua_pv_utm) AND ST_Intersects(k.chua_pv_utm, vt.vong)
    ), ct AS (
        SELECT count(*)::int AS n, coalesce(sum(l.trongso * coalesce(c.mucdouutien, 1)), 0)::numeric AS d
        FROM xebus.congtrinh c JOIN xebus.loaicongtrinh l USING (maloaicongtrinh), vt
        WHERE c.trangthai <> 'Ngừng hoạt động'
          AND ST_DWithin(ST_Transform(c.geom, 32648), vt.du, p_bankinh)
          AND NOT EXISTS (SELECT 1 FROM xebus.trambuyt s
                          WHERE s.trangthai <> 'Ngừng hoạt động'
                            AND ST_DWithin(ST_Transform(s.geom, 32648), ST_Transform(c.geom, 32648), p_bankinh))
    ), duong AS (
        SELECT d.madoan, d.tenduong FROM xebus.doanduong d
        ORDER BY d.geom <-> p_diem LIMIT 1
    )
    SELECT (SELECT x.maphuong FROM xebus.phuongxa x WHERE ST_Intersects(x.geom, p_diem) LIMIT 1),
           (SELECT x.tenphuong FROM xebus.phuongxa x WHERE ST_Intersects(x.geom, p_diem) LIMIT 1),
           (SELECT madoan FROM duong), (SELECT tenduong FROM duong),
           round(dan.d)::int,
           (SELECT count(*)::int FROM kdc),
           ct.n, round(ct.d, 2),
           round((3 * dan.d / 1000 + ct.d)::numeric, 2),
           (SELECT round(ST_Distance(ST_Transform(s.geom, 32648), vt.du))::int
              FROM xebus.trambuyt s, vt WHERE s.trangthai <> 'Ngừng hoạt động'
              ORDER BY s.geom <-> p_diem LIMIT 1),
           NOT xebus.fn_tren_mat_nuoc(p_diem)
    FROM dan, ct, vt;
$$;

-- Loại công trình "Lưu trú" quá dày đặc ở khu phố cổ => giảm trọng số
UPDATE xebus.loaicongtrinh SET trongso = 0.30 WHERE maloaicongtrinh = 'LT' AND trongso <> 0.30;

COMMIT;
