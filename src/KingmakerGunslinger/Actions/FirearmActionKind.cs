namespace KingmakerGunslinger.Actions
{
    internal enum FirearmActionKind
    {
        Unknown = 0,
        Reload = 1,
        // Retained at its historical numeric value for diagnostic compatibility;
        // the separate Overhaul maintenance action no longer exists.
        Overhaul = 2,
        Repair = 3
    }
}
