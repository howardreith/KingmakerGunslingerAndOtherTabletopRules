#!/usr/bin/env python3
"""Decode the Sprint 21 two-family donor census into a bind frame.

What it takes from the game is a bind-pose skeleton and nothing else: bone
names, parents and bind positions. No vertex, index, UV, bind matrix, texture
pixel, shader or animation curve is read here, and none is redistributed.

The census surveyed two families independently and both landed on the same
rig, for different measured reasons. That is a finding rather than an
assumption, and it is the reason this module takes the family name as an
argument: each body states which survey it is authored against, and a body
built from the other family's row fails rather than silently inheriting a
decision nobody took for it.

What the two families actually measured:

- The Bebelith. Its released donor is the Doomspider, and the Doomspider and
  the Giant Spider are the same 51-bone skeleton - the census measured both
  and they agree bone for bone. The only other demonic candidate the library
  offers is DemonEidolonUnit, a biped with one leg pair. So a Huge
  spider-hunter has exactly one rig available with more than two legs, and
  the body authored onto it must not be a Doomspider repaint: it walks on
  six, not eight, and carries its remaining pair elevated.
- The Giant Crab. Centipedes report four leg pairs of nothing - that family
  names its chains differently - and the only rig in the installation with
  eight real leg chains and two forward articulated chains is the same
  arachnid skeleton. A crab needs eight legs and two pincers, and this is
  where they are.

It validates hard before it returns, because a body authored against the
wrong frame is worse than a body that fails to build.
"""
import hashlib
import json
import math
from pathlib import Path

from mathutils import Vector

GIANT_SPIDER = "9e120b5e0ad3c794491c049aa24b9fde"
DOOMSPIDER = "51c66b0783a748c4b9538f0f0678c4d7"
RENDERER = "Spider"
ROOT_BONE = "Position"
BONE_COUNT = 51
ROW_SPACE = ("renderer-local bind frame; no native vertices, indices, "
             "UVs, textures or animation curves")
SPACE = "donor renderer local; +X right, +Z up, -Y forward"
SCOPE_PREFIX = "Sprint 21 two-family donor census"

# Which anchor each family authors against, and why it is that one. The
# Bebelith takes the Doomspider because that is the body it already wears and
# the frame a player's save already resolves; the crab takes the Giant
# Spider because it has no released body and that is the family's own anchor.
FAMILY_ANCHORS = {
    "bebelith": ("doomspider", DOOMSPIDER),
    "giant-crab": ("giant-spider", GIANT_SPIDER),
}

# The abdomen chain. Two tail bones with UpperTorso between them, all level
# at about z 0.7 and running backward: Sprint 20 measured it and this census
# re-measured it unchanged. The Bebelith hangs a web-spinning abdomen on it.
# The crab must not touch it at all - a crab with a spider's abdomen is the
# single most likely way this sprint's second body goes wrong.
ABDOMEN_CHAIN = ("Tail1_M", "UpperTorso", "Tail3_M")
TAIL_BONES = ("Tail1_M", "Tail3_M")

# Seven bones a side, forward-reaching. The Bebelith's elevated barbed
# foreclaws and the crab's pincers are both authored here, which is anatomy
# in both cases rather than substitution.
PEDIPALPS = tuple("pedipalp%d_%s" % (index, side)
                  for side in ("L", "R") for index in (1, 2, 3, 4, 5, 6, 7))

LEG_PARTS = tuple(
    "%s_Leg%d_%s" % (side, index, part)
    for side in ("L", "R") for index in (0, 1, 2, 3)
    for part in ("Upper", "Lower"))
LEG_FEET = tuple("%s_Foot%d" % (side, index)
                 for side in ("L", "R") for index in (0, 1, 2, 3))

TORSO = ("Position", "LowerTorso", "UpperTorso")
# The jaw. The first census run reported none, because this skeleton spells
# its legs capitalised and its cheliceræ lowercase and the candidate list had
# a capital C. The raw bone roster the capture also writes is what exposed
# it, and it is the reason the capture writes one.
CHELICERAE = ("chelicera_L", "chelicera_R")
FEMURS = tuple("femur%d_%s" % (index, side)
               for side in ("L", "R") for index in (1, 2, 3))

# Every bone this sprint is allowed to weight geometry to. A bone outside
# this set is an unreviewed driver, and the generators refuse one.
DRIVERS = (TORSO + TAIL_BONES + PEDIPALPS + LEG_PARTS + LEG_FEET +
           CHELICERAE + FEMURS)


def load(path, family):
    """Decode and hash one family's anchor row. The hash is the provenance."""
    raw = Path(path).read_bytes()
    document = json.loads(raw.decode("utf-8-sig"))
    rig_data, by_name = decode(document, family)
    return rig_data, by_name, hashlib.sha256(raw).hexdigest()


