#!/usr/bin/env python3
"""Original Ape and Dire Ape bodies, authored on the census-chosen rig.

Both bodies are project-owned geometry. The only game-sourced input is the
donor bind-pose skeleton: bone names, bind positions and parent links, read
by primate_capture. No native vertex, index, texture pixel, shader or
animation curve is read, and none is redistributed.

The two creatures share one rig and one generator, and are separated by
geometry rather than by rig: the Ape is a heavy-chested gorilla with a
modest sagittal crest, short canines and nails; the Dire Ape is a
Gigantopithecus, bulkier through the chest and shoulders, with a tall
crest, a shaggy collar, long canines and heavy curved claws on both hands.

What this file deliberately does NOT do:

- It authors no animation. The donor rig walks, stands and strikes on its
  own clips, which are a large upright biped and not a knuckle-walker.
  That is an honest presentation deviation, recorded as such; a faithful
  primate gait would be a general animation system, which is out of scope.
- It builds no retargeting, limb or creature framework. Every number below
  is a measured offset from one named bone of one named donor.
- It touches nothing in the game. It reads one private capture file and
  writes two files into a private output directory.
"""
import argparse
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
sys.path.insert(0, str(HERE.parent))
import generate_pteranodon as shared
import primate_capture as capture
import primate_regions as regions

WINDING = "shared-exporter-sprint18"
SPACE = "donor renderer local; +X left, +Y up, -Z forward"
KINDS, PRIMATE_ATLAS = regions.KINDS, regions.PRIMATE_ATLAS

LATERAL, UP, FORWARD = Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, -1))
_region_uv = shared.region_uv


def inset_region_uv(region, u, v):
    """Keep every coordinate two percent inside its region.

    The albedo is clamped, and a coordinate exactly on a region boundary
    samples the neighbouring region's edge texel. Insetting costs nothing
    visible and removes the whole class of bleed.
    """
    return _region_uv(region, 0.02 + 0.96 * min(1.0, max(0.0, u)),
                      0.02 + 0.96 * min(1.0, max(0.0, v)))


shared.ATLAS.clear()
shared.ATLAS.update(PRIMATE_ATLAS)
shared.region_uv = inset_region_uv


# --- primitives -----------------------------------------------------------
# Closed, outward, single- or few-bone masses. Everything below is built
# from these two, so every piece of either body is a closed volume whose
# orientation the exporter can check rather than assume.

def ellipsoid(bm, weights, uvs, centre, radii, influences, region="torso",
              latitudes=7, segments=12, frame=(LATERAL, UP, FORWARD)):
    """One closed ellipsoid, weighted to a fixed influence row."""
    lateral, up, forward = (axis.normalized() for axis in frame)
    centre = Vector(centre)
    rows = normalize(influences)
    top = bm.verts.new(centre + up * radii[1])
    bottom = bm.verts.new(centre - up * radii[1])
    for vertex, v in ((top, 1.0), (bottom, 0.0)):
        weights[vertex] = rows
        uvs[vertex] = shared.region_uv(region, 0.5, v)
    rings = []
    for latitude in range(1, latitudes):
        polar = math.pi * latitude / latitudes
        across = math.sin(polar)
        ring = []
        for step in range(segments):
            angle = math.tau * step / segments
            vertex = bm.verts.new(centre +
                                  lateral * (math.cos(angle) * radii[0] * across) +
                                  forward * (math.sin(angle) * radii[2] * across) +
                                  up * (math.cos(polar) * radii[1]))
            weights[vertex] = rows
            uvs[vertex] = shared.region_uv(region, step / float(segments),
                                           1.0 - latitude / float(latitudes))
            ring.append(vertex)
        rings.append(ring)
    for step in range(segments):
        nxt = (step + 1) % segments
        bm.faces.new((top, rings[0][step], rings[0][nxt]))
        for latitude in range(len(rings) - 1):
            bm.faces.new((rings[latitude][step], rings[latitude + 1][step],
                          rings[latitude + 1][nxt], rings[latitude][nxt]))
        bm.faces.new((bottom, rings[-1][nxt], rings[-1][step]))


