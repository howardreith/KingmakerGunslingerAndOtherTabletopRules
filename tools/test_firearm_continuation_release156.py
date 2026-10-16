"""Behavioral corruption fixtures for the v0.0.156 firearm patch boundary."""
import copy
import sys
import unittest

sys.dont_write_bytecode = True
import validate_firearm_continuation_release156 as gate


class FirearmContinuationReleaseTests(unittest.TestCase):
    def setUp(self):
        self.entries = [dict(symbol="Released", guid="a" * 32,
                             plannedType="BlueprintUnit", status="active")]
        self.released = {"version": "0.0.155", "milestone": "0.0.155-x",
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

    def test_unchanged_identities_pass(self):
        gate.check_identities(self.entries, copy.deepcopy(self.entries))

    def test_appended_identity_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_identities(self.entries, self.entries + [dict(
                symbol="New", guid="b" * 32, plannedType="BlueprintUnit",
                status="active")])

    def test_changed_identity_fails(self):
        changed = copy.deepcopy(self.entries)
        changed[0]["guid"] = "c" * 32
        with self.assertRaises(AssertionError):
            gate.check_identities(self.entries, changed)

    def test_firearm_and_version_files_pass(self):
        gate.check_production_scope(sorted(gate.FIREARM_PRODUCTION |
                                           gate.VERSION_PRODUCTION))

    def test_unrelated_production_file_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_production_scope(
                ["src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs"])

    def test_project_adding_only_fixture_passes(self):
        released = "<Project>\n  <ItemGroup>\n  </ItemGroup>\n</Project>\n"
        current = released.replace("  <ItemGroup>\n",
                                    "  <ItemGroup>\n" + gate.PROJECT_LINE + "\n")
        gate.check_project(released, current)

    def test_project_other_change_fails(self):
        released = "<Project>\n  <ItemGroup>\n  </ItemGroup>\n</Project>\n"
        current = released.replace("  <ItemGroup>\n", "  <ItemGroup>\n" +
                                   gate.PROJECT_LINE + "\n    <Compile Include=\"Other.cs\" />\n")
        with self.assertRaises(AssertionError):
            gate.check_project(released, current)

    def test_project_without_fixture_fails(self):
        released = "<Project>\n</Project>\n"
        with self.assertRaises(AssertionError):
            gate.check_project(released, released)

    def test_version_and_own_record_pass(self):
        gate.check_static(self.released, self.current)

    def test_changed_history_fails(self):
        self.current["history"] = {"value": 2}
        with self.assertRaises(AssertionError):
            gate.check_static(self.released, self.current)

    def test_removed_history_fails(self):
        del self.current["history"]
        with self.assertRaises(AssertionError):
            gate.check_static(self.released, self.current)

    def test_altered_record_fails(self):
        self.current[gate.RECORD_KEY]["identityChanges"] = 1
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
            gate.check_version("0.0.155", self.props, self.assembly)

    def test_stale_informational_version_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_version(gate.VERSION, self.props.replace(
                gate.INFORMATIONAL, "0.0.156-other"), self.assembly)


if __name__ == "__main__":
    unittest.main()
