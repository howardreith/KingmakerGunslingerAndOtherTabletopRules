"""Deterministic original armored-dinosaur paint; no imported imagery."""
import argparse
import ast
from pathlib import Path
import struct
import zlib


def atlas(root):
    source = root.parent / "pteranodon" / "generate_pteranodon.py"
    tree = ast.parse(source.read_text(encoding="utf-8-sig"))
    for node in tree.body:
        if isinstance(node, ast.Assign) and any(
                isinstance(target, ast.Name) and target.id == "ATLAS"
                for target in node.targets):
            return ast.literal_eval(node.value)
    raise ValueError("shared atlas missing")


PALETTES = {
    "triceratops": {
        "body": (101, 112, 73), "limbs": (82, 91, 61),
        "membrane": (127, 102, 69), "beak": (188, 173, 126),
        "crest": (218, 205, 158)},
    "stegosaurus": {
        "body": (91, 107, 78), "limbs": (70, 82, 60),
        "membrane": (151, 91, 58), "beak": (93, 77, 53),
        "crest": (177, 113, 68)},
}


def paint(kind, path, size=512):
    regions = atlas(Path(__file__).resolve().parent)
    pixels = bytearray(bytes((48, 52, 39, 255)) * (size * size))
    palette = PALETTES[kind]
    seed = 20261023 if kind == "triceratops" else 20261024
    for name, (u0, v0, u1, v1) in regions.items():
        base = palette.get(name, (110, 103, 75))
        for y in range(int(v0 * size), int(v1 * size)):
            for x in range(int(u0 * size), int(u1 * size)):
                grain = ((x * 73856093 ^ y * 19349663 ^ seed) & 255) / 255.0 - .5
                scale = 1.0 + .10 * grain
                if name == "body":
                    # Deterministic broad mottling, legible at party camera.
                    scale += .07 if ((x // 23 + y // 19) % 3 == 0) else -.02
                if name in ("crest", "membrane") and (x + 2 * y) % 31 < 3:
                    scale *= .82
                offset = ((size - 1 - y) * size + x) * 4
                rgb = tuple(max(0, min(255, round(value * scale))) for value in base)
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
