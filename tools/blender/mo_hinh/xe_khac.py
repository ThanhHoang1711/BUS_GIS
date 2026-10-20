"""Xe con + xe máy có người lái (mô hình gọn, dùng làm dòng xe nền). Trục: chạy theo +X, gốc ở mặt đất."""
import math
import bmesh
import bpy
from mathutils import Matrix
from . import vat_lieu as vl

MAU_XE_CON = ["#F2F2F0", "#1C1E21", "#9DA3A8", "#8B1E1E", "#2B4C7E", "#E8E4DA", "#4A4F55", "#C7C9CB"]
MAU_AO = ["#2F3A4A", "#7A2E2E", "#3F5B3A", "#D9D4C7", "#1F2937", "#5B4A7A", "#B5651D"]
MAU_MU = ["#E53935", "#FBC02D", "#FFFFFF", "#1E88E5", "#212121", "#43A047"]


def _hop(bm, x0, x1, y0, y1, z0, z1, mat, vat=0.0):
    m = bmesh.ops.create_cube(bm, size=1.0)
    bmesh.ops.transform(bm, verts=m["verts"], matrix=Matrix.Translation(((x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2)) @
                        Matrix.Diagonal((x1 - x0, y1 - y0, z1 - z0, 1)))
    for v in m["verts"]:
        for f in v.link_faces:
            f.material_index = mat
    if vat > 0:   # gán vật liệu TRƯỚC khi vát (bevel xoá đỉnh cũ; mặt mới lấy vật liệu mặt kề)
        bmesh.ops.bevel(bm, geom=list({e for v in m["verts"] for e in v.link_edges}), offset=vat, segments=2, affect="EDGES")


def _banh(bm, x, y, r, w, mat):
    m = bmesh.ops.create_cone(bm, cap_ends=True, segments=16, radius1=r, radius2=r, depth=w)
    bmesh.ops.transform(bm, verts=m["verts"], matrix=Matrix.Translation((x, y, r)) @ Matrix.Rotation(math.pi / 2, 4, "X"))
    for v in m["verts"]:
        for f in v.link_faces:
            f.material_index = mat


def _obj(ten, bm, mats):
    me = bpy.data.meshes.new(ten)
    bm.to_mesh(me)
    bm.free()
    for m in mats:
        me.materials.append(m)
    o = bpy.data.objects.new(ten, me)
    return o


# biên dạng thân xe (x dọc xe, z cao) – điểm đầu là mũi xe dưới, đi ngược chiều kim đồng hồ khi nhìn từ bên phải
BIEN_DANG = {
    "sedan": [(2.3, 0.32), (2.34, 0.62), (2.2, 0.78), (1.0, 0.92), (0.15, 1.38), (-0.9, 1.4), (-1.65, 1.0),
              (-2.25, 0.92), (-2.32, 0.6), (-2.28, 0.32)],
    "suv": [(2.25, 0.38), (2.3, 0.72), (2.12, 0.92), (1.2, 1.02), (0.55, 1.62), (-1.95, 1.66), (-2.2, 1.3),
            (-2.25, 0.72), (-2.2, 0.38)],
}
# cạnh kính (cạnh i → i+1 của biên dạng): kính chắn gió = 3, kính hậu = 5; điểm 3/4/5/6 = chân kính trước / nóc trước /
# nóc sau / chân kính sau
CANH_KINH = {"sedan": {3, 5}, "suv": {3, 5}}


def xe_con(ten, mau, kieu="sedan"):
    mats = [vl.don_gian("Son_" + mau, mau, nham=0.28, phu=1.0), vl.kinh("XeKinh"), vl.don_gian("XeLop", "#141414", nham=0.85),
            vl.don_gian("XeHau", "#B3121B", nham=0.2, phat=2.0, mau_phat="#FF2A2A"),
            vl.don_gian("XePhaNho", "#F4F4F4", nham=0.1, phat=1.5, mau_phat="#FFF6E5"),
            vl.don_gian("XeNhuaDen", "#1A1B1D", nham=0.6)]
    bd = BIEN_DANG[kieu]
    W2 = 0.88
    bm = bmesh.new()
    trai = [bm.verts.new((x, -W2, z)) for x, z in bd]
    phai = [bm.verts.new((x, W2, z)) for x, z in bd]
    n = len(bd)
    kinh_canh = CANH_KINH[kieu]
    bm.faces.new(trai[::-1]).material_index = 0
    bm.faces.new(phai).material_index = 0
    for i in range(n):
        j = (i + 1) % n
        f = bm.faces.new((trai[i], trai[j], phai[j], phai[i]))
        f.material_index = 1 if i in kinh_canh else 0
    # thu hẹp nóc (cabin hẹp hơn thân): các đỉnh z > 0.95 co vào 12 cm
    for v in bm.verts:
        if v.co.z > 0.97:
            v.co.y *= (W2 - 0.12) / W2
    bmesh.ops.bevel(bm, geom=list(bm.edges), offset=0.06, segments=2, affect="EDGES", clamp_overlap=True)
    # kính hông: hình thang nghiêng theo thành cabin (dưới rộng W2, trên co 12 cm), lệch ra ngoài 1,2 cm
    zb = max(bd[3][1], bd[6][1]) + 0.04
    zt = min(bd[4][1], bd[5][1]) - 0.06
    xa, xb = bd[3][0] - 0.12, bd[6][0] + 0.12
    xa_t, xb_t = bd[4][0] - 0.08, bd[5][0] + 0.08
    for s in (-1, 1):
        yb, yt = s * (W2 + 0.012), s * (W2 - 0.12 + 0.012)
        q = [(xa, yb, zb), (xb, yb, zb), (xb_t, yt, zt), (xa_t, yt, zt)]
        f = bm.faces.new([bm.verts.new(p) for p in (q if s > 0 else q[::-1])])
        f.material_index = 1
    # đèn
    for s in (-1, 1):
        for x, z0, z1, mi in ((bd[0][0] + 0.05, 0.6, 0.72, 4), (bd[-1][0] - 0.05, 0.62, 0.78, 3)):
            q = [(x, s * 0.55 - 0.18, z0), (x, s * 0.55 + 0.18, z0), (x, s * 0.55 + 0.18, z1), (x, s * 0.55 - 0.18, z1)]
            bm.faces.new([bm.verts.new(p) for p in q]).material_index = mi
    for x in (-1.45, 1.42):
        for s in (-1, 1):
            _banh(bm, x, s * 0.8, 0.33, 0.24, 2)
    return _obj(ten, bm, mats)


def xe_may(ten, mau_xe, mau_ao, mau_mu, hai_nguoi=False):
    mats = [vl.don_gian("Son_" + mau_xe, mau_xe, nham=0.35, phu=0.6), vl.don_gian("XeLop", "#141414", nham=0.85),
            vl.don_gian("Ao_" + mau_ao, mau_ao, nham=0.9), vl.don_gian("Mu_" + mau_mu, mau_mu, nham=0.35),
            vl.don_gian("Da", "#C69C7A", nham=0.7)]
    bm = bmesh.new()
    _banh(bm, -0.68, 0, 0.3, 0.1, 1)
    _banh(bm, 0.68, 0, 0.3, 0.1, 1)
    _hop(bm, -0.75, 0.55, -0.18, 0.18, 0.32, 0.72, 0, 0.06)     # thân xe
    _hop(bm, 0.45, 0.62, -0.32, 0.32, 0.95, 1.0, 1)             # ghi đông
    _hop(bm, 0.5, 0.62, -0.06, 0.06, 0.4, 1.0, 0)               # cổ lái
    for k, dx in enumerate([0.0] + ([-0.5] if hai_nguoi else [])):
        _hop(bm, -0.25 + dx, 0.15 + dx, -0.2, 0.2, 0.72, 1.4, 2, 0.06)    # thân người
        _hop(bm, -0.1 + dx, 0.45 + dx, -0.22, 0.22, 0.62, 0.8, 2, 0.04)   # đùi
        _hop(bm, -0.18 + dx, 0.08 + dx, -0.12, 0.12, 1.4, 1.5, 4)         # cổ
        m = bmesh.ops.create_uvsphere(bm, u_segments=12, v_segments=8, radius=0.15)
        bmesh.ops.translate(bm, verts=m["verts"], vec=(-0.05 + dx, 0, 1.6))
        for v in m["verts"]:
            for f in v.link_faces:
                f.material_index = 3
        if k == 0:
            _hop(bm, 0.05, 0.5, -0.27, -0.17, 1.05, 1.28, 2)               # tay
            _hop(bm, 0.05, 0.5, 0.17, 0.27, 1.05, 1.28, 2)
    return _obj(ten, bm, mats)


def bo_xe_nen():
    """Collection ẩn: 5 xe con + 8 xe máy (tỉ lệ xe máy cao như phố Hà Nội)."""
    col = bpy.data.collections.new("XeNen")
    for i in range(5):
        col.objects.link(xe_con("XeCon%d" % i, MAU_XE_CON[(i * 3) % len(MAU_XE_CON)], "suv" if i % 2 else "sedan"))
    for i in range(8):
        col.objects.link(xe_may("XeMay%d" % i, MAU_XE_CON[(i * 5 + 1) % len(MAU_XE_CON)], MAU_AO[i % len(MAU_AO)],
                                MAU_MU[(i * 2) % len(MAU_MU)], hai_nguoi=(i % 3 == 0)))
    return col
