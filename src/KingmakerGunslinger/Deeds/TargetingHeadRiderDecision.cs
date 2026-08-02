namespace KingmakerGunslinger.Deeds
{
    internal sealed class TargetingHeadRiderDecision
    {
        internal TargetingHeadRiderDecision(bool attackHit, bool immune)
        { AttackHit = attackHit; ImmuneToSneakAttack = immune; }
        internal bool AttackHit { get; private set; }
        internal bool ImmuneToSneakAttack { get; private set; }
        internal bool ShouldConfuse { get { return AttackHit && !ImmuneToSneakAttack; } }
        internal int DurationRounds { get { return ShouldConfuse ? 1 : 0; } }
    }
}
