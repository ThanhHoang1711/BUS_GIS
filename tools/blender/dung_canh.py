"""
Dựng cảnh video nền trang đăng nhập XeBus GIS (Blender 4.5, chạy nền) từ dữ liệu thật CSDLKG + tài nguyên CC0:
phố Tràng Thi (trạm 6-8 Tràng Thi) → máy quay nâng lên → lượn quanh Hồ Gươm, lớp tuyến/ghim trạm hiện dần.

    blender -b --factory-startup -P tools/blender/dung_canh.py -- <canh.json> <thư mục ra> <chế độ> [engine] [%] [mẫu] [khung]
      chế độ  thu  : render ảnh tĩnh tại các khung (vd "1,150,300,450,600")
              phim : render dãy khung "dau-cuoi" (bỏ qua khung đã có ⇒ chạy lại để render tiếp)
      engine  CYCLES (mặc định) | EEVEE | WORKBENCH (xem nhanh bố cục)
Video 600 khung @24 fps (25 s); dong_goi_video.ps1 hoà 24 khung cuối vào 24 khung đầu ⇒ 576 khung lặp liền.
Dữ liệu: tools/blender/xuat_du_lieu.sql (chỉ SELECT). Tài nguyên: tools/blender/tai_tai_nguyen.py.
"""
import json
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
JSON, RA, CHE_DO = argv[0], argv[1], (argv[2] if len(argv) > 2 else "thu")
ENGINE = argv[3] if len(argv) > 3 else "CYCLES"
TY_LE = int(argv[4]) if len(argv) > 4 else 100
MAU = int(argv[5]) if len(argv) > 5 else 96
KHUNG = argv[6] if len(argv) > 6 else "1,150,300,450,600"
SO_KHUNG = 600
import time as _time
_T0 = _time.time()
try:
    sys.stdout.reconfigure(line_buffering=True)
except Exception:
    pass


def moc_tg(ten):
    print("[%6.1fs] %s" % (_time.time() - _T0, ten), flush=True)

import bpy  # noqa: E402
from mo_hinh import canh_chung, thanh_pho, cay_den, diem_nhan, xe_buyt, xe_khac, nha_cho, giao_thong, ban_do, may_quay  # noqa: E402
from mo_hinh.giao_thong import Tuyen, lech  # noqa: E402

MAU_TUYEN = {"M01": "#2563EB", "M02": "#DC2626", "M03": "#16A34A", "M04": "#9333EA", "M05": "#EA580C", "M06": "#0891B2"}

with open(JSON, encoding="utf-8") as f:
    DL = json.load(f)

canh_chung.xoa_canh()
sc = bpy.context.scene
sc.frame_start, sc.frame_end = 1, SO_KHUNG
canh_chung.may_render(ENGINE, 1600, 1200, MAU, TY_LE)
canh_chung.bau_troi(1.0, xoay_hdri=0.0)
canh_chung.nang(5.5)
sc.view_settings.exposure = 0.3

# ---------------------------------------------------------------- thành phố
tuyen_duong = thanh_pho.dung_thanh_pho(DL)
moc_tg("thanh pho")
thanh_pho.tam_giac("Dao", DL.get("dao"), thanh_pho.Z_CO, __import__("mo_hinh.vat_lieu", fromlist=["co"]).co())
diem_nhan.dung_diem_nhan()
moc_tg("diem nhan")

# ---------------------------------------------------------------- phố chính + trạm mở đầu
bd = DL["trambd"]
pl = [tuple(p) for p in (bd["duong"][0] if isinstance(bd["duong"][0][0], list) else bd["duong"])]   # đông → tây
W = thanh_pho.BE_RONG.get("Tràng Thi", 14)
thanh_pho.pho_chinh(pl, W)
pho = Tuyen(pl)
s_tram = pho.chieu(bd["x"], bd["y"])
x_t, y_t, h_t = Tuyen(lech(pl, -(W / 2 + 2.3))).tai(s_tram)
nc = nha_cho.tao_nha_cho("NhaCho", bd["ten"], ("02", "03", "04"))
nc.location = (x_t, y_t, thanh_pho.Z_VIAHE_CAO)
nc.rotation_euler = (0, 0, h_t + math.pi)

# cây + đèn (tránh chỗ nhà chờ); pho_chinh phải là cùng list trong tuyen_duong để cây đứng trên vỉa hè cao
for i, (ten, w, p) in enumerate(tuyen_duong):
    if ten == "Tràng Thi" and len(p) == len(pl):
        tuyen_duong[i] = (ten, w, pl)
