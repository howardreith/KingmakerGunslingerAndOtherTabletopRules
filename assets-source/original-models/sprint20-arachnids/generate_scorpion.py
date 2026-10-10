#!/usr/bin/env python3
"""The original Giant Scorpion body, authored on the measured arachnid rig.

Project-owned geometry. The only game-sourced input is the donor bind-pose
skeleton - bone names, bind positions and parent links - read from the
Sprint 20 census capture. No native vertex, index, texture pixel, shader or
animation curve is read, and none is redistributed.

The donor is the Giant Spider, which the Sprint 14 census chose and the
Sprint 20 census re-measured. Three things about it make a scorpion the
best-fitting creature this project has put on a borrowed rig:

- Eight legs for eight legs. The fourth pair is a complete leg here - upper,
  lower and foot on both sides - where the Sprint 14 ants leave it empty and
  the beetles hang wings on it. This is the first creature in the project to
  weight all four chains a side as real legs.
- A scorpion's claws ARE its pedipalps, and the rig has a seven-bone
  pedipalp chain a side that already curls forward and down. The chelae are
  anatomy here rather than substitution.
- The abdomen chain runs backward from the body, which is where a metasoma
  attaches.

METASOMA_DRIVEN_BY_A_TWO_BONE_CHAIN. That last point is also the sprint's
honest limitation, and it is measured rather than guessed. A scorpion's
metasoma is five segments and a telson; this rig's abdomen chain is Tail1_M
and Tail3_M with UpperTorso between them, three bones, all level at about
z 0.7. The tail below is authored arching up and forward over that level
chain, so the whole metasoma sways as the abdomen sways rather than
articulating segment by segment, and the sting cannot be driven as a strike
of its own. Every other tail this project has authored runs Tail0 through
Tail4; this one does not, and that was worth a guarded transaction to learn.

What this file deliberately does NOT do:

- It authors no animation. The donor walks and strikes on its own clips.
- It builds no retargeting, limb or creature framework. Every number is a
  measured offset from one named bone of one named donor.
- It touches nothing in the game. It reads one private capture and writes
  into a private output directory.
"""
import argparse
import json
from pathlib import Path
import sys

import bpy
import bmesh
from mathutils import Vector

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve()
sys.path.insert(0, str(HERE.parents[1] / "pteranodon"))
sys.path.insert(0, str(HERE.parents[1] / "sprint14-insects"))
sys.path.insert(0, str(HERE.parent))
import generate_pteranodon as shared
import generate_sprint14_insects as insects
import arachnid_capture as capture

KIND = "giant-scorpion"
SPACE = capture.SPACE
# The same exporter, the same bind frame and the same triangle order the
# Sprint 14 insects on this rig ship with, so the runtime handles this body
# exactly as it handles its siblings.
WINDING = "shared-exporter-sprint18"

inset_region_uvs = insects.inset_region_uvs
tube = insects.tube
ellipsoid = insects.ellipsoid

# The donor frame, measured: +X right, +Z up, -Y forward.
LATERAL = Vector((1.0, 0.0, 0.0))
UP = Vector((0.0, 0.0, 1.0))
FORWARD = Vector((0.0, -1.0, 0.0))

# The shared atlas has five regions. A scorpion uses four of them: the
# carapace and pre-abdomen on body, the arched tail on crest so it can be
# painted as banded plate, the mouthparts and sting tip on beak, and the
# legs and chelae on limbs.
BODY_REGION, TAIL_REGION = "body", "crest"
POINT_REGION, LIMB_REGION = "beak", "limbs"


def prosoma(bm, weights, uvs, rig):
    """The carapace: one broad low shield over the leg roots.

    Wider than it is long and flattened on top, which is what separates a
    scorpion's prosoma from the spider's bulbous cephalothorax at a glance.
    """
    lower = capture.head(rig, "LowerTorso")
    points = [lower + Vector((0.0, -0.46, -0.02)),
              lower + Vector((0.0, -0.28, 0.01)),
              lower + Vector((0.0, -0.06, 0.02)),
              lower + Vector((0.0, 0.16, 0.02)),
              lower + Vector((0.0, 0.30, 0.00))]
    radii = [0.19, 0.32, 0.40, 0.37, 0.29]
    bones = ["LowerTorso"] * len(points)
    tube(bm, weights, uvs, points, radii, bones, BODY_REGION, segments=14)
    # The median eyes, small and close together on the carapace ridge.
    for side in (-1, 1):
        ellipsoid(bm, weights, uvs,
                  lower + Vector((side * 0.055, -0.20, 0.10)),
                  LATERAL, UP, FORWARD, (0.030, 0.026, 0.028), "LowerTorso",
                  region=POINT_REGION, latitudes=5, segments=7)


