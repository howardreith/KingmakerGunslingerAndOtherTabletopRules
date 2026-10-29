import argparse
from pathlib import Path
import sys
import math
import bpy
from mathutils import Vector, Matrix

parser=argparse.ArgumentParser()
parser.add_argument('--blend', required=True)
parser.add_argument('--output', required=True)
parser.add_argument('--clay', action='store_true')
args=parser.parse_args(sys.argv[sys.argv.index('--')+1:])
bpy.ops.wm.open_mainfile(filepath=str(Path(args.blend).resolve()))
scene=bpy.context.scene
scene.render.engine='CYCLES'
scene.cycles.samples=48
scene.render.resolution_x=1400
scene.render.resolution_y=1100
scene.render.resolution_percentage=100
if scene.world is None: scene.world=bpy.data.worlds.new('ReviewWorld')
scene.world.color=(.12,.12,.12)
scene.view_settings.view_transform='AgX'
scene.view_settings.look='AgX - Medium High Contrast'
for obj in list(scene.objects):
    if obj.type in ('LIGHT','CAMERA'): bpy.data.objects.remove(obj,do_unlink=True)
if args.clay:
    material=bpy.data.materials.new('UntexturedReview')
    material.diffuse_color=(.45,.45,.45,1)
    for obj in scene.objects:
        if obj.type=='MESH':
            obj.data.materials.clear();obj.data.materials.append(material)
def aim(obj, target):
    forward=(Vector(target)-obj.location).normalized()
    right=forward.cross(Vector((0,1,0))).normalized()
    up=right.cross(forward).normalized()
    obj.rotation_euler=Matrix((right,up,-forward)).transposed().to_4x4().to_euler()
for name,point,power,size in [('Key',(3,5,4),800,4),('Fill',(-3,3,1),550,3),('Rim',(0,4,-4),900,3)]:
    data=bpy.data.lights.new(name,'AREA');data.energy=power;data.shape='DISK';data.size=size
    light=bpy.data.objects.new(name,data);scene.collection.objects.link(light);light.location=point;aim(light,(0,.9,-.5))
data=bpy.data.cameras.new('Review');camera=bpy.data.objects.new('Review',data);scene.collection.objects.link(camera)
scene.camera=camera;data.type='ORTHO';data.ortho_scale=3.2
for name,point in [('front',(0,1.5,6)),('side',(6,1.7,.3)),('three-quarter',(3.4,2.5,4.7)),('gameplay',(3.7,5.7,5.0))]:
    camera.location=point;aim(camera,(0,.9,-.55));scene.render.filepath=str(Path(args.output).resolve())+'-'+name+'.png'
    bpy.ops.render.render(write_still=True)
