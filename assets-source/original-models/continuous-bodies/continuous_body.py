"""Continuous skinned bodies for the Expanded Summoning visual refinement.

The first original creatures were assembled from separate tubes and
ellipsoids, each rigidly owned by one bone. That construction left seams at
every joint, and the shared exporter reversed every triangle on the theory
that the donor renderer's space mirrors Blender's. It does not: the runtime
assigns positions and indices verbatim and every donor frame measured so far
is a proper rotation of Unity's, so Unity and Blender agree that the front of
triangle (a, b, c) is Cross(b - a, c - a). The reversal turned each outward
tube wall inside out and each inward cap outward, and the exported normals,
which were not reversed, then lit every visible face from behind.

This module replaces both halves for the refinement pilot only:

- A body is ONE closed surface: an implicit (metaball) volume through the
  measured donor joints, polygonised, reduced and smoothed, so limbs, neck and
  tail grow out of the torso instead of meeting it at a cap.
- Weights come from Blender's bone-heat solve over that surface, so every
  joint blends instead of hinging between two rigid shells.
- Small rigid features (horns, teeth, claws, eyes, ears, whiskers) are closed,
  outward-facing solids rooted inside the surface and owned by one bone.
- Export keeps Blender's winding and normals, splits vertices only where the
  texture seam requires it, and refuses a component that is open, wound
  inconsistently or not outward.

No legacy generator imports this file, so it cannot change their output.
"""
import base64
import hashlib
import json
import math
import struct
import zlib
from pathlib import Path

import bpy
import bmesh
import numpy as np
from mathutils import Matrix, Vector


def log(message):
    print("[continuous-body] " + message)


# --- rig -------------------------------------------------------------------

def review_armature(rig_data, name, leaf_fraction=0.6, bridge=None):
    """A preview armature in the donor frame. Leaf bones get a tail along the
    parent direction so the heat solve sees a real segment rather than a
    0.05-unit stub (the runtime never reads these tails). `bridge` maps a
    parent the capture does not contain to its nearest captured ancestor,
    so offline poses move a foot with its leg as the donor hierarchy does."""
    bridge = bridge or {}
    rows = {row["name"]: dict(row) for row in rig_data["bones"]}
    for row in rows.values():
        if row["parent"] not in rows and row["parent"] in bridge:
            row["parent"] = bridge[row["parent"]]
    rig_data = dict(rig_data, bones=list(rows.values()))
    children = {}
    for row in rig_data["bones"]:
        if row["parent"] in rows:
            children.setdefault(row["parent"], []).append(row["name"])
    armature = bpy.data.armatures.new(name)
    obj = bpy.data.objects.new(name, armature)
    bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode="EDIT")
    made = {}
    for row in sorted(rig_data["bones"], key=lambda value: value["depth"]):
        head = Vector(row["head"])
        if children.get(row["name"]):
            tail = sum((Vector(rows[c]["head"]) for c in children[row["name"]]),
                       Vector()) / len(children[row["name"]])
        elif row["parent"] in rows:
            parent = Vector(rows[row["parent"]]["head"])
            span = head - parent
            tail = head + span * leaf_fraction if span.length > 1e-6 else \
                head + Vector((0.0, 0.0, 0.05))
        else:
            tail = head + Vector((0.0, 0.0, 0.05))
        if (tail - head).length < 1e-5:
            tail = head + Vector((0.0, 0.0, 0.05))
        bone = armature.edit_bones.new(row["name"])
        bone.head = head
        bone.tail = tail
        made[row["name"]] = bone
    for row in rig_data["bones"]:
        if row["parent"] in made:
            made[row["name"]].parent = made[row["parent"]]
    bpy.ops.object.mode_set(mode="OBJECT")
    return obj


# --- implicit volume --------------------------------------------------------

