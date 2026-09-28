#!/usr/bin/env python3
"""Author a six-legged Giant Wasp on the measured flying-creature bind rig.

Only original geometry, weights, UVs and painting are exported. The local rig,
Blender project and FBX stay private; no native mesh or bind transform ships.
"""
import argparse
import hashlib
import json
from pathlib import Path
import sys

import bpy
import bmesh
from mathutils import Vector

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "pteranodon"))
import generate_pteranodon as shared  # noqa: E402


def tube(bm, weights, uvs, points, radii, bones, region, segments=10):
    shared.add_tube(bm, weights, uvs, points, radii, bones, region,
                    segments=segments)


def body(bm, weights, uvs, rig):
    lower = shared.head(rig, "LowerTorso")
    upper = shared.head(rig, "UpperTorso")
    neck = shared.head(rig, "Neck")
    head = shared.head(rig, "Head")
    tail = shared.head(rig, "Tail")
    # A compact head and muscular thorax precede a narrow petiole and striped
    # abdomen; none of these tubes follows the donor eagle's long neck.
    tube(bm, weights, uvs,
         [head + Vector((0, -0.08, -0.10)), head + Vector((0, 0.04, 0.17))],
         [0.20, 0.28], ["Head", "Head"], "body", 12)
    tube(bm, weights, uvs,
         [head + Vector((0, 0.04, 0.16)),
          neck + Vector((0, -0.10, 0.16))],
         [0.14, 0.18], ["Head", "Neck"], "limbs", 10)
    tube(bm, weights, uvs,
         [neck + Vector((0, -0.10, 0.15)), upper, lower + Vector((0, 0.22, -0.06))],
         [0.22, 0.39, 0.25], ["Neck", "UpperTorso", "LowerTorso"],
         "body", 14)
    tube(bm, weights, uvs,
         [lower + Vector((0, 0.20, 0.05)), tail + Vector((0, 0.43, -0.02))],
         [0.13, 0.17], ["LowerTorso", "Tail"], "limbs", 10)
    tube(bm, weights, uvs,
         [tail + Vector((0, 0.42, -0.05)), Vector((0, 1.46, 0.91)),
          Vector((0, 1.31, 1.30))],
         [0.29, 0.49, 0.18], ["Tail", "Tail", "Tail"], "body", 16)
    tube(bm, weights, uvs,
         [Vector((0, 1.31, 1.27)), Vector((0, 1.06, 1.90)),
          Vector((0, 0.92, 2.38))],
         [0.14, 0.055, 0.004], ["Tail", "Tail", "Tail"], "beak", 10)
    # Dark compound eyes, antennae and paired mandibles identify the insect
    # at camera distance, independently of the striped abdomen.
    for side in (-1, 1):
        sign = float(side)
        tube(bm, weights, uvs,
             [head + Vector((sign * 0.18, 0.025, -0.13)),
              head + Vector((sign * 0.27, 0.035, -0.18))],
             [0.105, 0.11], ["Head", "Head"], "crest", 10)
        tube(bm, weights, uvs,
             [head + Vector((sign * 0.10, 0.17, -0.18)),
              head + Vector((sign * 0.20, 0.29, -0.49)),
              head + Vector((sign * 0.27, 0.23, -0.68))],
             [0.045, 0.028, 0.005], ["Head"] * 3, "limbs", 7)
        tube(bm, weights, uvs,
             [head + Vector((sign * 0.09, -0.11, -0.21)),
              head + Vector((sign * 0.12, -0.19, -0.39))],
             [0.08, 0.005], ["Jaw", "Jaw"], "beak", 8)


def legs(bm, weights, uvs, rig):
    for side in ("L", "R"):
        sign = 1.0 if side == "L" else -1.0
        for index, along in enumerate((-0.48, -0.10, 0.30)):
            rise = 1.59 if index == 0 else 1.42 if index == 1 else 1.28
            anchor = "UpperTorso" if index == 0 else "LowerTorso"
            tube(bm, weights, uvs,
                 [Vector((sign * 0.25, rise, along)),
                  Vector((sign * (0.54 + index * 0.05), rise - 0.37,
                          along + 0.06)),
                  Vector((sign * (0.76 + index * 0.10), 0.66,
                          along + 0.15))],
                 [0.065, 0.045, 0.014],
                 [anchor, side + "_Leg0_Upper", side + "_Leg0_Lower"],
                 "limbs", 7)


