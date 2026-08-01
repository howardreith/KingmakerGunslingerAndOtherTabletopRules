using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Blueprints.Facts;
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

        internal static DeadeyeAbilityLogic Create(BlueprintUnitFact armedMarker)
        {
            if (armedMarker == null) throw new ArgumentNullException("armedMarker");
            var result = ScriptableObject.CreateInstance<DeadeyeAbilityLogic>();
            result.m_ArmedMarker = armedMarker;
            return result;
        }

        public bool IsAvailableFor(AbilityData ability)
        {
            return ability != null && ability.Caster != null &&
                m_ArmedMarker != null && !ability.Caster.HasFact(m_ArmedMarker);
        }

        public string GetReason()
        {
            return "Deadeye is already armed for the next firearm shot.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (m_ArmedMarker == null || context == null || context.Caster == null ||
                context.Caster.Descriptor == null)
                throw new InvalidOperationException("Deadeye delivery is missing its caster or marker.");
            UnitDescriptor caster = context.Caster.Descriptor;
            if (!caster.HasFact(m_ArmedMarker))
                caster.AddFact(m_ArmedMarker);
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }

        internal BlueprintUnitFact ArmedMarker { get { return m_ArmedMarker; } }
    }
}
