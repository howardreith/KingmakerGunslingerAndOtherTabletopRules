#!/usr/bin/env python3
"""Generate the Pteranodon replacement body against the donor's bind pose.

Run headless:

    blender --background --factory-startup --python generate_pteranodon.py \
        -- --rig rig.measured.json --out pteranodon.fbx [--parts all|body|membrane]
           [--albedo pteranodon-albedo.png --mesh-data pteranodon-mesh.json]

This supersedes the membrane-only prototype: the membrane code is unchanged and
the body is authored onto the same rig, in the same frame.

READ FIRST
----------
Everything is authored in the donor renderer's own space - +X to the creature's
left, +Y up, -Z forward - and against the **bind pose**, not a resolved live
pose. The two disagree by up to 3.954 units at the wingtip: the live pose of a
summoned unit has the wings folded (1.638 across against 3.152 long) because it
is whatever frame the animation system is on, while the bind pose is spread
(8.641 across against 2.937 long) and mirrors left to right to 1e-5. Authoring
against the live reading is how two earlier iterations failed review.

Where the donor's anatomy and a pterosaur's disagree, see body_plan.md; the
short version is that the beak extends forward of the last head bone on purpose,
the crest is weighted entirely to `Head` because a crest is bone, and the
donor's eagle tail fan is deliberately left with no geometry on it rather than
being dressed up as a tail.

Texture coordinates are generated here too, into the atlas `ATLAS` describes,
and `paint_pteranodon_albedo.py` paints that atlas. The two scripts share the
region table, so a change to it is a change to both.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
import sys

import bpy  # noqa: E402
import bmesh  # noqa: E402
from mathutils import Vector  # noqa: E402


# --- membrane -------------------------------------------------------------
CHORD_ROWS = 5
SPAN_SUBDIVISIONS = 3
SAG = 0.10
HALF_THICKNESS = 0.012
TRAILING_CHAIN = ["L_Foot0", "L_Feather_6_end", "L_Feather_5_end",
                  "L_Feather_4_end", "L_Feather_3_end", "L_Feather_2_end",
                  "L_Feather_1_end"]
TRAILING_BONES = ["L_Foot0", "L_Feather_6", "L_Feather_5", "L_Feather_4",
                  "L_Feather_3", "L_Feather_2", "L_Feather_1"]
LEADING_CHAIN = ["L_Arm_Upper", "L_Arm_Lower", "L_Palm", "L_Feather_1",
                 "L_Feather_1_end"]
LEADING_BONES = ["L_Arm_Upper", "L_Arm_Lower", "L_Palm", "L_Feather_1"]

# --- body -----------------------------------------------------------------
RING_SEGMENTS = 8
# Beak tip, forward of every bone. A Pteranodon's toothless beak is about as
# long again as its skull.
BEAK_TIP_Z = -2.25
LOWER_BEAK_TIP_Z = -2.12
BEAK_HALF_HEIGHT = 0.075
BEAK_HALF_WIDTH = 0.055
# Crest: a spike swept back and up from the skull, with the TIP as its apex.
# Iterations 5 and 6 both peaked directly above the skull and trailed down
# behind it, which reads as a wedge sitting on the head. A Pteranodon's crest
# is recognised by the opposite shape: the highest point is the trailing tip,
# and the upper edge runs from the brow to that tip in one rising line, about
# a third of a right angle above the beak. Length is about the beak's.
CREST_TIP_BACK = 1.05
CREST_TIP_UP = 0.72
# Half-width at the skull, thinning to a blade at the tip. The base is thick
# enough to register from the game's high camera, where a paper-thin sagittal
# blade vanishes edge-on.
CREST_HALF_WIDTH_BASE = 0.060
CREST_HALF_WIDTH_TIP = 0.012
# Only the first third of the donor's eagle tail carries geometry.
TAIL_STUB_FRACTION = 0.35
# Blend across the last quarter of each segment so joints do not crease.
JOINT_BLEND = 0.25

# --- texture atlas --------------------------------------------------------
# Regions of the albedo as (u0, v0, u1, v1), v upward as Unity samples it.
# Tubes map their length along u and a belly-to-back FOLD along v: the ring
# angle is folded so the left and right flanks share texels. That leaves the
# body with no seam anywhere and makes countershading a plain gradient in v.
# The membrane maps span along u and chord along v, and both faces of both
# wings share the one sheet. The crest is a side projection. The two beaks
# take the two halves of their region.
ATLAS = {
    "membrane": (0.0, 0.5, 1.0, 1.0),
    "body": (0.0, 0.25, 0.5, 0.5),
    "crest": (0.5, 0.25, 1.0, 0.5),
    "beak": (0.0, 0.0, 0.5, 0.25),
    "limbs": (0.5, 0.0, 1.0, 0.25),
}


def log(message):
    print("[pteranodon] " + message)


def parse_args():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--rig", required=True)
    parser.add_argument("--out", required=True)
    parser.add_argument("--parts", default="all",
                        choices=("all", "body", "membrane"))
    parser.add_argument("--blend-out", default=None)
    parser.add_argument("--report", default=None)
    parser.add_argument("--mesh-data", default=None,
                        help="runtime mesh data written for the loader")
    parser.add_argument("--albedo", default=None,
                        help="painted albedo the mesh data will reference")
    args = parser.parse_args(argv)
    if args.mesh_data and not args.albedo:
        parser.error("--mesh-data needs --albedo: the runtime loads both")
    return args


def load_rig(path):
    rig = json.loads(Path(path).read_text(encoding="utf-8"))
    if "bindPoseCount" not in rig:
        raise SystemExit(
            "rig was not built from bind poses; re-run convert_bind_rig.py")
    bones = {b["name"]: b for b in rig["bones"]}
    return rig, bones


def head(bones, name):
    if name not in bones:
        raise SystemExit("rig is missing bone " + name)
    return Vector(bones[name]["head"])


def mirror(point):
    return Vector((-point.x, point.y, point.z))


def region_uv(name, u, v):
    """Maps a (u, v) inside one atlas region to texture coordinates."""
    u0, v0, u1, v1 = ATLAS[name]
    u = min(1.0, max(0.0, u))
    v = min(1.0, max(0.0, v))
    return (u0 + u * (u1 - u0), v0 + v * (v1 - v0))


def fold(angle):
    """Belly 0, flank 0.5, back 1. The ring angle is folded rather than
    unwrapped so the two flanks share texels and there is no seam."""
    return 0.5 + 0.5 * math.sin(angle)


def resample(points, count):
    lengths = [0.0]
    for index in range(1, len(points)):
        lengths.append(lengths[-1] + (points[index] - points[index - 1]).length)
    total = lengths[-1]
    result = []
    for index in range(count):
        target = total * index / float(count - 1)
        segment = 0
        while segment < len(lengths) - 2 and lengths[segment + 1] < target:
            segment += 1
        span = lengths[segment + 1] - lengths[segment]
        factor = 0.0 if span <= 1e-9 else (target - lengths[segment]) / span
        result.append((points[segment].lerp(points[segment + 1], factor),
                       segment))
    return result


def basis(direction):
    """An orthonormal frame with -Z along `direction`."""
    forward = direction.normalized() if direction.length > 1e-9 \
        else Vector((0.0, 0.0, -1.0))
    up = Vector((0.0, 1.0, 0.0))
    if abs(forward.dot(up)) > 0.98:
        up = Vector((0.0, 0.0, 1.0))
    right = forward.cross(up).normalized()
    up = right.cross(forward).normalized()
    return right, up, forward


def add_tube(bm, weights, uvs, points, radii, bone_names, region,
             segments=RING_SEGMENTS, cap_start=True, cap_end=True, along=None):
    """A closed tube through `points`, each ring weighted to its own bone.

    `bone_names` has one entry per point; a ring blends into the next bone over
    the last JOINT_BLEND of its segment so the joint does not crease. `along`
    gives each ring's u inside `region`; by default it is the ring's fraction
    of the tube's length.
    """
    assert len(points) == len(radii) == len(bone_names)
    if along is None:
        lengths = [0.0]
        for index in range(1, len(points)):
            lengths.append(lengths[-1] +
                           (points[index] - points[index - 1]).length)
        total = lengths[-1] or 1.0
        along = [value / total for value in lengths]
    assert len(along) == len(points)
    rings = []
    for index, centre in enumerate(points):
        if index == 0:
            direction = points[1] - points[0]
        elif index == len(points) - 1:
            direction = points[-1] - points[-2]
        else:
            direction = points[index + 1] - points[index - 1]
        right, up, _ = basis(direction)
        ring = []
        for step in range(segments):
            angle = 2.0 * math.pi * step / segments
            offset = right * (math.cos(angle) * radii[index]) + \
                up * (math.sin(angle) * radii[index])
            vert = bm.verts.new(centre + offset)
            ring.append(vert)
            blend = min(JOINT_BLEND, 0.5)
            if index + 1 < len(bone_names) and bone_names[index + 1] != \
                    bone_names[index]:
                weights[vert] = [(bone_names[index], 1.0 - blend),
                                 (bone_names[index + 1], blend)]
            else:
                weights[vert] = [(bone_names[index], 1.0)]
            uvs[vert] = region_uv(region, along[index], fold(angle))
        rings.append(ring)

    for index in range(len(rings) - 1):
        for step in range(segments):
            nxt = (step + 1) % segments
            try:
                bm.faces.new((rings[index][step], rings[index + 1][step],
                              rings[index + 1][nxt], rings[index][nxt]))
            except ValueError:
                pass
    for cap, ring, name in ((cap_start, rings[0], bone_names[0]),
                            (cap_end, rings[-1], bone_names[-1])):
        if not cap:
            continue
        try:
            bm.faces.new(tuple(ring if ring is rings[-1] else reversed(ring)))
        except ValueError:
            pass
    return rings


def add_beak(bm, weights, uvs, root, tip, bone, half_height, half_width,
             region, band, rows=5):
    """A tapering wedge from the skull to a point. Upper and lower beak share
    this shape; only the bone, the tip and the atlas band differ. `band` is
    the (low, high) slice of the region's v the beak paints into, and the
    four sides fold belly-to-top inside it like a tube would."""
    grid = []
    band_low, band_high = band
    for row in range(rows):
        factor = row / float(rows - 1)
        centre = root.lerp(tip, factor)
        taper = (1.0 - factor) ** 0.7
        height = half_height * taper
        width = half_width * taper
        column = []
        for offset, side_fold in (((0.0, height), 1.0), ((width, 0.0), 0.5),
                                  ((0.0, -height), 0.0), ((-width, 0.0), 0.5)):
            vert = bm.verts.new(centre + Vector((offset[0], offset[1], 0.0)))
            weights[vert] = [(bone, 1.0)]
            uvs[vert] = region_uv(region, factor,
                                  band_low + side_fold * (band_high - band_low))
            column.append(vert)
        grid.append(column)
    for row in range(rows - 1):
        for side in range(4):
            nxt = (side + 1) % 4
            try:
                bm.faces.new((grid[row][side], grid[row + 1][side],
                              grid[row + 1][nxt], grid[row][nxt]))
            except ValueError:
                pass
    try:
        bm.faces.new(tuple(reversed(grid[0])))
    except ValueError:
        pass
    return grid


def add_crest(bm, weights, uvs, skull, bone):
    """A swept blade from the back of the skull. Weighted entirely to Head,
    because a crest is bone and should move exactly with the skull."""
    # Read clockwise from the brow: along the skull roof, up the rising upper
    # edge to the tip, then back down the lower edge to the occiput. The tip
    # is the highest point of the whole head; nothing on the crest is above
    # the line from brow to tip. A Pteranodon's crest is the silhouette
    # people recognise it by, so it is deliberately the largest feature on
    # the head, and it is authored as a spike rather than a fin.
    tip = Vector((0.0, skull.y + CREST_TIP_UP, skull.z + CREST_TIP_BACK))
    brow = Vector((0.0, skull.y + 0.05, skull.z - 0.14))
    occiput = Vector((0.0, skull.y + 0.06, skull.z + 0.16))
    profile = [
        brow,
        Vector((0.0, skull.y + 0.19, skull.z - 0.02)),
        brow.lerp(tip, 0.55) + Vector((0.0, 0.10, 0.0)),
        tip,
        occiput.lerp(tip, 0.5) - Vector((0.0, 0.02, 0.0)),
        occiput,
        Vector((0.0, skull.y - 0.02, skull.z + 0.12)),
    ]
    # Base points keep the skull's thickness; the blade thins towards the tip.
    thickness = [1.0, 0.9, 0.45, 0.0, 0.45, 1.0, 1.0]
    # The crest is painted as a side view: u from the brow back to the tip,
    # v from the lower edge up. Both faces share it.
    z_low = min(point.z for point in profile)
    z_high = max(point.z for point in profile)
    y_low = min(point.y for point in profile)
    y_high = max(point.y for point in profile)
    left, right = [], []
    for index, point in enumerate(profile):
        taper = CREST_HALF_WIDTH_TIP + (CREST_HALF_WIDTH_BASE -
                                        CREST_HALF_WIDTH_TIP) * thickness[index]
        vert_left = bm.verts.new(point + Vector((taper, 0.0, 0.0)))
        vert_right = bm.verts.new(point + Vector((-taper, 0.0, 0.0)))
        weights[vert_left] = [(bone, 1.0)]
        weights[vert_right] = [(bone, 1.0)]
        uv = region_uv("crest", (point.z - z_low) / (z_high - z_low),
                       (point.y - y_low) / (y_high - y_low))
        uvs[vert_left] = uv
        uvs[vert_right] = uv
        left.append(vert_left)
        right.append(vert_right)
    for index in range(len(profile) - 1):
        try:
            bm.faces.new((left[index], left[index + 1],
                          right[index + 1], right[index]))
        except ValueError:
            pass
    try:
        bm.faces.new(tuple(left))
        bm.faces.new(tuple(reversed(right)))
    except ValueError:
        pass


def sheet_normal(columns, i, j):
    i0, i1 = max(0, i - 1), min(len(columns) - 1, i + 1)
    j0, j1 = max(0, j - 1), min(len(columns[i]) - 1, j + 1)
    along = columns[i1][j] - columns[i0][j]
    across = columns[i][j1] - columns[i][j0]
    normal = along.cross(across)
    return Vector((0.0, 1.0, 0.0)) if normal.length < 1e-6 \
        else normal.normalized()


def wing_grid(bones, side):
    flip = (side == "R")
    leading_raw = [head(bones, n) for n in LEADING_CHAIN]
    trailing_raw = [head(bones, n) for n in TRAILING_CHAIN]
    if flip:
        leading_raw = [mirror(p) for p in leading_raw]
        trailing_raw = [mirror(p) for p in trailing_raw]

    stations = (len(TRAILING_CHAIN) - 1) * SPAN_SUBDIVISIONS + 1
    sampled = resample(leading_raw, stations)
    leading = [position for position, _ in sampled]
    leading_segment = [segment for _, segment in sampled]

    trailing, trailing_bone_of = [], []
    for index in range(stations):
        position = index / float(SPAN_SUBDIVISIONS)
        low = min(int(math.floor(position)), len(trailing_raw) - 2)
        factor = position - low
        trailing.append(trailing_raw[low].lerp(trailing_raw[low + 1], factor))
        trailing_bone_of.append((low, factor))

    columns, assignments = [], {}
    for i in range(stations):
        span = i / float(stations - 1)
        chord = (trailing[i] - leading[i]).length
        column = []
        for j in range(CHORD_ROWS):
            across = j / float(CHORD_ROWS - 1)
            base = leading[i].lerp(trailing[i], across)
            droop = math.sin(across * math.pi) * chord * SAG * (1.0 - span ** 2)
            column.append(base - Vector((0.0, droop, 0.0)))
            leading_bone = LEADING_BONES[min(leading_segment[i],
                                             len(LEADING_BONES) - 1)]
            low, factor = trailing_bone_of[i]
            near = TRAILING_BONES[low]
            far = TRAILING_BONES[min(low + 1, len(TRAILING_BONES) - 1)]
            if flip:
                leading_bone = "R" + leading_bone[1:]
                near = "R" + near[1:]
                far = "R" + far[1:]
            entries = [(leading_bone, 1.0 - across),
                       (near, across * (1.0 - factor)),
                       (far, across * factor)]
            assignments[(i, j)] = [(n, v) for n, v in entries if v > 1e-4]
        columns.append(column)
    return columns, assignments


def add_membrane(bm, weights, uvs, bones, side):
    columns, assignments = wing_grid(bones, side)
    stations = len(columns)
    sheets = []
    for sign in (1.0, -1.0):
        sheet = []
        for i, column in enumerate(columns):
            verts = []
            for j, position in enumerate(column):
                normal = sheet_normal(columns, i, j)
                vert = bm.verts.new(position + normal * HALF_THICKNESS * sign)
                weights[vert] = assignments[(i, j)]
                # Span along u from the root, chord along v from the leading
                # edge; the upper and lower faces and both wings share it.
                uvs[vert] = region_uv("membrane", i / float(stations - 1),
                                      j / float(CHORD_ROWS - 1))
                verts.append(vert)
            sheet.append(verts)
        sheets.append(sheet)
    for sheet_index, sheet in enumerate(sheets):
        for i in range(len(sheet) - 1):
            for j in range(len(sheet[i]) - 1):
                quad = (sheet[i][j], sheet[i + 1][j],
                        sheet[i + 1][j + 1], sheet[i][j + 1])
                if (sheet_index == 1) != (side == "R"):
                    quad = tuple(reversed(quad))
                try:
                    bm.faces.new(quad)
                except ValueError:
                    pass
    top, bottom = sheets
    for i in range(len(top) - 1):
        for j in (0, len(top[i]) - 1):
            try:
                bm.faces.new((top[i][j], top[i + 1][j],
                              bottom[i + 1][j], bottom[i][j]))
            except ValueError:
                pass


def add_body(bm, weights, uvs, bones):
    lower = head(bones, "LowerTorso")
    upper = head(bones, "UpperTorso")
    neck = head(bones, "Neck")
    skull = head(bones, "Head")
    jaw = head(bones, "Jaw")
    tail = head(bones, "Tail")
    tail_end = head(bones, "Tail_end")
    stub = tail.lerp(tail_end, TAIL_STUB_FRACTION)
    beak_root = skull + Vector((0.0, -0.02, -0.18))

    # The whole body - tail stub, torso, neck and skull - paints along one
    # strip by its position along the creature, tail at u = 0 and the front
    # of the skull at u = 1, so the texture is continuous across the tubes
    # that make it up.
    def body_along(point):
        return (stub.z - point.z) / (stub.z - beak_root.z)

    torso = [lower, upper, neck]
    add_tube(bm, weights, uvs, torso, [0.30, 0.26, 0.15],
             ["LowerTorso", "UpperTorso", "Neck"], "body", cap_start=False,
             along=[body_along(point) for point in torso])
    add_tube(bm, weights, uvs, [neck, skull], [0.15, 0.13], ["Neck", "Head"],
             "body", cap_start=False,
             along=[body_along(neck), body_along(skull)])
    add_tube(bm, weights, uvs, [skull, beak_root], [0.13, 0.10],
             ["Head", "Head"], "body", cap_start=False, cap_end=False,
             along=[body_along(skull), body_along(beak_root)])

    add_beak(bm, weights, uvs, beak_root,
             Vector((0.0, beak_root.y - 0.05, BEAK_TIP_Z)), "Head",
             BEAK_HALF_HEIGHT, BEAK_HALF_WIDTH, "beak", (0.5, 1.0))
    add_beak(bm, weights, uvs, jaw,
             Vector((0.0, jaw.y - 0.03, LOWER_BEAK_TIP_Z)), "Jaw",
             BEAK_HALF_HEIGHT * 0.7, BEAK_HALF_WIDTH * 0.85, "beak", (0.0, 0.5))
    add_crest(bm, weights, uvs, skull, "Head")

    for side in ("L", "R"):
        thigh = head(bones, side + "_Leg0_Upper")
        shank = head(bones, side + "_Leg0_Lower")
        ankle = head(bones, side + "_Foot0")
        add_tube(bm, weights, uvs, [thigh, shank, ankle], [0.10, 0.07, 0.05],
                 [side + "_Leg0_Upper", side + "_Leg0_Lower", side + "_Foot0"],
                 "limbs")
        for toe in range(1, 5):
            chain = ["%s_Finger_%d_1" % (side, toe),
                     "%s_Finger_%d_2" % (side, toe),
                     "%s_Finger_%d_2_end" % (side, toe)]
            points = [head(bones, n) for n in chain]
            add_tube(bm, weights, uvs, points, [0.028, 0.020, 0.010],
                     [chain[0], chain[1], chain[1]], "limbs", segments=5)

    # Start at the hips, not at the Tail bone. The Tail bone's head is 0.25
    # behind the torso's rear cap, so a tube that began there left the stub
    # floating clear of the body - visible immediately in the top view.
    stub_chain = [lower, tail, stub]
    add_tube(bm, weights, uvs, stub_chain, [0.22, 0.12, 0.05],
             ["LowerTorso", "Tail", "Tail"], "body", cap_start=False,
             along=[body_along(point) for point in stub_chain])


def build_armature(rig):
    armature = bpy.data.armatures.new("PteranodonRig")
    obj = bpy.data.objects.new("PteranodonRig", armature)
    bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode="EDIT")
    created = {}
    for bone in sorted(rig["bones"], key=lambda value: value["depth"]):
        edit = armature.edit_bones.new(bone["name"])
        edit.head = Vector(bone["head"])
        edit.tail = Vector(bone["tail"])
        if (edit.tail - edit.head).length < 1e-4:
            edit.tail = edit.head + Vector((0.0, 0.02, 0.0))
        created[bone["name"]] = edit
    for bone in rig["bones"]:
        if bone["parent"] and bone["parent"] in created:
            created[bone["name"]].parent = created[bone["parent"]]
    bpy.ops.object.mode_set(mode="OBJECT")
    return obj


def albedo_manifest(path):
    """What the runtime checks before it uses the albedo: the exact bytes,
    and the dimensions read straight from the PNG header."""
    data = Path(path).read_bytes()
    if data[:8] != bytes((0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)) \
            or data[12:16] != b"IHDR":
        raise SystemExit("albedo is not a PNG: " + str(path))
    return {
        "file": Path(path).name,
        "sha256": hashlib.sha256(data).hexdigest(),
        "width": int.from_bytes(data[16:20], "big"),
        "height": int.from_bytes(data[20:24], "big"),
        "bitDepth": data[24],
        "colorType": data[25],
    }


def attach_preview_material(obj, mesh, albedo):
    """A material for local review renders only: the runtime builds its own
    from the donor's, and nothing about this one ships."""
    material = bpy.data.materials.new("PteranodonAlbedoPreview")
    material.use_nodes = True
    nodes = material.node_tree.nodes
    principled = nodes.get("Principled BSDF")
    image_node = nodes.new("ShaderNodeTexImage")
    image_node.image = bpy.data.images.load(str(Path(albedo).resolve()))
    image_node.image.colorspace_settings.name = "sRGB"
    material.node_tree.links.new(image_node.outputs["Color"],
                                 principled.inputs["Base Color"])
    principled.inputs["Roughness"].default_value = 0.75
    mesh.materials.append(material)


