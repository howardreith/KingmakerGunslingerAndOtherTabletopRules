using System.Globalization;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Immutable, non-retaining diagnostics for Sprint 12 token migration.
    /// </summary>
    internal sealed class FirearmStateMigrationSnapshot
    {
        internal FirearmStateMigrationSnapshot(
            long observedLegacyTokenCount,
            long migratedItemCount,
            long redundantTokenCleanupCount,
            long conflictCount,
            long failureCount,
            long rollbackFailureCount)
        {
            ObservedLegacyTokenCount = observedLegacyTokenCount;
            MigratedItemCount = migratedItemCount;
            RedundantTokenCleanupCount = redundantTokenCleanupCount;
            ConflictCount = conflictCount;
            FailureCount = failureCount;
            RollbackFailureCount = rollbackFailureCount;
        }

        internal long ObservedLegacyTokenCount { get; private set; }

        internal long MigratedItemCount { get; private set; }

        internal long RedundantTokenCleanupCount { get; private set; }

        internal long ConflictCount { get; private set; }

        internal long FailureCount { get; private set; }

        internal long RollbackFailureCount { get; private set; }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "legacyObserved={0}; migrated={1}; redundantCleanups={2}; conflicts={3}; failures={4}; rollbackFailures={5}",
                ObservedLegacyTokenCount,
                MigratedItemCount,
                RedundantTokenCleanupCount,
                ConflictCount,
                FailureCount,
                RollbackFailureCount);
        }
    }
}
