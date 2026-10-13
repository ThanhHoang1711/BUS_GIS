# XeBus GIS – Thiết kế giao diện

Phong cách: **Modern GIS Desktop · Clean Enterprise · Light Urban Analytics**. Bản đồ là trung tâm, giao diện sáng, một màu hành động chính (xanh `#2563EB`); các màu khác chỉ dùng để mang nghĩa (trạng thái, ký hiệu bản đồ).

## 1. Design system (`XeBus_GIS/UI/Theme.cs` – không hard-code màu/font trong form)

### Màu
| Token | Giá trị | Dùng cho |
|---|---|---|
| `Background` | `#F5F7FB` | nền cửa sổ, nền trang Tổng quan / Quản trị |
| `Surface` / `SurfaceAlt` | `#FFFFFF` / `#F8FAFC` | panel, thẻ / header lưới, vùng phụ |
| `Accent` (+`AccentHover` `#1D4ED8`, `AccentPressed` `#1E40AF`) | `#2563EB` | nút chính, mục menu đang chọn, trạm trên bản đồ |
| `AccentSoft` | `#EAF1FE` | nền mục đang chọn, dòng lưới được chọn, công cụ đang bật |
| `Accent2` | `#06B6D4` | vùng phục vụ, biểu đồ phụ |
| `Highlight` | `#F97316` | **chỉ** trạm đề xuất / ứng viên |
| `MapChon` / `MapChonDuong` | `#FACC15` / `#EAB308` | đối tượng đang chọn trên bản đồ (khác màu đề xuất) |
| `Success` / `Warning` (`WarningText` `#B45309`) / `Danger` | `#16A34A` / `#F59E0B` / `#DC2626` | trạng thái |
| `Text` / `Muted` / `Disabled` | `#172033` / `#64748B` / `#94A3B8` | chữ chính / phụ / bị khoá |
| `Border` / `BorderStrong` | `#E2E8F0` / `#CBD5E1` | đường kẻ, viền ô nhập |
Thang màu bản đồ: `ThangMauSoTram` (0 trạm đỏ nhạt → xanh đậm), `ThangMauPhucVu` (< 25% đỏ → ≥ 75% xanh lá), `MauTuyen` (8 màu tuyến). Nhãn trạng thái trong lưới: `Theme.MauTrangThai()` (Hoạt động xanh lá, Đề xuất cam, Tạm dừng vàng, Ngừng/Khóa/Thất bại đỏ).

### Chữ (Segoe UI)
`FontTrang` 15 pt Semibold (tiêu đề trang) · `FontTieuDe` 11.25 pt Semibold (khu vực, thẻ) · `FontDam` 9.75 Semibold · `FontThuong` 9.75 · `FontNhan` 9 · `FontNho` 8.25 · `FontKpi` 20 pt. Không viết hoa toàn bộ (trừ nhãn nhóm menu cỡ nhỏ).

### Khoảng cách, hình khối
Bội số 4 (`Sp1..Sp6` = 4/8/12/16/24/32), bo góc 6 px (`BoGoc`) chỉ cho nút menu, nhãn trạng thái, ô logo. Thẻ = nền trắng + viền 1 px `Border` (`UiHelper.VienMong`), không đổ bóng.

### Icon
Font Segoe Fluent Icons (Win11) / Segoe MDL2 Assets (`UI/Icons.cs`), không emoji. Icon cạnh chữ của nút: `UiHelper.GanIcon` (vẽ glyph thành ảnh).

## 2. Thành phần dùng chung
| Thành phần | File | Ghi chú |
|---|---|---|
| Nút chính / phụ / nguy hiểm / icon | `UiHelper.NutChinh/NutPhu/NutNguyHiem/NutNguyHiemChinh/NutIcon` | hover, pressed, **vòng focus khi dùng bàn phím**, trạng thái disabled; `DatTrangThaiBat` cho công cụ đang bật |
| Tab gạch chân | `UiHelper.Tab/ChonTab` | tab con trong Thống kê, Quản trị |
| Lưới | `UiHelper.Luoi`, `NhanTrangThai`, `TrangThaiRong`, `CotVuaDu`, `CotCoDinh` | header nhạt, nhãn trạng thái bo tròn, thông báo khi lưới rỗng |
| Khu vực thu/mở | `UiHelper.KhuVucThuGon` | click tiêu đề khu vực; trả về hàm đặt trạng thái |
| Co giãn ngang | `UiHelper.CoGianNgang` | control đặt vị trí tuyệt đối co theo chiều rộng panel (kéo thanh chia) |
| Biểu đồ | `UiHelper.BieuDo` | nền trắng, lưới mảnh, chữ Muted |
| Thông báo | `Toast` (`UiHelper.ThongBao/ThanhCong/CanhBao`) | không chặn, góc dưới phải, tự đóng 3,5 s, di chuột lên để giữ |
| Hộp thoại | `HopThoai` (`UiHelper.Loi/XacNhan/XacNhanNguyHiem`) | thay MessageBox; thao tác xoá: nút đỏ, focus mặc định vào Huỷ |
| Thanh điều hướng | `UI/Controls/ThanhDieuHuong` | nhóm mục, thu gọn 220↔56 px (≈120 ms, khoá vẽ bản đồ trong lúc co giãn), tooltip khi thu gọn, ↑↓ Enter |
| Lớp & chú giải | `UI/Controls/PanelLopBanDo` | nhóm lớp, ô bật/tắt, ký hiệu, số đối tượng / Đang tải / Lỗi tải, chú giải loại ký hiệu |
| Chú giải ngắn | `UI/Controls/KhungChuGiai` | trong module (Phân tích, Đề xuất, Thống kê) |
| Thẻ số liệu | `UI/Controls/TheKpi` | nhãn · giá trị · chú thích, vạch màu ý nghĩa |
| Thanh bước | `UI/Controls/ThanhBuoc` | wizard Đề xuất; click bước đã qua để quay lại |
| Tiến trình | `UiHelper.ChayAsync` + `UiHelper.TienTrinh` | khoá nút + "Đang …" + thanh marquee ở status bar |