class Volume:
    """Closed primitives at their exact size, merged into one surface.

    A first version blended metaballs; overlapping fields add, so dense limb
    sweeps and overlapping body masses swelled far past their stated radii
    (a Rhinoceros head swallowed its own horns). The body is now the exact
    union of closed ellipsoids and capsule chains, computed by a voxel
    remesh, and finish_surface rounds the creases where they meet."""

    def __init__(self, resolution):
        self.resolution = resolution
        self.bm = bmesh.new()

    def ball(self, centre, radius):
        self.ellipsoid(centre, (Vector((1, 0, 0)), Vector((0, 1, 0)),
                                Vector((0, 0, 1))), (radius,) * 3)

    def ellipsoid(self, centre, axes, radii, segments=28, rings=16):
        """`axes` are three orthonormal vectors; `radii` the half extents."""
        bm = self.bm
        transform = Matrix((Vector(axes[0]) * radii[0],
                            Vector(axes[1]) * radii[1],
                            Vector(axes[2]) * radii[2])).transposed()
        place = lambda unit: bm.verts.new(Vector(centre) + transform @ unit)
        top = place(Vector((0.0, 0.0, 1.0)))
        rows = []
        for ring in range(1, rings):
            polar = math.pi * ring / rings
            rows.append([place(Vector((math.sin(polar) * math.cos(a),
                                       math.sin(polar) * math.sin(a),
                                       math.cos(polar))))
                         for a in (2.0 * math.pi * s / segments
                                   for s in range(segments))])
        bottom = place(Vector((0.0, 0.0, -1.0)))
        for step in range(segments):
            nxt = (step + 1) % segments
            bm.faces.new((top, rows[0][step], rows[0][nxt]))
            for ring in range(len(rows) - 1):
                bm.faces.new((rows[ring][step], rows[ring + 1][step],
                              rows[ring + 1][nxt], rows[ring][nxt]))
            bm.faces.new((bottom, rows[-1][nxt], rows[-1][step]))

    def sweep(self, points, radii, segments=24):
        """A capsule chain: a sphere at every point and a closed frustum
        between neighbours, so a limb tapers smoothly with no beads."""
        points = [Vector(p) for p in points]
        for point, radius in zip(points, radii):
            self.ball(point, radius)
        for index in range(len(points) - 1):
            self._frustum(points[index], points[index + 1], radii[index],
                          radii[index + 1], segments)

    def _frustum(self, a, b, ra, rb, segments):
        if (b - a).length < 1e-9:
            return
        side, up, _ = frame(b - a)
        bm = self.bm
        rows = []
        for centre, radius in ((a, ra), (b, rb)):
            rows.append([bm.verts.new(centre + side * math.cos(t) * radius +
                                      up * math.sin(t) * radius)
                         for t in (2.0 * math.pi * s / segments
                                   for s in range(segments))])
        for step in range(segments):
            nxt = (step + 1) % segments
            bm.faces.new((rows[0][step], rows[0][nxt], rows[1][nxt],
                          rows[1][step]))
        bm.faces.new(tuple(reversed(rows[0])))
        bm.faces.new(tuple(rows[1]))

    def shell(self, obj):
        """Add a closed surface built and relaxed on its own (a torso blended
        at a coarser scale than the limbs) and remove its object."""
        verts = [self.bm.verts.new(vertex.co) for vertex in obj.data.vertices]
        for polygon in obj.data.polygons:
            self.bm.faces.new(tuple(verts[index] for index in polygon.vertices))
        mesh = obj.data
        bpy.data.objects.remove(obj)
        bpy.data.meshes.remove(mesh)

    def polygonise(self, name):
        mesh = bpy.data.meshes.new(name)
        self.bm.to_mesh(mesh)
        self.bm.free()
        obj = bpy.data.objects.new(name, mesh)
        bpy.context.collection.objects.link(obj)
        for other in bpy.context.selected_objects:
            other.select_set(False)
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        remesh = obj.modifiers.new("Union", "REMESH")
        remesh.mode = "VOXEL"
        remesh.voxel_size = self.resolution
        remesh.adaptivity = 0.0
        bpy.ops.object.modifier_apply(modifier=remesh.name)
        return obj


def fan_triangles(bm):
    """A new BMesh with the same vertices in index order and every face
    split as a fixed fan in face-index order. bmesh.ops.triangulate is not
    used: its output face order varies between runs on 4.5.10."""
    bm.verts.index_update()
    out = bmesh.new()
    verts = [out.verts.new(vertex.co) for vertex in bm.verts]
    triangles, uses = [], {}
    for face in bm.faces:
        ids = [loop.vert.index for loop in face.loops]
        for corner in range(1, len(ids) - 1):
            triangle = (ids[0], ids[corner], ids[corner + 1])
            triangles.append(triangle)
            key = tuple(sorted(triangle))
            uses[key] = uses.get(key, 0) + 1
    # Decimating a dense, lumpy union (a bison's shaggy coat) can collapse a
    # thin region into two faces on the same three vertices: a zero-volume
    # fin. Both are dropped with any vertex they orphan; orient_outward
    # still refuses anything that is not then a closed, consistent shell.
    for triangle in triangles:
        if uses[tuple(sorted(triangle))] == 1:
            out.faces.new(tuple(verts[index] for index in triangle))
    orphans = [vertex for vertex in out.verts if not vertex.link_faces]
    if orphans:
        bmesh.ops.delete(out, geom=orphans, context="VERTS")
    bm.free()
    return out