def wing(bm, weights, uvs, rig, side, rear=False):
    sign = 1.0 if side == "L" else -1.0
    root = shared.head(rig, side + "_Arm_Upper") + \
        Vector((0, 0, 0.25 if rear else -0.04))
    elbow = shared.head(rig, side + "_Arm_Lower") + \
        Vector((0, -0.08, 0.36 if rear else -0.08))
    tip = shared.head(rig, side + "_Palm") * (0.65 if rear else 0.86) + \
        shared.head(rig, side + "_Arm_Lower") * (0.35 if rear else 0.14)
    tip += Vector((0, -0.15, 0.62 if rear else -0.03))
    trailing = elbow + Vector((sign * 0.23, -0.06,
                               0.71 if rear else 0.53))
    outline = [(root, side + "_Arm_Upper", (0.0, 0.0)),
               (elbow, side + "_Arm_Lower", (0.34, 0.12)),
               (tip, side + "_Palm", (1.0, 0.36)),
               (trailing, side + "_Arm_Lower", (0.52, 1.0))]
    # Thin, two-sided lanceolate membrane; two pairs are separate silhouettes.
    layers = []
    for depth in (-0.007, 0.007):
        verts = []
        for position, bone, uv in outline:
            vert = bm.verts.new(position + Vector((0, depth, 0)))
            weights[vert] = [(bone, 1.0)]
            uvs[vert] = shared.region_uv("membrane", *uv)
            verts.append(vert)
        bm.faces.new(tuple(verts if depth > 0 else reversed(verts)))
        layers.append(verts)
    for index in range(len(outline)):
        next_index = (index + 1) % len(outline)
        bm.faces.new((layers[0][index], layers[0][next_index],
                      layers[1][next_index], layers[1][index]))
    tube(bm, weights, uvs, [root, elbow, tip],
         [0.028, 0.026, 0.004],
         [side + "_Arm_Upper", side + "_Arm_Lower", side + "_Palm"],
         "limbs", 6)


def main():
    parser = argparse.ArgumentParser()
    for name in ("rig", "albedo", "mesh-data", "report", "blend-out",
                 "fbx-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:]
                             if "--" in sys.argv else [])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig = shared.load_rig(args.rig)
    armature = shared.build_armature(rig_data)
    armature.name = "GiantWaspRigPreview"
    mesh = bpy.data.meshes.new("GiantWaspMesh")
    obj = bpy.data.objects.new("GiantWasp", mesh)
    bpy.context.collection.objects.link(obj)
    bm = bmesh.new()
    weights, uvs = {}, {}
    body(bm, weights, uvs, rig)
    legs(bm, weights, uvs, rig)
    for side in ("L", "R"):
        wing(bm, weights, uvs, rig, side)
        wing(bm, weights, uvs, rig, side, rear=True)
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
    xs, ys, zs = ([getattr(vert.co, axis) for vert in mesh.vertices]
                  for axis in ("x", "y", "z"))
    report = {"schemaVersion": 3, "creature": "giant-wasp",
              "rigSha256": hashlib.sha256(Path(args.rig).read_bytes()).hexdigest(),
              "rigBoneCount": len(rig_data["bones"]),
              "vertices": len(mesh.vertices), "polygons": len(mesh.polygons),
              "boneGroups": len(groups),
              "maxInfluencesPerVertex": max(len(e) for e in indexed.values()),
              "extent": {"x": [min(xs), max(xs)], "y": [min(ys), max(ys)],
                         "z": [min(zs), max(zs)]},
              "uvAtlas": {name: list(region)
                          for name, region in shared.ATLAS.items()},
              "albedo": shared.albedo_manifest(args.albedo)}
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
    print("[giant-wasp] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
