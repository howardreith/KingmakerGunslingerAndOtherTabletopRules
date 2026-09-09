using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
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
                    library, registry, definition.Id) ?? ElementalBreathFactory.Register(
                        library, registry, definition.Id) ?? ElementalNereidFactory.Register(
                            library, registry, definition.Id, icon) ?? ElementalTreacherousFactory.Register(
                                registry, definition.Id, icon);
                ElementalCrystallineFormBlueprints crystalline = ElementalCrystallineFormFactory.Register(
                    registry, definition.Id, icon);
                ElementalBreezeKissedBlueprints breeze = ElementalBreezeKissedFactory.Register(
                    registry, definition.Id, icon);
                BlueprintFeature provider = registry.Register<BlueprintFeature>(
                    definition.ProviderSymbol,
                    () => CreateProvider(library, definition, icon, bloodBuff, daily, crystalline, breeze));
                BlueprintFeature marker = registry.Register<BlueprintFeature>(
                    definition.MarkerSymbol,
                    () => CreateMarker(definition, icon));
                ElementalBloodBlueprintFactory.Bind(bloodBuff, provider, marker);
                ElementalNereidFactory.Bind(daily, marker);
                ElementalTreacherousFactory.Bind(daily, marker);
                traits.Add(new ElementalAlternateTraitBlueprints(definition,
                    marker, provider, (bloodBuff == null ?
                        Array.Empty<BlueprintScriptableObject>() :
                        new BlueprintScriptableObject[] { bloodBuff }).Concat(daily == null ?
                            Array.Empty<BlueprintScriptableObject>() : daily.Mechanics).Concat(crystalline == null ?
                                Array.Empty<BlueprintScriptableObject>() : crystalline.Mechanics).Concat(breeze == null ?
                                    Array.Empty<BlueprintScriptableObject>() : breeze.Mechanics)));
            }

            foreach (ElementalAlternateTraitBlueprints trait in traits)
                AddExactExclusions(trait, traits);

            var selections = new List<
                ElementalAlternateTraitSelectionBlueprints>();
            foreach (ElementalAlternateTraitSelectionDefinition definition in
                ElementalAlternateTraitPolicy.SelectionsForRace(race))
            {
                ElementalAlternateTraitBlueprints[] choices = definition
                    .PublishedChoices.Select(choice => traits.Single(value =>
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
            BlueprintBuff bloodBuff, ElementalTraitDailyAbilityBlueprints daily,
            ElementalCrystallineFormBlueprints crystalline, ElementalBreezeKissedBlueprints breeze)
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
            if (daily != null || crystalline != null || breeze != null)
                // Native resources activate before TurnOn. Restore remembered
                // expenditure before the final reconciliation callback sees it.
                result.ComponentsArray = result.ComponentsArray.Where(value =>
                    !ReferenceEquals(value, controller)).Concat(daily == null ?
                        Array.Empty<BlueprintComponent>() : daily.ProviderComponents())
                    .Concat(crystalline == null ? Array.Empty<BlueprintComponent>() : crystalline.ProviderComponents())
                    .Concat(breeze == null ? Array.Empty<BlueprintComponent>() : breeze.ProviderComponents())
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
                !definition.IsPublished);
            result.HideInCharacterSheetAndLevelUp = !definition.IsPublished;
            var controller = ScriptableObject.CreateInstance<
                ElementalAlternateTraitMarkerController>();
            controller.Trait = (int)definition.Id;
            result.ComponentsArray = new BlueprintComponent[] { controller };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(Key(definition, "Marker.Name"),
                    definition.Name),
                LocalizationService.Create(Key(definition,
                    "Marker.Description"), definition.IsPublished ? definition.Description +
                    " Replaces: " + SlotText(definition.ReplacedSlots) + "." :
                    "Deferred. This retained development identity is unavailable for selection and does not replace a racial trait."), icon);
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
                    definition.PublishedChoices.Count + 1 ||
                choices.Any(value => value == null))
                throw new InvalidOperationException(
                    "An alternate-trait selection requires retain-base plus every published primary-slot option.");
            var result = ScriptableObject.CreateInstance<
                BlueprintFeatureSelection>();
            result.name = InternalName(definition.SelectionSymbol);
            result.Ranks = 1;
            result.IsClassFeature = false;
            result.HideInUI = false;
            result.IgnorePrerequisites = false;
            result.Obligatory = true;
            // Installed Races Unleashed alternate-racial-trait contract.
            result.Group = FeatureGroup.AasimarHeritage;
            result.Group2 = FeatureGroup.None;
            result.Groups = new[] { FeatureGroup.AasimarHeritage };
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
                value => target.Definition.IsPublished && value.Definition.IsPublished &&
                    value.Definition.Id != target.Definition.Id &&
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
                    value.Marker.HideInUI == value.Definition.IsPublished || !value.Provider.HideInUI ||
                    value.Marker.HideInCharacterSheetAndLevelUp == value.Definition.IsPublished ||
                    (value.Definition.IsPublished && !HasTraitSpecificMechanic(value)) ||
                    value.Marker.ComponentsArray.OfType<
                        ElementalAlternateTraitMarkerController>().Count() != 1 ||
                    value.Provider.ComponentsArray.OfType<
                        ElementalAlternateTraitProviderController>().Count() != 1) ||
                result.Selections().Any(value =>
                    value.Selection.Group != FeatureGroup.AasimarHeritage ||
                    value.Selection.Group2 != FeatureGroup.None ||
                    value.Selection.Groups == null ||
                    !value.Selection.Groups.SequenceEqual(new[] { FeatureGroup.AasimarHeritage }) ||
                    value.Selection.IsClassFeature || value.Selection.HideInUI ||
                    value.Selection.HideInCharacterSheetAndLevelUp ||
                    value.RetainMarker.ComponentsArray.OfType<Prerequisite>().Any() ||
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
                    trait.Definition.IsPublished && value.Definition.IsPublished &&
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

        internal static bool HasTraitSpecificMechanic(ElementalAlternateTraitBlueprints trait)
        {
            var components = trait.Provider.ComponentsArray ?? new BlueprintComponent[0];
            Func<StatType, int, ModifierDescriptor, bool> stat = (type, amount, descriptor) =>
                components.OfType<AddStatBonus>().Any(value => value.Stat == type &&
                    value.Value == amount && value.Descriptor == descriptor);
            switch (trait.Definition.Id)
            {
                case ElementalAlternateTraitId.WildfireHeart:
                    return stat(StatType.Initiative, 4, ModifierDescriptor.Racial);
                case ElementalAlternateTraitId.GraniteSkin:
                    return stat(StatType.AC, 1, ModifierDescriptor.NaturalArmor);
                case ElementalAlternateTraitId.LikeTheWind:
                    return stat(StatType.Speed, 5, ModifierDescriptor.Racial);
                case ElementalAlternateTraitId.WhisperingWind:
                    return stat(StatType.SkillStealth, 4, ModifierDescriptor.Racial);
                case ElementalAlternateTraitId.ThunderousResilience:
                    return components.OfType<AddDamageResistanceEnergy>().Any(value =>
                        value.Type == DamageEnergyType.Sonic && value.Value.Value == 5);
                case ElementalAlternateTraitId.BrazenFlame:
                    return components.OfType<ElementalBrazenFlameDamage>().Any();
                case ElementalAlternateTraitId.ForgeHardened:
                case ElementalAlternateTraitId.Secretive:
                    return components.OfType<ElementalAlternateTraitSaveBonus>().Any(value =>
                        value.Trait == (int)trait.Definition.Id);
                case ElementalAlternateTraitId.FireInsight:
                case ElementalAlternateTraitId.EarthInsight:
                case ElementalAlternateTraitId.AirInsight:
                    return components.OfType<ElementalSummonInsight>().Any(value =>
                        value.Trait == (int)trait.Definition.Id);
                case ElementalAlternateTraitId.FireInTheBlood:
                case ElementalAlternateTraitId.StoneInTheBlood:
                case ElementalAlternateTraitId.StormInTheBlood:
                    return components.OfType<ElementalBloodDamageTrigger>().Any(value =>
                        value.Trait == (int)trait.Definition.Id);
                case ElementalAlternateTraitId.TreacherousEarth:
                    return ElementalTreacherousPolicy.EligibilityQualified && ElementalTreacherousFactory.IsExactEffect(trait);
                case ElementalAlternateTraitId.NereidFascination:
                    return ElementalNereidFactory.IsExact(trait);
                case ElementalAlternateTraitId.EfreetiMagic:
                case ElementalAlternateTraitId.AcidBreath:
                case ElementalAlternateTraitId.OozeBreath:
                    return components.OfType<AddFacts>().Any(value => value.Facts != null &&
                        value.Facts.OfType<BlueprintAbility>().Any(ability =>
                            trait.Mechanics().Any(owned => ReferenceEquals(owned, ability)) &&
                            ability.ComponentsArray != null && ability.ComponentsArray.Length > 0)) &&
                        components.OfType<AddAbilityResources>().Any(value =>
                            trait.Mechanics().Any(owned => ReferenceEquals(owned, value.Resource)));
                case ElementalAlternateTraitId.CrystallineForm:
                    return components.OfType<ElementalCrystallineRayArmorClass>().Any() &&
                        components.OfType<ElementalCrystallineRayDeflection>().Any();
                case ElementalAlternateTraitId.BreezeKissed:
                    return components.OfType<ElementalBreezeKissedArmorClass>().Any() &&
                        components.OfType<AddFacts>().Any(value => value.Facts != null &&
                            value.Facts.OfType<BlueprintAbility>().Count() == 3);
                default:
                    return false;
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
