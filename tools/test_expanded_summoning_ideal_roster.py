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
POL04_CELLS = {
    P("brachiosaurus", "sm", 7, "One"), P("brachiosaurus", "sm", 8, "OneD3"),
    P("brachiosaurus", "sm", 9, D41)}
POL05_WRAPPER_CELLS = {
    P("redcap", "sm", 8, D41), P("redcap", "sm", 9, D41),
    P("axiomite", "sm", 9, D41), P("soul-eater", "sm", 9, D41),
    P("manticore", "sna", 8, D41), P("manticore", "sna", 9, D41),
    P("mite", "sna", 4, D41), P("mite", "sna", 5, D41), P("mite", "sna", 6, D41),
    P("mite", "sna", 7, D41), P("mite", "sna", 8, D41), P("mite", "sna", 9, D41),
}
EXPECTED_OUT_OF_SCOPE = POL04_CELLS | POL05_WRAPPER_CELLS
EXPECTED_MISSING = set()
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
        self.assertEqual((1208, 9, 0, 15, 0, 0, 0, 0),
                         (t["covered"], t["withheld"], t["missing"], t["out_of_scope"], t["extra"],
                          t["duplicates"], t["unregistered"], t["unexplained"]))
        # Every ideal placement has exactly one derived state; the four states account for the original ideal.
        states = list(self.cov["states"].values())
        self.assertEqual(t["ideal"], len(states))
        self.assertEqual((t["covered"], t["withheld"], t["missing"], t["out_of_scope"]),
                         tuple(states.count(s) for s in ("covered", "withheld", "missing", "out-of-scope")))
        self.assertTrue(all(stated == derived for _label, stated, derived in self.cov["cross"]))

    def test_each_discrepancy_is_the_named_placement(self):
        self.assertEqual(EXPECTED_MISSING, unpublished(self.cov, "missing"))
        self.assertEqual(EXPECTED_WITHHELD, unpublished(self.cov, "withheld"))
        self.assertEqual(EXPECTED_OUT_OF_SCOPE, unpublished(self.cov, "out-of-scope"))

    def test_dispositions_are_kept_apart(self):
        for placement in POL04_CELLS:
            text = disposition(self.cov, placement)
            self.assertTrue(text.startswith("Approved out of scope: POL-04"), placement)
            self.assertIn("2026-10-15", text)
            self.assertIn("https://legacy.aonprd.com/coreRulebook/spells/summonMonster.html", text)
        for placement in POL05_WRAPPER_CELLS:
            text = disposition(self.cov, placement)
            self.assertTrue(text.startswith("Approved out of scope: POL-05 / S5 T4, Alternative A"), placement)
            self.assertIn("no native option exists for this exact creature and higher tier", text)
            self.assertIn("not fulfilment of this placement", text)
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
        self.assertEqual("In scope complete 3/9 (6 approved out of scope)", status["mite"]["sna"])

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
        self.assertEqual("Approved out of scope 0/3", cov["family_status"]["brachiosaurus"]["sm"])
        self.assertFalse(any(cov["states"][p] == "covered" for p in EXPECTED_OUT_OF_SCOPE))
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
        self.assertEqual("Partial 2/4 (1 missing, 1 approved out of scope)", cov["family_status"]["axiomite"]["sm"])

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
        for placement in EXPECTED_OUT_OF_SCOPE:
            self.assertEqual("out-of-scope", cov["states"][placement])
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
        for p in POL05_WRAPPER_CELLS:
            wrappers.append({"family": p.family, "tier": p.parent, "key": p.key, "quantity": p.quantity,
                             "symbol": "fixture." + "/".join(map(str, p))})
        choices = roster.declared_choices(creatures, wrappers, set())
        cov = judge_parts(creatures=creatures, wrappers=wrappers, suppressed=set(),
                          symbols={c.symbol for c in choices})
        t = cov["totals"]
        self.assertEqual((1232, 0, 0, 0, 0, 0), (t["covered"], t["withheld"], t["missing"], t["extra"],
                                                 t["duplicates"], t["unregistered"]))
        self.assertEqual([], cov["stale_dispositions"])
        summary = roster.coverage_summary(cov)
        self.assertIn("is complete", summary)
        # Publishing the approved exclusions completes the original ideal but contradicts the decision.
        self.assertEqual(sorted(EXPECTED_OUT_OF_SCOPE), cov["scope_conflicts"])
        self.assertIn("15 SCOPE CONFLICT", summary)


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
        for placement in EXPECTED_OUT_OF_SCOPE:
            u = next(x for x in cov["unpublished"] if x.placement == placement)
            self.assertEqual(("out-of-scope", True), (u.state, u.recorded))
        self.assertEqual((1205, 12, 0, 15), (cov["totals"]["covered"], cov["totals"]["withheld"],
                                             cov["totals"]["missing"], cov["totals"]["out_of_scope"]))
        # Summary and rendered tables agree with the structured result.
        self.assertIn("12 withheld; 0 missing; 15 approved out of scope; 0 extra; 0 duplicate; 3 unexplained",
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
        self.assertEqual({"sm": {"covered": 0, "withheld": 0, "missing": 0, "out_of_scope": 3, "total": 3},
                          "sna": {"covered": 0, "withheld": 3, "missing": 0, "total": 3}},
                         cov["placement_counts"]["brachiosaurus"])
        self.assertEqual({"sm": "Approved out of scope 0/3", "sna": "Withheld 0/3"},
                         cov["family_status"]["brachiosaurus"])
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
        self.assertEqual("scoped", cov["creature_category"]["mite"])
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
            self.assertEqual(covered > 0, category in ("complete", "scoped", "partial"), key)


BRACHIOSAURUS_SM = [P("brachiosaurus", "sm", 7, "One"), P("brachiosaurus", "sm", 8, "OneD3"),
                    P("brachiosaurus", "sm", 9, D41)]
SM_SYMBOLS = ["KMG.Summoning.Ability.SM.Tier7.Brachiosaurus.One",
              "KMG.Summoning.Ability.SM.Tier8.Brachiosaurus.OneD3",
              "KMG.Summoning.Ability.SM.Tier9.Brachiosaurus.OneD4PlusOne"]
CENSUS = "20261119T201715Z-owner-0.33.0-inherited-template-census"
CASTS = "20261119T201938Z-owner-0.33.0-inherited-template-casts"
MENU = "20261116T181244Z-owner-0.33.0-summoning-publication-boundary"
S5T1_PACKAGE = "@3ef1bce7027b"
V191_PACKAGE = "@c09ca6255fee"


def rows_by_placement(rows):
    return {r.placement: r for r in rows}


def coherent_monster_brachiosaurus(sources):
    """Brachiosaurus also registered and declared in Summon Monster VII-IX, with the
    project's own totals, the visibility count and the ledger moved to match."""
    import json
    s = dict(sources)
    s["project"] = replace(replace(s["project"], 'C("brachiosaurus","Brachiosaurus",null,false,7,"Mastodon")',
                                   'C("brachiosaurus","Brachiosaurus",7,true,7,"Mastodon")'),
                           "ValidateFamily(SummonFamily.Monster, 112, 600)",
                           "ValidateFamily(SummonFamily.Monster, 113, 603)")
    s["visibility"] = replace(s["visibility"], "RegisteredLogicalPlacementCount = 1188;",
                              "RegisteredLogicalPlacementCount = 1191;")
    ledger = json.loads(s["ledger"])
    for index, symbol in enumerate(SM_SYMBOLS):
        ledger["entries"].append({"symbol": symbol, "guid": "%032x" % (index + 1), "status": "active"})
    s["ledger"] = json.dumps(ledger)
    return s


def cell_rows(table):
    return [line for line in table.splitlines() if line.startswith("| brachiosaurus | Summon Monster |")]


class DecisionPacketTests(unittest.TestCase):
    """S5 T3 / POL-04 decision rows: registered identity, declared and admitted
    choice, native evidence and original-ideal/approved scope are each read
    independently, for the exact family-specific identity only."""

    def setUp(self):
        self.rows = rows_by_placement(roster.cells(["brachiosaurus", "tyrannosaurus", "air-mephit"], SOURCES))

    def test_each_summon_monster_cell_is_reported_individually_and_excluded(self):
        for placement, symbol in zip(BRACHIOSAURUS_SM, SM_SYMBOLS):
            row = self.rows[placement]
            self.assertEqual((True, "approved out of scope", symbol), (row.ideal, row.scope, row.symbol), placement)
            self.assertEqual((False, False, False, "none", "none", "none"),
                             (row.registered, row.declared, row.admitted, row.graph, row.menu, row.cast), placement)
        rendered = cell_rows(roster.cell_table(roster.cells(["brachiosaurus"], SOURCES)))
        self.assertEqual(3, len(rendered))
        self.assertTrue(all("| yes | approved out of scope |" in row and row.endswith("| no | no | no | none | none | none |")
                            for row in rendered), rendered)

    def test_a_registered_natures_ally_identity_never_fills_a_summon_monster_cell(self):
        for placement in BRACHIOSAURUS_SNA:
            row = self.rows[placement]
            self.assertTrue(row.registered and row.admitted, placement)
            self.assertEqual(("in scope", "PASS:" + CENSUS + S5T1_PACKAGE, "PASS:" + MENU + V191_PACKAGE, "none"),
                             (row.scope, row.graph, row.menu, row.cast), placement)
        self.assertTrue(all("SNA" in self.rows[p].symbol for p in BRACHIOSAURUS_SNA))
        self.assertFalse(any(self.rows[p].registered or self.rows[p].graph != "none" or self.rows[p].menu != "none"
                             or self.rows[p].cast != "none" for p in BRACHIOSAURUS_SM),
                         "Nature's Ally identities, graphs and menu rows never reach a Monster cell.")

    def test_registration_declaration_and_native_evidence_are_independent(self):
        # Catalog only, no ledger identity: declared and admitted, never registered, no native evidence.
        project = replace(SOURCES["project"], 'C("brachiosaurus","Brachiosaurus",null,false,7,"Mastodon")',
                          'C("brachiosaurus","Brachiosaurus",7,true,7,"Mastodon")')
        rows = rows_by_placement(roster.cells(["brachiosaurus"], dict(SOURCES, project=project)))
        for placement in BRACHIOSAURUS_SM:
            row = rows[placement]
            self.assertEqual((True, True, False, "none", "none"),
                             (row.declared, row.admitted, row.registered, row.graph, row.cast), placement)

    def test_native_cast_evidence_is_exact_to_the_identity(self):
        cast = self.rows[P("air-mephit", "sm", 4, "One")]
        self.assertEqual("PASS:" + CASTS + S5T1_PACKAGE, cast.cast)
        self.assertEqual("PASS:" + MENU + V191_PACKAGE, cast.menu, "The menu row keeps its own v0.0.191 identity.")
        self.assertEqual("none", self.rows[P("air-mephit", "sna", 4, "One")].cast,
                         "A Summon Monster cast does not count for the Nature's Ally identity.")
        self.assertEqual("none", self.rows[P("tyrannosaurus", "sm", 7, "One")].cast,
                         "Graph evidence is not a cast.")
        self.assertEqual("none", self.rows[P("tyrannosaurus", "sm", 7, "One")].cast)
        self.assertTrue(all(r.menu in ("none", "PASS:" + MENU + V191_PACKAGE) for r in self.rows.values()),
                        "Menu credit comes only from the retained v0.0.191 parent census.")

    def test_a_failed_or_foreign_evidence_row_cannot_pass_a_cell(self):
        evidence = ('{"schemaVersion":1,"entries":['
                    '{"symbol":"KMG.Summoning.Ability.SNA.Tier7.Brachiosaurus.One","kind":"cast","result":"FAIL","session":"s1"},'
                    '{"symbol":"KMG.Summoning.Ability.SNA.Tier7.Brachiosaurus.One","kind":"graph","result":"PASS","session":"s3"},'
                    '{"symbol":"KMG.Summoning.Ability.SM.Tier7.Brachiosaurusx.One","kind":"cast","result":"PASS","session":"s2"}]}')
        rows = rows_by_placement(roster.cells(["brachiosaurus"], dict(SOURCES, evidence=evidence)))
        self.assertEqual("FAIL:s1", rows[P("brachiosaurus", "sna", 7, "One")].cast)
        self.assertEqual(("none", "none"), (rows[P("brachiosaurus", "sm", 7, "One")].cast,
                                            rows[P("brachiosaurus", "sm", 7, "One")].graph),
                         "A Nature's Ally graph and a misnamed cast never reach the Monster cell.")
        with self.assertRaises(roster.ReadError):
            roster.cells(["brachiosaurus"], dict(SOURCES, evidence='{"entries":[{"symbol":"x","kind":"seen","result":"PASS","session":"s"}]}'))


class ScopeDecisionTests(unittest.TestCase):
    """Alternative A through the real build, rendering and validation paths."""

    def setUp(self):
        self.saved = (dict(roster.APPROVED_OUT_OF_SCOPE), set(roster.SCOPE_PENDING))

    def tearDown(self):
        roster.APPROVED_OUT_OF_SCOPE.clear()
        roster.APPROVED_OUT_OF_SCOPE.update(self.saved[0])
        roster.SCOPE_PENDING.clear()
        roster.SCOPE_PENDING.update(self.saved[1])

    def unapproved(self):
        roster.APPROVED_OUT_OF_SCOPE.clear()
        return roster.build(SOURCES)

    def test_the_three_approvals_are_visible_and_not_called_published(self):
        _e, _d, cov, document, _trace = roster.build(SOURCES)
        self.assertEqual(sorted(EXPECTED_OUT_OF_SCOPE), sorted(cov["scope_verdicts"]))
        self.assertEqual(15, len(cov["scope_verdicts"]), "Exactly the three POL-04 and twelve POL-05 exclusions.")
        self.assertTrue(all(v == "excluded" for v in cov["scope_verdicts"].values()))
        summary = roster.coverage_summary(cov)
        self.assertIn("Source-derived coverage is incomplete: 1,208 of 1,232", summary)
        self.assertIn("0 missing; 15 approved out of scope", summary)
        self.assertIn("scope contract: 15 approved exclusions, 0 scope conflicts", summary)
        self.assertIn("Accounting against the original ideal: 1,208 admitted + 9 withheld + 0 missing + "
                      "15 approved out of scope = 1,232 ideal placements.", document)
        self.assertEqual(["| Ideal placements approved out of scope (excluded, not published) | 15 |"],
                         rows(document, "| Ideal placements approved out of scope"))
        self.assertEqual(["| Approved exclusions contradicted by a declared choice (scope conflict) | 0 |"],
                         rows(document, "| Approved exclusions contradicted"))
        excluded = rows(document, "| Brachiosaurus | Summon Monster | SM ")
        self.assertEqual(6, len(excluded), "three unpublished rows and three exclusion rows")
        self.assertEqual(3, len([r for r in excluded if "| Approved out of scope | yes | Approved out of scope: POL-04" in r]))
        self.assertEqual(3, len([r for r in excluded if "| Excluded: no declared choice; not published |" in r]))
        self.assertNotIn("Brachiosaurus", rows(document, "| Every ideal placement published |")[0])
        self.assertEqual(["| Every ideal placement in the approved scope published; the others approved out of scope, "
                          "not published | 6 | Axiomite, Brachiosaurus, Manticore, Mite, Redcap, Soul Eater |"],
                         rows(document, "| Every ideal placement in the approved scope"))
        self.assertEqual(1, len([r for r in rows(document, "| Brachiosaurus | 7 | 7 |")
                                 if "| Approved out of scope 0/3 | Complete 3/3 |" in r]))
        self.assertEqual("scoped", cov["creature_category"]["brachiosaurus"])
        messages = []
        self.assertEqual(0, roster.validate_scope(SOURCES, messages.append))
        self.assertEqual(["Scope contract valid: 15 approved exclusions, none declared, admitted or published."], messages)
        self.assertEqual(["| Some ideal placements published, some not | 0 |  |"],
                         rows(document, "| Some ideal placements published, some not"))
        self.assertEqual(12, len([r for r in excluded_rows(document) if "POL-05 / S5 T4" in r]))

    def test_approval_never_raises_the_admitted_count(self):
        approved = roster.build(SOURCES)[2]["totals"]
        unapproved = self.unapproved()[2]["totals"]
        self.assertEqual((unapproved["admitted"], unapproved["covered"]), (approved["admitted"], approved["covered"]))
        self.assertEqual(unapproved["missing"] - 15, approved["missing"])
        self.assertEqual((0, 15), (unapproved["out_of_scope"], approved["out_of_scope"]))

    def test_one_approved_cell_leaves_the_others_missing(self):
        roster.APPROVED_OUT_OF_SCOPE.clear()
        roster.APPROVED_OUT_OF_SCOPE[BRACHIOSAURUS_SM[0]] = "test decision"
        _e, _d, cov, document, _trace = roster.build(SOURCES)
        self.assertEqual({BRACHIOSAURUS_SM[0]}, unpublished(cov, "out-of-scope"))
        self.assertEqual(set(BRACHIOSAURUS_SM[1:]), unpublished(cov, "missing") & set(BRACHIOSAURUS_SM))
        self.assertEqual("Unpublished 0/3 (2 missing, 1 approved out of scope)",
                         cov["family_status"]["brachiosaurus"]["sm"])
        self.assertEqual("partial", cov["creature_category"]["brachiosaurus"])
        self.assertIn("14 missing; 1 approved out of scope; 0 extra; 0 duplicate; 14 unexplained; "
                      "scope contract: 1 approved exclusion, 0 scope conflicts", roster.coverage_summary(cov))
        self.assertEqual(2, len([r for r in rows(document, "| Brachiosaurus | Summon Monster | SM ")
                                 if "| No declared choice | no | Unexplained" in r]))

    def test_partial_wrapper_approval_leaves_the_other_wrapper_cells_missing(self):
        kept = {P("mite", "sna", 4, D41), P("redcap", "sm", 9, D41)}
        for placement in POL05_WRAPPER_CELLS - kept:
            del roster.APPROVED_OUT_OF_SCOPE[placement]
        cov = judge()
        self.assertEqual(POL04_CELLS | kept, unpublished(cov, "out-of-scope"))
        self.assertEqual(POL05_WRAPPER_CELLS - kept, unpublished(cov, "missing"))
        self.assertEqual(1208, cov["totals"]["covered"])
        self.assertEqual("Partial 3/9 (5 missing, 1 approved out of scope)", cov["family_status"]["mite"]["sna"])
        self.assertEqual("Partial 3/5 (1 missing, 1 approved out of scope)", cov["family_status"]["redcap"]["sm"])
        self.assertEqual("partial", cov["creature_category"]["mite"])
        self.assertIn("10 missing; 5 approved out of scope", roster.coverage_summary(cov))

    def test_an_approved_exclusion_with_a_coherent_monster_placement_is_a_scope_conflict(self):
        sources = coherent_monster_brachiosaurus(SOURCES)
        _e, _d, cov, document, _trace = roster.build(sources)
        self.assertEqual(sorted(POL04_CELLS), cov["scope_conflicts"])
        self.assertEqual(12, cov["totals"]["out_of_scope"], "A declared exclusion is not a resolved exclusion.")
        self.assertEqual(1211, cov["totals"]["covered"])
        self.assertEqual(0, cov["totals"]["unregistered"], "The Monster identities are coherently registered.")
        for placement in POL04_CELLS:
            self.assertEqual(("covered", "scope-conflict"), (cov["states"][placement], cov["scope_verdicts"][placement]))
        summary = roster.coverage_summary(cov)
        self.assertIn("3 SCOPE CONFLICT (declared despite approval: brachiosaurus SM VII One, "
                      "brachiosaurus SM VIII OneD3, brachiosaurus SM IX OneD4PlusOne)", summary)
        self.assertEqual(["| Approved exclusions contradicted by a declared choice (scope conflict) | 3 |"],
                         rows(document, "| Approved exclusions contradicted"))
        self.assertEqual(3, len([r for r in rows(document, "| Brachiosaurus | Summon Monster | SM ")
                                 if "**SCOPE CONFLICT**: declared despite the approval (admitted)" in r]))
        self.assertFalse([r for r in rows(document, "| Brachiosaurus | Summon Monster | SM ")
                          if "Excluded: no declared choice" in r])
        table = roster.cell_table(roster.cells(["brachiosaurus"], sources))
        self.assertTrue(all("SCOPE CONFLICT: approved out of scope but declared" in row and "| yes | yes | yes |" in row
                            for row in cell_rows(table)), table)
        messages = []
        self.assertEqual(1, roster.validate_scope(sources, messages.append))
        self.assertTrue(messages[0].startswith("Scope contract INVALID: approved out of scope but declared: "
                                               "brachiosaurus SM VII One (admitted)"), messages)
        # Freshness is a separate question and never reports scope as validated.
        freshness = []
        roster.run(True, sources, out=freshness.append)
        self.assertIn("Freshness only: the approved scope is validated by --validate-scope, not by --check.", freshness)

    def test_the_pending_unapproved_case_stays_truthful(self):
        roster.APPROVED_OUT_OF_SCOPE.clear()
        roster.SCOPE_PENDING.update(EXPECTED_OUT_OF_SCOPE)
        _e, _d, cov, document, _trace = roster.build(SOURCES)
        self.assertEqual(EXPECTED_OUT_OF_SCOPE, unpublished(cov, "missing") & EXPECTED_OUT_OF_SCOPE)
        self.assertEqual("Missing 0/3", cov["family_status"]["brachiosaurus"]["sm"])
        self.assertEqual("partial", cov["creature_category"]["brachiosaurus"])
        summary = roster.coverage_summary(cov)
        self.assertIn("9 withheld; 15 missing; 0 extra", summary)
        self.assertNotIn("approved out of scope", summary)
        self.assertNotIn("scope contract", summary)
        self.assertTrue(document.split("### Approved scope exclusions\n\n")[1].startswith("None."))
        self.assertTrue(all(r.scope == "pending owner decision" for r in
                            (rows_by_placement(roster.cells(["brachiosaurus"], SOURCES))[p] for p in BRACHIOSAURUS_SM)))
        self.assertEqual(3, len([r for r in rows(document, "| Brachiosaurus | Summon Monster | SM ")
                                 if "| No declared choice | yes | Pending owner scope decision |" in r]))

    def test_the_recorded_decision_is_exact(self):
        self.assertEqual(set(EXPECTED_OUT_OF_SCOPE), set(roster.APPROVED_OUT_OF_SCOPE))
        self.assertEqual(set(), roster.SCOPE_PENDING, "No placement awaits a decision.")
        self.assertEqual({}, roster.RECORDED_DISPOSITIONS)
        self.assertEqual({roster.POL04_DECISION, roster.POL05_DECISION}, set(roster.APPROVED_OUT_OF_SCOPE.values()))
        self.assertFalse(any(p.key not in ("brachiosaurus", "redcap", "axiomite", "soul-eater", "manticore", "mite")
                             for p in roster.APPROVED_OUT_OF_SCOPE), "No generated creature or other wrapper is restricted.")


POL05_CELLS = sorted(POL05_WRAPPER_CELLS)


def wrapper_rows(sources=SOURCES):
    return rows_by_placement(roster.cells(["redcap", "axiomite", "soul-eater", "manticore", "mite"], sources))


def excluded_rows(document):
    section = document.split("### Approved scope exclusions\n\n")[1].split("\n\n")[0]
    return [line for line in section.splitlines() if line.startswith("| ") and not line.startswith("| Creature")]


def coherent_excluded_wrappers(sources):
    """Redcap SM VIII and Mite SNA IV declared as coherent native wrappers despite their approval:
    catalog rows, the source's own wrapper count and active ledger identities all moved to match."""
    import json
    s = dict(sources)
    redcap = ('            N(7,"Redcap","Redcap",SummonMultiplicity.OneD4PlusOne,\n'
              '                "6e805a9e3ff445146a6386f5a704d4bc","af8bcff074b93c84b97938e486184222",'
              'SummonNativeSpawnBranch.Evil),\n')
    mite = ('            A(3,"Mite","mite","Mite",SummonMultiplicity.OneD4PlusOne,\n'
            '                "bb1bac85be6b1e44eafdc54a3b757c3e","0c433dcdefcaaeb4db78b07c3ebf4c94"),\n')
    s["wrappers"] = replace(replace(s["wrappers"], redcap, redcap + redcap.replace("N(7,", "N(8,")),
                            mite, mite + mite.replace("A(3,", "A(4,"))
    s["identities"] = replace(s["identities"], "NativeExpandedOptionIdentityCount = 29;",
                              "NativeExpandedOptionIdentityCount = 31;")
    ledger = json.loads(s["ledger"])
    for index, symbol in enumerate(("KMG.Summoning.NativeOption.SM.Tier8.Redcap.OneD4PlusOne",
                                    "KMG.Summoning.NativeOption.SNA.Tier4.Mite.OneD4PlusOne")):
        ledger["entries"].append({"symbol": symbol, "guid": "%032x" % (index + 101), "status": "active"})
    s["ledger"] = json.dumps(ledger)
    return s


class NativeWrapperCellTests(unittest.TestCase):
    """S5 T4 / POL-05, Alternative A: the twelve higher native-wrapper 1d4+1 cells are approved out of
    scope, each independently, from their own declarations and ledger identities only; no identity is
    inferred and no observation of another tier, quantity, family or build fills one."""

    def setUp(self):
        self.rows = wrapper_rows()

    def test_each_of_the_twelve_cells_is_an_approved_exclusion_and_undeclared(self):
        self.assertEqual(12, len(POL05_CELLS))
        for placement in POL05_CELLS:
            row = self.rows[placement]
            self.assertEqual((True, "approved out of scope", None, False, False, False, "none", "none", "none"),
                             (row.ideal, row.scope, row.symbol, row.registered, row.declared, row.admitted,
                              row.graph, row.menu, row.cast), placement)
        table = roster.cell_table(roster.cells(["redcap"], SOURCES))
        self.assertEqual(2, len([line for line in table.splitlines()
                                 if "| approved out of scope | none declared | no | no | no | none | none | none |" in line]))
        self.assertNotIn("KMG.Summoning.Ability.SM.Tier8.Redcap", table,
                         "A wrapper cell never shows a project-convention identity.")

    def test_existing_wrapper_rows_keep_their_exact_identity_and_v0191_menu_row(self):
        expected = {P("redcap", "sm", 7, D41): "KMG.Summoning.NativeOption.SM.Tier7.Redcap.OneD4PlusOne",
                    P("axiomite", "sm", 8, D41): "KMG.Summoning.NativeOption.SM.Tier8.Axiomite.OneD4PlusOne",
                    P("soul-eater", "sm", 8, D41): "KMG.Summoning.NativeOption.SM.Tier8.SoulEater.OneD4PlusOne",
                    P("manticore", "sna", 7, D41): "KMG.Summoning.NativeOption.SNA.Tier7.Manticore.OneD4PlusOne",
                    P("mite", "sna", 3, D41): "KMG.Summoning.NativeOption.SNA.Tier3.Mite.OneD4PlusOne"}
        for placement, symbol in expected.items():
            row = self.rows[placement]
            self.assertEqual(("in scope", symbol, True, True, True, "none", "PASS:" + MENU + V191_PACKAGE, "none"),
                             (row.scope, row.symbol, row.registered, row.declared, row.admitted,
                              row.graph, row.menu, row.cast), placement)
        self.assertEqual(15, len([r for r in self.rows.values() if r.declared and r.admitted]),
                         "All fifteen existing wrapper rows stay declared and admitted.")

    def test_an_observation_of_another_tier_or_quantity_never_fills_a_cell(self):
        self.assertTrue(self.rows[P("mite", "sna", 3, D41)].menu.startswith("PASS:"))
        self.assertTrue(self.rows[P("redcap", "sm", 7, D41)].menu.startswith("PASS:"))
        for placement in POL05_CELLS:
            self.assertEqual("none", self.rows[placement].menu, placement)
        evidence = ('{"entries":[{"symbol":"KMG.Summoning.NativeOption.SNA.Tier3.Mite.OneD4PlusOne","kind":"cast",'
                    '"result":"PASS","session":"other-tier","package":"x"}]}')
        rows = wrapper_rows(dict(SOURCES, evidence=evidence))
        self.assertEqual("none", rows[P("mite", "sna", 4, D41)].cast, "A tier III cast never fills tier IV.")

    def test_a_ledger_identity_without_a_declaration_stays_undeclared_and_unadmitted(self):
        import json
        ledger = json.loads(SOURCES["ledger"])
        ledger["entries"].append({"symbol": "KMG.Summoning.NativeOption.SNA.Tier4.Mite.OneD4PlusOne",
                                  "guid": "f" * 32, "status": "active"})
        sources = dict(SOURCES, ledger=json.dumps(ledger))
        rows = wrapper_rows(sources)
        row = rows[P("mite", "sna", 4, D41)]
        self.assertEqual(("KMG.Summoning.NativeOption.SNA.Tier4.Mite.OneD4PlusOne", True, False, False,
                          "approved out of scope"),
                         (row.symbol, row.registered, row.declared, row.admitted, row.scope),
                         "Registration, declaration and admission stay separate facts; registration alone is no conflict.")
        self.assertEqual((None, False), (rows[P("mite", "sna", 5, D41)].symbol, rows[P("mite", "sna", 5, D41)].registered))
        self.assertEqual([], judge(ledger=sources["ledger"])["scope_conflicts"])

    def test_the_twelve_cells_carry_the_pol05_decision_and_stirge_is_unchanged(self):
        cov = judge()
        for placement in POL05_CELLS:
            self.assertEqual(("out-of-scope", "excluded"), (cov["states"][placement], cov["scope_verdicts"][placement]))
            self.assertTrue(disposition(cov, placement).startswith("Approved out of scope: POL-05"), placement)
        self.assertEqual({"sm": "In scope complete 3/5 (2 approved out of scope)", "sna": "-"}, cov["family_status"]["redcap"])
        self.assertEqual("In scope complete 3/9 (6 approved out of scope)", cov["family_status"]["mite"]["sna"])
        self.assertEqual("scoped", cov["creature_category"]["mite"])
        for placement in EXPECTED_WITHHELD:
            self.assertEqual("withheld", cov["states"][placement])
            self.assertTrue(disposition(cov, placement).startswith("Owner withdrawal"))

    def test_coherent_excluded_wrappers_in_both_families_are_scope_conflicts(self):
        sources = coherent_excluded_wrappers(SOURCES)
        conflicted = [P("mite", "sna", 4, D41), P("redcap", "sm", 8, D41)]
        _e, _d, cov, document, _trace = roster.build(sources)
        self.assertEqual(sorted(conflicted), cov["scope_conflicts"])
        self.assertEqual((1210, 13, 0), (cov["totals"]["covered"], cov["totals"]["out_of_scope"],
                                         cov["totals"]["unregistered"]))
        summary = roster.coverage_summary(cov)
        self.assertIn("2 SCOPE CONFLICT (declared despite approval: mite SNA IV OneD4PlusOne, "
                      "redcap SM VIII OneD4PlusOne)", summary)
        conflicts = [r for r in excluded_rows(document) if "**SCOPE CONFLICT**" in r]
        self.assertEqual(2, len(conflicts))
        self.assertTrue(any(r.startswith("| Redcap | Summon Monster | SM VIII |") for r in conflicts))
        self.assertTrue(any(r.startswith("| Mite | Summon Nature's Ally | SNA IV |") for r in conflicts))
        rows = wrapper_rows(sources)
        for placement in conflicted:
            row = rows[placement]
            self.assertEqual(("SCOPE CONFLICT: approved out of scope but declared", True, True, True),
                             (row.scope, row.registered, row.declared, row.admitted), placement)
        messages = []
        self.assertEqual(1, roster.validate_scope(sources, messages.append))
        self.assertIn("mite SNA IV OneD4PlusOne (admitted)", messages[0])
        self.assertIn("redcap SM VIII OneD4PlusOne (admitted)", messages[0])


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
