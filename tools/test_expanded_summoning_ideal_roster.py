"""Placement-level coverage fixtures for the ideal-roster reporter (INT-08).

Every case runs the production readers and comparison on the real source
texts, either as they are or changed in one stated way in memory. No source
file and no generated document is written, except into a temporary directory.

The expected discrepancies are written out here placement by placement, not
derived from the code under test.
"""
import sys
import tempfile
import unittest
from pathlib import Path

sys.dont_write_bytecode = True
import expanded_summoning_ideal_roster as roster  # noqa: E402

P = roster.Placement
D41 = "OneD4PlusOne"
EXPECTED_MISSING = {
    P("brachiosaurus", "sm", 7, "One"), P("brachiosaurus", "sm", 8, "OneD3"),
    P("brachiosaurus", "sm", 9, D41),
    P("redcap", "sm", 8, D41), P("redcap", "sm", 9, D41),
    P("axiomite", "sm", 9, D41), P("soul-eater", "sm", 9, D41),
    P("manticore", "sna", 8, D41), P("manticore", "sna", 9, D41),
    P("mite", "sna", 4, D41), P("mite", "sna", 5, D41), P("mite", "sna", 6, D41),
    P("mite", "sna", 7, D41), P("mite", "sna", 8, D41), P("mite", "sna", 9, D41),
}
EXPECTED_WITHHELD = {P("stirge", "sna", 1, "One"), P("stirge", "sna", 2, "OneD3")} | {
    P("stirge", "sna", parent, D41) for parent in range(3, 10)}
BRACHIOSAURUS_SNA = {P("brachiosaurus", "sna", 7, "One"), P("brachiosaurus", "sna", 8, "OneD3"),
                     P("brachiosaurus", "sna", 9, D41)}
SOURCES = roster.read_sources()


def replace(text, old, new):
    assert text.count(old) == 1, (old, text.count(old))
    return text.replace(old, new)


def judge(**changes):
    """Coverage of the real sources with the named texts replaced."""
    sources = dict(SOURCES, **changes)
    entries = roster.read_ideal(sources["ideal"])
    return roster.coverage(entries, sources["project"], sources["wrappers"],
                           sources["visibility"], sources["identities"], sources["ledger"])


def judge_parts(creatures=None, wrappers=None, suppressed=None, symbols=None):
    """The comparison on parsed inputs changed directly, for changes that a
    one-line text edit cannot express."""
    entries = roster.read_ideal(SOURCES["ideal"])
    creatures = creatures if creatures is not None else roster.read_project(SOURCES["project"])
    wrappers = wrappers if wrappers is not None else roster.read_wrappers(SOURCES["wrappers"])
    suppressed = suppressed if suppressed is not None else roster.read_suppressed(SOURCES["visibility"])
    choices = roster.declared_choices(creatures, wrappers, suppressed)
    if symbols is None:
        symbols = roster.read_registered_symbols(SOURCES["ledger"])
    declared = roster.read_declared(SOURCES["identities"], SOURCES["visibility"], SOURCES["project"])
    declared["project_creatures"] = len(creatures)
    return roster.compare(roster.ideal_placements(entries), choices, symbols, declared)


def unpublished(cov, state=None):
    return {u[0] for u in cov["unpublished"] if state is None or u[1] == state}


def disposition(cov, placement):
    return next(u[2] for u in cov["unpublished"] if u[0] == placement)


