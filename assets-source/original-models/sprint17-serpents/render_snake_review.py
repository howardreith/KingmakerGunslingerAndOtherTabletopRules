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
         "party-camera": SIDE * .8 + FORWARD * .95 + UP * 1.4,
         "jaw-closeup": SIDE * 1.6 + FORWARD * .45 + UP * .30,
         "head-top": UP + FORWARD * .001}


def framing(points, camera):
    """Fit all evaluated vertices, including long/narrow and posed silhouettes."""
    low = Vector([min(p[i] for p in points) for i in range(3)])
    high = Vector([max(p[i] for p in points) for i in range(3)])
    centre = (low + high) / 2
    look_at(camera, centre)
    axes = camera.rotation_euler.to_matrix()
    right, up = axes @ Vector((1, 0, 0)), axes @ Vector((0, 1, 0))
    diameter = max(2 * abs((point - centre).dot(axis))
                   for point in points for axis in (right, up))
    return centre, max(diameter * 1.12, .01)


def render(args, shading, pose):
    bpy.ops.wm.open_mainfile(filepath=str(Path(args.blend).resolve()))
    creature = next(obj for obj in bpy.context.scene.objects if obj.type == "MESH")
    armature = next(obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE")
    worm = "Hips_Joints" in armature.pose.bones
    hybrid = "Torso_Lower" in armature.pose.bones
    if pose == "jaw-open":
        if not rotate_bone(armature, "Jaw_Down" if worm else "jaw" if hybrid else "Jaw_M",
                           (1, 0, 0), -32 if worm or hybrid else 32):
            raise SystemExit("jaw pose moved no joint")
    elif pose in ("neck-left", "neck-right", "tail-left", "tail-right"):
        sign = 1 if pose.endswith("left") else -1
        chain = (tuple("Body0" + str(i) for i in range(3, 15)) if worm else
                 ("tail", "tail1", "tail2", "tail3") if hybrid else
                 ("SpineA_M", "Spine1_M", "SpineB_M", "UpperTorso", "Neck_M"))
        for name in chain:
            if not rotate_bone(armature, name, (0, 0, 1) if worm or hybrid else (0, 1, 0), sign * 10):
                raise SystemExit("missing neck joint " + name)
    elif pose == "arms-forward" and hybrid:
        for side in ("L", "R"):
            for part, angle in (("Arm_Upper", 40), ("Arm_Lower", 35)):
                if not rotate_bone(armature, side + "_" + part, (1, 0, 0), angle):
                    raise SystemExit("missing arm joint")
    elif pose != "rest":
        raise SystemExit("unsupported synthetic pose")
    # Display both measured frames with +Z up/-Y forward. These are private
    # review transforms, never exported mesh transforms or native animation.
    if worm or hybrid:
        armature.rotation_euler.z = math.pi
    else:
        armature.rotation_euler.x = math.pi / 2
    apply_shading(creature, shading)
    bpy.context.view_layer.update()
    evaluated = creature.evaluated_get(bpy.context.evaluated_depsgraph_get())
    mesh = evaluated.to_mesh()
    points = [creature.matrix_world @ vertex.co for vertex in mesh.vertices]
    jaw_groups = {group.index for group in creature.vertex_groups
                  if group.name in ("Head", "Jaw_M", "Jaw_Down", "jaw", "jaw1")}
    head_points = [points[v.index] for v in creature.data.vertices
                   if any(g.group in jaw_groups and g.weight >= .5 for g in v.groups)]
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
            light.energy = (270 if index == 0 else 180) * (extent / 4) ** 2
            light.size = extent * 1.2
            obj = bpy.data.objects.new(light.name, light)
            bpy.context.collection.objects.link(obj)
            obj.location = centre + direction.normalized() * extent * 1.4
            look_at(obj, centre)
    camera = bpy.data.objects.new("ReviewCamera", bpy.data.cameras.new("ReviewCamera"))
    bpy.context.collection.objects.link(camera)
    scene.camera = camera
    camera.data.type = "ORTHO"
    output = Path(args.out_dir)
    output.mkdir(parents=True, exist_ok=True)
    written = []
    for view in args.views.split(","):
        if view not in VIEWS:
            raise SystemExit("unsupported view " + view)
        camera.location = centre + VIEWS[view].normalized() * extent * 2
        framing_points = head_points if view in ("jaw-closeup", "head-top") else points
        frame_centre, scale = framing(framing_points, camera)
        camera.location = frame_centre + VIEWS[view].normalized() * extent * 2
        _, camera.data.ortho_scale = framing(framing_points, camera)
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
        # The saved body decides which stress poses exist; do not invent a
        # shared snake/humanoid rig or imply these are native animations.
        hybrid = "Torso_Lower" in next(obj for obj in bpy.context.scene.objects
                                       if obj.type == "ARMATURE").pose.bones
        for pose in (("tail-left", "tail-right") if hybrid else ("neck-left", "neck-right")):
            render(args, "textured", pose)
        if hybrid:
            args.views = "three-quarter,side"
            render(args, "textured", "arms-forward")
        args.views = "jaw-closeup,head-top"
        render(args, "unlit", "rest")
        render(args, "textured", "jaw-open")
    else:
        render(args, args.shading, args.pose)


if __name__ == "__main__":
    main()
