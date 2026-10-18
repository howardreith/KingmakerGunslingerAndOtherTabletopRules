"""Build the refined Pteranodon as one continuous skinned body (VIS-020).

blender -b --factory-startup --python-exit-code 1 --python build_pteranodon.py -- \
    --rig <private rig.measured.json> \
    --albedo assets/pteranodon/pteranodon-albedo.png \
    --mesh-data assets/pteranodon/pteranodon-mesh.json \
    --report <private report.json> --blend-out <private source.blend>

The rig is the Giant Eagle bind frame `convert_bind_rig.py` makes from the
guarded capture (+X left, +Y up, -Z forward; wings spread and raised). The
owner found the first Pteranodon hard to recognise, with detached-looking
claws and toes and blocky body and wing planes, and it shipped inside out.
This one is a small body on a slender neck, a long toothless beak with a
lower bill on the jaw, a long crest swept back from the skull, thin legs
with clawed toes, and two membrane wings: closed thin shells stretched from
the arm and the long wing finger back to the ankle, narrowing to the tip,
with three small clawed fingers at each wrist. The membrane keeps the first
Pteranodon's weighting (each point shares its leading bone and the nearest
trailing bones), and every one of the rig's 46 audited bones carries
geometry.
"""
import argparse
import hashlib
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
import continuous_body as cb  # noqa: E402
import generate_pteranodon as shared  # noqa: E402

SPACE = "donor renderer local; +X left, +Y up, -Z forward"
SIDE = Vector((1.0, 0.0, 0.0))      # the creature's left
UP = Vector((0.0, 1.0, 0.0))
FORWARD = Vector((0.0, 0.0, -1.0))
REGION = {"body": 0, "membrane": 1, "crest": 2, "eye": 3, "beak": 4,
          "claw": 5, "bone": 6}
HEAT = ("LowerTorso", "UpperTorso", "Neck", "Head",
        "L_Arm_Upper", "R_Arm_Upper",
        "L_Leg0_Upper", "L_Leg0_Lower", "L_Foot0",
        "R_Leg0_Upper", "R_Leg0_Lower", "R_Foot0")
LEADING_CHAIN = ["Arm_Upper", "Arm_Lower", "Palm", "Feather_1",
                 "Feather_1_end"]
LEADING_BONES = ["Arm_Upper", "Arm_Lower", "Palm", "Feather_1"]
TRAILING_CHAIN = ["Foot0", "Feather_6_end", "Feather_5_end",
                  "Feather_4_end", "Feather_3_end", "Feather_2_end",
                  "Feather_1_end"]
TRAILING_BONES = ["Foot0", "Feather_6", "Feather_5", "Feather_4",
                  "Feather_3", "Feather_2", "Feather_1"]
HALF_THICKNESS = 0.014


def mirrored(point):
    return Vector((-point.x, point.y, point.z))


def wing_grid(at, prefix):
    """The left-hand membrane. The eagle's outer feather tips spread wide;
    a pterosaur's wing narrows to its tip, so the two outermost trailing
    points are drawn toward the wing finger (their bones still drive
    them)."""
    trailing = [at(prefix + name) for name in TRAILING_CHAIN]
    finger = at(prefix + "Feather_1")
    tip = at(prefix + "Feather_1_end")
    trailing[4] = trailing[4].lerp(finger, 0.30)
    trailing[5] = trailing[5].lerp(tip, 0.55)
    return cb.membrane_grid(
        [at(prefix + name) for name in LEADING_CHAIN], trailing,
        [prefix + name for name in LEADING_BONES],
        [prefix + name for name in TRAILING_BONES], UP,
        6, 9, scallop=0.0, sag=0.05)


