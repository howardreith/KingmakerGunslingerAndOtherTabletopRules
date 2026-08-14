using System;
using System.Linq;
using KingmakerGunslinger.CustomWeapons;

namespace KingmakerGunslinger.EasternWeapons
{
    internal enum EasternWeaponFamily
    {
        Wakizashi = 0,
        Katana = 1,
        Nodachi = 2
    }

    internal enum EasternWeaponGenericKind
    {
        Mundane = 0,
        Masterwork = 1,
        ColdIron = 2,
        PlusOne = 3
    }

    internal sealed class EasternWeaponGenericSpec
    {
        internal EasternWeaponGenericSpec(EasternWeaponFamily family,
            EasternWeaponGenericKind kind, string symbol, string internalName,
            string displayName, int cost, bool masterwork, bool coldIron,
            int enhancement)
        {
            if (string.IsNullOrWhiteSpace(symbol) ||
                string.IsNullOrWhiteSpace(internalName) ||
                string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Eastern weapon identity is incomplete.");
            if (cost < 0 || enhancement < 0 || enhancement > 5)
                throw new ArgumentOutOfRangeException("Eastern weapon price is invalid.");
            if (enhancement > 0 && !masterwork)
                throw new ArgumentException("Magic weapons must include masterwork quality.");
            Family = family;
            Kind = kind;
            Symbol = symbol;
            InternalName = internalName;
            DisplayName = displayName;
            Cost = cost;
            Masterwork = masterwork;
            ColdIron = coldIron;
            Enhancement = enhancement;
        }

        internal EasternWeaponFamily Family { get; private set; }
        internal EasternWeaponGenericKind Kind { get; private set; }
        internal string Symbol { get; private set; }
        internal string InternalName { get; private set; }
        internal string DisplayName { get; private set; }
        internal int Cost { get; private set; }
        internal bool Masterwork { get; private set; }
        internal bool ColdIron { get; private set; }
        internal int Enhancement { get; private set; }
    }

    internal static class EasternWeaponCatalog
    {
        internal const string AssetBundleName = "kingmakergunslinger.easternweapons";
        internal const int MasterworkPremium = 300;
        internal const int PlusOneMagicPremium = 2000;

        private static readonly CustomWeaponCategoryDefinition[] Categories =
        {
            new CustomWeaponCategoryDefinition("wakizashi", 0x004b4d48,
                "KMG.EasternWeapons.Wakizashi.WeaponType", 35, 2, 1, 6, 18, 2,
                CustomWeaponDamageForm.Piercing | CustomWeaponDamageForm.Slashing,
                CustomWeaponHandedness.Light,
                CustomWeaponProficiencyPolicy.Exotic,
                CustomWeaponFighterGroupPolicy.LightBlades, true, false, false,
                new CustomWeaponPresentationDefinition("Wakizashi", "WK",
                    "wakizashi.png", "KMG_EasternWeapons_Wakizashi")),
            new CustomWeaponCategoryDefinition("katana", 0x004b4d49,
                "KMG.EasternWeapons.Katana.WeaponType", 50, 6, 1, 8, 18, 2,
                CustomWeaponDamageForm.Slashing,
                CustomWeaponHandedness.OneHandedVersatile,
                CustomWeaponProficiencyPolicy.KatanaGripDependent,
                CustomWeaponFighterGroupPolicy.HeavyBlades, false, false, false,
                new CustomWeaponPresentationDefinition("Katana", "KA",
                    "katana.png", "KMG_EasternWeapons_Katana")),
            new CustomWeaponCategoryDefinition("nodachi", 0x004b4d4a,
                "KMG.EasternWeapons.Nodachi.WeaponType", 60, 8, 1, 10, 18, 2,
                CustomWeaponDamageForm.Piercing | CustomWeaponDamageForm.Slashing,
                CustomWeaponHandedness.TwoHanded,
                CustomWeaponProficiencyPolicy.Martial,
                CustomWeaponFighterGroupPolicy.HeavyBlades |
                    CustomWeaponFighterGroupPolicy.Polearms,
                false, false, false,
                new CustomWeaponPresentationDefinition("Nodachi", "NO",
                    "nodachi.png", "KMG_EasternWeapons_Nodachi"))
        };

        private static readonly EasternWeaponGenericSpec[] Items =
            Categories.SelectMany(CreateGenericFamily).ToArray();

        internal static CustomWeaponCategoryDefinition[] AllCategories
        { get { return (CustomWeaponCategoryDefinition[])Categories.Clone(); } }

        internal static EasternWeaponGenericSpec[] AllGenericItems
        { get { return (EasternWeaponGenericSpec[])Items.Clone(); } }

        internal static CustomWeaponCategoryDefinition RequireCategory(
            EasternWeaponFamily family)
        { return Categories[(int)family]; }

        internal static EasternWeaponGenericSpec RequireGeneric(
            EasternWeaponFamily family, EasternWeaponGenericKind kind)
        { return Items.Single(value => value.Family == family && value.Kind == kind); }

        private static EasternWeaponGenericSpec[] CreateGenericFamily(
            CustomWeaponCategoryDefinition category)
        {
            EasternWeaponFamily family = (EasternWeaponFamily)Array.IndexOf(
                Categories, category);
            string name = category.Presentation.DisplayName;
            string stem = "KMG.EasternWeapons." + name;
            string internalStem = "KMG_EasternWeapons_" + name;
            return new[]
            {
                new EasternWeaponGenericSpec(family,
                    EasternWeaponGenericKind.Mundane, stem + ".BaseItem",
                    internalStem + "_Item", name, category.BaseCost,
                    false, false, 0),
                new EasternWeaponGenericSpec(family,
                    EasternWeaponGenericKind.Masterwork, stem + ".MasterworkItem",
                    internalStem + "_Masterwork_Item", "Masterwork " + name,
                    category.BaseCost + MasterworkPremium, true, false, 0),
                new EasternWeaponGenericSpec(family,
                    EasternWeaponGenericKind.ColdIron, stem + ".ColdIronItem",
                    internalStem + "_ColdIron_Item", "Cold Iron " + name,
                    category.BaseCost * 2, false, true, 0),
                new EasternWeaponGenericSpec(family,
                    EasternWeaponGenericKind.PlusOne, stem + ".Plus1Item",
                    internalStem + "_Plus1_Item", "+1 " + name,
                    category.BaseCost + MasterworkPremium + PlusOneMagicPremium,
                    true, false, 1)
            };
        }
    }
}
