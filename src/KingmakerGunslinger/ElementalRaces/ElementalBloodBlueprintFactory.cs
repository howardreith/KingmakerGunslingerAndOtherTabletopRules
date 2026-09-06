using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Enums.Damage;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalBloodBlueprintFactory
    {
        internal static BlueprintBuff Register(BlueprintRegistry registry,
            ElementalAlternateTraitDefinition definition, Sprite icon)
        {
            if (!ElementalBloodPolicy.IsBloodTrait(definition.Id)) return null;
            string symbol = definition.MarkerSymbol + ".FastHealingBuff";
            return registry.Register<BlueprintBuff>(symbol, () =>
            {
                var buff = ScriptableObject.CreateInstance<BlueprintBuff>();
                buff.name = symbol.Replace('.', '_');
                buff.IsClassFeature = false;
                buff.Stacking = StackingType.Replace;
                buff.Frequency = DurationRate.Rounds;
                buff.FxOnStart = new PrefabLink();
                buff.FxOnRemove = new PrefabLink();
                buff.ResourceAssetIds = Array.Empty<string>();
                var healing = ScriptableObject.CreateInstance<ElementalBloodFastHealing>();
                healing.Trait = (int)definition.Id;
                buff.ComponentsArray = new BlueprintComponent[] { healing };
                BlueprintUnitFactAccess.Resolve().Configure(buff,
                    LocalizationService.Create(symbol + ".Name", definition.Name),
                    LocalizationService.Create(symbol + ".Description",
                        "Fast healing 2 for 1 round. Only hit points actually restored count " +
                        "toward the daily limit of twice your total character level. " +
                        "Repeated triggers refresh the duration without stacking the healing."), icon);
                return ElementalComponentIdentity.Prepare(buff);
            });
        }

        internal static BlueprintComponent[] ComponentsFor(
            ElementalAlternateTraitDefinition definition, BlueprintBuff buff)
        {
            if (buff == null) return Array.Empty<BlueprintComponent>();
            if (!ElementalBloodPolicy.IsBloodTrait(definition.Id))
                throw new InvalidOperationException("A blood buff cannot be bound to an unrelated trait.");
            var trigger = ScriptableObject.CreateInstance<ElementalBloodDamageTrigger>();
            trigger.Trait = (int)definition.Id;
            trigger.Energy = definition.Id == ElementalAlternateTraitId.FireInTheBlood
                ? DamageEnergyType.Fire : definition.Id == ElementalAlternateTraitId.StoneInTheBlood
                    ? DamageEnergyType.Acid : DamageEnergyType.Electricity;
            trigger.HealingBuff = buff;
            return new BlueprintComponent[] { trigger };
        }

        internal static void Bind(BlueprintBuff buff, BlueprintFeature provider, BlueprintFeature marker)
        {
            if (buff != null)
            {
                ElementalBloodFastHealing healing = buff.ComponentsArray.OfType<ElementalBloodFastHealing>().Single();
                healing.Provider = provider;
                healing.Marker = marker;
            }
        }
    }
}
