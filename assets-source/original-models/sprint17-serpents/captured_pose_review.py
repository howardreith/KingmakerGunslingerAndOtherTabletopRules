#!/usr/bin/env python3
"""Private replay of ORIGINAL vertices through measured current skin matrices.

This reads no native mesh/texture/clip. A replay is an offline diagnostic of
the captured poses, never a new in-game qualification or an animation export.
"""
import argparse
import base64
import hashlib
import json
import math
from pathlib import Path
import struct

KEYS = ("viper", "constrictor-snake", "salamander")


def capture_rows(path):
    rows = json.loads(Path(path).read_text(encoding="utf-8"))
    if not isinstance(rows, list) or [r.get("key") for r in rows] != list(KEYS):
        raise ValueError("not the closed three-body capture")
    for row in rows:
        if row.get("sourceCreature") != ("salamander" if row["key"] == "salamander" else "purple-worm"):
            raise ValueError("wrong original-body source")
        if not row.get("scope", "").startswith("original body binding/movement/lifetime only"):
            raise ValueError("not original-body research")
        samples = [row["idleSample"]] + row["movementSamples"]
        if not 4 <= len(samples) <= 81:
            raise ValueError("incomplete or unbounded pose capture")
        for sample in samples:
            transforms = sample["skinTransforms"]
            names = [t["name"] for t in transforms]
            if len(names) != (27 if row["key"] == "salamander" else 16) or len(set(names)) != len(names):
                raise ValueError("incomplete or ambiguous current frame")
            for transform in transforms:
                matrix = transform["skinToWorldRowMajor"]
                if len(matrix) != 16 or not all(math.isfinite(v) for v in matrix):
                    raise ValueError("invalid current matrix")
            for field in ("actorFloor", "lowestVertexFloor"):
                floor = sample[field]
                if (not floor["rayHit"] or floor["ownedCollider"] or
                        floor["layerMask"] != "0x200101" or floor["clearance"] is None or
                        abs(floor["normal"][1] - 1) > 1e-6):
                    raise ValueError("replay needs the measured flat floor, not an actor/nav origin")
    return rows


def decode_mesh(path):
    payload = json.loads(Path(path).read_text(encoding="utf-8"))
    count, triangles = payload["vertexCount"], payload["triangleCount"]
    names = payload["bones"]
    raw = base64.b64decode(payload["data"], validate=True)
    if payload["schemaVersion"] != 2 or len(raw) != count * 64 + triangles * 12:
        raise ValueError("not the complete original mesh payload")
    vertices = [struct.unpack_from("<3f", raw, index * 12) for index in range(count)]
    uv = [struct.unpack_from("<2f", raw, count * 24 + index * 8) for index in range(count)]
    faces = [struct.unpack_from("<3i", raw, count * 32 + index * 12) for index in range(triangles)]
    offset = count * 32 + triangles * 12
    weights = []
    for index in range(count):
        slots = [struct.unpack_from("<if", raw, offset + index * 32 + slot * 8) for slot in range(4)]
        if (any(bone < 0 or bone >= len(names) or not math.isfinite(weight) or weight < 0 for bone, weight in slots)
                or abs(sum(w for _, w in slots) - 1) > 1e-6):
            raise ValueError("invalid original influence row")
        weights.append([(names[bone], weight) for bone, weight in slots if weight > 0])
    if not all(math.isfinite(v) for point in vertices for v in point):
        raise ValueError("nonfinite original vertex")
    return dict(vertices=vertices, weights=weights, uv=uv, faces=faces, payload=payload)


def transform_point(matrix, point):
    return tuple(sum(matrix[row * 4 + col] * point[col] for col in range(3)) + matrix[row * 4 + 3]
                 for row in range(3))


def replay(vertices, weights, sample):
    frames = {row["name"]: row["skinToWorldRowMajor"] for row in sample["skinTransforms"]}
    if len(vertices) != len(weights):
        raise ValueError("unweighted original vertex")
    result = []
    for vertex, influences in zip(vertices, weights):
        pieces = [(transform_point(frames[name], vertex), weight) for name, weight in influences]
        result.append(tuple(sum(point[axis] * weight for point, weight in pieces) for axis in range(3)))
    return result


def dot(a, b):
    return sum(x * y for x, y in zip(a, b))


def normalized(value):
    length = math.sqrt(dot(value, value))
    if not math.isfinite(length) or length < 1e-8:
        raise ValueError("invalid support direction")
    return tuple(v / length for v in value)


def snake_support_frame(rows):
    """Author a coil plane against the measured root, without editing the rig.

    All observed snake frames must agree. This is not a claim about unsampled
    attack/death poses, and no captured transform is emitted into the package.
    """
    normals = []
    for row in rows[:2]:
        for sample in [row["idleSample"]] + row["movementSamples"]:
            root = next(t for t in sample["skinTransforms"] if t["name"] == "Hips_Joints")
            matrix = root["skinToWorldRowMajor"]
            if abs(matrix[7] - sample["actorFloor"]["hitPoint"][1]) > 1e-4:
                raise ValueError("snake root has an unaccounted ground offset")
            normals.append(normalized(matrix[4:7]))
    up = normals[0]
    if up[1] < .8 or any(dot(up, other) < 1 - 1e-8 for other in normals):
        raise ValueError("native root is not stable enough for an authored support plane")
    side = normalized(tuple(v - up[i] * up[0] for i, v in enumerate((1, 0, 0))))
    forward = (side[1] * up[2] - side[2] * up[1],
               side[2] * up[0] - side[0] * up[2], side[0] * up[1] - side[1] * up[0])
    return side, up, forward


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--capture", required=True)
    parser.add_argument("--mesh-directory", required=True)
    parser.add_argument("--report", required=True)
    args = parser.parse_args()
    rows = capture_rows(args.capture)
    report = dict(scope="offline original-geometry replay, not new runtime qualification",
                  captureSha256=hashlib.sha256(Path(args.capture).read_bytes()).hexdigest(), bodies=[])
    for row in rows:
        path = Path(args.mesh_directory) / (row["key"] + "-mesh.json")
        mesh = decode_mesh(path)
        samples = []
        for sample in [row["idleSample"]] + row["movementSamples"]:
            points = replay(mesh["vertices"], mesh["weights"], sample)
            floor = sample["lowestVertexFloor"]["hitPoint"][1]
            samples.append(dict(frame=sample["frame"], minimumClearance=min(p[1] for p in points) - floor,
                                capturedMinimumClearance=sample["lowestVertexFloor"]["clearance"],
                                belowFloorVertices=sum(p[1] < floor - 1e-4 for p in points)))
        report["bodies"].append(dict(key=row["key"], meshSha256=hashlib.sha256(path.read_bytes()).hexdigest(),
                                     samples=samples))
    Path(args.report).parent.mkdir(parents=True, exist_ok=True)
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    for row in report["bodies"]:
        print(row["key"], "min/max replay clearance", min(s["minimumClearance"] for s in row["samples"]),
              max(s["minimumClearance"] for s in row["samples"]), "max vertices below measured plane",
              max(s["belowFloorVertices"] for s in row["samples"]))


if __name__ == "__main__":
    main()
