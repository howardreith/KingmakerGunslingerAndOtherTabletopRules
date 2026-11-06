"""Original insectile Gelugon geometry on the measured free-hands rig.

Only original geometry/albedo and measured bone names are exported. The spear
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

SIDE, UP, FRONT = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, 1, 0))
TORSO = ('Torso_Lower', 'Torso_Upper', 'neck', 'neck1', 'Head')
ARMS = ('Arm_Upper', 'Arm_Lower', 'Palm', 'finger1', 'finger2', 'Bfinger1', 'Bfinger2')
LEGS = ('Leg0_Upper', 'Leg0_Lower', 'Foot0', 'toe')
BONES = TORSO + tuple(side + '_' + part for side in ('L', 'R')
                      for part in ARMS + LEGS)
TAIL = None


def build_body(bm, weights, uvs, rig, kind):
    global TAIL
    at = lambda name: common.shared.head(rig, name)

    def mass(point, radii, bone, region='body', rings=10, sides=14):
        common.ellipsoid(bm, weights, uvs, point, SIDE, UP, FRONT,
                         radii, bone, region, rings, sides)

    def tube(points, radii, names, region='limbs', sides=10):
        common.tube(bm, weights, uvs, points, radii, names, region, sides)

    pelvis, chest = at('Torso_Lower'), at('Torso_Upper')
    head = at('Head') + UP * .075 + FRONT * .055
    # A wasp-like waist and layered thorax, not the skeletal Bone Devil trunk.
    tube([pelvis, chest, at('neck'), at('neck1')],
         [.135, .24, .075, .065], list(TORSO[:-1]), 'membrane', 14)
    mass(pelvis - UP * .035, (.20, .16, .18), 'Torso_Lower')
    for level in range(5):
        z = -.11 + level * .085
        width = .145 + level * .018
        mass(chest + UP * z + FRONT * .22,
             (width, .055, .07), 'Torso_Upper', 'body', 8, 12)
    # Broad head with a projecting brow, enormous paired compound eyes and
    # four visibly separate mandible prongs. No human skull or horns.
    mass(at('neck'), (.10, .085, .095), 'neck')
    mass(at('neck1'), (.085, .075, .08), 'neck1')
    mass(head, (.145, .17, .115), 'Head', 'body', 10, 16)
    mass(head + UP * .095 + FRONT * .035, (.14, .065, .115), 'Head')
    for sign in (-1, 1):
        eye = head + SIDE * sign * .10 + FRONT * .095 + UP * .025
        mass(eye, (.080, .095, .062), 'Head', 'crest', 8, 12)
        # Low geometric lens relief survives the simple native material.
        for row in range(4):
            for column in range(4):
                x, z = (column - 1.5) * .027, (row - 1.5) * .031
                if x*x/.004 + z*z/.007 > 1:
                    continue
                mass(eye + SIDE * x + UP * z + FRONT * .051,
                     (.012, .014, .009), 'Head', 'crest', 4, 6)
        base = head + SIDE * sign * .075 + FRONT * .10 - UP * .065
        for offset in (-.035, .035):
            start = base + SIDE * offset
            tube([start, start + SIDE * sign * .045 + FRONT * .08 - UP * .05,
                  head + SIDE * sign * .03 + FRONT * .17 - UP * .17],
                 [.031, .025, .002], ['Head'] * 3, 'beak', 9)
        antenna = head + SIDE * sign * .08 + UP * .13
        tube([antenna, antenna + SIDE * sign * .07 + UP * .17,
              antenna + SIDE * sign * .13 + UP * .27 - FRONT * .06],
             [.014, .009, .002], ['Head'] * 3, 'body', 7)

    for side, sign in (('L', 1), ('R', -1)):
        arm = [side + '_' + name for name in ARMS[:3]]
        shoulder, elbow, palm = [at(name) for name in arm]
        tube([chest, shoulder, elbow, palm], [.095, .080, .045, .032],
             ['Torso_Upper', *arm], 'membrane', 10)
        mass(shoulder + SIDE * sign * .025 + UP * .035,
             (.13, .10, .11), arm[0])
        tube([shoulder + UP * .07, shoulder + SIDE * sign * .17 + UP * .13],
             [.075, .002], [arm[0]] * 2, 'body', 9)
        mass(elbow, (.06, .065, .06), arm[1])
        midpoint = elbow.lerp(palm, .50)
        mass(midpoint, (.055, .09, .05), arm[1])
        mass(palm, (.05, .07, .035), arm[2], 'body', 8, 12)
        for digit in ('finger', 'Bfinger'):
            first, second = side + '_' + digit + '1', side + '_' + digit + '2'
            end = at(second)
            direction = (end - at(first)).normalized()
            tube([palm, at(first), end, end + direction * .065],
                 [.019, .018, .012, .002], [arm[2], first, second, second],
                 'beak', 7)
        leg = [side + '_' + part for part in LEGS]
        points = [at(name) for name in leg]
        tube([pelvis, *points], [.105, .095, .058, .037, .03],
             ['Torso_Lower', *leg], 'membrane', 11)
        mass(points[0], (.10, .13, .095), leg[0])
        mass(points[1], (.075, .065, .06), leg[1])
        for offset in (-.035, .035):
            foot = points[-1] + SIDE * offset
            tube([foot, foot + FRONT * .12 - UP * .01], [.024, .002],
                 [leg[-1]] * 2, 'beak', 8)

    # Thick segmented tail curls laterally into the visible silhouette. It
    # follows the pelvis: independent tail choreography is disclosed backlog.
    points = [pelvis - FRONT * .06 - UP * .08,
              pelvis - FRONT * .22 + SIDE * .20 - UP * .10,
              pelvis - FRONT * .21 + SIDE * .46,
              pelvis - FRONT * .12 + SIDE * .70 + UP * .16,
              pelvis + FRONT * .02 + SIDE * .85 + UP * .38,
              pelvis + FRONT * .16 + SIDE * .86 + UP * .60,
              pelvis + FRONT * .24 + SIDE * .74 + UP * .77]
    before = set(bm.verts)
    tube(points, [.09, .088, .078, .067, .053, .037, .014],
         ['Torso_Lower'] * len(points), 'membrane', 12)
    for index, point in enumerate(points[1:-1], 1):
        r = .102 - index * .011
        mass(point, (r, r, r * .8), 'Torso_Lower', 'body', 8, 12)
    # A striking tail tip, not a scorpion poison gland or sting.
    mass(points[-1], (.025, .045, .02), 'Torso_Lower', 'body', 8, 10)
    bm.verts.index_update()
    TAIL = dict(bone='Torso_Lower', origin=list(points[0]), tip=list(points[-1]),
                segments=7, vertices=[v.index for v in bm.verts if v not in before])


def write_albedo(path):
    size = 512
    image = np.zeros((size, size, 3), dtype=np.float64)
    palette = {'body': (.39, .65, .76), 'crest': (.12, .48, .68),
               'beak': (.70, .82, .88), 'limbs': (.22, .43, .55),
               'membrane': (.045, .115, .16)}
    for region in sorted(paint.shared.ATLAS):
        (y0, y1, x0, x1), u, v = paint.shared.region_canvas(size, region)
        base = np.array(palette.get(region, palette['body']))
        pixels = np.broadcast_to(base, (*u.shape, 3)).copy()
        pattern = .86 + .12*np.sin(u*math.pi*12)*np.cos(v*math.pi*7)
        pattern += .025*np.sin(u*math.pi*47 + v*math.pi*31)
        image[y0:y1, x0:x1] = np.clip(pixels*pattern[..., None], 0, 1)
    Path(path).parent.mkdir(parents=True, exist_ok=True)
    paint.shared.write_png(path, image)


write_albedo(args.albedo)
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
    tailArticulatesIndependently=False, spearBakedIntoBody=False)
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
