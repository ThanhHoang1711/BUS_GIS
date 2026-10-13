-- =====================================================================
-- 01_schema.sql – Tạo schema xebus và các bảng theo thiết kế (Nhom5_Thietkehethong.docx)
-- AN TOÀN: chỉ tạo đối tượng MỚI trong schema xebus, không đụng schema public.
-- Idempotent: chạy lại nhiều lần không lỗi, không mất dữ liệu.
-- Chạy: psql -v ON_ERROR_STOP=1 -d CSDLKG -f 01_schema.sql
-- =====================================================================
BEGIN;

CREATE SCHEMA IF NOT EXISTS xebus;
SET LOCAL search_path = xebus, public;

-- ---------------------------------------------------------------------
-- Danh mục hành chính
-- ---------------------------------------------------------------------
-- B03. QUANHUYEN
CREATE TABLE IF NOT EXISTS xebus.quanhuyen (
    maquan      varchar(10)  PRIMARY KEY,
    tenquan     varchar(100) NOT NULL,
    geom        geometry(MultiPolygon, 4326)
);
COMMENT ON TABLE xebus.quanhuyen IS 'B03. Quận/huyện';

-- B04. PHUONGXA
CREATE TABLE IF NOT EXISTS xebus.phuongxa (
    maphuong    varchar(10)  PRIMARY KEY,
    maquan      varchar(10)  REFERENCES xebus.quanhuyen(maquan) ON UPDATE CASCADE,
    tenphuong   varchar(100) NOT NULL,
    geom        geometry(MultiPolygon, 4326)
);
COMMENT ON TABLE xebus.phuongxa IS 'B04. Phường/xã';

-- ---------------------------------------------------------------------
-- Tuyến, đoạn đường
-- ---------------------------------------------------------------------
-- B01. TUYENBUYT (+ thuộc tính kiểm soát idntao, tenntao)
CREATE TABLE IF NOT EXISTS xebus.tuyenbuyt (
    matuyen     varchar(10)  PRIMARY KEY,
    tentuyen    varchar(100) NOT NULL,
    mota        varchar(500),
    trangthai   varchar(20)  NOT NULL DEFAULT 'Hoạt động',
    ngaytao     timestamp    NOT NULL DEFAULT now(),
    ngaycapnhat timestamp,
    idntao      integer,
    tenntao     varchar(100),
    geom        geometry(MultiLineString, 4326)
);
COMMENT ON TABLE xebus.tuyenbuyt IS 'B01. Tuyến xe buýt';

-- B02. DOANDUONG
CREATE TABLE IF NOT EXISTS xebus.doanduong (
    madoan      varchar(10)  PRIMARY KEY,
    matuyen     varchar(10)  REFERENCES xebus.tuyenbuyt(matuyen) ON UPDATE CASCADE ON DELETE SET NULL,
    maphuong    varchar(10)  REFERENCES xebus.phuongxa(maphuong) ON UPDATE CASCADE ON DELETE SET NULL,
    tenduong    varchar(100),
    loaiduong   varchar(50),
    thutu       smallint,
    chieudaikm  numeric(10,3),
    tgdichuyen  numeric(8,2),
    trangthai   varchar(20)  NOT NULL DEFAULT 'Hoạt động',
    nguon_gid   integer,     -- bổ sung: gid trong public.giaothong
    geom        geometry(MultiLineString, 4326)
);
COMMENT ON TABLE xebus.doanduong IS 'B02. Đoạn đường';

-- ---------------------------------------------------------------------
-- Trạm, phương tiện
-- ---------------------------------------------------------------------
-- B05. LOAITRAM
CREATE TABLE IF NOT EXISTS xebus.loaitram (
    maloaitram  varchar(10)  PRIMARY KEY,
    tenloaitram varchar(100) NOT NULL,
    mota        varchar(500)
);
COMMENT ON TABLE xebus.loaitram IS 'B05. Loại trạm';

