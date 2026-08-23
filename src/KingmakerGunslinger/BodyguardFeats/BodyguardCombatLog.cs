using System;
using System.Globalization;
using System.Threading;
using Kingmaker.PubSubSystem;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.BodyguardFeats
{
    internal static class BodyguardCombatLog
    {
        private static long _published;
        private static long _faults;
        private static string _lastMessage;

        internal static long Published { get { return Interlocked.Read(ref _published); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }
        internal static string LastMessage { get { return _lastMessage; } }

        internal static bool PublishAttempt(string protector, string ally,
            string attacker, int naturalRoll, int attackBonus, bool success,
            int armorClassContribution)
        {
            string message = string.Format(CultureInfo.InvariantCulture,
                "{0} spends an attack of opportunity to Bodyguard {1} against {2}: {3} + {4} = {5} vs AC 10 ({6}{7}).",
                Normalize(protector, "The protector"),
                Normalize(ally, "the ally"), Normalize(attacker, "the attacker"),
                naturalRoll, attackBonus, naturalRoll + attackBonus,
                success ? "success" : "failure", success ? "; +" +
                    armorClassContribution.ToString(CultureInfo.InvariantCulture) +
                    " AC" : "");
            return Publish(message, "attempt-log.failed");
        }

        internal static bool PublishInterception(string protector, string ally,
            string attacker, bool nextTurnSwiftDebt)
        {
            string message = string.Format(CultureInfo.InvariantCulture,
                "{0} spends an immediate action for In Harm's Way: {1}'s attack remains a hit, and its complete delivery moves from {2} to {0}.{3}",
                Normalize(protector, "The protector"),
                Normalize(attacker, "The attacker"),
                Normalize(ally, "the protected ally"), nextTurnSwiftDebt ?
                    " The next actual turn's swift action is consumed." :
                    string.Empty);
            return Publish(message, "interception-log.failed");
        }

        internal static bool PublishImmediateUnavailable(string protector,
            string reason)
        {
            string message = string.Format(CultureInfo.InvariantCulture,
                "{0} cannot use In Harm's Way: {1}",
                Normalize(protector, "The protector"),
                Explain(reason));
            return Publish(message, "immediate-unavailable-log.failed");
        }

        private static bool Publish(string message, string faultCode)
        {
            try
            {
                EventBus.RaiseEvent<IWarningNotificationUIHandler>(
                    handler => handler.HandleWarning(message, true));
                _lastMessage = message;
                Interlocked.Increment(ref _published);
                return true;
            }
            catch (Exception exception)
            {
                Interlocked.Increment(ref _faults);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("bodyguard", faultCode,
                        "The combat reaction committed, but its player-facing log failed.",
                        exception);
                return false;
            }
        }

        private static string Normalize(string value, string fallback)
        { return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim(); }

        private static string Explain(string reason)
        {
            switch (reason)
            {
                case "protector-flat-footed":
                    return "it is flat-footed.";
                case "immediate-debt-pending-next-turn":
                    return "an immediate action was already used and its next actual turn's swift action is charged.";
                case "immediate-debt-charged-turn":
                    return "an immediate action was already used and this turn's swift action is unavailable.";
                case "swift-action-spent-this-turn":
                    return "its swift action for this turn has already been spent.";
                case "swift-cooldown-active":
                    return "the shared swift/immediate-action cooldown is active.";
                case "protector-dead":
                    return "it is dead.";
                case "protector-unconscious":
                    return "it is unconscious.";
                case "protector-incapacitated":
                case "protector-unable-to-act":
                    return "it is incapacitated.";
                default:
                    return "the immediate-action contract is unavailable (" +
                        Normalize(reason, "unknown reason") + ").";
            }
        }
    }
}
