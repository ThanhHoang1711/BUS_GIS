# CLAUDE.md – XeBus_GIS

Hệ thống GIS hỗ trợ quản lý, phân tích, thiết lập trạm xe buýt – TP Hà Nội (đề tài Nhóm 5, GVHD: thầy Nguyễn Sách Thành).
Tài liệu gốc: `Nhom5_Thietkehethong.docx` (khảo sát, phân tích, thiết kế CSDL + phân quyền).

## Môi trường làm việc (quan trọng)
- **Người dùng build/run trong Visual Studio 2022** (“visual tím”). Claude ở đây **chỉ viết code**, không chạy app. Người dùng muốn Claude làm liên tục, tự kiểm tra kỹ (MSBuild + chạy SQL trong transaction ROLLBACK + gọi repository qua reflection) rồi mới báo; người dùng test trên VS theo đợt.
- **Smart App Control (Windows 11) đang BẬT** trên máy: exe/dll chưa ký vừa build có thể bị chặn ("An Application Control policy has blocked this file"). Khi tự kiểm tra bằng PowerShell: nạp exe bằng `[Reflection.Assembly]::Load([IO.File]::ReadAllBytes(...))`, còn DLL NuGet (đã ký) nạp bằng `LoadFrom`. Không tự thay đổi cài đặt bảo mật của Windows.
- OS: Windows 11. Shell: PowerShell / Git Bash.

