using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KingmakerGunslinger.Acquisition
{
    /// <summary>
    /// Pure decisions for retired consumable maintenance-kit stock. Two exact
    /// blueprint identities were removed from every shop. This policy proves a
    /// published vendor table contains zero rows for a retired identity and
    /// selects the exact already-materialized items one vendor inventory sweep
    /// must remove. It never selects anything from the shared player inventory
    /// and never identifies non-retired items.
    /// </summary>
    internal static class RetiredVendorStockPolicy
    {
        /// <summary>
        /// Requires that a published vendor table exposes zero rows for one
        /// retired identity. One leftover row or many leftovers both fail.
        /// </summary>
        internal static void RequireTableAbsent(
            string vendorName,
            string retiredIdentity,
            int publishedRows)
        {
            if (publishedRows < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "publishedRows",
                    publishedRows,
                    "A published row count cannot be negative.");
            }

            if (publishedRows != 0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Vendor {0} still publishes {1} row(s) for retired maintenance identity {2}.",
                        vendorName,
                        publishedRows,
                        retiredIdentity));
            }
        }

        /// <summary>
        /// Selects the exact indexes of already-materialized retired items in one
        /// vendor inventory snapshot. The shared player inventory is protected:
        /// even exact retired matches there are never selected for removal, so
        /// player-owned legacy kits survive every shop sweep.
        /// </summary>
        internal static int[] SelectRetiredForRemoval(
            bool isSharedPlayerInventory,
            IList<string> materializedBlueprintKeys,
            ICollection<string> retiredKeys)
        {
            if (materializedBlueprintKeys == null)
            {
                throw new ArgumentNullException("materializedBlueprintKeys");
            }

            if (retiredKeys == null)
            {
                throw new ArgumentNullException("retiredKeys");
            }

            if (isSharedPlayerInventory)
            {
                return Array.Empty<int>();
            }

            var removal = new List<int>();
            for (int index = 0; index < materializedBlueprintKeys.Count; index++)
            {
                string key = materializedBlueprintKeys[index];
                if (key != null && retiredKeys.Contains(key))
                {
                    removal.Add(index);
                }
            }

            return removal.ToArray();
        }

        /// <summary>
        /// Formats the sweep outcome for diagnostics; a repeated sweep over
        /// already-clean stock must select nothing.
        /// </summary>
        internal static string DescribeSweep(
            string vendorName,
            int selectedCount,
            int removedCount)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "vendor={0};selected={1};removed={2}",
                vendorName,
                selectedCount,
                removedCount);
        }
    }
}
