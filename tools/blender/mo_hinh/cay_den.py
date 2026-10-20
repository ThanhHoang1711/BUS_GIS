"""
Cây xanh + đèn đường từ tài nguyên CC0 Poly Haven (assets/), nhân bản bằng Geometry Nodes (instance – nhẹ bộ nhớ):
  • cây vỉa hè dọc mọi phố (2 bên, cách 13–17 m), • cây công viên / quanh hồ rải theo diện tích thảm cỏ,
  • đèn đường dọc phố chính của trạm mở đầu.
Mỗi biến thể = một object (gộp thân + tán) trong collection ẩn; điểm đặt mang thuộc tính bien / ty_le / xoay.
"""
import math
import os
import random
import bpy
from . import vat_lieu as vl

A = vl.THU_MUC_ASSETS


def _nap(blend, ten_objs, ten_moi):
    """Nạp object từ .blend tài nguyên, sửa đường dẫn ảnh thành tuyệt đối, gộp thành 1 object."""
    duong_dan = os.path.join(A, blend)
    if not os.path.exists(duong_dan):
        return None
    with bpy.data.libraries.load(duong_dan, link=False) as (src, dst):
        dst.objects = [n for n in src.objects if n in ten_objs]
    goc = os.path.dirname(duong_dan)
    for img in bpy.data.images:
        if img.filepath.startswith("//") and not os.path.exists(bpy.path.abspath(img.filepath)):
            p = os.path.join(goc, img.filepath[2:].replace("/", os.sep))
            if os.path.exists(p):
                img.filepath = p
    objs = [o for o in dst.objects if o is not None]
    for o in objs:
        bpy.context.scene.collection.objects.link(o)
        o.parent = None
        o.location = (0, 0, 0)
        o.modifiers.clear() if o.type == "MESH" else None
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    if len(objs) > 1:
        bpy.ops.object.join()
    o = bpy.context.view_layer.objects.active
    o.name = ten_moi
    bpy.context.scene.collection.objects.unlink(o)
    return o


def bo_mau():
    """Collection ẩn gồm các biến thể: 0 = cây lớn (jacaranda), 1 = cây vỉa hè, 2 = đèn đường."""
    col = bpy.data.collections.new("MauCayDen")
    ds = [
        _nap("jacaranda_tree/jacaranda_tree.blend", {"jacaranda_tree_LOD1", "jacaranda_tree_trunk_LOD1"}, "CayLon"),
        _nap("tree_small_02/tree_small_02.blend", {"tree_small_02_LOD1", "tree_small_02_trunk"}, "CayVaHe"),
        _nap("street_lamp_01/street_lamp_01.blend", {"street_lamp_01"}, "DenDuong"),
    ]
    for o in ds:
        if o is not None:
            col.objects.link(o)
    return col, ds


def _cay_gn(ten, col):
    """Node tree: điểm → instance biến thể theo thuộc tính 'bien', xoay quanh Z theo 'xoay', tỉ lệ 'ty_le'."""
    nt = bpy.data.node_groups.new(ten, "GeometryNodeTree")
    nt.interface.new_socket("Geometry", in_out="INPUT", socket_type="NodeSocketGeometry")
    nt.interface.new_socket("Geometry", in_out="OUTPUT", socket_type="NodeSocketGeometry")
    n, l = nt.nodes, nt.links
    vao = n.new("NodeGroupInput")
    ra = n.new("NodeGroupOutput")
    m2p = n.new("GeometryNodeMeshToPoints")
    ci = n.new("GeometryNodeCollectionInfo")
    ci.inputs["Collection"].default_value = col
    ci.inputs["Separate Children"].default_value = True
    ci.inputs["Reset Children"].default_value = True
    ci.transform_space = "ORIGINAL"
    iop = n.new("GeometryNodeInstanceOnPoints")
    iop.inputs["Pick Instance"].default_value = True
    bien = n.new("GeometryNodeInputNamedAttribute")
    bien.data_type = "INT"
    bien.inputs["Name"].default_value = "bien"
    tl = n.new("GeometryNodeInputNamedAttribute")
    tl.data_type = "FLOAT"
    tl.inputs["Name"].default_value = "ty_le"
    xo = n.new("GeometryNodeInputNamedAttribute")
    xo.data_type = "FLOAT"
    xo.inputs["Name"].default_value = "xoay"
    cxyz = n.new("ShaderNodeCombineXYZ")
    l.new(xo.outputs["Attribute"], cxyz.inputs["Z"])
    l.new(vao.outputs[0], m2p.inputs["Mesh"])
    l.new(m2p.outputs["Points"], iop.inputs["Points"])
    l.new(ci.outputs[0], iop.inputs["Instance"])
    l.new(bien.outputs["Attribute"], iop.inputs["Instance Index"])
    l.new(cxyz.outputs[0], iop.inputs["Rotation"])
    l.new(tl.outputs["Attribute"], iop.inputs["Scale"])
    l.new(iop.outputs["Instances"], ra.inputs[0])
    return nt


