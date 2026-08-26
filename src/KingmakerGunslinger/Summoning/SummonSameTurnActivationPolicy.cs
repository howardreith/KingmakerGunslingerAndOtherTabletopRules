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
            if (request.ActualRequiresFullRound)
                return No(SummonSameTurnActivationDisposition.NativeFullRoundInvocation);
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
