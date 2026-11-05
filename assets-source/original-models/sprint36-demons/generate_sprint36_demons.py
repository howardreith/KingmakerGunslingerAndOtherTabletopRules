"""Original Babau and Hezrou bodies on the measured free-hands rig.

Only measured bone names and project-authored geometry are exported. Native
meshes, materials, textures, and animation clips remain runtime dependencies.
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
ATLAS_REGION = {
    'hide': 'body', 'slime': 'membrane', 'belly': 'body', 'wart': 'body',
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
        mass(upper + UP*.240 + FRONT*.005, (.155,.180,.115), 'Torso_Upper')
        head = head_bone + FRONT * .070 + UP * .025
        tube([at('neck'),at('neck1')+FRONT*.030,
              head-FRONT*.025-UP*.045], [.055,.052,.055],
             ['neck','neck1','Head'],'hide',12)
        mass(head, (.105, .145, .095), 'Head')
        bevel_mass(common,bm,weights,uvs,head+FRONT*.025-UP*.075,
                   (.065,.043,.073),'Head','body',.75)
        mass(head + FRONT * .103 - UP * .060, (.060, .035, .034),
             'Head', 'mouth')
        for sign in (-1, 1):
            mass(head + SIDE*sign*.067 + FRONT*.080 + UP*.040, (.012,.009,.009), 'Head', 'eye', 6, 10)
            mass(head + SIDE*sign*.060 + FRONT*.055 - UP*.014, (.033,.040,.040), 'Head')
            tube([head + SIDE * sign * .055 + UP * .060,
                  head + SIDE * sign * .115 - FRONT * .035 + UP * .165,
                  head + SIDE * sign * .145 - FRONT * .105 + UP * .205],
                 [.032, .018, .002], ['Head'] * 3, 'horn', 9)

        for side in ('L', 'R'):
            arm = [side + '_' + name for name in ARMS[:3]]
            shoulder, elbow, palm = [at(name) for name in arm]
            tube([upper+UP*.19, shoulder, shoulder.lerp(elbow,.45), elbow,
                  elbow.lerp(palm,.30)-FRONT*.012, palm],
                 [.060,.057,.061,.040,.050,.026],
                 ['Torso_Upper',arm[0],arm[0],arm[1],arm[1],arm[2]], 'hide', 12)
            muscle(common,bm,weights,uvs,shoulder,elbow,.061,.047,arm[0],'body')
            muscle(common,bm,weights,uvs,elbow.lerp(palm,.15),palm,.039,.030,arm[1],'body')
            mass(shoulder+UP*.012, (.055, .075, .044), arm[0])
            tube([elbow+FRONT*.018,palm+FRONT*.016], [.017,.010],
                 [arm[1],arm[2]],'hide',7)
            tube([palm-UP*.035,palm,palm+FRONT*.045], [.024,.041,.030],
                 [arm[2]]*3,'hide',10)
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
            thigh,knee,ankle,toe=points
            tube([pelvis,thigh,thigh.lerp(knee,.50),knee],
                 [.092,.083,.075,.045],['Torso_Lower',leg[0],leg[0],leg[1]],'hide',12)
            tube([knee,knee.lerp(ankle,.30)-FRONT*.014,ankle,
                  ankle.lerp(toe,.40)+FRONT*.015,toe],
                 [.043,.063,.034,.045,.028],
                 [leg[1],leg[1],leg[2],leg[2],leg[3]],'hide',12)
            tube([knee+FRONT*.027,ankle+FRONT*.018,toe+FRONT*.045],
                 [.014,.012,.007],[leg[1],leg[2],leg[3]],'hide',7)
            foot=toe+UP*.020+FRONT*.045
            bevel_mass(common,bm,weights,uvs,foot,(.057,.032,.085),leg[-1],'body')
            for digit in range(3):
                spread=(digit-1)*.037
                base=foot+SIDE*spread*.6+FRONT*.032
                tip=foot+SIDE*spread+FRONT*(.15-.018*abs(digit-1))-UP*.012
                tube([base,base.lerp(tip,.60),tip],[.018,.014,.003],
                     [leg[-1]]*3,'hide',8)

        for sign in (-1, 1):
            for index in range(4):
                z = .025 + index*.040
                tube([upper + UP*.260 + SIDE*sign*.025 + FRONT*.095 - UP*z,
                      upper + UP*.260 + SIDE*sign*.105 + FRONT*.105 - UP*(z+.025),
                      upper + UP*.260 + SIDE*sign*.135 + FRONT*.045 - UP*(z+.045)],
                     [.014,.016,.009], ['Torso_Upper']*3, 'hide', 8)
            mass(pelvis+SIDE*sign*.095+FRONT*.055, (.065,.070,.045), 'Torso_Lower')

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
        mass(upper + UP*.250 - FRONT*.025, (.335,.265,.255), 'Torso_Upper')
        head = head_bone + FRONT * .110 - UP * .120
        # The blunt skull, jutting mandible and sloped brow are deliberately
        # planar masses. A rounded head and oval black lip read as a toy frog.
        bevel_mass(common,bm,weights,uvs,head,(.255,.145,.185),'Head','body')
        bevel_mass(common,bm,weights,uvs,head-UP*.100+FRONT*.100,
                   (.275,.070,.205),'Head','body',.96)
        bevel_mass(common,bm,weights,uvs,head+FRONT*.282-UP*.040,
                   (.192,.018,.014),'Head','beak',.95)
        for sign in (-1, 1):
            bevel_mass(common,bm,weights,uvs,
                       head+SIDE*sign*.155+FRONT*.112+UP*.095,
                       (.105,.045,.105),'Head','body')
            mass(head + SIDE * sign * .180 + FRONT * .208 + UP * .045,
                 (.027, .017, .018), 'Head', 'eye', 8, 12)
            muscle(common,bm,weights,uvs,
                   head+SIDE*sign*.215-FRONT*.020-UP*.030,
                   head+SIDE*sign*.200+FRONT*.170-UP*.080,
                   .075,.065,'Head','body')

        for side in ('L', 'R'):
            arm = [side + '_' + name for name in ARMS[:3]]
            shoulder, elbow, palm = [at(name) for name in arm]
            tube([upper+UP*.18,shoulder,shoulder.lerp(elbow,.45),elbow,
                  elbow.lerp(palm,.28),palm], [.130,.123,.145,.090,.115,.070],
                 ['Torso_Upper',arm[0],arm[0],arm[1],arm[1],arm[2]],'hide',14)
            muscle(common,bm,weights,uvs,shoulder,elbow,.140,.108,arm[0],'body')
            muscle(common,bm,weights,uvs,elbow.lerp(palm,.05),palm,.098,.075,arm[1],'body')
            bevel_mass(common,bm,weights,uvs,palm+FRONT*.015,
                       (.084,.068,.070),arm[2],'body')
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
            thigh,knee,ankle,toe=points
            tube([pelvis,thigh,thigh.lerp(knee,.4),knee],
                 [.165,.150,.145,.105],['Torso_Lower',leg[0],leg[0],leg[1]],'hide',14)
            muscle(common,bm,weights,uvs,thigh,knee,.153,.130,leg[0],'body')
            # A weight-bearing calf/Achilles rather than a thick thigh
            # ending abruptly in a straight stick and dark round foot.
            tube([knee,knee.lerp(ankle,.30)-FRONT*.028,ankle,
                  ankle.lerp(toe,.42)+FRONT*.035,toe],
                 [.103,.128,.090,.108,.078],
                 [leg[1],leg[1],leg[2],leg[2],leg[3]],'hide',14)
            foot=toe+UP*.035+FRONT*.065
            bevel_mass(common,bm,weights,uvs,foot,(.120,.055,.130),leg[-1],'body',.98)
            for digit in range(3):
                spread=(digit-1)*.085
                base=foot+SIDE*spread*.65+FRONT*.045
                tip=foot+SIDE*spread+FRONT*(.190-.025*abs(digit-1))-UP*.018
                tube([base,base.lerp(tip,.65),tip],[.041,.034,.012],
                     [leg[-1]]*3,'hide',10)
                tube([tip-FRONT*.018,tip+FRONT*.045-UP*.006],
                     [.017,.002],[leg[-1]]*2,'claw',7)

        for index in range(9):
            x=(index-4)*.039
            tube([head + SIDE*x + FRONT*.291 - UP*.068,
                  head + SIDE*x + FRONT*.300 - UP*(.026 if index%2 else .036)],
                 [.010,.001], ['Head']*2, 'horn', 6)
        # Broad, shallow dorsal glands and lateral jaw creases.
        for sign in (-1,1):
            mass(upper + SIDE*sign*.15 - FRONT*.17 + UP*.08, (.11,.14,.08), 'Torso_Upper')
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

    consolidate_skin(bm, weights, uvs, common.shared.ATLAS,
                     regions=('body',), voxel=.007, triangles=8000)


def write_albedo(path):
    size = 512
    image = np.zeros((size, size, 3), dtype=np.float64)
    palette = {
        'babau': {
            'body': (.255, .17, .16), 'crest': (.38, .295, .225),
            'beak': (.13, .095, .085), 'limbs': (.42, .29, .18),
            'membrane': (.24, .28, .22)},
        'hezrou': {
            'body': (.36, .405, .235), 'crest': (.45, .38, .19),
            'limbs': (.63, .50, .25), 'beak': (.155, .105, .060),
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
