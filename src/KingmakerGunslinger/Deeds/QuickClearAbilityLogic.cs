using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Deeds
{
    [Serializable]
    public sealed class QuickClearAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        [SerializeField] private QuickClearMode m_Mode;
        internal static QuickClearAbilityLogic Create(QuickClearMode mode)
        {
            var result = ScriptableObject.CreateInstance<QuickClearAbilityLogic>();
            result.m_Mode = mode; return result;
        }
        public bool IsAvailableFor(AbilityData ability)
        {
            if (ability == null || ability.Caster == null)
                return false;
            Actions.ExactEquippedFirearmContext ignored; string reason;
            return QuickClearRuntime.Evaluate(ability.Caster, m_Mode,
                out ignored, out reason).ShouldRepair;
        }
        public string GetReason() { return "Requires at least 1 grit and exactly one equipped misfire-broken firearm."; }
        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null || context.Caster.Descriptor == null)
                throw new InvalidOperationException("Quick Clear delivery lacks its caster.");
            QuickClearRuntime.Execute(context.Caster.Descriptor, m_Mode);
            yield return new AbilityDeliveryTarget(target);
        }
        public override void Cleanup(AbilityExecutionContext context) { }
        internal QuickClearMode Mode { get { return m_Mode; } }
    }
}
