using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Rules boundary for one Stirge's session-scoped attachment.
    /// The runtime still owns target identity, turn timing and cleanup.</summary>
    internal static class StirgeAttachPolicy
    {
        internal const int MaximumConstitutionDamage = 4;
        internal const int DamagePerAttachedTurn = 1;
        internal const int MaintainGrappleRacialBonus = 8;
        internal const int DiseaseChancePercent = 10;

        internal static bool MayAttach(bool touchHit, bool alreadyAttached,
            bool targetAlive)
        {
            return touchHit && !alreadyAttached && targetAlive;
        }

        internal static int RequestedDamage(bool attached, bool targetAlive,
            int cumulativeConstitutionDamage)
        {
            if (cumulativeConstitutionDamage < 0 ||
                cumulativeConstitutionDamage > MaximumConstitutionDamage)
                throw new ArgumentOutOfRangeException(
                    "cumulativeConstitutionDamage");
            return attached && targetAlive && cumulativeConstitutionDamage <
                MaximumConstitutionDamage ? DamagePerAttachedTurn : 0;
        }

        internal static StirgeDrainStep EndTurn(bool attached,
            bool targetAlive, int cumulativeConstitutionDamage,
            int actualConstitutionDamage)
        {
            int requested = RequestedDamage(attached, targetAlive,
                cumulativeConstitutionDamage);
            if (actualConstitutionDamage < 0 ||
                actualConstitutionDamage > requested)
                throw new ArgumentOutOfRangeException(
                    "actualConstitutionDamage");
            if (!attached) return new StirgeDrainStep(0,
                cumulativeConstitutionDamage, false);
            if (!targetAlive || cumulativeConstitutionDamage ==
                    MaximumConstitutionDamage)
                return new StirgeDrainStep(0,
                    cumulativeConstitutionDamage, true);
            int after = cumulativeConstitutionDamage +
                actualConstitutionDamage;
            return new StirgeDrainStep(actualConstitutionDamage, after,
                after >= MaximumConstitutionDamage);
        }
    }

    internal struct StirgeDrainStep
    {
        internal StirgeDrainStep(int damage, int cumulativeDamage,
            bool detach)
        {
            Damage = damage;
            CumulativeDamage = cumulativeDamage;
            Detach = detach;
        }

        internal int Damage { get; private set; }
        internal int CumulativeDamage { get; private set; }
        internal bool Detach { get; private set; }
    }
}
