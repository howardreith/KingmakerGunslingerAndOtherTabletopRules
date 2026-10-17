#!/usr/bin/env python3
"""Deterministic original crocodilian scale, scute, tooth and eye atlas.

No native pixels, photos, scans, or downloaded texture enter this painting.
The established shared atlas/exporter makes the output compatible with the
project-owned skinned-mesh loader. Native materials supply runtime shading.

Visual refinement, 2026-10-17 (VIS-012, VIS-034): the first painting was a
dark, regular tile grid that read as blocky. This one is lighter, with
irregular scales of varied tone, cream ventral shields, broken blotches and
soft tail crossbands.
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


def cells(rng, h, w, columns, rows, jitter=0.8):
    """Jittered-grid Voronoi scales over a region: the gap to the nearest
    neighbouring cell (F2 - F1, in cell units) and each scale's own tone."""
    points = rng.random((rows + 2, columns + 2, 2))
    shades = rng.random((rows + 2, columns + 2))
    ys = (np.arange(h) + 0.5) / h * rows
    xs = (np.arange(w) + 0.5) / w * columns
    X, Y = np.meshgrid(xs, ys)
    cx, cy = np.floor(X).astype(int), np.floor(Y).astype(int)
    f1 = np.full((h, w), 9.0)
    f2 = np.full((h, w), 9.0)
    tone = np.zeros((h, w))
    for dy in (-1, 0, 1):
        for dx in (-1, 0, 1):
            gx = np.clip(cx + dx, -1, columns)
            gy = np.clip(cy + dy, -1, rows)
            offset = points[gy + 1, gx + 1]
            px = gx + 0.5 + (offset[..., 0] - 0.5) * jitter
            py = gy + 0.5 + (offset[..., 1] - 0.5) * jitter
            d = np.hypot(X - px, Y - py)
            f2 = np.where(d < f1, f1, np.minimum(f2, d))
            tone = np.where(d < f1, shades[gy + 1, gx + 1], tone)
            f1 = np.minimum(f1, d)
    return f2 - f1, tone


def shields(u, v, columns, rows):
    """Soft rectangular rows: the ventral shields."""
    x = np.mod(u * columns, 1.0)
    y = np.mod(v * rows, 1.0)
    edge = np.minimum(np.minimum(x, 1.0 - x), np.minimum(y, 1.0 - y))
    return shared.smoothstep(0.02, 0.16, edge)


def paint(kind, size):
    dire = kind == "dire-crocodile"
    result = np.zeros((size, size, 3), dtype=np.float64)
    seed = 20261017 + KINDS.index(kind) * 137
    for index, name in enumerate(sorted(shared.ATLAS)):
        rng = np.random.default_rng(seed + index)
        (y0, y1, x0, x1), u, v = shared.region_canvas(size, name)
        h, w = u.shape
        fine = shared.fbm(rng, h, w, 22, 3) - 0.5
        mottling = shared.fbm(rng, h, w, 4, 4) - 0.5
        if name in ("body", "limbs", "membrane"):
            # Ordinary: warm olive back and yellow-olive flanks over a cream
            # belly. Dire: colder slate-olive, not merely the same skin
            # enlarged. The game lights albedos warmly and brightly.
            if dire:
                back = colour((0.235, 0.262, 0.215))
                flank = colour((0.330, 0.345, 0.270))
                belly = colour((0.520, 0.500, 0.395))
            else:
                back = colour((0.285, 0.290, 0.170))
                flank = colour((0.395, 0.375, 0.225))
                belly = colour((0.600, 0.545, 0.375))
            # v folds the ring: 0 belly, 0.5 flank, 1 back.
            if name == "body":
                pixels = shared.mix(belly, flank, shared.smoothstep(0.16, 0.42, v))
                pixels = shared.mix(pixels, back, shared.smoothstep(0.55, 0.85, v))
            elif name == "membrane":
                pixels = shared.mix(flank, back, shared.smoothstep(0.35, 0.80, v))
            else:
                pixels = shared.mix(flank, back, shared.smoothstep(0.30, 0.85, v))
            # Irregular scales of varied tone with gently darkened seams,
            # not a black grid; cream ventral shields under the torso.
            columns, rows = ((40, 14) if name == "body" else
                             (34, 10) if name == "membrane" else (26, 9))
            gap, tone = cells(rng, h, w, columns, rows)
            shade = ((0.90 + 0.10 * shared.smoothstep(0.0, 0.16, gap)) *
                     (0.90 + 0.20 * tone))
            if name == "body":
                ventral = 1.0 - shared.smoothstep(0.16, 0.30, v)
                plates = shields(u, v, 30, 10)
                shade = shade * (1.0 - ventral) + (0.90 + 0.10 * plates) * ventral
            pixels = pixels * (shade + 0.08 * fine)[..., None]
            # Dark irregular blotches across the back and flanks and soft,
            # broken crossbands down the tail; none on the belly, and none
            # at the tail region's u edges, where the osteoderm blocks
            # sample their colour.
            blotch = (shared.smoothstep(0.10, 0.28, mottling) *
                      shared.smoothstep(0.35, 0.65, v))
            if name == "membrane":
                bands = shared.smoothstep(
                    0.45, 0.90, np.cos(u * np.pi * 12.0 + mottling * 1.2))
                bands *= shared.smoothstep(-0.15, 0.15, mottling + 0.1)
                blotch = np.maximum(blotch,
                                    bands * shared.smoothstep(0.30, 0.60, v))
                blotch *= (shared.smoothstep(0.03, 0.07, u) *
                           (1.0 - shared.smoothstep(0.93, 0.97, u)))
            pixels = pixels * (1.0 - 0.38 * blotch)[..., None]
        elif name == "beak":
            # Low-u: mouth lining. High-u: ivory teeth/claws. Geometry keeps
            # a gutter between the two, including the usual atlas inset.
            mouth = shared.mix(colour((0.20, 0.10, 0.09)),
                               colour((0.46, 0.27, 0.22)), v)
            enamel = shared.mix(colour((0.52, 0.46, 0.32)),
                                colour((0.86, 0.81, 0.62)), v)
            pixels = np.where((u < 0.5)[..., None], mouth, enamel)
            pixels *= (1.0 + 0.07 * fine)[..., None]
        else:
            # Amber iris and slit pupil, no glow or emissive map.
            iris = np.broadcast_to(colour((0.62, 0.48, 0.18)), (h, w, 3)).copy()
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
