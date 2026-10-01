using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The printed Wolverine rage trigger: "A wolverine that takes damage in
    /// combat flies into a rage on its next turn."
    ///
    /// <para>This component only schedules the rage. It watches damage
    /// resolved against its own carrier and, on actual positive damage,
    /// applies the hidden onset marker whose own round-boundary component
    /// starts the rage one round later. Nothing here applies the +4/+4/-2,
    /// which is what keeps the printed delay impossible to skip: there is no
    /// code path from a damage event to the rage state that does not pass
    /// through a round boundary.</para>
    ///
    /// <para>The rage is summon-local by construction. Both buffs are applied
    /// to <c>Owner.Unit</c> and to nothing else, so no owner, caster, ally or
    /// other summon can receive them, and both leave with the creature.</para>
    /// </summary>
    [Serializable]
    public sealed class SummonRageOnDamageComponent :
        RuleTargetLogicComponent<RuleDealDamage>
    {
        /// <summary>
        /// The hidden marker that waits one round boundary and then applies
        /// <see cref="RageBuff"/>.
        /// </summary>
        public BlueprintBuff OnsetBuff;

        /// <summary>
        /// The rage state itself, carried here only so this component can see
        /// whether the creature is already raging and decline to re-trigger.
        /// </summary>
        public BlueprintBuff RageBuff;

        public override void OnEventAboutToTrigger(RuleDealDamage evt) { }

        public override void OnEventDidTrigger(RuleDealDamage evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (owner == null || evt == null || OnsetBuff == null ||
                RageBuff == null) return;
            int actualDamage = Math.Max(0, evt.Damage);
            bool ownerIsTarget = ReferenceEquals(evt.Target, owner);
            bool ownerAvailable = SummonDiseaseExposure.IsAvailable(owner);
            bool alreadyRaging = HasBuff(owner, RageBuff);
            bool onsetPending = HasBuff(owner, OnsetBuff);
            if (!SummonRagePolicy.ShouldScheduleRage(actualDamage,
                ownerIsTarget, ownerAvailable, alreadyRaging, onsetPending))
                return;
            var context = new MechanicsContext(owner, owner.Descriptor,
                OnsetBuff, Fact == null ? null : Fact.MaybeContext,
                new TargetWrapper(owner));
            var apply = new RuleApplyBuff(owner, OnsetBuff, context, null,
                (buff, source, time) =>
                    owner.Descriptor.Buffs.AddBuff(buff, source, time));
            Rulebook.Trigger(apply);
        }

        internal static bool HasBuff(UnitEntityData unit, BlueprintBuff buff)
        {
            if (unit == null || unit.Descriptor == null || buff == null)
                return false;
            return unit.Descriptor.Buffs.GetBuff(buff) != null;
        }
    }
}
