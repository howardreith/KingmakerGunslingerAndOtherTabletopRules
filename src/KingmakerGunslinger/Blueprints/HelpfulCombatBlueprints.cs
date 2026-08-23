using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class HelpfulCombatBlueprints
    {
        internal const string Symbol = "KMG.Traits.HelpfulCombat";
        internal const string InternalName = "KMG_HelpfulCombat_Trait";
        internal const string DisplayName = "Helpful";
        internal const string Description =
            "When using the Aid Another action, you grant your ally a +3 bonus instead of a +2 bonus.";

        internal static BlueprintFeature Register(BlueprintRegistry registry,
            Sprite icon)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (icon == null) throw new ArgumentNullException("icon");
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                Symbol, () => Create(icon));
            Validate(feature);
            return feature;
        }

        internal static void Validate(BlueprintFeature feature)
        {
            if (feature == null || !string.Equals(feature.name, InternalName,
                    StringComparison.Ordinal) || feature.Ranks != 1 ||
                feature.HideInUI || feature.IsClassFeature || feature.Icon == null ||
                feature.Groups == null || feature.Groups.Length != 1 ||
                !feature.Groups.Contains(FeatureGroup.Trait) ||
                feature.ComponentsArray == null ||
                feature.ComponentsArray.Length != 0)
                throw new InvalidOperationException(
                    "Combat Helpful must remain a visible, mechanically inert rank-one trait until its exact optional Aid Another adapter is reconciled.");
        }

        private static BlueprintFeature Create(Sprite icon)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = InternalName;
            feature.Ranks = 1;
            feature.HideInUI = false;
            feature.IsClassFeature = false;
            feature.Groups = new[] { FeatureGroup.Trait };
            feature.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.Traits.HelpfulCombat.Name",
                    DisplayName),
                LocalizationService.Create(
                    "KMG.Traits.HelpfulCombat.Description", Description), icon);
            return feature;
        }
    }
}
