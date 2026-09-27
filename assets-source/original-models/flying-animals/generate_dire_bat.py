#!/usr/bin/env python3
"""Author an original Dire Bat body on the measured Giant Eagle bind frame.

The private --rig input and optional .blend/.fbx outputs contain native bone
transforms and stay outside the repository. The shipped mesh JSON contains
only original geometry, weights, UVs, and the structural bone names. Geometry
and painting use the established Pteranodon atlas/export path.
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

BAT_SPAN_SUBDIVISIONS = 6


def parse_args():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--rig", required=True)
    parser.add_argument("--albedo", required=True)
    parser.add_argument("--mesh-data", required=True)
    parser.add_argument("--report", required=True)
    parser.add_argument("--blend-out", required=True)
    parser.add_argument("--fbx-out", required=True)
    return parser.parse_args(argv)


def bat_wing_grid(bones, side):
    """Keep the proven bind-frame/weight mapping, scallop between finger tips.

    The Pteranodon trailing edge is almost straight. A bat's membrane draws
    inward between its long fingers, so every intermediate station moves a
    fraction of its chord toward the leading edge. Tip stations and every
    weight remain in the donor's measured frame.
    """
    columns, assignments = shared.wing_grid(
        bones, side, subdivisions=BAT_SPAN_SUBDIVISIONS)
    for station, column in enumerate(columns):
        span = station / float(len(columns) - 1)
        # The eagle feather endpoints make a shallow chord in plan view.
        # Move the membrane aft through the middle of the wing, tapering the
        # extra depth to zero at root and tip. The same finger weights carry
        # this original geometry when the donor animation folds the wing.
        aft = 0.72 * math.sin(math.pi * span)
        for chord, point in enumerate(column):
            column[chord] = point + Vector((0.0, 0.0,
                aft * chord / float(len(column) - 1)))
        between = ((station % BAT_SPAN_SUBDIVISIONS) /
                   float(BAT_SPAN_SUBDIVISIONS))
        notch = 0.31 * math.sin(math.pi * between)
        if notch <= 0.0:
            continue
        leading, trailing = column[0], column[-1]
        inset = (leading - trailing) * notch
        for chord, point in enumerate(column):
            column[chord] = point + inset * (chord /
                float(len(column) - 1))
    return columns, assignments


def add_ear(bm, weights, uvs, skull, sign):
    """A thick pointed pinna, head-bound, painted on the ear atlas region."""
    x = sign * 0.18
    profile = [
        (x - 0.11, skull.y + 0.04, skull.z + 0.01, 0.0, 0.0),
        (x + 0.10, skull.y + 0.04, skull.z + 0.01, 1.0, 0.0),
        (x + sign * 0.035, skull.y + 0.45, skull.z + 0.035, 0.5, 1.0),
    ]
    faces = []
    for depth in (-0.022, 0.022):
        verts = []
        for px, py, pz, u, v in profile:
            vert = bm.verts.new((px, py, pz + depth))
            weights[vert] = [("Head", 1.0)]
            uvs[vert] = shared.region_uv("crest", u, v)
            verts.append(vert)
        faces.append(verts)
    front, back = faces
    bm.faces.new(tuple(front))
    bm.faces.new(tuple(reversed(back)))
    for index in range(3):
        nxt = (index + 1) % 3
        bm.faces.new((front[index], back[index], back[nxt], front[nxt]))


def add_tail_membrane(bm, weights, uvs, bones):
    """A small uropatagium between the hind feet, bound to tail and ankles."""
    lower = shared.head(bones, "LowerTorso")
    tail = shared.head(bones, "Tail_end")
    left = shared.head(bones, "L_Foot0")
    right = shared.head(bones, "R_Foot0")
    outline = [(lower, "LowerTorso", 0.5, 0.0),
               (left, "L_Foot0", 1.0, 0.8),
               (tail, "Tail", 0.5, 1.0),
               (right, "R_Foot0", 0.0, 0.8)]
    for offset in (-0.012, 0.012):
        verts = []
        for point, bone, u, v in outline:
            vert = bm.verts.new(point + Vector((0.0, offset, 0.0)))
            weights[vert] = [(bone, 1.0)]
            uvs[vert] = shared.region_uv("membrane", u, v)
            verts.append(vert)
        bm.faces.new(tuple(verts if offset > 0 else reversed(verts)))


def add_body(bm, weights, uvs, bones):
    lower = shared.head(bones, "LowerTorso")
    upper = shared.head(bones, "UpperTorso")
    neck = shared.head(bones, "Neck")
    skull = shared.head(bones, "Head")
    jaw = shared.head(bones, "Jaw")
    muzzle = skull + Vector((0.0, -0.035, -0.43))
    shared.add_tube(bm, weights, uvs, [lower, upper, neck, skull],
                    [0.29, 0.34, 0.20, 0.22],
                    ["LowerTorso", "UpperTorso", "Neck", "Head"], "body")
    shared.add_tube(bm, weights, uvs, [skull, muzzle], [0.19, 0.07],
                    ["Head", "Head"], "beak", segments=10)
    shared.add_tube(bm, weights, uvs,
                    [jaw, jaw + Vector((0.0, -0.04, -0.31))],
                    [0.12, 0.045], ["Jaw", "Jaw"], "beak")
    for sign in (-1, 1):
        add_ear(bm, weights, uvs, skull, sign)
        side = "L" if sign > 0 else "R"
        hip = shared.head(bones, side + "_Leg0_Upper")
        knee = shared.head(bones, side + "_Leg0_Lower")
        foot = shared.head(bones, side + "_Foot0")
        shared.add_tube(bm, weights, uvs, [hip, knee, foot],
                        [0.10, 0.07, 0.04],
                        [side + "_Leg0_Upper", side + "_Leg0_Lower",
                         side + "_Foot0"], "limbs")
        for toe in range(1, 5):
            names = ["%s_Finger_%d_1" % (side, toe),
                     "%s_Finger_%d_2" % (side, toe),
                     "%s_Finger_%d_2_end" % (side, toe)]
            shared.add_tube(bm, weights, uvs,
                            [shared.head(bones, name) for name in names],
                            [0.021, 0.016, 0.007],
                            [names[0], names[1], names[1]],
                            "limbs", segments=5)
    add_tail_membrane(bm, weights, uvs, bones)


def add_wing_bones(bm, weights, uvs, bones, side):
    arm = [side + "_Arm_Upper", side + "_Arm_Lower",
           side + "_Palm", side + "_Feather_1"]
    shared.add_tube(bm, weights, uvs,
                    [shared.head(bones, name) for name in arm],
                    [0.10, 0.08, 0.06, 0.035], arm, "limbs")
    for index in range(1, 7):
        name = side + "_Feather_" + str(index)
        shared.add_tube(bm, weights, uvs,
                        [shared.head(bones, name),
                         shared.head(bones, name + "_end")],
                        [0.028, 0.006], [name, name], "limbs",
                        segments=5)


def main():
    args = parse_args()
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig, bones = shared.load_rig(args.rig)
    armature = shared.build_armature(rig)
    armature.name = "DireBatRigPreview"
    mesh = bpy.data.meshes.new("DireBatMesh")
    obj = bpy.data.objects.new("DireBat", mesh)
    bpy.context.collection.objects.link(obj)
    bm = bmesh.new()
    weights, uvs = {}, {}
    add_body(bm, weights, uvs, bones)
    for side in ("L", "R"):
        shared.add_membrane(bm, weights, uvs, bones, side,
                            grid_builder=bat_wing_grid)
        add_wing_bones(bm, weights, uvs, bones, side)
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
        "creature": "dire-bat",
        "rigSha256": hashlib.sha256(Path(args.rig).read_bytes()).hexdigest(),
        "rigBoneCount": len(rig["bones"]),
        "vertices": len(mesh.vertices),
        "polygons": len(mesh.polygons),
        "boneGroups": len(groups),
        "maxInfluencesPerVertex": max(len(e) for e in indexed.values()),
        "extent": {"x": [min(xs), max(xs)], "y": [min(ys), max(ys)],
                   "z": [min(zs), max(zs)]},
        "uvAtlas": {name: list(region)
                    for name, region in shared.ATLAS.items()},
        "albedo": shared.albedo_manifest(args.albedo),
    }
    if report["maxInfluencesPerVertex"] > 4:
        raise SystemExit("A vertex exceeds four bone influences")
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
    print("[dire-bat] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
