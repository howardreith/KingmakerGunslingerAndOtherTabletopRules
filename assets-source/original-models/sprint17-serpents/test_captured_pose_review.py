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
    @staticmethod
    def capture(contacts):
        matrix = [1, 0, 0, 0, 0, 1, 0, -6, 0, 0, 1, 0, 0, 0, 0, 1]
        floor = dict(rayHit=True, ownedCollider=False, layerMask="0x200101",
                     clearance=.005, hitPoint=[0, -6, 0], normal=[0, 1, 0])
        rows = []
        for key in replay.KEYS:
            names = ["driver" + str(i) for i in range(27 if key == "salamander" else 16)]
            sample = dict(frame=1, finite=True, poseFinite=True,
                          rendererLocalToWorldRowMajor=matrix,
                          skinTransforms=[dict(name=name, skinToWorldRowMajor=list(matrix)) for name in names],
                          actorFloor=copy.deepcopy(floor), lowestVertexFloor=copy.deepcopy(floor))
            row = dict(key=key, sourceCreature="salamander" if key == "salamander" else "purple-worm",
                       scope=replay.CONTACT_SCOPE if contacts else replay.BODY_SCOPE,
                       idleSample=copy.deepcopy(sample), movementSamples=[copy.deepcopy(sample) for _ in range(3)])
            if contacts:
                row["attackPoseSamples"] = [copy.deepcopy(sample) for _ in range(3)]
                row["nativeAttackContacts"] = [dict(frame=1, bodyPose=copy.deepcopy(sample))]
            rows.append(row)
        return rows

    def test_closed_capture_keeps_old_and_new_phases_without_input_mutation(self):
        for contacts in (False, True):
            rows = self.capture(contacts)
            original = copy.deepcopy(rows)
            self.assertIs(rows, replay.validate_capture(rows))
            self.assertEqual(original, rows)
            for row in rows:
                expected = ["idle"] + ["movement"] * 3 + (["attack"] * 3 + ["contact"] if contacts else [])
                self.assertEqual(expected, [phase for phase, _ in replay.pose_samples(row)])
                if contacts:
                    self.assertIs(row["attackPoseSamples"][0], replay.pose_samples(row)[4][1])
                    self.assertIs(row["nativeAttackContacts"][0]["bodyPose"], replay.pose_samples(row)[-1][1])

    def test_capture_rejects_foreign_or_mislabeled_scope(self):
        for scope in ("", "unknown", replay.CONTACT_SCOPE + " qualified", replay.BODY_SCOPE):
            rows = self.capture(True)
            rows[2]["scope"] = scope
            with self.assertRaises(ValueError):
                replay.validate_capture(rows)
        rows = self.capture(False)
        rows[0]["sourceCreature"] = "foreign"
        with self.assertRaises(ValueError):
            replay.validate_capture(rows)

    def test_attack_sample_bounds_are_not_hidden_by_valid_idle_and_movement(self):
        for count in (1, 2, 161):
            rows = self.capture(True)
            sample = rows[2]["attackPoseSamples"][0]
            rows[2]["attackPoseSamples"] = [copy.deepcopy(sample) for _ in range(count)]
            with self.assertRaises(ValueError):
                replay.validate_capture(rows)
        for count in (0, 3, 160):
            rows = self.capture(True)
            sample = rows[2]["attackPoseSamples"][0]
            rows[2]["attackPoseSamples"] = [copy.deepcopy(sample) for _ in range(count)]
            replay.validate_capture(rows)  # A missing attack remains explicitly zero, never invented.
        rows = self.capture(True)
        rows[2]["nativeAttackContacts"] *= 13
        with self.assertRaises(ValueError):
            replay.validate_capture(rows)
        rows = self.capture(True)
        rows[2]["nativeAttackContacts"][0]["frame"] = 2
        with self.assertRaises(ValueError):
            replay.validate_capture(rows)
        rows = self.capture(True)
        rows[2]["nativeAttackContacts"][0]["bodyPose"]["skinTransforms"][0]["skinToWorldRowMajor"][0] = math.nan
        with self.assertRaises(ValueError):
            replay.validate_capture(rows)

    def test_attack_frame_matrix_driver_and_floor_defects_are_rejected(self):
        for defect in ("frame", "finite", "matrix", "renderer", "driver", "duplicate",
                       "hit", "owned", "mask", "slope", "point", "clearance"):
            rows = self.capture(True)
            sample = rows[2]["attackPoseSamples"][1]
            if defect == "frame":
                sample["frame"] = -1
            elif defect == "finite":
                sample["poseFinite"] = False
            elif defect == "matrix":
                sample["skinTransforms"][0]["skinToWorldRowMajor"][3] = math.nan
            elif defect == "renderer":
                sample["rendererLocalToWorldRowMajor"] = [1]
            elif defect == "driver":
                sample["skinTransforms"][0]["name"] = "foreign"
            elif defect == "duplicate":
                sample["skinTransforms"][0]["name"] = sample["skinTransforms"][1]["name"]
            else:
                floor = sample["lowestVertexFloor"]
                field, value = dict(hit=("rayHit", False), owned=("ownedCollider", True),
                                    mask=("layerMask", "0"), slope=("normal", [0, .5, 0]),
                                    point=("hitPoint", [0, math.inf, 0]),
                                    clearance=("clearance", math.nan))[defect]
                floor[field] = value
            with self.assertRaises(ValueError, msg=defect):
                replay.validate_capture(rows)

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
