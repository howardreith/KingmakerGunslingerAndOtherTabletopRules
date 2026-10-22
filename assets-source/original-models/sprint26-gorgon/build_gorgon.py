"""Cleanup 2 Gorgon: original continuous armored bovine, not donor geometry.

The retained original project export supplies geometry/weights, never native
bind transforms. Optional historical Aurochs rig data is admitted ONLY for an
explicitly labelled offline pose preview after named-frame landmark checks.
No native capture is silently substituted for the original strict 31-bone one.
"""
import argparse
import base64
import hashlib
import json
import math
from pathlib import Path
import struct
import sys
import time
import zlib

import bpy
import bmesh
import numpy as np
from mathutils import Euler, Vector
from mathutils.bvhtree import BVHTree

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE.parent / "sprint25-giant-bodies"))
from refine_giant_bodies import ProjectInput, normalize_rows, cb, AXES
sys.path.insert(0, str(HERE.parent / "sprint21-arthropods"))
from verify_cleanup2_bodies import inspect

DONOR = "5bb9579fdb2b26b48bb10d61c81cfdfb"
PREFAB = "6032f7a5751c1104aa59647b176634c8"
INPUT_SHA = "0f3c8ce10c4c0d50650ecb5a042985d1b242977716c5b7a0286754d370fdc508"
PAINT_BASELINE_SHA = "f6cf501d0f8817ed327e7275736cda78bb5684f3270adee1e7c6448c5a32c9f2"
PAINT_GEOMETRY_SHA = "eb5d3c7a59695312ecc954c076edd6c5947374eaf782b6ba71cb6382d03a4dcf"
PLATE_EDGES = {}
BODY_GROUPS = [0, 1, 2, 3, 5, 8, 12, 15, 18]
POSES = {
    "stride": {"L_Arm_Upper": (24, 0, 0), "L_Arm_Lower": (-15, 0, 0),
               "R_Arm_Upper": (-18, 0, 0), "R_Leg0_Upper": (19, 0, 0),
               "L_Leg0_Upper": (-19, 0, 0), "Tail1": (0, 16, 0)},
    "head-turn-and-brace": {"Neck1": (0, 12, 0), "Head": (8, -18, 0),
               "Jaw": (12, 0, 0), "L_Arm_Upper": (16, 0, -6),
               "R_Arm_Upper": (-13, 0, 6), "Tail3": (12, 0, 0)}}


def bounds_centre(source, group):
    ids = {i for f in source.groups[group] for i in source.triangles[f]}
    return Vector(tuple((min(source.positions[i][k] for i in ids) +
                         max(source.positions[i][k] for i in ids)) / 2 for k in range(3)))


def preview_frame(path, source):
    """Historical frame admission, NOT native generation/capture admission."""
    raw = Path(path).read_bytes()
    value = json.loads(raw)
    if value.get("blueprint") != "KMG_Summoning_Unit_Aurochs" or \
            value.get("source") != "request-local summoned horse donor view" or \
            len(value.get("renderers", [])) != 1:
        raise SystemExit("unrecognized historical Aurochs preview frame")
    renderer = value["renderers"][0]
    rows = renderer["bones"]
    if renderer.get("renderer") != "horse_01" or renderer.get("mesh") != "KMG_aurochs_Original" or \
            renderer.get("boneCount") != 28 or renderer.get("bindPoseCount") != 28 or \
            len(rows) != 28 or {r["name"] for r in rows} != set(source.payload["bones"]):
        raise SystemExit("historical preview palette does not match original project Gorgon")
    at = {r["name"]: Vector(r["bindPosition"]) for r in rows}
    checks = [(25, "LowerTorso", (0, .49, 0)), (32, "UpperTorso", (0, .49, 0)),
              (39, "Chest", (0, .49, 0)), (40, "Head", (0, .16, .13)),
              (46, "Head", (-.28, .01, .20)), (51, "Head", (.28, .01, .20))]
    for first, side, sign in ((42, "L", -1), (47, "R", 1)):
        for j, name in enumerate(("Arm_Upper", "Arm_Lower", "Leg0_Upper", "Leg0_Lower")):
            checks.append((first + j, side + "_" + name, (sign * .11, 0, 0)))
    errors = [(bounds_centre(source, group) - at[name] - Vector(offset)).length
              for group, name, offset in checks]
    if max(errors) > 5e-6:
        raise SystemExit("historical frame points do not explain authored landmarks")
    sys.path.insert(0, str(HERE.parent / "ungulates"))
    import generate_ungulates as cattle
    rig, _, _ = cattle.measured_rig(path, "aurochs")
    return rig, {"kind": "historical-Aurochs-offline-preview-only", "sha256": hashlib.sha256(raw).hexdigest(),
                 "matchedPalette": 28, "matchedAuthoredLandmarks": len(checks),
                 "maximumLandmarkError": max(errors), "currentNativeBindCalibration": False}


