#!/usr/bin/env python3
"""Deterministic original ember-scale painting; no native texture input."""
import argparse
from pathlib import Path
import sys

import numpy as np

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import paint_snake_albedo as common


def paint(size):
    image = np.zeros((size, size, 3), dtype=np.float64)
    for index, region in enumerate(sorted(common.shared.ATLAS)):
        rng = np.random.default_rng(20261007 + index)
        (y0, y1, x0, x1), u, v = common.shared.region_canvas(size, region)
        grain = common.shared.fbm(rng, *u.shape, 15, 3) - .5
        if region in ("body", "limbs"):
            base = common.shared.mix(common.colour((.27, .06, .027)),
                                     common.colour((.66, .25, .055)), v)
            stripes = np.cos(u * 40 * np.pi + np.sin(v * 7) * .6)
            pixels = common.shared.mix(base, common.colour((.09, .03, .022)),
                                       common.shared.smoothstep(.55, .82, stripes) * .78)
            belly = 1 - common.shared.smoothstep(.16, .35, v)
            pixels = common.shared.mix(pixels, common.colour((.69, .43, .15)), belly)
            row = np.floor(v * 17)
            x = np.mod(u * 110 + .5 * np.mod(row, 2), 1)
            y = np.mod(v * 17, 1)
            edge = np.minimum(np.minimum(x, 1 - x), np.minimum(y, 1 - y))
            pixels *= (.78 + .22 * common.shared.smoothstep(.02, .13, edge) + .14 * grain)[..., None]
        elif region == "membrane":
            pixels = common.shared.mix(common.colour((.28, .035, .016)),
                                       common.colour((.89, .49, .07)), v)
        elif region == "beak":
            pixels = common.shared.mix(common.colour((.10, .027, .03)),
                                       common.colour((.38, .10, .10)), v)
        else:
            pixels = common.shared.mix(common.colour((.97, .60, .09)),
                                       common.colour((.015, .01, .005)),
                                       1 - common.shared.smoothstep(.04, .10, abs(u - .5)))
        image[y0:y1, x0:x1] = np.clip(pixels, 0, 1)
    return image


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--out", required=True)
    parser.add_argument("--size", type=int, default=1024)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    if not 128 <= args.size <= 4096:
        raise SystemExit("unsupported albedo size")
    Path(args.out).parent.mkdir(parents=True, exist_ok=True)
    common.shared.write_png(args.out, paint(args.size))


if __name__ == "__main__":
    main()
