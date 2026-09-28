#!/usr/bin/env python3
"""Paint a project-owned Stirge atlas; no game or third-party pixels are used."""
import argparse
from pathlib import Path
import sys

import numpy as np

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "pteranodon"))
import paint_pteranodon_albedo as shared  # noqa: E402

SEED = 20260928


def paint(size):
    result = np.zeros((size, size, 3), dtype=np.float64)
    for index, name in enumerate(sorted(shared.ATLAS)):
        rng = np.random.default_rng(SEED + index)
        (y0, y1, x0, x1), u, v = shared.region_canvas(size, name)
        fine = shared.fbm(rng, len(u), u.shape[1], 8, 4)
        grain = 0.91 + 0.18 * fine
        if name == "body":
            # Uneven rust-red fur, with a muted ochre belly along one side.
            # Deliberately no Wasp stripes or insect cuticle pattern.
            ochre = shared.smoothstep(0.37, 0.56, v)
            pixels = shared.mix(np.array((0.30, 0.085, 0.055)),
                                np.array((0.54, 0.28, 0.105)), ochre)
            fur = (0.5 + 0.5 * np.cos(2 * np.pi * (27 * u + 2 * v))) ** 7
            pixels *= (0.88 + 0.12 * fur)[..., None]
        elif name == "membrane":
            pixels = np.empty((*u.shape, 3), dtype=np.float64)
            pixels[:] = np.array((0.42, 0.14, 0.11))
            veins = (0.5 + 0.5 * np.cos((v + 0.20 * u) * 2 * np.pi * 9)) ** 20
            pixels *= (1.0 - 0.21 * veins)[..., None]
            pixels *= (0.83 + 0.17 * u)[..., None]
        elif name == "crest":
            pixels = np.empty((*u.shape, 3), dtype=np.float64)
            pixels[:] = np.array((0.09, 0.045, 0.04))
        elif name == "beak":
            pixels = np.empty((*u.shape, 3), dtype=np.float64)
            pixels[:] = np.array((0.18, 0.12, 0.09))
        else:
            pixels = np.empty((*u.shape, 3), dtype=np.float64)
            pixels[:] = np.array((0.25, 0.13, 0.09))
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
    print("[stirge] wrote " + args.out)


if __name__ == "__main__":
    main()
