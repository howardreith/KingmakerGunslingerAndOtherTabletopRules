using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Immutable identity binding for the two Sprint 35 bodies.</summary>
    public sealed class InfernalMartialProfile35 : BlueprintComponent
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Bite;
        public BlueprintItemWeapon Claw;
        public BlueprintItemWeapon Sting;

        internal static InfernalMartialProfile35 For(UnitEntityData owner)
        {
            ModContext context;
            if (owner?.Blueprint == null || !ModContext.TryGet(out context) ||
                context.FeatureModules?.Active?.ExpandedSummoning != true)
                return null;
            var profile = owner.Blueprint.GetComponent<InfernalMartialProfile35>();
            return profile != null &&
                ReferenceEquals(profile.OwningBlueprint, owner.Blueprint) ?
                profile : null;
        }
    }

    /// <summary>
    /// The incoming hit predicate is deliberately independent from injury.
    /// It reads the attack's actual weapon definition, never the attacker's
    /// body size or current reach. Direct retaliation damage creates no new
    /// attack event and therefore cannot recurse.
    /// </summary>
    [Serializable]
    public sealed class BarbedDefense35 :
        RuleTargetLogicComponent<RuleAttackWithWeapon>
    {
        private static readonly ConditionalWeakTable<UnitEntityData,
            ConditionalWeakTable<RuleAttackWithWeapon, object>> Resolved =
            new ConditionalWeakTable<UnitEntityData,
                ConditionalWeakTable<RuleAttackWithWeapon, object>>();

        public BlueprintUnit OwningBlueprint;

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            var defender = Owner?.Unit;
            var attacker = evt?.Initiator;
            var weapon = evt?.Weapon;
            var blueprint = weapon?.Blueprint;
            if (defender == null || attacker == null || blueprint == null ||
                OwningBlueprint == null || !ReferenceEquals(defender.Blueprint,
                    OwningBlueprint) || !ReferenceEquals(evt.Target, defender) ||
                ReferenceEquals(attacker, defender) ||
                InfernalMartialProfile35.For(defender) == null)
                return;
            bool hit = evt.AttackRoll?.IsHit == true;
            bool melee = !blueprint.IsRanged;
            bool manufactured = !blueprint.IsNatural && !blueprint.IsUnarmed;
            float rawReach = blueprint.Type == null ? 0 :
                blueprint.Type.AttackRange.Value;
            float ordinaryReach = GameConsts.MinWeaponRange.Value;
            if (!InfernalRules35.QualifiesForBarbedDefense(hit, melee,
                    manufactured, rawReach, ordinaryReach)) return;
            var events = Resolved.GetValue(defender,
                unit => new ConditionalWeakTable<RuleAttackWithWeapon, object>());
            object previous;
            if (events.TryGetValue(evt, out previous)) return;
            events.Add(evt, new object());
            var damage = new PhysicalDamage(new DiceFormula(1, DiceType.D8),
                PhysicalDamageForm.Piercing);
            damage.AddBonus(defender.Stats.Strength.Bonus);
            damage.AddAlignment(DamageAlignment.Lawful);
            damage.AddAlignment(DamageAlignment.Evil);
            var rule = new RuleDealDamage(defender, attacker,
                new DamageBundle(damage));
            var context = Fact?.MaybeContext;
            if (context != null) context.TriggerRule(rule);
            else Rulebook.Trigger(rule);
        }

        public override void OnTurnOff()
        {
            if (Owner?.Unit != null) Resolved.Remove(Owner.Unit);
            base.OnTurnOff();
        }
    }

    /// <summary>Fear belongs only to a wounding outgoing claw attack.</summary>
    [Serializable]
    public sealed class BarbedClawFear35 :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        private static readonly ConditionalWeakTable<UnitEntityData,
            ConditionalWeakTable<RuleAttackWithWeapon, object>> Resolved =
            new ConditionalWeakTable<UnitEntityData,
                ConditionalWeakTable<RuleAttackWithWeapon, object>>();

        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Claw;
        public BlueprintBuff Frightened;

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            var owner = Owner?.Unit;
            var victim = evt?.Target;
            bool exact = evt?.Weapon?.Blueprint != null && Claw != null &&
                ReferenceEquals(evt.Weapon.Blueprint, Claw);
            bool hit = evt?.AttackRoll?.IsHit == true;
            int damage = evt?.MeleeDamage == null ? 0 :
                Math.Max(0, evt.MeleeDamage.Damage);
            if (owner == null || victim == null || Frightened == null ||
                OwningBlueprint == null || !ReferenceEquals(owner.Blueprint,
                    OwningBlueprint) || !ReferenceEquals(evt.Initiator, owner) ||
                !InfernalRules35.QualifiesForClawFear(exact, hit, damage) ||
                !SummonDiseaseExposure.IsAvailable(victim)) return;
            var events = Resolved.GetValue(owner,
                unit => new ConditionalWeakTable<RuleAttackWithWeapon, object>());
            object previous;
            if (events.TryGetValue(evt, out previous)) return;
            events.Add(evt, new object());
            int dc = InfernalRules35.FearDc(InfernalRules35.BarbedHitDice,
                owner.Stats.Charisma.Bonus,
                InfernalRules35.BarbedFearRacialDcBonus);
            var context = new MechanicsContext(owner, victim.Descriptor,
                Frightened, Fact?.MaybeContext, new TargetWrapper(victim));
            context.Params.DC = dc;
            var save = new RuleSavingThrow(victim, SavingThrowType.Will, dc)
                { Reason = context };
            context.TriggerRule(save);
            if (save.IsPassed) return;
            var duration = new RuleRollDice(owner,
                new DiceFormula(1, DiceType.D4)) { Reason = context };
            int rounds = context.TriggerRule(duration).Result;
            victim.Buffs.AddBuff(Frightened, context,
                TimeSpan.FromSeconds(rounds * 6d));
        }

        public override void OnTurnOff()
        {
            if (Owner?.Unit != null) Resolved.Remove(Owner.Unit);
            base.OnTurnOff();
        }
    }

    /// <summary>
    /// A source-specific aura exposure. A successful save grants the printed
    /// 24-hour immunity to that source. A failed save gets a marker for the
    /// exact rolled fear duration, preventing the area's round callback from
    /// becoming repeated saves while leaving another Bone Devil independent.
    /// </summary>
    [Serializable]
    public sealed class BoneFearAura35 : AbilityAreaEffectLogic
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintBuff Frightened;
        public BlueprintBuff SourceMemory;

        private void Expose(MechanicsContext context, AreaEffectEntityData area,
            UnitEntityData victim)
        {
            var source = context?.MaybeCaster;
            if (source == null || victim == null || Frightened == null ||
                SourceMemory == null || OwningBlueprint == null ||
                !ReferenceEquals(source.Blueprint, OwningBlueprint) ||
                ReferenceEquals(source, victim) || victim.Destroyed ||
                victim.Descriptor.State.IsDead || area == null || area.IsEnded ||
                ContextActionArchonMenace.HasSource(victim, SourceMemory, source))
                return;
            int dc = InfernalRules35.FearDc(InfernalRules35.BoneHitDice,
                source.Stats.Charisma.Bonus,
                InfernalRules35.BoneFearRacialDcBonus);
            var effect = new MechanicsContext(source, victim.Descriptor,
                Frightened, context, new TargetWrapper(victim));
            effect.Params.DC = dc;
            var save = new RuleSavingThrow(victim, SavingThrowType.Will, dc)
                { Reason = effect };
            effect.TriggerRule(save);
            if (save.IsPassed)
            {
                var memory = victim.Buffs.AddBuff(SourceMemory, effect,
                    TimeSpan.FromHours(24));
                if (memory != null) memory.IsNotDispelable = true;
                return;
            }
            var duration = new RuleRollDice(source,
                new DiceFormula(1, DiceType.D6)) { Reason = effect };
            int rounds = effect.TriggerRule(duration).Result;
            victim.Buffs.AddBuff(Frightened, effect,
                TimeSpan.FromSeconds(rounds * 6d));
            var failedMemory = victim.Buffs.AddBuff(SourceMemory, effect,
                TimeSpan.FromSeconds(rounds * 6d));
            if (failedMemory != null) failedMemory.IsNotDispelable = true;
        }

        protected override void OnUnitEnter(MechanicsContext context,
            AreaEffectEntityData area, UnitEntityData unit)
        { Expose(context, area, unit); }

        protected override void OnUnitMove(MechanicsContext context,
            AreaEffectEntityData area, UnitEntityData unit)
        { Expose(context, area, unit); }

        protected override void OnTick(MechanicsContext context,
            AreaEffectEntityData area)
        {
            if (area == null || area.IsEnded) return;
            foreach (var unit in area.UnitsInside.ToArray())
                Expose(context, area, unit);
        }

        protected override void OnUnitExit(MechanicsContext context,
            AreaEffectEntityData area, UnitEntityData unit) { }
    }

    /// <summary>Captures the delivering sting's DC before native poison ticks.</summary>
    [Serializable]
    public sealed class ContextActionSetBonePoisonDc35 : ContextAction
    {
        public override string GetCaption() { return "Set Bone Devil poison DC"; }
        public override void RunAction()
        {
            if (Context?.Params != null)
                Context.Params.DC = InfernalRules35.BonePoisonDc;
        }
    }

    /// <summary>One small exact-owner AI wrapper for the legal SLA buttons.</summary>
    public sealed class InfernalSpellAi35 : BlueprintAiCastSpell
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintBuff ActiveBuff;
        public bool TargetSelf;
        public bool TargetEnemy = true;

        public override bool IsAvailable(DecisionContext context)
        {
            var owner = context?.Unit;
            if (owner == null || OwningBlueprint == null ||
                !ReferenceEquals(owner.Blueprint, OwningBlueprint) ||
                !owner.IsInCombat || ActiveBuff != null &&
                owner.Buffs.Enumerable.Any(buff => buff.IsTurnedOn &&
                    ReferenceEquals(buff.Blueprint, ActiveBuff))) return false;
            return base.IsAvailable(context);
        }

        public override TargetType GetTargetType(DecisionContext context)
        { return TargetSelf ? TargetType.Self : TargetEnemy ? TargetType.Enemy :
            base.GetTargetType(context); }

        public override bool IsTargetValid(DecisionContext context)
        {
            var owner = context?.Unit;
            var target = context?.Target?.Unit;
            if (owner == null || TargetSelf && target != null &&
                    !ReferenceEquals(owner, target) || TargetEnemy &&
                    (target == null || !owner.IsEnemy(target))) return false;
            return base.IsTargetValid(context);
        }
    }
}
