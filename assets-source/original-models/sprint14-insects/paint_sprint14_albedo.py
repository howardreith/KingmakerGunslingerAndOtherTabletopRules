#!/usr/bin/env python3
"""Paint deterministic original Giant Ant Soldier and Fire Beetle atlases.

Each painting carries the one cue that names its creature from the party
camera, because at that distance the silhouette and a single strong marking are
all a player reads:

- the Giant Ant Soldier's reddish-brown chitin, segmented across the gaster and
  darkest on the oversized head and mandibles, which is what says "soldier"
  rather than "worker";
- the Fire Beetle's near-black body with a red cast, and the pair of
  luminescent glands painted as the only bright thing on the creature. The
  glands are what the source gives it instead of fire damage, so they are the
  painting's whole subject.

The beetle's elytra and its wings are both blades in the membrane region and
have to read as hard shell and translucent film respectively, so the region is
split: the elytra take its first half and the wings its second, with a gutter
between them that neither samples.
"""
import argparse
from pathlib import Path
import sys

import numpy as np

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "pteranodon"))
import paint_pteranodon_albedo as shared  # noqa: E402


KINDS = ("giant-ant-soldier", "fire-beetle")
# Where the membrane region's two tenants sit, matching the u_range values the
# generator gives each blade.
MEMBRANE_SPLIT = 0.5


def colour(values):
    return np.array(values, dtype=np.float64)


def segments(u, period, sharpness=2.2):
    """Chitinous plate banding across a tube's length."""
    wave = 0.5 + 0.5 * np.cos(2.0 * np.pi * u * period)
    return wave ** sharpness


def striae(v, period):
    """Lengthwise ridges, which on a blade vary across its width."""
    wave = 0.5 + 0.5 * np.cos(2.0 * np.pi * v * period)
    return wave ** 3.0


def body(kind, rng, u, v):
    h, w = u.shape
    fine = shared.fbm(rng, h, w, 9, 5, aspect=5.0) - 0.5
    coarse = shared.fbm(rng, h, w, 3, 4) - 0.5
    if kind == "giant-ant-soldier":
        # Reddish-brown chitin. The dorsal surfaces catch the light hard,
        # because chitin is glossy where fur is not, and the plate banding runs
        # across the body's length so the gaster reads as segmented.
        base = colour((0.155, 0.072, 0.040))
        pixels = np.broadcast_to(base, (h, w, 3)).copy()
        pixels *= (1.0 + 0.26 * fine)[..., None]
        gloss = shared.smoothstep(0.58, 0.99, v)
        pixels = shared.mix(pixels, colour((0.305, 0.160, 0.092)),
                            0.70 * gloss)
        band = segments(u + 0.05 * coarse, 7.0)
        pixels = shared.mix(pixels, colour((0.088, 0.040, 0.026)),
                            0.42 * band)
        underside = 1.0 - shared.smoothstep(0.05, 0.32, v)
        pixels = shared.mix(pixels, colour((0.072, 0.036, 0.024)),
                            0.65 * underside)
    else:
        # Near-black with a red cast, which is what "reddish-black beetle"
        # looks like once it is lit: dark everywhere, and red only where a
        # highlight falls.
        base = colour((0.092, 0.040, 0.033))
        pixels = np.broadcast_to(base, (h, w, 3)).copy()
        pixels *= (1.0 + 0.20 * fine)[..., None]
        gloss = shared.smoothstep(0.62, 0.99, v)
        pixels = shared.mix(pixels, colour((0.215, 0.082, 0.056)),
                            0.65 * gloss)
        underside = 1.0 - shared.smoothstep(0.04, 0.30, v)
        pixels = shared.mix(pixels, colour((0.040, 0.020, 0.018)),
                            0.70 * underside)
    return pixels


