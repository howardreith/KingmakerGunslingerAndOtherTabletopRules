using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Persistence
{
    internal static class PersistenceEvidenceEvaluator
    {
        internal static PersistenceEvidenceEvaluation Evaluate(
            IEnumerable<PersistenceEvidenceObservation> observations)
        {
            if (observations == null)
            {
                throw new ArgumentNullException("observations");
            }

            List<PersistenceEvidenceObservation> materialized = observations.ToList();
            if (materialized.Any(observation => observation == null))
            {
                throw new ArgumentException("Evidence observations cannot contain null entries.", "observations");
            }

            if (materialized.Select(observation => observation.Sequence).Distinct().Count() != materialized.Count)
            {
                throw new InvalidOperationException("Evidence observation sequence numbers must be unique.");
            }

            var blocking = new List<string>();
            var warnings = new List<string>();
            int criticalPassed = 0;
            int criticalFailed = 0;
            int criticalIncomplete = 0;
            int highFailed = 0;

            foreach (PersistenceMatrixStepDefinition step in PersistenceMatrixCatalog.All)
            {
                List<PersistenceEvidenceObservation> stepObservations = materialized
                    .Where(observation => string.Equals(observation.StepId, step.Id, StringComparison.Ordinal))
                    .OrderBy(observation => observation.Sequence)
                    .ToList();
                PersistenceEvidenceObservation latest = stepObservations.LastOrDefault();

                if (step.Severity == PersistenceEvidenceSeverity.Critical)
                {
                    if (latest == null || latest.Status == PersistenceEvidenceStatus.Blocked)
                    {
                        criticalIncomplete++;
                        blocking.Add(step.Id);
                        continue;
                    }

                    if (latest.Status == PersistenceEvidenceStatus.Fail)
                    {
                        criticalFailed++;
                        blocking.Add(step.Id);
                        continue;
                    }

                    int distinctPassingRuns = stepObservations
                        .Where(observation => observation.Status == PersistenceEvidenceStatus.Pass)
                        .Select(observation => observation.RunId)
                        .Distinct(StringComparer.Ordinal)
                        .Count();
                    if (step.RequiresReproduction && distinctPassingRuns < 2)
                    {
                        criticalIncomplete++;
                        blocking.Add(step.Id);
                        warnings.Add(step.Id + " requires a second passing run.");
                        continue;
                    }

                    criticalPassed++;
                    continue;
                }

                if (latest != null && latest.Status == PersistenceEvidenceStatus.Fail)
                {
                    highFailed++;
                    warnings.Add(step.Id + " is High severity and currently failed.");
                }
            }

            PersistenceGateDecision decision = criticalFailed > 0
                ? PersistenceGateDecision.NoGoFailed
                : criticalIncomplete > 0
                    ? PersistenceGateDecision.NoGoIncomplete
                    : PersistenceGateDecision.Go;

            return new PersistenceEvidenceEvaluation(
                decision,
                criticalPassed,
                criticalFailed,
                criticalIncomplete,
                highFailed,
                blocking,
                warnings);
        }
    }
}
