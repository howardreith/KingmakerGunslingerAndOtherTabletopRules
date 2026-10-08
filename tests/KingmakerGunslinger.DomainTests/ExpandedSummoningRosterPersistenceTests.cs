using System;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningRosterPersistenceTests
    {
        private static System.Collections.Generic.Dictionary<string, bool> ReadySample()
        { return RosterReadinessTimeline.SnakePredicates.Concat(new[] { "ViewExists", "ViewDataMatches", "ViewIsInGame", "DissolveFinite" }).ToDictionary(p => p, p => true); }
        internal static void ReadinessFirstTransitionsRemainCompactAndExact()
        {
            var trace = new RosterReadinessTimeline(7); var sample = ReadySample(); sample["EntityFadedIn"] = false;
            trace.Observe(7, sample); sample["EntityFadedIn"] = true; trace.Observe(12, sample);
            for (int frame = 13; frame < 608; frame++) trace.Observe(frame, sample);
            Assertions.Equal(7, trace.FirstTrue["CanMove"], "Initial true condition retains its first frame.");
            Assertions.Equal(12, trace.FirstTrue["EntityFadedIn"], "False-to-true native fader transition.");
            Assertions.Equal((int?)12, trace.FirstAllSnakeFrame, "Exact first naturally settled frame.");
            Assertions.Equal(sample.Count, trace.FirstTrue.Count, "600 samples store one frame per predicate, not a frame log.");
            trace.Clear(); Assertions.Equal(0, trace.FirstTrue.Count, "Request-local observation cleared.");
            Assertions.Equal((int?)null, trace.FirstAllSnakeFrame, "No cross-request settlement state.");
        }
        internal static void ReadinessVisibilityLossDoesNotEraseEarlierSettlementOrWaiveFailure()
        {
            var trace = new RosterReadinessTimeline(10); var sample = ReadySample(); trace.Observe(10, sample);
            sample["EntityFadedIn"] = false; sample["ViewIsInGame"] = false; trace.Observe(30, sample);
            Assertions.Equal((int?)10, trace.FirstAllSnakeFrame, "Earlier settlement stays visible to diagnosis.");
            Assertions.True(trace.FailedSnakePredicates().SequenceEqual(new[] { "EntityFadedIn" }), "Original predicate remains failed, not waived.");
            Assertions.True(trace.FailedObservedPredicates().Contains("ViewIsInGame"), "Invalid view evidence cannot be lost.");
        }
        internal static void ReadinessEveryFalsePredicateAndUnknownSampleRemainVisible()
        {
            var trace = new RosterReadinessTimeline(null); var sample = ReadySample();
            foreach (var p in RosterReadinessTimeline.SnakePredicates) sample[p] = false;
            trace.Observe(15, sample);
            Assertions.Equal(5, trace.FailedSnakePredicates().Length, "Every native predicate failure retained independently.");
            Assertions.Equal((int?)null, trace.FirstAllSnakeFrame, "Never-ready state is not invented.");
            Assertions.Equal((int?)null, trace.FirstTrueIncludingNever["CanMove"], "Never-true predicates have explicit null frames, not missing evidence.");
            sample.Remove("CanAct"); bool rejected = false;
            try { trace.Observe(16, sample); } catch (ArgumentException) { rejected = true; }
            Assertions.True(rejected, "Incomplete samples fail closed.");
        }

        internal static void NativeEvilRowCannotContaminateNextCelestialCast()
        {
            var keys = ExpandedSummoningRosterPersistencePolicy.Keys;
            int frogIndex = Array.IndexOf(keys, "poisonous-frog");
            string precedingNative = keys.Take(frogIndex).Last(k => k.StartsWith("native:", StringComparison.Ordinal));
            var native = SummonNativeExpansionCatalog.All.First(n =>
                "native:" + n.UnitGuid == precedingNative && n.Multiplicity == SummonMultiplicity.One);
            Assertions.Equal("Thanadaemon", native.CreatureKey, "Last native branch before the failed Celestial cast.");
            int alignment = ExpandedSummoningRosterPersistencePolicy.CasterAlignmentFor(native);
            Assertions.Equal(5, alignment, "Native Evil branch legitimately needs NeutralEvil.");
            var frog = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).First(v =>
                v.Creature.Key == "poisonous-frog" && v.Multiplicity == SummonMultiplicity.One && SummonVisibilityCatalog.IsPublished(v));
            Assertions.True(frog.Creature.MonsterTemplated, "Default execution is Celestial, not the neutral chooser.");
            Assertions.True((alignment & 4) != 0, "Leaked Evil alignment is illegal for the Celestial execution.");
            alignment = ExpandedSummoningRosterPersistencePolicy.CasterAlignmentFor(frog);
            Assertions.Equal(3, alignment, "Each generated cast selects its own NeutralGood alignment.");
            Assertions.True((alignment & 2) != 0 && (alignment & 4) == 0, "Good, non-Evil alignment satisfies native Celestial availability, never bypassed.");
        }

        internal static void FixtureAlignmentRespectsEveryVariantAndNativeBranch()
        {
            var evil = new[] { "hell-hound", "erinyes-devil", "shadow-demon", "succubus", "salamander", "bebelith" };
            var good = new[] { "lantern-archon", "bralani-azata", "ghaele-azata" };
            foreach (var variant in ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)))
            {
                int expected = variant.Family == SummonFamily.NaturesAlly ? 17 :
                    variant.Creature.MonsterTemplated || good.Contains(variant.Creature.Key) ? 3 :
                    evil.Contains(variant.Creature.Key) ? 12 : 1;
                Assertions.Equal(expected, ExpandedSummoningRosterPersistencePolicy.CasterAlignmentFor(variant),
                    "Independent legal cast alignment for " + variant.StableKey);
            }
            foreach (var native in SummonNativeExpansionCatalog.All)
                Assertions.Equal(native.Branch == SummonNativeSpawnBranch.Evil ? 5 : 1,
                    ExpandedSummoningRosterPersistencePolicy.CasterAlignmentFor(native), "Native branch alignment for " + native.Symbol);
        }

        internal static void EveryCreatureAndNativeUnitIsCoveredOnce()
        {
            var keys = ExpandedSummoningRosterPersistencePolicy.Keys;
            Assertions.Equal(ExpandedSummoningCatalog.All.Count + SummonNativeExpansionCatalog.All
                .Select(n => n.UnitGuid).Distinct().Count(), keys.Length, "Whole roster, not a selected fixture subset.");
            Assertions.Equal(keys.Length, keys.Distinct(StringComparer.Ordinal).Count(), "One fixture per stable unit.");
            Assertions.True(ExpandedSummoningCatalog.All.All(c => keys.Contains(c.Key)), "No KMG creature omitted.");
            Assertions.True(SummonNativeExpansionCatalog.All.All(n => keys.Contains("native:" + n.UnitGuid)), "All29 retained wrappers' unit identities covered.");
            var copy = ExpandedSummoningRosterPersistencePolicy.Keys; copy[0] = "foreign";
            Assertions.False(ExpandedSummoningRosterPersistencePolicy.Keys.Contains("foreign"), "No caller can alter policy.");
        }

        internal static void OwnershipRequiresEveryExactReceiptField()
        {
            string[] good = { ExpandedSummoningRosterPersistencePolicy.ReceiptScope, "viper", "unit", "unit", "caster", "caster", "blueprint", "blueprint", "blueprint" };
            Func<string[],bool> owns = r => ExpandedSummoningRosterPersistencePolicy.Owns(r[0],r[1],r[2],r[3],r[4],r[5],r[6],r[7],r[8]);
            Assertions.True(owns(good), "Exact receipt allows only that unit/caster/blueprint.");
            for (int i=0;i<good.Length;i++) foreach (string invalid in new[] { null, "", "foreign", good[i].ToUpperInvariant() })
            {
                var wrong = (string[])good.Clone();wrong[i]=invalid;
                Assertions.False(owns(wrong), "Unknown/malformed field cannot authorize retirement.");
            }
            good[1] = "native:" + SummonNativeExpansionCatalog.All.First().UnitGuid;
            Assertions.True(owns(good), "A native unit also needs a complete owned receipt;native identity alone is insufficient.");
        }

        internal static void ScopeCannotOpenAnotherSaveOrScenario()
        {
            foreach (string s in new[] { RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningPrepare,
                RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyCleanup,RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyAbsent })
            {
                Assertions.True(ExpandedSummoningRosterPersistencePolicy.ObserveLoad(s,"whole-roster"),"Only existing closed trio.");
                foreach(string invalid in new[] { null,"","Whole-Roster","whole-roster-extra","snakes" })
                    Assertions.False(ExpandedSummoningRosterPersistencePolicy.ObserveLoad(s,invalid),"No alias or cross-scope authority.");
            }
            foreach(string s in new[] {null,"working-save-smoke","mod-load-smoke","observe-expanded-summoning-inventory"})
                Assertions.False(ExpandedSummoningRosterPersistencePolicy.ObserveLoad(s,"whole-roster"),"No new scenario family or broad hook.");
        }
    }
}
