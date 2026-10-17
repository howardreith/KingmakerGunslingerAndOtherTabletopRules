"""Build the refined Giant Wasp as one continuous skinned body (VIS-022).

blender -b --factory-startup --python-exit-code 1 --python build_giant_wasp.py -- \
    --rig <private rig.measured.json> \
    --albedo assets/flying-animals/giant-wasp-albedo.png \
    --mesh-data assets/flying-animals/giant-wasp-mesh.json \
    --report <private report.json> --blend-out <private source.blend>

The rig is the Giant Eagle bind frame (`../pteranodon/convert_bind_rig.py`;
+X left, +Y up, -Z forward). The owner found the first wasp's major pieces
and appendages failed to form a coherent insect, with the abdomen raised
high behind it, and it shipped inside out. This one is one connected body:
a head with large compound eyes, antennae and mandibles on a short neck, a
deep thorax, a narrow waist and a banded gaster carried behind and a little
below the thorax, ending in the sting. The gaster and sting ride `Tail`
alone, so the runtime's bounded sting pose (GiantWaspVisualSting) still
curls them under. Six legs leave the underside of the thorax on the eagle's
leg chain, and two pairs of narrow wings ride the arm chain.
"""
import argparse
import hashlib
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
sys.path.insert(0, str(HERE.parent / "pteranodon"))
import continuous_body as cb  # noqa: E402
import generate_pteranodon as shared  # noqa: E402

SPACE = "donor renderer local; +X left, +Y up, -Z forward"
SIDE = Vector((1.0, 0.0, 0.0))      # the creature's left
UP = Vector((0.0, 1.0, 0.0))
FORWARD = Vector((0.0, 0.0, -1.0))
REGION = {"body": 0, "wing": 1, "eye": 2, "sting": 3, "leg": 4,
          "antenna": 5}
HEAT = ("LowerTorso", "UpperTorso", "Neck", "Head", "Tail")
# The waist: everything behind it is the gaster and rides Tail alone.
WAIST_Z = 0.36


