"""Vật liệu dùng chung cho cảnh video nền (Cycles / EEVEE). Mọi màu nhập dạng hex sRGB."""
import os
import bpy

THU_MUC_ASSETS = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "assets")
_cache = {}


def hex_rgb(h, a=1.0):
    h = h.lstrip("#")
    c = [int(h[i:i + 2], 16) / 255 for i in (0, 2, 4)]
    c = [x / 12.92 if x <= 0.04045 else ((x + 0.055) / 1.055) ** 2.4 for x in c]
    return (c[0], c[1], c[2], a)


def _moi(ten):
    m = bpy.data.materials.new(ten)
    m.use_nodes = True
    return m, m.node_tree.nodes, m.node_tree.links, m.node_tree.nodes["Principled BSDF"]


def dat(p, ten, gt):
    if ten in p.inputs:
        p.inputs[ten].default_value = gt


def don_gian(ten, mau, nham=0.6, kim_loai=0.0, phu=0.0, phat=0.0, mau_phat=None, alpha=1.0):
    """Principled một màu (sơn, nhựa, kim loại…)."""
    key = ("dg", ten)
    if key in _cache:
        return _cache[key]
    m, n, l, p = _moi(ten)
    dat(p, "Base Color", hex_rgb(mau))
    dat(p, "Roughness", nham)
    dat(p, "Metallic", kim_loai)
    if phu > 0:
        dat(p, "Coat Weight", phu)
        dat(p, "Coat Roughness", 0.06)
    if phat > 0:
        dat(p, "Emission Color", hex_rgb(mau_phat or mau))
        dat(p, "Emission Strength", phat)
    if alpha < 1:
        dat(p, "Alpha", alpha)
        m.blend_method = "BLEND" if hasattr(m, "blend_method") else None
    _cache[key] = m
    return m


def kinh(ten="Kinh", mau="#10181D", nham=0.04):
    """Kính ô tô / nhà chờ: tối, bóng, phản chiếu trời."""
    key = ("kinh", ten)
    if key in _cache:
        return _cache[key]
    m, n, l, p = _moi(ten)
    dat(p, "Base Color", hex_rgb(mau))
    dat(p, "Roughness", nham)
    dat(p, "Specular IOR Level", 0.8)
    dat(p, "Coat Weight", 1.0)
    dat(p, "Coat Roughness", 0.02)
    _cache[key] = m
    return m


def kinh_trong(ten="KinhTrong"):
    """Kính trong (vách nhà chờ): truyền sáng nhẹ."""
    key = ("kt", ten)
    if key in _cache:
        return _cache[key]
    m, n, l, p = _moi(ten)
    dat(p, "Base Color", hex_rgb("#DDE7EA"))
    dat(p, "Roughness", 0.03)
    dat(p, "Transmission Weight", 1.0)
    dat(p, "IOR", 1.45)
    _cache[key] = m
    return m


def _anh(n, duong_dan, mau=True):
    t = n.new("ShaderNodeTexImage")
    if os.path.exists(duong_dan):
        t.image = bpy.data.images.load(duong_dan, check_existing=True)
        if not mau:
            t.image.colorspace_settings.name = "Non-Color"
    return t


def be_mat_anh(ten, ma_asset, kich_o=4.0, mau_nhan="#FFFFFF", sang=1.0, uv=False, them=None):
    """Vật liệu từ texture Poly Haven (Diffuse/nor_gl/Rough), chiếu theo toạ độ object (mét) hoặc UV."""
    key = ("anh", ten)
    if key in _cache:
        return _cache[key]
    m, n, l, p = _moi(ten)
    goc = os.path.join(THU_MUC_ASSETS, ma_asset, ma_asset)
    tc = n.new("ShaderNodeTexCoord")
    mp = n.new("ShaderNodeMapping")
    mp.inputs["Scale"].default_value = (1 / kich_o, 1 / kich_o, 1 / kich_o)
    l.new(tc.outputs["UV" if uv else "Object"], mp.inputs["Vector"])
    d = _anh(n, goc + "_Diffuse.jpg")
    r = _anh(n, goc + "_Rough.jpg", False)
    nm = _anh(n, goc + "_nor_gl.jpg", False)
    for t in (d, r, nm):
        l.new(mp.outputs["Vector"], t.inputs["Vector"])
    nhan = n.new("ShaderNodeMix")
    nhan.data_type = "RGBA"
    nhan.blend_type = "MULTIPLY"
    nhan.inputs["Factor"].default_value = 1.0
    l.new(d.outputs["Color"], nhan.inputs[6])
    nhan.inputs[7].default_value = tuple(x * sang for x in hex_rgb(mau_nhan)[:3]) + (1,)
    l.new(nhan.outputs[2], p.inputs["Base Color"])
    l.new(r.outputs["Color"], p.inputs["Roughness"])
    nmap = n.new("ShaderNodeNormalMap")
    nmap.inputs["Strength"].default_value = 0.8
    l.new(nm.outputs["Color"], nmap.inputs["Color"])
    l.new(nmap.outputs["Normal"], p.inputs["Normal"])
    if them:
        them(m, n, l, p, nhan)
    _cache[key] = m
    return m


