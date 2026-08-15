using System.Collections.Generic;

namespace KingmakerGunslinger.BrownFur
{
    internal sealed class CotwArcanistContractCandidate
    {
        internal bool CotwDetected { get; set; }
        internal bool CotwActive { get; set; }
        internal bool AssemblyIdentityResolved { get; set; }
        internal bool ArcanistClassResolved { get; set; }
        internal bool ArcanistProgressionResolved { get; set; }
        internal bool CastingSpellbookResolved { get; set; }
        internal bool MemorizationSpellbookResolved { get; set; }
        internal bool ReservoirResolved { get; set; }
        internal bool ExploitSelectionResolved { get; set; }
        internal bool MagicalSupremacyResolved { get; set; }
        internal bool SharedSpellsContractResolved { get; set; }
        internal bool ArchetypeArrayResolved { get; set; }
        internal bool TransmutationInventoryResolved { get; set; }
        internal IEnumerable<int> ExploitBearingLevels { get; set; }
    }
}