## Stack
| Thành phần | Chi tiết |
|---|---|
| UI | C# WinForms, .NET Framework **4.7.2**, solution `XeBus_GIS.sln`, project `XeBus_GIS/XeBus_GIS.csproj` (namespace `XeBus_GIS`) |
| Bản đồ | **MapWinGIS 4.9 x64** – `C:\dev\MapWinGIS\MapWinGIS.ocx`, TypeLib `{C368D713-CC5F-40ED-9F53-F84FE197B96A}` v4.9 ⇒ **PlatformTarget phải là x64** (AnyCPU/x86 sẽ lỗi “Class not registered”) |
| CSDL | PostgreSQL 17.4 + PostGIS 3.5.3 + pgRouting 3.8, `localhost:5432`, DB **CSDLKG**, user `postgres` |
| Truy cập DB | Npgsql 4.1.x (NuGet, PackageReference) |
| Biểu đồ / báo cáo | `System.Windows.Forms.DataVisualization` (Chart), ClosedXML (xuất Excel) |
| psql/pg_dump | `C:\Program Files\PostgreSQL\17\bin\` |

## QUY TẮC AN TOÀN CSDL – BẮT BUỘC
1. Chỉ làm việc trong DB **CSDLKG**. Mọi bảng/hàm/view mới nằm trong **schema `xebus`**.
2. **Không bao giờ** `ALTER / DROP / UPDATE / DELETE / TRUNCATE / INSERT` vào bảng thuộc schema `public`, `tiger`, `topology`. Chỉ được `SELECT` từ đó (để copy dữ liệu sang `xebus`).
   Ngoại lệ duy nhất (người dùng yêu cầu 2026-10-13): `database/08_giaothong_srid.sql` gán SRID 4326 cho `public.giaothong.geom` (không đổi toạ độ – đã kiểm md5). **Mọi lớp trong CSDLKG dùng EPSG:4326.**
3. Không tạo/sửa role, database, tablespace hay cấu hình cấp cluster. Phân quyền thực hiện ở **mức ứng dụng** (bảng `n_ngdung`, `quyen`, `quyen_nnd`, `nguoidung`).
4. Trước khi chạy script thay đổi CSDL: backup `pg_dump -Fc` vào `database/backup/`, đếm dòng bảng public (`database/00_kiemtra_truoc.sql`), chạy script với `psql -v ON_ERROR_STOP=1` trong transaction, rồi đếm lại – phải y nguyên.
5. Script phải idempotent (`IF NOT EXISTS`, `ON CONFLICT DO NOTHING`). Rollback toàn bộ: `database/99_rollback.sql` (`DROP SCHEMA xebus CASCADE`).
6. Code C# luôn dùng tham số (`@p`) – không nối chuỗi SQL với dữ liệu người dùng.

## Dữ liệu gốc có sẵn (schema public – CHỈ ĐỌC)
| Bảng | Số dòng | Ghi chú |
|---|---|---|
| `tramxebus` | 1258 | POINT 4326; `osm_id, code, fclass (bus_stop/bus_station), name` |
| `giaothong` | 1782 | MULTILINESTRING 4326 (trước 2026-10-13 khai báo SRID 0); `ten, loaiduongb, dorong, maduong…` |
| `hanoi` | 1 | MULTIPOLYGON 4326 ranh giới TP |
| `sudungdata` | 6451 | MULTIPOLYGON 4326 sử dụng đất OSM; `fclass` residential, commercial, industrial, park… |
| `nha` | 53094 | MULTIPOLYGON 4326 building (dùng ước tính dân số) |
| `thuyhea` | 4954 | MULTIPOLYGON 4326 thuỷ hệ |
| `tileset` | 2 | Không liên quan dự án – không đụng |

## Schema `xebus` (thiết kế theo file Word + bổ sung)
Tên định danh PostgreSQL viết **chữ thường, không dấu ngoặc kép** (Word ghi `maTuyen` ⇒ cột `matuyen`).

Nghiệp vụ:
- `tuyenbuyt` (B01) – matuyen PK, tentuyen, mota, trangthai, ngaytao, ngaycapnhat, idntao, tenntao, *geom MultiLineString*
- `doanduong` (B02) – madoan PK, matuyen FK, maphuong FK, tenduong, loaiduong, thutu, chieudaikm, tgdichuyen, trangthai, *geom, nguon_gid*
- `quanhuyen` (B03) – **KHÔNG DÙNG**: cấp quận/huyện đã bãi bỏ (2025). Bảng giữ theo thiết kế Word nhưng rỗng; không xây chức năng theo quận.
- `phuongxa` (B04) – maphuong PK (mã xã 8 số), maquan (NULL), tenphuong, *danso, dientichkm2, ghichu (đơn vị cũ sáp nhập), geom* – 126 xã/phường sau sắp xếp 2025, nguồn `data/shp/phuongxa.shp`. Mọi trạm/KDC/công trình/đoạn đường đã gán `maphuong`.
- `loaitram` (B05) – maloaitram PK, tenloaitram, mota
- `trambuyt` (B06) – matram PK, matuyen FK, maloaitram FK, madoan FK, maphuongtien FK, tentram, vido, kinhdo, trangthai (Hoạt động / Đề xuất / Ngừng hoạt động), ngaytao, ngaycapnhat, idntao, tenntao, *geom Point, osm_id*
- `phuongtien` (B07), `khudancu` (B08, *geom MultiPolygon*), `loaicongtrinh` (B10, trongso)
- `congtrinh` (B09) – **dạng ĐIỂM** (*geom Point*, *osm_id, nguon*): 2349 POI thu hút chuyến đi lọc từ `data/shp/congtrinh.shp` (trường, đại học, bệnh viện, phòng khám, TTTM/siêu thị, chợ, hành chính, văn hoá, thể thao, công viên, lưu trú) + 131 khu công nghiệp (điểm đại diện từ `public.sudungdata`). `quymo` là ước tính theo loại. Mã loại: TH, DH, BV, YT, TT, BL, HC, DL, GT, CV, LT, CN (TM không còn dùng).
- `ctpvkhudancu` (B06.1: matram, makhu, khoangcachm), `ctpvcongtrinh` (B06.2: matram, macongtrinh, khoangcachm)
- `tuyen_tram` (**bổ sung**: matuyen, matram, thutu) – một trạm thuộc nhiều tuyến, lưu thứ tự trên tuyến. `trambuyt.matuyen` giữ theo Word = tuyến chính.

Bảo mật: `n_ngdung` (B11), `quyen_nnd` (B11.1), `quyen` (B12), `nguoidung` (B13), `ls_dnhap` (B14), `saoluu` (B15).

Cột in nghiêng là bổ sung so với Word (geom PostGIS SRID 4326 để truy vấn không gian; `osm_id`/`nguon_gid` để truy vết nguồn).
Khoảng cách tính bằng `geography` (mét). Hiển thị bản đồ dùng `ST_Transform(geom, 3857)`.

Định tuyến (`database/04_tuyen_mau.sql`): bảng `mang_dinh`, `mang_canh` (pgRouting, dựng từ doanduong); `fn_duong_di(diem_tu, diem_den)`, `fn_dung_hinh_tuyen(matuyen)` (dựng lại geom tuyến từ `tuyen_tram`), `fn_tao_tuyen_mau(...)`. Tuyến mẫu M01–M06 (`tenntao = 'Dữ liệu mẫu'`, chạy lại 04 sẽ tạo lại): M01 Giáp Bát–Nhổn, M02 Hà Đông–Nguyễn Trãi, M03 Mỹ Đình–Long Biên, M04 Long Biên–Gia Lâm, M05 Giáp Bát–Ngọc Hồi, M06 Long Biên–Đông Anh. Mạng đường chỉ có trục chính nên tuyến đi theo trục chính.
Kiểm tra trực quan tuyến: script PowerShell vẽ PNG bằng System.Drawing (không cần matplotlib) rồi xem ảnh.

Phân tích (`database/06_phantich.sql`, tính trên UTM 48N EPSG:32648, có index hàm `ST_Transform(geom,32648)`):
- `fn_vung_phucvu(r, vung)` – hợp vòng phục vụ (hiển thị)
- `fn_tinh_ctpv(r, matram)` – lấp B06.1/B06.2
- `fn_phucvu_kdc(vung, r)` – tỷ lệ phục vụ từng KDC (lõi)
- `fn_khuvuc_thieutram(vung, r, nguong)` – Quy trình 3.2
- `fn_ungvien(vung, r, cach_toithieu, buoc, toida, nguong)` – Quy trình 3.3 + 3.4 – **phiên bản 2 ở `11_dexuat_v2.sql`**: chỉ tính dân/công trình CHƯA được phục vụ; `tong_diem = 3×dan_moi/1000 + Σ trongso×mucdouutien`; chọn tham lam phủ tối đa (trừ phần đã phủ sau mỗi lần chọn); loại hầm/cầu vượt/đường dẫn cầu (`fn_duong_dat_duoc_tram`) và điểm trên mặt nước (`fn_tren_mat_nuoc`, public.thuyhea); fallback đặt trong KDC khi không có đường gần (cột `nguon` = 'Đường' / 'Khu dân cư'). Toàn TP ~5 s, 1 phường < 0,1 s. Hàm dùng bảng tạm ⇒ gọi trong transaction đọc-ghi (không chạy được với `default_transaction_read_only`).
- `fn_danhgia_diem(diem, r)` – đánh giá điểm bất kỳ (v2, cùng công thức)
- `fn_tao_tram_dexuat(kinhdo, vido, ten, maloai, matuyen, idntao, tenntao, r)` – Quy trình 3.5
- `fn_thongke_luoi(kichthuoc, vung)`, `fn_thongke_phamvi(r, vung)`, view `v_thongke_tuyen`, `v_thongke_phuong` – Quy trình 4; `12_thongke.sql`: `fn_thongke_phuong(r)` (bảng tổng hợp theo phường: số trạm, trạm/km², dân/trạm, % dân KDC được phục vụ, công trình…), `fn_thongke_ct_phucvu(r)` (theo loại công trình)
- **Chọn vị trí đặt trạm (đa tiêu chí)** – `13_chon_vitri.sql`: `fn_dancu_chua_pv(vung, r)` (mảnh dân CHƯA được phục vụ, mật độ md người/m²: KDC theo dân ước tính + phần dân THẬT còn lại của phường rải đều ngoài KDC & mặt nước – dasymetric, tổng khớp số liệu phường); `fn_chon_vitri(vung, r, buoc)` rải ứng viên mỗi `buoc` m dọc doanduong trong vùng (+ 1 điểm trong KDC thiếu trạm xa đường) và trả 5 tiêu chí 0..1: `c_dan` (dân gần ≤ r/2 tính đủ, r/2..r một nửa; chuẩn hoá theo phân vị 90), `c_ct` (Σ trọng số × ưu tiên × (1−0,5·d/r), CT đã có trạm ×0,3; phân vị 90), `c_tram` (< 150 m → 0, ≥ 400 m → 1), `c_gt` (0,6 × cấp đường QL 1/TL 0,85/đô thị 0,7 + 0,4 nếu tuyến buýt ≤ 30 m), `c_tc` (cách nút giao – đỉnh `mang_dinh` ≥ 3 cạnh: < 25 m → 0, ≥ 75 m → 1) + số liệu gốc + `wkt_doan` (khúc đường của điểm, 3857). Điểm tổng có trọng số tính ở app. Hàm SQL thuần (không bảng tạm), 1 phường 0,1–0,3 s, vùng 50 km² ~1,4 s.
- `xebus.unaccent_vn(text)` (`10_tienich.sql`) – bỏ dấu tiếng Việt để tìm kiếm không dấu
- `fn_ma_tiep(bang, cot, tiento)` – sinh mã kế tiếp (TR1259…)
Tham số `vung` là geometry SRID 4326 (NULL = toàn TP). Lưu ý hiệu năng: CTE dùng nhiều lần trong hàm SQL nên `MATERIALIZED`.
Dân số KDC là **ước tính**: phân bổ dân số thật của phường (shapefile) cho các KDC trong phường theo diện tích, giới hạn mật độ 40 000 (phường) / 15 000 (xã) người/km² (`09_phuongxa_congtrinh.sql`) – ghi rõ khi hiển thị. Tổng ~2,28 triệu (phần dân sống ngoài các khu OSM đã vẽ không được tính). Dân số phường (`phuongxa.danso`, tổng 7,94 triệu) là số liệu thật.
Thống kê theo khu vực dùng **phường/xã** (`v_thongke_phuong`) – không dùng quận.

## Nhóm người dùng & quyền dữ liệu (theo Word mục 3.1)
| Bảng | Người quản lý (QL) | Khách hàng (KH) | Quản trị viên (QTV) |
|---|---|---|---|
| tuyenbuyt | E,R | R | R |
| doanduong, quanhuyen, phuongxa, loaitram | R | R | R |
| trambuyt | C,E,R | R | R |
| phuongtien, khudancu, congtrinh, loaicongtrinh | R | – | R |
| ctpvkhudancu, ctpvcongtrinh | C,E,R,D | – | R |

Chức năng: QL 18 chức năng (trạm, tuyến, đề xuất, thống kê); KH 6 chức năng xem/tra cứu; QTV: quản lý người dùng, phân quyền, lịch sử truy cập, sao lưu **+ `DX_CHONVITRI`** (Chọn vị trí đặt trạm đa tiêu chí – người dùng yêu cầu 2026-10-13: chỉ Quản trị viên; QL không có).
Mỗi form/nút phải kiểm tra `Session.Co(Quyen.XXX)` trước khi hiển thị/thực hiện.

## Cấu trúc thư mục
```
D:\XeBus_GIS\
├─ CLAUDE.md
├─ Nhom5_Thietkehethong.docx
├─ docs\UI_DESIGN.md          # design system + wireframe các màn hình
├─ database\                  # script SQL đánh số chạy theo thứ tự, backup\
└─ XeBus_GIS\                 # project WinForms
   ├─ Data\       Db.cs, *Repository.cs
   ├─ Models\
   ├─ Security\   PasswordHasher.cs, Session.cs, Quyen.cs
   ├─ Map\        MapManager.cs, MapTools.cs
   ├─ UI\         Theme.cs, Icons.cs, UiHelper.cs, Controls\ (control tự vẽ dùng chung)
   ├─ app.manifest   # DPI System aware
   └─ Forms\      FrmDangNhap, FrmMain, BoiCanh (ngữ cảnh + IModule), Uc* (màn hình), Frm* (hộp thoại)
