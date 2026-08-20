"""Deterministically generate three project-owned Elven Branched Spears.

Run with Blender 4.5 in background mode and PYTHONHASHSEED=0. Every variant is
metric, uses an identity root, places the primary grip at the origin, points its
central blade toward +Z, and has physically separated backward-swept prongs.
"""
import bpy
import datetime
import hashlib
import json
import math
import os
import struct
import zlib
from pathlib import Path
from bpy_extras.object_utils import world_to_camera_view
from mathutils import Quaternion, Vector

ROOT = Path(__file__).resolve().parent
BLEND = ROOT / "elven-branched-spear.blend"
ICON = ROOT / "elven-branched-spear-icon.png"
REPORT = ROOT / "elven-branched-spear-build-report.json"
RUNTIME_ICON = ROOT.parents[2] / "assets" / "game" / "icons" / \
    "elven-branched-spear.png"
ICON_RENDER_ANGLE_DEGREES = 42.0
BUTT_Z = -1.14
TIP_Z = 1.14
SUPPORT_Z = 0.37
HEAD_BASE_Z = 0.70

VARIANTS = {
    "classic": {
        "label": "ElvenBranchedSpear",
        "fbx": "elven-branched-spear.fbx",
        "branches": (
            ("LeftLow", (-0.58, 0.00, -0.36), (-0.024, 0.000, 0.82), 0.27, 0.052),
            ("RightHigh", (0.55, 0.00, -0.30), (0.024, 0.000, 0.91), 0.24, 0.050),
        ),
        "steel": (0.42, 0.52, 0.58),
        "inlay": (0.025, 0.18, 0.24),
    },
    "thorn": {
        "label": "ElvenBranchedSpearThorn",
        "fbx": "elven-branched-spear-thorn.fbx",
        "branches": (
            ("LeftLow", (-0.66, 0.05, -0.38), (-0.024, 0.000, 0.79), 0.28, 0.050),
            ("RightMid", (0.62, -0.04, -0.32), (0.024, 0.000, 0.88), 0.25, 0.048),
            ("LeftHigh", (-0.48, -0.08, -0.22), (-0.018, 0.004, 0.97), 0.19, 0.041),
        ),
        "steel": (0.36, 0.49, 0.43),
        "inlay": (0.11, 0.31, 0.16),
    },
    "crown": {
        "label": "ElvenBranchedSpearCrown",
        "fbx": "elven-branched-spear-crown.fbx",
        "branches": (
            ("LeftLow", (-0.70, 0.05, -0.42), (-0.026, 0.000, 0.77), 0.29, 0.055),
            ("RightLow", (0.70, -0.05, -0.42), (0.026, 0.000, 0.81), 0.29, 0.055),
            ("LeftHigh", (-0.54, -0.10, -0.20), (-0.020, 0.006, 0.96), 0.22, 0.044),
            ("RightHigh", (0.54, 0.10, -0.20), (0.020, -0.006, 1.00), 0.22, 0.044),
        ),
        "steel": (0.55, 0.65, 0.68),
        "inlay": (0.48, 0.31, 0.08),
    },
}

if os.environ.get("PYTHONHASHSEED") != "0":
    raise RuntimeError("Deterministic generation requires PYTHONHASHSEED=0")


def install_deterministic_fbx_clock():
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

    exporter.datetime.datetime = FixedDateTime

    def stable_uuid(key):
        if isinstance(key, int) and 0 <= key < 2 ** 63:
            return fbx_utils.UUID(key)
        canonical = repr(key).encode("utf-8")
        value = int.from_bytes(hashlib.sha256(canonical).digest()[:8], "big") & \
            ((1 << 63) - 1)
        while value in used_ids and stable_ids.get(key) != value:
            value = (value + 1) & ((1 << 63) - 1)
        stable_ids[key] = value
        used_ids.add(value)
        return fbx_utils.UUID(value)

    exporter.get_fbx_uuid_from_key = stable_uuid
    fbx_utils.get_fbx_uuid_from_key = stable_uuid
    return (exporter, fbx_utils, real_datetime, real_export_uuid,
            real_utils_uuid)