-- B07. PHUONGTIEN
CREATE TABLE IF NOT EXISTS xebus.phuongtien (
    maphuongtien   varchar(10) PRIMARY KEY,
    bienso         varchar(15) UNIQUE,
    loaiphuongtien varchar(50),
    succhua        smallint CHECK (succhua IS NULL OR succhua > 0),
    trangthai      varchar(20) NOT NULL DEFAULT 'Hoạt động'
);
COMMENT ON TABLE xebus.phuongtien IS 'B07. Phương tiện';

-- B06. TRAMBUYT (+ idntao, tenntao)
CREATE TABLE IF NOT EXISTS xebus.trambuyt (
    matram       varchar(10)  PRIMARY KEY,
    matuyen      varchar(10)  REFERENCES xebus.tuyenbuyt(matuyen)       ON UPDATE CASCADE ON DELETE SET NULL,
    maloaitram   varchar(10)  REFERENCES xebus.loaitram(maloaitram)     ON UPDATE CASCADE,
    madoan       varchar(10)  REFERENCES xebus.doanduong(madoan)        ON UPDATE CASCADE ON DELETE SET NULL,
    maphuongtien varchar(10)  REFERENCES xebus.phuongtien(maphuongtien) ON UPDATE CASCADE ON DELETE SET NULL,
    tentram      varchar(150) NOT NULL,
    vido         numeric(10,7) NOT NULL CHECK (vido  BETWEEN -90  AND 90),
    kinhdo       numeric(10,7) NOT NULL CHECK (kinhdo BETWEEN -180 AND 180),
    trangthai    varchar(20)  NOT NULL DEFAULT 'Hoạt động',
    ngaytao      timestamp    NOT NULL DEFAULT now(),
    ngaycapnhat  timestamp,
    idntao       integer,
    tenntao      varchar(100),
    maphuong     varchar(10)  REFERENCES xebus.phuongxa(maphuong) ON UPDATE CASCADE ON DELETE SET NULL, -- bổ sung: phục vụ tra cứu/thống kê theo khu vực
    osm_id       varchar(12), -- bổ sung: truy vết public.tramxebus
    geom         geometry(Point, 4326)
);
COMMENT ON TABLE xebus.trambuyt IS 'B06. Trạm xe buýt';

-- Bổ sung: TUYEN_TRAM – một trạm thuộc nhiều tuyến, lưu thứ tự trạm trên tuyến
CREATE TABLE IF NOT EXISTS xebus.tuyen_tram (
    matuyen varchar(10) NOT NULL REFERENCES xebus.tuyenbuyt(matuyen) ON UPDATE CASCADE ON DELETE CASCADE,
    matram  varchar(10) NOT NULL REFERENCES xebus.trambuyt(matram)   ON UPDATE CASCADE ON DELETE CASCADE,
    thutu   smallint    NOT NULL CHECK (thutu > 0),
    PRIMARY KEY (matuyen, matram),
    UNIQUE (matuyen, thutu) DEFERRABLE INITIALLY DEFERRED
);
COMMENT ON TABLE xebus.tuyen_tram IS 'Bổ sung: trạm thuộc tuyến theo thứ tự (Gán trạm vào tuyến)';

-- ---------------------------------------------------------------------
-- Dân cư, công trình
-- ---------------------------------------------------------------------
-- B08. KHUDANCU
CREATE TABLE IF NOT EXISTS xebus.khudancu (
    makhu       varchar(10)  PRIMARY KEY,
    maphuong    varchar(10)  REFERENCES xebus.phuongxa(maphuong) ON UPDATE CASCADE ON DELETE SET NULL,
    tenkhu      varchar(150),
    vido        numeric(10,7),
    kinhdo      numeric(10,7),
    danso       integer      CHECK (danso IS NULL OR danso >= 0),
    matdodancu  numeric(10,2),
    trangthai   varchar(20)  NOT NULL DEFAULT 'Hoạt động',
    nguon_gid   integer,     -- bổ sung: gid trong public.sudungdata
    geom        geometry(MultiPolygon, 4326)
);
COMMENT ON TABLE xebus.khudancu IS 'B08. Khu dân cư';

