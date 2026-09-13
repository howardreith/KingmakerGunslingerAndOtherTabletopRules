"""Corruption tests for cross-process compatibility attribution; no runtime claims."""
import copy
import sys
import unittest
sys.dont_write_bytecode = True
from validate_icon_runtime_evidence import compare_control

class ControlComparisonTests(unittest.TestCase):
    def setUp(self):
        self.candidate = {"controlRun": False, "runId": "candidate", "profile": {"enabled": True},
            "runtimeIdentity": {"loadedModuleSha256": "same", "moduleVersionId": "same"},
            "inventory": [{"symbol": "owned", "guid": "owned-guid", "type": "Ability",
                "registrationKind": "blueprint-library", "before": {"name": "old", "spriteInstanceId": 1},
                "beforeGraph": ["old-edge"], "afterGraph": ["old-edge", "compatibility-edge"],
                "beforeComponents": [{"instanceId": 2, "type": "old"}],
                "afterComponents": [{"instanceId": 2, "type": "old"}, {"instanceId": 3, "type": "added"}]}],
            "protectedAssignments": [{"guid": "foreign", "before": {"isNull": True},
                "after": {"isNull": False, "name": "native", "spriteInstanceId": 4}, "sameSpriteReference": False}]}
        self.control = copy.deepcopy(self.candidate)
        self.control.update(controlRun=True, runId="control")

    def test_matching_compatibility_transition_ignores_only_process_ids(self):
        self.control["protectedAssignments"][0]["after"]["spriteInstanceId"] = 999
        self.assertEqual([], compare_control(self.candidate, self.control))

    def test_extra_candidate_graph_edge_is_rejected(self):
        self.candidate["inventory"][0]["afterGraph"].append("unexpected")
        self.assertTrue(compare_control(self.candidate, self.control))

    def test_removed_component_is_rejected(self):
        self.candidate["inventory"][0]["afterComponents"].pop(0)
        self.assertTrue(compare_control(self.candidate, self.control))

    def test_different_foreign_sprite_is_rejected(self):
        self.candidate["protectedAssignments"][0]["after"]["name"] = "repainted"
        self.assertTrue(compare_control(self.candidate, self.control))

    def test_missing_protection_is_rejected(self):
        self.candidate["protectedAssignments"] = []
        self.assertTrue(compare_control(self.candidate, self.control))

    def test_wrong_artifact_profile_or_reused_run_is_rejected(self):
        for field, value in [("runId", "candidate"), ("controlRun", False), ("profile", {"enabled": False}),
                             ("runtimeIdentity", {"loadedModuleSha256": "other", "moduleVersionId": "same"})]:
            with self.subTest(field=field):
                changed = copy.deepcopy(self.control)
                changed[field] = value
                self.assertTrue(compare_control(self.candidate, changed))

if __name__ == "__main__":
    unittest.main(testRunner=unittest.TextTestRunner(stream=sys.stdout, verbosity=2))
