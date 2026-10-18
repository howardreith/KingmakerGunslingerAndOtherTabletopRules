"""Build the refined Hyena, Goblin Dog, Wolverine or Shadow Mastiff as one
continuous skinned body on the measured Wolf/Worg frame.

blender -b --factory-startup --python-exit-code 1 --python build_worg_frame.py -- \
    --kind hyena|goblin-dog|wolverine|shadow-mastiff \
    --capture <private sprint12-wolf-bind-rig.json | sprint12-worg-bind-rig.json> \
    --albedo assets/<sprint folder>/<kind>-albedo.png \
    --mesh-data assets/<sprint folder>/<kind>-mesh.json \
    --report <private report.json> --blend-out <private source.blend>

VIS-010 (Hyena), VIS-009 (Goblin Dog), VIS-014 (Wolverine) and VIS-033
(Shadow Mastiff). The Hyena rides the Wolf; the other three ride the Worg.
Both frames have the same 36 controls; each body binds the same reviewed 34
(every one but the two collar helpers). The Sprint 12 loader validates the
Hyena's and Goblin Dog's captures and the Sprint 13 loader the Wolverine's and
Shadow Mastiff's, each refusing a frame from the wrong donor or sprint.

Each is one continuous surface: a torso along the animal's own topline,
united and relaxed on a coarser union, joined by a neck to a skull and
muzzle, with legs along the measured bones. The lower jaw, ears, eyes, nose,
claws and teeth are closed rigid features rooted inside it.
"""
import argparse
import json
import math
from pathlib import Path
import sys

import bpy
import numpy as np
from mathutils import Vector

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE.parent / "continuous-bodies"))
sys.path.insert(0, str(HERE))
sys.path.insert(0, str(HERE.parent / "sprint13-creatures"))
import continuous_body as cb  # noqa: E402
import generate_sprint12_quadrupeds as sprint12  # noqa: E402
import generate_sprint13_creatures as sprint13  # noqa: E402

SPACE = "donor renderer local; +X left, +Y up, -Z forward"
SIDE = Vector((1.0, 0.0, 0.0))      # creature's left
UP = Vector((0.0, 1.0, 0.0))
FORWARD = Vector((0.0, 0.0, -1.0))
REGION = {"body": 0, "ear": 1, "eye": 2, "nose": 3, "claw": 4, "tooth": 5,
          "jaw": 6, "tail": 7}
BONES = ("Head", "L_Arm_Lower", "L_Arm_Upper", "L_Foot0", "L_Leg0_Lower",
         "L_Leg0_Lower2", "L_Leg0_Upper", "L_Palm", "R_Arm_Lower",
         "R_Arm_Upper", "R_Foot0", "R_Leg0_Lower", "R_Leg0_Lower2",
         "R_Leg0_Upper", "R_Palm", "Torso_Lower", "Torso_Upper", "ear_L",
         "ear_R", "front_paw__tip_R", "front_paw_tip_L", "hindpaw_tip_L",
         "hindpaw_tip_R", "jaw", "jaw_woo_down", "jaw_woo_up",
         "jaw_woo_up_add", "neck", "spine_0", "tail_01", "tail_02",
         "tail_03", "tail_04", "withers")
UPPER_LIMB = {"L_Arm_Upper", "R_Arm_Upper", "L_Leg0_Upper", "R_Leg0_Upper"}
TAIL = ("tail_01", "tail_02", "tail_03", "tail_04")


def ramp(value, start, width):
    t = max(0.0, min(1.0, (value - start) / width))
    return t * t * (3.0 - 2.0 * t)


def load_rig(capture, kind):
    if kind in ("hyena", "goblin-dog"):
        return sprint12.measured_rig(capture, kind)
    return sprint13.measured_rig(capture, kind)


