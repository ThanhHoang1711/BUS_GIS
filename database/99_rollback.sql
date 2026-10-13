-- =====================================================================
-- 99_rollback.sql – GỠ BỎ TOÀN BỘ những gì dự án XeBus_GIS đã tạo trong CSDLKG.
-- Chỉ xoá schema xebus (bảng, hàm, view, trigger của dự án).
-- Dữ liệu gốc ở schema public (tramxebus, giaothong, hanoi, nha, ...) KHÔNG bị ảnh hưởng.
-- CẢNH BÁO: mọi dữ liệu nhập qua ứng dụng (tuyến, trạm đề xuất, người dùng, lịch sử) sẽ mất.
--           Hãy sao lưu trước:  pg_dump -h localhost -U postgres -d CSDLKG -n xebus -Fc -f xebus.backup
-- =====================================================================
DROP SCHEMA IF EXISTS xebus CASCADE;
