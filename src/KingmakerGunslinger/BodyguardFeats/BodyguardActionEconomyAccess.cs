using System;
using System.Globalization;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.BodyguardFeats
{
    /// <summary>
    /// Thin adapter over Kingmaker's authoritative opportunity-attack count and
    /// shared swift-action cooldown. Bodyguard never creates an attack command,
    /// and In Harm's Way never owns a parallel per-round resource.
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
            out float swiftCooldown)
        {
            swiftCooldown = float.NaN;
            if (unit == null || unit.Descriptor == null ||
                unit.Descriptor.State == null || unit.CombatState == null ||
                unit.CombatState.Cooldown == null)
                return false;
            swiftCooldown = unit.CombatState.Cooldown.SwiftAction;
            return unit.Descriptor.State.CanAct &&
                unit.Descriptor.State.IsConscious &&
                !unit.Descriptor.State.IsDead && unit.HasSwiftAction() &&
                !float.IsNaN(swiftCooldown) &&
                !float.IsInfinity(swiftCooldown) && swiftCooldown <= 0f;
        }

        internal static bool TrySpendImmediateAction(UnitEntityData unit,
            out float before, out float after)
        {
            before = float.NaN;
            after = float.NaN;
            if (!CanSpendImmediateAction(unit, out before))
            {
                after = before;
                return false;
            }
            try
            {
                unit.CombatState.Cooldown.SwiftAction = before +
                    SwiftActionCooldownSeconds;
                after = unit.CombatState.Cooldown.SwiftAction;
                if (Math.Abs(after - (before + SwiftActionCooldownSeconds)) <
                    0.0001f) return true;
                TryRestoreImmediateAction(unit, before, after);
                after = unit.CombatState.Cooldown.SwiftAction;
                return false;
            }
            catch
            {
                TryRestoreImmediateAction(unit, before,
                    before + SwiftActionCooldownSeconds);
                try
                {
                    if (unit != null && unit.CombatState != null &&
                        unit.CombatState.Cooldown != null)
                        after = unit.CombatState.Cooldown.SwiftAction;
                }
                catch { }
                return false;
            }
        }

        internal static bool TryRestoreImmediateAction(UnitEntityData unit,
            float before, float expectedCurrent)
        {
            if (unit == null || unit.CombatState == null ||
                unit.CombatState.Cooldown == null || float.IsNaN(before) ||
                float.IsInfinity(before)) return false;
            try
            {
                float current = unit.CombatState.Cooldown.SwiftAction;
                if (Math.Abs(current - expectedCurrent) >= 0.0001f)
                    return false;
                unit.CombatState.Cooldown.SwiftAction = before;
                return Math.Abs(unit.CombatState.Cooldown.SwiftAction - before) <
                    0.0001f;
            }
            catch { return false; }
        }
    }
}
