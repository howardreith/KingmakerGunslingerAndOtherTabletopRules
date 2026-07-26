using System;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Reloading
{
    /// <summary>
    /// Coordinates the item-owned firearm state and the two shared-inventory components.
    /// All eligibility checks run before mutation. After the first possible write, every
    /// failure attempts to restore both resources and is never reported as a successful reload.
    /// </summary>
    internal sealed class FirearmReloadTransactionService
    {
        private readonly BasicAmmunitionTransactionService _ammunitionService;

        internal FirearmReloadTransactionService()
            : this(new BasicAmmunitionTransactionService())
        {
        }

        internal FirearmReloadTransactionService(
            BasicAmmunitionTransactionService ammunitionService)
        {
            _ammunitionService = ammunitionService ??
                throw new ArgumentNullException("ammunitionService");
        }

        internal FirearmReloadResult TryReloadOneBasicRound(
            IFirearmReloadStateStore stateStore,
            IBasicAmmunitionInventory inventory,
            FirearmStateRules rules,
            AmmunitionId ammunition)
        {
            if (stateStore == null)
            {
                throw new ArgumentNullException("stateStore");
            }

            if (inventory == null)
            {
                throw new ArgumentNullException("inventory");
            }

            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }

            if (ammunition == null)
            {
                throw new ArgumentNullException("ammunition");
            }

            FirearmState beforeState = stateStore.Read();
            if (beforeState == null)
            {
                throw new InvalidOperationException(
                    "The firearm reload state store returned a null state.");
            }

            BasicAmmunitionInventorySnapshot beforeInventory =
                BasicAmmunitionInventorySnapshot.Capture(inventory);

            FirearmReloadStatus? rejected = GetRejection(
                beforeState,
                beforeInventory);
            if (rejected.HasValue)
            {
                return new FirearmReloadResult(
                    rejected.Value,
                    beforeState,
                    beforeState,
                    beforeInventory,
                    beforeInventory);
            }

            FirearmState loadedState = FirearmStateMachine.Load(
                beforeState,
                rules,
                ammunition,
                1);
            bool inventoryMayHaveChanged = false;
            bool stateMayHaveChanged = false;

            try
            {
                // The component transaction has its own rollback, but mark the inventory
                // as potentially changed before invoking it. A synthetic or engine failure
                // may occur after a partial write and before that inner rollback completes.
                inventoryMayHaveChanged = true;
                BasicAmmunitionTransactionResult consumption =
                    _ammunitionService.TryConsumeOneLoad(inventory);
                if (!consumption.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Basic ammunition became unavailable after reload eligibility was established.");
                }

                // Replace may mutate the item and then fail during verification, so the
                // rollback path must inspect the state even when Replace throws.
                stateMayHaveChanged = true;
                stateStore.Replace(beforeState, loadedState);

                FirearmState verifiedState = stateStore.Read();
                if (verifiedState != loadedState)
                {
                    throw new InvalidOperationException(
                        "The exact firearm did not retain the expected loaded state after replacement.");
                }

                BasicAmmunitionInventorySnapshot verifiedInventory =
                    BasicAmmunitionInventorySnapshot.Capture(inventory);
                if (!consumption.After.Equals(verifiedInventory))
                {
                    throw new InvalidOperationException(
                        "Shared inventory changed unexpectedly after the firearm state was written.");
                }

                return new FirearmReloadResult(
                    FirearmReloadStatus.Loaded,
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
                        RestoreState(stateStore, beforeState, loadedState);
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
                        _ammunitionService.RestoreExact(inventory, beforeInventory);
                    }
                    catch (Exception exception)
                    {
                        inventoryRollbackException = exception;
                    }
                }

                throw new FirearmReloadTransactionException(
                    stateRollbackException == null && inventoryRollbackException == null
                        ? "Reload failed and both firearm state and ammunition were restored."
                        : "Reload failed and at least one rollback could not restore the exact pre-operation state.",
                    operationException,
                    stateRollbackException,
                    inventoryRollbackException);
            }
        }

        internal static FirearmReloadStatus? GetRejection(
            FirearmState state,
            BasicAmmunitionInventorySnapshot inventory)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            if (inventory == null)
            {
                throw new ArgumentNullException("inventory");
            }

            if (state.Condition == FirearmCondition.Wrecked)
            {
                return FirearmReloadStatus.Wrecked;
            }

            if (!state.IsEmpty)
            {
                return FirearmReloadStatus.AlreadyLoaded;
            }

            if (inventory.BlackPowderCharges == 0)
            {
                return FirearmReloadStatus.InsufficientBlackPowder;
            }

            if (inventory.LeadBalls == 0)
            {
                return FirearmReloadStatus.InsufficientLeadBall;
            }

            return null;
        }

        private static void RestoreState(
            IFirearmReloadStateStore stateStore,
            FirearmState expectedBefore,
            FirearmState attemptedLoadedState)
        {
            FirearmState current = stateStore.Read();
            if (current == expectedBefore)
            {
                return;
            }

            if (current != attemptedLoadedState)
            {
                throw new InvalidOperationException(
                    "Rollback refused to overwrite an unexpected concurrent firearm state.");
            }

            stateStore.Replace(attemptedLoadedState, expectedBefore);
            FirearmState restored = stateStore.Read();
            if (restored != expectedBefore)
            {
                throw new InvalidOperationException(
                    "Firearm-state rollback did not verify after replacement.");
            }
        }
    }
}