def material(name, color, metallic, roughness):
    value = bpy.data.materials.new(name)
    value.diffuse_color = (*color, 1.0)
    value.use_nodes = True
    node = value.node_tree.nodes.get("Principled BSDF")
    node.inputs["Base Color"].default_value = (*color, 1.0)
    node.inputs["Metallic"].default_value = metallic
    node.inputs["Roughness"].default_value = roughness
    return value


def cylinder(name, radius, depth, z, mat, vertices=20):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius,
                                       depth=depth, location=(0, 0, z))
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(mat)
    bpy.ops.object.shade_smooth()
    return obj


def leaf(name, length, width, thickness, mat):
    outline = [(0.0, 0.0), (width * 0.72, length * 0.31),
               (width, length * 0.58), (width * 0.58, length * 0.82),
               (0.0, length), (-width * 0.58, length * 0.82),
               (-width, length * 0.58), (-width * 0.72, length * 0.31)]
    verts = [(x, -thickness, z) for x, z in outline]
    verts += [(x, thickness, z) for x, z in outline]
    verts += [(0.0, -thickness * 1.65, length * 0.54),
              (0.0, thickness * 1.65, length * 0.54)]
    faces = []
    for index in range(8):
        nxt = (index + 1) % 8
        faces.append((index, nxt, 8 + nxt, 8 + index))
        faces.append((index, 16, nxt))
        faces.append((8 + nxt, 17, 8 + index))
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mat)
    bevel = obj.modifiers.new("EdgeSoftening", "BEVEL")
    bevel.width = 0.003
    bevel.segments = 2
    return obj


def point_at(obj, direction):
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(
        Vector(direction).normalized())


def apply_mesh_contract(objects):
    for obj in objects:
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        # These material-only meshes intentionally omit UVs. Blender's UV
        # island packing changes across otherwise identical headless runs; an
        # unused UV channel would therefore make the production FBX unstable.
        obj.select_set(False)


def build_variant(key, spec):
    prefix = spec["label"]
    wood = material(prefix + "Ash", (0.105, 0.055, 0.026), 0.0, 0.38)
    steel = material(prefix + "MoonSilver", spec["steel"], 0.88, 0.20)
    inlay = material(prefix + "Inlay", spec["inlay"], 0.62, 0.25)
    objects = [
        cylinder(prefix + "Shaft", 0.027, 1.88, -0.16, wood),
        cylinder(prefix + "ButtCap", 0.036, 0.08, -1.10, steel, 16),
        cylinder(prefix + "HeadCollar", 0.043, 0.14, 0.72, steel),
    ]
    for index, z in enumerate((-0.72, -0.35, 0.00, 0.30, 0.55)):
        objects.append(cylinder(prefix + "InlayBand%02d" % index,
                                0.0305, 0.018, z, inlay))
    central = leaf(prefix + "CentralLeaf", 0.44, 0.095, 0.0085, steel)
    central.location = (0, 0, HEAD_BASE_Z)
    objects.append(central)

    branch_records = []
    for name, direction, location, length, width in spec["branches"]:
        branch = leaf(prefix + "Branch" + name, length, width, 0.0075, steel)
        branch.location = location
        point_at(branch, direction)
        objects.append(branch)
        collar = cylinder(prefix + "Branch" + name + "Collar", 0.036,
                          0.060, location[2], inlay, 16)
        objects.append(collar)
        tip = Vector(location) + Vector(direction).normalized() * length
        branch_records.append({
            "name": name,
            "base": [round(value, 6) for value in location],
            "tip": [round(value, 6) for value in tip],
            "length": length,
            "width": width,
        })
    apply_mesh_contract(objects)
    root = bpy.data.objects.new(spec["label"], None)
    bpy.context.collection.objects.link(root)
    for obj in objects:
        obj.parent = root
    return {"key": key, "root": root, "objects": objects,
            "branches": branch_records}


def select_tree(root):
    root.select_set(True)
    for child in root.children_recursive:
        child.select_set(True)


def export_variant(built):
    bpy.ops.object.select_all(action="DESELECT")
    select_tree(built["root"])
    bpy.context.view_layer.objects.active = built["root"]
    path = ROOT / VARIANTS[built["key"]]["fbx"]
    bpy.ops.export_scene.fbx(filepath=str(path), use_selection=True,
                             apply_unit_scale=True,
                             apply_scale_options="FBX_SCALE_UNITS",
                             object_types={"EMPTY", "MESH"},
                             add_leaf_bones=False, bake_anim=False,
                             axis_forward="-Z", axis_up="Y")
    return path


