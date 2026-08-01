using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class QuickClearService
    {
        internal QuickClearDecision Evaluate(QuickClearRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (!request.ExactEquippedFirearm)
                return Reject(QuickClearStatus.NotExactEquippedFirearm, request.Mode);
            if (request.Condition != Firearms.FirearmCondition.Broken)
                return Reject(QuickClearStatus.NotBroken, request.Mode);
            if (!request.BrokenByMisfire)
                return Reject(QuickClearStatus.NotMisfireBroken, request.Mode);
            int cost = request.Mode == QuickClearMode.Move ? 1 : 0;
            // The standard action spends nothing but still requires one current grit.
            if (request.CurrentGrit < 1)
                return Reject(QuickClearStatus.InsufficientGrit, request.Mode);
            return new QuickClearDecision(QuickClearStatus.Eligible, request.Mode, cost);
        }

        private static QuickClearDecision Reject(QuickClearStatus status,
            QuickClearMode mode)
        {
            return new QuickClearDecision(status, mode, 0);
        }
    }
}
