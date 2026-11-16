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


# The one authorized released-row correction, stated here independently of
# the validator so that any widening of its constant is caught.
KIT_POSITION = 206
KIT = dict(symbol="KMG.Gunsmithing.GunsmithKit",
           guid="d52aacb753434691b1ed85a16cc87104",
           plannedType="BlueprintItem", status="active",
           milestone="Sixth Playtest 0.0.66")
FROZEN_NOTE = ("Stable non-consumable Gunsmith's Kit required for basic ammunition "
               "crafting and for unified Broken-or-Wrecked-to-Normal firearm repair.")
APPROVED_NOTE = ("Stable non-consumable Gunsmith's Kit required for basic ammunition "
                 "crafting and for Broken-to-Normal field repair. Wrecked firearms "
                 "are restored only by a completed full rest.")
ROOT = Path(__file__).resolve().parents[1]


def ledger(kit_note, size=210):
    """A synthetic released-shaped ledger with the Gunsmith's Kit record at
    its real position and every other row distinct and well formed."""
    rows = [dict(symbol="KMG.Fixture.Row%03d" % index, guid="%032x" % (index + 1),
                 plannedType="BlueprintUnit", status="active",
                 milestone="Sprint 1", notes="fixture note %d" % index)
            for index in range(size)]
    rows[KIT_POSITION] = dict(KIT, notes=kit_note)
    return rows