class CurrentSourceTests(unittest.TestCase):
    def setUp(self):
        self.cov = judge()

    def test_totals_reconcile_with_the_independent_counts(self):
        t = self.cov["totals"]
        self.assertEqual((1232, 1188, 29, 1217, 1208, 9),
                         (t["ideal"], t["project"], t["wrapper"], t["declared"], t["admitted"],
                          t["suppressed_choices"]))
        self.assertEqual((1208, 9, 15, 0, 0, 0, 0),
                         (t["covered"], t["withheld"], t["missing"], t["extra"], t["duplicates"],
                          t["unregistered"], t["unexplained"]))
        self.assertTrue(all(stated == derived for _label, stated, derived in self.cov["cross"]))

    def test_each_discrepancy_is_the_named_placement(self):
        self.assertEqual(EXPECTED_MISSING, unpublished(self.cov, "missing"))
        self.assertEqual(EXPECTED_WITHHELD, unpublished(self.cov, "withheld"))

    def test_dispositions_are_kept_apart(self):
        for placement in EXPECTED_MISSING:
            expected = "POL-04" if placement.key == "brachiosaurus" else "POL-05"
            self.assertTrue(disposition(self.cov, placement).startswith(expected), placement)
        for placement in EXPECTED_WITHHELD:
            self.assertTrue(disposition(self.cov, placement).startswith("Owner withdrawal"), placement)
        self.assertTrue(all(u.recorded for u in self.cov["unpublished"]))
        self.assertEqual([], self.cov["stale_dispositions"])

    def test_positive_ladders_are_complete(self):
        for placement in BRACHIOSAURUS_SNA:
            self.assertEqual("covered", self.cov["states"][placement])
        status = self.cov["family_status"]
        self.assertEqual("Complete 3/3", status["brachiosaurus"]["sna"])
        self.assertEqual({"sm": "Complete 3/3", "sna": "Complete 3/3"}, status["tyrannosaurus"])
        self.assertEqual({"sm": "Complete 2/2", "sna": "Complete 3/3"}, status["frost-giant"])
        self.assertEqual({"sm": "Complete 3/3", "sna": "-"}, status["bogeyman"])
        self.assertEqual("Withheld 0/9", status["stirge"]["sna"])
        self.assertEqual("Partial 3/9 (6 missing)", status["mite"]["sna"])

    def test_optional_provider_choices_are_not_core_coverage(self):
        # Spider Summoner rows (Races Unleashed) are not in either catalog read here.
        choices = roster.declared_choices(roster.read_project(SOURCES["project"]),
                                          roster.read_wrappers(SOURCES["wrappers"]),
                                          roster.read_suppressed(SOURCES["visibility"]))
        self.assertFalse([c for c in choices if c.symbol.startswith("KMG.Summoning.Spider.")])
        self.assertEqual({"project", "native-wrapper"}, {c.source for c in choices})


