#!/usr/bin/env python3
"""Deterministic original Girallon and Xill coats. No image input of any kind.

Every pixel is computed from seeded noise and the region layout, so the same
command always writes the same bytes. Nothing is sampled from the game, from a
photograph, or from any other image.

The two are painted as different materials, not as two tints of one. The
Girallon is a white-furred ape: coarse off-white hair over grey skin, a dull
pewter mantle across the shoulders, dark facial leather, yellowed canines and
grey-black claws. The Xill is chitin: a lacquered rust-red carapace with
hard-edged segment banding, darker joint membrane, green compound eyes and
near-black claws. Fur is soft and lengthwise; chitin is hard and banded, so
they do not even share a noise field.
"""
import argparse
from pathlib import Path
import sys

import numpy as np

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve()
sys.path.insert(0, str(HERE.parents[1] / "pteranodon"))
sys.path.insert(0, str(HERE.parents[1] / "sprint18-primates"))
sys.path.insert(0, str(HERE.parent))
import paint_pteranodon_albedo as shared
from primate_regions import PRIMATE_ATLAS

# The eight-region atlas is the Sprint 18 one, reused because the Sprint 19
# bodies are unwrapped by the Sprint 18 exporter into exactly those regions.
shared.ATLAS.clear()
shared.ATLAS.update(PRIMATE_ATLAS)

KINDS = ("girallon", "xill")


def colour(values):
    return np.array(values, dtype=np.float64)


COATS = {
    "girallon": dict(
        chitin=False,
        # A girallon is white. The first pass painted it at 0.72 over a 0.40
        # root and the strand modulation, which multiplies by as little as
        # 0.80, carried the rendered coat down into grey. The hair is lighter
        # now and its roots are much lighter, so the creature reads white
        # under the party camera rather than merely pale.
        fur=(0.90, 0.888, 0.866), furDeep=(0.62, 0.610, 0.596),
        mantle=(0.44, 0.434, 0.432), skin=(0.145, 0.105, 0.098),
        muzzle=(0.215, 0.150, 0.138), mane=(0.47, 0.462, 0.452),
        keratin=(0.20, 0.188, 0.180), gum=(0.30, 0.125, 0.130),
        tooth=(0.86, 0.82, 0.68), iris=(0.52, 0.14, 0.07),
        mantleCentre=0.34, mantleWidth=0.30, strands=104.0,
    ),
    "xill": dict(
        chitin=True,
        fur=(0.46, 0.145, 0.072), furDeep=(0.195, 0.058, 0.030),
        mantle=(0.58, 0.215, 0.095), skin=(0.165, 0.062, 0.040),
        muzzle=(0.40, 0.135, 0.062), mane=(0.52, 0.185, 0.082),
        keratin=(0.085, 0.072, 0.068), gum=(0.14, 0.045, 0.042),
        tooth=(0.42, 0.40, 0.33), iris=(0.07, 0.46, 0.17),
        mantleCentre=0.50, mantleWidth=0.44, strands=0.0,
        bands=9.0,
    ),
}


def fur_pixels(coat, rng, u, v, along_fur, grain):
    """Coarse hair: deep at the roots, lit at the tips, strand by strand.

    `v` runs belly (0) through flank (0.5) to back (1) on the trunk and
    around the limb elsewhere, which is why both the paler chest and the
    mantle are functions of v.
    """
    base = shared.mix(colour(coat["furDeep"]), colour(coat["fur"]),
                      shared.smoothstep(0.0, 0.75, v))
    chest = 1 - shared.smoothstep(0.80, 0.97, v)
    pixels = shared.mix(colour(coat["skin"]), base, chest)
    mantle = (1 - shared.smoothstep(0.0, coat["mantleWidth"],
                                    abs(u - coat["mantleCentre"]))) * \
        shared.smoothstep(0.58, 0.96, v)
    pixels = shared.mix(pixels, colour(coat["mantle"]), mantle * 0.80)
    strands = np.sin(u * coat["strands"] + grain * 7.5) * 0.5 + 0.5
    lengthwise = shared.fbm(rng, *u.shape, 9, 4, aspect=along_fur) - 0.5
    pixels *= (0.80 + 0.17 * strands + 0.26 * lengthwise + 0.10 * grain)[..., None]
    return pixels


def chitin_pixels(coat, rng, u, v, bands, grain):
    """Lacquered plate: hard segment banding and a specular-looking roll.

    Nothing here is a fur field with a different colour. A carapace reads by
    its banding and by the way light rolls across a curved plate, so the
    value gradient runs around the limb rather than along it, and the bands
    are hard-edged rather than filamentary.
    """
    roll = shared.smoothstep(0.08, 0.92, v)
    pixels = shared.mix(colour(coat["furDeep"]), colour(coat["fur"]), roll)
    # Segment seams: a narrow dark line where one plate overlaps the next.
    phase = np.abs(((u * bands) % 1.0) - 0.5) * 2.0
    seam = 1 - shared.smoothstep(0.62, 0.94, phase)
    pixels = shared.mix(colour(coat["skin"]), pixels, seam)
    # The lit crown of each plate, just behind its leading seam.
    crown = (1 - shared.smoothstep(0.0, 0.34, np.abs(phase - 0.26))) * roll
    pixels = shared.mix(pixels, colour(coat["mantle"]), crown * 0.55)
    mottle = shared.fbm(rng, *u.shape, 13, 3, aspect=1.4) - 0.5
    pixels *= (0.90 + 0.16 * mottle + 0.08 * grain)[..., None]
    return pixels


