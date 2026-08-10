using System;
using System.Globalization;
using System.Threading;
using Kingmaker.PubSubSystem;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Spells.ShieldOther
{
    internal static class ShieldOtherCombatLog
    {
        private static long _published;
        private static long _faults;
        private static string _lastMessage;

        internal static long Published { get { return Interlocked.Read(ref _published); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }
        internal static string LastMessage { get { return _lastMessage; } }

        internal static bool Publish(string subjectName, string casterName, int amount)
        {
            string message = string.Format(CultureInfo.InvariantCulture,
                "Shield Other transfers {0} damage from {1} to {2}.",
                amount, Normalize(subjectName, "the protected ally"),
                Normalize(casterName, "the caster"));
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
                    context.Logger.Failure("shield-other", "transfer-log.failed",
                        "Shield Other transferred damage, but its combat-log notification failed.",
                        exception);
                return false;
            }
        }

        private static string Normalize(string value, string fallback)
        { return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim(); }
    }
}
