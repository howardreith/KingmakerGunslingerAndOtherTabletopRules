"""Deterministic original Lillend palette; no reference pixels are copied."""
from pathlib import Path
import sys
import argparse
import numpy as np
sys.dont_write_bytecode = True

MODELS = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(MODELS / 'sprint17-serpents'))
import paint_snake_albedo as common

size = 1024
image = np.zeros((size, size, 3), dtype=np.float64)
colour, mix = common.colour, common.shared.mix
smooth = common.shared.smoothstep
for region in sorted(common.shared.ATLAS):
    (y0, y1, x0, x1), u, v = common.shared.region_canvas(size, region)
    if region == 'body':
        pixels = mix(colour((.018, .10, .055)), colour((.065, .24, .13)), v)
        row = np.floor(v * 16)
        x = np.mod(u * 110 + .5 * np.mod(row, 2), 1)
        y = np.mod(v * 16, 1)
        arc = y - .42*(2*x-1)**2
        edge = smooth(.015, .09, arc) * (1-smooth(.94, .99, y))
        pixels *= (.72 + .28 * edge)[..., None]
        pixels += (.026 * np.sin(x * np.pi) * np.sin(y * np.pi))[..., None]
        underside = 1 - smooth(.22, .36, v)
        scute = .94 + .06 * np.cos(u * 52 * np.pi)
        pixels = mix(pixels, colour((.76, .75, .55)) * scute[..., None], underside)
    elif region == 'limbs':
        pixels = mix(colour((.68, .43, .32)), colour((.96, .77, .61)), v)
        pixels = mix(pixels, colour((.49, .20, .17)), smooth(.66, .69, v) * (1-smooth(.75, .78, v)))
        pixels = mix(pixels, colour((.78, .79, .67)), smooth(.79, .80, v) * (1-smooth(.86, .88, v)))
        pixels = mix(pixels, colour((.10, .17, .11)), smooth(.89, .90, v))
    elif region == 'crest':
        strands = (.5 + .5 * np.sin(v * 54 * np.pi + np.sin(u * 7))) ** 2
        pixels = mix(colour((.12, .031, .016)), colour((.48, .19, .066)), .25 + .58*v + .16*strands)
    elif region == 'beak':
        pixels = mix(colour((.025, .15, .17)), colour((.07, .31, .33)), v)
        pixels = mix(pixels, colour((.85, .82, .66)), smooth(.64, .67, u))
        pixels = mix(pixels, colour((.69, .49, .17)), smooth(.88, .90, u))
        grain = .99 + .01*np.cos(v*160*np.pi)*np.cos(u*125*np.pi)
        pixels *= grain[..., None]
    else:
        pixels = mix(colour((.66, .64, .51)), colour((.96, .94, .83)), v)
        shaft = 1 - smooth(.012, .025, abs(u-.5))
        barbs = .018*np.cos((v*29-abs(u-.5)*12)*2*np.pi)
        pixels = mix(pixels, colour((.74, .69, .52)), shaft*.55)
        pixels += barbs[..., None]
    image[y0:y1, x0:x1] = np.clip(pixels, 0, 1)
parser = argparse.ArgumentParser()
parser.add_argument('--output', required=True)
args = parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else None)
common.shared.write_png(args.output, image)
