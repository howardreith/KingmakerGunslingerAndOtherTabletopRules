"""Generate the project's original Wakizashi, Katana, and Nodachi assets.

Run with Blender 4.5 in background mode. Each weapon uses metric scale, places
its primary grip at the origin, and points toward +Z. The six icon renders are
also generated from these exact meshes without third-party source artwork.
"""
import bpy
import datetime
import hashlib
import json
import math
import os
from pathlib import Path
from bpy_extras.object_utils import world_to_camera_view
from mathutils import Quaternion, Vector

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[2]
BLEND = ROOT / "eastern-weapons.blend"
REPORT = ROOT / "eastern-weapons-build-report.json"
RUNTIME_ICONS = REPO / "assets" / "game" / "icons"
ICON_RENDER_ANGLE_DEGREES = 42.0

if os.environ.get("PYTHONHASHSEED") != "0":
    raise RuntimeError(
        "Deterministic FBX generation requires PYTHONHASHSEED=0 before Blender starts")


def install_deterministic_fbx_clock():
    """Freeze Blender's otherwise current-time FBX header during export."""
    import io_scene_fbx.export_fbx_bin as exporter
    real_datetime = datetime.datetime

    class FixedDateTime(real_datetime):
        @classmethod
        def now(cls, tz=None):
            value = cls(1970, 1, 1, 10, 0, 0)
            return value if tz is None else tz.fromutc(value.replace(tzinfo=tz))

    exporter.datetime.datetime = FixedDateTime
    return exporter, real_datetime

WEAPONS = {
    "wakizashi": {
        "label": "Wakizashi", "butt": -0.20, "guard": 0.10,
        "tip": 0.56, "blade_width": 0.026, "curve": 0.055,
        "handle_radius": 0.017, "support": 0.07,
    },
    "katana": {
        "label": "Katana", "butt": -0.29, "guard": 0.12,
        "tip": 0.76, "blade_width": 0.030, "curve": 0.085,
        "handle_radius": 0.019, "support": 0.10,
    },
    "nodachi": {
        "label": "Nodachi", "butt": -0.42, "guard": 0.15,
        "tip": 1.16, "blade_width": 0.036, "curve": 0.140,
        "handle_radius": 0.022, "support": 0.13,
    },
}

PALETTES = {
    "wakizashi": ((0.42, 0.48, 0.54), (0.055, 0.075, 0.12),
                   (0.32, 0.24, 0.10)),
    "katana": ((0.46, 0.50, 0.54), (0.17, 0.035, 0.035),
               (0.40, 0.26, 0.08)),
    "nodachi": ((0.40, 0.47, 0.50), (0.035, 0.12, 0.09),
                (0.29, 0.20, 0.07)),
    "night-without-moon": ((0.15, 0.18, 0.25), (0.025, 0.018, 0.055),
                           (0.31, 0.17, 0.48)),
    "heavens-measure": ((0.60, 0.72, 0.76), (0.055, 0.18, 0.24),
                        (0.72, 0.54, 0.14)),
    "world-tree-severer": ((0.48, 0.58, 0.46), (0.07, 0.18, 0.08),
                           (0.55, 0.38, 0.10)),
}


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


def disc_guard(name, radius, depth, z, mat):
    bpy.ops.mesh.primitive_cylinder_add(vertices=32, radius=radius, depth=depth,
                                       location=(0, 0, z))
    obj = bpy.context.object
    obj.name = name
    obj.scale.y = 0.72
    obj.data.materials.append(mat)
    return obj