def sweep(bm, weights, uvs, rows, region="torso", sides=14, reference=LATERAL):
    """A closed elliptical tube through `rows`, capped at both ends.

    Each row is (centre, half_width, half_depth, influences). The section
    frame is transported along the path from the first row, so a limb that
    bends through a joint does not spin its cross-section, and the wide
    axis of the torso stays across the body rather than following the
    curve of the spine.
    """
    if len(rows) < 2:
        raise SystemExit("a sweep needs at least two rows")
    centres = [Vector(row[0]) for row in rows]
    lengths = [0.0]
    for index in range(1, len(centres)):
        lengths.append(lengths[-1] + (centres[index] - centres[index - 1]).length)
    total = lengths[-1]
    if total <= 1e-6:
        raise SystemExit("a sweep collapsed to a point")
    previous = Vector(reference)
    rings = []
    for index, (centre, width, depth, influences) in enumerate(
            (Vector(row[0]), row[1], row[2], row[3]) for row in rows):
        if index == 0:
            tangent = centres[1] - centres[0]
        elif index == len(centres) - 1:
            tangent = centres[-1] - centres[-2]
        else:
            tangent = centres[index + 1] - centres[index - 1]
        tangent = tangent.normalized()
        lateral = previous - tangent * previous.dot(tangent)
        if lateral.length < 0.05:
            lateral = tangent.cross(UP if abs(tangent.dot(UP)) < 0.9 else FORWARD)
        lateral.normalize()
        previous = lateral
        vertical = tangent.cross(lateral).normalized()
        shaped = normalize(influences)
        ring = []
        for step in range(sides):
            angle = math.tau * step / sides
            vertex = bm.verts.new(centre + lateral * (math.cos(angle) * width) +
                                  vertical * (math.sin(angle) * depth))
            weights[vertex] = shaped
            uvs[vertex] = shared.region_uv(region, lengths[index] / total,
                                           shared.fold(angle))
            ring.append(vertex)
        rings.append(ring)
    for index in range(len(rings) - 1):
        for step in range(sides):
            nxt = (step + 1) % sides
            bm.faces.new((rings[index][step], rings[index + 1][step],
                          rings[index + 1][nxt], rings[index][nxt]))
    for ring, row, u in ((rings[0], rows[0], 0.0), (rings[-1], rows[-1], 1.0)):
        cap = bm.verts.new(sum((vertex.co for vertex in ring), Vector()) / len(ring))
        weights[cap] = normalize(row[3])
        uvs[cap] = shared.region_uv(region, u, 0.5)
        for step in range(sides):
            nxt = (step + 1) % sides
            if ring is rings[0]:
                bm.faces.new((cap, ring[nxt], ring[step]))
            else:
                bm.faces.new((cap, ring[step], ring[nxt]))


def normalize(influences):
    """A clean influence row: named donor bones, positive, summing to one."""
    rows = [(name, float(value)) for name, value in influences if value > 0]
    if not rows or len(rows) > 3:
        raise SystemExit("an influence row must name one to three donor bones")
    total = sum(value for _, value in rows)
    if total <= 0:
        raise SystemExit("an influence row has no weight")
    return [(name, value / total) for name, value in rows]


# --- anatomy --------------------------------------------------------------

def profile(kind):
    """The two silhouettes, as explicit numbers rather than a scale factor.

    The Ape is a gorilla: enormous chest and shoulders, a pot belly, a low
    sagittal crest, short canines and nails. The Dire Ape is a
    Gigantopithecus: the same frame carried heavier everywhere, a tall
    crest, a shaggy collar, long canines and curved claws. Both are Large,
    which is what their printed entries say, so neither is rescaled.
    """
    if kind not in KINDS:
        raise SystemExit("unsupported creature " + str(kind))
    dire = kind == "dire-ape"
    return dict(
        bulk=1.14 if dire else 1.0,
        chest=1.10 if dire else 1.0,
        crest=0.30 if dire else 0.17,
        collar=0.30 if dire else 0.0,
        canine=0.21 if dire else 0.085,
        claw=0.26 if dire else 0.085,
        clawRadius=0.062 if dire else 0.05,
        brow=1.30 if dire else 1.0,
        clawed=dire,
    )


