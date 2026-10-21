"""Cleanup 2: reconstruct project-owned prototype surfaces as painted skins.

The immutable cleanup2-*-input.json files are ORIGINAL PROJECT GEOMETRY, not
native captures. Closest-triangle barycentric transfer retains their reviewed
donor weights. No native mesh, bind transform, or animation is reconstructed.
Blender 4.5.10: -b --factory-startup --python-exit-code 1 -P this.py --
  --kind elephant|brachiosaurus --output-dir <assets> --work-dir <private>
"""
import argparse
import base64
import hashlib
import json
import math
from pathlib import Path
import struct
import sys
import bpy
import bmesh
import numpy as np
from mathutils import Vector, geometry
from mathutils.bvhtree import BVHTree

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE.parent / "continuous-bodies"))
import continuous_body as cb
import audit_mesh_orientation as audit
AXES = (Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1)))


class ProjectInput:
    def __init__(self, path):
        self.path = Path(path)
        self.payload, self.positions, _, self.triangles, weights = audit.load(path)
        self.positions = [Vector(p) for p in self.positions]
        self.rows = [[(self.payload["bones"][i], v) for i, v in row if v > 0]
                     for row in weights]
        self.groups = audit.components(len(self.positions), self.triangles)

    def component(self, index, name="OriginalSurface"):
        faces = self.groups[index]
        ids = sorted({v for f in faces for v in self.triangles[f]})
        remap = {v: i for i, v in enumerate(ids)}
        tri = [tuple(remap[v] for v in self.triangles[f]) for f in faces]
        # All giant-body prototype components shipped inward; determine by
        # volume, never by a global renderer/shader exception.
        vol = sum(self.positions[self.triangles[f][0]].dot(
            self.positions[self.triangles[f][1]].cross(
                self.positions[self.triangles[f][2]])) for f in faces)
        if vol < 0:
            tri = [(a, c, b) for a, b, c in tri]
        mesh = bpy.data.meshes.new(name)
        mesh.from_pydata([self.positions[v] for v in ids], [], tri)
        mesh.update()
        obj = bpy.data.objects.new(name, mesh)
        bpy.context.collection.objects.link(obj)
        return obj, {i:self.rows[v] for i,v in enumerate(ids)}

    def transfer(self, obj, components):
        fs = [f for c in components for f in self.groups[c]]
        tri = [self.triangles[f] for f in fs]
        tree = BVHTree.FromPolygons(self.positions, tri, all_triangles=True)
        result = []
        for vertex in obj.data.vertices:
            co, _, index, _ = tree.find_nearest(vertex.co)
            ids = tri[index]
            # barycentric_transform with unit coordinate axes gives barycentric
            # coefficients. Transfer weights over the triangle, not vertex IDs.
            values = geometry.barycentric_transform(co,
                *(self.positions[i] for i in ids), *AXES)
            merged = {}
            for i, factor in zip(ids, values):
                for bone, value in self.rows[i]:
                    merged[bone] = merged.get(bone, 0) + value * max(0, factor)
            rows = sorted(merged.items(), key=lambda r: (-r[1], r[0]))[:4]
            total = sum(v for _, v in rows)
            result.append([(n, v / total) for n, v in rows if v > 1e-5])
        return cb.smooth_weights(obj, dict(enumerate(result)), iterations=6, factor=.45)


def normalize_rows(rows):
    out = {}
    for index,row in rows.items():
        row = sorted(row, key=lambda r: (-r[1], r[0]))[:4]
        total = sum(v for _, v in row)
        out[index] = [(n, v/total) for n, v in row if v > 1e-5]
    return out


