#!/usr/bin/env python3
"""Paint the Pteranodon albedo into the atlas the generator maps to.

Run headless, under Blender's Python because that interpreter is the one with
numpy here and because Blender writes the PNG:

    blender --background --factory-startup --python paint_pteranodon_albedo.py \
        -- --out pteranodon-albedo.png [--size 1024]

Every mark is a closed-form or seeded-noise function of (u, v): the same
arguments give the same bytes, which the build report and the mesh data pin by
hash. No photograph, scan, downloaded, traced or generative-model image is an
input; the palette and patterns are described in body-plan.md.

The regions come from `generate_pteranodon.ATLAS`, so the two scripts cannot
drift apart. Coordinates here are Unity's: u across, v UP, (0, 0) at the
bottom-left of the image.
"""
import argparse
from pathlib import Path
import sys

import numpy as np

# No bytecode cache beside the sources: the import is for one table.
sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
from generate_pteranodon import ATLAS  # noqa: E402

SEED = 20260923


def parse_args():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--out", required=True)
    parser.add_argument("--size", type=int, default=1024)
    return parser.parse_args(argv)


# --- noise ----------------------------------------------------------------

def value_noise(rng, height, width, cells_y, cells_x):
    """Smoothly interpolated random lattice; the building block of `fbm`."""
    grid = rng.random((cells_y + 1, cells_x + 1))
    ys = np.linspace(0.0, cells_y, height, endpoint=False)
    xs = np.linspace(0.0, cells_x, width, endpoint=False)
    y0 = np.floor(ys).astype(int)
    x0 = np.floor(xs).astype(int)
    fy = ys - y0
    fx = xs - x0
    fy = fy * fy * (3.0 - 2.0 * fy)
    fx = fx * fx * (3.0 - 2.0 * fx)
    top = grid[y0][:, x0] * (1.0 - fx) + grid[y0][:, x0 + 1] * fx
    bottom = grid[y0 + 1][:, x0] * (1.0 - fx) + grid[y0 + 1][:, x0 + 1] * fx
    return top * (1.0 - fy)[:, None] + bottom * fy[:, None]


def fbm(rng, height, width, cells, octaves, gain=0.5, aspect=1.0):
    """Fractal sum of value noise in [0, 1]. `aspect` > 1 stretches the
    pattern along u, which is how fibres and striations are made."""
    total = np.zeros((height, width))
    amplitude = 1.0
    normaliser = 0.0
    for octave in range(octaves):
        cells_y = max(1, int(round(cells * 2 ** octave)))
        cells_x = max(1, int(round(cells * aspect * 2 ** octave)))
        total += amplitude * value_noise(rng, height, width, cells_y, cells_x)
        normaliser += amplitude
        amplitude *= gain
    return total / normaliser


def smoothstep(edge0, edge1, value):
    t = np.clip((value - edge0) / (edge1 - edge0), 0.0, 1.0)
    return t * t * (3.0 - 2.0 * t)


def colour(r, g, b):
    return np.array((r, g, b), dtype=np.float64)


def mix(a, b, t):
    """Lerp between two colours, or two images, by a scalar image."""
    t = np.asarray(t)[..., None]
    return a * (1.0 - t) + b * t


def region_canvas(size, name):
    """The pixel box of one atlas region and its local (U, V) grids, V up."""
    u0, v0, u1, v1 = ATLAS[name]
    x0, x1 = int(round(u0 * size)), int(round(u1 * size))
    y0, y1 = int(round(v0 * size)), int(round(v1 * size))
    width, height = x1 - x0, y1 - y0
    us = (np.arange(width) + 0.5) / width
    vs = (np.arange(height) + 0.5) / height
    U, V = np.meshgrid(us, vs)
    return (y0, y1, x0, x1), U, V


# --- regions --------------------------------------------------------------

