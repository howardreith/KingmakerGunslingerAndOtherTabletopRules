#!/usr/bin/env python3
"""Original hybrid body prototype; weapon handling is a separate live gate."""
import argparse
import hashlib
import json
import math
from pathlib import Path
import sys

from mathutils import Vector

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import generate_snakes as common
import captured_pose_review as observed

BODY_BONES = ("Torso_Lower", "Torso_Upper", "neck", "neck1", "Head", "jaw", "jaw1",
              "tail", "tail1", "tail2", "tail3") + tuple(
                  side + "_" + part for side in ("L", "R") for part in
                  ("clavicle", "Arm_Upper", "Arm_Lower", "Palm", "finger1", "finger2",
                   "Bfinger1", "Bfinger2"))
SIDE, UP, FORWARD = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, 1, 0))


def body_dorsal_at(point):
    upright = max(0, min(1, (point.z - .60) / .40))
    return UP * (1 - upright) - FORWARD * upright


def measured_rig(path):
    raw = Path(path).read_bytes()
    rig, by_name = decode_rig(json.loads(raw))
    return rig, by_name, hashlib.sha256(raw).hexdigest()


def decode_rig(capture):
    if (capture.get("key") != "salamander" or
            capture.get("nativeBlueprint") != "e8276e28b2234a745900fed80670bfdb" or
            capture.get("prefab") != "9b1744531a4428e44aa9837ca984513a"):
        raise SystemExit("wrong measured Salamander donor")
    skins = capture.get("skinnedRenderers", [])
    if len(skins) != 2 or {s.get("renderer") for s in skins} != {"_lizardman001", "_ammunition04"}:
        raise SystemExit("unexpected hybrid donor renderer set")
    source = next(s for s in skins if s["renderer"] == "_lizardman001")
    rows = source["bones"]
    by_name = {r["name"]: r for r in rows}
    if (len(rows) != 39 or len(by_name) != 39 or source["boneCount"] != 39 or
            source["bindPoseCount"] != 39 or source["rootBone"] != "Torso_Lower" or
            set(BODY_BONES) - set(by_name)):
        raise SystemExit("incomplete hybrid body bind frame")
    if (sorted(r.get("index", -1) for r in rows) != list(range(39)) or
            any(len(r.get("bindPosition", [])) != 3 or
                any(not math.isfinite(v) for v in r["bindPosition"]) for r in rows)):
        raise SystemExit("invalid hybrid bind coordinates or indices")
    for parent, child in (("Torso_Lower", "tail"), ("tail", "tail1"),
                          ("tail1", "tail2"), ("tail2", "tail3"), ("Head", "jaw")):
        if by_name[child].get("parent") != parent:
            raise SystemExit("disconnected hybrid tail or jaw")

    def depth(name, visited=()):
        if name in visited:
            raise SystemExit("cyclic hybrid graph")
        parent = by_name[name].get("parent")
        return depth(parent, visited + (name,)) + 1 if parent in by_name else 0

    bones = []
    for row in rows:
        point = Vector(row["bindPosition"])
        children = [Vector(c["bindPosition"]) for c in rows if c.get("parent") == row["name"]]
        tail = sum(children, Vector()) / len(children) if children else point + UP * .04
        if (tail - point).length < .001:
            tail = point + UP * .04
        bones.append(dict(name=row["name"], parent=row.get("parent", ""),
                          index=row["index"], depth=depth(row["name"]),
                          head=list(point), tail=list(tail)))
    rig = dict(source=capture["scope"], space=capture["space"],
               renderer="_lizardman001", rootBone="Torso_Lower", bindPoseCount=39, bones=bones)
    return rig, {r["name"]: r for r in bones}


