#!/usr/bin/env python3
"""Private original continuous-chain prototype behavior and binding checks."""
import argparse
import copy
import json
import math
from pathlib import Path
import sys
import unittest

import bmesh
import bpy
from mathutils import Vector

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import generate_worm_snakes as model
import render_snake_review as review

CAPTURE = None
BODY_REVIEW = None


class WormPrototypeTests(unittest.TestCase):
    def test_authored_support_plane_tracks_every_measured_native_pose(self):
        rows = model.observed.capture_rows(BODY_REVIEW)
        frame = model.observed.snake_support_frame(rows)
        _, rig, _ = model.measured_rig(CAPTURE)
        for row in rows[:2]:
            bm, weights, uvs = bmesh.new(), {}, {}
            model.build_body(bm, weights, uvs, rig, row["key"], frame)
            vertices = [tuple(vertex.co) for vertex in bm.verts]
            influences = [weights[vertex] for vertex in bm.verts]
            for sample in [row["idleSample"]] + row["movementSamples"]:
                points = model.observed.replay(vertices, influences, sample)
                floor = sample["lowestVertexFloor"]["hitPoint"][1]
                clearance = min(point[1] for point in points) - floor
                self.assertGreaterEqual(clearance, -.0001, "original vertices may not penetrate the captured floor plane")
                self.assertLess(clearance, .012, "the repair must not just lift the whole coil into the air")
            self.assertTrue(all(edge.is_manifold for edge in bm.edges))
            self.assertTrue(all(math.isfinite(value) for vertex in bm.verts for value in vertex.co))
            bm.free()

    def test_pose_calibration_rejects_unstable_root_or_wrong_ground_offset(self):
        rows = model.observed.capture_rows(BODY_REVIEW)
        for defect in ("tilt", "height"):
            changed = copy.deepcopy(rows)
            root = next(t for t in changed[1]["movementSamples"][0]["skinTransforms"] if t["name"] == "Hips_Joints")
            root["skinToWorldRowMajor"][4 if defect == "tilt" else 7] += .10
            with self.assertRaises(ValueError):
                model.observed.snake_support_frame(changed)

    def test_supporting_coil_is_continuous_and_above_ground_in_bind_frame(self):
        _, rig, _ = model.measured_rig(CAPTURE)
        for kind in model.KINDS:
            radius = .25 if kind == "viper" else .47
            points, widths = model.supporting_coil(radius)
            self.assertTrue(all(p.y >= radius - 1e-6 for p in points),
                            "mathutils stores float32 coordinates")
            self.assertGreater(max(p.x for p in points) - min(p.x for p in points), 2.5)
            self.assertGreater(max(p.z for p in points) - min(p.z for p in points), 2.5)
            self.assertLess(widths[0], radius * .1)
            self.assertEqual(widths[-1], radius)
            self.assertGreater(model.body_dorsal_at(points[0]).dot(model.FORWARD), .95)
            bm, weights, uvs = bmesh.new(), {}, {}
            model.build_body(bm, weights, uvs, rig, kind)
            root = [v for v, row in weights.items() if row == [("Hips_Joints", 1)]]
            self.assertGreater(len(root), 400)
            self.assertGreaterEqual(min(v.co.y for v in root), -.025)
            visited, pending = set(), [root[0]]
            while pending:
                vertex = pending.pop()
                if vertex in visited:
                    continue
                visited.add(vertex)
                pending.extend(edge.other_vert(vertex) for edge in vertex.link_edges)
            self.assertTrue(all(v in visited for v, row in weights.items()
                                if any(name in model.BODY_CHAIN[:-1] for name, _ in row)),
                            "the supporting tail and upper body must be one surface")
            self.assertTrue(any(v in visited for v, row in weights.items()
                                if row == [("Head", 1)]),
                            "the connected surface must reach the skull; eyes/teeth are separate")
            bm.free()

    def test_review_framing_contains_long_narrow_silhouettes(self):
        camera = bpy.data.objects.new("FramingTest", bpy.data.cameras.new("FramingTest"))
        try:
            points = [Vector((x, y, z)) for x in (-.55, .55)
                      for y in (0, 12.04) for z in (-.44, .22)]
            centre = sum(points, Vector()) / len(points)
            for direction in review.VIEWS.values():
                camera.location = centre + direction.normalized() * 30
                frame_centre, size = review.framing(points, camera)
                axes = camera.rotation_euler.to_matrix()
                for axis in (axes @ Vector((1, 0, 0)), axes @ Vector((0, 1, 0))):
                    self.assertTrue(all(abs((p - frame_centre).dot(axis)) < size / 2
                                        for p in points))
        finally:
            camera_data = camera.data
            bpy.data.objects.remove(camera)
            bpy.data.cameras.remove(camera_data)

    def test_only_exact_native_pair_and_complete_continuous_chain(self):
        original = json.loads(Path(CAPTURE).read_text(encoding="utf-8"))
        for defect in ("key", "nativeBlueprint", "prefab", "missing-skin", "duplicate-index",
                       "nonfinite", "cycle", "disconnected-chain", "detached-jaw"):
            changed = copy.deepcopy(original)
            rows = next(s for s in changed["skinnedRenderers"] if s["renderer"] == "Purple_Worm")["bones"]
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
                next(r for r in rows if r["name"] ==
                     ("Body08" if defect == "disconnected-chain" else "Jaw_Down"))["parent"] = "foreign"
            with self.assertRaises(SystemExit, msg=defect):
                model.decode_rig(changed)

    def test_closed_finite_deterministic_geometry_uses_every_body_segment(self):
        _, rig, _ = model.measured_rig(CAPTURE)
        for kind in model.KINDS:
            results = []
            for _ in range(2):
                bm, weights, uvs = bmesh.new(), {}, {}
                model.build_body(bm, weights, uvs, rig, kind)
                bm.verts.index_update()
                self.assertTrue(all(edge.is_manifold for edge in bm.edges), kind)
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

    def test_no_radial_petals_horn_stones_or_legs_and_distinct_anatomy(self):
        _, rig, _ = model.measured_rig(CAPTURE)
        counts = []
        for kind in model.KINDS:
            bm, weights, uvs = bmesh.new(), {}, {}
            model.build_body(bm, weights, uvs, rig, kind)
            names = {n for row in weights.values() for n, _ in row}
            self.assertFalse(names & {"Jaw_Left", "Jaw_Right", "Jaw_Up", "Horn"})
            self.assertFalse(any("Stone" in name or "Leg" in name for name in names))
            jaw = [v for v, row in weights.items() if row == [("Jaw_Down", 1)]]
            self.assertGreater(len(jaw), 50)
            middle = [v for v, row in weights.items() if any(n == "Body08" for n, _ in row)]
            counts.append((len(bm.verts), max(abs(v.co.x) for v in middle)))
            bm.free()
        self.assertGreater(counts[1][0], counts[0][0], "constrictor has smaller but more teeth")
        self.assertGreater(counts[1][1], counts[0][1] * 1.7, "constrictor is materially heavier")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--capture", required=True)
    parser.add_argument("--body-review", required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    CAPTURE = args.capture
    BODY_REVIEW = args.body_review
    result = unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(WormPrototypeTests))
    if not result.wasSuccessful():
        raise RuntimeError("continuous-chain snake prototype checks failed")