def membrane(kind, rng, u, v):
    h, w = u.shape
    if kind != "fire-beetle":
        # The ant weights nothing to this region. It still has to be opaque and
        # deterministic, so it takes the creature's darkest chitin.
        pixels = np.broadcast_to(colour((0.075, 0.036, 0.024)),
                                 (h, w, 3)).copy()
        return pixels * (0.86 + 0.22 * shared.fbm(rng, h, w, 10, 4))[..., None]
    shell = np.broadcast_to(colour((0.205, 0.080, 0.056)), (h, w, 3)).copy()
    ridge = striae(v, 9.0)
    shell = shared.mix(shell, colour((0.058, 0.024, 0.020)), 0.45 * ridge)
    shell = shared.mix(shell, colour((0.270, 0.105, 0.070)),
                       0.55 * shared.smoothstep(0.55, 0.95,
                                                shared.fbm(rng, h, w, 6, 4)))
    # The wings stay dim on purpose. At party-camera distance a pale membrane
    # is the brightest thing on the creature and takes the eye away from the
    # glands, and the glands are the whole point of a Fire Beetle.
    film = np.broadcast_to(colour((0.395, 0.360, 0.315)), (h, w, 3)).copy()
    vein = striae(v, 16.0)
    film = shared.mix(film, colour((0.215, 0.192, 0.168)), 0.60 * vein)
    # The wing darkens towards its root, where it is thickest and folds.
    film = shared.mix(colour((0.250, 0.228, 0.200)), film,
                      shared.smoothstep(0.52, 0.80, u))
    return np.where((u < MEMBRANE_SPLIT)[..., None], shell, film)


def paint(kind, size):
    result = np.zeros((size, size, 3), dtype=np.float64)
    seed = 20261003 + KINDS.index(kind) * 131
    for index, name in enumerate(sorted(shared.ATLAS)):
        rng = np.random.default_rng(seed + index)
        (y0, y1, x0, x1), u, v = shared.region_canvas(size, name)
        h, w = u.shape
        grain = 0.90 + 0.18 * shared.fbm(rng, h, w, 14, 4)
        ant = kind == "giant-ant-soldier"
        if name == "body":
            pixels = body(kind, rng, u, v)
        elif name == "membrane":
            pixels = membrane(kind, rng, u, v)
        elif name == "limbs":
            # Legs and antennae. The ant's are a shade lighter than its body,
            # which is what makes six thin limbs readable against a dark
            # ground; the beetle's stay as dark as its shell.
            base = ((0.182, 0.088, 0.048) if ant else (0.102, 0.048, 0.039))
            pixels = np.broadcast_to(colour(base), (h, w, 3)).copy()
            pixels *= (0.80 + 0.28 * shared.fbm(rng, h, w, 24, 3))[..., None]
            joint = segments(u, 4.0)
            pixels = shared.mix(pixels, colour((0.060, 0.030, 0.022)),
                                0.35 * joint)
        elif name == "beak":
            # Mandibles, and the soldier's sting. Hardened and near-black at
            # the working edge, which is where a bite reads from.
            base = ((0.088, 0.048, 0.032) if ant else (0.074, 0.038, 0.032))
            tip = ((0.026, 0.017, 0.014) if ant else (0.030, 0.018, 0.016))
            pixels = shared.mix(colour(base), colour(tip), u ** 1.3)
        elif name == "crest":
            if ant:
                # Compound eyes: dark and glossy, lifted only at the dome top.
                pixels = np.broadcast_to(colour((0.048, 0.042, 0.040)),
                                         (h, w, 3)).copy()
                pixels = shared.mix(pixels, colour((0.165, 0.150, 0.140)),
                                    0.60 * shared.smoothstep(0.62, 0.99, v))
            else:
                # The luminescent glands, and the one bright thing anywhere on
                # this creature. A hot centre falling off to a red rim is what
                # a lit gland looks like in albedo; the light itself, if a view
                # ever owns one, is a separate instance-local decision and is
                # not a claim that the game models illumination.
                pixels = np.broadcast_to(colour((0.780, 0.230, 0.070)),
                                         (h, w, 3)).copy()
                core = shared.smoothstep(0.35, 0.98, v)
                pixels = shared.mix(pixels, colour((1.000, 0.760, 0.330)),
                                    0.85 * core)
                rim = 1.0 - shared.smoothstep(0.02, 0.28, v)
                pixels = shared.mix(pixels, colour((0.420, 0.090, 0.030)),
                                    0.70 * rim)
        else:
            base = ((0.120, 0.058, 0.034) if ant else (0.078, 0.034, 0.028))
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
    print("[sprint14-insect] painted " + args.kind + " " + args.out)


if __name__ == "__main__":
    main()
