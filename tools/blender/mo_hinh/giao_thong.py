"""
Giao thông lặp liền mạch đúng chu kỳ video (CHU_KY giây):
  • xe buýt chính: chạy → giảm tốc → dừng đón khách ở trạm → tăng tốc rời đi; 3 xe nối đuôi cách nhau đúng CHU_KY
    ⇒ khung cuối trùng khung đầu;
  • dòng xe nền (Geometry Nodes): N xe cách đều trên mỗi làn, trượt đúng n khoảng xe trong một chu kỳ, biến thể xe
    chọn theo (chỉ số mod n) ⇒ lặp không lộ; xe thu nhỏ dần ở hai đầu tuyến để không "hiện ra" đột ngột.
"""
import math
import bpy
from . import vat_lieu as vl

FPS = 24
CHU_KY = 24.0


# ------------------------------------------------------------------------------------------------ hình học tuyến
def lech(pl, d):
    """Polyline lệch sang TRÁI hướng đi một khoảng d (m) – d âm = sang phải."""
    kq = []
    n = len(pl)
    for i in range(n):
        if i == 0:
            dx, dy = pl[1][0] - pl[0][0], pl[1][1] - pl[0][1]
            k = 1
        elif i == n - 1:
            dx, dy = pl[-1][0] - pl[-2][0], pl[-1][1] - pl[-2][1]
            k = 1
        else:
            a = (pl[i][0] - pl[i - 1][0], pl[i][1] - pl[i - 1][1])
            b = (pl[i + 1][0] - pl[i][0], pl[i + 1][1] - pl[i][1])
            la, lb = math.hypot(*a) or 1, math.hypot(*b) or 1
            dx, dy = a[0] / la + b[0] / lb, a[1] / la + b[1] / lb
            k = 1 / max(0.35, math.hypot(dx, dy) / 2)
        l = math.hypot(dx, dy) or 1
        kq.append((pl[i][0] - dy / l * d * k, pl[i][1] + dx / l * d * k))
    return kq


class Tuyen:
    """Đi dọc polyline theo quãng đường s (m); s ngoài [0, L] ⇒ kéo dài thẳng theo hướng đầu/cuối."""

    def __init__(self, pl):
        self.pl = pl
        self.tich = [0.0]
        for i in range(1, len(pl)):
            self.tich.append(self.tich[-1] + math.dist(pl[i - 1], pl[i]))
        self.L = self.tich[-1]

    def tai(self, s):
        pl, t = self.pl, self.tich
        if s <= 0:
            i = 1
        elif s >= self.L:
            i = len(pl) - 1
        else:
            i = 1
            while t[i] < s:
                i += 1
        p0, p1 = pl[i - 1], pl[i]
        d = (t[i] - t[i - 1]) or 1e-6
        a = (s - t[i - 1]) / d
        x, y = p0[0] + a * (p1[0] - p0[0]), p0[1] + a * (p1[1] - p0[1])
        return x, y, math.atan2(p1[1] - p0[1], p1[0] - p0[0])

    def chieu(self, x, y):
        """Quãng đường s của điểm chiếu (x, y) lên tuyến."""
        tot, s_tot = 1e18, 0.0
        for i in range(1, len(self.pl)):
            (x0, y0), (x1, y1) = self.pl[i - 1], self.pl[i]
            dx, dy = x1 - x0, y1 - y0
            l2 = dx * dx + dy * dy or 1e-9
            a = max(0.0, min(1.0, ((x - x0) * dx + (y - y0) * dy) / l2))
            px, py = x0 + a * dx, y0 + a * dy
            d = (px - x) ** 2 + (py - y) ** 2
            if d < tot:
                tot, s_tot = d, self.tich[i - 1] + a * math.sqrt(l2)
        return s_tot


# ------------------------------------------------------------------------------------------------ xe buýt chính
def quang_duong_xe_buyt(tau, s_dung, t_dung=7.0, cho=3.2, v=8.0, a_giam=1.15, a_tang=1.0, v_sau=9.0):
    """s(τ): giảm tốc đều tới dừng đúng s_dung lúc τ = t_dung, đứng 'cho' giây, rồi tăng tốc tới v_sau."""
    t_giam = v / a_giam
    if tau <= t_dung:
        con = t_dung - tau
        if con <= t_giam:
            return s_dung - 0.5 * a_giam * con * con
        return s_dung - 0.5 * a_giam * t_giam * t_giam - v * (con - t_giam)
    if tau <= t_dung + cho:
        return s_dung
    t = tau - t_dung - cho
    t_tang = v_sau / a_tang
    if t <= t_tang:
        return s_dung + 0.5 * a_tang * t * t
    return s_dung + 0.5 * a_tang * t_tang * t_tang + v_sau * (t - t_tang)


def chay_xe_buyt(goc, banh, tuyen, s_dung, lech_pha, so_khung, z=0.0, **kw):
    """Đặt keyframe từng khung cho xe (gốc = empty) theo quang_duong_xe_buyt, τ = t + lech_pha·CHU_KY."""
    truoc = None
    goc_quay = 0.0
    for f in range(1, so_khung + 2):
        t = (f - 1) / FPS
        s = quang_duong_xe_buyt(t + lech_pha * CHU_KY, s_dung, **kw)
        x, y, h = tuyen.tai(s)
        goc.location = (x, y, z)
        goc.rotation_euler = (0, 0, h)
        goc.keyframe_insert("location", frame=f)
        goc.keyframe_insert("rotation_euler", frame=f)
        if truoc is not None:
            goc_quay += (s - truoc) / 0.5         # bán kính bánh 0,5 m
        for b in banh:
            b.rotation_euler = (math.pi / 2, goc_quay, 0)
            b.keyframe_insert("rotation_euler", index=1, frame=f)
        truoc = s
    for o in [goc] + list(banh):
        if o.animation_data and o.animation_data.action:
            for fc in o.animation_data.action.fcurves:
                for kp in fc.keyframe_points:
                    kp.interpolation = "LINEAR"


