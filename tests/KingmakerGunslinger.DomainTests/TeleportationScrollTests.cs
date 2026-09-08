using System;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    internal static class TeleportationScrollTests
    {
        internal static void VisibleSelectionDoesNotMoveViewport()
        { Assertions.Equal(100f, TeleportContextScrollPolicy.Reveal(600, 200, 100, 150, 190), "Keep an already visible source stable."); }
        internal static void SelectionAboveUsesSmallestScroll()
        { Assertions.Equal(80f, TeleportContextScrollPolicy.Reveal(600, 200, 100, 80, 120), "Only reveal the hidden top of the source."); }
        internal static void SelectionBelowUsesSmallestScroll()
        { Assertions.Equal(130f, TeleportContextScrollPolicy.Reveal(600, 200, 100, 290, 330), "Only reveal the hidden bottom of the source."); }
        internal static void FirstAndLastSourcesRemainReachable()
        {
            Assertions.Equal(0f, TeleportContextScrollPolicy.Reveal(600, 200, 400, 0, 40), "First source.");
            Assertions.Equal(400f, TeleportContextScrollPolicy.Reveal(600, 200, 0, 560, 600), "Last source.");
        }
        internal static void ShortContentDoesNotScroll()
        { Assertions.Equal(0f, TeleportContextScrollPolicy.Reveal(100, 200, 300, 60, 100), "All sources fit."); }
        internal static void OversizedRowHasStableTopAlignment()
        {
            float offset = TeleportContextScrollPolicy.Reveal(600, 100, 0, 200, 400);
            Assertions.Equal(200f, offset, "A tall source has a reachable top.");
            Assertions.Equal(offset, TeleportContextScrollPolicy.Reveal(600, 100, offset, 200, 400), "No repeated-frame oscillation.");
        }
        internal static void UnknownGeometryFailsClosed()
        {
            foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                Assertions.Throws<ArgumentOutOfRangeException>(() => TeleportContextScrollPolicy.Reveal(600, 200, invalid, 0, 40), "No invalid native offset.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => TeleportContextScrollPolicy.Reveal(600, 0, 0, 0, 40), "No empty viewport.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => TeleportContextScrollPolicy.Reveal(600, 200, 0, 80, 40), "No inverted row.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => TeleportContextScrollPolicy.Reveal(600, 200, 0, 560, 640), "No row outside content.");
        }
    }
}
