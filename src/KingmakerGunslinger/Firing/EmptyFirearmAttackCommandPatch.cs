using System;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Abilities;
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
        private static long _sequenceInterruptionRejections;
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
        internal static long SequenceInterruptionRejections
        { get { return Interlocked.Read(ref _sequenceInterruptionRejections); } }
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
            MethodInfo clickPrefix = typeof(EmptyFirearmAttackCommandPatch).GetMethod(
                "PlayerClickPrefix", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo clickPostfix = typeof(EmptyFirearmAttackCommandPatch).GetMethod(
                "PlayerClickPostfix", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo onClick = typeof(Kingmaker.Controllers.Clicks.Handlers.ClickUnitHandler)
                .GetMethod("OnClick", BindingFlags.Public | BindingFlags.Instance, null,
                new[]
                {
                    typeof(UnityEngine.GameObject),
                    typeof(UnityEngine.Vector3),
                    typeof(int),
                    typeof(bool),
                    typeof(bool)
                }, null);
            if (create == null || prefix == null || ended == null || endedPostfix == null)
                throw new MissingMethodException(
                    "Exact attack construction or reload completion contract was unavailable.");
            if (onClick == null || clickPrefix == null ||
                clickPostfix == null)
                throw new MissingMethodException(
                    "Exact player attack click contract was unavailable.");
            harmony.Patch(create, new HarmonyMethod(prefix), null, null);
            harmony.Patch(ended, null, new HarmonyMethod(endedPostfix), null);
            // The native player unit click opens a narrowly scoped
            // authorization for its own selected executors against the
            // clicked target; the postfix discards it as soon as the click
            // returns, so later automatic work in the same frame cannot
            // inherit the order.
            harmony.Patch(onClick, new HarmonyMethod(clickPrefix),
                new HarmonyMethod(clickPostfix), null);
        }

        private static void PlayerClickPrefix(UnityEngine.GameObject __0)
        {
            try
            {
                Kingmaker.View.UnitEntityView view = __0 == null
                    ? null
                    : __0.GetComponent<Kingmaker.View.UnitEntityView>();
                BrokenSequenceSuppressionRuntime.BeginPlayerAttackClick(
                    view == null ? null : view.EntityData);
            }
            catch
            {
                BrokenSequenceSuppressionRuntime.EndPlayerAttackClick();
            }
        }

        private static void PlayerClickPostfix()
        {
            BrokenSequenceSuppressionRuntime.EndPlayerAttackClick();
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
            BrokenSequenceConstructionDecision sequence =
                BrokenSequenceInterruptionPolicy.EvaluateConstruction(
                    BrokenSequenceSuppressionRuntime.IsSuppressed(
                        executor, firearm.Weapon),
                    BrokenSequenceSuppressionRuntime
                        .TryConsumePlayerAttackAuthorization(executor, __1));
            if (sequence == BrokenSequenceConstructionDecision.RejectInterrupted)
                return Reject(executor, __1,
                    EmptyFirearmCommandDisposition.RejectInterrupted,
                    firearm.Firearm.ItemDisplayName +
                    " broke during its attack sequence; issue a new attack order to fire it again.",
                    ref __result);
            if (sequence == BrokenSequenceConstructionDecision.AllowAndConsume)
                BrokenSequenceSuppressionRuntime.ConsumeSuppression(
                    executor, firearm.Weapon);
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
            if (disposition == EmptyFirearmCommandDisposition.RejectInterrupted)
                Interlocked.Increment(ref _sequenceInterruptionRejections);
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
                    Kingmaker.Items.ItemEntityWeapon capturedWeapon =
                        ResolveExactWeapon(executor);
                    lock (PendingGate)
                        Pending[command] = new PendingAttack(executor, target,
                            firearmWeapon: capturedWeapon,
                            paperMode: PaperCartridgeModeRuntime.IsActive(
                                executor.Descriptor,
                                BlueprintBootstrap.PaperCartridgeMode.Marker),
                            degradationEpoch: BrokenSequenceSuppressionRuntime
                                .GetDegradationEpoch(executor, capturedWeapon));
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
                PaperCartridgeModeRuntime.IsActive(executor.Descriptor,
                    BlueprintBootstrap.PaperCartridgeMode.Marker) != pending.PaperMode ||
                resolved.Firearm.Repository.State.IsEmpty ||
                resolved.EffectiveCondition == Firearms.FirearmCondition.Wrecked ||
                !BrokenSequenceInterruptionPolicy.MayResumeCapturedAttack(
                    pending.DegradationEpoch,
                    BrokenSequenceSuppressionRuntime.GetDegradationEpoch(
                        executor, resolved.Weapon)) ||
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
            return TurnBasedAllowsStandardAttack(true, turn != null,
                turn != null && turn.ActionsStates != null &&
                turn.ActionsStates.Standard != null &&
                turn.ActionsStates.Standard.CanUse);
        }

        internal static bool TurnBasedAllowsStandardAttack(bool isTurnBased,
            bool hasCurrentTurn, bool standardActionAvailable)
        {
            return !isTurnBased || (hasCurrentTurn && standardActionAvailable);
        }

        private sealed class PendingAttack
        {
            internal PendingAttack(UnitEntityData executor, UnitEntityData target,
                Kingmaker.Items.ItemEntityWeapon firearmWeapon, bool paperMode,
                int degradationEpoch)
            {
                Executor = executor;
                Target = target;
                FirearmWeapon = firearmWeapon;
                PaperMode = paperMode;
                DegradationEpoch = degradationEpoch;
            }

            internal UnitEntityData Executor { get; private set; }
            internal UnitEntityData Target { get; private set; }
            internal Kingmaker.Items.ItemEntityWeapon FirearmWeapon { get; private set; }
            internal bool PaperMode { get; private set; }
            internal int DegradationEpoch { get; private set; }
        }
    }
}
