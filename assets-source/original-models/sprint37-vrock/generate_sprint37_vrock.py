"""One original Vrock body on the measured free-hands rig.

Only measured bone names and project-authored geometry are exported. Native
meshes, materials, textures and animation clips remain runtime dependencies.

The rig carries no wing bones, so the wings are project-authored geometry
weighted to the upper torso: they move with the chest and do not articulate.
That is a deliberate, recorded limitation rather than a claim of flight
animation.
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
parser.add_argument('--kind', choices=('vrock',), default='vrock')
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
    'feather': 'body', 'wing': 'membrane', 'quill': 'crest',
    'bare': 'limbs', 'beak': 'beak', 'eye': 'crest', 'talon': 'beak'
}
DETAIL_COUNTS = {}


def build_body(bm, weights, uvs, rig, kind):
    at = lambda name: common.shared.head(rig, name)

    def mass(point, radii, bone, region='feather', rings=10, sides=16):
        common.ellipsoid(bm, weights, uvs, point, SIDE, UP, FRONT, radii,
                         bone, ATLAS_REGION.get(region, region), rings, sides)

    def tube(points, radii, names, region='feather', sides=10):
        common.tube(bm, weights, uvs, points, radii, names,
                    ATLAS_REGION.get(region, region), sides)

    pelvis = at('Torso_Lower')
    upper = at('Torso_Upper')
    neck = at('neck1')
    head_bone = at('Head')

    # An upright, deep-chested feathered trunk. Narrower than the Hezrou's
    # bulk and heavier than the Babau's, so neither silhouette is reused.
    common.sweep(bm, weights, uvs, [at(n) for n in TORSO[:-1]],
                 [.185, .215, .092, .068], [.150, .175, .078, .060],
                 list(TORSO[:-1]), up=FRONT, forward=UP)
    mass(pelvis - UP * .050 + FRONT * .010, (.175, .150, .150),
         'Torso_Lower')
    mass(upper + FRONT * .020, (.225, .170, .180), 'Torso_Upper')

    # The bare neck: a long thin bridge in bare-skin colour, which with the
    # beak is what makes this a vulture rather than a bird of prey.
    tube([neck - UP * .030, neck + FRONT * .030 + UP * .060,
          head_bone + FRONT * .045 + UP * .010],
         [.062, .052, .048], ['neck', 'neck1', 'Head'], 'bare', 12)

    # The ruff where bare neck meets plumage.
    for index, offset in enumerate((-.015, .020)):
        mass(neck - UP * (.055 - offset) - FRONT * .004,
             (.105 - index * .012, .095 - index * .012, .040),
             'neck', 'quill', 8, 16)

    # A small bald skull carried forward, with a long hooked beak.
    head = head_bone + FRONT * .070 + UP * .020
    mass(head, (.090, .105, .082), 'Head', 'bare')
    tube([head + FRONT * .055 + UP * .010,
          head + FRONT * .150 - UP * .022,
          head + FRONT * .205 - UP * .060],
         [.062, .040, .012], ['Head'] * 3, 'beak', 12)
    # The hook: a short downward spur under the tip of the upper mandible.
    tube([head + FRONT * .195 - UP * .048,
          head + FRONT * .196 - UP * .098],
         [.020, .004], ['Head'] * 2, 'beak', 8)
    tube([head + FRONT * .060 - UP * .030,
          head + FRONT * .165 - UP * .052],
         [.040, .014], ['Head'] * 2, 'bare', 10)
    for sign in (-1, 1):
        mass(head + SIDE * sign * .062 + FRONT * .040 + UP * .024,
             (.024, .022, .024), 'Head', 'eye', 7, 12)

    # Two rigid wings on the upper torso. Each is a swept leading-edge tube
    # with five tapering primaries hanging from it; the rig has no wing bone,
    # so everything here rides Torso_Upper and does not articulate.
    primaries = 0
    for sign in (-1, 1):
        root = upper + SIDE * sign * .150 - FRONT * .030 + UP * .040
        elbow = root + SIDE * sign * .330 - FRONT * .070 + UP * .235
        wrist = elbow + SIDE * sign * .390 - FRONT * .050 + UP * .060
        tube([root, elbow, wrist], [.062, .044, .022],
             ['Torso_Upper'] * 3, 'wing', 10)
        # Seven primaries, long enough that the span rather than the torso
        # carries the silhouette. The first pass dropped five of them a
        # tenth of a unit from a short leading edge and the wings read as
        # shoulder spikes rather than as wings.
        for index in range(7):
            t = index / 6.0
            base = root.lerp(elbow, t * 2.0) if t <= .5 else (
                elbow.lerp(wrist, (t - .5) * 2.0))
            drop = .230 + .300 * math.sin(math.pi * (0.22 + 0.62 * t))
            tip = base + Vector((sign * .070 * t, -.060 - .045 * t, -drop))
            tube([base, base.lerp(tip, .55), tip],
                 [.050, .038, .011], ['Torso_Upper'] * 3, 'wing', 8)
            primaries += 1

    for side in ('L', 'R'):
        arm = [side + '_' + name for name in ARMS[:3]]
        shoulder, elbow, palm = [at(name) for name in arm]
        tube([upper, shoulder, elbow, palm], [.098, .092, .058, .034],
             ['Torso_Upper', *arm], 'feather', 11)
        mass(shoulder, (.098, .090, .096), arm[0])
        mass(palm, (.046, .062, .030), arm[2], 'bare')
        for digit in ('finger', 'Bfinger'):
            first, second = side + '_' + digit + '1', side + '_' + digit + '2'
            direction = (at(second) - at(first)).normalized()
            for offset in (-.016, .016):
                shift = SIDE * offset
                tube([palm + shift, at(first) + shift, at(second) + shift,
                      at(second) + shift + direction * .095],
                     [.015, .014, .010, .002],
                     [arm[2], first, second, second], 'talon', 7)

        # Bird legs: a feathered thigh, a bare shin, and a foot that carries
        # three forward talons and one back spur.
        leg = [side + '_' + name for name in LEGS]
        points = [at(name) for name in leg]
        tube([pelvis, points[0], points[1]], [.118, .110, .070],
             ['Torso_Lower', leg[0], leg[1]], 'feather', 11)
        tube([points[1], points[2], points[3]], [.060, .036, .024],
             [leg[1], leg[2], leg[3]], 'bare', 10)
        foot = points[-1] + FRONT * .030
        mass(foot, (.040, .055, .024), leg[-1], 'bare', 8, 14)
        for index in range(3):
            spread = (index - 1) * .038
            tube([foot + SIDE * spread * .4,
                  foot + SIDE * spread + FRONT * .085 - UP * .012],
                 [.014, .003], [leg[-1]] * 2, 'talon', 7)
        tube([foot - FRONT * .018, foot - FRONT * .075 - UP * .010],
             [.012, .003], [leg[-1]] * 2, 'talon', 7)

    DETAIL_COUNTS.update(wings=2, primaries=primaries, talons=2)


def write_albedo(path):
    size = 512
    image = np.zeros((size, size, 3), dtype=np.float64)
    # Dull grey-green plumage over dull red-grey bare skin and a pale beak.
    # Nothing here repeats the Babau's slime green-black or the Hezrou's
    # olive-brown warts.
    palette = {
        'body': (.165, .185, .145), 'crest': (.105, .118, .095),
        'beak': (.50, .44, .30), 'limbs': (.355, .205, .185),
        'membrane': (.205, .225, .175)}
    for region in sorted(paint.shared.ATLAS):
        (y0, y1, x0, x1), u, v = paint.shared.region_canvas(size, region)
        base = np.array(palette.get(region, palette['body']))
        pixels = np.broadcast_to(base, (*u.shape, 3)).copy()
        # Long directional streaks read as feather groups rather than as the
        # Sprint 36 demons' scaled or warty mottle.
        pattern = .80 + .15 * np.maximum(0, np.sin(v * math.pi * 16 +
                                                   u * math.pi * 2)) ** 3
        pattern += .05 * np.cos(u * math.pi * 6)
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
    dict(visibleArms=2, visibleLegs=2, humanoidBody=True,
         jawSeparated=False, runtimeQualified=False))
path = Path(args.mesh_data)
payload = json.loads(path.read_text(encoding='utf-8'))
payload.update(
    creature=args.kind, nativeDonor=donor.DONOR_GUID,
    nativePrefab=donor.PREFAB_GUID, bodyRenderer=donor.BODY_RENDERER,
    accessoryRenderer=donor.ACCESSORY_RENDERER, nativeRendererCount=2,
    visibleArms=2, visibleLegs=2, humanoidBody=True,
    distinctSilhouette='vulture-headed-winged-demon',
    vultureHead=True, hookedBeak=True, rigidWings=True,
    visibleWings=DETAIL_COUNTS['wings'],
    visiblePrimaries=DETAIL_COUNTS['primaries'],
    visibleTalons=DETAIL_COUNTS['talons'],
    articulatedWingBones=False,
    jawSeparated=False, triangleWinding='authored-outward-sprint37')
path.write_text(json.dumps(payload, indent=1) + '\n', encoding='utf-8',
                newline='\n')

scene = bpy.context.scene
scene.render.engine = 'BLENDER_EEVEE_NEXT'
scene.world = scene.world or bpy.data.worlds.new('Sprint37VrockPreviewWorld')
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
