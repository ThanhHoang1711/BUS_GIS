-- =====================================================================
-- 08_giaothong_srid.sql – Khai báo hệ toạ độ EPSG:4326 cho public.giaothong
-- Thực hiện theo yêu cầu của chủ dữ liệu (2026-10-13): mọi lớp trong CSDLKG thống nhất 4326.
-- Toạ độ gốc vốn là kinh/vĩ độ WGS84 nhưng SRID khai báo = 0 => chỉ GÁN SRID, KHÔNG biến đổi toạ độ,
-- không thay đổi số dòng hay thuộc tính. Đây là thao tác DUY NHẤT dự án thực hiện trên schema public.
-- Idempotent: chạy lại không ảnh hưởng (đã là 4326 thì bỏ qua).
-- =====================================================================
BEGIN;

DO $$
BEGIN
    IF (SELECT format_type(atttypid, atttypmod) FROM pg_attribute
        WHERE attrelid = 'public.giaothong'::regclass AND attname = 'geom') <> 'geometry(MultiLineString,4326)' THEN
        ALTER TABLE public.giaothong
            ALTER COLUMN geom TYPE geometry(MultiLineString, 4326)
            USING ST_SetSRID(geom, 4326);
        RAISE NOTICE 'public.giaothong.geom -> geometry(MultiLineString,4326)';
    ELSE
        RAISE NOTICE 'public.giaothong đã là SRID 4326 – bỏ qua';
    END IF;
END $$;

COMMIT;
