namespace KingmakerGunslinger.Deeds
{
    internal sealed class TargetingLegsRiderService
    {
        internal TargetingLegsRiderDecision Evaluate(bool hit,
            bool immuneToSneakAttack, bool immuneToTrip)
        {
            return new TargetingLegsRiderDecision(hit, immuneToSneakAttack,
                immuneToTrip);
        }
    }
}
