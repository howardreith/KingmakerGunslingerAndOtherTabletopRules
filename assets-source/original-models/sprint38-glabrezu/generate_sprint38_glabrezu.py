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
# R7 roots the small arms inside the chest and fuses them to the skin, replaces
# the boxed head and banded trunk, and bakes position-driven materials.
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
import continuous_body as surface
import outsider_bake as bake
sys.path.insert(0, str(args.pipeline_root / 'sprint36-demons'))
from anatomy_planes import muscle

SIDE, UP, FRONT = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, 1, 0))
TORSO = ('Torso_Lower', 'Torso_Upper', 'neck', 'neck1', 'Head')
ARMS = ('Arm_Upper', 'Arm_Lower', 'Palm', 'finger1', 'finger2',
        'Bfinger1', 'Bfinger2')
LEGS = ('Leg0_Upper', 'Leg0_Lower', 'Foot0', 'toe')
BONES = TORSO + tuple(side + '_' + part for side in ('L', 'R')
                      for part in ARMS + LEGS)
# Authoring material IDs; the baked colour function gives each its final paint.
ATLAS_REGION = {
    'hide': 'body', 'plate': 'limbs', 'chitin': 'limbs',
    'horn': 'beak', 'claw': 'beak', 'eye': 'crest'}
DETAIL_COUNTS = {}


