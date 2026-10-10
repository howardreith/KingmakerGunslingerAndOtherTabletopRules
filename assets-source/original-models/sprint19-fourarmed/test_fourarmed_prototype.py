#!/usr/bin/env python3
"""Offline fixtures for the Girallon and Xill prototypes.

Run inside Blender, because the generator is a Blender program:

    blender -b --factory-startup -noaudio \
        --python test_fourarmed_prototype.py -- --capture <census.json>

These prove geometry, skinning and determinism. They prove nothing about the
running game; that is the guarded review's job.

The check this file exists for is `test_each_driver_chain_carries_two_arms`.
Every other claim about four arms is a render someone has to look at or a
field in a manifest someone has to believe. That one reads the built
geometry and shows that each arm bone drives two separated clusters of
vertices, which is what FOUR_ARMS_SHARE_TWO_DRIVER_CHAINS actually means.
"""
import argparse
import math
from pathlib import Path
import sys
import unittest

import bmesh
from mathutils import Vector

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve()
sys.path.insert(0, str(HERE.parents[1] / "sprint18-primates"))
sys.path.insert(0, str(HERE.parent))
import generate_fourarmed as generator
import primate_capture as capture

CAPTURE = None
KINDS = generator.KINDS
SHIPPED = HERE.parents[2].parent / "assets" / "sprint19-fourarmed"

# The bones each arm chain is made of. Both arms on a side are weighted to
# these and to nothing else.
ARM_CHAIN = ("Clavicle_01", "Up_Arm_01", "Forearm_01", "Forearm_02", "Hand_01")


def build(kind):
    """Build one body in memory and hand back its parts."""
    rig_data, rig, _ = capture.load(CAPTURE)
    bm = bmesh.new()
    weights, uvs = {}, {}
    generator.build_body(bm, weights, uvs, rig, kind)
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.normal_update()
    return bm, weights, uvs, rig_data


