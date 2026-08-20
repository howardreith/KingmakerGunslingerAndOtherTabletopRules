"""Generate normalized Musket and Blunderbuss FBX derivatives with rig markers."""
from pathlib import Path
import bpy
import json
import math
import hashlib
import datetime
from io_scene_fbx import export_fbx_bin, fbx_utils

class FixedDateTime(datetime.datetime):
    @classmethod
    def now(cls, tz=None):
        return cls(2000, 1, 1, 0, 0, 0, tzinfo=tz)

export_fbx_bin.datetime.datetime = FixedDateTime

def stable_key_to_uuid(uuids, key):
    if isinstance(key, int) and 0 <= key < 2**63:
        value = key
    else:
        value = int.from_bytes(hashlib.sha256(repr(key).encode("utf-8")).digest()[:8],
                               "big") & ((1 << 63) - 1)
    if value > 1_000_000_000:
        short = value % 1_000_000_000
        if short not in uuids:
            value = short
    while value in uuids:
        value += 1
    return fbx_utils.UUID(value)

fbx_utils._key_to_uuid = stable_key_to_uuid

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[3]
SPECS = (
    {"name": "musket-normalized", "source": REPO / "assets-source/third-party/models/mesh-masters-rifle-musket/source/Musket 01.fbx", "length": 1.34, "grip_fraction": 0.22, "support": (0.031, -0.051, 0.48), "color": (0.19, 0.10, 0.045, 1.0)},
    {"name": "blunderbuss-normalized", "source": REPO / "assets-source/third-party/models/ccotwist-blunderbuss/source/Blunderbuss_Low_Poly.fbx", "length": 0.86, "grip_fraction": 0.27, "support": (0.031, -0.051, 0.36), "color": (0.24, 0.12, 0.055, 1.0)},
)

def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()

def clear():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)

def marker(parent, name, position):
    obj = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(obj)
    obj.empty_display_type = "PLAIN_AXES"
    obj.empty_display_size = 0.04
    obj.parent = parent
    obj.location = position

def generate(spec):
    clear()
    bpy.ops.import_scene.fbx(filepath=str(spec["source"]))
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    if not meshes:
        raise RuntimeError("No mesh imported from " + str(spec["source"]))
    points = [obj.matrix_world @ v.co for obj in meshes for v in obj.data.vertices]
    mins = [min(p[i] for p in points) for i in range(3)]
    maxs = [max(p[i] for p in points) for i in range(3)]
    spans = [maxs[i] - mins[i] for i in range(3)]
    axis = spans.index(max(spans))
    root = bpy.data.objects.new("KMG_LongGunRoot", None)
    bpy.context.collection.objects.link(root)
    for obj in meshes:
        obj.parent = root
    root.rotation_euler[1] = math.radians(-90.0) if axis == 0 else 0.0
    root.scale = (spec["length"] / spans[axis],) * 3
    bpy.context.view_layer.objects.active = root
    root.select_set(True)
    for obj in meshes:
        obj.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    points = [obj.matrix_world @ v.co for obj in meshes for v in obj.data.vertices]
    zmin, zmax = min(p.z for p in points), max(p.z for p in points)
    grip_z = zmin + (zmax - zmin) * spec["grip_fraction"]
    cx = sum(p.x for p in points) / len(points)
    cy = sum(p.y for p in points) / len(points)
    for obj in meshes:
        obj.location.x -= cx
        obj.location.y -= cy
        obj.location.z -= grip_z
        for material in obj.data.materials:
            material.diffuse_color = spec["color"]
            material.use_nodes = False
            material.name = "KMG_AgedLongGun"
    root.location = (0, 0, 0)
    root.rotation_euler = (0, 0, 0)
    root.scale = (1, 1, 1)
    marker(root, "KMG_Grip", (0, 0, 0))
    marker(root, "KMG_Support", spec["support"])
    marker(root, "KMG_Butt", (0, 0, zmin - grip_z))
    marker(root, "KMG_Muzzle", (0, 0, zmax - grip_z))
    marker(root, "KMG_Back", (0, 0, 0))
    output = ROOT / (spec["name"] + ".fbx")
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for obj in root.children_recursive:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = root
    bpy.ops.export_scene.fbx(filepath=str(output), use_selection=True,
        apply_unit_scale=True, apply_scale_options="FBX_SCALE_UNITS",
        axis_forward="-Z", axis_up="Y", add_leaf_bones=False,
        bake_anim=False, path_mode="COPY")
    return {"name": spec["name"], "source": str(spec["source"].relative_to(REPO)),
        "sourceSha256": sha256(spec["source"]), "output": output.name,
        "outputSha256": sha256(output), "lengthMeters": spec["length"],
        "markers": ["KMG_Grip", "KMG_Support", "KMG_Butt", "KMG_Muzzle", "KMG_Back"]}

records = [generate(spec) for spec in SPECS]
(ROOT / "generation-report.json").write_text(json.dumps({"schemaVersion": 1,
    "generator": Path(__file__).name, "outputs": records}, indent=2) + "\n",
    encoding="utf-8")
print("KMG_LONG_GUN_DERIVATIVES " + json.dumps(records))
