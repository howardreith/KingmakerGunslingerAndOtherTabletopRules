using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class GunslingerDodgeService
    {
        // Legacy request/status vocabulary (DropProne, UnsupportedArmor,
        // Overloaded, CannotDropProne) remains serialized and append-only, but
        // the 0.0.63 player-facing adaptation deliberately does not use those
        // gates or apply the native Prone condition.
        internal GunslingerDodgeDecision Evaluate(GunslingerDodgeRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            GunslingerDodgeStatus status;
            if (!request.IsArmed) status = GunslingerDodgeStatus.NotArmed;
            else if (!request.IsRangedAttack) status = GunslingerDodgeStatus.NotRangedAttack;
            else if (request.CurrentGrit < 1)
                status = GunslingerDodgeStatus.InsufficientGrit;
            else status = GunslingerDodgeStatus.Eligible;
            return status == GunslingerDodgeStatus.Eligible
                ? new GunslingerDodgeDecision(status, request.Mode, 2, 1)
                : new GunslingerDodgeDecision(status, request.Mode, 0, 0);
        }
    }
}
