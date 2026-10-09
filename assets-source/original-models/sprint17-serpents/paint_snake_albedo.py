#!/usr/bin/env python3
"""Deterministic original snake-scale atlas; no native/source image input."""
import argparse
from pathlib import Path
import sys

import numpy as np

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "pteranodon"))
import paint_pteranodon_albedo as shared

KINDS = ("viper", "constrictor-snake")


def colour(values):
    return np.array(values, dtype=np.float64)


def paint(kind, size):
    viper = kind == "viper"
    result = np.zeros((size, size, 3), dtype=np.float64)
    for index, name in enumerate(sorted(shared.ATLAS)):
        rng = np.random.default_rng(20261006 + KINDS.index(kind) * 313 + index)
        (y0, y1, x0, x1), u, v = shared.region_canvas(size, name)
        h, w = u.shape
        grain = shared.fbm(rng, h, w, 15, 3) - .5
        if name in ("body", "limbs", "membrane"):
            # Tawny, keeled diamond-backed viper; olive and saddle-marked boa.
            base = colour((.44, .31, .16) if viper else (.29, .33, .19))
            dark = colour((.17, .13, .075) if viper else (.105, .15, .095))
            cream = colour((.68, .57, .34) if viper else (.57, .54, .32))
            longitudinal = u * 27
            if viper:
                diamond = abs(np.mod(longitudinal, 1) - .5) * 2 + (1 - v) * 1.6
                border = 1 - shared.smoothstep(.76, .88, diamond)
                centre = 1 - shared.smoothstep(.57, .66, diamond)
                pixels = shared.mix(base, cream, border)
                pixels = shared.mix(pixels, dark, centre)
            else:
                saddle = np.cos(longitudinal * 2 * np.pi) + .55 * np.cos(v * 3 * np.pi)
                pixels = shared.mix(base, dark, shared.smoothstep(.25, .55, saddle))
            ventral = 1 - shared.smoothstep(.16, .32, v)
            pixels = shared.mix(pixels, cream, ventral)
            row = np.floor(v * 13)
            x = np.mod(u * 145 + .5 * np.mod(row, 2), 1)
            y = np.mod(v * 13, 1)
            edge = np.minimum(np.minimum(x, 1 - x), np.minimum(y, 1 - y))
            scale = shared.smoothstep(.015, .12, edge)
            keel = np.exp(-((x - .5) / .12) ** 2) * np.sin(y * np.pi) ** 2
            pixels *= (.78 + .22 * scale + (.09 if viper else .03) * keel +
                       .12 * grain)[..., None]
        elif name == "beak":
            mouth = shared.mix(colour((.12, .04, .045)), colour((.33, .13, .15)), v)
            tooth = shared.mix(colour((.55, .46, .28)), colour((.89, .84, .67)), v)
            pixels = np.where((u < .5)[..., None], mouth, tooth)
            pixels *= (1 + .05 * grain)[..., None]
        else:
            iris = np.broadcast_to(colour((.65, .40, .10) if viper else
                                          (.53, .39, .14)), (h, w, 3)).copy()
            pupil = 1 - shared.smoothstep(.045, .095, abs(u - .5))
            pixels = shared.mix(iris, colour((.018, .02, .014)), pupil)
        result[y0:y1, x0:x1] = np.clip(pixels, 0, 1)
    return result


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", required=True, choices=KINDS)
    parser.add_argument("--out", required=True)
    parser.add_argument("--size", type=int, default=1024)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    if not 128 <= args.size <= 4096:
        raise SystemExit("unsupported albedo size")
    Path(args.out).parent.mkdir(parents=True, exist_ok=True)
    shared.write_png(args.out, paint(args.kind, args.size))
    print("[snake-prototype] painted " + args.kind + " " + args.out)


if __name__ == "__main__":
    main()
