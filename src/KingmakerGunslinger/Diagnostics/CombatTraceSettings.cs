using System.Threading;

namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// Process-lifetime switch for verbose read-only firearm diagnostics. It defaults off
    /// and is intentionally not persisted into a campaign or Unity Mod Manager settings file.
    /// </summary>
    internal static class CombatTraceSettings
    {
        private static int _enabled;

        internal static bool Enabled
        {
            get { return Volatile.Read(ref _enabled) != 0; }
        }

        internal static bool SetEnabled(bool enabled)
        {
            int next = enabled ? 1 : 0;
            int previous = Interlocked.Exchange(ref _enabled, next);
            return previous != next;
        }
    }
}
