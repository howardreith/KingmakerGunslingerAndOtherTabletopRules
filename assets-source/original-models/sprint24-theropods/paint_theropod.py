"""Deterministic original theropod paint; no imported imagery."""
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
    "deinonychus": {
        "body": (92, 112, 89), "limbs": (72, 88, 70),
        "membrane": (139, 126, 83), "beak": (207, 190, 137),
        "crest": (168, 74, 54)},
    "tyrannosaurus": {
        "body": (105, 91, 68), "limbs": (77, 70, 57),
        "membrane": (126, 88, 61), "beak": (214, 196, 148),
        "crest": (159, 103, 57)},
}


def paint(kind, path, size=512):
    regions = atlas(Path(__file__).resolve().parent)
    pixels = bytearray(bytes((43, 45, 36, 255)) * (size * size))
    palette = PALETTES[kind]
    seed = 20261025 if kind == "deinonychus" else 20261026
    for name, (u0, v0, u1, v1) in regions.items():
        base = palette.get(name, (108, 99, 75))
        for y in range(int(v0 * size), int(v1 * size)):
            for x in range(int(u0 * size), int(u1 * size)):
                grain = ((x * 73856093 ^ y * 19349663 ^ seed) & 255) / 255.0 - .5
                scale = 1.0 + .11 * grain
                if name == "body":
                    # Broad deterministic bands remain legible at party camera.
                    band = (x // 19 + y // 31 + (x + y) // 47) % 5
                    scale += .10 if band == 0 else (-.035 if band == 3 else 0)
                if name == "crest" and (x + 3 * y) % 37 < 4:
                    scale *= .78
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
