-- Xuất dữ liệu thật (CHỈ SELECT) quanh Hồ Hoàn Kiếm cho tools/blender/dung_canh.py.
--   psql -At -v ON_ERROR_STOP=1 -f tools/blender/xuat_du_lieu.sql -o canh.json
-- Toạ độ: mét UTM 48N (EPSG:32648) đã trừ gốc = tâm Hồ Hoàn Kiếm (105.8524, 21.0287). Đa giác (nước, cỏ, vỉa hè) được
-- chia tam giác trong PostGIS (ST_TriangulatePolygon – giữ lỗ/đảo) để Blender không nối sai polygon lõm.
--   nha    [gid, type, polygon]          public.nha trong 3,2 km (xa > 2,2 km: không vật trên mái)
--   venho  [tam giác]                    vành đường dạo 15 m quanh Hồ Hoàn Kiếm;  bo_ho [line] = bờ hồ
--   viahe  [tam giác]                    vành vỉa hè 3 m quanh nhà (khoảng trống còn lại đọc như phố nhỏ)
--   duong  [ten, loaiduongb, line]        public.giaothong trong 2,6 km
--   nuoc   [tam giác]                    public.thuyhea (water, riverbank, reservoir)
--   dao    [tam giác]                    đảo trong hồ (vòng trong của mặt nước: Ngọc Sơn, Tháp Rùa…)
--   xanh   [tam giác]                    public.sudungdata (park, grass, forest…)
--   tuyen  [matuyen, line]               xebus.tuyenbuyt
--   tram   [matram, x, y]                xebus.trambuyt trong 2,6 km
--   trambd {matram, ten, x, y, huong}   trạm mở đầu video (TR0102 – 6-8 Tràng Thi) + hướng phố (độ, từ trục x)
WITH c AS (SELECT ST_Transform(ST_SetSRID(ST_MakePoint(105.8524,21.0287),4326),32648) g),
o AS (SELECT ST_X(g) x, ST_Y(g) y, g FROM c),
vung AS (SELECT ST_Buffer(o.g, 3300) b FROM o),
ho AS (SELECT ST_Transform(geom,32648) g FROM public.thuyhea WHERE name = 'Hồ Hoàn Kiếm' LIMIT 1),
nha AS (SELECT n.gid, coalesce(n.type,'') t, ST_SimplifyPreserveTopology(ST_Transform(n.geom,32648),0.3) g
        FROM public.nha n, o WHERE ST_DWithin(ST_Transform(n.geom,32648), o.g, 3200)),
