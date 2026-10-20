"""
Xe buýt đô thị ~11,5 m (kiểu xe Hà Nội): thân bo tròn sơn trắng + dải màu tuyến, dải kính đen có trụ ngăn, 2 cửa
bên phải, bánh + lazăng + hốc bánh, đèn pha/đèn hậu, bảng LED số tuyến + điểm đến, cục điều hoà trên nóc.
Trục xe: chạy theo +X, gốc ở giữa xe trên mặt đất. Trả về (gốc_empty, [4 bánh]) – bánh quay quanh trục Y riêng.
"""
import math
import bmesh
import bpy
from . import vat_lieu as vl

DAI, RONG, CAO, GAM = 11.5, 2.5, 3.15, 0.32
FONT = "C:/Windows/Fonts/seguisb.ttf"


def _hop(bm, x0, x1, y0, y1, z0, z1):
    v = [bm.verts.new(p) for p in [(x0, y0, z0), (x1, y0, z0), (x1, y1, z0), (x0, y1, z0),
                                     (x0, y0, z1), (x1, y0, z1), (x1, y1, z1), (x0, y1, z1)]]
    f = [(0, 3, 2, 1), (4, 5, 6, 7), (0, 1, 5, 4), (1, 2, 6, 5), (2, 3, 7, 6), (3, 0, 4, 7)]
    return [bm.faces.new([v[i] for i in q]) for q in f]


def _mat_phang(bm, pts, mat_idx):
    f = bm.faces.new([bm.verts.new(p) for p in pts])
    f.material_index = mat_idx
    return f


def _vat_lieu_xe(mau_tuyen):
    return [
        vl.don_gian("XeSonTrang", "#F3F4F1", nham=0.28, phu=1.0),          # 0 thân
        vl.don_gian("XeSon_" + mau_tuyen, mau_tuyen, nham=0.3, phu=1.0),     # 1 dải màu tuyến
        vl.kinh("XeKinh"),                                                   # 2 kính
        vl.don_gian("XeDen", "#15171A", nham=0.45),                          # 3 nhựa đen (cản, hốc bánh, trụ)
        vl.don_gian("XePha", "#FFFFFF", nham=0.1, phat=6.0, mau_phat="#FFF4DE"),   # 4 đèn pha
        vl.don_gian("XeHau", "#B3121B", nham=0.2, phat=3.0, mau_phat="#FF2A2A"),   # 5 đèn hậu
        vl.don_gian("XeLed", "#FFB000", nham=0.3, phat=9.0, mau_phat="#FFB000"),   # 6 chữ LED
        vl.don_gian("XeXam", "#BFC3C6", nham=0.4),                           # 7 điều hoà / lưới
    ]


def _chu(text, co, x, y, z, xoay, mat):
    """Chữ (mesh) đặt tại (x,y,z), xoay (rx, ry, rz) độ."""
    cu = bpy.data.curves.new("chu", "FONT")
    cu.body = text
    try:
        cu.font = bpy.data.fonts.load(FONT, check_existing=True)
    except Exception:
        pass
    cu.size = co
    cu.align_x = "CENTER"
    cu.align_y = "CENTER"
    cu.extrude = 0.004
    o = bpy.data.objects.new("chu", cu)
    bpy.context.scene.collection.objects.link(o)
    o.location = (x, y, z)
    o.rotation_euler = tuple(math.radians(a) for a in xoay)
    o.data.materials.append(mat)
    return o


