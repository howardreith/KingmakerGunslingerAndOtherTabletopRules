using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    internal enum FavoredClassIntegrationAvailability
    {
        Pending = 0,
        /// <summary>The owned leaves could not be registered; nothing is offered.</summary>
        RegistrationFailed = 1,
        /// <summary>The user disabled the integration (restart-required setting).</summary>
        IntegrationDisabled = 2,
        HostAbsent = 3,
        HostDisabled = 4,
        UnsupportedBinary = 5,
        HostIncomplete = 6,
        /// <summary>Exact host ready and every eligible owned leaf published.</summary>
        Published = 7,
        /// <summary>Publication failed and rolled back; no partial graph remains.</summary>
        PublicationFailed = 8
    }

    /// <summary>Immutable status snapshot for diagnostics and the settings UI.</summary>
    internal sealed class FavoredClassIntegrationStatus
    {
        internal FavoredClassIntegrationStatus(FavoredClassIntegrationAvailability availability,
            string detail, int publishedLeaves, IEnumerable<string> skipped)
        {
            Availability = availability;
            Detail = detail ?? string.Empty;
            PublishedLeaves = publishedLeaves;
            Skipped = (skipped ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
        }

        internal FavoredClassIntegrationAvailability Availability { get; private set; }
        internal string Detail { get; private set; }
        internal int PublishedLeaves { get; private set; }
        internal IList<string> Skipped { get; private set; }

        internal bool SameAs(FavoredClassIntegrationStatus other)
        {
            return other != null && other.Availability == Availability &&
                other.PublishedLeaves == PublishedLeaves &&
                string.Equals(other.Detail, Detail, StringComparison.Ordinal) &&
                other.Skipped.SequenceEqual(Skipped, StringComparer.Ordinal);
        }

        public override string ToString()
        {
            return "availability=" + Availability + ";publishedLeaves=" + PublishedLeaves +
                ";skipped=" + string.Join(",", Skipped.ToArray()) + ";detail=" + Detail;
        }

        internal static FavoredClassIntegrationAvailability FromHostState(FavoredClassHostState state)
        {
            switch (state)
            {
                case FavoredClassHostState.Absent:
                    return FavoredClassIntegrationAvailability.HostAbsent;
                case FavoredClassHostState.Disabled:
                    return FavoredClassIntegrationAvailability.HostDisabled;
                case FavoredClassHostState.UnsupportedBinary:
                    return FavoredClassIntegrationAvailability.UnsupportedBinary;
                case FavoredClassHostState.IncompleteInitialization:
                    return FavoredClassIntegrationAvailability.HostIncomplete;
                default:
                    return FavoredClassIntegrationAvailability.Pending;
            }
        }
    }

    /// <summary>
    /// Process-wide status. Update reports whether the status changed so each
    /// state is logged once.
    /// </summary>
    internal static class FavoredClassIntegrationStatusRegistry
    {
        private static readonly object Gate = new object();
        private static FavoredClassIntegrationStatus _current = new FavoredClassIntegrationStatus(
            FavoredClassIntegrationAvailability.Pending,
            "Favored Class compatibility has not been resolved yet.", 0, null);

        internal static FavoredClassIntegrationStatus Current
        {
            get { lock (Gate) return _current; }
        }

        internal static bool Update(FavoredClassIntegrationStatus status)
        {
            if (status == null)
                throw new ArgumentNullException("status");
            lock (Gate)
            {
                if (_current.SameAs(status))
                    return false;
                _current = status;
                return true;
            }
        }
    }
}
