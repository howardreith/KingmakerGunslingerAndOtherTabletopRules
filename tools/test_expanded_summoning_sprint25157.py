"""Behavioral corruption fixtures for the Sprint 25 source boundary.

These prove the gate refuses, not that it exists. The two Sprint 25 specific
refusals are the point: a registered candidate that grew a Summon Monster
placement or a template child, and any suppression of the already-released
Elephant or Mastodon.
"""
import copy
import sys
import unittest

sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint25157 as gate


def row(symbol, guid, planned):
    return dict(symbol=symbol, guid=guid, plannedType=planned, status="active")


class Sprint25GateTests(unittest.TestCase):
    def setUp(self):
        self.old = [row("Released", "a" * 32, "BlueprintUnit")]
        self.tail = [row(symbol, value[0], value[1])
                     for symbol, value in gate.ADDED.items()]

    # --- the append boundary -------------------------------------------------
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
        changed = self.old + self.tail + [
            row("KMG.Summoning.Unit.Elephant", "c" * 32, "BlueprintUnit")]
        with self.assertRaises(AssertionError):
            gate.validate_append(self.old, changed)

    def test_the_allowed_set_is_exactly_ten_and_names_the_elephant_trample(self):
        # The append grew by exactly one on 2026-10-16, after the bounded
        # observation of the released Elephant recorded six grants and no
        # trample ability or component. This pins the new exact set rather
        # than only the count.
        self.assertEqual(10, len(gate.ADDED))
        self.assertIn("KMG.Summoning.Special.Elephant.Trample", gate.ADDED)
        self.assertEqual(
            "BlueprintAbility",
            gate.ADDED["KMG.Summoning.Special.Elephant.Trample"][1])
        # And the ten-row tail is what the gate actually accepts.
        self.assertEqual("registered",
                         gate.validate_append(self.old, self.old + self.tail))

    def test_the_other_reserved_elephant_symbols_still_fail(self):
        # Only the trample was justified by that observation. The chassis and
        # unit type remain reserved names, and appending either must still be
        # refused rather than waved through because one sibling was allowed.
        for symbol in ("KMG.Summoning.Natural.Elephant.Chassis",
                       "KMG.Summoning.Natural.Elephant.UnitType"):
            changed = self.old + self.tail + [
                row(symbol, "d" * 32, "BlueprintFeature")]
            with self.assertRaises(AssertionError):
                gate.validate_append(self.old, changed)

    def test_wrong_planned_type_fails(self):
        changed = copy.deepcopy(self.old + self.tail)
        changed[-1]["plannedType"] = "BlueprintBuff"
        with self.assertRaises(AssertionError):
            gate.validate_append(self.old, changed)

    def test_reserved_status_fails(self):
        changed = copy.deepcopy(self.old + self.tail)
        changed[-1]["status"] = "reserved"
        with self.assertRaises(AssertionError):
            gate.validate_append(self.old, changed)

    def test_duplicate_guid_fails(self):
        changed = copy.deepcopy(self.old + self.tail)
        changed[-1]["guid"] = changed[-2]["guid"]
        with self.assertRaises(AssertionError):
            gate.validate_append(self.old, changed)

    # --- Sprint 25's own two refusals ---------------------------------------
    def test_monster_placement_for_the_new_creature_fails(self):
        changed = self.old + self.tail + [row(
            "KMG.Summoning.Ability.SM.Tier7.Brachiosaurus.One",
            "d" * 32, "BlueprintAbility")]
        with self.assertRaises(AssertionError):
            gate.validate_append(self.old, changed)

    def test_template_execution_child_fails(self):
        for suffix in (".Celestial", ".Fiendish"):
            changed = self.old + self.tail + [row(
                "KMG.Summoning.Ability.SNA.Tier7.Brachiosaurus.One" + suffix,
                "e" * 32, "BlueprintAbility")]
            with self.assertRaises(AssertionError):
                gate.validate_append(self.old, changed)

    # --- visibility ----------------------------------------------------------
    def _source(self, keys):
        inner = ", ".join('"%s"' % key for key in keys)
        return ("private static readonly HashSet<string> SuppressedCreatureKeys =\n"
                "    new HashSet<string>(new[] { %s }, StringComparer.Ordinal);" % inner)

    def test_hidden_and_public_stages(self):
        self.assertEqual(
            "hidden",
            gate.validate_visibility(self._source(["brachiosaurus"]), "registered"))
        self.assertEqual(
            "public", gate.validate_visibility(self._source([]), "registered"))

    def test_census_cannot_suppress_an_unregistered_creature(self):
        with self.assertRaises(AssertionError):
            gate.validate_visibility(self._source(["brachiosaurus"]), "census")

    def test_released_elephant_or_mastodon_can_never_be_suppressed(self):
        for key in ("elephant", "mastodon"):
            with self.assertRaises(AssertionError):
                gate.validate_visibility(self._source([key]), "registered")
            with self.assertRaises(AssertionError):
                gate.validate_visibility(
                    self._source(["brachiosaurus", key]), "registered")

    def test_an_unrelated_extra_suppression_fails(self):
        with self.assertRaises(AssertionError):
            gate.validate_visibility(
                self._source(["brachiosaurus", "triceratops"]), "registered")

    def test_unreadable_suppression_source_fails(self):
        with self.assertRaises(AssertionError):
            gate.validate_visibility("no suppression set here", "registered")


if __name__ == "__main__":
    unittest.main(verbosity=0)
