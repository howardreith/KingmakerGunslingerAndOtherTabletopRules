#!/usr/bin/env python3
"""Private offline review of the two original ape bodies.

The poses here are synthetic. They are a bind-pose stress review: they
show whether the geometry holds together when the donor joints move, and
whether the silhouette reads at the distance a player actually sees a
summon from. They are NOT native animation, and nothing rendered here
proves a timing, a contact point or a runtime behaviour.

Nothing in this file ships. It writes review sheets into a private
directory and reads only the saved prototype blend.
"""
import argparse
import json
import math
from pathlib import Path
import sys

import bpy
from mathutils import Vector

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve()
sys.path.insert(0, str(HERE.parents[1] / "sprint14-insects"))
from render_sprint14_review import apply_shading as shared_shading, look_at, rotate_bone

# Review axes. The donor bind frame is +Y up with -Z forward, so the
# armature is turned a quarter turn about X for display and the creature
# then faces Blender +Y. This is a private review transform; no exported
# vertex, bind pose or native transform is rotated.
SIDE, UP, FORWARD = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, 1, 0))
VIEWS = {
    "three-quarter": SIDE * 1.0 + FORWARD * 1.0 + UP * 0.5,
    "side": SIDE * 1.8 + UP * 0.2,
    "front": FORWARD * 1.8 + UP * 0.25,
    "back": -FORWARD * 1.8 + UP * 0.25,
    "top": UP + FORWARD * 0.001,
    # Kingmaker looks down on the party from behind and above. This is the
    # frame that decides whether either ape is recognisable in play.
    "party-camera": SIDE * 0.8 + FORWARD * 0.95 + UP * 1.4,
    "head-closeup": SIDE * 1.1 + FORWARD * 1.4 + UP * 0.45,
    "hand-closeup": SIDE * 1.5 + FORWARD * 0.8 + UP * 0.15,
}
HEAD_GROUPS = ("Head", "Jaw_01", "L_Up_lip_01", "R_Up_lip_01",
               "L_Eyebrow_01", "R_Eyebrow_01", "L_Ear_01", "R_Ear_01")
HAND_GROUPS = tuple(side + "_" + part for side in ("L", "R") for part in
                    ("Hand_01", "Thumb_03", "Fore_Finger_03", "Midle_Finger_03",
                     "Little_Finger_03"))
POSES = ("rest", "jaw-open", "slam-forward", "slam-contact", "claw-rake",
         "fists-closed", "crouch")


def apply_shading(creature, shading):
    # Sprint 18 reviews with backface culling on, so an inside-out shell
    # cannot hide behind a two-sided preview material. The generator also
    # refuses to export one, and these two checks are deliberately
    # independent of each other.
    shared_shading(creature, shading)
    for material in creature.data.materials:
        if material is not None:
            material.use_backface_culling = True


def group_points(creature, names, points):
    indices = {group.index for group in creature.vertex_groups
               if group.name in names}
    return [points[vertex.index] for vertex in creature.data.vertices
            if any(entry.group in indices and entry.weight >= 0.5
                   for entry in vertex.groups)] or points


def evaluated_points(creature):
    evaluated = creature.evaluated_get(bpy.context.evaluated_depsgraph_get())
    mesh = evaluated.to_mesh()
    points = [creature.matrix_world @ vertex.co for vertex in mesh.vertices]
    evaluated.to_mesh_clear()
    return points


def swing(armature, creature, joints, groups, axis=(1, 0, 0), toward=None):
    """Rotate a chain, then check it actually moved the limb as claimed.

    `joints` is (bone, degrees) pairs, because a shoulder and an elbow do
    not take the same angle: driving both to the contact angle carries the
    hand past the target and back over the shoulder. The sign that swings
    an arm forward depends on the donor bone rest frames, so it is
    measured rather than assumed. If neither sign moves the measured group
    the way the pose name says, the review fails rather than quietly
    rendering a pose that is not the pose.
    """
    toward = Vector(toward if toward is not None else FORWARD)
    before = group_points(creature, groups, evaluated_points(creature))
    reference = sum((point.dot(toward) for point in before), 0.0) / len(before)
    for sign in (1, -1):
        for name, degrees in joints:
            if not rotate_bone(armature, name, axis, sign * degrees):
                raise SystemExit("the donor rig is missing joint " + name)
        bpy.context.view_layer.update()
        after = group_points(creature, groups, evaluated_points(creature))
        moved = sum((point.dot(toward) for point in after), 0.0) / len(after)
        if moved - reference > 0.05:
            return sign
        for name, _ in joints:
            rotate_bone(armature, name, axis, 0)
        bpy.context.view_layer.update()
    raise SystemExit("no sign of the rotation moved the limb as the pose claims")