def mirrored(point, side):
    """The same measured offset on either side of the midline."""
    return Vector((point[0] * (1 if side == "L" else -1), point[1], point[2]))


def at(rig, name):
    return Vector(capture_head(rig, name))


def capture_head(rig, name):
    if name not in rig:
        raise SystemExit("the donor rig is missing " + name)
    return rig[name]["head"]


def build_torso(bm, weights, uvs, rig, shape):
    bulk, chest = shape["bulk"], shape["chest"]
    pelvis, spine1 = at(rig, "Pelvis"), at(rig, "Spine_01")
    spine2, spine3 = at(rig, "Spine_02"), at(rig, "Spine_03")
    neck, head = at(rig, "Neck_01"), at(rig, "Head")
    # One continuous trunk from the seat to the base of the skull. The
    # widest ring is at the shoulders, which is what makes the silhouette
    # read as an ape rather than as the upright giant the rig came from.
    sweep(bm, weights, uvs, [
        ((0, pelvis.y - 0.33, pelvis.z - 0.14), 0.60 * bulk, 0.54 * bulk,
         [("Pelvis", 1)]),
        ((0, pelvis.y, pelvis.z - 0.06), 0.74 * bulk, 0.72 * bulk, [("Pelvis", 1)]),
        ((0, spine1.y + 0.02, spine1.z - 0.08), 0.84 * bulk, 0.78 * bulk,
         [("Spine_01", 0.7), ("Pelvis", 0.3)]),
        ((0, spine2.y, spine2.z - 0.11), 0.95 * bulk * chest, 0.84 * bulk,
         [("Spine_02", 1)]),
        ((0, (spine2.y + spine3.y) / 2, spine2.z - 0.11),
         1.00 * bulk * chest, 0.85 * bulk,
         [("Spine_02", 0.5), ("Spine_03", 0.5)]),
        ((0, spine3.y, spine3.z - 0.08), 1.02 * bulk * chest, 0.82 * bulk,
         [("Spine_03", 1)]),
        ((0, spine3.y + 0.37, spine3.z - 0.15), 0.94 * bulk, 0.66 * bulk,
         [("Spine_03", 0.8), ("Neck_01", 0.2)]),
        ((0, neck.y - 0.27, neck.z + 0.13), 0.70 * bulk, 0.52 * bulk,
         [("Neck_01", 0.75), ("Spine_03", 0.25)]),
        ((0, neck.y - 0.02, neck.z - 0.07), 0.44 * bulk, 0.42 * bulk,
         [("Neck_01", 1)]),
        ((0, head.y - 0.03, head.z + 0.22), 0.40 * bulk, 0.40 * bulk,
         [("Head", 0.7), ("Neck_01", 0.3)]),
    ], "torso", 16)

    # The gut. Its own donor bone sits forward of the pelvis, which is
    # exactly where an ape carries it.
    stomach = at(rig, "Stomach_01")
    ellipsoid(bm, weights, uvs, (0, stomach.y - 0.06, stomach.z + 0.46),
              (0.64 * bulk, 0.50 * bulk, 0.38 * bulk), [("Stomach_01", 1)],
              "torso", 8, 14)
    # Chest and shoulder mass.
    for side in ("L", "R"):
        shoulder = at(rig, side + "_Up_Arm_01")
        ellipsoid(bm, weights, uvs,
                  mirrored((abs(shoulder.x) + 0.07, shoulder.y - 0.08, shoulder.z - 0.03), side),
                  (0.46 * bulk, 0.44 * bulk, 0.44 * bulk),
                  [(side + "_Up_Arm_01", 1)], "limbs", 7, 12)
    # The trapezius hump, and on the Dire Ape the shaggy collar over it.
    ellipsoid(bm, weights, uvs, (0, spine3.y + 0.50, spine3.z - 0.18),
              (0.78 * bulk, 0.33 * bulk, 0.44 * bulk),
              [("Spine_03", 0.6), ("Neck_01", 0.4)], "mane", 7, 14)
    if shape["collar"] > 0:
        # A shaggy ruff around the shoulders: enough overlapping tufts to
        # read as one mass of hair rather than as a necklace of spheres.
        for step in range(16):
            angle = math.tau * step / 16
            centre = Vector((math.cos(angle) * 0.88 * bulk,
                             neck.y - 0.30 + math.sin(angle) * 0.36,
                             neck.z + 0.12 - math.sin(angle) * 0.46))
            ellipsoid(bm, weights, uvs, centre,
                      (shape["collar"] * 0.85, shape["collar"] * 0.95,
                       shape["collar"] * 0.85),
                      [("Neck_01", 0.6), ("Spine_03", 0.4)], "mane", 6, 10)