def build(rig, armature):
    at = lambda name: Vector(rig[name]["head"])
    volume = cb.Volume(0.014)
    axes = (SIDE, UP, FORWARD)
    head = at("Head")
    # A small, deep body under the wings.
    volume.ellipsoid(Vector((0, 1.30, 0.06)), axes, (0.21, 0.21, 0.34))
    volume.ellipsoid(Vector((0, 1.57, -0.22)), axes, (0.25, 0.24, 0.27))
    volume.sweep([Vector((0, 1.16, 0.34)), Vector((0, 1.36, 0.02)),
                  Vector((0, 1.58, -0.24))], [0.15, 0.21, 0.23])
    for sign in (1.0, -1.0):
        volume.ellipsoid(Vector((sign * 0.20, 1.80, -0.18)), axes,
                         (0.12, 0.12, 0.13))
    # A slender neck to a narrow skull.
    volume.sweep([Vector((0, 1.66, -0.38)), at("Neck"),
                  Vector((0, 1.90, -0.94)), head + Vector((0, 0.0, 0.08))],
                 [0.16, 0.12, 0.105, 0.11])
    volume.ellipsoid(head + Vector((0, 0.01, -0.05)), axes, (0.12, 0.14, 0.21))
    # The long, straight upper beak.
    volume.sweep([head + Vector((0, -0.02, -0.18)),
                  head + Vector((0, -0.07, -0.55)),
                  head + Vector((0, -0.12, -0.92)),
                  head + Vector((0, -0.16, -1.16))],
                 [0.095, 0.068, 0.040, 0.012])
    for prefix, sign in (("L_", 1.0), ("R_", -1.0)):
        hip, knee, ankle = (at(prefix + n) for n in
                            ("Leg0_Upper", "Leg0_Lower", "Foot0"))
        volume.sweep([Vector((sign * 0.14, 1.14, 0.18)), hip, knee],
                     [0.12, 0.085, 0.05])
        volume.sweep([knee, knee.lerp(ankle, 0.5), ankle],
                     [0.048, 0.042, 0.040])
        volume.ellipsoid(ankle + Vector((0, -0.02, -0.03)), axes,
                         (0.05, 0.035, 0.07))

    body = volume.polygonise("PteranodonBody")
    cb.finish_surface(body, 7000, smooth_iterations=12)
    positions = [v.co.copy() for v in body.data.vertices]
    heat = cb.heat_weights(body, armature, set(HEAT))
    heat = cb.clean_weights(heat, positions, armature, set(HEAT))
    heat = cb.clean_weights(cb.smooth_weights(body, heat), positions,
                            armature, set(HEAT))

    def into_head(q):
        t = max(0.0, min(1.0, ((head.z + 0.14) - q.z) / 0.12))
        return t * t * (3.0 - 2.0 * t)

    heat = cb.blend_toward(heat, positions,
                           lambda q, row: q.z < head.z + 0.14 and q.y > 1.62,
                           "Head", into_head)
    heat = cb.blend_toward(
        heat, positions,
        lambda q, row: head.z + 0.08 < q.z < -0.40 and q.y > 1.62,
        "Neck", lambda q: 0.55 * math.exp(-((q.z + 0.74) / 0.16) ** 2))
    rows = cb.clean_weights(heat, positions, armature, set(HEAT))
    parts = [(body, rows, REGION["body"])]

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), REGION[region]))

    # The lower bill on the jaw, the crest, the eyes.
    jaw = at("Jaw")
    bill_tip = head + Vector((0, -0.21, -1.10))
    rigid(cb.horn("PteranodonBill", jaw + Vector((0, 0.0, 0.02)), bill_tip,
                  0.075, 12, 8, flatten=0.55, hint=UP),
          [("Jaw", 1.0)], "beak")
    rigid(cb.horn("PteranodonCrest", head + Vector((0, 0.08, 0.02)),
                  head + Vector((0, 0.46, 0.66)), 0.15, 14, 8,
                  curve=Vector((0, 0.05, 0.0)), flatten=0.16, hint=SIDE),
          [("Head", 1.0)], "crest")
    for sign in (1.0, -1.0):
        rigid(cb.blob("PteranodonEye", head + SIDE * (sign * 0.10) +
                      UP * 0.04 + FORWARD * 0.10, (SIDE, UP, FORWARD),
                      (0.03, 0.03, 0.03), 12, 8), [("Head", 1.0)], "eye")
    # A short tail stub.
    tail = at("Tail")
    stub = [Vector((0, 1.10, 0.38)), tail, tail.lerp(at("Tail_end"), 0.14)]
    tail_obj = cb.tube("PteranodonTail", stub, [0.06, 0.04, 0.015], 8)
    parts.append((tail_obj, cb.rigid_rows(tail_obj, [("Tail", 1.0)]),
                  REGION["body"]))

    for prefix, flip in (("L_", False), ("R_", True)):
        arm = [at(prefix + n) for n in ("Arm_Upper", "Arm_Lower", "Palm")]
        arm = [arm[0], arm[0].lerp(arm[1], 0.5), arm[1],
               arm[1].lerp(arm[2], 0.5), arm[2]]
        obj = cb.tube("PteranodonArm", arm, [0.08, 0.068, 0.058, 0.05, 0.046], 10)
        parts.append((obj, cb.chain_rows(obj, [arm[0], arm[2], arm[4]],
                                         [prefix + "Arm_Upper",
                                          prefix + "Arm_Lower"]),
                      REGION["bone"]))
        # The long wing finger.
        palm = at(prefix + "Palm")
        finger = [palm, at(prefix + "Feather_1"), at(prefix + "Feather_1_end")]
        obj = cb.tube("PteranodonWingFinger", finger, [0.046, 0.032, 0.012], 8)
        parts.append((obj, cb.chain_rows(obj, finger,
                                         [prefix + "Palm",
                                          prefix + "Feather_1"]),
                      REGION["bone"]))
        # Three small clawed fingers at the wrist.
        for index, (lift, out) in enumerate(((0.02, 0.0), (-0.03, 0.04),
                                             (-0.08, 0.08))):
            base = palm + UP * lift + FORWARD * 0.03
            tip = base + FORWARD * 0.20 + UP * -0.06 + SIDE * (out if not flip
                                                               else -out)
            rigid(cb.horn("PteranodonHand", base, tip, 0.03, 8, 5, hint=UP),
                  [(prefix + "Palm", 1.0)], "claw")
        for toe in range(1, 5):
            names = [prefix + "Finger_%d_1" % toe, prefix + "Finger_%d_2" % toe,
                     prefix + "Finger_%d_2_end" % toe]
            points = [at(n) for n in names]
            # The eagle's toes are talons; this creature's feet are small, so
            # the last segment stops short on the same bones.
            points[2] = points[1].lerp(points[2], 0.55)
            obj = cb.tube("PteranodonToe", points, [0.024, 0.018, 0.011], 7)
            parts.append((obj, cb.chain_rows(obj, points, names[:2]),
                          REGION["body"]))
            direction = (points[2] - points[1]).normalized()
            rigid(cb.horn("PteranodonToeClaw", points[2] - direction * 0.01,
                          points[2] + direction * 0.06 - UP * 0.025, 0.014,
                          8, 4, hint=UP), [(names[1], 1.0)], "claw")
        columns, grid_rows = wing_grid(at, "L_")
        if flip:
            columns = [[mirrored(p) for p in column] for column in columns]
            grid_rows = {key: [("R_" + n[2:], v) for n, v in row]
                         for key, row in grid_rows.items()}
        obj, origin = cb.sheet("PteranodonMembrane", columns, HALF_THICKNESS)
        parts.append((obj, {index: grid_rows[origin[index]]
                            for index in range(len(obj.data.vertices))},
                      REGION["membrane"]))
    ptero, weights = cb.assemble("Pteranodon", parts)
    cb.require_single_body(ptero, REGION["body"])
    return ptero, weights


