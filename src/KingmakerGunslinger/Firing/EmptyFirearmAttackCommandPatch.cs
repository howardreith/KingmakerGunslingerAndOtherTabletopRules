using System;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Reloading;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace KingmakerGunslinger.Firing
{
    /// <summary>
    /// Rejects an empty firearm while UnitAttack.CreateAttackCommand is constructing
    /// the command, before a UnitAttack, animation, or attack rule exists.
    /// </summary>
    internal static class EmptyFirearmAttackCommandPatch
    {
        private static long _rejected;
        private static long _autoReloadReplacements;
        private static long _autoReloadResumedAttacks;
        private static long _autoReloadCanceledAttacks;
        private static long _evaluatedAttacks;
        private static readonly object PendingGate = new object();
        private static readonly Dictionary<UnitUseAbility, PendingAttack> Pending =
            new Dictionary<UnitUseAbility, PendingAttack>();
        internal static long Rejected { get { return Interlocked.Read(ref _rejected); } }
        internal static long AutoReloadReplacements
        { get { return Interlocked.Read(ref _autoReloadReplacements); } }
        internal static long AutoReloadResumedAttacks
        { get { return Interlocked.Read(ref _autoReloadResumedAttacks); } }
        internal static long AutoReloadCanceledAttacks
        { get { return Interlocked.Read(ref _autoReloadCanceledAttacks); } }
        internal static long EvaluatedAttacks
        { get { return Interlocked.Read(ref _evaluatedAttacks); } }

        internal static void Install(HarmonyInstance harmony)
        {
            if (harmony == null) throw new ArgumentNullException("harmony");
            MethodInfo prefix = typeof(EmptyFirearmAttackCommandPatch).GetMethod(
                "Prefix", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo create = typeof(UnitAttack).GetMethod("CreateAttackCommand",
                BindingFlags.Public | BindingFlags.Static, null,
                new[] { typeof(UnitEntityData), typeof(UnitEntityData) }, null);
            MethodInfo ended = typeof(UnitUseAbility).GetMethod("OnEnded",
                BindingFlags.NonPublic | BindingFlags.Instance, null,
                new[] { typeof(bool) }, null);
            MethodInfo endedPostfix = typeof(EmptyFirearmAttackCommandPatch).GetMethod(
                "ReloadEndedPostfix", BindingFlags.NonPublic | BindingFlags.Static);
            if (create == null || prefix == null || ended == null || endedPostfix == null)
                throw new MissingMethodException(
                    "Exact attack construction or reload completion contract was unavailable.");
            harmony.Patch(create, new HarmonyMethod(prefix), null, null);
            harmony.Patch(ended, null, new HarmonyMethod(endedPostfix), null);
        }

        private static bool Prefix(UnitEntityData __0, UnitEntityData __1,
            ref UnitCommand __result)
        {
            UnitEntityData executor = __0;
            if (executor == null || executor.Descriptor == null) return true;
            Interlocked.Increment(ref _evaluatedAttacks);
            ExactEquippedFirearmContext firearm;
            string reason;
            if (!ExactEquippedFirearmResolver.TryResolve(
                executor.Descriptor, out firearm, out reason))
            {
                if (reason != null && reason.IndexOf("ambiguous",
                    StringComparison.OrdinalIgnoreCase) >= 0)
                    return Reject(executor, __1,
                        EmptyFirearmCommandDisposition.RejectAmbiguous,
                        "Firearm attack rejected: equipped firearms are ambiguous.",
                        ref __result);
                return true;
            }
            bool autoReload = IsReloadAutoUse(executor);
            bool reloadLegal = autoReload &&
                executor.GetAvailableAutoUseAbility() != null;
            EmptyFirearmCommandDisposition disposition =
                EmptyFirearmAttackPolicy.Evaluate(true, false,
                    firearm.Firearm.Repository.State, autoReload, reloadLegal);
            if (disposition == EmptyFirearmCommandDisposition.Allow) return true;
            string message = disposition == EmptyFirearmCommandDisposition.QueueReload
                ? firearm.Firearm.ItemDisplayName + " is unloaded; auto-reload will run."
                : disposition == EmptyFirearmCommandDisposition.RejectWrecked
                ? firearm.Firearm.ItemDisplayName + " is Wrecked."
                : firearm.Firearm.ItemDisplayName + " is unloaded.";
            return Reject(executor, __1, disposition, message, ref __result);
        }

        private static bool IsReloadAutoUse(UnitEntityData executor)
        {
            if (executor == null || executor.AutoUseAbility == null ||
                BlueprintBootstrap.ReloadTestMusketAbility == null) return false;
            return ReferenceEquals(BlueprintBootstrap.ReloadTestMusketAbility,
                executor.AutoUseAbility.Blueprint);
        }

        private static bool Reject(UnitEntityData executor, UnitEntityData target,
            EmptyFirearmCommandDisposition disposition, string message,
            ref UnitCommand result)
        {
            Interlocked.Increment(ref _rejected);
            if (disposition == EmptyFirearmCommandDisposition.QueueReload)
                Interlocked.Increment(ref _autoReloadReplacements);
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Info("firearms", "attack.command-rejected",
                    message + " disposition=" + disposition);
            result = null;
            if (disposition == EmptyFirearmCommandDisposition.QueueReload &&
                executor != null)
            {
                var reload = executor.GetAvailableAutoUseAbility();
                if (reload != null)
                {
                    var command = new UnitUseAbility(reload,
                        new Kingmaker.Utility.TargetWrapper(executor));
                    lock (PendingGate)
                        Pending[command] = new PendingAttack(executor, target,
                            firearmWeapon: ResolveExactWeapon(executor));
                    result = command;
                }
            }
            return false;
        }

        private static Kingmaker.Items.ItemEntityWeapon ResolveExactWeapon(
            UnitEntityData executor)
        {
            ExactEquippedFirearmContext resolved;
            string reason;
            return executor != null && executor.Descriptor != null &&
                ExactEquippedFirearmResolver.TryResolve(executor.Descriptor,
                    out resolved, out reason) ? resolved.Weapon : null;
        }

        private static void ReloadEndedPostfix(UnitUseAbility __instance, bool __0)
        {
            CompletePending(__instance, __0, true, false);
        }

        internal static UnitCommand CompletePendingForRuntimeTest(UnitUseAbility command,
            bool interrupted)
        {
            return CompletePending(command, interrupted, false, true);
        }

        private static UnitCommand CompletePending(UnitUseAbility command, bool interrupted,
            bool schedule, bool runtimeObservedSuccess)
        {
            PendingAttack pending;
            lock (PendingGate)
            {
                if (command == null || !Pending.TryGetValue(command, out pending))
                    return null;
                Pending.Remove(command);
            }
            if (interrupted || (!runtimeObservedSuccess &&
                command.Result != UnitCommand.ResultType.Success))
            {
                Interlocked.Increment(ref _autoReloadCanceledAttacks);
                return null;
            }
            if (Kingmaker.Game.Instance == null)
            {
                Interlocked.Increment(ref _autoReloadCanceledAttacks);
                return null;
            }
            if (schedule)
            {
                Kingmaker.Game.Instance.ScheduleAction(() => ResumeAttack(pending));
                return null;
            }
            return ResumeAttack(pending);
        }

        private static UnitCommand ResumeAttack(PendingAttack pending)
        {
            UnitEntityData executor = pending == null ? null : pending.Executor;
            UnitEntityData target = pending == null ? null : pending.Target;
            ExactEquippedFirearmContext resolved;
            string reason;
            if (executor == null || executor.Descriptor == null || target == null ||
                target.Descriptor == null || !IsReloadAutoUse(executor) ||
                !ExactEquippedFirearmResolver.TryResolve(executor.Descriptor,
                    out resolved, out reason) ||
                !ReferenceEquals(resolved.Weapon, pending.FirearmWeapon) ||
                resolved.Firearm.Repository.State.IsEmpty ||
                resolved.EffectiveCondition == Firearms.FirearmCondition.Wrecked ||
                !TurnBasedAllowsStandardAttack())
            {
                Interlocked.Increment(ref _autoReloadCanceledAttacks);
                return null;
            }
            UnitCommand attack = UnitAttack.CreateAttackCommand(executor, target);
            if (attack == null)
            {
                Interlocked.Increment(ref _autoReloadCanceledAttacks);
                return null;
            }
            executor.Commands.AddToQueue(attack);
            Interlocked.Increment(ref _autoReloadResumedAttacks);
            return attack;
        }

        private static bool TurnBasedAllowsStandardAttack()
        {
            TurnBased.Controllers.CombatController controller = Kingmaker.Game.Instance == null
                ? null : Kingmaker.Game.Instance.TurnBasedCombatController;
            if (controller == null ||
                !TurnBased.Controllers.CombatController.IsInTurnBasedCombat()) return true;
            TurnBased.Controllers.TurnController turn = controller.CurrentTurn;
            return turn != null && turn.ActionsStates != null &&
                turn.ActionsStates.Standard != null &&
                turn.ActionsStates.Standard.CanUse;
        }

        private sealed class PendingAttack
        {
            internal PendingAttack(UnitEntityData executor, UnitEntityData target,
                Kingmaker.Items.ItemEntityWeapon firearmWeapon)
            {
                Executor = executor;
                Target = target;
                FirearmWeapon = firearmWeapon;
            }

            internal UnitEntityData Executor { get; private set; }
            internal UnitEntityData Target { get; private set; }
            internal Kingmaker.Items.ItemEntityWeapon FirearmWeapon { get; private set; }
        }
    }
}
