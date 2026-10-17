"""One bounded articulation experiment for the Brachiosaurus neck.

THE PROBLEM, restated from measurement rather than from preference. The donor's
forward chain is a trunk whose bind pose hangs down. An authored sauropod neck
rises, so it leaves that chain's bind path, and the runtime binds our weights to
the DONOR's bind poses - `TryResolveDonorBinding` takes
`donorMesh.bindposes[donorSlot]`. Every chain bone therefore pivots 3.1 to 7.5
units away from the skin it would drive. Redistributing weights over those same
pivots was measured four ways and is closed: full chain 3.46 visible stretch and
10.51 units of displacement, damped to a quarter a collapse to 0.14 of rest
length, and the rigid Head-only control, which survives the probe only because
it no longer answers to it. None of those moved a pivot.

THE PROPOSED MAPPING, stated before it is run.

  Source frames. The donor's forward-chain bones Head, Tail7_M, Tail11_M,
  Tail21_M, Tail31_M, Tail41_M, Tail5_M, Tail6_M - specifically the rotation
  each one has RELATIVE TO ITS OWN REST POSE, which is what a clip animates and
  what survives a retarget. Not their world transforms, and not their positions.

  Target frames. Seven private driver transforms, KMG_BrachioNeck_01..07, owned
  by this project. Driver k's head sits at the k-th joint of the AUTHORED neck.
  Driver 1 is parented to the donor's `Head` bone; driver k to driver k-1. The
  authored neck's base ring stays on the donor's own `Head`.

  The map. Driver k takes the rest-relative rotation of donor chain bone k, and
  applies it in driver k's own rest frame. Expressed as a world-space rotation
  about the driver's own head, that is the same rotation the donor bone applies
  about its head - which is why the shape of the motion carries across while the
  pivot does not. Copying a raw local quaternion would NOT do this: the two rest
  orientations differ, and the driver would snap to the donor's orientation
  instead of inheriting its delta.

  What this is expected to buy. The neck bends with the donor's own chain
  animation, about joints that lie on the neck. Lever arms fall from 6.4x-33.4x
  to approximately zero by construction.

  What is deliberately unchanged. The donor's body, shoulder, legs and tail
  motion: the driver chain hangs off `Head`, so everything upstream of it is the
  donor's, untouched. Shared donor meshes, transforms, clips and skeleton assets
  are never modified. No global bind pose is swapped. Every other bone in the
  array keeps its donor transform and donor bind pose.

WHAT THIS SCRIPT CAN AND CANNOT SETTLE. It is offline Blender geometry. It can
show whether the mapping produces coherent articulated bending of the authored
neck under a probe that genuinely changes the neck's shape, and it compares that
against the preserved rigid control at the same scale and camera. It cannot
show that the runtime supports the mapping: see RUNTIME BOUNDARY in the report
this writes, which is read from the shipping attach path, not assumed.

Run:
  blender --background --python-exit-code 1 --python neck_retarget_experiment.py \
      -- --census <sprint25-donor-census.json> --out <dir>
"""
import argparse
import json
import math
from pathlib import Path
import sys

import bpy
import bmesh
from mathutils import Matrix, Vector

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
ROOT = HERE.parent
for folder in (ROOT / "pteranodon", ROOT / "sprint12-quadrupeds", HERE):
    sys.path.insert(0, str(folder))
import generate_pteranodon as shared            # noqa: E402
from generate_sprint12_quadrupeds import ellipsoid, tube   # noqa: E402
import graybox_giant_bodies as graybox          # noqa: E402
import prototype_giant_bodies as proto          # noqa: E402

DRIVER_PREFIX = "KMG_BrachioNeck_"
# One probe that the control ignores and one it cannot: the chain probe moves
# the donor's forward chain, and the shoulder probe moves a bone both variants
# answer to. A variant that passes only the first has proved nothing.
PROBE_DEGREES = (8.0, 16.0)


