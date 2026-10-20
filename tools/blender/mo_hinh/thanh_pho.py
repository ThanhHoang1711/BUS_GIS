"""
Dựng thành phố từ dữ liệu thật (canh.json – xuat_du_lieu.sql): nhà (mặt tiền thủ tục + bồn nước inox trên mái),
vỉa hè quanh khối nhà, nền phố, đường (bề rộng theo tên phố, vạch sơn theo UV), mặt hồ có kè đá, thảm cỏ,
và riêng phố của trạm mở đầu: bó vỉa + vỉa hè cao cho cảnh quay cận.
Độ cao (z): nền −0,30 · nước −0,20 · cỏ 0,12 · vỉa hè quanh nhà 0,15 · đường 0,17 · vỉa hè phố chính 0,32.
"""
import math
import random
import bmesh
import bpy
from . import vat_lieu as vl

Z_NEN, Z_NUOC, Z_CO, Z_VIAHE, Z_DUONG, Z_VIAHE_CAO = -0.30, -0.20, 0.12, 0.15, 0.17, 0.32

# bề rộng lòng đường (m) theo tên phố – mặc định 14 m
BE_RONG = {
    "Trần Quang Khải": 26, "Nguyễn Khoái": 24, "Yên Phụ": 24, "Tôn Đức Thắng": 24, "Xã Đàn": 26, "Lê Duẩn": 20,
    "Trần Hưng Đạo": 18, "Đại Cồ Việt": 26, "Trần Khánh Dư": 20, "Trần Khát Chân": 22, "Nguyễn Văn Cừ": 28,
    "Trần Nhật Duật": 22, "Nguyễn Thái Học": 16, "Phan Ðình Phùng": 18, "Tràng Tiền": 14, "Tràng Thi": 14,
    "Điện Biên Phủ": 20, "Hoàng Hoa Thám": 18, "Giảng Võ": 22, "Kim Ngưu": 16, "Kim Mã": 22, "Giải Phóng": 26,
    "Hàng Ðậu": 16, "Hàng Khay": 14, "Cầu Chương Dương": 18, "Đường dẫn cầu Chương Dương": 16, "Nguyễn Lương Bằng": 20,
}

# màu sơn nhà phố Hà Nội (vàng thuộc địa, kem, trắng, ghi, hồng nhạt, xanh xám) – trọng số
MAU_NHA = [("#F1EEE6", 5), ("#E9E1CF", 5), ("#E7CF8E", 5), ("#D9B867", 3), ("#D9D5CC", 3), ("#B9B5AD", 3),
           ("#E3C9BB", 2), ("#C8D2D2", 2), ("#D6B79B", 2), ("#9FA7A3", 2), ("#E8D9B0", 3), ("#CBD9C4", 1), ("#D9C3D0", 1),
           ("#A9B8C4", 1), ("#8E8B84", 2)]


def _link(o):
    bpy.context.scene.collection.objects.link(o)
    return o


def cac_duong(g):
    if not g:
        return []
    if isinstance(g[0][0], list):
        return [l for l in g if len(l) >= 2]
    return [g] if len(g) >= 2 else []


def cac_poly(g):
    if not g:
        return []
    return g if isinstance(g[0][0][0], list) else [g]


def _sach(vong):
    pts = [(p[0], p[1]) for p in vong]
    if len(pts) > 1 and pts[0] == pts[-1]:
        pts = pts[:-1]
    s = []
    for p in pts:
        if not s or abs(p[0] - s[-1][0]) > 0.05 or abs(p[1] - s[-1][1]) > 0.05:
            s.append(p)
    if len(s) > 2 and abs(s[0][0] - s[-1][0]) < 0.05 and abs(s[0][1] - s[-1][1]) < 0.05:
        s.pop()
    return s if len(s) >= 3 else None


