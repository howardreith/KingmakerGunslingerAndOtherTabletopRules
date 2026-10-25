"""Original deterministic Lillend atlas; no source or native image input."""
from pathlib import Path
import sys
import argparse
import numpy as np
sys.dont_write_bytecode=True
sys.path.insert(0,str(Path(__file__).resolve().parent.parent/"sprint17-serpents"))
import paint_snake_albedo as common
size=1024
image=np.zeros((size,size,3),dtype=np.float64)
for index,region in enumerate(sorted(common.shared.ATLAS)):
    (y0,y1,x0,x1),u,v=common.shared.region_canvas(size,region)
    colour=common.colour
    mix=common.shared.mix
    if region=="body":
        pixels=mix(colour((.08,.30,.24)),colour((.25,.55,.35)),v)
        row=np.floor(v*18);x=np.mod(u*80+.5*np.mod(row,2),1);y=np.mod(v*18,1)
        edge=np.minimum(np.minimum(x,1-x),np.minimum(y,1-y))
        pixels*= (.78+.22*common.shared.smoothstep(.02,.12,edge))[...,None]
        belly=1-common.shared.smoothstep(.12,.32,v)
        pixels=mix(pixels,colour((.70,.71,.44)),belly*.75)
    elif region=="limbs": pixels=mix(colour((.64,.40,.29)),colour((.94,.76,.58)),v)
    elif region=="membrane":
        pixels=mix(colour((.53,.62,.68)),colour((.96,.93,.80)),v)
        vein=.04*np.cos(u*42*np.pi+v*4)
        pixels=np.clip(pixels+vein[...,None],0,1)
    elif region=="beak":
        pixels=mix(colour((.04,.16,.24)),colour((.10,.35,.38)),v)
        trim=(common.shared.smoothstep(.90,.97,np.cos(u*8*np.pi)))
        pixels=mix(pixels,colour((.86,.65,.24)),trim*.9)
    else: pixels=mix(colour((.07,.035,.13)),colour((.27,.12,.32)),v)
    image[y0:y1,x0:x1]=np.clip(pixels,0,1)
parser=argparse.ArgumentParser()
parser.add_argument('--output',required=True)
args=parser.parse_args(sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else None)
common.shared.write_png(args.output,image)
