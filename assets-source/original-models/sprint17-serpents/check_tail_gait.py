"""Offline check of the Salamander tail gait on the shipped mesh (VIS-029).

blender -b --factory-startup --python-exit-code 1 -P check_tail_gait.py -- <salamander-human-mesh.json>

Mirrors src/KingmakerGunslinger/Summoning/SalamanderTailGait.cs and skins the
tail with the runtime's convention (tail bone i bound at its rest point with
identity rotation; bone i placed at the gait point with the rotation that
carries rest segment i onto the gait segment). For each amplitude and
wavelength, over 16 phases, it reports the lowest tail vertex against rest
and the closest approach of tail vertices whose dominant bones are three or
more links apart, and fails if the shipped settings (wavelength 2.6, moving
amplitude 0.12) lower the tail by more than 1 mm or bring coils within 5 cm.
"""
import base64
import json
import math
import struct
import sys

import numpy as np

path = sys.argv[sys.argv.index("--") + 1]
payload = json.load(open(path, encoding="utf-8"))
blob = base64.b64decode(payload["data"])
n, t = payload["vertexCount"], payload["triangleCount"]
pos = np.frombuffer(blob, dtype="<f4", count=3 * n, offset=0).reshape(n, 3).astype(np.float64)
off = 12 * n + 12 * n + 8 * n + 12 * t
w = np.frombuffer(blob, dtype=np.dtype([("i", "<i4"), ("w", "<f4")]), count=4 * n, offset=off).reshape(n, 4)
bones = payload["bones"]
tail_index = {bones.index("KMG_SalamanderTail%02d" % k): k for k in range(10)}
rest = np.array(payload["originalTailRest"], dtype=np.float64)

seg = np.linalg.norm(np.diff(rest, axis=0), axis=1)
arc = np.concatenate([[0.0], np.cumsum(seg)])
length = arc[-1]


def smoothstep(e0, e1, x):
    u = np.clip((x - e0) / (e1 - e0), 0.0, 1.0)
    return u * u * (3 - 2 * u)


def normals():
    out = []
    for i in range(11):
        a = rest[max(i - 1, 0)]
        b = rest[min(i + 1, 10)]
        tx, tz = b[0] - a[0], b[2] - a[2]
        m = math.hypot(tx, tz)
        out.append((tz / m, 0.0, -tx / m))
    return np.array(out)


NRM = normals()


def gait_points(amplitude, wavelength, phase, ramp=0.3):
    env = smoothstep(0.0, ramp, arc / length)
    offs = amplitude * env * np.sin(2 * math.pi * arc / wavelength - phase)
    return rest + NRM * offs[:, None]


def from_to(a, b):
    a = a / np.linalg.norm(a)
    b = b / np.linalg.norm(b)
    v = np.cross(a, b)
    c = float(np.dot(a, b))
    if c < -0.999999:
        raise ValueError("opposite")
    k = np.array([[0, -v[2], v[1]], [v[2], 0, -v[0]], [-v[1], v[0], 0]])
    return np.eye(3) + k + k @ k * (1.0 / (1.0 + c))


def skin(points):
    mats = [from_to(rest[i + 1] - rest[i], points[i + 1] - points[i]) for i in range(10)]
    out = np.zeros_like(pos)
    for slot in range(4):
        idx = w["i"][:, slot]
        wt = w["w"][:, slot].astype(np.float64)
        moved = pos.copy()
        for bone, k in tail_index.items():
            sel = idx == bone
            if sel.any():
                moved[sel] = points[k] + (pos[sel] - rest[k]) @ mats[k].T
        out += moved * wt[:, None]
    return out


tail_weight = np.zeros(n)
dominant = np.full(n, -1)
best = np.zeros(n)
for slot in range(4):
    idx = w["i"][:, slot]
    wt = w["w"][:, slot]
    for bone, k in tail_index.items():
        sel = idx == bone
        tail_weight[sel] += wt[sel]
        better = sel & (wt > best)
        dominant[better] = k
        best[better] = wt[better]
tail = np.where(tail_weight > 0.5)[0]
print("vertices %d, tail-dominant %d, tail length %.3f" % (n, len(tail), length))


def closest_far_pair(p):
    q = p[tail]
    dom = dominant[tail]
    d = np.linalg.norm(q[:, None, :] - q[None, :, :], axis=2)
    far = np.abs(dom[:, None] - dom[None, :]) >= 3
    return d[far].min()


rest_skin = skin(rest.copy())
print("rest reproduces bind: max |dv| %.2e" % np.abs(rest_skin - pos).max())
rest_low = pos[tail][:, 1].min()
rest_gap = closest_far_pair(pos)
print("rest: lowest tail y %.4f, closest far pair %.4f" % (rest_low, rest_gap))
SHIPPED = (0.12, 2.6)
failures = []
for amplitude in (0.06, 0.09, 0.12, 0.16):
    for wavelength in (1.8, 2.6):
        lows, gaps = [], []
        for step in range(16):
            phase = 2 * math.pi * step / 16
            p = skin(gait_points(amplitude, wavelength, phase))
            lows.append(p[tail][:, 1].min())
            gaps.append(closest_far_pair(p))
        print("A=%.2f lambda=%.1f: lowest y %.4f (rest %.4f), closest far pair %.4f (rest %.4f)" % (
            amplitude, wavelength, min(lows), rest_low, min(gaps), rest_gap))
        if (amplitude, wavelength) == SHIPPED:
            if rest_low - min(lows) > 0.001:
                failures.append("shipped gait lowers the tail by %.4f" % (rest_low - min(lows)))
            if min(gaps) < 0.05:
                failures.append("shipped gait brings coils within %.4f" % min(gaps))
if failures:
    raise SystemExit("; ".join(failures))
print("shipped gait (amplitude %.2f, wavelength %.1f): PASS" % SHIPPED)
