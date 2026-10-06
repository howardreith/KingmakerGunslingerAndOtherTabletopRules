#!/usr/bin/env python3
"""Private original snake review; synthetic poses are not native animation proof."""
import argparse
import json
import math
from pathlib import Path
import sys

import bpy
from mathutils import Vector

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "sprint14-insects"))
from render_sprint14_review import apply_shading, look_at, rotate_bone

SIDE, UP, FORWARD = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, -1, 0))
VIEWS = {"three-quarter": SIDE + FORWARD + UP * .65,
         "side": SIDE * 1.8 + UP * .22, "top": UP + FORWARD * .001,
         "party-camera": SIDE * .8 + FORWARD * .95 + UP * 1.4}


def render(args, shading, pose):
    bpy.ops.wm.open_mainfile(filepath=str(Path(args.blend).resolve()))
    creature = next(obj for obj in bpy.context.scene.objects if obj.type == "MESH")
    armature = next(obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE")
    if pose == "jaw-open":
        if not rotate_bone(armature, "Jaw_M", (1, 0, 0), 32):
            raise SystemExit("jaw pose moved no joint")
    elif pose in ("neck-left", "neck-right"):
        sign = 1 if pose == "neck-left" else -1
        for name in ("SpineA_M", "Spine1_M", "SpineB_M", "UpperTorso", "Neck_M"):
            if not rotate_bone(armature, name, (0, 1, 0), sign * 10):
                raise SystemExit("missing neck joint " + name)
    elif pose != "rest":
        raise SystemExit("unsupported synthetic pose")
    # Renderer +Y-up/+Z-forward becomes Blender +Z-up/-Y-forward.
    armature.rotation_euler.x = math.pi / 2
    apply_shading(creature, shading)
    bpy.context.view_layer.update()
    evaluated = creature.evaluated_get(bpy.context.evaluated_depsgraph_get())
    mesh = evaluated.to_mesh()
    points = [creature.matrix_world @ vertex.co for vertex in mesh.vertices]
    evaluated.to_mesh_clear()
    minimum = Vector([min(p[i] for p in points) for i in range(3)])
    maximum = Vector([max(p[i] for p in points) for i in range(3)])
    centre = (minimum + maximum) / 2
    extent = max((maximum - minimum).length, .1)
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    scene.eevee.taa_render_samples = 32
    scene.render.resolution_x = scene.render.resolution_y = args.resolution
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    scene.world = bpy.data.worlds.new("SnakeReviewWorld")
    scene.world.color = (.8, .8, .8) if shading == "silhouette" else (.08, .09, .10)
    scene.view_settings.view_transform = "Standard"
    if shading in ("clay", "textured"):
        bpy.ops.mesh.primitive_plane_add(size=extent * 6,
            location=(centre.x, centre.y, minimum.z - .012))
        floor = bpy.context.object
        material = bpy.data.materials.new("ReviewFloor")
        material.diffuse_color = (.19, .20, .21, 1)
        floor.data.materials.append(material)
        for index, direction in enumerate((SIDE + FORWARD + UP * 2, -SIDE + UP)):
            light = bpy.data.lights.new("ReviewLight" + str(index), "AREA")
            light.energy = (1800 if index == 0 else 1200) * (extent / 4) ** 2
            if shading == "clay":
                light.energy *= .15
            light.size = extent * 1.2
            obj = bpy.data.objects.new(light.name, light)
            bpy.context.collection.objects.link(obj)
            obj.location = centre + direction.normalized() * extent * 1.4
            look_at(obj, centre)
    camera = bpy.data.objects.new("ReviewCamera", bpy.data.cameras.new("ReviewCamera"))
    bpy.context.collection.objects.link(camera)
    scene.camera = camera
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = extent * .95
    output = Path(args.out_dir)
    output.mkdir(parents=True, exist_ok=True)
    written = []
    for view in args.views.split(","):
        if view not in VIEWS:
            raise SystemExit("unsupported view " + view)
        camera.location = centre + VIEWS[view].normalized() * extent * 2
        look_at(camera, centre)
        path = output / ("%s-%s-%s-%s.png" % (creature.name, shading, pose, view))
        scene.render.filepath = str(path.resolve())
        bpy.ops.render.render(write_still=True)
        written.append(path.name)
    print("[snake-review] " + json.dumps(dict(files=written,
          warning="synthetic offline poses; no runtime timing/contact qualification")))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--blend", required=True)
    parser.add_argument("--out-dir", required=True)
    parser.add_argument("--shading", choices=("clay", "silhouette", "textured", "unlit"),
                        default="textured")
    parser.add_argument("--pose", default="rest")
    parser.add_argument("--views", default="three-quarter,side,top,party-camera")
    parser.add_argument("--resolution", type=int, default=640)
    parser.add_argument("--suite", action="store_true")
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    if args.suite:
        for shading in ("clay", "silhouette", "textured", "unlit"):
            render(args, shading, "rest")
        args.views = "three-quarter,side"
        render(args, "textured", "jaw-open")
        args.views = "top"
        for pose in ("neck-left", "neck-right"):
            render(args, "textured", pose)
    else:
        render(args, args.shading, args.pose)


if __name__ == "__main__":
    main()
