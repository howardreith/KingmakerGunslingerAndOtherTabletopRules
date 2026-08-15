"""Generate deterministic project-owned Pistol.Duelist and Pistol.LastWord."""
import bpy
import datetime
import hashlib
import json
import os
import struct
import zlib
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parent
BLEND = ROOT / "firearm-pistol-variants.blend"
REPORT = ROOT / "firearm-pistol-variants-build-report.json"
RENDERS = ROOT / "renders"
MARKERS = {
    "KMG_Grip": (0.0, 0.0, 0.0),
    # Authored for validation even though one-handed rigs create no IK target.
    "KMG_Support": (0.0, -0.020, 0.145),
    "KMG_Butt": (0.0, 0.0, -0.075),
    "KMG_Muzzle": (0.0, 0.0, 0.264),
}

if os.environ.get("PYTHONHASHSEED") != "0":
    raise RuntimeError("Deterministic generation requires PYTHONHASHSEED=0")


def install_deterministic_fbx_contract():
    import io_scene_fbx.export_fbx_bin as exporter
    import io_scene_fbx.fbx_utils as fbx_utils
    real_datetime = datetime.datetime
    real_export_uuid = exporter.get_fbx_uuid_from_key
    real_utils_uuid = fbx_utils.get_fbx_uuid_from_key
    stable_ids, used_ids = {}, set()

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


def add_markers(root):
    for name in sorted(MARKERS):
        marker = bpy.data.objects.new(name, None)
        bpy.context.collection.objects.link(marker)
        marker.empty_display_type = "PLAIN_AXES"
        marker.empty_display_size = 0.020
        marker.location = MARKERS[name]
        marker.parent = root


def build_variant(name, mode):
    root = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(root)
    if mode == "duelist":
        wood = material(name + "_Rosewood", (0.24, 0.055, 0.030), 0.0, 0.38)
        steel = material(name + "_BrightSteel", (0.34, 0.38, 0.42), 0.78, 0.18)
        ornament = material(name + "_SilverGuard", (0.62, 0.66, 0.68), 0.72, 0.16)
        objects = [
            cylinder(name + "_OctagonalBarrel", 0.0105, 0.229,
                     (0.0, 0.0, 0.1495), steel, 8),
            cylinder(name + "_FlaredMuzzle", 0.0145, 0.027,
                     (0.0, 0.0, 0.2505), ornament, 16),
            box(name + "_SlenderLock", (0.043, 0.031, 0.071),
                (0.0, -0.002, 0.040), steel),
            box_between(name + "_SweptGrip", (0.0, 0.0, 0.010),
                        (0.032, 0.0, -0.062), 0.036, 0.030, wood),
            cylinder(name + "_RoundPommel", 0.023, 0.020,
                     (0.032, 0.0, -0.066), ornament, 16),
            box(name + "_GuardFront", (0.010, 0.009, 0.061),
                (-0.025, -0.020, -0.003), ornament),
            box_between(name + "_GuardSweep", (-0.025, -0.020, -0.030),
                        (0.020, -0.020, -0.053), 0.009, 0.009, ornament),
            cylinder(name + "_BarrelCollar", 0.014, 0.014,
                     (0.0, 0.0, 0.090), ornament, 16),
        ]
    elif mode == "last-word":
        wood = material(name + "_Ebony", (0.035, 0.028, 0.026), 0.0, 0.30)
        steel = material(name + "_BlackenedSteel", (0.075, 0.085, 0.095), 0.82, 0.16)
        ornament = material(name + "_GoldFurniture", (0.55, 0.31, 0.045), 0.76, 0.14)
        objects = [
            cylinder(name + "_HeavyBarrel", 0.0140, 0.232,
                     (0.0, 0.0, 0.148), steel, 12),
            cylinder(name + "_CrownedMuzzle", 0.0200, 0.030,
                     (0.0, 0.0, 0.249), ornament, 12),
            cylinder(name + "_RearBand", 0.0170, 0.020,
                     (0.0, 0.0, 0.090), ornament, 12),
            box(name + "_AngularReceiver", (0.052, 0.040, 0.080),
                (0.0, -0.002, 0.038), steel),
            box_between(name + "_AngularGrip", (0.0, 0.0, 0.004),
                        (-0.030, 0.0, -0.066), 0.045, 0.036, wood),
            box(name + "_CoffinPommel", (0.055, 0.041, 0.021),
                (-0.030, 0.0, -0.071), ornament),
            box(name + "_GuardPost", (0.011, 0.010, 0.066),
                (0.028, -0.023, -0.004), ornament),
            box_between(name + "_GuardHook", (0.028, -0.023, -0.034),
                        (-0.018, -0.023, -0.052), 0.011, 0.010, ornament),
            box(name + "_SightRidge", (0.009, 0.010, 0.106),
                (0.0, 0.015, 0.173), ornament),
        ]
    else:
        raise RuntimeError("Unknown Pistol variant mode " + mode)
    for obj in objects:
        obj.parent = root
    add_markers(root)
    return root


