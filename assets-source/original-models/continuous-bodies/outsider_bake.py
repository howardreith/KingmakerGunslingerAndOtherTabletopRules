"""Position-baked albedo for the R7 Glabrezu, Nalfeshnee and Ice Devil revisions.

The legacy outsider atlas paints five flat tiles, so a fused skin can only vary
by primitive. Here the atlas tiles are kept as material IDs for the authoring
code, the whole model is unwrapped with the library's existing `unwrap`, and
each texel is evaluated from its bind-pose position, normal, material ID and
baked cavity occlusion. Only project-authored geometry and closed-form colour
functions are inputs. No other generator imports this module.
"""
import math
from pathlib import Path

import bmesh
import bpy
import numpy as np
from mathutils import Vector
from mathutils.bvhtree import BVHTree

import continuous_body as surface

NORMAL_LAYER = 'KMG_ContinuousSkinNormal'
REGIONS = ('body', 'limbs', 'membrane', 'beak', 'crest')
BODY, LIMBS, MEMBRANE, BEAK, CREST = range(5)


def smooth(low, high, value):
    t = np.clip((value - low) / (high - low), 0.0, 1.0)
    return t * t * (3.0 - 2.0 * t)


def mix(a, b, t):
    return a + (b - a) * t[..., None]


def rgb(values, count):
    return np.broadcast_to(np.array(values, dtype=np.float64), (count, 3)).copy()


def hemisphere_directions(count=96):
    golden = math.pi * (3.0 - math.sqrt(5.0))
    rows = []
    for index in range(count):
        z = 1.0 - (index + .5) * 2.0 / count
        radius = math.sqrt(max(0.0, 1.0 - z * z))
        rows.append(Vector((math.cos(golden * index) * radius,
                            math.sin(golden * index) * radius, z)))
    return rows


def cavity(tree, position, normal, directions, reach):
    open_weight, total = 0.0, 0.0
    origin = position + normal * .012
    for direction in directions:
        facing = direction.dot(normal)
        if facing <= .05:
            continue
        total += facing
        if tree.ray_cast(origin, direction, reach)[0] is None:
            open_weight += facing
    return open_weight / total if total else 1.0


def face_region(uv_rows, atlas):
    u = sum(row[0] for row in uv_rows) / len(uv_rows)
    v = sum(row[1] for row in uv_rows) / len(uv_rows)
    for index, name in enumerate(REGIONS):
        rect = atlas[name]
        if rect[0] <= u <= rect[2] and rect[1] <= v <= rect[3]:
            return index
    raise ValueError('authored UV lies outside the material tiles')


def rasterize(mesh, attributes, size):
    """Yield (positions, normals, regions, cavity, pixels) for every texel."""
    mesh.calc_loop_triangles()
    uv = mesh.uv_layers.active.data
    positions, normals, cavities = attributes
    regions = mesh.attributes[surface.REGION_LAYER].data
    out_p, out_n, out_r, out_c, out_px = [], [], [], [], []
    for triangle in mesh.loop_triangles:
        corners = np.array([uv[i].uv for i in triangle.loops]) * size - .5
        ids = list(triangle.vertices)
        low = np.clip(np.floor(corners.min(axis=0)).astype(int), 0, size - 1)
        high = np.clip(np.ceil(corners.max(axis=0)).astype(int), 0, size - 1)
        xs, ys = np.meshgrid(np.arange(low[0], high[0] + 1),
                             np.arange(low[1], high[1] + 1))
        xs, ys = xs.ravel(), ys.ravel()
        (x0, y0), (x1, y1), (x2, y2) = corners
        det = (y1 - y2) * (x0 - x2) + (x2 - x1) * (y0 - y2)
        if abs(det) < 1e-12:
            continue
        a = ((y1 - y2) * (xs - x2) + (x2 - x1) * (ys - y2)) / det
        b = ((y2 - y0) * (xs - x2) + (x0 - x2) * (ys - y2)) / det
        c = 1.0 - a - b
        inside = (a >= -.02) & (b >= -.02) & (c >= -.02)
        if not inside.any():
            continue
        bary = np.stack([a[inside], b[inside], c[inside]], axis=1)
        out_p.append(bary @ positions[ids])
        blended = bary @ normals[ids]
        out_n.append(blended / np.maximum(
            np.linalg.norm(blended, axis=1, keepdims=True), 1e-9))
        out_c.append(bary @ cavities[ids])
        out_r.append(np.full(len(bary), regions[ids[0]].value))
        out_px.append(np.stack([ys[inside], xs[inside]], axis=1))
    return (np.concatenate(out_p), np.concatenate(out_n), np.concatenate(out_r),
            np.concatenate(out_c), np.concatenate(out_px))


