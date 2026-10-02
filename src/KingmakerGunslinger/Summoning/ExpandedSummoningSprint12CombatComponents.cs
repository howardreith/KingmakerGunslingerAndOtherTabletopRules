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
    /// <summary>
    /// Shared exposure plumbing for the Sprint 12 injury and contact diseases.
    /// Every trigger resolves the same way - one printed Fortitude save, then
    /// the exact payload buff for its printed duration - so the save, the
    /// context ownership and the exemption test live in one place instead of
    /// being re-derived per component.
    /// </summary>
    internal static class SummonDiseaseExposure
    {
        /// <summary>
        /// True when <paramref name="unit"/> carries one of the unit types the
        /// printed rule exempts. Selection is by exact blueprint reference, so
        /// no name or faction heuristic can widen it.
        /// </summary>
        internal static bool IsExempt(UnitEntityData unit,
            BlueprintUnitType[] excluded)
        {
            if (unit == null || excluded == null || unit.Blueprint == null)
                return false;
            for (int index = 0; index < excluded.Length; index++)
                if (excluded[index] != null &&
                    ReferenceEquals(unit.Blueprint.Type, excluded[index]))
                    return true;
            return false;
        }

        internal static bool IsAvailable(UnitEntityData unit)
        {
            return unit != null && unit.Descriptor != null && !unit.Destroyed &&
                !unit.Descriptor.State.IsDead;
        }

        /// <summary>
        /// Rolls the printed save for <paramref name="victim"/> against a DC
        /// owned by <paramref name="source"/> and applies the payload on a
        /// failure. The source keeps ownership of the context so the DC, the
        /// caster and the save identity stay local to this creature.
        /// </summary>
        internal static void Resolve(UnitEntityData source,
            UnitEntityData victim, MechanicsContext parentContext,
            BlueprintBuff payload, int fortitudeDc, int durationSeconds)
        {
            var context = new MechanicsContext(source, victim.Descriptor,
                payload, parentContext, new TargetWrapper(victim));
            context.Params.DC = fortitudeDc;
            var saving = new RuleSavingThrow(victim,
                SavingThrowType.Fortitude, fortitudeDc);
            saving.Reason = context;
            context.TriggerRule(saving);
            if (saving.IsPassed) return;
            TimeSpan? duration = durationSeconds > 0 ?
                TimeSpan.FromSeconds(durationSeconds) : (TimeSpan?)null;
            var apply = new RuleApplyBuff(victim, payload, context, duration,
                (buff, buffSource, time) =>
                    victim.Descriptor.Buffs.AddBuff(buff, buffSource, time));
            Rulebook.Trigger(apply);
        }
    }

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
        public BlueprintUnitType[] ExcludedUnitTypes;
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
            bool targetAvailable = SummonDiseaseExposure.IsAvailable(target);
            bool excluded = targetAvailable && SummonDiseaseExposure.IsExempt(
                target, ExcludedUnitTypes);
            if (owner == null || evt == null || DiseaseBuff == null ||
                FortitudeDc < 1 || !ReferenceEquals(evt.Initiator, owner) ||
                !SummonInjuryDiseasePolicy.ShouldResolve(exactBite, hit,
                    actualDamage, targetAvailable, excluded)) return;
            object prior;
            if (ResolvedAttacks.TryGetValue(evt, out prior)) return;
            ResolvedAttacks.Add(evt, new object());
            SummonDiseaseExposure.Resolve(owner, target,
                Fact == null ? null : Fact.MaybeContext, DiseaseBuff,
                FortitudeDc, DurationSeconds);
        }
    }

    /// <summary>
    /// The printed Goblin Dog allergic reaction is not only a bite rider. A
    /// creature "who deals damage to a goblin dog with a natural weapon or
    /// unarmed attack" is exposed by its own blow, so this component watches
    /// attacks whose target is the carrier and makes the attacker save. A
    /// manufactured weapon never triggers it, and the same weak event key
    /// stops a replayed callback from rolling twice.
    /// </summary>
    [Serializable]
    public sealed class SummonContactAllergyCounterComponent :
        RuleTargetLogicComponent<RuleAttackWithWeapon>
    {
        private static readonly ConditionalWeakTable<RuleAttackWithWeapon,
            object> ResolvedAttacks =
                new ConditionalWeakTable<RuleAttackWithWeapon, object>();

        public BlueprintBuff DiseaseBuff;
        public BlueprintUnitType[] ExcludedUnitTypes;
        public int FortitudeDc;
        public int DurationSeconds;

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitEntityData attacker = evt == null ? null : evt.Initiator;
            ItemEntityWeapon weapon = evt == null ? null : evt.Weapon;
            BlueprintItemWeapon blueprint = weapon == null ? null :
                weapon.Blueprint;
            bool naturalOrUnarmed = blueprint != null &&
                (blueprint.IsNatural || blueprint.IsUnarmed);
            int actualDamage = evt == null || evt.MeleeDamage == null ? 0 :
                Math.Max(0, evt.MeleeDamage.Damage);
            bool hit = evt != null && evt.AttackRoll != null &&
                evt.AttackRoll.IsHit;
            bool attackerAvailable = SummonDiseaseExposure.IsAvailable(attacker);
            bool excluded = attackerAvailable &&
                SummonDiseaseExposure.IsExempt(attacker, ExcludedUnitTypes);
            if (owner == null || evt == null || DiseaseBuff == null ||
                FortitudeDc < 1 || !ReferenceEquals(evt.Target, owner) ||
                ReferenceEquals(attacker, owner) ||
                !SummonInjuryDiseasePolicy.ShouldResolveNaturalCounterContact(
                    hit, actualDamage, naturalOrUnarmed, attackerAvailable,
                    excluded)) return;
            object prior;
            if (ResolvedAttacks.TryGetValue(evt, out prior)) return;
            ResolvedAttacks.Add(evt, new object());
            SummonDiseaseExposure.Resolve(owner, attacker,
                Fact == null ? null : Fact.MaybeContext, DiseaseBuff,
                FortitudeDc, DurationSeconds);
        }
    }

    /// <summary>
    /// The printed rule's remaining trigger exposes a creature that
    /// "otherwise comes into contact with a goblin dog (including attempts to
    /// grapple or ride the creature)". A grapple attempt against the carrier
    /// is the one such contact Kingmaker models as its own rule event; the
    /// printed text exposes the attempt, so a failed grapple still counts.
    /// Riding is outside the charter, which excludes mounted combat.
    /// </summary>
    [Serializable]
    public sealed class SummonContactAllergyManeuverComponent :
        RuleTargetLogicComponent<RuleCombatManeuver>
    {
        private static readonly ConditionalWeakTable<RuleCombatManeuver,
            object> ResolvedManeuvers =
                new ConditionalWeakTable<RuleCombatManeuver, object>();

        public BlueprintBuff DiseaseBuff;
        public BlueprintUnitType[] ExcludedUnitTypes;
        public int FortitudeDc;
        public int DurationSeconds;

        public override void OnEventAboutToTrigger(RuleCombatManeuver evt) { }

        public override void OnEventDidTrigger(RuleCombatManeuver evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitEntityData initiator = evt == null ? null : evt.Initiator;
            bool grapple = evt != null && evt.Type == CombatManeuver.Grapple;
            bool initiatorAvailable =
                SummonDiseaseExposure.IsAvailable(initiator);
            bool excluded = initiatorAvailable &&
                SummonDiseaseExposure.IsExempt(initiator, ExcludedUnitTypes);
            if (owner == null || evt == null || DiseaseBuff == null ||
                FortitudeDc < 1 || !ReferenceEquals(evt.Target, owner) ||
                ReferenceEquals(initiator, owner) ||
                !SummonInjuryDiseasePolicy.ShouldResolveManeuverContact(
                    grapple, initiatorAvailable, excluded)) return;
            object prior;
            if (ResolvedManeuvers.TryGetValue(evt, out prior)) return;
            ResolvedManeuvers.Add(evt, new object());
            SummonDiseaseExposure.Resolve(owner, initiator,
                Fact == null ? null : Fact.MaybeContext, DiseaseBuff,
                FortitudeDc, DurationSeconds);
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
