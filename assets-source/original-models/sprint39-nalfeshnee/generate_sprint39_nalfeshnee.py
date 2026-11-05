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

SIDE, UP, FRONT = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, 1, 0))
TORSO = ('Torso_Lower', 'Torso_Upper', 'neck', 'neck1', 'Head')
ARMS = ('Arm_Upper', 'Arm_Lower', 'Palm', 'finger1', 'finger2',
        'Bfinger1', 'Bfinger2')
LEGS = ('Leg0_Upper', 'Leg0_Lower', 'Foot0', 'toe')
BONES = TORSO + tuple(side + '_' + part for side in ('L', 'R')
                      for part in ARMS + LEGS)
# The albedo atlas has five regions; map this creature's materials onto them.
ATLAS_REGION = {
    'hide': 'body', 'belly': 'limbs', 'wing': 'membrane',
    'tusk': 'beak', 'claw': 'beak', 'eye': 'crest', 'snout': 'crest'}
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

    # THE BULK. The trunk sweep is widest low and narrows hard toward the
    # shoulders, which is the inverse of every other outsider on this rig -
    # the Vrock and the Glabrezu both broaden upward into a deep chest. Here
    # the gut is the widest thing in the figure and the shoulders are
    # comparatively small, and that single proportion is the creature.
    common.sweep(bm, weights, uvs, [at(n) for n in TORSO[:-1]],
                 [.365, .310, .125, .110], [.300, .260, .110, .100],
                 list(TORSO[:-1]), up=FRONT, forward=UP)
    # The belly itself, hung forward and low so it overhangs the legs rather
    # than sitting as a barrel between them. Built as one mass that overlaps
    # the trunk sweep generously: an earlier arrangement with a separate
    # belly ball left a visible waist crease where the two surfaces met,
    # which read as a belt rather than as fat.
    mass(pelvis - UP * .030 + FRONT * .070, (.405, .345, .340),
         'Torso_Lower', 'belly')
    mass(pelvis + UP * .095 + FRONT * .060, (.390, .325, .315),
         'Torso_Lower', 'belly')
    mass(upper - UP * .035 + FRONT * .040, (.335, .265, .260),
         'Torso_Upper', 'belly')
    # Shoulders, deliberately modest against that gut.
    mass(upper + UP * .060 + FRONT * .005, (.255, .205, .190), 'Torso_Upper')

    # Thorax and trapezius bridge the low abdominal control to the actual shoulders.
    mass(upper + UP*.245 + FRONT*.025, (.290,.245,.235), 'Torso_Upper')
    mass(upper + UP*.360 - FRONT*.035, (.205,.185,.195), 'neck')

    # A short thick neck. It has to clear the shoulder mass or the head reads
    # as a growth on the chest.
    tube([neck - UP * .010, neck + FRONT * .030 + UP * .050,
          head_bone + FRONT * .040 + UP * .018],
         [.090, .082, .076], ['neck', 'neck1', 'Head'], 'hide', 12)

    # THE BOAR HEAD: a broad low skull, a blunt snout that ends in a disc
    # rather than a taper, a heavy lower jaw, and the tusks.
    head = head_bone + FRONT * .115 - UP * .080
    mass(head, (.162, .151, .146), 'Head')
    tube([head + FRONT * .068 - UP * .014,
          head + FRONT * .142 - UP * .030],
         [.105, .085], ['Head'] * 2, 'hide', 12)
    # The snout disc. A boar's muzzle is cut off flat; a taper would read
    # canine and this creature already shares a rig with one.
    mass(head + FRONT * .176 - UP * .036, (.083, .039, .066), 'Head',
         'snout', 8, 14)
    # The heavy lower jaw, set back and below.
    tube([head + FRONT * .062 - UP * .066,
          head + FRONT * .150 - UP * .072],
         [.084, .063], ['Head'] * 2, 'hide', 10)
    tusks = 0
    ears = 0
    for sign in (-1, 1):
        # THE TUSKS. The long pair comes up out of the LOWER jaw and curves
        # past the snout line, which is the half a viewer actually reads at
        # distance; a shorter upper pair sits behind it.
        tube([head + SIDE * sign * .076 + FRONT * .142 - UP * .070,
              head + SIDE * sign * .092 + FRONT * .215 - UP * .012,
              head + SIDE * sign * .096 + FRONT * .228 + UP * .065],
             [.026, .019, .003], ['Head'] * 3, 'tusk', 8)
        tube([head + SIDE * sign * .042 + FRONT * .150 - UP * .028,
              head + SIDE * sign * .048 + FRONT * .160 - UP * .072],
             [.013, .002], ['Head'] * 2, 'tusk', 7)
        tusks += 2
        # Small eyes set wide on the brow, which is what keeps a skull this
        # broad reading as a boar rather than as a dog.
        mass(head + SIDE * sign * .126 + FRONT * .095 + UP * .048,
             (.014, .012, .012), 'Head', 'eye', 7, 12)
        # Small ears swept back against the skull.
        mass(head + SIDE * sign * .144 - FRONT * .030 + UP * .090,
             (.033, .055, .065), 'Head', 'hide', 7, 12)
        ears += 1

    # THE WINGS, small and tucked. Project geometry weighted to Torso_Upper:
    # the rig has no wing chain, so they move with the chest. Their span is
    # deliberately NARROWER than the belly is wide, which is the mismatch
    # that identifies the creature and the opposite of the Vrock's build.
    wing_panels = 0
    for sign in (-1, 1):
        # Rooted high on the shoulder and well behind it. The first pass
        # put the root at UP .070 / FRONT -.050 and the whole wing sank into
        # the shoulder mass - present in the mesh, invisible in a render -
        # so it is lifted and pushed back until the membrane clears the
        # torso silhouette entirely.
        root = upper + SIDE * sign * .105 + UP * .150 - FRONT * .120
        elbow = root + SIDE * sign * .165 + UP * .175 - FRONT * .095
        wrist = elbow + SIDE * sign * .135 + UP * .020 - FRONT * .060
        # The leading edge.
        tube([root, elbow, wrist], [.034, .026, .018],
             ['Torso_Upper'] * 3, 'hide', 9)
        def edge_at(u, root=root, elbow=elbow, wrist=wrist):
            """A point along the leading edge, shoulder to wrist."""
            return root.lerp(elbow, u / .60) if u <= .60 else                 elbow.lerp(wrist, (u - .60) / .40)

        # The membrane. Built from overlapping WIDE tapering tubes rather
        # than from a grid of ellipsoids: this exporter has no metaball
        # merge, so separate ellipsoids render as a string of beads - which
        # is exactly what the first pass produced and what the Sprint 37
        # Vrock docstring warns about. Tubes are continuous surfaces, and
        # neighbouring ones overlap enough that the sheet closes.
        grid = []
        for index in range(13):
            u = index / 12.0
            base = edge_at(u)
            drop = .105 + .18 * math.sin(u * math.pi * .86 + .18)
            tip = base + Vector((sign * .055 * u, .034 + .030 * u, -drop))
            grid.append([base.lerp(tip, j/3) for j in range(4)])
        sheet, _ = surface.sheet('NalfeshneeWing', grid, .004)
        new = [bm.verts.new(v.co) for v in sheet.data.vertices]
        for v in new:
            weights[v] = [('Torso_Upper', 1)]
            uvs[v] = common.shared.region_uv('membrane', .5, .5)
        for face in sheet.data.polygons:
            bm.faces.new([new[i] for i in face.vertices])
        data = sheet.data
        bpy.data.objects.remove(sheet, do_unlink=True)
        bpy.data.meshes.remove(data)
        wing_panels += 1

    arms = 0
    claws = 0
    for side in ('L', 'R'):
        sign = -1 if side == 'L' else 1
        arm = [side + '_' + name for name in ARMS[:3]]
        shoulder, elbow, palm = [at(name) for name in arm]

        # The arms, on the rig's real animated chains. Short and thick, and
        # held out from the gut because they cannot hang against it.
        tube([upper, shoulder, elbow, palm], [.154, .139, .104, .065],
             ['Torso_Upper', *arm], 'hide', 12)
        mass(shoulder, (.132, .126, .138), arm[0])
        mass(elbow, (.074, .078, .074), arm[1])
        hand = palm + FRONT * .018
        mass(hand, (.065, .076, .050), arm[2], 'belly', 9, 14)
        # The three claws ride the rig's two real digit chains plus the palm,
        # so the native grip animation still opens and closes the hand and
        # every measured bone carries geometry, which the exporter requires.
        for digit, lift in (('finger', .026), ('Bfinger', -.030)):
            first, second = side + '_' + digit + '1', side + '_' + digit + '2'
            root, tip = at(first), at(second)
            direction = (tip - root).normalized()
            tube([hand + UP * lift * .4, root + UP * lift,
                  tip + UP * lift * .7,
                  tip + direction * .062 + UP * lift * .4],
                 [.026, .022, .014, .003],
                 [arm[2], first, second, second], 'claw', 8)
            claws += 1
        # A third claw on the palm itself, so the printed three-clawed hand
        # reads even though the rig offers only two digit chains.
        tube([hand + FRONT * .016,
              hand + FRONT * .070 - UP * .030],
             [.013, .002], [arm[2]] * 2, 'claw', 6)
        claws += 1
        arms += 1

        # Short thick legs, mostly swallowed by the gut. Only the shin and
        # foot clear the mass, which is what makes the creature read as
        # squatting under its own weight rather than standing on it.
        leg = [side + '_' + name for name in LEGS]
        points = [at(name) for name in leg]
        tube([pelvis, points[0], points[1]], [.168, .158, .104],
             ['Torso_Lower', leg[0], leg[1]], 'hide', 11)
        tube([points[1], points[2], points[3]], [.096, .062, .042],
             [leg[1], leg[2], leg[3]], 'hide', 10)
        foot = points[-1] + FRONT * .024
        mass(foot, (.090, .045, .120), leg[-1], 'belly', 8, 14)
        for index in range(3):
            spread = (index - 1) * .042
            tube([foot + SIDE * spread * .4,
                  foot + SIDE * spread + FRONT * .078 - UP * .010],
                 [.016, .003], [leg[-1]] * 2, 'claw', 7)

    consolidate_skin(bm, weights, uvs, common.shared.ATLAS,
                     voxel=.009, triangles=9500)

    DETAIL_COUNTS.update(wings=wing_panels, arms=arms, handClaws=claws,
                         tusks=tusks, ears=ears)


