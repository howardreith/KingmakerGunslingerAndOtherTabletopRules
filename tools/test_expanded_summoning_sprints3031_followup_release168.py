"""Behavioral corruption fixtures for the v0.0.168 Sprints 30-31 follow-up release boundary."""
import copy
import sys
import unittest

sys.dont_write_bytecode = True
import validate_expanded_summoning_sprints3031_followup_release168 as gate


def row(symbol, number):
    return dict(symbol=symbol, guid="%032x" % number, plannedType="BlueprintAbility",
                status="active", milestone="m", notes="")


def creature(monster, ally):
    return {"monster": monster, "ally": ally}


# The released v0.0.167 roster: (Summon Monster tier, Summon Nature's Ally
# tier) and how many creatures have it.
RELEASED_ROSTER = [((4, 4), 26), ((2, 2), 11), ((3, 3), 11), ((6, 6), 8), ((7, 7), 8),
                   ((1, 1), 7), ((5, 5), 7), ((6, None), 5), ((None, 6), 5),
                   ((None, 4), 4), ((8, 8), 4), ((5, None), 3), ((None, 5), 3),
                   ((None, 7), 3), ((9, None), 3), ((3, None), 2), ((4, None), 2),
                   ((None, 8), 2), ((None, 9), 2), ((2, 1), 1), ((None, 3), 1),
                   ((7, None), 1), ((None, 1), 1), ((8, None), 1), ((2, None), 1)]


class Sprints3031FollowupReleaseTests(unittest.TestCase):
    def setUp(self):
        self.released_entries = [row("KMG.Released.One", 1), row("KMG.Released.Two", 2)]
        self.released = {"version": "0.0.167", "milestone": "0.0.167-x",
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
        # 122 creatures: 101 Summon Monster rows worth 568 and 104 Summon
        # Nature's Ally rows worth 582, plus 29 native wrappers, make 1,179.
        self.creatures = [creature(*tiers) for tiers, count in RELEASED_ROSTER
                          for _ in range(count)]

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
        gate.check_production_scope(sorted(gate.RELEASE_PRODUCTION | gate.VERSION_PRODUCTION |
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

    def test_pinned_assets_pass(self):
        gate.check_art(self.changed, self.digest)

    def test_unpinned_asset_fails(self):
        changed = dict(self.changed)
        changed["assets/sprint17-serpents/viper-mesh.json"] = "M"
        with self.assertRaises(AssertionError):
            gate.check_art(changed, self.digest)

    def test_deleted_or_renamed_pin_fails(self):
        path = sorted(gate.ART_MODIFIED)[0]
        for status in ("A", "D", "R"):
            changed = dict(self.changed)
            changed[path] = status
            with self.assertRaises(AssertionError):
                gate.check_art(changed, self.digest)

    def test_missing_pinned_asset_fails(self):
        changed = dict(self.changed)
        del changed[sorted(gate.ART_MODIFIED)[0]]
        with self.assertRaises(AssertionError):
            gate.check_art(changed, self.digest)

    def test_altered_asset_bytes_fail(self):
        path = sorted(gate.ART)[0]
        self.digests[path] = "0" * 64
        with self.assertRaises(AssertionError):
            gate.check_art(self.changed, self.digest)

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
            gate.check_census(self.creatures, ["lemure"])

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
            gate.check_version("0.0.167", self.props, self.assembly)

    def test_stale_informational_version_fails(self):
        with self.assertRaises(AssertionError):
            gate.check_version(gate.VERSION, self.props.replace(
                gate.INFORMATIONAL, "0.0.168-other"), self.assembly)


if __name__ == "__main__":
    unittest.main()
