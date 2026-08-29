using System;
using System.Globalization;
using System.Threading;

namespace KingmakerGunslinger.Firearms
{
    internal interface IFirearmConditionCombatLogSink
    {
        void Publish(string itemDisplayName, FirearmCondition before,
            FirearmCondition after, string reason);
    }

    internal interface IFirearmConditionTopNotificationSink
    {
        void Publish(string message);
    }

    /// <summary>
    /// Dependency-free formatting and failure isolation for concise firearm
    /// degradation notifications. Recovery and unchanged states are deliberately
    /// outside this dispatcher.
    /// </summary>
    internal sealed class FirearmConditionNotificationDispatcher
    {
        private readonly IFirearmConditionTopNotificationSink _sink;
        private long _published;
        private long _faults;
        private string _lastMessage;

        internal FirearmConditionNotificationDispatcher(
            IFirearmConditionTopNotificationSink sink)
        {
            _sink = sink ?? throw new ArgumentNullException("sink");
        }

        internal long Published { get { return Interlocked.Read(ref _published); } }
        internal long Faults { get { return Interlocked.Read(ref _faults); } }
        internal long Attempts { get { return Published + Faults; } }
        internal string LastMessage { get { return _lastMessage; } }

        internal bool Publish(string wielderDisplayName, string itemDisplayName,
            FirearmCondition before, FirearmCondition after,
            Action<Exception> failure)
        {
            string message;
            if (!TryFormat(wielderDisplayName, itemDisplayName, before, after,
                    out message))
                return false;

            _lastMessage = message;
            try
            {
                _sink.Publish(message);
                Interlocked.Increment(ref _published);
                return true;
            }
            catch (Exception exception)
            {
                Interlocked.Increment(ref _faults);
                if (failure != null)
                {
                    try { failure(exception); }
                    catch { }
                }
                return false;
            }
        }

        internal static bool IsDegradation(FirearmCondition before,
            FirearmCondition after)
        {
            return before == FirearmCondition.Normal &&
                    after == FirearmCondition.Broken ||
                before == FirearmCondition.Broken &&
                    after == FirearmCondition.Wrecked;
        }

        internal static bool TryFormat(string wielderDisplayName,
            string itemDisplayName, FirearmCondition before,
            FirearmCondition after, out string message)
        {
            message = null;
            if (!IsDegradation(before, after)) return false;

            string item = Normalize(itemDisplayName, "Firearm");
            string condition = after == FirearmCondition.Broken
                ? "broken"
                : "wrecked";
            string wielder = Normalize(wielderDisplayName, null);
            message = string.IsNullOrEmpty(wielder)
                ? string.Format(CultureInfo.InvariantCulture,
                    "{0} is now {1}.", item, condition)
                : string.Format(CultureInfo.InvariantCulture,
                    "{0}'s {1} is now {2}.", wielder, item, condition);
            return true;
        }

        private static string Normalize(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }

    /// <summary>
    /// Enforces player-feedback order at the post-commit boundary: existing
    /// native combat log first, then the additional transient top notification.
    /// </summary>
    internal sealed class FirearmConditionDegradationFeedback
    {
        private readonly IFirearmConditionCombatLogSink _combatLog;
        private readonly FirearmConditionNotificationDispatcher _notification;

        internal FirearmConditionDegradationFeedback(
            IFirearmConditionCombatLogSink combatLog,
            FirearmConditionNotificationDispatcher notification)
        {
            _combatLog = combatLog ?? throw new ArgumentNullException("combatLog");
            _notification = notification ??
                throw new ArgumentNullException("notification");
        }

        internal bool PublishAfterCommit(string wielderDisplayName,
            string itemDisplayName, FirearmCondition before,
            FirearmCondition after, string reason, Action<Exception> failure)
        {
            if (!FirearmConditionNotificationDispatcher.IsDegradation(
                    before, after))
                return false;

            _combatLog.Publish(itemDisplayName, before, after, reason);
            return _notification.Publish(wielderDisplayName, itemDisplayName,
                before, after, failure);
        }
    }
}
