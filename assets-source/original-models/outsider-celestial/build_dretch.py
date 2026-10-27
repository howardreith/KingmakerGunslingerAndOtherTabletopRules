"""Original Dretch on the measured free-hands rig; no native art copied.

The October 25 refinement replaces the first union-of-primitives body with a
signed-distance sculpt (`sdf_sculpt.py`): a compact, heavy demon whose
swollen belly is carried by a compressed chest and pelvis, a hunched shoulder
girdle that swallows the neck, cartilage bat ears grown from the skull, a
heavy brow over recessed eyes, a snub nose, thick lips over a native-jawed
underbite with tusks, and large connected hands and feet. Only geometry-free
bone positions are read from the private census.
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
sys.path.insert(0, str(HERE))
sys.path.insert(0, str(HERE.parent / "continuous-bodies"))
sys.path.insert(0, str(HERE.parent / "sprint24-theropods"))
import continuous_body as cb
import generate_theropods as donor
import sdf_sculpt as sd

HEAT = {"Torso_Lower", "Torso_Upper", "neck", "neck1", "Head"} | {
    side + name for side in ("L", "R") for name in (
        "_Arm_Upper", "_Arm_Lower", "_Palm", "_Leg0_Upper", "_Leg0_Lower",
        "_Foot0", "_toe")}
BONES = HEAT | {side + name for side in ("L", "R") for name in (
    "_finger1", "_finger2", "_Bfinger2", "_claw")} | {"jaw", "jaw1"}

SKIN, BELLY, EARIN, EYE, MOUTH, CLAW, TOOTH, NOSE, LIP = 0, 1, 2, 3, 4, 5, 6, 7, 8
VOXEL = 0.0065
UP = np.array((0.0, 0.0, 1.0))
FRONT = np.array((0.0, 1.0, 0.0))


def unit(v):
    v = np.asarray(v, dtype=np.float64)
    return v / np.linalg.norm(v)


def rig_points(rig):
    return {name: np.array(rig[name]["head"], dtype=np.float64) for name in rig}


def head_centre(at):
    # Hunched: the skull sits low and forward of the native head control.
    return at["Head"] + np.array((0.0, 0.110, -0.095))


def hand_frame(at, side):
    wrist, f1, b2 = at[side + "_Palm"], at[side + "_finger1"], at[side + "_Bfinger2"]
    axis = unit(f1 - wrist)
    thumb = b2 - wrist
    width = unit(thumb - axis * float(thumb @ axis))
    normal = unit(np.cross(axis, width))
    return wrist, axis, width, normal


def sculpt_body(at):
    f = sd.Field((-0.92, -0.50, -0.02), (0.92, 0.70, 2.08), VOXEL)
    C = head_centre(at)
    pelvis = at["Torso_Lower"]
    # Compressed chest, swollen belly carried on the pelvis, heavy hips.
    f.ellipsoid((0, 0.040, 1.30), (0.295, 0.230, 0.250), material=SKIN)
    f.ellipsoid((0, 0.185, 1.020), (0.315, 0.290, 0.300), k=0.10, material=BELLY)
    f.ellipsoid((0, 0.175, 0.840), (0.270, 0.235, 0.170), k=0.08, material=BELLY)
    f.ellipsoid(pelvis + (0, -0.010, 0.000), (0.270, 0.220, 0.180), k=0.08, material=SKIN)
    for s in (-1, 1):
        f.ellipsoid((s * 0.130, -0.130, 0.860), (0.135, 0.110, 0.130), k=0.06, material=SKIN)
        f.ellipsoid((s * 0.120, 0.130, 1.330), (0.135, 0.085, 0.100), k=0.05, material=SKIN)
        # Hunched trapezius that swallows the neck.
        f.cone((0, -0.050, 1.62), (s * 0.27, -0.050, 1.53), 0.140, 0.100, k=0.08, material=SKIN)
    # Belly folds: two sagging creases across the lower belly and navel.
    def belly_folds(P):
        local = P - np.array((0.0, 0.185, 1.020))
        front = sd.smoothstep(0.05, 0.20, local[..., 1])
        crease = 0.006 * np.exp(-((local[..., 2] + 0.16 + 0.05 * local[..., 0] ** 2 * 8) / 0.018) ** 2)
        crease += 0.004 * np.exp(-((local[..., 2] + 0.06) / 0.015) ** 2) * sd.smoothstep(0.24, 0.12, np.abs(local[..., 0]))
        return front * crease
    f.displace(belly_folds, (-0.4, -0.2, 0.6), (0.4, 0.55, 1.4))
    f.sphere((0, 0.468, 0.975), 0.020, op="cut", k=0.012, material=BELLY)

    # Short thick neck into a big skull.
    f.chain([(0, -0.02, 1.52), at["neck"], C + (0, -0.08, -0.03)], [0.170, 0.160, 0.150], k=0.07, material=SKIN)
    f.ellipsoid(C + (0, -0.020, 0.030), (0.205, 0.200, 0.190), k=0.06, material=SKIN)
    f.ellipsoid(C + (0, -0.110, 0.050), (0.150, 0.110, 0.140), k=0.05, material=SKIN)
    for s in (-1, 1):
        # Heavy cheeks and jowls.
        f.ellipsoid(C + (s * 0.130, 0.090, -0.070), (0.090, 0.090, 0.085), k=0.05, material=SKIN)
        # Brow ridge overhanging recessed eyes.
        f.ellipsoid(C + (s * 0.075, 0.170, 0.075), (0.080, 0.050, 0.040), k=0.03, material=SKIN)
    f.ellipsoid(C + (0, 0.180, 0.080), (0.060, 0.045, 0.035), k=0.03, material=SKIN)
    # Snub nose with flared nostrils.
    f.ellipsoid(C + (0, 0.215, -0.010), (0.060, 0.050, 0.055), k=0.03, material=NOSE)
    f.ellipsoid(C + (0, 0.245, -0.030), (0.055, 0.030, 0.035), k=0.02, material=NOSE)
    for s in (-1, 1):
        f.sphere(C + (s * 0.026, 0.268, -0.040), 0.014, op="cut", k=0.006, material=NOSE)
    # Thick upper lip, wide mouth and a heavy underbitten lower jaw.
    f.ellipsoid(C + (0, 0.195, -0.090), (0.140, 0.065, 0.040), k=0.03, material=LIP)
    f.box(C + (0, 0.150, -0.150), (0.135, 0.090, 0.045), round_=0.040, k=0.03, material=SKIN)
    f.ellipsoid(C + (0, 0.215, -0.140), (0.120, 0.045, 0.040), k=0.025, material=LIP)
    f.box(C + (0, 0.145, -0.112), (0.115, 0.095, 0.007), round_=0.006, op="cut", k=0.006)
    # Recessed eye sockets.
    for s in (-1, 1):
        f.sphere(C + (s * 0.080, 0.170, 0.030), 0.040, op="cut", k=0.012, material=SKIN)

    def wrinkles(P):
        local = P - C
        brow = sd.smoothstep(0.10, 0.04, np.abs(local[..., 0])) * sd.smoothstep(0.06, 0.10, local[..., 2]) * \
            sd.smoothstep(0.10, 0.16, local[..., 1])
        skin = 0.004 * sd.fbm(P, 22.0, 611)
        return brow * 0.003 * np.sin(local[..., 2] * 2 * np.pi / 0.020) + skin
    f.displace(wrinkles, C - 0.30, C + 0.30)
    # Cartilage bat ears grown out of the skull: thick at the root, a thin
    # pointed blade angled up and out, with a hollow inner face.
    for s in (-1, 1):
        root = C + (s * 0.165, -0.010, 0.080)
        tip = C + (s * 0.430, 0.000, 0.300)
        f.cone(root, root + (tip - root) * 0.30, 0.060, 0.040, k=0.04, material=SKIN)
        axes = sd.frame(tip - root, hint=(0, 1, 0))
        mid = root + (tip - root) * 0.50
        def blade(P, mid=mid, axes=axes, root=root, tip=tip):
            q = (P - mid) @ axes.T
            length = np.linalg.norm(tip - root) * 0.55
            t = np.clip((q[..., 2] + length) / (2 * length), 0, 1)
            half_width = 0.110 * (1 - t) ** 0.8 + 0.006
            across = np.abs(q[..., 0]) - half_width
            along = np.abs(q[..., 2]) - length
            flat = np.abs(q[..., 1]) - 0.012
            return np.maximum(np.maximum(across, along), flat)
        lo = np.minimum(root, tip) - 0.15
        hi = np.maximum(root, tip) + 0.15
        f.custom(blade, lo, hi, k=0.020, material=SKIN)
        inner = mid + axes[1] * 0.020 * (1 if s > 0 else 1)
        f.paint(lambda P, mid=mid, axes=axes: (np.abs((P - mid) @ axes[0]) < 0.075) &
                (np.abs((P - mid) @ axes[2]) < 0.10) & (((P - mid) @ axes[1]) > -0.002),
                lo, hi, EARIN)

    for side, s in (("L", 1), ("R", -1)):
        S, E, W = at[side + "_Arm_Upper"], at[side + "_Arm_Lower"], at[side + "_Palm"]
        arm, fore = unit(E - S), unit(W - E)
        f.ellipsoid(S + (s * 0.020, 0.0, 0.030), (0.140, 0.130, 0.125), k=0.07, material=SKIN)
        f.cone(S, E, 0.105, 0.082, k=0.04, material=SKIN)
        f.ellipsoid(S + (E - S) * 0.45 + FRONT * 0.025, (0.070, 0.064, 0.110), axes=sd.frame(arm), k=0.03, material=SKIN)
        f.cone(E, W, 0.088, 0.060, k=0.04, material=SKIN)
        f.ellipsoid(E + (W - E) * 0.25, (0.078, 0.066, 0.110), axes=sd.frame(fore), k=0.03, material=SKIN)
        # Large hand: knuckled palm, four long fingers on the finger chain and
        # a thick thumb rooted on the palm's thumb side.
        wrist, axis, width, normal = hand_frame(at, side)
        f1, f2, b2 = at[side + "_finger1"], at[side + "_finger2"], at[side + "_Bfinger2"]
        palm_axes = np.stack([width, normal, axis])
        f.box(wrist + axis * 0.072, (0.082, 0.040, 0.080), axes=palm_axes, round_=0.030, k=0.028, material=SKIN)
        tip_dir = unit(f2 - f1)
        for offset, length in ((-0.054, 0.030), (-0.019, 0.050), (0.017, 0.046), (0.052, 0.024)):
            base = wrist + axis * 0.135 + width * offset
            mid = f1 + width * offset * 1.05
            distal = f2 + width * offset * 1.05
            tip = distal + tip_dir * length
            f.chain([base, mid, distal, tip], [0.029, 0.026, 0.022, 0.018], k=0.012, material=SKIN)
            f.sphere(mid, 0.0285, k=0.008, material=SKIN)
        thumb_base = wrist + axis * 0.040 + width * 0.055
        f.chain([thumb_base, (thumb_base + b2) / 2 + normal * 0.010, b2 + unit(b2 - thumb_base) * 0.02],
                [0.034, 0.029, 0.022], k=0.014, material=SKIN)

        # Short, thick bowed legs and broad three-toed feet.
        H, K, A, T, Cl = (at[side + n] for n in ("_Leg0_Upper", "_Leg0_Lower", "_Foot0", "_toe", "_claw"))
        thigh, shin = unit(K - H), unit(A - K)
        f.cone(H + (s * 0.02, 0, 0.03), K, 0.160, 0.110, k=0.06, material=SKIN)
        f.ellipsoid(H + (K - H) * 0.45 + FRONT * 0.030 + s * np.array((0.02, 0, 0)), (0.115, 0.105, 0.170),
                    axes=sd.frame(thigh), k=0.05, material=SKIN)
        f.sphere(K, 0.105, k=0.04, material=SKIN)
        f.cone(K, A, 0.098, 0.074, k=0.04, material=SKIN)
        f.ellipsoid(K + (A - K) * 0.30 - FRONT * 0.035, (0.085, 0.080, 0.115), axes=sd.frame(shin), k=0.04, material=SKIN)
        f.cone(A, T + UP * 0.055, 0.074, 0.066, k=0.04, material=SKIN)
        foot_dir = unit((Cl - T) * (1, 1, 0))
        lateral = np.cross(foot_dir, UP)
        f.ellipsoid(T + foot_dir * 0.040 + UP * 0.050, (0.100, 0.115, 0.055),
                    axes=np.stack([lateral, foot_dir, UP]), k=0.04, material=SKIN)
        for spread in (-0.060, 0.0, 0.060):
            toe0 = T + foot_dir * 0.080 + lateral * spread * 0.8 + UP * 0.045
            toe1 = Cl + foot_dir * 0.030 + lateral * spread * 1.3 + UP * 0.032
            f.chain([toe0, toe1], [0.036, 0.026], k=0.020, material=SKIN)
        heel = T - foot_dir * 0.065 + UP * 0.050
        f.sphere(heel, 0.055, k=0.04, material=SKIN)
        f.custom(lambda P: -P[..., 2] + 0.004, (H[0] - 0.25, H[1] - 0.45, -0.02),
                 (H[0] + 0.25, H[1] + 0.45, 0.04), op="intersect")
    return f


def weights_for_body(body, armature, at):
    points = [v.co.copy() for v in body.data.vertices]
    rows = cb.heat_weights(body, armature, HEAT)
    rows = cb.clean_weights(cb.smooth_weights(body, rows), points, armature, HEAT)
    rows = {index: dict(row) for index, row in rows.items()}
    P = np.array([tuple(p) for p in points])
    C = head_centre(at)
    for index, p in enumerate(P):
        local = p - C
        if np.linalg.norm(local) > 0.62:
            continue
        row = rows[index]
        # The whole skull and ears follow the head control; the neck blends.
        head = sd.smoothstep(-0.16, -0.08, local[2] + 0.55 * local[1]) * \
            sd.smoothstep(-0.18, -0.10, local[1] + 0.3)
        if head > 0:
            row = {k: v * (1 - head) for k, v in row.items()}
            row["Head"] = row.get("Head", 0.0) + head
        line = -0.112
        # Bounded to the jaw itself: the chest and belly below never follow it.
        jaw = sd.smoothstep(line + 0.010, line - 0.010, local[2]) * sd.smoothstep(-0.240, -0.205, local[2]) * \
            sd.smoothstep(0.03, 0.09, local[1]) * sd.smoothstep(0.155, 0.130, abs(local[0]))
        if jaw > 0:
            row = {k: v * (1 - jaw) for k, v in row.items()}
            row["jaw1"] = row.get("jaw1", 0.0) + 0.6 * jaw
            row["jaw"] = row.get("jaw", 0.0) + 0.4 * jaw
        rows[index] = row
    for side in ("L", "R"):
        wrist, axis, width, normal = hand_frame(at, side)
        f1, f2, b2 = at[side + "_finger1"], at[side + "_finger2"], at[side + "_Bfinger2"]
        t1 = float((f1 - wrist) @ axis)
        t2 = float((f2 - wrist) @ axis)
        thumb_base = wrist + axis * 0.040 + width * 0.055
        thumb_dir = unit(b2 - thumb_base)
        thumb_len = float(np.linalg.norm(b2 - thumb_base))
        for index, p in enumerate(P):
            rel = p - wrist
            along = float(rel @ axis)
            if along < 0.02 or np.linalg.norm(rel) > 0.40:
                continue
            tb = float((p - thumb_base) @ thumb_dir) / thumb_len
            thumb_gap = np.linalg.norm(np.cross(p - thumb_base, thumb_dir))
            if tb > 0.15 and thumb_gap < 0.045 and float(rel @ width) > 0.040:
                share = sd.smoothstep(0.25, 0.85, tb)
                row = {side + "_Palm": 1 - share, side + "_Bfinger2": share}
            elif along > 0.11 and abs(float(rel @ normal)) < 0.06:
                if along < t1:
                    share = sd.smoothstep(0.11, t1, along)
                    row = {side + "_Palm": 1 - share, side + "_finger1": share}
                else:
                    share = sd.smoothstep(t1 + 0.02, t2, along)
                    row = {side + "_finger1": 1 - share, side + "_finger2": share}
            else:
                continue
            rows[index] = {k: v for k, v in row.items() if v > 1e-4}
        T, Cl = at[side + "_toe"], at[side + "_claw"]
        foot = unit((Cl - T) * (1, 1, 0))
        for index, p in enumerate(P):
            if p[2] > 0.14 or abs(p[0] - T[0]) > 0.16:
                continue
            ahead = float((p - T) @ foot)
            toe = sd.smoothstep(0.03, 0.10, ahead)
            claw = sd.smoothstep(0.14, 0.22, ahead)
            if toe > 0:
                row = {k: v * (1 - toe) for k, v in rows[index].items()}
                row[side + "_toe"] = row.get(side + "_toe", 0.0) + toe * (1 - claw)
                row[side + "_claw"] = row.get(side + "_claw", 0.0) + toe * claw
                rows[index] = row
    out = {}
    for index, row in rows.items():
        items = sorted(((k, v) for k, v in dict(row).items() if v > 0), key=lambda e: (-e[1], e[0]))[:4]
        total = sum(v for _, v in items)
        out[index] = [(k, v / total) for k, v in items]
    return out


def build(rig, armature):
    at = rig_points(rig)
    body = sculpt_body(at)
    body_obj = body.polygonise("DretchBody", largest_only=True)
    cb.finish_surface(body_obj, 8200, smooth_iterations=2)
    body_rows = weights_for_body(body_obj, armature, at)
    parts = [(body_obj, body_rows, SKIN)]
    C = head_centre(at)
    X, Y, Z = (Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1)))

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), region))
    for s in (-1, 1):
        rigid(cb.blob("DretchEye", Vector(C + (s * 0.080, 0.180, 0.030)), (X, Y, Z),
                      (0.026, 0.020, 0.022), 14, 9), [("Head", 1.0)], EYE)
        # Upper fangs and big lower tusks of the underbite.
        rigid(cb.horn("DretchFang", Vector(C + (s * 0.075, 0.205, -0.098)),
                      Vector(C + (s * 0.075, 0.212, -0.150)), 0.014, 8, 4, hint=Y), [("Head", 1.0)], TOOTH)
        rigid(cb.horn("DretchTusk", Vector(C + (s * 0.100, 0.200, -0.140)),
                      Vector(C + (s * 0.108, 0.214, -0.040)), 0.022, 10, 5, curve=Vector((s * 0.012, 0.010, 0)),
                      hint=Y), [("jaw1", 0.6), ("jaw", 0.4)], TOOTH)
    for side in ("L", "R"):
        wrist, axis, width, normal = hand_frame(at, side)
        f1, f2, b2 = at[side + "_finger1"], at[side + "_finger2"], at[side + "_Bfinger2"]
        tip_dir = unit(f2 - f1)
        for offset, length in ((-0.054, 0.030), (-0.019, 0.050), (0.017, 0.046), (0.052, 0.024)):
            tip = f2 + width * offset * 1.05 + tip_dir * length
            claw_dir = unit(tip_dir * 0.85 - normal * 0.5)
            rigid(cb.horn("DretchHandClaw", Vector(tip - tip_dir * 0.012), Vector(tip + claw_dir * 0.090),
                          0.016, 8, 4, curve=Vector(-normal * 0.014), hint=Vector(normal)),
                  [(side + "_finger2", 1.0)], CLAW)
        thumb_base = wrist + axis * 0.040 + width * 0.055
        thumb_tip = b2 + unit(b2 - thumb_base) * 0.02
        rigid(cb.horn("DretchThumbClaw", Vector(thumb_tip - unit(b2 - thumb_base) * 0.008),
                      Vector(thumb_tip + unit(b2 - thumb_base) * 0.060), 0.016, 8, 4, hint=Vector(normal)),
              [(side + "_Bfinger2", 1.0)], CLAW)
        T, Cl = at[side + "_toe"], at[side + "_claw"]
        foot_dir = unit((Cl - T) * (1, 1, 0))
        lateral = np.cross(foot_dir, UP)
        s = 1 if side == "L" else -1
        for spread in (-0.060, 0.0, 0.060):
            tip = Cl + foot_dir * 0.050 + lateral * spread * 1.3 + UP * 0.030
            rigid(cb.horn("DretchFootClaw", Vector(tip - foot_dir * 0.015), Vector(tip + foot_dir * 0.075 - UP * 0.025),
                          0.016, 8, 4, curve=Vector(UP * 0.010), hint=Vector(UP)),
                  [(side + "_claw", 1.0)], CLAW)
    mesh, weights = cb.assemble("DretchOriginal", parts)
    cb.require_single_body(mesh)
    names = {name for row in weights.values() for name, _ in row}
    if names != BONES:
        raise SystemExit("bone set differs from the reviewed contract: " + json.dumps(sorted(names ^ BONES)))
    return mesh, weights, body


def colours_for(body, at):
    C = head_centre(at)

    def colours(p, n, region):
        out = np.zeros((len(p), 3))
        skin_like = region == SKIN
        mat = region.copy()
        mat[skin_like] = body.material_at(p[skin_like], n[skin_like])
        ao = np.ones(len(p))
        ao[skin_like] = body.cavity(p[skin_like], n[skin_like])
        broad = cb.fbm(p, 4.0, 311)
        fine = cb.fbm(p, 40.0, 312)
        mottle = sd.smoothstep(0.1, 0.5, sd.fbm(p, 9.0, 313))
        # Slate grey with a cool violet cast, darker toward the hands and feet.
        skin = np.array([.36, .38, .44]) * (0.86 + 0.20 * broad[:, None])
        skin = skin * (1.0 - 0.12 * mottle[:, None]) + np.array([.04, .0, .05]) * mottle[:, None]
        extremity = np.clip((0.55 - p[:, 2]) / 0.5, 0, 1) * 0.25
        skin = skin * (1 - extremity[:, None])
        belly = np.array([.66, .54, .50]) * (0.88 + 0.16 * broad[:, None])
        palette = {BELLY: belly, EARIN: np.array([.62, .38, .36]), EYE: np.array([.82, .62, .18]),
                   MOUTH: np.array([.22, .08, .08]), CLAW: np.array([.09, .08, .085]),
                   TOOTH: np.array([.80, .74, .58]), NOSE: np.array([.46, .38, .44]),
                   LIP: np.array([.38, .30, .36])}
        bellyness = sd.smoothstep(0.035, -0.045, sd.d_ellipsoid(p, (0, 0.205, 0.975), (0.305, 0.300, 0.330)))
        bellyness = bellyness * sd.smoothstep(-0.05, 0.10, p[:, 1] - 0.05)
        out[:] = skin * (1 - bellyness[:, None]) + belly * bellyness[:, None]
        palette.pop(BELLY)
        for key, colour in palette.items():
            sel = mat == key
            if sel.any():
                out[sel] = colour if colour.ndim == 1 else colour[sel]
        # Soft transition from slate skin into the warmer belly.
        out = out * (0.94 + 0.08 * fine[:, None])
        eye = mat == EYE
        if eye.any():
            for s in (-1, 1):
                centre = C + (s * 0.080, 0.200, 0.030)
                pupil = np.abs((p[eye] - centre) @ np.array((1.0, 0.0, 0.0))) < 0.004
                pupil &= np.linalg.norm(p[eye] - centre, axis=1) < 0.018
                sub = out[eye]
                sub[pupil] = (0.03, 0.02, 0.02)
                out[eye] = sub
        shade = 0.52 + 0.48 * ao
        return out * shade[:, None]
    return colours


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out", "preview"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = donor.measured_rig(args.capture)
    armature = cb.review_armature(rig_data, "DretchReviewRig")
    mesh, weights, body = build(rig, armature)
    cb.unwrap(mesh)
    cb.bake_albedo(mesh, 1024, colours_for(body, rig_points(rig)), args.albedo)
    cb.attach_albedo_preview(mesh, args.albedo)
    extra = {"creature": "dretch", "donorFamily": "lizardfolk-free-hands",
             "nativeDonor": donor.DONOR_GUID, "nativePrefab": donor.PREFAB_GUID,
             "bodyRenderer": donor.BODY_RENDERER, "accessoryRenderer": donor.ACCESSORY_RENDERER,
             "nativeRendererCount": 2, "visibleArms": 2, "visibleLegs": 2,
             "visibleTail": False, "potbelly": True, "largeEars": True, "jawJoints": 2}
    summary = cb.write_mesh_data(args.mesh_data, mesh, weights, rig_hash, args.albedo,
                                 "donor renderer local; +X lateral, +Y forward, +Z up", extra)
    report = {"schemaVersion": 1, "builder": Path(__file__).name, "rigSha256": rig_hash,
              **extra, **summary, "albedo": cb.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    groups = {}
    for index, row in weights.items():
        for name, value in row:
            group = groups.get(name) or mesh.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    modifier = mesh.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    mesh.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    scene = bpy.context.scene
    scene.render.engine = 'CYCLES'
    scene.cycles.device = 'CPU'
    scene.cycles.samples = 16
    scene.world = scene.world or bpy.data.worlds.new("DretchPreviewWorld")
    scene.world.color = (.18, .18, .18)
    bpy.ops.object.camera_add(location=(3.1, 4.8, 3.0))
    camera = bpy.context.object
    camera.rotation_euler = (Vector((0, 0, 1.0)) - camera.location).to_track_quat('-Z', 'Y').to_euler()
    camera.data.type = 'ORTHO'
    camera.data.ortho_scale = 2.65
    scene.camera = camera
    bpy.ops.object.light_add(type='AREA', location=(2.5, 3.5, 5.0))
    light = bpy.context.object
    light.data.energy = 550
    light.data.shape = 'DISK'
    light.data.size = 4
    light.rotation_euler = (-light.location).to_track_quat('-Z', 'Y').to_euler()
    scene.render.resolution_x, scene.render.resolution_y = 640, 640
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = 'PNG'
    scene.render.filepath = args.preview
    bpy.ops.render.render(write_still=True)
    print('[dretch] ' + json.dumps(report, sort_keys=True))


if __name__ == '__main__':
    main()
