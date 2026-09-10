using System;
using System.Collections.Generic;
using KingmakerGunslinger.Acquisition;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Behavioral coverage for the retired maintenance-kit stock policy: the
    /// published-table absence guard must reject one leftover row and many
    /// leftover rows, and the vendor-inventory sweep selector must pick exactly
    /// the retired materialized items while protecting the shared player
    /// inventory and every unrelated item.
    /// </summary>
    internal static class RetiredVendorStockPolicyTests
    {
        private const string RepairKit = "KMG.Test.FirearmRepairKitItem";
        private const string OverhaulKit = "KMG.Gunsmithing.OverhaulKit";
        private const string GunsmithKit = "KMG.Gunsmithing.GunsmithKit";
        private const string BlackPowder = "KMG.Test.BlackPowderItem";

        internal static void TableAbsenceAcceptsZeroRows()
        {
            RetiredVendorStockPolicy.RequireTableAbsent(
                "capital-blacksmith", RepairKit, 0);
            RetiredVendorStockPolicy.RequireTableAbsent(
                "btsl-vendors", OverhaulKit, 0);
        }

        internal static void TableAbsenceRejectsOneLeftoverRow()
        {
            InvalidOperationException exception = Assertions.Throws<
                InvalidOperationException>(
                () => RetiredVendorStockPolicy.RequireTableAbsent(
                    "capital-blacksmith", RepairKit, 1),
                "One leftover published row must be rejected.");
            Assertions.True(exception.Message.Contains("1 row") &&
                exception.Message.Contains(RepairKit),
                "The rejection must name the exact row count and identity.");
        }

        internal static void TableAbsenceRejectsManyLeftoverRows()
        {
            Assertions.Throws<InvalidOperationException>(
                () => RetiredVendorStockPolicy.RequireTableAbsent(
                    "btsl-vendors", OverhaulKit, 3),
                "Many leftover published rows must be rejected.");
        }

        internal static void TableAbsenceRejectsNegativeRows()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => RetiredVendorStockPolicy.RequireTableAbsent(
                    "capital-blacksmith", RepairKit, -1),
                "A negative row count must fail closed.");
        }

        internal static void SweepSelectsSingleRetiredItem()
        {
            var stock = new[]
            {
                GunsmithKit, RepairKit, BlackPowder
            };
            int[] removal = RetiredVendorStockPolicy.SelectRetiredForRemoval(
                false, stock, RetiredKeys());
            Assertions.Equal(1, removal.Length,
                "A single retired item must be selected.");
            Assertions.Equal(1, removal[0],
                "The exact retired item index must be selected.");
        }

        internal static void SweepSelectsManyRetiredItemsOfBothIdentities()
        {
            var stock = new[]
            {
                BlackPowder, RepairKit, GunsmithKit, OverhaulKit,
                RepairKit, BlackPowder, OverhaulKit
            };
            int[] removal = RetiredVendorStockPolicy.SelectRetiredForRemoval(
                false, stock, RetiredKeys());
            CollectionOrderExact(removal, new[] { 1, 3, 4, 6 });
        }

        internal static void SweepNeverSelectsUnrelatedOrToolStock()
        {
            var stock = new[]
            {
                GunsmithKit, GunsmithKit, BlackPowder, "KMG.Test.LeadBulletItem"
            };
            int[] removal = RetiredVendorStockPolicy.SelectRetiredForRemoval(
                false, stock, RetiredKeys());
            Assertions.Equal(0, removal.Length,
                "Reusable-tool and unrelated stock must never be selected.");
        }

        internal static void SweepProtectsPlayerInventoryEvenWithRetiredKits()
        {
            var playerStock = new[]
            {
                RepairKit, RepairKit, OverhaulKit, GunsmithKit
            };
            int[] removal = RetiredVendorStockPolicy.SelectRetiredForRemoval(
                true, playerStock, RetiredKeys());
            Assertions.Equal(0, removal.Length,
                "Legacy player-owned obsolete kits must survive every shop sweep.");
        }

        internal static void SweepOnCleanStockSelectsNothing()
        {
            var cleanStock = new[]
            {
                GunsmithKit, BlackPowder, "KMG.Test.LeadBulletItem"
            };
            Assertions.Equal(0,
                RetiredVendorStockPolicy.SelectRetiredForRemoval(
                    false, cleanStock, RetiredKeys()).Length,
                "A repeated sweep over clean vendor stock must select nothing.");
        }

        internal static void SweepRejectsNullInputs()
        {
            Assertions.Throws<ArgumentNullException>(
                () => RetiredVendorStockPolicy.SelectRetiredForRemoval(
                    false, null, RetiredKeys()),
                "A null inventory snapshot must be rejected.");
            Assertions.Throws<ArgumentNullException>(
                () => RetiredVendorStockPolicy.SelectRetiredForRemoval(
                    false, new string[0], null),
                "A null retired identity set must be rejected.");
        }

        internal static void SweepDescriptionNamesVendorAndCounts()
        {
            string described = RetiredVendorStockPolicy.DescribeSweep(
                "OTP_Bokken", 3, 3);
            Assertions.True(described.Contains("vendor=OTP_Bokken") &&
                described.Contains("selected=3") &&
                described.Contains("removed=3"),
                "The sweep diagnostic must name the vendor and exact counts.");
        }

        private static ICollection<string> RetiredKeys()
        {
            return new HashSet<string> { RepairKit, OverhaulKit };
        }

        private static void CollectionOrderExact(int[] observed, int[] expected)
        {
            bool equal = observed.Length == expected.Length;
            if (equal)
            {
                for (int index = 0; index < observed.Length; index++)
                {
                    equal &= observed[index] == expected[index];
                }
            }

            Assertions.True(equal,
                "The removal selection must be exact and stable in order; observed=[" +
                string.Join(",", observed) + "] expected=[" +
                string.Join(",", expected) + "].");
        }
    }
}
