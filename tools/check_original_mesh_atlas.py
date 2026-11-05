"""Check that each schema-2 triangle samples one convex atlas rectangle.

Per-corner validity is insufficient: interpolation between two tiles can cross
a third tile. Containment of all three corners in the SAME rectangle proves
containment of every barycentric interior sample, including degenerate UVs.
"""
import base64
import hashlib
import json
import math
from pathlib import Path
import struct
import sys
from collections import defaultdict


def check(path):
    raw = Path(path).read_bytes()
    payload = json.loads(raw)
    blob = base64.b64decode(payload['data'], validate=True)
    n, t = payload['vertexCount'], payload['triangleCount']
    uv = [struct.unpack_from('<2f', blob, 24*n + 8*i) for i in range(n)]
    if any(not math.isfinite(c) or not 0 <= c <= 1 for pair in uv for c in pair):
        raise ValueError('nonfinite or out-of-range texture coordinate')
    rectangles = list(payload['uvAtlas'].values())
    crossing = []
    for i in range(t):
        indices = struct.unpack_from('<3i', blob, 32*n + 12*i)
        if min(indices) < 0 or max(indices) >= n:
            raise ValueError('invalid triangle index')
        corners = [uv[index] for index in indices]
        if not any(all(r[0]-1e-7 <= u <= r[2]+1e-7 and
                       r[1]-1e-7 <= v <= r[3]+1e-7 for u,v in corners)
                   for r in rectangles):
            crossing.append(i)
    # UV-only splits must keep deformation and smooth shading continuous.
    # Compare only duplicate positions assigned to different material tiles.
    # Hard features within a single tile may intentionally split normals.
    groups = defaultdict(list)
    for i,pair in enumerate(uv):
        region = next((k for k,r in payload['uvAtlas'].items()
                       if r[0] < pair[0] < r[2] and r[1] < pair[1] < r[3]), None)
        pos = struct.unpack_from('<3f',blob,12*i)
        normal = struct.unpack_from('<3f',blob,12*n+12*i)
        weights = struct.unpack_from('<ifififif',blob,32*n+12*t+32*i)
        groups[pos].append((region,normal,weights))
    seams = [g for g in groups.values() if len({v[0] for v in g}) > 1]
    normal_mismatches = sum(len({v[1] for v in g}) > 1 for g in seams)
    weight_mismatches = sum(len({v[2] for v in g}) > 1 for g in seams)
    return {'file': str(path), 'sha256': hashlib.sha256(raw).hexdigest(),
            'triangles': t, 'trianglesSpanningAtlasRegions': len(crossing),
            'firstCrossingTriangles': crossing[:16], 'materialSeamPositions': len(seams),
            'seamNormalMismatches': normal_mismatches,
            'seamWeightMismatches': weight_mismatches}


if __name__ == '__main__':
    results = [check(path) for path in sys.argv[1:]]
    if not results:
        raise SystemExit('Pass one or more exported mesh JSON files.')
    print(json.dumps(results, indent=2))
    raise SystemExit(1 if any(r['trianglesSpanningAtlasRegions'] or
                             r['seamNormalMismatches'] or r['seamWeightMismatches']
                             for r in results) else 0)
