using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Coordinates unified same-item Broken/Wrecked-to-Normal repair. The reusable
    /// Gunsmith's Kit is verified in the shared inventory before mutation and is
    /// never consumed, spent, or replaced. Eligibility is checked before mutation;
    /// a later failure attempts to restore the exact pre-operation firearm state.
    /// </summary>
    internal sealed class FirearmRepairTransactionService
    {
        internal FirearmRepairResult TryRepairToNormal(
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
            bool stateMayHaveChanged = false;

            try
            {
                stateMayHaveChanged = true;
                stateStore.Replace(beforeState, repairedState);

                FirearmState verifiedState = stateStore.Read();
                if (verifiedState != repairedState)
                {
                    throw new InvalidOperationException(
                        "The exact firearm did not retain the expected unified repair state.");
                }

                RepairKitInventorySnapshot verifiedInventory =
                    RepairKitInventorySnapshot.Capture(inventory);
                if (!beforeInventory.Equals(verifiedInventory))
                {
                    throw new InvalidOperationException(
                        "The reusable Gunsmith's Kit count changed during a repair that consumes nothing.");
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

                throw new FirearmRepairTransactionException(
                    stateRollbackException == null
                        ? "Unified repair failed and the exact firearm state was restored; no inventory mutation was attempted."
                        : "Unified repair failed and the firearm-state rollback could not restore the exact pre-operation state.",
                    operationException,
                    stateRollbackException,
                    null);
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

            if (state.Condition != FirearmCondition.Broken &&
                state.Condition != FirearmCondition.Wrecked)
            {
                return FirearmRepairStatus.NotBroken;
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
    }
}
