-- =====================================================================
-- 13_chon_vitri.sql – Công cụ "Chọn vị trí đặt trạm" (đánh giá đa tiêu chí, bổ sung Quy trình 3)
-- Người dùng chọn một khu vực (vẽ hoặc phường/xã); hệ thống rải điểm ứng viên dọc các đoạn đường trong khu vực
-- và chấm 5 tiêu chí không gian, mỗi tiêu chí chuẩn hoá về 0..1:
--   c_dan  Nhu cầu dân cư   : dân KDC CHƯA được trạm nào phục vụ trong bán kính r (người ở gần ≤ r/2 tính đủ,
--                             r/2..r tính một nửa), chia cho giá trị lớn nhất trong khu vực.
--   c_ct   Công trình       : Σ trọng số loại × mức ưu tiên × (1 − 0,5·d/r); công trình đã có trạm phục vụ ×0,3;
--                             chia cho giá trị lớn nhất trong khu vực.
--   c_tram Trạm hiện có     : khoảng cách tới trạm gần nhất d: < 150 m → 0; 150–400 m tăng dần; ≥ 400 m → 1.
--   c_gt   Giao thông       : 0,6 × cấp đường (Quốc lộ 1 · Tỉnh lộ 0,85 · Đường đô thị/huyện 0,7) + 0,4 nếu có
--                             tuyến buýt đi qua đoạn đường (≤ 30 m) ⇒ ghép ngay vào tuyến được.
--   c_tc   Tiếp cận an toàn : khoảng cách tới nút giao (đỉnh mạng đường có ≥ 3 cạnh): < 25 m → 0; 25–75 m tăng dần;
--                             ≥ 75 m → 1 (trạm không nên đặt sát nút giao).
-- Điểm tổng = Σ trọng số × tiêu chí do ứng dụng tính (người dùng kéo thanh trọng số – không phải gọi lại CSDL).
-- Loại cứng: hầm, cầu vượt, đường dẫn cầu (fn_duong_dat_duoc_tram), điểm trên mặt nước (fn_tren_mat_nuoc).
-- Khu dân cư thiếu trạm không có đường trong mạng gần đó: thêm 1 điểm trong khu (nguon = 'Khu dân cư').
-- Dữ liệu không có lưu lượng giao thông / số khách ⇒ dùng cấp đường thay thế; dân số KDC là ước tính.
--
-- Quyền: DX_CHONVITRI – chỉ cấp cho nhóm Quản trị viên (yêu cầu người dùng 2026-10-13).
-- Chỉ tạo đối tượng trong schema xebus; public.thuyhea chỉ được SELECT (trừ mặt nước). Chạy lại được nhiều lần.
-- =====================================================================
BEGIN;

