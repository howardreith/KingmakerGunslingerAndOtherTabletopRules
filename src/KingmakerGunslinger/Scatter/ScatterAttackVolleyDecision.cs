using System;

namespace KingmakerGunslinger.Scatter
{
    /// <summary>Aggregate decision for one independently rolled scatter volley.</summary>
    internal sealed class ScatterAttackVolleyDecision
    {
        /// <summary>
        /// The scatter volley's attack penalty, expressed in the Kingmaker
        /// 2.1.7b native convention: <c>RuleAttackWithWeapon.AttackBonusPenalty</c>
        /// is a positive quantity that the engine SUBTRACTS. Measured on the
        /// qualified reference assembly (Assembly-CSharp.dll, SHA-256
        /// 3b6450ff...f5afb): the constructor stores the argument verbatim into
        /// an initonly field, forwards it verbatim into RuleAttackRoll, whose
        /// setter is a plain auto-property, and
        /// RuleCalculateAttackBonus::OnTrigger computes
        /// <c>BaseAttackBonus - AttackBonusPenalty</c> with no clamp anywhere on
        /// the path. So positive two realises the documented -2 attack penalty
        /// (ADR-0037:40-41); a negative value here would realise a +2 BONUS.
        /// Every other penalty site in this repository uses the same positive
        /// convention (for example EasternWeaponProficiencyRuntime's
        /// <c>AttackBonusPenalty + 4</c> for a -4 penalty).
        /// </summary>
        internal const int AttackPenalty = 2;

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
