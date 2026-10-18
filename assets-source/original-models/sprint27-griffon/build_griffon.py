"""Original low-detail Griffon on the measured Kingmaker Manticore rig.

Private capture and preview .blend stay local. Only original geometry/UVs/paint,
weights and native structural bone NAMES are distributable. One bounded prototype.
"""
import argparse
import base64
import hashlib
import json
import math
from pathlib import Path
import struct
import sys
import zlib
import bpy
import bmesh
from mathutils import Vector

sys.dont_write_bytecode = True
MODELS = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(MODELS / "ungulates"))
import generate_ungulates as shapes
shared = shapes.shared
shared.ATLAS = {"body": (.02,.02,.48,.48), "limbs": (.52,.02,.98,.28),
                "crest": (.02,.52,.48,.78), "beak": (.52,.32,.98,.48),
                "membrane": (.02,.82,.88,.98), "eye": (.92,.82,.99,.98)}
DONOR = "da807351572c540478b703957d863e68"
PREFAB = "e667b3d46fad6ac44bf36cdd5ab33af8"

def chunk(kind, data):
    return struct.pack(">I",len(data))+kind+data+struct.pack(">I",zlib.crc32(kind+data)&0xffffffff)

def paint(path):
    palette = {"body":(157,106,53),"limbs":(177,133,57),"crest":(221,211,177),
               "beak":(194,147,49),"membrane":(98,65,37),"eye":(16,12,7)}
    size=512; data=bytearray()
    for y in range(size):
        data.append(0)
        for x in range(size):
            u,v=(x+.5)/size,1-(y+.5)/size
            colour=(97,66,34)
            for name,region in shared.ATLAS.items():
                if region[0]<=u<=region[2] and region[1]<=v<=region[3]:
                    colour=palette[name];break
            grain=1+.035*math.sin(x*.37+y*.19)+.02*math.sin(x*.13-y*.31)
            data.extend(max(0,min(255,round(c*grain))) for c in colour);data.append(255)
    path.write_bytes(b"\x89PNG\r\n\x1a\n"+chunk(b"IHDR",struct.pack(">IIBBBBB",size,size,8,6,0,0,0))+chunk(b"IDAT",zlib.compress(bytes(data),9))+chunk(b"IEND",b""))

def rig_from_capture(path):
    raw=Path(path).read_bytes(); doc=json.loads(raw)
    matches=[v for v in doc["donors"] if v["nativeBlueprint"]==DONOR and v["prefab"]==PREFAB]
    if len(matches)!=1 or len(matches[0]["skinnedRenderers"])!=1:
        raise ValueError("Exact single measured Manticore renderer required")
    skin=matches[0]["skinnedRenderers"][0]
    if skin["renderer"]!="manticoreMesh" or skin["boneCount"]!=57 or skin["bindPoseCount"]!=57:
        raise ValueError("Measured Manticore binding changed")
    rows={b["name"]:b for b in skin["bones"]}
    def depth(n):
        p=rows[n]["parent"]
        return depth(p)+1 if p in rows else 0
    bones=[]
    for n,b in rows.items():
        children=[Vector(v["bindPosition"]) for v in rows.values() if v["parent"]==n]
        h=Vector(b["bindPosition"])
        t=sum(children,Vector())/len(children) if children else h+Vector((0,.05,0))
        if (t-h).length<.001:t=h+Vector((0,.05,0))
        bones.append({"name":n,"parent":b["parent"],"head":list(h),"tail":list(t),"depth":depth(n)})
    return {"bones":bones},rows,hashlib.sha256(raw).hexdigest()