def tao_xe_buyt(ten, mau_tuyen="#DC2626", so_tuyen="02", diem_den="Tràng Thi", bang_ten=True):
    mats = _vat_lieu_xe(mau_tuyen)
    bm = bmesh.new()
    L2, W2 = DAI / 2, RONG / 2

    # --- thân: hộp bo cạnh
    than = _hop(bm, -L2, L2, -W2, W2, GAM, CAO)
    bmesh.ops.bevel(bm, geom=list({e for f in than for e in f.edges}), offset=0.16, segments=4, affect="EDGES", profile=0.6)
    # nóc hơi vồng: nâng các đỉnh trên nóc ở giữa
    for v in bm.verts:
        if v.co.z > CAO - 0.02 and abs(v.co.y) < W2 - 0.2:
            v.co.z += 0.04

    e = 0.012   # lệch ra ngoài thân để tránh trùng mặt

    # --- dải màu tuyến: thân dưới + chỉ mảnh trên cửa sổ (hai bên + đầu + đuôi)
    for s in (-1, 1):
        y = s * (W2 + e)
        _mat_phang(bm, [(-L2 + 0.25, y, GAM + 0.12), (L2 - 0.25, y, GAM + 0.12), (L2 - 0.25, y, 1.12), (-L2 + 0.25, y, 1.12)][::(-s)], 1)
        _mat_phang(bm, [(-L2 + 0.25, y, 2.92), (L2 - 0.25, y, 2.92), (L2 - 0.25, y, 3.0), (-L2 + 0.25, y, 3.0)][::(-s)], 1)
    for s in (-1, 1):
        x = s * (L2 + e)
        _mat_phang(bm, [(x, -W2 + 0.25, GAM + 0.12), (x, W2 - 0.25, GAM + 0.12), (x, W2 - 0.25, 0.95), (x, -W2 + 0.25, 0.95)][::s], 1)

    # --- kính hai bên (y = ±): dải kính + trụ ngăn
    x_dau, x_cuoi = -L2 + 0.55, L2 - 0.55
    for s in (-1, 1):
        y = s * (W2 + e)
        _mat_phang(bm, [(x_dau, y, 1.42), (x_cuoi, y, 1.42), (x_cuoi, y, 2.78), (x_dau, y, 2.78)][::(-s)], 2)
        y2 = s * (W2 + 2 * e)
        x = x_dau + 1.55
        while x < x_cuoi - 0.4:
            _mat_phang(bm, [(x - 0.05, y2, 1.42), (x + 0.05, y2, 1.42), (x + 0.05, y2, 2.78), (x - 0.05, y2, 2.78)][::(-s)], 3)
            x += 1.55
    # cửa bên phải (-y): cửa trước (đầu xe) + cửa giữa – kính cao tới sàn, khung đen
    y = -(W2 + 3 * e)
    for xc in (L2 - 1.25, -0.6):
        _mat_phang(bm, [(xc - 0.62, y, GAM + 0.08), (xc + 0.62, y, GAM + 0.08), (xc + 0.62, y, 2.82), (xc - 0.62, y, 2.82)][::-1], 3)
        y3 = -(W2 + 4 * e)
        for dx in (-0.31, 0.31):
            _mat_phang(bm, [(xc + dx - 0.26, y3, GAM + 0.25), (xc + dx + 0.26, y3, GAM + 0.25), (xc + dx + 0.26, y3, 2.7), (xc + dx - 0.26, y3, 2.7)][::-1], 2)

    # --- kính chắn gió (đầu xe +X) + kính sau
    x = L2 + e
    _mat_phang(bm, [(x, -W2 + 0.18, 1.08), (x, W2 - 0.18, 1.08), (x, W2 - 0.18, 2.72), (x, -W2 + 0.18, 2.72)], 2)
    x = -(L2 + e)
    _mat_phang(bm, [(x, -W2 + 0.3, 1.75), (x, W2 - 0.3, 1.75), (x, W2 - 0.3, 2.65), (x, -W2 + 0.3, 2.65)][::-1], 2)
    # bảng LED điểm đến (đầu xe, trên kính) – nền đen
    x = L2 + 2 * e
    _mat_phang(bm, [(x, -W2 + 0.3, 2.76), (x, W2 - 0.3, 2.76), (x, W2 - 0.3, 3.02), (x, -W2 + 0.3, 3.02)], 3)

    # --- đèn pha (đầu) + đèn hậu (đuôi) + cản
    x = L2 + 2 * e
    for s in (-1, 1):
        y0, y1 = s * (W2 - 0.18), s * (W2 - 0.62)
        _mat_phang(bm, [(x, min(y0, y1), 0.62), (x, max(y0, y1), 0.62), (x, max(y0, y1), 0.78), (x, min(y0, y1), 0.78)], 4)
    x = -(L2 + 2 * e)
    for s in (-1, 1):
        y0, y1 = s * (W2 - 0.12), s * (W2 - 0.32)
        _mat_phang(bm, [(x, min(y0, y1), 0.55), (x, max(y0, y1), 0.55), (x, max(y0, y1), 1.25), (x, min(y0, y1), 1.25)][::-1], 5)
    for x0, x1 in ((L2 - 0.12, L2 + 0.06), (-L2 - 0.06, -L2 + 0.12)):   # cản trước, cản sau
        for q in _hop(bm, x0, x1, -W2 + 0.05, W2 - 0.05, GAM + 0.0, GAM + 0.2):
            q.material_index = 3

    # --- điều hoà trên nóc + gương chiếu hậu (tay đòn + mặt gương ở hai góc đầu xe)
    TRUC = (3.55, -2.75)
    for q in _hop(bm, -1.8, 1.4, -0.9, 0.9, CAO + 0.02, CAO + 0.34):
        q.material_index = 7
    for s in (-1, 1):
        for q in _hop(bm, L2 - 0.05, L2 + 0.32, s * W2 - 0.03, s * W2 + 0.03, 2.62, 2.68) +                  _hop(bm, L2 + 0.26, L2 + 0.36, s * (W2 + 0.12) - 0.13, s * (W2 + 0.12) + 0.13, 2.05, 2.62):
            q.material_index = 3

    me = bpy.data.meshes.new(ten)
    bm.to_mesh(me)
    bm.free()
    for m in mats:
        me.materials.append(m)
    for p in me.polygons:
        p.use_smooth = False
    than_obj = bpy.data.objects.new(ten, me)
    bpy.context.scene.collection.objects.link(than_obj)

    # --- khoét hốc bánh (boolean Exact, mặt cắt nhận vật liệu nhựa đen của dao cắt ⇒ thành vè chắn bùn)
    m_lot = vl.don_gian("XeHocBanh", "#0E0F11", nham=0.7)
    dao = []
    for xb in TRUC:
        bpy.ops.mesh.primitive_cylinder_add(vertices=40, radius=0.64, depth=RONG + 0.6, location=(xb, 0, 0.5), rotation=(math.pi / 2, 0, 0))
        d = bpy.context.active_object
        d.data.materials.append(m_lot)
        dao.append(d)
    bpy.ops.object.select_all(action="DESELECT")
    for d in dao[1:]:
        d.select_set(True)
    dao[0].select_set(True)
    bpy.context.view_layer.objects.active = dao[0]
    bpy.ops.object.join()
    cat = dao[0]
    md = than_obj.modifiers.new("hoc_banh", "BOOLEAN")
    md.operation = "DIFFERENCE"
    md.solver = "EXACT"
    md.object = cat
    try:
        md.material_mode = "TRANSFER"
    except Exception:
        pass
    bpy.context.view_layer.objects.active = than_obj
    bpy.ops.object.modifier_apply(modifier=md.name)
    bpy.data.objects.remove(cat, do_unlink=True)
    # lòng hốc: tấm chắn tối bên trong (che phần rỗng của thân)
    bm2 = bmesh.new()
    for xb in TRUC:
        _hop(bm2, xb - 0.66, xb + 0.66, -W2 + 0.42, W2 - 0.42, 0.42, 1.16)
    me2 = bpy.data.meshes.new(ten + "_lot")
    bm2.to_mesh(me2)
    bm2.free()
    me2.materials.append(m_lot)
    lot = bpy.data.objects.new(ten + "_lot", me2)
    bpy.context.scene.collection.objects.link(lot)
    bpy.ops.object.select_all(action="DESELECT")
    lot.select_set(True)
    than_obj.select_set(True)
    bpy.context.view_layer.objects.active = than_obj
    bpy.ops.object.join()

    # --- chữ LED: số tuyến + điểm đến (đầu xe), số tuyến bên hông
    phu = []
    if bang_ten:
        phu.append(_chu("%s  %s" % (so_tuyen, diem_den), 0.17, L2 + 0.03, 0, 2.89, (90, 0, 90), mats[6]))
        phu.append(_chu(so_tuyen, 0.32, -L2 + 1.2, -(W2 + 0.03), 2.25, (90, 0, 0), mats[6]))
        phu.append(_chu("XeBus", 0.42, -2.0, W2 + 0.02, 0.78, (90, 0, 180), vl.don_gian("XeChuTrang", "#FFFFFF", nham=0.3)))
        phu.append(_chu("XeBus", 0.42, 1.6, -(W2 + 0.02), 0.78, (90, 0, 0), vl.don_gian("XeChuTrang", "#FFFFFF", nham=0.3)))
    # gộp chữ vào thân
    if phu:
        bpy.ops.object.select_all(action="DESELECT")
        for o in phu:
            o.select_set(True)
        bpy.context.view_layer.objects.active = phu[0]
        bpy.ops.object.convert(target="MESH")
        for o in phu:
            o.select_set(True)
        than_obj.select_set(True)
        bpy.context.view_layer.objects.active = than_obj
        bpy.ops.object.join()

    # --- bánh xe: lốp + lazăng (object riêng để quay)
    banh = []
    for xb in TRUC:
        for s in (-1, 1):
            bpy.ops.mesh.primitive_cylinder_add(vertices=28, radius=0.5, depth=0.3, location=(xb, s * (W2 - 0.2), 0.5), rotation=(math.pi / 2, 0, 0))
            b = bpy.context.active_object
            b.name = ten + "_banh"
            b.data.materials.append(vl.don_gian("XeLop", "#141414", nham=0.85))
            b.data.materials.append(vl.don_gian("XeLaZang", "#A9AFB4", nham=0.3, kim_loai=1.0))
            # mặt ngoài = lazăng
            for p in b.data.polygons:
                if abs(p.normal.z) > 0.9 and p.normal.z * (-s) > 0:   # trục trụ = Z cục bộ, xoay 90° quanh X ⇒ +Z cục bộ = −Y thế giới
                    p.material_index = 1
            bpy.ops.object.shade_smooth()
            banh.append(b)

    goc = bpy.data.objects.new(ten + "_goc", None)
    bpy.context.scene.collection.objects.link(goc)
    than_obj.parent = goc
    for b in banh:
        b.parent = goc
    return goc, than_obj, banh
