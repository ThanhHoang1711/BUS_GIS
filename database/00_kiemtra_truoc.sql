-- Đếm số dòng các bảng dữ liệu gốc (schema public) – chạy TRƯỚC và SAU mỗi script để đảm bảo không mất dữ liệu.
-- Chỉ đọc.
SELECT 'tramxebus' AS bang, count(*) FROM public.tramxebus
UNION ALL SELECT 'giaothong',  count(*) FROM public.giaothong
UNION ALL SELECT 'hanoi',      count(*) FROM public.hanoi
UNION ALL SELECT 'nha',        count(*) FROM public.nha
UNION ALL SELECT 'sudungdata', count(*) FROM public.sudungdata
UNION ALL SELECT 'thuyhea',    count(*) FROM public.thuyhea
UNION ALL SELECT 'tileset',    count(*) FROM public.tileset
UNION ALL SELECT 'pointcloud_formats', count(*) FROM public.pointcloud_formats
UNION ALL SELECT 'spatial_ref_sys',    count(*) FROM public.spatial_ref_sys
ORDER BY 1;
