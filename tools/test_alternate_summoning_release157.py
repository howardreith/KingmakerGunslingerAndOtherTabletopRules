"""Behavioral corruption fixtures for the v0.0.157 alternate summoning release boundary."""
import copy
import sys
import unittest

sys.dont_write_bytecode = True
import validate_alternate_summoning_release157 as gate


def row(symbol, number):
    return dict(symbol=symbol, guid="%032x" % number, plannedType="BlueprintAbility",
                status="active", milestone="m", notes="")


class AlternateSummoningReleaseTests(unittest.TestCase):
    def setUp(self):
        self.released_entries = [row("KMG.Released.One", 1), row("KMG.Released.Two", 2)]
        self.adapters = [row(gate.ADAPTER_PREFIX + "Family.Tier1.Creature%d.One" % index,
                             1000 + index) for index in range(gate.APPENDED_IDENTITIES)]
        self.released = {"version": "0.0.156", "milestone": "0.0.156-x",
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

    def test_exact_adapter_append_passes(self):
        gate.check_identities(self.released_entries,
                              copy.deepcopy(self.released_entries) + self.adapters)

    def test_missing_adapter_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_identities(self.released_entries,
                                  copy.deepcopy(self.released_entries) + self.adapters[:-1])

    def test_foreign_appended_identity_fails(self):
        appended = copy.deepcopy(self.adapters)
        appended[-1] = row("KMG.Summoning.Unit.NewCreature", 9999)
        with self.assertRaises(AssertionError):
            gate.check_identities(self.released_entries,
                                  copy.deepcopy(self.released_entries) + appended)

    def test_changed_released_identity_fails(self):
        changed = copy.deepcopy(self.released_entries)
        changed[0]["guid"] = "c" * 32
        with self.assertRaises(AssertionError):
            gate.check_identities(self.released_entries, changed + self.adapters)

    def test_release_and_version_files_pass(self):
        gate.check_production_scope(sorted(gate.ALTERNATE_SUMMONING_PRODUCTION |
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
            gate.check_version("0.0.156", self.props, self.assembly)

    def test_stale_informational_version_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_version(gate.VERSION, self.props.replace(
                gate.INFORMATIONAL, "0.0.157-other"), self.assembly)


if __name__ == "__main__":
    unittest.main()
