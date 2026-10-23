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


def chela_rows(obj, line, names):
    # A single arclength coordinate across the continuous folded limb avoids
    # nearest-branch weight swaps on the inside of a tight elbow.
    cumulative = [0.]
    for a, b in zip(line, line[1:]):
        cumulative.append(cumulative[-1] + (b - a).length)
    result = {}
    for vertex in obj.data.vertices:
        nearest = []
        for i, (a, b) in enumerate(zip(line, line[1:])):
            axis = b - a
            t = max(0., min(1., (vertex.co-a).dot(axis)/axis.length_squared))
            nearest.append(((vertex.co-a.lerp(b,t)).length_squared,
                            cumulative[i] + t*axis.length))
        closest = min(pair[0] for pair in nearest)
        shares = [(np.exp(-(distance-closest)/(.12*.12)), along)
                  for distance, along in nearest]
        along = sum(a*b for a,b in shares)/sum(a for a,b in shares)
        def smooth(value):
            value = max(0., min(1., value))
            return value*value*(3.-2.*value)
        elbow = smooth((along-cumulative[2]+.34)/.68)
        wrist = smooth((along-cumulative[4]+.15)/.30)
        palm = smooth((along-cumulative[6]+.10)/.20)
        groups = (1.-elbow, elbow-wrist, wrist-palm, palm)
        row = []
        for group, i in enumerate((0, 2, 4)):
            t = smooth((along-cumulative[i])/(cumulative[i+1]-cumulative[i]))
            row.extend(((names[i], groups[group]*(1.-t)),
                        (names[i+1], groups[group]*t)))
        row.append((names[6], groups[3]))
        row = [(name, value) for name,value in row if value>1e-6]
        total = sum(value for _,value in row)
        row = [(name,value/total) for name,value in row]
        assert len(row)<=4 and all(value>=0. for _,value in row)
        result[vertex.index] = row
    return result

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
        # Original folded upper arm/forearm. The seven private render pivots
        # follow this authored centerline; native source joints stay untouched.
        root = Vector((sign * .50, -.32, .58))
        elbow = Vector((sign * 1.42, -.65, .58))
        wrist = Vector((sign * 1.30, -1.30, .52))
        palm = Vector((sign * 1.05, -1.55, .52))
        size = 1.16 if suffix == "L" else 1.
        volume = cb.Volume(.010)
        armline = [root, root.lerp(elbow, .5), elbow, elbow.lerp(wrist, .5), wrist, wrist.lerp(palm, .5), palm]
        volume.sweep(armline, [v * size for v in
                              (.155, .155, .080, .135, .100, .145, .165)], 20)
        volume.ellipsoid(palm, AXES, (.175 * size, .125 * size, .27 * size))
        lines = [(armline, names)]
        for lift in (-1, 1):
            finger_root = palm + UP * lift * .066 * size + FORWARD * .15
            outer = palm + UP * lift * .15 * size + FORWARD * .35 * size
            tip = palm + UP * lift * .095 * size + FORWARD * .61 * size
            line = [palm, finger_root, outer, tip]
            volume.sweep(line, [v * size for v in (.095, .085, .060, .012)], 16)
            lines.append((line, [names[6]] * 3))
        obj = union_part(parts, "CrabIntegratedChela", volume, lines, 2, 1800, 4)
        parts[-1] = (obj, chela_rows(obj, armline, names), 2)
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
                 originalChelaPivotSchema="cleanup2-folded-chela-v4",
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
        tips = np.clip((-p[:, 1]-1.85)/.25, 0., 1.) * (region == 2)
        elbow_radius = np.sqrt((np.abs(p[:, 0])-1.42)**2+(p[:, 1]+.65)**2)
        elbow_joint = np.exp(-(elbow_radius/.11)**2) * (region == 2)
        rgb *= (1.-.20*elbow_joint-.30*tips)[:, None]
        return rgb
    export(args, rig_data, digest, build(rig),
           "donor renderer local; +X right, +Z up, -Y forward", extra, coat)
    # The shared exporter uses host newline handling. Keep this pair's JSON
    # byte identity stable across Windows generation and LF Git checkout.
    for output in (args.mesh_data, args.report):
        path = Path(output)
        path.write_bytes(path.read_bytes().replace(b"\r\n", b"\n"))


if __name__ == "__main__":
    main()
