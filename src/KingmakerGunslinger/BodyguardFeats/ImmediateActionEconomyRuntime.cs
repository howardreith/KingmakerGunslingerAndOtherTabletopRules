using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using TurnBased.Controllers;

namespace KingmakerGunslinger.BodyguardFeats
{
    /// <summary>
    /// Save-stable turn debt for the gap in Kingmaker's native action model.
    /// Native SwiftAction remains the RTWP budget and the charged-turn command
    /// gate; two hidden facts correlate an off-turn spend with the unit's next
    /// actual turn rather than a global round number.
    /// </summary>
    internal static class ImmediateActionEconomyRuntime
    {
        [ThreadStatic]
        private static UnitEntityData _preparingTurnUnit;

        internal static void BeginTurnPrepare(TurnController turn)
        {
            _preparingTurnUnit = turn == null ? null : turn.Unit;
        }

        internal static void EndTurnPrepare(TurnController turn)
        {
            if (turn == null || ReferenceEquals(_preparingTurnUnit, turn.Unit))
                _preparingTurnUnit = null;
        }

        internal static ImmediateActionDebtState ObserveDebt(
            UnitEntityData unit)
        {
            BodyguardModeBlueprintSet modes = Modes;
            if (unit == null || unit.Descriptor == null || modes == null)
                return ImmediateActionDebtState.None;
            bool pending = unit.Descriptor.HasFact(modes.ImmediatePending);
            bool active = unit.Descriptor.HasFact(modes.ImmediateChargedTurn);
            if (pending && active)
                throw new InvalidOperationException(
                    "A unit owns both In Harm's Way immediate-action debt states.");
            return pending ? ImmediateActionDebtState.PendingNextTurn :
                active ? ImmediateActionDebtState.ChargedTurn :
                ImmediateActionDebtState.None;
        }

        internal static bool TryAddPending(UnitEntityData unit,
            out Fact added)
        {
            added = null;
            BodyguardModeBlueprintSet modes = Modes;
            if (unit == null || unit.Descriptor == null || modes == null ||
                ObserveDebt(unit) != ImmediateActionDebtState.None) return false;
            added = unit.Descriptor.AddFact(modes.ImmediatePending);
            bool committed = added != null && ReferenceEquals(added.Blueprint,
                    modes.ImmediatePending) &&
                ObserveDebt(unit) == ImmediateActionDebtState.PendingNextTurn;
            if (!committed && added != null)
                unit.Descriptor.RemoveFact(added);
            if (committed) Log("debt.pending", unit,
                "nextActualTurn=charged");
            return committed;
        }

        internal static bool TryRemoveAddedPending(UnitEntityData unit,
            Fact added)
        {
            BodyguardModeBlueprintSet modes = Modes;
            if (unit == null || unit.Descriptor == null || modes == null ||
                added == null || !ReferenceEquals(added.Blueprint,
                    modes.ImmediatePending)) return false;
            try
            {
                Fact current = unit.Descriptor.GetFact(modes.ImmediatePending);
                if (!ReferenceEquals(current, added)) return false;
                unit.Descriptor.RemoveFact(added);
                bool restored = ObserveDebt(unit) ==
                    ImmediateActionDebtState.None;
                if (restored) Log("debt.rollback", unit,
                    "state=none");
                return restored;
            }
            catch { return false; }
        }

        internal static void OnCooldownCleared(
            UnitCombatState.Cooldowns cooldown)
        {
            if (cooldown == null || !CombatController.IsInTurnBasedCombat())
                return;
            UnitEntityData unit = _preparingTurnUnit;
            if (unit == null || unit.CombatState == null ||
                !ReferenceEquals(unit.CombatState.Cooldown, cooldown)) return;
            _preparingTurnUnit = null;
            ImmediateActionDebtState state;
            try { state = ObserveDebt(unit); }
            catch (Exception exception)
            {
                Fault("turn-start.observe", unit, exception);
                return;
            }
            if (state == ImmediateActionDebtState.PendingNextTurn)
            {
                if (!TryTransition(unit, Modes.ImmediatePending,
                        Modes.ImmediateChargedTurn))
                {
                    Fault("turn-start.transition", unit, null);
                    return;
                }
                state = ImmediateActionDebtState.ChargedTurn;
            }
            if (state != ImmediateActionDebtState.ChargedTurn) return;
            cooldown.SwiftAction = BodyguardActionEconomyAccess
                .SwiftActionCooldownSeconds;
            Log("debt.turn-start", unit, "state=charged;swiftCooldown=" +
                cooldown.SwiftAction.ToString("R",
                    CultureInfo.InvariantCulture));
        }