def mesosoma(bm, weights, uvs, rig):
    """The segmented pre-abdomen, from the carapace back to the tail root."""
    lower = capture.head(rig, "LowerTorso")
    tail1 = capture.head(rig, "Tail1_M")
    upper = capture.head(rig, "UpperTorso")
    tail3 = capture.head(rig, "Tail3_M")
    # It has to reach Tail3_M. The first build stopped at UpperTorso and the
    # metasoma, which starts at Tail3_M, floated clear of the animal.
    points = [lower + Vector((0.0, 0.24, 0.00)),
              tail1 + Vector((0.0, -0.02, -0.01)),
              tail1 + Vector((0.0, 0.18, -0.01)),
              upper + Vector((0.0, 0.02, 0.00)),
              upper.lerp(tail3, 0.55),
              tail3 + Vector((0.0, -0.02, 0.01))]
    radii = [0.29, 0.275, 0.255, 0.215, 0.175, 0.140]
    bones = ["LowerTorso", "Tail1_M", "Tail1_M", "UpperTorso", "UpperTorso",
             "Tail3_M"]
    tube(bm, weights, uvs, points, radii, bones, BODY_REGION, segments=12)


def metasoma(bm, weights, uvs, rig):
    """The tail: five segments arching up and forward, then the telson.

    Authored over a level chain. The arc is geometry; the chain underneath
    it is Tail1_M, UpperTorso and Tail3_M, which all sit at about z 0.7 and
    run backward. Weighting walks along that chain as the arc rises, so the
    tail bends with the abdomen rather than hinging at one point - which is
    as much articulation as this rig can give it.
    """
    tail3 = capture.head(rig, "Tail3_M")
    base = tail3 + Vector((0.0, -0.04, 0.02))
    arc = ((0.00, 0.10, 0.16), (0.00, 0.17, 0.42), (0.00, 0.14, 0.70),
           (0.00, -0.02, 0.94), (0.00, -0.28, 1.09), (0.00, -0.58, 1.14))
    points = [base + Vector(offset) for offset in arc]
    radii = [0.115, 0.108, 0.100, 0.092, 0.084, 0.076]
    # The near half leans on UpperTorso and the far half on Tail3_M, so the
    # metasoma is carried by the whole abdomen chain and not by its tip.
    bones = ["UpperTorso", "UpperTorso", "Tail3_M", "Tail3_M", "Tail3_M",
             "Tail3_M"]
    tube(bm, weights, uvs, points, radii, bones, TAIL_REGION, segments=10)

    # The telson: a bulb under the last segment, then the sting curving down
    # and forward out of it. The sting is this creature's printed poison
    # attack, so a player has to be able to see it.
    telson = base + Vector((0.0, -0.74, 1.10))
    ellipsoid(bm, weights, uvs, telson, LATERAL, UP, FORWARD,
              (0.085, 0.090, 0.105), "Tail3_M", region=TAIL_REGION,
              latitudes=7, segments=10)
    sting = [telson + Vector((0.0, -0.06, -0.02)),
             telson + Vector((0.0, -0.17, -0.11)),
             telson + Vector((0.0, -0.27, -0.21)),
             telson + Vector((0.0, -0.33, -0.28))]
    tube(bm, weights, uvs, sting, [0.050, 0.033, 0.014, 0.004],
         ["Tail3_M"] * 4, POINT_REGION, segments=8)


