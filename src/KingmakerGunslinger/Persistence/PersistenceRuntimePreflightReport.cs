using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace KingmakerGunslinger.Persistence
{
    /// <summary>
    /// Immutable ordered I01/I02 preflight report.
    /// </summary>
    internal sealed class PersistenceRuntimePreflightReport
    {
        private readonly ReadOnlyCollection<PersistenceRuntimePreflightCheck> _checks;

        internal PersistenceRuntimePreflightReport(
            IEnumerable<PersistenceRuntimePreflightCheck> checks)
        {
            if (checks == null)
            {
                throw new ArgumentNullException("checks");
            }

            List<PersistenceRuntimePreflightCheck> materialized = checks.ToList();
            if (materialized.Any(check => check == null))
            {
                throw new ArgumentException("A preflight report cannot contain null checks.", "checks");
            }

            string[] expected = { "I01", "I02" };
            if (materialized.Count != expected.Length ||
                !materialized.Select(check => check.StepId).SequenceEqual(expected, StringComparer.Ordinal))
            {
                throw new ArgumentException(
                    "A preflight report must contain I01 followed by I02 exactly once.",
                    "checks");
            }

            _checks = new ReadOnlyCollection<PersistenceRuntimePreflightCheck>(materialized);
        }

        internal IReadOnlyList<PersistenceRuntimePreflightCheck> Checks
        {
            get { return _checks; }
        }

        internal bool AllPassed
        {
            get { return _checks.All(check => check.Status == PersistenceEvidenceStatus.Pass); }
        }

        internal PersistenceRuntimePreflightCheck Require(string stepId)
        {
            PersistenceRuntimePreflightCheck result = _checks.SingleOrDefault(
                check => string.Equals(check.StepId, stepId, StringComparison.Ordinal));
            if (result == null)
            {
                throw new KeyNotFoundException("The preflight report contains no check for " + stepId + ".");
            }

            return result;
        }

        public override string ToString()
        {
            return string.Join(" | ", _checks.Select(check => check.ToString()).ToArray());
        }
    }
}
