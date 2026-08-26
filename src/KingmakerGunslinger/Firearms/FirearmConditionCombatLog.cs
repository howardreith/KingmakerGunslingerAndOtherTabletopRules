using System;
using System.Globalization;
using System.Threading;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Publishes one concise native combat-log entry only after an exact item-owned
    /// condition transition commits.
    /// </summary>
    internal static class FirearmConditionCombatLog
    {
        private static long _published;
        private static long _faults;
        private static string _lastMessage;

        internal static long Published { get { return Interlocked.Read(ref _published); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }
        internal static long Attempts { get { return Published + Faults; } }
        internal static string LastMessage { get { return _lastMessage; } }

        internal static string Format(string itemDisplayName,
            FirearmCondition before, FirearmCondition after, string reason)
        {
            string item = Normalize(itemDisplayName, "Firearm");
            string cause = NormalizeCause(reason);
            if (before == after)
                throw new ArgumentException(
                    "A condition notification requires an actual transition.",
                    "after");
            return string.Format(CultureInfo.InvariantCulture,
                "{0}: {1} ({2}).", item, after, cause);
        }

        internal static bool Publish(string itemDisplayName,
            FirearmCondition before, FirearmCondition after, string reason)
        {
            string message = Format(itemDisplayName, before, after, reason);
            _lastMessage = message;
            if (NativeCombatLog.Publish("firearm", "condition-log.failed",
                    message,
                    "The firearm condition committed, but its native combat-log entry failed."))
            {
                Interlocked.Increment(ref _published);
                return true;
            }
            Interlocked.Increment(ref _faults);
            return false;
        }

        private static string Normalize(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static string NormalizeCause(string reason)
        {
            string value = Normalize(reason, "condition change");
            return value.IndexOf("misfire", StringComparison.OrdinalIgnoreCase) >= 0
                ? "misfire" : value;
        }
    }
}
