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
import continuous_body as surface

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
            mass(shoulder-UP*.035, (.118, .155, .105), arm[0], 'body')
            tube([upper+UP*.20, shoulder, elbow, palm],
                 [.125, .112, .083, .05],
                 ['Torso_Upper', *arm], 'body', 12)
        else:
            # Flared scapular/acromion and trochlear plates cover bone ends;
            # a round knob at every measured joint made the old cadaver a toy.
            mass(shoulder+UP*.010, (.085, .037, .060), arm[0], 'bone')
            mass(elbow+FRONT*.014, (.057, .030, .044), arm[1], 'bone')
            tube([upper+UP*.23, shoulder, elbow, palm],
                 [.060, .047, .033, .024],
                 ['Torso_Upper', *arm], 'bone', 9)
            for offset in (-.018,.018):
                tube([elbow+SIDE*offset,palm+SIDE*offset*.6],
                     [.020,.013],[arm[1],arm[2]],'bone',9)
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
            hip,knee,ankle,toe=leg_points
            common.sweep(bm,weights,uvs,
                [pelvis,hip,hip.lerp(knee,.42)+FRONT*.015,knee,
                 knee.lerp(ankle,.25)-FRONT*.025,knee.lerp(ankle,.67),ankle,
                 ankle.lerp(toe,.45)-FRONT*.02,toe],
                [.153,.141,.131,.082,.095,.073,.068,.065,.072],
                [.114,.120,.123,.066,.093,.067,.062,.060,.055],
                ['Torso_Lower',leg_names[0],leg_names[0],leg_names[1],
                 leg_names[1],leg_names[1],leg_names[2],leg_names[2],leg_names[3]],
                up=FRONT,forward=UP,steps=3,sides=14)
        else:
            hip,knee,ankle,toe=leg_points
            # Femur and tibia taper between broad, flatter condyles. Paired
            # lower shafts and the heel/three toes read as a skeleton's foot.
            common.sweep(bm,weights,uvs,
                [pelvis,hip,hip.lerp(knee,.22),hip.lerp(knee,.65),knee,
                 knee.lerp(ankle,.27),knee.lerp(ankle,.7),ankle,toe],
                [.095,.069,.047,.039,.058,.036,.029,.040,.047],
                [.064,.056,.035,.032,.044,.026,.021,.029,.029],
                ['Torso_Lower',leg_names[0],leg_names[0],leg_names[0],leg_names[1],
                 leg_names[1],leg_names[1],leg_names[2],leg_names[3]],
                region=ATLAS_REGION['bone'],up=FRONT,forward=UP,steps=3,sides=12)
            mass(knee+FRONT*.026,(.058,.033,.042),leg_names[1],'bone',8,12)
            tube([knee+SIDE*.023,ankle+SIDE*.022], [.018,.014],
                 [leg_names[1],leg_names[2]],'bone',9)
        foot=leg_points[-1]+FRONT*.045
        mass(leg_points[2]+FRONT*.015-UP*.01,
             (.052 if barbed else .038,.043 if barbed else .029,.068),
             leg_names[2],'body' if barbed else 'bone',8,14)
        mass(foot,(.095 if barbed else .082,.038 if barbed else .026,.105),
             leg_names[-1],'body' if barbed else 'bone',8,14)
        for digit in range(3):
            spread=(digit-1)*(.054 if barbed else .050)
            tube([foot+SIDE*spread*.55,
                  foot+SIDE*spread+FRONT*.095,
                  foot+SIDE*spread*1.08+FRONT*.145-UP*.009],
                 [.028 if barbed else .018,.023 if barbed else .014,.007],
                 [leg_names[-1]]*3,'body' if barbed else 'bone',9)
            tube([foot+SIDE*spread+FRONT*.105,
                  foot+SIDE*spread*1.09+FRONT*.177-UP*.018],
                 [.013,.0015],[leg_names[-1]]*2,'claw',7)

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
            # Swept iliac crests have an anatomical basin, rather than two
            # hip balls. Their anterior spine joins the narrowed femoral neck.
            tube([pelvis+SIDE*sign*.04+UP*.085-FRONT*.02,
                  pelvis+SIDE*sign*.115+UP*.07+FRONT*.015,
                  pelvis+SIDE*sign*.15+UP*.015+FRONT*.055,
                  pelvis+SIDE*sign*.09-UP*.04+FRONT*.045],
                 [.037,.045,.034,.025],['Torso_Lower']*4,'shell',10)
        # Crown horns and open mandibular hooks make the skull insectile.
        for sign in (-1, 1):
            tube([head + SIDE * sign * .055 + UP * .07,
                  head + SIDE * sign * .145 + UP * .22 - FRONT * .04],
                 [.032, .002], ['Head'] * 2, 'horn', 9)
            tube([head + SIDE * sign * .045 + FRONT * .10 - UP * .035,
                  head + SIDE * sign * .11 + FRONT * .18 - UP * .10],
                 [.025, .002], ['Head'] * 2, 'horn', 8)
        # One thin insectile membrane grows from each scapula. Its broad
        # continuous leaf and sparse veins replace the old shoulder spike fan.
        for sign in (-1, 1):
            root = upper + SIDE * sign * .10 - FRONT * .075 + UP * .25
            tip=root+SIDE*sign*.54-FRONT*.19+UP*.33
            grid=[]
            for column in range(11):
                t=column/10
                center=root.lerp(tip,t)-FRONT*(.04*math.sin(t*math.pi))
                width=.008+.16*math.sin(t*math.pi)**.8
                grid.append([center+Vector((0,.22,-1)).normalized()*((row/4-.5)*2*width)
                             for row in range(5)])
            sheet,origin=surface.sheet('BoneDevilWing',grid,.003)
            added=[bm.verts.new(v.co) for v in sheet.data.vertices]
            for vertex,(column,row) in zip(added,origin):
                weights[vertex]=[('Torso_Upper',1)]
                uvs[vertex]=common.shared.region_uv(ATLAS_REGION['wing'],.02+.96*column/10,.02+.96*row/4)
            for face in sheet.data.polygons:
                bm.faces.new([added[i] for i in face.vertices])
            data=sheet.data
            bpy.data.objects.remove(sheet,do_unlink=True)
            bpy.data.meshes.remove(data)
            tube([root,root.lerp(tip,.5)-FRONT*.04,tip],[.021,.012,.002],
                 ['Torso_Upper']*3,'shell',9)
            for column in (3,6,8):
                t=column/10
                center=grid[column][2]
                tube([root,center,grid[column][-1]], [.014,.010,.002],
                     ['Torso_Upper']*3,'shell',8)
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
        common.sweep(bm,weights,uvs,points,[.07,.062,.053,.042,.031,.018],
                     [.062,.053,.045,.036,.025,.014],['Torso_Lower']*len(points),
                     region=ATLAS_REGION['tail'],up=FRONT,forward=UP,steps=4,sides=14)
        for index, point in enumerate(points[1:-1], 1):
            mass(point, (.062-index*.008,.036-index*.004,.048-index*.005),
                 'Torso_Lower','shell',7,12)
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
            'membrane': (.54, .56, .49)}}[args.kind]
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