def paint_membrane(rng, U, V):
    """The wing: leather, warmer and darker at the root, thinner and lighter
    toward the tip, with the actinofibrils fanning back from the root and a
    reinforced leading edge. v runs leading edge (0) to trailing edge (1)."""
    height, width = U.shape
    root = colour(0.40, 0.27, 0.19)
    tip = colour(0.58, 0.42, 0.30)
    base = mix(root, tip, U ** 0.8)

    # Actinofibrils run across the chord, from the leading edge back to the
    # trailing edge, fanning outward slightly toward the tip: rays from a
    # point well below the leading edge, so near the arm they are almost
    # chord-wise and at the tip they sweep back.
    angle = np.arctan2(V + 1.8, U - 0.3)
    fibres = 0.5 + 0.5 * np.cos(angle * 1080.0)
    fibres = fibres ** 14
    strength = 0.16 * (0.35 + 0.65 * U) * (0.4 + 0.6 * fbm(rng, height, width, 5, 3))
    base = base * (1.0 - (fibres * strength)[..., None])

    # Coarser veins, fewer and fainter, on the same fan.
    veins = (0.5 + 0.5 * np.cos(angle * 108.0 + 0.7)) ** 30
    base = base * (1.0 - (0.10 * veins)[..., None])

    # Mottling, and lighter translucent patches where the membrane is thin.
    mottle = fbm(rng, height, width, 4, 5) - 0.5
    base = base * (1.0 + 0.14 * mottle)[..., None]
    thin = smoothstep(0.58, 0.80, fbm(rng, height, width, 3, 4))
    base = mix(base, base * 1.18 + 0.03, 0.55 * thin * (0.3 + 0.7 * U))

    # The leading edge is muscle and bone under skin: darker and matte.
    edge = 1.0 - smoothstep(0.0, 0.09, V)
    base = mix(base, colour(0.30, 0.22, 0.17), 0.85 * edge)
    # The trailing edge thins to a slightly lighter, worn rim.
    rim = smoothstep(0.90, 1.0, V)
    base = mix(base, colour(0.63, 0.49, 0.36), 0.5 * rim)
    return base


def paint_body(rng, U, V):
    """Tail stub (u = 0) to the front of the skull (u = 1), belly (v = 0) to
    back (v = 1). Countershaded pycnofibre pelt, a lighter throat, and a
    darker head with a warm flush where the crest rises from it."""
    height, width = U.shape
    back = colour(0.33, 0.25, 0.20)
    belly = colour(0.76, 0.68, 0.56)
    boundary = 0.46 + 0.08 * (fbm(rng, height, width, 6, 3) - 0.5)
    base = mix(belly, back, smoothstep(boundary - 0.18, boundary + 0.18, V))

    # Fine pelt: streaks stretched along the body.
    pelt = fbm(rng, height, width, 6, 4, aspect=10.0) - 0.5
    base = base * (1.0 + 0.16 * pelt)[..., None]
    # Coarser patching under it.
    patch = fbm(rng, height, width, 3, 3) - 0.5
    base = base * (1.0 + 0.10 * patch)[..., None]

    # A slightly darker dorsal line.
    spine = smoothstep(0.90, 0.98, V)
    base = mix(base, colour(0.26, 0.19, 0.16), 0.5 * spine)

    # Throat: paler, warmer, on the underside of the neck.
    throat = smoothstep(0.70, 0.80, U) * (1.0 - smoothstep(0.93, 0.99, U)) * \
        (1.0 - smoothstep(0.25, 0.42, V))
    base = mix(base, colour(0.80, 0.67, 0.58), 0.7 * throat)

    # Head: darker overall, with a flush along the top where the crest grows.
    head = smoothstep(0.84, 0.92, U)
    base = mix(base, colour(0.29, 0.21, 0.18), 0.6 * head)
    flush = head * smoothstep(0.62, 0.85, V)
    base = mix(base, colour(0.56, 0.28, 0.18), 0.55 * flush)
    return base


