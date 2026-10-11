#!/usr/bin/env python3
"""The original Giant Crab painting.

Project-owned procedural pixels. No game texture, photograph or
generative-model output is an input; every value below is computed from
noise and the shared UV atlas the other bodies on this rig use.

The creature is a wet grey-blue shell with pale mottling, which is the one
palette left in this family. Five creatures now share this skeleton and a
player picks between them in one menu: the ants are red-brown, the beetles
near-black, the scorpion pale warm sand, the Bebelith dark blue-black with
hot seams. A crab is cool and light, and it is the only one of the five
with a pale underside - which matters, because a broad shell seen from the
party camera shows its rim.

The pincers are painted apart from the shell and from the legs. They are
the creature's only attack and the thing a player watches, so they carry
the one strong value change on the animal: a pale hand hardening to a dark
orange-red working edge at the finger tips, which is what a real crab's
claws do and is readable at any distance.
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

KIND = "giant-crab"
colour = insects.colour
segments = insects.segments
striae = insects.striae

SHELL = (0.286, 0.326, 0.368)
SHELL_DARK = (0.146, 0.174, 0.206)
SHELL_PALE = (0.456, 0.482, 0.502)
LIMB = (0.232, 0.268, 0.306)
JOINT = (0.118, 0.140, 0.170)
CLAW = (0.372, 0.388, 0.398)
CLAW_EDGE = (0.344, 0.148, 0.086)
POINT = (0.090, 0.098, 0.112)
EYE = (0.024, 0.024, 0.028)


def carapace(rng, u, v):
    """The shell: wet grey-blue with pale mottling and a darker rim.

    Mottling rather than banding. The other arthropods on this rig are
    segmented animals and read as rings of plate; a crab's carapace is one
    continuous shield, so what gives it surface here is blotching at two
    scales and nothing periodic at all.
    """
    h, w = u.shape
    pixels = np.broadcast_to(colour(SHELL), (h, w, 3)).copy()
    pixels *= (0.82 + 0.30 * shared.fbm(rng, h, w, 9, 4))[..., None]
    # Two scales of blotch, the larger lighter and the finer darker.
    pixels = shared.mix(pixels, colour(SHELL_PALE),
                        0.34 * shared.fbm(rng, h, w, 5, 3))
    pixels = shared.mix(pixels, colour(SHELL_DARK),
                        0.26 * shared.fbm(rng, h, w, 23, 3))
    # The rim: the shell's edge is where it is thickest and darkest, and it
    # is the line that separates the animal from the ground under it.
    rim = np.clip((np.abs(v - 0.5) - 0.30) / 0.20, 0.0, 1.0)
    pixels = shared.mix(pixels, colour(SHELL_DARK), 0.50 * rim)
    pixels = shared.mix(pixels, colour(JOINT), 0.08 * striae(v, 30.0))
    return pixels


def pincers(rng, u, v):
    """The two pincers: pale hands hardening to a dark working edge.

    The one strong value change on the creature, and it is here because
    these are the only limbs that attack. u runs from the shoulder to the
    palm, so the gradient tells a player which end seizes.
    """
    h, w = u.shape
    pixels = np.broadcast_to(colour(CLAW), (h, w, 3)).copy()
    pixels *= (0.84 + 0.28 * shared.fbm(rng, h, w, 18, 4))[..., None]
    pixels = shared.mix(pixels, colour(JOINT), 0.34 * segments(u, 5.0))
    # The hand lightens and then the edge hardens to orange-red, which is
    # what a crab's claw actually looks like and is the only warm colour on
    # the animal.
    pixels = shared.mix(pixels, colour(SHELL_PALE),
                        0.40 * np.clip((u - 0.42) / 0.30, 0.0, 1.0))
    pixels = shared.mix(pixels, colour(CLAW_EDGE),
                        0.68 * np.clip((u - 0.78) / 0.22, 0.0, 1.0))
    return pixels


def limbs(rng, u, v):
    """Eight walking legs, a shade darker than the shell.

    Eight thin limbs under a broad shield have to stay separable from it
    and from each other, so they are darker than the carapace and their
    joints darker still. The last segment pales toward the tip, which is
    what makes eight legs read as eight rather than as a fringe.
    """
    h, w = u.shape
    pixels = np.broadcast_to(colour(LIMB), (h, w, 3)).copy()
    pixels *= (0.80 + 0.32 * shared.fbm(rng, h, w, 26, 3))[..., None]
    pixels = shared.mix(pixels, colour(JOINT), 0.44 * segments(u, 6.0))
    pixels = shared.mix(pixels, colour(SHELL_PALE),
                        0.30 * np.clip((u - 0.80) / 0.20, 0.0, 1.0))
    return pixels


def points(rng, u, v):
    """The eyestalks and the finger tips."""
    h, w = u.shape
    pixels = np.broadcast_to(colour(POINT), (h, w, 3)).copy()
    pixels *= (0.84 + 0.26 * shared.fbm(rng, h, w, 30, 3))[..., None]
    pixels = shared.mix(pixels, colour(CLAW_EDGE),
                        0.60 * np.clip((u - 0.50) / 0.50, 0.0, 1.0))
    # The two eyes, dark and close together near the region's start.
    for offset in (0.08, 0.14):
        eye = np.exp(-(((u - offset) ** 2) / 0.0014 +
                       ((v - 0.5) ** 2) / 0.012))
        pixels = shared.mix(pixels, colour(EYE), 0.86 * eye)
    return pixels


def paint(size):
    result = np.zeros((size, size, 3), dtype=np.float64)
    seed = 20261012
    for index, name in enumerate(sorted(shared.ATLAS)):
        rng = np.random.default_rng(seed + index * 151)
        (y0, y1, x0, x1), u, v = shared.region_canvas(size, name)
        h, w = u.shape
        if name == "body":
            pixels = carapace(rng, u, v)
        elif name == "crest":
            pixels = pincers(rng, u, v)
        elif name == "limbs":
            pixels = limbs(rng, u, v)
        elif name == "beak":
            pixels = points(rng, u, v)
        else:
            # The membrane region carries no crab geometry. Filled with the
            # shell's darkest value rather than left black so a stray
            # sample can never read as a hole in the creature.
            pixels = np.broadcast_to(colour(SHELL_DARK), (h, w, 3)).copy()
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
