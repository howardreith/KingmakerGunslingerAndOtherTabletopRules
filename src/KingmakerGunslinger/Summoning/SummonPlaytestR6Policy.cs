using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Repair R6 (PRES-01..07) decisions that do not need the game: the native
    /// sound events used for the two cast cues, and which native cast style a
    /// rig can actually play.
    /// </summary>
    internal static class SummonPlaytestR6Policy
    {
        /// <summary>Both cue events live in the always-used native VFX sound bank.</summary>
        internal const string CueBank = "VFX";
        internal const string VrockScreechCue = "EarPiercingScream";

        /// <summary>
        /// Kingmaker ships no horn or trumpet effect. This is the closest
        /// native one-shot; its fit is the owner's listening decision.
        /// </summary>
        internal const string TrumpetBlastCue = "InspireCourage";

        internal const string RedcapSummonedUnitGuid = "af8bcff074b93c84b97938e486184222";

        private static readonly string[] StylePreference =
            { "Omni", "Directional", "Self", "Touch", "SelfTouch", "Point" };

        /// <summary>
        /// The ability's own cast style when the rig has a playable clip for
        /// it; otherwise the first playable style by preference; null when the
        /// rig can play none. Never Immediate, which skips the animation.
        /// </summary>
        internal static string ChooseCastStyle(string current, IEnumerable<string> playableStyles)
        {
            string[] playable = (playableStyles ?? Enumerable.Empty<string>())
                .Where(value => value != null && value != "Immediate").ToArray();
            if (current != null && playable.Contains(current, StringComparer.Ordinal)) return current;
            return StylePreference.FirstOrDefault(value => playable.Contains(value, StringComparer.Ordinal));
        }
    }
}
