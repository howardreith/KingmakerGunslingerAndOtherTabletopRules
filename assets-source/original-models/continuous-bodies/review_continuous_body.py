"""Offline review renders of a continuous-body source file.

blender -b --factory-startup --python-exit-code 1 --python review_continuous_body.py -- \
    --source <private source.blend> --out <dir> --name <creature> \
    --up z|y --forward=-y|+y|-z|+z [--poses <json>] [--size 720]

Back-face culling is always on: an inward or missing surface renders as a
hole, which is the defect class this refinement exists to remove. Poses
rotate named preview bones (degrees about the bone's local X/Y/Z) to probe
deformation; they are authoring checks, not native animation evidence.
"""
import argparse
import json
import math
import sys
from pathlib import Path

import bpy
from mathutils import Euler, Matrix, Vector


def args():
    argv = sys.argv[sys.argv.index("--") + 1:]
    p = argparse.ArgumentParser()
    p.add_argument("--source", required=True)
    p.add_argument("--out", required=True)
    p.add_argument("--name", required=True)
    p.add_argument("--up", choices=("y", "z"), required=True)
    p.add_argument("--forward", required=True)
    p.add_argument("--poses", default=None)
    p.add_argument("--views", default="party,side,front,rear,top,low")
    p.add_argument("--size", type=int, default=720)
    p.add_argument("--clay", action="store_true")
    return p.parse_args(argv)


def frame_rotation(forward_key, up_key):
    fwd = {"-y": Vector((0, -1, 0)), "+y": Vector((0, 1, 0)),
           "-z": Vector((0, 0, -1)), "+z": Vector((0, 0, 1))}[forward_key]
    up = Vector((0, 0, 1)) if up_key == "z" else Vector((0, 1, 0))
    side = fwd.cross(up)
    basis = Matrix((side, fwd, up)).transposed()
    target = Matrix(((-1, 0, 0), (0, -1, 0), (0, 0, 1))).transposed()
    rot = target @ basis.inverted()
    if rot.determinant() < 0:
        raise SystemExit("frame mapping would mirror")
    return rot.to_4x4()


def main():
    a = args()
    bpy.ops.wm.open_mainfile(filepath=a.source)
    scene = bpy.context.scene
    mesh_obj = next(o for o in scene.objects if o.type == "MESH")
    arm_obj = next(o for o in scene.objects if o.type == "ARMATURE")
    root = bpy.data.objects.new("ReviewRoot", None)
    scene.collection.objects.link(root)
    arm_obj.parent = root
    root.matrix_world = frame_rotation(a.forward, a.up)
    bpy.context.view_layer.update()
    for material in mesh_obj.data.materials:
        if material:
            material.use_backface_culling = True
    if not mesh_obj.data.materials:
        material = bpy.data.materials.new("review")
        material.use_backface_culling = True
        mesh_obj.data.materials.append(material)

    scene.render.engine = "BLENDER_WORKBENCH"
    shading = scene.display.shading
    shading.light = "STUDIO"
    shading.color_type = "SINGLE" if a.clay else "TEXTURE"
    shading.single_color = (0.62, 0.60, 0.56)
    shading.show_backface_culling = True
    shading.show_cavity = True
    shading.cavity_type = "WORLD"
    shading.background_type = "VIEWPORT"
    shading.background_color = (0.20, 0.27, 0.25)
    scene.render.resolution_x = a.size
    scene.render.resolution_y = a.size
    cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam"))
    cam.data.type = "ORTHO"
    scene.collection.objects.link(cam)
    scene.camera = cam

    poses = {"bind": {}}
    if a.poses:
        poses.update(json.loads(Path(a.poses).read_text(encoding="utf-8")))
    views = {"party": Vector((0.55, -0.55, 1.10)), "rear": Vector((0.25, 1.0, 0.55)),
             "side": Vector((1.0, 0.0, 0.12)), "front": Vector((0.25, -1.0, 0.30)),
             "top": Vector((0.0, 0.001, 1.0)), "low": Vector((-0.7, 0.7, 0.05)),
             "otherside": Vector((-1.0, 0.0, 0.12))}
    out = Path(a.out)
    out.mkdir(parents=True, exist_ok=True)
    for pose_name, rotations in poses.items():
        for bone in arm_obj.pose.bones:
            bone.rotation_mode = "XYZ"
            bone.rotation_euler = (0.0, 0.0, 0.0)
        for bone_name, degrees in rotations.items():
            bone = arm_obj.pose.bones.get(bone_name)
            if bone is None:
                raise SystemExit("pose names an absent bone: " + bone_name)
            bone.rotation_euler = Euler([math.radians(d) for d in degrees])
        bpy.context.view_layer.update()
        depsgraph = bpy.context.evaluated_depsgraph_get()
        evaluated = mesh_obj.evaluated_get(depsgraph)
        points = [evaluated.matrix_world @ v.co for v in evaluated.data.vertices]
        lo = Vector([min(p[i] for p in points) for i in range(3)])
        hi = Vector([max(p[i] for p in points) for i in range(3)])
        centre, radius = (lo + hi) / 2, (hi - lo).length / 2
        cam.data.ortho_scale = radius * 2.15
        view_list = a.views.split(",") if pose_name == "bind" else ["party", "side"]
        for key in view_list:
            direction = views[key].normalized()
            cam.location = centre + direction * radius * 4
            cam.rotation_euler = (centre - cam.location).to_track_quat(
                "-Z", "Y").to_euler()
            scene.render.filepath = str(out / ("%s-%s-%s%s.png" % (
                a.name, pose_name, key, "-clay" if a.clay else "")))
            bpy.ops.render.render(write_still=True)
    print("[review] wrote", out)


if __name__ == "__main__":
    main()