def curved_blade(name, start, tip, width, curve, steel_mat, edge_mat):
    """Build an asymmetric, single-edged blade with a restrained kissaki.

    +X is the spine/concave side. -X is the sharpened convex side used as the
    attack-leading edge. The wedge cross-section and separately shaded bevel
    make that contract mechanically inspectable instead of relying on texture.
    """
    sections = 18
    thickness = width * 0.10
    vertices = []
    for index in range(sections + 1):
        t = index / sections
        z = start + (tip - start) * t
        center = curve * (t * t)
        kissaki = 1.0 if t <= 0.82 else max(0.0, (1.0 - t) / 0.18)
        spine = center + width * 0.31 * kissaki
        bevel = center - width * 0.42 * kissaki
        edge = center - width * 0.69 * kissaki
        station_thickness = max(0.00012, thickness * (0.55 + 0.45 * kissaki))
        # Five points form a blunt spine and one thin, unmistakable edge.
        vertices.extend([
            (spine, -station_thickness, z),
            (bevel, -station_thickness * 1.12, z),
            (edge, 0.0, z),
            (bevel, station_thickness * 1.12, z),
            (spine, station_thickness, z),
        ])
    faces = []
    for index in range(sections):
        base = index * 5
        nxt = (index + 1) * 5
        for side in range(5):
            faces.append((base + side, base + (side + 1) % 5,
                          nxt + (side + 1) % 5, nxt + side))
    faces.append((0, 4, 3, 2, 1))
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(steel_mat)
    obj.data.materials.append(edge_mat)
    for polygon_index, polygon in enumerate(obj.data.polygons[:-1]):
        if polygon_index % 5 in (1, 2):
            polygon.material_index = 1
    bevel = obj.modifiers.new("EdgeSoftening", "BEVEL")
    bevel.width = 0.0015
    bevel.segments = 2
    return obj


def wrap_bands(prefix, butt, guard, radius, mat):
    values = []
    count = 7 if guard - butt < 0.36 else 10
    for index in range(count):
        z = butt + (index + 0.7) * (guard - butt) / count
        values.append(cylinder(prefix + "Wrap%02d" % index,
                               radius * 1.12, 0.008, z, mat, 16))
    return values


def apply_mesh_transforms(objects):
    for obj in objects:
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        if obj.type == "MESH":
            bpy.ops.object.mode_set(mode="EDIT")
            bpy.ops.mesh.select_all(action="SELECT")
            bpy.ops.uv.smart_project(angle_limit=math.radians(66),
                                     island_margin=0.025)
            bpy.ops.object.mode_set(mode="OBJECT")
        obj.select_set(False)


def build_weapon(key, spec):
    steel, wrap, accent = PALETTES[key]
    steel_mat = material(spec["label"] + "Steel", steel, 0.88, 0.18)
    wrap_mat = material(spec["label"] + "Wrap", wrap, 0.08, 0.42)
    accent_mat = material(spec["label"] + "Accent", accent, 0.72, 0.24)
    edge_color = tuple(min(1.0, value * 1.24 + 0.08) for value in steel)
    edge_mat = material(spec["label"] + "CuttingEdge", edge_color, 0.92, 0.12)
    objects = []
    handle_mid = (spec["butt"] + spec["guard"]) / 2.0
    handle = cylinder(spec["label"] + "Handle", spec["handle_radius"],
                      spec["guard"] - spec["butt"], handle_mid, wrap_mat, 20)
    handle.scale.y = 0.72
    objects.append(handle)
    bands = wrap_bands(spec["label"], spec["butt"], spec["guard"],
                       spec["handle_radius"], accent_mat)
    for band in bands:
        band.scale.y = 0.76
    objects.extend(bands)
    pommel = cylinder(spec["label"] + "Pommel",
                      spec["handle_radius"] * 1.35, 0.025,
                      spec["butt"] - 0.006, accent_mat, 20)
    pommel.scale.y = 0.72
    objects.append(pommel)
    objects.append(disc_guard(spec["label"] + "Guard",
                              spec["blade_width"] * 1.48, 0.012,
                              spec["guard"], accent_mat))
    objects.append(curved_blade(spec["label"] + "Blade",
                                spec["guard"] + 0.012, spec["tip"],
                                spec["blade_width"], spec["curve"],
                                steel_mat, edge_mat))
    apply_mesh_transforms(objects)
    root = bpy.data.objects.new(spec["label"], None)
    bpy.context.collection.objects.link(root)
    for obj in objects:
        obj.parent = root
    return {"root": root, "objects": objects,
            "materials": (steel_mat, wrap_mat, accent_mat, edge_mat)}