def build(kind, source):
    # Smooth the torso alone before joining limbs; a dense sum of overlapping
    # fields would inflate it. Voxel union preserves the prescribed extents.
    volume = cb.Volume(.048)
    limb_ids={9,10,11,12} if kind=="elephant" else {5,6,7,8}
    excluded = {3, 4, 5, 6, 8} if kind == "elephant" else {4}
    reference_ids=[i for i in range(len(source.groups)) if i not in excluded]
    excluded |= limb_ids
    included = [i for i in range(len(source.groups)) if i not in excluded]
    for i in included:
        obj, _ = source.component(i)
        volume.shell(obj)
    # A broad transition from withers into neck, buried inside both masses.
    volume.ellipsoid(Vector((0, 3.85, -1.24)), AXES,
                     (.78 if kind == "elephant" else .86, .83, .82))
    # The greybox's eight-sided zigzag leg tubes are replaced by substantial
    # load-bearing columns. Their feet stay at the original contact locations;
    # weights still transfer from the real published leg chains.
    for sign in (-1,1):
        for front in (True,False):
            if front:
                chain=[(sign*.80,3.65,-1.27),(sign*.73,2.45,-1.20),
                       (sign*.70,1.35,-1.33),(sign*.70,.38,-1.57)]
            else:
                chain=[(sign*.81,3.36,1.70),(sign*.70,2.35,1.55),
                       (sign*.68,1.28,1.47),(sign*.69,.38,1.39)]
            radius=.98 if kind=="elephant" else .90
            volume.sweep(chain,[r*radius for r in (.65,.52,.43,.46)],segments=24)
            foot=Vector(chain[-1]);foot.y=.24
            volume.ellipsoid(foot,AXES,(.48*radius,.22,.57*radius))
    if kind == "elephant":
        tail_points=[(0,3.85,2.1),(0,3.3,2.65),(0,2.8,2.94),
                     (0,2.25,3.12),(0,1.85,3.19)]
        tail_radii=[.15,.11,.085,.065,.045]
    else:
        # A sauropod's countertail is a long taper, not the donor's truncated
        # thick pendant. Its root stays inside the pelvis and shares the same
        # five existing tail controls; gameplay dimensions are unchanged.
        tail_points=[(0,3.6,2.0),(0,3.25,2.9),(0,2.9,3.8),
                     (0,2.62,4.8),(0,2.45,5.75)]
        tail_radii=[.55,.40,.25,.13,.035]
    volume.sweep(tail_points,tail_radii,segments=20)
    body = volume.polygonise(kind.title() + "ContinuousSkin")
    cb.finish_surface(body, 11000 if kind == "elephant" else 12500,
                      smooth_iterations=22)
    rows = source.transfer(body, reference_ids)
    tail_rows=cb.chain_rows(body,[Vector(p) for p in tail_points],
                           ["Tail%d_M"%i for i in range(5)],blend=.3)
    # Keep the barrel on the spine and limb rotation in the limbs. Original
    # greybox graded sockets are retained through transfer below the belly.
    for i, vertex in enumerate(body.data.vertices):
        p = vertex.co
        if p.z>2.7 and abs(p.x)<.62:
            rows[i]=tail_rows[i]
        if p.y > 2.7 and -.9 < p.z < 2.5:
            amount = min(1, (p.y - 2.7) / .7) * .88
            host = "UpperTorso" if p.z < .5 else "LowerTorso"
            row = {n: v * (1-amount) for n, v in rows[i]}
            row[host] = row.get(host, 0) + amount
            rows[i] = list(row.items())
    parts = [(body, normalize_rows(rows), 0)]
    if kind == "elephant":
        tuft=cb.blob("ElephantTailTuft",Vector(tail_points[-1]),AXES,
                     (.075,.13,.075),segments=12,rings=8)
        parts.append((tuft,cb.rigid_rows(tuft,[("Tail4_M",1)]),3))
        for i in (5, 6):
            obj, weights = source.component(i, "ElephantIvoryTusk")
            # Subdivide this authored taper before smoothing so the silhouette
            # retains a sharp tip while the low resolution octagon softens.
            parts.append((obj, weights, 1))
        for side, sign in (("L", 1), ("R", -1)):
            # A fan-shaped pinna, with its inner edge buried in the cranium.
            # Both layers and the rim form one closed shell; no double-sided
            # material conceals missing geometry.
            grid = []
            for j in range(9):
                t = j / 8
                width=.16+1.14*math.sin(math.pi*t)**.65
                # Broad fan across X/Y with only a shallow forward cup in Z.
                # The earlier X/Z oval looked like a sphere from the side.
                grid.append([Vector((sign*(.48+width*u),4.72-1.8*t,
                                     -1.95+.40*u-.10*math.sin(t*math.pi)))
                             for u in (0,.25,.5,.75,1)])
            ear, _ = cb.sheet("ElephantPinna_"+side, grid, .028)
            parts.append((ear, cb.rigid_rows(ear, [("Ear_"+side,1)]), 0))
        for sign in (-1, 1):
            eye = cb.blob("ElephantEye", Vector((sign*.51, 4.26, -2.20)),
                          AXES, (.052,.043,.055), segments=12,rings=8)
            parts.append((eye, cb.rigid_rows(eye, [("Head",1)]), 2))
            for front in (True,False):
                bone=("L" if sign>0 else "R")+("_Palm" if front else "_Foot0")
                for j in (-1,0,1):
                    nail=cb.blob("ElephantToenail",Vector((sign*.70+j*.19,.17,
                        -2.04 if front else .91)),AXES,(.10,.11,.055),segments=12,rings=8)
                    parts.append((nail,cb.rigid_rows(nail,[(bone,1)]),4))
    else:
        for sign in (-1, 1):
            eye = cb.blob("SauropodEye", Vector((sign*.225,7.61,-4.41)),
                          AXES,(.037,.04,.04),segments=12,rings=8)
            parts.append((eye,cb.rigid_rows(eye,[("Head",1)]),2))
    obj, weights = cb.assemble(kind.title()+"Refined", parts)
    cb.require_single_body(obj)
    cb.unwrap(obj)
    return obj, weights


