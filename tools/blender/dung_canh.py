"""
Dựng cảnh 3D trung tâm Hà Nội từ DỮ LIỆU THẬT của CSDLKG và render chuỗi khung hình làm video nền trang đăng nhập.

Chạy (không mở giao diện Blender):
    blender -b --factory-startup -P tools/blender/dung_canh.py -- <canh.json> <thu_muc_ra> [so_khung] [rong] [cao]

canh.json sinh bằng tools/blender/xuat_du_lieu.sql (chỉ SELECT): toạ độ mét UTM 48N (EPSG:32648) đã trừ tâm
(105.846, 21.030 – quanh Hồ Gươm), bán kính 3,5 km:
    nha   [gid, type, polygon]   – public.nha (OSM building)
    nuoc  [polygon|multipolygon] – public.thuyhea (water, riverbank, reservoir)
    duong [loaiduong, line]      – xebus.doanduong
    tuyen [matuyen, line]        – xebus.tuyenbuyt (M01…)
    tram  [x, y]                 – xebus.trambuyt

Phong cách: thành phố tông tối xanh đêm, tuyến buýt phát sáng đúng màu tuyến trong app (Theme.MauTuyen),
trạm nhấp nháy màu teal, các chấm "xe buýt" chạy dọc tuyến, camera lượn chậm qua lại (vòng lặp liền mạch).
"""
import bpy
import bmesh
import json
import math
import random
import sys

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
DUONG_DAN_JSON = argv[0]
THU_MUC_RA = argv[1]
SO_KHUNG = int(argv[2]) if len(argv) > 2 else 192
RONG = int(argv[3]) if len(argv) > 3 else 960
CAO = int(argv[4]) if len(argv) > 4 else 660

random.seed(20261013)


def hex_rgb(h, a=1.0):
    h = h.lstrip("#")
    c = [int(h[i:i + 2], 16) / 255 for i in (0, 2, 4)]
    # sRGB -> linear
    c = [x / 12.92 if x <= 0.04045 else ((x + 0.055) / 1.055) ** 2.4 for x in c]
    return (c[0], c[1], c[2], a)


# Màu tuyến – trùng Theme.MauTuyen (M01 = phần tử 0 …)
MAU_TUYEN = ["#2563EB", "#DC2626", "#16A34A", "#9333EA", "#EA580C", "#0891B2", "#DB2777", "#65A30D"]
MAU_TEAL = "#2DD4BF"

with open(DUONG_DAN_JSON, encoding="utf-8") as f:
    DL = json.load(f)

# ---------------------------------------------------------------- cảnh trống
bpy.ops.wm.read_factory_settings(use_empty=True)
sc = bpy.context.scene
sc.render.engine = "BLENDER_EEVEE_NEXT" if "BLENDER_EEVEE_NEXT" in [e.identifier for e in bpy.types.RenderSettings.bl_rna.properties["engine"].enum_items] else "BLENDER_EEVEE"
sc.render.resolution_x = RONG
sc.render.resolution_y = CAO
sc.render.resolution_percentage = 100
sc.frame_start = 1
sc.frame_end = SO_KHUNG
sc.render.fps = 24
sc.render.image_settings.file_format = "JPEG"
sc.render.image_settings.quality = 82
sc.render.filepath = THU_MUC_RA.rstrip("/\\") + "/k_"
sc.view_settings.view_transform = "AgX" if "AgX" in [v.identifier for v in sc.view_settings.bl_rna.properties["view_transform"].enum_items] else "Filmic"
sc.view_settings.look = "None"
try:
    sc.eevee.taa_render_samples = 24
except Exception:
    pass

# nền trời: xanh đêm
world = bpy.data.worlds.new("Troi")
sc.world = world
world.use_nodes = True
bg = world.node_tree.nodes["Background"]
bg.inputs[0].default_value = hex_rgb("#08131B")
bg.inputs[1].default_value = 1.0


def vat_lieu(ten, mau, phat_sang=0.0, mau_phat=None, nham=0.85, kim_loai=0.0):
    m = bpy.data.materials.new(ten)
    m.use_nodes = True
    p = m.node_tree.nodes["Principled BSDF"]
    p.inputs["Base Color"].default_value = hex_rgb(mau)
    p.inputs["Roughness"].default_value = nham
    p.inputs["Metallic"].default_value = kim_loai
    if phat_sang > 0:
        p.inputs["Emission Color"].default_value = hex_rgb(mau_phat or mau)
        p.inputs["Emission Strength"].default_value = phat_sang
    return m


