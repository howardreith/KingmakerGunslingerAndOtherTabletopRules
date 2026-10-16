"""Roster propagation and identity fixtures for alternate summoning adapters.

These run the real manifest tool against the real catalog and ledger, adding a
test-only creature in memory. Nothing on disk is written.
"""
import copy
import json
import sys
import unittest

sys.dont_write_bytecode = True
import expanded_summoning_manifest as tool


def creature(key, monster=None, ally=None, templated=False):
    return {"key": key, "name": key, "monster": monster, "templated": templated,
            "ally": ally, "visual": key}


class AlternateSummoningIdentityTests(unittest.TestCase):
    def setUp(self):
        self.creatures = tool.parsed_creatures()
        self.families = tool.adapter_families()
        self.plan = tool.adapter_planned(self.creatures, self.families)
        self.manifest = json.loads(tool.MANIFEST.read_text(encoding="utf-8"))

    def keys(self, family):
        return [row for row in self.families if row["family"] == family]

    def test_shipped_ledger_matches_the_catalog_projection(self):
        tool.validate_adapters(self.manifest, self.plan)

    def test_every_family_offers_each_eligible_placement_once(self):
        for family in self.families:
            field = "monster" if family["family"] == "SM" else "ally"
            expected = sum(1 for tier in family["tiers"] for value in self.creatures
                           if value[field] is not None and value[field] <= tier)
            observed = sum(1 for symbol, _ in self.plan if symbol.startswith(
                tool.ADAPTER_PREFIX + family["key"] + ".Tier"))
            self.assertEqual(expected, observed, family["key"])

    def test_new_placement_reaches_only_eligible_callers(self):
        added = self.creatures + [creature("test-only-sna-beast", ally=4)]
        plan = tool.adapter_planned(added, self.families)
        new = sorted(set(plan) - set(self.plan))
        self.assertTrue(new)
        for family in self.families:
            mine = [symbol for symbol, _ in new if symbol.startswith(
                tool.ADAPTER_PREFIX + family["key"] + ".")]
            if family["family"] == "SM":
                self.assertEqual([], mine, "an SNA-only entry gained an SM route")
                continue
            tiers = [tier for tier in family["tiers"] if tier >= 4]
            self.assertEqual(len(tiers), len(mine), family["key"])
            for tier in tiers:
                count = "One" if tier == 4 else "OneD3" if tier == 5 else "OneD4PlusOne"
                self.assertIn(f"{tool.ADAPTER_PREFIX}{family['key']}.Tier{tier}."
                              f"TestOnlySnaBeast.{count}", mine)

    def test_allocation_is_append_only_and_deterministic(self):
        added = self.creatures + [creature("test-only-monster", monster=2,
                                           templated=True)]
        plan = tool.adapter_planned(added, self.families)
        manifest = copy.deepcopy(self.manifest)
        before = copy.deepcopy(manifest["entries"])
        appended = tool.allocate_adapters(manifest, plan)
        self.assertEqual(len(set(plan) - set(self.plan)), appended)
        self.assertEqual(before, manifest["entries"][:len(before)])
        tool.validate_adapters(manifest, plan)
        again = copy.deepcopy(self.manifest)
        tool.allocate_adapters(again, plan)
        self.assertEqual(manifest, again, "allocation is not reproducible")
        self.assertEqual(0, tool.allocate_adapters(manifest, plan))

    def test_hand_edited_or_stray_adapter_identity_fails(self):
        manifest = copy.deepcopy(self.manifest)
        target = next(entry for entry in manifest["entries"]
                      if entry["symbol"].startswith(tool.ADAPTER_PREFIX))
        target["guid"] = "0" * 32
        with self.assertRaises(SystemExit):
            tool.validate_adapters(manifest, self.plan)
        manifest = copy.deepcopy(self.manifest)
        manifest["entries"].append({"symbol": tool.ADAPTER_PREFIX + "Ghost.Tier1.X.One",
                                    "guid": "1" * 32, "plannedType": "BlueprintAbility",
                                    "status": "active"})
        with self.assertRaises(SystemExit):
            tool.validate_adapters(manifest, self.plan)

    def test_canonical_foundation_is_untouched(self):
        tool.validate(self.manifest, tool.planned())
        canonical = {symbol for symbol, _ in tool.planned()}
        self.assertFalse(canonical & {symbol for symbol, _ in self.plan})


if __name__ == "__main__":
    unittest.main()
