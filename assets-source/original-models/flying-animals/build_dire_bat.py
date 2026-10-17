"""Build the refined Dire Bat as one continuous skinned body (VIS-013).

blender -b --factory-startup --python-exit-code 1 --python build_dire_bat.py -- \
    --rig <private rig.measured.json> \
    --albedo assets/flying-animals/dire-bat-albedo.png \
    --mesh-data assets/flying-animals/dire-bat-mesh.json \
    --report <private report.json> --blend-out <private source.blend>

The rig is the Giant Eagle bind frame that `../pteranodon/convert_bind_rig.py`
makes from the guarded Pteranodon capture (+X left, +Y up, -Z forward; the
wings spread and raised). The owner found the first bat hard to recognise,
with detached-looking claws and toes and blocky, overlapping body and wing
pieces, and it shipped inside out. This one is a compact furred body with a
short, thick neck, a blunt-snouted head with large pointed ears, a nose leaf
and fangs, thin legs with clawed toes, and two membrane wings: closed thin
shells stretched over the arm and four long fingers and drawn in between
the finger tips to the ankle, with a thumb claw at each wrist. The membrane
keeps the proven weighting of the first bat (each point shares its leading
bone and the nearest trailing bones); the body, arms and fingers carry their
own bones, so every one of the rig's 46 audited bones carries geometry.
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
sys.path.insert(0, str(HERE.parent / "pteranodon"))
import continuous_body as cb  # noqa: E402
import generate_pteranodon as shared  # noqa: E402

SPACE = "donor renderer local; +X left, +Y up, -Z forward"
SIDE = Vector((1.0, 0.0, 0.0))      # the creature's left
UP = Vector((0.0, 1.0, 0.0))
FORWARD = Vector((0.0, 0.0, -1.0))
REGION = {"body": 0, "membrane": 1, "ear": 2, "eye": 3, "tooth": 4,
          "claw": 5, "bone": 6, "nose": 7}
HEAT = ("LowerTorso", "UpperTorso", "Neck", "Head",
        "L_Arm_Upper", "R_Arm_Upper",
        "L_Leg0_Upper", "L_Leg0_Lower", "L_Foot0",
        "R_Leg0_Upper", "R_Leg0_Lower", "R_Foot0")
# The wing grid: stations between consecutive trailing points, chord rows.
STATIONS_PER_LINK = 6
CHORD_ROWS = 9
HALF_THICKNESS = 0.014
LEADING_CHAIN = ["Arm_Upper", "Arm_Lower", "Palm", "Feather_1",
                 "Feather_1_end"]
LEADING_BONES = ["Arm_Upper", "Arm_Lower", "Palm", "Feather_1"]
TRAILING_CHAIN = ["Foot0", "Feather_6_end", "Feather_5_end",
                  "Feather_4_end", "Feather_3_end", "Feather_2_end",
                  "Feather_1_end"]
TRAILING_BONES = ["Foot0", "Feather_6", "Feather_5", "Feather_4",
                  "Feather_3", "Feather_2", "Feather_1"]


def mirrored(point):
    return Vector((-point.x, point.y, point.z))


def wing_grid(at, prefix):
    """The left-hand membrane: the arm and first finger lead; the trailing
    edge runs from the ankle through the finger tips, scalloped inward
    between them the way a bat's membrane draws in."""
    return cb.membrane_grid(
        [at(prefix + name) for name in LEADING_CHAIN],
        [at(prefix + name) for name in TRAILING_CHAIN],
        [prefix + name for name in LEADING_BONES],
        [prefix + name for name in TRAILING_BONES], UP,
        STATIONS_PER_LINK, CHORD_ROWS, scallop=0.26, scallop_from=2, sag=0.06)


