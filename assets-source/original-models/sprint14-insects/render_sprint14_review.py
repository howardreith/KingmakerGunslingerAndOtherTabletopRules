#!/usr/bin/env python3
"""Render the Sprint 14 insect review sheet from one .blend.

The owner's acceleration order moves art iteration offline, with the game
reserved for what offline review cannot prove. That only works if the offline
sheet is actually broad enough to decide something, so this renderer does more
than Sprint 13's four textured angles: it takes clay, unlit, textured and
silhouette shading, a party-camera framing, a top-down view, and synthetic
poses.

The poses are what let the insect hypothesis be judged before a launch is spent
on it. No donor animation leaves the game and none is used here; these are
rotations this script invents, applied to the private preview armature:

- `stride` swings the three weighted leg chains into the insect alternating
  tripod - {L0, R1, L2} forward against {R0, L1, R2} back - which is the pose
  in which "six visible legs and no phantom spider-leg contact" either holds or
  does not. The amplitude is fourteen degrees because the donor's own legs fan
  only eighteen to thirty-three degrees apart, and the first attempt at
  twenty-six crossed adjacent legs into two fans. What the donor's real walk
  cycle does at its own amplitude is a live-review question; what this pose can
  settle is whether the mesh deforms cleanly under a swing at all;
- `wing-stroke` raises the beetle's fourth chain, the one carrying its wings,
  so the wing's deformation at the top of a stroke can be seen rather than
  guessed at.

A posed render still says nothing about contact timing, fades, selection,
collision or crowding, which remain the guarded live review's job.
"""
import argparse
import math
from pathlib import Path
import sys

import bpy
from mathutils import Quaternion, Vector


KINDS = ("giant-ant-soldier", "giant-ant-worker", "fire-beetle")
# The Sprint 14 donor is Z-up in renderer space, unlike the Sprint 12 and 13
# quadrupeds, so the preview root needs no rotation into the review scene at
# all. Its mouthparts sit at negative Y, so that is forward.
FORWARD = Vector((0.0, -1.0, 0.0))
SIDE = Vector((1.0, 0.0, 0.0))
UP = Vector((0.0, 0.0, 1.0))

VIEWS = {
    "three-quarter": (SIDE * 1.00 + FORWARD * 1.25 + UP * 0.55, 58, 1.55),
    "side": (SIDE * 1.65 + UP * 0.25, 58, 1.55),
    "front": (FORWARD * 1.65 + UP * 0.18, 58, 1.55),
    "top": (UP * 1.0 + FORWARD * 0.001, 58, 1.70),
    # Roughly what the game's own camera gives a player: high, angled, and far
    # enough back that only the silhouette and one strong marking survive.
    "party-camera": (SIDE * 0.80 + FORWARD * 0.95 + UP * 1.30, 40, 2.35),
}
SHADINGS = ("textured", "clay", "unlit", "silhouette")
POSES = ("rest", "stride", "wing-stroke")
# Which leg chains the stride swings forward. The other three take the opposite
# swing, which is the insect alternating tripod.
TRIPOD_FORWARD = (("L", "0"), ("R", "1"), ("L", "2"))


def look_at(obj, point):
    obj.rotation_euler = (point - obj.location).to_track_quat(
        "-Z", "Y").to_euler()


def rotate_bone(armature, name, axis, degrees):
    """Rotate one pose bone about an axis given in armature space."""
    bone = armature.pose.bones.get(name)
    if bone is None:
        return False
    rest = armature.data.bones[name].matrix_local.to_3x3()
    local = rest.inverted() @ Vector(axis)
    if local.length < 1e-9:
        return False
    bone.rotation_mode = "QUATERNION"
    bone.rotation_quaternion = Quaternion(local.normalized(),
                                          math.radians(degrees))
    return True


