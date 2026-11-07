using System;

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
    }
}
