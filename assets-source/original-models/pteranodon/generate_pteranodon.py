#!/usr/bin/env python3
"""Generate the Pteranodon replacement body against the donor's bind pose.

Run headless:

    blender --background --factory-startup --python generate_pteranodon_body.py \
        -- --rig rig.measured.json --out pteranodon.fbx [--parts all|body|membrane]

This supersedes the membrane-only prototype: the membrane code is unchanged and
the body is authored onto the same rig, in the same frame.

READ FIRST
----------
Everything is authored in the donor renderer's own space - +X to the creature's
left, +Y up, -Z forward - and against the **bind pose**, not a resolved live
pose. The two disagree by up to 3.954 units at the wingtip: the live pose of a
summoned unit has the wings folded (1.638 across against 3.152 long) because it
is whatever frame the animation system is on, while the bind pose is spread
(8.641 across against 2.937 long) and mirrors left to right to 1e-5. Authoring
against the live reading is how two earlier iterations failed review.

Where the donor's anatomy and a pterosaur's disagree, see body_plan.md; the
short version is that the beak extends forward of the last head bone on purpose,
the crest is weighted entirely to `Head` because a crest is bone, and the
donor's eagle tail fan is deliberately left with no geometry on it rather than
being dressed up as a tail.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
import sys

import bpy  # noqa: E402
import bmesh  # noqa: E402
from mathutils import Vector  # noqa: E402


# --- membrane -------------------------------------------------------------
CHORD_ROWS = 5
SPAN_SUBDIVISIONS = 3
SAG = 0.10
HALF_THICKNESS = 0.012
TRAILING_CHAIN = ["L_Foot0", "L_Feather_6_end", "L_Feather_5_end",
                  "L_Feather_4_end", "L_Feather_3_end", "L_Feather_2_end",
                  "L_Feather_1_end"]
TRAILING_BONES = ["L_Foot0", "L_Feather_6", "L_Feather_5", "L_Feather_4",
                  "L_Feather_3", "L_Feather_2", "L_Feather_1"]
LEADING_CHAIN = ["L_Arm_Upper", "L_Arm_Lower", "L_Palm", "L_Feather_1",
                 "L_Feather_1_end"]
LEADING_BONES = ["L_Arm_Upper", "L_Arm_Lower", "L_Palm", "L_Feather_1"]

# --- body -----------------------------------------------------------------
RING_SEGMENTS = 8
# Beak tip, forward of every bone. A Pteranodon's toothless beak is about as
# long again as its skull.
BEAK_TIP_Z = -2.25
LOWER_BEAK_TIP_Z = -2.12
BEAK_HALF_HEIGHT = 0.075
BEAK_HALF_WIDTH = 0.055
# Crest: swept back and up from the skull.
CREST_BACK_Z = -0.18
CREST_TOP_Y = 2.86
CREST_HALF_WIDTH = 0.035
# Only the first third of the donor's eagle tail carries geometry.
TAIL_STUB_FRACTION = 0.35
# Blend across the last quarter of each segment so joints do not crease.
JOINT_BLEND = 0.25


def log(message):
    print("[pteranodon] " + message)


def parse_args():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--rig", required=True)
    parser.add_argument("--out", required=True)
    parser.add_argument("--parts", default="all",
                        choices=("all", "body", "membrane"))
    parser.add_argument("--blend-out", default=None)
    parser.add_argument("--report", default=None)
    return parser.parse_args(argv)


def load_rig(path):
    rig = json.loads(Path(path).read_text(encoding="utf-8"))
    if "bindPoseCount" not in rig:
        raise SystemExit(
            "rig was not built from bind poses; re-run convert_bind_rig.py")
    bones = {b["name"]: b for b in rig["bones"]}
    return rig, bones


def head(bones, name):
    if name not in bones:
        raise SystemExit("rig is missing bone " + name)
    return Vector(bones[name]["head"])


def mirror(point):
    return Vector((-point.x, point.y, point.z))


def resample(points, count):
    lengths = [0.0]
    for index in range(1, len(points)):
        lengths.append(lengths[-1] + (points[index] - points[index - 1]).length)
    total = lengths[-1]
    result = []
    for index in range(count):
        target = total * index / float(count - 1)
        segment = 0
        while segment < len(lengths) - 2 and lengths[segment + 1] < target:
            segment += 1
        span = lengths[segment + 1] - lengths[segment]
        factor = 0.0 if span <= 1e-9 else (target - lengths[segment]) / span
        result.append((points[segment].lerp(points[segment + 1], factor),
                       segment))
    return result


def basis(direction):
    """An orthonormal frame with -Z along `direction`."""
    forward = direction.normalized() if direction.length > 1e-9 \
        else Vector((0.0, 0.0, -1.0))
    up = Vector((0.0, 1.0, 0.0))
    if abs(forward.dot(up)) > 0.98:
        up = Vector((0.0, 0.0, 1.0))
    right = forward.cross(up).normalized()
    up = right.cross(forward).normalized()
    return right, up, forward


def add_tube(bm, weights, points, radii, bone_names, segments=RING_SEGMENTS,
             cap_start=True, cap_end=True):
    """A closed tube through `points`, each ring weighted to its own bone.

    `bone_names` has one entry per point; a ring blends into the next bone over
    the last JOINT_BLEND of its segment so the joint does not crease.
    """
    assert len(points) == len(radii) == len(bone_names)
    rings = []
    for index, centre in enumerate(points):
        if index == 0:
            direction = points[1] - points[0]
        elif index == len(points) - 1:
            direction = points[-1] - points[-2]
        else:
            direction = points[index + 1] - points[index - 1]
        right, up, _ = basis(direction)
        ring = []
        for step in range(segments):
            angle = 2.0 * math.pi * step / segments
            offset = right * (math.cos(angle) * radii[index]) + \
                up * (math.sin(angle) * radii[index])
            vert = bm.verts.new(centre + offset)
            ring.append(vert)
            blend = min(JOINT_BLEND, 0.5)
            if index + 1 < len(bone_names) and bone_names[index + 1] != \
                    bone_names[index]:
                weights[vert] = [(bone_names[index], 1.0 - blend),
                                 (bone_names[index + 1], blend)]
            else:
                weights[vert] = [(bone_names[index], 1.0)]
        rings.append(ring)

    for index in range(len(rings) - 1):
        for step in range(segments):
            nxt = (step + 1) % segments
            try:
                bm.faces.new((rings[index][step], rings[index + 1][step],
                              rings[index + 1][nxt], rings[index][nxt]))
            except ValueError:
                pass
    for cap, ring, name in ((cap_start, rings[0], bone_names[0]),
                            (cap_end, rings[-1], bone_names[-1])):
        if not cap:
            continue
        try:
            bm.faces.new(tuple(ring if ring is rings[-1] else reversed(ring)))
        except ValueError:
            pass
    return rings


def add_beak(bm, weights, root, tip, bone, half_height, half_width, rows=5):
    """A tapering wedge from the skull to a point. Upper and lower beak share
    this shape; only the bone and the tip differ."""
    grid = []
    for row in range(rows):
        factor = row / float(rows - 1)
        centre = root.lerp(tip, factor)
        taper = (1.0 - factor) ** 0.7
        height = half_height * taper
        width = half_width * taper
        column = []
        for offset in ((0.0, height), (width, 0.0), (0.0, -height),
                       (-width, 0.0)):
            vert = bm.verts.new(centre + Vector((offset[0], offset[1], 0.0)))
            weights[vert] = [(bone, 1.0)]
            column.append(vert)
        grid.append(column)
    for row in range(rows - 1):
        for side in range(4):
            nxt = (side + 1) % 4
            try:
                bm.faces.new((grid[row][side], grid[row + 1][side],
                              grid[row + 1][nxt], grid[row][nxt]))
            except ValueError:
                pass
    try:
        bm.faces.new(tuple(reversed(grid[0])))
    except ValueError:
        pass
    return grid


def add_crest(bm, weights, skull, bone):
    """A swept blade from the back of the skull. Weighted entirely to Head,
    because a crest is bone and should move exactly with the skull."""
    # Read clockwise from the brow: up the leading edge, over the top, then
    # back and down to a trailing point well behind the skull. A Pteranodon's
    # crest is the silhouette people recognise it by, so it is deliberately
    # the largest feature on the head.
    profile = [
        Vector((0.0, skull.y + 0.04, skull.z - 0.10)),
        Vector((0.0, skull.y + 0.42, skull.z - 0.06)),
        Vector((0.0, CREST_TOP_Y, skull.z + 0.34)),
        Vector((0.0, CREST_TOP_Y - 0.26, CREST_BACK_Z)),
        Vector((0.0, skull.y + 0.30, CREST_BACK_Z - 0.02)),
        Vector((0.0, skull.y + 0.05, skull.z + 0.26)),
    ]
    left, right = [], []
    for index, point in enumerate(profile):
        # Thin to a blade at the trailing tip rather than ending in a slab.
        taper = CREST_HALF_WIDTH * (1.0 if index < 3 else 0.45)
        vert_left = bm.verts.new(point + Vector((taper, 0.0, 0.0)))
        vert_right = bm.verts.new(point + Vector((-taper, 0.0, 0.0)))
        weights[vert_left] = [(bone, 1.0)]
        weights[vert_right] = [(bone, 1.0)]
        left.append(vert_left)
        right.append(vert_right)
    for index in range(len(profile) - 1):
        try:
            bm.faces.new((left[index], left[index + 1],
                          right[index + 1], right[index]))
        except ValueError:
            pass
    try:
        bm.faces.new(tuple(left))
        bm.faces.new(tuple(reversed(right)))
    except ValueError:
        pass


def sheet_normal(columns, i, j):
    i0, i1 = max(0, i - 1), min(len(columns) - 1, i + 1)
    j0, j1 = max(0, j - 1), min(len(columns[i]) - 1, j + 1)
    along = columns[i1][j] - columns[i0][j]
    across = columns[i][j1] - columns[i][j0]
    normal = along.cross(across)
    return Vector((0.0, 1.0, 0.0)) if normal.length < 1e-6 \
        else normal.normalized()


def wing_grid(bones, side):
    flip = (side == "R")
    leading_raw = [head(bones, n) for n in LEADING_CHAIN]
    trailing_raw = [head(bones, n) for n in TRAILING_CHAIN]
    if flip:
        leading_raw = [mirror(p) for p in leading_raw]
        trailing_raw = [mirror(p) for p in trailing_raw]

    stations = (len(TRAILING_CHAIN) - 1) * SPAN_SUBDIVISIONS + 1
    sampled = resample(leading_raw, stations)
    leading = [position for position, _ in sampled]
    leading_segment = [segment for _, segment in sampled]

    trailing, trailing_bone_of = [], []
    for index in range(stations):
        position = index / float(SPAN_SUBDIVISIONS)
        low = min(int(math.floor(position)), len(trailing_raw) - 2)
        factor = position - low
        trailing.append(trailing_raw[low].lerp(trailing_raw[low + 1], factor))
        trailing_bone_of.append((low, factor))

    columns, assignments = [], {}
    for i in range(stations):
        span = i / float(stations - 1)
        chord = (trailing[i] - leading[i]).length
        column = []
        for j in range(CHORD_ROWS):
            across = j / float(CHORD_ROWS - 1)
            base = leading[i].lerp(trailing[i], across)
            droop = math.sin(across * math.pi) * chord * SAG * (1.0 - span ** 2)
            column.append(base - Vector((0.0, droop, 0.0)))
            leading_bone = LEADING_BONES[min(leading_segment[i],
                                             len(LEADING_BONES) - 1)]
            low, factor = trailing_bone_of[i]
            near = TRAILING_BONES[low]
            far = TRAILING_BONES[min(low + 1, len(TRAILING_BONES) - 1)]
            if flip:
                leading_bone = "R" + leading_bone[1:]
                near = "R" + near[1:]
                far = "R" + far[1:]
            entries = [(leading_bone, 1.0 - across),
                       (near, across * (1.0 - factor)),
                       (far, across * factor)]
            assignments[(i, j)] = [(n, v) for n, v in entries if v > 1e-4]
        columns.append(column)
    return columns, assignments


def add_membrane(bm, weights, bones, side):
    columns, assignments = wing_grid(bones, side)
    sheets = []
    for sign in (1.0, -1.0):
        sheet = []
        for i, column in enumerate(columns):
            verts = []
            for j, position in enumerate(column):
                normal = sheet_normal(columns, i, j)
                vert = bm.verts.new(position + normal * HALF_THICKNESS * sign)
                weights[vert] = assignments[(i, j)]
                verts.append(vert)
            sheet.append(verts)
        sheets.append(sheet)
    for sheet_index, sheet in enumerate(sheets):
        for i in range(len(sheet) - 1):
            for j in range(len(sheet[i]) - 1):
                quad = (sheet[i][j], sheet[i + 1][j],
                        sheet[i + 1][j + 1], sheet[i][j + 1])
                if (sheet_index == 1) != (side == "R"):
                    quad = tuple(reversed(quad))
                try:
                    bm.faces.new(quad)
                except ValueError:
                    pass
    top, bottom = sheets
    for i in range(len(top) - 1):
        for j in (0, len(top[i]) - 1):
            try:
                bm.faces.new((top[i][j], top[i + 1][j],
                              bottom[i + 1][j], bottom[i][j]))
            except ValueError:
                pass


def add_body(bm, weights, bones):
    lower = head(bones, "LowerTorso")
    upper = head(bones, "UpperTorso")
    neck = head(bones, "Neck")
    skull = head(bones, "Head")
    jaw = head(bones, "Jaw")

    add_tube(bm, weights, [lower, upper, neck], [0.30, 0.26, 0.15],
             ["LowerTorso", "UpperTorso", "Neck"], cap_start=False)
    add_tube(bm, weights, [neck, skull], [0.15, 0.13], ["Neck", "Head"],
             cap_start=False)
    add_tube(bm, weights,
             [skull, skull + Vector((0.0, -0.02, -0.18))],
             [0.13, 0.10], ["Head", "Head"], cap_start=False, cap_end=False)

    beak_root = skull + Vector((0.0, -0.02, -0.18))
    add_beak(bm, weights, beak_root,
             Vector((0.0, beak_root.y - 0.05, BEAK_TIP_Z)), "Head",
             BEAK_HALF_HEIGHT, BEAK_HALF_WIDTH)
    add_beak(bm, weights, jaw,
             Vector((0.0, jaw.y - 0.03, LOWER_BEAK_TIP_Z)), "Jaw",
             BEAK_HALF_HEIGHT * 0.7, BEAK_HALF_WIDTH * 0.85)
    add_crest(bm, weights, skull, "Head")

    for side in ("L", "R"):
        thigh = head(bones, side + "_Leg0_Upper")
        shank = head(bones, side + "_Leg0_Lower")
        ankle = head(bones, side + "_Foot0")
        add_tube(bm, weights, [thigh, shank, ankle], [0.10, 0.07, 0.05],
                 [side + "_Leg0_Upper", side + "_Leg0_Lower", side + "_Foot0"])
        for toe in range(1, 5):
            chain = ["%s_Finger_%d_1" % (side, toe),
                     "%s_Finger_%d_2" % (side, toe),
                     "%s_Finger_%d_2_end" % (side, toe)]
            points = [head(bones, n) for n in chain]
            add_tube(bm, weights, points, [0.028, 0.020, 0.010],
                     [chain[0], chain[1], chain[1]], segments=5)

    tail = head(bones, "Tail")
    tail_end = head(bones, "Tail_end")
    stub = tail.lerp(tail_end, TAIL_STUB_FRACTION)
    # Start at the hips, not at the Tail bone. The Tail bone's head is 0.25
    # behind the torso's rear cap, so a tube that began there left the stub
    # floating clear of the body - visible immediately in the top view.
    add_tube(bm, weights, [lower, tail, stub], [0.22, 0.12, 0.05],
             ["LowerTorso", "Tail", "Tail"], cap_start=False)


def build_armature(rig):
    armature = bpy.data.armatures.new("PteranodonRig")
    obj = bpy.data.objects.new("PteranodonRig", armature)
    bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode="EDIT")
    created = {}
    for bone in sorted(rig["bones"], key=lambda value: value["depth"]):
        edit = armature.edit_bones.new(bone["name"])
        edit.head = Vector(bone["head"])
        edit.tail = Vector(bone["tail"])
        if (edit.tail - edit.head).length < 1e-4:
            edit.tail = edit.head + Vector((0.0, 0.02, 0.0))
        created[bone["name"]] = edit
    for bone in rig["bones"]:
        if bone["parent"] and bone["parent"] in created:
            created[bone["name"]].parent = created[bone["parent"]]
    bpy.ops.object.mode_set(mode="OBJECT")
    return obj


def main():
    args = parse_args()
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig, bones = load_rig(args.rig)
    armature = build_armature(rig)

    mesh = bpy.data.meshes.new("PteranodonMesh")
    obj = bpy.data.objects.new("Pteranodon", mesh)
    bpy.context.collection.objects.link(obj)
    bm = bmesh.new()
    weights = {}
    if args.parts in ("all", "body"):
        add_body(bm, weights, bones)
    if args.parts in ("all", "membrane"):
        for side in ("L", "R"):
            add_membrane(bm, weights, bones, side)
    bm.normal_update()
    bm.to_mesh(mesh)
    indexed = {}
    for vert, entries in weights.items():
        indexed[vert.index] = entries
    bm.free()

    groups = {}
    for entries in indexed.values():
        for name, _ in entries:
            if name not in groups:
                groups[name] = obj.vertex_groups.new(name=name)
    for index, entries in indexed.items():
        total = sum(value for _, value in entries) or 1.0
        for name, value in entries:
            groups[name].add([index], value / total, "REPLACE")
    modifier = obj.modifiers.new(name="Armature", type="ARMATURE")
    modifier.object = armature
    obj.parent = armature

    xs = [v.co.x for v in mesh.vertices]
    ys = [v.co.y for v in mesh.vertices]
    zs = [v.co.z for v in mesh.vertices]
    report = {
        "schemaVersion": 2,
        "parts": args.parts,
        "rigSource": rig.get("source"),
        "rigSpace": rig.get("space"),
        "rigSha256": hashlib.sha256(Path(args.rig).read_bytes()).hexdigest(),
        "rigBoneCount": len(rig["bones"]),
        "vertices": len(mesh.vertices),
        "polygons": len(mesh.polygons),
        "boneGroups": len(obj.vertex_groups),
        "maxInfluencesPerVertex": max(len(e) for e in indexed.values()),
        "extent": {"x": [min(xs), max(xs)], "y": [min(ys), max(ys)],
                   "z": [min(zs), max(zs)]},
        "span": max(xs) - min(xs),
        "length": max(zs) - min(zs),
        "spanToLength": (max(xs) - min(xs)) / (max(zs) - min(zs)),
    }
    log(json.dumps(report, indent=1))
    if report["maxInfluencesPerVertex"] > 4:
        raise SystemExit("Unity allows at most four bone influences per vertex")
    if args.report:
        with open(args.report, "w", encoding="utf-8", newline="\n") as handle:
            json.dump(report, handle, indent=1)

    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.export_scene.fbx(
        filepath=args.out, use_selection=True, add_leaf_bones=False,
        bake_anim=False, path_mode="COPY", apply_scale_options="FBX_SCALE_ALL",
        object_types={"ARMATURE", "MESH"})
    log("wrote " + args.out)
    if args.blend_out:
        bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
        log("wrote " + args.blend_out)


if __name__ == "__main__":
    main()
