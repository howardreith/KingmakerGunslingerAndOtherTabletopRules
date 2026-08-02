namespace KingmakerGunslinger.Deeds
{
    internal sealed class TargetingArmsRiderDecision
    {
        internal TargetingArmsRiderDecision(bool hit, bool immuneToSneakAttack)
        { Hit = hit; ImmuneToSneakAttack = immuneToSneakAttack; }
        internal bool Hit { get; private set; }
        internal bool ImmuneToSneakAttack { get; private set; }
        internal bool ShouldDisableMainHand
        { get { return Hit && !ImmuneToSneakAttack; } }
    }
}
