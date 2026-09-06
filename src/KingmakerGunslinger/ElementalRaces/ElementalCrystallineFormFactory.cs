using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.ResourceLinks;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal sealed class ElementalCrystallineFormBlueprints
    {
        internal BlueprintAbilityResource Resource;
        internal BlueprintBuff ArmedBuff;
        internal BlueprintActivatableAbility Mode;
        internal BlueprintScriptableObject[] Mechanics { get { return new BlueprintScriptableObject[] { Resource, ArmedBuff, Mode }; } }

        internal BlueprintComponent[] ProviderComponents()
        {
            var facts = ScriptableObject.CreateInstance<AddFacts>();
            facts.Facts = new BlueprintUnitFact[] { Mode };
            facts.DoNotRestoreMissingFacts = false;
            var add = ScriptableObject.CreateInstance<AddAbilityResources>();
            add.Resource = Resource;
            add.RestoreAmount = true;
            add.RestoreOnLevelUp = false;
            var memory = ScriptableObject.CreateInstance<ElementalTraitDailyResourceState>();
            memory.Resource = Resource;
            var deflect = ScriptableObject.CreateInstance<ElementalCrystallineRayDeflection>();
            deflect.Resource = Resource;
            deflect.ArmedBuff = ArmedBuff;
            deflect.Mode = Mode;
            return new BlueprintComponent[] { facts, add, memory,
                ScriptableObject.CreateInstance<ElementalCrystallineRayArmorClass>(), deflect };
        }
    }

    internal static class ElementalCrystallineFormFactory
    {
        internal const string Prefix = "KMG.ElementalRaces.Traits.Oread.CrystallineForm";
        internal const string Description = ElementalCrystallineFormPolicy.Description;

        internal static ElementalCrystallineFormBlueprints Register(BlueprintRegistry registry,
            ElementalAlternateTraitId trait, Sprite icon)
        {
            if (trait != ElementalAlternateTraitId.CrystallineForm) return null;
            var graph = new ElementalCrystallineFormBlueprints();
            graph.Resource = registry.Register<BlueprintAbilityResource>(Prefix + ".Resource", () => {
                var value = ScriptableObject.CreateInstance<BlueprintAbilityResource>();
                value.name = (Prefix + ".Resource").Replace('.', '_');
                value.LocalizedName = LocalizationService.Create(Prefix + ".Resource.Name", "Crystalline Deflection Uses");
                value.LocalizedDescription = LocalizationService.Create(Prefix + ".Resource.Description", Description);
                ElementalRaceAbilityFactory.ConfigureBaseAmount(value, 1);
                return value;
            });
            graph.ArmedBuff = registry.Register<BlueprintBuff>(Prefix + ".ArmedBuff", () => {
                var value = ScriptableObject.CreateInstance<BlueprintBuff>();
                value.name = (Prefix + ".ArmedBuff").Replace('.', '_');
                value.ComponentsArray = Array.Empty<BlueprintComponent>();
                value.FxOnStart = new PrefabLink();
                value.FxOnRemove = new PrefabLink();
                value.ResourceAssetIds = Array.Empty<string>();
                BlueprintUnitFactAccess.Resolve().Configure(value,
                    LocalizationService.Create(Prefix + ".ArmedBuff.Name", "Crystalline Deflection Ready"),
                    LocalizationService.Create(Prefix + ".ArmedBuff.Description", Description), icon);
                return ElementalComponentIdentity.Prepare(value);
            });
            graph.Mode = registry.Register<BlueprintActivatableAbility>(Prefix + ".Mode", () => {
                var value = ScriptableObject.CreateInstance<BlueprintActivatableAbility>();
                value.name = (Prefix + ".Mode").Replace('.', '_');
                value.Buff = graph.ArmedBuff;
                value.Group = ActivatableAbilityGroup.None;
                value.WeightInGroup = 1;
                value.IsOnByDefault = false;
                value.ActivationType = AbilityActivationType.Immediately;
                value.DeactivateImmediately = true;
                value.DeactivateIfCombatEnded = false;
                value.DeactivateAfterFirstRound = false;
                value.DeactivateIfOwnerDisabled = false;
                value.DeactivateIfOwnerUnconscious = false;
                value.OnlyInCombat = false;
                value.ActionBarAutoFillIgnored = false;
                value.ResourceAssetIds = Array.Empty<string>();
                var resource = ScriptableObject.CreateInstance<ActivatableAbilityResourceLogic>();
                resource.RequiredResource = graph.Resource;
                // Native Never requires one available use but never spends it
                // on activation, rounds or attacks. The exact hit subscriber commits it.
                resource.SpendType = ActivatableAbilityResourceLogic.ResourceSpendType.Never;
                value.ComponentsArray = new BlueprintComponent[] { resource };
                BlueprintUnitFactAccess.Resolve().Configure(value,
                    LocalizationService.Create(Prefix + ".Mode.Name", "Deflect Next Ray"),
                    LocalizationService.Create(Prefix + ".Mode.Description", Description), icon);
                return ElementalComponentIdentity.Prepare(value);
            });
            return graph;
        }
    }
}
