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
            if (float.IsNaN(request.DistanceFeet) ||
                float.IsInfinity(request.DistanceFeet) || request.DistanceFeet < 0f)
                throw new ArgumentOutOfRangeException("DistanceFeet");
            int range = CloseRangeFeet(request.CasterLevel);
            if (!request.SubjectPresent) return Invalid("subject-missing", range);
            if (!request.CasterPresent) return Invalid("caster-missing", range);
            if (!request.CasterAlive) return Invalid("caster-dead", range);
            if (!request.SameArea) return Invalid("different-area", range);
            if (request.DistanceFeet > range) return Invalid("out-of-range", range);
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
