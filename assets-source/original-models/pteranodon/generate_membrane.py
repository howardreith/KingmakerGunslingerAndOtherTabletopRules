#!/usr/bin/env python3
"""Generate the Pteranodon wing-membrane test mesh against the donor bind pose.

Run headless:

    blender --background --factory-startup --python generate_membrane.py -- \
        --rig rig.measured.json --out membrane.fbx [--blend-out membrane.blend]

This is the Sprint 2 membrane prototype: the smallest piece of original
geometry that can prove a replacement mesh deforms correctly under the donor's
own animations. It is not the finished creature.

WHY THIS EXISTS SEPARATELY FROM generate_pteranodon.py
------------------------------------------------------
Two earlier iterations were authored against bone positions resolved from the
LIVE transforms of a summoned unit. Those are whatever the animation system has
them at, and the resolved layout had the wings folded - 1.638 across against
3.152 long. The bind poses, which is what the skinning maths actually uses,
describe a fully spread pose 8.641 across against 2.937 long, and disagree with
the live reading by 3.954 units at the wingtip. A membrane authored against a
folded pose looks plausible at rest and tears open the moment the wings spread,
which is exactly how those iterations failed visual review.

Everything here reads `head`/`tail` out of rig.measured.json, which is now
built from `SkinnedMeshRenderer.sharedMesh.bindposes`. Nothing native is
copied: the vertices are generated from the planform rules below, and the only
donor-derived numbers are bone positions, which are measurements rather than
art.

PLANFORM
--------
A pterosaur's brachiopatagium is one sheet bounded by

  leading edge   shoulder -> elbow -> wrist -> elongated finger -> wingtip
  trailing edge  wingtip -> back along the finger fan -> ankle
  root           ankle -> up the flank -> shoulder

The donor rig ends each wing in six feather bones. Feather_1 is the longest and
most forward, so it stands in for the elongated fourth finger and carries the
leading edge out to the tip; Feather_2..6 fan backwards and their ends are the
trailing-edge control points, with Feather_6 - which hangs off the shoulder -
anchoring the innermost station just above the ankle.

Pairing each trailing station with its own feather bone is the whole point: the
trailing edge then follows those bones through the native animations instead of
sliding through them.
"""
import argparse
import hashlib
import json
import math
import sys

from pathlib import Path  # noqa: E402

import bpy  # noqa: E402
import bmesh  # noqa: E402
from mathutils import Vector  # noqa: E402


# --- membrane shape --------------------------------------------------------
# Chordwise rows between the leading and trailing edge. Three is enough to show
# bending; more rows cost nothing and make the sag read properly.
CHORD_ROWS = 5
# Extra spanwise stations inserted between each pair of rig-driven stations, so
# the sheet bends smoothly rather than faceting at every feather.
SPAN_SUBDIVISIONS = 3
# Slack in the sheet, as a fraction of the local chord. A membrane hangs; a
# perfectly flat quad reads as cardboard.
SAG = 0.10
# Half-thickness. The sheet is doubled so it is not invisible edge-on and so it
# has a sane normal on both faces.
HALF_THICKNESS = 0.012
# Trailing stations, root to tip. Feather_1_end is the wingtip, where the
# leading and trailing edges meet.
TRAILING_CHAIN = ["L_Foot0", "L_Feather_6_end", "L_Feather_5_end",
                  "L_Feather_4_end", "L_Feather_3_end", "L_Feather_2_end",
                  "L_Feather_1_end"]
# The bone that drives each trailing station, in the same order.
TRAILING_BONES = ["L_Foot0", "L_Feather_6", "L_Feather_5", "L_Feather_4",
                  "L_Feather_3", "L_Feather_2", "L_Feather_1"]
# Leading edge, root to tip, with the bone that drives each span.
LEADING_CHAIN = ["L_Arm_Upper", "L_Arm_Lower", "L_Palm", "L_Feather_1",
                 "L_Feather_1_end"]
LEADING_BONES = ["L_Arm_Upper", "L_Arm_Lower", "L_Palm", "L_Feather_1"]


def log(message):
    print("[membrane] " + message)


def parse_args():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--rig", required=True)
    parser.add_argument("--out", required=True)
    parser.add_argument("--blend-out", default=None)
    parser.add_argument("--report", default=None)
    return parser.parse_args(argv)


def load_rig(path):
    with open(path, "r", encoding="utf-8") as handle:
        rig = json.load(handle)
    bones = {b["name"]: b for b in rig["bones"]}
    for required in LEADING_CHAIN + TRAILING_CHAIN:
        if required not in bones:
            raise SystemExit("rig is missing bone " + required)
    if "bindPoseCount" not in rig:
        raise SystemExit(
            "rig was not built from bind poses; re-run convert_bind_rig.py")
    return rig, bones


def head(bones, name):
    return Vector(bones[name]["head"])


def mirror(point):
    """Right side is the exact mirror; the bind pose is symmetric to 1e-5."""
    return Vector((-point.x, point.y, point.z))


def resample(points, count):
    """Evenly spaced points along a polyline, by arclength."""
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


def build_armature(rig, bones):
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


