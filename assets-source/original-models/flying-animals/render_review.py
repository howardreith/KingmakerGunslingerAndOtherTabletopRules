#!/usr/bin/env python3
"""Render private bind-frame source from party-like angles for art review.

This is a visual inspection tool only. It does not establish live animation,
attack contact, targeting, or Kingmaker material correctness.
"""
import argparse
from pathlib import Path
import sys

import bpy
from mathutils import Vector


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--blend", required=True)
    parser.add_argument("--out", required=True)
    parser.add_argument("--view", choices=("top", "front", "side"),
                        default="top")
    parser.add_argument("--scale", type=float, default=11.5)
    parser.add_argument("--rest-mesh", action="store_true",
                        help="show authored vertices without the Blender pose")
    args = parser.parse_args(argv)
    bpy.ops.wm.open_mainfile(filepath=str(Path(args.blend).resolve()))
    if args.rest_mesh:
        for obj in bpy.data.objects:
            for modifier in obj.modifiers:
                if modifier.type == "ARMATURE":
                    modifier.show_render = False
    scene = bpy.context.scene
    scene.render.engine = "CYCLES"
    scene.cycles.samples = 32
    scene.render.resolution_x = 960
    scene.render.resolution_y = 960
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    world = bpy.data.worlds.new("NeutralReviewWorld")
    scene.world = world
    world.use_nodes = True
    world.node_tree.nodes.get("Background").inputs[0].default_value = (
        0.12, 0.14, 0.16, 1.0)
    world.node_tree.nodes.get("Background").inputs[1].default_value = 0.7
    camera_data = bpy.data.cameras.new("ReviewCamera")
    camera = bpy.data.objects.new("ReviewCamera", camera_data)
    scene.collection.objects.link(camera)
    target = Vector((0.0, 1.7, 0.0))
    positions = {
        "top": (0.0, 9.0, -5.0),
        "front": (0.0, 3.2, -9.0),
        "side": (9.0, 3.5, 0.0),
    }
    camera.location = positions[args.view]
    camera.rotation_euler = (target - camera.location).to_track_quat(
        "-Z", "Y").to_euler()
    camera_data.type = "ORTHO"
    camera_data.ortho_scale = args.scale
    scene.camera = camera
    for name, location, energy, size in (
            ("Key", (2.0, 8.0, -2.0), 1400, 7),
            ("Fill", (-5.0, 5.0, 2.0), 900, 6)):
        data = bpy.data.lights.new(name, "AREA")
        data.energy = energy
        data.shape = "DISK"
        data.size = size
        obj = bpy.data.objects.new(name, data)
        scene.collection.objects.link(obj)
        obj.location = location
        obj.rotation_euler = (target - obj.location).to_track_quat(
            "-Z", "Y").to_euler()
    scene.render.filepath = str(Path(args.out).resolve())
    bpy.ops.render.render(write_still=True)
    print("[review] " + args.view + " " + args.out)


if __name__ == "__main__":
    main()
