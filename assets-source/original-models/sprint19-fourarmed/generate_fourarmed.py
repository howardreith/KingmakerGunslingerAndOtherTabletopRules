#!/usr/bin/env python3
"""Original Girallon and Xill bodies, authored on the reused census rig.

Both bodies are project-owned geometry. The only game-sourced input is the
donor bind-pose skeleton: bone names, bind positions and parent links, read by
the Sprint 18 primate_capture. No native vertex, index, texture pixel, shader
or animation curve is read, and none is redistributed.

The two creatures share one rig and one generator and are separated by
geometry rather than by rig. The Girallon is a white four-armed ape: an
upright trunk, four long heavy arms and clawed hands on all four. The Xill is
a chitinous four-armed outsider: a segmented thorax, four thin jointed arms,
digitigrade legs and a mandibled wedge of a head. Nothing is shared between
their silhouettes.

FOUR ARMS ON A TWO-ARMED RIG. Both creatures print four arms and the donor has
two. No four-arm rig is built and no bone is invented. Each side's two arms
are skinned to that side's single existing driver chain, with the lower arm's
geometry offset down and out from the upper arm's, so both right arms swing
together on the donor's right-arm animation and both left arms on its left.
For a creature whose printed routine is four claws striking one target in one
sequence this reads correctly, and the lower arms cannot be posed
independently of the upper. That is recorded here, in the shipped mesh, in the
runtime contract and in the release notes, under
FOUR_ARMS_SHARE_TWO_DRIVER_CHAINS.

What this file deliberately does NOT do:

- It authors no animation. The donor rig walks, stands and strikes on its own
  clips, which belong to a large upright biped.
- It builds no retargeting, limb or creature framework. Every number below is
  a measured offset from one named bone of one named donor.
- It touches nothing in the game. It reads one private capture file and writes
  two files into a private output directory.
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
sys.path.insert(0, str(HERE.parents[1] / "sprint18-primates"))
sys.path.insert(0, str(HERE.parent))
import generate_pteranodon as shared
import generate_primates as primates
import primate_capture as capture
import primate_regions as regions

# The exporter, the winding convention and the eight-region atlas are the
# Sprint 18 ones, reused rather than copied. They are genuinely the same
# thing: the same rig, the same bind frame and the same triangle order, which
# is why the shipped meshes declare the Sprint 18 winding marker.
WINDING = primates.WINDING
SPACE = primates.SPACE
PRIMATE_ATLAS = regions.PRIMATE_ATLAS
KINDS = ("girallon", "xill")

sweep = primates.sweep
ellipsoid = primates.ellipsoid
add_claw = primates.add_claw
component_volumes = primates.component_volumes


def at(rig, name, offset=None):
    """A donor bone's bind head, optionally displaced.

    The displacement is the whole four-arm mechanism: the lower arm is built
    around points moved down and out from the upper arm's bones, and then
    weighted to those same bones. Nothing about the rig changes.
    """
    point = Vector(primates.capture_head(rig, name))
    return point if offset is None else point + offset


def profile(kind):
    """The two silhouettes, as explicit numbers rather than a scale factor."""
    if kind not in KINDS:
        raise SystemExit("unsupported creature " + str(kind))
    girallon = kind == "girallon"
    return dict(
        kind=kind,
        girallon=girallon,
        # The Girallon is Large and the Xill Medium, which their printed
        # entries say. The engine scales the view, so the geometry differs in
        # build rather than in size: the ape is thick and the outsider thin.
        printedSize="Large" if girallon else "Medium",
        bulk=1.06 if girallon else 0.72,
        chest=1.10 if girallon else 0.86,
        # How far the lower arm pair is carried below and outboard of the
        # upper. Measured so the two hands on a side never interpenetrate at
        # the bind pose and the four read as four from the party camera.
        lowerDrop=Vector((0.0, -0.92, 0.0)) if girallon
        else Vector((0.0, -0.78, 0.0)),
        lowerSpread=0.34 if girallon else 0.26,
        lowerForward=-0.22 if girallon else -0.30,
        lowerScale=0.88 if girallon else 0.90,
        # Shortened from the first pass, where four claws on each of four
        # hands read as a bundle of spikes rather than as hands. They are
        # still geometry and still visible, because both creatures print
        # claw attacks.
        claw=0.19 if girallon else 0.15,
        clawRadius=0.060 if girallon else 0.038,
        clawed=True,
        crest=0.24 if girallon else 0.0,
        collar=0.26 if girallon else 0.0,
        canine=0.24 if girallon else 0.0,
        brow=1.24 if girallon else 0.9,
        mandible=0.0 if girallon else 0.46,
        segments=0 if girallon else 4,
    )


def lower_offset(shape, side):
    """Where this side's lower arm is carried, relative to its upper."""
    lateral = shape["lowerSpread"] * (1 if side == "L" else -1)
    return shape["lowerDrop"] + Vector((lateral, 0.0, shape["lowerForward"]))