def dien_tich(pts):
    return sum(pts[i][0] * pts[(i + 1) % len(pts)][1] - pts[(i + 1) % len(pts)][0] * pts[i][1] for i in range(len(pts))) / 2


# ------------------------------------------------------------------------------------------------ nhà
def _so_tang(gid, loai, dt, kc):
    r = random.Random(gid)
    loai = (loai or "").lower()
    if loai in ("apartments", "office", "hotel", "commercial") or dt > 2500:
        t = r.randint(7, 14)
    elif dt > 900:
        t = r.randint(5, 9)
    elif dt > 250:
        t = r.randint(4, 7)
    else:
        t = r.choice([3, 3, 4, 4, 4, 5, 5, 6])
    if loai in ("church", "cathedral", "temple", "pagoda"):
        t = 4
    if kc < 260:      # sát hồ: thấp tầng (quy hoạch khu hồ)
        t = min(t, 6)
    return t


def ds_nha_that(du_lieu, gioi_han=3200):
    """Nhà OSM → [(pts ngược chiều kim đồng hồ, số tầng, hạt, mái dốc?)]."""
    kq = []
    for gid, loai, g in du_lieu:
        for poly in cac_poly(g):
            if not poly:
                continue
            pts = _sach(poly[0])
            if not pts:
                continue
            dt = dien_tich(pts)
            if abs(dt) < 15:
                continue
            if dt < 0:
                pts.reverse()
            cx = sum(p[0] for p in pts) / len(pts)
            cy = sum(p[1] for p in pts) / len(pts)
            kc = math.hypot(cx, cy)
            if kc > gioi_han:
                continue
            kq.append((pts, _so_tang(gid, loai, abs(dt), kc), gid, False))
    return kq


