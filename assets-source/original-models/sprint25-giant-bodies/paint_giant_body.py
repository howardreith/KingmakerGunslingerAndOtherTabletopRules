"""Deterministic original giant-body paint; no imported imagery.

Same category of work as the released Sprint 23 and Sprint 24 texture
authoring: Python standard library only, the shared UV atlas, deterministic
colour and grain arithmetic, and a hand-rolled PNG encoder. No image
generation, no Blender, and no pixel is copied from any other creature.

The two creatures do not share a grain algorithm, because they do not share a
skin. A sauropod gets large non-overlapping tubercle cells with dorsal
counter-shading; an elephant gets a two-frequency wrinkle network, dust
mottling and genuinely ivory tusks. Republishing dinosaur pixels as Elephant
art would be the one thing this file exists to avoid.

The atlas slot names are the shared family's five generic regions, not
anatomy claims. The per-creature mapping is explicit in REGION_ROLES and is
restated in SOURCE.md so `beak` is never mistaken for an actual beak.
"""
import argparse
import ast
from pathlib import Path
import struct
import zlib


def atlas(root):
    """The shared five-region atlas, read from the released pteranodon seam."""
    source = root.parent / "pteranodon" / "generate_pteranodon.py"
    tree = ast.parse(source.read_text(encoding="utf-8-sig"))
    for node in tree.body:
        if isinstance(node, ast.Assign) and any(
                isinstance(target, ast.Name) and target.id == "ATLAS"
                for target in node.targets):
            return ast.literal_eval(node.value)
    raise ValueError("shared atlas missing")


# What each shared atlas slot carries on each creature.
REGION_ROLES = {
    "brachiosaurus": {
        "membrane": "neck and dorsal expanse",
        "body": "torso flank",
        "crest": "tail",
        "beak": "small head",
        "limbs": "columnar legs",
    },
    "elephant": {
        "membrane": "hide expanse and ears",
        "body": "torso flank",
        "crest": "trunk",
        "beak": "tusks",
        "limbs": "legs",
    },
}

PALETTES = {
    # Muted sauropod olive-grey, darker dorsally, paler on the small head.
    "brachiosaurus": {
        "membrane": (96, 102, 84), "body": (104, 108, 88),
        "crest": (92, 97, 80), "beak": (118, 120, 98),
        "limbs": (84, 88, 74)},
    # Hairless grey hide with a warm dust cast, and ivory that reads as ivory.
    "elephant": {
        "membrane": (122, 118, 115), "body": (128, 123, 119),
        "crest": (118, 114, 110), "beak": (226, 219, 198),
        "limbs": (112, 108, 105)},
}

GROUNDS = {"brachiosaurus": (41, 45, 35, 255), "elephant": (52, 50, 48, 255)}
SEEDS = {"brachiosaurus": 20261027, "elephant": 20261028}


def _hash(x, y, seed):
    """Cheap deterministic integer hash in [0, 1)."""
    return ((x * 73856093) ^ (y * 19349663) ^ seed) & 255


def _sauropod_scale(x, y, region, size, seed):
    """Large tubercle cells plus gentle dorsal counter-shading."""
    scale = 1.0 + .09 * (_hash(x, y, seed) / 255.0 - .5)
    if region in ("membrane", "body", "crest"):
        # Coarse 17px lattice of non-overlapping tubercles: each cell takes one
        # deterministic tone, so the skin reads as pebbled rather than noisy
        # at party-camera distance.
        cell = _hash(x // 17, y // 17, seed) / 255.0
        scale += .13 * (cell - .5)
        # A darker centre dot per cell gives the tubercle a readable edge.
        if (x % 17 - 8) ** 2 + (y % 17 - 8) ** 2 <= 4:
            scale *= .90
        # Counter-shading: paler toward the belly, which is the low v side of
        # every region, so the silhouette keeps volume under flat lighting.
        scale += .07 * (1.0 - (y % max(1, size // 4)) / float(max(1, size // 4)))
    if region == "limbs":
        # Vertical columnar banding, no pebbling: these are weight-bearing
        # pillars, not flank hide.
        scale += .05 if (x // 9) % 2 == 0 else -.03
    if region == "beak":
        scale += .04 * (_hash(x // 5, y // 5, seed) / 255.0 - .5)
    return scale


def _elephant_scale(x, y, region, size, seed):
    """Two-frequency wrinkle network, dust mottling, and clean ivory."""
    scale = 1.0 + .06 * (_hash(x, y, seed) / 255.0 - .5)
    if region == "beak":
        # Tusks stay smooth and bright: no wrinkles, only a faint lengthwise
        # grain so they do not read as painted plastic.
        return scale + .03 * (1.0 - (x % 11) / 11.0)
    if region in ("membrane", "body", "crest", "limbs"):
        # Coarse creases and finer cross-creases. Elephant hide is a network of
        # deep folds, so the darkening is line-shaped rather than cell-shaped.
        coarse = (x + 2 * y) % 23
        fine = (3 * x - y) % 11
        if coarse < 2:
            scale *= .84
        elif coarse < 4:
            scale *= .93
        if fine < 1:
            scale *= .89
        # Dust sits in the folds and on the back.
        dust = _hash(x // 13, y // 13, seed + 7) / 255.0
        scale += .08 * (dust - .5)
    if region == "crest":
        # The trunk carries stronger transverse rings than the flank.
        if (y % 7) < 1:
            scale *= .86
    if region == "limbs":
        scale += .04 * (1.0 - (y % 19) / 19.0)
    return scale


GRAIN = {"brachiosaurus": _sauropod_scale, "elephant": _elephant_scale}


def paint(kind, path, size=512):
    regions = atlas(Path(__file__).resolve().parent)
    pixels = bytearray(bytes(GROUNDS[kind]) * (size * size))
    palette = PALETTES[kind]
    seed = SEEDS[kind]
    grain = GRAIN[kind]
    for name, (u0, v0, u1, v1) in regions.items():
        base = palette[name]
        for y in range(int(v0 * size), int(v1 * size)):
            for x in range(int(u0 * size), int(u1 * size)):
                scale = grain(x, y, name, size, seed)
                offset = ((size - 1 - y) * size + x) * 4
                rgb = tuple(max(0, min(255, round(value * scale)))
                            for value in base)
                pixels[offset:offset + 4] = bytes(rgb + (255,))
    Path(path).parent.mkdir(parents=True, exist_ok=True)

    def chunk(name, data):
        crc = zlib.crc32(name + data) & 0xffffffff
        return struct.pack(">I", len(data)) + name + data + struct.pack(">I", crc)

    scanlines = b"".join(
        b"\0" + pixels[row * size * 4:(row + 1) * size * 4]
        for row in range(size))
    png = (b"\x89PNG\r\n\x1a\n" +
           chunk(b"IHDR", struct.pack(">IIBBBBB", size, size, 8, 6, 0, 0, 0)) +
           chunk(b"IDAT", zlib.compress(scanlines, 9)) + chunk(b"IEND", b""))
    Path(path).write_bytes(png)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", choices=tuple(PALETTES), required=True)
    parser.add_argument("--out", required=True)
    args = parser.parse_args()
    paint(args.kind, args.out)