# --- bodies ---------------------------------------------------------------

def build_trunk(bm, weights, uvs, rig, shape):
    """An upright trunk for both, built from different masses.

    The Girallon's is an ape's: a deep chest over a drawn-in waist. The
    Xill's is an insect's: a tall thorax over a narrow segmented abdomen.
    """
    bulk, chest = shape["bulk"], shape["chest"]
    pelvis = at(rig, "Pelvis")
    stomach = at(rig, "Stomach_01")
    spine = [at(rig, "Spine_0%d" % index) for index in (1, 2, 3)]
    neck = at(rig, "Neck_01")
    rows = [
        (pelvis + Vector((0, -0.10, 0)), 0.78 * bulk, 0.62 * bulk,
         [("Pelvis", 1)]),
        (pelvis, 0.80 * bulk, 0.64 * bulk, [("Pelvis", 1)]),
        (stomach, 0.74 * bulk * (1.0 if shape["girallon"] else 0.84),
         0.60 * bulk, [("Stomach_01", 0.7), ("Pelvis", 0.3)]),
        (spine[0], 0.76 * bulk, 0.62 * bulk,
         [("Spine_01", 0.8), ("Stomach_01", 0.2)]),
        (spine[1], 0.88 * bulk * chest, 0.66 * bulk,
         [("Spine_02", 0.85), ("Spine_01", 0.15)]),
        (spine[2], 0.92 * bulk * chest, 0.64 * bulk,
         [("Spine_03", 0.85), ("Spine_02", 0.15)]),
        (neck, 0.40 * bulk, 0.36 * bulk,
         [("Neck_01", 0.7), ("Spine_03", 0.3)]),
    ]
    sweep(bm, weights, uvs, rows, "torso", 16)
    # The Xill's segmented abdomen, as rings of its own rather than a tint.
    for index in range(shape["segments"]):
        height = pelvis.lerp(spine[0], 0.18 + index * 0.24)
        ellipsoid(bm, weights, uvs, height,
                  (0.80 * bulk - index * 0.03, 0.08 * bulk,
                   0.64 * bulk - index * 0.02),
                  [("Stomach_01", 0.6), ("Pelvis", 0.4)], "torso", 14, 8)
    # The Girallon's shoulder mantle, which is the only thing that gives a
    # near-white creature a silhouette.
    if shape["collar"]:
        for side in ("L", "R"):
            root = at(rig, side + "_Clavicle_01")
            ellipsoid(bm, weights, uvs,
                      root + Vector((0, 0.10, 0.06)),
                      (0.46 * bulk, shape["collar"], 0.40 * bulk),
                      [(side + "_Clavicle_01", 0.6), ("Spine_03", 0.4)],
                      "mane", 12, 8)