## 3. Bố cục FrmMain
```
┌ Menu 220/56 ┐┌ Đầu trang: breadcrumb · Tiêu đề trang ·········· [Tìm nhanh Ctrl+F] [Tài khoản ▾] ┐
│ ▣ XeBus GIS ││ Công cụ: [Chọn][Kéo] | [+][−][Toàn cảnh] | [Đo dài][Đo diện tích] | [Bỏ chọn][Làm mới] gợi ý… [▣] │
│ CHUNG       │├ Module 370 ┤┊├──────────── Bản đồ MapWinGIS ────────────┤┊├ Thông tin | Lớp & chú giải ┤
│ Tổng quan   ││            │┊│                                          │┊│                            │
│ Bản đồ GIS  ││            │┊│                                          │┊│                            │
│ QUẢN LÝ     │└────────────┘ └──────────────────────────────────────────┘ └────────────────────────────┘
│ Trạm, Tuyến │ Thanh trạng thái: toạ độ · tỷ lệ · WGS 84 · thông báo … [tiến trình] ● CSDLKG
│ PHÂN TÍCH   │   ┊ = thanh chia kéo được (Splitter); panel phải ẩn/hiện bằng nút ▣ hoặc ›
│ Thiếu trạm, Đề xuất, Thống kê
│ HỆ THỐNG    │ Tổng quan, Quản trị: hiển thị toàn trang (ẩn bản đồ, công cụ, panel phải)
└ « Thu gọn ──┘
```
- Mục menu ẩn theo quyền (`Session.Co/CoNhom`); Thiếu trạm cần `DX_THIEUTRAM`.
- Phím tắt: **Ctrl+F** tìm nhanh, **Ctrl+B** thu/mở menu, **F5** nạp lại bản đồ, **Esc** huỷ vẽ/đo/chọn điểm hoặc đóng gợi ý tìm kiếm.
- Màn hình laptop (< 1600 px) mở với menu thu gọn để dành chỗ cho bản đồ.
- Không đặt control nào đè lên bản đồ (ActiveX); trạng thái tải hiện ở status bar và trên nút.

## 4. Màn hình
| Màn hình | Nội dung chính |
|---|---|
| Đăng nhập (`FrmDangNhap`, 720×480) | dải nhận diện xanh nhạt (3 chức năng chính) + cột đăng nhập; lỗi hiện ngay dưới ô mật khẩu |
| Tổng quan (`UcTongQuan`) | 6 thẻ KPI (dữ liệu thật), biểu đồ 12 phường/xã nhiều trạm, bảng phường/xã cần chú ý, trạm cập nhật gần đây, % công trình được phục vụ theo loại; double-click → mở trên bản đồ |
| Trạm (`UcTram`) | tra cứu thuộc tính (thu/mở) · tra cứu không gian (vẽ vùng / phường / bán kính) · lưới kết quả có nhãn trạng thái · sửa |
| Tuyến (`UcTuyen`) | danh sách tuyến có ô màu đúng màu trên bản đồ · trạm theo thứ tự (số trên bản đồ) · sửa, gán trạm |
| Thiếu trạm (`UcPhanTich`) | 1 vùng · 2 tiêu chí (có câu giải thích) · 3 kết quả: KPI, chú giải, KDC thiếu trạm, vùng đã phục vụ; nút chuyển sang Đề xuất cùng vùng/tiêu chí |
| Đề xuất (`UcDeXuat`) | thanh bước 5 bước, chú giải bước 2–3, biểu đồ điểm dân số/công trình, đánh giá vị trí tự chọn |
| Thống kê (`UcThongKe`) | tab Khu vực / Tuyến / Phạm vi phục vụ (tab gạch chân), biểu đồ có tiêu đề trục, chú giải thang màu bản đồ, xuất Excel |
| Quản trị (`UcQuanTri`, toàn trang) | Người dùng · Nhóm & phân quyền · Lịch sử truy cập · Sao lưu |

## 5. Ghi chú kỹ thuật bản đồ
- Mọi lớp (nghiệp vụ và tạm) đặt `Shapefile.CollisionMode = AllowCollisions`; mặc định MapWinGIS **bỏ không vẽ** điểm trùng vị trí với điểm của lớp khác (trạm đánh số trên tuyến, ứng viên trùng trạm).
- Vùng phục vụ (MultiPolygon lớn nhiều lỗ) phải tách từng polygon (`ST_Dump`) trước khi `ImportFromWKT`, nếu không các phần bị nối thành đường thẳng dài.
- Ẩn/hiện khung bản đồ (chuyển sang trang toàn trang) làm control co về 0 → `FrmMain` lưu và khôi phục `Extents`.
- DPI: `app.manifest` khai báo System aware; kích thước tạo trong code dùng `UiHelper.Dp()`. Bỏ dòng `<ApplicationManifest>` trong csproj để quay về chế độ cũ nếu cần.
