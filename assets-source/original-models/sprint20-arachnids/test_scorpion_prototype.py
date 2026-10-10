#!/usr/bin/env python3
"""Offline fixtures for the Giant Scorpion prototype.

These prove things about the built geometry, not about a render or a
manifest field. A manifest can say a creature has eight legs; only the mesh
can show that eight leg chains carry vertices.

Run under Blender's bundled Python, which is where bmesh and numpy live:

    blender --background --factory-startup --python test_scorpion_prototype.py \\
        -- --capture <private census capture>
"""
import argparse
import sys
import unittest

import bmesh

sys.dont_write_bytecode = True
from pathlib import Path  # noqa: E402

HERE = Path(__file__).resolve()
sys.path.insert(0, str(HERE.parents[1] / "pteranodon"))
sys.path.insert(0, str(HERE.parents[1] / "sprint14-insects"))
sys.path.insert(0, str(HERE.parent))
import generate_scorpion as generator  # noqa: E402
import arachnid_capture as capture  # noqa: E402

CAPTURE = None
_BUILT = {}


def build():
    """Build the body once and reuse it across the fixtures."""
    if "mesh" not in _BUILT:
        rig_data, rig, rig_hash = capture.load(CAPTURE)
        bm = bmesh.new()
        weights, uvs = {}, {}
        generator.build_body(bm, weights, uvs, rig)
        bm.verts.ensure_lookup_table()
        _BUILT["mesh"] = bm
        _BUILT["weights"] = weights
        _BUILT["uvs"] = uvs
        _BUILT["rig"] = rig
        _BUILT["hash"] = rig_hash
    return (_BUILT["mesh"], _BUILT["weights"], _BUILT["uvs"], _BUILT["rig"])


def drivers(weights):
    return {name for entries in weights.values() for name, _ in entries}


class ScorpionPrototypeTests(unittest.TestCase):
    def test_every_vertex_is_weighted_and_painted(self):
        bm, weights, uvs, _ = build()
        self.assertEqual(len(bm.verts), len(weights))
        self.assertEqual(len(bm.verts), len(uvs))
        for entries in weights.values():
            self.assertTrue(entries)
            self.assertLessEqual(len(entries), 4)
            self.assertGreater(sum(value for _, value in entries), 0.0)

    def test_it_walks_on_all_eight_legs(self):
        """The reason this creature fits this rig.

        The Sprint 14 ants leave the fourth chain empty and the beetles hang
        wings on it. A scorpion has eight legs and this rig has eight, so
        every chain must carry real leg geometry - including the feet, which
        is what keeps a leg on the ground.
        """
        _, weights, _, _ = build()
        used = drivers(weights)
        for side in ("L", "R"):
            for index in range(4):
                for part in ("Upper", "Lower"):
                    self.assertIn("%s_Leg%d_%s" % (side, index, part), used)
                self.assertIn("%s_Foot%d" % (side, index), used)
        self.assertEqual({"0": "full", "1": "full", "2": "full", "3": "full"},
                         generator.chain_usage(used))

    def test_the_metasoma_rides_the_abdomen_chain(self):
        """METASOMA_DRIVEN_BY_A_TWO_BONE_CHAIN, proved from the geometry.

        The tail is authored arching up and forward over a chain that is
        level and runs backward. What must be true is that the arch is
        carried by that chain and by nothing else: no invented bone, and no
        quiet reliance on a leg or a pedipalp to hold the tail up.
        """
        _, weights, _, rig = build()
        used = drivers(weights)
        self.assertIn("Tail3_M", used)
        self.assertIn("UpperTorso", used)
        # The chain really is this short. If the rig ever grows a Tail2_M or
        # a Tail4_M the limitation is stale and the tail should be rebuilt.
        for absent in ("Tail0_M", "Tail2_M", "Tail4_M"):
            self.assertNotIn(absent, rig)
        self.assertEqual(2, len(capture.TAIL_BONES))

    def test_the_tail_arches_above_the_body(self):
        """A scorpion's silhouette is the tail over the back.

        Measured from the built vertices rather than from the author's
        intent: the highest geometry weighted to the abdomen chain has to
        sit well above the highest geometry weighted to the carapace.
        """
        bm, weights, _, _ = build()
        tail_top = max(
            vertex.co.z for vertex, entries in weights.items()
            if any(name == "Tail3_M" for name, _ in entries))
        body_top = max(
            vertex.co.z for vertex, entries in weights.items()
            if any(name == "LowerTorso" for name, _ in entries))
        self.assertGreater(tail_top, body_top + 0.6)

    def test_the_chelae_reach_in_front_of_the_body(self):
        """Two claws held forward, which is the printed routine's opening.

        Forward is -Y in this frame. The pedipalp chain is about a fifth as
        long as a leg, so the geometry deliberately extends past it; what
        this checks is that it extends the right way.
        """
        bm, weights, _, rig = build()
        chela_front = min(
            vertex.co.y for vertex, entries in weights.items()
            if any(name.startswith("pedipalp") for name, _ in entries))
        body_front = min(
            vertex.co.y for vertex, entries in weights.items()
            if any(name == "LowerTorso" for name, _ in entries))
        self.assertLess(chela_front, body_front)
        for side in ("L", "R"):
            self.assertIn("pedipalp7_%s" % side, drivers(weights))

    def test_no_unreviewed_bone_carries_geometry(self):
        _, weights, _, _ = build()
        for name in drivers(weights):
            self.assertIn(name, capture.DRIVERS, name)

    def test_the_capture_must_be_the_measured_rig(self):
        """A body fitted to a frame nobody looked at is the failure mode."""
        rig_data, rig, _ = capture.load(CAPTURE)
        self.assertEqual(capture.BONE_COUNT, len(rig_data["bones"]))
        self.assertEqual(capture.RENDERER, rig_data["renderer"])
        self.assertEqual(capture.ROOT_BONE, rig_data["rootBone"])
        self.assertEqual(capture.SPACE, rig_data["space"])


def main():
    global CAPTURE
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--capture", required=True)
    arguments = parser.parse_args(argv)
    CAPTURE = arguments.capture
    runner = unittest.TextTestRunner(verbosity=2)
    suite = unittest.TestLoader().loadTestsFromTestCase(
        ScorpionPrototypeTests)
    if not runner.run(suite).wasSuccessful():
        raise SystemExit("Giant Scorpion prototype fixtures failed")


if __name__ == "__main__":
    main()
