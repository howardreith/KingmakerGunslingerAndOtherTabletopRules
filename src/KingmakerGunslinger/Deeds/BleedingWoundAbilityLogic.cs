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
    public sealed class BleedingWoundAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        public BlueprintBuff ArmedMarker;
        public BlueprintBuff[] AllMarkers;

        public bool IsAvailableFor(AbilityData ability)
        {
            return ability != null && ability.Caster != null &&
                ArmedMarker != null && !HasMarker(ability, ArmedMarker);
        }

        public string GetReason()
        {
            return "This Bleeding Wound choice is already armed.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null || ArmedMarker == null ||
                AllMarkers == null)
                throw new InvalidOperationException(
                    "Bleeding Wound delivery is missing its caster or markers.");
            foreach (BlueprintBuff marker in AllMarkers)
            {
                Buff fact = context.Caster.Descriptor.Buffs.RawFacts
                    .OfType<Buff>().FirstOrDefault(f => marker != null &&
                        ReferenceEquals(f.Blueprint, marker));
                if (fact != null)
                    context.Caster.Descriptor.Buffs.RemoveFact(fact);
            }
            context.Caster.Descriptor.Buffs.AddBuff(ArmedMarker, context, null);
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }

        private static bool HasMarker(AbilityData ability, BlueprintBuff marker)
        {
            return ability.Caster.Buffs.RawFacts.OfType<Buff>()
                .Any(f => ReferenceEquals(f.Blueprint, marker));
        }
    }
}
