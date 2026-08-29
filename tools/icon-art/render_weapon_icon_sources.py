"""Render only 2D icon sources from the checked-in project-owned weapon FBXs.

This deliberately does not regenerate or rewrite any production FBX or .blend
container. Runtime 128px textures are produced separately by
New-IconOverhaulAssets.ps1 from these normalized 512px transparent sources.
"""
from __future__ import annotations

import bpy
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
REPO = ROOT.parents[1]
REPORT = REPO / "assets-source" / "original-icons" / (
    "icon-overhaul-weapon-render-report.json")
TARGET_ANGLE_DEGREES = 42.0
SOURCE_SIZE = 512
SAFETY_SCALE = 1.12
LIGHT_MODE = "FLAT"

SPECS = (
    ("wakizashi", "assets-source/original-models/eastern-weapons/wakizashi.fbx",
     "assets-source/original-models/eastern-weapons/wakizashi-icon-source.png"),
    ("katana", "assets-source/original-models/eastern-weapons/katana.fbx",
     "assets-source/original-models/eastern-weapons/katana-icon-source.png"),
    ("nodachi", "assets-source/original-models/eastern-weapons/nodachi.fbx",
     "assets-source/original-models/eastern-weapons/nodachi-icon-source.png"),
    ("night-without-moon",
     "assets-source/original-models/eastern-weapons/wakizashi-capstone.fbx",
     "assets-source/original-models/eastern-weapons/night-without-moon-icon-source.png"),
    ("heavens-measure",
     "assets-source/original-models/eastern-weapons/katana-capstone.fbx",
     "assets-source/original-models/eastern-weapons/heavens-measure-icon-source.png"),
    ("world-tree-severer",
     "assets-source/original-models/eastern-weapons/nodachi-capstone.fbx",
     "assets-source/original-models/eastern-weapons/world-tree-severer-icon-source.png"),
    ("elven-branched-spear",
     "assets-source/original-models/elven-branched-spear/elven-branched-spear.fbx",
     "assets-source/original-models/elven-branched-spear/elven-branched-spear-icon.png"),
)

if os.environ.get("PYTHONHASHSEED") != "0":
    raise RuntimeError("Deterministic icon rendering requires PYTHONHASHSEED=0")


def clear_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for blocks in (bpy.data.meshes, bpy.data.curves, bpy.data.materials,
                   bpy.data.cameras, bpy.data.lights):
        for block in list(blocks):
            if block.users == 0:
                blocks.remove(block)


def material_palette() -> list[dict]:
    """Record the unchanged project material swatches used by Workbench."""
    records: list[dict] = []
    for material in sorted(bpy.data.materials, key=lambda value: value.name):
        color = tuple(float(channel) for channel in material.diffuse_color)
        records.append({
            "material": material.name,
            "diffuseColor": [round(value, 6) for value in color],
        })
    return records


def mesh_points() -> list[Vector]:
    bpy.context.view_layer.update()
    graph = bpy.context.evaluated_depsgraph_get()
    points: list[Vector] = []
    for obj in bpy.context.scene.objects:
        if obj.type != "MESH":
            continue
        evaluated = obj.evaluated_get(graph)
        mesh = evaluated.to_mesh()
        try:
            points.extend(evaluated.matrix_world @ vertex.co
                          for vertex in mesh.vertices)
        finally:
            evaluated.to_mesh_clear()
    if not points:
        raise RuntimeError("Imported icon source has no mesh vertices")
    return points


def marker(name: str) -> Vector:
    matches = [obj for obj in bpy.context.scene.objects
               if obj.name == name or obj.name.startswith(name + ".") or
               obj.name.startswith(name + "__")]
    if len(matches) != 1:
        raise RuntimeError(f"Expected one {name} marker; observed {len(matches)}")
    return matches[0].matrix_world.translation.copy()


def principal_line(points: list[Vector], authored_butt: Vector,
                   authored_tip: Vector) -> tuple[Vector, Vector]:
    """Return mesh-grounded butt/tip points along the longest covariance axis."""
    center = sum(points, Vector()) / len(points)
    axis = (authored_tip - authored_butt).normalized()
    for _ in range(24):
        next_axis = Vector((0.0, 0.0, 0.0))
        for point in points:
            delta = point - center
            next_axis += delta * delta.dot(axis)
        if next_axis.length <= 0.0000001:
            raise RuntimeError("Weapon mesh principal axis is degenerate")
        axis = next_axis.normalized()
    if axis.dot(authored_tip - authored_butt) < 0.0:
        axis.negate()
    distances = [(point - center).dot(axis) for point in points]
    return center + axis * min(distances), center + axis * max(distances)


def look_at(obj, point: Vector) -> None:
    obj.rotation_euler = (point - obj.location).to_track_quat(
        "-Z", "Y").to_euler()


def projected_angle(camera, butt: Vector, tip: Vector) -> float:
    scene = bpy.context.scene
    bpy.context.view_layer.update()
    back = world_to_camera_view(scene, camera, butt)
    front = world_to_camera_view(scene, camera, tip)
    return math.degrees(math.atan2(front.y - back.y, front.x - back.x))


