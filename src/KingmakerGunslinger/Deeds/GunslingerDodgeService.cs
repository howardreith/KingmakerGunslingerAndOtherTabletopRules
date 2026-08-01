using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class GunslingerDodgeService
    {
        internal GunslingerDodgeDecision Evaluate(GunslingerDodgeRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            GunslingerDodgeStatus status;
            if (!request.IsArmed) status = GunslingerDodgeStatus.NotArmed;
            else if (!request.IsRangedAttack) status = GunslingerDodgeStatus.NotRangedAttack;
            else if (request.Armor != GunslingerDodgeArmor.Light &&
                request.Armor != GunslingerDodgeArmor.Medium)
                status = GunslingerDodgeStatus.UnsupportedArmor;
            else if (request.Load != GunslingerDodgeLoad.Light)
                status = GunslingerDodgeStatus.Overloaded;
            else if (request.CurrentGrit < 1)
                status = GunslingerDodgeStatus.InsufficientGrit;
            else if (request.Mode == GunslingerDodgeMode.DropProne &&
                !request.CanDropProne)
                status = GunslingerDodgeStatus.CannotDropProne;
            else status = GunslingerDodgeStatus.Eligible;
            return status == GunslingerDodgeStatus.Eligible
                ? new GunslingerDodgeDecision(status, request.Mode,
                    request.Mode == GunslingerDodgeMode.DropProne ? 4 : 2, 1)
                : new GunslingerDodgeDecision(status, request.Mode, 0, 0);
        }
    }
}
