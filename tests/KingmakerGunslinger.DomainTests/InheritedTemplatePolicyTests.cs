using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// S5 T1 / DEF-05. Expected placement intent comes from two inputs that do
    /// not depend on the new policy: ExpandedSummoningCatalog (family and
    /// templating) and the donor table below, which records the native template
    /// identities each native Summon Monster carrier applied in the retained
    /// pre-change census (session 20261118T032921Z-owner-0.33.0-template-census-baseline-cotw,
    /// accepted Sprint 4 product 16708bd5e237, 281 untemplated placements). The
    /// census evaluator's verdicts on synthetic graphs are then checked, with a
    /// mutation for every false-pass the story names.
    /// </summary>
    internal static class InheritedTemplatePolicyTests
    {
        private const string CelestialI = "83a8a909e7ad19b4fa57e306884cc8bd";
        private const string FiendishI = "c707fdbe58d0c614a89a872b76777f4b";
        private const string CelestialII = "ec906031bb8766f42a37a52cf1d89836";
        private const string FiendishII = "4b808ccd7e0b7bd43b10b2d47733b1a4";
        private static readonly string[] Native = { CelestialI, FiendishI, CelestialII, FiendishII };
        private static readonly HashSet<string> NativeSet = new HashSet<string>(Native, StringComparer.Ordinal);

        /// <summary>The v0.0.186 declared creature list, kept only to prove the evaluator rejects it.</summary>
        private static readonly string[] StaleCreatureList = {
            "lemure", "dretch", "hound-archon", "lantern-archon", "trumpet-archon", "astral-deva",
            "bearded-devil", "kyton-chain-devil", "barbed-devil", "bone-devil", "babau", "hezrou" };

        private static string[] Donor(SummonVariantSpec variant)
        {
            if (variant.Family != SummonFamily.Monster) return new string[0];
            // The retained census read untemplated placements only. A templated placement's
            // disposition does not depend on its donor, so a shape it never observed uses none.
            bool templated = variant.Creature.MonsterTemplated;
            switch (variant.ParentTier * 10 + (int)variant.Multiplicity)
            {
                case 20: case 30: case 31: case 41: case 42: case 52: return new[] { CelestialI, FiendishI };
                case 40: return new[] { FiendishI, CelestialII };
                case 51: case 62: return new[] { FiendishII, CelestialII };
                default:
                    if (variant.ParentTier >= 5 || templated) return new string[0];
                    throw new InvalidOperationException("No retained donor observation for " + variant.StableKey);
            }
        }

        private static IEnumerable<SummonVariantSpec> All()
        {
            return ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly));
        }

        private static InheritedTemplateBranch Branch(bool donorApplies, bool removal, bool deliberate = false)
        {
            // A donor's alignment-conditional application sits at index 0; project actions follow.
            return new InheritedTemplateBranch(donorApplies ? 0 : -1, removal ? new[] { 1 } : new int[0],
                true, true, deliberate);
        }

        /// <summary>What a correct build publishes, derived from intent and the donor table only.</summary>
        private static List<InheritedTemplateObservation> CorrectGraph(out List<string> expected,
            Func<SummonVariantSpec, bool> removeWhen = null)
        {
            expected = new List<string>();
            var result = new List<InheritedTemplateObservation>();
            foreach (SummonVariantSpec variant in All())
            {
                string[] donor = Donor(variant);
                bool templated = variant.Family == SummonFamily.Monster && variant.Creature.MonsterTemplated;
                bool applies = donor.Length > 0;
                bool remove = removeWhen != null ? removeWhen(variant) :
                    variant.Family == SummonFamily.Monster && !templated && applies;
                string symbol = variant.StableKey;
                foreach (string published in templated ? new[] { symbol, symbol + ".Celestial", symbol + ".Fiendish" } : new[] { symbol })
                {
                    expected.Add("canonical|" + published);
                    result.Add(new InheritedTemplateObservation(published, "canonical", variant.Family, templated,
                        templated, donor, new[] { Branch(applies, remove, templated) }));
                }
            }
            return result;
        }

        private static InheritedTemplateCensusResult Judge(IEnumerable<InheritedTemplateObservation> graph, IEnumerable<string> expected)
        {
            return InheritedTemplateCensus.Evaluate(graph, expected, NativeSet);
        }

        internal static void PolicyMatchesTheRetainedDonorGraphs()
        {
            int removals = 0, untemplatedMonster = 0;
            foreach (SummonVariantSpec variant in All())
            {
                InheritedTemplateDisposition policy = InheritedTemplatePolicy.Decide(variant.Family,
                    variant.Creature.MonsterTemplated, Donor(variant), NativeSet);
                var probe = new InheritedTemplateObservation(variant.StableKey, "canonical", variant.Family,
                    variant.Family == SummonFamily.Monster && variant.Creature.MonsterTemplated, false,
                    Donor(variant), new InheritedTemplateBranch[0]);
                Assertions.Equal(InheritedTemplateCensus.Expected(probe, NativeSet), policy,
                    "The policy and the census's independent contract disagree on " + variant.StableKey);
                if (variant.Family == SummonFamily.Monster && !variant.Creature.MonsterTemplated) untemplatedMonster++;
                if (policy == InheritedTemplateDisposition.RemoveInherited) removals++;
            }
            // Cross-check with the retained census, not a pass criterion of the census itself.
            Assertions.Equal(281, untemplatedMonster, "The retained census covered 281 untemplated placements.");
            Assertions.Equal(65 + 16, removals, "Removal belongs exactly on the 81 placements whose donor applied a template.");
        }

        internal static void TheSameCreatureDiffersByPlacement()
        {
            SummonVariantSpec[] lemure = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key == "lemure").ToArray();
            InheritedTemplateDisposition[] dispositions = lemure.Select(value => InheritedTemplatePolicy.Decide(
                value.Family, value.Creature.MonsterTemplated, Donor(value), NativeSet)).Distinct().ToArray();
            Assertions.True(dispositions.Contains(InheritedTemplateDisposition.RemoveInherited) &&
                dispositions.Contains(InheritedTemplateDisposition.NoInheritedTemplate),
                "One creature must need removal on some placements and not on others.");
            SummonCreatureSpec both = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Select(value => value.Creature).FirstOrDefault(creature => !creature.MonsterTemplated &&
                    creature.NaturesAllyTier.HasValue && creature.MonsterTier.HasValue &&
                    creature.MonsterTier.Value <= 4);
            Assertions.True(both != null, "A creature published by both families at a low tier exists.");
            SummonVariantSpec monster = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .First(value => ReferenceEquals(value.Creature, both) && Donor(value).Length > 0);
            SummonVariantSpec ally = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)
                .First(value => ReferenceEquals(value.Creature, both));
            Assertions.Equal(InheritedTemplateDisposition.RemoveInherited, InheritedTemplatePolicy.Decide(
                monster.Family, false, Donor(monster), NativeSet), "Its Summon Monster placement removes.");
            Assertions.Equal(InheritedTemplateDisposition.OutsideSummonMonster, InheritedTemplatePolicy.Decide(
                ally.Family, false, Donor(ally), NativeSet), "Its Nature's Ally placement is left alone.");
            Assertions.Equal(InheritedTemplateDisposition.PreserveDeliberateTemplate, InheritedTemplatePolicy.Decide(
                SummonFamily.Monster, true, new[] { CelestialI, FiendishI }, NativeSet),
                "A deliberately templated placement keeps its template even when the donor applies one.");
        }

        internal static void ACorrectGraphPassesAndRedundancyIsListed()
        {
            List<string> expected;
            List<InheritedTemplateObservation> graph = CorrectGraph(out expected);
            SummonVariantSpec[] mirrored = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Where(value => !value.Creature.MonsterTemplated && Donor(value).Length > 0).Take(3).ToArray();
            SummonVariantSpec clean = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .First(value => !value.Creature.MonsterTemplated && Donor(value).Length == 0);
            foreach (SummonVariantSpec variant in mirrored.Concat(new[] { clean }))
            {
                string key = "MasterSummoner." + variant.StableKey;
                expected.Add("alternate:MasterSummoner|" + key);
                // The alternate host graph applies no native template; the canonical donor decides.
                graph.Add(new InheritedTemplateObservation(key, "alternate:MasterSummoner", variant.Family, false,
                    false, Donor(variant), new[] { Branch(false, Donor(variant).Length > 0) }));
            }
            InheritedTemplateCensusResult result = Judge(graph, expected);
            Assertions.True(result.Passed, "A correct graph failed: " + string.Join(", ", result.Findings.Take(5).ToArray()));
            Assertions.Equal(mirrored.Length, result.JustifiedRedundant.Count,
                "Every mirrored alternate removal is listed as justified redundancy, and nothing else is.");
            Assertions.True(result.Dispositions[InheritedTemplateDisposition.PreserveDeliberateTemplate] > 0 &&
                result.Dispositions[InheritedTemplateDisposition.OutsideSummonMonster] > 0,
                "Templated and Nature's Ally controls are part of the census.");
            Assertions.True(graph.Any(value => value.Key.Contains("OneD3") || value.Key.Contains("OneD4PlusOne")),
                "Quantity placements are part of the census.");
        }

        internal static void TheStaleCreatureListIsRejected()
        {
            List<string> expected;
            List<InheritedTemplateObservation> stale = CorrectGraph(out expected, variant =>
                variant.Family == SummonFamily.Monster && !variant.Creature.MonsterTemplated &&
                Array.IndexOf(StaleCreatureList, variant.Creature.Key) >= 0);
            InheritedTemplateCensusResult result = Judge(stale, expected);
            int omitted = result.Findings.Count(value => value.EndsWith(":omitted", StringComparison.Ordinal));
            int extra = result.Findings.Count(value => value.EndsWith(":extra", StringComparison.Ordinal));
            Assertions.False(result.Passed, "The creature-name list must fail the census.");
            // Matches the v0.0.186 graph observation (65 apply without removal, 36 removal without application).
            Assertions.Equal(65, omitted, "Stale list omissions.");
            Assertions.Equal(36, extra, "Stale list extras.");
        }

        private static void Mutation(string label, string category,
            Func<List<InheritedTemplateObservation>, List<string>, List<InheritedTemplateObservation>> mutate)
        {
            List<string> expected;
            List<InheritedTemplateObservation> graph = mutate(CorrectGraph(out expected), expected);
            InheritedTemplateCensusResult result = Judge(graph, expected);
            Assertions.True(result.Findings.Any(value => value.EndsWith(":" + category, StringComparison.Ordinal)),
                label + ": expected a '" + category + "' finding, got " + string.Join(", ", result.Findings.Take(4).ToArray()));
        }

        private static List<InheritedTemplateObservation> Replace(List<InheritedTemplateObservation> graph,
            Func<InheritedTemplateObservation, bool> which, Func<InheritedTemplateObservation, InheritedTemplateObservation> with)
        {
            int index = graph.FindIndex(value => which(value));
            if (index < 0) throw new InvalidOperationException("No observation to mutate.");
            graph[index] = with(graph[index]);
            return graph;
        }

        private static bool Removes(InheritedTemplateObservation value)
        {
            return value.Family == SummonFamily.Monster && !value.MonsterTemplated && value.DonorAppliedTemplateIds.Length > 0;
        }

        private static InheritedTemplateObservation With(InheritedTemplateObservation value, InheritedTemplateBranch branch,
            string[] donor = null, SummonFamily? family = null)
        {
            return new InheritedTemplateObservation(value.Key, value.Caller, family ?? value.Family, value.MonsterTemplated,
                value.DeliberateTemplateAbility, donor ?? value.DonorAppliedTemplateIds, new[] { branch });
        }

        internal static void EveryNamedFalsePassIsCaught()
        {
            Mutation("omitted cleanup", "omitted", (g, e) => Replace(g, Removes, v => With(v, Branch(true, false))));
            Mutation("incorrect extra cleanup", "extra", (g, e) => Replace(g,
                v => v.Family == SummonFamily.Monster && !v.MonsterTemplated && v.DonorAppliedTemplateIds.Length == 0,
                v => With(v, Branch(false, true))));
            Mutation("cleanup on the wrong placement (owner)", "wrong-owner", (g, e) => Replace(g, Removes,
                v => With(v, new InheritedTemplateBranch(0, new[] { 1 }, false, true, false))));
            Mutation("cleanup moved to another placement", "omitted", (g, e) =>
            {
                g = Replace(g, Removes, v => With(v, Branch(true, false)));
                return Replace(g, v => v.Family == SummonFamily.Monster && !v.MonsterTemplated &&
                    v.DonorAppliedTemplateIds.Length == 0, v => With(v, Branch(false, true)));
            });
            Mutation("cleanup before the donor application", "before-donor", (g, e) => Replace(g, Removes,
                v => With(v, new InheritedTemplateBranch(2, new[] { 1 }, true, true, false))));
            Mutation("duplicated cleanup", "duplicate-removal", (g, e) => Replace(g, Removes,
                v => With(v, new InheritedTemplateBranch(0, new[] { 1, 2 }, true, true, false))));
            Mutation("a removal naming a non-native look-alike (the project celestial template or a foreign buff)",
                "identities", (g, e) => Replace(g, Removes, v => With(v, new InheritedTemplateBranch(0, new[] { 1 }, true, false, false))));
            Mutation("deliberate template lost", "deliberate-template-missing", (g, e) => Replace(g,
                v => v.DeliberateTemplateAbility, v => With(v, Branch(true, false, false))));
            Mutation("wrong donor (generated graph applies, nothing expected)", "unremoved-application", (g, e) => Replace(g,
                v => v.Family == SummonFamily.Monster && !v.MonsterTemplated && v.DonorAppliedTemplateIds.Length == 0,
                v => With(v, Branch(true, false))));
            Mutation("a reporter that skips a placement", "skipped", (g, e) => { g.RemoveAt(g.FindIndex(Removes)); return g; });
            Mutation("a reporter that reports a placement twice", "duplicate", (g, e) => { g.Add(g.First(Removes)); return g; });
            Mutation("a Nature's Ally donor applying a native template", "ally-native-template", (g, e) => Replace(g,
                v => v.Family == SummonFamily.NaturesAlly, v => With(v, Branch(true, false), new[] { CelestialI, FiendishI })));
            Mutation("a placement with no spawn branch", "no-spawn-branch", (g, e) => Replace(g, Removes,
                v => new InheritedTemplateObservation(v.Key, v.Caller, v.Family, v.MonsterTemplated,
                    v.DeliberateTemplateAbility, v.DonorAppliedTemplateIds, new InheritedTemplateBranch[0])));
        }

        internal static void TheRuleRejectsMissingIdentities()
        {
            Assertions.Throws<ArgumentException>(() => InheritedTemplatePolicy.Decide(SummonFamily.Monster, false,
                new string[0], new HashSet<string>()), "The rule needs the native identities.");
            Assertions.Equal(InheritedTemplateDisposition.NoInheritedTemplate, InheritedTemplatePolicy.Decide(
                SummonFamily.Monster, false, new[] { "0000000000000000000000000000beef" }, NativeSet),
                "A non-native buff applied by a donor is not an inherited template and is not removed.");
        }
    }
}