def hoof(name, centre, half_width, half_depth, bottom, top):
    """Solid cloven half-hoof with flat sole and planar beveled sidewalls."""
    bm = bmesh.new()
    rings = []
    for y, scale in ((bottom, .88), (bottom + .025, 1), (top - .025, .94), (top, .82)):
        w, d = half_width * scale, half_depth * scale
        cut = .025
        shape = [(-w+cut, -d), (w-cut, -d), (w, -d+cut), (w, d-cut),
                 (w-cut, d), (-w+cut, d), (-w, d-cut), (-w, -d+cut)]
        rings.append([bm.verts.new((centre.x+x, y, centre.z+z)) for x, z in shape])
    for a, b in zip(rings, rings[1:]):
        for i in range(8):
            j = (i+1) % 8
            bm.faces.new((a[i], b[i], b[j], a[j]))
    bm.faces.new(tuple(rings[0]))
    bm.faces.new(tuple(reversed(rings[-1])))
    return cb._rigid_object(name, bm)


def add_plate(parts, tree, source, name, grid, region, layer=0):
    # Flank grids project outward from inside; the helmet projects down from
    # above. Surface normals place thin armour outside the actual body.
    placed = []
    for i, column in enumerate(grid):
        made = []
        for j, (origin, direction) in enumerate(column):
            hit, normal, _, _ = tree.ray_cast(Vector(origin), Vector(direction), 3)
            if hit is None:
                raise SystemExit("plate ray missed authored body: " + name)
            u, v = i/(len(grid)-1), j/(len(column)-1)
            bevel = min(1, min(u, 1-u, v, 1-v) / .10)
            made.append(hit + normal * (.024 + .012 * bevel + .0025*layer))
        placed.append(made)
    plate, _ = cb.sheet(name, placed, .012)
    # Hard rim normals distinguish sheet metal from a rounded cushion. Split
    # only high-dihedral crease edges after the closed shell is validated;
    # coincident copies receive identical transferred skin weights below.
    bm = bmesh.new(); bm.from_mesh(plate.data)
    sharp = [edge for edge in bm.edges if len(edge.link_faces) == 2 and
             edge.calc_face_angle() > math.radians(48)]
    bmesh.ops.split_edges(bm, edges=sharp)
    bm.to_mesh(plate.data); bm.free(); plate.data.update()
    edge = placed[0] + [c[-1] for c in placed[1:]] + \
           list(reversed(placed[-1][:-1])) + [c[0] for c in reversed(placed[1:-1])]
    PLATE_EDGES[region] = [(np.array(edge[i]), np.array(edge[(i+1) % len(edge)]))
                           for i in range(len(edge))]
    weights = source.transfer(plate, [0, 1, 2, 3])
    weights = normalize_rows(weights)
    seam_rows = {}
    for vertex in plate.data.vertices:
        key = tuple(round(value,5) for value in vertex.co)
        weights[vertex.index] = seam_rows.setdefault(key, weights[vertex.index])
    parts.append((plate, weights, region))


