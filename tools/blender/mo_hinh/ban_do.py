"""
Lớp "bản đồ GIS" xuất hiện ở đoạn bay cao: dải màu tuyến tự vẽ dần dọc mặt đường (vật liệu phẳng, không phát sáng),
ghim trạm trồi lên lần lượt từ tâm ra ngoài; tất cả thu lại trước khi máy quay hạ xuống phố (vòng lặp liền).
"""
import math
import bmesh
import bpy
from mathutils import Matrix
from . import vat_lieu as vl
from .thanh_pho import dai


def _vat_lieu_tuyen(ten, mau, dai_m, f_ve, f_xong, f_an, f_het):
    """Principled màu tuyến × Transparent theo (u < tiến độ) × độ hiện – tiến độ & độ hiện có keyframe."""
    m = bpy.data.materials.new(ten)
    m.use_nodes = True
    n, l = m.node_tree.nodes, m.node_tree.links
    out = n["Material Output"]
    p = n["Principled BSDF"]
    p.inputs["Base Color"].default_value = vl.hex_rgb(mau)
    p.inputs["Roughness"].default_value = 0.5
    tr = n.new("ShaderNodeBsdfTransparent")
    mix = n.new("ShaderNodeMixShader")
    uv = n.new("ShaderNodeUVMap")
    tach = n.new("ShaderNodeSeparateXYZ")
    l.new(uv.outputs["UV"], tach.inputs["Vector"])
    tien = n.new("ShaderNodeValue")
    tien.label = "tien_do"
    hien = n.new("ShaderNodeValue")
    hien.label = "hien"
    lt = n.new("ShaderNodeMath")
    lt.operation = "LESS_THAN"
    l.new(tach.outputs["X"], lt.inputs[0])
    l.new(tien.outputs[0], lt.inputs[1])
    nhan = n.new("ShaderNodeMath")
    nhan.operation = "MULTIPLY"
    l.new(lt.outputs[0], nhan.inputs[0])
    l.new(hien.outputs[0], nhan.inputs[1])
    l.new(nhan.outputs[0], mix.inputs["Fac"])
    l.new(tr.outputs[0], mix.inputs[1])
    l.new(p.outputs[0], mix.inputs[2])
    l.new(mix.outputs[0], out.inputs["Surface"])
    if hasattr(m, "blend_method"):
        m.blend_method = "HASHED"
    # keyframe
    for f, gt in ((1, 0.0), (f_ve, 0.0), (f_xong, dai_m + 5)):
        tien.outputs[0].default_value = gt
        tien.outputs[0].keyframe_insert("default_value", frame=f)
    for f, gt in ((1, 1.0), (f_an, 1.0), (f_het, 0.0)):
        hien.outputs[0].default_value = gt
        hien.outputs[0].keyframe_insert("default_value", frame=f)
    return m


def dai_tuyen(ten, pl, mau, f_ve=300, f_xong=360, f_an=498, f_het=522, rong=4.0, z=0.21):
    bm = bmesh.new()
    uv = bm.loops.layers.uv.new("UVMap")
    dai(bm, pl, -rong / 2, rong / 2, z, uv)
    me = bpy.data.meshes.new(ten)
    bm.to_mesh(me)
    bm.free()
    L = sum(math.dist(pl[i], pl[i + 1]) for i in range(len(pl) - 1))
    me.materials.append(_vat_lieu_tuyen(ten + "_VL", mau, L, f_ve, f_xong, f_an, f_het))
    o = bpy.data.objects.new(ten, me)
    bpy.context.scene.collection.objects.link(o)
    return o


def ghim_tram(ds_tram, f_dau=300, f_an=496, ban_kinh=1800):
    """Ghim trạm: thân mảnh + đầu tròn (than chì + vòng trắng), mọc lên theo khoảng cách tới tâm."""
    m_dau = vl.don_gian("GhimDau", "#111827", nham=0.35)
    m_vong = vl.don_gian("GhimVong", "#FFFFFF", nham=0.4)
    bm = bmesh.new()
    c = bmesh.ops.create_cone(bm, cap_ends=True, segments=10, radius1=0.12, radius2=0.6, depth=30.0)
    bmesh.ops.translate(bm, verts=c["verts"], vec=(0, 0, 15.0))
    s = bmesh.ops.create_uvsphere(bm, u_segments=24, v_segments=14, radius=3.3)
    bmesh.ops.translate(bm, verts=s["verts"], vec=(0, 0, 32.5))
    r = bmesh.ops.create_cone(bm, cap_ends=True, segments=24, radius1=1.5, radius2=1.5, depth=6.9)
    bmesh.ops.transform(bm, verts=r["verts"], matrix=Matrix.Translation((0, 0, 32.5)) @ Matrix.Rotation(math.pi / 2, 4, "X"))
    for v in r["verts"]:
        for f in v.link_faces:
            f.material_index = 1
    me = bpy.data.meshes.new("Ghim")
    bm.to_mesh(me)
    bm.free()
    me.materials.append(m_dau)
    me.materials.append(m_vong)
    ds = []
    for ma, x, y in ds_tram:
        kc = math.hypot(x, y)
        if kc > ban_kinh:
            continue
        o = bpy.data.objects.new("Ghim_" + ma, me)
        bpy.context.scene.collection.objects.link(o)
        o.location = (x, y, 0.3)
        tre = int(kc / ban_kinh * 36)
        for f, k in ((1, 0.0), (f_dau + tre, 0.0), (f_dau + tre + 14, 1.08), (f_dau + tre + 20, 1.0),
                     (f_an + tre // 3, 1.0), (f_an + tre // 3 + 12, 0.0)):
            o.scale = (k, k, k)
            o.keyframe_insert("scale", frame=f)
        ds.append(o)
    return ds
