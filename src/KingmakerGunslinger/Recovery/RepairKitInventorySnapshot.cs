using System;
using System.Globalization;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Immutable Firearm Repair Kit count used for eligibility, verification,
    /// diagnostics, and transaction rollback.
    /// </summary>
    internal sealed class RepairKitInventorySnapshot : IEquatable<RepairKitInventorySnapshot>
    {
        internal RepairKitInventorySnapshot(int repairKits)
        {
            if (repairKits < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "repairKits",
                    repairKits,
                    "Firearm Repair Kit count cannot be negative.");
            }

            RepairKits = repairKits;
        }

        internal int RepairKits { get; private set; }

        internal bool HasOneKit
        {
            get { return RepairKits > 0; }
        }

        internal static RepairKitInventorySnapshot Capture(
            IRepairKitInventory inventory)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException("inventory");
            }

            int count = inventory.Count();
            if (count < 0)
            {
                throw new InvalidOperationException(
                    "A repair-kit inventory returned a negative count.");
            }

            return new RepairKitInventorySnapshot(count);
        }

        public bool Equals(RepairKitInventorySnapshot other)
        {
            return !ReferenceEquals(other, null) && RepairKits == other.RepairKits;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RepairKitInventorySnapshot);
        }

        public override int GetHashCode()
        {
            return RepairKits;
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "repairKits={0}",
                RepairKits);
        }
    }
}
