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

        /// <summary>
        /// GS-AUD-0042. ADR-0037 requires Vital Strike-style damage increases
        /// to be excluded from scatter attacks. Precision damage is suppressed
        /// through DisablePrecisionDamage on the live damage path; this
        /// property is NOT consumed by the pipeline and is read only by
        /// <c>scatter.volley-damage-exclusions</c>.
        ///
        /// It is deliberately left inert, because the route it would guard is
        /// unreachable in Kingmaker 2.1.7b. Measured on the qualified
        /// reference assembly (Assembly-CSharp.dll, SHA-256
        /// 3b6450ff...f5afb):
        ///
        /// - The multiplier is applied by
        ///   <c>AbilityCustomMeleeAttack/VitalStrike</c>, an
        ///   <c>IInitiatorRulebookHandler&lt;RuleCalculateWeaponStats&gt;</c>.
        /// - It is added to that component's <c>EventHandlers</c> collection
        ///   only for <c>AttackType.VitalStrike</c>, and <c>EventHandlers</c>
        ///   is an <c>IDisposable</c> that subscribes on open and
        ///   unsubscribes on dispose.
        /// - Inside that same scope the component constructs and triggers its
        ///   OWN melee <c>RuleAttackWithWeapon</c>, so the handler is live only
        ///   for that one attack.
        /// - Scatter builds its own <c>RuleAttackWithWeapon</c> per target in
        ///   <c>ScatterShotRuntime</c>, outside any such scope, so the
        ///   multiplier cannot reach it.
        ///
        /// This project's only Vital Strike usages are summoned-creature melee
        /// features (the Sprint 29 Storm Giant and the Glabrezu's native
        /// feature), not gunslinger abilities, so the audit's precondition -
        /// a gunslinger holding Vital Strike and using it with a scatter
        /// attack - has no supported route.
        ///
        /// ADR-0037 is NOT weakened to match the code: the stated rule stands
        /// and this property stays as its declared contract, ready if a
        /// reachable route ever appears. The bounded limit of the check above
        /// is that it covers the engine's own mechanism; it does not enumerate
        /// every foreign component that might attach a Vital Strike-style
        /// multiplier to a ranged attack. Enforcing the exclusion would need
        /// such a route demonstrated first, with actual damage evidence and an
        /// ordinary-attack control.
        /// </summary>
        internal bool AllowsVitalStrikeDamage { get { return false; } }
    }
}
