"""Offline anatomy/weight review of the private editable original source."""
import argparse
import math
from pathlib import Path
import sys
import bpy
from mathutils import Matrix, Quaternion, Vector

def main():
    parser=argparse.ArgumentParser(); parser.add_argument('--source',required=True); parser.add_argument('--out',required=True)
    args=parser.parse_args(sys.argv[sys.argv.index('--')+1:])
    source=Path(args.source).resolve(); out=Path(args.out).resolve()
    bpy.ops.wm.open_mainfile(filepath=str(source))
    scene=bpy.context.scene; obj=bpy.data.objects['Ankylosaurus']; arm=bpy.data.objects['AnkylosaurusEditableRig']
    for p in obj.data.polygons: p.use_smooth=True
    camera_data=bpy.data.cameras.new('ReviewCamera'); camera=bpy.data.objects.new('ReviewCamera',camera_data); scene.collection.objects.link(camera);scene.camera=camera
    camera_data.type='ORTHO'
    scene.render.resolution_x=900;scene.render.resolution_y=700;scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG';scene.render.film_transparent=False
    scene.world=bpy.data.worlds.new('OriginalReviewWorld');scene.world.color=(.13,.13,.13)
    scene.render.engine='BLENDER_EEVEE_NEXT'
    for name,loc,energy,size in [('Key',(3,6,3),900,5),('Fill',(-3,4,-3),650,4)]:
        data=bpy.data.lights.new(name,'AREA');data.energy=energy;data.shape='DISK';data.size=size
        light=bpy.data.objects.new(name,data);scene.collection.objects.link(light);light.location=loc;light.rotation_euler=(Vector((0,.6,0))-light.location).to_track_quat('-Z','Y').to_euler()
    original=obj.active_material
    clay=bpy.data.materials.new('OriginalClay');clay.diffuse_color=(.48,.49,.43,1)
    silhouette=bpy.data.materials.new('OriginalSilhouette');silhouette.diffuse_color=(.025,.025,.025,1)
    unlit=bpy.data.materials.new('OriginalUnlit');unlit.use_nodes=True;unlit.node_tree.nodes.clear()
    image=bpy.data.images.get('ankylosaurus-albedo.png')
    tex=unlit.node_tree.nodes.new('ShaderNodeTexImage');tex.image=image
    emit=unlit.node_tree.nodes.new('ShaderNodeEmission');output=unlit.node_tree.nodes.new('ShaderNodeOutputMaterial')
    unlit.node_tree.links.new(tex.outputs['Color'],emit.inputs['Color']);unlit.node_tree.links.new(emit.outputs[0],output.inputs['Surface'])
    out.mkdir(parents=True,exist_ok=True)
    shots=[('clay-quarter',(4,2.8,-4),clay,5.4,0),('silhouette-side',(5,1.5,0),silhouette,5.1,0),
           ('textured-quarter',(4,2.8,-4),original,5.4,0),('unlit-top',(0,6,-.3),unlit,5.1,0),
           ('party-camera',(4,5,4),original,8.5,0),('tail-extreme',(4,3,-4),clay,5.8,math.radians(110)),
           ('tail-contact',(4,3,4),original,5.8,math.radians(150))]
    tail=arm.pose.bones['Tail0_M']; rest=arm.data.bones['Tail0_M'].matrix_local.to_quaternion();tail.rotation_mode='QUATERNION'
    for name,position,material,scale,angle in shots:
        obj.data.materials[0]=material
        tail.rotation_quaternion=rest.inverted() @ Quaternion(Vector((0,1,0)),angle) @ rest
        camera.location=position
        back=(camera.location-Vector((0,.6,-.25))).normalized()
        # Kingmaker and this editable source use +Y up. Blender's tracking
        # helper assumes its own +Z world up, which rolled earlier previews.
        up=Vector((0,1,0)) if abs(back.y)<.98 else Vector((0,0,1))
        right=up.cross(back).normalized(); up=back.cross(right).normalized()
        camera.rotation_euler=Matrix((right,up,back)).transposed().to_euler();camera_data.ortho_scale=scale
        scene.render.filepath=str(out/(name+'.png'));bpy.ops.render.render(write_still=True)
    print('Offline original anatomy/weight images written; native contact is not qualified by these poses.')

if __name__=='__main__':main()
