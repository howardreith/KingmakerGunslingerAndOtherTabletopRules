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
            plannedType=value[1]) for symbol, value in gate.ADDED.items()]

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


if __name__ == "__main__":
    unittest.main()
