using System;

namespace KingmakerGunslinger.Presentation
{
    internal enum PlayerFacingTextState
    {
        /// <summary>A stored string whose key resolves to real text.</summary>
        Assigned = 0,

        /// <summary>No string was ever stored; anything displayed is the engine's or the environment's stand-in.</summary>
        Unassigned = 1,

        /// <summary>A string is stored but resolves to nothing visible.</summary>
        Blank = 2,

        /// <summary>A string is stored but the active localization pack has no text for its key.</summary>
        UnknownKey = 3
    }

    /// <summary>
    /// Decides whether displayed unit-fact text was genuinely assigned. Kingmaker 2.1.7b resolves a string that was
    /// never stored to the non-empty placeholder "&lt;null&gt;", and a key missing from the active pack to
    /// "[unknown key: ...]". A plain non-empty test accepts both, and another mod or a localization pack may resolve
    /// either one differently, so the decision uses the stored field as well as the resolved text.
    /// </summary>
    internal static class PlayerFacingTextAssignment
    {
        internal const string EngineUnassignedPlaceholder = "<null>";
        internal const string EngineUnknownKeyPrefix = "[unknown key: ";

        /// <param name="fieldStored">The fact stores a localized string with a key (or a shared string).</param>
        /// <param name="resolved">The text the fact's own getter returns in this process.</param>
        /// <param name="unassignedResolution">What this process returns for a string that was never stored.</param>
        internal static PlayerFacingTextState Classify(bool fieldStored,
            string resolved, string unassignedResolution)
        {
            bool blank = string.IsNullOrWhiteSpace(resolved);
            if (!fieldStored && (blank ||
                string.Equals(resolved, EngineUnassignedPlaceholder,
                    StringComparison.Ordinal) ||
                string.Equals(resolved, unassignedResolution,
                    StringComparison.Ordinal)))
                return PlayerFacingTextState.Unassigned;
            if (blank) return PlayerFacingTextState.Blank;
            if (resolved.StartsWith(EngineUnknownKeyPrefix, StringComparison.Ordinal))
                return PlayerFacingTextState.UnknownKey;
            // A subtype that supplies its own text without the stored field still
            // shows the player real text, so it is not reported as missing.
            return PlayerFacingTextState.Assigned;
        }

        internal static string Describe(PlayerFacingTextState state)
        {
            switch (state)
            {
                case PlayerFacingTextState.Assigned: return "assigned";
                case PlayerFacingTextState.Unassigned: return "unassigned";
                case PlayerFacingTextState.Blank: return "blank";
                case PlayerFacingTextState.UnknownKey: return "unknown-key";
                default: throw new ArgumentOutOfRangeException("state");
            }
        }
    }
}