def nha(ds, ten="Nha", vat_tren_mai=2200):
    """ds = [(pts, số tầng, hạt, mái dốc)] → mesh nhà (tường: mặt tiền thủ tục; mái bằng / mái ngói) + vật trên mái."""
    m_tuong, m_mai, m_ngoi = vl.mat_tien_nha(), vl.mai_nha(), vl.mai_ngoi()
    m_inox = vl.don_gian("Inox", "#C9CED2", nham=0.25, kim_loai=1.0)
    m_tum = vl.don_gian("TumThang", "#D9D4CA", nham=0.85)
    bm = bmesh.new()
    uv = bm.loops.layers.uv.new("UVMap")
    mau = bm.loops.layers.float_color.new("mau")
    hat = bm.loops.layers.float_color.new("hat")
    tong_tr = sum(w for _, w in MAU_NHA)
    bm_mai = bmesh.new()     # bồn nước, tum thang
    O_CUA = 3.2
    from mathutils import Matrix

    def mau_hinh(tao):
        """Hình mẫu dựng 1 lần → (toạ độ đỉnh, chỉ số mặt). bmesh.ops trên bm_mai đang lớn dần tốn O(cả lưới) mỗi lần gọi."""
        b = bmesh.new()
        tao(b)
        b.verts.index_update()
        kq = ([v.co.copy() for v in b.verts], [[v.index for v in f.verts] for f in b.faces])
        b.free()
        return kq

    MH_BON = mau_hinh(lambda b: bmesh.ops.create_cone(b, cap_ends=True, segments=14, radius1=0.55, radius2=0.55, depth=1.6))
    MH_TUM = mau_hinh(lambda b: bmesh.ops.create_cube(b, size=1))

    def chep(mh, M, vat):
        ds_v = [bm_mai.verts.new(M @ co) for co in mh[0]]
        for f in mh[1]:
            bm_mai.faces.new([ds_v[i] for i in f]).material_index = vat

    def chon_mau(r):
        x = r.uniform(0, tong_tr)
        for hx, w in MAU_NHA:
            x -= w
            if x <= 0:
                return vl.hex_rgb(hx)
        return vl.hex_rgb(MAU_NHA[0][0])

    def to_mau(f, c, hz):
        for lp in f.loops:
            lp[mau] = c
            lp[hat] = hz

    for pts, t, gid, mai_doc in ds:
        r = random.Random(gid * 7 + 3)
        h = t * 3.3 + r.uniform(0.3, 0.9)
        c = chon_mau(r)
        hz = (r.random(), 0, 0, 1)
        duoi = [bm.verts.new((x, y, Z_VIAHE)) for x, y in pts]
        tren = [bm.verts.new((x, y, Z_VIAHE + h)) for x, y in pts]
        n = len(pts)
        if not mai_doc:
            try:
                f = bm.faces.new(tren)
                f.material_index = 1
                to_mau(f, c, hz)
                for lp in f.loops:
                    lp[uv].uv = (lp.vert.co.x, lp.vert.co.y)
            except ValueError:
                pass
        else:
            # mái ngói 2 dốc trên hình chữ nhật (pts = 4 đỉnh, cạnh 0-1 là mặt tiền ngắn): nóc dọc chiều dài
            p0, p1, p2, p3 = [bm.verts.new((x, y, Z_VIAHE + h)) for x, y in pts]
            m01 = ((pts[0][0] + pts[1][0]) / 2, (pts[0][1] + pts[1][1]) / 2)
            m23 = ((pts[2][0] + pts[3][0]) / 2, (pts[2][1] + pts[3][1]) / 2)
            cao_noc = min(2.4, math.dist(pts[0], pts[1]) * 0.42)
            a = bm.verts.new((m01[0], m01[1], Z_VIAHE + h + cao_noc))
            b = bm.verts.new((m23[0], m23[1], Z_VIAHE + h + cao_noc))
            for q, mi in (((p1, p2, b, a), 2), ((p3, p0, a, b), 2), ((p0, p1, a), 0), ((p2, p3, b), 0)):
                try:
                    f = bm.faces.new(q)
                    f.material_index = mi
                    to_mau(f, c, hz)
                    for lp in f.loops:
                        lp[uv].uv = (lp.vert.co.x + lp.vert.co.y, lp.vert.co.z) if mi == 2 else (0.0, 0.0)
                except ValueError:
                    pass
        for i in range(n):
            j = (i + 1) % n
            dai_t = math.dist(pts[i], pts[j])
            if dai_t < 0.05:
                continue
            try:
                f = bm.faces.new((duoi[i], duoi[j], tren[j], tren[i]))
            except ValueError:
                continue
            f.material_index = 0
            # u căn giữa để ô cửa đối xứng trên mỗi bức tường; tường quá hẹp ⇒ không có cửa
            lech = (dai_t % O_CUA) / 2 if dai_t >= 2.4 else 0
            u0, u1 = (-lech, dai_t - lech) if dai_t >= 2.4 else (0.0, 0.0)
            for lp, (uu, vv) in zip(f.loops, ((u0, 0), (u1, 0), (u1, h), (u0, h))):
                lp[uv].uv = (uu, vv)
            to_mau(f, c, hz)
        # trên mái bằng: bồn nước inox nằm ngang + tum thang (nhà thấp tầng, gần trung tâm)
        cx = sum(p[0] for p in pts) / n
        cy = sum(p[1] for p in pts) / n
        if not mai_doc and t <= 7 and abs(dien_tich(pts)) > 30 and math.hypot(cx, cy) < vat_tren_mai:
            for k in range(r.choice([0, 1, 1, 2])):
                px, py = cx + r.uniform(-1.5, 1.5), cy + r.uniform(-1.5, 1.5)
                chep(MH_BON, Matrix.Translation((px, py, Z_VIAHE + h + 0.85)) @
                     Matrix.Rotation(r.uniform(0, math.pi), 4, "Z") @ Matrix.Rotation(math.pi / 2, 4, "Y"), 0)
            if r.random() < 0.45:
                chep(MH_TUM, Matrix.Translation((cx - 1.0, cy + 0.8, Z_VIAHE + h + 1.25)) @ Matrix.Diagonal((2.4, 2.4, 2.5, 1)), 1)
    me = bpy.data.meshes.new(ten)
    bm.to_mesh(me)
    bm.free()
    me.materials.append(m_tuong)
    me.materials.append(m_mai)
    me.materials.append(m_ngoi)
    o = _link(bpy.data.objects.new(ten, me))
    me2 = bpy.data.meshes.new(ten + "_Mai")
    bm_mai.to_mesh(me2)
    bm_mai.free()
    me2.materials.append(m_inox)
    me2.materials.append(m_tum)
    _link(bpy.data.objects.new(ten + "_Mai", me2))
    print(ten, len(ds))
    return o


