using System;

namespace KingmakerGunslinger.Presentation
{
    internal static class PlayerFacingTextPolicy
    {
        private static readonly string[] ForbiddenPhrases =
        {
            "implementation",
            "internal marker",
            "native-style",
            "stable weapon",
            "single stable",
            "exact weapon",
            "genuine sneak",
            "damage-stat replacement",
            "Kingmaker has no",
            "native bane",
            "native weapon-size",
            "KMG_",
            "<null>",
            "â€",
            "�"
        };

        internal static string FindIssue(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            foreach (string phrase in ForbiddenPhrases)
                if (text.IndexOf(phrase,
                        StringComparison.OrdinalIgnoreCase) >= 0)
                    return phrase;
            return null;
        }

        internal static bool IsClean(string text)
        {
            return FindIssue(text) == null;
        }

        internal static bool IsScreenSafe(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length > 96 ||
                text.IndexOf('\r') >= 0 || text.IndexOf('\n') >= 0 ||
                text.IndexOf(';') >= 0 || text.IndexOf("Exception",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("System.", StringComparison.OrdinalIgnoreCase)
                    >= 0 || !IsClean(text)) return false;
            int hexadecimalRun = 0;
            foreach (char value in text)
            {
                if (value >= '0' && value <= '9' ||
                    value >= 'a' && value <= 'f' ||
                    value >= 'A' && value <= 'F') hexadecimalRun++;
                else hexadecimalRun = 0;
                if (hexadecimalRun >= 24) return false;
            }
            return true;
        }
    }
}
