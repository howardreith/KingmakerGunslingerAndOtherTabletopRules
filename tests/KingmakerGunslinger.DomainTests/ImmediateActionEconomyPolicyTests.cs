using KingmakerGunslinger.BodyguardFeats;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ImmediateActionEconomyPolicyTests
    {
        internal static void TurnBasedAvailabilityIsTurnAware()
        {
            ImmediateActionEconomyInput ownTurn = Valid(true);
            Assert(ownTurn, true, "available");

            ownTurn.RawHasSwiftAction = false;
            ownTurn.RawSwiftCooldown = 6f;
            Assert(ownTurn, false, "swift-action-spent-this-turn");

            ImmediateActionEconomyInput offTurn = Valid(false);
            offTurn.RawHasSwiftAction = false;
            offTurn.RawSwiftCooldown = 1.5f;
            Assert(offTurn, true, "available");

            offTurn.RawSwiftCooldown = 6f;
            Assert(offTurn, true, "available");
        }

        internal static void OffTurnDebtFollowsTheNextActualTurn()
        {
            Assertions.Equal(ImmediateActionDebtState.PendingNextTurn,
                ImmediateActionEconomyPolicy.DebtAfterSpend(
                    ImmediateActionCombatMode.TurnBased, false),
                "Off-turn use did not charge the unit's next actual turn.");
            ImmediateActionDebtState pending = ImmediateActionDebtState
                .PendingNextTurn;
            Assertions.Equal(pending,
                ImmediateActionEconomyPolicy.OnActualTurnCompleted(pending),
                "A global/foreign turn boundary cleared pending debt.");
            Assertions.Equal(pending,
                ImmediateActionEconomyPolicy.OnTurnDelayed(
                    ImmediateActionDebtState.ChargedTurn),
                "Delay did not carry debt to the delayed actual turn.");
            ImmediateActionDebtState active = ImmediateActionEconomyPolicy
                .OnActualTurnStarted(pending);
            Assertions.Equal(ImmediateActionDebtState.ChargedTurn, active,
                "The charged turn did not consume its swift action.");
            Assertions.Equal(ImmediateActionDebtState.None,
                ImmediateActionEconomyPolicy.OnActualTurnCompleted(active),
                "Debt did not clear after the charged actual turn completed.");
        }

        internal static void DebtFlatFootedAndIncapacitationFailExactly()
        {
            ImmediateActionEconomyInput input = Valid(false);
            input.DebtState = ImmediateActionDebtState.PendingNextTurn;
            Assert(input, false, "immediate-debt-pending-next-turn");
            input.DebtState = ImmediateActionDebtState.ChargedTurn;
            Assert(input, false, "immediate-debt-charged-turn");
            input.DebtState = ImmediateActionDebtState.None;
            input.FlatFooted = true;
            Assert(input, false, "protector-flat-footed");
            input.FlatFooted = false;
            input.CanAct = false;
            Assert(input, false, "protector-incapacitated");
            input.CanAct = true;
            input.Conscious = false;
            Assert(input, false, "protector-unconscious");
            input.Conscious = true;
            input.Alive = false;
            Assert(input, false, "protector-dead");
        }

        internal static void RealTimeUsesTheNativeSixSecondBudget()
        {
            ImmediateActionEconomyInput input = Valid(false);
            input.CombatMode = ImmediateActionCombatMode.RealTime;
            input.RawHasSwiftAction = true;
            input.RawSwiftCooldown = 0f;
            Assert(input, true, "available");
            input.RawHasSwiftAction = false;
            input.RawSwiftCooldown = 3f;
            Assert(input, false, "swift-cooldown-active");
            Assertions.Equal(ImmediateActionDebtState.None,
                ImmediateActionEconomyPolicy.DebtAfterSpend(
                    ImmediateActionCombatMode.RealTime, false),
                "RTWP incorrectly created turn debt alongside its cooldown.");
        }

        internal static void CompletedTurnSwiftDoesNotBlockLaterImmediate()
        {
            ImmediateActionEconomyInput input = Valid(false);
            input.RawHasSwiftAction = false;
            input.RawSwiftCooldown = 6f;
            Assert(input, true, "available");
            Assertions.Equal(ImmediateActionDebtState.PendingNextTurn,
                ImmediateActionEconomyPolicy.DebtAfterSpend(
                    input.CombatMode, input.ProtectorIsCurrentTurn),
                "A later off-turn immediate did not charge the coming turn.");
        }

        private static ImmediateActionEconomyInput Valid(bool ownTurn)
        {
            return new ImmediateActionEconomyInput
            {
                ContractReadable = true,
                Alive = true,
                Conscious = true,
                CanAct = true,
                FlatFooted = false,
                CombatMode = ImmediateActionCombatMode.TurnBased,
                ProtectorIsCurrentTurn = ownTurn,
                RawHasSwiftAction = true,
                RawSwiftCooldown = 0f,
                DebtState = ImmediateActionDebtState.None
            };
        }

        private static void Assert(ImmediateActionEconomyInput input,
            bool available, string reason)
        {
            ImmediateActionEconomyDecision decision =
                ImmediateActionEconomyPolicy.Evaluate(input);
            Assertions.True(decision.Available == available &&
                decision.Code == reason,
                "Expected immediate decision " + available + "/" + reason +
                " but observed " + decision.Available + "/" + decision.Code +
                ".");
        }
    }
}
