namespace KingmakerGunslinger.Deeds
{
    internal sealed class EvasiveDecision
    {
        internal EvasiveDecision(bool shouldBeActive, bool stateChanges)
        { ShouldBeActive = shouldBeActive; StateChanges = stateChanges; }
        internal bool ShouldBeActive { get; private set; }
        internal bool StateChanges { get; private set; }
        internal int NativeBenefitCount { get { return ShouldBeActive ? 3 : 0; } }
    }
}
