using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
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

    /// <summary>
    /// Kingmaker's ordinary invisibility remains the primary consumer for
    /// offensive spell/ability dispatch. Installed 2.1.7b does not raise that
    /// dispatch for a real UnitAttack in the measured first-area contract, so
    /// this exact-owner fallback ends only Bone Devil's private effect when its
    /// own hostile attack roll completes. Misses still count as attacks.
    /// </summary>
    [Serializable]
    public sealed class BoneOrdinaryInvisibility35 :
        RuleInitiatorLogicComponent<RuleAttackRoll>
    {
        public BlueprintUnit OwningBlueprint;

        private void Break(UnitEntityData target, bool exactActor)
        {
            UnitEntityData owner = Owner?.Unit;
            bool exactOwner = exactActor && owner?.Blueprint != null &&
                OwningBlueprint != null && ReferenceEquals(owner.Blueprint,
                    OwningBlueprint);
            if (!InfernalRules35.ShouldBreakBoneInvisibility(exactOwner,
                    Fact?.Active == true, owner?.IsInGame == true,
                    target?.IsInGame == true,
                    owner != null && target != null && owner.IsEnemy(target)))
                return;
            Owner.Buffs.RemoveFact(Fact);
        }

        public override void OnEventAboutToTrigger(RuleAttackRoll evt) { }

        public override void OnEventDidTrigger(RuleAttackRoll evt)
        {
            Break(evt?.Target, evt != null &&
                ReferenceEquals(evt.Initiator, Owner?.Unit));
        }
    }

    /// <summary>
    /// PF1 Unholy Blight resolution for one creature selected by the
    /// ability's native 20-foot burst. The ability framework owns range,
    /// action cost, resource debit, and per-target spell resistance. This
    /// action owns the alignment-sensitive dice, real Will save, and finite
    /// independently timed sickened effect.
    /// </summary>
    [Serializable]
    public sealed class ContextActionInfernalUnholyBlight35 : ContextAction
    {
        public BlueprintUnit SourceUnit;
        public BlueprintUnitFact OutsiderType;
        public BlueprintBuff Sickened;

        public override string GetCaption()
        { return "Resolve Barbed Devil Unholy Blight against one creature"; }

        public override void RunAction()
        {
            UnitEntityData caster = Context == null ? null :
                Context.MaybeCaster;
            UnitEntityData target = Target == null ? null : Target.Unit;
            if (caster == null || target == null || target.Descriptor == null ||
                target.Destroyed || target.Descriptor.State.IsDead ||
                SourceUnit == null || OutsiderType == null || Sickened == null ||
                !ReferenceEquals(caster.Blueprint, SourceUnit)) return;

            Alignment alignment = target.Descriptor.Alignment.Value;
            bool good = alignment.HasComponent(AlignmentComponent.Good);
            bool evil = alignment.HasComponent(AlignmentComponent.Evil);
            if (evil) return;
            bool goodOutsider = good && target.Descriptor.HasFact(OutsiderType);
            int dc = InfernalRules35.UnholyBlightWillDc(
                caster.Stats.Charisma.ModifiedValue);
            var resolution = new MechanicsContext(caster, target.Descriptor,
                Context.SourceAbility, Context, new TargetWrapper(target));
            resolution.Params.CasterLevel =
                InfernalRules35.SpellLikeCasterLevel;
            resolution.Params.SpellLevel =
                InfernalRules35.UnholyBlightSpellLevel;
            resolution.Params.DC = dc;
            var saving = new RuleSavingThrow(target, SavingThrowType.Will, dc) {
                Reason = resolution };
            resolution.TriggerRule(saving);

            DiceType die = goodOutsider ? DiceType.D6 : DiceType.D8;
            var roll = new RuleRollDice(caster, new DiceFormula(
                InfernalRules35.UnholyBlightDiceCount(goodOutsider), die)) {
                Reason = resolution };
            int rolled = resolution.TriggerRule(roll).Result;
            int amount = InfernalRules35.UnholyBlightDamage(rolled, good,
                evil, saving.IsPassed);
            var damage = new DirectDamage(
                new DiceFormula(0, DiceType.Zero), amount);
            // UnholyBlightDamage already owns every alignment/save fraction.
            var dealt = new RuleDealDamage(caster, target,
                new DamageBundle(new BaseDamage[] { damage })) {
                Reason = resolution,
                SourceAbility = Context.SourceAbility };
            resolution.TriggerRule(dealt);

            if (!InfernalRules35.UnholyBlightSickens(good, evil,
                    saving.IsPassed)) return;
            var durationRoll = new RuleRollDice(caster,
                new DiceFormula(1, DiceType.D4)) { Reason = resolution };
            int rounds = resolution.TriggerRule(durationRoll).Result;
            var sickenedContext = AstralVictimEffectContext.Create(caster,
                target, Sickened, new AbilityParams {
                    CasterLevel = InfernalRules35.SpellLikeCasterLevel,
                    SpellLevel = InfernalRules35.UnholyBlightSpellLevel,
                    DC = dc });
            var apply = new RuleApplyBuff(target, Sickened, sickenedContext,
                TimeSpan.FromSeconds(GameConsts.RoundDuration * rounds),
                (buff, source, duration) =>
                    target.Descriptor.Buffs.AddBuff(buff, source, duration));
            Rulebook.Trigger(apply);
            foreach (Buff buff in target.Buffs.Enumerable.Where(value =>
                ReferenceEquals(value.Blueprint, Sickened) &&
                ReferenceEquals(value.Context?.ParentContext,
                    sickenedContext)))
                buff.IsFromSpell = true;
        }
    }

    /// <summary>One small exact-owner AI wrapper for the legal SLA buttons.</summary>
    public sealed class InfernalSpellAi35 : BlueprintAiCastSpell
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintBuff ActiveBuff;
        public bool TargetSelf;
        public bool TargetEnemy = true;
        public bool AvoidFriendlyArea;
        public int AreaRadiusFeet;
        public bool WallPlacement;
        public BlueprintUnit WallSection;

        public override bool IsAvailable(DecisionContext context)
        {
            var owner = context?.Unit;
            bool exactOwner = owner?.Blueprint != null &&
                OwningBlueprint != null && ReferenceEquals(owner.Blueprint,
                    OwningBlueprint);
            bool hasActiveEffect = owner != null && ActiveBuff != null &&
                owner.Buffs.Enumerable.Any(buff => buff.IsTurnedOn &&
                    ReferenceEquals(buff.Blueprint, ActiveBuff));
            var granted = owner == null || Ability == null ? null :
                owner.Descriptor.Abilities.GetAbility(Ability);
            bool grantedAvailable = granted != null &&
                new AbilityData(granted).IsAvailable;
            if (!InfernalRules35.SpellAiCanAct(exactOwner,
                    owner?.IsInCombat == true, hasActiveEffect,
                    granted != null, grantedAvailable)) return false;
            return base.IsAvailable(context);
        }

        public override TargetType GetTargetType(DecisionContext context)
        { return TargetSelf ? TargetType.Self : TargetEnemy ? TargetType.Enemy :
            base.GetTargetType(context); }

        public override bool IsTargetValid(DecisionContext context)
        {
            var owner = context?.Unit;
            var target = context?.Target?.Unit;
            if (WallPlacement)
            {
                InfernalWallPlacement35 placement;
                var granted = owner == null || Ability == null ? null :
                    owner.Descriptor.Abilities.GetAbility(Ability);
                var data = granted == null ? null : new AbilityData(granted);
                return owner != null && target != null &&
                    InfernalWallPlacementRuntime35.TryPlanAi(owner, target,
                        WallSection, out placement) && data != null &&
                    data.IsAvailable && data.CanTarget(
                        new TargetWrapper(placement.Center));
            }
            if (owner == null || TargetSelf && target != null &&
                    !ReferenceEquals(owner, target) || TargetEnemy &&
                    (target == null || !owner.IsEnemy(target))) return false;
            if (AvoidFriendlyArea)
            {
                if (target == null || AreaRadiusFeet <= 0 ||
                    target.Descriptor.Alignment.Value.HasComponent(
                        AlignmentComponent.Evil) ||
                    Game.Instance?.State?.Units?.All == null) return false;
                float radius = new Feet(AreaRadiusFeet).Meters;
                if (Game.Instance.State.Units.All.Any(unit =>
                    unit != null && !unit.Destroyed && unit.IsInState &&
                    !ReferenceEquals(unit, owner) && !owner.IsEnemy(unit) &&
                    !unit.Descriptor.Alignment.Value.HasComponent(
                        AlignmentComponent.Evil) &&
                    (unit.Position - target.Position).magnitude <=
                        radius + unit.Corpulence)) return false;
            }
            return base.IsTargetValid(context);
        }

        public override UnitCommand CreateCommand(DecisionContext context,
            UnitEntityData target)
        {
            if (!WallPlacement) return base.CreateCommand(context, target);
            UnitEntityData owner = context?.Unit;
            InfernalWallPlacement35 placement;
            var granted = owner == null || Ability == null ? null :
                owner.Descriptor.Abilities.GetAbility(Ability);
            var data = granted == null ? null : new AbilityData(granted);
            if (data == null || !data.IsAvailable ||
                !InfernalWallPlacementRuntime35.TryPlanAi(owner, target,
                    WallSection, out placement) ||
                !data.CanTarget(new TargetWrapper(placement.Center)))
                return null;
            return new UnitUseAbility(data,
                new TargetWrapper(placement.Center));
        }
    }
}
