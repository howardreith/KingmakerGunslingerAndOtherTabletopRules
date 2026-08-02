using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Deeds
{
    [Serializable]
    public sealed class StartlingShotAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        public BlueprintBuff FlatFootedBuff;

        public bool IsAvailableFor(AbilityData ability)
        {
            if (ability == null || ability.Caster == null) return false;
            Actions.ExactEquippedFirearmContext firearm;
            return StartlingShotRuntime.Evaluate(ability.Caster, true,
                out firearm).ShouldApply;
        }

        public string GetReason()
        {
            return "Requires positive grit and exactly one equipped loaded, non-Wrecked firearm.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null || target == null ||
                target.Unit == null)
                throw new InvalidOperationException(
                    "Startling Shot requires an enemy unit target.");
            StartlingShotRuntime.Execute(context.Caster.Descriptor, target.Unit,
                FlatFootedBuff, context);
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }
    }
}
