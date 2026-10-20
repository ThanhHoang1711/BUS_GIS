"""Thiết lập dùng chung: bầu trời (HDRI CC0), nắng chiều, máy render (Cycles GPU / EEVEE), quản lý màu."""
import math
import os
import bpy
from . import vat_lieu as vl

HDRI = os.path.join(vl.THU_MUC_ASSETS, "kloofendal_48d_partly_cloudy_puresky", "kloofendal_48d_partly_cloudy_puresky.hdr")
if not os.path.exists(HDRI):
    HDRI = os.path.join(vl.THU_MUC_ASSETS, "citrus_orchard_puresky", "citrus_orchard_puresky.hdr")

# Hướng nắng: phương vị (độ, từ Bắc theo chiều kim đồng hồ) + độ cao – chiều muộn, nắng từ tây-tây nam
NANG_PHUONG_VI = 248.0
NANG_DO_CAO = 32.0


def xoa_canh():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def bau_troi(do_sang=0.9, xoay_hdri=0.0):
    sc = bpy.context.scene
    w = bpy.data.worlds.new("Troi")
    sc.world = w
    w.use_nodes = True
    n, l = w.node_tree.nodes, w.node_tree.links
    bg = n["Background"]
    if os.path.exists(HDRI):
        env = n.new("ShaderNodeTexEnvironment")
        env.image = bpy.data.images.load(HDRI, check_existing=True)
        tc = n.new("ShaderNodeTexCoord")
        mp = n.new("ShaderNodeMapping")
        mp.inputs["Rotation"].default_value = (0, 0, math.radians(xoay_hdri))
        l.new(tc.outputs["Generated"], mp.inputs["Vector"])
        l.new(mp.outputs["Vector"], env.inputs["Vector"])
        # kẹp độ sáng: bỏ đĩa mặt trời của HDRI (bóng đổ do đèn Sun quyết định – tránh 2 bóng), giữ màu trời
        tach = n.new("ShaderNodeSeparateColor")
        gop = n.new("ShaderNodeCombineColor")
        l.new(env.outputs["Color"], tach.inputs["Color"])
        for i in range(3):
            k = n.new("ShaderNodeMath")
            k.operation = "MINIMUM"
            k.inputs[1].default_value = 3.0
            l.new(tach.outputs[i], k.inputs[0])
            l.new(k.outputs[0], gop.inputs[i])
        l.new(gop.outputs["Color"], bg.inputs["Color"])
    else:
        bg.inputs["Color"].default_value = vl.hex_rgb("#BFD3E6")
    bg.inputs["Strength"].default_value = do_sang
    return w


def nang(nang_luong=4.2):
    """Đèn mặt trời theo NANG_PHUONG_VI / NANG_DO_CAO. Trục Blender: +Y = Bắc, +X = Đông."""
    pv = math.radians(NANG_PHUONG_VI)
    dc = math.radians(NANG_DO_CAO)
    # hướng TỚI mặt trời
    huong = (math.sin(pv) * math.cos(dc), math.cos(pv) * math.cos(dc), math.sin(dc))
    sun = bpy.data.lights.new("Nang", "SUN")
    sun.energy = nang_luong
    sun.color = (1.0, 0.9, 0.78)
    sun.angle = math.radians(1.2)
    o = bpy.data.objects.new("Nang", sun)
    bpy.context.scene.collection.objects.link(o)
    # đèn sun chiếu theo −Z cục bộ ⇒ xoay để −Z trùng hướng từ mặt trời xuống
    from mathutils import Vector
    o.rotation_euler = (-Vector(huong)).to_track_quat("-Z", "Y").to_euler()
    return o


def may_render(engine="CYCLES", rong=1600, cao=1200, mau=96, ty_le=100):
    sc = bpy.context.scene
    sc.render.resolution_x = rong
    sc.render.resolution_y = cao
    sc.render.resolution_percentage = ty_le
    sc.render.fps = 24
    try:
        sc.view_settings.view_transform = "AgX"
        sc.view_settings.look = "AgX - Medium High Contrast"
    except Exception:
        pass
    if engine == "CYCLES":
        sc.render.engine = "CYCLES"
        prefs = bpy.context.preferences.addons["cycles"].preferences
        for loai in ("OPTIX", "CUDA"):
            try:
                prefs.compute_device_type = loai
                prefs.get_devices()
                co = False
                for d in prefs.devices:
                    d.use = d.type == loai
                    co |= d.use
                if co:
                    break
            except Exception:
                continue
        sc.cycles.device = "GPU"
        sc.cycles.samples = mau
        sc.cycles.use_adaptive_sampling = True
        sc.cycles.adaptive_threshold = 0.02
        sc.cycles.use_denoising = True
        try:
            sc.cycles.denoiser = "OPTIX"
        except Exception:
            pass
        sc.cycles.max_bounces = 6
        sc.cycles.diffuse_bounces = 3
        sc.cycles.glossy_bounces = 3
        sc.cycles.transmission_bounces = 6
        sc.cycles.caustics_reflective = False
        sc.cycles.caustics_refractive = False
        sc.cycles.blur_glossy = 1.0
        sc.render.use_persistent_data = True
    elif engine == "WORKBENCH":
        sc.render.engine = "BLENDER_WORKBENCH"
        sc.display.shading.light = "STUDIO"
        sc.display.shading.color_type = "MATERIAL"
    else:
        ids = [e.identifier for e in bpy.types.RenderSettings.bl_rna.properties["engine"].enum_items]
        sc.render.engine = "BLENDER_EEVEE_NEXT" if "BLENDER_EEVEE_NEXT" in ids else "BLENDER_EEVEE"
        try:
            sc.eevee.taa_render_samples = 32
            sc.eevee.use_raytracing = True
            sc.eevee.use_shadows = True
        except Exception:
            pass
    sc.render.image_settings.file_format = "JPEG"
    sc.render.image_settings.quality = 94
