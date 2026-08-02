using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class EvasiveService
    {
        internal EvasiveDecision Evaluate(EvasiveRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            bool active = request.GunslingerLevel >= 15 && request.CurrentGrit >= 1;
            return new EvasiveDecision(active, active != request.BenefitsActive);
        }
    }
}