def build(rig, armature, kind, params):
    at = lambda name: Vector(rig[name]["head"])
    hips, middle, chest = at("Torso_Lower"), at("spine_0"), at("Torso_Upper")
    ground = min(at(name).y for name in ("hindpaw_tip_L", "hindpaw_tip_R",
                                         "front_paw_tip_L",
                                         "front_paw__tip_R")) - 0.066
    torso = cb.Volume(params["torso_resolution"])
    volume = torso

    def point(y, z, x=0.0):
        return Vector((x, ground + y, z))

    def mass(centre, radii, axes=(SIDE, UP, FORWARD)):
        volume.ellipsoid(centre, axes, radii)

    # The torso along the animal's own topline (a capsule chain), with its
    # chest and haunch masses, united and relaxed on a coarser union.
    volume.sweep([point(y, z) for y, z, _ in params["dorsal"]],
                 [radius for _, _, radius in params["dorsal"]])
    for y, z, radii in params["masses"]:
        mass(point(y, z), radii)
    for sign in (-1.0, 1.0):
        shoulder = params["shoulder"]
        mass(point(shoulder[0], chest.z - 0.16, sign * shoulder[1]),
             shoulder[2])
        haunch = params["haunch"]
        mass(point(haunch[0], hips.z - 0.02, sign * haunch[1]), haunch[2])
    # The neck from the shoulders to behind the skull.
    head = at("Head") - UP * params["head_drop"]
    neck_root = point(params["neck_root"][0], params["neck_root"][1])
    volume.sweep([neck_root, neck_root.lerp(head + FORWARD * 0.04, 0.6),
                  head + FORWARD * 0.06], params["neck_radii"])
    trunk = torso.polygonise(kind.title() + "Torso")
    for vertex in trunk.data.vertices:
        vertex.co.x *= params["torso_width"]
    cb.relax(trunk, params["torso_smooth"])
    volume = cb.Volume(params["resolution"])
    volume.shell(trunk)

    # Skull and muzzle on the head axis, from the back of the skull to the
    # nose: a cranium, cheeks and a muzzle tapering to the nose.
    nose = at("jaw_woo_up") - UP * params["head_drop"] + \
        FORWARD * params["muzzle_extra"]
    axis = (nose - head).normalized()
    face_up = axis.cross(SIDE).normalized()
    if face_up.dot(UP) < 0:
        face_up = -face_up
    head_axes = (SIDE, face_up, axis)
    w = params["head_width"]
    length = (nose - head).length
    cranium = head + axis * 0.10 * length
    volume.ellipsoid(cranium + face_up * 0.02, head_axes,
                     (0.095 * w, 0.090 * w, 0.12 * length + 0.05))
    volume.ellipsoid(head + axis * 0.32 * length - face_up * 0.045,
                     head_axes, (0.085 * w, 0.070 * w, 0.20 * length))
    volume.sweep([head + axis * 0.30 * length,
                  head + axis * 0.70 * length - face_up * 0.01,
                  nose - axis * params["nose_radius"]],
                 [params["muzzle_radii"][0] * w, params["muzzle_radii"][1] * w,
                  params["nose_radius"]])
    for prefix in ("L", "R"):
        sign = 1.0 if prefix == "L" else -1.0
        upper, elbow = at(prefix + "_Arm_Upper"), at(prefix + "_Arm_Lower")
        wrist = at(prefix + "_Palm")
        front = [point(params["front_top"], upper.z + 0.02, upper.x * 0.70),
                 elbow, wrist,
                 point(0.035, wrist.z - 0.05, wrist.x)]
        hip, stifle = at(prefix + "_Leg0_Upper"), at(prefix + "_Leg0_Lower")
        hock, foot = at(prefix + "_Leg0_Lower2"), at(prefix + "_Foot0")
        hind = [point(params["hind_top"], hip.z + 0.02, hip.x * 0.70),
                stifle, hock, foot, point(0.035, foot.z - 0.05, foot.x)]
        volume.sweep(front, params["front_radii"])
        volume.sweep(hind, params["hind_radii"])
        for chain, radii in ((front, params["front_radii"]),
                             (hind, params["hind_radii"])):
            # A paw: a pad on the ground under the toes.
            pad = chain[-1]
            mass(Vector((pad.x, ground + 0.03, pad.z)),
                 (radii[-1] * 1.25, 0.032, radii[-1] * 1.6))
    if params["tail"]["mode"] == "bones":
        tail = [at(name) for name in TAIL]
        volume.sweep([point(params["tail"]["root"], hips.z + 0.04)] + tail,
                     params["tail"]["radii"])

    body = volume.polygonise(kind.title() + "Body")
    cb.finish_surface(body, params["triangles"],
                      smooth_iterations=params["smooth"])
    positions = [v.co.copy() for v in body.data.vertices]
    allowed = set(BONES)
    rows = cb.heat_weights(body, armature, allowed)
    rows = cb.clean_weights(rows, positions, armature, allowed)
    # The torso buries the long upper limb bones; above the belly line the
    # body follows the spine, so a leg swing does not drag the flank.
    line = ground + params["belly"]

    def damp(q):
        return 0.85 * ramp(q.y, line + 0.02, 0.18)

    for target, half in (("Torso_Lower", lambda q: q.z > middle.z + 0.10),
                         ("spine_0", lambda q: chest.z + 0.10 < q.z <=
                          middle.z + 0.10),
                         ("Torso_Upper", lambda q: q.z <= chest.z + 0.10)):
        rows = cb.blend_toward(
            rows, positions,
            lambda q, row, half=half: half(q) and any(
                name in UPPER_LIMB for name, _ in row),
            target, damp)
    rows = cb.smooth_weights(body, rows)

    def reach(q):
        along = (q - head).dot(axis)
        return along, ((q - head) - axis * along).length

    # Head owns the skull and muzzle; the upper lip and the nose take their
    # own controls, and the withers the top of the shoulders.
    def head_share(q):
        along, across = reach(q)
        return ramp(along, -0.10, 0.10) * (1.0 - ramp(across, 0.13 * w, 0.06))

    def lip_share(q):
        along, across = reach(q)
        below = -(q - head).dot(face_up)
        return 0.45 * ramp(along, 0.55 * length, 0.10) * ramp(below, 0.0, 0.03)

    def nose_share(q):
        return 0.55 * (1.0 - ramp((q - nose).length, 0.03, 0.06))

    withers = at("withers") - UP * params["withers_drop"]

    def withers_share(q):
        return 0.40 * (1.0 - ramp((q - withers).length, 0.06, 0.10))

    # A tail that follows the frame's tail ends at tail_04, a leaf the heat
    # solve leaves bare; it takes the last segment.
    tip_from, tip_to = at("tail_03"), at("tail_04")
    tip_span = (tip_to - tip_from).length

    def tail_tip_share(q):
        if params["tail"]["mode"] != "bones":
            return 0.0
        along = (q - tip_from).dot(tip_to - tip_from) / (tip_span * tip_span)
        return 0.85 * ramp(along, 0.25, 0.60)

    for target, share in (("Head", head_share), ("jaw_woo_up_add", lip_share),
                          ("jaw_woo_up", nose_share),
                          ("withers", withers_share),
                          ("tail_04", tail_tip_share)):
        rows = cb.blend_toward(rows, positions,
                               lambda q, row, share=share: share(q) > 0,
                               target, share)
    rows = cb.smooth_weights(body, rows, iterations=2)
    rows = cb.clean_weights(rows, positions, armature, allowed)
    parts = [(body, rows, REGION["body"])]

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), REGION[region]))

    # The lower jaw: a rigid closed strand from the hinge to the chin, on
    # jaw at the hinge grading to jaw_woo_down at the chin, so a bite opens
    # it as one piece.
    hinge = at("jaw") - UP * params["head_drop"]
    chin = nose - face_up * params["jaw_depth"] - axis * 0.05
    jaw = cb.strand("Jaw", [hinge - axis * 0.02, hinge.lerp(chin, 0.5), chin],
                    [r * w for r in params["jaw_radii"]], 12)
    jaw_rows = {}
    span = (chin - hinge).length
    for index, vertex in enumerate(jaw.data.vertices):
        t = max(0.0, min(1.0, (vertex.co - hinge).dot(chin - hinge) /
                         (span * span)))
        row = [("jaw", 1.0 - t), ("jaw_woo_down", t)]
        jaw_rows[index] = sorted([e for e in row if e[1] > 0.0],
                                 key=lambda e: (-e[1], e[0]))
    parts.append((jaw, jaw_rows, REGION["jaw"]))
    for side in (-1.0, 1.0):
        # Upper and lower canines.
        for root, down, size in ((nose - axis * 0.06 - face_up * 0.035, -1.0,
                                  params["fang"]),
                                 (chin + face_up * 0.02 - axis * 0.01, 1.0,
                                  params["fang"] * 0.7)):
            base = root + SIDE * side * 0.028 * w
            rigid(cb.horn("Fang", base, base + face_up * down * size,
                          0.010 * w, segments=8, rings=4),
                  [("Head" if down < 0 else "jaw_woo_down", 1.0)], "tooth")
        ear_bone = "ear_L" if side > 0 else "ear_R"
        ear_root = at(ear_bone) - UP * params["head_drop"]
        spec = params["ear"]
        rise = (UP * spec["rise"] + SIDE * side * spec["splay"] -
                FORWARD * spec["lean"]).normalized()
        facing = (FORWARD - rise * FORWARD.dot(rise)).normalized()
        rigid(cb.blob("Ear", ear_root + rise * spec["size"][1] * 0.8,
                      (rise.cross(facing), rise, facing), spec["size"], 14, 8),
              [(ear_bone, 1.0)], "ear")
        eye = cranium + axis * 0.10 * length + SIDE * side * 0.072 * w + \
            face_up * 0.035
        rigid(cb.blob("Eye", eye, (SIDE, axis, face_up),
                      (0.014 * params["eye"], 0.020 * params["eye"],
                       0.016 * params["eye"]), 10, 6),
              [("Head", 1.0)], "eye")
    rigid(cb.blob("Nose", nose - axis * 0.005, (SIDE, face_up, axis),
                  (0.030 * w, 0.020 * w, 0.020), 10, 6),
          [("jaw_woo_up", 1.0)], "nose")
    for prefix, tips in (("L", ("front_paw_tip_L", "hindpaw_tip_L")),
                         ("R", ("front_paw__tip_R", "hindpaw_tip_R"))):
        for tip_bone, paw_bone in zip(tips, (prefix + "_Palm",
                                             prefix + "_Foot0")):
            paw = at(paw_bone)
            for lateral in (-1.5, -0.5, 0.5, 1.5):
                root = Vector((paw.x + lateral * 0.017, ground + 0.03,
                               paw.z - 0.05 - 0.05))
                rigid(cb.horn("Claw", root, root + FORWARD * 0.035 -
                              UP * 0.025, 0.010, segments=8, rings=4),
                      [(tip_bone, 1.0)], "claw")
    if params["tail"]["mode"] == "hang":
        # A short tail hanging from the tailhead, weighted down the tail
        # chain by its length (the frame's tail is held out long).
        spec = params["tail"]
        top = point(spec["root"], hips.z + 0.06)
        hang = [top, top - FORWARD * 0.06 - UP * 0.03,
                top - FORWARD * 0.10 - UP * spec["drop"] * 0.5,
                top - FORWARD * 0.12 - UP * spec["drop"]]
        strand = cb.strand("Tail", hang, spec["radii"], 10)
        tail_rows = {}
        for index, vertex in enumerate(strand.data.vertices):
            t = max(0.0, min(1.0, (top.y - vertex.co.y) / spec["drop"])) * 2.999
            low, share = int(t), t - int(t)
            row = [(TAIL[low], 1.0 - share), (TAIL[low + 1], share)]
            tail_rows[index] = sorted([e for e in row if e[1] > 0.0],
                                      key=lambda e: (-e[1], e[0]))
        parts.append((strand, tail_rows, REGION["tail"]))
    animal, weights = cb.assemble(kind.title(), parts)
    cb.require_single_body(animal, REGION["body"])
    return animal, weights, {"ground": ground, "head": head, "nose": nose,
                             "axis": axis, "face_up": face_up,
                             "hips": hips, "chest": chest, "length": length}


