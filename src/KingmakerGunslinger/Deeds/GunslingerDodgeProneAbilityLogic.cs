using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Deeds
{
    [Serializable]
    public sealed class GunslingerDodgeProneAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        [SerializeField] private BlueprintUnitFact m_ArmedMarker;

        internal static GunslingerDodgeProneAbilityLogic Create(
            BlueprintUnitFact armedMarker)
        {
            if (armedMarker == null) throw new ArgumentNullException("armedMarker");
            var result = ScriptableObject.CreateInstance<GunslingerDodgeProneAbilityLogic>();
            result.m_ArmedMarker = armedMarker;
            return result;
        }

        public bool IsAvailableFor(AbilityData ability)
        {
            return ability != null && ability.Caster != null &&
                m_ArmedMarker != null && !ability.Caster.HasFact(m_ArmedMarker) &&
                !ability.Caster.State.HasCondition(UnitCondition.Prone);
        }

        public string GetReason()
        {
            return "Requires standing and not already armed for the next ranged attack.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (m_ArmedMarker == null || context == null || context.Caster == null ||
                context.Caster.Descriptor == null)
                throw new InvalidOperationException("Dodge delivery lacks its caster or marker.");
            UnitDescriptor caster = context.Caster.Descriptor;
            if (!caster.HasFact(m_ArmedMarker) &&
                !caster.State.HasCondition(UnitCondition.Prone))
                caster.AddFact(m_ArmedMarker);
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }
        internal BlueprintUnitFact ArmedMarker { get { return m_ArmedMarker; } }
    }
}