class MutationTests(unittest.TestCase):
    def test_registration_without_a_choice_in_the_family_does_not_cover_it(self):
        cov = judge()
        self.assertEqual("ProjectOwned", cov["identity"]["brachiosaurus"])
        self.assertEqual("Missing 0/3", cov["family_status"]["brachiosaurus"]["sm"])
        # Tyrannosaurus keeps its identity and its Nature's Ally ladder but
        # loses its Summon Monster tier.
        cov = judge(project=replace(SOURCES["project"], 'C("tyrannosaurus","Tyrannosaurus",7,',
                                    'C("tyrannosaurus","Tyrannosaurus",null,'))
        lost = {P("tyrannosaurus", "sm", 7, "One"), P("tyrannosaurus", "sm", 8, "OneD3"),
                P("tyrannosaurus", "sm", 9, D41)}
        self.assertEqual(EXPECTED_MISSING | lost, unpublished(cov, "missing"))
        self.assertEqual("ProjectOwned", cov["identity"]["tyrannosaurus"])
        self.assertEqual({"sm": "Missing 0/3", "sna": "Complete 3/3"}, cov["family_status"]["tyrannosaurus"])
        for placement in lost:
            self.assertTrue(disposition(cov, placement).startswith("Unexplained"))
        self.assertEqual(3, cov["totals"]["unexplained"])

    def test_a_missing_parent_tier_stays_missing_beside_the_others(self):
        cov = judge(wrappers=replace(
            SOURCES["wrappers"],
            '            N(8,"Axiomite","Axiomite",SummonMultiplicity.OneD4PlusOne,\n'
            '                "12815be343acc4c45b24554089c8c4bc","5adafaa653dc4de40b3ac73fa002a2ec",'
            'SummonNativeSpawnBranch.NonEvil),\n', ""))
        self.assertIn(P("axiomite", "sm", 8, D41), unpublished(cov, "missing"))
        self.assertTrue(disposition(cov, P("axiomite", "sm", 8, D41)).startswith("Unexplained"))
        self.assertEqual("covered", cov["states"][P("axiomite", "sm", 6, "One")])
        self.assertEqual("covered", cov["states"][P("axiomite", "sm", 7, "OneD3")])
        self.assertEqual("Partial 2/4 (2 missing)", cov["family_status"]["axiomite"]["sm"])

    def test_a_wrong_quantity_fails_its_placement(self):
        cov = judge(wrappers=replace(SOURCES["wrappers"],
                                     'N(6,"Redcap","Redcap",SummonMultiplicity.OneD3,',
                                     'N(6,"Redcap","Redcap",SummonMultiplicity.One,'))
        self.assertIn(P("redcap", "sm", 6, "OneD3"), unpublished(cov, "missing"))
        self.assertEqual([P("redcap", "sm", 6, "One")], [c.placement for c in cov["extra"]])
        self.assertEqual(["KMG.Summoning.NativeOption.SM.Tier6.Redcap.One"],
                         [c.symbol for c in cov["unregistered"]])
        self.assertEqual(29, cov["totals"]["wrapper"])

    def test_same_count_replacements_cannot_pass_through_totals(self):
        base = judge_parts()
        cases = {
            "family": (P("thanadaemon", "sm", 9, "One"), P("thanadaemon", "sna", 9, "One"),
                       lambda w: w.update(family="sna") if w["key"] == "thanadaemon" else None),
            "tier": (P("nereid", "sna", 8, "One"), P("nereid", "sna", 7, "One"),
                     lambda w: w.update(tier=7) if (w["key"], w["tier"]) == ("nereid", 8) else None),
            "creature": (P("hamadryad", "sna", 9, "One"), P("nereid", "sna", 9, "One"),
                         lambda w: w.update(key="nereid") if w["key"] == "hamadryad" else None),
        }
        for name, (lost, added, change) in cases.items():
            with self.subTest(name):
                wrappers = roster.read_wrappers(SOURCES["wrappers"])
                for w in wrappers:
                    change(w)
                cov = judge_parts(wrappers=wrappers)
                self.assertEqual(base["totals"]["admitted"], cov["totals"]["admitted"])
                self.assertEqual(base["totals"]["declared"], cov["totals"]["declared"])
                self.assertEqual(base["totals"]["covered"] - 1, cov["totals"]["covered"])
                self.assertIn(lost, unpublished(cov, "missing"))
                self.assertEqual([added], [c.placement for c in cov["extra"]])

    def test_duplicate_choices_stay_visible(self):
        bogeyman = ('            N(7,"Bogeyman","Bogeyman",SummonMultiplicity.One,\n'
                    '                "2920d48574933c24391fbb9e18f87bf5","4f04734e4ad930e438eec3182dfd95fe",'
                    'SummonNativeSpawnBranch.Direct),\n')
        cov = judge(wrappers=replace(SOURCES["wrappers"], bogeyman, bogeyman + bogeyman))
        self.assertEqual(1, cov["totals"]["duplicates"])
        placement, symbols = cov["duplicates"][0]
        self.assertEqual(P("bogeyman", "sm", 7, "One"), placement)
        self.assertEqual(["KMG.Summoning.NativeOption.SM.Tier7.Bogeyman.One"] * 2, symbols)
        self.assertEqual((30, 1218), (cov["totals"]["wrapper"], cov["totals"]["declared"]))
        # A wrapper duplicating a project choice is also visible.
        mite = '            A(1,"Mite","mite","Mite",SummonMultiplicity.One,\n'
        dog = ('            N(1,"Dog","Dog",SummonMultiplicity.One,\n'
               '                "00000000000000000000000000000000","00000000000000000000000000000000",'
               'SummonNativeSpawnBranch.Direct),\n')
        cov = judge(wrappers=replace(SOURCES["wrappers"], mite, dog + mite))
        self.assertEqual([(P("dog", "sm", 1, "One"), ["KMG.Summoning.Ability.SM.Tier1.Dog.One",
                                                       "KMG.Summoning.NativeOption.SM.Tier1.Dog.One"])],
                         cov["duplicates"])

    def test_suppression_is_distinct_from_absent_registration(self):
        cov = judge(visibility=replace(SOURCES["visibility"], 'new[] { "stirge" }',
                                       'new[] { "stirge", "brachiosaurus" }'))
        for placement in BRACHIOSAURUS_SNA:
            self.assertEqual("withheld", cov["states"][placement])
            self.assertFalse(next(u for u in cov["unpublished"] if u.placement == placement).recorded)
        for placement in {p for p in EXPECTED_MISSING if p.key == "brachiosaurus"}:
            self.assertEqual("missing", cov["states"][placement])
        # Stirge's registration removed altogether: missing, not withheld.
        cov = judge(project=replace(SOURCES["project"], 'C("stirge","Stirge",null,false,1),', ""),
                    visibility=replace(SOURCES["visibility"], 'new[] { "stirge" }', "new string[0]"))
        self.assertEqual(set(), unpublished(cov, "withheld"))
        self.assertEqual(EXPECTED_MISSING | EXPECTED_WITHHELD, unpublished(cov, "missing"))
        for placement in EXPECTED_WITHHELD:
            self.assertTrue(disposition(cov, placement).startswith("Unexplained"))

    def test_complete_valid_placements_pass(self):
        creatures = roster.read_project(SOURCES["project"])
        next(c for c in creatures if c["key"] == "brachiosaurus")["sm"] = 7
        wrappers = roster.read_wrappers(SOURCES["wrappers"])
        for p in EXPECTED_MISSING - {p for p in EXPECTED_MISSING if p.key == "brachiosaurus"}:
            wrappers.append({"family": p.family, "tier": p.parent, "key": p.key, "quantity": p.quantity,
                             "symbol": "fixture." + "/".join(map(str, p))})
        choices = roster.declared_choices(creatures, wrappers, set())
        cov = judge_parts(creatures=creatures, wrappers=wrappers, suppressed=set(),
                          symbols={c.symbol for c in choices})
        t = cov["totals"]
        self.assertEqual((1232, 0, 0, 0, 0, 0), (t["covered"], t["withheld"], t["missing"], t["extra"],
                                                 t["duplicates"], t["unregistered"]))
        self.assertEqual(sorted(EXPECTED_MISSING), cov["stale_dispositions"])
        self.assertIn("is complete", roster.coverage_summary(cov))