def build_head(bm, weights, uvs, rig, shape):
    """An ape's skull for one, an insect's wedge for the other."""
    bulk = shape["bulk"]
    head = at(rig, "Head")
    jaw = at(rig, "Jaw_01")
    forward = Vector((0, 0, -1))
    if shape["girallon"]:
        ellipsoid(bm, weights, uvs, head,
                  (0.60 * bulk, 0.58 * bulk, 0.62 * bulk),
                  [("Head", 1)], "face", 18, 14)
        # A low sagittal crest and the heavy brow the eyes sit under.
        ellipsoid(bm, weights, uvs, head + Vector((0, 0.48 * bulk, 0.04)),
                  (0.16 * bulk, shape["crest"], 0.44 * bulk),
                  [("Head", 1)], "mane", 10, 8)
        ellipsoid(bm, weights, uvs,
                  head + forward * 0.42 * bulk + Vector((0, 0.18, 0)),
                  (0.56 * bulk * shape["brow"] * 0.5, 0.14 * bulk, 0.20 * bulk),
                  [("Head", 1)], "face", 12, 8)
        # A prognathic muzzle carried on the jaw bone, so an open mouth moves.
        # The muzzle is facial hide and belongs in the face region: the first
        # pass put it in the mouth region, which is painted half gum and half
        # tooth, and the creature wore a pale card across its face.
        sweep(bm, weights, uvs, [
            (head + forward * 0.30 * bulk, 0.40 * bulk, 0.34 * bulk,
             [("Head", 1)]),
            (jaw + forward * 0.30 * bulk, 0.34 * bulk, 0.28 * bulk,
             [("Jaw_01", 0.6), ("Head", 0.4)]),
            (jaw + forward * 0.66 * bulk, 0.26 * bulk, 0.20 * bulk,
             [("Jaw_01", 1)]),
        ], "face", 12)
        # The mouth itself, which is the only thing that should carry gum
        # and tooth colour.
        ellipsoid(bm, weights, uvs, jaw + forward * 0.60 * bulk,
                  (0.17 * bulk, 0.06 * bulk, 0.08 * bulk),
                  [("Jaw_01", 1)], "mouth", 10, 8)
        for side in ("L", "R"):
            mirror = 1 if side == "L" else -1
            ellipsoid(bm, weights, uvs,
                      head + Vector((mirror * 0.22 * bulk, 0.10,
                                     -0.56 * bulk)),
                      (0.11, 0.10, 0.09), [("Head", 1)], "eye", 10, 8)
            # Canines: a girallon is a predator and its mouth says so.
            tip = jaw + forward * 0.66 * bulk + Vector(
                (mirror * 0.14 * bulk, -0.04, 0))
            sweep(bm, weights, uvs, [
                (tip, 0.045, 0.040, [("Jaw_01", 1)]),
                (tip + Vector((0, -shape["canine"], -0.04)), 0.018, 0.014,
                 [("Jaw_01", 1)]),
            ], "nail", 6)
            ellipsoid(bm, weights, uvs,
                      at(rig, side + "_Ear_01"), (0.08, 0.16, 0.05),
                      [(side + "_Ear_01", 1)], "face", 8, 8)
            ellipsoid(bm, weights, uvs,
                      at(rig, side + "_Eyebrow_01"), (0.13, 0.05, 0.07),
                      [(side + "_Eyebrow_01", 1)], "mane", 8, 6)
            ellipsoid(bm, weights, uvs,
                      at(rig, side + "_Up_lip_01"), (0.10, 0.05, 0.07),
                      [(side + "_Up_lip_01", 1)], "mouth", 8, 6)
    else:
        # A forward wedge, widest at the eyes and narrowing to the maw.
        sweep(bm, weights, uvs, [
            (head + Vector((0, 0.22, 0.26)), 0.34 * bulk, 0.30 * bulk,
             [("Head", 1)]),
            (head, 0.50 * bulk, 0.42 * bulk, [("Head", 1)]),
            (head + forward * 0.42 * bulk, 0.44 * bulk, 0.34 * bulk,
             [("Head", 1)]),
            (jaw + forward * 0.56 * bulk, 0.26 * bulk, 0.20 * bulk,
             [("Jaw_01", 0.7), ("Head", 0.3)]),
        ], "face", 14)
        for side in ("L", "R"):
            mirror = 1 if side == "L" else -1
            # Four compound eyes: a large pair forward and a small pair
            # above, which is most of what says outsider and not animal.
            ellipsoid(bm, weights, uvs,
                      head + Vector((mirror * 0.24 * bulk, 0.02,
                                     -0.50 * bulk)),
                      (0.13, 0.11, 0.10), [("Head", 1)], "eye", 10, 8)
            ellipsoid(bm, weights, uvs,
                      head + Vector((mirror * 0.32 * bulk, 0.24,
                                     -0.30 * bulk)),
                      (0.085, 0.075, 0.07), [("Head", 1)], "eye", 8, 8)
            # Mandibles: the printed bite, so they are geometry.
            root = jaw + forward * 0.48 * bulk + Vector(
                (mirror * 0.20 * bulk, 0.02, 0))
            sweep(bm, weights, uvs, [
                (root, 0.055, 0.048, [("Jaw_01", 1)]),
                (root + Vector((-mirror * 0.10, -0.12, -shape["mandible"] * 0.6)),
                 0.038, 0.030, [("Jaw_01", 1)]),
                (root + Vector((-mirror * 0.17, -0.22, -shape["mandible"])),
                 0.016, 0.012, [("Jaw_01", 1)]),
            ], "nail", 6)
            ellipsoid(bm, weights, uvs,
                      at(rig, side + "_Ear_01"), (0.05, 0.09, 0.04),
                      [(side + "_Ear_01", 1)], "face", 8, 6)
            ellipsoid(bm, weights, uvs,
                      at(rig, side + "_Eyebrow_01"), (0.09, 0.04, 0.05),
                      [(side + "_Eyebrow_01", 1)], "face", 8, 6)
            ellipsoid(bm, weights, uvs,
                      at(rig, side + "_Up_lip_01"), (0.07, 0.04, 0.05),
                      [(side + "_Up_lip_01", 1)], "mouth", 8, 6)
        ellipsoid(bm, weights, uvs, jaw + forward * 0.60 * bulk,
                  (0.13, 0.09, 0.08), [("Jaw_01", 1)], "mouth", 10, 8)


