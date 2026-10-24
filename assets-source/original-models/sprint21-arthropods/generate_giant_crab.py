#!/usr/bin/env python3
"""The original Giant Crab body, authored on the measured arachnid rig.

Project-owned geometry. The only game-sourced input is the donor bind-pose
skeleton - bone names, bind positions and parent links - read from the
Sprint 21 two-family census capture. No native vertex, index, texture pixel,
shader or animation curve is read, and none is redistributed.

The census audited this creature's donor independently of the Bebelith's and
reached the same rig by a different route. What a crab needs is eight real
leg chains and two forward articulated chains; the centipedes the search
found report four leg pairs of nothing, because that family names its chains
differently, and the arachnid skeleton is the only thing in the installation
that has them. That the other family landed there too is a coincidence of
what the game ships, not a shared decision.

What makes this a crab rather than another arthropod on the same bones:

- A BROAD SHELL, half again as wide as it is long. The carapace is the
  silhouette, and every other creature on this rig is longer than it is
  wide. This one is authored across the body rather than along it.
- A LATERAL STANCE. The eight legs leave the flanks sideways and bend down
  outside the shell's own outline rather than fanning front to back. That is
  how a crab stands and it is also the reason the creature carries a printed
  +12 against trip.
- TWO LARGE PINCERS, unequal, held forward and low. They ride the pedipalp
  chains - the same chains the scorpion's chelae and the Bebelith's foreclaws
  use, which is anatomy in all three cases - and they are the printed
  creature's only attack.
- NO ABDOMEN AND NO STING. The rig has an abdomen chain and this body must
  not touch it. A crab with a spider's abdomen is the single most likely way
  this body goes wrong, so the generator refuses to build one rather than
  relying on nobody adding it.

CRAB_WALKS_A_SPIDER_RIG_FRONT_TO_BACK. The honest limitation, measured: the
donor's four leg pairs are positioned fore and aft along the body, because
they belong to a creature that walks forwards. The geometry pushes each leg
out sideways from its own chain, so the stance reads laterally at rest and
in a turn - but the walk cycle is the donor's, and a crab that scuttles
sideways is not something a borrowed clip can be made to do.

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
import sprint21_capture as capture

KIND = "giant-crab"
FAMILY = "giant-crab"
SPACE = capture.SPACE
WINDING = "shared-exporter-sprint18"

inset_region_uvs = insects.inset_region_uvs
tube = insects.tube
ellipsoid = insects.ellipsoid

LATERAL = Vector((1.0, 0.0, 0.0))
UP = Vector((0.0, 0.0, 1.0))
FORWARD = Vector((0.0, -1.0, 0.0))

# The shared atlas has five regions. A crab uses four: the carapace on body,
# the two pincers on crest so the hard working edges can be painted apart
# from the shell, the eyestalks and finger tips on beak, and the eight legs
# on limbs.
BODY_REGION, CLAW_REGION = "body", "crest"
POINT_REGION, LIMB_REGION = "beak", "limbs"

# Medium, like the donor. The one scale factor here is lateral: the shell is
# pushed out across the body because that is the proportion that identifies
# the animal and the donor's own is the opposite.
WIDE = 1.55


def carapace(bm, weights, uvs, rig):
    """The shell: broad, low, and wider than it is long.

    Built across the body rather than along it, which is the one proportion
    that has to survive. Every other creature on this rig - the spider, the
    scorpion, the ants, the beetles, the Bebelith - runs longer than wide,
    so this is also what separates the crab from its menu neighbours at a
    glance.
    """
    lower = capture.head(rig, "LowerTorso")
    upper = capture.head(rig, "UpperTorso")
    centre = lower.lerp(upper, 0.30)
    # ONE domed mass, with one front section overlapping it heavily enough
    # to read as the same shell.
    #
    # The first build used four separate ellipsoids - a main mass, a rim, a
    # back and six anterolateral teeth - on the assumption they would read
    # as one carapace. They do not: a separate ellipsoid is a separate
    # closed surface, and the teeth rendered as six pebbles lying beside the
    # animal. It was also far too flat; up-radius 0.19 against a lateral
    # 0.81 is a plate, and a crab's shell is domed.
    ellipsoid(bm, weights, uvs, centre + Vector((0.0, 0.02, 0.05)),
              LATERAL, UP, FORWARD,
              (0.53 * WIDE, 0.30, 0.47), "LowerTorso",
              region=BODY_REGION, latitudes=13, segments=20)
    # The front, lower and narrower and set well inside the dome so the two
    # overlap rather than stack. This is the edge the key light catches and
    # the line that separates shell from pincers.
    ellipsoid(bm, weights, uvs, centre + Vector((0.0, -0.26, -0.01)),
              LATERAL, UP, FORWARD,
              (0.44 * WIDE, 0.21, 0.26), "LowerTorso",
              region=BODY_REGION, latitudes=9, segments=18)
    # Two eyestalks, short and vertical ON THE FRONT EDGE. The first build
    # placed them from the body centre and they stood on top of the shell
    # like two chimneys.
    # On the dome's own front SURFACE. The forward radius is 0.47, so a
    # stalk rooted 0.40 ahead of centre is still inside the shell - which is
    # why the second build's eyes rendered as two dots poking through the
    # top of it.
    front = centre + Vector((0.0, -0.49, 0.06))
    for side in (-1, 1):
        root = front + Vector((side * 0.15, 0.0, 0.06))
        tip = root + Vector((side * 0.015, -0.02, 0.10))
        tube(bm, weights, uvs, [root, root.lerp(tip, 0.6), tip],
             [0.032, 0.028, 0.026], ["LowerTorso"] * 3, POINT_REGION,
             segments=7)
        ellipsoid(bm, weights, uvs, tip + Vector((0.0, -0.01, 0.020)),
                  LATERAL, UP, FORWARD, (0.036, 0.034, 0.038), "LowerTorso",
                  region=POINT_REGION, latitudes=6, segments=8)


def pincer(bm, weights, uvs, rig, side, larger):
    """One pincer, along the seven pedipalp bones.

    Held forward and low, where the shell does not hide it. The two are
    deliberately unequal - a living crab's are, and two matched claws held
    symmetrically read as a machine - and both open, because a shut claw is
    a lump on the end of an arm and this creature's whole printed routine is
    two claws seizing.
    """
    names = ["pedipalp%d_%s" % (index, side) for index in range(1, 8)]
    points = [capture.head(rig, name) for name in names]
    reach = points[3] - points[0]
    if reach.length < 1e-5:
        reach = Vector((0.0, -1.0, 0.0))
    direction = reach.normalized()
    outward = Vector((1.0 if side == "R" else -1.0, 0.0, 0.0))
    scale = 1.26 if larger else 1.0
    lift = 0.07 if larger else 0.0

    # Brought in. The first build reached far enough that the larger
    # pincer left the shell behind and read as a separate animal.
    elbow = points[3] + direction * 0.11 + outward * 0.10 + UP * (0.02 + lift)
    wrist = elbow + direction * 0.20 + outward * 0.03 + UP * (0.01 + lift)
    arm_points = [points[0], points[1], points[2], points[3], elbow, wrist]
    # Heavier. A pincer arm as thin as a walking leg reads as a ninth leg
    # with a lump on it.
    arm_radii = [r * scale for r in (0.135, 0.126, 0.120, 0.114, 0.108, 0.100)]
    arm_bones = [names[0], names[1], names[2], names[3], names[4], names[5]]
    tube(bm, weights, uvs, arm_points, arm_radii, arm_bones, CLAW_REGION,
         segments=10)

    # The hand: flattened and wider across the reach than along it, which is
    # a pincer. The Sprint 20 chela is deeper than it is wide because a
    # scorpion's is a blade; a crab's is a nutcracker.
    palm = wrist + direction * 0.11
    ellipsoid(bm, weights, uvs, palm, LATERAL, UP, FORWARD,
              (0.150 * scale, 0.112 * scale, 0.190 * scale), names[6],
              region=CLAW_REGION, latitudes=9, segments=12)
    # Two fingers held apart in height, both emerging from the palm's front
    # face. The gap is the shape a player reads at party camera.
    inward = Vector((-1.0 if side == "R" else 1.0, 0.0, 0.0))
    base = palm + direction * (0.08 * scale)
    for index, rise in enumerate((0.062, -0.058)):
        root = base + UP * (rise * scale) + inward * (0.010 * scale)
        aim = (direction + inward * 0.12 + UP * (0.26 if index == 0 else -0.22)
               ).normalized()
        finger = [root, root + aim * (0.085 * scale),
                  root + aim * (0.155 * scale), root + aim * (0.210 * scale)]
        tube(bm, weights, uvs, finger,
             [r * scale for r in (0.050, 0.038, 0.021, 0.005)],
             [names[6]] * 4, POINT_REGION, segments=8)


def legs(bm, weights, uvs, rig):
    """Eight walking legs, four a side, pushed out sideways.

    The donor's chains are positioned fore and aft because they belong to a
    creature that walks forwards. Each leg is carried out along +-X from its
    own chain before it bends down, so the stance reads laterally - which is
    the printed one, and the reason this creature carries a +12 against
    trip. The walk cycle underneath is still the donor's, and that is the
    recorded limitation.
    """
    for side in ("L", "R"):
        sign = -1.0 if side == "L" else 1.0
        for index in range(4):
            upper_name = "%s_Leg%d_Upper" % (side, index)
            lower_name = "%s_Leg%d_Lower" % (side, index)
            foot_name = "%s_Foot%d" % (side, index)
            upper = capture.head(rig, upper_name)
            lower = capture.head(rig, lower_name)
            foot = capture.head(rig, foot_name)
            # Out before down. The push grows along the body so the four
            # legs a side splay rather than running parallel, which is what
            # stopped the icon's first pass reading as a ladder.
            # Out and UP at the knee, then down to the foot. The donor's
            # upper, lower and foot bones are nearly collinear in bind pose -
            # a spider's are - so following them gave eight straight spikes
            # and no joint anywhere.
            # Out, and barely up. The donor is a squat spider: its feet sit
            # about a tenth below its torso, so there is no vertical room for
            # a raised knee and the second build's 0.13 lifted every leg
            # above the shell.
            push = sign * (0.14 + 0.045 * index)
            knee = lower + Vector((push, 0.0, 0.02))
            ankle = foot + Vector((push * 1.3, 0.0, 0.0))
            stride = ankle - knee
            if stride.length < 1e-5:
                stride = Vector((push, 0.0, -1.0))
            toe = ankle + stride.normalized() * 0.08
            points = [upper, upper.lerp(knee, 0.55), knee,
                      knee.lerp(ankle, 0.55), ankle, toe]
            radii = [0.066, 0.058, 0.050, 0.036, 0.022, 0.006]
            bones = [upper_name, upper_name, lower_name, lower_name,
                     foot_name, foot_name]
            tube(bm, weights, uvs, points, radii, bones, LIMB_REGION,
                 segments=8)


def build_body(bm, weights, uvs, rig):
    carapace(bm, weights, uvs, rig)
    # The left pincer is the larger, as a living crab's usually is.
    for side in ("L", "R"):
        pincer(bm, weights, uvs, rig, side, larger=(side == "L"))
    legs(bm, weights, uvs, rig)


def chain_usage(used):
    """Which leg chains carry geometry. All four, which is the point."""
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
    raise SystemExit("Giant Crab is built by build_giant_crab.py since Cleanup 2; "
                     "see SOURCE.md. The legacy geometry is retained for history.")
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out",
                 "fbx-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:]
                             if "--" in sys.argv else [])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = capture.load(args.capture, FAMILY)
    armature = shared.build_armature(rig_data)
    armature.name = "GiantCrabRigPreview"
    mesh = bpy.data.meshes.new("GiantCrabMesh")
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
        raise SystemExit("a crab walks on all eight legs: " +
                         json.dumps(usage, sort_keys=True))
    drivers = set(groups)
    if not set(capture.PEDIPALPS) <= drivers:
        raise SystemExit("both pincers must ride their whole pedipalp chain")
    # No abdomen and no sting. Checked from the built geometry rather than
    # trusted: Tail1_M carries the back of the shell and nothing else on
    # that chain may be reached at all.
    if "Tail3_M" in drivers:
        raise SystemExit("a crab has no abdomen and no sting; Tail3_M is the "
                         "chain a spider's abdomen and a scorpion's metasoma "
                         "hang on and this body may not touch it")
    if not set(capture.CHELICERAE).isdisjoint(drivers):
        raise SystemExit("a crab's mouthparts are under its shell; the "
                         "chelicerae belong to the creatures that bite")
    # The proportion that identifies the animal, proved from the mesh.
    xs = [vertex.co.x for vertex in mesh.vertices]
    ys = [vertex.co.y for vertex in mesh.vertices]
    width, length = max(xs) - min(xs), max(ys) - min(ys)
    if width <= length:
        raise SystemExit("a crab is wider than it is long: width %.3f "
                         "length %.3f" % (width, length))

    modifier = obj.modifiers.new(name="Armature", type="ARMATURE")
    modifier.object = armature
    obj.parent = armature
    shared.attach_preview_material(obj, mesh, args.albedo)
    zs = [vertex.co.z for vertex in mesh.vertices]
    report = {"schemaVersion": 3, "creature": KIND,
              "donorFamily": "giant-spider", "captureSprint": "Sprint 21",
              "rigSha256": rig_hash, "rigBoneCount": len(rig_data["bones"]),
              "vertices": len(mesh.vertices), "polygons": len(mesh.polygons),
              "boneGroups": len(groups),
              "maxInfluencesPerVertex": max(map(len, indexed.values())),
              "legChainUsage": usage,
              "visibleLegs": 2 * sum(1 for value in usage.values()
                                     if value == "full"),
              "widthOverLength": round(width / length, 4),
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
    payload["donorBlueprint"] = capture.GIANT_SPIDER
    payload["donorRenderer"] = capture.RENDERER
    payload["legChainUsage"] = usage
    payload["visibleLegs"] = report["visibleLegs"]
    payload["visiblePincers"] = 2
    payload["widthOverLength"] = report["widthOverLength"]
    payload["printedSize"] = "Medium"
    payload["authoredLimitation"] = "CRAB_WALKS_A_SPIDER_RIG_FRONT_TO_BACK"
    with Path(args.mesh_data).open("w", encoding="utf-8", newline="\n") as out:
        out.write(json.dumps(payload, indent=1) + "\n")

    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    armature.select_set(True)
    bpy.ops.export_scene.fbx(filepath=args.fbx_out, use_selection=True,
                             add_leaf_bones=False)
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("built", KIND, "vertices", len(mesh.vertices),
          "polygons", len(mesh.polygons), "legs", report["visibleLegs"],
          "width/length", report["widthOverLength"])


if __name__ == "__main__":
    main()
