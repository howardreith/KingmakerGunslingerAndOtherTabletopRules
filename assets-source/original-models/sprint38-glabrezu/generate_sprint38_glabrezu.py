"""One original four-armed Glabrezu body on the measured free-hands rig.

Only measured bone names and project-authored geometry are exported. Native
meshes, materials, textures and animation clips remain runtime dependencies.

The rig has two arm chains and this creature has four limbs, so the split is
deliberate and recorded rather than hidden: the two large PINCER arms ride the
rig's real animated arm chains, because they are the signature limbs and the
printed rend carriers, and the two smaller claw arms are project geometry
weighted to the upper torso. The small arms therefore move with the chest and
do not articulate independently. That is the same accepted shape the released
four-armed Girallon and Xill take on a two-armed rig, and the same one the
Sprint 37 Vrock's wings take; it is a recorded limitation, not a claim of
four-limb articulation.
"""
import argparse
import json
import math
from pathlib import Path
import sys

import bpy
import numpy as np
from mathutils import Vector

sys.dont_write_bytecode = True
parser = argparse.ArgumentParser()
parser.add_argument('--kind', choices=('glabrezu',), default='glabrezu')
parser.add_argument('--pipeline-root', type=Path,
                    default=Path(__file__).resolve().parents[1])
for name in ('capture', 'albedo', 'mesh-data', 'report', 'blend-out',
             'preview'):
    parser.add_argument('--' + name, required=True)
args = parser.parse_args(sys.argv[sys.argv.index('--') + 1:])
for name in ('capture', 'albedo', 'mesh_data', 'report', 'blend_out',
             'preview'):
    setattr(args, name, str(Path(getattr(args, name)).resolve()))
for name in ('sprint17-serpents', 'sprint24-theropods'):
    sys.path.insert(0, str(args.pipeline_root / name))
import generate_snakes as common
import generate_theropods as donor
import paint_snake_albedo as paint

SIDE, UP, FRONT = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, 1, 0))
TORSO = ('Torso_Lower', 'Torso_Upper', 'neck', 'neck1', 'Head')
ARMS = ('Arm_Upper', 'Arm_Lower', 'Palm', 'finger1', 'finger2',
        'Bfinger1', 'Bfinger2')
LEGS = ('Leg0_Upper', 'Leg0_Lower', 'Foot0', 'toe')
BONES = TORSO + tuple(side + '_' + part for side in ('L', 'R')
                      for part in ARMS + LEGS)
# The albedo atlas has five regions; map this creature's materials onto them.
ATLAS_REGION = {
    'hide': 'body', 'plate': 'crest', 'chitin': 'membrane',
    'bare': 'limbs', 'horn': 'beak', 'claw': 'beak', 'eye': 'crest'}
DETAIL_COUNTS = {}


