using System;
using System.Globalization;
using Kingmaker;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using TurnBased.Controllers;

namespace KingmakerGunslinger.BodyguardFeats
{
    internal sealed class BodyguardImmediateActionSnapshot
    {
        internal bool ContractReadable { get; set; }
        internal bool Alive { get; set; }
        internal bool Conscious { get; set; }
        internal bool CanAct { get; set; }
        internal bool HasSwiftAction { get; set; }
        internal float SwiftCooldown { get; set; }
        internal float StandardCooldown { get; set; }
        internal float MoveCooldown { get; set; }
        internal bool IsInCombat { get; set; }
        internal bool IsWaitingInitiative { get; set; }
        internal bool TurnBased { get; set; }
        internal bool ProtectorIsCurrentTurn { get; set; }
        internal string CurrentTurnId { get; set; }
        internal bool FlatFooted { get; set; }
        internal bool FlatFootedReadable { get; set; }
        internal ImmediateActionDebtState DebtState { get; set; }
        internal ImmediateActionEconomyDecision Decision { get; set; }
        internal bool Available
        {
            get { return Decision != null && Decision.Available; }
        }
        internal string Reason
        { get { return Decision == null ? "action-contract-unreadable" :
            Decision.Code; } }
    }

    internal sealed class BodyguardImmediateActionSpendToken
    {
        internal UnitEntityData Unit { get; set; }
        internal ImmediateActionCombatMode CombatMode { get; set; }
        internal bool ProtectorWasCurrentTurn { get; set; }
        internal float SwiftBefore { get; set; }
        internal float SwiftAfter { get; set; }
        internal Fact AddedPendingFact { get; set; }
        internal ImmediateActionDebtState DebtAfter { get; set; }
        internal bool Committed { get; set; }
    }

    /// <summary>
    /// Thin adapter over Kingmaker's authoritative opportunity-attack count and
    /// shared swift-action cooldown. Kingmaker has no complete immediate-action
    /// resource: turn-based off-turn spends use save-stable next-actual-turn
    /// debt, while native SwiftAction remains the command gate and RTWP timer.
    /// </summary>
    internal static class BodyguardActionEconomyAccess
    {
        internal const float SwiftActionCooldownSeconds = 6f;

        internal static bool CanSpendAttackOfOpportunity(UnitEntityData unit,
            UnitEntityData attacker, out int remaining)
        {
            remaining = 0;
            if (unit == null || attacker == null || unit.Descriptor == null ||
                unit.Descriptor.State == null || unit.CombatState == null)
                return false;
            remaining = Math.Max(0,
                unit.CombatState.AttackOfOpportunityCount);
            if (!unit.Descriptor.State.CanAct ||
                !unit.Descriptor.State.IsConscious ||
                unit.Descriptor.State.IsDead || remaining <= 0 ||
                !unit.CombatState.CanAttackOfOpportunity)
                return false;
            try
            {
                // simulate=true executes Kingmaker's complete native eligibility
                // path, including CanActInCombat/Combat Reflexes and transient
                // combat gates, but branches before UnitAttackOfOpportunity,
                // event publication, cooldown mutation, and count expenditure.
                return unit.CombatState.AttackOfOpportunity(attacker, true) &&
                    unit.Descriptor.State.CanAct &&
                    unit.Descriptor.State.IsConscious &&
                    !unit.Descriptor.State.IsDead &&
                    unit.CombatState.CanAttackOfOpportunity &&
                    unit.CombatState.AttackOfOpportunityCount == remaining;
            }
            catch { return false; }
        }

        internal static string DescribeAttackOfOpportunityState(
            UnitEntityData unit, UnitEntityData attacker)
        {
            if (unit == null || attacker == null || unit.Descriptor == null ||
                unit.Descriptor.State == null || unit.CombatState == null)
                return "nativeAooState=unavailable";
            try
            {
                UnitState state = unit.Descriptor.State;
                return "nativeAooState={count=" +
                    unit.CombatState.AttackOfOpportunityCount.ToString(
                        CultureInfo.InvariantCulture) + ",canAoo=" +
                    unit.CombatState.CanAttackOfOpportunity +
                    ",canActInCombat=" + unit.CombatState.CanActInCombat +
                    ",canAct=" + state.CanAct + ",conscious=" +
                    state.IsConscious + ",dead=" + state.IsDead +
                    ",disableAoo=" + state.HasCondition(
                        UnitCondition.DisableAttacksOfOpportunity) +
                    ",cannotAttack=" + state.HasCondition(
                        UnitCondition.CanNotAttack) + ",confusion=" +
                    state.HasCondition(UnitCondition.Confusion) +
                    ",beforeInitiative=" + state.HasCondition(
                        UnitCondition.AttackOfOpportunityBeforeInitiative) +
                    ",protectorPrevent=" + unit.CombatState
                        .PreventAttacksOfOpporunityNextFrame +
                    ",attackerPrevent=" + attacker.CombatState
                        .PreventAttacksOfOpporunityNextFrame +
                    ",motion=" + unit.HasMotionThisTick + ",memory=" +
                    (attacker.Memory != null && attacker.Memory.Contains(unit)) +
                    ",threatHand=" +
                    (UnitEngagementExtension.GetThreatHand(unit) != null) + "}";
            }
            catch (Exception exception)
            {
                return "nativeAooState=fault:" +
                    exception.GetType().FullName;
            }
        }

