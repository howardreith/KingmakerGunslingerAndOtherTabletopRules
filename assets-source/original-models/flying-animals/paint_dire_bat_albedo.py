#!/usr/bin/env python3
"""Deterministically paint Dire Bat fur, leather, ears, muzzle and bone.

Only the original procedural Pteranodon atlas/painter is reused. No native or
third-party texture is an input. The exported 1024px PNG is project-owned.
"""
import argparse
from pathlib import Path
import sys

import numpy as np

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "pteranodon"))
import paint_pteranodon_albedo as shared  # noqa: E402

SEED = 20260927
REGION_PALETTE = {
    "membrane": ((0.12, 0.085, 0.095), (0.31, 0.20, 0.18)),
    "body": ((0.17, 0.13, 0.13), (0.40, 0.30, 0.25)),
    "crest": ((0.27, 0.13, 0.16), (0.53, 0.30, 0.28)),
    "beak": ((0.13, 0.095, 0.10), (0.29, 0.17, 0.17)),
    "limbs": ((0.11, 0.085, 0.085), (0.28, 0.20, 0.17)),
}


def parse_args():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--out", required=True)
    parser.add_argument("--size", type=int, default=1024)
    return parser.parse_args(argv)


def paint(size):
    result = np.zeros((size, size, 3), dtype=np.float64)
    for index, name in enumerate(sorted(REGION_PALETTE)):
        rng = np.random.default_rng(SEED + index)
        (y0, y1, x0, x1), U, V = shared.region_canvas(size, name)
        height, width = U.shape
        fine = shared.fbm(rng, height, width, 9, 4, aspect=4.0)
        coarse = shared.fbm(rng, height, width, 3, 3)
        shade = np.clip(0.18 + 0.52 * fine + 0.30 * coarse, 0.0, 1.0)
        low, high = (np.asarray(colour) for colour in REGION_PALETTE[name])
        pixels = shared.mix(low, high, shade)
        if name == "membrane":
            # The long digits fan through the leather. Dark veins are most
            # visible against the wingtip and leave the rim translucent.
            rays = (0.5 + 0.5 * np.cos((V + U * 0.28) * 2.0 * np.pi * 13)) ** 18
            pixels *= (1.0 - 0.19 * rays)[..., None]
            rim = shared.smoothstep(0.88, 1.0, V)
            pixels = shared.mix(pixels, pixels * 1.21, 0.45 * rim)
        elif name == "body":
            # Belly and throat are lighter; short mottled fibres stay dark
            # enough to read under the game's party-camera lighting.
            belly = 1.0 - shared.smoothstep(0.2, 0.70, V)
            pixels = shared.mix(pixels, pixels * 1.20, 0.45 * belly)
        elif name == "crest":
            inner = np.clip((U * (1.0 - U) * 4.0) * V, 0.0, 1.0)
            pixels = shared.mix(pixels, np.array((0.55, 0.31, 0.31)),
                                0.65 * inner)
        result[y0:y1, x0:x1, :] = pixels
    return np.clip(result, 0.0, 1.0)


def main():
    args = parse_args()
    if args.size < 128 or args.size > 4096:
        raise SystemExit("Albedo size is outside the supported range")
    shared.write_png(args.out, paint(args.size))
    print("[dire-bat] wrote " + args.out)


if __name__ == "__main__":
    main()
