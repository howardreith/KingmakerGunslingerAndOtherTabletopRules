using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class CheatDeathService
    {
        internal CheatDeathDecision Evaluate(CheatDeathRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (!request.OwnsTarget) return Reject(CheatDeathStatus.WrongTarget,
                request.FinalHitPoints);
            if (!request.FirstEvaluation) return Reject(CheatDeathStatus.Duplicate,
                request.FinalHitPoints);
            if (request.GunslingerLevel < 19) return Reject(
                CheatDeathStatus.LevelTooLow, request.FinalHitPoints);
            if (request.AppliedDamage == 0) return Reject(
                CheatDeathStatus.NotLethal, request.FinalHitPoints);
            if (request.FinalHitPoints > 0) return Reject(CheatDeathStatus.NotLethal,
                request.FinalHitPoints);
            if (request.CurrentGrit < 1) return Reject(
                CheatDeathStatus.InsufficientGrit, request.FinalHitPoints);
            return new CheatDeathDecision(CheatDeathStatus.Applied,
                request.CurrentGrit, 1);
        }

        private static CheatDeathDecision Reject(CheatDeathStatus status,
            int hitPoints)
        { return new CheatDeathDecision(status, 0, hitPoints); }
    }
}