D:\XeBus_GIS\data\shp\        # shapefile người dùng cung cấp: phuongxa.shp, congtrinh.shp (WGS84, UTF-8)
```

## Quy ước code
- Mỗi Form/UserControl có cặp `X.cs` + `X.Designer.cs` (để mở được Designer trong VS) và phải được khai báo trong `XeBus_GIS.csproj` (`<Compile Include=... SubType=Form/UserControl>` + `DependentUpon`). **Project dạng cũ – file mới không tự vào project.**
- Giao diện tiếng Việt có dấu; file .cs lưu UTF-8 (có BOM) để VS hiển thị đúng.
- Màu/font chỉ lấy từ `UI/Theme.cs`, không hard-code màu trong form.
- Truy vấn DB qua `Data/Db.cs`; thao tác dài chạy `async/await` (Task.Run) + disable nút + con trỏ chờ.
- Connection string: `App.config` (trong repo, mật khẩu là chỗ giữ `MAT_KHAU_POSTGRES`) bị ghi đè bởi `XeBus_GIS/App.local.config` (riêng từng máy, `.gitignore`; mẫu `App.local.config.example`) – đọc qua `Data/CauHinh.cs` (`CauHinh.ChuoiKetNoi`, `CauHinh.Doc` cho appSettings). **Không bao giờ ghi mật khẩu thật vào App.config** (repo public). Chưa có file riêng ⇒ trang đăng nhập báo hướng dẫn.
- **DPI (125/150%…)**: mọi `*.Designer.cs` đặt `AutoScaleMode.None` + toạ độ thiết kế ở 96 DPI; ctor gọi `UiHelper.CoGianDpi(this)` NGAY sau `InitializeComponent()` (trước code dựng control bằng `Dp()`). Không dùng auto-scale của WinForms: chế độ Font không scale control con (Font gốc đặt sau AutoScaleMode), chế độ Dpi scale Label/ListBox hai lần khi Font gốc khác mặc định (đã đo: ×2,25 ở 150%). Control trong `UI.Controls` và UserControl đã tự co giãn chỉ được nhân vị trí/kích thước. Thuộc tính px do Designer ghi mà Scale() không đụng (vd `ThanhDieuHuong.RongMo`, `Splitter.MinSize`) phải là px thiết kế rồi nhân `Dp()` khi dùng. Form/control mới: theo đúng mẫu này. Tự kiểm tra DPI thật: scratchpad `dpi.ps1` tạm đổi tỷ lệ màn hình (DisplayConfig, luôn trả về 100%) – chỉ khi người dùng cho phép.

## Thiết kế giao diện (tóm tắt – chi tiết ở docs/UI_DESIGN.md)
Phong cách **Enterprise GIS Desktop · Modern Cartographic Workspace** (đợt 2, 2026-10-15), có **chế độ sáng/tối chuyển nóng**: token trong `Theme` là thuộc tính, 2 bảng `BangSang`/`BangToi`; `UI/ThemeManager.cs` áp lên form đang mở (ánh xạ màu cũ→mới theo giá trị + hành động đăng ký từ UiHelper + WM_SETREDRAW + tiêu đề cửa sổ tối); `UI/CaiDatNguoiDung.cs` lưu lựa chọn (`%LOCALAPPDATA%\XeBus_GIS\caidat.txt`: theme, nen, vitri, vitri_thoitiet, rong_panel_phai); màu chữ trên nút Accent dùng `Theme.ChuTrenAccent` (tối: Accent sáng ⇒ chữ tối); màu ký hiệu bản đồ cố định (`Theme.MapTram`…). Khi tự kiểm tra bằng `ThemeManager.DoiCheDo` nhớ trả `caidat.txt` về `theme=sang` (hàm này GHI lựa chọn). Header: chip thời tiết (Open-Meteo, `Data/ThoiTietService`) + chip vị trí (`Map/DinhVi`, `FrmChonViTri`; không định vị IP, không lưu toạ độ Windows) + nút sáng/tối. Toolbar chia nhóm có nhãn (Điều hướng/Chọn/Đo đạc/Dữ liệu/Xuất). Panel phải mặc định thu thành dải icon, thẻ `TheDoiTuong` thay lưới 2 cột. UcThongKe đưa biểu đồ + bảng xuống khung dưới. Chi tiết: docs/UI_DESIGN.md. **Khi làm giao diện: dùng skill `ui-ux-pro-max` và `ui-design-system`** (cài trong `.claude/skills/`; nếu chưa nạp thì đọc `SKILL.md` và chạy `python .claude/skills/ui-ux-pro-max/scripts/search.py "..." --design-system`). Bảng màu B “slate + teal” (2026-10-13, người dùng chê bản xanh dương):
`Background #F4F6F8` · `Surface #FFFFFF` · `Sidebar #EEF2F5` (thanh bên tông riêng) · `Accent #0F766E` teal (hành động chính duy nhất, trạm trên bản đồ) · `AccentSoft #E6F4F1` (mục đang chọn) · `Accent2 #0EA5E9` (vùng phục vụ) · `Highlight #F97316` (**chỉ** trạm đề xuất/ứng viên) · `MapChon #FACC15` (đối tượng đang chọn trên bản đồ) · `Success #16A34A` · `Warning #F59E0B` · `Danger #DC2626` · `Text #0F172A` · `Muted #475569` · `Border #E2E7EC`. Phương án A (ấm, kiểu Claude.ai, accent `#2457D6`) giữ trong lịch sử git nếu cần đổi.
Font Segoe UI (`FontTrang` 15pt … `FontNho` 8.25pt); icon Segoe Fluent Icons/MDL2 (không emoji).
- Dùng chung: `UI/UiHelper.cs` (nút có hover/pressed/focus bàn phím/disabled, `Tab` gạch chân, `Luoi` + `NhanTrangThai` + `TrangThaiRong`, `CoGianNgang` cho panel đặt vị trí tuyệt đối, `KhuVucThuGon`, `BieuDo`, `GanIcon`) và `UI/Controls/` (ThanhDieuHuong, PanelLopBanDo, KhungChuGiai, TheKpi, ThanhBuoc, Toast, HopThoai – control dựng bằng code, không có Designer).
- Thông báo: `UiHelper.ThongBao/ThanhCong/CanhBao` = toast không chặn; `Loi`, `XacNhan`, `XacNhanNguyHiem` (xoá) = hộp thoại `HopThoai`. Không gọi `MessageBox` trực tiếp. Text tự vẽ dùng `TextFormatFlags.NoPrefix` (tên có "&").
- FrmMain: menu trái (220↔56, Ctrl+B; nút thu/mở ở góc trên cạnh tên app như Claude.ai, khi thu gọn di chuột lên logo để mở) · đầu trang (breadcrumb, tiêu đề, tìm nhanh Ctrl+F, tài khoản) · công cụ bản đồ (có đo dài/diện tích, Esc huỷ) · module trái ⇔ bản đồ ⇔ panel phải "Thông tin | Lớp & chú giải" (Splitter). Tổng quan và Quản trị là trang toàn màn hình (ẩn bản đồ). `BoiCanh.MoModule(khoa)` mở màn hình khác (TONGQUAN, BANDO, TRAM, TUYEN, PHANTICH, DEXUAT, THONGKE, QUANTRI); FrmMain tự ẩn `lblTieuDe` của module.
- **Bản đồ nền**: MapWinGIS 4.9 chỉ tải tile qua http, máy chủ OSM chỉ còn https, mạng của người dùng không phân giải được `openstreetmap.org`, CARTO đòi API key ⇒ `Map/MayChuNenCucBo.cs` mở `127.0.0.1:<cổng ngẫu nhiên>`, lấy tile https (User-Agent riêng, tuỳ chọn làm nhạt, tuỳ chọn ghép lớp nhãn PNG lên nền), đệm `%LOCALAPPDATA%\XeBus_GIS\tiles`. **Mặc định `EsriXam`** = Esri World Light Gray Base + Reference (người dùng chê OSM rối, 2026-10-14; Esri ô `{z}/{y}/{x}`, tối đa z16), còn `EsriPho` (World Street Map), `OSM` (HOT làm nhạt), `OSMChuan`, `None`. Người dùng đổi ở đầu tab "Lớp & chú giải" (`NhomChip`, `MapManager.CacNen`), lưu `%LOCALAPPDATA%\XeBus_GIS\nen.txt`; chưa chọn thì theo `App.config` `NenBanDo`.
- **Click chọn đối tượng** (`BanDoRepository.TimTaiDiem`): CHỈ trạm (ưu tiên) → công trình (khi lớp bật) → tuyến (chỉ ở màn hình Tuyến). Không chọn đường/KDC/phường nữa (người dùng yêu cầu). Dung sai theo cỡ ký hiệu trạm đang hiển thị (`MapManager.DungSaiChonM`, ≥ 10 px).
- **Ký hiệu công trình**: `Map/KyHieuCongTrinh.cs` – mỗi mã loại một icon Segoe Fluent trên hình tròn màu nhóm (trường E7BE, y tế E95E, TTTM E7BF, chợ E719, KCN EC07, hành chính E825, văn hoá E734, thể thao E805, công viên EC0A, lưu trú E80F); `KyHieuTram.Ve/Anh(..., glyph, tron)`; cỡ `CoCongTrinhTheoTyLe` (> 1:60.000 chấm, rồi 15/19/24 px), cập nhật cùng ký hiệu trạm ở `ExtentsChanged`. Lớp công trình: cột `kieu` = mã loại. Chú giải `ChuGiai(ten, mau, glyph)` (KieuKyHieu.IconTron).
- **Ký hiệu trạm**: hình xe buýt giống logo (`Map/KyHieuTram.cs` vẽ PNG đúng cỡ, `ptSymbolPicture`, PNG alpha hiển thị đúng); cỡ theo tỷ lệ trong `MapManager.CapNhatKyHieuTram` (ExtentsChanged): > 1:120.000 chấm tròn, rồi 14/18/24/30 px; trạm đang chọn cùng hình nền vàng.
- **Khung dưới bản đồ**: `BoiCanh.HienBangDuoi/AnBangDuoi` – bảng kết quả nằm ngang dưới bản đồ + panel phải (UcTram dùng).
- **Đăng nhập** (`FrmDangNhap`): trang toàn màn hình, trái `UI/Controls/NenVideo` = **WPF MediaElement trong ElementHost** phát `Assets/nen_dangnhap.mp4` (H.264 1600×1100 24 fps, 8 s lặp liền bằng `MediaTimeline` RepeatBehavior.Forever; ảnh tĩnh `nen_dangnhap.jpg` khi chưa mở được/tắt hiệu ứng) + lớp phủ, giới thiệu, số liệu thật vẽ cùng trong WPF; phải là form. **Không phát video bằng GDI+** (bản chuỗi JPEG + hoà trộn CPU trước đây bị người dùng chê mờ và giật). Claude không xem được video đang chạy – kiểm tra bằng `MediaClock.CurrentTime` tăng đều + 2 ảnh chụp cách nhau. Trang này dùng tông than chì `Theme.DnNhan*`/`DnPhu` (người dùng chê xanh ngọc "kiểu AI" ở đây; trong app vẫn teal). Video phong cách sa bàn ban ngày (nhà trắng ngà, bóng nắng, nước, cây xanh), KHÔNG neon/glow, KHÔNG làm mờ. Ô nhập dùng `UiHelper.KhungO` (khung 38 px, nút trong khung ⇒ các ô dài bằng nhau).
- **Đăng ký** (`UcDangKy` trong cột phải trang đăng nhập): `NguoiDungRepository.DangKy` tạo người dùng nhóm Khách hàng, trạng thái **"Chờ duyệt"** (không đăng nhập được, báo "đang chờ quản trị viên duyệt"); QTV duyệt ở `UcQuanTri` tab Người dùng: bộ lọc "Tất cả | Chờ duyệt (n)" (tự chuyển sang Chờ duyệt khi có), chọn tài khoản ⇒ khung "Duyệt tài khoản" (chọn nhóm, `QuanTriRepository.Duyet`) / "Từ chối" (xoá yêu cầu, `TuChoi`) – cả hai chỉ tác động bản ghi còn "Chờ duyệt". Video dựng bằng Blender (`D:\Tools\blender-4.5.10-windows-x64\blender.exe`) từ dữ liệu CSDLKG: `tools/blender/xuat_du_lieu.sql` (chỉ SELECT) → `dung_canh.py` (render khung JPEG) → `dong_goi_video.ps1` (ffmpeg → mp4 yuv420p + ảnh tĩnh) (xem đầu mỗi file). Windows tắt hiệu ứng ⇒ ảnh tĩnh.
- VS Designer có thể tự ghi lại thuộc tính vào `*.Designer.cs` khi mở form (đã gặp `navBar.BackColor = White`) – control tự vẽ nên ẩn thuộc tính màu bằng `DesignerSerializationVisibility.Hidden`.
- DPI: `app.manifest` (System aware); kích thước tạo trong code nhân `UiHelper.Dp()`; control Designer co giãn bằng `UiHelper.CoGianDpi` (xem Quy ước code).

