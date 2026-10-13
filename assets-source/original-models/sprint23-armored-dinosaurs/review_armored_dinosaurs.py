"""Offline shape, paint and deterministic pose review for Sprint 23 bodies."""
import argparse
import math
from pathlib import Path
import sys

import bpy
from mathutils import Matrix, Quaternion, Vector


def point_camera(camera, target):
    back = (camera.location - target).normalized()
    up = Vector((0, 1, 0)) if abs(back.y) < .98 else Vector((0, 0, 1))
    right = up.cross(back).normalized()
    up = back.cross(right).normalized()
    camera.rotation_euler = Matrix((right, up, back)).transposed().to_euler()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", choices=("triceratops", "stegosaurus"), required=True)
    parser.add_argument("--source", required=True)
    parser.add_argument("--out", required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.open_mainfile(filepath=str(Path(args.source).resolve()))
    title = args.kind.title()
    obj = bpy.data.objects[title]
    arm = bpy.data.objects[title + "EditableRig"]
    scene = bpy.context.scene
    for polygon in obj.data.polygons:
        polygon.use_smooth = True
    camera_data = bpy.data.cameras.new("ReviewCamera")
    camera = bpy.data.objects.new("ReviewCamera", camera_data)
    scene.collection.objects.link(camera)
    scene.camera = camera
    camera_data.type = "ORTHO"
    scene.render.resolution_x = 1000
    scene.render.resolution_y = 720
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.world = bpy.data.worlds.new("OriginalReviewWorld")
    scene.world.color = (.12, .12, .12)
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    for name, location, energy, size in (
            ("Key", (6, 8, -7), 1100, 6), ("Fill", (-5, 5, 6), 700, 5)):
        data = bpy.data.lights.new(name, "AREA")
        data.energy, data.shape, data.size = energy, "DISK", size
        light = bpy.data.objects.new(name, data)
        scene.collection.objects.link(light)
        light.location = location
        light.rotation_euler = (Vector((0, 2, 0)) - light.location).to_track_quat("-Z", "Y").to_euler()
    original = obj.active_material
    clay = bpy.data.materials.new("OriginalClay")
    clay.diffuse_color = (.48, .49, .43, 1)
    silhouette = bpy.data.materials.new("OriginalSilhouette")
    silhouette.diffuse_color = (.018, .018, .018, 1)
    unlit = bpy.data.materials.new("OriginalUnlit")
    unlit.use_nodes = True
    unlit.node_tree.nodes.clear()
    image = next(image for image in bpy.data.images
                 if image.filepath and image.filepath.lower().endswith("-albedo.png"))
    texture = unlit.node_tree.nodes.new("ShaderNodeTexImage")
    texture.image = image
    emission = unlit.node_tree.nodes.new("ShaderNodeEmission")
    output = unlit.node_tree.nodes.new("ShaderNodeOutputMaterial")
    unlit.node_tree.links.new(texture.outputs["Color"], emission.inputs["Color"])
    unlit.node_tree.links.new(emission.outputs[0], output.inputs["Surface"])
    out = Path(args.out).resolve()
    out.mkdir(parents=True, exist_ok=True)
    if args.kind == "triceratops":
        target, scale = Vector((0, 2.45, 0)), 8.6
        shots = [
            ("clay-quarter", (6, 6, -7), clay, 0),
            ("silhouette-side", (7, 3.2, 0), silhouette, 0),
            ("textured-quarter", (-6, 6, -7), original, 0),
            ("unlit-top", (0, 10, 1), unlit, 0),
            ("party-camera", (7, 10, 8), original, 0),
            ("walk-extreme", (7, 5, -6), clay, 1),
            ("turn-extreme", (-7, 5, -5), clay, 2),
            ("horn-contact", (6, 4, -8), original, 3)]
        front, rear = "L_Arm_Upper", "R_Leg0_Upper"
    else:
        target, scale = Vector((0, .65, -.25)), 5.9
        shots = [
            ("clay-quarter", (4, 3, 4), clay, 0),
            ("silhouette-side", (5, 1.5, 0), silhouette, 0),
            ("textured-quarter", (-4, 3, 4), original, 0),
            ("unlit-top", (0, 7, -.2), unlit, 0),
            ("party-camera", (5, 7, 5), original, 0),
            ("walk-extreme", (4, 3, 4), clay, 1),
            ("turn-extreme", (-4, 3, 4), clay, 2),
            ("tail-sweep", (4, 3, -4), clay, 3),
            ("tail-contact", (4, 3, 4), original, 4)]
        front, rear = "LeftUpperArm", "RightUpperLeg0"
    posed = []

    def restore():
        for bone, rotation in posed:
            bone.rotation_quaternion = rotation
        posed[:] = []

    def rotate(name, axis, degrees):
        bone = arm.pose.bones[name]
        bone.rotation_mode = "QUATERNION"
        original_rotation = bone.rotation_quaternion.copy()
        posed.append((bone, original_rotation))
        bone.rotation_quaternion = Quaternion(Vector(axis), math.radians(degrees)) @ original_rotation

    for name, position, material, pose in shots:
        restore()
        if pose == 1:
            rotate(front, (1, 0, 0), 24)
            rotate(rear, (1, 0, 0), -20)
        elif pose == 2:
            rotate("Head", (0, 1, 0), 24)
            rotate("LowerTorso", (0, 1, 0), -10)
        elif args.kind == "triceratops" and pose == 3:
            rotate("Head", (1, 0, 0), -18)
        elif args.kind == "stegosaurus" and pose in (3, 4):
            rotate("Tail0_M", (0, 1, 0), 105 if pose == 3 else 145)
        obj.data.materials[0] = material
        camera.location = position
        point_camera(camera, target)
        camera_data.ortho_scale = scale
        scene.render.filepath = str(out / (name + ".png"))
        bpy.ops.render.render(write_still=True)
    restore()
    print("Offline original-body review frames written; native contact remains a runtime criterion.")


if __name__ == "__main__":
    main()
