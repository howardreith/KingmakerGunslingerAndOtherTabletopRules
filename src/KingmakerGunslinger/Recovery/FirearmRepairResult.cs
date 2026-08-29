using System;
using System.Globalization;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Immutable result of one player-facing ordinary repair attempt. Rejected results prove
    /// that neither the exact firearm state nor the Firearm Repair Kit count changed.
    /// </summary>
    internal sealed class FirearmRepairResult
    {
        internal FirearmRepairResult(
            FirearmRepairStatus status,
            FirearmState beforeState,
            FirearmState afterState,
            RepairKitInventorySnapshot beforeInventory,
            RepairKitInventorySnapshot afterInventory)
        {
            if (!Enum.IsDefined(typeof(FirearmRepairStatus), status))
            {
                throw new ArgumentOutOfRangeException(
                    "status",
                    status,
                    "Unknown firearm-repair status.");
            }

            Status = status;
            BeforeState = beforeState ?? throw new ArgumentNullException("beforeState");
            AfterState = afterState ?? throw new ArgumentNullException("afterState");
            BeforeInventory = beforeInventory ?? throw new ArgumentNullException("beforeInventory");
            AfterInventory = afterInventory ?? throw new ArgumentNullException("afterInventory");

            if (status == FirearmRepairStatus.Repaired)
            {
                if (BeforeState.Condition != FirearmCondition.Broken ||
                    AfterState.Condition != FirearmCondition.Normal ||
                    !AfterState.IsEmpty ||
                    AfterInventory.RepairKits != BeforeInventory.RepairKits - 1)
                {
                    throw new ArgumentException(
                        "A successful ordinary repair must change a Broken firearm to empty Normal, discard its loaded ammunition, and consume exactly one Firearm Repair Kit.");
                }
            }
            else if (BeforeState != AfterState ||
                !BeforeInventory.Equals(AfterInventory))
            {
                throw new ArgumentException(
                    "A rejected ordinary repair must leave exact firearm state and repair-kit inventory unchanged.");
            }
        }

        internal FirearmRepairStatus Status { get; private set; }

        internal FirearmState BeforeState { get; private set; }

        internal FirearmState AfterState { get; private set; }

        internal RepairKitInventorySnapshot BeforeInventory { get; private set; }

        internal RepairKitInventorySnapshot AfterInventory { get; private set; }

        internal bool Succeeded
        {
            get { return Status == FirearmRepairStatus.Repaired; }
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
