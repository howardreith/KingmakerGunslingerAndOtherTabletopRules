using System.Globalization;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Immutable process-lifetime diagnostics for migration from Sprint 13 direct
    /// item-reference records to Sprint 14 engine-identity records.
    /// </summary>
    internal sealed class FirearmStateIdentityMigrationSnapshot
    {
        internal FirearmStateIdentityMigrationSnapshot(
            long observedLegacyRecords,
            long migratedRecords,
            long redundantRecordsRemoved,
            long unresolvedRecordsPreserved,
            long conflicts,
            long failures,
            long rollbackFailures)
        {
            ObservedLegacyRecords = observedLegacyRecords;
            MigratedRecords = migratedRecords;
            RedundantRecordsRemoved = redundantRecordsRemoved;
            UnresolvedRecordsPreserved = unresolvedRecordsPreserved;
            Conflicts = conflicts;
            Failures = failures;
            RollbackFailures = rollbackFailures;
        }

        internal long ObservedLegacyRecords { get; private set; }

        internal long MigratedRecords { get; private set; }

        internal long RedundantRecordsRemoved { get; private set; }

        internal long UnresolvedRecordsPreserved { get; private set; }

        internal long Conflicts { get; private set; }

        internal long Failures { get; private set; }

        internal long RollbackFailures { get; private set; }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "legacyObserved={0}; migrated={1}; redundantRemoved={2}; unresolvedPreserved={3}; conflicts={4}; failures={5}; rollbackFailures={6}",
                ObservedLegacyRecords,
                MigratedRecords,
                RedundantRecordsRemoved,
                UnresolvedRecordsPreserved,
                Conflicts,
                Failures,
                RollbackFailures);
        }
    }
}
