"""Deterministically generate four variants for each Eastern blade family.

Every exported blade carries a mesh-grounded semantic frame: physical tip and
pommel ends, the longitudinal blade axis, blade-plane normal, cutting-edge
polarity, grip, optional support hand, and renderer center. The Unity builder
uses that complete frame rather than treating an identity FBX import as a
native Kingmaker hand frame.
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
REPO = ROOT.parents[2]
BLEND = ROOT / "eastern-weapons.blend"
REPORT = ROOT / "eastern-weapons-build-report.json"
RUNTIME_ICONS = REPO / "assets" / "game" / "icons"
ICON_RENDER_ANGLE_DEGREES = 42.0
SEMANTIC_AXIS_DISTANCE = 0.10

if os.environ.get("PYTHONHASHSEED") != "0":
    raise RuntimeError("Deterministic generation requires PYTHONHASHSEED=0")

FAMILIES = {
    "wakizashi": {
        "label": "Wakizashi", "butt": -0.20, "guard": 0.10,
        "tip": 0.56, "blade_width": 0.026, "curve": 0.055,
        "handle_radius": 0.017, "support": None,
    },
    "katana": {
        "label": "Katana", "butt": -0.29, "guard": 0.12,
        "tip": 0.76, "blade_width": 0.030, "curve": 0.085,
        "handle_radius": 0.019, "support": None,
    },
    "nodachi": {
        "label": "Nodachi", "butt": -0.42, "guard": 0.15,
        "tip": 1.16, "blade_width": 0.036, "curve": 0.140,
        "handle_radius": 0.022, "support": -0.169,
    },
}

VARIANTS = {
    "wakizashi": (
        ("classic", "Wakizashi", "disc", 1.00, 1.00, "round"),
        ("petal", "WakizashiPetal", "petal", 1.10, 0.90, "cap"),
        ("moon", "WakizashiMoon", "wing", 0.92, 1.28, "spike"),
        ("capstone", "WakizashiCapstone", "crown", 1.06, 1.42, "crown"),
    ),
    "katana": (
        ("classic", "Katana", "disc", 1.00, 1.00, "round"),
        ("reed", "KatanaReed", "bar", 0.92, 0.82, "cap"),
        ("regal", "KatanaRegal", "wing", 1.10, 1.16, "spike"),
        ("capstone", "KatanaCapstone", "crown", 1.04, 1.34, "crown"),
    ),
    "nodachi": (
        ("classic", "Nodachi", "disc", 1.00, 1.00, "round"),
        ("cleaver", "NodachiCleaver", "bar", 1.22, 0.74, "cap"),
        ("titan", "NodachiTitan", "wing", 1.13, 1.08, "spike"),
        ("capstone", "NodachiCapstone", "crown", 1.18, 1.24, "crown"),
    ),
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

CAPSTONE_ICONS = {
    "night-without-moon": "wakizashi",
    "heavens-measure": "katana",
    "world-tree-severer": "nodachi",
}


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


def box(name, dimensions, location, mat, rotation=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=location,
                                   rotation=(0, 0, rotation))
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = dimensions
    obj.data.materials.append(mat)
    return obj


def guard_objects(prefix, mode, radius, z, mat):
    values = []
    if mode in ("disc", "petal", "crown"):
        sides = 32 if mode == "disc" else 8
        disc = cylinder(prefix + "GuardCore", radius, 0.012, z, mat, sides)
        disc.scale.y = 0.72 if mode == "disc" else 0.58
        values.append(disc)
    if mode == "petal":
        for index, angle in enumerate((0.0, math.pi / 2)):
            values.append(box(prefix + "GuardPetal" + str(index),
                (radius * 2.45, radius * 0.34, 0.010), (0, 0, z), mat,
                angle + math.pi / 4))
    elif mode == "bar":
        values.append(box(prefix + "GuardBar",
            (radius * 3.25, radius * 0.36, 0.013), (0, 0, z), mat))
        for side in (-1, 1):
            values.append(cylinder(prefix + "GuardCap" + str(side),
                radius * 0.24, 0.016, z, mat, 12))
            values[-1].location.x = side * radius * 1.62
    elif mode in ("wing", "crown"):
        spread = 3.05 if mode == "wing" else 3.55
        sweep = math.radians(13 if mode == "wing" else 24)
        for side in (-1, 1):
            wing = box(prefix + "GuardWing" + str(side),
                (radius * spread / 2, radius * 0.34, 0.014),
                (side * radius * spread / 4, 0, z), mat, side * sweep)
            values.append(wing)
            if mode == "crown":
                jewel = cylinder(prefix + "GuardJewel" + str(side),
                    radius * 0.25, 0.019, z, mat, 12)
                jewel.location.x = side * radius * spread * 0.52
                values.append(jewel)
    return values


def curved_blade(name, start, tip, width, curve, steel_mat, edge_mat):
    sections = 20
    thickness = width * 0.10
    vertices = []
    for index in range(sections + 1):
        t = index / sections
        z = start + (tip - start) * t
        center = curve * (t * t)
        kissaki = 1.0 if t <= 0.80 else max(0.0, (1.0 - t) / 0.20)
        spine = center + width * 0.34 * kissaki
        bevel = center - width * 0.40 * kissaki
        edge = center - width * 0.72 * kissaki
        station = max(0.00012, thickness * (0.55 + 0.45 * kissaki))
        vertices.extend([(spine, -station, z),
                         (bevel, -station * 1.12, z), (edge, 0.0, z),
                         (bevel, station * 1.12, z), (spine, station, z)])
    faces = []
    for index in range(sections):
        base, nxt = index * 5, (index + 1) * 5
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
    bevel_modifier = obj.modifiers.new("EdgeSoftening", "BEVEL")
    bevel_modifier.width = 0.0015
    bevel_modifier.segments = 2
    return obj


def wrap_bands(prefix, butt, guard, radius, mat, dense=False):
    values = []
    count = (9 if guard - butt < 0.36 else 12) if dense else \
        (7 if guard - butt < 0.36 else 10)
    for index in range(count):
        z = butt + (index + 0.7) * (guard - butt) / count
        band = cylinder(prefix + "Wrap%02d" % index, radius * 1.12,
                        0.008, z, mat, 16)
        band.scale.y = 0.76
        values.append(band)
    return values


def pommel_objects(prefix, mode, radius, butt, mat):
    if mode == "round":
        value = cylinder(prefix + "Pommel", radius * 1.35, 0.025,
                         butt - 0.006, mat, 20)
        value.scale.y = 0.72
        return [value]
    if mode == "cap":
        value = cylinder(prefix + "PommelCap", radius * 1.55, 0.018,
                         butt - 0.003, mat, 12)
        value.scale.y = 0.62
        return [value]
    length = 0.045 if mode == "spike" else 0.060
    values = [cylinder(prefix + "PommelNeck", radius * 0.72, length,
                       butt - length / 2, mat, 12)]
    if mode == "crown":
        crown = cylinder(prefix + "PommelCrown", radius * 1.55, 0.018,
                         butt - length, mat, 8)
        crown.scale.y = 0.62
        values.append(crown)
    return values


def apply_mesh_contract(objects):
    for obj in objects:
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        # Opaque material-only meshes do not consume UVs. Omitting an unused UV
        # layer avoids Blender's nondeterministic island-packing operator.
        obj.select_set(False)


def evaluated_points(objects):
    bpy.context.view_layer.update()
    depsgraph = bpy.context.evaluated_depsgraph_get()
    points = []
    for obj in objects:
        evaluated = obj.evaluated_get(depsgraph)
        mesh = evaluated.to_mesh()
        try:
            points.extend(evaluated.matrix_world @ vertex.co
                          for vertex in mesh.vertices)
        finally:
            evaluated.to_mesh_clear()
    if not points:
        raise RuntimeError("Eastern blade mesh contract has no evaluated vertices")
    return points


def bounds_for_points(points):
    minimum = Vector((math.inf, math.inf, math.inf))
    maximum = Vector((-math.inf, -math.inf, -math.inf))
    for point in points:
        minimum.x = min(minimum.x, point.x)
        minimum.y = min(minimum.y, point.y)
        minimum.z = min(minimum.z, point.z)
        maximum.x = max(maximum.x, point.x)
        maximum.y = max(maximum.y, point.y)
        maximum.z = max(maximum.z, point.z)
    return minimum, maximum


def evaluated_bounds(objects):
    points = evaluated_points(objects)
    minimum, maximum = bounds_for_points(points)
    return minimum, maximum, points


def extreme_center(points, forward):
    extreme = (max(point.z for point in points) if forward else
               min(point.z for point in points))
    selected = [point for point in points
                if abs(point.z - extreme) <= 0.00001]
    if not selected:
        raise RuntimeError("Eastern blade endpoint selection is empty")
    return sum(selected, Vector()) / len(selected)


def rounded_vector(value):
    return [round(component, 6) for component in value]


def cutting_edge_measurement(blade):
    bpy.context.view_layer.update()
    depsgraph = bpy.context.evaluated_depsgraph_get()
    evaluated = blade.evaluated_get(depsgraph)
    mesh = evaluated.to_mesh()
    try:
        edge_indices = {index for index, material in
                        enumerate(blade.data.materials)
                        if material is not None and
                        material.name.endswith("CuttingEdge")}
        if len(edge_indices) != 1:
            raise RuntimeError(blade.name +
                               " does not have one CuttingEdge material")
        edge_vertices = set()
        for polygon in mesh.polygons:
            if polygon.material_index in edge_indices:
                edge_vertices.update(polygon.vertices)
        if not edge_vertices:
            raise RuntimeError(blade.name +
                               " has no geometry bound to CuttingEdge")
        all_points = [evaluated.matrix_world @ vertex.co
                      for vertex in mesh.vertices]
        edge_points = [all_points[index] for index in edge_vertices]
        all_minimum, all_maximum = bounds_for_points(all_points)
        edge_minimum, edge_maximum = bounds_for_points(edge_points)
        all_mean_x = sum(point.x for point in all_points) / len(all_points)
        edge_mean_x = sum(point.x for point in edge_points) / len(edge_points)
        return {
            "material": next(iter(edge_indices)),
            "vertexCount": len(edge_vertices),
            "minimum": rounded_vector(edge_minimum),
            "maximum": rounded_vector(edge_maximum),
            "meanX": round(edge_mean_x, 6),
            "allMeanX": round(all_mean_x, 6),
            "ownsNegativeXExtreme":
                edge_minimum.x <= all_minimum.x + 0.002,
            "isNegativeXOfBladeMean": edge_mean_x < all_mean_x - 0.001,
        }
    finally:
        evaluated.to_mesh_clear()


def measure_mesh_contract(label, objects, spec):
    minimum, maximum, points = evaluated_bounds(objects)
    center = (minimum + maximum) * 0.5
    span = maximum - minimum
    blade = next(obj for obj in objects if obj.name == label + "Blade")
    handle = next(obj for obj in objects if obj.name == label + "Handle")
    pommels = [obj for obj in objects if obj.name.startswith(label + "Pommel")]
    blade_minimum, blade_maximum, blade_points = evaluated_bounds([blade])
    handle_minimum, handle_maximum, _ = evaluated_bounds([handle])
    pommel_minimum, _, _ = evaluated_bounds(pommels)
    tip = extreme_center(blade_points, True)
    butt = extreme_center(points, False)
    cutting_edge = cutting_edge_measurement(blade)
    tip_matches_mesh = (abs(blade_maximum.z - maximum.z) <= 0.002 and
                        abs(tip.z - maximum.z) <= 0.00001)
    butt_matches_mesh = (abs(pommel_minimum.z - minimum.z) <= 0.002 and
                         abs(butt.z - minimum.z) <= 0.00001)
    grip_inside_handle = handle_minimum.z < 0.0 < handle_maximum.z
    grip_clear_of_blade = blade_minimum.z > 0.05
    blade_normal_ratio = ((blade_maximum.y - blade_minimum.y) /
                          (blade_maximum.z - blade_minimum.z))
    positive_identity_scales = all(
        all(abs(component - 1.0) <= 0.000001 for component in obj.scale)
        for obj in objects)
    if abs(maximum.z - spec["tip"]) > 0.004:
        raise RuntimeError(label + " physical tip disagrees with authored length")
    if not tip_matches_mesh or not butt_matches_mesh:
        raise RuntimeError(label +
                           " physical tip/pommel markers do not own mesh ends")
    if not grip_inside_handle or not grip_clear_of_blade:
        raise RuntimeError(label + " grip is not confined to the handle")
    if blade_normal_ratio >= 0.05:
        raise RuntimeError(label + " +Y is not a thin blade-plane normal")
    if (not cutting_edge["ownsNegativeXExtreme"] or
            not cutting_edge["isNegativeXOfBladeMean"]):
        raise RuntimeError(label + " CuttingEdge is not the physical -X edge")
    if not positive_identity_scales:
        raise RuntimeError(label + " has reflected or nonidentity mesh scale")
    return {
        "evaluatedVertexCount": len(points),
        "minimum": rounded_vector(minimum),
        "maximum": rounded_vector(maximum),
        "center": rounded_vector(center),
        "span": rounded_vector(span),
        "physicalTip": rounded_vector(tip),
        "physicalButt": rounded_vector(butt),
        "sourceForward": [0.0, 0.0, 1.0],
        "sourceBladeNormal": [0.0, 1.0, 0.0],
        "sourceCuttingEdge": [-1.0, 0.0, 0.0],
        "tipIsBladeExtreme": tip_matches_mesh,
        "pommelIsRearExtreme": butt_matches_mesh,
        "gripInsidePhysicalHandle": grip_inside_handle,
        "gripClearOfPhysicalBlade": grip_clear_of_blade,
        "bladeNormalToForwardSpanRatio": round(blade_normal_ratio, 6),
        "cuttingEdge": cutting_edge,
        "positiveIdentityMeshScales": positive_identity_scales,
    }


def add_semantic_marker(root, key, name, location):
    # Blender object names are scene-global. Keep stable variant-qualified names
    # in the .blend, then expose exact names in each independently exported FBX.
    marker = bpy.data.objects.new(name + "__" + key, None)
    bpy.context.collection.objects.link(marker)
    marker.empty_display_type = "PLAIN_AXES"
    marker.empty_display_size = 0.035
    marker.location = location
    marker.rotation_euler = (0.0, 0.0, 0.0)
    marker.scale = (1.0, 1.0, 1.0)
    marker.parent = root
    return marker


def palette_for(family, variant):
    steel, wrap, accent = PALETTES[family]
    shifts = {"classic": 1.0, "petal": 1.08, "reed": 0.92,
              "cleaver": 0.86, "moon": 0.78, "regal": 1.16,
              "titan": 0.94, "capstone": 1.22}
    factor = shifts[variant]
    return (tuple(min(0.88, c * factor) for c in steel), wrap,
            tuple(min(0.82, c * factor) for c in accent))


def build_variant(family, variant_tuple):
    variant, label, guard_mode, width_scale, curve_scale, pommel_mode = \
        variant_tuple
    spec = FAMILIES[family]
    steel, wrap, accent = palette_for(family, variant)
    steel_mat = material(label + "Steel", steel, 0.88, 0.18)
    wrap_mat = material(label + "Wrap", wrap, 0.08, 0.42)
    accent_mat = material(label + "Accent", accent, 0.72, 0.24)
    edge_color = tuple(min(1.0, value * 1.24 + 0.08) for value in steel)
    edge_mat = material(label + "CuttingEdge", edge_color, 0.92, 0.12)
    objects = []
    handle_mid = (spec["butt"] + spec["guard"]) / 2.0
    handle = cylinder(label + "Handle", spec["handle_radius"],
                      spec["guard"] - spec["butt"], handle_mid, wrap_mat, 20)
    handle.scale.y = 0.72
    objects.append(handle)
    objects.extend(wrap_bands(label, spec["butt"], spec["guard"],
                             spec["handle_radius"], accent_mat,
                             variant in ("regal", "titan", "capstone")))
    objects.extend(pommel_objects(label, pommel_mode,
                                  spec["handle_radius"], spec["butt"],
                                  accent_mat))
    width = spec["blade_width"] * width_scale
    objects.extend(guard_objects(label, guard_mode, width * 1.48,
                                 spec["guard"], accent_mat))
    objects.append(curved_blade(label + "Blade", spec["guard"] + 0.012,
                                spec["tip"], width,
                                spec["curve"] * curve_scale,
                                steel_mat, edge_mat))
    apply_mesh_contract(objects)
    mesh_contract = measure_mesh_contract(label, objects, spec)
    root = bpy.data.objects.new(label, None)
    bpy.context.collection.objects.link(root)
    for obj in objects:
        obj.parent = root
    markers = {
        "KMG_Grip": add_semantic_marker(
            root, family + "." + variant, "KMG_Grip", (0, 0, 0)),
        "KMG_Tip": add_semantic_marker(
            root, family + "." + variant, "KMG_Tip",
            mesh_contract["physicalTip"]),
        "KMG_Butt": add_semantic_marker(
            root, family + "." + variant, "KMG_Butt",
            mesh_contract["physicalButt"]),
        "KMG_Forward": add_semantic_marker(
            root, family + "." + variant, "KMG_Forward",
            (0, 0, SEMANTIC_AXIS_DISTANCE)),
        "KMG_BladeNormal": add_semantic_marker(
            root, family + "." + variant, "KMG_BladeNormal",
            (0, SEMANTIC_AXIS_DISTANCE, 0)),
        "KMG_Edge": add_semantic_marker(
            root, family + "." + variant, "KMG_Edge",
            (-SEMANTIC_AXIS_DISTANCE, 0, 0)),
        "KMG_Stored": add_semantic_marker(
            root, family + "." + variant, "KMG_Stored",
            mesh_contract["center"]),
    }
    if spec["support"] is not None:
        markers["KMG_Support"] = add_semantic_marker(
            root, family + "." + variant, "KMG_Support",
            (0, 0, spec["support"]))
    filename = family + ("" if variant == "classic" else "-" + variant) + ".fbx"
    return {"family": family, "variant": variant, "label": label,
            "filename": filename, "root": root, "objects": objects,
            "materials": (steel_mat, wrap_mat, accent_mat, edge_mat),
            "markers": markers, "meshContract": mesh_contract,
            "geometry": {"guard": guard_mode, "widthScale": width_scale,
                         "curveScale": curve_scale, "pommel": pommel_mode}}


def select_tree(root):
    root.select_set(True)
    for child in root.children_recursive:
        child.select_set(True)


def export_variant(built):
    for key, value in BUILT.items():
        for semantic, marker in value["markers"].items():
            marker.name = semantic + "__" + key
    for semantic, marker in built["markers"].items():
        marker.name = semantic
    try:
        bpy.ops.object.select_all(action="DESELECT")
        select_tree(built["root"])
        bpy.context.view_layer.objects.active = built["root"]
        path = ROOT / built["filename"]
        bpy.ops.export_scene.fbx(filepath=str(path), use_selection=True,
                                 apply_unit_scale=True,
                                 apply_scale_options="FBX_SCALE_UNITS",
                                 object_types={"EMPTY", "MESH"},
                                 add_leaf_bones=False, bake_anim=False,
                                 axis_forward="-Z", axis_up="Y")
        return path
    finally:
        for key, value in BUILT.items():
            for semantic, marker in value["markers"].items():
                marker.name = semantic + "__" + key


def look_at(obj, point):
    obj.rotation_euler = (Vector(point) - obj.location).to_track_quat(
        "-Z", "Y").to_euler()


def configure_palette(built, palette):
    edge = tuple(min(1.0, value * 1.24 + 0.08) for value in palette[0])
    properties = ((palette[0], 0.88, 0.18), (palette[1], 0.08, 0.42),
                  (palette[2], 0.72, 0.24), (edge, 0.92, 0.12))
    for mat, (color, metallic, roughness) in zip(built["materials"], properties):
        mat.diffuse_color = (*color, 1.0)
        node = mat.node_tree.nodes.get("Principled BSDF")
        node.inputs["Base Color"].default_value = (*color, 1.0)
        node.inputs["Metallic"].default_value = metallic
        node.inputs["Roughness"].default_value = roughness


def projected_angle(camera, butt, tip):
    bpy.context.view_layer.update()
    butt_view = world_to_camera_view(bpy.context.scene, camera,
                                     Vector((0, 0, butt)))
    tip_view = world_to_camera_view(bpy.context.scene, camera,
                                    Vector((0, 0, tip)))
    return math.degrees(math.atan2(tip_view.y - butt_view.y,
                                   tip_view.x - butt_view.x))


def apply_icon_roll(camera, butt, tip):
    base = camera.rotation_euler.to_quaternion()
    initial = projected_angle(camera, butt, tip)
    view_axis = base @ Vector((0, 0, -1))
    previous_roll, previous_angle = 0.0, initial
    roll = ICON_RENDER_ANGLE_DEGREES - initial
    observed = initial
    for _ in range(8):
        camera.rotation_euler = (Quaternion(view_axis, math.radians(roll)) @
                                 base).to_euler()
        observed = projected_angle(camera, butt, tip)
        if abs(observed - ICON_RENDER_ANGLE_DEGREES) <= 0.05:
            break
        slope = (observed - previous_angle) / (roll - previous_roll)
        if abs(slope) < 0.0001:
            raise RuntimeError("Icon camera roll solver has zero slope")
        previous_roll, previous_angle = roll, observed
        roll += (ICON_RENDER_ANGLE_DEGREES - observed) / slope
    if abs(observed - ICON_RENDER_ANGLE_DEGREES) > 0.05:
        raise RuntimeError("Icon camera roll did not reach its contract")
    return observed, roll


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
        output.extend(struct.pack(">I", zlib.crc32(chunk_type + payload) &
                                  0xFFFFFFFF))
    path.write_bytes(output)


def render_icon(built, palette_key, filename):
    for value in BUILT.values():
        value["root"].hide_render = value is not built
    configure_palette(built, PALETTES[palette_key])
    spec = FAMILIES[built["family"]]
    camera = bpy.data.objects.get("IconCamera")
    camera.data.ortho_scale = (spec["tip"] - spec["butt"]) * 1.35
    look_at(camera, (0.025, 0, (spec["tip"] + spec["butt"]) / 2))
    observed, roll = apply_icon_roll(camera, spec["butt"], spec["tip"])
    source = ROOT / (filename + "-icon-source.png")
    runtime = RUNTIME_ICONS / (filename + ".png")
    scene = bpy.context.scene
    scene.render.resolution_x = scene.render.resolution_y = 512
    scene.render.filepath = str(source)
    bpy.ops.render.render(write_still=True)
    normalize_png(source)
    scene.render.resolution_x = scene.render.resolution_y = 128
    scene.render.filepath = str(runtime)
    bpy.ops.render.render(write_still=True)
    normalize_png(runtime)
    return source, runtime, {"tipDirection": "upper-right",
        "buttDirection": "lower-left", "targetAngleDegrees": 42.0,
        "observedAngleDegrees": round(observed, 6),
        "cameraRollDegrees": round(roll, 6),
        "sourceDimensions": [512, 512], "runtimeDimensions": [128, 128],
        "background": "transparent RGBA"}


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
BUILT = {}
for family, variants in VARIANTS.items():
    for variant in variants:
        value = build_variant(family, variant)
        BUILT[family + "." + value["variant"]] = value

exporter, fbx_utils, real_datetime, real_export_uuid, real_utils_uuid = \
    install_deterministic_fbx_contract()
exports = [export_variant(value) for value in BUILT.values()]
exporter.datetime.datetime = real_datetime
exporter.get_fbx_uuid_from_key = real_export_uuid
fbx_utils.get_fbx_uuid_from_key = real_utils_uuid

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
scene.display.render_aa = "OFF"
scene.display.shading.light = "STUDIO"
scene.display.shading.color_type = "MATERIAL"
scene.display.shading.show_shadows = False
scene.display.shading.show_cavity = False
scene.display.shading.show_specular_highlight = False

icon_outputs = []
icon_contracts = {}
for family in FAMILIES:
    source, runtime, contract = render_icon(BUILT[family + ".classic"],
                                            family, family)
    icon_outputs.extend((source, runtime))
    icon_contracts[family] = contract
for icon_name, family in CAPSTONE_ICONS.items():
    source, runtime, contract = render_icon(BUILT[family + ".capstone"],
                                            icon_name, icon_name)
    icon_outputs.extend((source, runtime))
    icon_contracts[icon_name] = contract

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
    "schemaVersion": 3,
    "generator": Path(__file__).name,
    "blenderVersion": bpy.app.version_string,
    "license": "Original project-owned assets; repository license applies",
    "sourceCoordinateContract": "+Z longitudinal forward; +Y blade normal; -X physical cutting edge; mesh-grounded tip/pommel; grip origin; metric",
    "unityImportCoordinateContract": "FBX import reflects Blender X: Unity +Z longitudinal, raw +Y marker, +X physical edge; Unity builder reverses the oriented normal to -Y to restore the authored right-handed +Y-normal/-X-edge relationship before donor-basis solving",
    "equippedExportContract": "12 identity roots with KMG semantic markers exported before render-only cameras/lights",
    "bladeContract": "family-safe curved asymmetric single edge at local -X; blunt spine at local +X",
    "determinism": {"verifiedCleanRuns": 2,
        "byteStableBoundary": "12 FBXs and 12 normalized PNGs",
        "blendContainer": "semantic regeneration; Blender session metadata is not byte-stable",
        "fbxStabilization": "SHA-256 exporter UUIDs; unused UV packing omitted",
        "pngStabilization": "session metadata removed; exact pixels retained"},
    "iconRender": icon_contracts,
    "families": {key: dict(value,
        overallLengthMeters=value["tip"] - value["butt"])
        for key, value in FAMILIES.items()},
    "variants": {key: {"prefab": value["label"],
        "fbx": value["filename"], "geometry": value["geometry"],
        "meshObjects": len(value["objects"]),
        "semanticMarkers": {
            name: rounded_vector(marker.location)
            for name, marker in value["markers"].items()
        },
        "meshFrame": value["meshContract"]} for key, value in BUILT.items()},
    "meshObjects": len(mesh_objects),
    "triangles": sum(len(obj.data.loop_triangles) for obj in mesh_objects),
    "outputs": {},
}
for path in [Path(__file__), BLEND] + exports + icon_outputs:
    report["outputs"][path.name] = {"sha256": sha256(path),
                                    "bytes": path.stat().st_size}
REPORT.write_text(json.dumps(report, indent=2, sort_keys=True) + "\n",
                  encoding="utf-8")
print("KMG_EASTERN_WEAPONS_BUILD " + json.dumps(report, sort_keys=True))
