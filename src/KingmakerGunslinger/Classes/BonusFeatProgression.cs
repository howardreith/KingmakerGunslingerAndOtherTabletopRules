using System;

namespace KingmakerGunslinger.Classes
{
    internal static class BonusFeatProgression
    {
        private static readonly int[] ExactLevels = { 4, 8, 12, 16, 20 };

        internal static int[] Levels
        {
            get { return (int[])ExactLevels.Clone(); }
        }

        internal static bool GrantsAt(int gunslingerLevel)
        {
            if (gunslingerLevel < 0 || gunslingerLevel > 20)
                throw new ArgumentOutOfRangeException("gunslingerLevel");
            return gunslingerLevel >= 4 && gunslingerLevel % 4 == 0;
        }
    }
}
