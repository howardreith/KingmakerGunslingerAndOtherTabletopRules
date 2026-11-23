using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Acquisition
{
    internal enum RetiredVendorHistoryDecision
    {
        // KnownItems is absent: the native diff is skipped, so every copy is
        // already retained and nothing is changed.
        NoHistory,
        // History records no retired row above its current published count.
        NoRetiredRows,
        // One or more retired history rows were lowered to the current
        // published count, so the native diff makes no decrement for them.
        Neutralized
    }

    internal sealed class RetiredVendorHistoryPlan<TItem>
    {
        internal RetiredVendorHistoryPlan(RetiredVendorHistoryDecision decision,
            KeyValuePair<TItem, int>[] neutralize)
        { Decision = decision; Neutralize = neutralize; }
        internal RetiredVendorHistoryDecision Decision { get; private set; }
        // Item and the history count to record for it (its current published
        // fixed-row count; zero for a fully retired row).
        internal KeyValuePair<TItem, int>[] Neutralize { get; private set; }
    }

    /// <summary>
    /// LIM-01 (Sprint 6 D1, owner policy of 2026-11-23): retire the authored
    /// shared-vendor publication rows, but never subtract an already
    /// materialized copy whose provenance cannot be proven.
    ///
    /// The native shared vendor table persists only blueprint/count entries
    /// and the known fixed-row history. An unsold original and an original
    /// that was bought and followed by a matching sale have the same saved
    /// representation, so no surviving field proves that a materialized copy
    /// is authored stock. For the exact retired table/item pairs, the known
    /// history row is lowered to the current published count before the
    /// native first-access diff, which then makes no decrement for that item.
    /// Every other item and table keeps native reconciliation.
    /// </summary>
    internal static class RetiredVendorHistoryPolicy
    {
        internal static RetiredVendorHistoryPlan<TItem> Plan<TItem>(
            IDictionary<TItem, int> known, IDictionary<TItem, int> published,
            IEnumerable<TItem> retired)
        {
            if (published == null) throw new ArgumentNullException("published");
            if (retired == null) throw new ArgumentNullException("retired");
            if (known == null)
                return new RetiredVendorHistoryPlan<TItem>(RetiredVendorHistoryDecision.NoHistory,
                    new KeyValuePair<TItem, int>[0]);
            var neutralize = new List<KeyValuePair<TItem, int>>();
            foreach (TItem item in retired.Distinct())
            {
                int knownCount;
                if (!known.TryGetValue(item, out knownCount)) continue;
                int publishedCount;
                if (!published.TryGetValue(item, out publishedCount)) publishedCount = 0;
                // Only a history count above the current publication would
                // make the native diff subtract; equal or lower is left alone.
                if (knownCount > publishedCount)
                    neutralize.Add(new KeyValuePair<TItem, int>(item, publishedCount));
            }
            return new RetiredVendorHistoryPlan<TItem>(neutralize.Count == 0 ?
                RetiredVendorHistoryDecision.NoRetiredRows : RetiredVendorHistoryDecision.Neutralized,
                neutralize.ToArray());
        }

        /// <summary>
        /// Applies a plan to the persisted history in place. A zero count is
        /// removed rather than stored: the native diff reads an absent row as
        /// zero, and the native code then replaces the history with the current
        /// fixed rows, so the next access and a fresh load see no difference.
        /// </summary>
        internal static void Apply<TItem>(IDictionary<TItem, int> known,
            RetiredVendorHistoryPlan<TItem> plan)
        {
            if (known == null || plan == null) return;
            foreach (KeyValuePair<TItem, int> row in plan.Neutralize)
            {
                if (row.Value == 0) known.Remove(row.Key);
                else known[row.Key] = row.Value;
                int after;
                bool present = known.TryGetValue(row.Key, out after);
                if (row.Value == 0 ? present : (!present || after != row.Value))
                    throw new InvalidOperationException("Retired vendor history row was not neutralized.");
            }
        }
    }
}
