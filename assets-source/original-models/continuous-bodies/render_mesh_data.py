"""Render shipped schema-2 mesh data in Blender exactly as the runtime builds it.

blender -b --factory-startup --python render_mesh_data.py -- \
    --mesh <mesh.json> --out-dir <dir> --name <prefix> --up z|y \
    [--forward=-y|+y|-z|+z] [--mode shipped|outward] [--views top,rear,side]

The positions, normals, texture coordinates and triangle order are taken
verbatim from the payload, as PteranodonAssetRuntime.BuildMesh does. Back-face
culling is on, so a face is drawn only when Cross(b - a, c - a) points at the
camera - the same front-face rule Unity applies for an unmirrored transform.
"outward" mode reverses only the components whose shipped order faces inward,
as a diagnostic of what consistent outward winding would look like.
"""
import argparse
import base64
import json
import math
import struct
import sys
from collections import defaultdict
from pathlib import Path

import bpy
from mathutils import Matrix, Vector


def args():
    argv = sys.argv[sys.argv.index("--") + 1:]
    p = argparse.ArgumentParser()
    p.add_argument("--mesh", required=True)
    p.add_argument("--out-dir", required=True)
    p.add_argument("--name", required=True)
    p.add_argument("--up", choices=("y", "z"), required=True)
    p.add_argument("--forward", required=True)
    p.add_argument("--mode", default="shipped", choices=("shipped", "outward"))
    p.add_argument("--views", default="party,rear,side,front")
    p.add_argument("--size", type=int, default=640)
    p.add_argument("--normals", default="shipped", choices=("shipped", "auto"))
    return p.parse_args(argv)


def load(path):
    payload = json.loads(Path(path).read_text(encoding="utf-8"))
    blob = base64.b64decode(payload["data"])
    n, t = payload["vertexCount"], payload["triangleCount"]
    o = 0
    pos = [struct.unpack_from("<3f", blob, o + 12 * i) for i in range(n)]
    o += 12 * n
    nrm = [struct.unpack_from("<3f", blob, o + 12 * i) for i in range(n)]
    o += 12 * n
    uv = [struct.unpack_from("<2f", blob, o + 8 * i) for i in range(n)]
    o += 8 * n
    tri = [struct.unpack_from("<3i", blob, o + 12 * i) for i in range(t)]
    return payload, pos, nrm, uv, tri


def outward_fix(pos, tri):
    parent = list(range(len(pos)))

    def find(x):
        while parent[x] != x:
            parent[x] = parent[parent[x]]
            x = parent[x]
        return x
    for a, b, c in tri:
        parent[find(a)] = find(b)
        parent[find(b)] = find(c)
    groups = defaultdict(list)
    for i, (a, b, c) in enumerate(tri):
        groups[find(a)].append(i)
    fixed = list(tri)
    for faces in groups.values():
        verts = {v for f in faces for v in tri[f]}
        cc = sum((Vector(pos[v]) for v in verts), Vector()) / len(verts)
        for f in faces:
            a, b, c = (Vector(pos[v]) for v in tri[f])
            normal = (b - a).cross(c - a)
            if normal.dot((a + b + c) / 3 - cc) < 0:
                fixed[f] = (tri[f][0], tri[f][2], tri[f][1])
    return fixed


def main():
    a = args()
    payload, pos, nrm, uv, tri = load(a.mesh)
    if a.mode == "outward":
        tri = outward_fix(pos, tri)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    mesh = bpy.data.meshes.new(a.name)
    mesh.from_pydata(pos, [], tri)
    mesh.update()
    layer = mesh.uv_layers.new(name="uv")
    for loop in mesh.loops:
        layer.data[loop.index].uv = uv[loop.vertex_index]
    if a.normals == "shipped":
        mesh.normals_split_custom_set_from_vertices([Vector(v) for v in nrm])
    obj = bpy.data.objects.new(a.name, mesh)
    bpy.context.collection.objects.link(obj)

    # Rotate the donor frame so the creature stands Z-up facing -Y in Blender.
    # A proper rotation, so front faces stay front faces.
    fwd = {"-y": Vector((0, -1, 0)), "+y": Vector((0, 1, 0)),
           "-z": Vector((0, 0, -1)), "+z": Vector((0, 0, 1))}[a.forward]
    up = Vector((0, 0, 1)) if a.up == "z" else Vector((0, 1, 0))
    side = fwd.cross(up)
    basis = Matrix((side, fwd, up)).transposed()   # columns: frame axes
    # side = forward x up, so a proper target needs side = (-Y) x Z = -X.
    target = Matrix(((-1, 0, 0), (0, -1, 0), (0, 0, 1))).transposed()
    rot = target @ basis.inverted()
    if rot.determinant() < 0:
        raise SystemExit("frame mapping would mirror")
    obj.matrix_world = rot.to_4x4()
    bpy.context.view_layer.update()

    albedo = Path(a.mesh).with_name(payload["albedo"]["file"])
    mat = bpy.data.materials.new("albedo")
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    img = nodes.new("ShaderNodeTexImage")
    img.image = bpy.data.images.load(str(albedo))
    mat.node_tree.links.new(img.outputs["Color"],
                            nodes["Principled BSDF"].inputs["Base Color"])
    mat.use_backface_culling = True
    mesh.materials.append(mat)

    world_pts = [obj.matrix_world @ Vector(p) for p in pos]
    lo = Vector([min(p[i] for p in world_pts) for i in range(3)])
    hi = Vector([max(p[i] for p in world_pts) for i in range(3)])
    centre = (lo + hi) / 2
    radius = (hi - lo).length / 2

    scene = bpy.context.scene
    scene.render.engine = "BLENDER_WORKBENCH"
    shading = scene.display.shading
    shading.light = "STUDIO"
    shading.color_type = "TEXTURE"
    shading.show_backface_culling = True
    shading.show_cavity = False
    scene.render.resolution_x = a.size
    scene.render.resolution_y = a.size
    scene.render.film_transparent = False
    world = bpy.data.worlds.new("w")
    scene.world = world
    scene.display.shading.background_type = "VIEWPORT"
    scene.display.shading.background_color = (0.22, 0.30, 0.28)

    cam_data = bpy.data.cameras.new("cam")
    cam_data.type = "ORTHO"
    cam_data.ortho_scale = radius * 2.3
    cam = bpy.data.objects.new("cam", cam_data)
    scene.collection.objects.link(cam)
    scene.camera = cam
    views = {
        # Kingmaker's party camera looks down at roughly 55 degrees.
        "party": (Vector((0.55, -0.55, 1.10)), "party"),
        "rear": (Vector((0.0, 1.0, 0.45)), "rear"),
        "side": (Vector((1.0, 0.0, 0.15)), "side"),
        "front": (Vector((0.0, -1.0, 0.35)), "front"),
        "top": (Vector((0.0, 0.0001, 1.0)), "top"),
        "below": (Vector((0.3, -0.3, -1.0)), "below"),
    }
    out = Path(a.out_dir)
    out.mkdir(parents=True, exist_ok=True)
    for key in a.views.split(","):
        direction, label = views[key]
        cam.location = centre + direction.normalized() * radius * 4
        look = (centre - cam.location).normalized()
        cam.rotation_euler = look.to_track_quat("-Z", "Y").to_euler()
        scene.render.filepath = str(out / ("%s-%s-%s.png" % (a.name, a.mode, label)))
        bpy.ops.render.render(write_still=True)
    print("[render] done", a.name, a.mode)


if __name__ == "__main__":
    main()