def colour_field(kind, landmarks):
    ground = landmarks["ground"]
    palette = PALETTES[kind]

    def colour(p, n, region):
        up = np.clip(n[:, 1] * 0.5 + 0.5, 0, 1)[:, None]
        base = palette["belly"] + (palette["back"] - palette["belly"]) * up
        broad = cb.fbm(p, 1.0 / 0.25, 41)
        hair = cb.fbm(p * np.array([3.0, 0.8, 3.0]), 1.0 / 0.03, 42)
        base = base * (0.88 + 0.18 * broad[:, None]) * \
            (0.90 + 0.16 * hair[:, None])
        height = p[:, 1] - ground
        legs = np.clip((palette["leg_line"] - height) / 0.12, 0, 1)[:, None]
        base = base * (1 - legs) + palette["legs"] * legs * \
            (0.9 + 0.2 * broad[:, None])
        if palette["spots"] is not None:
            # The spotted hyena's dark spots, on the flanks and legs.
            first, second = cb.cells(p, 1.0 / 0.075, 43)
            spot = np.clip((0.34 - first) / 0.08, 0, 1) * \
                np.clip((p[:, 1] - ground - 0.08) / 0.05, 0, 1)
            base = base * (1 - spot[:, None]) + palette["spots"] * \
                spot[:, None]
        if palette.get("mane") is not None:
            # A short dark mane down the neck and the line of the back.
            ridge = np.exp(-(p[:, 0] / 0.035) ** 2) * \
                np.clip((n[:, 1] - 0.5) / 0.3, 0, 1)
            base = base * (1 - 0.7 * ridge[:, None]) + palette["mane"] * \
                0.7 * ridge[:, None]
        if palette["band"] is not None:
            # The wolverine's pale band from shoulder to hip along the flank.
            flank = np.clip(np.abs(n[:, 0]) * 1.5 - 0.3, 0, 1)
            band_y = ground + palette["band_height"]
            band = np.exp(-((p[:, 1] - band_y) / 0.065) ** 2) * flank * \
                np.clip((landmarks["hips"].z + 0.10 - p[:, 2]) / 0.1, 0, 1) * \
                np.clip((p[:, 2] - landmarks["chest"].z + 0.15) / 0.1, 0, 1)
            base = base * (1 - band[:, None]) + palette["band"] * band[:, None]
        if palette["sores"] is not None:
            # The goblin dog's mottled, mangy hide: dark patches and raw sores.
            patch = cb.fbm(p, 1.0 / 0.08, 44)
            dark = np.clip((patch - 0.55) / 0.10, 0, 1)[:, None]
            base = base * (1 - 0.45 * dark) + palette["patches"] * 0.45 * dark
            first, _ = cb.cells(p, 1.0 / 0.06, 45)
            sore = np.clip((0.16 - first) / 0.05, 0, 1)[:, None]
            base = base * (1 - sore) + palette["sores"] * sore
        # A darker muzzle.
        to_nose = np.linalg.norm(p - np.array(landmarks["nose"]), axis=1)
        mask = np.clip((0.16 - to_nose) / 0.08, 0, 1)[:, None]
        base = base * (1 - mask) + palette["muzzle"] * mask
        rgb = base
        rgb = np.where((region == REGION["jaw"])[:, None],
                       base * 0.9, rgb)
        rgb = np.where((region == REGION["tail"])[:, None],
                       palette["tail"] * (0.9 + 0.2 * hair[:, None]), rgb)
        rgb = np.where((region == REGION["ear"])[:, None], palette["ear"], rgb)
        rgb = np.where((region == REGION["eye"])[:, None], palette["eye"], rgb)
        rgb = np.where((region == REGION["nose"])[:, None],
                       np.array([0.05, 0.04, 0.04]), rgb)
        rgb = np.where((region == REGION["claw"])[:, None],
                       np.array([0.10, 0.09, 0.08]), rgb)
        rgb = np.where((region == REGION["tooth"])[:, None],
                       np.array([0.80, 0.76, 0.64]), rgb)
        return rgb
    return colour


