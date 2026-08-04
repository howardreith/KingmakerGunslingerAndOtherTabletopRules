using System;
using System.Globalization;
using System.Threading;
using Kingmaker.PubSubSystem;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Publishes one concise native warning/log event only after an exact item-owned
    /// condition transition commits. BattleLogManager consumes the same native event,
    /// so this is ordinary player UI rather than a Unity Mod Manager diagnostic.
    /// </summary>
    internal static class FirearmConditionCombatLog
    {
        private static long _published;
        private static long _faults;
        private static string _lastMessage;

        internal static long Published { get { return Interlocked.Read(ref _published); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }
        internal static string LastMessage { get { return _lastMessage; } }

        internal static string Format(string itemDisplayName,
            FirearmCondition before, FirearmCondition after, string reason)
        {
            string item = Normalize(itemDisplayName, "Firearm");
            string cause = Normalize(reason, "condition change");
            if (before == after)
                throw new ArgumentException(
                    "A condition notification requires an actual transition.",
                    "after");
            return string.Format(CultureInfo.InvariantCulture,
                "{0} condition: {1} -> {2} ({3}).",
                item, before, after, cause);
        }

        internal static bool Publish(string itemDisplayName,
            FirearmCondition before, FirearmCondition after, string reason)
        {
            string message = Format(itemDisplayName, before, after, reason);
            try
            {
                EventBus.RaiseEvent<IWarningNotificationUIHandler>(
                    handler => handler.HandleWarning(message, false));
                _lastMessage = message;
                Interlocked.Increment(ref _published);
                return true;
            }
            catch (Exception exception)
            {
                Interlocked.Increment(ref _faults);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("firearm", "condition-log.failed",
                        "The firearm condition committed, but its player-facing combat-log notification failed.",
                        exception);
                return false;
            }
        }

        private static string Normalize(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
