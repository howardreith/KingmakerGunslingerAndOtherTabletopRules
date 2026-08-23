using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Enums;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.CustomWeapons;

namespace KingmakerGunslinger.EasternWeapons
{
    internal sealed class EasternWeaponSelectorPublication
    {
        private const string SourceKey = "eastern-weapons";
        private readonly BlueprintFeatureSelection _ewpSelection;
        private readonly BlueprintFeatureSelection _finesseSelection;
        private readonly BlueprintFeature[] _ewpFeaturesBefore;
        private readonly BlueprintFeature[] _ewpAllBefore;
        private readonly BlueprintFeature[] _finesseFeaturesBefore;
        private readonly BlueprintFeature[] _finesseAllBefore;
        private bool _rolledBack;

        private EasternWeaponSelectorPublication(
            BlueprintFeatureSelection ewpSelection,
            BlueprintFeatureSelection finesseSelection)
        {
            _ewpSelection = ewpSelection;
            _finesseSelection = finesseSelection;
            _ewpFeaturesBefore = ewpSelection.Features;
            _ewpAllBefore = ewpSelection.AllFeatures;
            _finesseFeaturesBefore = finesseSelection.Features;
            _finesseAllBefore = finesseSelection.AllFeatures;
        }

        internal static EasternWeaponSelectorPublication Publish(
            LibraryScriptableObject library, BlueprintFeature wakizashiEwp,
            BlueprintFeature katanaEwp, BlueprintFeature wakizashiFinesse,
            BlueprintParametrizedFeature[] parameterSelectors,
            bool publishSelectors)
        {
            if (library == null || wakizashiEwp == null || katanaEwp == null ||
                wakizashiFinesse == null || parameterSelectors == null ||
                parameterSelectors.Any(value => value == null))
                throw new ArgumentNullException(
                    "Eastern selector publication is incomplete.");
            BlueprintFeatureSelection ewpSelection = BlueprintLibraryLookup
                .RequireExact<BlueprintFeatureSelection>(library,
                    Blueprints.EasternWeaponBlueprints
                        .NativeExoticWeaponProficiencySelectionGuid,
                    "native Exotic Weapon Proficiency selection");
            BlueprintFeatureSelection finesseSelection = BlueprintLibraryLookup
                .RequireExact<BlueprintFeatureSelection>(library,
                    Blueprints.EasternWeaponBlueprints
                        .NativeFinesseTrainingSelectionGuid,
                    "native Rogue Finesse Training selection");
            BlueprintFeature curveAnchor = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(library,
                    Blueprints.EasternWeaponBlueprints
                        .NativeElvenCurveBladeProficiencyGuid,
                    "native Elven Curve Blade proficiency ordering anchor");
            BlueprintFeature spearAnchor = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(library,
                    Blueprints.EasternWeaponBlueprints
                        .ElvenBranchedSpearProficiencyGuid,
                    "KMG Elven Branched Spear proficiency ordering anchor");
            var publication = new EasternWeaponSelectorPublication(
                ewpSelection, finesseSelection);
            try
            {
                if (publishSelectors)
                {
                    ewpSelection.Features = Remove(ewpSelection.Features,
                        wakizashiEwp, katanaEwp);
                    ewpSelection.AllFeatures = InsertOrderedAfter(
                        ewpSelection.AllFeatures, curveAnchor, spearAnchor,
                        katanaEwp, wakizashiEwp);
                    finesseSelection.Features = AppendUnique(
                        finesseSelection.Features, wakizashiFinesse);
                    finesseSelection.AllFeatures = AppendUnique(
                        finesseSelection.AllFeatures, wakizashiFinesse);
                }
                else
                {
                    ewpSelection.Features = Remove(ewpSelection.Features,
                        wakizashiEwp, katanaEwp);
                    ewpSelection.AllFeatures = Remove(ewpSelection.AllFeatures,
                        wakizashiEwp, katanaEwp);
                    finesseSelection.Features = Remove(finesseSelection.Features,
                        wakizashiFinesse);
                    finesseSelection.AllFeatures = Remove(
                        finesseSelection.AllFeatures, wakizashiFinesse);
                }
                CustomWeaponSelectorRuntime.Configure(SourceKey,
                    parameterSelectors, EasternWeaponCatalog.AllCategories
                        .Select(value => new CustomWeaponSelectorOption(
                            (WeaponCategory)value.CategoryValue,
                            value.Presentation.DisplayName,
                            value.Presentation.Acronym)).ToArray(),
                    publishSelectors);
                publication.Validate(wakizashiEwp, katanaEwp,
                    wakizashiFinesse, curveAnchor, spearAnchor,
                    publishSelectors);
                return publication;
            }
            catch
            {
                publication.Rollback();
                throw;
            }
        }