def make_body(bm,w,uv,rig):
    at=lambda n:Vector(rig[n]["bindPosition"])
    ell=lambda c,r,b,region="body",lat=6,seg=12:shapes.ellipsoid(bm,w,uv,c,Vector(r),b,region,lat,seg)
    tube=lambda pts,rs,bs,region="body",seg=10:shared.add_tube(bm,w,uv,pts,rs,bs,region,segments=seg)
    # Broad feline torso and rounded haunches; feathered eagle forequarters.
    ell(at("Torso_Lower"),( .48,.48,.72),"Torso_Lower")
    ell(at("Spine1_M"),(.47,.46,.62),"Spine1_M")
    ell(at("Torso_Upper")+Vector((0,.03,0)),(.48,.53,.53),"Torso_Upper","crest")
    tube([at("Torso_Upper"),at("Neck_M"),at("Head")],[.37,.27,.24],["Torso_Upper","Neck_M","Head"],"crest")
    skull=at("Head")
    ell(skull+Vector((0,.06,.11)),(.26,.28,.32),"Head","crest",8,16)
    # Hooked upper beak and separate lower beak follow the native jaw action.
    tube([skull+Vector((0,.03,.28)),skull+Vector((0,.01,.53)),skull+Vector((0,-.17,.66))],[.15,.11,.012],["Head"]*3,"beak",8)
    tube([at("Jaw_M"),skull+Vector((0,-.14,.50))],[.10,.025],["Jaw_M"]*2,"beak",8)
    for sign in (-1,1):
        ell(skull+Vector((sign*.235,.10,.20)),(.036,.046,.045),"Head","eye",4,8)
        # Three swept crest feathers provide an eagle silhouette without a mane.
        for j in range(3):
            tube([skull+Vector((sign*.13,.22,.02-j*.06)),skull+Vector((sign*.16,.24,-.20-j*.08))],[.055,.006],["Head"]*2,"crest",6)
    for side,sign in (("L",-1),("R",1)):
        arm=[side+"_Arm_Upper",side+"_Arm_Lower",side+"_Palm"]
        tube([at(n) for n in arm],[.19,.13,.10],arm,"crest")
        ell(at(arm[1]),(.13,.18,.13),arm[1],"crest")
        palm=at(arm[-1]); toe=at("frontToes_"+side)
        ell(palm,(.15,.12,.19),arm[-1],"limbs")
        for j in (-1,0,1):
            root=palm+Vector((j*.09,0,.07))
            tip=toe+Vector((j*.13,-.025,.23))
            tube([root,tip,tip+Vector((0,-.07,.06))],[.043,.027,.005],[arm[-1]]*3,"beak",6)
        leg=[side+"_Leg0_Upper",side+"_Leg0_Lower",side+"_Foot0","backAnkle_"+side,"backToes_"+side]
        ell(at(leg[0])+Vector((sign*.08,-.06,-.06)),(.24,.36,.31),leg[0])
        tube([at(n) for n in leg],[.22,.15,.10,.085,.095],leg)
        ell(at(leg[-1])+Vector((0,0,.07)),(.19,.12,.24),leg[-1])
        for j in (-1,0,1):
            ell(at(leg[-1])+Vector((j*.10,-.01,.19)),(.065,.07,.11),leg[-1],"crest",4,8)
        # Feathered wing skeleton and broad overlapping closed vanes. The rig
        # owns motion; this adds no animation callbacks or collision geometry.
        base=["wing01_"+side,"wing02_"+side,"wing03_"+side]
        tube([at(n) for n in base],[.19,.16,.12],base,"membrane")
        pivot=at(base[-1])
        tip=at("wingD3_"+side)
        for j in range(11):
            t=j/10
            root=at(base[0]).lerp(pivot,.22+.78*t)
            end=pivot.lerp(tip,t*.92)+Vector((sign*.20,-.06,-.70-.34*t))
            bone=base[0] if t<.22 else base[1] if t<.50 else "wingD2_"+side
            # Each feather is a closed ellipsoid stretched along its own span.
            delta=end-root; centre=(root+end)*.5
            indices=set(bm.verts)
            ell(centre,(.145,.045,delta.length*.56),bone,"membrane",5,10)
            rot=Vector((0,0,1)).rotation_difference(delta.normalized())
            for vert in set(bm.verts)-indices:vert.co=centre+rot@(vert.co-centre)
        for j in range(5):
            t=j/4
            p=at(base[0]).lerp(pivot,t)
            ell(p,(.19,.10,.27),base[0] if t<.4 else base[1],"body",5,10)
    tail=["Torso_Lower","Tail0_M","Tail1_M","Tail2_M","Tail3_M"]
    tube([at(n) for n in tail],[.25,.11,.075,.06,.035],tail)
    ell(at("Tail3_M"),(.13,.14,.28),"Tail3_M","membrane",6,12)

