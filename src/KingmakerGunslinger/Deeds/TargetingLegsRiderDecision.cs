namespace KingmakerGunslinger.Deeds
{
    internal sealed class TargetingLegsRiderDecision
    {
        internal TargetingLegsRiderDecision(bool hit,
            bool immuneToSneakAttack, bool immuneToTrip)
        {
            Hit = hit;
            ImmuneToSneakAttack = immuneToSneakAttack;
            ImmuneToTrip = immuneToTrip;
        }
        internal bool Hit { get; private set; }
        internal bool ImmuneToSneakAttack { get; private set; }
        internal bool ImmuneToTrip { get; private set; }
        internal bool ShouldTrip
        { get { return Hit && !ImmuneToSneakAttack && !ImmuneToTrip; } }
    }
}
