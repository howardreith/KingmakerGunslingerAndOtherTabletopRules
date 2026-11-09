"""One original corpulent boar-headed winged Nalfeshnee body on the measured
free-hands rig.

Only measured bone names and project-authored geometry are exported. Native
meshes, materials, textures and animation clips remain runtime dependencies.

The silhouette is carried by BULK, which is what separates it from every
released outsider on this rig. The Vrock is lean with a wing span wider than
it is tall; the Glabrezu is a towering trunk with two huge pincers; this
creature is a pear-shaped mass far wider at the gut than at the shoulders,
with short limbs that the belly overhangs and with wings that are identifying
precisely by being too small for it.

The rig has two arm chains and no wing chains, so the split is deliberate and
recorded rather than hidden: the two arms ride the rig's real animated chains
and the hand claws ride its two digit chains, so the native grip animation
still opens and closes them, while the two wings are project geometry weighted
to the upper torso. The wings therefore move with the chest and do not
articulate independently. That is the same accepted shape the released Sprint
37 Vrock's wings take, and it is a recorded limitation rather than a claim of
wing articulation.
"""
# Owner visual overhaul: continuous organic skin, revised anatomy and albedo.
# R7 replaces the rounded figure and slab wings: a hunched boar-headed ape on the
# same joints, fused wing roots with fingered membranes, position-baked materials.
# Rig/wing articulation limits above remain; native aesthetic review is separate.

import argparse
import json
import math
from pathlib import Path
import sys

import bpy
import bmesh
import numpy as np
from mathutils import Vector

sys.dont_write_bytecode = True
parser = argparse.ArgumentParser()
parser.add_argument('--kind', choices=('nalfeshnee',), default='nalfeshnee')
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
    'hide': 'body', 'wing': 'membrane', 'tusk': 'beak', 'claw': 'beak',
    'eye': 'crest'}
DETAIL_COUNTS = {}