def withheld_brachiosaurus(sources):
    """Brachiosaurus withheld beside Stirge, with the source's own withheld
    count moved from 9 to 12 so the inputs stay coherent."""
    s = dict(sources)
    s["visibility"] = replace(replace(s["visibility"], 'new[] { "stirge" }',
                                      'new[] { "stirge", "brachiosaurus" }'),
                              "SuppressedLogicalPlacementCount = 9;", "SuppressedLogicalPlacementCount = 12;")
    return s


def stirge_at_tier_two(sources):
    """Stirge still withdrawn, registered at Nature's Ally II instead of I, with
    the source's own totals moved to match (1,188 to 1,187 registered, 9 to 8
    withheld, 588 to 587 in Nature's Ally). The ideal roster is unchanged."""
    s = dict(sources)
    s["project"] = replace(replace(s["project"], 'C("stirge","Stirge",null,false,1)',
                                   'C("stirge","Stirge",null,false,2)'),
                           "ValidateFamily(SummonFamily.NaturesAlly, 105, 588)",
                           "ValidateFamily(SummonFamily.NaturesAlly, 105, 587)")
    s["visibility"] = replace(replace(s["visibility"], "RegisteredLogicalPlacementCount = 1188;",
                                      "RegisteredLogicalPlacementCount = 1187;"),
                              "SuppressedLogicalPlacementCount = 9;", "SuppressedLogicalPlacementCount = 8;")
    return s


def rows(document, prefix):
    return [line for line in document.splitlines() if line.startswith(prefix)]


