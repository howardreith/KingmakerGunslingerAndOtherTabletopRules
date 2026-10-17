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
import collections
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

# What build() hands back. The shape alone was enough while this script only
# rendered static states; a weighted prototype needs the influences, the
# texture coordinates and the measured landmarks as well, so the binding step
# consumes exactly the geometry that was rendered rather than a second copy of
# the body definition that could drift from it.
BuiltBody = collections.namedtuple(
    "BuiltBody", "obj weights uvs landmarks anchorRing parts")

# The measured chains. Classification is spatial, from the captured bind
# frames, and must not be re-derived from these names.
FORWARD_CHAIN = ["Tail7_M", "Tail11_M", "Tail21_M", "Tail31_M", "Tail41_M",
                 "Tail5_M", "Tail6_M"]
REAR_CHAIN = ["Tail0_M", "Tail1_M", "Tail2_M", "Tail3_M", "Tail4_M"]
# The forward chain's ring resolution, named once because the binding step
# has to know how many vertices the anchor ring holds.
FORWARD_SEGMENTS = 10
LIMB_SEGMENTS = 8

# How many extra rings the "graded" seam treatment puts across a seam. Two is
# the smallest count that gives the step more than one band to happen in.
SEAM_RINGS = 2

# The parts that partition the body. Anything else build() records is an
# overlay: a named subset of one of these, for measurement.
PRIMARY_PARTS = ("torso", "shoulder", "forward", "skull", "ears", "tusks",
                 "tail", "limbs")

LEG_BONES = ["L_Arm_Upper", "L_Arm_Lower", "L_Palm",
             "R_Arm_Upper", "R_Arm_Lower", "R_Palm",
             "L_Leg0_Upper", "L_Leg0_Lower", "L_Foot0",
             "R_Leg0_Upper", "R_Leg0_Lower", "R_Foot0"]

# Each limb as the donor actually articulates it, hip to sole. The two middle
# joints are the ones the first pass skipped: the donor bends there, and skin
# that no ring weights cannot follow it.
LIMB_CHAINS = (
    ("L_Arm_Upper", "L_Arm_Lower", "frontKnee_L", "frontAnkle_L", "L_Palm"),
    ("R_Arm_Upper", "R_Arm_Lower", "frontKnee_R", "frontAnkle_R", "R_Palm"),
    ("L_Leg0_Upper", "L_Leg0_Lower", "backKnee_L", "backAnkle_L", "L_Foot0"),
    ("R_Leg0_Upper", "R_Leg0_Lower", "backKnee_R", "backAnkle_R", "R_Foot0"),
)

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


def taper_along(points, base, tip):
    """Radii that fall with ARC LENGTH rather than with ring index.

    The donor's forward chain is not evenly spaced: Head -> Tail7_M is 1.754
    units and every joint after it is 0.44 to 0.54. Tapering by index spends
    almost no narrowing over the first 38 per cent of the length and all of it
    over the short distal joints, which is what made the trunk read as a
    constant-width column with a hook on the end - and what makes a uniform
    per-joint curl look like a rigid boom plus a wrist flick, because the bend
    per unit length is 9 degrees at the base against 36 distally.
    """
    lengths = [0.0]
    for index in range(1, len(points)):
        lengths.append(lengths[-1]
                       + (points[index] - points[index - 1]).length)
    total = lengths[-1] or 1.0
    return [base + (tip - base) * (value / total) for value in lengths]


def insert_seam(points, radii, count):
    """Subdivide the first segment of a tube `count` times.

    The seam between a part and the body it leaves is one segment long by
    default, so a single band of faces has to absorb the whole step from
    torso-driven skin to part-driven skin. These rings give that step somewhere
    to happen gradually. They add no shape: every inserted ring lies on the
    segment it subdivides, with its radius interpolated.
    """
    if count < 1:
        return points, radii
    head, tail = points[0], points[1]
    inserted, inserted_radii = [], []
    for step in range(1, count + 1):
        share = step / float(count + 1)
        inserted.append(head + (tail - head) * share)
        inserted_radii.append(radii[0] + (radii[1] - radii[0]) * share)
    return ([head] + inserted + points[1:],
            [radii[0]] + inserted_radii + radii[1:])


