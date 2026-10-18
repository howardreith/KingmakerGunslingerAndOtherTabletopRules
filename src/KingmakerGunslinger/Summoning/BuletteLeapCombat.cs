using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class BuletteLeapAttempt
    {
        internal WeakReference<AbilityExecutionContext> Context;
        internal BlueprintAbility Ability;
        internal BlueprintItemWeapon Claw;
        internal UnitEntityData Target;
        internal UnitAttack Command;
        internal RuleSkillCheck Check;
        internal int JumpBonus;
        internal int PlannedClaws;
    }

    internal static class BuletteLeapRuntime
    {
        private static readonly ConditionalWeakTable<UnitEntityData, BuletteLeapAttempt>
            Armed = new ConditionalWeakTable<UnitEntityData, BuletteLeapAttempt>();
        private static readonly ConditionalWeakTable<UnitAttack, BuletteLeapAttempt>
            Commands = new ConditionalWeakTable<UnitAttack, BuletteLeapAttempt>();

        internal static void Begin(AbilityExecutionContext context, BlueprintUnit ownerBlueprint,
            BlueprintAbility ability, BlueprintItemWeapon claw)
        {
            var owner = context == null ? null : context.Caster;
            if (owner == null || !ReferenceEquals(owner.Blueprint, ownerBlueprint) ||
                !ReferenceEquals(context.AbilityBlueprint, ability) || claw == null)
                throw new InvalidOperationException("Bulette Leap requires its exact owner and charge ability.");
            Armed.Remove(owner);
            var charge = ability.GetComponent<AbilityCustomCharge>();
            if (charge == null || context.MainTarget == null || context.MainTarget.Unit == null ||
                !charge.CanTarget(owner, context.MainTarget)) return;
            // This modifier belongs to this real check alone. Ordinary Mobility
            // and every other creature's checks remain native and unchanged.
            int bonus = Sprint26MagicalBeastRules.JumpOnlyBonus(owner.Stats.Speed.ModifiedValue);
            var check = new RuleSkillCheck(owner, StatType.SkillMobility, 20) {
                IgnoreDifficultyBonusToDC = true, Reason = context
            };
            check.Bonus.BaseValue = bonus;
            context.TriggerRule(check);
            Armed.Add(owner, new BuletteLeapAttempt {
                Context = new WeakReference<AbilityExecutionContext>(context),
                Ability = ability, Claw = claw, Target = context.MainTarget.Unit,
                Check = check, JumpBonus = bonus
            });
        }

        internal static void BindQueued(UnitAttack command)
        {
            if (command == null || !command.IsCharge || command.Executor == null) return;
            BuletteLeapAttempt attempt;
            if (!Armed.TryGetValue(command.Executor, out attempt)) return;
            AbilityExecutionContext context;
            if (!attempt.Context.TryGetTarget(out context) ||
                !ReferenceEquals(context.AbilityBlueprint, attempt.Ability) ||
                !ReferenceEquals(context.Caster, command.Executor) ||
                !ReferenceEquals(command.Target, attempt.Target) ||
                (attempt.Command != null && !ReferenceEquals(attempt.Command, command))) return;
            attempt.Command = command;
            BuletteLeapAttempt recorded;
            if (!Commands.TryGetValue(command, out recorded)) Commands.Add(command, attempt);
        }

        internal static void Prepare(UnitAttack command)
        {
            if (command == null || !command.IsCharge || command.Executor == null) return;
            // Native charge queues its attack and then cleans up the ability.
            // Bind at that queue seam while the exact context is alive; later
            // OnStart owns only that one command and does not need a live source
            // context or an armed state that could leak to a future charge.
            BuletteLeapAttempt attempt;
            if (!Commands.TryGetValue(command, out attempt)) return;
            if (!ReferenceEquals(command.Target, attempt.Target) || !ReferenceEquals(command, attempt.Command)) return;
            if (attempt.Check.IsPassed) command.ForceFullAttack = true;
        }

        internal static void Substitute(UnitAttack command, ref List<AttackHandInfo> attacks)
        {
            BuletteLeapAttempt attempt;
            if (command == null || !command.IsCharge || !Commands.TryGetValue(command, out attempt) ||
                !attempt.Check.IsPassed) return;
            var claws = attacks.Where(row => row.Hand != null && row.Hand.MaybeWeapon != null &&
                ReferenceEquals(row.Hand.MaybeWeapon.Blueprint, attempt.Claw)).ToArray();
            if (claws.Length == 0)
                throw new InvalidOperationException("A successful Bulette Leap has no legal claw slot.");
            // Four equal primary attacks, never a bite or an added ordinary routine.
            var result = new List<AttackHandInfo>();
            for (int i = 0; i < 4; i++) result.Add(new AttackHandInfo(claws[i % claws.Length].Hand, 0, i));
            attacks = result;
            attempt.PlannedClaws = result.Count;
        }

        internal static BuletteLeapAttempt Observation(UnitAttack command)
        {
            BuletteLeapAttempt result;
            return command != null && Commands.TryGetValue(command, out result) ? result : null;
        }

        internal static void End(AbilityExecutionContext context)
        {
            if (context == null || context.Caster == null) return;
            BuletteLeapAttempt attempt;
            AbilityExecutionContext armed;
            if (Armed.TryGetValue(context.Caster, out attempt) &&
                attempt.Context.TryGetTarget(out armed) && ReferenceEquals(armed, context))
                Armed.Remove(context.Caster);
        }

        internal static bool IsArmed(UnitEntityData owner)
        { BuletteLeapAttempt value; return owner != null && Armed.TryGetValue(owner, out value); }
    }

    [Serializable]
    public sealed class BuletteLeapOnCast : BlueprintComponent, IAbilityOnCastLogic
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintAbility Leap;
        public BlueprintItemWeapon Claw;
        public void OnCast(AbilityExecutionContext context)
        { BuletteLeapRuntime.Begin(context, OwningBlueprint, Leap, Claw); }
    }

    [HarmonyPatch(typeof(UnitCommands), "AddToQueueFirst")]
    internal static class BuletteLeapQueuePatch
    { private static void Prefix(UnitCommand __0) { BuletteLeapRuntime.BindQueued(__0 as UnitAttack); } }

    [HarmonyPatch(typeof(UnitAttack), "InitAttacks")]
    internal static class BuletteLeapPreparePatch
    { private static void Prefix(UnitAttack __instance) { BuletteLeapRuntime.Prepare(__instance); } }

    [HarmonyPatch(typeof(UnitAttack), "CreateFullAttack")]
    internal static class BuletteLeapAttacksPatch
    {
        private static void Postfix(UnitAttack __instance, ref List<AttackHandInfo> __result)
        { BuletteLeapRuntime.Substitute(__instance, ref __result); }
    }

    [HarmonyPatch(typeof(AbilityCustomCharge), "Cleanup")]
    internal static class BuletteLeapCleanupPatch
    { private static void Postfix(AbilityExecutionContext context) { BuletteLeapRuntime.End(context); } }

    [Serializable]
    public sealed class MagicalBeastPrimaryStrength : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Weapon;
        public bool SavageBite;
        public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt) { }
        public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
            var owner = Owner == null ? null : Owner.Unit;
            if (evt == null || owner == null || !ReferenceEquals(owner.Blueprint, OwningBlueprint) ||
                !ReferenceEquals(evt.Initiator, owner) || evt.Weapon == null ||
                !ReferenceEquals(evt.Weapon.Blueprint, Weapon) || evt.Weapon.IsSecondary || evt.SecondaryWeapon ||
                evt.DamageBonusStat != StatType.Strength || evt.DamageDescription == null || evt.DamageDescription.Count == 0) return;
            int strength = owner.Stats.Strength.Bonus;
            int native = strength > 0 ? (int)(strength * evt.DamageBonusStatMultiplier) : strength;
            int desired = SavageBite ? Sprint26MagicalBeastRules.SavageBiteStrength(strength) : strength;
            var row = evt.DamageDescription[0];
            row.Bonus += desired - native;
            evt.DamageDescription[0] = row;
        }
    }
}