def duong_nhua():
    """Mặt đường: nhựa asphalt_02 + vạch sơn theo UV (u = mét dọc đường, v = 0..1 ngang đường).
    Thuộc tính 'so_lan' (số làn mỗi chiều) trên mesh quyết định có vạch tim/vạch làn."""
    def them(m, n, l, p, nhan):
        uv = n.new("ShaderNodeUVMap")
        tach = n.new("ShaderNodeSeparateXYZ")
        l.new(uv.outputs["UV"], tach.inputs["Vector"])
        # vạch tim nét đứt: |v-0.5| < 0.006 và fract(u/9) < 0.45  ; vạch mép liền: v<0.035 hoặc v>0.965 (bề rộng 0.12 m tương đối)
        def toan(op, a, b=None, ten=None):
            x = n.new("ShaderNodeMath")
            x.operation = op
            for i, v in enumerate((a, b)):
                if v is None:
                    continue
                if isinstance(v, (int, float)):
                    x.inputs[i].default_value = v
                else:
                    l.new(v, x.inputs[i])
            return x.outputs[0]
        v = tach.outputs["Y"]
        u = tach.outputs["X"]
        lech = toan("ABSOLUTE", toan("SUBTRACT", v, 0.5))
        tim = toan("LESS_THAN", lech, 0.0075)
        dut = toan("LESS_THAN", toan("FRACT", toan("DIVIDE", u, 9.0)), 0.45)
        vach_tim = toan("MULTIPLY", tim, dut)
        mep = toan("MAXIMUM", toan("LESS_THAN", v, 0.022), toan("GREATER_THAN", v, 0.978))
        mep_trong = toan("MAXIMUM", toan("GREATER_THAN", v, 0.006), 0)   # bỏ mép ngoài cùng
        vach = toan("MINIMUM", toan("MAXIMUM", vach_tim, toan("MULTIPLY", mep, toan("LESS_THAN", toan("ABSOLUTE", toan("SUBTRACT", v, 0.5)), 0.493))), 1.0)
        # độ mòn của sơn
        nz = n.new("ShaderNodeTexNoise")
        nz.inputs["Scale"].default_value = 1.5
        mon = toan("GREATER_THAN", nz.outputs["Fac"], 0.38)
        vach = toan("MULTIPLY", vach, mon)
        tron = n.new("ShaderNodeMix")
        tron.data_type = "RGBA"
        l.new(vach, tron.inputs["Factor"])
        l.new(nhan.outputs[2], tron.inputs[6])
        tron.inputs[7].default_value = hex_rgb("#E9E6DD")
        l.new(tron.outputs[2], p.inputs["Base Color"])
    return be_mat_anh("DuongNhua", "asphalt_02", kich_o=3.5, mau_nhan="#8C8C8C", sang=1.0, them=them)


def via_he():
    return be_mat_anh("ViaHe", "concrete_tiles", kich_o=2.4, mau_nhan="#D8D2C6", sang=1.0)


def nen_dat():
    """Khoảng trống giữa các khối nhà (phố nhỏ) – nhựa cũ, sáng hơn đường chính."""
    return be_mat_anh("NenDat", "concrete_floor_01", kich_o=5.0, mau_nhan="#9C978E", sang=1.0)


def be_tong():
    return be_mat_anh("BeTong", "concrete_floor_01", kich_o=3.0, mau_nhan="#C9C4BA", sang=1.0)


