using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace KingmakerGunslinger.Persistence
{
    internal sealed class PersistenceEvidenceEvaluation
    {
        internal PersistenceEvidenceEvaluation(
            PersistenceGateDecision decision,
            int criticalPassed,
            int criticalFailed,
            int criticalIncomplete,
            int highFailed,
            IEnumerable<string> blockingStepIds,
            IEnumerable<string> warnings)
        {
            Decision = decision;
            CriticalPassed = criticalPassed;
            CriticalFailed = criticalFailed;
            CriticalIncomplete = criticalIncomplete;
            HighFailed = highFailed;
            BlockingStepIds = new ReadOnlyCollection<string>(
                new List<string>(blockingStepIds ?? throw new ArgumentNullException("blockingStepIds")));
            Warnings = new ReadOnlyCollection<string>(
                new List<string>(warnings ?? throw new ArgumentNullException("warnings")));
        }

        internal PersistenceGateDecision Decision { get; private set; }

        internal int CriticalPassed { get; private set; }

        internal int CriticalFailed { get; private set; }

        internal int CriticalIncomplete { get; private set; }

        internal int HighFailed { get; private set; }

        internal IReadOnlyList<string> BlockingStepIds { get; private set; }

        internal IReadOnlyList<string> Warnings { get; private set; }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "decision={0}; criticalPassed={1}; criticalFailed={2}; criticalIncomplete={3}; highFailed={4}; blockers={5}; warnings={6}",
                Decision,
                CriticalPassed,
                CriticalFailed,
                CriticalIncomplete,
                HighFailed,
                BlockingStepIds.Count,
                Warnings.Count);
        }
    }
}
