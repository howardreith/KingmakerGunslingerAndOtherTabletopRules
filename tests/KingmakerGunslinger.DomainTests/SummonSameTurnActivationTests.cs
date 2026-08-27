using System;
using System.IO;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SummonSameTurnActivationTests
    {
        internal static void OutsideCombatIsNative()
        {
            SummonSameTurnActivationRequest request = Repairable();
            request.InCombat = false;
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.NotInCombat);
            SummonTurnEnrollmentRequest enrollment = EnrollmentReady();
            enrollment.InCombat = false;
            AssertEnrollmentNo(enrollment,
                SummonTurnEnrollmentDisposition.NotInCombat);
        }

        internal static void RealTimeWithPauseIsNative()
        {
            SummonSameTurnActivationRequest request = Repairable();
            request.TurnBased = false;
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.RealTimeWithPause);
            SummonTurnEnrollmentRequest enrollment = EnrollmentReady();
            enrollment.TurnBased = false;
            AssertEnrollmentNo(enrollment,
                SummonTurnEnrollmentDisposition.RealTimeWithPause);
        }

        internal static void NonSummonsAreNative()
        {
            SummonSameTurnActivationRequest request = Repairable();
            request.GenuineSummonRule = false;
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.NotGenuineSummon);

            request = Repairable();
            request.SummoningSpell = false;
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.NotSummoningSpell);

            SummonTurnEnrollmentRequest enrollment = EnrollmentReady();
            enrollment.GenuineSummon = false;
            AssertEnrollmentNo(enrollment,
                SummonTurnEnrollmentDisposition.NotGenuineSummon);
        }

        internal static void OutsideCasterTurnIsNative()
        {
            SummonSameTurnActivationRequest request = Repairable();
            request.CasterOwnsCurrentTurn = false;
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.OutsideCasterTurn);
            SummonTurnEnrollmentRequest enrollment = EnrollmentReady();
            enrollment.CreatedDuringCasterTurn = false;
            AssertEnrollmentNo(enrollment,
                SummonTurnEnrollmentDisposition.OutsideCasterTurn);
        }

        internal static void NativeFullRoundAndImmediatePathsAreNative()
        {
            SummonSameTurnActivationRequest request = Repairable();
            request.ActualRequiresFullRound = true;
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.NativeFullRoundInvocation);

            request = Repairable();
            request.BlueprintRequiresFullRound = false;
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.NativeAlreadyImmediate);
        }

        internal static void AlreadyEligibleSummonIsNotDuplicated()
        {
            SummonSameTurnActivationRequest request = Repairable();
            Normalize(request);
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.AlreadyEligible);
            AssertEnrollmentNo(EnrollmentReady(),
                SummonTurnEnrollmentDisposition.NativeReady);
        }

        internal static void CorrelatedAcceleratedCommandOverridesStaleGetter()
        {
            SummonSameTurnActivationRequest request = Repairable();
            request.ActualRequiresFullRound = true;
            request.AcceleratedCommandCorrelated = true;
            SummonSameTurnActivationDecision decision =
                SummonSameTurnActivationPolicy.Evaluate(request);
            Assertions.True(decision.ShouldRepair &&
                decision.RemoveAppearanceLock &&
                decision.RemoveLifecycleGrace,
                "An exact accelerated UnitUseAbility correlation did not " +
                "override the post-spend full-round getter value.");
        }

        internal static void NormalizedAcceleratedCommandRemainsIdempotent()
        {
            SummonSameTurnActivationRequest request = Repairable();
            request.ActualRequiresFullRound = true;
            request.AcceleratedCommandCorrelated = false;
            Normalize(request);
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.AlreadyEligible);
        }

        internal static void MissingOpportunityIsRepairedExactly()
        {
            SummonSameTurnActivationDecision decision =
                SummonSameTurnActivationPolicy.Evaluate(Repairable());
            Assertions.True(decision.ShouldRepair &&
                decision.RemoveAppearanceLock &&
                decision.RemoveLifecycleGrace,
                "A proven accelerated summon did not remove exactly the " +
                "appearance lock and native six-second lifecycle grace.");

            SummonTurnEnrollmentRequest enrollment = EnrollmentReady();
            enrollment.LiveSummonCount = 0;
            SummonTurnEnrollmentDecision enrollmentDecision =
                SummonTurnEnrollmentPolicy.Evaluate(enrollment);
            Assertions.True(enrollmentDecision.HoldCasterEnd &&
                enrollmentDecision.Disposition ==
                    SummonTurnEnrollmentDisposition.AwaitWorldRegistration,
                "A deferred successful summon did not preserve its native " +
                "current-caster enrollment boundary.");
        }

        internal static void AlreadyActedSummonIsNotReactivated()
        {
            SummonSameTurnActivationRequest request = Repairable();
            request.SummonAlreadyActed = true;
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.AlreadyActed);
            SummonTurnEnrollmentRequest enrollment = EnrollmentReady();
            enrollment.AlreadyActedCount = 1;
            AssertEnrollmentNo(enrollment,
                SummonTurnEnrollmentDisposition.AlreadyActed);
        }

        internal static void DuplicateCallbackIsANoOp()
        {
            SummonSameTurnActivationRequest request = Repairable();
            SummonSameTurnActivationDecision first =
                SummonSameTurnActivationPolicy.Evaluate(request);
            Assertions.True(first.ShouldRepair,
                "The first exact summon callback was not repairable.");
            Normalize(request);
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.AlreadyEligible);

            SummonTurnEnrollmentRequest enrollment = EnrollmentReady();
            AssertEnrollmentNo(enrollment,
                SummonTurnEnrollmentDisposition.NativeReady);
            AssertEnrollmentNo(enrollment,
                SummonTurnEnrollmentDisposition.NativeReady);
        }

        internal static void EverySpawnedUnitIsIndependentlyEligible()
        {
            SummonSameTurnActivationRequest first = Repairable();
            SummonSameTurnActivationRequest second = Repairable();
            SummonSameTurnActivationRequest third = Repairable();
            Assertions.True(SummonSameTurnActivationPolicy.Evaluate(first)
                    .ShouldRepair &&
                SummonSameTurnActivationPolicy.Evaluate(second)
                    .ShouldRepair &&
                SummonSameTurnActivationPolicy.Evaluate(third)
                    .ShouldRepair,
                "A multi-creature invocation was incorrectly keyed by " +
                "caster or spell instead of each RuleSummonUnit result.");
            Normalize(first);
            Assertions.False(SummonSameTurnActivationPolicy.Evaluate(first)
                    .ShouldRepair,
                "Normalizing one summoned unit changed its siblings.");
            Assertions.True(SummonSameTurnActivationPolicy.Evaluate(second)
                    .ShouldRepair &&
                SummonSameTurnActivationPolicy.Evaluate(third)
                    .ShouldRepair,
                "Normalizing one summoned unit suppressed another unit.");

            SummonTurnEnrollmentRequest enrollment = EnrollmentReady();
            enrollment.SuccessfulSummonCount = 3;
            enrollment.LiveSummonCount = 3;
            enrollment.CombatEnrolledCount = 3;
            enrollment.TurnOrderMemberCount = 3;
            enrollment.InitiativePreparedCount = 2;
            Assertions.True(SummonTurnEnrollmentPolicy.Evaluate(enrollment)
                    .HoldCasterEnd,
                "A multi-summon window released before every unique unit " +
                "completed native initiative enrollment.");
            enrollment.InitiativePreparedCount = 3;
            AssertEnrollmentNo(enrollment,
                SummonTurnEnrollmentDisposition.NativeReady);
        }

        internal static void FollowingRoundHasNoStaleActivation()
        {
            SummonSameTurnActivationRequest request = Repairable();
            Normalize(request);
            request.CasterOwnsCurrentTurn = false;
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.OutsideCasterTurn);
            SummonTurnEnrollmentRequest enrollment = EnrollmentReady();
            enrollment.SameRound = false;
            AssertEnrollmentNo(enrollment,
                SummonTurnEnrollmentDisposition.StaleRound);
        }

        internal static void AmbiguousStateFailsClosed()
        {
            SummonSameTurnActivationRequest request = Repairable();
            request.ObservedLifecycleSeconds = 143d;
            AssertNoRepair(request, SummonSameTurnActivationDisposition
                .AmbiguousLifecycleDuration);

            request = Repairable();
            request.LifecycleContextMatches = false;
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.ContextMismatch);

            request = Repairable();
            request.HasLifecycle = false;
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.MissingLifecycle);

            SummonTurnEnrollmentRequest enrollment = EnrollmentReady();
            enrollment.LiveSummonCount = 2;
            AssertEnrollmentNo(enrollment,
                SummonTurnEnrollmentDisposition.AmbiguousCounts);
        }

        internal static void DeferredEnrollmentStagesHoldCasterEnd()
        {
            SummonTurnEnrollmentRequest request = EnrollmentReady();
            request.InvocationSealed = false;
            AssertEnrollmentHold(request, SummonTurnEnrollmentDisposition
                .AwaitInvocationCompletion);

            request = EnrollmentReady();
            request.SuccessfulSummonCount = 0;
            request.LiveSummonCount = 0;
            request.CombatEnrolledCount = 0;
            request.TurnOrderMemberCount = 0;
            request.InitiativePreparedCount = 0;
            AssertEnrollmentHold(request, SummonTurnEnrollmentDisposition
                .AwaitSummonSpawn);

            request = EnrollmentReady();
            request.LiveSummonCount = 0;
            AssertEnrollmentHold(request, SummonTurnEnrollmentDisposition
                .AwaitWorldRegistration);

            request = EnrollmentReady();
            request.CombatEnrolledCount = 0;
            AssertEnrollmentHold(request, SummonTurnEnrollmentDisposition
                .AwaitCombatEnrollment);

            request = EnrollmentReady();
            request.TurnOrderMemberCount = 0;
            AssertEnrollmentHold(request, SummonTurnEnrollmentDisposition
                .AwaitTurnOrderEnrollment);

            request = EnrollmentReady();
            request.InitiativePreparedCount = 0;
            AssertEnrollmentHold(request, SummonTurnEnrollmentDisposition
                .AwaitInitiativePreparation);
        }

        internal static void EnrollmentTimeoutFailsOpen()
        {
            SummonTurnEnrollmentRequest request = EnrollmentReady();
            request.InitiativePreparedCount = 0;
            request.HoldAttemptCount = request.MaxHoldAttempts;
            AssertEnrollmentNo(request,
                SummonTurnEnrollmentDisposition.TimedOut);
        }

        internal static void PartialNativeStateIsNormalizedOnlyAsNeeded()
        {
            SummonSameTurnActivationRequest request = Repairable();
            request.HasAppearanceLock = false;
            SummonSameTurnActivationDecision graceOnly =
                SummonSameTurnActivationPolicy.Evaluate(request);
            Assertions.True(graceOnly.ShouldRepair &&
                !graceOnly.RemoveAppearanceLock &&
                graceOnly.RemoveLifecycleGrace,
                "A compatible prior appearance correction was not preserved.");

            request = Repairable();
            request.ObservedLifecycleSeconds =
                request.ExpectedLifecycleSeconds;
            SummonSameTurnActivationDecision appearanceOnly =
                SummonSameTurnActivationPolicy.Evaluate(request);
            Assertions.True(appearanceOnly.ShouldRepair &&
                appearanceOnly.RemoveAppearanceLock &&
                !appearanceOnly.RemoveLifecycleGrace,
                "A compatible prior duration correction was not preserved.");
        }

        internal static void RuntimeSourceUsesExactNativeBoundary()
        {
            string root = Environment.CurrentDirectory;
            string runtime = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "SummonSameTurnActivationRuntime.cs"));
            foreach (string token in new[] {
                "RuleSummonUnit",
                "SourceAbilityContext.Ability",
                "ability.Spellbook != null",
                "AbilityType.Spell",
                "SpellDescriptor.Summoning",
                "ReferenceEquals(ability.Caster, caster.Descriptor)",
                "ReferenceEquals(ability.Caster.Unit, caster)",
                "ReferenceEquals(rule.Context.MaybeCaster, caster)",
                "ReferenceEquals(turn.Unit, caster)",
                "SummonAcceleratedInvocationRuntime",
                "UnitUseAbility",
                "RuleCastSpell",
                "[ThreadStatic]",
                "ReferenceEquals(entry.Ability, ability)",
                "AcceleratedCommandCorrelated",
                "SceneEntitiesState",
                "SummonAcceleratedInvocationRuntime.Clear()",
                "ability.RequireFullRoundAction",
                "ability.Blueprint.IsFullRoundAction",
                "SummonedUnitAppearBuff",
                "SummonedUnitBuff",
                "ReferenceEquals(lifecycle.Context",
                "ReferenceEquals(appearance.Context",
                ".SourceAbilityContext, sourceAbilityContext)",
                "ReferenceEquals(lifecycle.Context.MaybeCaster, caster)",
                "lifecycle.TimeLeft.TotalSeconds",
                "Lifecycle.EndTime",
                "TimeSpan.FromSeconds",
                "Buffs.UpdateNextEvent()",
                "Buffs.RemoveFact",
                "SummonCurrentTurnEnrollmentRuntime",
                "ArmInvocation",
                "SealInvocation",
                "enrollment-register=duplicate-unit",
                "HandleUnitRollsInitiative",
                "CombatState.Prepared",
                "controller.SortedUnits",
                "UnitCombatJoinController",
                "JoinMissingSummons",
                "summon.JoinCombat()",
                "window.JoinAttempted",
                "AllowTurnTick",
                "MaxHoldAttempts",
                "ReferenceEquals(window.CasterTurn, turn)",
                "HarmonyPatch(typeof(TurnController), \"Tick\"",
                "HarmonyPatch(typeof(RuleSummonUnit)" })
                Assertions.True(runtime.Contains(token),
                    "The exact native summon repair seam is missing: " +
                    token);
            foreach (string forbidden in new[] {
                "ForceToEnd", "HandleUnitJoinCombat", ".Initiative =",
                ".Cooldown.", ".Commands.Run(", "AcadamaeGraduate",
                "CallOfTheWild" })
                Assertions.False(runtime.Contains(forbidden),
                    "The summon repair owns forbidden turn/action or optional-" +
                    "mod machinery: " + forbidden);

            string scenario = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "SummonSameTurnActivationScenario.cs"));
            foreach (string token in new[] {
                "enrollment-turn-tick=NativeReady",
                "enrollment-turn-tick=TimedOut",
                "enrollment-native-join=joined:True",
                "enrollment-native-join=failed-open",
                "CountOccurrences" })
                Assertions.True(scenario.Contains(token),
                    "Guarded acceptance does not discriminate native-ready " +
                    "enrollment from timeout: " + token);
        }

        private static SummonSameTurnActivationRequest Repairable()
        {
            return new SummonSameTurnActivationRequest
            {
                InCombat = true,
                TurnBased = true,
                GenuineSummonRule = true,
                SummoningSpell = true,
                HasLiveSummon = true,
                CasterMatchesInvocation = true,
                CasterOwnsCurrentTurn = true,
                AcceleratedCommandCorrelated = false,
                ActualRequiresFullRound = false,
                BlueprintRequiresFullRound = true,
                SummonAlreadyActed = false,
                HasLifecycle = true,
                LifecycleContextMatches = true,
                HasAppearanceLock = true,
                AppearanceContextMatches = true,
                ExpectedLifecycleSeconds = 120d,
                ObservedLifecycleSeconds = 126d
            };
        }

        private static SummonTurnEnrollmentRequest EnrollmentReady()
        {
            return new SummonTurnEnrollmentRequest
            {
                InCombat = true,
                TurnBased = true,
                GenuineSummon = true,
                CreatedDuringCasterTurn = true,
                SameCombatController = true,
                SameRound = true,
                CasterTurnStillCurrent = true,
                InvocationSealed = true,
                SuccessfulSummonCount = 1,
                LiveSummonCount = 1,
                CombatEnrolledCount = 1,
                TurnOrderMemberCount = 1,
                InitiativePreparedCount = 1,
                AlreadyActedCount = 0,
                HoldAttemptCount = 0,
                MaxHoldAttempts = 240
            };
        }

        private static void Normalize(
            SummonSameTurnActivationRequest request)
        {
            request.HasAppearanceLock = false;
            request.ObservedLifecycleSeconds =
                request.ExpectedLifecycleSeconds;
        }

        private static void AssertNoRepair(
            SummonSameTurnActivationRequest request,
            SummonSameTurnActivationDisposition disposition)
        {
            SummonSameTurnActivationDecision decision =
                SummonSameTurnActivationPolicy.Evaluate(request);
            Assertions.False(decision.ShouldRepair ||
                    decision.RemoveAppearanceLock ||
                    decision.RemoveLifecycleGrace,
                "A no-intervention boundary requested a summon repair.");
            Assertions.Equal(disposition, decision.Disposition,
                "The fail-closed summon decision was not diagnostic.");
        }

        private static void AssertEnrollmentHold(
            SummonTurnEnrollmentRequest request,
            SummonTurnEnrollmentDisposition disposition)
        {
            SummonTurnEnrollmentDecision decision =
                SummonTurnEnrollmentPolicy.Evaluate(request);
            Assertions.True(decision.HoldCasterEnd,
                "A missing native summon enrollment stage did not hold the " +
                "already-requested caster end transition.");
            Assertions.Equal(disposition, decision.Disposition,
                "The summon enrollment wait stage was not diagnostic.");
        }

        private static void AssertEnrollmentNo(
            SummonTurnEnrollmentRequest request,
            SummonTurnEnrollmentDisposition disposition)
        {
            SummonTurnEnrollmentDecision decision =
                SummonTurnEnrollmentPolicy.Evaluate(request);
            Assertions.False(decision.HoldCasterEnd,
                "A native/no-intervention enrollment boundary held the " +
                "caster end transition.");
            Assertions.Equal(disposition, decision.Disposition,
                "The summon enrollment no-op was not diagnostic.");
        }
    }
}