def nuoc_ho():
    """Mặt hồ xanh ngọc (Hồ Gươm): bóng, gợn sóng nhỏ bằng noise bump."""
    key = ("nuoc",)
    if key in _cache:
        return _cache[key]
    m, n, l, p = _moi("NuocHo")
    dat(p, "Base Color", hex_rgb("#33584C"))
    dat(p, "Roughness", 0.04)
    dat(p, "Specular IOR Level", 0.7)
    nz = n.new("ShaderNodeTexNoise")
    nz.inputs["Scale"].default_value = 0.9
    nz.inputs["Detail"].default_value = 6
    tc = n.new("ShaderNodeTexCoord")
    mp = n.new("ShaderNodeMapping")
    mp.inputs["Scale"].default_value = (0.25, 0.6, 1)
    l.new(tc.outputs["Object"], mp.inputs["Vector"])
    l.new(mp.outputs["Vector"], nz.inputs["Vector"])
    bump = n.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = 0.12
    bump.inputs["Distance"].default_value = 0.05
    l.new(nz.outputs["Fac"], bump.inputs["Height"])
    l.new(bump.outputs["Normal"], p.inputs["Normal"])
    _cache[key] = m
    return m


def co():
    """Cỏ / thảm xanh: màu biến thiên theo noise lớn."""
    key = ("co",)
    if key in _cache:
        return _cache[key]
    m, n, l, p = _moi("Co")
    nz = n.new("ShaderNodeTexNoise")
    nz.inputs["Scale"].default_value = 0.08
    nz.inputs["Detail"].default_value = 4
    tc = n.new("ShaderNodeTexCoord")
    l.new(tc.outputs["Object"], nz.inputs["Vector"])
    ramp = n.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].color = hex_rgb("#4E6B2F")
    ramp.color_ramp.elements[1].color = hex_rgb("#7C9447")
    l.new(nz.outputs["Fac"], ramp.inputs["Fac"])
    l.new(ramp.outputs["Color"], p.inputs["Base Color"])
    dat(p, "Roughness", 0.95)
    _cache[key] = m
    return m


