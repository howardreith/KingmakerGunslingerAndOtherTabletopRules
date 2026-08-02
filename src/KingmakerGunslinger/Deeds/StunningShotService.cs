using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class StunningShotService
    {
        internal StunningShotDecision Evaluate(StunningShotRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (!request.OwnedAttack) return Reject(StunningShotStatus.WrongOwner, false);
            if (!request.ExactFirearm) return Reject(StunningShotStatus.WrongWeapon, false);
            if (!request.FirstEvaluation) return Reject(StunningShotStatus.Duplicate, false);
            if (request.Level < 19) return Reject(StunningShotStatus.LevelTooLow, true);
            if (!request.Hit) return Reject(StunningShotStatus.Miss, true);
            if (request.ImmuneToCriticalHit)
                return Reject(StunningShotStatus.CriticalImmune, true);
            if (request.CurrentGrit < 2)
                return Reject(StunningShotStatus.InsufficientGrit, true);
            return new StunningShotDecision(StunningShotStatus.Applied, true, 2,
                10 + request.Level / 2 + request.WisdomModifier);
        }

        private static StunningShotDecision Reject(StunningShotStatus status,
            bool consume)
        { return new StunningShotDecision(status, consume, 0, 0); }
    }
}
