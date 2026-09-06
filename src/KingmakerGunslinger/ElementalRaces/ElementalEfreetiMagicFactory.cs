using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal sealed class ElementalTraitDailyAbilityBlueprints
    {
        internal BlueprintAbilityResource Resource;
        internal BlueprintAbility Ability;
        internal BlueprintScriptableObject[] Mechanics;
        internal BlueprintComponent ParameterComponent;

        internal BlueprintComponent[] ProviderComponents()
        {
            var facts = ScriptableObject.CreateInstance<AddFacts>();
            facts.Facts = new BlueprintUnitFact[] { Ability };
            facts.DoNotRestoreMissingFacts = false;
            var add = ScriptableObject.CreateInstance<AddAbilityResources>();
            add.Resource = Resource;
            add.UseThisAsResource = false;
            add.Amount = 0;
            add.RestoreAmount = true;
            add.RestoreOnLevelUp = false;
            var memory = ScriptableObject.CreateInstance<ElementalTraitDailyResourceState>();
            memory.Resource = Resource;
            BlueprintComponent parameters = ParameterComponent;
            if (parameters == null)
            {
                var racial = ScriptableObject.CreateInstance<ElementalRacialSpellLikeParameters>();
                racial.Ability = Ability;
                racial.Stat = StatType.Charisma;
                racial.SpellLevel = 1;
                parameters = racial;
            }
            return new BlueprintComponent[] { facts, add, memory, parameters };
        }
    }

    internal static class ElementalEfreetiMagicFactory
    {
        internal const string Prefix = "KMG.ElementalRaces.Traits.Ifrit.EfreetiMagic";
        internal const string EnlargeDonorGuid = "c60969e7f264e6d4b84a1499fdcf9039";
        internal const string ReduceDonorGuid = "4e0e9aba6447d514f88eff1464cc4763";
        private const string Description = "Once per ordinary rest, choose Enlarge Person or Reduce Person. " +
            "These spell-like abilities share one use, use your total character level as caster level " +
            "and Charisma for saving throw DCs, and require no material components or arcane spell failure. " +
            "Native person targeting includes this project's Ifrit and its heritages; no other spell's targeting is changed.";

        internal static ElementalTraitDailyAbilityBlueprints Register(
            LibraryScriptableObject library, BlueprintRegistry registry, ElementalAlternateTraitId trait)
        {
            if (trait != ElementalAlternateTraitId.EfreetiMagic) return null;
            BlueprintAbility enlarge = Donor(library, EnlargeDonorGuid);
            BlueprintAbility reduce = Donor(library, ReduceDonorGuid);
            BlueprintAbilityResource resource = registry.Register<BlueprintAbilityResource>(
                Prefix + ".Resource", () =>
                {
                    var value = ScriptableObject.CreateInstance<BlueprintAbilityResource>();
                    value.name = (Prefix + ".Resource").Replace('.', '_');
                    value.LocalizedName = LocalizationService.Create(Prefix + ".Resource.Name", "Efreeti Magic Uses");
                    value.LocalizedDescription = LocalizationService.Create(Prefix + ".Resource.Description", Description);
                    ElementalRaceAbilityFactory.ConfigureBaseAmount(value, 1);
                    return value;
                });
            BlueprintAbility parent = registry.Register<BlueprintAbility>(Prefix + ".Ability", () =>
            {
                BlueprintAbility value = Clone(enlarge, Prefix + ".Ability", resource, "Efreeti Magic", Description);
                value.ComponentsArray = new BlueprintComponent[] {
                    ScriptableObject.CreateInstance<AbilityVariants>(),
                    ElementalRaceAbilityFactory.ResourceCost(resource, true)
                };
                return value;
            });
            BlueprintAbility large = registry.Register<BlueprintAbility>(Prefix + ".EnlargePerson",
                () => Clone(enlarge, Prefix + ".EnlargePerson", resource, "Enlarge Person",
                    enlarge.Description + "\n\n" + Description));
            BlueprintAbility small = registry.Register<BlueprintAbility>(Prefix + ".ReducePerson",
                () => Clone(reduce, Prefix + ".ReducePerson", resource, "Reduce Person",
                    reduce.Description + "\n\n" + Description));
            large.Parent = small.Parent = parent;
            parent.ComponentsArray.OfType<AbilityVariants>().Single().Variants = new[] { large, small };
            return new ElementalTraitDailyAbilityBlueprints {
                Resource = resource, Ability = parent,
                Mechanics = new BlueprintScriptableObject[] { resource, parent, large, small }
            };
        }

        private static BlueprintAbility Donor(LibraryScriptableObject library, string guid)
        {
            BlueprintAbility donor = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                library, guid, "native Efreeti Magic person spell");
            if (donor.Type != AbilityType.Spell || donor.Parent != null || donor.Icon == null ||
                donor.ComponentsArray.OfType<AbilityVariants>().Any() ||
                !donor.ComponentsArray.OfType<AbilityEffectRunAction>().Any() ||
                !donor.ComponentsArray.OfType<SpellComponent>().Any())
                throw new InvalidOperationException("The exact native Efreeti Magic donor contract changed.");
            return donor;
        }

        private static BlueprintAbility Clone(BlueprintAbility donor, string symbol,
            BlueprintAbilityResource resource, string name, string description)
        {
            BlueprintAbility value = BlueprintCloneService.Clone(donor, symbol.Replace('.', '_'));
            value.ComponentsArray = value.ComponentsArray.Where(
                ElementalRaceAbilityFactory.IsSafeNativeEffect).Concat(
                    new BlueprintComponent[] { ElementalRaceAbilityFactory.ResourceCost(resource, true) }).ToArray();
            value.Type = AbilityType.SpellLike;
            value.Parent = null;
            value.Hidden = false;
            value.ActionBarAutoFillIgnored = false;
            value.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            value.ResourceAssetIds = Array.Empty<string>();
            BlueprintUnitFactAccess.Resolve().Configure(value,
                LocalizationService.Create(symbol + ".Name", name),
                LocalizationService.Create(symbol + ".Description", description), donor.Icon);
            return value;
        }
    }
}
