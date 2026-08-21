"""Generate canonical Musket, Blunderbuss, and Rifle presentation FBXs."""
from pathlib import Path
import bpy
import json
import hashlib
import datetime
from io_scene_fbx import export_fbx_bin, fbx_utils
from mathutils import Matrix, Vector


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
MARKERS = (
    "KMG_Grip",
    "KMG_Support",
    "KMG_Butt",
    "KMG_Muzzle",
    "KMG_Back",
    "KMG_WeaponUp",
    "KMG_WeaponForward",
)

# The preserved sources all use physical +X from butt to muzzle and physical
# +Z toward the top of the stock/receiver. Grip coordinates were selected on
# the actual wrist immediately behind each trigger, not inferred from a model
# origin or a generic percentage. The generated frame maps source +X/+Z to
# canonical Blender +Z/+Y. Unity's FBX import changes canonical X to -X.
SPECS = (
    {
        "name": "musket-normalized",
        "source": REPO / "assets-source/third-party/models/mesh-masters-rifle-musket/source/Musket 01.fbx",
        "length": 1.34,
        "source_grip": (-0.009519563, -0.000581455, -0.010190492),
        # Runtime contact calibration against the exact native Heavy Crossbow
        # places the left-hand IK station at 0.374 m from the weapon root.
        "support_forward": 0.374,
        "grip_evidence": "narrow wooden wrist immediately behind trigger guard",
        "color": (0.19, 0.10, 0.045, 1.0),
    },
    {
        "name": "blunderbuss-normalized",
        "source": REPO / "assets-source/third-party/models/ccotwist-blunderbuss/source/Blunderbuss_Low_Poly.fbx",
        "length": 0.86,
        "source_grip": (-1.045805006, -0.010080880, -0.074088934),
        "support_forward": 0.36,
        "grip_evidence": "narrow stock wrist immediately behind trigger guard",
        "color": (0.24, 0.12, 0.055, 1.0),
    },
    {
        "name": "rifle-normalized",
        "source": REPO / "assets-source/third-party/models/killian-delias-winchester-lever-action-rifle/source/fusilALevier.fbx",
        "length": 1.01,
        "source_grip": (-0.180000000, 0.000000000, 0.145000000),
        "support_forward": 0.374,
        "grip_evidence": "stock wrist behind gachette and inside the rear levier span",
        "color": (0.22, 0.12, 0.055, 1.0),
    },
)


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def clear():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def numbers(vector):
    return [round(float(value), 9) for value in vector]


def marker(parent, name, position):
    obj = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(obj)
    obj.empty_display_type = "PLAIN_AXES"
    obj.empty_display_size = 0.04
    obj.parent = parent
    obj.location = position


def bounds(points):
    minimum = Vector(tuple(min(point[index] for point in points)
                           for index in range(3)))
    maximum = Vector(tuple(max(point[index] for point in points)
                           for index in range(3)))
    return minimum, maximum


def endpoint(points, axis, forward):
    projections = [point.dot(axis) for point in points]
    extreme = max(projections) if forward else min(projections)
    span = max(projections) - min(projections)
    tolerance = max(1.0e-6, span * 0.003)
    section = [point for point, projection in zip(points, projections)
               if abs(projection - extreme) <= tolerance]
    mean = sum(section, Vector()) / len(section)
    return mean + axis * (extreme - mean.dot(axis)), len(section)


def canonical_matrix(grip, scale, source_right, source_up, source_forward):
    return Matrix((
        (source_right.x * scale, source_right.y * scale,
         source_right.z * scale, -grip.dot(source_right) * scale),
        (source_up.x * scale, source_up.y * scale,
         source_up.z * scale, -grip.dot(source_up) * scale),
        (source_forward.x * scale, source_forward.y * scale,
         source_forward.z * scale, -grip.dot(source_forward) * scale),
        (0.0, 0.0, 0.0, 1.0),
    ))


def transformed_point(matrix, point):
    return matrix @ point


def validate(spec, canonical_points, marker_points, scale, nearest_grip):
    minimum, maximum = bounds(canonical_points)
    grip = marker_points["KMG_Grip"]
    support = marker_points["KMG_Support"]
    butt = marker_points["KMG_Butt"]
    muzzle = marker_points["KMG_Muzzle"]
    weapon_up = marker_points["KMG_WeaponUp"] - grip
    # The physical bore axis is canonical +Z. Endpoint centers may have a
    # small stock/barrel height offset and therefore must not redefine roll.
    forward = marker_points["KMG_WeaponForward"] - grip
    if scale <= 0.0 or abs(forward.dot(weapon_up.normalized())) >= 0.01:
        raise RuntimeError(spec["name"] + " has a degenerate semantic basis")
    if muzzle.z <= 0.0 or butt.z >= 0.0 or not (0.0 < support.z < muzzle.z):
        raise RuntimeError(spec["name"] + " has reversed or unordered anchors")
    if abs(muzzle.z - maximum.z) > 1.0e-5 or abs(butt.z - minimum.z) > 1.0e-5:
        raise RuntimeError(spec["name"] + " endpoints do not match renderer bounds")
    if (Vector((support.x, support.y, 0.0))).length > 0.10:
        raise RuntimeError(spec["name"] + " support target is outside the fore-end")
    if nearest_grip > 0.12:
        raise RuntimeError(spec["name"] + " grip is not adjacent to source geometry")
    if minimum.x > grip.x or maximum.x < grip.x or minimum.y > grip.y or maximum.y < grip.y:
        raise RuntimeError(spec["name"] + " grip is outside the renderer cross-section")