def look_at(obj, point):
    obj.rotation_euler = (Vector(point) - obj.location).to_track_quat(
        "-Z", "Y").to_euler()


def projected_angle(camera, butt, tip):
    scene = bpy.context.scene
    bpy.context.view_layer.update()
    butt_view = world_to_camera_view(scene, camera, Vector((0, 0, butt)))
    tip_view = world_to_camera_view(scene, camera, Vector((0, 0, tip)))
    return math.degrees(math.atan2(tip_view.y - butt_view.y,
                                   tip_view.x - butt_view.x))


def apply_icon_roll(camera, butt, tip):
    base = camera.rotation_euler.to_quaternion()
    initial = projected_angle(camera, butt, tip)
    view_axis = base @ Vector((0, 0, -1))
    previous_roll, previous_angle = 0.0, initial
    roll, observed = ICON_RENDER_ANGLE_DEGREES - initial, initial
    for _ in range(8):
        camera.rotation_euler = (Quaternion(view_axis, math.radians(roll)) @
                                 base).to_euler()
        observed = projected_angle(camera, butt, tip)
        if abs(observed - ICON_RENDER_ANGLE_DEGREES) <= 0.05:
            break
        slope = (observed - previous_angle) / (roll - previous_roll)
        if abs(slope) < 0.0001:
            raise RuntimeError("Spear icon camera roll solver has zero slope")
        previous_roll, previous_angle = roll, observed
        roll += (ICON_RENDER_ANGLE_DEGREES - observed) / slope
    if abs(observed - ICON_RENDER_ANGLE_DEGREES) > 0.05:
        raise RuntimeError("Spear icon camera roll contract failed")
    return observed, roll


def sha256(path):
    value = hashlib.sha256()
    with open(path, "rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            value.update(chunk)
    return value.hexdigest()


def normalize_png(path):
    """Strip Blender session metadata while preserving exact rendered pixels."""
    source = path.read_bytes()
    if source[:8] != b"\x89PNG\r\n\x1a\n":
        raise RuntimeError("Expected PNG output: " + str(path))
    output = bytearray(source[:8])
    offset = 8
    while offset < len(source):
        length = struct.unpack(">I", source[offset:offset + 4])[0]
        chunk_type = source[offset + 4:offset + 8]
        payload = source[offset + 8:offset + 8 + length]
        offset += length + 12
        if chunk_type in {b"tEXt", b"eXIf", b"oFFs", b"pHYs"}:
            continue
        output.extend(struct.pack(">I", length))
        output.extend(chunk_type)
        output.extend(payload)
        output.extend(struct.pack(">I", zlib.crc32(chunk_type + payload) &
                                  0xFFFFFFFF))
    path.write_bytes(output)


bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.materials,
                   bpy.data.cameras, bpy.data.lights):
    for block in list(datablocks):
        if block.users == 0:
            datablocks.remove(block)

BUILT = {key: build_variant(key, spec) for key, spec in VARIANTS.items()}
exporter, fbx_utils, real_datetime, real_export_uuid, real_utils_uuid = \
    install_deterministic_fbx_clock()
exports = [export_variant(value) for value in BUILT.values()]
exporter.datetime.datetime = real_datetime
exporter.get_fbx_uuid_from_key = real_export_uuid
fbx_utils.get_fbx_uuid_from_key = real_utils_uuid

# Render the classic profile only. Cameras and lights are created after FBX
# export and therefore can never enter a production FBX.
for key, value in BUILT.items():
    value["root"].hide_render = key != "classic"
