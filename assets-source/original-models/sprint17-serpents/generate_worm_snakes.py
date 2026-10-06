#!/usr/bin/env python3
"""Original continuous-chain snake prototype, not native worm geometry.

Only an exact private bind-metadata capture is consumed. Radial jaw petals,
horn and stone joints never carry authored vertices. No animation is copied.
"""
import hashlib
import json
import math
from pathlib import Path
import sys

from mathutils import Vector

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import generate_snakes as common

KINDS = common.KINDS
BODY_CHAIN = ("Hips_Joints",) + tuple("Body0" + str(i) for i in range(2, 15)) + ("Head",)
BODY_BONES = BODY_CHAIN + ("Jaw_Down",)
SIDE, UP, FORWARD = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, 1, 0))


def supporting_coil(radius):
    """Original ground-plane tail, continuous with the first body segment.

    The native bind chain rises along +Y; +Z is the upper-jaw direction,
    not ground-up. This is geometry on Hips, not a new animation driver.
    Native locomotion/ground support still require live measurement.
    """
    points, widths = [], []
    for index in range(29):
        t = index / 28
        angle = -math.pi / 2 + t * math.pi * 2.6
        extent = 1.85 * (1 - t) + .55 * t
        width = radius * min(1, .06 + t * 3.2)
        points.append(Vector((math.cos(angle) * extent, radius,
                              math.sin(angle) * extent)))
        widths.append(width)
    # A curved inward continuation, not a separate torus or intersecting cap.
    points.extend((Vector((.18, radius + .08, .30)),
                   Vector((0, radius + .18, .08))))
    widths.extend((radius, radius))
    return points, widths


def body_dorsal_at(point):
    # Dorsal paint follows the top of the supporting coil, then the neck's
    # jaw-up axis. These directions only control original UVs, never the rig.
    upright = max(0, min(1, (point.y - .60) / .80))
    return FORWARD * (1 - upright) + UP * upright


def measured_rig(path):
    raw = Path(path).read_bytes()
    rig, by_name = decode_rig(json.loads(raw))
    return rig, by_name, hashlib.sha256(raw).hexdigest()


def decode_rig(capture):
    if (capture.get("key") != "purple-worm" or
            capture.get("nativeBlueprint") != "bf2216f48b3f4d24c9c502007649340d" or
            capture.get("prefab") != "130f0866af3249a4e817ec7e6e9ecd89"):
        raise SystemExit("wrong native worm research capture")
    skins = capture.get("skinnedRenderers", [])
    if len(skins) != 2 or {s.get("renderer") for s in skins} != {"Purple_Worm", "Purple_Worm_Stones"}:
        raise SystemExit("unexpected native worm renderer set")
    source = next(s for s in skins if s["renderer"] == "Purple_Worm")
    rows = source["bones"]
    by_name = {r["name"]: r for r in rows}
    if (len(rows) != 40 or len(by_name) != 40 or source["boneCount"] != 40 or
            source["bindPoseCount"] != 40 or source["rootBone"] != "Hips_Joints" or
            set(BODY_BONES) - set(by_name)):
        raise SystemExit("incomplete native worm body frame")
    if (sorted(r.get("index", -1) for r in rows) != list(range(40)) or
            any(len(r.get("bindPosition", [])) != 3 or
                any(not math.isfinite(v) for v in r["bindPosition"]) for r in rows)):
        raise SystemExit("invalid native bind coordinates or indices")
    for parent, child in zip(BODY_CHAIN, BODY_CHAIN[1:]):
        if by_name[child].get("parent") != parent:
            raise SystemExit("discontinuous native worm body chain")
    if by_name["Jaw_Down"].get("parent") != "Head":
        raise SystemExit("detached native lower jaw")

    def depth(name, visited=()):
        if name in visited:
            raise SystemExit("cyclic bone graph")
        parent = by_name[name].get("parent")
        return depth(parent, visited + (name,)) + 1 if parent in by_name else 0

    bones = []
    for row in rows:
        point = Vector(row["bindPosition"])
        children = [Vector(c["bindPosition"]) for c in rows if c.get("parent") == row["name"]]
        tail = sum(children, Vector()) / len(children) if children else point + FORWARD * .1
        if (tail - point).length < .001:
            tail = point + FORWARD * .1
        bones.append(dict(name=row["name"], parent=row.get("parent", ""),
                          index=row["index"], depth=depth(row["name"]),
                          head=list(point), tail=list(tail)))
    rig = dict(source=capture["scope"], space=capture["space"],
               renderer="Purple_Worm", rootBone="Hips_Joints", bindPoseCount=40, bones=bones)
    return rig, {r["name"]: r for r in bones}