def select_tree(root):
    root.select_set(True)
    for child in root.children_recursive:
        child.select_set(True)


def export(root, filename):
    target_markers = [child for child in root.children_recursive
                      if child.type == "EMPTY" and
                      child.name.split(".", 1)[0] in MARKERS]
    marker_bases = {child.name.split(".", 1)[0] for child in target_markers}
    if marker_bases != set(MARKERS):
        raise RuntimeError(root.name + " does not own exactly four semantic markers")

    # Blender object names are global, so the second variant's in-source marker
    # names receive numeric suffixes. Normalize the selected tree only while
    # exporting: every standalone FBX must expose the exact KMG_* contract.
    other_markers = [obj for obj in bpy.context.scene.objects
                     if obj not in target_markers and obj.type == "EMPTY" and
                     obj.name.split(".", 1)[0] in MARKERS]
    target_names = [(obj, obj.name) for obj in target_markers]
    other_names = [(obj, obj.name) for obj in other_markers]
    try:
        for index, (obj, original) in enumerate(other_names):
            obj.name = "__KMG_EXPORT_DISABLED_{:02d}_{}".format(index, original)
        for obj, original in target_names:
            obj.name = original.split(".", 1)[0]

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
    finally:
        for obj, original in target_names:
            obj.name = original
        for obj, original in other_names:
            obj.name = original


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


def normalize_png(path):
    source = path.read_bytes()
    output, offset = bytearray(source[:8]), 8
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


def render(root, filename):
    low, high = bounds_for(root)
    center = (low + high) / 2
    extent = max((high - low).length, 0.30)
    camera_data = bpy.data.cameras.new(root.name + "RenderCamera")
    camera = bpy.data.objects.new(root.name + "RenderCamera", camera_data)
    bpy.context.collection.objects.link(camera)
    camera.location = center + Vector((extent * 1.0, -extent * 1.55, extent * 0.25))
    camera.rotation_euler = (center - camera.location).to_track_quat("-Z", "Y").to_euler()
    camera_data.type = "ORTHO"
    camera_data.ortho_scale = extent * 1.65
    light_data = bpy.data.lights.new(root.name + "Key", "AREA")
    light = bpy.data.objects.new(root.name + "Key", light_data)
    bpy.context.collection.objects.link(light)
    light.location = center + Vector((extent, -extent, extent))
    light.rotation_euler = (center - light.location).to_track_quat("-Z", "Y").to_euler()
    light_data.energy = 650
    light_data.size = extent
    scene = bpy.context.scene
    scene.camera = camera
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    scene.render.resolution_x = 720
    scene.render.resolution_y = 480
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


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def describe(root, fbx, image, variant):
    low, high = bounds_for(root)
    meshes = [obj for obj in root.children_recursive if obj.type == "MESH"]
    return {
        "name": root.name,
        "variant": variant,
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
        "markersMeters": {key: list(value) for key, value in MARKERS.items()},
        "provenance": "project-owned clean-room deterministic Blender source",
    }


RENDERS.mkdir(parents=True, exist_ok=True)
bpy.context.preferences.filepaths.save_version = 0
clear_scene()
exporter, fbx_utils, real_datetime, real_export_uuid, real_utils_uuid = \
    install_deterministic_fbx_contract()
records, roots = [], []
try:
    for name, mode, filename, image_name, variant in (
        ("PistolDuelist", "duelist", "pistol-duelist.fbx",
         "pistol-duelist-source.png", "Pistol.Duelist"),
        ("PistolLastWord", "last-word", "pistol-last-word.fbx",
         "pistol-last-word-source.png", "Pistol.LastWord"),
    ):
        for prior in roots:
            prior.hide_render = True
            for child in prior.children_recursive:
                child.hide_render = True
        root = build_variant(name, mode)
        fbx = export(root, filename)
        image = render(root, image_name)
        records.append(describe(root, fbx, image, variant))
        roots.append(root)
        root.location.x = -0.22 if mode == "duelist" else 0.22
    for root in roots:
        root.hide_render = False
        for child in root.children_recursive:
            child.hide_render = False
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND), compress=True)
finally:
    exporter.datetime.datetime = real_datetime
    exporter.get_fbx_uuid_from_key = real_export_uuid
    fbx_utils.get_fbx_uuid_from_key = real_utils_uuid

report = {
    "schemaVersion": 1,
    "generator": "generate_firearm_pistol_variants.py",
    "blenderVersion": bpy.app.version_string,
    "unitSystem": "METRIC",
    "axis": "+Z muzzle; identity firing-hand grip",
    "semanticLengthMeters": round((Vector(MARKERS["KMG_Muzzle"]) -
                                    Vector(MARKERS["KMG_Butt"])).length, 6),
    "variants": records,
    "blend": BLEND.name,
    "blendSha256": sha256(BLEND),
    "determinism": "FBX and normalized PNG must match across clean runs; .blend embeds session metadata and is semantic-only reproducible",
}
REPORT.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
print(json.dumps(report, indent=2))
