#!/usr/bin/env python3
"""Deterministic original primate coats. No native or source image input.

Every pixel is computed from seeded noise and the region layout, so the
same command always writes the same bytes. Nothing is sampled from the
game, from a photograph, or from any other image.

The Ape is a silverback gorilla: near-black fur over a bare charcoal
chest, a silver saddle across the lower back, black facial skin and dark
nails. The Dire Ape is a Gigantopithecus: a russet-brown shaggy coat, a
straw-pale mane and crest, weathered grey facial skin, yellowed canines
and horn-coloured claws.
"""
import argparse
from pathlib import Path
import sys

import numpy as np

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve()
sys.path.insert(0, str(HERE.parents[1] / "pteranodon"))
sys.path.insert(0, str(HERE.parent))
import paint_pteranodon_albedo as shared
from primate_regions import KINDS, PRIMATE_ATLAS

shared.ATLAS.clear()
shared.ATLAS.update(PRIMATE_ATLAS)


def colour(values):
    return np.array(values, dtype=np.float64)


# The two coats, named rather than interpolated, so each is reviewable on
# its own terms instead of as a tint of the other.
COATS = {
    "ape": dict(
        fur=(0.085, 0.078, 0.082), furDeep=(0.030, 0.028, 0.034),
        saddle=(0.56, 0.56, 0.60), skin=(0.055, 0.050, 0.054),
        muzzle=(0.105, 0.088, 0.086), mane=(0.055, 0.052, 0.058),
        keratin=(0.17, 0.155, 0.150), gum=(0.26, 0.115, 0.125),
        tooth=(0.80, 0.78, 0.70), iris=(0.21, 0.125, 0.065),
        saddleCentre=0.30, saddleWidth=0.26, strands=118.0,
    ),
    "dire-ape": dict(
        fur=(0.225, 0.120, 0.062), furDeep=(0.098, 0.050, 0.028),
        saddle=(0.40, 0.29, 0.17), skin=(0.145, 0.120, 0.108),
        muzzle=(0.215, 0.165, 0.140), mane=(0.62, 0.50, 0.30),
        keratin=(0.62, 0.56, 0.44), gum=(0.30, 0.135, 0.130),
        tooth=(0.84, 0.79, 0.62), iris=(0.30, 0.165, 0.070),
        saddleCentre=0.46, saddleWidth=0.40, strands=86.0,
    ),
}


def coat_pixels(coat, rng, u, v, along_fur, grain):
    """Fur: deep at the roots, lit at the tips, with hair-length strands.

    `v` runs belly (0) through flank (0.5) to back (1) on the trunk and
    around the limb elsewhere, which is why the lighter chest and the
    saddle are both functions of v.
    """
    base = shared.mix(colour(coat["furDeep"]), colour(coat["fur"]),
                      shared.smoothstep(0.0, 0.75, v))
    # The bare chest an ape wears: fur thins to skin down the middle.
    chest = 1 - shared.smoothstep(0.80, 0.97, v)
    pixels = shared.mix(colour(coat["skin"]), base, chest)
    # The silver saddle of a mature male, across the lower back only.
    saddle = (1 - shared.smoothstep(0.0, coat["saddleWidth"],
                                    abs(u - coat["saddleCentre"]))) * \
        shared.smoothstep(0.55, 0.95, v)
    pixels = shared.mix(pixels, colour(coat["saddle"]), saddle * 0.85)
    strands = np.sin(u * coat["strands"] + grain * 7.5) * 0.5 + 0.5
    lengthwise = shared.fbm(rng, *u.shape, 9, 4, aspect=along_fur) - 0.5
    pixels *= (0.80 + 0.17 * strands + 0.26 * lengthwise + 0.10 * grain)[..., None]
    return pixels