## Build & chạy (người dùng thực hiện trong VS)
1. Mở `XeBus_GIS.sln`, cấu hình **Debug | Any CPU** (csproj đã đặt `PlatformTarget=x64` cho cả Debug/Release – exe luôn là 64-bit).
2. Lần đầu: Restore NuGet (tự động khi build).
3. F5. Tài khoản mẫu: `admin` / `Admin@123`, `quanly` / `Quanly@123`, `khach` / `Khach@123` (hoặc link “Tiếp tục với tư cách khách”).

Kiểm tra biên dịch từ dòng lệnh (Claude dùng để tự kiểm tra, không chạy app):
`"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" XeBus_GIS.sln -restore -p:Configuration=Debug -v:minimal`

## Ghi chú kỹ thuật MapWinGIS
- Interop sinh từ `COMReference` trong csproj (tlbimp/aximp). Thuộc tính có chỉ số dùng dạng `get_X(i)` / `set_X(i, v)` (vd `sf.set_ShapeSelected(i, true)`, `Map.set_LayerVisible(h, b)`).
- `AxMap` được tạo trong code (`FrmMain.TaoBanDo`), cấu hình sau khi form hiển thị (`Shown`) – không đặt thuộc tính ActiveX trước khi control có handle.
- Lớp nghiệp vụ = Shapefile trong bộ nhớ (`CreateNewWithShapeID` + `Shape.ImportFromWKT`), dữ liệu lấy bằng SQL trả cột `ma` + `wkt` (EPSG:3857). Phân loại ký hiệu dùng cột ASCII `kieu` (tránh so sánh chuỗi có dấu trong biểu thức MapWinGIS).
- **`Shapefile.CollisionMode = AllowCollisions`** (đặt trong `MapManager.DungLop`): mặc định MapWinGIS KHÔNG vẽ ký hiệu điểm trùng vị trí với điểm của lớp khác ⇒ trước 2026-10-13 lớp "trạm đánh số trên tuyến" / ứng viên trùng trạm không hiện.
- MultiPolygon lớn nhiều lỗ (vùng phục vụ toàn TP) phải tách `ST_Dump` thành từng polygon rồi mới `ImportFromWKT`, nếu không các phần bị nối thành đường thẳng dài (`DeXuatRepository.VungPhucVu`).
- Ẩn khung bản đồ (trang toàn màn hình) làm control co về 0 ⇒ `FrmMain` lưu/khôi phục `Extents`; lúc menu co giãn dùng `AxMap.LockWindow`.
- Bật/tắt lớp từ module qua `MapManager.HienLop` (phát `HienThiThayDoi` để panel Lớp đồng bộ).
- Tra enum/chữ ký API: sinh interop bằng `TlbImp.exe C:\dev\MapWinGIS\MapWinGIS.ocx` vào scratchpad rồi reflection (file XML tài liệu đi kèm không đủ).

