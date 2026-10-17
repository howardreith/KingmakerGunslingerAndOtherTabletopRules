"""Build the refined Xill as one continuous skinned body (VIS-030).

blender -b --factory-startup --python-exit-code 1 --python build_xill.py -- \
    --kind xill --capture <private sprint18-primate-donor-census.json> \
    --albedo assets/sprint19-fourarmed/xill-albedo.png \
    --mesh-data assets/sprint19-fourarmed/xill-mesh.json \
    --report <private report.json> --blend-out <private source.blend>

The same Troll guard frame as the apes (`../sprint18-primates/`). The owner
found the first Xill's fingers detached-looking and its anatomy wiry and
visibly polygonal, and it shipped inside out. The Xill is now one continuous
chitinous body: a tall banded thorax over a ridged, narrower abdomen, a
short neck, a forward wedge of a head with two large and two small green
compound eyes, brow plates and inward-curving mandibles on the jaw, fuller
upper arms grown out of the shoulders, and digitigrade legs. The lower pair
of arms are smooth closed tubes rooted inside the thorax, carried below and
outboard of the upper pair on the same drivers
(FOUR_ARMS_SHARE_TWO_DRIVER_CHAINS, as before). All four hands have a thumb
and three fingers rooted in the palm, with near-black claws. The coat keeps
the first Xill's lacquered rust-red carapace and dark segment banding. Every
one of the 56 reviewed drivers carries geometry and nothing else does.
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
sys.path.insert(0, str(HERE.parent / "sprint18-primates"))
import continuous_body as cb  # noqa: E402
import primate_capture as capture  # noqa: E402
import build_primates as apes  # noqa: E402

KINDS = ("xill",)
SPACE = apes.SPACE
WINDING = apes.WINDING
SIDE, UP, FORWARD = apes.SIDE, apes.UP, apes.FORWARD
REGION = apes.REGION
BULK = 0.86
# The lower pair, relative to the upper: below, outboard and forward.
LOWER_DROP, LOWER_SPREAD, LOWER_FORWARD, LOWER_SCALE = 0.78, 0.26, 0.30, 0.90


def lower_offset(sign):
    return Vector((sign * LOWER_SPREAD, -LOWER_DROP, -LOWER_FORWARD))


def build(rig, armature):
    shape = dict(dire=False, bulk=BULK, claw=0.15, canine=0.0, crest=0.0,
                 collar=False, brow=0.08)
    b = BULK
    at = lambda name: Vector(rig[name]["head"])
    volume = cb.Volume(0.024)
    axes = (SIDE, UP, FORWARD)
    # A tall thorax over a narrower, ridged abdomen.
    volume.ellipsoid(Vector((0, 3.42, 0.24)), axes, (0.66 * b, 0.70 * b, 0.52 * b))
    volume.sweep([Vector((0, 2.22, 0.46)), Vector((0, 2.60, 0.34)),
                  Vector((0, 3.00, 0.28))], [0.50 * b, 0.42 * b, 0.52 * b])
    for index in range(4):
        y = 2.24 + index * 0.21
        volume.ellipsoid(Vector((0, y, 0.40 - index * 0.03)), axes,
                         ((0.56 - index * 0.03) * b, 0.12 * b,
                          (0.46 - index * 0.02) * b))
    # The neck and the forward wedge of the head.
    volume.sweep([Vector((0, 3.88, 0.12)), Vector((0, 4.18, -0.24)),
                  Vector((0, 4.36, -0.58))], [0.34 * b, 0.28 * b, 0.26 * b])
    volume.ellipsoid(Vector((0, 4.48, -0.90)), axes, (0.40 * b, 0.34 * b, 0.44 * b))
    volume.sweep([Vector((0, 4.44, -1.00)), Vector((0, 4.30, -1.28)),
                  Vector((0, 4.14, -1.44))], [0.30 * b, 0.22 * b, 0.13 * b])
    volume.sweep([Vector((0.24, 4.60, -1.16)), Vector((0, 4.64, -1.24)),
                  Vector((-0.24, 4.60, -1.16))], [shape["brow"]] * 3)
    for side, sign in (("L_", 1.0), ("R_", -1.0)):
        volume.ellipsoid(Vector((sign * 0.70, 3.86, 0.12)), axes,
                         (0.30 * b, 0.30 * b, 0.30 * b))
        shoulder, elbow = at(side + "Up_Arm_01"), at(side + "Forearm_01")
        twist, wrist = at(side + "Forearm_02"), at(side + "Hand_01")
        volume.sweep([Vector((sign * 0.48, 3.92, 0.12)), shoulder,
                      shoulder.lerp(elbow, 0.5), elbow, elbow.lerp(twist, 0.5),
                      twist, wrist],
                     [r * b for r in (0.30, 0.28, 0.25, 0.21, 0.20, 0.17, 0.15)])
        knuckles = sum((at(side + d + "_01") for d in apes.DIGITS[1:]),
                       Vector()) / 3.0
        volume.sweep([wrist, wrist.lerp(knuckles, 0.55), knuckles],
                     [0.16 * b, 0.15 * b, 0.13 * b])
        hip, knee = at(side + "UpLeg_01"), at(side + "Leg_01")
        ankle, toe = at(side + "Foot_01"), at(side + "Foot_Toe_01")
        volume.sweep([Vector((sign * 0.36, 2.28, 0.40)), hip,
                      hip.lerp(knee, 0.5), knee],
                     [r * b for r in (0.38, 0.34, 0.28, 0.21)])
        volume.sweep([knee, knee.lerp(ankle, 0.5), ankle],
                     [r * b for r in (0.20, 0.16, 0.13)])
        volume.sweep([ankle + Vector((0, -0.16, 0.04)),
                      Vector((ankle.x, 0.20, -0.08)),
                      Vector((toe.x, 0.15, toe.z - 0.04))],
                     [0.14 * b, 0.13 * b, 0.10 * b])

    body = volume.polygonise("XillBody")
    cb.finish_surface(body, 8000, smooth_iterations=10)
    positions = [v.co.copy() for v in body.data.vertices]
    allowed = set(apes.heat_bones())
    heat = cb.heat_weights(body, armature, allowed)
    heat = cb.clean_weights(heat, positions, armature, allowed)
    heat = cb.clean_weights(cb.smooth_weights(body, heat), positions,
                            armature, allowed)
    smooth = apes.smooth
    heat = cb.blend_toward(heat, positions,
                           lambda q, row: q.z < -0.40 and q.y > 4.05,
                           "Head", lambda q: smooth((-0.40 - q.z) / 0.30))
    for side, sign in (("L_", 1.0), ("R_", -1.0)):
        heat = cb.blend_toward(
            heat, positions,
            lambda q, row, s=sign: q.x * s > 0.02 and q.z < -1.20 and
            4.08 < q.y < 4.30,
            side + "Up_lip_01", lambda q: 0.55)
        heat = cb.blend_toward(
            heat, positions,
            lambda q, row, s=sign: q.x * s > 0.02 and q.z < -1.05 and
            q.y > 4.55 and abs(q.x) < 0.36,
            side + "Eyebrow_01", lambda q: 0.55)
        heat = cb.blend_toward(
            heat, positions,
            lambda q, row, s=sign: q.x * s > 0.22 and 3.85 < q.y < 4.20 and
            -0.10 < q.z < 0.40 and abs(q.x) < 0.65,
            side + "Clavicle_01", lambda q: 0.35)
        twist, wrist = at(side + "Forearm_02"), at(side + "Hand_01")
        heat = cb.blend_toward(
            heat, positions,
            lambda q, row, a=twist, w=wrist: (q - a.lerp(w, 0.3)).length < 0.26,
            side + "Forearm_02", lambda q: 0.45)
        toe = at(side + "Foot_Toe_01")
        heat = cb.blend_toward(
            heat, positions,
            lambda q, row, t=toe: q.y < 0.36 and q.z < t.z + 0.22 and
            abs(q.x - t.x) < 0.28,
            side + "Foot_Toe_01", lambda q: 0.7)
    heat = cb.blend_toward(
        heat, positions,
        lambda q, row: q.z < -0.10 and 2.20 < q.y < 3.00 and abs(q.x) < 0.55,
        "Stomach_01", lambda q: 0.45)
    face_bones = {"Head", "Stomach_01"} | {s + n for s in ("L_", "R_") for n in
                                           ("Up_lip_01", "Eyebrow_01")}
    rows = cb.clean_weights(heat, positions, armature, allowed | face_bones)
    parts = [(body, rows, REGION["body"])]

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), REGION[region]))

    # The jaw plate, mandibles, compound eyes and ear spines.
    rigid(cb.blob("XillJaw", Vector((0, 4.06, -1.20)), axes,
                  (0.20, 0.08, 0.22), 16, 10), [("Jaw_01", 1.0)], "jaw")
    for side, sign in (("L_", 1.0), ("R_", -1.0)):
        root = Vector((sign * 0.17, 4.08, -1.32))
        rigid(cb.horn("XillMandible", root,
                      root + Vector((-sign * 0.16, -0.18, -0.30)), 0.055, 10, 6,
                      curve=Vector((-sign * 0.05, 0, -0.02)), flatten=0.6,
                      hint=UP), [("Jaw_01", 1.0)], "claw")
        rigid(cb.blob("XillEye", Vector((sign * 0.24, 4.47, -1.24)), axes,
                      (0.12, 0.10, 0.09), 14, 10), [("Head", 1.0)], "eye")
        rigid(cb.blob("XillEye", Vector((sign * 0.30, 4.68, -1.02)), axes,
                      (0.075, 0.07, 0.065), 12, 8), [("Head", 1.0)], "eye")
        ear = at(side + "Ear_01")
        rigid(cb.horn("XillEarSpine", ear + SIDE * (-sign * 0.10),
                      ear + SIDE * (sign * 0.10) + UP * 0.20 + FORWARD * -0.18,
                      0.06, 8, 5, hint=FORWARD), [(side + "Ear_01", 1.0)],
              "claw")
        apes.add_hand_and_digits(parts, rig, side, shape, REGION["hand"])
        # The lower arm: the upper arm's line carried below, outboard and
        # forward, rooted inside the thorax, on the same drivers.
        offset = lower_offset(sign)
        shoulder, elbow = at(side + "Up_Arm_01"), at(side + "Forearm_01")
        twist, wrist = at(side + "Forearm_02"), at(side + "Hand_01")
        line = [Vector((sign * 0.30, 3.18, 0.10)), shoulder + offset,
                elbow + offset, twist + offset, wrist + offset]
        s = LOWER_SCALE * b
        arm = cb.tube("XillLowerArm", line,
                      [0.26 * s, 0.25 * s, 0.20 * s, 0.17 * s, 0.15 * s], 12)
        parts.append((arm, cb.chain_rows(arm, line,
                                         [side + "Clavicle_01",
                                          side + "Up_Arm_01",
                                          side + "Forearm_01",
                                          side + "Forearm_02"]),
                      REGION["body"]))
        knuckles = sum((at(side + d + "_01") for d in apes.DIGITS[1:]),
                       Vector()) / 3.0 + offset
        palm = cb.tube("XillLowerPalm", [wrist + offset,
                                         (wrist + offset).lerp(knuckles, 0.55),
                                         knuckles],
                       [0.15 * s, 0.145 * s, 0.12 * s], 10)
        parts.append((palm, cb.rigid_rows(palm, [(side + "Hand_01", 1.0)]),
                      REGION["body"]))
        apes.add_hand_and_digits(parts, rig, side, shape, REGION["hand"],
                                 offset=offset, scale=LOWER_SCALE)
    xill, weights = cb.assemble("Xill", parts)
    cb.require_single_body(xill, REGION["body"])
    return xill, weights


def colour_field():
    shell = np.array((0.46, 0.145, 0.072))
    deep = np.array((0.195, 0.058, 0.030))
    mantle = np.array((0.58, 0.215, 0.095))
    membrane = np.array((0.165, 0.062, 0.040))
    keratin = np.array((0.085, 0.072, 0.068))
    iris = np.array((0.07, 0.46, 0.17))

    def colour(p, n, region):
        y = p[:, 1]
        up = np.clip(n[:, 1] * 0.5 + 0.5, 0, 1)[:, None]
        jitter = (cb.fbm(p, 1.0 / 0.15, 111) - 0.5) * 0.10
        # Hard-edged lacquered segment banding: bands along the height.
        phase = np.mod((y + jitter) * 3.6, 1.0)
        band = np.clip((phase - 0.80) / 0.05, 0, 1) * np.clip((0.97 - phase) / 0.05, 0, 1)
        base = shell + (mantle - shell) * up
        rgb = base + (deep - base) * band[:, None]
        sheen = cb.fbm(p, 1.0 / 0.08, 112)
        rgb = rgb * (0.88 + 0.20 * sheen[:, None])
        rgb = np.where((region == REGION["hand"])[:, None],
                       np.where((n[:, 1] < -0.2)[:, None], membrane * 1.6, rgb), rgb)
        rgb = np.where((region == REGION["jaw"])[:, None], membrane * 1.5, rgb)
        rgb = np.where((region == REGION["eye"])[:, None], iris, rgb)
        rgb = np.where((region == REGION["claw"])[:, None], keratin, rgb)
        return rgb
    return colour


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", required=True, choices=KINDS)
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = capture.load(args.capture)
    armature = cb.review_armature(rig_data, "XillEditableRig")
    xill, weights = build(rig, armature)
    cb.unwrap(xill)
    cb.bake_albedo(xill, 1024, colour_field(), args.albedo)
    cb.attach_albedo_preview(xill, args.albedo)
    extra = {"triangleWinding": WINDING, "creature": "xill",
             "donorBlueprint": capture.BLUEPRINT, "donorPrefab": capture.PREFAB,
             "donorRenderer": capture.BODY_RENDERER, "visibleLimbs": 6,
             "visibleArms": 4, "armDriverChains": 2,
             "lowerArmsShareUpperArmDrivers": True, "clawedHands": True,
             "printedSize": "Medium",
             "authoredLimitation": "FOUR_ARMS_SHARE_TWO_DRIVER_CHAINS"}
    summary = cb.write_mesh_data(args.mesh_data, xill, weights, rig_hash,
                                 args.albedo, SPACE, extra)
    if sorted(summary["bones"]) != sorted(capture.DRIVERS):
        raise SystemExit("the body must bind exactly the 56 reviewed drivers: " +
                         json.dumps(sorted(set(capture.DRIVERS) ^
                                           set(summary["bones"]))))
    report = {"schemaVersion": 1, "builder": "build_xill.py",
              "rigSha256": rig_hash, **extra, **summary,
              "albedo": cb.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    groups = {}
    for index, row in weights.items():
        for name, value in row:
            group = groups.get(name) or xill.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    modifier = xill.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    xill.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[xill] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
