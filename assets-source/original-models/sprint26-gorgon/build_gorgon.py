"""Original modest armored-bull skin on the measured Horse frame.

Only original geometry/UVs/albedo and native bone NAMES are exported. The
geometry-free native capture and editable rig preview remain machine-local.
"""
import argparse
import base64
import hashlib
import json
import math
from pathlib import Path
import struct
import sys
import zlib
import bpy
import bmesh
from mathutils import Vector

sys.dont_write_bytecode = True
MODELS = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(MODELS / "ungulates"))
import generate_ungulates as cattle
shared = cattle.shared
shared.ATLAS = {"eye": (.94, .92, .995, .98), **shared.ATLAS}
DONOR = "5bb9579fdb2b26b48bb10d61c81cfdfb"
PREFAB = "6032f7a5751c1104aa59647b176634c8"
SPACE = "donor renderer local; +X right, +Y up, +Z forward"


def png_chunk(kind, data):
    return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", zlib.crc32(kind + data) & 0xffffffff)


def paint(path):
    # Original matte iron, horn, muzzle and hoof atlas; no native pixels.
    size = 512
    palette = {"body": (78, 91, 99), "limbs": (60, 70, 76),
               "crest": (126, 125, 107), "beak": (47, 54, 54),
               "membrane": (116, 128, 133), "eye": (201, 150, 55)}
    rows = bytearray()
    for y in range(size):
        rows.append(0)
        for x in range(size):
            u, v = (x + .5) / size, 1 - (y + .5) / size
            colour = (65, 74, 78)
            for name, region in shared.ATLAS.items():
                if region[0] <= u <= region[2] and region[1] <= v <= region[3]:
                    colour = palette.get(name, (80, 88, 91))
                    break
            grain = 1 + .055 * math.sin(x * .39 + y * .17) + .035 * math.sin(x * .07 - y * .29)
            rows.extend(max(0, min(255, round(c * grain))) for c in colour)
            rows.append(255)
    header = struct.pack(">IIBBBBB", size, size, 8, 6, 0, 0, 0)
    path.write_bytes(b"\x89PNG\r\n\x1a\n" + png_chunk(b"IHDR", header) +
                     png_chunk(b"IDAT", zlib.compress(bytes(rows), 9)) + png_chunk(b"IEND", b""))


