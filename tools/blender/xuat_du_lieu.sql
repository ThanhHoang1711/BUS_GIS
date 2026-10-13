-- Xuất dữ liệu thật (CHỈ SELECT) quanh trung tâm Hà Nội cho tools/blender/dung_canh.py.
-- psql -At -v ON_ERROR_STOP=1 -f tools/blender/xuat_du_lieu.sql -o canh.json
-- Mặt nước được chia tam giác (ST_TriangulatePolygon, giữ lỗ/đảo) để Blender không nối sai polygon lõm.
WITH c AS (SELECT ST_Transform(ST_SetSRID(ST_MakePoint(105.8460,21.0300),4326),32648) g,
                  ST_X(ST_Transform(ST_SetSRID(ST_MakePoint(105.8460,21.0300),4326),32648)) x,
                  ST_Y(ST_Transform(ST_SetSRID(ST_MakePoint(105.8460,21.0300),4326),32648)) y),
vung AS (SELECT ST_Buffer(c.g, 3600) b FROM c)
SELECT json_build_object(
 'nha', (SELECT json_agg(json_build_array(n.gid, coalesce(n.type,''), ST_AsGeoJSON(ST_Translate(ST_SimplifyPreserveTopology(ST_Transform(n.geom,32648),0.8), -c.x, -c.y),1)::json->'coordinates'))
         FROM public.nha n, c WHERE ST_DWithin(ST_Transform(n.geom,32648), c.g, 3500)),
 'nuoc', (SELECT json_agg(ST_AsGeoJSON(ST_Translate(t.geom, -c.x, -c.y),1)::json->'coordinates')
         FROM c, (SELECT (ST_Dump(ST_TriangulatePolygon(ST_MakeValid(ST_SimplifyPreserveTopology(ST_Intersection(ST_Transform(w.geom,32648), v.b),2))))).geom
                  FROM public.thuyhea w, vung v
                  WHERE ST_Intersects(ST_Transform(w.geom,32648), v.b) AND w.fclass IN ('water','riverbank','reservoir')) t),
 'duong', (SELECT json_agg(json_build_array(coalesce(d.loaiduong,''), ST_AsGeoJSON(ST_Translate(ST_Intersection(ST_Transform(d.geom,32648), v.b), -c.x, -c.y),1)::json->'coordinates'))
         FROM xebus.doanduong d, c, vung v WHERE ST_Intersects(ST_Transform(d.geom,32648), v.b)),
 'tuyen', (SELECT json_agg(json_build_array(t.matuyen, ST_AsGeoJSON(ST_Translate(ST_Intersection(ST_Transform(t.geom,32648), v.b), -c.x, -c.y),1)::json->'coordinates'))
         FROM xebus.tuyenbuyt t, c, vung v WHERE ST_Intersects(ST_Transform(t.geom,32648), v.b)),
 'tram', (SELECT json_agg(json_build_array(ST_X(ST_Transform(t.geom,32648))-c.x, ST_Y(ST_Transform(t.geom,32648))-c.y))
         FROM xebus.trambuyt t, c WHERE ST_DWithin(ST_Transform(t.geom,32648), c.g, 3500))
);
