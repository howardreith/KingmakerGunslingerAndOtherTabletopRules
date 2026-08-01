using System;

namespace KingmakerGunslinger.Classes
{
    internal static class GunTrainingProgression
    {
        private static readonly int[] ExactLevels = { 5, 9, 13, 17 };

        internal static int[] Levels
        {
            get { return (int[])ExactLevels.Clone(); }
        }

        internal static bool GrantsAt(int gunslingerLevel)
        {
            if (gunslingerLevel < 0 || gunslingerLevel > 20)
                throw new ArgumentOutOfRangeException("gunslingerLevel");
            return gunslingerLevel >= 5 && (gunslingerLevel - 5) % 4 == 0;
        }
    }
}
