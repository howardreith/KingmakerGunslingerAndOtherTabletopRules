namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>How the installed ProperFlanking2 Improved Sunder chain reads.</summary>
    internal enum SunderProviderChain
    {
        /// <summary>Provider not active and none of its identities present.</summary>
        Inactive,
        /// <summary>Provider not active, yet some of its identities or wiring exist.</summary>
        InactiveIdentitiesPresent,
        /// <summary>Provider active with the exact feat, action, toggle, buff and replacement wiring.</summary>
        ActiveExact,
        /// <summary>Provider active, but an identity is missing or wired differently.</summary>
        ActiveMismatch
    }

    internal enum SunderCleanupOutcome
    {
        /// <summary>Exact chain: the claimed live state was observed before and gone after.</summary>
        Proven,
        /// <summary>Provider genuinely inactive: feat and action only. A qualified,
        /// native-only reading; it does not observe the replacement buff.</summary>
        ProvenFactLevelProviderInactive,
        /// <summary>The chain identities could not be verified, so absence proves nothing.</summary>
        ChainUnverified,
        /// <summary>The claimed pre-removal state was not observed.</summary>
        PreconditionMissing,
        /// <summary>Something claimed removed was still present.</summary>
        NotRemoved
    }

    /// <summary>One unit's separately read Sunder state. A present toggle is
    /// not a running one, and an unresolved blueprint is not an absent fact.</summary>
    internal sealed class SunderUnitState
    {
        internal bool Feat, Action, TogglePresent, ToggleOn, ToggleRunning;
        internal int BuffPresent, BuffActive;

        internal bool LiveChain
        { get { return Feat && Action && TogglePresent && ToggleOn && ToggleRunning && BuffActive >= 1; } }

        internal bool Gone
        { get { return !Feat && !Action && !TogglePresent && BuffPresent == 0; } }
    }

    /// <summary>
    /// Verdicts for the menu-order/Sunder observer, following the R1 chain
    /// discipline: the provider's activation is read, the exact chain is
    /// verified, and a missing or mismatched identity can never stand in for
    /// a successful removal.
    /// </summary>
    internal static class SunderObservationVerdict
    {
        internal static SunderProviderChain ClassifyProvider(bool providerActive, bool featResolved,
            bool actionResolved, bool actionGrantedByFeat, bool toggleResolved, bool toggleGrantedByFeat,
            bool toggleNamesExact, bool toggleDefaultOn, bool buffResolved, bool toggleAppliesBuff,
            bool replacementComponent)
        {
            if (!providerActive)
                return toggleResolved || buffResolved || toggleGrantedByFeat ?
                    SunderProviderChain.InactiveIdentitiesPresent : SunderProviderChain.Inactive;
            return featResolved && actionResolved && actionGrantedByFeat && toggleResolved && toggleGrantedByFeat &&
                toggleNamesExact && toggleDefaultOn && buffResolved && toggleAppliesBuff && replacementComponent ?
                SunderProviderChain.ActiveExact : SunderProviderChain.ActiveMismatch;
        }

        /// <summary>Whether absence on a fresh summon may be asserted at all:
        /// the native feat and action must resolve, and an active provider's
        /// chain must be exact.</summary>
        internal static bool AbsenceAssertable(SunderProviderChain chain, bool featResolved, bool actionResolved)
        {
            return featResolved && actionResolved &&
                (chain == SunderProviderChain.ActiveExact || chain == SunderProviderChain.Inactive);
        }

        /// <param name="requireRunningToggle">True for a weapon wielder whose
        /// default-on toggle is claimed to run; false where only the feat and
        /// action are claimed (a toggle seen before must still be gone after).</param>
        internal static SunderCleanupOutcome ClassifyCleanup(SunderProviderChain chain, bool requireRunningToggle,
            SunderUnitState before, SunderUnitState after)
        {
            if (chain != SunderProviderChain.ActiveExact && chain != SunderProviderChain.Inactive)
                return SunderCleanupOutcome.ChainUnverified;
            if (before == null || after == null || !before.Feat || !before.Action)
                return SunderCleanupOutcome.PreconditionMissing;
            if (chain == SunderProviderChain.ActiveExact && requireRunningToggle && !before.LiveChain)
                return SunderCleanupOutcome.PreconditionMissing;
            if (!after.Gone) return SunderCleanupOutcome.NotRemoved;
            return chain == SunderProviderChain.ActiveExact ? SunderCleanupOutcome.Proven :
                SunderCleanupOutcome.ProvenFactLevelProviderInactive;
        }

        internal static bool Passes(SunderCleanupOutcome outcome)
        {
            return outcome == SunderCleanupOutcome.Proven || outcome == SunderCleanupOutcome.ProvenFactLevelProviderInactive;
        }
    }

    internal enum SunderReferenceKind { Fact, Brain, Component }

    /// <summary>One reference from a unit blueprint to an exact Sunder source.</summary>
    internal sealed class SunderReference
    {
        internal SunderReferenceKind Kind;
        /// <summary>Component type full name (Component references only).</summary>
        internal string ComponentType;
        /// <summary>The component's own top-level field holding the reference.</summary>
        internal string Field;
        internal string Path;
        internal string Target;

        internal string Describe() { return Kind.ToString().ToLowerInvariant() + ":" + Path + ":" + Target; }
    }

    internal sealed class SunderCarrierReading
    {
        internal SunderReference[] Carriers = new SunderReference[0];
        internal SunderReference[] Cleanup = new SunderReference[0];
        internal bool IsCarrier { get { return Carriers.Length > 0; } }
    }

    /// <summary>
    /// Separates grants and executable AI from the withdrawal fixer's own
    /// removal lists. Only the exact fixer type's Facts, AiActions and
    /// Resources lists are cleanup; every other fact, brain action, component
    /// or field (including the fixer's other fields) still makes a carrier.
    /// </summary>
    internal static class SunderCarrierClassifier
    {
        internal const string WithdrawalFixerType = "KingmakerGunslinger.Summoning.SummonWithdrawnCapabilityFixer";
        private static readonly string[] RemovalFields = { "Facts", "AiActions", "Resources" };

        internal static bool IsWithdrawalCleanup(SunderReference reference)
        {
            return reference != null && reference.Kind == SunderReferenceKind.Component &&
                string.Equals(reference.ComponentType, WithdrawalFixerType, System.StringComparison.Ordinal) &&
                System.Array.IndexOf(RemovalFields, reference.Field) >= 0;
        }

        internal static SunderCarrierReading Classify(System.Collections.Generic.IEnumerable<SunderReference> references)
        {
            var carriers = new System.Collections.Generic.List<SunderReference>();
            var cleanup = new System.Collections.Generic.List<SunderReference>();
            foreach (SunderReference reference in references ?? new SunderReference[0])
            {
                if (reference == null) continue;
                (IsWithdrawalCleanup(reference) ? cleanup : carriers).Add(reference);
            }
            return new SunderCarrierReading { Carriers = carriers.ToArray(), Cleanup = cleanup.ToArray() };
        }
    }
}
