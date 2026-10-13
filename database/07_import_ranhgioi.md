# Import ranh giới quận/huyện, phường/xã

Chạy khi đã có shapefile ranh giới hành chính Hà Nội (vd. GADM `gadm41_VNM_2.shp` = quận/huyện, `gadm41_VNM_3.shp` = phường/xã, hoặc dữ liệu của Sở TN&MT).
Mọi thao tác chỉ ghi vào schema `xebus` (bảng tạm `xebus.tmp_*` được xoá sau khi dùng).

## 1. Nạp shapefile vào bảng tạm

```bat
set PGPASSWORD=<mật khẩu>
set PGBIN=C:\Program Files\PostgreSQL\17\bin
rem -s 4326 nếu shapefile là WGS84; nếu là VN-2000 thì dùng -s <srid>:4326 để chuyển đổi (vd. -s 9210:4326)
rem -W UTF-8 nếu thuộc tính tiếng Việt có dấu bị lỗi thì thử -W LATIN1
"%PGBIN%\shp2pgsql.exe" -s 4326 -W UTF-8 -I gadm41_VNM_2.shp xebus.tmp_quan  | "%PGBIN%\psql.exe" -h localhost -U postgres -d CSDLKG -v ON_ERROR_STOP=1
"%PGBIN%\shp2pgsql.exe" -s 4326 -W UTF-8 -I gadm41_VNM_3.shp xebus.tmp_phuong | "%PGBIN%\psql.exe" -h localhost -U postgres -d CSDLKG -v ON_ERROR_STOP=1
```

Xem tên cột thực tế: `\d xebus.tmp_quan` (GADM: `name_1` tỉnh, `name_2` quận, `gid_2` mã; cấp 3 thêm `name_3`, `gid_3`).

## 2. Chép vào bảng chính (sửa tên cột cho khớp shapefile)

```sql
BEGIN;
-- Quận/huyện thuộc Hà Nội
INSERT INTO xebus.quanhuyen (maquan, tenquan, geom)
SELECT 'Q' || lpad(row_number() OVER (ORDER BY name_2)::text, 3, '0'), name_2, ST_Multi(geom)
FROM xebus.tmp_quan
WHERE name_1 IN ('Hà Nội', 'Ha Noi', 'Hanoi')
ON CONFLICT (maquan) DO NOTHING;

-- Phường/xã, gắn mã quận theo tên
INSERT INTO xebus.phuongxa (maphuong, maquan, tenphuong, geom)
SELECT 'P' || lpad(row_number() OVER (ORDER BY p.name_2, p.name_3)::text, 4, '0'), q.maquan, p.name_3, ST_Multi(p.geom)
FROM xebus.tmp_phuong p
JOIN xebus.quanhuyen q ON q.tenquan = p.name_2
WHERE p.name_1 IN ('Hà Nội', 'Ha Noi', 'Hanoi')
ON CONFLICT (maphuong) DO NOTHING;

-- Gán phường cho các bảng của dự án (CHỈ bảng trong xebus)
UPDATE xebus.trambuyt  t SET maphuong = p.maphuong FROM xebus.phuongxa p
 WHERE t.maphuong IS NULL AND ST_Contains(p.geom, t.geom);
UPDATE xebus.khudancu  k SET maphuong = p.maphuong FROM xebus.phuongxa p
 WHERE k.maphuong IS NULL AND ST_Contains(p.geom, ST_PointOnSurface(k.geom));
UPDATE xebus.congtrinh c SET maphuong = p.maphuong FROM xebus.phuongxa p
 WHERE c.maphuong IS NULL AND ST_Contains(p.geom, ST_PointOnSurface(c.geom));
UPDATE xebus.doanduong d SET maphuong = p.maphuong FROM xebus.phuongxa p
 WHERE d.maphuong IS NULL AND ST_Contains(p.geom, ST_LineInterpolatePoint(ST_GeometryN(d.geom, 1), 0.5));

DROP TABLE xebus.tmp_quan;
DROP TABLE xebus.tmp_phuong;
COMMIT;
```

Lưu ý: trigger của `trambuyt` sẽ cập nhật `ngaycapnhat` cho các trạm được gán phường.

## 3. Kiểm tra
```sql
SELECT count(*) FROM xebus.quanhuyen;            -- Hà Nội: 30 quận/huyện
SELECT count(*) FROM xebus.phuongxa;
SELECT count(*) FILTER (WHERE maphuong IS NULL) FROM xebus.trambuyt;
SELECT * FROM xebus.v_thongke_quan ORDER BY so_tram DESC;
```
