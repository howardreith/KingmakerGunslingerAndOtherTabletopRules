using System;

namespace KingmakerGunslinger.Spells.ProtectionFromAlignment
{
    internal enum ProtectionDescriptionPublicationDecision
    {
        Publish = 0,
        AlreadyPublished = 1
    }

    internal static class ProtectionDescriptionPublicationPolicy
    {
        private const string OwnedPrefix = "KMG.ProtectionFromAlignment.";

        internal static ProtectionDescriptionPublicationDecision Decide(
            string currentKey, string expectedKey)
        {
            if (string.IsNullOrWhiteSpace(expectedKey) ||
                !expectedKey.StartsWith(OwnedPrefix, StringComparison.Ordinal))
                throw new ArgumentException(
                    "An owned protection-description key is required.",
                    "expectedKey");
            if (string.Equals(currentKey, expectedKey,
                StringComparison.Ordinal))
                return ProtectionDescriptionPublicationDecision.AlreadyPublished;
            if (!string.IsNullOrWhiteSpace(currentKey) &&
                currentKey.StartsWith(OwnedPrefix, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "A protection description already uses an unexpected owned key: " +
                    currentKey + ".");
            return ProtectionDescriptionPublicationDecision.Publish;
        }

        internal static bool IsOwnedKey(string key)
        {
            return !string.IsNullOrWhiteSpace(key) &&
                key.StartsWith(OwnedPrefix, StringComparison.Ordinal);
        }
    }
}
