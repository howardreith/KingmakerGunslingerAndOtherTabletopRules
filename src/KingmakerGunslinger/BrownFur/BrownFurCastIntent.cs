using System;

namespace KingmakerGunslinger.BrownFur
{
    internal sealed class BrownFurCastIntent
    {
        internal BrownFurCastIntent(string transactionIdentity, string caster,
            string canonicalSpell, string selectedVariant, string sourceSpellbook,
            string target, bool powerfulChangeRequested,
            BrownFurAbilityScore selectedAbilityScore,
            bool shareTransmutationRequested, bool supremacyApplicable,
            int expectedReservoirCost, string targetingAdapter,
            string bonusAdapter, string durationAdapter)
        {
            if (string.IsNullOrWhiteSpace(transactionIdentity))
                throw new ArgumentException("A transaction identity is required.",
                    "transactionIdentity");
            TransactionIdentity = transactionIdentity;
            Caster = caster ?? string.Empty;
            CanonicalSpell = canonicalSpell ?? string.Empty;
            SelectedVariant = selectedVariant ?? string.Empty;
            SourceSpellbook = sourceSpellbook ?? string.Empty;
            Target = target ?? string.Empty;
            PowerfulChangeRequested = powerfulChangeRequested;
            SelectedAbilityScore = selectedAbilityScore;
            ShareTransmutationRequested = shareTransmutationRequested;
            TransmutationSupremacyApplicable = supremacyApplicable;
            ExpectedReservoirCost = expectedReservoirCost;
            TargetingAdapter = targetingAdapter ?? string.Empty;
            BonusAdapter = bonusAdapter ?? string.Empty;
            DurationAdapter = durationAdapter ?? string.Empty;
        }

        internal string TransactionIdentity { get; private set; }
        internal string Caster { get; private set; }
        internal string CanonicalSpell { get; private set; }
        internal string SelectedVariant { get; private set; }
        internal string SourceSpellbook { get; private set; }
        internal string Target { get; private set; }
        internal bool PowerfulChangeRequested { get; private set; }
        internal BrownFurAbilityScore SelectedAbilityScore { get; private set; }
        internal bool ShareTransmutationRequested { get; private set; }
        internal bool TransmutationSupremacyApplicable { get; private set; }
        internal int ExpectedReservoirCost { get; private set; }
        internal string TargetingAdapter { get; private set; }
        internal string BonusAdapter { get; private set; }
        internal string DurationAdapter { get; private set; }
    }
}