def orient_outward(bm):
    """Each closed island outward by its exact signed volume.

    Every part here is built with consistent winding, so this only decides
    the side. Blender's recalc_face_normals is not used: measured on 4.5.10
    it picks the wrong side for small closed shapes far from the origin (the
    same ellipsoid is outward at the origin and inward 1 m away in the
    centimetre Dog frame) and does not choose the same loop order from run
    to run, which reorders the exported vertices. An island whose winding
    is inconsistent is refused rather than guessed at."""
    # Fresh BMesh elements carry no valid index until this is called; the
    # island walk below keys on it.
    bm.verts.index_update()
    bm.edges.index_update()
    bm.faces.index_update()
    bm.faces.ensure_lookup_table()
    bm.edges.ensure_lookup_table()
    for edge in bm.edges:
        faces = edge.link_faces
        if len(faces) != 2:
            raise SystemExit("surface is not closed and manifold at an edge")
        a, b = edge.verts
        forward = sum(1 for face in faces for loop in face.loops
                      if loop.vert == a and loop.link_loop_next.vert == b)
        if forward != 1:
            raise SystemExit("surface winding is inconsistent at an edge")
    seen = set()
    for start in bm.faces:
        if start.index in seen:
            continue
        island, stack = [], [start]
        seen.add(start.index)
        while stack:
            face = stack.pop()
            island.append(face)
            for edge in face.edges:
                for other in edge.link_faces:
                    if other.index not in seen:
                        seen.add(other.index)
                        stack.append(other)
        verts = {v for f in island for v in f.verts}
        centre = sum((v.co for v in verts), Vector()) / len(verts)
        volume = 0.0
        for face in island:
            corners = [loop.vert.co - centre for loop in face.loops]
            for i in range(1, len(corners) - 1):
                volume += corners[0].dot(corners[i].cross(corners[i + 1]))
        if volume < 0.0:
            bmesh.ops.reverse_faces(bm, faces=island)
    bm.normal_update()


def relax(obj, iterations):
    """Volume-preserving Laplacian passes. Each pass reaches about one voxel,
    so masses meeting over tenths of a metre are relaxed on a coarse union
    (see Volume.shell); a bison's hump and cape kept their creases through
    44 passes at limb resolution."""
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    for _ in range(iterations):
        bmesh.ops.smooth_laplacian_vert(
            bm, verts=bm.verts, lambda_factor=0.6, lambda_border=0.0,
            use_x=True, use_y=True, use_z=True, preserve_volume=True)
    bm.to_mesh(obj.data)
    bm.free()
    obj.data.update()
    return obj


def finish_surface(obj, target_triangles, smooth_iterations=6,
                   voxel_size=None):
    """Relax, reduce and orient the union surface into one closed,
    consistently outward shell of roughly `target_triangles` triangles.
    Volume-preserving Laplacian passes on the dense union round the creases
    where parts meet into continuous fillets before the reduction."""
    bpy.context.view_layer.objects.active = obj
    for other in bpy.context.selected_objects:
        other.select_set(False)
    obj.select_set(True)
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    for _ in range(smooth_iterations):
        bmesh.ops.smooth_laplacian_vert(
            bm, verts=bm.verts, lambda_factor=0.6, lambda_border=0.0,
            use_x=True, use_y=True, use_z=True, preserve_volume=True)
    bm.to_mesh(obj.data)
    bm.free()
    faces = sum(len(p.vertices) - 2 for p in obj.data.polygons)
    if faces > target_triangles:
        decimate = obj.modifiers.new("Decimate", "DECIMATE")
        decimate.decimate_type = "COLLAPSE"
        decimate.ratio = target_triangles / float(faces)
        decimate.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=decimate.name)
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-6)
    bm = fan_triangles(bm)
    for _ in range(2):
        bmesh.ops.smooth_vert(bm, verts=bm.verts, factor=0.35,
                              use_axis_x=True, use_axis_y=True, use_axis_z=True)
    orient_outward(bm)
    bm.to_mesh(obj.data)
    bm.free()
    obj.data.update()
    return obj


# --- rigid features --------------------------------------------------------

def frame(direction, hint=Vector((0.0, 0.0, 1.0))):
    forward = Vector(direction).normalized()
    if abs(forward.dot(hint)) > 0.95:
        hint = Vector((1.0, 0.0, 0.0)) if abs(forward.x) < 0.9 else \
            Vector((0.0, 1.0, 0.0))
    side = forward.cross(hint).normalized()
    up = side.cross(forward).normalized()
    return side, up, forward


def horn(name, base, tip, base_radius, segments=12, rings=6, curve=None,
         flatten=1.0, hint=Vector((0.0, 0.0, 1.0))):
    """A closed, tapering, optionally curved spike from `base` to `tip`.
    `curve` offsets the mid-line by that vector times sin(pi * t)."""
    bm = bmesh.new()
    base, tip = Vector(base), Vector(tip)
    side, up, forward = frame(tip - base, hint)
    rows = []
    for ring in range(rings):
        t = ring / float(rings)
        centre = base.lerp(tip, t)
        if curve is not None:
            centre = centre + Vector(curve) * math.sin(math.pi * t)
        radius = base_radius * (1.0 - t) ** 0.9
        row = []
        for step in range(segments):
            angle = 2.0 * math.pi * step / segments
            row.append(bm.verts.new(centre + side * math.cos(angle) * radius +
                                    up * math.sin(angle) * radius * flatten))
        rows.append(row)
    apex = bm.verts.new(tip)
    floor = bm.verts.new(base - forward * base_radius * 0.2)
    for ring in range(rings - 1):
        for step in range(segments):
            nxt = (step + 1) % segments
            bm.faces.new((rows[ring][step], rows[ring][nxt],
                          rows[ring + 1][nxt], rows[ring + 1][step]))
    for step in range(segments):
        nxt = (step + 1) % segments
        bm.faces.new((rows[-1][step], rows[-1][nxt], apex))
        bm.faces.new((rows[0][nxt], rows[0][step], floor))
    return _rigid_object(name, bm)


