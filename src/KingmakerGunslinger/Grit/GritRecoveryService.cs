using System;

namespace KingmakerGunslinger.Grit
{
    /// <summary>
    /// Pure eligibility policy shared by confirmed-critical and killing-blow
    /// runtime adapters. Each native event is evaluated separately so one shot
    /// that satisfies both clauses may restore once for each distinct clause.
    /// </summary>
    internal sealed class GritRecoveryService
    {
        internal GritRecoveryDecision Evaluate(GritRecoveryRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            GritRecoveryStatus status;
            if (!request.QualifyingOutcome)
                status = GritRecoveryStatus.NotQualifyingOutcome;
            else if (!request.IsExactFirearm)
                status = GritRecoveryStatus.NotExactFirearm;
            else if (!request.IsInCombat)
                status = GritRecoveryStatus.NotInCombat;
            else if (!request.IsCreature)
                status = GritRecoveryStatus.InvalidTarget;
            else if (request.IsHelplessOrUnaware)
                status = GritRecoveryStatus.HelplessOrUnawareTarget;
            else if (request.TargetHitDice * 2 < request.CharacterLevel)
                status = GritRecoveryStatus.InsignificantTarget;
            else
                status = GritRecoveryStatus.Eligible;

            return new GritRecoveryDecision(request.EventKind, status);
        }
    }
}