def select_tree(root, selected=True):
    root.select_set(selected)
    for child in root.children_recursive:
        child.select_set(selected)


def export_weapon(key, built):
    bpy.ops.object.select_all(action="DESELECT")
    select_tree(built["root"])
    bpy.context.view_layer.objects.active = built["root"]
    path = ROOT / (key + ".fbx")
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


def configure_palette(built, palette):
    edge = tuple(min(1.0, value * 1.24 + 0.08) for value in palette[0])
    properties = ((palette[0], 0.88, 0.18),
                  (palette[1], 0.08, 0.42),
                  (palette[2], 0.72, 0.24),
                  (edge, 0.92, 0.12))
    for mat, (color, metallic, roughness) in zip(built["materials"], properties):
        mat.diffuse_color = (*color, 1.0)
        node = mat.node_tree.nodes.get("Principled BSDF")
        node.inputs["Base Color"].default_value = (*color, 1.0)
        node.inputs["Metallic"].default_value = metallic
        node.inputs["Roughness"].default_value = roughness


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
    previous_roll = 0.0
    previous_angle = initial
    roll = ICON_RENDER_ANGLE_DEGREES - initial
    observed = initial
    for _ in range(8):
        camera.rotation_euler = (Quaternion(view_axis, math.radians(roll)) @
                                 base).to_euler()
        observed = projected_angle(camera, butt, tip)
        if abs(observed - ICON_RENDER_ANGLE_DEGREES) <= 0.05:
            break
        denominator = roll - previous_roll
        slope = (observed - previous_angle) / denominator
        if abs(slope) < 0.0001:
            raise RuntimeError("Icon camera roll solver has zero slope")
        previous_roll, previous_angle = roll, observed
        roll += (ICON_RENDER_ANGLE_DEGREES - observed) / slope
    if abs(observed - ICON_RENDER_ANGLE_DEGREES) > 0.05:
        raise RuntimeError("Icon camera roll did not reach the diagonal contract: " +
                           repr((initial, observed, roll)))
    return observed, roll


def render_icon(built, palette_key, filename):
    for value in BUILT.values():
        value["root"].hide_render = value is not built
    configure_palette(built, PALETTES[palette_key])
    target = (0.025, 0, (WEAPONS[filename if filename in WEAPONS else
                                FAMILY_FOR_CAPSTONE[filename]]["tip"] +
                            WEAPONS[filename if filename in WEAPONS else
                                    FAMILY_FOR_CAPSTONE[filename]]["butt"]) / 2)
    camera = bpy.data.objects.get("IconCamera")
    spec = WEAPONS[filename if filename in WEAPONS else
                   FAMILY_FOR_CAPSTONE[filename]]
    length = spec["tip"] - spec["butt"]
    camera.data.ortho_scale = length * 1.35
    look_at(camera, target)
    observed_angle, camera_roll = apply_icon_roll(camera, spec["butt"],
                                                   spec["tip"])
    scene = bpy.context.scene
    source = ROOT / (filename + "-icon-source.png")
    runtime = RUNTIME_ICONS / (filename + ".png")
    scene.render.resolution_x = scene.render.resolution_y = 512
    scene.render.resolution_percentage = 100
    scene.render.filepath = str(source)
    bpy.ops.render.render(write_still=True)
    scene.render.resolution_x = scene.render.resolution_y = 128
    scene.render.filepath = str(runtime)
    bpy.ops.render.render(write_still=True)
    return source, runtime, {
        "tipDirection": "upper-right", "buttDirection": "lower-left",
        "targetAngleDegrees": ICON_RENDER_ANGLE_DEGREES,
        "observedAngleDegrees": round(observed_angle, 6),
        "cameraRollDegrees": round(camera_roll, 6),
        "sourceDimensions": [512, 512], "runtimeDimensions": [128, 128],
        "background": "transparent RGBA",
    }


def sha256(path):
    value = hashlib.sha256()
    with open(path, "rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            value.update(chunk)
    return value.hexdigest()


bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.materials,
                   bpy.data.cameras, bpy.data.lights):
    for block in list(datablocks):
        if block.users == 0:
            datablocks.remove(block)