def armour(bm, weights, uvs, rig):
    at = lambda name: Vector(rig[name]["head"])
    # Separate overlapping closed plates follow their owning torso joints.
    # This is deliberately modest geometry rather than articulated plate physics.
    for bone in ("LowerTorso", "UpperTorso", "Chest"):
        centre = at(bone)
        for sign in (-1, 1):
            for level in range(3):
                p = centre + Vector((sign * (.44 + .025 * level),
                                      .32 - .22 * level, -.05 + .025 * level))
                cattle.ellipsoid(bm, weights, uvs, p,
                    Vector((.14, .20, .31)), bone, "membrane", 4, 10)
        cattle.ellipsoid(bm, weights, uvs, centre + Vector((0, .49, 0)),
            Vector((.43, .12, .30)), bone, "membrane", 4, 10)
    skull = at("Head")
    cattle.ellipsoid(bm, weights, uvs, skull + Vector((0, .16, .13)),
        Vector((.31, .16, .34)), "Head", "membrane", 5, 12)
    cattle.ellipsoid(bm, weights, uvs, skull + Vector((0, -.06, .39)),
        Vector((.26, .11, .16)), "Head", "membrane", 4, 12)
    for side, sign in (("L", -1), ("R", 1)):
        for part in ("Arm_Upper", "Arm_Lower", "Leg0_Upper", "Leg0_Lower"):
            bone = side + "_" + part
            cattle.ellipsoid(bm, weights, uvs, at(bone) + Vector((sign * .11, 0, 0)),
                Vector((.16, .18, .18)), bone, "membrane", 4, 10)
        # Two small living amber eyes remain visible below the helmet.
        cattle.ellipsoid(bm, weights, uvs, skull + Vector((sign * .28, .01, .20)),
            Vector((.04, .04, .045)), "Head", "eye", 4, 8)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--capture", required=True)
    parser.add_argument("--output-dir", required=True)
    parser.add_argument("--work-dir", required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    output, work = Path(args.output_dir), Path(args.work_dir)
    output.mkdir(parents=True, exist_ok=True)
    work.mkdir(parents=True, exist_ok=True)
    raw = Path(args.capture).read_bytes()
    captured = json.loads(raw)
    rows = [v for v in captured["donors"] if v["key"] == "gorgon" and
            v["nativeBlueprint"] == DONOR and v["prefab"] == PREFAB]
    if len(rows) != 1 or len(rows[0]["skinnedRenderers"]) != 1:
        raise SystemExit("exact single measured Horse renderer required")
    skin = rows[0]["skinnedRenderers"][0]
    if skin["renderer"] != "horse_01" or skin["boneCount"] != 31:
        raise SystemExit("Horse renderer contract changed")
    translated = {"source": "Sprint26 horse geometry-free bind capture",
                  "space": captured["donors"][0]["space"], "renderers": [skin]}
    translated_path = work / "horse-bind-input.json"
    translated_path.write_text(json.dumps(translated), encoding="utf-8")
    rig_data, rig, rig_hash = cattle.measured_rig(translated_path, "aurochs")
    bpy.ops.wm.read_factory_settings(use_empty=True)
    armature = shared.build_armature(rig_data)
    mesh = bpy.data.meshes.new("GorgonOriginalMesh")
    obj = bpy.data.objects.new("GorgonOriginal", mesh)
    bpy.context.collection.objects.link(obj)
    bm = bmesh.new(); weights, uvs = {}, {}
    # Reuse project-owned bovine construction, never a native bull mesh.
    cattle.cattle(bm, weights, uvs, rig, "aurochs")
    armour(bm, weights, uvs, rig)
    # Closed project primitives arrive with mixed winding. Normalize each
    # connected surface, then use the orientation of the visible native plates
    # under the shared exporter’s final handedness reversal.
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bmesh.ops.reverse_faces(bm, faces=list(bm.faces))
    bm.normal_update(); bm.to_mesh(mesh)
    indexed = {v.index: entries for v, entries in weights.items()}
    uv_indexed = {v.index: uv for v, uv in uvs.items()}
    bm.free()
    layer = mesh.uv_layers.new(name="Albedo")
    for loop in mesh.loops:
        layer.data[loop.index].uv = uv_indexed[loop.vertex_index]
    albedo = output / "gorgon-albedo.png"
    paint(albedo)
    report = {"creature": "gorgon", "rigSha256": rig_hash,
              "captureSha256": hashlib.sha256(raw).hexdigest(),
              "nativeDonor": DONOR, "nativePrefab": PREFAB,
              "vertices": len(mesh.vertices), "polygons": len(mesh.polygons),
              "visibleLegs": 4, "horns": 2, "armourPlates": True,
              "maxInfluencesPerVertex": max(map(len, indexed.values()))}
    path = output / "gorgon-mesh.json"
    shared.write_mesh_data(path, obj, mesh, indexed, uv_indexed, report, albedo)
    payload = json.loads(path.read_text(encoding="utf-8"))
    # Preserve the native-visible face orientation while lighting the closed
    # surfaces outward. Reversing Blender faces also reversed its vertex normals;
    # the shared exporter stores those separately from its handedness flip.
    data = bytearray(base64.b64decode(payload["data"]))
    for offset in range(payload["vertexCount"] * 12, payload["vertexCount"] * 24, 4):
        struct.pack_into("<f", data, offset, -struct.unpack_from("<f", data, offset)[0])
    payload["data"] = base64.b64encode(data).decode("ascii")
    payload.update({"creature": "gorgon", "nativeDonor": DONOR,
                    "space": SPACE, "visibleLegs": 4, "horns": 2,
                    "armourPlates": True})
    path.write_text(json.dumps(payload, indent=1) + "\n", encoding="utf-8", newline="\n")
    groups = {}
    for index, entries in indexed.items():
        for name, value in entries:
            if name not in groups:
                groups[name] = obj.vertex_groups.new(name=name)
            groups[name].add([index], value, "REPLACE")
    modifier = obj.modifiers.new("Armature", "ARMATURE"); modifier.object = armature
    obj.parent = armature
    shared.attach_preview_material(obj, mesh, str(albedo))
    bpy.ops.wm.save_as_mainfile(filepath=str(work / "gorgon-original.blend"))
    report["bones"] = payload["bones"]
    report["meshSha256"] = hashlib.sha256(path.read_bytes()).hexdigest()
    report["albedoSha256"] = hashlib.sha256(albedo.read_bytes()).hexdigest()
    (work / "generation-report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print("[gorgon-original] " + json.dumps(report))


if __name__ == "__main__":
    main()
