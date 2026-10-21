"""Offline refusal checks for the Sprint 28 public source boundary."""
import copy
import json
import sys
import tempfile
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import patch

sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint28162 as gate
import validate_repository as dispatch


class GiantBoundaryTests(unittest.TestCase):
    def setUp(self):
        self.released = [("released", 1, True, 1)]
        self.giants = sorted(gate.EXPECTED_GIANTS)
        self.root = Path(__file__).resolve().parents[1]
        self.old = [{"symbol": "KMG.Released.A", "guid": "a"*32,
                     "plannedType": "BlueprintUnit", "status": "active",
                     "milestone": "released", "notes": "preserved"}]
        # This refusal fixture describes the released Sprint 28 tail. Later
        # append-only sprints move the current ledger tail without changing it.
        self.tail = json.loads(gate.common.blob(
            self.root, "v0.0.162", "blueprints/blueprints.json")
            .decode("utf-8-sig"))["entries"][-gate.TAIL_COUNT:]

    def test_exact_public_rows_and_identity_tail(self):
        self.assertEqual("public", gate.check_scope(
            self.released, self.released + self.giants, set()))
        gate.check_tail(self.old, self.old + self.tail)

    def test_public_scope_refuses_other_creature_family_and_suppression(self):
        for current, hidden in [
            (self.released + self.giants[:-1], set()),
            (self.released + self.giants + [("frost-giant", None, False, 7)], set()),
            (self.released + [("hill-giant", 6, False, 6)] + self.giants[1:], set()),
            (self.released + self.giants, {"stone-giant"}),
            (self.giants + self.released, set())]:
            with self.assertRaises(AssertionError):
                gate.check_scope(self.released, current, hidden)

    def test_identity_prefix_and_tail_refusals(self):
        bad = copy.deepcopy(self.old + self.tail)
        bad[0]["guid"] = "b"*32
        with self.assertRaises(AssertionError):
            gate.check_tail(self.old, bad)
        for index, field, value in [
            (1, "guid", "0"*32), (1, "status", "reserved"),
            (len(self.tail), "plannedType", "BlueprintUnit")]:
            bad = copy.deepcopy(self.old + self.tail)
            bad[index][field] = value
            with self.assertRaises(AssertionError):
                gate.check_tail(self.old, bad)

    def test_active_dispatch_propagates_child_exit(self):
        for code in (0, 7):
            with tempfile.TemporaryDirectory() as directory:
                root = Path(directory)
                (root / "Info.json").write_text(
                    json.dumps({"Version": "0.0.162"}), encoding="utf-8")
                with patch.object(sys, "argv", ["validator", "--root", str(root)]), \
                     patch.object(dispatch.subprocess, "run",
                                  return_value=SimpleNamespace(returncode=code)) as child:
                    self.assertEqual(code, dispatch.main())
                    self.assertEqual("validate_expanded_summoning_sprint28162.py",
                                     Path(child.call_args.args[0][1]).name)
                    self.assertEqual(["--root", str(root.resolve())],
                                     child.call_args.args[0][-2:])


if __name__ == "__main__":
    unittest.main()
