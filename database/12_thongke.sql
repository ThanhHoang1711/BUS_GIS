-- =====================================================================
-- 12_thongke.sql – Thống kê (Quy trình 4) theo phường/xã (cấp quận đã bãi bỏ)
-- Chỉ trong schema xebus.
-- =====================================================================
BEGIN;

-- 4.1 Số trạm theo phường/xã (dùng cột maphuong đã gán)
CREATE OR REPLACE VIEW xebus.v_thongke_phuong AS
SELECT p.maphuong, p.tenphuong, NULL::varchar(10) AS maquan, NULL::varchar(100) AS tenquan,
       count(t.matram)                                                     AS so_tram,
       round(p.dientichkm2, 2)                                              AS dientich_km2,
       p.geom
FROM xebus.phuongxa p
LEFT JOIN xebus.trambuyt t ON t.maphuong = p.maphuong AND t.trangthai <> 'Ngừng hoạt động'
GROUP BY p.maphuong, p.tenphuong, p.dientichkm2, p.geom;

-- 4.1 + 4.3 Bảng tổng hợp theo phường/xã với bán kính phục vụ r
DROP FUNCTION IF EXISTS xebus.fn_thongke_phuong(double precision);
CREATE FUNCTION xebus.fn_thongke_phuong(p_bankinh double precision DEFAULT 500)
RETURNS TABLE (maphuong varchar, tenphuong varchar, danso int, dientich_km2 numeric,
               so_tram int, so_tram_dexuat int, tram_tren_km2 numeric, dan_tren_tram int,
               dan_kdc int, dan_kdc_pv int, tyle_pv numeric, so_ct int, so_ct_pv int, so_tuyen int)
LANGUAGE sql STABLE AS $$
    WITH pv AS MATERIALIZED (
        SELECT k.maphuong, sum(f.danso) AS dan, sum(f.danso * f.tyle_phucvu) AS dan_pv
        FROM xebus.fn_phucvu_kdc(NULL, p_bankinh) f
        JOIN xebus.khudancu k ON k.makhu = f.makhu
        GROUP BY k.maphuong
    ), ct AS MATERIALIZED (
        SELECT c.maphuong, count(*) AS n,
               count(*) FILTER (WHERE EXISTS (
                   SELECT 1 FROM xebus.trambuyt s
                   WHERE s.trangthai <> 'Ngừng hoạt động'
                     AND ST_DWithin(ST_Transform(s.geom, 32648), ST_Transform(c.geom, 32648), p_bankinh))) AS n_pv
        FROM xebus.congtrinh c GROUP BY c.maphuong
    ), tr AS MATERIALIZED (
        SELECT t.maphuong,
               count(DISTINCT t.matram) FILTER (WHERE t.trangthai = 'Hoạt động') AS n,
               count(DISTINCT t.matram) FILTER (WHERE t.trangthai = 'Đề xuất') AS n_dx,
               count(DISTINCT x.matuyen) AS n_tuyen
        FROM xebus.trambuyt t LEFT JOIN xebus.tuyen_tram x ON x.matram = t.matram
        GROUP BY t.maphuong
    )
    SELECT p.maphuong, p.tenphuong, p.danso, p.dientichkm2,
           coalesce(tr.n, 0)::int, coalesce(tr.n_dx, 0)::int,
           round(coalesce(tr.n, 0) / NULLIF(p.dientichkm2, 0), 2),
           CASE WHEN coalesce(tr.n, 0) > 0 THEN round(p.danso::numeric / tr.n)::int END,
           round(coalesce(pv.dan, 0))::int, round(coalesce(pv.dan_pv, 0))::int,
           round((pv.dan_pv / NULLIF(pv.dan, 0) * 100)::numeric, 1),
           coalesce(ct.n, 0)::int, coalesce(ct.n_pv, 0)::int,
           coalesce(tr.n_tuyen, 0)::int
    FROM xebus.phuongxa p
    LEFT JOIN pv ON pv.maphuong = p.maphuong
    LEFT JOIN ct ON ct.maphuong = p.maphuong
    LEFT JOIN tr ON tr.maphuong = p.maphuong
    ORDER BY p.tenphuong;
$$;

-- 4.3 Phạm vi phục vụ theo loại công trình
DROP FUNCTION IF EXISTS xebus.fn_thongke_ct_phucvu(double precision);
CREATE FUNCTION xebus.fn_thongke_ct_phucvu(p_bankinh double precision DEFAULT 500)
RETURNS TABLE (maloai varchar, tenloai varchar, so_ct int, so_ct_pv int, tyle_pv numeric)
LANGUAGE sql STABLE AS $$
    SELECT l.maloaicongtrinh, l.tenloaicongtrinh, count(c.*)::int,
           count(c.*) FILTER (WHERE EXISTS (
               SELECT 1 FROM xebus.trambuyt s
               WHERE s.trangthai <> 'Ngừng hoạt động'
                 AND ST_DWithin(ST_Transform(s.geom, 32648), ST_Transform(c.geom, 32648), p_bankinh)))::int,
           round((count(c.*) FILTER (WHERE EXISTS (
               SELECT 1 FROM xebus.trambuyt s
               WHERE s.trangthai <> 'Ngừng hoạt động'
                 AND ST_DWithin(ST_Transform(s.geom, 32648), ST_Transform(c.geom, 32648), p_bankinh)))
                 * 100.0 / NULLIF(count(c.*), 0))::numeric, 1)
    FROM xebus.loaicongtrinh l
    JOIN xebus.congtrinh c ON c.maloaicongtrinh = l.maloaicongtrinh
    GROUP BY l.maloaicongtrinh, l.tenloaicongtrinh
    ORDER BY 3 DESC;
$$;

COMMIT;