def colour_field(kind):
    def colour(points, normals, regions):
        noise = cb.fbm(points, 4.0, 531, octaves=3)
        grain = cb.value_noise(points, 28, 41)
        if kind == "elephant":
            # Warm dust on grey hide; branched spatial creases preserve a
            # continuous skin treatment across the new UV islands.
            folds = np.exp(-np.abs(np.sin(points[:,1]*34 + noise*12)) * 10)
            base = np.array((.39,.37,.34))
            value = .82 + .23*noise + .045*grain - .10*folds
        else:
            pebble, _ = cb.cells(points, 18, 713)
            base = np.array((.36,.42,.30))
            value = .83 + .23*noise + .045*grain + .07*pebble
        rgb = value[:,None] * base
        belly = np.clip((3.2-points[:,1])/.9,0,1) * np.clip(1-abs(points[:,0])/1.4,0,1)
        rgb += belly[:,None] * np.array((.045,.042,.03))
        rgb[regions==1] = np.array((.78,.73,.59)) * (.92+.08*noise[regions==1,None])
        rgb[regions==2] = (.055,.043,.028)
        rgb[regions==3] = (.12,.11,.10)
        rgb[regions==4] = (.49,.47,.42)
        return np.clip(rgb,0,1)
    return colour


def main():
    parser=argparse.ArgumentParser()
    parser.add_argument("--kind",choices=("elephant","brachiosaurus"),required=True)
    parser.add_argument("--output-dir",required=True)
    parser.add_argument("--work-dir",required=True)
    args=parser.parse_args(sys.argv[sys.argv.index("--")+1:])
    out,work=Path(args.output_dir).resolve(),Path(args.work_dir).resolve()
    out.mkdir(parents=True,exist_ok=True);work.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    source=ProjectInput(HERE/("cleanup2-"+args.kind+"-input.json"))
    obj,weights=build(args.kind,source)
    albedo=out/(args.kind+"-albedo.png")
    cb.bake_albedo(obj,1024,colour_field(args.kind),albedo)
    cb.attach_albedo_preview(obj,albedo)
    # Preserve exact existing unit, donor and runtime schema contracts.
    extra={k:v for k,v in source.payload.items() if k not in {
        "data","vertexCount","triangleCount","bones","albedo","uvAtlas","winding"}}
    extra.update({"authoringRevision":"cleanup2-continuous-painted",
                  "sourceGeometrySha256":hashlib.sha256(source.path.read_bytes()).hexdigest()})
    report=cb.write_mesh_data(out/(args.kind+"-mesh.json"),obj,weights,
        source.payload["rigSha256"],albedo,source.payload["space"],extra)
    bpy.ops.wm.save_as_mainfile(filepath=str(work/(args.kind+"-source.blend")))
    (work/(args.kind+"-report.json")).write_text(json.dumps(report,indent=2)+"\n")
    print(json.dumps(report))

if __name__=="__main__":main()
