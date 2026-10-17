"""Build the refined Ankylosaurus as one continuous skinned body (VIS-028).

blender -b --factory-startup --python-exit-code 1 --python build_ankylosaurus.py -- \
    --capture <private sprint22-ankylosaurus-contracts.json> \
    --albedo assets/ankylosaurus/ankylosaurus-albedo.png \
    --mesh-data assets/ankylosaurus/ankylosaurus-mesh.json \
    --report <private report.json> --blend-out <private source.blend>

The Hodag survey is validated by the Ankylosaurus generator's own loader; the
body binds the same reviewed 24 drivers. The Hodag is a low, sprawling frame,
which suits an ankylosaur: a very broad, low torso relaxed as one surface,
short sprawled legs on the measured bones, a small beaked head with a rigid
lower jaw and horns at the back corners of the skull, and a tail tapering
down its six joints to a moderate two-lobed club. Rows of domed osteoderms
and lateral spikes are closed rigid features rooted in the back. The owner
found the first export's face confused, its club lobes oversized and
bulbous and its club faces bright cyan; the club is now bone-tan.
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
import generate_ankylosaurus as anky  # noqa: E402

SPACE = "donor renderer local; +X right, +Y up, +Z forward"
RIGHT = Vector((1.0, 0.0, 0.0))
UP = Vector((0.0, 1.0, 0.0))
FORWARD = Vector((0.0, 0.0, 1.0))
REGION = {"body": 0, "scute": 1, "spike": 2, "club": 3, "eye": 4, "beak": 5,
          "horn": 6}
TAIL = tuple("Tail%d_M" % index for index in range(6))


def ramp(value, start, width):
    t = max(0.0, min(1.0, (value - start) / width))
    return t * t * (3.0 - 2.0 * t)


def spine_bone(z):
    if z < 0.10:
        return "LowerTorso"
    return "Spine1_M" if z < 0.60 else "UpperTorso"


def build(rig, armature):
    at = lambda name: Vector(rig[name]["head"])
    head, jaw_pivot = at("Head"), at("Jaw_M")
    tail = [at(name) for name in TAIL]
    torso = cb.Volume(0.02)
    # A very broad, low torso: a capsule chain along the back, flattened
    # after the union so it is far wider than deep.
    torso.sweep([Vector((0, 0.60, -0.62)), Vector((0, 0.64, -0.30)),
                 Vector((0, 0.66, 0.10)), Vector((0, 0.66, 0.50)),
                 Vector((0, 0.64, 0.86)), Vector((0, 0.60, 1.08))],
                [0.20, 0.30, 0.34, 0.34, 0.28, 0.18])
    torso.ellipsoid(Vector((0, 0.50, 0.20)), (RIGHT, UP, FORWARD),
                    (0.30, 0.20, 0.70))
    trunk = torso.polygonise("AnkylosaurusTorso")
    for vertex in trunk.data.vertices:
        vertex.co.x *= 2.1
        vertex.co.y = 0.60 + (vertex.co.y - 0.60) * 0.95
    cb.relax(trunk, 24)
    volume = cb.Volume(0.009)
    volume.shell(trunk)
    # A short neck and a small, broad head with a narrow beak.
    volume.sweep([Vector((0, 0.60, 1.00)), at("Neck_M"), head + UP * 0.02],
                 [0.20, 0.17, 0.15])
    volume.ellipsoid(head + FORWARD * 0.04 + UP * 0.03, (RIGHT, UP, FORWARD),
                     (0.21, 0.13, 0.20))
    volume.sweep([head + FORWARD * 0.10, head + FORWARD * 0.30 - UP * 0.04],
                 [0.14, 0.085])
    # Short, sprawled legs on the measured bones, with broad feet.
    for side in ("Left", "Right"):
        sign = -1.0 if side == "Left" else 1.0
        shoulder, elbow = at(side + "UpperArm"), at(side + "LowerArm")
        hand = at(side + "Hand")
        volume.sweep([Vector((sign * 0.22, 0.62, shoulder.z)), shoulder, elbow,
                      hand, Vector((hand.x, 0.06, hand.z + 0.06))],
                     [0.17, 0.14, 0.11, 0.085, 0.08])
        knee = at(side + "LowerLeg0")
        foot = at(side + "Foot0")
        # The Hodag's upper-leg joint sits below its knee; the leg runs from
        # the body straight to the knee so it does not hang a ball under the
        # belly (a first build did).
        volume.sweep([Vector((sign * 0.30, 0.60, -0.26)), knee, foot,
                      Vector((foot.x, 0.06, foot.z + 0.05))],
                     [0.19, 0.13, 0.095, 0.09])
        for paw in (hand, foot):
            volume.ellipsoid(Vector((paw.x, 0.05, paw.z + 0.07)),
                             (RIGHT, UP, FORWARD), (0.11, 0.05, 0.13))
    # The tail tapers down its six joints toward the club.
    volume.sweep([Vector((0, 0.60, -0.55))] + tail,
                 [0.22, 0.20, 0.15, 0.11, 0.085, 0.07, 0.06])
    body = volume.polygonise("AnkylosaurusBody")
    cb.finish_surface(body, 7600, smooth_iterations=12)
    positions = [v.co.copy() for v in body.data.vertices]
    allowed = set(anky.DRIVERS)
    rows = cb.heat_weights(body, armature, allowed)
    rows = cb.clean_weights(rows, positions, armature, allowed)
    rows = cb.smooth_weights(body, rows)

    def head_share(q):
        return ramp(q.z, head.z - 0.22, 0.14) * ramp(q.y, 0.30, 0.10)

    rows = cb.blend_toward(rows, positions, lambda q, row: head_share(q) > 0,
                           "Head", head_share)
    rows = cb.smooth_weights(body, rows, iterations=2)
    rows = cb.clean_weights(rows, positions, armature, allowed)
    regions = {index: REGION["beak"] if (q - (head + FORWARD * 0.30 -
                                             UP * 0.04)).length < 0.10
               else REGION["body"] for index, q in enumerate(positions)}
    parts = [(body, rows, regions)]

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), REGION[region]))

    # The lower jaw, a narrow beaked piece on Jaw_M.
    rigid(cb.blob("AnkylosaurusJaw", jaw_pivot + FORWARD * 0.14 - UP * 0.08,
                  (RIGHT, UP, FORWARD), (0.13, 0.05, 0.17), 14, 8),
          [("Jaw_M", 1.0)], "beak")
    for sign in (-1.0, 1.0):
        # Short horns at the back corners of the skull, and eyes.
        corner = head + RIGHT * sign * 0.18 + UP * 0.07 - FORWARD * 0.06
        rigid(cb.horn("AnkylosaurusHeadHorn", corner, corner + RIGHT * sign *
                      0.10 - FORWARD * 0.06 + UP * 0.02, 0.045, 8, 4),
              [("Head", 1.0)], "horn")
        eye = head + RIGHT * sign * 0.19 + UP * 0.06 + FORWARD * 0.10
        rigid(cb.blob("AnkylosaurusEye", eye, (RIGHT, UP, FORWARD),
                      (0.025, 0.022, 0.025), 10, 6), [("Head", 1.0)], "eye")
    # Rows of domed osteoderms across the back and lateral spikes along the
    # flanks, each on the spine control under it.
    for z in (-0.52, -0.30, -0.08, 0.14, 0.36, 0.58, 0.78, 0.96):
        bone = spine_bone(z)
        width = 0.68 if z < 0.86 else 0.50
        for column in (-2, -1, 0, 1, 2):
            x = column * width * 0.36
            lift = math.sqrt(max(0.04, 1.0 - (x / (width + 0.05)) ** 2))
            dome = Vector((x, 0.63 + 0.31 * lift, z))
            rigid(cb.blob("AnkylosaurusScute", dome, (RIGHT, UP, FORWARD),
                          (0.080, 0.050, 0.085), 10, 6),
                  [(bone, 1.0)], "scute")
        for sign in (-1.0, 1.0):
            base = Vector((sign * (width + 0.02), 0.56, z))
            rigid(cb.horn("AnkylosaurusSpike", base - RIGHT * sign * 0.05,
                          base + RIGHT * sign * 0.13 + UP * 0.03, 0.040, 8, 4),
                  [(bone, 1.0)], "spike")
    # Smaller osteoderm rings down the tail.
    for index, name in enumerate(TAIL[:5]):
        point = tail[index]
        for sign in (-1.0, 1.0):
            base = point + RIGHT * sign * (0.16 - 0.02 * index) + UP * 0.02
            rigid(cb.horn("AnkylosaurusTailSpike", base, base + RIGHT * sign *
                          0.07, 0.030, 8, 4), [(name, 1.0)], "spike")
    # The club: two moderate bony lobes either side of the tail tip.
    club = tail[5] - FORWARD * 0.10
    for sign in (-1.0, 1.0):
        rigid(cb.blob("AnkylosaurusClubLobe", club + RIGHT * sign * 0.10,
                      (RIGHT, UP, FORWARD), (0.13, 0.08, 0.16), 16, 10),
              [("Tail5_M", 1.0)], "club")
    rigid(cb.blob("AnkylosaurusClubCore", club, (RIGHT, UP, FORWARD),
                  (0.10, 0.07, 0.18), 14, 8), [("Tail5_M", 1.0)], "club")
    animal, weights = cb.assemble("Ankylosaurus", parts)
    cb.require_single_body(animal, REGION["body"])
    return animal, weights


def colour_field():
    def colour(p, n, region):
        up = np.clip(n[:, 1] * 0.5 + 0.5, 0, 1)[:, None]
        back = np.array([0.30, 0.26, 0.18])
        belly = np.array([0.46, 0.40, 0.30])
        base = belly + (back - belly) * np.clip(up * 1.3 - 0.15, 0, 1)
        broad = cb.fbm(p, 1.0 / 0.25, 81)
        scale, second = cb.cells(p, 1.0 / 0.05, 82)
        rim = np.clip((second - scale) * 3.0, 0, 1)
        base = base * (0.86 + 0.20 * broad[:, None]) * \
            (0.82 + 0.18 * rim[:, None])
        bone = np.array([0.60, 0.53, 0.40]) * (0.85 + 0.25 * broad[:, None])
        rgb = np.where((region == REGION["scute"])[:, None], bone * 0.85, base)
        rgb = np.where((region == REGION["spike"])[:, None], bone, rgb)
        rgb = np.where((region == REGION["horn"])[:, None], bone, rgb)
        rgb = np.where((region == REGION["club"])[:, None], bone * 0.92, rgb)
        rgb = np.where((region == REGION["beak"])[:, None],
                       np.array([0.18, 0.16, 0.13]), rgb)
        rgb = np.where((region == REGION["eye"])[:, None],
                       np.array([0.05, 0.04, 0.03]), rgb)
        return rgb
    return colour


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = anky.measured_rig(args.capture)
    armature = cb.review_armature(rig_data, "AnkylosaurusRigPreview")
    animal, weights = build(rig, armature)
    cb.unwrap(animal)
    cb.bake_albedo(animal, 1024, colour_field(), args.albedo)
    cb.attach_albedo_preview(animal, args.albedo)
    extra = {"creature": "ankylosaurus", "nativeDonor": anky.DONOR,
             "visibleLegs": 4, "tailJoints": 6, "terminalClub": True}
    summary = cb.write_mesh_data(args.mesh_data, animal, weights, rig_hash,
                                 args.albedo, SPACE, extra)
    if sorted(summary["bones"]) != sorted(anky.DRIVERS):
        raise SystemExit("Ankylosaurus must bind exactly the reviewed 24 "
                         "drivers: " + json.dumps(summary["bones"]))
    report = {"schemaVersion": 1, "builder": "build_ankylosaurus.py",
              "donorFamily": "hodag", "rigSha256": rig_hash, **extra,
              **summary, "albedo": cb.albedo_manifest(args.albedo)}
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
    print("[ankylosaurus] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
