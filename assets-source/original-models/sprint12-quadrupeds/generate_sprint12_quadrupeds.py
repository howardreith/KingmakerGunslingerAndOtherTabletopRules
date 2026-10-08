#!/usr/bin/env python3
"""Build three original Sprint 12 quadrupeds on private donor bind frames.

The request-local capture supplies only bone names and a measured renderer-local
bind frame. Shipped mesh data contains original geometry, UVs, weights, bone
names and an original albedo hash; no donor transform or native art ships.
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


KINDS = ("dire-rat", "hyena", "goblin-dog")
DONORS = {"dire-rat": "dog", "hyena": "wolf", "goblin-dog": "worg"}


def measured_rig(path, kind):
    raw = Path(path).read_bytes()
    captured = json.loads(raw)
    donor = DONORS[kind]
    expected = "hidden Sprint 12 " + donor + " donor view"
    if expected not in captured.get("source", ""):
        raise SystemExit("wrong donor bind frame for " + kind)
    renderers = captured.get("renderers", [])
    if len(renderers) != 1:
        raise SystemExit("Sprint 12 donor must have exactly one renderer")
    source = renderers[0]
    rows = source.get("bones", [])
    if not rows or source.get("boneCount") != source.get("bindPoseCount") or \
            len(rows) != source.get("boneCount"):
        raise SystemExit("donor bind frame is incomplete")
    by_name = {row["name"]: row for row in rows}
    if len(by_name) != len(rows):
        raise SystemExit("donor bind frame repeats a bone name")
    if any(len(row.get("bindPosition", [])) != 3 for row in rows):
        raise SystemExit("donor bind frame has an invalid position")
    children = {}
    for row in rows:
        if row.get("parent") in by_name:
            children.setdefault(row["parent"], []).append(row["name"])

    def depth(name):
        parent = by_name[name].get("parent")
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
            # The axis only exists in the private preview armature. Runtime
            # binding uses the donor's own complete bind-pose array.
            tail = head + Vector((0.0, 0.05, 0.0))
        if (tail - head).length < 0.001:
            tail = head + Vector((0.0, 0.05, 0.0))
        bones.append({"name": name, "parent": row.get("parent", ""),
                      "index": row["index"], "depth": depth(name),
                      "head": list(head), "tail": list(tail)})
    rig = {"source": captured["source"], "space": captured["space"],
           "renderer": source["renderer"], "rootBone": source["rootBone"],
           "bindPoseCount": source["bindPoseCount"], "bones": bones}
    return rig, {row["name"]: row for row in bones}, \
        hashlib.sha256(raw).hexdigest()


def tube(bm, weights, uvs, points, radii, bones, region="body", segments=10):
    if len(points) < 2 or not (len(points) == len(radii) == len(bones)):
        raise ValueError("tube needs matching point, radius and bone rows")
    shared.add_tube(bm, weights, uvs, points, radii, bones, region,
                    segments=segments)


def ellipsoid(bm, weights, uvs, centre, lateral, up, forward, radii, bone,
              region="body", latitudes=7, segments=12):
    """Closed single-bone ellipsoid in an explicit creature coordinate frame."""
    lateral = lateral.normalized()
    up = up.normalized()
    forward = forward.normalized()
    top = bm.verts.new(centre + up * radii[1])
    bottom = bm.verts.new(centre - up * radii[1])
    weights[top] = [(bone, 1.0)]
    weights[bottom] = [(bone, 1.0)]
    uvs[top] = shared.region_uv(region, 0.5, 1.0)
    uvs[bottom] = shared.region_uv(region, 0.5, 0.0)
    rings = []
    import math
    for latitude in range(1, latitudes):
        polar = math.pi * latitude / latitudes
        horizontal = math.sin(polar)
        ring = []
        for step in range(segments):
            angle = 2.0 * math.pi * step / segments
            offset = lateral * (math.cos(angle) * radii[0] * horizontal) + \
                forward * (math.sin(angle) * radii[2] * horizontal) + \
                up * (math.cos(polar) * radii[1])
            vertex = bm.verts.new(centre + offset)
            weights[vertex] = [(bone, 1.0)]
            uvs[vertex] = shared.region_uv(
                region, step / float(segments),
                1.0 - latitude / float(latitudes))
            ring.append(vertex)
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


def tuft(bm, weights, uvs, base, tip, radius, bone):
    tube(bm, weights, uvs, [base, base.lerp(tip, 0.55), tip],
         [radius, radius * 0.58, max(radius * 0.04, 0.0008)],
         [bone, bone, bone], "crest", 6)


def paw(bm, weights, uvs, centre, lateral, up, forward, scale, bone):
    ellipsoid(bm, weights, uvs, centre + forward * scale * 0.28,
              lateral, up, forward, (scale * 0.62, scale * 0.24,
              scale * 0.90), bone, "limbs", 5, 10)


def eyes(bm, weights, uvs, centre, lateral, up, forward, spread, size, bone):
    for sign in (-1.0, 1.0):
        eye = centre + lateral * (spread * sign) + up * (size * 0.45) + \
            forward * (size * 1.8)
        ellipsoid(bm, weights, uvs, eye, lateral, up, forward,
                  (size, size, size * 0.55), bone, "crest", 5, 10)


def dire_rat(bm, weights, uvs, rig):
    at = lambda name: shared.head(rig, name)
    side, up, forward = (Vector((1, 0, 0)), Vector((0, 0, 1)),
                         Vector((0, -1, 0)))
    pelvis, lower, upper = (at(name) for name in
                            ("Pelvis", "LowerTorso", "UpperTorso"))
    n0, n1, skull, nose, jaw = (at(name) for name in
        ("Neck0", "Neck1", "Head", "Nose", "Jaw"))
    tube(bm, weights, uvs,
         [at("Tail00"), pelvis, lower, upper, n0, n1],
         [17.0, 33.0, 35.0, 30.0, 22.0, 16.0],
         ["Tail00", "Pelvis", "LowerTorso", "UpperTorso", "Neck0", "Neck1"],
         "body", 12)
    # Wedge the head into the long pointed rat muzzle; the terminal Nose bone
    # keeps the snout stable through the donor's bite animation.
    tube(bm, weights, uvs,
         [n1, skull, skull + forward * 18.0, nose,
          nose + forward * 12.0 + up * -1.5],
         [15.0, 19.0, 15.0, 9.0, 3.2],
         ["Neck1", "Head", "Head", "Nose", "Nose"], "body", 12)
    tube(bm, weights, uvs,
         [jaw + up * -1.0, jaw + forward * 20.0 + up * -3.0,
          nose + forward * 9.0 + up * -5.0],
         [10.5, 7.0, 2.2], ["Jaw", "Jaw", "Jaw"], "beak", 10)
    for ear_name in ("L_Ear", "R_Ear"):
        ellipsoid(bm, weights, uvs, at(ear_name) + up * 7.0,
                  side, up, forward, (2.8, 14.5, 10.5), ear_name,
                  "beak", 7, 12)
    eyes(bm, weights, uvs, skull, side, up, forward, 14.5, 3.0, "Head")
    ellipsoid(bm, weights, uvs, nose + forward * 10.0,
              side, up, forward, (5.0, 4.5, 3.5), "Nose", "crest", 5, 10)

    for prefix in ("L", "R"):
        front = [prefix + "_Arm_Upper", prefix + "_Arm_Lower",
                 prefix + "_Palm", prefix + "_Finger0"]
        hind = [prefix + "_Leg0_Upper", prefix + "_Leg0_Lower",
                prefix + "_Leg0_Lower1", prefix + "_Leg0_Foot",
                prefix + "_Leg0_Toe0"]
        tube(bm, weights, uvs, [at(name) for name in front],
             [6.2, 5.4, 4.2, 2.5], front, "limbs", 8)
        tube(bm, weights, uvs, [at(name) for name in hind],
             [7.0, 6.2, 5.0, 4.1, 2.5], hind, "limbs", 8)
        paw(bm, weights, uvs, at(front[-1]), side, up, forward,
            8.0, front[-1])
        paw(bm, weights, uvs, at(hind[-1]), side, up, forward,
            9.2, hind[-1])

    tail_names = ["Tail00", "Tail01", "Tail02", "Tail03"]
    tail_points = [at(name) for name in tail_names]
    direction = (tail_points[-1] - tail_points[-2]).normalized()
    tail_points += [tail_points[-1] + direction * 28.0 + up * -3.0,
                    tail_points[-1] + direction * 58.0 + up * -9.0]
    tube(bm, weights, uvs, tail_points,
         [6.5, 5.2, 4.0, 2.8, 1.5, 0.25],
         tail_names + ["Tail03", "Tail03"], "limbs", 9)
    # Six rigid Head-weighted whiskers survive party-camera mip distance better
    # than alpha cards and contain no extra material or renderer.
    root = nose + forward * 7.0 + up * -1.0
    for sign in (-1.0, 1.0):
        for rise in (-5.0, 0.0, 5.0):
            tip = root + side * sign * 34.0 + forward * 8.0 + up * rise
            tube(bm, weights, uvs, [root, tip], [0.65, 0.16],
                 ["Head", "Head"], "crest", 5)


def canine_leg(bm, weights, uvs, rig, names, radii, side, up, forward,
               paw_scale):
    at = lambda name: shared.head(rig, name)
    tube(bm, weights, uvs, [at(name) for name in names], radii, names,
         "limbs", 9)
    paw(bm, weights, uvs, at(names[-1]), side, up, forward,
        paw_scale, names[-1])


def hyena(bm, weights, uvs, rig):
    at = lambda name: shared.head(rig, name)
    side, up, forward = (Vector((1, 0, 0)), Vector((0, 1, 0)),
                         Vector((0, 0, -1)))
    lower, spine, upper, withers, neck, skull = (at(name) for name in
        ("Torso_Lower", "spine_0", "Torso_Upper", "withers", "neck", "Head"))
    shoulder_centre = upper.lerp(neck, 0.48) + up * 0.08
    tube(bm, weights, uvs,
         [at("tail_01"), lower, spine, upper, shoulder_centre, neck],
         [0.15, 0.28, 0.30, 0.35, 0.36, 0.25],
         ["tail_01", "Torso_Lower", "spine_0", "Torso_Upper", "withers", "neck"],
         "body", 12)
    # 2026-10-01 art repair. The first head was as wide as the torso it sat
    # on - a 0.275 skull against a 0.36 body - and reached 0.34 forward, so a
    # rest-pose review frame read as an amphibian rather than a hyena. The
    # skull drops to 0.235 and the muzzle to 0.29 forward, which keeps the
    # blunt heavy head the species is known for while letting the shoulders
    # stay the widest part of the silhouette.
    muzzle = skull + forward * 0.29 + up * -0.015
    tube(bm, weights, uvs,
         [neck, skull, skull + forward * 0.085,
          skull + forward * 0.205 + up * -0.01, muzzle],
         [0.24, 0.235, 0.205, 0.155, 0.085],
         ["neck", "Head", "Head", "jaw_woo_up_add", "jaw_woo_up"],
         "body", 12)
    tube(bm, weights, uvs,
         [at("jaw"), skull + forward * 0.17 + up * -0.115,
          skull + forward * 0.275 + up * -0.09],
         [0.13, 0.10, 0.034], ["jaw", "jaw_woo_down", "jaw_woo_down"],
         "beak", 10)
    ellipsoid(bm, weights, uvs, muzzle + forward * 0.022,
              side, up, forward, (0.075, 0.062, 0.050), "jaw_woo_up",
              "crest", 5, 10)
    for ear_name, sign in (("ear_L", 1.0), ("ear_R", -1.0)):
        ear_centre = skull + side * sign * 0.17 + up * 0.185 + forward * -0.015
        ellipsoid(bm, weights, uvs, ear_centre,
                  side, up, forward, (0.028, 0.125, 0.100), ear_name,
                  "body", 6, 12)
    eyes(bm, weights, uvs, skull, side, up, forward, 0.185, 0.023, "Head")

    for prefix in ("L", "R"):
        canine_leg(bm, weights, uvs, rig,
            [prefix + "_Arm_Upper", prefix + "_Arm_Lower",
             prefix + "_Palm", "front_paw_tip_" + prefix
             if prefix == "L" else "front_paw__tip_R"],
            [0.105, 0.080, 0.055, 0.035], side, up, forward, 0.105)
        canine_leg(bm, weights, uvs, rig,
            [prefix + "_Leg0_Upper", prefix + "_Leg0_Lower",
             prefix + "_Leg0_Lower2", prefix + "_Foot0",
             "hindpaw_tip_" + prefix],
            [0.115, 0.090, 0.065, 0.050, 0.032], side, up, forward, 0.115)
    # 2026-10-01 art repair. The first tail was thicker in the middle than at
    # its root (0.105 rising to 0.12 against a 0.36 body) and was painted from
    # the body region, so a review frame showed a broad spotted paddle rather
    # than a tail. It now tapers from root to tip the way the Goblin Dog's
    # does, stays in the limb region, and extends past the last bone so the
    # bushy tip is not a flat cut face. A hyena tail is short and brushy, so
    # it keeps more volume than the Goblin Dog's whip tail.
    tail_names = ["tail_01", "tail_02", "tail_03", "tail_04"]
    tail_points = [at(name) for name in tail_names]
    direction = (tail_points[-1] - tail_points[-2]).normalized()
    tail_points.append(tail_points[-1] + direction * 0.16 + up * -0.03)
    tube(bm, weights, uvs, tail_points,
         [0.075, 0.062, 0.042, 0.016, 0.004],
         tail_names + ["tail_04"], "limbs", 9)
    # The raised dark mane and heavy forequarters establish the hyena at the
    # party camera even before its spotted painting resolves.
    #
    # 2026-10-01 art repair. The first mane ran at a fixed height that ignored
    # the body radius under it: 0.27 above a spine of radius 0.30 buried it,
    # 0.35 above an upper torso of radius 0.35 cut it exactly in half, and
    # 0.27 above a neck of radius 0.25 left it floating. A rest-pose frame
    # showed the result as a flat dark sliver lying on the back rather than a
    # mane. Each station now sits clear of its own local radius with the bulk
    # seated inside the body, and the profile peaks over the withers and
    # tapers both ways, which is the shape the species actually carries.
    # The first seated attempt cleared the body but stood so proud of it that
    # it read as a detached slab. The ridge now rises only a little above each
    # local radius, so it follows the dorsal line instead of hovering over it.
    tube(bm, weights, uvs,
         [spine + up * 0.285, upper + up * 0.345,
          shoulder_centre + up * 0.325, neck + up * 0.235],
         [0.040, 0.062, 0.056, 0.034],
         ["spine_0", "Torso_Upper", "withers", "neck"],
         "crest", 8)


def goblin_dog(bm, weights, uvs, rig):
    at = lambda name: shared.head(rig, name)
    side, up, forward = (Vector((1, 0, 0)), Vector((0, 1, 0)),
                         Vector((0, 0, -1)))
    lower, spine, upper, neck, skull = (at(name) for name in
        ("Torso_Lower", "spine_0", "Torso_Upper", "neck", "Head"))
    tube(bm, weights, uvs,
         [at("tail_01"), lower, spine, upper, at("withers"), neck],
         [0.085, 0.19, 0.21, 0.20, 0.18, 0.145],
         ["tail_01", "Torso_Lower", "spine_0", "Torso_Upper", "withers", "neck"],
         "body", 11)
    # Flat rat nose, beady eyes and protruding incisors distinguish the
    # long-legged rodent from both the native Worg and the spotted Hyena.
    upper_muzzle = at("jaw_woo_up")
    tube(bm, weights, uvs,
         [neck, skull, at("jaw_woo_up_add"), upper_muzzle,
          upper_muzzle + forward * 0.16 + up * -0.025],
         [0.16, 0.19, 0.15, 0.115, 0.055],
         ["neck", "Head", "jaw_woo_up_add", "jaw_woo_up", "jaw_woo_up"],
         "body", 11)
    tube(bm, weights, uvs,
         [at("jaw"), at("jaw_woo_down"),
          at("jaw_woo_down") + forward * 0.10 + up * 0.015],
         [0.115, 0.08, 0.025], ["jaw", "jaw_woo_down", "jaw_woo_down"],
         "body", 9)
    ellipsoid(bm, weights, uvs,
              upper_muzzle + forward * 0.17 + up * -0.025,
              side, up, forward, (0.06, 0.052, 0.04), "jaw_woo_up",
              "crest", 5, 10)
    for ear_name in ("ear_L", "ear_R"):
        ellipsoid(bm, weights, uvs, at(ear_name) + up * 0.06,
                  side, up, forward, (0.026, 0.145, 0.105), ear_name,
                  "limbs", 7, 12)
    eyes(bm, weights, uvs, skull, side, up, forward, 0.145, 0.019, "Head")
    for sign in (-1.0, 1.0):
        root = upper_muzzle + side * sign * 0.033 + up * -0.055 + forward * 0.04
        tube(bm, weights, uvs,
             [root, root + forward * 0.12 + up * -0.055],
             [0.028, 0.008], ["jaw_woo_up", "jaw_woo_up"], "beak", 7)

    for prefix in ("L", "R"):
        canine_leg(bm, weights, uvs, rig,
            [prefix + "_Arm_Upper", prefix + "_Arm_Lower",
             prefix + "_Palm", "front_paw_tip_" + prefix
             if prefix == "L" else "front_paw__tip_R"],
            [0.067, 0.050, 0.034, 0.022], side, up, forward, 0.080)
        canine_leg(bm, weights, uvs, rig,
            [prefix + "_Leg0_Upper", prefix + "_Leg0_Lower",
             prefix + "_Leg0_Lower2", prefix + "_Foot0",
             "hindpaw_tip_" + prefix],
            [0.074, 0.056, 0.040, 0.030, 0.020], side, up, forward, 0.086)
    tail_names = ["tail_01", "tail_02", "tail_03", "tail_04"]
    points = [at(name) for name in tail_names]
    direction = (points[-1] - points[-2]).normalized()
    points.append(points[-1] + direction * 0.23 + up * -0.04)
    tube(bm, weights, uvs, points,
         [0.045, 0.035, 0.025, 0.014, 0.003],
         tail_names + ["tail_04"], "limbs", 8)
    # Sparse rigid tufts read as mange without alpha, particles or a second
    # renderer. Most of the hide remains visibly bare and narrow-ribbed.
    for index, (base, bone) in enumerate(((spine, "spine_0"),
                                          (upper, "Torso_Upper"),
                                          (neck, "neck"))):
        for offset in (-0.07, 0.0, 0.07):
            start = base + side * offset + up * (0.18 - index * 0.015)
            tuft(bm, weights, uvs, start,
                 start + up * (0.06 + 0.01 * index) +
                 side * (offset * 0.15), 0.018, bone)


BUILDERS = {"dire-rat": dire_rat, "hyena": hyena,
            "goblin-dog": goblin_dog}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", choices=KINDS, required=True)
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out",
                 "fbx-out"):
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
    BUILDERS[args.kind](bm, weights, uvs, rig)
    bm.normal_update()
    bm.to_mesh(mesh)
    indexed = {vertex.index: entries for vertex, entries in weights.items()}
    uv_indexed = {vertex.index: uv for vertex, uv in uvs.items()}
    bm.free()
    if len(indexed) != len(mesh.vertices) or len(uv_indexed) != len(mesh.vertices):
        raise SystemExit("original mesh has an unweighted or unpainted vertex")
    layer = mesh.uv_layers.new(name="Albedo")
    for loop in mesh.loops:
        layer.data[loop.index].uv = uv_indexed[loop.vertex_index]
    groups = {}
    for entries in indexed.values():
        for name, _ in entries:
            if name not in rig:
                raise SystemExit("mesh references absent donor bone " + name)
            if name not in groups:
                groups[name] = obj.vertex_groups.new(name=name)
    for index, entries in indexed.items():
        total = sum(value for _, value in entries)
        if total <= 0 or len(entries) > 4:
            raise SystemExit("invalid influence row at vertex " + str(index))
        for name, value in entries:
            groups[name].add([index], value / total, "REPLACE")
    modifier = obj.modifiers.new(name="Armature", type="ARMATURE")
    modifier.object = armature
    obj.parent = armature
    shared.attach_preview_material(obj, mesh, args.albedo)
    xs, ys, zs = ([getattr(vertex.co, axis) for vertex in mesh.vertices]
                  for axis in ("x", "y", "z"))
    report = {"schemaVersion": 3, "creature": args.kind,
              "donorFamily": DONORS[args.kind], "rigSha256": rig_hash,
              "rigBoneCount": len(rig_data["bones"]),
              "vertices": len(mesh.vertices), "polygons": len(mesh.polygons),
              "boneGroups": len(groups),
              "maxInfluencesPerVertex": max(map(len, indexed.values())),
              "extent": {"x": [min(xs), max(xs)],
                         "y": [min(ys), max(ys)],
                         "z": [min(zs), max(zs)]},
              "uvAtlas": {name: list(region)
                          for name, region in shared.ATLAS.items()},
              "albedo": shared.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    shared.write_mesh_data(args.mesh_data, obj, mesh, indexed, uv_indexed,
                           report, args.albedo)
    payload = json.loads(Path(args.mesh_data).read_text(encoding="utf-8"))
    payload["space"] = ("donor renderer local; +X right, -Y forward, +Z up"
                        if args.kind == "dire-rat" else
                        "donor renderer local; +X left, +Y up, -Z forward")
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
    print("[sprint12-quadruped] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
