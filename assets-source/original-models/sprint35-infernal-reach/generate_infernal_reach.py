"""Original Barbed and Bone Devil bodies on the measured free-hands rig.

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
parser.add_argument('--kind', choices=('barbed-devil', 'bone-devil'), required=True)
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
TAIL = None
SPINES = 0
ATLAS_REGION = {
    'shell': 'crest', 'head': 'body', 'bone': 'limbs', 'claw': 'beak',
    'spine': 'crest', 'horn': 'crest', 'wing': 'membrane',
    'tail': 'body', 'stinger': 'beak'
}


def build_body(bm, weights, uvs, rig, kind):
    global TAIL, SPINES
    barbed = kind == 'barbed-devil'
    at = lambda name: common.shared.head(rig, name)

    def mass(point, radii, bone, region='body', rings=10, sides=16):
        common.ellipsoid(bm, weights, uvs, point, SIDE, UP, FRONT, radii,
                         bone, ATLAS_REGION.get(region, region), rings, sides)

    def tube(points, radii, names, region='body', sides=10):
        common.tube(bm, weights, uvs, points, radii, names,
                    ATLAS_REGION.get(region, region), sides)

    pelvis = at('Torso_Lower')
    upper = at('Torso_Upper')
    head = at('Head') + UP * (.055 if barbed else .08) + FRONT * .02
    common.sweep(bm, weights, uvs, [at(n) for n in TORSO[:-1]],
                 [.205 if barbed else .13, .265 if barbed else .16,
                  .10 if barbed else .075, .08 if barbed else .06],
                 [.15 if barbed else .095, .18 if barbed else .11,
                  .085 if barbed else .06, .07 if barbed else .05],
                 list(TORSO[:-1]), up=FRONT, forward=UP)
    mass(pelvis - UP * .055, (.21 if barbed else .13,
                              .16 if barbed else .10,
                              .14 if barbed else .09),
         'Torso_Lower', 'shell')
    mass(head, (.13 if barbed else .105, .16 if barbed else .14,
                .115 if barbed else .09), 'Head', 'head')
    mass(head + FRONT * .105 - UP * .015,
         (.075 if barbed else .055, .055, .05), 'Head', 'head')

    # A massive, thorned silhouette for Hamatula; a narrow, jointed and
    # visibly exoskeletal silhouette for Osyluth. Both retain the donor only
    # as an animation carrier.
    for side, sign in (('L', 1), ('R', -1)):
        arm = [side + '_' + name for name in ARMS[:3]]
        shoulder, elbow, palm = [at(name) for name in arm]
        if barbed:
            mass(shoulder, (.12, .13, .125), arm[0], 'shell')
            tube([upper, shoulder, elbow, palm],
                 [.115, .105, .078, .05],
                 ['Torso_Upper', *arm], 'body', 12)
        else:
            mass(shoulder, (.075, .08, .08), arm[0], 'shell')
            mass(elbow, (.065, .06, .065), arm[1], 'shell')
            tube([upper, shoulder, elbow, palm],
                 [.055, .055, .038, .027],
                 ['Torso_Upper', *arm], 'bone', 9)
        mass(palm, (.055 if barbed else .035, .075 if barbed else .05,
                    .045 if barbed else .027), arm[2], 'shell')
        for digit in ('finger', 'Bfinger'):
            first, second = side + '_' + digit + '1', side + '_' + digit + '2'
            direction = (at(second) - at(first)).normalized()
            for offset in (-.016, .016):
                shift = SIDE * offset
                tube([palm + shift, at(first) + shift, at(second) + shift,
                      at(second) + shift + direction * (.12 if barbed else .07)],
                     [.018 if barbed else .012, .018 if barbed else .012,
                      .012 if barbed else .008, .002],
                     [arm[2], first, second, second], 'claw', 7)

        leg_names = [side + '_' + name for name in LEGS]
        leg_points = [at(name) for name in leg_names]
        if barbed:
            tube([pelvis, *leg_points], [.145, .13, .105, .065, .038],
                 ['Torso_Lower', *leg_names], 'body', 12)
        else:
            tube([pelvis, *leg_points], [.075, .06, .043, .032, .022],
                 ['Torso_Lower', *leg_names], 'bone', 9)
            for point, bone, radius in zip(leg_points[:3], leg_names[:3],
                                           (.08, .07, .055)):
                mass(point, (radius, radius * .8, radius), bone, 'shell', 8, 12)
        mass(leg_points[-1] + FRONT * .05,
             (.08 if barbed else .055, .045 if barbed else .025,
              .15 if barbed else .11), leg_names[-1], 'claw')

    if barbed:
        # Horns, cheek hooks, and four rows of rigid body thorns. The spikes
        # are ordinary skinned geometry; no physics or attack consumer exists.
        for sign in (-1, 1):
            tube([head + SIDE * sign * .075 + UP * .08,
                  head + SIDE * sign * .15 + UP * .20 - FRONT * .025,
                  head + SIDE * sign * .115 + UP * .32 + FRONT * .03],
                 [.05, .028, .002], ['Head'] * 3, 'spine', 10)
            tube([head + SIDE * sign * .10 + FRONT * .04,
                  head + SIDE * sign * .20 + FRONT * .08 - UP * .03],
                 [.035, .002], ['Head'] * 2, 'spine', 9)
        anchors = [
            (upper + SIDE * x + FRONT * y + UP * z, 'Torso_Upper',
             (SIDE * x + FRONT * y + UP * (.35 + z)).normalized(), length)
            for x, y, z, length in
            ((-.19, 0, .02, .24), (.19, 0, .02, .24),
             (-.15, -.10, -.02, .20), (.15, -.10, -.02, .20),
             (-.11, .11, .05, .18), (.11, .11, .05, .18),
             (0, -.14, .08, .22), (0, .15, .04, .18))]
        for side, sign in (('L', 1), ('R', -1)):
            for bone_name, outward in ((side + '_Arm_Upper', SIDE * sign),
                                       (side + '_Arm_Lower', SIDE * sign),
                                       (side + '_Leg0_Upper', SIDE * sign),
                                       (side + '_Leg0_Lower', SIDE * sign)):
                anchors.append((at(bone_name), bone_name,
                                (outward + UP * .35 - FRONT * .15).normalized(),
                                .16))
        for origin, bone, direction, length in anchors:
            tube([origin, origin + direction * length], [.035, .002],
                 [bone, bone], 'spine', 9)
        SPINES = len(anchors) + 4
    else:
        # Crown horns and open mandibular hooks make the skull insectile.
        for sign in (-1, 1):
            tube([head + SIDE * sign * .055 + UP * .07,
                  head + SIDE * sign * .145 + UP * .22 - FRONT * .04],
                 [.032, .002], ['Head'] * 2, 'horn', 9)
            tube([head + SIDE * sign * .045 + FRONT * .10 - UP * .035,
                  head + SIDE * sign * .11 + FRONT * .18 - UP * .10],
                 [.025, .002], ['Head'] * 2, 'horn', 8)
        # Two small ribbed wings: recognizable silhouette, no vertical flight
        # or collision claim. Ordinary-map Airborne remains the rules surface.
        for sign in (-1, 1):
            root = upper + SIDE * sign * .08 - FRONT * .04 + UP * .04
            for spread, lift in ((.38, .30), (.48, .10), (.34, -.10)):
                tube([root, root + SIDE * sign * spread - FRONT * .10 +
                      UP * lift], [.025, .002],
                     ['Torso_Upper'] * 2, 'wing', 8)
        # One segmented, up-curved stinging tail. All tail vertices use one
        # measured pelvis bone so the private view can deform only this region
        # toward the real target at the native sting event.
        points = [pelvis + FRONT * .03 - UP * .02,
                  pelvis - FRONT * .20 - UP * .08,
                  pelvis - FRONT * .42 + UP * .02,
                  pelvis - FRONT * .58 + UP * .20,
                  pelvis - FRONT * .57 + UP * .42,
                  pelvis - FRONT * .43 + UP * .57]
        before = set(bm.verts)
        tube(points, [.07, .068, .06, .05, .035, .018],
             ['Torso_Lower'] * len(points), 'tail', 12)
        for index, point in enumerate(points[1:-1], 1):
            mass(point, (.075 - index * .007, .075 - index * .007,
                         .065 - index * .006), 'Torso_Lower', 'shell', 8, 12)
        tip = points[-1] + FRONT * .09 + UP * .12
        tube([points[-1], tip], [.045, .002],
             ['Torso_Lower'] * 2, 'stinger', 10)
        bm.verts.index_update()
        TAIL = dict(bone='Torso_Lower', origin=list(points[0]), tip=list(tip),
                    segments=6,
                    vertices=[vertex.index for vertex in bm.verts
                              if vertex not in before])


def write_albedo(path):
    size = 512
    image = np.zeros((size, size, 3), dtype=np.float64)
    palette = {
        'barbed-devil': {
            'body': (.42, .08, .045), 'crest': (.19, .04, .02),
            'beak': (.08, .045, .03), 'limbs': (.34, .055, .03),
            'membrane': (.25, .035, .025)},
        'bone-devil': {
            'body': (.45, .37, .25), 'crest': (.70, .61, .43),
            'limbs': (.55, .48, .34), 'beak': (.14, .09, .045),
            'membrane': (.32, .24, .15)}}[args.kind]
    for region in sorted(paint.shared.ATLAS):
        (y0, y1, x0, x1), u, v = paint.shared.region_canvas(size, region)
        base = np.array(palette.get(region, palette['body']))
        pixels = np.broadcast_to(base, (*u.shape, 3)).copy()
        ridges = .80 + .15 * np.maximum(0, np.sin(u * math.pi * 12)) ** 4
        mottling = .05 * np.cos(v * math.pi * 9) * np.sin(u * math.pi * 5)
        pixels *= (ridges + mottling)[..., None]
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
    visibleSpines=SPINES if args.kind == 'barbed-devil' else 0,
    segmentedStingingTail=args.kind == 'bone-devil',
    visibleWings=2 if args.kind == 'bone-devil' else 0,
    tail=TAIL, jawSeparated=False,
    triangleWinding='authored-outward-sprint35')
path.write_text(json.dumps(payload, indent=1) + '\n', encoding='utf-8',
                newline='\n')

scene = bpy.context.scene
scene.render.engine = 'CYCLES'
scene.cycles.samples = 16
scene.world = scene.world or bpy.data.worlds.new('InfernalReachPreviewWorld')
scene.world.color = (.12, .12, .12)
bpy.ops.object.camera_add(location=(3.2, 5.4, 3.0))
camera = bpy.context.object
camera.rotation_euler = (Vector((0, 0, 1.0)) - camera.location).to_track_quat('-Z', 'Y').to_euler()
camera.data.type = 'ORTHO'
camera.data.ortho_scale = 3.1 if args.kind == 'bone-devil' else 2.8
scene.camera = camera
bpy.ops.object.light_add(type='AREA', location=(2.5, 3.5, 5))
light = bpy.context.object
light.data.energy = 650
light.data.size = 4
light.rotation_euler = (-light.location).to_track_quat('-Z', 'Y').to_euler()
scene.render.resolution_x = scene.render.resolution_y = 720
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = 'PNG'
scene.render.filepath = args.preview
bpy.ops.render.render(write_still=True)
