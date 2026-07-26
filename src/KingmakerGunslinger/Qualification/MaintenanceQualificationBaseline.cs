using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Qualification
{
    /// <summary>
    /// Process-local baseline captured after the deterministic Sprint 29 fixture is
    /// prepared. It is correlation evidence only and is never used for gameplay
    /// decisions or persistence.
    /// </summary>
    internal sealed class MaintenanceQualificationBaseline
    {
        internal MaintenanceQualificationBaseline(
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
                    "A baseline repository identity is required.",
                    "repositoryIdentity");
            }

            if (string.IsNullOrWhiteSpace(secondRepositoryIdentity))
            {
                throw new ArgumentException(
                    "A baseline second-item repository identity is required.",
                    "secondRepositoryIdentity");
            }

            if (string.Equals(
                    repositoryIdentity,
                    secondRepositoryIdentity,
                    StringComparison.Ordinal) ||
                runtimeReferenceHash == secondRuntimeReferenceHash)
            {
                throw new ArgumentException(
                    "The qualification fixture requires two distinct exact firearm items.");
            }

            if (revision < 0 || secondRevision < 0 || visibleFirearms < 2 ||
                repairKits < 2 || blackPowder < 1 || leadBalls < 1 ||
                overhaulCompleted < 0 || repairCompleted < 0 ||
                reloadCompleted < 0 || totalFaults < 0 || totalDuplicates < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "revision",
                    "The maintenance baseline requires nonnegative values, at least two visible firearms, two repair kits, and one complete ammunition pair.");
            }

            ExactState = exactState ?? throw new ArgumentNullException("exactState");
            SecondItemState = secondItemState ?? throw new ArgumentNullException("secondItemState");
            if (!ExactState.IsEmpty || ExactState.Condition != FirearmCondition.Wrecked)
            {
                throw new ArgumentException(
                    "The maintenance qualification baseline exact item must be empty/Wrecked.",
                    "exactState");
            }

            if (!SecondItemState.IsEmpty ||
                SecondItemState.Condition != FirearmCondition.Normal)
            {
                throw new ArgumentException(
                    "The maintenance qualification baseline second item must be empty/Normal.",
                    "secondItemState");
            }

            RepositoryIdentity = repositoryIdentity;
            RuntimeReferenceHash = runtimeReferenceHash;
            Revision = revision;
            VisibleFirearms = visibleFirearms;
            SecondRepositoryIdentity = secondRepositoryIdentity;
            SecondRuntimeReferenceHash = secondRuntimeReferenceHash;
            SecondRevision = secondRevision;
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
