#!/usr/bin/env python3
"""Paint deterministic original Wolverine, Shadow Mastiff and Poison Frog atlases.

Each painting carries the one cue that identifies its species at a glance from
the party camera, because at that distance the silhouette and a single strong
marking are all a player actually reads:

- the Wolverine's pale flank stripe running hip to shoulder over a near-black
  coat, and the lighter mask across its face;
- the Shadow Mastiff's near-black coat with a cold blue sheen on the raised
  surfaces, so it reads as a thing made of shadow rather than a black dog;
- the Poison Frog's aposematic colouring, a saturated warning hue broken by
  dark blotches, which is also what separates it from the drab Giant Frog it
  shares a rig with.
"""
import argparse
from pathlib import Path
import sys

import numpy as np

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "pteranodon"))
import paint_pteranodon_albedo as shared  # noqa: E402


KINDS = ("wolverine", "shadow-mastiff", "poisonous-frog")


def colour(values):
    return np.array(values, dtype=np.float64)


def blotches(rng, u, v, count, minimum, maximum):
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
    if kind == "wolverine":
        # A dark brown-black coat with the pale band that runs along each
        # flank from the hip to the shoulder and meets across the rump. That
        # band is the single marking that names the animal.
        coat = colour((0.085, 0.062, 0.050))
        pixels = np.broadcast_to(coat, (h, w, 3)).copy()
        pixels *= (1.0 + 0.30 * fine)[..., None]
        band_centre = 0.56 + 0.05 * coarse
        band = np.exp(-(((v - band_centre) / 0.085) ** 2))
        pixels = shared.mix(pixels, colour((0.52, 0.42, 0.27)), 0.80 * band)
        # The belly stays dark; wolverines are not pale underneath.
        belly = 1.0 - shared.smoothstep(0.10, 0.34, v)
        pixels = shared.mix(pixels, colour((0.055, 0.042, 0.036)),
                            0.55 * belly)
        # A lighter mask over the muzzle and brow.
        mask = shared.smoothstep(0.80, 0.99, u) * \
            shared.smoothstep(0.35, 0.85, v)
        pixels = shared.mix(pixels, colour((0.40, 0.33, 0.24)), 0.45 * mask)
    elif kind == "shadow-mastiff":
        # Near-black, lifted only on the raised surfaces and only towards a
        # cold blue, so the creature reads as shadow-bodied rather than as a
        # black dog. Nothing here is lighter than a quarter value.
        coat = colour((0.052, 0.054, 0.070))
        pixels = np.broadcast_to(coat, (h, w, 3)).copy()
        sheen = shared.fbm(rng, h, w, 5, 5, aspect=4.0)
        pixels = shared.mix(pixels, colour((0.125, 0.140, 0.205)),
                            0.55 * shared.smoothstep(0.55, 0.95, sheen))
        pixels *= (1.0 + 0.22 * fine)[..., None]
        # The spine and shoulders catch what little light there is.
        dorsal = shared.smoothstep(0.76, 0.99, v)
        pixels = shared.mix(pixels, colour((0.150, 0.168, 0.240)),
                            0.40 * dorsal)
        # The underside falls away into near-nothing.
        belly = 1.0 - shared.smoothstep(0.08, 0.40, v)
        pixels = shared.mix(pixels, colour((0.022, 0.024, 0.034)),
                            0.70 * belly)
    else:
        # Aposematic colouring: a saturated warning ground broken by hard dark
        # blotches, wet-looking rather than furred.
        warning = colour((0.86, 0.52, 0.065))
        pixels = np.broadcast_to(warning, (h, w, 3)).copy()
        gradient = shared.smoothstep(0.15, 0.95, v)
        pixels = shared.mix(colour((0.72, 0.20, 0.055)), pixels, gradient)
        mark = blotches(rng, u, v, 42, 0.030, 0.085)
        pixels = shared.mix(pixels, colour((0.055, 0.045, 0.055)),
                            0.92 * mark)
        # Damp skin: a tight high-frequency sheen rather than fur grain.
        damp = shared.fbm(rng, h, w, 18, 4)
        pixels *= (0.88 + 0.26 * damp)[..., None]
        throat = 1.0 - shared.smoothstep(0.06, 0.26, v)
        pixels = shared.mix(pixels, colour((0.94, 0.86, 0.52)), 0.55 * throat)
    return pixels


def paint(kind, size):
    result = np.zeros((size, size, 3), dtype=np.float64)
    seed = 20261002 + KINDS.index(kind) * 97
    for index, name in enumerate(sorted(shared.ATLAS)):
        rng = np.random.default_rng(seed + index)
        (y0, y1, x0, x1), u, v = shared.region_canvas(size, name)
        h, w = u.shape
        grain = 0.88 + 0.20 * shared.fbm(rng, h, w, 12, 4)
        if name == "body":
            pixels = body(kind, rng, u, v)
        elif name == "limbs":
            # The Wolverine's legs and tail are the darkest part of it; the
            # Mastiff's stay in shadow; the frog's limbs carry the warning
            # ground so the whole animal reads as one hazard.
            base = ((0.055, 0.042, 0.034) if kind == "wolverine" else
                    (0.038, 0.040, 0.052) if kind == "shadow-mastiff" else
                    (0.78, 0.42, 0.075))
            pixels = np.broadcast_to(colour(base), (h, w, 3)).copy()
            texture = shared.fbm(rng, h, w, 22, 3)
            pixels *= (0.80 + 0.26 * texture)[..., None]
            if kind == "poisonous-frog":
                mark = blotches(rng, u, v, 16, 0.030, 0.070)
                pixels = shared.mix(pixels, colour((0.060, 0.050, 0.060)),
                                    0.85 * mark)
        elif name == "beak":
            base = ((0.115, 0.090, 0.075) if kind == "wolverine" else
                    (0.045, 0.047, 0.060) if kind == "shadow-mastiff" else
                    (0.60, 0.26, 0.095))
            tip = ((0.045, 0.035, 0.030) if kind == "wolverine" else
                   (0.018, 0.019, 0.026) if kind == "shadow-mastiff" else
                   (0.90, 0.80, 0.46))
            pixels = shared.mix(colour(base), colour(tip), u ** 1.4)
        elif name == "crest":
            # Eyes and nose pads. The Mastiff's eyes are the one bright thing
            # on it, which is what makes a shadow-coated hound readable at all.
            base = ((0.030, 0.024, 0.020) if kind == "wolverine" else
                    (0.62, 0.70, 0.92) if kind == "shadow-mastiff" else
                    (0.040, 0.034, 0.030))
            pixels = np.broadcast_to(colour(base), (h, w, 3)).copy()
            pixels *= (0.78 + 0.22 * v)[..., None]
        else:
            # Unused atlas space is still deterministic and opaque.
            base = ((0.14, 0.11, 0.09) if kind == "wolverine" else
                    (0.040, 0.042, 0.055) if kind == "shadow-mastiff" else
                    (0.52, 0.28, 0.070))
            pixels = np.broadcast_to(colour(base), (h, w, 3)).copy()
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
    print("[sprint13-creature] painted " + args.kind + " " + args.out)


if __name__ == "__main__":
    main()
