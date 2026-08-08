using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            BlueprintUnitFactAccess.Resolve().SetIconIfMissing(progression,
                fallbackIcon);
            var visited = new HashSet<BlueprintUnitFact>();
            foreach (LevelEntry entry in progression.LevelEntries)
                foreach (BlueprintFeatureBase feature in entry.Features)
                    Visit(feature, fallbackIcon, visited);

            foreach (BlueprintUnitFact fact in visited)
            {
                BlueprintAbility ability = fact as BlueprintAbility;
                if (ability == null || ability.Hidden) continue;
                CompleteTooltipMetadata(ability);
                string duration = ability.LocalizedDuration == null ? null :
                    ability.LocalizedDuration.ToString();
                string saving = ability.LocalizedSavingThrow == null ? null :
                    ability.LocalizedSavingThrow.ToString();
                if (string.IsNullOrWhiteSpace(duration) ||
                    string.IsNullOrWhiteSpace(saving) ||
                    duration.IndexOf("<null>", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    saving.IndexOf("<null>", StringComparison.OrdinalIgnoreCase) >= 0)
                    throw new InvalidOperationException(
                        "Player-facing ability tooltip metadata is incomplete: " + ability.name);
            }
        }

        internal static void ConfigureTracks(BlueprintProgression progression,
            params BlueprintFeatureBase[][] tracks)
        {
            if (progression == null) throw new ArgumentNullException("progression");
            progression.UIGroups = (tracks ?? Array.Empty<BlueprintFeatureBase[]>())
                .Where(features => features != null && features.Length > 1)
                .Select(features => new UIGroup { Features = features.ToList() })
                .ToArray();
            if (progression.UIGroups.Length == 0)
                throw new InvalidOperationException(
                    "Gunslinger progression exposed no player-facing UI groups.");
        }

        internal static void ApplyArchetypes(BlueprintCharacterClass characterClass,
            Sprite fallbackIcon)
        {
            if (characterClass == null) throw new ArgumentNullException("characterClass");
            if (fallbackIcon == null) throw new ArgumentNullException("fallbackIcon");
            var visited = new HashSet<BlueprintUnitFact>();
            foreach (BlueprintArchetype archetype in characterClass.Archetypes ??
                Array.Empty<BlueprintArchetype>())
                foreach (LevelEntry entry in archetype == null ||
                    archetype.AddFeatures == null ? Array.Empty<LevelEntry>() :
                    archetype.AddFeatures)
                    foreach (BlueprintFeatureBase feature in entry == null ||
                        entry.Features == null ? new List<BlueprintFeatureBase>() :
                        entry.Features)
                        Visit(feature, fallbackIcon, visited);
            ValidateAbilities(visited);
        }

        private static void CompleteTooltipMetadata(BlueprintAbility ability)
        {
            string description = ability.Description ?? string.Empty;
            if (ability.LocalizedDuration == null ||
                string.IsNullOrWhiteSpace(ability.LocalizedDuration.ToString()))
                ability.LocalizedDuration = LocalizationService.Create(
                    "KMG.Presentation." + ability.name + ".Duration",
                    "See description");
            if (ability.LocalizedSavingThrow == null ||
                string.IsNullOrWhiteSpace(ability.LocalizedSavingThrow.ToString()))
            {
                bool mentionsSave = description.IndexOf("saving throw",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                    description.IndexOf(" save", StringComparison.OrdinalIgnoreCase) >= 0;
                ability.LocalizedSavingThrow = LocalizationService.Create(
                    "KMG.Presentation." + ability.name + ".SavingThrow",
                    mentionsSave ? "See description" : "None");
            }
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
            foreach (BlueprintComponent component in fact.ComponentsArray)
                foreach (BlueprintUnitFact child in ReferencedFacts(component))
                    Visit(child, fallbackIcon, visited);
        }

        private static IEnumerable<BlueprintUnitFact> ReferencedFacts(
            BlueprintComponent component)
        {
            if (component == null) yield break;
            foreach (FieldInfo field in component.GetType().GetFields(
                BindingFlags.Instance | BindingFlags.Public))
            {
                object value = field.GetValue(component);
                BlueprintUnitFact fact = value as BlueprintUnitFact;
                if (fact != null) yield return fact;
                var facts = value as IEnumerable<BlueprintUnitFact>;
                if (facts != null)
                    foreach (BlueprintUnitFact child in facts)
                        if (child != null) yield return child;
            }
        }

        private static void ValidateAbilities(
            IEnumerable<BlueprintUnitFact> visited)
        {
            foreach (BlueprintAbility ability in visited.OfType<BlueprintAbility>()
                .Where(value => !value.Hidden))
            {
                CompleteTooltipMetadata(ability);
                if (string.IsNullOrWhiteSpace(ability.LocalizedDuration.ToString()) ||
                    string.IsNullOrWhiteSpace(ability.LocalizedSavingThrow.ToString()))
                    throw new InvalidOperationException(
                        "Archetype ability tooltip metadata is incomplete: " +
                        ability.name);
            }
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
