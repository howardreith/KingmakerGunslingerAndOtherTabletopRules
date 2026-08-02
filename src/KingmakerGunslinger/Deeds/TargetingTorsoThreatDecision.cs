namespace KingmakerGunslinger.Deeds
{
    internal sealed class TargetingTorsoThreatDecision
    {
        internal TargetingTorsoThreatDecision(bool marked, int naturalRoll,
            bool hit, bool immuneToSneakAttack)
        {
            Marked = marked;
            NaturalRoll = naturalRoll;
            Hit = hit;
            ImmuneToSneakAttack = immuneToSneakAttack;
        }

        internal bool Marked { get; private set; }
        internal int NaturalRoll { get; private set; }
        internal bool Hit { get; private set; }
        internal bool ImmuneToSneakAttack { get; private set; }
        internal bool ShouldThreat
        {
            get
            {
                return Marked && Hit && !ImmuneToSneakAttack &&
                    NaturalRoll >= 19;
            }
        }
    }
}