def paint(kind, size):
    coat = COATS[kind]
    result = np.zeros((size, size, 3), dtype=np.float64)
    for index, name in enumerate(sorted(PRIMATE_ATLAS)):
        rng = np.random.default_rng(20261009 + KINDS.index(kind) * 677 + index * 29)
        (y0, y1, x0, x1), u, v = shared.region_canvas(size, name)
        height, width = u.shape
        grain = shared.fbm(rng, height, width, 17, 3) - 0.5
        if name == "torso":
            pixels = coat_pixels(coat, rng, u, v, 3.5, grain)
        elif name == "limbs":
            pixels = coat_pixels(coat, rng, u, v, 5.0, grain)
            # Forearms and shins wear their fur shorter and darker.
            pixels = shared.mix(pixels, colour(coat["furDeep"]),
                                shared.smoothstep(0.55, 1.0, u) * 0.45)
        elif name == "mane":
            # Crest and collar. Each tuft is small and maps the whole
            # region, so any strong periodic pattern here aliases into
            # bands: the first pass striped the collar like a tiger. What
            # belongs here is one hair colour, darker at the roots, with
            # fine grain over it.
            pixels = shared.mix(colour(coat["furDeep"]), colour(coat["mane"]),
                                shared.smoothstep(0.05, 0.65, v))
            fine = shared.fbm(rng, height, width, 6, 4, aspect=4.0) - 0.5
            pixels *= (0.92 + 0.20 * fine + 0.06 * grain)[..., None]
        elif name == "face":
            # Bare skin: leathery, creased around the brow and muzzle,
            # with a warmer muzzle than the surrounding hide.
            creases = shared.fbm(rng, height, width, 11, 4, aspect=2.2)
            pixels = shared.mix(colour(coat["skin"]), colour(coat["muzzle"]),
                                shared.smoothstep(0.30, 0.85, v))
            pixels *= (0.86 + 0.28 * creases)[..., None]
        elif name == "eye":
            # Dark, wet and the same from every angle: the iris band sits
            # at the equator of the eyeball, so no camera finds a blank
            # sphere. Apes have dark sclera, so there is none to paint.
            pupil = 1 - shared.smoothstep(0.03, 0.12, abs(v - 0.5))
            pixels = shared.mix(colour(coat["iris"]), colour((0.016, 0.014, 0.013)),
                                pupil)
            highlight = (1 - shared.smoothstep(0.0, 0.09,
                                               np.hypot(u - 0.30, v - 0.66))) * 0.55
            pixels = shared.mix(pixels, colour((0.70, 0.70, 0.72)), highlight)
        elif name == "mouth":
            gum = shared.mix(colour((0.11, 0.045, 0.05)), colour(coat["gum"]), v)
            tooth = shared.mix(colour(coat["tooth"]) * 0.72, colour(coat["tooth"]), v)
            pixels = np.where((u < 0.5)[..., None], gum, tooth)
            pixels *= (1 + 0.06 * grain)[..., None]
        elif name == "hands":
            # Palms, knuckle pads and soles: bare, creased leather, paler
            # than the coat and darkest in the creases.
            leather = shared.mix(colour(coat["skin"]) * 1.5,
                                 colour(coat["muzzle"]) * 1.25,
                                 shared.smoothstep(0.2, 0.9, v))
            lines = shared.fbm(rng, height, width, 6, 3, aspect=3.0)
            pixels = shared.mix(leather, colour(coat["skin"]) * 0.8,
                                shared.smoothstep(0.42, 0.56, lines))
            pixels *= (0.9 + 0.2 * grain)[..., None]
        else:
            # Keratin: a dark nail on the Ape, a pale horn claw on the
            # Dire Ape, both darkening toward the root.
            pixels = shared.mix(colour(coat["keratin"]) * 0.55,
                                colour(coat["keratin"]),
                                shared.smoothstep(0.0, 0.8, u))
            ridges = np.sin(v * 28) * 0.5 + 0.5
            pixels *= (0.9 + 0.12 * ridges + 0.08 * grain)[..., None]
        result[y0:y1, x0:x1] = np.clip(pixels, 0, 1)
    return result


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", required=True, choices=KINDS)
    parser.add_argument("--out", required=True)
    parser.add_argument("--size", type=int, default=1024)
    args = parser.parse_args(
        sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    if not 128 <= args.size <= 4096:
        raise SystemExit("unsupported albedo size")
    Path(args.out).parent.mkdir(parents=True, exist_ok=True)
    shared.write_png(args.out, paint(args.kind, args.size))
    print("[primate-prototype] painted " + args.kind + " " + args.out)


if __name__ == "__main__":
    main()
