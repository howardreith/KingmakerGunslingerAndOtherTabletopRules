"""Build the refined Aurochs or Bison as one continuous skinned body.

blender -b --factory-startup --python-exit-code 1 --python build_cattle.py -- \
    --kind aurochs|bison --capture <private sprint11-horse-bind-rig.json> \
    --albedo assets/ungulates/<kind>-albedo.png \
    --mesh-data assets/ungulates/<kind>-mesh.json \
    --report <private report.json> --blend-out <private source.blend>

VIS-017 (Aurochs) and VIS-023 (Bison). The Horse capture is validated by the
ungulate generator's own loader. A horse carries a long neck and a high head
on long legs; cattle read as a deep barrel on shorter legs with a short,
thick neck and the head carried near the line of the back. The barrel
therefore hangs low enough to bury the upper legs, and the skull sits below
and ahead of the Horse's high head pivot. Horns, ears, eyes, cloven hooves
and the tail switch are closed rigid features rooted inside the surface.
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
from generate_ungulates import measured_rig  # noqa: E402

SPACE = "donor renderer local; +X right, +Y up, +Z forward"
SIDE = Vector((1.0, 0.0, 0.0))      # creature's right
UP = Vector((0.0, 1.0, 0.0))
FORWARD = Vector((0.0, 0.0, 1.0))
REGION = {"body": 0, "horn": 1, "ear": 2, "eye": 3, "hoof": 4, "switch": 5,
          "muzzle": 6}
BONES = ("Chest", "Head", "Jaw", "L_Arm_Lower", "L_Arm_Upper", "L_Fingers",
         "L_Foot0", "L_Leg0_Lower", "L_Leg0_Upper", "L_Palm", "L_Toes0",
         "LowerTorso", "Neck1", "Neck2", "R_Arm_Lower", "R_Arm_Upper",
         "R_Fingers", "R_Foot0", "R_Leg0_Lower", "R_Leg0_Upper", "R_Palm",
         "R_Toes0", "Tail1", "Tail2", "Tail3", "Tail4", "Tail5", "UpperTorso")
UPPER_LIMB = {"L_Arm_Upper", "R_Arm_Upper", "L_Leg0_Upper", "R_Leg0_Upper"}


def ramp(value, start, width):
    t = max(0.0, min(1.0, (value - start) / width))
    return t * t * (3.0 - 2.0 * t)


def build(rig, armature, kind, params):
    at = lambda name: Vector(rig[name]["head"])
    rump, middle, chest = at("LowerTorso"), at("UpperTorso"), at("Chest")
    ground = min(at(name).y for name in ("L_Toes0", "R_Toes0", "L_Fingers",
                                         "R_Fingers")) - 0.215
    tail = [at("Tail%d" % index) for index in range(1, 6)]
    # The torso masses meet over tenths of a metre, so they are united and
    # relaxed on their own coarse union; the limbs and head join it at limb
    # resolution.
    torso = cb.Volume(params["torso_resolution"])
    volume = torso

    def point(y, z, x=0.0):
        return Vector((x, ground + y, z))

    def mass(centre, radii, axes=(SIDE, UP, FORWARD)):
        volume.ellipsoid(centre, axes, radii)

    # A deep barrel from rump to brisket, hung low beneath the Horse's spine
    # so it buries the upper legs.
    for z, y, radii in params["trunk"]:
        mass(point(y, z), radii)
    for x in (-1.0, 1.0):
        # Haunches over the hip and stifle, so each hind leg grows out of
        # the body instead of meeting it at a cap.
        mass(point(params["haunch"][0], rump.z - 0.10,
                   x * params["haunch"][1]), params["haunch"][2])
        # Shoulders over the elbow.
        mass(point(params["shoulder"][0], chest.z + 0.28,
                   x * params["shoulder"][1]), params["shoulder"][2])
    # Species masses: the bison's high shoulder hump sloping back to small
    # hindquarters, and its cape of long hair over the neck and shoulders.
    # A first build scattered small clumps instead; they piled up as bubbles.
    for y, z, radii in params["masses"]:
        mass(point(y, z), radii)
    if params["dorsal"]:
        # A bison's back is one line from the rump up to the hump over the
        # forelegs and steeply down to the neck: a capsule chain along it.
        volume.sweep([point(y, z) for y, z, _ in params["dorsal"]],
                     [radius for _, _, radius in params["dorsal"]])
    # A short, deep neck carried forward to the poll.
    poll = point(*params["poll"])
    neck_root = point(params["neck_root"], chest.z + 0.30)
    volume.sweep([neck_root, neck_root.lerp(poll, 0.55), poll],
                 params["neck_radii"])
    if params["dewlap"]:
        mass(neck_root.lerp(poll, 0.45) - UP * 0.24 + FORWARD * 0.02,
             params["dewlap"])
    trunk = torso.polygonise(kind.title() + "Torso")
    # The chain is round in section; the torso is narrowed after the union
    # so it reads deeper than it is wide.
    for vertex in trunk.data.vertices:
        vertex.co.x *= params["torso_width"]
    cb.relax(trunk, params["torso_smooth"])
    volume = cb.Volume(params["resolution"])
    volume.shell(trunk)
    # The skull runs forward and down from the poll: a broad forehead
    # between the horns, a long face and a broad muzzle.
    axis = Vector(params["head_axis"]).normalized()
    face_up = axis.cross(SIDE).normalized()
    if face_up.dot(UP) < 0:
        face_up = -face_up
    head_axes = (SIDE, face_up, axis)
    length = params["head_length"]
    cranium = poll + axis * 0.10 * length / 0.60
    mid = poll + axis * 0.32 * length / 0.60
    muzzle = poll + axis * 0.54 * length / 0.60
    w = params["head_width"]
    volume.sweep([cranium, mid, muzzle], [0.15 * w, 0.125 * w, 0.115 * w])
    volume.ellipsoid(cranium + face_up * 0.04, head_axes,
                     (0.19 * w, 0.12 * w, 0.14 * w))
    volume.ellipsoid(cranium.lerp(mid, 0.6) - face_up * 0.06, head_axes,
                     (0.15 * w, 0.12 * w, 0.20 * w))
    volume.ellipsoid(muzzle + axis * 0.04, head_axes,
                     (0.13 * w, 0.11 * w, 0.09 * w))
    # Hair on the head (the bison's woolly forecap and beard), placed along
    # the head axis and across it from the poll.
    for along, across, radii in params["head_hair"]:
        volume.ellipsoid(poll + axis * along + face_up * across, head_axes,
                         radii)
    for prefix in ("L", "R"):
        sign = -1.0 if prefix == "L" else 1.0
        upper, elbow = at(prefix + "_Arm_Upper"), at(prefix + "_Arm_Lower")
        knee, fetlock = at(prefix + "_Palm"), at(prefix + "_Fingers")
        front = [point(params["front_top"], upper.z - 0.02, upper.x * 0.85),
                 elbow, knee, fetlock,
                 point(0.08, fetlock.z + 0.04, fetlock.x)]
        hip, stifle = at(prefix + "_Leg0_Upper"), at(prefix + "_Leg0_Lower")
        hock, hind_fetlock = at(prefix + "_Foot0"), at(prefix + "_Toes0")
        hind = [point(params["hind_top"], hip.z, hip.x * 0.85), stifle, hock,
                hind_fetlock, point(0.08, hind_fetlock.z + 0.04,
                                    hind_fetlock.x)]
        volume.sweep(front, params["front_radii"])
        volume.sweep(hind, params["hind_radii"])
        if params["chaps"]:
            # Long hair on the forelegs.
            mass(elbow.lerp(knee, 0.30) + SIDE * sign * 0.02, params["chaps"])
    # Only the tailhead is part of the body; the tail itself hangs (below).
    volume.sweep([point(params["tail_root"], rump.z - 0.38), tail[0]],
                 params["tail_radii"][:2])

    body = volume.polygonise(kind.title() + "Body")
    cb.finish_surface(body, params["triangles"],
                      smooth_iterations=params["smooth"])
    positions = [v.co.copy() for v in body.data.vertices]
    allowed = set(BONES)
    rows = cb.heat_weights(body, armature, allowed)
    rows = cb.clean_weights(rows, positions, armature, allowed)
    # The barrel buries the Horse's long upper limb bones; above the belly
    # line it follows the spine, so a leg swing does not drag the flank.
    line = ground + params["belly"]

    def damp(q):
        return 0.85 * ramp(q.y, line + 0.05, 0.45)

    for target, half in (("LowerTorso", lambda q: q.z < rump.z + 0.25),
                         ("UpperTorso", lambda q: rump.z + 0.25 <= q.z < chest.z),
                         ("Chest", lambda q: q.z >= chest.z)):
        rows = cb.blend_toward(
            rows, positions,
            lambda q, row, half=half: half(q) and any(
                name in UPPER_LIMB for name, _ in row),
            target, damp)
    rows = cb.smooth_weights(body, rows)

    # Head owns the skull outright, fading in along the head axis and out
    # across it; Jaw takes the chin below the line of the mouth, so a bite
    # opens it without moving the forehead.
    def reach(q):
        along = (q - poll).dot(axis)
        return along, ((q - poll) - axis * along).length

    def head_share(q):
        along, across = reach(q)
        return ramp(along, -0.22, 0.30) * (1.0 - ramp(across, 0.26 * w, 0.14))

    def jaw_share(q):
        along, across = reach(q)
        below = -(q - poll).dot(face_up)
        return ramp(along, 0.20 * length / 0.60, 0.12) * \
            ramp(below, 0.05 * w, 0.05) * (1.0 - ramp(across, 0.26 * w, 0.10))

    # The Horse's two neck controls bend a short, deep cattle neck; where
    # the body buries them the heat solve leaves Neck2 bare, so it takes a
    # smooth share of the neck between its root and the poll.
    neck_axis = (poll - neck_root).normalized()
    neck_length = (poll - neck_root).length

    def neck_share(q):
        along = (q - neck_root).dot(neck_axis) / neck_length
        across = ((q - neck_root) - neck_axis * along * neck_length).length
        fade = 1.0 - ramp(across, params["neck_radii"][0] * 0.9, 0.25)
        return 0.45 * ramp(along, 0.15, 0.35) * \
            (1.0 - ramp(along, 0.85, 0.30)) * fade

    rows = cb.blend_toward(rows, positions, lambda q, row: neck_share(q) > 0,
                           "Neck2", neck_share)
    rows = cb.blend_toward(rows, positions, lambda q, row: head_share(q) > 0,
                           "Head", head_share)
    rows = cb.blend_toward(rows, positions, lambda q, row: jaw_share(q) > 0,
                           "Jaw", jaw_share)
    rows = cb.smooth_weights(body, rows, iterations=2)
    rows = cb.clean_weights(rows, positions, armature, allowed)
    regions = {}
    muzzle_tip = muzzle + axis * 0.10
    for index, q in enumerate(positions):
        regions[index] = REGION["muzzle"] if \
            (q - muzzle_tip).length < 0.13 * w else REGION["body"]
    parts = [(body, rows, regions)]

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), REGION[region]))

    horns = []
    for sign in (-1.0, 1.0):
        out = SIDE * sign
        base = cranium + out * params["horn_base"][0] * w + \
            face_up * params["horn_base"][1] - axis * 0.03
        spec = params["horn"]
        tip = base + out * spec["out"] + FORWARD * spec["forward"] + \
            UP * spec["up"]
        curve = out * spec["curve"][0] + FORWARD * spec["curve"][1] + \
            UP * spec["curve"][2]
        rigid(cb.horn("Horn", base - out * 0.04, tip, spec["radius"],
                      segments=14, rings=10, curve=curve, hint=FORWARD),
              [("Head", 1.0)], "horn")
        horns.append((base - out * 0.04, tip, curve))
        # Ears stand out sideways below the horns.
        ear_dir = (out * 0.85 - UP * 0.35 + FORWARD * 0.10).normalized()
        ear_base = cranium + out * 0.15 * w - face_up * 0.02 + axis * 0.04
        rigid(cb.blob("Ear", ear_base + ear_dir * 0.09 * params["ear"],
                      (FORWARD, ear_dir, ear_dir.cross(FORWARD).normalized()),
                      (0.065 * params["ear"], 0.11 * params["ear"],
                       0.03 * params["ear"]), 14, 8),
              [("Head", 1.0)], "ear")
        eye = cranium.lerp(mid, 0.55) + out * 0.135 * w + face_up * 0.03
        rigid(cb.blob("Eye", eye, (SIDE, axis, face_up),
                      (0.028, 0.034, 0.028), 12, 8), [("Head", 1.0)], "eye")
    for prefix in ("L", "R"):
        for bone in (prefix + "_Fingers", prefix + "_Toes0"):
            foot = at(bone)
            # Two dark cloven toes under each foot, with a cleft between
            # them (touching toes share an equator vertex).
            for lateral in (-1.0, 1.0):
                toe = point(0.045, foot.z + 0.05,
                            foot.x + lateral * 0.048 * params["hoof"])
                rigid(cb.blob("Hoof", toe, (SIDE, UP, FORWARD),
                              (0.042 * params["hoof"], 0.05, 0.075 *
                               params["hoof"]), 12, 8),
                      [(bone, 1.0)], "hoof")
    # Cattle carry the tail hanging from the tailhead, not held out like the
    # Horse's (a first build left it sticking straight back). The strand is
    # rooted in the tailhead and weighted down the Horse's tail chain by
    # height, so a swish moves it from the root.
    top = tail[0] + UP * 0.02 - FORWARD * 0.04
    drop = params["tail_drop"]
    hang = [top, top - FORWARD * 0.10 - UP * 0.10,
            top - FORWARD * 0.15 - UP * drop * 0.45,
            top - FORWARD * 0.17 - UP * drop]
    strand = cb.strand("Tail", hang, params["tail_radii"][1:5], 10)
    chain = ("Tail1", "Tail2", "Tail3", "Tail4", "Tail5")
    tail_rows = {}
    for index, vertex in enumerate(strand.data.vertices):
        t = max(0.0, min(1.0, (top.y - vertex.co.y) / drop)) * 3.999
        low, share = int(t), t - int(t)
        row = [(chain[low], 1.0 - share), (chain[low + 1], share)]
        tail_rows[index] = sorted([entry for entry in row if entry[1] > 0.0],
                                  key=lambda entry: (-entry[1], entry[0]))
    parts.append((strand, tail_rows, REGION["body"]))
    rigid(cb.blob("Switch", hang[-1] - UP * 0.10, (SIDE, UP, FORWARD),
                  (0.055, 0.15, 0.055), 12, 8), [("Tail5", 1.0)], "switch")
    animal, weights = cb.assemble(kind.title(), parts)
    cb.require_single_body(animal, REGION["body"])
    return animal, weights, {
        "ground": ground, "rump": rump, "chest": chest, "poll": poll,
        "axis": axis, "face_up": face_up, "muzzle": muzzle_tip,
        "horns": horns, "width": w}


def colour_field(kind, landmarks):
    ground = landmarks["ground"]
    palette = PALETTES[kind]

    def colour(p, n, region):
        up = np.clip(n[:, 1] * 0.5 + 0.5, 0, 1)[:, None]
        z = p[:, 2]
        # Front-to-back blend (the bison's dark cape over a paler rear).
        rear = np.clip((landmarks["chest"].z - 0.10 - z) / 0.80, 0, 1)[:, None]
        coat = palette["front"] * (1 - rear) + palette["rear"] * rear
        base = palette["belly"] + (coat - palette["belly"]) * (0.45 + 0.55 * up)
        broad = cb.fbm(p, 1.0 / 0.35, 31)
        hair = cb.fbm(p * np.array([3.0, 0.7, 3.0]), 1.0 / 0.05, 32)
        base = base * (0.86 + 0.20 * broad[:, None]) * \
            (0.88 + 0.18 * hair[:, None])
        # A pale stripe down the spine (the aurochs' eel stripe).
        if palette["stripe"] is not None:
            spine = np.exp(-(p[:, 0] / 0.07) ** 2) * \
                np.clip((n[:, 1] - 0.55) / 0.3, 0, 1) * \
                np.clip((p[:, 1] - ground - 1.30) / 0.2, 0, 1)
            base = base * (1 - 0.6 * spine[:, None]) + \
                palette["stripe"] * 0.6 * spine[:, None]
        # Darker lower legs.
        height = p[:, 1] - ground
        legs = np.clip((0.75 - height) / 0.35, 0, 1)[:, None]
        base = base * (1 - legs) + palette["legs"] * legs * \
            (0.9 + 0.2 * broad[:, None])
        muzzle = palette["muzzle"] * (0.9 + 0.15 * broad[:, None])
        rgb = np.where((region == REGION["muzzle"])[:, None], muzzle, base)
        # Horns pale to dark at the tip (aurochs), or black throughout.
        reach = np.zeros(len(p))
        nearest = np.full(len(p), np.inf)
        for root, tip, _ in landmarks["horns"]:
            root, tip = np.array(root), np.array(tip)
            span = tip - root
            t = np.clip((p - root) @ span / (span @ span), 0, 1)
            gap = np.linalg.norm(p - (root + t[:, None] * span), axis=1)
            reach = np.where(gap < nearest, t, reach)
            nearest = np.minimum(gap, nearest)
        tipped = np.clip((reach - palette["horn_tip"]) / 0.15, 0, 1)[:, None]
        horn = palette["horn"] * (1 - tipped) + \
            np.array([0.06, 0.05, 0.045]) * tipped
        horn = horn * (0.9 + 0.15 * cb.fbm(p, 1.0 / 0.04, 33))[:, None]
        rgb = np.where((region == REGION["horn"])[:, None], horn, rgb)
        rgb = np.where((region == REGION["ear"])[:, None], base * 0.9, rgb)
        rgb = np.where((region == REGION["eye"])[:, None],
                       np.array([0.04, 0.03, 0.03]), rgb)
        rgb = np.where((region == REGION["hoof"])[:, None],
                       np.array([0.08, 0.07, 0.06]), rgb)
        rgb = np.where((region == REGION["switch"])[:, None],
                       palette["legs"] * 0.8, rgb)
        return rgb
    return colour


PALETTES = {
    "aurochs": {"front": np.array([0.26, 0.17, 0.11]),
                "rear": np.array([0.24, 0.16, 0.10]),
                "belly": np.array([0.17, 0.12, 0.08]),
                "stripe": np.array([0.58, 0.47, 0.32]),
                "legs": np.array([0.10, 0.08, 0.06]),
                "muzzle": np.array([0.70, 0.64, 0.55]),
                "horn": np.array([0.82, 0.77, 0.66]), "horn_tip": 0.72},
    "bison": {"front": np.array([0.19, 0.12, 0.07]),
              "rear": np.array([0.42, 0.30, 0.18]),
              "belly": np.array([0.15, 0.10, 0.06]),
              "stripe": None,
              "legs": np.array([0.11, 0.08, 0.05]),
              "muzzle": np.array([0.09, 0.07, 0.06]),
              "horn": np.array([0.10, 0.09, 0.08]), "horn_tip": 2.0},
}

# Proportions in donor metres, tuned against the measured Horse frame.
# Trunk rows are (z, height of centre above ground, (side, up, forward)).
PROPORTIONS = {
    "aurochs": {
        "resolution": 0.016,
        "trunk": ((-0.62, 1.38, (0.30, 0.34, 0.30)),
                  (-0.05, 1.27, (0.36, 0.43, 0.86)),
                  (0.46, 1.31, (0.34, 0.47, 0.36)),
                  (0.50, 1.56, (0.24, 0.26, 0.40)),
                  (0.70, 1.05, (0.20, 0.28, 0.18))),
        "haunch": (1.24, 0.16, (0.18, 0.36, 0.27)),
        "shoulder": (1.20, 0.17, (0.16, 0.32, 0.22)),
        "belly": 0.86,
        "poll": (1.80, 1.20),
        "neck_root": 1.42,
        "neck_radii": (0.36, 0.28, 0.19),
        "dewlap": (0.10, 0.18, 0.20),
        "head_axis": (0.0, -0.66, 0.75),
        "head_length": 0.62,
        "head_width": 1.0,
        "front_top": 1.24, "hind_top": 1.30,
        "front_radii": (0.18, 0.14, 0.08, 0.07, 0.068),
        "hind_radii": (0.21, 0.15, 0.085, 0.07, 0.068),
        "tail_root": 1.50,
        "tail_radii": (0.08, 0.06, 0.042, 0.032, 0.026),
        "tail_drop": 0.95,
        "horn_base": (0.15, 0.10),
        "horn": {"out": 0.46, "forward": 0.38, "up": 0.34, "radius": 0.085,
                 "curve": (0.10, -0.14, -0.02)},
        "ear": 1.0, "hoof": 1.0,
        "masses": (), "dorsal": (), "head_hair": (), "chaps": None,
        "smooth": 12, "torso_resolution": 0.03, "torso_smooth": 24,
        "torso_width": 1.0,
        "triangles": 6400,
    },
    "bison": {
        "resolution": 0.016,
        "trunk": ((-0.05, 1.18, (0.40, 0.40, 0.78)),
                  (0.74, 1.02, (0.30, 0.34, 0.24))),
        "haunch": (1.14, 0.14, (0.16, 0.31, 0.24)),
        "shoulder": (1.16, 0.20, (0.19, 0.36, 0.26)),
        "masses": (),
        "dorsal": ((1.30, -0.62, 0.28), (1.32, -0.22, 0.38),
                   (1.42, 0.18, 0.52), (1.48, 0.46, 0.56),
                   (1.40, 0.78, 0.46), (1.30, 1.00, 0.34)),
        "smooth": 12,
        "torso_resolution": 0.03, "torso_smooth": 30, "torso_width": 0.8,
        "belly": 0.80,
        "poll": (1.36, 1.22),
        "neck_root": 1.40,
        "neck_radii": (0.42, 0.36, 0.26),
        "dewlap": None,
        "head_axis": (0.0, -0.86, 0.51),
        "head_length": 0.48,
        "head_width": 1.25,
        "head_hair": ((0.02, 0.10, (0.27, 0.20, 0.20)),
                      (0.22, -0.20, (0.10, 0.17, 0.12))),
        "chaps": (0.15, 0.28, 0.16),
        "front_top": 1.16, "hind_top": 1.20,
        "front_radii": (0.19, 0.145, 0.095, 0.082, 0.078),
        "hind_radii": (0.20, 0.15, 0.095, 0.08, 0.075),
        "tail_root": 1.38,
        "tail_radii": (0.07, 0.05, 0.036, 0.028, 0.024),
        "tail_drop": 0.70,
        "horn_base": (0.19, 0.10),
        "horn": {"out": 0.13, "forward": 0.03, "up": 0.19, "radius": 0.06,
                 "curve": (0.05, -0.01, -0.02)},
        "ear": 0.8, "hoof": 1.1,
        "triangles": 7600,
    },
}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", choices=tuple(PROPORTIONS), required=True)
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = measured_rig(args.capture, args.kind)
    armature = cb.review_armature(rig_data, args.kind.title() + "RigPreview")
    animal, weights, landmarks = build(rig, armature, args.kind,
                                       PROPORTIONS[args.kind])
    cb.unwrap(animal)
    cb.bake_albedo(animal, 1024, colour_field(args.kind, landmarks),
                   args.albedo)
    cb.attach_albedo_preview(animal, args.albedo)
    summary = cb.write_mesh_data(args.mesh_data, animal, weights, rig_hash,
                                 args.albedo, SPACE)
    if tuple(summary["bones"]) != BONES:
        raise SystemExit(args.kind + " must bind exactly the reviewed 28 Horse "
                         "controls: " + json.dumps(summary["bones"]))
    report = {"schemaVersion": 1, "creature": args.kind,
              "builder": "build_cattle.py", "donorFamily": "horse",
              "rigSha256": rig_hash, **summary,
              "albedo": cb.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    groups = {}
    for index, row in weights.items():
        for name, value in row:
            group = groups.get(name) or animal.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    modifier = animal.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    animal.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[" + args.kind + "] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
