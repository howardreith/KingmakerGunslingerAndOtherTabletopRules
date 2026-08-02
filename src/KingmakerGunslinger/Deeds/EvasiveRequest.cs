using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class EvasiveRequest
    {
        internal EvasiveRequest(int gunslingerLevel, int currentGrit,
            bool benefitsActive)
        {
            if (gunslingerLevel < 0 || gunslingerLevel > 20)
                throw new ArgumentOutOfRangeException("gunslingerLevel");
            if (currentGrit < 0) throw new ArgumentOutOfRangeException("currentGrit");
            GunslingerLevel = gunslingerLevel;
            CurrentGrit = currentGrit;
            BenefitsActive = benefitsActive;
        }
        internal int GunslingerLevel { get; private set; }
        internal int CurrentGrit { get; private set; }
        internal bool BenefitsActive { get; private set; }
    }
}
