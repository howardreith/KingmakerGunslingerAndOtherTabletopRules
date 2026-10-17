"""Original Ankylosaurus geometry; private Hodag names/rest frame only.

Run in Blender 4.5 LTS. Native vertices, UVs, textures, bind matrices and
animation curves are never read or exported. The editable private preview
armature uses the measured frame; the installable mesh contains names/weights.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
import sys
import bpy
import bmesh
from mathutils import Vector

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "pteranodon"))
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "sprint12-quadrupeds"))
import generate_pteranodon as shared
from generate_sprint12_quadrupeds import ellipsoid, tube, tuft

DONOR = "c3524f96954a1d94f8525b86e7626633"
DRIVERS = ("LowerTorso", "Spine1_M", "UpperTorso", "Neck_M", "Head", "Jaw_M") + tuple("Tail%d_M" % i for i in range(6)) + tuple(
    side + part for side in ("Left", "Right")
    for part in ("UpperArm", "LowerArm", "Hand", "UpperLeg0", "LowerLeg0", "Foot0"))


def measured_rig(path):
    raw = Path(path).read_bytes()
    capture = json.loads(raw)
    rows = [row for row in capture["donors"] if row.get("key") == "hodag" and row.get("nativeBlueprint") == DONOR]
    if len(rows) != 1 or len(rows[0]["skinnedRenderers"]) != 1:
        raise ValueError("exact single-renderer Hodag capture required")
    skin = rows[0]["skinnedRenderers"][0]
    if skin["renderer"] != "hodag" or skin["rootBone"] != "LowerTorso" or skin["boneCount"] != 36 or skin["bindPoseCount"] != 36:
        raise ValueError("Hodag renderer/bind contract changed")
    data = skin["bones"]
    by_name = {row["name"]: row for row in data}
    if len(by_name) != 36 or not set(DRIVERS).issubset(by_name):
        raise ValueError("missing or repeated reviewed driver")
    if not any(clip and clip["name"] == "Hodag_attack_tail" for manager in rows[0]["animationManagers"] for action in manager["actions"] if action for clip in (action.get("clips") or [])):
        raise ValueError("native tail action metadata missing")
    def depth(name):
        parent = by_name[name].get("parent")
        return depth(parent) + 1 if parent in by_name else 0
    bones = []
    for row in data:
        head = Vector(row["bindPosition"])
        children = [Vector(value["bindPosition"]) for value in data if value.get("parent") == row["name"]]
        tail = sum(children, Vector()) / len(children) if children else head + Vector((0, .04, 0))
        if (tail-head).length < .001:
            tail = head + Vector((0, .04, 0))
        bones.append(dict(name=row["name"], parent=row.get("parent", ""), index=row["index"], depth=depth(row["name"]), head=list(head), tail=list(tail)))
    rig = dict(source="Sprint 22 private geometry-free Hodag capture", space=rows[0]["space"], renderer="hodag", rootBone="LowerTorso", bindPoseCount=36, bones=bones)
    return rig, {bone["name"]: bone for bone in bones}, hashlib.sha256(raw).hexdigest()


def original_body(bm, weights, uvs, rig):
    side, up, forward = Vector((1,0,0)), Vector((0,1,0)), Vector((0,0,1))
    at = lambda name: Vector(rig[name]["head"])
    def oval(centre, radii, bone, region="body", latitudes=7, segments=14):
        ellipsoid(bm, weights, uvs, Vector(centre), side, up, forward, radii, bone, region, latitudes, segments)
    # Broad low barrel. Cross sections are original dimensions around the
    # measured spine, with no native shape or silhouette retained.
    stations = [(-.68,.61,.13,.12,"LowerTorso"),(-.40,.64,.55,.26,"LowerTorso"),
                (0,.65,.74,.30,"LowerTorso"),(.36,.66,.78,.32,"Spine1_M"),
                (.76,.67,.65,.29,"UpperTorso"),(1.02,.63,.34,.21,"UpperTorso"),
                (1.20,.59,.19,.13,"Neck_M")]
    rings=[]
    for row, (z,y,width,height,bone) in enumerate(stations):
        ring=[]
        for i in range(20):
            a=2*math.pi*i/20
            v=bm.verts.new((math.cos(a)*width,y+math.sin(a)*height,z))
            weights[v]=[(bone,1.)]; uvs[v]=shared.region_uv("body",row/(len(stations)-1),i/20)
            ring.append(v)
        rings.append(ring)
    for j in range(len(rings)-1):
        for i in range(20):
            bm.faces.new((rings[j][i],rings[j][(i+1)%20],rings[j+1][(i+1)%20],rings[j+1][i]))
    bm.faces.new(tuple(reversed(rings[0]))); bm.faces.new(tuple(rings[-1]))
    # Small broad head and toothless beak; no Hodag horns, mane or fangs.
    tube(bm,weights,uvs,[at("Neck_M"),at("Head")],[.19,.16],["Neck_M","Head"],"body",14)
    oval(at("Head")+forward*.03,(.225,.16,.265),"Head")
    oval(at("Head")+forward*.245-up*.02,(.17,.095,.13),"Head","beak")
    oval(at("Jaw_M")+forward*.13-up*.085,(.16,.065,.20),"Jaw_M","beak")
    for sign in (-1,1):
        oval(at("Head")+side*(sign*.205)+up*.075+forward*.115,(.033,.029,.031),"Head","crest",5,10)
        oval(at("Head")+side*(sign*.19)-forward*.13,(.11,.08,.12),"Head","membrane",5,10)
    # Six rows of seated domed osteoderms, with low lateral spikes.
    for row,z in enumerate((-.38,-.12,.16,.44,.72,.94)):
        bone="LowerTorso" if z<.1 else "Spine1_M" if z<.6 else "UpperTorso"
        width=.64 if z<.8 else .45
        for col in (-2,-1,0,1,2):
            x=col*width*.34
            y=.67+.29*math.sqrt(max(.05,1-(x/(width+.08))**2))
            oval((x,y,z),(.135,.075,.145),bone,"membrane",5,10)
        for sign in (-1,1):
            base=Vector((sign*(width+.015),.72,z))
            tuft(bm,weights,uvs,base,base+side*(sign*.205)+up*.045-forward*.07,.09,bone)
    # Thick short quadruped legs, closed overlapping spans on exact drivers.
    for s in ("Left","Right"):
        for names in ((s+"UpperArm",s+"LowerArm",s+"Hand"),(s+"UpperLeg0",s+"LowerLeg0",s+"Foot0")):
            a,b,c=(at(name) for name in names)
            tube(bm,weights,uvs,[a,a.lerp(b,.55),b],[.15,.155,.11],[names[0]]*3,"limbs",12)
            tube(bm,weights,uvs,[b,b.lerp(c,.6),c],[.12,.105,.09],[names[1]]*3,"limbs",12)
            oval(c-up*.05+forward*.04,(.145,.075,.19),names[2],"limbs",5,12)
            for digit in (-1,0,1):
                oval(c-up*.065+forward*.17+side*(digit*.075),(.025,.03,.055),names[2],"beak",4,8)
    # Real six-bone shaft; substantial terminal two-lobed bony club, rigidly
    # owned by the terminal tail joint. No generic tail spikes.
    names=["Tail%d_M" % i for i in range(6)]
    points=[at(name) for name in names]
    tube(bm,weights,uvs,points,[.145,.125,.095,.075,.060,.048],names,"body",14)
    tip=points[-1]-forward*.035
    oval(tip,(.36,.205,.27),"Tail5_M","membrane",9,20)
    for sign in (-1,1):
        oval(tip+side*(sign*.19),( .205,.195,.24),"Tail5_M","membrane",7,14)


def main():
    parser=argparse.ArgumentParser()
    for name in ("capture","albedo","mesh-data","report","blend-out"):
        parser.add_argument("--"+name,required=True)
    args=parser.parse_args(sys.argv[sys.argv.index("--")+1:])
    raise SystemExit("the Ankylosaurus is built by build_ankylosaurus.py since "
                     "the visual refinement (VIS-028); this module keeps its "
                     "capture loader; see SOURCE.md")
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data,rig,rig_hash=measured_rig(args.capture)
    armature=shared.build_armature(rig_data)
    armature.name="AnkylosaurusEditableRig"
    mesh=bpy.data.meshes.new("AnkylosaurusOriginalMesh")
    obj=bpy.data.objects.new("Ankylosaurus",mesh); bpy.context.collection.objects.link(obj)
    bm=bmesh.new(); weights,uvs={},{}
    original_body(bm,weights,uvs,rig)
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces)); bm.to_mesh(mesh)
    indexed={v.index:entries for v,entries in weights.items()}; uv_indexed={v.index:uv for v,uv in uvs.items()}; bm.free()
    if len(indexed)!=len(mesh.vertices) or len(uv_indexed)!=len(mesh.vertices):
        raise ValueError("unweighted/unpainted original vertex")
    layer=mesh.uv_layers.new(name="OriginalAlbedo")
    for loop in mesh.loops: layer.data[loop.index].uv=uv_indexed[loop.vertex_index]
    groups={name:obj.vertex_groups.new(name=name) for name in sorted({name for row in indexed.values() for name,_ in row})}
    if not set(groups).issubset(DRIVERS): raise ValueError("unreviewed driver")
    for index,entries in indexed.items():
        for name,value in entries: groups[name].add([index],value,"REPLACE")
    modifier=obj.modifiers.new(name="NativeFramePreview",type="ARMATURE"); modifier.object=armature; obj.parent=armature
    shared.attach_preview_material(obj,mesh,args.albedo)
    report=dict(schemaVersion=1,creature="ankylosaurus",donorFamily="hodag",rigSha256=rig_hash,vertices=len(mesh.vertices),polygons=len(mesh.polygons),drivers=sorted(groups),visibleLegs=4,tailJoints=6,terminalClub=True,albedo=shared.albedo_manifest(args.albedo))
    shared.write_mesh_data(args.mesh_data,obj,mesh,indexed,uv_indexed,report,args.albedo)
    payload=json.loads(Path(args.mesh_data).read_text())
    payload.update(space="donor renderer local; +Y up, +Z forward",creature="ankylosaurus",nativeDonor=DONOR,visibleLegs=4,tailJoints=6,terminalClub=True)
    Path(args.mesh_data).write_text(json.dumps(payload,indent=1)+"\n",encoding="utf-8")
    Path(args.report).write_text(json.dumps(report,indent=2)+"\n",encoding="utf-8")
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[ankylosaurus] "+json.dumps(report,sort_keys=True))

if __name__=="__main__": main()