RUNTIME_ICONS.mkdir(parents=True, exist_ok=True)
BUILT = {key: build_weapon(key, value) for key, value in WEAPONS.items()}
fbx_exporter, real_datetime = install_deterministic_fbx_clock()
exports = [export_weapon(key, value) for key, value in BUILT.items()]
fbx_exporter.datetime.datetime = real_datetime

bpy.ops.object.camera_add(location=(2.35, -3.6, 1.85))
camera = bpy.context.object
camera.name = "IconCamera"
camera.data.type = "ORTHO"
bpy.context.scene.camera = camera
bpy.ops.object.light_add(type="AREA", location=(2.4, -2.5, 3.2))
key_light = bpy.context.object
key_light.name = "IconKey"
key_light.data.energy = 850
key_light.data.size = 3.5
look_at(key_light, (0, 0, 0.3))
bpy.ops.object.light_add(type="AREA", location=(-2.0, 0.8, 1.7))
fill_light = bpy.context.object
fill_light.name = "IconFill"
fill_light.data.energy = 430
fill_light.data.size = 2.8
look_at(fill_light, (0, 0, 0.3))
scene = bpy.context.scene
scene.render.engine = "BLENDER_WORKBENCH"
scene.render.image_settings.file_format = "PNG"
scene.render.image_settings.color_mode = "RGBA"
scene.render.film_transparent = True
scene.display.shading.light = "STUDIO"
scene.display.shading.color_type = "MATERIAL"
scene.display.shading.show_shadows = False
scene.display.shading.show_cavity = False
scene.display.shading.show_specular_highlight = False

FAMILY_FOR_CAPSTONE = {
    "night-without-moon": "wakizashi",
    "heavens-measure": "katana",
    "world-tree-severer": "nodachi",
}
icon_outputs = []
icon_contracts = {}
for family in WEAPONS:
    source, runtime, contract = render_icon(BUILT[family], family, family)
    icon_outputs.extend((source, runtime))
    icon_contracts[family] = contract
for icon_name, family in FAMILY_FOR_CAPSTONE.items():
    source, runtime, contract = render_icon(BUILT[family], icon_name, icon_name)
    icon_outputs.extend((source, runtime))
    icon_contracts[icon_name] = contract

for value in BUILT.values():
    value["root"].hide_render = False
bpy.ops.wm.save_as_mainfile(filepath=str(BLEND))
blend_backup = Path(str(BLEND) + "1")
if blend_backup.exists():
    blend_backup.unlink()

mesh_objects = [obj for value in BUILT.values() for obj in value["objects"]]
for obj in mesh_objects:
    if obj.type == "MESH":
        obj.data.calc_loop_triangles()
report = {
    "schemaVersion": 1,
    "generator": Path(__file__).name,
    "blenderVersion": bpy.app.version_string,
    "license": "Original project-owned assets; repository license applies",
    "sourceCoordinateContract": "+Z tip; grip origin; metric",
    "equippedExportContract": "exported before render-only camera creation; identity roots",
    "bladeContract": "curved asymmetric single edge at local -X; blunt spine at local +X",
    "iconRender": icon_contracts,
    "weapons": {
        key: {"buttZ": spec["butt"], "tipZ": spec["tip"],
              "overallLengthMeters": spec["tip"] - spec["butt"],
              "supportHandZ": spec["support"]}
        for key, spec in WEAPONS.items()
    },
    "meshObjects": len(mesh_objects),
    "triangles": sum(len(obj.data.loop_triangles) for obj in mesh_objects
                     if obj.type == "MESH"),
    "outputs": {},
}
for path in [Path(__file__), BLEND] + exports + icon_outputs:
    report["outputs"][path.name] = {
        "sha256": sha256(path), "bytes": path.stat().st_size,
    }
REPORT.write_text(json.dumps(report, indent=2, sort_keys=True) + "\n",
                  encoding="utf-8")
print("KMG_EASTERN_WEAPONS_BUILD " + json.dumps(report, sort_keys=True))