def mat_tien_nha():
    """
    Mặt tiền nhà phố thủ tục: UV tường u = mét dọc tường (đã căn giữa ô cửa), v = mét theo chiều cao.
    Thuộc tính góc 'mau' = màu sơn từng nhà, 'hat' (x) = số ngẫu nhiên của nhà. Tầng 3,3 m, ô cửa 3,2 m;
    tầng trệt là mặt tiền cửa hàng; cửa sổ kính tối phản chiếu + một số ô có rèm sáng / cửa chớp.
    """
    key = ("mattien",)
    if key in _cache:
        return _cache[key]
    m, n, l, p = _moi("MatTienNha")

    def toan(op, a, b=None):
        x = n.new("ShaderNodeMath")
        x.operation = op
        for i, v in enumerate((a, b)):
            if v is None:
                continue
            if isinstance(v, (int, float)):
                x.inputs[i].default_value = v
            else:
                l.new(v, x.inputs[i])
        return x.outputs[0]

    def tron(fac, a, b):
        x = n.new("ShaderNodeMix")
        x.data_type = "RGBA"
        if isinstance(fac, (int, float)):
            x.inputs["Factor"].default_value = fac
        else:
            l.new(fac, x.inputs["Factor"])
        for i, v in ((6, a), (7, b)):
            if isinstance(v, tuple):
                x.inputs[i].default_value = v
            else:
                l.new(v, x.inputs[i])
        return x.outputs[2]

    def tron_so(fac, a, b):
        x = n.new("ShaderNodeMix")
        x.data_type = "FLOAT"
        l.new(fac, x.inputs["Factor"])
        for i, v in ((2, a), (3, b)):
            if isinstance(v, (int, float)):
                x.inputs[i].default_value = v
            else:
                l.new(v, x.inputs[i])
        return x.outputs[0]

    uv = n.new("ShaderNodeUVMap")
    tach = n.new("ShaderNodeSeparateXYZ")
    l.new(uv.outputs["UV"], tach.inputs["Vector"])
    u, v = tach.outputs["X"], tach.outputs["Y"]
    mau = n.new("ShaderNodeAttribute")
    mau.attribute_name = "mau"
    hat = n.new("ShaderNodeAttribute")
    hat.attribute_name = "hat"
    hat_x = n.new("ShaderNodeSeparateColor")
    l.new(hat.outputs["Color"], hat_x.inputs["Color"])
    seed = hat_x.outputs["Red"]

    tang = 3.3
    o_cua = 3.2
    fu = toan("FRACT", toan("DIVIDE", u, o_cua))
    fv = toan("FRACT", toan("DIVIDE", v, tang))
    so_tang = toan("FLOOR", toan("DIVIDE", v, tang))
    so_o = toan("FLOOR", toan("DIVIDE", u, o_cua))

    def trong(x, a, b):
        return toan("MULTIPLY", toan("GREATER_THAN", x, a), toan("LESS_THAN", x, b))

    cua_so = toan("MULTIPLY", toan("MULTIPLY", trong(fu, 0.2, 0.8), trong(fv, 0.3, 0.84)), toan("GREATER_THAN", so_tang, 0.5))
    khung = toan("MULTIPLY", toan("MULTIPLY", trong(fu, 0.16, 0.84), trong(fv, 0.26, 0.88)), toan("GREATER_THAN", so_tang, 0.5))
    khung = toan("SUBTRACT", khung, cua_so)
    # ban công: dải ngang ở chân cửa sổ của một số nhà
    ban_cong = toan("MULTIPLY", toan("MULTIPLY", trong(fv, 0.24, 0.31), trong(fu, 0.1, 0.9)),
                    toan("MULTIPLY", toan("GREATER_THAN", so_tang, 0.5), toan("GREATER_THAN", seed, 0.45)))
    tret = toan("LESS_THAN", v, 3.6)
    cua_hang = toan("MULTIPLY", toan("MULTIPLY", tret, trong(fu, 0.05, 0.95)), trong(v, 0.25, 3.0))

    # ngẫu nhiên từng ô cửa: rèm sáng / cửa chớp xanh / kính tối
    whash = n.new("ShaderNodeTexWhiteNoise")
    whash.noise_dimensions = "3D"
    ghep = n.new("ShaderNodeCombineXYZ")
    l.new(so_o, ghep.inputs[0])
    l.new(so_tang, ghep.inputs[1])
    l.new(toan("MULTIPLY", seed, 97.0), ghep.inputs[2])
    l.new(ghep.outputs[0], whash.inputs["Vector"])
    r_o = whash.outputs["Value"]
    mau_kinh = tron(toan("GREATER_THAN", r_o, 0.72), hex_rgb("#1E2A31"), hex_rgb("#C9BFA8"))          # rèm
    mau_kinh = tron(toan("MULTIPLY", toan("GREATER_THAN", r_o, 0.9), toan("GREATER_THAN", seed, 0.6)), mau_kinh, hex_rgb("#4F7A62"))  # cửa chớp xanh
    # tường: màu nhà × vết bẩn
    nz = n.new("ShaderNodeTexNoise")
    nz.inputs["Scale"].default_value = 0.35
    nz.inputs["Detail"].default_value = 8
    tc = n.new("ShaderNodeTexCoord")
    l.new(tc.outputs["Object"], nz.inputs["Vector"])
    ban = toan("ADD", toan("MULTIPLY", nz.outputs["Fac"], 0.35), 0.78)
    tuong = n.new("ShaderNodeMix")
    tuong.data_type = "RGBA"
    tuong.blend_type = "MULTIPLY"
    tuong.inputs["Factor"].default_value = 1.0
    l.new(mau.outputs["Color"], tuong.inputs[6])
    gray = n.new("ShaderNodeCombineColor")
    for i in range(3):
        l.new(ban, gray.inputs[i])
    l.new(gray.outputs["Color"], tuong.inputs[7])
    # vệt bẩn chảy dọc dưới cửa sổ
    tuong_c = tuong.outputs[2]
    c = tron(khung, tuong_c, hex_rgb("#E9E6DF"))
    c = tron(ban_cong, c, hex_rgb("#5B5F60"))
    c = tron(cua_so, c, mau_kinh)
    # cửa hàng: kính tối + bên trong ấm nhẹ
    mau_hang = tron(toan("GREATER_THAN", r_o, 0.5), hex_rgb("#2A2622"), hex_rgb("#4A3A2C"))
    c = tron(cua_hang, c, mau_hang)
    # biển hiệu cửa hàng (dải màu trên cửa tầng trệt) – đỏ, vàng, xanh, trắng xen kẽ theo từng gian 6 m
    bien = toan("MULTIPLY", toan("MULTIPLY", trong(v, 3.02, 3.52), trong(fu, 0.03, 0.97)), toan("GREATER_THAN", seed, 0.25))
    wb = n.new("ShaderNodeTexWhiteNoise")
    wb.noise_dimensions = "2D"
    gb = n.new("ShaderNodeCombineXYZ")
    l.new(toan("FLOOR", toan("DIVIDE", u, 6.4)), gb.inputs[0])
    l.new(toan("MULTIPLY", seed, 31.0), gb.inputs[1])
    l.new(gb.outputs[0], wb.inputs["Vector"])
    ramp_b = n.new("ShaderNodeValToRGB")
    ramp_b.color_ramp.interpolation = "CONSTANT"
    els = ramp_b.color_ramp.elements
    els[0].color = hex_rgb("#C62828")
    els[1].position = 0.25
    els[1].color = hex_rgb("#F2B705")
    for pos, hx in ((0.45, "#1E5AA8"), (0.62, "#F4F1EA"), (0.8, "#2E7D32")):
        e = els.new(pos)
        e.color = hex_rgb(hx)
    l.new(wb.outputs["Value"], ramp_b.inputs["Fac"])
    c = tron(bien, c, ramp_b.outputs["Color"])
    l.new(c, p.inputs["Base Color"])
    kinh_mask = toan("MAXIMUM", cua_so, cua_hang)
    l.new(tron_so(kinh_mask, 0.82, 0.06), p.inputs["Roughness"])
    l.new(tron_so(kinh_mask, 0.3, 0.9), p.inputs["Specular IOR Level"])
    # lõm cửa sổ (bump)
    bump = n.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = 0.6
    bump.inputs["Distance"].default_value = 0.08
    l.new(toan("SUBTRACT", 1.0, toan("MAXIMUM", toan("MAXIMUM", cua_so, cua_hang), toan("MULTIPLY", khung, 0.4))), bump.inputs["Height"])
    l.new(bump.outputs["Normal"], p.inputs["Normal"])
    _cache[key] = m
    return m


