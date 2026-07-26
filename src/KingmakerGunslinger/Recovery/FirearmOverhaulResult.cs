using System;
using System.Globalization;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Immutable result of one same-item overhaul attempt. Rejected results prove that
    /// neither the exact firearm state nor the Firearm Repair Kit count changed.
    /// </summary>
    internal sealed class FirearmOverhaulResult
    {
        internal FirearmOverhaulResult(
            FirearmOverhaulStatus status,
            FirearmState beforeState,
            FirearmState afterState,
            RepairKitInventorySnapshot beforeInventory,
            RepairKitInventorySnapshot afterInventory)
        {
            if (!Enum.IsDefined(typeof(FirearmOverhaulStatus), status))
            {
                throw new ArgumentOutOfRangeException(
                    "status",
                    status,
                    "Unknown firearm-overhaul status.");
            }

            Status = status;
            BeforeState = beforeState ?? throw new ArgumentNullException("beforeState");
            AfterState = afterState ?? throw new ArgumentNullException("afterState");
            BeforeInventory = beforeInventory ?? throw new ArgumentNullException("beforeInventory");
            AfterInventory = afterInventory ?? throw new ArgumentNullException("afterInventory");

            if (status == FirearmOverhaulStatus.Overhauled)
            {
                if (BeforeState.Condition != FirearmCondition.Wrecked ||
                    !BeforeState.IsEmpty ||
                    AfterState.Condition != FirearmCondition.Broken ||
                    !AfterState.IsEmpty ||
                    AfterInventory.RepairKits != BeforeInventory.RepairKits - 1)
                {
                    throw new ArgumentException(
                        "A successful overhaul must change an empty Wrecked firearm to empty Broken and consume exactly one Firearm Repair Kit.");
                }
            }
            else if (BeforeState != AfterState ||
                !BeforeInventory.Equals(AfterInventory))
            {
                throw new ArgumentException(
                    "A rejected overhaul must leave exact firearm state and repair-kit inventory unchanged.");
            }
        }

        internal FirearmOverhaulStatus Status { get; private set; }

        internal FirearmState BeforeState { get; private set; }

        internal FirearmState AfterState { get; private set; }

        internal RepairKitInventorySnapshot BeforeInventory { get; private set; }

        internal RepairKitInventorySnapshot AfterInventory { get; private set; }

        internal bool Succeeded
        {
            get { return Status == FirearmOverhaulStatus.Overhauled; }
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "status={0}; beforeState=[{1}]; afterState=[{2}]; beforeInventory=[{3}]; afterInventory=[{4}]",
                Status,
                BeforeState,
                AfterState,
                BeforeInventory,
                AfterInventory);
        }
    }
}
