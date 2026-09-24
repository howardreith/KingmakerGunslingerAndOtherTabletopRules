using System;

namespace KingmakerGunslinger.Classes
{
    internal sealed class GunslingerInitiativeService
    {
        internal int CalculateBonus(int currentGrit)
        {
            return CalculateBonus(currentGrit, 0);
        }

        /// <summary>
        /// The deed's bonus plus the earned Ifrit favored-class steps (G11),
        /// which improve the deed only while the deed itself applies.
        /// </summary>
        internal int CalculateBonus(int currentGrit, int favoredClassSteps)
        {
            if (currentGrit < 0)
                throw new ArgumentOutOfRangeException("currentGrit");
            if (favoredClassSteps < 0)
                throw new ArgumentOutOfRangeException("favoredClassSteps");
            return currentGrit > 0 ? checked(2 + favoredClassSteps) : 0;
        }
    }
}
