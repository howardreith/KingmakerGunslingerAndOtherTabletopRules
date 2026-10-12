#!/usr/bin/env python3
"""The original Bebelith painting.

Project-owned procedural pixels. No game texture, photograph or
generative-model output is an input; every value below is computed from
noise and the shared UV atlas the other bodies on this rig use.

The creature is a demon: dark blue-black chitin with a cold sheen, deeper
than anything else on this rig and deliberately so. Four creatures share
this skeleton and a player chooses between them in one menu - the ants are
red-brown, the beetles near-black, the scorpion pale warm sand - so this
one is the only cold-coloured animal in the family, and the only one whose
plates carry a faint red seam where they meet.

Those seams are the one warm thing in the painting, and they are where a
demon's heat shows through its armour. They run along the segment joints
rather than being sprayed over the whole body, so at party camera the
creature reads as dark blue plate with fire at the gaps, and at icon size
as a dark blue animal.

Deliberately NOT a Doomspider repaint. The released body a player sees today
is the game's own spider in the game's own colours; the point of this one is
that it cannot be mistaken for it, and the colour is half of that.
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

KIND = "bebelith"
colour = insects.colour
segments = insects.segments
striae = insects.striae

# Dark blue-black, with enough blue left in the midtones that it does not
# collapse to grey under the game's lighting.
CHITIN = (0.082, 0.098, 0.158)
CHITIN_DARK = (0.034, 0.042, 0.078)
PLATE = (0.112, 0.128, 0.196)
LIMB = (0.066, 0.080, 0.132)
JOINT = (0.026, 0.030, 0.056)
POINT = (0.040, 0.034, 0.038)
POINT_TIP = (0.014, 0.012, 0.016)
# The demon's heat, at the plate seams and nowhere else.
SEAM = (0.42, 0.13, 0.07)
EYE = (0.52, 0.09, 0.09)


def carapace(rng, u, v):
    """The cephalothorax and abdomen: dark plate with hot seams."""
    h, w = u.shape
    pixels = np.broadcast_to(colour(CHITIN), (h, w, 3)).copy()
    pixels *= (0.84 + 0.28 * shared.fbm(rng, h, w, 15, 4))[..., None]
    joints = segments(u, 5.0)
    pixels = shared.mix(pixels, colour(CHITIN_DARK), 0.46 * joints)
    # The seam glow, narrow and only in the darkest part of each joint, so
    # it reads as a gap in armour rather than as a stripe painted on it.
    pixels = shared.mix(pixels, colour(SEAM), 0.34 * joints ** 3)
    # A raised median ridge down the back, lighter than the flanks.
    ridge = np.exp(-((v - 0.5) ** 2) / 0.005)
    pixels = shared.mix(pixels, colour(PLATE), 0.30 * ridge)
    pixels = shared.mix(pixels, colour(JOINT), 0.12 * striae(v, 44.0))
    return pixels


def foreclaws(rng, u, v):
    """The two elevated barbed foreclaws, on their own region.

    They are the limb a spider does not have, so they are painted apart
    from the legs: a shade lighter at the arm so the raised silhouette
    separates from the dark body behind it, then hardening along the blade
    to a near-black working edge with hot seams at each barb root.
    """
    h, w = u.shape
    pixels = np.broadcast_to(colour(PLATE), (h, w, 3)).copy()
    pixels *= (0.86 + 0.26 * shared.fbm(rng, h, w, 22, 4))[..., None]
    pixels = shared.mix(pixels, colour(CHITIN_DARK), 0.44 * segments(u, 7.0))
    # Darken along the blade. u runs root to tip, so this is the gradient
    # that tells a player which end tears.
    pixels = shared.mix(pixels, colour(POINT),
                        0.62 * np.clip(u, 0.0, 1.0) ** 2)
    # Heat at the barb roots, which sit in the blade's second half.
    barbs = segments(np.clip((u - 0.45) / 0.55, 0.0, 1.0), 3.0)
    pixels = shared.mix(pixels, colour(SEAM), 0.26 * barbs ** 2)
    return pixels


def limbs(rng, u, v):
    """Six walking legs, darker than the body so they recede.

    The opposite choice to the scorpion's, on purpose. That creature's
    eight thin legs had to stay readable against a pale carapace; these six
    should fall back so the raised foreclaws and the shell carry the
    silhouette.
    """
    h, w = u.shape
    pixels = np.broadcast_to(colour(LIMB), (h, w, 3)).copy()
    pixels *= (0.80 + 0.32 * shared.fbm(rng, h, w, 28, 3))[..., None]
    pixels = shared.mix(pixels, colour(JOINT), 0.42 * segments(u, 6.0))
    pixels = shared.mix(pixels, colour(POINT),
                        0.46 * np.clip((u - 0.74) / 0.26, 0.0, 1.0))
    return pixels


def points(rng, u, v):
    """Mandibles, the eight eyes, the claw barbs and the spinnerets.

    The eyes are the only strongly saturated thing on the creature and they
    are red, because that is what says demon on an animal this dark.
    """
    h, w = u.shape
    pixels = np.broadcast_to(colour(POINT), (h, w, 3)).copy()
    pixels *= (0.82 + 0.28 * shared.fbm(rng, h, w, 32, 3))[..., None]
    pixels = shared.mix(pixels, colour(POINT_TIP),
                        0.72 * np.clip((u - 0.52) / 0.48, 0.0, 1.0))
    # Two rows of four eyes near the region's start.
    for row, (centre, spread) in enumerate(((0.44, 0.0018), (0.56, 0.0016))):
        for offset in (0.07, 0.13):
            eye = np.exp(-(((u - offset) ** 2) / 0.0012 +
                           ((v - centre) ** 2) / spread))
            pixels = shared.mix(pixels, colour(EYE), 0.84 * eye)
    return pixels


def paint(size):
    result = np.zeros((size, size, 3), dtype=np.float64)
    seed = 20261011
    for index, name in enumerate(sorted(shared.ATLAS)):
        rng = np.random.default_rng(seed + index * 149)
        (y0, y1, x0, x1), u, v = shared.region_canvas(size, name)
        h, w = u.shape
        if name == "body":
            pixels = carapace(rng, u, v)
        elif name == "crest":
            pixels = foreclaws(rng, u, v)
        elif name == "limbs":
            pixels = limbs(rng, u, v)
        elif name == "beak":
            pixels = points(rng, u, v)
        else:
            # The membrane region carries no bebelith geometry. Filled with
            # the body's darkest chitin rather than left black so a stray
            # sample can never read as a hole in the creature.
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
