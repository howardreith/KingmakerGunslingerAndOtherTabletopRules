"""Generate the bounded Musket fit experiment and source-authored markers."""
import bpy
import datetime
import hashlib
import json
import math
import os
import struct
import zlib
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[2]
SOURCE_FBX = (REPO / "assets-source" / "third-party" / "models" /
              "mesh-masters-rifle-musket" / "source" / "Musket 01.fbx")
BLEND = ROOT / "musket-fit-candidates.blend"
REPORT = ROOT / "musket-fit-candidates-build-report.json"
RENDERS = ROOT / "renders"

MARKERS = {
    "KMG_Grip": (0.0, 0.0, 0.0),
    # FBX import into Unity mirrors Blender X; this authors the runtime target
    # at (-0.030976,-0.051069,0.586040).
    "KMG_Support": (0.030976, -0.051069, 0.586040),
    "KMG_Butt": (0.0, 0.0, -0.169533),
    "KMG_Muzzle": (0.0, 0.0, 1.180452),
}

if os.environ.get("PYTHONHASHSEED") != "0":
    raise RuntimeError("Deterministic generation requires PYTHONHASHSEED=0")
if not SOURCE_FBX.is_file():
    raise RuntimeError("Preserved Mesh Masters source is missing: " + str(SOURCE_FBX))


def install_deterministic_fbx_contract():
    import io_scene_fbx.export_fbx_bin as exporter
    import io_scene_fbx.fbx_utils as fbx_utils
    real_datetime = datetime.datetime
    real_export_uuid = exporter.get_fbx_uuid_from_key
    real_utils_uuid = fbx_utils.get_fbx_uuid_from_key
    stable_ids = {}
    used_ids = set()

    class FixedDateTime(real_datetime):
        @classmethod
        def now(cls, tz=None):
            value = cls(1970, 1, 1, 10, 0, 0)
            return value if tz is None else tz.fromutc(value.replace(tzinfo=tz))

    def stable_uuid(key):
        if isinstance(key, int) and 0 <= key < 2 ** 63:
            return fbx_utils.UUID(key)
        if key in stable_ids:
            return fbx_utils.UUID(stable_ids[key])
        value = int.from_bytes(hashlib.sha256(repr(key).encode("utf-8"))
                               .digest()[:8], "big") & ((1 << 63) - 1)
        if value == 0:
            value = 1
        while value in used_ids:
            value = (value + 1) & ((1 << 63) - 1)
        stable_ids[key] = value
        used_ids.add(value)
        return fbx_utils.UUID(value)

    exporter.datetime.datetime = FixedDateTime
    exporter.get_fbx_uuid_from_key = stable_uuid
    fbx_utils.get_fbx_uuid_from_key = stable_uuid
    return exporter, fbx_utils, real_datetime, real_export_uuid, real_utils_uuid


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for collection in (bpy.data.meshes, bpy.data.materials, bpy.data.cameras,
                       bpy.data.lights):
        for value in list(collection):
            if value.users == 0:
                collection.remove(value)


def material(name, color, metallic, roughness):
    value = bpy.data.materials.new(name)
    value.diffuse_color = (*color, 1.0)
    value.use_nodes = True
    node = value.node_tree.nodes.get("Principled BSDF")
    node.inputs["Base Color"].default_value = (*color, 1.0)
    node.inputs["Metallic"].default_value = metallic
    node.inputs["Roughness"].default_value = roughness
    return value


def parent_keep_world(obj, root):
    matrix = obj.matrix_world.copy()
    obj.parent = root
    obj.matrix_world = matrix


def apply_mesh_transform(obj):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    obj.select_set(False)


def cylinder(name, radius, depth, location, mat, vertices=16):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius,
                                       depth=depth, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.data.name = name + "Mesh"
    obj.data.materials.append(mat)
    bpy.ops.object.shade_smooth()
    return obj


def box(name, dimensions, location, mat):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.data.name = name + "Mesh"
    obj.dimensions = dimensions
    obj.data.materials.append(mat)
    apply_mesh_transform(obj)
    return obj


def box_between(name, start, end, width, depth, mat):
    start, end = Vector(start), Vector(end)
    vector = end - start
    obj = box(name, (width, depth, vector.length), (start + end) / 2, mat)
    obj.rotation_euler = vector.to_track_quat("Z", "Y").to_euler()
    apply_mesh_transform(obj)
    return obj


def add_markers(root, values=MARKERS):
    for name in sorted(values):
        marker = bpy.data.objects.new(name, None)
        bpy.context.collection.objects.link(marker)
        marker.empty_display_type = "PLAIN_AXES"
        marker.empty_display_size = 0.045
        marker.location = values[name]
        marker.parent = root


