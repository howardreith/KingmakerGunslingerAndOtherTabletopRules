using System;

namespace KingmakerGunslinger.Scatter
{
    /// <summary>Aggregate decision for one independently rolled scatter volley.</summary>
    internal sealed class ScatterAttackVolleyDecision
    {
        internal const int AttackPenalty = -2;

        internal ScatterAttackVolleyDecision(
            int targetCount,
            int hitCount,
            int misfireRollCount,
            int criticalThreatCount,
            int confirmedCriticalCount)
        {
            if (targetCount < 0) throw new ArgumentOutOfRangeException("targetCount");
            if (hitCount < 0 || hitCount > targetCount) throw new ArgumentOutOfRangeException("hitCount");
            if (misfireRollCount < 0 || misfireRollCount > targetCount)
                throw new ArgumentOutOfRangeException("misfireRollCount");
            if (criticalThreatCount < 0 || criticalThreatCount > hitCount)
                throw new ArgumentOutOfRangeException("criticalThreatCount");
            if (confirmedCriticalCount < 0 || confirmedCriticalCount > criticalThreatCount)
                throw new ArgumentOutOfRangeException("confirmedCriticalCount");

            TargetCount = targetCount;
            HitCount = hitCount;
            MisfireRollCount = misfireRollCount;
            CriticalThreatCount = criticalThreatCount;
            ConfirmedCriticalCount = confirmedCriticalCount;
        }

        internal int TargetCount { get; private set; }
        internal int HitCount { get; private set; }
        internal int MisfireRollCount { get; private set; }
        internal int CriticalThreatCount { get; private set; }
        internal int ConfirmedCriticalCount { get; private set; }
        internal bool AllRollsMisfire
        {
            get { return TargetCount > 0 && MisfireRollCount == TargetCount; }
        }
        internal bool AllowsPrecisionDamage { get { return false; } }
        internal bool AllowsVitalStrikeDamage { get { return false; } }
    }
}