# ------------------------------------------------------------------------------------------------ lấp khoảng trống
class LuoiChiem:
    """Lưới chiếm chỗ ô o mét trên hình vuông bán kính R: 1 = đã có (nhà, đường, nước, cỏ…)."""

    def __init__(self, R, o=2.0):
        self.R, self.o = R, o
        self.n = int(2 * R / o)
        self.a = bytearray(self.n * self.n)

    def to(self, pts):
        R, o, n, a = self.R, self.o, self.n, self.a
        ys = [p[1] for p in pts]
        j0, j1 = max(0, int((min(ys) + R) / o)), min(n - 1, int((max(ys) + R) / o))
        m = len(pts)
        for j in range(j0, j1 + 1):
            yc = -R + (j + 0.5) * o
            xs = []
            for i in range(m):
                (x1, y1), (x2, y2) = pts[i][:2], pts[(i + 1) % m][:2]
                if (y1 <= yc < y2) or (y2 <= yc < y1):
                    xs.append(x1 + (yc - y1) * (x2 - x1) / (y2 - y1))
            xs.sort()
            for k in range(0, len(xs) - 1, 2):
                i0, i1 = max(0, int((xs[k] + R) / o)), min(n - 1, int((xs[k + 1] + R) / o))
                if i1 >= i0:
                    a[j * n + i0: j * n + i1 + 1] = b"\x01" * (i1 - i0 + 1)

    def ban(self, x, y):
        i, j = int((x + self.R) / self.o), int((y + self.R) / self.o)
        if i < 0 or j < 0 or i >= self.n or j >= self.n:
            return True
        return self.a[j * self.n + i] == 1


def _chu_nhat(cx, cy, rong, sau, goc):
    """4 đỉnh ngược chiều kim đồng hồ; cạnh 0-1 = mặt tiền (rộng), dọc hướng goc+90°."""
    ux, uy = math.cos(goc), math.sin(goc)          # chiều sâu
    vx, vy = -uy, ux                               # chiều rộng
    hr, hs = rong / 2, sau / 2
    return [(cx - ux * hs - vx * hr, cy - uy * hs - vy * hr), (cx - ux * hs + vx * hr, cy - uy * hs + vy * hr),
            (cx + ux * hs + vx * hr, cy + uy * hs + vy * hr), (cx + ux * hs - vx * hr, cy + uy * hs - vy * hr)]


