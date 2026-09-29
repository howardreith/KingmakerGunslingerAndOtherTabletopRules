using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.Items.Slots;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.BodyguardFeats;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// A contact action for the native overrun delivery. The delivery moves
    /// the unit and invokes its Actions for every contacted creature; this
    /// action replaces the native CMB result with the printed trample rule.
    /// It is installed only on hidden Sprint 11 summons until the movement,
    /// AoO choice and round cadence have been qualified in both combat modes.
    /// </summary>
    [Serializable]
    public sealed class ContextActionUngulateTrample : ContextAction
    {
        private static readonly string[] ExactTrampleAbilityNames = {
            "KMG_Summoning_Special_Aurochs_Trample",
            "KMG_Summoning_Special_Bison_Trample",
            "KMG_Summoning_Special_WoollyRhinoceros_Trample"
        };

        private static readonly ConditionalWeakTable<UnitEntityData,
            TrampleRoundLedger> Ledgers = new ConditionalWeakTable<
                UnitEntityData, TrampleRoundLedger>();

        public BlueprintUnit SourceUnit;
        public string CreatureKey;

        public override string GetCaption()
        { return "Apply printed ungulate trample at one native path contact"; }

        public override void RunAction()
        {
            UnitEntityData caster = Context == null ? null :
                Context.MaybeCaster;
            UnitEntityData target = Target == null ? null : Target.Unit;
            if (caster == null || target == null || SourceUnit == null ||
                !ReferenceEquals(caster.Blueprint, SourceUnit) ||
                caster.Descriptor == null || target.Descriptor == null ||
                caster.Destroyed || target.Destroyed ||
                target.Descriptor.State.IsDead ||
                ReferenceEquals(caster, target) || !target.IsEnemy(caster) ||
                string.IsNullOrEmpty(target.UniqueId)) return;

            UngulateRulesProfile rules = UngulateRulesPolicy.For(CreatureKey);
            int activeStampedeGroup = rules.Stampede ?
                UngulateStampedeRuntime.ActiveGroupSize(caster) : 0;
            if (!rules.CanTrample((int)caster.Descriptor.State.Size,
                    (int)target.Descriptor.State.Size,
                    activeStampedeGroup)) return;

            long round = CombatController.IsInTurnBasedCombat() &&
                Game.Instance.TurnBasedCombatController != null ?
                Game.Instance.TurnBasedCombatController.RoundNumber :
                Game.Instance.Player.GameTime.Ticks /
                    TimeSpan.FromSeconds(6d).Ticks;
            TrampleRoundLedger ledger = Ledgers.GetOrCreateValue(caster);
            if (!ledger.TryClaim(round, target.UniqueId)) return;

            TrampleTargetResponseDecision response = ResolveTargetResponse(
                target, caster);
            if (response ==
                TrampleTargetResponseDecision.OpportunityAttackStops)
            {
                ledger.Halt(round);
                InterruptExactTrampleCommand(caster);
                return;
            }

            bool half = false;
            if (response == TrampleTargetResponseDecision.ReflexSave)
            {
                var save = new RuleSavingThrow(target,
                    SavingThrowType.Reflex, rules.TrampleSaveDc(
                        activeStampedeGroup));
                Context.TriggerRule(save);
                half = save.IsPassed;
            }

            DiceType die = rules.TrampleDieSides == 6 ? DiceType.D6 :
                throw new InvalidOperationException(
                    "Unexpected ungulate trample die: " + CreatureKey);
            var damage = new PhysicalDamage(new DiceFormula(
                rules.TrampleDiceCount, die),
                PhysicalDamageForm.Bludgeoning);
            damage.AddBonus(rules.TrampleBonus);
            damage.Half = half;
            var dealt = new RuleDealDamage(caster, target, damage) {
                HalfBecauseSavingThrow = half
            };
            Context.TriggerRule(dealt);
        }

        internal static TrampleTargetResponseDecision ResolveTargetResponse(
            UnitEntityData defender, UnitEntityData trampler)
        {
            WeaponSlot hand = null;
            try
            { hand = UnitEngagementExtension.GetThreatHand(defender); }
            catch { }

            bool hasMeleeAttack = hand != null && hand.HasWeapon &&
                hand.Weapon != null && hand.Weapon.Blueprint != null &&
                hand.Weapon.Blueprint.IsMelee;
            bool threatens = false;
            if (hasMeleeAttack)
            {
                try
                {
                    threatens = UnitEngagementExtension.IsReach(defender,
                        trampler, hand);
                }
                catch { }
            }

            int remaining;
            bool nativePermits = BodyguardActionEconomyAccess
                .CanSpendAttackOfOpportunity(defender, trampler,
                    out remaining);
            bool canAct = defender.Descriptor != null &&
                defender.Descriptor.State != null &&
                defender.Descriptor.State.CanAct &&
                defender.Descriptor.State.IsConscious &&
                !defender.Descriptor.State.IsDead;
            bool legal = TrampleTargetResponsePolicy.HasLegalOpportunityAttack(
                remaining > 0, canAct, hasMeleeAttack, threatens,
                nativePermits);
            bool executed = false;
            if (legal)
            {
                int before, after;
                executed = BodyguardActionEconomyAccess
                    .TrySpendAttackOfOpportunity(defender, trampler,
                        out before, out after) && before > 0 &&
                    after == before - 1;
                if (executed)
                {
                    // Kingmaker has no interactive contact-response prompt.
                    // Resolve the owner-authorized automatic branch now so
                    // lethal/control results precede this contact's damage.
                    var attack = new RuleAttackWithWeapon(defender, trampler,
                        hand.Weapon, 4) {
                        IsAttackOfOpportunity = true
                    };
                    Rulebook.Trigger(attack);
                }
            }

            return TrampleTargetResponsePolicy.Resolve(legal, executed,
                TramplerCanContinue(trampler));
        }

        private static bool TramplerCanContinue(UnitEntityData trampler)
        {
            return trampler != null && !trampler.Destroyed &&
                trampler.HPLeft > 0 &&
                trampler.Descriptor != null &&
                trampler.Descriptor.State != null &&
                !trampler.Descriptor.State.IsDead &&
                trampler.Descriptor.State.IsConscious &&
                trampler.Descriptor.State.CanAct &&
                trampler.Descriptor.State.CanMove;
        }

        internal static bool IsExactTrampleCommand(UnitUseAbility command)
        {
            string name = command == null || command.Spell == null ||
                command.Spell.Blueprint == null ? null :
                command.Spell.Blueprint.name;
            return !string.IsNullOrEmpty(name) &&
                ExactTrampleAbilityNames.Contains(name,
                    StringComparer.Ordinal);
        }

        private static bool HasActiveExactTrampleCommand(
            UnitEntityData trampler)
        {
            if (trampler == null || trampler.Commands == null) return false;
            try
            {
                return trampler.Commands.Raw.OfType<UnitUseAbility>().Any(
                    command => IsExactTrampleCommand(command) &&
                        ReferenceEquals(command.Executor, trampler) &&
                        command.IsRunning && !command.IsFinished);
            }
            catch { return false; }
        }

        private static void InterruptExactTrampleCommand(
            UnitEntityData trampler)
        {
            if (trampler == null || trampler.Commands == null) return;
            UnitUseAbility[] commands;
            try
            {
                commands = trampler.Commands.Raw.OfType<UnitUseAbility>()
                    .Where(command => IsExactTrampleCommand(command) &&
                        ReferenceEquals(command.Executor, trampler) &&
                        !command.IsFinished).ToArray();
            }
            catch { return; }
            foreach (UnitUseAbility command in commands)
                command.Interrupt(true);
        }

        internal static bool SuppressDuplicateMovementOpportunityAttack(
            UnitEntityData defender, UnitEntityData trampler)
        {
            if (defender == null || trampler == null ||
                string.IsNullOrEmpty(defender.UniqueId) ||
                !HasActiveExactTrampleCommand(trampler)) return false;
            TrampleRoundLedger ledger;
            return Ledgers.TryGetValue(trampler, out ledger) &&
                ledger.HasClaim(CurrentRound(), defender.UniqueId);
        }

        internal static long CurrentRound()
        {
            return CombatController.IsInTurnBasedCombat() &&
                Game.Instance.TurnBasedCombatController != null ?
                Game.Instance.TurnBasedCombatController.RoundNumber :
                Game.Instance.Player.GameTime.Ticks /
                    TimeSpan.FromSeconds(6d).Ticks;
        }
    }

    /// <summary>
    /// Session-only coordinated-action observation for Stampede. RTWP requires
    /// three simultaneously running exact native commands. Turn-based retains
    /// only commands that entered OnAction in the current native round and are
    /// still running or finished successfully. It never submits a command.
    /// </summary>
    internal static class UngulateStampedeRuntime
    {
        private sealed class Registration
        {
            internal BlueprintUnit Unit;
            internal BlueprintAbility Ability;
        }

        private sealed class ExecutionState
        {
            internal long Round;
            internal UnitUseAbility Command;
        }

        private static readonly object Sync = new object();
        private static readonly Dictionary<string, Registration> Registrations =
            new Dictionary<string, Registration>(StringComparer.Ordinal);
        private static readonly ConditionalWeakTable<UnitEntityData,
            ExecutionState> TurnBasedExecutions = new ConditionalWeakTable<
                UnitEntityData, ExecutionState>();

        internal static void Register(BlueprintUnit unit,
            BlueprintAbility ability)
        {
            if (unit == null || ability == null ||
                string.IsNullOrEmpty(unit.AssetGuid))
                throw new ArgumentException(
                    "Stampede registration requires exact unit and ability identities.");
            lock (Sync)
                Registrations[unit.AssetGuid] = new Registration {
                    Unit = unit,
                    Ability = ability
                };
        }

        internal static void RecordAction(UnitUseAbility command)
        {
            UnitEntityData unit = command == null ? null : command.Executor;
            BlueprintAbility expected;
            if (!CombatController.IsInTurnBasedCombat() || unit == null ||
                !TryExpectedAbility(unit, out expected) ||
                command.Spell == null ||
                !ReferenceEquals(command.Spell.Blueprint, expected)) return;
            TurnBasedExecutions.Remove(unit);
            TurnBasedExecutions.Add(unit, new ExecutionState {
                Round = ContextActionUngulateTrample.CurrentRound(),
                Command = command
            });
        }

        internal static void Clear(UnitEntityData unit)
        {
            if (unit != null) TurnBasedExecutions.Remove(unit);
        }

        internal static int ActiveGroupSize(UnitEntityData actor)
        {
            if (actor == null || Game.Instance == null ||
                Game.Instance.State == null ||
                Game.Instance.State.Units == null) return 0;
            bool turnBased = CombatController.IsInTurnBasedCombat();
            long round = ContextActionUngulateTrample.CurrentRound();
            var candidates = Game.Instance.State.Units.All
                .Where(value => value != null).Distinct().ToList();
            if (!candidates.Contains(actor)) candidates.Add(actor);
            return StampedeFormationPolicy.QualifiedGroupSize(actor,
                candidates, value => IsEligible(actor, value, turnBased,
                    round), AreAdjacent);
        }

        private static bool IsEligible(UnitEntityData actor,
            UnitEntityData candidate, bool turnBased, long round)
        {
            BlueprintAbility expected;
            if (candidate == null || candidate.Destroyed ||
                !candidate.IsInGame || candidate.Descriptor == null ||
                candidate.Descriptor.State == null ||
                candidate.Descriptor.State.IsDead ||
                !candidate.Descriptor.State.IsConscious ||
                candidate.CombatState == null ||
                !candidate.CombatState.IsInCombat ||
                !actor.IsAlly(candidate) || !candidate.IsAlly(actor) ||
                !TryExpectedAbility(candidate, out expected) ||
                candidate.Descriptor.Abilities.GetAbility(expected) == null)
                return false;
            if (!turnBased) return HasRunningCommand(candidate, expected);
            ExecutionState state;
            if (!TurnBasedExecutions.TryGetValue(candidate, out state) ||
                state == null || state.Round != round ||
                state.Command == null ||
                !ReferenceEquals(state.Command.Executor, candidate) ||
                state.Command.Spell == null ||
                !ReferenceEquals(state.Command.Spell.Blueprint, expected))
                return false;
            return state.Command.IsRunning && !state.Command.IsFinished ||
                state.Command.IsFinished &&
                state.Command.Result == UnitCommand.ResultType.Success;
        }

        private static bool TryExpectedAbility(UnitEntityData unit,
            out BlueprintAbility ability)
        {
            ability = null;
            if (unit == null || unit.Blueprint == null ||
                string.IsNullOrEmpty(unit.Blueprint.AssetGuid)) return false;
            Registration registration;
            lock (Sync)
                if (!Registrations.TryGetValue(unit.Blueprint.AssetGuid,
                        out registration)) return false;
            if (registration == null ||
                !ReferenceEquals(registration.Unit, unit.Blueprint))
                return false;
            ability = registration.Ability;
            return ability != null;
        }

        private static bool HasRunningCommand(UnitEntityData unit,
            BlueprintAbility expected)
        {
            if (unit.Commands == null) return false;
            try
            {
                return unit.Commands.Raw.OfType<UnitUseAbility>().Any(command =>
                    ReferenceEquals(command.Executor, unit) &&
                    command.Spell != null &&
                    ReferenceEquals(command.Spell.Blueprint, expected) &&
                    command.IsRunning && !command.IsFinished);
            }
            catch { return false; }
        }

        private static bool AreAdjacent(UnitEntityData left,
            UnitEntityData right)
        {
            if (left == null || right == null) return false;
            float edgeDistance = Vector3.Distance(left.Position,
                right.Position) - Math.Max(0f, left.Corpulence) -
                Math.Max(0f, right.Corpulence);
            return edgeDistance <= 5.Feet().Meters + 0.01f;
        }
    }

    [HarmonyPatch(typeof(UnitUseAbility), "OnAction")]
    internal static class UngulateStampedeCommandActionPatch
    {
        private static void Prefix(UnitUseAbility __instance)
        { UngulateStampedeRuntime.RecordAction(__instance); }
    }

    [HarmonyPatch(typeof(UnitCombatState), "LeaveCombat")]
    internal static class UngulateStampedeLeaveCombatPatch
    {
        private static void Postfix(UnitCombatState __instance)
        {
            if (__instance != null)
                UngulateStampedeRuntime.Clear(__instance.Unit);
        }
    }

    /// <summary>
    /// Native Disengage must still remove engagement and raise its events. For
    /// an already-resolved contact in the exact active Trample command, only
    /// its ordinary movement AoO is temporarily disabled so Combat Reflexes
    /// cannot produce an ordinary-plus-special duplicate.
    /// </summary>
    [HarmonyPatch(typeof(UnitCombatState), "Disengage",
        new[] { typeof(UnitEntityData) })]
    internal static class UngulateTrampleDisengagePatch
    {
        private static void Prefix(UnitCombatState __instance,
            UnitEntityData __0, out bool __state)
        {
            __state = false;
            if (__instance == null || __instance.Unit == null ||
                __instance.PreventAttacksOfOpporunityNextFrame ||
                !ContextActionUngulateTrample
                    .SuppressDuplicateMovementOpportunityAttack(
                        __instance.Unit, __0)) return;
            __instance.PreventAttacksOfOpporunityNextFrame = true;
            __state = true;
        }

        private static void Postfix(UnitCombatState __instance, bool __state)
        {
            if (__state && __instance != null)
                __instance.PreventAttacksOfOpporunityNextFrame = false;
        }
    }

    /// <summary>
    /// AbilityCustomOverrun with AutoSuccess accepts targets beyond its own
    /// speed range. The printed trample is a full-round path of at most twice
    /// the creature's current speed, checked before the native forced path.
    /// </summary>
    [Serializable]
    public sealed class UngulateTramplePathChecker : BlueprintComponent,
        IAbilityTargetChecker
    {
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            if (caster == null || target == null || caster.View == null ||
                caster.CombatSpeedMps <= 0f) return false;
            float distance = Vector3.Distance(caster.Position, target.Point);
            return distance > caster.View.Corpulence &&
                distance <= caster.CombatSpeedMps * 12f;
        }
    }
}