-- B10. LOAICONGTRINH
CREATE TABLE IF NOT EXISTS xebus.loaicongtrinh (
    maloaicongtrinh  varchar(10)  PRIMARY KEY,
    tenloaicongtrinh varchar(100) NOT NULL,
    mota             varchar(500),
    trongso          numeric(4,2) NOT NULL DEFAULT 1
);
COMMENT ON TABLE xebus.loaicongtrinh IS 'B10. Loại công trình';

-- B09. CONGTRINH
CREATE TABLE IF NOT EXISTS xebus.congtrinh (
    macongtrinh     varchar(10)  PRIMARY KEY,
    maphuong        varchar(10)  REFERENCES xebus.phuongxa(maphuong) ON UPDATE CASCADE ON DELETE SET NULL,
    maloaicongtrinh varchar(10)  REFERENCES xebus.loaicongtrinh(maloaicongtrinh) ON UPDATE CASCADE,
    tencongtrinh    varchar(150),
    diachi          varchar(200),
    vido            numeric(10,7),
    kinhdo          numeric(10,7),
    quymo           integer,
    mucdouutien     smallint,
    trangthai       varchar(20)  NOT NULL DEFAULT 'Hoạt động',
    nguon_gid       integer,     -- bổ sung: gid trong public.sudungdata
    geom            geometry(MultiPolygon, 4326)
);
COMMENT ON TABLE xebus.congtrinh IS 'B09. Công trình';

-- B06.1. CTPVKHUDANCU
CREATE TABLE IF NOT EXISTS xebus.ctpvkhudancu (
    matram      varchar(10) NOT NULL REFERENCES xebus.trambuyt(matram) ON UPDATE CASCADE ON DELETE CASCADE,
    makhu       varchar(10) NOT NULL REFERENCES xebus.khudancu(makhu)  ON UPDATE CASCADE ON DELETE CASCADE,
    khoangcachm numeric(10,2),
    PRIMARY KEY (matram, makhu)
);
COMMENT ON TABLE xebus.ctpvkhudancu IS 'B06.1. Chi tiết phục vụ khu dân cư';

-- B06.2. CTPVCONGTRINH
CREATE TABLE IF NOT EXISTS xebus.ctpvcongtrinh (
    matram      varchar(10) NOT NULL REFERENCES xebus.trambuyt(matram)       ON UPDATE CASCADE ON DELETE CASCADE,
    macongtrinh varchar(10) NOT NULL REFERENCES xebus.congtrinh(macongtrinh) ON UPDATE CASCADE ON DELETE CASCADE,
    khoangcachm numeric(10,2),
    PRIMARY KEY (matram, macongtrinh)
);
COMMENT ON TABLE xebus.ctpvcongtrinh IS 'B06.2. Chi tiết phục vụ công trình';

-- ---------------------------------------------------------------------
-- Bảo mật (mục 3.2.1)
-- ---------------------------------------------------------------------
-- B11. N_NGDUNG
CREATE TABLE IF NOT EXISTS xebus.n_ngdung (
    idnhom   serial       PRIMARY KEY,
    tennhom  varchar(50)  NOT NULL UNIQUE,
    mota     varchar(255)
);
COMMENT ON TABLE xebus.n_ngdung IS 'B11. Nhóm người dùng';

-- B12. QUYEN
CREATE TABLE IF NOT EXISTS xebus.quyen (
    idquyen     serial       PRIMARY KEY,
    tenquyen    varchar(100) NOT NULL,
    machucnang  varchar(30)  NOT NULL,
    loaithaotac varchar(10)  NOT NULL CHECK (loaithaotac IN ('XEM','THEM','SUA','XOA')),
    mota        varchar(255),
    UNIQUE (machucnang, loaithaotac)
);
COMMENT ON TABLE xebus.quyen IS 'B12. Quyền theo chức năng × thao tác';

