using System;

namespace KingmakerGunslinger.Classes
{
    internal sealed class NimbleService
    {
        internal int CalculateBonus(int gunslingerLevel, NimbleArmor armor,
            bool retainsDexterityBonus)
        {
            if (gunslingerLevel < 0 || gunslingerLevel > 20)
                throw new ArgumentOutOfRangeException("gunslingerLevel");
            if (!Enum.IsDefined(typeof(NimbleArmor), armor) || armor == NimbleArmor.Unknown)
                throw new ArgumentOutOfRangeException("armor");
            if (!retainsDexterityBonus ||
                (armor != NimbleArmor.None && armor != NimbleArmor.Light) ||
                gunslingerLevel < 2)
                return 0;
            return Math.Min(5, 1 + ((gunslingerLevel - 2) / 4));
        }
    }
}
