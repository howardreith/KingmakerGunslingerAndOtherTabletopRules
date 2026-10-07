#!/usr/bin/env python3
"""Render deterministic clear-floor rest-pose review views from one .blend."""
import argparse
import math
from pathlib import Path
import sys

import bpy
from mathutils import Vector


def look_at(camera, point):
    camera.rotation_euler = (point - camera.location).to_track_quat("-Z", "Y").to_euler()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--blend", required=True)
    parser.add_argument("--out-dir", required=True)
    parser.add_argument("--views",
                        default="three-quarter,side,front,high")
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:]
                             if "--" in sys.argv else [])
    bpy.ops.wm.open_mainfile(filepath=str(Path(args.blend).resolve()))
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    if len(meshes) != 1:
        raise SystemExit("review file must contain exactly one creature mesh")
    creature = meshes[0]
    dog_frame = creature.name == "dire-rat"
    if not dog_frame:
        armatures = [obj for obj in bpy.context.scene.objects
                     if obj.type == "ARMATURE"]
        if len(armatures) != 1:
            raise SystemExit("review file must contain one preview armature")
        # Wolf/Worg renderer space is Y-up. Rotate the private preview root
        # into Blender's Z-up review scene; shipped vertices stay untouched.
        armatures[0].rotation_euler.x = math.pi * 0.5
        bpy.context.view_layer.update()
    corners = [creature.matrix_world @ Vector(corner)
               for corner in creature.bound_box]
    minimum = Vector((min(point.x for point in corners),
                      min(point.y for point in corners),
                      min(point.z for point in corners)))
    maximum = Vector((max(point.x for point in corners),
                      max(point.y for point in corners),
                      max(point.z for point in corners)))
    centre = (minimum + maximum) * 0.5
    extent = max((maximum - minimum).length, 0.1)
    side = Vector((1.0, 0.0, 0.0))
    up = Vector((0.0, 0.0, 1.0))
    forward = Vector((0.0, -1.0, 0.0)) if dog_frame else Vector((0.0, 1.0, 0.0))

    bpy.context.scene.render.engine = "BLENDER_EEVEE_NEXT"
    bpy.context.scene.render.resolution_x = 768
    bpy.context.scene.render.resolution_y = 768
    bpy.context.scene.render.resolution_percentage = 100
    bpy.context.scene.render.image_settings.file_format = "PNG"
    bpy.context.scene.render.film_transparent = False
    if bpy.context.scene.world is None:
        bpy.context.scene.world = bpy.data.worlds.new("ReviewWorld")
    bpy.context.scene.world.color = (0.055, 0.065, 0.075)
    bpy.ops.mesh.primitive_plane_add(size=extent * 4.0)
    floor = bpy.context.object
    floor.name = "ReviewFloor"
    floor.location = Vector((centre.x, centre.y, minimum.z - extent * 0.015))
    floor_material = bpy.data.materials.new("ReviewFloorMaterial")
    floor_material.diffuse_color = (0.18, 0.20, 0.22, 1.0)
    floor.data.materials.append(floor_material)

    for index, (offset, energy, size) in enumerate((
            (side * 0.8 + forward * 0.6 + up * 1.5, 1100.0, 5.0),
            (-side * 1.2 - forward * 0.2 + up * 0.8, 700.0, 4.0))):
        light_data = bpy.data.lights.new("ReviewLight" + str(index), "AREA")
        # The Dog donor works in centimetre-scale coordinates while Wolf/Worg
        # use metre-scale values. Area-light power follows the square of scale.
        light_data.energy = energy * max(1.0, (extent / 3.0) ** 2)
        light_data.shape = "DISK"
        light_data.size = extent * size
        light = bpy.data.objects.new("ReviewLight" + str(index), light_data)
        bpy.context.collection.objects.link(light)
        light.location = centre + offset.normalized() * extent * 1.8
        look_at(light, centre)
    camera_data = bpy.data.cameras.new("ReviewCamera")
    camera = bpy.data.objects.new("ReviewCamera", camera_data)
    bpy.context.collection.objects.link(camera)
    camera.data.lens = 58
    bpy.context.scene.camera = camera

    views = (("three-quarter", side * 1.0 + forward * 1.25 + up * 0.55),
             ("side", side * 1.65 + up * 0.25),
             ("front", forward * 1.65 + up * 0.18),
             ("high", side * 0.65 + forward * 0.70 + up * 1.55))
    requested = {value.strip() for value in args.views.split(",") if value.strip()}
    known = {name for name, _ in views}
    if not requested or requested - known:
        raise SystemExit("unknown or empty review view set")
    for name, direction in views:
        if name not in requested:
            continue
        camera.location = centre + direction.normalized() * extent * 1.55
        look_at(camera, centre)
        out = Path(args.out_dir) / (creature.name + "-" + name + ".png")
        out.parent.mkdir(parents=True, exist_ok=True)
        bpy.context.scene.render.filepath = str(out.resolve())
        bpy.ops.render.render(write_still=True)
    print("[sprint12-quadruped] rendered " + creature.name)


if __name__ == "__main__":
    main()
