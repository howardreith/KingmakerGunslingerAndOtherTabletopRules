"""Reject identity, suppression, icon, body and binary drift at the Sprint 19 seam.

These are corruption fixtures for the Sprint 19 gate itself: each one proves
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

import validate_expanded_summoning_sprint19148 as gate
from validate_repository import VALIDATORS

ROOT = Path(__file__).resolve().parents[1]


class Sprint19Tests(unittest.TestCase):
    def test_version_dispatch_and_historical_dispatch_are_intact(self):
        self.assertEqual("validate_expanded_summoning_sprint19148.py",
                         VALIDATORS["0.0.148"])
        self.assertEqual("validate_expanded_summoning_sprint18147.py",
                         VALIDATORS["0.0.147"])
        self.assertEqual("validate_expanded_summoning_checkpoint146.py",
                         VALIDATORS["0.0.146"])

    def test_the_append_is_exactly_twenty_two_identities(self):
        self.assertEqual(3005, gate.validate_identity_append(ROOT))

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
            "symbol": "KMG.Summoning.Special.Xill.Implant",
            "guid": "0" * 32, "plannedType": "BlueprintFeature",
            "status": "active", "milestone": "Expanded Summoning", "notes": ""})
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError,
                                        "exactly 22 identities"):
                gate.validate_identity_append(ROOT)

    def test_an_identity_outside_the_two_creatures_is_rejected(self):
        """Sprint 19 may allocate for the Girallon and the Xill and nothing else.

        The Sprint 20 Giant Scorpion branches from this sprint's release, so
        the way this could realistically go wrong is a scorpion identity
        arriving a sprint early.
        """
        current = gate.document(ROOT, "blueprints/blueprints.json")
        altered = copy.deepcopy(current)
        # Rename one of Sprint 19's own appended identities rather than
        # adding a twenty-third, so the count check cannot fire first and
        # the fixture really does exercise the scope check.
        renamed = dict(altered["entries"][-1])
        renamed["symbol"] = "KMG.Summoning.Special.GiantScorpion.Sting"
        altered["entries"][-1] = renamed
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "outside its two creatures"):
                gate.validate_identity_append(ROOT)

    def test_a_templated_sprint19_creature_is_rejected(self):
        """Ten roots, not thirty.

        Templating either creature would allocate twenty celestial and
        fiendish execution children nobody reviewed, and the printed tables
        template neither.
        """
        current = gate.document(ROOT, "blueprints/blueprints.json")
        altered = copy.deepcopy(current)
        for index in range(20):
            altered["entries"].append({
                "symbol": "KMG.Summoning.Ability.SM.Tier5.Xill.Celestial%d" % index,
                "guid": ("%032x" % (0xabc0000 + index)),
                "plannedType": "BlueprintAbility", "status": "active",
                "milestone": "Expanded Summoning", "notes": ""})
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError, "exactly 22 identities"):
                gate.validate_identity_append(ROOT)

    def test_leaving_a_sprint19_creature_withheld_is_rejected(self):
        """Publication is all ten roots or none of them."""
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / "src/KingmakerGunslinger/Summoning"
            source.mkdir(parents=True)
            (source / "SummonVisibilityCatalog.cs").write_text(
                "RegisteredLogicalPlacementCount = 1044;\n"
                "SuppressedLogicalPlacementCount = 10;\n"
                'SuppressedCreatureKeys = new HashSet<string>(new[] { '
                '"girallon", "xill" }, StringComparer.Ordinal);\n',
                encoding="utf-8")
            with self.assertRaisesRegex(
                    AssertionError, "did not remove its two keys: girallon"):
                gate.validate_suppression(root)

    def test_withholding_a_released_ape_is_rejected(self):
        """Sprint 18 shipped. Neither ape may be withheld to make room."""
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / "src/KingmakerGunslinger/Summoning"
            source.mkdir(parents=True)
            (source / "SummonVisibilityCatalog.cs").write_text(
                "RegisteredLogicalPlacementCount = 1044;\n"
                "SuppressedLogicalPlacementCount = 12;\n"
                'SuppressedCreatureKeys = new HashSet<string>(new[] { '
                '"girallon", "xill", "dire-ape" }, StringComparer.Ordinal);\n',
                encoding="utf-8")
            with self.assertRaisesRegex(
                    AssertionError, "withheld a creature v0.0.147 published"):
                gate.validate_suppression(root)

    def test_a_changed_placement_contract_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / "src/KingmakerGunslinger/Summoning"
            source.mkdir(parents=True)
            (source / "SummonVisibilityCatalog.cs").write_text(
                "RegisteredLogicalPlacementCount = 1044;\n"
                "SuppressedLogicalPlacementCount = 0;\n"
                "new HashSet<string>(new string[0], StringComparer.Ordinal)\n",
                encoding="utf-8")
            # A Girallon on the Summon Monster table, which no printed table
            # puts it on, and a roster that did not grow.
            (source / "ExpandedSummoningCatalog.cs").write_text(
                'C("girallon","Girallon",5,false,5,"Troll"),\n'
                'C("xill","Xill",5,false,null,"Troll"),\n'
                'C("ape","Ape",3,true,3,"Troll"),\n'
                'C("dire-ape","Dire Ape",4,true,4,"Troll"),\n'
                "ValidateFamily(SummonFamily.Monster, 90, 519)\n"
                "ValidateFamily(SummonFamily.NaturesAlly, 88, 515)\n",
                encoding="utf-8")
            with self.assertRaisesRegex(
                    AssertionError, "placement contract differs"):
                gate.validate_suppression(root)

    def test_withholding_any_other_creature_is_rejected(self):
        """Publication means nothing is withheld, not nothing of ours is."""
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / "src/KingmakerGunslinger/Summoning"
            source.mkdir(parents=True)
            (source / "SummonVisibilityCatalog.cs").write_text(
                "RegisteredLogicalPlacementCount = 1044;\n"
                "SuppressedLogicalPlacementCount = 0;\n"
                'SuppressedCreatureKeys = new HashSet<string>(new[] { '
                '"salamander" }, StringComparer.Ordinal);\n',
                encoding="utf-8")
            with self.assertRaisesRegex(
                    AssertionError, "nothing may be"):
                gate.validate_suppression(root)

    def test_a_body_the_build_output_does_not_require_is_rejected(self):
        """A builder hand copy must not be the only thing that ships a body."""
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            shipped = root / "assets/sprint19-fourarmed"
            shipped.mkdir(parents=True)
            (shipped / "girallon-mesh.json").write_text("{}", encoding="utf-8")
            (shipped / "girallon-albedo.png").write_bytes(b"png")
            scripts = root / "scripts"
            scripts.mkdir()
            # The builder copies both; the output validator requires only
            # the mesh, which is how a painting ships from a local build and
            # vanishes from a clean one.
            (scripts / "Build-Local.ps1").write_text(
                "sprint19-fourarmed girallon", encoding="utf-8")
            (scripts / "validate-build-output.ps1").write_text(
                "'assets\\sprint19-fourarmed\\girallon-mesh.json'", encoding="utf-8")
            with self.assertRaisesRegex(
                    AssertionError, "not required of the build output"):
                gate.validate_shipped_bodies_reach_the_release_build(root)

    def test_one_painting_cannot_serve_both_creatures(self):
        manifest = gate.document(
            ROOT,
            "assets-source/original-icons/expanded-summoning/icon-manifest.json")
        altered = copy.deepcopy(manifest)
        rows = {row["key"]: row for row in altered["icons"]}
        rows["xill"]["outputSha256"] = rows["girallon"]["outputSha256"]
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "must not share one painting"):
                gate.validate_icons(ROOT)

    def test_a_withheld_creature_still_needs_its_own_icon(self):
        manifest = gate.document(
            ROOT,
            "assets-source/original-icons/expanded-summoning/icon-manifest.json")
        altered = copy.deepcopy(manifest)
        altered["icons"] = [row for row in altered["icons"]
                            if row["key"] != "xill"]
        altered["count"] = len(altered["icons"])
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError, "must be 113"):
                gate.validate_icons(ROOT)

    def test_the_frozen_four_claw_rend_cannot_drift(self):
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT19-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["girallon"]["printedProfile"]["specialAttacks"][0]["trigger"] = \
            "two claws hit the same target"
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "printed Girallon rend line changed"):
                gate.validate_contract(ROOT)

    def test_the_frozen_xill_profile_cannot_drift(self):
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT19-CONTRACT.json")
        altered = copy.deepcopy(contract)
        # Dropping the carried shield bonus is the realistic way this one
        # goes wrong, and it reads as armour class 19.
        altered["xill"]["printedProfile"]["armorClass"] = 19
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "printed Xill profile changed"):
                gate.validate_contract(ROOT)

    def test_the_frozen_arithmetic_cannot_drift(self):
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT19-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["placement"]["surfaceAfter"]["visibleChoicesAfterPublication"] = 1063
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError, "arithmetic drifted"):
                gate.validate_contract(ROOT)

    def test_a_body_claiming_four_driver_chains_is_rejected(self):
        """The authored limitation is the point, so it cannot be overstated.

        A mesh declaring four independent chains would be claiming a rig this
        project did not build, and the runtime would rebody a creature whose
        lower arms do not move with anything.
        """
        for key, field, value in (("girallon", "armDriverChains", 4),
                                  ("xill", "lowerArmsShareUpperArmDrivers", False),
                                  ("girallon", "visibleArms", 2),
                                  ("xill", "printedSize", "Large")):
            body = gate.document(
                ROOT, "assets/sprint19-fourarmed/" + key + "-mesh.json")
            altered = copy.deepcopy(body)
            altered[field] = value
            real = gate.document

            def fake(root, path, _key=key, _altered=altered):
                if path.endswith(_key + "-mesh.json"):
                    return _altered
                return real(root, path)

            with patch.object(gate, "document", side_effect=fake):
                with self.assertRaisesRegex(
                        AssertionError, "anatomy contract broken: " + key):
                    gate.validate(ROOT)

    def test_a_body_weighting_an_empty_branch_is_rejected(self):
        body = gate.document(ROOT, "assets/sprint19-fourarmed/girallon-mesh.json")
        altered = copy.deepcopy(body)
        altered["bones"] = altered["bones"][:-1] + ["Tail_01"]
        real = gate.document

        def fake(root, path, _altered=altered):
            if path.endswith("girallon-mesh.json"):
                return _altered
            return real(root, path)

        with patch.object(gate, "document", side_effect=fake):
            with self.assertRaisesRegex(
                    AssertionError, "deliberately empty branch"):
                gate.validate(ROOT)

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
                                 allowed=gate.SPRINT19_CHANGED), 80)

    def test_the_shipped_sprint18_bodies_are_inside_the_boundary(self):
        """Sprint 18's four body files are protected now, not editable."""
        self.assertIn("assets/sprint18-primates/", gate.SUMMONING_PROTECTED)
        for path in gate.SPRINT19_CHANGED:
            self.assertFalse(path.startswith("assets/sprint18-primates/"),
                             "Sprint 19 may not change a shipped Sprint 18 body")


if __name__ == "__main__":
    unittest.main()
