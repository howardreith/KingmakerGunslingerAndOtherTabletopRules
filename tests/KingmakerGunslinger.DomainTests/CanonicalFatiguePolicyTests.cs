using System;
using System.IO;
using KingmakerGunslinger.Fatigue;

namespace KingmakerGunslinger.DomainTests
{
    internal static class CanonicalFatiguePolicyTests
    {
        internal static void FreshFatigueAppliesFatigued()
        {
            Assert(CanonicalFatigueState.Neither,
                CanonicalConditionKind.Fatigued, true,
                CanonicalFatigueState.Fatigued,
                CanonicalConditionKind.Fatigued, false);
        }

        internal static void RepeatedFatigueEscalates()
        {
            Assert(CanonicalFatigueState.Fatigued,
                CanonicalConditionKind.Fatigued, true,
                CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Exhausted, true);
        }

        internal static void ExhaustedFatigueNeverDowngrades()
        {
            Assert(CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Fatigued, true,
                CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Exhausted, false);
        }

        internal static void FreshExhaustionAppliesExhausted()
        {
            Assert(CanonicalFatigueState.Neither,
                CanonicalConditionKind.Exhausted, true,
                CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Exhausted, false);
        }

        internal static void FatiguedExhaustionReplacesFatigue()
        {
            Assert(CanonicalFatigueState.Fatigued,
                CanonicalConditionKind.Exhausted, true,
                CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Exhausted, false);
        }

        internal static void ExhaustedExhaustionIsIdempotent()
        {
            Assert(CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Exhausted, true,
                CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Exhausted, false);
        }

        internal static void SameSequenceFatigueIsDeterministic()
        {
            CanonicalFatigueStateDecision first =
                CanonicalFatigueStatePolicy.Decide(
                    CanonicalFatigueState.Neither,
                    CanonicalConditionKind.Fatigued, true);
            CanonicalFatigueStateDecision second =
                CanonicalFatigueStatePolicy.Decide(first.After,
                    CanonicalConditionKind.Fatigued, true);
            Assertions.True(first.After == CanonicalFatigueState.Fatigued &&
                second.After == CanonicalFatigueState.Exhausted &&
                second.Escalated,
                "Two same-sequence successful fatigue effects were not deterministic.");
        }

        internal static void BlockedApplicationsPreserveState()
        {
            foreach (CanonicalFatigueState state in new[] {
                CanonicalFatigueState.Neither,
                CanonicalFatigueState.Fatigued,
                CanonicalFatigueState.Exhausted })
            foreach (CanonicalConditionKind incoming in new[] {
                CanonicalConditionKind.Fatigued,
                CanonicalConditionKind.Exhausted })
            {
                CanonicalFatigueStateDecision decision =
                    CanonicalFatigueStatePolicy.Decide(state, incoming, false);
                Assertions.True(!decision.ApplicationSucceeded &&
                    decision.After == state && !decision.Escalated,
                    "A blocked canonical condition changed state.");
            }
        }

        internal static void ShorterDurationCannotShortenCondition()
        {
            CanonicalConditionDuration longer =
                CanonicalConditionDuration.Temporary(1000L);
            CanonicalConditionDuration shorter =
                CanonicalConditionDuration.Temporary(400L);
            Assertions.True(CanonicalConditionDuration.PreserveLongest(longer,
                    shorter).Equals(longer) &&
                CanonicalConditionDuration.PreserveLongest(shorter,
                    longer).Equals(longer),
                "A shorter second condition shortened the impairment.");
        }

        internal static void PermanentDurationDominatesTemporary()
        {
            CanonicalConditionDuration permanent =
                CanonicalConditionDuration.PermanentDuration();
            CanonicalConditionDuration temporary =
                CanonicalConditionDuration.Temporary(1000L);
            Assertions.True(CanonicalConditionDuration.PreserveLongest(
                    permanent, temporary).Permanent &&
                CanonicalConditionDuration.PreserveLongest(temporary,
                    permanent).Permanent,
                "An independent permanent condition was shortened.");
        }