-- B11.1. QUYEN_NND
CREATE TABLE IF NOT EXISTS xebus.quyen_nnd (
    idnhom  integer NOT NULL REFERENCES xebus.n_ngdung(idnhom) ON DELETE CASCADE,
    idquyen integer NOT NULL REFERENCES xebus.quyen(idquyen)   ON DELETE CASCADE,
    PRIMARY KEY (idnhom, idquyen)
);
COMMENT ON TABLE xebus.quyen_nnd IS 'B11.1. Gán quyền cho nhóm người dùng';

-- B13. NGUOIDUNG
CREATE TABLE IF NOT EXISTS xebus.nguoidung (
    idndung   serial       PRIMARY KEY,
    tenndung  varchar(100) NOT NULL,
    chucvu    varchar(100),
    donvi     varchar(100),
    tendnhap  varchar(50)  NOT NULL UNIQUE,
    matkhau   varchar(255) NOT NULL,     -- PBKDF2: iter:saltBase64:hashBase64
    idnhom    integer      NOT NULL REFERENCES xebus.n_ngdung(idnhom),
    trangthai varchar(20)  NOT NULL DEFAULT 'Hoạt động'
);
COMMENT ON TABLE xebus.nguoidung IS 'B13. Người dùng';

-- B14. LS_DNHAP
CREATE TABLE IF NOT EXISTS xebus.ls_dnhap (
    idlsdn      bigserial    PRIMARY KEY,
    idndung     integer      REFERENCES xebus.nguoidung(idndung) ON DELETE SET NULL,
    tendnhap    varchar(50), -- bổ sung: lưu cả tên đăng nhập khi đăng nhập thất bại
    tgdangnhap  timestamp    NOT NULL DEFAULT now(),
    tgdangxuat  timestamp,
    diachiip    varchar(45),
    tenthietbi  varchar(100),
    ketqua      varchar(20)  NOT NULL
);
COMMENT ON TABLE xebus.ls_dnhap IS 'B14. Lịch sử đăng nhập';

-- B15. SAOLUU
CREATE TABLE IF NOT EXISTS xebus.saoluu (
    idsaoluu   serial       PRIMARY KEY,
    idndung    integer      REFERENCES xebus.nguoidung(idndung) ON DELETE SET NULL,
    thoigian   timestamp    NOT NULL DEFAULT now(),
    filepath   varchar(500) NOT NULL,
    kichthuoc  bigint,
    loaisaoluu varchar(20)  NOT NULL DEFAULT 'Thủ công'
);
COMMENT ON TABLE xebus.saoluu IS 'B15. Sao lưu';

-- ---------------------------------------------------------------------
-- Index
-- ---------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS ix_quanhuyen_geom  ON xebus.quanhuyen USING gist (geom);
CREATE INDEX IF NOT EXISTS ix_phuongxa_geom   ON xebus.phuongxa  USING gist (geom);
CREATE INDEX IF NOT EXISTS ix_tuyenbuyt_geom  ON xebus.tuyenbuyt USING gist (geom);
CREATE INDEX IF NOT EXISTS ix_doanduong_geom  ON xebus.doanduong USING gist (geom);
CREATE INDEX IF NOT EXISTS ix_trambuyt_geom   ON xebus.trambuyt  USING gist (geom);
CREATE INDEX IF NOT EXISTS ix_khudancu_geom   ON xebus.khudancu  USING gist (geom);
CREATE INDEX IF NOT EXISTS ix_congtrinh_geom  ON xebus.congtrinh USING gist (geom);
-- index geography cho ST_DWithin theo mét
CREATE INDEX IF NOT EXISTS ix_trambuyt_geog   ON xebus.trambuyt  USING gist ((geom::geography));
CREATE INDEX IF NOT EXISTS ix_khudancu_geog   ON xebus.khudancu  USING gist ((geom::geography));
CREATE INDEX IF NOT EXISTS ix_congtrinh_geog  ON xebus.congtrinh USING gist ((geom::geography));