def build_body(bm, weights, uvs, rig, kind):
    at = lambda name: common.shared.head(rig, name)

    def mass(point, radii, bone, region='hide', rings=12, sides=18, axes=None):
        side, up, front = axes or (SIDE, UP, FRONT)
        common.ellipsoid(bm, weights, uvs, point, side, up, front, radii,
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

    # THE TRUNK: a hunched boar-headed ape. The gut still overhangs the legs,
    # but as one sagging apron below a narrower ribcage, a pectoral shelf and a
    # scapular hump, so the mass reads as anatomy and not as one balloon.
    common.sweep(bm, weights, uvs, [at(n) for n in TORSO[:-1]],
                 [.300, .310, .165, .140], [.245, .260, .150, .130],
                 list(TORSO[:-1]), up=FRONT, forward=UP)
    mass(pelvis + Vector((0, .115, -.060)), (.385, .365, .320), 'Torso_Lower')
    mass(pelvis + Vector((0, .155, -.250)), (.255, .135, .205), 'Torso_Lower')
    for sign in (-1, 1):
        mass(pelvis + Vector((sign*.290, -.020, .120)), (.115, .165, .185),
             'Torso_Lower')
    mass(upper + Vector((0, .045, .150)), (.315, .265, .270), 'Torso_Upper')
    mass(upper + Vector((0, -.165, .355)), (.395, .215, .255), 'Torso_Upper')
    mass(upper + Vector((0, -.075, .485)), (.240, .160, .205), 'neck')
    for sign in (-1, 1):
        mass(upper + Vector((sign*.170, .265, .205)), (.175, .118, .092),
             'Torso_Upper')
        # Heavy lats carry the shoulder down into the flank.
        mass(upper + Vector((sign*.300, -.040, .130)), (.115, .240, .165),
             'Torso_Upper')

    # A short bull neck carrying the head low and forward of the hump.
    tube([upper + Vector((0, -.030, .395)), at('neck1') + Vector((0, .030, 0)),
          head_bone + Vector((0, .090, -.040))],
         [.190, .168, .142], ['neck', 'neck1', 'Head'], 'hide', 16)

    # THE BOAR HEAD: wedge skull, heavy brow, long flat-ended snout, underslung
    # jaw. The rooting disc is cut flat; a taper would read as a dog.
    head = head_bone + Vector((0, .135, -.030))
    mass(head + Vector((0, -.030, .050)), (.165, .155, .175), 'Head')
    section([head + Vector((0, .040, .075)), head + Vector((0, .170, .032)),
             head + Vector((0, .285, -.012)), head + Vector((0, .345, -.024))],
            [.150, .124, .100, .092], [.128, .100, .076, .068], ['Head'] * 4)
    mass(head + Vector((0, .360, -.024)), (.095, .074, .028), 'Head')
    section([head + Vector((0, .010, -.120)), head + Vector((0, .165, -.150)),
             head + Vector((0, .300, -.150))],
            [.122, .094, .070], [.066, .052, .038], ['Head'] * 3)
    tusks = ears = 0
    for sign in (-1, 1):
        mass(head + Vector((sign*.122, .110, -.070)), (.068, .074, .105), 'Head')
        mass(head + Vector((sign*.086, .120, .090)), (.086, .034, .062), 'Head')
        # Lower tusks sweep out of the jaw corners past the snout.
        base = head + Vector((sign*.086, .185, -.145))
        tube([base, base + Vector((sign*.040, .095, .050)),
              base + Vector((sign*.066, .150, .165)),
              base + Vector((sign*.048, .128, .275))],
             [.038, .032, .021, .004], ['Head'] * 4, 'tusk', 10)
        upper_tusk = head + Vector((sign*.058, .275, -.060))
        tube([upper_tusk, upper_tusk + Vector((sign*.038, .058, -.058)),
              upper_tusk + Vector((sign*.074, .070, -.012))],
             [.021, .015, .003], ['Head'] * 3, 'tusk', 9)
        tusks += 2
        mass(head + Vector((sign*.124, .135, .035)), (.021, .018, .018),
             'Head', 'eye', 8, 12)
        ear_out = Vector((sign*.72, -.28, .64)).normalized()
        ear_flat = Vector((sign*.24, .94, .22)).normalized()
        ear_up = ear_flat.cross(ear_out).normalized()
        mass(head + Vector((sign*.170, -.095, .105)), (.098, .058, .020),
             'Head', 'hide', 9, 14, (ear_out, ear_up, ear_flat))
        ears += 1

    # A bristled mane from crown to mid-back: the second boar cue at game zoom.
    crest = [head + Vector((0, -.075, .150)), upper + Vector((0, -.135, .570)),
             upper + Vector((0, -.300, .560)), upper + Vector((0, -.400, .290))]
    for index in range(14):
        t = index / 13 * (len(crest) - 1)
        i = min(int(t), len(crest) - 2)
        root = crest[i].lerp(crest[i + 1], t - i)
        bone = 'Head' if index < 4 else 'neck' if index < 7 else 'Torso_Upper'
        lean = Vector(((-1) ** index * .018, -.032, .058))
        tube([root - lean * .55, root + lean * .55, root + lean * 1.20],
             [.028, .017, .002], [bone] * 3, 'claw', 7)

    # THE WINGS: vestigial and undersized for the body, but real wings. Each
    # root grows out of a scapular mound, then an arm and three spread fingers
    # hold a scalloped membrane. The rig has no wing chain: they follow the
    # chest, a recorded limitation.
    wing_panels = 0

    def path(points, s):
        x = s * (len(points) - 1)
        i = min(int(x), len(points) - 2)
        return points[i].lerp(points[i + 1], x - i)

    for sign in (-1, 1):
        scapula = upper + Vector((sign*.225, -.245, .435))
        mass(scapula, (.140, .140, .125), 'Torso_Upper')
        root = upper + Vector((sign*.205, -.235, .445))
        elbow = upper + Vector((sign*.420, -.430, .665))
        wrist = upper + Vector((sign*.585, -.470, .610))
        flank = upper + Vector((sign*.270, -.330, .070))
        tube([root, root.lerp(elbow, .55) + UP*.020, elbow, wrist],
             [.066, .046, .036, .026], ['Torso_Upper'] * 4, 'hide', 10)
        tips = [upper + Vector((sign*.770, -.450, .325)),
                upper + Vector((sign*.675, -.505, .045)),
                upper + Vector((sign*.475, -.470, -.115))]
        fingers = []
        for tip in tips:
            knuckle = wrist.lerp(tip, .52) + Vector((sign*.022, -.018, .015))
            finger = [wrist, knuckle, tip]
            tube(finger, [.022, .015, .004], ['Torso_Upper'] * 3, 'hide', 8)
            fingers.append(finger)
        hook = wrist + Vector((sign*.045, .040, .075))
        tube([wrist, hook], [.022, .003], ['Torso_Upper'] * 2, 'claw', 7)
        strips = [(fingers[0], fingers[1], 1.0), (fingers[1], fingers[2], 1.0),
                  (fingers[2], [wrist, elbow, root, flank], .78)]
        for near, far, scallop_scale in strips:
            grid = []
            for i in range(11):
                t = i / 10.0
                column = []
                for j in range(9):
                    s = j / 8.0
                    pull = .200 * scallop_scale * math.sin(math.pi * t) * s * s
                    reach = s * (1.0 - pull)
                    point = path(near, reach).lerp(path(far, reach), t)
                    point = point + FRONT * (-.034 * math.sin(math.pi * t) *
                                             math.sin(math.pi * s))
                    column.append(point)
                grid.append(column)
            panel, _ = surface.sheet('NalfeshneeWing', grid, .005)
            new = [bm.verts.new(v.co) for v in panel.data.vertices]
            for v in new:
                weights[v] = [('Torso_Upper', 1)]
                uvs[v] = common.shared.region_uv('membrane', .5, .5)
            for face in panel.data.polygons:
                bm.faces.new([new[i] for i in face.vertices])
            data = panel.data
            bpy.data.objects.remove(panel, do_unlink=True)
            bpy.data.meshes.remove(data)
        wing_panels += 1

    arms = claws = 0
    for side in ('L', 'R'):
        sign = 1 if side == 'L' else -1
        arm = [side + '_' + name for name in ARMS[:3]]
        shoulder, elbow, palm = [at(name) for name in arm]
        # Long heavy ape arms on the real chains, swollen at the forearm.
        tube([upper + Vector((sign*.190, 0, .270)), shoulder,
              shoulder.lerp(elbow, .45), elbow, elbow.lerp(palm, .38), palm],
             [.178, .172, .150, .112, .136, .084],
             ['Torso_Upper', arm[0], arm[0], arm[1], arm[1], arm[2]], 'hide', 16)
        mass(shoulder + Vector((sign*.030, 0, .018)), (.162, .190, .160), arm[0])
        muscle(common, bm, weights, uvs, shoulder, elbow, .140, .122, arm[0], 'body')
        muscle(common, bm, weights, uvs, elbow, elbow.lerp(palm, .78), .128, .112,
               arm[1], 'body')
        hand = palm + Vector((sign*.012, .012, -.042))
        mass(hand, (.098, .092, .096), arm[2])
        for digit, lift in (('finger', .030), ('Bfinger', -.030)):
            first, second = side + '_' + digit + '1', side + '_' + digit + '2'
            root, tip = at(first), at(second)
            direction = (tip - root).normalized()
            flesh_tip = tip + UP * lift * .7
            tube([hand + UP * lift * .4, root + UP * lift, flesh_tip],
                 [.052, .046, .034], [arm[2], first, second], 'hide', 11)
            tube([flesh_tip - direction * .012, flesh_tip + direction * .095],
                 [.030, .003], [second, second], 'claw', 9)
            claws += 1
        thumb = hand + Vector((sign*.082, .062, -.030))
        tube([hand, thumb], [.046, .034], [arm[2]] * 2, 'hide', 10)
        tube([thumb - Vector((0, 0, .006)),
              thumb + Vector((sign*.020, .060, -.040))],
             [.026, .003], [arm[2]] * 2, 'claw', 8)
        claws += 1
        arms += 1

        # Short bowed legs on the measured digitigrade joints.
        leg = [side + '_' + name for name in LEGS]
        hip, knee, ankle, toe = [at(name) for name in leg]
        tube([pelvis, hip, hip.lerp(knee, .50), knee],
             [.198, .228, .208, .150],
             ['Torso_Lower', leg[0], leg[0], leg[1]], 'hide', 16)
        mass(knee + Vector((0, .030, 0)), (.124, .112, .100), leg[1])
        tube([knee, knee.lerp(ankle, .36) - FRONT * .030, ankle,
              ankle.lerp(toe, .50), toe + UP * .030],
             [.126, .150, .104, .106, .104],
             [leg[1], leg[1], leg[2], leg[2], leg[3]], 'hide', 14)
        foot = toe + Vector((0, .072, .014))
        mass(foot, (.138, .052, .168), leg[3])
        for index in range(3):
            spread = (index - 1) * .082
            root = foot + Vector((spread * .8, .090, -.006))
            tube([foot + Vector((spread * .6, 0, 0)), root + FRONT * .058],
                 [.052, .040], [leg[3]] * 2, 'hide', 10)
            tube([root + FRONT * .044, root + Vector((spread * .08, .118, -.012))],
                 [.036, .004], [leg[3]] * 2, 'claw', 9)

    consolidate_skin(bm, weights, uvs, common.shared.ATLAS,
                     regions=('body',), voxel=.008, triangles=10500)
    bake.bake_model(bm, weights, uvs, common.shared.ATLAS, args.albedo, colour)

    DETAIL_COUNTS.update(wings=wing_panels, arms=arms, handClaws=claws,
                         tusks=tusks, ears=ears)


def colour(p, n, region, cavity):
    """Bind-pose paint: x lateral, y forward, z up. Original closed-form colour."""
    count = len(p)
    blotch = surface.fbm(p, 5.5, 11)
    grain = surface.fbm(p, 42.0, 23)
    x, y, z = p[:, 0], p[:, 1], p[:, 2]
    # Soot-dark back and limbs over a dull umber hide.
    hide = bake.mix(bake.rgb((.205, .160, .140), count),
                    bake.rgb((.375, .275, .215), count),
                    bake.smooth(.34, .68, blotch))
    # Sagging belly, chest and inner arms fade to a dusty, bruised flesh.
    front = bake.smooth(-.15, .55, n[:, 1]) * bake.smooth(.04, .26, y)
    gut = front * bake.smooth(.50, .62, z) * (1 - bake.smooth(1.38, 1.60, z))
    flesh = bake.mix(bake.rgb((.450, .355, .300), count),
                     bake.rgb((.360, .270, .260), count),
                     bake.smooth(.35, .70, surface.fbm(p, 9.0, 37)))
    skin = bake.mix(hide, flesh, gut * .92)
    # Darken toward hands and feet so the extremities separate from the gut.
    ends = np.maximum(1 - bake.smooth(.16, .46, z),
                      bake.smooth(.50, .72, np.abs(x)) * (1 - bake.smooth(1.0, 1.2, z)))
    skin = bake.mix(skin, bake.rgb((.125, .100, .098), count), ends * .72)
    # Back ridge and top of the skull run almost black under the mane.
    spine = (1 - bake.smooth(.05, .21, np.abs(x))) * (1 - bake.smooth(-.26, -.02, y))
    skin = bake.mix(skin, bake.rgb((.110, .090, .086), count), spine * .68)
    # Snout: pale fleshy muzzle, wet dark rooting disc, black lip line, nostrils.
    head_y, head_z = .247, 1.769
    muzzle = bake.smooth(head_y + .18, head_y + .33, y) * bake.smooth(1.60, 1.66, z)
    skin = bake.mix(skin, bake.rgb((.465, .305, .275), count), muzzle * .85)
    disc = bake.smooth(head_y + .345, head_y + .372, y) * bake.smooth(1.60, 1.66, z)
    skin = bake.mix(skin, bake.rgb((.265, .150, .150), count), disc)
    for sign in (-1.0, 1.0):
        dist = np.sqrt(((x - sign * .040) / .022) ** 2 +
                       ((z - (head_z - .015)) / .020) ** 2)
        skin = bake.mix(skin, bake.rgb((.025, .018, .018), count),
                        (1 - bake.smooth(.8, 1.3, dist)) *
                        bake.smooth(head_y + .34, head_y + .375, y))
    lip = (1 - bake.smooth(.006, .022, np.abs(z - (head_z - .098)))) * \
        bake.smooth(head_y + .05, head_y + .14, y)
    skin = bake.mix(skin, bake.rgb((.040, .028, .028), count), lip)
    skin *= (.90 + .20 * grain)[..., None]

    result = skin
    # Wing membrane: oxblood with paler radial veins, darkest near the body.
    veins = np.abs(np.sin(np.arctan2(z - 1.8, np.abs(x) - .58) * 15.0))
    membrane = bake.mix(bake.rgb((.215, .105, .100), count),
                        bake.rgb((.420, .245, .205), count),
                        (1 - bake.smooth(.04, .20, veins)) * .75 +
                        .25 * bake.smooth(.3, .75, blotch))
    # Tusks and claws: stained ivory. Mane bristles and foot claws run darker.
    tips = bake.mix(bake.rgb((.420, .345, .215), count),
                    bake.rgb((.790, .725, .560), count),
                    bake.smooth(.35, .70, surface.fbm(p, 16.0, 53)))
    darker = (y < .10) & (z > 1.2) & (np.abs(x) < .26)
    tips[darker] = tips[darker] * .22 + np.array((.03, .025, .025))
    feet = z < .34
    tips[feet] = tips[feet] * .62
    eye = bake.mix(bake.rgb((.780, .520, .100), count),
                   bake.rgb((.950, .800, .260), count),
                   bake.smooth(.3, .9, n[:, 1]))
    result = np.where((region == bake.MEMBRANE)[..., None], membrane, result)
    result = np.where((region == bake.BEAK)[..., None], tips, result)
    result = np.where((region == bake.CREST)[..., None], eye, result)
    # Baked cavity: the folds, armpits and neck crease carry the form even
    # under the game's flat summon lighting. Eyes keep their value.
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
    dict(visibleArms=2, visibleLegs=2, humanoidBody=True,
         jawSeparated=False, runtimeQualified=False))
