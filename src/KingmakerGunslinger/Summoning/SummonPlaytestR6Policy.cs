using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>One sampled frame of a native command, for the R6 observer's predicates.</summary>
    internal sealed class R6CommandFrame
    {
        internal string Action;
        internal string Clip;
        internal bool Started;
        internal bool Acted;
        internal int GameFrame;
    }

    /// <summary>
    /// Repair R6 (PRES-01..07) decisions that do not need the game: the native
    /// sound events used for the two cast cues, which cast style survives the
    /// engine's target-dependent rewriting, Redcap ownership, and the
    /// observer's acceptance predicates.
    /// </summary>
    internal static class SummonPlaytestR6Policy
    {
        /// <summary>Both cue events live in the native VFX sound bank.</summary>
        internal const string CueBank = "VFX";
        internal const string VrockScreechCue = "EarPiercingScream";

        /// <summary>
        /// A provisional native one-shot. The installed event and source-file
        /// names were searched for horn, trumpet, bugle, fanfare and brass and
        /// the only match is unrelated boss music; fit is the owner's decision.
        /// </summary>
        internal const string TrumpetBlastCue = "InspireCourage";

        internal const string RedcapSummonedUnitGuid = "af8bcff074b93c84b97938e486184222";
        internal const string RedcapScytheClipPrefix = "Attack_with_scythe";
        internal const string KmgAbilityPrefix = "KMG_Summoning_";

        // UnitUseAbility rewrites Touch and SelfTouch by target, and this rig
        // has no SelfTouch clip. These styles are not rewritten by target.
        private static readonly string[] TargetStableStyles = { "Omni", "Self", "Directional", "Point" };

        /// <summary>
        /// A playable style the engine will not rewrite for a self or ally
        /// target; null when the rig can play none of them.
        /// </summary>
        internal static string ChooseCastStyle(string current, IEnumerable<string> playableStyles)
        {
            string[] playable = (playableStyles ?? Enumerable.Empty<string>()).Where(value => value != null).ToArray();
            if (current != null && TargetStableStyles.Contains(current, StringComparer.Ordinal) &&
                playable.Contains(current, StringComparer.Ordinal)) return current;
            return TargetStableStyles.FirstOrDefault(value => playable.Contains(value, StringComparer.Ordinal));
        }

        /// <summary>
        /// A native RedcapSummoned is bound only when its own summon context
        /// names a KMG ability that really spawns this unit. The unit identity
        /// alone also matches the base game's own summons.
        /// </summary>
        internal static bool RedcapBindingAllowed(string unitGuid, string sourceAbilityName, bool abilitySpawnsThisUnit)
        {
            return unitGuid == RedcapSummonedUnitGuid && abilitySpawnsThisUnit && sourceAbilityName != null &&
                sourceAbilityName.StartsWith(KmgAbilityPrefix, StringComparison.Ordinal);
        }

        /// <summary>
        /// A real cast gesture: one of the rig's clips for the resolved style
        /// plays on its cast action while the command has started and not yet
        /// acted, the command was not cancelled, and it applied exactly once.
        /// </summary>
        internal static bool CastGestureObserved(IEnumerable<R6CommandFrame> frames, string castAction,
            IEnumerable<string> expectedClips, int applied, bool cancelled)
        {
            string[] expected = (expectedClips ?? Enumerable.Empty<string>()).Where(value => value != null).ToArray();
            return !cancelled && applied == 1 && expected.Length > 0 && frames != null &&
                frames.Any(frame => frame.Started && !frame.Acted && frame.Action == castAction &&
                    expected.Contains(frame.Clip, StringComparer.Ordinal));
        }

        /// <summary>
        /// The scythe clip is playing at the game frame the real scythe attack
        /// rolled, within a small sampling tolerance, on a command that acted.
        /// </summary>
        internal static bool ScytheSwingObserved(IEnumerable<R6CommandFrame> frames, IEnumerable<int> scytheRollFrames, int tolerance)
        {
            R6CommandFrame[] swings = (frames ?? Enumerable.Empty<R6CommandFrame>()).Where(frame => frame.Clip != null &&
                frame.Clip.StartsWith(RedcapScytheClipPrefix, StringComparison.Ordinal)).ToArray();
            int[] rolls = (scytheRollFrames ?? Enumerable.Empty<int>()).ToArray();
            return rolls.Length > 0 && rolls.All(roll => swings.Any(frame => Math.Abs(frame.GameFrame - roll) <= tolerance));
        }
    }
}