def authored_neck(rest):
    """The authored upright neck's joint positions, from the same builder the
    body uses - graybox's re-aim - so this is the neck that would ship."""
    points = graybox.chain_points(rest, graybox.FORWARD_CHAIN, "Head")
    return graybox.reaim(points, points[0],
                         graybox.UP * 0.82 + graybox.FORWARD * 0.57)


def add_drivers(rig, neck):
    """Append the private driver chain to the donor rig description."""
    by_name = {bone["name"]: bone for bone in rig["bones"]}
    head = by_name["Head"]
    parent = "Head"
    depth = head["depth"]
    for index in range(1, len(neck)):
        depth += 1
        tail = (neck[index + 1] if index + 1 < len(neck)
                else neck[index] + (neck[index] - neck[index - 1]) * 0.5)
        rig["bones"].append({
            "name": "%s%02d" % (DRIVER_PREFIX, index),
            "parent": parent, "index": 1000 + index, "depth": depth,
            "head": list(neck[index]), "tail": list(tail)})
        parent = "%s%02d" % (DRIVER_PREFIX, index)
    return {bone["name"]: bone for bone in rig["bones"]}


def build_neck_body(rest, neck, retargeted):
    """A minimal shoulder-and-neck body: torso stub, neck, small head.

    Deliberately not the whole creature. The question is whether the neck
    bends coherently about pivots that lie on it, and four legs and a tail
    would only add render time and reasons to re-review settled geometry.
    """
    mesh = bpy.data.meshes.new("neck_experiment")
    obj = bpy.data.objects.new("neck_experiment", mesh)
    bpy.context.scene.collection.objects.link(obj)
    bm = bmesh.new()
    weights, uvs = {}, {}

    torso_mid = (rest["LowerTorso"] + rest["UpperTorso"]) * 0.5
    body_len = (rest["UpperTorso"] - rest["LowerTorso"]).length
    radii = (body_len * 0.42, body_len * 0.46, body_len * 0.72)
    neck_base, neck_tip = body_len * 0.22, body_len * 0.055
    lateral, up, forward = graybox.LATERAL, graybox.UP, graybox.FORWARD

    ellipsoid(bm, weights, uvs, torso_mid, lateral, up, forward, radii,
              "UpperTorso", region="body")
    torso = list(range(len(bm.verts)))

    # The same solved anchor the body uses, so the seam under test is the one
    # that would ship rather than a looser one made for this experiment.
    head_joint = neck[0]
    toward = (torso_mid - head_joint).normalized()
    anchor_radius = neck_base * 1.22

    def hull(point):
        offset = point - torso_mid
        return ((offset.dot(lateral) / radii[0]) ** 2 +
                (offset.dot(up) / radii[1]) ** 2 +
                (offset.dot(forward) / radii[2]) ** 2)

    anchor = None
    for step in range(10, 101):
        trial = head_joint + toward * (neck_base * step / 10.0)
        right, lift, _ = shared.basis(head_joint - trial)
        ring = [trial + right * (math.cos(a) * anchor_radius)
                + lift * (math.sin(a) * anchor_radius)
                for a in (2.0 * math.pi * i / graybox.FORWARD_SEGMENTS
                          for i in range(graybox.FORWARD_SEGMENTS))]
        if max(hull(point) for point in ring) <= 0.85:
            anchor = trial
            break
    if anchor is None:
        raise SystemExit("no anchor sink puts the seam ring inside the torso")

    points = [anchor] + list(neck)
    # Driver k owns the authored neck's joint k. The base ring stays on the
    # donor's own Head, which is where the neck's base actually is.
    if retargeted:
        bones = ["UpperTorso", "Head"] + ["%s%02d" % (DRIVER_PREFIX, index)
                                          for index in range(1, len(neck))]
    else:
        bones = ["UpperTorso"] + ["Head"] * len(neck)
    tube_radii = ([anchor_radius] +
                  graybox.taper_along(points[1:], neck_base, neck_tip))
    start = len(bm.verts)
    tube(bm, weights, uvs, points, tube_radii, bones, region="membrane",
         segments=graybox.FORWARD_SEGMENTS)
    neck_verts = list(range(start, len(bm.verts)))
    bm.verts.ensure_lookup_table()
    for index in range(start, start + graybox.FORWARD_SEGMENTS):
        weights[bm.verts[index]] = [("UpperTorso", 1.0)]

    head_size = body_len * 0.115
    head_dir = (points[-1] - points[-2]).normalized()
    head_up = (up - head_dir * up.dot(head_dir)).normalized()
    head_lat = head_dir.cross(head_up).normalized()
    start = len(bm.verts)
    ellipsoid(bm, weights, uvs, points[-1] - head_dir * head_size * 0.30,
              head_lat, head_up, head_dir,
              (head_size * 0.70, head_size * 0.78, head_size),
              bones[-1], region="beak")
    skull = list(range(start, len(bm.verts)))

    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    bm.normal_update()
    bm.verts.index_update()
    bm.to_mesh(mesh)
    indexed = {vert.index: entries for vert, entries in weights.items()}
    uv_indexed = {vert.index: uv for vert, uv in uvs.items()}
    bm.free()
    mesh.validate()
    parts = {"torso": torso, "neck": neck_verts, "skull": skull}
    return obj, indexed, uv_indexed, parts


