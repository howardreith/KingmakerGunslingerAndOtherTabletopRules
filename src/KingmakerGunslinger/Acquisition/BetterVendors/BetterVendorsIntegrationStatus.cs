using System;

namespace KingmakerGunslinger.Acquisition.BetterVendors
{
    internal enum BetterVendorsIntegrationAvailability
    {
        Pending = 0,
        NotInstalled = 1,
        InstalledInactive = 2,
        Incompatible = 3,
        Ready = 4,
        Faulted = 5
    }

    internal sealed class BetterVendorsIntegrationStatus
    {
        internal BetterVendorsIntegrationStatus(
            BetterVendorsIntegrationAvailability availability, string detail)
        {
            Availability = availability;
            Detail = detail ?? string.Empty;
        }

        internal BetterVendorsIntegrationAvailability Availability
        { get; private set; }

        internal string Detail { get; private set; }

        internal bool SameAs(BetterVendorsIntegrationStatus other)
        {
            return other != null && other.Availability == Availability &&
                string.Equals(other.Detail, Detail, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return "availability=" + Availability + ";detail=" + Detail;
        }
    }

    /// <summary>
    /// Process-wide compatibility status. Update reports whether the status
    /// actually changed so callers log each state once instead of spamming the
    /// log on every retry checkpoint or trading event.
    /// </summary>
    internal static class BetterVendorsIntegrationStatusRegistry
    {
        private static readonly object Gate = new object();
        private static BetterVendorsIntegrationStatus _current =
            new BetterVendorsIntegrationStatus(
                BetterVendorsIntegrationAvailability.Pending,
                "Better Vendors compatibility has not been resolved yet.");

        internal static BetterVendorsIntegrationStatus Current
        {
            get { lock (Gate) return _current; }
        }

        internal static bool Update(BetterVendorsIntegrationStatus status)
        {
            if (status == null) throw new ArgumentNullException("status");
            lock (Gate)
            {
                if (status.SameAs(_current)) return false;
                _current = status;
                return true;
            }
        }

        /// <summary>Test seam: restores the initial pending state.</summary>
        internal static void ResetForTests()
        {
            lock (Gate)
                _current = new BetterVendorsIntegrationStatus(
                    BetterVendorsIntegrationAvailability.Pending,
                    "Better Vendors compatibility has not been resolved yet.");
        }
    }
}