        internal static void OnTurnDisposed(TurnController turn)
        {
            UnitEntityData unit = turn == null ? null : turn.Unit;
            if (unit == null || unit.Descriptor == null || Modes == null) return;
            ImmediateActionDebtState state;
            try { state = ObserveDebt(unit); }
            catch (Exception exception)
            {
                Fault("turn-end.observe", unit, exception);
                return;
            }
            if (state != ImmediateActionDebtState.ChargedTurn) return;
            bool delayed = turn.Status == TurnController.TurnStatus.Delayed;
            bool transitioned = delayed ? TryTransition(unit,
                Modes.ImmediateChargedTurn, Modes.ImmediatePending) :
                TryRemove(unit, Modes.ImmediateChargedTurn);
            if (!transitioned)
            {
                Fault(delayed ? "turn-delay.transition" :
                    "turn-end.clear", unit, null);
                return;
            }
            Log(delayed ? "debt.delayed" : "debt.cleared", unit,
                delayed ? "state=pending-next-actual-turn" : "state=none");
        }

        internal static bool HasChargedTurnDebt(UnitEntityData unit)
        {
            try
            { return ObserveDebt(unit) == ImmediateActionDebtState.ChargedTurn; }
            catch { return true; }
        }

        internal static void RestoreAfterLoad(UnitEntityData unit)
        {
            if (unit == null || unit.CombatState == null ||
                unit.CombatState.Cooldown == null) return;
            if (!HasChargedTurnDebt(unit)) return;
            unit.CombatState.Cooldown.SwiftAction = Math.Max(
                BodyguardActionEconomyAccess.SwiftActionCooldownSeconds,
                unit.CombatState.Cooldown.SwiftAction);
            Log("debt.post-load", unit, "state=charged;swiftCooldown=" +
                unit.CombatState.Cooldown.SwiftAction.ToString("R",
                    CultureInfo.InvariantCulture));
        }

        internal static void ClearAll(string reason)
        {
            _preparingTurnUnit = null;
            BodyguardModeBlueprintSet modes = Modes;
            if (modes == null || Game.Instance == null ||
                Game.Instance.State == null || Game.Instance.State.Units == null)
                return;
            int cleared = 0;
            foreach (UnitEntityData unit in Game.Instance.State.Units.All
                .Where(value => value != null && value.Descriptor != null))
            {
                if (TryRemove(unit, modes.ImmediatePending)) cleared++;
                if (TryRemove(unit, modes.ImmediateChargedTurn)) cleared++;
            }
            Log("debt.cleanup", null, "reason=" + reason + ";cleared=" +
                cleared.ToString(CultureInfo.InvariantCulture));
        }

        private static bool TryTransition(UnitEntityData unit,
            BlueprintFeature from, BlueprintFeature to)
        {
            if (unit == null || unit.Descriptor == null || from == null ||
                to == null) return false;
            Fact original = unit.Descriptor.GetFact(from);
            if (original == null || unit.Descriptor.HasFact(to)) return false;
            Fact added = unit.Descriptor.AddFact(to);
            if (added == null || !ReferenceEquals(added.Blueprint, to))
                return false;
            try
            {
                unit.Descriptor.RemoveFact(original);
            }
            catch { }
            bool ownsFrom = unit.Descriptor.HasFact(from);
            bool ownsTo = unit.Descriptor.HasFact(to);
            if (!ownsFrom && ownsTo) return true;
            if (!ownsFrom && !ownsTo)
            {
                Fact failClosed = unit.Descriptor.AddFact(to);
                return failClosed != null && unit.Descriptor.HasFact(to);
            }
            try { unit.Descriptor.RemoveFact(added); }
            catch { }
            return false;
        }

        private static bool TryRemove(UnitEntityData unit,
            BlueprintFeature feature)
        {
            if (unit == null || unit.Descriptor == null || feature == null)
                return false;
            Fact fact = unit.Descriptor.GetFact(feature);
            if (fact == null) return false;
            try
            {
                unit.Descriptor.RemoveFact(fact);
                return !unit.Descriptor.HasFact(feature);
            }
            catch { return false; }
        }

        private static BodyguardModeBlueprintSet Modes
        {
            get
            {
                BodyguardFeatBlueprintSet set = BlueprintBootstrap.BodyguardFeats;
                return set == null ? null : set.Modes;
            }
        }

        private static void Log(string eventName, UnitEntityData unit,
            string detail)
        {
            ModContext context;
            if (!ModContext.TryGet(out context)) return;
            context.Logger.Info("bodyguard", "immediate-action." + eventName,
                "unit=" + Identity(unit) + ";" + detail);
        }

        private static void Fault(string eventName, UnitEntityData unit,
            Exception exception)
        {
            ModContext context;
            if (!ModContext.TryGet(out context)) return;
            string detail = "unit=" + Identity(unit) + ";exception=" +
                (exception == null ? "<none>" : exception.GetType().FullName +
                    ":" + exception.Message);
            if (exception == null)
                context.Logger.Warning("bodyguard", "immediate-action." +
                    eventName + ".failed", detail);
            else
                context.Logger.Failure("bodyguard", "immediate-action." +
                    eventName + ".failed", detail, exception);
        }

        private static string Identity(UnitEntityData unit)
        {
            return unit == null ? "<null>" :
                (unit.CharacterName ?? "<unnamed>") + "/" +
                (unit.UniqueId ?? "<no-id>");
        }
    }
}
