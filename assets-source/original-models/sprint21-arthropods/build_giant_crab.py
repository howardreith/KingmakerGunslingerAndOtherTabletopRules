"""Cleanup 2 crab: flattened broad carapace, robust jointed legs and chelae.

Uses the previously measured Sprint 14 Giant Spider bind frame. Sprint 21's
independent census established the identical 51-bone donor; this builder
records the actual input hash rather than pretending to reuse that capture.
Only the geometry-free donor skeleton is read. No animation is authored.
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
sys.path.insert(0, str(HERE.parent / "sprint14-insects"))
import generate_sprint14_insects as insects
from cleanup2_authored_parts import cb, union_part, rigid, export, painted

SIDE, UP, FORWARD = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, -1, 0))
AXES = SIDE, UP, FORWARD


def build(rig):
    at = lambda name: Vector(rig[name]["head"])
    parts = []
    centre = at("LowerTorso") + Vector((0, .075, .025))
    shell = cb.Volume(.014)
    shell.ellipsoid(centre, AXES, (.86, .245, .57), 36, 20)
    # The flattened underside is tucked into the dome, not a second ball.
    shell.ellipsoid(centre - UP * .11, AXES, (.78, .13, .49))
    union_part(parts, "CrabCarapace", shell,
               [([centre - SIDE * .5, centre + SIDE * .5],
                 ["LowerTorso", "LowerTorso"])], 0, 2300)
    for sign, suffix in ((-1, "L"), (1, "R")):
        # Short rooted eyestalks, above the forward shell edge.
        root = centre + FORWARD * .49 + SIDE * sign * .19 + UP * .025
        eye = root + UP * .13 + FORWARD * .02
        stalk = cb.tube("CrabEyestalk", [root, eye], [.035, .026], 10)
        rigid(parts, stalk, "LowerTorso", 1)
        rigid(parts, cb.blob("CrabEye", eye, AXES, (.045, .038, .045), 12, 8),
              "LowerTorso", 3)
        for i in range(4):
            names = [f"{suffix}_Leg{i}_Upper", f"{suffix}_Leg{i}_Lower",
                     f"{suffix}_Foot{i}"]
            hip, knee, foot = (at(n) for n in names)
            # Broad coxae grow from inside the shell. The knee stays below
            # its crown; following the spider's raised knee made wire legs.
            root = Vector((sign * .53, hip.y, centre.z - .09))
            hip = Vector((sign * .77, hip.y, centre.z - .10))
            knee = Vector((knee.x + sign * .15, knee.y * .78,
                           centre.z - .015))
            ankle = Vector((foot.x + sign * .10, foot.y * .80, foot.z))
            toe = ankle + (ankle - knee).normalized() * .17 - UP * .07
            line = [root, hip, knee, ankle, toe]
            volume = cb.Volume(.010)
            volume.sweep(line, [.115, .105, .092, .038, .012], 18)
            union_part(parts, "CrabWalkingLeg", volume,
                       [(line, [names[0], names[0], names[1], names[2]])],
                       1, 650, 3)

        names = [f"pedipalp{i}_{suffix}" for i in range(1, 8)]
        joints = [at(n) for n in names]
        # Two folded arm segments, then one broad, flattened palm. Both
        # fingers emerge from the same continuous chela surface.
        root = centre + SIDE * sign * .43 + FORWARD * .32 - UP * .12
        elbow = joints[2] + SIDE * sign * .16 + FORWARD * .13
        wrist = joints[4] + SIDE * sign * .20 + FORWARD * .24
        palm = wrist + FORWARD * .15
        size = 1.16 if suffix == "L" else 1.
        volume = cb.Volume(.010)
        armline = [root, joints[0], joints[1], elbow, joints[3], wrist, palm]
        volume.sweep(armline, [v * size for v in
                              (.145, .14, .135, .125, .14, .155, .18)], 20)
        volume.ellipsoid(palm, AXES, (.21 * size, .125 * size, .24 * size))
        lines = [(armline, names)]
        for lift in (-1, 1):
            finger_root = palm + UP * lift * .066 * size + FORWARD * .12
            outer = palm + UP * lift * .15 * size + FORWARD * .29 * size
            tip = palm + UP * lift * .095 * size + FORWARD * .49 * size
            line = [palm, finger_root, outer, tip]
            volume.sweep(line, [v * size for v in (.10, .09, .067, .013)], 16)
            lines.append((line, [names[6]] * 3))
        union_part(parts, "CrabIntegratedChela", volume, lines, 2, 1800, 4)
    return parts


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, digest = insects.measured_rig(args.capture, "giant-stag-beetle")
    if len(rig_data["bones"]) != 51:
        raise SystemExit("Giant Crab requires the measured 51-bone spider frame")
    extra = dict(creature="giant-crab", donorBlueprint="9e120b5e0ad3c794491c049aa24b9fde",
                 donorRenderer="Spider", visibleLegs=8, visiblePincers=2,
                 printedSize="Medium", widthOverLength=1.3,
                 legChainUsage={str(i): "full" for i in range(4)},
                 authoredLimitation="CRAB_WALKS_A_SPIDER_RIG_FRONT_TO_BACK")
    base_coat = painted((.21, .105, .065), (.48, .28, .16), 2,
                        {1: (.26, .14, .085), 2: (.43, .22, .12),
                         3: (.025, .018, .012)})
    def coat(p, n, region):
        rgb = base_coat(p, n, region)
        radius = np.sqrt((p[:, 0] / .86) ** 2 + ((p[:, 1] - .075) / .57) ** 2)
        rim = np.clip((radius - .86) / .12, 0, 1) * (region == 0)
        rgb += rim[:, None] * np.array((.09, .065, .035))
        speckle = np.clip((cb.fbm(p, 70., 325) - .58) * 8., 0., 1.)
        rgb *= 1. - .18 * speckle[:, None] * (region != 3)[:, None]
        tips = ((p[:, 1] < -1.04) & (region == 2))[:, None]
        return np.where(tips, rgb * .67, rgb)
    export(args, rig_data, digest, build(rig),
           "donor renderer local; +X right, +Z up, -Y forward", extra, coat)


if __name__ == "__main__":
    main()
