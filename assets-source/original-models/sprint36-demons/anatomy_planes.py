"""Scoped original authoring primitives for the revised demon bodies.

These shape surfaces around the existing measured joints; they never move a
native joint, create a driver, or supply animation/combat behavior.
"""
from mathutils import Vector


def muscle(common, bm, weights, uvs, a, b, width, depth, bone, region):
    axis = (b-a).normalized()
    across = axis.cross(Vector((0, 1, 0)))
    if across.length < .01:
        across = axis.cross(Vector((0, 0, 1)))
    across.normalize()
    normal = across.cross(axis).normalized()
    common.ellipsoid(bm, weights, uvs, (a+b)*.5, across, axis, normal,
                     (width, (b-a).length*.60, depth), bone, region, 9, 12)


def bevel_mass(common, bm, weights, uvs, centre, radii, bone, region,
               front_taper=.82):
    """Closed beveled eight-sided mass: jaw/heel/forefoot planes, not a ball."""
    width, height, depth = radii
    perimeter = [(-.70,-1),(.70,-1),(1,-.65),(front_taper,.65),
                 (.65,1),(-.65,1),(-front_taper,.65),(-1,-.65)]
    rows=[]
    for z, scale in [(-1,.70),(-.65,1),(.65,1),(1,.70)]:
        row=[]
        for i,(x,y) in enumerate(perimeter):
            v=bm.verts.new(centre + Vector((x*width*scale,y*depth*scale,z*height)))
            weights[v]=[(bone,1)]
            uvs[v]=common.shared.region_uv(region,.08+.84*i/8,.12+.76*(z+1)/2)
            row.append(v)
        rows.append(row)
    bm.faces.new(list(reversed(rows[0])))
    for lower, upper in zip(rows,rows[1:]):
        for i in range(8):
            j=(i+1)%8
            bm.faces.new((lower[i],lower[j],upper[j],upper[i]))
    bm.faces.new(rows[-1])
