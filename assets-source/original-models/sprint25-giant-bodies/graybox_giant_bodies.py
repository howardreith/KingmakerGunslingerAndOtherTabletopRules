"""Low-detail motion prototype for the Sprint 25 giant bodies.

This is a graybox, not final geometry. Its whole purpose is to answer one
measured question before any polish: the donor's seven-joint forward chain is a
TRUNK - it is parented to Head and at rest it hangs almost straight down,
dropping 3.82 units over 4.57 units of articulated length - and Brachiosaurus
needs a long neck that rises. Authoring a neck onto that chain means re-aiming
every joint roughly 180 degrees from its bind pose, and the native clips that
drive it (idle, walk, run, gore_attack, slam_attack) were authored to swing a
hanging trunk.

So this builds both bodies on the real captured bind frames and renders three
states: the chain at its measured rest, the chain re-aimed upright for a
sauropod neck, and the same upright neck under a trunk-like curl. The third is
a GEOMETRIC PROBE of the authored rig, deliberately labelled as such - it is
not native motion evidence, because the census captured clip names but no
animation curves.

Run:
  blender --background --python-exit-code 1 --python graybox_giant_bodies.py \
      -- --census <sprint25-donor-census.json> --out <dir>
"""
import argparse
import json
import math
from pathlib import Path
import sys

import bpy
import bmesh
from mathutils import Matrix, Quaternion, Vector

sys.dont_write_bytecode = True
ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "pteranodon"))
sys.path.insert(0, str(ROOT / "sprint12-quadrupeds"))
import generate_pteranodon as shared          # noqa: E402
from generate_sprint12_quadrupeds import ellipsoid, tube   # noqa: E402

# The measured chains. Classification is spatial, from the captured bind
# frames, and must not be re-derived from these names.
FORWARD_CHAIN = ["Tail7_M", "Tail11_M", "Tail21_M", "Tail31_M", "Tail41_M",
                 "Tail5_M", "Tail6_M"]
REAR_CHAIN = ["Tail0_M", "Tail1_M", "Tail2_M", "Tail3_M", "Tail4_M"]
LEG_BONES = ["L_Arm_Upper", "L_Arm_Lower", "L_Palm",
             "R_Arm_Upper", "R_Arm_Lower", "R_Palm",
             "L_Leg0_Upper", "L_Leg0_Lower", "L_Foot0",
             "R_Leg0_Upper", "R_Leg0_Lower", "R_Foot0"]

# The shared five-slot atlas has fixed names, and paint_giant_body.py already
# assigns each slot a role per creature. The graybox uses the same assignment
# so a textured render later means what the albedo intends.
REGIONS = {
    "brachiosaurus": {"forward": "membrane", "head": "beak",
                      "tusk": "beak", "tail": "crest",
                      "limb": "limbs", "body": "body"},
    "elephant": {"forward": "crest", "head": "membrane",
                 "tusk": "beak", "tail": "membrane",
                 "limb": "limbs", "body": "body"},
}


def point_camera(camera, target):
    """The released review scripts' helper, unchanged: Blender Z-up."""
    back = (camera.location - target).normalized()
    up = Vector((0, 0, 1)) if abs(back.z) < .98 else Vector((0, 1, 0))
    right = up.cross(back).normalized()
    up = back.cross(right).normalized()
    camera.rotation_euler = Matrix((right, up, back)).transposed().to_euler()


def load_rest(census_path):
    """The renderer-local BIND frame, which is the space every released body in
    this project is authored in - not the Unity world positions, which carry an
    arbitrary scene placement.

    Measured in that frame: X is lateral, Y is up with the feet at y = 0, and
    -Z is forward (Head sits at z = -2.0, the hips at z = +1.97). Blender is
    Z-up, so each point is rotated +90 degrees about X, which preserves
    handedness and leaves the creature facing +Y with its feet on z = 0.
    """
    doc = json.loads(Path(census_path).read_text(encoding="utf-8"))
    renderer = doc["donors"][0]["skinnedRenderers"][0]
    out = {}
    for bone in renderer["bones"]:
        x, y, z = bone["bindPosition"]
        out[bone["name"]] = Vector((x, -z, y))
    return out


# Authoring axes are now fixed by the conversion above rather than derived.
LATERAL = Vector((1.0, 0.0, 0.0))
FORWARD = Vector((0.0, 1.0, 0.0))
UP = Vector((0.0, 0.0, 1.0))


def frame(rest):
    return LATERAL, UP, FORWARD


def chain_points(rest, names, root):
    return [rest[root]] + [rest[n] for n in names]


