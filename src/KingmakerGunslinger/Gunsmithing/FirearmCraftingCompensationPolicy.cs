using System;
using System.Linq;

namespace KingmakerGunslinger.Gunsmithing
{
    /// <summary>The exact compensation a failed crafting attempt should apply.</summary>
    internal sealed class FirearmCraftingCompensationPlan
    {
        internal FirearmCraftingCompensationPlan(long goldToRestore,
            int[] itemsToRemove, bool removeMarker, string invariant)
        {
            GoldToRestore = goldToRestore;
            ItemsToRemove = itemsToRemove ?? new int[0];
            RemoveMarker = removeMarker;
            Invariant = invariant;
        }

        /// <summary>Never more than this transaction actually took.</summary>
        internal long GoldToRestore { get; private set; }
        /// <summary>Per output, never more than this transaction actually added
        /// and never more than is currently present.</summary>
        internal int[] ItemsToRemove { get; private set; }
        /// <summary>True only when this transaction added the marker itself.</summary>
        internal bool RemoveMarker { get; private set; }
        /// <summary>
        /// Null when the pre-transaction state can be restored exactly.
        /// Otherwise a description of what this transaction cannot put back,
        /// which the caller must report WITHOUT discarding the original cause.
        /// </summary>
        internal string Invariant { get; private set; }
    }

    /// <summary>
    /// GS-AUD-0036. Decides a failed firearm-crafting attempt's compensation
    /// from what actually committed, rather than from a recomputed difference.
    ///
    /// The previous rollback had three problems:
    ///
    /// 1. It refunded <c>moneyBefore - player.Money</c>, a difference that can
    ///    absorb a change this transaction never made, so it could hand back
    ///    gold it never took. It then detected that over-refund and threw,
    ///    leaving the over-refund in place.
    /// 2. That detection threw a fresh exception with no inner exception, so
    ///    the real reason the craft failed was unrecoverable from the logs.
    /// 3. It removed the entitlement marker whenever the caster had it,
    ///    rather than only when this transaction added it. This is ownership
    ///    HARDENING, not a demonstrated defect: Complete rejects
    ///    caster.HasFact(marker) at entry, before the try block, so an
    ///    ordinary attempt that begins with the marker already held cannot
    ///    reach the rollback at all. Tracking ownership removes the dependence
    ///    on that entry guard and on no other system granting the marker
    ///    mid-transaction, neither of which this compensation should have to
    ///    assume.
    ///
    /// This policy is pure arithmetic over observed counts so it is directly
    /// testable, and it is consumed by the production service rather than
    /// existing only for tests.
    ///
    /// Clamping is what makes the compensation exact: gold is restored up to
    /// what this transaction could have debited and no more, and each output is
    /// removed up to what this transaction actually added and what is still
    /// present.
    ///
    /// Repeat-safety needs more than the clamp, and the clamp alone does not
    /// provide it. After a compensation that fully restored the pre-transaction
    /// state the observed debit is zero, so replanning returns nothing - but
    /// after a compensation whose invariant was violated, for example an
    /// external debit larger than this cost, the observed difference is still
    /// non-zero and replanning would refund again:
    ///
    ///     moneyBefore 100, goldCost 22, moneyNow 28  -> restore 22 (money 50)
    ///     replan from 50 with no bookkeeping         -> restore 22 again
    ///
    /// So callers that may run compensation more than once must pass what they
    /// have already given back and already removed, through
    /// <paramref name="alreadyRefunded"/> and <paramref name="alreadyRemoved"/>.
    /// With those declared the plan is genuinely repeat-safe. The production
    /// service applies the plan exactly once and therefore passes zero, but the
    /// parameters exist so a retry is correct by construction rather than by
    /// relying on the single-call assumption.
    /// </summary>
    internal static class FirearmCraftingCompensationPolicy
    {
        internal static FirearmCraftingCompensationPlan Plan(long moneyBefore,
            long moneyNow, long goldCost, int[] countsBefore, int[] countsNow,
            int[] committedAdds, bool markerAddedHere, long alreadyRefunded = 0,
            int[] alreadyRemoved = null)
        {
            if (goldCost < 0) throw new ArgumentOutOfRangeException("goldCost");
            if (alreadyRefunded < 0) throw new ArgumentOutOfRangeException("alreadyRefunded");
            if (countsBefore == null) throw new ArgumentNullException("countsBefore");
            if (countsNow == null) throw new ArgumentNullException("countsNow");
            if (committedAdds == null) throw new ArgumentNullException("committedAdds");
            if (countsNow.Length != countsBefore.Length ||
                committedAdds.Length != countsBefore.Length)
                throw new ArgumentException("Crafting compensation inputs disagree in length.");

            if (alreadyRemoved != null && alreadyRemoved.Length != countsBefore.Length)
                throw new ArgumentException("Crafting compensation inputs disagree in length.");

            // Only what this transaction could have taken, never a difference
            // some other system caused, and never more than it has not already
            // given back.
            long observedDebit = moneyBefore - moneyNow;
            long restore = Math.Max(0, Math.Min(goldCost, observedDebit) - alreadyRefunded);

            var remove = new int[countsBefore.Length];
            for (int index = 0; index < remove.Length; index++)
            {
                if (committedAdds[index] < 0)
                    throw new ArgumentOutOfRangeException("committedAdds");
                int settled = alreadyRemoved == null ? 0 : alreadyRemoved[index];
                if (settled < 0) throw new ArgumentOutOfRangeException("alreadyRemoved");
                int present = countsNow[index] - countsBefore[index];
                remove[index] = Math.Max(0, Math.Min(
                    committedAdds[index] - settled, present));
            }

            // What this transaction cannot put back by itself. Reported, never
            // "corrected" by taking gold or items it does not own.
            string invariant = null;
            if (observedDebit > goldCost)
                invariant = "gold decreased by " + observedDebit +
                    " but this transaction could only debit " + goldCost +
                    "; the difference was not made here and is not refunded";
            else if (observedDebit < 0)
                invariant = "gold increased by " + (-observedDebit) +
                    " during the attempt; nothing is reclaimed";
            else
            {
                for (int index = 0; index < remove.Length; index++)
                {
                    int present = countsNow[index] - countsBefore[index];
                    if (present > committedAdds[index])
                    {
                        invariant = "output " + index + " gained " + present +
                            " but this transaction added " + committedAdds[index] +
                            "; the surplus was not made here and is not removed";
                        break;
                    }
                    if (present < 0)
                    {
                        invariant = "output " + index + " lost " + (-present) +
                            " during the attempt; nothing is restored";
                        break;
                    }
                }
            }

            return new FirearmCraftingCompensationPlan(restore, remove,
                markerAddedHere, invariant);
        }

        /// <summary>True when the plan restores the pre-transaction state exactly.</summary>
        internal static bool RestoresPreState(FirearmCraftingCompensationPlan plan,
            long moneyBefore, long moneyNow, int[] countsBefore, int[] countsNow)
        {
            if (plan == null) throw new ArgumentNullException("plan");
            if (plan.Invariant != null) return false;
            if (moneyNow + plan.GoldToRestore != moneyBefore) return false;
            return !countsBefore.Where((value, index) =>
                countsNow[index] - plan.ItemsToRemove[index] != value).Any();
        }
    }
}
