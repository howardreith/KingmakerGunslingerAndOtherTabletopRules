#!/usr/bin/env python3
"""Original hybrid anatomy/provenance tests, not weapon/animation qualification."""
import argparse
import copy
import json
import math
from pathlib import Path
import sys
import unittest

import bmesh
import numpy as np

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import generate_salamander as model
import paint_salamander_albedo as painting

CAPTURE = None
BODY_REVIEW = None


class HybridTests(unittest.TestCase):
    def test_exact_donor_and_complete_frame_only(self):
        original = json.loads(Path(CAPTURE).read_text(encoding="utf-8"))
        for defect in ("key", "nativeBlueprint", "prefab", "missing-skin", "duplicate-index",
                       "nonfinite", "cycle", "disconnected-tail"):
            changed = copy.deepcopy(original)
            rows = next(s for s in changed["skinnedRenderers"] if s["renderer"] == "_lizardman001")["bones"]
            if defect in ("key", "nativeBlueprint", "prefab"):
                changed[defect] = "foreign"
            elif defect == "missing-skin":
                changed["skinnedRenderers"].pop()
            elif defect == "duplicate-index":
                rows[1]["index"] = rows[0]["index"]
            elif defect == "nonfinite":
                rows[0]["bindPosition"][0] = float("nan")
            elif defect == "cycle":
                rows[0]["parent"] = rows[0]["name"]
            else:
                next(r for r in rows if r["name"] == "tail2")["parent"] = "foreign"
            with self.assertRaises(SystemExit, msg=defect):
                model.decode_rig(changed)

    def test_closed_finite_deterministic_body_and_exact_weights(self):
        _, rig, _ = model.measured_rig(CAPTURE)
        results = []
        for _ in range(2):
            bm, weights, uvs = bmesh.new(), {}, {}
            model.build_body(bm, weights, uvs, rig, "salamander")
            self.assertTrue(all(edge.is_manifold for edge in bm.edges))
            self.assertEqual(len(bm.verts), len(weights))
            self.assertEqual(len(bm.verts), len(uvs))
            self.assertEqual(set(model.BODY_BONES),
                             {name for row in weights.values() for name, _ in row})
            for row in weights.values():
                self.assertTrue(0 < len(row) <= 2)
                self.assertAlmostEqual(1, sum(weight for _, weight in row), places=6)
                self.assertTrue(all(0 < weight <= 1 for _, weight in row))
            self.assertTrue(all(math.isfinite(c) for v in bm.verts for c in v.co))
            self.assertTrue(all(0 < c < 1 for uv in uvs.values() for c in uv))
            self.assertTrue(all(face.calc_area() > 1e-10 for face in bm.faces))
            results.append([(tuple(v.co), weights[v], uvs[v]) for v in bm.verts])
            bm.free()
        self.assertEqual(results[0], results[1])

    def test_humanoid_arms_but_no_leg_armor_or_weapon_geometry(self):
        _, rig, _ = model.measured_rig(CAPTURE)
        bm, weights, uvs = bmesh.new(), {}, {}
        model.build_body(bm, weights, uvs, rig, "salamander")
        names = {n for row in weights.values() for n, _ in row}
        self.assertFalse(any(any(token in n for token in ("Leg", "Foot", "toe", "claw")) for n in names))
        for side in ("L", "R"):
            self.assertIn(side + "_Palm", names)
            self.assertIn(side + "_Arm_Lower", names)
        tail = [v for v, row in weights.items() if any(n == "tail3" for n, _ in row)]
        self.assertLess(min(v.co.y for v in tail), -2)
        self.assertGreater(max(v.co.x for v in tail), 1)
        self.assertFalse(any("weapon" in n.lower() or "armor" in n.lower() for n in names))
        bm.free()

    def test_original_paint_is_finite_deterministic_and_distinct_from_snakes(self):
        image = painting.paint(128)
        self.assertEqual((128, 128, 3), image.shape)
        self.assertTrue(np.isfinite(image).all())
        self.assertTrue(((image >= 0) & (image <= 1)).all())
        self.assertTrue(np.array_equal(image, painting.paint(128)))
        self.assertFalse(np.array_equal(image, painting.common.paint("viper", 128)))

    def test_waist_and_finger_roots_are_connected_in_the_authored_skin(self):
        _, rig, _ = model.measured_rig(CAPTURE)
        bm, weights, uvs = bmesh.new(), {}, {}
        model.build_body(bm, weights, uvs, rig, "salamander")
        remaining = set(bm.verts)
        components = []
        while remaining:
            pending, found = [next(iter(remaining))], set()
            while pending:
                vertex = pending.pop()
                if vertex in found:
                    continue
                found.add(vertex)
                pending.extend(edge.other_vert(vertex) for edge in vertex.link_edges)
            remaining -= found
            components.append({name for vertex in found for name, _ in weights[vertex]})
        self.assertTrue(any({"tail3", "Torso_Lower", "Torso_Upper", "neck1"} <= names
                            for names in components), "one continuous waist/lower-body skin")
        for side in ("L", "R"):
            for names in components:
                if names & {side + "_finger1", side + "_Bfinger1"}:
                    self.assertIn(side + "_Palm", names, "finger roots extend into the palm")
        bm.free()

    def test_lower_coil_dorsal_paint_does_not_roll_onto_the_belly(self):
        _, rig, _ = model.measured_rig(CAPTURE)
        bm, weights, uvs = bmesh.new(), {}, {}
        model.build_body(bm, weights, uvs, rig, "salamander")
        _, v0, _, v1 = model.common.shared.ATLAS["body"]
        midpoint = (v0 + v1) / 2
        tail = [v for v, row in weights.items() if any(n == "tail3" for n, _ in row)]
        top = [v for v in tail if v.co.z > .37]
        bottom = [v for v in tail if v.co.z < .05]
        self.assertTrue(top and bottom)
        self.assertTrue(all(uvs[v][1] > midpoint for v in top))
        self.assertTrue(all(uvs[v][1] < midpoint for v in bottom))
        bm.free()

    def test_supported_body_is_closed_deterministic_and_preserves_upper_body_and_distal_tail(self):
        _, rig, _ = model.measured_rig(CAPTURE)
        results = []
        for _ in range(2):
            bm, weights, uvs = bmesh.new(), {}, {}
            model.build_body(bm, weights, uvs, rig, "salamander")
            original = {v: (tuple(v.co), list(weights[v]), uvs[v]) for v in bm.verts}
            model.author_ground_support(weights)
            changed = 0
            for v in bm.verts:
                before = original[v]
                now = (tuple(v.co), weights[v], uvs[v])
                if (any(name not in {"Torso_Lower", "tail", "tail1", "tail2", "tail3"} for name, _ in before[1])
                        or all(name == "tail3" for name, _ in before[1])):
                    self.assertEqual(before, now, "upper body and the complete pure-tail3 distal coil are untouched")
                if before != now:
                    changed += 1
                    self.assertEqual(before[0][:2], tuple(v.co)[:2], "only original vertical sculpt")
                    self.assertEqual(before[2], uvs[v], "paint coordinates unchanged")
                    self.assertTrue(-1.45 < v.co.y < -.55)
                    self.assertGreaterEqual(v.co.z, .0049999)
                self.assertTrue(0 < len(weights[v]) <= 3)
                self.assertAlmostEqual(1, sum(weight for _, weight in weights[v]), places=6)
                self.assertTrue(all(0 < weight <= 1 for _, weight in weights[v]))
            self.assertGreater(changed, 10, "nontrivial authored belly support")
            self.assertTrue(any(row == [(model.observed.HYBRID_SUPPORT, 1)] for row in weights.values()))
            self.assertTrue(all(edge.is_manifold for edge in bm.edges))
            self.assertTrue(all(face.calc_area() > 1e-10 for face in bm.faces))
            self.assertTrue(all(math.isfinite(c) for v in bm.verts for c in v.co))
            results.append([(tuple(v.co), weights[v], uvs[v]) for v in bm.verts])
            bm.free()
        self.assertEqual(results[0], results[1])

    def test_distal_curl_preserves_every_weight_uv_upper_body_and_horizontal_coordinate(self):
        _, rig, _ = model.measured_rig(CAPTURE)
        bm, weights, uvs = bmesh.new(), {}, {}
        model.build_body(bm, weights, uvs, rig, "salamander")
        model.author_ground_support(weights)
        original = {v: (tuple(v.co), list(weights[v]), uvs[v]) for v in bm.verts}
        model.author_distal_clearance(weights)
        lifted = 0
        for vertex, influences in weights.items():
            before = original[vertex]
            self.assertEqual(before[1], influences)
            self.assertEqual(before[2], uvs[vertex])
            self.assertEqual(before[0][:2], tuple(vertex.co)[:2])
            distal = sum(weight for name, weight in influences if name == "tail3")
            if distal == 0:
                self.assertEqual(before[0], tuple(vertex.co), "all non-distal geometry untouched")
            else:
                delta = vertex.co.z - before[0][2]
                self.assertGreater(delta, 0)
                self.assertLessEqual(delta, .1400001)
                lifted += 1
        self.assertGreater(lifted, 100)
        self.assertTrue(all(edge.is_manifold for edge in bm.edges))
        self.assertTrue(all(face.calc_area() > 1e-10 for face in bm.faces))
        bm.free()

    def test_supported_original_body_clears_all_measured_floor_poses(self):
        _, rig, _ = model.measured_rig(CAPTURE)
        bm, weights, uvs = bmesh.new(), {}, {}
        model.build_body(bm, weights, uvs, rig, "salamander", supported=True)
        row = model.observed.capture_rows(BODY_REVIEW)[2]
        vertices = [tuple(v.co) for v in bm.verts]
        influences = [weights[v] for v in bm.verts]
        samples = model.observed.pose_samples(row)
        self.assertGreaterEqual(len(samples), 4)
        for phase, sample in samples:
            points = model.observed.replay(vertices, influences, sample, hybrid_support=True)
            floor = sample["lowestVertexFloor"]["hitPoint"][1]
            clearance = min(p[1] for p in points) - floor
            self.assertGreaterEqual(clearance, -.00001,
                                    "no original vertex below measured floor: " + phase + str(sample["frame"]))
            self.assertLess(clearance, .006, "authored support remains within 6mm, not the former 31cm float")
        bm.free()


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--capture", required=True)
    parser.add_argument("--body-review", required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    CAPTURE = args.capture
    BODY_REVIEW = args.body_review
    result = unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(HybridTests))
    if not result.wasSuccessful():
        raise RuntimeError("original hybrid prototype checks failed")