def chela(bm, weights, uvs, rig, side):
    """One pedipalp, built along the seven bones the rig already has.

    Upper arm, forearm, then a boxy hand with two fingers held apart. A shut
    claw is a lump on the end of an arm; an open one reads as a claw, and
    this creature's printed routine is two claws seizing one target.
    """
    names = ["pedipalp%d_%s" % (index, side) for index in range(1, 8)]
    points = [capture.head(rig, name) for name in names]
    # The chain is about a fifth as long as a leg, and a scorpion's chelae
    # are its largest limbs. The first build followed the bones exactly and
    # the claws rendered as two nubs beside the mouth. Geometry does not
    # have to stay inside the bones that drive it - the Sprint 14 beetle
    # wings make the same point - so the arm is carried forward past the
    # chain's end along its own direction and the hand is built there.
    reach = points[3] - points[0]
    if reach.length < 1e-5:
        reach = Vector((0.0, -1.0, 0.0))
    direction = reach.normalized()
    outward = Vector((1.0 if side == "R" else -1.0, 0.0, 0.0))
    # Held forward and a little up, which is how a scorpion carries them and
    # what keeps them off the ground line where eight legs already are. The
    # second build left them level and they read as two balls among the legs.
    elbow = points[3] + direction * 0.18 + outward * 0.12 + UP * 0.06
    wrist = elbow + direction * 0.34 + outward * 0.02 + UP * 0.10
    arm_points = [points[0], points[1], points[2], points[3], elbow, wrist]
    arm_radii = [0.105, 0.098, 0.092, 0.088, 0.084, 0.080]
    arm_bones = [names[0], names[1], names[2], names[3], names[4], names[5]]
    tube(bm, weights, uvs, arm_points, arm_radii, arm_bones, LIMB_REGION,
         segments=10)

    # The chela itself: long and flattened rather than round. A sphere on
    # the end of an arm is a knob; a chela is a hand, deeper than it is wide
    # and longer than it is deep.
    hand_centre = wrist + direction * 0.20 + UP * 0.04
    ellipsoid(bm, weights, uvs, hand_centre, LATERAL, UP, FORWARD,
              (0.090, 0.135, 0.230), names[6], region=LIMB_REGION,
              latitudes=9, segments=12)

    # Two fingers with a gap. Direction follows the chain's own last step,
    # so the claw points where the pedipalp points rather than where a
    # constant would send it.
    inward = Vector((-1.0 if side == "R" else 1.0, 0.0, 0.0))
    # Two fingers, each a real digit rather than a pin: as thick at the base
    # as the arm is, and long enough that the gap between them is the shape
    # a player reads. The fixed finger runs on from the hand and the movable
    # one closes toward it.
    for index, lean in enumerate((0.26, -0.20)):
        root = hand_centre + direction * 0.19 + inward * (lean * 0.11)
        aim = (direction + inward * lean * 0.55 +
               UP * (0.18 if index == 0 else -0.20)).normalized()
        finger = [root, root + aim * 0.130, root + aim * 0.245,
                  root + aim * 0.330]
        tube(bm, weights, uvs, finger, [0.070, 0.052, 0.028, 0.006],
             [names[6]] * 4, LIMB_REGION, segments=8)


def legs(bm, weights, uvs, rig):
    """Eight legs, four a side, every chain a real leg.

    The fourth pair is what makes this creature fit the rig: the ants leave
    that chain empty and the beetles hang wings on it, and a scorpion simply
    walks on it.
    """
    for side in ("L", "R"):
        for index in range(4):
            upper_name = "%s_Leg%d_Upper" % (side, index)
            lower_name = "%s_Leg%d_Lower" % (side, index)
            foot_name = "%s_Foot%d" % (side, index)
            upper = capture.head(rig, upper_name)
            lower = capture.head(rig, lower_name)
            foot = capture.head(rig, foot_name)
            toe = foot + (foot - lower).normalized() * 0.11
            points = [upper, upper.lerp(lower, 0.55), lower,
                      lower.lerp(foot, 0.55), foot, toe]
            radii = [0.062, 0.052, 0.046, 0.034, 0.022, 0.006]
            bones = [upper_name, upper_name, lower_name, lower_name,
                     foot_name, foot_name]
            tube(bm, weights, uvs, points, radii, bones, LIMB_REGION,
                 segments=8)


def chelicerae(bm, weights, uvs, rig):
    """The small mouthparts, which a scorpion has and which the rig names."""
    for side in ("L", "R"):
        name = "chelicera_%s" % side
        root = capture.head(rig, name)
        points = [root, root + Vector((0.0, -0.055, -0.010)),
                  root + Vector((0.0, -0.095, -0.020))]
        tube(bm, weights, uvs, points, [0.030, 0.021, 0.006], [name] * 3,
             POINT_REGION, segments=7)


def build_body(bm, weights, uvs, rig):
    prosoma(bm, weights, uvs, rig)
    mesosoma(bm, weights, uvs, rig)
    metasoma(bm, weights, uvs, rig)
    chelicerae(bm, weights, uvs, rig)
    for side in ("L", "R"):
        chela(bm, weights, uvs, rig, side)
    legs(bm, weights, uvs, rig)


