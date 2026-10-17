"""Orientation audit of shipped schema-2 creature mesh data.

Decodes the exact payload bytes and, for every connected component, reports
closure (boundary edges), relative winding consistency (shared edges walked in
the same direction), signed volume from the SHIPPED triangle order, and how
the SHIPPED vertex normals agree with that triangle order.

Convention used for interpretation: for identical coordinates, both Blender
and Unity treat Cross(b - a, c - a) as the front side of triangle (a, b, c).
A positive signed volume therefore means the shipped triangles face outward
when rendered with back-face culling, provided the loader neither reverses
indices nor mirrors an axis (verify against the runtime loader separately).
"""
import base64
import json
import struct
import sys
from collections import defaultdict


def load(path):
    payload = json.loads(open(path, encoding="utf-8").read())
    blob = base64.b64decode(payload["data"])
    n, t = payload["vertexCount"], payload["triangleCount"]
    off = 0
    pos = [struct.unpack_from("<3f", blob, off + 12 * i) for i in range(n)]
    off += 12 * n
    nrm = [struct.unpack_from("<3f", blob, off + 12 * i) for i in range(n)]
    off += 12 * n
    off += 8 * n
    tri = [struct.unpack_from("<3i", blob, off + 12 * i) for i in range(t)]
    off += 12 * t
    w = []
    for i in range(n):
        row = struct.unpack_from("<ifififif", blob, off + 32 * i)
        w.append(tuple((row[k], row[k + 1]) for k in range(0, 8, 2)))
    off += 32 * n
    assert off == len(blob), (off, len(blob))
    return payload, pos, nrm, tri, w


def sub(a, b):
    return (a[0] - b[0], a[1] - b[1], a[2] - b[2])


def cross(a, b):
    return (a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2],
            a[0] * b[1] - a[1] * b[0])


def dot(a, b):
    return a[0] * b[0] + a[1] * b[1] + a[2] * b[2]


def weld_map(pos):
    ids = {}
    return [ids.setdefault(p, len(ids)) for p in pos]


def components(n, tri):
    parent = list(range(n))

    def find(x):
        while parent[x] != x:
            parent[x] = parent[parent[x]]
            x = parent[x]
        return x
    for a, b, c in tri:
        for u, v in ((a, b), (b, c)):
            ru, rv = find(u), find(v)
            if ru != rv:
                parent[ru] = rv
    groups = defaultdict(list)
    for index, (a, b, c) in enumerate(tri):
        groups[find(a)].append(index)
    return list(groups.values())


def audit(path, bones_of_interest=None):
    payload, pos, nrm, tri, w = load(path)
    # Texture seams duplicate vertices at identical positions; weld them so
    # closure and winding are judged on the surface, not the UV islands.
    weld = weld_map(pos)
    tri = [(weld[a], weld[b], weld[c]) for a, b, c in tri]
    canonical = {}
    for index, key in enumerate(weld):
        canonical.setdefault(key, index)
    pos = [pos[canonical[k]] for k in range(len(canonical))]
    nrm = [nrm[canonical[k]] for k in range(len(canonical))]
    w = [w[canonical[k]] for k in range(len(canonical))]
    bones = payload["bones"]
    rows = []
    for faces in components(len(pos), tri):
        directed = defaultdict(int)
        volume = 0.0
        agree = disagree = 0
        verts = set()
        for f in faces:
            a, b, c = tri[f]
            verts.update((a, b, c))
            for u, v in ((a, b), (b, c), (c, a)):
                directed[(u, v)] += 1
            pa, pb, pc = pos[a], pos[b], pos[c]
            volume += dot(pa, cross(pb, pc)) / 6.0
            fn = cross(sub(pb, pa), sub(pc, pa))
            for vi in (a, b, c):
                if dot(fn, nrm[vi]) >= 0:
                    agree += 1
                else:
                    disagree += 1
        undirected = defaultdict(list)
        for (u, v), count in directed.items():
            undirected[frozenset((u, v))].append(((u, v), count))
        boundary = same_direction = 0
        for key, entries in undirected.items():
            total = sum(count for _, count in entries)
            if total == 1:
                boundary += 1
            # Same directed edge used twice by two faces = inconsistent.
            for _, count in entries:
                if count > 1:
                    same_direction += 1
        cpts = [pos[v] for v in verts]
        cc = tuple(sum(p[k] for p in cpts) / len(cpts) for k in range(3))
        out_area = in_area = 0.0
        for f in faces:
            a, b, c = tri[f]
            pa, pb, pc = pos[a], pos[b], pos[c]
            fn = cross(sub(pb, pa), sub(pc, pa))
            area = dot(fn, fn) ** 0.5 / 2.0
            fc = tuple((pa[k] + pb[k] + pc[k]) / 3.0 for k in range(3))
            if dot(fn, sub(fc, cc)) >= 0:
                out_area += area
            else:
                in_area += area
        dominant = defaultdict(float)
        for vi in verts:
            for bone_index, weight in w[vi]:
                if weight > 0:
                    dominant[bones[bone_index]] += weight
        owner = max(dominant, key=dominant.get)
        ys = [pos[v] for v in verts]
        centre = tuple(sum(p[k] for p in ys) / len(ys) for k in range(3))
        rows.append({
            "triangles": len(faces), "vertices": len(verts),
            "owner": owner, "centre": [round(x, 3) for x in centre],
            "boundaryEdges": boundary, "sameDirectionEdges": same_direction,
            "signedVolume": volume,
            "outwardAreaFraction": out_area / ((out_area + in_area) or 1.0),
            "normalAgreesWithWinding": agree, "normalOpposesWinding": disagree,
        })
    rows.sort(key=lambda r: -r["triangles"])
    return payload, rows


def main():
    for path in sys.argv[1:]:
        payload, rows = audit(path)
        print("==", path)
        print("   vertices %d triangles %d bones %d space=%r" % (
            payload["vertexCount"], payload["triangleCount"],
            len(payload["bones"]), payload.get("space")))
        outward = sum(1 for r in rows if r["signedVolume"] > 0)
        inward = sum(1 for r in rows if r["signedVolume"] < 0)
        closed = sum(1 for r in rows if r["boundaryEdges"] == 0)
        inconsistent = sum(1 for r in rows if r["sameDirectionEdges"])
        print("   components %d: signedVolume>0 %d, <0 %d; closed %d; "
              "with same-direction shared edges %d" % (
                  len(rows), outward, inward, closed, inconsistent))
        for r in rows:
            print("   %5d tri  %-18s outwardArea=%4.0f%%  vol=%+.4g  "
                  "boundary=%d  sameDir=%d  normal agree/oppose=%d/%d  c=%s" % (
                      r["triangles"], r["owner"],
                      100.0 * r["outwardAreaFraction"], r["signedVolume"],
                      r["boundaryEdges"], r["sameDirectionEdges"],
                      r["normalAgreesWithWinding"], r["normalOpposesWinding"],
                      r["centre"]))


if __name__ == "__main__":
    main()