def build_head(bm, weights, uvs, rig, shape):
    bulk, head, jaw = shape["bulk"], at(rig, "Head"), at(rig, "Jaw_01")
    # A big braincase on a short neck. An ape head is large against its
    # body, and the first version was not: it read as a lump on the
    # shoulders rather than as a head.
    ellipsoid(bm, weights, uvs, (0, head.y, head.z + 0.04),
              (0.50 * bulk, 0.50 * bulk, 0.50 * bulk), [("Head", 1)], "face", 9, 16)
    ellipsoid(bm, weights, uvs, (0, head.y - 0.16, head.z + 0.38),
              (0.44 * bulk, 0.42 * bulk, 0.40 * bulk), [("Head", 1)], "face", 7, 12)
    # The sagittal crest: the chewing-muscle ridge along the midline. Low
    # on the Ape, tall on the Dire Ape, and the clearest head difference
    # between them at party-camera distance.
    ellipsoid(bm, weights, uvs, (0, head.y + 0.30, head.z + 0.02),
              (0.17 * bulk, shape["crest"] + 0.14, 0.52 * bulk), [("Head", 1)],
              "mane", 7, 12)
    for side in ("L", "R"):
        # The two brow halves overlap across the midline on purpose: a
        # continuous shelf over both eyes is the single most ape-reading
        # feature of the face, and two separate pads are not that.
        brow = at(rig, side + "_Eyebrow_01")
        ellipsoid(bm, weights, uvs,
                  mirrored((0.17, brow.y - 0.02, brow.z + 0.27), side),
                  (0.30 * bulk, 0.09 * shape["brow"], 0.14 * bulk),
                  [(side + "_Eyebrow_01", 1)], "face", 6, 12)
        # Set deep under the shelf, so what shows is a dark eye in shadow
        # rather than a sphere stuck on the front of the skull.
        ellipsoid(bm, weights, uvs,
                  mirrored((0.195, brow.y - 0.155, brow.z + 0.205), side),
                  (0.088, 0.086, 0.082), [("Head", 1)], "eye", 6, 10)
        ear = at(rig, side + "_Ear_01")
        ellipsoid(bm, weights, uvs,
                  mirrored((abs(ear.x) - 0.09, ear.y - 0.07, ear.z + 0.02), side),
                  (0.050, 0.115 * bulk, 0.090 * bulk), [(side + "_Ear_01", 1)],
                  "face", 6, 10)
        # The muzzle halves ride their own lip bones, so the face keeps
        # whatever the donor clips do with them instead of going rigid.
        # They overlap at the midline and read as one broad low snout.
        lip = at(rig, side + "_Up_lip_01")
        ellipsoid(bm, weights, uvs,
                  mirrored((0.10, lip.y + 0.07, lip.z + 0.26), side),
                  (0.33 * bulk, 0.30 * bulk, 0.20 * bulk),
                  [(side + "_Up_lip_01", 1)], "face", 8, 14)
        # Canines. Hidden inside the lip on the Ape, which has no bite
        # attack at all; long and showing on the Dire Ape, which bites.
        ellipsoid(bm, weights, uvs,
                  mirrored((0.165, lip.y - 0.12 - shape["canine"] * 0.5, lip.z + 0.06), side),
                  (0.052, shape["canine"], 0.058), [(side + "_Up_lip_01", 1)],
                  "mouth", 6, 8)
    # The jaw is its own mass on its own bone: the Dire Ape bite has to
    # open a mouth rather than slide a solid head forward.
    sweep(bm, weights, uvs, [
        ((0, jaw.y - 0.02, jaw.z + 0.14), 0.33 * bulk, 0.21 * bulk, [("Jaw_01", 1)]),
        ((0, jaw.y - 0.10, jaw.z - 0.24), 0.29 * bulk, 0.18 * bulk, [("Jaw_01", 1)]),
        ((0, jaw.y - 0.10, jaw.z - 0.50), 0.21 * bulk, 0.14 * bulk, [("Jaw_01", 1)]),
    ], "face", 12)