def mai_nha():
    """Mái bằng bê tông xám có vệt ố."""
    key = ("mai",)
    if key in _cache:
        return _cache[key]
    m, n, l, p = _moi("MaiNha")
    nz = n.new("ShaderNodeTexNoise")
    nz.inputs["Scale"].default_value = 0.3
    nz.inputs["Detail"].default_value = 6
    tc = n.new("ShaderNodeTexCoord")
    l.new(tc.outputs["Object"], nz.inputs["Vector"])
    ramp = n.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].color = hex_rgb("#6E6A63")
    ramp.color_ramp.elements[1].color = hex_rgb("#A7A298")
    l.new(nz.outputs["Fac"], ramp.inputs["Fac"])
    l.new(ramp.outputs["Color"], p.inputs["Base Color"])
    dat(p, "Roughness", 0.9)
    _cache[key] = m
    return m


def mai_ngoi():
    """Ngói đỏ nâu (mái dốc nhà cũ phố cổ): rãnh ngói theo UV u, vệt rêu/bẩn theo noise."""
    key = ("ngoi",)
    if key in _cache:
        return _cache[key]
    m, n, l, p = _moi("MaiNgoi")
    uv = n.new("ShaderNodeUVMap")
    wave = n.new("ShaderNodeTexWave")
    wave.wave_type = "BANDS"
    wave.inputs["Scale"].default_value = 1.0
    wave.inputs["Distortion"].default_value = 0.0
    mp = n.new("ShaderNodeMapping")
    mp.inputs["Scale"].default_value = (3.2, 3.2, 3.2)
    l.new(uv.outputs["UV"], mp.inputs["Vector"])
    l.new(mp.outputs["Vector"], wave.inputs["Vector"])
    nz = n.new("ShaderNodeTexNoise")
    nz.inputs["Scale"].default_value = 0.6
    tc = n.new("ShaderNodeTexCoord")
    l.new(tc.outputs["Object"], nz.inputs["Vector"])
    ramp = n.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].color = hex_rgb("#5E3A2B")
    ramp.color_ramp.elements[1].color = hex_rgb("#A2593C")
    l.new(nz.outputs["Fac"], ramp.inputs["Fac"])
    mix = n.new("ShaderNodeMix")
    mix.data_type = "RGBA"
    mix.blend_type = "MULTIPLY"
    mix.inputs["Factor"].default_value = 0.35
    l.new(ramp.outputs["Color"], mix.inputs[6])
    l.new(wave.outputs["Color"], mix.inputs[7])
    l.new(mix.outputs[2], p.inputs["Base Color"])
    dat(p, "Roughness", 0.8)
    bump = n.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = 0.5
    l.new(wave.outputs["Fac"], bump.inputs["Height"])
    l.new(bump.outputs["Normal"], p.inputs["Normal"])
    _cache[key] = m
    return m