def build(rig, armature):
    at = lambda name: Vector(rig[name]["head"])
    volume = cb.Volume(0.016)
    axes = (SIDE, UP, FORWARD)
    head = at("Head")
    # A compact, deep, furred body, hips to chest.
    volume.ellipsoid(Vector((0, 1.22, 0.14)), axes, (0.25, 0.25, 0.33))
    volume.ellipsoid(Vector((0, 1.56, -0.26)), axes, (0.31, 0.30, 0.31))
    volume.sweep([Vector((0, 1.10, 0.40)), Vector((0, 1.34, 0.06)),
                  Vector((0, 1.58, -0.26)), Vector((0, 1.76, -0.52))],
                 [0.19, 0.27, 0.29, 0.24])
    # Shoulders that carry the arms out of the chest.
    for sign in (1.0, -1.0):
        volume.ellipsoid(Vector((sign * 0.22, 1.82, -0.20)), axes,
                         (0.15, 0.14, 0.15))
    # A thick furred ruff from the chest to the head, as wide as the chest
    # at its root, so the head sits on the body rather than on a neck.
    volume.sweep([Vector((0, 1.66, -0.40)), Vector((0, 1.80, -0.66)),
                  Vector((0, 1.88, -0.86)), head + Vector((0, -0.02, 0.06))],
                 [0.29, 0.27, 0.24, 0.20])
    # A rounded skull and a short, blunt snout.
    volume.ellipsoid(head + Vector((0, 0.0, -0.03)), axes, (0.20, 0.19, 0.21))
    volume.sweep([head + Vector((0, -0.02, -0.12)),
                  head + Vector((0, -0.06, -0.28)),
                  head + Vector((0, -0.08, -0.37))], [0.15, 0.11, 0.075])
    # Thin legs with small feet on the measured bones.
    for prefix, sign in (("L_", 1.0), ("R_", -1.0)):
        hip, knee, ankle = (at(prefix + n) for n in
                            ("Leg0_Upper", "Leg0_Lower", "Foot0"))
        volume.sweep([Vector((sign * 0.16, 1.12, 0.20)), hip, knee],
                     [0.15, 0.10, 0.055])
        volume.sweep([knee, knee.lerp(ankle, 0.5), ankle],
                     [0.052, 0.045, 0.042])
        volume.ellipsoid(ankle + Vector((0, -0.02, -0.03)), axes,
                         (0.055, 0.04, 0.075))

    body = volume.polygonise("DireBatBody")
    cb.finish_surface(body, 7000, smooth_iterations=12)
    positions = [v.co.copy() for v in body.data.vertices]
    heat = cb.heat_weights(body, armature, set(HEAT))
    heat = cb.clean_weights(heat, positions, armature, set(HEAT))
    heat = cb.clean_weights(cb.smooth_weights(body, heat), positions,
                            armature, set(HEAT))

    def into_head(q):
        t = max(0.0, min(1.0, ((head.z + 0.18) - q.z) / 0.16))
        return t * t * (3.0 - 2.0 * t)

    heat = cb.blend_toward(heat, positions,
                           lambda q, row: q.z < head.z + 0.18 and q.y > 1.70,
                           "Head", into_head)
    heat = cb.blend_toward(
        heat, positions,
        lambda q, row: head.z + 0.10 < q.z < -0.45 and q.y > 1.62,
        "Neck", lambda q: 0.45 * math.exp(-((q.z + 0.78) / 0.14) ** 2))
    rows = cb.clean_weights(heat, positions, armature, set(HEAT))
    parts = [(body, rows, REGION["body"])]

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), REGION[region]))

    # The lower jaw, ears, eyes, nose leaf and fangs.
    jaw, jaw_end = at("Jaw"), at("Jaw_end")
    jaw_axis = (jaw_end - jaw).normalized()
    rigid(cb.blob("DireBatJaw", jaw + jaw_axis * 0.13 + UP * 0.02,
                  (SIDE, jaw_axis, SIDE.cross(jaw_axis)), (0.085, 0.15, 0.04),
                  16, 10), [("Jaw", 1.0)], "body")
    for sign in (1.0, -1.0):
        # Broad, pointed ears, cupped forward and swept back at the tip.
        base = head + SIDE * (sign * 0.12) + UP * 0.10 + FORWARD * -0.04
        tip = head + SIDE * (sign * 0.31) + UP * 0.50 + FORWARD * -0.20
        rigid(cb.horn("DireBatEar", base, tip, 0.17, 16, 7,
                      curve=FORWARD * -0.05 + SIDE * (sign * 0.03),
                      flatten=0.24, hint=FORWARD),
              [("Head", 1.0)], "ear")
        rigid(cb.blob("DireBatEye", head + SIDE * (sign * 0.115) +
                      UP * 0.05 + FORWARD * 0.16, (SIDE, UP, FORWARD),
                      (0.032, 0.032, 0.026), 12, 8), [("Head", 1.0)], "eye")
        root = head + SIDE * (sign * 0.05) + UP * -0.10 + FORWARD * 0.32
        rigid(cb.horn("DireBatFang", root, root - UP * 0.10 + FORWARD * 0.01,
                      0.02, 8, 4, hint=FORWARD), [("Head", 1.0)], "tooth")
    leaf = head + UP * -0.04 + FORWARD * 0.40
    rigid(cb.horn("DireBatNoseLeaf", leaf, leaf + UP * 0.13 + FORWARD * 0.01,
                  0.045, 10, 5, flatten=0.35, hint=FORWARD),
          [("Head", 1.0)], "nose")
    # A short tail stub under the body.
    tail = at("Tail")
    stub = [Vector((0, 1.08, 0.40)), tail, tail.lerp(at("Tail_end"), 0.18)]
    tail_obj = cb.tube("DireBatTail", stub, [0.07, 0.05, 0.02], 8)
    parts.append((tail_obj, cb.rigid_rows(tail_obj, [("Tail", 1.0)]),
                  REGION["body"]))

    for prefix, flip in (("L_", False), ("R_", True)):
        # The arm along the leading edge.
        arm = [at(prefix + n) for n in ("Arm_Upper", "Arm_Lower", "Palm")]
        arm = [arm[0], arm[0].lerp(arm[1], 0.5), arm[1],
               arm[1].lerp(arm[2], 0.5), arm[2]]
        obj = cb.tube("DireBatArm", arm, [0.09, 0.075, 0.062, 0.052, 0.046], 10)
        parts.append((obj, cb.chain_rows(obj, [arm[0], arm[2], arm[4]],
                                      [prefix + "Arm_Upper",
                                       prefix + "Arm_Lower"]),
                      REGION["bone"]))
        # Four long fingers from the wrist.
        palm = at(prefix + "Palm")
        for index in range(1, 5):
            name = prefix + "Feather_%d" % index
            points = [palm, at(name), at(name + "_end")]
            obj = cb.tube("DireBatFinger", points, [0.042, 0.03, 0.012], 8)
            parts.append((obj, cb.chain_rows(obj, points,
                                          [prefix + "Palm", name]),
                          REGION["bone"]))
        # The thumb claw at the wrist.
        rigid(cb.horn("DireBatThumb", palm, palm + FORWARD * 0.17 + UP * 0.04,
                      0.035, 8, 5, hint=UP), [(prefix + "Palm", 1.0)], "claw")
        # Clawed toes on the measured toe chains.
        for toe in range(1, 5):
            names = [prefix + "Finger_%d_1" % toe, prefix + "Finger_%d_2" % toe,
                     prefix + "Finger_%d_2_end" % toe]
            points = [at(n) for n in names]
            # The eagle's toes are talons; this creature's feet are small, so
            # the last segment stops short on the same bones.
            points[2] = points[1].lerp(points[2], 0.60)
            obj = cb.tube("DireBatToe", points, [0.026, 0.02, 0.012], 7)
            parts.append((obj, cb.chain_rows(obj, points, names[:2]),
                          REGION["body"]))
            direction = (points[2] - points[1]).normalized()
            rigid(cb.horn("DireBatToeClaw", points[2] - direction * 0.01,
                          points[2] + direction * 0.07 - UP * 0.03, 0.016, 8, 4,
                          hint=UP), [(names[1], 1.0)], "claw")
        # The membrane: a closed thin shell over the wing grid.
        columns, grid_rows = wing_grid(at, "L_")
        if flip:
            columns = [[mirrored(p) for p in column] for column in columns]
            grid_rows = {key: [("R_" + n[2:], v) for n, v in row]
                         for key, row in grid_rows.items()}
        obj, origin = cb.sheet("DireBatMembrane", columns, HALF_THICKNESS)
        rows_for = {index: grid_rows[origin[index]]
                    for index in range(len(obj.data.vertices))}
        parts.append((obj, rows_for, REGION["membrane"]))
    bat, weights = cb.assemble("DireBat", parts)
    cb.require_single_body(bat, REGION["body"])
    return bat, weights


