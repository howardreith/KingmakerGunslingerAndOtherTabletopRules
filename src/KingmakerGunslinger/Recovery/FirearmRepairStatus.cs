namespace KingmakerGunslinger.Recovery
{
    internal enum FirearmRepairStatus
    {
        Repaired = 1,
        NotBroken = 2,
        // Retained at its historical numeric value for diagnostic compatibility.
        // Unified repair no longer emits this status; a successful repair
        // preserves every surviving loaded round in the exact firearm state.
        Loaded = 3,
        // Retained at the numeric value previously used for an insufficient
        // consumable repair-kit count; the reusable-tool rejection keeps it.
        InsufficientRepairKit = 4,
        // A Wrecked firearm is no longer a field-repair target: only a
        // completed full rest restores it (mission Z-FIREARM-MAINTENANCE).
        WreckedRequiresRest = 5
    }
}
