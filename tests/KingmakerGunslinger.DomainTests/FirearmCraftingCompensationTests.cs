using System;
using KingmakerGunslinger.Gunsmithing;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// GS-AUD-0036. The compensation a failed firearm-crafting attempt applies.
    /// Faults are modelled at each boundary - debit, creation, insertion and
    /// rollback - by the observed state each boundary would leave behind.
    /// </summary>
    internal static class FirearmCraftingCompensationTests
    {
        private const long Cost = 22;

        private static FirearmCraftingCompensationPlan Plan(long moneyBefore,
            long moneyNow, int[] before, int[] now, int[] committed,
            bool markerAddedHere = true)
        {
            return FirearmCraftingCompensationPolicy.Plan(moneyBefore, moneyNow,
                Cost, before, now, committed, markerAddedHere);
        }

        internal static void FaultAtDebitRestoresNothingItDidNotTake()
        {
            // SpendMoney did not move money: nothing to refund, nothing added.
            FirearmCraftingCompensationPlan plan = Plan(100, 100,
                new[] { 0, 0 }, new[] { 0, 0 }, new[] { 0, 0 }, false);
            Assertions.Equal(0, (int)plan.GoldToRestore,
                "A failure before any debit must refund nothing.");
            Assertions.Equal(0, plan.ItemsToRemove[0] + plan.ItemsToRemove[1],
                "A failure before any insertion must remove nothing.");
            Assertions.False(plan.RemoveMarker,
                "A marker this transaction never added must not be removed.");
            Assertions.Equal(null, plan.Invariant,
                "An untouched state is exactly restorable.");
            Assertions.True(FirearmCraftingCompensationPolicy.RestoresPreState(
                plan, 100, 100, new[] { 0, 0 }, new[] { 0, 0 }),
                "The plan must restore the pre-transaction state.");
        }

        internal static void FaultAfterDebitRefundsExactlyTheDebit()
        {
            FirearmCraftingCompensationPlan plan = Plan(100, 100 - Cost,
                new[] { 5, 5 }, new[] { 5, 5 }, new[] { 0, 0 });
            Assertions.Equal((int)Cost, (int)plan.GoldToRestore,
                "A failure after the debit must refund exactly the debit.");
            Assertions.Equal(null, plan.Invariant, "This state is exactly restorable.");
            Assertions.True(FirearmCraftingCompensationPolicy.RestoresPreState(
                plan, 100, 100 - Cost, new[] { 5, 5 }, new[] { 5, 5 }),
                "Refunding the debit restores the pre-transaction money.");
        }

        internal static void FaultAfterPartialInsertionRemovesOnlyWhatCommitted()
        {
            // First output inserted, second not; verification then failed.
            FirearmCraftingCompensationPlan plan = Plan(100, 100 - Cost,
                new[] { 5, 5 }, new[] { 25, 5 }, new[] { 20, 0 });
            Assertions.Equal(20, plan.ItemsToRemove[0],
                "The inserted output must be removed exactly.");
            Assertions.Equal(0, plan.ItemsToRemove[1],
                "An output that was never inserted must not be removed.");
            Assertions.Equal((int)Cost, (int)plan.GoldToRestore,
                "The debit is still refunded exactly.");
            Assertions.Equal(null, plan.Invariant, "This state is exactly restorable.");
            Assertions.True(FirearmCraftingCompensationPolicy.RestoresPreState(
                plan, 100, 100 - Cost, new[] { 5, 5 }, new[] { 25, 5 }),
                "The plan restores both money and inventory exactly.");
        }

        internal static void CompensationIsRepeatSafeAfterAFullRestore()
        {
            // Applying the plan once brings the state back; planning again from
            // the restored state observes no difference and does nothing.
            FirearmCraftingCompensationPlan first = Plan(100, 100 - Cost,
                new[] { 5 }, new[] { 25 }, new[] { 20 });
            Assertions.Equal((int)Cost, (int)first.GoldToRestore, "First pass refunds the debit.");
            Assertions.Equal(20, first.ItemsToRemove[0], "First pass removes the insertion.");

            FirearmCraftingCompensationPlan second = Plan(100, 100,
                new[] { 5 }, new[] { 5 }, new[] { 20 });
            Assertions.Equal(0, (int)second.GoldToRestore,
                "A second pass must not refund again.");
            Assertions.Equal(0, second.ItemsToRemove[0],
                "A second pass must not remove items that are no longer there.");
        }

        /// <summary>
        /// The clamp alone is NOT repeat-safe. When the first compensation left
        /// an unrestorable invariant, the observed difference is still non-zero
        /// and an undeclared replan refunds again. Repeat-safety requires the
        /// caller to declare what it has already settled.
        /// </summary>
        internal static void RepeatSafetyRequiresDeclaredBookkeeping()
        {
            // External debit of 50 on top of this transaction's 22.
            FirearmCraftingCompensationPlan first = Plan(100, 100 - Cost - 50,
                new[] { 0 }, new[] { 0 }, new[] { 0 });
            Assertions.Equal((int)Cost, (int)first.GoldToRestore,
                "The first pass refunds only this transaction's own debit.");
            Assertions.True(first.Invariant != null,
                "The external debit leaves an unrestorable invariant.");

            // Money is now 50. Replanning WITHOUT bookkeeping refunds again -
            // the honest boundary of the clamp.
            FirearmCraftingCompensationPlan undeclared = Plan(100, 50,
                new[] { 0 }, new[] { 0 }, new[] { 0 });
            Assertions.Equal((int)Cost, (int)undeclared.GoldToRestore,
                "Without declared bookkeeping a replan does refund again.");

            // Declaring the prior refund makes the replan a no-op.
            FirearmCraftingCompensationPlan declared =
                FirearmCraftingCompensationPolicy.Plan(100, 50, Cost,
                    new[] { 0 }, new[] { 0 }, new[] { 0 }, false, Cost, null);
            Assertions.Equal(0, (int)declared.GoldToRestore,
                "A declared prior refund must not be repeated.");

            // The same holds for items.
            FirearmCraftingCompensationPlan itemsDeclared =
                FirearmCraftingCompensationPolicy.Plan(100, 100 - Cost, Cost,
                    new[] { 5 }, new[] { 25 }, new[] { 20 }, false, 0, new[] { 20 });
            Assertions.Equal(0, itemsDeclared.ItemsToRemove[0],
                "A declared prior removal must not be repeated.");

            FirearmCraftingCompensationPlan partial =
                FirearmCraftingCompensationPolicy.Plan(100, 100 - Cost, Cost,
                    new[] { 5 }, new[] { 25 }, new[] { 20 }, false, 5, new[] { 8 });
            Assertions.Equal((int)Cost - 5, (int)partial.GoldToRestore,
                "A partially settled refund continues from where it stopped.");
            Assertions.Equal(12, partial.ItemsToRemove[0],
                "A partially settled removal continues from where it stopped.");

            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FirearmCraftingCompensationPolicy.Plan(100, 90, Cost,
                    new[] { 0 }, new[] { 0 }, new[] { 0 }, false, -1, null),
                "A negative declared refund must fail closed.");
            Assertions.Throws<ArgumentException>(() =>
                FirearmCraftingCompensationPolicy.Plan(100, 90, Cost,
                    new[] { 0 }, new[] { 0 }, new[] { 0 }, false, 0, new[] { 0, 0 }),
                "A declared-removal array of the wrong length must fail closed.");
        }

        internal static void AnExternalGoldChangeIsReportedNotAbsorbed()
        {
            // Money fell by more than this transaction could debit: the surplus
            // was not made here, so it is reported and NOT refunded. This is the
            // over-refund the old code produced and then could not undo.
            FirearmCraftingCompensationPlan plan = Plan(100, 100 - Cost - 50,
                new[] { 0 }, new[] { 0 }, new[] { 0 });
            Assertions.Equal((int)Cost, (int)plan.GoldToRestore,
                "Only this transaction's own debit may be refunded.");
            Assertions.True(plan.Invariant != null &&
                plan.Invariant.Contains("could only debit"),
                "A larger external debit must be reported.");
            Assertions.False(FirearmCraftingCompensationPolicy.RestoresPreState(
                plan, 100, 100 - Cost - 50, new[] { 0 }, new[] { 0 }),
                "An unrestorable state must not claim to restore the pre-state.");

            // Money rose during the attempt: nothing is reclaimed.
            FirearmCraftingCompensationPlan gained = Plan(100, 130,
                new[] { 0 }, new[] { 0 }, new[] { 0 });
            Assertions.Equal(0, (int)gained.GoldToRestore,
                "A transaction must never take gold to balance its own rollback.");
            Assertions.True(gained.Invariant != null &&
                gained.Invariant.Contains("gold increased"),
                "An external gain must be reported.");
        }

        internal static void AnExternalItemChangeIsReportedNotAbsorbed()
        {
            // More of the output appeared than this transaction added.
            FirearmCraftingCompensationPlan surplus = Plan(100, 100 - Cost,
                new[] { 5 }, new[] { 30 }, new[] { 20 });
            Assertions.Equal(20, surplus.ItemsToRemove[0],
                "Only this transaction's own insertion may be removed.");
            Assertions.True(surplus.Invariant != null &&
                surplus.Invariant.Contains("surplus was not made here"),
                "An external item gain must be reported.");

            // The output was consumed by something else mid-attempt.
            FirearmCraftingCompensationPlan lost = Plan(100, 100 - Cost,
                new[] { 5 }, new[] { 3 }, new[] { 0 });
            Assertions.Equal(0, lost.ItemsToRemove[0],
                "Nothing is removed when the output already fell below the start.");
            Assertions.True(lost.Invariant != null && lost.Invariant.Contains("lost 2"),
                "An external item loss must be reported.");
        }

        internal static void APreExistingMarkerIsNeverStripped()
        {
            FirearmCraftingCompensationPlan held = Plan(100, 100 - Cost,
                new[] { 0 }, new[] { 0 }, new[] { 0 }, false);
            Assertions.False(held.RemoveMarker,
                "A marker the caster already held must survive a failed craft.");

            FirearmCraftingCompensationPlan added = Plan(100, 100 - Cost,
                new[] { 0 }, new[] { 0 }, new[] { 0 }, true);
            Assertions.True(added.RemoveMarker,
                "A marker this transaction added must be removed.");
        }

        internal static void InvalidCompensationInputsFailClosed()
        {
            Assertions.Throws<ArgumentException>(() =>
                FirearmCraftingCompensationPolicy.Plan(100, 90, Cost,
                    new[] { 0, 0 }, new[] { 0 }, new[] { 0, 0 }, false),
                "Disagreeing input lengths must fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FirearmCraftingCompensationPolicy.Plan(100, 90, -1,
                    new[] { 0 }, new[] { 0 }, new[] { 0 }, false),
                "A negative cost must fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FirearmCraftingCompensationPolicy.Plan(100, 90, Cost,
                    new[] { 0 }, new[] { 0 }, new[] { -1 }, false),
                "A negative committed count must fail closed.");
            Assertions.Throws<ArgumentNullException>(() =>
                FirearmCraftingCompensationPolicy.Plan(100, 90, Cost,
                    null, new[] { 0 }, new[] { 0 }, false),
                "Missing counts must fail closed.");
        }
    }
}