def main():
    p=argparse.ArgumentParser()
    for n in ("capture","output-dir","work-dir"):p.add_argument("--"+n,required=True)
    a=p.parse_args(sys.argv[sys.argv.index("--")+1:])
    out,work=Path(a.output_dir),Path(a.work_dir);out.mkdir(parents=True,exist_ok=True);work.mkdir(parents=True,exist_ok=True)
    rig,rows,capture_hash=rig_from_capture(a.capture)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    armature=shared.build_armature(rig)
    mesh=bpy.data.meshes.new("GriffonOriginalMesh");obj=bpy.data.objects.new("GriffonOriginal",mesh);bpy.context.collection.objects.link(obj)
    bm=bmesh.new();weights,uvs={},{}
    make_body(bm,weights,uvs,rows)
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.normal_update();bm.to_mesh(mesh)
    indexed={v.index:e for v,e in weights.items()};coords={v.index:u for v,u in uvs.items()};bm.free()
    layer=mesh.uv_layers.new(name="Albedo")
    for loop in mesh.loops:layer.data[loop.index].uv=coords[loop.vertex_index]
    albedo=out/"griffon-albedo.png";paint(albedo)
    report={"creature":"griffon","rigSha256":capture_hash,"captureSha256":capture_hash,"nativeDonor":DONOR,"nativePrefab":PREFAB,"visibleLegs":4,"featheredWings":2,"eagleHead":True,"felineHindquarters":True,"maxInfluencesPerVertex":max(map(len,indexed.values()))}
    path=out/"griffon-mesh.json"
    shared.write_mesh_data(path,obj,mesh,indexed,coords,report,albedo)
    payload=json.loads(path.read_text(encoding="utf-8"));data=bytearray(base64.b64decode(payload["data"]))
    # The legacy serializer reverses winding; these vertices already use the
    # measured renderer frame. Restore Blender's outward triangle order locally.
    start=payload["vertexCount"]*32
    for o in range(start,start+payload["triangleCount"]*12,12):
        x,y,z=struct.unpack_from("<3i",data,o);struct.pack_into("<3i",data,o,x,z,y)
    payload["data"]=base64.b64encode(data).decode("ascii")
    payload.update(report);payload.update({"bodyRenderer":"manticoreMesh","nativeRendererCount":1,"nativeBoneCount":57,"space":"donor renderer local; +X right, +Y up, +Z forward"})
    path.write_text(json.dumps(payload,indent=1)+"\n",encoding="utf-8",newline="\n")
    for n in payload["bones"]:
        g=obj.vertex_groups.new(name=n)
        for i,es in indexed.items():
            for b,value in es:
                if b==n:g.add([i],value,"REPLACE")
    mod=obj.modifiers.new("Armature","ARMATURE");mod.object=armature;obj.parent=armature
    shared.attach_preview_material(obj,mesh,str(albedo))
    bpy.ops.wm.save_as_mainfile(filepath=str(work/"griffon-original.blend"))
    report.update({"vertices":payload["vertexCount"],"triangles":payload["triangleCount"],"bones":payload["bones"],"meshSha256":hashlib.sha256(path.read_bytes()).hexdigest(),"albedoSha256":hashlib.sha256(albedo.read_bytes()).hexdigest()})
    (work/"generation-report.json").write_text(json.dumps(report,indent=2)+"\n",encoding="utf-8")
    print("[griffon-original] "+json.dumps(report))

if __name__=="__main__":main()
