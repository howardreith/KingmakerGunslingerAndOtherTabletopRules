using System;

namespace KingmakerGunslinger.Scatter
{
    /// <summary>Immutable observation of one native attack roll for one target.</summary>
    internal sealed class ScatterAttackRollObservation
    {
        internal ScatterAttackRollObservation(
            object target,
            string stableIdentity,
            int naturalRoll,
            bool isHit,
            bool isCriticalThreat,
            bool isCriticalConfirmed)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (target.GetType().IsValueType)
                throw new ArgumentException("A scatter roll target must have reference identity.", "target");
            if (string.IsNullOrWhiteSpace(stableIdentity))
                throw new ArgumentException("A stable target identity is required.", "stableIdentity");
            if (naturalRoll < 1 || naturalRoll > 20)
                throw new ArgumentOutOfRangeException("naturalRoll");
            if (isCriticalThreat && !isHit)
                throw new ArgumentException("A critical threat must also be a hit.", "isCriticalThreat");
            if (isCriticalConfirmed && !isCriticalThreat)
                throw new ArgumentException("A confirmed critical requires a threat.", "isCriticalConfirmed");

            Target = target;
            StableIdentity = stableIdentity.Trim();
            NaturalRoll = naturalRoll;
            IsHit = isHit;
            IsCriticalThreat = isCriticalThreat;
            IsCriticalConfirmed = isCriticalConfirmed;
        }

        internal object Target { get; private set; }
        internal string StableIdentity { get; private set; }
        internal int NaturalRoll { get; private set; }
        internal bool IsHit { get; private set; }
        internal bool IsCriticalThreat { get; private set; }
        internal bool IsCriticalConfirmed { get; private set; }

        internal bool IsMisfire(int misfireThreshold)
        {
            if (misfireThreshold < 1 || misfireThreshold > 20)
                throw new ArgumentOutOfRangeException("misfireThreshold");
            return NaturalRoll <= misfireThreshold;
        }
    }
}
