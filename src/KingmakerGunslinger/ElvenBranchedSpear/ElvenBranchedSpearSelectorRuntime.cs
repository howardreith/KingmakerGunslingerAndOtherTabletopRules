using System;
using System.Collections.Generic;
using System.Linq;
using Harmony12;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.ElvenBranchedSpear
{
    internal static class ElvenBranchedSpearSelectorRuntime
    {
        private static readonly object Sync = new object();
        private static BlueprintParametrizedFeature[] _selectors;
        private static bool _publicationEnabled;

        internal static void Configure(BlueprintParametrizedFeature[] selectors,
            bool publicationEnabled)
        {
            if (selectors == null || selectors.Length == 0 ||
                selectors.Any(value => value == null) ||
                selectors.Distinct().Count() != selectors.Length)
                throw new ArgumentException("Spear parameter selectors are incomplete.");
            lock (Sync)
            {
                _selectors = (BlueprintParametrizedFeature[])selectors.Clone();
                _publicationEnabled = publicationEnabled;
            }
        }

        internal static void Rollback()
        {
            lock (Sync)
            {
                _selectors = null;
                _publicationEnabled = false;
            }
        }

        internal static IEnumerable<FeatureUIData> Append(
            BlueprintParametrizedFeature feature, IEnumerable<FeatureUIData> source)
        {
            FeatureUIData[] existing = (source ?? Enumerable.Empty<FeatureUIData>())
                .Where(value => value != null).ToArray();
            lock (Sync)
            {
                if (!_publicationEnabled || _selectors == null ||
                    Array.IndexOf(_selectors, feature) < 0)
                    return existing;
            }
            WeaponCategory category = ElvenBranchedSpearCategoryRuntime.Category;
            if (existing.Any(value => value.Param != null &&
                value.Param.WeaponCategory.HasValue &&
                value.Param.WeaponCategory.Value.Equals(category)))
                return existing;
            const string name = "Elven Branched Spear";
            const string description = "Apply this ordinary chosen-weapon feature to the Elven Branched Spear category.";
            var result = new List<FeatureUIData>(existing)
            {
                new FeatureUIData(feature, new FeatureParam(category), name,
                    description, feature.Icon, name)
            };
            return result.OrderBy(value => value.Name,
                StringComparer.CurrentCultureIgnoreCase).ToArray();
        }
    }

    [HarmonyPatch(typeof(BlueprintParametrizedFeature), "GetFullSelectionItems")]
    internal static class ElvenBranchedSpearFullSelectorPatch
    {
        private static void Postfix(BlueprintParametrizedFeature __instance,
            ref IEnumerable<FeatureUIData> __result)
        {
            __result = ElvenBranchedSpearSelectorRuntime.Append(__instance, __result);
        }
    }

    [HarmonyPatch(typeof(BlueprintParametrizedFeature), "ExtractSelectionItems")]
    internal static class ElvenBranchedSpearLevelUpSelectorPatch
    {
        private static void Postfix(BlueprintParametrizedFeature __instance,
            UnitDescriptor beforeLevelUpUnit, UnitDescriptor previewUnit,
            ref IEnumerable<IFeatureSelectionItem> __result)
        {
            IEnumerable<FeatureUIData> source = (__result ??
                Enumerable.Empty<IFeatureSelectionItem>()).OfType<FeatureUIData>();
            __result = ElvenBranchedSpearSelectorRuntime.Append(__instance, source)
                .Cast<IFeatureSelectionItem>();
        }
    }
}
