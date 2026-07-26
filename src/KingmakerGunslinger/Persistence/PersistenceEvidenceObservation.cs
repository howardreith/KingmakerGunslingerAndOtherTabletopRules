using System;
using System.Linq;

namespace KingmakerGunslinger.Persistence
{
    internal sealed class PersistenceEvidenceObservation
    {
        internal PersistenceEvidenceObservation(
            long sequence,
            string stepId,
            PersistenceEvidenceStatus status,
            string observedAtUtc,
            string runId,
            string note,
            string beforeSnapshot,
            string afterSnapshot,
            string saveBeforeSha256,
            string saveAfterSha256)
        {
            if (sequence <= 0)
            {
                throw new ArgumentOutOfRangeException("sequence");
            }

            PersistenceMatrixCatalog.Require(stepId);
            if (!Enum.IsDefined(typeof(PersistenceEvidenceStatus), status))
            {
                throw new ArgumentOutOfRangeException("status");
            }

            DateTimeOffset parsed;
            if (string.IsNullOrWhiteSpace(observedAtUtc) ||
                !DateTimeOffset.TryParse(
                    observedAtUtc,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out parsed) ||
                parsed.Offset != TimeSpan.Zero)
            {
                throw new ArgumentException(
                    "An ISO-8601 UTC observation timestamp is required.",
                    "observedAtUtc");
            }

            if (string.IsNullOrWhiteSpace(runId))
            {
                throw new ArgumentException("A run ID is required.", "runId");
            }

            Sequence = sequence;
            StepId = stepId.Trim();
            Status = status;
            ObservedAtUtc = parsed.ToUniversalTime().ToString("O");
            RunId = runId.Trim();
            Note = Normalize(note);
            BeforeSnapshot = Normalize(beforeSnapshot);
            AfterSnapshot = Normalize(afterSnapshot);
            SaveBeforeSha256 = NormalizeHash(saveBeforeSha256, "saveBeforeSha256");
            SaveAfterSha256 = NormalizeHash(saveAfterSha256, "saveAfterSha256");
        }

        internal long Sequence { get; private set; }

        internal string StepId { get; private set; }

        internal PersistenceEvidenceStatus Status { get; private set; }

        internal string ObservedAtUtc { get; private set; }

        internal string RunId { get; private set; }

        internal string Note { get; private set; }

        internal string BeforeSnapshot { get; private set; }

        internal string AfterSnapshot { get; private set; }

        internal string SaveBeforeSha256 { get; private set; }

        internal string SaveAfterSha256 { get; private set; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string NormalizeHash(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string normalized = value.Trim().ToLowerInvariant();
            if (normalized.Length != 64 ||
                normalized.Any(character =>
                    !((character >= '0' && character <= '9') ||
                      (character >= 'a' && character <= 'f'))))
            {
                throw new ArgumentException(
                    "A SHA-256 value must contain exactly 64 lowercase hexadecimal characters.",
                    parameterName);
            }

            return normalized;
        }
    }
}
