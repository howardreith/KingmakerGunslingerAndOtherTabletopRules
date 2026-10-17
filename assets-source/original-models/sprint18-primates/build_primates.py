"""Build the refined Ape and Dire Ape as continuous skinned bodies.

blender -b --factory-startup --python-exit-code 1 --python build_primates.py -- \
    --kind ape|dire-ape --capture <private sprint18-primate-donor-census.json> \
    --albedo assets/sprint18-primates/KEY-albedo.png \
    --mesh-data assets/sprint18-primates/KEY-mesh.json \
    --report <private report.json> --blend-out <private source.blend>

The census is validated by `primate_capture.load`; the frame is the Troll
guard's (+X left, +Y up, -Z forward), hunched, with long arms whose hands
hang below the knees. The owner found the first Ape very dark, crude and hard
to recognise, with exaggerated teeth and an earth-elemental impression
(VIS-019), and the Dire Ape better coloured and detailed but still weak as a
species (VIS-025); both shipped inside out. Each is now one continuous body:
a massive chest and shoulders with a high back, a pot belly, short thick
neck, a domed skull with a brow ridge, a projecting muzzle and a separate
jaw, small ears, long heavy arms ending in palms with a thumb and three
fingers rooted in them, and thick bent legs with broad feet. The Ape wears a
lighter dark-grey coat with a bare chest and face and a silver saddle and has
small canines and nails; the Dire Ape keeps its approved russet coat, straw
mane and crest, weathered skin, yellowed canines and horn claws. Every one
of the 56 reviewed drivers carries geometry and nothing else does.
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
import primate_capture as capture  # noqa: E402

KINDS = ("ape", "dire-ape")
SPACE = "donor renderer local; +X left, +Y up, -Z forward"
WINDING = "outward-visual-refinement"
SIDE = Vector((1.0, 0.0, 0.0))      # the creature's left
UP = Vector((0.0, 1.0, 0.0))
FORWARD = Vector((0.0, 0.0, -1.0))
REGION = {"body": 0, "hand": 1, "eye": 2, "tooth": 3, "claw": 4, "ear": 5,
          "jaw": 6}
DIGITS = ("Thumb", "Fore_Finger", "Midle_Finger", "Little_Finger")


def profile(kind):
    dire = kind == "dire-ape"
    return dict(dire=dire, bulk=1.12 if dire else 1.0,
                crest=0.15 if dire else 0.07, collar=dire,
                canine=0.16 if dire else 0.06, claw=0.20 if dire else 0.0,
                brow=0.125 if dire else 0.11)


def heat_bones():
    torso = ["Pelvis", "Spine_01", "Spine_02", "Spine_03", "Neck_01", "Head",
             "Stomach_01"]
    for side in ("L_", "R_"):
        torso += [side + n for n in ("Clavicle_01", "Up_Arm_01", "Forearm_01",
                                     "Forearm_02", "Hand_01", "UpLeg_01",
                                     "Leg_01", "Foot_01", "Foot_Toe_01")]
    return torso


def smooth(t):
    t = max(0.0, min(1.0, t))
    return t * t * (3.0 - 2.0 * t)


def add_hand_and_digits(parts, rig, side, shape, region, offset=None,
                        scale=1.0):
    """Fingers and thumb as closed tubes rooted inside the palm, on the
    donor's own finger chains, so no digit floats off the hand. `offset`
    displaces the whole hand (the Xill's lower pair)."""
    at = lambda n: Vector(rig[side + n]["head"]) + (offset or Vector())
    wrist = at("Hand_01")
    knuckles = sum((at(d + "_01") for d in DIGITS[1:]), Vector()) / 3.0
    for digit in DIGITS:
        f1, f2, f3 = (at(digit + "_0%d" % i) for i in (1, 2, 3))
        root = wrist.lerp(knuckles, 0.45) if digit != "Thumb" else \
            wrist.lerp(f1, 0.35)
        tip = f3 + (f3 - f2) * (0.35 if digit != "Thumb" else 0.30)
        points = [root, f1, f2, f3, tip]
        width = (0.11 if digit == "Thumb" else 0.10) * shape["bulk"] * scale
        obj = cb.tube("PrimateDigit", points,
                      [width * 1.1, width, width * 0.86, width * 0.74,
                       width * 0.55], 10)
        parts.append((obj, cb.chain_rows(obj, points,
                                         [side + "Hand_01", side + digit + "_01",
                                          side + digit + "_02",
                                          side + digit + "_03"]),
                      region))
        if shape["claw"]:
            direction = (tip - f3).normalized()
            claw = cb.horn("PrimateClaw", tip - direction * 0.04,
                           tip + direction * shape["claw"] * scale -
                           UP * 0.05 * scale, width * 0.62, 10, 5, hint=SIDE)
            parts.append((claw, cb.rigid_rows(claw, [(side + digit + "_03", 1.0)]),
                          REGION["claw"]))
        else:
            nail = cb.blob("PrimateNail", tip - (tip - f3).normalized() * 0.02,
                           (SIDE, UP, FORWARD), (width * 0.55,) * 3, 10, 6)
            parts.append((nail, cb.rigid_rows(nail, [(side + digit + "_03", 1.0)]),
                          REGION["claw"]))


def build(rig, armature, kind):
    shape = profile(kind)
    b = shape["bulk"]
    at = lambda name: Vector(rig[name]["head"])
    volume = cb.Volume(0.028)
    axes = (SIDE, UP, FORWARD)
    head = at("Head")
    # Torso: massive chest and shoulders, high back, pot belly, hips.
    volume.ellipsoid(Vector((0, 3.45, 0.16)), axes, (0.92 * b, 0.78 * b, 0.72 * b))
    volume.ellipsoid(Vector((0, 2.70, 0.00)), axes, (0.72 * b, 0.62 * b, 0.64 * b))
    volume.ellipsoid(Vector((0, 2.30, 0.44)), axes, (0.62 * b, 0.42 * b, 0.46 * b))
    volume.sweep([Vector((0, 2.45, 0.62)), Vector((0, 3.20, 0.80)),
                  Vector((0, 3.92, 0.48))], [0.48 * b, 0.62 * b, 0.56 * b])
    # A short, thick neck carrying the head forward.
    volume.sweep([Vector((0, 3.88, 0.12)), Vector((0, 4.16, -0.26)),
                  Vector((0, 4.36, -0.66))], [0.62 * b, 0.52 * b, 0.42 * b])
    # The skull, brow ridge, muzzle and nose; a sagittal crest.
    volume.ellipsoid(Vector((0, 4.47, -0.94)), axes, (0.46, 0.44, 0.46))
    volume.sweep([Vector((0.30, 4.52, -1.27)), Vector((0, 4.56, -1.36)),
                  Vector((-0.30, 4.52, -1.27))], [shape["brow"]] * 3)
    volume.ellipsoid(Vector((0, 4.20, -1.28)), axes, (0.34, 0.25, 0.24))
    volume.ellipsoid(Vector((0, 4.31, -1.45)), axes, (0.12, 0.08, 0.06))
    volume.sweep([Vector((0, 4.74, -1.02)), Vector((0, 4.83, -0.74)),
                  Vector((0, 4.72, -0.46))],
                 [shape["crest"], shape["crest"] * 1.1, shape["crest"] * 0.8])
    for side, sign in (("L_", 1.0), ("R_", -1.0)):
        # Shoulders and a long, heavy arm to the wrist.
        volume.ellipsoid(Vector((sign * 0.82, 3.86, 0.12)), axes,
                         (0.42 * b, 0.40 * b, 0.42 * b))
        shoulder, elbow = at(side + "Up_Arm_01"), at(side + "Forearm_01")
        twist, wrist = at(side + "Forearm_02"), at(side + "Hand_01")
        volume.sweep([Vector((sign * 0.55, 3.95, 0.12)), shoulder,
                      shoulder.lerp(elbow, 0.5), elbow, elbow.lerp(twist, 0.5),
                      twist, wrist],
                     [r * b for r in (0.44, 0.42, 0.37, 0.32, 0.31, 0.27, 0.22)])
        # The palm, from the wrist to the knuckles.
        knuckles = sum((at(side + d + "_01") for d in DIGITS[1:]), Vector()) / 3.0
        volume.sweep([wrist, wrist.lerp(knuckles, 0.55), knuckles],
                     [0.22 * b, 0.21 * b, 0.18 * b])
        # Thick bent legs and broad feet.
        hip, knee = at(side + "UpLeg_01"), at(side + "Leg_01")
        ankle, toe = at(side + "Foot_01"), at(side + "Foot_Toe_01")
        volume.sweep([Vector((sign * 0.40, 2.30, 0.40)), hip,
                      hip.lerp(knee, 0.5), knee],
                     [r * b for r in (0.50, 0.46, 0.38, 0.28)])
        volume.sweep([knee, knee.lerp(ankle, 0.5), ankle],
                     [r * b for r in (0.27, 0.22, 0.18)])
        volume.sweep([ankle + Vector((0, -0.18, 0.05)),
                      Vector((ankle.x, 0.22, -0.08)),
                      Vector((toe.x, 0.17, toe.z - 0.06))],
                     [0.19 * b, 0.18 * b, 0.15 * b])
        volume.ellipsoid(Vector((ankle.x, 0.20, 0.20)), axes,
                         (0.16 * b, 0.13 * b, 0.20 * b))

    body = volume.polygonise(kind + "Body")
    cb.finish_surface(body, 9000, smooth_iterations=10)
    positions = [v.co.copy() for v in body.data.vertices]
    allowed = set(heat_bones())
    heat = cb.heat_weights(body, armature, allowed)
    heat = cb.clean_weights(heat, positions, armature, allowed)
    heat = cb.clean_weights(cb.smooth_weights(body, heat), positions,
                            armature, allowed)

    # The head and face, forward of the neck, ride Head; lips and brows
    # share their own small drivers; the belly front shares Stomach_01.
    heat = cb.blend_toward(heat, positions,
                           lambda q, row: q.z < -0.40 and q.y > 4.05,
                           "Head", lambda q: smooth((-0.40 - q.z) / 0.30))
    for side, sign in (("L_", 1.0), ("R_", -1.0)):
        heat = cb.blend_toward(
            heat, positions,
            lambda q, row, s=sign: q.x * s > 0.02 and q.z < -1.24 and
            4.06 < q.y < 4.27,
            side + "Up_lip_01", lambda q: 0.55)
        heat = cb.blend_toward(
            heat, positions,
            lambda q, row, s=sign: q.x * s > 0.02 and q.z < -1.18 and
            q.y > 4.47 and abs(q.x) < 0.42,
            side + "Eyebrow_01", lambda q: 0.55)
        heat = cb.blend_toward(
            heat, positions,
            lambda q, row, s=sign: q.x * s > 0.25 and 3.85 < q.y < 4.25 and
            -0.10 < q.z < 0.45 and abs(q.x) < 0.75,
            side + "Clavicle_01", lambda q: 0.35)
        twist, wrist = at(side + "Forearm_02"), at(side + "Hand_01")
        heat = cb.blend_toward(
            heat, positions,
            lambda q, row, a=twist, w=wrist: (q - a.lerp(w, 0.3)).length < 0.32,
            side + "Forearm_02", lambda q: 0.45)
        toe = at(side + "Foot_Toe_01")
        heat = cb.blend_toward(
            heat, positions,
            lambda q, row, t=toe: q.y < 0.40 and q.z < t.z + 0.25 and
            abs(q.x - t.x) < 0.35,
            side + "Foot_Toe_01", lambda q: 0.7)
    heat = cb.blend_toward(
        heat, positions,
        lambda q, row: q.z < -0.30 and 2.25 < q.y < 3.05 and abs(q.x) < 0.70,
        "Stomach_01", lambda q: 0.45)
    face_bones = {"Head", "Stomach_01"} | {s + n for s in ("L_", "R_") for n in
                                           ("Up_lip_01", "Eyebrow_01")}
    rows = cb.clean_weights(heat, positions, armature, allowed | face_bones)
    parts = [(body, rows, REGION["body"])]

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), REGION[region]))

    # The separate lower jaw, the canines, eyes and ears.
    rigid(cb.blob("PrimateJaw", Vector((0, 4.03, -1.18)), axes,
                  (0.27, 0.11, 0.25), 18, 10), [("Jaw_01", 1.0)], "jaw")
    for side, sign in (("L_", 1.0), ("R_", -1.0)):
        root = Vector((sign * 0.13, 4.10, -1.40))
        rigid(cb.horn("PrimateCanine", root,
                      root - UP * shape["canine"] + FORWARD * 0.01,
                      0.035 if not shape["dire"] else 0.045, 8, 5, hint=FORWARD),
              [("Head", 1.0)], "tooth")
        rigid(cb.blob("PrimateEye", Vector((sign * 0.17, 4.42, -1.32)), axes,
                      (0.06, 0.06, 0.05), 12, 8), [("Head", 1.0)], "eye")
        rigid(cb.blob("PrimateEar", at(side + "Ear_01") + SIDE * (sign * -0.04),
                      axes, (0.05, 0.11, 0.09), 12, 8),
              [(side + "Ear_01", 1.0)], "ear")
        add_hand_and_digits(parts, rig, side, shape, REGION["hand"])
    ape, weights = cb.assemble(kind, parts)
    cb.require_single_body(ape, REGION["body"])
    return ape, weights


