using System;
using System.IO;
using System.Threading;
using KingmakerGunslinger.Fatigue;

namespace KingmakerGunslinger.DomainTests
{
    internal static class CanonicalFatiguePolicyTests
    {
        private const CanonicalFatigueApplicationIntent Native =
            CanonicalFatigueApplicationIntent.NativePassthrough;
        private const CanonicalFatigueApplicationIntent Acadamae =
            CanonicalFatigueApplicationIntent.EscalateIfAlreadyFatigued;

        internal static void FreshOrdinaryFatigueAppliesFatigued()
        {
            Assert(CanonicalFatigueState.Neither,
                CanonicalConditionKind.Fatigued, true, Native,
                CanonicalFatigueState.Fatigued,
                CanonicalConditionKind.Fatigued, false);
        }

        internal static void RepeatedOrdinaryFatigueRemainsFatigued()
        {
            Assert(CanonicalFatigueState.Fatigued,
                CanonicalConditionKind.Fatigued, true, Native,
                CanonicalFatigueState.Fatigued,
                CanonicalConditionKind.Fatigued, false);
        }

        internal static void SameSequenceOrdinaryFatigueRemainsFatigued()
        {
            CanonicalFatigueStateDecision first =
                CanonicalFatigueStatePolicy.Decide(
                    CanonicalFatigueState.Neither,
                    CanonicalConditionKind.Fatigued, true, Native);
            CanonicalFatigueStateDecision second =
                CanonicalFatigueStatePolicy.Decide(first.After,
                    CanonicalConditionKind.Fatigued, true, Native);
            Assertions.True(first.After == CanonicalFatigueState.Fatigued &&
                second.After == CanonicalFatigueState.Fatigued &&
                second.EffectiveIncoming ==
                    CanonicalConditionKind.Fatigued &&
                !second.Escalated,
                "Synchronous ordinary fatigue was reinterpreted as exhaustion.");
        }

        internal static void ExhaustedOrdinaryFatigueNeverDowngrades()
        {
            Assert(CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Fatigued, true, Native,
                CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Fatigued, false);
        }

        internal static void FreshNativeExhaustionAppliesExhausted()
        {
            Assert(CanonicalFatigueState.Neither,
                CanonicalConditionKind.Exhausted, true, Native,
                CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Exhausted, false);
        }

        internal static void FatiguedNativeExhaustionReplacesFatigue()
        {
            Assert(CanonicalFatigueState.Fatigued,
                CanonicalConditionKind.Exhausted, true, Native,
                CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Exhausted, false);
        }

        internal static void ExhaustedNativeExhaustionIsIdempotent()
        {
            Assert(CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Exhausted, true, Native,
                CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Exhausted, false);
        }

        internal static void FreshAcadamaeFatigueAppliesFatigued()
        {
            Assert(CanonicalFatigueState.Neither,
                CanonicalConditionKind.Fatigued, true, Acadamae,
                CanonicalFatigueState.Fatigued,
                CanonicalConditionKind.Fatigued, false);
        }

        internal static void RepeatedAcadamaeFatigueEscalates()
        {
            Assert(CanonicalFatigueState.Fatigued,
                CanonicalConditionKind.Fatigued, true, Acadamae,
                CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Exhausted, true);
        }

        internal static void ExhaustedAcadamaeFatigueRemainsExhausted()
        {
            Assert(CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Fatigued, true, Acadamae,
                CanonicalFatigueState.Exhausted,
                CanonicalConditionKind.Exhausted, false);
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
            foreach (CanonicalFatigueApplicationIntent intent in new[] {
                Native, Acadamae })
            {
                CanonicalFatigueStateDecision decision =
                    CanonicalFatigueStatePolicy.Decide(state, incoming,
                        false, intent);
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
                "An explicit permanent condition was shortened.");
        }