def wing_grid(bones, side):
    """Vertex positions and per-vertex bone weights for one wing.

    Returns (columns, weights) where columns[i][j] is a Vector and
    weights[(i, j)] is a list of (bone_name, weight).
    """
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

    # Trailing stations keep their rig-driven points exactly and interpolate
    # between them, so every feather end stays on the boundary.
    trailing = []
    trailing_bone_of = []
    for index in range(stations):
        position = index / float(SPAN_SUBDIVISIONS)
        low = min(int(math.floor(position)), len(trailing_raw) - 2)
        factor = position - low
        trailing.append(trailing_raw[low].lerp(trailing_raw[low + 1], factor))
        trailing_bone_of.append((low, factor))

    columns, weights = [], {}
    for i in range(stations):
        span = i / float(stations - 1)
        chord = (trailing[i] - leading[i]).length
        column = []
        for j in range(CHORD_ROWS):
            across = j / float(CHORD_ROWS - 1)
            base = leading[i].lerp(trailing[i], across)
            # Hang the sheet: greatest slack mid-chord, fading to nothing at the
            # tip where the two edges meet.
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
            weights[(i, j)] = [(name, value) for name, value in entries
                               if value > 1e-4]
        columns.append(column)
    return columns, weights


def build_mesh(bones):
    mesh = bpy.data.meshes.new("PteranodonMembrane")
    obj = bpy.data.objects.new("PteranodonMembrane", mesh)
    bpy.context.collection.objects.link(obj)
    bm = bmesh.new()
    assignments = []   # (BMVert, [(bone, weight)])

    for side in ("L", "R"):
        columns, weights = wing_grid(bones, side)
        # Two sheets, offset along the local normal, joined at the rim.
        sheets = []
        for sign in (1.0, -1.0):
            sheet = []
            for i, column in enumerate(columns):
                verts = []
                for j, position in enumerate(column):
                    normal = sheet_normal(columns, i, j)
                    vert = bm.verts.new(position + normal * HALF_THICKNESS * sign)
                    verts.append(vert)
                    assignments.append((vert, weights[(i, j)]))
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
                        pass          # degenerate at the closing tip
        # Close the rim so the sheet is a solid, not two loose surfaces.
        top, bottom = sheets
        for i in range(len(top) - 1):
            for pair in ((0, 0), (len(top[i]) - 1, len(top[i]) - 1)):
                j = pair[0]
                try:
                    bm.faces.new((top[i][j], top[i + 1][j],
                                  bottom[i + 1][j], bottom[i][j]))
                except ValueError:
                    pass

    bm.normal_update()
    bm.to_mesh(mesh)
    index_of = {}
    for vert, entries in assignments:
        index_of[vert.index] = entries
    bm.free()
    return obj, index_of


def sheet_normal(columns, i, j):
    """Approximate surface normal from the neighbouring grid points."""
    i0 = max(0, i - 1)
    i1 = min(len(columns) - 1, i + 1)
    j0 = max(0, j - 1)
    j1 = min(len(columns[i]) - 1, j + 1)
    along = columns[i1][j] - columns[i0][j]
    across = columns[i][j1] - columns[i][j0]
    normal = along.cross(across)
    if normal.length < 1e-6:
        return Vector((0.0, 1.0, 0.0))
    return normal.normalized()


def apply_weights(obj, armature, weights):
    groups = {}
    for entries in weights.values():
        for name, _ in entries:
            if name not in groups:
                groups[name] = obj.vertex_groups.new(name=name)
    for index, entries in weights.items():
        total = sum(value for _, value in entries) or 1.0
        for name, value in entries:
            groups[name].add([index], value / total, "REPLACE")
    modifier = obj.modifiers.new(name="Armature", type="ARMATURE")
    modifier.object = armature
    obj.parent = armature


def main():
    args = parse_args()
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig, bones = load_rig(args.rig)
    armature = build_armature(rig, bones)
    obj, weights = build_mesh(bones)
    apply_weights(obj, armature, weights)

    mesh = obj.data
    xs = [v.co.x for v in mesh.vertices]
    ys = [v.co.y for v in mesh.vertices]
    zs = [v.co.z for v in mesh.vertices]
    influences = sorted({len(entries) for entries in weights.values()})
    digest = hashlib.sha256(Path(args.rig).read_bytes()).hexdigest()
    report = {
        "schemaVersion": 1,
        "rigSource": rig.get("source"),
        "rigSpace": rig.get("space"),
        "rigSha256": digest,
        "rigBoneCount": len(rig["bones"]),
        "leadingChain": LEADING_CHAIN,
        "trailingChain": TRAILING_CHAIN,
        "chordRows": CHORD_ROWS,
        "spanSubdivisions": SPAN_SUBDIVISIONS,
        "sag": SAG,
        "halfThickness": HALF_THICKNESS,
        "vertices": len(mesh.vertices),
        "polygons": len(mesh.polygons),
        "boneGroups": len(obj.vertex_groups),
        "maxInfluencesPerVertex": max(len(e) for e in weights.values()),
        "influenceCounts": influences,
        "extent": {"x": [min(xs), max(xs)], "y": [min(ys), max(ys)],
                   "z": [min(zs), max(zs)]},
        "span": max(xs) - min(xs),
    }
    log(json.dumps(report, indent=1))
    if report["maxInfluencesPerVertex"] > 4:
        raise SystemExit("Unity allows at most four bone influences per vertex")
    if args.report:
        # .gitattributes mandates LF for .json; text mode on Windows
        # would emit CRLF and the repository validator rejects it.
        with open(args.report, "w", encoding="utf-8",
                  newline="
") as handle:
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
