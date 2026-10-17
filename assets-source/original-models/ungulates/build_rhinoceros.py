"""Build the refined Rhinoceros (VIS-024) or Woolly Rhinoceros (VIS-003) as
one continuous skinned body.

blender -b --factory-startup --python-exit-code 1 --python build_rhinoceros.py -- \
    [--kind woolly-rhinoceros] \
    --capture <private sprint11-mastodon-bind-rig.json> \
    --albedo assets/ungulates/<kind>-albedo.png \
    --mesh-data assets/ungulates/<kind>-mesh.json \
    --report <private report.json> --blend-out <private source.blend>

The Mastodon capture is validated by the ungulate generator's own loader.
The Mastodon stands tall on long columnar legs; a rhinoceros reads as a
heavy barrel close to the ground. The body therefore hangs deep enough to
bury the upper legs, so only short, thick lower legs show, and the large
head is carried forward and low from the donor's high head pivot. Horns,
ears, eyes, toenails and the tail tuft are closed rigid features rooted
inside the surface. The Woolly Rhinoceros shares the frame and the 26
controls; it adds a high shoulder hump, a coat hanging below the belly, a
long side-flattened front horn, smaller ears and a red-brown streaked coat.
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
    # Its front dome stops short of the nape: reaching further, it wrapped
    # the base of the head and hooded it whenever the head dropped.
    belly = params["belly"]
    r = params["barrel"]
    volume.sweep([point(belly + r[0] + 0.04, rump.z + 0.32),
                  point(belly + r[1] + 0.02, rump.z - 0.75),
                  point(belly + r[2] + 0.06, shoulder.z + 0.75),
                  point(belly + r[3] + 0.08, shoulder.z - 0.02)], r)
    # Withers hump and a short, deep neck, narrower than the chest, falling
    # to the head.
    mass(point(shoulder.y - ground - 0.20 + params["hump"][1] - 0.40,
               shoulder.z - 0.10), params["hump"])
    nape = point(head.y - ground - 0.72, head.z - 0.50)
    volume.sweep([point(shoulder.y - ground - 0.55, shoulder.z - 0.55),
                  nape], [0.98, 0.74])
    mass(nape + UP * 0.30 - FORWARD * 0.30, (0.50, 0.34, 0.60))
    if params["coat"]:
        # The woolly coat: a barrel hung lower (see "belly") and a fuller
        # throat; the painted streaks carry the hair. Separate hanging locks
        # read as bubbles and a belly coat mass as a ridge.
        mass(point(shoulder.y - ground - 1.20, shoulder.z - 0.55),
             params["coat"])
    # Long, boxy head carried forward and low: one tapering skull from the
    # cranium to the muzzle (separate masses left a waist that read as a
    # ball on a stalk), a deep, straight jaw line beneath it and a broad,
    # square upper lip - not a tapering trunk.
    axis = (FORWARD * 0.74 - UP * 0.67).normalized()
    face_up = axis.cross(SIDE).normalized()
    if face_up.dot(UP) < 0:
        face_up = -face_up
    cranium = nape + axis * 0.40
    mid = cranium + axis * 0.76
    muzzle = mid + axis * 0.78
    head_axes = (SIDE, face_up, axis)
    volume.sweep([cranium, mid, mid.lerp(muzzle, 0.6), muzzle],
                 [0.58, 0.50, 0.46, 0.42])
    volume.ellipsoid(cranium, head_axes, (0.56, 0.60, 0.72))
    volume.ellipsoid(cranium.lerp(muzzle, 0.42) - face_up * 0.22,
                     head_axes, (0.50, 0.40, 1.10))
    volume.ellipsoid(muzzle + axis * 0.10 - face_up * 0.10, head_axes,
                     (0.50, 0.36, 0.38))
    for prefix in ("L", "R"):
        front_top = at(prefix + "_Arm_Upper")
        palm = at(prefix + "_Palm")
        # Limb tops start buried in the barrel so the thighs and shoulders
        # flow out of it instead of showing as balls on its sides.
        front = [point(belly + 0.95, front_top.z, front_top.x * 0.78),
                 at(prefix + "_Arm_Lower"), at("frontKnee_" + prefix),
                 point(0.34, palm.z, palm.x)]
        hip = at(prefix + "_Leg0_Upper")
        foot = at(prefix + "_Foot0")
        hind = [point(belly + 1.00, hip.z - 0.20, hip.x * 0.78),
                at(prefix + "_Leg0_Lower"), at("backKnee_" + prefix),
                point(0.34, foot.z, foot.x)]
        for chain, radii in ((front, params["front_radii"]),
                             (hind, params["hind_radii"])):
            volume.sweep(chain, radii)
            # A broad, flat-soled foot.
            pad = point(0.14, chain[-1].z - 0.06, chain[-1].x)
            volume.ellipsoid(pad, (SIDE, UP, FORWARD),
                             (radii[-1] * 1.16, 0.14, radii[-1] * 1.22))
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
    # The Mastodon's Neck and Head pivots sit high in the withers, well
    # behind and above this low-carried head, and the heat solve gives Neck
    # no skin at all. Neck therefore takes the front of the neck and Head
    # the skull, each fading in smoothly along the head axis and out across
    # it, so lowering the head bends the neck over both joints. A first
    # review saw a hard cut-off on the neck sides shear into hood-like
    # flaps when the head dropped.
    def ramp(value, start, width):
        t = max(0.0, min(1.0, (value - start) / width))
        return t * t * (3.0 - 2.0 * t)

    def reach(q):
        along = (q - nape).dot(axis)
        return along, ((q - nape) - axis * along).length

    def neck_share(q):
        along, across = reach(q)
        return 0.65 * ramp(along, -1.70, 0.90) * (1.0 - ramp(across, 0.90, 0.70))

    def head_share(q):
        along, across = reach(q)
        return ramp(along, -0.70, 1.00) * (1.0 - ramp(across, 0.75, 0.55))
    rows = cb.blend_toward(rows, positions, lambda q, row: neck_share(q) > 0,
                           "Neck", neck_share)
    rows = cb.blend_toward(rows, positions, lambda q, row: head_share(q) > 0,
                           "Head", head_share)
    rows = cb.smooth_weights(body, rows, iterations=3)
    rows = cb.clean_weights(rows, positions, armature, allowed)
    parts = [(body, rows, REGION["body"])]

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), REGION[region]))

    # Keratin horns rooted on the face: a long front horn on the muzzle that
    # sweeps back, and a shorter rear horn above the eyes. Never tusks.
    front_base = muzzle + face_up * 0.30
    front_tip = front_base + (face_up * 0.95 + axis * 0.12).normalized() * \
        params["front_horn"]
    rigid(cb.horn("RhinoFrontHorn", front_base - face_up * 0.12, front_tip,
                  params["front_radius"], segments=14, rings=8,
                  curve=-axis * params["front_curve"],
                  flatten=params["front_flatten"], hint=axis),
          [("Head", 1.0)], "horn")
    rear_base = mid + face_up * 0.40 + axis * 0.08
    rigid(cb.horn("RhinoRearHorn", rear_base - face_up * 0.10,
                  rear_base + face_up * params["rear_horn"], 0.23,
                  segments=12, rings=6, curve=-axis * 0.05, flatten=0.9,
                  hint=axis),
          [("Head", 1.0)], "horn")
    for sign in (1.0, -1.0):
        # Large, rounded, cupped ears opening forward; spikes would read as
        # more horns.
        # A second review found them sunk to nubs; they stand clear of the
        # skull now.
        ear_base = cranium + SIDE * sign * 0.34 + face_up * 0.46 - axis * 0.18
        rise = (UP * 0.85 + SIDE * sign * 0.40 - axis * 0.25).normalized()
        facing = (axis - rise * axis.dot(rise)).normalized()
        ear = params["ear_scale"]
        rigid(cb.blob("RhinoEar", ear_base + rise * 0.30 * ear,
                      (rise.cross(facing), rise, facing),
                      (0.16 * ear, 0.38 * ear, 0.11 * ear), 16, 10),
              [("Head", 1.0)], "ear")
        eye = mid + SIDE * sign * 0.52 - face_up * 0.02 - axis * 0.16
        rigid(cb.blob("RhinoEye", eye, (SIDE, axis, face_up),
                      (0.05, 0.08, 0.055), 12, 8), [("Head", 1.0)], "eye")
    for prefix in ("L", "R"):
        for bone, radius in ((prefix + "_Palm", params["front_radii"][-1]),
                             (prefix + "_Foot0", params["hind_radii"][-1])):
            foot = at(bone)
            # Three flat nail plates half sunk into the front of the foot.
            for lateral in (-0.55, 0.0, 0.55):
                nail = point(0.10, foot.z - 0.06, foot.x) + \
                    FORWARD * radius * 1.06 + SIDE * lateral * radius
                rigid(cb.blob("RhinoNail", nail, (SIDE, FORWARD, UP),
                              (0.24 * radius, 0.11 * radius, 0.09), 10, 6),
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
    lip = muzzle + axis * 0.10 - face_up * 0.10
    nostrils = [lip + axis * 0.30 + face_up * 0.12 + SIDE * sign * 0.24
                for sign in (1.0, -1.0)]
    mouth = (lip - face_up * 0.08, axis, face_up)
    horns = [(front_base - face_up * 0.12, front_tip),
             (rear_base - face_up * 0.10,
              rear_base + face_up * params["rear_horn"])]
    return rhino, weights, {"ground": ground, "girth": 1.2, "horns": horns,
                            "nostrils": nostrils, "mouth": mouth,
                            "rump": rump, "head": head, "folds": folds,
                            "barrel_floor": ground + belly + 0.30}


def colour_field(landmarks, palette):
    girth = landmarks["girth"]
    ground = landmarks["ground"]

    def colour(p, n, region):
        up = n[:, 1]
        hide, under = palette["hide"], palette["under"]
        t = np.clip(up * 0.5 + 0.5, 0, 1)[:, None]
        base = under + (hide - under) * t
        broad = cb.fbm(p, 1.0 / (0.6 * girth), 21)
        pebble, second = cb.cells(p, 1.0 / (0.07 * girth), 22)
        crease = np.clip((second - pebble) * 4.0, 0, 1)
        base = base * (0.84 + 0.22 * broad[:, None]) * \
            (0.86 + 0.14 * crease[:, None])
        if palette["fur"]:
            # Long hair: streaks running down the body.
            streak = cb.fbm(p * np.array([5.0, 0.5, 5.0]), 1.0 / 0.12, 24)
            base = base * (0.78 + 0.40 * streak[:, None])
        # Heavy skin folds read as darker bands at party distance.
        folds = np.zeros(len(p))
        for centre, width in landmarks["folds"]:
            folds = np.maximum(folds, np.exp(-((p[:, 2] - centre) / width) ** 2))
        # Folds sit on the flanks of the barrel, not across the back or
        # down the legs.
        flank = np.clip(np.abs(n[:, 0]) * 1.6 - 0.5, 0, 1) * \
            np.clip((ground + 3.9 - p[:, 1]) / 0.8, 0, 1) * \
            np.clip((p[:, 1] - landmarks["barrel_floor"]) / 0.4, 0, 1)
        base = base * (1.0 - palette["folds"] * (folds * flank)[:, None])
        # Dust on the legs and darker, worn feet.
        height = (p[:, 1] - ground) / girth
        dust = np.clip(1.0 - height / 1.6, 0, 1)[:, None]
        base = base * (1 - 0.25 * dust) + palette["dust"] * 0.25 * dust
        foot = np.clip(1.0 - height / 0.35, 0, 1)[:, None]
        base = base * (1 - 0.5 * foot) + np.array([0.18, 0.17, 0.16]) * \
            0.5 * foot
        # Nostrils and the line of the mouth, so the square lip reads as a
        # face rather than a blank bulb.
        face = np.zeros(len(p))
        for nostril in landmarks["nostrils"]:
            d = np.linalg.norm(p - np.array(nostril), axis=1)
            face = np.maximum(face, np.clip(1.0 - d / 0.11, 0, 1) ** 0.7)
        centre, along, rise = (np.array(v) for v in landmarks["mouth"])
        rel = p - centre
        depth = rel @ along
        height = rel @ rise
        line = np.exp(-(height / 0.035) ** 2) * \
            np.clip((depth + 0.55) / 0.20, 0, 1) * \
            (depth < 0.45) * (np.abs(p[:, 0]) < 0.6)
        face = np.maximum(face, 0.75 * line)
        base = base * (1.0 - 0.78 * face[:, None])
        # Horns darken toward the root and pale toward the tip; flat-toned
        # horns seen nearly end-on from the front read as holes in the face.
        reach = np.zeros(len(p))
        nearest = np.full(len(p), np.inf)
        for root, tip in landmarks["horns"]:
            root, tip = np.array(root), np.array(tip)
            span = tip - root
            t = np.clip((p - root) @ span / (span @ span), 0, 1)
            gap = np.linalg.norm(p - (root + t[:, None] * span), axis=1)
            reach = np.where(gap < nearest, t, reach)
            nearest = np.minimum(gap, nearest)
        horn = palette["horn_root"] + \
            palette["horn_rise"] * (reach ** 0.8)[:, None]
        horn = horn * (0.88 + 0.20 * cb.fbm(p * np.array([4.0, 0.6, 4.0]),
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
    parser.add_argument("--kind", choices=tuple(PROPORTIONS),
                        default="rhinoceros")
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = measured_rig(args.capture, args.kind)
    # The capture carries only the 26 controls the body binds; the Mastodon's
    # ankle helpers sit between each knee and foot.
    armature = cb.review_armature(rig_data, "RhinocerosRigPreview", bridge={
        "frontAnkle_L": "frontKnee_L", "frontAnkle_R": "frontKnee_R",
        "backAnkle_L": "backKnee_L", "backAnkle_R": "backKnee_R"})
    params = dict(DEFAULTS)
    params.update(PROPORTIONS[args.kind])
    rhino, weights, landmarks = build(rig, armature, params)
    cb.unwrap(rhino)
    cb.bake_albedo(rhino, 1024, colour_field(landmarks, PALETTES[args.kind]),
                   args.albedo)
    cb.attach_albedo_preview(rhino, args.albedo)
    summary = cb.write_mesh_data(args.mesh_data, rhino, weights, rig_hash,
                                 args.albedo, SPACE)
    if not set(summary["bones"]) <= set(params["bones"]):
        raise SystemExit("Rhinoceros binds an unreviewed control")
    report = {"schemaVersion": 1, "creature": args.kind,
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
    print("[" + args.kind + "] " + json.dumps(report, sort_keys=True))


# Proportions in donor units, tuned against the measured Mastodon frame.
PROPORTIONS = {
    "rhinoceros": {
        "belly": 1.25,
        "barrel": (1.22, 1.38, 1.42, 1.30),
        "hump": (0.86, 0.40, 1.15),
        "coat": None,
        "front_horn": 1.30, "front_curve": 0.16, "front_flatten": 0.85,
        "front_radius": 0.31,
        "rear_horn": 0.60,
        "ear_scale": 1.0,
        "front_radii": (0.70, 0.58, 0.52, 0.48),
        "hind_radii": (0.80, 0.62, 0.52, 0.48),
        "triangles": 5200,
    },
    "woolly-rhinoceros": {
        "belly": 1.00,
        "barrel": (1.28, 1.44, 1.50, 1.36),
        "hump": (0.92, 0.62, 1.10),
        "coat": (0.80, 0.55, 0.70),
        "front_horn": 2.05, "front_curve": 0.42, "front_flatten": 0.55,
        "front_radius": 0.40,
        "rear_horn": 0.70,
        "ear_scale": 0.72,
        "front_radii": (0.74, 0.62, 0.55, 0.50),
        "hind_radii": (0.84, 0.66, 0.55, 0.50),
        "triangles": 5600,
    },
}

# Stored albedo values. The game lights them warmly and brightly: the first
# native capture showed the 0.41 grey hide as tan, so the hide is a darker,
# cooler slate that stays grey in torchlight.
PALETTES = {
    "rhinoceros": {
        "hide": np.array([0.36, 0.36, 0.385]),
        "under": np.array([0.29, 0.29, 0.30]),
        "dust": np.array([0.34, 0.31, 0.28]),
        "fur": False, "folds": 0.14,
        "horn_root": np.array([0.24, 0.21, 0.18]),
        "horn_rise": np.array([0.32, 0.29, 0.24]),
    },
    "woolly-rhinoceros": {
        "hide": np.array([0.40, 0.27, 0.16]),
        "under": np.array([0.24, 0.16, 0.10]),
        "dust": np.array([0.26, 0.19, 0.13]),
        "fur": True, "folds": 0.0,
        "horn_root": np.array([0.16, 0.14, 0.12]),
        "horn_rise": np.array([0.26, 0.22, 0.18]),
    },
}


if __name__ == "__main__":
    main()
