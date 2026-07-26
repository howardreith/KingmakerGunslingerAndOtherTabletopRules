using System;
using System.Globalization;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Reloading
{
    /// <summary>
    /// Immutable result of one reload attempt. Rejected results prove that neither the
    /// exact firearm state nor either shared-inventory component changed.
    /// </summary>
    internal sealed class FirearmReloadResult
    {
        internal FirearmReloadResult(
            FirearmReloadStatus status,
            FirearmState beforeState,
            FirearmState afterState,
            BasicAmmunitionInventorySnapshot beforeInventory,
            BasicAmmunitionInventorySnapshot afterInventory)
        {
            if (!Enum.IsDefined(typeof(FirearmReloadStatus), status))
            {
                throw new ArgumentOutOfRangeException("status", status, "Unknown reload status.");
            }

            Status = status;
            BeforeState = beforeState ?? throw new ArgumentNullException("beforeState");
            AfterState = afterState ?? throw new ArgumentNullException("afterState");
            BeforeInventory = beforeInventory ?? throw new ArgumentNullException("beforeInventory");
            AfterInventory = afterInventory ?? throw new ArgumentNullException("afterInventory");

            if (status == FirearmReloadStatus.Loaded)
            {
                bool loadableCondition =
                    BeforeState.Condition == FirearmCondition.Normal ||
                    BeforeState.Condition == FirearmCondition.Broken;
                if (!loadableCondition ||
                    !BeforeState.IsEmpty ||
                    AfterState.Condition != BeforeState.Condition ||
                    AfterState.LoadedRounds != 1 ||
                    AfterState.LoadedAmmunition == null ||
                    AfterInventory.BlackPowderCharges != BeforeInventory.BlackPowderCharges - 1 ||
                    AfterInventory.LeadBalls != BeforeInventory.LeadBalls - 1)
                {
                    throw new ArgumentException(
                        "A successful reload must preserve an empty Normal or Broken firearm's condition, load exactly one round, and consume exactly one of each component.");
                }
            }
            else
            {
                if (BeforeState != AfterState || !BeforeInventory.Equals(AfterInventory))
                {
                    throw new ArgumentException(
                        "A rejected reload must leave both firearm state and inventory unchanged.");
                }
            }
        }

        internal FirearmReloadStatus Status { get; private set; }

        internal FirearmState BeforeState { get; private set; }

        internal FirearmState AfterState { get; private set; }

        internal BasicAmmunitionInventorySnapshot BeforeInventory { get; private set; }

        internal BasicAmmunitionInventorySnapshot AfterInventory { get; private set; }

        internal bool Succeeded
        {
            get { return Status == FirearmReloadStatus.Loaded; }
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
