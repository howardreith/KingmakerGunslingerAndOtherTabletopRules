namespace KingmakerGunslinger.Deeds
{
    public sealed class ExpertLoadingRequest
    {
        public ExpertLoadingRequest(bool exactFirearm, bool eligibleAttack,
            bool firstEvaluation, bool misfire, bool wouldExplode,
            int currentGrit)
        {
            if (currentGrit < 0) throw new System.ArgumentOutOfRangeException("currentGrit");
            ExactFirearm = exactFirearm; EligibleAttack = eligibleAttack;
            FirstEvaluation = firstEvaluation; Misfire = misfire;
            WouldExplode = wouldExplode; CurrentGrit = currentGrit;
        }

        public bool ExactFirearm { get; private set; }
        public bool EligibleAttack { get; private set; }
        public bool FirstEvaluation { get; private set; }
        public bool Misfire { get; private set; }
        public bool WouldExplode { get; private set; }
        public int CurrentGrit { get; private set; }
    }
}
