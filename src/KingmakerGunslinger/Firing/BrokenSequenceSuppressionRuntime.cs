using System.Threading;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Firing
{
    /// <summary>
    /// Main-thread, transient order state. The ordinary misfire, Dead Shot,
    /// and Scatter Shot all call this only after a verified item commit.
    /// ConditionalWeakTable ownership lives in the dependency-free ledger.
    /// </summary>
    internal static class BrokenSequenceSuppressionRuntime
    {
        internal static FirearmAttackOrderLedger Orders = new FirearmAttackOrderLedger();
        private static long _recoveryCalls, _recoveryReleases;
        internal static long RecoveryCalls { get { return Interlocked.Read(ref _recoveryCalls); } }
        internal static long RecoveryReleases { get { return Interlocked.Read(ref _recoveryReleases); } }

        internal static void OnCommittedDegradation(object wielder, object weapon)
        {
            Orders.Break(wielder, weapon);
        }

        /// <summary>
        /// POL-01: called by Quick Clear, Repair Firearm and completed-rest
        /// maintenance only after their transition of this exact firearm has
        /// committed and can no longer be rolled back. Reads the item's
        /// committed condition itself; only Normal releases its misfire
        /// locks. Returns the locks released.
        /// </summary>
        internal static int OnCommittedRecovery(object weapon)
        {
            Interlocked.Increment(ref _recoveryCalls);
            if (weapon == null) return 0;
            try
            {
                FirearmCondition committed = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.Condition;
                int released = Orders.Recover(weapon, committed);
                if (released > 0) Interlocked.Add(ref _recoveryReleases, released);
                return released;
            }
            catch (System.Exception exception)
            {
                // Fail closed: the lock stays, the recovered item is unaffected.
                KingmakerGunslinger.Bootstrap.ModContext context;
                if (KingmakerGunslinger.Bootstrap.ModContext.TryGet(out context))
                    context.Logger.Failure("firearms", "recovery.release-failed",
                        "A committed firearm recovery could not release its misfire lock; the lock remains.",
                        exception);
                return 0;
            }
        }
        internal static bool IsSuppressed(object wielder, object weapon)
        {
            return Orders.IsSuppressed(wielder, weapon);
        }
        internal static int GetDegradationEpoch(object wielder, object weapon)
        {
            return Orders.Epoch(wielder, weapon);
        }
        internal static void ClearForRuntimeTest()
        {
            Orders = new FirearmAttackOrderLedger();
        }
    }
}
