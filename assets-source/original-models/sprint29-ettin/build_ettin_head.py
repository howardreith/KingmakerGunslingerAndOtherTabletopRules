"""Original sculpted Ettin head/collar; generate owned runtime mesh arrays.

Blender 4.5.10 -b --factory-startup --python-exit-code 1 -P this.py --
 --output <src/.../EttinAuthoredHeadMesh.cs> --work-dir <local-only dir>
No native geometry, transforms or animation data are inputs. Units follow the
existing second-head art reference. Native placement still requires review.
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
    volume=cb.Volume(.009)
    def mass(c,r):volume.ellipsoid(Vector(c),AXES,r)
    # Integrated broad collar, oblique neck column and cranium. The collar
    # overlaps the donor nape; its vertices are owned by the neck, not skull.
    # A long oblique trapezius/neck emerges from the shoulder. No separate
    # toroidal collar or projecting oval cuff. Smaller forehead/jaw/ears and
    # a shallow nose/cheek plane replace the previous balloon-like features.
    mass((.20,-.78,-.085),(.30,.19,.20))
    volume.sweep([(.23,-.79,-.08),(.36,-.53,-.065),(.49,-.28,-.035),
                  (.52,-.15,-.025)],[.215,.18,.135,.13],segments=24)
    mass((.52,.065,-.015),(.195,.245,.173))
    mass((.52,-.105,.055),(.172,.105,.135))
    mass((.52,-.145,.115),(.13,.055,.075))
    mass((.52,.045,.172),(.034,.075,.035))
    mass((.52,-.005,.19),(.052,.033,.04))
    for sign in (-1,1):
        mass((.52+sign*.12,-.035,.112),(.054,.062,.044))
        mass((.52+sign*.087,.132,.14),(.094,.031,.039))
        mass((.52+sign*.192,.045,-.015),(.028,.071,.035))
    skin=volume.polygonise("EttinSculptedHeadAndCollar")
    cb.finish_surface(skin,3200,smooth_iterations=10)
    parts=[(skin,cb.rigid_rows(skin,[("Head",1)]),0)]
    # Scalp and a few broad swept locks, rooted inside the scalp/head. These
    # are authored mesh solids, with no runtime primitive construction.
    hairvol=cb.Volume(.0075)
    # Receding horseshoe hairline and tapered swept nape locks. The forehead
    # stays exposed; the cap is no longer a smooth solid helmet silhouette.
    hairvol.ellipsoid(Vector((.52,.242,-.085)),AXES,(.184,.081,.125))
    for dx in (-.153,-.105,-.052,0,.052,.105,.153):
        hairvol.sweep([(.52+dx,.262,-.105),(.52+dx,.14,-.183),
                       (.52+dx*.86,-.06,-.158)],
                      [.031,.029,.008],segments=10)
    for sign in (-1,1):
        hairvol.sweep([(.52+sign*.173,.19,-.065),(.52+sign*.184,.05,-.08),
                       (.52+sign*.155,-.075,-.062)], [.031,.025,.006],segments=10)
    hair=hairvol.polygonise("EttinSweptHair")
    cb.finish_surface(hair,1300,smooth_iterations=4)
    parts.append((hair,cb.rigid_rows(hair,[("Head",1)]),2))
    for sign in (-1,1):
        centre=Vector((.52+sign*.08,.087,.155))
        socket=cb.blob("EttinEyeSocket",centre,AXES,(.042,.023,.015),segments=12,rings=8)
        parts.append((socket,cb.rigid_rows(socket,[("Head",1)]),1))
        eye=cb.blob("EttinEye",centre+Vector((0,0,.011)),AXES,
                    (.022,.008,.008),segments=12,rings=8)
        parts.append((eye,cb.rigid_rows(eye,[("Head",1)]),3))
        pupil=cb.blob("EttinPupil",centre+Vector((0,0,.019)),AXES,
                      (.006,.006,.0035),segments=10,rings=6)
        parts.append((pupil,cb.rigid_rows(pupil,[("Head",1)]),1))
        nostril=cb.blob("EttinNostril",Vector((.52+sign*.027,-.017,.217)),AXES,
                       (.011,.006,.004),segments=10,rings=6)
        parts.append((nostril,cb.rigid_rows(nostril,[("Head",1)]),1))
    mouth=cb.tube("EttinMouthCrease",[(.425,-.118,.163),(.48,-.105,.185),
                    (.56,-.105,.185),(.615,-.118,.163)],[.005,.006,.006,.005],segments=8)
    parts.append((mouth,cb.rigid_rows(mouth,[("Head",1)]),1))
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
            broad=math.sin(u*23+math.sin(v*17))*math.sin(v*31)*.016
            if slot==0:
                shade=1+broad+grain*.075
                face=math.exp(-((u-.5)/.15)**6)
                for line in (.82,.858,.889):
                    crease=math.exp(-((v-line-.007*math.cos((u-.5)*29))/.003)**2)*face
                    shade-=crease*.13
                # Hand-authored subtle beard/under-eye weathering, in OUR UVs.
                shade-=face*math.exp(-((v-.65)/.055)**2)*(.08+grain*.09)
                rgb=(.365*shade,.235*shade,.128*shade)
            elif slot==1:
                rgb=(.070+grain*.008,.037+grain*.006,.019+grain*.004)
            elif slot==2:
                strand=.5+.5*math.sin(u*220+math.sin(v*11)*2)
                shade=.78+strand*.35+grain*.13
                rgb=(.092*shade,.057*shade,.029*shade)
            else:
                shade=.94+grain*.055
                rgb=(.49*shade,.35*shade,.17*shade)
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
        u=.5+math.atan2(p.x-.52,p.z+.03)/(2*math.pi)
        v=max(.01,min(.99,(p.y+.95)/1.32))
        uv.data[loop.index].uv=((u+slot%2)*.5,(v+slot//2)*.5)
    for v in mesh.vertices:
        p=v.co;n=v.normal
        t=max(0,min(1,(p.y+.64)/.44))
        head=t*t*(3-2*t)
        slot=vertex_regions[v.index]
        u=.5+math.atan2(p.x-.52,p.z+.03)/(2*math.pi)
        texv=max(.01,min(.99,(p.y+.95)/1.32))
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
    rig={"bones":[{"name":"Neck","parent":"","depth":0,"head":[.20,-.78,-.085]},
                  {"name":"SecondHead","parent":"Neck","depth":1,"head":[.52,.02,0]}]}
    arm=cb.review_armature(rig,"EttinAuthoringControls")
    neck=obj.vertex_groups.new(name="Neck");skull=obj.vertex_groups.new(name="SecondHead")
    for v in obj.data.vertices:
        t=max(0,min(1,(v.co.y+.64)/.44));w=t*t*(3-2*t)
        if w<1:neck.add([v.index],1-w,"REPLACE")
        if w>0:skull.add([v.index],w,"REPLACE")
    mod=obj.modifiers.new("AuthoringPose","ARMATURE");mod.object=arm;obj.parent=arm
    bpy.ops.wm.save_as_mainfile(filepath=str(work/"ettin-head-source.blend"))
    (work/"ettin-head-report.json").write_text(json.dumps(report,indent=2)+"\n")
    print(json.dumps(report))

if __name__=="__main__":main()