def build(source):
    PLATE_EDGES.clear()
    volume = cb.Volume(.018)
    # Deliberately heavy bovine proportions, anchored to the original authored
    # shoulder/hip/hoof locations. No native vertex is an authoring input.
    volume.ellipsoid((0, 1.53, -.05), AXES, (.56, .57, .94))
    volume.ellipsoid((0, 1.58, .48), AXES, (.57, .60, .49))
    volume.ellipsoid((0, 1.53, -.60), AXES, (.52, .53, .45))
    volume.sweep([(0, 1.63, .53), (0, 1.76, .87), (0, 2.05, 1.16)], [.45, .36, .29])
    volume.ellipsoid((0, 2.08, 1.20), AXES, (.30, .30, .33))
    volume.ellipsoid((0, 1.90, 1.55), AXES, (.275, .20, .29))
    volume.ellipsoid((0, 1.79, 1.51), AXES, (.23, .14, .24))
    for sign in (-1, 1):
        volume.sweep([(sign*.34, 1.57, .63), (sign*.335, 1.20, .79),
                      (sign*.335, .96, .67), (sign*.335, .56, .66),
                      (sign*.335, .28, .69)], [.25, .215, .17, .135, .125])
        volume.sweep([(sign*.34, 1.46, -.61), (sign*.31, 1.15, -.57),
                      (sign*.30, .96, -.60), (sign*.28, .55, -.78),
                      (sign*.28, .245, -.795)], [.26, .22, .175, .14, .125])
    volume.sweep([(0,1.57,-.84),(0,1.47,-1.06),(.02,1.30,-1.29),
                  (.035,1.15,-1.48),(.05,1.08,-1.63)], [.10,.075,.060,.045,.035])
    body = volume.polygonise("GorgonContinuousBovine")
    cb.finish_surface(body, 9500, smooth_iterations=22)
    rows = normalize_rows(source.transfer(body, BODY_GROUPS))
    parts = [(body, rows, 0)]
    tree = BVHTree.FromPolygons([v.co.copy() for v in body.data.vertices],
                               [tuple(p.vertices) for p in body.data.polygons], all_triangles=True)
    # Thin overlapping tapered lames: two staggered side rows and dorsal
    # saddles, curved over the actual continuous body, not inflated pillows.
    region = 10
    params = (0, .065, .23, .50, .77, .935, 1)
    for sign in (-1, 1):
        for row, (low, high) in enumerate(((.50, 1.23), (1.14, 1.90))):
            for column, zc in enumerate((-.69, -.25, .19, .57)):
                centre = zc + (.07 if row else 0)
                length = .60
                if column == 3:
                    # End the last flank lame before the shoulder hinge;
                    # projecting it across the neck/leg transition crumples it.
                    centre, length = .48 + .02*row, .46
                grid = []
                for u in params:
                    col = []
                    for v in params:
                        theta = low + (high-low)*v
                        taper = .79 + .21*min(1, min(v,1-v)/.16)
                        z = centre + (u-.5)*length*taper
                        col.append(((0,1.53,z), (sign*math.sin(theta),math.cos(theta),0)))
                    grid.append(col)
                add_plate(parts, tree, source, "GorgonFlankLame", grid, region,
                          layer=column+(1 if row==0 else 0)); region += 1
    for column, zc in enumerate((-.67, -.23, .21, .55)):
        grid = [[((0,1.53,zc+(u-.5)*.53), (math.sin(-.57+1.14*v),math.cos(-.57+1.14*v),0))
                 for v in params] for u in params]
        add_plate(parts, tree, source, "GorgonDorsalLame", grid, region, layer=column+3); region += 1
    grid = [[((-.215+.43*u,2.9,1.12+.33*v), (0,-1,0)) for v in params] for u in params]
    add_plate(parts, tree, source, "GorgonForeheadShield", grid, region)
    for group, name, colour in ((4,"GorgonHornLeft",3),(11,"GorgonHornRight",3),
                                (46,"GorgonEyeLeft",4),(51,"GorgonEyeRight",4)):
        obj, weights = source.component(group, name)
        parts.append((obj, weights, colour))
    for side, sign in (("L",-1),("R",1)):
        for front in (True,False):
            bone = side + ("_Fingers" if front else "_Toes0")
            for offset in (-.077,.077):
                centre = Vector((sign*(.335 if front else .28)+offset,0,.715 if front else -.77))
                obj = hoof("GorgonClovenHoof", centre, .070, .175,
                           .107 if front else .072, .31 if front else .28)
                parts.append((obj,cb.rigid_rows(obj,[(bone,1)]),2))
        ear, _ = cb.sheet("GorgonBovineEar", [[Vector((sign*(.24+.28*u),2.12+.05*math.sin(math.pi*u),
            1.07+(v-.5)*(.10+.11*math.sin(math.pi*u)))) for v in (0,.5,1)] for u in (0,.25,.5,.75,1)],.012)
        parts.append((ear,cb.rigid_rows(ear,[("Head",1)]),0))
        nose = cb.blob("GorgonNostril", Vector((sign*.165,1.94,1.815)), AXES, (.065,.038,.018),segments=14,rings=8)
        parts.append((nose,cb.rigid_rows(nose,[("Head",1)]),5))
    obj, weights = cb.assemble("GorgonCleanup2",parts)
    cb.require_single_body(obj)
    cb.unwrap(obj)
    return obj, weights


