using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class StartlingShotService
    {
        internal StartlingShotDecision Evaluate(StartlingShotRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (!request.ExactEquippedFirearm)
                return Reject(StartlingShotStatus.NotExactEquippedFirearm);
            if (request.Condition == FirearmCondition.Wrecked)
                return Reject(StartlingShotStatus.Wrecked);
            if (request.LoadedRounds < 1)
                return Reject(StartlingShotStatus.Empty);
            if (request.CurrentGrit < 1)
                return Reject(StartlingShotStatus.InsufficientGrit);
            if (!request.ValidEnemyTarget)
                return Reject(StartlingShotStatus.InvalidTarget);
            return new StartlingShotDecision(StartlingShotStatus.Eligible, 1, 0, 1);
        }

        private static StartlingShotDecision Reject(StartlingShotStatus status)
        {
            return new StartlingShotDecision(status, 0, 0, 0);
        }
    }
}