def _family(document, family):
    if family not in FAMILY_ANCHORS:
        raise SystemExit("unknown census family: " + str(family))
    families = document.get("families")
    if not isinstance(families, list) or len(families) != 2:
        raise SystemExit("the capture is not a two-family census")
    found = [row for row in families if row.get("family") == family]
    if len(found) != 1:
        raise SystemExit("the capture has no family named " + family)
    return found[0]


def _row(block, family):
    key, guid = FAMILY_ANCHORS[family]
    rows = block.get("rows")
    if not isinstance(rows, list) or not rows:
        raise SystemExit("the " + family + " survey carries no rows")
    found = [row for row in rows if row.get("nativeBlueprint") == guid]
    if len(found) != 1:
        raise SystemExit("the " + family + " survey does not hold exactly "
                         "one row for its anchor")
    row = found[0]
    if row.get("key") != key or row.get("isAnchor") is not True:
        raise SystemExit("the " + family + " anchor row is not " + key)
    if row.get("space") != ROW_SPACE:
        raise SystemExit("the donor row was not captured read-only and detached")
    if row.get("skippedRenderers"):
        raise SystemExit("the donor capture skipped a renderer; its frame is "
                         "incomplete")
    return row


def decode(document, family):
    """Validate the capture hard, then shape it for the Blender armature."""
    if not str(document.get("scope", "")).startswith(SCOPE_PREFIX):
        raise SystemExit("not a read-only Sprint 21 two-family census capture")
    block = _family(document, family)
    row = _row(block, family)

    skins = row.get("skinnedRenderers")
    if not isinstance(skins, list) or len(skins) != 1 or \
            skins[0].get("renderer") != RENDERER:
        raise SystemExit("the donor no longer has exactly its one body skin")
    body = skins[0]
    bones = body.get("bones")
    if (body.get("rootBone") != ROOT_BONE or
            body.get("boneCount") != BONE_COUNT or
            body.get("bindPoseCount") != BONE_COUNT or
            not isinstance(bones, list) or len(bones) != BONE_COUNT):
        raise SystemExit("the donor body frame is not the surveyed 51-bone rig")

    by_row = {}
    for bone in bones:
        name, position = bone.get("name"), bone.get("bindPosition")
        if (not name or name in by_row or not isinstance(position, list) or
                len(position) != 3 or
                any(not math.isfinite(float(value)) for value in position)):
            raise SystemExit("the donor bind frame is malformed at " + str(name))
        by_row[name] = bone
    for name in DRIVERS:
        if name not in by_row:
            raise SystemExit("the donor rig is missing a reviewed bone: " + name)

    # The measurements the census reported, re-asserted so a generator cannot
    # run against a frame that quietly changed shape.
    if row.get("walkingLegPairs") != 4:
        raise SystemExit("the donor no longer has four complete leg pairs")
    if row.get("forelimbBones") != 10:
        raise SystemExit("the forelimb chains are no longer what was measured")
    if row.get("abdomenBones") != len(TAIL_BONES):
        raise SystemExit("the abdomen chain is no longer two tail bones")

    children = {}
    for bone in bones:
        parent = bone.get("parent")
        if parent in by_row:
            children.setdefault(parent, []).append(bone["name"])

    def depth(name):
        parent = by_row[name].get("parent")
        return depth(parent) + 1 if parent in by_row else 0

    shaped = []
    for bone in bones:
        name = bone["name"]
        point = Vector(bone["bindPosition"])
        child_names = children.get(name, [])
        if child_names:
            tail = sum((Vector(by_row[child]["bindPosition"])
                        for child in child_names), Vector()) / len(child_names)
        else:
            # The axis exists only in the private preview armature. Runtime
            # binding uses the donor's own complete bind-pose array.
            tail = point + Vector((0.0, 0.05, 0.0))
        if (tail - point).length < 0.001:
            tail = point + Vector((0.0, 0.05, 0.0))
        shaped.append({"name": name, "parent": bone.get("parent", ""),
                       "index": bone["index"], "depth": depth(name),
                       "head": list(point), "tail": list(tail)})

    rig_data = {"source": "Sprint 21 " + family + " donor view",
                "space": SPACE, "renderer": RENDERER,
                "rootBone": ROOT_BONE,
                "bindPoseCount": BONE_COUNT, "bones": shaped}
    return rig_data, {bone["name"]: bone for bone in shaped}


def head(rig, name):
    if name not in rig:
        raise SystemExit("the donor rig is missing " + name)
    return Vector(rig[name]["head"])
