using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalRaceBlueprintFactory
    {
        internal const int ResistanceValue = 5;
        internal const int KeenSensesPerceptionBonus = 2;

        internal static ElementalRaceBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            ElementalRaceIdentityCatalog.Validate();
            BlueprintRace aasimar = BlueprintLibraryLookup.RequireExact<
                BlueprintRace>(library,
                    ElementalRaceIdentityCatalog.AasimarRaceGuid,
                    "native Aasimar race and outsider presentation precedent");
            BlueprintFeature keen = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library,
                    ElementalRaceIdentityCatalog.KeenSensesGuid,
                    "native Keen Senses racial feature");
            BlueprintFeature slow = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library,
                    ElementalRaceIdentityCatalog.SlowAndSteadyGuid,
                    "native Dwarf Slow and Steady racial feature");
            BlueprintFeature outsider = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library,
                    ElementalRaceIdentityCatalog.OutsiderTypeGuid,
                    "native Outsider type fact");

            ValidateNativeDonors(aasimar, keen, slow, outsider);
            var result = new List<ElementalRaceBlueprints>();
            foreach (ElementalRaceDefinition definition in
                ElementalRaceCatalog.Ordered())
            {
                BlueprintAbilityResource resource =
                    ElementalRaceAbilityFactory.RegisterResource(registry,
                        definition);
                Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility ability =
                    ElementalRaceAbilityFactory.RegisterAbility(library,
                        registry, definition, resource);
                BlueprintFeature resistance = registry.Register<
                    BlueprintFeature>(definition.ResistanceSymbol,
                        () => CreateResistance(definition));
                BlueprintFeature affinity = registry.Register<BlueprintFeature>(
                    definition.AffinitySymbol,
                    () => CreateAffinity(definition));
                BlueprintFeature sla =
                    ElementalRaceAbilityFactory.RegisterFeature(registry,
                        definition, resource, ability);
                BlueprintRace race = registry.Register<BlueprintRace>(
                    definition.RaceSymbol,
                    () => CreateRace(definition, aasimar, keen, slow,
                        outsider, resistance, affinity, sla));
                var blueprints = new ElementalRaceBlueprints(definition, race,
                    resistance, affinity, sla, resource, ability);
                ValidateRace(blueprints, aasimar, keen, slow, outsider);
                result.Add(blueprints);
            }
            return new ElementalRaceBlueprintSet(result);
        }

        private static BlueprintFeature CreateResistance(
            ElementalRaceDefinition definition)
        {
            var feature = BaseFeature(definition.ResistanceSymbol);
            var resistance = ScriptableObject.CreateInstance<
                AddDamageResistanceEnergy>();
            resistance.Type = definition.Resistance;
            resistance.Value = new ContextValue
            {
                ValueType = ContextValueType.Simple,
                Value = ResistanceValue
            };
            feature.ComponentsArray = new BlueprintComponent[] { resistance };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(LocalizationKey(definition,
                    "Resistance.Name"), definition.Resistance +
                    " Resistance"),
                LocalizationService.Create(LocalizationKey(definition,
                    "Resistance.Description"), "You have " +
                    definition.Resistance.ToString().ToLowerInvariant() +
                    " resistance " + ResistanceValue + "."), null);
            return feature;
        }

        private static BlueprintFeature CreateAffinity(
            ElementalRaceDefinition definition)
        {
            var feature = BaseFeature(definition.AffinitySymbol);
            var affinity = ScriptableObject.CreateInstance<
                ElementalSpellAffinity>();
            affinity.DescriptorMask = checked((int)definition.Affinity);
            feature.ComponentsArray = new BlueprintComponent[] { affinity };
            string affinityName = definition.Kind == ElementalRaceKind.Sylph ?
                "Air Affinity" : definition.Kind == ElementalRaceKind.Undine ?
                    "Water Affinity" : definition.Affinity + " Affinity";
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(LocalizationKey(definition,
                    "Affinity.Name"), affinityName),
                LocalizationService.Create(LocalizationKey(definition,
                    "Affinity.Description"), "+1 racial bonus to spell DC " +
                    "for spells with the " + definition.Affinity +
                    " descriptor. This does not increase caster level."), null);
            return feature;
        }

        private static BlueprintRace CreateRace(
            ElementalRaceDefinition definition, BlueprintRace aasimar,
            BlueprintFeature keen, BlueprintFeature slow,
            BlueprintFeature outsider, BlueprintFeature resistance,
            BlueprintFeature affinity, BlueprintFeature sla)
        {
            BlueprintRace race = BlueprintCloneService.Clone(aasimar,
                InternalName(definition.RaceSymbol));
            race.ComponentsArray = definition.Stats.Select(value =>
            {
                var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
                bonus.Stat = value.Stat;
                bonus.Value = value.Value;
                bonus.Descriptor = Kingmaker.Enums.ModifierDescriptor.Racial;
                return (BlueprintComponent)bonus;
            }).ToArray();
            var features = new List<BlueprintFeature>
            {
                outsider, keen, resistance, affinity, sla
            };
            if (definition.SlowAndSteady) features.Insert(2, slow);
            race.Features = features.ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(race,
                LocalizationService.Create(LocalizationKey(definition,
                    "Race.Name"), definition.DisplayName),
                LocalizationService.Create(LocalizationKey(definition,
                    "Race.Description"), definition.Description),
                aasimar.Icon);
            return race;
        }

        private static BlueprintFeature BaseFeature(string symbol)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = InternalName(symbol);
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = false;
            feature.Groups = Array.Empty<FeatureGroup>();
            return feature;
        }

        private static void ValidateNativeDonors(BlueprintRace aasimar,
            BlueprintFeature keen, BlueprintFeature slow,
            BlueprintFeature outsider)
        {
            AddStatBonus perception = (keen.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<AddStatBonus>()
                .SingleOrDefault();
            if (aasimar.Presets == null || aasimar.Presets.Length < 1 ||
                aasimar.MaleOptions == null || aasimar.FemaleOptions == null ||
                perception == null ||
                perception.Stat != StatType.SkillPerception ||
                perception.Value != KeenSensesPerceptionBonus ||
                (slow.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .Length < 2 ||
                (outsider.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .Length != 0)
                throw new InvalidOperationException(
                    "Installed native race mechanic donors do not match the qualified contract.");
        }

        private static void ValidateRace(ElementalRaceBlueprints value,
            BlueprintRace aasimar, BlueprintFeature keen,
            BlueprintFeature slow, BlueprintFeature outsider)
        {
            BlueprintRace race = value.Race;
            AddStatBonus[] stats = (race.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<AddStatBonus>()
                .ToArray();
            AddDamageResistanceEnergy resistance = value.Resistance
                .ComponentsArray.OfType<AddDamageResistanceEnergy>().Single();
            if (ReferenceEquals(race, aasimar) || race.Size != aasimar.Size ||
                race.RaceId != aasimar.RaceId || stats.Length != 3 ||
                !race.Features.Contains(outsider) ||
                !race.Features.Contains(keen) ||
                !race.Features.Contains(value.Resistance) ||
                !race.Features.Contains(value.Affinity) ||
                !race.Features.Contains(value.SlaFeature) ||
                race.Features.Contains(slow) != value.Definition.SlowAndSteady ||
                resistance.Type != value.Definition.Resistance ||
                value.Affinity.ComponentsArray.OfType<
                    ElementalSpellAffinity>().Single().DescriptorMask !=
                        checked((int)value.Definition.Affinity) ||
                race.Presets == null || race.Presets.Length < 1 ||
                race.MaleOptions == null || race.FemaleOptions == null)
                throw new InvalidOperationException(value.Definition.DisplayName +
                    " race blueprint failed deterministic validation.");
        }

        private static string LocalizationKey(
            ElementalRaceDefinition definition, string suffix)
        {
            return "KMG.ElementalRaces." + definition.Kind + "." + suffix;
        }

        private static string InternalName(string symbol)
        {
            return symbol.Replace('.', '_');
        }
    }
}