def colour_field():
    def colour(p, n, region):
        up = n[:, 1]
        back = np.array([0.36, 0.30, 0.25])
        belly = np.array([0.56, 0.50, 0.42])
        t = np.clip(up * 0.5 + 0.5, 0, 1)[:, None]
        skin = belly + (back - belly) * t
        broad = cb.fbm(p, 1.0 / 0.3, 81)
        fine = cb.fbm(p, 1.0 / 0.03, 82)
        skin = skin * (0.88 + 0.18 * broad[:, None]) * (0.94 + 0.10 * fine[:, None])
        membrane = np.array([0.30, 0.22, 0.17]) * \
            (0.88 + 0.18 * cb.fbm(p * np.array([1.0, 1.0, 3.0]), 1.0 / 0.2, 83)[:, None])
        # The crest: rust, darkening to its rim.
        rim = np.clip((p[:, 1] - 2.15) / 0.25, 0, 1)[:, None]
        crest = np.array([0.56, 0.24, 0.13]) * (1 - 0.45 * rim)
        # The beak: horn-pale, darkening to the tip.
        tip = np.clip((-p[:, 2] - 1.75) / 0.45, 0, 1)[:, None]
        beak = np.array([0.72, 0.64, 0.48]) * (1 - 0.45 * tip)
        rgb = skin
        rgb = np.where((region == REGION["membrane"])[:, None], membrane, rgb)
        rgb = np.where((region == REGION["bone"])[:, None], membrane * 1.1, rgb)
        rgb = np.where((region == REGION["crest"])[:, None], crest, rgb)
        rgb = np.where((region == REGION["beak"])[:, None], beak, rgb)
        # The upper beak is part of the body surface: paint it by position.
        upper_beak = (region == REGION["body"]) & (p[:, 2] < -1.30) & (p[:, 1] > 1.60)
        rgb = np.where(upper_beak[:, None], beak, rgb)
        rgb = np.where((region == REGION["eye"])[:, None],
                       np.array([0.05, 0.03, 0.02]), rgb)
        rgb = np.where((region == REGION["claw"])[:, None],
                       np.array([0.16, 0.13, 0.11]), rgb)
        return rgb
    return colour


def main():
    parser = argparse.ArgumentParser()
    for name in ("rig", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig = shared.load_rig(args.rig)
    rig_hash = hashlib.sha256(Path(args.rig).read_bytes()).hexdigest()
    armature = cb.review_armature(rig_data, "PteranodonEditableRig")
    ptero, weights = build(rig, armature)
    cb.unwrap(ptero)
    cb.bake_albedo(ptero, 1024, colour_field(), args.albedo)
    cb.attach_albedo_preview(ptero, args.albedo)
    extra = {"creature": "pteranodon", "donorFamily": "giant-eagle",
             "visibleLegs": 2, "wings": 2, "handFingers": 3,
             "membrane": "closed thin shell", "crest": True,
             "toothlessBeak": True}
    summary = cb.write_mesh_data(args.mesh_data, ptero, weights, rig_hash,
                                 args.albedo, SPACE, extra)
    if len(summary["bones"]) != 46:
        raise SystemExit("the Pteranodon must carry geometry on all 46 "
                         "audited bones: " + json.dumps(summary["bones"]))
    report = {"schemaVersion": 1, "builder": "build_pteranodon.py",
              "rigSha256": rig_hash, **extra, **summary,
              "albedo": cb.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    groups = {}
    for index, row in weights.items():
        for name, value in row:
            group = groups.get(name) or ptero.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    modifier = ptero.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    ptero.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[pteranodon] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