def build_body(bm, weights, uvs, rig, kind):
    if kind not in KINDS:
        raise ValueError("unknown snake identity")
    viper = kind == "viper"
    at = lambda name: common.shared.head(rig, name)
    radius = .25 if viper else .47
    points, widths = supporting_coil(radius)
    heights = [width * .88 for width in widths]
    names = ["Hips_Joints"] * len(points)
    # Each portion follows its measured segment, unlike the water rig's rigid
    # lower coil. The taper is original anatomy, not native vertices/UVs.
    for i, name in enumerate(BODY_CHAIN[1:], 1):
        t = i / (len(BODY_CHAIN) - 1)
        width = radius
        if t > .78:
            width *= 1 - (t - .78) * 1.7
        points.append(at(name))
        widths.append(width)
        heights.append(width * .88)
        names.append(name)
    skull = at("Head")
    head_width = .55 if viper else .49
    for offset, width, height in ((.23, head_width, .26),
                                  (.63, head_width * .88, .23),
                                  (1.03, head_width * .48, .17),
                                  (1.12, head_width * .18, .09)):
        points.append(skull + FORWARD * offset - UP * .04)
        widths.append(width)
        heights.append(height)
        names.append("Head")
    common.sweep(bm, weights, uvs, points, widths, heights, names,
                 up=UP, forward=FORWARD, uv_dorsal_at=body_dorsal_at)
    # Authored conventional lower jaw: no worm's radial side/upper petals.
    hinge = at("Jaw_Down")
    jaw_points = [hinge + UP * .16 - FORWARD * .16,
                  skull + FORWARD * .45 - UP * .34,
                  skull + FORWARD * .93 - UP * .23,
                  skull + FORWARD * 1.10 - UP * .17]
    common.sweep(bm, weights, uvs, jaw_points,
                 [head_width * .72, head_width * .84, head_width * .48, head_width * .16],
                 [.09, .09, .055, .028], ["Jaw_Down"] * 4,
                 steps=3, up=UP, forward=FORWARD)

    def mass(centre, radii, bone, region):
        common.ellipsoid(bm, weights, uvs, centre, SIDE, UP, FORWARD,
                         radii, bone, region, 6, 12)

    before = set(uvs)
    mass(skull + FORWARD * .61 - UP * .235,
         (head_width * .63, .026, .36), "Jaw_Down", "beak")
    common.remap_beak_uv(uvs, set(uvs) - before, .06, .43)
    for sign in (-1, 1):
        eye = skull + SIDE * sign * head_width * .84 + FORWARD * .36 + UP * .095
        mass(eye, (.067, .062, .078), "Head", "crest")
        for i in range(2 if viper else 5):
            base = (skull + SIDE * sign * head_width * (.65 - i * .055) +
                    FORWARD * (.53 + i * (.17 if viper else .10)) - UP * .16)
            tip = base - UP * (.22 if viper and i == 0 else .075) - FORWARD * .045
            before = set(uvs)
            common.tube(bm, weights, uvs,
                        [base, base.lerp(tip, .6) + FORWARD * .023, tip],
                        [.034 if viper else .019, .016, .003], ["Head"] * 3, "beak", 7)
            common.remap_beak_uv(uvs, set(uvs) - before, .57, .94)


def main():
    args = common.parse_args()
    rig_data, rig, rig_hash = measured_rig(args.capture)
    common.write_prototype(args, rig_data, rig, rig_hash, build_body, BODY_BONES,
                           "native-purple-worm",
                           "donor renderer local; +X lateral, +Y longitudinal, +Z upper jaw")


if __name__ == "__main__":
    main()