def build_arm(bm, weights, uvs, rig, shape, side, lower):
    """One arm, skinned to this side's single driver chain.

    When `lower` is true every point is displaced by this side's lower-arm
    offset and the limb is built slightly thinner, but the bone weights are
    identical to the upper arm's. That is the whole of
    FOUR_ARMS_SHARE_TWO_DRIVER_CHAINS: two arms, one chain, no new bone.
    """
    bulk = shape["bulk"] * (shape["lowerScale"] if lower else 1.0)
    offset = lower_offset(shape, side) if lower else None
    girallon = shape["girallon"]

    def point(name):
        return at(rig, side + "_" + name, offset)

    clavicle, upper = point("Clavicle_01"), point("Up_Arm_01")
    elbow, twist, wrist = (point("Forearm_01"), point("Forearm_02"),
                           point("Hand_01"))
    # An ape's arm is thick and even; an outsider's is thin with a hard
    # elbow. The lower pair is the same shape at a smaller scale.
    if girallon:
        rows = [(clavicle, 0.40, 0.40, [(side + "_Clavicle_01", 1)]),
                (upper, 0.38, 0.38, [(side + "_Up_Arm_01", 1)]),
                (upper.lerp(elbow, 0.5), 0.34, 0.34,
                 [(side + "_Up_Arm_01", 0.75), (side + "_Forearm_01", 0.25)]),
                (elbow, 0.36, 0.36, [(side + "_Forearm_01", 1)]),
                (twist, 0.30, 0.29,
                 [(side + "_Forearm_02", 0.7), (side + "_Forearm_01", 0.3)]),
                (wrist, 0.23, 0.21, [(side + "_Hand_01", 1)])]
    else:
        rows = [(clavicle, 0.26, 0.26, [(side + "_Clavicle_01", 1)]),
                (upper, 0.23, 0.23, [(side + "_Up_Arm_01", 1)]),
                (upper.lerp(elbow, 0.5), 0.18, 0.18,
                 [(side + "_Up_Arm_01", 0.75), (side + "_Forearm_01", 0.25)]),
                (elbow, 0.22, 0.22, [(side + "_Forearm_01", 1)]),
                (twist, 0.16, 0.15,
                 [(side + "_Forearm_02", 0.7), (side + "_Forearm_01", 0.3)]),
                (wrist, 0.13, 0.12, [(side + "_Hand_01", 1)])]
    sweep(bm, weights, uvs,
          [(where, a * bulk, b * bulk, bones) for where, a, b, bones in rows],
          "limbs", 12)

    digits = ("Fore_Finger", "Midle_Finger", "Little_Finger")
    knuckles = [point(name + "_01") for name in digits]
    knuckle_line = sum(knuckles, Vector()) / len(knuckles)
    palm = (0.27, 0.15) if girallon else (0.17, 0.09)
    sweep(bm, weights, uvs, [
        (wrist, palm[0] * 0.85 * bulk, palm[1] * bulk,
         [(side + "_Hand_01", 1)]),
        (wrist.lerp(knuckle_line, 0.55), palm[0] * bulk, palm[1] * bulk,
         [(side + "_Hand_01", 1)]),
        (knuckle_line, palm[0] * 1.02 * bulk, palm[1] * 1.05 * bulk,
         [(side + "_Hand_01", 1)]),
    ], "hands", 12)

    width = 1.0 if girallon else 0.62
    for name, taper in zip(digits, (1.0, 1.04, 0.88)):
        bones = [side + "_" + name + "_%02d" % index for index in (1, 2, 3)]
        points = [point(name + "_%02d" % index) for index in (1, 2, 3)]
        tip = points[2] + (points[2] - points[1]).normalized() * 0.16
        scale = taper * bulk * width
        sweep(bm, weights, uvs, [
            (points[0], 0.085 * scale, 0.080 * scale, [(bones[0], 1)]),
            (points[1], 0.075 * scale, 0.072 * scale,
             [(bones[1], 0.75), (bones[0], 0.25)]),
            (points[2], 0.066 * scale, 0.062 * scale,
             [(bones[2], 0.75), (bones[1], 0.25)]),
            (tip, 0.043 * scale, 0.041 * scale, [(bones[2], 1)]),
        ], "hands", 8)
        add_claw(bm, weights, uvs, shape, tip,
                 (tip - points[2]).normalized(), bones[2])

    thumb = [side + "_Thumb_%02d" % index for index in (1, 2, 3)]
    points = [point("Thumb_%02d" % index) for index in (1, 2, 3)]
    tip = points[2] + (points[2] - points[1]).normalized() * 0.12
    scale = bulk * width
    sweep(bm, weights, uvs, [
        (points[0], 0.100 * scale, 0.095 * scale, [(thumb[0], 1)]),
        (points[1], 0.088 * scale, 0.084 * scale,
         [(thumb[1], 0.75), (thumb[0], 0.25)]),
        (points[2], 0.073 * scale, 0.069 * scale,
         [(thumb[2], 0.75), (thumb[1], 0.25)]),
        (tip, 0.048 * scale, 0.046 * scale, [(thumb[2], 1)]),
    ], "hands", 8)
    add_claw(bm, weights, uvs, shape, tip,
             (tip - points[2]).normalized(), thumb[2])