def body_pixels(coat, rng, u, v, along, grain):
    if coat["chitin"]:
        return chitin_pixels(coat, rng, u, v, coat["bands"], grain)
    return fur_pixels(coat, rng, u, v, along, grain)


def paint(kind, size):
    coat = COATS[kind]
    chitin = coat["chitin"]
    result = np.zeros((size, size, 3), dtype=np.float64)
    for index, name in enumerate(sorted(PRIMATE_ATLAS)):
        rng = np.random.default_rng(
            20261010 + KINDS.index(kind) * 751 + index * 31)
        (y0, y1, x0, x1), u, v = shared.region_canvas(size, name)
        height, width = u.shape
        grain = shared.fbm(rng, height, width, 17, 3) - 0.5
        if name == "torso":
            pixels = body_pixels(coat, rng, u, v, 3.5, grain)
        elif name == "limbs":
            pixels = body_pixels(coat, rng, u, v, 5.0, grain)
            # The far end of every limb is darker: shorter fur on the ape,
            # harder and less lacquered plate on the outsider.
            # The ape's limb darkening is gentler than the outsider's: a
            # white creature whose arms go grey stops reading as white.
            pixels = shared.mix(pixels, colour(coat["furDeep"]),
                                shared.smoothstep(0.55, 1.0, u) *
                                (0.38 if chitin else 0.22))
        elif name == "mane":
            if chitin:
                # The Xill has no hair. This region carries its dorsal
                # plates, so it is painted as more carapace rather than as
                # a coat the creature does not have.
                pixels = chitin_pixels(coat, rng, u, v, 5.0, grain)
            else:
                # The mantle. Each tuft is small and maps the whole region,
                # so any strong periodic pattern aliases into bands - the
                # Sprint 18 collar was striped like a tiger before this was
                # understood. One hair colour, darker at the roots, with
                # fine grain over it.
                pixels = shared.mix(colour(coat["furDeep"]),
                                    colour(coat["mane"]),
                                    shared.smoothstep(0.05, 0.65, v))
                fine = shared.fbm(rng, height, width, 6, 4, aspect=4.0) - 0.5
                pixels *= (0.92 + 0.20 * fine + 0.06 * grain)[..., None]
        elif name == "face":
            if chitin:
                pixels = chitin_pixels(coat, rng, u, v, 3.0, grain)
                pixels = shared.mix(pixels, colour(coat["muzzle"]),
                                    shared.smoothstep(0.35, 0.9, v) * 0.5)
            else:
                creases = shared.fbm(rng, height, width, 11, 4, aspect=2.2)
                pixels = shared.mix(colour(coat["skin"]),
                                    colour(coat["muzzle"]),
                                    shared.smoothstep(0.30, 0.85, v))
                pixels *= (0.86 + 0.28 * creases)[..., None]
        elif name == "eye":
            if chitin:
                # A compound eye: a lens lattice, not a wet sphere. The
                # facets are the point, so they are painted rather than
                # left to a highlight.
                facets = (np.sin(u * 46) * np.sin(v * 46)) * 0.5 + 0.5
                pixels = shared.mix(colour(coat["iris"]) * 0.45,
                                    colour(coat["iris"]), facets)
                pixels *= (0.88 + 0.22 * grain)[..., None]
            else:
                pupil = 1 - shared.smoothstep(0.03, 0.12, abs(v - 0.5))
                pixels = shared.mix(colour(coat["iris"]),
                                    colour((0.016, 0.014, 0.013)), pupil)
                highlight = (1 - shared.smoothstep(
                    0.0, 0.09, np.hypot(u - 0.30, v - 0.66))) * 0.55
                pixels = shared.mix(pixels, colour((0.70, 0.70, 0.72)),
                                    highlight)
        elif name == "mouth":
            gum = shared.mix(colour((0.10, 0.040, 0.044)),
                             colour(coat["gum"]), v)
            tooth = shared.mix(colour(coat["tooth"]) * 0.72,
                               colour(coat["tooth"]), v)
            pixels = np.where((u < 0.5)[..., None], gum, tooth)
            pixels *= (1 + 0.06 * grain)[..., None]
        elif name == "hands":
            if chitin:
                # The underside of a claw-hand: joint membrane, darker and
                # duller than the plate it sits between.
                pixels = shared.mix(colour(coat["skin"]),
                                    colour(coat["furDeep"]),
                                    shared.smoothstep(0.2, 0.9, v))
                pixels *= (0.88 + 0.24 * grain)[..., None]
            else:
                leather = shared.mix(colour(coat["skin"]) * 1.5,
                                     colour(coat["muzzle"]) * 1.25,
                                     shared.smoothstep(0.2, 0.9, v))
                lines = shared.fbm(rng, height, width, 6, 3, aspect=3.0)
                pixels = shared.mix(leather, colour(coat["skin"]) * 0.8,
                                    shared.smoothstep(0.42, 0.56, lines))
                pixels *= (0.9 + 0.2 * grain)[..., None]
        else:
            # Keratin. Both creatures print claw attacks, so both have real
            # claws: grey-black horn on the ape, near-black chitin hooks on
            # the outsider, each darkening toward the root.
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
    print("[fourarmed-prototype] painted " + args.kind + " " + args.out)


if __name__ == "__main__":
    main()
