using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Deeds
{
    [Serializable]
    public sealed class GunslingerDodgeProneAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        [SerializeField] private BlueprintUnitFact m_ArmedMarker;
        [SerializeField] private BlueprintBuff m_ArmorClassBuff;

        internal static GunslingerDodgeProneAbilityLogic Create(
            BlueprintUnitFact armedMarker, BlueprintBuff armorClassBuff)
        {
            if (armedMarker == null) throw new ArgumentNullException("armedMarker");
            if (armorClassBuff == null) throw new ArgumentNullException("armorClassBuff");
            var result = ScriptableObject.CreateInstance<GunslingerDodgeProneAbilityLogic>();
            result.m_ArmedMarker = armedMarker;
            result.m_ArmorClassBuff = armorClassBuff;
            return result;
        }

        public bool IsAvailableFor(AbilityData ability)
        {
            if (ability == null || ability.Caster == null || m_ArmorClassBuff == null)
                return false;
            GunslingerClassBlueprintSet set = BlueprintBootstrap.GunslingerClass;
            return set != null && ability.Caster.Resources.GetResourceAmount(
                set.Grit.Resource) >= TrueGritRuntime.Evaluate(ability.Caster,
                    TrueGritDeed.GunslingersDodge, 1, false).EffectiveCost;
        }

        public string GetReason()
        {
            return "Requires enough Grit to pay the immediate 1 Grit cost.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (m_ArmedMarker == null || m_ArmorClassBuff == null ||
                context == null || context.Caster == null ||
                context.Caster.Descriptor == null)
                throw new InvalidOperationException("Dodge delivery lacks its caster or marker.");
            UnitDescriptor caster = context.Caster.Descriptor;
            if (caster.HasFact(m_ArmedMarker)) caster.RemoveFact(m_ArmedMarker);
            var buffContext = new MechanicsContext(context.Caster, caster,
                context.Ability.Blueprint, null, new TargetWrapper(context.Caster));
            if (caster.Buffs.AddBuff(m_ArmorClassBuff, buffContext,
                TimeSpan.FromSeconds(6d)) == null)
                throw new InvalidOperationException("Gunslinger's Dodge buff was not created.");
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }
        internal BlueprintUnitFact ArmedMarker { get { return m_ArmedMarker; } }
        internal BlueprintBuff ArmorClassBuff { get { return m_ArmorClassBuff; } }
    }

    internal sealed class DodgeGritResourceLogic : AbilityResourceLogic
    {
        internal static DodgeGritResourceLogic Create(BlueprintAbilityResource grit)
        {
            var value = ScriptableObject.CreateInstance<DodgeGritResourceLogic>();
            value.name = "$KMG_DodgeNativeGrit";
            value.RequiredResource = grit;
            value.IsSpendResource = true;
            value.CostIsCustom = true;
            value.Amount = 0;
            return value;
        }

        public override void Spend(AbilityData ability)
        {
            if (ability == null || ability.Caster == null) return;
            TrueGritDecision cost = TrueGritRuntime.Evaluate(ability.Caster,
                TrueGritDeed.GunslingersDodge, 1, false);
            if (!cost.Available || ability.Caster.Resources.GetResourceAmount(
                    RequiredResource) < cost.EffectiveCost)
                throw new InvalidOperationException(
                    "Gunslinger's Dodge requires enough Grit.");
            ability.Caster.Resources.Spend(RequiredResource, cost.EffectiveCost);
        }
    }
}
