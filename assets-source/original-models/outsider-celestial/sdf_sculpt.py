"""Signed-distance sculpting for the Outsider/Celestial body refinement.

The first exports unioned closed ellipsoids and capsules
(`continuous_body.Volume`). That can only add mass: it cannot carve an eye
socket or a mouth, crease a fold of flesh, or blend two masses over a chosen
width, so the bodies read as rounded toys at the party camera.

This module evaluates an editable signed-distance body on one fixed voxel grid
instead:

- primitives (ellipsoids, round cones, chains, rounded boxes, planes) placed
  relative to the measured donor joints;
- smooth union, smooth subtraction and smooth intersection with an explicit
  blend width, so a fillet, a crease or a carved socket is a stated choice;
- displacement for folds and wrinkles;
- a per-voxel material id painted by primitives and regions, sampled again
  when the albedo is baked;
- an ambient cavity term from the same field, so folds and sockets darken in
  the texture as well as in the silhouette.

OpenVDB, bundled with Blender 4.5, extracts the zero surface. Its output order
depends on threading, so vertices and faces are put into a canonical order
before Blender sees them; the same source therefore exports the same bytes.
`continuous_body` still relaxes, weights, unwraps, bakes, exports and audits
the result, so every existing contract (closed outward components, one
continuous body, at most four influences) applies unchanged.
"""
import math

import bmesh
import bpy
import numpy as np
import openvdb as vdb
from mathutils import Vector

AXES = np.eye(3)


def vec(value):
    return np.asarray(tuple(value), dtype=np.float64)


def frame(direction, hint=(0.0, 0.0, 1.0)):
    """Orthonormal rows (side, up, forward) with `forward` along `direction`."""
    forward = vec(direction)
    forward = forward / np.linalg.norm(forward)
    hint = vec(hint)
    if abs(float(forward @ hint)) > 0.95:
        hint = vec((1, 0, 0)) if abs(forward[0]) < 0.9 else vec((0, 1, 0))
    side = np.cross(forward, hint)
    side /= np.linalg.norm(side)
    up = np.cross(side, forward)
    return np.stack([side, up, forward])


def rotation(axis, degrees):
    axis = vec(axis) / np.linalg.norm(vec(axis))
    a = math.radians(degrees)
    x, y, z = axis
    c, s, t = math.cos(a), math.sin(a), 1 - math.cos(a)
    return np.array([[t * x * x + c, t * x * y - s * z, t * x * z + s * y],
                     [t * x * y + s * z, t * y * y + c, t * y * z - s * x],
                     [t * x * z - s * y, t * y * z + s * x, t * z * z + c]])


# --- smooth boolean operators ------------------------------------------------

def smin(a, b, k):
    if k <= 0.0:
        return np.minimum(a, b)
    h = np.clip(0.5 + 0.5 * (b - a) / k, 0.0, 1.0)
    return b * (1.0 - h) + a * h - k * h * (1.0 - h)


def smax(a, b, k):
    return -smin(-a, -b, k)


# --- primitive distances (P has shape (..., 3)) ------------------------------

def d_sphere(P, centre, radius):
    return np.linalg.norm(P - vec(centre), axis=-1) - radius


def d_ellipsoid(P, centre, radii, axes=AXES):
    """Inigo Quilez's bound for an ellipsoid with orthonormal row `axes`."""
    q = (P - vec(centre)) @ np.asarray(axes, dtype=np.float64).T
    r = vec(radii)
    k0 = np.linalg.norm(q / r, axis=-1)
    k1 = np.linalg.norm(q / (r * r), axis=-1)
    return k0 * (k0 - 1.0) / np.maximum(k1, 1e-9)


def d_round_cone(P, a, b, ra, rb):
    """Exact distance to the hull of two spheres (a, ra) and (b, rb)."""
    a, b = vec(a), vec(b)
    ba = b - a
    l2 = float(ba @ ba)
    if l2 < 1e-12:
        return d_sphere(P, a, max(ra, rb))
    rr = ra - rb
    a2 = l2 - rr * rr
    il2 = 1.0 / l2
    pa = P - a
    y = pa @ ba
    z = y - l2
    x = pa * l2 - y[..., None] * ba
    x2 = np.sum(x * x, axis=-1)
    y2 = y * y * l2
    z2 = z * z * l2
    k = math.copysign(1.0, rr) * rr * rr * x2
    out = (np.sqrt(np.maximum(x2 * a2 * il2, 0.0)) + y * rr) * il2 - ra
    first = np.sign(z) * a2 * z2 > k
    second = np.sign(y) * a2 * y2 < k
    out = np.where(first, np.sqrt(np.maximum(x2 + z2, 0.0)) * il2 - rb, out)
    out = np.where(second & ~first, np.sqrt(np.maximum(x2 + y2, 0.0)) * il2 - ra, out)
    return out