def blob(name, centre, axes, radii, segments=16, rings=10):
    """A closed ellipsoid with explicit axes (any handedness), built in a
    fixed element order (create_uvsphere's face order varies between runs)."""
    bm = bmesh.new()
    transform = Matrix((Vector(axes[0]) * radii[0], Vector(axes[1]) * radii[1],
                        Vector(axes[2]) * radii[2])).transposed()
    place = lambda unit: bm.verts.new(Vector(centre) + transform @ unit)
    top = place(Vector((0.0, 0.0, 1.0)))
    rows = []
    for ring in range(1, rings):
        polar = math.pi * ring / rings
        rows.append([place(Vector((math.sin(polar) * math.cos(a),
                                   math.sin(polar) * math.sin(a),
                                   math.cos(polar))))
                     for a in (2.0 * math.pi * s / segments
                               for s in range(segments))])
    bottom = place(Vector((0.0, 0.0, -1.0)))
    for step in range(segments):
        nxt = (step + 1) % segments
        bm.faces.new((top, rows[0][step], rows[0][nxt]))
        for ring in range(len(rows) - 1):
            bm.faces.new((rows[ring][step], rows[ring + 1][step],
                          rows[ring + 1][nxt], rows[ring][nxt]))
        bm.faces.new((bottom, rows[-1][nxt], rows[-1][step]))
    return _rigid_object(name, bm)


def strand(name, points, radii, segments=6):
    """A closed thin tube through `points` (whiskers, tail tips, claws)."""
    bm = bmesh.new()
    points = [Vector(p) for p in points]
    rows = []
    for index, centre in enumerate(points):
        if index == 0:
            direction = points[1] - points[0]
        elif index == len(points) - 1:
            direction = points[-1] - points[-2]
        else:
            direction = points[index + 1] - points[index - 1]
        side, up, _ = frame(direction)
        rows.append([bm.verts.new(centre + side * math.cos(a) * radii[index] +
                                  up * math.sin(a) * radii[index])
                     for a in (2.0 * math.pi * s / segments
                               for s in range(segments))])
    for ring in range(len(rows) - 1):
        for step in range(segments):
            nxt = (step + 1) % segments
            bm.faces.new((rows[ring][step], rows[ring][nxt],
                          rows[ring + 1][nxt], rows[ring + 1][step]))
    start = bm.verts.new(points[0] - (points[1] - points[0]).normalized() *
                         radii[0] * 0.5)
    end = bm.verts.new(points[-1] + (points[-1] - points[-2]).normalized() *
                       radii[-1] * 0.5)
    for step in range(segments):
        nxt = (step + 1) % segments
        bm.faces.new((rows[0][nxt], rows[0][step], start))
        bm.faces.new((rows[-1][step], rows[-1][nxt], end))
    return _rigid_object(name, bm)


def tube(name, points, radii, segments=8):
    """Like `strand`, but each ring's frame is carried along the path by
    parallel transport. `strand` frames every ring on its own, so a sharply
    bent path (a raptor's folded arm) twists between rings and folds its
    own faces; `strand` stays as it is for the bodies already built with it."""
    bm = bmesh.new()
    points = [Vector(p) for p in points]
    tangents = []
    for index in range(len(points)):
        if index == 0:
            direction = points[1] - points[0]
        elif index == len(points) - 1:
            direction = points[-1] - points[-2]
        else:
            direction = (points[index + 1] - points[index]).normalized() + \
                (points[index] - points[index - 1]).normalized()
        tangents.append(direction.normalized())
    side, up, _ = frame(tangents[0])
    rows = []
    for index, centre in enumerate(points):
        if index > 0:
            turn = tangents[index - 1].rotation_difference(tangents[index])
            side, up = turn @ side, turn @ up
        rows.append([bm.verts.new(centre + side * math.cos(a) * radii[index] +
                                  up * math.sin(a) * radii[index])
                     for a in (2.0 * math.pi * s / segments
                               for s in range(segments))])
    for ring in range(len(rows) - 1):
        for step in range(segments):
            nxt = (step + 1) % segments
            bm.faces.new((rows[ring][step], rows[ring][nxt],
                          rows[ring + 1][nxt], rows[ring + 1][step]))
    start = bm.verts.new(points[0] - tangents[0] * radii[0] * 0.5)
    end = bm.verts.new(points[-1] + tangents[-1] * radii[-1] * 0.5)
    for step in range(segments):
        nxt = (step + 1) % segments
        bm.faces.new((rows[0][nxt], rows[0][step], start))
        bm.faces.new((rows[-1][step], rows[-1][nxt], end))
    return _rigid_object(name, bm)


def _rigid_object(name, bm):
    bm = fan_triangles(bm)
    orient_outward(bm)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    return obj


# --- weights ---------------------------------------------------------------