# ------------------------------------------------------------------------------------------------ dòng xe nền (GN)
def _gn_dong(ten, col, N, n, so_bien, hat):
    nt = bpy.data.node_groups.new(ten, "GeometryNodeTree")
    nt.interface.new_socket("Geometry", in_out="INPUT", socket_type="NodeSocketGeometry")
    nt.interface.new_socket("Geometry", in_out="OUTPUT", socket_type="NodeSocketGeometry")
    N_, L_ = nt.nodes, nt.links
    vao, ra = N_.new("NodeGroupInput"), N_.new("NodeGroupOutput")

    def toan(op, a, b=None):
        x = N_.new("ShaderNodeMath")
        x.operation = op
        for i, v in enumerate((a, b)):
            if v is None:
                continue
            if isinstance(v, (int, float)):
                x.inputs[i].default_value = v
            else:
                L_.new(v, x.inputs[i])
        return x.outputs[0]

    diem = N_.new("GeometryNodePoints")
    diem.inputs["Count"].default_value = N
    idx = N_.new("GeometryNodeInputIndex").outputs[0]
    tg = N_.new("GeometryNodeInputSceneTime")
    giay = toan("DIVIDE", toan("SUBTRACT", tg.outputs["Frame"], 1.0), float(FPS))
    # hệ số trên tuyến: i/N + n·t/(CHU_KY·N)
    he_so = toan("FRACT", toan("ADD", toan("DIVIDE", idx, float(N)), toan("MULTIPLY", giay, n / (CHU_KY * N))))
    mau = N_.new("GeometryNodeSampleCurve")
    mau.mode = "FACTOR"
    mau.use_all_curves = True
    L_.new(vao.outputs[0], mau.inputs["Curves"])
    L_.new(he_so, mau.inputs["Factor"])
    dat = N_.new("GeometryNodeSetPosition")
    L_.new(diem.outputs[0], dat.inputs["Geometry"])
    L_.new(mau.outputs["Position"], dat.inputs["Position"])
    xoay = N_.new("FunctionNodeAlignRotationToVector")
    xoay.axis = "X"
    xoay.pivot_axis = "Z"
    L_.new(mau.outputs["Tangent"], xoay.inputs["Vector"])
    # biến thể: theo (i mod n) ⇒ sau một chu kỳ xe i đứng đúng chỗ xe i+n cùng biến thể
    rv = N_.new("FunctionNodeRandomValue")
    rv.data_type = "INT"
    rv.inputs[4].default_value = 0                     # Min (int) – socket trùng tên vector/float/int ⇒ dùng chỉ số
    rv.inputs[5].default_value = max(0, so_bien - 1)   # Max (int)
    rv.inputs["Seed"].default_value = hat
    L_.new(toan("FLOORED_MODULO", idx, float(n)), rv.inputs["ID"])
    # thu nhỏ ở 12 m đầu/cuối tuyến
    dai = N_.new("GeometryNodeCurveLength")
    L_.new(vao.outputs[0], dai.inputs["Curve"])
    s = toan("MULTIPLY", he_so, dai.outputs[0])
    gan = toan("MINIMUM", s, toan("SUBTRACT", dai.outputs[0], s))
    ty_le = toan("MINIMUM", toan("DIVIDE", gan, 12.0), 1.0)
    iop = N_.new("GeometryNodeInstanceOnPoints")
    iop.inputs["Pick Instance"].default_value = True
    ci = N_.new("GeometryNodeCollectionInfo")
    ci.inputs["Collection"].default_value = col
    ci.inputs["Separate Children"].default_value = True
    ci.inputs["Reset Children"].default_value = True
    L_.new(dat.outputs[0], iop.inputs["Points"])
    L_.new(ci.outputs[0], iop.inputs["Instance"])
    L_.new(rv.outputs[2] if len(rv.outputs) > 2 else rv.outputs[0], iop.inputs["Instance Index"])
    L_.new(xoay.outputs[0], iop.inputs["Rotation"])
    L_.new(ty_le, iop.inputs["Scale"])
    L_.new(iop.outputs[0], ra.inputs[0])
    return nt


def dong_xe(ten, pl, col, khoang=24.0, v=9.0, z=0.18, hat=0):
    """Một làn xe trên polyline pl (đã lệch làn). Trả về object curve có modifier GN."""
    if len(pl) < 2:
        return None
    cu = bpy.data.curves.new(ten, "CURVE")
    cu.dimensions = "3D"
    sp = cu.splines.new("POLY")
    sp.points.add(len(pl) - 1)
    for i, (x, y) in enumerate(pl):
        sp.points[i].co = (x, y, z, 1)
    o = bpy.data.objects.new(ten, cu)
    bpy.context.scene.collection.objects.link(o)
    L = Tuyen(pl).L
    # n khoảng xe trôi qua mỗi chu kỳ (v·T ≈ n·khoảng), N = bội của n để mẫu biến thể lặp tròn
    n = max(1, round(v * CHU_KY / khoang))
    N = max(n, int(round(L / khoang / n)) * n)
    md = o.modifiers.new("Dong", "NODES")
    md.node_group = _gn_dong(ten + "_GN", col, N, n, len(col.objects), hat)
    return o
