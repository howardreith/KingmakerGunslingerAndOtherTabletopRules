using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;
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
        private readonly Dictionary<BlueprintFeature, BlueprintComponent[]>
            _martialBefore;
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
            _martialBefore = new Dictionary<BlueprintFeature,
                BlueprintComponent[]>();
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
            BlueprintFeature nativeMartial = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(library,
                    Blueprints.EasternWeaponBlueprints
                        .NativeMartialWeaponProficiencyGuid,
                    "native Martial Weapon Proficiency feature");

            var publication = new EasternWeaponSelectorPublication(
                ewpSelection, finesseSelection);
            try
            {
                BlueprintFeature[] broadMartial = publication.PublishMartial(
                    library, nativeMartial);
                EasternWeaponProficiencyRuntime.Configure(broadMartial);
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
                    wakizashiFinesse, curveAnchor, spearAnchor, broadMartial,
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
            foreach (KeyValuePair<BlueprintFeature, BlueprintComponent[]> entry in
                _martialBefore) entry.Key.ComponentsArray = entry.Value;
            EasternWeaponProficiencyRuntime.Rollback();
            CustomWeaponSelectorRuntime.Rollback(SourceKey);
            _rolledBack = true;
        }

        private BlueprintFeature[] PublishMartial(
            LibraryScriptableObject library, BlueprintFeature nativeMartial)
        {
            AddProficiencies[] nativeGrants = (nativeMartial.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<AddProficiencies>()
                .Where(value => (value.WeaponProficiencies ??
                    Array.Empty<WeaponCategory>()).Distinct().Count() >= 20)
                .OrderByDescending(value => (value.WeaponProficiencies ??
                    Array.Empty<WeaponCategory>()).Distinct().Count())
                .ToArray();
            if (nativeGrants.Length == 0)
                throw new InvalidOperationException(
                    "Native Martial Weapon Proficiency has no broad weapon grant.");
            int largestGrant = (nativeGrants[0].WeaponProficiencies ??
                Array.Empty<WeaponCategory>()).Distinct().Count();
            AddProficiencies[] largest = nativeGrants.Where(value =>
                (value.WeaponProficiencies ?? Array.Empty<WeaponCategory>())
                    .Distinct().Count() == largestGrant).ToArray();
            if (largest.Length != 1)
                throw new InvalidOperationException(
                    "Native Martial Weapon Proficiency broad grant is ambiguous.");
            AddProficiencies nativeGrant = largest[0];
            WeaponCategory[] authority = (nativeGrant.WeaponProficiencies ??
                Array.Empty<WeaponCategory>()).Distinct().ToArray();
            if (authority.Length < 20)
                throw new InvalidOperationException(
                    "Native Martial Weapon Proficiency category authority changed.");
            WeaponCategory nodachi = EasternWeaponCategoryRuntime.Category(
                EasternWeaponFamily.Nodachi);
            var broad = new List<BlueprintFeature>();
            foreach (BlueprintFeature feature in library.GetAllBlueprints()
                .OfType<BlueprintFeature>().Where(value => value != null))
            {
                BlueprintComponent[] components = feature.ComponentsArray ??
                    Array.Empty<BlueprintComponent>();
                AddProficiencies[] grants = components.OfType<AddProficiencies>()
                    .Where(value => authority.All(category =>
                        (value.WeaponProficiencies ??
                            Array.Empty<WeaponCategory>()).Contains(category)))
                    .ToArray();
                if (grants.Length == 0) continue;
                broad.Add(feature);
                BlueprintComponent[] next =
                    (BlueprintComponent[])components.Clone();
                bool changed = false;
                foreach (AddProficiencies grant in grants)
                {
                    WeaponCategory[] categories = grant.WeaponProficiencies ??
                        Array.Empty<WeaponCategory>();
                    if (categories.Contains(nodachi)) continue;
                    var replacement = (AddProficiencies)
                        UnityEngine.Object.Instantiate(grant);
                    replacement.WeaponProficiencies = categories
                        .Concat(new[] { nodachi }).ToArray();
                    next[Array.IndexOf(components, grant)] = replacement;
                    changed = true;
                }
                if (changed)
                {
                    _martialBefore.Add(feature, components);
                    feature.ComponentsArray = next;
                }
            }
            if (!broad.Contains(nativeMartial))
                throw new InvalidOperationException(
                    "Native Martial Weapon Proficiency was not classified as broad.");
            return broad.Distinct().ToArray();
        }

        private void Validate(BlueprintFeature wakizashiEwp,
            BlueprintFeature katanaEwp, BlueprintFeature wakizashiFinesse,
            BlueprintFeature curveAnchor, BlueprintFeature spearAnchor,
            BlueprintFeature[] broadMartial, bool publishSelectors)
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
            WeaponCategory nodachi = EasternWeaponCategoryRuntime.Category(
                EasternWeaponFamily.Nodachi);
            if (broadMartial.Length == 0 || broadMartial.Any(feature =>
                !(feature.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .OfType<AddProficiencies>().Any(grant =>
                        (grant.WeaponProficiencies ??
                            Array.Empty<WeaponCategory>()).Count(value =>
                                value.Equals(nodachi)) == 1)))
                throw new InvalidOperationException(
                    "Broad martial grants did not receive exactly one Nodachi category.");
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
