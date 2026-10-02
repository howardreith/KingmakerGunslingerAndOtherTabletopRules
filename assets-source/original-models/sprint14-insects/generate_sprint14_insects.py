#!/usr/bin/env python3
"""Build the Sprint 14 insect vertical slices on the Giant Spider bind frame.

The owner's order asks for two minimal slices before five Sprint 14 and 15
models are authored against one donor: a ground insect with six visible legs
and no phantom spider-leg contacts, and a flyer with credible wings and no
hidden eighth leg. This script authors both, and the reason both can ride the
same rig is what the measured bind frame turned out to contain.

The donor has four leg chains a side, eight in all, fanning front to back. The
first three a side - Leg0 forward, Leg1 outward, Leg2 outward and back - are
exactly an insect's fore, mid and hind pair, and in a standard alternating
tetrapod gait the donor moves {L0, R1, L2} against {R0, L1, R2}, which is the
insect alternating tripod itself. So six legs is a subtraction from this rig
rather than an addition to it, and the gait comes out right rather than merely
tolerable.

That leaves Leg3 a side, and the two slices answer it differently. The ant
weights nothing to it, so there is no eighth leg to hide: a bone with no
vertices draws nothing. The beetle gives it the membranous wings, because Leg3
is the one spare chain that is already behind the body and already rises above
it - knee at 1.064 against a body at 0.687 - which is where a beetle's wings
are and nothing else on this rig is.

Only bone names and a measured bind frame came out of the game. No donor
vertices, triangles, materials, textures or animation data enter this
repository.
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
HERE = Path(__file__).resolve()
sys.path.insert(0, str(HERE.parents[1] / "pteranodon"))
sys.path.insert(0, str(HERE.parents[1] / "sprint13-creatures"))
import generate_pteranodon as shared  # noqa: E402
# The atlas inset and the two primitive builders come from the Sprint 13
# generator rather than being copied, because the inset encodes the bleed
# repair that gave the Shadow Mastiff a glowing neck ring, and a second copy
# would be free to drift away from it.
from generate_sprint13_creatures import (  # noqa: E402
    ellipsoid, inset_region_uvs, tube)


KINDS = ("giant-ant-soldier", "giant-ant-worker", "fire-beetle")
DONORS = {kind: "giant-spider" for kind in KINDS}
CAPTURE_SPRINT = {kind: "Sprint 14" for kind in KINDS}
SPACES = {kind: "donor renderer local; +X right, +Z up, -Y forward"
          for kind in KINDS}


def spider_frame():
    """The donor's own frame, read off the measured capture.

    The abdomen chain runs out to +Y at a nearly constant height, so +Y is
    backward and the mouthparts at negative Y are the front; the knees rise to
    1.06 over a body at 0.69, so +Z is up.
    """
    return (Vector((1.0, 0.0, 0.0)), Vector((0.0, 0.0, 1.0)),
            Vector((0.0, -1.0, 0.0)))


def leg_chain(prefix, index):
    return (prefix + "_Leg" + index + "_Upper",
            prefix + "_Leg" + index + "_Lower",
            prefix + "_Foot" + index)


# What each slice may weight to each of the donor's eight leg chains. The build
# fails if the mesh disagrees, which is what turns "six visible legs and no
# phantom eighth" into an offline invariant rather than something a reviewer has
# to catch by eye.
EXPECTED_CHAINS = {
    "giant-ant-soldier": {
        "0": "full", "1": "full", "2": "full", "3": "empty"},
    "giant-ant-worker": {
        "0": "full", "1": "full", "2": "full", "3": "empty"},
    "fire-beetle": {
        "0": "full", "1": "full", "2": "full", "3": "upper-and-lower"},
}

# The two ant castes differ in exactly the three things that separate them in
# the source - head size, mandible weight and whether there is a sting - so
# they are one builder taking a caste rather than two near-copies of ninety
# lines. A worker's head is smaller because it does not have to carry a
# soldier's mandibles, and its gaster is correspondingly the larger mass.
ANT_CASTES = {
    "giant-ant-soldier": {
        "head": (0.360, 0.315, 0.330), "headReach": 0.56,
        "mandibleRadii": (0.085, 0.066, 0.042, 0.014),
        "mandibleReach": 0.58, "mandibleSpread": 0.190,
        "gaster": 1.00, "sting": True,
    },
    "giant-ant-worker": {
        "head": (0.276, 0.244, 0.258), "headReach": 0.49,
        "mandibleRadii": (0.055, 0.043, 0.027, 0.010),
        "mandibleReach": 0.37, "mandibleSpread": 0.145,
        "gaster": 1.10, "sting": False,
    },
}


def measured_rig(path, kind):
    raw = Path(path).read_bytes()
    captured = json.loads(raw)
    expected = ("hidden " + CAPTURE_SPRINT[kind] + " " + DONORS[kind] +
                " donor view")
    if expected not in captured.get("source", ""):
        raise SystemExit("wrong donor bind frame for " + kind)
    renderers = captured.get("renderers", [])
    if len(renderers) != 1:
        raise SystemExit("the donor must have exactly one renderer")
    source = renderers[0]
    rows = source.get("bones", [])
    if (not rows or source.get("boneCount") != source.get("bindPoseCount") or
            len(rows) != source.get("boneCount")):
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
    return (rig, {row["name"]: row for row in bones},
            hashlib.sha256(raw).hexdigest())


def blade(bm, weights, uvs, spine, widths, thicknesses, lateral, normal,
          bones, region, segments=8, u_range=(0.0, 1.0)):
    """A flat double-sided blade through `spine`, as a wing or an elytron.

    The shared tube builder picks its own cross-section frame from the spine
    direction, which is right for a limb and wrong for anything that has to
    stay flat in a chosen plane. This takes the width and thickness axes
    explicitly, and keeps the shared v convention so the ring closes without a
    texture seam.

    `u_range` restricts the blade to part of its atlas region. The beetle needs
    it: its elytra and its wings are both blades and both belong in the
    membrane region, but one has to read as hard shell and the other as
    translucent film, so they take half the region each rather than sharing one
    painting. The halves leave a gutter at the midpoint that neither samples,
    because the two paintings differ sharply there and a bilinear sample across
    the boundary would put a pale line on the shell and a dark one on the
    film - the same defect the atlas inset exists to prevent at the region
    edges.
    """
    if not (len(spine) == len(widths) == len(thicknesses) == len(bones)):
        raise ValueError("blade needs matching spine, width and bone rows")
    lateral = lateral.normalized()
    normal = normal.normalized()
    lengths = [0.0]
    for index in range(1, len(spine)):
        lengths.append(lengths[-1] + (spine[index] - spine[index - 1]).length)
    total = lengths[-1] or 1.0
    rings = []
    for index, centre in enumerate(spine):
        ring = []
        for step in range(segments):
            angle = 2.0 * math.pi * step / segments
            offset = (lateral * (math.cos(angle) * widths[index]) +
                      normal * (math.sin(angle) * thicknesses[index]))
            vertex = bm.verts.new(centre + offset)
            blend = min(shared.JOINT_BLEND, 0.5)
            if (index + 1 < len(bones) and
                    bones[index + 1] != bones[index]):
                weights[vertex] = [(bones[index], 1.0 - blend),
                                   (bones[index + 1], blend)]
            else:
                weights[vertex] = [(bones[index], 1.0)]
            uvs[vertex] = shared.region_uv(
                region,
                u_range[0] + (u_range[1] - u_range[0]) *
                (lengths[index] / total),
                shared.fold(angle))
            ring.append(vertex)
        rings.append(ring)
    for index in range(len(rings) - 1):
        for step in range(segments):
            nxt = (step + 1) % segments
            try:
                bm.faces.new((rings[index][step], rings[index + 1][step],
                              rings[index + 1][nxt], rings[index][nxt]))
            except ValueError:
                pass
    for ring, flip in ((rings[0], True), (rings[-1], False)):
        try:
            bm.faces.new(tuple(reversed(ring)) if flip else tuple(ring))
        except ValueError:
            pass
    return rings


def insect_leg(bm, weights, uvs, rig, prefix, index, scale):
    """One insect leg: a heavy femur to the knee, then a thin tibia and tarsus.

    The thick-then-thin break at the knee is the whole read. A spider's limb
    tapers evenly from body to tip, the first build inherited that, and six of
    them read as spikes radiating out of a lump. An insect's femur is the
    thickest part of it and its tibia is a wire, and that contrast is what
    survives at party-camera distance when nothing else about a leg does.

    The donor's foot bone sits well clear of the ground in the bind frame, so
    the geometry continues a little past it along the knee-to-foot line rather
    than stopping at the bone. That is what puts a tarsus where the foot
    contact is instead of ending the limb in mid-air.
    """
    upper, lower, foot = leg_chain(prefix, index)
    at = lambda name: shared.head(rig, name)
    hip, knee, toe = at(upper), at(lower), at(foot)
    direction = toe - knee
    if direction.length < 1e-6:
        direction = Vector((0.0, 0.0, -1.0))
    tube(bm, weights, uvs,
         [hip, hip + (knee - hip) * 0.55, knee, knee + (toe - knee) * 0.50,
          toe, toe + direction.normalized() * 0.20],
         [scale * value for value in (1.00, 1.15, 0.76, 0.42, 0.33, 0.15)],
         [upper, upper, lower, lower, foot, foot], "limbs", 9)


def giant_ant(bm, weights, uvs, rig, caste):
    """An ant is three masses on a thread, and that is the whole silhouette.

    Head, mesosoma and gaster separated by a pinched petiole is what makes an
    ant read as an ant rather than as a beetle or a spider, and the donor gives
    all three for nothing: the cephalothorax carries the head and mesosoma,
    Tail1_M is the petiole, and the abdomen chain beyond it is the gaster. The
    proportions are the soldier caste's - roughly three tenths head, a quarter
    mesosoma, a third gaster - because an oversized head and the mandibles on
    it are what separate a soldier from a worker, and they are also why the
    chartered bite has a grab. The first build made the head the smallest of
    the three and the waist too short to see, so it read as a two-part body
    with a muzzle. The worker caste takes the same body with a smaller head,
    lighter mandibles, a slightly larger gaster and no sting, which is exactly
    what the Worker template removes from the stat block.

    The mandibles ride the donor's own chelicerae and the antennae its
    pedipalps, so both articulate. The pedipalp chain descends as it reaches
    forward, which is the opposite of an antenna, so each ring is lifted
    further than the last until the chain arches over the head and comes down
    past the mandible tips: the donor supplies the motion and the geometry
    supplies the shape.
    """
    at = lambda name: shared.head(rig, name)
    side, up, forward = spider_frame()
    thorax = at("LowerTorso")
    petiole, gaster, rear = (at(name) for name in
                             ("Tail1_M", "UpperTorso", "Tail3_M"))

    shape = ANT_CASTES[caste]
    head_centre = thorax + forward * shape["headReach"] + up * 0.020
    ellipsoid(bm, weights, uvs, head_centre, side, up, forward,
              shape["head"], "LowerTorso", "body", 9, 16)

    # The mesosoma, humped at the front the way an ant's pronotum is and
    # narrowing hard into the waist.
    tube(bm, weights, uvs,
         [head_centre - forward * 0.26,
          thorax + forward * 0.10 + up * 0.075,
          thorax - forward * 0.08 + up * 0.055,
          thorax - forward * 0.24 - up * 0.010,
          petiole - forward * 0.02 - up * 0.030],
         [0.175, 0.240, 0.215, 0.140, 0.078],
         ["LowerTorso", "LowerTorso", "LowerTorso", "LowerTorso", "Tail1_M"],
         "body", 14)

    # The waist and the gaster. The waist is short and the contrast across it
    # is severe - a twentieth of the body's width against a quarter of it -
    # because what reads at distance is the ratio, not the length. The first
    # revision held it narrow for twice as far and got a thin neck with a cone
    # at each end, which is a spindle rather than an ant. The gaster behind it
    # is carried a little above the thorax line, the way an ant's is.
    tube(bm, weights, uvs,
         [petiole + up * 0.005,
          petiole - forward * 0.09 + up * 0.010,
          petiole - forward * 0.19 + up * 0.045,
          petiole - forward * 0.31 + up * 0.070,
          gaster - forward * 0.08 + up * 0.080,
          gaster - forward * 0.30 + up * 0.070,
          gaster - forward * 0.46 + up * 0.040,
          gaster - forward * 0.56 + up * 0.010],
         [0.078, 0.070] + [value * shape["gaster"] for value in
                           (0.178, 0.238, 0.258, 0.208, 0.098, 0.026)],
         ["Tail1_M", "Tail1_M", "Tail1_M", "UpperTorso", "UpperTorso",
          "UpperTorso", "Tail3_M", "Tail3_M"],
         "body", 16)

    # The sting. The soldier has one and the worker does not, so its presence
    # is a silhouette feature rather than decoration: a player can tell the two
    # castes apart from behind.
    if shape["sting"]:
        sting_root = gaster - forward * 0.56 + up * 0.010
        tube(bm, weights, uvs,
             [sting_root, sting_root - forward * 0.11 - up * 0.075],
             [0.026, 0.007], ["Tail3_M", "Tail3_M"], "beak", 8)

    for sign, suffix in ((1.0, "_R"), (-1.0, "_L")):
        chel = "chelicera" + suffix
        reach = shape["mandibleReach"]
        base = (head_centre + forward * (reach * 0.52) +
                side * (sign * shape["mandibleSpread"]) - up * 0.070)
        tube(bm, weights, uvs,
             [base,
              base + forward * (reach * 0.38) + side * (sign * 0.100),
              base + forward * (reach * 0.72) + side * (sign * 0.060),
              base + forward * reach - side * (sign * 0.070)],
             list(shape["mandibleRadii"]),
             [chel, chel, chel, chel], "beak", 8)

        palps = ["pedipalp1", "pedipalp2", "pedipalp3", "pedipalp5",
                 "pedipalp7"]
        lift = (0.120, 0.300, 0.500, 0.600, 0.620)
        reach = (0.000, 0.100, 0.260, 0.480, 0.700)
        tube(bm, weights, uvs,
             [at(name + suffix) + up * lift[index] + forward * reach[index]
              for index, name in enumerate(palps)],
             [0.042, 0.034, 0.026, 0.020, 0.010],
             [name + suffix for name in palps], "limbs", 7)

        # Compound eyes on the sides of the head capsule, half buried in it so
        # they break its outline.
        ellipsoid(bm, weights, uvs,
                  head_centre + side * (sign * shape["head"][0] * 0.78) +
                  up * (shape["head"][1] * 0.32) + forward * 0.080,
                  side, up, forward,
                  tuple(value * shape["head"][0] / 0.360
                        for value in (0.095, 0.092, 0.105)),
                  "LowerTorso", "crest", 6, 10)

        # Six legs, on the donor's forward, outward and rearward chains. Leg3
        # is left with no geometry at all, so there is no eighth leg to hide
        # and nothing of the donor's fourth pair can reach the ground.
        for index, scale in (("0", 0.112), ("1", 0.118), ("2", 0.120)):
            insect_leg(bm, weights, uvs, rig, suffix[1], index, scale)


def fire_beetle(bm, weights, uvs, rig):
    """A beetle is one hard shell, and its glow is light rather than fire.

    The primary source gives the Fire Beetle a pair of luminescent glands and
    no fire damage at all, so the glands are the one deliberate feature: two
    domes on the front of the pronotum, painted as the only bright thing on
    the creature, and small enough not to be mistaken for its eyes. Everything
    else is compactness - no waist, a low head tucked under the pronotum, and
    elytra covering the abdomen as two plates with a suture between them. The
    body stops short of the donor's last abdomen bone, because a spider's
    abdomen is far longer than a beetle's and a bone with no geometry on it
    simply draws nothing.

    The wings are why this slice exists. They are weighted to the donor's spare
    fourth leg chain, which is the only pair on this rig already behind the
    body and already above it, but they are not laid along it: a wing runs out
    and back from the shoulder nearly level where a leg dives outward and down,
    and geometry does not have to follow the bone that drives it. The chain
    supplies a stroke and the geometry supplies a wing. Nothing is weighted to
    the fourth foot, so no part of a wing can reach the ground.
    """
    at = lambda name: shared.head(rig, name)
    side, up, forward = spider_frame()
    thorax = at("LowerTorso")
    petiole, abdomen = at("Tail1_M"), at("UpperTorso")

    # Head, neck, pronotum and abdomen as one continuous shell. The previous
    # revision built them as three overlapping primitives, and on a body this
    # wide three primitives read as a stack of plates with seams between them.
    # It is a blade rather than a tube because a tube's cross-section is
    # circular and a beetle is half again as wide as it is tall; the width and
    # height rows below are that section, ring by ring.
    head_centre = thorax + forward * 0.66 - up * 0.040
    body_profile = [
        (forward * 0.92, 0.075, 0.055, "LowerTorso"),
        (forward * 0.86, 0.185, 0.130, "LowerTorso"),
        (forward * 0.66 - up * 0.040, 0.335, 0.195, "LowerTorso"),
        (forward * 0.46 - up * 0.035, 0.255, 0.165, "LowerTorso"),
        (forward * 0.30 + up * 0.010, 0.480, 0.250, "LowerTorso"),
        (forward * 0.10 + up * 0.045, 0.600, 0.300, "LowerTorso"),
        (-forward * 0.08 + up * 0.055, 0.580, 0.305, "LowerTorso"),
    ]
    spine = [thorax + offset for offset, _, _, _ in body_profile]
    widths = [width for _, width, _, _ in body_profile]
    heights = [height for _, _, height, _ in body_profile]
    bones = [bone for _, _, _, bone in body_profile]
    spine += [petiole - forward * 0.02 + up * 0.050,
              abdomen - forward * 0.14 + up * 0.030,
              abdomen - forward * 0.34,
              abdomen - forward * 0.46,
              abdomen - forward * 0.52]
    widths += [0.640, 0.590, 0.405, 0.170, 0.055]
    heights += [0.330, 0.310, 0.215, 0.090, 0.030]
    bones += ["Tail1_M", "UpperTorso", "UpperTorso", "Tail3_M", "Tail3_M"]
    blade(bm, weights, uvs, spine, widths, heights, side, up, bones,
          "body", 16)

    for sign, suffix in ((1.0, "_R"), (-1.0, "_L")):
        # One elytron a side: a long flattened plate over the abdomen, offset
        # from the midline so the suture between the pair shows from above.
        # They ride the abdomen chain, which barely moves, which is exactly
        # right for wing covers.
        blade(bm, weights, uvs,
              [thorax - forward * 0.04 + side * (sign * 0.285) + up * 0.300,
               petiole - forward * 0.02 + side * (sign * 0.320) + up * 0.285,
               abdomen - forward * 0.14 + side * (sign * 0.300) + up * 0.265,
               abdomen - forward * 0.34 + side * (sign * 0.205) + up * 0.165,
               abdomen - forward * 0.48 + side * (sign * 0.075) + up * 0.050],
              [0.300, 0.330, 0.310, 0.215, 0.065],
              [0.130, 0.165, 0.160, 0.105, 0.030],
              side, up,
              ["LowerTorso", "Tail1_M", "UpperTorso", "UpperTorso",
               "Tail3_M"],
              "membrane", 8, (0.02, 0.46))

        # The luminescent glands, on the front of the pronotum where the
        # source puts them. This is a modelled and painted gland; it is not a
        # claim that the game has an illumination model.
        ellipsoid(bm, weights, uvs,
                  head_centre + side * (sign * 0.170) + up * 0.150 -
                  forward * 0.030,
                  side, up, forward, (0.110, 0.088, 0.105), "LowerTorso",
                  "crest", 7, 12)

        # Short mandibles. A fire beetle's bite is a plain 1d4 and its jaws are
        # correspondingly small.
        chel = "chelicera" + suffix
        base = (head_centre + forward * 0.195 + side * (sign * 0.130) -
                up * 0.045)
        tube(bm, weights, uvs,
             [base, base + forward * 0.17 + side * (sign * 0.025),
              base + forward * 0.31 - side * (sign * 0.075)],
             [0.072, 0.048, 0.015], [chel, chel, chel], "beak", 8)

        # Clubbed antennae, shorter than the ant's and thickening at the tip.
        palps = ["pedipalp1", "pedipalp3", "pedipalp5", "pedipalp7"]
        lift = (0.090, 0.235, 0.290, 0.265)
        reach = (0.000, 0.160, 0.350, 0.520)
        tube(bm, weights, uvs,
             [at(name + suffix) + up * lift[index] + forward * reach[index]
              for index, name in enumerate(palps)],
             [0.042, 0.032, 0.030, 0.046],
             [name + suffix for name in palps], "limbs", 7)

        for index, scale in (("0", 0.120), ("1", 0.126), ("2", 0.128)):
            insect_leg(bm, weights, uvs, rig, suffix[1], index, scale)

        # The membranous wings on the spare fourth chain.
        upper, lower, _ = leg_chain(suffix[1], "3")
        root = thorax + side * (sign * 0.300) - forward * 0.14 + up * 0.430
        spine = [root,
                 root + side * (sign * 0.40) - forward * 0.18 + up * 0.070,
                 root + side * (sign * 0.80) - forward * 0.36 + up * 0.105,
                 root + side * (sign * 1.14) - forward * 0.52 + up * 0.105,
                 root + side * (sign * 1.38) - forward * 0.65 + up * 0.075,
                 root + side * (sign * 1.52) - forward * 0.73 + up * 0.040]
        span = spine[-1] - spine[0]
        width_axis = span.cross(up)
        if width_axis.length < 1e-6:
            width_axis = forward
        normal_axis = width_axis.cross(span)
        if normal_axis.length < 1e-6:
            normal_axis = up
        blade(bm, weights, uvs, spine,
              [0.120, 0.330, 0.430, 0.420, 0.315, 0.135],
              [0.012, 0.016, 0.015, 0.013, 0.010, 0.006],
              width_axis, normal_axis,
              [upper, upper, upper, lower, lower, lower], "membrane", 8,
              (0.54, 0.98))


BUILDERS = {
    "giant-ant-soldier":
        lambda bm, weights, uvs, rig:
            giant_ant(bm, weights, uvs, rig, "giant-ant-soldier"),
    "giant-ant-worker":
        lambda bm, weights, uvs, rig:
            giant_ant(bm, weights, uvs, rig, "giant-ant-worker"),
    "fire-beetle": fire_beetle,
}


def chain_usage(kind, used):
    """Classify what the finished mesh actually weighted to each leg chain.

    This is the offline half of the owner's "six visible legs, no phantom
    eighth" requirement: the review sheet shows what a player would see, and
    this shows that nothing is attached to the chains that must stay empty.
    """
    observed = {}
    for index in ("0", "1", "2", "3"):
        kinds = set()
        for prefix in ("L", "R"):
            upper, lower, foot = leg_chain(prefix, index)
            present = tuple(name in used for name in (upper, lower, foot))
            if present == (False, False, False):
                kinds.add("empty")
            elif present == (True, True, True):
                kinds.add("full")
            elif present == (True, True, False):
                kinds.add("upper-and-lower")
            else:
                kinds.add("partial")
        if len(kinds) != 1:
            raise SystemExit("leg chain " + index + " differs left to right")
        observed[index] = kinds.pop()
    if observed != EXPECTED_CHAINS[kind]:
        raise SystemExit("leg chain usage for " + kind + " is " +
                         json.dumps(observed, sort_keys=True) + ", expected " +
                         json.dumps(EXPECTED_CHAINS[kind], sort_keys=True))
    return observed


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
            if name not in groups:
                groups[name] = obj.vertex_groups.new(name=name)
    for index, entries in indexed.items():
        total = sum(value for _, value in entries)
        if total <= 0 or len(entries) > 4:
            raise SystemExit("invalid influence row at vertex " + str(index))
        for name, value in entries:
            groups[name].add([index], value / total, "REPLACE")
    chains = chain_usage(args.kind, set(groups))
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
              "legChainUsage": chains,
              "visibleLegs": 2 * sum(1 for value in chains.values()
                                     if value == "full"),
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
    # Shipped, not merely reported: the runtime's allowed-bone list refuses a
    # mesh that binds the donor's fourth foot, and this lets everything
    # downstream of the build check the same thing without the private report.
    payload["legChainUsage"] = chains
    payload["visibleLegs"] = report["visibleLegs"]
    with Path(args.mesh_data).open("w", encoding="utf-8", newline="\n") as out:
        out.write(json.dumps(payload, indent=1) + "\n")
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.export_scene.fbx(filepath=args.fbx_out, use_selection=True,
                             add_leaf_bones=False, bake_anim=False,
                             path_mode="COPY",
                             apply_scale_options="FBX_SCALE_ALL",
                             object_types={"ARMATURE", "MESH"})
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[sprint14-insect] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