def d_chain(P, points, radii):
    out = None
    for index in range(len(points) - 1):
        part = d_round_cone(P, points[index], points[index + 1],
                            radii[index], radii[index + 1])
        out = part if out is None else np.minimum(out, part)
    return out


def d_box(P, centre, half, axes=AXES, round_=0.0):
    q = np.abs((P - vec(centre)) @ np.asarray(axes, dtype=np.float64).T) - (vec(half) - round_)
    outside = np.linalg.norm(np.maximum(q, 0.0), axis=-1)
    inside = np.minimum(np.max(q, axis=-1), 0.0)
    return outside + inside - round_


def d_plane(P, point, normal):
    n = vec(normal)
    return (P - vec(point)) @ (n / np.linalg.norm(n))


# --- noise -------------------------------------------------------------------

def _hash(ix, iy, iz, seed):
    h = (ix.astype(np.int64) * 73856093) ^ (iy.astype(np.int64) * 19349663) ^ \
        (iz.astype(np.int64) * 83492791) ^ (seed * 2654435761)
    h = (h ^ (h >> 13)) * 1274126177
    h = h ^ (h >> 16)
    return (h & 0xFFFFFF).astype(np.float64) / float(0xFFFFFF)


def noise(P, scale, seed):
    """Deterministic smooth lattice noise in [-1, 1]."""
    p = P * scale
    base = np.floor(p)
    f = p - base
    f = f * f * (3.0 - 2.0 * f)
    i = base.astype(np.int64)
    total = 0.0
    for dx in (0, 1):
        for dy in (0, 1):
            for dz in (0, 1):
                w = (f[..., 0] if dx else 1 - f[..., 0]) * \
                    (f[..., 1] if dy else 1 - f[..., 1]) * \
                    (f[..., 2] if dz else 1 - f[..., 2])
                total = total + w * _hash(i[..., 0] + dx, i[..., 1] + dy,
                                          i[..., 2] + dz, seed)
    return total * 2.0 - 1.0


def fbm(P, scale, seed, octaves=4):
    value, amplitude, norm = 0.0, 1.0, 0.0
    for octave in range(octaves):
        value = value + amplitude * noise(P, scale * 2 ** octave, seed + 97 * octave)
        norm += amplitude
        amplitude *= 0.5
    return value / norm


def smoothstep(edge0, edge1, x):
    t = np.clip((x - edge0) / (edge1 - edge0), 0.0, 1.0)
    return t * t * (3.0 - 2.0 * t)


# --- the field ---------------------------------------------------------------