def heat_weights(obj, armature, allowed):
    """Blender's bone-heat solve over the closed body, restricted to the
    reviewed donor bones. Returns {vertex index: [(bone, weight), ...]}."""
    for bone in armature.data.bones:
        bone.use_deform = bone.name in allowed
    for other in bpy.context.selected_objects:
        other.select_set(False)
    obj.select_set(True)
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.object.parent_set(type="ARMATURE_AUTO")
    names = {group.index: group.name for group in obj.vertex_groups}
    rows = {}
    for vertex in obj.data.vertices:
        rows[vertex.index] = [(names[g.group], g.weight) for g in vertex.groups
                              if g.weight > 0.0 and names[g.group] in allowed]
    obj.parent = None
    for modifier in list(obj.modifiers):
        obj.modifiers.remove(modifier)
    obj.vertex_groups.clear()
    # Unity skins every bound control; Blender's armature modifier ignores a
    # vertex group whose bone is not a deform bone. Restore the flag so the
    # saved source poses offline with exactly the weights that ship (a
    # first review saw rigid claws, jaws and arms left behind otherwise).
    for bone in armature.data.bones:
        bone.use_deform = True
    return rows


def nearest_bone(point, armature, allowed):
    best, distance = None, float("inf")
    for bone in armature.data.bones:
        if bone.name not in allowed:
            continue
        head, tail = bone.head_local, bone.tail_local
        segment = tail - head
        t = 0.0 if segment.length_squared < 1e-12 else max(0.0, min(1.0, (
            point - head).dot(segment) / segment.length_squared))
        gap = (head + segment * t - point).length
        if gap < distance:
            best, distance = bone.name, gap
    return best


def clean_weights(rows, positions, armature, allowed, limit=4, floor=0.02):
    """At most `limit` influences, none below `floor`, summing to one; a
    vertex the heat solve left empty takes its nearest allowed bone."""
    cleaned = {}
    for index, row in rows.items():
        row = sorted(((name, value) for name, value in row if value > 0.0),
                     key=lambda entry: (-entry[1], entry[0]))[:limit]
        total = sum(value for _, value in row)
        if total > 0:
            row = [(name, value / total) for name, value in row]
            row = [(name, value) for name, value in row if value >= floor]
        if not row:
            row = [(nearest_bone(positions[index], armature, allowed), 1.0)]
        total = sum(value for _, value in row)
        cleaned[index] = [(name, value / total) for name, value in row]
    return cleaned


def smooth_weights(obj, rows, iterations=4, factor=0.5):
    """Laplacian passes of the weights over the body's edges, in vertex
    order, so the transitions the heat solve leaves sharp (where a neck
    meets a hump, or a limb root meets the barrel) blend instead of
    creasing under linear-blend skinning. Geometry does not move."""
    mesh = obj.data
    neighbours = {index: [] for index in rows}
    for edge in mesh.edges:
        a, b = edge.vertices
        if a in neighbours and b in neighbours:
            neighbours[a].append(b)
            neighbours[b].append(a)
    current = {index: dict(row) for index, row in rows.items()}
    for _ in range(iterations):
        updated = {}
        for index in sorted(current):
            around = neighbours[index]
            if not around:
                updated[index] = current[index]
                continue
            mean = {}
            for other in around:
                for name, value in current[other].items():
                    mean[name] = mean.get(name, 0.0) + value / len(around)
            names = set(mean) | set(current[index])
            updated[index] = {name: (1.0 - factor) * current[index].get(name, 0.0)
                              + factor * mean.get(name, 0.0) for name in names}
        current = updated
    return {index: sorted(((n, v) for n, v in row.items() if v > 0.0),
                          key=lambda e: (-e[1], e[0]))
            for index, row in current.items()}


def blend_toward(rows, positions, selector, target, amount):
    """Move `amount(position)` of each selected vertex's weight onto
    `target`. Used for species-local damping (e.g. tiny theropod arms)."""
    for index, row in rows.items():
        if not selector(positions[index], row):
            continue
        share = max(0.0, min(1.0, amount(positions[index])))
        if share <= 0.0:
            continue
        scaled = [(name, value * (1.0 - share)) for name, value in row]
        merged = {}
        for name, value in scaled + [(target, share)]:
            merged[name] = merged.get(name, 0.0) + value
        rows[index] = sorted(merged.items(), key=lambda e: (-e[1], e[0]))
    return rows


# --- assembly --------------------------------------------------------------

REGION_LAYER = "kmg_region"


def tag_region(obj, region):
    """`region` is one id for the whole part or a {vertex: id} table."""
    mesh = obj.data
    layer = mesh.attributes.get(REGION_LAYER) or \
        mesh.attributes.new(REGION_LAYER, "INT", "POINT")
    for index, item in enumerate(layer.data):
        item.value = region[index] if isinstance(region, dict) else region
    return obj


def regions_by_weight(rows, default, rules):
    """Per-vertex regions from skin weights: the first (bones, region) rule
    whose bones carry at least half of a vertex's weight wins."""
    table = {}
    for index, row in rows.items():
        table[index] = default
        for bones, region in rules:
            if sum(value for name, value in row if name in bones) >= 0.5:
                table[index] = region
                break
    return table


