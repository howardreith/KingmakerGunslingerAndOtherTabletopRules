using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalHeritageBlueprintFactory
    {
        internal static ElementalHeritageRaceBlueprints Register(
            LibraryScriptableObject library, BlueprintRegistry registry,
            ElementalRaceDefinition raceDefinition,
            BlueprintFeature legacyAffinity,
            BlueprintFeature legacySlaFeature,
            BlueprintAbilityResource legacySlaResource,
            BlueprintAbility legacySlaAbility)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            if (raceDefinition == null)
                throw new ArgumentNullException("raceDefinition");
            if (legacyAffinity == null)
                throw new ArgumentNullException("legacyAffinity");
            if (legacySlaFeature == null)
                throw new ArgumentNullException("legacySlaFeature");
            if (legacySlaResource == null)
                throw new ArgumentNullException("legacySlaResource");
            if (legacySlaAbility == null)
                throw new ArgumentNullException("legacySlaAbility");
            if (legacySlaAbility.Icon == null)
                throw new InvalidOperationException(
                    "The legacy racial SLA requires a non-null icon before heritage registration.");

            ElementalHeritageRace race = ToHeritageRace(raceDefinition.Kind);
            var choices = new List<ElementalHeritageBlueprints>();
            foreach (ElementalHeritageDefinition definition in
                ElementalHeritagePolicy.ForRace(race))
            {
                if (definition.IsGeneral)
                {
                    ValidateGeneralIdentity(definition, raceDefinition);
                    BlueprintFeature marker = registry.Register<
                        BlueprintFeature>(definition.MarkerSymbol,
                            () => CreateMarker(definition,
                                legacySlaAbility.Icon));
                    choices.Add(new ElementalHeritageBlueprints(definition,
                        marker, legacyAffinity, legacySlaFeature,
                        legacySlaResource, legacySlaAbility, null));
                    continue;
                }

                ElementalHeritageSlaBlueprints sla =
                    ElementalHeritageAbilityFactory.Register(library,
                        registry, definition, legacySlaAbility.Icon);
                BlueprintFeature affinity = registry.Register<
                    BlueprintFeature>(definition.AffinityFeatureSymbol,
                        () => CreateAffinity(definition, sla.Ability.Icon));
                BlueprintFeature alternateMarker = registry.Register<
                    BlueprintFeature>(definition.MarkerSymbol,
                        () => CreateMarker(definition, sla.Ability.Icon));
                choices.Add(new ElementalHeritageBlueprints(definition,
                    alternateMarker, affinity, sla.Feature, sla.Resource,
                    sla.Ability, sla.AuxiliaryBlueprints));
            }

            BlueprintFeature[] markers = choices.Select(value => value.Marker)
                .ToArray();
            string selectionSymbol = choices.Select(value =>
                    value.Definition.SelectionSymbol)
                .Distinct(StringComparer.Ordinal).Single();
            BlueprintFeatureSelection selection = registry.Register<
                BlueprintFeatureSelection>(selectionSymbol,
                    () => CreateSelection(race, markers,
                        legacySlaAbility.Icon));
            var result = new ElementalHeritageRaceBlueprints(race, selection,
                choices);
            Validate(result, legacyAffinity, legacySlaFeature,
                legacySlaResource, legacySlaAbility);
            return result;
        }

        private static BlueprintFeature CreateMarker(
            ElementalHeritageDefinition definition,
            UnityEngine.Sprite icon)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = InternalName(definition.MarkerSymbol);
            result.Ranks = 1;
            result.IsClassFeature = false;
            result.HideInUI = false;
            result.Groups = Array.Empty<FeatureGroup>();
            var components = new List<BlueprintComponent>();
            foreach (ElementalHeritageStatModifier delta in
                ElementalHeritagePolicy.NetDeltas(definition))
            {
                var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
                bonus.Stat = ToStatType(delta.Stat);
                bonus.Value = delta.Value;
                bonus.Descriptor = ModifierDescriptor.Racial;
                components.Add(bonus);
            }
            var controller = ScriptableObject.CreateInstance<
                ElementalHeritageMarkerController>();
            controller.Heritage = (int)definition.Id;
            components.Add(controller);
            result.ComponentsArray = components.ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(LocalizationKey(definition,
                    "Marker.Name"), definition.Name),
                LocalizationService.Create(LocalizationKey(definition,
                    "Marker.Description"), definition.Description), icon);
            return ElementalComponentIdentity.Prepare(result);
        }

        private static BlueprintFeature CreateAffinity(
            ElementalHeritageDefinition definition,
            UnityEngine.Sprite icon)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = InternalName(definition.AffinityFeatureSymbol);
            result.Ranks = 1;
            result.IsClassFeature = false;
            result.HideInUI = false;
            result.Groups = Array.Empty<FeatureGroup>();
            var affinity = ScriptableObject.CreateInstance<
                ElementalSpellAffinity>();
            affinity.DescriptorMask = checked((int)ToDescriptor(
                definition.Affinity));
            result.ComponentsArray = new BlueprintComponent[]
            {
                affinity,
                ScriptableObject.CreateInstance<
                    ElementalOwnedProviderController>()
            };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(LocalizationKey(definition,
                    "Affinity.Name"), definition.AffinityName),
                LocalizationService.Create(LocalizationKey(definition,
                    "Affinity.Description"),
                    definition.AffinityDescription), icon);
            return ElementalComponentIdentity.Prepare(result);
        }

        private static BlueprintFeatureSelection CreateSelection(
            ElementalHeritageRace race, BlueprintFeature[] choices,
            UnityEngine.Sprite icon)
        {
            if (choices == null || choices.Length !=
                    ElementalHeritagePolicy.ChoicesPerRace ||
                choices.Any(value => value == null) || icon == null)
                throw new InvalidOperationException(
                    "An elemental heritage selection requires exactly three complete choices.");
            var result = ScriptableObject.CreateInstance<
                BlueprintFeatureSelection>();
            result.name = "KMG_ElementalRaces_" + race +
                "_HeritageSelection";
            result.Ranks = 1;
            result.IsClassFeature = false;
            result.HideInUI = false;
            result.IgnorePrerequisites = false;
            result.Obligatory = true;
            result.Group = FeatureGroup.None;
            result.Group2 = FeatureGroup.None;
            result.Features = (BlueprintFeature[])choices.Clone();
            result.AllFeatures = (BlueprintFeature[])choices.Clone();
            result.ComponentsArray = new BlueprintComponent[]
            {
                ScriptableObject.CreateInstance<
                    ElementalHeritageSelectionController>()
            };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(
                    "KMG.ElementalRaces." + race +
                    ".HeritageSelection.Name", race + " Heritage"),
                LocalizationService.Create(
                    "KMG.ElementalRaces." + race +
                    ".HeritageSelection.Description",
                    "Choose exactly one " + race +
                    " heritage. Existing pre-0.0.115 characters without a marker retain General heritage behavior."),
                icon);
            return result;
        }

        private static void Validate(ElementalHeritageRaceBlueprints result,
            BlueprintFeature legacyAffinity,
            BlueprintFeature legacySlaFeature,
            BlueprintAbilityResource legacySlaResource,
            BlueprintAbility legacySlaAbility)
        {
            ElementalHeritageBlueprints[] choices = result.Choices().ToArray();
            ElementalHeritageBlueprints general = result.General;
            if (!result.Selection.Obligatory ||
                result.Selection.IgnorePrerequisites ||
                result.Selection.Features == null ||
                !result.Selection.Features.SequenceEqual(
                    result.Selection.AllFeatures) ||
                result.Selection.Icon == null ||
                !ReferenceEquals(general.Affinity, legacyAffinity) ||
                !ReferenceEquals(general.SlaFeature, legacySlaFeature) ||
                !ReferenceEquals(general.SlaResource, legacySlaResource) ||
                !ReferenceEquals(general.SlaAbility, legacySlaAbility) ||
                result.Selection.ComponentsArray.OfType<
                    ElementalHeritageSelectionController>().Count() != 1)
                throw new InvalidOperationException(
                    result.Race + " heritage selection graph drifted.");
            foreach (ElementalHeritageBlueprints choice in choices)
            {
                ElementalHeritageStatModifier[] expected =
                    ElementalHeritagePolicy.NetDeltas(choice.Definition)
                    .ToArray();
                AddStatBonus[] actual = choice.Marker.ComponentsArray.OfType<
                    AddStatBonus>().ToArray();
                ElementalHeritageMarkerController controller = choice.Marker
                    .ComponentsArray.OfType<
                        ElementalHeritageMarkerController>().Single();
                if (choice.Marker.Icon == null ||
                    controller.Heritage != (int)choice.Definition.Id ||
                    actual.Length != expected.Length ||
                    actual.Any(value => value.Descriptor !=
                        ModifierDescriptor.Racial) ||
                    expected.Any(value => !actual.Any(component =>
                        component.Stat == ToStatType(value.Stat) &&
                        component.Value == value.Value)))
                    throw new InvalidOperationException(choice.Definition.Name +
                        " marker stat overlay drifted.");
                if (!choice.Definition.IsGeneral)
                {
                    ElementalSpellAffinity affinity = choice.Affinity
                        .ComponentsArray.OfType<
                            ElementalSpellAffinity>().Single();
                    if (affinity.DescriptorMask != checked((int)ToDescriptor(
                            choice.Definition.Affinity)) ||
                        choice.Affinity.Icon == null ||
                        choice.SlaAbility.Icon == null)
                        throw new InvalidOperationException(
                            choice.Definition.Name +
                            " active provider graph drifted.");
                }
            }
        }

        private static void ValidateGeneralIdentity(
            ElementalHeritageDefinition definition,
            ElementalRaceDefinition race)
        {
            if (!definition.IsGeneral ||
                !string.Equals(definition.AffinityFeatureSymbol,
                    race.AffinitySymbol, StringComparison.Ordinal) ||
                !string.Equals(definition.SlaFeatureSymbol,
                    race.SlaFeatureSymbol, StringComparison.Ordinal) ||
                !string.Equals(definition.SlaResourceSymbol,
                    race.SlaResourceSymbol, StringComparison.Ordinal) ||
                !string.Equals(definition.SlaAbilitySymbol,
                    race.SlaAbilitySymbol, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "General heritage must retain every 0.0.114 provider identity.");
        }

        private static ElementalHeritageRace ToHeritageRace(
            ElementalRaceKind race)
        {
            return (ElementalHeritageRace)(int)race;
        }

        private static StatType ToStatType(ElementalHeritageStat stat)
        {
            switch (stat)
            {
                case ElementalHeritageStat.Strength: return StatType.Strength;
                case ElementalHeritageStat.Dexterity: return StatType.Dexterity;
                case ElementalHeritageStat.Constitution:
                    return StatType.Constitution;
                case ElementalHeritageStat.Intelligence:
                    return StatType.Intelligence;
                case ElementalHeritageStat.Wisdom: return StatType.Wisdom;
                case ElementalHeritageStat.Charisma: return StatType.Charisma;
                default: throw new ArgumentOutOfRangeException("stat");
            }
        }

        private static SpellDescriptor ToDescriptor(
            ElementalHeritageAffinity affinity)
        {
            switch (affinity)
            {
                case ElementalHeritageAffinity.Fire:
                    return SpellDescriptor.Fire;
                case ElementalHeritageAffinity.Acid:
                    return SpellDescriptor.Acid;
                case ElementalHeritageAffinity.Electricity:
                    return SpellDescriptor.Electricity;
                case ElementalHeritageAffinity.Cold:
                    return SpellDescriptor.Cold;
                default: throw new ArgumentOutOfRangeException("affinity");
            }
        }

        private static string LocalizationKey(
            ElementalHeritageDefinition definition, string suffix)
        {
            return "KMG.ElementalRaces.Heritage." + definition.Id + "." +
                suffix;
        }

        private static string InternalName(string symbol)
        {
            return symbol.Replace('.', '_');
        }
    }
}