def build_body(bm, weights, uvs, rig, kind, supported=False):
    if kind != "salamander":
        raise ValueError("not the hybrid identity")
    at = lambda name: common.shared.head(rig, name)

    def mass(centre, radii, bone, region="body"):
        common.ellipsoid(bm, weights, uvs, centre, SIDE, UP, FORWARD,
                         radii, bone, region, 8, 16)

    # A continuous rising serpentine lower body, never geometry on leg/foot
    # drivers. The distal curve belongs to tail3; native movement still needs
    # measured ground/contact review and is not invented by this rest shape.
    tail_names = ["Torso_Lower", "tail", "tail1", "tail2", "tail3"]
    points = [at(name) for name in tail_names]
    widths = [.22, .28, .31, .29, .24]
    heights = [.22, .26, .25, .23, .21]
    for point, radius in (((.10, -1.85, .19), .18), ((.55, -2.08, .16), .14),
                          ((1.02, -1.83, .13), .11), ((1.05, -1.36, .11), .08),
                          ((.76, -1.07, .095), .045), ((.57, -1.05, .09), .012)):
        points.append(Vector(point))
        widths.append(radius)
        heights.append(radius * .85)
        tail_names.append("tail3")
    # One skin from the distal coil through the waist and neck: no coplanar
    # caps or hard seam between two separately closed body segments.
    torso_points = [at("Torso_Upper"), at("Torso_Upper") + UP * .24,
                    at("neck"), at("neck1"), at("Head")]
    common.sweep(bm, weights, uvs,
        list(reversed(points)) + torso_points,
        list(reversed(widths)) + [.255, .30, .135, .11, .105],
        list(reversed(heights)) + [.185, .16, .105, .095, .10],
        list(reversed(tail_names)) + ["Torso_Upper", "Torso_Upper", "neck", "neck1", "Head"],
        up=FORWARD, forward=UP, uv_dorsal_at=body_dorsal_at)
    # Original chest/abdominal masses soften the upright silhouette.
    mass(at("Torso_Lower") + UP * .19 + FORWARD * .08,
         (.18, .23, .115), "Torso_Lower")
    for sign in (-1, 1):
        mass(at("Torso_Upper") + UP * .17 + SIDE * sign * .12 + FORWARD * .10,
             (.15, .14, .105), "Torso_Upper")
    mass(at("Head") + UP * .06, (.13, .18, .12), "Head")
    mass(at("Head") + FORWARD * .08 - UP * .025, (.10, .075, .09), "Head")
    common.sweep(bm, weights, uvs,
        [at("jaw"), at("jaw1"), at("jaw1") + FORWARD * .045],
        [.10, .105, .065], [.045, .04, .018], ["jaw", "jaw1", "jaw1"],
        steps=3, up=UP, forward=FORWARD)
    before = set(uvs)
    mass(at("jaw1") + FORWARD * .01 + UP * .03, (.08, .009, .045), "jaw1", "beak")
    common.remap_beak_uv(uvs, set(uvs) - before, .06, .43)

    for side, sign in (("L", 1), ("R", -1)):
        names = [side + "_" + part for part in ("clavicle", "Arm_Upper", "Arm_Lower", "Palm")]
        common.tube(bm, weights, uvs, [at(name) for name in names],
                    [.095, .115, .083, .048], names, "limbs", 12)
        mass(at(side + "_Palm") - UP * .067 + SIDE * sign * .022,
             (.077, .10, .071), side + "_Palm", "limbs")
        # Three long fingers plus thumb. Parallel authored digits use the two
        # measured flexion chains; this is not extra bones or a limb rewrite.
        for digit in range(3):
            offset = FORWARD * ((digit - 1) * .039)
            first, second = side + "_finger1", side + "_finger2"
            common.tube(bm, weights, uvs,
                [at(side + "_Palm") - UP * .055 + offset,
                 at(first) + offset, at(second) + offset,
                 at(second) + offset + Vector((-sign * .025, .005, -.05))],
                [.024, .023, .018, .007], [side + "_Palm", first, second, second], "limbs", 8)
        first, second = side + "_Bfinger1", side + "_Bfinger2"
        common.tube(bm, weights, uvs,
            [at(side + "_Palm") - UP * .045, at(first), at(second),
             at(second) + Vector((-sign * .03, .03, -.02))],
            [.03, .027, .021, .007], [side + "_Palm", first, second, second], "limbs", 8)
        mass(at("Head") + SIDE * sign * .088 + FORWARD * .113 + UP * .075,
             (.019, .019, .017), "Head", "crest")
        # Authored flame-like swept crest, not the donor's ears or snout.
        for i in range(3):
            base = at("Head") + SIDE * sign * (.09 - i * .018) + UP * (.11 + i * .027)
            tip = base + SIDE * sign * (.15 - i * .015) - FORWARD * (.10 + i * .025) + UP * (.08 + i * .035)
            common.tube(bm, weights, uvs, [base, base.lerp(tip, .6) + UP * .025, tip],
                        [.032, .025, .002], ["Head"] * 3, "membrane", 8)

    if supported:
        author_ground_support(weights)