VL_DAT = vat_lieu("Dat", "#0B1821", nham=0.95)
VL_NUOC = vat_lieu("Nuoc", "#0C3440", phat_sang=0.25, mau_phat="#0E7490", nham=0.15, kim_loai=0.2)
VL_DUONG = vat_lieu("Duong", "#16303B", phat_sang=0.9, mau_phat="#1E5A66", nham=0.6)
VL_TRAM = vat_lieu("Tram", MAU_TEAL, phat_sang=6.0, mau_phat=MAU_TEAL)

# toà nhà: 3 sắc độ để mặt phố không phẳng màu
VL_NHA = [vat_lieu("Nha%d" % i, c, nham=0.75) for i, c in enumerate(["#1A2733", "#202F3D", "#273848"])]
# đỉnh nhà sáng nhẹ (cửa sổ/đèn) – trộn bằng chính màu phát sáng thấp
for m, s in zip(VL_NHA, (0.015, 0.025, 0.04)):
    p = m.node_tree.nodes["Principled BSDF"]
    p.inputs["Emission Color"].default_value = hex_rgb("#A5F3FC")
    p.inputs["Emission Strength"].default_value = s


def lien_ket(obj):
    sc.collection.objects.link(obj)
    return obj


# ---------------------------------------------------------------- mặt đất
bm = bmesh.new()
bmesh.ops.create_circle(bm, cap_ends=True, radius=9000, segments=128)
me = bpy.data.meshes.new("Dat")
bm.to_mesh(me)
bm.free()
dat = lien_ket(bpy.data.objects.new("Dat", me))
dat.data.materials.append(VL_DAT)


# ---------------------------------------------------------------- polygon -> mesh
def vong_hop_le(vong):
    pts = [(p[0], p[1]) for p in vong]
    if len(pts) > 1 and pts[0] == pts[-1]:
        pts = pts[:-1]
    # bỏ đỉnh trùng liên tiếp
    sach = []
    for p in pts:
        if not sach or (abs(p[0] - sach[-1][0]) > 0.05 or abs(p[1] - sach[-1][1]) > 0.05):
            sach.append(p)
    return sach if len(sach) >= 3 else None


def them_khoi(bm, vong, z0, z1):
    """Đáy + tường + mái (n-gon) cho một vòng ngoài polygon."""
    pts = vong_hop_le(vong)
    if not pts:
        return
    # đảm bảo ngược chiều kim đồng hồ để pháp tuyến hướng ra ngoài
    dien_tich = sum(pts[i][0] * pts[(i + 1) % len(pts)][1] - pts[(i + 1) % len(pts)][0] * pts[i][1] for i in range(len(pts)))
    if dien_tich < 0:
        pts.reverse()
    duoi = [bm.verts.new((x, y, z0)) for x, y in pts]
    tren = [bm.verts.new((x, y, z1)) for x, y in pts]
    try:
        bm.faces.new(tren)
    except ValueError:
        pass
    if z1 - z0 > 0.01:
        n = len(pts)
        for i in range(n):
            j = (i + 1) % n
            try:
                bm.faces.new((duoi[i], duoi[j], tren[j], tren[i]))
            except ValueError:
                pass


def chieu_cao(gid, loai, dien_tich):
    r = random.Random(gid)
    loai = (loai or "").lower()
    if loai in ("apartments", "office", "commercial", "hotel"):
        h = r.uniform(28, 75)
    elif loai in ("hospital", "university", "school", "government", "public", "civic"):
        h = r.uniform(14, 30)
    elif loai in ("house", "terrace", "residential", "detached"):
        h = r.uniform(8, 16)
    else:
        h = r.uniform(7, 20)
    if dien_tich > 3000:
        h *= 1.25
    return h


def dien_tich_vong(v):
    return abs(sum(v[i][0] * v[(i + 1) % len(v)][1] - v[(i + 1) % len(v)][0] * v[i][1] for i in range(len(v)))) / 2


nhom_bm = [bmesh.new() for _ in VL_NHA]
for gid, loai, toa_do in DL.get("nha") or []:
    # MultiPolygon: [[[ring]]] ; Polygon: [[ring]]
    polys = toa_do if toa_do and isinstance(toa_do[0][0][0], list) else [toa_do]
    for poly in polys:
        if not poly:
            continue
        ngoai = poly[0]
        if len(ngoai) < 4:
            continue
        dt = dien_tich_vong(ngoai)
        if dt < 12:
            continue
        h = chieu_cao(gid, loai, dt)
        them_khoi(nhom_bm[gid % len(nhom_bm)], ngoai, 0.0, h)

