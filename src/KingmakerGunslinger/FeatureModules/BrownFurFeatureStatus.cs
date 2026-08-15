using System;

namespace KingmakerGunslinger.FeatureModules
{
    internal enum BrownFurDependencyAvailability
    {
        Unavailable = 0,
        Available = 1,
        Blocked = 2
    }

    internal sealed class BrownFurFeatureStatus
    {
        internal BrownFurFeatureStatus(BrownFurDependencyAvailability availability,
            bool published, string detail)
        {
            if (published && availability != BrownFurDependencyAvailability.Available)
                throw new ArgumentException(
                    "Brown-Fur cannot be published without a compatible dependency.",
                    "published");
            Availability = availability;
            Published = published;
            Detail = detail ?? string.Empty;
        }

        internal BrownFurDependencyAvailability Availability { get; private set; }
        internal bool Published { get; private set; }
        internal string Detail { get; private set; }

        internal string DependencyStatus
        {
            get
            {
                switch (Availability)
                {
                    case BrownFurDependencyAvailability.Available:
                        return "Available  compatible Call of the Wild detected";
                    case BrownFurDependencyAvailability.Blocked:
                        return "Blocked  installed Call of the Wild is incompatible";
                    default:
                        return "Unavailable  Call of the Wild not detected";
                }
            }
        }

        internal string PublicationStatus
        { get { return Published ? "Published" : "Not published"; } }
    }

    internal static class BrownFurFeatureStatusRegistry
    {
        private static readonly object Sync = new object();
        private static BrownFurFeatureStatus _current = new BrownFurFeatureStatus(
            BrownFurDependencyAvailability.Unavailable, false, "not-reconciled");

        internal static BrownFurFeatureStatus Current
        {
            get { lock (Sync) return _current; }
        }

        internal static void Update(BrownFurFeatureStatus status)
        {
            if (status == null) throw new ArgumentNullException("status");
            lock (Sync) _current = status;
        }
    }
}
