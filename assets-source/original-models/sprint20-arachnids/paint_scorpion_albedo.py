#!/usr/bin/env python3
"""The original Giant Scorpion painting.

Project-owned procedural pixels. No game texture, photograph or
generative-model output is an input; every value below is computed from
noise and the shared UV atlas the other bodies on this rig use.

The creature is a desert scorpion: warm sand-brown chitin with the darker
banding a scorpion shows at every segment joint, a carapace a shade deeper
than the legs so eight thin limbs stay readable against it, and a tail that
darkens along its arc toward a near-black sting. The one saturated thing in
the whole painting is the bead of venom at the sting's tip, because that is
the creature's printed attack and it is what a player should see first.

Deliberately NOT the Sprint 14 insects' palette. Those share this rig and a
player chooses between them in the same menu: the ants are red-brown and the
beetles near-black, so this one is pale warm sand, which is the one place in
that family where a scorpion can be told apart at icon size.
"""
import argparse
from pathlib import Path
import sys

import numpy as np

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve()
sys.path.insert(0, str(HERE.parents[1] / "pteranodon"))
sys.path.insert(0, str(HERE.parents[1] / "sprint14-insects"))
import paint_pteranodon_albedo as shared  # noqa: E402
import paint_sprint14_albedo as insects  # noqa: E402

KIND = "giant-scorpion"
colour = insects.colour
segments = insects.segments
striae = insects.striae

# Warm sand, a touch grey so it does not read as plastic.
CHITIN = (0.372, 0.258, 0.132)
CHITIN_DARK = (0.208, 0.138, 0.066)
PLATE = (0.412, 0.292, 0.150)
LIMB = (0.330, 0.226, 0.116)
JOINT = (0.166, 0.106, 0.050)
POINT = (0.088, 0.054, 0.030)
POINT_TIP = (0.030, 0.019, 0.013)
VENOM = (0.58, 0.74, 0.22)
EYE = (0.040, 0.034, 0.030)


def carapace(rng, u, v):
    """The prosoma and pre-abdomen: banded plate, darker at each joint."""
    h, w = u.shape
    pixels = np.broadcast_to(colour(CHITIN), (h, w, 3)).copy()
    pixels *= (0.86 + 0.24 * shared.fbm(rng, h, w, 16, 4))[..., None]
    # Segment joints across the body, which is what makes a chitinous
    # abdomen read as segmented rather than as one smooth sausage.
    pixels = shared.mix(pixels, colour(CHITIN_DARK), 0.42 * segments(u, 6.0))
    # A darker median ridge down the carapace.
    ridge = np.exp(-((v - 0.5) ** 2) / 0.004)
    pixels = shared.mix(pixels, colour(CHITIN_DARK), 0.26 * ridge)
    # Fine striae, the grain real chitin has.
    pixels = shared.mix(pixels, colour(JOINT), 0.10 * striae(v, 48.0))
    return pixels


def tail(rng, u, v):
    """The metasoma: the same plate, darkening along the arc to the sting.

    The arc's progress is u, so the gradient runs from the tail root to the
    telson and the sting end is almost black. That is the one cue a player
    gets about which end stings, and the geometry cannot give it: the whole
    tail rides one abdomen chain and never moves independently.
    """
    h, w = u.shape
    pixels = np.broadcast_to(colour(PLATE), (h, w, 3)).copy()
    pixels *= (0.88 + 0.22 * shared.fbm(rng, h, w, 20, 4))[..., None]
    # Heavy banding: a scorpion's tail is its most obviously segmented part.
    pixels = shared.mix(pixels, colour(CHITIN_DARK), 0.52 * segments(u, 9.0))
    # Darken toward the telson.
    pixels = shared.mix(pixels, colour(POINT), 0.55 * np.clip(u, 0.0, 1.0) ** 2)
    return pixels


def limbs(rng, u, v):
    """Eight legs and two chelae, a shade lighter than the carapace."""
    h, w = u.shape
    pixels = np.broadcast_to(colour(LIMB), (h, w, 3)).copy()
    pixels *= (0.82 + 0.30 * shared.fbm(rng, h, w, 26, 3))[..., None]
    pixels = shared.mix(pixels, colour(JOINT), 0.38 * segments(u, 5.0))
    # The chela fingers darken to a hard working edge, as the claws do.
    pixels = shared.mix(pixels, colour(POINT),
                        0.42 * np.clip((u - 0.72) / 0.28, 0.0, 1.0))
    return pixels


def points(rng, u, v):
    """Chelicerae, the eyes and the sting itself.

    The sting's venom bead lives here, at the far end of u, and it is the
    only saturated colour on the creature.
    """
    h, w = u.shape
    pixels = np.broadcast_to(colour(POINT), (h, w, 3)).copy()
    pixels *= (0.84 + 0.26 * shared.fbm(rng, h, w, 30, 3))[..., None]
    pixels = shared.mix(pixels, colour(POINT_TIP),
                        0.70 * np.clip((u - 0.55) / 0.45, 0.0, 1.0))
    # The eyes: a small dark cluster near the region's start.
    eye = np.exp(-(((u - 0.12) ** 2) / 0.0016 + ((v - 0.5) ** 2) / 0.010))
    pixels = shared.mix(pixels, colour(EYE), 0.80 * eye)
    # The venom bead, at the very tip.
    bead = np.exp(-(((u - 0.965) ** 2) / 0.0006 + ((v - 0.5) ** 2) / 0.020))
    pixels = shared.mix(pixels, colour(VENOM), 0.92 * bead)
    return pixels


def paint(size):
    result = np.zeros((size, size, 3), dtype=np.float64)
    seed = 20261010
    for index, name in enumerate(sorted(shared.ATLAS)):
        rng = np.random.default_rng(seed + index * 137)
        (y0, y1, x0, x1), u, v = shared.region_canvas(size, name)
        h, w = u.shape
        if name == "body":
            pixels = carapace(rng, u, v)
        elif name == "crest":
            pixels = tail(rng, u, v)
        elif name == "limbs":
            pixels = limbs(rng, u, v)
        elif name == "beak":
            pixels = points(rng, u, v)
        else:
            # The membrane region carries no scorpion geometry at all. It is
            # filled with the carapace colour rather than left black so a
            # stray sample can never read as a hole in the creature.
            pixels = np.broadcast_to(colour(CHITIN_DARK), (h, w, 3)).copy()
        result[y0:y1, x0:x1] = np.clip(pixels, 0.0, 1.0)
    return result


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--out", required=True)
    parser.add_argument("--size", type=int, default=1024)
    arguments = parser.parse_args(
        sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else None)
    if not 128 <= arguments.size <= 4096:
        raise SystemExit("unsupported albedo size")
    shared.write_png(arguments.out, paint(arguments.size))
    print("painted", KIND, "->", arguments.out)


if __name__ == "__main__":
    main()