        internal static void CordReceivesIntentSpecificIncomingCondition()
        {
            CanonicalFatigueStateDecision ordinaryRepeated =
                CanonicalFatigueStatePolicy.Decide(
                    CanonicalFatigueState.Fatigued,
                    CanonicalConditionKind.Fatigued, true, Native);
            CanonicalFatigueStateDecision acadamaeRepeated =
                CanonicalFatigueStatePolicy.Decide(
                    CanonicalFatigueState.Fatigued,
                    CanonicalConditionKind.Fatigued, true, Acadamae);
            CanonicalFatigueStateDecision nativeExhausted =
                CanonicalFatigueStatePolicy.Decide(
                    CanonicalFatigueState.Fatigued,
                    CanonicalConditionKind.Exhausted, true, Native);
            Assertions.True(ordinaryRepeated.EffectiveIncoming ==
                    CanonicalConditionKind.Fatigued &&
                acadamaeRepeated.EffectiveIncoming ==
                    CanonicalConditionKind.Exhausted &&
                nativeExhausted.EffectiveIncoming ==
                    CanonicalConditionKind.Exhausted,
                "Cord did not receive the request-specific incoming condition.");
        }

        internal static void IntentMatchesExactCollectionAndBlueprint()
        {
            object firstCollection = new object();
            object secondCollection = new object();
            object expectedBlueprint = new object();
            object otherBlueprint = new object();
            using (CanonicalFatigueApplicationIntentScope.Request request =
                CanonicalFatigueApplicationIntentScope
                    .EnterAcadamaeEscalation(firstCollection,
                        expectedBlueprint))
            {
                Assertions.Equal(Native,
                    CanonicalFatigueApplicationIntentScope.Claim(
                        secondCollection, expectedBlueprint),
                    "Acadamae intent leaked to a second unit collection.");
                Assertions.Equal(Native,
                    CanonicalFatigueApplicationIntentScope.Claim(
                        firstCollection, otherBlueprint),
                    "Acadamae intent leaked to another canonical blueprint.");
                Assertions.Equal(Acadamae,
                    CanonicalFatigueApplicationIntentScope.Claim(
                        firstCollection, expectedBlueprint),
                    "The exact Acadamae request was not claimed.");
            }
        }

        internal static void IntentIsOneShotAndDoesNotLeakLater()
        {
            object collection = new object();
            object blueprint = new object();
            using (CanonicalFatigueApplicationIntentScope.Request request =
                CanonicalFatigueApplicationIntentScope
                    .EnterAcadamaeEscalation(collection, blueprint))
            {
                Assertions.Equal(Acadamae,
                    CanonicalFatigueApplicationIntentScope.Claim(collection,
                        blueprint),
                    "The explicit request was not claimed.");
                Assertions.Equal(Native,
                    CanonicalFatigueApplicationIntentScope.Claim(collection,
                        blueprint),
                    "One explicit request affected a second application.");
            }
            Assertions.Equal(Native,
                CanonicalFatigueApplicationIntentScope.Claim(collection,
                    blueprint),
                "Disposed Acadamae intent leaked to a later native request.");
        }

        internal static void NestedUnrelatedIntentCannotClaimOuterRequest()
        {
            object outerCollection = new object();
            object innerCollection = new object();
            object blueprint = new object();
            using (CanonicalFatigueApplicationIntentScope.Request outer =
                CanonicalFatigueApplicationIntentScope
                    .EnterAcadamaeEscalation(outerCollection, blueprint))
            {
                using (CanonicalFatigueApplicationIntentScope.Request inner =
                    CanonicalFatigueApplicationIntentScope
                        .EnterAcadamaeEscalation(innerCollection, blueprint))
                {
                    Assertions.Equal(Native,
                        CanonicalFatigueApplicationIntentScope.Claim(
                            outerCollection, blueprint),
                        "A nested unrelated request exposed outer intent.");
                    Assertions.Equal(Acadamae,
                        CanonicalFatigueApplicationIntentScope.Claim(
                            innerCollection, blueprint),
                        "The nested exact request was not claimed.");
                }
                Assertions.Equal(Acadamae,
                    CanonicalFatigueApplicationIntentScope.Claim(
                        outerCollection, blueprint),
                    "Nested cleanup consumed the outer exact request.");
            }
        }

