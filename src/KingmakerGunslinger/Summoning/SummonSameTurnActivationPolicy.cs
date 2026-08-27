using System;

namespace KingmakerGunslinger.Summoning
{
    internal enum SummonSameTurnActivationDisposition
    {
        Repair,
        NotInCombat,
        RealTimeWithPause,
        NotGenuineSummon,
        NotSummoningSpell,
        MissingLiveSummon,
        CasterMismatch,
        OutsideCasterTurn,
        NativeFullRoundInvocation,
        NativeAlreadyImmediate,
        AlreadyActed,
        MissingLifecycle,
        ContextMismatch,
        AmbiguousLifecycleDuration,
        AlreadyEligible
    }

    internal sealed class SummonSameTurnActivationRequest
    {
        internal bool InCombat { get; set; }
        internal bool TurnBased { get; set; }
        internal bool GenuineSummonRule { get; set; }
        internal bool SummoningSpell { get; set; }
        internal bool HasLiveSummon { get; set; }
        internal bool CasterMatchesInvocation { get; set; }
        internal bool CasterOwnsCurrentTurn { get; set; }
        internal bool AcceleratedCommandCorrelated { get; set; }
        internal bool ActualRequiresFullRound { get; set; }
        internal bool BlueprintRequiresFullRound { get; set; }
        internal bool SummonAlreadyActed { get; set; }
        internal bool HasLifecycle { get; set; }
        internal bool LifecycleContextMatches { get; set; }
        internal bool HasAppearanceLock { get; set; }
        internal bool AppearanceContextMatches { get; set; }
        internal double ExpectedLifecycleSeconds { get; set; }
        internal double ObservedLifecycleSeconds { get; set; }
    }

    internal sealed class SummonSameTurnActivationDecision
    {
        internal SummonSameTurnActivationDecision(
            SummonSameTurnActivationDisposition disposition,
            bool removeAppearanceLock, bool removeLifecycleGrace)
        {
            Disposition = disposition;
            RemoveAppearanceLock = removeAppearanceLock;
            RemoveLifecycleGrace = removeLifecycleGrace;
        }

        internal SummonSameTurnActivationDisposition Disposition
        { get; private set; }

        internal bool RemoveAppearanceLock { get; private set; }

        internal bool RemoveLifecycleGrace { get; private set; }

        internal bool ShouldRepair
        { get { return Disposition == SummonSameTurnActivationDisposition.Repair; } }
    }

    internal enum SummonTurnEnrollmentDisposition
    {
        NativeReady,
        AwaitInvocationCompletion,
        AwaitSummonSpawn,
        AwaitWorldRegistration,
        AwaitCombatEnrollment,
        AwaitTurnOrderEnrollment,
        AwaitInitiativePreparation,
        NotInCombat,
        RealTimeWithPause,
        NotGenuineSummon,
        OutsideCasterTurn,
        StaleCombatController,
        StaleRound,
        CasterTurnAdvanced,
        AlreadyActed,
        TimedOut,
        AmbiguousCounts
    }

    internal sealed class SummonTurnEnrollmentRequest
    {
        internal bool InCombat { get; set; }
        internal bool TurnBased { get; set; }
        internal bool GenuineSummon { get; set; }
        internal bool CreatedDuringCasterTurn { get; set; }
        internal bool SameCombatController { get; set; }
        internal bool SameRound { get; set; }
        internal bool CasterTurnStillCurrent { get; set; }
        internal bool InvocationSealed { get; set; }
        internal int SuccessfulSummonCount { get; set; }
        internal int LiveSummonCount { get; set; }
        internal int CombatEnrolledCount { get; set; }
        internal int TurnOrderMemberCount { get; set; }
        internal int InitiativePreparedCount { get; set; }
        internal int AlreadyActedCount { get; set; }
        internal int HoldAttemptCount { get; set; }
        internal int MaxHoldAttempts { get; set; }
    }

