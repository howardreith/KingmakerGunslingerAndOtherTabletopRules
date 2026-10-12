"""Deterministic original earthy hide/bone atlas, no imported imagery."""
import argparse
from pathlib import Path
import struct
import zlib

# Same reviewed region table as the existing project mesh generator.
import ast
def atlas(root):
    tree=ast.parse((root.parent/'pteranodon'/'generate_pteranodon.py').read_text(encoding='utf-8-sig'))
    for node in tree.body:
        if isinstance(node,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='ATLAS' for t in node.targets):
            return ast.literal_eval(node.value)
    raise ValueError('shared atlas missing')

def paint(path,size=512):
    regions=atlas(Path(__file__).resolve().parent)
    pixels=bytearray(bytes((55,52,31,255))*(size*size))
    palette={'body':(91,99,61),'limbs':(78,85,50),'membrane':(147,142,100),'beak':(79,69,47),'crest':(28,25,16)}
    for name,(u0,v0,u1,v1) in regions.items():
        base=palette.get(name,(119,111,76))
        for y in range(int(v0*size),int(v1*size)):
            for x in range(int(u0*size),int(u1*size)):
                # Pure integer coordinate grain and shallow scale seams; no
                # global RNG, time, floating image operations or metadata.
                grain=((x*73856093 ^ y*19349663 ^ 20261012)&255)/255-.5
                seam=1 if x%19<2 or y%17<2 else 0
                light=1+.085*grain-.11*seam
                offset=((size-1-y)*size+x)*4
                pixels[offset:offset+4]=bytes(tuple(max(0,min(255,round(c*light))) for c in base)+(255,))
    Path(path).parent.mkdir(parents=True,exist_ok=True)
    def chunk(name,data):
        return struct.pack('>I',len(data))+name+data+struct.pack('>I',zlib.crc32(name+data)&0xffffffff)
    scanlines=b''.join(b'\0'+pixels[row*size*4:(row+1)*size*4] for row in range(size))
    png=b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>IIBBBBB',size,size,8,6,0,0,0))+chunk(b'IDAT',zlib.compress(scanlines,9))+chunk(b'IEND',b'')
    Path(path).write_bytes(png)

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--out',required=True);paint(p.parse_args().out)