        internal static void ExceptionalIntentScopeCannotLeak()
        {
            object collection = new object();
            object blueprint = new object();
            try
            {
                using (CanonicalFatigueApplicationIntentScope.Request request =
                    CanonicalFatigueApplicationIntentScope
                        .EnterAcadamaeEscalation(collection, blueprint))
                {
                    throw new InvalidOperationException("expected");
                }
            }
            catch (InvalidOperationException exception)
            {
                Assertions.Equal("expected", exception.Message,
                    "Unexpected exception escaped the intent cleanup test.");
            }
            Assertions.Equal(Native,
                CanonicalFatigueApplicationIntentScope.Claim(collection,
                    blueprint),
                "Exceptional Acadamae application leaked intent.");
        }

        internal static void IntentIsThreadLocal()
        {
            object collection = new object();
            object blueprint = new object();
            CanonicalFatigueApplicationIntent otherThread = Acadamae;
            using (CanonicalFatigueApplicationIntentScope.Request request =
                CanonicalFatigueApplicationIntentScope
                    .EnterAcadamaeEscalation(collection, blueprint))
            {
                var thread = new Thread(() =>
                    otherThread =
                        CanonicalFatigueApplicationIntentScope.Claim(
                            collection, blueprint));
                thread.Start();
                thread.Join();
                Assertions.Equal(Native, otherThread,
                    "Acadamae intent crossed a managed thread boundary.");
                Assertions.Equal(Acadamae,
                    CanonicalFatigueApplicationIntentScope.Claim(collection,
                        blueprint),
                    "Other-thread probing consumed the owning thread request.");
            }
        }

