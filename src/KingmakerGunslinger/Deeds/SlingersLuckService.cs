namespace KingmakerGunslinger.Deeds
{
    internal sealed class SlingersLuckService
    {
        internal SlingersLuckDecision Evaluate(SlingersLuckRequest request)
        {
            if (request == null) throw new System.ArgumentNullException("request");
            int cost = request.ArmedKind == SlingersLuckRollKind.SavingThrow ? 2 : 1;
            if (!request.Armed) return Reject(SlingersLuckStatus.NotArmed, request);
            if (request.ArmedKind != request.EventKind)
                return Reject(SlingersLuckStatus.WrongKind, request);
            if (!request.FirstEvaluation)
                return Reject(SlingersLuckStatus.Duplicate, request);
            if (request.GunslingerLevel < 15)
                return Reject(SlingersLuckStatus.LevelTooLow, request);
            if (request.CurrentGrit < cost)
                return Reject(SlingersLuckStatus.InsufficientGrit, request);
            return new SlingersLuckDecision(SlingersLuckStatus.Applied, cost,
                request.SecondRoll);
        }

        private static SlingersLuckDecision Reject(SlingersLuckStatus status,
            SlingersLuckRequest request)
        { return new SlingersLuckDecision(status, 0, request.FirstRoll); }
    }
}
