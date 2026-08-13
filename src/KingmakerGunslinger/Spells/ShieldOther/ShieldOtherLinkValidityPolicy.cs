using System;

namespace KingmakerGunslinger.Spells.ShieldOther
{
    internal static class ShieldOtherLinkValidityPolicy
    {
        internal static ShieldOtherLinkValidityDecision Evaluate(
            ShieldOtherLinkValidityRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (request.CasterLevel < 0)
                throw new ArgumentOutOfRangeException("CasterLevel");
            int range = CloseRangeFeet(request.CasterLevel);
            if (!request.SubjectPresent) return Invalid("subject-missing", range);
            if (!request.CasterPresent) return Invalid("caster-missing", range);
            if (request.CasterLevel == 0) return Invalid("caster-level-missing", range);
            if (!request.CasterAlive) return Invalid("caster-dead", range);
            if (!request.SameArea) return Invalid("different-area", range);
            return new ShieldOtherLinkValidityDecision(true, "valid", range);
        }

        internal static int CloseRangeFeet(int casterLevel)
        {
            if (casterLevel < 0) throw new ArgumentOutOfRangeException("casterLevel");
            return checked(25 + (casterLevel / 2) * 5);
        }

        private static ShieldOtherLinkValidityDecision Invalid(string status, int range)
        { return new ShieldOtherLinkValidityDecision(false, status, range); }
    }
}
