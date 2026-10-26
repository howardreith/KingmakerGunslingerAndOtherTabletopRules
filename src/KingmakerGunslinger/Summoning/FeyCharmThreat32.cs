using System;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    // Only the private Satyr/Lillend Charm terminal uses this stateless adapter.
    // Native Charm faction handling is retained. Threats from its caster or
    // apparent allies break the effect; unrelated enemies and harmless acts do not.
    [Serializable]
    public sealed class FeyCharmThreat32 : BuffLogic, IUnitCommandStartHandler,
        IApplyAbilityEffectHandler, ITargetRulebookHandler<RuleAttackWithWeapon>,
        ITargetRulebookHandler<RuleDealDamage>
    {
        private bool SourceThreatens(UnitEntityData actor)
        {
            var caster = Buff?.Context?.MaybeCaster;
            return Buff != null && Buff.Active && !Buff.IsDisposed && caster != null &&
                !caster.Destroyed && caster.IsInState && actor != null &&
                !ReferenceEquals(actor, Owner.Unit) && (ReferenceEquals(actor, caster) ||
                (!actor.IsEnemy(caster) && !caster.IsEnemy(actor) && actor.IsAlly(caster)));
        }

        private static bool Harmful(BlueprintAbility ability, UnitEntityData recipient)
        {
            if (ability == null) return false;
            // Native positive healing has no ally metadata. Its actual target
            // relation is determined by IsUndead, including the installed
            // negative-energy-affinity seam. RestoreHP alone is not an exemption.
            var positiveHealing = SpellDescriptor.Cure | SpellDescriptor.RestoreHP;
            if ((ability.SpellDescriptor & positiveHealing) == positiveHealing)
                return recipient != null && recipient.Descriptor.IsUndead;
            // Native metadata is relation-sensitive. Helpful ally effects such
            // as healing must not break Charm because their enemy metadata is
            // harmful. Untagged actual damage is handled by its native rule.
            return ability != null && (ability.EffectOnAlly == AbilityEffectOnUnit.Harmful ||
                (ability.EffectOnAlly == AbilityEffectOnUnit.None && ability.EffectOnEnemy == AbilityEffectOnUnit.Harmful));
        }

        public void HandleUnitCommandDidStart(UnitCommand command)
        {
            if (command == null || !SourceThreatens(command.Executor)) return;
            var attack = command as UnitAttack;
            var use = command as UnitUseAbility;
            if (attack != null && ReferenceEquals(attack.Target, Owner.Unit) ||
                use != null && ReferenceEquals(use.Target?.Unit, Owner.Unit) && Harmful(use.Spell?.Blueprint, Owner.Unit))
                Buff.Remove();
        }

        // Unlike the global offensive-action event, this native callback is not
        // filtered by hostility. AoE recipients arrive before SR/saving throws.
        public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, TargetWrapper target)
        {
            if (ReferenceEquals(target?.Unit, Owner.Unit) && SourceThreatens(context?.MaybeCaster) &&
                Harmful(context?.AbilityBlueprint, Owner.Unit)) Buff.Remove();
        }
        public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, TargetWrapper target) { }
        public void OnAbilityEffectApplied(AbilityExecutionContext context) { }

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            // Includes native attacks of opportunity without UnitAttack, even misses.
            if (evt != null && ReferenceEquals(evt.Target, Owner.Unit) && SourceThreatens(evt.Initiator)) Buff.Remove();
        }
        public void OnEventDidTrigger(RuleAttackWithWeapon evt) { }
        public void OnEventAboutToTrigger(RuleDealDamage evt) { }
        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (evt != null && !evt.IsFake && evt.Damage > 0 && ReferenceEquals(evt.Target, Owner.Unit) &&
                SourceThreatens(evt.Initiator)) Buff.Remove();
        }
    }
}
