"""Máy quay: một cú máy liên tục qua các mốc (khung, vị trí, điểm nhìn) – nội suy Bezier mượt; DOF lấy nét theo điểm nhìn."""
import bpy


def tao_may_quay(moc, lens=30.0, f_stop=4.0):
    """moc = [(khung, (x,y,z) máy, (x,y,z) điểm nhìn, lens tuỳ chọn)]."""
    sc = bpy.context.scene
    dich = bpy.data.objects.new("DiemNhin", None)
    sc.collection.objects.link(dich)
    cd = bpy.data.cameras.new("MayQuay")
    cd.lens = lens
    cd.clip_start = 0.1
    cd.clip_end = 9000
    cd.sensor_width = 36
    cd.dof.use_dof = True
    cd.dof.focus_object = dich
    cd.dof.aperture_fstop = f_stop
    cam = bpy.data.objects.new("MayQuay", cd)
    sc.collection.objects.link(cam)
    sc.camera = cam
    tr = cam.constraints.new("TRACK_TO")
    tr.target = dich
    tr.track_axis = "TRACK_NEGATIVE_Z"
    tr.up_axis = "UP_Y"
    for m in moc:
        f, p, t = m[0], m[1], m[2]
        cam.location = p
        cam.keyframe_insert("location", frame=f)
        dich.location = t
        dich.keyframe_insert("location", frame=f)
        if len(m) > 3:
            cd.lens = m[3]
            cd.keyframe_insert("lens", frame=f)
    for o in (cam, dich, cd):
        ad = o.animation_data
        if ad and ad.action:
            for fc in ad.action.fcurves:
                for kp in fc.keyframe_points:
                    kp.interpolation = "BEZIER"
                    kp.handle_left_type = kp.handle_right_type = "AUTO_CLAMPED"
                fc.update()
    return cam, dich