def build_arm(bm, weights, uvs, rig, shape, side):
    bulk = shape["bulk"]
    clavicle = at(rig, side + "_Clavicle_01")
    upper = at(rig, side + "_Up_Arm_01")
    elbow = at(rig, side + "_Forearm_01")
    twist = at(rig, side + "_Forearm_02")
    wrist = at(rig, side + "_Hand_01")
    # Long, heavy arms, with the forearm kept thick well past the elbow.
    # The rig already hangs the hands lower than the knees, which is why
    # the census chose it; the geometry does not have to stretch to it.
    sweep(bm, weights, uvs, [
        (clavicle, 0.44 * bulk, 0.44 * bulk, [(side + "_Clavicle_01", 1)]),
        (upper, 0.42 * bulk, 0.42 * bulk, [(side + "_Up_Arm_01", 1)]),
        (upper.lerp(elbow, 0.5), 0.38 * bulk, 0.38 * bulk,
         [(side + "_Up_Arm_01", 0.75), (side + "_Forearm_01", 0.25)]),
        (elbow, 0.40 * bulk, 0.40 * bulk, [(side + "_Forearm_01", 1)]),
        (twist, 0.34 * bulk, 0.33 * bulk,
         [(side + "_Forearm_02", 0.7), (side + "_Forearm_01", 0.3)]),
        (wrist, 0.26 * bulk, 0.24 * bulk, [(side + "_Hand_01", 1)]),
    ], "limbs", 12)

    digits = ("Fore_Finger", "Midle_Finger", "Little_Finger")
    knuckles = [at(rig, side + "_" + name + "_01") for name in digits]
    knuckle_line = sum(knuckles, Vector()) / len(knuckles)
    # A long ape palm, wide across the knuckles and shallow front to back.
    sweep(bm, weights, uvs, [
        (wrist, 0.24 * bulk, 0.14 * bulk, [(side + "_Hand_01", 1)]),
        (wrist.lerp(knuckle_line, 0.55), 0.29 * bulk, 0.15 * bulk,
         [(side + "_Hand_01", 1)]),
        (knuckle_line, 0.30 * bulk, 0.16 * bulk, [(side + "_Hand_01", 1)]),
    ], "hands", 12)

    for name, taper in zip(digits, (1.0, 1.04, 0.88)):
        bones = [side + "_" + name + "_%02d" % index for index in (1, 2, 3)]
        points = [at(rig, bone) for bone in bones]
        tip = points[2] + (points[2] - points[1]).normalized() * 0.16
        sweep(bm, weights, uvs, [
            (points[0], 0.088 * taper * bulk, 0.082 * taper * bulk, [(bones[0], 1)]),
            (points[1], 0.078 * taper * bulk, 0.074 * taper * bulk,
             [(bones[1], 0.75), (bones[0], 0.25)]),
            (points[2], 0.068 * taper * bulk, 0.064 * taper * bulk,
             [(bones[2], 0.75), (bones[1], 0.25)]),
            (tip, 0.044 * taper * bulk, 0.042 * taper * bulk, [(bones[2], 1)]),
        ], "hands", 8)
        add_claw(bm, weights, uvs, shape, tip, (tip - points[2]).normalized(), bones[2])
        # The knuckle pad an ape walks on.
        ellipsoid(bm, weights, uvs, points[0], (0.10 * bulk, 0.095 * bulk, 0.10 * bulk),
                  [(bones[0], 1)], "hands", 6, 10)

    thumb = [side + "_Thumb_%02d" % index for index in (1, 2, 3)]
    points = [at(rig, bone) for bone in thumb]
    tip = points[2] + (points[2] - points[1]).normalized() * 0.12
    # Short and thick, the way an ape thumb is next to its fingers.
    sweep(bm, weights, uvs, [
        (points[0], 0.105 * bulk, 0.100 * bulk, [(thumb[0], 1)]),
        (points[1], 0.092 * bulk, 0.088 * bulk, [(thumb[1], 0.75), (thumb[0], 0.25)]),
        (points[2], 0.076 * bulk, 0.072 * bulk, [(thumb[2], 0.75), (thumb[1], 0.25)]),
        (tip, 0.050 * bulk, 0.048 * bulk, [(thumb[2], 1)]),
    ], "hands", 8)
    add_claw(bm, weights, uvs, shape, tip, (tip - points[2]).normalized(), thumb[2])


