#!/usr/bin/env python3
"""Build three original Sprint 13 creatures on private donor bind frames.

The request-local capture supplies only bone names and a measured renderer-local
bind frame. Shipped mesh data contains original geometry, UVs, weights, bone
names and an original albedo hash; no donor transform or native art ships.

The Wolverine and the Shadow Mastiff ride the Worg rig, whose frame Sprint 12
captured for the Goblin Dog, so both read their bones from a Sprint 12 capture.
The Poison Frog rides the Giant Poisonous Frog rig, captured in Sprint 13.
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


KINDS = ("wolverine", "shadow-mastiff", "poisonous-frog")
# The donor family each creature's bones come from, and the sprint whose
# capture recorded that family's bind frame.
DONORS = {"wolverine": "worg", "shadow-mastiff": "worg",
          "poisonous-frog": "giant-poisonous-frog"}
CAPTURE_SPRINT = {"wolverine": "Sprint 12", "shadow-mastiff": "Sprint 12",
                  "poisonous-frog": "Sprint 13"}


def measured_rig(path, kind):
    raw = Path(path).read_bytes()
    captured = json.loads(raw)
    donor = DONORS[kind]
    expected = "hidden " + CAPTURE_SPRINT[kind] + " " + donor + " donor view"
    if expected not in captured.get("source", ""):
        raise SystemExit("wrong donor bind frame for " + kind)
    renderers = captured.get("renderers", [])
    if len(renderers) != 1:
        raise SystemExit("the donor must have exactly one renderer")
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
        point = Vector(row["bindPosition"])
        child_names = children.get(name, [])
        if child_names:
            tail = sum((Vector(by_name[child]["bindPosition"])
                        for child in child_names), Vector()) / len(child_names)
        else:
            # The axis only exists in the private preview armature. Runtime
            # binding uses the donor's own complete bind-pose array.
            tail = point + Vector((0.0, 0.05, 0.0))
        if (tail - point).length < 0.001:
            tail = point + Vector((0.0, 0.05, 0.0))
        bones.append({"name": name, "parent": row.get("parent", ""),
                      "index": row["index"], "depth": depth(name),
                      "head": list(point), "tail": list(tail)})
    rig = {"source": captured["source"], "space": captured["space"],
           "renderer": source["renderer"], "rootBone": source["rootBone"],
           "bindPoseCount": source["bindPoseCount"], "bones": bones}
    return rig, {row["name"]: row for row in bones}, \
        hashlib.sha256(raw).hexdigest()


# Each atlas region is pulled this far in from its own edge before the mesh is
# written.
#
# 2026-10-02 art repair. The regions tile the atlas edge to edge, and a tube's
# first and last rings land exactly on its region's u edges, so a bilinear
# sample there reads half its colour from the neighbouring region. The body
# region sits directly beside the crest region, and the Shadow Mastiff is the
# first creature whose crest is deliberately bright - a pale blue, so that its
# eyes are the one light feature on a shadow-black coat. The result was a
# glowing blue ring around its neck and a blue smear across its muzzle. Insetting
# every coordinate inside its own region removes the bleed for good, and costs
# only a two per cent border of texels that nothing was sampling anyway.
UV_REGION_INSET = 0.02


def inset_region_uvs(uvs, margin=UV_REGION_INSET):
    regions = [(name, shared.ATLAS[name]) for name in shared.ATLAS]
    for vertex, (u, v) in list(uvs.items()):
        for name, (u0, v0, u1, v1) in regions:
            if u0 - 1e-9 <= u <= u1 + 1e-9 and v0 - 1e-9 <= v <= v1 + 1e-9:
                uvs[vertex] = (
                    u0 + margin * (u1 - u0) +
                    (u - u0) * (1.0 - 2.0 * margin),
                    v0 + margin * (v1 - v0) +
                    (v - v0) * (1.0 - 2.0 * margin))
                break
        else:
            raise SystemExit("mesh UV falls outside every atlas region")


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


def paw(bm, weights, uvs, centre, lateral, up, forward, scale, bone,
        region="limbs"):
    ellipsoid(bm, weights, uvs, centre + forward * scale * 0.28,
              lateral, up, forward, (scale * 0.62, scale * 0.24,
              scale * 0.90), bone, region, 5, 10)


def quadruped_leg(bm, weights, uvs, rig, names, radii, side, up, forward,
                  paw_scale):
    at = lambda name: shared.head(rig, name)
    tube(bm, weights, uvs, [at(name) for name in names], radii, names,
         "limbs", 9)
    paw(bm, weights, uvs, at(names[-1]), side, up, forward,
        paw_scale, names[-1])


def worg_frame():
    return Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, -1))


def wolverine(bm, weights, uvs, rig):
    """A wolverine is a mustelid, not a dog, and the silhouette has to say so.

    Three proportions carry the read, and all three are measured against the
    Hyena already shipped on this same rig family: the body is heavier than the
    shoulder (0.40 at the chest against the Hyena's 0.36) and sits low, the
    muzzle is short and blunt (0.19 forward against the Hyena's 0.29), and the
    tail is a thick bush rather than a taper. The ears are small and round and
    set low on the skull, where a canid's are tall triangles set high.
    """
    at = lambda name: shared.head(rig, name)
    side, up, forward = worg_frame()
    lower, spine, upper, withers, neck, skull = (at(name) for name in
        ("Torso_Lower", "spine_0", "Torso_Upper", "withers", "neck", "Head"))
    # The chest is the widest point and the back dips behind it, which is what
    # gives a wolverine its hunched, front-heavy stance.
    chest_centre = upper.lerp(neck, 0.42) + up * 0.02
    tube(bm, weights, uvs,
         [at("tail_01"), lower, spine, upper, chest_centre, neck],
         [0.185, 0.355, 0.368, 0.385, 0.372, 0.250],
         ["tail_01", "Torso_Lower", "spine_0", "Torso_Upper", "withers",
          "neck"],
         "body", 12)
    # Short blunt head: a broad skull with almost no taper, ending in a wide
    # nose pad rather than a snout.
    # 2026-10-02 art repair. The first head took the body tube's own radius at
    # the neck and then only ever narrowed, so neck, skull and muzzle formed
    # one unbroken cone and a rest-pose review frame read as an anteater. The
    # neck ring now pinches below the shoulder and the skull ring swells back
    # out above it, which is what makes a broad blunt mustelid head legible as
    # a head at all.
    muzzle = skull + forward * 0.155 + up * -0.02
    tube(bm, weights, uvs,
         [neck, skull, skull + forward * 0.070,
          skull + forward * 0.120 + up * -0.012, muzzle],
         [0.195, 0.252, 0.236, 0.182, 0.105],
         ["neck", "Head", "Head", "jaw_woo_up_add", "jaw_woo_up"],
         "body", 12)
    tube(bm, weights, uvs,
         [at("jaw"), skull + forward * 0.115 + up * -0.105,
          skull + forward * 0.185 + up * -0.085],
         [0.160, 0.128, 0.060], ["jaw", "jaw_woo_down", "jaw_woo_down"],
         "beak", 10)
    ellipsoid(bm, weights, uvs, muzzle + forward * 0.020,
              side, up, forward, (0.072, 0.058, 0.045), "jaw_woo_up",
              "crest", 5, 10)
    # Small rounded ears set on top of the skull rather than out to the side.
    #
    # 2026-10-02 art repair. The first pair sat 0.105 above a skull of radius
    # 0.215 and stood only 0.056 tall, so both were entirely inside the head
    # and no review frame showed an ear at all. Their bases now meet the skull
    # surface and their caps clear it, while staying short and round: a
    # canid's are tall triangles carried high, and that difference is one of
    # the few head cues that survives the party camera.
    for ear_name, sign in (("ear_L", 1.0), ("ear_R", -1.0)):
        ear_centre = skull + side * sign * 0.135 + up * 0.180 + \
            forward * -0.030
        ellipsoid(bm, weights, uvs, ear_centre,
                  side, up, forward, (0.038, 0.072, 0.068), ear_name,
                  "body", 6, 12)
    # 2026-10-02 art repair. The shared eye helper measures its spread from
    # the skull's own origin, which leaves the beads inside any head wider than
    # the creatures it was written for, so neither eye reached the surface.
    # They are placed against the skull instead, small and deep-set as a
    # mustelid's are.
    for sign in (-1.0, 1.0):
        ellipsoid(bm, weights, uvs,
                  skull + side * (sign * 0.195) + up * 0.112 +
                  forward * 0.060,
                  side, up, forward, (0.024, 0.024, 0.020), "Head",
                  "crest", 5, 10)

    # Short, heavy legs. Thicker than the Hyena's and shorter in reach, which
    # keeps the body low to the ground.
    for prefix in ("L", "R"):
        front_tip = "front_paw_tip_L" if prefix == "L" else \
            "front_paw__tip_R"
        quadruped_leg(bm, weights, uvs, rig,
            [prefix + "_Arm_Upper", prefix + "_Arm_Lower",
             prefix + "_Palm", front_tip],
            [0.160, 0.135, 0.100, 0.062], side, up, forward, 0.152)
        quadruped_leg(bm, weights, uvs, rig,
            [prefix + "_Leg0_Upper", prefix + "_Leg0_Lower",
             prefix + "_Leg0_Lower2", prefix + "_Foot0",
             "hindpaw_tip_" + prefix],
            [0.170, 0.142, 0.105, 0.080, 0.052], side, up, forward, 0.162)

    # A short bush rather than the tapering rope a canid carries.
    #
    # 2026-10-02 art repair. The first tail held 0.13 of girth as far out as
    # the third bone and ran the whole donor tail chain, so a review frame
    # showed a beaver's paddle reaching sixty per cent of the body length
    # behind an animal whose own tail is nearer a quarter of it. The girth now
    # falls away by the third bone, which keeps the bushy base the species is
    # read by and lets the rest of the chain carry a wisp.
    tail_names = ["tail_01", "tail_02", "tail_03", "tail_04"]
    tail_points = [at(name) for name in tail_names]
    direction = (tail_points[-1] - tail_points[-2]).normalized()
    tail_points.append(tail_points[-1] + direction * 0.055)
    tube(bm, weights, uvs, tail_points,
         [0.138, 0.122, 0.072, 0.024, 0.006],
         tail_names + [tail_names[-1]], "limbs", 10)


def shadow_mastiff(bm, weights, uvs, rig):
    """A big lean hound with a deep chest and a long heavy tail.

    Printed as a Medium outsider with a 50-foot speed and a tail slap it can
    actually strike with, so the tail is long and thick enough to read as a
    weapon rather than a rudder. Against the Hyena on this same rig family the
    chest is deeper and the waist narrower, which reads as a running hound, and
    the muzzle is longer.
    """
    at = lambda name: shared.head(rig, name)
    side, up, forward = worg_frame()
    lower, spine, upper, withers, neck, skull = (at(name) for name in
        ("Torso_Lower", "spine_0", "Torso_Upper", "withers", "neck", "Head"))
    # 2026-10-02 art repair. The first body ran nearly the same girth from
    # rump to shoulder and lifted the withers ring by 0.055, so a review frame
    # showed a long low tube with an angular hump - an otter rather than a
    # hound. The loin now draws in well below the chest, which is the single
    # proportion that reads as a deep-chested running dog, and the withers
    # lift drops to a value that no longer breaks the topline.
    chest_centre = upper.lerp(neck, 0.46) + up * 0.030
    tube(bm, weights, uvs,
         [at("tail_01"), lower, spine, upper, chest_centre, neck],
         [0.150, 0.228, 0.238, 0.332, 0.352, 0.222],
         ["tail_01", "Torso_Lower", "spine_0", "Torso_Upper", "withers",
          "neck"],
         "body", 12)
    # 2026-10-02 art repair. The head took the body tube's radius at the neck
    # and then only narrowed, over a muzzle reaching 0.330 forward, so a
    # review frame read as an otter rather than a hound. The neck ring pinches
    # and the skull ring swells above it, and the muzzle pulls back to a
    # length a mastiff would carry.
    muzzle = skull + forward * 0.290 + up * -0.020
    tube(bm, weights, uvs,
         [neck, skull, skull + forward * 0.095,
          skull + forward * 0.210 + up * -0.012, muzzle],
         [0.185, 0.232, 0.206, 0.142, 0.075],
         ["neck", "Head", "Head", "jaw_woo_up_add", "jaw_woo_up"],
         "body", 12)
    tube(bm, weights, uvs,
         [at("jaw"), skull + forward * 0.175 + up * -0.115,
          skull + forward * 0.275 + up * -0.090],
         [0.140, 0.108, 0.034], ["jaw", "jaw_woo_down", "jaw_woo_down"],
         "beak", 10)
    # 2026-10-02 art repair. The nose pad was painted from the crest region,
    # which on this creature alone is a pale blue so that its eyes are the one
    # bright feature on a shadow-black coat - so the first build gave it a
    # glowing white nose. The pad moves to the jaw region, which is as dark as
    # the rest of the head, and the eyes keep the crest to themselves.
    ellipsoid(bm, weights, uvs, muzzle + forward * 0.024,
              side, up, forward, (0.062, 0.052, 0.042), "jaw_woo_up",
              "beak", 5, 10)
    # Ears carried high, as a hound's are.
    #
    # 2026-10-02 art repair. The first pair stood 0.280 tall on a 0.215 skull
    # and only 0.052 wide, so a review frame showed a rectangular slab
    # floating over the head rather than an ear. They now meet the skull
    # surface at the base and keep enough depth to read as ears from the side.
    for ear_name, sign in (("ear_L", 1.0), ("ear_R", -1.0)):
        ear_centre = skull + side * sign * 0.150 + up * 0.175 + \
            forward * -0.030
        ellipsoid(bm, weights, uvs, ear_centre,
                  side, up, forward, (0.052, 0.112, 0.072), ear_name,
                  "body", 6, 12)
    # 2026-10-02 art repair. The shared eye helper left both beads inside the
    # skull. On this creature that cost the whole design: its crest region is
    # painted a pale blue precisely so that the eyes are the one bright feature
    # on a shadow-black coat, and a buried eye shows nothing at all. They are
    # placed against the skull surface and made large enough to carry at the
    # party camera, which is the only distance that matters here.
    for sign in (-1.0, 1.0):
        ellipsoid(bm, weights, uvs,
                  skull + side * (sign * 0.175) + up * 0.085 +
                  forward * 0.105,
                  side, up, forward, (0.032, 0.032, 0.026), "Head",
                  "crest", 5, 10)

    for prefix in ("L", "R"):
        front_tip = "front_paw_tip_L" if prefix == "L" else \
            "front_paw__tip_R"
        quadruped_leg(bm, weights, uvs, rig,
            [prefix + "_Arm_Upper", prefix + "_Arm_Lower",
             prefix + "_Palm", front_tip],
            [0.120, 0.096, 0.066, 0.042], side, up, forward, 0.112)
        quadruped_leg(bm, weights, uvs, rig,
            [prefix + "_Leg0_Upper", prefix + "_Leg0_Lower",
             prefix + "_Leg0_Lower2", prefix + "_Foot0",
             "hindpaw_tip_" + prefix],
            [0.134, 0.108, 0.076, 0.058, 0.038], side, up, forward, 0.122)

    # The printed tail slap needs reach, so the tail still runs past the last
    # donor bone.
    #
    # 2026-10-02 art repair. Keeping mass along the whole length produced a
    # flat paddle as long as the body, which read as a beaver's tail on a dog.
    # Reach is what the attack needs, not girth, so the length stays and the
    # taper becomes a hound's: heavy at the root, a whip by the tip.
    tail_names = ["tail_01", "tail_02", "tail_03", "tail_04"]
    tail_points = [at(name) for name in tail_names]
    direction = (tail_points[-1] - tail_points[-2]).normalized()
    tail_points += [tail_points[-1] + direction * 0.130 + up * -0.020,
                    tail_points[-1] + direction * 0.240 + up * -0.055]
    tube(bm, weights, uvs, tail_points,
         [0.088, 0.070, 0.052, 0.034, 0.018, 0.005],
         tail_names + [tail_names[-1], tail_names[-1]], "limbs", 10)


def poisonous_frog(bm, weights, uvs, rig):
    """A small frog's proportions, not a scaled-down large one.

    The creature shares its rig with the Giant Poisonous Frog, so the thing
    that has to differ is proportion rather than size: a small frog is rounder
    in the body, shorter in the leg relative to that body, and much larger in
    the eye relative to the head. Those three ratios are what separate a thumb
    sized poison frog from a dog sized one, and they are what this mesh sets.
    The mechanical Tiny footprint and the view-scale entry carry the size.
    """
    at = lambda name: shared.head(rig, name)
    side, up, forward = (Vector((1, 0, 0)), Vector((0, 1, 0)),
                         Vector((0, 0, 1)))
    lower, spine, chest, skull = (at(name) for name in
        ("LowerTorso", "spine_3_joint", "chest_joint", "Head"))
    # A round, almost spherical body: widest at the middle and short from hip
    # to shoulder, which is the small-frog signature.
    #
    # 2026-10-02 art repair. The widest ring was placed at the donor's stomach
    # bone, which hangs 0.40 below the spine it belongs to, so the body dived
    # in the middle and rose again at both ends. A review frame showed a
    # creased wedge rather than a frog. The ring now sits on the spine, where
    # the body actually is, and keeps its weight on the stomach bone so the
    # belly still deforms with the donor's own belly joint. The girth comes
    # down with it, because at the first build's 0.52 the body dwarfed the
    # head and limbs it has to be read against.
    tube(bm, weights, uvs,
         [lower + forward * -0.16, lower, spine, at("UpperTorso"), chest,
          skull],
         [0.20, 0.36, 0.42, 0.44, 0.40, 0.25],
         ["LowerTorso", "LowerTorso", "spine_3_joint", "stomach",
          "chest_joint", "Head"],
         "body", 12)
    # A broad flat head with a wide mouth line and almost no snout.
    #
    # 2026-10-02 art repair. The head was a circular tube, so widening it
    # enough to carry the eyes would also have made it a ball. A frog's head
    # is wide and low - far wider than it is tall - and that shape is what
    # gives the eyes somewhere to sit, so the skull is now an explicit
    # flattened dome and the jaw is a shelf that protrudes below it to carry
    # the mouth line from the side.
    jaw = at("jaw_skinJoint1")
    head_centre = skull + forward * 0.060 + up * 0.020
    ellipsoid(bm, weights, uvs, head_centre, side, up, forward,
              (0.400, 0.260, 0.280), "Head", "body", 7, 14)
    tube(bm, weights, uvs,
         [jaw + up * -0.055, jaw + forward * 0.16 + up * -0.070,
          jaw + forward * 0.29 + up * -0.050],
         [0.250, 0.200, 0.080],
         ["jaw_skinJoint1", "jaw_endJoint", "jaw_endJoint"], "beak", 12)
    # Large domed eyes set high and wide, the clearest small-frog cue.
    #
    # 2026-10-02 art repair. The domes were placed at the donor's own eyelid
    # joints, which sit where the Giant Poisonous Frog's eyes are rather than
    # where this smaller head is, so both hung in the air clear of the skull
    # and a review frame showed one floating beside the creature. They are now
    # placed against the skull dome itself, half buried in it and breaking its
    # outline at the top and the sides the way a frog's eyes do. They keep
    # their eyelid-joint weights, so the donor's blink still drives them.
    for lid, sign in (("l_t_eyeLid_joint", -1.0), ("r_t_eyeLid_joint", 1.0)):
        centre = skull + side * (sign * 0.275) + up * 0.185 + forward * 0.120
        ellipsoid(bm, weights, uvs, centre,
                  side, up, forward, (0.130, 0.134, 0.126), lid,
                  "crest", 6, 12)

    # Short folded legs. The hind legs stay tucked rather than extended, which
    # keeps the body compact instead of sprawling like the larger frog.
    for prefix in ("L", "R"):
        hind = [prefix + "_Leg0_Upper", prefix + "_Leg0_Lower",
                prefix + "_Foot0"]
        tube(bm, weights, uvs, [at(name) for name in hind],
             [0.190, 0.138, 0.088], hind, "limbs", 9)
        toe = ("l_toe_joint" if prefix == "L" else "r_toe_joint")
        toe_tip = ("l_toeTip_joint" if prefix == "L" else "r_toeTip_joint")
        tube(bm, weights, uvs, [at(prefix + "_Foot0"), at(toe), at(toe_tip)],
             [0.088, 0.066, 0.030], [prefix + "_Foot0", toe, toe_tip],
             "limbs", 8)
        front = [prefix + "_Arm_Upper", prefix + "_Arm_Lower", prefix + "_Palm"]
        present = [name for name in front if name in rig]
        if len(present) >= 2:
            tube(bm, weights, uvs, [at(name) for name in present],
                 [0.115, 0.088, 0.060][:len(present)], present, "limbs", 8)
            # 2026-10-02 art repair. The pad was narrower than the tube it
            # capped, so each forelimb ended in a visible flat cut face.
            paw(bm, weights, uvs, at(present[-1]), side, up, forward,
                0.110, present[-1])


BUILDERS = {"wolverine": wolverine, "shadow-mastiff": shadow_mastiff,
            "poisonous-frog": poisonous_frog}
# The creature coordinate frame each mesh was authored in, recorded in the
# shipped payload so a reader never has to infer it.
SPACES = {
    "wolverine": "donor renderer local; +X left, +Y up, -Z forward",
    "shadow-mastiff": "donor renderer local; +X left, +Y up, -Z forward",
    "poisonous-frog": "donor renderer local; +X left, +Y up, +Z forward",
}


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
    inset_region_uvs(uvs)
    bm.normal_update()
    bm.to_mesh(mesh)
    indexed = {vertex.index: entries for vertex, entries in weights.items()}
    uv_indexed = {vertex.index: uv for vertex, uv in uvs.items()}
    bm.free()
    if len(indexed) != len(mesh.vertices) or \
            len(uv_indexed) != len(mesh.vertices):
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
              "donorFamily": DONORS[args.kind],
              "captureSprint": CAPTURE_SPRINT[args.kind],
              "rigSha256": rig_hash,
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
    payload["space"] = SPACES[args.kind]
    with Path(args.mesh_data).open("w", encoding="utf-8", newline="\n") as out:
        out.write(json.dumps(payload, indent=1) + "\n")
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.export_scene.fbx(filepath=args.fbx_out, use_selection=True,
        add_leaf_bones=False, bake_anim=False, path_mode="COPY",
        apply_scale_options="FBX_SCALE_ALL",
        object_types={"ARMATURE", "MESH"})
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[sprint13-creature] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
