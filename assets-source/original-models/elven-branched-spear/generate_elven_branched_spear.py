"""Deterministically generate the project's original Elven Branched Spear.

Run with Blender 4.5 in background mode. The exported weapon is aligned on +Z,
has its primary grip at the origin, and uses metric dimensions.
"""
import bpy
import hashlib
import json
import math
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parent
FBX = ROOT / "elven-branched-spear.fbx"
BLEND = ROOT / "elven-branched-spear.blend"
ICON = ROOT / "elven-branched-spear-icon.png"
REPORT = ROOT / "elven-branched-spear-build-report.json"


def material(name, color, metallic, roughness):
    value = bpy.data.materials.new(name)
    value.diffuse_color = (*color, 1.0)
    value.use_nodes = True
    node = value.node_tree.nodes.get("Principled BSDF")
    node.inputs["Base Color"].default_value = (*color, 1.0)
    node.inputs["Metallic"].default_value = metallic
    node.inputs["Roughness"].default_value = roughness
    return value


def cylinder(name, radius, depth, z, mat, vertices=16):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius,
                                       depth=depth, location=(0, 0, z))
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(mat)
    bpy.ops.object.shade_smooth()
    return obj


def leaf(name, length, width, thickness, mat):
    # Symmetric leaf with a raised central ridge and pointed ends.
    outline = [(0.0, 0.0), (width * 0.72, length * 0.31),
               (width, length * 0.58), (width * 0.58, length * 0.82),
               (0.0, length), (-width * 0.58, length * 0.82),
               (-width, length * 0.58), (-width * 0.72, length * 0.31)]
    verts = [(x, -thickness, z) for x, z in outline]
    verts += [(x, thickness, z) for x, z in outline]
    verts += [(0.0, -thickness * 1.65, length * 0.54),
              (0.0, thickness * 1.65, length * 0.54)]
    faces = []
    for i in range(8):
        j = (i + 1) % 8
        faces.append((i, j, 8 + j, 8 + i))
    # Four triangular fans per face give the blade its restrained ridge.
    for i in range(8):
        j = (i + 1) % 8
        faces.append((i, 16, j))
        faces.append((8 + j, 17, 8 + i))
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


def look_at(obj, point):
    obj.rotation_euler = (Vector(point) - obj.location).to_track_quat(
        "-Z", "Y").to_euler()


bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.materials,
                   bpy.data.cameras, bpy.data.lights):
    for block in list(datablocks):
        if block.users == 0:
            datablocks.remove(block)

wood = material("ElvenAsh", (0.105, 0.055, 0.026), 0.0, 0.38)
steel = material("MoonSilver", (0.42, 0.52, 0.58), 0.88, 0.20)
inlay = material("BlueInlay", (0.025, 0.18, 0.24), 0.62, 0.25)

objects = []
objects.append(cylinder("Shaft", 0.027, 2.35, 0.37, wood, 20))
objects.append(cylinder("ButtCap", 0.036, 0.11, -0.86, steel, 16))
objects.append(cylinder("HeadCollar", 0.043, 0.18, 1.50, steel, 20))
for index, z in enumerate((-0.48, 0.02, 0.52, 0.92, 1.22)):
    objects.append(cylinder("InlayBand%02d" % index, 0.0305, 0.018,
                            z, inlay, 20))

central = leaf("CentralLeaf", 0.54, 0.105, 0.0085, steel)
central.location = (0, 0, 1.47)
objects.append(central)

# Staggered, forward-raked branch blades. Their narrow silhouette remains clear
# at an isometric camera without becoming wider than a native polearm stance.
branches = [
    ("BranchLeftLow", (-0.36, 0.0, 0.50), (-0.025, 0.0, 1.13), 0.32, 0.060),
    ("BranchRightMid", (0.34, 0.0, 0.55), (0.025, 0.0, 1.26), 0.30, 0.057),
    ("BranchLeftHigh", (-0.29, 0.0, 0.67), (-0.020, 0.0, 1.37), 0.25, 0.052),
]
for name, direction, location, length, width in branches:
    blade = leaf(name, length, width, 0.006, steel)
    blade.location = location
    point_at(blade, direction)
    objects.append(blade)
    collar = cylinder(name + "Collar", 0.034, 0.055, location[2], inlay, 16)
    objects.append(collar)

# Apply transforms and generate conservative smart UVs for every mesh.
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

root = bpy.data.objects.new("ElvenBranchedSpear", None)
bpy.context.collection.objects.link(root)
for obj in objects:
    obj.parent = root

# Deterministic icon render with transparent background.
bpy.ops.object.camera_add(location=(3.15, -4.35, 2.45))
camera = bpy.context.object
camera.name = "IconCamera"
camera.data.type = "ORTHO"
camera.data.ortho_scale = 3.45
look_at(camera, (0, 0, 0.45))
bpy.context.scene.camera = camera
bpy.ops.object.light_add(type="AREA", location=(2.5, -2.5, 4.0))
key = bpy.context.object
key.data.energy = 900
key.data.shape = "DISK"
key.data.size = 4.0
look_at(key, (0, 0, 0.5))
bpy.ops.object.light_add(type="AREA", location=(-2.5, 1.0, 2.0))
fill = bpy.context.object
fill.data.energy = 450
fill.data.size = 3.0
look_at(fill, (0, 0, 0.7))
scene = bpy.context.scene
scene.render.engine = "BLENDER_EEVEE_NEXT"
scene.render.resolution_x = 512
scene.render.resolution_y = 512
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.film_transparent = True
scene.render.filepath = str(ICON)
scene.view_settings.look = "AgX - Medium High Contrast"
bpy.ops.render.render(write_still=True)

# Cameras/lights are source-only and must not enter the FBX.
bpy.ops.object.select_all(action="DESELECT")
root.select_set(True)
for obj in objects:
    obj.select_set(True)
bpy.context.view_layer.objects.active = root
bpy.ops.export_scene.fbx(filepath=str(FBX), use_selection=True,
                         apply_unit_scale=True, apply_scale_options="FBX_SCALE_UNITS",
                         object_types={"EMPTY", "MESH"}, add_leaf_bones=False,
                         bake_anim=False, axis_forward="-Z", axis_up="Y")
bpy.ops.wm.save_as_mainfile(filepath=str(BLEND))


def sha256(path):
    value = hashlib.sha256()
    with open(path, "rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            value.update(chunk)
    return value.hexdigest()


report = {
    "schemaVersion": 1,
    "generator": Path(__file__).name,
    "blenderVersion": bpy.app.version_string,
    "license": "Original project-owned asset; repository license applies",
    "sourceCoordinateContract": "+Z tip; grip origin; metric",
    "dimensionsMeters": {"buttZ": -0.915, "tipZ": 2.01,
                         "maximumWidth": 0.26},
    "meshObjects": len(objects),
    "triangles": sum(len(obj.data.loop_triangles) for obj in objects
                     if obj.type == "MESH"),
    "outputs": {}
}
for path in (Path(__file__), FBX, BLEND, ICON):
    report["outputs"][path.name] = {"sha256": sha256(path),
                                    "bytes": path.stat().st_size}
REPORT.write_text(json.dumps(report, indent=2, sort_keys=True) + "\n",
                  encoding="utf-8")
print("KMG_ELVEN_BRANCHED_SPEAR_BUILD " + json.dumps(report, sort_keys=True))
