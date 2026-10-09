#!/usr/bin/env python3
"""The one native input Sprint 18 takes: the donor bind-pose skeleton.

Nothing else from the game is read. No vertices, no indices, no texture
pixels, no shader source, no animation curve. What arrives is bone names,
their bind positions in the local frame of the donor renderer, and the
parent links between them, exactly as the guarded primate donor census
recorded them read-only.

The rig that comes out of here is the pose the original Ape and Dire Ape
bodies are authored against. It is never redistributed: the capture lives
outside the repository and only its hash is published.
"""
import hashlib
import json
import math
from pathlib import Path

# The census settled these on recorded evidence. They are pinned rather
# than searched for, so a different capture cannot quietly become the rig.
BLUEPRINT = "b98735a1737ae494dbe5cbeca1c7c083"
BLUEPRINT_NAME = "CR10_FerociousTrollGuard"
PREFAB = "0bc98460fca38964aae3af6ad5c655ee"
VIEW = "TrollGuard"
BODY_RENDERER = "Troll_base"
EQUIPMENT_RENDERER = "polySurface1"
ROOT_BONE = "Pelvis"
BODY_BONE_COUNT = 61
EQUIPMENT_BONE_COUNT = 62
VIEW_SCALE = 0.7

TORSO = ("Pelvis", "Spine_01", "Spine_02", "Spine_03", "Neck_01", "Head",
         "Jaw_01", "Stomach_01")
FACE = tuple(side + "_" + part for side in ("L", "R")
             for part in ("Up_lip_01", "Eyebrow_01", "Ear_01"))
ARM = tuple(side + "_" + part for side in ("L", "R") for part in
            ("Clavicle_01", "Up_Arm_01", "Forearm_01", "Forearm_02", "Hand_01",
             "Thumb_01", "Thumb_02", "Thumb_03",
             "Fore_Finger_01", "Fore_Finger_02", "Fore_Finger_03",
             "Midle_Finger_01", "Midle_Finger_02", "Midle_Finger_03",
             "Little_Finger_01", "Little_Finger_02", "Little_Finger_03"))
LEG = tuple(side + "_" + part for side in ("L", "R")
            for part in ("UpLeg_01", "Leg_01", "Foot_01", "Foot_Toe_01"))

# Every bone the original bodies may weight geometry to. Deliberately
# absent, and absent for a stated anatomical reason rather than by
# oversight: Tail_01 and Tail_02, because an ape has no tail, and
# Tongue_01 through Tongue_03, because neither printed routine has a
# tongue attack and an unseen tongue is geometry nobody asked for. A
# weight landing on any of the five would mean the generator had begun
# inventing anatomy, so it is a hard failure rather than a warning.
DRIVERS = TORSO + FACE + ARM + LEG
EXCLUDED = ("Tail_01", "Tail_02", "Tongue_01", "Tongue_02", "Tongue_03")

SCOPE = ("Sprint 18 primate donor census: detached read-only view prefabs only. "
         "No campaign actor is spawned, no save is read or written, no native "
         "asset is modified.")
SPACE = ("renderer-local bind frame; no native vertices, indices, UVs, "
         "textures or animation curves")
SOURCE_MODE = ("detached native prefab; read-only, never instantiated, "
               "activated or modified")
UP_AXIS = 1


def load(path):
    """Decode and hash one census capture. The hash is the provenance."""
    raw = Path(path).read_bytes()
    document = json.loads(raw.decode("utf-8-sig"))
    rig, by_name = decode(document)
    return rig, by_name, hashlib.sha256(raw).hexdigest()


def _row(document):
    rows = document.get("rows")
    if not isinstance(rows, list) or not rows:
        raise SystemExit("the capture carries no surveyed rows")
    found = [row for row in rows if row.get("nativeBlueprint") == BLUEPRINT]
    if len(found) != 1:
        raise SystemExit("the capture does not hold exactly one census-chosen donor")
    row = found[0]
    if (row.get("blueprintName") != BLUEPRINT_NAME or row.get("prefab") != PREFAB or
            row.get("view") != VIEW or row.get("size") != "Large" or
            row.get("meetsMinimumCredibleBindHeight") is not True):
        raise SystemExit("the census-chosen donor row is not the reviewed one")
    # Read-only provenance is recorded per row, where the reader wrote it.
    if row.get("space") != SPACE or row.get("sourceMode") != SOURCE_MODE:
        raise SystemExit("the donor row was not captured read-only and detached")
    scale = row.get("viewScale")
    if (not isinstance(scale, list) or len(scale) != 3 or
            any(abs(float(value) - VIEW_SCALE) > 1e-6 for value in scale)):
        raise SystemExit("the donor view scale is not the surveyed uniform 0.7")
    if row.get("skippedRenderers"):
        raise SystemExit("the donor capture skipped a renderer; its frame is incomplete")
    return row


