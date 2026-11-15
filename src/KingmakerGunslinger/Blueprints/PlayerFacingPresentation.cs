using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Presentation;
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
            bool projectOwned = fact.name != null &&
                fact.name.StartsWith("KMG_", StringComparison.Ordinal);
            if (projectOwned && !IsHiddenInUi(fact))
            {
                BlueprintUnitFactAccess access = BlueprintUnitFactAccess.Resolve();
                RequireAssignedText(fact, access);
                access.SetIconIfMissing(fact, fallbackIcon);
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

        /// <summary>
        /// Kingmaker's own per-type UI visibility: a feature's HideInUI, an
        /// ability's Hidden, and a buff's HiddenInUi flag. A project fact that
        /// none of these hides is shown to the player and must carry text.
        /// </summary>
        internal static bool IsHiddenInUi(BlueprintUnitFact fact)
        {
            var feature = fact as BlueprintFeature;
            var ability = fact as BlueprintAbility;
            var buff = fact as BlueprintBuff;
            return (feature != null && feature.HideInUI) ||
                (ability != null && ability.Hidden) ||
                (buff != null && buff.IsHiddenInUI);
        }

        // Text counts only when it was assigned. Kingmaker shows "<null>" for
        // a string that was never stored, so a non-empty test alone accepts a
        // fact with no name; other installations resolve the same gap to an
        // empty string and fail it.
        private static void RequireAssignedText(BlueprintUnitFact fact,
            BlueprintUnitFactAccess access)
        {
            PlayerFacingTextState name = access.GetNameState(fact);
            PlayerFacingTextState description = access.GetDescriptionState(fact);
            if (name == PlayerFacingTextState.Assigned &&
                description == PlayerFacingTextState.Assigned) return;
            throw new InvalidOperationException(
                "Player-facing fact has incomplete localization: " + fact.name +
                " (name=" + PlayerFacingTextAssignment.Describe(name) +
                ";description=" + PlayerFacingTextAssignment.Describe(description) +
                ";" + LocalizationService.DescribeEnvironment() + ")");
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
