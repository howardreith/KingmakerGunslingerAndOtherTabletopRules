#!/usr/bin/env python3
"""Behavior regressions for the private original-vertex replay arithmetic."""
import math
import copy
from pathlib import Path
import sys
import unittest

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import captured_pose_review as replay


class PoseReplayTests(unittest.TestCase):
    def test_row_major_affine_transform_uses_translation_once(self):
        matrix = [2, 0, 0, 10, 0, 3, 0, -6, 0, 0, 4, 20, 0, 0, 0, 1]
        self.assertEqual((12, 0, 32), replay.transform_point(matrix, (1, 2, 3)))

    def test_weighted_world_vertices_keep_named_driver_identity(self):
        first = [1, 0, 0, 10, 0, 1, 0, -6, 0, 0, 1, 20, 0, 0, 0, 1]
        second = [1, 0, 0, 14, 0, 1, 0, -8, 0, 0, 1, 24, 0, 0, 0, 1]
        sample = dict(skinTransforms=[dict(name="B", skinToWorldRowMajor=second),
                                      dict(name="A", skinToWorldRowMajor=first)])
        self.assertEqual([(12, -6.5, 21)], replay.replay([(1, 0, 0)], [[("A", .75), ("B", .25)]], sample))
        with self.assertRaises(KeyError):
            replay.replay([(0, 0, 0)], [[("foreign", 1)]], sample)
        with self.assertRaises(ValueError):
            replay.replay([(0, 0, 0)], [], sample)

    def test_support_frame_is_orthonormal_and_has_no_floor_axis_leak(self):
        up = replay.normalized((.01, .96, -.275))
        matrix = [1, 0, 0, 0, up[0], up[1], up[2], -6, 0, 0, 1, 0, 0, 0, 0, 1]
        sample = dict(actorFloor=dict(hitPoint=[0, -6, 0]),
                      skinTransforms=[dict(name="Hips_Joints", skinToWorldRowMajor=matrix)])
        rows = [dict(idleSample=sample, movementSamples=[sample]) for _ in range(2)]
        side, actual_up, forward = replay.snake_support_frame(rows)
        for axis in (side, actual_up, forward):
            self.assertAlmostEqual(1, replay.dot(axis, axis))
        self.assertAlmostEqual(0, replay.dot(side, actual_up))
        self.assertAlmostEqual(0, replay.dot(forward, actual_up))
        self.assertAlmostEqual(0, replay.dot(side, forward))
        point = tuple(side[i] * 2 + actual_up[i] * .25 + forward[i] * 3 for i in range(3))
        self.assertAlmostEqual(-5.75, replay.transform_point(matrix, point)[1])

    def test_invalid_support_direction_is_not_silently_normalized(self):
        for value in ((0, 0, 0), (math.nan, 0, 0), (math.inf, 0, 0)):
            with self.assertRaises(ValueError):
                replay.normalized(value)

    def test_hybrid_support_uses_only_explicit_renderer_frame_without_mutating_capture(self):
        native = [1, 0, 0, 0, 0, 1, 0, 3, 0, 0, 1, 0, 0, 0, 0, 1]
        renderer = [1, 0, 0, 10, 0, 0, 1, -6, 0, -1, 0, 20, 0, 0, 0, 1]
        sample = dict(skinTransforms=[dict(name="Torso_Lower", skinToWorldRowMajor=native)],
                      rendererLocalToWorldRowMajor=renderer)
        original = copy.deepcopy(sample)
        influences = [[(replay.HYBRID_SUPPORT, 1)], [("Torso_Lower", 1)]]
        points = [(1, 2, .005), (1, 2, .005)]
        with self.assertRaises(KeyError):
            replay.replay(points, influences, sample)
        self.assertEqual([(9, -5.995, 22), (1, 5, .005)],
                         replay.replay(points, influences, sample, hybrid_support=True))
        self.assertEqual(original, sample)

    def test_hybrid_support_rejects_foreign_missing_nonfinite_and_ambiguous_frames(self):
        matrix = [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1]
        sample = dict(skinTransforms=[dict(name="Torso_Lower", skinToWorldRowMajor=matrix)],
                      rendererLocalToWorldRowMajor=matrix)
        for defect in ("worm", "foreign", "missing", "short", "nonfinite", "duplicate", "captured-support"):
            changed = copy.deepcopy(sample)
            if defect in ("worm", "foreign"):
                changed["skinTransforms"][0]["name"] = "Hips_Joints" if defect == "worm" else "foreign"
            elif defect == "missing":
                changed.pop("rendererLocalToWorldRowMajor")
            elif defect == "short":
                changed["rendererLocalToWorldRowMajor"] = [1]
            elif defect == "nonfinite":
                changed["rendererLocalToWorldRowMajor"][7] = math.nan
            elif defect == "duplicate":
                changed["skinTransforms"].append(copy.deepcopy(changed["skinTransforms"][0]))
            else:
                changed["skinTransforms"].append(dict(name=replay.HYBRID_SUPPORT, skinToWorldRowMajor=matrix))
            with self.assertRaises((ValueError, KeyError), msg=defect):
                replay.replay([(0, 0, 0)], [[(replay.HYBRID_SUPPORT, 1)]], changed, hybrid_support=True)


if __name__ == "__main__":
    unittest.main(verbosity=2)
