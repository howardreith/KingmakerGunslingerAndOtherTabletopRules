"""Behavioral corruption fixtures for the Sprint 23 source boundary."""
import copy
import sys
import unittest

sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint23152 as gate


class Sprint23GateTests(unittest.TestCase):
    def setUp(self):
        self.old = [dict(symbol="Released", guid="a" * 32,
                         plannedType="BlueprintUnit")]
        self.current = self.old + [dict(symbol=symbol, guid=value[0],
            plannedType=value[1], status=("reserved" if symbol ==
                "KMG.Summoning.Natural.Triceratops.GoreType" else "active"))
            for symbol, value in gate.ADDED.items()]

    def test_exact_append_passes(self):
        gate.validate_append(self.old, self.current)

    def test_released_guid_move_fails(self):
        changed = copy.deepcopy(self.current)
        changed[0]["guid"] = "b" * 32
        with self.assertRaises(AssertionError):
            gate.validate_append(self.old, changed)

    def test_extra_identity_fails(self):
        changed = self.current + [dict(symbol="Sprint24", guid="c" * 32,
                                       plannedType="BlueprintUnit")]
        with self.assertRaises(AssertionError):
            gate.validate_append(self.old, changed)

    def test_wrong_type_fails(self):
        changed = copy.deepcopy(self.current)
        changed[-1]["plannedType"] = "BlueprintBuff"
        with self.assertRaises(AssertionError):
            gate.validate_append(self.old, changed)

    def test_duplicate_guid_fails(self):
        changed = copy.deepcopy(self.current)
        changed[-1]["guid"] = changed[0]["guid"]
        with self.assertRaises(AssertionError):
            gate.validate_append(self.old, changed)

    def test_rejected_gore_type_cannot_be_reactivated(self):
        changed = copy.deepcopy(self.current)
        changed[-1]["status"] = "active"
        with self.assertRaises(AssertionError):
            gate.validate_append(self.old, changed)

    def test_targeted_persistence_files_are_explicitly_scoped(self):
        self.assertIn(
            "src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningRosterPersistencePolicy.cs",
            gate.ALLOWED_PRODUCTION)
        self.assertIn(
            "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningRosterPersistence.cs",
            gate.ALLOWED_PRODUCTION)
        self.assertNotIn(
            "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint24Review.cs",
            gate.ALLOWED_PRODUCTION)

    def test_sprint24_admission_is_research_only(self):
        self.assertIn(
            "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint24Census.cs",
            gate.ALLOWED_PRODUCTION)
        self.assertIn(
            "src/KingmakerGunslinger/RuntimeTesting/Sprint24TheropodCensusPolicy.cs",
            gate.ALLOWED_PRODUCTION)
        for production in (
                "src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.Theropods.cs",
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint24Review.cs",
                "src/KingmakerGunslinger/Summoning/TheropodRulesPolicy.cs"):
            self.assertNotIn(production, gate.ALLOWED_PRODUCTION)


if __name__ == "__main__":
    unittest.main()
