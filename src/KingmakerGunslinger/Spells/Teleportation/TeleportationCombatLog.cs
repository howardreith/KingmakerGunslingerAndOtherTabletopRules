using Kingmaker.UI.Common;
using KingmakerGunslinger.Diagnostics;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal static class TeleportationCombatLog
    {
        internal static void Publish(string message, TeleportTransactionState state)
        {
            // The global-map UI normally has no BattleLogView. Its native warning
            // notification is the player-facing result in that context.
            var game = Kingmaker.Game.Instance;
            if (game != null && game.UI != null && game.UI.BattleLogManager != null && game.UI.BattleLogManager.LogView != null)
                NativeCombatLog.Publish("teleportation", "result.log-unavailable", message, state.ToString());
            // The native helper owns the transient notification. No extra dialog is
            // opened during the selected spell confirmation's hide animation.
            UIUtility.SendWarning(message);
        }
    }
}
