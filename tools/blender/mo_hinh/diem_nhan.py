"""
Điểm nhấn Hồ Gươm (mô hình đơn giản, đủ nhận ra từ trên cao): Tháp Rùa trên đảo nhỏ, cụm đền Ngọc Sơn trên đảo lớn,
cầu Thê Húc đỏ cong nối đảo với bờ đông. Toạ độ (m, gốc = tâm hồ) lấy từ vòng trong mặt nước Hồ Hoàn Kiếm (CSDLKG).
"""
import math
import bmesh
import bpy
from mathutils import Matrix
from . import vat_lieu as vl

THAP_RUA = (-10.9, -92.6)
DAO_NGOC_SON = (-1.2, 220.9)
CAU = ((31.8, 214.5), (58.3, 218.8))     # đầu đảo → bờ
Z_DAT = 0.12


def _hop(bm, cx, cy, z0, rx, ry, cao, mat, xoay=0.0):
    m = bmesh.ops.create_cube(bm, size=1.0)
    bmesh.ops.transform(bm, verts=m["verts"],
                        matrix=Matrix.Translation((cx, cy, z0 + cao / 2)) @ Matrix.Rotation(xoay, 4, "Z") @ Matrix.Diagonal((2 * rx, 2 * ry, cao, 1)))
    for v in m["verts"]:
        for f in v.link_faces:
            f.material_index = mat


def _mai(bm, cx, cy, z0, rx, ry, cao, mat, xoay=0.0, nhon=0.35):
    """Mái dốc 4 phía, đầu đao hơi cong lên (rút gọn): đáy chữ nhật → đỉnh là đường nóc."""
    c, s = math.cos(xoay), math.sin(xoay)

    def p(x, y, z):
        return bm.verts.new((cx + x * c - y * s, cy + x * s + y * c, z))
    a = [p(-rx, -ry, z0), p(rx, -ry, z0), p(rx, ry, z0), p(-rx, ry, z0)]
    nr = rx * nhon
    b = [p(-nr, 0, z0 + cao), p(nr, 0, z0 + cao)]
    # đầu đao: góc mái nhô lên chút
    for v in a:
        v.co.z += 0.25
    for q in [(a[0], a[1], b[1], b[0]), (a[1], a[2], b[1]), (a[2], a[3], b[0], b[1]), (a[3], a[0], b[0])]:
        bm.faces.new(q).material_index = mat
    bm.faces.new((a[3], a[2], a[1], a[0])).material_index = mat


def _obj(ten, bm, mats):
    me = bpy.data.meshes.new(ten)
    bm.to_mesh(me)
    bm.free()
    for m in mats:
        me.materials.append(m)
    o = bpy.data.objects.new(ten, me)
    bpy.context.scene.collection.objects.link(o)
    return o


def thap_rua():
    m_da = vl.don_gian("DaThapRua", "#B8A27A", nham=0.85)
    m_toi = vl.don_gian("CuaThap", "#2F2A22", nham=0.9)
    m_mai = vl.don_gian("MaiThap", "#5C4A3A", nham=0.8)
    bm = bmesh.new()
    x, y = THAP_RUA
    z = Z_DAT
    for rx, ry, cao in ((4.0, 3.2, 4.4), (3.1, 2.5, 3.2), (2.3, 1.8, 2.8)):
        _hop(bm, x, y, z, rx, ry, cao, 0)
        # cửa vòm (mảng tối) mỗi mặt
        for sx in (-1, 1):
            _hop(bm, x + sx * (rx + 0.01), y, z + cao * 0.18, 0.02, ry * 0.35, cao * 0.6, 1)
            _hop(bm, x, y + sx * (ry + 0.01), z + cao * 0.18, rx * 0.3, 0.02, cao * 0.6, 1)
        _hop(bm, x, y, z + cao, rx + 0.25, ry + 0.25, 0.25, 2)    # gờ mái tầng
        z += cao + 0.25
    _mai(bm, x, y, z, 2.0, 1.6, 1.4, 2, nhon=0.1)
    return _obj("ThapRua", bm, [m_da, m_toi, m_mai])