CREATE INDEX IF NOT EXISTS ix_trambuyt_matuyen   ON xebus.trambuyt (matuyen);
CREATE INDEX IF NOT EXISTS ix_trambuyt_maloai    ON xebus.trambuyt (maloaitram);
CREATE INDEX IF NOT EXISTS ix_trambuyt_trangthai ON xebus.trambuyt (trangthai);
CREATE INDEX IF NOT EXISTS ix_trambuyt_maphuong  ON xebus.trambuyt (maphuong);
CREATE INDEX IF NOT EXISTS ix_trambuyt_ten       ON xebus.trambuyt (lower(tentram));
CREATE INDEX IF NOT EXISTS ix_tuyen_tram_matram  ON xebus.tuyen_tram (matram);
CREATE INDEX IF NOT EXISTS ix_doanduong_matuyen  ON xebus.doanduong (matuyen);
CREATE INDEX IF NOT EXISTS ix_ls_dnhap_ndung     ON xebus.ls_dnhap (idndung, tgdangnhap DESC);

-- ---------------------------------------------------------------------
-- Trigger: tự đồng bộ geom <-> vido/kinhdo, cập nhật ngaycapnhat
-- ---------------------------------------------------------------------
CREATE OR REPLACE FUNCTION xebus.trg_diem_vido_kinhdo() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
    IF TG_OP = 'INSERT' AND NEW.geom IS NULL AND NEW.vido IS NOT NULL AND NEW.kinhdo IS NOT NULL THEN
        NEW.geom := ST_SetSRID(ST_MakePoint(NEW.kinhdo, NEW.vido), 4326);
    ELSIF TG_OP = 'UPDATE' AND (NEW.vido IS DISTINCT FROM OLD.vido OR NEW.kinhdo IS DISTINCT FROM OLD.kinhdo)
          AND NEW.geom IS NOT DISTINCT FROM OLD.geom THEN
        NEW.geom := ST_SetSRID(ST_MakePoint(NEW.kinhdo, NEW.vido), 4326);
    ELSIF NEW.geom IS NOT NULL THEN
        NEW.vido   := round(ST_Y(NEW.geom)::numeric, 7);
        NEW.kinhdo := round(ST_X(NEW.geom)::numeric, 7);
    END IF;
    IF TG_OP = 'UPDATE' THEN
        NEW.ngaycapnhat := now();
    END IF;
    RETURN NEW;
END $$;

DROP TRIGGER IF EXISTS trg_trambuyt_diem ON xebus.trambuyt;
CREATE TRIGGER trg_trambuyt_diem BEFORE INSERT OR UPDATE ON xebus.trambuyt
    FOR EACH ROW EXECUTE FUNCTION xebus.trg_diem_vido_kinhdo();

CREATE OR REPLACE FUNCTION xebus.trg_ngaycapnhat() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
    NEW.ngaycapnhat := now();
    RETURN NEW;
END $$;

DROP TRIGGER IF EXISTS trg_tuyenbuyt_capnhat ON xebus.tuyenbuyt;
CREATE TRIGGER trg_tuyenbuyt_capnhat BEFORE UPDATE ON xebus.tuyenbuyt
    FOR EACH ROW EXECUTE FUNCTION xebus.trg_ngaycapnhat();

-- Sinh mã kế tiếp dạng <tiền tố><số> (vd TR0001) – dùng khi thêm trạm/tuyến mới từ ứng dụng
CREATE OR REPLACE FUNCTION xebus.fn_ma_tiep(p_bang text, p_cot text, p_tiento text, p_do_dai int DEFAULT 4)
RETURNS varchar LANGUAGE plpgsql AS $$
DECLARE v_max int;
BEGIN
    EXECUTE format('SELECT coalesce(max(substring(%I from %s)::int), 0) FROM xebus.%I WHERE %I ~ %L',
                   p_cot, length(p_tiento) + 1, p_bang, p_cot, '^' || p_tiento || '[0-9]+$')
    INTO v_max;
    RETURN p_tiento || lpad((v_max + 1)::text, p_do_dai, '0');
END $$;

COMMIT;