def neck_centreline(parts, coords):
    size = graybox.FORWARD_SEGMENTS
    rings = parts["neck"]
    return [sum((coords[i] for i in rings[start:start + size]), Vector()) / size
            for start in range(0, len(rings), size)]


def turns(centres):
    rows = []
    for index in range(1, len(centres) - 1):
        first = centres[index] - centres[index - 1]
        second = centres[index + 1] - centres[index]
        if first.length < 1e-6 or second.length < 1e-6:
            continue
        rows.append(round(math.degrees(math.acos(max(-1.0, min(1.0,
            first.normalized().dot(second.normalized()))))), 1))
    return rows


def lever_arms(parts, coords, rig_names, indexed):
    """How far each neck driver sits from the skin it drives. This is the
    number the whole experiment exists to move."""
    out = {}
    owner = {index: max(entries, key=lambda row: row[1])[0]
             for index, entries in indexed.items()}
    for name, bone in rig_names.items():
        if not name.startswith(DRIVER_PREFIX) and name != "Head":
            continue
        owned = [coords[i] for i in parts["neck"] + parts["skull"]
                 if owner.get(i) == name]
        if not owned:
            continue
        centroid = sum(owned, Vector()) / len(owned)
        spread = sum((p - centroid).length for p in owned) / len(owned)
        offset = (centroid - Vector(bone["head"])).length
        out[name] = {"vertices": len(owned),
                     "boneHeadToSkinCentroid": round(offset, 4),
                     "leverArmRatio": round(offset / spread, 3)
                     if spread > 1e-6 else None}
    return out


