"""
Render ảnh xem thử mô hình (xe buýt + nhà chờ) – để duyệt mô hình trước khi dựng cả cảnh.
    blender -b --factory-startup -P tools/blender/xem_mo_hinh.py -- <thư mục ra> [CYCLES|EEVEE] [mẫu]
"""
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
RA = argv[0] if argv else "."
ENGINE = argv[1] if len(argv) > 1 else "CYCLES"
MAU = int(argv[2]) if len(argv) > 2 else 64

import bpy  # noqa: E402
from mo_hinh import canh_chung, vat_lieu as vl, xe_buyt, nha_cho  # noqa: E402

canh_chung.xoa_canh()
sc = bpy.context.scene
canh_chung.may_render(ENGINE, 1280, 800, MAU)
canh_chung.bau_troi(0.9)
canh_chung.nang(4.2)

# nền: vỉa hè + đường
bpy.ops.mesh.primitive_plane_add(size=80, location=(0, 0, 0))
nen = bpy.context.active_object
nen.data.materials.append(vl.duong_nhua())
bpy.ops.mesh.primitive_plane_add(size=1, location=(0, 9, 0.15))
vh = bpy.context.active_object
vh.scale = (60, 8, 1)
vh.data.materials.append(vl.via_he())

nc = nha_cho.tao_nha_cho("NhaCho")
nc.location = (0, 8.0, 0.15)
goc, than, banh = xe_buyt.tao_xe_buyt("Xe", "#DC2626", "02", "Kim Mã")
goc.location = (-2.0, 3.2, 0)

cam_d = bpy.data.cameras.new("Cam")
cam_d.lens = 35
cam = bpy.data.objects.new("Cam", cam_d)
sc.collection.objects.link(cam)
sc.camera = cam


def nhin(vi_tri, dich, ten):
    from mathutils import Vector
    cam.location = vi_tri
    cam.rotation_euler = (Vector(dich) - Vector(vi_tri)).to_track_quat("-Z", "Y").to_euler()
    sc.render.filepath = os.path.join(RA, ten)
    bpy.ops.render.render(write_still=True)
    print("ANH", sc.render.filepath)


nhin((14, -9, 2.2), (-1, 5, 1.6), "xem_a_chinh_dien.jpg")
nhin((-16, -6, 4.5), (0, 5, 1.2), "xem_b_duoi_xe.jpg")
nhin((9, 2.5, 1.7), (-3, 4, 1.5), "xem_c_dau_xe.jpg")
nhin((2, -1, 1.6), (0, 8, 1.6), "xem_d_nha_cho.jpg")