# chỗ máy quay đứng ở phố (khung 1, 110, 200) – không trồng cây chắn ống kính
_tr = [(Tuyen(lech(pl, W / 2 + 3.4)).tai(s_tram + d)[:2]) for d in (24, 19, 18)]
cay_den.cay_va_den(DL, tuyen_duong, pl, (x_t, y_t), tranh_them=[(x, y, 8.0) for x, y in _tr])
moc_tg("cay den")

# ---------------------------------------------------------------- xe buýt chính (tuyến 02 – đỏ)
lan_buyt = lech(pl, -(W / 2 - 1.9))
# kéo dài 300 m về phía đông (xe đến từ phía hồ) và 300 m về phía tây
(x0, y0), (x1, y1) = lan_buyt[0], lan_buyt[1]
d = math.dist((x0, y0), (x1, y1)) or 1
dau = (x0 - (x1 - x0) / d * 300, y0 - (y1 - y0) / d * 300)
(xa, ya), (xb, yb) = lan_buyt[-2], lan_buyt[-1]
d = math.dist((xa, ya), (xb, yb)) or 1
cuoi = (xb + (xb - xa) / d * 300, yb + (yb - ya) / d * 300)
tuyen_buyt = Tuyen([dau] + lan_buyt + [cuoi])
s_dung = tuyen_buyt.chieu(x_t, y_t) - 3.0
for k in (-1, 0, 1):
    goc, than, banh = xe_buyt.tao_xe_buyt("XeBuyt%d" % (k + 1), MAU_TUYEN["M02"], "02", "Kim Mã")
    giao_thong.chay_xe_buyt(goc, banh, tuyen_buyt, s_dung, k, SO_KHUNG, z=thanh_pho.Z_DUONG)
    moc_tg("xe buyt chinh")

# ---------------------------------------------------------------- dòng xe nền
xe_nen = xe_khac.bo_xe_nen()
nguoc = list(reversed(pl))
giao_thong.dong_xe("Dong_TT_tay", lech(pl, -1.75), xe_nen, khoang=30, v=8.5, z=thanh_pho.Z_DUONG, hat=1)
giao_thong.dong_xe("Dong_TT_dong1", lech(nguoc, -1.75), xe_nen, khoang=26, v=9.0, z=thanh_pho.Z_DUONG, hat=2)
giao_thong.dong_xe("Dong_TT_dong2", lech(nguoc, -5.25), xe_nen, khoang=20, v=7.5, z=thanh_pho.Z_DUONG, hat=3)
k = 10
for ten, w, p in tuyen_duong:
    if ten == "Tràng Thi" or len(p) < 2:
        continue
    if min(math.hypot(x, y) for x, y in p) > 1500 or Tuyen(p).L < 120:
        continue
    k += 1
    giao_thong.dong_xe("Dong_%d_a" % k, lech(p, -w / 4), xe_nen, khoang=18, v=9.0, z=thanh_pho.Z_DUONG, hat=k)
    giao_thong.dong_xe("Dong_%d_b" % k, lech(list(reversed(p)), -w / 4), xe_nen, khoang=20, v=8.5, z=thanh_pho.Z_DUONG, hat=k + 50)

# xe buýt các tuyến khác (đoạn bay cao) + dải tuyến
for ma, g in DL.get("tuyen") or []:
    mau = MAU_TUYEN.get(ma, "#2563EB")
    goc_b, than_b, banh_b = xe_buyt.tao_xe_buyt("Xe_" + ma, mau, ma[1:], "", bang_ten=False)
    bpy.ops.object.select_all(action="DESELECT")
    for o in [than_b] + banh_b:
        o.parent = None
        o.select_set(True)
    bpy.context.view_layer.objects.active = than_b
    bpy.ops.object.join()
    bpy.context.scene.collection.objects.unlink(than_b)
    bpy.data.objects.remove(goc_b)
    col_b = bpy.data.collections.new("Bus_" + ma)
    col_b.objects.link(than_b)
    for p in thanh_pho.cac_duong(g):
        if Tuyen(p).L < 200:
            continue
        giao_thong.dong_xe("Buyt_%s_a" % ma, lech(p, -3.0), col_b, khoang=320, v=9.0, z=thanh_pho.Z_DUONG, hat=7)
        giao_thong.dong_xe("Buyt_%s_b" % ma, lech(list(reversed(p)), -3.0), col_b, khoang=360, v=9.0, z=thanh_pho.Z_DUONG, hat=8)
        ban_do.dai_tuyen("Tuyen_" + ma, p, mau, f_an=900, f_het=950)
