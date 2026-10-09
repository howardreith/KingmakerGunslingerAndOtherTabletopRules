#!/usr/bin/env python3
"""Offline checks on the two original ape bodies, run under Blender.

These prove what can be proved without the game: that the capture decoder
fails closed, that both bodies are closed, finite, fully weighted and
deterministic, that neither weights a deliberately empty donor branch,
that every shell is outward in the frame the exporter assumes, and that
the two paintings are original, deterministic and different from each
other. None of this proves a runtime behaviour.
"""
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
import generate_primates as model
import paint_primate_albedo as painting
import primate_capture as capture

CAPTURE = None


class PrototypeTests(unittest.TestCase):
    def test_capture_scope_is_exact_and_fails_closed(self):
        original = json.loads(Path(CAPTURE).read_bytes().decode("utf-8-sig"))
        capture.decode(original)
        for key, value in (("scope", "something else"),
                           ("installedPrimateUnitTypes", 1),
                           ("capturedPrefabCount", 27)):
            changed = copy.deepcopy(original)
            changed[key] = value
            with self.assertRaises(SystemExit):
                capture.decode(changed)
        for key, value in (("nativeBlueprint", "foreign"), ("prefab", "foreign"),
                           ("view", "foreign"), ("size", "Huge"),
                           ("meetsMinimumCredibleBindHeight", False)):
            changed = copy.deepcopy(original)
            row = next(r for r in changed["rows"]
                       if r["nativeBlueprint"] == capture.BLUEPRINT)
            row[key] = value
            with self.assertRaises(SystemExit):
                capture.decode(changed)
        for defect in ("duplicate-index", "nonfinite", "cycle", "missing-driver",
                       "missing-empty-branch", "skipped-renderer"):
            changed = copy.deepcopy(original)
            row = next(r for r in changed["rows"]
                       if r["nativeBlueprint"] == capture.BLUEPRINT)
            body = next(s for s in row["skinnedRenderers"]
                        if s["renderer"] == capture.BODY_RENDERER)
            if defect == "duplicate-index":
                body["bones"][1]["index"] = body["bones"][0]["index"]
            elif defect == "nonfinite":
                body["bones"][0]["bindPosition"][0] = float("nan")
            elif defect == "cycle":
                body["bones"][0]["parent"] = body["bones"][0]["name"]
            elif defect == "missing-driver":
                target = next(b for b in body["bones"] if b["name"] == "Jaw_01")
                target["name"] = "Jaw_Renamed"
            elif defect == "missing-empty-branch":
                target = next(b for b in body["bones"] if b["name"] == "Tail_01")
                target["name"] = "Tail_Renamed"
            else:
                row["skippedRenderers"] = ["something"]
            with self.assertRaises(SystemExit):
                capture.decode(changed)

    def test_body_is_closed_finite_weighted_and_deterministic(self):
        _, rig, _ = capture.load(CAPTURE)
        for kind in model.KINDS:
            results = []
            for _ in range(2):
                bm, weights, uvs = bmesh.new(), {}, {}
                model.build_body(bm, weights, uvs, rig, kind)
                bm.verts.index_update()
                self.assertTrue(all(edge.is_manifold for edge in bm.edges), kind)
                self.assertEqual(len(bm.verts), len(weights))
                self.assertEqual(len(bm.verts), len(uvs))
                named = {name for row in weights.values() for name, _ in row}
                self.assertEqual(set(capture.DRIVERS), named,
                                 kind + " must use exactly the reviewed drivers")
                self.assertFalse(named & set(capture.EXCLUDED),
                                 kind + " must leave the tail and tongue empty")
                self.assertTrue(all(0 < len(row) <= 3 for row in weights.values()))
                for row in weights.values():
                    self.assertAlmostEqual(1, sum(w for _, w in row), places=6)
                    self.assertTrue(all(0 < w <= 1 for _, w in row))
                self.assertTrue(all(math.isfinite(c) for v in bm.verts for c in v.co))
                self.assertTrue(all(0 < c < 1 for uv in uvs.values() for c in uv))
                self.assertTrue(all(face.calc_area() > 1e-10 for face in bm.faces))
                results.append([(tuple(v.co), weights[v], uvs[v]) for v in bm.verts])
                bm.free()
            self.assertEqual(results[0], results[1], kind + " generator is deterministic")

    def test_every_shell_is_outward_before_the_exporter_reverses_it(self):
        """The check the shipped winding depends on.

        Blender orients a closed shell outward, which in its right-handed
        frame makes the shell's signed volume positive; the exporter then
        reverses the winding once for the engine's left-handed frame. If a
        shell came out negative here, no downstream reversal could make
        both it and its neighbours right.
        """
        _, rig, _ = capture.load(CAPTURE)
        for kind in model.KINDS:
            bm, weights, uvs = bmesh.new(), {}, {}
            model.build_body(bm, weights, uvs, rig, kind)
            bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
            bm.verts.index_update()
            seen, shells = set(), []
            for vertex in bm.verts:
                if vertex in seen:
                    continue
                stack, group = [vertex], []
                seen.add(vertex)
                while stack:
                    current = stack.pop()
                    group.append(current)
                    for edge in current.link_edges:
                        other = edge.other_vert(current)
                        if other not in seen:
                            seen.add(other)
                            stack.append(other)
                shells.append(set(group))
            for group in shells:
                volume = 0.0
                for face in bm.faces:
                    if face.verts[0] not in group:
                        continue
                    loop = list(face.verts)
                    for corner in range(1, len(loop) - 1):
                        a, b, c = loop[0].co, loop[corner].co, loop[corner + 1].co
                        volume += a.dot(b.cross(c)) / 6.0
                self.assertGreater(volume, 0.0, kind + " has an inside-out shell")
            self.assertGreaterEqual(len(shells), 40, kind)
            bm.free()

    def test_the_two_bodies_are_actually_different_creatures(self):
        _, rig, _ = capture.load(CAPTURE)
        built = {}
        for kind in model.KINDS:
            bm, weights, uvs = bmesh.new(), {}, {}
            model.build_body(bm, weights, uvs, rig, kind)
            built[kind] = sorted(tuple(round(c, 5) for c in v.co) for v in bm.verts)
            bm.free()
        self.assertNotEqual(built["ape"], built["dire-ape"])
        # The Dire Ape is the bulkier of the two and wears a collar the Ape
        # does not, so it carries strictly more geometry.
        self.assertGreater(len(built["dire-ape"]), len(built["ape"]))
        self.assertTrue(model.profile("dire-ape")["clawed"])
        self.assertFalse(model.profile("ape")["clawed"])
        self.assertGreater(model.profile("dire-ape")["crest"],
                           model.profile("ape")["crest"])

    def test_paint_is_original_deterministic_and_distinct(self):
        import numpy as np
        images = [painting.paint(kind, 128) for kind in model.KINDS]
        for kind, pixels in zip(model.KINDS, images):
            self.assertEqual((128, 128, 3), pixels.shape)
            self.assertTrue(np.isfinite(pixels).all())
            self.assertTrue(((pixels >= 0) & (pixels <= 1)).all())
            self.assertTrue(np.array_equal(pixels, painting.paint(kind, 128)))
        self.assertFalse(np.array_equal(images[0], images[1]))

    def test_the_atlas_the_generator_writes_is_the_atlas_the_painter_fills(self):
        self.assertEqual(model.PRIMATE_ATLAS, painting.PRIMATE_ATLAS)
        for name, region in model.PRIMATE_ATLAS.items():
            u0, v0, u1, v1 = region
            self.assertTrue(0 <= u0 < u1 <= 1 and 0 <= v0 < v1 <= 1, name)
        boxes = list(model.PRIMATE_ATLAS.values())
        for first in range(len(boxes)):
            for second in range(first + 1, len(boxes)):
                a, b = boxes[first], boxes[second]
                overlap = (min(a[2], b[2]) > max(a[0], b[0]) and
                           min(a[3], b[3]) > max(a[1], b[1]))
                self.assertFalse(overlap, "atlas regions overlap")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--capture", required=True)
    args = parser.parse_args(
        sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    CAPTURE = args.capture
    result = unittest.TextTestRunner(verbosity=2).run(
        unittest.defaultTestLoader.loadTestsFromTestCase(PrototypeTests))
    if not result.wasSuccessful():
        raise RuntimeError("original primate prototype behavior checks failed")