def write_image(path, size, pixels, colours, dilate=14):
    image = np.zeros((size, size, 3), dtype=np.float64)
    filled = np.zeros((size, size), dtype=bool)
    image[pixels[:, 0], pixels[:, 1]] = colours
    filled[pixels[:, 0], pixels[:, 1]] = True
    for _ in range(dilate):
        if filled.all():
            break
        grown, mask = image.copy(), filled.copy()
        for dy, dx in ((0, 1), (0, -1), (1, 0), (-1, 0)):
            shifted = np.roll(np.roll(filled, dy, axis=0), dx, axis=1)
            source = np.roll(np.roll(image, dy, axis=0), dx, axis=1)
            take = shifted & ~mask
            grown[take] = source[take]
            mask |= take
        image, filled = grown, mask
    image[~filled] = image[filled].mean(axis=0)
    Path(path).parent.mkdir(parents=True, exist_ok=True)
    Path(path).write_bytes(surface.encode_png(
        (np.flipud(image) * 255.0 + .5).astype(np.uint8)))


def bake_model(bm, weights, uvs, atlas, path, colour, size=1024, reach=.42):
    """Unwrap every face of `bm`, bake `colour` to `path` and rewrite `bm`.

    `colour(positions, normals, regions, cavity)` returns linear-free display
    RGB in [0, 1]. Every exported vertex then carries a stored smooth normal,
    because UV seams open the closed solids that normal recalculation needs.
    """
    bm.verts.ensure_lookup_table()
    layer = bm.verts.layers.float_vector.get(NORMAL_LAYER)
    if layer is None:
        layer = bm.verts.layers.float_vector.new(NORMAL_LAYER)
    discrete = [face for face in bm.faces
                if not any(v[layer].length_squared > .5 for v in face.verts)]
    bmesh.ops.recalc_face_normals(bm, faces=discrete)
    bm.normal_update()
    bm.verts.index_update()
    bm.faces.index_update()
    verts = list(bm.verts)
    positions = np.array([v.co[:] for v in verts], dtype=np.float64)
    normals = np.array([(v[layer] if v[layer].length_squared > .5 else v.normal)[:]
                        for v in verts], dtype=np.float64)
    tree = BVHTree.FromBMesh(bm)
    directions = hemisphere_directions()
    cavities = np.array([cavity(tree, v.co, Vector(normals[v.index]),
                                directions, reach) for v in verts])
    old_weights = {v.index: list(weights[v]) for v in verts}
    faces = [[v.index for v in face.verts] for face in bm.faces]
    face_regions = [face_region([uvs[verts[i]] for i in ids], atlas)
                    for ids in faces]

    mesh = bpy.data.meshes.new('KMGOutsiderBake')
    mesh.from_pydata(positions.tolist(), [], faces)
    mesh.update()
    obj = bpy.data.objects.new('KMGOutsiderBake', mesh)
    bpy.context.collection.objects.link(obj)
    region_attr = mesh.attributes.new(surface.REGION_LAYER, 'INT', 'POINT')
    for ids, region in zip(faces, face_regions):
        for index in ids:
            region_attr.data[index].value = region
    surface.unwrap(obj)
    p, n, r, c, pixels = rasterize(mesh, (positions, normals, cavities), size)
    write_image(path, size, pixels, np.clip(colour(p, n, r, c), 0.0, 1.0))

    uv = mesh.uv_layers.active.data
    loops = [(list(poly.vertices), list(poly.loop_indices))
             for poly in mesh.polygons]
    rows = [[tuple(uv[i].uv) for i in indices] for _, indices in loops]
    bm.clear()
    weights.clear()
    uvs.clear()
    layer = bm.verts.layers.float_vector.get(NORMAL_LAYER)
    if layer is None:
        layer = bm.verts.layers.float_vector.new(NORMAL_LAYER)
    made = {}
    for (ids, _), row in zip(loops, rows):
        corners = []
        for index, pair in zip(ids, row):
            key = (index, round(pair[0], 6), round(pair[1], 6))
            if key not in made:
                vertex = bm.verts.new(positions[index])
                vertex[layer] = normals[index]
                weights[vertex] = old_weights[index]
                uvs[vertex] = (min(.999, max(.001, pair[0])),
                               min(.999, max(.001, pair[1])))
                made[key] = vertex
            corners.append(made[key])
        bm.faces.new(corners)
    bpy.data.objects.remove(obj, do_unlink=True)
    bpy.data.meshes.remove(mesh)
    bm.normal_update()
