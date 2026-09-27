#!/usr/bin/env python3
"""Author a feathered eagle on the measured flying-animal bind frame.

The private --rig input, .blend and .fbx contain native bone transforms and
remain local. The shipped JSON contains only project-owned mesh geometry,
weights, UVs, structural bone names and the exact original albedo hash.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
import sys

import bpy
import bmesh
from mathutils import Vector

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "pteranodon"))
import generate_pteranodon as shared  # noqa: E402


def parse_args():
    parser = argparse.ArgumentParser()
    for name in ("rig", "albedo", "mesh-data", "report", "blend-out", "fbx-out"):
        parser.add_argument("--" + name, required=True)
    return parser.parse_args(sys.argv[sys.argv.index("--") + 1:]
                             if "--" in sys.argv else [])


def feather(bm, weights, uvs, root, tip, width, binding, span):
    """A tapered, shallowly cambered two-sided vane with one stable rig bind."""
    flight = (tip - root).normalized()
    across = flight.cross(Vector((0.0, 1.0, 0.0))).normalized()
    sheets = []
    for face in (1.0, -1.0):
        rows = []
        for row in range(5):
            t = row / 4.0
            centre = root.lerp(tip, t)
            half = width * math.sin(math.pi * (0.06 + 0.90 * t))
            half *= 0.18 + 0.82 * min(1.0, t * 5.0)
            line = []
            for side in (-1.0, 1.0):
                point = centre + across * half * side
                point.y += 0.035 * math.sin(t * math.pi) + face * 0.009
                vertex = bm.verts.new(point)
                weights[vertex] = list(binding)
                uvs[vertex] = shared.region_uv("membrane", span, t)
                line.append(vertex)
            rows.append(line)
        sheets.append(rows)
    for face, rows in enumerate(sheets):
        for row in range(4):
            quad = (rows[row][0], rows[row][1],
                    rows[row + 1][1], rows[row + 1][0])
            bm.faces.new(quad if face == 0 else tuple(reversed(quad)))
    for row in range(4):
        for side in range(2):
            bm.faces.new((sheets[0][row][side], sheets[0][row + 1][side],
                          sheets[1][row + 1][side], sheets[1][row][side]))


def add_body(bm, weights, uvs, bones):
    lower = shared.head(bones, "LowerTorso")
    upper = shared.head(bones, "UpperTorso")
    neck = shared.head(bones, "Neck")
    skull = shared.head(bones, "Head")
    jaw = shared.head(bones, "Jaw")
    tail = shared.head(bones, "Tail")
    shared.add_tube(bm, weights, uvs, [lower, upper, neck, skull],
                    [0.34, 0.39, 0.20, 0.19],
                    ["LowerTorso", "UpperTorso", "Neck", "Head"], "body")
    bill = skull + Vector((0.0, -0.015, -0.17))
    shared.add_beak(bm, weights, uvs, bill,
                    bill + Vector((0.0, -0.12, -0.32)), "Head",
                    0.085, 0.085, "beak", (0.5, 1.0))
    shared.add_beak(bm, weights, uvs, jaw,
                    jaw + Vector((0.0, -0.08, -0.23)), "Jaw",
                    0.045, 0.065, "beak", (0.0, 0.5))
    # The bird's tail is a distinct fan, bound to the accepted donor Tail.
    shared.add_tube(bm, weights, uvs, [lower, tail], [0.22, 0.12],
                    ["LowerTorso", "Tail"], "body")
    tail_end = shared.head(bones, "Tail_end")
    for index in range(7):
        spread = (index - 3) / 3.0
        tip = tail_end + Vector((0.40 * spread, 0.0, 0.13))
        feather(bm, weights, uvs, tail, tip, 0.12,
                [("Tail", 1.0)], 0.08 + index * 0.12)
    for side in ("L", "R"):
        hip = shared.head(bones, side + "_Leg0_Upper")
        knee = shared.head(bones, side + "_Leg0_Lower")
        foot = shared.head(bones, side + "_Foot0")
        shared.add_tube(bm, weights, uvs, [hip, knee, foot],
                        [0.11, 0.075, 0.045],
                        [side + "_Leg0_Upper", side + "_Leg0_Lower",
                         side + "_Foot0"], "limbs")
        for toe in range(1, 5):
            first = "%s_Finger_%d_1" % (side, toe)
            second = "%s_Finger_%d_2" % (side, toe)
            end = second + "_end"
            shared.add_tube(bm, weights, uvs,
                            [shared.head(bones, name)
                             for name in (first, second, end)],
                            [0.028, 0.020, 0.007],
                            [first, second, second], "limbs", segments=5)


def add_wing(bm, weights, uvs, bones, side):
    # A covered wing surface follows the complete live donor animation. The
    # separately tapered vanes break its trailing edge into bird primaries.
    shared.add_membrane(bm, weights, uvs, bones, side)
    columns, assignments = shared.wing_grid(bones, side)
    for station in range(1, len(columns) - 1):
        span = station / float(len(columns) - 1)
        root = columns[station][2]
        length = 0.12 + 0.10 * math.sin(span * math.pi)
        outward = 0.13 if side == "L" else -0.13
        tip = columns[station][-1] + Vector((outward, -0.045, length))
        feather(bm, weights, uvs, root, tip, 0.17,
                assignments[(station, 2)], span)
    # The six donor feather chains each carry one longer tip plume.
    for index in range(1, 7):
        name = side + "_Feather_" + str(index)
        root = shared.head(bones, name)
        tip = shared.head(bones, name + "_end")
        feather(bm, weights, uvs, root,
                tip + Vector((0.0, -0.03, 0.12)), 0.16,
                [(name, 1.0)], index / 7.0)


def main():
    args = parse_args()
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig, bones = shared.load_rig(args.rig)
    armature = shared.build_armature(rig)
    armature.name = "EagleRigPreview"
    mesh = bpy.data.meshes.new("EagleMesh")
    obj = bpy.data.objects.new("Eagle", mesh)
    bpy.context.collection.objects.link(obj)
    bm = bmesh.new()
    weights, uvs = {}, {}
    add_body(bm, weights, uvs, bones)
    for side in ("L", "R"):
        add_wing(bm, weights, uvs, bones, side)
    bm.normal_update()
    bm.to_mesh(mesh)
    indexed = {vert.index: entries for vert, entries in weights.items()}
    uv_indexed = {vert.index: uv for vert, uv in uvs.items()}
    bm.free()
    layer = mesh.uv_layers.new(name="Albedo")
    for loop in mesh.loops:
        layer.data[loop.index].uv = uv_indexed[loop.vertex_index]
    groups = {}
    for entries in indexed.values():
        for name, _ in entries:
            if name not in groups:
                groups[name] = obj.vertex_groups.new(name=name)
    for index, entries in indexed.items():
        total = sum(value for _, value in entries)
        for name, value in entries:
            groups[name].add([index], value / total, "REPLACE")
    modifier = obj.modifiers.new(name="Armature", type="ARMATURE")
    modifier.object = armature
    obj.parent = armature
    shared.attach_preview_material(obj, mesh, args.albedo)
    xs, ys, zs = ([getattr(v.co, axis) for v in mesh.vertices]
                  for axis in ("x", "y", "z"))
    report = {
        "schemaVersion": 3,
        "creature": "eagle",
        "rigSha256": hashlib.sha256(Path(args.rig).read_bytes()).hexdigest(),
        "rigBoneCount": len(rig["bones"]),
        "vertices": len(mesh.vertices),
        "polygons": len(mesh.polygons),
        "boneGroups": len(groups),
        "maxInfluencesPerVertex": max(len(e) for e in indexed.values()),
        "extent": {"x": [min(xs), max(xs)], "y": [min(ys), max(ys)],
                   "z": [min(zs), max(zs)]},
        "albedo": shared.albedo_manifest(args.albedo),
    }
    if report["maxInfluencesPerVertex"] > 4:
        raise SystemExit("Eagle vertex exceeds four bone influences")
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    shared.write_mesh_data(args.mesh_data, obj, mesh, indexed, uv_indexed,
                           report, args.albedo)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.export_scene.fbx(filepath=args.fbx_out, use_selection=True,
        add_leaf_bones=False, bake_anim=False, path_mode="COPY",
        apply_scale_options="FBX_SCALE_ALL", object_types={"ARMATURE", "MESH"})
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[eagle] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