CREATE OR REPLACE FUNCTION xebus.fn_dancu_chua_pv(p_vung geometry, p_bankinh double precision DEFAULT 500)
RETURNS TABLE (makhu varchar, md double precision, gu geometry)
LANGUAGE sql STABLE AS $$
    WITH v AS MATERIALIZED (
        SELECT ST_Buffer(ST_Transform(p_vung, 32648), p_bankinh) AS mr,
               ST_GeomFromText('POLYGON EMPTY', 32648) AS rong
    ),
    -- vùng đã được trạm hiện có phục vụ (hợp vòng bán kính r)
    pv AS MATERIALIZED (
        SELECT coalesce(ST_Union(ST_Buffer(ST_Transform(s.geom, 32648), p_bankinh, 8)), (SELECT rong FROM v)) AS g
        FROM xebus.trambuyt s, v
        WHERE s.trangthai <> 'Ngừng hoạt động' AND ST_DWithin(ST_Transform(s.geom, 32648), v.mr, p_bankinh)
    ),
    nuoc AS MATERIALIZED (
        SELECT coalesce(ST_Union(ST_Transform(w.geom, 32648)), (SELECT rong FROM v)) AS g
        FROM public.thuyhea w, v
        WHERE w.fclass IN ('water', 'riverbank', 'reservoir') AND ST_Intersects(w.geom, ST_Transform(v.mr, 4326))
    ),
    kdc_all AS MATERIALIZED (
        SELECT k.makhu, k.maphuong, coalesce(k.danso, 0) AS danso, ST_Transform(k.geom, 32648) AS gu
        FROM xebus.khudancu k, v
        WHERE k.trangthai <> 'Ngừng hoạt động' AND ST_Intersects(ST_Transform(k.geom, 32648), v.mr)
    ),
    -- Phân bổ dân số (dasymetric): trong KDC dùng dân số ước tính của KDC; phần còn lại của dân số THẬT của phường
    -- rải đều trên diện tích phường ngoài KDC và mặt nước ⇒ tổng dân mỗi phường khớp số liệu thật.
    phuong AS MATERIALIZED (
        SELECT p.maphuong, ST_Intersection(ST_Transform(p.geom, 32648), v.mr) AS g,
               greatest(0, coalesce(p.danso, 0) - coalesce(k.dan, 0))
                 / NULLIF(ST_Area(ST_Transform(p.geom, 32648)) - coalesce(k.dt, 0) - coalesce(w.dt, 0), 0) AS md
        FROM xebus.phuongxa p CROSS JOIN v
        LEFT JOIN LATERAL (SELECT sum(kd.danso) AS dan, sum(ST_Area(ST_Transform(kd.geom, 32648))) AS dt
                           FROM xebus.khudancu kd WHERE kd.maphuong = p.maphuong AND kd.trangthai <> 'Ngừng hoạt động') k ON true
        LEFT JOIN LATERAL (SELECT sum(ST_Area(ST_Transform(ST_Intersection(x.geom, p.geom), 32648))) AS dt
                           FROM public.thuyhea x
                           WHERE x.fclass IN ('water', 'riverbank', 'reservoir') AND ST_Intersects(x.geom, p.geom)) w ON true
        WHERE ST_Intersects(ST_Transform(p.geom, 32648), v.mr)
    ),
    -- các mảnh dân cư CHƯA được phục vụ, mỗi mảnh có mật độ md (người/m²); chia nhỏ để tính giao nhanh
    dancu AS MATERIALIZED (
        SELECT x.makhu, x.md, ST_Subdivide(x.g, 64) AS gu
        FROM (
            SELECT k.makhu, k.danso / NULLIF(ST_Area(k.gu), 0) AS md,
                   ST_CollectionExtract(ST_Difference(k.gu, pv.g), 3) AS g
            FROM kdc_all k, pv WHERE k.danso > 0
            UNION ALL
            SELECT NULL, p.md,
                   ST_CollectionExtract(ST_Difference(ST_Difference(ST_Difference(p.g,
                       coalesce((SELECT ST_Union(gu) FROM kdc_all), (SELECT rong FROM v))), nuoc.g), pv.g), 3)
            FROM phuong p, pv, nuoc WHERE p.md > 0
        ) x
        WHERE x.md > 0 AND NOT ST_IsEmpty(x.g)
    )
    SELECT d.makhu, d.md, d.gu FROM dancu d;
$$;

COMMENT ON FUNCTION xebus.fn_dancu_chua_pv IS
'Mảnh dân cư CHƯA được trạm phục vụ quanh vùng (mở rộng r), mật độ md người/m² (UTM 32648): KDC theo dân ước tính, phần còn lại của phường rải đều dân số thật còn lại (trừ mặt nước)';

CREATE OR REPLACE FUNCTION xebus.fn_chon_vitri(
    p_vung geometry,
    p_bankinh double precision DEFAULT 500,
    p_buoc double precision DEFAULT 50)
RETURNS TABLE (id int, madoan varchar, tenduong varchar, loaiduong varchar, nguon text,
               kinhdo numeric, vido numeric, maphuong varchar, tenphuong varchar,
               dan int, dan_gan int, so_ct int, so_ct_moi int, diem_ct numeric, ct_mota text,
               kc_tram int, tram_gan varchar, tuyen text, kc_nut int,
               c_dan numeric, c_ct numeric, c_tram numeric, c_gt numeric, c_tc numeric,
               wkt_diem text, wkt_doan text)
