using System;

namespace KingmakerGunslinger.Cord
{
    internal static class CordSubstitutionPolicy
    {
        /// <summary>
        /// The one Cord substitution rule, and the one the shipped mod runs:
        /// CordConditionPatches.ResolveCanonical calls this to decide the
        /// damage and whether exhaustion downgrades to fatigue.
        ///
        /// GS-AUD-0018: this class previously had no production caller at all.
        /// The live patch re-implemented the identical capping rule inline,
        /// so seven passing tests measured logic the mod never executed and
        /// any drift between the two copies was invisible. Production now
        /// executes this policy, so those tests are real coverage.
        ///
        /// The former nativeNonlethal parameter was also removed. Kingmaker
        /// 2.1.7b has no usable native nonlethal damage path, so that branch
        /// was unreachable - a smaller instance of the same problem, with its
        /// own passing test. The owner-approved adaptation is untyped
        /// self-damage capped so the Cord can never reduce its wearer below
        /// 1 HP, which is what this computes.
        /// </summary>
        internal static CordSubstitutionDecision Decide(bool exactCordEquipped,
            CordConditionKind condition, int d6Roll, int currentHitPoints)
        {
            if (!Enum.IsDefined(typeof(CordConditionKind), condition))
                throw new ArgumentOutOfRangeException("condition");
            if (!exactCordEquipped)
                return new CordSubstitutionDecision(false, 0, false, "not-equipped");
            if (d6Roll < 1 || d6Roll > 6)
                throw new ArgumentOutOfRangeException("d6Roll");
            if (currentHitPoints < 0)
                throw new ArgumentOutOfRangeException("currentHitPoints");
            int damage = Math.Min(d6Roll, Math.Max(0, currentHitPoints - 1));
            return new CordSubstitutionDecision(true, damage,
                condition == CordConditionKind.Exhaustion, "capped-equivalent");
        }
    }
}