def write_mesh_data(path, obj, mesh, weights, uvs, report, albedo):
    """Emit the mesh as data the runtime loader can build a Mesh from.

    This is the shipped path, and the AssetBundle is the alternative rather than
    the other way round. Three reasons, in order of weight:

    - It carries strictly less. The bundle format would embed bind poses, a
      material and import settings; this carries our vertices, our normals, our
      texture coordinates, our triangles, our weights, and the donor's bone
      NAMES. Names are the binding contract and are already recorded in the
      native audit; no donor transform leaves the machine.
    - It does not depend on a Unity editor licence or a specific editor version.
      That dependency is not hypothetical: the 2018.4.10f1 install that built
      every previous bundle stopped accepting its licence between 2026-08-21 and
      2026-09-23 and now demands account credentials to re-activate.
    - It is less code than the bundle path on both sides.

    Triangles are emitted with a flipped winding. Blender is right-handed with
    +Z up; the donor renderer's space is left-handed with +Y up, which is the
    space these vertices are authored in, so the handedness difference shows up
    as inside-out faces unless the winding is reversed here.

    The albedo is referenced by name, exact hash and header dimensions; the
    runtime refuses the mesh if the texture beside it is not that file.
    """
    import struct

    # Blender 4.5 computes vertex normals itself; calc_normals_split was
    # removed. Per-vertex normals are what a skinned mesh needs anyway.
    vertices, normals, coords, triangles = [], [], [], []
    for vertex in mesh.vertices:
        vertices.append((vertex.co.x, vertex.co.y, vertex.co.z))
        normals.append((vertex.normal.x, vertex.normal.y, vertex.normal.z))
        if vertex.index not in uvs:
            raise SystemExit("vertex %d has no texture coordinate" % vertex.index)
        coords.append(uvs[vertex.index])
    for polygon in mesh.polygons:
        loop = list(polygon.vertices)
        for corner in range(1, len(loop) - 1):
            triangles.extend((loop[0], loop[corner + 1], loop[corner]))

    order = sorted({name for entries in weights.values()
                    for name, _ in entries})
    index_of = {name: index for index, name in enumerate(order)}
    bone_weights = []
    for index in range(len(mesh.vertices)):
        entries = sorted(weights.get(index, []), key=lambda e: -e[1])[:4]
        total = sum(value for _, value in entries) or 1.0
        slots = [(index_of[name], value / total) for name, value in entries]
        while len(slots) < 4:
            slots.append((0, 0.0))
        bone_weights.append(slots)

    payload = {
        "schemaVersion": 2,
        "space": "donor renderer local; +X left, +Y up, -Z forward",
        "rigSha256": report["rigSha256"],
        "bones": order,
        "uvAtlas": {name: list(region) for name, region in ATLAS.items()},
        "albedo": albedo_manifest(albedo),
        "vertexCount": len(vertices),
        "triangleCount": len(triangles) // 3,
    }
    blob = bytearray()
    for value in vertices:
        blob += struct.pack("<3f", *value)
    for value in normals:
        blob += struct.pack("<3f", *value)
    for value in coords:
        blob += struct.pack("<2f", *value)
    for value in triangles:
        blob += struct.pack("<i", value)
    for slots in bone_weights:
        for bone_index, weight in slots:
            blob += struct.pack("<if", bone_index, weight)

    import base64
    payload["data"] = base64.b64encode(bytes(blob)).decode("ascii")
    with open(path, "w", encoding="utf-8", newline=chr(10)) as handle:
        json.dump(payload, handle, indent=1)