def write_albedo(path):
    size = 512
    image = np.zeros((size, size, 3), dtype=np.float64)
    # A dark mottled red-brown hide with a paler warm belly and a darker
    # wing membrane, matched to the roster icon rather than to a neighbour.
    # Nothing repeats the Vrock's grey-green plumage, the Glabrezu's near
    # black chitin, the Babau's slime green-black or the Hezrou's olive.
    palette = {
        'body': (.47, .285, .210), 'crest': (.235, .125, .095),
        'beak': (.80, .73, .56), 'limbs': (.47, .285, .210),
        'membrane': (.29, .14, .12)}
    for region in sorted(paint.shared.ATLAS):
        (y0, y1, x0, x1), u, v = paint.shared.region_canvas(size, region)
        base = np.array(palette.get(region, palette['body']))
        pixels = np.broadcast_to(base, (*u.shape, 3)).copy()
        # Broad soft blotching with a fine break-up, which is what a heavy
        # hide reads as. Deliberately lower frequency than the Glabrezu's,
        # because the surfaces here are much larger.
        pattern = .86 + .13 * np.sin(u * math.pi * 4 + 0.7) * np.sin(
            v * math.pi * 5 + 1.3)
        pattern += .04 * np.sin(u * math.pi * 17) * np.cos(v * math.pi * 21)
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
    jawSeparated=False, triangleWinding='authored-outward-sprint39')
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
