using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.CustomWeapons;
using KingmakerGunslinger.EasternWeapons;

namespace KingmakerGunslinger.DomainTests
{
    internal static class EasternWeaponFoundationTests
    {
        internal static void LockedCategoryProfilesAreExact()
        {
            CustomWeaponCategoryDefinition[] categories =
                EasternWeaponCatalog.AllCategories;
            Assertions.Equal(3, categories.Length,
                "Eastern Weapons must contain exactly three categories.");
            Assertions.True(categories.Select(value => value.CategoryValue)
                .SequenceEqual(new[] { 0x004b4d48, 0x004b4d49, 0x004b4d4a }),
                "Stable category values changed.");
            Assertions.True(categories.Select(value => value.Presentation.Acronym)
                .SequenceEqual(new[] { "WK", "KA", "NO" }),
                "Category acronyms changed.");

            CustomWeaponCategoryDefinition wakizashi =
                EasternWeaponCatalog.RequireCategory(EasternWeaponFamily.Wakizashi);
            Assertions.True(wakizashi.BaseCost == 35 && wakizashi.WeightPounds == 2 &&
                wakizashi.DamageDiceCount == 1 && wakizashi.DamageDieSides == 6 &&
                wakizashi.CriticalThreatMinimum == 18 &&
                wakizashi.CriticalMultiplier == 2 && wakizashi.Finessable &&
                wakizashi.Handedness == CustomWeaponHandedness.Light &&
                wakizashi.Proficiency == CustomWeaponProficiencyPolicy.Exotic &&
                wakizashi.FighterGroups ==
                    CustomWeaponFighterGroupPolicy.LightBlades &&
                wakizashi.DamageForms == (CustomWeaponDamageForm.Piercing |
                    CustomWeaponDamageForm.Slashing),
                "Wakizashi profile changed.");

            CustomWeaponCategoryDefinition katana =
                EasternWeaponCatalog.RequireCategory(EasternWeaponFamily.Katana);
            Assertions.True(katana.BaseCost == 50 && katana.WeightPounds == 6 &&
                katana.DamageDiceCount == 1 && katana.DamageDieSides == 8 &&
                katana.CriticalThreatMinimum == 18 &&
                katana.CriticalMultiplier == 2 && !katana.Finessable &&
                katana.Handedness == CustomWeaponHandedness.OneHandedVersatile &&
                katana.Proficiency ==
                    CustomWeaponProficiencyPolicy.KatanaGripDependent &&
                katana.FighterGroups ==
                    CustomWeaponFighterGroupPolicy.HeavyBlades &&
                katana.DamageForms == CustomWeaponDamageForm.Slashing,
                "Katana profile changed.");

            CustomWeaponCategoryDefinition nodachi =
                EasternWeaponCatalog.RequireCategory(EasternWeaponFamily.Nodachi);
            Assertions.True(nodachi.BaseCost == 60 && nodachi.WeightPounds == 8 &&
                nodachi.DamageDiceCount == 1 && nodachi.DamageDieSides == 10 &&
                nodachi.CriticalThreatMinimum == 18 &&
                nodachi.CriticalMultiplier == 2 && !nodachi.Finessable &&
                nodachi.Handedness == CustomWeaponHandedness.TwoHanded &&
                nodachi.Proficiency == CustomWeaponProficiencyPolicy.Martial &&
                nodachi.FighterGroups ==
                    (CustomWeaponFighterGroupPolicy.HeavyBlades |
                        CustomWeaponFighterGroupPolicy.Polearms),
                "Nodachi profile changed.");
            Assertions.True(categories.All(value => !value.Reach && !value.Thrown),
                "Eastern weapons must not be reach or thrown weapons.");
        }

        internal static void GenericCatalogIsExact()
        {
            EasternWeaponGenericSpec[] items = EasternWeaponCatalog.AllGenericItems;
            Assertions.Equal(12, items.Length,
                "Eastern generic catalog must contain twelve items.");
            foreach (EasternWeaponFamily family in Enum.GetValues(
                typeof(EasternWeaponFamily)).Cast<EasternWeaponFamily>())
            {
                EasternWeaponGenericSpec[] path = items.Where(value =>
                    value.Family == family).OrderBy(value => value.Kind).ToArray();
                CustomWeaponCategoryDefinition category =
                    EasternWeaponCatalog.RequireCategory(family);
                Assertions.Equal(4, path.Length,
                    "Each Eastern family must contain four generic items.");
                Assertions.True(path.Select(value => value.Cost).SequenceEqual(
                    new[] { category.BaseCost, category.BaseCost + 300,
                        category.BaseCost * 2, category.BaseCost + 2300 }),
                    "Generic tabletop pricing changed for " + family + ".");
                Assertions.True(!path[0].Masterwork && !path[0].ColdIron &&
                    path[0].Enhancement == 0 && path[1].Masterwork &&
                    !path[1].ColdIron && path[1].Enhancement == 0 &&
                    !path[2].Masterwork && path[2].ColdIron &&
                    path[2].Enhancement == 0 && path[3].Masterwork &&
                    !path[3].ColdIron && path[3].Enhancement == 1,
                    "Generic quality construction changed for " + family + ".");
            }
            Assertions.Equal(12, items.Select(value => value.Symbol).Distinct().Count(),
                "Generic symbols are not unique.");
        }

        internal static void RegistryFailsClosedOnCollisions()
        {
            var registry = new CustomWeaponCategoryRegistry();
            foreach (CustomWeaponCategoryDefinition definition in
                EasternWeaponCatalog.AllCategories) registry.Add(definition);
            Assertions.Equal(3, registry.All.Length,
                "Registry lost an Eastern category.");
            Assertions.Throws<InvalidOperationException>(() => registry.Add(
                EasternWeaponCatalog.RequireCategory(EasternWeaponFamily.Wakizashi)),
                "Registry accepted a duplicate category definition.");
            Assertions.Throws<InvalidOperationException>(() =>
                registry.ValidateLoadedValues(new[] {
                    new KeyValuePair<int, string>(0x004b4d49, "ForeignKatana") }),
                "Registry accepted a loaded category collision.");
            registry.ValidateLoadedValues(new[] {
                new KeyValuePair<int, string>(74, "NativeCategory") });
        }
    }
}
