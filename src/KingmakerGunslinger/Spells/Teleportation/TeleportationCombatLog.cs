using Kingmaker.UI.Common;
using KingmakerGunslinger.Diagnostics;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal static class TeleportationCombatLog
    {
        internal static void Publish(string message, TeleportTransactionState state)
        {
            NativeCombatLog.Publish("teleportation", "result.log-unavailable", message, state.ToString());
            // The native helper owns the transient notification. No extra dialog is
            // opened during the selected spell confirmation's hide animation.
            UIUtility.SendWarning(message);
        }
    }
}