def build(kind, rest, neck_state, seam="mixed"):
    """One graybox body. neck_state is 'rest', 'upright' or 'curled'.

    `seam` is how the neck anchor and the limb sockets join the body, and it
    is a parameter rather than a decision buried in the builder because the
    weighted prototype measures all three:

      "blend"   add_tube's default, a quarter of each seam ring on the part it
                feeds - which lets a slam carry the socket out of the torso;
      "pin"     the seam ring driven by the torso alone - which keeps it in the
                body and makes the one band behind it absorb the whole step;
      "graded"  extra rings across BOTH seams with the influence graded
                over them, so no single band takes the step;
      "mixed"   graded limb sockets and a pinned neck anchor. This is what
                the measurements chose, and the two halves of it disagree for
                a reason: a limb socket spans a real transition and wants room
                for it, while the neck anchor is a stub sunk inside the torso
                whose far ring already sits on the pivot, so extra rings there
                only shorten the band that has to absorb the step;
      "chained" the same extra rings, but graded along the donor's OWN
                joints across the seam - for the neck that is
                UpperTorso -> Neck -> Head, and the middle one is the point:
                the donor has a Neck joint, the first pass weighted nothing to
                it, and a gore swing of that joint tore a wedge at the
                shoulder. It was measured and it LOST - elephant gore 1.39 ->
                2.01, trunk-curl floor 0.81 -> 0.31 - for the same reason
                grading the neck lost: inserting rings shortens the bands, so
                the same step across a third of the length reads as a larger
                stretch. The wedge is not a weighting defect at all. It is a
                cylinder leaving a sphere with no shoulder mass between them,
                and it belongs to the geometry pass.
    """
    lateral, up, forward = frame(rest)
    mesh = bpy.data.meshes.new(kind + "_graybox")
    obj = bpy.data.objects.new(kind + "_graybox", mesh)
    bpy.context.scene.collection.objects.link(obj)
    bm = bmesh.new()
    weights, uvs = {}, {}
    parts = {}

    def mark(name, start):
        """Record which vertices each part owns.

        The binding step has to measure one part against another - tusk
        against trunk, skull against torso - and texture coordinates cannot
        tell them apart, because the atlas regions share edges and a pole
        vertex lands exactly on one. The builder already knows, so it says.
        """
        parts.setdefault(name, []).extend(range(start, len(bm.verts)))

    def pin(indices, bone):
        """Drive a ring from one bone alone.

        A ring that lies on the torso's surface - the neck's anchor, a limb's
        socket - is the TORSO's skin, not the neck's or the limb's. The ring
        blend that add_tube applies by default left a quarter of each on the
        part it feeds, and the weighted prototype measured what that costs: the
        gore pose pulled the anchor ring out of the torso hull, and a slam
        stretched a shoulder seam to 1.68 of its rest length. Pinned to the
        torso, those rings cannot move away from the body at all.
        """
        if seam == "blend":
            return
        bm.verts.ensure_lookup_table()
        for index in indices:
            weights[bm.verts[index]] = [(bone, 1.0)]

    def grade(rings, chain, enabled):
        """Spread influence along `chain` across the rings.

        `chain` is the bones the seam actually crosses, body end first. For a
        limb that is the torso and the limb's own bone; for the neck it is
        UpperTorso, Neck and Head. The end rings are left as they were - ring
        zero pinned to the body, the last ring as add_tube weighted it - so
        this only fills in the steps between them.
        """
        if not enabled or len(rings) < 3 or len(chain) < 2:
            return
        bm.verts.ensure_lookup_table()
        for position in range(1, len(rings) - 1):
            place = position / float(len(rings) - 1) * (len(chain) - 1)
            lower = min(int(place), len(chain) - 2)
            share = place - lower
            rows = [(chain[lower], 1.0 - share), (chain[lower + 1], share)]
            rows = [row for row in rows if row[1] > 1e-4]
            for index in rings[position]:
                weights[bm.verts[index]] = rows

    torso_mid = (rest["LowerTorso"] + rest["UpperTorso"]) * 0.5
    body_len = (rest["UpperTorso"] - rest["LowerTorso"]).length

    if kind == "brachiosaurus":
        body_radii = (body_len * 0.42, body_len * 0.46, body_len * 0.72)
        neck_base, neck_tip = body_len * 0.22, body_len * 0.055
    else:
        body_radii = (body_len * 0.46, body_len * 0.44, body_len * 0.66)
        neck_base, neck_tip = body_len * 0.17, body_len * 0.055

    slots = REGIONS[kind]
    start = len(bm.verts)
    ellipsoid(bm, weights, uvs, torso_mid, lateral, up, forward,
              body_radii, "UpperTorso", region=slots["body"])
    mark("torso", start)

    # The forward chain. Three references are kept apart on purpose, because
    # conflating them is what put the elephant's skull, ears and tusks on a
    # point inside the torso:
    #   head_joint  - the ANATOMICAL reference, the captured Head position.
    #                 Everything that belongs to the head hangs off this and
    #                 it never moves.
    #   pts[0]      - a seam-overlap ANCHOR sunk inside the hull so the tube
    #                 starts under the surface. It is not anatomy.
    #   the sink on the skull below is a deliberate ARTISTIC offset.
    #
    # States: 'rest' is what was measured. The upright re-aim belongs to the
    # sauropod neck only - an elephant's trunk must never be stood upright -
    # and the curl is a probe that can apply to either.
    pts = chain_points(rest, FORWARD_CHAIN, "Head")
    if neck_state in ("upright", "upright-curled"):
        pts = reaim(pts, pts[0], up * 0.82 + forward * 0.57)
    if neck_state in ("curled", "upright-curled"):
        pts = curl(pts, pts[0], lateral, 16.0)
    head_joint = pts[0]
    bones = ["Head"] + FORWARD_CHAIN

    # The review measured 18 to 52 pixels of background between torso and
    # chain: a free-floating island, because the chain began exactly at the
    # Head joint, which sits ON the torso surface rather than inside it. The
    # anchor sinks toward the torso CENTRE - sinking backward along the chain
    # instead put it outside the hull on the far side and hung a slab down the
    # flank - and it is weighted to UpperTorso.
    # The sink is solved against the hull, not guessed. At the guessed 1.35
    # the independent review found the ring's outermost vertex 1.008
    # (Brachiosaurus) and 1.145 (Elephant) of a hull radius OUTSIDE the torso,
    # and saw it as a flat cap and a protruding box on the chest of both
    # creatures. The ring is a disc, so the test has to be on the disc rather
    # than on its centre: this takes the smallest sink that puts every vertex
    # of it inside, with margin.
    toward = (torso_mid - head_joint).normalized()
    anchor_radius = neck_base * 1.22

    def hull_value(point):
        offset = point - torso_mid
        return ((offset.dot(lateral) / body_radii[0]) ** 2 +
                (offset.dot(up) / body_radii[1]) ** 2 +
                (offset.dot(forward) / body_radii[2]) ** 2)

    anchor = None
    for step in range(10, 101):
        trial = head_joint + toward * (neck_base * step / 10.0)
        right, lift, _ = shared.basis(head_joint - trial)
        ring = [trial + right * (math.cos(angle) * anchor_radius)
                + lift * (math.sin(angle) * anchor_radius)
                for angle in (2.0 * math.pi * index / FORWARD_SEGMENTS
                              for index in range(FORWARD_SEGMENTS))]
        if max(hull_value(point) for point in ring) <= 0.85:
            anchor = trial
            break
    if anchor is None:
        raise SystemExit("no anchor sink puts the whole seam ring inside the "
                         "torso hull; the torso is too small for this neck")
    pts = [anchor] + pts
    bones = ["UpperTorso"] + bones
    radii = [anchor_radius] + taper_along(pts[1:], neck_base, neck_tip)
    extra = SEAM_RINGS if seam in ("graded", "chained") else 0
    pts, radii = insert_seam(pts, radii, extra)
    bones = [bones[0]] * extra + bones
    # The anchor ring is the first ring this call creates, so its vertices are
    # the first FORWARD_SEGMENTS the bmesh hands out here. Recording the range
    # is how the binding step can later prove, per pose, that the ring the
    # reviewer saw floating is still inside the torso hull.
    start = len(bm.verts)
    seam_rings = [list(range(start + index * FORWARD_SEGMENTS,
                             start + (index + 1) * FORWARD_SEGMENTS))
                  for index in range(extra + 2)]
    anchor_ring = seam_rings[0]
    tube(bm, weights, uvs, pts, radii,
         bones, region=slots["forward"], segments=FORWARD_SEGMENTS)
    mark("forward", start)
    pin(anchor_ring, "UpperTorso")
    # "graded" steps straight from the torso to the head, which is the
    # comparison; "chained" goes through the Neck joint the donor actually
    # has between them.
    grade(seam_rings,
          ["UpperTorso", "Neck", "Head"] if seam == "chained"
          else ["UpperTorso", "Head"],
          seam in ("graded", "chained"))

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
        head_at, aim_from, aim_to = head_joint, rest["UpperTorso"], head_joint
    head_dir = (aim_to - aim_from).normalized()
    head_up = (up - head_dir * up.dot(head_dir)).normalized()
    head_lat = head_dir.cross(head_up).normalized()
    # The review found the skull separated from the neck tip under curl and
    # off-axis even uncurled: it was pushed OUTWARD past the final cap. It now
    # sinks back into the last segment so the join stays closed at any curl.
    start = len(bm.verts)
    ellipsoid(bm, weights, uvs, head_at - head_dir * head_size * 0.30,
              head_lat, head_up, head_dir,
              (head_size * 0.70, head_size * 0.78, head_size),
              "Head" if kind == "elephant" else FORWARD_CHAIN[-1],
              region=slots["head"])
    mark("skull", start)

    # The elephant's ears and tusks, because tusk clearance against the trunk
    # is one of the things this prototype has to show.
    if kind == "elephant":
        # The review measured these projecting upward above the skull as two
        # narrow spikes, which read as horns. The cause was offsetting along
        # (ear - headBase), which on this rig points mostly UP. An ear is a
        # flat fan hung BESIDE the skull, so it is offset along lateral only,
        # dropped to skull height, and swept back.
        for ear, sign in (("Ear_L", 1.0), ("Ear_R", -1.0)):
            if ear not in rest:
                continue
            centre = (head_joint
                      + lateral * (sign * body_len * 0.20)
                      - up * (body_len * 0.02)
                      - forward * (body_len * 0.10))
            start = len(bm.verts)
            ellipsoid(bm, weights, uvs, centre, lateral, up, forward,
                      (body_len * 0.035, body_len * 0.19, body_len * 0.21),
                      ear, region=slots["head"])
            mark("ears", start)
        for sign in (-1.0, 1.0):
            root = head_joint + lateral * (sign * body_len * 0.105)                 - up * body_len * 0.055
            tip = root + head_dir * body_len * 0.40                 - up * body_len * 0.10 + lateral * (sign * body_len * 0.045)
            start = len(bm.verts)
            tube(bm, weights, uvs, [root, (root + tip) * 0.5, tip],
                 [body_len * 0.035, body_len * 0.026, body_len * 0.012],
                 ["Head", "Head", "Head"], region=slots["tusk"], segments=6)
            mark("tusks", start)

    # The shoulder. The wedge where the neck leaves the body survived every
    # seam weighting because no weighting can put mass where there is none:
    # a cylinder meets a sphere with nothing between them. The donor has a
    # `Neck` joint sitting between UpperTorso and Head, and until now nothing
    # in this body weighted it at all. It carries the shoulder, so the mass
    # bridging that join is also the first thing here that deforms with the
    # joint the gore pose swings.
    if "Neck" in rest:
        start = len(bm.verts)
        shoulder_dir = (head_joint - torso_mid).normalized()
        shoulder_up = (up - shoulder_dir * up.dot(shoulder_dir)).normalized()
        shoulder_lat = shoulder_dir.cross(shoulder_up).normalized()
        ellipsoid(bm, weights, uvs, rest["Neck"],
                  shoulder_lat, shoulder_up, shoulder_dir,
                  (neck_base * 1.30, neck_base * 1.34, neck_base * 1.46),
                  "Neck", region=slots["body"])
        mark("shoulder", start)

    # The real tail keeps the rear chain; the neck never borrows it.
    tail_pts = chain_points(rest, REAR_CHAIN, "LowerTorso")
    tail_base = body_len * 0.15
    tail_pts = [tail_pts[0] + (torso_mid - tail_pts[0]).normalized()
                * (tail_base * 1.5)] + tail_pts
    start = len(bm.verts)
    tube(bm, weights, uvs, tail_pts,
         [tail_base * 1.2] + taper_along(tail_pts[1:], tail_base,
                                         body_len * 0.035),
         ["LowerTorso"] * 2 + REAR_CHAIN, region=slots["tail"], segments=8)
    mark("tail", start)

    # The donor's own sole plane. Its four foot joints are coplanar to
    # 0.006 units, so the 0.085 spread the review measured between fore and
    # hind soles was the foot RING tilting about its joint, not the donor.
    # Every sole is flattened onto this plane, and the review ground is drawn
    # there, so "planted" means one thing and is checkable.
    sole_plane = min(rest[bone].z for bone in
                     ("L_Palm", "R_Palm", "L_Foot0", "R_Foot0")
                     if bone in rest)

    # Four columnar limbs from the measured joints.
    for chain_bones in LIMB_CHAINS:
        if not all(bone in rest for bone in chain_bones):
            continue
        upper_b, lower_b, foot_b = (chain_bones[0], chain_bones[1],
                                    chain_bones[-1])
        top, knee, foot = rest[upper_b], rest[lower_b], rest[foot_b]
        radius = body_len * (0.115 if kind == "brachiosaurus" else 0.125)
        # The limb top is sunk toward the torso centre so the proximal cap is
        # inside the hull rather than exposed in silhouette, and the foot is
        # NOT extended below its measured joint. The donor already places the
        # feet and the chain tip on the same ground, and the old extension
        # broke exactly that: it made the elephant trunk float and the
        # brachiosaurus tail tip dip under the foot plane.
        socket = top + (torso_mid - top).normalized() * (radius * 1.5)
        # One ring per real joint, so every joint the donor bends owns skin.
        limb_pts = [socket] + [rest[bone] for bone in chain_bones]
        limb_radii = [radius * 1.25, radius, radius * 0.84,
                      radius * 0.82, radius * 0.80, radius * 0.80]
        extra = SEAM_RINGS if seam in ("graded", "chained", "mixed") else 0
        limb_pts, limb_radii = insert_seam(limb_pts, limb_radii, extra)
        limb_bones = [upper_b] * (extra + 1) + list(chain_bones)
        start = len(bm.verts)
        tube(bm, weights, uvs, limb_pts, limb_radii, limb_bones,
             region=slots["limb"], segments=LIMB_SEGMENTS)
        mark("limbs", start)
        limb_rings = [list(range(start + index * LIMB_SEGMENTS,
                                 start + (index + 1) * LIMB_SEGMENTS))
                      for index in range(extra + 2)]
        parts.setdefault("sockets", []).extend(limb_rings[0])
        # The forelimbs hang off UpperTorso and the hind legs off LowerTorso in
        # the donor's own hierarchy, so each socket is pinned to its own host.
        if "Arm" in upper_b:
            parts.setdefault("forelimbs", []).extend(
                range(start, len(bm.verts)))
        # The LAST ring of the whole tube, which is the sole. limb_rings
        # enumerates only the seam rings at the other end, and flattening
        # those would drop the shoulder to the ground.
        bm.verts.ensure_lookup_table()
        sole_ring = range(start + (len(limb_pts) - 1) * LIMB_SEGMENTS,
                          start + len(limb_pts) * LIMB_SEGMENTS)
        for index in sole_ring:
            bm.verts[index].co.z = sole_plane
        host = "UpperTorso" if "Arm" in upper_b else "LowerTorso"
        pin(limb_rings[0], host)
        # A limb socket crosses no intermediate donor joint: UpperTorso
        # parents L_Arm_Upper directly.
        grade(limb_rings, [host, upper_b],
              seam in ("graded", "chained", "mixed"))

    # P6 classified and repaired. The enclosed dark patch the review found was
    # NOT a hole: the topology report shows zero boundary and zero non-manifold
    # edges, so the shell is closed. The face-orientation pass showed the real
    # cause - the tube end caps are wound inward, so their backfaces were being
    # seen through the surface. Recalculating normals outward fixes the actual
    # surface defect; it is not a material or a shading trick.
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    bm.normal_update()
    # The influence rows are keyed by BMVert, and a BMVert does not survive
    # bm.free(). Re-key them by vertex index first, the way the released
    # generators do, so the binding step can address mesh vertices.
    bm.verts.index_update()
    bm.to_mesh(mesh)
    indexed = {vert.index: entries for vert, entries in weights.items()}
    uv_indexed = {vert.index: uv for vert, uv in uvs.items()}
    bm.free()
    mesh.validate()
    if len(mesh.vertices) != len(indexed):
        raise SystemExit("validate() changed the vertex count, so the "
                         "influence rows no longer address this mesh")
    landmarks = {
        "torsoMid": torso_mid.copy(), "bodyRadii": tuple(body_radii),
        "bodyLength": body_len, "headJoint": head_joint.copy(),
        "anchor": anchor.copy(), "chainTip": pts[-1].copy(),
        "neckState": neck_state, "solePlane": sole_plane,
        "anchorSink": (anchor - head_joint).length,
    }
    # "sockets" is an overlay inside "limbs", so the cover check counts the
    # parts that partition the body and not the overlays on top of it.
    covered = set()
    for name, rows in parts.items():
        if name in PRIMARY_PARTS:
            covered.update(rows)
    if len(covered) != len(mesh.vertices):
        raise SystemExit("a part of the body was built without being named")
    return BuiltBody(obj, indexed, uv_indexed, landmarks, anchor_ring, parts)


