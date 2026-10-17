"""Build the refined Poisonous Frog as one continuous skinned body (VIS-006).

blender -b --factory-startup --python-exit-code 1 --python build_poisonous_frog.py -- \
    --capture <private sprint13-giant-poisonous-frog-bind-rig.json> \
    --albedo assets/sprint13-creatures/poisonous-frog-albedo.png \
    --mesh-data assets/sprint13-creatures/poisonous-frog-mesh.json \
    --report <private report.json> --blend-out <private source.blend>

The Giant Poisonous Frog capture is validated by the Sprint 13 generator's own
loader. The donor's bind pose holds the hind legs stretched straight back and
the arms forward; the animations fold them into the crouch, so the body is
authored along that pose: a squat body and a wide head as one surface, the
eyes bulging on the eyelid controls, the chin on both jaw controls, arms with
fingered hands and long webbed hind feet. The owner questioned the first
export's orange; the frog is now an emerald green with black blotches, still
a warning colour, over a paler belly.
"""
import argparse
import json
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
from generate_sprint13_creatures import measured_rig  # noqa: E402

SPACE = "donor renderer local; +X right, +Y up, +Z forward"
LEFT = Vector((-1.0, 0.0, 0.0))
UP = Vector((0.0, 1.0, 0.0))
FORWARD = Vector((0.0, 0.0, 1.0))
REGION = {"body": 0, "eye": 1, "pad": 2}
BONES = ("Head", "L_Arm_Lower", "L_Arm_Upper", "L_Foot0", "L_Leg0_Lower",
         "L_Leg0_Upper", "L_Palm", "LowerTorso", "R_Arm_Lower", "R_Arm_Upper",
         "R_Foot0", "R_Leg0_Lower", "R_Leg0_Upper", "R_Palm", "chest_joint",
         "jaw_endJoint", "jaw_skinJoint1", "l_t_eyeLid_joint",
         "l_toeTip_joint", "l_toe_joint", "r_t_eyeLid_joint",
         "r_toeTip_joint", "r_toe_joint", "spine_3_joint", "stomach")


def ramp(value, start, width):
    t = max(0.0, min(1.0, (value - start) / width))
    return t * t * (3.0 - 2.0 * t)


def build(rig, armature):
    at = lambda name: Vector(rig[name]["head"])
    hips, middle, chest = at("LowerTorso"), at("spine_3_joint"), \
        at("chest_joint")
    head = at("Head")
    torso = cb.Volume(0.03)
    # A squat body rising to the shoulders, and a wide, flat head as wide as
    # the body, united and relaxed together so the head never reads as a
    # separate piece.
    torso.ellipsoid(hips.lerp(chest, 0.45) - UP * 0.04,
                    (LEFT, UP, FORWARD), (0.40, 0.30, 0.58))
    torso.ellipsoid(hips + FORWARD * 0.05, (LEFT, UP, FORWARD),
                    (0.34, 0.26, 0.30))
    torso.ellipsoid(head + FORWARD * 0.02 + UP * 0.02, (LEFT, UP, FORWARD),
                    (0.42, 0.22, 0.34))
    torso.ellipsoid(head + FORWARD * 0.22 + UP * 0.06, (LEFT, UP, FORWARD),
                    (0.34, 0.16, 0.20))
    trunk = cb.relax(torso.polygonise("FrogTorso"), 24)
    volume = cb.Volume(0.012)
    volume.shell(trunk)
    for prefix, side in (("l", -1.0), ("r", 1.0)):
        upper = "L" if prefix == "l" else "R"
        shoulder = Vector((side * 0.17, 0.55, 0.94))
        volume.sweep([shoulder, at(upper + "_Arm_Upper"),
                      Vector((side * 0.528, 0.335, 0.811)),
                      at(upper + "_Arm_Lower"), at(upper + "_Palm")],
                     [0.10, 0.085, 0.07, 0.055, 0.05])
        # A flat hand with three fingers reaching forward.
        palm = at(upper + "_Palm")
        for lateral, reach in ((-0.10, 0.30), (0.0, 0.40), (0.10, 0.32)):
            tip = palm + FORWARD * reach + LEFT * side * -lateral * 2.0 - \
                UP * 0.04
            volume.sweep([palm, tip], [0.045, 0.028])
        hip, knee = at(upper + "_Leg0_Upper"), at(upper + "_Leg0_Lower")
        ankle = at(upper + "_Foot0")
        toe, tip = at(prefix + "_toe_joint"), at(prefix + "_toeTip_joint")
        volume.sweep([hip + LEFT * side * 0.06, knee, ankle, toe, tip],
                     [0.21, 0.13, 0.085, 0.06, 0.035])
        # Long webbed toes fanning from the toe joint.
        for spread in (-0.14, 0.0, 0.14):
            end = tip + Vector((spread * side, 0.05, spread * 0.4))
            volume.sweep([toe, end], [0.045, 0.022])
    body = volume.polygonise("PoisonousFrogBody")
    cb.finish_surface(body, 6400, smooth_iterations=10)
    positions = [v.co.copy() for v in body.data.vertices]
    allowed = set(BONES)
    rows = cb.heat_weights(body, armature, allowed)
    rows = cb.clean_weights(rows, positions, armature, allowed)
    rows = cb.smooth_weights(body, rows)
    # The chin and lower lip follow both jaw controls; the belly shares the
    # stomach control; the eyelid controls carry the skin over each eye.
    mouth = head + FORWARD * 0.20 - UP * 0.02

    def jaw_share(q):
        below = mouth.y - q.y
        ahead = q.z - head.z + 0.10
        return 0.60 * ramp(below, 0.0, 0.06) * ramp(ahead, 0.0, 0.10)

    belly = at("stomach")

    def stomach_share(q):
        return 0.55 * (1.0 - ramp((q - belly - UP * 0.04).length, 0.10, 0.18))

    lids = {name: at(name) for name in ("l_t_eyeLid_joint",
                                        "r_t_eyeLid_joint")}

    def lid_share(name):
        return lambda q: 0.65 * (1.0 - ramp((q - lids[name]).length, 0.10,
                                            0.10))

    for target, share in (("jaw_skinJoint1", jaw_share),
                          ("stomach", stomach_share),
                          ("l_t_eyeLid_joint", lid_share("l_t_eyeLid_joint")),
                          ("r_t_eyeLid_joint", lid_share("r_t_eyeLid_joint"))):
        rows = cb.blend_toward(rows, positions,
                               lambda q, row, share=share: share(q) > 0,
                               target, share)
    # jaw_endJoint sits on jaw_skinJoint1; it takes half of the chin.
    rows = cb.blend_toward(rows, positions,
                           lambda q, row: any(name == "jaw_skinJoint1"
                                              for name, _ in row),
                           "jaw_endJoint",
                           lambda q: 0.5 * jaw_share(q) / 0.60)
    rows = cb.smooth_weights(body, rows, iterations=2)
    rows = cb.clean_weights(rows, positions, armature, allowed)
    parts = [(body, rows, REGION["body"])]
    for name in lids:
        # Bulging eyes rooted in the skull under each eyelid control.
        centre = lids[name] - UP * 0.10 - FORWARD * 0.02
        eye = cb.blob("Eye", centre, (LEFT, UP, FORWARD), (0.13, 0.12, 0.13),
                      16, 10)
        parts.append((eye, cb.rigid_rows(eye, [(name, 1.0)]), REGION["eye"]))
    frog, weights = cb.assemble("PoisonousFrog", parts)
    cb.require_single_body(frog, REGION["body"])
    return frog, weights, {"head": head, "belly": belly,
                           "ground": min(p.y for p in positions)}