def assemble(name, parts):
    """Merge (object, weights {local index: rows}, region) parts into one
    mesh IN THE GIVEN ORDER and return it with its merged weight table.

    The join operator appends parts in Blender's own selection order, which
    varies between runs, while the weight table assumes this order; the merge
    is therefore explicit and every merged vertex is checked against the part
    it came from."""
    bm = bmesh.new()
    weights, regions, expected = {}, {}, []
    body_vertices = len(parts[0][0].data.vertices)
    for obj, rows, region in parts:
        offset = len(bm.verts)
        bm.from_mesh(obj.data)
        for vertex in obj.data.vertices:
            index = offset + vertex.index
            weights[index] = rows[vertex.index]
            regions[index] = region[vertex.index] \
                if isinstance(region, dict) else region
            expected.append(vertex.co.copy())
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    if len(mesh.vertices) != len(expected) or any(
            (vertex.co - expected[vertex.index]).length > 1e-9
            for vertex in mesh.vertices):
        raise SystemExit("merged vertex order does not match the parts")
    for obj, _, _ in parts:
        data = obj.data
        bpy.data.objects.remove(obj)
        bpy.data.meshes.remove(data)
    target = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(target)
    tag_region(target, regions)
    # The first part is the continuous body; its vertices come first.
    target["kmg_body_vertices"] = body_vertices
    return target, weights


def require_single_body(obj, region=None):
    """The continuous body must be ONE closed island: a limb, paw or tail
    volume that failed to merge would otherwise pass every orientation
    check as a separate, detached solid. The body is the first merged part,
    whatever colour regions its vertices carry."""
    mesh = obj.data
    body = list(range(int(obj["kmg_body_vertices"])))
    parent = {index: index for index in body}

    def find(x):
        while parent[x] != x:
            parent[x] = parent[parent[x]]
            x = parent[x]
        return x
    for edge in mesh.edges:
        a, b = edge.vertices
        if a in parent and b in parent:
            parent[find(a)] = find(b)
    roots = {}
    for index in body:
        roots[find(index)] = roots.get(find(index), 0) + 1
    if len(roots) != 1:
        raise SystemExit("continuous body split into islands of " +
                         ", ".join(str(n) for n in sorted(roots.values())) +
                         " vertices")


def attach_albedo_preview(obj, albedo):
    """Review-only material; the runtime builds its own from the donor's."""
    material = bpy.data.materials.new(obj.name + "AlbedoPreview")
    material.use_nodes = True
    nodes = material.node_tree.nodes
    image = nodes.new("ShaderNodeTexImage")
    image.image = bpy.data.images.load(str(Path(albedo).resolve()))
    material.node_tree.links.new(image.outputs["Color"],
                                 nodes["Principled BSDF"].inputs["Base Color"])
    material.use_backface_culling = True
    obj.data.materials.clear()
    obj.data.materials.append(material)
    # Review only: the runtime lights the exported smooth vertex normals.
    obj.data.shade_smooth()


def rigid_rows(obj, bones):
    """Every vertex of a rigid feature on fixed (bone, weight) pairs."""
    return {vertex.index: list(bones) for vertex in obj.data.vertices}


def unwrap(obj, angle_limit=1.15, margin=0.006):
    for other in bpy.context.selected_objects:
        other.select_set(False)
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=angle_limit, island_margin=margin,
                             area_weight=0.0, correct_aspect=True,
                             scale_to_bounds=False)
    bpy.ops.object.mode_set(mode="OBJECT")
    return obj.data.uv_layers.active


# --- painting ---------------------------------------------------------------

def _hash3(ix, iy, iz, seed):
    h = (ix.astype(np.int64) * 73856093) ^ (iy.astype(np.int64) * 19349663) ^ \
        (iz.astype(np.int64) * 83492791) ^ (seed * 2654435761)
    h = (h ^ (h >> 13)) * 1274126177
    h = h ^ (h >> 16)
    return (h & 0xFFFFFF).astype(np.float64) / float(0xFFFFFF)


def value_noise(points, scale, seed):
    """Deterministic trilinear lattice noise in [0, 1]."""
    p = points * scale
    base = np.floor(p)
    f = p - base
    f = f * f * (3.0 - 2.0 * f)
    i = base.astype(np.int64)
    total = 0.0
    for dx in (0, 1):
        for dy in (0, 1):
            for dz in (0, 1):
                w = (f[:, 0] if dx else 1 - f[:, 0]) * \
                    (f[:, 1] if dy else 1 - f[:, 1]) * \
                    (f[:, 2] if dz else 1 - f[:, 2])
                total = total + w * _hash3(i[:, 0] + dx, i[:, 1] + dy,
                                           i[:, 2] + dz, seed)
    return total


def fbm(points, scale, seed, octaves=4):
    value, amplitude, norm = 0.0, 1.0, 0.0
    for octave in range(octaves):
        value = value + amplitude * value_noise(points, scale * 2 ** octave,
                                                seed + 101 * octave)
        norm += amplitude
        amplitude *= 0.5
    return value / norm


