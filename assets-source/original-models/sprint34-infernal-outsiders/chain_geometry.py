"""Geometry-only link frame; independent of Blender and the combat runtime."""
import math


def perpendicular_axes(major, requested_minor):
    """Preserve a link's intended plane unless it degenerates at a drape.

    The major axis is the local chain tangent. Project the requested minor
    axis into its perpendicular plane. Near parallel inputs use the least
    aligned cardinal axis, so a vertical chain has the same full opening as
    a horizontal one. This never changes the chain path or attack ownership.
    """
    def dot(a, b):
        return sum(x*y for x,y in zip(a,b))

    def unit(v):
        length = math.sqrt(dot(v,v))
        if not math.isfinite(length) or length < 1e-9:
            raise ValueError('nonfinite or zero link tangent')
        return tuple(x/length for x in v)

    a = unit(major)
    if len(requested_minor) != 3 or not all(math.isfinite(v) for v in requested_minor):
        raise ValueError('invalid link minor axis')
    projected = tuple(v-a[i]*dot(a,requested_minor) for i,v in enumerate(requested_minor))
    if dot(projected,projected) < .04:
        hint = min(((1,0,0),(0,1,0),(0,0,1)), key=lambda v: abs(dot(a,v)))
        projected = tuple(v-a[i]*dot(a,hint) for i,v in enumerate(hint))
    return a, unit(projected)
