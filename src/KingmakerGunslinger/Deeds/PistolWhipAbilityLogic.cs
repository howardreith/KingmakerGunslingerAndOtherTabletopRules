using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Deeds
{
    [Serializable]
    public sealed class PistolWhipAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        [SerializeField] private BlueprintItemWeapon m_OneHandedSurrogate;
        [SerializeField] private BlueprintItemWeapon m_TwoHandedSurrogate;

        internal static PistolWhipAbilityLogic Create(BlueprintItemWeapon oneHanded,
            BlueprintItemWeapon twoHanded)
        {
            if (oneHanded == null || twoHanded == null)
                throw new ArgumentNullException("oneHanded");
            var result = ScriptableObject.CreateInstance<PistolWhipAbilityLogic>();
            result.m_OneHandedSurrogate = oneHanded;
            result.m_TwoHandedSurrogate = twoHanded;
            return result;
        }

        public bool IsAvailableFor(AbilityData ability)
        {
            if (ability == null || ability.Caster == null) return false;
            Actions.ExactEquippedFirearmContext ignored;
            string reason;
            return PistolWhipRuntime.Evaluate(ability.Caster, out ignored,
                out reason).ShouldAttack;
        }

        public string GetReason()
        {
            return "Requires 1 grit and exactly one equipped non-Wrecked firearm.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (target == null || target.Unit == null)
                throw new InvalidOperationException("Pistol-Whip requires a unit target.");
            PistolWhipRuntime.Execute(context, target.Unit,
                m_OneHandedSurrogate, m_TwoHandedSurrogate);
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }
        internal BlueprintItemWeapon OneHandedSurrogate { get { return m_OneHandedSurrogate; } }
        internal BlueprintItemWeapon TwoHandedSurrogate { get { return m_TwoHandedSurrogate; } }
    }
}