def build(rig, armature):
    at = lambda name: Vector(rig[name]["head"])
    volume = cb.Volume(0.014)
    axes = (SIDE, UP, FORWARD)
    head = at("Head")
    # The head capsule on a short neck.
    capsule = head + Vector((0, -0.04, -0.02))
    volume.ellipsoid(capsule, axes, (0.24, 0.23, 0.19))
    volume.sweep([capsule + Vector((0, -0.02, 0.14)), Vector((0, 1.78, -0.78)),
                  Vector((0, 1.72, -0.66))], [0.10, 0.085, 0.10])
    # The thorax: deep, humped, tapering into the waist.
    volume.ellipsoid(Vector((0, 1.58, -0.30)), axes, (0.30, 0.31, 0.42))
    volume.sweep([Vector((0, 1.68, -0.62)), Vector((0, 1.62, -0.30)),
                  Vector((0, 1.44, 0.06)), Vector((0, 1.30, 0.26))],
                 [0.20, 0.31, 0.24, 0.12])
    # The waist and the gaster behind it, a little below the thorax line.
    volume.sweep([Vector((0, 1.28, 0.24)), Vector((0, 1.24, 0.36)),
                  Vector((0, 1.22, 0.44))], [0.085, 0.06, 0.08])
    volume.sweep([Vector((0, 1.22, 0.46)), Vector((0, 1.17, 0.70)),
                  Vector((0, 1.10, 1.00)), Vector((0, 1.01, 1.30)),
                  Vector((0, 0.93, 1.52)), Vector((0, 0.88, 1.64))],
                 [0.12, 0.30, 0.34, 0.27, 0.15, 0.05])

    body = volume.polygonise("GiantWaspBody")
    cb.finish_surface(body, 6500, smooth_iterations=8)
    positions = [v.co.copy() for v in body.data.vertices]
    heat = cb.heat_weights(body, armature, set(HEAT))
    heat = cb.clean_weights(heat, positions, armature, set(HEAT))
    rows = {}
    for index, q in enumerate(positions):
        if q.z > WAIST_Z:
            # The gaster rides Tail alone, so the sting pose carries it.
            rows[index] = [("Tail", 1.0)]
        elif q.z < head.z + 0.24:
            rows[index] = [("Head", 1.0)]
        elif q.z > WAIST_Z - 0.10:
            # Into the waist: the thorax hands over to Tail.
            t = (q.z - (WAIST_Z - 0.10)) / 0.10
            rows[index] = [("LowerTorso", 1.0 - t), ("Tail", t)]
        else:
            rows[index] = [(n, v) for n, v in heat[index] if n != "Tail"] or \
                [("UpperTorso", 1.0)]
    rows = cb.clean_weights(rows, positions, armature, set(HEAT))
    parts = [(body, rows, REGION["body"])]

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), REGION[region]))

    # Compound eyes, antennae, mandibles; the sting.
    for sign in (1.0, -1.0):
        rigid(cb.blob("GiantWaspEye", capsule + SIDE * (sign * 0.17) +
                      UP * 0.04 + FORWARD * 0.04, (SIDE, UP, FORWARD),
                      (0.10, 0.15, 0.11), 16, 10), [("Head", 1.0)], "eye")
        root = capsule + SIDE * (sign * 0.07) + UP * 0.17 + FORWARD * 0.12
        antenna = [root, root + UP * 0.16 + FORWARD * 0.10 + SIDE * (sign * 0.05),
                   root + UP * 0.24 + FORWARD * 0.30 + SIDE * (sign * 0.12),
                   root + UP * 0.20 + FORWARD * 0.52 + SIDE * (sign * 0.18)]
        rigid(cb.tube("GiantWaspAntenna", antenna, [0.022, 0.018, 0.015, 0.01], 7),
              [("Head", 1.0)], "antenna")
        base = capsule + SIDE * (sign * 0.08) + UP * -0.14 + FORWARD * 0.14
        rigid(cb.horn("GiantWaspMandible", base,
                      base + FORWARD * 0.12 + UP * -0.05 - SIDE * (sign * 0.07),
                      0.04, 8, 5, curve=SIDE * (sign * 0.02), hint=UP),
              [("Head", 1.0)], "sting")
    rigid(cb.horn("GiantWaspSting", Vector((0, 0.90, 1.58)),
                  Vector((0, 0.80, 1.86)), 0.045, 10, 6,
                  curve=Vector((0, -0.03, 0.0)), hint=SIDE),
          [("Tail", 1.0)], "sting")

    for prefix, sign in (("L_", 1.0), ("R_", -1.0)):
        upper, lower, foot = (prefix + n for n in
                              ("Leg0_Upper", "Leg0_Lower", "Foot0"))
        # Six legs from the underside of the thorax, fore, mid and hind,
        # all on the eagle's single leg chain.
        for along, splay in ((-0.48, -0.30), (-0.22, 0.0), (0.04, 0.32)):
            root = Vector((sign * 0.10, 1.36, along))
            knee = Vector((sign * 0.48, 1.32, along + splay * 0.6))
            ankle = Vector((sign * 0.66, 0.38, along + splay))
            toe = Vector((sign * 0.72, 0.03, along + splay * 1.2))
            points = [root, knee, ankle, toe]
            obj = cb.tube("GiantWaspLeg", points, [0.055, 0.042, 0.03, 0.014], 8)
            parts.append((obj, cb.chain_rows(obj, points, [upper, lower, foot]),
                          REGION["leg"]))
        # Two pairs of narrow wings on the arm chain, shorter than the
        # eagle's spread: the forewing to about two thirds of the arm.
        shoulder = at(prefix + "Arm_Upper")
        elbow = at(prefix + "Arm_Lower")
        palm = at(prefix + "Palm")
        for rear, reach, chord, back in ((False, 0.70, 0.36, 0.0),
                                         (True, 0.52, 0.26, 0.30)):
            root = shoulder + FORWARD * (-back) + UP * -0.04
            tip = root + (palm.lerp(elbow, 0.25) - shoulder) * reach
            stations = 14
            columns = []
            span_axis = (tip - root).normalized()
            trail_axis = (-FORWARD - span_axis * (-FORWARD).dot(span_axis)).normalized()
            for i in range(stations):
                s = i / float(stations - 1)
                lead = root.lerp(tip, s)
                width = chord * (0.35 + 0.65 * math.sin(math.pi * min(1.0, s * 1.15)))
                width = max(width, 0.04)
                columns.append([lead + trail_axis * (width * j / 6.0)
                                for j in range(7)])
            obj, origin = cb.sheet("GiantWaspWing", columns, 0.008)
            bones = [prefix + "Arm_Upper", prefix + "Arm_Lower",
                     prefix + "Palm"]
            wing_rows = {}
            for index in range(len(obj.data.vertices)):
                i, _ = origin[index]
                s = i / float(stations - 1)
                if s < 0.5:
                    wing_rows[index] = [(bones[0], 1.0 - s), (bones[1], s)]
                else:
                    wing_rows[index] = [(bones[1], 1.5 - s), (bones[2], s - 0.5)]
            parts.append((obj, wing_rows, REGION["wing"]))
    wasp, weights = cb.assemble("GiantWasp", parts)
    cb.require_single_body(wasp, REGION["body"])
    return wasp, weights


