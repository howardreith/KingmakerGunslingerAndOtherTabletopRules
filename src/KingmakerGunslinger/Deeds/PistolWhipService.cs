using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class PistolWhipService
    {
        internal PistolWhipDecision Evaluate(PistolWhipRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (!request.ExactEquippedFirearm)
                return Reject(PistolWhipStatus.NotExactEquippedFirearm,
                    request.TwoHanded);
            if (request.Condition == FirearmCondition.Wrecked)
                return Reject(PistolWhipStatus.Wrecked, request.TwoHanded);
            if (request.CurrentGrit < 1)
                return Reject(PistolWhipStatus.InsufficientGrit,
                    request.TwoHanded);
            return new PistolWhipDecision(PistolWhipStatus.Eligible,
                request.TwoHanded, request.TwoHanded ? 10 : 6, 1);
        }

        private static PistolWhipDecision Reject(PistolWhipStatus status,
            bool twoHanded)
        {
            return new PistolWhipDecision(status, twoHanded, 0, 0);
        }
    }
}
