-- =====================================================================
-- 10_tienich.sql – Hàm tiện ích cho ứng dụng (chỉ trong schema xebus)
-- =====================================================================
BEGIN;

-- Bỏ dấu tiếng Việt + chữ thường (tìm kiếm không phân biệt dấu, không cần extension unaccent)
CREATE OR REPLACE FUNCTION xebus.unaccent_vn(p text)
RETURNS text LANGUAGE sql IMMUTABLE PARALLEL SAFE AS $$
    SELECT translate(lower(p),
        'àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ',
        'aaaaaaaaaaaaaaaaaeeeeeeeeeeeiiiiiooooooooooooooooouuuuuuuuuuuyyyyyd');
$$;

CREATE INDEX IF NOT EXISTS ix_trambuyt_ten_khongdau ON xebus.trambuyt (xebus.unaccent_vn(tentram) text_pattern_ops);

COMMIT;