for i, b in enumerate(nhom_bm):
    me = bpy.data.meshes.new("Nha%d" % i)
    b.to_mesh(me)
    b.free()
    o = lien_ket(bpy.data.objects.new("Nha%d" % i, me))
    o.data.materials.append(VL_NHA[i])

# ---------------------------------------------------------------- mặt nước (sông Hồng, Hồ Tây, Hồ Gươm…)
bmn = bmesh.new()
for toa_do in DL.get("nuoc") or []:
    if not toa_do:
        continue
    polys = toa_do if isinstance(toa_do[0][0][0], list) else [toa_do]
    for poly in polys:
        if poly:
            them_khoi(bmn, poly[0], 0.4, 0.4)
me = bpy.data.meshes.new("Nuoc")
bmn.to_mesh(me)
bmn.free()
o = lien_ket(bpy.data.objects.new("Nuoc", me))
o.data.materials.append(VL_NUOC)


# ---------------------------------------------------------------- đường, tuyến: curve có bề dày
def cac_duong(toa_do):
    """LineString [[x,y]...] hoặc MultiLineString [[[x,y]...]...] -> list các polyline."""
    if not toa_do:
        return []
    if isinstance(toa_do[0][0], list):
        return [l for l in toa_do if len(l) >= 2]
    return [toa_do] if len(toa_do) >= 2 else []


def tao_curve(ten, polylines, z, be_rong, vat):
    cu = bpy.data.curves.new(ten, "CURVE")
    cu.dimensions = "3D"
    cu.bevel_depth = be_rong
    cu.bevel_resolution = 2
    for pl in polylines:
        sp = cu.splines.new("POLY")
        sp.points.add(len(pl) - 1)
        for i, (x, y) in enumerate(pl):
            sp.points[i].co = (x, y, z, 1)
    o = lien_ket(bpy.data.objects.new(ten, cu))
    o.data.materials.append(vat)
    return o


tat_ca_duong = []
for loai, toa_do in DL.get("duong") or []:
    tat_ca_duong += cac_duong(toa_do)
tao_curve("Duong", tat_ca_duong, 0.6, 2.2, VL_DUONG)

xe_chay = []   # (đối tượng chấm xe, polyline dài nhất của tuyến, lệch pha)
for ma, toa_do in DL.get("tuyen") or []:
    so = int("".join(ch for ch in ma if ch.isdigit()) or "1")
    mau = MAU_TUYEN[(so - 1) % len(MAU_TUYEN)]
    vl = vat_lieu("Tuyen_" + ma, mau, phat_sang=9.0, mau_phat=mau, nham=0.4)
    pls = cac_duong(toa_do)
    tao_curve("Tuyen_" + ma, pls, 3.0, 7.0, vl)
    if pls:
        dai = max(pls, key=len)
        vl_xe = vat_lieu("Xe_" + ma, "#FFFFFF", phat_sang=20.0, mau_phat=mau)
        for k in range(3):
            bpy.ops.mesh.primitive_uv_sphere_add(radius=18, segments=16, ring_count=8)
            xe = bpy.context.active_object
            xe.data.materials.append(vl_xe)
            xe_chay.append((xe, dai, k / 3.0))

# ---------------------------------------------------------------- trạm: trụ nhỏ phát sáng, nhấp nháy
bmt = bmesh.new()
for x, y in DL.get("tram") or []:
    m = bmesh.ops.create_cone(bmt, cap_ends=True, segments=12, radius1=11, radius2=11, depth=10)
    bmesh.ops.translate(bmt, verts=m["verts"], vec=(x, y, 6))
me = bpy.data.meshes.new("Tram")
bmt.to_mesh(me)
bmt.free()
o = lien_ket(bpy.data.objects.new("Tram", me))
o.data.materials.append(VL_TRAM)

# nhấp nháy trạm: cường độ phát sáng theo sin, chu kỳ chia hết số khung (lặp liền)
p_tram = VL_TRAM.node_tree.nodes["Principled BSDF"].inputs["Emission Strength"]
for f in range(1, SO_KHUNG + 1, 4):
    p_tram.default_value = 4.0 + 3.0 * (0.5 + 0.5 * math.sin(2 * math.pi * 4 * (f - 1) / SO_KHUNG))
    p_tram.keyframe_insert("default_value", frame=f)