def main():
    args = parse_args()
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig, bones = load_rig(args.rig)
    armature = build_armature(rig)

    mesh = bpy.data.meshes.new("PteranodonMesh")
    obj = bpy.data.objects.new("Pteranodon", mesh)
    bpy.context.collection.objects.link(obj)
    bm = bmesh.new()
    weights = {}
    uvs = {}
    if args.parts in ("all", "body"):
        add_body(bm, weights, uvs, bones)
    if args.parts in ("all", "membrane"):
        for side in ("L", "R"):
            add_membrane(bm, weights, uvs, bones, side)
    bm.normal_update()
    bm.to_mesh(mesh)
    indexed = {}
    for vert, entries in weights.items():
        indexed[vert.index] = entries
    uv_indexed = {}
    for vert, uv in uvs.items():
        uv_indexed[vert.index] = uv
    bm.free()

    # The same coordinates on the Blender mesh, so the .blend and the .fbx
    # carry them and a review render can show the painted creature.
    layer = mesh.uv_layers.new(name="Albedo")
    for loop in mesh.loops:
        layer.data[loop.index].uv = uv_indexed[loop.vertex_index]

    groups = {}
    for entries in indexed.values():
        for name, _ in entries:
            if name not in groups:
                groups[name] = obj.vertex_groups.new(name=name)
    for index, entries in indexed.items():
        total = sum(value for _, value in entries) or 1.0
        for name, value in entries:
            groups[name].add([index], value / total, "REPLACE")
    modifier = obj.modifiers.new(name="Armature", type="ARMATURE")
    modifier.object = armature
    obj.parent = armature
    if args.albedo:
        attach_preview_material(obj, mesh, args.albedo)

    xs = [v.co.x for v in mesh.vertices]
    ys = [v.co.y for v in mesh.vertices]
    zs = [v.co.z for v in mesh.vertices]
    report = {
        "schemaVersion": 3,
        "parts": args.parts,
        "rigSource": rig.get("source"),
        "rigSpace": rig.get("space"),
        "rigSha256": hashlib.sha256(Path(args.rig).read_bytes()).hexdigest(),
        "rigBoneCount": len(rig["bones"]),
        "vertices": len(mesh.vertices),
        "polygons": len(mesh.polygons),
        "boneGroups": len(obj.vertex_groups),
        "maxInfluencesPerVertex": max(len(e) for e in indexed.values()),
        "extent": {"x": [min(xs), max(xs)], "y": [min(ys), max(ys)],
                   "z": [min(zs), max(zs)]},
        "span": max(xs) - min(xs),
        "length": max(zs) - min(zs),
        "spanToLength": (max(xs) - min(xs)) / (max(zs) - min(zs)),
        "uvAtlas": {name: list(region) for name, region in ATLAS.items()},
        "albedo": albedo_manifest(args.albedo) if args.albedo else None,
    }
    log(json.dumps(report, indent=1))
    if report["maxInfluencesPerVertex"] > 4:
        raise SystemExit("Unity allows at most four bone influences per vertex")
    if args.report:
        with open(args.report, "w", encoding="utf-8", newline="\n") as handle:
            json.dump(report, handle, indent=1)

    if args.mesh_data:
        write_mesh_data(args.mesh_data, obj, mesh, indexed, uv_indexed, report,
                        args.albedo)
        log("wrote " + args.mesh_data)

    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.export_scene.fbx(
        filepath=args.out, use_selection=True, add_leaf_bones=False,
        bake_anim=False, path_mode="COPY", apply_scale_options="FBX_SCALE_ALL",
        object_types={"ARMATURE", "MESH"})
    log("wrote " + args.out)
    if args.blend_out:
        bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
        log("wrote " + args.blend_out)


if __name__ == "__main__":
    main()