def reaim(points, pivot, target_dir):
    """Rigidly swing a chain so its overall direction becomes target_dir,
    preserving every segment length and the chain's internal curvature. This
    is exactly what re-aiming the bind pose does: it does not invent joints."""
    current = (points[-1] - points[0]).normalized()
    target = Vector(target_dir).normalized()
    axis = current.cross(target)
    if axis.length < 1e-6:
        return list(points)
    rotation = Quaternion(axis.normalized(),
                          math.acos(max(-1.0, min(1.0, current.dot(target)))))
    return [pivot + rotation @ (p - pivot) for p in points]


def curl(points, pivot, axis, degrees_per_joint):
    """Progressive per-joint rotation, the way a trunk curls. A geometric
    probe of the authored rig, not a replay of a native clip."""
    out = [points[0]]
    accumulated = Quaternion(Vector(axis).normalized(), 0.0)
    for index in range(1, len(points)):
        accumulated = (Quaternion(Vector(axis).normalized(),
                                  math.radians(degrees_per_joint))
                       @ accumulated)
        pivot_point = out[index - 1]
        segment = points[index] - points[index - 1]
        out.append(pivot_point + accumulated @ segment)
    return out


def taper(count, base, tip):
    return [base + (tip - base) * (i / max(1, count - 1)) for i in range(count)]


