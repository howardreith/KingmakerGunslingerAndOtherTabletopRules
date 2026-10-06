#!/usr/bin/env python3
"""Render original geometry in private measured poses, not native game assets.

The flat review plane uses the captured floor height. These diagnostic images
are supporting art only and never replace native lifecycle/contact testing.
"""
import argparse
from pathlib import Path
import sys

import bpy
from mathutils import Vector

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import captured_pose_review as observed
from render_snake_review import framing, look_at, apply_shading, VIEWS


def render(args, row, sample, label, shading):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    original = observed.decode_mesh(Path(args.mesh_directory) / (row["key"] + "-mesh.json"))
    support = observed.HYBRID_SUPPORT in original["payload"]["bones"]
    if support and row["key"] != "salamander":
        raise ValueError("support driver on non-hybrid original mesh")
    points = observed.replay(original["vertices"], original["weights"], sample, support)
    origin = sample["actorPosition"]
    # Unity world +Y up -> Blender +Z up. The axis swap reverses handedness;
    # reverse the Unity triangles too, preserving the original outer surface.
    points = [Vector((p[0] - origin[0], p[2] - origin[2], p[1] - origin[1])) for p in points]
    mesh = bpy.data.meshes.new("OriginalCapturedPose")
    mesh.from_pydata(points, [], [(a, c, b) for a, b, c in original["faces"]])
    creature = bpy.data.objects.new(row["key"], mesh)
    bpy.context.collection.objects.link(creature)
    uv = mesh.uv_layers.new(name="OriginalAlbedo")
    for loop in mesh.loops:
        uv.data[loop.index].uv = original["uv"][loop.vertex_index]
    material = bpy.data.materials.new("OriginalAlbedo")
    material.use_nodes = True
    shader = material.node_tree.nodes.get("Principled BSDF")
    texture = material.node_tree.nodes.new("ShaderNodeTexImage")
    texture.image = bpy.data.images.load(str((Path(args.mesh_directory) / original["payload"]["albedo"]["file"]).resolve()))
    material.node_tree.links.new(texture.outputs["Color"], shader.inputs["Base Color"])
    shader.inputs["Roughness"].default_value = .8
    creature.data.materials.append(material)
    for polygon in mesh.polygons:
        polygon.use_smooth = True
    apply_shading(creature, shading)
    low = Vector([min(p[i] for p in points) for i in range(3)])
    high = Vector([max(p[i] for p in points) for i in range(3)])
    centre, extent = (low + high) / 2, max((high - low).length, .1)
    plane_height = sample["lowestVertexFloor"]["hitPoint"][1] - origin[1]
    bpy.ops.mesh.primitive_plane_add(size=extent * 5, location=(centre.x, centre.y, plane_height))
    plane = bpy.context.object
    plane.name = "MeasuredFlatFloorDiagnostic"
    floor_material = bpy.data.materials.new("DiagnosticFloor")
    floor_material.diffuse_color = (.18, .19, .20, 1)
    plane.data.materials.append(floor_material)
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    scene.eevee.taa_render_samples = 32
    scene.render.resolution_x = scene.render.resolution_y = 640
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.world = bpy.data.worlds.new("OriginalPoseDiagnostic")
    scene.world.color = (.12, .13, .14)
    scene.view_settings.view_transform = "Standard"
    for index, direction in enumerate((Vector((1, -1, 2)), Vector((-1, .2, 1)))):
        light = bpy.data.lights.new("ReviewLight" + str(index), "AREA")
        light.energy = (270 if index == 0 else 180) * (extent / 4) ** 2
        light.size = extent * 1.2
        obj = bpy.data.objects.new(light.name, light)
        bpy.context.collection.objects.link(obj)
        obj.location = centre + direction.normalized() * extent * 1.4
        look_at(obj, centre)
    camera = bpy.data.objects.new("DiagnosticCamera", bpy.data.cameras.new("DiagnosticCamera"))
    bpy.context.collection.objects.link(camera)
    scene.camera = camera
    camera.data.type = "ORTHO"
    for view in args.views.split(","):
        direction = VIEWS[view]
        camera.location = centre + direction.normalized() * extent * 2
        framed, size = framing(points, camera)
        camera.location = framed + direction.normalized() * extent * 2
        look_at(camera, framed)
        camera.data.ortho_scale = size
        output = Path(args.out_dir) / (row["key"] + "-" + label + "-" + shading + "-" + view + ".png")
        output.parent.mkdir(parents=True, exist_ok=True)
        scene.render.filepath = str(output.resolve())
        bpy.ops.render.render(write_still=True)


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "mesh-directory", "out-dir"):
        parser.add_argument("--" + name, required=True)
    parser.add_argument("--key", choices=observed.KEYS)
    parser.add_argument("--views", default="three-quarter,side,top")
    parser.add_argument("--attack-poses", action="store_true",
                        help="Also review the first, lowest captured and last actual attack poses.")
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    for row in observed.capture_rows(args.capture):
        if args.key and args.key != row["key"]:
            continue
        for label, sample in (("idle", row["idleSample"]), ("moving", row["movementSamples"][0]),
                              ("settled", row["movementSamples"][-1])):
            render(args, row, sample, label, "clay")
        render(args, row, row["idleSample"], "idle", "textured")
        if args.attack_poses:
            samples = row.get("attackPoseSamples", [])
            if not samples:
                raise ValueError("no measured attack poses; never synthesize missing attack evidence")
            worst = min(samples, key=lambda sample: sample["lowestVertexFloor"]["clearance"])
            for label, sample in (("attack-first", samples[0]), ("attack-lowest", worst),
                                  ("attack-last", samples[-1])):
                render(args, row, sample, label, "clay")
            render(args, row, worst, "attack-lowest", "textured")


if __name__ == "__main__":
    main()