def chain_usage(used):
    """Which leg chains carry geometry. All eight, which is the point."""
    usage = {}
    for index in range(4):
        parts = {"%s_Leg%d_%s" % (side, index, part)
                 for side in ("L", "R") for part in ("Upper", "Lower")}
        feet = {"%s_Foot%d" % (side, index) for side in ("L", "R")}
        if parts <= used and feet <= used:
            usage[str(index)] = "full"
        elif (parts | feet) & used:
            usage[str(index)] = "partial"
        else:
            usage[str(index)] = "empty"
    return usage


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out",
                 "fbx-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:]
                             if "--" in sys.argv else [])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = capture.load(args.capture)
    armature = shared.build_armature(rig_data)
    armature.name = "GiantScorpionRigPreview"
    mesh = bpy.data.meshes.new("GiantScorpionMesh")
    obj = bpy.data.objects.new(KIND, mesh)
    bpy.context.collection.objects.link(obj)
    bm = bmesh.new()
    weights, uvs = {}, {}
    build_body(bm, weights, uvs, rig)
    inset_region_uvs(uvs)
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.normal_update()
    bm.to_mesh(mesh)
    indexed = {vertex.index: entries for vertex, entries in weights.items()}
    uv_indexed = {vertex.index: uv for vertex, uv in uvs.items()}
    bm.free()
    if (len(indexed) != len(mesh.vertices) or
            len(uv_indexed) != len(mesh.vertices)):
        raise SystemExit("original mesh has an unweighted or unpainted vertex")

    layer = mesh.uv_layers.new(name="Albedo")
    for loop in mesh.loops:
        layer.data[loop.index].uv = uv_indexed[loop.vertex_index]
    groups = {}
    for entries in indexed.values():
        for name, _ in entries:
            if name not in rig:
                raise SystemExit("mesh references absent donor bone " + name)
            if name not in capture.DRIVERS:
                raise SystemExit("unreviewed donor bone carries geometry: " + name)
            if name not in groups:
                groups[name] = obj.vertex_groups.new(name=name)
    for index, entries in indexed.items():
        total = sum(value for _, value in entries)
        if total <= 0 or len(entries) > 4:
            raise SystemExit("invalid influence row at vertex " + str(index))
        for name, value in entries:
            groups[name].add([index], value / total, "REPLACE")

    usage = chain_usage(set(groups))
    if any(value != "full" for value in usage.values()):
        raise SystemExit("a scorpion walks on all eight legs: " +
                         json.dumps(usage, sort_keys=True))
    # The limitation, proved from the built geometry rather than asserted in
    # a manifest field: the tail is carried by the abdomen chain and nothing
    # else, and that chain is three bones.
    tail_drivers = {name for index, entries in indexed.items()
                    for name, _ in entries} & set(capture.ABDOMEN_CHAIN)
    if not {"Tail3_M", "UpperTorso"} <= tail_drivers:
        raise SystemExit("the metasoma must ride the abdomen chain")

    modifier = obj.modifiers.new(name="Armature", type="ARMATURE")
    modifier.object = armature
    obj.parent = armature
    shared.attach_preview_material(obj, mesh, args.albedo)
    xs, ys, zs = ([getattr(vertex.co, axis) for vertex in mesh.vertices]
                  for axis in ("x", "y", "z"))
    report = {"schemaVersion": 3, "creature": KIND,
              "donorFamily": "giant-spider", "captureSprint": "Sprint 20",
              "rigSha256": rig_hash, "rigBoneCount": len(rig_data["bones"]),
              "vertices": len(mesh.vertices), "polygons": len(mesh.polygons),
              "boneGroups": len(groups),
              "maxInfluencesPerVertex": max(map(len, indexed.values())),
              "legChainUsage": usage,
              "visibleLegs": 2 * sum(1 for value in usage.values()
                                     if value == "full"),
              "abdomenChainDrivers": sorted(tail_drivers),
              "extent": {"x": [min(xs), max(xs)], "y": [min(ys), max(ys)],
                         "z": [min(zs), max(zs)]},
              "uvAtlas": {name: list(region)
                          for name, region in shared.ATLAS.items()},
              "albedo": shared.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    shared.write_mesh_data(args.mesh_data, obj, mesh, indexed, uv_indexed,
                           report, args.albedo)
    payload = json.loads(Path(args.mesh_data).read_text(encoding="utf-8"))
    payload["space"] = SPACE
    payload["triangleWinding"] = WINDING
    payload["creature"] = KIND
    payload["donorBlueprint"] = capture.BLUEPRINT
    payload["donorRenderer"] = capture.RENDERER
    payload["legChainUsage"] = usage
    payload["visibleLegs"] = report["visibleLegs"]
    payload["visibleClaws"] = 2
    payload["metasomaSegments"] = 5
    payload["abdomenChainDrivers"] = sorted(tail_drivers)
    payload["printedSize"] = "Large"
    payload["authoredLimitation"] = "METASOMA_DRIVEN_BY_A_TWO_BONE_CHAIN"
    with Path(args.mesh_data).open("w", encoding="utf-8", newline="\n") as out:
        out.write(json.dumps(payload, indent=1) + "\n")

    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    armature.select_set(True)
    bpy.ops.export_scene.fbx(filepath=args.fbx_out, use_selection=True,
                             add_leaf_bones=False)
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("built", KIND, "vertices", len(mesh.vertices),
          "polygons", len(mesh.polygons), "legs", report["visibleLegs"])


if __name__ == "__main__":
    main()