COATS = {
    # Lifted from the first coat's near-black (0.085) so the form reads.
    "ape": dict(fur=(0.33, 0.30, 0.28), tips=(0.44, 0.41, 0.38),
                skin=(0.23, 0.20, 0.19), saddle=(0.66, 0.66, 0.68),
                mane=(0.33, 0.30, 0.28), keratin=(0.20, 0.18, 0.17),
                tooth=(0.80, 0.78, 0.70), iris=(0.21, 0.125, 0.065)),
    # The Dire Ape's approved coat, kept from its first painting.
    "dire-ape": dict(fur=(0.225, 0.120, 0.062), tips=(0.30, 0.17, 0.09),
                     skin=(0.145, 0.120, 0.108), saddle=(0.40, 0.29, 0.17),
                     mane=(0.62, 0.50, 0.30), keratin=(0.62, 0.56, 0.44),
                     tooth=(0.84, 0.79, 0.62), iris=(0.30, 0.165, 0.070)),
}


def colour_field(kind):
    coat = {k: np.array(v) for k, v in COATS[kind].items()}
    dire = kind == "dire-ape"

    def colour(p, n, region):
        x, y, z = p[:, 0], p[:, 1], p[:, 2]
        up = np.clip(n[:, 1] * 0.5 + 0.5, 0, 1)[:, None]
        streak = cb.fbm(p * np.array([3.0, 0.6, 3.0]), 1.0 / 0.035, 101)
        broad = cb.fbm(p, 1.0 / 0.5, 102)
        fur = coat["fur"] + (coat["tips"] - coat["fur"]) * up
        fur = fur * (0.80 + 0.30 * streak[:, None]) * (0.90 + 0.16 * broad[:, None])
        # Every marking is a soft field broken by noise: hard boxes read as
        # blocky patches on the body.
        jitter = (cb.fbm(p, 1.0 / 0.12, 104) - 0.5) * 0.16

        def ramp(value, low, high):
            t = np.clip((value - low) / (high - low), 0, 1)
            return t * t * (3 - 2 * t)

        # Bare face and, on the Ape, a bare oval chest.
        face = ramp(-z + jitter, 1.04, 1.20) * ramp(y, 3.92, 4.02) * \
            (1 - ramp(y + jitter, 4.56, 4.66))
        chest = 0.0 if dire else \
            np.clip(1 - (x / 0.50) ** 2 - ((y + jitter - 3.38) / 0.46) ** 2, 0, 1) ** 0.6 * \
            ramp(-n[:, 2], 0.15, 0.45)
        bare = np.clip(face + chest, 0, 1)[:, None]
        skin = coat["skin"] * (0.85 + 0.25 * cb.fbm(p, 1.0 / 0.06, 103)[:, None])
        # Nostrils on the nose pad and the line of the mouth.
        nostrils = sum(np.exp(-(((x - s) / 0.035) ** 2 + ((y - 4.30) / 0.03) ** 2))
                       for s in (0.065, -0.065)) * ramp(-z, 1.40, 1.48)
        mouth = np.exp(-((y - 4.06) / 0.022) ** 2) * ramp(-z, 1.22, 1.32)
        skin = skin * (1 - 0.65 * np.clip(nostrils + mouth, 0, 1))[:, None]
        rgb = fur + (skin - fur) * bare
        # The saddle across the lower back.
        saddle = np.exp(-((y + jitter - 2.85) / 0.36) ** 2) * ramp(n[:, 2], 0.10, 0.45)
        rgb = rgb + (coat["saddle"] - rgb) * (saddle * 0.8)[:, None]
        if dire:
            # Straw mane: the crest and a ruff over the neck and shoulders.
            crest = ramp(y + jitter, 4.56, 4.70) * ramp(z, -1.20, -1.05)
            ruff = ramp(y + jitter, 3.70, 3.92) * (1 - ramp(y + jitter, 4.30, 4.46)) * \
                ramp(z + jitter, -0.55, -0.30) * (1 - ramp(np.abs(x), 0.90, 1.15))
            mane = np.clip(crest + ruff, 0, 1)[:, None]
            rgb = rgb + (coat["mane"] * (0.85 + 0.25 * streak[:, None]) - rgb) * mane
        rgb = np.where((region == REGION["hand"])[:, None],
                       np.where((n[:, 1] < -0.2)[:, None], coat["skin"] * 1.25,
                                fur), rgb)
        rgb = np.where((region == REGION["jaw"])[:, None],
                       np.where((n[:, 1] < 0.3)[:, None], fur, coat["skin"]), rgb)
        rgb = np.where((region == REGION["ear"])[:, None], coat["skin"] * 1.1, rgb)
        rgb = np.where((region == REGION["eye"])[:, None], coat["iris"] * 0.6, rgb)
        rgb = np.where((region == REGION["tooth"])[:, None], coat["tooth"], rgb)
        rgb = np.where((region == REGION["claw"])[:, None], coat["keratin"], rgb)
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
    armature = cb.review_armature(rig_data, args.kind + "EditableRig")
    ape, weights = build(rig, armature, args.kind)
    cb.unwrap(ape)
    cb.bake_albedo(ape, 1024, colour_field(args.kind), args.albedo)
    cb.attach_albedo_preview(ape, args.albedo)
    shape = profile(args.kind)
    extra = {"triangleWinding": WINDING, "creature": args.kind,
             "donorBlueprint": capture.BLUEPRINT, "donorPrefab": capture.PREFAB,
             "donorRenderer": capture.BODY_RENDERER, "tailGeometry": False,
             "tongueGeometry": False, "jawSeparated": True, "visibleLimbs": 4,
             "clawedHands": shape["dire"], "opposableThumbs": True,
             "knuckleWalkAuthored": False}
    summary = cb.write_mesh_data(args.mesh_data, ape, weights, rig_hash,
                                 args.albedo, SPACE, extra)
    if sorted(summary["bones"]) != sorted(capture.DRIVERS):
        raise SystemExit("the body must bind exactly the 56 reviewed drivers: " +
                         json.dumps(sorted(set(capture.DRIVERS) ^
                                           set(summary["bones"]))))
    report = {"schemaVersion": 1, "builder": "build_primates.py",
              "rigSha256": rig_hash, **extra, **summary,
              "albedo": cb.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    groups = {}
    for index, row in weights.items():
        for name, value in row:
            group = groups.get(name) or ape.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    modifier = ape.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    ape.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[" + args.kind + "] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
