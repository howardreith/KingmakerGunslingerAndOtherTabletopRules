using System;
using Kingmaker.UI.Common;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Native Kingmaker adapter for post-commit firearm degradation feedback.
    /// In Kingmaker 2.1.7b, UIUtility.SendWarning(string) routes through the
    /// native warning event, whose WarningsText handler owns the transient
    /// warning presentation.
    /// </summary>
    internal static class FirearmConditionTopNotification
    {
        internal const string NativeApi =
            "Kingmaker.UI.Common.UIUtility.SendWarning(System.String)";
        internal const string NativeHandler =
            "Kingmaker.UI.WarningsText";

        private static readonly FirearmConditionNotificationDispatcher
            Notification = new FirearmConditionNotificationDispatcher(
                new NativeTopNotificationSink());
        private static readonly FirearmConditionDegradationFeedback Feedback =
            new FirearmConditionDegradationFeedback(
                new NativeCombatLogSink(), Notification);

        internal static long Published { get { return Notification.Published; } }
        internal static long Faults { get { return Notification.Faults; } }
        internal static long Attempts { get { return Notification.Attempts; } }
        internal static string LastMessage { get { return Notification.LastMessage; } }

        internal static bool PublishAfterCommittedDegradation(
            string wielderDisplayName, string itemDisplayName,
            FirearmCondition before, FirearmCondition after, string reason)
        {
            return Feedback.PublishAfterCommit(wielderDisplayName,
                itemDisplayName, before, after, reason, RecordFailure);
        }

        private static void RecordFailure(Exception exception)
        {
            ModContext context;
            if (!ModContext.TryGet(out context)) return;
            context.Logger.Failure(
                "firearm",
                "condition-notification.failed",
                "The firearm condition committed, but its native top notification failed.",
                exception);
        }

        private sealed class NativeCombatLogSink :
            IFirearmConditionCombatLogSink
        {
            public void Publish(string itemDisplayName,
                FirearmCondition before, FirearmCondition after, string reason)
            {
                FirearmConditionCombatLog.Publish(itemDisplayName,
                    before, after, reason);
            }
        }

        private sealed class NativeTopNotificationSink :
            IFirearmConditionTopNotificationSink
        {
            public void Publish(string message)
            {
                UIUtility.SendWarning(message);
            }
        }
    }
}