def topology_report(obj):
    """Classify surface defects by topology, not by looking at shading.

    A dark enclosed patch in a render is either a real hole, a flipped face,
    or just a shadow. Matcap-style shading cannot tell them apart, so this
    counts the things that actually distinguish them: boundary edges bound a
    hole, non-manifold edges mean the surface is not a clean shell, and a
    closed shell whose signed volume comes out NEGATIVE is wound inside out.

    The signed volume replaced an earlier test that asked whether a face
    pointed away from the whole body's centroid. On a body of overlapping
    placed primitives that labels the inner surface of every leg as inward: it
    reported 78 and 105 flipped faces on bodies whose shells are all closed and
    correctly wound. Signed volume is exact per shell, and indifferent to
    convexity and to where the rest of the body is.
    """
    mesh = obj.data
    bm = bmesh.new()
    bm.from_mesh(mesh)
    bm.normal_update()
    boundary = set(e for e in bm.edges if len(e.link_faces) == 1)
    nonmanifold = [e for e in bm.edges if len(e.link_faces) > 2]
    wire = [e for e in bm.edges if len(e.link_faces) == 0]
    loose = [v for v in bm.verts if not v.link_edges]

    shells, seen = [], set()
    for face in bm.faces:
        if face.index in seen:
            continue
        group, stack = [], [face]
        seen.add(face.index)
        while stack:
            current = stack.pop()
            group.append(current)
            for edge in current.edges:
                for neighbour in edge.link_faces:
                    if neighbour.index not in seen:
                        seen.add(neighbour.index)
                        stack.append(neighbour)
        shells.append(group)

    open_shells, inside_out = 0, 0
    for group in shells:
        if any(edge in boundary for face in group for edge in face.edges):
            open_shells += 1
            continue
        volume = 0.0
        for face in group:
            verts = face.verts[:]
            for index in range(1, len(verts) - 1):
                volume += verts[0].co.dot(
                    verts[index].co.cross(verts[index + 1].co)) / 6.0
        if volume < 0.0:
            inside_out += 1

    report = {
        "verts": len(bm.verts), "edges": len(bm.edges), "faces": len(bm.faces),
        "boundaryEdges": len(boundary), "nonManifoldEdges": len(nonmanifold),
        "wireEdges": len(wire), "looseVerts": len(loose),
        "shells": len(shells), "openShells": open_shells,
        "insideOutShells": inside_out,
    }
    bm.free()
    return report


