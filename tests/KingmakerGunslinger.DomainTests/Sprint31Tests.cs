using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void CatalogPistolExact()
        {
            ProductionFirearmWeaponSpec spec = ProductionFirearmCatalog.CreatePistol();
            AssertWeaponSpec(spec, "pistol", "Pistol", FirearmKind.Pistol, 1, 8, 4, false, 1000, 4f, true);
            Assertions.Equal(20, spec.Definition.RangeIncrementFeet, "Pistol range mismatch.");
        }

        private static void CatalogMusketExact()
        {
            ProductionFirearmWeaponSpec spec = ProductionFirearmCatalog.CreateMusket();
            AssertWeaponSpec(spec, "musket", "Musket", FirearmKind.Musket, 1, 12, 4, true, 1500, 9f, true);
            Assertions.Equal(40, spec.Definition.RangeIncrementFeet, "Musket range mismatch.");
        }

        private static void CatalogBlunderbussExact()
        {
            ProductionFirearmWeaponSpec spec = ProductionFirearmCatalog.CreateBlunderbuss();
            AssertWeaponSpec(spec, "blunderbuss", "Blunderbuss", FirearmKind.Blunderbuss, 1, 8, 2, true, 2000, 8f, false);
            Assertions.False(spec.Definition.HasFixedRangeIncrement,
                "Blunderbuss catalog entry invented a numeric range.");
        }

        private static void CatalogFactoriesAreFresh()
        {
            ProductionFirearmWeaponSpec first = ProductionFirearmCatalog.CreatePistol();
            ProductionFirearmWeaponSpec second = ProductionFirearmCatalog.CreatePistol();
            Assertions.False(ReferenceEquals(first, second), "Catalog factory reused a spec instance.");
            Assertions.False(ReferenceEquals(first.Definition, second.Definition),
                "Catalog factory reused a definition instance.");
            Assertions.Equal(first, second, "Fresh canonical specs must compare equal.");
        }

        private static void CatalogSpecialRangeCannotBeFireable()
        {
            ProductionFirearmWeaponSpec blunderbuss = ProductionFirearmCatalog.CreateBlunderbuss();
            Assertions.Throws<ArgumentException>(
                () => new ProductionFirearmWeaponSpec(
                    blunderbuss.Key,
                    blunderbuss.DisplayName,
                    blunderbuss.Definition,
                    blunderbuss.DamageDiceCount,
                    blunderbuss.DamageDieSides,
                    blunderbuss.CriticalMultiplier,
                    blunderbuss.IsTwoHanded,
                    blunderbuss.CostGold,
                    blunderbuss.WeightPounds,
                    true),
                "Special-range content must remain non-fireable before scatter execution exists.");
        }

        private static void CatalogHandednessMismatchRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new ProductionFirearmWeaponSpec(
                    "bad-musket",
                    "Bad Musket",
                    FirearmDefinitions.CreateEarlyMusket(),
                    1,
                    12,
                    4,
                    false,
                    1500,
                    9f,
                    true),
                "A two-handed firearm kind accepted one-handed presentation.");
        }

        private static void CatalogFormattingDeterministic()
        {
            Assertions.Equal(
                "Pistol; damage=1d8; critical=x4; twoHanded=False; cost=1000gp; weight=4lb; playerFireable=True; definition=(Early Pistol; capacity=1; range=20ft; misfire=1-1; misfireBurst=5ft; reload=(Standard; freeHand=True; roundsPerAction=1); scatter=False)",
                ProductionFirearmCatalog.CreatePistol().ToString(),
                "Production firearm formatting changed.");
        }

        private static void AssertWeaponSpec(
            ProductionFirearmWeaponSpec spec,
            string key,
            string name,
            FirearmKind kind,
            int diceCount,
            int dieSides,
            int critical,
            bool twoHanded,
            int cost,
            float weight,
            bool fireable)
        {
            Assertions.Equal(key, spec.Key, "Catalog key mismatch.");
            Assertions.Equal(name, spec.DisplayName, "Catalog display name mismatch.");
            Assertions.Equal(kind, spec.Definition.Kind, "Catalog firearm kind mismatch.");
            Assertions.Equal(diceCount, spec.DamageDiceCount, "Damage dice count mismatch.");
            Assertions.Equal(dieSides, spec.DamageDieSides, "Damage die mismatch.");
            Assertions.Equal(critical, spec.CriticalMultiplier, "Critical multiplier mismatch.");
            Assertions.Equal(twoHanded, spec.IsTwoHanded, "Handedness mismatch.");
            Assertions.Equal(cost, spec.CostGold, "Cost mismatch.");
            Assertions.Equal(weight, spec.WeightPounds, "Weight mismatch.");
            Assertions.Equal(fireable, spec.IsPlayerFireable, "Player-fireable status mismatch.");
        }
    }
}
