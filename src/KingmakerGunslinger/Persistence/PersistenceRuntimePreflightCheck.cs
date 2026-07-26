using System;
using System.Globalization;

namespace KingmakerGunslinger.Persistence
{
    /// <summary>
    /// One deterministic result produced by the trusted Sprint 17 runtime preflight.
    /// Only I01 and I02 are eligible for automatic evidence recording.
    /// </summary>
    internal sealed class PersistenceRuntimePreflightCheck
    {
        internal PersistenceRuntimePreflightCheck(
            string stepId,
            PersistenceEvidenceStatus status,
            string detail)
        {
            PersistenceMatrixStepDefinition step = PersistenceMatrixCatalog.Require(stepId);
            if (!string.Equals(step.Id, "I01", StringComparison.Ordinal) &&
                !string.Equals(step.Id, "I02", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "The runtime preflight may produce checks only for I01 and I02.",
                    "stepId");
            }

            if (!Enum.IsDefined(typeof(PersistenceEvidenceStatus), status))
            {
                throw new ArgumentOutOfRangeException("status");
            }

            if (string.IsNullOrWhiteSpace(detail))
            {
                throw new ArgumentException("A preflight detail is required.", "detail");
            }

            StepId = step.Id;
            Status = status;
            Detail = detail.Trim();
        }

        internal string StepId { get; private set; }

        internal PersistenceEvidenceStatus Status { get; private set; }

        internal string Detail { get; private set; }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}={1}[{2}]",
                StepId,
                Status,
                Detail);
        }
    }
}
