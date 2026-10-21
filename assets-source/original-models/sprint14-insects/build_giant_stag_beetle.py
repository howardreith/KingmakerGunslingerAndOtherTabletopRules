"""Cleanup 2 Stag Beetle: distinct head/pronotum, paired elytra, six legs.

Only this beetle is rebuilt. Fire Beetle and ant exports are untouched. The
geometry-free measured spider skeleton supplies existing animation drivers;
the spare fourth leg pair remains empty and the jaws use the chelicerae.
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
import generate_sprint14_insects as legacy
from cleanup2_authored_parts import cb, union_part, rigid, export, painted

SIDE, UP, FORWARD = legacy.spider_frame()
AXES = SIDE, UP, FORWARD


def build(rig):
    at = lambda n: Vector(rig[n]["head"])
    parts = []
    thorax = at("LowerTorso")
    abdomen = at("UpperTorso")
    shell = cb.Volume(.014)
    head = thorax + FORWARD * .53 - UP * .035
    shell.ellipsoid(head, AXES, (.32, .19, .28))
    shell.ellipsoid(thorax + FORWARD * .08 + UP * .025,
                    AXES, (.49, .265, .39))
    shell.ellipsoid(abdomen - FORWARD * .02, AXES, (.53, .29, .70), 36, 20)
    spine = [head + FORWARD * .16, head, thorax,
             at("Tail1_M"), abdomen, at("Tail3_M")]
    union_part(parts, "StagBeetleShell", shell,
               [(spine, ["LowerTorso", "LowerTorso", "LowerTorso",
                         "Tail1_M", "UpperTorso", "Tail3_M"])], 0, 3600, 5)
    for sign, suffix in ((-1, "L"), (1, "R")):
        # Convex wing cases sit tightly on the abdominal surface, meeting
        # in a narrow dark suture instead of the old coarse raised blades.
        cover = cb.Volume(.010)
        cover.ellipsoid(abdomen + SIDE * sign * .245 + UP * .19,
                        AXES, (.274, .155, .64), 32, 18)
        line = [at("Tail1_M"), abdomen, at("Tail3_M")]
        union_part(parts, "StagBeetleElytron", cover,
                   [(line, ["Tail1_M", "UpperTorso", "Tail3_M"])],
                   1, 1350, 4)
        eye = head + SIDE * sign * .265 + UP * .05 + FORWARD * .08
        rigid(parts, cb.blob("StagBeetleEye", eye, AXES, (.073, .061, .078), 14, 10),
              "LowerTorso", 3)
        jaw = "chelicera_" + suffix
        root = head + SIDE * sign * .15 + FORWARD * .15
        line = [root, root + FORWARD * .25 + SIDE * sign * .10 + UP * .06,
                root + FORWARD * .61 + SIDE * sign * .13 + UP * .13,
                root + FORWARD * .92 + SIDE * sign * .03 + UP * .15,
                root + FORWARD * 1.07 - SIDE * sign * .11 + UP * .12]
        chela = cb.Volume(.009)
        chela.sweep(line, [.13, .12, .089, .055, .014], 20)
        tine = line[2]
        chela.sweep([tine, tine + FORWARD * .11 - SIDE * sign * .23 + UP * .02],
                    [.065, .012], 16)
        union_part(parts, "StagBeetleAntlerMandible", chela,
                   [(line, [jaw] * 4)], 2, 1400, 4)
        # A compact elbowed, clubbed antenna, rooted in the capsule. Its
        # thin stalk is never another leg and the terminal club is visible.
        ant_names = [f"pedipalp{i}_{suffix}" for i in (1, 3, 5, 7)]
        ant_line = [head + SIDE * sign * .25,
                    head + SIDE * sign * .42 + FORWARD * .10 + UP * .12,
                    head + SIDE * sign * .44 + FORWARD * .31 + UP * .15,
                    head + SIDE * sign * .43 + FORWARD * .40 + UP * .17]
        antenna = cb.Volume(.007)
        antenna.sweep(ant_line, [.037, .027, .026, .057], 14)
        union_part(parts, "StagBeetleClubbedAntenna", antenna,
                   [(ant_line, ant_names)], 2, 350, 3)
        for i in range(3):
            names = list(legacy.leg_chain(suffix, str(i)))
            hip, knee, ankle = (at(n) for n in names)
            root = Vector((sign * .31, hip.y, thorax.z - .10))
            hip = Vector((sign * .48, hip.y, thorax.z - .12))
            knee = Vector((knee.x, knee.y, thorax.z + .17))
            toe = ankle + (ankle - knee).normalized() * .17 - UP * .04
            line = [root, hip, hip.lerp(knee, .56), knee, ankle, toe]
            limb = cb.Volume(.009)
            # Femur/tibia/tarsus are anatomically distinct, without the
            # former .21-radius wedge obscuring the shell and leg count.
            limb.sweep(line, [.10, .105, .115, .075, .037, .018], 18)
            union_part(parts, "StagBeetleLeg", limb,
                       [(line, [names[0], names[0], names[0], names[1], names[2]])],
                       2, 750, 3)
    return parts


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    data, rig, digest = legacy.measured_rig(args.capture, "giant-stag-beetle")
    extra = dict(creature="giant-stag-beetle", visibleLegs=6,
                 legChainUsage={"0": "full", "1": "full", "2": "full", "3": "empty"})
    colour = painted((.12, .061, .034), (.29, .15, .075), 2,
                     {2: (.09, .046, .025), 3: (.018, .012, .009)})
    def coat(p, n, region):
        rgb = colour(p, n, region)
        # Warm dark mahogany wing covers with subtle longitudinal grooves.
        groove = (.5 + .5 * np.sin(np.abs(p[:, 0]) * 100.)) ** 10
        cover = np.array((.255, .108, .048)) * \
            (.82 + .22 * np.clip(n[:, 2], 0, 1)[:, None]) * \
            (1. - .13 * groove[:, None])
        return np.where((region == 1)[:, None], cover, rgb)
    export(args, data, digest, build(rig),
           "donor renderer local; +X right, +Z up, -Y forward", extra, coat)


if __name__ == "__main__":
    main()
