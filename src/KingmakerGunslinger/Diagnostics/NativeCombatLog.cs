using System;
using System.Threading;
using Kingmaker;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.UI.Log;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Diagnostics
{
    internal static class NativeCombatLog
    {
        private sealed class BattleLogSink : IPlayerCombatLogSink
        {
            internal static readonly BattleLogSink Instance =
                new BattleLogSink();

            public void Add(string message)
            {
                if (Game.Instance == null || Game.Instance.UI == null ||
                    Game.Instance.UI.BattleLogManager == null ||
                    Game.Instance.UI.BattleLogManager.LogView == null ||
                    GameLogStrings.Instance == null)
                    throw new InvalidOperationException(
                        "The native combat-log view is unavailable.");
                Game.Instance.UI.BattleLogManager.LogView.AddLogEntry(
                    message, GameLogStrings.Instance.DefaultColor,
                    LogChannel.Combat, null, PrefixIcon.None);
            }
        }

        private static readonly PlayerCombatLogPublicationService Service =
            new PlayerCombatLogPublicationService(BattleLogSink.Instance);
        private static long _published;
        private static long _faults;
        private static string _lastMessage;

        internal static long Published
        { get { return Interlocked.Read(ref _published); } }
        internal static long Faults
        { get { return Interlocked.Read(ref _faults); } }
        internal static long Attempts { get { return Published + Faults; } }
        internal static string LastMessage { get { return _lastMessage; } }

        internal static bool Publish(string subsystem, string faultCode,
            string message, string committedResult)
        {
            // Retain the exact player-facing message even when a save-free
            // runtime fixture has no initialized BattleLogView. Publication is
            // presentation-only and must never falsify the committed mechanic.
            _lastMessage = message;
            bool published = Service.Publish(message, exception =>
            {
                Interlocked.Increment(ref _faults);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure(subsystem, faultCode,
                        committedResult, exception);
            });
            if (!published) return false;
            Interlocked.Increment(ref _published);
            return true;
        }
    }
}
