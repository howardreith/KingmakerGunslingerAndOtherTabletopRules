using System;
using System.Globalization;
using System.Threading;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;

namespace KingmakerGunslinger.BodyguardFeats
{
    internal static class BodyguardCombatLog
    {
        private static long _published;
        private static long _faults;
        private static string _lastMessage;

        internal static long Published { get { return Interlocked.Read(ref _published); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }
        internal static long Attempts { get { return Published + Faults; } }
        internal static string LastMessage { get { return _lastMessage; } }

        internal static bool PublishAttempt(string protector, string ally,
            string attacker, int naturalRoll, int attackBonus, bool success,
            int armorClassContribution)
        {
            string message = string.Format(CultureInfo.InvariantCulture,
                "Bodyguard: {0} aids {1} against {2}. {3} + {4} = {5} vs AC 10 - {6}{7}.",
                Normalize(protector, "The protector"),
                Normalize(ally, "the ally"), Normalize(attacker, "the attacker"),
                naturalRoll, attackBonus, naturalRoll + attackBonus,
                success ? "success" : "failed", success ? " (+" +
                    armorClassContribution.ToString(CultureInfo.InvariantCulture) +
                    " AC)" : "");
            return Publish(message, "attempt-log.failed");
        }

        internal static bool PublishInterception(string protector, string ally,
            string attacker, bool nextTurnSwiftDebt)
        {
            string message = string.Format(CultureInfo.InvariantCulture,
                "In Harm's Way: {0} intercepts {1}'s attack on {2}.{3}",
                Normalize(protector, "The protector"),
                Normalize(attacker, "The attacker"),
                Normalize(ally, "the protected ally"), nextTurnSwiftDebt ?
                    " Swift action owed." :
                    string.Empty);
            return Publish(message, "interception-log.failed");
        }

        internal static bool PublishImmediateUnavailable(string protector,
            string reason)
        {
            string message = string.Format(CultureInfo.InvariantCulture,
                "In Harm's Way unavailable for {0}: {1}",
                Normalize(protector, "The protector"),
                Explain(reason));
            return Publish(message, "immediate-unavailable-log.failed");
        }

        private static bool Publish(string message, string faultCode)
        {
            _lastMessage = message;
            if (NativeCombatLog.Publish("bodyguard", faultCode, message,
                    "The combat reaction committed, but its native combat-log entry failed."))
            {
                Interlocked.Increment(ref _published);
                return true;
            }
            Interlocked.Increment(ref _faults);
            return false;
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
                    return "its next turn's swift action is already owed.";
                case "immediate-debt-charged-turn":
                    return "this turn's swift action is unavailable.";
                case "swift-action-spent-this-turn":
                    return "its swift action was already spent.";
                case "swift-cooldown-active":
                    return "the swift-action cooldown is active.";
                case "protector-dead":
                    return "it is dead.";
                case "protector-unconscious":
                    return "it is unconscious.";
                case "protector-incapacitated":
                case "protector-unable-to-act":
                    return "it is incapacitated.";
                default:
                    return "the immediate action is unavailable.";
            }
        }
    }
}
