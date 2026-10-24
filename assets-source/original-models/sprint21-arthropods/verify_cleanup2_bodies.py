"""Verify the shipped geometry/weight risks of the Cleanup 2 body repairs.

Run with Python 3, --root <repository> --out <private-report.json>.
This checks authored bytes only; it explicitly cannot certify native motion,
contact, material behavior or owner aesthetic acceptance.
"""
import argparse
import base64
from collections import defaultdict
import hashlib
import json
import math
from pathlib import Path
import struct
import sys

sys.dont_write_bytecode = True

FILES = {
    "giant-crab": "sprint21-arthropods",
    "giant-stag-beetle": "sprint14-insects",
    "girallon": "sprint19-fourarmed",
    "stegosaurus": "sprint23-armored-dinosaurs",
    "deinonychus": "sprint24-theropods",
}


def dot(a, b):
    return sum(x * y for x, y in zip(a, b))


def sub(a, b):
    return tuple(x - y for x, y in zip(a, b))


def cross(a, b):
    return (a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2],
            a[0] * b[1] - a[1] * b[0])


def read(path):
    payload = json.loads(path.read_text(encoding="utf-8"))
    blob = base64.b64decode(payload["data"])
    n, t = payload["vertexCount"], payload["triangleCount"]
    assert len(blob) == n * 64 + t * 12, "schema-2 buffer extent"
    positions = [struct.unpack_from("<3f", blob, 12 * i) for i in range(n)]
    normals = [struct.unpack_from("<3f", blob, n * 12 + 12 * i) for i in range(n)]
    triangles = [struct.unpack_from("<3i", blob, n * 32 + i * 12) for i in range(t)]
    start = n * 32 + t * 12
    rows = []
    for i in range(n):
        row = [struct.unpack_from("<if", blob, start + i * 32 + j * 8) for j in range(4)]
        assert all(0 <= index < len(payload["bones"]) and math.isfinite(value)
                   and value >= 0. for index, value in row), "valid driver weights"
        assert abs(sum(v for _, v in row) - 1.) < 2e-6, "normalized weights"
        rows.append({payload["bones"][index]: value for index, value in row if value > 1e-6})
    return payload, positions, normals, triangles, rows


def inspect(path):
    payload, points, normals, triangles, rows = read(path)
    assert all(all(math.isfinite(v) for v in p) for p in points), "finite positions"
    assert all(abs(dot(n, n) - 1.) < .002 for n in normals), "unit normals"
    canonical, weld, copies = {}, [], defaultdict(list)
    for i, p in enumerate(points):
        key = tuple(round(v, 5) for v in p)
        index = canonical.setdefault(key, len(canonical))
        weld.append(index)
        copies[index].append(i)
    parent = list(range(len(canonical)))
    def find(i):
        while parent[i] != i:
            parent[i] = parent[parent[i]]
            i = parent[i]
        return i
    for a, b, c in triangles:
        assert 0 <= min(a, b, c) and max(a, b, c) < len(points), "triangle indices"
        parent[find(weld[a])] = find(weld[b])
        parent[find(weld[b])] = find(weld[c])
    groups = defaultdict(list)
    for tri in triangles:
        groups[find(weld[tri[0]])].append(tri)
    components = []
    facing = 0
    for faces in groups.values():
        edges = defaultdict(int)
        volume = 0.
        vertices = set()
        for a, b, c in faces:
            vertices.update((a, b, c))
            for edge in ((weld[a], weld[b]), (weld[b], weld[c]), (weld[c], weld[a])):
                edges[edge] += 1
            volume += dot(points[a], cross(points[b], points[c])) / 6.
            normal = cross(sub(points[b], points[a]), sub(points[c], points[a]))
            averaged = tuple(sum(normals[i][axis] for i in (a, b, c)) for axis in range(3))
            facing += dot(normal, averaged) > 0.
        assert all(count == 1 and edges[(b, a)] == 1 for (a, b), count in edges.items()), \
            "closed, consistently outward topology"
        assert volume > 0., "positive signed volume"
        drivers = {name for i in vertices for name, value in rows[i].items() if value > .001}
        components.append(dict(triangles=len(faces), volume=volume, drivers=sorted(drivers)))
    assert facing / len(triangles) > .99, "normals agree with front faces"
    # Texture seams must not tear: every coincident copy follows precisely
    # the same blend of donor transforms, in every possible animation pose.
    for ids in copies.values():
        for i in ids[1:]:
            a, b = rows[ids[0]], rows[i]
            assert all(abs(a.get(n, 0.) - b.get(n, 0.)) < 1e-6 for n in set(a) | set(b)), \
                "UV seam copies keep equal skin weights"
    albedo = path.with_name(payload["albedo"]["file"])
    assert hashlib.sha256(albedo.read_bytes()).hexdigest() == payload["albedo"]["sha256"]
    return payload, components, dict(
        meshSha256=hashlib.sha256(path.read_bytes()).hexdigest(),
        albedoSha256=payload["albedo"]["sha256"], vertices=len(points),
        triangles=len(triangles), closedOutwardComponents=len(components),
        frontFacingNormalFraction=facing / len(triangles),
        extent={axis: [min(p[i] for p in points), max(p[i] for p in points)]
                for i, axis in enumerate("xyz")})


