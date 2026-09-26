#!/usr/bin/env python3
"""Turn the captured bind poses into the generator's rig format.

The earlier converter walked the captured LOCAL transforms up the hierarchy.
That was measuring the wrong thing: local transforms on a live summoned unit are
whatever the animation system has them at, and the resolved layout showed the
wings folded (1.638 across against 3.152 long). The bind poses disagree with it
by up to 3.954 units at the wingtip and describe a fully spread, exactly
mirrored pose (8.641 across against 2.937 long) - the frame the original mesh's
vertices were authored in, and therefore the frame a replacement must use.

bindposes[i] is already expressed in renderer space, so this needs no hierarchy
walk at all: invert it once at capture time, read the position and rotation off,
and the bone is placed. The head is that position; the tail points at the mean of
the bone's children, or along its own forward axis when it has none.

Input:  pteranodon-bind-rig.json  (from the run's evidence directory)
Output: rig.measured.json         (generator input)
"""
import argparse
import json
import math
from pathlib import Path

LEAF_LENGTH = 0.05


def quat_to_matrix(q):
    """Unity quaternion (x, y, z, w) -> 3x3 row-major rotation."""
    x, y, z, w = q
    n = math.sqrt(x * x + y * y + z * z + w * w) or 1.0
    x, y, z, w = x / n, y / n, z / n, w / n
    return [
        [1 - 2 * (y * y + z * z), 2 * (x * y - z * w), 2 * (x * z + y * w)],
        [2 * (x * y + z * w), 1 - 2 * (x * x + z * z), 2 * (y * z - x * w)],
        [2 * (x * z - y * w), 2 * (y * z + x * w), 1 - 2 * (x * x + y * y)],
    ]


def mat_apply(m, v):
    return [sum(m[i][k] * v[k] for k in range(3)) for i in range(3)]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--capture", required=True)
    parser.add_argument("--out", required=True)
    args = parser.parse_args()

    capture = json.loads(Path(args.capture).read_text(encoding="utf-8"))
    bones = {b["name"]: b for b in capture["bones"]}
    missing = [b["name"] for b in capture["bones"] if "bindPosition" not in b]
    if missing:
        raise SystemExit("capture predates the bind-pose fix: %s" % missing[:3])

    children = {}
    for name, bone in bones.items():
        parent = bone.get("parent")
        if parent in bones:
            children.setdefault(parent, []).append(name)

    depth = {}

    def depth_of(name):
        if name in depth:
            return depth[name]
        parent = bones[name].get("parent")
        depth[name] = (depth_of(parent) + 1) if parent in bones else 0
        return depth[name]

    out_bones = []
    for name, bone in bones.items():
        head = list(bone["bindPosition"])
        rot = quat_to_matrix(bone["bindRotation"])
        kids = children.get(name, [])
        if kids:
            tail = [sum(bones[k]["bindPosition"][i] for k in kids) / len(kids)
                    for i in range(3)]
        else:
            forward = mat_apply(rot, [0.0, 1.0, 0.0])
            tail = [head[i] + forward[i] * LEAF_LENGTH for i in range(3)]
        if all(abs(tail[i] - head[i]) < 1e-5 for i in range(3)):
            tail = [head[0], head[1] + 0.02, head[2]]
        out_bones.append({
            "name": name,
            "parent": bone.get("parent") if bone.get("parent") in bones else None,
            "index": bone["index"],
            "depth": depth_of(name),
            "head": head,
            "tail": tail,
        })

    out_bones.sort(key=lambda b: b["index"])
    result = {
        "source": "bind poses of the attached summoned Pteranodon "
                  "(SkinnedMeshRenderer.sharedMesh.bindposes inverted)",
        "space": "renderer-local; the frame the donor mesh was authored in",
        "rootBone": capture["rootBone"],
        "renderer": capture["renderer"],
        "bindPoseCount": capture["bindPoseCount"],
        "bones": out_bones,
    }
    Path(args.out).write_text(json.dumps(result, indent=1), encoding="utf-8")

    xs = [b["head"][0] for b in out_bones]
    ys = [b["head"][1] for b in out_bones]
    zs = [b["head"][2] for b in out_bones]
    print("wrote %s with %d bones" % (args.out, len(out_bones)))
    print("  extent X %.3f..%.3f  Y %.3f..%.3f  Z %.3f..%.3f"
          % (min(xs), max(xs), min(ys), max(ys), min(zs), max(zs)))
    print("  wingspan %.3f   body length %.3f   ratio %.2f"
          % (max(xs) - min(xs), max(zs) - min(zs),
             (max(xs) - min(xs)) / (max(zs) - min(zs))))

    # Left and right must mirror exactly, or the capture is not an authoring
    # pose and nothing downstream should trust it.
    worst, worst_bone = 0.0, None
    for bone in out_bones:
        if not bone["name"].startswith("L_"):
            continue
        twin = bones.get("R_" + bone["name"][2:])
        if twin is None:
            continue
        gap = max(abs(bone["head"][0] + twin["bindPosition"][0]),
                  abs(bone["head"][1] - twin["bindPosition"][1]),
                  abs(bone["head"][2] - twin["bindPosition"][2]))
        if gap > worst:
            worst, worst_bone = gap, bone["name"]
    print("  worst left/right mirror error %.5f (%s)" % (worst, worst_bone))


if __name__ == "__main__":
    main()
