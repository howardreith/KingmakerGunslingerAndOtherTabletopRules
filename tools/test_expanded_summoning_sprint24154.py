"""Behavioral corruption fixtures for the Sprint 24 source boundary."""
import copy
import sys
import unittest

sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint24154 as gate


class Sprint24GateTests(unittest.TestCase):
    def setUp(self):
        self.old = [dict(symbol="Released", guid="a" * 32,
                         plannedType="BlueprintUnit", status="active")]
        self.tail = [dict(symbol=symbol, guid=value[0],
                          plannedType=value[1], status="active")
                     for symbol, value in gate.ADDED.items()]

    def test_census_prefix_passes(self):
        self.assertEqual("census", gate.validate_append(self.old, self.old))

    def test_exact_append_passes(self):
        self.assertEqual("registered",
                         gate.validate_append(self.old, self.old + self.tail))

    def test_released_row_move_fails(self):
        changed = copy.deepcopy(self.old + self.tail)
        changed[0]["guid"] = "b" * 32
        with self.assertRaises(AssertionError):
            gate.validate_append(self.old, changed)

    def test_missing_identity_fails(self):
        with self.assertRaises(AssertionError):
            gate.validate_append(self.old, self.old + self.tail[:-1])

    def test_extra_identity_fails(self):
        changed = self.old + self.tail + [dict(
            symbol="Sprint25", guid="c" * 32,
            plannedType="BlueprintUnit", status="active")]
        with self.assertRaises(AssertionError):
            gate.validate_append(self.old, changed)

    def test_wrong_type_and_guid_fail(self):
        wrong_type = copy.deepcopy(self.old + self.tail)
        wrong_type[-1]["plannedType"] = "BlueprintAbility"
        wrong_guid = copy.deepcopy(self.old + self.tail)
        wrong_guid[-1]["guid"] = "d" * 32
        for changed in (wrong_type, wrong_guid):
            with self.assertRaises(AssertionError):
                gate.validate_append(self.old, changed)

    def test_retired_or_duplicate_identity_fails(self):
        retired = copy.deepcopy(self.old + self.tail)
        retired[-1]["status"] = "reserved"
        duplicate = copy.deepcopy(self.old + self.tail)
        duplicate[-1]["guid"] = duplicate[0]["guid"]
        for changed in (retired, duplicate):
            with self.assertRaises(AssertionError):
                gate.validate_append(self.old, changed)

    def test_visibility_states_are_atomic(self):
        empty = "SuppressedCreatureKeys = new HashSet<string>(StringComparer.Ordinal);"
        hidden = ('SuppressedCreatureKeys = new HashSet<string>(new[] { '
                  '"deinonychus", "tyrannosaurus" }, StringComparer.Ordinal);')
        half = ('SuppressedCreatureKeys = new HashSet<string>(new[] { '
                '"deinonychus" }, StringComparer.Ordinal);')
        self.assertEqual("census", gate.validate_visibility(empty, "census"))
        self.assertEqual("hidden", gate.validate_visibility(hidden, "registered"))
        self.assertEqual("public", gate.validate_visibility(empty, "registered"))
        with self.assertRaises(AssertionError):
            gate.validate_visibility(half, "registered")

    def test_identity_shape_is_exact(self):
        self.assertEqual(48, len(gate.ADDED))
        self.assertEqual(2, sum(symbol.startswith("KMG.Summoning.Unit.")
                                for symbol in gate.ADDED))
        self.assertEqual(18, sum(symbol.startswith("KMG.Summoning.Ability.") and
                                 not symbol.endswith((".Celestial", ".Fiendish"))
                                 for symbol in gate.ADDED))
        self.assertEqual(18, sum(symbol.endswith((".Celestial", ".Fiendish"))
                                 for symbol in gate.ADDED))
        self.assertEqual(10, sum(not symbol.startswith(("KMG.Summoning.Unit.",
                                                        "KMG.Summoning.Ability."))
                                 for symbol in gate.ADDED))

    def test_production_boundary_admits_only_named_sprint24_seams(self):
        self.assertIn(
            "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint24Census.cs",
            gate.ALLOWED_PRODUCTION)
        self.assertIn(
            "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs",
            gate.ALLOWED_PRODUCTION)
        # The version stamp is a per-sprint seam, as on Sprint 23.
        self.assertIn(
            "src/KingmakerGunslinger/Properties/AssemblyInfo.cs",
            gate.ALLOWED_PRODUCTION)
        # Sprint 24 is based on the actual released master, not the pre-rewrite one.
        self.assertEqual("fd6c56ebd8eb82e313c6005972e4538d9b1444c9", gate.BASELINE)
        # The inventory observer must record the published theropod carriers.
        self.assertIn(
            "src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningInventoryObservationPolicy.cs",
            gate.ALLOWED_PRODUCTION)
        # The theropod family must be known to the shared resource observer.
        self.assertIn(
            "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningResourceReceipts.cs",
            gate.ALLOWED_PRODUCTION)
        # The swallowed-state reload scope is a deliberate Sprint 24 seam.
        self.assertIn(
            "src/KingmakerGunslinger/Summoning/ExpandedSummoningSpecialProfiles.cs",
            gate.ALLOWED_PRODUCTION)
        self.assertNotIn(
            "src/KingmakerGunslinger/Summoning/ArmoredDinosaurRulesPolicy.cs",
            gate.ALLOWED_PRODUCTION)
        # Another sprint's grapple carrier still cannot ride this boundary.
        self.assertNotIn(
            "src/KingmakerGunslinger/Summoning/CrocodilianAttackVisualPose.cs",
            gate.ALLOWED_PRODUCTION)


if __name__ == "__main__":
    unittest.main()