def cells(points, scale, seed):
    """Distance to the nearest jittered lattice point (scales, plates)."""
    p = points * scale
    base = np.floor(p).astype(np.int64)
    best = np.full(len(points), 9.0)
    second = np.full(len(points), 9.0)
    for dx in (-1, 0, 1):
        for dy in (-1, 0, 1):
            for dz in (-1, 0, 1):
                cx, cy, cz = base[:, 0] + dx, base[:, 1] + dy, base[:, 2] + dz
                jitter = np.stack([_hash3(cx, cy, cz, seed + k)
                                   for k in range(3)], axis=1)
                gap = np.linalg.norm(np.stack([cx, cy, cz], axis=1) + jitter - p,
                                     axis=1)
                second = np.where(gap < best, best, np.minimum(second, gap))
                best = np.minimum(best, gap)
    return best, second


def bake_albedo(obj, size, colour, path, dilate=12):
    """Rasterise every UV triangle and evaluate `colour(positions, normals,
    regions)` at its texels; returns the PNG bytes written to `path`."""
    mesh = obj.data
    mesh.calc_loop_triangles()
    uv = mesh.uv_layers.active.data
    region_layer = mesh.attributes[REGION_LAYER].data
    image = np.zeros((size, size, 3), dtype=np.float64)
    filled = np.zeros((size, size), dtype=bool)
    batch_pos, batch_nrm, batch_reg, batch_px = [], [], [], []
    for tri in mesh.loop_triangles:
        loops = tri.loops
        verts = tri.vertices
        t = np.array([uv[l].uv for l in loops]) * size - 0.5
        p = np.array([mesh.vertices[v].co for v in verts])
        n = np.array([mesh.vertices[v].normal for v in verts])
        region = region_layer[verts[0]].value
        lo = np.floor(t.min(axis=0)).astype(int)
        hi = np.ceil(t.max(axis=0)).astype(int)
        lo = np.clip(lo, 0, size - 1)
        hi = np.clip(hi, 0, size - 1)
        xs, ys = np.meshgrid(np.arange(lo[0], hi[0] + 1),
                             np.arange(lo[1], hi[1] + 1))
        xs, ys = xs.ravel(), ys.ravel()
        (x0, y0), (x1, y1), (x2, y2) = t
        det = (y1 - y2) * (x0 - x2) + (x2 - x1) * (y0 - y2)
        if abs(det) < 1e-12:
            continue
        a = ((y1 - y2) * (xs - x2) + (x2 - x1) * (ys - y2)) / det
        b = ((y2 - y0) * (xs - x2) + (x0 - x2) * (ys - y2)) / det
        c = 1.0 - a - b
        inside = (a >= -0.02) & (b >= -0.02) & (c >= -0.02)
        if not inside.any():
            continue
        bary = np.stack([a[inside], b[inside], c[inside]], axis=1)
        batch_pos.append(bary @ p)
        normals = bary @ n
        batch_nrm.append(normals / np.maximum(
            np.linalg.norm(normals, axis=1, keepdims=True), 1e-9))
        batch_reg.append(np.full(len(bary), region))
        batch_px.append(np.stack([ys[inside], xs[inside]], axis=1))
    positions = np.concatenate(batch_pos)
    normals = np.concatenate(batch_nrm)
    regions = np.concatenate(batch_reg)
    pixels = np.concatenate(batch_px)
    rgb = np.clip(colour(positions, normals, regions), 0.0, 1.0)
    image[pixels[:, 0], pixels[:, 1]] = rgb
    filled[pixels[:, 0], pixels[:, 1]] = True
    # Grow every island outward so mip levels never sample the empty atlas.
    for _ in range(dilate):
        if filled.all():
            break
        grown = image.copy()
        mask = filled.copy()
        for dy, dx in ((0, 1), (0, -1), (1, 0), (-1, 0)):
            shifted = np.roll(np.roll(filled, dy, axis=0), dx, axis=1)
            source = np.roll(np.roll(image, dy, axis=0), dx, axis=1)
            take = shifted & ~mask
            grown[take] = source[take]
            mask |= take
        image, filled = grown, mask
    image[~filled] = image[filled].mean(axis=0)
    # Texture coordinates have v upward; PNG rows run top-down.
    data = (np.flipud(image) * 255.0 + 0.5).astype(np.uint8)
    png = encode_png(data)
    Path(path).write_bytes(png)
    return png


def encode_png(rgb):
    """Deterministic 8-bit RGB PNG (no timestamps, fixed compression)."""
    height, width, _ = rgb.shape
    raw = b"".join(b"\x00" + rgb[row].tobytes() for row in range(height))

    def chunk(kind, payload):
        return struct.pack(">I", len(payload)) + kind + payload + \
            struct.pack(">I", zlib.crc32(kind + payload) & 0xFFFFFFFF)
    header = struct.pack(">IIBBBBB", width, height, 8, 2, 0, 0, 0)
    return b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", header) + \
        chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b"")


# --- export ----------------------------------------------------------------

def albedo_manifest(path):
    data = Path(path).read_bytes()
    if data[:8] != b"\x89PNG\r\n\x1a\n" or data[12:16] != b"IHDR":
        raise SystemExit("albedo is not a PNG: " + str(path))
    return {"file": Path(path).name,
            "sha256": hashlib.sha256(data).hexdigest(),
            "width": int.from_bytes(data[16:20], "big"),
            "height": int.from_bytes(data[20:24], "big"),
            "bitDepth": data[24], "colorType": data[25]}