LANGUAGE sql STABLE AS $$
    WITH v AS MATERIALIZED (
        SELECT ST_Transform(p_vung, 32648) AS gu
    ),
    dancu AS MATERIALIZED (
        SELECT * FROM xebus.fn_dancu_chua_pv(p_vung, p_bankinh)
    ),
    kdc AS MATERIALIZED (   -- phần KDC chưa được phục vụ (dùng cho ứng viên trong khu dân cư)
        SELECT d.makhu, ST_Union(d.gu) AS gu FROM dancu d WHERE d.makhu IS NOT NULL GROUP BY d.makhu
    ),
    -- công trình quanh khu vực (đánh dấu đã có trạm phục vụ hay chưa)
    ct AS MATERIALIZED (
        SELECT c.macongtrinh, l.tenloaicongtrinh AS tenloai, ST_Transform(c.geom, 32648) AS gu,
               (l.trongso * coalesce(c.mucdouutien, 1))::numeric AS diem,
               NOT EXISTS (SELECT 1 FROM xebus.trambuyt s
                           WHERE s.trangthai <> 'Ngừng hoạt động'
                             AND ST_DWithin(ST_Transform(s.geom, 32648), ST_Transform(c.geom, 32648), p_bankinh)) AS moi
        FROM xebus.congtrinh c JOIN xebus.loaicongtrinh l USING (maloaicongtrinh), v
        WHERE c.trangthai <> 'Ngừng hoạt động'
          AND ST_DWithin(ST_Transform(c.geom, 32648), v.gu, p_bankinh)
    ),
    -- nút giao: đỉnh mạng đường nối ≥ 3 cạnh
    nut AS MATERIALIZED (
        SELECT ST_Transform(d.geom, 32648) AS gu
        FROM xebus.mang_dinh d, v
        WHERE ST_DWithin(ST_Transform(d.geom, 32648), v.gu, 200)
          AND (SELECT count(*) FROM xebus.mang_canh c WHERE c.source = d.id OR c.target = d.id) >= 3
    ),
    tuyen AS MATERIALIZED (
        SELECT t.matuyen, ST_Transform(t.geom, 32648) AS gu
        FROM xebus.tuyenbuyt t, v
        WHERE t.geom IS NOT NULL AND t.trangthai <> 'Ngừng hoạt động'
          AND ST_DWithin(ST_Transform(t.geom, 32648), v.gu, 50)
    ),
    -- các đoạn đường (đã cắt theo khu vực) đặt được trạm
    duong AS MATERIALIZED (
        SELECT d.madoan, d.tenduong, d.loaiduong, x.geom AS g
        FROM xebus.doanduong d, v,
             LATERAL ST_Dump(ST_CollectionExtract(ST_Intersection(ST_Transform(d.geom, 32648), v.gu), 2)) x
        WHERE d.trangthai <> 'Ngừng hoạt động'
          AND xebus.fn_duong_dat_duoc_tram(d.tenduong)
          AND ST_Intersects(ST_Transform(d.geom, 32648), v.gu)
          AND ST_Length(x.geom) >= 10
    ),
    -- ứng viên dọc đường: chia mỗi đoạn thành n khúc ≈ p_buoc, điểm ở giữa khúc
    uv_duong AS (
        SELECT du.madoan, du.tenduong, du.loaiduong, 'Đường'::text AS nguon,
               ST_LineInterpolatePoint(du.g, (i + 0.5) / n.n) AS gu,
               ST_LineSubstring(du.g, i::float8 / n.n, (i + 1)::float8 / n.n) AS doan
        FROM duong du
        CROSS JOIN LATERAL (SELECT greatest(1, round(ST_Length(du.g) / p_buoc))::int AS n) n
        CROSS JOIN LATERAL generate_series(0, n.n - 1) i
    ),
    -- KDC thiếu trạm nằm xa mạng đường: 1 điểm trong phần chưa phục vụ lớn nhất
    uv_kdc AS (
        SELECT NULL::varchar AS madoan, kd.tenkhu AS tenduong, NULL::varchar AS loaiduong, 'Khu dân cư'::text AS nguon,
               ST_PointOnSurface(p.geom) AS gu, NULL::geometry AS doan
        FROM kdc k
        JOIN xebus.khudancu kd ON kd.makhu = k.makhu
        CROSS JOIN v
        CROSS JOIN LATERAL (
            SELECT d.geom FROM ST_Dump(ST_CollectionExtract(ST_Intersection(k.gu, v.gu), 3)) d
            ORDER BY ST_Area(d.geom) DESC LIMIT 1) p
        WHERE ST_Area(p.geom) > 2000
          AND NOT EXISTS (SELECT 1 FROM duong du WHERE ST_DWithin(du.g, k.gu, p_bankinh / 2))
    ),
    uv AS MATERIALIZED (
        SELECT row_number() OVER (ORDER BY a.nguon DESC, a.madoan, ST_X(a.gu), ST_Y(a.gu))::int AS id, a.*,
               ST_Transform(a.gu, 4326) AS g4326
        FROM (SELECT * FROM uv_duong UNION ALL SELECT * FROM uv_kdc) a
        WHERE NOT xebus.fn_tren_mat_nuoc(ST_Transform(a.gu, 4326))
    ),
    tinh AS MATERIALIZED (
        SELECT u.*,
               coalesce(dm.dan, 0) AS dan, coalesce(dm.dan_gan, 0) AS dan_gan,
               coalesce(cc.n, 0) AS so_ct, coalesce(cc.n_moi, 0) AS so_ct_moi, coalesce(cc.diem, 0) AS diem_ct,
               cc.mota AS ct_mota,
               tr.kc AS kc_tram, tr.ten AS tram_gan,
               tu.ds AS tuyen,
               nu.kc AS kc_nut
        FROM uv u
        LEFT JOIN LATERAL (
            SELECT sum(k.md * ST_Area(ST_Intersection(k.gu, ST_Buffer(u.gu, p_bankinh, 8)))) AS dan,
                   sum(k.md * ST_Area(ST_Intersection(k.gu, ST_Buffer(u.gu, p_bankinh / 2, 8))))
                       FILTER (WHERE ST_DWithin(k.gu, u.gu, p_bankinh / 2)) AS dan_gan
            FROM dancu k WHERE ST_DWithin(k.gu, u.gu, p_bankinh)) dm ON true
        LEFT JOIN LATERAL (
            SELECT count(*)::int AS n, count(*) FILTER (WHERE c.moi)::int AS n_moi,
                   sum(c.diem * (1 - 0.5 * ST_Distance(c.gu, u.gu) / p_bankinh) * CASE WHEN c.moi THEN 1 ELSE 0.3 END) AS diem,
                   (SELECT string_agg(x.so || ' ' || x.tenloai, ', ' ORDER BY x.so DESC, x.tenloai)
                      FROM (SELECT c2.tenloai, count(*) AS so FROM ct c2
                            WHERE ST_DWithin(c2.gu, u.gu, p_bankinh) GROUP BY c2.tenloai) x) AS mota
            FROM ct c WHERE ST_DWithin(c.gu, u.gu, p_bankinh)) cc ON true
        LEFT JOIN LATERAL (
            SELECT round(ST_Distance(ST_Transform(s.geom, 32648), u.gu))::int AS kc, s.tentram AS ten
            FROM xebus.trambuyt s
            WHERE s.trangthai <> 'Ngừng hoạt động'
            ORDER BY s.geom <-> u.g4326 LIMIT 1) tr ON true
        LEFT JOIN LATERAL (
            SELECT string_agg(DISTINCT t.matuyen, ', ' ORDER BY t.matuyen) AS ds
            FROM tuyen t WHERE ST_DWithin(t.gu, u.gu, 30)) tu ON true
        LEFT JOIN LATERAL (
            SELECT round(ST_Distance(n.gu, u.gu))::int AS kc
            FROM nut n ORDER BY n.gu <-> u.gu LIMIT 1) nu ON true
    ),
    -- mốc chuẩn hoá: phân vị 90 (giá trị cực lớn không làm mọi điểm khác về gần 0)
    moc AS (
        SELECT percentile_cont(0.9) WITHIN GROUP (ORDER BY t.dan_gan + 0.5 * (t.dan - t.dan_gan))
                   FILTER (WHERE t.dan > 0) AS dan,
               percentile_cont(0.9) WITHIN GROUP (ORDER BY t.diem_ct) FILTER (WHERE t.diem_ct > 0) AS ct
        FROM tinh t
    )
    SELECT t.id, t.madoan, t.tenduong, t.loaiduong, t.nguon,
           round(ST_X(t.g4326)::numeric, 7), round(ST_Y(t.g4326)::numeric, 7),
           px.maphuong, px.tenphuong,
           round(t.dan)::int, round(t.dan_gan)::int, t.so_ct, t.so_ct_moi, round(t.diem_ct::numeric, 2), t.ct_mota,
           t.kc_tram, t.tram_gan, t.tuyen, t.kc_nut,
           -- c_dan
           round(least(1, coalesce((t.dan_gan + 0.5 * (t.dan - t.dan_gan)) / NULLIF(moc.dan, 0), 0))::numeric, 3),
           -- c_ct
           round(least(1, coalesce(t.diem_ct / NULLIF(moc.ct, 0), 0))::numeric, 3),
           -- c_tram
           round(CASE WHEN t.kc_tram IS NULL THEN 1
                      WHEN t.kc_tram < 150 THEN 0
                      ELSE least(1, (t.kc_tram - 150) / 250.0) END::numeric, 3),
           -- c_gt
           round((0.6 * CASE t.loaiduong WHEN 'Quốc lộ' THEN 1 WHEN 'Tỉnh lộ' THEN 0.85
                                         WHEN 'Đường đô thị / huyện' THEN 0.7
                                         ELSE CASE WHEN t.nguon = 'Đường' THEN 0.6 ELSE 0.2 END END
                  + CASE WHEN t.tuyen IS NOT NULL THEN 0.4 ELSE 0 END)::numeric, 3),
           -- c_tc
           round(CASE WHEN t.nguon <> 'Đường' THEN 0.5
                      WHEN t.kc_nut IS NULL THEN 1
                      WHEN t.kc_nut < 25 THEN 0
                      ELSE least(1, (t.kc_nut - 25) / 50.0) END::numeric, 3),
           ST_AsText(ST_Transform(t.gu, 3857)),
           CASE WHEN t.doan IS NULL THEN NULL ELSE ST_AsText(ST_Transform(t.doan, 3857)) END
    FROM tinh t CROSS JOIN moc
    LEFT JOIN LATERAL (SELECT x.maphuong, x.tenphuong FROM xebus.phuongxa x
                       WHERE ST_Intersects(x.geom, t.g4326) LIMIT 1) px ON true
    ORDER BY t.id;
$$;

COMMENT ON FUNCTION xebus.fn_chon_vitri IS
'Chọn vị trí đặt trạm: ứng viên dọc đường trong vùng + 5 tiêu chí chuẩn hoá 0..1 (c_dan, c_ct, c_tram, c_gt, c_tc); điểm tổng có trọng số tính ở ứng dụng';

-- Quyền mới – chỉ nhóm Quản trị viên (idnhom 3)
INSERT INTO xebus.quyen (tenquyen, machucnang, loaithaotac, mota)
VALUES ('Chọn vị trí đặt trạm theo đa tiêu chí', 'DX_CHONVITRI', 'THEM', 'Quy trình 3.6 (bổ sung)')
ON CONFLICT (machucnang, loaithaotac) DO NOTHING;

INSERT INTO xebus.quyen_nnd (idnhom, idquyen)
SELECT 3, idquyen FROM xebus.quyen WHERE machucnang = 'DX_CHONVITRI'
ON CONFLICT DO NOTHING;

COMMIT;