def add_claw(bm, weights, uvs, shape, root, direction, bone):
    """A nail on the Ape, a curved claw on the Dire Ape.

    The Dire Ape claws are the printed 1d4 primary claw attacks, so they
    are visible and long. The Ape has no claw attack and gets a nail.
    """
    length, radius = shape["claw"], shape["clawRadius"]
    curl = (direction + Vector((0, -0.55 if shape["clawed"] else -0.15, 0))).normalized()
    sweep(bm, weights, uvs, [
        (root - direction * 0.03, radius, radius * 0.72, [(bone, 1)]),
        (root + direction * length * 0.45, radius * 0.78, radius * 0.56, [(bone, 1)]),
        (root + direction * length * 0.45 + curl * length * 0.55,
         radius * 0.22, radius * 0.18, [(bone, 1)]),
    ], "nail", 8)


def build_leg(bm, weights, uvs, rig, shape, side):
    bulk = shape["bulk"]
    hip = at(rig, side + "_UpLeg_01")
    knee = at(rig, side + "_Leg_01")
    ankle = at(rig, side + "_Foot_01")
    toe = at(rig, side + "_Foot_Toe_01")
    # Short, thick legs under a long trunk: the ape proportion the printed
    # entry implies and the donor rig happens to carry.
    sweep(bm, weights, uvs, [
        (hip + Vector((0, 0.14, 0)), 0.54 * bulk, 0.52 * bulk, [(side + "_UpLeg_01", 1)]),
        (hip.lerp(knee, 0.5), 0.44 * bulk, 0.44 * bulk,
         [(side + "_UpLeg_01", 0.75), (side + "_Leg_01", 0.25)]),
        (knee, 0.36 * bulk, 0.36 * bulk, [(side + "_Leg_01", 1)]),
        (knee.lerp(ankle, 0.55), 0.31 * bulk, 0.30 * bulk, [(side + "_Leg_01", 1)]),
        (ankle, 0.26 * bulk, 0.25 * bulk,
         [(side + "_Foot_01", 0.7), (side + "_Leg_01", 0.3)]),
    ], "limbs", 12)
    sole = Vector((ankle.x, 0.36, ankle.z + 0.16))
    sweep(bm, weights, uvs, [
        (sole, 0.26 * bulk, 0.25 * bulk, [(side + "_Foot_01", 1)]),
        (Vector((ankle.x, 0.30, ankle.z - 0.26)), 0.29 * bulk, 0.26 * bulk,
         [(side + "_Foot_01", 0.6), (side + "_Foot_Toe_01", 0.4)]),
        (Vector((toe.x, 0.27, toe.z + 0.02)), 0.28 * bulk, 0.23 * bulk,
         [(side + "_Foot_Toe_01", 1)]),
        (Vector((toe.x, 0.25, toe.z - 0.30)), 0.21 * bulk, 0.17 * bulk,
         [(side + "_Foot_Toe_01", 1)]),
    ], "hands", 12)
    # The divergent big toe, on the inner edge of the foot.
    inner = -1 if side == "L" else 1
    sweep(bm, weights, uvs, [
        (Vector((toe.x + inner * 0.11, 0.25, toe.z + 0.12)), 0.105 * bulk,
         0.095 * bulk, [(side + "_Foot_Toe_01", 1)]),
        (Vector((toe.x + inner * 0.27, 0.23, toe.z - 0.08)), 0.090 * bulk,
         0.082 * bulk, [(side + "_Foot_Toe_01", 1)]),
        (Vector((toe.x + inner * 0.36, 0.22, toe.z - 0.26)), 0.062 * bulk,
         0.058 * bulk, [(side + "_Foot_Toe_01", 1)]),
    ], "hands", 8)


