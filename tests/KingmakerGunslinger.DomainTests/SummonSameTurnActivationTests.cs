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
        }

        internal static void RealTimeWithPauseIsNative()
        {
            SummonSameTurnActivationRequest request = Repairable();
            request.TurnBased = false;
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.RealTimeWithPause);
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
        }

        internal static void OutsideCasterTurnIsNative()
        {
            SummonSameTurnActivationRequest request = Repairable();
            request.CasterOwnsCurrentTurn = false;
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.OutsideCasterTurn);
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
        }

        internal static void AlreadyActedSummonIsNotReactivated()
        {
            SummonSameTurnActivationRequest request = Repairable();
            request.SummonAlreadyActed = true;
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.AlreadyActed);
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
        }

        internal static void FollowingRoundHasNoStaleActivation()
        {
            SummonSameTurnActivationRequest request = Repairable();
            Normalize(request);
            request.CasterOwnsCurrentTurn = false;
            AssertNoRepair(request,
                SummonSameTurnActivationDisposition.OutsideCasterTurn);
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
                "HarmonyPatch(typeof(RuleSummonUnit)" })
                Assertions.True(runtime.Contains(token),
                    "The exact native summon repair seam is missing: " +
                    token);
            foreach (string forbidden in new[] {
                "ForceToEnd", "JoinCombat", ".Initiative =",
                ".Cooldown.", ".Commands.Run(", "AcadamaeGraduate",
                "CallOfTheWild" })
                Assertions.False(runtime.Contains(forbidden),
                    "The summon repair owns forbidden turn/action or optional-" +
                    "mod machinery: " + forbidden);
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
    }
}
