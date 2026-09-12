using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Gunsmithing
{
    internal enum CompletedRestMaintenanceStatus
    {
        NotARestCompletion = 0,
        NoCapableRepairer = 1,
        NoRepairKit = 2,
        MaintenanceDue = 3
    }

    internal enum CompletedRestItemDecision
    {
        SkipNormal = 0,
        RestoreBroken = 1,
        RestoreWrecked = 2
    }

    /// <summary>
    /// Pure decisions for automatic firearm maintenance after a genuine
    /// completed full rest. Only a successful, uninterrupted, non-skip-time
    /// rest qualifies; a participating capable gunsmith plus at least one
    /// reusable Gunsmith's Kit restore the party's carried Broken and Wrecked
    /// firearms directly to Normal (a Wrecked firearm stays unloaded).
    /// </summary>
    internal static class CompletedRestMaintenancePolicy
    {
        internal static CompletedRestMaintenanceStatus EvaluateRest(
            bool restSucceeded,
            bool interruptedByEncounter,
            bool skipTime,
            bool hasCapableRepairer,
            bool hasRepairKit)
        {
            if (!restSucceeded || interruptedByEncounter || skipTime)
            {
                return CompletedRestMaintenanceStatus.NotARestCompletion;
            }

            if (!hasCapableRepairer)
            {
                return CompletedRestMaintenanceStatus.NoCapableRepairer;
            }

            if (!hasRepairKit)
            {
                return CompletedRestMaintenanceStatus.NoRepairKit;
            }

            return CompletedRestMaintenanceStatus.MaintenanceDue;
        }

        internal static CompletedRestItemDecision EvaluateItem(
            FirearmCondition actualCondition)
        {
            if (!Enum.IsDefined(typeof(FirearmCondition), actualCondition))
            {
                throw new ArgumentOutOfRangeException("actualCondition");
            }

            switch (actualCondition)
            {
                case FirearmCondition.Normal:
                    return CompletedRestItemDecision.SkipNormal;
                case FirearmCondition.Broken:
                    return CompletedRestItemDecision.RestoreBroken;
                default:
                    return CompletedRestItemDecision.RestoreWrecked;
            }
        }

        /// <summary>
        /// A blocked explanation is owed only when a genuine rest completed
        /// while damaged carried firearms could not be restored.
        /// </summary>
        internal static bool ShouldReportBlocked(
            CompletedRestMaintenanceStatus status,
            int damagedCarriedCount)
        {
            return damagedCarriedCount > 0 &&
                (status == CompletedRestMaintenanceStatus.NoCapableRepairer ||
                    status == CompletedRestMaintenanceStatus.NoRepairKit);
        }

        internal static string DescribeRestored(
            int restoredBroken,
            int restoredWrecked,
            int failed)
        {
            if (restoredBroken < 0 || restoredWrecked < 0 || failed < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "Restoration counts cannot be negative.");
            }

            int restored = restoredBroken + restoredWrecked;
            if (restored == 0 && failed == 0)
            {
                return null;
            }

            return restored == 0
                ? "Full-rest firearm maintenance attempted damaged firearms but every restoration failed; nothing was changed."
                : string.Format(
                    "Full-rest maintenance restored {0} firearm{1} to Normal ({2} Broken, {3} Wrecked){4}.",
                    restored,
                    restored == 1 ? string.Empty : "s",
                    restoredBroken,
                    restoredWrecked,
                    failed == 0
                        ? string.Empty
                        : "; " + failed + " restoration" +
                            (failed == 1 ? string.Empty : "s") + " failed");
        }

        internal static string DescribeBlocked(
            CompletedRestMaintenanceStatus status,
            int damagedCarriedCount)
        {
            if (!ShouldReportBlocked(status, damagedCarriedCount))
            {
                return null;
            }

            return status == CompletedRestMaintenanceStatus.NoRepairKit
                ? string.Format(
                    "{0} damaged carried firearm{1} could not be maintained: a reusable Gunsmith's Kit is required in the shared inventory.",
                    damagedCarriedCount,
                    damagedCarriedCount == 1 ? string.Empty : "s")
                : string.Format(
                    "{0} damaged carried firearm{1} could not be maintained: no resting gunsmith has the Gunsmithing repair capability.",
                    damagedCarriedCount,
                    damagedCarriedCount == 1 ? string.Empty : "s");
        }
    }
}
