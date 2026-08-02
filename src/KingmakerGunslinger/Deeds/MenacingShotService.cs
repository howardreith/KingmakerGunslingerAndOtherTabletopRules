using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class MenacingShotService
    {
        internal MenacingShotDecision Evaluate(MenacingShotRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (request.GunslingerLevel < 15)
                return Reject(MenacingShotStatus.BelowRequiredLevel);
            if (!request.ExactEquippedFirearm)
                return Reject(MenacingShotStatus.NotExactEquippedFirearm);
            if (request.Condition == FirearmCondition.Wrecked)
                return Reject(MenacingShotStatus.Wrecked);
            if (request.LoadedRounds < 1) return Reject(MenacingShotStatus.Empty);
            if (request.CurrentGrit < 1)
                return Reject(MenacingShotStatus.InsufficientGrit);
            return new MenacingShotDecision(MenacingShotStatus.Eligible,
                10 + (request.GunslingerLevel / 2) + request.WisdomModifier,
                request.GunslingerLevel, 1, 1);
        }

        private static MenacingShotDecision Reject(MenacingShotStatus status)
        { return new MenacingShotDecision(status, 0, 0, 0, 0); }
    }
}
