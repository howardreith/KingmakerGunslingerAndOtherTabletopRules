using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Deeds
{
    [Serializable]
    public sealed class DeadeyeAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        [SerializeField]
        private BlueprintUnitFact m_ArmedMarker;
        [SerializeField] private BlueprintBuff m_ArmedBuff;

        internal static DeadeyeAbilityLogic Create(BlueprintUnitFact armedMarker,
            BlueprintBuff armedBuff)
        {
            if (armedMarker == null) throw new ArgumentNullException("armedMarker");
            if (armedBuff == null) throw new ArgumentNullException("armedBuff");
            var result = ScriptableObject.CreateInstance<DeadeyeAbilityLogic>();
            result.m_ArmedMarker = armedMarker;
            result.m_ArmedBuff = armedBuff;
            return result;
        }

        public bool IsAvailableFor(AbilityData ability)
        {
            if (ability == null || ability.Caster == null || m_ArmedBuff == null ||
                ability.Caster.Buffs.RawFacts.Any(value =>
                    ReferenceEquals(value.Blueprint, m_ArmedBuff))) return false;
            GunslingerClassBlueprintSet set = BlueprintBootstrap.GunslingerClass;
            if (set == null) return false;
            TrueGritDecision cost = TrueGritRuntime.Evaluate(ability.Caster,
                TrueGritDeed.Deadeye, 1, false);
            return cost.Available && ability.Caster.Resources.GetResourceAmount(
                set.Grit.Resource) >= cost.EffectiveCost;
        }

        public string GetReason()
        {
            return "Requires enough Grit and no existing Deadeye Armed buff.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (m_ArmedMarker == null || m_ArmedBuff == null || context == null || context.Caster == null ||
                context.Caster.Descriptor == null)
                throw new InvalidOperationException("Deadeye delivery is missing its caster or marker.");
            UnitDescriptor caster = context.Caster.Descriptor;
            if (caster.HasFact(m_ArmedMarker)) caster.RemoveFact(m_ArmedMarker);
            var buffContext = new MechanicsContext(context.Caster, caster,
                context.Ability.Blueprint, null, new TargetWrapper(context.Caster));
            if (caster.Buffs.AddBuff(m_ArmedBuff, buffContext,
                TimeSpan.FromSeconds(6d)) == null)
                throw new InvalidOperationException("Deadeye Armed buff was not created.");
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }

        internal BlueprintUnitFact ArmedMarker { get { return m_ArmedMarker; } }
        internal BlueprintBuff ArmedBuff { get { return m_ArmedBuff; } }
    }

    internal sealed class DeadeyeGritResourceLogic : AbilityResourceLogic
    {
        internal static DeadeyeGritResourceLogic Create(BlueprintAbilityResource grit)
        {
            var value = ScriptableObject.CreateInstance<DeadeyeGritResourceLogic>();
            value.name = "$KMG_DeadeyeNativeGrit";
            value.RequiredResource = grit; value.IsSpendResource = true;
            value.CostIsCustom = true; value.Amount = 0;
            return value;
        }

        public override void Spend(AbilityData ability)
        {
            if (ability == null || ability.Caster == null) return;
            TrueGritDecision cost = TrueGritRuntime.Evaluate(ability.Caster,
                TrueGritDeed.Deadeye, 1, false);
            if (!cost.Available || ability.Caster.Resources.GetResourceAmount(
                    RequiredResource) < cost.EffectiveCost)
                throw new InvalidOperationException("Deadeye requires enough Grit.");
            ability.Caster.Resources.Spend(RequiredResource, cost.EffectiveCost);
        }
    }
}