def smoothstep(value):
    value = max(0, min(1, value))
    return value * value * (3 - 2 * value)


def author_ground_support(weights):
    """A static original belly support patch, not native leg/rig manipulation.

    Only the proximal lower body's original cross section is sculpted/weighted.
    The distal tail and every upper-body/hand driver remain unchanged. The
    renderer-frame driver is a proposed binding seam, NOT runtime-qualified.
    """
    lower = {"Torso_Lower", "tail", "tail1", "tail2", "tail3"}
    for vertex, influences in list(weights.items()):
        if not all(name in lower for name, _ in influences):
            continue
        # Full support under the central proximal coil, smoothly tapering out
        # before the distal tail. This is original anatomy, not a floor ray or
        # per-frame deformation of a native mesh.
        along = smoothstep((vertex.co.y + 1.45) / .25) * smoothstep((-.55 - vertex.co.y) / .25)
        cross_section = smoothstep((.95 - vertex.co.z) / .25)
        # Fade through the tail2/tail3 joint by its existing weights. A hard
        # exclusion of every mixed tail3 vertex makes a crease. Pure tail3
        # vertices, including the return coil in this Y band, stay identical.
        proximal = sum(weight for name, weight in influences if name != "tail3")
        support = along * cross_section * proximal
        if support <= 0:
            continue
        # Carry the whole proximal cross section, not just its underside.
        # Pinning a thin underside alone stretches it into a skirt as the
        # native torso bobs; the preserved v5 panels demonstrate that defect.
        vertex.co.z = max(.005, vertex.co.z - .16 * support)
        weights[vertex] = [(name, weight * (1 - support)) for name, weight in influences
                           if weight * (1 - support) > 1e-7]
        if support > 1e-7:
            weights[vertex].append((observed.HYBRID_SUPPORT, support))


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    parser.add_argument("--supported-prototype", action="store_true")
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    args.kind = "salamander"
    rig_data, rig, rig_hash = measured_rig(args.capture)
    allowed = BODY_BONES
    if args.supported_prototype:
        # Original offline preview frame, not a copied or new native joint.
        # Only original coordinates and this project-owned name are exported.
        rig_data["bones"].append(dict(name=observed.HYBRID_SUPPORT, parent="", index=39,
                                      depth=0, head=[0, 0, 0], tail=[0, 0, .1]))
        allowed += (observed.HYBRID_SUPPORT,)
    builder = lambda bm, weights, uvs, bones, kind: build_body(bm, weights, uvs, bones, kind, args.supported_prototype)
    common.write_prototype(args, rig_data, rig, rig_hash, builder, allowed,
        "native-lizardfolk-hybrid", "donor renderer local; +X lateral, +Y forward, +Z up",
        dict(visibleArms=2, weaponIncluded=False, weaponHandlingQualified=False,
             rendererFrameSupportPrototype=args.supported_prototype), max_influences=3 if args.supported_prototype else 2)


if __name__ == "__main__":
    main()
