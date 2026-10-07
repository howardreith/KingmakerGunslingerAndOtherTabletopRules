#!/usr/bin/env python3
"""Paint deterministic original Dire Rat, Hyena and Goblin Dog atlases."""
import argparse
from pathlib import Path
import sys

import numpy as np

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "pteranodon"))
import paint_pteranodon_albedo as shared  # noqa: E402


KINDS = ("dire-rat", "hyena", "goblin-dog")


def colour(values):
    return np.array(values, dtype=np.float64)


def spots(rng, u, v, count, minimum, maximum):
    result = np.zeros_like(u)
    for _ in range(count):
        x = rng.uniform(0.02, 0.98)
        y = rng.uniform(0.08, 0.98)
        rx = rng.uniform(minimum, maximum)
        ry = rx * rng.uniform(0.65, 1.35)
        distance = ((u - x) / rx) ** 2 + ((v - y) / ry) ** 2
        result = np.maximum(result, np.clip(1.0 - distance, 0.0, 1.0) ** 2)
    return result


def body(kind, rng, u, v):
    h, w = u.shape
    fine = shared.fbm(rng, h, w, 7, 5, aspect=7.0) - 0.5
    coarse = shared.fbm(rng, h, w, 3, 4) - 0.5
    if kind == "dire-rat":
        back, belly = colour((0.19, 0.135, 0.095)), colour((0.48, 0.39, 0.29))
        edge = shared.smoothstep(0.30 + 0.08 * coarse,
                                 0.68 + 0.08 * coarse, v)
        pixels = shared.mix(belly, back, edge)
        pixels *= (1.0 + 0.28 * fine)[..., None]
        dorsal = shared.smoothstep(0.84, 0.98, v)
        pixels = shared.mix(pixels, colour((0.095, 0.07, 0.055)),
                            0.55 * dorsal)
    elif kind == "hyena":
        back, belly = colour((0.48, 0.29, 0.105)), colour((0.70, 0.52, 0.27))
        pixels = shared.mix(belly, back, shared.smoothstep(0.25, 0.80, v))
        mark = spots(rng, u, v, 54, 0.025, 0.075)
        pixels = shared.mix(pixels, colour((0.095, 0.065, 0.045)),
                            0.88 * mark)
        pixels *= (1.0 + 0.17 * fine)[..., None]
        saddle = shared.smoothstep(0.78, 0.98, v) * \
            (1.0 - shared.smoothstep(0.82, 0.98, u))
        pixels = shared.mix(pixels, colour((0.12, 0.075, 0.045)),
                            0.48 * saddle)
    else:
        skin = colour((0.39, 0.31, 0.27))
        pixels = np.broadcast_to(skin, (h, w, 3)).copy()
        mottling = shared.fbm(rng, h, w, 4, 5)
        pixels *= (0.76 + 0.34 * mottling)[..., None]
        mange = spots(rng, u, v, 34, 0.025, 0.095)
        pixels = shared.mix(pixels, colour((0.16, 0.12, 0.105)),
                            0.72 * mange)
        inflamed = spots(rng, u + 0.071, v, 18, 0.018, 0.055)
        pixels = shared.mix(pixels, colour((0.47, 0.17, 0.14)),
                            0.36 * inflamed)
        ribs = (0.5 + 0.5 * np.cos(2 * np.pi * (8.0 * u + 0.13 * coarse))) ** 18
        ribs *= (1.0 - shared.smoothstep(0.56, 0.78, v))
        pixels *= (1.0 - 0.16 * ribs)[..., None]
    return pixels


def paint(kind, size):
    result = np.zeros((size, size, 3), dtype=np.float64)
    seed = 20260930 + KINDS.index(kind) * 101
    for index, name in enumerate(sorted(shared.ATLAS)):
        rng = np.random.default_rng(seed + index)
        (y0, y1, x0, x1), u, v = shared.region_canvas(size, name)
        h, w = u.shape
        grain = 0.88 + 0.20 * shared.fbm(rng, h, w, 12, 4)
        if name == "body":
            pixels = body(kind, rng, u, v)
        elif name == "limbs":
            base = ((0.47, 0.25, 0.22) if kind == "dire-rat" else
                    (0.40, 0.28, 0.15) if kind == "hyena" else
                    (0.42, 0.27, 0.25))
            pixels = np.broadcast_to(colour(base), (h, w, 3)).copy()
            scales = shared.fbm(rng, h, w, 24, 3)
            pixels *= (0.78 + 0.28 * scales)[..., None]
        elif name == "beak":
            base = ((0.58, 0.32, 0.30) if kind == "dire-rat" else
                    (0.20, 0.16, 0.12) if kind == "hyena" else
                    (0.72, 0.63, 0.43))
            tip = ((0.28, 0.16, 0.15) if kind == "dire-rat" else
                   (0.09, 0.07, 0.055) if kind == "hyena" else
                   (0.88, 0.80, 0.58))
            pixels = shared.mix(colour(base), colour(tip), u ** 1.4)
        elif name == "crest":
            base = ((0.028, 0.022, 0.018) if kind != "hyena" else
                    (0.11, 0.065, 0.035))
            pixels = np.broadcast_to(colour(base), (h, w, 3)).copy()
            pixels *= (0.78 + 0.22 * v)[..., None]
        else:
            # Unused atlas space is still deterministic and opaque.
            pixels = np.broadcast_to(colour((0.20, 0.16, 0.14)),
                                     (h, w, 3)).copy()
        result[y0:y1, x0:x1] = np.clip(pixels * grain[..., None], 0, 1)
    return result


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", choices=KINDS, required=True)
    parser.add_argument("--out", required=True)
    parser.add_argument("--size", type=int, default=1024)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:]
                             if "--" in sys.argv else [])
    if not 128 <= args.size <= 4096:
        raise SystemExit("unsupported albedo size")
    shared.write_png(args.out, paint(args.kind, args.size))
    print("[sprint12-quadruped] painted " + args.kind + " " + args.out)


if __name__ == "__main__":
    main()
