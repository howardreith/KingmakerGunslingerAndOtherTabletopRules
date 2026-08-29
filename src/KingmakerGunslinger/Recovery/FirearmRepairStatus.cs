namespace KingmakerGunslinger.Recovery
{
    internal enum FirearmRepairStatus
    {
        Repaired = 1,
        NotBroken = 2,
        // Retained at its historical numeric value for diagnostic compatibility.
        // Ordinary repair no longer emits this status; a successful repair
        // destroys every round in the exact firearm-owned loaded state.
        Loaded = 3,
        InsufficientRepairKit = 4
    }
}
