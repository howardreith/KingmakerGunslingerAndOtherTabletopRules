"""Behavioral corruption fixtures for the v0.0.158 visual refinement release boundary."""
import copy
import sys
import unittest

sys.dont_write_bytecode = True
import validate_expanded_summoning_visual_refinement_release158 as gate


def row(symbol, number):
    return dict(symbol=symbol, guid="%032x" % number, plannedType="BlueprintAbility",
                status="active", milestone="m", notes="")


class VisualRefinementReleaseTests(unittest.TestCase):
    def setUp(self):
        self.released_entries = [row("KMG.Released.One", 1), row("KMG.Released.Two", 2)]
        self.released = {"version": "0.0.157", "milestone": "0.0.157-x",
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
        self.changed = {path: "M" for path in gate.ART}
        self.digests = dict(gate.ART)

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
        gate.check_production_scope(sorted(gate.VISUAL_REFINEMENT_PRODUCTION |
                                           gate.STARTUP_REPAIR_PRODUCTION |
                                           gate.VERSION_PRODUCTION | {gate.PROJECT}))

    def test_unrelated_production_file_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_production_scope(
                ["src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs"])

    def test_project_adding_only_pinned_items_passes(self):
        released = "<Project>\n  <ItemGroup>\n  </ItemGroup>\n</Project>\n"
        current = released.replace("  <ItemGroup>\n", "  <ItemGroup>\n" +
                                   "\n".join(gate.PROJECT_LINES) + "\n")
        gate.check_project(released, current)

    def test_project_other_change_fails(self):
        released = "<Project>\n  <ItemGroup>\n  </ItemGroup>\n</Project>\n"
        current = released.replace("  <ItemGroup>\n", "  <ItemGroup>\n" +
                                   "\n".join(gate.PROJECT_LINES) +
                                   "\n    <Compile Include=\"Other.cs\" />\n")
        with self.assertRaises(AssertionError):
            gate.check_project(released, current)

    def test_project_missing_item_fails(self):
        released = "<Project>\n  <ItemGroup>\n  </ItemGroup>\n</Project>\n"
        current = released.replace("  <ItemGroup>\n", "  <ItemGroup>\n" +
                                   "\n".join(gate.PROJECT_LINES[1:]) + "\n")
        with self.assertRaises(AssertionError):
            gate.check_project(released, current)

    def test_reviewed_assets_pass(self):
        gate.check_art(self.changed, self.digest)

    def test_unreviewed_asset_change_fails(self):
        self.changed["assets/sprint17-serpents/viper-mesh.json"] = "M"
        with self.assertRaises(AssertionError):
            gate.check_art(self.changed, self.digest)

    def test_added_or_removed_asset_fails(self):
        path = sorted(gate.ART)[0]
        for status in ("A", "D", "R"):
            changed = dict(self.changed)
            changed[path] = status
            with self.assertRaises(AssertionError):
                gate.check_art(changed, self.digest)

    def test_reverted_reviewed_asset_fails(self):
        del self.changed[sorted(gate.ART)[0]]
        with self.assertRaises(AssertionError):
            gate.check_art(self.changed, self.digest)

    def test_different_asset_bytes_fail(self):
        self.digests[sorted(gate.ART)[-1]] = "0" * 64
        with self.assertRaises(AssertionError):
            gate.check_art(self.changed, self.digest)

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
            gate.check_version("0.0.157", self.props, self.assembly)

    def test_stale_informational_version_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_version(gate.VERSION, self.props.replace(
                gate.INFORMATIONAL, "0.0.158-other"), self.assembly)


if __name__ == "__main__":
    unittest.main()
