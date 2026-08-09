namespace KingmakerGunslinger.Acadamae
{
    internal sealed class AcadamaePrerequisiteDecision
    {
        internal AcadamaePrerequisiteDecision(bool eligible, string status, int level)
        { Eligible = eligible; Status = status; EffectiveWizardLevel = level; }
        internal bool Eligible { get; private set; }
        internal string Status { get; private set; }
        internal int EffectiveWizardLevel { get; private set; }
    }
}