class AuthorizedNoteCorrectionTests(unittest.TestCase):
    """The development comparator's one authorized released-row
    correction, and everything it must still refuse."""

    def setUp(self):
        self.frozen = ledger(FROZEN_NOTE)
        self.approved = ledger(APPROVED_NOTE)

    def refuses(self, released, current):
        with self.assertRaises(AssertionError):
            gate.check_development_ledger(released, current)

    def test_the_authorization_is_exactly_one_row_one_field_one_direction(self):
        self.assertEqual(1, len(gate.AUTHORIZED_RELEASED_ROW_CORRECTIONS))
        correction = gate.AUTHORIZED_RELEASED_ROW_CORRECTIONS[0]
        self.assertEqual(KIT_POSITION, correction["position"])
        self.assertEqual(dict(KIT, notes=FROZEN_NOTE), correction["before"])
        self.assertEqual(dict(KIT, notes=APPROVED_NOTE), correction["after"])
        self.assertEqual({"position", "authorization", "before", "after"},
                         set(correction))

    def test_frozen_to_approved_passes_with_no_appended_identity(self):
        appended, corrections = gate.check_development_ledger(self.frozen, self.approved)
        self.assertEqual(0, appended)
        self.assertEqual(1, len(corrections))
        record = corrections[0]
        self.assertEqual((KIT_POSITION, KIT["symbol"], KIT["guid"], ["notes"]),
                         (record["position"], record["symbol"], record["guid"],
                          record["fields"]))
        self.assertEqual({"notes": FROZEN_NOTE}, record["before"])
        self.assertEqual({"notes": APPROVED_NOTE}, record["after"])
        self.assertIn("INT-07", record["authorization"])

    def test_correction_with_a_valid_appended_row_reports_the_append(self):
        current = self.approved + [row("KMG.Added.New", "e" * 32)]
        appended, corrections = gate.check_development_ledger(self.frozen, current)
        self.assertEqual(1, appended)
        self.assertEqual([KIT_POSITION], [value["position"] for value in corrections])

    def test_identical_historical_rows_pass_with_no_correction(self):
        self.assertEqual((0, []), gate.check_development_ledger(
            self.frozen, copy.deepcopy(self.frozen)))

    def test_a_baseline_already_approved_passes_an_identical_approved_row(self):
        self.assertEqual((0, []), gate.check_development_ledger(
            self.approved, copy.deepcopy(self.approved)))
        current = self.approved + [row("KMG.Added.New", "e" * 32)]
        self.assertEqual((1, []), gate.check_development_ledger(self.approved, current))

    def test_approved_back_to_frozen_fails(self):
        self.refuses(self.approved, self.frozen)

    def test_unrecognized_wording_fails_in_either_place(self):
        for before, after in (
                (FROZEN_NOTE, "Stable non-consumable Gunsmith's Kit."),
                (FROZEN_NOTE, APPROVED_NOTE + " "),
                (FROZEN_NOTE, APPROVED_NOTE.lower()),
                (FROZEN_NOTE, APPROVED_NOTE.replace("Broken-to-Normal", "Broken to Normal")),
                (FROZEN_NOTE, ""),
                (FROZEN_NOTE + " ", APPROVED_NOTE),
                ("Some other released wording.", APPROVED_NOTE),
                (APPROVED_NOTE + " (draft)", APPROVED_NOTE)):
            self.refuses(ledger(before), ledger(after))

    def test_the_approved_wording_gains_nothing_on_another_row_or_position(self):
        # Another record at position 206 taking the approved note.
        released = ledger(FROZEN_NOTE)
        current = ledger(APPROVED_NOTE)
        released[KIT_POSITION] = dict(released[KIT_POSITION - 1])
        released[KIT_POSITION - 1] = dict(KIT, notes=FROZEN_NOTE)
        current[KIT_POSITION] = dict(released[KIT_POSITION], notes=APPROVED_NOTE)
        current[KIT_POSITION - 1] = dict(KIT, notes=FROZEN_NOTE)
        self.refuses(released, current)
        # The Kit record itself at another position.
        released = ledger(FROZEN_NOTE)
        current = ledger(APPROVED_NOTE)
        for rows in (released, current):
            rows[KIT_POSITION - 1], rows[KIT_POSITION] = rows[KIT_POSITION], rows[KIT_POSITION - 1]
        self.refuses(released, current)
        # The approved wording on another record, Kit unchanged or corrected.
        for kit_note in (FROZEN_NOTE, APPROVED_NOTE):
            current = ledger(kit_note)
            current[10]["notes"] = APPROVED_NOTE
            self.refuses(self.frozen, current)
        # The frozen wording on another record changing to the approved one.
        released = ledger(FROZEN_NOTE)
        current = ledger(APPROVED_NOTE)
        released[10]["notes"] = FROZEN_NOTE
        current[10]["notes"] = APPROVED_NOTE
        self.refuses(released, current)

    def test_any_other_field_of_the_kit_record_fails(self):
        changes = {"symbol": "KMG.Gunsmithing.GunsmithKit2",
                   "guid": "d52aacb753434691b1ed85a16cc87105",
                   "plannedType": "BlueprintItemEquipment", "status": "retired",
                   "milestone": "Seventh Playtest"}
        for field, value in changes.items():
            for note in (APPROVED_NOTE, FROZEN_NOTE):
                current = ledger(note)
                current[KIT_POSITION][field] = value
                self.refuses(self.frozen, current)

    def test_another_released_change_fails_beside_the_correction(self):
        for field, value in (("notes", "fixture note 10 changed"),
                             ("guid", "f" * 32), ("symbol", "KMG.Fixture.Other"),
                             ("plannedType", "BlueprintItem"), ("status", "reserved"),
                             ("milestone", "Sprint 2")):
            current = copy.deepcopy(self.approved)
            current[10][field] = value
            self.refuses(self.frozen, current)

    def test_reorder_removal_truncation_malformed_and_duplicates_fail(self):
        current = copy.deepcopy(self.approved)
        current[3], current[4] = current[4], current[3]
        self.refuses(self.frozen, current)
        self.refuses(self.frozen, self.approved[:3] + self.approved[4:])
        self.refuses(self.frozen, self.approved[:KIT_POSITION + 1])
        current = copy.deepcopy(self.approved)
        del current[20]["notes"]
        self.refuses(self.frozen, current)
        current = copy.deepcopy(self.approved)
        current[20]["extra"] = 1
        self.refuses(self.frozen, current)
        current = copy.deepcopy(self.approved)
        current[20]["guid"] = "BAD"
        self.refuses(self.frozen, current)
        self.refuses(self.frozen, self.approved + [row("KMG.Added.New", KIT["guid"])])
        self.refuses(self.frozen, self.approved + [row(KIT["symbol"], "e" * 32)])

    def test_the_historical_comparator_is_unchanged(self):
        # The release validators import check_append_only; it still allows
        # no released row to change, including this one.
        with self.assertRaises(AssertionError):
            gate.check_append_only(self.frozen, self.approved)
        self.assertEqual(0, gate.check_append_only(self.frozen, copy.deepcopy(self.frozen)))


