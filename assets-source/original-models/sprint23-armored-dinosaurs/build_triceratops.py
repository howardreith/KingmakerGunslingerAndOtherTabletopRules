"""Build the refined Triceratops as one continuous skinned body (VIS-002).

blender -b --factory-startup --python-exit-code 1 --python build_triceratops.py -- \
    --capture <private sprint11-mastodon-bind-rig.json> \
    --albedo assets/sprint23-armored-dinosaurs/triceratops-albedo.png \
    --mesh-data assets/sprint23-armored-dinosaurs/triceratops-mesh.json \
    --report <private report.json> --blend-out <private source.blend>

The Mastodon capture is validated by the Sprint 23 generator's own loader.
On the Mastodon frame, as for the Rhinoceros, the body hangs as a heavy
barrel low enough to bury the long upper legs, so only short, thick legs
show; the back rises from the shoulders to high hips, and a long, thick
tail runs back from the rump, weighted down the five tail controls by its
length. The great head is carried low and forward from the Mastodon's high
head pivot: a parrot-like beak, a short nose horn, two long brow horns and
a broad frill with a scalloped rim. Horns, frill, rim scallops, eyes and
nails are closed rigid features rooted inside the surface. The owner found
the first export's olive awkward; the hide is now an earthy grey-brown
with a boldly patterned frill.
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
import generate_armored_dinosaurs as armored  # noqa: E402

SPACE = "donor renderer local; +X lateral, +Y up, -Z forward"
SIDE = Vector((1.0, 0.0, 0.0))
UP = Vector((0.0, 1.0, 0.0))
FORWARD = Vector((0.0, 0.0, -1.0))
REGION = {"body": 0, "horn": 1, "frill": 2, "eye": 3, "nail": 4, "beak": 5,
          "rim": 6}
BONES = ("Head", "L_Arm_Lower", "L_Arm_Upper", "L_Foot0", "L_Leg0_Lower",
         "L_Leg0_Upper", "L_Palm", "LowerTorso", "Neck", "R_Arm_Lower",
         "R_Arm_Upper", "R_Foot0", "R_Leg0_Lower", "R_Leg0_Upper", "R_Palm",
         "Spine", "Tail0_M", "Tail1_M", "Tail2_M", "Tail3_M", "Tail4_M",
         "UpperTorso", "backKnee_L", "backKnee_R", "frontKnee_L",
         "frontKnee_R")
TAIL = ("Tail0_M", "Tail1_M", "Tail2_M", "Tail3_M", "Tail4_M")
UPPER_LIMB = {"L_Arm_Upper", "R_Arm_Upper", "L_Leg0_Upper", "R_Leg0_Upper"}


def ramp(value, start, width):
    t = max(0.0, min(1.0, (value - start) / width))
    return t * t * (3.0 - 2.0 * t)


def build(rig, armature):
    at = lambda name: Vector(rig[name]["head"])
    rump, shoulder = at("LowerTorso"), at("UpperTorso")
    spine_mid = at("Spine").z
    head = at("Head")
    ground = min(at(name).y for name in ("L_Palm", "R_Palm",
                                         "L_Foot0", "R_Foot0"))
    volume = cb.Volume(0.05)

    def point(y, z, x=0.0):
        return Vector((x, ground + y, z))

    def mass(centre, radii, axes=(SIDE, UP, FORWARD)):
        volume.ellipsoid(centre, axes, radii)

    # A heavy barrel whose back rises from the shoulders to the high hips.
    volume.sweep([point(2.85, rump.z + 0.30), point(2.95, rump.z - 0.40),
                  point(2.75, spine_mid - 0.30), point(2.50, shoulder.z + 0.10),
                  point(2.35, shoulder.z - 0.55)],
                 [1.10, 1.40, 1.50, 1.35, 1.10])
    belly = 1.20
    for sign in (-1.0, 1.0):
        mass(point(2.35, rump.z - 0.25, sign * 0.62), (0.62, 0.95, 0.85))
        mass(point(2.05, shoulder.z - 0.25, sign * 0.65), (0.52, 0.85, 0.70))
    # A long, thick tail running back and down from the rump.
    tail_line = [point(2.85, rump.z + 0.50), point(2.60, rump.z + 1.50),
                 point(2.20, rump.z + 2.50), point(1.80, rump.z + 3.40),
                 point(1.50, rump.z + 4.10)]
    volume.sweep(tail_line, [0.85, 0.62, 0.42, 0.24, 0.08])
    # A short, deep neck carrying the great head low and forward.
    nape = point(head.y - ground - 1.15, head.z - 0.55)
    volume.sweep([point(2.30, shoulder.z - 0.70), nape], [1.05, 0.85])
    axis = (FORWARD * 0.86 - UP * 0.50).normalized()
    face_up = axis.cross(SIDE).normalized()
    if face_up.dot(UP) < 0:
        face_up = -face_up
    head_axes = (SIDE, face_up, axis)
    cranium = nape + axis * 0.55
    mid = cranium + axis * 0.80
    snout = mid + axis * 0.70
    volume.ellipsoid(cranium, head_axes, (0.72, 0.68, 0.80))
    volume.sweep([cranium, mid, snout], [0.70, 0.58, 0.44])
    volume.ellipsoid(cranium.lerp(mid, 0.5) - face_up * 0.30, head_axes,
                     (0.66, 0.42, 0.85))
    # The parrot-like beak, hooked down at the tip.
    beak_tip = snout + axis * 0.42 - face_up * 0.22
    volume.sweep([snout - face_up * 0.05, beak_tip], [0.40, 0.14])
    for prefix in ("L", "R"):
        front_top = at(prefix + "_Arm_Upper")
        palm = at(prefix + "_Palm")
        front = [point(belly + 0.95, front_top.z, front_top.x * 0.80),
                 at(prefix + "_Arm_Lower"), at("frontKnee_" + prefix),
                 point(0.34, palm.z, palm.x)]
        hip = at(prefix + "_Leg0_Upper")
        foot = at(prefix + "_Foot0")
        hind = [point(belly + 1.05, hip.z - 0.20, hip.x * 0.80),
                at(prefix + "_Leg0_Lower"), at("backKnee_" + prefix),
                point(0.34, foot.z, foot.x)]
        for chain, radii in ((front, (0.66, 0.56, 0.48, 0.44)),
                             (hind, (0.80, 0.62, 0.50, 0.46))):
            volume.sweep(chain, radii)
            mass(point(0.14, chain[-1].z - 0.06, chain[-1].x),
                 (radii[-1] * 1.16, 0.14, radii[-1] * 1.22))

    body = volume.polygonise("TriceratopsBody")
    cb.finish_surface(body, 6400, smooth_iterations=24)
    positions = [v.co.copy() for v in body.data.vertices]
    allowed = set(BONES)
    rows = cb.heat_weights(body, armature, allowed)
    rows = cb.clean_weights(rows, positions, armature, allowed)
    # The barrel buries the Mastodon's long upper limb bones; above the belly
    # line it follows the spine.
    line = ground + belly

    def damp(q):
        return 0.85 * ramp(q.y, line - 0.10, 0.90)

    for target, half in (("UpperTorso", lambda q: q.z < spine_mid),
                         ("LowerTorso", lambda q: q.z >= spine_mid)):
        rows = cb.blend_toward(
            rows, positions,
            lambda q, row, half=half: half(q) and any(
                name in UPPER_LIMB for name, _ in row),
            target, damp)
    rows = cb.smooth_weights(body, rows)

    # As for the Rhinoceros: Neck and Head take the neck and the skull
    # through smooth ramps along and across the head axis.
    def reach(q):
        along = (q - nape).dot(axis)
        return along, ((q - nape) - axis * along).length

    def neck_share(q):
        along, across = reach(q)
        return 0.65 * ramp(along, -1.70, 0.90) * (1.0 - ramp(across, 0.95, 0.70))

    def head_share(q):
        along, across = reach(q)
        return ramp(along, -0.70, 1.00) * (1.0 - ramp(across, 0.85, 0.55))

    # Behind the rump the tail chain owns the tail by distance along it.
    tail_from = tail_line[0]
    tail_axis = (tail_line[-1] - tail_from).normalized()
    tail_span = (tail_line[-1] - tail_from).length

    def tail_row(q):
        t = (q - tail_from).dot(tail_axis) / tail_span
        if t <= 0.0:
            return None
        station = min(t, 1.0) * 3.999
        low, share = int(station), station - int(station)
        weight = ramp(t, 0.0, 0.12)
        return weight, [(TAIL[low], (1.0 - share) * weight),
                        (TAIL[low + 1], share * weight)]

    for target, share in (("Neck", neck_share), ("Head", head_share)):
        rows = cb.blend_toward(rows, positions,
                               lambda q, row, share=share: share(q) > 0,
                               target, share)
    for index, q in enumerate(positions):
        tail = tail_row(q)
        if tail is None:
            continue
        weight, chain = tail
        merged = {}
        for name, value in rows[index]:
            merged[name] = merged.get(name, 0.0) + value * (1.0 - weight)
        for name, value in chain:
            if value > 0.0:
                merged[name] = merged.get(name, 0.0) + value
        rows[index] = sorted(merged.items(), key=lambda e: (-e[1], e[0]))
    rows = cb.smooth_weights(body, rows, iterations=3)
    rows = cb.clean_weights(rows, positions, armature, allowed)
    regions = {index: REGION["beak"] if (q - beak_tip).length < 0.45 else
               REGION["body"] for index, q in enumerate(positions)}
    parts = [(body, rows, regions)]

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), REGION[region]))

    # The frill: a broad, thin shield rising up and back from the skull,
    # with a scalloped rim.
    frill_up = (face_up * 0.52 - axis * 0.86).normalized()
    frill_centre = cranium + frill_up * 1.12 + face_up * 0.10
    frill_normal = frill_up.cross(SIDE).normalized()
    if frill_normal.dot(FORWARD) < 0:
        frill_normal = -frill_normal
    rigid(cb.blob("TriceratopsFrill", frill_centre,
                  (SIDE, frill_up, frill_normal), (1.32, 1.12, 0.14), 28, 14),
          [("Head", 1.0)], "frill")
    for step in range(11):
        angle = math.pi * (0.06 + 0.88 * step / 10.0)
        rim = frill_centre + SIDE * math.cos(angle) * 1.29 + \
            frill_up * math.sin(angle) * 1.09
        rigid(cb.blob("TriceratopsRim", rim, (SIDE, frill_up, frill_normal),
                      (0.16, 0.16, 0.10), 10, 6), [("Head", 1.0)], "rim")
    # Two long brow horns sweeping forward and up, and a short nose horn.
    for sign in (1.0, -1.0):
        base = cranium + SIDE * sign * 0.36 + face_up * 0.55 + axis * 0.25
        tip = base + (axis * 0.80 + face_up * 0.55 + SIDE * sign * 0.12
                      ).normalized() * 1.55
        rigid(cb.horn("TriceratopsBrowHorn", base - face_up * 0.15, tip, 0.24,
                      segments=14, rings=8, curve=-face_up * 0.14, hint=axis),
              [("Head", 1.0)], "horn")
        eye = cranium.lerp(mid, 0.25) + SIDE * sign * 0.62 + face_up * 0.18
        rigid(cb.blob("TriceratopsEye", eye, (SIDE, axis, face_up),
                      (0.06, 0.09, 0.06), 12, 8), [("Head", 1.0)], "eye")
    nose = snout + face_up * 0.36
    rigid(cb.horn("TriceratopsNoseHorn", nose - face_up * 0.12,
                  nose + (face_up * 0.9 + axis * 0.3).normalized() * 0.50,
                  0.18, segments=12, rings=6, hint=axis),
          [("Head", 1.0)], "horn")
    for prefix in ("L", "R"):
        for bone, radius in ((prefix + "_Palm", 0.44), (prefix + "_Foot0", 0.46)):
            foot = at(bone)
            for lateral in (-0.55, 0.0, 0.55):
                nail = point(0.10, foot.z - 0.06, foot.x) + \
                    FORWARD * radius * 1.06 + SIDE * lateral * radius
                rigid(cb.blob("TriceratopsNail", nail, (SIDE, FORWARD, UP),
                              (0.24 * radius, 0.11 * radius, 0.09), 10, 6),
                      [(bone, 1.0)], "nail")
    tri, weights = cb.assemble("Triceratops", parts)
    cb.require_single_body(tri, REGION["body"])
    return tri, weights, {"ground": ground, "frill": (frill_centre, SIDE,
                                                     frill_up),
                          "horns": [], "rump": rump}


def colour_field(landmarks):
    ground = landmarks["ground"]
    centre, side, rise = (np.array(v) for v in landmarks["frill"])

    def colour(p, n, region):
        up = np.clip(n[:, 1] * 0.5 + 0.5, 0, 1)[:, None]
        back = np.array([0.30, 0.27, 0.22])
        belly = np.array([0.44, 0.40, 0.32])
        base = belly + (back - belly) * np.clip(up * 1.3 - 0.15, 0, 1)
        broad = cb.fbm(p, 1.0 / 0.8, 71)
        scale, second = cb.cells(p, 1.0 / 0.12, 72)
        rim = np.clip((second - scale) * 3.0, 0, 1)
        stripes = 0.5 + 0.5 * np.sin(p[:, 2] * 3.2 + broad * 4.0)
        dorsal = np.clip((n[:, 1] - 0.3) * 1.5, 0, 1) * np.clip(stripes - 0.55, 0, 1) * 1.8
        base = base * (0.86 + 0.20 * broad[:, None]) * \
            (0.84 + 0.16 * rim[:, None]) * (1 - 0.35 * dorsal[:, None])
        height = (p[:, 1] - ground)
        feet = np.clip((0.6 - height) / 0.4, 0, 1)[:, None]
        base = base * (1 - 0.5 * feet) + np.array([0.18, 0.17, 0.15]) * 0.5 * feet
        # The frill: rust with a dark rim and a pale eye-spot on each side.
        rel = p - centre
        u = rel @ side / 1.25
        v = rel @ rise / 1.05
        radial = np.sqrt(u * u + v * v)
        frill = np.array([0.52, 0.30, 0.16]) * (0.85 + 0.25 * broad[:, None])
        edge = np.clip((radial - 0.72) / 0.12, 0, 1)[:, None]
        frill = frill * (1 - edge) + np.array([0.12, 0.09, 0.07]) * edge
        spot = np.exp(-(((np.abs(u) - 0.45) / 0.14) ** 2 + ((v - 0.40) / 0.16) ** 2))
        frill = frill * (1 - spot[:, None]) + np.array([0.70, 0.62, 0.44]) * \
            spot[:, None]
        rgb = np.where((region == REGION["frill"])[:, None], frill, base)
        rgb = np.where((region == REGION["rim"])[:, None],
                       np.array([0.14, 0.11, 0.08]), rgb)
        rgb = np.where((region == REGION["beak"])[:, None],
                       np.array([0.12, 0.11, 0.10]), rgb)
        horn = np.array([0.66, 0.60, 0.48]) * (0.9 + 0.15 * broad[:, None])
        rgb = np.where((region == REGION["horn"])[:, None], horn, rgb)
        rgb = np.where((region == REGION["eye"])[:, None],
                       np.array([0.05, 0.04, 0.03]), rgb)
        rgb = np.where((region == REGION["nail"])[:, None],
                       np.array([0.22, 0.20, 0.17]), rgb)
        return rgb
    return colour


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = armored.measured_rig(args.capture, "triceratops")
    armature = cb.review_armature(rig_data, "TriceratopsRigPreview", bridge={
        "frontAnkle_L": "frontKnee_L", "frontAnkle_R": "frontKnee_R",
        "backAnkle_L": "backKnee_L", "backAnkle_R": "backKnee_R"})
    tri, weights, landmarks = build(rig, armature)
    cb.unwrap(tri)
    cb.bake_albedo(tri, 1024, colour_field(landmarks), args.albedo)
    cb.attach_albedo_preview(tri, args.albedo)
    extra = {"creature": "triceratops",
             "nativeDonor": armored.DONORS["triceratops"], "visibleLegs": 4,
             "horns": 3, "frill": True, "dorsalPlates": 0, "tailJoints": 5,
             "tailSpikes": 0, "terminalClub": False}
    summary = cb.write_mesh_data(args.mesh_data, tri, weights, rig_hash,
                                 args.albedo, SPACE, extra)
    if tuple(summary["bones"]) != BONES:
        raise SystemExit("Triceratops must bind exactly the reviewed 26 "
                         "Mastodon controls: " + json.dumps(summary["bones"]))
    report = {"schemaVersion": 1, "builder": "build_triceratops.py",
              "donorFamily": "mastodon", "rigSha256": rig_hash, **extra,
              **summary, "albedo": cb.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    groups = {}
    for index, row in weights.items():
        for name, value in row:
            group = groups.get(name) or tri.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    modifier = tri.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    tri.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[triceratops] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
