"""Original sculpted Ettin head/collar; generate owned runtime mesh arrays.

Blender 4.5.10 -b --factory-startup --python-exit-code 1 -P this.py --
 --output <src/.../EttinAuthoredHeadMesh.cs> --work-dir <local-only dir>
No native geometry, pixels or animation data are inputs. The authored frame is
Head-local -Z forward/+Y up, established by the actual Hill Jaw/HeadCenterFX
scalar references and rejected native6 images. Native fit still needs review.
"""
import argparse
import base64
import hashlib
import json
import math
import binascii
from pathlib import Path
import struct
import sys
import zlib
import bpy
from mathutils import Vector
sys.dont_write_bytecode=True
HERE=Path(__file__).resolve().parent
sys.path.insert(0,str(HERE.parent/"continuous-bodies"))
import continuous_body as cb
AXES=(Vector((1,0,0)),Vector((0,1,0)),Vector((0,0,1)))


def build():
    volume=cb.Volume(.0075)
    def mass(c,r):volume.ellipsoid(Vector(c),AXES,r)
    # A short broad trapezius fan joins the native shoulder/neck region.
    # Negative Z is anatomical FRONT; the previous +Z face was backwards.
    mass((.13,-.43,.045),(.325,.215,.175))
    volume.sweep([(.18,-.43,.035),(.32,-.26,.005),(.43,-.08,-.035)],
                 [.205,.185,.14],segments=24)
    mass((.44,.135,-.085),(.202,.237,.18))
    mass((.44,-.027,-.111),(.192,.137,.147))
    mass((.44,-.105,-.145),(.15,.075,.106))
    mass((.44,.106,-.247),(.038,.089,.048))
    mass((.44,.045,-.275),(.06,.037,.042))
    for sign in (-1,1):
        mass((.44+sign*.125,.045,-.191),(.062,.057,.039))
        mass((.44+sign*.081,.207,-.199),(.101,.032,.047))
        mass((.44+sign*.202,.14,-.013),(.031,.064,.029))
    volume.sweep([(.325,-.005,-.242),(.40,.008,-.26),(.48,.003,-.26),
                  (.555,-.015,-.242)],[.015,.018,.018,.015],segments=16)
    mass((.44,-.055,-.239),(.105,.018,.029))
    skin=volume.polygonise("EttinSculptedHeadAndCollar")
    cb.finish_surface(skin,5200,smooth_iterations=5)
    # Broad facial planes, then REAL recessed orbit and mouth cavities rather
    # than spheres stuck onto a smooth mask. Every cutter is original code.
    for vertex in skin.data.vertices:
        p=vertex.co
        if p.y>.23 and p.z<-.20:p.z=max(p.z,-.255+.10*abs(p.x-.44))
        if -.13<p.y<-.06 and p.z<-.21:p.z=max(p.z,-.258+.06*abs(p.x-.44))
        if .18<p.y<.245 and p.z<-.225:
            p.y+=(min(.1,abs(p.x-.44))*.11-.009)
        if .012<p.y<.10 and abs(p.x-.44)>.06 and p.z<-.22:
            p.z=max(p.z,-.254+.23*abs(p.x-.44))
    skin.data.update()
    def carve(name,centre,radii):
        cutter=cb.blob(name,Vector(centre),AXES,radii,segments=32,rings=20)
        bpy.context.view_layer.objects.active=skin
        modifier=skin.modifiers.new(name,'BOOLEAN');modifier.operation='DIFFERENCE'
        modifier.solver='EXACT';modifier.object=cutter
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        bpy.data.objects.remove(cutter,do_unlink=True)
    for sign in (-1,1):
        carve('OriginalOrbitalRecess',(.44+sign*.085,.151,-.255),(.051,.031,.06))
    carve('OriginalGrimaceRecess',(.44,-.027,-.263),(.123,.026,.063))
    for sign in (-1,1):
        carve('OriginalMouthCorner',(.44+sign*.105,-.044,-.245),(.025,.025,.04))
    # Boolean intersections leave microscopic slivers; remesh our original
    # surface before reduction rather than admitting degenerate export faces.
    remesh=skin.modifiers.new('CleanOriginalCavities','REMESH')
    remesh.mode='VOXEL';remesh.voxel_size=.0045
    bpy.ops.object.modifier_apply(modifier=remesh.name)
    cb.finish_surface(skin,5600,smooth_iterations=1)
    parts=[(skin,cb.rigid_rows(skin,[("Head",1)]),0)]
    hairvol=cb.Volume(.0075)
    hairvol.ellipsoid(Vector((.44,.332,.023)),AXES,(.192,.087,.124))
    for i,dx in enumerate((-.16,-.11,-.055,0,.055,.11,.16)):
        hairvol.sweep([(.44+dx,.345,.035),(.44+dx,.225,.107),
                       (.44+dx*.9,.04+.023*math.sin(i*2.4),.094)],
                      [.028+.004*math.sin(i*1.7),.024,.004],segments=10)
    for sign in (-1,1):
        hairvol.sweep([(.44+sign*.185,.29,-.005),(.44+sign*.20,.17,.014),
                       (.44+sign*.181,.075,.004)], [.029,.026,.005],segments=10)
    hair=hairvol.polygonise("EttinSweptHair")
    cb.finish_surface(hair,1600,smooth_iterations=3)
    parts.append((hair,cb.rigid_rows(hair,[("Head",1)]),2))
    for sign in (-1,1):
        centre=Vector((.44+sign*.085,.15,-.223))
        socket=cb.blob("EttinEyeSocket",centre,AXES,(.037,.017,.012),segments=16,rings=10)
        parts.append((socket,cb.rigid_rows(socket,[("Head",1)]),1))
        eye=cb.blob("EttinEye",centre+Vector((0,0,-.014)),AXES,
                    (.017,.0055,.005),segments=16,rings=10)
        parts.append((eye,cb.rigid_rows(eye,[("Head",1)]),3))
        pupil=cb.blob("EttinPupil",centre+Vector((0,0,-.022)),AXES,
                      (.005,.006,.0015),segments=12,rings=8)
        parts.append((pupil,cb.rigid_rows(pupil,[("Head",1)]),1))
        nostril=cb.blob("EttinNostril",Vector((.44+sign*.025,.026,-.294)),AXES,
                       (.012,.008,.003),segments=12,rings=8)
        parts.append((nostril,cb.rigid_rows(nostril,[("Head",1)]),1))
    mouth=cb.blob('EttinRecessedMouth',Vector((.44,-.027,-.217)),AXES,
                  (.12,.023,.014),segments=28,rings=16)
    parts.append((mouth,cb.rigid_rows(mouth,[("Head",1)]),1))
    for dx in (-.077,-.029,.025,.076):
        tooth=cb.tube('EttinGrimaceTooth',[(.44+dx,-.041,-.242),
                     (.44+dx,-.022,-.245)],[.008,.006],segments=8)
        parts.append((tooth,cb.rigid_rows(tooth,[("Head",1)]),3))
    obj,_=cb.assemble("EttinAuthoredHead",parts)
    cb.require_single_body(obj)
    for polygon in obj.data.polygons:polygon.use_smooth=True
    return obj


