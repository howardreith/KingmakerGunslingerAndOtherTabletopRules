using System;

namespace KingmakerGunslinger.BodyguardFeats
{
    internal enum ImmediateActionCombatMode
    {
        RealTime = 0,
        TurnBased = 1
    }

    internal enum ImmediateActionDebtState
    {
        None = 0,
        PendingNextTurn = 1,
        ChargedTurn = 2
    }

    internal enum ImmediateActionAvailabilityReason
    {
        Available = 0,
        ContractUnreadable,
        Dead,
        Unconscious,
        Incapacitated,
        FlatFooted,
        PendingNextTurnDebt,
        ChargedTurnDebt,
        SwiftSpentThisTurn,
        SwiftCooldownActive
    }

    internal sealed class ImmediateActionEconomyInput
    {
        internal bool ContractReadable { get; set; }
        internal bool Alive { get; set; }
        internal bool Conscious { get; set; }
        internal bool CanAct { get; set; }
        internal bool FlatFooted { get; set; }
        internal ImmediateActionCombatMode CombatMode { get; set; }
        internal bool ProtectorIsCurrentTurn { get; set; }
        internal bool RawHasSwiftAction { get; set; }
        internal float RawSwiftCooldown { get; set; }
        internal ImmediateActionDebtState DebtState { get; set; }
    }

    internal sealed class ImmediateActionEconomyDecision
    {
        internal ImmediateActionEconomyDecision(
            ImmediateActionAvailabilityReason reason)
        { Reason = reason; }

        internal ImmediateActionAvailabilityReason Reason { get; private set; }
        internal bool Available
        { get { return Reason == ImmediateActionAvailabilityReason.Available; } }
        internal string Code
        { get { return ImmediateActionEconomyPolicy.Code(Reason); } }
    }

    /// <summary>
    /// Rules-level immediate/swift policy. In turn-based mode a positive raw
    /// SwiftAction cooldown on somebody else's turn is bookkeeping from the
    /// completed turn, not proof that the coming turn's swift action was spent.
    /// Off-turn use therefore creates debt tied to that unit's next actual turn.
    /// </summary>
    internal static class ImmediateActionEconomyPolicy
    {
        internal static ImmediateActionEconomyDecision Evaluate(
            ImmediateActionEconomyInput input)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (!input.ContractReadable)
                return Decide(ImmediateActionAvailabilityReason
                    .ContractUnreadable);
            if (!input.Alive)
                return Decide(ImmediateActionAvailabilityReason.Dead);
            if (!input.Conscious)
                return Decide(ImmediateActionAvailabilityReason.Unconscious);
            if (!input.CanAct)
                return Decide(ImmediateActionAvailabilityReason.Incapacitated);
            if (input.FlatFooted)
                return Decide(ImmediateActionAvailabilityReason.FlatFooted);
            if (input.DebtState == ImmediateActionDebtState.PendingNextTurn)
                return Decide(ImmediateActionAvailabilityReason
                    .PendingNextTurnDebt);
            if (input.DebtState == ImmediateActionDebtState.ChargedTurn)
                return Decide(ImmediateActionAvailabilityReason
                    .ChargedTurnDebt);
            if (float.IsNaN(input.RawSwiftCooldown) ||
                float.IsInfinity(input.RawSwiftCooldown))
                return Decide(ImmediateActionAvailabilityReason
                    .ContractUnreadable);

            if (input.CombatMode == ImmediateActionCombatMode.TurnBased)
            {
                if (!input.ProtectorIsCurrentTurn)
                    return Decide(ImmediateActionAvailabilityReason.Available);
                if (input.RawSwiftCooldown > 0f ||
                    !input.RawHasSwiftAction)
                    return Decide(ImmediateActionAvailabilityReason
                        .SwiftSpentThisTurn);
                return Decide(ImmediateActionAvailabilityReason.Available);
            }

            if (input.RawSwiftCooldown > 0f || !input.RawHasSwiftAction)
                return Decide(ImmediateActionAvailabilityReason
                    .SwiftCooldownActive);
            return Decide(ImmediateActionAvailabilityReason.Available);
        }

        internal static ImmediateActionDebtState DebtAfterSpend(
            ImmediateActionCombatMode mode, bool protectorIsCurrentTurn)
        {
            return mode == ImmediateActionCombatMode.TurnBased &&
                !protectorIsCurrentTurn ?
                    ImmediateActionDebtState.PendingNextTurn :
                    ImmediateActionDebtState.None;
        }

        internal static ImmediateActionDebtState OnActualTurnStarted(
            ImmediateActionDebtState state)
        {
            return state == ImmediateActionDebtState.PendingNextTurn ?
                ImmediateActionDebtState.ChargedTurn : state;
        }

        internal static ImmediateActionDebtState OnActualTurnCompleted(
            ImmediateActionDebtState state)
        {
            return state == ImmediateActionDebtState.ChargedTurn ?
                ImmediateActionDebtState.None : state;
        }

        internal static ImmediateActionDebtState OnTurnDelayed(
            ImmediateActionDebtState state)
        {
            return state == ImmediateActionDebtState.ChargedTurn ?
                ImmediateActionDebtState.PendingNextTurn : state;
        }

        internal static string Code(ImmediateActionAvailabilityReason reason)
        {
            switch (reason)
            {
                case ImmediateActionAvailabilityReason.Available:
                    return "available";
                case ImmediateActionAvailabilityReason.ContractUnreadable:
                    return "action-contract-unreadable";
                case ImmediateActionAvailabilityReason.Dead:
                    return "protector-dead";
                case ImmediateActionAvailabilityReason.Unconscious:
                    return "protector-unconscious";
                case ImmediateActionAvailabilityReason.Incapacitated:
                    return "protector-incapacitated";
                case ImmediateActionAvailabilityReason.FlatFooted:
                    return "protector-flat-footed";
                case ImmediateActionAvailabilityReason.PendingNextTurnDebt:
                    return "immediate-debt-pending-next-turn";
                case ImmediateActionAvailabilityReason.ChargedTurnDebt:
                    return "immediate-debt-charged-turn";
                case ImmediateActionAvailabilityReason.SwiftSpentThisTurn:
                    return "swift-action-spent-this-turn";
                case ImmediateActionAvailabilityReason.SwiftCooldownActive:
                    return "swift-cooldown-active";
                default:
                    return "unknown";
            }
        }

        private static ImmediateActionEconomyDecision Decide(
            ImmediateActionAvailabilityReason reason)
        { return new ImmediateActionEconomyDecision(reason); }
    }
}
