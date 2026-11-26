using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    // The persistence lifecycle coverage contract: each scope requires exactly the
    // traits its matrix is declared to exercise, and every published trait outside
    // a scope's matrix names the route that covers it.
    internal static class ElementalPersistenceTraitCoverageTests
    {
        private static readonly string[] Phases = { "before-death", "native-dead", "native-resurrected",
            "native-polymorphed", "native-polymorph-return", "native-armor-equipped", "native-equipment-and-doll-restored" };

        // Fixture labels from the fixture plan (race x sex x heritage), built the way
        // the persistence scenario labels its fixtures, independent of any evidence.
        private static string[] PlannedFixtures()
        {
            var labels = new List<string>();
            foreach (ElementalHeritageRace race in Enum.GetValues(typeof(ElementalHeritageRace)))
                foreach (string sex in new[] { "male", "female" })
                    foreach (var heritage in ElementalHeritagePolicy.Ordered().Where(value => value.ParentRace == race))
                    {
                        string raceAndSex = race.ToString().ToLowerInvariant() + "-" + sex;
                        labels.Add(heritage.IsGeneral ? raceAndSex : raceAndSex + "-" + heritage.Id.ToString().ToLowerInvariant());
                    }
            return labels.ToArray();
        }

        // Realistic evidence: the 168 ordinary-scope lifecycle rows of native run
        // 20261126T1328558287358Z (candidate a49dffd66c), reduced to the evaluated fields.
        private static List<LifecycleCoverageRow> OrdinaryEvidence()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "tests", "fixtures", "elemental-persistence",
                "ordinary-lifecycle-rows-a49dffd66c.json");
            return JObject.Parse(File.ReadAllText(path))["rows"].Select(row => new LifecycleCoverageRow(
                (string)row["fixture"], (string)row["phase"], (bool)row["exact"], row["traits"].Values<string>())).ToList();
        }

        // The Nereid scope's evidence: the same rows with each Undine fixture carrying
        // the scope's planned trait instead of its ordinary breath replacement.
        private static List<LifecycleCoverageRow> NereidEvidence()
        {
            return OrdinaryEvidence().Select(row => row.Fixture.StartsWith("undine-", StringComparison.Ordinal)
                ? new LifecycleCoverageRow(row.Fixture, row.Phase, row.Exact, new[] { "NereidFascination" }) : row).ToList();
        }

        private static string Evaluate(bool nereid, IList<LifecycleCoverageRow> rows)
        {
            return ElementalPersistenceTraitCoverage.LifecycleFailure(nereid, PlannedFixtures(), Phases, rows);
        }

        private static LifecycleCoverageRow WithTraits(LifecycleCoverageRow row, IEnumerable<string> traits)
        {
            return new LifecycleCoverageRow(row.Fixture, row.Phase, row.Exact, traits);
        }

        internal static void ScopeContractsPartitionThePublishedCatalog()
        {
            var published = ElementalPersistenceTraitCoverage.Published();
            Assertions.Equal(21, published.Length, "The published catalog changed; review every persistence coverage contract.");
            foreach (bool nereid in new[] { false, true })
            {
                var required = ElementalPersistenceTraitCoverage.RequiredLifecycleTraits(nereid);
                var elsewhere = ElementalPersistenceTraitCoverage.CoveredElsewhere(nereid);
                Assertions.Equal(null, ElementalPersistenceTraitCoverage.ContractFailure(published, required, elsewhere.Keys),
                    "Each scope's matrix plus its named routes must cover the published catalog exactly once.");
                Assertions.False(required.OrderBy(value => value).SequenceEqual(published),
                    "A scope's required matrix coverage is not the full published catalog.");
                Assertions.True(elsewhere.ContainsKey(ElementalAlternateTraitId.TreacherousEarth) &&
                    elsewhere.Values.All(route => !string.IsNullOrWhiteSpace(route)),
                    "Treacherous Earth is owned by its named effect/mechanics route in every scope.");
            }
            Assertions.Equal(19, ElementalPersistenceTraitCoverage.RequiredLifecycleTraits(false).Length, "Ordinary matrix coverage.");
            Assertions.True(ElementalPersistenceTraitCoverage.CoveredElsewhere(false).ContainsKey(ElementalAlternateTraitId.NereidFascination),
                "The ordinary scope routes Nereid Fascination to its qualification scope.");
            var nereidElsewhere = ElementalPersistenceTraitCoverage.CoveredElsewhere(true);
            Assertions.True(nereidElsewhere.ContainsKey(ElementalAlternateTraitId.AcidBreath) &&
                nereidElsewhere.ContainsKey(ElementalAlternateTraitId.OozeBreath) &&
                ElementalPersistenceTraitCoverage.RequiredLifecycleTraits(true).Contains(ElementalAlternateTraitId.NereidFascination),
                "The Nereid scope covers Nereid Fascination and routes the breath replacements to the ordinary matrix.");
            var routedEverywhere = ElementalPersistenceTraitCoverage.CoveredElsewhere(false).Keys
                .Intersect(ElementalPersistenceTraitCoverage.CoveredElsewhere(true).Keys).ToArray();
            Assertions.True(routedEverywhere.SequenceEqual(new[] { ElementalAlternateTraitId.TreacherousEarth }),
                "Only Treacherous Earth lies outside both lifecycle matrices.");

            // The contract check itself fails closed.
            var ordinary = ElementalPersistenceTraitCoverage.RequiredLifecycleTraits(false);
            var ordinaryElsewhere = ElementalPersistenceTraitCoverage.CoveredElsewhere(false).Keys.ToArray();
            Assertions.True((ElementalPersistenceTraitCoverage.ContractFailure(published.Concat(new[] { (ElementalAlternateTraitId)99 }),
                ordinary, ordinaryElsewhere) ?? "").Contains("no coverage contract"), "A newly published trait without a route must fail.");
            Assertions.True((ElementalPersistenceTraitCoverage.ContractFailure(published, ordinary,
                ordinaryElsewhere.Concat(new[] { ElementalAlternateTraitId.AirInsight })) ?? "").Contains("both required and routed"),
                "A trait may not be both required and routed elsewhere.");
            Assertions.True((ElementalPersistenceTraitCoverage.ContractFailure(published.Where(value =>
                value != ElementalAlternateTraitId.TreacherousEarth), ordinary, ordinaryElsewhere) ?? "").Contains("unpublished"),
                "A route for an unpublished trait must fail.");
        }

        internal static void FixturePlanSuppliesDeclaredCoverage()
        {
            foreach (bool nereid in new[] { false, true })
            {
                var supplied = new HashSet<ElementalAlternateTraitId>();
                foreach (ElementalHeritageRace race in Enum.GetValues(typeof(ElementalHeritageRace)))
                    for (int sex = 0; sex < 2; ++sex)
                        for (int heritage = 0; heritage < 3; ++heritage)
                            supplied.UnionWith(ElementalVisibleTraitPersistencePolicy.Traits(race, sex, heritage, nereid));
                var declared = ElementalPersistenceTraitCoverage.RequiredLifecycleTraits(nereid);
                Assertions.True(supplied.SetEquals(declared),
                    "The fixture plan must supply exactly its declared coverage (nereid=" + nereid + "): plan-only [" +
                    string.Join(",", supplied.Except(declared)) + "] declared-only [" + string.Join(",", declared.Except(supplied)) + "]");
            }
            Assertions.Equal(24, PlannedFixtures().Distinct().Count(), "Twenty-four distinct planned fixtures.");
        }

        internal static void EvaluatorAcceptsCompleteScopeEvidence()
        {
            var ordinary = OrdinaryEvidence();
            Assertions.Equal(168, ordinary.Count, "The recorded native evidence has 24 fixtures x 7 phases.");
            Assertions.True(new HashSet<string>(ordinary.Select(row => row.Fixture)).SetEquals(PlannedFixtures()),
                "The recorded fixtures equal the planned fixtures.");
            Assertions.Equal(null, Evaluate(false, ordinary), "Complete exact ordinary evidence is accepted.");
            Assertions.Equal(null, Evaluate(true, NereidEvidence()), "Complete exact Nereid-scope evidence is accepted.");
            // The old full-catalog requirement rejected this same complete evidence.
            var published = ElementalPersistenceTraitCoverage.Published().Select(value => value.ToString());
            Assertions.False(new HashSet<string>(ordinary.SelectMany(row => row.Traits)).SetEquals(published),
                "Complete ordinary evidence does not, and need not, cover the full published catalog.");
            // Scopes are not interchangeable.
            Assertions.True((Evaluate(true, ordinary) ?? "").Contains("trait coverage"),
                "Ordinary evidence must not satisfy the Nereid scope.");
            Assertions.True((Evaluate(false, NereidEvidence()) ?? "").Contains("trait coverage"),
                "Nereid evidence must not satisfy the ordinary scope.");
        }

        internal static void EvaluatorRejectsIncompleteOrInexactEvidence()
        {
            var rows = OrdinaryEvidence();
            Func<Func<LifecycleCoverageRow, LifecycleCoverageRow>, string> mapped = map => Evaluate(false, rows.Select(map).ToList());

            string missing = mapped(row => WithTraits(row, row.Traits.Where(value => value != "AirInsight")));
            Assertions.True((missing ?? "").Contains("missing [AirInsight]"), "A missing required trait must fail: " + missing);

            var extra = rows.ToList();
            extra[0] = WithTraits(extra[0], extra[0].Traits.Concat(new[] { "TreacherousEarth" }));
            string unexpected = Evaluate(false, extra);
            Assertions.True((unexpected ?? "").Contains("unexpected [TreacherousEarth]"), "An unexpected trait must fail: " + unexpected);

            string fixture = rows[0].Fixture;
            string droppedFixture = Evaluate(false, rows.Where(row => row.Fixture != fixture).ToList());
            Assertions.True(droppedFixture != null, "A missing fixture must fail.");

            var droppedPhase = rows.ToList();
            droppedPhase.RemoveAt(3);
            Assertions.True(Evaluate(false, droppedPhase) != null, "A missing phase row must fail.");

            // Same row count, one phase duplicated in place of another: still incomplete.
            var swapped = rows.ToList();
            int index = swapped.FindIndex(row => row.Fixture == fixture && row.Phase == Phases[2]);
            swapped[index] = new LifecycleCoverageRow(fixture, Phases[1], true, swapped[index].Traits);
            string phaseFailure = Evaluate(false, swapped);
            Assertions.True((phaseFailure ?? "").Contains("fixture " + fixture + " phases"), "A missing required phase must fail: " + phaseFailure);

            var reordered = rows.ToList();
            int first = reordered.FindIndex(row => row.Fixture == fixture);
            var hold = reordered[first]; reordered[first] = reordered[first + 1]; reordered[first + 1] = hold;
            Assertions.True(Evaluate(false, reordered) != null, "Out-of-order phases must fail.");

            var inexact = rows.ToList();
            inexact[10] = new LifecycleCoverageRow(inexact[10].Fixture, inexact[10].Phase, false, inexact[10].Traits);
            Assertions.True((Evaluate(false, inexact) ?? "").Contains("inexact observation"), "An individually failed observation must fail.");

            Assertions.True(Evaluate(false, new List<LifecycleCoverageRow>()) != null, "Empty evidence must fail.");
        }
    }
}