def build(kind, rest, neck_state):
    """One graybox body. neck_state is 'rest', 'upright' or 'curled'."""
    lateral, up, forward = frame(rest)
    mesh = bpy.data.meshes.new(kind + "_graybox")
    obj = bpy.data.objects.new(kind + "_graybox", mesh)
    bpy.context.scene.collection.objects.link(obj)
    bm = bmesh.new()
    weights, uvs = {}, {}

    torso_mid = (rest["LowerTorso"] + rest["UpperTorso"]) * 0.5
    body_len = (rest["UpperTorso"] - rest["LowerTorso"]).length

    if kind == "brachiosaurus":
        body_radii = (body_len * 0.42, body_len * 0.46, body_len * 0.72)
        neck_base, neck_tip = body_len * 0.20, body_len * 0.085
    else:
        body_radii = (body_len * 0.46, body_len * 0.44, body_len * 0.66)
        neck_base, neck_tip = body_len * 0.17, body_len * 0.055

    slots = REGIONS[kind]
    ellipsoid(bm, weights, uvs, torso_mid, lateral, up, forward,
              body_radii, "UpperTorso", region=slots["body"])

    # The forward chain. Rest is what was measured; upright is the sauropod
    # re-aim; curled is the probe.
    pts = chain_points(rest, FORWARD_CHAIN, "Head")
    if neck_state in ("upright", "curled"):
        pts = reaim(pts, pts[0], up * 0.82 + forward * 0.57)
    if neck_state == "curled":
        pts = curl(pts, pts[0], lateral, 16.0)
    bones = ["Head"] + FORWARD_CHAIN
    tube(bm, weights, uvs, pts, taper(len(pts), neck_base, neck_tip),
         bones, region=slots["forward"], segments=10)

    # Where the skull sits is the one thing these two creatures do NOT share.
    # A sauropod's head is at the far end of the neck, so it rides the chain
    # tip and a wrong skull angle shows there. An elephant's head is at the
    # BASE: the trunk descends from it, so the head belongs at the Head joint
    # and only the trunk tip hangs free. Putting the elephant's head at the
    # chain tip is what made the first render read as a limb with a foot.
    head_size = body_len * (0.115 if kind == "brachiosaurus" else 0.26)
    if kind == "brachiosaurus":
        head_at, aim_from, aim_to = pts[-1], pts[-2], pts[-1]
    else:
        head_at, aim_from, aim_to = pts[0], rest["UpperTorso"], pts[0]
    head_dir = (aim_to - aim_from).normalized()
    head_up = (up - head_dir * up.dot(head_dir)).normalized()
    head_lat = head_dir.cross(head_up).normalized()
    ellipsoid(bm, weights, uvs, head_at + head_dir * head_size * 0.55,
              head_lat, head_up, head_dir,
              (head_size * 0.70, head_size * 0.78, head_size),
              "Head" if kind == "elephant" else FORWARD_CHAIN[-1],
              region=slots["head"])

    # The elephant's ears and tusks, because tusk clearance against the trunk
    # is one of the things this prototype has to show.
    if kind == "elephant":
        for ear in ("Ear_L", "Ear_R"):
            if ear not in rest:
                continue
            side = (rest[ear] - pts[0])
            ellipsoid(bm, weights, uvs, rest[ear] + side * 0.45,
                      head_lat, head_up, head_dir,
                      (body_len * 0.055, body_len * 0.21, body_len * 0.16),
                      ear, region=slots["head"])
        for sign in (-1.0, 1.0):
            root = pts[0] + lateral * (sign * body_len * 0.105)                 - up * body_len * 0.055
            tip = root + head_dir * body_len * 0.40                 - up * body_len * 0.10 + lateral * (sign * body_len * 0.045)
            tube(bm, weights, uvs, [root, (root + tip) * 0.5, tip],
                 [body_len * 0.035, body_len * 0.026, body_len * 0.012],
                 ["Head", "Head", "Head"], region=slots["tusk"], segments=6)

    # The real tail keeps the rear chain; the neck never borrows it.
    tail_pts = chain_points(rest, REAR_CHAIN, "LowerTorso")
    tube(bm, weights, uvs, tail_pts,
         taper(len(tail_pts), body_len * 0.15, body_len * 0.035),
         ["LowerTorso"] + REAR_CHAIN, region=slots["tail"], segments=8)

    # Four columnar limbs from the measured joints.
    for upper_b, lower_b, foot_b in (
            ("L_Arm_Upper", "L_Arm_Lower", "L_Palm"),
            ("R_Arm_Upper", "R_Arm_Lower", "R_Palm"),
            ("L_Leg0_Upper", "L_Leg0_Lower", "L_Foot0"),
            ("R_Leg0_Upper", "R_Leg0_Lower", "R_Foot0")):
        if not all(b in rest for b in (upper_b, lower_b, foot_b)):
            continue
        top, knee, foot = rest[upper_b], rest[lower_b], rest[foot_b]
        ground = foot - up * (body_len * 0.06)
        radius = body_len * (0.115 if kind == "brachiosaurus" else 0.125)
        tube(bm, weights, uvs, [top, knee, foot, ground],
             [radius, radius * 0.84, radius * 0.78, radius * 0.86],
             [upper_b, lower_b, foot_b, foot_b], region=slots["limb"],
             segments=8)

    bm.to_mesh(mesh)
    bm.free()
    mesh.validate()
    return obj


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--census", required=True)
    parser.add_argument("--out", required=True)
    parser.add_argument("--kind", default="both")
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    args = parser.parse_args(argv)

    rest = load_rest(args.census)
    out = Path(args.out)
    out.mkdir(parents=True, exist_ok=True)

    scene = bpy.context.scene
    for obj in list(bpy.data.objects):
        bpy.data.objects.remove(obj, do_unlink=True)

    camera_data = bpy.data.cameras.new("ReviewCamera")
    camera = bpy.data.objects.new("ReviewCamera", camera_data)
    scene.collection.objects.link(camera)
    scene.camera = camera
    camera_data.type = "ORTHO"
    scene.render.resolution_x = 1000
    scene.render.resolution_y = 720
    scene.render.image_settings.file_format = "PNG"
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    scene.render.film_transparent = False
    world = bpy.data.worlds.new("ReviewWorld")
    scene.world = world
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[1].default_value = 1.6

    light_data = bpy.data.lights.new("Key", type="SUN")
    light_data.energy = 3.0
    light = bpy.data.objects.new("Key", light_data)
    light.rotation_euler = (math.radians(55), 0.0, math.radians(35))
    scene.collection.objects.link(light)

    kinds = ("brachiosaurus", "elephant") if args.kind == "both" else (args.kind,)
    states = {"brachiosaurus": ("rest", "upright", "curled"),
              "elephant": ("rest", "curled")}
    written = []
    for kind in kinds:
        for state in states[kind]:
            for obj in [o for o in bpy.data.objects
                        if o.name.endswith("_graybox")]:
                bpy.data.objects.remove(obj, do_unlink=True)
            body = build(kind, rest, state)
            centre = sum((Vector(c) for c in body.bound_box),
                         Vector()) / 8.0
            centre = body.matrix_world @ centre
            extent = max(body.dimensions) * 0.78

            for view, offset in (("side", Vector((1.0, 0.0, 0.12))),
                                 ("front", Vector((0.0, -1.0, 0.12))),
                                 ("rear", Vector((0.0, 1.0, 0.12))),
                                 ("party", Vector((0.72, -0.72, 0.55)))):
                camera.location = centre + offset.normalized() * extent * 4.0
                point_camera(camera, centre)
                camera_data.ortho_scale = extent * 2.6
                name = "%s-%s-%s.png" % (kind, state, view)
                scene.render.filepath = str(out / name)
                bpy.ops.render.render(write_still=True)
                written.append(name)
    print("GRAYBOX RENDERS: " + ", ".join(written))


if __name__ == "__main__":
    main()
