"""Corruption fixtures for the development-candidate source validator."""
import copy
import sys
import unittest

sys.dont_write_bytecode = True
import validate_development_candidate as gate
import validate_alternate_summoning_release157 as released


def row(symbol, guid, status="active"):
    return dict(symbol=symbol, guid=guid, plannedType="BlueprintUnit",
                status=status, milestone="Sprint 1", notes="")


class DevelopmentCandidateValidatorTests(unittest.TestCase):
    def setUp(self):
        self.accepted = [row("KMG.Released.A", "a" * 32),
                         row("KMG.Released.B", "b" * 32, "reserved")]
        self.current = copy.deepcopy(self.accepted) + [row("KMG.Added.C", "c" * 32)]
        self.props = ("<KmgVersion>" + released.VERSION + "</KmgVersion>"
                      "<KmgInformationalVersion>" + released.INFORMATIONAL +
                      "</KmgInformationalVersion>")
        self.assembly = ('AssemblyVersion("' + released.VERSION + '") '
                         'AssemblyFileVersion("' + released.VERSION + '") '
                         'AssemblyInformationalVersion("' +
                         released.INFORMATIONAL + '")')

    def test_appended_identities_pass(self):
        self.assertEqual(1, gate.check_append_only(self.accepted, self.current))

    def test_unchanged_ledger_passes_with_no_appends(self):
        self.assertEqual(0, gate.check_append_only(self.accepted,
                                                   copy.deepcopy(self.accepted)))

    def test_changed_released_row_fails(self):
        self.current[0]["guid"] = "d" * 32
        with self.assertRaises(AssertionError):
            gate.check_append_only(self.accepted, self.current)

    def test_removed_released_row_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_append_only(self.accepted, self.current[1:])

    def test_reordered_released_rows_fail(self):
        self.current[0], self.current[1] = self.current[1], self.current[0]
        with self.assertRaises(AssertionError):
            gate.check_append_only(self.accepted, self.current)

    def test_colliding_guid_fails(self):
        self.current[2]["guid"] = "a" * 32
        with self.assertRaises(AssertionError):
            gate.check_append_only(self.accepted, self.current)

    def test_colliding_symbol_fails(self):
        self.current[2]["symbol"] = "KMG.Released.A"
        with self.assertRaises(AssertionError):
            gate.check_append_only(self.accepted, self.current)

    def test_malformed_guid_fails(self):
        for guid in ("C" * 32, "c" * 31, "c" * 8 + "-" + "c" * 23, 7):
            current = copy.deepcopy(self.current)
            current[2]["guid"] = guid
            with self.assertRaises(AssertionError):
                gate.check_append_only(self.accepted, current)

    def test_malformed_symbol_status_and_fields_fail(self):
        for field, value in (("symbol", "Other.Name"), ("status", "live"),
                             ("plannedType", ""), ("milestone", "")):
            current = copy.deepcopy(self.current)
            current[2][field] = value
            with self.assertRaises(AssertionError):
                gate.check_append_only(self.accepted, current)
        current = copy.deepcopy(self.current)
        del current[2]["notes"]
        with self.assertRaises(AssertionError):
            gate.check_append_only(self.accepted, current)
        current = copy.deepcopy(self.current)
        current[2]["extra"] = 1
        with self.assertRaises(AssertionError):
            gate.check_append_only(self.accepted, current)

    def test_released_version_metadata_passes(self):
        self.assertEqual("8726ec737a73f9a0956f3ae9cbab2aceeb7ffc7c", gate.BASELINE)
        self.assertEqual(released.VERSION, gate.BASELINE_VERSION)
        gate.check_version(released.VERSION, self.props, self.assembly)

    def test_version_bump_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_version("0.0.158", self.props.replace("0.0.157", "0.0.158"),
                               self.assembly.replace("0.0.157", "0.0.158"))
        with self.assertRaises(AssertionError):
            gate.check_version(released.VERSION, self.props,
                               self.assembly.replace("0.0.157\")", "0.0.158\")", 1))

    def test_old_baseline_and_changed_informational_metadata_fail(self):
        with self.assertRaises(AssertionError):
            gate.check_version("0.0.156", self.props.replace("0.0.157", "0.0.156"),
                               self.assembly.replace("0.0.157", "0.0.156"))
        for props, assembly in ((self.props.replace(released.INFORMATIONAL, "test"), self.assembly),
                                (self.props, self.assembly.replace(released.INFORMATIONAL, "test"))):
            with self.assertRaises(AssertionError):
                gate.check_version(released.VERSION, props, assembly)

    def test_static_record_must_be_unchanged(self):
        released = {"version": "0.0.157", "history": {"value": 1}}
        gate.check_static(released, copy.deepcopy(released))
        for current in ({"version": "0.0.158", "history": {"value": 1}},
                        {"version": "0.0.157", "history": {"value": 2}},
                        {"version": "0.0.157", "history": {"value": 1},
                         "developmentQualification": True}):
            with self.assertRaises(AssertionError):
                gate.check_static(released, current)

    def test_purpose_names(self):
        for purpose in ("alternate-summoning-native", "abc", "a1-b2"):
            self.assertEqual(purpose, gate.check_purpose(purpose))
        for purpose in ("", "ab", "Release", "-lead", "trail-", "a--b",
                        "has space", "dot.name", "x" * 49, None, "..\\escape"):
            with self.assertRaises(AssertionError):
                gate.check_purpose(purpose)


if __name__ == "__main__":
    unittest.main()
