#!/usr/bin/env python3
"""Original Lillend prototype on a CLOSED human anatomical subset.

Native input is the existing private bind-only capture, never geometry,
pixels or animation curves. Ten extra drivers and their tail sweep are
project-authored; the tail motion reuses the accepted Salamander recipe.
No native animation curves or new retargeting/limb framework.
Exported original geometry and pixels are separate from native qualification.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
import sys

from mathutils import Matrix, Vector

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "sprint17-serpents"))
import generate_snakes as common

NATIVE = ("Pelvis", "Spine_1", "Spine_2", "Spine_3", "Neck", "Head") + tuple(
    side + "_" + part for side in ("L", "R") for part in
    ("Clavicle", "Up_arm", "ForeArm", "Hand",
     "Toe_1_01", "Toe_1_02", "Toe_2_01", "Toe_2_02", "Toe_3_01", "Toe_3_02"))
TAIL = tuple("KMG_LillendTail%02d" % index for index in range(10))
BONES = NATIVE + TAIL
SIDE, UP, FORWARD = Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1))
# Original authored rest shape in renderer/view axes; never native bone data.
REST = ((0, .29, 0), (-.45, .27, -.40), (-.78, .25, -.95),
        (-.50, .23, -1.52), (.15, .22, -1.60), (.72, .21, -1.27),
        (.82, .19, -.66), (.46, .17, -.30), (.20, .155, -.67),
        (.22, .14, -1.05), (.52, .13, -1.12))
TIMES = (0, .15, .30, .45, .55, .60, .70, .80, 1.0, 1.2, 1.4)
DURATION, ACT_TIME = 1.4, .60
# Reuse the original Salamander close-melee S sweep. Its historical spear
# contact observations do not establish Lillend contact; this body needs its
# own native sword/tail observation. Keep the proximal coil and sweep forward.
# These fixed authored directions take no live target, reach or damage input.
STRIKE_DIRECTIONS = ((-.85, .10, -.30), (-.30, 0, .95), (.90, .10, .40),
                     (.90, .12, -.40), (.50, .10, .85), (-.95, 0, .40),
                     (-.30, 0, .95), (.10, -.10, .99), (.10, -.10, .99),
                     (-.15, -.10, .98))


def tail_pose(time):
    """One fixed original sweep, no target distance/position or native curve.

    Ten constant-length segments uncoil into a forward tail slap and recover.
    The contact plateau is authored, not a runtime collision/contact clamp.
    """
    if not math.isfinite(time) or not 0 <= time <= DURATION:
        raise ValueError("outside the original tail clip")
    if time <= .15:
        phase = 0
    elif time < .55:
        phase = (time - .15) / .40
    elif time <= .80:
        phase = 1
    else:
        phase = (DURATION - time) / .60
    blend = phase * phase * (3 - 2 * phase)
    points = [Vector(REST[0])]
    for a, b, authored in zip(REST, REST[1:], STRIKE_DIRECTIONS):
        delta = Vector(b) - Vector(a)
        goal = Vector(authored).normalized()
        # Interpolate the original yaw and elevation separately. A shortest
        # quaternion arc between nearly opposite directions can dip beneath
        # the ground during wind-up even when both endpoints are above it.
        yaw = math.atan2(delta.x, delta.z)
        turn = (math.atan2(goal.x, goal.z) - yaw + math.pi) % (2 * math.pi) - math.pi
        pitch = math.atan2(delta.y, math.hypot(delta.x, delta.z))
        pitch += (math.atan2(goal.y, math.hypot(goal.x, goal.z)) - pitch) * blend
        yaw += turn * blend
        direction = Vector((math.sin(yaw) * math.cos(pitch), math.sin(pitch),
                            math.cos(yaw) * math.cos(pitch))) * delta.length
        points.append(points[-1] + direction)
    return points


def pose_original_tail(armature, time):
    """Offline review only; transform original drivers, never a native bone."""
    points = tail_pose(time)
    for index, name in enumerate(TAIL):
        source = armature.data.bones[name].matrix_local
        rotation = (Vector(REST[index + 1]) - Vector(REST[index])).rotation_difference(
            points[index + 1] - points[index])
        armature.pose.bones[name].matrix = (Matrix.Translation(points[index]) @
            rotation.to_matrix().to_4x4() @ Matrix.Translation(-Vector(REST[index])) @ source)
        common.bpy.context.view_layer.update()


def decode_rig(capture):
    if (capture.get("blueprint") != "86dc43534645e234eb35431131e3b669" or
            capture.get("prefab") != "ced3729f4b4abab4da4ef63d8489f857" or
            capture.get("bodyRenderer") != "Renderer_Character_Diffuse_Cutout" or
            capture.get("rootBone") != "Pelvis" or capture.get("paletteCount") != 1776 or
            capture.get("uniqueTransformCount") != 177):
        raise ValueError("wrong exact human bind-only capture")
    rows = capture.get("bones", [])
    if len(rows) != 177 or len({row.get("name") for row in rows}) != 177:
        raise ValueError("incomplete or ambiguous native Transform groups")
    lookup = {row["name"]: row for row in rows}
    if set(NATIVE) - set(lookup):
        raise ValueError("missing closed anatomical driver")
    slots = [slot for row in rows for slot in row.get("paletteSlots", [])]
    if sorted(slots) != list(range(1776)):
        raise ValueError("every original palette slot must be accounted for")
    result = []
    for index, name in enumerate(NATIVE):
        row = lookup[name]
        position = row.get("firstBindPosition", [])
        if (row.get("finite") is not True or row.get("invertible") is not True or
                row.get("derivedFrameFinite") is not True or
                not isinstance(row.get("maximumBindDifference"), (int, float)) or
                not math.isfinite(row["maximumBindDifference"]) or
                not 0 <= row["maximumBindDifference"] <= .00001 or
                len(position) != 3 or any(not math.isfinite(value) for value in position) or
                row.get("firstPaletteSlot") != row["paletteSlots"][0]):
            raise ValueError("unusable or disagreeing anatomical bind: " + name)
        expected = ("Position" if name == "Pelvis" else
                    "Pelvis" if name == "Spine_1" else
                    "Spine_1" if name == "Spine_2" else
                    "Spine_2" if name == "Spine_3" else
                    "Spine_3" if name == "Neck" else
                    "Neck" if name == "Head" else
                    "Spine_3" if name.endswith("Clavicle") else
                    name[0] + "_Clavicle" if name.endswith("Up_arm") else
                    name[0] + "_Up_arm" if name.endswith("ForeArm") else
                    name[0] + "_ForeArm" if name.endswith("Hand") else
                    name[0] + "_Hand" if name.endswith("_01") else name[:-1] + "1")
        if row.get("parent") != expected:
            raise ValueError("anatomical parent differs: " + name)
        children = [r for r in rows if r.get("parent") == name and r["name"] in NATIVE]
        tip = Vector(children[0]["firstBindPosition"]) if children else Vector(position) + UP * .04
        if (tip - Vector(position)).length < .001:
            tip = Vector(position) + UP * .04
        result.append(dict(name=name, parent=expected if expected in NATIVE else "",
                           index=index, head=list(position), tail=list(tip)))
    # Only the new ten-driver chain is authored; no native transform is added,
    # parented, renamed, rotated or otherwise changed by this offline recipe.
    for index, name in enumerate(TAIL):
        result.append(dict(name=name, parent=TAIL[index - 1] if index else "",
                           index=len(NATIVE) + index, head=list(REST[index]),
                           tail=list(REST[index + 1])))
    by_name = {row["name"]: row for row in result}
    def depth(name):
        parent = by_name[name]["parent"]
        return depth(parent) + 1 if parent else 0
    for row in result:
        row["depth"] = depth(row["name"])
    return dict(bones=result, source="exact-private-human-anatomy-plus-original-tail",
                space="renderer-local +X lateral +Y up +Z forward"), by_name


def measured_rig(path):
    raw = Path(path).read_bytes()
    rig, by_name = decode_rig(json.loads(raw))
    return rig, by_name, hashlib.sha256(raw).hexdigest()


def dorsal(point):
    upright = max(0, min(1, (point.y - .65) / .4))
    return UP * (1 - upright) - FORWARD * upright


def build_body(bm, weights, uvs, rig, kind):
    if kind != "lillend-azata":
        raise ValueError("private Lillend body only")
    at = lambda name: common.shared.head(rig, name)
    def mass(centre, radii, bone, region="limbs"):
        common.ellipsoid(bm, weights, uvs, centre, SIDE, UP, FORWARD,
                         radii, bone, region, 8, 16)
    # Original continuous serpent coil joins the covered humanoid waist.
    points = list(reversed([Vector(p) for p in REST])) + [Vector((0, .66, 0)), at("Pelvis")]
    names = list(reversed(list(TAIL) + [TAIL[-1]])) + ["Pelvis", "Pelvis"]
    widths = list(reversed([.24,.25,.24,.22,.19,.16,.13,.10,.075,.045,.008])) + [.22,.22]
    heights = list(reversed([.25,.23,.21,.19,.17,.15,.12,.09,.065,.035,.008])) + [.18,.18]
    common.sweep(bm, weights, uvs, points, widths, heights, names,
                 up=FORWARD, forward=UP, uv_dorsal_at=dorsal)
    torso = [at(n) for n in NATIVE[:5]]
    common.sweep(bm, weights, uvs, torso, [.23,.18,.235,.25,.09],
                 [.16,.13,.145,.15,.08], list(NATIVE[:5]), region="beak",
                 up=FORWARD,forward=UP)
    # Original elven face, ears, hair and robe; no borrowed face geometry.
    mass(at("Neck") + UP*.035, (.075,.10,.075), "Neck")
    head = at("Head")
    mass(head + UP*.07 + FORWARD*.035, (.105,.15,.10), "Head")
    mass(head + UP*.035 + FORWARD*.125, (.020,.045,.028), "Head")
    mass(head + UP*.15 - FORWARD*.035, (.118,.10,.105), "Head", "crest")
    for side,sign in (("L",-1),("R",1)):
        ear = head + SIDE*sign*.10 + UP*.09
        tip = ear + SIDE*sign*.115 + UP*.075 - FORWARD*.035
        common.tube(bm,weights,uvs,[ear,ear.lerp(tip,.55),tip],[.040,.030,.004],["Head"]*3,"limbs",8)
        mass(head + SIDE*sign*.047 + UP*.085 + FORWARD*.126,(.016,.010,.010),"Head","crest")
        common.tube(bm,weights,uvs,[head+SIDE*sign*.10+UP*.16,
            head+SIDE*sign*.13-UP*.03-FORWARD*.03,
            at("Spine_3")+SIDE*sign*.15-FORWARD*.13],
            [.060,.067,.022],["Head","Head","Spine_3"],"crest",12)
        arm=[side+"_"+p for p in ("Clavicle","Up_arm","ForeArm","Hand")]
        common.tube(bm,weights,uvs,[at(n) for n in arm],[.065,.075,.054,.040],arm,"limbs",12)
        palm=at(side+"_Hand")
        fingers=[at(side+"_Toe_%d_01"%d) for d in (1,2,3)]
        mass((sum(fingers,Vector())/3+palm)/2,(.048,.052,.043),side+"_Hand")
        for digit in (1,2,3):
            first,second=(side+"_Toe_%d_%02d"%(digit,j) for j in (1,2))
            direction=(at(second)-at(first)).normalized()
            common.tube(bm,weights,uvs,[palm,at(first),at(second),at(second)+direction*.025],
                [.018,.018,.013,.004],[side+"_Hand",first,second,second],"limbs",8)
        # Two feathered wings, folded outward. They follow the native torso;
        # flight/navigation remains the accepted ordinary-map adaptation.
        root=at("Spine_3")+SIDE*sign*.15-FORWARD*.15
        elbow=root+SIDE*sign*.42+UP*.43-FORWARD*.15
        wrist=root+SIDE*sign*.72+UP*.68-FORWARD*.20
        common.tube(bm,weights,uvs,[root,elbow,wrist],[.11,.13,.085],["Spine_3"]*3,"membrane",12)
        for index in range(13):
            t=index/12
            base=root.lerp(wrist,t)
            tip=base+SIDE*sign*(.38+.20*t)-UP*(.50-.12*t)-FORWARD*(.09+.12*t)
            axis=(tip-base).normalized()
            normal=FORWARD
            lateral=axis.cross(normal).normalized()
            common.ellipsoid(bm,weights,uvs,(base+tip)/2,lateral,axis,normal,
                (.072,(tip-base).length*.55,.025),"Spine_3","membrane",6,12)


# Reuse the proved triangle operation without changing any released exporter.
# Native acceptance of this new body remains separate.
_finish_serpent = common.finish_original_winding
def finish_lillend(payload,kind):
    if kind != "lillend-azata": raise ValueError("private Lillend winding only")
    result=_finish_serpent(payload,"salamander")
    result["triangleWinding"]="authored-outward-sprint32"
    return result
common.finish_original_winding=finish_lillend


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    parser.add_argument("--preview-time", type=float, default=0)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    args.kind = "lillend-azata"
    rig_data, rig, rig_hash = measured_rig(args.capture)
    common.write_prototype(args, rig_data, rig, rig_hash, build_body, BONES,
        "exact-human-anatomical-subset-plus-original-tail",
        "renderer-local +X lateral +Y up +Z forward",
        dict(visibleArms=2, visibleWings=2, weaponIncluded=False, weaponHandlingQualified=False,
             nativeAnatomicalDrivers=len(NATIVE), originalTailDrivers=len(TAIL),
             jawSeparated=False, jawArticulated=False, tailAnimationQualified=False))
    path = Path(args.mesh_data)
    payload = json.loads(path.read_text(encoding="utf-8"))
    # Authored points only. The captured native frames are NOT exported.
    payload["originalTailRest"] = [list(point) for point in REST]
    payload["originalTailSlap"] = dict(duration=DURATION, actTime=ACT_TIME,
        frames=[dict(time=time, points=[list(point) for point in tail_pose(time)]) for time in TIMES],
        source="project-authored fixed sweep; no native curve or target input")
    payload["jawArticulated"] = False
    payload["jawSeparated"] = False
    path.write_text(json.dumps(payload, indent=1) + "\n", encoding="utf-8", newline="\n")
    if args.preview_time:
        armature = next(obj for obj in common.bpy.context.scene.objects if obj.type == "ARMATURE")
        pose_original_tail(armature, args.preview_time)
        common.bpy.ops.wm.save_as_mainfile(filepath=str(Path(args.blend_out).resolve()))


if __name__ == "__main__":
    main()