        internal static bool TrySpendAttackOfOpportunity(UnitEntityData unit,
            UnitEntityData attacker, out int before, out int after)
        {
            before = 0;
            after = 0;
            if (!CanSpendAttackOfOpportunity(unit, attacker, out before))
            {
                after = before;
                return false;
            }

            try
            {
                unit.CombatState.AttackOfOpportunityCount = before - 1;
                after = unit.CombatState.AttackOfOpportunityCount;
                if (after == before - 1) return true;
                if (after != before)
                    unit.CombatState.AttackOfOpportunityCount = before;
                after = unit.CombatState.AttackOfOpportunityCount;
                return false;
            }
            catch
            {
                try
                {
                    if (unit != null && unit.CombatState != null &&
                        unit.CombatState.AttackOfOpportunityCount == before - 1)
                        unit.CombatState.AttackOfOpportunityCount = before;
                    if (unit != null && unit.CombatState != null)
                        after = unit.CombatState.AttackOfOpportunityCount;
                }
                catch { }
                return false;
            }
        }

        internal static bool CanSpendImmediateAction(UnitEntityData unit,
            UnitEntityData attacker, out float swiftCooldown,
            out string reason)
        {
            BodyguardImmediateActionSnapshot snapshot =
                ObserveImmediateAction(unit, attacker);
            swiftCooldown = snapshot.SwiftCooldown;
            reason = snapshot.Reason;
            return snapshot.Available;
        }

        internal static BodyguardImmediateActionSnapshot ObserveImmediateAction(
            UnitEntityData unit, UnitEntityData attacker = null)
        {
            var result = new BodyguardImmediateActionSnapshot
            {
                SwiftCooldown = float.NaN,
                StandardCooldown = float.NaN,
                MoveCooldown = float.NaN
            };
            if (unit == null || unit.Descriptor == null ||
                unit.Descriptor.State == null || unit.CombatState == null ||
                unit.CombatState.Cooldown == null)
                return result;
            try
            {
                result.ContractReadable = true;
                result.Alive = !unit.Descriptor.State.IsDead;
                result.Conscious = unit.Descriptor.State.IsConscious;
                result.CanAct = unit.Descriptor.State.CanAct;
                result.SwiftCooldown = unit.CombatState.Cooldown.SwiftAction;
                result.StandardCooldown = unit.CombatState.Cooldown
                    .StandardAction;
                result.MoveCooldown = unit.CombatState.Cooldown.MoveAction;
                result.IsInCombat = unit.CombatState.IsInCombat;
                result.IsWaitingInitiative = unit.CombatState
                    .IsWaitingInitiative;
                result.HasSwiftAction = unit.HasSwiftAction();
                result.TurnBased = CombatController.IsInTurnBasedCombat();
                if (result.TurnBased)
                {
                    var controller = Game.Instance == null ? null :
                        Game.Instance.TurnBasedCombatController;
                    var turn = controller == null ? null :
                        controller.CurrentTurn;
                    UnitEntityData current = turn == null ? null : turn.Unit;
                    if (controller == null || turn == null || current == null)
                        result.ContractReadable = false;
                    result.ProtectorIsCurrentTurn = ReferenceEquals(current,
                        unit);
                    result.CurrentTurnId = current == null ? null :
                        current.UniqueId;
                }
                else
                {
                    result.ProtectorIsCurrentTurn = false;
                    result.CurrentTurnId = null;
                }
                result.FlatFootedReadable = TryObserveFlatFooted(attacker,
                    unit, out bool flatFooted);
                result.FlatFooted = flatFooted;
                if (!result.FlatFootedReadable)
                    result.ContractReadable = false;
                result.DebtState = ImmediateActionEconomyRuntime.ObserveDebt(
                    unit);
            }
            catch
            { result.ContractReadable = false; }
            result.Decision = ImmediateActionEconomyPolicy.Evaluate(
                new ImmediateActionEconomyInput
                {
                    ContractReadable = result.ContractReadable,
                    Alive = result.Alive,
                    Conscious = result.Conscious,
                    CanAct = result.CanAct,
                    FlatFooted = result.FlatFooted,
                    CombatMode = result.TurnBased ?
                        ImmediateActionCombatMode.TurnBased :
                        ImmediateActionCombatMode.RealTime,
                    ProtectorIsCurrentTurn = result.ProtectorIsCurrentTurn,
                    RawHasSwiftAction = result.HasSwiftAction,
                    RawSwiftCooldown = result.SwiftCooldown,
                    DebtState = result.DebtState
                });
            return result;
        }