def colour_field(landmarks):
    def colour(p, n, region):
        up = np.clip(n[:, 1] * 0.5 + 0.5, 0, 1)[:, None]
        back = np.array([0.10, 0.46, 0.22])
        belly = np.array([0.42, 0.56, 0.30])
        base = belly + (back - belly) * np.clip(up * 1.4 - 0.2, 0, 1)
        broad = cb.fbm(p, 1.0 / 0.30, 51)
        base = base * (0.85 + 0.25 * broad[:, None])
        # Black blotches over the back, sides and legs, not the belly.
        blot = cb.fbm(p, 1.0 / 0.16, 52)
        dark = np.clip((blot - 0.56) / 0.06, 0, 1) * \
            np.clip(up[:, 0] * 2.0 - 0.3, 0, 1)
        base = base * (1 - dark[:, None]) + np.array([0.03, 0.04, 0.03]) * \
            dark[:, None]
        # A darker mouth line.
        mouth = np.exp(-((p[:, 1] - landmarks["head"].y + 0.02) / 0.02) ** 2) * \
            np.clip((p[:, 2] - landmarks["head"].z + 0.05) / 0.1, 0, 1)
        base = base * (1 - 0.6 * mouth[:, None])
        rgb = np.where((region == REGION["eye"])[:, None],
                       np.array([0.03, 0.03, 0.03]), base)
        return rgb
    return colour


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = measured_rig(args.capture, "poisonous-frog")
    armature = cb.review_armature(rig_data, "PoisonousFrogRigPreview")
    frog, weights, landmarks = build(rig, armature)
    cb.unwrap(frog)
    cb.bake_albedo(frog, 1024, colour_field(landmarks), args.albedo)
    cb.attach_albedo_preview(frog, args.albedo)
    summary = cb.write_mesh_data(args.mesh_data, frog, weights, rig_hash,
                                 args.albedo, SPACE)
    if tuple(summary["bones"]) != BONES:
        raise SystemExit("Poisonous Frog must bind exactly the reviewed 25 "
                         "controls: " + json.dumps(summary["bones"]))
    report = {"schemaVersion": 1, "creature": "poisonous-frog",
              "builder": "build_poisonous_frog.py",
              "donorFamily": "giant-poisonous-frog", "rigSha256": rig_hash,
              **summary, "albedo": cb.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    groups = {}
    for index, row in weights.items():
        for name, value in row:
            group = groups.get(name) or frog.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    modifier = frog.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    frog.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[poisonous-frog] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