PALETTES = {
    "hyena": {"back": np.array([0.50, 0.40, 0.27]),
              "belly": np.array([0.46, 0.38, 0.27]),
              "legs": np.array([0.40, 0.32, 0.22]), "leg_line": 0.16,
              "spots": np.array([0.16, 0.11, 0.08]), "band": None,
              "mane": np.array([0.18, 0.13, 0.09]),
              "sores": None, "muzzle": np.array([0.14, 0.11, 0.09]),
              "tail": np.array([0.12, 0.09, 0.07]),
              "ear": np.array([0.30, 0.23, 0.16]),
              "eye": np.array([0.06, 0.05, 0.04])},
    "goblin-dog": {"back": np.array([0.47, 0.38, 0.35]),
                   "belly": np.array([0.52, 0.42, 0.40]),
                   "legs": np.array([0.40, 0.31, 0.29]), "leg_line": 0.10,
                   "spots": None, "band": None,
                   "sores": np.array([0.52, 0.20, 0.17]),
                   "patches": np.array([0.24, 0.20, 0.20]),
                   "muzzle": np.array([0.36, 0.26, 0.25]),
                   "tail": np.array([0.44, 0.34, 0.32]),
                   "ear": np.array([0.50, 0.33, 0.31]),
                   "eye": np.array([0.55, 0.42, 0.08])},
    "wolverine": {"back": np.array([0.21, 0.15, 0.10]),
                  "belly": np.array([0.13, 0.09, 0.07]),
                  "legs": np.array([0.08, 0.06, 0.05]), "leg_line": 0.22,
                  "spots": None, "band": np.array([0.62, 0.48, 0.31]),
                  "band_height": 0.60, "sores": None,
                  "muzzle": np.array([0.08, 0.06, 0.05]),
                  "tail": np.array([0.10, 0.08, 0.06]),
                  "ear": np.array([0.12, 0.09, 0.07]),
                  "eye": np.array([0.04, 0.03, 0.03])},
    "shadow-mastiff": {"back": np.array([0.08, 0.08, 0.10]),
                       "belly": np.array([0.05, 0.05, 0.06]),
                       "legs": np.array([0.05, 0.05, 0.06]), "leg_line": 0.12,
                       "spots": None, "band": None, "sores": None,
                       "muzzle": np.array([0.04, 0.04, 0.05]),
                       "tail": np.array([0.07, 0.07, 0.09]),
                       "ear": np.array([0.07, 0.07, 0.09]),
                       "eye": np.array([0.85, 0.90, 0.98])},
}