def build_pass_through():
    """Round-trip the preserved source without changing its mesh geometry."""
    clear_scene()
    bpy.ops.import_scene.fbx(filepath=str(SOURCE_FBX), use_anim=False)
    imported = list(bpy.context.scene.objects)
    if not any(obj.type == "MESH" for obj in imported):
        raise RuntimeError("Pass-through import produced no mesh")
    root = bpy.data.objects.new("MusketPassThrough", None)
    bpy.context.collection.objects.link(root)
    for obj in imported:
        if obj.parent is None:
            parent_keep_world(obj, root)
    # These are the exact legacy source-space points consumed by the current
    # Unity transform. They make the diagnostic round-trip self-describing.
    # Blender's standards-compliant FBX import presents the source's long axis
    # as +X (the original Unity import presents it as -X). The opposite Unity
    # Y rotation below is therefore an equivalent round-trip, not a mesh edit.
    source_markers = {
        "KMG_Grip": (-0.0400, 0.0, 0.0),
        "KMG_Support": (0.1000, -0.0122, -0.0074),
        "KMG_Butt": (-0.0805, 0.0, 0.0),
        "KMG_Muzzle": (0.2420, 0.0, 0.0),
    }
    add_markers(root, source_markers)
    return root


def build_project_candidate(name, mode):
    clear_scene()
    root = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(root)
    wood = material(name + "_GrayboxWood", (0.25, 0.13, 0.06), 0.0, 0.58)
    steel = material(name + "_GrayboxSteel", (0.24, 0.28, 0.31), 0.72, 0.25)
    brass = material(name + "_GrayboxFurniture", (0.45, 0.29, 0.08), 0.60, 0.28)
    objects = []

    # The barrel/muzzle, firing grip, lock reference, and physical fore-end are
    # identical between controls. +Z is the firing axis and the grip is origin.
    objects.append(cylinder("Barrel", 0.018, 1.105, (0.0, 0.0, 0.6275), steel, 20))
    objects.append(cylinder("MuzzleBand", 0.026, 0.042, (0.0, 0.0, 1.159), brass, 20))
    objects.append(box("Lock", (0.062, 0.040, 0.170), (0.0, -0.006, 0.105), steel))
    objects.append(box("Furniture", (0.075, 0.052, 0.055), (0.0, -0.006, 0.010), brass))
    # The support point lies on the lower-left fore-end surface, not in air.
    objects.append(box("ForeEnd", (0.068, 0.104, 0.530),
                       (-0.002, 0.0, 0.505), wood))
    objects.append(cylinder("Ramrod", 0.006, 0.835, (0.027, -0.032, 0.672), brass, 12))

    if mode == "minimal":
        objects.append(box_between("Stock", (0.0, 0.0, 0.02),
                                   (0.0, 0.0, -0.080), 0.052, 0.034, wood))
        objects.append(box("ButtCap", (0.058, 0.039, 0.018),
                           (0.0, 0.0, -0.080), brass))
    elif mode == "clearance":
        # Narrow torso-facing thickness and a dropped, curved centerline move
        # volume around the immutable pose without translating the whole rig.
        objects.append(box_between("StockFront", (0.0, 0.0, 0.025),
                                   (0.018, 0.0, -0.055), 0.056, 0.030, wood))
        objects.append(box_between("StockDrop", (0.018, 0.0, -0.055),
                                   (0.048, 0.0, -0.125), 0.064, 0.028, wood))
        objects.append(box_between("StockButt", (0.048, 0.0, -0.125),
                                   (0.0, 0.0, -0.169533), 0.074, 0.027, wood))
        objects.append(box("ButtCap", (0.078, 0.032, 0.020),
                           (0.0, 0.0, -0.169533), brass))
    else:
        raise RuntimeError("Unknown candidate mode " + mode)

    for obj in objects:
        obj.parent = root
    add_markers(root)
    return root


def select_tree(root):
    root.select_set(True)
    for child in root.children_recursive:
        child.select_set(True)


def export(root, filename):
    bpy.ops.object.select_all(action="DESELECT")
    select_tree(root)
    bpy.context.view_layer.objects.active = root
    path = ROOT / filename
    bpy.ops.export_scene.fbx(filepath=str(path), use_selection=True,
                             apply_unit_scale=True,
                             apply_scale_options="FBX_SCALE_UNITS",
                             object_types={"EMPTY", "MESH"},
                             add_leaf_bones=False, bake_anim=False,
                             axis_forward="-Z", axis_up="Y")
    return path


def bounds_for(root):
    points = []
    for obj in root.children_recursive:
        if obj.type == "MESH":
            points.extend(obj.matrix_world @ Vector(corner) for corner in obj.bound_box)
    if not points:
        raise RuntimeError(root.name + " has empty visible hierarchy")
    low = Vector((min(p.x for p in points), min(p.y for p in points),
                  min(p.z for p in points)))
    high = Vector((max(p.x for p in points), max(p.y for p in points),
                   max(p.z for p in points)))
    return low, high


