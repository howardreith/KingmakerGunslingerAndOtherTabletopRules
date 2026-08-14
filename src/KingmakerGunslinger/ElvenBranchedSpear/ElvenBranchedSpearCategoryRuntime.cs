using System;
using Harmony12;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Enums;

namespace KingmakerGunslinger.ElvenBranchedSpear
{
    internal static class ElvenBranchedSpearCategoryRuntime
    {
        internal const string DisplayName = "Elven Branched Spear";
        internal const string Monogram = "EB";

        private static readonly WeaponSubCategory[] SubCategories =
        {
            WeaponSubCategory.Melee,
            WeaponSubCategory.Finessable,
            WeaponSubCategory.TwoHanded,
            WeaponSubCategory.Exotic,
            WeaponSubCategory.Metal
        };

        internal static WeaponCategory Category
        { get { return (WeaponCategory)ElvenBranchedSpearCatalog.WeaponCategoryValue; } }

        internal static bool Owns(WeaponCategory category)
        { return category.Equals(Category); }

        internal static bool HasSubCategory(WeaponSubCategory subCategory)
        {
            if (subCategory == WeaponSubCategory.None) return true;
            return Array.IndexOf(SubCategories, subCategory) >= 0;
        }

        internal static WeaponSubCategory[] GetSubCategories()
        { return (WeaponSubCategory[])SubCategories.Clone(); }
    }

    [HarmonyPatch(typeof(StatsStrings), "GetText",
        new[] { typeof(WeaponCategory) })]
    internal static class ElvenBranchedSpearCategoryDisplayNamePatch
    {
        private static void Postfix(WeaponCategory stat, ref string __result)
        {
            if (ElvenBranchedSpearCategoryRuntime.Owns(stat))
                __result = ElvenBranchedSpearCategoryRuntime.DisplayName;
        }
    }

    [HarmonyPatch(typeof(WeaponCategoryExtension), "HasSubCategory")]
    internal static class ElvenBranchedSpearHasSubCategoryPatch
    {
        private static void Postfix(WeaponCategory category,
            WeaponSubCategory subCategory, ref bool __result)
        {
            if (ElvenBranchedSpearCategoryRuntime.Owns(category))
                __result = ElvenBranchedSpearCategoryRuntime.HasSubCategory(subCategory);
        }
    }

    [HarmonyPatch(typeof(WeaponCategoryExtension), "GetSubCategories")]
    internal static class ElvenBranchedSpearGetSubCategoriesPatch
    {
        private static void Postfix(WeaponCategory category,
            ref WeaponSubCategory[] __result)
        {
            if (ElvenBranchedSpearCategoryRuntime.Owns(category))
                __result = ElvenBranchedSpearCategoryRuntime.GetSubCategories();
        }
    }
}
