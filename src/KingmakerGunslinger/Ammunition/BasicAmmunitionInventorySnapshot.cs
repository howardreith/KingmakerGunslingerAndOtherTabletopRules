using System;
using System.Globalization;

namespace KingmakerGunslinger.Ammunition
{
    /// <summary>
    /// Immutable count snapshot used for verification, diagnostics, and rollback.
    /// </summary>
    internal sealed class BasicAmmunitionInventorySnapshot : IEquatable<BasicAmmunitionInventorySnapshot>
    {
        internal BasicAmmunitionInventorySnapshot(int blackPowderCharges, int leadBalls)
        {
            if (blackPowderCharges < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "blackPowderCharges",
                    blackPowderCharges,
                    "Black-powder count cannot be negative.");
            }

            if (leadBalls < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "leadBalls",
                    leadBalls,
                    "Lead-ball count cannot be negative.");
            }

            BlackPowderCharges = blackPowderCharges;
            LeadBalls = leadBalls;
        }

        internal int BlackPowderCharges { get; private set; }

        internal int LeadBalls { get; private set; }

        internal bool HasOneLoad
        {
            get { return BlackPowderCharges > 0 && LeadBalls > 0; }
        }

        internal static BasicAmmunitionInventorySnapshot Capture(
            IBasicAmmunitionInventory inventory)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException("inventory");
            }

            return new BasicAmmunitionInventorySnapshot(
                ReadNonnegativeCount(inventory, BasicAmmunitionComponent.BlackPowderCharge),
                ReadNonnegativeCount(inventory, BasicAmmunitionComponent.LeadBall));
        }

        internal int Count(BasicAmmunitionComponent component)
        {
            switch (component)
            {
                case BasicAmmunitionComponent.BlackPowderCharge:
                    return BlackPowderCharges;
                case BasicAmmunitionComponent.LeadBall:
                    return LeadBalls;
                default:
                    throw new ArgumentOutOfRangeException(
                        "component",
                        component,
                        "Unknown basic-ammunition component.");
            }
        }

        public bool Equals(BasicAmmunitionInventorySnapshot other)
        {
            return !ReferenceEquals(other, null) &&
                BlackPowderCharges == other.BlackPowderCharges &&
                LeadBalls == other.LeadBalls;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BasicAmmunitionInventorySnapshot);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (BlackPowderCharges * 397) ^ LeadBalls;
            }
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "blackPowder={0}; leadBalls={1}; completeLoads={2}",
                BlackPowderCharges,
                LeadBalls,
                Math.Min(BlackPowderCharges, LeadBalls));
        }

        private static int ReadNonnegativeCount(
            IBasicAmmunitionInventory inventory,
            BasicAmmunitionComponent component)
        {
            int count = inventory.Count(component);
            if (count < 0)
            {
                throw new InvalidOperationException(
                    "An ammunition inventory returned a negative count for " + component + ".");
            }

            return count;
        }
    }
}