def build_body(bm, weights, uvs, rig, kind):
    at = lambda name: common.shared.head(rig, name)

    def mass(point, radii, bone, region='hide', rings=10, sides=16):
        common.ellipsoid(bm, weights, uvs, point, SIDE, UP, FRONT, radii,
                         bone, ATLAS_REGION.get(region, region), rings, sides)

    def tube(points, radii, names, region='hide', sides=10):
        common.tube(bm, weights, uvs, points, radii, names,
                    ATLAS_REGION.get(region, region), sides)

    pelvis = at('Torso_Lower')
    upper = at('Torso_Upper')
    neck = at('neck1')
    head_bone = at('Head')

    # A heavy upright trunk, broader at the shoulders than at the hips so the
    # pincer arms have a root. Deliberately bulkier than the Vrock's
    # deep-chested bird trunk and than the Babau's lean skirmisher.
    common.sweep(bm, weights, uvs, [at(n) for n in TORSO[:-1]],
                 [.205, .250, .105, .080], [.175, .215, .090, .072],
                 list(TORSO[:-1]), up=FRONT, forward=UP)
    mass(pelvis - UP * .045 + FRONT * .010, (.195, .168, .165),
         'Torso_Lower')
    mass(upper + FRONT * .015, (.262, .196, .205), 'Torso_Upper')
    # Shoulder plates over the pincer roots, in the harder plate tone, so the
    # arms separate from the chest in a silhouette.
    for sign in (-1, 1):
        mass(upper + SIDE * sign * .175 + UP * .055 - FRONT * .010,
             (.108, .098, .062), 'Torso_Upper', 'plate', 8, 14)

    # A short thick neck carrying a canine head well clear of the shoulders.
    tube([neck - UP * .020, neck + FRONT * .025 + UP * .045,
          head_bone + FRONT * .035 + UP * .010],
         [.092, .082, .074], ['neck', 'neck1', 'Head'], 'hide', 12)

    # The head: a domed skull, a long muzzle and swept horns. The muzzle is
    # the longest feature, which is what keeps it canine rather than the
    # Girallon's flat ape face or the Vrock's beak.
    head = head_bone + FRONT * .055 + UP * .018
    mass(head, (.108, .112, .100), 'Head')
    tube([head + FRONT * .070 - UP * .010,
          head + FRONT * .150 - UP * .030,
          head + FRONT * .205 - UP * .044],
         [.072, .056, .040], ['Head'] * 3, 'hide', 12)
    # Lower jaw, carried slightly open so the fangs read.
    tube([head + FRONT * .065 - UP * .056,
          head + FRONT * .180 - UP * .076],
         [.050, .028], ['Head'] * 2, 'hide', 10)
    fangs = 0
    for sign in (-1, 1):
        # Upper and lower canines.
        tube([head + SIDE * sign * .034 + FRONT * .168 - UP * .036,
              head + SIDE * sign * .034 + FRONT * .176 - UP * .082],
             [.013, .002], ['Head'] * 2, 'claw', 7)
        tube([head + SIDE * sign * .030 + FRONT * .160 - UP * .072,
              head + SIDE * sign * .030 + FRONT * .166 - UP * .034],
             [.011, .002], ['Head'] * 2, 'claw', 7)
        fangs += 2
        # Eyes.
        mass(head + SIDE * sign * .062 + FRONT * .072 + UP * .030,
             (.022, .020, .022), 'Head', 'eye', 7, 12)
    horns = 0
    for sign in (-1, 1):
        # One long swept horn and a shorter one behind it.
        tube([head + SIDE * sign * .058 + UP * .082 - FRONT * .010,
              head + SIDE * sign * .104 + UP * .150 - FRONT * .092,
              head + SIDE * sign * .128 + UP * .176 - FRONT * .196],
             [.026, .017, .003], ['Head'] * 3, 'horn', 9)
        tube([head + SIDE * sign * .076 + UP * .036 - FRONT * .022,
              head + SIDE * sign * .112 + UP * .052 - FRONT * .108],
             [.017, .003], ['Head'] * 2, 'horn', 8)
        horns += 2

    pincers = 0
    small_arms = 0
    claws = 0
    for side in ('L', 'R'):
        sign = -1 if side == 'L' else 1
        arm = [side + '_' + name for name in ARMS[:3]]
        shoulder, elbow, palm = [at(name) for name in arm]

        # THE PINCER ARMS, on the rig's real animated chains. Thicker than
        # anything the released outsiders put on these bones, because the
        # printed creature carries its mass in these two limbs.
        tube([upper, shoulder, elbow, palm], [.126, .120, .092, .060],
             ['Torso_Upper', *arm], 'hide', 12)
        mass(shoulder, (.118, .110, .114), arm[0])
        mass(elbow, (.080, .086, .080), arm[1])

        # The claw itself: a wrist boss and two jaws, in the harder chitin
        # tone so the pincer separates from the arm carrying it.
        #
        # The jaws ride the rig's two real digit chains - the upper jaw on
        # finger1/finger2 and the lower on Bfinger1/Bfinger2 - rather than
        # being welded to the palm. That is the whole reason the pincers were
        # given the animated arm chains: the native grip animation opens and
        # closes the digits, so this claw opens and closes with it instead of
        # being a rigid prop. It is also what lets every measured bone carry
        # geometry, which the exporter requires.
        boss = palm + FRONT * .020
        mass(boss, (.062, .070, .058), arm[2], 'chitin', 9, 14)
        jaw_teeth = 0
        for digit, lift in (('finger', .030), ('Bfinger', -.034)):
            first, second = side + '_' + digit + '1', side + '_' + digit + '2'
            root, tip = at(first), at(second)
            direction = (tip - root).normalized()
            # A thick tapering jaw along the digit, three times a finger's
            # girth, reaching well past the last joint.
            tube([boss + UP * lift * .4, root + UP * lift,
                  tip + UP * lift * .8,
                  tip + direction * .105 + UP * lift * .5],
                 [.044, .040, .029, .008],
                 [arm[2], first, second, second], 'chitin', 10)
            # Inner teeth on the biting edge, pointing across the gap, so the
            # opening reads as a grasping claw rather than two spikes.
            for index in range(3):
                along = .34 + index * .28
                base = root.lerp(tip, along) + UP * lift * .9
                tube([base, base - UP * lift * .9 * .62],
                     [.010, .002], [second] * 2, 'claw', 6)
                jaw_teeth += 1
        pincers += 1

        # THE SMALLER CLAW ARMS, from the chest. Project geometry weighted to
        # Torso_Upper: the rig has no third and fourth arm chain, so these
        # move with the chest and do not articulate. Short enough that the
        # hands sit inside the pincer span, which is what makes them read as
        # a second pair on the chest rather than as four equal arms.
        root = upper + SIDE * sign * .090 + FRONT * .110 - UP * .055
        mid = root + SIDE * sign * .120 + FRONT * .080 - UP * .105
        wrist = mid + SIDE * sign * .038 + FRONT * .060 - UP * .120
        tube([root, mid, wrist], [.052, .042, .030],
             ['Torso_Upper'] * 3, 'hide', 10)
        hand = wrist + FRONT * .022 - UP * .018
        mass(hand, (.034, .040, .026), 'Torso_Upper', 'bare', 8, 12)
        for index in range(3):
            spread = (index - 1) * .026
            tube([hand + SIDE * spread * .5,
                  hand + SIDE * spread + FRONT * .052 - UP * .034],
                 [.011, .002], ['Torso_Upper'] * 2, 'claw', 6)
            claws += 1
        small_arms += 1

        # Heavy digitigrade legs.
        leg = [side + '_' + name for name in LEGS]
        points = [at(name) for name in leg]
        tube([pelvis, points[0], points[1]], [.140, .132, .086],
             ['Torso_Lower', leg[0], leg[1]], 'hide', 11)
        tube([points[1], points[2], points[3]], [.078, .050, .034],
             [leg[1], leg[2], leg[3]], 'hide', 10)
        foot = points[-1] + FRONT * .026
        mass(foot, (.050, .064, .030), leg[-1], 'bare', 8, 14)
        for index in range(3):
            spread = (index - 1) * .040
            tube([foot + SIDE * spread * .4,
                  foot + SIDE * spread + FRONT * .082 - UP * .012],
                 [.015, .003], [leg[-1]] * 2, 'claw', 7)

    DETAIL_COUNTS.update(pincers=pincers, smallArms=small_arms,
                         smallClaws=claws, horns=horns, fangs=fangs,
                         pincerTeeth=jaw_teeth * 2)


