using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Firing;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Astral Deva warhammer rider. It counts exact resolved hits by
    /// this unit's private hammer, asks for one Fortitude save after the second
    /// same-opponent hit in a round, and applies one independently expiring
    /// native Stunned condition for a native d6 rounds on failure.</summary>
    [Serializable]
    public sealed class AstralDevaHammerStun :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>, ITickEachRound
    {
        private static readonly ReferenceEventGate Events = new ReferenceEventGate();
        private static readonly ConditionalWeakTable<UnitEntityData,
            AstralDevaStunTracker> Trackers =
            new ConditionalWeakTable<UnitEntityData, AstralDevaStunTracker>();

        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Warhammer;
        public BlueprintBuff Stunned;

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitEntityData victim = evt == null ? null : evt.Target;
            RuleAttackRoll attack = evt == null ? null : evt.AttackRoll;
            if (owner == null || victim == null || victim.Destroyed ||
                victim.Descriptor.State.IsDead || OwningBlueprint == null ||
                Warhammer == null || Stunned == null || evt.Weapon == null ||
                evt.Weapon.Blueprint == null || attack == null || !attack.IsHit ||
                !ReferenceEquals(evt.Initiator, owner) ||
                !ReferenceEquals(owner.Blueprint, OwningBlueprint) ||
                !ReferenceEquals(evt.Weapon.Blueprint, Warhammer) ||
                string.IsNullOrEmpty(victim.UniqueId) || !Events.TryMark(attack))
                return;

            bool attempt;
            lock (Trackers)
            { attempt = TrackerFor(owner).RecordHit(victim.UniqueId); }
            if (!attempt) return;

            int dc = AstralDevaRulesPolicy.StunDifficultyClass(
                owner.Stats.Strength.Bonus);
            var context = new MechanicsContext(owner, victim.Descriptor, Stunned,
                Fact == null ? null : Fact.MaybeContext, new TargetWrapper(victim));
            context.Params.DC = dc;
            var saving = new RuleSavingThrow(victim, SavingThrowType.Fortitude, dc)
            { Reason = context };
            context.TriggerRule(saving);
            if (saving.IsPassed) return;

            var durationRoll = new RuleRollDice(owner,
                new DiceFormula(1, DiceType.D6)) { Reason = context };
            int rounds = context.TriggerRule(durationRoll).Result;
            victim.Descriptor.Buffs.AddBuff(Stunned, context,
                TimeSpan.FromSeconds(rounds * 6d));
        }

        public void OnNewRound()
        { Forget(Owner == null ? null : Owner.Unit); }

        public override void OnTurnOff()
        {
            Forget(Owner == null ? null : Owner.Unit);
            base.OnTurnOff();
        }

        private static AstralDevaStunTracker TrackerFor(UnitEntityData owner)
        {
            AstralDevaStunTracker tracker;
            if (Trackers.TryGetValue(owner, out tracker)) return tracker;
            tracker = new AstralDevaStunTracker();
            Trackers.Add(owner, tracker);
            return tracker;
        }

        private static void Forget(UnitEntityData owner)
        {
            if (owner == null) return;
            lock (Trackers) { Trackers.Remove(owner); }
        }

        internal static AstralDevaStunTracker ObservedTracker(UnitEntityData owner)
        {
            if (owner == null) return null;
            AstralDevaStunTracker tracker;
            lock (Trackers)
            { return Trackers.TryGetValue(owner, out tracker) ? tracker : null; }
        }

        internal static void ForgetObserved(UnitEntityData owner)
        { Forget(owner); }
    }

    /// <summary>The lesser-globe half of one Astral Deva protective aura.
    /// The recipient buff's context identifies the exact emitter. Spells and
    /// SLAs of level three or lower that originate outside that emitter's
    /// twenty-foot aura are immune; an origin inside remains unaffected.</summary>
    [Serializable]
    public sealed class AstralProtectiveGlobe :
        RuleTargetLogicComponent<RuleSpellResistanceCheck>
    {
        public BlueprintUnit OwningBlueprint;
        public int MaximumSpellLevel;
        public int RadiusFeet;
        private static readonly FieldInfo TargetIsImmune =
            typeof(RuleSpellResistanceCheck).GetField(
                "<TargetIsImmune>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);

        public override void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
        { Apply(evt); }

        public override void OnEventDidTrigger(RuleSpellResistanceCheck evt)
        { Apply(evt); }

        private void Apply(RuleSpellResistanceCheck evt)
        {
            UnitEntityData target = Owner == null ? null : Owner.Unit;
            UnitEntityData emitter = Fact == null || Fact.MaybeContext == null ?
                null : Fact.MaybeContext.MaybeCaster;
            UnitEntityData source = evt == null || evt.Context == null ?
                null : evt.Context.MaybeCaster;
            BlueprintAbility ability = evt == null || evt.Context == null ?
                null : evt.Context.SourceAbility;
            if (evt == null || target == null || emitter == null || source == null ||
                ability == null || Fact == null || !Fact.Active ||
                OwningBlueprint == null || !ReferenceEquals(emitter.Blueprint, OwningBlueprint) ||
                MaximumSpellLevel != AstralDevaRulesPolicy.LesserGlobeMaximumSpellLevel ||
                RadiusFeet != AstralDevaRulesPolicy.AuraRadiusFeet ||
                evt.Context.Params == null ||
                !AstralDevaRulesPolicy.BlocksLesserGlobe(
                    evt.Context.Params.SpellLevel,
                    ability.Type == AbilityType.Spell,
                    ability.Type == AbilityType.SpellLike,
                    Vector3.Distance(emitter.Position, source.Position),
                    source.Corpulence))
                return;
            if (TargetIsImmune == null || TargetIsImmune.FieldType != typeof(bool) ||
                TargetIsImmune.IsInitOnly)
                throw new MissingFieldException(typeof(RuleSpellResistanceCheck).FullName,
                    "<TargetIsImmune>k__BackingField");
            TargetIsImmune.SetValue(evt, true);
        }
    }

    /// <summary>Use the exact resource-backed Heal SLA only for a living,
    /// friendly target with meaningful damage. The ability owns its action,
    /// targeting, resource debit and native healing effect.</summary>
    public sealed class AstralHealingEligibility : Consideration
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintAbility Heal;
        public int MinimumDamage;
        public override float Score(DecisionContext context)
        {
            UnitEntityData actor = context == null ? null : context.Unit;
            UnitEntityData target = context == null || context.Target == null ?
                null : context.Target.Unit;
            if (actor == null || target == null || Heal == null ||
                !ReferenceEquals(actor.Blueprint, OwningBlueprint) ||
                actor.IsEnemy(target) || target.Destroyed ||
                target.Descriptor.State.IsDead || target.Descriptor.IsUndead ||
                target.Damage < MinimumDamage) return 0f;
            var granted = actor.Descriptor.Abilities.GetAbility(Heal);
            if (granted == null) return 0f;
            var data = new AbilityData(granted);
            return data.IsAvailable && data.CanTarget(new TargetWrapper(target)) ? 1f : 0f;
        }
    }
}