class PrototypeTests(unittest.TestCase):
    def test_body_is_closed_finite_weighted_and_deterministic(self):
        results = []
        for kind in KINDS:
            bm, weights, uvs, _ = build(kind)
            try:
                self.assertTrue(all(edge.is_manifold for edge in bm.edges), kind)
                self.assertEqual(len(bm.verts), len(weights), kind)
                self.assertEqual(len(bm.verts), len(uvs), kind)
                named = {name for row in weights.values() for name, _ in row}
                self.assertEqual(set(capture.DRIVERS), named,
                                 kind + " must weight every reviewed driver")
                self.assertFalse(named & set(capture.EXCLUDED),
                                 kind + " must leave the empty branches empty")
                self.assertTrue(all(0 < len(row) <= 3 for row in weights.values()))
                for row in weights.values():
                    self.assertAlmostEqual(1, sum(w for _, w in row), places=6)
                    self.assertTrue(all(0 < w <= 1 for _, w in row))
                self.assertTrue(all(math.isfinite(c)
                                    for v in bm.verts for c in v.co))
                self.assertTrue(all(0 < c < 1
                                    for uv in uvs.values() for c in uv))
                self.assertTrue(all(face.calc_area() > 1e-10
                                    for face in bm.faces))
                results.append((len(bm.verts), len(bm.faces)))
            finally:
                bm.free()
        # Determinism: the same capture builds the same body twice.
        for kind, first in zip(KINDS, results):
            bm, _, _, _ = build(kind)
            try:
                self.assertEqual(first, (len(bm.verts), len(bm.faces)),
                                 kind + " generator is deterministic")
            finally:
                bm.free()

    def test_each_driver_chain_carries_two_arms(self):
        """Four arms on two chains, proved from the geometry.

        For each side, the vertices weighted to that side's arm bones must
        fall into two clusters separated by more than either cluster's own
        spread. One cluster would mean the lower arm had not been built; a
        cluster weighted to a bone outside the chain would mean a bone had
        been invented.
        """
        for kind in KINDS:
            bm, weights, _, _ = build(kind)
            try:
                shape = generator.profile(kind)
                for side in ("L", "R"):
                    chain = {side + "_" + part for part in ARM_CHAIN}
                    points = [Vector(vertex.co) for vertex, row in weights.items()
                              if any(name in chain for name, _ in row)]
                    self.assertGreater(len(points), 200,
                                       kind + " " + side + " arm is too sparse")
                    # Split on height, which is the axis the lower arm is
                    # dropped along. The donor bind frame is +Y up.
                    heights = sorted(point.y for point in points)
                    midpoint = (heights[0] + heights[-1]) / 2
                    upper = [p for p in points if p.y > midpoint]
                    lower = [p for p in points if p.y <= midpoint]
                    self.assertGreater(len(upper), 80,
                                       kind + " " + side + " upper arm missing")
                    self.assertGreater(len(lower), 80,
                                       kind + " " + side + " lower arm missing")
                    drop = abs(shape["lowerDrop"].y)
                    span = heights[-1] - heights[0]
                    self.assertGreater(
                        span, drop * 0.9,
                        kind + " " + side + " arms are not separated by the "
                        "authored drop; the lower pair has collapsed onto the "
                        "upper")
            finally:
                bm.free()

    def test_no_arm_vertex_escapes_its_own_side(self):
        """A lower arm is offset, not mirrored.

        The offset carries the lower arm outboard, so a sign error would put
        a left arm's geometry on the right of the midline and the creature
        would wear three arms on one side and one on the other.
        """
        for kind in KINDS:
            bm, weights, _, _ = build(kind)
            try:
                for side, expected in (("L", 1), ("R", -1)):
                    chain = {side + "_" + part for part in ARM_CHAIN}
                    xs = [vertex.co.x for vertex, row in weights.items()
                          if any(name in chain for name, _ in row)]
                    self.assertTrue(
                        all(x * expected > -0.05 for x in xs),
                        kind + " " + side + " arm crosses the midline")
            finally:
                bm.free()

    def test_every_shell_is_outward(self):
        """No inside-out shell, recomputed here and not read from the export.

        The generator refuses a non-positive shell too. These two checks are
        deliberately independent: a winding convention that is wrong in both
        places is a convention, and one that is wrong in one is a bug.
        """
        import bpy
        for kind in KINDS:
            bm, _, _, _ = build(kind)
            mesh = bpy.data.meshes.new("probe" + kind)
            try:
                bm.to_mesh(mesh)
                volumes = generator.component_volumes(mesh)
                self.assertTrue(volumes, kind + " built no shell at all")
                for shell, volume in volumes.items():
                    self.assertGreater(volume, 0,
                                       kind + " shell " + str(shell) +
                                       " is inside out")
            finally:
                bm.free()
                bpy.data.meshes.remove(mesh)

    def test_shipped_bodies_declare_what_they_are(self):
        """The four shipped files say the same thing the generator built."""
        import hashlib
        import json
        for kind in KINDS:
            mesh_path = SHIPPED / (kind + "-mesh.json")
            if not mesh_path.is_file():
                self.skipTest("shipped bodies are not present in this tree")
            body = json.loads(mesh_path.read_text(encoding="utf-8-sig"))
            self.assertEqual(kind, body["creature"])
            self.assertEqual(generator.WINDING, body["triangleWinding"])
            self.assertEqual(capture.PREFAB, body["donorPrefab"])
            self.assertEqual(capture.BODY_RENDERER, body["donorRenderer"])
            self.assertEqual(6, body["visibleLimbs"])
            self.assertEqual(4, body["visibleArms"])
            self.assertEqual(2, body["armDriverChains"])
            self.assertTrue(body["lowerArmsShareUpperArmDrivers"])
            self.assertTrue(body["clawedHands"])
            self.assertEqual("FOUR_ARMS_SHARE_TWO_DRIVER_CHAINS",
                             body["authoredLimitation"])
            self.assertEqual("Large" if kind == "girallon" else "Medium",
                             body["printedSize"])
            self.assertEqual(sorted(capture.DRIVERS), sorted(body["bones"]))
            self.assertFalse(set(body["bones"]) & set(capture.EXCLUDED))
            painting = SHIPPED / body["albedo"]["file"]
            self.assertTrue(painting.is_file())
            self.assertEqual(body["albedo"]["sha256"],
                             hashlib.sha256(painting.read_bytes()).hexdigest(),
                             kind + " ships a painting it does not name")


def main():
    global CAPTURE
    parser = argparse.ArgumentParser()
    parser.add_argument("--capture", required=True)
    args = parser.parse_args(
        sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    CAPTURE = args.capture
    result = unittest.TextTestRunner(verbosity=2).run(
        unittest.TestLoader().loadTestsFromTestCase(PrototypeTests))
    raise SystemExit(0 if result.wasSuccessful() else 1)


if __name__ == "__main__":
    main()
