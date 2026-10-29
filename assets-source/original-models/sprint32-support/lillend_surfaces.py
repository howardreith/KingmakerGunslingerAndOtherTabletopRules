"""Authored Lillend surfaces on the existing closed anatomical/tail drivers.

Uses the project's established signed-distance sculptor; no new runtime system.
All dimensions below are original art decisions relative to measured joints.
"""
import math
import sys
from pathlib import Path

import bpy
import numpy as np
from mathutils import Vector
sys.dont_write_bytecode = True

X, Y, Z = Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1))


def build_body(bm, weights, uvs, rig, kind, common, rest, native, tail):
    if kind != "lillend-azata":
        raise ValueError("exact Lillend surfaces only")
    models = Path(common.__file__).resolve().parents[1]
    for folder in ("outsider-celestial", "continuous-bodies"):
        sys.path.insert(0, str(models / folder))
    import sdf_sculpt as sd
    import continuous_body as cb
    at = lambda name: common.shared.head(rig, name)
    uv = common.shared.region_uv

    def mass(centre, radii, bone, region, material_v=None):
        before = set(bm.verts)
        common.ellipsoid(bm, weights, uvs, centre, X, Y, Z,
                         radii, bone, region, 8, 16)
        if material_v is not None:
            for vertex in set(bm.verts) - before:
                uvs[vertex] = uv(region, .5, material_v)

    # Keep the released rest centreline and action: opening the visible coil
    # comes from the progressive cross-section rather than changed contact.
    points = list(reversed([Vector(p) for p in rest])) + [Vector((0, .66, 0)), at("Pelvis")]
    names = list(reversed(list(tail) + [tail[-1]])) + ["Pelvis", "Pelvis"]
    widths = list(reversed([.255, .24, .215, .19, .162, .135, .105, .075, .047, .024, .006])) + [.205, .205]
    heights = list(reversed([.255, .215, .19, .17, .146, .124, .096, .069, .042, .021, .006])) + [.16, .16]
    def dorsal(point):
        upright = max(0, min(1, (point.y - .29) / .18))
        return Y * (1 - upright) - Z * upright
    common.sweep(bm, weights, uvs, points, widths, heights, names,
                 steps=6, sides=24, up=Z, forward=Y, uv_dorsal_at=dorsal)

    # A single sculpted skin surface: shoulders join arms, wrists join palms,
    # and fingers grow from the palms. Carved facial planes survive reduction.
    heads = np.array([tuple(at(name)) for name in native])
    lower = heads.min(axis=0) - (.17, .12, .20)
    upper = heads.max(axis=0) + (.17, .26, .24)
    field = sd.Field(lower, upper, .0045)
    p, s1, s2, s3, neck, head = [at(name) for name in native[:6]]
    field.chain([p, s1, s2, s3], [.14, .115, .13, .135], k=.045)
    for sign, side in ((-1, "L"), (1, "R")):
        shoulder, elbow, wrist = [at(side + suffix) for suffix in ("_Up_arm", "_ForeArm", "_Hand")]
        field.chain([s3, at(side + "_Clavicle"), shoulder], [.125, .085, .078], k=.045)
        field.chain([shoulder, shoulder.lerp(elbow, .45), elbow, elbow.lerp(wrist, .55), wrist],
                    [.075, .068, .047, .043, .030], k=.025)
        digits = [at(side + "_Toe_%d_01" % d) for d in (1, 2, 3)]
        palm = (sum(digits, Vector()) / 3 + wrist) / 2
        field.cone(wrist, palm, .031, .039, k=.018)
        field.ellipsoid(palm, (.040, .043, .029), k=.018)
        for digit in (1, 2, 3):
            first, second = [at(side + "_Toe_%d_%02d" % (digit, j)) for j in (1, 2)]
            direction = (second - first).normalized()
            if digit == 3:
                field.chain([palm, first, second, second + direction * .015],
                            [.021, .011, .009, .005], k=.007)
            else:
                lateral = (digits[0] - digits[1]).normalized() * .010
                for spread in (-1, 1):
                    a, b = first + lateral * spread, second + lateral * spread
                    field.chain([palm, a, b, b + direction * .018],
                                [.019, .0075, .0065, .004], k=.004)
    field.chain([s3, neck, head + Y * .035], [.083, .061, .057], k=.035)
    field.ellipsoid(head + Y * .090 + Z * .008, (.092, .126, .084), k=.028)
    field.ellipsoid(head + Y * .034 + Z * .053, (.076, .076, .062), k=.018)
    field.ellipsoid(head - Y * .014 + Z * .077, (.041, .026, .030), k=.012)
    for sign in (-1, 1):
        field.ellipsoid(head + X * sign * .052 + Y * .064 + Z * .080,
                        (.031, .039, .029), k=.018)
        # Adult orbital recess, brow and lower eyelid, not a painted dot.
        eye = head + X * sign * .038 + Y * .088 + Z * .104
        field.ellipsoid(eye, (.027, .013, .019), op="cut", k=.004)
        field.cone(eye + X * sign * .025 + Y * .016 - Z * .006,
                   eye - X * sign * .022 + Y * .019, .010, .008, k=.009)
    field.ellipsoid(head + Y * .067 + Z * .113, (.013, .037, .018), k=.012)
    field.ellipsoid(head + Y * .037 + Z * .129, (.020, .012, .019), k=.007)
    field.ellipsoid(head + Y * .014 + Z * .106, (.029, .0055, .011), k=.004, material=1)
    field.ellipsoid(head + Y * .009 + Z * .112, (.026, .0025, .009), op="cut", k=.001, material=1)
    skin = field.polygonise("LillendContinuousSkin", largest_only=True)
    cb.finish_surface(skin, 15500, smooth_iterations=1)

    segments = []
    for name in native:
        parent = rig[name]["parent"]
        if parent in native:
            segments.append((parent, name, at(parent), at(name)))
    def skin_weights(point):
        if point.y > neck.y + .025 and abs(point.x - head.x) < .20:
            return [("Head", 1)]
        best = None
        for a_name, b_name, a, b in segments:
            delta = b - a
            t = max(0, min(1, (point - a).dot(delta) / max(1e-10, delta.length_squared)))
            distance = (point - (a + delta * t)).length_squared
            if best is None or distance < best[0]:
                best = (distance, a_name, b_name, t)
        _, a, b, t = best
        return [(name, value) for name, value in ((a, 1 - t), (b, t)) if value > 1e-7]
    material_ids = field.material_at(np.array([tuple(v.co) for v in skin.data.vertices]))
    vertices = []
    for old, material in zip(skin.data.vertices, material_ids):
        vertex = bm.verts.new(old.co)
        vertices.append(vertex)
        weights[vertex] = skin_weights(vertex.co)
        value = .72 if material == 1 else .22 + .30 * max(0, min(1, (vertex.co.y - p.y) / (head.y + .22 - p.y)))
        uvs[vertex] = uv("limbs", .25 + .5 * max(0, min(1, vertex.co.z + .4)), value)
    for polygon in skin.data.polygons:
        bm.faces.new([vertices[i] for i in polygon.vertices])
    bpy.data.objects.remove(skin, do_unlink=True)

    # Small inset almond eyes, irises, brows and pointed pinnae.
    for sign in (-1, 1):
        eye = head + X * sign * .038 + Y * .088 + Z * .102
        mass(eye, (.016, .0055, .008), "Head", "limbs", .82)
        mass(eye + Z * .007, (.0045, .0045, .0025), "Head", "limbs", .92)
        common.tube(bm, weights, uvs,
                    [eye - X * .024 + Y * .013, eye + Y * .019, eye + X * .025 + Y * .011],
                    [.002, .003, .0015], ["Head"] * 3, "crest", 6)
        ear = head + X * sign * .083 + Y * .060
        common.sweep(bm, weights, uvs,
                     [ear, ear + X * sign * .045 + Y * .025,
                      ear + X * sign * .097 + Y * .070 - Z * .025],
                     [.021, .025, .0015], [.036, .022, .0015], ["Head"] * 3,
                     region="limbs", steps=3, sides=10, up=Y, forward=Z)

    # Real short garment shell with a shaped neckline, layered hem and folds.
    # Its rings interpolate the same torso drivers as the covered skin.
    waist = p.y * .5 + s2.y * .5
    rings = [
        (p.y - .19, .253, .190, "Pelvis", "beak", .76),
        (p.y + .015, .225, .172, "Pelvis", "beak", .78),
        (waist, .178, .150, "Spine_1", "beak", .76),
        (s2.y + .05, .216, .177, "Spine_2", "beak", .76),
        (s3.y + .14, .242, .190, "Spine_3", "beak", .76),
        (neck.y - .045, .090, .078, "Neck", "beak", .76)]
    def garment(rows, colour_u, folds):
        built = []
        for i, (height, width, depth, bone, region, material_v) in enumerate(rows):
            ring = []
            for j in range(48):
                angle = j * math.tau / 48
                front = max(0, math.sin(angle))
                fold = folds * math.sin(7 * angle + i * .30) * (1 - i / max(1, len(rows) - 1))
                y = height - (.038 * front ** 3 if i == len(rows) - 1 else 0)
                centre = at(bone)
                v = bm.verts.new(Vector((centre.x + math.cos(angle) * (width + fold), y,
                                         centre.z + math.sin(angle) * (depth + fold))))
                ring.append(v); weights[v] = [(bone, 1)]
                uvs[v] = uv(region, colour_u, material_v + .025 * math.cos(7 * angle))
            if built:
                for j in range(48):
                    bm.faces.new((built[-1][j], built[-1][(j + 1) % 48], ring[(j + 1) % 48], ring[j]))
            built.append(ring)
        # Fold the rim inward so the shell has a visible hem instead of a
        # paper-thin silhouette. The body underneath closes the garment view.
        for ring in (built[0], built[-1]):
            inner = []
            for v in ring:
                p0 = v.co.copy(); p0.x *= .97; p0.z *= .97
                w = bm.verts.new(p0); weights[w] = weights[v][:]; uvs[w] = uv("beak", .94, .5); inner.append(w)
            for j in range(48):
                bm.faces.new((ring[j], ring[(j + 1) % 48], inner[(j + 1) % 48], inner[j]))
        return built
    garment(rings, .76, .008)
    # Teal fitted bodice and overskirt, with ivory visible between split panels.
    garment([(p.y - .17, .260, .198, "Pelvis", "beak", .48),
             (p.y + .02, .235, .184, "Pelvis", "beak", .46),
             (waist, .186, .158, "Spine_1", "beak", .45),
             (s2.y + .05, .224, .185, "Spine_2", "beak", .48),
             (s3.y + .10, .248, .200, "Spine_3", "beak", .48)], .30, .010)
    # A narrow gold belt is modeled once, not periodically painted stripes.
    garment([(waist - .013, .204, .178, "Spine_1", "beak", .5),
             (waist + .013, .204, .178, "Spine_1", "beak", .5)], .94, .0)
    # Paired narrow seams connect the bodice construction to its belt.
    for sign in (-1, 1):
        before = set(bm.verts)
        common.tube(bm, weights, uvs,
                    [Vector((sign*.19, s3.y+.08, .155)), Vector((sign*.105, s2.y+.04, .171)),
                     Vector((sign*.095, waist+.015, .164))], [.003,.003,.003],
                    ['Spine_3','Spine_2','Spine_1'], 'beak', 6)
        for vertex in set(bm.verts)-before: uvs[vertex] = uv('beak', .94, .5)
    for side in ('L', 'R'):
        shoulder, elbow = at(side + '_Up_arm'), at(side + '_ForeArm')
        common.sweep(bm, weights, uvs,
                     [shoulder, shoulder.lerp(elbow, .23), shoulder.lerp(elbow, .45)],
                     [.081, .086, .079], [.081, .086, .079],
                     [side + '_Up_arm'] * 3, region='beak', steps=3, sides=20, up=Z, forward=Y)
        # Remap the last sleeve's vertices to ivory fabric.
        for vertex in list(bm.verts)[-((2*3+1)*20+2):]:
            uvs[vertex] = uv('beak', .76, .5)

    # Auburn cap and irregular, flattened locks with tapered ends and curls.
    mass(head + Y * .143 - Z * .010, (.105, .080, .097), "Head", "crest", .45)
    for lock in range(9):
        t = lock / 8
        sign = -1 if lock < 4 else 1
        common.sweep(bm, weights, uvs,
                     [head + X * (-.025 + .050*t) + Y*.201 + Z*.015,
                      head + X * sign*.055 + Y*(.147+.020*t) + Z*.088,
                      head + X * sign*.096 + Y*.105 + Z*.050,
                      head + X * sign*.113 + Y*.030 - Z*.018],
                     [.016,.027,.024,.008],[.009,.014,.013,.004], ['Head']*4,
                     region='crest', steps=4, sides=10, up=Z, forward=Y)
    for sign in (-1, 1):
        for lock in range(7):
            t = lock / 6
            root = head + X * sign * (.015 + .072 * t) + Y * (.196 - .035 * t) - Z * (.018 + .045 * t)
            end = s3 + X * sign * (.13 + .028 * math.sin(lock * 1.7)) - Z * (.13 + .035 * t) + Y * (.045 + .035 * math.cos(lock))
            points = [root, head + X * sign * (.09 + .022 * t) + Y * .113 - Z * .022,
                      head + X * sign * (.116 + .018 * math.sin(lock)) - Y * .015 - Z * (.06 + .025 * t),
                      end + Y * .075 + X * sign * .019, end]
            common.sweep(bm, weights, uvs, points,
                         [.018, .032, .030, .022, .003], [.010, .018, .018, .012, .002],
                         ["Head", "Head", "Head", "Spine_3", "Spine_3"],
                         region="crest", steps=4, sides=10, up=Z, forward=Y)

    # Closed, curved feather vanes: two coverts rows over varied secondaries
    # and long tapered primaries. All remain on the approved Spine_3 driver.
    def feather(base, tip, width, twist=0):
        axis = (tip - base).normalized()
        lateral = axis.cross(Z).normalized()
        normal = lateral.cross(axis).normalized()
        rows = []
        for i in range(10):
            t = i / 9
            centre = base.lerp(tip, t) - Z * (.025 * math.sin(math.pi * t))
            w = max(.001, width * math.sin(math.pi * (.025 + .95 * t)) ** .70 * (1 - .40 * t))
            ring = []
            for j in range(8):
                angle = j * math.tau / 8
                v = bm.verts.new(centre + lateral * (w * math.cos(angle)) + normal * (.006 * math.sin(angle)))
                weights[v] = [("Spine_3", 1)]
                uvs[v] = uv("membrane", .5 + .46 * math.cos(angle), .04 + .92 * t)
                ring.append(v)
            if rows:
                for j in range(8): bm.faces.new((rows[-1][j], rows[-1][(j + 1) % 8], ring[(j + 1) % 8], ring[j]))
            rows.append(ring)
        for ring in (rows[0], rows[-1]):
            v = bm.verts.new(sum((p0.co for p0 in ring), Vector()) / len(ring))
            weights[v] = [("Spine_3", 1)]; uvs[v] = uv("membrane", .5, .5)
            for j in range(8): bm.faces.new((v, ring[j], ring[(j + 1) % 8]))
    for sign in (-1, 1):
        root = s3 + X * sign * .135 - Z * .145
        elbow = root + X * sign * .29 + Y * .32 - Z * .13
        wrist = root + X * sign * .49 + Y * .39 - Z * .27
        for i in range(14):
            t = i / 13
            base = root.lerp(wrist, t) + Y*.045 + Z*.015
            feather(base, base + X*sign*.05 - Y*.13 + Z*.035, .055)
        for i in range(11):
            t = i / 10
            base = root.lerp(wrist, t)
            tip = base + X * sign * (.12 + .15 * t) - Y * (.34 + .20 * t) - Z * (.12 + .13 * t)
            feather(base, tip, .055 + .009 * math.sin(i))
        for layer in range(2):
            for i in range(12):
                t = i / 11
                base = root.lerp(wrist, t) + Y * (.025 - layer * .062) + Z * (.025 + layer * .012)
                tip = base + X * sign * (.07 + .08 * t) - Y * (.16 + .065 * layer) - Z * .035
                feather(base, tip, .041 + .009 * layer)
