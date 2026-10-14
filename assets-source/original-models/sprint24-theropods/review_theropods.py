"""Offline shape, paint and deterministic pose review for Sprint 24 bodies."""
import argparse
import math
from pathlib import Path
import sys

import bpy
from mathutils import Matrix, Quaternion, Vector


def point_camera(camera, target):
    back = (camera.location - target).normalized()
    up = Vector((0, 0, 1)) if abs(back.z) < .98 else Vector((0, 1, 0))
    right = up.cross(back).normalized()
    up = back.cross(right).normalized()
    camera.rotation_euler = Matrix((right, up, back)).transposed().to_euler()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", choices=("deinonychus", "tyrannosaurus"),
                        required=True)
    parser.add_argument("--source", required=True)
    parser.add_argument("--out", required=True)
    parser.add_argument("--shot", help="render only this named review shot")
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.open_mainfile(filepath=str(Path(args.source).resolve()))
    title = "Deinonychus" if args.kind == "deinonychus" else "Tyrannosaurus"
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
    scene.world.color = (.10, .10, .10)
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    for name, location, energy, size in (
            ("Key", (5, 6, 7), 1050, 5), ("Fill", (-4, 2, 5), 650, 4)):
        data = bpy.data.lights.new(name, "AREA")
        data.energy, data.shape, data.size = energy, "DISK", size
        light = bpy.data.objects.new(name, data)
        scene.collection.objects.link(light)
        light.location = location
        light.rotation_euler = (Vector((0, 0, 1)) - light.location).to_track_quat(
            "-Z", "Y").to_euler()
    # A neutral project-owned review floor makes foot contact and silhouette
    # height visible without entering the shipped mesh.
    bpy.ops.mesh.primitive_plane_add(size=12, location=(0, 0, 0))
    floor = bpy.context.object
    floor.name = "ReviewFloor"
    floor_material = bpy.data.materials.new("ReviewFloorMaterial")
    floor_material.diffuse_color = (.12, .13, .12, 1)
    floor.data.materials.append(floor_material)

    original = obj.active_material
    clay = bpy.data.materials.new("OriginalClay")
    clay.diffuse_color = (.48, .49, .43, 1)
    silhouette = bpy.data.materials.new("OriginalSilhouette")
    silhouette.diffuse_color = (.012, .012, .012, 1)
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
    target, scale = Vector((0, -.03, .91)), 3.6
    shots = [
        ("clay-quarter", (3.4, 3.8, 2.8), clay, "rest"),
        ("silhouette-side", (4.2, .2, 1.25), silhouette, "rest"),
        ("textured-quarter", (-3.3, 3.8, 2.7), original, "rest"),
        ("unlit-top", (0, -.4, 6.0), unlit, "rest"),
        ("party-camera", (4.5, -5.2, 5.1), original, "rest"),
        ("walk-extreme", (3.9, 2.3, 2.2), clay, "walk"),
        ("turn-extreme", (-3.8, 2.4, 2.2), clay, "turn"),
        ("bite-contact", (3.7, 2.9, 1.7), original, "bite")]
    if args.kind == "deinonychus":
        shots += [
            ("talon-contact", (3.7, .8, 1.7), original, "talon"),
            ("foreclaw-contact", (-3.6, 2.3, 1.7), original, "foreclaw"),
            ("pounce-extreme", (3.8, 2.2, 1.9), clay, "pounce")]
    else:
        shots += [("jaw-extreme", (-3.7, 2.9, 1.8), clay, "bite-wide")]
    if args.shot:
        shots = [shot for shot in shots if shot[0] == args.shot]
        if not shots:
            raise ValueError("unknown review shot: " + args.shot)
    posed = []

    def restore():
        for bone, rotation in reversed(posed):
            bone.rotation_quaternion = rotation
        posed[:] = []

    def rotate(name, axis, degrees):
        bone = arm.pose.bones[name]
        bone.rotation_mode = "QUATERNION"
        original_rotation = bone.rotation_quaternion.copy()
        posed.append((bone, original_rotation))
        # The requested axes are review-scene/global axes. Convert them through
        # the captured rest frame before assigning Blender's bone-local pose;
        # treating them as local made the diagonally oriented lizard jaw orbit
        # toward the tail instead of hinging beneath the skull.
        rest = bone.bone.matrix_local.to_quaternion()
        global_rotation = Quaternion(Vector(axis), math.radians(degrees))
        local_rotation = rest.inverted() @ global_rotation @ rest
        bone.rotation_quaternion = local_rotation @ original_rotation

    for name, position, material, pose in shots:
        restore()
        if pose == "walk":
            rotate("L_Leg0_Upper", (1, 0, 0), 24)
            rotate("R_Leg0_Upper", (1, 0, 0), -22)
            rotate("tail", (0, 0, 1), -9)
        elif pose == "turn":
            rotate("Torso_Lower", (0, 0, 1), -11)
            rotate("tail", (0, 0, 1), 28)
            rotate("tail1", (0, 0, 1), 18)
            rotate("Head", (0, 0, 1), -22)
        elif pose in ("bite", "bite-wide"):
            rotate("jaw", (1, 0, 0), -24 if pose == "bite" else -28)
            rotate("Head", (1, 0, 0), -8)
        elif pose == "talon":
            rotate("L_Leg0_Upper", (1, 0, 0), -28)
            rotate("L_Leg0_Lower", (1, 0, 0), 38)
            rotate("L_toe", (1, 0, 0), -30)
        elif pose == "foreclaw":
            rotate("R_Arm_Upper", (1, 0, 0), -34)
            rotate("R_Arm_Lower", (1, 0, 0), 26)
        elif pose == "pounce":
            rotate("Torso_Lower", (1, 0, 0), -18)
            rotate("L_Leg0_Upper", (1, 0, 0), 34)
            rotate("R_Leg0_Upper", (1, 0, 0), 34)
            rotate("L_Arm_Upper", (1, 0, 0), -30)
            rotate("R_Arm_Upper", (1, 0, 0), -30)
            rotate("jaw", (1, 0, 0), 20)
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
