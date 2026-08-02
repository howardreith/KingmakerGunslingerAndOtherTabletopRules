namespace KingmakerGunslinger.Deeds
{
    internal sealed class TargetingArmsRiderService
    {
        internal TargetingArmsRiderDecision Evaluate(bool hit,
            bool immuneToSneakAttack)
        { return new TargetingArmsRiderDecision(hit, immuneToSneakAttack); }
    }
}
