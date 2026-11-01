"""Original Babau and Hezrou bodies on the measured free-hands rig.

Only measured bone names and project-authored geometry are exported. Native
meshes, materials, textures, and animation clips remain runtime dependencies.
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
parser.add_argument('--kind', choices=('babau', 'hezrou'), required=True)
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
ARMS = ('Arm_Upper', 'Arm_Lower', 'Palm', 'finger1', 'finger2',
        'Bfinger1', 'Bfinger2')
LEGS = ('Leg0_Upper', 'Leg0_Lower', 'Foot0', 'toe')
BONES = TORSO + tuple(side + '_' + part for side in ('L', 'R')
                      for part in ARMS + LEGS)
ATLAS_REGION = {
    'hide': 'body', 'slime': 'membrane', 'belly': 'body', 'wart': 'crest',
    'claw': 'beak', 'horn': 'crest', 'mouth': 'beak', 'eye': 'limbs'
}
DETAIL_COUNTS = {}


def build_body(bm, weights, uvs, rig, kind):
    babau = kind == 'babau'
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

    if babau:
        # A narrow, long-limbed skirmisher with a forward skull and swept
        # horns. Thin surface lobes read as clinging slime without particles.
        common.sweep(bm, weights, uvs, [at(n) for n in TORSO[:-1]],
                     [.115, .145, .078, .060], [.085, .105, .060, .050],
                     list(TORSO[:-1]), up=FRONT, forward=UP)
        mass(pelvis - UP * .045, (.13, .10, .105), 'Torso_Lower')
        head = head_bone + FRONT * .070 + UP * .025
        mass(head, (.105, .145, .095), 'Head')
        mass(head + FRONT * .125 - UP * .025, (.072, .092, .052),
             'Head', 'mouth')
        for sign in (-1, 1):
            tube([head + SIDE * sign * .055 + UP * .060,
                  head + SIDE * sign * .115 - FRONT * .035 + UP * .165,
                  head + SIDE * sign * .145 - FRONT * .105 + UP * .205],
                 [.032, .018, .002], ['Head'] * 3, 'horn', 9)

        for side in ('L', 'R'):
            arm = [side + '_' + name for name in ARMS[:3]]
            shoulder, elbow, palm = [at(name) for name in arm]
            tube([upper, shoulder, elbow, palm], [.070, .067, .042, .026],
                 ['Torso_Upper', *arm], 'hide', 10)
            mass(shoulder, (.076, .070, .075), arm[0])
            mass(palm, (.038, .058, .026), arm[2])
            for digit in ('finger', 'Bfinger'):
                first, second = side + '_' + digit + '1', side + '_' + digit + '2'
                direction = (at(second) - at(first)).normalized()
                for offset in (-.014, .014):
                    shift = SIDE * offset
                    tube([palm + shift, at(first) + shift, at(second) + shift,
                          at(second) + shift + direction * .105],
                         [.014, .013, .009, .002],
                         [arm[2], first, second, second], 'claw', 7)

            leg = [side + '_' + name for name in LEGS]
            points = [at(name) for name in leg]
            tube([pelvis, *points], [.085, .080, .052, .030, .020],
                 ['Torso_Lower', *leg], 'hide', 10)
            mass(points[-1] + FRONT * .045, (.055, .035, .125),
                 leg[-1], 'claw')

        slime_anchors = [
            (upper + SIDE * -.105 + FRONT * .070, 'Torso_Upper', .085),
            (upper + SIDE * .100 - FRONT * .075 + UP * .040,
             'Torso_Upper', .070),
            (pelvis + SIDE * -.075 - FRONT * .075, 'Torso_Lower', .065),
            (neck + SIDE * .055 + FRONT * .055, 'neck1', .050),
        ]
        for origin, bone, length in slime_anchors:
            tube([origin, origin - UP * length + FRONT * .018],
                 [.027, .004], [bone, bone], 'slime', 8)
        DETAIL_COUNTS.update(horns=2, slimeLobes=len(slime_anchors), warts=0)
    else:
        # A squat, broad toad-demon silhouette: heavy belly and shoulders,
        # forward muzzle, eye bulbs, jowls, thick limbs, and visible warts.
        common.sweep(bm, weights, uvs, [at(n) for n in TORSO[:-1]],
                     [.245, .295, .155, .105], [.205, .245, .125, .090],
                     list(TORSO[:-1]), up=FRONT, forward=UP)
        mass(pelvis - UP * .065 + FRONT * .025, (.285, .235, .220),
             'Torso_Lower', 'belly', 12, 20)
        mass(upper + FRONT * .025, (.310, .235, .245),
             'Torso_Upper', 'hide', 12, 20)
        head = head_bone + FRONT * .095 - UP * .015
        mass(head, (.245, .205, .175), 'Head', 'hide', 12, 20)
        mass(head + FRONT * .175 - UP * .055, (.220, .125, .090),
             'Head', 'mouth', 10, 18)
        for sign in (-1, 1):
            mass(head + SIDE * sign * .145 + FRONT * .055 + UP * .125,
                 (.070, .065, .070), 'Head', 'eye', 9, 14)
            mass(head + SIDE * sign * .155 + FRONT * .105 - UP * .070,
                 (.095, .080, .075), 'Head', 'belly', 8, 14)

        for side in ('L', 'R'):
            arm = [side + '_' + name for name in ARMS[:3]]
            shoulder, elbow, palm = [at(name) for name in arm]
            mass(shoulder, (.150, .135, .150), arm[0])
            mass(elbow, (.125, .110, .125), arm[1])
            tube([upper, shoulder, elbow, palm], [.145, .140, .105, .065],
                 ['Torso_Upper', *arm], 'hide', 13)
            mass(palm, (.090, .105, .065), arm[2])
            for digit in ('finger', 'Bfinger'):
                first, second = side + '_' + digit + '1', side + '_' + digit + '2'
                direction = (at(second) - at(first)).normalized()
                for offset in (-.020, .020):
                    shift = SIDE * offset
                    tube([palm + shift, at(first) + shift, at(second) + shift,
                          at(second) + shift + direction * .075],
                         [.021, .019, .013, .003],
                         [arm[2], first, second, second], 'claw', 8)

            leg = [side + '_' + name for name in LEGS]
            points = [at(name) for name in leg]
            mass(points[0], (.165, .145, .160), leg[0])
            mass(points[1], (.135, .125, .135), leg[1])
            tube([pelvis, *points], [.175, .165, .125, .080, .050],
                 ['Torso_Lower', *leg], 'hide', 13)
            mass(points[-1] + FRONT * .035, (.115, .085, .145),
                 leg[-1], 'claw', 9, 16)

        wart_anchors = [
            (upper + SIDE * -.20 + FRONT * .12 + UP * .11, 'Torso_Upper'),
            (upper + SIDE * .16 - FRONT * .15 + UP * .06, 'Torso_Upper'),
            (upper + SIDE * .06 + FRONT * .20 + UP * .17, 'Torso_Upper'),
            (pelvis + SIDE * -.16 + FRONT * .16, 'Torso_Lower'),
            (pelvis + SIDE * .18 - FRONT * .11 - UP * .02, 'Torso_Lower'),
            (head + SIDE * -.11 + FRONT * .14 + UP * .10, 'Head'),
            (head + SIDE * .10 - FRONT * .10 + UP * .12, 'Head'),
        ]
        for origin, bone in wart_anchors:
            mass(origin, (.030, .026, .025), bone, 'wart', 6, 10)
        DETAIL_COUNTS.update(horns=0, slimeLobes=0, warts=len(wart_anchors))


def write_albedo(path):
    size = 512
    image = np.zeros((size, size, 3), dtype=np.float64)
    palette = {
        'babau': {
            'body': (.085, .12, .115), 'crest': (.055, .045, .035),
            'beak': (.025, .030, .028), 'limbs': (.22, .25, .17),
            'membrane': (.19, .32, .24)},
        'hezrou': {
            'body': (.19, .245, .11), 'crest': (.34, .29, .095),
            'limbs': (.47, .42, .15), 'beak': (.075, .055, .025),
            'membrane': (.14, .17, .07)}}[args.kind]
    for region in sorted(paint.shared.ATLAS):
        (y0, y1, x0, x1), u, v = paint.shared.region_canvas(size, region)
        base = np.array(palette.get(region, palette['body']))
        pixels = np.broadcast_to(base, (*u.shape, 3)).copy()
        if args.kind == 'babau':
            pattern = .78 + .18 * np.maximum(0, np.sin(u * math.pi * 10 +
                                                       v * math.pi * 3)) ** 4
            pattern += .06 * np.cos(v * math.pi * 11)
        else:
            pattern = .76 + .16 * np.maximum(0, np.cos(u * math.pi * 8) *
                                              np.sin(v * math.pi * 7)) ** 2
            pattern += .07 * np.cos(v * math.pi * 5)
        pixels *= pattern[..., None]
        image[y0:y1, x0:x1] = np.clip(pixels, 0, 1)
    Path(path).parent.mkdir(parents=True, exist_ok=True)
    paint.shared.write_png(path, image)


write_albedo(args.albedo)
rig_data, rig, rig_hash = donor.measured_rig(args.capture)
finish = common.finish_original_winding
common.finish_original_winding = lambda payload, kind: finish(payload, 'salamander')
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
    distinctSilhouette='lean-horned-skirmisher' if args.kind == 'babau'
                       else 'broad-squat-toad-bruiser',
    protectiveSlimeSurface=args.kind == 'babau',
    toadBody=args.kind == 'hezrou',
    visibleHorns=DETAIL_COUNTS['horns'],
    visibleSlimeLobes=DETAIL_COUNTS['slimeLobes'],
    visibleWarts=DETAIL_COUNTS['warts'],
    jawSeparated=False, triangleWinding='authored-outward-sprint36')
path.write_text(json.dumps(payload, indent=1) + '\n', encoding='utf-8',
                newline='\n')

scene = bpy.context.scene
scene.render.engine = 'BLENDER_EEVEE_NEXT'
scene.world = scene.world or bpy.data.worlds.new('Sprint36DemonPreviewWorld')
scene.world.color = (.10, .10, .10)
bpy.ops.object.camera_add(location=(3.4, 5.8, 3.0))
camera = bpy.context.object
camera.rotation_euler = (Vector((0, 0, .95)) - camera.location).to_track_quat('-Z', 'Y').to_euler()
camera.data.type = 'ORTHO'
camera.data.ortho_scale = 2.9 if args.kind == 'babau' else 3.2
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
