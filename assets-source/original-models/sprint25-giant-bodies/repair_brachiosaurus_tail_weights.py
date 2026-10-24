"""Original project tail weighting, v4 root-pinned proposal.

This does not read native geometry, bind data or clips. Geometry, UV, normals,
triangles and every non-tail weight row remain byte-identical. Runtime pivots
are authored renderer-space positions; the live driver derives its remap from
the actual installed bind matrices, never from an old native rig capture.
"""
import argparse
import base64
import hashlib
import json
from pathlib import Path
import struct


def repair(path):
    path = Path(path)
    doc = json.loads(path.read_text())
    raw = bytearray(base64.b64decode(doc["data"]))
    n, triangles = doc["vertexCount"], doc["triangleCount"]
    weight_start = n * 32 + triangles * 12
    assert len(raw) == weight_start + n * 32, "Unsupported project payload"
    names = ["Tail%d_M" % i for i in range(5)]
    palette = [doc["bones"].index(name) for name in names]
    rows = [struct.unpack_from("<ifififif", raw, weight_start + i * 32) for i in range(n)]
    tail = {i for i, row in enumerate(rows) if any(row[j] in palette and row[j+1] > 0 for j in range(0, 8, 2))}
    assert len(tail) == 605, "Unexpected original tail geometry"
    for i in tail:
        assert all(rows[i][j] in palette or rows[i][j+1] == 0 for j in range(0, 8, 2))
    boundary, boundary_triangles = set(), 0
    for i in range(triangles):
        tri = struct.unpack_from("<iii", raw, n * 32 + i * 12)
        owned = [v for v in tri if v in tail]
        if 0 < len(owned) < 3:
            boundary.update(owned)
            boundary_triangles += 1
    assert len(boundary) == 40 and boundary_triangles == 70, "Unexpected original pelvis junction"
    zs = [struct.unpack_from("<f", raw, i * 12 + 8)[0] for i in range(n)]
    boundary_z = max(zs[i] for i in boundary)
    centers = [max(3.15, boundary_z + .30), 4.25, 5.25, 5.8]
    previous = bytes(raw)
    changed = 0
    for i in sorted(tail):
        u = max(0., min(1., (zs[i] - boundary_z) / .70))
        blend = 0. if i in boundary else u * u * (3. - 2. * u)
        weights = [1. - blend, 0., 0., 0., 0.]
        if zs[i] <= centers[0]:
            weights[1] = blend
        elif zs[i] >= centers[-1]:
            weights[4] = blend
        else:
            segment = next(j for j in range(3) if centers[j] <= zs[i] <= centers[j+1])
            t = (zs[i] - centers[segment]) / (centers[segment+1] - centers[segment])
            weights[segment+1] = blend * (1. - t)
            weights[segment+2] = blend * t
        active = [(palette[j], w) for j, w in enumerate(weights) if w > 1e-8]
        assert 0 < len(active) <= 4
        total = sum(w for _, w in active)
        active = [(bone, w / total) for bone, w in active]
        active += [(0, 0.)] * (4 - len(active))
        packed = struct.pack("<ifififif", *(item for pair in active for item in pair))
        start = weight_start + i * 32
        if bytes(raw[start:start+32]) != packed:
            changed += 1
        raw[start:start+32] = packed
    assert raw[:weight_start] == previous[:weight_start]
    assert all(raw[weight_start+i*32:weight_start+(i+1)*32] == previous[weight_start+i*32:weight_start+(i+1)*32]
               for i in range(n) if i not in tail)
    metadata = {"schema": 1, "proposal": "cleanup2-root-pinned-tail-v4",
        "pivotsRendererLocal": [[0., 3.25, 2.9], [0., 2.9, 3.8], [0., 2.62, 4.8], [0., 2.45, 5.75]],
        "bones": names[1:], "tail0Unchanged": True, "tailVertices": len(tail),
        "pelvisJunctionTriangles": boundary_triangles, "pinnedJunctionVertices": len(boundary),
        "rootTransitionLength": .70, "weightCenters": centers,
        "geometryAndNonTailWeightsUnchanged": True}
    doc["attackGestureTailRig"] = metadata
    doc["data"] = base64.b64encode(raw).decode("ascii")
    path.write_text(json.dumps(doc, indent=1) + "\n", encoding="utf-8", newline="\n")
    return {"asset": path.name, "changedTailWeightRows": changed, "geometrySha256": hashlib.sha256(raw[:weight_start]).hexdigest(),
            "sha256": hashlib.sha256(path.read_bytes()).hexdigest(), "rig": metadata}


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("asset")
    print(json.dumps(repair(parser.parse_args().asset), indent=2))
