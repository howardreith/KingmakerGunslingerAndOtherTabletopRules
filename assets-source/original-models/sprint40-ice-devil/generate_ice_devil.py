"""Original insectile Gelugon geometry on the measured free-hands rig.

Only original geometry/albedo and measured bone names are exported. R7 fuses
the carapace, under-body, joints and tail into one skin with tapered limb
sheaths, a reframed head and position-baked materials. The spear
is the actual equipped native weapon, never geometry baked into this body.
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
parser.add_argument('--kind', default='ice-devil', choices=('ice-devil',))
parser.add_argument('--pipeline-root', type=Path,
                    default=Path(__file__).resolve().parents[1])
for name in ('capture', 'albedo', 'mesh-data', 'report', 'blend-out', 'preview'):
    parser.add_argument('--' + name, required=True)
args = parser.parse_args(sys.argv[sys.argv.index('--') + 1:])
for name in ('capture', 'albedo', 'mesh_data', 'report', 'blend_out', 'preview'):
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

SIDE, UP, FRONT = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, 1, 0))
TORSO = ('Torso_Lower', 'Torso_Upper', 'neck', 'neck1', 'Head')
ARMS = ('Arm_Upper', 'Arm_Lower', 'Palm', 'finger1', 'finger2', 'Bfinger1', 'Bfinger2')
LEGS = ('Leg0_Upper', 'Leg0_Lower', 'Foot0', 'toe')
BONES = TORSO + tuple(side + '_' + part for side in ('L', 'R')
                      for part in ARMS + LEGS)
TAIL = None
ATLAS_REGION = {'carapace': 'body', 'joint': 'limbs', 'under': 'limbs',
                'claw': 'beak', 'eye': 'crest'}


def build_body(bm, weights, uvs, rig, kind):
    global TAIL
    at = lambda name: common.shared.head(rig, name)

    def mass(point, radii, bone, region='carapace', rings=12, sides=16):
        common.ellipsoid(bm, weights, uvs, point, SIDE, UP, FRONT,
                         radii, bone, ATLAS_REGION.get(region, region), rings, sides)

    def tube(points, radii, names, region='under', sides=12):
        common.tube(bm, weights, uvs, points, radii, names,
                    ATLAS_REGION.get(region, region), sides)

    def spindle(a, b, width, bone_a, bone_b):
        """A limb sheath: slim at both joints, full at the middle, so limbs
        read as jointed armour and not as rods strung with beads."""
        tube([a, a.lerp(b, .22), a.lerp(b, .55), a.lerp(b, .84), b],
             [width * .50, width * .88, width, width * .72, width * .42],
             [bone_a, bone_a, bone_a, bone_b, bone_b], 'carapace', 12)

    pelvis, chest = at('Torso_Lower'), at('Torso_Upper')
    head = at('Head') + UP * .075 + FRONT * .055
    # One continuous insectile trunk: full thorax, a preserved wasp waist, then
    # a dorsal shell that wraps the back and overlaps the ventral plates, so
    # the pale carapace and dark under-body read as one armoured creature.
    tube([pelvis, pelvis.lerp(chest, .55), chest, at('neck'), at('neck1')],
         [.135, .118, .232, .090, .070], list(('Torso_Lower', 'Torso_Upper',
         'Torso_Upper', 'neck', 'neck1')), 'under', 16)
    mass(pelvis - UP * .030, (.195, .150, .175), 'Torso_Lower')
    mass(chest + UP * .120 - FRONT * .060, (.265, .265, .205), 'Torso_Upper')
    mass(chest + UP * .285 - FRONT * .030, (.205, .135, .165), 'Torso_Upper')
    for level in range(5):
        z = -.11 + level * .085
        width = .150 + level * .018
        mass(chest + UP * z + FRONT * .185,
             (width, .056, .080), 'Torso_Upper', 'carapace', 9, 16)
    # Keel ridge down the back: five overlapping dorsal plates.
    for level in range(5):
        z = -.10 + level * .105
        mass(chest + UP * z - FRONT * .190, (.128 + level * .010, .070, .070),
             'Torso_Upper', 'carapace', 9, 14)
    # Neck collar overlapping the head join.
    mass(at('neck') + UP * .010, (.118, .070, .108), 'neck')
    mass(at('neck1'), (.092, .080, .086), 'neck1')

    # Head: narrower wedge under a heavy brow. The compound eyes stay the
    # identity but sit into the skull; four mandible prongs stay separate.
    mass(head, (.128, .150, .125), 'Head', 'carapace', 12, 18)
    mass(head + UP * .085 + FRONT * .045, (.135, .050, .110), 'Head')
    mass(head + FRONT * .105 - UP * .070, (.080, .058, .080), 'Head')
    for sign in (-1, 1):
        eye = head + SIDE * sign * .092 + FRONT * .092 + UP * .018
        mass(eye, (.070, .083, .056), 'Head', 'eye', 8, 12)
        for row in range(4):
            for column in range(4):
                x, z = (column - 1.5) * .024, (row - 1.5) * .027
                if x*x/.0032 + z*z/.0056 > 1:
                    continue
                mass(eye + SIDE * x + UP * z + FRONT * .046,
                     (.010, .012, .008), 'Head', 'eye', 4, 6)
        # Cheek plate frames the eye and roots the mandibles.
        mass(head + SIDE * sign * .088 + FRONT * .058 - UP * .070,
             (.062, .062, .070), 'Head')
        base = head + SIDE * sign * .062 + FRONT * .135 - UP * .082
        for offset, length in ((-.028, 1.0), (.028, .78)):
            start = base + SIDE * offset
            mid = start + SIDE * sign * .050 + FRONT * .082 * length - UP * .046
            tip = head + SIDE * sign * .018 + FRONT * (.135 + .105 * length) - \
                UP * (.100 + .090 * length)
            tube([start - FRONT * .024, start, mid, tip],
                 [.036, .032, .025, .002], ['Head'] * 4, 'claw', 10)
        antenna = head + SIDE * sign * .080 + UP * .120
        tube([antenna - UP * .020, antenna + SIDE * sign * .070 + UP * .170,
              antenna + SIDE * sign * .130 + UP * .270 - FRONT * .060],
             [.016, .010, .002], ['Head'] * 3, 'carapace', 8)

    for side, sign in (('L', 1), ('R', -1)):
        arm = [side + '_' + name for name in ARMS[:3]]
        shoulder, elbow, palm = [at(name) for name in arm]
        tube([chest, chest.lerp(shoulder, .55), shoulder], [.105, .092, .080],
             ['Torso_Upper', 'Torso_Upper', arm[0]], 'under', 12)
        # Pauldron: one curved plate with the accepted spike, fused to the shell.
        mass(shoulder + SIDE * sign * .020 + UP * .035, (.135, .105, .118), arm[0])
        tube([shoulder + UP * .070, shoulder + SIDE * sign * .090 + UP * .110,
              shoulder + SIDE * sign * .175 + UP * .130],
             [.070, .040, .003], [arm[0]] * 3, 'carapace', 10)
        spindle(shoulder, elbow, .078, arm[0], arm[1])
        mass(elbow, (.056, .058, .056), arm[1], 'joint')
        spindle(elbow, palm, .068, arm[1], arm[2])
        mass(palm, (.052, .070, .040), arm[2], 'carapace', 9, 12)
        for digit in ('finger', 'Bfinger'):
            first, second = side + '_' + digit + '1', side + '_' + digit + '2'
            end = at(second)
            direction = (end - at(first)).normalized()
            tube([palm, at(first), end, end + direction * .065],
                 [.022, .020, .013, .002], [arm[2], first, second, second],
                 'claw', 8)

        leg = [side + '_' + part for part in LEGS]
        hip, knee, ankle, toe = [at(name) for name in leg]
        tube([pelvis, hip], [.112, .100], ['Torso_Lower', leg[0]], 'under', 12)
        mass(hip, (.102, .125, .098), leg[0])
        spindle(hip, knee, .098, leg[0], leg[1])
        mass(knee + FRONT * .012, (.074, .066, .062), leg[1], 'joint')
        spindle(knee, ankle, .072, leg[1], leg[2])
        mass(ankle, (.046, .046, .046), leg[2], 'joint')
        spindle(ankle, toe, .054, leg[2], leg[3])
        for offset in (-.035, .035):
            foot = toe + SIDE * offset
            tube([foot, foot + FRONT * .12 - UP * .01], [.026, .002],
                 [leg[-1]] * 2, 'claw', 8)

    # The accepted tail curve, now tapering rings on a continuous core. It
    # follows the pelvis: independent tail choreography stays disclosed backlog.
    points = [pelvis - FRONT * .06 - UP * .08,
              pelvis - FRONT * .22 + SIDE * .20 - UP * .10,
              pelvis - FRONT * .21 + SIDE * .46,
              pelvis - FRONT * .12 + SIDE * .70 + UP * .16,
              pelvis + FRONT * .02 + SIDE * .85 + UP * .38,
              pelvis + FRONT * .16 + SIDE * .86 + UP * .60,
              pelvis + FRONT * .24 + SIDE * .74 + UP * .77]
    tube(points, [.092, .088, .078, .067, .053, .037, .016],
         ['Torso_Lower'] * len(points), 'under', 14)
    for index, point in enumerate(points[1:-1], 1):
        tangent = (points[index + 1] - points[index - 1]).normalized()
        r = .128 - index * .012
        for step in (-.26, .22):
            ring = point.lerp(points[index + (1 if step > 0 else -1)], abs(step))
            mass(ring, (r, r, r * .62), 'Torso_Lower', 'carapace', 9, 14)
    mass(points[-1], (.026, .046, .021), 'Torso_Lower', 'carapace', 9, 12)
    TAIL = dict(bone='Torso_Lower', origin=list(points[0]), tip=list(points[-1]),
                segments=7)

    consolidate_skin(bm, weights, uvs, common.shared.ATLAS,
                     regions=('body', 'limbs'), voxel=.0065, triangles=12000)
    bake.bake_model(bm, weights, uvs, common.shared.ATLAS, args.albedo, colour)


def colour(p, n, region, cavity):
    """Bind-pose paint: x lateral, y forward, z up. Original closed-form colour."""
    count = len(p)
    grain = surface.fbm(p, 46.0, 131)
    frost = surface.fbm(p, 13.0, 137)
    x, y, z = p[:, 0], p[:, 1], p[:, 2]
    # Pale cold shell over a dark slate-blue under-body. The shell falls toward
    # slate in every recess, so plate edges melt into the dark rather than ending hard.
    pale = bake.mix(bake.rgb((.555, .700, .770), count),
                    bake.rgb((.800, .885, .915), count),
                    bake.smooth(.34, .72, frost))
    under = bake.mix(bake.rgb((.070, .125, .180), count),
                     bake.rgb((.170, .265, .335), count),
                     bake.smooth(.3, .7, frost))
    exposure = bake.smooth(.30, .66, cavity)
    skin = bake.mix(under, pale, exposure)
    # Thin frost veining across the shell.
    vein = 1 - bake.smooth(.0, .040, np.abs(frost - .50))
    skin = bake.mix(skin, bake.rgb((.300, .420, .500), count), vein * exposure * .42)
    skin *= (.93 + .14 * grain)[..., None]
    joint = bake.mix(bake.rgb((.075, .130, .185), count),
                     bake.rgb((.205, .300, .370), count),
                     bake.smooth(.30, .75, frost) * bake.smooth(.20, .80, cavity))
    claw = bake.mix(bake.rgb((.520, .660, .740), count),
                    bake.rgb((.860, .935, .960), count),
                    bake.smooth(.35, .75, surface.fbm(p, 20.0, 149)))
    eye = bake.mix(bake.rgb((.050, .270, .470), count),
                   bake.rgb((.230, .660, .900), count),
                   bake.smooth(.1, .9, n[:, 1]))
    result = np.where((region == bake.LIMBS)[..., None], joint, skin)
    result = np.where((region == bake.BEAK)[..., None], claw, result)
    result = np.where((region == bake.CREST)[..., None], eye, result)
    shade = .50 + .50 * bake.smooth(.10, .70, cavity)
    shade = np.where(region == bake.CREST, 1.0, shade)
    return result * shade[..., None]


rig_data, rig, rig_hash = donor.measured_rig(args.capture)
finish = common.finish_original_winding
common.finish_original_winding = lambda payload, kind: finish(payload, 'salamander')
common.write_prototype(args, rig_data, rig, rig_hash, build_body, BONES,
    'lizardfolk-free-hands', 'donor renderer local; +X lateral, +Y forward, +Z up',
    dict(visibleArms=2, visibleLegs=2, humanoidBody=True,
         jawSeparated=False, runtimeQualified=False))
path = Path(args.mesh_data)
payload = json.loads(path.read_text(encoding='utf-8'))
payload.update(creature='ice-devil', nativeDonor=donor.DONOR_GUID,
    nativePrefab=donor.PREFAB_GUID, bodyRenderer=donor.BODY_RENDERER,
    accessoryRenderer=donor.ACCESSORY_RENDERER, nativeRendererCount=2,
    visibleArms=2, visibleLegs=2, visibleWings=0, humanoidBody=True,
    distinctSilhouette='insectile-compound-eyed-segmented-gelugon',
    compoundEyes=2, antennae=2, mandibles=4, tail=TAIL,
    tailArticulatesIndependently=False, spearBakedIntoBody=False,
    uvAtlas={'body': [0.0, 0.0, 1.0, 1.0]})
path.write_bytes((json.dumps(payload, separators=(',', ':'))+'\n').encode('utf-8'))

# Use the established private Blender preview path, not extracted game art.
bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
scene = bpy.context.scene
scene.render.engine = 'BLENDER_EEVEE_NEXT'
bpy.ops.object.camera_add(location=(2.4, 3.9, 2.1))
camera = bpy.context.object
target = Vector((0, 0, .85))
camera.rotation_euler = (target-camera.location).to_track_quat('-Z', 'Y').to_euler()
camera.data.type = 'ORTHO'
camera.data.ortho_scale = 2.9
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
