"""Original mastiff-headed Hound Archon on the measured free-hands rig.

The October 25 refinement replaces the first union-of-primitives body with a
signed-distance sculpt (`sdf_sculpt.py`): a mastiff skull with brow, stop,
short broad muzzle, flews and a separate lower jaw; recessed eyes; floppy
ears grown from the skull; a heavy humanoid torso on the measured joints;
connected thumbs and fingers; digitigrade canine legs in leather boots. The
pale garment is real geometry now - a sleeveless tunic with neckline, belt,
split hem and broad folds, and a mantle over the right shoulder - instead of a
colour mask on the body. No native mesh, texture or animation is copied; only
geometry-free bone positions are read from the private census.
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
FINGERS = {side + name for side in ("L", "R") for name in (
    "_finger1", "_finger2", "_Bfinger1", "_Bfinger2")}
BONES = HEAT | FINGERS | {"jaw", "jaw1"}

FUR, MUZZLE, NOSE, EYE, PUPIL, MOUTH, CLAW, TOOTH = 0, 1, 2, 3, 4, 5, 6, 7
CLOTH, TRIM, LEATHER, BRONZE, EAR, MANTLE = 8, 9, 10, 11, 12, 13
VOXEL = 0.0060
GARMENT_LO, GARMENT_HI, GARMENT_VOXEL = (-0.47, -0.36, 0.50), (0.47, 0.46, 1.74), 0.0045
UP = np.array((0.0, 0.0, 1.0))
FRONT = np.array((0.0, 1.0, 0.0))


def unit(v):
    v = np.asarray(v, dtype=np.float64)
    return v / np.linalg.norm(v)


def head_centre(at):
    return at["Head"] + np.array((0.0, 0.030, 0.040))


def hand_frame(at, side):
    wrist, f1, b1 = at[side + "_Palm"], at[side + "_finger1"], at[side + "_Bfinger1"]
    axis = unit(f1 - wrist)
    thumb = b1 - wrist
    width = unit(thumb - axis * float(thumb @ axis))
    normal = unit(np.cross(axis, width))
    return wrist, axis, width, normal


def torso(f, at, scale=1.0, k=0.07):
    """Heavy warrior torso shared by the body and the cloth form."""
    pelvis = at["Torso_Lower"]
    f.ellipsoid((0, 0.030, 1.36), (0.235 * scale, 0.165 * scale, 0.225 * scale), material=FUR)
    f.ellipsoid((0, 0.035, 1.12), (0.182 * scale, 0.146 * scale, 0.170 * scale), k=k, material=FUR)
    f.ellipsoid(pelvis + (0, 0.020, 0.020), (0.200 * scale, 0.155 * scale, 0.145 * scale), k=k, material=FUR)
    for s in (-1, 1):
        f.ellipsoid((s * 0.100, 0.150, 1.385), (0.115 * scale, 0.066 * scale, 0.086 * scale), k=0.045, material=FUR)
        f.ellipsoid((s * 0.150, -0.080, 1.300), (0.100 * scale, 0.090 * scale, 0.180 * scale), k=0.055, material=FUR)
        f.ellipsoid((s * 0.088, -0.122, 0.868), (0.102 * scale, 0.092 * scale, 0.112 * scale), k=0.055, material=FUR)


def sculpt_body(at):
    f = sd.Field((-0.88, -0.46, -0.02), (0.88, 0.58, 2.14), VOXEL)
    C = head_centre(at)
    torso(f, at)
    for s in (-1, 1):
        # Trapezius raising a short, powerful shoulder line.
        f.cone((0, -0.03, 1.60), (s * 0.25, -0.03, 1.535), 0.092, 0.072, k=0.06, material=FUR)

    # Thick neck, dewlap and mastiff skull.
    f.chain([(0, -0.005, 1.45), at["neck"], at["neck1"], C + (0, -0.04, -0.05)],
            [0.140, 0.132, 0.112, 0.100], k=0.06, material=FUR)
    f.ellipsoid(C + (0, 0.020, -0.165), (0.095, 0.085, 0.072), k=0.05, material=FUR)
    f.ellipsoid(C + (0, -0.020, 0.030), (0.145, 0.138, 0.122), k=0.05, material=FUR)
    f.ellipsoid(C + (0, -0.100, 0.020), (0.085, 0.060, 0.085), k=0.04, material=FUR)
    for s in (-1, 1):
        f.ellipsoid(C + (s * 0.100, 0.050, -0.040), (0.080, 0.085, 0.080), k=0.05, material=FUR)
        f.ellipsoid(C + (s * 0.055, 0.115, 0.040), (0.050, 0.035, 0.028), k=0.025, material=FUR)
    f.sphere(C + (0, 0.137, 0.048), 0.016, op="cut", k=0.014, material=FUR)
    # Short, broad, deep muzzle; nose leather; philtrum and nostrils.
    f.box(C + (0, 0.180, -0.048), (0.082, 0.062, 0.066), round_=0.056, k=0.040, material=MUZZLE)
    f.ellipsoid(C + (0, 0.236, -0.004), (0.050, 0.030, 0.034), k=0.016, material=NOSE)
    # Deep stop between the brows and a dark mask around the eyes.
    f.sphere(C + (0, 0.150, 0.008), 0.020, op="cut", k=0.020, material=FUR)

    f.box(C + (0, 0.262, -0.040), (0.004, 0.014, 0.030), round_=0.003, op="cut", k=0.005, material=MUZZLE)
    for s in (-1, 1):
        f.sphere(C + (s * 0.022, 0.264, -0.008), 0.011, op="cut", k=0.004, material=NOSE)
        # Hanging flews over the lower jaw.
        f.ellipsoid(C + (s * 0.060, 0.180, -0.108), (0.056, 0.078, 0.062), k=0.03, material=MUZZLE)
    f.ellipsoid(C + (0, 0.222, -0.112), (0.070, 0.034, 0.046), k=0.03, material=MUZZLE)
    # Lower jaw (native jaw controls) and a 12 mm mouth line above it.
    f.box(C + (0, 0.130, -0.150), (0.065, 0.100, 0.032), round_=0.028, k=0.02, material=MUZZLE)
    f.box(C + (0, 0.132, -0.130), (0.052, 0.074, 0.006), round_=0.005, op="cut", k=0.006, material=MOUTH)
    # Recessed eyes under the brow, heavy lower lids.
    for s in (-1, 1):
        f.sphere(C + (s * 0.060, 0.112, 0.012), 0.026, op="cut", k=0.010, material=FUR)
        f.ellipsoid(C + (s * 0.058, 0.116, -0.017), (0.032, 0.014, 0.012), k=0.010, material=MUZZLE)

    def wrinkles(P):
        local = P - C
        band = sd.smoothstep(0.085, 0.03, np.abs(local[..., 0])) * \
            sd.smoothstep(0.025, 0.06, local[..., 2]) * sd.smoothstep(0.04, 0.10, local[..., 1])
        return 0.0040 * band * np.sin(local[..., 2] * 2 * np.pi / 0.021 + 180 * local[..., 0] ** 2)
    f.displace(wrinkles, C - 0.18, C + 0.18)
    # The dark mastiff mask reaches up around the eyes (painted last, so no
    # later socket or brow primitive overrides it).
    for s in (-1, 1):
        eye_c = C + (s * 0.060, 0.112, 0.012)
        f.paint(lambda P, e=eye_c: np.linalg.norm((P - e) * np.array((1.0, 1.0, 1.25)), axis=-1) < 0.050,
                eye_c - 0.07, eye_c + 0.07, MUZZLE)
    # Large floppy ears set high on the skull: thick at the fold, thin and
    # rounded where they hang beside the cheeks.
    for s in (-1, 1):
        root = C + (s * 0.128, -0.010, 0.092)
        fold = C + (s * 0.158, 0.012, 0.050)
        tip = C + (s * 0.158, 0.040, -0.075)
        f.cone(root, fold, 0.030, 0.026, k=0.025, material=EAR)
        axes = sd.frame(tip - fold, hint=(s, 0.0, 0))
        f.ellipsoid((fold + tip) / 2, (0.060, 0.012, 0.078), axes=axes, k=0.020, material=EAR)

    for side, s in (("L", 1), ("R", -1)):
        S, E, W = at[side + "_Arm_Upper"], at[side + "_Arm_Lower"], at[side + "_Palm"]
        arm, fore = unit(E - S), unit(W - E)
        f.ellipsoid(S + (s * 0.024, 0.0, 0.024), (0.132, 0.124, 0.118), k=0.060, material=FUR)
        f.cone(S, E, 0.112, 0.084, k=0.040, material=FUR)
        f.ellipsoid(S + (E - S) * 0.45 + FRONT * 0.034, (0.078, 0.070, 0.112), axes=sd.frame(arm), k=0.03, material=FUR)
        f.ellipsoid(S + (E - S) * 0.40 - FRONT * 0.040, (0.076, 0.068, 0.114), axes=sd.frame(arm), k=0.03, material=FUR)
        f.cone(E, W, 0.092, 0.062, k=0.040, material=FUR)
        f.ellipsoid(E + (W - E) * 0.22, (0.080, 0.068, 0.110), axes=sd.frame(fore), k=0.03, material=FUR)
        b0, b1 = E + (W - E) * 0.34, E + (W - E) * 0.95
        f.cone(b0, b1, 0.086, 0.068, k=0.004, material=LEATHER)
        for end, r in ((b0, 0.091), (b1, 0.073)):
            f.cone(end - fore * 0.009, end + fore * 0.009, r, r, k=0.003, material=BRONZE)

        wrist, axis, width, normal = hand_frame(at, side)
        f1, f2 = at[side + "_finger1"], at[side + "_finger2"]
        palm_axes = np.stack([width, normal, axis])
        f.box(wrist + axis * 0.068, (0.060, 0.034, 0.066), axes=palm_axes, round_=0.026, k=0.024, material=FUR)
        tip_dir = unit(f2 - f1)
        for offset, length in ((-0.036, 0.010), (-0.012, 0.022), (0.012, 0.020), (0.034, 0.006)):
            base = wrist + axis * 0.120 + width * offset
            mid = f1 + width * offset * 0.92
            distal = f2 + width * offset * 0.84
            tip = distal + tip_dir * length
            f.chain([base, mid, distal, tip], [0.0220, 0.0200, 0.0170, 0.0140], k=0.010, material=FUR)
            f.sphere(tip + tip_dir * 0.007, 0.0120, k=0.004, material=CLAW)
        b1p, b2p = at[side + "_Bfinger1"], at[side + "_Bfinger2"]
        thumb_tip = b2p + unit(b2p - b1p) * 0.022
        f.ellipsoid(wrist + axis * 0.045 + width * 0.032 + normal * 0.006, (0.030, 0.025, 0.044),
                    axes=palm_axes, k=0.016, material=FUR)
        f.chain([wrist + axis * 0.035 + width * 0.042, b1p, b2p, thumb_tip],
                [0.027, 0.023, 0.019, 0.016], k=0.012, material=FUR)
        f.sphere(thumb_tip + unit(b2p - b1p) * 0.007, 0.013, k=0.004, material=CLAW)

        H, K, A, T, Cl = (at[side + n] for n in ("_Leg0_Upper", "_Leg0_Lower", "_Foot0", "_toe", "_claw"))
        thigh, shin = unit(K - H), unit(A - K)
        f.cone(H + (0, 0, 0.03), K, 0.125, 0.086, k=0.055, material=FUR)
        f.ellipsoid(H + (K - H) * 0.42 + FRONT * 0.032, (0.096, 0.090, 0.170), axes=sd.frame(thigh), k=0.045, material=FUR)
        f.ellipsoid(H + (K - H) * 0.45 - FRONT * 0.044, (0.086, 0.080, 0.160), axes=sd.frame(thigh), k=0.045, material=FUR)
        f.sphere(K + FRONT * 0.012, 0.080, k=0.035, material=FUR)
        f.cone(K, A, 0.075, 0.056, k=0.035, material=FUR)
        f.ellipsoid(K + (A - K) * 0.32 - FRONT * 0.032 + UP * 0.012, (0.068, 0.066, 0.120),
                    axes=sd.frame(shin), k=0.035, material=FUR)
        # Boot: folded cuff over the hock, shaft over the metatarsus, round toe.
        f.cone(A + UP * 0.012, T + UP * 0.052, 0.060, 0.055, k=0.02, material=LEATHER)
        f.cone(A + UP * 0.000, A + UP * 0.038, 0.069, 0.066, k=0.006, material=LEATHER)
        f.cone(A + UP * 0.038, A + UP * 0.049, 0.071, 0.071, k=0.003, material=BRONZE)
        foot_dir = unit((Cl - T) * (1, 1, 0))
        heel = T - foot_dir * 0.055 + UP * 0.052
        toe = Cl + foot_dir * 0.020 + UP * 0.044
        f.chain([heel, T + foot_dir * 0.06 + UP * 0.046, toe], [0.050, 0.054, 0.043], k=0.03, material=LEATHER)
        f.custom(lambda P: -P[..., 2] + 0.004, (H[0] - 0.2, H[1] - 0.4, -0.02), (H[0] + 0.2, H[1] + 0.4, 0.03),
                 op="intersect")
    return f


def garment_form(at, shoulder=None):
    """The smoothed torso and thighs the cloth hangs from. `shoulder` adds
    one shoulder cap (the mantle rests on it; the tunic is sleeveless)."""
    g = sd.Field(GARMENT_LO, GARMENT_HI, GARMENT_VOXEL)
    torso(g, at, scale=1.045, k=0.11)
    for side in ("L", "R"):
        H, K = at[side + "_Leg0_Upper"], at[side + "_Leg0_Lower"]
        g.cone(H + (0, 0, 0.04), K + (0, 0.01, 0.0), 0.145, 0.110, k=0.10)

    def skirt(P):
        Q = P * np.array((1.0, 1.12, 1.0))
        return sd.d_round_cone(Q, (0, 0.0, 1.00), (0, 0.075 * 1.12, 0.58), 0.205, 0.270)
    g.custom(skirt, (-0.36, -0.36, 0.48), (0.36, 0.46, 1.06), k=0.10)
    if shoulder is not None:
        S = at[shoulder + "_Arm_Upper"]
        s = 1 if shoulder == "L" else -1
        g.ellipsoid(S + (s * 0.020, 0, 0.030), (0.125, 0.118, 0.112), k=0.10)
    return g


def sculpt_garment(at):
    form = garment_form(at)
    mantle_form = garment_form(at, shoulder="R")
    X, Y, Z = form.grid()
    P = np.stack([X, Y, Z], axis=-1)
    theta = np.arctan2(Y, X)
    hem_amp = sd.smoothstep(1.00, 0.60, Z)
    folds = 0.010 * hem_amp * np.sin(theta * 9.0 + 1.3 * np.sin(theta * 2.0) + (Z - 0.6) * 4.0)
    chest_folds = 0.0035 * sd.smoothstep(1.06, 1.28, Z) * sd.smoothstep(1.52, 1.34, Z) * \
        np.sin(theta * 7.0 + Z * 22.0)
    G = (form.d + folds + chest_folds).astype(np.float32)
    tunic = np.maximum(G - 0.014, -G)
    neck = np.sqrt((X / 0.135) ** 2 + ((Y - 0.030) / 0.120) ** 2) - 1.0
    neck_cut = np.maximum(neck * 0.12, 1.47 - Z)
    v_neck = np.maximum(np.maximum(np.abs(X) - (Z - 1.31) * 0.42, 1.31 - Z), 0.06 - Y)
    tunic = sd.smax(tunic, -np.minimum(neck_cut, v_neck), 0.006)
    for side, s in (("L", 1), ("R", -1)):
        S, E = at[side + "_Arm_Upper"], at[side + "_Arm_Lower"]
        hole = sd.d_round_cone(P, S + (s * -0.030, 0, -0.050), E, 0.112, 0.100)
        tunic = sd.smax(tunic, -hole, 0.008)
        slit = np.maximum(np.maximum(np.abs(X - s * 0.27) - 0.13, np.abs(Y + 0.01) - 0.024), Z - 0.84)
        tunic = sd.smax(tunic, -slit, 0.010)
    hem = 0.565 + 0.012 * np.sin(theta * 3.0) + 0.03 * sd.smoothstep(0.16, 0.24, np.abs(X))
    tunic = sd.smax(tunic, hem - Z, 0.006)
    out = sd.Field(GARMENT_LO, GARMENT_HI, GARMENT_VOXEL)
    out.d = tunic.astype(np.float32)
    out.mat[:] = CLOTH
    out.mat[(Z < hem + 0.032) & (tunic < 0.02)] = TRIM
    belt = np.maximum(np.maximum(G - 0.026, 0.004 - G), np.abs(Z - 1.030) - 0.027)
    out.d = np.minimum(out.d, belt).astype(np.float32)
    out.mat[belt < 0.004] = LEATHER
    buckle = sd.d_box(P, (0, 0.222, 1.030), (0.034, 0.012, 0.031), round_=0.006)
    out.d = np.minimum(out.d, buckle).astype(np.float32)
    out.mat[buckle < 0.004] = BRONZE
    # Mantle over the right shoulder, across the chest to the left hip; its
    # own folds run across the band and its lower edge is rolled.
    lsh = np.array((-0.27, 0.0, 1.62))
    hip = np.array((0.22, 0.0, 0.92))
    n = unit(np.cross(hip - lsh, FRONT))
    along = unit(hip - lsh)
    plane = (P - lsh) @ n
    run = (P - lsh) @ along
    M = mantle_form.d + 0.006 * np.sin(run * 2 * np.pi / 0.11 + plane * 18) + folds * 0.5
    slab = np.abs(plane) - 0.105
    mantle = np.maximum(np.maximum(M - 0.032, 0.011 - M), slab)
    roll = np.maximum(np.maximum(M - 0.040, 0.012 - M), np.abs(plane + 0.103) - 0.014)
    mantle = np.maximum(np.minimum(mantle, roll), 0.56 - Z)
    out.d = sd.smin(out.d, mantle, 0.004).astype(np.float32)
    out.mat[(mantle < 0.004) & (mantle <= tunic)] = MANTLE
    brooch = sd.d_ellipsoid(P, (-0.215, 0.170, 1.45), (0.032, 0.013, 0.032))
    out.d = np.minimum(out.d, brooch).astype(np.float32)
    out.mat[brooch < 0.004] = BRONZE
    return out, form


def rig_points(rig):
    return {name: np.array(rig[name]["head"], dtype=np.float64) for name in rig}


def inset_under_cloth(body, form, garment):
    """Pull the body 9 mm in wherever cloth actually covers it, so skin cannot
    show through the shell in the bind pose (armholes keep their shape)."""
    sl, P = body._block(np.array(GARMENT_LO), np.array(GARMENT_HI))
    flat = P.reshape(-1, 3)
    fd = form.sample(flat).reshape(P.shape[:3])
    gd = garment.sample(flat).reshape(P.shape[:3])
    covered = sd.smoothstep(0.05, 0.0, fd) * sd.smoothstep(0.045, 0.02, gd)
    body.d[sl] = (body.d[sl] + 0.009 * covered).astype(np.float32)


def weights_for_body(body, armature, at):
    points = [v.co.copy() for v in body.data.vertices]
    rows = cb.heat_weights(body, armature, HEAT)
    rows = cb.clean_weights(cb.smooth_weights(body, rows), points, armature, HEAT)
    rows = {index: dict(row) for index, row in rows.items()}
    P = np.array([tuple(p) for p in points])
    C = head_centre(at)
    for index, p in enumerate(P):
        local = p - C
        if np.linalg.norm(local) > 0.32:
            continue
        row = dict(rows[index])
        head = sd.smoothstep(-0.10, -0.05, local[2] + 0.45 * local[1])
        if head > 0:
            row = {k: v * (1 - head) for k, v in row.items()}
            row["Head"] = row.get("Head", 0.0) + head
        # The lower jaw below the mouth line follows the native jaw controls;
        # the outer flews stay with the skull and the mouth corners blend.
        # Under the hanging front lip the chin starts lower.
        line = -0.130 - 0.026 * sd.smoothstep(0.17, 0.21, local[1])
        # Bounded to the jaw itself: the dewlap and chest below never follow it.
        jaw = sd.smoothstep(line + 0.008, line - 0.008, local[2]) * sd.smoothstep(-0.215, -0.190, local[2]) * \
            sd.smoothstep(0.06, 0.10, local[1]) * sd.smoothstep(0.066, 0.054, abs(local[0]))
        if jaw > 0:
            row = {k: v * (1 - jaw) for k, v in row.items()}
            row["jaw1"] = row.get("jaw1", 0.0) + 0.65 * jaw
            row["jaw"] = row.get("jaw", 0.0) + 0.35 * jaw
        rows[index] = row
    for side in ("L", "R"):
        wrist, axis, width, normal = hand_frame(at, side)
        f1, f2 = at[side + "_finger1"], at[side + "_finger2"]
        b1, b2 = at[side + "_Bfinger1"], at[side + "_Bfinger2"]
        t1 = float((f1 - wrist) @ axis)
        t2 = float((f2 - wrist) @ axis)
        for index, p in enumerate(P):
            rel = p - wrist
            along = float(rel @ axis)
            if along < 0.02 or np.linalg.norm(rel) > 0.34:
                continue
            thumb_gap = np.linalg.norm(np.cross(p - b1, unit(b2 - b1)))
            finger_gap = abs(float(rel @ normal))
            across = float(rel @ width)
            if across > 0.028 and along < t1 and thumb_gap < 0.045:
                tb = float((p - b1) @ unit(b2 - b1)) / max(np.linalg.norm(b2 - b1), 1e-6)
                if tb < 0:
                    share = sd.smoothstep(-0.6, 0.0, tb)
                    row = {side + "_Palm": 1 - share, side + "_Bfinger1": share}
                else:
                    share = sd.smoothstep(0.3, 0.9, tb)
                    row = {side + "_Bfinger1": 1 - share, side + "_Bfinger2": share}
            elif along > 0.10 and finger_gap < 0.05:
                if along < t1:
                    share = sd.smoothstep(0.10, t1, along)
                    row = {side + "_Palm": 1 - share, side + "_finger1": share}
                else:
                    share = sd.smoothstep(t1 + 0.02, t2, along)
                    row = {side + "_finger1": 1 - share, side + "_finger2": share}
            else:
                continue
            rows[index] = {k: v for k, v in row.items() if v > 1e-4}
        # Boot toes follow the native toe control.
        T, Cl = at[side + "_toe"], at[side + "_claw"]
        foot = unit((Cl - T) * (1, 1, 0))
        for index, p in enumerate(P):
            if p[2] > 0.13 or abs(p[0] - T[0]) > 0.10:
                continue
            ahead = float((p - T) @ foot)
            share = sd.smoothstep(0.02, 0.10, ahead)
            if share > 0:
                row = {k: v * (1 - share) for k, v in rows[index].items()}
                row[side + "_toe"] = row.get(side + "_toe", 0.0) + share
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
    garment, form = sculpt_garment(at)
    inset_under_cloth(body, form, garment)
    body_obj = body.polygonise("HoundArchonBody", largest_only=True)
    cb.finish_surface(body_obj, 7400, smooth_iterations=2)
    body_rows = weights_for_body(body_obj, armature, at)
    body_points = np.array([tuple(v.co) for v in body_obj.data.vertices])
    garment_obj = garment.polygonise("HoundArchonGarment")
    cb.finish_surface(garment_obj, 2800, smooth_iterations=1)
    cloth_points = np.array([tuple(v.co) for v in garment_obj.data.vertices])
    cloth_rows = sd.closest_rows(cloth_points, body_points, [body_rows[i] for i in range(len(body_points))])
    parts = [(body_obj, body_rows, FUR), (garment_obj, cloth_rows, CLOTH)]
    C = head_centre(at)

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), region))
    for s in (-1, 1):
        eye = C + (s * 0.060, 0.106, 0.011)
        rigid(cb.blob("HoundEye", Vector(eye), (Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1))),
                      (0.022, 0.018, 0.019), 14, 9), [("Head", 1.0)], EYE)
        rigid(cb.horn("HoundUpperCanine", Vector(C + (s * 0.046, 0.226, -0.112)),
                      Vector(C + (s * 0.046, 0.229, -0.150)), 0.0095, 8, 4, hint=Vector((0, 1, 0))),
              [("Head", 1.0)], TOOTH)
        rigid(cb.horn("HoundLowerCanine", Vector(C + (s * 0.040, 0.205, -0.140)),
                      Vector(C + (s * 0.040, 0.207, -0.110)), 0.0090, 8, 4, hint=Vector((0, 1, 0))),
              [("jaw1", 0.65), ("jaw", 0.35)], TOOTH)
    mesh, weights = cb.assemble("HoundArchonOriginal", parts)
    cb.require_single_body(mesh)
    names = {name for row in weights.values() for name, _ in row}
    if names != BONES:
        raise SystemExit("bone set differs from the reviewed contract: " +
                         json.dumps(sorted(names ^ BONES)))
    return mesh, weights, body, garment


def colours_for(body, garment, at):
    C = head_centre(at)

    def colours(p, n, region):
        out = np.zeros((len(p), 3))
        body_like = (region == FUR)
        cloth_like = (region == CLOTH)
        mat = np.full(len(p), -1)
        mat[body_like] = body.material_at(p[body_like], n[body_like])
        mat[cloth_like] = garment.material_at(p[cloth_like], n[cloth_like])
        mat[~(body_like | cloth_like)] = region[~(body_like | cloth_like)]
        ao = np.ones(len(p))
        ao[body_like] = body.cavity(p[body_like], n[body_like])
        ao[cloth_like] = garment.cavity(p[cloth_like], n[cloth_like])
        broad = cb.fbm(p, 5.0, 321)
        fine = cb.fbm(p, 60.0, 322)
        # Fur: warm brown, brindle streaks running along the body, darker
        # back and lighter chest; mastiff mask darkens toward the muzzle.
        # Brindle: fine dark streaks running down the body, not blotches.
        streak = sd.fbm(p * np.array((1.0, 1.0, 0.16)), 26.0, 401)
        brindle = sd.smoothstep(0.05, 0.30, streak)
        fur = np.array([.47, .28, .14]) * (0.90 + 0.12 * broad[:, None])
        fur = fur * (1.0 - 0.30 * brindle[:, None])
        back = np.clip(-n[:, 1], 0, 1) * 0.10 + np.clip(n[:, 1], 0, 1) * (p[:, 2] < 1.55) * -0.06
        fur = fur * (1.0 - back[:, None])
        fur = fur * (0.92 + 0.10 * fine[:, None])
        fur = fur * (0.90 + 0.12 * np.clip(n[:, 2], 0, 1)[:, None])
        head_dark = sd.smoothstep(0.06, 0.20, (p - C) @ FRONT) * (np.linalg.norm(p - C, axis=1) < 0.32)
        fur = fur * (1.0 - 0.45 * head_dark[:, None])
        palette = {
            MUZZLE: np.array([.13, .095, .075]), NOSE: np.array([.045, .038, .035]),
            EYE: np.array([.36, .19, .07]), PUPIL: np.array([.02, .02, .02]),
            MOUTH: np.array([.23, .09, .08]), CLAW: np.array([.07, .055, .045]),
            TOOTH: np.array([.80, .75, .62]), CLOTH: np.array([.83, .79, .68]),
            TRIM: np.array([.66, .49, .19]), LEATHER: np.array([.21, .135, .08]),
            BRONZE: np.array([.62, .44, .19]), EAR: np.array([.22, .13, .075]),
            MANTLE: np.array([.88, .85, .76]),
        }
        out[:] = fur
        for key, colour in palette.items():
            sel = mat == key
            if not sel.any():
                continue
            tone = colour * (0.90 + 0.14 * broad[sel, None])
            if key in (CLOTH, MANTLE):
                weave = 0.97 + 0.03 * np.sin(p[sel, 2] * 900) * np.sin(p[sel, 0] * 900 + p[sel, 1] * 900)
                tone = tone * weave[:, None]
            if key in (BRONZE, TRIM):
                tone = tone * (0.85 + 0.30 * np.clip(n[sel, 2] * 0.5 + 0.5, 0, 1))[:, None]
            out[sel] = tone
        eye_sel = mat == EYE
        if eye_sel.any():
            for s in (-1, 1):
                centre = C + (s * 0.060, 0.124, 0.011)
                pupil = np.linalg.norm(p[eye_sel] - centre, axis=1) < 0.0075
                sub = out[eye_sel]
                sub[pupil] = (0.02, 0.02, 0.02)
                out[eye_sel] = sub
        shade = 0.55 + 0.45 * ao
        return out * shade[:, None]
    return colours


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out", "preview"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = donor.measured_rig(args.capture)
    armature = cb.review_armature(rig_data, "HoundArchonReviewRig")
    mesh, weights, body, garment = build(rig, armature)
    cb.unwrap(mesh)
    cb.bake_albedo(mesh, 1024, colours_for(body, garment, rig_points(rig)), args.albedo)
    cb.attach_albedo_preview(mesh, args.albedo)
    extra = {"creature": "hound-archon", "donorFamily": "lizardfolk-free-hands",
             "nativeDonor": donor.DONOR_GUID, "nativePrefab": donor.PREFAB_GUID,
             "bodyRenderer": donor.BODY_RENDERER, "accessoryRenderer": donor.ACCESSORY_RENDERER,
             "nativeRendererCount": 2, "visibleArms": 2, "visibleLegs": 2,
             "visibleTail": False, "canineHead": True, "humanoidBody": True, "jawJoints": 2}
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
    scene.world = scene.world or bpy.data.worlds.new("HoundArchonPreviewWorld")
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
    print('[hound-archon] ' + json.dumps(report, sort_keys=True))


if __name__ == '__main__':
    main()
