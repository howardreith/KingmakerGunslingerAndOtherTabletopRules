using System;
using System.Globalization;
using System.Threading;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;

namespace KingmakerGunslinger.Spells.ShieldOther
{
    internal static class ShieldOtherCombatLog
    {
        private static long _published;
        private static long _faults;
        private static string _lastMessage;

        internal static long Published { get { return Interlocked.Read(ref _published); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }
        internal static long Attempts { get { return Published + Faults; } }
        internal static string LastMessage { get { return _lastMessage; } }

        internal static bool Publish(string subjectName, string casterName, int amount)
        {
            string message = string.Format(CultureInfo.InvariantCulture,
                "Shield Other transfers {0} damage from {1} to {2}.",
                amount, Normalize(subjectName, "the protected ally"),
                Normalize(casterName, "the caster"));
            _lastMessage = message;
            if (NativeCombatLog.Publish("shield-other", "transfer-log.failed",
                    message,
                    "Shield Other transferred damage, but its native combat-log entry failed."))
            {
                Interlocked.Increment(ref _published);
                return true;
            }
            Interlocked.Increment(ref _faults);
            return false;
        }

        private static string Normalize(string value, string fallback)
        { return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim(); }
    }
}
