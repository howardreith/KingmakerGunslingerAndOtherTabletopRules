"""Original Barbazu and Kyton geometry; measured native rig names only in exports.
No native mesh, texture or clip is copied. Local .blend previews retain private
bind measurements and never enter the distributable package.
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
parser.add_argument('--kind', choices=('bearded-devil', 'kyton-chain-devil'), required=True)
parser.add_argument('--pipeline-root', type=Path, default=Path(__file__).resolve().parents[1])
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
sys.path.insert(0, str(args.pipeline_root / 'sprint34-infernal-outsiders'))
from chain_geometry import perpendicular_axes
sys.path.insert(0, str(args.pipeline_root / 'continuous-bodies'))
from outsider_surface import consolidate_skin

SIDE, UP, FRONT = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, 1, 0))
TORSO = ('Torso_Lower', 'Torso_Upper', 'neck', 'neck1', 'Head')
ARMS = ('Arm_Upper', 'Arm_Lower', 'Palm', 'finger1', 'finger2', 'Bfinger1', 'Bfinger2')
LEGS = ('Leg0_Upper', 'Leg0_Lower', 'Foot0', 'toe')
BONES = TORSO + tuple(side + '_' + part for side in ('L', 'R') for part in ARMS + LEGS)
CHAINS = []


def body(bm, weights, uvs, rig, kind):
    barbazu = kind == 'bearded-devil'
    at = lambda name: common.shared.head(rig, name)
    def mass(point, radii, bone, region='body'):
        common.ellipsoid(bm, weights, uvs, point, SIDE, UP, FRONT, radii, bone, region, 10, 18)
    def tube(points, radii, names, region='body', sides=12):
        common.tube(bm, weights, uvs, points, radii, names, region, sides)
    torso = [at(n) for n in TORSO[:-1]]
    common.sweep(bm, weights, uvs, torso, [.17, .205, .13, .075],
                 [.135, .145, .10, .065], list(TORSO[:-1]), up=FRONT, forward=UP)
    pelvis = at('Torso_Lower')
    upper = at('Torso_Upper')
    # Connected weight-bearing hips, obliques and pectoral planes; the
    # neck/limbs are fused below before armour, beard or chains are added.
    mass(pelvis + UP*.055, (.19, .16, .13), 'Torso_Lower')
    for sign in (-1, 1):
        mass(upper + SIDE*sign*.10 + FRONT*.035 + UP*.20,
             (.145, .145, .12), 'Torso_Upper')
        mass(pelvis + SIDE*sign*.09 + UP*.19,
             (.105,.165,.10),'Torso_Lower')
    mass(pelvis + UP*.17 + FRONT*.018, (.155,.17,.125), 'Torso_Lower')
    head = at('Head') + UP * .05 + FRONT * .025
    tube([at('neck1'),head-UP*.065],[.074,.070],['neck1','Head'],'body',12)
    mass(head, (.105, .143, .092), 'Head')
    mass(head + FRONT * .080 - UP * .005, (.027, .055, .039), 'Head')
    mass(head + FRONT * .050 - UP * .075, (.083, .056, .065), 'Head')
    for sign in (-1,1):
        mass(head + SIDE*sign*.057 + FRONT*.063 + UP*.04,
             (.063,.025,.046),'Head')
        mass(head + SIDE*sign*.066 + FRONT*.055 - UP*.030,
             (.041,.042,.045),'Head')
    for side, sign in (('L', 1), ('R', -1)):
        upper, lower, palm = [side + '_' + name for name in ARMS[:3]]
        shoulder = at(upper)
        # Deltoid drapes down the humerus; it is not a ball sitting on a rod.
        mass(shoulder-UP*.035, (.094, .135, .082), upper)
        tube([at('Torso_Upper')+UP*.20, shoulder, at(lower), at(palm)],
             [.125, .102, .067, .045], ['Torso_Upper', upper, lower, palm])
        mass(at(palm), (.05, .07, .045), palm)
        for digit in ('finger', 'Bfinger'):
            first, second = side + '_' + digit + '1', side + '_' + digit + '2'
            direction = (at(second) - at(first)).normalized()
            for offset in (-.014, .014):
                shift = SIDE * offset
                tube([at(palm)+shift, at(first)+shift, at(second)+shift,
                      at(second)+shift+direction*(.085 if barbazu else .035)],
                     [.016, .017, .012, .003], [palm, first, second, second],
                     'body' if not barbazu else 'limbs', 8)
        leg = [side + '_' + name for name in LEGS]
        hip,knee,ankle,toe = [at(n) for n in leg]
        # A quadriceps taper into the patella; the high calf then narrows
        # toward an Achilles/ankle before spreading into a planted forefoot.
        points=[pelvis,hip,hip.lerp(knee,.42)+FRONT*.018,knee,
                knee.lerp(ankle,.27)-FRONT*.025,knee.lerp(ankle,.67),ankle,
                ankle.lerp(toe,.45)-FRONT*.018,toe]
        common.sweep(bm,weights,uvs,points,
            [.145,.133,.123,.073,.087,.068,.066,.062,.070],
            [.105,.110,.114,.063,.090,.065,.058,.058,.056],
            ['Torso_Lower',leg[0],leg[0],leg[1],leg[1],leg[1],leg[2],leg[2],leg[3]],
            up=FRONT,forward=UP,steps=3,sides=14)
        mass(knee+FRONT*.025,(.071,.048,.047),leg[1])
        foot=toe+FRONT*.042
        mass(ankle+FRONT*.018-UP*.010,(.052,.046,.075),leg[2])
        mass(foot,(.093,.039,.128),leg[3])
        for digit in range(3):
            spread=(digit-1)*.055
            tube([foot+SIDE*spread*.65,
                  foot+SIDE*spread+FRONT*.085,
                  foot+SIDE*spread+FRONT*.130-UP*.008],
                 [.027,.021,.006],[leg[3]]*3,'body',9)
        mass(head + SIDE * sign * .046 + FRONT * .103 + UP * .035,
             (.019, .008, .009), 'Head', 'crest')
        # Pointed ears and unmistakable broad beard belong only to Barbazu.
        if barbazu:
            tube([head + SIDE*sign*.09, head + SIDE*sign*.20 + UP*.09],
                 [.045, .002], ['Head']*2, 'body', 10)
            tube([head + SIDE*sign*.075 + UP*.10,
                  head + SIDE*sign*.13 + UP*.22 - FRONT*.03,
                  head + SIDE*sign*.10 + UP*.31 + FRONT*.04],
                 [.045, .027, .002], ['Head']*3, 'limbs', 12)
    consolidate_skin(bm, weights, uvs, common.shared.ATLAS, regions=('body',), voxel=.009,
                     triangles=6500)
    # A shaped leather skirt covers hips and upper thighs. A torso jerkin on
    # Kyton supplies an infernal warden's clothing instead of an exposed
    # pale mannequin with an isolated groin patch and crossing straps.
    common.sweep(bm,weights,uvs,
        [pelvis-UP*.23+FRONT*.06,pelvis-UP*.10+FRONT*.06,pelvis+UP*.035+FRONT*.05],
        [.33,.32,.255],[.185,.175,.155],['Torso_Lower']*3,
        region='beak',up=FRONT,forward=UP,steps=2,sides=18)
    if not barbazu:
        common.sweep(bm,weights,uvs,
            [pelvis+UP*.02+FRONT*.07,pelvis+UP*.22+FRONT*.08,
             pelvis+UP*.42+FRONT*.08,pelvis+UP*.61+FRONT*.075],
            [.255,.235,.275,.224],[.158,.175,.190,.150],
            ['Torso_Lower','Torso_Lower','Torso_Upper','Torso_Upper'],
            region='beak',up=FRONT,forward=UP,steps=3,sides=18)
        # One asymmetric shoulder plate keeps the stance directional.
        mass(at('L_Arm_Upper')+UP*.025,(.13,.064,.115),'L_Arm_Upper','limbs')
        for side in ('L','R'):
            ankle,toe=at(side+'_Foot0'),at(side+'_toe')
            common.sweep(bm,weights,uvs,
                [ankle+UP*.012,ankle.lerp(toe,.40)-FRONT*.015,toe+FRONT*.035+UP*.008],
                [.083,.078,.100],[.073,.070,.120],
                [side+'_Foot0',side+'_Foot0',side+'_toe'],region='beak',
                up=FRONT,forward=UP,steps=3,sides=14)
    if barbazu:
        # Beard grows through the jawline, not from floating root beads.
        # Several tapered curved locks leave the mouth and native glaive clear.
        for strand in range(11):
            x = (strand - 5) * .015
            origin = head + SIDE*x + FRONT*(.09+.023*(1-abs(x)/.09)) - UP*.075
            bend = (strand % 3 - 1)*.025
            length=.21+.07*(1-abs(x)/.09)
            tube([origin,origin+SIDE*bend+FRONT*(.045+.015*(strand%2))-UP*.10,
                  origin+SIDE*bend*.5+FRONT*.08-UP*(length*.75),
                  origin-SIDE*bend+FRONT*.065-UP*length],
                 [.023,.022,.015,.002],['Head']*4,'membrane',10)
    else:
        def link(center, axis_a, axis_b, bone, radius=.009):
            axis_a,axis_b=(Vector(v) for v in perpendicular_axes(axis_a,axis_b))
            before=set(bm.verts)
            points = [center + axis_a * (.045*math.cos(i*math.tau/12)) +
                      axis_b * (.027*math.sin(i*math.tau/12)) for i in range(13)]
            tube(points, [radius]*13, [bone]*13, 'limbs', 6)
            return dict(center=list(center),basisA=list(axis_a),basisB=list(axis_b),
                        majorRadius=.045,minorRadius=.027,tubeRadius=radius,
                        vertices=[v for v in bm.verts if v not in before])
        # Fitted chains follow the actual chest and hip sections. Their root
        # manacles enclose each palm: previous branches began .12 m clear of
        # the hand and visually floated even in the unposed shipped mesh.
        for height, width, depth, bone in ((.015,.277,.193,'Torso_Lower'),
                (.24,.255,.202,'Torso_Upper'),(.50,.274,.208,'Torso_Upper')):
            for i in range(22):
                t = i*math.tau/22
                center = pelvis + UP*height + SIDE*(width*math.cos(t)) + FRONT*(.075+depth*math.sin(t))
                tangent = -SIDE*width*math.sin(t) + FRONT*depth*math.cos(t)
                link(center, tangent, UP if i%2 else SIDE*math.cos(t)+FRONT*math.sin(t), bone)
        def garment_front(x,z):
            sections=((.02,.255,.158,.07),(.22,.235,.175,.08),
                      (.42,.275,.190,.08),(.61,.224,.150,.075))
            h=z-pelvis.z
            for first,second in zip(sections,sections[1:]):
                if h<=second[0]:
                    t=max(0,min(1,(h-first[0])/(second[0]-first[0])))
                    rx=first[1]+(second[1]-first[1])*t
                    ry=first[2]+(second[2]-first[2])*t
                    center=pelvis.y+first[3]+(second[3]-first[3])*t
                    return center+ry*math.sqrt(max(.02,1-(x/rx)**2))+.009
            return pelvis.y+.15
        for sign in (1,):
            start=at('Torso_Upper')+SIDE*sign*.20+UP*.32+FRONT*.15
            end=pelvis-SIDE*sign*.10+UP*.045+FRONT*.165
            axis=(end-start).normalized()
            for i in range(15):
                t=i/14
                center=start.lerp(end,t)
                center.y=garment_front(center.x,center.z)
                link(center,axis,SIDE if i%2 else FRONT,
                     'Torso_Upper' if t<.55 else 'Torso_Lower')
        # An asymmetric short catenary crosses the lower breastplate and
        # terminates at its belt, instead of a second mirrored harness strap.
        for i in range(15):
            t=i/14
            center=pelvis+UP*(.30-.10*math.sin(t*math.pi))+SIDE*(-.19+.38*t)+FRONT*.187
            center.y=garment_front(center.x,center.z)
            direction=SIDE*.38-UP*(.10*math.pi*math.cos(t*math.pi))
            link(center,direction,FRONT if i%2 else UP,'Torso_Upper')
        for side, sign in (('L',1),('R',-1)):
            palm = side+'_Palm'
            for i in range(8):
                t = i*math.tau/8
                center = at(palm) + SIDE*(.052*math.cos(t)) + FRONT*(.047*math.sin(t))
                link(center, -SIDE*math.sin(t)+FRONT*math.cos(t), UP, palm,.011)
            for branch in (-1,1):
                points = []
                for i in range(16):
                    t = i/15
                    points.append(at(palm) + SIDE*sign*(.026+.24*t + .075*math.sin(t*math.pi)) +
                                  FRONT*(branch*(.018+.12*t) + .06*math.sin(t*math.pi)) -
                                  UP*(.70*t-.04*math.sin(t*math.pi)))
                old = set(bm.verts)
                links=[]
                for i, center in enumerate(points):
                    tangent = (points[min(i+1,15)]-points[max(0,i-1)]).normalized()
                    cross = tangent.cross(UP).normalized()
                    links.append(link(center, tangent, UP if i%2 else cross, palm, .010))
                    if i%4 == 3:
                        for direction in (-1,1):
                            tube([center,center+UP*direction*.075-SIDE*sign*.025],
                                 [.018,.002],[palm]*2,'limbs',6)
                CHAINS.append(dict(bone=palm, origin=list(points[0]), tip=list(points[-1]),
                                   vertices=[v for v in bm.verts if v not in old],links=links))
    bm.verts.index_update()
    for chain in CHAINS:
        chain['vertices'] = [v.index for v in chain['vertices']]
        for link in chain['links']:
            link['vertices']=[v.index for v in link['vertices']]


def albedo(path):
    size=512; image=np.zeros((size,size,3),dtype=np.float64)
    for region in sorted(paint.shared.ATLAS):
        (y0,y1,x0,x1),u,v=paint.shared.region_canvas(size,region)
        base=np.array((.34,.405,.21) if args.kind=='bearded-devil' else (.43,.39,.36))
        if region=='limbs':base=np.array((.30,.25,.16) if args.kind=='bearded-devil' else (.27,.285,.30))
        if region=='membrane':base=np.array((.105,.125,.075))
        if region=='beak':base=np.array((.145,.105,.085) if args.kind=='bearded-devil' else (.19,.075,.065))
        if region=='crest':base=np.array((.85,.33,.06) if args.kind=='bearded-devil' else (.24,.08,.07))
        pixels=np.broadcast_to(base,(*u.shape,3)).copy()
        pixels *= (.89+.075*np.sin(u*math.pi*6)*np.cos(v*math.pi*5)+.08*v)[...,None]
        pixels *= (1+.022*np.sin(u*math.pi*71)*np.cos(v*math.pi*53))[...,None]
        if args.kind=='kyton-chain-devil' and region=='limbs':
            pixels += (.22*np.maximum(0,np.sin(u*math.pi*6))**12)[...,None]
        image[y0:y1,x0:x1]=np.clip(pixels,0,1)
    Path(path).parent.mkdir(parents=True,exist_ok=True)
    paint.shared.write_png(path,image)

albedo(args.albedo)
rig_data,rig,rig_hash=donor.measured_rig(args.capture)
finish=common.finish_original_winding
common.finish_original_winding=lambda payload,kind:finish(payload,'salamander')
common.write_prototype(args,rig_data,rig,rig_hash,body,BONES,'lizardfolk-free-hands',
                       'donor renderer local; +X lateral, +Y forward, +Z up',
                       dict(visibleArms=2,visibleLegs=2,humanoidBody=True,jawSeparated=False,runtimeQualified=False))
path=Path(args.mesh_data); payload=json.loads(path.read_text(encoding='utf-8'))
payload.update(creature=args.kind,nativeDonor=donor.DONOR_GUID,nativePrefab=donor.PREFAB_GUID,
               bodyRenderer=donor.BODY_RENDERER,accessoryRenderer=donor.ACCESSORY_RENDERER,
               nativeRendererCount=2,visibleArms=2,visibleLegs=2,humanoidBody=True,
               visibleBeard=args.kind=='bearded-devil',visibleChains=len(CHAINS),chains=CHAINS,
               jawSeparated=False,triangleWinding='authored-outward-sprint34')
path.write_text(json.dumps(payload,indent=1)+'\n',encoding='utf-8',newline='\n')
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16
scene.world=scene.world or bpy.data.worlds.new('InfernalPreviewWorld');scene.world.color=(.14,.14,.14)
bpy.ops.object.camera_add(location=(3.2,5,3));camera=bpy.context.object
camera.rotation_euler=(Vector((0,0,1.0))-camera.location).to_track_quat('-Z','Y').to_euler()
camera.data.type='ORTHO';camera.data.ortho_scale=4.3 if CHAINS else 2.6;scene.camera=camera
bpy.ops.object.light_add(type='AREA',location=(2.5,3.5,5));light=bpy.context.object
light.data.energy=650;light.data.size=4
light.rotation_euler=(-light.location).to_track_quat('-Z','Y').to_euler()
scene.render.resolution_x=scene.render.resolution_y=720;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.render.filepath=args.preview
bpy.ops.render.render(write_still=True)