def build_body(bm, weights, uvs, rig, kind):
    shape = profile(kind)
    build_torso(bm, weights, uvs, rig, shape)
    build_head(bm, weights, uvs, rig, shape)
    for side in ("L", "R"):
        build_arm(bm, weights, uvs, rig, shape, side)
        build_leg(bm, weights, uvs, rig, shape, side)


# --- export ---------------------------------------------------------------

def component_volumes(mesh):
    """Signed volume of every connected shell, in Blender's own order.

    This is the check that replaces trusting a winding convention. Blender
    orients a closed shell outward, which in a right-handed frame makes its
    signed volume positive. The shipped exporter then reverses the winding
    once for the engine's left-handed frame, which is what every other
    original body in this repository ships. If any shell here comes out
    negative the mesh is inside out at the source, and no amount of
    reversing downstream would make both shells right.
    """
    parent = list(range(len(mesh.vertices)))

    def root(index):
        while parent[index] != index:
            parent[index] = parent[parent[index]]
            index = parent[index]
        return index

    for edge in mesh.edges:
        a, b = root(edge.vertices[0]), root(edge.vertices[1])
        if a != b:
            parent[a] = b
    totals = {}
    for polygon in mesh.polygons:
        loop = list(polygon.vertices)
        shell = root(loop[0])
        for corner in range(1, len(loop) - 1):
            a = mesh.vertices[loop[0]].co
            b = mesh.vertices[loop[corner]].co
            c = mesh.vertices[loop[corner + 1]].co
            totals[shell] = totals.get(shell, 0.0) + a.dot(b.cross(c)) / 6.0
    return totals


