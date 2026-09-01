using KingmakerGunslinger.Presentation;

namespace KingmakerGunslinger.Gunsmithing
{
    /// <summary>Keeps craft availability and failure text concise and safe.</summary>
    internal static class GunsmithingPlayerFacingReasonPolicy
    {
        [System.ThreadStatic] private static string _lastReason;

        internal static void Remember(string reason)
        {
            _lastReason = PlayerFacingTextPolicy.IsScreenSafe(reason) ?
                reason : "Cannot craft now.";
        }

        internal static string CurrentOrFallback()
        {
            return PlayerFacingTextPolicy.IsScreenSafe(_lastReason) ?
                _lastReason : "Cannot craft now.";
        }
    }
}