def dat_diem(ten, diem, col):
    """diem = [(x, y, z, bien, ty_le, xoay)] → object điểm có modifier GN nhân bản."""
    me = bpy.data.meshes.new(ten)
    me.vertices.add(len(diem))
    me.vertices.foreach_set("co", [c for d in diem for c in d[:3]])
    a1 = me.attributes.new("bien", "INT", "POINT")
    a1.data.foreach_set("value", [d[3] for d in diem])
    a2 = me.attributes.new("ty_le", "FLOAT", "POINT")
    a2.data.foreach_set("value", [d[4] for d in diem])
    a3 = me.attributes.new("xoay", "FLOAT", "POINT")
    a3.data.foreach_set("value", [d[5] for d in diem])
    o = bpy.data.objects.new(ten, me)
    bpy.context.scene.collection.objects.link(o)
    md = o.modifiers.new("NhanBan", "NODES")
    md.node_group = _cay_gn(ten + "_GN", col)
    return o


# ------------------------------------------------------------------------------------------------ chọn vị trí
class LuoiTamGiac:
    """Tra nhanh điểm có nằm trong tập tam giác (mặt nước) không – lưới 50 m."""

    def __init__(self, ds_tam_giac, o=50.0):
        self.o = o
        self.luoi = {}
        for t in ds_tam_giac:
            xs, ys = [p[0] for p in t], [p[1] for p in t]
            for i in range(int(min(xs) // o), int(max(xs) // o) + 1):
                for j in range(int(min(ys) // o), int(max(ys) // o) + 1):
                    self.luoi.setdefault((i, j), []).append(t)

    def chua(self, x, y):
        for t in self.luoi.get((int(x // self.o), int(y // self.o)), []):
            (x1, y1), (x2, y2), (x3, y3) = t[0][:2], t[1][:2], t[2][:2]
            d = (y2 - y3) * (x1 - x3) + (x3 - x2) * (y1 - y3)
            if abs(d) < 1e-9:
                continue
            a = ((y2 - y3) * (x - x3) + (x3 - x2) * (y - y3)) / d
            b = ((y3 - y1) * (x - x3) + (x1 - x3) * (y - y3)) / d
            if a >= 0 and b >= 0 and a + b <= 1:
                return True
        return False


def tam_giac_cua(ds):
    kq = []
    for g in ds or []:
        polys = g if isinstance(g[0][0][0], list) else [g]
        for poly in polys:
            ring = [p for p in poly[0]]
            if len(ring) >= 3:
                kq.append(ring[:3])
    return kq


def cay_va_den(dl, tuyen_duong, pho_chinh, tram_bd, ban_kinh=1700, tranh_them=()):
    col, mau = bo_mau()
    if mau[0] is None or mau[1] is None:
        print("thiếu tài nguyên cây – chạy tools/blender/tai_tai_nguyen.py")
        return
    r = random.Random(20261018)
    nuoc = LuoiTamGiac(tam_giac_cua(dl.get("nuoc")))
    tranh = [(tram_bd[0], tram_bd[1], 9.0)] + list(tranh_them)     # giữ trống quanh nhà chờ + chỗ máy quay đứng

    def duoc(x, y):
        if math.hypot(x, y) > ban_kinh or nuoc.chua(x, y):
            return False
        return all(math.hypot(x - a, y - b) > rr for a, b, rr in tranh)

    diem = []
    # cây vỉa hè
    for ten, w, pl in tuyen_duong:
        cao = 0.32 if pl is pho_chinh else 0.15
        for s in (1, -1):
            u_tiep = r.uniform(0, 8)
            u = 0.0
            for i in range(len(pl) - 1):
                p0, p1 = pl[i], pl[i + 1]
                d = math.dist(p0, p1)
                if d < 0.01:
                    continue
                tx, ty = (p1[0] - p0[0]) / d, (p1[1] - p0[1]) / d
                while u_tiep <= u + d:
                    k = u_tiep - u
                    lech = w / 2 + 2.1
                    x, y = p0[0] + tx * k - ty * lech * s, p0[1] + ty * k + tx * lech * s
                    if duoc(x, y):
                        if r.random() < 0.85:
                            diem.append((x, y, cao, 1, r.uniform(2.0, 2.6), r.uniform(0, 6.283)))
                        else:
                            diem.append((x, y, cao, 0, r.uniform(0.38, 0.5), r.uniform(0, 6.283)))
                    u_tiep += r.uniform(13, 17)
                u += d
    # cây công viên / quanh hồ: rải theo diện tích từng tam giác cỏ
    for t in tam_giac_cua(dl.get("xanh")):
        (x1, y1), (x2, y2), (x3, y3) = t[0][:2], t[1][:2], t[2][:2]
        dt = abs((x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1)) / 2
        cx, cy = (x1 + x2 + x3) / 3, (y1 + y2 + y3) / 3
        mat_do = 140.0 if math.hypot(cx, cy) < 700 else 260.0
        so = dt / mat_do
        so = int(so) + (1 if r.random() < so - int(so) else 0)
        for _ in range(so):
            a, b = r.random(), r.random()
            if a + b > 1:
                a, b = 1 - a, 1 - b
            x, y = x1 + a * (x2 - x1) + b * (x3 - x1), y1 + a * (y2 - y1) + b * (y3 - y1)
            if duoc(x, y):
                if r.random() < 0.7:
                    diem.append((x, y, 0.12, 0, r.uniform(0.5, 0.78), r.uniform(0, 6.283)))
                else:
                    diem.append((x, y, 0.12, 1, r.uniform(2.2, 2.8), r.uniform(0, 6.283)))
    # hàng cây lớn ven Hồ Gươm: cách mép nước 7 m, mỗi 10 m (phía không phải mặt nước)
    bo = dl.get("bo_ho") or []
    for i in range(len(bo) - 1):
        (x0, y0), (x1, y1) = bo[i], bo[i + 1]
        d = math.dist((x0, y0), (x1, y1))
        if d < 0.5:
            continue
        nx, ny = -(y1 - y0) / d, (x1 - x0) / d
        for k in range(int(d // 10) + 1):
            a = min(1.0, (k * 10 + r.uniform(0, 3)) / d)
            px, py = x0 + a * (x1 - x0), y0 + a * (y1 - y0)
            for s in (1, -1):
                x, y = px + nx * 7 * s, py + ny * 7 * s
                if not nuoc.chua(x, y) and not nuoc.chua(px + nx * 1.5 * s, py + ny * 1.5 * s):
                    diem.append((x, y, 0.16, 0, r.uniform(0.55, 0.78), r.uniform(0, 6.283)))
                    break
    # đèn đường phố chính (2 bên, so le, cách 26 m)
    if mau[2] is not None and pho_chinh:
        w = 14
        for s in (1, -1):
            u_tiep = 6.0 if s > 0 else 19.0
            u = 0.0
            pl = pho_chinh
            for i in range(len(pl) - 1):
                p0, p1 = pl[i], pl[i + 1]
                d = math.dist(p0, p1)
                if d < 0.01:
                    continue
                tx, ty = (p1[0] - p0[0]) / d, (p1[1] - p0[1]) / d
                while u_tiep <= u + d:
                    k = u_tiep - u
                    lech = w / 2 + 0.6
                    x, y = p0[0] + tx * k - ty * lech * s, p0[1] + ty * k + tx * lech * s
                    if all(math.hypot(x - a, y - b) > 6 for a, b, _ in tranh):
                        goc = math.atan2(ty, tx) + (math.pi / 2 if s > 0 else -math.pi / 2)
                        diem.append((x, y, 0.32, 2, 1.25, goc))
                    u_tiep += 26.0
                u += d
    print("cay + den:", len(diem))
    return dat_diem("CayDen", diem, col)