def colour_field():
    def colour(p, n, region):
        black = np.array([0.08, 0.07, 0.06])
        yellow = np.array([0.78, 0.56, 0.10])
        z = p[:, 2]
        # The gaster: yellow and black bands, black at the waist.
        band = 0.5 + 0.5 * np.cos((z - 0.62) * math.pi / 0.17)
        gaster = np.where((band > 0.55)[:, None], yellow, black)
        # The thorax: black with two yellow marks; the face yellow.
        mark = np.exp(-((z + 0.05) / 0.06) ** 2) * (p[:, 1] > 1.62)
        thorax = black + (yellow - black) * mark[:, None]
        face = (z < -1.10) & (p[:, 1] < 1.95)
        rgb = np.where((z > WAIST_Z)[:, None], gaster, thorax)
        rgb = np.where(face[:, None], yellow, rgb)
        fine = cb.fbm(p, 1.0 / 0.05, 91)
        rgb = rgb * (0.88 + 0.18 * fine[:, None])
        wing = np.array([0.52, 0.42, 0.26]) * \
            (0.85 + 0.25 * cb.fbm(p, 1.0 / 0.08, 92)[:, None])
        rgb = np.where((region == REGION["wing"])[:, None], wing, rgb)
        rgb = np.where((region == REGION["eye"])[:, None],
                       np.array([0.22, 0.08, 0.05]), rgb)
        rgb = np.where((region == REGION["sting"])[:, None],
                       np.array([0.10, 0.08, 0.07]), rgb)
        rgb = np.where((region == REGION["leg"])[:, None],
                       np.array([0.62, 0.44, 0.12]), rgb)
        rgb = np.where((region == REGION["antenna"])[:, None],
                       np.array([0.12, 0.10, 0.08]), rgb)
        return rgb
    return colour


def main():
    parser = argparse.ArgumentParser()
    for name in ("rig", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig = shared.load_rig(args.rig)
    rig_hash = hashlib.sha256(Path(args.rig).read_bytes()).hexdigest()
    armature = cb.review_armature(rig_data, "GiantWaspEditableRig")
    wasp, weights = build(rig, armature)
    cb.unwrap(wasp)
    cb.bake_albedo(wasp, 1024, colour_field(), args.albedo)
    cb.attach_albedo_preview(wasp, args.albedo)
    extra = {"creature": "giant-wasp", "donorFamily": "giant-eagle",
             "visibleLegs": 6, "wings": 4, "stingRidesTail": True}
    summary = cb.write_mesh_data(args.mesh_data, wasp, weights, rig_hash,
                                 args.albedo, SPACE, extra)
    if not {"L_Arm_Upper", "R_Arm_Upper", "Tail"} <= set(summary["bones"]):
        raise SystemExit("the wings and sting must ride their donor bones")
    report = {"schemaVersion": 1, "builder": "build_giant_wasp.py",
              "rigSha256": rig_hash, **extra, **summary,
              "albedo": cb.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    groups = {}
    for index, row in weights.items():
        for name, value in row:
            group = groups.get(name) or wasp.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    modifier = wasp.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    wasp.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[giant-wasp] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
