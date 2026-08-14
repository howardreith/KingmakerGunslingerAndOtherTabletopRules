"""Generate the project's original Wakizashi, Katana, and Nodachi assets.

Run with Blender 4.5 in background mode. Each weapon uses metric scale, places
its primary grip at the origin, and points toward +Z. The six icon renders are
also generated from these exact meshes without third-party source artwork.
"""
import bpy
import hashlib
import json
import math
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[2]
BLEND = ROOT / "eastern-weapons.blend"
REPORT = ROOT / "eastern-weapons-build-report.json"
RUNTIME_ICONS = REPO / "assets" / "game" / "icons"

WEAPONS = {
    "wakizashi": {
        "label": "Wakizashi", "butt": -0.20, "guard": 0.10,
        "tip": 0.56, "blade_width": 0.036, "curve": 0.040,
        "handle_radius": 0.017, "support": 0.07,
    },
    "katana": {
        "label": "Katana", "butt": -0.29, "guard": 0.12,
        "tip": 0.76, "blade_width": 0.042, "curve": 0.058,
        "handle_radius": 0.019, "support": 0.10,
    },
    "nodachi": {
        "label": "Nodachi", "butt": -0.42, "guard": 0.15,
        "tip": 1.16, "blade_width": 0.050, "curve": 0.080,
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


def torus_guard(name, major, minor, z, mat):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor,
                                    major_segments=24, minor_segments=8,
                                    location=(0, 0, z),
                                    rotation=(math.pi / 2.0, 0, 0))
    obj = bpy.context.object
    obj.name = name
    obj.scale.y = 0.66
    obj.data.materials.append(mat)
    return obj


def curved_blade(name, start, tip, width, curve, mat):
    sections = 12
    thickness = width * 0.12
    vertices = []
    for index in range(sections + 1):
        t = index / sections
        z = start + (tip - start) * t
        center = curve * (t * t)
        half = width * (1.0 - 0.82 * max(0.0, (t - 0.72) / 0.28))
        if index == sections:
            half = 0.0005
        # Four points per station form a restrained central ridge.
        vertices.extend([
            (center - half, -thickness, z),
            (center, -thickness * 1.55, z),
            (center + half * 0.72, -thickness, z),
            (center, thickness * 1.55, z),
        ])
    faces = []
    for index in range(sections):
        base = index * 4
        nxt = (index + 1) * 4
        for side in range(4):
            faces.append((base + side, base + (side + 1) % 4,
                          nxt + (side + 1) % 4, nxt + side))
    faces.append((0, 3, 2, 1))
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mat)
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
    objects = []
    handle_mid = (spec["butt"] + spec["guard"]) / 2.0
    objects.append(cylinder(spec["label"] + "Handle",
                            spec["handle_radius"],
                            spec["guard"] - spec["butt"], handle_mid,
                            wrap_mat, 20))
    objects.extend(wrap_bands(spec["label"], spec["butt"], spec["guard"],
                              spec["handle_radius"], accent_mat))
    objects.append(cylinder(spec["label"] + "Pommel",
                            spec["handle_radius"] * 1.35, 0.025,
                            spec["butt"] - 0.006, accent_mat, 20))
    objects.append(torus_guard(spec["label"] + "Guard",
                               spec["blade_width"] * 1.38,
                               spec["handle_radius"] * 0.28,
                               spec["guard"], accent_mat))
    objects.append(curved_blade(spec["label"] + "Blade",
                                spec["guard"] + 0.012, spec["tip"],
                                spec["blade_width"], spec["curve"],
                                steel_mat))
    apply_mesh_transforms(objects)
    root = bpy.data.objects.new(spec["label"], None)
    bpy.context.collection.objects.link(root)
    for obj in objects:
        obj.parent = root
    return {"root": root, "objects": objects,
            "materials": (steel_mat, wrap_mat, accent_mat)}


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
    properties = ((palette[0], 0.88, 0.18),
                  (palette[1], 0.08, 0.42),
                  (palette[2], 0.72, 0.24))
    for mat, (color, metallic, roughness) in zip(built["materials"], properties):
        mat.diffuse_color = (*color, 1.0)
        node = mat.node_tree.nodes.get("Principled BSDF")
        node.inputs["Base Color"].default_value = (*color, 1.0)
        node.inputs["Metallic"].default_value = metallic
        node.inputs["Roughness"].default_value = roughness


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
    camera.data.ortho_scale = length * 1.42
    look_at(camera, target)
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
    return source, runtime


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
exports = [export_weapon(key, value) for key, value in BUILT.items()]

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
scene.render.engine = "BLENDER_EEVEE_NEXT"
scene.render.image_settings.file_format = "PNG"
scene.render.image_settings.color_mode = "RGBA"
scene.render.film_transparent = True
scene.view_settings.look = "AgX - Medium High Contrast"

FAMILY_FOR_CAPSTONE = {
    "night-without-moon": "wakizashi",
    "heavens-measure": "katana",
    "world-tree-severer": "nodachi",
}
icon_outputs = []
for family in WEAPONS:
    icon_outputs.extend(render_icon(BUILT[family], family, family))
for icon_name, family in FAMILY_FOR_CAPSTONE.items():
    icon_outputs.extend(render_icon(BUILT[family], icon_name, icon_name))

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