    internal sealed class SummonTurnEnrollmentDecision
    {
        internal SummonTurnEnrollmentDecision(
            SummonTurnEnrollmentDisposition disposition,
            bool holdCasterEnd)
        {
            Disposition = disposition;
            HoldCasterEnd = holdCasterEnd;
        }

        internal SummonTurnEnrollmentDisposition Disposition
        { get; private set; }

        internal bool HoldCasterEnd { get; private set; }

        internal bool NativeReady
        { get { return Disposition == SummonTurnEnrollmentDisposition.NativeReady; } }
    }

    /// <summary>
    /// Decides whether the caster's native turn processing should wait briefly
    /// for deferred world registration, the exact summon-scoped native combat
    /// join, and Owlcat's initiative controller. The policy never grants an
    /// action or edits initiative; a ready result means native scheduling has
    /// enough state to choose every summon normally.
    /// </summary>
    internal static class SummonTurnEnrollmentPolicy
    {
        internal static SummonTurnEnrollmentDecision Evaluate(
            SummonTurnEnrollmentRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (!request.InCombat)
                return No(SummonTurnEnrollmentDisposition.NotInCombat);
            if (!request.TurnBased)
                return No(SummonTurnEnrollmentDisposition.RealTimeWithPause);
            if (!request.GenuineSummon)
                return No(SummonTurnEnrollmentDisposition.NotGenuineSummon);
            if (!request.CreatedDuringCasterTurn)
                return No(SummonTurnEnrollmentDisposition.OutsideCasterTurn);
            if (!request.SameCombatController)
                return No(SummonTurnEnrollmentDisposition
                    .StaleCombatController);
            if (!request.SameRound)
                return No(SummonTurnEnrollmentDisposition.StaleRound);
            if (!request.CasterTurnStillCurrent)
                return No(SummonTurnEnrollmentDisposition
                    .CasterTurnAdvanced);
            if (request.AlreadyActedCount > 0)
                return No(SummonTurnEnrollmentDisposition.AlreadyActed);
            if (!CountsAreCoherent(request))
                return No(SummonTurnEnrollmentDisposition.AmbiguousCounts);
            if (request.HoldAttemptCount >= request.MaxHoldAttempts)
                return No(SummonTurnEnrollmentDisposition.TimedOut);
            if (!request.InvocationSealed)
                return Hold(SummonTurnEnrollmentDisposition
                    .AwaitInvocationCompletion);
            if (request.SuccessfulSummonCount == 0)
                return Hold(SummonTurnEnrollmentDisposition
                    .AwaitSummonSpawn);
            if (request.LiveSummonCount < request.SuccessfulSummonCount)
                return Hold(SummonTurnEnrollmentDisposition
                    .AwaitWorldRegistration);
            if (request.CombatEnrolledCount < request.SuccessfulSummonCount)
                return Hold(SummonTurnEnrollmentDisposition
                    .AwaitCombatEnrollment);
            if (request.TurnOrderMemberCount < request.SuccessfulSummonCount)
                return Hold(SummonTurnEnrollmentDisposition
                    .AwaitTurnOrderEnrollment);
            if (request.InitiativePreparedCount <
                request.SuccessfulSummonCount)
                return Hold(SummonTurnEnrollmentDisposition
                    .AwaitInitiativePreparation);
            return No(SummonTurnEnrollmentDisposition.NativeReady);
        }

        private static bool CountsAreCoherent(
            SummonTurnEnrollmentRequest request)
        {
            int total = request.SuccessfulSummonCount;
            return total >= 0 && request.MaxHoldAttempts > 0 &&
                request.HoldAttemptCount >= 0 &&
                IsCount(request.LiveSummonCount, total) &&
                IsCount(request.CombatEnrolledCount, total) &&
                IsCount(request.TurnOrderMemberCount, total) &&
                IsCount(request.InitiativePreparedCount, total) &&
                IsCount(request.AlreadyActedCount, total);
        }

        private static bool IsCount(int value, int maximum)
        { return value >= 0 && value <= maximum; }

