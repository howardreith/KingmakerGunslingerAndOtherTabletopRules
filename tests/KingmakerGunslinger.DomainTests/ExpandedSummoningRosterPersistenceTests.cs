using System;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningRosterPersistenceTests
    {
        internal static void PersistenceReadinessSeparatesNativeControlFromCurrentVisibility()
        {
            var sample = ReadySample(); sample["EntityFadedIn"] = false; sample["DissolveSettled"] = false;
            var trace = new RosterReadinessTimeline(10); trace.Observe(10, sample);
            Assertions.True(trace.ReadyForSave, "Dormant/occluded/invisible native view is save-ready after natural appearance/control settlement.");
            Assertions.Equal(10, trace.FirstPersistenceReadyFrame.Value, "Per-unit first native settlement recorded.");
            Assertions.True(trace.FailedSnakePredicates().SequenceEqual(new[] { "EntityFadedIn", "DissolveSettled" }),
                "Original creature visual contract still fails; it is never relabeled PASS.");
            foreach (string required in RosterReadinessTimeline.PersistencePredicates)
            {
                var broken = ReadySample(); broken[required] = false;
                Assertions.False(RosterReadinessTimeline.PersistenceReady(broken), "Still requires " + required);
                broken.Remove(required);
                Assertions.False(RosterReadinessTimeline.PersistenceReady(broken), "Unknown state fails closed: " + required);
            }
            Assertions.False(RosterReadinessTimeline.PersistenceReady(null), "Missing sample cannot qualify.");
        }

        internal static void PersistenceReadinessRequiresCurrentControlAndClearsHistory()
        {
            var trace = new RosterReadinessTimeline(1); var sample = ReadySample();
            sample["AppearanceBuffAbsent"] = false; sample["CanAct"] = false; sample["CanMove"] = false;
            trace.Observe(1, sample);
            Assertions.False(trace.ReadyForSave || trace.FirstPersistenceReadyFrame.HasValue, "No save while native appearance owns control.");
            trace.Observe(50, ReadySample());
            Assertions.True(trace.ReadyForSave && trace.FirstPersistenceReadyFrame == 50, "Natural lock release is recorded once.");
            trace.Observe(51, sample);
            Assertions.False(trace.ReadyForSave, "Earlier settlement never hides a renewed lock/control loss.");
            Assertions.Equal(50, trace.FirstPersistenceReadyFrame.Value, "Historical first settlement preserved.");
            trace.Clear();
            Assertions.False(trace.ReadyForSave || trace.FirstPersistenceReadyFrame.HasValue, "Finally clears all request-local readiness history.");
        }

        internal static void ReadinessFrameMapIsObjectWithNullFirstFramesAndStableKeys()
        {
            var trace = new RosterReadinessTimeline(7); var sample = ReadySample();
            sample["EntityFadedIn"] = false; trace.Observe(7, sample); trace.Observe(19, sample);
            var first = RosterReadinessReport.NullableFrameMap(trace.FirstTrueIncludingNever);
            var reordered = RosterReadinessReport.NullableFrameMap(trace.FirstTrueIncludingNever.Reverse());
            Assertions.True(first is JObject && first.Type == JTokenType.Object, "Predicate map must be a JSON object, never an array.");
            Assertions.Equal(JTokenType.Null, first["EntityFadedIn"].Type, "Never-true predicate is explicit JSON null.");
            Assertions.Equal(7, (int)first["CanMove"], "First true frame survives subsequent samples.");
            Assertions.True(first.Properties().Select(p => p.Name).SequenceEqual(sample.Keys.OrderBy(k => k, StringComparer.Ordinal)),
                "Every observed predicate name is retained in ordinal order.");
            Assertions.Equal(first.ToString(Formatting.None), reordered.ToString(Formatting.None), "Input enumeration order cannot alter evidence.");
        }

        internal static void ReadinessFrameMapIgnoresAmbientGameSerializer()
        {
            var original = JsonConvert.DefaultSettings;
            JsonConvert.DefaultSettings = () => { throw new InvalidOperationException("Game serializer must not be consulted."); };
            try
            {
                var map = RosterReadinessReport.NullableFrameMap(new[] {
                    new System.Collections.Generic.KeyValuePair<string, int?>("never", null),
                    new System.Collections.Generic.KeyValuePair<string, int?>("first", 17) });
                Assertions.Equal("{\"first\":17,\"never\":null}", map.ToString(Formatting.None), "Explicit JTokens bypass configured converters.");
            }
            finally { JsonConvert.DefaultSettings = original; }
        }

        private static JObject ReadinessRow(string key, string id, params string[] failures)
        {
            return new JObject { ["key"] = key, ["unitId"] = id,
                ["failedPredicateNames"] = new JArray(failures),
                ["failedObservedPredicateNames"] = new JArray(failures),
                ["firstFrameAllSnakePredicates"] = JValue.CreateNull() };
        }

        internal static void ReadinessReportPreservesRowsAfterFormattingFailure()
        {
            var report = new RosterReadinessReport("request", "prepare", 3);
            Assertions.Equal(0, ((JArray)report.Result["units"]).Count, "Top-level report exists before row construction.");
            report.AddUnit("first", "u1", () => ReadinessRow("first", "u1", "EntityFadedIn"));
            report.AddUnit("broken", "u2", () => { throw new InvalidOperationException("Injected formatting failure"); });
            report.AddUnit("last", "u3", () => ReadinessRow("last", "u3"));
            bool cleared = false; report.Finish(() => { cleared = true; });
            Assertions.True(cleared && (bool)report.Result["observerClearedInFinally"], "Failure still clears request-local tracking.");
            Assertions.Equal(2, (int)report.Result["successfulUnitRows"], "Other unit rows are preserved.");
            Assertions.True(report.HasErrors, "A serialized observer error still fails the scenario.");
            var error = report.Result["observerErrors"][0];
            Assertions.Equal("broken", (string)error["key"], "Exact error key.");
            Assertions.Equal("u2", (string)error["unitId"], "Exact error unit.");
            Assertions.Equal(typeof(InvalidOperationException).FullName, (string)error["exceptionType"], "Exception type retained.");
            Assertions.Equal("Injected formatting failure", (string)error["message"], "Exception message retained.");
            Assertions.Equal("per-unit-final-state-row", (string)error["observerStage"], "Observer stage retained.");
            Assertions.Equal(1, (int)report.Result["summaryByFailedPredicate"]["EntityFadedIn"]["count"], "Summary uses successful rows only.");
            Assertions.Equal(1, ((JArray)report.Result["failingUnits"]).Count, "Observer errors remain separate from actual readiness failures.");
            Assertions.True(JObject.Parse(report.Result.ToString(Formatting.None))["observerErrors"] is JArray, "Report remains writable despite the row error.");
        }

        internal static void ReadinessReportNeverClaimsFailedClearSucceeded()
        {
            var report = new RosterReadinessReport("request", "prepare", 1);
            report.AddUnit("unit", "id", () => ReadinessRow("unit", "id"));
            report.Finish(() => { throw new InvalidOperationException("Injected clear failure"); });
            Assertions.True(report.HasErrors, "Failed clearing cannot qualify.");
            Assertions.False((bool)report.Result["observerClearedInFinally"], "Never claim unsuccessful clearing passed.");
            Assertions.Equal("clear-request-local-timelines", (string)report.Result["observerErrors"][0]["observerStage"], "Clear failure is diagnosable.");
            Assertions.Equal(1, ((JArray)JObject.Parse(report.Result.ToString(Formatting.None))["units"]).Count, "Completed row evidence survives clearing failure.");
        }

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
