using System;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using UnityEngine;

namespace KingmakerGunslinger.Deeds
{
    /// <summary>
    /// The registered Deadeye ability's grit component: availability, the
    /// displayed and native-checked cost and the debit all come from
    /// DeadeyeCostPolicy (one ordinary grit, adjusted by True Grit). The
    /// native resource logic spends when the activation commits; the armed
    /// state itself is applied by the ability's native apply-buff action.
    /// Before POL-02 the availability and displayed cost fell through to the
    /// native custom-cost fallback (a logged error and a cost of 1), and an
    /// unregistered DeadeyeAbilityLogic class carried a second copy of the
    /// cost; that class is removed.
    /// </summary>
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

        private static TrueGritDecision Arming(AbilityData ability)
        {
            return TrueGritRuntime.Evaluate(ability.Caster, TrueGritDeed.Deadeye,
                DeadeyeCostPolicy.OrdinaryArmingCost, false);
        }

        public override bool IsAvailableFor(AbilityData ability)
        {
            return ability != null && ability.Caster != null && RequiredResource != null &&
                Arming(ability).Available;
        }

        public override int CalculateCost(AbilityData ability)
        {
            if (ability == null || ability.Caster == null) return DeadeyeCostPolicy.OrdinaryArmingCost;
            return Arming(ability).NativeCheckCost;
        }

        public override void Spend(AbilityData ability)
        {
            if (ability == null || ability.Caster == null) return;
            TrueGritDecision cost = Arming(ability);
            if (!cost.Available || ability.Caster.Resources.GetResourceAmount(
                    RequiredResource) < cost.EffectiveCost)
                throw new InvalidOperationException("Deadeye requires enough Grit.");
            ability.Caster.Resources.Spend(RequiredResource, cost.EffectiveCost);
        }
    }
}
