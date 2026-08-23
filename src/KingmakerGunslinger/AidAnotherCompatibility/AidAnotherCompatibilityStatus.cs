using System;

namespace KingmakerGunslinger.AidAnotherCompatibility
{
    internal enum OptionalAidAnotherAvailability
    {
        Absent = 0,
        Pending = 1,
        Compatible = 2,
        Blocked = 3
    }

    internal sealed class AidAnotherCompatibilityStatus
    {
        internal AidAnotherCompatibilityStatus(
            OptionalAidAnotherAvailability cotw,
            OptionalAidAnotherAvailability favoredClass,
            bool? favoredTraitsEnabled, bool ordinaryAidIntegrated,
            bool helpfulPublished, string detail)
        {
            if (ordinaryAidIntegrated &&
                cotw != OptionalAidAnotherAvailability.Compatible)
                throw new ArgumentException(
                    "Ordinary Aid Another cannot be integrated without a compatible CotW contract.",
                    "ordinaryAidIntegrated");
            if (helpfulPublished && (cotw !=
                    OptionalAidAnotherAvailability.Compatible ||
                favoredClass != OptionalAidAnotherAvailability.Compatible ||
                favoredTraitsEnabled != true))
                throw new ArgumentException(
                    "Helpful cannot be published without both compatible mods and enabled traits.",
                    "helpfulPublished");
            Cotw = cotw;
            FavoredClass = favoredClass;
            FavoredTraitsEnabled = favoredTraitsEnabled;
            OrdinaryAidIntegrated = ordinaryAidIntegrated;
            HelpfulPublished = helpfulPublished;
            Detail = detail ?? string.Empty;
        }

        internal OptionalAidAnotherAvailability Cotw { get; private set; }
        internal OptionalAidAnotherAvailability FavoredClass { get; private set; }
        internal bool? FavoredTraitsEnabled { get; private set; }
        internal bool OrdinaryAidIntegrated { get; private set; }
        internal bool HelpfulPublished { get; private set; }
        internal string Detail { get; private set; }

        internal string CotwStatus
        { get { return "Call of the Wild: " + Cotw; } }

        internal string FavoredClassStatus
        {
            get
            {
                string traits = FavoredTraitsEnabled.HasValue ?
                    (FavoredTraitsEnabled.Value ? "traits enabled" :
                        "traits disabled") : "traits unavailable";
                return "Favored Class: " + FavoredClass + " (" + traits + ")";
            }
        }

        internal string PublicationStatus
        {
            get
            {
                if (HelpfulPublished) return "compatible and Helpful published";
                if (Cotw == OptionalAidAnotherAvailability.Blocked ||
                    FavoredClass == OptionalAidAnotherAvailability.Blocked)
                    return "blocked structural contract";
                if (Cotw == OptionalAidAnotherAvailability.Compatible &&
                    FavoredClass == OptionalAidAnotherAvailability.Compatible)
                    return "compatible but module OFF or traits disabled; Helpful not published";
                return "optional Helpful extension not published";
            }
        }
    }

    internal static class AidAnotherCompatibilityStatusRegistry
    {
        private static readonly object Gate = new object();
        private static AidAnotherCompatibilityStatus _current =
            new AidAnotherCompatibilityStatus(
                OptionalAidAnotherAvailability.Pending,
                OptionalAidAnotherAvailability.Pending, null, false, false,
                "not-reconciled");

        internal static AidAnotherCompatibilityStatus Current
        { get { lock (Gate) return _current; } }

        internal static void Update(AidAnotherCompatibilityStatus status)
        {
            if (status == null) throw new ArgumentNullException("status");
            lock (Gate) _current = status;
        }
    }
}
