"""Corruption fixtures for the development-candidate source validator."""
import copy
import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

sys.dont_write_bytecode = True
import validate_development_candidate as gate

PROPS = ("<KmgVersion>1.2.3</KmgVersion>"
         "<KmgInformationalVersion>1.2.3-fixture</KmgInformationalVersion>")
ASSEMBLY = ('AssemblyVersion("1.2.3") AssemblyFileVersion("1.2.3") '
            'AssemblyInformationalVersion("1.2.3-fixture")')


def row(symbol, guid, status="active"):
    return dict(symbol=symbol, guid=guid, plannedType="BlueprintUnit",
                status=status, milestone="Sprint 1", notes="")


class DevelopmentCandidateValidatorTests(unittest.TestCase):
    def setUp(self):
        self.accepted = [row("KMG.Released.A", "a" * 32),
                         row("KMG.Released.B", "b" * 32, "reserved")]
        self.current = copy.deepcopy(self.accepted) + [row("KMG.Added.C", "c" * 32)]

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

    @staticmethod
    def metadata(info="1.2.3", props=PROPS, assembly=ASSEMBLY):
        files = {"Directory.Build.props": props,
                 "src/KingmakerGunslinger/Properties/AssemblyInfo.cs": assembly}
        return gate.version_metadata(info, files.__getitem__)

    def test_released_version_metadata_passes(self):
        gate.check_version(self.metadata(), self.metadata())

    def test_version_bump_fails(self):
        for current in (
                self.metadata(info="1.2.4"),
                self.metadata(props=PROPS.replace(">1.2.3<", ">1.2.4<")),
                self.metadata(props=PROPS.replace("1.2.3-fixture", "1.2.4-fixture")),
                self.metadata(assembly=ASSEMBLY.replace('AssemblyVersion("1.2.3")',
                                                        'AssemblyVersion("1.2.4")')),
                self.metadata(assembly=ASSEMBLY.replace('FileVersion("1.2.3")',
                                                        'FileVersion("1.2.4")')),
                self.metadata(assembly=ASSEMBLY.replace("1.2.3-fixture", "other"))):
            with self.assertRaises(AssertionError):
                gate.check_version(self.metadata(), current)

    def test_missing_or_repeated_version_metadata_fails(self):
        for props, assembly in (
                ("<KmgVersion>1.2.3</KmgVersion>", ASSEMBLY),
                (PROPS, 'AssemblyVersion("1.2.3") AssemblyFileVersion("1.2.3")'),
                (PROPS + "<KmgVersion>1.2.3</KmgVersion>", ASSEMBLY)):
            with self.assertRaises(AssertionError):
                self.metadata(props=props, assembly=assembly)

    def test_static_record_must_be_unchanged(self):
        released = {"version": "0.0.156", "history": {"value": 1}}
        gate.check_static(released, copy.deepcopy(released))
        for current in ({"version": "0.0.157", "history": {"value": 1}},
                        {"version": "0.0.156", "history": {"value": 2}},
                        {"version": "0.0.156", "history": {"value": 1},
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


class ReleaseBaselineTests(unittest.TestCase):
    """The baseline is the publisher's `v<version>` tag, found in a real
    Git history, so no validator edit is needed after a release."""

    def setUp(self):
        self.scratch = tempfile.TemporaryDirectory(ignore_cleanup_errors=True)
        self.root = Path(self.scratch.name)
        self.commits = 0
        self.git("init", "--quiet")

    def tearDown(self):
        self.scratch.cleanup()

    def git(self, *arguments):
        return subprocess.run(
            ["git", "-c", "user.name=DevelopmentFixture",
             "-c", "user.email=fixture@example.invalid", *arguments],
            cwd=self.root, text=True, capture_output=True, check=True).stdout.strip()

    def commit(self, version):
        # A distinct message per commit: two commits with the same parent,
        # tree, message and second would otherwise be the same object.
        self.commits += 1
        (self.root / "Info.json").write_text(json.dumps({"Version": version}),
                                             encoding="utf-8")
        self.git("add", "Info.json")
        self.git("commit", "--quiet", "--allow-empty", "-m",
                 "%s #%d" % (version, self.commits))
        return self.git("rev-parse", "HEAD")

    def test_tag_of_the_active_version_is_the_baseline(self):
        released = self.commit("1.2.3")
        self.git("tag", "v1.2.3")
        self.commit("1.2.3")
        self.assertEqual(("v1.2.3", released),
                         gate.resolve_baseline(self.root, "1.2.3"))

    def test_missing_tag_fails(self):
        self.commit("1.2.3")
        self.git("tag", "v1.2.3")
        self.commit("1.2.4")
        with self.assertRaises(AssertionError):
            gate.resolve_baseline(self.root, "1.2.4")

    def test_tag_the_candidate_does_not_extend_fails(self):
        start = self.commit("1.2.3")
        self.commit("1.2.3")
        work = self.git("rev-parse", "HEAD")
        self.git("checkout", "--quiet", "--detach", start)
        self.commit("1.2.3")
        self.git("tag", "v1.2.3")
        self.git("checkout", "--quiet", "--detach", work)
        with self.assertRaises(AssertionError):
            gate.resolve_baseline(self.root, "1.2.3")

    def test_tag_on_another_version_fails(self):
        self.commit("1.2.2")
        self.git("tag", "v1.2.3")
        self.commit("1.2.3")
        with self.assertRaises(AssertionError):
            gate.resolve_baseline(self.root, "1.2.3")

    def test_malformed_version_fails(self):
        self.commit("1.2.3")
        self.git("tag", "v1.2.3")
        for version in (None, "", "1.2", "v1.2.3", "1.2.3-dev", "1.2.3 "):
            with self.assertRaises(AssertionError):
                gate.resolve_baseline(self.root, version)


class ReleasedArtTests(unittest.TestCase):
    """Released Sprint 24 art must stay present and packaged; its bytes may
    change in a development candidate."""

    def setUp(self):
        self.scratch = tempfile.TemporaryDirectory(ignore_cleanup_errors=True)
        self.root = Path(self.scratch.name)
        for relative in gate.released155.ART:
            path = self.root / relative
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(b"released")
        for script in gate.PACKAGE_SCRIPTS:
            path = self.root / script
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text("sprint24-theropods deinonychus tyrannosaurus",
                            encoding="utf-8")

    def tearDown(self):
        self.scratch.cleanup()

    def test_present_and_packaged_art_passes(self):
        gate.check_released_art_owned(self.root)

    def test_refined_art_bytes_pass(self):
        for relative in gate.released155.ART:
            (self.root / relative).write_bytes(b"refined")
        gate.check_released_art_owned(self.root)

    def test_missing_art_fails(self):
        (self.root / sorted(gate.released155.ART)[0]).unlink()
        with self.assertRaises(AssertionError):
            gate.check_released_art_owned(self.root)

    def test_unpackaged_art_fails(self):
        (self.root / gate.PACKAGE_SCRIPTS[1]).write_text(
            "sprint24-theropods deinonychus", encoding="utf-8")
        with self.assertRaises(AssertionError):
            gate.check_released_art_owned(self.root)


if __name__ == "__main__":
    unittest.main()
