#!/usr/bin/env python3
"""Deterministic original crocodilian scale, scute, tooth and eye atlas.

No native pixels, photos, scans, or downloaded texture enter this painting.
The established shared atlas/exporter makes the output compatible with the
project-owned skinned-mesh loader. Native materials supply runtime shading.
"""
import argparse
from pathlib import Path
import sys

import numpy as np

sys.dont_write_bytecode = True
ROOT = next(path for path in Path(__file__).resolve().parents
            if (path / "Info.json").is_file())
sys.path.insert(0, str(ROOT / "assets-source/original-models/pteranodon"))
import paint_pteranodon_albedo as shared  # noqa: E402

KINDS = ("crocodile", "dire-crocodile")


def colour(values):
    return np.array(values, dtype=np.float64)


def scales(u, v, columns, rows):
    """Rounded staggered plates, with a dark seam and a shallow dorsal keel."""
    row = np.floor(v * rows)
    x = np.mod(u * columns + 0.5 * np.mod(row, 2), 1.0)
    y = np.mod(v * rows, 1.0)
    edge = np.minimum(np.minimum(x, 1.0 - x), np.minimum(y, 1.0 - y))
    plate = shared.smoothstep(0.015, 0.14, edge)
    keel = np.exp(-((x - 0.5) / 0.10) ** 2) * np.sin(np.pi * y) ** 2
    return plate, keel


def paint(kind, size):
    dire = kind == "dire-crocodile"
    result = np.zeros((size, size, 3), dtype=np.float64)
    seed = 20261005 + KINDS.index(kind) * 137
    for index, name in enumerate(sorted(shared.ATLAS)):
        rng = np.random.default_rng(seed + index)
        (y0, y1, x0, x1), u, v = shared.region_canvas(size, name)
        h, w = u.shape
        fine = shared.fbm(rng, h, w, 22, 3) - 0.5
        mottling = shared.fbm(rng, h, w, 5, 4) - 0.5
        if name in ("body", "limbs", "membrane"):
            # Ordinary: warm olive hide, cream ventral shields. Dire: colder
            # slate-olive with stronger scutes, not merely the same skin enlarged.
            dorsal = colour((0.155, 0.181, 0.125) if not dire else
                            (0.125, 0.156, 0.153))
            belly = colour((0.48, 0.425, 0.285) if not dire else
                           (0.36, 0.365, 0.285))
            ventral = 1.0 - shared.smoothstep(0.12, 0.44, v)
            if name != "body":
                ventral = np.zeros_like(v)
            pixels = shared.mix(dorsal, belly, ventral)
            count = (18, 9) if name == "body" else (12, 6)
            plate, keel = scales(u, v, *count)
            pixels *= (0.69 + 0.31 * plate + 0.12 * keel +
                       0.19 * fine + 0.24 * mottling)[..., None]
            # Broken tail bands read from the party camera without replacing
            # the scale pattern with stripes across the entire animal.
            bands = (0.5 + 0.5 * np.cos(u * np.pi * 16.0)) ** 5
            if name == "membrane":
                pixels *= (0.90 - 0.16 * bands + 0.15 * keel)[..., None]
        elif name == "beak":
            # Low-u: mouth lining. High-u: ivory teeth/claws. Geometry keeps
            # a gutter between the two, including the usual atlas inset.
            mouth = shared.mix(colour((0.105, 0.052, 0.048)),
                               colour((0.29, 0.16, 0.13)), v)
            enamel = shared.mix(colour((0.43, 0.37, 0.235)),
                                colour((0.85, 0.79, 0.58)), v)
            pixels = np.where((u < 0.5)[..., None], mouth, enamel)
            pixels *= (1.0 + 0.07 * fine)[..., None]
        else:
            # Amber iris and slit pupil, no glow or emissive map.
            iris = np.broadcast_to(colour((0.46, 0.34, 0.12)), (h, w, 3)).copy()
            iris *= (0.88 + 0.19 * np.cos(u * 2 * np.pi * 18) ** 2)[..., None]
            pupil = 1.0 - shared.smoothstep(0.055, 0.10, abs(u - 0.5))
            pixels = shared.mix(iris, colour((0.022, 0.028, 0.019)), pupil)
        result[y0:y1, x0:x1] = np.clip(pixels, 0, 1)
    return result


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", required=True, choices=KINDS)
    parser.add_argument("--out", required=True)
    parser.add_argument("--size", type=int, default=1024)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:]
                             if "--" in sys.argv else [])
    if not 128 <= args.size <= 4096:
        raise SystemExit("unsupported albedo size")
    Path(args.out).parent.mkdir(parents=True, exist_ok=True)
    shared.write_png(args.out, paint(args.kind, args.size))
    print("[crocodilian] painted " + args.kind + " " + args.out)


if __name__ == "__main__":
    main()