## Lộ trình
Phase 0 tài liệu → 1 CSDL → 2 khung app (đăng nhập, form chính, bản đồ) → 3 Quản lý trạm → 4 Quản lý tuyến + gán trạm → 5 Đề xuất trạm mới → 6 Thống kê + xuất báo cáo → 7 Quản trị.
**Đã xong toàn bộ Phase 0–7** (chờ người dùng chạy thử trên VS):
| Phase | Thành phần |
|---|---|
| 3 Quản lý trạm | `UcTram`, `FrmCapNhatTram`, `TramRepository` |
| 4 Quản lý tuyến | `UcTuyen`, `FrmCapNhatTuyen`, `FrmGanTram` (cửa sổ không chặn – click trạm trên bản đồ để thêm), `TuyenRepository` |
| 5 Đề xuất trạm mới | `UcDeXuat` (wizard 5 bước, biểu đồ cột chồng điểm dân số/công trình, đánh giá vị trí tự chọn "TC1…"), `DeXuatRepository`. Tab **"Chọn vị trí đặt trạm"** (`UcChonViTri` + `ChonViTriRepository`, quyền `DX_CHONVITRI`): UcDeXuat gom nội dung wizard vào panel và thêm thanh 2 tab khi có cả hai quyền; chỉ có DX_CHONVITRI (QTV) ⇒ chỉ hiện tab này. Cột trái: khu vực (phường/vẽ), 5 `ThanhTrongSo` + mẫu trọng số (`NhomChip`), chip "xem từng tiêu chí trên bản đồ" (đường tô theo `Theme.ThangMauPhuHop` + lớp ngữ cảnh: dân chưa PV / vùng đã PV / nút giao / bật lớp Công trình, Tuyến) + `ThanhThangMau`, `KhungChuGiai`. Khung dưới bản đồ: xếp hạng vị trí (thanh chồng đóng góp 5 màu `Theme.MauTieuChi`) / xếp hạng đường (dải màu dọc đường) · `BieuDoRadar` (tự vẽ) · `BangTieuChi` (số liệu từng tiêu chí) + tạo trạm đề xuất. Kéo trọng số chỉ tính lại ở app (Timer 180 ms), không gọi CSDL. |
| 6 Thống kê | `UcThongKe` (Khu vực: phường/lưới 1–2 km + choropleth; Tuyến; Phạm vi PV: tô phường theo % dân được PV + viền vùng phục vụ), `ThongKeRepository`, `BaoCaoExcel` (ClosedXML, kèm ảnh bản đồ `MapManager.ChupAnh`) |
| 7 Quản trị | `UcQuanTri` – trang trong cửa sổ chính (người dùng, nhóm & phân quyền, lịch sử truy cập, sao lưu pg_dump `-n xebus`), `QuanTriRepository` – chốt an toàn: không xoá/khoá/chuyển nhóm QTV hoạt động cuối, không tự xoá mình, nhóm QTV luôn giữ QT_PHANQUYEN |
`UcDangPhatTrien` không còn được dùng (giữ làm mẫu).
**Nâng cấp UI/UX (2026-10-13, không đổi CSDL):** `UcTongQuan` + `TongQuanRepository` (Tổng quan, chỉ SELECT); `UcPhanTich` (Phân tích khu vực thiếu trạm – màn hình riêng, quyền `DX_THIEUTRAM`, dùng `fn_khuvuc_thieutram`, chuyển vùng sang Đề xuất qua `UcDeXuat.DatVung`); `BanDoRepository.TimNhanh` (ô tìm nhanh); `CongCu.DoDai/DoDienTich`; `FrmQuanTri` → `UcQuanTri` (bản cũ đã bỏ khỏi project).
Tuyến: theo Word chỉ E,R (không tạo/xoá tuyến trong app). Trạng thái tuyến: Hoạt động / Tạm dừng / Ngừng hoạt động. Nhãn số thứ tự trạm trên bản đồ: `MapManager.DatNhan(lớp, "[thutu]", màu)`.
Mẫu module: UserControl có ctor rỗng (Designer) + ctor `(BoiCanh)`; chỉ phản ứng sự kiện `MapTools.VeVungXong/ChonDiemXong` khi chính module đang chờ (`_dangCho`); lớp tạm riêng, xoá ở `NgungKichHoat`. Chọn vị trí trên bản đồ từ hộp thoại: trả `DialogResult.Retry`, module bật công cụ ChonDiem rồi mở lại hộp thoại.
**Tự kiểm tra giao diện bằng ảnh** (không mở VS, không hiện cửa sổ trên màn hình người dùng): PowerShell `-STA` nạp exe từ bytes, `Session.BatDau(...)` với quyền từ `NguoiDungRepository.NapQuyen(nhom)` (không ghi `ls_dnhap`), tạo form ở (-4000,0), `Show()` + vòng `DoEvents`, `ChonModule(...)`, chụp bằng Win32 `PrintWindow(hwnd, hdc, 2)` rồi xem PNG; đặt `DaDangXuat = true` trước `Close()`. **Bắt buộc** `SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext())` ngay trước MỖI lần gọi hàm async – PowerShell trả context về mặc định giữa các câu lệnh ⇒ phần sau `await` chạy ở thread pool, gọi COM sai luồng ⇒ AccessViolation/treo (lỗi bộ test, không phải app). Chạy bằng `Start-Process` + `WaitForExit(timeout)`.
Tự kiểm tra repository không cần chạy app: nạp `bin\Debug\XeBus_GIS.exe` vào PowerShell bằng reflection (xem ghi chú Smart App Control ở đầu file), set `Db._connStr`, gọi hàm tĩnh; tham số List/mảng tạo bằng `::new()` để PowerShell không bọc PSObject. Hàm ghi dữ liệu: thử bằng SQL tương đương trong `BEGIN … ROLLBACK`, hoặc tạo bản ghi thử rồi xoá.
Thứ tự script CSDL (cài mới từ đầu): 00 → 01 → 02 → 03 → 04 → 05 → 06 → 08 (SRID giaothong) → nạp `data/shp/*.shp` bằng `shp2pgsql -c -s 4326 -W UTF-8` vào `xebus.tmp_phuongxa`, `xebus.tmp_congtrinh` → 09 (phường/xã, công trình, dân số KDC) → 10 (unaccent_vn) → 11 (đề xuất v2, ghi đè hàm của 06) → 12 (thống kê) → 13 (chọn vị trí đa tiêu chí + quyền DX_CHONVITRI). 07 (import quận) đã lỗi thời – không dùng.
Thông số kiểm thử gần nhất (2026-10-13): 1258 trạm, 6 tuyến mẫu (102 lượt dừng), 126 phường/xã, 859 KDC, 2480 công trình; 35,6% dân KDC trong 500 m; 9 phường/xã chưa có trạm (vd Xã Thư Lâm, Phường Vĩnh Hưng).
Bản đồ có thêm lớp “Ranh giới phường/xã”; lớp Công trình là điểm, tô màu theo nhóm (cột `kieu`: YT, GD, TM, CN, KH).
csproj có `RuntimeIdentifier=win-x64` (bắt buộc cho NuGet restore khi x64 + PackageReference).