def decode(document):
    """Validate the capture hard, then shape it for the Blender armature."""
    if document.get("scope") != SCOPE:
        raise SystemExit("not a read-only Sprint 18 census capture")
    if (document.get("capturedPrefabCount") != document.get("surveyedPrefabCount") or
            document.get("surveyedPrefabCount") != document.get("prefabCap") or
            document.get("credibleBindHeightCount") != document.get("prefabCap")):
        raise SystemExit("the capture is not the complete passing census")
    # No primate unit type exists in the installed library, which is why
    # both bodies have to be original geometry. If one ever appears, the
    # donor decision is stale and has to be taken again before authoring.
    if document.get("installedPrimateUnitTypes") != 0:
        raise SystemExit("the installed library now has a primate unit type; "
                         "re-run the donor audit before authoring original bodies")
    row = _row(document)
    skins = row.get("skinnedRenderers")
    if not isinstance(skins, list) or len(skins) != 2 or {
            skin.get("renderer") for skin in skins} != {BODY_RENDERER, EQUIPMENT_RENDERER}:
        raise SystemExit("the donor no longer has exactly its body and equipment skins")
    equipment = next(skin for skin in skins if skin["renderer"] == EQUIPMENT_RENDERER)
    if equipment.get("boneCount") != EQUIPMENT_BONE_COUNT:
        raise SystemExit("the suppressed equipment skin changed shape")
    body = next(skin for skin in skins if skin["renderer"] == BODY_RENDERER)
    bones = body.get("bones")
    if (body.get("rootBone") != ROOT_BONE or body.get("boneCount") != BODY_BONE_COUNT or
            body.get("bindPoseCount") != BODY_BONE_COUNT or
            not isinstance(bones, list) or len(bones) != BODY_BONE_COUNT):
        raise SystemExit("the donor body frame is not the surveyed 61-bone rig")
    by_row = {}
    for bone in bones:
        name, position = bone.get("name"), bone.get("bindPosition")
        if (not name or name in by_row or not isinstance(position, list) or
                len(position) != 3 or
                any(not math.isfinite(float(value)) for value in position)):
            raise SystemExit("an unusable or repeated donor bind frame")
        by_row[name] = bone
    if sorted(bone.get("index", -1) for bone in bones) != list(range(BODY_BONE_COUNT)):
        raise SystemExit("the donor bind indices are not a complete run")
    missing = [name for name in DRIVERS if name not in by_row]
    if missing:
        raise SystemExit("the donor is missing drivers: " + ", ".join(missing))
    stray = [name for name in EXCLUDED if name not in by_row]
    if stray:
        raise SystemExit("the excluded donor branches are no longer where they were")
    if set(by_row) != set(DRIVERS) | set(EXCLUDED):
        raise SystemExit("the donor body frame holds unreviewed bones")

    def depth(name, seen=()):
        if name in seen:
            raise SystemExit("the donor bone graph has a cycle")
        parent = by_row[name].get("parent")
        return depth(parent, seen + (name,)) + 1 if parent in by_row else 0

    shaped = []
    for bone in bones:
        head = [float(value) for value in bone["bindPosition"]]
        children = [[float(value) for value in other["bindPosition"]]
                    for other in bones if other.get("parent") == bone["name"]]
        if children:
            tail = [sum(point[axis] for point in children) / len(children)
                    for axis in range(3)]
        else:
            tail = list(head)
        if math.dist(tail, head) < 1e-3:
            tail = [head[0], head[1] + 0.08, head[2]]
        parent = bone.get("parent")
        shaped.append(dict(name=bone["name"], index=bone["index"],
                           parent=parent if parent in by_row else "",
                           depth=depth(bone["name"]), head=head, tail=tail))
    height = max(bone["head"][UP_AXIS] for bone in shaped) - \
        min(bone["head"][UP_AXIS] for bone in shaped)
    if not 4.0 <= height <= 4.6:
        raise SystemExit("the donor bind height moved outside the surveyed range")
    rig = dict(source=document["scope"], space=SPACE,
               blueprint=BLUEPRINT, prefab=PREFAB, renderer=BODY_RENDERER,
               rootBone=ROOT_BONE, bindPoseCount=BODY_BONE_COUNT,
               viewScale=VIEW_SCALE, bindHeight=height, bones=shaped)
    return rig, {bone["name"]: bone for bone in shaped}