def render(root, filename):
    low, high = bounds_for(root)
    center = (low + high) / 2
    extent = max((high - low).length, 0.5)
    camera_data = bpy.data.cameras.new(root.name + "RenderCamera")
    camera = bpy.data.objects.new(root.name + "RenderCamera", camera_data)
    bpy.context.collection.objects.link(camera)
    camera.location = center + Vector((extent * 1.10, -extent * 1.55, extent * 0.24))
    camera.rotation_euler = (center - camera.location).to_track_quat("-Z", "Y").to_euler()
    camera_data.type = "ORTHO"
    camera_data.ortho_scale = extent * 1.12
    light_data = bpy.data.lights.new(root.name + "Key", "AREA")
    light = bpy.data.objects.new(root.name + "Key", light_data)
    bpy.context.collection.objects.link(light)
    light.location = center + Vector((extent, -extent, extent))
    light.rotation_euler = (center - light.location).to_track_quat("-Z", "Y").to_euler()
    light_data.energy = 800
    light_data.shape = "DISK"
    light_data.size = extent
    scene = bpy.context.scene
    scene.camera = camera
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    scene.render.resolution_x = 900
    scene.render.resolution_y = 420
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = True
    path = RENDERS / filename
    scene.render.filepath = str(path)
    bpy.ops.render.render(write_still=True)
    normalize_png(path)
    bpy.data.objects.remove(camera, do_unlink=True)
    bpy.data.objects.remove(light, do_unlink=True)
    return path


def normalize_png(path):
    source = path.read_bytes()
    output = bytearray(source[:8])
    offset = 8
    while offset < len(source):
        length = struct.unpack(">I", source[offset:offset + 4])[0]
        chunk_type = source[offset + 4:offset + 8]
        payload = source[offset + 8:offset + 8 + length]
        offset += length + 12
        if chunk_type in {b"tEXt", b"eXIf", b"oFFs", b"pHYs"}:
            continue
        output.extend(struct.pack(">I", length) + chunk_type + payload)
        output.extend(struct.pack(">I", zlib.crc32(chunk_type + payload) & 0xFFFFFFFF))
    path.write_bytes(output)


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def describe(root, fbx, image, provenance, markers=MARKERS):
    low, high = bounds_for(root)
    meshes = [obj for obj in root.children_recursive if obj.type == "MESH"]
    return {
        "name": root.name,
        "fbx": fbx.name,
        "fbxSha256": sha256(fbx),
        "render": str(image.relative_to(ROOT)).replace("\\", "/"),
        "renderSha256": sha256(image),
        "meshCount": len(meshes),
        "vertexCount": sum(len(obj.data.vertices) for obj in meshes),
        "triangleCount": sum(len(poly.vertices) - 2 for obj in meshes
                             for poly in obj.data.polygons),
        "materialCount": len({slot.material.name for obj in meshes
                              for slot in obj.material_slots if slot.material}),
        "boundsMeters": {
            "minimum": [round(value, 6) for value in low],
            "maximum": [round(value, 6) for value in high],
            "size": [round(value, 6) for value in high - low],
        },
        "markersMeters": {key: list(value) for key, value in markers.items()},
        "provenance": provenance,
    }


RENDERS.mkdir(parents=True, exist_ok=True)
bpy.context.preferences.filepaths.save_version = 0
exporter, fbx_utils, real_datetime, real_export_uuid, real_utils_uuid = \
    install_deterministic_fbx_contract()
records = []
try:
    root = build_pass_through()
    fbx = export(root, "musket-pass-through.fbx")
    image = render(root, "musket-pass-through-source.png")
    records.append(describe(root, fbx, image,
        "Mesh Masters Flintlock Rifle derivative; CC-BY-4.0", {
            "KMG_Grip": (-0.0400, 0.0, 0.0),
            "KMG_Support": (0.1000, -0.0122, -0.0074),
            "KMG_Butt": (-0.0805, 0.0, 0.0),
            "KMG_Muzzle": (0.2420, 0.0, 0.0),
        }))

    root = build_project_candidate("MusketMinimalControl", "minimal")
    fbx = export(root, "musket-minimal-control.fbx")
    image = render(root, "musket-minimal-control-source.png")
    records.append(describe(root, fbx, image, "project-owned clean-room graybox"))

    root = build_project_candidate("MusketClearanceStock", "clearance")
    fbx = export(root, "musket-clearance-stock.fbx")
    image = render(root, "musket-clearance-stock-source.png")
    records.append(describe(root, fbx, image, "project-owned clean-room graybox"))

    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND), compress=True)
finally:
    exporter.datetime.datetime = real_datetime
    exporter.get_fbx_uuid_from_key = real_export_uuid
    fbx_utils.get_fbx_uuid_from_key = real_utils_uuid

report = {
    "schemaVersion": 1,
    "generator": "generate_musket_fit_candidates.py",
    "blenderVersion": bpy.app.version_string,
    "unitSystem": "METRIC",
    "axis": "+Z muzzle; identity firing-hand grip",
    "sourceFbx": str(SOURCE_FBX.relative_to(REPO)).replace("\\", "/"),
    "sourceFbxSha256": sha256(SOURCE_FBX),
    "semanticLengthMeters": round(Vector(MARKERS["KMG_Muzzle"])
                                  .__sub__(Vector(MARKERS["KMG_Butt"])).length, 6),
    "candidates": records,
    "blend": BLEND.name,
    "blendSha256": sha256(BLEND),
    "determinism": "FBX and normalized PNG must match across clean runs; .blend embeds session metadata and is semantic-only reproducible",
}
REPORT.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
print(json.dumps(report, indent=2))