def anatomy(key, payload, components):
    sets = [set(c["drivers"]) for c in components]
    bones = set(payload["bones"])
    if key in ("giant-crab", "giant-stag-beetle"):
        count = 4 if key == "giant-crab" else 3
        for side in ("L", "R"):
            for i in range(count):
                limb = {f"{side}_Leg{i}_Upper", f"{side}_Leg{i}_Lower", f"{side}_Foot{i}"}
                assert sum(limb <= drivers for drivers in sets) == 1, "one connected leg per chain"
        if key == "giant-stag-beetle":
            assert not any("Leg3" in n or "Foot3" in n for n in bones), "empty fourth leg pair"
            assert all(sum({f"chelicera_{s}"} == d for d in sets) == 1 for s in ("L", "R"))
        else:
            assert not ({"Tail3_M", "chelicera_L", "chelicera_R"} & bones), "no spider abdomen/jaws"
            for side in ("L", "R"):
                chela = {f"pedipalp{i}_{side}" for i in range(1, 8)}
                assert sum(chela <= d for d in sets) == 1, "integrated entire claw chain"
    elif key == "girallon":
        for side in ("L", "R"):
            needed = {side + "_" + n for n in ("Clavicle_01", "Forearm_01", "Hand_01",
                      "Fore_Finger_03", "Midle_Finger_03", "Little_Finger_03", "Thumb_03")}
            assert sum(needed <= d for d in sets) == 1, "upper arm joins elbow, wrist, palm and all digit chains"
            lower = {side + "_" + n for n in ("Clavicle_01", "Up_Arm_01", "Forearm_01", "Forearm_02")}
            assert sum(lower <= d for d in sets) == 2, "two connected arm/hand surfaces per side"
        assert payload.get("lowerHandsUseForearmDriver"), "explicit simplified lower-hand policy"
    elif key == "deinonychus":
        # The cleanup follow-up joined both forelimbs to the trunk surface.
        for side in ("L", "R"):
            needed = {"Torso_Lower", "Torso_Upper", "Head"} | {side + n for n in (
                "_Arm_Upper", "_Arm_Lower", "_Palm", "_finger2", "_Bfinger2")}
            assert sum(needed <= d for d in sets) == 1, "arm, palm and every finger chain join the trunk"
    else:
        needed = {f"Tail{i}_M" for i in range(6)} | {"Head", "Neck_M"} | \
            {s + n for s in ("Left", "Right") for n in
             ("UpperArm", "LowerArm", "Hand", "UpperLeg0", "LowerLeg0", "Foot0")}
        assert sum(needed <= d for d in sets) == 1, "continuous dinosaur trunk/neck/tail/four legs"


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", required=True)
    parser.add_argument("--out", required=True)
    args = parser.parse_args()
    result = {"scope": "authored geometry and skin weights only", "nativeVisualVerified": False,
              "ownerAestheticAcceptance": "pending", "creatures": {}}
    for key, directory in FILES.items():
        path = Path(args.root) / "assets" / directory / (key + "-mesh.json")
        payload, components, report = inspect(path)
        anatomy(key, payload, components)
        result["creatures"][key] = dict(status="PASS", **report)
    Path(args.out).write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    main()
