namespace KingmakerGunslinger.Spells.ProtectionFromAlignment
{
    internal struct ProtectionControlImmunityRequest
    {
        internal ProtectionControlImmunityRequest(ProtectionAlignment protectedAgainst,
            string sourceAbilityGuid, string terminalBuffGuid, bool sourceClassified,
            ProtectionAlignment sourceAlignment)
        {
            ProtectedAgainst = protectedAgainst;
            SourceAbilityGuid = sourceAbilityGuid;
            TerminalBuffGuid = terminalBuffGuid;
            SourceClassified = sourceClassified;
            SourceAlignment = sourceAlignment;
        }

        internal ProtectionAlignment ProtectedAgainst { get; private set; }
        internal string SourceAbilityGuid { get; private set; }
        internal string TerminalBuffGuid { get; private set; }
        internal bool SourceClassified { get; private set; }
        internal ProtectionAlignment SourceAlignment { get; private set; }
    }

    internal struct ProtectionControlImmunityDecision
    {
        internal ProtectionControlImmunityDecision(bool qualifyingControl,
            bool matchingAlignment, bool usedTrustedAlignment, string reason)
        {
            QualifyingControl = qualifyingControl;
            MatchingAlignment = matchingAlignment;
            UsedTrustedAlignment = usedTrustedAlignment;
            Reason = reason;
        }

        internal bool QualifyingControl { get; private set; }
        internal bool MatchingAlignment { get; private set; }
        internal bool UsedTrustedAlignment { get; private set; }
        internal bool ShouldBlock { get { return QualifyingControl && MatchingAlignment; } }
        internal string Reason { get; private set; }
    }

    internal static class ProtectionControlImmunityPolicy
    {
        internal static ProtectionControlImmunityDecision Evaluate(
            MentalControlCatalog catalog, ProtectionControlImmunityRequest request)
        {
            MentalControlCatalogEntry ability = null;
            MentalControlCatalogEntry buff = null;
            bool abilityMatched = catalog != null && catalog.TryGetAbility(
                request.SourceAbilityGuid, out ability);
            bool buffMatched = catalog != null && catalog.TryGetBuff(
                request.TerminalBuffGuid, out buff);
            if (!abilityMatched) ability = null;
            if (!buffMatched) buff = null;
            if (!abilityMatched && !buffMatched)
                return new ProtectionControlImmunityDecision(false, false, false,
                    "unregistered-effect");

            if (request.SourceClassified)
            {
                bool matches = (request.SourceAlignment & request.ProtectedAgainst) != 0;
                return new ProtectionControlImmunityDecision(true, matches, false,
                    matches ? "registered-matching-source" :
                        "registered-nonmatching-source");
            }

            ProtectionAlignment trusted = ProtectionAlignment.None;
            if (ability != null && ability.TrustedSourceAlignment.HasValue)
                trusted |= ability.TrustedSourceAlignment.Value;
            if (buff != null && buff.TrustedSourceAlignment.HasValue)
                trusted |= buff.TrustedSourceAlignment.Value;
            bool trustedMatches = (trusted & request.ProtectedAgainst) != 0;
            return new ProtectionControlImmunityDecision(true, trustedMatches,
                trustedMatches, trustedMatches ? "registered-trusted-source" :
                    "registered-source-unresolved-fail-open");
        }
    }
}