def write_albedo(path):
    size = 512
    image = np.zeros((size, size, 3), dtype=np.float64)
    # Dark red-brown mottled hide, harder near-black chitin on the pincers,
    # pale horn. Nothing repeats the Vrock's grey-green plumage, the Babau's
    # slime green-black or the Hezrou's olive warts.
    palette = {
        'body': (.230, .118, .098), 'crest': (.150, .080, .070),
        'beak': (.430, .385, .310), 'limbs': (.285, .150, .128),
        'membrane': (.120, .098, .095)}
    for region in sorted(paint.shared.ATLAS):
        (y0, y1, x0, x1), u, v = paint.shared.region_canvas(size, region)
        base = np.array(palette.get(region, palette['body']))
        pixels = np.broadcast_to(base, (*u.shape, 3)).copy()
        # A coarse blotched hide rather than the Vrock's directional feather
        # streaks: overlapping low-frequency lobes with a fine break-up.
        pattern = .84 + .14 * np.sin(u * math.pi * 5 + 1.1) * np.sin(
            v * math.pi * 7 + 0.4)
        pattern += .05 * np.sin(u * math.pi * 23) * np.cos(v * math.pi * 19)
        pixels *= pattern[..., None]
        image[y0:y1, x0:x1] = np.clip(pixels, 0, 1)
    Path(path).parent.mkdir(parents=True, exist_ok=True)
    paint.shared.write_png(path, image)


