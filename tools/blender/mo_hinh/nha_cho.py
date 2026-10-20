"""
Nhà chờ xe buýt: bệ bê tông, 4 cột thép, mái tối, băng biển tên trạm phía trước (TRẠM XE BUÝT · tên trạm + số tuyến),
vách kính sau + một bên, hộp đèn thông tin hệ thống XeBus, ghế, cột biển báo trạm.
Trục: dài theo X, mặt trước (phía lòng đường) hướng −Y, gốc ở giữa bệ trên mặt vỉa hè.
"""
import math
import bmesh
import bpy
from . import vat_lieu as vl

FONT = "C:/Windows/Fonts/seguisb.ttf"
FONT_DAM = "C:/Windows/Fonts/segoeuib.ttf"


def _hop(bm, x0, x1, y0, y1, z0, z1, mat):
    v = [bm.verts.new(p) for p in [(x0, y0, z0), (x1, y0, z0), (x1, y1, z0), (x0, y1, z0),
                                     (x0, y0, z1), (x1, y0, z1), (x1, y1, z1), (x0, y1, z1)]]
    for q in [(0, 3, 2, 1), (4, 5, 6, 7), (0, 1, 5, 4), (1, 2, 6, 5), (2, 3, 7, 6), (3, 0, 4, 7)]:
        bm.faces.new([v[i] for i in q]).material_index = mat


def _chu(text, co, vi_tri, xoay, mat, font=FONT, can="CENTER"):
    cu = bpy.data.curves.new("chu", "FONT")
    cu.body = text
    try:
        cu.font = bpy.data.fonts.load(font, check_existing=True)
    except Exception:
        pass
    cu.size = co
    cu.align_x = can
    cu.align_y = "CENTER"
    cu.extrude = 0.003
    o = bpy.data.objects.new("chu", cu)
    bpy.context.scene.collection.objects.link(o)
    o.location = vi_tri
    o.rotation_euler = tuple(math.radians(a) for a in xoay)
    o.data.materials.append(mat)
    return o


def tao_nha_cho(ten, ten_tram="6-8 Tràng Thi", cac_tuyen=("02", "03", "04")):
    m_be = vl.be_tong()
    m_thep = vl.don_gian("ThepToi", "#2A2E33", nham=0.35, kim_loai=0.6)
    m_mai = vl.don_gian("MaiNhaCho", "#30353B", nham=0.5)
    m_kinh = vl.kinh_trong()
    m_ghe = vl.don_gian("GheGo", "#8A6A4A", nham=0.55)
    m_trang = vl.don_gian("ChuTrangNC", "#FFFFFF", nham=0.4, phat=0.6, mau_phat="#FFFFFF")
    m_hop_den = vl.don_gian("HopDen", "#1E3A5F", nham=0.4, phat=1.6, mau_phat="#2B5A8C")
    m_tuyen = vl.don_gian("ChipTuyen", "#DC2626", nham=0.4, phat=0.4)
    mats = [m_be, m_thep, m_mai, m_kinh, m_ghe, m_hop_den]

    bm = bmesh.new()
    X = 3.2
    _hop(bm, -X - 0.2, X + 0.2, -1.05, 1.05, 0.0, 0.12, 0)                    # bệ
    for x in (-X + 0.1, X - 0.1):
        for y in (-0.55, 0.78):
            _hop(bm, x - 0.06, x + 0.06, y - 0.06, y + 0.06, 0.12, 2.72, 1)     # cột
    _hop(bm, -X - 0.25, X + 0.25, -1.25, 1.0, 2.72, 2.84, 2)                 # mái
    _hop(bm, -X - 0.25, X + 0.25, -1.3, -1.22, 2.42, 2.88, 2)                # băng biển tên trước
    _hop(bm, -X + 0.15, X - 0.15, 0.8, 0.82, 0.32, 2.45, 3)                  # vách kính sau
    _hop(bm, -X + 0.15, X - 0.15, 0.77, 0.85, 2.42, 2.48, 1)                 # thanh khung trên
    _hop(bm, -X + 0.15, X - 0.15, 0.77, 0.85, 0.28, 0.34, 1)                 # thanh khung dưới
    _hop(bm, X - 0.12, X - 0.08, -0.45, 0.76, 0.32, 2.45, 3)                 # vách kính bên
    _hop(bm, -X + 0.06, -X + 0.18, -0.4, 0.76, 0.2, 2.5, 1)                  # khung hộp đèn
    _hop(bm, -X + 0.04, -X + 0.2, -0.32, 0.68, 0.45, 2.3, 5)                 # hộp đèn (2 mặt sáng)
    _hop(bm, -1.6, 1.6, 0.25, 0.68, 0.44, 0.5, 4)                            # mặt ghế
    for x in (-1.4, 0, 1.4):
        _hop(bm, x - 0.04, x + 0.04, 0.4, 0.55, 0.12, 0.44, 1)
    # cột biển báo trạm (đứng ở mép vỉa hè, đầu nhà chờ)
    _hop(bm, X + 0.75, X + 0.83, -0.85, -0.77, 0.12, 3.3, 1)
    _hop(bm, X + 0.5, X + 1.08, -0.86, -0.76, 2.35, 3.25, 2)

    me = bpy.data.meshes.new(ten)
    bm.to_mesh(me)
    bm.free()
    for m in mats:
        me.materials.append(m)
    o = bpy.data.objects.new(ten, me)
    bpy.context.scene.collection.objects.link(o)

    chu = [
        _chu("TRẠM XE BUÝT", 0.17, (-X + 0.15, -1.31, 2.72), (90, 0, 0), m_trang, FONT_DAM, "LEFT"),
        _chu(ten_tram, 0.15, (-X + 0.15, -1.31, 2.53), (90, 0, 0), m_trang, FONT, "LEFT"),
        # biển cột
        _chu("XE", 0.2, (X + 0.79, -0.87, 3.0), (90, 0, 0), m_trang, FONT_DAM),
        _chu("BUÝT", 0.2, (X + 0.79, -0.87, 2.75), (90, 0, 0), m_trang, FONT_DAM),
        # hộp đèn: thông tin hệ thống
        _chu("XeBus", 0.26, (-X + 0.21, 0.18, 1.85), (90, 0, 90), m_trang, FONT_DAM),
        _chu("Tra cứu trạm", 0.1, (-X + 0.21, 0.18, 1.55), (90, 0, 90), m_trang),
        _chu("và tuyến buýt", 0.1, (-X + 0.21, 0.18, 1.4), (90, 0, 90), m_trang),
    ]
    # chip số tuyến trên băng biển tên (phải)
    x = X - 0.2
    for so in reversed(cac_tuyen):
        bm2 = bmesh.new()
        _hop(bm2, x - 0.36, x, -1.32, -1.31, 2.5, 2.8, 0)
        me2 = bpy.data.meshes.new("chip")
        bm2.to_mesh(me2)
        bm2.free()
        me2.materials.append(m_tuyen)
        c = bpy.data.objects.new("chip", me2)
        bpy.context.scene.collection.objects.link(c)
        chu.append(c)
        chu.append(_chu(so, 0.16, (x - 0.18, -1.325, 2.65), (90, 0, 0), m_trang, FONT_DAM))
        x -= 0.45

    bpy.ops.object.select_all(action="DESELECT")
    for c in chu:
        if c.type == "FONT":
            c.select_set(True)
            bpy.context.view_layer.objects.active = c
    bpy.ops.object.convert(target="MESH")
    for c in chu:
        c.select_set(True)
    o.select_set(True)
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.join()
    return o