bpy.ops.object.camera_add(location=(3.15, -4.35, 2.45))
camera = bpy.context.object
camera.name = "IconCamera"
camera.data.type = "ORTHO"
camera.data.ortho_scale = 2.75
look_at(camera, (0, 0, 0.0))
observed_icon_angle, icon_camera_roll = apply_icon_roll(camera, BUTT_Z, TIP_Z)
bpy.context.scene.camera = camera
bpy.ops.object.light_add(type="AREA", location=(2.5, -2.5, 4.0))
key_light = bpy.context.object
key_light.name = "IconKey"
key_light.data.energy = 900
key_light.data.size = 4.0
look_at(key_light, (0, 0, 0.5))
bpy.ops.object.light_add(type="AREA", location=(-2.5, 1.0, 2.0))
fill_light = bpy.context.object
fill_light.name = "IconFill"
fill_light.data.energy = 450
fill_light.data.size = 3.0
look_at(fill_light, (0, 0, 0.7))
scene = bpy.context.scene
scene.render.engine = "BLENDER_WORKBENCH"
scene.render.resolution_x = scene.render.resolution_y = 512
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.film_transparent = True
scene.render.filepath = str(ICON)
scene.display.render_aa = "OFF"
scene.display.shading.light = "STUDIO"
scene.display.shading.color_type = "MATERIAL"
scene.display.shading.show_shadows = False
scene.display.shading.show_cavity = False
scene.display.shading.show_specular_highlight = False
bpy.ops.render.render(write_still=True)
normalize_png(ICON)
scene.render.resolution_x = scene.render.resolution_y = 128
scene.render.filepath = str(RUNTIME_ICON)
bpy.ops.render.render(write_still=True)
normalize_png(RUNTIME_ICON)
for value in BUILT.values():
    value["root"].hide_render = False
bpy.ops.wm.save_as_mainfile(filepath=str(BLEND))
backup = Path(str(BLEND) + "1")
if backup.exists():
    backup.unlink()

mesh_objects = [obj for value in BUILT.values() for obj in value["objects"]]
for obj in mesh_objects:
    obj.data.calc_loop_triangles()
report = {
    "schemaVersion": 2,
    "generator": Path(__file__).name,
    "blenderVersion": bpy.app.version_string,
    "license": "Original project-owned asset; repository license applies",
    "sourceCoordinateContract": "+Z central tip; primary grip origin; metric",
    "equippedExportContract": "three identity roots exported before render-only camera/light creation",
    "branchContract": "physical backward-swept prongs with separated lateral tips outside the shaft grip region",
    "determinism": {
        "verifiedCleanRuns": 2,
        "byteStableOutputs": [
            "elven-branched-spear.fbx",
            "elven-branched-spear-thorn.fbx",
            "elven-branched-spear-crown.fbx",
            "elven-branched-spear-icon.png",
            "assets/game/icons/elven-branched-spear.png",
        ],
        "blendContainer": "Semantically regenerated; Blender session metadata prevents byte-identical .blend containers.",
        "fbxStabilization": "SHA-256-derived exporter UUIDs; unused nondeterministic UV packing omitted.",
        "pngStabilization": "Rendered pixels preserved; Blender session metadata chunks removed.",
    },
    "iconRender": {
        "tipDirection": "upper-right", "buttDirection": "lower-left",
        "targetAngleDegrees": ICON_RENDER_ANGLE_DEGREES,
        "observedAngleDegrees": round(observed_icon_angle, 6),
        "cameraRollDegrees": round(icon_camera_roll, 6),
        "sourceDimensions": [512, 512], "runtimeDimensions": [128, 128],
        "background": "transparent RGBA",
    },
    "dimensionsMeters": {"buttZ": BUTT_Z, "tipZ": TIP_Z,
                         "supportZ": SUPPORT_Z,
                         "shaftGripExclusionMaxZ": HEAD_BASE_Z},
    "variants": {
        key: {"prefab": value["root"].name,
              "fbx": VARIANTS[key]["fbx"],
              "branchCount": len(value["branches"]),
              "branches": value["branches"]}
        for key, value in BUILT.items()
    },
    "meshObjects": len(mesh_objects),
    "triangles": sum(len(obj.data.loop_triangles) for obj in mesh_objects),
    "outputs": {},
}
for path in [Path(__file__), BLEND, ICON, RUNTIME_ICON] + exports:
    report["outputs"][path.name] = {
        "sha256": sha256(path), "bytes": path.stat().st_size}
REPORT.write_text(json.dumps(report, indent=2, sort_keys=True) + "\n",
                  encoding="utf-8")
print("KMG_ELVEN_BRANCHED_SPEAR_BUILD " + json.dumps(report, sort_keys=True))