def apply_pose(armature, pose):
    """Pose the preview armature, and report what it actually moved."""
    moved = []
    if pose == "rest":
        return moved
    for prefix in ("L", "R"):
        for index in ("0", "1", "2"):
            swing = 14.0 if (prefix, index) in TRIPOD_FORWARD else -14.0
            # About the up axis, a leg sticking out sideways swings fore and
            # aft, which is the stride itself.
            if rotate_bone(armature, prefix + "_Leg" + index + "_Upper",
                           UP, swing):
                moved.append(prefix + "_Leg" + index + "_Upper")
            # The lift turns about the depth axis, not the side axis. A leg
            # that extends along X and is rotated about X only rolls about its
            # own length and the foot never leaves the ground, which is what
            # the first attempt did; turning about Y is what raises it. The
            # sign flips with the side because the two legs point opposite
            # ways along X.
            lift = 16.0 if (prefix, index) in TRIPOD_FORWARD else -9.0
            if rotate_bone(armature, prefix + "_Leg" + index + "_Lower",
                           Vector((0.0, 1.0, 0.0)),
                           lift * (1.0 if prefix == "L" else -1.0)):
                moved.append(prefix + "_Leg" + index + "_Lower")
    if pose == "wing-stroke":
        for prefix, sign in (("R", -1.0), ("L", 1.0)):
            # About the depth axis a laterally extended chain rises, which is
            # the top of a wing stroke.
            if rotate_bone(armature, prefix + "_Leg3_Upper",
                           Vector((0.0, 1.0, 0.0)), sign * 42.0):
                moved.append(prefix + "_Leg3_Upper")
            if rotate_bone(armature, prefix + "_Leg3_Lower",
                           Vector((0.0, 1.0, 0.0)), sign * 24.0):
                moved.append(prefix + "_Leg3_Lower")
    return moved


def albedo_image(creature):
    for slot in creature.material_slots:
        material = slot.material
        if material is None or not material.use_nodes:
            continue
        for node in material.node_tree.nodes:
            if node.type == "TEX_IMAGE" and node.image is not None:
                return node.image
    return None


