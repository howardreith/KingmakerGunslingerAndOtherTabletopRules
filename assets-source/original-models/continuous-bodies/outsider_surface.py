"""Opt-in continuous skin for the owner-requested outsider visual revision.

Uses the existing exact voxel union and outward exporter checks. Native rig
metadata remains input-only. Teeth, eyes, feathers, chains and other chosen
atlas regions remain separate solids; no other generator imports this module.
"""
from collections import defaultdict
import bpy
import bmesh
from mathutils import Vector
from mathutils.bvhtree import BVHTree
import continuous_body as surface


def consolidate_skin(bm, weights, uvs, atlas, regions=('body', 'limbs'),
                     voxel=.010, triangles=7000):
    bm.verts.index_update()
    bm.faces.index_update()
    verts = list(bm.verts)
    positions = [v.co.copy() for v in verts]
    old_weights = {v.index: list(weights[v]) for v in verts}
    old_uv = {v.index: tuple(uvs[v]) for v in verts}

    def tile(pair):
        matches = [name for name, r in atlas.items()
                   if r[0] < pair[0] < r[2] and r[1] < pair[1] < r[3]]
        if len(matches) != 1:
            raise ValueError('original UV must lie inside exactly one atlas tile')
        return matches[0]

    def skin(v):
        u, v = old_uv[v.index]
        return any(atlas[r][0] < u < atlas[r][2] and
                   atlas[r][1] < v < atlas[r][3] for r in regions)

    organic, discrete = [], []
    for face in bm.faces:
        ids = [v.index for v in face.verts]
        (organic if all(skin(v) for v in face.verts) else discrete).append(ids)
    if not organic:
        raise ValueError('no original organic surface selected')
    source = bpy.data.meshes.new('OutsiderOriginalSkinTransfer')
    source.from_pydata(positions, [], organic)
    source.update()
    obj = bpy.data.objects.new('OutsiderContinuousSkin', source.copy())
    bpy.context.collection.objects.link(obj)
    for other in bpy.context.selected_objects:
        other.select_set(False)
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    remesh = obj.modifiers.new('ExactSkinUnion', 'REMESH')
    remesh.mode = 'VOXEL'
    remesh.voxel_size = voxel
    remesh.adaptivity = 0
    bpy.ops.object.modifier_apply(modifier=remesh.name)
    surface.finish_surface(obj, triangles, smooth_iterations=12)

    source.calc_loop_triangles()
    faces = [tuple(f.vertices) for f in source.loop_triangles]
    face_tiles = []
    for face in faces:
        names = {tile(old_uv[i]) for i in face}
        if len(names) != 1:
            raise ValueError('source skin triangle already crosses atlas tiles')
        face_tiles.append(names.pop())
    tree = BVHTree.FromPolygons(positions, faces, all_triangles=True)
    rows, paint = {}, {}
    for vertex in obj.data.vertices:
        point, normal, index, distance = tree.find_nearest(vertex.co)
        ids = faces[index]
        a, b, c = [positions[i] for i in ids]
        e0, e1, d = b-a, c-a, point-a
        aa, ab, bb = e0.dot(e0), e0.dot(e1), e1.dot(e1)
        denominator = aa*bb-ab*ab
        if abs(denominator) < 1e-18:
            bary = [1, 0, 0]
        else:
            v = (bb*d.dot(e0)-ab*d.dot(e1))/denominator
            w = (aa*d.dot(e1)-ab*d.dot(e0))/denominator
            bary = [max(0,1-v-w), max(0,v), max(0,w)]
            total = sum(bary)
            bary = [v/total for v in bary]
        row = defaultdict(float)
        for original, fraction in zip(ids, bary):
            for bone, influence in old_weights[original]:
                row[bone] += influence*fraction
        rows[vertex.index] = sorted(row.items(), key=lambda r: (-r[1],r[0]))
        original_uv = tuple(sum(old_uv[i][k]*f for i,f in zip(ids,bary))
                            for k in (0,1))
        rect = atlas[face_tiles[index]]
        # Transfer tile-LOCAL coordinates. A fused face chooses one material
        # region as a whole below; its corners can never interpolate across
        # unrelated rectangles. Keep the original padding at tile boundaries.
        paint[vertex.index] = tuple(max(.02, min(.98,
            (original_uv[k]-rect[k])/(rect[k+2]-rect[k]))) for k in (0,1))
    rows = surface.smooth_weights(obj, rows, iterations=3, factor=.35)
    for index, row in rows.items():
        row = sorted(row, key=lambda r: (-r[1],r[0]))[:2]
        total = sum(w for _,w in row)
        rows[index] = [(name,w/total) for name,w in row]

    # Split UV seams by FACE region, retaining identical position, smooth
    # normal and skin weights on each duplicate. The schema already supports
    # split vertices. Only the opt-in normal layer changes legacy export;
    # unsplit/discrete geometry continues using its ordinary vertex normal.
    bm.clear()
    weights.clear()
    uvs.clear()
    normal_layer = bm.verts.layers.float_vector.get('KMG_ContinuousSkinNormal')
    if normal_layer is None:
        normal_layer = bm.verts.layers.float_vector.new('KMG_ContinuousSkinNormal')
    skin_vertices = {}
    for face in obj.data.polygons:
        centre = sum((obj.data.vertices[i].co for i in face.vertices), Vector()) / len(face.vertices)
        _, _, nearest, _ = tree.find_nearest(centre)
        region = face_tiles[nearest]
        rect = atlas[region]
        corners = []
        for index in face.vertices:
            key = (index, region)
            if key not in skin_vertices:
                original = obj.data.vertices[index]
                v = bm.verts.new(original.co)
                v[normal_layer] = original.normal
                weights[v] = rows[index]
                uvs[v] = tuple(rect[k] + paint[index][k]*(rect[k+2]-rect[k])
                               for k in (0,1))
                skin_vertices[key] = v
            corners.append(skin_vertices[key])
        bm.faces.new(corners)
    copied = {}
    for face in discrete:
        for index in face:
            if index not in copied:
                v = bm.verts.new(positions[index])
                copied[index] = v
                weights[v] = old_weights[index]
                uvs[v] = old_uv[index]
        bm.faces.new([copied[index] for index in face])
    generated = obj.data
    bpy.data.objects.remove(obj, do_unlink=True)
    bpy.data.meshes.remove(generated)
    bpy.data.meshes.remove(source)
    bm.normal_update()
