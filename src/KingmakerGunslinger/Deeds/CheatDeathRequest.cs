using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class CheatDeathRequest
    {
        internal CheatDeathRequest(int gunslingerLevel, int currentGrit,
            int finalHitPoints, int appliedDamage, bool ownsTarget,
            bool firstEvaluation)
        {
            if (gunslingerLevel < 0) throw new ArgumentOutOfRangeException(
                "gunslingerLevel");
            if (currentGrit < 0) throw new ArgumentOutOfRangeException(
                "currentGrit");
            if (appliedDamage < 0) throw new ArgumentOutOfRangeException(
                "appliedDamage");
            GunslingerLevel = gunslingerLevel;
            CurrentGrit = currentGrit;
            FinalHitPoints = finalHitPoints;
            AppliedDamage = appliedDamage;
            OwnsTarget = ownsTarget;
            FirstEvaluation = firstEvaluation;
        }

        internal int GunslingerLevel { get; private set; }
        internal int CurrentGrit { get; private set; }
        internal int FinalHitPoints { get; private set; }
        internal int AppliedDamage { get; private set; }
        internal bool OwnsTarget { get; private set; }
        internal bool FirstEvaluation { get; private set; }
    }
}