def original_colour(points, normals, regions):
    noise = cb.fbm(points, 9, 26011, octaves=3)
    grain = cb.value_noise(points, 95, 26012)
    rgb = np.array((.235,.280,.295))[None,:] * (.86+.18*noise[:,None]+.035*grain[:,None])
    rgb += np.clip(normals[:,1],0,1)[:,None]*np.array((.025,.027,.028))
    for region, edges in PLATE_EDGES.items():
        mask = regions == region
        p = points[mask]
        if not len(p): continue
        distance = np.full(len(p),10.)
        for a,b in edges:
            span=b-a
            t=np.clip(((p-a)@span)/max(float(span@span),1e-12),0,1)
            distance=np.minimum(distance,np.linalg.norm(p-a-t[:,None]*span,axis=1))
        rim=np.exp(-(distance/.025)**2)
        brushed=.014*np.sin(p[:,1]*160+p[:,2]*31)
        rgb[mask]=np.array((.48,.55,.57))*(.88+.16*noise[mask,None]+brushed[:,None])
        rgb[mask]+=rim[:,None]*np.array((.18,.18,.18))
    rgb[regions==2]=np.array((.105,.125,.135))*(.90+.12*noise[regions==2,None])
    rgb[regions==3]=np.array((.61,.59,.46))*(.88+.12*noise[regions==3,None])
    rgb[regions==4]=(.83,.49,.055)
    rgb[regions==5]=(.027,.033,.035)
    return np.clip(rgb,0,1)


def colour(points, normals, regions):
    """Gunmetal proposal on the known authored plate regions only."""
    rgb = original_colour(points, normals, regions)
    noise = cb.fbm(points, 9, 26011, octaves=3)
    for region, edges in PLATE_EDGES.items():
        mask = regions == region
        p = points[mask]
        if not len(p): continue
        distance = np.full(len(p), 10.)
        for a, b in edges:
            span = b-a
            t = np.clip(((p-a)@span)/max(float(span@span), 1e-12), 0, 1)
            distance = np.minimum(distance, np.linalg.norm(p-a-t[:,None]*span, axis=1))
        # The old .025 rim/.18 highlight painted broad white piping. Keep
        # a narrow worn edge, dark overlap falloff and restrained direction
        # strokes. These are albedo marks, not shader/metalness claims.
        rim = np.exp(-(distance/.008)**2)
        overlap = .07*np.exp(-(distance/.035)**2)
        brushed = .018*np.sin(p[:,1]*160+p[:,2]*31) + .005*np.sin(p[:,1]*393-p[:,2]*17)
        rgb[mask] = np.array((.285,.330,.345))*(.86+.15*noise[mask,None]+brushed[:,None]-overlap[:,None])
        rgb[mask] += rim[:,None]*np.array((.055,.060,.062))
    return np.clip(rgb, 0, 1)


def read_original_rgb_png(path):
    """Read only our deterministic RGB8/no-filter encoder's own output."""
    data = Path(path).read_bytes()
    assert data[:8] == b"\x89PNG\r\n\x1a\n" and data[24:29] == bytes((8,2,0,0,0))
    width, height = struct.unpack_from(">II", data, 16)
    at, compressed = 8, bytearray()
    while at < len(data):
        length = struct.unpack_from(">I", data, at)[0]
        kind, payload = data[at+4:at+8], data[at+8:at+8+length]
        if kind == b"IDAT": compressed.extend(payload)
        at += length + 12
    rows = np.frombuffer(zlib.decompress(compressed), dtype=np.uint8).reshape(height, width*3+1)
    assert not rows[:,0].any(), "Unknown PNG filtering"
    return rows[:,1:].reshape(height, width, 3).copy()