class ReviewControlTests(unittest.TestCase):
    """The review's two summary failures, through the production build and
    generation path on coherent in-memory sources."""

    def test_unrecorded_withholding_counts_as_unexplained(self):
        _entries, _d, cov, document, _trace = roster.build(withheld_brachiosaurus(SOURCES))
        unexplained = [u for u in cov["unpublished"] if not u.recorded]
        self.assertEqual(BRACHIOSAURUS_SNA, {u.placement for u in unexplained})
        self.assertTrue(all(u.state == "withheld" for u in unexplained))
        self.assertEqual((3, 3, 0), (cov["totals"]["unexplained"], cov["totals"]["unexplained_withheld"],
                                     cov["totals"]["unexplained_missing"]))
        for placement in EXPECTED_WITHHELD:
            u = next(x for x in cov["unpublished"] if x.placement == placement)
            self.assertEqual(("withheld", True), (u.state, u.recorded))
            self.assertTrue(u.disposition.startswith("Owner withdrawal"))
        for placement in {p for p in EXPECTED_MISSING if p.key == "brachiosaurus"}:
            u = next(x for x in cov["unpublished"] if x.placement == placement)
            self.assertEqual(("missing", True), (u.state, u.recorded))
        self.assertEqual((1205, 12, 15), (cov["totals"]["covered"], cov["totals"]["withheld"],
                                          cov["totals"]["missing"]))
        # Summary and rendered tables agree with the structured result.
        self.assertIn("12 withheld; 15 missing; 0 extra; 0 duplicate; 3 unexplained",
                      roster.coverage_summary(cov))
        self.assertEqual(["| Unpublished placements without a recorded disposition | 3 (3 withheld, 0 missing) |"],
                         rows(document, "| Unpublished placements without a recorded disposition"))
        brachiosaurus = rows(document, "| Brachiosaurus | Summon Nature's Ally |")
        self.assertEqual(3, len(brachiosaurus))
        self.assertTrue(all("| Registered, withheld | no | Unexplained" in row for row in brachiosaurus))
        self.assertEqual(9, len([row for row in rows(document, "| Stirge | Summon Nature's Ally |")
                                 if "| Registered, withheld | yes | Owner withdrawal" in row]))

    def test_mixed_missing_and_withheld_brachiosaurus_is_not_called_unregistered(self):
        _entries, _d, cov, document, _trace = roster.build(withheld_brachiosaurus(SOURCES))
        self.assertEqual({"sm": {"covered": 0, "withheld": 0, "missing": 3, "total": 3},
                          "sna": {"covered": 0, "withheld": 3, "missing": 0, "total": 3}},
                         cov["placement_counts"]["brachiosaurus"])
        self.assertEqual({"sm": "Missing 0/3", "sna": "Withheld 0/3"}, cov["family_status"]["brachiosaurus"])
        self.assertEqual("mixed", cov["creature_category"]["brachiosaurus"])
        self.assertEqual(143, cov["available_somewhere"])
        self.assertEqual(["| No ideal placement published; some registered and withheld, some with no "
                          "declared choice | 1 | Brachiosaurus |"],
                         rows(document, "| No ideal placement published; some registered"))
        self.assertEqual(["| No ideal placement published or registered | 0 |  |"],
                         rows(document, "| No ideal placement published or registered"))
        self.assertNotIn("Brachiosaurus", rows(document, "| Some ideal placements published")[0])

    def test_withdrawn_stirge_with_a_shifted_tier_is_not_available(self):
        _entries, _d, cov, document, _trace = roster.build(stirge_at_tier_two(SOURCES))
        # Registered at II, Stirge's choices are II single, III 1d3 and IV-IX
        # 1d4+1. Against its ideal I single, II 1d3 and III-IX 1d4+1, six
        # match (withheld), three have no choice and two choices are extra.
        self.assertEqual({"sna": {"covered": 0, "withheld": 6, "missing": 3, "total": 9}, "sm": None},
                         cov["placement_counts"]["stirge"])
        self.assertEqual({P("stirge", "sna", 1, "One"), P("stirge", "sna", 2, "OneD3"),
                          P("stirge", "sna", 3, D41)},
                         {u.placement for u in cov["unpublished"] if u.placement.key == "stirge"
                          and u.state == "missing"})
        self.assertEqual(["KMG.Summoning.Ability.SNA.Tier2.Stirge.One",
                          "KMG.Summoning.Ability.SNA.Tier3.Stirge.OneD3"], [c.symbol for c in cov["extra"]])
        self.assertEqual("Unpublished 0/9 (6 withheld, 3 missing)", cov["family_status"]["stirge"]["sna"])
        self.assertEqual("mixed", cov["creature_category"]["stirge"])
        self.assertEqual(144, cov["available_somewhere"])
        self.assertIn("Available somewhere: 144 creatures", document)
        self.assertNotIn("Stirge", rows(document, "| Some ideal placements published")[0])
        self.assertEqual(1, len([row for row in rows(document, "| Stirge | - | 1 |")
                                 if "| Unpublished 0/9 (6 withheld, 3 missing) |" in row]))
        self.assertEqual((3, 0, 3), (cov["totals"]["unexplained"], cov["totals"]["unexplained_withheld"],
                                     cov["totals"]["unexplained_missing"]))

    def test_creature_categories_follow_the_placement_counts(self):
        cov = judge()
        self.assertEqual("complete", cov["creature_category"]["tyrannosaurus"])
        self.assertEqual("partial", cov["creature_category"]["mite"])
        self.assertEqual("withheld", cov["creature_category"]["stirge"])
        self.assertEqual(144, cov["available_somewhere"])
        # A creature with no choice at all: Hamadryad's only wrapper removed.
        cov = judge(wrappers=replace(
            SOURCES["wrappers"],
            '            A(9,"Hamadryad","hamadryad","Hamadryad",SummonMultiplicity.One,\n'
            '                "f6751c3b22dbd884093e350a37420368","32a7776fb5bb9fa408b97757c04d4247"),\n', ""))
        self.assertEqual("missing", cov["creature_category"]["hamadryad"])
        self.assertEqual("Missing 0/1", cov["family_status"]["hamadryad"]["sna"])
        self.assertEqual(143, cov["available_somewhere"])
        for key, counts in cov["placement_counts"].items():
            category = cov["creature_category"][key]
            covered = sum(c["covered"] for c in counts.values() if c)
            self.assertEqual(covered > 0, category in ("complete", "partial"), key)


