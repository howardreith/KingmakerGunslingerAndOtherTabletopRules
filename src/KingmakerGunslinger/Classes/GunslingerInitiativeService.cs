using System;

namespace KingmakerGunslinger.Classes
{
    internal sealed class GunslingerInitiativeService
    {
        internal int CalculateBonus(int currentGrit)
        {
            if (currentGrit < 0)
                throw new ArgumentOutOfRangeException("currentGrit");
            return currentGrit > 0 ? 2 : 0;
        }
    }
}
