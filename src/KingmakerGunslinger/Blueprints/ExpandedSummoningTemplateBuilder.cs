using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ExpandedSummoningTemplateBuilder
    {
        internal static void Configure(
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            ConfigureOne(Require(bySymbol, "KMG.Summoning.Template.Celestial.Low"),
                true, false);
            ConfigureOne(Require(bySymbol, "KMG.Summoning.Template.Celestial.High"),
                true, true);
            ConfigureOne(Require(bySymbol, "KMG.Summoning.Template.Fiendish.Low"),
                false, false);
            ConfigureOne(Require(bySymbol, "KMG.Summoning.Template.Fiendish.High"),
                false, true);
        }

        private static void ConfigureOne(BlueprintBuff buff, bool celestial,
            bool high)
        {
            int value = high ? 10 : 5;
            var components = new List<BlueprintComponent>();
            if (celestial)
            {
                components.Add(Energy(DamageEnergyType.Acid, value));
                components.Add(Energy(DamageEnergyType.Cold, value));
                components.Add(Energy(DamageEnergyType.Electricity, value));
            }
            else
            {
                components.Add(Energy(DamageEnergyType.Cold, value));
                components.Add(Energy(DamageEnergyType.Fire, value));
            }
            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(value);
            dr.BypassedByAlignment = true;
            dr.Alignment = celestial ? DamageAlignment.Evil : DamageAlignment.Good;
            components.Add(dr);
            if (high)
            {
                var resistance = ScriptableObject.CreateInstance<AddSpellResistance>();
                resistance.AddCR = true;
                resistance.Value = Simple(5);
                components.Add(resistance);
            }
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = true;
            buff.ComponentsArray = components.ToArray();
            string kind = celestial ? "Celestial" : "Fiendish";
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create("KMG.ExpandedSummoning.Template." + kind +
                    (high ? ".High" : ".Low") + ".Name", kind + " Template"),
                LocalizationService.Create("KMG.ExpandedSummoning.Template." + kind +
                    (high ? ".High" : ".Low") + ".Description",
                    kind + " resistances and alignment-bypassed damage reduction."),
                null);
        }

        private static AddDamageResistanceEnergy Energy(DamageEnergyType type,
            int value)
        {
            var result = ScriptableObject.CreateInstance<AddDamageResistanceEnergy>();
            result.Type = type;
            result.Value = Simple(value);
            return result;
        }

        private static ContextValue Simple(int value)
        { return new ContextValue { ValueType = ContextValueType.Simple, Value = value }; }

        private static BlueprintBuff Require(
            IDictionary<string, BlueprintScriptableObject> values, string symbol)
        { return (BlueprintBuff)values[symbol]; }
    }
}
