using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Deeds
{
    [Serializable]
    public sealed class ExpertLoadingAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        public BlueprintBuff ArmedMarker;

        public bool IsAvailableFor(AbilityData ability)
        {
            return ability != null && ability.Caster != null &&
                ArmedMarker != null && !ability.Caster.Buffs.RawFacts
                    .OfType<Buff>().Any(value =>
                        ReferenceEquals(value.Blueprint, ArmedMarker));
        }

        public string GetReason()
        {
            return "Expert Loading is already armed.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null || ArmedMarker == null)
                throw new InvalidOperationException(
                    "Expert Loading delivery is missing its caster or marker.");
            context.Caster.Descriptor.Buffs.AddBuff(ArmedMarker, context, null);
            if (!context.Caster.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Any(value => ReferenceEquals(value.Blueprint, ArmedMarker)))
                throw new InvalidOperationException(
                    "Expert Loading armed marker was rejected.");
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }
    }
}
