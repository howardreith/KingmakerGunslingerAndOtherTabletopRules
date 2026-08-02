using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class PlayerFacingPresentation
    {
        internal static void Apply(BlueprintProgression progression,
            Sprite fallbackIcon)
        {
            if (progression == null) throw new ArgumentNullException("progression");
            if (fallbackIcon == null) throw new ArgumentNullException("fallbackIcon");
            var visited = new HashSet<BlueprintUnitFact>();
            foreach (LevelEntry entry in progression.LevelEntries)
                foreach (BlueprintFeatureBase feature in entry.Features)
                    Visit(feature, fallbackIcon, visited);

            progression.UIGroups = progression.LevelEntries
                .Select(entry => entry.Features.Where(IsVisibleProjectFeature)
                    .Distinct().ToArray())
                .Where(features => features.Length > 1)
                .Select(features => new UIGroup { Features = features.ToList() })
                .ToArray();
            if (progression.UIGroups.Length == 0)
                throw new InvalidOperationException(
                    "Gunslinger progression exposed no player-facing UI groups.");
        }

        private static void Visit(BlueprintUnitFact fact, Sprite fallbackIcon,
            HashSet<BlueprintUnitFact> visited)
        {
            if (fact == null || !visited.Add(fact)) return;
            var feature = fact as BlueprintFeature;
            var ability = fact as BlueprintAbility;
            bool hidden = (feature != null && feature.HideInUI) ||
                (ability != null && ability.Hidden);
            bool projectOwned = fact.name != null &&
                fact.name.StartsWith("KMG_", StringComparison.Ordinal);
            if (projectOwned && !hidden)
            {
                if (string.IsNullOrWhiteSpace(fact.Name) ||
                    string.IsNullOrWhiteSpace(fact.Description))
                    throw new InvalidOperationException(
                        "Player-facing fact has incomplete localization: " + fact.name);
                BlueprintUnitFactAccess.Resolve().SetIconIfMissing(fact,
                    fallbackIcon);
            }
            var selection = fact as BlueprintFeatureSelection;
            if (selection != null && selection.AllFeatures != null)
                foreach (BlueprintFeature child in selection.AllFeatures)
                    Visit(child, fallbackIcon, visited);
            if (fact.ComponentsArray == null) return;
            foreach (AddFacts grant in fact.ComponentsArray.OfType<AddFacts>())
                if (grant.Facts != null)
                    foreach (BlueprintUnitFact child in grant.Facts)
                        Visit(child, fallbackIcon, visited);
        }

        private static bool IsVisibleProjectFeature(BlueprintFeatureBase feature)
        {
            var concrete = feature as BlueprintFeature;
            return concrete != null && !concrete.HideInUI &&
                concrete.name != null && concrete.name.StartsWith("KMG_",
                    StringComparison.Ordinal);
        }
    }
}