        internal static bool TrySpendImmediateAction(UnitEntityData unit,
            UnitEntityData attacker,
            out BodyguardImmediateActionSpendToken token)
        {
            BodyguardImmediateActionSnapshot snapshot =
                ObserveImmediateAction(unit, attacker);
            token = new BodyguardImmediateActionSpendToken
            {
                Unit = unit,
                CombatMode = snapshot.TurnBased ?
                    ImmediateActionCombatMode.TurnBased :
                    ImmediateActionCombatMode.RealTime,
                ProtectorWasCurrentTurn = snapshot.ProtectorIsCurrentTurn,
                SwiftBefore = snapshot.SwiftCooldown,
                SwiftAfter = snapshot.SwiftCooldown,
                DebtAfter = snapshot.DebtState
            };
            if (!snapshot.Available) return false;
            try
            {
                ImmediateActionDebtState debt = ImmediateActionEconomyPolicy
                    .DebtAfterSpend(token.CombatMode,
                        token.ProtectorWasCurrentTurn);
                if (debt == ImmediateActionDebtState.PendingNextTurn)
                {
                    Fact added;
                    if (!ImmediateActionEconomyRuntime.TryAddPending(unit,
                            out added)) return false;
                    token.AddedPendingFact = added;
                    token.DebtAfter = debt;
                    token.SwiftAfter = unit.CombatState.Cooldown.SwiftAction;
                    token.Committed = true;
                    return true;
                }

                unit.CombatState.Cooldown.SwiftAction = token.SwiftBefore +
                    SwiftActionCooldownSeconds;
                token.SwiftAfter = unit.CombatState.Cooldown.SwiftAction;
                token.DebtAfter = ImmediateActionDebtState.None;
                token.Committed = Math.Abs(token.SwiftAfter -
                    (token.SwiftBefore + SwiftActionCooldownSeconds)) <
                    0.0001f;
                if (token.Committed) return true;
                TryRollbackImmediateAction(token);
                return false;
            }
            catch
            {
                TryRollbackImmediateAction(token);
                return false;
            }
        }

        internal static bool TryRollbackImmediateAction(
            BodyguardImmediateActionSpendToken token)
        {
            if (token == null || token.Unit == null ||
                token.Unit.CombatState == null ||
                token.Unit.CombatState.Cooldown == null) return false;
            if (token.AddedPendingFact != null)
                return ImmediateActionEconomyRuntime.TryRemoveAddedPending(
                    token.Unit, token.AddedPendingFact);
            if (float.IsNaN(token.SwiftBefore) ||
                float.IsInfinity(token.SwiftBefore)) return false;
            try
            {
                float current = token.Unit.CombatState.Cooldown.SwiftAction;
                if (Math.Abs(current - token.SwiftAfter) >= 0.0001f)
                    return false;
                token.Unit.CombatState.Cooldown.SwiftAction =
                    token.SwiftBefore;
                return Math.Abs(token.Unit.CombatState.Cooldown.SwiftAction -
                    token.SwiftBefore) < 0.0001f;
            }
            catch { return false; }
        }

        private static bool TryObserveFlatFooted(UnitEntityData attacker,
            UnitEntityData protector, out bool flatFooted)
        {
            flatFooted = false;
            if (protector == null) return false;
            if (attacker == null) return true;
            try
            {
                var rule = new RuleCheckTargetFlatFooted(attacker, protector);
                Rulebook.Trigger(rule);
                flatFooted = rule.IsFlatFooted;
                return true;
            }
            catch { return false; }
        }
    }
}
