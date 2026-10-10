"""Reject identity, suppression, contract and build-reachability drift at the
Sprint 20 seam.

These are corruption fixtures for the Sprint 20 gate itself: each one proves
the validator rejects a specific way this sprint could go wrong, so a passing
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

import validate_expanded_summoning_sprint20149 as gate
from validate_repository import VALIDATORS

ROOT = Path(__file__).resolve().parents[1]


class Sprint20Tests(unittest.TestCase):
    def test_version_dispatch_and_historical_dispatch_are_intact(self):
        self.assertEqual("validate_expanded_summoning_sprint20149.py",
                         VALIDATORS["0.0.149"])
        self.assertEqual("validate_expanded_summoning_sprint19148.py",
                         VALIDATORS["0.0.148"])
        self.assertEqual("validate_expanded_summoning_sprint18147.py",
                         VALIDATORS["0.0.147"])

    def test_the_append_is_exactly_thirty_two_identities(self):
        self.assertEqual(3037, gate.validate_identity_append(ROOT))

    def test_the_identity_arithmetic_is_stated_rather_than_a_magic_number(self):
        """Thirty-two is a derivation, and it must stay one.

        It was thirty-one until the grab got a carrier. The creature printed
        grab from its first commit - in its profile's attribution line, in its
        +12 grapple derivation and in the registration commit's own message -
        and carried nothing that grabs, so the tail is counted here rather
        than written down.
        """
        self.assertEqual(12, gate.SPRINT20_ROOTS)
        self.assertEqual(12, gate.SPRINT20_EXECUTION_CHILDREN)
        self.assertEqual(6, gate.SPRINT20_MONSTER_ROOTS)
        self.assertEqual(7, len(gate.SPRINT20_IDENTITY_TAIL))
        self.assertEqual(32, gate.SPRINT20_IDENTITIES)
        self.assertEqual(1 + 12 + 12 + len(gate.SPRINT20_IDENTITY_TAIL),
                         gate.SPRINT20_IDENTITIES)
        self.assertIn("KMG.Summoning.Special.GiantScorpion.Traits",
                      gate.SPRINT20_IDENTITY_TAIL)

    def test_a_grab_claimed_in_prose_with_no_carrier_is_rejected(self):
        """What actually went wrong, held to a fixture.

        Grab is a printed claw rider and the whole of the difference between
        the printed CMB +8 and grapple +12. Three places said this creature
        had it: its profile's attribution line, that derivation, and the
        registration commit's message. None of them is the thing that grabs
        and none of them would have failed.

        A released file identical to the working tree is exactly the shape of
        that defect - nothing was added, so nothing was wired - and it is the
        one case the check must refuse rather than pass quietly.
        """
        unchanged = (ROOT / "src/KingmakerGunslinger/Blueprints"
                     / "ExpandedSummoningSpecialBuilder.cs").read_bytes()

        def already_released(root, ref, path):
            return unchanged
        with patch.object(gate, "blob", side_effect=already_released):
            with self.assertRaisesRegex(AssertionError,
                                        "not wired in the special builder"):
                gate.validate_the_grab_is_the_only_addition(ROOT)

    def test_a_grab_that_reaches_the_sting_is_rejected(self):
        """One limb too far and the sting grabs.

        The spec counts limbs rather than naming weapons, so the whole of what
        keeps grab off the sting is that the count stops at the second claw.
        A spec reading ClawCount rather than ClawCount - 1 would reach the
        sting, and nothing else about the creature would look wrong.
        """
        released = (ROOT / "src/KingmakerGunslinger/Blueprints"
                    / "ExpandedSummoningSpecialBuilder.cs").read_text(
                        encoding="utf-8")
        overreaching = released.replace(
            "Additional = GiantScorpionRulesPolicy.ClawCount - 1",
            "Additional = GiantScorpionRulesPolicy.ClawCount")
        self.assertNotEqual(released, overreaching)
        # The released side stays the real one, so the block reads as this
        # sprint's own addition exactly as it does in a real run.
        with patch.object(gate.Path, "read_text",
                          lambda self, **kw: overreaching):
            with self.assertRaisesRegex(AssertionError,
                                        "grab spec is missing"):
                gate.validate_the_grab_is_the_only_addition(ROOT)

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
            "symbol": "KMG.Summoning.Natural.GiantScorpion.Tremorsense",
            "guid": "0" * 32, "plannedType": "BlueprintFeature",
            "status": "active", "milestone": "Expanded Summoning", "notes": ""})
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError,
                                        "exactly 32 identities"):
                gate.validate_identity_append(ROOT)

    def test_an_identity_outside_the_one_creature_is_rejected(self):
        """Sprint 21 branches from this sprint's release.

        The realistic way this goes wrong is a Bebelith or Giant Crab identity
        arriving a sprint early, so the fixture renames one of Sprint 20's own
        appended entries rather than adding a thirty-second: that way the
        count check cannot fire first and the scope check is really exercised.
        """
        current = gate.document(ROOT, "blueprints/blueprints.json")
        altered = copy.deepcopy(current)
        renamed = dict(altered["entries"][-1])
        renamed["symbol"] = "KMG.Summoning.Special.Bebelith.WebEscape"
        altered["entries"][-1] = renamed
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "outside its one creature"):
                gate.validate_identity_append(ROOT)

    def test_an_untemplated_scorpion_is_rejected(self):
        """Dropping the template would silently halve the identity cost.

        Twelve of the thirty-one entries are celestial and fiendish children.
        A scorpion registered untemplated would still pass a naive count check
        if twelve other identities appeared, so this fixture removes exactly
        the children and proves the structural check fires.
        """
        current = gate.document(ROOT, "blueprints/blueprints.json")
        altered = copy.deepcopy(current)
        altered["entries"] = [
            e for e in altered["entries"]
            if not (e["symbol"].endswith(".Celestial") or
                    e["symbol"].endswith(".Fiendish"))
            or "GiantScorpion" not in e["symbol"]]
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError,
                                        "exactly 32 identities"):
                gate.validate_identity_append(ROOT)

    def test_a_templated_natures_ally_root_is_rejected(self):
        """Summon Nature's Ally never templates.

        A celestial Nature's Ally scorpion would be a rule this project does
        not have, so the fixture moves one child onto the Nature's Ally table
        and keeps the total at thirty-one.
        """
        current = gate.document(ROOT, "blueprints/blueprints.json")
        altered = copy.deepcopy(current)
        for index, entry in enumerate(altered["entries"]):
            if "GiantScorpion" in entry["symbol"] and \
                    entry["symbol"].endswith(".Celestial"):
                moved = dict(entry)
                moved["symbol"] = entry["symbol"].replace(".SM.", ".SNA.")
                altered["entries"][index] = moved
                break
        else:
            self.fail("no celestial child to move")
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "Summon Nature's Ally never templates"):
                gate.validate_identity_append(ROOT)

    def test_publishing_the_scorpion_early_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / "src/KingmakerGunslinger/Summoning"
            source.mkdir(parents=True)
            (source / "SummonVisibilityCatalog.cs").write_text(
                "RegisteredLogicalPlacementCount = 1056;\n"
                "SuppressedLogicalPlacementCount = 0;\n"
                "SuppressedCreatureKeys = new HashSet<string>(new string[0], "
                "StringComparer.Ordinal);\n",
                encoding="utf-8")
            with self.assertRaisesRegex(
                    AssertionError, "register its creature withheld"):
                gate.validate_suppression(root)

    def test_withholding_a_released_creature_is_rejected(self):
        """v0.0.148 published both apes and both Sprint 19 creatures."""
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / "src/KingmakerGunslinger/Summoning"
            source.mkdir(parents=True)
            (source / "SummonVisibilityCatalog.cs").write_text(
                "RegisteredLogicalPlacementCount = 1056;\n"
                "SuppressedLogicalPlacementCount = 17;\n"
                'SuppressedCreatureKeys = new HashSet<string>(new[] { '
                '"giant-scorpion", "girallon" }, StringComparer.Ordinal);\n',
                encoding="utf-8")
            with self.assertRaisesRegex(
                    AssertionError, "withheld a creature v0.0.148 published"):
                gate.validate_suppression(root)

    def test_a_changed_placement_contract_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / "src/KingmakerGunslinger/Summoning"
            source.mkdir(parents=True)
            (source / "SummonVisibilityCatalog.cs").write_text(
                "RegisteredLogicalPlacementCount = 1056;\n"
                "SuppressedLogicalPlacementCount = 12;\n"
                'SuppressedCreatureKeys = new HashSet<string>(new[] { '
                '"giant-scorpion" }, StringComparer.Ordinal);\n',
                encoding="utf-8")
            # A tier-3 scorpion, which no printed table puts it on, and a
            # roster that did not grow.
            (source / "ExpandedSummoningCatalog.cs").write_text(
                'C("giant-scorpion","Giant Scorpion",3,true,3,"Giant Spider"),\n'
                'C("girallon","Girallon",null,false,5,"Troll"),\n'
                'C("xill","Xill",5,false,null,"Troll"),\n'
                'C("ape","Ape",3,true,3,"Troll"),\n'
                'C("dire-ape","Dire Ape",4,true,4,"Troll"),\n'
                "ValidateFamily(SummonFamily.Monster, 91, 524)\n"
                "ValidateFamily(SummonFamily.NaturesAlly, 89, 520)\n",
                encoding="utf-8")
            with self.assertRaisesRegex(
                    AssertionError, "placement contract differs"):
                gate.validate_suppression(root)

    def test_the_frozen_printed_profile_cannot_drift(self):
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT20-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["printedProfile"]["hitPoints"]["total"] = 45
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError, "printed Giant "
                                        "Scorpion profile changed"):
                gate.validate_contract(ROOT)

    def test_the_frozen_poison_graph_cannot_drift(self):
        """Six rounds is the whole reason this poison needs its own carrier."""
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT20-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["printedProfile"]["poison"]["frequency"] = "1/round for 4 rounds"
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError,
                                        "printed poison graph changed"):
                gate.validate_contract(ROOT)

    def test_the_frozen_trip_defence_cannot_drift(self):
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT20-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["printedProfile"]["combatManeuverDefense"]["versusTrip"] = 23
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError,
                                        "eight-legged trip defence changed"):
                gate.validate_contract(ROOT)

    def test_the_frozen_arithmetic_cannot_drift(self):
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT20-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["placement"]["executionChildren"] = 24
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError, "arithmetic drifted"):
                gate.validate_contract(ROOT)

    def test_quietly_implementing_an_omission_is_rejected(self):
        """An omission that stops being recorded has stopped being honest."""
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT20-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["honestOmissions"] = [
            row for row in altered["honestOmissions"]
            if row["id"] != "PER_LIMB_REACH_UNREPRESENTED"]
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "stopped recording an omission"):
                gate.validate_contract(ROOT)

    def test_a_body_the_build_output_does_not_require_is_rejected(self):
        """A builder hand copy must not be the only thing that ships a body."""
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            shipped = root / "assets/sprint20-arachnids"
            shipped.mkdir(parents=True)
            (shipped / "giant-scorpion-mesh.json").write_text(
                "{}", encoding="utf-8")
            scripts = root / "scripts"
            scripts.mkdir()
            scripts.joinpath("Build-Local.ps1").write_text(
                "sprint20-arachnids giant", encoding="utf-8")
            scripts.joinpath("validate-build-output.ps1").write_text(
                "nothing required here", encoding="utf-8")
            with self.assertRaisesRegex(
                    AssertionError, "not required of the build output"):
                gate.validate_shipped_bodies_reach_the_release_build(root)

    def test_the_released_master_and_summoning_trees_are_exact(self):
        self.assertGreater(
            gate.preserved_files(ROOT, gate.MASTER, gate.MASTER_PROTECTED), 0)
        self.assertGreater(
            gate.preserved_files(ROOT, gate.MASTER, gate.SUMMONING_PROTECTED,
                                 allowed=gate.SPRINT20_CHANGED), 0)

    def test_the_shipped_sprint19_bodies_are_inside_the_boundary(self):
        """Sprint 19's four body files are protected now, not editable."""
        self.assertIn("assets/sprint19-fourarmed/", gate.SUMMONING_PROTECTED)
        for path in gate.SPRINT20_CHANGED:
            self.assertFalse(path.startswith("assets/sprint19-fourarmed/"), path)

    def test_a_new_file_the_release_already_had_is_rejected(self):
        for path in gate.SPRINT20_NEW:
            self.assertNotIn(path, gate.tracked(ROOT, gate.MASTER), path)


if __name__ == "__main__":
    unittest.main(verbosity=2)
