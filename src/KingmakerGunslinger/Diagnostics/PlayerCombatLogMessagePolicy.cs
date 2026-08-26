using System;
using System.Text.RegularExpressions;

namespace KingmakerGunslinger.Diagnostics
{
    internal interface IPlayerCombatLogSink
    {
        void Add(string message);
    }

    internal sealed class PlayerCombatLogPublicationService
    {
        private readonly IPlayerCombatLogSink _sink;

        internal PlayerCombatLogPublicationService(IPlayerCombatLogSink sink)
        {
            _sink = sink ?? throw new ArgumentNullException("sink");
        }

        internal bool Publish(string message, Action<Exception> onFailure)
        {
            try
            {
                _sink.Add(PlayerCombatLogMessagePolicy.RequireValid(message));
                return true;
            }
            catch (Exception exception)
            {
                if (onFailure != null) onFailure(exception);
                return false;
            }
        }
    }

    internal static class PlayerCombatLogMessagePolicy
    {
        internal const int PreferredMaximumLength = 100;
        internal const int HardMaximumLength = 160;

        private static readonly Regex InternalGuid = new Regex(
            "(?i)(?:[0-9a-f]{32}|[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})",
            RegexOptions.CultureInvariant);

        private static readonly string[] InternalWords =
        {
            "diagnostic", "trace", "runtime", "constructor", "blueprint",
            "unity mod manager", "umm:"
        };

        internal static string RequireValid(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException(
                    "A player combat-log message is required.", "message");
            string value = message.Trim();
            if (!string.Equals(value, message, StringComparison.Ordinal))
                throw new ArgumentException(
                    "A player combat-log message must already be trimmed.",
                    "message");
            if (value.Length > HardMaximumLength)
                throw new ArgumentException(
                    "A player combat-log message is too long.", "message");
            if (InternalGuid.IsMatch(value))
                throw new ArgumentException(
                    "A player combat-log message contains an internal identity.",
                    "message");
            foreach (string word in InternalWords)
                if (value.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                    throw new ArgumentException(
                        "A player combat-log message contains internal terminology.",
                        "message");
            if (value.IndexOf(';') >= 0 && value.IndexOf('=') >= 0)
                throw new ArgumentException(
                    "A player combat-log message contains structured diagnostics.",
                    "message");
            return value;
        }

        internal static bool IsPreferredLength(string message)
        {
            return RequireValid(message).Length <= PreferredMaximumLength;
        }
    }
}
