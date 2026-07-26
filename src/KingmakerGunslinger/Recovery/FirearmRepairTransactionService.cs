using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Coordinates ordinary same-item Broken-to-Normal repair with one shared-inventory
    /// Firearm Repair Kit. Eligibility is checked before mutation. Later failures attempt
    /// to restore both resources to their exact pre-operation values.
    /// </summary>
    internal sealed class FirearmRepairTransactionService
    {
        internal FirearmRepairResult TryRepairBrokenToNormal(
            IFirearmRepairStateStore stateStore,
            IRepairKitInventory inventory)
        {
            if (stateStore == null)
            {
                throw new ArgumentNullException("stateStore");
            }

            if (inventory == null)
            {
                throw new ArgumentNullException("inventory");
            }

            FirearmState beforeState = stateStore.Read();
            if (beforeState == null)
            {
                throw new InvalidOperationException(
                    "The firearm repair state store returned a null state.");
            }

            RepairKitInventorySnapshot beforeInventory =
                RepairKitInventorySnapshot.Capture(inventory);
            FirearmRepairStatus? rejection = GetRejection(
                beforeState,
                beforeInventory);
            if (rejection.HasValue)
            {
                return new FirearmRepairResult(
                    rejection.Value,
                    beforeState,
                    beforeState,
                    beforeInventory,
                    beforeInventory);
            }

            FirearmState repairedState = FirearmStateMachine.Repair(beforeState);
            bool inventoryMayHaveChanged = false;
            bool stateMayHaveChanged = false;

            try
            {
                inventoryMayHaveChanged = true;
                inventory.Remove(1);
                RepairKitInventorySnapshot afterConsumption =
                    RepairKitInventorySnapshot.Capture(inventory);
                if (afterConsumption.RepairKits != beforeInventory.RepairKits - 1)
                {
                    throw new InvalidOperationException(
                        "Shared inventory did not retain the exact expected repair-kit count after consumption.");
                }

                stateMayHaveChanged = true;
                stateStore.Replace(beforeState, repairedState);

                FirearmState verifiedState = stateStore.Read();
                if (verifiedState != repairedState)
                {
                    throw new InvalidOperationException(
                        "The exact firearm did not retain the expected empty/Normal ordinary repair state.");
                }

                RepairKitInventorySnapshot verifiedInventory =
                    RepairKitInventorySnapshot.Capture(inventory);
                if (!afterConsumption.Equals(verifiedInventory))
                {
                    throw new InvalidOperationException(
                        "Shared inventory changed unexpectedly after the exact firearm state was written.");
                }

                return new FirearmRepairResult(
                    FirearmRepairStatus.Repaired,
                    beforeState,
                    verifiedState,
                    beforeInventory,
                    verifiedInventory);
            }
            catch (Exception operationException)
            {
                Exception stateRollbackException = null;
                Exception inventoryRollbackException = null;

                if (stateMayHaveChanged)
                {
                    try
                    {
                        RestoreState(stateStore, beforeState, repairedState);
                    }
                    catch (Exception exception)
                    {
                        stateRollbackException = exception;
                    }
                }

                if (inventoryMayHaveChanged)
                {
                    try
                    {
                        RestoreInventory(inventory, beforeInventory);
                    }
                    catch (Exception exception)
                    {
                        inventoryRollbackException = exception;
                    }
                }

                throw new FirearmRepairTransactionException(
                    stateRollbackException == null && inventoryRollbackException == null
                        ? "Ordinary repair failed and both exact firearm state and repair-kit inventory were restored."
                        : "Ordinary repair failed and at least one rollback could not restore the exact pre-operation state.",
                    operationException,
                    stateRollbackException,
                    inventoryRollbackException);
            }
        }

        internal static FirearmRepairStatus? GetRejection(
            FirearmState state,
            RepairKitInventorySnapshot inventory)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            if (inventory == null)
            {
                throw new ArgumentNullException("inventory");
            }

            if (state.Condition != FirearmCondition.Broken)
            {
                return FirearmRepairStatus.NotBroken;
            }

            if (!state.IsEmpty)
            {
                return FirearmRepairStatus.Loaded;
            }

            if (!inventory.HasOneKit)
            {
                return FirearmRepairStatus.InsufficientRepairKit;
            }

            return null;
        }

        private static void RestoreState(
            IFirearmRepairStateStore stateStore,
            FirearmState expectedBefore,
            FirearmState attemptedRepair)
        {
            FirearmState current = stateStore.Read();
            if (current == expectedBefore)
            {
                return;
            }

            if (current != attemptedRepair)
            {
                throw new InvalidOperationException(
                    "Rollback refused to overwrite an unexpected concurrent firearm state.");
            }

            stateStore.Replace(attemptedRepair, expectedBefore);
            if (stateStore.Read() != expectedBefore)
            {
                throw new InvalidOperationException(
                    "Firearm-state rollback did not verify after replacement.");
            }
        }

        private static void RestoreInventory(
            IRepairKitInventory inventory,
            RepairKitInventorySnapshot expected)
        {
            int current = inventory.Count();
            if (current < 0)
            {
                throw new InvalidOperationException(
                    "Cannot restore a negative repair-kit inventory count.");
            }

            if (current < expected.RepairKits)
            {
                inventory.Add(expected.RepairKits - current);
            }
            else if (current > expected.RepairKits)
            {
                inventory.Remove(current - expected.RepairKits);
            }

            RepairKitInventorySnapshot restored =
                RepairKitInventorySnapshot.Capture(inventory);
            if (!expected.Equals(restored))
            {
                throw new InvalidOperationException(
                    "Repair-kit rollback verification failed. Expected [" +
                    expected + "]; observed [" + restored + "].");
            }
        }
    }
}
