namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Canonical immutable firearm definitions shared by blueprint creation,
    /// runtime probes, diagnostics, and dependency-free domain tests.
    /// </summary>
    internal static class FirearmDefinitions
    {
        internal static FirearmDefinition CreateEarlyMusket()
        {
            return new FirearmDefinition(
                FirearmEra.Early,
                FirearmKind.Musket,
                1,
                40,
                2,
                5,
                new ReloadProfile(ReloadActionType.FullRound, true, 1),
                false);
        }
    }
}
