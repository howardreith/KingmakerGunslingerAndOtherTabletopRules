using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalAlternateTraitBlueprintFactory
    {
        internal static ElementalAlternateTraitRaceBlueprints Register(
            LibraryScriptableObject library, BlueprintRegistry registry,
            ElementalHeritageRace race,
            Sprite icon)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (icon == null) throw new ArgumentNullException("icon");
            ElementalAlternateTraitDefinition[] definitions =
                ElementalAlternateTraitPolicy.ForRace(race).ToArray();
            var traits = new List<ElementalAlternateTraitBlueprints>();
            foreach (ElementalAlternateTraitDefinition definition in
                definitions)
            {
                BlueprintBuff bloodBuff = ElementalBloodBlueprintFactory.Register(
                    registry, definition, icon);
                ElementalTraitDailyAbilityBlueprints daily = ElementalEfreetiMagicFactory.Register(
                    library, registry, definition.Id);
                BlueprintFeature provider = registry.Register<BlueprintFeature>(
                    definition.ProviderSymbol,
                    () => CreateProvider(library, definition, icon, bloodBuff, daily));
                BlueprintFeature marker = registry.Register<BlueprintFeature>(
                    definition.MarkerSymbol,
                    () => CreateMarker(definition, icon));
                ElementalBloodBlueprintFactory.Bind(bloodBuff, provider, marker);
                traits.Add(new ElementalAlternateTraitBlueprints(definition,
                    marker, provider, (bloodBuff == null ?
                        Array.Empty<BlueprintScriptableObject>() :
                        new BlueprintScriptableObject[] { bloodBuff }).Concat(daily == null ?
                            Array.Empty<BlueprintScriptableObject>() : daily.Mechanics)));
            }

            foreach (ElementalAlternateTraitBlueprints trait in traits)
                AddExactExclusions(trait, traits);

            var selections = new List<
                ElementalAlternateTraitSelectionBlueprints>();
            foreach (ElementalAlternateTraitSelectionDefinition definition in
                ElementalAlternateTraitPolicy.SelectionsForRace(race))
            {
                ElementalAlternateTraitBlueprints[] choices = definition
                    .Choices.Select(choice => traits.Single(value =>
                        value.Definition.Id == choice.Id)).ToArray();
                BlueprintFeature retain = registry.Register<BlueprintFeature>(
                    definition.RetainMarkerSymbol,
                    () => CreateRetainMarker(definition, icon));
                BlueprintFeature[] entries = new[] { retain }.Concat(
                    choices.Select(value => value.Marker)).ToArray();
                BlueprintFeatureSelection selection = registry.Register<
                    BlueprintFeatureSelection>(definition.SelectionSymbol,
                        () => CreateSelection(definition, entries, icon));
                selections.Add(new ElementalAlternateTraitSelectionBlueprints(
                    definition, selection, retain, choices));
            }

            var result = new ElementalAlternateTraitRaceBlueprints(race,
                traits, selections);
            Validate(result);
            return result;
        }

        private static BlueprintFeature CreateProvider(
            LibraryScriptableObject library,
            ElementalAlternateTraitDefinition definition, Sprite icon,
            BlueprintBuff bloodBuff, ElementalTraitDailyAbilityBlueprints daily)
        {
            BlueprintFeature result = BaseFeature(definition.ProviderSymbol,
                true);
            var controller = ScriptableObject.CreateInstance<
                ElementalAlternateTraitProviderController>();
            controller.Trait = (int)definition.Id;
            result.ComponentsArray = new BlueprintComponent[] { controller }
                .Concat(ElementalAlternateTraitPassiveFactory.ComponentsFor(
                    definition.Id))
                .Concat(ElementalSummonInsightFactory.ComponentsFor(library,
                    definition.Id))
                .Concat(ElementalBloodBlueprintFactory.ComponentsFor(definition,
                    bloodBuff)).ToArray();
            if (daily != null)
                // Native resources activate before TurnOn. Restore remembered
                // expenditure before the final reconciliation callback sees it.
                result.ComponentsArray = result.ComponentsArray.Where(value =>
                    !ReferenceEquals(value, controller)).Concat(daily.ProviderComponents())
                    .Concat(new BlueprintComponent[] { controller }).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(Key(definition, "Provider.Name"),
                    definition.Name + " Provider"),
                LocalizationService.Create(Key(definition,
                    "Provider.Description"), definition.Description), icon);
            return ElementalComponentIdentity.Prepare(result);
        }

        private static BlueprintFeature CreateMarker(
            ElementalAlternateTraitDefinition definition, Sprite icon)
        {
            BlueprintFeature result = BaseFeature(definition.MarkerSymbol,
                false);
            var controller = ScriptableObject.CreateInstance<
                ElementalAlternateTraitMarkerController>();
            controller.Trait = (int)definition.Id;
            result.ComponentsArray = new BlueprintComponent[] { controller };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(Key(definition, "Marker.Name"),
                    definition.Name),
                LocalizationService.Create(Key(definition,
                    "Marker.Description"), definition.Description +
                    " Replaces: " + SlotText(definition.ReplacedSlots) +
                    "."), icon);
            return result;
        }

        private static BlueprintFeature CreateRetainMarker(
            ElementalAlternateTraitSelectionDefinition definition,
            Sprite icon)
        {
            BlueprintFeature result = BaseFeature(
                definition.RetainMarkerSymbol, false);
            var controller = ScriptableObject.CreateInstance<
                ElementalAlternateTraitRetainController>();
            controller.Race = (int)definition.Race;
            controller.Slot = (int)definition.Slot;
            result.ComponentsArray = new BlueprintComponent[] { controller };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(Key(definition, "Retain.Name"),
                    "No Additional " + SlotText(definition.Slot) +
                    " Replacement"),
                LocalizationService.Create(Key(definition,
                    "Retain.Description"), "Keep the " +
                    SlotText(definition.Slot).ToLowerInvariant() +
                    " provided by the active " + definition.Race +
                    " heritage unless another selected alternate trait " +
                    "replaces it. This choice adds no further replacement."),
                icon);
            return result;
        }

        private static BlueprintFeatureSelection CreateSelection(
            ElementalAlternateTraitSelectionDefinition definition,
            BlueprintFeature[] choices, Sprite icon)
        {
            if (choices == null || choices.Length !=
                    definition.Choices.Count + 1 ||
                choices.Any(value => value == null))
                throw new InvalidOperationException(
                    "An alternate-trait selection requires retain-base plus every primary-slot option.");
            var result = ScriptableObject.CreateInstance<
                BlueprintFeatureSelection>();
            result.name = InternalName(definition.SelectionSymbol);
            result.Ranks = 1;
            result.IsClassFeature = false;
            result.HideInUI = false;
            result.IgnorePrerequisites = false;
            result.Obligatory = true;
            result.Group = FeatureGroup.None;
            result.Group2 = FeatureGroup.None;
            result.Features = (BlueprintFeature[])choices.Clone();
            result.AllFeatures = (BlueprintFeature[])choices.Clone();
            result.ComponentsArray = new BlueprintComponent[0];
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(Key(definition, "Selection.Name"),
                    definition.Name),
                LocalizationService.Create(Key(definition,
                    "Selection.Description"), definition.Description), icon);
            return result;
        }

        private static void AddExactExclusions(
            ElementalAlternateTraitBlueprints target,
            IEnumerable<ElementalAlternateTraitBlueprints> all)
        {
            var components = new List<BlueprintComponent>(
                target.Marker.ComponentsArray ?? new BlueprintComponent[0]);
            foreach (ElementalAlternateTraitBlueprints conflict in all.Where(
                value => value.Definition.Id != target.Definition.Id &&
                    (value.Definition.ReplacedSlots &
                        target.Definition.ReplacedSlots) != 0))
            {
                var prerequisite = ScriptableObject.CreateInstance<
                    PrerequisiteNoFeature>();
                prerequisite.Feature = conflict.Marker;
                prerequisite.Group = Prerequisite.GroupType.All;
                components.Add(prerequisite);
            }
            target.Marker.ComponentsArray = components.ToArray();
        }

        private static BlueprintFeature BaseFeature(string symbol,
            bool hidden)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = InternalName(symbol);
            result.Ranks = 1;
            result.IsClassFeature = false;
            result.HideInUI = hidden;
            result.Groups = Array.Empty<FeatureGroup>();
            return result;
        }

        private static void Validate(
            ElementalAlternateTraitRaceBlueprints result)
        {
            if (result.RegisteredCount != result.Traits().Count * 2 +
                    result.Selections().Count * 2 +
                    result.Traits().Sum(value => value.Mechanics().Count) ||
                result.Traits().Any(value => value.Marker.Icon == null ||
                    value.Provider.Icon == null ||
                    value.Marker.HideInUI || !value.Provider.HideInUI ||
                    value.Marker.ComponentsArray.OfType<
                        ElementalAlternateTraitMarkerController>().Count() != 1 ||
                    value.Provider.ComponentsArray.OfType<
                        ElementalAlternateTraitProviderController>().Count() != 1) ||
                result.Selections().Any(value =>
                    !value.Selection.Obligatory ||
                    value.Selection.IgnorePrerequisites ||
                    value.Selection.Icon == null ||
                    value.RetainMarker.Icon == null ||
                    value.RetainMarker.ComponentsArray.OfType<
                        ElementalAlternateTraitRetainController>().Count() != 1))
                throw new InvalidOperationException(
                    "Elemental alternate-trait blueprint graph drifted.");
            foreach (ElementalAlternateTraitBlueprints trait in
                result.Traits())
            {
                int expected = result.Traits().Count(value =>
                    value.Definition.Id != trait.Definition.Id &&
                    (value.Definition.ReplacedSlots &
                        trait.Definition.ReplacedSlots) != 0);
                if (trait.Marker.ComponentsArray.OfType<
                        PrerequisiteNoFeature>().Count() != expected)
                    throw new InvalidOperationException(
                        trait.Definition.Name +
                        " does not carry every exact overlap exclusion.");
            }
        }

        private static string SlotText(ElementalRacialTraitSlot slots)
        {
            var names = new List<string>();
            if ((slots & ElementalRacialTraitSlot.EnergyResistance) != 0)
                names.Add("Energy Resistance");
            if ((slots & ElementalRacialTraitSlot.ElementalAffinity) != 0)
                names.Add("Elemental Affinity");
            if ((slots & ElementalRacialTraitSlot.RacialSpellLikeAbility) != 0)
                names.Add("Racial Spell-Like Ability");
            return string.Join(" and ", names);
        }

        private static string Key(ElementalAlternateTraitDefinition definition,
            string suffix)
        {
            return "KMG.ElementalRaces.Traits." + definition.Id + "." +
                suffix;
        }

        private static string Key(
            ElementalAlternateTraitSelectionDefinition definition,
            string suffix)
        {
            return "KMG.ElementalRaces.Traits." + definition.Race + "." +
                definition.Slot + "." + suffix;
        }

        private static string InternalName(string symbol)
        {
            return symbol.Replace('.', '_');
        }
    }
}
