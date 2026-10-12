#!/usr/bin/env python3
"""The original Bebelith body, authored on the measured arachnid rig.

Project-owned geometry. The only game-sourced input is the donor bind-pose
skeleton - bone names, bind positions and parent links - read from the
Sprint 21 two-family census capture. No native vertex, index, texture pixel,
shader or animation curve is read, and none is redistributed.

This is the first body in the series authored for a creature that already
ships wearing something. The Bebelith is released as a Doomspider, and the
census measured the Doomspider and the Giant Spider to be the same 51-bone
skeleton, bone for bone. So the rig is not in question and the whole
authoring problem is the opposite of the usual one: not "what can this rig
carry" but "what must this body do so it is not a repaint of what the rig
already wears".

Four things, each measured rather than chosen:

- It walks on SIX. The donor has four complete leg pairs and the Doomspider
  uses all eight; a bebelith's printed anatomy is six hind walking legs and
  two elevated foreclaws. The fourth leg chain therefore carries no walking
  geometry at all - the same shape the Sprint 14 ants use for a chain they
  do not walk on - and this is the single largest reason the silhouette
  cannot be mistaken for the creature it replaces.
- It carries two BARBED FORECLAWS, elevated. They ride the seven-bone
  pedipalp chains, which already reach forward, raised well above the
  walking line and carrying a row of spines a spider's pedipalps do not
  have. These are the limbs the printed Dismantle Armor tears with.
- It has a real JAW. The rig names chelicera_L and chelicera_R, which the
  first census run reported as absent because the candidate list spelled
  them with a capital C. They are built here as heavy mandibles rather than
  the small mouthparts a spider gets, because the printed bite is a primary
  attack with its own 2d6 die and it carries rot.
- It is HUGE, and the abdomen SPINS. The printed space and reach are fifteen
  feet. Size is not geometry on this rig - the view scale carries it, which
  scales skeleton and mesh together - and the abdomen ends in spinnerets
  because the printed web comes out of it.

BEBELITH_ABDOMEN_RIDES_A_TWO_BONE_CHAIN. The honest limitation, measured and
the same one Sprint 20 recorded: the abdomen chain is Tail1_M and Tail3_M
with UpperTorso between them, three bones, all level at about z 0.7. A
bebelith's abdomen is one mass rather than a scorpion's articulated tail, so
this costs less here than it did there - but the spinnerets cannot aim, and
the web is cast from a body that sways rather than from a turret.

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

KIND = "bebelith"
FAMILY = "bebelith"
SPACE = capture.SPACE
WINDING = "shared-exporter-sprint18"

inset_region_uvs = insects.inset_region_uvs
tube = insects.tube
ellipsoid = insects.ellipsoid

LATERAL = Vector((1.0, 0.0, 0.0))
UP = Vector((0.0, 0.0, 1.0))
FORWARD = Vector((0.0, -1.0, 0.0))

# The shared atlas has five regions. A bebelith uses four: the cephalothorax
# and abdomen on body, the barbed foreclaws on crest so the spines can be
# painted as a row of hard points, the mandibles and claw tips and spinnerets
# on beak, and the six walking legs on limbs.
BODY_REGION, CLAW_REGION = "body", "crest"
POINT_REGION, LIMB_REGION = "beak", "limbs"

# Size is NOT geometry here. The first build multiplied every offset by
# 1.62 to make a Huge creature and the donor's bone positions did not
# scale with it: the body inflated around an unchanged skeleton, the legs
# stayed where the bones put them, and the render was a lump on the ground
# with spikes coming out of it. On this rig size is the view scale's job -
# it scales skeleton and geometry together, and it is how every other
# borrowed body in this series is sized. So the geometry is authored at
# donor scale and SummonViewScaleCatalog carries the printed footprint.

# The walking chains. Three pairs, not four: this is the decision that makes
# the body a bebelith rather than a repaint of the spider it replaces.
WALKING_CHAINS = (0, 1, 2)
ELEVATED_CHAIN = 3


def cephalothorax(bm, weights, uvs, rig):
    """The forebody: deeper and more angular than a spider's bulb.

    A bebelith hunts other demons, and what the silhouette has to say first
    is that the front of it is armour with a mouth in it. So the mass runs
    low and long rather than round, and it rises to a hard ridge over the
    leg roots instead of doming.
    """
    lower = capture.head(rig, "LowerTorso")
    points = [lower + Vector((0.0, -0.62, 0.02)),
              lower + Vector((0.0, -0.40, 0.08)),
              lower + Vector((0.0, -0.14, 0.11)),
              lower + Vector((0.0, 0.12, 0.09)),
              lower + Vector((0.0, 0.32, 0.04))]
    radii = [r for r in (0.20, 0.36, 0.46, 0.43, 0.33)]
    tube(bm, weights, uvs, points, radii, ["LowerTorso"] * len(points),
         BODY_REGION, segments=14)
    # Eight eyes in two rows, which is what a demon built on a spider's plan
    # keeps. Small: at party camera they are a texture detail, and the
    # painting carries them.
    for side in (-1, 1):
        for row, (back, lift, size) in enumerate((
                (-0.30, 0.17, 0.034), (-0.26, 0.13, 0.028))):
            for offset in (0.05, 0.11):
                ellipsoid(bm, weights, uvs,
                          lower + Vector((side * offset, back, lift)),
                          LATERAL, UP, FORWARD,
                          tuple(v for v in (size, size * 0.88, size)),
                          "LowerTorso", region=POINT_REGION,
                          latitudes=5, segments=7)


def abdomen(bm, weights, uvs, rig):
    """The web-spinning abdomen: one heavy mass, not a segmented tail.

    This is where a bebelith differs most from the scorpion that shares the
    rig. A metasoma had to arch off this chain and articulate; an abdomen
    simply hangs on it, so the two-bone chain costs almost nothing here and
    the mass can be as large as the printed creature's.
    """
    lower = capture.head(rig, "LowerTorso")
    tail1 = capture.head(rig, "Tail1_M")
    upper = capture.head(rig, "UpperTorso")
    tail3 = capture.head(rig, "Tail3_M")
    waist = lower.lerp(tail1, 0.6)
    # Forward and up. The first build hung it behind the last leg pair with
    # nothing under it and the whole animal sat down; a spider-plan abdomen
    # rides over the hips rather than trailing past them.
    points = [lower + Vector((0.0, 0.22, 0.07)),
              waist + Vector((0.0, 0.00, 0.09)),
              tail1 + Vector((0.0, 0.00, 0.13)),
              upper + Vector((0.0, -0.04, 0.14)),
              upper.lerp(tail3, 0.5) + Vector((0.0, -0.02, 0.12)),
              tail3 + Vector((0.0, -0.06, 0.07))]
    # Narrow at the waist and then swelling, which is what a spider-plan
    # abdomen does and a scorpion's pre-abdomen does not.
    radii = [r for r in (0.24, 0.17, 0.31, 0.36, 0.32, 0.18)]
    bones = ["LowerTorso", "Tail1_M", "Tail1_M", "UpperTorso", "Tail3_M",
             "Tail3_M"]
    tube(bm, weights, uvs, points, radii, bones, BODY_REGION, segments=14)
    # The spinnerets. Two short pairs at the rear, which is the only part of
    # the body that says where the printed web comes from.
    #
    # Anchored to the abdomen's last built POINT, not to the chain's last
    # bone. The first build used the bone, the abdomen geometry had already
    # moved forward and up off it, and the two pairs rendered floating in
    # the air behind the animal.
    rear = points[-1] + Vector((0.0, 0.06, -0.02))
    for side in (-1, 1):
        for index, (out, back, drop) in enumerate((
                (0.07, 0.07, 0.00), (0.04, 0.10, -0.05))):
            root = rear + Vector((side * out, back, drop))
            tip = root + Vector((side * 0.02, 0.09, -0.03))
            tube(bm, weights, uvs, [root, root.lerp(tip, 0.55), tip],
                 [0.038, 0.026, 0.008],
                 ["Tail3_M"] * 3, POINT_REGION, segments=7)


def mandibles(bm, weights, uvs, rig):
    """The jaw, which the printed bite needs and a spider's does not have.

    The rig names chelicera_L and chelicera_R. The first census run reported
    no jaw on this skeleton because the candidate list spelled them with a
    capital C; the raw bone roster the capture also writes is what exposed
    it. They are built heavy here - as long as the eyes are wide apart and
    thick enough to read at party camera - because the bite is a primary
    attack with its own 2d6 die and it is what carries rot.
    """
    for side in ("L", "R"):
        name = "chelicera_%s" % side
        root = capture.head(rig, name)
        sign = -1.0 if side == "L" else 1.0
        points = [root,
                  root + Vector((sign * 0.02, -0.11, -0.02)),
                  root + Vector((sign * 0.05, -0.21, -0.07)),
                  root + Vector((sign * 0.06, -0.28, -0.13))]
        radii = [r for r in (0.062, 0.050, 0.030, 0.007)]
        tube(bm, weights, uvs, points, radii, [name] * 4, POINT_REGION,
             segments=8)
        # A fang barb halfway down the outer edge, which is what makes a
        # mandible read as a weapon rather than a feeler.
        barb = root + Vector((sign * 0.055, -0.19, -0.055))
        tube(bm, weights, uvs,
             [barb, barb + Vector((sign * 0.05, -0.02, -0.04))],
             [0.022, 0.004], [name] * 2, POINT_REGION,
             segments=6)


def foreclaw(bm, weights, uvs, rig, side):
    """One elevated barbed foreclaw, along the seven pedipalp bones.

    Raised well above the walking line, which is the printed posture and
    also what keeps the two of them from reading as a seventh and eighth
    leg. The barbs are the part a spider does not have, and they are the
    reason this limb is painted on its own atlas region.
    """
    names = ["pedipalp%d_%s" % (index, side) for index in range(1, 8)]
    points = [capture.head(rig, name) for name in names]
    reach = points[3] - points[0]
    if reach.length < 1e-5:
        reach = Vector((0.0, -1.0, 0.0))
    direction = reach.normalized()
    outward = Vector((1.0 if side == "R" else -1.0, 0.0, 0.0))
    # Carried high. The Sprint 20 chelae sat level and read as two balls
    # among the legs; these are a bebelith's defining limb and they are held
    # where a mantis holds its own.
    # The scorpion's proven reach, raised. The first build took a longer
    # one along a direction read from the chain's first four bones, which on
    # this rig point sideways rather than forward: one foreclaw left the
    # frame entirely and the other disappeared behind the body.
    elbow = points[3] + direction * 0.18 + outward * 0.12 + UP * 0.24
    wrist = elbow + direction * 0.33 + outward * 0.02 + UP * 0.11
    arm_points = [points[0], points[1], points[2], points[3], elbow, wrist]
    arm_radii = [r for r in (0.115, 0.108, 0.102, 0.098, 0.094, 0.086)]
    arm_bones = [names[0], names[1], names[2], names[3], names[4], names[5]]
    tube(bm, weights, uvs, arm_points, arm_radii, arm_bones, CLAW_REGION,
         segments=10)

    # The claw: one long curved blade rather than a crab's two fingers. A
    # bebelith tears armour apart with these, and a blade says that where an
    # open pincer says seizing.
    inward = Vector((-1.0 if side == "R" else 1.0, 0.0, 0.0))
    blade_aim = (direction * 1.0 + inward * 0.28 - UP * 0.42).normalized()
    blade = [wrist,
             wrist + blade_aim * 0.15,
             wrist + blade_aim * 0.27,
             wrist + blade_aim * 0.38]
    tube(bm, weights, uvs, blade,
         [r for r in (0.088, 0.070, 0.040, 0.008)],
         [names[6]] * 4, CLAW_REGION, segments=9)
    # Three barbs along the blade's outer edge. These are the whole reason
    # the limb is its own region: they have to paint as hard points against
    # a chitin that is matte everywhere else.
    for step, size in ((0.15, 0.032), (0.25, 0.026), (0.34, 0.019)):
        root = wrist + blade_aim * step + outward * 0.022
        tip = root + (outward * 0.7 - UP * 0.3 + direction * 0.2).normalized() \
            * 0.078
        tube(bm, weights, uvs, [root, tip],
             [size, 0.004], [names[6]] * 2, POINT_REGION,
             segments=6)


def legs(bm, weights, uvs, rig):
    """Six walking legs, three chains a side.

    The fourth chain carries nothing. That is the decision the whole body
    rests on: the donor walks on eight and this creature walks on six, so
    the chain is left empty exactly as the Sprint 14 ants leave it, and the
    silhouette stops being the Doomspider's.

    They are also long. A Huge creature standing on a Medium rig's leg
    positions would squat; the geometry runs past each foot along the leg's
    own direction so the animal stands as tall as its footprint is wide.
    """
    for side in ("L", "R"):
        for index in WALKING_CHAINS:
            upper_name = "%s_Leg%d_Upper" % (side, index)
            lower_name = "%s_Leg%d_Lower" % (side, index)
            foot_name = "%s_Foot%d" % (side, index)
            upper = capture.head(rig, upper_name)
            lower = capture.head(rig, lower_name)
            foot = capture.head(rig, foot_name)
            # A knee. The donor's upper, lower and foot bones are nearly
            # collinear in bind pose - a spider's are - so following them
            # exactly gave six straight spikes and a body resting on the
            # ground. Each leg is carried out and up at the knee before it
            # drops, which is what a walking leg does and what lifts the
            # animal onto it.
            sign = -1.0 if side == "L" else 1.0
            knee = lower + Vector((sign * (0.10 + 0.03 * index), 0.0, 0.11))
            ankle = foot + Vector((sign * (0.13 + 0.04 * index), 0.0, 0.0))
            stride = ankle - knee
            if stride.length < 1e-5:
                stride = Vector((sign * 0.2, 0.0, -1.0))
            toe = ankle + stride.normalized() * 0.09
            points = [upper, upper.lerp(knee, 0.55), knee,
                      knee.lerp(ankle, 0.55), ankle, toe]
            # Heavier than a spider's. This creature hunts other demons and
            # the legs have to look like they carry it.
            radii = [r for r in
                     (0.088, 0.076, 0.066, 0.048, 0.030, 0.008)]
            bones = [upper_name, upper_name, lower_name, lower_name,
                     foot_name, foot_name]
            tube(bm, weights, uvs, points, radii, bones, LIMB_REGION,
                 segments=8)


def build_body(bm, weights, uvs, rig):
    cephalothorax(bm, weights, uvs, rig)
    abdomen(bm, weights, uvs, rig)
    mandibles(bm, weights, uvs, rig)
    for side in ("L", "R"):
        foreclaw(bm, weights, uvs, rig, side)
    legs(bm, weights, uvs, rig)


def chain_usage(used):
    """Which leg chains carry geometry. Three full, one empty, by design."""
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
    rig_data, rig, rig_hash = capture.load(args.capture, FAMILY)
    armature = shared.build_armature(rig_data)
    armature.name = "BebelithRigPreview"
    mesh = bpy.data.meshes.new("BebelithMesh")
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
    # Six legs, proved from the built geometry rather than asserted. The
    # fourth chain must be EMPTY, not partial: a bebelith that walks on
    # seven legs is a defect, and a bebelith that walks on eight is the
    # Doomspider this body exists to replace.
    expected = {"0": "full", "1": "full", "2": "full", "3": "empty"}
    if usage != expected:
        raise SystemExit("a bebelith walks on six legs and elevates its "
                         "foreclaws: " + json.dumps(usage, sort_keys=True))
    drivers = {name for entries in indexed.values() for name, _ in entries}
    if not set(capture.PEDIPALPS) <= drivers:
        raise SystemExit("both foreclaws must ride their whole pedipalp chain")
    if not set(capture.CHELICERAE) <= drivers:
        raise SystemExit("the printed bite needs a jaw that the rig has")
    abdomen_drivers = drivers & set(capture.ABDOMEN_CHAIN)
    if not {"Tail1_M", "Tail3_M", "UpperTorso"} <= abdomen_drivers:
        raise SystemExit("the abdomen must ride the whole abdomen chain")

    modifier = obj.modifiers.new(name="Armature", type="ARMATURE")
    modifier.object = armature
    obj.parent = armature
    shared.attach_preview_material(obj, mesh, args.albedo)
    xs, ys, zs = ([getattr(vertex.co, axis) for vertex in mesh.vertices]
                  for axis in ("x", "y", "z"))
    report = {"schemaVersion": 3, "creature": KIND,
              "donorFamily": "doomspider", "captureSprint": "Sprint 21",
              "rigSha256": rig_hash, "rigBoneCount": len(rig_data["bones"]),
              "vertices": len(mesh.vertices), "polygons": len(mesh.polygons),
              "boneGroups": len(groups),
              "maxInfluencesPerVertex": max(map(len, indexed.values())),
              "legChainUsage": usage,
              "visibleLegs": 2 * sum(1 for value in usage.values()
                                     if value == "full"),
              "abdomenChainDrivers": sorted(abdomen_drivers),
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
    payload["donorBlueprint"] = capture.DOOMSPIDER
    payload["donorRenderer"] = capture.RENDERER
    payload["legChainUsage"] = usage
    payload["visibleLegs"] = report["visibleLegs"]
    payload["visibleForeclaws"] = 2
    payload["spinneretPairs"] = 2
    payload["abdomenChainDrivers"] = sorted(abdomen_drivers)
    payload["printedSize"] = "Huge"
    payload["authoredLimitation"] = "BEBELITH_ABDOMEN_RIDES_A_TWO_BONE_CHAIN"
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
