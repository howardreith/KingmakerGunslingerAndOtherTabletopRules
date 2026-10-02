#!/usr/bin/env python3
"""Author a Tiny blood-drinking Stirge on the measured flying-creature rig.

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
    # A short, furry head and thorax, yellow ventral belly, and small tapered
    # abdomen. The forward needle is a mouthpart, not the rear Wasp stinger.
    tube(bm, weights, uvs,
         [head + Vector((0, -0.06, -0.10)), head + Vector((0, 0.04, 0.13))],
         [0.19, 0.23], ["Head", "Head"], "body", 12)
    tube(bm, weights, uvs,
         [head + Vector((0, 0.04, 0.13)),
          neck + Vector((0, -0.10, 0.12))],
         [0.16, 0.21], ["Head", "Neck"], "body", 10)
    tube(bm, weights, uvs,
         [neck + Vector((0, -0.10, 0.12)), upper, lower + Vector((0, 0.12, -0.05))],
         [0.22, 0.34, 0.28], ["Neck", "UpperTorso", "LowerTorso"],
         "body", 14)
    tube(bm, weights, uvs,
         [lower + Vector((0, 0.12, 0.05)), tail + Vector((0, 0.34, -0.02))],
         [0.24, 0.23], ["LowerTorso", "Tail"], "body", 10)
    tube(bm, weights, uvs,
         [tail + Vector((0, 0.32, -0.04)), Vector((0, 1.22, 0.78)),
          Vector((0, 1.10, 1.16))],
         [0.28, 0.36, 0.07], ["Tail", "Tail", "Tail"], "body", 16)
    # The single conspicuous forward proboscis ends beyond the six legs.
    tube(bm, weights, uvs,
         [head + Vector((0, -0.12, -0.19)),
          head + Vector((0, -0.19, -0.58)),
          head + Vector((0, -0.26, -1.12))],
         [0.065, 0.034, 0.003], ["Head", "Head", "Head"], "beak", 10)
    # Small dark mammalian eyes; no compound-eye globes, antennae or mandibles.
    for side in (-1, 1):
        sign = float(side)
        tube(bm, weights, uvs,
             [head + Vector((sign * 0.15, 0.025, -0.14)),
              head + Vector((sign * 0.21, 0.020, -0.19))],
             [0.075, 0.075], ["Head", "Head"], "crest", 10)


def legs(bm, weights, uvs, rig):
    for side in ("L", "R"):
        sign = 1.0 if side == "L" else -1.0
        for index, along in enumerate((-0.44, -0.08, 0.26)):
            rise = 1.48 if index == 0 else 1.39 if index == 1 else 1.28
            anchor = "UpperTorso" if index == 0 else "LowerTorso"
            tube(bm, weights, uvs,
                 [Vector((sign * 0.23, rise, along)),
                  Vector((sign * (0.46 + index * 0.06), rise - 0.29,
                          along - 0.14)),
                  Vector((sign * (0.70 + index * 0.09), 0.78,
                          along - 0.22)),
                  Vector((sign * (0.74 + index * 0.10), 0.64,
                          along - 0.31))],
                 [0.045, 0.028, 0.015, 0.003],
                 [anchor, side + "_Leg0_Upper", side + "_Leg0_Lower",
                  side + "_Leg0_Lower"],
                 "limbs", 7)


def wing(bm, weights, uvs, rig, side, rear=False):
    sign = 1.0 if side == "L" else -1.0
    root = shared.head(rig, side + "_Arm_Upper") + \
        Vector((0, 0, 0.21 if rear else -0.07))
    elbow = shared.head(rig, side + "_Arm_Lower") + \
        Vector((0, -0.10, 0.34 if rear else -0.10))
    tip = shared.head(rig, side + "_Palm") * (0.65 if rear else 0.86) + \
        shared.head(rig, side + "_Arm_Lower") * (0.35 if rear else 0.14)
    tip += Vector((0, -0.15, 0.57 if rear else -0.06))
    # A scalloped, fleshy membrane is framed by two long struts. Its
    # trailing inward notch distinguishes it from the Wasp's lanceolate wing.
    trailing_tip = tip + Vector((-sign * 0.20, -0.02, 0.52))
    notch = elbow + Vector((sign * 0.14, -0.05,
                            0.24 if rear else 0.31))
    trailing_root = root + Vector((sign * 0.05, -0.02,
                                   0.54 if rear else 0.66))
    outline = [(root, side + "_Arm_Upper", (0.0, 0.0)),
               (elbow, side + "_Arm_Lower", (0.34, 0.12)),
               (tip, side + "_Palm", (1.0, 0.36)),
               (trailing_tip, side + "_Palm", (0.85, 1.0)),
               (notch, side + "_Arm_Lower", (0.52, 0.52)),
               (trailing_root, side + "_Arm_Upper", (0.16, 0.96))]
    # Two pairs of two-sided fleshy batlike wings, separately silhouetted.
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
         [0.036, 0.028, 0.004],
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
    armature.name = "StirgeRigPreview"
    mesh = bpy.data.meshes.new("StirgeMesh")
    obj = bpy.data.objects.new("Stirge", mesh)
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
    report = {"schemaVersion": 3, "creature": "stirge",
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
    print("[stirge] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