def apply_pose(armature, creature, pose):
    """One synthetic pose. Only donor joints move; nothing is retargeted."""
    def arms(shoulder, elbow, sides=("L", "R")):
        return ([(side + "_Up_Arm_01", shoulder) for side in sides] +
                [(side + "_Forearm_01", elbow) for side in sides])

    if pose == "rest":
        return "bind pose as authored"
    if pose == "jaw-open":
        if not rotate_bone(armature, "Jaw_01", (1, 0, 0), 26):
            raise SystemExit("the donor rig is missing its jaw")
        return "jaw hinge opened 26 degrees"
    if pose == "slam-forward":
        swing(armature, creature, arms(50, 25), HAND_GROUPS)
        return "both arms raised into a forward slam"
    if pose == "slam-contact":
        swing(armature, creature, arms(76, 8), HAND_GROUPS)
        return "both arms at a synthetic forward contact"
    if pose == "claw-rake":
        swing(armature, creature, arms(70, 18, ("R",)), HAND_GROUPS)
        rotate_bone(armature, "Jaw_01", (1, 0, 0), 18)
        return "one arm forward with the jaw part open"
    if pose == "fists-closed":
        for side in ("L", "R"):
            for digit in ("Fore_Finger", "Midle_Finger", "Little_Finger"):
                for index in (1, 2, 3):
                    rotate_bone(armature, "%s_%s_%02d" % (side, digit, index),
                                (1, 0, 0), 38)
            for index in (1, 2):
                rotate_bone(armature, "%s_Thumb_%02d" % (side, index), (1, 0, 0), 24)
        return "both hands curled toward a knuckle stance"
    if pose == "crouch":
        for side in ("L", "R"):
            rotate_bone(armature, side + "_UpLeg_01", (1, 0, 0), 24)
            rotate_bone(armature, side + "_Leg_01", (1, 0, 0), -34)
        return "legs flexed into a crouch"
    raise SystemExit("unsupported synthetic pose " + pose)


def framing(points, camera):
    low = Vector([min(point[axis] for point in points) for axis in range(3)])
    high = Vector([max(point[axis] for point in points) for axis in range(3)])
    centre = (low + high) / 2
    look_at(camera, centre)
    axes = camera.rotation_euler.to_matrix()
    right, up = axes @ Vector((1, 0, 0)), axes @ Vector((0, 1, 0))
    diameter = max(2 * abs((point - centre).dot(axis))
                   for point in points for axis in (right, up))
    return centre, max(diameter * 1.12, 0.01)


def render(args, shading, pose):
    bpy.ops.wm.open_mainfile(filepath=str(Path(args.blend).resolve()))
    creature = next(obj for obj in bpy.context.scene.objects if obj.type == "MESH")
    armature = next(obj for obj in bpy.context.scene.objects
                    if obj.type == "ARMATURE")
    if "Pelvis" not in armature.pose.bones or "Jaw_01" not in armature.pose.bones:
        raise SystemExit("this blend is not a Sprint 18 primate prototype")
    armature.rotation_euler.x = math.pi / 2
    bpy.context.view_layer.update()
    description = apply_pose(armature, creature, pose)
    apply_shading(creature, shading)
    bpy.context.view_layer.update()
    points = evaluated_points(creature)
    low = Vector([min(point[axis] for point in points) for axis in range(3)])
    high = Vector([max(point[axis] for point in points) for axis in range(3)])
    centre, extent = (low + high) / 2, max((high - low).length, 0.1)

    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    scene.eevee.taa_render_samples = 32
    scene.render.resolution_x = scene.render.resolution_y = args.resolution
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    scene.world = bpy.data.worlds.new("PrimateReviewWorld")
    scene.world.color = (0.8, 0.8, 0.8) if shading == "silhouette" else (0.07, 0.08, 0.09)
    scene.view_settings.view_transform = "Standard"
    if shading in ("clay", "textured"):
        bpy.ops.mesh.primitive_plane_add(size=extent * 6,
                                         location=(centre.x, centre.y, low.z - 0.012))
        floor = bpy.context.object
        material = bpy.data.materials.new("ReviewFloor")
        material.diffuse_color = (0.19, 0.20, 0.21, 1)
        floor.data.materials.append(material)
        for index, direction in enumerate((SIDE + FORWARD + UP * 2, -SIDE + UP * 0.6)):
            light = bpy.data.lights.new("ReviewLight" + str(index), "AREA")
            light.energy = (300 if index == 0 else 190) * (extent / 4) ** 2
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
        focus = points
        if view == "head-closeup":
            focus = group_points(creature, HEAD_GROUPS, points)
        elif view == "hand-closeup":
            focus = group_points(creature, HAND_GROUPS, points)
        direction = VIEWS[view].normalized()
        frame_centre, _ = framing(focus, camera)
        camera.location = frame_centre + direction * extent * 2
        _, camera.data.ortho_scale = framing(focus, camera)
        path = output / ("%s-%s-%s-%s.png" % (creature.name, shading, pose, view))
        scene.render.filepath = str(path.resolve())
        bpy.ops.render.render(write_still=True)
        written.append(path.name)
    print("[primate-review] " + json.dumps(dict(
        pose=pose, shading=shading, applied=description, files=written,
        warning="synthetic offline poses; no runtime timing or contact proof")))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--blend", required=True)
    parser.add_argument("--out-dir", required=True)
    parser.add_argument("--shading", default="textured",
                        choices=("clay", "silhouette", "textured", "unlit"))
    parser.add_argument("--pose", default="rest", choices=POSES)
    parser.add_argument("--views",
                        default="three-quarter,side,front,party-camera")
    parser.add_argument("--resolution", type=int, default=640)
    parser.add_argument("--suite", action="store_true")
    args = parser.parse_args(
        sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    if not args.suite:
        render(args, args.shading, args.pose)
        return
    for shading in ("clay", "silhouette", "textured", "unlit"):
        render(args, shading, "rest")
    args.views = "three-quarter,side,top"
    for pose in ("slam-forward", "slam-contact", "claw-rake", "crouch"):
        render(args, "textured", pose)
    args.views = "head-closeup"
    render(args, "textured", "jaw-open")
    render(args, "unlit", "rest")
    args.views = "hand-closeup"
    render(args, "textured", "fists-closed")
    render(args, "unlit", "rest")


if __name__ == "__main__":
    main()