def build_leg(bm, weights, uvs, rig, shape, side):
    """A standing leg for the ape, a digitigrade one for the outsider."""
    bulk = shape["bulk"]
    hip, knee = at(rig, side + "_UpLeg_01"), at(rig, side + "_Leg_01")
    ankle, toe = at(rig, side + "_Foot_01"), at(rig, side + "_Foot_Toe_01")
    thickness = 1.0 if shape["girallon"] else 0.68
    sweep(bm, weights, uvs, [
        (hip + Vector((0, 0.14, 0)), 0.52 * bulk * thickness,
         0.50 * bulk * thickness, [(side + "_UpLeg_01", 1)]),
        (hip.lerp(knee, 0.5), 0.43 * bulk * thickness,
         0.43 * bulk * thickness,
         [(side + "_UpLeg_01", 0.75), (side + "_Leg_01", 0.25)]),
        (knee, 0.35 * bulk * thickness, 0.35 * bulk * thickness,
         [(side + "_Leg_01", 1)]),
        (knee.lerp(ankle, 0.55), 0.30 * bulk * thickness,
         0.29 * bulk * thickness, [(side + "_Leg_01", 1)]),
        (ankle, 0.25 * bulk * thickness, 0.24 * bulk * thickness,
         [(side + "_Foot_01", 0.7), (side + "_Leg_01", 0.3)]),
    ], "limbs", 12)
    sole = Vector((ankle.x, 0.36, ankle.z + 0.16))
    sweep(bm, weights, uvs, [
        (sole, 0.25 * bulk * thickness, 0.24 * bulk * thickness,
         [(side + "_Foot_01", 1)]),
        (Vector((ankle.x, 0.30, ankle.z - 0.26)), 0.28 * bulk * thickness,
         0.25 * bulk * thickness,
         [(side + "_Foot_01", 0.6), (side + "_Foot_Toe_01", 0.4)]),
        (Vector((toe.x, 0.27, toe.z + 0.02)), 0.27 * bulk * thickness,
         0.22 * bulk * thickness, [(side + "_Foot_Toe_01", 1)]),
        (Vector((toe.x, 0.25, toe.z - 0.30)), 0.20 * bulk * thickness,
         0.16 * bulk * thickness, [(side + "_Foot_Toe_01", 1)]),
    ], "hands", 12)
    inner = -1 if side == "L" else 1
    sweep(bm, weights, uvs, [
        (Vector((toe.x + inner * 0.11, 0.25, toe.z + 0.12)),
         0.100 * bulk * thickness, 0.090 * bulk * thickness,
         [(side + "_Foot_Toe_01", 1)]),
        (Vector((toe.x + inner * 0.26, 0.23, toe.z - 0.08)),
         0.086 * bulk * thickness, 0.078 * bulk * thickness,
         [(side + "_Foot_Toe_01", 1)]),
        (Vector((toe.x + inner * 0.35, 0.22, toe.z - 0.26)),
         0.058 * bulk * thickness, 0.055 * bulk * thickness,
         [(side + "_Foot_Toe_01", 1)]),
    ], "hands", 8)