write_albedo(args.albedo)
rig_data, rig, rig_hash = donor.measured_rig(args.capture)
finish = common.finish_original_winding
common.finish_original_winding = lambda payload, kind: finish(payload,
                                                              'salamander')
common.write_prototype(
    args, rig_data, rig, rig_hash, build_body, BONES, 'lizardfolk-free-hands',
    'donor renderer local; +X lateral, +Y forward, +Z up',
    dict(visibleArms=4, visibleLegs=2, humanoidBody=True,
         jawSeparated=False, runtimeQualified=False))
path = Path(args.mesh_data)
payload = json.loads(path.read_text(encoding='utf-8'))
payload.update(
    creature=args.kind, nativeDonor=donor.DONOR_GUID,
    nativePrefab=donor.PREFAB_GUID, bodyRenderer=donor.BODY_RENDERER,
    accessoryRenderer=donor.ACCESSORY_RENDERER, nativeRendererCount=2,
    visibleArms=4, visibleLegs=2, humanoidBody=True,
    distinctSilhouette='four-armed-pincered-demon',
    canineHead=True, sweptHorns=True,
    visiblePincers=DETAIL_COUNTS['pincers'],
    visibleSmallArms=DETAIL_COUNTS['smallArms'],
    visibleSmallClaws=DETAIL_COUNTS['smallClaws'],
    visibleHorns=DETAIL_COUNTS['horns'],
    visibleFangs=DETAIL_COUNTS['fangs'],
    visiblePincerTeeth=DETAIL_COUNTS['pincerTeeth'],
    # The pincer jaws ride the rig's two digit chains, so the native
    # grip animation opens and closes the claw.
    pincerJawsOnDigitChains=True,
    # The rig carries two arm chains. The pincers ride them and animate; the
    # smaller chest arms are weighted to the upper torso and do not.
    articulatedArmChains=2, articulatedPincers=True,
    articulatedSmallArms=False,
    jawSeparated=False, triangleWinding='authored-outward-sprint38')
path.write_text(json.dumps(payload, indent=1) + '\n', encoding='utf-8',
                newline='\n')

scene = bpy.context.scene
scene.render.engine = 'BLENDER_EEVEE_NEXT'
scene.world = scene.world or bpy.data.worlds.new('Sprint38GlabrezuPreviewWorld')
scene.world.color = (.10, .10, .10)
bpy.ops.object.camera_add(location=(3.4, 5.8, 3.0))
camera = bpy.context.object
camera.rotation_euler = (Vector((0, 0, .95)) -
                         camera.location).to_track_quat('-Z', 'Y').to_euler()
camera.data.type = 'ORTHO'
camera.data.ortho_scale = 3.1
scene.camera = camera
bpy.ops.object.light_add(type='AREA', location=(2.5, 3.5, 5))
light = bpy.context.object
light.data.energy = 700
light.data.size = 4
light.rotation_euler = (-light.location).to_track_quat('-Z', 'Y').to_euler()
scene.render.resolution_x = scene.render.resolution_y = 720
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = 'PNG'
scene.render.filepath = args.preview
bpy.ops.render.render(write_still=True)