def den_ngoc_son():
    m_tuong = vl.don_gian("TuongDen", "#C9A15A", nham=0.8)
    m_cot = vl.don_gian("CotDen", "#8E2B22", nham=0.6)
    m_mai = vl.don_gian("MaiDen", "#5E3326", nham=0.75)
    bm = bmesh.new()
    x, y = DAO_NGOC_SON
    xoay = math.radians(8)
    for dx, dy, rx, ry, cao, mh in ((0, 4, 7.5, 5.0, 4.2, 3.2), (0, -9, 6.0, 4.0, 3.8, 2.8), (12, -2, 3.0, 3.0, 5.5, 2.6)):
        _hop(bm, x + dx, y + dy, Z_DAT, rx, ry, cao, 0, xoay)
        _mai(bm, x + dx, y + dy, Z_DAT + cao, rx + 1.2, ry + 1.2, mh, 2, xoay)
        for sx in (-1, 1):
            _hop(bm, x + dx + sx * (rx + 0.4), y + dy - ry - 0.4, Z_DAT, 0.22, 0.22, cao, 1, xoay)
    return _obj("DenNgocSon", bm, [m_tuong, m_cot, m_mai])


def cau_the_huc():
    """Cầu gỗ sơn đỏ cong vòm, có lan can + cột."""
    m_do = vl.don_gian("CauDo", "#C2262E", nham=0.45, phu=0.4)
    m_go = vl.don_gian("SanCau", "#7A2A22", nham=0.7)
    (x0, y0), (x1, y1) = CAU
    dx, dy = x1 - x0, y1 - y0
    L = math.hypot(dx, dy)
    tx, ty = dx / L, dy / L
    # kéo dài vào đảo và lên bờ 3 m
    x0, y0, x1, y1 = x0 - tx * 3, y0 - ty * 3, x1 + tx * 3, y1 + ty * 3
    L += 6
    nx, ny = -ty, tx
    W = 1.7
    N = 28
    bm = bmesh.new()
    truoc = None
    for i in range(N + 1):
        t = i / N
        px, py = x0 + tx * L * t, y0 + ty * L * t
        z = Z_DAT + 0.3 + 2.4 * math.sin(math.pi * t)
        v = [bm.verts.new((px + nx * W, py + ny * W, z)), bm.verts.new((px - nx * W, py - ny * W, z)),
             bm.verts.new((px + nx * W, py + ny * W, z - 0.35)), bm.verts.new((px - nx * W, py - ny * W, z - 0.35))]
        if truoc:
            bm.faces.new((truoc[0], v[0], v[1], truoc[1])).material_index = 1
            bm.faces.new((truoc[2], truoc[3], v[3], v[2])).material_index = 0
            bm.faces.new((truoc[0], truoc[2], v[2], v[0])).material_index = 0
            bm.faces.new((truoc[1], v[1], v[3], truoc[3])).material_index = 0
        truoc = v
        # cột lan can + tay vịn
        if i % 2 == 0:
            for s in (1, -1):
                _hop(bm, px + nx * W * s, py + ny * W * s, z, 0.07, 0.07, 1.0, 0)
        if i < N:
            t2 = (i + 1) / N
            z2 = Z_DAT + 0.3 + 2.4 * math.sin(math.pi * t2)
            for s in (1, -1):
                a = (px + nx * W * s, py + ny * W * s)
                b = (x0 + tx * L * t2 + nx * W * s, y0 + ty * L * t2 + ny * W * s)
                q = [bm.verts.new((a[0], a[1], z + 0.92)), bm.verts.new((b[0], b[1], z2 + 0.92)),
                     bm.verts.new((b[0], b[1], z2 + 1.02)), bm.verts.new((a[0], a[1], z + 1.02))]
                bm.faces.new(q).material_index = 0
    # trụ đỡ
    for t in (0.25, 0.5, 0.75):
        px, py = x0 + tx * L * t, y0 + ty * L * t
        z = Z_DAT + 0.3 + 2.4 * math.sin(math.pi * t) - 0.35
        _hop(bm, px, py, -0.6, 0.25, W * 0.8, z + 0.6, 0, math.atan2(ty, tx))
    return _obj("CauTheHuc", bm, [m_do, m_go])


def dung_diem_nhan():
    thap_rua()
    den_ngoc_son()
    cau_the_huc()