        internal void Rollback()
        {
            if (_rolledBack) return;
            _ewpSelection.Features = _ewpFeaturesBefore;
            _ewpSelection.AllFeatures = _ewpAllBefore;
            _finesseSelection.Features = _finesseFeaturesBefore;
            _finesseSelection.AllFeatures = _finesseAllBefore;
            CustomWeaponSelectorRuntime.Rollback(SourceKey);
            _rolledBack = true;
        }

        private void Validate(BlueprintFeature wakizashiEwp,
            BlueprintFeature katanaEwp, BlueprintFeature wakizashiFinesse,
            BlueprintFeature curveAnchor, BlueprintFeature spearAnchor,
            bool publishSelectors)
        {
            int expected = publishSelectors ? 1 : 0;
            if (Count(_ewpSelection.Features, wakizashiEwp) != 0 ||
                Count(_ewpSelection.Features, katanaEwp) != 0 ||
                Count(_ewpSelection.AllFeatures, wakizashiEwp) != expected ||
                Count(_ewpSelection.AllFeatures, katanaEwp) != expected ||
                Count(_finesseSelection.Features, wakizashiFinesse) != expected ||
                Count(_finesseSelection.AllFeatures, wakizashiFinesse) != expected)
                throw new InvalidOperationException(
                    "Eastern static selector publication is not exact.");
            if (publishSelectors &&
                ((Count(_ewpSelection.AllFeatures, spearAnchor) == 1 &&
                  (!ImmediatelyFollows(_ewpSelection.AllFeatures, spearAnchor,
                    curveAnchor) || !ImmediatelyFollows(
                        _ewpSelection.AllFeatures, katanaEwp, spearAnchor))) ||
                 (Count(_ewpSelection.AllFeatures, spearAnchor) == 0 &&
                  !ImmediatelyFollows(_ewpSelection.AllFeatures, katanaEwp,
                    curveAnchor)) ||
                 Count(_ewpSelection.AllFeatures, spearAnchor) > 1 ||
                 !ImmediatelyFollows(_ewpSelection.AllFeatures, wakizashiEwp,
                    katanaEwp)))
                throw new InvalidOperationException(
                    "Eastern proficiencies are not in the merged native order.");
        }

        private static BlueprintFeature[] InsertOrderedAfter(
            BlueprintFeature[] source, BlueprintFeature curveAnchor,
            params BlueprintFeature[] ordered)
        {
            BlueprintFeature[] additions = ordered.Skip(1).ToArray();
            BlueprintFeature[] normalized = Remove(source, additions);
            int curveIndex = Array.FindIndex(normalized, value =>
                SameFeature(value, curveAnchor));
            int spearCount = ordered.Length == 0 ? 0 : normalized.Count(value =>
                SameFeature(value, ordered[0]));
            if (curveIndex < 0 || normalized.Count(value =>
                    SameFeature(value, curveAnchor)) != 1 || ordered.Length == 0 ||
                spearCount > 1 || spearCount == 1 &&
                !SameFeature(normalized.ElementAtOrDefault(curveIndex + 1),
                    ordered[0]))
                throw new InvalidOperationException(
                    "Accepted Elven Branched Spear merged ordering changed.");
            var result = normalized.ToList();
            int insertion = curveIndex + (spearCount == 1 ? 2 : 1);
            for (int index = 1; index < ordered.Length; index++)
                result.Insert(insertion++, ordered[index]);
            return result.ToArray();
        }

        private static BlueprintFeature[] AppendUnique(BlueprintFeature[] source,
            BlueprintFeature addition)
        {
            source = source ?? Array.Empty<BlueprintFeature>();
            return source.Any(value => SameFeature(value, addition)) ? source :
                source.Concat(new[] { addition }).ToArray();
        }

        private static BlueprintFeature[] Remove(BlueprintFeature[] source,
            params BlueprintFeature[] removals)
        {
            return (source ?? Array.Empty<BlueprintFeature>()).Where(value =>
                !removals.Any(removal => SameFeature(value, removal))).ToArray();
        }

        private static int Count(BlueprintFeature[] values,
            BlueprintFeature expected)
        {
            return (values ?? Array.Empty<BlueprintFeature>()).Count(value =>
                SameFeature(value, expected));
        }

        private static bool ImmediatelyFollows(BlueprintFeature[] source,
            BlueprintFeature addition, BlueprintFeature anchor)
        {
            source = source ?? Array.Empty<BlueprintFeature>();
            int anchorIndex = Array.FindIndex(source, value =>
                SameFeature(value, anchor));
            return anchorIndex >= 0 && anchorIndex + 1 < source.Length &&
                SameFeature(source[anchorIndex + 1], addition);
        }

        private static bool SameFeature(BlueprintFeature left,
            BlueprintFeature right)
        {
            return ReferenceEquals(left, right) || left != null && right != null &&
                string.Equals(left.AssetGuid, right.AssetGuid,
                    StringComparison.Ordinal);
        }
    }
}