class Field:
    """A signed-distance body on one fixed grid (negative inside)."""

    def __init__(self, lo, hi, voxel, far=0.25):
        self.voxel = float(voxel)
        self.origin = vec(lo)
        self.shape = tuple(int(math.ceil((h - l) / voxel - 1e-9)) + 1
                           for l, h in zip(lo, hi))
        self.far = float(far)
        self.d = np.full(self.shape, self.far, dtype=np.float32)
        self.mat = np.zeros(self.shape, dtype=np.int16)

    def grid(self):
        """World coordinates of every voxel (float32, indexing ij)."""
        axes = [(self.origin[k] + self.voxel * np.arange(self.shape[k])).astype(np.float32)
                for k in range(3)]
        return np.meshgrid(*axes, indexing="ij")

    def _block(self, lo, hi):
        size = np.array(self.shape)
        i0 = np.clip(np.floor((vec(lo) - self.origin) / self.voxel).astype(int), 0, size)
        i1 = np.clip(np.ceil((vec(hi) - self.origin) / self.voxel).astype(int) + 1, 0, size)
        if np.any(i1 <= i0):
            return None, None
        sl = tuple(slice(int(a), int(b)) for a, b in zip(i0, i1))
        axes = [self.origin[k] + self.voxel * np.arange(i0[k], i1[k]) for k in range(3)]
        X, Y, Z = np.meshgrid(*axes, indexing="ij")
        return sl, np.stack([X, Y, Z], axis=-1)

    def _apply(self, distance, lo, hi, op, k, material):
        sl, P = self._block(lo, hi)
        if sl is None:
            return
        new = distance(P).astype(np.float64)
        old = self.d[sl].astype(np.float64)
        if op == "union":
            out = smin(new, old, k)
            if material is not None:
                self.mat[sl] = np.where(new < old, material, self.mat[sl])
        elif op == "cut":
            out = smax(old, -new, k)
            if material is not None:
                self.mat[sl] = np.where((-new > old - k) & (old < self.voxel * 3),
                                        material, self.mat[sl])
        elif op == "intersect":
            out = smax(old, new, k)
        else:
            raise ValueError(op)
        self.d[sl] = out.astype(np.float32)

    # Bounds are each primitive's box grown by the blend width and a margin,
    # so a smooth operator never changes the field outside its block.
    def _margin(self, k):
        return k + 3.0 * self.voxel

    def ellipsoid(self, centre, radii, axes=AXES, op="union", k=0.0, material=None):
        m = max(radii) + self._margin(k)
        c = vec(centre)
        self._apply(lambda P: d_ellipsoid(P, c, radii, axes), c - m, c + m, op, k, material)

    def sphere(self, centre, radius, op="union", k=0.0, material=None):
        self.ellipsoid(centre, (radius,) * 3, AXES, op, k, material)

    def cone(self, a, b, ra, rb, op="union", k=0.0, material=None):
        a, b = vec(a), vec(b)
        m = max(ra, rb) + self._margin(k)
        self._apply(lambda P: d_round_cone(P, a, b, ra, rb),
                    np.minimum(a, b) - m, np.maximum(a, b) + m, op, k, material)

    def chain(self, points, radii, op="union", k=0.0, material=None):
        points = [vec(p) for p in points]
        m = max(radii) + self._margin(k)
        lo = np.min(points, axis=0) - m
        hi = np.max(points, axis=0) + m
        self._apply(lambda P: d_chain(P, points, radii), lo, hi, op, k, material)

    def box(self, centre, half, axes=AXES, round_=0.0, op="union", k=0.0, material=None):
        c = vec(centre)
        m = float(np.linalg.norm(vec(half))) + self._margin(k)
        self._apply(lambda P: d_box(P, c, half, axes, round_), c - m, c + m, op, k, material)

    def custom(self, distance, lo, hi, op="union", k=0.0, material=None):
        m = self._margin(k)
        self._apply(distance, vec(lo) - m, vec(hi) + m, op, k, material)

    def displace(self, offset, lo, hi):
        """Add `offset(P)` (metres, positive moves the surface inward)."""
        sl, P = self._block(lo, hi)
        if sl is None:
            return
        self.d[sl] = (self.d[sl].astype(np.float64) + offset(P)).astype(np.float32)

    def paint(self, inside, lo, hi, material):
        """Material only: voxels where `inside(P)` is true take `material`."""
        sl, P = self._block(lo, hi)
        if sl is None:
            return
        self.mat[sl] = np.where(inside(P), material, self.mat[sl])

    # --- sampling --------------------------------------------------------

    def _index(self, points):
        return (np.asarray(points, dtype=np.float64) - self.origin) / self.voxel

    def sample(self, points):
        """Trilinear distance at arbitrary points (far outside the grid)."""
        q = self._index(points)
        size = np.array(self.shape) - 1
        base = np.clip(np.floor(q).astype(int), 0, size - 1)
        f = np.clip(q - base, 0.0, 1.0)
        out = np.zeros(len(q))
        for dx in (0, 1):
            for dy in (0, 1):
                for dz in (0, 1):
                    w = (f[:, 0] if dx else 1 - f[:, 0]) * \
                        (f[:, 1] if dy else 1 - f[:, 1]) * \
                        (f[:, 2] if dz else 1 - f[:, 2])
                    out += w * self.d[base[:, 0] + dx, base[:, 1] + dy, base[:, 2] + dz]
        outside = np.any((q < 0) | (q > size), axis=1)
        return np.where(outside, self.far, out)

    def material_at(self, points, normals=None, depth=None):
        """Material of the voxel just inside the surface at each point."""
        points = np.asarray(points, dtype=np.float64)
        if normals is not None:
            points = points - np.asarray(normals) * (depth or self.voxel)
        q = np.clip(np.rint(self._index(points)).astype(int), 0,
                    np.array(self.shape) - 1)
        return self.mat[q[:, 0], q[:, 1], q[:, 2]].astype(int)

    def cavity(self, points, normals, steps=(0.01, 0.02, 0.04, 0.07)):
        """Ambient occlusion in [0, 1] (1 = open) from the field itself."""
        points = np.asarray(points, dtype=np.float64)
        normals = np.asarray(normals, dtype=np.float64)
        occlusion = np.zeros(len(points))
        weight = 1.0
        for step in steps:
            d = self.sample(points + normals * step)
            occlusion += weight * np.clip((step - d) / step, 0.0, 1.0)
            weight *= 0.6
        return np.clip(1.0 - occlusion / 2.2, 0.0, 1.0)

    # --- surface ---------------------------------------------------------

    def polygonise(self, name, largest_only=False):
        """The zero surface as a Blender object, in canonical element order."""
        grid = vdb.FloatGrid(self.far)
        grid.copyFromArray(self.d, ijk=(0, 0, 0), tolerance=0.0)
        points, quads = grid.convertToQuads(isovalue=0.0)
        points = self.origin + np.asarray(points, dtype=np.float64) * self.voxel
        quads = np.asarray(quads, dtype=np.int64)
        if len(quads) == 0:
            raise SystemExit("signed-distance body has no surface: " + name)
        # Weld coincident points, then order everything canonically.
        key = np.round(points, 7)
        unique, inverse = np.unique(key, axis=0, return_inverse=True)
        quads = inverse.reshape(-1)[quads]
        order = np.lexsort((unique[:, 2], unique[:, 1], unique[:, 0]))
        rank = np.empty(len(order), dtype=np.int64)
        rank[order] = np.arange(len(order))
        verts = unique[order]
        quads = rank[quads]
        keep = np.array([len(set(row)) == 4 for row in quads.tolist()], dtype=bool)
        quads = quads[keep]
        # Start each face at its smallest vertex (winding kept), then sort.
        start = np.argmin(quads, axis=1)
        roll = (np.arange(4)[None, :] + start[:, None]) % 4
        quads = np.take_along_axis(quads, roll, axis=1)
        quads = quads[np.lexsort(quads.T[::-1])]
        mesh = bpy.data.meshes.new(name)
        mesh.from_pydata([tuple(p) for p in verts.tolist()], [], quads.tolist())
        mesh.validate(clean_customdata=False)
        mesh.update()
        obj = bpy.data.objects.new(name, mesh)
        bpy.context.collection.objects.link(obj)
        bm = bmesh.new()
        bm.from_mesh(mesh)
        manifold = all(len(e.link_faces) == 2 for e in bm.edges) and             all(v.link_faces for v in bm.verts)
        bm.free()
        if not manifold:
            # OpenVDB's quads can pinch at ambiguous cells; a voxel remesh at
            # the field's own resolution closes them into one manifold surface.
            for other in bpy.context.selected_objects:
                other.select_set(False)
            obj.select_set(True)
            bpy.context.view_layer.objects.active = obj
            remesh = obj.modifiers.new("Manifold", "REMESH")
            remesh.mode = "VOXEL"
            remesh.voxel_size = self.voxel
            remesh.adaptivity = 0.0
            bpy.ops.object.modifier_apply(modifier=remesh.name)
        obj["kmg_vdb_manifold"] = bool(manifold)
        if largest_only:
            # Thin carved features can leave a detached sliver inside the
            # surface; the continuous body keeps only its largest island.
            bm = bmesh.new()
            bm.from_mesh(obj.data)
            bm.verts.ensure_lookup_table()
            seen, islands = set(), []
            for start in bm.verts:
                if start.index in seen:
                    continue
                stack, island = [start], []
                seen.add(start.index)
                while stack:
                    vert = stack.pop()
                    island.append(vert)
                    for edge in vert.link_edges:
                        other = edge.other_vert(vert)
                        if other.index not in seen:
                            seen.add(other.index)
                            stack.append(other)
                islands.append(island)
            islands.sort(key=len, reverse=True)
            drop = [v for island in islands[1:] for v in island]
            obj["kmg_dropped_island_vertices"] = len(drop)
            if drop:
                bmesh.ops.delete(bm, geom=drop, context="VERTS")
            bm.to_mesh(obj.data)
            bm.free()
            obj.data.update()
        return obj


def closest_rows(points, source_points, source_rows):
    """Weights for a dependent surface (cloth, fused accessories) from the
    nearest body vertices, averaged over the closest four by inverse distance."""
    source_points = np.asarray(source_points, dtype=np.float64)
    out = {}
    for index, p in enumerate(np.asarray(points, dtype=np.float64)):
        gap = np.linalg.norm(source_points - p, axis=1)
        near = np.argpartition(gap, 4)[:4]
        total = {}
        weights = 1.0 / np.maximum(gap[near], 1e-4)
        weights /= weights.sum()
        for w, i in zip(weights, near):
            for name, value in source_rows[int(i)]:
                total[name] = total.get(name, 0.0) + w * value
        row = sorted(total.items(), key=lambda e: (-e[1], e[0]))[:4]
        s = sum(v for _, v in row)
        out[index] = [(n, v / s) for n, v in row]
    return out
