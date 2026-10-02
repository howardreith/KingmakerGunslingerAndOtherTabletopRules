#!/usr/bin/env python3
"""Paint an original golden eagle atlas without native or third-party pixels."""
import argparse
from pathlib import Path
import sys

import numpy as np

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "pteranodon"))
import paint_pteranodon_albedo as shared  # noqa: E402

SEED = 20260927 + 9
PALETTE = {
    "membrane": ((0.13, 0.09, 0.055), (0.46, 0.30, 0.15)),
    "body": ((0.23, 0.15, 0.075), (0.56, 0.37, 0.17)),
    "crest": ((0.28, 0.20, 0.11), (0.65, 0.48, 0.25)),
    "beak": ((0.26, 0.20, 0.09), (0.72, 0.55, 0.19)),
    "limbs": ((0.21, 0.15, 0.065), (0.62, 0.47, 0.16)),
}


def paint(size):
    image = np.zeros((size, size, 3), dtype=np.float64)
    for index, name in enumerate(sorted(PALETTE)):
        rng = np.random.default_rng(SEED + index)
        (y0, y1, x0, x1), U, V = shared.region_canvas(size, name)
        height, width = U.shape
        fine = shared.fbm(rng, height, width, 8, 4, aspect=3.0)
        broad = shared.fbm(rng, height, width, 3, 3)
        low, high = (np.asarray(colour) for colour in PALETTE[name])
        shade = np.clip(0.22 + 0.50 * fine + 0.28 * broad, 0.0, 1.0)
        pixels = shared.mix(low, high, shade)
        if name == "membrane":
            # Repeated shaft and vane marks read as overlapping flight feathers.
            shaft = (0.5 + 0.5 * np.cos(V * 2.0 * np.pi * 28)) ** 16
            dark_tip = shared.smoothstep(0.73, 1.0, V)
            pixels *= (1.0 - 0.25 * shaft - 0.27 * dark_tip)[..., None]
            pixels *= (0.86 + 0.14 * np.sin(U * np.pi))[..., None]
        elif name == "body":
            # Pale nape and mantle, warm brown breast, darker back.
            nape = shared.smoothstep(0.62, 0.92, U)
            pixels = shared.mix(pixels, pixels * 1.25, 0.55 * nape)
            fleck = (0.5 + 0.5 * np.cos(U * 2.0 * np.pi * 21)) ** 14
            pixels *= (1.0 - 0.12 * fleck)[..., None]
        elif name == "beak":
            hook = shared.smoothstep(0.60, 1.0, U)
            pixels *= (1.0 - 0.35 * hook)[..., None]
        image[y0:y1, x0:x1, :] = pixels
    return np.clip(image, 0.0, 1.0)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--out", required=True)
    parser.add_argument("--size", type=int, default=1024)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:]
                             if "--" in sys.argv else [])
    if not 128 <= args.size <= 4096:
        raise SystemExit("Unsupported eagle atlas size")
    shared.write_png(args.out, paint(args.size))
    print("[eagle] wrote " + args.out)


if __name__ == "__main__":
    main()