def bake_plate_only_paint(obj, albedo, work):
    baseline, candidate, ownership = (work/filename for filename in
        ("gorgon-before-paint.png", "gorgon-plate-proposal.png", "gorgon-plate-pixel-mask.png"))
    cb.bake_albedo(obj, 1024, original_colour, baseline)
    cb.bake_albedo(obj, 1024, colour, candidate)
    def mask_colour(points, normals, regions):
        return np.repeat(((regions >= 10) & (regions <= 30))[:,None], 3, axis=1).astype(float)
    cb.bake_albedo(obj, 1024, mask_colour, ownership)
    old, new, mask = (read_original_rgb_png(path) for path in (baseline, candidate, ownership))
    baseline_sha = hashlib.sha256(baseline.read_bytes()).hexdigest()
    if baseline_sha != PAINT_BASELINE_SHA:
        raise SystemExit("previous retained paint did not reproduce exactly")
    # Mask follows the exact shared UV rasterizer and padding ownership.
    # Unowned background mean changes from a full rebake are excluded too.
    plate = (mask == 255).all(axis=2)
    result = old.copy(); result[plate] = new[plate]
    assert np.array_equal(result[~plate], old[~plate])
    assert set(PLATE_EDGES) == set(range(10,31))
    albedo.write_bytes(cb.encode_png(result))
    return {"plateOwnedPixelsIncludingPadding": int(plate.sum()),
        "changedPlatePixels": int(np.any(result != old, axis=2).sum()),
        "nonPlatePixelsChanged": int(np.any(result[~plate] != old[~plate], axis=1).sum()),
        "baselineAlbedoSha256": baseline_sha,
        "plateRegions": list(range(10,31)), "shaderOrParameterChanges": False}


def audit_poses(path, armature, out):
    """Skin EXACT exported bytes with synthetic historical-frame rotations."""
    payload = json.loads(path.read_text())
    original = base64.b64decode(payload["data"])
    n = payload["vertexCount"]; t = payload["triangleCount"]
    reports = {}
    for name, rotations in POSES.items():
        for bone in armature.pose.bones:
            bone.rotation_mode="XYZ"; bone.rotation_euler=(0,0,0)
        for bone, degrees in rotations.items():
            armature.pose.bones[bone].rotation_euler=Euler(tuple(math.radians(v) for v in degrees))
        bpy.context.view_layer.update()
        matrices=[armature.pose.bones[bone].matrix @ armature.data.bones[bone].matrix_local.inverted()
                  for bone in payload["bones"]]
        data=bytearray(original)
        for i in range(n):
            p=Vector(struct.unpack_from("<3f",original,i*12))
            normal=Vector(struct.unpack_from("<3f",original,n*12+i*12))
            result=Vector(); direction=Vector()
            for slot in range(4):
                index, weight=struct.unpack_from("<if",original,n*32+t*12+i*32+slot*8)
                result += (matrices[index] @ p)*weight
                direction += (matrices[index].to_3x3() @ normal)*weight
            struct.pack_into("<3f",data,i*12,*result)
            struct.pack_into("<3f",data,n*12+i*12,*direction.normalized())
        posed=dict(payload,data=base64.b64encode(data).decode("ascii"),
                    syntheticPose=name,nativeMotionEvidence=False)
        target=out/("gorgon-"+name+"-mesh.json")
        target.write_text(json.dumps(posed,indent=1)+"\n",encoding="utf-8",newline="\n")
        _, components, report=inspect(target)
        reports[name]=dict(status="PASS",**report)
    for bone in armature.pose.bones: bone.rotation_euler=(0,0,0)
    bpy.context.view_layer.update()
    return reports


