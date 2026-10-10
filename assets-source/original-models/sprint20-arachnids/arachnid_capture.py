#!/usr/bin/env python3
"""Decode the Sprint 20 arachnid donor census into a bind frame.

The Sprint 14 insects read a capture written by that sprint's own survey.
That file did not survive - its provenance records that the request-local
capture stays outside Git and the package - so Sprint 20 re-measured the rig
and this module reads the census the Sprint 20 survey writes instead.

What it takes from the game is a bind-pose skeleton and nothing else: bone
names, parents and bind positions. No vertex, index, UV, bind matrix, texture
pixel, shader or animation curve is read here, and none is redistributed.

It validates hard before it returns, because a body authored against the
wrong frame is worse than a body that fails to build. The checks are the
measurements the census passed on: one renderer, 51 bones, the two-bone
abdomen chain, the seven-bone pedipalp chains and the complete fourth leg
pair. If the installed rig stops matching, authoring stops rather than
silently fitting geometry to a frame nobody looked at.
"""
import hashlib
import json
import math
from pathlib import Path

from mathutils import Vector

BLUEPRINT = "9e120b5e0ad3c794491c049aa24b9fde"
BLUEPRINT_NAME = "GiantSpiderSummoned"
RENDERER = "Spider"
ROOT_BONE = "Position"
BONE_COUNT = 51
# Two different strings, and conflating them cost a build: the capture
# states its provenance per row, while the axis convention is what the
# shipped mesh declares and is derived from the bind positions - legs
# fanning in +-X, forward at -Y, up at +Z, the same frame every Sprint
# 14 insect mesh on this rig declares.
ROW_SPACE = ("renderer-local bind frame; no native vertices, indices, "
             "UVs, textures or animation curves")
SPACE = "donor renderer local; +X right, +Z up, -Y forward"
SCOPE_PREFIX = "Sprint 20 arachnid donor census"

# The chain a scorpion's metasoma is authored onto. Three bones, not the
# five a tail usually gets: Tail1_M and Tail3_M with UpperTorso between them,
# all of them level at about z 0.7 and running backward in +Y. The tail
# geometry arches up and forward over that level chain, so the whole
# metasoma sways as the abdomen sways rather than articulating segment by
# segment. METASOMA_DRIVEN_BY_A_TWO_BONE_CHAIN names the two tail bones; this
# is the complete driver list.
ABDOMEN_CHAIN = ("Tail1_M", "UpperTorso", "Tail3_M")
TAIL_BONES = ("Tail1_M", "Tail3_M")

# A scorpion's claws are its pedipalps, and this rig has seven bones of them
# a side. That is anatomy rather than substitution and it is the reason the
# chelae need nothing invented.
PEDIPALPS = tuple("pedipalp%d_%s" % (index, side)
                  for side in ("L", "R") for index in (1, 2, 3, 4, 5, 6, 7))

# Eight legs for eight legs. The fourth pair is a complete leg on this rig -
# upper, lower and foot, both sides - where the Sprint 14 ants leave it empty
# and the beetles hang wings on it. This is the first creature in the project
# to weight all four chains a side as legs.
LEGS = tuple("%s_%s%d%s" % (side, part, index, "")
             for side in ("L", "R") for index in (0, 1, 2, 3)
             for part in ("Leg",))
LEG_PARTS = tuple(
    "%s_Leg%d_%s" % (side, index, part)
    for side in ("L", "R") for index in (0, 1, 2, 3)
    for part in ("Upper", "Lower"))
LEG_FEET = tuple("%s_Foot%d" % (side, index)
                 for side in ("L", "R") for index in (0, 1, 2, 3))

TORSO = ("Position", "LowerTorso", "UpperTorso")
CHELICERAE = ("chelicera_L", "chelicera_R")
FEMURS = tuple("femur%d_%s" % (index, side)
               for side in ("L", "R") for index in (1, 2, 3))

# Every bone this project is allowed to weight geometry to. A bone outside
# this set is an unreviewed driver, and the exporter refuses one.
DRIVERS = (TORSO + TAIL_BONES + PEDIPALPS + LEG_PARTS + LEG_FEET +
           CHELICERAE + FEMURS)


def load(path):
    """Decode and hash one census capture. The hash is the provenance."""
    raw = Path(path).read_bytes()
    document = json.loads(raw.decode("utf-8-sig"))
    rig_data, by_name = decode(document)
    return rig_data, by_name, hashlib.sha256(raw).hexdigest()


def _row(document):
    rows = document.get("rows")
    if not isinstance(rows, list) or not rows:
        raise SystemExit("the capture carries no surveyed rows")
    found = [row for row in rows if row.get("nativeBlueprint") == BLUEPRINT]
    if len(found) != 1:
        raise SystemExit("the capture does not hold exactly one anchor donor")
    row = found[0]
    if row.get("blueprintName") != BLUEPRINT_NAME or row.get("isAnchor") is not True:
        raise SystemExit("the anchor row is not the reviewed Giant Spider")
    if row.get("space") != ROW_SPACE:
        raise SystemExit("the donor row was not captured read-only and detached")
    if row.get("skippedRenderers"):
        raise SystemExit("the donor capture skipped a renderer; its frame is incomplete")
    return row


def decode(document):
    """Validate the capture hard, then shape it for the Blender armature."""
    if not str(document.get("scope", "")).startswith(SCOPE_PREFIX):
        raise SystemExit("not a read-only Sprint 20 arachnid census capture")
    # The Sprint 14 conclusion, re-measured. If a scorpion ever appears in
    # the installed library the donor decision is stale and has to be taken
    # again before any geometry is authored against the spider.
    if document.get("installedScorpions"):
        raise SystemExit("the installed library now has a scorpion; re-take "
                         "the donor decision before authoring a body")
    row = _row(document)
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
    # The three measurements the census reported, re-asserted here so the
    # generator cannot run against a frame that quietly changed shape.
    if row.get("abdomenChainBones") != len(TAIL_BONES):
        raise SystemExit("the abdomen chain is no longer two tail bones")
    if row.get("chelaChainBones") != 7:
        raise SystemExit("the pedipalp chain is no longer seven bones")
    if row.get("fourthLegBones") != 6:
        raise SystemExit("the fourth leg pair is no longer a complete leg")

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

    rig_data = {"source": "hidden Sprint 20 giant-spider donor view",
                "space": SPACE, "renderer": RENDERER,
                "rootBone": ROOT_BONE,
                "bindPoseCount": BONE_COUNT, "bones": shaped}
    return rig_data, {bone["name"]: bone for bone in shaped}


def head(rig, name):
    if name not in rig:
        raise SystemExit("the donor rig is missing " + name)
    return Vector(rig[name]["head"])
