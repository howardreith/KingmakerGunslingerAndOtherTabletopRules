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
sys.path.insert(0, str(args.pipeline_root / 'continuous-bodies'))
from outsider_surface import consolidate_skin

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
                 [.19 if barbed else .13, .225 if barbed else .14,
                  .14 if barbed else .075, .08 if barbed else .06],
                 [.15 if barbed else .095, .16 if barbed else .10,
                  .085 if barbed else .06, .07 if barbed else .05],
                 list(TORSO[:-1]), up=FRONT, forward=UP)
    mass(pelvis - UP * .025, (.21 if barbed else .155,
                              .16 if barbed else .10,
                              .14 if barbed else .09),
         'Torso_Lower', 'body' if barbed else 'bone')
    tube([at('neck1'),head-UP*.085],[.076 if barbed else .058,.070 if barbed else .052],
         ['neck1','Head'],'body' if barbed else 'bone',12)
    mass(head, (.13 if barbed else .098, .15 if barbed else .155,
                .105 if barbed else .08), 'Head', 'head' if barbed else 'bone')
    mass(head + FRONT * .072 - UP * .058,
         (.083 if barbed else .067, .053 if barbed else .065, .06), 'Head', 'head' if barbed else 'bone')
    for sign in (-1,1):
        # Brow and cheek planes establish a face instead of a head sphere.
        mass(head + SIDE*sign*.064 + FRONT*.062 + UP*.04,
             (.066 if barbed else .046,.025,.045),'Head','head' if barbed else 'bone')
        mass(head + SIDE*sign*.045 + FRONT*.091 + UP*.016,
             (.023,.012,.018),'Head','claw')
    mass(upper + UP*.20 + FRONT*.012,
         (.25 if barbed else .17,.21 if barbed else .18,.15 if barbed else .10),
         'Torso_Upper','body')

    # A massive, thorned silhouette for Hamatula; a narrow, jointed and
    # visibly exoskeletal silhouette for Osyluth. Both retain the donor only
    # as an animation carrier.
    for side, sign in (('L', 1), ('R', -1)):
        arm = [side + '_' + name for name in ARMS[:3]]
        shoulder, elbow, palm = [at(name) for name in arm]
        if barbed:
            mass(shoulder, (.13, .125, .12), arm[0], 'body')
            tube([upper+UP*.20, shoulder, elbow, palm],
                 [.125, .112, .083, .05],
                 ['Torso_Upper', *arm], 'body', 12)
        else:
            mass(shoulder, (.075, .08, .08), arm[0], 'shell')
            mass(elbow, (.065, .06, .065), arm[1], 'shell')
            tube([upper+UP*.23, shoulder, elbow, palm],
                 [.071, .060, .043, .031],
                 ['Torso_Upper', *arm], 'bone', 9)
        mass(palm, (.055 if barbed else .035, .075 if barbed else .05,
                    .045 if barbed else .027), arm[2], 'shell')
        for digit in ('finger', 'Bfinger'):
            first, second = side + '_' + digit + '1', side + '_' + digit + '2'
            direction = (at(second) - at(first)).normalized()
            for offset in (-.016, .016):
                shift = SIDE * offset
                tube([palm + shift, at(first) + shift, at(second) + shift],
                     [.018 if barbed else .012, .018 if barbed else .012,
                      .012 if barbed else .008],
                     [arm[2], first, second], 'body' if barbed else 'bone', 8)
                tube([at(second)+shift-direction*.012,
                      at(second)+shift+direction*(.10 if barbed else .075)-FRONT*.015],
                     [.012 if barbed else .008,.0015],[second]*2,'claw',8)

        leg_names = [side + '_' + name for name in LEGS]
        leg_points = [at(name) for name in leg_names]
        if barbed:
            tube([pelvis, *leg_points], [.145, .13, .105, .065, .038],
                 ['Torso_Lower', *leg_names], 'body', 12)
        else:
            tube([pelvis, *leg_points], [.09, .073, .047, .035, .025],
                 ['Torso_Lower', *leg_names], 'bone', 9)
            for point, bone, radius in zip(leg_points[:3], leg_names[:3],
                                           (.08, .07, .055)):
                mass(point, (radius, radius * .8, radius), bone, 'shell', 8, 12)
        mass(leg_points[-1] + FRONT * .05,
             (.08 if barbed else .055, .045 if barbed else .025,
              .15 if barbed else .11), leg_names[-1], 'claw')

    # Only the connected skin/bone scaffold is fused. Armour, individual ribs,
    # eyes and weapon-like claws remain deliberate attached solids.
    consolidate_skin(bm, weights, uvs, common.shared.ATLAS, regions=('body','limbs'), voxel=.008,
                     triangles=6800)
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
        anchors=[]
        for side,sign in (('L',1),('R',-1)):
            shoulder=at(side+'_Arm_Upper')
            anchors.extend([
                (shoulder+UP*.04-FRONT*.02,side+'_Arm_Upper',
                 (SIDE*sign*.6+UP*.8).normalized(),.28),
                (upper+UP*.30+SIDE*sign*.15-FRONT*.10,'Torso_Upper',
                 (SIDE*sign*.3-FRONT*.55+UP*.8).normalized(),.27),
                (upper+UP*.17+SIDE*sign*.16-FRONT*.12,'Torso_Upper',
                 (SIDE*sign*.5-FRONT*.6+UP*.5).normalized(),.24),
                (upper+UP*.36+SIDE*sign*.07-FRONT*.10,'Torso_Upper',
                 (-FRONT*.5+UP*.9).normalized(),.24)])
        for side, sign in (('L', 1), ('R', -1)):
            for bone_name, outward in ((side + '_Arm_Upper', SIDE * sign),
                                       (side + '_Arm_Lower', SIDE * sign),
                                       (side + '_Leg0_Upper', SIDE * sign),
                                       (side + '_Leg0_Lower', SIDE * sign)):
                anchors.append((at(bone_name)+outward*.05, bone_name,
                                (outward + UP * .35 - FRONT * .15).normalized(),
                                .19))
        # Thorn roots are embedded broad pads, with a swept tapered hook.
        # Smaller dorsal rows build a dense silhouette without hiding the face.
        for height in (.06,.17,.28,.39):
            for sign in (-1,1):
                anchors.append((pelvis + UP*height + SIDE*sign*.09 - FRONT*.14,
                    'Torso_Lower' if height<.2 else 'Torso_Upper',
                    (-FRONT*.65+SIDE*sign*.35+UP*.65).normalized(),.23))
        for origin, bone, direction, length in anchors:
            mass(origin, (.050,.043,.044), bone,'shell',7,12)
            tube([origin-direction*.012, origin + direction * length*.64,
                  origin + direction * length + UP*.018], [.033,.018,.0015],
                 [bone]*3, 'spine', 9)
        # A short barbed tail completes the outline; it is visual geometry
        # and adds no tail attack or change to the existing combat profile.
        tail_points=[pelvis-UP*.06-FRONT*.07,
                     pelvis-UP*.17-FRONT*.31,
                     pelvis-UP*.08-FRONT*.52,
                     pelvis+UP*.11-FRONT*.58]
        tube(tail_points,[.07,.057,.036,.010],['Torso_Lower']*4,'body',12)
        for point in tail_points[1:]:
            for sign in (-1,1):
                tube([point,point+SIDE*sign*.10+UP*.035], [.026,.0015],
                     ['Torso_Lower']*2,'spine',8)
        SPINES = len(anchors) + 4 + 6
    else:
        # A segmented exposed ribcage and pelvis distinguish the long insectile
        # cadaver from the Babau's lean organic torso at gameplay distance.
        for row in range(6):
            z=upper.z+.30-row*.048
            width=.18-row*.012
            for sign in (-1,1):
                tube([Vector((sign*.028,upper.y+.105,z+.018)),
                      Vector((sign*width*.76,upper.y+.13,z-.012)),
                      Vector((sign*width,upper.y+.035,z-.04)),
                      Vector((sign*width*.70,upper.y-.075,z-.025))],
                     [.019,.019,.016,.012],['Torso_Upper']*4,'shell',9)
        tube([upper+UP*.32+FRONT*.12,upper+UP*.035+FRONT*.10],
             [.027,.018],['Torso_Upper']*2,'shell',10)
        for sign in (-1,1):
            mass(pelvis+SIDE*sign*.105+UP*.01,(.075,.085,.055),
                 'Torso_Lower','shell',9,14)
        # Crown horns and open mandibular hooks make the skull insectile.
        for sign in (-1, 1):
            tube([head + SIDE * sign * .055 + UP * .07,
                  head + SIDE * sign * .145 + UP * .22 - FRONT * .04],
                 [.032, .002], ['Head'] * 2, 'horn', 9)
            tube([head + SIDE * sign * .045 + FRONT * .10 - UP * .035,
                  head + SIDE * sign * .11 + FRONT * .18 - UP * .10],
                 [.025, .002], ['Head'] * 2, 'horn', 8)
        # Thin overlapping insectile wing vanes are rooted into the scapulae;
        # their closed flattened solids replace isolated bare spokes.
        for sign in (-1, 1):
            root = upper + SIDE * sign * .10 - FRONT * .075 + UP * .25
            mass(root,(.065,.075,.045),'Torso_Upper','shell',8,12)
            for vane in range(7):
                u=vane/6
                tip=root+SIDE*sign*(.32+.17*math.sin(u*math.pi))-FRONT*(.14+.06*u)+UP*(.32-.48*u)
                before_vane=set(bm.verts)
                tube([root,root.lerp(tip,.6),tip],[.042,.045,.004],
                     ['Torso_Upper']*3,'wing',10)
                # The vein-bearing wing is a thin plate, not another row of
                # round thorns. Positive-axis flattening preserves orientation.
                for vertex in bm.verts:
                    if vertex not in before_vane:
                        vertex.co.y=root.y+(vertex.co.y-root.y)*.22
        # One segmented, up-curved stinging tail. All tail vertices use one
        # measured pelvis bone so the private view can deform only this region
        # toward the real target at the native sting event.
        points = [pelvis + FRONT * .03 - UP * .02,
                  pelvis - FRONT * .20 - UP * .08,
                  pelvis - FRONT * .42 + UP * .02,
                  pelvis - FRONT * .60 + UP * .20,
                  pelvis - FRONT * .62 + UP * .45,
                  pelvis - FRONT * .48 + UP * .64]
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
            'body': (.48, .16, .095), 'crest': (.32, .205, .125),
            'beak': (.105, .075, .047), 'limbs': (.40, .13, .075),
            'membrane': (.28, .09, .06)},
        'bone-devil': {
            'body': (.38, .34, .26), 'crest': (.76, .70, .54),
            'limbs': (.66, .61, .47), 'beak': (.13, .105, .07),
            'membrane': (.42, .39, .29)}}[args.kind]
    for region in sorted(paint.shared.ATLAS):
        (y0, y1, x0, x1), u, v = paint.shared.region_canvas(size, region)
        base = np.array(palette.get(region, palette['body']))
        pixels = np.broadcast_to(base, (*u.shape, 3)).copy()
        ridges = .88 + .08 * np.maximum(0, np.sin(u * math.pi * 12)) ** 4
        mottling = .04 * np.cos(v * math.pi * 9) * np.sin(u * math.pi * 5)
        mottling += .025*np.sin(u*math.pi*67)*np.cos(v*math.pi*43)
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
