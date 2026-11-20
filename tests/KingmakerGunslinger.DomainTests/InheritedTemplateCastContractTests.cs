using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>DEF-05 second review: positive cases and the named negative controls for the five cast/census contracts.</summary>
    internal static class InheritedTemplateCastContractTests
    {
        private static bool Has(IList<string> failures, string text)
        {
            return failures.Any(value => value.Contains(text));
        }

        private static Dictionary<string, int> Sources(params Tuple<string, int>[] values)
        {
            return values.ToDictionary(value => value.Item1, value => value.Item2, StringComparer.Ordinal);
        }

        // ---- Correction 1: exact source debit ----

        internal static void SourceDebitIsExact()
        {
            Func<Dictionary<string, int>> before = () => Sources(Tuple.Create("spontaneous|sorc|4", 6),
                Tuple.Create("spontaneous|sorc|3", 7), Tuple.Create("resource|other", 3), Tuple.Create("resource|ms", 5));
            const string spellEvent = "spend-spell|sorc|1";
            Dictionary<string, int> after = before(); after["spontaneous|sorc|4"] = 5;
            Assertions.Equal(0, InheritedSourceLedger.Judge(new[] { spellEvent }, before(), after,
                "spontaneous|sorc|4", 1, spellEvent).Count, "One spontaneous use spent once passes.");
            Assertions.True(Has(InheritedSourceLedger.Judge(new[] { spellEvent, spellEvent, "restore-spontaneous|sorc|4|1" },
                before(), after, "spontaneous|sorc|4", 1, spellEvent), "expected-spend-events:2"),
                "Two uses spent and one restored must fail although the net change is one.");
            Dictionary<string, int> wrongLevel = before(); wrongLevel["spontaneous|sorc|3"] = 6;
            Assertions.True(Has(InheritedSourceLedger.Judge(new[] { spellEvent }, before(), wrongLevel,
                "spontaneous|sorc|4", 1, spellEvent), "source-changed:spontaneous|sorc|3"), "The wrong spell level spent must fail.");
            const string resourceEvent = "spend-resource|ms|2";
            Dictionary<string, int> collateral = before(); collateral["resource|ms"] = 3; collateral["resource|other"] = 2;
            IList<string> both = InheritedSourceLedger.Judge(new[] { resourceEvent, "spend-resource|other|1" }, before(), collateral,
                "resource|ms", 2, resourceEvent);
            Assertions.True(Has(both, "source-changed:resource|other") && Has(both, "unexpected-event:spend-resource|other"),
                "The expected resource plus an unrelated resource spent must fail.");
            Dictionary<string, int> exactResource = before(); exactResource["resource|ms"] = 3;
            Assertions.Equal(0, InheritedSourceLedger.Judge(new[] { resourceEvent }, before(), exactResource,
                "resource|ms", 2, resourceEvent).Count, "The exact calculated resource cost spent once passes.");
            Assertions.True(Has(InheritedSourceLedger.Judge(new string[0], before(), before(), "resource|ms", 0, "spend-resource|ms|0"),
                "non-positive-expected-debit"), "A zero calculated cost is not accepted as a debit.");
            Dictionary<string, int> touched = before(); touched["resource|other"] = 2;
            Assertions.True(Has(InheritedSourceLedger.Judge(new string[0], before(), touched, null, 0, null), "source-changed:resource|other"),
                "A cancellation with collateral source mutation must fail.");
            Assertions.Equal(0, InheritedSourceLedger.Judge(new string[0], before(), before(), null, 0, null).Count,
                "A clean cancellation passes.");
            Assertions.True(Has(InheritedSourceLedger.Judge(new[] { "restore-spontaneous|sorc|4|1" }, before(), before(), null, 0, null),
                "unexpected-event"), "A cancellation with a spend and a matching restore must fail.");
        }

        // ---- Correction 2: exact quantity contract ----

        private static InheritedSpawnExpression Single(string unit)
        {
            return new InheritedSpawnExpression(unit, "Zero", "Simple", 0, "Simple", 1, "Default");
        }

        private static InheritedSpawnExpression Group(string unit, string die, string bonusKind = "Rank")
        {
            return new InheritedSpawnExpression(unit, die, "Simple", 1, bonusKind, bonusKind == "Simple" ? 1 : 0,
                bonusKind == "Rank" ? "ProjectilesCount" : "Default");
        }

        internal static void QuantityExpressionIsExact()
        {
            int min, max;
            Assertions.Equal(0, InheritedQuantityContract.Expression(SummonMultiplicity.OneD4PlusOne, "mephit",
                new[] { Group("mephit", "D4") }, 1, 1, out min, out max).Count, "1d4+1 passes.");
            Assertions.True(min == 2 && max == 5, "1d4+1 ranges 2..5.");
            Assertions.True(Has(InheritedQuantityContract.Expression(SummonMultiplicity.OneD4PlusOne, "mephit",
                new[] { Group("mephit", "D4") }, 1, 0, out min, out max), "wrong-bonus-value"), "d4 instead of d4+1 must fail.");
            Assertions.True(Has(InheritedQuantityContract.Expression(SummonMultiplicity.OneD4PlusOne, "mephit",
                new[] { Group("mephit", "D3") }, 1, 1, out min, out max), "wrong-die"), "d3+1 instead of d4+1 must fail.");
            Assertions.True(Has(InheritedQuantityContract.Expression(SummonMultiplicity.OneD4PlusOne, "mephit",
                new[] { new InheritedSpawnExpression("mephit", "Zero", "Simple", 0, "Simple", 3, "Default") }, 1, 1, out min, out max),
                "wrong-die"), "A fixed count that happens to fall in range must fail.");
            Assertions.True(Has(InheritedQuantityContract.Expression(SummonMultiplicity.OneD4PlusOne, "mephit",
                new[] { Group("wolf", "D4") }, 1, 1, out min, out max), "wrong-unit"), "The right expression on the wrong unit must fail.");
            Assertions.True(Has(InheritedQuantityContract.Expression(SummonMultiplicity.One, "mephit",
                new[] { Single("mephit"), Single("mephit") }, 1, 0, out min, out max), "spawn-branches:2/1"), "An extra spawn branch must fail.");
            Assertions.Equal(0, InheritedQuantityContract.Expression(SummonMultiplicity.One, "elemental",
                new[] { Single("elemental"), Single("elemental") }, 2, 0, out min, out max).Count,
                "Two donor branches (a tier VII alignment branch) pass when the donor has two.");
            Assertions.Equal(0, InheritedQuantityContract.Output(2, 5, "mephit", new[] { "mephit", "mephit", "mephit" },
                new[] { "a", "b", "c" }, new[] { true, true, true }).Count, "Three distinct live mephits pass for 1d4+1.");
            Assertions.True(Has(InheritedQuantityContract.Output(2, 5, "mephit", new[] { "mephit", "mephit" },
                new[] { "a", "a" }, new[] { true, true }), "duplicate-spawn-event"), "A duplicate spawn event must fail.");
            Assertions.True(Has(InheritedQuantityContract.Output(1, 1, "mephit", new[] { "mephit", "wolf" },
                new[] { "a", "b" }, new[] { true, true }), "foreign-unit"), "A hidden foreign unit must fail.");
        }

        // ---- Correction 3: exact duration ----

        internal static void DurationIsExact()
        {
            var units = new[] { "a", "b" };
            Assertions.Equal(0, InheritedDurationContract.Judge(units, new[] { Tuple.Create("a", 120d, 0d), Tuple.Create("b", 120d, 0d) },
                120, 0).Count, "120 s base and no bonus per unit passes.");
            Assertions.True(Has(InheritedDurationContract.Judge(new[] { "a" }, new[] { Tuple.Create("a", 1d, 0d) }, 120, 0), "duration:a"),
                "A one-second duration must fail.");
            Assertions.True(Has(InheritedDurationContract.Judge(new[] { "a" }, new[] { Tuple.Create("a", 60d, 60d) }, 120, 0), "duration:a"),
                "The right total from the wrong base and bonus must fail.");
            Assertions.True(Has(InheritedDurationContract.Judge(units, new[] { Tuple.Create("a", 120d, 0d) }, 120, 0), "duration-for-unit:b"),
                "A missing duration for one unit of a quantity summon must fail.");
            Assertions.True(Has(InheritedDurationContract.Judge(new[] { "a" }, new[] { Tuple.Create("z", 120d, 0d) }, 120, 0), "duration-for-other-unit:z"),
                "A duration attached to a different unit must fail.");
            Assertions.Equal(0, InheritedDurationContract.Judge(new string[0], new Tuple<string, double, double>[0], 0, 0).Count,
                "A cancellation has no duration observation.");
        }

        // ---- Correction 4: exact template and smite sets ----

        internal static void TemplateAndSmiteSetsAreExact()
        {
            var native = new HashSet<string> { "n1", "n2", "n3", "n4" };
            var templates = new HashSet<string> { "celA", "celB", "fieA", "fieB" };
            var celestialSmites = new HashSet<string> { "smiteC" };
            var fiendishSmites = new HashSet<string> { "smiteF" };
            Func<string, string[], string, string, IList<string>> judge = (expectation, buffs, template, smite) =>
                InheritedBuffSetContract.Judge(expectation, buffs, native, templates, celestialSmites, fiendishSmites, template, smite);
            Assertions.Equal(0, judge("celestial", new[] { "marker", "celA", "smiteC" }, "celA", "smiteC").Count, "Exact celestial set passes.");
            Assertions.Equal(0, judge("fiendish", new[] { "fieA", "smiteF" }, "fieA", "smiteF").Count, "Exact fiendish set passes.");
            Assertions.Equal(0, judge("none", new[] { "marker" }, null, null).Count, "An untemplated unit with no template or smite passes.");
            Assertions.True(Has(judge("none", new[] { "n2" }, null, null), "native-template"), "An inherited native template must fail.");
            Assertions.True(Has(judge("celestial", new[] { "celA", "celB", "smiteC" }, "celA", "smiteC"), "template-set"),
                "The expected template plus another band must fail.");
            Assertions.True(Has(judge("celestial", new[] { "celA", "smiteC", "smiteF" }, "celA", "smiteC"), "opposite-smite"),
                "Both smites must fail.");
            Assertions.True(Has(judge("celestial", new[] { "celA", "smiteF" }, "celA", "smiteC"), "opposite-smite"),
                "The opposite smite without the opposite template must fail.");
            Assertions.True(Has(judge("celestial", new[] { "celA", "celA", "smiteC" }, "celA", "smiteC"), "template-set"),
                "A duplicate expected template must fail.");
            Assertions.True(Has(judge("celestial", new[] { "celA" }, "celA", "smiteC"), "smite-set"),
                "The expected template without the expected smite must fail.");
            Assertions.True(Has(judge("none", new[] { "smiteF" }, null, null), "project-smite"), "A stray smite on an untemplated unit must fail.");
        }

        // ---- Third review: the deliberate template is derived independently of the child graph ----

        internal static void DeliberateTemplateIsIndependent()
        {
            var native = new HashSet<string> { "n1", "n2", "n3", "n4" };
            // Expected identities as the observer resolves them: band from unit class levels, then stable symbols.
            Assertions.Equal("Low", InheritedDeliberateTemplateContract.Band(4), "HD 4 is the low band.");
            Assertions.Equal("Mid", InheritedDeliberateTemplateContract.Band(5), "HD 5 is the mid band.");
            Assertions.Equal("Mid", InheritedDeliberateTemplateContract.Band(10), "HD 10 is the mid band.");
            Assertions.Equal("High", InheritedDeliberateTemplateContract.Band(11), "HD 11 is the high band.");
            for (int hitDice = 0; hitDice <= 30; hitDice++)
                Assertions.Equal(SummonTemplateBandPolicy.Select(hitDice).ToString(), InheritedDeliberateTemplateContract.Band(hitDice),
                    "The restated band contract agrees with the production policy at HD " + hitDice);
            const string celLow = "celLow", celMid = "celMid", fieLow = "fieLow", smiteC = "smiteC", smiteF = "smiteF";
            Func<string, string, string, string, string[], InheritedDynamicTemplateView> view = (ct, ft, cs, fs, replaced) =>
                new InheritedDynamicTemplateView(ct, ft, cs, fs, replaced);
            string[] four = { "n1", "n2", "n3", "n4" };
            Func<InheritedDynamicTemplateView[], IList<string>> graph = actions =>
                InheritedDeliberateTemplateContract.Graph(actions, celLow, fieLow, smiteC, smiteF, native);
            Assertions.Equal(0, graph(new[] { view(celLow, fieLow, smiteC, smiteF, four) }).Count, "The exact graph passes.");
            Assertions.True(Has(graph(new[] { view(celMid, fieLow, smiteC, smiteF, four) }), "celestial-template:celMid/celLow"),
                "A celestial action wired to the wrong band must fail.");
            Assertions.True(Has(graph(new[] { view(fieLow, fieLow, smiteC, smiteF, four) }), "celestial-template:fieLow/celLow"),
                "A celestial action wired to a fiendish template while keeping the celestial smite must fail.");
            Assertions.True(Has(graph(new[] { view(celLow, fieLow, smiteF, smiteF, four) }), "celestial-smite:smiteF/smiteC"),
                "The right template with the wrong smite must fail.");
            Assertions.True(Has(graph(new InheritedDynamicTemplateView[0]), "dynamic-template-actions:0"),
                "A missing dynamic-template action must fail.");
            Assertions.True(Has(graph(new[] { view(celLow, fieLow, smiteC, smiteF, four), view(celLow, fieLow, smiteC, smiteF, four) }),
                "dynamic-template-actions:2"), "Duplicate dynamic-template actions must fail.");
            Assertions.True(Has(graph(new[] { view(celLow, fieLow, smiteC, smiteF, new[] { "n1", "n2", "n3" }) }), "replaced-native"),
                "Incomplete replaced native identities must fail.");
            Assertions.True(Has(graph(new[] { view(celLow, fieLow, smiteC, smiteF, new[] { "n1", "n2", "n3", "x" }) }), "replaced-native"),
                "A wrong replaced native identity must fail.");
            // The live check, fed the independent expectation, fails on the wrong band even though the
            // defective child applied exactly what it wired.
            var templates = new HashSet<string> { celLow, celMid, fieLow };
            IList<string> live = InheritedBuffSetContract.Judge("celestial", new[] { celMid, smiteC }, native, templates,
                new HashSet<string> { smiteC }, new HashSet<string> { smiteF }, celLow, smiteC);
            Assertions.True(Has(live, "template-set:celMid/celLow"),
                "A unit carrying exactly the wrong band its defective child wired must still fail.");
            Assertions.Equal(0, InheritedBuffSetContract.Judge("celestial", new[] { celLow, smiteC }, native, templates,
                new HashSet<string> { smiteC }, new HashSet<string> { smiteF }, celLow, smiteC).Count,
                "The exact live set passes with the independent expectation.");
        }

        // ---- Correction 5: no extra alternate child ----

        internal static void ExtraAlternateChildrenAreRejected()
        {
            var expected = new Dictionary<string, Tuple<string, SummonFamily?>>(StringComparer.Ordinal)
            {
                { "MS.T4.Mephit.One", Tuple.Create("Monster.4.air-mephit.One", (SummonFamily?)SummonFamily.Monster) },
                { "MS.T4.Wolf.One", Tuple.Create("Monster.4.wolf.OneD3", (SummonFamily?)SummonFamily.Monster) },
            };
            var actual = new Dictionary<string, Tuple<string, SummonFamily?>>(expected, StringComparer.Ordinal);
            Assertions.Equal(0, InheritedChildSetContract.Compare("MasterSummoner", SummonFamily.Monster, expected, actual).Count,
                "Equal sets pass.");
            actual["MS.T4.Stray.One"] = Tuple.Create("Monster.4.stray.One", (SummonFamily?)SummonFamily.Monster);
            Assertions.True(Has(InheritedChildSetContract.Compare("MasterSummoner", SummonFamily.Monster, expected, actual),
                "MS.T4.Stray.One:extra-child"), "All expected children plus one unexpected creature child must fail.");
            var missing = new Dictionary<string, Tuple<string, SummonFamily?>>(expected, StringComparer.Ordinal);
            missing.Remove("MS.T4.Wolf.One");
            Assertions.True(Has(InheritedChildSetContract.Compare("MasterSummoner", SummonFamily.Monster, expected, missing), "missing-child"),
                "A missing child must fail.");
            var wrong = new Dictionary<string, Tuple<string, SummonFamily?>>(expected, StringComparer.Ordinal);
            wrong["MS.T4.Wolf.One"] = Tuple.Create("NaturesAlly.4.wolf.OneD3", (SummonFamily?)SummonFamily.NaturesAlly);
            Assertions.True(Has(InheritedChildSetContract.Compare("MasterSummoner", SummonFamily.Monster, expected, wrong), "wrong-placement-family"),
                "A child bound to the other spell family must fail.");
            var nulls = new Dictionary<string, Tuple<string, SummonFamily?>>(expected, StringComparer.Ordinal);
            nulls["MS.T4.Wolf.One"] = Tuple.Create((string)null, (SummonFamily?)null);
            Assertions.True(Has(InheritedChildSetContract.Compare("MasterSummoner", SummonFamily.Monster, expected, nulls), "null-placement"),
                "A child with no placement must fail.");
            var duplicate = new Dictionary<string, Tuple<string, SummonFamily?>>(expected, StringComparer.Ordinal);
            duplicate["MS.T4.Wolf.One"] = expected["MS.T4.Mephit.One"];
            Assertions.True(Has(InheritedChildSetContract.Compare("MasterSummoner", SummonFamily.Monster, expected, duplicate), "duplicate-placement"),
                "Two children on one placement the projection gives once must fail.");
        }
    }
}
