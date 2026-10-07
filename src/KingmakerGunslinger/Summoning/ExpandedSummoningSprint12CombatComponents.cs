using System;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>One exact natural bite can expose its target only after the
    /// native attack has hit and dealt positive final damage. The weak event
    /// key prevents a replayed provider callback from rolling a second save.</summary>
    [Serializable]
    public sealed class SummonInjuryDiseaseComponent :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        private static readonly ConditionalWeakTable<RuleAttackWithWeapon,
            object> ResolvedAttacks =
                new ConditionalWeakTable<RuleAttackWithWeapon, object>();

        public BlueprintItemWeapon BiteWeapon;
        public BlueprintBuff DiseaseBuff;
        public BlueprintUnitType ExcludedUnitType;
        public int FortitudeDc;
        public int DurationSeconds;

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitEntityData target = evt == null ? null : evt.Target;
            ItemEntityWeapon weapon = evt == null ? null : evt.Weapon;
            int actualDamage = evt == null || evt.MeleeDamage == null ? 0 :
                Math.Max(0, evt.MeleeDamage.Damage);
            bool exactBite = weapon != null && BiteWeapon != null &&
                ReferenceEquals(weapon.Blueprint, BiteWeapon);
            bool hit = evt != null && evt.AttackRoll != null &&
                evt.AttackRoll.IsHit;
            bool targetAvailable = target != null && target.Descriptor != null &&
                !target.Destroyed && !target.Descriptor.State.IsDead;
            bool excluded = targetAvailable && ExcludedUnitType != null &&
                target.Blueprint != null &&
                ReferenceEquals(target.Blueprint.Type, ExcludedUnitType);
            if (owner == null || evt == null || DiseaseBuff == null ||
                FortitudeDc < 1 || !ReferenceEquals(evt.Initiator, owner) ||
                !SummonInjuryDiseasePolicy.ShouldResolve(exactBite, hit,
                    actualDamage, targetAvailable, excluded)) return;
            object prior;
            if (ResolvedAttacks.TryGetValue(evt, out prior)) return;
            ResolvedAttacks.Add(evt, new object());
            Resolve(owner, target);
        }

        private void Resolve(UnitEntityData owner, UnitEntityData target)
        {
            var context = new MechanicsContext(owner, target.Descriptor,
                DiseaseBuff, Fact == null ? null : Fact.MaybeContext,
                new TargetWrapper(target));
            context.Params.DC = FortitudeDc;
            var saving = new RuleSavingThrow(target,
                SavingThrowType.Fortitude, FortitudeDc);
            saving.Reason = context;
            context.TriggerRule(saving);
            if (saving.IsPassed) return;
            TimeSpan? duration = DurationSeconds > 0 ?
                TimeSpan.FromSeconds(DurationSeconds) : (TimeSpan?)null;
            var apply = new RuleApplyBuff(target, DiseaseBuff, context,
                duration, (buff, source, time) =>
                    target.Descriptor.Buffs.AddBuff(buff, source, time));
            Rulebook.Trigger(apply);
        }
    }

    /// <summary>The Goblin Dog reaction ends only after positive healing from
    /// an actual spell, spell-like ability, or supernatural ability. Ordinary
    /// rest, regeneration, and zero-value healing rules do not clear it.</summary>
    [Serializable]
    public sealed class SummonAllergicReactionHealingComponent :
        RuleTargetLogicComponent<RuleHealDamage>
    {
        public override void OnEventAboutToTrigger(RuleHealDamage evt) { }

        public override void OnEventDidTrigger(RuleHealDamage evt)
        {
            if (evt == null || Owner == null ||
                !ReferenceEquals(evt.Target, Owner.Unit)) return;
            BlueprintAbility source = evt.Reason != null &&
                evt.Reason.Ability != null ? evt.Reason.Ability.Blueprint :
                evt.Reason == null || evt.Reason.Context == null ? null :
                    evt.Reason.Context.SourceAbility;
            AbilityType? type = source == null ? (AbilityType?)null : source.Type;
            if (SummonInjuryDiseasePolicy.ShouldRemoveAllergicReaction(
                    Math.Max(0, evt.Value),
                    type == AbilityType.Spell,
                    type == AbilityType.SpellLike,
                    type == AbilityType.Supernatural))
                Owner.Buffs.RemoveFact(Fact);
        }
    }
}
