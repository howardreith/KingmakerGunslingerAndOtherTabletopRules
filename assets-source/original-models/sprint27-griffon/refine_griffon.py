"""Cleanup 2 original Griffon skin and closed feather fans.

Regenerates from the retained project-authored .163 mesh; no native captures
or animation curves required. The Manticore's published rig names/weights and
the original model's wing landmarks are preserved, with smooth skin bridges.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
import sys
import bpy
import numpy as np
from mathutils import Vector
sys.dont_write_bytecode=True
HERE=Path(__file__).resolve().parent
sys.path.insert(0,str(HERE.parent/"sprint25-giant-bodies"))
from refine_giant_bodies import ProjectInput, normalize_rows, cb, AXES


def build(source):
    # Keep the hooked beak, talons, eyes and subtle swept nape feather tips.
    rigid_ids={5,6,7,11,24,25,26,53,54,55}
    replaced_ids={18,19,20,47,48,49,73,74}
    wing_ids=set(range(27,44)) | set(range(56,73))
    body_ids=[i for i in range(len(source.groups)) if i not in rigid_ids|wing_ids|replaced_ids]
    volume=cb.Volume(.018)
    for i in body_ids:
        obj,_=source.component(i)
        volume.shell(obj)
    # Scapular mass and feathered neck ruff grow into the feline barrel.
    volume.ellipsoid(Vector((0,1.54,.66)),AXES,(.43,.43,.54))
    for sign in (-1,1):
        volume.sweep([(sign*.27,1.62,.57),(sign*.40,1.85,.60),
                      (sign*.58,2.07,.73)],[.26,.22,.17],segments=20)
    tail=[(0,1.45,-.45),(0,1.31,-.95),(.17,1.22,-1.43),
          (.34,1.42,-1.91),(.29,1.83,-2.08)]
    volume.sweep(tail,[.20,.13,.10,.075,.055],segments=20)
    body=volume.polygonise("GriffonContinuousSkin")
    cb.finish_surface(body,8500,smooth_iterations=20)
    rows=source.transfer(body,body_ids+[27,56])
    tail_rows=cb.chain_rows(body,[Vector(p) for p in tail],
                           ["Torso_Lower","Tail0_M","Tail1_M","Tail2_M","Tail3_M"],blend=.3)
    for i,vertex in enumerate(body.data.vertices):
        if vertex.co.z<-.95 and vertex.co.y>1.05:rows[i]=tail_rows[i]
    parts=[(body,normalize_rows(rows),0)]
    tuft_volume=cb.Volume(.01)
    tuft_volume.sweep([(.29,1.80,-2.08),(.28,1.90,-2.11),
                       (.21,2.03,-2.12),(.12,2.08,-2.09)],
                      [.06,.12,.10,.015],segments=16)
    tuft=tuft_volume.polygonise("GriffonLionTailTuft")
    cb.finish_surface(tuft,400,smooth_iterations=8)
    parts.append((tuft,cb.rigid_rows(tuft,[("Tail3_M",1)]),4))
    for i in sorted(rigid_ids):
        obj,weights=source.component(i,"GriffonFeature")
        region=2 if i in (7,11) else 1 if i in (5,6) else 3
        parts.append((obj,weights,region))
    for side,sign in (("L",-1),("R",1)):
        for j in (-1,0,1):
            x=sign*.501+j*.095
            toe=cb.tube("GriffonRootedTalon",[(x,.22,1.04),(x,.15,1.17),
                         (x,.075,1.235)],[.048,.035,.009],segments=12)
            parts.append((toe,cb.rigid_rows(toe,[(side+"_Palm",1)]),3))
        # These are landmarks on the original authored wing surface, not a
        # replacement bind rig. The renderer retains native animation.
        a=Vector((sign*.2488,1.7006,.5502))
        wrist=Vector((sign*1.1845,2.6131,1.2340))
        tip=Vector((sign*2.6116,4.0303,-.3247))
        for j in range(4):
            p=a.lerp(wrist,(j+.5)/9)
            direction=Vector((sign*.09,.08,-.40))
            lateral=Vector((sign*.65,.75,0)).normalized()
            rows=[]
            for k in range(5):
                t=k/4;centre=p+direction*t
                width=.012+.17*math.sin(t*math.pi)**.6
                rows.append([centre+lateral*width*u for u in (-1,0,1)])
            covert,_=cb.sheet("GriffonShoulderCovert",rows,.018)
            weights=source.transfer(covert,[27,56])
            parts.append((covert,normalize_rows(weights),4))
        leading=[];trailing=[];grid=[]
        for j in range(17):
            t=j/16
            if t<.48:
                q=t/.48
                p=a.lerp(wrist,q)
                length=.50+.65*q
            else:
                q=(t-.48)/.52
                p=wrist.lerp(tip,q)
                length=1.15-.53*q
            lead=p
            trail=p+Vector((sign*.04,-.12,-length))
            leading.append(lead);trailing.append(trail)
            grid.append([lead.lerp(trail,k/5)+Vector((0,.055*math.sin(math.pi*k/5),0))
                         for k in range(6)])
        fan,origin=cb.sheet("GriffonFeatherFan_"+side,grid,.022)
        rows=source.transfer(fan,sorted(wing_ids))
        # The proximal feather web blends into the chest, while the outer
        # fan follows native wing controls. A single body owns the root.
        for index,(j,k) in enumerate(origin):
            t=j/16
            if t<.14:
                amount=(1-t/.14)*.6
                row={n:v*(1-amount) for n,v in rows[index]}
                row["Torso_Upper"]=row.get("Torso_Upper",0)+amount
                rows[index]=list(row.items())
        parts.append((fan,normalize_rows(rows),4))
        # Individually tapered vanes overlap the web and each other. Their
        # breadth is in the fan's plane; unlike the old ellipsoids, they read
        # as feathers from party/front/side views rather than long needles.
        for j in range(13):
            t=(j+.5)/13
            s=min(15,int(t*16));alpha=t*16-s
            lead=leading[s].lerp(leading[s+1],alpha)
            trail=trailing[s].lerp(trailing[s+1],alpha)
            root=lead.lerp(trail,.42)
            end=trail+Vector((sign*(.07+.12*t),-.03,-.30-.18*t))
            axis=end-root
            lateral=Vector((sign*.72,.68,0)).normalized()
            feather_grid=[]
            for k in range(7):
                q=k/6
                width=.012+.16*math.sin(math.pi*q)**.7
                centre=root+axis*q
                feather_grid.append([centre+lateral*width*u for u in (-1,-.4,.4,1)])
            feather,_=cb.sheet("GriffonPrimary_"+side+str(j),feather_grid,.012)
            weights=source.transfer(feather,sorted(wing_ids))
            parts.append((feather,normalize_rows(weights),4))
    obj,weights=cb.assemble("GriffonRefined",parts)
    cb.require_single_body(obj)
    cb.unwrap(obj)
    return obj,weights


def colour(points,normals,regions):
    noise=cb.fbm(points,11,2047,octaves=3)
    rgb=np.zeros_like(points)
    ivory=np.array((.66,.61,.48));tawny=np.array((.45,.29,.13))
    # Soft feather-to-fur transition across neck/shoulders, with the lower
    # eagle forelimbs pale and the lion haunches warm.
    front=np.clip((points[:,2]-.24)/.55,0,1)
    base=tawny[None,:]*(1-front[:,None])+ivory[None,:]*front[:,None]
    rgb=base*(.87+.20*noise[:,None])
    root=np.clip((abs(points[:,0])-.26)/.19,0,1)*np.clip((points[:,1]-1.65)/.25,0,1)
    root*=np.clip(1-abs(points[:,2]-.65)/.43,0,1)*(regions==0)
    rgb=rgb*(1-root[:,None])+np.array((.34,.26,.15))*root[:,None]
    rgb[regions==1]=np.array((.62,.43,.12))*(.94+.06*noise[regions==1,None])
    rgb[regions==2]=(.055,.036,.017)
    rgb[regions==3]=(.34,.28,.18)
    feather=regions==4
    bars=.5+.5*np.sin(points[feather,2]*17+points[feather,1]*2.8)
    rgb[feather]=np.array((.32,.23,.13))*(.76+.23*bars[:,None]+.10*noise[feather,None])
    return np.clip(rgb,0,1)


def main():
    parser=argparse.ArgumentParser()
    parser.add_argument("--output-dir",required=True)
    parser.add_argument("--work-dir",required=True)
    args=parser.parse_args(sys.argv[sys.argv.index("--")+1:])
    out,work=Path(args.output_dir).resolve(),Path(args.work_dir).resolve()
    out.mkdir(parents=True,exist_ok=True);work.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    source=ProjectInput(HERE/"cleanup2-griffon-input.json")
    obj,weights=build(source)
    albedo=out/"griffon-albedo.png"
    cb.bake_albedo(obj,1024,colour,albedo)
    cb.attach_albedo_preview(obj,albedo)
    extra={k:v for k,v in source.payload.items() if k not in {
        "data","vertexCount","triangleCount","bones","albedo","uvAtlas","winding"}}
    extra.update({"authoringRevision":"cleanup2-continuous-skin-feather-fans",
                  "maxInfluencesPerVertex":max(map(len,weights.values())),
                  "sourceGeometrySha256":hashlib.sha256(source.path.read_bytes()).hexdigest()})
    report=cb.write_mesh_data(out/"griffon-mesh.json",obj,weights,
        source.payload["rigSha256"],albedo,source.payload["space"],extra)
    bpy.ops.wm.save_as_mainfile(filepath=str(work/"griffon-source.blend"))
    (work/"griffon-report.json").write_text(json.dumps(report,indent=2)+"\n")
    print(json.dumps(report))

if __name__=="__main__":main()
