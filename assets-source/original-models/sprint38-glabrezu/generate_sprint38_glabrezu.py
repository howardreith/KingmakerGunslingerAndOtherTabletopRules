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
# Owner visual overhaul: continuous organic skin, revised anatomy and albedo.
# Rig/wing articulation limits above remain; native aesthetic review is separate.

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

sys.path.insert(0, str(args.pipeline_root / 'continuous-bodies'))
from outsider_surface import consolidate_skin
sys.path.insert(0, str(args.pipeline_root / 'sprint36-demons'))
from anatomy_planes import muscle, bevel_mass

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
                 [.265, .335, .125, .100], [.210, .265, .115, .090],
                 list(TORSO[:-1]), up=FRONT, forward=UP)
    mass(pelvis - UP * .045 + FRONT * .010, (.245, .195, .200),
         'Torso_Lower')
    mass(upper + FRONT * .015, (.345, .245, .260), 'Torso_Upper')
    # Shoulder plates over the pincer roots, in the harder plate tone, so the
    # arms separate from the chest in a silhouette.
    for sign in (-1, 1):
        mass(upper + SIDE * sign * .175 + UP * .055 - FRONT * .010,
             (.108, .098, .062), 'Torso_Upper', 'plate', 8, 14)

    mass(upper + UP*.255 + FRONT*.010, (.330,.250,.245), 'Torso_Upper')

    # A short thick neck carrying a canine head well clear of the shoulders.
    tube([neck - UP * .020, neck + FRONT * .025 + UP * .045,
          head_bone + FRONT * .035 + UP * .010],
         [.092, .082, .074], ['neck', 'neck1', 'Head'], 'hide', 12)

    # The head: a domed skull, a long muzzle and swept horns. The muzzle is
    # the longest feature, which is what keeps it canine rather than the
    # Girallon's flat ape face or the Vrock's beak.
    head = head_bone + FRONT * .080 + UP * .025
    bevel_mass(common,bm,weights,uvs,head,(.165,.165,.140),'Head','body')
    bevel_mass(common,bm,weights,uvs,head+FRONT*.155-UP*.025,
               (.095,.065,.150),'Head','body',.65)
    # Lower jaw, carried slightly open so the fangs read.
    bevel_mass(common,bm,weights,uvs,head+FRONT*.145-UP*.100,
               (.105,.045,.145),'Head','body',.75)
    fangs = 0
    for sign in (-1, 1):
        # Upper and lower canines.
        tube([head + SIDE * sign * .061 + FRONT * .225 - UP * .026,
              head + SIDE * sign * .061 + FRONT * .240 - UP * .110],
             [.020, .003], ['Head'] * 2, 'claw', 8)
        tube([head + SIDE * sign * .052 + FRONT * .210 - UP * .110,
              head + SIDE * sign * .052 + FRONT * .220 - UP * .044],
             [.017, .003], ['Head'] * 2, 'claw', 8)
        fangs += 2
        # Eyes.
        bevel_mass(common,bm,weights,uvs,
                   head+SIDE*sign*.104+FRONT*.087+UP*.070,
                   (.069,.032,.063),'Head','body')
        mass(head + SIDE * sign * .120 + FRONT * .137 + UP * .038,
             (.017, .012, .012), 'Head', 'eye', 7, 12)
    horns = 0
    for sign in (-1, 1):
        # One long swept horn and a shorter one behind it.
        tube([head + SIDE * sign * .090 + UP * .125 - FRONT * .035,
              head + SIDE * sign * .160 + UP * .240 - FRONT * .110,
              head + SIDE * sign * .190 + UP * .330 - FRONT * .220],
             [.039, .023, .004], ['Head'] * 3, 'horn', 11)
        tube([head + SIDE * sign * .135 + UP * .040 - FRONT * .065,
              head + SIDE * sign * .210 + UP * .120 - FRONT * .170],
             [.026, .003], ['Head'] * 2, 'horn', 9)
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
        tube([upper+UP*.20,shoulder,shoulder.lerp(elbow,.42),elbow,
              elbow.lerp(palm,.30),palm], [.150,.132,.153,.105,.125,.080],
             ['Torso_Upper',arm[0],arm[0],arm[1],arm[1],arm[2]],'hide',14)
        muscle(common,bm,weights,uvs,shoulder,elbow,.148,.118,arm[0],'body')

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
                  tip + direction * .175 + UP * lift * .5],
                 [.072, .067, .051, .010],
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
        root = upper + SIDE * sign * .165 + FRONT * .235 + UP * .155
        mid = root + SIDE * sign * .170 + FRONT * .110 - UP * .085
        wrist = mid + SIDE * sign * .070 + FRONT * .060 - UP * .170
        tube([root,root.lerp(mid,.5)+UP*.012,mid,mid.lerp(wrist,.45),wrist],
             [.064,.073,.046,.052,.034],['Torso_Upper']*5,'bare',12)
        hand = wrist + FRONT * .025 - UP * .022
        bevel_mass(common,bm,weights,uvs,hand,(.047,.048,.035),'Torso_Upper','limbs')
        for index in range(3):
            spread = (index - 1) * .035
            tube([hand + SIDE * spread * .5,
                  hand + SIDE * spread + FRONT * .080 - UP * .050],
                 [.014, .003], ['Torso_Upper'] * 2, 'claw', 7)
            claws += 1
        small_arms += 1

        # Heavy digitigrade legs.
        leg = [side + '_' + name for name in LEGS]
        points = [at(name) for name in leg]
        thigh,knee,ankle,toe=points
        tube([pelvis,thigh,thigh.lerp(knee,.45),knee], [.142,.126,.123,.075],
             ['Torso_Lower',leg[0],leg[0],leg[1]],'hide',13)
        tube([knee,knee.lerp(ankle,.3)-FRONT*.025,ankle,
              ankle.lerp(toe,.4)+FRONT*.025,toe], [.073,.103,.066,.083,.052],
             [leg[1],leg[1],leg[2],leg[2],leg[3]],'hide',13)
        muscle(common,bm,weights,uvs,knee.lerp(ankle,.10),ankle,.077,.065,leg[1],'body')
        foot = toe + FRONT * .065 + UP*.027
        bevel_mass(common,bm,weights,uvs,foot,(.090,.044,.105),leg[-1],'body')
        for index in range(3):
            spread = (index - 1) * .068
            tube([foot + SIDE * spread * .4,
                  foot + SIDE * spread + FRONT * .160 - UP * .014],
                 [.026, .004], [leg[-1]] * 2, 'claw', 9)

    DETAIL_COUNTS.update(pincers=pincers, smallArms=small_arms,
                         smallClaws=claws, horns=horns, fangs=fangs,
                         pincerTeeth=jaw_teeth * 2)

    consolidate_skin(bm, weights, uvs, common.shared.ATLAS,
                     regions=('body',), voxel=.007, triangles=8000)


def write_albedo(path):
    size = 512
    image = np.zeros((size, size, 3), dtype=np.float64)
    # Dark red-brown mottled hide, harder near-black chitin on the pincers,
    # pale horn. Nothing repeats the Vrock's grey-green plumage, the Babau's
    # slime green-black or the Hezrou's olive warts.
    palette = {
        'body': (.39, .22, .17), 'crest': (.28, .15, .105),
        'beak': (.72, .63, .46), 'limbs': (.44, .25, .18),
        'membrane': (.26, .24, .22)}
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
