using System;

namespace KingmakerGunslinger.Qualification
{
    /// <summary>
    /// Process-local holder for the accelerated Sprint 29 qualification baseline. It is
    /// diagnostic-only and intentionally resets on process restart.
    /// </summary>
    internal static class MaintenanceQualificationSession
    {
        private static readonly object Gate = new object();
        private static MaintenanceQualificationBaseline _baseline;

        internal static bool IsActive
        {
            get
            {
                lock (Gate)
                {
                    return _baseline != null;
                }
            }
        }

        internal static void Begin(MaintenanceQualificationBaseline baseline)
        {
            if (baseline == null)
            {
                throw new ArgumentNullException("baseline");
            }

            lock (Gate)
            {
                _baseline = baseline;
            }
        }

        internal static MaintenanceQualificationReport Evaluate(
            MaintenanceQualificationObservation observation)
        {
            if (observation == null)
            {
                throw new ArgumentNullException("observation");
            }

            MaintenanceQualificationBaseline baseline;
            lock (Gate)
            {
                baseline = _baseline;
            }

            if (baseline == null)
            {
                return new MaintenanceQualificationReport(
                    MaintenanceQualificationStage.Failed,
                    false,
                    new[] { "fixture=FAIL(no active Sprint 29 baseline)" });
            }

            return new MaintenanceQualificationService().Evaluate(
                baseline,
                observation);
        }


        internal static bool TryGetBaseline(
            out MaintenanceQualificationBaseline baseline)
        {
            lock (Gate)
            {
                baseline = _baseline;
                return baseline != null;
            }
        }

        internal static void Reset()
        {
            lock (Gate)
            {
                _baseline = null;
            }
        }
    }
}
