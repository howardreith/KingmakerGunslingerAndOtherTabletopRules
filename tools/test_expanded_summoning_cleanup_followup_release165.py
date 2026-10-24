"""Behavioral corruption fixtures for the v0.0.165 cleanup follow-up release boundary."""
import copy
import sys
import unittest

sys.dont_write_bytecode = True
import validate_expanded_summoning_cleanup_followup_release165 as gate


def row(symbol, number):
    return dict(symbol=symbol, guid="%032x" % number, plannedType="BlueprintAbility",
                status="active", milestone="m", notes="")


def creature(monster, ally):
    return {"monster": monster, "ally": ally}


class CleanupFollowupReleaseTests(unittest.TestCase):
    def setUp(self):
        self.released_entries = [row("KMG.Released.One", 1), row("KMG.Released.Two", 2)]
        self.released = {"version": "0.0.164", "milestone": "0.0.164-x",
                         "history": {"value": 1}}
        self.current = {"version": gate.VERSION, "milestone": gate.INFORMATIONAL,
                        "history": {"value": 1},
                        gate.RECORD_KEY: copy.deepcopy(gate.RECORD)}
        self.props = ("<KmgVersion>" + gate.VERSION + "</KmgVersion>"
                      "<KmgInformationalVersion>" + gate.INFORMATIONAL +
                      "</KmgInformationalVersion>")
        self.assembly = ('AssemblyVersion("' + gate.VERSION + '") '
                         'AssemblyFileVersion("' + gate.VERSION + '") '
                         'AssemblyInformationalVersion("' + gate.INFORMATIONAL + '")')
        self.changed = {path: "M" for path in gate.ART_MODIFIED}
        self.changed.update({path: "A" for path in gate.ART_ADDED})
        self.digests = dict(gate.ART)
        # 117 creatures: 96 Summon Monster rows worth 545 and 104 Summon
        # Nature's Ally rows worth 582, plus 29 native wrappers, make 1,156.
        self.creatures = ([creature(4, 4)] * 62 + [creature(4, 5)] * 3 +
                          [creature(5, 5)] * 31 + [creature(None, 5)] * 8 +
                          [creature(None, None)] * 13)

    def digest(self, path):
        return self.digests[path]

    def test_unchanged_ledger_passes(self):
        gate.check_identities(self.released_entries, copy.deepcopy(self.released_entries))

    def test_appended_identity_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_identities(self.released_entries, copy.deepcopy(self.released_entries) +
                                  [row("KMG.Summoning.Unit.NewCreature", 9999)])

    def test_changed_released_identity_fails(self):
        changed = copy.deepcopy(self.released_entries)
        changed[0]["guid"] = "c" * 32
        with self.assertRaises(AssertionError):
            gate.check_identities(self.released_entries, changed)

    def test_release_and_version_files_pass(self):
        gate.check_production_scope(sorted(gate.CLEANUP_PRODUCTION | gate.VERSION_PRODUCTION |
                                           {gate.PROJECT}))

    def test_unrelated_production_file_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_production_scope(
                ["src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs"])

    def test_unchanged_project_passes(self):
        released = "<Project>\n  <ItemGroup>\n    <Compile Include=\"A.cs\" />\n  </ItemGroup>\n</Project>\n"
        gate.check_project(released, released)

    def test_project_added_line_fails(self):
        released = "<Project>\n  <ItemGroup>\n    <Compile Include=\"A.cs\" />\n  </ItemGroup>\n</Project>\n"
        current = released.replace("  </ItemGroup>", "    <Compile Include=\"Other.cs\" />\n  </ItemGroup>")
        with self.assertRaises(AssertionError):
            gate.check_project(released, current)

    def test_project_removed_line_fails(self):
        released = "<Project>\n  <ItemGroup>\n    <Compile Include=\"A.cs\" />\n  </ItemGroup>\n</Project>\n"
        current = released.replace("    <Compile Include=\"A.cs\" />\n", "")
        with self.assertRaises(AssertionError):
            gate.check_project(released, current)

    def test_reviewed_assets_pass(self):
        gate.check_art(self.changed, self.digest)

    def test_unreviewed_asset_change_fails(self):
        self.changed["assets/sprint17-serpents/viper-mesh.json"] = "M"
        with self.assertRaises(AssertionError):
            gate.check_art(self.changed, self.digest)

    def test_modified_asset_status_change_fails(self):
        path = sorted(gate.ART_MODIFIED)[0]
        for status in ("A", "D", "R"):
            changed = dict(self.changed)
            changed[path] = status
            with self.assertRaises(AssertionError):
                gate.check_art(changed, self.digest)

    def test_added_asset_must_be_added(self):
        path = sorted(gate.ART_ADDED)[0]
        for status in ("M", "D", "R"):
            changed = dict(self.changed)
            changed[path] = status
            with self.assertRaises(AssertionError):
                gate.check_art(changed, self.digest)

    def test_reverted_or_missing_asset_fails(self):
        for path in (sorted(gate.ART_MODIFIED)[0], sorted(gate.ART_ADDED)[-1]):
            changed = dict(self.changed)
            del changed[path]
            with self.assertRaises(AssertionError):
                gate.check_art(changed, self.digest)

    def test_different_asset_bytes_fail(self):
        for path in (sorted(gate.ART_MODIFIED)[-1], sorted(gate.ART_ADDED)[0]):
            digests = dict(self.digests)
            digests[path] = "0" * 64
            with self.assertRaises(AssertionError):
                gate.check_art(self.changed, digests.__getitem__)

    def test_released_census_passes(self):
        gate.check_census(self.creatures, [])

    def test_changed_census_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_census(self.creatures[:-1], [])
        changed = list(self.creatures)
        changed[0] = creature(3, 4)
        with self.assertRaises(AssertionError):
            gate.check_census(changed, [])

    def test_withheld_creature_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_census(self.creatures, ["ettin"])

    def test_version_and_own_record_pass(self):
        gate.check_static(self.released, self.current)

    def test_changed_history_fails(self):
        self.current["history"] = {"value": 2}
        with self.assertRaises(AssertionError):
            gate.check_static(self.released, self.current)

    def test_altered_record_fails(self):
        self.current[gate.RECORD_KEY]["appendedIdentities"] = 1
        with self.assertRaises(AssertionError):
            gate.check_static(self.released, self.current)

    def test_missing_record_fails(self):
        del self.current[gate.RECORD_KEY]
        with self.assertRaises(AssertionError):
            gate.check_static(self.released, self.current)

    def test_exact_version_passes(self):
        gate.check_version(gate.VERSION, self.props, self.assembly)

    def test_wrong_version_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_version("0.0.164", self.props, self.assembly)

    def test_stale_informational_version_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_version(gate.VERSION, self.props.replace(
                gate.INFORMATIONAL, "0.0.165-other"), self.assembly)


if __name__ == "__main__":
    unittest.main()
