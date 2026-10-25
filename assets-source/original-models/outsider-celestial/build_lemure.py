"""Original Lemure body, bound to the measured Lizardfolk Free Hands rig.

The October 25 refinement replaces the first upright column and its evenly
spaced base lobes with a signed-distance sculpt (`sdf_sculpt.py`): a slumped
mound of flesh pooled into a few large, asymmetric sagging masses with real
creases between them; a suffering face embedded in the top of the mass under
a hood of flesh, with a heavy brow, recessed eyes, a sagging nose and an open
mouth whose lower lip drips toward the chest; and two heavy arms with thick
wrists, broad palms and continuous finger roots ending in black claws. Only
geometry-free bone positions are read from the private census; no native
mesh, texture or animation is copied.
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
    side + name for side in ("L", "R") for name in ("_Arm_Upper", "_Arm_Lower", "_Palm")}
BONES = HEAT | {side + name for side in ("L", "R") for name in ("_finger1", "_finger2", "_Bfinger2")}

FLESH, MOUTH, EYE, CLAW, LIP, PALE = 0, 1, 2, 3, 4, 5
VOXEL = 0.0070
UP = np.array((0.0, 0.0, 1.0))
FRONT = np.array((0.0, 1.0, 0.0))


def unit(v):
    v = np.asarray(v, dtype=np.float64)
    return v / np.linalg.norm(v)


def rig_points(rig):
    return {name: np.array(rig[name]["head"], dtype=np.float64) for name in rig}


def face_centre(at):
    # The face sits in the front of the mass, below and ahead of the native
    # head control, so the whole hood turns with that control.
    return at["Head"] + np.array((0.0, 0.150, -0.110))


def hand_frame(at, side):
    wrist, f1, b2 = at[side + "_Palm"], at[side + "_finger1"], at[side + "_Bfinger2"]
    axis = unit(f1 - wrist)
    thumb = b2 - wrist
    width = unit(thumb - axis * float(thumb @ axis))
    normal = unit(np.cross(axis, width))
    return wrist, axis, width, normal


def sculpt_body(at):
    f = sd.Field((-0.95, -0.80, -0.02), (0.95, 0.80, 2.06), VOXEL)
    F = face_centre(at)
    # Pooled base: a broad slab with a few large asymmetric lobes; small blend
    # widths leave real creases where the masses fold over each other.
    f.ellipsoid((0.0, 0.020, 0.260), (0.640, 0.580, 0.260), material=FLESH)
    # A pear-shaped mound: each mass blends widely into the one below it.
    f.ellipsoid((0.0, 0.030, 0.550), (0.560, 0.500, 0.360), k=0.15, material=FLESH)
    # A low rim where the flesh pools on the ground.
    f.ellipsoid((0.0, 0.060, 0.070), (0.700, 0.640, 0.080), k=0.10, material=FLESH)
    for centre, radii, k in (
            ((-0.340, 0.250, 0.200), (0.320, 0.290, 0.200), 0.050),
            ((0.380, -0.060, 0.220), (0.300, 0.360, 0.215), 0.050),
            ((0.060, -0.390, 0.250), (0.380, 0.250, 0.240), 0.060),
            ((0.300, 0.360, 0.130), (0.260, 0.230, 0.130), 0.045)):
        f.ellipsoid(centre, radii, k=k, material=FLESH)
    # Slumped middle and hunched upper masses with sagging rolls in front.
    f.ellipsoid((0.0, 0.000, 0.880), (0.470, 0.400, 0.410), k=0.14, material=FLESH)
    f.ellipsoid((0.0, 0.230, 0.660), (0.380, 0.210, 0.190), k=0.030, material=FLESH)
    f.ellipsoid((-0.040, 0.210, 0.960), (0.330, 0.200, 0.170), k=0.030, material=FLESH)
    # One continuous hunched upper mass; the face is inset in its front.
    f.ellipsoid((0.0, -0.020, 1.300), (0.410, 0.350, 0.360), k=0.10, material=FLESH)
    f.ellipsoid((0.030, -0.160, 1.480), (0.330, 0.260, 0.290), k=0.10, material=FLESH)
    f.ellipsoid((0.0, 0.060, 1.600), (0.300, 0.290, 0.300), k=0.12, material=FLESH)
    f.ellipsoid(F + (0, -0.040, 0.150), (0.250, 0.220, 0.150), k=0.10, material=FLESH)
    # Sloping masses from the crown to the shoulders: one dome, no neck.
    for s in (-1, 1):
        f.ellipsoid((s * 0.230, -0.030, 1.570), (0.230, 0.260, 0.210), k=0.13, material=FLESH)
    f.ellipsoid((0.0, 0.180, 1.330), (0.330, 0.200, 0.190), k=0.06, material=FLESH)
    # Heavy brow and drooping cheeks blended into the mass.
    f.ellipsoid(F + (0, 0.120, 0.085), (0.155, 0.070, 0.050), k=0.05, material=FLESH)
    for s in (-1, 1):
        f.ellipsoid(F + (s * 0.105, 0.110, -0.060), (0.080, 0.070, 0.095), k=0.06, material=FLESH)
        f.ellipsoid(F + (s * 0.120, 0.080, -0.185), (0.090, 0.075, 0.105), k=0.06, material=FLESH)
    # Sagging nose and an open, anguished mouth with a drooping lower lip.
    f.ellipsoid(F + (0, 0.185, 0.000), (0.038, 0.050, 0.060), k=0.025, material=FLESH)
    f.ellipsoid(F + (0, 0.180, -0.115), (0.125, 0.065, 0.105), k=0.03, material=LIP)
    f.ellipsoid(F + (0, 0.210, -0.120), (0.072, 0.095, 0.086), op="cut", k=0.02, material=MOUTH)
    f.ellipsoid(F + (0, 0.168, -0.240), (0.095, 0.065, 0.058), k=0.03, material=LIP)
    # Recessed eyes: a deep left socket and a half-shut, sagging right one.
    f.sphere(F + (-0.078, 0.150, 0.048), 0.046, op="cut", k=0.015, material=FLESH)
    f.sphere(F + (0.078, 0.148, 0.035), 0.034, op="cut", k=0.015, material=FLESH)
    f.ellipsoid(F + (0.078, 0.168, 0.055), (0.045, 0.025, 0.020), k=0.012, material=FLESH)

    # A few large, tilted sagging creases rather than even rings: each is a
    # warped band around the body, deepest in front and fading at the back.
    creases = ((0.42, 0.10, -0.06, 0.020, 0.018), (0.58, -0.08, 0.05, 0.016, 0.020),
               (0.78, 0.12, 0.08, 0.014, 0.016), (1.02, -0.10, -0.04, 0.012, 0.018),
               (1.15, 0.06, 0.10, 0.010, 0.014), (0.27, -0.05, 0.03, 0.016, 0.022))

    def flesh(P):
        warp = sd.fbm(P, 2.2, 701) * 0.10
        total = 0.0
        for z0, tilt_x, tilt_y, depth, width in creases:
            height = z0 + tilt_x * P[..., 0] + tilt_y * P[..., 1] + warp
            band = np.exp(-((P[..., 2] - height) / width) ** 2)
            front = 0.35 + 0.65 * sd.smoothstep(-0.25, 0.20, P[..., 1])
            total = total + depth * band * front
        lumps = sd.fbm(P, 5.0, 702)
        return total + 0.005 * lumps
    f.displace(flesh, (-0.95, -0.80, -0.02), (0.95, 0.80, 1.80))

    def drips(P):
        # Flesh running down from the lower lip and chin.
        local = P - (F + np.array((0.0, 0.17, -0.18)))
        lines = np.cos(local[..., 0] * 2 * np.pi / 0.045) * sd.smoothstep(0.10, 0.02, np.abs(local[..., 0]))
        fall = sd.smoothstep(0.02, -0.06, local[..., 2]) * sd.smoothstep(-0.30, -0.12, local[..., 2])
        return -0.010 * np.clip(lines, 0, 1) * fall
    f.displace(drips, F + (-0.15, 0.0, -0.55), F + (0.15, 0.35, -0.05))

    for side, s in (("L", 1), ("R", -1)):
        S, E, W = at[side + "_Arm_Upper"], at[side + "_Arm_Lower"], at[side + "_Palm"]
        fore = unit(W - E)
        f.ellipsoid(S + (s * 0.050, 0.020, -0.010), (0.170, 0.160, 0.160), k=0.09, material=FLESH)
        f.cone(S + (s * 0.03, 0, 0), E, 0.158, 0.122, k=0.06, material=FLESH)
        f.ellipsoid(E + (W - E) * 0.25, (0.122, 0.110, 0.130), axes=sd.frame(fore), k=0.05, material=FLESH)
        f.cone(E, W, 0.122, 0.094, k=0.05, material=FLESH)
        # Fat wrist folds.
        f.cone(W - fore * 0.040, W - fore * 0.010, 0.104, 0.102, k=0.015, material=FLESH)
        wrist, axis, width, normal = hand_frame(at, side)
        f1, f2, b2 = at[side + "_finger1"], at[side + "_finger2"], at[side + "_Bfinger2"]
        palm_axes = np.stack([width, normal, axis])
        f.box(wrist + axis * 0.072, (0.096, 0.050, 0.092), axes=palm_axes, round_=0.038, k=0.038, material=FLESH)
        tip_dir = unit(f2 - f1)
        for offset in (-0.040, 0.030):
            base = wrist + axis * 0.120 + width * offset
            mid = f1 + width * offset * 1.10
            tip = f2 + width * offset * 1.15 + tip_dir * 0.020
            f.chain([base, mid, tip], [0.045, 0.038, 0.031], k=0.020, material=FLESH)
        thumb_base = wrist + axis * 0.040 + width * 0.060
        f.chain([thumb_base, b2 + unit(b2 - thumb_base) * 0.015], [0.044, 0.030], k=0.020, material=FLESH)
    return f


def weights_for_body(body, armature, at):
    points = [v.co.copy() for v in body.data.vertices]
    rows = cb.heat_weights(body, armature, HEAT)
    rows = cb.clean_weights(cb.smooth_weights(body, rows), points, armature, HEAT)
    rows = {index: dict(row) for index, row in rows.items()}
    P = np.array([tuple(p) for p in points])
    F = face_centre(at)
    arm_chains = [[at[side + n] for n in ("_Arm_Upper", "_Arm_Lower", "_Palm", "_finger1", "_finger2")]
                  for side in ("L", "R")]

    def arm_gap(p):
        best = 9.0
        for chain in arm_chains:
            for a, b in zip(chain, chain[1:]):
                t = np.clip(float((p - a) @ (b - a)) / float((b - a) @ (b - a)), 0.0, 1.0)
                best = min(best, float(np.linalg.norm(p - (a + (b - a) * t))))
        return best
    for index, p in enumerate(P):
        row = rows[index]
        # The pooled base cannot split into the donor's two legs; hands that
        # hang low still follow their arms.
        low = sd.smoothstep(0.70, 0.55, p[2]) * sd.smoothstep(0.16, 0.24, arm_gap(p))
        if low > 0:
            row = {k: v * (1 - low) for k, v in row.items()}
            row["Torso_Lower"] = row.get("Torso_Lower", 0.0) + low
        # The hooded face turns with the native head control; the mass around
        # it blends through the neck controls.
        local = p - F
        face = sd.smoothstep(0.30, 0.18, np.linalg.norm(local * np.array((1.0, 1.4, 1.0)))) * \
            sd.smoothstep(-0.10, 0.05, local[1])
        if face > 0:
            row = {k: v * (1 - face) for k, v in row.items()}
            row["Head"] = row.get("Head", 0.0) + face
        rows[index] = row
    for side in ("L", "R"):
        wrist, axis, width, normal = hand_frame(at, side)
        f1, f2, b2 = at[side + "_finger1"], at[side + "_finger2"], at[side + "_Bfinger2"]
        t1 = float((f1 - wrist) @ axis)
        t2 = float((f2 - wrist) @ axis)
        thumb_base = wrist + axis * 0.040 + width * 0.060
        thumb_dir = unit(b2 - thumb_base)
        thumb_len = float(np.linalg.norm(b2 - thumb_base))
        for index, p in enumerate(P):
            rel = p - wrist
            along = float(rel @ axis)
            if along < 0.02 or np.linalg.norm(rel) > 0.42:
                continue
            tb = float((p - thumb_base) @ thumb_dir) / thumb_len
            thumb_gap = np.linalg.norm(np.cross(p - thumb_base, thumb_dir))
            if tb > 0.15 and thumb_gap < 0.060 and float(rel @ width) > 0.045:
                share = sd.smoothstep(0.25, 0.85, tb)
                row = {side + "_Palm": 1 - share, side + "_Bfinger2": share}
            elif along > 0.11 and abs(float(rel @ normal)) < 0.07:
                if along < t1:
                    share = sd.smoothstep(0.11, t1, along)
                    row = {side + "_Palm": 1 - share, side + "_finger1": share}
                else:
                    share = sd.smoothstep(t1 + 0.02, t2, along)
                    row = {side + "_finger1": 1 - share, side + "_finger2": share}
            else:
                continue
            rows[index] = {k: v for k, v in row.items() if v > 1e-4}
    out = {}
    for index, row in rows.items():
        items = sorted(((k, v) for k, v in dict(row).items() if v > 0), key=lambda e: (-e[1], e[0]))[:4]
        total = sum(v for _, v in items)
        out[index] = [(k, v / total) for k, v in items]
    return out


def build(rig, armature):
    at = rig_points(rig)
    body = sculpt_body(at)
    body_obj = body.polygonise("LemureBody", largest_only=True)
    cb.finish_surface(body_obj, 7600, smooth_iterations=2)
    body_rows = weights_for_body(body_obj, armature, at)
    parts = [(body_obj, body_rows, FLESH)]
    F = face_centre(at)
    X, Y, Z = (Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1)))

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), region))
    # One open, wet yellow eye; the other is a slit under a sagging lid.
    rigid(cb.blob("LemureEye", Vector(F + (-0.078, 0.152, 0.048)), (X, Y, Z), (0.036, 0.028, 0.031), 14, 9),
          [("Head", 1.0)], EYE)
    rigid(cb.blob("LemureEyeShut", Vector(F + (0.078, 0.150, 0.033)), (X, Y, Z), (0.026, 0.020, 0.012), 12, 8),
          [("Head", 1.0)], EYE)
    for side in ("L", "R"):
        wrist, axis, width, normal = hand_frame(at, side)
        f1, f2, b2 = at[side + "_finger1"], at[side + "_finger2"], at[side + "_Bfinger2"]
        tip_dir = unit(f2 - f1)
        for offset in (-0.040, 0.030):
            tip = f2 + width * offset * 1.15 + tip_dir * 0.020
            claw = unit(tip_dir * 0.8 - normal * 0.6)
            rigid(cb.horn("LemureClaw", Vector(tip - tip_dir * 0.022), Vector(tip + claw * 0.175),
                          0.034, 10, 5, curve=Vector(-normal * 0.035), hint=Vector(normal)),
                  [(side + "_finger2", 1.0)], CLAW)
        thumb_base = wrist + axis * 0.040 + width * 0.060
        tdir = unit(b2 - thumb_base)
        tip = b2 + tdir * 0.015
        claw = unit(tdir * 0.8 - normal * 0.6)
        rigid(cb.horn("LemureThumbClaw", Vector(tip - tdir * 0.020), Vector(tip + claw * 0.120),
                      0.028, 10, 5, curve=Vector(-normal * 0.025), hint=Vector(normal)),
              [(side + "_Bfinger2", 1.0)], CLAW)
    mesh, weights = cb.assemble("LemureOriginal", parts)
    cb.require_single_body(mesh)
    names = {name for row in weights.values() for name, _ in row}
    if names != BONES:
        raise SystemExit("bone set differs from the reviewed contract: " + json.dumps(sorted(names ^ BONES)))
    return mesh, weights, body


def colours_for(body, at):
    def colours(p, n, region):
        out = np.zeros((len(p), 3))
        flesh_like = region == FLESH
        mat = region.copy()
        mat[flesh_like] = body.material_at(p[flesh_like], n[flesh_like])
        ao = np.ones(len(p))
        ao[flesh_like] = body.cavity(p[flesh_like], n[flesh_like])
        broad = cb.fbm(p, 3.5, 301)
        fine = cb.fbm(p, 30.0, 302)
        blush = sd.smoothstep(0.15, 0.55, sd.fbm(p, 6.0, 303))
        # Desaturated pale flesh with restrained warm blush; deeper values in
        # the folds come from the field's own cavity term.
        flesh = np.array([.85, .72, .66]) * (0.90 + 0.12 * broad[:, None])
        flesh = flesh * (1 - 0.30 * blush[:, None]) + np.array([.86, .56, .54]) * 0.30 * blush[:, None]
        flesh = flesh * (0.95 + 0.07 * fine[:, None])
        palette = {MOUTH: np.array([.24, .07, .07]), EYE: np.array([.80, .70, .30]),
                   CLAW: np.array([.07, .06, .06]), LIP: np.array([.62, .38, .36])}
        out[:] = flesh
        for key, colour in palette.items():
            sel = mat == key
            if sel.any():
                out[sel] = colour * (0.92 + 0.10 * broad[sel, None])
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
    armature = cb.review_armature(rig_data, "LemureReviewRig")
    mesh, weights, body = build(rig, armature)
    cb.unwrap(mesh)
    cb.bake_albedo(mesh, 1024, colours_for(body, rig_points(rig)), args.albedo)
    cb.attach_albedo_preview(mesh, args.albedo)
    extra = {"creature": "lemure", "donorFamily": "lizardfolk-free-hands",
             "nativeDonor": donor.DONOR_GUID, "nativePrefab": donor.PREFAB_GUID,
             "bodyRenderer": donor.BODY_RENDERER,
             "accessoryRenderer": donor.ACCESSORY_RENDERER,
             "nativeRendererCount": 2, "visibleArms": 2, "visibleLegs": 0,
             "visibleTail": False, "amorphousLowerBody": True}
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
    scene.world = scene.world or bpy.data.worlds.new("LemurePreviewWorld")
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
    print('[lemure] ' + json.dumps(report, sort_keys=True))


if __name__ == '__main__':
    main()
