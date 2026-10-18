using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Harmony12;
using UnityEngine;
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
        internal bool EligibleAtStart;
        internal Vector3 StartPosition;
        internal float TravelAtQueue;
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
            var attempt = new BuletteLeapAttempt {
                Context = new WeakReference<AbilityExecutionContext>(context),
                Ability = ability, Claw = claw, Target = context.MainTarget?.Unit,
                StartPosition = owner.Position
            };
            Armed.Add(owner, attempt);
            var charge = ability.GetComponent<AbilityCustomCharge>();
            attempt.EligibleAtStart = charge != null && attempt.Target != null &&
                charge.CanTarget(owner, context.MainTarget) && BuletteLeapChargeGeometry.HasRunup(owner, attempt.Target);
            if (!attempt.EligibleAtStart) return;
            int bonus = Sprint26MagicalBeastRules.JumpOnlyBonus(owner.Stats.Speed.ModifiedValue);
            var check = new RuleSkillCheck(owner, StatType.SkillMobility, 20) {
                IgnoreDifficultyBonusToDC = true, Reason = context
            };
            check.Bonus.BaseValue = bonus;
            context.TriggerRule(check);
            attempt.Check = check; attempt.JumpBonus = bonus;
        }

        internal static bool BindQueued(UnitAttack command)
        {
            if (command == null || !command.IsCharge || command.Executor == null) return true;
            BuletteLeapAttempt attempt;
            if (!Armed.TryGetValue(command.Executor, out attempt)) return true;
            AbilityExecutionContext context;
            if (!attempt.Context.TryGetTarget(out context) ||
                !ReferenceEquals(context.AbilityBlueprint, attempt.Ability) ||
                !ReferenceEquals(context.Caster, command.Executor) ||
                !ReferenceEquals(command.Target, attempt.Target) ||
                (attempt.Command != null && !ReferenceEquals(attempt.Command, command))) return true;
            attempt.Command = command;
            attempt.TravelAtQueue = Vector3.Distance(attempt.StartPosition, command.Executor.Position);
            BuletteLeapAttempt recorded;
            if (!Commands.TryGetValue(command, out recorded)) Commands.Add(command, attempt);
            // An enemy can move inward during real-time charge. Native charge
            // target validation alone permits a sub-ten-foot run-up with this
            // Huge body's reach. Retain its spent parent action but withhold
            // the now-illegal strike; never grant four claws or a free attack.
            if (!attempt.EligibleAtStart || !Sprint26MagicalBeastRules.ChargeTravelIsLegal(attempt.TravelAtQueue)) {
                command.Interrupt(true);
                return false;
            }
            return true;
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
            if (attempt.Check != null && attempt.Check.IsPassed && Sprint26MagicalBeastRules.ChargeTravelIsLegal(attempt.TravelAtQueue)) command.ForceFullAttack = true;
        }

        internal static void Substitute(UnitAttack command, ref List<AttackHandInfo> attacks)
        {
            BuletteLeapAttempt attempt;
            if (command == null || !command.IsCharge || !Commands.TryGetValue(command, out attempt) ||
                attempt.Check == null || !attempt.Check.IsPassed || !Sprint26MagicalBeastRules.ChargeTravelIsLegal(attempt.TravelAtQueue)) return;
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
    public sealed class BuletteLeapChargeGeometry : BlueprintComponent, IAbilityTargetChecker
    {
        public BlueprintUnit OwningBlueprint;
        public bool CanTarget(UnitEntityData caster, Kingmaker.Utility.TargetWrapper target)
        { return caster != null && ReferenceEquals(caster.Blueprint, OwningBlueprint) && target?.Unit != null && HasRunup(caster, target.Unit); }
        internal static bool HasRunup(UnitEntityData caster, UnitEntityData target)
        {
            if (caster?.View == null || target == null || target.Destroyed || !target.IsInState) return false;
            // Inspected native Init only builds the attack list, records target
            // consciousness and calculates this approach radius. It neither
            // queues nor runs an attack. Retain current native reach/radii.
            var probe = new UnitAttack(target); probe.Init(caster);
            return Sprint26MagicalBeastRules.ChargeHasRunup(caster.DistanceTo(target), probe.ApproachRadius);
        }
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
    { private static bool Prefix(UnitCommand __0) { return BuletteLeapRuntime.BindQueued(__0 as UnitAttack); } }

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

    [HarmonyPatch(typeof(Kingmaker.Items.ItemEntityWeapon), "get_AttackRange")]
    internal static class Sprint26NaturalReachPatch
    {
        private static void Postfix(Kingmaker.Items.ItemEntityWeapon __instance, ref Kingmaker.Utility.Feet __result)
        {
            KingmakerGunslinger.Bootstrap.ModContext context;
            if (__instance?.Wielder?.Unit?.Blueprint == null || __instance.Blueprint == null ||
                !KingmakerGunslinger.Bootstrap.ModContext.TryGet(out context) ||
                context.FeatureModules?.Active == null || !context.FeatureModules.Active.ExpandedSummoning) return;
            int added = Sprint26MagicalBeastRules.ReachAdjustment(
                __instance.Wielder.Unit.Blueprint.AssetGuid, __instance.Blueprint.AssetGuid);
            if (added != 0) __result = new Kingmaker.Utility.Feet(__result.Value + added);
        }
    }

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