        internal static void CordReceivesEffectiveIncomingCondition()
        {
            CanonicalFatigueStateDecision fresh =
                CanonicalFatigueStatePolicy.Decide(
                    CanonicalFatigueState.Neither,
                    CanonicalConditionKind.Fatigued, true);
            CanonicalFatigueStateDecision repeated =
                CanonicalFatigueStatePolicy.Decide(
                    CanonicalFatigueState.Fatigued,
                    CanonicalConditionKind.Fatigued, true);
            Assertions.True(fresh.EffectiveIncoming ==
                    CanonicalConditionKind.Fatigued &&
                repeated.EffectiveIncoming ==
                    CanonicalConditionKind.Exhausted,
                "Cord ordering did not receive the post-escalation condition kind.");
        }

        internal static void RuntimeCoordinatorUsesExactPostSuccessBoundary()
        {
            string source = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Fatigue", "CanonicalFatigueApplicationRuntime.cs"));
            foreach (string token in new[] {
                "FatiguedGuid =",
                "ExhaustedGuid =",
                "[ThreadStatic] private static ApplicationScope _activeScope",
                "[ThreadStatic] private static int _replacementDepth",
                "[HarmonyPatch(typeof(BuffCollection), \"TriggerRuleApplyBuff\"",
                "[HarmonyAfter(\"CallOfTheWild\")]",
                "if (result == null)",
                "blocked-by-native-rule",
                "if (!NativeConditionPresent(scope))",
                "blocked-by-native-condition-immunity",
                "CanonicalFatigueStatePolicy.Decide",
                "SelectLongest(",
                "PreserveLongestDuration(",
                "ApplyRelated(scope.Buffs, exhausted, source)",
                "CordConditionRuntime.ResolveCanonical",
                "RemoveAll(scope.Buffs, fatigued, null)",
                "Normalize(scope.Buffs, exhausted, replacement)",
                "private static Exception Finalizer",
                "CanonicalFatigueApplicationRuntime.End(__state)" })
                Assertions.True(source.Contains(token),
                    "Canonical runtime lacks exact coordinator contract: " +
                    token);
            Assertions.False(source.Contains(".name.Contains") ||
                source.Contains("Description.Contains") ||
                source.Contains("skipOriginal = true"),
                "Canonical fatigue must use exact identities after native success.");
        }

        internal static void GuardedScenariosUseActualCanonicalFacts()
        {
            string runner = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            foreach (string token in new[] {
                "RunDisposableFatigueEscalation()",
                "CanonicalFatigueApplicationRuntime.FatiguedGuid",
                "CanonicalFatigueApplicationRuntime.ExhaustedGuid",
                "unit.Descriptor.Buffs.AddBuff(fatigued",
                "unit.Descriptor.Buffs.AddBuff(exhausted",
                "AddConditionImmunity(\n                    UnitCondition.Fatigued)",
                "UnitSerialization.Serialize(unit.Descriptor)",
                "ToObject<\n                    UnitDescriptor>()",
                "Buffs.SetupPreview(\n                        serializedDescriptor)",
                "serializedUnit.Dispose()",
                "RestController.ApplyRest",
                "secondFailureEscalated",
                "exhaustedRepeatStable",
                "canonicalRepeated",
                "canonicalSameFrame" })
                Assertions.True(runner.Contains(token),
                    "Guarded fatigue qualification lacks actual-path token: " +
                    token);
            Assertions.False(runner.Contains(
                    "JsonConvert.SerializeObject(\n                    permanentExhaustion"),
                "A live Buff cannot be serialized as a standalone native save root.");
            string catalog = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs"));
            string automation = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "scripts",
                "RuntimeAutomation.Common.ps1"));
            Assertions.True(catalog.Contains(
                    "\"disposable-fatigue-escalation\"") &&
                automation.Contains(
                    "'disposable-fatigue-escalation' = [pscustomobject]@{") &&
                automation.Contains("RequiresManualInteraction = $false"),
                "The focused fatigue scenario is not safely guarded and cataloged.");
        }

        private static void Assert(CanonicalFatigueState before,
            CanonicalConditionKind incoming, bool succeeded,
            CanonicalFatigueState after,
            CanonicalConditionKind effective, bool escalated)
        {
            CanonicalFatigueStateDecision decision =
                CanonicalFatigueStatePolicy.Decide(before, incoming,
                    succeeded);
            Assertions.True(decision.Before == before &&
                decision.Incoming == incoming &&
                decision.ApplicationSucceeded == succeeded &&
                decision.After == after &&
                decision.EffectiveIncoming == effective &&
                decision.Escalated == escalated,
                "Canonical fatigue transition mismatch for " + before +
                " + " + incoming + ".");
        }
    }
}