def build_body(bm, weights, uvs, rig, kind):
    at = lambda name: common.shared.head(rig, name)

    def mass(point, radii, bone, region='hide', rings=12, sides=18):
        common.ellipsoid(bm, weights, uvs, point, SIDE, UP, FRONT, radii,
                         bone, ATLAS_REGION.get(region, region), rings, sides)

    def tube(points, radii, names, region='hide', sides=12):
        common.tube(bm, weights, uvs, points, radii, names,
                    ATLAS_REGION.get(region, region), sides)

    def section(points, widths, heights, names, sides=18):
        common.sweep(bm, weights, uvs, points, widths, heights, names,
                     region='body', steps=5, sides=sides, up=UP, forward=FRONT)

    pelvis = at('Torso_Lower')
    upper = at('Torso_Upper')
    head_bone = at('Head')

    # A tall V-shaped demon torso: broad pectoral shelf and lats over a narrow
    # waist, replacing the stacked barrel and its belt-like seams.
    common.sweep(bm, weights, uvs, [at(n) for n in TORSO[:-1]],
                 [.205, .295, .150, .118], [.160, .225, .135, .105],
                 list(TORSO[:-1]), up=FRONT, forward=UP)
    mass(pelvis + Vector((0, .005, -.040)), (.235, .175, .185), 'Torso_Lower')
    mass(upper + Vector((0, .010, .205)), (.335, .275, .245), 'Torso_Upper')
    mass(upper + Vector((0, -.090, .335)), (.325, .165, .185), 'Torso_Upper')
    mass(upper + Vector((0, -.060, .470)), (.205, .125, .160), 'neck')
    for sign in (-1, 1):
        mass(upper + Vector((sign*.150, .205, .285)), (.170, .112, .085),
             'Torso_Upper')
        mass(upper + Vector((sign*.235, -.060, .095)), (.105, .265, .135),
             'Torso_Upper')
        # Abdominal ridges: two soft rows narrowing into the waist.
        for row in range(3):
            mass(upper + Vector((sign*.060, .172 - row*.012, .020 - row*.112)),
                 (.058, .054, .040), 'Torso_Upper' if row < 2 else 'Torso_Lower')
    tube([upper + Vector((0, -.020, .385)), at('neck1') + Vector((0, .020, 0)),
          head_bone + Vector((0, .050, -.015))],
         [.142, .118, .098], ['neck', 'neck1', 'Head'], 'hide', 14)

    # The head: one tapered canine skull instead of three stacked boxes. Long
    # muzzle, heavy brow, jaw held open so the fangs read at game zoom.
    head = head_bone + Vector((0, .090, .015))
    mass(head + Vector((0, -.015, .035)), (.132, .128, .148), 'Head')
    section([head + Vector((0, .040, .040)), head + Vector((0, .150, .004)),
             head + Vector((0, .265, -.030)), head + Vector((0, .315, -.040))],
            [.122, .086, .066, .056], [.098, .068, .050, .040], ['Head'] * 4)
    section([head + Vector((0, .000, -.098)), head + Vector((0, .140, -.142)),
             head + Vector((0, .265, -.168))],
            [.104, .070, .050], [.056, .036, .026], ['Head'] * 3)
    fangs = horns = 0
    for sign in (-1, 1):
        mass(head + Vector((sign*.100, .030, -.045)), (.056, .080, .105), 'Head')
        mass(head + Vector((sign*.078, .095, .070)), (.074, .028, .060), 'Head')
        # Pointed ears give the canine read beside the horns.
        tube([head + Vector((sign*.112, -.070, .075)),
              head + Vector((sign*.178, -.125, .135)),
              head + Vector((sign*.210, -.165, .190))],
             [.048, .028, .004], ['Head'] * 3, 'hide', 9)
        tube([head + Vector((sign*.050, .262, -.056)),
              head + Vector((sign*.052, .274, -.142))],
             [.021, .003], ['Head'] * 2, 'claw', 8)
        tube([head + Vector((sign*.040, .246, -.150)),
              head + Vector((sign*.044, .258, -.070))],
             [.018, .003], ['Head'] * 2, 'claw', 8)
        tube([head + Vector((sign*.066, .160, -.040)),
              head + Vector((sign*.068, .165, -.100))],
             [.012, .002], ['Head'] * 2, 'claw', 7)
        fangs += 2
        mass(head + Vector((sign*.100, .098, .022)), (.019, .014, .016),
             'Head', 'eye', 8, 12)
        horn = head + Vector((sign*.082, -.040, .130))
        mass(horn, (.050, .036, .050), 'Head', 'hide', 8, 12)
        tube([horn, horn + Vector((sign*.082, -.075, .105)),
              horn + Vector((sign*.125, -.190, .190)),
              horn + Vector((sign*.118, -.300, .225))],
             [.040, .030, .018, .003], ['Head'] * 4, 'horn', 11)
        short = head + Vector((sign*.126, -.075, .070))
        tube([short, short + Vector((sign*.082, -.112, .058))],
             [.026, .003], ['Head'] * 2, 'horn', 9)
        horns += 2

    pincers = small_arms = claws = 0
    jaw_teeth = 0
    for side in ('L', 'R'):
        sign = 1 if side == 'L' else -1
        arm = [side + '_' + name for name in ARMS[:3]]
        shoulder, elbow, palm = [at(name) for name in arm]

        # THE PINCER ARMS, on the rig's real animated chains.
        tube([upper + Vector((sign*.160, 0, .255)), shoulder,
              shoulder.lerp(elbow, .42), elbow, elbow.lerp(palm, .30), palm],
             [.150, .140, .152, .104, .126, .082],
             ['Torso_Upper', arm[0], arm[0], arm[1], arm[1], arm[2]], 'hide', 16)
        mass(shoulder + Vector((sign*.030, 0, .025)), (.140, .155, .135), arm[0])
        muscle(common, bm, weights, uvs, shoulder, elbow, .148, .118, arm[0], 'body')
        # Chitin vambrace over the forearm, narrowing into the wrist boss.
        tube([elbow.lerp(palm, .12), elbow.lerp(palm, .52), palm],
             [.128, .126, .090], [arm[1], arm[1], arm[2]], 'chitin', 12)
        boss = palm + FRONT * .020
        mass(boss, (.076, .082, .070), arm[2], 'chitin', 10, 14)
        jaw_teeth = 0
        for digit, lift in (('finger', .030), ('Bfinger', -.034)):
            first, second = side + '_' + digit + '1', side + '_' + digit + '2'
            root, tip = at(first), at(second)
            direction = (tip - root).normalized()
            tube([boss + UP * lift * .4, root + UP * lift, tip + UP * lift * .8,
                  tip + direction * .175 + UP * lift * .5],
                 [.074, .068, .052, .010],
                 [arm[2], first, second, second], 'chitin', 11)
            for index in range(3):
                along = .34 + index * .28
                base = root.lerp(tip, along) + UP * lift * .9
                tube([base, base - UP * lift * .9 * .62],
                     [.011, .002], [second] * 2, 'claw', 6)
                jaw_teeth += 1
        pincers += 1

        # THE SMALLER ARMS grow out of the chest. The root sits inside a
        # subpectoral mound and the skin union fuses them; the old root floated
        # in front of the torso. Weighted to Torso_Upper: the rig has only two
        # arm chains, so these follow the chest and do not articulate.
        mound = upper + Vector((sign*.190, .135, .110))
        mass(mound, (.120, .112, .120), 'Torso_Upper')
        root = upper + Vector((sign*.180, .120, .105))
        mid = root + Vector((sign*.240, .140, -.105))
        wrist = mid + Vector((sign*.050, .130, -.170))
        tube([root, root.lerp(mid, .5) + UP*.018, mid, mid.lerp(wrist, .42), wrist],
             [.098, .084, .054, .060, .040], ['Torso_Upper'] * 5, 'hide', 13)
        mass(root.lerp(mid, .48), (.078, .074, .082), 'Torso_Upper')
        mass(mid.lerp(wrist, .40), (.060, .082, .060), 'Torso_Upper')
        hand = wrist + Vector((sign*.004, .028, -.030))
        mass(hand, (.054, .052, .050), 'Torso_Upper')
        for index in range(3):
            spread = (index - 1) * .038
            base = hand + Vector((spread, .040, -.022))
            tube([hand + Vector((spread * .5, 0, 0)), base],
                 [.026, .019], ['Torso_Upper'] * 2, 'hide', 9)
            tube([base - FRONT * .006, base + Vector((spread * .4, .062, -.040))],
                 [.017, .003], ['Torso_Upper'] * 2, 'claw', 7)
            claws += 1
        small_arms += 1

        # Heavy digitigrade legs on the measured joints.
        leg = [side + '_' + name for name in LEGS]
        thigh, knee, ankle, toe = [at(name) for name in leg]
        tube([pelvis, thigh, thigh.lerp(knee, .45), knee], [.150, .138, .135, .082],
             ['Torso_Lower', leg[0], leg[0], leg[1]], 'hide', 14)
        mass(thigh.lerp(knee, .42) + FRONT * .016, (.110, .158, .122), leg[0])
        mass(knee + FRONT * .022, (.082, .074, .070), leg[1])
        tube([knee, knee.lerp(ankle, .30) - FRONT * .026, ankle,
              ankle.lerp(toe, .40) + FRONT * .024, toe],
             [.076, .106, .066, .084, .054],
             [leg[1], leg[1], leg[2], leg[2], leg[3]], 'hide', 13)
        foot = toe + Vector((0, .064, .027))
        mass(foot, (.092, .042, .108), leg[-1])
        for index in range(3):
            spread = (index - 1) * .068
            tube([foot + Vector((spread * .4, 0, 0)),
                  foot + Vector((spread, .162, -.014))],
                 [.028, .004], [leg[-1]] * 2, 'claw', 9)

    DETAIL_COUNTS.update(pincers=pincers, smallArms=small_arms,
                         smallClaws=claws, horns=horns, fangs=fangs,
                         pincerTeeth=jaw_teeth * 2)

    consolidate_skin(bm, weights, uvs, common.shared.ATLAS,
                     regions=('body',), voxel=.007, triangles=11000)
    bake.bake_model(bm, weights, uvs, common.shared.ATLAS, args.albedo, colour)


