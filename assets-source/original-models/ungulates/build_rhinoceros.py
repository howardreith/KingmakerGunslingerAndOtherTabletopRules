"""Build the refined Rhinoceros as one continuous skinned body (VIS-024).

blender -b --factory-startup --python-exit-code 1 --python build_rhinoceros.py -- \
    --capture <private sprint11-mastodon-bind-rig.json> \
    --albedo assets/ungulates/rhinoceros-albedo.png \
    --mesh-data assets/ungulates/rhinoceros-mesh.json \
    --report <private report.json> --blend-out <private source.blend>

The Mastodon capture is validated by the ungulate generator's own loader.
The Mastodon stands tall on long columnar legs; a rhinoceros reads as a
heavy barrel close to the ground. The body therefore hangs deep enough to
bury the upper legs, so only short, thick lower legs show, and the large
head is carried forward and low from the donor's high head pivot. Horns,
ears, eyes, toenails and the tail tuft are closed rigid features rooted
inside the surface.
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
from generate_ungulates import measured_rig  # noqa: E402

SPACE = "donor renderer local; +X left, +Y up, -Z forward"
SIDE = Vector((1.0, 0.0, 0.0))      # creature's left
UP = Vector((0.0, 1.0, 0.0))
FORWARD = Vector((0.0, 0.0, -1.0))
REGION = {"body": 0, "horn": 1, "ear": 2, "eye": 3, "nail": 4, "tuft": 5}


def build(rig, armature, params):
    at = lambda name: Vector(rig[name]["head"])
    rump, shoulder = at("LowerTorso"), at("UpperTorso")
    spine_mid = at("Spine").z
    head = at("Head")
    tail = [at("Tail%d_M" % index) for index in range(5)]
    ground = min(at(name).y for name in ("L_Palm", "R_Palm",
                                         "L_Foot0", "R_Foot0"))
    volume = cb.Volume(0.05)

    def point(y, z, x=0.0):
        return Vector((x, ground + y, z))

    def mass(centre, radii):
        volume.ellipsoid(centre, (SIDE, UP, FORWARD), radii)

    # One heavy barrel hung low beneath the Mastodon's spine, running past
    # both ends of it, so it buries the upper legs and reads long and low.
    belly = params["belly"]
    r = params["barrel"]
    volume.sweep([point(belly + r[0] + 0.04, rump.z + 0.55),
                  point(belly + r[1] + 0.02, rump.z - 0.75),
                  point(belly + r[2] + 0.06, shoulder.z + 0.75),
                  point(belly + r[3] + 0.08, shoulder.z - 0.30)], r)
    # Withers hump and a short, deep neck falling to the head.
    mass(point(shoulder.y - ground - 0.05, shoulder.z - 0.10),
         (0.72, 0.42, 0.95))
    nape = point(head.y - ground - 0.80, head.z - 0.50)
    volume.sweep([point(shoulder.y - ground - 0.55, shoulder.z - 0.55),
                  nape], [1.06, 0.84])
    mass(nape + UP * 0.40 - FORWARD * 0.25, (0.56, 0.42, 0.72))
    # Long, boxy head carried forward and low: cranium, mid-face, deep
    # cheeks and a blunt, square muzzle - not a tapering trunk.
    axis = (FORWARD * 0.74 - UP * 0.67).normalized()
    cranium = nape + axis * 0.40
    mid = cranium + axis * 0.72
    muzzle = mid + axis * 0.70
    head_axes = (SIDE, axis.cross(SIDE), axis)
    volume.ellipsoid(cranium, head_axes, (0.56, 0.60, 0.72))
    volume.ellipsoid(mid, head_axes, (0.48, 0.52, 0.64))
    volume.ellipsoid(muzzle, head_axes, (0.50, 0.44, 0.46))
    volume.ellipsoid(cranium.lerp(mid, 0.5) - UP * 0.28, head_axes,
                     (0.54, 0.44, 0.70))
    for prefix in ("L", "R"):
        front_top = at(prefix + "_Arm_Upper")
        palm = at(prefix + "_Palm")
        front = [point(belly + 0.55, front_top.z, front_top.x * 0.92),
                 at(prefix + "_Arm_Lower"), at("frontKnee_" + prefix),
                 point(0.34, palm.z, palm.x)]
        hip = at(prefix + "_Leg0_Upper")
        foot = at(prefix + "_Foot0")
        hind = [point(belly + 0.65, hip.z - 0.20, hip.x * 0.92),
                at(prefix + "_Leg0_Lower"), at("backKnee_" + prefix),
                point(0.34, foot.z, foot.x)]
        for chain, radii in ((front, params["front_radii"]),
                             (hind, params["hind_radii"])):
            volume.sweep(chain, radii)
            pad = point(0.20, chain[-1].z - 0.06, chain[-1].x)
            volume.ellipsoid(pad, (SIDE, UP, FORWARD),
                             (radii[-1] * 1.12, 0.20, radii[-1] * 1.18))
    volume.sweep(tail[:4], [0.17, 0.12, 0.09, 0.07])

    body = volume.polygonise("RhinocerosBody")
    cb.finish_surface(body, params["triangles"], smooth_iterations=24)
    positions = [v.co.copy() for v in body.data.vertices]
    allowed = set(params["bones"])
    rows = cb.heat_weights(body, armature, allowed)
    rows = cb.clean_weights(rows, positions, armature, allowed)
    # The barrel buries the Mastodon's long upper limb bones, so the heat
    # solve hands belly and flank vertices large upper-limb shares and a leg
    # swing drags the barrel with it. Above the belly line the barrel
    # follows the spine; the visible leg columns keep their own weights.
    upper_limb = {"L_Arm_Upper", "R_Arm_Upper", "L_Leg0_Upper",
                  "R_Leg0_Upper"}
    line = ground + belly

    def damp(position):
        t = max(0.0, min(1.0, (position.y - line + 0.10) / 0.90))
        return 0.85 * t * t * (3.0 - 2.0 * t)
    for target, half in (("UpperTorso", lambda q: q.z < spine_mid),
                         ("LowerTorso", lambda q: q.z >= spine_mid)):
        rows = cb.blend_toward(
            rows, positions,
            lambda q, row, half=half: half(q) and any(
                name in upper_limb for name, _ in row),
            target, damp)
    rows = cb.smooth_weights(body, rows)
    # The head moves as one with its rigid horns, ears and eyes: Head owns
    # it outright forward of the nape, ramping in over the neck.
    def into_head(q):
        t = max(0.0, min(1.0, (q - nape).dot(axis) / 0.55 + 0.35))
        return t * t * (3.0 - 2.0 * t)
    def in_head(q, row):
        along = (q - nape).dot(axis)
        return along > -0.20 and             ((q - nape) - axis * along).length < 0.85
    rows = cb.blend_toward(rows, positions, in_head, "Head", into_head)
    rows = cb.clean_weights(rows, positions, armature, allowed)
    parts = [(body, rows, REGION["body"])]

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), REGION[region]))

    # Keratin horns rooted on the face: a long front horn on the muzzle that
    # sweeps back, and a shorter rear horn above the eyes. Never tusks.
    face_up = axis.cross(SIDE).normalized()
    if face_up.dot(UP) < 0:
        face_up = -face_up
    front_base = muzzle + face_up * 0.30
    front_tip = front_base + (face_up * 0.95 + axis * 0.12).normalized() * \
        params["front_horn"]
    rigid(cb.horn("RhinoFrontHorn", front_base - face_up * 0.12, front_tip,
                  0.31, segments=14, rings=8, curve=-axis * 0.16,
                  flatten=0.85, hint=axis),
          [("Head", 1.0)], "horn")
    rear_base = mid + face_up * 0.40 + axis * 0.08
    rigid(cb.horn("RhinoRearHorn", rear_base - face_up * 0.10,
                  rear_base + face_up * params["rear_horn"], 0.23,
                  segments=12, rings=6, curve=-axis * 0.05, flatten=0.9,
                  hint=axis),
          [("Head", 1.0)], "horn")
    for sign in (1.0, -1.0):
        ear_base = cranium + SIDE * sign * 0.36 + face_up * 0.48 - axis * 0.16
        rigid(cb.horn("RhinoEar", ear_base, ear_base +
                      (UP * 0.85 + SIDE * sign * 0.40 - axis * 0.25)
                      .normalized() * 0.62, 0.19, segments=10, flatten=0.55,
                      hint=axis),
              [("Head", 1.0)], "ear")
        eye = mid + SIDE * sign * 0.46 - face_up * 0.02 - axis * 0.16
        rigid(cb.blob("RhinoEye", eye, (SIDE, axis, face_up),
                      (0.05, 0.08, 0.055), 12, 8), [("Head", 1.0)], "eye")
    for prefix in ("L", "R"):
        for bone, radius in ((prefix + "_Palm", params["front_radii"][-1]),
                             (prefix + "_Foot0", params["hind_radii"][-1])):
            foot = at(bone)
            for lateral in (-0.55, 0.0, 0.55):
                nail = point(0.13, foot.z - 0.06, foot.x) + \
                    FORWARD * radius * 1.12 + SIDE * lateral * radius
                rigid(cb.blob("RhinoNail", nail, (SIDE, FORWARD, UP),
                              (0.22 * radius, 0.15 * radius, 0.12), 10, 6),
                      [(bone, 1.0)], "nail")
    tuft_dir = (tail[4] - tail[3]).normalized()
    rigid(cb.strand("RhinoTailTuft", [tail[3], tail[4],
                                      tail[4] + tuft_dir * 0.30],
                    [0.07, 0.06, 0.015], 8),
          [("Tail4_M", 1.0)], "tuft")
    rhino, weights = cb.assemble("Rhinoceros", parts)
    cb.require_single_body(rhino, REGION["body"])
    folds = [(shoulder.z + 0.55, 0.07), (rump.z - 0.35, 0.08),
             (shoulder.z - 0.45, 0.06), (shoulder.z - 0.75, 0.05)]
    return rhino, weights, {"ground": ground, "girth": 1.2,
                            "rump": rump, "head": head, "folds": folds}


def colour_field(landmarks):
    girth = landmarks["girth"]
    ground = landmarks["ground"]

    def colour(p, n, region):
        up = n[:, 1]
        hide = np.array([0.52, 0.49, 0.45])
        under = np.array([0.44, 0.42, 0.39])
        t = np.clip(up * 0.5 + 0.5, 0, 1)[:, None]
        base = under + (hide - under) * t
        broad = cb.fbm(p, 1.0 / (0.6 * girth), 21)
        pebble, second = cb.cells(p, 1.0 / (0.07 * girth), 22)
        crease = np.clip((second - pebble) * 4.0, 0, 1)
        base = base * (0.84 + 0.22 * broad[:, None]) * \
            (0.86 + 0.14 * crease[:, None])
        # Heavy skin folds read as darker bands at party distance.
        folds = np.zeros(len(p))
        for centre, width in landmarks["folds"]:
            folds = np.maximum(folds, np.exp(-((p[:, 2] - centre) / width) ** 2))
        # Folds sit on the flanks and lower sides, not across the back.
        flank = np.clip(np.abs(n[:, 0]) * 1.6 - 0.5, 0, 1) *             np.clip((landmarks["ground"] + 3.9 - p[:, 1]) / 0.8, 0, 1)
        base = base * (1.0 - 0.20 * (folds * flank)[:, None])
        # Dust on the legs and darker, worn feet.
        height = (p[:, 1] - ground) / girth
        dust = np.clip(1.0 - height / 1.6, 0, 1)[:, None]
        base = base * (1 - 0.25 * dust) + np.array([0.43, 0.38, 0.32]) * \
            0.25 * dust
        foot = np.clip(1.0 - height / 0.35, 0, 1)[:, None]
        base = base * (1 - 0.5 * foot) + np.array([0.22, 0.21, 0.20]) * \
            0.5 * foot
        horn = np.array([0.36, 0.31, 0.25]) * \
            (0.85 + 0.25 * cb.fbm(p * np.array([4.0, 0.6, 4.0]),
                                  1.0 / (0.1 * girth), 23))[:, None]
        rgb = np.where((region == REGION["horn"])[:, None], horn, base)
        rgb = np.where((region == REGION["ear"])[:, None], base * 0.92, rgb)
        rgb = np.where((region == REGION["eye"])[:, None],
                       np.array([0.05, 0.04, 0.035]), rgb)
        rgb = np.where((region == REGION["nail"])[:, None],
                       np.array([0.30, 0.27, 0.23]), rgb)
        rgb = np.where((region == REGION["tuft"])[:, None],
                       np.array([0.12, 0.11, 0.10]), rgb)
        return rgb
    return colour


DEFAULTS = {
    "bones": ("Head", "L_Arm_Lower", "L_Arm_Upper", "L_Foot0", "L_Leg0_Lower",
              "L_Leg0_Upper", "L_Palm", "LowerTorso", "Neck", "R_Arm_Lower",
              "R_Arm_Upper", "R_Foot0", "R_Leg0_Lower", "R_Leg0_Upper",
              "R_Palm", "Spine", "Tail0_M", "Tail1_M", "Tail2_M", "Tail3_M",
              "Tail4_M", "UpperTorso", "backKnee_L", "backKnee_R",
              "frontKnee_L", "frontKnee_R"),
}


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = measured_rig(args.capture, "rhinoceros")
    # The capture carries only the 26 controls the body binds; the Mastodon's
    # ankle helpers sit between each knee and foot.
    armature = cb.review_armature(rig_data, "RhinocerosRigPreview", bridge={
        "frontAnkle_L": "frontKnee_L", "frontAnkle_R": "frontKnee_R",
        "backAnkle_L": "backKnee_L", "backAnkle_R": "backKnee_R"})
    params = dict(DEFAULTS)
    params.update(PROPORTIONS)
    rhino, weights, landmarks = build(rig, armature, params)
    cb.unwrap(rhino)
    cb.bake_albedo(rhino, 1024, colour_field(landmarks), args.albedo)
    cb.attach_albedo_preview(rhino, args.albedo)
    summary = cb.write_mesh_data(args.mesh_data, rhino, weights, rig_hash,
                                 args.albedo, SPACE)
    if not set(summary["bones"]) <= set(params["bones"]):
        raise SystemExit("Rhinoceros binds an unreviewed control")
    report = {"schemaVersion": 1, "creature": "rhinoceros",
              "builder": "build_rhinoceros.py", "donorFamily": "mastodon",
              "rigSha256": rig_hash, **summary,
              "albedo": cb.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    groups = {}
    for index, row in weights.items():
        for name, value in row:
            group = groups.get(name) or rhino.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    modifier = rhino.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    rhino.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[rhinoceros] " + json.dumps(report, sort_keys=True))


# Proportions in donor units, tuned against the measured Mastodon frame.
PROPORTIONS = {
    "belly": 1.25,
    "barrel": (1.22, 1.38, 1.42, 1.30),
    "front_horn": 1.30,
    "rear_horn": 0.60,
    "front_radii": (0.70, 0.58, 0.52, 0.48),
    "hind_radii": (0.80, 0.62, 0.52, 0.48),
    "triangles": 5200,
}


if __name__ == "__main__":
    main()
