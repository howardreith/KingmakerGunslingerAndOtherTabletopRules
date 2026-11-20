using System;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>S5 T5 / POL-06 observer controls: the per-conversion quantity
    /// verdict must catch associations the aggregate set cannot.</summary>
    internal static class Pol06ConversionObservationVerdictTests
    {
        private const string Single = "sm4-single|Extend|0|1", D3 = "sm4-d3|Extend|0|1", D4 = "sm4-d4|Extend|0|1";

        private static Pol06ConversionObservationVerdict.ExpectedConversion Expect(string key, SummonMultiplicity quantity, params string[] children)
        { return new Pol06ConversionObservationVerdict.ExpectedConversion { Key = key, Tier = 4, Quantity = quantity, Children = children }; }

        private static Pol06ConversionObservationVerdict.ProjectedChild Child(int instance, string key, string child)
        { return new Pol06ConversionObservationVerdict.ProjectedChild { ConversionInstance = instance, ConversionKey = key, Child = child }; }

        private static Pol06ConversionObservationVerdict.Reading Reading(params Pol06ConversionObservationVerdict.ProjectedChild[] projected)
        {
            return new Pol06ConversionObservationVerdict.Reading {
                Expected = new[] { Expect(Single, SummonMultiplicity.One, "a", "b"), Expect(D3, SummonMultiplicity.OneD3, "c", "d"), Expect(D4, SummonMultiplicity.OneD4PlusOne, "e", "f") },
                Projected = projected, ExpectedRetained = new[] { "cure" }, ActualRetained = new[] { "cure" } };
        }

        private static Pol06ConversionObservationVerdict.ProjectedChild[] Correct()
        {
            return new[] { Child(1, Single, "a"), Child(1, Single, "b"), Child(2, D3, "c"), Child(2, D3, "d"), Child(3, D4, "e"), Child(3, D4, "f") };
        }

        internal static void ExactAssociationsPass()
        {
            var reading = Reading(Correct());
            Assertions.Equal(0, Pol06ConversionObservationVerdict.AssociationProblems(reading).Count, "Each conversion owning exactly its own creatures has no problem.");
            Assertions.True(Pol06ConversionObservationVerdict.AggregateComplete(reading), "The aggregate check also passes.");
        }

        // The reviewer's counterexample: d3 and d4+1 crossed, combined set unchanged.
        internal static void CrossedD3AndD4AssociationsFailWhileTheAggregatePasses()
        {
            var reading = Reading(Child(1, Single, "a"), Child(1, Single, "b"), Child(2, D3, "e"), Child(2, D3, "f"), Child(3, D4, "c"), Child(3, D4, "d"));
            var problems = Pol06ConversionObservationVerdict.AssociationProblems(reading);
            Assertions.True(Pol06ConversionObservationVerdict.AggregateComplete(reading), "The crossed mapping keeps the combined creature set, so the aggregate check alone passes.");
            Assertions.True(problems.Contains("missing:" + D3 + ":c") && problems.Contains("unexpected:" + D3 + ":e") &&
                problems.Contains("missing:" + D4 + ":e") && problems.Contains("unexpected:" + D4 + ":c"),
                "The per-conversion verdict fails both crossed quantities: " + string.Join(",", problems.ToArray()));
            Assertions.False(problems.Any(value => value.Contains(Single)), "The unaffected Single conversion is not blamed.");
        }

        internal static void MissingDuplicateAndUnexpectedChildrenFail()
        {
            var missing = Correct().Where(value => value.Child != "f").ToArray();
            Assertions.True(Pol06ConversionObservationVerdict.AssociationProblems(Reading(missing)).Contains("missing:" + D4 + ":f"), "A missing child fails.");
            Assertions.False(Pol06ConversionObservationVerdict.AggregateComplete(Reading(missing)), "A missing child also fails the aggregate.");
            var duplicate = Correct().Concat(new[] { Child(2, D3, "c") }).ToArray();
            Assertions.True(Pol06ConversionObservationVerdict.AssociationProblems(Reading(duplicate)).Contains("duplicate:" + D3 + ":c"), "A duplicated child fails.");
            var unexpected = Correct().Concat(new[] { Child(1, Single, "z") }).ToArray();
            Assertions.True(Pol06ConversionObservationVerdict.AssociationProblems(Reading(unexpected)).Contains("unexpected:" + Single + ":z"), "An unexpected child fails.");
        }

        internal static void ConversionIdentityRawAndRetainedRowsAreChecked()
        {
            var split = Correct().Select(value => value.Child == "d" ? Child(9, D3, "d") : value).ToArray();
            Assertions.True(Pol06ConversionObservationVerdict.AssociationProblems(Reading(split)).Contains("conversion-projected-by-several-instances:" + D3),
                "One conversion's creatures under two conversion objects fail.");
            var foreign = Correct().Concat(new[] { Child(4, "sm4-d3|Reach|0|1", "c") }).ToArray();
            Assertions.True(Pol06ConversionObservationVerdict.AssociationProblems(Reading(foreign)).Contains("unexpected-conversion:sm4-d3|Reach|0|1"),
                "A conversion Call of the Wild did not build fails.");
            var leftRaw = Reading(Correct().Where(value => value.ConversionKey != D4).ToArray()); leftRaw.Raw = new[] { D4 };
            var rawProblems = Pol06ConversionObservationVerdict.AssociationProblems(leftRaw);
            Assertions.True(rawProblems.Contains("left-raw:" + D4) && rawProblems.Contains("missing:" + D4 + ":e"), "A projectable row left raw fails.");
            var sameTier = Reading(Correct()); sameTier.Expected = sameTier.Expected.Concat(new[] { new Pol06ConversionObservationVerdict.ExpectedConversion {
                Key = "sm5-d3|Reach|0|1", Tier = 5, Quantity = SummonMultiplicity.OneD3, ExpectRaw = true } }).ToArray();
            Assertions.True(Pol06ConversionObservationVerdict.AssociationProblems(sameTier).Contains("expected-raw-count:sm5-d3|Reach|0|1:0"), "An expected raw row must be present.");
            sameTier.Raw = new[] { "sm5-d3|Reach|0|1" };
            Assertions.Equal(0, Pol06ConversionObservationVerdict.AssociationProblems(sameTier).Count, "The expected raw row present and every projection exact pass.");
            var cureLost = Reading(Correct()); cureLost.ActualRetained = new string[0];
            Assertions.True(Pol06ConversionObservationVerdict.AssociationProblems(cureLost).Contains("retained-rows-changed"), "A lost cure row fails.");
            var withdrawn = Reading(Correct()); withdrawn.Expected[0].Children = new string[0];
            Assertions.True(Pol06ConversionObservationVerdict.AssociationProblems(withdrawn).Contains("withdrawn-conversion-projected:" + Single),
                "A conversion with no permitted creature must not be projected.");
        }

        internal static void ControlledCountsAndQuantityExpressions()
        {
            Assertions.Equal(1, Pol06ConversionObservationVerdict.ExpectedCount(SummonMultiplicity.One, 2), "Single summons one.");
            Assertions.Equal(2, Pol06ConversionObservationVerdict.ExpectedCount(SummonMultiplicity.OneD3, 2), "A forced d3 of 2 summons two.");
            Assertions.Equal(3, Pol06ConversionObservationVerdict.ExpectedCount(SummonMultiplicity.OneD4PlusOne, 2), "A forced d4 of 2 summons three.");
            bool refused = false;
            try { Pol06ConversionObservationVerdict.ExpectedCount(SummonMultiplicity.OneD3, 4); } catch (ArgumentOutOfRangeException) { refused = true; }
            Assertions.True(refused, "An impossible forced die is refused.");
            Assertions.True(Pol06ConversionObservationVerdict.QuantityOfCountDice("Zero") == SummonMultiplicity.One &&
                Pol06ConversionObservationVerdict.QuantityOfCountDice("D3") == SummonMultiplicity.OneD3 &&
                Pol06ConversionObservationVerdict.QuantityOfCountDice("D4") == SummonMultiplicity.OneD4PlusOne &&
                Pol06ConversionObservationVerdict.QuantityOfCountDice("D6") == null, "Count dice map to quantities; anything else is unknown.");
            var rows = new[] { Tuple.Create("Wolf", "Extend", "w|e"), Tuple.Create("Wolf", "Heighten", "w|h"), Tuple.Create("Wolf", "Extend", "w|e") };
            Assertions.Equal(0, Pol06ConversionObservationVerdict.IndistinguishableRows(rows).Count, "Different metamagic lists distinguish the same label.");
            Assertions.Equal(1, Pol06ConversionObservationVerdict.IndistinguishableRows(rows.Concat(new[] { Tuple.Create("Wolf", "Extend", "w2|e") })).Count,
                "Two semantics under one label and metamagic list are indistinguishable.");
        }
    }
}
