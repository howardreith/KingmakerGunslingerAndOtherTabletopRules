namespace KingmakerGunslinger.Deeds
{
    public sealed class ExpertLoadingService
    {
        public ExpertLoadingDecision Evaluate(ExpertLoadingRequest request)
        {
            if (request == null) throw new System.ArgumentNullException("request");
            bool consume = request.ExactFirearm && request.EligibleAttack &&
                request.FirstEvaluation;
            bool suppress = consume && request.Misfire &&
                request.WouldExplode &&
                request.CurrentGrit >= 1;
            return new ExpertLoadingDecision(consume, suppress);
        }
    }
}
