namespace KingmakerGunslinger.Acadamae
{
    internal sealed class AcadamaePrerequisiteRequest
    {
        internal int CommittedWizardLevel { get; set; }
        internal int PendingWizardLevels { get; set; }
        internal bool HasSpecialistSchool { get; set; }
        internal bool PendingSpecialistSchool { get; set; }
        internal bool IsUniversalist { get; set; }
        internal bool PendingUniversalist { get; set; }
        internal bool ConjurationForbidden { get; set; }
        internal bool PendingConjurationForbidden { get; set; }
        internal bool GivesUpSpecialization { get; set; }
    }
}
