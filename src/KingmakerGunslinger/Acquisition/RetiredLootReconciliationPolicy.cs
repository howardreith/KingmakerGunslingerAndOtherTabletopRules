using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Acquisition
{
    internal enum RetiredLootDecision
    {
        /// <summary>The container has no persisted known-item history. The
        /// engine skips reconciliation, so every instance is retained.</summary>
        NoHistoryRetained,
        /// <summary>The retired item is absent from the container's known
        /// history: a fresh container, or one already reconciled.</summary>
        NotInHistory,
        /// <summary>The history records the retired row but no instance is in
        /// the container any more (it was claimed). Only the history row is
        /// retired; the engine would otherwise log a failed removal.</summary>
        AlreadyClaimed,
        /// <summary>Every present instance is the untouched generated reward
        /// and is removed exactly.</summary>
        RemovedUntouched,
        /// <summary>Untouched rewards are removed exactly; other present
        /// instances have party-held provenance and are retained.</summary>
        RemovedUntouchedRetainedAmbiguous,
        /// <summary>Every present instance has been held by the party
        /// (claimed and returned, or deposited) and is retained.</summary>
        RetainedAmbiguous
    }

    internal sealed class RetiredLootPlan<T> where T : class
    {
        internal RetiredLootPlan(RetiredLootDecision decision, IList<T> remove,
            IList<T> retain, bool retireHistory)
        {
            Decision = decision; Remove = remove.ToArray(); Retain = retain.ToArray();
            RetireHistory = retireHistory;
        }
        internal RetiredLootDecision Decision { get; private set; }
        internal T[] Remove { get; private set; }
        internal T[] Retain { get; private set; }
        /// <summary>Set the retired item's known count to zero before native
        /// reconciliation so the engine never decrements by blueprint.</summary>
        internal bool RetireHistory { get; private set; }
    }

    /// <summary>
    /// Existing-save handling for a named weapon retired from its exact authored
    /// loot source (Thornstep, 2026-11-11). Kingmaker 2.1.7b
    /// <c>LootPersistentData.UpdateLoot</c> would subtract a withdrawn row by
    /// blueprint and count, which cannot tell an untouched reward from a copy the
    /// player claimed and later put back. Provenance comes from the persisted
    /// <c>ItemEntity.Time</c>. The entity constructor leaves it at zero, and
    /// <c>ItemsCollection.Add</c> stamps the current game time only when an item
    /// enters the party inventory (<c>Split</c> also stamps it). A zero-time
    /// instance in the source container has therefore never been held by the
    /// party. Any other instance is ambiguous and is retained. The plan never
    /// removes more than the history recorded, never touches another item, and
    /// is idempotent because the history row is retired to zero.
    /// </summary>
    internal static class RetiredLootReconciliationPolicy
    {
        internal static RetiredLootPlan<T> Plan<T>(bool historyPresent,
            int knownRetiredCount, IEnumerable<T> retiredInContainer,
            Func<T, bool> neverHeldByParty) where T : class
        {
            if (retiredInContainer == null) throw new ArgumentNullException("retiredInContainer");
            if (neverHeldByParty == null) throw new ArgumentNullException("neverHeldByParty");
            T[] present = retiredInContainer.Where(value => value != null).ToArray();
            if (!historyPresent)
                return new RetiredLootPlan<T>(RetiredLootDecision.NoHistoryRetained,
                    new T[0], present, false);
            if (knownRetiredCount <= 0)
                return new RetiredLootPlan<T>(RetiredLootDecision.NotInHistory,
                    new T[0], present, false);
            T[] untouched = present.Where(neverHeldByParty).Take(knownRetiredCount).ToArray();
            T[] retained = present.Where(value => !untouched.Contains(value)).ToArray();
            RetiredLootDecision decision = present.Length == 0
                ? RetiredLootDecision.AlreadyClaimed
                : untouched.Length == 0 ? RetiredLootDecision.RetainedAmbiguous
                : retained.Length == 0 ? RetiredLootDecision.RemovedUntouched
                : RetiredLootDecision.RemovedUntouchedRetainedAmbiguous;
            return new RetiredLootPlan<T>(decision, untouched, retained, true);
        }

        /// <summary>The engine's own provenance test, exposed for the adapter
        /// and tests: zero means never stamped by the party inventory.</summary>
        internal static bool NeverHeldByParty(TimeSpan itemTime)
        { return itemTime == TimeSpan.Zero; }
    }
}