def apply_roll(camera, butt: Vector, tip: Vector) -> tuple[float, float]:
    base = camera.rotation_euler.to_quaternion()
    initial = projected_angle(camera, butt, tip)
    view_axis = base @ Vector((0.0, 0.0, -1.0))
    previous_roll, previous_angle = 0.0, initial
    roll, observed = TARGET_ANGLE_DEGREES - initial, initial
    for _ in range(10):
        camera.rotation_euler = (Quaternion(view_axis, math.radians(roll)) @
                                 base).to_euler()
        observed = projected_angle(camera, butt, tip)
        if abs(observed - TARGET_ANGLE_DEGREES) <= 0.02:
            break
        denominator = roll - previous_roll
        if abs(denominator) < 0.000001:
            denominator = 0.000001
        slope = (observed - previous_angle) / denominator
        if abs(slope) < 0.0001:
            raise RuntimeError("Icon camera roll solver has zero slope")
        previous_roll, previous_angle = roll, observed
        roll += (TARGET_ANGLE_DEGREES - observed) / slope
    if abs(observed - TARGET_ANGLE_DEGREES) > 0.02:
        raise RuntimeError("Icon camera roll contract failed")
    return observed, roll


def projected_bounds(camera, points: list[Vector]) -> tuple[float, ...]:
    inverse = camera.matrix_world.inverted()
    projected = [inverse @ point for point in points]
    xs = [point.x for point in projected]
    ys = [point.y for point in projected]
    return min(xs), min(ys), max(xs), max(ys)


def center_and_fit(camera, points: list[Vector]) -> tuple[float, ...]:
    minimum_x, minimum_y, maximum_x, maximum_y = projected_bounds(
        camera, points)
    center_x = (minimum_x + maximum_x) * 0.5
    center_y = (minimum_y + maximum_y) * 0.5
    rotation = camera.matrix_world.to_quaternion()
    camera.location += (rotation @ Vector((1.0, 0.0, 0.0))) * center_x
    camera.location += (rotation @ Vector((0.0, 1.0, 0.0))) * center_y
    bpy.context.view_layer.update()
    minimum_x, minimum_y, maximum_x, maximum_y = projected_bounds(
        camera, points)
    width = maximum_x - minimum_x
    height = maximum_y - minimum_y
    camera.data.ortho_scale = max(width, height) * SAFETY_SCALE
    return minimum_x, minimum_y, maximum_x, maximum_y, width, height


def normalize_png(path: Path) -> None:
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


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def render(key: str, fbx_relative: str, output_relative: str) -> dict:
    clear_scene()
    fbx = REPO / fbx_relative
    output = REPO / output_relative
    bpy.ops.import_scene.fbx(filepath=str(fbx), use_anim=False)
    palette = material_palette()
    points = mesh_points()
    authored_tip = marker("KMG_Tip")
    authored_butt = marker("KMG_Butt")
    butt, tip = principal_line(points, authored_butt, authored_tip)
    center = sum(points, Vector()) / len(points)

    bpy.ops.object.camera_add(location=center + Vector((3.15, -4.35, 2.45)))
    camera = bpy.context.object
    camera.name = "KMG_IconOverhaulCamera"
    camera.data.type = "ORTHO"
    look_at(camera, center)
    observed, roll = apply_roll(camera, butt, tip)
    bounds = center_and_fit(camera, points)
    bpy.context.scene.camera = camera

    scene = bpy.context.scene
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.render.resolution_x = SOURCE_SIZE
    scene.render.resolution_y = SOURCE_SIZE
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"
    scene.render.film_transparent = True
    scene.render.filepath = str(output)
    scene.display.render_aa = "FXAA"
    scene.display.shading.light = LIGHT_MODE
    scene.display.shading.color_type = "MATERIAL"
    scene.display.shading.show_shadows = True
    scene.display.shading.show_cavity = True
    scene.display.shading.cavity_type = "WORLD"
    scene.display.shading.show_specular_highlight = True
    output.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.render.render(write_still=True)
    normalize_png(output)
    return {
        "key": key,
        "input": fbx_relative,
        "inputSha256": sha256(fbx),
        "output": output_relative,
        "outputSha256": sha256(output),
        "outputBytes": output.stat().st_size,
        "sourceDimensions": [SOURCE_SIZE, SOURCE_SIZE],
        "targetAngleDegrees": TARGET_ANGLE_DEGREES,
        "observedAngleDegrees": round(observed, 6),
        "cameraRollDegrees": round(roll, 6),
        "projectedWidth": round(bounds[4], 6),
        "projectedHeight": round(bounds[5], 6),
        "orthographicSafetyScale": SAFETY_SCALE,
        "materialPalette": palette,
        "workbenchLightMode": LIGHT_MODE,
        "background": "transparent RGBA",
    }


records = [render(*spec) for spec in SPECS]
report = {
    "schemaVersion": 1,
    "generator": "tools/icon-art/render_weapon_icon_sources.py",
    "blenderVersion": bpy.app.version_string,
    "license": "Project-owned FBX inputs; repository license applies",
    "purpose": "render-only 2D icon reframing; no FBX or blend mutation",
    "records": records,
}
REPORT.write_text(json.dumps(report, indent=2, sort_keys=True) + "\n",
                  encoding="utf-8")
print("KMG_ICON_OVERHAUL_SOURCE_RENDER " + json.dumps(report, sort_keys=True))