def colour_field():
    def colour(p, n, region):
        up = n[:, 1]
        back = np.array([0.20, 0.15, 0.11])
        belly = np.array([0.33, 0.25, 0.18])
        t = np.clip(up * 0.5 + 0.5, 0, 1)[:, None]
        fur = belly + (back - belly) * t
        streak = cb.fbm(p * np.array([3.0, 0.5, 3.0]), 1.0 / 0.025, 71)
        broad = cb.fbm(p, 1.0 / 0.3, 72)
        fur = fur * (0.82 + 0.26 * streak[:, None]) * (0.92 + 0.14 * broad[:, None])
        membrane = np.array([0.19, 0.145, 0.13]) * \
            (0.90 + 0.16 * cb.fbm(p, 1.0 / 0.4, 73)[:, None])
        rgb = fur
        rgb = np.where((region == REGION["membrane"])[:, None], membrane, rgb)
        rgb = np.where((region == REGION["bone"])[:, None],
                       membrane * 0.85, rgb)
        inner = np.clip(-n[:, 2], 0, 1)[:, None]  # the ear's forward face
        ear = np.array([0.26, 0.17, 0.15]) + inner * np.array([0.14, 0.08, 0.07])
        rgb = np.where((region == REGION["ear"])[:, None], ear, rgb)
        rgb = np.where((region == REGION["nose"])[:, None],
                       np.array([0.32, 0.20, 0.18]), rgb)
        rgb = np.where((region == REGION["eye"])[:, None],
                       np.array([0.03, 0.025, 0.02]), rgb)
        rgb = np.where((region == REGION["tooth"])[:, None],
                       np.array([0.85, 0.80, 0.66]), rgb)
        rgb = np.where((region == REGION["claw"])[:, None],
                       np.array([0.13, 0.11, 0.10]), rgb)
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
    armature = cb.review_armature(rig_data, "DireBatEditableRig")
    bat, weights = build(rig, armature)
    cb.unwrap(bat)
    cb.bake_albedo(bat, 1024, colour_field(), args.albedo)
    cb.attach_albedo_preview(bat, args.albedo)
    extra = {"creature": "dire-bat", "donorFamily": "giant-eagle",
             "visibleLegs": 2, "wings": 2, "wingFingers": 4,
             "membrane": "closed thin shell", "ears": 2}
    summary = cb.write_mesh_data(args.mesh_data, bat, weights, rig_hash,
                                 args.albedo, SPACE, extra)
    if len(summary["bones"]) != 46:
        raise SystemExit("the Dire Bat must carry geometry on all 46 audited "
                         "bones: " + json.dumps(summary["bones"]))
    report = {"schemaVersion": 1, "builder": "build_dire_bat.py",
              "rigSha256": rig_hash, **extra, **summary,
              "albedo": cb.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    groups = {}
    for index, row in weights.items():
        for name, value in row:
            group = groups.get(name) or bat.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    modifier = bat.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    bat.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[dire-bat] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
