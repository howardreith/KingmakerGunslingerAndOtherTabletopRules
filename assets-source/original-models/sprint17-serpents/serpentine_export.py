#!/usr/bin/env python3
"""Sprint 17's measured outward face convention, not a shared rig rewrite."""
import base64
import struct

KEYS = ("viper", "constrictor-snake", "salamander")
WINDING = "authored-outward-sprint17"


def finish_original_winding(payload, creature):
    """Undo only the shared exporter's triangle flip on these original bodies.

    Exact32b native A/B keeps vertices, normals, materials and binding fixed:
    the reversed order produces solid outer surfaces on all three S17 bodies.
    Qualified families keep their existing exporter/convention unchanged.
    """
    if creature not in KEYS or payload.get("schemaVersion") != 2:
        raise ValueError("not a closed Sprint 17 original export")
    if "triangleWinding" in payload:
        raise ValueError("original export winding was already finalized")
    count, triangles = payload.get("vertexCount"), payload.get("triangleCount")
    if type(count) is not int or type(triangles) is not int or count < 3 or triangles < 1:
        raise ValueError("incomplete original geometry counts")
    raw = base64.b64decode(payload["data"], validate=True)
    if len(raw) != count * 64 + triangles * 12:
        raise ValueError("incomplete original geometry bytes")
    corrected = bytearray(raw)
    for index in range(triangles):
        offset = count * 32 + index * 12
        a, b, c = struct.unpack_from("<3i", raw, offset)
        if len({a, b, c}) != 3 or min(a, b, c) < 0 or max(a, b, c) >= count:
            raise ValueError("invalid original triangle indices")
        struct.pack_into("<3i", corrected, offset, a, c, b)
    result = dict(payload)
    result["data"] = base64.b64encode(corrected).decode("ascii")
    result["triangleWinding"] = WINDING
    return result
