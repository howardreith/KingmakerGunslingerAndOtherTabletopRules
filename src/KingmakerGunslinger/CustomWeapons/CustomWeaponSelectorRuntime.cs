using System;
using System.Collections.Generic;
using System.Linq;
using Harmony12;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.CustomWeapons
{
    internal sealed class CustomWeaponSelectorOption
    {
        internal CustomWeaponSelectorOption(WeaponCategory category,
            string displayName, string monogram)
        {
            if (string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(monogram))
                throw new ArgumentException(
                    "Custom weapon selector presentation is incomplete.");
            Category = category;
            DisplayName = displayName;
            Monogram = monogram;
        }

        internal WeaponCategory Category { get; private set; }
        internal string DisplayName { get; private set; }
        internal string Monogram { get; private set; }
    }

    /// <summary>
    /// One shared publication and sorting boundary for every custom weapon
    /// category. Source-specific configuration permits exact transactional
    /// rollback without disturbing a previously published feature family.
    /// </summary>
    internal static class CustomWeaponSelectorRuntime
    {
        private static readonly object Sync = new object();
        private static readonly Dictionary<string, SourceState> Sources =
            new Dictionary<string, SourceState>(StringComparer.Ordinal);

        internal static void Configure(string sourceKey,
            BlueprintParametrizedFeature[] selectors,
            CustomWeaponSelectorOption[] options, bool publicationEnabled)
        {
            if (string.IsNullOrWhiteSpace(sourceKey) || selectors == null ||
                selectors.Length == 0 || selectors.Any(value => value == null) ||
                selectors.Distinct().Count() != selectors.Length ||
                options == null || options.Length == 0 ||
                options.Any(value => value == null) ||
                options.Select(value => value.Category).Distinct().Count() !=
                    options.Length)
                throw new ArgumentException(
                    "Custom weapon selector configuration is incomplete.");
            lock (Sync)
            {
                Sources[sourceKey] = new SourceState(selectors, options,
                    publicationEnabled);
                ValidateConflicts();
            }
        }

        internal static void Rollback(string sourceKey)
        {
            if (string.IsNullOrWhiteSpace(sourceKey)) return;
            lock (Sync) Sources.Remove(sourceKey);
        }

        internal static IEnumerable<FeatureUIData> Append(
            BlueprintParametrizedFeature feature,
            IEnumerable<FeatureUIData> source)
        {
            FeatureUIData[] existing = (source ??
                Enumerable.Empty<FeatureUIData>()).Where(value => value != null)
                .ToArray();
            CustomWeaponSelectorOption[] additions;
            lock (Sync)
            {
                additions = Sources.Values.Where(value => value.Enabled &&
                        Array.IndexOf(value.Selectors, feature) >= 0)
                    .SelectMany(value => value.Options)
                    .GroupBy(value => value.Category)
                    .Select(value => value.Single()).ToArray();
            }
            if (additions.Length == 0) return existing;
            var result = new List<FeatureUIData>(existing);
            foreach (CustomWeaponSelectorOption option in additions)
            {
                if (result.Any(value => value.Param != null &&
                    value.Param.WeaponCategory.HasValue &&
                    value.Param.WeaponCategory.Value.Equals(option.Category)))
                    continue;
                result.Add(new FeatureUIData(feature,
                    new FeatureParam(option.Category), option.DisplayName,
                    string.Empty, null, option.Monogram));
            }
            return result.OrderBy(value => value.Name,
                StringComparer.CurrentCultureIgnoreCase).ToArray();
        }

        private static void ValidateConflicts()
        {
            foreach (IGrouping<WeaponCategory, CustomWeaponSelectorOption> group in
                Sources.Values.SelectMany(value => value.Options)
                    .GroupBy(value => value.Category))
            {
                if (group.Select(value => value.DisplayName)
                        .Distinct(StringComparer.Ordinal).Count() != 1 ||
                    group.Select(value => value.Monogram)
                        .Distinct(StringComparer.Ordinal).Count() != 1)
                    throw new InvalidOperationException(
                        "Custom weapon selector category presentation collides: " +
                        (int)group.Key + ".");
            }
        }

        private sealed class SourceState
        {
            internal SourceState(BlueprintParametrizedFeature[] selectors,
                CustomWeaponSelectorOption[] options, bool enabled)
            {
                Selectors = (BlueprintParametrizedFeature[])selectors.Clone();
                Options = (CustomWeaponSelectorOption[])options.Clone();
                Enabled = enabled;
            }
            internal BlueprintParametrizedFeature[] Selectors { get; private set; }
            internal CustomWeaponSelectorOption[] Options { get; private set; }
            internal bool Enabled { get; private set; }
        }
    }

    [HarmonyPatch(typeof(BlueprintParametrizedFeature),
        "GetFullSelectionItems")]
    internal static class CustomWeaponFullSelectorPatch
    {
        private static void Postfix(BlueprintParametrizedFeature __instance,
            ref IEnumerable<FeatureUIData> __result)
        {
            __result = CustomWeaponSelectorRuntime.Append(__instance, __result);
        }
    }

    [HarmonyPatch(typeof(BlueprintParametrizedFeature),
        "ExtractSelectionItems")]
    internal static class CustomWeaponLevelUpSelectorPatch
    {
        private static void Postfix(BlueprintParametrizedFeature __instance,
            UnitDescriptor beforeLevelUpUnit, UnitDescriptor previewUnit,
            ref IEnumerable<IFeatureSelectionItem> __result)
        {
            IEnumerable<FeatureUIData> source = (__result ??
                Enumerable.Empty<IFeatureSelectionItem>()).OfType<FeatureUIData>();
            __result = CustomWeaponSelectorRuntime.Append(__instance, source)
                .Cast<IFeatureSelectionItem>();
        }
    }
}