def apply_shading(creature, shading):
    """Replace the preview material for the clay, unlit and silhouette sheets."""
    if shading == "textured":
        return
    image = albedo_image(creature) if shading == "unlit" else None
    material = bpy.data.materials.new("Review" + shading.capitalize())
    material.use_nodes = True
    tree = material.node_tree
    for node in list(tree.nodes):
        if node.type != "OUTPUT_MATERIAL":
            tree.nodes.remove(node)
    output = next(node for node in tree.nodes
                  if node.type == "OUTPUT_MATERIAL")
    if shading == "clay":
        # A single matte grey, so the only thing the frame can be read for is
        # form. This is where a buried ear or a creased body shows.
        body = tree.nodes.new("ShaderNodeBsdfDiffuse")
        body.inputs["Color"].default_value = (0.62, 0.60, 0.58, 1.0)
        body.inputs["Roughness"].default_value = 1.0
    elif shading == "unlit":
        # The painting itself, with the lighting taken out of the argument.
        body = tree.nodes.new("ShaderNodeEmission")
        body.inputs["Strength"].default_value = 1.0
        if image is not None:
            texture = tree.nodes.new("ShaderNodeTexImage")
            texture.image = image
            texture.interpolation = "Linear"
            tree.links.new(texture.outputs["Color"], body.inputs["Color"])
        else:
            body.inputs["Color"].default_value = (0.7, 0.7, 0.7, 1.0)
    else:
        # Pure black against a bright world: the outline and nothing else,
        # which is the frame that decides whether the creature is readable at
        # the distance a player sees it from.
        body = tree.nodes.new("ShaderNodeEmission")
        body.inputs["Color"].default_value = (0.0, 0.0, 0.0, 1.0)
        body.inputs["Strength"].default_value = 1.0
    tree.links.new(body.outputs[0], output.inputs["Surface"])
    creature.data.materials.clear()
    creature.data.materials.append(material)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--blend", required=True)
    parser.add_argument("--out-dir", required=True)
    parser.add_argument("--views", default="three-quarter,side,front,top,"
                                           "party-camera")
    parser.add_argument("--shading", default="textured")
    parser.add_argument("--pose", default="rest")
    parser.add_argument("--resolution", type=int, default=768)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:]
                             if "--" in sys.argv else [])
    if args.shading not in SHADINGS:
        raise SystemExit("unknown shading " + args.shading)
    if args.pose not in POSES:
        raise SystemExit("unknown pose " + args.pose)
    requested = [value.strip() for value in args.views.split(",")
                 if value.strip()]
    if not requested or set(requested) - set(VIEWS):
        raise SystemExit("unknown or empty review view set")

    bpy.ops.wm.open_mainfile(filepath=str(Path(args.blend).resolve()))
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    if len(meshes) != 1:
        raise SystemExit("review file must contain exactly one creature mesh")
    creature = meshes[0]
    if creature.name not in KINDS:
        raise SystemExit("unknown Sprint 14 creature " + creature.name)
    armatures = [obj for obj in bpy.context.scene.objects
                 if obj.type == "ARMATURE"]
    if len(armatures) != 1:
        raise SystemExit("review file must contain one preview armature")
    armature = armatures[0]
    moved = apply_pose(armature, args.pose)
    if args.pose != "rest" and not moved:
        raise SystemExit("pose " + args.pose + " moved no bone")
    apply_shading(creature, args.shading)
    bpy.context.view_layer.update()

    evaluated = creature.evaluated_get(bpy.context.evaluated_depsgraph_get())
    posed = evaluated.to_mesh()
    corners = [creature.matrix_world @ vertex.co for vertex in posed.vertices]
    evaluated.to_mesh_clear()
    minimum = Vector((min(point.x for point in corners),
                      min(point.y for point in corners),
                      min(point.z for point in corners)))
    maximum = Vector((max(point.x for point in corners),
                      max(point.y for point in corners),
                      max(point.z for point in corners)))
    centre = (minimum + maximum) * 0.5
    extent = max((maximum - minimum).length, 0.1)

    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    scene.render.resolution_x = args.resolution
    scene.render.resolution_y = args.resolution
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    if scene.world is None:
        scene.world = bpy.data.worlds.new("ReviewWorld")
    if args.shading == "silhouette":
        scene.world.color = (0.88, 0.90, 0.93)
    elif args.shading == "unlit":
        scene.world.color = (0.02, 0.02, 0.025)
    else:
        scene.world.color = (0.055, 0.065, 0.075)

    if args.shading in ("textured", "clay"):
        bpy.ops.mesh.primitive_plane_add(size=extent * 4.0)
        floor = bpy.context.object
        floor.name = "ReviewFloor"
        floor.location = Vector((centre.x, centre.y,
                                 minimum.z - extent * 0.015))
        floor_material = bpy.data.materials.new("ReviewFloorMaterial")
        floor_material.diffuse_color = (0.18, 0.20, 0.22, 1.0)
        floor.data.materials.append(floor_material)
        for index, (offset, energy, size) in enumerate((
                (SIDE * 0.8 + FORWARD * 0.6 + UP * 1.5, 1100.0, 5.0),
                (-SIDE * 1.2 - FORWARD * 0.2 + UP * 0.8, 700.0, 4.0))):
            light_data = bpy.data.lights.new("ReviewLight" + str(index),
                                             "AREA")
            light_data.energy = energy * max(1.0, (extent / 3.0) ** 2)
            light_data.shape = "DISK"
            light_data.size = extent * size
            light = bpy.data.objects.new("ReviewLight" + str(index),
                                         light_data)
            bpy.context.collection.objects.link(light)
            light.location = centre + offset.normalized() * extent * 1.8
            look_at(light, centre)

    camera_data = bpy.data.cameras.new("ReviewCamera")
    camera = bpy.data.objects.new("ReviewCamera", camera_data)
    bpy.context.collection.objects.link(camera)
    scene.camera = camera
    written = []
    for name in requested:
        direction, lens, distance = VIEWS[name]
        camera.data.lens = lens
        camera.location = centre + direction.normalized() * extent * distance
        look_at(camera, centre)
        out = (Path(args.out_dir) / (creature.name + "-" + args.pose + "-" +
                                     args.shading + "-" + name + ".png"))
        out.parent.mkdir(parents=True, exist_ok=True)
        scene.render.filepath = str(out.resolve())
        bpy.ops.render.render(write_still=True)
        written.append(out.name)
    print("[sprint14-insect] rendered " + creature.name + " pose=" +
          args.pose + " shading=" + args.shading + " posedBones=" +
          str(len(moved)) + " views=" + ",".join(written))


if __name__ == "__main__":
    main()