def run(census, out):
    rest = graybox.load_rest(census)
    neck = authored_neck(rest)
    report = {
        "schemaVersion": 1,
        "experiment": "private neck-driver chain, exact-instance retarget",
        "mapping": {
            "sourceFrames": "donor forward-chain bones Head, Tail7_M, "
                            "Tail11_M, Tail21_M, Tail31_M, Tail41_M, Tail5_M, "
                            "Tail6_M; their rest-relative rotation only",
            "targetFrames": "%s01..07, heads on the authored neck's joints, "
                            "driver 1 parented to the donor Head bone" %
                            DRIVER_PREFIX,
            "rule": "driver k applies donor chain bone k's rest-relative "
                    "rotation in driver k's own rest frame",
            "unchanged": "donor body, shoulder, legs and tail motion; donor "
                         "meshes, transforms, clips and skeleton assets; every "
                         "other bone's transform and bind pose",
        },
        "variants": [],
    }
    print("MAPPING: " + json.dumps(report["mapping"], indent=1))

    framing = {}
    for retargeted in (False, True):
        name = "retargeted" if retargeted else "rigid-control"
        scene, camera, camera_data = proto.setup_scene()
        rig, rig_names, rig_hash = proto.donor_rig(census)
        if retargeted:
            rig_names = add_drivers(rig, neck)
        armature = shared.build_armature(rig)
        armature.name = "GiantBodyDonorRig"
        obj, indexed, uv_indexed, parts = build_neck_body(rest, neck,
                                                          retargeted)
        obj.name = name
        mesh = obj.data

        layer = mesh.uv_layers.new(name="Albedo")
        for loop in mesh.loops:
            layer.data[loop.index].uv = uv_indexed[loop.vertex_index]
        groups = {}
        for entries in indexed.values():
            for bone, _ in entries:
                if bone not in rig_names:
                    raise SystemExit("experiment references absent bone " + bone)
                if bone not in groups:
                    groups[bone] = obj.vertex_groups.new(name=bone)
        for index, entries in indexed.items():
            total = sum(value for _, value in entries)
            for bone, value in entries:
                groups[bone].add([index], value / total, "REPLACE")
        modifier = obj.modifiers.new(name="Armature", type="ARMATURE")
        modifier.object = armature
        obj.parent = armature

        proto.clear_pose(armature)
        rest_coords = proto.evaluated_coords(obj)
        proto.place_ground({"solePlane": 0.0352})
        record = {
            "variant": name,
            "weighting": ("authored neck on seven private drivers"
                          if retargeted else
                          "authored neck on the donor Head bone alone"),
            "vertices": len(mesh.vertices),
            "boneGroups": len(groups),
            "driverBones": sorted(n for n in groups if
                                  n.startswith(DRIVER_PREFIX)),
            "leverArms": lever_arms(parts, rest_coords, rig_names, indexed),
            "restTurns": turns(neck_centreline(parts, rest_coords)),
            "poses": {},
            "renders": [],
        }

        if not framing:
            centre = sum(rest_coords, Vector()) / len(rest_coords)
            extent = max(max(p[a] for p in rest_coords) -
                         min(p[a] for p in rest_coords) for a in range(3))
            framing["centre"], framing["extent"] = centre, extent

        chain = ["Head"] + graybox.FORWARD_CHAIN
        drivers = ["Head"] + ["%s%02d" % (DRIVER_PREFIX, index)
                              for index in range(1, len(neck))]
        poses = [("bind", [])]
        for degrees in PROBE_DEGREES:
            rotations = [(bone, graybox.LATERAL, degrees) for bone in chain]
            if retargeted:
                # The map, applied: every driver takes the rotation its donor
                # counterpart took. The donor chain is posed as well, because
                # in play it is the clip that moves and the drivers follow.
                rotations += [(bone, graybox.LATERAL, degrees)
                              for bone in drivers[1:]]
            poses.append(("chain-probe-%02d" % int(degrees), rotations))
        # A bone both variants answer to, so neither can pass by ignoring it.
        poses.append(("shoulder-swing", [("Neck", graybox.LATERAL, -24.0),
                                         ("Head", graybox.LATERAL, -16.0)]))

        for pose, rotations in poses:
            proto.apply_pose(armature, rotations)
            coords = proto.evaluated_coords(obj)
            displacement = [(coords[i] - rest_coords[i]).length
                            for i in range(len(coords))]
            neck_moved = max((coords[i] - rest_coords[i]).length
                             for i in parts["neck"])
            high, low = proto.edge_extremes(mesh, rest_coords, coords,
                                            {i: "x" for i in range(len(coords))})
            record["poses"][pose] = {
                "posedBones": [bone for bone, _, _ in rotations],
                "maxVertexDisplacement": round(max(displacement), 4),
                "neckMaxDisplacement": round(neck_moved, 4),
                "maxEdgeStretch": round(high[0], 4),
                "minEdgeStretch": round(low[0], 4),
                "withinTolerance": bool(high[0] <= proto.STRETCH_CEILING and
                                        low[0] >= proto.STRETCH_FLOOR),
                "neckTurns": turns(neck_centreline(parts, coords)),
                "baseMeshSha256": proto.base_mesh_digest(mesh),
            }
            for view, offset in (proto.SIDE, proto.PARTY):
                camera.location = (framing["centre"] +
                                   offset.normalized() * framing["extent"] * 4.0)
                graybox.point_camera(camera, framing["centre"])
                camera_data.ortho_scale = framing["extent"] * 1.9
                filename = "%s-%s-%s.png" % (name, pose, view)
                scene.render.filepath = str(out / filename)
                bpy.ops.render.render(write_still=True)
                record["renders"].append(filename)
        report["variants"].append(record)

    # The acceptance condition the owner set: a probe the neck IGNORES is not
    # evidence of articulated motion.
    retarget = [v for v in report["variants"] if v["variant"] == "retargeted"][0]
    control = [v for v in report["variants"]
               if v["variant"] == "rigid-control"][0]
    probe = "chain-probe-16"
    report["acceptance"] = {
        "probeChangesTheNeck": retarget["poses"][probe]["neckMaxDisplacement"],
        "controlIgnoresTheSameProbe":
            control["poses"][probe]["neckMaxDisplacement"],
        "retargetedWithinTolerance":
            retarget["poses"][probe]["withinTolerance"],
        "neckTurnsUnderProbe": retarget["poses"][probe]["neckTurns"],
        "restTurns": retarget["restTurns"],
    }
    if retarget["poses"][probe]["neckMaxDisplacement"] < 0.25:
        report["acceptance"]["verdict"] = (
            "FAILED: the probe does not change the retargeted neck's shape, "
            "so this is not evidence of articulated motion")
    else:
        report["acceptance"]["verdict"] = (
            "the probe changes the retargeted neck's shape; see the stretch "
            "and turn figures for whether it does so coherently")

    report["runtimeBoundary"] = {
        "readFrom": "ExpandedSummoningPteranodonViewPatch attach path and "
                    "PteranodonAssetRuntime.TryResolveDonorBinding",
        "whatAlreadyFits":
            "the patch replaces donor.bones and mesh.bindposes wholesale with "
            "arrays it composes, on the donor's own renderer component, and "
            "restores OriginalBones/OriginalMesh on rollback. Extra entries in "
            "those two arrays are therefore the same kind of object the patch "
            "already writes; root bone, bounds, material, fader and controller "
            "integration are untouched by this mapping.",
        "whatDoesNotFitYet": [
            "TryResolveDonorBinding refuses any name the donor does not have "
            "(donor-bone-missing), so it cannot resolve a private driver; it "
            "would need an explicit, bounded extension that creates the "
            "drivers and returns their transforms and OUR computed bind poses",
            "the driver bind poses are ours to compute - the renderer-space "
            "inverse bind of each driver at the authored rest - and nothing "
            "in the donor supplies them",
            "the map is per-frame: a driver must take its donor counterpart's "
            "rest-relative rotation every frame, which is a small owned "
            "component in LateUpdate, not a static parenting",
            "teardown must destroy the driver GameObjects on rollback, module "
            "OFF and view destruction, alongside the existing restoration",
        ],
        "notAssumed":
            "none of the above is implemented or proven in play. This script "
            "settles the geometry of the mapping only.",
    }

    path = out / "neck-retarget-report.json"
    path.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print("NECK RETARGET REPORT: " + str(path))
    for variant in report["variants"]:
        for pose, values in variant["poses"].items():
            print("  %-14s %-18s neckDisp=%7.3f stretch=%.3f/%.3f turns=%s"
                  % (variant["variant"], pose, values["neckMaxDisplacement"],
                     values["maxEdgeStretch"], values["minEdgeStretch"],
                     values["neckTurns"]))
    print("ACCEPTANCE: " + report["acceptance"]["verdict"])


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--census", required=True)
    parser.add_argument("--out", required=True)
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    args = parser.parse_args(argv)
    out = Path(args.out)
    out.mkdir(parents=True, exist_ok=True)
    run(args.census, out)


if __name__ == "__main__":
    main()
