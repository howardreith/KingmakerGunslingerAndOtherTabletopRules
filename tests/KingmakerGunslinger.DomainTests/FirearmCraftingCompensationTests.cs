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

            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FirearmCraftingCompensationPolicy.Plan(100, 90, Cost,
                    new[] { 0 }, new[] { 0 }, new[] { 0 }, false, -1, null),
                "A negative declared refund must fail closed.");
            Assertions.Throws<ArgumentException>(() =>
                FirearmCraftingCompensationPolicy.Plan(100, 90, Cost,
                    new[] { 0 }, new[] { 0 }, new[] { 0 }, false, 0, new[] { 0, 0 }),
                "A declared-removal array of the wrong length must fail closed.");
        }

        /// <summary>
        /// An ordinary interrupted compensation, resumed. No external
        /// interference. The balances and counts are the CURRENT ones, so they
        /// already include the first pass - which is exactly what the earlier
        /// version of this fixture got wrong by declaring a partial refund
        /// while still supplying the pre-refund balance.
        ///
        ///     100 -> debit 22 -> 78 -> refund 5, interrupted -> 83
        ///     resume: remaining budget 17, observed deficit 17 -> restore 17
        ///
        /// Subtracting the declared refund from the deficit as well would
        /// return 12 here and strand 5 gold permanently.
        /// </summary>
        internal static void AnInterruptedCompensationResumesExactly()
        {
            const int outputBefore = 5;
            const int inserted = 20;

            // First pass on the real failure: refund 22, remove 20.
            FirearmCraftingCompensationPlan first = Plan(100, 100 - Cost,
                new[] { outputBefore }, new[] { outputBefore + inserted },
                new[] { inserted });
            Assertions.Equal((int)Cost, (int)first.GoldToRestore,
                "The first pass plans the whole debit.");
            Assertions.Equal(inserted, first.ItemsToRemove[0],
                "The first pass plans the whole insertion.");

            // It is interrupted after returning 5 gold and removing 8 items.
            const int refunded = 5;
            const int removed = 8;
            long moneyNow = 100 - Cost + refunded;                 // 83
            int countNow = outputBefore + inserted - removed;      // 17

            FirearmCraftingCompensationPlan resumed =
                FirearmCraftingCompensationPolicy.Plan(100, moneyNow, Cost,
                    new[] { outputBefore }, new[] { countNow },
                    new[] { inserted }, false, refunded, new[] { removed });

            Assertions.Equal((int)Cost - refunded, (int)resumed.GoldToRestore,
                "The resumed pass must return the remaining 17 gold, not 12.");
            Assertions.Equal(inserted - removed, resumed.ItemsToRemove[0],
                "The resumed pass must remove the remaining 12 items.");
            Assertions.Equal(null, resumed.Invariant,
                "An ordinary resumed compensation has no unrestorable residual.");
            Assertions.True(FirearmCraftingCompensationPolicy.RestoresPreState(
                resumed, 100, moneyNow, new[] { outputBefore }, new[] { countNow }),
                "Applying the resumed plan restores the pre-transaction state exactly.");

            // Cumulative accounting: the two passes together return exactly the
            // cost and remove exactly the insertion, never more.
            Assertions.Equal((int)Cost, refunded + (int)resumed.GoldToRestore,
                "The passes together must return exactly the cost.");
            Assertions.Equal(inserted, removed + resumed.ItemsToRemove[0],
                "The passes together must remove exactly the insertion.");

            // Resuming again after the second pass completes is a no-op.
            FirearmCraftingCompensationPlan settled =
                FirearmCraftingCompensationPolicy.Plan(100, 100, Cost,
                    new[] { outputBefore }, new[] { outputBefore },
                    new[] { inserted }, false, (long)Cost, new[] { inserted });
            Assertions.Equal(0, (int)settled.GoldToRestore,
                "A fully settled transaction refunds nothing further.");
            Assertions.Equal(0, settled.ItemsToRemove[0],
                "A fully settled transaction removes nothing further.");
            Assertions.Equal(null, settled.Invariant,
                "A fully settled transaction reports no residual.");
        }

        /// <summary>
        /// A retry must keep reporting an external residual. Measured against
        /// the ORIGINAL cost, a pass that had already refunded enough would
        /// report a clean restore while the external deficit was still there.
        /// </summary>
        internal static void AnExternalResidualSurvivesTheRetry()
        {
            const int external = 5;

            // 100 -> this transaction debits 22, something else debits 5 -> 73.
            FirearmCraftingCompensationPlan first = Plan(100, 100 - Cost - external,
                new[] { 0 }, new[] { 0 }, new[] { 0 });
            Assertions.Equal((int)Cost, (int)first.GoldToRestore,
                "Only this transaction's own debit may be refunded.");
            Assertions.True(first.Invariant != null,
                "The external debit must be reported on the first pass.");

            // Refund the whole 22 -> 95. The 5 is still missing and is not ours.
            FirearmCraftingCompensationPlan after =
                FirearmCraftingCompensationPolicy.Plan(100, 100 - external, Cost,
                    new[] { 0 }, new[] { 0 }, new[] { 0 }, false, (long)Cost, null);
            Assertions.Equal(0, (int)after.GoldToRestore,
                "A fully refunded transaction must not pay for an external debit.");
            Assertions.True(after.Invariant != null &&
                after.Invariant.Contains("only account for 0 more"),
                "The external residual must still be reported after the retry.");
            Assertions.False(FirearmCraftingCompensationPolicy.RestoresPreState(
                after, 100, 100 - external, new[] { 0 }, new[] { 0 }),
                "A state with an external residual must not claim to be restored.");

            // The same for items: our 20 are gone, but 3 we never added remain.
            FirearmCraftingCompensationPlan items =
                FirearmCraftingCompensationPolicy.Plan(100, 100, Cost,
                    new[] { 5 }, new[] { 8 }, new[] { 20 }, false, (long)Cost,
                    new[] { 20 });
            Assertions.Equal(0, items.ItemsToRemove[0],
                "A fully removed insertion must not remove items we never added.");
            Assertions.True(items.Invariant != null &&
                items.Invariant.Contains("only account for 0 more"),
                "An external item surplus must still be reported after the retry.");
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
                plan.Invariant.Contains("only account for " + Cost + " more"),
                "A larger external debit must be reported against the remaining budget.");
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
