"""Offline helpers for the four explicitly assigned Cleanup 2 bodies.

No native geometry is read. The existing continuous-body authoring library
owns union, outward export and deterministic painting; this file only joins
species-authored closed parts and interpolates their reviewed donor drivers.
"""
import json
from pathlib import Path
import sys

import bpy
import numpy as np
from mathutils import Vector

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "continuous-bodies"))
import continuous_body as cb


def chain_weights(obj, lines, blend=.32):
    """Smooth local weights over authored connected branches.

    Each line is (points, segment drivers). Proximity shares the influence
    between branches at a junction; the four strongest contributions are
    normalized. In particular, digit roots share the palm's Hand driver.
    """
    candidates = []
    for points, bones in lines:
        points = [Vector(p) for p in points]
        for i in range(len(points) - 1):
            candidates.append((points[i], points[i + 1], bones[i],
                               bones[min(i + 1, len(bones) - 1)]))
    rows = {}
    for vertex in obj.data.vertices:
        near = []
        for a, b, own, following in candidates:
            axis = b - a
            t = max(0., min(1., (vertex.co - a).dot(axis) /
                           max(axis.length_squared, 1e-10)))
            distance = (vertex.co - a.lerp(b, t)).length
            share = max(0., min(1., (t - (1. - blend)) / blend)) * .5
            near.append((distance, own, following, share))
        closest = min(v[0] for v in near)
        merged = {}
        for distance, own, following, share in near:
            # A small junction radius blends adjoining segments while
            # keeping a fingertip owned by its own terminal chain.
            strength = np.exp(-((distance - closest) / .045) ** 2)
            if strength < .002:
                continue
            merged[own] = merged.get(own, 0.) + strength * (1. - share)
            merged[following] = merged.get(following, 0.) + strength * share
        values = sorted(merged.items(), key=lambda p: (-p[1], p[0]))[:4]
        total = sum(v for _, v in values)
        rows[vertex.index] = [(n, v / total) for n, v in values if v > 0.]
    return rows


def union_part(parts, name, volume, lines, region, target=3500, smooth=5):
    obj = volume.polygonise(name)
    cb.finish_surface(obj, target, smooth_iterations=smooth)
    parts.append((obj, chain_weights(obj, lines), region))
    return obj


def rigid(parts, obj, bone, region):
    parts.append((obj, cb.rigid_rows(obj, [(bone, 1.)]), region))
    return obj


def export(args, rig_data, rig_hash, parts, space, extra, colour):
    obj, rows = cb.assemble(extra["creature"], parts)
    cb.unwrap(obj)
    cb.bake_albedo(obj, 1024, colour, args.albedo)
    cb.attach_albedo_preview(obj, args.albedo)
    extra = dict(extra, triangleWinding="outward-visual-refinement",
                 cleanupIteration=2)
    if extra["creature"] == "giant-crab":
        points = [v.co for v in obj.data.vertices]
        extra["widthOverLength"] = round(
            (max(p.x for p in points) - min(p.x for p in points)) /
            (max(p.y for p in points) - min(p.y for p in points)), 4)
    summary = cb.write_mesh_data(args.mesh_data, obj, rows, rig_hash,
                                 args.albedo, space, extra)
    report = dict(schemaVersion=1, runtimeQualified=False,
                  rigSha256=rig_hash, **extra, **summary,
                  albedo=cb.albedo_manifest(args.albedo))
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    armature = cb.review_armature(rig_data, extra["creature"] + "EditableRig")
    groups = {}
    for index, row in rows.items():
        for name, value in row:
            group = groups.get(name) or obj.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    obj.modifiers.new("ReviewedDonorRig", "ARMATURE").object = armature
    obj.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=str(Path(args.blend_out).resolve()))
    print("[cleanup2-authoring] " + json.dumps(report, sort_keys=True))
    return report


def painted(base, top, up_axis, accents):
    """Fine original skin/chitin field with a directional broad highlight."""
    base, top = np.array(base), np.array(top)
    def colour(p, n, region):
        light = np.clip(n[:, up_axis] * .5 + .5, 0., 1.)[:, None]
        grain = cb.fbm(p, 34., 321)
        mottles = cb.fbm(p, 5., 322)
        variation = .80 + .22 * grain[:, None] + .18 * mottles[:, None]
        rgb = (base + (top - base) * light) * variation
        for key, value in accents.items():
            if callable(value):
                tint = value(p, n)
            else:
                tint = np.array(value)
            rgb = np.where((region == key)[:, None], tint * variation, rgb)
        return rgb
    return colour
