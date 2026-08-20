using System;
using System.Threading;
using Kingmaker.PubSubSystem;
using KingmakerGunslinger.Bootstrap;
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
        internal static string LastMessage { get { return _lastMessage; } }

        internal static bool Publish(FirearmDefinition definition,
            double distanceMeters, FirearmArmorClassDecision decision)
        {
            if (decision == null) throw new ArgumentNullException("decision");
            string message = FirearmArmorClassPresentation.Format(
                definition, distanceMeters,
                decision.EffectivePenetrationRangeFeet,
                decision.UsesTouchArmorClass, decision.Reason);
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
                    context.Logger.Failure("firearms", "ac-player-log.failed",
                        "The firearm AC branch committed, but its player-facing battle-log annotation failed.",
                        exception);
                return false;
            }
        }
    }
}
