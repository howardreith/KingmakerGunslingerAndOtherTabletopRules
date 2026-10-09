#!/usr/bin/env python3
"""Offline clay/silhouette/paint/pose review; no native animation is exported.

Synthetic jaw, stride and tail stress poses check binding, not live animation
timing or contact. Those remain guarded runtime acceptance items.
"""
import argparse
import copy
import json
import math
from pathlib import Path
import sys

import bpy
from mathutils import Vector

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "sprint14-insects"))
from render_sprint14_review import apply_shading, look_at, rotate_bone

SIDE, UP, FORWARD = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, 1, 0))
VIEWS = {
    "three-quarter": SIDE * 1.1 + FORWARD * 1.0 + UP * .8,
    "side": SIDE * 1.8 + UP * .27,
    "top": UP + FORWARD * .001,
    "party-camera": SIDE * .8 + FORWARD * .95 + UP * 1.4,
}


def pose(armature, name):
    moved = []
    if name == "jaw-open":
        if rotate_bone(armature, "cent_jaw1_jnt", (1, 0, 0), -32):
            moved.append("cent_jaw1_jnt")
    elif name in ("tail-left", "tail-right"):
        sign = 1 if name == "tail-left" else -1
        for index in range(1, 8):
            bone = "cent_tail%d_jnt" % index
            if rotate_bone(armature, bone, (0, 1, 0), sign * 13):
                moved.append(bone)
    elif name == "stride":
        for prefix, sign in (("left", 1), ("right", -1)):
            for joint, amount in (("_hand1_jnt", 17), ("_leg1_jnt", -17),
                                  ("_hand2_jnt", -12), ("_leg2_jnt", 12)):
                bone = prefix + joint
                if rotate_bone(armature, bone, (0, 1, 0), sign * amount):
                    moved.append(bone)
    elif name != "rest":
        raise SystemExit("unknown pose " + name)
    return moved


def setup(args):
    bpy.ops.wm.open_mainfile(filepath=str(Path(args.blend).resolve()))
    creature = next(obj for obj in bpy.context.scene.objects if obj.type == "MESH")
    armature = next(obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE")
    moved = pose(armature, args.pose)
    if args.pose != "rest" and not moved:
        raise SystemExit("pose moved no bones")
    # Renderer-local Y-up becomes Blender Z-up; -Z-forward becomes +Y.
    armature.rotation_euler.x = math.pi / 2
    apply_shading(creature, args.shading)
    objects = [creature]
    if args.compare_blend:
        if args.pose != "rest":
            raise SystemExit("scale comparison must use rest pose")
        with bpy.data.libraries.load(str(Path(args.compare_blend).resolve())) as (source, target):
            target.objects = source.objects
        for obj in target.objects:
            if obj is not None:
                bpy.context.collection.objects.link(obj)
                if obj.type == "ARMATURE":
                    obj.rotation_euler.x = math.pi / 2
                    obj.scale = (args.compare_scale,) * 3
                    obj.location.x = 2.7
                if obj.type == "MESH":
                    objects.append(obj)
                    apply_shading(obj, args.shading)
        armature.location.x = -1.1
    bpy.context.view_layer.update()
    points = []
    for obj in objects:
        evaluated = obj.evaluated_get(bpy.context.evaluated_depsgraph_get())
        mesh = evaluated.to_mesh()
        points.extend(obj.matrix_world @ vertex.co for vertex in mesh.vertices)
        evaluated.to_mesh_clear()
    minimum = Vector([min(p[i] for p in points) for i in range(3)])
    maximum = Vector([max(p[i] for p in points) for i in range(3)])
    centre = (minimum + maximum) / 2
    extent = max((maximum - minimum).length, .1)
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    scene.eevee.taa_render_samples = 32
    scene.render.resolution_x = args.resolution
    scene.render.resolution_y = args.resolution
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    scene.world = bpy.data.worlds.new("CrocodilianReviewWorld")
    scene.world.color = (.8, .8, .8) if args.shading == "silhouette" else (.08, .09, .10)
    scene.view_settings.view_transform = "Standard"
    if args.shading in ("clay", "textured"):
        bpy.ops.mesh.primitive_plane_add(size=extent * 6, location=(centre.x, centre.y, minimum.z - .012))
        floor = bpy.context.object
        material = bpy.data.materials.new("ReviewFloor")
        material.diffuse_color = (.19, .20, .21, 1)
        floor.data.materials.append(material)
        for index, direction in enumerate((SIDE + FORWARD + UP * 2, -SIDE + UP)):
            data = bpy.data.lights.new("ReviewLight" + str(index), "AREA")
            data.energy = (1800 if index == 0 else 1200) * (extent / 4) ** 2
            if args.shading == "clay":
                data.energy *= .15
            data.size = extent * 1.2
            light = bpy.data.objects.new(data.name, data)
            bpy.context.collection.objects.link(light)
            light.location = centre + direction.normalized() * extent * 1.4
            look_at(light, centre)
    camera = bpy.data.objects.new("ReviewCamera", bpy.data.cameras.new("ReviewCamera"))
    bpy.context.collection.objects.link(camera)
    scene.camera = camera
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = extent * (.95 if not args.compare_blend else 1.0)
    out = Path(args.out_dir)
    out.mkdir(parents=True, exist_ok=True)
    written = []
    for view in args.views.split(","):
        if view not in VIEWS:
            raise SystemExit("unknown view " + view)
        camera.location = centre + VIEWS[view].normalized() * extent * 2
        look_at(camera, centre)
        label = "scale-comparison" if args.compare_blend else creature.name
        path = out / ("%s-%s-%s-%s.png" % (label, args.shading, args.pose, view))
        scene.render.filepath = str(path.resolve())
        bpy.ops.render.render(write_still=True)
        written.append(path.name)
    print("[crocodilian-review] " + json.dumps(dict(
        files=written, pose=args.pose, moved=moved, compareScale=args.compare_scale,
        warning="synthetic offline poses; not runtime timing or contact evidence")))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--blend", required=True)
    parser.add_argument("--out-dir", required=True)
    parser.add_argument("--shading", choices=("clay", "silhouette", "textured", "unlit"), default="textured")
    parser.add_argument("--pose", default="rest")
    parser.add_argument("--views", default="three-quarter,side,top,party-camera")
    parser.add_argument("--resolution", type=int, default=768)
    parser.add_argument("--compare-blend")
    parser.add_argument("--compare-scale", type=float, default=2.0)
    parser.add_argument("--suite", action="store_true")
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    if not args.suite:
        setup(args)
        return
    directory = Path(args.blend).parent
    for kind in ("crocodile", "dire-crocodile"):
        for shading in ("clay", "silhouette", "textured", "unlit"):
            cell = copy.copy(args)
            cell.blend = str(directory / (kind + ".blend"))
            cell.shading, cell.pose = shading, "rest"
            setup(cell)
        for state in ("jaw-open", "tail-left", "tail-right", "stride"):
            cell = copy.copy(args)
            cell.blend = str(directory / (kind + ".blend"))
            cell.shading, cell.pose = "textured", state
            cell.views = "three-quarter,side" if state == "jaw-open" else "top"
            setup(cell)
    cell = copy.copy(args)
    cell.blend = str(directory / "crocodile.blend")
    cell.compare_blend = str(directory / "dire-crocodile.blend")
    cell.shading, cell.pose = "textured", "rest"
    cell.views = "three-quarter,top"
    setup(cell)


if __name__ == "__main__":
    main()