def export(args, rig_data, rig, rig_hash):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    armature = shared.build_armature(rig_data)
    armature.name = "PrimateDonorRigPreview"
    mesh = bpy.data.meshes.new(args.kind + "OriginalMesh")
    obj = bpy.data.objects.new(args.kind, mesh)
    bpy.context.collection.objects.link(obj)
    bm = bmesh.new()
    weights, uvs = {}, {}
    build_body(bm, weights, uvs, rig, args.kind)
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.normal_update()
    bm.to_mesh(mesh)
    indexed = {vertex.index: row for vertex, row in weights.items()}
    painted = {vertex.index: row for vertex, row in uvs.items()}
    bm.free()
    if len(indexed) != len(mesh.vertices) or len(painted) != len(mesh.vertices):
        raise SystemExit("an original vertex is unweighted or unpainted")

    volumes = component_volumes(mesh)
    inverted = sorted(value for value in volumes.values() if value <= 0)
    if inverted:
        raise SystemExit("%d original shells are inside out: %s"
                         % (len(inverted), inverted[:4]))

    layer = mesh.uv_layers.new(name="OriginalAlbedo")
    for loop in mesh.loops:
        layer.data[loop.index].uv = painted[loop.vertex_index]
    groups = {}
    for index, entries in indexed.items():
        for name, value in entries:
            if name not in capture.DRIVERS:
                raise SystemExit("unreviewed donor bone carries geometry: " + name)
            if name in capture.EXCLUDED:
                raise SystemExit("an excluded donor branch carries geometry: " + name)
            if name not in groups:
                groups[name] = obj.vertex_groups.new(name=name)
            groups[name].add([index], value, "REPLACE")
    unused = sorted(set(capture.DRIVERS) - set(groups))
    if unused:
        raise SystemExit("reviewed donor bones carry no geometry: " + ", ".join(unused))
    obj.modifiers.new(name="DonorRigPreview", type="ARMATURE").object = armature
    obj.parent = armature
    shared.attach_preview_material(obj, mesh, args.albedo)
    for material in mesh.materials:
        material.use_backface_culling = True
    for polygon in mesh.polygons:
        polygon.use_smooth = True

    shape = profile(args.kind)
    extent = {axis: [min(getattr(v.co, axis) for v in mesh.vertices),
                     max(getattr(v.co, axis) for v in mesh.vertices)]
              for axis in ("x", "y", "z")}
    report = dict(schemaVersion=1, creature=args.kind,
                  donorFamily="native-troll-guard", donorBlueprint=capture.BLUEPRINT,
                  donorPrefab=capture.PREFAB, donorRenderer=capture.BODY_RENDERER,
                  rigSha256=rig_hash, rigBoneCount=len(rig_data["bones"]),
                  vertices=len(mesh.vertices), polygons=len(mesh.polygons),
                  boneGroups=len(groups),
                  maxInfluencesPerVertex=max(len(row) for row in indexed.values()),
                  shells=len(volumes), smallestShellVolume=min(volumes.values()),
                  tailGeometry=False, tongueGeometry=False, jawSeparated=True,
                  visibleLimbs=4, clawedHands=shape["clawed"],
                  opposableThumbs=True, knuckleWalkAuthored=False,
                  runtimeQualified=False, extent=extent,
                  albedo=shared.albedo_manifest(args.albedo))
    for path in (args.report, args.mesh_data, args.blend_out):
        Path(path).parent.mkdir(parents=True, exist_ok=True)
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    shared.write_mesh_data(args.mesh_data, obj, mesh, indexed, painted, report,
                           args.albedo)
    payload = json.loads(Path(args.mesh_data).read_text(encoding="utf-8"))
    payload.update(space=SPACE, triangleWinding=WINDING, creature=args.kind,
                   donorBlueprint=capture.BLUEPRINT, donorPrefab=capture.PREFAB,
                   donorRenderer=capture.BODY_RENDERER, tailGeometry=False,
                   tongueGeometry=False, jawSeparated=True, visibleLimbs=4,
                   clawedHands=shape["clawed"], opposableThumbs=True,
                   knuckleWalkAuthored=False)
    Path(args.mesh_data).write_text(json.dumps(payload, indent=1) + "\n",
                                    encoding="utf-8", newline="\n")
    bpy.ops.wm.save_as_mainfile(filepath=str(Path(args.blend_out).resolve()))
    print("[primate-prototype] " + json.dumps(report, sort_keys=True))
    return report


def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", required=True, choices=KINDS)
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    return parser.parse_args(
        sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])


def main():
    args = parse_args()
    rig_data, rig, rig_hash = capture.load(args.capture)
    export(args, rig_data, rig, rig_hash)


if __name__ == "__main__":
    main()