ban_do.dai_tuyen("Tuyen_02", [tuyen_buyt.pl[0]] + pl, MAU_TUYEN["M02"], f_ve=290, f_xong=330, f_an=900, f_het=950)
ban_do.ghim_tram([tuple(t) for t in DL.get("tram") or []], f_dau=330, f_an=900)
moc_tg("dong xe + ban do")

# ---------------------------------------------------------------- máy quay
def tren_pho(s, lech_m, z):
    x, y, _ = Tuyen(lech(pl, lech_m)).tai(s)
    return (x, y, z)


bus_dung = tuyen_buyt.tai(s_dung)
moc = [
    (1, tren_pho(s_tram + 24, W / 2 + 3.4, 1.55), tren_pho(s_tram - 10, -(W / 2 - 1.9), 1.9), 28),
    (110, tren_pho(s_tram + 19, W / 2 + 3.0, 1.7), tren_pho(s_tram - 3, -(W / 2 - 1.9), 1.8)),
    (200, tren_pho(s_tram + 18, W / 2 + 4.5, 6.5), (bus_dung[0], bus_dung[1], 1.8)),
    (275, tren_pho(s_tram + 30, 2.0, 22.0), tren_pho(s_tram - 40, 0, 0.0), 30),
    (360, tren_pho(s_tram + 70, 30.0, 95.0), (-60.0, -150.0, 0.0)),
    (450, (-430.0, -520.0, 210.0), (-10.0, -40.0, 0.0), 32),
    (530, (-120.0, -640.0, 230.0), (10.0, 0.0, 0.0)),
    (600, (180.0, -600.0, 220.0), (20.0, 40.0, 0.0), 32),
]
cam, dich = may_quay.tao_may_quay(moc, lens=28, f_stop=3.2)
moc_tg("may quay")

# ---------------------------------------------------------------- sương xa (mist pass)
try:
    sc.view_layers[0].use_pass_mist = True
    sc.world.mist_settings.start = 160
    sc.world.mist_settings.depth = 2600
    sc.world.mist_settings.falloff = "QUADRATIC"
    sc.use_nodes = True
    nt = sc.node_tree
    for n in list(nt.nodes):
        nt.nodes.remove(n)
    rl = nt.nodes.new("CompositorNodeRLayers")
    mx = nt.nodes.new("CompositorNodeMixRGB")
    mx.blend_type = "MIX"
    mm = nt.nodes.new("CompositorNodeMath")
    mm.operation = "MULTIPLY"
    mm.inputs[1].default_value = 0.3
    nt.links.new(rl.outputs["Mist"], mm.inputs[0])
    nt.links.new(mm.outputs[0], mx.inputs[0])
    nt.links.new(rl.outputs["Image"], mx.inputs[1])
    mx.inputs[2].default_value = (0.74, 0.8, 0.88, 1)
    co = nt.nodes.new("CompositorNodeComposite")
    nt.links.new(mx.outputs[0], co.inputs["Image"])
except Exception as e:
    print("không đặt được mist:", e)

if ENGINE == "CYCLES":
    sc.render.use_motion_blur = True
    sc.render.motion_blur_shutter = 0.35

os.makedirs(RA, exist_ok=True)
if CHE_DO == "thu":
    for f in [int(x) for x in KHUNG.split(",")]:
        sc.frame_set(f)
        sc.render.filepath = os.path.join(RA, "thu_%04d.jpg" % f)
        bpy.ops.render.render(write_still=True)
        print("ANH", sc.render.filepath)
elif CHE_DO == "luu":
    # lưu cảnh đã dựng: render từng đoạn bằng  blender -b canh.blend -s <đầu> -e <cuối> -a  (bỏ qua khung đã có)
    sc.render.filepath = os.path.join(RA, "k_")
    sc.render.use_overwrite = False
    sc.render.use_placeholder = True
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(RA, "canh.blend"))
    print("LUU", os.path.join(RA, "canh.blend"))
else:
    dau_k, cuoi_k = [int(x) for x in KHUNG.split("-")]
    sc.frame_start, sc.frame_end = dau_k, cuoi_k
    sc.render.filepath = os.path.join(RA, "k_")
    sc.render.use_overwrite = False
    sc.render.use_placeholder = True
    bpy.ops.render.render(animation=True)
    print("XONG", dau_k, cuoi_k)
