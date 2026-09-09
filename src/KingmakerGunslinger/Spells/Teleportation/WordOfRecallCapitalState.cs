namespace KingmakerGunslinger.Spells.Teleportation
{
    internal sealed class WordOfRecallCapitalState
    {
        private WordOfRecallCapitalState(bool known, bool established, string destinationId, string diagnostic)
        { Known = known; Established = established; DestinationId = destinationId; Diagnostic = diagnostic; }
        internal bool Known { get; private set; }
        internal bool Established { get; private set; }
        internal string DestinationId { get; private set; }
        internal string Diagnostic { get; private set; }
        internal static readonly WordOfRecallCapitalState Unknown = new WordOfRecallCapitalState(false, false, null, "unknown-capital-state");
        internal static readonly WordOfRecallCapitalState Before = new WordOfRecallCapitalState(true, false,
            WordOfRecallDestinationPolicy.OlegId, "capital-region-unclaimed");
        internal static WordOfRecallCapitalState Resolve(bool capitalRegionClaimed, bool ownedSettlement, string locationId)
        {
            // Capital's native region constructs its prebuilt settlement before the
            // region is claimed. Settlement existence alone is not establishment.
            if (!capitalRegionClaimed) return Before;
            string id = ownedSettlement && locationId == WordOfRecallDestinationPolicy.CapitalId ? locationId : null;
            return new WordOfRecallCapitalState(true, true, id,
                id == null ? "established-capital-invalid-no-fallback" : "established-capital");
        }
    }
}
