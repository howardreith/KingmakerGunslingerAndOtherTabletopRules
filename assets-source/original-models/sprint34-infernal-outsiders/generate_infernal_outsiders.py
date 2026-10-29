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
    common.sweep(bm, weights, uvs, torso, [.17, .25 if barbazu else .205, .09, .075],
                 [.12, .15, .08, .065], list(TORSO[:-1]), up=FRONT, forward=UP)
    pelvis = at('Torso_Lower')
    mass(pelvis - UP * .06, (.19, .16, .13), 'Torso_Lower', 'beak')
    head = at('Head') + UP * .05 + FRONT * .025
    mass(head, (.11, .15, .105), 'Head')
    mass(head + FRONT * .095 - UP * .005, (.025, .05, .04), 'Head')
    mass(head + FRONT * .075 - UP * .09, (.075, .043, .062), 'Head')
    for side, sign in (('L', 1), ('R', -1)):
        upper, lower, palm = [side + '_' + name for name in ARMS[:3]]
        shoulder = at(upper)
        mass(shoulder, (.10, .12, .105), upper)
        tube([at('Torso_Upper'), shoulder, at(lower), at(palm)],
             [.10, .09, .06, .042], ['Torso_Upper', upper, lower, palm])
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
        tube([pelvis, *(at(n) for n in leg)], [.13, .12, .09, .055, .035],
             ['Torso_Lower', *leg])
        mass(at(leg[-1]) + FRONT * .045, (.075, .043, .13), leg[-1], 'body')
        mass(head + SIDE * sign * .046 + FRONT * .103 + UP * .035,
             (.025, .009, .012), 'Head', 'crest')
        # Pointed ears and unmistakable broad beard belong only to Barbazu.
        if barbazu:
            tube([head + SIDE*sign*.09, head + SIDE*sign*.20 + UP*.09],
                 [.045, .002], ['Head']*2, 'body', 10)
            tube([head + SIDE*sign*.075 + UP*.10,
                  head + SIDE*sign*.13 + UP*.22 - FRONT*.03,
                  head + SIDE*sign*.10 + UP*.31 + FRONT*.04],
                 [.045, .027, .002], ['Head']*3, 'limbs', 12)
    if barbazu:
        for strand in range(11):
            x = (strand - 5) * .017
            origin = head + SIDE*x + FRONT*.10 - UP*.085
            bend = (strand % 3 - 1)*.025
            tube([origin, origin + SIDE*bend + FRONT*.06 - UP*.16,
                  origin - SIDE*bend + FRONT*.08 - UP*(.37 + .025*(5-abs(strand-5)))],
                 [.026, .024, .003], ['Head']*3, 'membrane', 10)
    else:
        def link(center, axis_a, axis_b, bone, radius=.009):
            points = [center + axis_a * (.045*math.cos(i*math.tau/12)) +
                      axis_b * (.027*math.sin(i*math.tau/12)) for i in range(13)]
            tube(points, [radius]*13, [bone]*13, 'limbs', 6)
        # Iron loops wrap the torso and cross the shoulders. They follow the
        # same measured skin weights as the body; no rope solver is involved.
        for height, bone in ((.0,'Torso_Lower'),(.25,'Torso_Upper'),(.45,'Torso_Upper')):
            for i in range(18):
                t = i*math.tau/18
                center = pelvis + UP*height + SIDE*(.22*math.cos(t)) + FRONT*(.165*math.sin(t))
                tangent = -SIDE*math.sin(t) + FRONT*math.cos(t)
                link(center, tangent, UP if i%2 else SIDE*math.cos(t)+FRONT*math.sin(t), bone)
        for side, sign in (('L',1),('R',-1)):
            palm = side+'_Palm'
            for branch in (-1,1):
                points = []
                for i in range(18):
                    t = i/17
                    points.append(at(palm) + SIDE*sign*(.05+.95*t) +
                                  FRONT*(branch*.11 + .25*math.sin(t*4)) + UP*(.12*math.sin(t*6)-.32*t))
                old = set(bm.verts)
                for i, center in enumerate(points):
                    tangent = (points[min(i+1,17)]-points[max(0,i-1)]).normalized()
                    cross = tangent.cross(UP).normalized()
                    link(center, tangent, UP if i%2 else cross, palm, .010)
                    if i%4 == 3:
                        for direction in (-1,1):
                            tube([center,center+UP*direction*.075-SIDE*sign*.025],
                                 [.018,.002],[palm]*2,'limbs',6)
                CHAINS.append(dict(bone=palm, origin=list(points[0]), tip=list(points[-1]),
                                   vertices=[v for v in bm.verts if v not in old]))
    bm.verts.index_update()
    for chain in CHAINS:
        chain['vertices'] = [v.index for v in chain['vertices']]


def albedo(path):
    size=512; image=np.zeros((size,size,3),dtype=np.float64)
    for region in sorted(paint.shared.ATLAS):
        (y0,y1,x0,x1),u,v=paint.shared.region_canvas(size,region)
        base=np.array((.25,.34,.16) if args.kind=='bearded-devil' else (.64,.61,.59))
        if region=='limbs':base=np.array((.25,.20,.12) if args.kind=='bearded-devil' else (.16,.18,.20))
        if region=='membrane':base=np.array((.035,.045,.028))
        if region=='beak':base=np.array((.16,.12,.10))
        if region=='crest':base=np.array((.85,.33,.06) if args.kind=='bearded-devil' else (.24,.08,.07))
        pixels=np.broadcast_to(base,(*u.shape,3)).copy()
        pixels *= (.82+.12*np.sin(u*math.pi*10)*np.cos(v*math.pi*7)+.08*v)[...,None]
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