class ReadDiagnosticTests(unittest.TestCase):
    def refused(self, **changes):
        messages = []
        code = roster.run(True, sources=dict(SOURCES, **changes), out=messages.append)
        self.assertEqual(1, code)
        self.assertTrue(messages[0].startswith("Ideal roster input could not be read completely"), messages)
        return messages[0]

    def test_malformed_wrapper_lines_are_named(self):
        message = self.refused(wrappers=replace(SOURCES["wrappers"],
                                                'N(6,"Redcap","Redcap",SummonMultiplicity.OneD3,',
                                                'N(6,"Redcap","Redcap",SummonMultiplicity.Two,'))
        text = SOURCES["wrappers"]
        line = text[:text.index('N(6,"Redcap","Redcap",SummonMultiplicity.OneD3,')].count(chr(10)) + 1
        self.assertIn("line %d:" % line, message)
        message = self.refused(wrappers=replace(SOURCES["wrappers"], 'N(7,"Bogeyman","Bogeyman",',
                                                'N(7,Bogeyman,"Bogeyman",'))
        self.assertIn("1 factory calls could not be read", message)

    def test_incompletely_read_catalogs_are_refused(self):
        self.assertIn("144 of 145", self.refused(ideal=replace(
            SOURCES["ideal"], 'R("stirge","Stirge",null,1,', 'R("stirge","Stirge",nul,1,')))
        self.assertIn("133 of 134", self.refused(project=replace(
            SOURCES["project"], 'C("dog","Dog",1,true,1)', 'C("dog","Dog",one,true,1)')))
        self.assertIn("SuppressedCreatureKeys", self.refused(visibility=replace(
            SOURCES["visibility"], "SuppressedCreatureKeys =", "SuppressedKeys =")))
        self.assertIn("identity manifest", self.refused(ledger=SOURCES["ledger"][:1000]))

    def test_inconsistent_inputs_are_refused(self):
        self.assertIn("absent from the ideal roster", self.refused(wrappers=replace(
            SOURCES["wrappers"], 'N(5,"Redcap","Redcap",', 'N(5,"Goblin","Goblin",')))
        self.assertIn("RegisteredLogicalPlacementCount", self.refused(visibility=replace(
            SOURCES["visibility"], "RegisteredLogicalPlacementCount = 1188;",
            "RegisteredLogicalPlacementCount = 1189;")))


class GenerationTests(unittest.TestCase):
    def test_output_is_deterministic_and_check_detects_staleness(self):
        first = roster.build(SOURCES)[3:]
        self.assertEqual(first, roster.build(dict(SOURCES))[3:])
        with tempfile.TemporaryDirectory() as directory:
            roster_path = Path(directory) / "roster.md"
            trace_path = Path(directory) / "trace.md"
            messages = []
            self.assertEqual(0, roster.run(False, SOURCES, roster_path, trace_path, messages.append))
            self.assertEqual(first, (roster_path.read_bytes().decode("utf-8"),
                                     trace_path.read_bytes().decode("utf-8")))
            messages = []
            self.assertEqual(0, roster.run(True, SOURCES, roster_path, trace_path, messages.append))
            # Fresh and incomplete are separate facts.
            self.assertIn("current", messages[0])
            self.assertIn("is incomplete", messages[1])
            roster_path.write_bytes(roster_path.read_bytes().replace(b"1,208", b"1,232", 1))
            messages = []
            self.assertEqual(1, roster.run(True, SOURCES, roster_path, trace_path, messages.append))
            self.assertIn("stale", messages[0])

    def test_the_committed_documents_are_current(self):
        messages = []
        self.assertEqual(0, roster.run(True, out=messages.append), messages)

    def test_the_ideal_row_is_labelled_ideal(self):
        document = roster.build(SOURCES)[3]
        self.assertIn("| Ideal quantity placements | 1,232 | 1,232 |", document)
        self.assertNotIn("Core visible quantity placements", document)
        self.assertIn("Coverage is **incomplete**", document)


if __name__ == "__main__":
    unittest.main()
