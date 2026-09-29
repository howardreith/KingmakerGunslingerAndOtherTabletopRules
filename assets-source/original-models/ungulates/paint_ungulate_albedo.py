#!/usr/bin/env python3
"""Paint four deterministic original ungulate skin/fur atlases."""
import argparse
from pathlib import Path
import sys

import numpy as np

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "pteranodon"))
import paint_pteranodon_albedo as shared  # noqa: E402

COATS = {
    "aurochs": ((0.17, 0.11, 0.075), (0.73, 0.66, 0.51), (0.11, 0.085, 0.07)),
    "bison": ((0.22, 0.115, 0.06), (0.64, 0.52, 0.36), (0.10, 0.07, 0.055)),
    "rhinoceros": ((0.36, 0.33, 0.28), (0.75, 0.69, 0.55), (0.25, 0.23, 0.21)),
    "woolly-rhinoceros": ((0.30, 0.16, 0.085), (0.75, 0.67, 0.48),
                          (0.17, 0.10, 0.065)),
}


def paint(kind, size):
    coat, horn, hoof = (np.array(row) for row in COATS[kind])
    result = np.zeros((size, size, 3), dtype=np.float64)
    seed = list(COATS).index(kind) * 17 + 20260929
    for index, name in enumerate(sorted(shared.ATLAS)):
        rng = np.random.default_rng(seed + index)
        (y0, y1, x0, x1), u, v = shared.region_canvas(size, name)
        fine = shared.fbm(rng, len(u), u.shape[1], 8, 5)
        grain = 0.84 + 0.30 * fine
        if name == "body":
            pixels = np.empty((*u.shape, 3), dtype=np.float64)
            pixels[:] = coat
            # The bison and woolly rhino's vertical fur break-up is painted,
            # while the bare rhino receives broad folded-skin creases.
            if kind in ("bison", "woolly-rhinoceros"):
                strands = (0.5 + 0.5 * np.cos(2 * np.pi *
                    (52 * u + 3 * fine))) ** 11
                pixels *= (0.80 + 0.30 * strands)[..., None]
            elif kind == "rhinoceros":
                folds = (0.5 + 0.5 * np.cos(2 * np.pi *
                    (4 * u + 0.20 * v))) ** 20
                pixels *= (1.0 - 0.23 * folds)[..., None]
            pixels *= (0.80 + 0.16 * v)[..., None]
        elif name == "crest":
            pixels = np.empty((*u.shape, 3), dtype=np.float64)
            pixels[:] = horn
            pixels *= (0.88 + 0.12 * u)[..., None]
            growth = np.cos(2 * np.pi * (16 * u + 0.7 * fine))
            pixels *= (0.94 + 0.06 * growth)[..., None]
        elif name == "beak":
            pixels = np.empty((*u.shape, 3), dtype=np.float64)
            pixels[:] = hoof
            pixels *= (0.78 + 0.20 * v)[..., None]
        elif name == "limbs":
            pixels = np.empty((*u.shape, 3), dtype=np.float64)
            pixels[:] = coat * 0.78
            pixels *= (0.80 + 0.20 * v)[..., None]
        else:
            pixels = np.empty((*u.shape, 3), dtype=np.float64)
            pixels[:] = coat
        result[y0:y1, x0:x1] = np.clip(pixels * grain[..., None], 0, 1)
    return result


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", choices=tuple(COATS), required=True)
    parser.add_argument("--out", required=True)
    parser.add_argument("--size", type=int, default=1024)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:]
                             if "--" in sys.argv else [])
    if not 128 <= args.size <= 4096:
        raise SystemExit("unsupported albedo size")
    shared.write_png(args.out, paint(args.kind, args.size))
    print("[ungulate] painted " + args.kind + " " + args.out)


if __name__ == "__main__":
    main()