def lap_day(dl, nha_that, tuyen_duong, ban_kinh=1600, hat=11):
    """Rải dãy nhà ống vào chỗ trống (dữ liệu OSM thiếu nhà khu phố cổ) – không đè nhà thật / đường / nước / cỏ."""
    r = random.Random(hat)
    L = LuoiChiem(ban_kinh + 60, 2.0)
    for pts, _, _, _ in nha_that:
        L.to(pts)
    for ten, w, pl in tuyen_duong:
        nua = w / 2 + (6.0 if ten == "Tràng Thi" else 3.0)
        for i in range(len(pl) - 1):
            (x0, y0), (x1, y1) = pl[i], pl[i + 1]
            d = math.dist((x0, y0), (x1, y1))
            if d < 0.01:
                continue
            nx, ny = -(y1 - y0) / d * nua, (x1 - x0) / d * nua
            L.to([(x0 + nx, y0 + ny), (x1 + nx, y1 + ny), (x1 - nx, y1 - ny), (x0 - nx, y0 - ny)])
    for khoa in ("nuoc", "xanh", "venho", "dao"):
        for g in dl.get(khoa) or []:
            for poly in cac_poly(g):
                if poly and len(poly[0]) >= 3:
                    L.to(poly[0])
    # hướng phố gần nhất (lưới thô 40 m)
    mau_duong = []
    for ten, w, pl in tuyen_duong:
        for i in range(len(pl) - 1):
            (x0, y0), (x1, y1) = pl[i], pl[i + 1]
            d = math.dist((x0, y0), (x1, y1))
            g = math.atan2(y1 - y0, x1 - x0)
            for k in range(int(d // 15) + 1):
                a = min(1.0, k * 15 / d) if d else 0
                mau_duong.append((x0 + a * (x1 - x0), y0 + a * (y1 - y0), g))

    def huong(x, y):
        tot, g = 1e18, 0.0
        for px, py, pg in mau_duong:
            dd = (px - x) ** 2 + (py - y) ** 2
            if dd < tot:
                tot, g = dd, pg
        return g
    cache = {}
    ket = []
    buoc = 6.0
    o_list = [(i, j) for i in range(int(-ban_kinh / buoc), int(ban_kinh / buoc) + 1)
              for j in range(int(-ban_kinh / buoc), int(ban_kinh / buoc) + 1)]
    r.shuffle(o_list)
    for i, j in o_list:
        cx, cy = i * buoc + r.uniform(-2, 2), j * buoc + r.uniform(-2, 2)
        if math.hypot(cx, cy) > ban_kinh or L.ban(cx, cy):
            continue
        k = (int(cx // 40), int(cy // 40))
        if k not in cache:
            cache[k] = huong(k[0] * 40 + 20, k[1] * 40 + 20)
        goc = cache[k] + (math.pi / 2 if r.random() < 0.5 else 0) + r.uniform(-0.05, 0.05)
        rong, sau = r.uniform(4.2, 6.8), r.uniform(9, 17)
        if r.random() < 0.25:
            rong, sau = r.uniform(8, 14), r.uniform(10, 18)
        # kiểm tra khung nở rộng 1 m
        thu = _chu_nhat(cx, cy, rong + 2, sau + 2, goc)
        ok = True
        for a in range(5):
            for b in range(5):
                ta, tb = a / 4, b / 4
                x = thu[0][0] + ta * (thu[1][0] - thu[0][0]) + tb * (thu[3][0] - thu[0][0])
                y = thu[0][1] + ta * (thu[1][1] - thu[0][1]) + tb * (thu[3][1] - thu[0][1])
                if L.ban(x, y):
                    ok = False
                    break
            if not ok:
                break
        if not ok:
            continue
        pts = _chu_nhat(cx, cy, rong, sau, goc)
        L.to(pts)
        so_tang = r.choice([2, 3, 3, 4, 4, 4, 5, 5, 6])
        mai_doc = so_tang <= 3 and r.random() < 0.55 or r.random() < 0.12
        ket.append((pts, so_tang, 900000 + len(ket), mai_doc))
    print("nha lap day:", len(ket))
    return ket


def ven_ho(dl):
    """Đường dạo lát đá quanh hồ."""
    return tam_giac("VenHo", dl.get("venho"), Z_VIAHE + 0.01, vl.via_he())


# ------------------------------------------------------------------------------------------------ tam giác phẳng
def tam_giac(ten, ds, z, mat, gop=False):
    bm = bmesh.new()
    for g in ds or []:
        for poly in cac_poly(g):
            if not poly:
                continue
            pts = _sach(poly[0])
            if not pts:
                continue
            if dien_tich(pts) < 0:
                pts.reverse()
            try:
                bm.faces.new([bm.verts.new((x, y, z)) for x, y in pts])
            except ValueError:
                pass
    if gop:
        bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=0.02)
    me = bpy.data.meshes.new(ten)
    bm.to_mesh(me)
    bm.free()
    me.materials.append(mat)
    return _link(bpy.data.objects.new(ten, me))


def nen(ban_kinh=7000):
    bm = bmesh.new()
    bmesh.ops.create_circle(bm, cap_ends=True, radius=ban_kinh, segments=96)
    for v in bm.verts:
        v.co.z = Z_NEN
    me = bpy.data.meshes.new("Nen")
    bm.to_mesh(me)
    bm.free()
    me.materials.append(vl.nen_dat())
    return _link(bpy.data.objects.new("Nen", me))


def mat_nuoc(ds):
    """Mặt nước + kè đá (thành đứng từ mặt nước lên mặt đất quanh mép)."""
    o = tam_giac("Nuoc", ds, Z_NUOC, vl.nuoc_ho(), gop=True)
    bm = bmesh.new()
    bm.from_mesh(o.data)
    bien = [e for e in bm.edges if len(e.link_faces) == 1]
    ke = bmesh.new()
    for e in bien:
        a, b = e.verts[0].co, e.verts[1].co
        v = [ke.verts.new((a.x, a.y, Z_NUOC - 0.05)), ke.verts.new((b.x, b.y, Z_NUOC - 0.05)),
             ke.verts.new((b.x, b.y, Z_CO + 0.08)), ke.verts.new((a.x, a.y, Z_CO + 0.08))]
        ke.faces.new(v)
    bm.free()
    me = bpy.data.meshes.new("KeDa")
    ke.to_mesh(me)
    ke.free()
    me.materials.append(vl.be_tong())
    _link(bpy.data.objects.new("KeDa", me))
    return o


# ------------------------------------------------------------------------------------------------ đường
def dai(bm, pts, a, b, z, uv=None, u_toi=None):
    """Dải giữa hai đường lệch a..b (m, dương = bên trái hướng đi) dọc polyline, nối góc kiểu miter.
    UV: u = mét dọc tuyến, v = 0 (lệch a) … 1 (lệch b)."""
    n = len(pts)
    if n < 2:
        return
    phap = []
    for i in range(n):
        if i == 0:
            dx, dy = pts[1][0] - pts[0][0], pts[1][1] - pts[0][1]
            k = 1.0
        elif i == n - 1:
            dx, dy = pts[-1][0] - pts[-2][0], pts[-1][1] - pts[-2][1]
            k = 1.0
        else:
            d1 = (pts[i][0] - pts[i - 1][0], pts[i][1] - pts[i - 1][1])
            d2 = (pts[i + 1][0] - pts[i][0], pts[i + 1][1] - pts[i][1])
            l1, l2 = math.hypot(*d1) or 1, math.hypot(*d2) or 1
            dx, dy = d1[0] / l1 + d2[0] / l2, d1[1] / l1 + d2[1] / l2
            cos_nua = max(0.35, math.hypot(dx, dy) / 2)
            k = 1 / cos_nua
        l = math.hypot(dx, dy) or 1
        phap.append((-dy / l * k, dx / l * k))
    u = 0.0
    truoc = None
    for i in range(n):
        if i:
            u += math.dist(pts[i - 1], pts[i])
        nx, ny = phap[i]
        va = bm.verts.new((pts[i][0] + nx * a, pts[i][1] + ny * a, z))
        vb = bm.verts.new((pts[i][0] + nx * b, pts[i][1] + ny * b, z))
        if truoc:
            try:
                f = bm.faces.new((truoc[0], va, vb, truoc[1]) if b > a else (truoc[1], vb, va, truoc[0]))
                if uv:
                    for lp in f.loops:
                        lp[uv].uv = (u if lp.vert in (va, vb) else truoc[2], 0.0 if lp.vert in (va, truoc[0]) else 1.0)
            except ValueError:
                pass
        truoc = (va, vb, u)


def duong(ds_duong, gioi_han=2700):
    """Lòng đường mọi phố (z hơi khác nhau giữa các phố để nút giao không nhấp nháy)."""
    bm = bmesh.new()
    uv = bm.loops.layers.uv.new("UVMap")
    tuyen = []
    for i, (ten, loai, g) in enumerate(ds_duong):
        w = BE_RONG.get(ten, 14)
        for pl in cac_duong(g):
            dai(bm, pl, -w / 2, w / 2, Z_DUONG + (i % 9) * 0.002, uv)
            tuyen.append((ten, w, pl))
    me = bpy.data.meshes.new("Duong")
    bm.to_mesh(me)
    bm.free()
    me.materials.append(vl.duong_nhua())
    _link(bpy.data.objects.new("Duong", me))
    return tuyen


def pho_chinh(pl, w, rong_he=5.0):
    """Phố của trạm mở đầu: vỉa hè cao 15 cm hai bên + bó vỉa đá (cho cảnh quay cận)."""
    bm = bmesh.new()
    for s in (1, -1):
        dai(bm, pl, s * w / 2, s * (w / 2 + rong_he), Z_VIAHE_CAO)
    me = bpy.data.meshes.new("ViaHeChinh")
    bm.to_mesh(me)
    bm.free()
    me.materials.append(vl.via_he())
    _link(bpy.data.objects.new("ViaHeChinh", me))
    # bó vỉa: thành đứng + mặt trên 25 cm
    bm = bmesh.new()
    for s in (1, -1):
        dai(bm, pl, s * w / 2, s * (w / 2 + 0.25), Z_VIAHE_CAO + 0.005)
        # thành đứng
        n = len(pl)
        for i in range(n - 1):
            p0, p1 = pl[i], pl[i + 1]
            dx, dy = p1[0] - p0[0], p1[1] - p0[1]
            l = math.hypot(dx, dy) or 1
            nx, ny = -dy / l * s * w / 2, dx / l * s * w / 2
            v = [bm.verts.new((p0[0] + nx, p0[1] + ny, Z_DUONG - 0.02)), bm.verts.new((p1[0] + nx, p1[1] + ny, Z_DUONG - 0.02)),
                 bm.verts.new((p1[0] + nx, p1[1] + ny, Z_VIAHE_CAO + 0.005)), bm.verts.new((p0[0] + nx, p0[1] + ny, Z_VIAHE_CAO + 0.005))]
            bm.faces.new(v)
    me = bpy.data.meshes.new("BoVia")
    bm.to_mesh(me)
    bm.free()
    me.materials.append(vl.don_gian("DaBoVia", "#B9B4AA", nham=0.8))
    _link(bpy.data.objects.new("BoVia", me))


def dung_thanh_pho(dl, lap=True):
    import time
    t0 = time.time()

    def moc(ten):
        print("   thanh pho / %-12s %6.1fs" % (ten, time.time() - t0), flush=True)
    nen()
    nha_that = ds_nha_that(dl.get("nha") or [])
    nha(nha_that, "Nha")
    moc("nha")
    tam_giac("ViaHe", dl.get("viahe"), Z_VIAHE, vl.via_he())
    moc("via he")
    tam_giac("Co", dl.get("xanh"), Z_CO, vl.co())
    ven_ho(dl)
    mat_nuoc(dl.get("nuoc"))
    moc("nuoc co")
    tuyen = duong(dl.get("duong") or [])
    moc("duong")
    if lap:
        ds = lap_day(dl, nha_that, tuyen)
        moc("lap day")
        nha(ds, "NhaLapDay")
        moc("nha lap day")
    return tuyen