# ---------------------------------------------------------------- xe chạy dọc tuyến (vị trí tính sẵn từng khung)
def diem_theo_ti_le(pl, t):
    do_dai = [0.0]
    for i in range(1, len(pl)):
        do_dai.append(do_dai[-1] + math.dist(pl[i - 1], pl[i]))
    tong = do_dai[-1] or 1.0
    s = (t % 1.0) * tong
    for i in range(1, len(pl)):
        if do_dai[i] >= s:
            a = (s - do_dai[i - 1]) / max(1e-6, do_dai[i] - do_dai[i - 1])
            return (pl[i - 1][0] + a * (pl[i][0] - pl[i - 1][0]), pl[i - 1][1] + a * (pl[i][1] - pl[i - 1][1]))
    return pl[-1]


for xe, pl, lech in xe_chay:
    # đi hết tuyến đúng 1 lần mỗi vòng lặp; chiều đi/về xen kẽ để không "nhảy" cùng lúc
    for f in range(1, SO_KHUNG + 1, 2):
        t = lech + (f - 1) / SO_KHUNG
        x, y = diem_theo_ti_le(pl, t)
        xe.location = (x, y, 14)
        xe.keyframe_insert("location", frame=f)

# ---------------------------------------------------------------- ánh sáng
bpy.ops.object.light_add(type="SUN", rotation=(math.radians(58), 0, math.radians(-35)))
mt = bpy.context.active_object
mt.data.energy = 1.6
mt.data.color = (0.75, 0.85, 1.0)
bpy.ops.object.light_add(type="SUN", rotation=(math.radians(70), 0, math.radians(150)))
mt2 = bpy.context.active_object
mt2.data.energy = 0.5
mt2.data.color = (1.0, 0.8, 0.6)

# ---------------------------------------------------------------- camera lượn qua lại (vòng lặp liền)
bpy.ops.object.empty_add(location=(150, -100, 0))
dich = bpy.context.active_object
bpy.ops.object.camera_add()
cam = bpy.context.active_object
sc.camera = cam
cam.data.lens = 32
cam.data.clip_end = 20000
rang = cam.constraints.new("TRACK_TO")
rang.target = dich
rang.track_axis = "TRACK_NEGATIVE_Z"
rang.up_axis = "UP_Y"
BAN_KINH, DO_CAO = 2500, 2050
for f in range(1, SO_KHUNG + 1, 2):
    pha = 2 * math.pi * (f - 1) / SO_KHUNG
    goc = math.radians(-60 + 16 * math.sin(pha))
    r = BAN_KINH + 180 * math.cos(pha)
    cam.location = (r * math.cos(goc), r * math.sin(goc), DO_CAO + 120 * math.sin(pha))
    cam.keyframe_insert("location", frame=f)

for o in [cam] + [x[0] for x in xe_chay]:
    if o.animation_data and o.animation_data.action:
        for fc in o.animation_data.action.fcurves:
            for kp in fc.keyframe_points:
                kp.interpolation = "LINEAR"

# ---------------------------------------------------------------- hậu kỳ: glow + mờ nhẹ + sương xa
sc.use_nodes = True
cay = sc.node_tree
for n in list(cay.nodes):
    cay.nodes.remove(n)
vao = cay.nodes.new("CompositorNodeRLayers")
glow = cay.nodes.new("CompositorNodeGlare")
glow.glare_type = "FOG_GLOW"
try:
    glow.quality = "HIGH"
    glow.size = 8
    glow.threshold = 0.6
    glow.mix = 0.15
except Exception:
    pass
mo = cay.nodes.new("CompositorNodeBlur")
mo.filter_type = "GAUSS"
mo.size_x = max(1, RONG // 320)
mo.size_y = max(1, RONG // 320)
ra = cay.nodes.new("CompositorNodeComposite")
cay.links.new(vao.outputs["Image"], glow.inputs["Image"])
cay.links.new(glow.outputs["Image"], mo.inputs["Image"])
cay.links.new(mo.outputs["Image"], ra.inputs["Image"])

if len(argv) > 5 and argv[5] == "thu":
    # chỉ render 1 khung để xem thử bố cục
    sc.frame_set(1)
    sc.render.filepath = THU_MUC_RA.rstrip("/\\") + "/thu.jpg"
    bpy.ops.render.render(write_still=True)
else:
    bpy.ops.render.render(animation=True)
print("XONG", SO_KHUNG, "khung ->", THU_MUC_RA)
