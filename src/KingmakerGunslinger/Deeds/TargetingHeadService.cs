using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class TargetingHeadService
    {
        internal TargetingHeadDecision Evaluate(TargetingHeadRequest request)
        {
            if (request == null) throw new System.ArgumentNullException("request");
            TargetingHeadStatus status;
            if (!request.ExactFirearm) status = TargetingHeadStatus.NoExactFirearm;
            else if (request.Condition == FirearmCondition.Wrecked) status = TargetingHeadStatus.Wrecked;
            else if (request.LoadedRounds == 0) status = TargetingHeadStatus.Empty;
            else if (request.CurrentGrit < 1) status = TargetingHeadStatus.InsufficientGrit;
            else if (!request.ValidTarget) status = TargetingHeadStatus.InvalidTarget;
            else status = TargetingHeadStatus.Accepted;
            return new TargetingHeadDecision(status);
        }
        internal TargetingHeadRiderDecision EvaluateRider(bool hit, bool immune)
        { return new TargetingHeadRiderDecision(hit, immune); }
    }
}
