"""Build the refined Tyrannosaurus as one continuous skinned body (VIS-001).

blender -b --factory-startup --python-exit-code 1 --python build_tyrannosaurus.py -- \
    --capture <private sprint24-theropod-census.json> \
    --albedo assets/sprint24-theropods/tyrannosaurus-albedo.png \
    --mesh-data assets/sprint24-theropods/tyrannosaurus-mesh.json \
    --report <private report.json> --blend-out <private source.blend>

The Lizardfolk census is validated by the Sprint 24 generator's own loader.
Its legs are already digitigrade, which suits a theropod, but its torso,
neck and head stand upright, its arms hang in an A-pose about 0.45 m from
the chest and its tail droops to the ground. The body is therefore authored
leaning forward over the hips with a horizontal countertail; the deep skull
sits forward of the head pivot like a real occipital joint; and the tiny
forearms are rooted in the chest and follow Torso_Upper for most of their
weight, so the donor's broad arm swings reach them only as a restrained
fraction instead of throwing them through a wide arc.
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
import continuous_body as cb  # noqa: E402
import generate_theropods as theropods  # noqa: E402

SPACE = "donor renderer local; +X lateral, +Y forward, +Z up"
SIDE = Vector((1.0, 0.0, 0.0))      # creature's left
UP = Vector((0.0, 0.0, 1.0))
FORWARD = Vector((0.0, 1.0, 0.0))
REGION = {"body": 0, "jaw": 1, "tooth": 2, "eye": 3, "claw": 4, "arm": 5,
          "toe": 6}
TAIL = ("tail", "tail1", "tail2", "tail3")
# The body's heat solve; arms, jaw, toes, claws and tail are explicit.
HEAT = ("Torso_Lower", "Torso_Upper", "neck", "neck1", "Head",
        "L_Leg0_Upper", "L_Leg0_Lower", "L_Foot0", "L_toe",
        "R_Leg0_Upper", "R_Leg0_Lower", "R_Foot0", "R_toe")
# Shares of each forelimb segment that follow its own control rather than
# the chest; the rest rides Torso_Upper.
ARM_SHARE = {"upper": 0.10, "lower": 0.16, "hand": 0.22, "finger": 0.26}


def tail_profile():
    # Horizontal countertail: centre line and radius along the body axis.
    return ([Vector((0, -0.25, 1.03)), Vector((0, -0.65, 1.00)),
             Vector((0, -1.10, 0.94)), Vector((0, -1.60, 0.85)),
             Vector((0, -2.10, 0.75)), Vector((0, -2.55, 0.66))],
            [0.38, 0.30, 0.22, 0.15, 0.09, 0.04])


def build(rig, armature):
    at = lambda name: Vector(rig[name]["head"])
    volume = cb.Volume(0.018)
    axes = (SIDE, FORWARD, UP)
    # Massive hips and a deep barrel leaning forward over them.
    volume.ellipsoid(Vector((0, -0.10, 1.00)), axes, (0.40, 0.55, 0.38))
    volume.sweep([Vector((0, -0.10, 1.02)), Vector((0, 0.20, 1.06)),
                  Vector((0, 0.45, 1.14)), Vector((0, 0.60, 1.26))],
                 [0.40, 0.44, 0.40, 0.32])
    volume.ellipsoid(Vector((0, 0.25, 0.88)), axes, (0.32, 0.42, 0.24))
    # A short, thick neck rising forward to the skull.
    volume.sweep([Vector((0, 0.58, 1.30)), Vector((0, 0.64, 1.46)),
                  Vector((0, 0.62, 1.58))], [0.32, 0.27, 0.25])
    # Deep, long skull: a tall rear cranium with heavy jowls, a long muzzle
    # and a blunt snout, carried forward of and below the donor's high head
    # pivot like a real occipital joint.
    volume.ellipsoid(Vector((0, 0.72, 1.66)), axes, (0.21, 0.30, 0.22))
    volume.ellipsoid(Vector((0, 0.74, 1.50)), axes, (0.20, 0.25, 0.15))
    volume.ellipsoid(Vector((0, 1.00, 1.62)), axes, (0.17, 0.30, 0.18))
    volume.ellipsoid(Vector((0, 1.25, 1.57)), axes, (0.14, 0.20, 0.14))
    for sign in (1.0, -1.0):
        volume.ball(Vector((sign * 0.12, 0.88, 1.80)), 0.055)
        volume.ball(Vector((sign * 0.10, 0.80, 1.84)), 0.045)
    for prefix in ("L", "R"):
        hip, knee = at(prefix + "_Leg0_Upper"), at(prefix + "_Leg0_Lower")
        ankle, ball = at(prefix + "_Foot0"), at(prefix + "_toe")
        sign = 1.0 if hip.x > 0 else -1.0
        volume.sweep([Vector((sign * 0.22, -0.05, 0.98)),
                      Vector((sign * 0.20, 0.05, 0.72)), knee],
                     [0.34, 0.27, 0.17])
        volume.sweep([knee, knee.lerp(ankle, 0.5), ankle], [0.16, 0.12, 0.10])
        volume.sweep([ankle, ankle.lerp(ball, 0.5), ball + UP * 0.06],
                     [0.09, 0.075, 0.075])
    tail_points, tail_radii = tail_profile()
    volume.sweep(tail_points, tail_radii)

    body = volume.polygonise("TyrannosaurusBody")
    cb.finish_surface(body, 5800, smooth_iterations=16)
    positions = [v.co.copy() for v in body.data.vertices]
    heat = cb.heat_weights(body, armature, set(HEAT))
    heat = cb.clean_weights(heat, positions, armature, set(HEAT))
    heat = cb.clean_weights(cb.smooth_weights(body, heat), positions,
                            armature, set(HEAT))
    # The thick neck sits ahead of the donor's neck control, which the heat
    # solve then leaves unused; give the neck column an explicit share of it
    # that peaks mid-neck, so the native neck motion still bends it.
    heat = cb.blend_toward(
        heat, positions,
        lambda q, row: 0.40 < q.y < 0.85 and 1.22 < q.z < 1.70,
        "neck", lambda q: 0.55 * math.exp(-((q.z - 1.44) / 0.12) ** 2))
    # The skull moves as one with its rigid teeth and eyes: Head owns it
    # outright ahead of the neck, ramping in over the throat.
    def into_skull(q):
        t = max(0.0, min(1.0, (q.y - 0.56) / 0.16))
        return t * t * (3.0 - 2.0 * t)
    heat = cb.blend_toward(heat, positions,
                           lambda q, row: q.y > 0.56 and q.z > 1.36,
                           "Head", into_skull)
    heat = cb.clean_weights(heat, positions, armature, set(HEAT))
    rows = {}
    for index, point in enumerate(positions):
        # Behind the hips the tail chain owns the surface, blended smoothly
        # from the hips through the four captured joints.
        share = tail_chain_weights(point)
        if share is None:
            rows[index] = heat[index]
            continue
        merged = {}
        for name, value in heat[index]:
            merged[name] = merged.get(name, 0.0) + value * share[0]
        for name, value in share[1]:
            merged[name] = merged.get(name, 0.0) + value
        rows[index] = sorted(merged.items(), key=lambda e: (-e[1], e[0]))
    rows = cb.clean_weights(rows, positions, armature, set(HEAT) | set(TAIL))
    parts = [(body, rows, REGION["body"])]

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), REGION[region]))

    # Deep lower jaw on its own two joints, so the native bite opens it.
    jaw_root = Vector((0, 0.62, 1.44))
    jaw_tip = Vector((0, 1.34, 1.43))
    jaw_axis = (jaw_tip - jaw_root).normalized()
    jaw = cb.blob("TyrannosaurusJaw", jaw_root.lerp(jaw_tip, 0.5),
                  (SIDE, jaw_axis, SIDE.cross(jaw_axis)),
                  (0.165, (jaw_tip - jaw_root).length * 0.54, 0.095), 20, 12)
    jaw_rows = {}
    for vertex in jaw.data.vertices:
        t = max(0.0, min(1.0, (vertex.co - jaw_root).dot(jaw_axis) /
                         (jaw_tip - jaw_root).length))
        jaw_rows[vertex.index] = [("jaw1", 0.35 + 0.65 * t),
                                  ("jaw", 0.65 - 0.65 * t)]
    parts.append((jaw, jaw_rows, REGION["jaw"]))
    for sign in (1.0, -1.0):
        for index in range(8):
            t = index / 7.0
            root = Vector((sign * (0.15 - 0.06 * t), 0.78 + 0.56 * t,
                           1.47 - 0.02 * t))
            rigid(cb.horn("TyrannosaurusTooth", root + UP * 0.025,
                          root - UP * (0.085 - 0.03 * t), 0.022, 8, 3,
                          hint=FORWARD),
                  [("Head", 1.0)], "tooth")
        for index in range(6):
            t = index / 5.0
            root = Vector((sign * (0.13 - 0.05 * t), 0.86 + 0.44 * t, 1.48))
            rigid(cb.horn("TyrannosaurusLowerTooth", root - UP * 0.025,
                          root + UP * (0.060 - 0.02 * t), 0.018, 8, 3,
                          hint=FORWARD),
                  [("jaw1", 1.0)], "tooth")
        eye = Vector((sign * 0.168, 0.86, 1.74))
        rigid(cb.blob("TyrannosaurusEye", eye, (SIDE, FORWARD, UP),
                      (0.030, 0.036, 0.030), 12, 8), [("Head", 1.0)], "eye")
    # Tiny two-fingered forearms rooted in the chest.
    for prefix, sign in (("L", 1.0), ("R", -1.0)):
        shoulder = Vector((sign * 0.27, 0.64, 1.12))
        elbow = Vector((sign * 0.31, 0.74, 1.00))
        wrist = Vector((sign * 0.31, 0.86, 0.99))
        arm = cb.strand("TyrannosaurusArm", [shoulder - SIDE * sign * 0.08,
                                             shoulder, elbow, wrist],
                        [0.075, 0.062, 0.044, 0.034], 10)
        arm_rows = {}
        for vertex in arm.data.vertices:
            along = (vertex.co - shoulder).dot((wrist - shoulder).normalized()) / \
                (wrist - shoulder).length
            if along < 0.45:
                own, name = ARM_SHARE["upper"], prefix + "_Arm_Upper"
            elif along < 0.9:
                own, name = ARM_SHARE["lower"], prefix + "_Arm_Lower"
            else:
                own, name = ARM_SHARE["hand"], prefix + "_Palm"
            arm_rows[vertex.index] = [("Torso_Upper", 1.0 - own), (name, own)]
        parts.append((arm, arm_rows, REGION["arm"]))
        for offset, first, second in ((-0.02, "_finger1", "_finger2"),
                                      (0.02, "_Bfinger1", "_Bfinger2")):
            base = wrist + SIDE * sign * offset
            tip = base + FORWARD * 0.08 - UP * 0.045
            finger = cb.strand("TyrannosaurusFinger", [base, base.lerp(tip, 0.5),
                                                      tip], [0.020, 0.016, 0.009], 7)
            finger_rows = {}
            for vertex in finger.data.vertices:
                tip_side = (vertex.co - base).dot((tip - base).normalized()) > \
                    0.5 * (tip - base).length
                name = prefix + (second if tip_side else first)
                share = ARM_SHARE["finger"]
                finger_rows[vertex.index] = [("Torso_Upper", 1.0 - share),
                                             (name, share)]
            parts.append((finger, finger_rows, REGION["claw"]))
    # Three forward toes on the ball of each foot, with claws.
    for prefix in ("L", "R"):
        ball, tip_bone = at(prefix + "_toe"), at(prefix + "_claw")
        sign = 1.0 if ball.x > 0 else -1.0
        reach = (tip_bone - ball)
        reach.z = 0.0
        length = max(reach.length, 0.18) * 1.3
        ahead = reach.normalized()
        for spread, scale in ((-0.42, 0.85), (0.0, 1.0), (0.42, 0.85)):
            direction = (ahead * math.cos(spread) +
                         SIDE * sign * math.sin(spread)).normalized()
            base = ball + UP * 0.045
            toe_tip = base + direction * length * scale - UP * 0.015
            toe = cb.strand("TyrannosaurusToe", [base - direction * 0.04,
                                                 base.lerp(toe_tip, 0.55),
                                                 toe_tip],
                            [0.055, 0.044, 0.030], 9)
            toe_rows = {}
            for vertex in toe.data.vertices:
                t = max(0.0, min(1.0, (vertex.co - base).dot(direction) /
                                 (length * scale)))
                toe_rows[vertex.index] = [(prefix + "_toe", 1.0 - 0.7 * t),
                                          (prefix + "_claw", 0.7 * t)]
            parts.append((toe, toe_rows, REGION["toe"]))
            rigid(cb.horn("TyrannosaurusClaw", toe_tip - direction * 0.015,
                          toe_tip + direction * 0.07 - UP * 0.03, 0.028, 8, 4,
                          hint=UP),
                  [(prefix + "_claw", 1.0)], "claw")
    rex, weights = cb.assemble("Tyrannosaurus", parts)
    cb.require_single_body(rex, REGION["body"])
    return rex, weights


def tail_chain_weights(point):
    """None outside the tail column; otherwise (share kept by the body
    solve, [(tail bone, weight), ...]) blending along the four captured
    joints. The donor's ankles sit behind its hips but below the tail, so
    the column starts above the legs and ramps in behind the hips."""
    stations = (-0.25, -0.70, -1.20, -1.75, -2.25)
    names = ("Torso_Lower",) + TAIL
    y = point.y
    if y >= stations[0] or point.z < 0.62:
        return None
    ramp = min(1.0, (stations[0] - y) / 0.40)
    ramp = ramp * ramp * (3 - 2 * ramp)
    for index in range(len(stations) - 1):
        if y >= stations[index + 1]:
            t = (stations[index] - y) / (stations[index] - stations[index + 1])
            smooth = t * t * (3 - 2 * t)
            pair = [(names[index], (1.0 - smooth) * ramp),
                    (names[index + 1], smooth * ramp)]
            return 1.0 - ramp, [(n, v) for n, v in pair if v > 0.0]
    return 1.0 - ramp, [("tail3", ramp)]


def colour_field():
    def colour(p, n, region):
        up = n[:, 2]
        back = np.array([0.36, 0.33, 0.25])
        flank = np.array([0.50, 0.45, 0.34])
        belly = np.array([0.70, 0.64, 0.50])
        t = np.clip(up * 0.5 + 0.5, 0, 1)[:, None]
        base = np.where(t > 0.5, flank + (back - flank) * ((t - 0.5) / 0.5),
                        belly + (flank - belly) * (t / 0.5))
        scale, second = cb.cells(p, 1.0 / 0.035, 31)
        rim = np.clip((second - scale) * 3.0, 0, 1)
        broad = cb.fbm(p, 1.0 / 0.35, 32)
        stripes = 0.5 + 0.5 * np.sin(p[:, 1] * 9.0 + broad * 5.0)
        dorsal = np.clip((up - 0.25) * 1.6, 0, 1) * np.clip(stripes - 0.45, 0,
                                                            1) * 1.4
        base = base * (0.80 + 0.20 * rim[:, None]) * \
            (0.88 + 0.22 * broad[:, None]) * (1 - 0.35 * dorsal[:, None])
        rgb = base
        rgb = np.where((region == REGION["jaw"])[:, None], base * 1.05, rgb)
        rgb = np.where((region == REGION["tooth"])[:, None],
                       np.array([0.86, 0.80, 0.64]), rgb)
        rgb = np.where((region == REGION["eye"])[:, None],
                       np.array([0.55, 0.35, 0.08]), rgb)
        rgb = np.where((region == REGION["claw"])[:, None],
                       np.array([0.20, 0.18, 0.15]), rgb)
        return rgb
    return colour


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = theropods.measured_rig(args.capture)
    armature = cb.review_armature(rig_data, "TyrannosaurusEditableRig")
    rex, weights = build(rig, armature)
    cb.unwrap(rex)
    cb.bake_albedo(rex, 1024, colour_field(), args.albedo)
    cb.attach_albedo_preview(rex, args.albedo)
    extra = {"creature": "tyrannosaurus", "donorFamily": "lizardfolk-free-hands",
             "nativeDonor": theropods.DONOR_GUID,
             "nativePrefab": theropods.PREFAB_GUID,
             "bodyRenderer": theropods.BODY_RENDERER,
             "accessoryRenderer": theropods.ACCESSORY_RENDERER,
             "nativeRendererCount": 2, "visibleLegs": 2, "visibleForelimbs": 2,
             "biped": True, "jawJoints": 2, "tailJoints": 4,
             "sickleFootClaws": 0, "massiveSkull": True, "tinyForearms": True,
             "balancedCountertail": True}
    summary = cb.write_mesh_data(args.mesh_data, rex, weights, rig_hash,
                                 args.albedo, SPACE, extra)
    if sorted(summary["bones"]) != sorted(theropods.DRIVERS):
        raise SystemExit("Tyrannosaurus must bind exactly the reviewed 35 "
                         "drivers: " + json.dumps(summary["bones"]))
    report = {"schemaVersion": 1, "builder": "build_tyrannosaurus.py",
              "rigSha256": rig_hash, **extra, **summary,
              "albedo": cb.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    groups = {}
    for index, row in weights.items():
        for name, value in row:
            group = groups.get(name) or rex.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    modifier = rex.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    rex.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[tyrannosaurus] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
