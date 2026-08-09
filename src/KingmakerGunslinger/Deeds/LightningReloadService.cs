using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class LightningReloadService
    {
        public LightningReloadDecision Evaluate(LightningReloadRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            LightningReloadStatus status;
            if (!request.ExactFirearm) status = LightningReloadStatus.NotFirearm;
            else if (request.CurrentGrit < 1) status = LightningReloadStatus.NoGrit;
            else if (request.UsedThisRound) status = LightningReloadStatus.UsedThisRound;
            else if (request.Condition == FirearmCondition.Wrecked) status = LightningReloadStatus.Wrecked;
            else if (request.LoadedRounds >= request.Capacity) status = LightningReloadStatus.Loaded;
            else if (!request.HasBasicAmmunition) status = LightningReloadStatus.MissingAmmunition;
            else status = LightningReloadStatus.Available;
            return new LightningReloadDecision(status, request.Action);
        }
    }
}
