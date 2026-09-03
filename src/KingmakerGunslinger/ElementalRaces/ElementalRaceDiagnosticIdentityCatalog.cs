namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>
    /// Reserved identity used only by the guarded development race probe.
    /// It is never registered during ordinary bootstrap and never published
    /// to character creation. The probe registers and removes it in one
    /// request-local transaction.
    /// </summary>
    internal static class ElementalRaceDiagnosticIdentityCatalog
    {
        internal const string Symbol =
            "KMG.ElementalRaces.Diagnostics.ProbeRace";

        internal const string Guid =
            "57005fca40ab4775ae2fea5613214054";
    }
}