path = Path(args.mesh_data)
payload = json.loads(path.read_text(encoding='utf-8'))
payload.update(
    creature=args.kind, nativeDonor=donor.DONOR_GUID,
    nativePrefab=donor.PREFAB_GUID, bodyRenderer=donor.BODY_RENDERER,
    accessoryRenderer=donor.ACCESSORY_RENDERER, nativeRendererCount=2,
    visibleArms=2, visibleLegs=2, humanoidBody=True,
    distinctSilhouette='corpulent-boar-headed-winged-demon',
    boarHead=True, tusked=True, corpulent=True,
    visibleWings=DETAIL_COUNTS['wings'],
    visibleHandClaws=DETAIL_COUNTS['handClaws'],
    visibleTusks=DETAIL_COUNTS['tusks'],
    visibleEars=DETAIL_COUNTS['ears'],
    # The hand claws ride the rig's two digit chains, so the native grip
    # animation opens and closes them.
    handClawsOnDigitChains=True,
    # The rig carries two arm chains and no wing chain. The arms ride them
    # and animate; the wings are weighted to the upper torso and do not.
    articulatedArmChains=2, articulatedArms=True,
    articulatedWings=False,
    jawSeparated=False, triangleWinding='authored-outward-sprint39',
    uvAtlas={'body': [0.0, 0.0, 1.0, 1.0]})
path.write_text(json.dumps(payload, indent=1) + '\n', encoding='utf-8',
                newline='\n')

scene = bpy.context.scene
scene.render.engine = 'BLENDER_EEVEE_NEXT'
scene.world = scene.world or bpy.data.worlds.new(
    'Sprint39NalfeshneePreviewWorld')
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
