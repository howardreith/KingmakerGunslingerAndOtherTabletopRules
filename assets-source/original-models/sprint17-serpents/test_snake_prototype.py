#!/usr/bin/env python3
"""Behavior checks under Blender, using the private research capture explicitly."""
import argparse
import copy
import json
import math
from pathlib import Path
import sys
import unittest

import bmesh

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import generate_snakes as model
import paint_snake_albedo as painting

CAPTURE = None


class PrototypeTests(unittest.TestCase):
    def test_scope_is_exact_and_fails_closed(self):
        original = json.loads(Path(CAPTURE).read_text(encoding="utf-8"))
        for key, value in (("key", "foreign"), ("nativeBlueprint", "foreign"),
                           ("prefab", "foreign")):
            changed = copy.deepcopy(original)
            changed[key] = value
            with self.assertRaises(SystemExit):
                model.decode_rig(changed)
        changed = copy.deepcopy(original)
        changed["skinnedRenderers"].pop()
        with self.assertRaises(SystemExit):
            model.decode_rig(changed)
        for defect in ("duplicate-index", "nonfinite", "cycle"):
            changed = copy.deepcopy(original)
            rows = next(r for r in changed["skinnedRenderers"] if r["renderer"] == "body")["bones"]
            if defect == "duplicate-index":
                rows[1]["index"] = rows[0]["index"]
            elif defect == "nonfinite":
                rows[0]["bindPosition"][0] = float("nan")
            else:
                rows[0]["parent"] = rows[0]["name"]
            with self.assertRaises(SystemExit):
                model.decode_rig(changed)

    def test_original_mesh_closed_finite_and_exactly_weighted(self):
        _, rig, _ = model.measured_rig(CAPTURE)
        for kind in model.KINDS:
            results = []
            for repetition in range(2):
                bm, weights, uvs = bmesh.new(), {}, {}
                model.build_body(bm, weights, uvs, rig, kind)
                bm.verts.index_update()
                self.assertTrue(all(edge.is_manifold for edge in bm.edges), kind)
                self.assertEqual(len(bm.verts), len(weights))
                self.assertEqual(len(bm.verts), len(uvs))
                self.assertEqual(set(model.BODY_BONES),
                    {name for row in weights.values() for name, _ in row})
                self.assertTrue(all(0 < len(row) <= 2 for row in weights.values()))
                for row in weights.values():
                    self.assertAlmostEqual(1, sum(weight for _, weight in row), places=6)
                    self.assertTrue(all(0 < weight <= 1 for _, weight in row))
                self.assertTrue(all(math.isfinite(c) for v in bm.verts for c in v.co))
                self.assertTrue(all(0 < c < 1 for uv in uvs.values() for c in uv))
                self.assertTrue(all(face.calc_area() > 1e-10 for face in bm.faces))
                results.append([(tuple(vertex.co), weights[vertex], uvs[vertex]) for vertex in bm.verts])
                bm.free()
            self.assertEqual(results[0], results[1], "generator is deterministic")

    def test_mouth_and_teeth_stay_in_their_known_subregions(self):
        marker = object()
        for low, high in ((.06, .43), (.57, .94)):
            for u in (0, .25, .5, .75, 1):
                for v in (0, .5, 1):
                    coordinates = {marker: model.shared.region_uv("beak", u, v)}
                    model.remap_beak_uv(coordinates, [marker], low, high)
                    x, y = coordinates[marker]
                    self.assertTrue(.01 <= x <= .49)
                    self.assertTrue(.005 <= y <= .245)
                    self.assertLess(x, .25) if high < .5 else self.assertGreater(x, .25)

    def test_paint_is_original_deterministic_and_distinct(self):
        import numpy as np
        images = [painting.paint(kind, 128) for kind in model.KINDS]
        for kind, pixels in zip(model.KINDS, images):
            self.assertEqual((128, 128, 3), pixels.shape)
            self.assertTrue(np.isfinite(pixels).all())
            self.assertTrue(((pixels >= 0) & (pixels <= 1)).all())
            self.assertTrue(np.array_equal(pixels, painting.paint(kind, 128)))
        self.assertFalse(np.array_equal(images[0], images[1]))


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--capture", required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    CAPTURE = args.capture
    result = unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(PrototypeTests))
    if not result.wasSuccessful():
        raise RuntimeError("original snake prototype behavior checks failed")
