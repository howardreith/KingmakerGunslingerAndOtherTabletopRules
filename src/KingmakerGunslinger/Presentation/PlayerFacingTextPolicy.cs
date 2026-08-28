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
    }
}
