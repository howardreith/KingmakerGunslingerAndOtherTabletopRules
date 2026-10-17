"""Low-detail WEIGHTED prototype for the Sprint 25 giant bodies.

`graybox_giant_bodies.py` rebuilds a static shape per state. That answers
"what does the authored silhouette look like" and nothing else: no vertex is
bound to a bone, so no render it produces is evidence about skinning, and
re-deriving a pose by moving points proves only that the script can move
points.

This script runs the same authored bodies through the **binding pipeline the
released generators use** - a donor-measured armature, one vertex group per
donor bone, normalised influences, an Armature modifier, FBX export - and then
moves the creature by **posing bones only**. Every deformation below therefore
comes out of the skin cluster. The base mesh is hashed in every state and the
hash is reported, so a reader can check that claim rather than take it.

What it can and cannot prove:

- It CAN prove the influences exist, are legal for Unity, survive export, and
  deform the surface coherently; and it can measure, per pose, whether the
  skin tears, whether the neck seam stays inside the torso, whether the feet
  stay planted and whether contact parts collide.
- It CANNOT prove native motion. The guarded census captured clip NAMES,
  durations and event times - never curves - so the poses here are authored
  probes named after the donor's clips, not those clips. Native animation
  qualification remains outstanding and nothing here substitutes for it.

The one decision it exists to settle: the donor's seven-joint forward chain is
a TRUNK whose bind pose hangs down. The Elephant's trunk is authored along it
and is bind-faithful by construction. A Brachiosaurus neck must RISE, so the
authored geometry leaves the chain's bind path - and every bone in that chain
then pivots somewhere other than where the skin it drives actually is. Three
Brachiosaurus variants are built and measured so the cost is a number instead
of an opinion:

  bind-faithful            geometry on the chain's own bind path
  authored-upright         the sauropod silhouette, full chain influence
  authored-upright-damped  the same silhouette, chain influence moved onto the
                           Head bone whose head sits at the neck's own base

Run:
  blender --background --python-exit-code 1 --python prototype_giant_bodies.py \
      -- --census <sprint25-donor-census.json> --out <dir> [--albedo-dir <dir>]
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
import struct
import sys

import bpy
from mathutils import Matrix, Quaternion, Vector

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
ROOT = HERE.parent
for folder in (ROOT / "pteranodon", ROOT / "sprint12-quadrupeds", HERE):
    sys.path.insert(0, str(folder))
import generate_pteranodon as shared            # noqa: E402
import graybox_giant_bodies as graybox          # noqa: E402

# The donor this sprint is allowed to measure, pinned by prefab so a census
# from some other creature cannot be bound by accident.
DONOR_KEY = "mastodon"
DONOR_PREFAB = "fe9f25fe62e0f3b49a13910a996470b2"

# Skin tolerances. An edge more than a third longer under pose is a tear
# forming; one under two thirds of its rest length is a pinch. These are
# graybox tolerances for placed primitives, not final-art tolerances.
STRETCH_CEILING = 1.35
STRETCH_FLOOR = 0.65

FOOT_BONES = ("L_Palm", "R_Palm", "L_Foot0", "R_Foot0")


# --------------------------------------------------------------------------
# the donor bind frame, as an armature
# --------------------------------------------------------------------------
def donor_rig(census_path):
    """The donor's complete 40-joint bind frame, converted to Blender Z-up.

    The gates are the released quadruped generator's, unchanged in substance:
    one skinned renderer, boneCount == bindPoseCount == rows, unique names,
    three-component positions. Runtime binding uses the donor's own bind-pose
    array; this armature is the local preview of it.
    """
    raw = Path(census_path).read_bytes()
    doc = json.loads(raw)
    donors = [d for d in doc.get("donors", []) if d.get("key") == DONOR_KEY]
    if len(donors) != 1:
        raise SystemExit("census must carry exactly one " + DONOR_KEY + " donor")
    donor = donors[0]
    if donor.get("prefab") != DONOR_PREFAB:
        raise SystemExit("wrong donor prefab: " + str(donor.get("prefab")))
    renderers = donor.get("skinnedRenderers", [])
    if len(renderers) != 1:
        raise SystemExit("the giant-body donor must expose one skinned renderer")
    source = renderers[0]
    rows = source.get("bones", [])
    if not rows or source.get("boneCount") != source.get("bindPoseCount") or \
            len(rows) != source.get("boneCount"):
        raise SystemExit("donor bind frame is incomplete")
    by_name = {row["name"]: row for row in rows}
    if len(by_name) != len(rows):
        raise SystemExit("donor bind frame repeats a bone name")
    if any(len(row.get("bindPosition", [])) != 3 for row in rows):
        raise SystemExit("donor bind frame has an invalid position")

    # The one conversion this project authors in, identical to
    # graybox_giant_bodies.load_rest: renderer-local bind frame (Y up,
    # -Z forward) to Blender Z-up, which preserves handedness.
    position = {}
    for row in rows:
        x, y, z = row["bindPosition"]
        position[row["name"]] = Vector((x, -z, y))

    children = {}
    for row in rows:
        if row.get("parent") in by_name:
            children.setdefault(row["parent"], []).append(row["name"])

    def depth(name):
        parent = by_name[name].get("parent")
        return depth(parent) + 1 if parent in by_name else 0

    bones = []
    for row in rows:
        name = row["name"]
        head = position[name]
        kids = children.get(name, [])
        if kids:
            tail = sum((position[kid] for kid in kids), Vector()) / len(kids)
        else:
            # A leaf has no child to aim at. Continue the segment it came
            # from, so the trunk tip and the feet keep the chain's own
            # direction instead of a synthetic axis that would curl the tip
            # sideways under pose.
            parent = by_name[name].get("parent")
            if parent in position and (head - position[parent]).length > 1e-5:
                along = head - position[parent]
                tail = head + along.normalized() * (along.length * 0.5)
            else:
                tail = head + Vector((0.0, 0.0, 0.05))
        if (tail - head).length < 1e-4:
            tail = head + Vector((0.0, 0.0, 0.05))
        bones.append({"name": name, "parent": row.get("parent", ""),
                      "index": row["index"], "depth": depth(name),
                      "head": list(head), "tail": list(tail)})
    rig = {"source": donor.get("scope", ""), "space": donor.get("space", ""),
           "renderer": source["renderer"], "rootBone": source["rootBone"],
           "bindPoseCount": source["bindPoseCount"], "bones": bones}
    return rig, {bone["name"]: bone for bone in bones}, \
        hashlib.sha256(raw).hexdigest()


# --------------------------------------------------------------------------
# binding
# --------------------------------------------------------------------------
def damp_forward_chain(keep, host):
    """Move most of the forward chain's influence onto one host bone.

    This is a weighting change, not a geometry change: the silhouette is
    untouched. It exists because the chain's bind path hangs below an authored
    upright neck, so a chain bone rotates the neck about a pivot that is not
    on the neck. `host` is the bone whose bind head IS the neck's base.
    """
    def apply(indexed):
        out = {}
        for index, entries in indexed.items():
            merged = {}
            for name, value in entries:
                if name in graybox.FORWARD_CHAIN:
                    merged[name] = merged.get(name, 0.0) + value * keep
                    merged[host] = merged.get(host, 0.0) + value * (1.0 - keep)
                else:
                    merged[name] = merged.get(name, 0.0) + value
            rows = sorted(merged.items(), key=lambda row: -row[1])
            rows = [row for row in rows if row[1] > 1e-4][:4]
            out[index] = rows
        return out
    return apply


def bind(built, armature, rig_names, weight_filter=None):
    """The released binding pipeline: groups, normalised influences, modifier.

    Every gate here is the one the released generators raise, and each refuses
    rather than repairs: an unweighted or unpainted vertex, a bone the donor
    does not have, an empty influence row, or more than Unity's four
    influences per vertex.
    """
    obj, mesh = built.obj, built.obj.data
    indexed, uv_indexed = dict(built.weights), built.uvs
    if len(indexed) != len(mesh.vertices) or \
            len(uv_indexed) != len(mesh.vertices):
        raise SystemExit("prototype has an unweighted or unpainted vertex")
    if weight_filter is not None:
        indexed = weight_filter(indexed)

    layer = mesh.uv_layers.new(name="Albedo")
    for loop in mesh.loops:
        layer.data[loop.index].uv = uv_indexed[loop.vertex_index]

    groups = {}
    for entries in indexed.values():
        for name, _ in entries:
            if name not in rig_names:
                raise SystemExit("prototype references absent donor bone "
                                 + name)
            if name not in groups:
                groups[name] = obj.vertex_groups.new(name=name)
    for index, entries in indexed.items():
        total = sum(value for _, value in entries)
        if total <= 0 or len(entries) > 4:
            raise SystemExit("invalid influence row at vertex " + str(index))
        for name, value in entries:
            groups[name].add([index], value / total, "REPLACE")

    modifier = obj.modifiers.new(name="Armature", type="ARMATURE")
    modifier.object = armature
    obj.parent = armature
    return indexed, groups


# --------------------------------------------------------------------------
# posing: bones only
# --------------------------------------------------------------------------
def clear_pose(armature):
    for pose_bone in armature.pose.bones:
        pose_bone.matrix_basis = Matrix()
    bpy.context.view_layer.update()


def rotate_in_armature_space(armature, bone, axis, degrees):
    """Rotate one bone by `degrees` about `axis`, about its own head.

    `axis` is given in armature space, and the bone's rest basis converts it
    into the local rotation Blender stores. A parent that is itself posed
    carries the axis with it, which is what a joint chain does and what an
    animation clip would do.
    """
    pose_bone = armature.pose.bones.get(bone)
    if pose_bone is None:
        raise SystemExit("donor has no bone named " + bone)
    turn = Quaternion(Vector(axis).normalized(), math.radians(degrees))
    rest = pose_bone.bone.matrix_local.to_3x3()
    pose_bone.rotation_mode = "QUATERNION"
    pose_bone.rotation_quaternion = (
        rest.inverted() @ turn.to_matrix() @ rest).to_quaternion()


def apply_pose(armature, rotations):
    clear_pose(armature)
    for bone, axis, degrees in rotations:
        rotate_in_armature_space(armature, bone, axis, degrees)
    bpy.context.view_layer.update()


def upright_reaim(rest):
    """The re-aim the authored sauropod neck needs, as ONE bone rotation.

    Identical in construction to graybox_giant_bodies.reaim - axis is
    current x target, angle is acos(current . target) - so the pose and the
    authored geometry are the same swing expressed two ways.
    """
    points = graybox.chain_points(rest, graybox.FORWARD_CHAIN, "Head")
    current = (points[-1] - points[0]).normalized()
    target = (graybox.UP * 0.82 + graybox.FORWARD * 0.57).normalized()
    axis = current.cross(target)
    if axis.length < 1e-6:
        return []
    angle = math.degrees(math.acos(max(-1.0, min(1.0, current.dot(target)))))
    return [("Head", axis.normalized(), angle)]


def pose_library(rest):
    """Authored probe poses, named after the donor's own clips.

    The names are the donor's (`walk`, `gore_attack`, `slam_attack`); the
    amplitudes are this project's, because the census captured no curves. A
    reader must not read these as the native clips.
    """
    chain = ["Head"] + graybox.FORWARD_CHAIN
    lateral = graybox.LATERAL
    curl = [(bone, lateral, 16.0) for bone in chain]
    return {
        "bind": [],
        "neck-upright": upright_reaim(rest),
        # Same probe, two names: it is a neck on one creature and a trunk on
        # the other, and a render filename should say which.
        "chain-curl-probe": curl,
        "trunk-curl": curl,
        # The 16 degree probe is out of tolerance by the generator's own
        # measurement, so a review of it is a review of a failing frame. This
        # is the largest amplitude the sweep passes.
        "trunk-curl-8": [(bone, lateral, 8.0) for bone in chain],
        # Named for what it does and not for what it would need to prove.
        # It rotates joints only: no root motion and no IK, so the stance feet
        # do NOT stay on the ground, and the numbers say by how much. Calling
        # it walk-contact asserted a contact this probe cannot hold.
        "walk-swing": [("L_Arm_Upper", lateral, 22.0),
                         ("L_Arm_Lower", lateral, -12.0),
                         ("R_Arm_Upper", lateral, -14.0),
                         ("L_Leg0_Upper", lateral, -18.0),
                         ("R_Leg0_Upper", lateral, 16.0),
                         ("R_Leg0_Lower", lateral, 10.0)],
        "gore-strike": [("Neck", lateral, -24.0), ("Head", lateral, -16.0)],
        # The donor bends its forelimb at frontKnee_L and frontAnkle_L, which
        # sit BETWEEN L_Arm_Lower and L_Palm in its hierarchy and which no
        # authored ring weights. If a clip bends them, the foot follows and
        # the shin does not, so this pose measures that seam rather than
        # leaving it to be discovered in game.
        "knee-flex": [("frontKnee_L", lateral, 26.0),
                      ("frontAnkle_L", lateral, -14.0)],
        "slam-foreleg": [("L_Arm_Upper", lateral, 52.0),
                         ("L_Arm_Lower", lateral, -34.0)],
    }


# --------------------------------------------------------------------------
# measurement
# --------------------------------------------------------------------------
def evaluated_coords(obj):
    """The skinned result: the mesh as the depsgraph evaluates it."""
    graph = bpy.context.evaluated_depsgraph_get()
    evaluated = obj.evaluated_get(graph)
    mesh = evaluated.to_mesh()
    coords = [vertex.co.copy() for vertex in mesh.vertices]
    evaluated.to_mesh_clear()
    return coords


def base_mesh_digest(mesh):
    """The UNevaluated vertices. Equal across every pose is the proof that
    the motion came from the skin cluster and not from rebuilt geometry."""
    blob = b"".join(struct.pack("<3f", *vertex.co) for vertex in mesh.vertices)
    return hashlib.sha256(blob).hexdigest()


def dominant_bones(indexed):
    return {index: max(entries, key=lambda row: row[1])[0]
            for index, entries in indexed.items()}


def part_bones(built, part, indexed):
    """Which bones drive one named part, heaviest first."""
    totals = {}
    for index in built.parts.get(part, []):
        for name, value in indexed[index]:
            totals[name] = totals.get(name, 0.0) + value
    return [name for name, _ in sorted(totals.items(), key=lambda r: -r[1])]


def rigid_distortion(built, part, rest_coords, posed_coords):
    """How far a part departed from a rigid motion, in absolute units.

    A part driven by one bone must move rigidly, so every vertex keeps its
    distance to the part's own centroid. This is the harness checking itself:
    if a single-bone part distorts, the pose or the skinning path is wrong and
    no other number in this report can be trusted.
    """
    indices = built.parts.get(part, [])
    if not indices:
        return None
    rest_centre = sum((rest_coords[i] for i in indices), Vector()) / len(indices)
    posed_centre = sum((posed_coords[i] for i in indices),
                       Vector()) / len(indices)
    worst = 0.0
    for index in indices:
        before = (rest_coords[index] - rest_centre).length
        after = (posed_coords[index] - posed_centre).length
        worst = max(worst, abs(after - before))
    return round(worst, 6)


def edge_extremes(mesh, rest_coords, posed_coords, dominant, outside=None):
    """How far the skin stretched and pinched, and across which bones.

    `outside` is the set of vertices that lie outside the torso hull. Given
    it, an edge counts when EITHER end is outside, because such an edge crosses
    the surface and part of it is seen. Only edges with both ends buried in the
    torso are dropped, and those are the ones nothing can show.
    """
    worst_high = (1.0, None)
    worst_low = (1.0, None)
    for edge in mesh.edges:
        first, second = edge.vertices
        if outside is not None and first not in outside and \
                second not in outside:
            continue
        rest_length = (rest_coords[first] - rest_coords[second]).length
        if rest_length < 1e-6:
            continue
        ratio = (posed_coords[first] - posed_coords[second]).length / rest_length
        pair = sorted({dominant[first], dominant[second]})
        if ratio > worst_high[0]:
            worst_high = (ratio, pair)
        if ratio < worst_low[0]:
            worst_low = (ratio, pair)
    return worst_high, worst_low


def facet_spacing(mesh, coords, indices):
    """Mean edge length inside one part.

    The seam gap is a distance to the nearest torso vertex, and a distance is
    only meaningful against the spacing of the surface it is measured on: a gap
    well under one facet is a flush join at this resolution, and the island the
    reviewer measured was many facets wide.
    """
    owned = set(indices)
    lengths = [(coords[e.vertices[0]] - coords[e.vertices[1]]).length
               for e in mesh.edges
               if e.vertices[0] in owned and e.vertices[1] in owned]
    return sum(lengths) / max(1, len(lengths))


def ellipsoid_value(point, landmarks):
    """< 1 is inside the torso hull, > 1 is outside it. The radii order is
    the one ellipsoid() takes: lateral, up, forward."""
    offset = point - landmarks["torsoMid"]
    lateral, up, forward = landmarks["bodyRadii"]
    return ((offset.dot(graybox.LATERAL) / lateral) ** 2 +
            (offset.dot(graybox.UP) / up) ** 2 +
            (offset.dot(graybox.FORWARD) / forward) ** 2)


def min_distance(first, second, limit=4000):
    """Closest approach between two vertex sets. Both sets are small at
    graybox resolution; the limit keeps the pairing honest if that changes."""
    if not first or not second:
        return None
    if len(first) * len(second) > limit * limit:
        raise SystemExit("contact sets too large for an exact pairing")
    best = None
    for a in first:
        for b in second:
            gap = (a - b).length
            if best is None or gap < best:
                best = gap
    return best


def measure(mesh, rest_coords, posed_coords, indexed, dominant,
            built, contact_pair):
    """Everything this pose is allowed to claim, as numbers."""
    landmarks = built.landmarks
    displacement = [(posed_coords[i] - rest_coords[i]).length
                    for i in range(len(posed_coords))]
    worst_high, worst_low = edge_extremes(mesh, rest_coords, posed_coords,
                                          dominant)
    # Only edges buried entirely inside the torso are dropped. An edge from a
    # pinned seam ring to the ring outside it crosses the surface, so the tear
    # it can open is visible and has to be counted.
    outside = set(index for index in range(len(posed_coords))
                  if ellipsoid_value(posed_coords[index],
                                     built.landmarks) > 1.0)
    visible_high, visible_low = edge_extremes(mesh, rest_coords, posed_coords,
                                              dominant, outside)
    socket_values = [ellipsoid_value(posed_coords[index], built.landmarks)
                     for index in built.parts.get("sockets", [])]
    anchor_values = [ellipsoid_value(posed_coords[i], landmarks)
                     for i in built.anchorRing]
    # Two readings of the seam the reviewer measured as a free-floating
    # island. The hull value says whether the ring is inside the torso's
    # analytic surface; the distance to the nearest torso vertex is what a
    # gap would actually show as, and no analytic surface is involved in it.
    seam_gap = min_distance([posed_coords[i] for i in built.anchorRing],
                            [posed_coords[i] for i in built.parts["torso"]])
    feet = {}
    for bone in FOOT_BONES:
        owned = [posed_coords[i] for i, name in dominant.items()
                 if name == bone]
        if owned:
            feet[bone] = round(min(point.z for point in owned), 4)
    gap = min_distance(
        [posed_coords[i] for i in built.parts.get(contact_pair[0], [])],
        [posed_coords[i] for i in built.parts.get(contact_pair[1], [])])
    tip = [posed_coords[i] for i, name in dominant.items()
           if name == graybox.FORWARD_CHAIN[-1]]
    return {
        "maxVertexDisplacement": round(max(displacement), 4),
        "meanVertexDisplacement": round(sum(displacement) /
                                        max(1, len(displacement)), 4),
        "movedVertices": sum(1 for value in displacement if value > 1e-5),
        "maxEdgeStretch": round(worst_high[0], 4),
        "maxEdgeStretchBones": worst_high[1],
        "minEdgeStretch": round(worst_low[0], 4),
        "minEdgeStretchBones": worst_low[1],
        "maxVisibleEdgeStretch": round(visible_high[0], 4),
        "maxVisibleEdgeStretchBones": visible_high[1],
        "minVisibleEdgeStretch": round(visible_low[0], 4),
        "minVisibleEdgeStretchBones": visible_low[1],
        "skinWithinTolerance": bool(worst_high[0] <= STRETCH_CEILING and
                                    worst_low[0] >= STRETCH_FLOOR),
        "visibleSkinWithinTolerance": bool(
            visible_high[0] <= STRETCH_CEILING and
            visible_low[0] >= STRETCH_FLOOR),
        "socketRingMaxHullValue": round(max(socket_values), 4)
        if socket_values else None,
        "socketRingsInsideTorso": bool(socket_values and
                                       max(socket_values) < 1.0),
        "anchorRingMaxHullValue": round(max(anchor_values), 4),
        "anchorRingInsideTorso": bool(max(anchor_values) < 1.0),
        "anchorRingInsideFraction": round(
            sum(1 for value in anchor_values if value < 1.0) /
            float(len(anchor_values)), 3),
        "anchorToTorsoMinDistance": round(seam_gap, 4)
        if seam_gap is not None else None,
        "footMinZ": feet,
        "footLiftAboveSolePlane": {
            bone: round(value - landmarks["solePlane"], 4)
            for bone, value in feet.items()},
        "solePlane": round(landmarks["solePlane"], 4),
        "chainTurns": chain_turns(built, posed_coords),
        "forwardToForelimbMinDistance": round(min_distance(
            [posed_coords[i] for i in built.parts.get("forward", [])],
            [posed_coords[i] for i in built.parts.get("forelimbs", [])]), 4)
        if built.parts.get("forelimbs") else None,
        "chainTipMinZ": round(min(point.z for point in tip), 4) if tip else None,
        "contactPair": list(contact_pair),
        "rigidPartDistortion": {
            part: rigid_distortion(built, part, rest_coords, posed_coords)
            for part in ("skull", "tusks")},
        "contactMinDistance": round(gap, 4) if gap is not None else None,
        "contactTouching": bool(gap is not None and gap <= 0.0),
        "baseMeshSha256": base_mesh_digest(mesh),
    }


def chain_turns(built, coords):
    """The angle between consecutive posed segments of the forward chain.

    A per-joint probe should turn the chain by the same angle at every joint.
    Whether it LOOKS that way depends on the segment lengths, which on this
    donor differ by a factor of four, so this reports the turn and the length
    it is spread over - the second is what the eye reads.
    """
    rings = built.parts["forward"]
    size = graybox.FORWARD_SEGMENTS
    centres = [sum((coords[i] for i in rings[start:start + size]), Vector())
               / size
               for start in range(0, len(rings), size)]
    rows = []
    for index in range(1, len(centres) - 1):
        first = centres[index] - centres[index - 1]
        second = centres[index + 1] - centres[index]
        if first.length < 1e-6 or second.length < 1e-6:
            continue
        angle = math.degrees(math.acos(max(-1.0, min(1.0,
                             first.normalized().dot(second.normalized())))))
        rows.append({"joint": index, "turnDegrees": round(angle, 1),
                     "segmentLength": round(second.length, 3),
                     "degreesPerUnit": round(angle / second.length, 1)})
    return rows


def pivot_offsets(indexed, rest_coords, rig_names):
    """Where each forward-chain bone sits relative to the skin it drives.

    This is the number the Brachiosaurus neck question turns on. A bone whose
    bind head is far from its own skin rotates that skin about a pivot that is
    not on it, so a small joint angle becomes a large sweep. The ratio is the
    lever arm in units of the skin's own radius.
    """
    out = {}
    for bone in ["Head"] + graybox.FORWARD_CHAIN:
        owned = [rest_coords[i] for i, entries in indexed.items()
                 if max(entries, key=lambda row: row[1])[0] == bone]
        if not owned or bone not in rig_names:
            continue
        head = Vector(rig_names[bone]["head"])
        centroid = sum(owned, Vector()) / len(owned)
        spread = sum((point - centroid).length for point in owned) / len(owned)
        out[bone] = {
            "vertices": len(owned),
            "boneHeadToSkinCentroid": round((centroid - head).length, 4),
            "skinRadius": round(spread, 4),
            "leverArmRatio": round((centroid - head).length / spread, 3)
            if spread > 1e-6 else None,
        }
    return out


# --------------------------------------------------------------------------
# scene, materials, renders
# --------------------------------------------------------------------------
def setup_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    camera_data = bpy.data.cameras.new("ReviewCamera")
    camera = bpy.data.objects.new("ReviewCamera", camera_data)
    scene.collection.objects.link(camera)
    scene.camera = camera
    camera_data.type = "ORTHO"
    scene.render.resolution_x = 1000
    scene.render.resolution_y = 720
    scene.render.image_settings.file_format = "PNG"
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    scene.render.film_transparent = False
    world = bpy.data.worlds.new("ReviewWorld")
    scene.world = world
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[1].default_value = 1.6
    # Three suns, the graybox lighting, so a front or rear view is readable.
    for name, pitch, yaw, energy in (("Key", 52, 35, 3.0),
                                     ("Fill", 60, 200, 1.9),
                                     ("Side", 70, 290, 1.4)):
        light_data = bpy.data.lights.new(name, type="SUN")
        light_data.energy = energy
        light = bpy.data.objects.new(name, light_data)
        light.rotation_euler = (math.radians(pitch), 0.0, math.radians(yaw))
        scene.collection.objects.link(light)
    # A ground plane. It is created at z = 0 and moved to the donor's own
    # sole plane once the body is built, so "planted" is checkable against the
    # plane the soles were flattened onto rather than against a convention.
    # At this camera elevation it reads as a horizon line rather than a floor
    # that hides the legs.
    ground = bpy.data.meshes.new("Ground")
    ground.from_pydata([(-60.0, -60.0, 0.0), (60.0, -60.0, 0.0),
                        (60.0, 60.0, 0.0), (-60.0, 60.0, 0.0)],
                       [], [(0, 1, 2, 3)])
    ground.update()
    surface = bpy.data.materials.new("GroundPlane")
    surface.use_nodes = True
    surface.node_tree.nodes["Principled BSDF"].inputs["Base Color"]        .default_value = (0.30, 0.30, 0.32, 1.0)
    ground.materials.append(surface)
    scene.collection.objects.link(bpy.data.objects.new("Ground", ground))
    return scene, camera, camera_data


def place_ground(landmarks):
    """Put the review ground on the plane the soles were flattened onto."""
    ground = bpy.data.objects.get("Ground")
    if ground is not None:
        ground.location.z = landmarks["solePlane"]


def bone_colour(name):
    """A stable colour per bone name, so the same bone is the same colour in
    every weight render of every variant."""
    digest = hashlib.sha256(name.encode("utf-8")).digest()
    sector, fraction = divmod(digest[0] / 255.0 * 6.0, 1.0)
    high, low = 0.95, 0.18
    rising = low + (high - low) * fraction
    falling = high - (high - low) * fraction
    table = [(high, rising, low), (falling, high, low), (low, high, rising),
             (low, falling, high), (rising, low, high), (high, low, falling)]
    red, green, blue = table[int(sector) % 6]
    return (red, green, blue, 1.0)


def weight_material(obj, indexed):
    """Paint each vertex with its dominant bone's colour, as real vertex
    colours on the bound mesh. A reader can see which bone owns which surface
    instead of taking the influence table on trust."""
    mesh = obj.data
    attribute = mesh.color_attributes.new(name="DominantBone",
                                          type="FLOAT_COLOR", domain="POINT")
    for index, entries in indexed.items():
        attribute.data[index].color = bone_colour(
            max(entries, key=lambda row: row[1])[0])
    material = bpy.data.materials.new("DominantBoneWeights")
    material.use_nodes = True
    tree = material.node_tree
    for node in list(tree.nodes):
        tree.nodes.remove(node)
    colour = tree.nodes.new("ShaderNodeVertexColor")
    colour.layer_name = "DominantBone"
    emission = tree.nodes.new("ShaderNodeEmission")
    output = tree.nodes.new("ShaderNodeOutputMaterial")
    tree.links.new(colour.outputs["Color"], emission.inputs["Color"])
    tree.links.new(emission.outputs[0], output.inputs["Surface"])
    return material


SIDE = ("side", Vector((1.0, 0.0, 0.12)))
FRONT = ("front", Vector((0.0, 1.0, 0.12)))
PARTY = ("party", Vector((0.72, 0.72, 0.55)))


def render_views(scene, camera, camera_data, coords, out, stem, views):
    """Frame the POSED body, so a pose that moves cannot leave the frame."""
    centre = sum(coords, Vector()) / len(coords)
    extent = max(max(point[axis] for point in coords) -
                 min(point[axis] for point in coords) for axis in range(3))
    written = []
    for view, offset in views:
        camera.location = centre + offset.normalized() * extent * 4.0
        graybox.point_camera(camera, centre)
        # 1.35 cropped the feet off every tall pose: the frame is 1000x720,
        # so the vertical field is only 0.72 of the orthographic scale.
        camera_data.ortho_scale = extent * 1.9
        name = stem + "-" + view + ".png"
        scene.render.filepath = str(out / name)
        bpy.ops.render.render(write_still=True)
        written.append(name)
    return written


# --------------------------------------------------------------------------
# one variant, end to end
# --------------------------------------------------------------------------
VARIANTS = (
    # name, creature, authored state, weight filter, poses, contact pair,
    # seam treatment, and whether to add the weight and textured passes.
    #
    # The first four are one question asked four ways: a Brachiosaurus neck
    # has to rise, the donor chain that would drive it hangs down, and these
    # measure what each answer costs. The last three are the seam treatment,
    # measured on the creature whose geometry is already bind-faithful so
    # nothing else is moving.
    ("brachiosaurus-bind-faithful", "brachiosaurus", "rest", None,
     ("bind", "neck-upright", "chain-curl-probe", "walk-swing",
      "knee-flex"),
     ("skull", "torso"), "mixed", True),
    ("brachiosaurus-authored-upright", "brachiosaurus", "upright", None,
     ("bind", "chain-curl-probe", "gore-strike", "walk-swing",
      "knee-flex"),
     ("skull", "torso"), "mixed", True),
    ("brachiosaurus-authored-upright-damped", "brachiosaurus", "upright",
     ("damp", 0.25, "Head"),
     ("bind", "chain-curl-probe", "gore-strike"),
     ("skull", "torso"), "mixed", False),
    # Chain influence removed entirely: the authored neck becomes one rigid
    # piece driven by the Head bone, whose bind head IS the neck's base. It
    # gives up the chain's secondary motion, and it is the only weighting of
    # the three that can be correct about its pivot.
    ("brachiosaurus-authored-upright-rigid", "brachiosaurus", "upright",
     ("damp", 0.0, "Head"),
     ("bind", "chain-curl-probe", "gore-strike", "walk-swing", "knee-flex"),
     ("skull", "torso"), "mixed", True),
    ("elephant-bind-faithful", "elephant", "rest", None,
     ("bind", "trunk-curl-8", "trunk-curl", "gore-strike", "slam-foreleg",
      "walk-swing", "knee-flex"),
     ("tusks", "forward"), "mixed", True),
    # The same Elephant under the two seam treatments the graded one replaces,
    # so the trade is a measured triple rather than an assertion.
    ("elephant-seam-pinned", "elephant", "rest", None,
     ("bind", "slam-foreleg", "gore-strike"),
     ("tusks", "forward"), "pin", False),
    ("elephant-seam-blended", "elephant", "rest", None,
     ("bind", "slam-foreleg", "gore-strike"),
     ("tusks", "forward"), "blend", False),
    ("elephant-seam-graded", "elephant", "rest", None,
     ("bind", "slam-foreleg", "gore-strike"),
     ("tusks", "forward"), "graded", False),
    ("elephant-seam-chained", "elephant", "rest", None,
     ("bind", "slam-foreleg", "gore-strike"),
     ("tusks", "forward"), "chained", False),
)


def run_variant(spec, census, rig, rig_names, rig_hash, out, albedo_dir):
    name, kind, state, filter_spec, poses, contact, seam, extras = spec
    scene, camera, camera_data = setup_scene()
    rest = graybox.load_rest(census)
    armature = shared.build_armature(rig)
    armature.name = "GiantBodyDonorRig"
    built = graybox.build(kind, rest, state, seam=seam)
    place_ground(built.landmarks)
    obj = built.obj
    obj.name = name

    weight_filter = None
    if filter_spec is not None:
        weight_filter = damp_forward_chain(filter_spec[1], filter_spec[2])
    indexed, groups = bind(built, armature, rig_names, weight_filter)

    dominant = dominant_bones(indexed)
    library = pose_library(rest)
    mesh = obj.data
    clear_pose(armature)
    rest_coords = evaluated_coords(obj)

    record = {
        "variant": name,
        "creature": kind,
        "authoredState": state,
        "seamTreatment": seam,
        "weighting": ("full donor chain influence" if filter_spec is None else
                      "forward-chain influence damped to %.2f, remainder on %s"
                      % (filter_spec[1], filter_spec[2])),
        "donor": {"key": DONOR_KEY, "prefab": DONOR_PREFAB,
                  "renderer": rig["renderer"], "rootBone": rig["rootBone"],
                  "bindPoseCount": rig["bindPoseCount"],
                  "censusSha256": rig_hash},
        "mesh": {
            "vertices": len(mesh.vertices),
            "polygons": len(mesh.polygons),
            "boneGroups": len(obj.vertex_groups),
            "maxInfluencesPerVertex": max(len(e) for e in indexed.values()),
            "weightedVertices": len(indexed),
            "donorBonesWithoutInfluence":
                len(rig_names) - len(obj.vertex_groups),
            "bindPoseDigest": base_mesh_digest(mesh),
        },
        "topology": graybox.topology_report(obj),
        "torsoFacetSpacing": round(facet_spacing(mesh, rest_coords,
                                                 built.parts["torso"]), 4),
        "partBones": {part: part_bones(built, part, indexed)
                      for part in sorted(built.parts)},
        "forwardChainPivots": pivot_offsets(indexed, rest_coords, rig_names),
        "poses": {},
        "renders": [],
    }

    for pose in poses:
        if pose not in library:
            raise SystemExit("no authored pose named " + pose)
        apply_pose(armature, library[pose])
        coords = evaluated_coords(obj)
        measured = measure(mesh, rest_coords, coords, indexed, dominant,
                           built, contact)
        measured["posedBones"] = [bone for bone, _, _ in library[pose]]
        measured["totalChainAngleDegrees"] = round(
            sum(abs(degrees) for bone, _, degrees in library[pose]
                if bone == "Head" or bone in graybox.FORWARD_CHAIN), 1)
        record["poses"][pose] = measured
        views = [SIDE, PARTY] + ([FRONT] if pose == "bind" else [])
        record["renders"] += render_views(scene, camera, camera_data, coords,
                                          out, name + "-" + pose, views)

    # How much per-joint curl the authored skin takes before it tears. The
    # graybox swept this geometrically, for silhouette; this sweeps it through
    # the skin cluster, which is a different question with a different answer,
    # and it is measurement only - no render, no clip.
    sweep = []
    chain = ["Head"] + graybox.FORWARD_CHAIN
    for degrees in (4.0, 8.0, 12.0, 16.0, 24.0):
        apply_pose(armature, [(bone, graybox.LATERAL, degrees)
                              for bone in chain])
        coords = evaluated_coords(obj)
        high, low = edge_extremes(mesh, rest_coords, coords, dominant)
        outside = set(index for index in range(len(coords))
                      if ellipsoid_value(coords[index],
                                         built.landmarks) > 1.0)
        visible = edge_extremes(mesh, rest_coords, coords, dominant,
                                outside)[0]
        sweep.append({
            "degreesPerJoint": degrees,
            "totalDegrees": round(degrees * len(chain), 1),
            "maxEdgeStretch": round(high[0], 4),
            "maxVisibleEdgeStretch": round(visible[0], 4),
            "minEdgeStretch": round(low[0], 4),
            "worstJoint": high[1],
            "maxVertexDisplacement": round(
                max((coords[i] - rest_coords[i]).length
                    for i in range(len(coords))), 4),
            "withinTolerance": bool(high[0] <= STRETCH_CEILING and
                                    low[0] >= STRETCH_FLOOR),
        })
    record["forwardChainCurlSweep"] = sweep

    # Two more passes on the bind pose: who owns which surface, and the
    # painted albedo on the real UVs. The comparison variants skip them,
    # because they exist for their numbers and a packet of near-identical
    # bind renders is harder to review, not easier.
    apply_pose(armature, library["bind"])
    if extras:
        mesh.materials.clear()
        mesh.materials.append(weight_material(obj, indexed))
        record["renders"] += render_views(scene, camera, camera_data,
                                          rest_coords, out, name + "-weights",
                                          [SIDE, PARTY])
    if albedo_dir and extras:
        albedo = Path(albedo_dir) / (kind + "-albedo.png")
        if not albedo.is_file():
            raise SystemExit("missing albedo " + str(albedo))
        mesh.materials.clear()
        shared.attach_preview_material(obj, mesh, str(albedo))
        record["albedo"] = shared.albedo_manifest(str(albedo))
        record["renders"] += render_views(scene, camera, camera_data,
                                          rest_coords, out,
                                          name + "-textured", [SIDE, PARTY])

    # Export through the shipping path, so the binding is proved to survive
    # it rather than only to exist in this session's memory.
    fbx = out / (name + ".fbx")
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.export_scene.fbx(filepath=str(fbx), use_selection=True,
                             add_leaf_bones=False, bake_anim=False,
                             path_mode="COPY",
                             apply_scale_options="FBX_SCALE_ALL",
                             object_types={"ARMATURE", "MESH"})
    data = fbx.read_bytes()
    record["export"] = {"file": fbx.name, "bytes": len(data),
                        "sha256": hashlib.sha256(data).hexdigest()}
    return record


def verify_export(path):
    """Re-import the FBX and count what came back. An influence table that
    does not survive export is not a binding, however good the render looked."""
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=str(path))
    meshes = [o for o in bpy.data.objects if o.type == "MESH"]
    armatures = [o for o in bpy.data.objects if o.type == "ARMATURE"]
    if len(meshes) != 1 or len(armatures) != 1:
        raise SystemExit("re-imported " + path.name + " is not one skinned mesh")
    obj, armature = meshes[0], armatures[0]
    influences = [len([g for g in v.groups if g.weight > 1e-4])
                  for v in obj.data.vertices]
    totals = [sum(g.weight for g in v.groups) for v in obj.data.vertices]
    return {
        "file": path.name,
        "vertices": len(obj.data.vertices),
        "polygons": len(obj.data.polygons),
        "bones": len(armature.data.bones),
        "vertexGroups": len(obj.vertex_groups),
        "hasArmatureModifier": any(m.type == "ARMATURE" for m in obj.modifiers),
        "unweightedVertices": sum(1 for count in influences if count == 0),
        "maxInfluencesPerVertex": max(influences),
        "minWeightSum": round(min(totals), 4),
        "maxWeightSum": round(max(totals), 4),
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--census", required=True)
    parser.add_argument("--out", required=True)
    parser.add_argument("--albedo-dir", default=None)
    parser.add_argument("--variant", default="all")
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    args = parser.parse_args(argv)

    out = Path(args.out)
    out.mkdir(parents=True, exist_ok=True)
    rig, rig_names, rig_hash = donor_rig(args.census)

    selected = [spec for spec in VARIANTS if args.variant in ("all", spec[0])]
    if not selected:
        raise SystemExit("no variant named " + args.variant)

    report = {
        "schemaVersion": 1,
        "what": "low-detail weighted prototype; skinning and authored probe "
                "poses only, no native animation evidence",
        "generator": Path(__file__).name,
        "censusSha256": rig_hash,
        "donorBoneCount": len(rig["bones"]),
        "skinTolerances": {"stretchCeiling": STRETCH_CEILING,
                           "stretchFloor": STRETCH_FLOOR},
        "variants": [],
        "exportVerification": [],
    }
    for spec in selected:
        report["variants"].append(
            run_variant(spec, args.census, rig, rig_names, rig_hash, out,
                        args.albedo_dir))
    for record in report["variants"]:
        report["exportVerification"].append(
            verify_export(out / record["export"]["file"]))

    path = out / "prototype-report.json"
    path.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print("WEIGHTED PROTOTYPE REPORT: " + str(path))
    for record in report["variants"]:
        for pose, values in record["poses"].items():
            print("  %-38s %-16s disp=%7.3f all=%.2f/%.2f "
                  "visible=%.2f/%.2f seam=%.3f socketHull=%s"
                  % (record["variant"], pose,
                     values["maxVertexDisplacement"],
                     values["maxEdgeStretch"], values["minEdgeStretch"],
                     values["maxVisibleEdgeStretch"],
                     values["minVisibleEdgeStretch"],
                     values["anchorToTorsoMinDistance"],
                     values["socketRingMaxHullValue"]))


if __name__ == "__main__":
    main()
