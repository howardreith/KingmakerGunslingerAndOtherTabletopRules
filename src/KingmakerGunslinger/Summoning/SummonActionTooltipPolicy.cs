using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Repair R3 UI-05/UI-06: tooltip rows for the owned trample and overrun
    /// actions. Both are cloned from the native overrun ability, whose
    /// Duration/Saving Throw strings ("Trample" and the maneuver description)
    /// were inherited verbatim. The native tooltip omits a row whose text is
    /// empty and prints "&lt;null&gt;" for a missing string, so inapplicable rows
    /// are the empty string. Action cost is not a duration, and an overrun's
    /// combat maneuver check is not a saving throw.
    /// </summary>
    internal static class SummonActionTooltipPolicy
    {
        /// <summary>Both actions are instantaneous movements: no Duration row.</summary>
        internal const string InstantaneousDuration = "";

        /// <summary>Overrun resolves by a combat maneuver check: no save row.</summary>
        internal const string OverrunSavingThrow = "";

        /// <summary>
        /// The implemented trample contract: a target that can make a legal
        /// attack of opportunity makes it instead of saving; otherwise Reflex
        /// halves. Stampede raises the DC by 2 only while it is active.
        /// </summary>
        internal static string TrampleSavingThrow(int dc, bool stampede)
        {
            if (dc <= 0) throw new ArgumentOutOfRangeException("dc");
            return "Reflex DC " + dc + " half" +
                (stampede ? " (DC " + (dc + 2) + " during Stampede)" : "") +
                "; none for a target that makes its attack of opportunity instead";
        }

        /// <summary>
        /// VIS-03: the exact owned movement actions (nine Tramples and the
        /// Brachiosaurus Overrun). They target a point with
        /// <c>AbilityRange.DoubleMove</c>, which has no native range string, so
        /// the native target row fell back to its custom-range wording around
        /// <c>GetRange()</c>, which is 0 ft for DoubleMove ("within 0 ft.
        /// feet"). With no spell component their school is None, which the
        /// native fact header prints as "None".
        /// </summary>
        internal static readonly ReadOnlyCollection<string> MovementTargetAbilityGuids =
            new ReadOnlyCollection<string>(new[]
            {
                "e58bb2d887ad435eaaa220c1c4528211", // Aurochs Trample
                "48d90c0f49c54e84a463b006e507e322", // Bison Trample
                "0f12c70e9b264ae484fb720d0c6845aa", // Woolly Rhinoceros Trample
                "e4421da48c564c4f8553c4538a5ae0c4", // Giant Stag Beetle Trample
                "20c83d35a8234c9fb35e8f174d19e6ba", // Triceratops Trample
                "309ec3dedcf04c15b3011933f5eafef4", // Brachiosaurus Trample
                "a7c34e1d9b5f4a02ae6d8c71f3b02594", // Elephant Trample
                "a1f52aace5124004b76e2351c7023566", // Mastodon Trample
                "c55343b590f54f0ea18fd258c92dfcd5", // Gorgon Trample
                "b41f7d2a6c8e40539af1c2d5e7b30486", // Brachiosaurus Overrun
            });

        private static readonly HashSet<string> MovementTargets =
            new HashSet<string>(MovementTargetAbilityGuids, StringComparer.Ordinal);

        /// <summary>
        /// The native DoubleMove legality distance is
        /// <c>CombatSpeedMps * 6</c>, and <c>Speed.Mps</c> is the speed in
        /// metres divided by three: twice the creature's current speed (plus
        /// the native body radii). The wording states that without a number,
        /// because the target row is built from the blueprint, not the unit.
        /// </summary>
        internal const string MovementTargetRange = "within twice this creature's speed";

        /// <summary>
        /// Only an exact owned movement action that still targets a point at
        /// DoubleMove range is presented this way; anything else, including
        /// the native overrun it was cloned from, keeps its native text.
        /// </summary>
        internal static bool PresentsMovementTarget(string assetGuid, bool doubleMoveRange,
            bool canTargetPoint)
        {
            return assetGuid != null && doubleMoveRange && canTargetPoint &&
                MovementTargets.Contains(assetGuid);
        }

        /// <summary>
        /// The native target row with its own localized "Target point" prefix
        /// and line ending, and the movement range instead of "0 ft. feet".
        /// </summary>
        internal static string MovementTarget(string targetPoint, string range)
        {
            if (string.IsNullOrEmpty(targetPoint)) throw new ArgumentException("targetPoint");
            if (string.IsNullOrEmpty(range)) throw new ArgumentException("range");
            return targetPoint + " " + range + ".\n";
        }
    }
}