def main():
    start=time.monotonic()
    parser=argparse.ArgumentParser()
    parser.add_argument("--output-dir",required=True)
    parser.add_argument("--work-dir",required=True)
    parser.add_argument("--input-mesh",default=str(HERE/"cleanup2-gorgon-input.json"))
    parser.add_argument("--preview-rig",default=None)
    args=parser.parse_args(sys.argv[sys.argv.index("--")+1:])
    out, work=Path(args.output_dir).resolve(),Path(args.work_dir).resolve()
    out.mkdir(parents=True,exist_ok=True);work.mkdir(parents=True,exist_ok=True)
    source=ProjectInput(args.input_mesh)
    if hashlib.sha256(source.path.read_bytes()).hexdigest()!=INPUT_SHA or \
            source.payload.get("nativeDonor")!=DONOR or len(source.payload["bones"])!=28:
        raise SystemExit("exact retained project-authored Gorgon input required")
    rig, preview=(preview_frame(args.preview_rig,source) if args.preview_rig else (None,None))
    bpy.ops.wm.read_factory_settings(use_empty=True)
    obj,weights=build(source)
    albedo=out/"gorgon-albedo.png"
    paint_report = bake_plate_only_paint(obj, albedo, work)
    cb.attach_albedo_preview(obj,albedo)
    extra={k:v for k,v in source.payload.items() if k not in {
        "data","vertexCount","triangleCount","bones","albedo","uvAtlas","winding"}}
    extra.update({"authoringRevision":"cleanup2-continuous-bovine-thin-lames",
                  "sourceGeometrySha256":INPUT_SHA,"maxInfluencesPerVertex":4,
                  "thinConformalPlates":21,"flatClovenHoofHalves":8,
                  "paintRevision":"cleanup2-gunmetal-plates-only"})
    path=out/"gorgon-mesh.json"
    cb.write_mesh_data(path,obj,weights,source.payload["rigSha256"],albedo,source.payload["space"],extra)
    payload,components,report=inspect(path)
    if hashlib.sha256(base64.b64decode(payload["data"])).hexdigest() != PAINT_GEOMETRY_SHA:
        raise SystemExit("paint-only checkpoint changed decoded geometry/normals/UV/indices/weights")
    paint_report["decodedMeshBlobSha256Unchanged"] = PAINT_GEOMETRY_SHA
    if payload["bones"]!=source.payload["bones"]: raise SystemExit("published named driver palette/order changed")
    if not any(set(c["drivers"])==set(payload["bones"]) for c in components):
        raise SystemExit("continuous bovine body does not carry all published drivers")
    for terminal in ("L_Fingers","R_Fingers","L_Toes0","R_Toes0"):
        if sum(set(c["drivers"])=={terminal} for c in components)!=2:
            raise SystemExit("each foot must retain two separate rigid hoof halves")
    positions={tuple(vertex.co) for vertex in obj.data.vertices}
    for group in (4,11,46,51):
        ids={i for face in source.groups[group] for i in source.triangles[face]}
        if not all(tuple(source.positions[i]) in positions for i in ids):
            raise SystemExit("original horn/eye authored geometry changed")
    report["anatomyContracts"]={"continuousBodyCarriesAllDrivers":True,"exactBonePaletteAndOrder":28,
        "rigidClovenHoofHalves":8,"originalHornAndEyePointSetsPreserved":4,"thinConformalPlates":21}
    report["paintOnly"] = paint_report
    if rig:
        armature=cb.review_armature(rig,"HistoricalAurochsOfflinePreview")
        for name in payload["bones"]: obj.vertex_groups.new(name=name)
        for index,row in weights.items():
            for bone,weight in row: obj.vertex_groups[bone].add([index],weight,"REPLACE")
        modifier=obj.modifiers.new("HistoricalPreviewOnly","ARMATURE");modifier.object=armature
        obj.parent=armature
        pose_dir=work/"exported-poses";pose_dir.mkdir(exist_ok=True)
        (pose_dir/albedo.name).write_bytes(albedo.read_bytes())
        report["exportedSyntheticPoses"]=audit_poses(path,armature,pose_dir)
    bpy.ops.wm.save_as_mainfile(filepath=str(work/"gorgon-source.blend"))
    report.update({"status":"PASS","scope":"actual exported authored geometry/weights; synthetic poses only",
        "sourceGeometrySha256":INPUT_SHA,"historicalPreview":preview,"nativeVisualVerified":False,
        "ownerVisualApproval":"pending","elapsedSeconds":time.monotonic()-start})
    (work/"gorgon-report.json").write_text(json.dumps(report,indent=2)+"\n",encoding="utf-8",newline="\n")
    print(json.dumps(report,indent=2))


if __name__=="__main__": main()
