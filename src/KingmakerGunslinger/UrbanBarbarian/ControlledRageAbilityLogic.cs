using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;

namespace KingmakerGunslinger.UrbanBarbarian
{
    [Serializable]
    public sealed class ControlledRageAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        public BlueprintFeature Selection;
        public BlueprintFeature[] TierSelections;
        public int Tier;

        public bool IsAvailableFor(AbilityData ability)
        {
            if (ability == null || ability.Caster == null || Selection == null ||
                TierSelections == null || TierSelections.Length == 0) return false;
            ControlledRageTier tier;
            if (!ControlledRageRuntime.TryCurrentTier(ability.Caster, out tier) ||
                (int)tier != Tier ||
                ControlledRageRuntime.IsUrbanRageActive(ability.Caster))
                return false;
            return !ability.Caster.HasFact(Selection);
        }

        public string GetReason()
        {
            return "Only a current-tier allocation can be changed, and not while Controlled Rage is active.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null ||
                !IsAvailableFor(context.Ability))
                throw new InvalidOperationException(
                    "Controlled Rage allocation prerequisites changed before execution.");
            var previous = TierSelections.Where(value => value != null &&
                context.Caster.Descriptor.HasFact(value)).ToArray();
            foreach (BlueprintFeature feature in previous)
                context.Caster.Descriptor.RemoveFact(feature);
            try
            {
                if (context.Caster.Descriptor.AddFact(Selection) == null ||
                    !context.Caster.Descriptor.HasFact(Selection))
                    throw new InvalidOperationException(
                        "Controlled Rage selection fact was rejected.");
            }
            catch
            {
                foreach (BlueprintFeature feature in previous)
                    if (!context.Caster.Descriptor.HasFact(feature))
                        context.Caster.Descriptor.AddFact(feature);
                throw;
            }
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }
    }
}
