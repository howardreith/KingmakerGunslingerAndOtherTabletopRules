using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Scatter
{
    [Serializable]
    public sealed class ScatterShotAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        internal static ScatterShotAbilityLogic Create()
        {
            return UnityEngine.ScriptableObject.CreateInstance<ScatterShotAbilityLogic>();
        }

        public bool IsAvailableFor(AbilityData ability)
        {
            string reason;
            return ability != null && ability.Caster != null &&
                ScatterShotRuntime.IsAvailable(ability.Caster, out reason);
        }

        public string GetReason()
        {
            return "Requires exactly one equipped, loaded, non-Wrecked Blunderbuss.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.MaybeCaster == null || target == null ||
                target.Unit == null)
                throw new InvalidOperationException(
                    "Scatter Shot requires a caster and a unit direction target.");
            ScatterShotRuntime.ExecuteForRuntimeTest(context.MaybeCaster, target.Unit);
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }
    }
}
