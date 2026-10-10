"""Reject identity, suppression, contract and Bebelith drift at the Sprint 21 seam.

These are corruption fixtures for the Sprint 21 gate itself: each one proves the
validator rejects a specific way this sprint could go wrong, so a passing gate
means something.

Two of them exist because Sprint 20 shipped the defect they describe. A printed
line can have a closing derivation and no carrier, and neither the arithmetic
nor a registered identity will notice - only a live creature will, which is how
a Giant Scorpion came to read CMD 27 against a printed 31 and Perception 0
against a printed +4. One of them exists because this creature is the first in
the series on one table, so the structural mistake available to it is one no
earlier sprint could make: owing execution children it has not allocated.
"""
import copy
import json
import tempfile
import unittest
import sys
from pathlib import Path
from unittest.mock import patch

sys.dont_write_bytecode = True

import validate_expanded_summoning_sprint21150 as gate
from validate_repository import VALIDATORS

ROOT = Path(__file__).resolve().parents[1]


class Sprint21Tests(unittest.TestCase):
    def test_version_dispatch_and_historical_dispatch_are_intact(self):
        self.assertEqual("validate_expanded_summoning_sprint21150.py",
                         VALIDATORS["0.0.150"])
        self.assertEqual("validate_expanded_summoning_sprint20149.py",
                         VALIDATORS["0.0.149"])
        self.assertEqual("validate_expanded_summoning_sprint19148.py",
                         VALIDATORS["0.0.148"])

    def test_the_append_is_exactly_twentytwo_identities(self):
        self.assertEqual(3061, gate.validate_identity_append(ROOT))

    def test_the_identity_arithmetic_is_stated_rather_than_a_magic_number(self):
        """Twenty-two is a derivation, and it must stay one.

        For the crab: one unit, seven placements, five facts, and - uniquely
        in this series - no execution children at all. For the Bebelith: nine
        facts and nothing else, because its unit and its three roots were
        allocated when it shipped and this sprint changes only what stands
        behind them.
        """
        self.assertEqual(7, gate.SPRINT21_ROOTS)
        self.assertEqual(0, gate.SPRINT21_MONSTER_ROOTS)
        self.assertEqual(0, gate.SPRINT21_EXECUTION_CHILDREN)
        self.assertEqual(5, len(gate.SPRINT21_IDENTITY_TAIL))
        self.assertEqual(9, len(gate.BEBELITH_IDENTITY_TAIL))
        self.assertEqual(22, gate.SPRINT21_IDENTITIES)
        self.assertEqual(1 + 7 + 0 + len(gate.SPRINT21_IDENTITY_TAIL)
                         + len(gate.BEBELITH_IDENTITY_TAIL),
                         gate.SPRINT21_IDENTITIES)
        for required in ("KMG.Summoning.Natural.GiantCrab.UnitType",
                         "KMG.Summoning.Natural.GiantCrab.MindlessImmunity",
                         "KMG.Summoning.Natural.GiantCrab.TripDefense",
                         "KMG.Summoning.Natural.GiantCrab.RacialSkills",
                         "KMG.Summoning.Special.GiantCrab.Traits"):
            self.assertIn(required, gate.SPRINT21_IDENTITY_TAIL)
        # Rot needs two - a carrier and the victim-owned state that outlives
        # the summon - and the web needs four, because an ability nothing
        # casts is an ability the creature does not have.
        for required in ("KMG.Summoning.Special.Bebelith.Rot",
                         "KMG.Summoning.Special.Bebelith.RotState",
                         "KMG.Summoning.Special.Bebelith.PenetratingStrike",
                         "KMG.Summoning.Special.Bebelith.TripDefense",
                         "KMG.Summoning.Special.Bebelith.RacialSkills",
                         "KMG.Summoning.Special.Bebelith.Web",
                         "KMG.Summoning.Special.Bebelith.WebResource",
                         "KMG.Summoning.Special.Bebelith.WebAi",
                         "KMG.Summoning.Special.Bebelith.Brain"):
            self.assertIn(required, gate.BEBELITH_IDENTITY_TAIL)
        # And the released four are NOT appended. Re-appending one would mean
        # the creature had been reallocated rather than overhauled.
        for released in gate.BEBELITH_RELEASED_IDENTITIES:
            self.assertNotIn(released, gate.SPRINT21_IDENTITY_TAIL)
            self.assertNotIn(released, gate.BEBELITH_IDENTITY_TAIL)
        # No weapon. A Medium creature takes the shared native 1d4 claw
        # unscaled, which is why the Large Sprint 18 and 20 creatures had to
        # own theirs and this one does not.
        self.assertFalse(any("Claw" in symbol or "Bite" in symbol
                             for symbol in gate.SPRINT21_IDENTITY_TAIL))

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
            "symbol": "KMG.Summoning.Natural.GiantCrab.Surprise",
            "guid": "f" * 32, "plannedType": "BlueprintFeature",
            "status": "active", "milestone": "Expanded Summoning",
            "notes": "unreviewed"})
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError,
                                        "exactly 22 identities"):
                gate.validate_identity_append(ROOT)

    def test_an_identity_outside_the_one_creature_is_rejected(self):
        current = gate.document(ROOT, "blueprints/blueprints.json")
        altered = copy.deepcopy(current)
        altered["entries"][-1] = dict(altered["entries"][-1],
                                      symbol="KMG.Summoning.Natural.Lobster.Claw")
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "outside its two creatures"):
                gate.validate_identity_append(ROOT)

    def test_publishing_the_crab_early_is_rejected(self):
        """Publication is earned by a passing review, not by an edit."""
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / "src/KingmakerGunslinger/Summoning"
            source.mkdir(parents=True)
            (source / "SummonVisibilityCatalog.cs").write_text(
                "RegisteredLogicalPlacementCount = 1063;\n"
                "SuppressedLogicalPlacementCount = 0;\n"
                "SuppressedCreatureKeys = new HashSet<string>("
                "Array.Empty<string>(), StringComparer.Ordinal);\n",
                encoding="utf-8")
            with self.assertRaisesRegex(
                    AssertionError, "register its creature withheld"):
                gate.validate_suppression(root)

    def test_withholding_a_released_creature_is_rejected(self):
        """v0.0.149 published the apes, the Sprint 19 pair and the scorpion."""
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / "src/KingmakerGunslinger/Summoning"
            source.mkdir(parents=True)
            (source / "SummonVisibilityCatalog.cs").write_text(
                "RegisteredLogicalPlacementCount = 1063;\n"
                "SuppressedLogicalPlacementCount = 19;\n"
                'SuppressedCreatureKeys = new HashSet<string>(new[] { '
                '"giant-crab", "giant-scorpion" }, StringComparer.Ordinal);\n',
                encoding="utf-8")
            with self.assertRaisesRegex(
                    AssertionError, "withheld a creature v0.0.149 published"):
                gate.validate_suppression(root)

    def test_withholding_the_released_bebelith_is_rejected(self):
        """Its body is this sprint's scope; its publication is not."""
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / "src/KingmakerGunslinger/Summoning"
            source.mkdir(parents=True)
            (source / "SummonVisibilityCatalog.cs").write_text(
                "RegisteredLogicalPlacementCount = 1063;\n"
                "SuppressedLogicalPlacementCount = 10;\n"
                'SuppressedCreatureKeys = new HashSet<string>(new[] { '
                '"giant-crab", "bebelith" }, StringComparer.Ordinal);\n',
                encoding="utf-8")
            with self.assertRaisesRegex(
                    AssertionError, "withheld a creature v0.0.149 published"):
                gate.validate_suppression(root)

    def test_a_templated_crab_is_rejected(self):
        """The structural mistake only this creature could make.

        Summon Nature's Ally never templates and this creature is not on the
        Summon Monster table, so it owns no celestial or fiendish execution
        children. A catalog entry that templated it would owe fourteen
        identities this sprint has not allocated, and the ledger check would
        then fail for a reason that did not name the cause.
        """
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / "src/KingmakerGunslinger/Summoning"
            source.mkdir(parents=True)
            (source / "SummonVisibilityCatalog.cs").write_text(
                "RegisteredLogicalPlacementCount = 1063;\n"
                "SuppressedLogicalPlacementCount = 7;\n"
                'SuppressedCreatureKeys = new HashSet<string>(new[] { '
                '"giant-crab" }, StringComparer.Ordinal);\n',
                encoding="utf-8")
            (source / "ExpandedSummoningCatalog.cs").write_text(
                'C("giant-crab","Giant Crab",null,true,3,"Giant Spider"),\n'
                'C("giant-scorpion","Giant Scorpion",4,true,4,"Giant Spider"),\n'
                'C("girallon","Girallon",null,false,5,"Troll"),\n'
                'C("xill","Xill",5,false,null,"Troll"),\n'
                'C("ape","Ape",3,true,3,"Troll"),\n'
                'C("dire-ape","Dire Ape",4,true,4,"Troll"),\n'
                "ValidateFamily(SummonFamily.Monster, 92, 530)\n"
                "ValidateFamily(SummonFamily.NaturesAlly, 91, 533)\n",
                encoding="utf-8")
            with self.assertRaisesRegex(AssertionError, "never templates"):
                gate.validate_suppression(root)

    def test_the_frozen_printed_profile_cannot_drift(self):
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT21-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["giantCrab"]["printedProfile"]["hitPoints"]["total"] = 25
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError,
                                        "printed Giant Crab profile changed"):
                gate.validate_contract(ROOT)

    def test_the_frozen_trip_defence_cannot_drift(self):
        """Twelve is the whole reason this creature owns a carrier."""
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT21-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["giantCrab"]["printedProfile"]["combatManeuverDefense"][
            "versusTrip"] = 23
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "eight-legged trip defence changed"):
                gate.validate_contract(ROOT)

    def test_a_swim_speed_folded_into_the_ground_speed_is_rejected(self):
        """The boundary this creature exists to test.

        The printed swim speed is recorded and unrepresented. Folding it into
        the ground speed would turn a declared omission into a silent
        substitution, and a player would get a faster land creature than the
        book prints rather than a land creature missing a swim speed.
        """
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT21-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["giantCrab"]["printedProfile"]["swimSpeedFeet"] = 30
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError, "aquatic line changed"):
                gate.validate_contract(ROOT)

    def test_dropping_an_omission_from_the_contract_is_rejected(self):
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT21-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["giantCrab"]["acceptedLimitations"] = [
            row for row in altered["giantCrab"]["acceptedLimitations"]
            if row["id"] != "ORDINARY_MAP_LAND_USE_SCOPE"]
        altered["bebelith"]["acceptedLimitations"] = [
            row for row in altered["bebelith"]["acceptedLimitations"]
            if row["id"] != "ORDINARY_MAP_LAND_USE_SCOPE"]
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "stopped recording an omission"):
                gate.validate_contract(ROOT)

    def test_dropping_the_aquatic_boundary_is_rejected(self):
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT21-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["hardBoundaries"] = [row for row in altered["hardBoundaries"]
                                     if "aquatic" not in row.lower()]
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "stopped forbidding: aquatic"):
                gate.validate_contract(ROOT)

    def test_a_contract_that_drops_the_bebelith_roots_is_rejected(self):
        """Three roots at tiers seven, eight and nine, published.

        The Bebelith's implementation is this sprint's work and its identity
        is not. A contract that stopped declaring the roots preserved would
        let a save made against v0.0.149 stop resolving them.
        """
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT21-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["bebelith"]["releasedIdentity"]["roots"] = \
            altered["bebelith"]["releasedIdentity"]["roots"][:2]
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "released identity preserved"):
                gate.validate_contract(ROOT)

    def test_renaming_the_released_bebelith_is_rejected(self):
        """A source may print Bebilith. Renaming the shipped key is a save
        and user-interface migration, not a creature correction, and the
        frozen contract has to keep saying so."""
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT21-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["spelling"]["key"] = "bebilith"
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(AssertionError, "spelling"):
                gate.validate_contract(ROOT)

    def test_penetrating_strike_leaking_onto_non_demons_is_rejected(self):
        """The asymmetry a live review exists to catch.

        Cold iron and good are granted against demons only. Granting them
        universally is invisible against a demon and wrong against everything
        else, so the prohibition has to stay in the frozen contract.
        """
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT21-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["bebelith"]["penetratingStrike"]["prohibitions"] = [
            row for row in
            altered["bebelith"]["penetratingStrike"]["prohibitions"]
            if "non-demon" not in row]
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "penetrating strike contract changed"):
                gate.validate_contract(ROOT)

    def test_dropping_a_dismantle_safety_clause_is_rejected(self):
        """The highest-risk mechanic in the sprint, and the list that bounds
        it. Any one of these clauses going missing is the difference between
        a wearer-side effect and a mutated inventory."""
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT21-CONTRACT.json")
        for clause in ("never owner party gear", "unarmoured target",
                       "permanent inventory corruption"):
            altered = copy.deepcopy(contract)
            altered["bebelith"]["dismantleArmor"]["mandatorySafety"] = [
                row for row in
                altered["bebelith"]["dismantleArmor"]["mandatorySafety"]
                if clause not in row]
            with patch.object(gate, "document", return_value=altered):
                with self.assertRaisesRegex(
                        AssertionError, "safety list dropped"):
                    gate.validate_contract(ROOT)

    def test_inventing_an_item_durability_system_is_rejected(self):
        """Kingmaker exposes no item-durability component to a mod, so the
        only way to make Dismantle Armor destroy an item literally is to
        build one. The frozen contract forbids it and must keep doing so."""
        contract = gate.document(
            ROOT, "planning/EXPANDED-SUMMONING-SPRINT21-CONTRACT.json")
        altered = copy.deepcopy(contract)
        altered["bebelith"]["dismantleArmor"]["engineGapPolicy"] = \
            "Build whatever the printed effect needs."
        with patch.object(gate, "document", return_value=altered):
            with self.assertRaisesRegex(
                    AssertionError, "item-durability system"):
                gate.validate_contract(ROOT)

    def test_a_moved_released_bebelith_identity_is_rejected(self):
        """The easiest way for this sprint to go wrong.

        Its implementation is this sprint's work, which is why the released
        comparison that an earlier draft of this gate made line by line is
        gone. What replaces it is narrower and is the thing that actually
        must not move: the unit and the three roots keep the exact GUIDs they
        shipped with, so a save made against v0.0.149 still resolves them.
        Reallocating one would be a save and user-interface migration.
        """
        ledger = gate.document(ROOT, "blueprints/blueprints.json")
        moved = copy.deepcopy(ledger)
        for entry in moved["entries"]:
            if entry["symbol"] == "KMG.Summoning.Unit.Bebelith":
                entry["guid"] = "0" * 32
                break
        else:
            self.fail("The released Bebelith unit is not in the ledger.")
        with patch.object(gate, "document", return_value=moved):
            with self.assertRaisesRegex(
                    AssertionError, "moved a released Bebelith identity"):
                gate.validate_the_released_bebelith_identity_is_preserved(ROOT)

    def test_an_unwired_carrier_is_rejected(self):
        """What Sprint 20 shipped twice.

        Both of this creature's own carriers are printed lines whose
        arithmetic closes without them, so a registered identity and a closed
        derivation prove nothing. This is the check that does.
        """
        builder = (ROOT / "src/KingmakerGunslinger/Blueprints"
                   / "ExpandedSummoningNaturalBuilder.cs").read_text(
                       encoding="utf-8-sig")
        unwired = builder.replace(
            "defence.Bonus = GiantCrabRulesPolicy.EightLegTripBonus",
            "defence.Bonus = 8")
        self.assertNotEqual(builder, unwired)
        with patch.object(gate.Path, "read_text",
                          lambda self, **kw: unwired
                          if self.name.endswith("ExpandedSummoningNaturalBuilder.cs")
                          else (ROOT / "src/KingmakerGunslinger/Summoning"
                                / "ExpandedSummoningNaturalProfiles.cs").read_bytes()
                               .decode("utf-8-sig")):
            with self.assertRaisesRegex(
                    AssertionError, "take its value from the rules policy"):
                gate.validate_the_measured_carriers_are_wired(ROOT)

    def test_a_wired_swim_bonus_is_rejected(self):
        """A substitution dressed as an implementation.

        Kingmaker has no Swim skill. A line that set one would make the
        declared omission untrue, and the creature would quietly carry a bonus
        on a skill the book does not give it.
        """
        builder = (ROOT / "src/KingmakerGunslinger/Blueprints"
                   / "ExpandedSummoningNaturalBuilder.cs").read_text(
                       encoding="utf-8-sig")
        substituted = builder.replace(
            "perception.Value = GiantCrabRulesPolicy.RacialPerceptionBonus",
            "perception.Value = GiantCrabRulesPolicy.RacialPerceptionBonus;\n"
            "            athletics.Value = GiantCrabRulesPolicy.RacialSwimBonus")
        self.assertNotEqual(builder, substituted)
        profiles = (ROOT / "src/KingmakerGunslinger/Summoning"
                    / "ExpandedSummoningNaturalProfiles.cs").read_text(
                        encoding="utf-8-sig")
        with patch.object(gate.Path, "read_text",
                          lambda self, **kw: substituted
                          if self.name.endswith("ExpandedSummoningNaturalBuilder.cs")
                          else profiles):
            with self.assertRaisesRegex(
                    AssertionError, "must not\n?\\s*be wired to any skill"):
                gate.validate_the_measured_carriers_are_wired(ROOT)

    def test_the_released_trees_are_exact(self):
        self.assertGreater(
            gate.preserved_files(ROOT, gate.MASTER, gate.MASTER_PROTECTED), 0)
        self.assertGreater(
            gate.preserved_files(ROOT, gate.MASTER, gate.SUMMONING_PROTECTED,
                                 allowed=gate.SPRINT21_CHANGED), 0)

    def test_the_shipped_sprint20_body_is_inside_the_boundary(self):
        """Sprint 20's two body files are protected now, not editable.

        The crab rides the same rig, so the tempting mistake is to edit the
        scorpion's generator rather than write the crab's.
        """
        self.assertIn("assets/sprint20-arachnids/", gate.SUMMONING_PROTECTED)
        self.assertIn("assets-source/original-models/sprint20-arachnids/",
                      gate.SUMMONING_PROTECTED)
        for path in ("assets/sprint20-arachnids/giant-scorpion-mesh.json",
                     "assets/sprint20-arachnids/giant-scorpion-albedo.png"):
            self.assertNotIn(path, gate.SPRINT21_CHANGED)
            self.assertNotIn(path, gate.SPRINT21_NEW)


if __name__ == "__main__":
    unittest.main(verbosity=2)
