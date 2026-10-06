#!/usr/bin/env python3
"""Original snake prototypes; native input is private bind metadata only.

No native geometry, textures or animation data are read. The water donor's
arms/effect branches receive no geometry. Runtime integration/qualification
is deliberately separate: these files alone do not publish a creature.
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
for family in ("pteranodon", "sprint13-creatures"):
    sys.path.insert(0, str(HERE.parents[1] / family))
import generate_pteranodon as shared
from generate_sprint13_creatures import ellipsoid, tube

KINDS = ("viper", "constrictor-snake")
BODY_BONES = ("LowerTorso", "SpineA_M", "Spine1_M", "SpineB_M",
              "UpperTorso", "Neck_M", "Head", "Jaw_M")
SIDE, UP, FORWARD = Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1))
_region_uv = shared.region_uv


def safe_region_uv(region, u, v):
    return _region_uv(region, .02 + .96 * u, .02 + .96 * v)


shared.region_uv = safe_region_uv


def remap_beak_uv(uvs, vertices, low, high):
    """Remap known beak coordinates, without inferring a shared atlas edge."""
    u0, v0, u1, v1 = shared.ATLAS["beak"]
    for vertex in vertices:
        u, v = uvs[vertex]
        local_u = ((u - u0) / (u1 - u0) - .02) / .96
        local_v = ((v - v0) / (v1 - v0) - .02) / .96
        uvs[vertex] = shared.region_uv("beak", low + (high - low) * local_u,
                                     local_v)


def measured_rig(path):
    raw = Path(path).read_bytes()
    rig, by_name = decode_rig(json.loads(raw))
    return rig, by_name, hashlib.sha256(raw).hexdigest()


def decode_rig(capture):
    if (capture.get("key") != "medium-water-elemental" or
            capture.get("nativeBlueprint") != "62a3e860e6e72e6499c38bb8b2fe303e" or
            capture.get("prefab") != "dc296683c2a3d2648afa516aeb030fb8"):
        raise SystemExit("wrong native research capture")
    skins = capture.get("skinnedRenderers", [])
    expected = {"body", "bubblegum_stripe", "grebeshok", "grebeshok_outer",
                "pool_skirt", "stripes_all", "stripes_outer"}
    if len(skins) != 7 or {s.get("renderer") for s in skins} != expected:
        raise SystemExit("unexpected native water renderer set")
    source = next(s for s in skins if s["renderer"] == "body")
    rows = source["bones"]
    by_name = {r["name"]: r for r in rows}
    if (len(rows) != 43 or len(by_name) != 43 or source["boneCount"] != 43 or
            source["bindPoseCount"] != 43 or source["rootBone"] != "LowerTorso" or
            set(BODY_BONES) - set(by_name)):
        raise SystemExit("incomplete native water body frame")
    if (sorted(r.get("index", -1) for r in rows) != list(range(43)) or
            any(len(r.get("bindPosition", [])) != 3 or
                any(not math.isfinite(v) for v in r["bindPosition"]) for r in rows)):
        raise SystemExit("invalid native bind coordinates or indices")

    def depth(name, visited=()):
        if name in visited:
            raise SystemExit("cyclic bone graph")
        parent = by_name[name].get("parent")
        return depth(parent, visited + (name,)) + 1 if parent in by_name else 0

    bones = []
    for row in rows:
        point = Vector(row["bindPosition"])
        children = [Vector(c["bindPosition"]) for c in rows
                    if c.get("parent") == row["name"]]
        tail = sum(children, Vector()) / len(children) if children else point + UP * .2
        if (tail - point).length < .001:
            tail = point + UP * .2
        bones.append(dict(name=row["name"], parent=row.get("parent", ""),
                          index=row["index"], depth=depth(row["name"]),
                          head=list(point), tail=list(tail)))
    rig = dict(source=capture["scope"], space=capture["space"],
               renderer="body", rootBone="LowerTorso", bindPoseCount=43, bones=bones)
    return rig, {r["name"]: r for r in bones}


def catmull(a, b, c, d, t):
    return .5 * ((2 * b) + (-a + c) * t +
                 (2 * a - 5 * b + 4 * c - d) * t * t +
                 (-a + 3 * b - 3 * c + d) * t * t * t)


def sweep(bm, weights, uvs, points, widths, heights, names, region="body",
          steps=4, sides=14, up=UP, forward=FORWARD, uv_dorsal_at=None):
    """Continuous closed original skin, smoothly blended between named drivers."""
    if not len(points) == len(widths) == len(heights) == len(names):
        raise ValueError("inconsistent sweep rows")
    samples = []
    for index in range(len(points) - 1):
        a, b, c, d = (points[max(index - 1, 0)], points[index], points[index + 1],
                      points[min(index + 2, len(points) - 1)])
        for step in range(steps):
            t = step / float(steps)
            blend = {names[index]: 1 - t}
            blend[names[index + 1]] = blend.get(names[index + 1], 0) + t
            samples.append((catmull(a, b, c, d, t),
                            widths[index] * (1 - t) + widths[index + 1] * t,
                            heights[index] * (1 - t) + heights[index + 1] * t,
                            [(k, v) for k, v in blend.items() if v > 1e-7]))
    samples.append((points[-1], widths[-1], heights[-1], [(names[-1], 1)]))
    # Transport the cross-section from the muzzle back toward the tail.
    # Starting at the coil would accumulate arbitrary roll through the S-neck
    # and turn the triangular head onto its edge.
    laterals, prior_side = {}, SIDE
    handedness = 1 if forward.cross(SIDE).dot(up) > 0 else -1
    for index in reversed(range(len(samples))):
        tangent = (samples[min(index + 1, len(samples) - 1)][0] -
                   samples[max(index - 1, 0)][0]).normalized()
        lateral = prior_side - tangent * prior_side.dot(tangent)
        if lateral.length < .01:
            lateral = tangent.cross(up if abs(tangent.dot(up)) < .9 else forward)
        lateral.normalize()
        prior_side = lateral
        laterals[index] = (lateral, tangent.cross(lateral).normalized() * handedness)
    rings = []
    for index, (centre, width, height, influences) in enumerate(samples):
        lateral, vertical = laterals[index]
        ring = []
        for side in range(sides):
            angle = side * math.tau / sides
            vertex = bm.verts.new(centre + lateral * math.cos(angle) * width +
                                  vertical * math.sin(angle) * height)
            weights[vertex] = influences
            across = shared.fold(angle)
            if uv_dorsal_at is not None:
                # An upright hybrid changes which side is dorsal between tail
                # and chest. Paint orientation need not inherit transported
                # section roll (which can otherwise put belly scales on top).
                dorsal = Vector(uv_dorsal_at(centre))
                tangent = lateral.cross(vertical).normalized()
                dorsal -= tangent * dorsal.dot(tangent)
                if dorsal.length < 1e-7:
                    raise ValueError("dorsal paint direction parallel to skin")
                radial = lateral * math.cos(angle) + vertical * math.sin(angle)
                across = max(0, min(1, .5 + .5 * radial.dot(dorsal.normalized())))
            uvs[vertex] = shared.region_uv(region, index / (len(samples) - 1), across)
            ring.append(vertex)
        if rings:
            for side in range(sides):
                bm.faces.new((rings[-1][side], rings[-1][(side + 1) % sides],
                              ring[(side + 1) % sides], ring[side]))
        rings.append(ring)
    for ring, sample in ((rings[0], samples[0]), (rings[-1], samples[-1])):
        centre = bm.verts.new(sample[0])
        weights[centre] = sample[3]
        uvs[centre] = shared.region_uv(region, 0 if ring is rings[0] else 1, .5)
        for side in range(sides):
            bm.faces.new((centre, ring[side], ring[(side + 1) % sides]))


def build_body(bm, weights, uvs, rig, kind):
    viper = kind == "viper"
    at = lambda name: shared.head(rig, name)
    radius = 1.25 if viper else 2.35
    # The low tail coil belongs to LowerTorso, not an invented limb chain.
    # All forebody segments retain the donor's measured articulations.
    points, widths, heights, names = [], [], [], []
    for index in range(19):
        t = index / 18.0
        angle = math.radians(-210 + 300 * t)
        r = 10.5 + 2 * (1 - t)
        point = Vector((r * math.cos(angle), radius * .86,
                        -10.5 + r * math.sin(angle)))
        points.append(point)
        width = .09 + (radius - .09) * math.sin(t * math.pi / 2) ** .65
        widths.append(width)
        heights.append(width * .86)
        names.append("LowerTorso")
    for index, name in enumerate(BODY_BONES[:-1]):
        points.append(at(name))
        width = radius * (1 if index < 4 else .86 if index == 4 else .68)
        widths.append(width)
        heights.append(width * .94)
        names.append(name)
    skull = at("Head")
    # A broad triangular viper head; a narrower rounded constrictor head.
    head_width = 2.35 if viper else 1.80
    for offset, width in ((.9, head_width), (2.6, head_width * .87),
                          (4.3, head_width * .50), (4.7, head_width * .16)):
        points.append(skull + FORWARD * offset - UP * .13)
        widths.append(width)
        heights.append(.74 if viper else .87)
        names.append("Head")
    sweep(bm, weights, uvs, points, widths, heights, names)
    hinge = at("Jaw_M")
    sweep(bm, weights, uvs,
          [hinge, hinge + FORWARD * 1.0 + UP * .2,
           skull + FORWARD * 3.7 - UP * .86,
           skull + FORWARD * 4.6 - UP * .76],
          [head_width * .78, head_width * .87, head_width * .51, head_width * .18],
          [.30, .30, .22, .12], ["Jaw_M"] * 4, steps=3)

    def mass(centre, radii, bone, region):
        ellipsoid(bm, weights, uvs, centre, SIDE, UP, FORWARD,
                  radii, bone, region, 6, 12)

    # Independent lower jaw lining; teeth use the other half of the same atlas.
    before = set(uvs)
    mass(hinge + FORWARD * 1.65 + UP * .40,
         (head_width * .65, .08, 1.95), "Jaw_M", "beak")
    remap_beak_uv(uvs, set(uvs) - before, .06, .43)
    for sign in (-1, 1):
        eye = skull + SIDE * sign * head_width * .83 + FORWARD * 1.25 + UP * .38
        mass(eye, (.29, .25, .34), "Head", "crest")
        # Long folding-fang anatomy is a viper distinction; the constrictor
        # has smaller curved gripping teeth rather than the same venom fangs.
        for index in range(2 if viper else 5):
            base = (skull + SIDE * sign * head_width * (.65 - index * .06) +
                    FORWARD * (2.35 + index * (.8 if viper else .45)) - UP * .54)
            tip = base - UP * (1.1 if viper and index == 0 else .40) - FORWARD * .18
            first = set(uvs)
            tube(bm, weights, uvs, [base, base.lerp(tip, .6) + FORWARD * .1, tip],
                 [.16 if viper else .09, .075, .012], ["Head"] * 3, "beak", 7)
            remap_beak_uv(uvs, set(uvs) - first, .57, .94)


def write_prototype(args, rig_data, rig, rig_hash, builder, allowed_bones,
                    donor_family, space, anatomy=None, max_influences=2):
    """Export original geometry only; private armatures never enter the package."""
    bpy.ops.wm.read_factory_settings(use_empty=True)
    armature = shared.build_armature(rig_data)
    armature.name = args.kind + "NativeRigPreview"
    mesh = bpy.data.meshes.new(args.kind + "OriginalMesh")
    obj = bpy.data.objects.new(args.kind, mesh)
    bpy.context.collection.objects.link(obj)
    bm = bmesh.new()
    weights, uvs = {}, {}
    builder(bm, weights, uvs, rig, args.kind)
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.normal_update()
    bm.to_mesh(mesh)
    indexed = {v.index: row for v, row in weights.items()}
    uv_indexed = {v.index: row for v, row in uvs.items()}
    bm.free()
    if len(indexed) != len(mesh.vertices) or len(uv_indexed) != len(mesh.vertices):
        raise SystemExit("unweighted/unpainted original vertex")
    layer = mesh.uv_layers.new(name="OriginalAlbedo")
    for loop in mesh.loops:
        layer.data[loop.index].uv = uv_indexed[loop.vertex_index]
    groups = {}
    for index, entries in indexed.items():
        total = sum(value for _, value in entries)
        if total <= 0 or len(entries) > max_influences:
            raise SystemExit("invalid influence row")
        for name, value in entries:
            if name not in allowed_bones:
                raise SystemExit("unapproved branch cannot carry snake geometry")
            if name not in groups:
                groups[name] = obj.vertex_groups.new(name=name)
            groups[name].add([index], value / total, "REPLACE")
    if set(groups) != set(allowed_bones):
        raise SystemExit("incomplete torso/head/jaw influence set")
    obj.modifiers.new(name="NativeRigPreview", type="ARMATURE").object = armature
    obj.parent = armature
    shared.attach_preview_material(obj, mesh, args.albedo)
    for polygon in mesh.polygons:
        polygon.use_smooth = True
    report = dict(schemaVersion=1, creature=args.kind, donorFamily=donor_family,
                  rigSha256=rig_hash, rigBoneCount=len(rig_data["bones"]), vertices=len(mesh.vertices),
                  polygons=len(mesh.polygons), boneGroups=len(groups),
                  maxInfluencesPerVertex=max(map(len, indexed.values())),
                  visibleLegs=0, jawSeparated=True, runtimeQualified=False,
                  extent={axis: [min(getattr(v.co, axis) for v in mesh.vertices),
                                 max(getattr(v.co, axis) for v in mesh.vertices)]
                          for axis in ("x", "y", "z")}, albedo=shared.albedo_manifest(args.albedo))
    report.update(anatomy or {})
    for path in (args.report, args.mesh_data, args.blend_out):
        Path(path).parent.mkdir(parents=True, exist_ok=True)
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    shared.write_mesh_data(args.mesh_data, obj, mesh, indexed, uv_indexed, report, args.albedo)
    payload = json.loads(Path(args.mesh_data).read_text(encoding="utf-8"))
    payload.update(space=space,
                   visibleLegs=0, jawSeparated=True)
    Path(args.mesh_data).write_text(json.dumps(payload, indent=1) + "\n", encoding="utf-8", newline="\n")
    bpy.ops.wm.save_as_mainfile(filepath=str(Path(args.blend_out).resolve()))
    print("[snake-prototype] " + json.dumps(report, sort_keys=True))


def parse_args(include_body_review=False):
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", required=True, choices=KINDS)
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    if include_body_review:
        parser.add_argument("--body-review", required=True)
    return parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])


def main():
    args = parse_args()
    rig_data, rig, rig_hash = measured_rig(args.capture)
    write_prototype(args, rig_data, rig, rig_hash, build_body, BODY_BONES,
                    "native-water-elemental",
                    "donor renderer local; +X right, +Y up, +Z forward")


if __name__ == "__main__":
    main()
