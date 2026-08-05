namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Canonical immutable firearm definitions shared by blueprint creation,
    /// runtime probes, diagnostics, and dependency-free domain tests.
    /// </summary>
    internal static class FirearmDefinitions
    {
        internal static FirearmDefinition CreateEarlyPistol()
        {
            return new FirearmDefinition(
                FirearmEra.Early,
                FirearmKind.Pistol,
                1,
                20,
                1,
                5,
                new ReloadProfile(ReloadActionType.Standard, true, 1),
                false);
        }

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

        internal static FirearmDefinition CreateEarlyBlunderbuss()
        {
            return new FirearmDefinition(
                FirearmEra.Early,
                FirearmKind.Blunderbuss,
                1,
                10,
                2,
                10,
                new ReloadProfile(ReloadActionType.FullRound, true, 1),
                true);
        }

        internal static FirearmDefinition CreateAdvancedRifle()
        {
            return new FirearmDefinition(FirearmEra.Advanced, FirearmKind.Rifle,
                1, 80, 1, 5, new ReloadProfile(ReloadActionType.Move, true, 1), false);
        }

        internal static FirearmDefinition CreateAdvancedRevolver()
        {
            return new FirearmDefinition(FirearmEra.Advanced, FirearmKind.Revolver,
                6, 20, 1, 5, new ReloadProfile(ReloadActionType.Move, true, 6), false);
        }
    }
}
