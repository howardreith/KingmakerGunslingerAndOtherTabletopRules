using System;
using Harmony12;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Enums;
using KingmakerGunslinger.CustomWeapons;

namespace KingmakerGunslinger.EasternWeapons
{
    internal static class EasternWeaponCategoryRuntime
    {
        private static bool _presentationEnabled;

        internal static void Configure(bool presentationEnabled)
        { _presentationEnabled = presentationEnabled; }

        internal static WeaponCategory Category(EasternWeaponFamily family)
        { return (WeaponCategory)EasternWeaponCatalog.RequireCategory(family).CategoryValue; }

        internal static bool TryGet(WeaponCategory category,
            out CustomWeaponCategoryDefinition definition)
        {
            int value = (int)category;
            foreach (CustomWeaponCategoryDefinition candidate in
                EasternWeaponCatalog.AllCategories)
                if (candidate.CategoryValue == value)
                {
                    definition = candidate;
                    return true;
                }
            definition = null;
            return false;
        }

        internal static bool HasSubCategory(WeaponCategory category,
            WeaponSubCategory subCategory)
        {
            CustomWeaponCategoryDefinition definition;
            if (!TryGet(category, out definition)) return false;
            if (subCategory == WeaponSubCategory.None ||
                subCategory == WeaponSubCategory.Melee ||
                subCategory == WeaponSubCategory.Metal) return true;
            if (subCategory == WeaponSubCategory.Light)
                return definition.Handedness == CustomWeaponHandedness.Light;
            if (subCategory == WeaponSubCategory.Finessable)
                return definition.Finessable;
            if (subCategory == WeaponSubCategory.TwoHanded)
                return definition.Handedness == CustomWeaponHandedness.TwoHanded;
            if (subCategory == WeaponSubCategory.Exotic)
                return definition.Proficiency != CustomWeaponProficiencyPolicy.Martial;
            if (subCategory == WeaponSubCategory.Martial)
                return definition.Proficiency == CustomWeaponProficiencyPolicy.Martial;
            if (subCategory == WeaponSubCategory.OneHandedPiercing)
                return definition.Handedness != CustomWeaponHandedness.TwoHanded &&
                    (definition.DamageForms & CustomWeaponDamageForm.Piercing) != 0;
            if (subCategory == WeaponSubCategory.OneHandedSlashing)
                return definition.Handedness != CustomWeaponHandedness.TwoHanded &&
                    (definition.DamageForms & CustomWeaponDamageForm.Slashing) != 0;
            return false;
        }

        internal static WeaponSubCategory[] GetSubCategories(
            WeaponCategory category)
        {
            CustomWeaponCategoryDefinition definition;
            if (!TryGet(category, out definition))
                return Array.Empty<WeaponSubCategory>();
            var values = new System.Collections.Generic.List<WeaponSubCategory>
            {
                WeaponSubCategory.Melee,
                WeaponSubCategory.Metal
            };
            if (definition.Handedness == CustomWeaponHandedness.Light)
                values.Add(WeaponSubCategory.Light);
            if (definition.Handedness == CustomWeaponHandedness.TwoHanded)
                values.Add(WeaponSubCategory.TwoHanded);
            if (definition.Finessable) values.Add(WeaponSubCategory.Finessable);
            values.Add(definition.Proficiency == CustomWeaponProficiencyPolicy.Martial
                ? WeaponSubCategory.Martial : WeaponSubCategory.Exotic);
            if (definition.Handedness != CustomWeaponHandedness.TwoHanded &&
                (definition.DamageForms & CustomWeaponDamageForm.Piercing) != 0)
                values.Add(WeaponSubCategory.OneHandedPiercing);
            if (definition.Handedness != CustomWeaponHandedness.TwoHanded &&
                (definition.DamageForms & CustomWeaponDamageForm.Slashing) != 0)
                values.Add(WeaponSubCategory.OneHandedSlashing);
            return values.ToArray();
        }

        internal static bool IsKatana(WeaponCategory category)
        { return category.Equals(Category(EasternWeaponFamily.Katana)); }

        internal static bool PresentationEnabled
        { get { return _presentationEnabled; } }
    }

    [HarmonyPatch(typeof(StatsStrings), "GetText",
        new[] { typeof(WeaponCategory) })]
    internal static class EasternWeaponCategoryDisplayNamePatch
    {
        private static void Postfix(WeaponCategory stat, ref string __result)
        {
            CustomWeaponCategoryDefinition definition;
            if (EasternWeaponCategoryRuntime.PresentationEnabled &&
                EasternWeaponCategoryRuntime.TryGet(stat, out definition))
                __result = definition.Presentation.DisplayName;
        }
    }

    [HarmonyPatch(typeof(WeaponCategoryExtension), "HasSubCategory")]
    internal static class EasternWeaponHasSubCategoryPatch
    {
        private static void Postfix(WeaponCategory category,
            WeaponSubCategory subCategory, ref bool __result)
        {
            CustomWeaponCategoryDefinition definition;
            if (EasternWeaponCategoryRuntime.TryGet(category, out definition))
                __result = EasternWeaponCategoryRuntime.HasSubCategory(
                    category, subCategory);
        }
    }

    [HarmonyPatch(typeof(WeaponCategoryExtension), "GetSubCategories")]
    internal static class EasternWeaponGetSubCategoriesPatch
    {
        private static void Postfix(WeaponCategory category,
            ref WeaponSubCategory[] __result)
        {
            CustomWeaponCategoryDefinition definition;
            if (EasternWeaponCategoryRuntime.TryGet(category, out definition))
                __result = EasternWeaponCategoryRuntime.GetSubCategories(category);
        }
    }

    [HarmonyPatch(typeof(BlueprintWeaponType),
        "get_IsOneHandedWhichCanBeUsedWithTwoHands")]
    internal static class EasternWeaponVersatileKatanaPatch
    {
        private static void Postfix(BlueprintWeaponType __instance,
            ref bool __result)
        {
            if (__instance != null &&
                EasternWeaponCategoryRuntime.IsKatana(__instance.Category))
                __result = true;
        }
    }
}
