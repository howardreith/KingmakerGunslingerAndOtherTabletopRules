using System;

namespace KingmakerGunslinger.Cord
{
    internal static class CordSubstitutionPolicy
    {
        internal static CordSubstitutionDecision Decide(bool exactCordEquipped,
            CordConditionKind condition, int d6Roll, int currentHitPoints,
            bool nativeNonlethal)
        {
            if (!Enum.IsDefined(typeof(CordConditionKind), condition))
                throw new ArgumentOutOfRangeException("condition");
            if (!exactCordEquipped)
                return new CordSubstitutionDecision(false, 0, false, "not-equipped");
            if (d6Roll < 1 || d6Roll > 6)
                throw new ArgumentOutOfRangeException("d6Roll");
            if (currentHitPoints < 0)
                throw new ArgumentOutOfRangeException("currentHitPoints");
            int damage = nativeNonlethal ? d6Roll :
                Math.Min(d6Roll, Math.Max(0, currentHitPoints - 1));
            return new CordSubstitutionDecision(true, damage,
                condition == CordConditionKind.Exhaustion,
                nativeNonlethal ? "native-nonlethal" : "capped-equivalent");
        }
    }
}
