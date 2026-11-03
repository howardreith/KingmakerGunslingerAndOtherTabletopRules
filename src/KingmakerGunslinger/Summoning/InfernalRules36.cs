using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Summoning
{
    public sealed class InfernalMartialProfile36 : BlueprintComponent
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Bite;
        public BlueprintItemWeapon Claw;
        public BlueprintItemWeapon Longspear;

        internal static InfernalMartialProfile36 For(UnitEntityData owner)
        {
            ModContext context;
            if (owner?.Blueprint == null || !ModContext.TryGet(out context) ||
                context.FeatureModules?.Active?.ExpandedSummoning != true)
                return null;
            var profile = owner.Blueprint.GetComponent<
                InfernalMartialProfile36>();
            return profile != null && ReferenceEquals(profile.OwningBlueprint,
                owner.Blueprint) ? profile : null;
        }
    }

    [HarmonyPatch(typeof(ItemEntityWeapon), "get_AttackRange")]
    internal static class InfernalWeaponReach36
    {
        private static void Postfix(ItemEntityWeapon __instance,
            ref Feet __result)
        {
            var profile = InfernalMartialProfile36.For(
                __instance?.Wielder?.Unit);
            if (profile?.Longspear != null &&
                ReferenceEquals(__instance.Blueprint, profile.Longspear))
                __result = new Feet(__result.Value + 4);
        }
    }

    [HarmonyPatch(typeof(UnitAttack), "CreateFullAttack")]
    internal static class BabauAlternativeRoutine36
    {
        private static void Postfix(UnitAttack __instance,
            List<AttackHandInfo> __result)
        {
            var owner = __instance?.Executor;
            var profile = InfernalMartialProfile36.For(owner);
            if (profile?.Longspear == null || profile.Claw == null ||
                __result == null || !ReferenceEquals(
                    owner.Body?.PrimaryHand?.MaybeWeapon?.Blueprint,
                    profile.Longspear)) return;
            // Armed: longspear iteratives plus the dynamically secondary bite.
            // Natural: two claws plus the bite. Never append both routines.
            __result.RemoveAll(attack => attack?.Hand?.MaybeWeapon != null &&
                ReferenceEquals(attack.Hand.MaybeWeapon.Blueprint,
                    profile.Claw));
        }
    }

    [Serializable]
    public sealed class ProtectiveSlime36 :
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
            UnitEntityData defender = Owner?.Unit;
            UnitEntityData attacker = evt?.Initiator;
            BlueprintItemWeapon weapon = evt?.Weapon?.Blueprint;
            bool exact = defender?.Blueprint != null &&
                OwningBlueprint != null &&
                ReferenceEquals(defender.Blueprint, OwningBlueprint) &&
                ReferenceEquals(evt?.Target, defender) &&
                InfernalMartialProfile36.For(defender) != null;
            bool qualifies = weapon != null &&
                InfernalRules36Policy.ProtectiveSlimeNaturalStrike(
                    evt.AttackRoll?.IsHit == true, !weapon.IsRanged,
                    weapon.IsNatural, weapon.IsUnarmed);
            if (!exact || !qualifies || attacker == null ||
                ReferenceEquals(attacker, defender)) return;
            var events = Resolved.GetValue(defender, unit =>
                new ConditionalWeakTable<RuleAttackWithWeapon, object>());
            object previous;
            if (events.TryGetValue(evt, out previous)) return;
            events.Add(evt, new object());
            int dc = InfernalRules36Policy.ConstitutionDc(
                InfernalRules36Policy.BabauHitDice,
                defender.Stats.Constitution.Bonus);
            var context = new MechanicsContext(defender,
                attacker.Descriptor, Fact?.Blueprint, Fact?.MaybeContext,
                new TargetWrapper(attacker));
            context.Params.DC = dc;
            var save = new RuleSavingThrow(attacker,
                SavingThrowType.Reflex, dc) { Reason = context };
            context.TriggerRule(save);
            if (save.IsPassed) return;
            var acid = new EnergyDamage(new DiceFormula(1, DiceType.D8),
                DamageEnergyType.Acid);
            context.TriggerRule(new RuleDealDamage(defender, attacker,
                new DamageBundle(acid)) { Reason = context });
        }

        public override void OnTurnOff()
        {
            if (Owner?.Unit != null) Resolved.Remove(Owner.Unit);
            base.OnTurnOff();
        }
    }

    public sealed class BabauWeaponModeAi36 : BlueprintAiSwitchWeapon
    {
        public BlueprintUnit OwningBlueprint;

        private int DesiredSet(UnitEntityData owner)
        {
            var profile = InfernalMartialProfile36.For(owner);
            if (profile?.Longspear == null || profile.Claw == null ||
                !ReferenceEquals(owner.Blueprint, OwningBlueprint) ||
                !owner.IsInCombat || Game.Instance?.State?.Units == null)
                return -1;
            float nearest = float.PositiveInfinity;
            foreach (var enemy in Game.Instance.State.Units.All)
            {
                if (enemy == null || enemy.Destroyed || !enemy.IsInState ||
                    enemy.Descriptor.State.IsDead || !owner.IsEnemy(enemy) ||
                    !owner.Memory.ContainsVisible(enemy) ||
                    !owner.HasLOS(enemy)) continue;
                nearest = Math.Min(nearest, owner.DistanceTo(enemy) -
                    owner.Corpulence - enemy.Corpulence);
            }
            if (float.IsPositiveInfinity(nearest)) return -1;
            bool disarmed = owner.Buffs.GetBuff(BlueprintRoot.Instance
                .SystemMechanics.DisarmMainHandBuff) != null;
            BlueprintItemWeapon wanted = nearest <= new Feet(5).Meters ||
                disarmed ? profile.Claw : profile.Longspear;
            if (ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon?.Blueprint,
                    wanted)) return -1;
            for (int index = 0;
                index < owner.Body.HandsEquipmentSets.Count; index++)
                if (ReferenceEquals(owner.Body.HandsEquipmentSets[index]
                        .PrimaryHand.MaybeItem?.Blueprint, wanted))
                    return index;
            return -1;
        }

        public override bool IsAvailable(DecisionContext context)
        { return DesiredSet(context?.Unit) >= 0; }

        public override bool IsExpended(DecisionContext context)
        { return false; }

        public override UnitCommand CreateCommand(DecisionContext context,
            UnitEntityData target)
        {
            int index = DesiredSet(context?.Unit);
            return index < 0 ? null : new UnitSwitchHandEquipmentSet(index);
        }
    }

    public sealed class BabauDispelAi36 : BlueprintAiCastSpell
    {
        private sealed class State { internal TimeSpan Next; }
        private static readonly ConditionalWeakTable<UnitEntityData, State>
            States = new ConditionalWeakTable<UnitEntityData, State>();
        public BlueprintUnit OwningBlueprint;

        /// <summary>
        /// Whether one effect is harmful to the unit carrying it, from the two
        /// native facts the installed build actually provides: the blueprint's
        /// own <c>Harmful</c> flag, and whether the effect's caster is an enemy
        /// of its owner. Nothing in the engine classifies this for a dispel
        /// consumer, so the Babau's own selector has to.
        /// </summary>
        internal static bool HarmfulToOwner(Buff buff, UnitEntityData owner)
        {
            if (buff?.Blueprint == null) return true;
            if (buff.Blueprint.Harmful) return true;
            UnitEntityData caster = buff.Context?.MaybeCaster;
            return caster != null && owner != null && caster.IsEnemy(owner);
        }

        internal static bool HasUsefulEffect(UnitEntityData target)
        {
            return target?.Descriptor?.Buffs != null &&
                target.Buffs.Enumerable.Any(buff =>
                    InfernalRules36Policy.UsefulDispelEffect(
                        buff?.IsTurnedOn == true,
                        buff?.Blueprint?.IsHiddenInUI == false,
                        buff?.IsFromSpell == true,
                        buff?.IsNotDispelable == true,
                        buff?.Context != null,
                        HarmfulToOwner(buff, target)));
        }

        public override TargetType GetTargetType(DecisionContext context)
        { return TargetType.Enemy; }

        public override bool IsAvailable(DecisionContext context)
        {
            UnitEntityData owner = context?.Unit;
            if (owner?.Blueprint == null || OwningBlueprint == null ||
                !ReferenceEquals(owner.Blueprint, OwningBlueprint) ||
                !owner.IsInCombat) return false;
            State state;
            if (States.TryGetValue(owner, out state) &&
                Game.Instance.TimeController.GameTime < state.Next)
                return false;
            var granted = Ability == null ? null :
                owner.Descriptor.Abilities.GetAbility(Ability);
            if (granted == null || !new AbilityData(granted).IsAvailable ||
                Game.Instance?.State?.Units == null) return false;
            if (!Game.Instance.State.Units.All.Any(unit => unit != null &&
                    !unit.Destroyed && unit.IsInState &&
                    !unit.Descriptor.State.IsDead && owner.IsEnemy(unit) &&
                    owner.Memory.ContainsVisible(unit) && owner.HasLOS(unit) &&
                    HasUsefulEffect(unit))) return false;
            return base.IsAvailable(context);
        }

        public override bool IsTargetValid(DecisionContext context)
        {
            UnitEntityData owner = context?.Unit;
            UnitEntityData target = context?.Target?.Unit;
            return owner != null && target != null && owner.IsEnemy(target) &&
                owner.Memory.ContainsVisible(target) && owner.HasLOS(target) &&
                HasUsefulEffect(target) && base.IsTargetValid(context);
        }

        public override UnitCommand CreateCommand(DecisionContext context,
            UnitEntityData target)
        {
            UnitEntityData owner = context?.Unit;
            UnitCommand command = base.CreateCommand(context, target);
            if (owner != null && command != null)
                States.GetValue(owner, unit => new State()).Next =
                    Game.Instance.TimeController.GameTime +
                    TimeSpan.FromSeconds(GameConsts.RoundDuration);
            return command;
        }
    }

    [Serializable]
    public sealed class ContextActionInfernalChaosHammer36 : ContextAction
    {
        public BlueprintUnit SourceUnit;
        public BlueprintUnitFact OutsiderType;
        public BlueprintBuff Slowed;
        public int CasterLevel;
        public int SpellLevel;
        public int DamageDiceCount;
        public int OutsiderDamageDiceCount;

        public override string GetCaption()
        { return "Resolve Chaos Hammer against one creature"; }

        public override void RunAction()
        {
            UnitEntityData caster = Context?.MaybeCaster;
            UnitEntityData target = Target?.Unit;
            if (caster == null || target?.Descriptor == null ||
                target.Destroyed || target.Descriptor.State.IsDead ||
                SourceUnit == null || OutsiderType == null ||
                Slowed == null || !ReferenceEquals(caster.Blueprint,
                    SourceUnit)) return;
            Alignment alignment = target.Descriptor.Alignment.Value;
            bool lawful = alignment.HasComponent(AlignmentComponent.Lawful);
            bool chaotic = alignment.HasComponent(AlignmentComponent.Chaotic);
            if (chaotic) return;
            bool lawfulOutsider = lawful &&
                target.Descriptor.HasFact(OutsiderType);
            int dc = 10 + SpellLevel + caster.Stats.Charisma.Bonus;
            var resolution = new MechanicsContext(caster,
                target.Descriptor, Context.SourceAbility, Context,
                new TargetWrapper(target));
            resolution.Params.CasterLevel = CasterLevel;
            resolution.Params.SpellLevel = SpellLevel;
            resolution.Params.DC = dc;
            var saving = new RuleSavingThrow(target,
                SavingThrowType.Will, dc) { Reason = resolution };
            resolution.TriggerRule(saving);
            DiceType die = lawfulOutsider ? DiceType.D6 : DiceType.D8;
            var roll = new RuleRollDice(caster, new DiceFormula(
                lawfulOutsider ? OutsiderDamageDiceCount : DamageDiceCount,
                die)) { Reason = resolution };
            int amount = InfernalRules36Policy.AlignmentBurstDamage(
                resolution.TriggerRule(roll).Result, lawful, chaotic,
                saving.IsPassed);
            var damage = new DirectDamage(new DiceFormula(0, DiceType.Zero),
                amount);
            resolution.TriggerRule(new RuleDealDamage(caster, target,
                new DamageBundle(new BaseDamage[] { damage })) {
                    Reason = resolution,
                    SourceAbility = Context.SourceAbility });
            if (!InfernalRules36Policy.AlignmentBurstCondition(lawful,
                    chaotic, saving.IsPassed)) return;
            var duration = new RuleRollDice(caster,
                new DiceFormula(1, DiceType.D6)) { Reason = resolution };
            int rounds = resolution.TriggerRule(duration).Result;
            var slowContext = AstralVictimEffectContext.Create(caster,
                target, Slowed, new AbilityParams {
                    CasterLevel = CasterLevel,
                    SpellLevel = SpellLevel,
                    DC = dc });
            var apply = new RuleApplyBuff(target, Slowed, slowContext,
                TimeSpan.FromSeconds(GameConsts.RoundDuration * rounds),
                (buff, source, span) =>
                    target.Descriptor.Buffs.AddBuff(buff, source, span));
            Rulebook.Trigger(apply);
            foreach (Buff buff in target.Buffs.Enumerable.Where(value =>
                ReferenceEquals(value.Blueprint, Slowed) &&
                ReferenceEquals(value.Context?.ParentContext, slowContext)))
                buff.IsFromSpell = true;
        }
    }

    [Serializable]
    public sealed class HezrouStench36 : AbilityAreaEffectLogic
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintBuff Sickened;
        public BlueprintBuff SourceMemory;
        public BlueprintUnitFact PoisonImmunity;

        private void Expose(MechanicsContext context,
            AreaEffectEntityData area, UnitEntityData victim)
        {
            UnitEntityData source = context?.MaybeCaster;
            bool available = SummonDiseaseExposure.IsAvailable(victim) &&
                area != null && !area.IsEnded;
            bool exact = source?.Blueprint != null &&
                OwningBlueprint != null &&
                ReferenceEquals(source.Blueprint, OwningBlueprint) &&
                !ReferenceEquals(source, victim);
            bool immune = victim?.Descriptor != null &&
                PoisonImmunity != null &&
                victim.Descriptor.HasFact(PoisonImmunity);
            bool remembered = source != null && victim != null &&
                SourceMemory != null && ContextActionArchonMenace.HasSource(
                    victim, SourceMemory, source);
            if (!InfernalRules36Policy.CanExposeStench(available, exact,
                    immune, remembered) || Sickened == null ||
                SourceMemory == null) return;
            int dc = InfernalRules36Policy.ConstitutionDc(
                InfernalRules36Policy.HezrouHitDice,
                source.Stats.Constitution.Bonus);
            var effect = new MechanicsContext(source, victim.Descriptor,
                Sickened, context, new TargetWrapper(victim));
            effect.Params.DC = dc;
            var save = new RuleSavingThrow(victim,
                SavingThrowType.Fortitude, dc) { Reason = effect };
            effect.TriggerRule(save);
            TimeSpan memoryDuration = save.IsPassed ?
                TimeSpan.FromHours(
                    InfernalRules36Policy.SuccessfulStenchMemoryHours) :
                TimeSpan.FromSeconds(GameConsts.RoundDuration *
                    InfernalRules36Policy.StenchSickenedRounds);
            if (!save.IsPassed)
            {
                var apply = new RuleApplyBuff(victim, Sickened, effect,
                    memoryDuration, (buff, sourceContext, duration) =>
                        victim.Descriptor.Buffs.AddBuff(buff, sourceContext,
                            duration));
                Rulebook.Trigger(apply);
            }
            Buff marker = victim.Buffs.AddBuff(SourceMemory, effect,
                memoryDuration);
            if (marker != null) marker.IsNotDispelable = true;
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
            foreach (UnitEntityData unit in area.UnitsInside.ToArray())
                Expose(context, area, unit);
        }

        protected override void OnUnitExit(MechanicsContext context,
            AreaEffectEntityData area, UnitEntityData unit) { }
    }

    [Serializable]
    public sealed class HezrouHeldNausea36 :
        OwnedGameLogicComponent<UnitDescriptor>,
        ITickEachRound
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintBuff Nauseated;
        public BlueprintUnitFact PoisonImmunity;

        private static IEnumerable<Buff> Owned(UnitEntityData victim,
            BlueprintBuff blueprint, UnitEntityData source)
        {
            return victim.Buffs.Enumerable.Where(buff =>
                ReferenceEquals(buff.Blueprint, blueprint) &&
                ReferenceEquals(buff.Context?.MaybeCaster, source)).ToArray();
        }

        public void OnNewRound()
        {
            UnitEntityData source = Owner?.Unit;
            SummonGrabComponent grab = SummonGrabComponent.Find(source);
            if (source?.Blueprint == null || OwningBlueprint == null ||
                !ReferenceEquals(source.Blueprint, OwningBlueprint) ||
                grab?.GrappledBuff == null || Nauseated == null) return;
            foreach (UnitEntityData victim in SummonMultiHoldComponent
                .HeldTargets(source, grab.GrappledBuff))
            {
                bool immune = PoisonImmunity != null &&
                    victim.Descriptor.HasFact(PoisonImmunity);
                if (!InfernalRules36Policy.CanTestHeldNausea(
                        SummonDiseaseExposure.IsAvailable(victim), true, true,
                        immune)) continue;
                int dc = InfernalRules36Policy.ConstitutionDc(
                    InfernalRules36Policy.HezrouHitDice,
                    source.Stats.Constitution.Bonus);
                var effect = new MechanicsContext(source,
                    victim.Descriptor, Nauseated, Fact?.MaybeContext,
                    new TargetWrapper(victim));
                effect.Params.DC = dc;
                var save = new RuleSavingThrow(victim,
                    SavingThrowType.Fortitude, dc) { Reason = effect };
                effect.TriggerRule(save);
                foreach (Buff existing in Owned(victim, Nauseated, source))
                    victim.Descriptor.Buffs.RemoveFact(existing);
                if (save.IsPassed) continue;
                TimeSpan recovery = TimeSpan.FromSeconds(
                    GameConsts.RoundDuration *
                    InfernalRules36Policy.HeldNauseaRecoveryRounds);
                var apply = new RuleApplyBuff(victim, Nauseated, effect,
                    recovery, (buff, sourceContext, duration) =>
                        victim.Descriptor.Buffs.AddBuff(buff, sourceContext,
                            duration));
                Rulebook.Trigger(apply);
            }
        }
    }

    /// <summary>
    /// Keeps a failed held-nausea result alive while its exact source still
    /// holds the victim. The first round boundary that observes release starts
    /// the printed one-minute continuously-free recovery clock. The source's
    /// round observer still owns later saves and removes only its own instances
    /// on success; independent Hezrou sources therefore never cure each other.
    /// </summary>
    [Serializable]
    public sealed class HezrouHeldNauseaDuration36 : BuffLogic,
        ITickEachRound
    {
        [JsonProperty] private bool wasHeld;

        private bool IsHeld()
        {
            UnitEntityData source = Buff?.Context?.MaybeCaster;
            UnitEntityData victim = Owner?.Unit;
            SummonGrabComponent grab = SummonGrabComponent.Find(source);
            return source != null && victim != null &&
                grab?.GrappledBuff != null && ReferenceEquals(
                    SummonHeldComponent.HolderOf(victim,
                        grab.GrappledBuff), source);
        }

        private void ExtendFromNow()
        {
            if (Buff == null || Game.Instance?.TimeController == null) return;
            Buff.EndTime = Game.Instance.TimeController.GameTime +
                TimeSpan.FromSeconds(GameConsts.RoundDuration *
                    InfernalRules36Policy.HeldNauseaRecoveryRounds);
        }

        public override void OnTurnOn()
        {
            base.OnTurnOn();
            wasHeld = IsHeld();
            if (wasHeld) ExtendFromNow();
        }

        public void OnNewRound()
        {
            if (IsHeld())
            {
                wasHeld = true;
                ExtendFromNow();
            }
            else if (wasHeld)
            {
                wasHeld = false;
                ExtendFromNow();
            }
        }
    }
}
