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
            : this(status, beforeState, afterState,
                ReloadAmmunitionProfileCatalog.LooseBasic,
                FromBasic(beforeInventory, "beforeInventory"),
                FromBasic(afterInventory, "afterInventory"))
        {
        }

        internal FirearmReloadResult(
            FirearmReloadStatus status,
            FirearmState beforeState,
            FirearmState afterState,
            ReloadAmmunitionProfile profile,
            ReloadAmmunitionInventorySnapshot beforeInventory,
            ReloadAmmunitionInventorySnapshot afterInventory)
        {
            if (!Enum.IsDefined(typeof(FirearmReloadStatus), status))
            {
                throw new ArgumentOutOfRangeException("status", status, "Unknown reload status.");
            }

            Status = status;
            BeforeState = beforeState ?? throw new ArgumentNullException("beforeState");
            AfterState = afterState ?? throw new ArgumentNullException("afterState");
            Profile = profile ?? throw new ArgumentNullException("profile");
            BeforeInventory = beforeInventory ?? throw new ArgumentNullException("beforeInventory");
            AfterInventory = afterInventory ?? throw new ArgumentNullException("afterInventory");

            if (status == FirearmReloadStatus.Loaded)
            {
                int roundsLoaded = AfterState.LoadedRounds - BeforeState.LoadedRounds;
                bool loadableCondition =
                    BeforeState.Condition == FirearmCondition.Normal ||
                    BeforeState.Condition == FirearmCondition.Broken;
                if (!loadableCondition ||
                    roundsLoaded <= 0 ||
                    AfterState.Condition != BeforeState.Condition ||
                    AfterState.LoadedAmmunition == null ||
                    (!BeforeState.IsEmpty && BeforeState.LoadedAmmunition != AfterState.LoadedAmmunition) ||
                    !HasExactConsumption(BeforeInventory, AfterInventory, Profile, roundsLoaded))
                {
                    throw new ArgumentException(
                        "A successful reload must preserve condition and ammunition identity, load a positive round count, and consume the matching component count.");
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

        internal ReloadAmmunitionProfile Profile { get; private set; }

        internal ReloadAmmunitionInventorySnapshot BeforeInventory { get; private set; }

        internal ReloadAmmunitionInventorySnapshot AfterInventory { get; private set; }

        internal bool Succeeded
        {
            get { return Status == FirearmReloadStatus.Loaded; }
        }

        internal int RoundsLoaded
        {
            get { return Succeeded ? AfterState.LoadedRounds - BeforeState.LoadedRounds : 0; }
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

        private static bool HasExactConsumption(ReloadAmmunitionInventorySnapshot before,
            ReloadAmmunitionInventorySnapshot after, ReloadAmmunitionProfile profile, int rounds)
        {
            try
            {
                ReloadAmmunitionTransactionService.VerifyDelta(before, after, profile, rounds);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private static ReloadAmmunitionInventorySnapshot FromBasic(
            BasicAmmunitionInventorySnapshot snapshot, string parameterName)
        {
            if (snapshot == null) throw new ArgumentNullException(parameterName);
            return new ReloadAmmunitionInventorySnapshot(
                snapshot.BlackPowderCharges, snapshot.LeadBalls, 0);
        }
    }
}