# Proportions in donor metres, tuned against the measured Wolf/Worg frame.
# Topline rows are (height above ground, z, radius); masses are
# (height, z, (side, up, forward)).
PROPORTIONS = {
    "hyena": {
        "dorsal": ((0.56, 0.42, 0.13), (0.58, 0.28, 0.16), (0.62, 0.02, 0.17),
                   (0.68, -0.30, 0.21), (0.74, -0.50, 0.20)),
        "masses": ((0.60, -0.34, (0.17, 0.22, 0.26)),
                   (0.55, 0.06, (0.15, 0.16, 0.30))),
        "shoulder": (0.62, 0.13, (0.10, 0.17, 0.13)),
        "haunch": (0.55, 0.12, (0.09, 0.14, 0.12)),
        "neck_root": (0.72, -0.56), "neck_radii": (0.17, 0.15, 0.12),
        "head_drop": 0.02, "muzzle_extra": -0.11, "head_width": 1.25,
        "muzzle_radii": (0.075, 0.062), "nose_radius": 0.045,
        "jaw_depth": 0.075, "jaw_radii": (0.052, 0.044, 0.032), "fang": 0.03,
        "ear": {"rise": 1.0, "splay": 0.35, "lean": 0.10,
                "size": (0.050, 0.056, 0.022)},
        "eye": 1.0, "withers_drop": 0.18,
        "front_top": 0.60, "hind_top": 0.52,
        "front_radii": (0.095, 0.062, 0.037, 0.037),
        "hind_radii": (0.10, 0.070, 0.036, 0.032, 0.034),
        "tail": {"mode": "hang", "root": 0.56, "drop": 0.30,
                 "radii": (0.032, 0.028, 0.034, 0.045)},
        "belly": 0.40,
        "torso_resolution": 0.012, "torso_smooth": 24, "torso_width": 1.0,
        "resolution": 0.007, "smooth": 10, "triangles": 6000,
    },
    "goblin-dog": {
        "dorsal": ((0.56, 0.42, 0.09), (0.62, 0.28, 0.12), (0.70, 0.04, 0.12),
                   (0.68, -0.30, 0.15), (0.66, -0.50, 0.14)),
        "masses": ((0.55, -0.34, (0.12, 0.15, 0.16)),),
        "shoulder": (0.60, 0.09, (0.07, 0.12, 0.10)),
        "haunch": (0.56, 0.09, (0.07, 0.12, 0.10)),
        "neck_root": (0.66, -0.56), "neck_radii": (0.11, 0.09, 0.08),
        "head_drop": 0.06, "muzzle_extra": 0.02, "head_width": 1.20,
        "muzzle_radii": (0.060, 0.042), "nose_radius": 0.032,
        "jaw_depth": 0.06, "jaw_radii": (0.042, 0.034, 0.024), "fang": 0.045,
        "ear": {"rise": 0.75, "splay": 0.75, "lean": 0.25,
                "size": (0.075, 0.085, 0.012)},
        "eye": 1.5, "withers_drop": 0.24,
        "front_top": 0.56, "hind_top": 0.54,
        "front_radii": (0.065, 0.040, 0.024, 0.028),
        "hind_radii": (0.075, 0.050, 0.026, 0.024, 0.027),
        "tail": {"mode": "bones", "root": 0.60,
                 "radii": (0.040, 0.030, 0.020, 0.014, 0.008)},
        "belly": 0.44,
        "torso_resolution": 0.012, "torso_smooth": 20, "torso_width": 0.85,
        "resolution": 0.007, "smooth": 10, "triangles": 5600,
    },
    "wolverine": {
        "dorsal": ((0.46, 0.42, 0.17), (0.52, 0.26, 0.21), (0.54, 0.02, 0.22),
                   (0.50, -0.30, 0.21), (0.50, -0.48, 0.19)),
        "masses": ((0.38, -0.10, (0.19, 0.17, 0.42)),
                   (0.38, -0.34, (0.18, 0.17, 0.20))),
        "shoulder": (0.40, 0.12, (0.10, 0.14, 0.12)),
        "haunch": (0.40, 0.12, (0.10, 0.15, 0.13)),
        "neck_root": (0.48, -0.56), "neck_radii": (0.16, 0.14, 0.12),
        "head_drop": 0.30, "muzzle_extra": -0.16, "head_width": 1.20,
        "muzzle_radii": (0.072, 0.060), "nose_radius": 0.042,
        "jaw_depth": 0.06, "jaw_radii": (0.050, 0.042, 0.030), "fang": 0.025,
        "ear": {"rise": 0.8, "splay": 0.6, "lean": 0.0,
                "size": (0.034, 0.032, 0.014)},
        "eye": 0.9, "withers_drop": 0.42,
        "front_top": 0.40, "hind_top": 0.42,
        "front_radii": (0.10, 0.070, 0.046, 0.046),
        "hind_radii": (0.11, 0.075, 0.048, 0.042, 0.046),
        "tail": {"mode": "hang", "root": 0.48, "drop": 0.22,
                 "radii": (0.060, 0.070, 0.065, 0.045)},
        "belly": 0.20,
        "torso_resolution": 0.012, "torso_smooth": 24, "torso_width": 1.1,
        "resolution": 0.007, "smooth": 10, "triangles": 6000,
    },
    "shadow-mastiff": {
        "dorsal": ((0.66, 0.42, 0.12), (0.68, 0.28, 0.14), (0.70, 0.04, 0.13),
                   (0.72, -0.30, 0.20), (0.80, -0.52, 0.19)),
        "masses": ((0.58, -0.36, (0.16, 0.24, 0.22)),),
        "shoulder": (0.66, 0.11, (0.09, 0.16, 0.12)),
        "haunch": (0.60, 0.10, (0.08, 0.15, 0.12)),
        "neck_root": (0.76, -0.56), "neck_radii": (0.15, 0.13, 0.11),
        "head_drop": 0.0, "muzzle_extra": -0.04, "head_width": 1.25,
        "muzzle_radii": (0.075, 0.062), "nose_radius": 0.045,
        "jaw_depth": 0.08, "jaw_radii": (0.055, 0.046, 0.034), "fang": 0.03,
        "ear": {"rise": 1.0, "splay": 0.25, "lean": 0.15,
                "size": (0.046, 0.088, 0.016)},
        "eye": 1.8, "withers_drop": 0.16,
        "front_top": 0.62, "hind_top": 0.58,
        "front_radii": (0.085, 0.055, 0.034, 0.038),
        "hind_radii": (0.095, 0.065, 0.036, 0.032, 0.036),
        "tail": {"mode": "bones", "root": 0.66,
                 "radii": (0.060, 0.050, 0.040, 0.030, 0.018)},
        "belly": 0.44,
        "torso_resolution": 0.012, "torso_smooth": 22, "torso_width": 0.85,
        "resolution": 0.007, "smooth": 10, "triangles": 6000,
    },
}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", choices=tuple(PROPORTIONS), required=True)
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = load_rig(args.capture, args.kind)
    armature = cb.review_armature(rig_data, args.kind.title() + "RigPreview")
    animal, weights, landmarks = build(rig, armature, args.kind,
                                       PROPORTIONS[args.kind])
    cb.unwrap(animal)
    cb.bake_albedo(animal, 1024, colour_field(args.kind, landmarks),
                   args.albedo)
    cb.attach_albedo_preview(animal, args.albedo)
    summary = cb.write_mesh_data(args.mesh_data, animal, weights, rig_hash,
                                 args.albedo, SPACE)
    if tuple(summary["bones"]) != BONES:
        raise SystemExit(args.kind + " must bind exactly the reviewed 34 "
                         "controls: " + json.dumps(summary["bones"]))
    report = {"schemaVersion": 1, "creature": args.kind,
              "builder": "build_worg_frame.py",
              "donorFamily": "wolf" if args.kind == "hyena" else "worg",
              "rigSha256": rig_hash, **summary,
              "albedo": cb.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    groups = {}
    for index, row in weights.items():
        for name, value in row:
            group = groups.get(name) or animal.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    modifier = animal.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    animal.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[" + args.kind + "] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
