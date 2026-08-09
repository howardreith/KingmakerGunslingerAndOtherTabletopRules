using System;

namespace KingmakerGunslinger.Acadamae
{
    internal static class AcadamaePrerequisitePolicy
    {
        internal static AcadamaePrerequisiteDecision Decide(AcadamaePrerequisiteRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (request.CommittedWizardLevel < 0 || request.PendingWizardLevels < 0)
                throw new ArgumentOutOfRangeException("request");
            int level = request.CommittedWizardLevel + request.PendingWizardLevels;
            string status = level < 1 ? "wizard-level-required" :
                request.GivesUpSpecialization ? "specialization-replaced" :
                request.IsUniversalist || request.PendingUniversalist ? "universalist-ineligible" :
                !(request.HasSpecialistSchool || request.PendingSpecialistSchool) ? "specialist-school-required" :
                request.ConjurationForbidden || request.PendingConjurationForbidden ? "conjuration-forbidden" : "eligible";
            return new AcadamaePrerequisiteDecision(status == "eligible", status, level);
        }
    }
}
