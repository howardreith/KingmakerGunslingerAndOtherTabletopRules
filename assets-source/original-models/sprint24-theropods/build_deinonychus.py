"""Build the refined Deinonychus as one continuous skinned body (VIS-027).

blender -b --factory-startup --python-exit-code 1 --python build_deinonychus.py -- \
    --capture <private sprint24-theropod-census.json> \
    --albedo assets/sprint24-theropods/deinonychus-albedo.png \
    --mesh-data assets/sprint24-theropods/deinonychus-mesh.json \
    --report <private report.json> --blend-out <private source.blend>

The Lizardfolk census is validated by the Sprint 24 generator's own loader,
as for the Tyrannosaurus (see build_tyrannosaurus.py for the frame). The
Deinonychus is authored on the same frame as a slender, agile theropod
leaning forward over the hips: a long, low, narrow skull on an S-curved
neck, a two-joint lower jaw with tooth rows, long folded arms with three
large clawed fingers, slender digitigrade legs with a raised sickle claw on
each foot, and a long, stiff countertail. The owner found the first
export's arm swing comical; the arms are rooted in the chest and follow
Torso_Upper for half or more of their weight.
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
HEAT = ("Torso_Lower", "Torso_Upper", "neck", "neck1", "Head",
        "L_Leg0_Upper", "L_Leg0_Lower", "L_Foot0", "L_toe",
        "R_Leg0_Upper", "R_Leg0_Lower", "R_Foot0", "R_toe")
# Shares of each forelimb segment that follow its own control; the rest
# rides Torso_Upper, so the donor's wide A-pose swings arrive restrained.
ARM_SHARE = {"upper": 0.30, "lower": 0.38, "hand": 0.45, "finger": 0.50}


def tail_profile():
    # A long, stiff, nearly horizontal countertail.
    return ([Vector((0, -0.22, 0.95)), Vector((0, -0.60, 0.96)),
             Vector((0, -1.05, 0.94)), Vector((0, -1.55, 0.90)),
             Vector((0, -2.05, 0.86)), Vector((0, -2.45, 0.83))],
            [0.19, 0.14, 0.10, 0.07, 0.04, 0.018])


def build(rig, armature):
    at = lambda name: Vector(rig[name]["head"])
    volume = cb.Volume(0.012)
    axes = (SIDE, FORWARD, UP)
    # Slim hips and a narrow, deep chest leaning forward over them.
    volume.ellipsoid(Vector((0, -0.08, 0.94)), axes, (0.19, 0.30, 0.19))
    volume.sweep([Vector((0, -0.08, 0.96)), Vector((0, 0.16, 1.00)),
                  Vector((0, 0.36, 1.05)), Vector((0, 0.48, 1.10))],
                 [0.20, 0.21, 0.19, 0.15])
    volume.ellipsoid(Vector((0, 0.18, 0.88)), axes, (0.16, 0.26, 0.13))
    # A long S-curved neck carrying the head forward and up.
    volume.sweep([Vector((0, 0.46, 1.12)), Vector((0, 0.58, 1.27)),
                  Vector((0, 0.62, 1.42)), Vector((0, 0.70, 1.54))],
                 [0.13, 0.10, 0.085, 0.08])
    # A long, low, narrow skull and snout.
    volume.ellipsoid(Vector((0, 0.74, 1.59)), axes, (0.085, 0.10, 0.085))
    volume.sweep([Vector((0, 0.74, 1.58)), Vector((0, 0.90, 1.56)),
                  Vector((0, 1.05, 1.53)), Vector((0, 1.13, 1.51))],
                 [0.085, 0.072, 0.054, 0.038])
    for prefix in ("L", "R"):
        hip, knee = at(prefix + "_Leg0_Upper"), at(prefix + "_Leg0_Lower")
        ankle, ball = at(prefix + "_Foot0"), at(prefix + "_toe")
        sign = 1.0 if hip.x > 0 else -1.0
        # Slender thighs flowing out of the hips, then the digitigrade shin
        # and foot along the measured bones.
        volume.sweep([Vector((sign * 0.10, -0.06, 0.92)),
                      Vector((sign * 0.14, 0.02, 0.72)), knee],
                     [0.15, 0.13, 0.08])
        volume.sweep([knee, knee.lerp(ankle, 0.5), ankle],
                     [0.075, 0.055, 0.045])
        volume.sweep([ankle, ankle.lerp(ball, 0.5), ball + UP * 0.04],
                     [0.042, 0.036, 0.034])
    tail_points, tail_radii = tail_profile()
    volume.sweep(tail_points, tail_radii)

    body = volume.polygonise("DeinonychusBody")
    cb.finish_surface(body, 6400, smooth_iterations=14)
    positions = [v.co.copy() for v in body.data.vertices]
    heat = cb.heat_weights(body, armature, set(HEAT))
    heat = cb.clean_weights(heat, positions, armature, set(HEAT))
    heat = cb.clean_weights(cb.smooth_weights(body, heat), positions,
                            armature, set(HEAT))
    # The neck column shares both neck controls, peaking mid-neck, so the
    # native neck motion bends it along its length.
    heat = cb.blend_toward(
        heat, positions, lambda q, row: 0.42 < q.y < 0.72 and 1.10 < q.z < 1.56,
        "neck", lambda q: 0.50 * math.exp(-((q.z - 1.26) / 0.10) ** 2))
    heat = cb.blend_toward(
        heat, positions, lambda q, row: 0.50 < q.y < 0.76 and 1.30 < q.z < 1.60,
        "neck1", lambda q: 0.45 * math.exp(-((q.z - 1.44) / 0.08) ** 2))

    def into_skull(q):
        t = max(0.0, min(1.0, (q.y - 0.64) / 0.12))
        return t * t * (3.0 - 2.0 * t)

    heat = cb.blend_toward(heat, positions,
                           lambda q, row: q.y > 0.64 and q.z > 1.46,
                           "Head", into_skull)
    heat = cb.clean_weights(heat, positions, armature, set(HEAT))
    rows = {}
    for index, point in enumerate(positions):
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

    # A long, shallow lower jaw on its two joints.
    jaw_root = Vector((0, 0.72, 1.52))
    jaw_tip = Vector((0, 1.10, 1.47))
    jaw_axis = (jaw_tip - jaw_root).normalized()
    jaw = cb.blob("DeinonychusJaw", jaw_root.lerp(jaw_tip, 0.5),
                  (SIDE, jaw_axis, SIDE.cross(jaw_axis)),
                  (0.062, (jaw_tip - jaw_root).length * 0.54, 0.034), 18, 10)
    jaw_rows = {}
    for vertex in jaw.data.vertices:
        t = max(0.0, min(1.0, (vertex.co - jaw_root).dot(jaw_axis) /
                         (jaw_tip - jaw_root).length))
        jaw_rows[vertex.index] = [("jaw1", 0.35 + 0.65 * t),
                                  ("jaw", 0.65 - 0.65 * t)]
    parts.append((jaw, jaw_rows, REGION["jaw"]))
    for sign in (1.0, -1.0):
        for index in range(9):
            t = index / 8.0
            root = Vector((sign * (0.052 - 0.020 * t), 0.80 + 0.32 * t,
                           1.505 - 0.02 * t))
            rigid(cb.horn("DeinonychusTooth", root + UP * 0.012,
                          root - UP * (0.034 - 0.010 * t), 0.009, 6, 3,
                          hint=FORWARD),
                  [("Head", 1.0)], "tooth")
        eye = Vector((sign * 0.072, 0.80, 1.625))
        rigid(cb.blob("DeinonychusEye", eye, (SIDE, FORWARD, UP),
                      (0.020, 0.026, 0.022), 12, 8), [("Head", 1.0)], "eye")
    # Long folded arms with three large clawed fingers, rooted in the chest.
    for prefix, sign in (("L", 1.0), ("R", -1.0)):
        shoulder = Vector((sign * 0.14, 0.44, 1.07))
        elbow = Vector((sign * 0.20, 0.42, 0.90))
        wrist = Vector((sign * 0.21, 0.64, 0.86))
        arm = cb.tube("DeinonychusArm", [shoulder - SIDE * sign * 0.06,
                                           shoulder, elbow, wrist],
                        [0.060, 0.050, 0.036, 0.030], 10)
        arm_rows = {}
        span = (wrist - shoulder).length
        for vertex in arm.data.vertices:
            along = (vertex.co - shoulder).dot((wrist - shoulder).normalized()) / \
                span
            if along < 0.40:
                own, name = ARM_SHARE["upper"], prefix + "_Arm_Upper"
            elif along < 0.85:
                own, name = ARM_SHARE["lower"], prefix + "_Arm_Lower"
            else:
                own, name = ARM_SHARE["hand"], prefix + "_Palm"
            arm_rows[vertex.index] = [("Torso_Upper", 1.0 - own), (name, own)]
        parts.append((arm, arm_rows, REGION["arm"]))
        for offset, first, second in ((-0.034, "_finger1", "_finger2"),
                                      (0.0, "_finger1", "_finger2"),
                                      (0.034, "_Bfinger1", "_Bfinger2")):
            base = wrist + SIDE * sign * offset
            tip = base + FORWARD * 0.10 - UP * 0.04
            finger = cb.tube("DeinonychusFinger", [base, base.lerp(tip, 0.5),
                                                     tip],
                               [0.016, 0.013, 0.008], 7)
            finger_rows = {}
            for vertex in finger.data.vertices:
                tip_side = (vertex.co - base).dot((tip - base).normalized()) > \
                    0.5 * (tip - base).length
                name = prefix + (second if tip_side else first)
                share = ARM_SHARE["finger"]
                finger_rows[vertex.index] = [("Torso_Upper", 1.0 - share),
                                             (name, share)]
            parts.append((finger, finger_rows, REGION["arm"]))
            rigid(cb.horn("DeinonychusHandClaw", tip - FORWARD * 0.01,
                          tip + FORWARD * 0.03 - UP * 0.045, 0.011, 8, 4,
                          hint=UP),
                  [(prefix + second, 1.0)], "claw")
    # Two forward toes and the raised sickle claw on each foot.
    for prefix in ("L", "R"):
        ball, tip_bone = at(prefix + "_toe"), at(prefix + "_claw")
        sign = 1.0 if ball.x > 0 else -1.0
        reach = (tip_bone - ball)
        reach.z = 0.0
        length = max(reach.length, 0.16) * 1.15
        ahead = reach.normalized()
        for spread in (-0.22, 0.22):
            direction = (ahead * math.cos(spread) +
                         SIDE * sign * math.sin(spread)).normalized()
            base = ball + UP * 0.03
            toe_tip = base + direction * length - UP * 0.012
            toe = cb.strand("DeinonychusToe", [base - direction * 0.03,
                                               base.lerp(toe_tip, 0.55),
                                               toe_tip],
                            [0.032, 0.026, 0.018], 8)
            toe_rows = {}
            for vertex in toe.data.vertices:
                t = max(0.0, min(1.0, (vertex.co - base).dot(direction) /
                                 length))
                toe_rows[vertex.index] = [(prefix + "_toe", 1.0 - 0.7 * t),
                                          (prefix + "_claw", 0.7 * t)]
            parts.append((toe, toe_rows, REGION["toe"]))
            rigid(cb.horn("DeinonychusToeClaw", toe_tip - direction * 0.01,
                          toe_tip + direction * 0.04 - UP * 0.02, 0.015, 8, 4,
                          hint=UP),
                  [(prefix + "_claw", 1.0)], "claw")
        # The raised sickle claw on the inner toe, held clear of the ground.
        root = ball + UP * 0.07 + SIDE * sign * -0.025 + ahead * 0.02
        rigid(cb.horn("DeinonychusSickle", root, root + ahead * 0.09 +
                      UP * 0.07, 0.026, 10, 6, curve=ahead * 0.05 - UP * 0.03,
                      hint=SIDE),
              [(prefix + "_toe", 1.0)], "claw")
    dino, weights = cb.assemble("Deinonychus", parts)
    cb.require_single_body(dino, REGION["body"])
    return dino, weights


def tail_chain_weights(point):
    """As for the Tyrannosaurus: behind the hips the tail chain owns the
    surface, blended along the four captured joints."""
    stations = (-0.22, -0.65, -1.15, -1.70, -2.20)
    names = ("Torso_Lower",) + TAIL
    y = point.y
    if y >= stations[0] or point.z < 0.62:
        return None
    ramp = min(1.0, (stations[0] - y) / 0.35)
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
        back = np.array([0.24, 0.18, 0.12])
        flank = np.array([0.46, 0.34, 0.20])
        belly = np.array([0.66, 0.58, 0.44])
        t = np.clip(up * 0.5 + 0.5, 0, 1)[:, None]
        base = np.where(t > 0.5, flank + (back - flank) * ((t - 0.5) / 0.5),
                        belly + (flank - belly) * (t / 0.5))
        broad = cb.fbm(p, 1.0 / 0.25, 61)
        # Feather-like streaks along the body and dark bands across the
        # back and tail.
        streak = cb.fbm(p * np.array([3.0, 0.4, 3.0]), 1.0 / 0.03, 62)
        bands = 0.5 + 0.5 * np.sin(p[:, 1] * 16.0 + broad * 3.0)
        dorsal = np.clip((up - 0.2) * 1.8, 0, 1) * np.clip(bands - 0.55, 0, 1) * 2.0
        base = base * (0.86 + 0.20 * broad[:, None]) * \
            (0.88 + 0.18 * streak[:, None]) * (1 - 0.45 * dorsal[:, None])
        # A dark mask through the eye.
        mask = np.exp(-((p[:, 2] - 1.62) / 0.03) ** 2) * \
            np.clip((p[:, 1] - 0.70) / 0.05, 0, 1) * \
            np.clip((0.98 - p[:, 1]) / 0.05, 0, 1)
        base = base * (1 - 0.6 * mask[:, None])
        rgb = base
        rgb = np.where((region == REGION["jaw"])[:, None], base * 1.05, rgb)
        rgb = np.where((region == REGION["arm"])[:, None], base, rgb)
        rgb = np.where((region == REGION["tooth"])[:, None],
                       np.array([0.86, 0.80, 0.64]), rgb)
        rgb = np.where((region == REGION["eye"])[:, None],
                       np.array([0.62, 0.42, 0.08]), rgb)
        rgb = np.where((region == REGION["claw"])[:, None],
                       np.array([0.14, 0.12, 0.10]), rgb)
        return rgb
    return colour


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = theropods.measured_rig(args.capture)
    armature = cb.review_armature(rig_data, "DeinonychusEditableRig")
    dino, weights = build(rig, armature)
    cb.unwrap(dino)
    cb.bake_albedo(dino, 1024, colour_field(), args.albedo)
    cb.attach_albedo_preview(dino, args.albedo)
    extra = {"creature": "deinonychus", "donorFamily": "lizardfolk-free-hands",
             "nativeDonor": theropods.DONOR_GUID,
             "nativePrefab": theropods.PREFAB_GUID,
             "bodyRenderer": theropods.BODY_RENDERER,
             "accessoryRenderer": theropods.ACCESSORY_RENDERER,
             "nativeRendererCount": 2, "visibleLegs": 2, "visibleForelimbs": 2,
             "biped": True, "jawJoints": 2, "tailJoints": 4,
             "sickleFootClaws": 2, "massiveSkull": False, "tinyForearms": False,
             "balancedCountertail": True}
    summary = cb.write_mesh_data(args.mesh_data, dino, weights, rig_hash,
                                 args.albedo, SPACE, extra)
    if sorted(summary["bones"]) != sorted(theropods.DRIVERS):
        raise SystemExit("Deinonychus must bind exactly the reviewed 35 "
                         "drivers: " + json.dumps(summary["bones"]))
    report = {"schemaVersion": 1, "builder": "build_deinonychus.py",
              "rigSha256": rig_hash, **extra, **summary,
              "albedo": cb.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    groups = {}
    for index, row in weights.items():
        for name, value in row:
            group = groups.get(name) or dino.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    modifier = dino.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    dino.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[deinonychus] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
