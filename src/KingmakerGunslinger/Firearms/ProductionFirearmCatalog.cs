namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Canonical early-firearm catalog derived from the authoritative local table.
    /// </summary>
    internal static class ProductionFirearmCatalog
    {
        internal static ProductionFirearmWeaponSpec CreatePistol()
        {
            return new ProductionFirearmWeaponSpec(
                "pistol",
                "Pistol",
                FirearmDefinitions.CreateEarlyPistol(),
                1,
                8,
                4,
                false,
                1000,
                4f,
                true);
        }

        internal static ProductionFirearmWeaponSpec CreateMusket()
        {
            return new ProductionFirearmWeaponSpec(
                "musket",
                "Musket",
                FirearmDefinitions.CreateEarlyMusket(),
                1,
                12,
                4,
                true,
                1500,
                9f,
                true);
        }

        internal static ProductionFirearmWeaponSpec CreateBlunderbuss()
        {
            return new ProductionFirearmWeaponSpec(
                "blunderbuss",
                "Blunderbuss",
                FirearmDefinitions.CreateEarlyBlunderbuss(),
                1,
                8,
                2,
                true,
                2000,
                8f,
                false);
        }

        internal static ProductionFirearmWeaponSpec CreateAdvancedRifle()
        {
            return new ProductionFirearmWeaponSpec("advanced-rifle", "Advanced Rifle",
                FirearmDefinitions.CreateAdvancedRifle(), 1, 10, 4, true, 5000, 12f, true);
        }

        internal static ProductionFirearmWeaponSpec CreateAdvancedRevolver()
        {
            return new ProductionFirearmWeaponSpec("advanced-revolver", "Advanced Revolver",
                FirearmDefinitions.CreateAdvancedRevolver(), 1, 8, 4, false, 4000, 4f, true);
        }
    }
}
