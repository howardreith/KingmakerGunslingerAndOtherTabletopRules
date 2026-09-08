using System;
using System.Globalization;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    internal static class TeleportationExplorationTests
    {
        private const string Map = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", Point = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb", Other = "cccccccccccccccccccccccccccccccc";
        internal static void StationaryArrivalSuppressesRepeatedTicks()
        {
            var state = new TeleportExplorationBoundary(Map, Point, 10.25f);
            for (int tick = 0; tick < 100; tick++) Assertions.True(state.Suppress(Map, Point, 10.25f, false), "No delayed exploration from the same magical arrival.");
        }
        internal static void NativeWalkingReleases()
        { Assertions.True(!new TeleportExplorationBoundary(Map, Point, 0).Suppress(Map, Point, 0, true), "Ordinary walking restores native exploration before traversing a node."); }
        internal static void ChangedPointOrMapReleases()
        {
            var state = new TeleportExplorationBoundary(Map, Point, 1);
            Assertions.True(!state.Suppress(Map, Other, 1, false), "Native/scripted relocation releases the old arrival.");
            Assertions.True(!state.Suppress(Other, Point, 1, false), "Another map has no inherited suppression.");
            Assertions.True(!state.Suppress(Map, null, 1, false), "An edge position is ordinary travel.");
        }
        internal static void TravelWhileDisabledDoesNotRetainOldSuppression()
        { Assertions.True(!new TeleportExplorationBoundary(Map, Point, 2).Suppress(Map, Point, 3, false), "Native mileage distinguishes a later return while the module was OFF."); }
        internal static void SavedBoundaryRoundTripsInvariantly()
        {
            var old = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                var state = new TeleportExplorationBoundary(Map, Point, 12.375f);
                var restored = TeleportExplorationBoundary.Parse(state.Serialize());
                Assertions.Equal(state.Serialize(), restored.Serialize(), "Exact save-owned payload survives culture and reload.");
                Assertions.True(restored.Suppress(Map, Point, 12.375f, false), "Loading at the magical destination does not perform exploration.");
            }
            finally { CultureInfo.CurrentCulture = old; }
        }
        internal static void LegacyCampaignHasNoBoundary()
        { Assertions.Equal<TeleportExplorationBoundary>(null, TeleportExplorationBoundary.Parse(null), "Absent old-save field keeps native exploration."); }
        internal static void OrdinaryWalkingReleasesMalformedSavedData()
        {
            Assertions.True(!TeleportExplorationBoundary.SuppressSaved("corrupt", Map, Point, 0, true), "Ordinary travel remains native even with corrupt spell state.");
            Assertions.True(!TeleportExplorationBoundary.SuppressSaved(null, Map, Point, 0, false), "No spell boundary leaves native exploration unchanged.");
            Assertions.Throws<FormatException>(() => TeleportExplorationBoundary.SuppressSaved("corrupt", Map, Point, 0, false), "Stationary corrupt state still fails closed.");
        }
        internal static void InvalidBoundariesFailClosed()
        {
            foreach (float miles in new[] { -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                Assertions.Throws<ArgumentException>(() => new TeleportExplorationBoundary(Map, Point, miles), "No invalid native mileage.");
            Assertions.Throws<ArgumentException>(() => new TeleportExplorationBoundary("display name", Point, 0), "Map identity must be persistent.");
            Assertions.Throws<ArgumentException>(() => new TeleportExplorationBoundary(Map, "", 0), "Point identity must be persistent.");
            foreach (string payload in new[] { "", "2|" + Map + "|" + Point + "|0", "1|" + Map + "|" + Point + "|01", "1|" + Map + "|" + Point + "|1,5" })
                Assertions.Throws<FormatException>(() => TeleportExplorationBoundary.Parse(payload), "Unknown/noncanonical data cannot silently enable exploration.");
        }
    }
}