        private static SummonTurnEnrollmentDecision Hold(
            SummonTurnEnrollmentDisposition disposition)
        { return new SummonTurnEnrollmentDecision(disposition, true); }

        private static SummonTurnEnrollmentDecision No(
            SummonTurnEnrollmentDisposition disposition)
        { return new SummonTurnEnrollmentDecision(disposition, false); }
    }

    /// <summary>
    /// Stateless fail-closed policy for correcting Owlcat's full-round summon
    /// grace when the exact live spell invocation is Standard or Swift. State
    /// normalization is the idempotence key: after the canonical appearance
    /// lock and lifecycle grace are gone, a duplicate callback is a no-op.
    /// </summary>
    internal static class SummonSameTurnActivationPolicy
    {
        internal const double NativeGraceSeconds = 6d;
        private const double DurationToleranceSeconds = 0.5d;

        internal static SummonSameTurnActivationDecision Evaluate(
            SummonSameTurnActivationRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (!request.InCombat)
                return No(SummonSameTurnActivationDisposition.NotInCombat);
            if (!request.TurnBased)
                return No(SummonSameTurnActivationDisposition.RealTimeWithPause);
            if (!request.GenuineSummonRule)
                return No(SummonSameTurnActivationDisposition.NotGenuineSummon);
            if (!request.SummoningSpell)
                return No(SummonSameTurnActivationDisposition.NotSummoningSpell);
            if (!request.HasLiveSummon)
                return No(SummonSameTurnActivationDisposition.MissingLiveSummon);
            if (!request.CasterMatchesInvocation)
                return No(SummonSameTurnActivationDisposition.CasterMismatch);
            if (!request.CasterOwnsCurrentTurn)
                return No(SummonSameTurnActivationDisposition.OutsideCasterTurn);
            if (!request.BlueprintRequiresFullRound)
                return No(SummonSameTurnActivationDisposition.NativeAlreadyImmediate);
            if (request.SummonAlreadyActed)
                return No(SummonSameTurnActivationDisposition.AlreadyActed);
            if (!request.HasLifecycle)
                return No(SummonSameTurnActivationDisposition.MissingLifecycle);
            if (!request.LifecycleContextMatches ||
                request.HasAppearanceLock &&
                    !request.AppearanceContextMatches)
                return No(SummonSameTurnActivationDisposition.ContextMismatch);

            bool lifecycleHasGrace = IsNear(
                request.ObservedLifecycleSeconds,
                request.ExpectedLifecycleSeconds + NativeGraceSeconds);
            bool lifecycleIsNative = IsNear(
                request.ObservedLifecycleSeconds,
                request.ExpectedLifecycleSeconds);
            if (!lifecycleHasGrace && !lifecycleIsNative)
                return No(SummonSameTurnActivationDisposition
                    .AmbiguousLifecycleDuration);

            bool removeAppearance = request.HasAppearanceLock;
            bool removeGrace = lifecycleHasGrace;
            if (!removeAppearance && !removeGrace)
                return No(SummonSameTurnActivationDisposition.AlreadyEligible);
            if (request.ActualRequiresFullRound &&
                !request.AcceleratedCommandCorrelated)
                return No(SummonSameTurnActivationDisposition.NativeFullRoundInvocation);
            return new SummonSameTurnActivationDecision(
                SummonSameTurnActivationDisposition.Repair,
                removeAppearance, removeGrace);
        }

        private static bool IsNear(double actual, double expected)
        {
            return !double.IsNaN(actual) && !double.IsInfinity(actual) &&
                !double.IsNaN(expected) && !double.IsInfinity(expected) &&
                expected >= 0d &&
                Math.Abs(actual - expected) <= DurationToleranceSeconds;
        }

        private static SummonSameTurnActivationDecision No(
            SummonSameTurnActivationDisposition disposition)
        {
            return new SummonSameTurnActivationDecision(disposition,
                false, false);
        }
    }
}
