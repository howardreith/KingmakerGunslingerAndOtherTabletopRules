"""Reject identity, suppression, icon and binary drift at the Sprint 18 seam.

These are corruption fixtures for the Sprint 18 gate itself: each one proves
the validator rejects a specific way the sprint could go wrong, so a passing
gate means something.
"""
import copy
import json
import tempfile
import unittest
import sys
from pathlib import Path
from unittest.mock import patch

sys.dont_write_bytecode = True

import validate_expanded_summoning_sprint18147 as gate
from validate_repository import VALIDATORS

ROOT = Path(__file__).resolve().parents[1]


class Sprint18Tests(unittest.TestCase):
    def test_version_dispatch_and_historical_dispatch_are_intact(self):
        self.assertEqual("validate_expanded_summoning_sprint18147.py",
                         VALIDATORS["0.0.147"])
        self.assertEqual("validate_expanded_summoning_checkpoint146.py",
                         VALIDATORS["0.0.146"])
        self.assertEqual("validate_heirloom_nodachi_icon145.py",
                         VALIDATORS["0.0.145"])

    def test_the_append_is_exactly_fifty_eight_identities(self):
        self.assertEqual(2982, gate.validate_identity_append(ROOT))

    def test_a_moved_historical_identity_is_rejected(self):
        current = gate.document(ROOT, "blueprints/blueprints.json")
        altered = copy.deepcopy(current)
        altered["entries"][1], altered["entries"][0] = \
            altered["entries"][0], altered["entries"][1]
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "moved or rewrote a historical identity"):
                gate.validate_identity_append(ROOT)

    def test_an_extra_appended_identity_is_rejected(self):
        current = gate.document(ROOT, "blueprints/blueprints.json")
        altered = copy.deepcopy(current)
        altered["entries"].append({
            "symbol": "KMG.Summoning.Special.Girallon.Rend",
            "guid": "0" * 32, "plannedType": "BlueprintFeature",
            "status": "active", "milestone": "Expanded Summoning", "notes": ""})
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError, "exactly 60 identities"):
                gate.validate_identity_append(ROOT)

    def test_publishing_an_ape_early_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / "src/KingmakerGunslinger/Summoning"
            source.mkdir(parents=True)
            (source / "SummonVisibilityCatalog.cs").write_text(
                "RegisteredLogicalPlacementCount = 1034;\n"
                "SuppressedLogicalPlacementCount = 0;\n"
                'new HashSet<string>(StringComparer.Ordinal);\n',
                encoding="utf-8")
            with self.assertRaisesRegex(
                    AssertionError, "publication surface differs"):
                gate.validate_suppression(root)

    def test_a_changed_placement_contract_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / "src/KingmakerGunslinger/Summoning"
            source.mkdir(parents=True)
            (source / "SummonVisibilityCatalog.cs").write_text(
                "RegisteredLogicalPlacementCount = 1034;\n"
                "SuppressedLogicalPlacementCount = 26;\n"
                '"ape", "dire-ape"\n', encoding="utf-8")
            (source / "ExpandedSummoningCatalog.cs").write_text(
                'C("ape","Ape",3,true,3,"Owlbear"),\n'
                'C("dire-ape","Dire Ape",5,true,5,"Owlbear"),\n'
                "ValidateFamily(SummonFamily.Monster, 90, 519)\n"
                "ValidateFamily(SummonFamily.NaturesAlly, 88, 515)\n",
                encoding="utf-8")
            with self.assertRaisesRegex(
                    AssertionError, "placement contract differs"):
                gate.validate_suppression(root)

    def test_one_painting_cannot_serve_both_apes(self):
        manifest = gate.document(
            ROOT,
            "assets-source/original-icons/expanded-summoning/icon-manifest.json")
        altered = copy.deepcopy(manifest)
        rows = {row["key"]: row for row in altered["icons"]}
        rows["dire-ape"]["outputSha256"] = rows["ape"]["outputSha256"]
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "must not share one painting"):
                gate.validate_icons(ROOT)

    def test_a_withheld_creature_still_needs_its_own_icon(self):
        manifest = gate.document(
            ROOT,
            "assets-source/original-icons/expanded-summoning/icon-manifest.json")
        altered = copy.deepcopy(manifest)
        altered["icons"] = [row for row in altered["icons"] if row["key"] != "ape"]
        altered["count"] = len(altered["icons"])
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError, "must be 111"):
                gate.validate_icons(ROOT)

    def test_the_frozen_rend_line_cannot_drift(self):
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT18-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["direApe"]["printedProfile"]["specialAttacks"][0]["damage"] = "1d6+6"
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError, "printed rend line changed"):
                gate.validate_contract(ROOT)

    def test_the_frozen_arithmetic_cannot_drift(self):
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT18-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["placement"]["newRootTotal"] = 24
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError, "arithmetic drifted"):
                gate.validate_contract(ROOT)

    def test_binary_bytes_are_not_line_normalized(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "asset.png").write_bytes(b"binary\r\npayload")
            with patch.object(gate.subprocess, "check_output",
                              return_value="asset.png\n"), \
                    patch.object(gate, "blob", return_value=b"binary\npayload"):
                with self.assertRaisesRegex(
                        AssertionError, "Accepted production changed"):
                    gate.preserved_files(root, "accepted", ("asset",))

    def test_an_allowed_file_is_the_only_thing_the_sprint_may_change(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "kept.cs").write_text("changed\n", encoding="utf-8")
            (root / "allowed.cs").write_text("changed\n", encoding="utf-8")
            with patch.object(gate.subprocess, "check_output",
                              return_value="kept.cs\nallowed.cs\n"), \
                    patch.object(gate, "blob", return_value=b"original\n"):
                with self.assertRaisesRegex(
                        AssertionError, "Accepted production changed: kept.cs"):
                    gate.preserved_files(root, "accepted", ("kept", "allowed"),
                                         allowed=("allowed.cs",))
                self.assertEqual(
                    0, gate.preserved_files(root, "accepted",
                                            ("kept", "allowed"),
                                            allowed=("kept.cs", "allowed.cs")))

    def test_the_released_master_and_summoning_trees_are_exact(self):
        self.assertGreater(
            gate.preserved_files(ROOT, gate.MASTER, gate.MASTER_PROTECTED), 40)
        self.assertGreater(
            gate.preserved_files(ROOT, gate.MASTER, gate.SUMMONING_PROTECTED,
                                 allowed=gate.SPRINT18_CHANGED), 80)


if __name__ == "__main__":
    unittest.main()