def split_for_export(obj, weights):
    """Per-vertex arrays as the runtime needs them: one UV per vertex, so a
    vertex is duplicated only where the texture seam gives its loops
    different coordinates. Position, normal and weights are shared, so the
    seam is invisible in shading and deformation. Triangles keep Blender's
    winding: Cross(b - a, c - a) is the outward front in Unity as well."""
    mesh = obj.data
    mesh.calc_loop_triangles()
    uv = mesh.uv_layers.active.data
    key_to_index = {}
    positions, normals, coords, rows = [], [], [], []
    triangles = []
    for tri in mesh.loop_triangles:
        corner = []
        for loop, vertex in zip(tri.loops, tri.vertices):
            u, v = uv[loop].uv
            key = (vertex, round(u, 6), round(v, 6))
            if key not in key_to_index:
                key_to_index[key] = len(positions)
                co = mesh.vertices[vertex].co
                nrm = mesh.vertices[vertex].normal
                positions.append((co.x, co.y, co.z))
                normals.append((nrm.x, nrm.y, nrm.z))
                coords.append((min(1.0, max(0.0, u)), min(1.0, max(0.0, v))))
                rows.append(weights[vertex])
            corner.append(key_to_index[key])
        triangles.append(tuple(corner))
    return positions, normals, coords, triangles, rows


def orientation_audit(positions, triangles):
    """Per connected component: closed, consistently wound, outward. The
    same rule the domain test applies to the shipped bytes."""
    welded = {}
    weld = []
    for p in positions:
        key = (round(p[0], 5), round(p[1], 5), round(p[2], 5))
        weld.append(welded.setdefault(key, len(welded)))
    parent = list(range(len(welded)))

    def find(x):
        while parent[x] != x:
            parent[x] = parent[parent[x]]
            x = parent[x]
        return x
    for a, b, c in triangles:
        wa, wb, wc = weld[a], weld[b], weld[c]
        parent[find(wa)] = find(wb)
        parent[find(wb)] = find(wc)
    groups = {}
    for index, (a, b, c) in enumerate(triangles):
        groups.setdefault(find(weld[a]), []).append(index)
    report = []
    for faces in groups.values():
        directed = {}
        volume = 0.0
        for f in faces:
            a, b, c = triangles[f]
            wa, wb, wc = weld[a], weld[b], weld[c]
            for edge in ((wa, wb), (wb, wc), (wc, wa)):
                directed[edge] = directed.get(edge, 0) + 1
            pa, pb, pc = (Vector(positions[i]) for i in (a, b, c))
            volume += pa.dot(pb.cross(pc)) / 6.0
        open_edges = sum(1 for (u, v) in directed if (v, u) not in directed)
        repeated = sum(1 for count in directed.values() if count > 1)
        report.append({"triangles": len(faces), "openEdges": open_edges,
                       "repeatedDirectedEdges": repeated, "volume": volume})
    return report


def write_mesh_data(path, obj, weights, rig_sha256, albedo_path, space,
                    extra=None):
    positions, normals, coords, triangles, rows = split_for_export(obj, weights)
    audit = orientation_audit(positions, triangles)
    bad = [row for row in audit if row["openEdges"] or
           row["repeatedDirectedEdges"] or row["volume"] <= 0.0]
    if bad:
        raise SystemExit("refusing to export open, inconsistent or inward "
                         "components: " + json.dumps(bad[:5]))
    order = sorted({name for row in rows for name, _ in row})
    index_of = {name: index for index, name in enumerate(order)}
    blob = bytearray()
    for value in positions:
        blob += struct.pack("<3f", *value)
    for value in normals:
        blob += struct.pack("<3f", *value)
    for value in coords:
        blob += struct.pack("<2f", *value)
    for tri in triangles:
        blob += struct.pack("<3i", *tri)
    for row in rows:
        if len(row) > 4:
            raise SystemExit("Unity allows at most four bone influences")
        total = sum(value for _, value in row)
        slots = [(index_of[name], value / total) for name, value in row]
        while len(slots) < 4:
            slots.append((0, 0.0))
        for bone_index, weight in slots:
            blob += struct.pack("<if", bone_index, weight)
    payload = {
        "schemaVersion": 2,
        "space": space,
        "rigSha256": rig_sha256,
        "bones": order,
        "uvAtlas": {"body": [0.0, 0.0, 1.0, 1.0]},
        "albedo": albedo_manifest(albedo_path),
        "vertexCount": len(positions),
        "triangleCount": len(triangles),
        "winding": "outward; front is Cross(b - a, c - a), not reversed",
        "data": base64.b64encode(bytes(blob)).decode("ascii"),
    }
    if extra:
        payload.update(extra)
    with open(path, "w", encoding="utf-8", newline="\n") as handle:
        handle.write(json.dumps(payload, indent=1) + "\n")
    return {"vertices": len(positions), "triangles": len(triangles),
            "components": len(audit), "bones": order,
            "minimumComponentVolume": min(row["volume"] for row in audit)}