def build_body(bm, weights, uvs, rig, kind):
    shape = profile(kind)
    build_trunk(bm, weights, uvs, rig, shape)
    build_head(bm, weights, uvs, rig, shape)
    for side in ("L", "R"):
        build_arm(bm, weights, uvs, rig, shape, side, lower=False)
        build_arm(bm, weights, uvs, rig, shape, side, lower=True)
        build_leg(bm, weights, uvs, rig, shape, side)


# --- export ---------------------------------------------------------------

def anatomy(shape):
    """What the shipped body declares about itself.

    Six visible limbs: four arms and two legs. Four visible arms on exactly
    two animation driver chains, with the lower arms sharing the upper arms'
    bones. The runtime refuses a body that declares anything else, so a mesh
    cannot quietly claim a rig this project did not build.
    """
    return dict(visibleLimbs=6, visibleArms=4, armDriverChains=2,
                lowerArmsShareUpperArmDrivers=True, clawedHands=True,
                printedSize=shape["printedSize"],
                authoredLimitation="FOUR_ARMS_SHARE_TWO_DRIVER_CHAINS")


def export(args, rig_data, rig, rig_hash):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    armature = shared.build_armature(rig_data)
    armature.name = "FourArmedDonorRigPreview"
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
                  runtimeQualified=False, extent=extent,
                  albedo=shared.albedo_manifest(args.albedo))
    report.update(anatomy(shape))
    for path in (args.report, args.mesh_data, args.blend_out):
        Path(path).parent.mkdir(parents=True, exist_ok=True)
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    shared.write_mesh_data(args.mesh_data, obj, mesh, indexed, painted, report,
                           args.albedo)
    payload = json.loads(Path(args.mesh_data).read_text(encoding="utf-8"))
    payload.update(space=SPACE, triangleWinding=WINDING, creature=args.kind,
                   donorBlueprint=capture.BLUEPRINT, donorPrefab=capture.PREFAB,
                   donorRenderer=capture.BODY_RENDERER)
    payload.update(anatomy(shape))
    Path(args.mesh_data).write_text(json.dumps(payload, indent=1) + "\n",
                                    encoding="utf-8", newline="\n")
    bpy.ops.wm.save_as_mainfile(filepath=str(Path(args.blend_out).resolve()))
    print("[fourarmed-prototype] " + json.dumps(report, sort_keys=True))
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
