#!/usr/bin/env python3
"""Build four original skinned ungulates from private live donor bind frames.

The capture is a machine-local input. Only original geometry, UVs, named bone
weights and an original albedo enter the shipped schema-2 mesh-data file.
The Blender project and FBX retain donor transforms and stay local.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
import random
import sys

import bpy
import bmesh
from mathutils import Vector

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "pteranodon"))
import generate_pteranodon as shared  # noqa: E402

KINDS = ("aurochs", "bison", "rhinoceros", "woolly-rhinoceros")


def measured_rig(path, kind):
    raw = Path(path).read_bytes()
    captured = json.loads(raw)
    renderers = captured.get("renderers", [])
    if len(renderers) != 1:
        raise SystemExit("ungulate donor must have exactly one skinned renderer")
    source = renderers[0]
    rows = source["bones"]
    if source["boneCount"] != source["bindPoseCount"] or len(rows) != source["boneCount"]:
        raise SystemExit("donor bind frame is incomplete")
    expected = "horse" if kind in ("aurochs", "bison") else "mastodon"
    if expected not in captured["source"]:
        raise SystemExit("wrong donor bind frame for " + kind)
    by_name = {row["name"]: row for row in rows}
    if len(by_name) != len(rows):
        raise SystemExit("duplicate donor bone")
    children = {}
    for row in rows:
        if row["parent"] in by_name:
            children.setdefault(row["parent"], []).append(row["name"])

    def depth(name):
        parent = by_name[name]["parent"]
        return depth(parent) + 1 if parent in by_name else 0

    bones = []
    for row in rows:
        name = row["name"]
        head = Vector(row["bindPosition"])
        child_names = children.get(name, [])
        if child_names:
            tail = sum((Vector(by_name[child]["bindPosition"])
                        for child in child_names), Vector()) / len(child_names)
        else:
            tail = head + Vector((0, 0.05, 0))
        if (tail - head).length < 0.001:
            tail = head + Vector((0, 0.05, 0))
        bones.append({"name": name, "parent": row["parent"],
                      "index": row["index"], "depth": depth(name),
                      "head": list(head), "tail": list(tail)})
    rig = {"source": captured["source"], "space": captured["space"],
           "renderer": source["renderer"], "rootBone": source["rootBone"],
           "bindPoseCount": source["bindPoseCount"], "bones": bones}
    return rig, {row["name"]: row for row in bones}, hashlib.sha256(raw).hexdigest()


def tube(bm, weights, uvs, points, radii, bones, region="body", segments=12):
    shared.add_tube(bm, weights, uvs, points, radii, bones, region,
                    segments=segments)


def elliptical_tube(bm, weights, uvs, points, widths, heights, bone,
                    region="beak", segments=10, cap_start=True, cap_end=True):
    """Build a closed, flattened tube with one exact donor-bone owner.

    Hooves need a broad horizontal footprint and a shallow vertical profile.
    The shared circular tube made each short foot read as a thin radial fan
    when animated, so feet use explicit width and height at every ring.
    """
    if not (len(points) == len(widths) == len(heights)) or len(points) < 2:
        raise ValueError("elliptical tube needs matching ring profiles")
    lengths = [0.0]
    for index in range(1, len(points)):
        lengths.append(lengths[-1] +
                       (points[index] - points[index - 1]).length)
    total = lengths[-1] or 1.0
    rings = []
    for index, centre in enumerate(points):
        if widths[index] <= 0.0 and heights[index] <= 0.0:
            vertex = bm.verts.new(centre)
            weights[vertex] = [(bone, 1.0)]
            uvs[vertex] = shared.region_uv(
                region, lengths[index] / total, 0.5)
            rings.append([vertex])
            continue
        if index == 0:
            direction = points[1] - points[0]
        elif index == len(points) - 1:
            direction = points[-1] - points[-2]
        else:
            direction = points[index + 1] - points[index - 1]
        right, up, _ = shared.basis(direction)
        ring = []
        for step in range(segments):
            angle = 2.0 * math.pi * step / segments
            offset = right * (math.cos(angle) * widths[index]) + \
                up * (math.sin(angle) * heights[index])
            vertex = bm.verts.new(centre + offset)
            ring.append(vertex)
            weights[vertex] = [(bone, 1.0)]
            uvs[vertex] = shared.region_uv(
                region, lengths[index] / total, shared.fold(angle))
        rings.append(ring)
    for index in range(len(rings) - 1):
        if len(rings[index]) == 1:
            tip = rings[index][0]
            for step in range(segments):
                nxt = (step + 1) % segments
                bm.faces.new((tip, rings[index + 1][step],
                              rings[index + 1][nxt]))
            continue
        if len(rings[index + 1]) == 1:
            tip = rings[index + 1][0]
            for step in range(segments):
                nxt = (step + 1) % segments
                bm.faces.new((rings[index][step], tip,
                              rings[index][nxt]))
            continue
        for step in range(segments):
            nxt = (step + 1) % segments
            bm.faces.new((rings[index][step], rings[index + 1][step],
                          rings[index + 1][nxt], rings[index][nxt]))
    if cap_start and len(rings[0]) > 1:
        bm.faces.new(tuple(reversed(rings[0])))
    if cap_end and len(rings[-1]) > 1:
        bm.faces.new(tuple(rings[-1]))


def ellipsoid(bm, weights, uvs, centre, radii, bone, region="limbs",
              latitudes=6, segments=12):
    """Build one closed, single-bone ellipsoid.

    A rotationally symmetric joint cover can follow either side of a donor
    pivot without opening a seam when the adjoining rigid spans rotate.  The
    same primitive gives Rhinoceros feet a rounded pad instead of a radial
    end cap.
    """
    top = bm.verts.new(centre + Vector((0.0, radii.y, 0.0)))
    bottom = bm.verts.new(centre - Vector((0.0, radii.y, 0.0)))
    weights[top] = [(bone, 1.0)]
    weights[bottom] = [(bone, 1.0)]
    uvs[top] = shared.region_uv(region, 0.5, 1.0)
    uvs[bottom] = shared.region_uv(region, 0.5, 0.0)
    rings = []
    for latitude in range(1, latitudes):
        polar = math.pi * latitude / latitudes
        horizontal = math.sin(polar)
        ring = []
        for step in range(segments):
            angle = 2.0 * math.pi * step / segments
            offset = Vector((math.cos(angle) * radii.x * horizontal,
                             math.cos(polar) * radii.y,
                             math.sin(angle) * radii.z * horizontal))
            vertex = bm.verts.new(centre + offset)
            ring.append(vertex)
            weights[vertex] = [(bone, 1.0)]
            uvs[vertex] = shared.region_uv(region, 0.5,
                                           shared.fold(angle))
        rings.append(ring)
    for step in range(segments):
        nxt = (step + 1) % segments
        bm.faces.new((top, rings[0][step], rings[0][nxt]))
        for latitude in range(len(rings) - 1):
            bm.faces.new((rings[latitude][step],
                          rings[latitude + 1][step],
                          rings[latitude + 1][nxt],
                          rings[latitude][nxt]))
        bm.faces.new((bottom, rings[-1][nxt], rings[-1][step]))


def hoof(bm, weights, uvs, anchor, bone, forward, scale, toes):
    """Author a compact parallel-toed hoof instead of a splayed end disc."""
    lateral = Vector((1.0, 0.0, 0.0))
    if toes == 3:
        # One rounded, flattened pad reads as a massive three-toed foot at
        # party-camera distance.  It has no triangulated end fan or separate
        # projections that can turn into claws under Mastodon animation.
        centre = anchor + forward * (0.15 * scale)
        ellipsoid(bm, weights, uvs, centre,
                  Vector((0.72 * scale, 0.48 * scale, 0.82 * scale)),
                  bone, "beak", 6, 12)
        return
    offsets = [0.0] if toes == 1 else [-0.43, 0.43]
    for offset in offsets:
        length = scale
        side = lateral * (offset * scale)
        points = [anchor + side - forward * (0.32 * length),
                  anchor + side + forward * (0.32 * length),
                  anchor + side + forward * (0.72 * length)]
        elliptical_tube(bm, weights, uvs, points,
                        [0.34 * scale, 0.39 * scale, 0.25 * scale],
                        [0.38 * scale, 0.42 * scale, 0.25 * scale],
                        bone, "beak", 10)


def articulated_leg(bm, weights, uvs, points, radii, bones, segments=12):
    """Build a Rhino limb as overlapping, donor-controlled spindle spans.

    No face crosses a donor pivot.  Every tapered span is rigidly controlled
    by its own chain bone, narrows to a tiny closed tip beyond each shared
    pivot and overlaps the adjoining span.  The tapered tips remove broad cap
    disks and open holes without restoring a stretchable cross-pivot bridge
    or introducing a separate joint solid.
    """
    if not (len(points) == len(radii) == len(bones)) or len(points) < 2:
        raise ValueError("articulated leg needs matching joint profiles")

    # Mastodon's last ankle helper mainly turns the foot.  Giving that short
    # helper its own visible sleeve produced four stacked barrels below the
    # torso, so the lower-leg mass spans knee-to-foot under the knee control
    # while the separate authored foot still follows the native foot bone.
    spans = ((0, 1, 0), (1, 2, 1), (2, 4, 2))
    for span_index, (start_index, end_index, bone_index) in enumerate(spans):
        direction = points[end_index] - points[start_index]
        length = direction.length
        along = direction.normalized()
        overlap = min(length * 0.28,
                      min(radii[start_index], radii[end_index]) * 0.84)
        start = (points[start_index] - along * overlap
                 if span_index > 0 else points[start_index])
        end = points[end_index] + along * overlap
        start_radius = radii[start_index]
        end_radius = radii[end_index]
        start_tip = 0.0 if span_index > 0 else start_radius * 0.90
        elliptical_tube(
            bm, weights, uvs,
            [start, start.lerp(end, 0.08), start.lerp(end, 0.20),
             start.lerp(end, 0.50), start.lerp(end, 0.80),
             start.lerp(end, 0.92), end],
            [start_tip, start_radius * 0.72,
             start_radius * 0.84,
             (start_radius + end_radius) * 0.45,
             end_radius * 0.84, end_radius * 0.72, 0.0],
            [start_tip, start_radius * 0.72,
             start_radius * 0.84,
             (start_radius + end_radius) * 0.45,
             end_radius * 0.84, end_radius * 0.72, 0.0],
            bones[bone_index], "limbs", segments)

def cattle(bm, weights, uvs, rig, kind):
    at = lambda name: shared.head(rig, name)
    bison = kind == "bison"
    hind, upper, chest = at("LowerTorso"), at("UpperTorso"), at("Chest")
    n1, n2, skull, jaw = (at(name) for name in ("Neck1", "Neck2", "Head", "Jaw"))
    bulk = 1.17 if bison else 1.0
    tube(bm, weights, uvs,
         [hind + Vector((0, 0, -0.30)), hind, upper, chest,
          chest + Vector((0, 0.02, 0.24))],
         [0.35, 0.49, 0.52, 0.57, 0.35] if not bison else
         [0.43, 0.58, 0.62, 0.76, 0.46],
         ["LowerTorso", "LowerTorso", "UpperTorso", "Chest", "Chest"])
    tube(bm, weights, uvs, [chest, n1, n2, skull],
         [0.53, 0.47, 0.37, 0.31] if not bison else
         [0.69, 0.63, 0.43, 0.34],
         ["Chest", "Neck1", "Neck2", "Head"])
    if bison:
        tube(bm, weights, uvs,
             [upper + Vector((0, 0.13, 0.04)),
              chest + Vector((0, 0.48, -0.03)),
              n1 + Vector((0, 0.25, -0.09))],
             [0.48, 0.57, 0.27], ["UpperTorso", "Chest", "Neck1"])
    muzzle = skull + Vector((0, -0.17, 0.32))
    tube(bm, weights, uvs,
         [skull, muzzle, muzzle + Vector((0, -0.09, 0.25))],
         [0.31, 0.26, 0.19], ["Head"] * 3, "beak")
    tube(bm, weights, uvs,
         [jaw, jaw + Vector((0, -0.06, 0.19))],
         [0.23, 0.17], ["Jaw"] * 2, "beak", 10)
    for side in ("L", "R"):
        sign = -1 if side == "L" else 1
        base = skull + Vector((sign * 0.23, 0.09, 0.03))
        if bison:
            horn = [base, base + Vector((sign * 0.25, -0.03, 0.03)),
                    base + Vector((sign * 0.43, 0.23, 0.07))]
            radii = [0.115, 0.075, 0.008]
        else:
            horn = [base, base + Vector((sign * 0.31, 0.14, 0.04)),
                    base + Vector((sign * 0.62, 0.24, -0.04)),
                    base + Vector((sign * 0.80, 0.43, 0.02))]
            radii = [0.13, 0.10, 0.055, 0.006]
        tube(bm, weights, uvs, horn, radii, ["Head"] * len(horn),
             "crest", 9)
        for prefix, chain in (("front", ("Arm_Upper", "Arm_Lower", "Palm", "Fingers")),
                              ("hind", ("Leg0_Upper", "Leg0_Lower", "Foot0", "Toes0"))):
            names = [side + "_" + name for name in chain]
            points = [at(name) + Vector((sign * (0.20 if prefix == "front" else 0.12)
                                        * bulk, 0, 0)) for name in names]
            tube(bm, weights, uvs, points,
                 [0.19, 0.17, 0.14, 0.125] if bison else
                 [0.17, 0.145, 0.12, 0.105], names, "limbs", 10)
            hoof(bm, weights, uvs,
                 points[-1] + Vector((0, -0.055, 0)), names[-1],
                 Vector((0, 0, 1)), 0.25 if bison else 0.22, 2)
    tail_names = ["Tail1", "Tail2", "Tail3", "Tail4", "Tail5"]
    tube(bm, weights, uvs, [at(name) for name in tail_names],
         [0.10, 0.07, 0.055, 0.045, 0.085], tail_names, "limbs", 8)
    if bison:
        tube(bm, weights, uvs,
             [jaw, jaw + Vector((0, -0.25, 0.02)),
              jaw + Vector((0, -0.47, 0.10))],
             [0.19, 0.14, 0.02], ["Jaw"] * 3, "body", 8)
        tuft_field(bm, weights, uvs, chest, "Chest", 25, 0.63, 0.50, 202611)


def tuft_field(bm, weights, uvs, centre, bone, count, spread, length, seed):
    rng = random.Random(seed)
    for _ in range(count):
        x = rng.uniform(-spread, spread)
        z = rng.uniform(-spread, spread)
        start = centre + Vector((x, rng.uniform(0.18, 0.44), z))
        tip = start + Vector((x * 0.08, length * rng.uniform(0.40, 0.80),
                              rng.uniform(-0.16, 0.08)))
        tube(bm, weights, uvs, [start, tip], [0.075, 0.008],
             [bone, bone], "body", 5)


def rhinoceros(bm, weights, uvs, rig, kind):
    at = lambda name: shared.head(rig, name)
    woolly = kind == "woolly-rhinoceros"
    rear, spine, shoulder, neck, skull = (at(name) for name in
        ("LowerTorso", "Spine", "UpperTorso", "Neck", "Head"))
    tube(bm, weights, uvs,
         [rear + Vector((0, 0, 0.28)), rear, spine, shoulder, neck],
         [0.75, 1.02, 1.12, 1.12, 0.83] if not woolly else
         [0.91, 1.20, 1.31, 1.28, 0.94],
         ["LowerTorso", "LowerTorso", "Spine", "UpperTorso", "Neck"])
    tube(bm, weights, uvs,
         [neck, skull, skull + Vector((0, -0.14, -0.45)),
          skull + Vector((0, -0.24, -0.78))],
         [0.83, 0.72, 0.56, 0.40] if not woolly else
         [0.94, 0.81, 0.61, 0.46],
         ["Neck", "Head", "Head", "Head"])
    # Nasal and brow horns are head-weighted keratin, never mastodon tusks.
    nose = skull + Vector((0, -0.01, -0.60))
    tube(bm, weights, uvs,
         [nose, nose + Vector((0, 0.28, -0.27)),
          nose + Vector((0, 0.94 if woolly else 0.80, -0.72))],
         [0.26, 0.17, 0.008], ["Head"] * 3, "crest", 11)
    brow = skull + Vector((0, 0.32, -0.17))
    tube(bm, weights, uvs,
         [brow, brow + Vector((0, 0.24, -0.11)),
          brow + Vector((0, 0.53, -0.27))],
         [0.16, 0.10, 0.007], ["Head"] * 3, "crest", 9)
    for side in ("L", "R"):
        sign = 1 if side == "L" else -1
        ear = skull + Vector((sign * 0.52, 0.27, 0.07))
        tube(bm, weights, uvs,
             [ear, ear + Vector((sign * 0.22, 0.30, 0.15))],
             [0.20, 0.015], ["Head"] * 2, "body", 8)
        front_names = [side + "_Arm_Upper", side + "_Arm_Lower",
                       "frontKnee_" + side, "frontAnkle_" + side,
                       side + "_Palm"]
        rear_names = [side + "_Leg0_Upper", side + "_Leg0_Lower",
                      "backKnee_" + side, "backAnkle_" + side,
                      side + "_Foot0"]
        for names in (front_names, rear_names):
            points = [at(name) for name in names]
            radii = ([0.50, 0.46, 0.40, 0.35, 0.31] if woolly else
                     [0.45, 0.41, 0.36, 0.32, 0.28])
            articulated_leg(bm, weights, uvs, points, radii, names)
            hoof(bm, weights, uvs, points[-1], names[-1],
                 Vector((0, 0, -1)), 0.52 if woolly else 0.48, 3)
    tail = ["Tail0_M", "Tail1_M", "Tail2_M", "Tail3_M", "Tail4_M"]
    tube(bm, weights, uvs, [at(name) for name in tail],
         [0.16, 0.13, 0.10, 0.07, 0.04], tail, "limbs", 8)
    if woolly:
        tuft_field(bm, weights, uvs, shoulder, "UpperTorso", 40,
                   1.02, 0.40, 202612)
        tuft_field(bm, weights, uvs, neck, "Neck", 28,
                   0.62, 0.30, 202613)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", choices=KINDS, required=True)
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out", "fbx-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:]
                             if "--" in sys.argv else [])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = measured_rig(args.capture, args.kind)
    armature = shared.build_armature(rig_data)
    armature.name = args.kind + "RigPreview"
    mesh = bpy.data.meshes.new(args.kind + "Mesh")
    obj = bpy.data.objects.new(args.kind, mesh)
    bpy.context.collection.objects.link(obj)
    bm = bmesh.new()
    weights, uvs = {}, {}
    if args.kind in ("aurochs", "bison"):
        cattle(bm, weights, uvs, rig, args.kind)
    else:
        rhinoceros(bm, weights, uvs, rig, args.kind)
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
    xs, ys, zs = ([getattr(vertex.co, axis) for vertex in mesh.vertices]
                  for axis in ("x", "y", "z"))
    report = {"schemaVersion": 3, "creature": args.kind,
              "rigSha256": rig_hash,
              "rigBoneCount": len(rig_data["bones"]),
              "vertices": len(mesh.vertices), "polygons": len(mesh.polygons),
              "boneGroups": len(groups),
              "maxInfluencesPerVertex": max(map(len, indexed.values())),
              "extent": {"x": [min(xs), max(xs)],
                         "y": [min(ys), max(ys)], "z": [min(zs), max(zs)]},
              "uvAtlas": {name: list(region)
                          for name, region in shared.ATLAS.items()},
              "albedo": shared.albedo_manifest(args.albedo)}
    if report["maxInfluencesPerVertex"] > 4:
        raise SystemExit("vertex has too many donor influences")
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    shared.write_mesh_data(args.mesh_data, obj, mesh, indexed, uv_indexed,
                           report, args.albedo)
    payload = json.loads(Path(args.mesh_data).read_text(encoding="utf-8"))
    payload["space"] = ("donor renderer local; +X right, +Y up, +Z forward"
                        if args.kind in ("aurochs", "bison") else
                        "donor renderer local; +X left, +Y up, -Z forward")
    # The shared exporter uses the host's text newline. Normalize this final
    # schema file to LF so Git checkout and standalone package bytes agree.
    with Path(args.mesh_data).open("w", encoding="utf-8", newline="\n") as out:
        out.write(json.dumps(payload, indent=1) + "\n")
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.export_scene.fbx(filepath=args.fbx_out, use_selection=True,
        add_leaf_bones=False, bake_anim=False, path_mode="COPY",
        apply_scale_options="FBX_SCALE_ALL", object_types={"ARMATURE", "MESH"})
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[ungulate] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
