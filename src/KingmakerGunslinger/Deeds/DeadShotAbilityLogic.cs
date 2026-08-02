using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Deeds
{
    [Serializable]
    public sealed class DeadShotAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        public bool IsAvailableFor(AbilityData ability)
        {
            if (ability == null || ability.Caster == null) return false;
            Actions.ExactEquippedFirearmContext ignored;
            string reason;
            return DeadShotRuntime.Evaluate(ability.Caster, out ignored,
                out reason).ShouldAttack;
        }

        public string GetReason()
        {
            return "Requires 1 grit and one loaded, non-Wrecked, non-scatter firearm.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (target == null || target.Unit == null)
                throw new InvalidOperationException("Dead Shot requires a unit target.");
            DeadShotRuntime.Execute(context, target.Unit);
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }
    }
}
