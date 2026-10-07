#!/usr/bin/env python3
"""Closed original human/Salamander authoring tests, not runtime proof."""
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
import generate_human_salamander as model

CAPTURE = None


class HumanSalamanderTests(unittest.TestCase):
    def capture(self):
        return json.loads(Path(CAPTURE).read_text(encoding="utf-8"))

    def test_exact_capture_and_every_native_slot(self):
        original = self.capture()
        rig, by_name = model.decode_rig(original)
        self.assertEqual(36, len(rig["bones"]))
        self.assertEqual(set(model.BONES), set(by_name))
        for field in ("blueprint", "prefab", "bodyRenderer", "rootBone",
                      "paletteCount", "uniqueTransformCount"):
            bad = copy.deepcopy(original)
            bad[field] = "foreign"
            with self.assertRaises(ValueError, msg=field):
                model.decode_rig(bad)
        bad = copy.deepcopy(original)
        bad["bones"][0]["paletteSlots"].append(1776)
        with self.assertRaises(ValueError):
            model.decode_rig(bad)

    def test_selected_duplicates_must_agree_and_parent_be_exact(self):
        original = self.capture()
        for name in model.NATIVE:
            for field, value in (("maximumBindDifference", .00002),
                                 ("maximumBindDifference", float("nan")),
                                 ("maximumBindDifference", -1),
                                 ("finite", False), ("invertible", False),
                                 ("derivedFrameFinite", False),
                                 ("parent", "weapon-storage")):
                bad = copy.deepcopy(original)
                next(row for row in bad["bones"] if row["name"] == name)[field] = value
                with self.assertRaises(ValueError, msg=name + "/" + field):
                    model.decode_rig(bad)
        bad = copy.deepcopy(original)
        next(row for row in bad["bones"] if row["name"] == "Head")["firstBindPosition"][0] = float("inf")
        with self.assertRaises(ValueError):
            model.decode_rig(bad)

    def test_disagreeing_weapon_storage_is_never_adopted(self):
        original = self.capture()
        disagree = {row["name"] for row in original["bones"]
                    if row.get("maximumBindDifference", 0) > .00001}
        self.assertEqual(17, len(disagree))
        self.assertFalse(set(model.NATIVE) & disagree)
        before = copy.deepcopy(original)
        _, rig = model.decode_rig(original)
        self.assertEqual(before, original, "no input frame repair or first-slot overwrite")
        self.assertFalse(disagree & set(rig))
        self.assertFalse(any(token.lower() in name.lower() for name in model.BONES
                             for token in ("cape", "weapon", "leg", "foot")))

    def test_original_tail_is_independent_and_has_ten_drivers(self):
        _, rig = model.decode_rig(self.capture())
        self.assertEqual(10, len(model.TAIL))
        for index, name in enumerate(model.TAIL):
            self.assertEqual(list(model.REST[index]), rig[name]["head"])
            self.assertEqual(list(model.REST[index + 1]), rig[name]["tail"])
            self.assertEqual(model.TAIL[index - 1] if index else "", rig[name]["parent"])
        self.assertTrue(all(point[1] > 0 for point in model.REST))
        lengths = [(model.Vector(b) - model.Vector(a)).length
                   for a, b in zip(model.REST, model.REST[1:])]
        self.assertTrue(all(.25 < value < .8 for value in lengths))
        self.assertGreater(sum(lengths), 4)
        self.assertLess(sum(lengths), 6)

    def test_finite_closed_repeatable_geometry_weights_and_uv(self):
        _, rig = model.decode_rig(self.capture())
        outputs = []
        for _ in range(2):
            bm, weights, uvs = bmesh.new(), {}, {}
            model.build_body(bm, weights, uvs, rig, "salamander")
            self.assertTrue(all(edge.is_manifold for edge in bm.edges))
            self.assertTrue(all(face.calc_area() > 1e-10 for face in bm.faces))
            self.assertEqual(len(bm.verts), len(weights))
            self.assertEqual(len(bm.verts), len(uvs))
            self.assertEqual(set(model.BONES), {name for row in weights.values() for name, _ in row})
            for row in weights.values():
                self.assertTrue(0 < len(row) <= 2)
                self.assertAlmostEqual(1, sum(value for _, value in row), places=6)
                self.assertTrue(all(0 < value <= 1 for _, value in row))
            self.assertTrue(all(math.isfinite(value) for vertex in bm.verts for value in vertex.co))
            self.assertGreaterEqual(min(vertex.co.y for vertex in bm.verts), .005)
            self.assertTrue(all(0 < value < 1 for uv in uvs.values() for value in uv))
            outputs.append([(tuple(v.co), weights[v], uvs[v]) for v in bm.verts])
            bm.free()
        self.assertEqual(outputs[0], outputs[1])

    def test_coil_waist_neck_share_one_continuous_skin(self):
        _, rig = model.decode_rig(self.capture())
        bm, weights, uvs = bmesh.new(), {}, {}
        model.build_body(bm, weights, uvs, rig, "salamander")
        remaining, groups = set(bm.verts), []
        while remaining:
            pending, found = [next(iter(remaining))], set()
            while pending:
                vertex = pending.pop()
                if vertex in found:
                    continue
                found.add(vertex)
                pending.extend(edge.other_vert(vertex) for edge in vertex.link_edges)
            remaining -= found
            groups.append({name for vertex in found for name, _ in weights[vertex]})
        self.assertTrue(any(set(model.TAIL) | set(model.NATIVE[:6]) <= names for names in groups))
        for side in ("L", "R"):
            for names in groups:
                if any(name.startswith(side + "_Toe_") for name in names):
                    self.assertIn(side + "_Hand", names)
        bm.free()

    def test_other_creatures_cannot_use_the_hybrid_recipe(self):
        _, rig = model.decode_rig(self.capture())
        bm = bmesh.new()
        try:
            for key in ("viper", "constrictor-snake", "", None):
                with self.assertRaises(ValueError):
                    model.build_body(bm, {}, {}, rig, key)
            self.assertEqual(0, len(bm.verts))
        finally:
            bm.free()

    def test_authored_tail_preserves_lengths_and_recovers_without_a_target(self):
        for time in [index * model.DURATION / 140 for index in range(141)]:
            points = model.tail_pose(time)
            self.assertEqual(11, len(points))
            self.assertEqual(tuple(model.Vector(model.REST[0])), tuple(points[0]))
            self.assertTrue(all(math.isfinite(value) for point in points for value in point))
            for index in range(10):
                length = (model.Vector(model.REST[index + 1]) - model.Vector(model.REST[index])).length
                self.assertAlmostEqual(length, (points[index + 1] - points[index]).length, places=5)
        for time in (0, model.DURATION):
            self.assertTrue(all((point - model.Vector(rest)).length < 1e-5
                                for point, rest in zip(model.tail_pose(time), model.REST)))
        for time in (.55, model.ACT_TIME, .70, .80):
            points = model.tail_pose(time)
            self.assertTrue(all(abs(point.x) < 1e-5 for point in points))
            self.assertGreater(points[-1].z, 4)
        for time in (-.001, 1.401, float("nan"), float("inf")):
            with self.assertRaises(ValueError):
                model.tail_pose(time)

    def test_offline_pose_changes_only_original_drivers(self):
        model.common.bpy.ops.wm.read_factory_settings(use_empty=True)
        rig, _ = model.decode_rig(self.capture())
        armature = model.common.shared.build_armature(rig)
        before = {name: armature.pose.bones[name].matrix.copy() for name in model.BONES}
        model.pose_original_tail(armature, model.ACT_TIME)
        for name in model.NATIVE:
            self.assertEqual(before[name], armature.pose.bones[name].matrix, name)
        self.assertTrue(all(before[name] != armature.pose.bones[name].matrix for name in model.TAIL))
        points = model.tail_pose(model.ACT_TIME)
        self.assertTrue(all((armature.pose.bones[name].head - points[index]).length < 1e-5
                            for index, name in enumerate(model.TAIL)))


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--capture", required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    CAPTURE = args.capture
    suite = unittest.defaultTestLoader.loadTestsFromTestCase(HumanSalamanderTests)
    result = unittest.TextTestRunner(verbosity=2).run(suite)
    if not result.wasSuccessful():
        raise SystemExit(1)