class ReleasedLedgerCorrectionTests(unittest.TestCase):
    """The authorization against the actual released ledger of v0.0.191."""

    @classmethod
    def setUpClass(cls):
        found = subprocess.run(["git", "-c", "core.longpaths=true", "show",
                                "v0.0.191:blueprints/blueprints.json"],
                               cwd=ROOT, capture_output=True, check=False)
        cls.released = None if found.returncode else \
            json.loads(found.stdout.decode("utf-8-sig"))["entries"]

    def setUp(self):
        if self.released is None:
            self.skipTest("release tag v0.0.191 is not available; fetch it with "
                          "git fetch --tags origin")

    def test_the_released_row_is_the_authorized_before(self):
        self.assertEqual(dict(KIT, notes=FROZEN_NOTE), self.released[KIT_POSITION])

    def test_the_released_ledger_against_itself_passes(self):
        self.assertEqual((0, []), gate.check_development_ledger(
            self.released, copy.deepcopy(self.released)))

    def test_the_released_ledger_with_only_the_correction_passes(self):
        current = copy.deepcopy(self.released)
        current[KIT_POSITION]["notes"] = APPROVED_NOTE
        appended, corrections = gate.check_development_ledger(self.released, current)
        self.assertEqual(0, appended)
        self.assertEqual([(KIT_POSITION, ["notes"])],
                         [(value["position"], value["fields"]) for value in corrections])

    def test_the_released_ledger_with_one_more_note_change_fails(self):
        current = copy.deepcopy(self.released)
        current[KIT_POSITION]["notes"] = APPROVED_NOTE
        current[KIT_POSITION + 1]["notes"] += " changed"
        with self.assertRaises(AssertionError):
            gate.check_development_ledger(self.released, current)


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


class ReleasedPublicationTests(unittest.TestCase):
    released = {"deinonychus", "tyrannosaurus", "elephant"}
    current = released | {"gorgon", "bulette"}

    def test_new_withheld_pair_preserves_every_released_creature(self):
        gate.check_released_publication(self.released, self.current, set(), {"gorgon", "bulette"})

    def test_public_candidate_with_no_withheld_keys_passes(self):
        gate.check_released_publication(self.released, self.current, set(), set())

    def test_one_or_both_released_theropods_cannot_be_hidden(self):
        for keys in ({"deinonychus"}, {"deinonychus", "tyrannosaurus"}):
            with self.assertRaisesRegex(AssertionError, "Released public creatures"):
                gate.check_released_publication(self.released, self.current, set(), keys)

    def test_other_released_creature_cannot_be_hidden(self):
        with self.assertRaisesRegex(AssertionError, "Released public creatures"):
            gate.check_released_publication(self.released, self.current, set(), {"elephant", "gorgon"})

    def test_owner_withdrawn_released_creature_is_the_only_exemption(self):
        released = self.released | {"stirge"}
        current = self.current | {"stirge"}
        gate.check_released_publication(released, current, set(), {"stirge"})
        self.assertEqual({"stirge"}, set(gate.OWNER_WITHDRAWN_RELEASED))
        with self.assertRaisesRegex(AssertionError, "Released public creatures became suppressed: elephant"):
            gate.check_released_publication(released, current, set(), {"stirge", "elephant"})

    def test_owner_withdrawn_key_must_still_be_registered(self):
        with self.assertRaisesRegex(AssertionError, "no registered creature"):
            gate.check_released_publication(self.released, self.current, set(), {"stirge"})

    def test_unregistered_suppression_key_is_refused(self):
        with self.assertRaisesRegex(AssertionError, "no registered creature"):
            gate.check_released_publication(self.released, self.current, set(), {"unknown"})

    def test_missing_released_catalog_key_is_refused(self):
        with self.assertRaisesRegex(AssertionError, "disappeared"):
            gate.check_released_publication(self.released, self.current - {"elephant"}, set(), {"gorgon"})


if __name__ == "__main__":
    unittest.main()