def generate(spec):
    clear()
    bpy.ops.import_scene.fbx(filepath=str(spec["source"]))
    meshes = sorted((obj for obj in bpy.context.scene.objects
                     if obj.type == "MESH"), key=lambda obj: obj.name)
    if not meshes:
        raise RuntimeError("No mesh imported from " + str(spec["source"]))

    source_points = [obj.matrix_world @ vertex.co
                     for obj in meshes for vertex in obj.data.vertices]
    source_minimum, source_maximum = bounds(source_points)
    source_forward = Vector((1.0, 0.0, 0.0))
    source_up = Vector((0.0, 0.0, 1.0))
    source_right = source_up.cross(source_forward).normalized()
    if abs(source_forward.dot(source_up)) >= 0.001:
        raise RuntimeError(spec["name"] + " source frame is collinear")
    source_butt, butt_vertices = endpoint(source_points, source_forward, False)
    source_muzzle, muzzle_vertices = endpoint(source_points, source_forward, True)
    source_length = source_muzzle.dot(source_forward) - source_butt.dot(source_forward)
    scale = spec["length"] / source_length
    source_grip = Vector(spec["source_grip"])
    transform = canonical_matrix(source_grip, scale, source_right,
                                 source_up, source_forward)

    for obj in meshes:
        obj.data = obj.data.copy()
        obj.data.transform(transform @ obj.matrix_world)
        obj.data.update()
        obj.parent = None
        obj.matrix_world = Matrix.Identity(4)
        for material in obj.data.materials:
            material.diffuse_color = spec["color"]
            material.use_nodes = False
            material.name = "KMG_AgedLongGun"

    for obj in tuple(bpy.context.scene.objects):
        if obj not in meshes:
            bpy.data.objects.remove(obj, do_unlink=True)
    root = bpy.data.objects.new("KMG_LongGunRoot", None)
    bpy.context.collection.objects.link(root)
    for obj in meshes:
        obj.parent = root

    canonical_points = [vertex.co.copy() for obj in meshes
                        for vertex in obj.data.vertices]
    canonical_minimum, canonical_maximum = bounds(canonical_points)
    source_back = (source_minimum + source_maximum) * 0.5
    marker_points = {
        "KMG_Grip": Vector(),
        # Blender +X becomes Unity -X on import. This authored +X therefore
        # produces the exact Heavy Crossbow left-hand target X=-0.031 m.
        "KMG_Support": Vector((0.031, -0.051, spec["support_forward"])),
        "KMG_Butt": transformed_point(transform, source_butt),
        "KMG_Muzzle": transformed_point(transform, source_muzzle),
        "KMG_Back": transformed_point(transform, source_back),
        "KMG_WeaponUp": Vector((0.0, 0.10, 0.0)),
        "KMG_WeaponForward": Vector((0.0, 0.0, 0.10)),
    }
    nearest_grip = min((point - source_grip).length for point in source_points) * scale
    validate(spec, canonical_points, marker_points, scale, nearest_grip)
    for name in MARKERS:
        marker(root, name, marker_points[name])

    output = ROOT / (spec["name"] + ".fbx")
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for obj in root.children_recursive:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = root
    bpy.ops.export_scene.fbx(
        filepath=str(output), use_selection=True,
        object_types={"EMPTY", "MESH"},
        apply_unit_scale=True, apply_scale_options="FBX_SCALE_UNITS",
        axis_forward="-Z", axis_up="Y", add_leaf_bones=False,
        bake_anim=False, path_mode="COPY")

    unity_markers = {name: numbers(Vector((-point.x, point.y, point.z)))
                     for name, point in marker_points.items()}
    return {
        "name": spec["name"],
        "source": spec["source"].relative_to(REPO).as_posix(),
        "sourceSha256": sha256(spec["source"]),
        "output": output.name,
        "outputSha256": sha256(output),
        "lengthMeters": spec["length"],
        "sourceFrame": {
            "forward": numbers(source_forward),
            "up": numbers(source_up),
            "right": numbers(source_right),
            "grip": numbers(source_grip),
            "gripEvidence": spec["grip_evidence"],
            "butt": numbers(source_butt),
            "muzzle": numbers(source_muzzle),
            "buttEndpointVertexCount": butt_vertices,
            "muzzleEndpointVertexCount": muzzle_vertices,
        },
        "canonicalFrame": {
            "forward": [0.0, 0.0, 1.0],
            "up": [0.0, 1.0, 0.0],
            "right": [1.0, 0.0, 0.0],
            "scale": round(scale, 9),
            "boundsMinimum": numbers(canonical_minimum),
            "boundsMaximum": numbers(canonical_maximum),
            "nearestGripGeometryMeters": round(nearest_grip, 9),
        },
        "markers": list(MARKERS),
        "blenderMarkerMeters": {name: numbers(marker_points[name])
                                 for name in MARKERS},
        "expectedUnityMarkerMeters": unity_markers,
        "unityAxisConversion": "Blender canonical (x,y,z) -> Unity (-x,y,z)",
    }


records = [generate(spec) for spec in SPECS]
(ROOT / "generation-report.json").write_text(json.dumps({
    "schemaVersion": 2,
    "generator": Path(__file__).name,
    "sourceForwardAxis": "+X physical butt-to-muzzle",
    "sourceUpAxis": "+Z physical stock/receiver up",
    "canonicalForwardAxis": "+Z",
    "canonicalUpAxis": "+Y",
    "outputs": records,
}, indent=2) + "\n", encoding="utf-8")
print("KMG_LONG_GUN_DERIVATIVES " + json.dumps(records))
