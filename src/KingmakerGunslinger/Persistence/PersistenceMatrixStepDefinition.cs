using System;

namespace KingmakerGunslinger.Persistence
{
    internal sealed class PersistenceMatrixStepDefinition
    {
        internal PersistenceMatrixStepDefinition(
            string id,
            PersistenceEvidenceSeverity severity,
            string operation,
            string requiredResult,
            bool requiresReproduction)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("A persistence-matrix step ID is required.", "id");
            }

            if (string.IsNullOrWhiteSpace(operation))
            {
                throw new ArgumentException("A persistence-matrix operation is required.", "operation");
            }

            if (string.IsNullOrWhiteSpace(requiredResult))
            {
                throw new ArgumentException("A required result is required.", "requiredResult");
            }

            if (!Enum.IsDefined(typeof(PersistenceEvidenceSeverity), severity))
            {
                throw new ArgumentOutOfRangeException("severity");
            }

            Id = id.Trim();
            Severity = severity;
            Operation = operation.Trim();
            RequiredResult = requiredResult.Trim();
            RequiresReproduction = requiresReproduction;
        }

        internal string Id { get; private set; }

        internal PersistenceEvidenceSeverity Severity { get; private set; }

        internal string Operation { get; private set; }

        internal string RequiredResult { get; private set; }

        internal bool RequiresReproduction { get; private set; }
    }
}