        internal static void RuntimeCoordinatorUsesExactPostSuccessBoundary()
        {
            string root = Environment.CurrentDirectory;
            string runtime = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Fatigue",
                "CanonicalFatigueApplicationRuntime.cs"));
            string intent = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Fatigue",
                "CanonicalFatigueApplicationIntentScope.cs"));
            string acadamae = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Acadamae",
                "AcadamaeCastingPatches.cs"));
            foreach (string token in new[] {
                "FatiguedGuid =",
                "ExhaustedGuid =",
                "[ThreadStatic] private static ApplicationScope _activeScope",
                @"[HarmonyPatch(typeof(BuffCollection), ""TriggerRuleApplyBuff""",
                @"[HarmonyAfter(""CallOfTheWild"")]",
                "if (result == null)",
                "blocked-by-native-rule",
                "if (!NativeConditionPresent(scope))",
                "blocked-by-native-condition-immunity",
                "CanonicalFatigueApplicationIntentScope.Claim",
                "scope.Incoming, true, scope.Intent",
                "ResolveNativePassthrough",
                "CordConditionRuntime.ResolveCanonical",
                "private static Exception Finalizer",
                "CanonicalFatigueApplicationRuntime.End(__state)" })
                Assertions.True(runtime.Contains(token),
                    "Canonical runtime lacks exact coordinator contract: " +
                    token);
            foreach (string token in new[] {
                "[ThreadStatic] private static Request _active",
                "EnterAcadamaeEscalation(",
                "ReferenceEquals(request.BuffCollection, buffCollection)",
                "ReferenceEquals(request.ExpectedBlueprint, blueprint)",
                "request.Claimed = true",
                "if (ReferenceEquals(_active, this))" })
                Assertions.True(intent.Contains(token),
                    "Request-local fatigue intent lacks safety contract: " +
                    token);
            Assertions.True(acadamae.Contains(
                    ".ApplyPermanentAcadamaeFatigue(") &&
                runtime.Contains("using (CanonicalFatigueApplicationIntentScope.Request") &&
                runtime.Contains(".EnterAcadamaeEscalation(buffs, fatigued)"),
                "Only the Acadamae adapter may enter escalating intent.");
            Assertions.False(runtime.Contains(".name.Contains") ||
                runtime.Contains("Description.Contains") ||
                runtime.Contains("StackTrace") ||
                runtime.Contains("UnitPartWeariness") ||
                runtime.Contains("GlobalMap") ||
                runtime.Contains("skipOriginal = true") ||
                acadamae.Contains("ApplyPermanentFatigue"),
                "Canonical fatigue intent must not use heuristics, travel patches, or the ambiguous old adapter.");
        }

        internal static void GuardedScenariosUseActualCanonicalFacts()
        {
            string runner = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            foreach (string token in new[] {
                "RunDisposableNativeFatigueRefresh()",
                "RunDisposableAcadamaeFatigueEscalation()",
                "CanonicalFatigueApplicationRuntime.FatiguedGuid",
                "CanonicalFatigueApplicationRuntime.ExhaustedGuid",
                "unit.Descriptor.Buffs.AddBuff(fatigued",
                "unit.Descriptor.Buffs.AddBuff(exhausted",
                "AddConditionImmunity(",
                "UnitCondition.Fatigued",
                "RestController.ApplyRest",
                "ApplyPermanentAcadamaeFatigue(",
                "StartFatiguePersistence()",
                "WorkingSaveFatiguePrepare",
                "WorkingSaveFatigueVerifyCleanup",
                "WorkingSaveFatigueVerifyAbsent",
                "_workingSaveSmoke.ArmExactWorkingSaveWrite()",
                "freshly deserialized native BuffCollection",
                "secondFailureEscalated",
                "exhaustedRepeatStable",
                "nativeRepeated",
                "nativeSameFrame",
                "acadamaeEscalated" })
                Assertions.True(runner.Contains(token),
                    "Guarded fatigue qualification lacks actual-path token: " +
                    token);
            Assertions.False(runner.Contains(
                    "UnitSerialization.Serialize(unit.Descriptor)"),
                "Fatigue persistence must not use a descriptor-only surrogate save.");

            string catalog = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs"));
            string automation = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "scripts",
                "RuntimeAutomation.Common.ps1"));
            string quote = ((char)34).ToString();
            foreach (string scenario in new[] {
                "disposable-native-fatigue-refresh",
                "disposable-acadamae-fatigue-escalation",
                "working-save-fatigue-prepare",
                "working-save-fatigue-verify-cleanup",
                "working-save-fatigue-verify-absent" })
            {
                Assertions.True(catalog.Contains(quote + scenario + quote) &&
                    automation.Contains("'" + scenario +
                        "' = [pscustomobject]@{"),
                    "Focused fatigue scenario is not guarded: " + scenario);
            }
            Assertions.True(automation.Contains(
                    "RequiresManualInteraction = $false"),
                "Focused fatigue scenarios must be autonomous.");

            string orchestrator = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "scripts",
                "Invoke-FatigueWorkingSavePersistence.ps1"));
            foreach (string token in new[] {
                "[ValidateSet('KMG_AUTOMATION_WORKING')]",
                "working-save-fatigue-prepare",
                "working-save-fatigue-verify-cleanup",
                "working-save-fatigue-verify-absent",
                "-ReuseInstalledArtifact",
                "Wait-ForGuardedKingmakerExit" })
                Assertions.True(orchestrator.Contains(token),
                    "Fatigue persistence orchestrator lacks guarded token: " +
                    token);
            Assertions.False(orchestrator.Contains("Kingmaker.exe") ||
                orchestrator.Contains("KMG_AUTOMATION_BASELINE"),
                "Fatigue persistence must preserve the guarded Steam working-save boundary.");
        }

        private static void Assert(CanonicalFatigueState before,
            CanonicalConditionKind incoming, bool succeeded,
            CanonicalFatigueApplicationIntent intent,
            CanonicalFatigueState after,
            CanonicalConditionKind effective, bool escalated)
        {
            CanonicalFatigueStateDecision decision =
                CanonicalFatigueStatePolicy.Decide(before, incoming,
                    succeeded, intent);
            Assertions.True(decision.Before == before &&
                decision.Incoming == incoming &&
                decision.ApplicationSucceeded == succeeded &&
                decision.Intent == intent &&
                decision.After == after &&
                decision.EffectiveIncoming == effective &&
                decision.Escalated == escalated,
                "Canonical fatigue transition mismatch for " + before +
                " + " + incoming + " under " + intent + ".");
        }
    }
}
