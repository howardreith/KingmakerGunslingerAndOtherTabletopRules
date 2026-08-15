namespace KingmakerGunslinger.BrownFur
{
    internal static class CotwArcanistContractPolicy
    {
        internal static CotwArcanistContractDecision Evaluate(
            CotwArcanistContractCandidate candidate)
        {
            if (candidate == null || !candidate.CotwDetected)
                return Unavailable("Call of the Wild was not detected");
            if (!candidate.CotwActive) return Blocked("cotw-active");
            if (!candidate.AssemblyIdentityResolved) return Blocked("assembly-identity");
            if (!candidate.ArcanistClassResolved) return Blocked("arcanist-class");
            if (!candidate.ArcanistProgressionResolved) return Blocked("arcanist-progression");
            if (!candidate.CastingSpellbookResolved) return Blocked("casting-spellbook");
            if (!candidate.MemorizationSpellbookResolved) return Blocked("memorization-spellbook");
            if (!candidate.ReservoirResolved) return Blocked("arcane-reservoir");
            if (!candidate.ExploitSelectionResolved) return Blocked("exploit-selection");
            if (!candidate.MagicalSupremacyResolved) return Blocked("magical-supremacy");
            if (!candidate.SharedSpellsContractResolved) return Blocked("shared-spells-signature");
            if (!candidate.ArchetypeArrayResolved) return Blocked("archetype-array");
            if (!candidate.TransmutationInventoryResolved) return Blocked("transmutation-inventory");

            CotwProgressionDecision progression = CotwProgressionPolicy.Resolve(
                candidate.ExploitBearingLevels);
            if (!progression.Compatible)
                return new CotwArcanistContractDecision(
                    CotwContractAvailability.Incompatible, progression,
                    "exploit-progression:" + progression.Reason);
            return new CotwArcanistContractDecision(
                CotwContractAvailability.Compatible, progression, string.Empty);
        }

        private static CotwArcanistContractDecision Unavailable(string reason)
        {
            return new CotwArcanistContractDecision(
                CotwContractAvailability.Unavailable,
                CotwProgressionDecision.Reject(reason), reason);
        }

        private static CotwArcanistContractDecision Blocked(string check)
        {
            return new CotwArcanistContractDecision(
                CotwContractAvailability.Incompatible,
                CotwProgressionDecision.Reject("contract check failed: " + check),
                check);
        }
    }
}