def colour(p, n, region, cavity):
    """Bind-pose paint: x lateral, y forward, z up. Original closed-form colour."""
    count = len(p)
    blotch = surface.fbm(p, 6.5, 71)
    grain = surface.fbm(p, 44.0, 83)
    x, y, z = p[:, 0], p[:, 1], p[:, 2]
    # Oxblood hide, darker across the back and shoulders.
    hide = bake.mix(bake.rgb((.235, .105, .095), count),
                    bake.rgb((.405, .200, .155), count),
                    bake.smooth(.34, .68, blotch))
    back = bake.smooth(-.10, .22, -n[:, 1]) * bake.smooth(.90, 1.20, z)
    hide = bake.mix(hide, bake.rgb((.125, .068, .070), count), back * .80)
    # Paler chest and abdomen so the small arms' roots read against the torso.
    chest = bake.smooth(-.10, .55, n[:, 1]) * bake.smooth(.92, 1.06, z) * \
        (1 - bake.smooth(1.56, 1.72, z)) * (1 - bake.smooth(.26, .44, np.abs(x)))
    flesh = bake.mix(bake.rgb((.440, .275, .220), count),
                     bake.rgb((.370, .210, .175), count),
                     bake.smooth(.35, .70, surface.fbm(p, 12.0, 91)))
    skin = bake.mix(hide, flesh, chest * .90)
    # Burned-dark hands, feet and small-arm tips fade out of the hide.
    ends = 1 - bake.smooth(.14, .44, z)
    skin = bake.mix(skin, bake.rgb((.100, .062, .062), count), ends * .78)
    # Muzzle pale along the lips, black nose and mouth interior.
    head_y, head_z = .202, 1.814
    muzzle = bake.smooth(head_y + .13, head_y + .24, y) * bake.smooth(1.62, 1.68, z)
    skin = bake.mix(skin, bake.rgb((.300, .165, .140), count), muzzle * .7)
    nose = bake.smooth(head_y + .285, head_y + .315, y) * \
        bake.smooth(head_z - .070, head_z - .030, z)
    skin = bake.mix(skin, bake.rgb((.030, .024, .024), count), nose)
    mouth = (1 - bake.smooth(.000, .034, np.abs(z - (head_z - .098 - .090 *
             np.clip((y - head_y) / .30, 0, 1))))) * bake.smooth(head_y + .02, head_y + .12, y)
    skin = bake.mix(skin, bake.rgb((.040, .022, .024), count), mouth)
    skin *= (.90 + .20 * grain)[..., None]

    result = skin
    # Pincer chitin: near-black with a cold grey sheen along raised edges.
    chitin = bake.mix(bake.rgb((.070, .062, .064), count),
                      bake.rgb((.225, .205, .205), count),
                      bake.smooth(.42, .78, surface.fbm(p, 15.0, 97)))
    chitin = bake.mix(chitin, bake.rgb((.280, .240, .235), count),
                      bake.smooth(.55, .95, cavity) * .35)
    horn = bake.mix(bake.rgb((.330, .255, .180), count),
                    bake.rgb((.800, .720, .540), count),
                    bake.smooth(1.92, 2.28, z))
    claw_mask = z < 1.70
    horn[claw_mask] = bake.mix(bake.rgb((.300, .240, .175), claw_mask.sum()),
                               bake.rgb((.760, .690, .530), claw_mask.sum()),
                               bake.smooth(.35, .75, surface.fbm(p[claw_mask], 18.0, 101)))
    eye = bake.mix(bake.rgb((.700, .300, .060), count),
                   bake.rgb((.980, .620, .120), count),
                   bake.smooth(.3, .9, n[:, 1]))
    result = np.where((region == bake.LIMBS)[..., None], chitin, result)
    result = np.where((region == bake.BEAK)[..., None], horn, result)
    result = np.where((region == bake.CREST)[..., None], eye, result)
    shade = .32 + .68 * bake.smooth(.30, .90, cavity)
    shade = np.where(region == bake.CREST, 1.0, shade)
    return result * shade[..., None]


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
    jawSeparated=False, triangleWinding='authored-outward-sprint38',
    uvAtlas={'body': [0.0, 0.0, 1.0, 1.0]})
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
