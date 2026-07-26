using System;
using System.Globalization;

namespace KingmakerGunslinger.Qualification
{
    /// <summary>
    /// Concise deterministic PASS/FAIL matrix emitted by the Sprint 29 in-game harness.
    /// </summary>
    internal sealed class MaintenanceQualificationReport
    {
        internal MaintenanceQualificationReport(
            MaintenanceQualificationStage stage,
            bool passed,
            string[] checks)
        {
            if (!Enum.IsDefined(typeof(MaintenanceQualificationStage), stage))
            {
                throw new ArgumentOutOfRangeException(
                    "stage",
                    stage,
                    "Unknown maintenance qualification stage.");
            }

            if (checks == null || checks.Length == 0)
            {
                throw new ArgumentException(
                    "At least one qualification check is required.",
                    "checks");
            }

            Stage = stage;
            Passed = passed;
            Checks = (string[])checks.Clone();
        }

        internal MaintenanceQualificationStage Stage { get; private set; }
        internal bool Passed { get; private set; }
        internal string[] Checks { get; private set; }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "overall={0}; stage={1}; checks=[{2}]",
                Passed ? "PASS" : "FAIL",
                Stage,
                string.Join(" | ", Checks));
        }
    }
}
