"""Build the refined Dire Rat as one continuous skinned body (VIS-005).

blender -b --factory-startup --python-exit-code 1 --python build_dire_rat.py -- \
    --capture <private sprint12-dog-bind-rig.json> \
    --albedo assets/sprint12-quadrupeds/dire-rat-albedo.png \
    --mesh-data assets/sprint12-quadrupeds/dire-rat-mesh.json \
    --report <private report.json> --blend-out <private source.blend>

The Dog capture is validated by the Sprint 12 generator's own loader. Body,
legs, neck and the root of the tail are one closed surface skinned by the
bone-heat solve; the lower jaw, ears, eyes, nose, incisors, whiskers and the
naked tail tip are closed rigid features rooted inside it.
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
from generate_sprint12_quadrupeds import measured_rig  # noqa: E402

SPACE = "donor renderer local; +X right, -Y forward, +Z up"
# The exact reviewed 32 Dog controls, in the order the weights index.
BONES = (
    "Head", "Jaw", "L_Arm_Lower", "L_Arm_Upper", "L_Ear", "L_Finger0",
    "L_Leg0_Foot", "L_Leg0_Lower", "L_Leg0_Lower1", "L_Leg0_Toe0",
    "L_Leg0_Upper", "L_Palm", "LowerTorso", "Neck0", "Neck1", "Nose",
    "Pelvis", "R_Arm_Lower", "R_Arm_Upper", "R_Ear", "R_Finger0",
    "R_Leg0_Foot", "R_Leg0_Lower", "R_Leg0_Lower1", "R_Leg0_Toe0",
    "R_Leg0_Upper", "R_Palm", "Tail00", "Tail01", "Tail02", "Tail03",
    "UpperTorso")
# Rigid features own these; the body's heat solve does not.
RIGID_ONLY = {"Jaw", "L_Ear", "R_Ear"}
SIDE = Vector((1.0, 0.0, 0.0))
UP = Vector((0.0, 0.0, 1.0))
FORWARD = Vector((0.0, -1.0, 0.0))
REGION = {"body": 0, "tail": 1, "ear": 2, "eye": 3, "nose": 4, "tooth": 5,
          "whisker": 6, "jaw": 7, "paw": 8}
TAIL_BONES = {"Tail01", "Tail02", "Tail03"}
PAW_BONES = {side + name for side in ("L", "R") for name in
             ("_Palm", "_Finger0", "_Leg0_Foot", "_Leg0_Toe0")}


def build(rig, armature):
    at = lambda name: Vector(rig[name]["head"])
    pelvis, upper = at("Pelvis"), at("UpperTorso")
    neck0, neck1, head = at("Neck0"), at("Neck1"), at("Head")
    nose, jaw = at("Nose"), at("Jaw")
    tail = [at(name) for name in ("Tail00", "Tail01", "Tail02", "Tail03")]
    length = (head - pelvis).length
    # The Dog donor stands tall: hips 78 cm up and the head pivot at 112 cm.
    # A rat is long and low, so the body hangs BELOW the spine - its dorsal
    # line just clears the bones and the belly sits near the knees - leaving
    # only short lower legs in view. All measures are in the Dog's units.
    unit = length / 95.0
    u = lambda value: value * unit
    volume = cb.Volume(u(0.55))

    def mass(centre, radii):
        volume.ellipsoid(centre, (SIDE, FORWARD, UP), tuple(u(r) for r in radii))

    spine_z = upper.z
    # One continuous trunk with an arched back: highest over the haunches,
    # falling through the shoulders into the neck, over a full belly.
    volume.sweep([Vector((0.0, pelvis.y + u(16), spine_z - u(20))),
                  Vector((0.0, pelvis.y - u(4), spine_z - u(12))),
                  Vector((0.0, upper.y + u(8), spine_z - u(17))),
                  Vector((0.0, neck0.y + u(10), spine_z - u(19)))],
                 [u(22), u(30), u(27), u(23)])
    mass(Vector((0.0, upper.y + u(10), spine_z - u(31))), (22, 36, 16))
    # The skull sits below and ahead of the Dog's high head pivot so the
    # head continues the falling dorsal line instead of rising on a neck.
    skull = Vector((0.0, head.y - u(8), head.z - u(18)))
    volume.sweep([Vector((0.0, neck0.y + u(1), spine_z - u(10))),
                  Vector((0.0, neck1.y - u(1), skull.z - u(1))),
                  skull + FORWARD * u(-6)],
                 [u(20), u(16), u(14)])
    mass(skull, (14.5, 17, 13))
    snout_tip = Vector((0.0, nose.y - u(10), skull.z - u(6.5)))
    volume.sweep([skull + FORWARD * u(10), Vector((0.0, nose.y + u(3),
                  skull.z - u(3))), snout_tip], [u(11), u(7), u(3.5)])
    for prefix, sign in (("L", -1.0), ("R", 1.0)):
        # Limb roots start inside the trunk so they grow out of it.
        shoulder = Vector((sign * u(12), at(prefix + "_Arm_Upper").y + u(1),
                           spine_z - u(19)))
        front = [shoulder, at(prefix + "_Arm_Lower"), at(prefix + "_Palm"),
                 at(prefix + "_Finger0")]
        volume.sweep(front, [u(12), u(8), u(5), u(3.7)])
        hand = front[-1] + FORWARD * u(3) + UP * u(-1.5)
        volume.ellipsoid(hand, (SIDE, FORWARD, UP), (u(4.5), u(6.5), u(2.2)))
        hip = Vector((sign * u(12), at(prefix + "_Leg0_Upper").y - u(2),
                      spine_z - u(15)))
        toe = at(prefix + "_Leg0_Toe0")
        heel = at(prefix + "_Leg0_Foot")
        paw = Vector((toe.x, (heel.y + toe.y) * 0.5 - u(4), u(2.6)))
        hind = [hip, at(prefix + "_Leg0_Lower"), at(prefix + "_Leg0_Lower1"),
                heel, Vector((heel.x, heel.y - u(2), u(3.2)))]
        volume.sweep(hind, [u(14), u(9.5), u(5.4), u(4.0), u(3.3)])
        volume.ellipsoid(paw, (SIDE, FORWARD, UP), (u(4.8), u(11), u(2.5)))
    volume.sweep(tail, [u(7), u(5.5), u(4.2), u(3.4)])

    body = volume.polygonise("DireRatBody")
    cb.finish_surface(body, 4400, smooth_iterations=26)
    positions = [v.co.copy() for v in body.data.vertices]
    allowed = set(BONES) - RIGID_ONLY
    rows = cb.heat_weights(body, armature, allowed)
    rows = cb.clean_weights(rows, positions, armature, allowed)
    rows = cb.clean_weights(cb.smooth_weights(body, rows), positions,
                            armature, allowed)
    # The Dog's Pelvis sits 12 cm behind LowerTorso and the smoothed heat
    # solve leaves it under the weight floor; the haunches follow it.
    rows = cb.blend_toward(
        rows, positions,
        lambda q, row: (q - pelvis).length < u(30) and q.z > u(40),
        "Pelvis", lambda q: 0.45 * math.exp(-((q - pelvis).length / u(18)) ** 2))
    # The skull moves as one with its rigid jaw, eyes, ears and whiskers:
    # Head owns it outright, ramping in from the neck, and the snout tip
    # follows Nose with the whiskers and incisors rooted there.
    ramp = lambda value: max(0.0, min(1.0, value)) ** 2 * \
        (3.0 - 2.0 * max(0.0, min(1.0, value)))
    rows = cb.blend_toward(rows, positions, lambda q, row: q.y < head.y + u(10) and q.z > u(62),
                           "Head", lambda q: ramp((head.y + u(10) - q.y) / u(12)))
    rows = cb.blend_toward(rows, positions, lambda q, row: q.y < nose.y + u(4) and q.z > u(62),
                           "Nose", lambda q: 0.8 * ramp((nose.y + u(4) - q.y) / u(8)))
    rows = cb.clean_weights(rows, positions, armature, allowed)
    regions = cb.regions_by_weight(
        rows, REGION["body"], ((TAIL_BONES, REGION["tail"]),
                               (PAW_BONES, REGION["paw"])))
    # Fur runs down to the wrist and ankle; only the hands and feet are bare.
    for index, region in regions.items():
        if region == REGION["paw"] and positions[index].z > u(9):
            regions[index] = REGION["body"]
        # Fur covers the rump round the tail root; the tail is bare only
        # where it leaves the body.
        if region == REGION["tail"] and positions[index].y < pelvis.y + u(42):
            regions[index] = REGION["body"]
    parts = [(body, rows, regions)]

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), REGION[region]))

    # Naked, scaly tail: trailing back and drooping to the floor.
    # The curve starts gently from the last tail joint instead of kinking.
    extension = [tail[-1] - (tail[-1] - tail[-2]).normalized() * u(2),
                 Vector((0.0, tail[-1].y + u(9), tail[-1].z - u(4))),
                 Vector((0.0, tail[-1].y + u(20), tail[-1].z - u(15))),
                 Vector((0.0, tail[-1].y + u(33), tail[-1].z - u(33))),
                 Vector((0.0, tail[-1].y + u(47), u(16))),
                 Vector((0.0, tail[-1].y + u(59), u(5)))]
    rigid(cb.strand("DireRatTail", extension,
                    [u(3.6), u(3.2), u(2.7), u(2.0), u(1.2), u(0.35)],
                    segments=8),
          [("Tail03", 1.0)], "tail")
    # Lower jaw on its own control so the native bite opens the mouth.
    jaw_root = Vector((0.0, jaw.y - u(2), skull.z - u(8)))
    jaw_tip = snout_tip + FORWARD * u(-5) - UP * u(3.5)
    rigid(cb.blob("DireRatJaw", jaw_root.lerp(jaw_tip, 0.5),
                  (SIDE, (jaw_tip - jaw_root).normalized(),
                   SIDE.cross((jaw_tip - jaw_root).normalized())),
                  (u(6.5), (jaw_tip - jaw_root).length * 0.56, u(3.8))),
          [("Jaw", 1.0)], "jaw")
    for prefix, sign in (("L", -1.0), ("R", 1.0)):
        ear_name = prefix + "_Ear"
        centre = skull + SIDE * sign * u(10.5) + UP * u(13) - FORWARD * u(3)
        outward = (SIDE * sign * 0.45 + FORWARD * 0.89).normalized()
        rigid(cb.blob("DireRatEar" + prefix, centre,
                      (outward, UP.cross(outward), UP),
                      (u(1.4), u(9.5), u(10.5))), [(ear_name, 1.0)], "ear")
        eye = skull + SIDE * sign * u(10.5) + FORWARD * u(8) + UP * u(3)
        rigid(cb.blob("DireRatEye" + prefix, eye, (SIDE, FORWARD, UP),
                      (u(3.1), u(3.3), u(3.0))), [("Head", 1.0)], "eye")
        for rise in (-1.2, 0.0, 1.2):
            root = snout_tip + SIDE * sign * u(3.6) + FORWARD * u(-5) + \
                UP * u(1.6 + rise)
            whisker_tip = root + SIDE * sign * u(22) + FORWARD * u(-4) + \
                UP * u(rise * 2.5 - 1.5)
            rigid(cb.strand("DireRatWhisker", [root, root.lerp(
                whisker_tip, 0.5), whisker_tip],
                [u(0.35), u(0.22), u(0.08)], 5),
                [("Nose", 1.0)], "whisker")
        incisor = snout_tip + SIDE * sign * u(0.9) + FORWARD * u(-1.5) - \
            UP * u(1.5)
        rigid(cb.strand("DireRatIncisor", [incisor + UP * u(1.5),
                                           incisor - UP * u(3.5)],
                        [u(0.8), u(0.55)], 6),
              [("Nose", 1.0)], "tooth")
    rigid(cb.blob("DireRatNose", snout_tip + FORWARD * u(1.2),
                  (SIDE, FORWARD, UP), (u(2.2), u(1.8), u(1.9))),
          [("Nose", 1.0)], "nose")
    rat, weights = cb.assemble("DireRat", parts)
    cb.require_single_body(rat, REGION["body"])
    landmarks = {"pelvis": pelvis, "head": head, "nose": nose,
                 "tailRoot": tail[0], "girth": u(26)}
    return rat, weights, landmarks


def colour_field(landmarks):
    girth = landmarks["girth"]
    tail_root = np.array(landmarks["tailRoot"])

    def colour(p, n, region):
        up = n[:, 2]
        fur = cb.fbm(p, 1.0 / (0.09 * girth), 11)
        streak = cb.fbm(p * np.array([3.0, 0.5, 3.0]), 1.0 / (0.25 * girth), 12)
        # Brown-grey agouti back, lighter flanks and a cream-grey belly.
        dorsal = np.array([0.45, 0.39, 0.33])
        flank = np.array([0.55, 0.49, 0.42])
        belly = np.array([0.74, 0.69, 0.62])
        t = np.clip(up * 0.5 + 0.5, 0.0, 1.0)[:, None]
        base = np.where(t > 0.55, flank + (dorsal - flank) * ((t - 0.55) / 0.45),
                        belly + (flank - belly) * (t / 0.55))
        base = base * (0.84 + 0.28 * fur[:, None]) * \
            (0.93 + 0.12 * streak[:, None])
        skin = np.array([0.76, 0.60, 0.56]) * \
            (0.92 + 0.12 * cb.value_noise(p, 1.0 / (0.03 * girth), 13))[:, None]
        along = np.linalg.norm(p - tail_root, axis=1)
        ring = 0.5 + 0.5 * np.sin(along / (0.035 * girth) * math.pi)
        tail_skin = np.array([0.70, 0.56, 0.53]) * (0.88 + 0.12 * ring)[:, None]
        rgb = base
        for key, value in (("tail", tail_skin), ("paw", skin)):
            rgb = np.where((region == REGION[key])[:, None], value, rgb)
        ear = np.array([0.74, 0.55, 0.53]) * (0.85 + 0.2 * fur)[:, None]
        rgb = np.where((region == REGION["ear"])[:, None], ear, rgb)
        rgb = np.where((region == REGION["eye"])[:, None],
                       np.array([0.03, 0.025, 0.025]), rgb)
        rgb = np.where((region == REGION["nose"])[:, None],
                       np.array([0.78, 0.52, 0.52]), rgb)
        rgb = np.where((region == REGION["tooth"])[:, None],
                       np.array([0.88, 0.72, 0.36]), rgb)
        rgb = np.where((region == REGION["whisker"])[:, None],
                       np.array([0.20, 0.18, 0.17]), rgb)
        # A light chin, but darker than the belly so the closed mouth does
        # not read as a pale strip.
        jaw = (belly * 0.55 + flank * 0.45) * (0.86 + 0.18 * fur[:, None])
        rgb = np.where((region == REGION["jaw"])[:, None], jaw, rgb)
        return rgb
    return colour


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = measured_rig(args.capture, "dire-rat")
    armature = cb.review_armature(rig_data, "DireRatRigPreview")
    rat, weights, landmarks = build(rig, armature)
    cb.unwrap(rat)
    cb.bake_albedo(rat, 1024, colour_field(landmarks), args.albedo)
    cb.attach_albedo_preview(rat, args.albedo)
    summary = cb.write_mesh_data(args.mesh_data, rat, weights, rig_hash,
                                 args.albedo, SPACE)
    if tuple(summary["bones"]) != BONES:
        raise SystemExit("Dire Rat must bind exactly the reviewed 32 Dog "
                         "controls: " + json.dumps(summary["bones"]))
    report = {"schemaVersion": 1, "creature": "dire-rat",
              "builder": "build_dire_rat.py", "donorFamily": "dog",
              "rigSha256": rig_hash, **summary,
              "albedo": cb.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    groups = {}
    for index, row in weights.items():
        for name, value in row:
            group = groups.get(name) or rat.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    modifier = rat.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    rat.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[dire-rat] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
