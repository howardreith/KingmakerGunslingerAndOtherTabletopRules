"""Original wind-spirit humanoid and winged Shadow Demon; reuses only the existing measured-rig exporter.

No native geometry, texture or animation is read or exported. Runtime attachment,
invisibility presentation and native motion remain separate qualification gates.
"""
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
parser.add_argument('--kind', choices=('invisible-stalker', 'shadow-demon'), required=True)
parser.add_argument('--pipeline-root', type=Path, default=Path(__file__).resolve().parents[1])
for name in ('capture', 'albedo', 'mesh-data', 'report', 'blend-out', 'preview'):
    parser.add_argument('--' + name, required=True)
args = parser.parse_args(sys.argv[sys.argv.index('--') + 1:])
for folder in ('sprint17-serpents', 'sprint24-theropods'):
    sys.path.insert(0, str(args.pipeline_root / folder))
import generate_snakes as common
import generate_theropods as donor
import paint_snake_albedo as paint

SIDE, UP, FRONT = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, 1, 0))
TORSO = ('Torso_Lower', 'Torso_Upper', 'neck', 'neck1', 'Head')
ARMS = ('Arm_Upper', 'Arm_Lower', 'Palm', 'finger1', 'finger2', 'Bfinger1', 'Bfinger2')
BONES = TORSO + tuple(side + '_' + part for side in ('L', 'R') for part in ARMS)


def body(bm, weights, uvs, rig, kind):
    if kind not in ('invisible-stalker', 'shadow-demon'):
        raise ValueError('Only the two selected Sprint33 bodies are authored here')
    demon = kind == 'shadow-demon'
    at = lambda name: common.shared.head(rig, name)
    def mass(point, radii, bone, region='body'):
        common.ellipsoid(bm, weights, uvs, point, SIDE, UP, FRONT,
                         radii, bone, region, 10, 20)
    # Narrow waist, readable human chest, a long neck and an unadorned head.
    common.sweep(bm, weights, uvs, [at(n) for n in TORSO[:-1]],
                 [.17, .25, .095, .075], [.12, .145, .08, .065],
                 list(TORSO[:-1]), up=FRONT, forward=UP)
    head = at('Head') + UP * .055 + FRONT * .02
    mass(head, (.115, .155, .105), 'Head')
    mass(head + FRONT * .095 - UP * .018, (.023, .048, .035), 'Head', 'limbs')
    for side, sign in (('L', -1), ('R', 1)):
        upper, lower, palm = [side + '_' + name for name in ARMS[:3]]
        shoulder = at(upper)
        mass(shoulder, (.105, .13, .10), upper, 'limbs')
        common.tube(bm, weights, uvs,
                    [at('Torso_Upper'), shoulder, at(lower), at(palm)],
                    [.12, .093, .066, .047],
                    ['Torso_Upper', upper, lower, palm], 'limbs', 16)
        mass(at(palm) + FRONT * .015, (.052, .072, .050), palm, 'limbs')
        for digit in ('finger', 'Bfinger'):
            first, second = side + '_' + digit + '1', side + '_' + digit + '2'
            direction = (at(second) - at(first)).normalized()
            common.tube(bm, weights, uvs,
                        [at(palm), at(first), at(second), at(second) + direction * (.14 if demon else .045)],
                        [.022, .024, .018, .004], [palm, first, second, second], 'limbs', 10)
        mass(head + SIDE * sign * .045 + FRONT * .105 + UP * .038,
             (.023, .009, .010), 'Head', 'crest')
    if demon:
        # Project-authored horns and pointed ears, anchored to the native head.
        for sign in (-1, 1):
            common.tube(bm, weights, uvs,
                        [head + SIDE * sign * .075 + UP * .10,
                         head + SIDE * sign * .14 + UP * .25 - FRONT * .04,
                         head + SIDE * sign * .13 + UP * .38 + FRONT * .035],
                        [.052, .032, .002], ['Head'] * 3, 'limbs', 12)
            common.tube(bm, weights, uvs,
                        [head + SIDE * sign * .085,
                         head + SIDE * sign * .23 + UP * .12 - FRONT * .04],
                        [.048, .002], ['Head'] * 2, 'limbs', 10)
            # Closed, thin bat-wing membrane: original polygon, not donor wings.
            origin = at('Torso_Upper') - FRONT * .16
            outline = [(.12, .12), (.48, .55), (.82, .69), (1.26, .26),
                       (.96, .06), (1.10, -.28), (.77, -.12), (.78, -.58),
                       (.47, -.31), (.34, -.72), (.15, -.18)]
            layers = []
            for depth in (-.012, .012):
                layer = []
                for x, z in outline:
                    vertex = bm.verts.new(origin + SIDE * sign * x + UP * z + FRONT * depth)
                    weights[vertex] = [('Torso_Upper', 1.0)]
                    uvs[vertex] = common.shared.region_uv('membrane', x / 1.4, (z + .8) / 1.6)
                    layer.append(vertex)
                layers.append(layer)
            caps = [bm.faces.new(layers[0]), bm.faces.new(list(reversed(layers[1])))]
            bmesh.ops.triangulate(bm, faces=caps)
            for i in range(len(outline)):
                j = (i + 1) % len(outline)
                bm.faces.new((layers[0][i], layers[1][i], layers[1][j], layers[0][j]))
            # Fingers/ribs support the recognizable scalloped silhouette.
            for x, z in (outline[2], outline[3], outline[5], outline[7], outline[9]):
                common.tube(bm, weights, uvs,
                            [origin + SIDE * sign * .13,
                             origin + SIDE * sign * .55 + UP * .40,
                             origin + SIDE * sign * x + UP * z],
                            [.035, .025, .004], ['Torso_Upper'] * 3, 'limbs', 10)

    # Three restrained trailing ribbons replace legs and the elemental vortex.
    # They are original closed geometry, carried by the native pelvis motion.
    pelvis = at('Torso_Lower')
    for index in (-1, 0, 1):
        points = []
        for step in range(9):
            t = step / 8
            points.append(pelvis + SIDE * (index * .11 + .075 * math.sin(t * 4 + index) * t)
                          + UP * (.05 - .85 * t) - FRONT * (.03 + .20 * t * t))
        common.sweep(bm, weights, uvs, points,
                     [.09 * (1 - step / 8) + .003 for step in range(9)],
                     [.065 * (1 - step / 8) + .002 for step in range(9)],
                     ['Torso_Lower'] * 9, up=FRONT, forward=UP)


