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

        // ---- Review finding 1: the production donor is checked against an independent contract ----

        private static InheritedTemplateObservation DonorObservation(string key, SummonFamily family, int tier,
            SummonMultiplicity multiplicity, string donorName, string donorGuid, string[] applied)
        {
            bool applies = applied.Length > 0 && family == SummonFamily.Monster;
            return new InheritedTemplateObservation(key, "canonical", family, false, false, applied,
                new[] { new InheritedTemplateBranch(applies ? 0 : -1, applies ? new[] { 1 } : new int[0], true, true, false, 0, true) },
                tier, multiplicity, donorGuid, donorName, new[] { applies });
        }

        private static IList<string> DonorFindings(params InheritedTemplateObservation[] graph)
        {
            return InheritedTemplateCensus.Evaluate(graph, graph.Select(value => value.Caller + "|" + value.Key), NativeSet, true).Findings;
        }

        internal static void TheProductionDonorIsCheckedIndependently()
        {
            // The contract agrees with the retained observation on every untemplated shape.
            foreach (SummonVariantSpec variant in ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Where(value => !value.Creature.MonsterTemplated))
            {
                bool retained;
                Assertions.Equal(null, InheritedTemplateDonorContract.Check(variant.Family, variant.ParentTier,
                    variant.Multiplicity, "SummonMonsterCarrier", "g" + variant.ParentTier + variant.Multiplicity,
                    Donor(variant), out retained), "Contract disagrees with the retained donor at " + variant.StableKey);
                Assertions.True(retained, "Every untemplated shape has a retained signature: " + variant.StableKey);
            }
            Assertions.True(DonorFindings(DonorObservation("ok", SummonFamily.Monster, 4, SummonMultiplicity.One,
                "SummonMonsterIVSingle", "g4s", new[] { FiendishI, CelestialII })).Count == 0, "The right donor passes.");
            Func<IList<string>, string, bool> has = (findings, text) => findings.Any(value => value.Contains(text));
            Assertions.True(has(DonorFindings(DonorObservation("single-gets-d3", SummonFamily.Monster, 4, SummonMultiplicity.One,
                "SummonMonsterIVd3", "g4d3", new[] { CelestialI, FiendishI })), ":wrong-donor:signature"),
                "Single and d3 carriers swapped (tier 4) must fail.");
            Assertions.True(has(DonorFindings(DonorObservation("d3-gets-d4", SummonFamily.Monster, 5, SummonMultiplicity.OneD3,
                "SummonMonsterVd4", "g5d4", new[] { CelestialI, FiendishI })), ":wrong-donor:signature"),
                "d3 and d4+1 carriers swapped (tier 5) must fail.");
            Assertions.True(has(DonorFindings(DonorObservation("wrong-pair", SummonFamily.Monster, 4, SummonMultiplicity.One,
                "SummonMonsterIVSingle", "g4s", new[] { CelestialI, FiendishII })), ":wrong-donor:signature"),
                "A donor with the wrong native-template pair must fail.");
            Assertions.True(has(DonorFindings(DonorObservation("ally-on-monster", SummonFamily.NaturesAlly, 8, SummonMultiplicity.OneD3,
                "SummonMonsterVIIId3", "g8d3", new string[0])), ":wrong-donor:family"),
                "A Nature's Ally placement mapped to a Summon Monster carrier must fail, even with no template to show it.");
            Assertions.True(has(DonorFindings(DonorObservation("tier-one", SummonFamily.Monster, 1, SummonMultiplicity.One,
                "SummonMonsterISingle", "notTheParent", new string[0])), ":wrong-donor:tier-one-identity"),
                "Tier one must use the native direct parent.");
            Assertions.True(has(DonorFindings(
                DonorObservation("a", SummonFamily.Monster, 3, SummonMultiplicity.One, "SummonMonsterIIISingle", "same", new[] { CelestialI, FiendishI }),
                DonorObservation("b", SummonFamily.Monster, 3, SummonMultiplicity.OneD3, "SummonMonsterIIId3", "same", new[] { CelestialI, FiendishI })),
                ":shapes-share-donor:"), "Two quantities of one tier on one carrier must fail (a swap the signature cannot see).");
            Assertions.True(has(DonorFindings(
                DonorObservation("a", SummonFamily.Monster, 3, SummonMultiplicity.One, "SummonMonsterIIISingle", "one", new[] { CelestialI, FiendishI }),
                DonorObservation("b", SummonFamily.Monster, 3, SummonMultiplicity.One, "SummonMonsterIIISingle", "two", new[] { CelestialI, FiendishI })),
                ":shape-donor-inconsistent:"), "One shape on two carriers must fail.");
            bool shapeRetained;
            Assertions.Equal(null, InheritedTemplateDonorContract.Check(SummonFamily.Monster, 2, SummonMultiplicity.OneD3,
                "SummonMonsterIId3", "g2d3", new[] { CelestialI, FiendishI }, out shapeRetained), "An unretained shape is not failed.");
            Assertions.False(shapeRetained, "An unretained shape is reported as recorded-now, not proved.");
        }

        // ---- Review finding 2: every expected alternate family child is accounted for ----

        internal static void AMissingAlternateFamilyIsReportedPerChild()
        {
            List<string> expected;
            List<InheritedTemplateObservation> graph = CorrectGraph(out expected);
            string[] families = { "MonsterTactician", "Summoner", "MasterSummoner", "Occultist", "Naturalist", "FeralHunter", "Preservationist" };
            SummonVariantSpec sample = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .First(value => !value.Creature.MonsterTemplated && Donor(value).Length == 0);
            foreach (string family in families)
            {
                expected.Add("alternate:" + family + "|" + family + ".child");
                if (family != "Summoner")
                    graph.Add(new InheritedTemplateObservation(family + ".child", "alternate:" + family, SummonFamily.Monster,
                        false, false, new string[0], new[] { Branch(false, false) }));
            }
            IList<string> findings = Judge(graph, expected).Findings;
            Assertions.Equal(1, findings.Count, "Exactly the missing family's child is reported: " + string.Join(",", findings.ToArray()));
            Assertions.True(findings[0] == "alternate:Summoner|Summoner.child:skipped", "The family that produced nothing is named.");
        }

        // ---- Review finding 3: donor immutability receipts ----

        private sealed class Node { internal string Buff; }

        private static InheritedTemplateDonorReceipt Receipt(object array, object[] components, object[] spawns,
            object[] afterArrays, object[][] afterElements, object[] actions, string[] types, string[] buffs)
        {
            return new InheritedTemplateDonorReceipt("guid", "SummonMonsterIVSingle", array, components, spawns, afterArrays,
                afterElements, actions, types, buffs, buffs.Where(NativeSet.Contains).ToArray());
        }

        internal static void DonorReceiptsSeeMutations()
        {
            object array = new object(), component = new object(), spawn = new object(), after = new object();
            object apply = new Node { Buff = CelestialI }, other = new Node(), replacement = new Node();
            string[] types = { "Conditional", "ContextActionApplyBuff" };
            InheritedTemplateDonorReceipt before = Receipt(array, new[] { component }, new[] { spawn }, new[] { after },
                new[] { new[] { other, apply } }, new[] { other, apply }, types, new[] { CelestialI });
            Assertions.Equal(0, InheritedTemplateDonorReceipt.Differences(before, Receipt(array, new[] { component },
                new[] { spawn }, new[] { after }, new[] { new[] { other, apply } }, new[] { other, apply }, types,
                new[] { CelestialI })).Count, "An unchanged donor has no differences.");
            Assertions.True(InheritedTemplateDonorReceipt.Differences(before, Receipt(array, new[] { component }, new[] { spawn },
                new[] { after }, new[] { new[] { other, replacement } }, new[] { other, replacement }, types, new[] { CelestialI }))
                .Contains("actions"), "Same action count with one action replaced must differ.");
            Assertions.True(InheritedTemplateDonorReceipt.Differences(before, Receipt(array, new[] { component }, new[] { spawn },
                new[] { after }, new[] { new[] { apply, other } }, new[] { apply, other }, types, new[] { CelestialI }))
                .Contains("after-spawn-actions"), "Changed action order must differ.");
            Assertions.True(InheritedTemplateDonorReceipt.Differences(before, Receipt(array, new[] { component }, new[] { spawn },
                new[] { after }, new[] { new[] { other, apply } }, new[] { other, apply }, types, new[] { FiendishI }))
                .Contains("apply-buff-ids"), "An ApplyBuff's buff changed in place must differ.");
            Assertions.True(InheritedTemplateDonorReceipt.Differences(before, Receipt(array, new[] { component }, new[] { spawn },
                new[] { new object() }, new[] { new[] { other, apply } }, new[] { other, apply }, types, new[] { CelestialI }))
                .Contains("after-spawn-arrays"), "An after-spawn array replaced by an equal-length one must differ.");
        }

        // ---- Review finding 4: branch-level pairing and redundancy ----

        private static IList<string> BranchFindings(out IList<string> redundant, params InheritedTemplateBranch[] branches)
        {
            var value = new InheritedTemplateObservation("Monster.4.mephit.One", "canonical", SummonFamily.Monster, false, false,
                new[] { FiendishI, CelestialII }, branches, 4, SummonMultiplicity.One, null, null, new[] { true, false });
            InheritedTemplateCensusResult result = InheritedTemplateCensus.Evaluate(new[] { value },
                new[] { "canonical|Monster.4.mephit.One" }, NativeSet);
            redundant = result.JustifiedRedundant;
            return result.Findings;
        }

        internal static void BranchesArePairedAndRedundancyIsExplicit()
        {
            IList<string> redundant;
            // Donor: branch 0 applies, branch 1 does not. A correct clone removes on both (placement-level).
            IList<string> correct = BranchFindings(out redundant,
                new InheritedTemplateBranch(0, new[] { 1 }, true, true, false, 0, true),
                new InheritedTemplateBranch(-1, new[] { 0 }, true, true, false, 1, true));
            Assertions.True(correct.Count == 0, "A correct two-branch clone passes: " + string.Join(",", correct.ToArray()));
            Assertions.True(redundant.Count == 1 && redundant[0].Contains("branch1:placement-removal-on-donor-branch-without-application"),
                "The canonical removal with nothing to remove is listed, not hidden.");
            Func<IList<string>, string, bool> has = (findings, text) => findings.Any(item => item.Contains(text));
            Assertions.True(has(BranchFindings(out redundant,
                new InheritedTemplateBranch(-1, new[] { 0 }, true, true, false, 0, true),
                new InheritedTemplateBranch(-1, new[] { 0 }, true, true, false, 1, true)), "branch0:application-lost"),
                "An applying donor branch whose clone lost the application must fail.");
            Assertions.True(has(BranchFindings(out redundant,
                new InheritedTemplateBranch(0, new[] { 1 }, true, true, false, 0, true)), "donor-branch1:branch-skipped"),
                "A skipped donor branch must fail.");
            Assertions.True(has(BranchFindings(out redundant,
                new InheritedTemplateBranch(0, new[] { 1 }, true, true, false, 0, true),
                new InheritedTemplateBranch(0, new[] { 1 }, true, true, false, 0, true)), "branch-duplicate-pairing"),
                "Two generated branches on one donor branch must fail.");
            Assertions.True(has(BranchFindings(out redundant,
                new InheritedTemplateBranch(0, new int[0], true, true, false, 0, true),
                new InheritedTemplateBranch(-1, new[] { 0 }, true, true, false, 1, true)), "branch0:omitted"),
                "A removal only on the wrong branch must fail.");
            Assertions.True(has(BranchFindings(out redundant,
                new InheritedTemplateBranch(0, new[] { 1 }, true, true, false, -1, false),
                new InheritedTemplateBranch(-1, new[] { 0 }, true, true, false, 1, true)), "branch0:branch-unpaired"),
                "A generated branch whose shape matches no donor branch must fail.");
        }
    }
}
