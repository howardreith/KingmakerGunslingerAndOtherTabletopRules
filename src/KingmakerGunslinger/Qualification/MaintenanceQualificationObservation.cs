using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Qualification
{
    /// <summary>
    /// Immutable process-local observation compared with one deterministic maintenance
    /// fixture baseline.
    /// </summary>
    internal sealed class MaintenanceQualificationObservation
    {
        internal MaintenanceQualificationObservation(
            string repositoryIdentity,
            int runtimeReferenceHash,
            long revision,
            FirearmState exactState,
            int visibleFirearms,
            string secondRepositoryIdentity,
            int secondRuntimeReferenceHash,
            long secondRevision,
            FirearmState secondItemState,
            int repairKits,
            int blackPowder,
            int leadBalls,
            long overhaulCompleted,
            long repairCompleted,
            long reloadCompleted,
            long totalFaults,
            long totalDuplicates)
        {
            if (string.IsNullOrWhiteSpace(repositoryIdentity))
            {
                throw new ArgumentException(
                    "An observed repository identity is required.",
                    "repositoryIdentity");
            }

            if (string.IsNullOrWhiteSpace(secondRepositoryIdentity))
            {
                throw new ArgumentException(
                    "An observed second-item repository identity is required.",
                    "secondRepositoryIdentity");
            }

            if (revision < 0 || secondRevision < 0 || visibleFirearms < 0 ||
                repairKits < 0 || blackPowder < 0 || leadBalls < 0 ||
                overhaulCompleted < 0 || repairCompleted < 0 ||
                reloadCompleted < 0 || totalFaults < 0 || totalDuplicates < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "revision",
                    "Maintenance qualification observation values cannot be negative.");
            }

            RepositoryIdentity = repositoryIdentity;
            RuntimeReferenceHash = runtimeReferenceHash;
            Revision = revision;
            ExactState = exactState ?? throw new ArgumentNullException("exactState");
            VisibleFirearms = visibleFirearms;
            SecondRepositoryIdentity = secondRepositoryIdentity;
            SecondRuntimeReferenceHash = secondRuntimeReferenceHash;
            SecondRevision = secondRevision;
            SecondItemState = secondItemState ?? throw new ArgumentNullException("secondItemState");
            RepairKits = repairKits;
            BlackPowder = blackPowder;
            LeadBalls = leadBalls;
            OverhaulCompleted = overhaulCompleted;
            RepairCompleted = repairCompleted;
            ReloadCompleted = reloadCompleted;
            TotalFaults = totalFaults;
            TotalDuplicates = totalDuplicates;
        }

        internal string RepositoryIdentity { get; private set; }
        internal int RuntimeReferenceHash { get; private set; }
        internal long Revision { get; private set; }
        internal FirearmState ExactState { get; private set; }
        internal int VisibleFirearms { get; private set; }
        internal string SecondRepositoryIdentity { get; private set; }
        internal int SecondRuntimeReferenceHash { get; private set; }
        internal long SecondRevision { get; private set; }
        internal FirearmState SecondItemState { get; private set; }
        internal int RepairKits { get; private set; }
        internal int BlackPowder { get; private set; }
        internal int LeadBalls { get; private set; }
        internal long OverhaulCompleted { get; private set; }
        internal long RepairCompleted { get; private set; }
        internal long ReloadCompleted { get; private set; }
        internal long TotalFaults { get; private set; }
        internal long TotalDuplicates { get; private set; }
    }
}
