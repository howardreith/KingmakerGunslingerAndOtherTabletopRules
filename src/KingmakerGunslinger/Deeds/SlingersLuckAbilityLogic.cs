using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Deeds
{
    [Serializable]
    public sealed class SlingersLuckAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        public BlueprintBuff ArmedMarker;
        public BlueprintAbilityResource Grit;
        public int Cost;

        public bool IsAvailableFor(AbilityData ability)
        {
            return ability != null && ability.Caster != null &&
                ArmedMarker != null && Grit != null && Cost > 0 &&
                ability.Caster.Resources.GetResourceAmount(Grit) >= Cost &&
                !ability.Caster.Buffs.RawFacts.OfType<Buff>().Any(value =>
                    ReferenceEquals(value.Blueprint, ArmedMarker));
        }

        public string GetReason()
        { return "Slinger's Luck is already armed or lacks its fixed grit cost."; }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null || ArmedMarker == null ||
                Grit == null || Cost <= 0 ||
                context.Caster.Descriptor.Resources.GetResourceAmount(Grit) < Cost)
                throw new InvalidOperationException(
                    "Slinger's Luck cannot arm without its exact prerequisites.");
            context.Caster.Descriptor.Buffs.AddBuff(ArmedMarker, context, null);
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }
    }
}
