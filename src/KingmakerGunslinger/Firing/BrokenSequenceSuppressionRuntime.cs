using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;

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
        internal static void OnCommittedDegradation(object wielder, object weapon)
        {
            Orders.Break(wielder, weapon);
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
