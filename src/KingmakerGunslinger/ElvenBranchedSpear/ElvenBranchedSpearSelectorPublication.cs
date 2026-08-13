using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;

namespace KingmakerGunslinger.ElvenBranchedSpear
{
    internal sealed class ElvenBranchedSpearSelectorPublication
    {
        private readonly BlueprintFeatureSelection _ewpSelection;
        private readonly BlueprintFeatureSelection _finesseSelection;
        private readonly BlueprintFeature _familiarity;
        private readonly BlueprintFeature[] _ewpFeaturesBefore;
        private readonly BlueprintFeature[] _ewpAllBefore;
        private readonly BlueprintFeature[] _finesseFeaturesBefore;
        private readonly BlueprintFeature[] _finesseAllBefore;
        private readonly BlueprintComponent[] _familiarityBefore;
        private bool _rolledBack;

        private ElvenBranchedSpearSelectorPublication(
            BlueprintFeatureSelection ewpSelection,
            BlueprintFeatureSelection finesseSelection,
            BlueprintFeature familiarity)
        {
            _ewpSelection = ewpSelection;
            _finesseSelection = finesseSelection;
            _familiarity = familiarity;
            _ewpFeaturesBefore = ewpSelection.Features;
            _ewpAllBefore = ewpSelection.AllFeatures;
            _finesseFeaturesBefore = finesseSelection.Features;
            _finesseAllBefore = finesseSelection.AllFeatures;
            _familiarityBefore = familiarity.ComponentsArray;
        }

        internal static ElvenBranchedSpearSelectorPublication Publish(
            BlueprintFeatureSelection ewpSelection, BlueprintFeature ewp,
            BlueprintFeatureSelection finesseSelection, BlueprintFeature finesse,
            BlueprintFeature familiarity, WeaponCategory category,
            BlueprintParametrizedFeature[] parameterSelectors,
            bool publishSelectors)
        {
            if (ewpSelection == null || ewp == null || finesseSelection == null ||
                finesse == null || familiarity == null ||
                parameterSelectors == null || parameterSelectors.Any(value => value == null))
                throw new ArgumentNullException("Spear selector publication is incomplete.");
            var publication = new ElvenBranchedSpearSelectorPublication(
                ewpSelection, finesseSelection, familiarity);
            try
            {
                publication.PublishFamiliarity(category);
                if (publishSelectors)
                {
                    ewpSelection.Features = AppendUnique(ewpSelection.Features, ewp);
                    ewpSelection.AllFeatures = AppendUnique(ewpSelection.AllFeatures, ewp);
                    finesseSelection.Features = AppendUnique(finesseSelection.Features,
                        finesse);
                    finesseSelection.AllFeatures = AppendUnique(
                        finesseSelection.AllFeatures, finesse);
                }
                ElvenBranchedSpearSelectorRuntime.Configure(parameterSelectors,
                    publishSelectors);
                publication.Validate(ewp, finesse, category, publishSelectors);
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
            _familiarity.ComponentsArray = _familiarityBefore;
            ElvenBranchedSpearSelectorRuntime.Rollback();
            _rolledBack = true;
        }

        private void PublishFamiliarity(WeaponCategory category)
        {
            BlueprintComponent[] source = _familiarity.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            AddProficiencies[] grants = source.OfType<AddProficiencies>().ToArray();
            if (grants.Length != 1)
                throw new InvalidOperationException(
                    "Native Elven Weapon Familiarity proficiency shape changed.");
            var replacement = (AddProficiencies)UnityEngine.Object.Instantiate(grants[0]);
            WeaponCategory[] categories = replacement.WeaponProficiencies ??
                Array.Empty<WeaponCategory>();
            replacement.WeaponProficiencies = categories.Contains(category)
                ? categories.ToArray()
                : categories.Concat(new[] { category }).ToArray();
            BlueprintComponent[] next = (BlueprintComponent[])source.Clone();
            next[Array.IndexOf(source, grants[0])] = replacement;
            _familiarity.ComponentsArray = next;
        }

        private void Validate(BlueprintFeature ewp, BlueprintFeature finesse,
            WeaponCategory category, bool publishSelectors)
        {
            int expected = publishSelectors ? 1 : 0;
            if (Count(_ewpSelection.Features, ewp) != expected ||
                Count(_ewpSelection.AllFeatures, ewp) != expected ||
                Count(_finesseSelection.Features, finesse) != expected ||
                Count(_finesseSelection.AllFeatures, finesse) != expected)
                throw new InvalidOperationException(
                    "Spear static selector publication is not exact.");
            AddProficiencies grant = (_familiarity.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<AddProficiencies>().Single();
            if ((grant.WeaponProficiencies ?? Array.Empty<WeaponCategory>())
                .Count(value => value.Equals(category)) != 1)
                throw new InvalidOperationException(
                    "Elven Weapon Familiarity did not receive exactly one spear category.");
        }

        private static int Count(BlueprintFeature[] values, BlueprintFeature expected)
        {
            return (values ?? Array.Empty<BlueprintFeature>()).Count(value =>
                ReferenceEquals(value, expected) || value != null &&
                string.Equals(value.AssetGuid, expected.AssetGuid,
                    StringComparison.Ordinal));
        }

        private static BlueprintFeature[] AppendUnique(BlueprintFeature[] source,
            BlueprintFeature addition)
        {
            source = source ?? Array.Empty<BlueprintFeature>();
            if (source.Any(value => ReferenceEquals(value, addition) || value != null &&
                string.Equals(value.AssetGuid, addition.AssetGuid,
                    StringComparison.Ordinal))) return source;
            return source.Concat(new[] { addition }).ToArray();
        }
    }
}
