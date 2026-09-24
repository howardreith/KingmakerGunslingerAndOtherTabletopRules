using System;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// Pure arithmetic of Gunslinger favored-class mechanics that combine
    /// with an existing rule rather than stand alone.
    /// </summary>
    internal static class FavoredClassMechanicsPolicy
    {
        /// <summary>
        /// I08/S06: the change of a DC bound to half the class level when the
        /// effective level rises by the earned steps (floor((L+e)/2) - floor(L/2)).
        /// </summary>
        internal static int HalfLevelDelta(int level, int earned)
        {
            if (earned <= 0)
                return 0;
            int baseLevel = Math.Max(0, level);
            return (baseLevel + earned) / 2 - baseLevel / 2;
        }

        /// <summary>
        /// G02/G08/G16: the earned firearm confirmation bonus does not stack
        /// with Critical Focus, and the better contribution is preserved.
        /// Critical Focus applies its own bonus natively, so this adds only
        /// the excess of the earned bonus over it (never a negative amount).
        /// </summary>
        internal static int ConfirmationContribution(int earnedSteps, int criticalFocusBonus)
        {
            if (earnedSteps < 0)
                throw new ArgumentOutOfRangeException("earnedSteps");
            return Math.Max(0, earnedSteps - Math.Max(0, criticalFocusBonus));
        }
    }
}
