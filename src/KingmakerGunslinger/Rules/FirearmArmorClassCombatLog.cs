using System;
using System.Threading;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Rules
{
    internal static class FirearmArmorClassCombatLog
    {
        private static long _published;
        private static long _faults;
        private static string _lastMessage;

        internal static long Published { get { return Interlocked.Read(ref _published); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }
        internal static long Attempts { get { return Published + Faults; } }
        internal static string LastMessage { get { return _lastMessage; } }

        internal static bool Publish(FirearmDefinition definition,
            double distanceMeters, FirearmArmorClassDecision decision)
        {
            if (decision == null) throw new ArgumentNullException("decision");
            string message = FirearmArmorClassPresentation.Format(
                definition, distanceMeters,
                decision.EffectivePenetrationRangeFeet,
                decision.UsesTouchArmorClass, decision.Reason);
            _lastMessage = message;
            if (NativeCombatLog.Publish("firearms", "ac-player-log.failed",
                    message,
                    "The firearm AC branch committed, but its native combat-log annotation failed."))
            {
                Interlocked.Increment(ref _published);
                return true;
            }
            Interlocked.Increment(ref _faults);
            return false;
        }
    }
}