def original_paint():
    """Four original paint regions in one owned 512-square atlas; no images in."""
    size=512;data=bytearray()
    for y in range(size):
        for x in range(size):
            slot=(x//256)+2*(y//256);u=(x%256)/255;v=(y%256)/255
            h=((x*374761393+y*668265263)^((x+11)*(y+37)*1274126177))&0xffffffff
            grain=((h^(h>>13))&255)/255-.5
            broad=math.sin(u*23+math.sin(v*17))*math.sin(v*31)*.025
            if slot==0:
                shade=1+broad+grain*.12
                face=math.exp(-((u-.5)/.15)**6)
                for line in (.81,.845,.88):
                    crease=math.exp(-((v-line-.007*math.cos((u-.5)*29))/.0035)**2)*face
                    shade-=crease*.24
                for eye_u in (.45,.55):
                    shade-=.22*math.exp(-((u-eye_u)/.035)**2-((v-.74)/.025)**2)
                shade-=face*math.exp(-((v-.59)/.062)**2)*(.15+grain*.20)
                root=.8+.2*min(1,max(0,(v-.22)/.39))
                rgb=(.465*shade*root,.405*shade*root,.315*shade*root)
            elif slot==1:
                rgb=(.045+grain*.008,.035+grain*.006,.025+grain*.004)
            elif slot==2:
                strand=.5+.5*math.sin(u*220+math.sin(v*11)*2)
                shade=.78+strand*.35+grain*.13
                rgb=(.115*shade,.105*shade,.089*shade)
            else:
                shade=.94+grain*.055
                rgb=(.60*shade,.57*shade,.47*shade)
            data.extend(max(0,min(255,round(c*255))) for c in rgb);data.append(255)
    def chunk(kind,payload):
        return struct.pack('>I',len(payload))+kind+payload+struct.pack('>I',binascii.crc32(kind+payload)&0xffffffff)
    # PNG stores top scanline first; authored atlas/UV coordinates are bottom-up.
    rows=b''.join(b'\0'+data[y*size*4:(y+1)*size*4] for y in reversed(range(size)))
    png=b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>2I5B',size,size,8,6,0,0,0))+chunk(b'IDAT',zlib.compress(rows,9))+chunk(b'IEND',b'')
    return png


def write_source(obj,path,paint):
    mesh=obj.data
    mesh.calc_loop_triangles()
    regions=mesh.attributes[cb.REGION_LAYER].data
    groups=[[] for _ in range(4)]
    for tri in mesh.loop_triangles:
        groups[regions[tri.vertices[0]].value].extend(tri.vertices)
    blob=bytearray(struct.pack("<i",len(mesh.vertices)))
    vertex_regions={v:regions[tri.vertices[0]].value for tri in mesh.loop_triangles for v in tri.vertices}
    uv=mesh.uv_layers.new(name="OriginalHeadPaint")
    for loop in mesh.loops:
        p=mesh.vertices[loop.vertex_index].co;slot=vertex_regions[loop.vertex_index]
        u=.5+math.atan2(p.x-.44,-p.z-.03)/(2*math.pi)
        v=max(.01,min(.99,(p.y+.65)/1.08))
        uv.data[loop.index].uv=((u+slot%2)*.5,(v+slot//2)*.5)
    for v in mesh.vertices:
        p=v.co;n=v.normal
        t=max(0,min(1,(p.y+.43)/.29))
        head=t*t*(3-2*t)
        slot=vertex_regions[v.index]
        u=.5+math.atan2(p.x-.44,-p.z-.03)/(2*math.pi)
        texv=max(.01,min(.99,(p.y+.65)/1.08))
        blob+=struct.pack("<9f",*p,*n,head,(u+slot%2)*.5,(texv+slot//2)*.5)
    for group in groups:
        blob+=struct.pack("<i",len(group))+struct.pack("<%di"%len(group),*group)
    encoded=base64.b64encode(blob).decode("ascii")
    quoted=' +\n'.join('            "'+encoded[i:i+120]+'"' for i in range(0,len(encoded),120))+';'
    image=base64.b64encode(paint).decode('ascii')
    image_quoted=' +\n'.join('            "'+image[i:i+120]+'"' for i in range(0,len(image),120))+';'
    source='''// Generated by assets-source/original-models/sprint29-ettin/build_ettin_head.py.
// Original project geometry only. Regenerate; do not hand-edit PackedMesh.
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    internal static class EttinAuthoredHeadMesh
    {
        internal const int MaterialCount = 4;
        internal const string GeometrySha256 = "'''+hashlib.sha256(blob).hexdigest()+'''";
        internal const string PaintSha256 = "'''+hashlib.sha256(paint).hexdigest()+'''";
        private const string PackedPaint =
'''+image_quoted+'''
        private const string PackedMesh =
'''+quoted+'''

        internal static Mesh Build(Matrix4x4 authoredToRoot)
        {
            using (var reader = new BinaryReader(new MemoryStream(
                Convert.FromBase64String(PackedMesh))))
            {
                int count = reader.ReadInt32();
                var vertices = new Vector3[count];
                var normals = new Vector3[count];
                var weights = new BoneWeight[count];
                var uv = new Vector2[count];
                Matrix4x4 normalMatrix = authoredToRoot.inverse.transpose;
                for (int i = 0; i < count; i++)
                {
                    vertices[i] = authoredToRoot.MultiplyPoint3x4(new Vector3(
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
                    normals[i] = normalMatrix.MultiplyVector(new Vector3(
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())).normalized;
                    float skull = reader.ReadSingle();
                    uv[i] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    weights[i] = new BoneWeight { boneIndex0 = 0, weight0 = 1f-skull,
                        boneIndex1 = 1, weight1 = skull };
                }
                var mesh = new Mesh { name = "KMG_Ettin_AuthoredHead" };
                mesh.vertices = vertices;
                mesh.normals = normals;
                mesh.boneWeights = weights;
                mesh.uv = uv;
                mesh.subMeshCount = MaterialCount;
                for (int material = 0; material < MaterialCount; material++)
                {
                    int length = reader.ReadInt32();
                    var indices = new int[length];
                    for (int i = 0; i < length; i++) indices[i] = reader.ReadInt32();
                    mesh.SetTriangles(indices, material);
                }
                if (reader.BaseStream.Position != reader.BaseStream.Length)
                    throw new InvalidDataException("Ettin authored geometry has trailing bytes.");
                mesh.RecalculateBounds();
                return mesh;
            }
        }

        internal static Texture2D BuildAlbedo()
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            try
            {
                texture.name = EttinSecondHeadVisual.ObjectPrefix + "_OriginalAlbedo";
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = FilterMode.Bilinear;
                Type imageConversion = Type.GetType(
                    "UnityEngine.ImageConversion, UnityEngine.ImageConversionModule", false);
                MethodInfo load = imageConversion == null ? null : imageConversion.GetMethod(
                    "LoadImage", BindingFlags.Public | BindingFlags.Static, null,
                    new[] { typeof(Texture2D), typeof(byte[]), typeof(bool) }, null);
                if (load == null || !(bool)load.Invoke(null, new object[] {
                    texture, Convert.FromBase64String(PackedPaint), false }))
                    throw new InvalidDataException("Original Ettin head paint could not be decoded.");
                return texture;
            }
            catch { UnityEngine.Object.DestroyImmediate(texture); throw; }
        }
    }
}
'''
    Path(path).write_text(source,encoding="utf-8",newline="\n")
    return {"vertices":len(mesh.vertices),"triangles":sum(len(g)//3 for g in groups),
            "geometrySha256":hashlib.sha256(blob).hexdigest(),
            "paintSha256":hashlib.sha256(paint).hexdigest()}


def main():
    p=argparse.ArgumentParser();p.add_argument("--output",required=True);p.add_argument("--work-dir",required=True)
    a=p.parse_args(sys.argv[sys.argv.index("--")+1:]);work=Path(a.work_dir).resolve();work.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    obj=build();paint=original_paint();(work/'ettin-original-albedo.png').write_bytes(paint)
    report=write_source(obj,Path(a.output).resolve(),paint)
    # Material colors mirror the per-instance runtime materials for preview.
    image=bpy.data.images.load(str(work/'ettin-original-albedo.png'))
    # Both readers use the same actual PNG and authored UVs; no preview tint.
    for name in ('skin','shadow','hair','eye'):
        m=bpy.data.materials.new(name);m.use_nodes=True
        bsdf=m.node_tree.nodes.get('Principled BSDF');bsdf.inputs['Roughness'].default_value=.9
        tex=m.node_tree.nodes.new('ShaderNodeTexImage');tex.image=image
        m.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color'])
        obj.data.materials.append(m)
    regions=obj.data.attributes[cb.REGION_LAYER].data
    for poly in obj.data.polygons:poly.material_index=regions[poly.vertices[0]].value
    # Explicit authoring controls only. They are not represented as native
    # bind measurements and never ship. Preview poses exercise the same
    # smooth collar/skull weight field encoded in the generated C# mesh.
    rig={"bones":[{"name":"Neck","parent":"","depth":0,"head":[.13,-.43,.045]},
                  {"name":"SecondHead","parent":"Neck","depth":1,"head":[.44,-.035,-.02]}]}
    arm=cb.review_armature(rig,"EttinAuthoringControls")
    neck=obj.vertex_groups.new(name="Neck");skull=obj.vertex_groups.new(name="SecondHead")
    for v in obj.data.vertices:
        t=max(0,min(1,(v.co.y+.43)/.29));w=t*t*(3-2*t)
        if w<1:neck.add([v.index],1-w,"REPLACE")
        if w>0:skull.add([v.index],w,"REPLACE")
    mod=obj.modifiers.new("AuthoringPose","ARMATURE");mod.object=arm;obj.parent=arm
    bpy.ops.wm.save_as_mainfile(filepath=str(work/"ettin-head-source.blend"))
    (work/"ettin-head-report.json").write_text(json.dumps(report,indent=2)+"\n")
    print(json.dumps(report))

if __name__=="__main__":main()
