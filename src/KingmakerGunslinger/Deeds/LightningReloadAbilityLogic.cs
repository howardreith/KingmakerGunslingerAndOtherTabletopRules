using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Deeds
{
    [Serializable]
    public sealed class LightningReloadAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        public BlueprintItem BlackPowder;
        public BlueprintItem LeadBall;
        public BlueprintBuff UsedMarker;

        public bool IsAvailableFor(AbilityData ability)
        {
            try
            {
                return ability != null && ability.Caster != null &&
                    LightningReloadRuntime.Evaluate(ability.Caster,
                        BlackPowder, LeadBall, UsedMarker).Decision.IsAvailable;
            }
            catch { return false; }
        }

        public string GetReason()
        {
            return "Requires positive grit, an unloaded non-Wrecked equipped firearm, the ammunition selected by Use Paper Cartridges, and no Lightning Reload use this round. Paper mode never falls back to loose ammunition.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null)
                throw new InvalidOperationException(
                    "Lightning Reload delivery has no caster.");
            LightningReloadRuntime.Execute(context.Caster.Descriptor, context,
                BlackPowder, LeadBall, UsedMarker);
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }
    }
}
