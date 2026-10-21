"""Cleanup 2 Girallon arm/palm/digit union; no Xill consumer.

The first hand's palm ended at the averaged knuckle line while each digit
began as an independently capped tube fully owned by its first finger bone.
The lower arm's offset then rotated about the donor's original wrist. The
new arm through fingertips is one surface, with a web grown into the palm
and smooth Hand-to-digit weights across that web. This prevents a gap from
opening even when the finger and wrist drivers disagree strongly.
"""
from pathlib import Path
import sys

import bpy
from mathutils import Vector

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "sprint21-arthropods"))
from cleanup2_authored_parts import cb, chain_weights


def build(bm, weights, uvs, rig, shape, side, lower, legacy):
    bulk = shape["bulk"] * (shape["lowerScale"] if lower else 1.)
    offset = legacy.lower_offset(shape, side) if lower else Vector()
    point = lambda name: legacy.at(rig, side + "_" + name, offset)
    clavicle, upper = point("Clavicle_01"), point("Up_Arm_01")
    elbow, twist, wrist = point("Forearm_01"), point("Forearm_02"), point("Hand_01")
    root = clavicle.copy()
    root.x = .42 if side == "L" else -.42
    root.z += .36 if lower else .08
    armline = [root, clavicle, upper, upper.lerp(elbow, .5), elbow, twist, wrist]
    names = [side + "_" + n for n in ("Clavicle_01", "Clavicle_01", "Up_Arm_01",
                                      "Up_Arm_01", "Forearm_01", "Forearm_02",
                                      "Hand_01")]
    volume = cb.Volume(.012)
    volume.sweep(armline, [v * bulk for v in (.35, .38, .37, .33, .33, .28, .215)], 22)
    digits = ("Fore_Finger", "Midle_Finger", "Little_Finger")
    knuckles = sum((point(d + "_01") for d in digits), Vector()) / 3.
    palmline = [wrist, wrist.lerp(knuckles, .50), knuckles]
    volume.sweep(palmline, [.22 * bulk, .235 * bulk, .21 * bulk], 22)
    # There is no second wrist or knuckle pivot on the two-armed donor.
    # Letting its upper wrist rotate the offset lower hand swept that hand
    # around the upper wrist and stretched the lower digit web in probes.
    # The simplified lower hand therefore follows its forearm as one
    # authored shape. Upper hands retain all native digit articulation.
    hand_driver = side + ("_Forearm_02" if lower else "_Hand_01")
    if lower:
        names[-1] = hand_driver
    lines = [(armline, names), (palmline, [hand_driver] * 3)]
    tips = []
    for digit in (*digits, "Thumb"):
        bones = [side + "_" + digit + "_%02d" % i for i in (1, 2, 3)]
        p1, p2, p3 = [point(digit + "_%02d" % i) for i in (1, 2, 3)]
        # A root well inside the palm, not a cap at the first knuckle.
        root = wrist.lerp(p1, .42) if digit == "Thumb" else wrist.lerp(knuckles, .50)
        tip = p3 + (p3 - p2).normalized() * .12 * bulk
        line = [root, p1, p2, p3, tip]
        width = .105 * bulk if digit == "Thumb" else .09 * bulk
        volume.sweep(line, [width * 1.10, width, width * .88, width * .76,
                            width * .58], 18)
        lines.append((line, [hand_driver] * 4 if lower else [hand_driver, *bones]))
        tips.append((tip, (tip - p3).normalized(), hand_driver if lower else bones[-1]))
    obj = volume.polygonise("GirallonConnectedArmHand")
    cb.finish_surface(obj, 3100, smooth_iterations=5)
    rows = chain_weights(obj, lines)
    # The existing fur/skin atlas is retained; new arms use its limb tile,
    # and the palm plus distal digits use the dark hand tile.
    made = {}
    lo_x = min(v.co.x for v in obj.data.vertices)
    hi_x = max(v.co.x for v in obj.data.vertices)
    lo_y = min(v.co.y for v in obj.data.vertices)
    hi_y = max(v.co.y for v in obj.data.vertices)
    def vertex_for(index, region):
        if (index, region) in made:
            return made[(index, region)]
        v = obj.data.vertices[index]
        nv = bm.verts.new(v.co)
        made[(index, region)] = nv
        weights[nv] = rows[index]
        u0, v0, u1, v1 = legacy.PRIMATE_ATLAS[region]
        uvs[nv] = (u0 + (u1 - u0) * (.03 + .94 * (v.co.x - lo_x) / (hi_x - lo_x)),
                   v0 + (v1 - v0) * (.03 + .94 * (v.co.y - lo_y) / (hi_y - lo_y)))
        return nv
    for polygon in obj.data.polygons:
        hand_share = sum(value for i in polygon.vertices for name, value in rows[i]
                         if "Finger" in name or "Thumb" in name or "Hand_01" in name) / len(polygon.vertices)
        if lower:
            # A spatial skin boundary is needed when the lower hand's
            # simplified fingers intentionally share the forearm driver.
            centre = sum((obj.data.vertices[i].co for i in polygon.vertices), Vector()) / len(polygon.vertices)
            hand_axis = (knuckles - wrist).normalized()
            hand_share = 1. if (centre - wrist).dot(hand_axis) > -.025 else 0.
        region = "hands" if hand_share >= .48 else "limbs"
        # Split only the atlas seam. Both copies keep identical position,
        # normal and weights, so the seam cannot open under deformation.
        bm.faces.new(tuple(vertex_for(i, region) for i in polygon.vertices))
    mesh = obj.data
    bpy.data.objects.remove(obj, do_unlink=True)
    bpy.data.meshes.remove(mesh)
    for tip, direction, bone in tips:
        # The terminal chain owns both the digit tip and its rooted claw.
        legacy.add_claw(bm, weights, uvs, shape, tip - direction * .025,
                        direction, bone)


def smooth_atlas_seam_normals(payload):
    """UV seam copies have equal weights and one common smooth normal."""
    import base64
    import struct
    data = bytearray(base64.b64decode(payload["data"]))
    count = payload["vertexCount"]
    groups = {}
    for i in range(count):
        point = struct.unpack_from("<3f", data, i * 12)
        groups.setdefault(tuple(round(v, 6) for v in point), []).append(i)
    for ids in groups.values():
        if len(ids) < 2:
            continue
        normal = sum((Vector(struct.unpack_from("<3f", data, count * 12 + i * 12))
                      for i in ids), Vector()).normalized()
        for i in ids:
            struct.pack_into("<3f", data, count * 12 + i * 12, *normal)
    payload["data"] = base64.b64encode(data).decode("ascii")
