"""Cleanup 2 Stegosaurus, authored on the unchanged six-joint Hodag rig.

An elongated elliptical barrel replaces the overlapping circular torso
tubes. Four weight-blended legs grow from it; the low head and narrowing
neck, staggered dorsal plates and muscular thagomizer remain distinct.
"""
import argparse
from pathlib import Path
import sys

import bpy
import numpy as np
from mathutils import Vector

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
sys.path.insert(0, str(HERE.parent / "sprint21-arthropods"))
import generate_armored_dinosaurs as legacy
from cleanup2_authored_parts import cb, chain_weights, union_part, rigid, export, painted

SIDE, UP, FORWARD = Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1))
AXES = SIDE, UP, FORWARD


def build(rig):
    at = lambda n: Vector(rig[n]["head"])
    # Original mesh proportions only. The source skeleton, view scale and
    # mechanical dimensions remain untouched. Lift the barrel above longer
    # supporting limbs instead of widening the former crouched sockets.
    lift = .10
    parts = []
    body = cb.Volume(.014)
    # Long, high-hipped body. Its belly clears the ground and does not
    # swallow the legs as the former radius-.86 circular tube did.
    body.ellipsoid(Vector((0, .70 + lift, .05)), AXES, (.43, .41, .82), 36, 22)
    body.ellipsoid(Vector((0, .77 + lift, -.20)), AXES, (.44, .41, .48))
    body.ellipsoid(Vector((0, .65 + lift, .60)), AXES, (.32, .30, .43))
    neckline = [at("UpperTorso") + UP * lift, at("Neck_M") + UP * .08,
                at("Head") + UP * .07,
                at("Head") + FORWARD * .23 + UP * .045]
    body.sweep(neckline, [.27, .18, .13, .08], 20)
    body.ellipsoid(at("Head") + FORWARD * .07 + UP * .07, AXES, (.14, .13, .24))
    spine = [at(n) for n in ("Tail0_M", "LowerTorso", "Spine1_M", "UpperTorso")]
    lines = [(spine, ["LowerTorso", "LowerTorso", "Spine1_M", "UpperTorso"]),
             (neckline, ["UpperTorso", "Neck_M", "Head", "Head"])]
    tail_names = ["Tail%d_M" % i for i in range(6)]
    tailline = [at(n) + UP * rise for n, rise in
                zip(tail_names, (.10, .065, .025, 0., 0., 0.))]
    body.sweep(tailline, [.30, .24, .18, .135, .095, .055], 20)
    lines.append((tailline, tail_names))
    for prefix in ("Left", "Right"):
        for names in ((prefix + "UpperArm", prefix + "LowerArm", prefix + "Hand"),
                      (prefix + "UpperLeg0", prefix + "LowerLeg0", prefix + "Foot0")):
            hip, knee, ankle = (at(n) for n in names)
            is_rear = "Leg0" in names[0]
            root = Vector((hip.x * .82, .80 if is_rear else .76, hip.z))
            # The old path passed down through the Hodag upper hind pivot
            # and then back up through its knee. The authored thigh now
            # descends continuously. It retains the same driver sequence.
            if is_rear:
                hip = Vector((hip.x * .88, .64, hip.z - .055))
                knee = Vector((knee.x * 1.16, .405, knee.z))
            else:
                hip.x *= 1.02
                knee.x *= .84
                knee.z += .045
            # Retain the measured joints and continuous barrel attachment,
            # but distinguish the limb shaft from a compact supporting foot.
            # The supporting pad keeps the preceding authored sole plane:
            # .105 + .085 equals the previous offset plus radius (.10 + .09).
            toe = ankle + FORWARD * .10 - UP * .105
            line = [root, hip, knee, ankle, toe]
            radii = [.20, .17, .12, .10, .105] if is_rear else \
                    [.155, .14, .11, .09, .10]
            body.sweep(line, radii, 20)
            body.ellipsoid(toe, AXES, (.125, .085, .13), 20, 12)
            # Three shallow toe ends join the supporting pad. Their soles
            # stay above its lowest plane; no extra floating foot pieces.
            for toe_index in (-1, 0, 1):
                body.ellipsoid(toe + SIDE * toe_index * .063 +
                               FORWARD * .09 + UP * .007,
                               AXES, (.043, .063, .067), 16, 10)
            lines.append((line, [names[0], names[0], names[1], names[2]]))
    union_part(parts, "StegosaurusContinuousBody", body, lines, 0, 10000, 7)
    # Nine broad thin plates in two staggered files. Each root is buried
    # into the back, and its owning control is the local body control.
    stations = [(-.63, .91, .25, .16, "Tail0_M"),
                (-.38, 1.01, .43, .20, "LowerTorso"),
                (-.13, 1.09, .62, .24, "LowerTorso"),
                (.14, 1.06, .75, .26, "Spine1_M"),
                (.42, 1.01, .69, .25, "Spine1_M"),
                (.69, .91, .55, .22, "UpperTorso"),
                (.91, .79, .40, .18, "UpperTorso"),
                (1.12, .69, .28, .15, "Neck_M"),
                (1.31, .61, .16, .11, "Head")]
    for i, (z, y, height, depth, bone) in enumerate(stations):
        sign = -1 if i % 2 else 1
        centre = Vector((sign * .065, y + (lift if z < 1. else .075), z))
        # A gently convex rhomboid plate, not a triangular paper plane.
        # The broad closed plate is the established authored plate, with
        # its exact two-sided volume and convex rim, preserved as a solid.
        import bmesh
        bm = bmesh.new()
        w, uv = {}, {}
        legacy.plate(bm, w, uv, centre, .045, height, depth, bone)
        # The old plate builder's two end caps oppose its walls. Correct
        # those caps explicitly; do not hide them with double-sided paint.
        bm.faces.ensure_lookup_table()
        bm.faces[0].normal_flip()
        bm.faces[1].normal_flip()
        bm = cb.fan_triangles(bm)
        cb.orient_outward(bm)
        mesh = bpy.data.meshes.new("StegosaurusPlate")
        bm.to_mesh(mesh)
        bm.free()
        obj = bpy.data.objects.new("StegosaurusPlate", mesh)
        bpy.context.collection.objects.link(obj)
        rigid(parts, obj, bone, 1)
    for joint, spread, lift in (("Tail4_M", .38, .17), ("Tail5_M", .46, .10)):
        base = at(joint)
        for sign in (-1, 1):
            root = base + SIDE * sign * .045
            tip = base + SIDE * sign * spread + UP * lift - FORWARD * .22
            rigid(parts, cb.horn("StegosaurusThagomizer", root, tip, .075,
                                  12, 8, hint=UP), joint, 2)
    for sign in (-1, 1):
        eye = at("Head") + SIDE * sign * .122 + UP * .12 + FORWARD * .07
        rigid(parts, cb.blob("StegosaurusEye", eye, AXES, (.022, .025, .026), 10, 6),
              "Head", 3)
    # Keep the jaw control represented, with a rooted small lower beak.
    rigid(parts, cb.blob("StegosaurusJaw", at("Jaw_M") + FORWARD * .12 + UP * .01,
                         AXES, (.09, .065, .16), 18, 10), "Jaw_M", 2)
    return parts


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    data, rig, digest = legacy.measured_rig(args.capture, "stegosaurus")
    extra = dict(creature="stegosaurus", nativeDonor=legacy.DONORS["stegosaurus"],
                 visibleLegs=4, horns=0, frill=False, dorsalPlates=9,
                 tailJoints=6, tailSpikes=4, terminalClub=False)
    base_coat = painted((.17, .19, .12), (.37, .39, .25), 1,
                        {1: (.47, .26, .14), 2: (.24, .21, .15),
                         3: (.035, .025, .012)})
    def coat(p, n, region):
        rgb = base_coat(p, n, region)
        stripes = np.clip(np.sin(p[:, 2] * 17. + cb.fbm(p, 7., 333) * 2.) - .3,
                          0., 1.) * np.clip(n[:, 1], 0., 1.)
        rgb *= 1. - .23 * stripes[:, None] * (region == 0)[:, None]
        belly = np.clip(-n[:, 1], 0., 1.) * (region == 0)
        rgb += belly[:, None] * np.array((.065, .06, .035))
        # Fine original scale edges and irregular folds interrupt the smooth
        # rubbery diffuse field. These are albedo marks, not a shader claim.
        first, second = cb.cells(p, 34., 841)
        scale_edge = np.exp(-np.maximum(second - first, 0.) * 42.)
        rgb *= 1. - .17 * scale_edge[:, None] * (region == 0)[:, None]
        wrinkles = np.exp(-np.abs(np.sin(p[:, 1] * 52. + cb.fbm(p, 6., 842) * 7.)) * 15.)
        rgb *= 1. - .075 * wrinkles[:, None] * (region == 0)[:, None]
        plate_grain = cb.fbm(p * np.array((1., .38, 2.)), 21., 843)
        plate_rgb = np.array((.36, .235, .145)) * (.77 + .35 * plate_grain[:, None])
        return np.where((region == 1)[:, None], plate_rgb, rgb)
    export(args, data, digest, build(rig),
           "donor renderer local; +X lateral, +Y up, +Z forward", extra, coat)


if __name__ == "__main__":
    main()
