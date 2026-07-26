using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Coordinates the exact item-owned firearm state and one shared-inventory repair kit.
    /// All eligibility checks run before mutation. Any later failure attempts to restore
    /// both resources to their exact pre-operation values and is never reported as success.
    /// </summary>
    internal sealed class FirearmOverhaulTransactionService
    {
        internal FirearmOverhaulResult TryOverhaulWreckedToBroken(
            IFirearmOverhaulStateStore stateStore,
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
                    "The firearm overhaul state store returned a null state.");
            }

            RepairKitInventorySnapshot beforeInventory =
                RepairKitInventorySnapshot.Capture(inventory);
            FirearmOverhaulStatus? rejection = GetRejection(
                beforeState,
                beforeInventory);
            if (rejection.HasValue)
            {
                return new FirearmOverhaulResult(
                    rejection.Value,
                    beforeState,
                    beforeState,
                    beforeInventory,
                    beforeInventory);
            }

            FirearmState overhauledState =
                FirearmStateMachine.OverhaulWrecked(beforeState);
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
                stateStore.Replace(beforeState, overhauledState);

                FirearmState verifiedState = stateStore.Read();
                if (verifiedState != overhauledState)
                {
                    throw new InvalidOperationException(
                        "The exact firearm did not retain the expected empty/Broken overhaul state.");
                }

                RepairKitInventorySnapshot verifiedInventory =
                    RepairKitInventorySnapshot.Capture(inventory);
                if (!afterConsumption.Equals(verifiedInventory))
                {
                    throw new InvalidOperationException(
                        "Shared inventory changed unexpectedly after the exact firearm state was written.");
                }

                return new FirearmOverhaulResult(
                    FirearmOverhaulStatus.Overhauled,
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
                        RestoreState(stateStore, beforeState, overhauledState);
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

                throw new FirearmOverhaulTransactionException(
                    stateRollbackException == null && inventoryRollbackException == null
                        ? "Overhaul failed and both exact firearm state and repair-kit inventory were restored."
                        : "Overhaul failed and at least one rollback could not restore the exact pre-operation state.",
                    operationException,
                    stateRollbackException,
                    inventoryRollbackException);
            }
        }

        internal static FirearmOverhaulStatus? GetRejection(
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

            if (state.Condition != FirearmCondition.Wrecked)
            {
                return FirearmOverhaulStatus.NotWrecked;
            }

            if (!inventory.HasOneKit)
            {
                return FirearmOverhaulStatus.InsufficientRepairKit;
            }

            return null;
        }

        private static void RestoreState(
            IFirearmOverhaulStateStore stateStore,
            FirearmState expectedBefore,
            FirearmState attemptedOverhaul)
        {
            FirearmState current = stateStore.Read();
            if (current == expectedBefore)
            {
                return;
            }

            if (current != attemptedOverhaul)
            {
                throw new InvalidOperationException(
                    "Rollback refused to overwrite an unexpected concurrent firearm state.");
            }

            stateStore.Replace(attemptedOverhaul, expectedBefore);
            FirearmState restored = stateStore.Read();
            if (restored != expectedBefore)
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
