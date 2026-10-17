#!/usr/bin/env python3
"""Targeted outward repair of a shipped schema-2 mesh (visual refinement).

python repair_outward_winding.py <mesh.json> [--marker <triangleWinding>] [--check]

Bodies written by the legacy shared writer (`pteranodon/generate_pteranodon
.write_mesh_data`, which emits (a, c, b)) ship inside out: every measured
donor frame is a proper rotation of Unity's, so their triangles face inward
while their authored normals face out. Mirrored parts can instead carry
inward normals. This repairs the shipped bytes directly, so a body can be
corrected without the private donor capture its generator needs.

1. Each triangle is wound to agree with its own three authored normals, the
   direction the body is lit from. (Walking shared edges for topological
   consistency instead was tried and rendered thin wing solids with wrong
   shading; a plain whole-mesh flip left mirrored parts, such as the Fire
   Beetle's eyes, dark.)
2. A closed component (every welded edge used at least twice) whose volume is
   then negative is a part whose normals point inward: its triangles are
   reversed and its normals negated.

Positions, UVs, weights, bones and every other JSON field stay byte for byte,
except `triangleWinding` when --marker names a new value. Nothing is written
unless every closed component is then outward and no component's normals
mostly oppose its triangles. --check reports whether a repair would change
anything (exit 1) without writing.
"""
import base64
import json
import re
import struct
import sys
from collections import defaultdict
from pathlib import Path


def sub(a, b):
    return (a[0] - b[0], a[1] - b[1], a[2] - b[2])


def cross(a, b):
    return (a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2],
            a[0] * b[1] - a[1] * b[0])


def dot(a, b):
    return a[0] * b[0] + a[1] * b[1] + a[2] * b[2]


def components(weld, tris):
    parent = {}

    def find(x):
        parent.setdefault(x, x)
        while parent[x] != x:
            parent[x] = parent[parent[x]]
            x = parent[x]
        return x
    for a, b, c in tris:
        for u, v in ((weld[a], weld[b]), (weld[b], weld[c])):
            ru, rv = find(u), find(v)
            if ru != rv:
                parent[ru] = rv
    groups = defaultdict(list)
    for index, (a, b, c) in enumerate(tris):
        groups[find(weld[a])].append(index)
    return list(groups.values())


def describe(faces, weld, pos, nrm, tris):
    uses = defaultdict(int)
    volume = 0.0
    agree = oppose = 0
    for f in faces:
        a, b, c = tris[f]
        wa, wb, wc = weld[a], weld[b], weld[c]
        for u, v in ((wa, wb), (wb, wc), (wc, wa)):
            uses[frozenset((u, v))] += 1
        volume += dot(pos[a], cross(pos[b], pos[c])) / 6.0
        face = cross(sub(pos[b], pos[a]), sub(pos[c], pos[a]))
        for vertex in (a, b, c):
            if dot(face, nrm[vertex]) >= 0:
                agree += 1
            else:
                oppose += 1
    return {"closed": all(n >= 2 for n in uses.values()), "volume": volume,
            "agree": agree, "oppose": oppose, "triangles": len(faces)}


def main():
    args = sys.argv[1:]
    path = Path(args[0])
    check = "--check" in args
    marker = args[args.index("--marker") + 1] if "--marker" in args else None
    text = path.read_text(encoding="utf-8")
    payload = json.loads(text)
    blob = bytearray(base64.b64decode(payload["data"]))
    n, t = payload["vertexCount"], payload["triangleCount"]
    tri_offset = 32 * n  # positions, normals, then texture coordinates
    pos = [struct.unpack_from("<3f", blob, 12 * i) for i in range(n)]
    nrm = [list(struct.unpack_from("<3f", blob, 12 * n + 12 * i)) for i in range(n)]
    tris = [list(struct.unpack_from("<3i", blob, tri_offset + 12 * i)) for i in range(t)]
    original = [list(row) for row in tris]
    ids = {}
    weld = [ids.setdefault(p, len(ids)) for p in pos]

    for index, (a, b, c) in enumerate(tris):
        face = cross(sub(pos[b], pos[a]), sub(pos[c], pos[a]))
        lit = [nrm[a][k] + nrm[b][k] + nrm[c][k] for k in range(3)]
        if dot(face, lit) < 0:
            tris[index] = [a, c, b]

    groups = components(weld, tris)
    negated = set()
    for faces in groups:
        row = describe(faces, weld, pos, nrm, tris)
        if row["closed"] and row["volume"] < 0:
            for f in faces:
                a, b, c = tris[f]
                tris[f] = [a, c, b]
            for v in {v for f in faces for v in tris[f]}:
                nrm[v] = [-c for c in nrm[v]]
                negated.add(v)

    rows = [describe(faces, weld, pos, nrm, tris) for faces in groups]
    bad = [r for r in rows if r["oppose"] > r["agree"] or
           (r["closed"] and r["volume"] <= 0)]
    opposing = sum(r["oppose"] for r in rows)
    total = opposing + sum(r["agree"] for r in rows)
    reversed_faces = sum(1 for a, b in zip(original, tris) if a != b)
    print("%s: %d components (%d closed); reversed %d of %d triangles, negated %d "
          "normals; opposing corners after %d/%d" % (path.name, len(rows),
          sum(1 for r in rows if r["closed"]), reversed_faces, t, len(negated),
          opposing, total))
    if bad:
        raise SystemExit("refusing: %d components still inward or lit from "
                         "behind: %s" % (len(bad), json.dumps(bad[:3])))
    changed = reversed_faces or negated or (
        marker is not None and payload.get("triangleWinding") != marker)
    if check:
        return 1 if changed else 0
    if not changed:
        return 0
    for i in range(t):
        struct.pack_into("<3i", blob, tri_offset + 12 * i, *tris[i])
    for i in range(n):
        struct.pack_into("<3f", blob, 12 * n + 12 * i, *nrm[i])
    old = payload["data"]
    if text.count(old) != 1:
        raise SystemExit("mesh-data blob not found exactly once in the JSON text")
    text = text.replace(old, base64.b64encode(bytes(blob)).decode("ascii"))
    if marker is not None:
        pattern = re.compile(r'("triangleWinding"\s*:\s*)"[^"]*"')
        if len(pattern.findall(text)) != 1:
            raise SystemExit("triangleWinding marker not found exactly once")
        text = pattern.sub(lambda m: m.group(1) + json.dumps(marker), text)
    path.write_text(text, encoding="utf-8", newline="\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
