using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces.Visuals;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalRaceBlueprintFactory
    {
        internal const int ResistanceValue = 5;
        internal const int KeenSensesPerceptionBonus = 2;

        internal static ElementalRaceBlueprintSet Register(
            LibraryScriptableObject library, BlueprintManifest manifest,
            BlueprintRegistry registry, ModLogger logger)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (manifest == null) throw new ArgumentNullException("manifest");
            if (registry == null) throw new ArgumentNullException("registry");
            if (logger == null) throw new ArgumentNullException("logger");
            ElementalRaceIdentityCatalog.Validate();
            BlueprintRace aasimar = BlueprintLibraryLookup.RequireExact<
                BlueprintRace>(library,
                    ElementalRaceIdentityCatalog.AasimarRaceGuid,
                    "native Aasimar race and presentation precedent");
            BlueprintRace tiefling = BlueprintLibraryLookup.RequireExact<
                BlueprintRace>(library,
                    ElementalRaceIdentityCatalog.TieflingRaceGuid,
                    "native Tiefling race-type behavior precedent");
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

            ValidateNativeDonors(aasimar, tiefling, keen, slow, outsider);
            ElementalRaceVisualSet visuals = ElementalRaceVisualFactory.Register(
                library, manifest, registry, logger, aasimar);
            try
            {
                var result = new List<ElementalRaceBlueprints>();
                foreach (ElementalRaceDefinition definition in
                    ElementalRaceCatalog.Ordered())
                {
                    ElementalRaceVisualBlueprints raceVisuals = visuals.Require(
                        definition.Kind);
                    BlueprintAbilityResource resource =
                        ElementalRaceAbilityFactory.RegisterResource(registry,
                            definition);
                    Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility ability =
                        ElementalRaceAbilityFactory.RegisterAbility(library,
                            registry, definition, resource, aasimar.Icon);
                    BlueprintFeature resistance = registry.Register<
                        BlueprintFeature>(definition.ResistanceSymbol,
                            () => CreateResistance(definition));
                    BlueprintFeature affinity = registry.Register<BlueprintFeature>(
                        definition.AffinitySymbol,
                        () => CreateAffinity(definition));
                    BlueprintFeature sla =
                        ElementalRaceAbilityFactory.RegisterFeature(registry,
                            definition, resource, ability);
                    ElementalHeritageRaceBlueprints heritages =
                        ElementalHeritageBlueprintFactory.Register(library,
                            registry, definition, affinity, sla, resource,
                            ability);
                    ElementalAlternateTraitRaceBlueprints alternateTraits =
                        ElementalAlternateTraitBlueprintFactory.Register(
                            library, registry, ToHeritageRace(definition.Kind),
                            ability.Icon);
                    BlueprintRace race = registry.Register<BlueprintRace>(
                        definition.RaceSymbol,
                        () => CreateRace(definition, aasimar, keen, slow,
                            resistance, affinity, sla,
                            heritages.Selection, alternateTraits, raceVisuals));
                    var blueprints = new ElementalRaceBlueprints(definition, race,
                        resistance, affinity, sla, resource, ability,
                        raceVisuals, heritages, alternateTraits);
                    ValidateRace(blueprints, aasimar, keen, slow, outsider);
                    result.Add(blueprints);
                }
                var set = new ElementalRaceBlueprintSet(result, visuals);
                ElementalHeritageRuntime.Configure(set);
                return set;
            }
            catch
            {
                visuals.RollbackResources();
                throw;
            }
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
            feature.ComponentsArray = new BlueprintComponent[]
            {
                resistance,
                ScriptableObject.CreateInstance<
                    ElementalOwnedProviderController>()
            };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(LocalizationKey(definition,
                    "Resistance.Name"), definition.Resistance +
                    " Resistance"),
                LocalizationService.Create(LocalizationKey(definition,
                    "Resistance.Description"), "You have " +
                    definition.Resistance.ToString().ToLowerInvariant() +
                    " resistance " + ResistanceValue + "."), null);
            return ElementalComponentIdentity.Prepare(feature);
        }

        private static BlueprintFeature CreateAffinity(
            ElementalRaceDefinition definition)
        {
            var feature = BaseFeature(definition.AffinitySymbol);
            var affinity = ScriptableObject.CreateInstance<
                ElementalSpellAffinity>();
            affinity.DescriptorMask = checked((int)definition.Affinity);
            feature.ComponentsArray = new BlueprintComponent[]
            {
                affinity,
                ScriptableObject.CreateInstance<
                    ElementalOwnedProviderController>()
            };
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
            return ElementalComponentIdentity.Prepare(feature);
        }

        private static BlueprintRace CreateRace(
            ElementalRaceDefinition definition, BlueprintRace aasimar,
            BlueprintFeature keen, BlueprintFeature slow,
            BlueprintFeature resistance,
            BlueprintFeature affinity, BlueprintFeature sla,
            BlueprintFeatureSelection heritageSelection,
            ElementalAlternateTraitRaceBlueprints alternateTraits,
            ElementalRaceVisualBlueprints visuals)
        {
            BlueprintRace race = BlueprintCloneService.Clone(aasimar,
                InternalName(definition.RaceSymbol));
            var components = definition.Stats.Select(value =>
            {
                var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
                bonus.Stat = value.Stat;
                bonus.Value = value.Value;
                bonus.Descriptor = Kingmaker.Enums.ModifierDescriptor.Racial;
                return (BlueprintComponent)bonus;
            }).ToList();
            var heritageController = ScriptableObject.CreateInstance<
                ElementalHeritageRaceController>();
            heritageController.Race = (int)definition.Kind;
            components.Add(heritageController);
            race.ComponentsArray = components.ToArray();
            var features = new List<BlueprintFeature>
            {
                keen, resistance, affinity, sla, heritageSelection
            };
            features.AddRange(alternateTraits.Selections().Select(value =>
                (BlueprintFeature)value.Selection));
            if (definition.SlowAndSteady) features.Insert(1, slow);
            race.Features = features.ToArray();
            race.Presets = visuals.Presets;
            race.MaleOptions = visuals.MaleOptions;
            race.FemaleOptions = visuals.FemaleOptions;
            BlueprintUnitFactAccess.Resolve().Configure(race,
                LocalizationService.Create(LocalizationKey(definition,
                    "Race.Name"), definition.DisplayName),
                LocalizationService.Create(LocalizationKey(definition,
                    "Race.Description"), definition.Description),
                aasimar.Icon);
            return ElementalComponentIdentity.Prepare(race);
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
            BlueprintRace tiefling, BlueprintFeature keen, BlueprintFeature slow,
            BlueprintFeature outsider)
        {
            AddStatBonus perception = (keen.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<AddStatBonus>()
                .SingleOrDefault();
            if (aasimar.Presets == null || aasimar.Presets.Length < 1 ||
                aasimar.MaleOptions == null || aasimar.FemaleOptions == null ||
                tiefling == null ||
                (aasimar.Features ?? Array.Empty<BlueprintFeature>())
                    .Contains(outsider) ||
                (tiefling.Features ?? Array.Empty<BlueprintFeature>())
                    .Contains(outsider) ||
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
            ElementalHeritageRaceController heritageController =
                (race.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<ElementalHeritageRaceController>().SingleOrDefault();
            AddDamageResistanceEnergy resistance = value.Resistance
                .ComponentsArray.OfType<AddDamageResistanceEnergy>().Single();
            if (ReferenceEquals(race, aasimar) || race.Size != aasimar.Size ||
                race.RaceId != aasimar.RaceId || stats.Length != 3 ||
                race.Features.Contains(outsider) ||
                !race.Features.Contains(keen) ||
                !race.Features.Contains(value.Resistance) ||
                !race.Features.Contains(value.Affinity) ||
                !race.Features.Contains(value.SlaFeature) ||
                !race.Features.Contains(value.Heritages.Selection) ||
                value.AlternateTraits.Selections().Any(selection =>
                    !race.Features.Contains(selection.Selection)) ||
                race.Features.Contains(slow) != value.Definition.SlowAndSteady ||
                heritageController == null ||
                heritageController.Race != (int)value.Definition.Kind ||
                resistance.Type != value.Definition.Resistance ||
                value.Affinity.ComponentsArray.OfType<
                    ElementalSpellAffinity>().Single().DescriptorMask !=
                        checked((int)value.Definition.Affinity) ||
                race.Presets == null || race.Presets.Length != 3 ||
                !race.Presets.SequenceEqual(value.Visuals.Presets) ||
                !ReferenceEquals(race.MaleOptions,
                    value.Visuals.MaleOptions) ||
                !ReferenceEquals(race.FemaleOptions,
                    value.Visuals.FemaleOptions))
                throw new InvalidOperationException(value.Definition.DisplayName +
                    " race blueprint failed deterministic validation.");
        }

        private static ElementalHeritageRace ToHeritageRace(
            ElementalRaceKind race)
        {
            return (ElementalHeritageRace)(int)race;
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
