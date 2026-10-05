#!/usr/bin/env python3
"""Original crocodilian bodies on the privately measured Monitor Lizard rig.

Only the bind frame is an input from the game. Geometry, topology, UVs,
paintings and synthetic review poses are project-authored. The private blend
and FBX contain measured bone positions and are never redistribution assets.
Runtime JSON contains original geometry/UV/weights and bone names, no bind
matrices or native vertices. All four legs, jaw and seven tail joints remain
functional; no extra limb, animator or movement system is introduced.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
import sys

import bpy
import bmesh
from mathutils import Vector

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve()
for family in ("pteranodon", "sprint13-creatures", "sprint14-insects"):
    sys.path.insert(0, str(HERE.parents[1] / family))
import generate_pteranodon as shared
from generate_sprint13_creatures import ellipsoid, tube
from generate_sprint14_insects import blade

KINDS = ("crocodile", "dire-crocodile")
SIDE, UP, FORWARD = Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, -1))

# Inset while the region identity is still known. Inferring the region later
# from an exact shared boundary can put a limb endpoint into the ivory tile.
_region_uv = shared.region_uv
def safe_region_uv(region, u, v):
    return _region_uv(region, .02 + .96 * u, .02 + .96 * v)
shared.region_uv = safe_region_uv


def measured_rig(path):
    raw = Path(path).read_bytes()
    data = json.loads(raw)
    if data.get("source") != "request-local hidden Sprint 16 monitor-lizard donor view":
        raise SystemExit("wrong private donor capture")
    if len(data.get("renderers", [])) != 1:
        raise SystemExit("expected one Monitor Lizard renderer")
    source = data["renderers"][0]
    rows = source["bones"]
    by_name = {row["name"]: row for row in rows}
    if (len(rows) != 41 or len(by_name) != 41 or
            source["boneCount"] != 41 or source["bindPoseCount"] != 41 or
            source["rootBone"] != "cent_spine1_jnt"):
        raise SystemExit("incomplete Monitor Lizard frame")

    def depth(name):
        parent = by_name[name].get("parent")
        return depth(parent) + 1 if parent in by_name else 0

    bones = []
    for row in rows:
        point = Vector(row["bindPosition"])
        children = [Vector(child["bindPosition"]) for child in rows
                    if child.get("parent") == row["name"]]
        tail = sum(children, Vector()) / len(children) if children else point + UP * .05
        if (tail - point).length < .001:
            tail = point + UP * .05
        bones.append(dict(name=row["name"], parent=row.get("parent", ""),
                          index=row["index"], depth=depth(row["name"]),
                          head=list(point), tail=list(tail)))
    rig = dict(source=data["source"], space=data["space"],
               renderer=source["renderer"], rootBone=source["rootBone"],
               bindPoseCount=41, bones=bones)
    return rig, {row["name"]: row for row in bones}, hashlib.sha256(raw).hexdigest()


def build_body(bm, weights, uvs, rig, kind):
    dire = kind == "dire-crocodile"
    width = 1.15 if dire else 1.0
    depth = 1.12 if dire else 1.0
    at = lambda name: shared.head(rig, name)

    def mass(centre, radii, bone, region="body", segments=12):
        before = set(uvs)
        ellipsoid(bm, weights, uvs, centre, SIDE, UP, FORWARD,
                  radii, bone, region, 6, segments)
        if region == "beak":
            for vertex in set(uvs) - before:
                u, v = uvs[vertex]
                uvs[vertex] = shared.region_uv("beak", .04 + .40 * (u / .5),
                                               v / .25)

    def flat(points, widths, heights, names, region="body", segments=16):
        return blade(bm, weights, uvs, points, widths, heights, SIDE, UP,
                     names, region, segments)

    # Wide, flattened torso. Shoulders and hips sink below their driving
    # bones; long low belly and broad thorax separate this from a monitor.
    names = ["cent_tail1_jnt", "cent_spine1_jnt",
             "cent_spine2_jnt", "cent_spine3_jnt", "cent_neck1_jnt",
             "cent_neck2_jnt"]
    points = [at(name) - UP * .045 for name in names]
    body_widths = [value * width for value in (.18, .34, .36, .31, .235, .20)]
    body_heights = [value * depth for value in (.13, .17, .175, .155, .12, .09)]

    # Broad blunt skull and U-shaped muzzle, not a long pointed monitor cone.
    # Both sets of teeth are rigidly attached to the corresponding jaw.
    skull = at("cent_head1_jnt")
    muzzle = skull + FORWARD * (.53 if dire else .49)
    head_points = [at("cent_neck2_jnt"), skull, skull + FORWARD * .13,
                   muzzle - FORWARD * .10, muzzle, muzzle + FORWARD * .015]
    head_points = [point - UP * .014 for point in head_points]
    head_widths = [value * width for value in (.17, .225, .20, .165, .14, .065)]
    head_heights = [.082, .090, .057, .049, .042, .025]
    # One continuous skin from tail tip through torso to muzzle. Separate
    # capped primitives created visible collars at neck and tail, so those
    # joints now share topology rather than hiding a gap by overlap.
    tails = ["cent_tail%d_jnt" % index for index in range(1, 8)]
    tail_points = [at(name) for name in tails]
    tail_points.append(tail_points[-1] +
                       (tail_points[-1] - tail_points[-2]).normalized() * .24)
    tail_widths = [value * width for value in (.195, .15, .115, .081, .055, .034, .019, .003)]
    tail_heights = [value * depth for value in (.16, .146, .121, .10, .079, .054, .034, .004)]
    skin_points = list(reversed(tail_points)) + points[1:-1] + head_points
    skin_widths = list(reversed(tail_widths)) + body_widths[1:-1] + head_widths
    skin_heights = list(reversed(tail_heights)) + body_heights[1:-1] + head_heights
    skin = flat(skin_points, skin_widths, skin_heights,
                list(reversed(tails + [tails[-1]])) + names[1:-1] +
                ["cent_neck2_jnt"] + ["cent_head1_jnt"] * 5)
    for index, ring in enumerate(skin):
        region = "membrane" if index < len(tail_points) else "body"
        u = (index / float(len(tail_points) - 1) if region == "membrane" else
             (index - len(tail_points)) / float(len(skin) - len(tail_points) - 1))
        for step, vertex in enumerate(ring):
            uvs[vertex] = shared.region_uv(region, u,
                                           shared.fold(step * 2 * math.pi / len(ring)))
    hinge = at("cent_jaw1_jnt")
    jaw_points = [hinge, hinge + FORWARD * .16,
                  Vector((0, hinge.y - .021, muzzle.z + .02)),
                  Vector((0, hinge.y - .01, muzzle.z - .012))]
    flat(jaw_points, [.19 * width, .185 * width, .14 * width, .065 * width],
         [.049, .035, .032, .017], ["cent_jaw1_jnt"] * 4)
    # The mouth lining is a thin closed inner surface, not an activation
    # effect and not borrowed Purple Worm geometry.
    mouth = hinge + FORWARD * .245 + UP * .025
    mass(mouth, (.143 * width, .006, .24), "cent_jaw1_jnt", "beak")

    for sign in (-1, 1):
        # Raised ocular ridges at the back of the skull; tiny lateral iris.
        eye = skull + SIDE * sign * .166 * width + UP * .078 - FORWARD * .025
        mass(eye - UP * .018, (.060, .026, .08), "cent_head1_jnt", "body")
        mass(eye + SIDE * sign * .030,
             (.024, .015, .031), "cent_head1_jnt", "crest", 12)
        nostril = muzzle + SIDE * sign * .075 * width + UP * .026 + FORWARD * -.035
        mass(nostril, (.027, .012, .035), "cent_head1_jnt", "body")
        for index in range(9):
            t = index / 8.0
            z = skull.z - .09 - t * (.37 if dire else .33)
            x = sign * width * (.19 - .055 * t)
            tooth = .025 + .015 * math.sin(index * 1.9) ** 2
            for lower in (False, True):
                base = Vector((x, hinge.y + (.01 if lower else .052), z))
                tip = base + UP * (tooth if lower else -tooth)
                first = set(uvs)
                tube(bm, weights, uvs, [base, tip], [.0095, .0015],
                     ["cent_jaw1_jnt" if lower else "cent_head1_jnt"] * 2,
                     "beak", 6)
                # Ivory occupies the high-u half, separated from mouth tissue.
                for vertex in set(uvs) - first:
                    u, v = uvs[vertex]
                    uvs[vertex] = shared.region_uv("beak", .56 + .40 * (u / .5),
                                                  (v / .25) * .96 + .02)

    # Tail retains all seven native articulations. Compressed lateral width
    # and dorsal keel produce the paddle silhouette without aquatic mechanics.
    # Four sprawling limbs, with broad palms and separate blunt clawed toes.
    for prefix, sign in (("left", 1), ("right", -1)):
        for front in (True, False):
            names = ([prefix + "_hand1_jnt", prefix + "_hand2_jnt",
                      prefix + "_arm1_jnt"] if front else
                     [prefix + "_leg1_jnt", prefix + "_leg2_jnt",
                      prefix + "_foot1_jnt"])
            hip, elbow, foot = (at(name) for name in names)
            tube(bm, weights, uvs,
                 [hip, hip.lerp(elbow, .50), elbow, elbow.lerp(foot, .55), foot],
                 [value * width for value in (.135, .135, .098, .074, .055)],
                 [names[0], names[0], names[1], names[1], names[2]], "limbs", 10)
            palm = foot + FORWARD * .07 - UP * .036
            mass(palm, (.083 * width, .039, .112), names[2], "limbs")
            for digit in range(4 if not front else 5):
                lateral = (digit - (1.5 if not front else 2)) * .038
                base = palm + SIDE * lateral + FORWARD * .055
                tip = base + FORWARD * (.11 - abs(lateral) * .6) + SIDE * lateral * .3
                tube(bm, weights, uvs, [base, tip], [.018, .008],
                     [names[2]] * 2, "limbs", 7)

    # Low osteoderm rows, not fantasy spikes. Dire has heavier square plates
    # and a double tail crest, an anatomical distinction beyond simple scale.
    def scute(centre, radii, bone):
        rx, height, rz = radii
        vertices = []
        for upper in (False, True):
            for x, z in ((-1, -1), (-1, 1), (1, 1), (1, -1)):
                vertex = bm.verts.new(centre + Vector((
                    x * rx * (.25 if upper else 1),
                    height if upper else -.012,
                    z * rz * (.65 if upper else 1))))
                weights[vertex] = [(bone, 1)]
                uvs[vertex] = shared.region_uv("membrane", (z + 1) / 2,
                                               .88 if upper else .5)
                vertices.append(vertex)
        for face in ((0, 1, 2, 3), (4, 7, 6, 5), (0, 4, 5, 1),
                     (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0)):
            bm.faces.new(tuple(vertices[i] for i in face))

    for index in range(11):
        z = -.50 + index * .10
        bone = ("cent_neck1_jnt" if z < -.32 else
                "cent_spine3_jnt" if z < -.15 else
                "cent_spine2_jnt" if z < .18 else "cent_ass1_jnt")
        # Sample the authored torso surface, rather than a bone's height:
        # the latter buries central plates inside the flattened body.
        segment = next(i for i in range(len(skin_points) - 1)
                       if skin_points[i].z >= z >= skin_points[i + 1].z)
        t = ((skin_points[segment].z - z) /
             (skin_points[segment].z - skin_points[segment + 1].z))
        c = skin_points[segment].lerp(skin_points[segment + 1], t)
        w = skin_widths[segment] * (1 - t) + skin_widths[segment + 1] * t
        h = skin_heights[segment] * (1 - t) + skin_heights[segment + 1] * t
        for column in (-1.5, -.5, .5, 1.5):
            x = column * .102 * width
            y = c.y + h * math.sqrt(max(0, 1 - (x / w) ** 2))
            scute(Vector((x, y, z)),
                  (.044 * width, .021 if not dire else .032, .043), bone)
    for index, bone in enumerate(tails[:-1]):
        centre = at(bone)
        h = (.16, .146, .121, .10, .079, .054)[index] * depth
        for offset in ((-.055, .055) if index < 3 else (0,)):
            scute(centre + UP * (h - .009) + SIDE * offset,
                  (.026, .043 if dire else .029, .083), bone)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", required=True, choices=KINDS)
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:]
                             if "--" in sys.argv else [])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = measured_rig(args.capture)
    armature = shared.build_armature(rig_data)
    armature.name = args.kind + "RigPreview"
    mesh = bpy.data.meshes.new(args.kind + "Mesh")
    obj = bpy.data.objects.new(args.kind, mesh)
    bpy.context.collection.objects.link(obj)
    bm = bmesh.new()
    weights, uvs = {}, {}
    build_body(bm, weights, uvs, rig, args.kind)
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.normal_update()
    bm.to_mesh(mesh)
    indexed = {vertex.index: entries for vertex, entries in weights.items()}
    uv_indexed = {vertex.index: uv for vertex, uv in uvs.items()}
    bm.free()
    if len(indexed) != len(mesh.vertices) or len(uv_indexed) != len(mesh.vertices):
        raise SystemExit("unweighted/unpainted original vertex")
    layer = mesh.uv_layers.new(name="Albedo")
    for loop in mesh.loops:
        layer.data[loop.index].uv = uv_indexed[loop.vertex_index]
    groups = {}
    for index, entries in indexed.items():
        total = sum(value for _, value in entries)
        if total <= 0 or len(entries) > 4:
            raise SystemExit("invalid bone influence row")
        for name, value in entries:
            if name not in rig:
                raise SystemExit("unmeasured bone: " + name)
            if name not in groups:
                groups[name] = obj.vertex_groups.new(name=name)
            groups[name].add([index], value / total, "REPLACE")
    required = ["cent_head1_jnt", "cent_jaw1_jnt"] + [
        "cent_tail%d_jnt" % index for index in range(1, 8)] + [
            side + joint for side in ("left", "right")
            for joint in ("_hand1_jnt", "_hand2_jnt", "_arm1_jnt",
                          "_leg1_jnt", "_leg2_jnt", "_foot1_jnt")]
    if set(required) - set(groups):
        raise SystemExit("missing jaw/tail/limb influence")
    obj.modifiers.new(name="Armature", type="ARMATURE").object = armature
    obj.parent = armature
    shared.attach_preview_material(obj, mesh, args.albedo)
    for polygon in mesh.polygons:
        polygon.use_smooth = True
    report = dict(schemaVersion=1, creature=args.kind, donorFamily="monitor-lizard",
                  captureSprint="Sprint 16", rigSha256=rig_hash, rigBoneCount=41,
                  vertices=len(mesh.vertices), polygons=len(mesh.polygons),
                  boneGroups=len(groups), maxInfluencesPerVertex=max(map(len, indexed.values())),
                  visibleLegs=4, jawSeparated=True, tailJoints=7,
                  extent={axis: [min(getattr(v.co, axis) for v in mesh.vertices),
                                 max(getattr(v.co, axis) for v in mesh.vertices)]
                          for axis in ("x", "y", "z")},
                  albedo=shared.albedo_manifest(args.albedo))
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    shared.write_mesh_data(args.mesh_data, obj, mesh, indexed, uv_indexed, report, args.albedo)
    payload = json.loads(Path(args.mesh_data).read_text(encoding="utf-8"))
    payload.update(space="donor renderer local; +X right, +Y up, -Z forward",
                   visibleLegs=4, jawSeparated=True, tailJoints=7)
    Path(args.mesh_data).write_text(json.dumps(payload, indent=1) + "\n", encoding="utf-8", newline="\n")
    bpy.ops.wm.save_as_mainfile(filepath=str(Path(args.blend_out).resolve()))
    print("[crocodilian] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