def albedo(path):
    size = 512
    image = np.zeros((size, size, 3), dtype=np.float64)
    for region in sorted(paint.shared.ATLAS):
        (y0, y1, x0, x1), u, v = paint.shared.region_canvas(size, region)
        wave = .5 + .5 * np.sin(18 * np.pi * u + 4 * np.sin(v * 6))
        base = np.array((.075, .09, .14) if args.kind == 'shadow-demon' else (.40, .67, .74))
        pixels = np.broadcast_to(base, (*u.shape, 3)).copy()
        pixels *= (.78 + .20 * wave)[..., None]
        pixels += (.06 * v)[..., None]
        if region == 'crest':
            pixels[:] = (.65, .24, .30) if args.kind == 'shadow-demon' else (.16, .34, .40)
        image[y0:y1, x0:x1] = np.clip(pixels, 0, 1)
    Path(path).parent.mkdir(parents=True, exist_ok=True)
    paint.shared.write_png(path, image)


albedo(args.albedo)
rig_data, rig, rig_hash = donor.measured_rig(args.capture)
finish = common.finish_original_winding
common.finish_original_winding = lambda payload, kind: finish(payload, 'salamander')
common.write_prototype(args, rig_data, rig, rig_hash, body, BONES,
                       'lizardfolk-free-hands',
                       'donor renderer local; +X lateral, +Y forward, +Z up',
                       dict(visibleArms=2, humanoidBody=True, visibleLegs=0,
                            jawSeparated=False, runtimeQualified=False))
path = Path(args.mesh_data)
payload = json.loads(path.read_text(encoding='utf-8'))
payload.update(creature=args.kind, nativeDonor=donor.DONOR_GUID, nativePrefab=donor.PREFAB_GUID,
               bodyRenderer=donor.BODY_RENDERER, accessoryRenderer=donor.ACCESSORY_RENDERER,
               nativeRendererCount=2, visibleArms=2, humanoidBody=True,
               visibleWings=2 if args.kind == "shadow-demon" else 0,
               jawSeparated=False, triangleWinding='authored-outward-sprint33')
path.write_text(json.dumps(payload, indent=1) + '\n', encoding='utf-8', newline='\n')
scene = bpy.context.scene
scene.render.engine = 'CYCLES'
scene.cycles.samples = 16
scene.world = scene.world or bpy.data.worlds.new('StalkerPreviewWorld')
scene.world.color = (.12, .12, .12)
bpy.ops.object.camera_add(location=(3.2, 5, 3.2))
camera = bpy.context.object
camera.rotation_euler = (Vector((0, 0, 1.1)) - camera.location).to_track_quat('-Z', 'Y').to_euler()
camera.data.type = 'ORTHO'
camera.data.ortho_scale = 3.3 if args.kind == 'shadow-demon' else 2.8
scene.camera = camera
bpy.ops.object.light_add(type='AREA', location=(2.5, 3.5, 5))
light = bpy.context.object
light.data.energy = 550
light.data.size = 4
light.rotation_euler = (-light.location).to_track_quat('-Z', 'Y').to_euler()
scene.render.resolution_x = scene.render.resolution_y = 640
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = 'PNG'
scene.render.filepath = args.preview
bpy.ops.render.render(write_still=True)