def paint_crest(rng, U, V):
    """Side view of the crest, brow (u = 0) to tip (u = 1), lower edge (v = 0)
    to upper (v = 1). Dark horn at the base rising into a display red that
    deepens toward the tip, with faint growth banding along its length."""
    height, width = U.shape
    horn = colour(0.30, 0.21, 0.18)
    display = colour(0.62, 0.30, 0.18)
    deep = colour(0.42, 0.17, 0.12)
    base = mix(horn, display, smoothstep(0.08, 0.40, U))
    base = mix(base, deep, smoothstep(0.72, 1.0, U))

    bands = 0.5 + 0.5 * np.cos(U * 2.0 * np.pi * 9.0 + 1.3)
    base = base * (1.0 + 0.07 * (bands - 0.5))[..., None]
    mottle = fbm(rng, height, width, 5, 4) - 0.5
    base = base * (1.0 + 0.12 * mottle)[..., None]

    # Edges darken, as a keratin sheath does where it thins.
    upper = smoothstep(0.90, 1.0, V)
    lower = 1.0 - smoothstep(0.0, 0.10, V)
    base = mix(base, horn * 0.85, 0.6 * np.maximum(upper, lower))
    return base


def paint_beak(rng, U, V):
    """Root (u = 0) to tip (u = 1). The lower half of the region is the lower
    beak and the upper half the upper beak; inside each, v folds belly to
    top. Dark horn at the root wearing to pale keratin at the tip, with
    striations along the length and a dark line along the mouth."""
    height, width = U.shape
    root = colour(0.27, 0.23, 0.20)
    tip = colour(0.66, 0.58, 0.45)
    base = mix(root, tip, U ** 1.3)
    striae = fbm(rng, height, width, 6, 4, aspect=14.0) - 0.5
    base = base * (1.0 + 0.14 * striae)[..., None]

    lower_half = V < 0.5
    local = np.where(lower_half, V * 2.0, (V - 0.5) * 2.0)
    # The mouth line is the top of the lower beak and the bottom of the
    # upper beak; the darkening sits on each side of the seam.
    mouth = np.where(lower_half, smoothstep(0.86, 1.0, local),
                     1.0 - smoothstep(0.0, 0.14, local))
    base = mix(base, colour(0.16, 0.13, 0.12), 0.55 * mouth)
    # The lower beak is a shade darker than the upper.
    base = base * np.where(lower_half, 0.90, 1.0)[..., None]
    # A slight ridge highlight along the top of the upper beak.
    ridge = np.where(lower_half, 0.0, smoothstep(0.86, 1.0, local))
    base = mix(base, base * 1.12, 0.6 * ridge)
    return base


def paint_limbs(rng, U, V):
    """Legs and toes: dark scaled skin with a faint cellular relief."""
    height, width = U.shape
    base = np.broadcast_to(colour(0.34, 0.28, 0.24), (height, width, 3)).copy()
    scales = fbm(rng, height, width, 28, 2, gain=0.6)
    cells = np.abs(scales - 0.5) * 2.0
    base = base * (1.0 - 0.18 * cells)[..., None]
    patch = fbm(rng, height, width, 3, 3) - 0.5
    base = base * (1.0 + 0.12 * patch)[..., None]
    return base


PAINTERS = {
    "membrane": paint_membrane,
    "body": paint_body,
    "crest": paint_crest,
    "beak": paint_beak,
    "limbs": paint_limbs,
}


def paint(size):
    image = np.zeros((size, size, 3), dtype=np.float64)
    # Regions are painted in a fixed order, each with its own generator
    # stream, so editing one region never changes another's bytes.
    for index, name in enumerate(sorted(PAINTERS)):
        rng = np.random.default_rng(SEED + index)
        (y0, y1, x0, x1), U, V = region_canvas(size, name)
        image[y0:y1, x0:x1, :] = PAINTERS[name](rng, U, V)
    return np.clip(image, 0.0, 1.0)


def write_png(path, image):
    """Blender writes the file. The image is a byte buffer, so the values go
    in as they are - no view transform, no colour management - and come out
    as an 8-bit RGB PNG."""
    import bpy
    size = image.shape[0]
    handle = bpy.data.images.new("PteranodonAlbedo", size, size, alpha=False)
    rgba = np.ones((size, size, 4), dtype=np.float32)
    rgba[:, :, :3] = image
    handle.pixels.foreach_set(rgba.ravel())
    handle.filepath_raw = str(Path(path).resolve())
    handle.file_format = "PNG"
    handle.save()


def main():
    args = parse_args()
    image = paint(args.size)
    write_png(args.out, image)
    print("[albedo] wrote " + args.out)


if __name__ == "__main__":
    main()
