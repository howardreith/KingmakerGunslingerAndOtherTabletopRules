#!/usr/bin/env python3
"""Paint a project-owned wasp atlas; no game or third-party image is used."""
import argparse
from pathlib import Path
import sys

import numpy as np

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "pteranodon"))
import paint_pteranodon_albedo as shared  # noqa: E402

SEED = 20260927


def paint(size):
    result = np.zeros((size, size, 3), dtype=np.float64)
    for index, name in enumerate(sorted(shared.ATLAS)):
        rng = np.random.default_rng(SEED + index)
        (y0, y1, x0, x1), u, v = shared.region_canvas(size, name)
        fine = shared.fbm(rng, len(u), u.shape[1], 8, 4)
        grain = 0.91 + 0.18 * fine
        if name == "body":
            # The abdomen's black transverse bands are part of the albedo,
            # not a repeated native material or a flat yellow proxy.
            band = np.cos(2 * np.pi * (4.0 * u + 0.07 * v))
            border = shared.smoothstep(-0.15, 0.22, band)
            amber = np.array((0.87, 0.55, 0.075))
            charcoal = np.array((0.085, 0.075, 0.065))
            pixels = shared.mix(charcoal, amber, border)
            pixels *= (0.86 + 0.14 * (1.0 - v))[..., None]
        elif name == "membrane":
            pixels = np.empty((*u.shape, 3), dtype=np.float64)
            pixels[:] = np.array((0.69, 0.59, 0.36))
            veins = (0.5 + 0.5 * np.cos((v + 0.22 * u) * 2 * np.pi * 13)) ** 24
            pixels *= (1.0 - 0.26 * veins)[..., None]
            pixels *= (0.78 + 0.20 * u)[..., None]
        elif name == "crest":
            pixels = np.empty((*u.shape, 3), dtype=np.float64)
            pixels[:] = np.array((0.18, 0.12, 0.085))
        elif name == "beak":
            pixels = np.empty((*u.shape, 3), dtype=np.float64)
            pixels[:] = np.array((0.10, 0.08, 0.07))
        else:
            pixels = np.empty((*u.shape, 3), dtype=np.float64)
            pixels[:] = np.array((0.16, 0.105, 0.065))
        result[y0:y1, x0:x1] = np.clip(pixels * grain[..., None], 0, 1)
    return result


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--out", required=True)
    parser.add_argument("--size", type=int, default=1024)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:]
                             if "--" in sys.argv else [])
    if not 128 <= args.size <= 4096:
        raise SystemExit("unsupported atlas size")
    shared.write_png(args.out, paint(args.size))
    print("[giant-wasp] wrote " + args.out)


if __name__ == "__main__":
    main()
