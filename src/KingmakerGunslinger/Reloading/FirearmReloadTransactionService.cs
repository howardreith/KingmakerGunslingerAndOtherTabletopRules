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
        private readonly ReloadAmmunitionTransactionService _ammunitionService;

        internal FirearmReloadTransactionService()
            : this(new ReloadAmmunitionTransactionService())
        {
        }

        internal FirearmReloadTransactionService(
            ReloadAmmunitionTransactionService ammunitionService)
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
            return TryReloadBasicRounds(stateStore, inventory, rules, ammunition, 1);
        }

        internal FirearmReloadResult TryReloadBasicRounds(
            IFirearmReloadStateStore stateStore,
            IBasicAmmunitionInventory inventory,
            FirearmStateRules rules,
            AmmunitionId ammunition,
            int roundsPerAction)
        {
            if (ammunition == null)
            {
                throw new ArgumentNullException("ammunition");
            }
            ReloadAmmunitionProfile legacyProfile =
                ammunition == ReloadAmmunitionProfileCatalog.LooseBasic.LoadedAmmunition
                    ? ReloadAmmunitionProfileCatalog.LooseBasic
                    : new ReloadAmmunitionProfile(ammunition,
                        ReloadAmmunitionSourceKind.LooseBasic, "Loose basic ammunition",
                        null, new FirearmKind[0], 1, 0, 0);
            return TryReloadRounds(stateStore,
                new BasicReloadAmmunitionInventoryAdapter(inventory), rules,
                legacyProfile, roundsPerAction);
        }

        internal FirearmReloadResult TryReloadRounds(
            IFirearmReloadStateStore stateStore,
            IReloadAmmunitionInventory inventory,
            FirearmStateRules rules,
            ReloadAmmunitionProfile profile,
            int roundsPerAction)
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

            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }
            if (roundsPerAction <= 0 || roundsPerAction > rules.Capacity)
            {
                throw new ArgumentOutOfRangeException("roundsPerAction");
            }

            FirearmState beforeState = stateStore.Read();
            if (beforeState == null)
            {
                throw new InvalidOperationException(
                    "The firearm reload state store returned a null state.");
            }

            ReloadAmmunitionInventorySnapshot beforeInventory =
                ReloadAmmunitionInventorySnapshot.Capture(inventory);

            FirearmReloadStatus? rejected = GetRejection(
                beforeState,
                beforeInventory,
                rules,
                profile,
                roundsPerAction);
            if (rejected.HasValue)
            {
                return new FirearmReloadResult(
                    rejected.Value,
                    beforeState,
                    beforeState,
                    profile,
                    beforeInventory,
                    beforeInventory);
            }

            int roundsToLoad = Math.Min(roundsPerAction, rules.Capacity - beforeState.LoadedRounds);
            FirearmState loadedState = FirearmStateMachine.Load(
                beforeState,
                rules,
                profile.LoadedAmmunition,
                roundsToLoad);
            bool inventoryMayHaveChanged = false;
            bool stateMayHaveChanged = false;

            try
            {
                // The component transaction has its own rollback, but mark the inventory
                // as potentially changed before invoking it. A synthetic or engine failure
                // may occur after a partial write and before that inner rollback completes.
                inventoryMayHaveChanged = true;
                ReloadAmmunitionInventorySnapshot afterConsumption =
                    _ammunitionService.Consume(inventory, profile, roundsToLoad);
                if (afterConsumption == null)
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

                ReloadAmmunitionInventorySnapshot verifiedInventory =
                    ReloadAmmunitionInventorySnapshot.Capture(inventory);
                if (!afterConsumption.Equals(verifiedInventory))
                {
                    throw new InvalidOperationException(
                        "Shared inventory changed unexpectedly after the firearm state was written.");
                }

                return new FirearmReloadResult(
                    FirearmReloadStatus.Loaded,
                    beforeState,
                    verifiedState,
                    profile,
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
            return GetRejection(state,
                new ReloadAmmunitionInventorySnapshot(inventory.BlackPowderCharges, inventory.LeadBalls, 0),
                new FirearmStateRules(1,
                new[] { FirearmStateTokenCatalog.DiagnosticLeadBall }),
                ReloadAmmunitionProfileCatalog.LooseBasic, 1);
        }

        internal static FirearmReloadStatus? GetRejection(
            FirearmState state,
            BasicAmmunitionInventorySnapshot inventory,
            FirearmStateRules rules,
            AmmunitionId ammunition,
            int roundsPerAction)
        {
            if (ammunition != ReloadAmmunitionProfileCatalog.LooseBasic.LoadedAmmunition)
            {
                throw new ArgumentException("The legacy reload overload supports only loose basic ammunition.", "ammunition");
            }
            return GetRejection(state,
                new ReloadAmmunitionInventorySnapshot(inventory.BlackPowderCharges, inventory.LeadBalls, 0),
                rules, ReloadAmmunitionProfileCatalog.LooseBasic, roundsPerAction);
        }

        internal static FirearmReloadStatus? GetRejection(
            FirearmState state,
            ReloadAmmunitionInventorySnapshot inventory,
            FirearmStateRules rules,
            ReloadAmmunitionProfile profile,
            int roundsPerAction)
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

            if (state.LoadedRounds >= rules.Capacity)
            {
                return FirearmReloadStatus.AlreadyLoaded;
            }

            if (!rules.IsCompatible(profile.LoadedAmmunition))
            {
                throw new FirearmStateTransitionException(
                    FirearmStateTransitionError.IncompatibleAmmunition,
                    "The selected ammunition is incompatible with this firearm.");
            }

            if (!state.IsEmpty && state.LoadedAmmunition != profile.LoadedAmmunition)
            {
                throw new FirearmStateTransitionException(
                    FirearmStateTransitionError.MixedAmmunition,
                    "A partially loaded firearm cannot mix ammunition identities.");
            }

            int required = Math.Min(roundsPerAction, rules.Capacity - state.LoadedRounds);

            if (profile.SourceKind == ReloadAmmunitionSourceKind.PaperCartridge)
            {
                return inventory.PaperCartridges < required
                    ? FirearmReloadStatus.InsufficientPaperCartridge
                    : (FirearmReloadStatus?)null;
            }

            if (inventory.BlackPowderCharges < required)
            {
                return FirearmReloadStatus.InsufficientBlackPowder;
            }

            if (inventory.LeadBalls < required)
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