tbd AS (SELECT b.matram, b.tentram, ST_Transform(b.geom,32648) g FROM xebus.trambuyt b WHERE b.matram = 'TR0102'),
dbd AS (SELECT ST_Transform(gg.geom,32648) g FROM public.giaothong gg, tbd ORDER BY gg.geom <-> ST_Transform(tbd.g,4326) LIMIT 1)
SELECT json_build_object(
 'nha', (SELECT json_agg(json_build_array(nha.gid, nha.t, ST_AsGeoJSON(ST_Translate(nha.g, -o.x, -o.y),2)::json->'coordinates')) FROM nha, o),
 'venho', (SELECT json_agg(ST_AsGeoJSON(ST_Translate(t.geom, -o.x, -o.y),2)::json->'coordinates')
           FROM o, (SELECT (ST_Dump(ST_TriangulatePolygon(ST_MakeValid(ST_SimplifyPreserveTopology(
                      ST_Difference(ST_Buffer(ho.g, 15.0), ho.g), 0.8))))).geom FROM ho) t),
 'bo_ho', (SELECT ST_AsGeoJSON(ST_Translate(ST_SimplifyPreserveTopology(ST_ExteriorRing((ST_Dump(ho.g)).geom), 1.0), -o.x, -o.y),2)::json->'coordinates' FROM ho, o LIMIT 1),
 'viahe', (SELECT json_agg(ST_AsGeoJSON(ST_Translate(t.geom, -o.x, -o.y),2)::json->'coordinates')
           FROM o, (SELECT (ST_Dump(ST_TriangulatePolygon(ST_MakeValid(ST_SimplifyPreserveTopology(
                      ST_Difference(ST_Buffer(nha.g, 3.0, 'join=mitre quad_segs=2'), nha.g), 0.5))))).geom FROM nha) t),
 'duong', (SELECT json_agg(json_build_array(coalesce(d.ten,''), coalesce(d.loaiduongb,0),
                  ST_AsGeoJSON(ST_Translate(ST_Intersection(ST_Transform(d.geom,32648), v.b), -o.x, -o.y),2)::json->'coordinates'))
           FROM public.giaothong d, o, vung v WHERE ST_Intersects(ST_Transform(d.geom,32648), v.b)),
 'nuoc', (SELECT json_agg(ST_AsGeoJSON(ST_Translate(t.geom, -o.x, -o.y),2)::json->'coordinates')
          FROM o, (SELECT (ST_Dump(ST_TriangulatePolygon(ST_MakeValid(ST_SimplifyPreserveTopology(ST_Intersection(ST_Transform(w.geom,32648), v.b),1))))).geom
                   FROM public.thuyhea w, vung v
                   WHERE ST_Intersects(ST_Transform(w.geom,32648), v.b) AND w.fclass IN ('water','riverbank','reservoir')) t),
 'dao', (SELECT json_agg(ST_AsGeoJSON(ST_Translate(t.geom, -o.x, -o.y),2)::json->'coordinates')
         FROM o, (SELECT (ST_Dump(ST_TriangulatePolygon(ST_MakePolygon(ST_InteriorRingN(p.g, k))))).geom
                  FROM (SELECT (ST_Dump(ST_Transform(w.geom,32648))).geom g FROM public.thuyhea w, vung v
                        WHERE ST_Intersects(ST_Transform(w.geom,32648), v.b) AND w.fclass IN ('water','reservoir')) p,
                       generate_series(1, ST_NumInteriorRings(p.g)) k) t),
 'xanh', (SELECT json_agg(ST_AsGeoJSON(ST_Translate(t.geom, -o.x, -o.y),2)::json->'coordinates')
          FROM o, (SELECT (ST_Dump(ST_TriangulatePolygon(ST_MakeValid(ST_SimplifyPreserveTopology(ST_Intersection(ST_Transform(u.geom,32648), v.b),1))))).geom
                   FROM public.sudungdata u, vung v
                   WHERE ST_Intersects(ST_Transform(u.geom,32648), v.b)
                     AND u.fclass IN ('park','grass','forest','recreation_ground','meadow','scrub','orchard','cemetery')) t),
 'tuyen', (SELECT json_agg(json_build_array(t.matuyen, ST_AsGeoJSON(ST_Translate(ST_Intersection(ST_Transform(t.geom,32648), v.b), -o.x, -o.y),2)::json->'coordinates'))
           FROM xebus.tuyenbuyt t, o, vung v WHERE ST_Intersects(ST_Transform(t.geom,32648), v.b)),
 'tram', (SELECT json_agg(json_build_array(t.matram, round((ST_X(ST_Transform(t.geom,32648))-o.x)::numeric,2), round((ST_Y(ST_Transform(t.geom,32648))-o.y)::numeric,2)))
          FROM xebus.trambuyt t, o WHERE ST_DWithin(ST_Transform(t.geom,32648), o.g, 2600)),
 'trambd', (SELECT json_build_object('matram', tbd.matram, 'ten', tbd.tentram,
                   'x', round((ST_X(tbd.g)-o.x)::numeric,2), 'y', round((ST_Y(tbd.g)-o.y)::numeric,2),
                   'huong', round(degrees(ST_Azimuth(ST_StartPoint(ST_LineMerge(dbd.g)), ST_EndPoint(ST_LineMerge(dbd.g))))::numeric,1),
                   'duong', ST_AsGeoJSON(ST_Translate(dbd.g, -o.x, -o.y),2)::json->'coordinates')
            FROM tbd, dbd, o)
);