def orientation_material():
    """Blender's face-orientation overlay, as a real material: blue where a
    face points at the camera, red where its back does."""
    mat = bpy.data.materials.new("FaceOrientation")
    mat.use_nodes = True
    tree = mat.node_tree
    for node in list(tree.nodes):
        tree.nodes.remove(node)
    geo = tree.nodes.new("ShaderNodeNewGeometry")
    mix = tree.nodes.new("ShaderNodeMixRGB")
    mix.inputs[1].default_value = (0.12, 0.35, 0.95, 1.0)
    mix.inputs[2].default_value = (0.95, 0.15, 0.10, 1.0)
    emit = tree.nodes.new("ShaderNodeEmission")
    out = tree.nodes.new("ShaderNodeOutputMaterial")
    tree.links.new(geo.outputs["Backfacing"], mix.inputs[0])
    tree.links.new(mix.outputs[0], emit.inputs["Color"])
    tree.links.new(emit.outputs[0], out.inputs["Surface"])
    return mat


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--census", required=True)
    parser.add_argument("--out", required=True)
    parser.add_argument("--kind", default="both")
    parser.add_argument("--diagnostic", action="store_true",
                        help="add face-orientation renders and a topology "
                             "report that classifies surface defects")
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

    # A single sun left the front and rear views backlit and unreadable, and
    # an unreadable render cannot be inspected. Three suns from different
    # azimuths give every view enough form to judge silhouette and joins.
    # This is review lighting only - it changes no geometry.
    for name, pitch, yaw, energy in (("Key", 52, 35, 3.0),
                                     ("Fill", 60, 200, 1.9),
                                     ("Side", 70, 290, 1.4)):
        light_data = bpy.data.lights.new(name, type="SUN")
        light_data.energy = energy
        light = bpy.data.objects.new(name, light_data)
        light.rotation_euler = (math.radians(pitch), 0.0, math.radians(yaw))
        scene.collection.objects.link(light)

    kinds = ("brachiosaurus", "elephant") if args.kind == "both" else (args.kind,)
    # The elephant's curl is a TRUNK curl from its measured rest pose. Only
    # the sauropod gets the upright re-aim, and only its curled probe stacks
    # the two. Previously 'curled' meant both for both creatures, so the
    # elephant's curled render stood its trunk up like a neck.
    states = {"brachiosaurus": ("rest", "upright", "upright-curled"),
              "elephant": ("rest", "curled")}
    written = []
    for kind in kinds:
        for state in states[kind]:
            for obj in [o for o in bpy.data.objects
                        if o.name.endswith("_graybox")]:
                bpy.data.objects.remove(obj, do_unlink=True)
            body = build(kind, rest, state).obj
            centre = sum((Vector(c) for c in body.bound_box),
                         Vector()) / 8.0
            centre = body.matrix_world @ centre
            extent = max(body.dimensions) * 0.78

            # The conversion leaves the creature facing +Y, so the camera
            # that sees its FRONT stands at +Y looking back along -Y, and the
            # rear camera stands at -Y. These were the wrong way round, which
            # mislabelled every front and rear render produced so far.
            for view, offset in (("side", Vector((1.0, 0.0, 0.12))),
                                 ("front", Vector((0.0, 1.0, 0.12))),
                                 ("rear", Vector((0.0, -1.0, 0.12))),
                                 ("party", Vector((0.72, 0.72, 0.55)))):
                camera.location = centre + offset.normalized() * extent * 4.0
                point_camera(camera, centre)
                camera_data.ortho_scale = extent * 2.6
                name = "%s-%s-%s.png" % (kind, state, view)
                scene.render.filepath = str(out / name)
                bpy.ops.render.render(write_still=True)
                written.append(name)

            if args.diagnostic:
                report = topology_report(body)
                print("TOPOLOGY %s-%s: %s" % (kind, state, json.dumps(report)))
                body.data.materials.clear()
                body.data.materials.append(orientation_material())
                for view, offset in (("orient-front", Vector((0.0, 1.0, 0.12))),
                                     ("orient-side", Vector((1.0, 0.0, 0.12)))):
                    camera.location = centre + offset.normalized() * extent * 4.0
                    point_camera(camera, centre)
                    name = "%s-%s-%s.png" % (kind, state, view)
                    scene.render.filepath = str(out / name)
                    bpy.ops.render.render(write_still=True)
                    written.append(name)
    print("GRAYBOX RENDERS: " + ", ".join(written))


if __name__ == "__main__":
    main()
