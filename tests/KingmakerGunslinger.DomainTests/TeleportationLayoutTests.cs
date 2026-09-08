using System;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    internal static class TeleportationLayoutTests
    {
        internal static void NativeActionsAndRowsFitAroundSelectedAnchor()
        {
            foreach (float height in new[] { 600f, 900f, 1200f })
            foreach (float anchor in new[] { 0f, 0.1f, 0.5f, 0.9f, 1f })
            {
                float rows = TeleportContextLayoutPolicy.MaximumRowsHeight(height, anchor, 160, 30);
                Assertions.True(rows >= 30 && rows + 160 <= height * Math.Max(anchor, 1 - anchor),
                    "The original actions and source viewport must fit above or below the selected native point.");
            }
        }
        internal static void LongListsScrollBeforeObscuringNativeActions()
        {
            float rows = TeleportContextLayoutPolicy.MaximumRowsHeight(1200, 0.5f, 180, 35.5f);
            Assertions.True(rows < 12 * 35.5f && rows >= 6 * 35.5f,
                "A long real source list scrolls while a short list fits without taking space from native actions.");
            Assertions.Equal(rows, TeleportContextLayoutPolicy.MaximumRowsHeight(1200, 0.5f, 180, 35.5f), "Geometry is deterministic.");
        }
        internal static void UnprovenOrInsufficientGeometryFailsClosed()
        {
            foreach (float value in new[] { float.NaN, float.PositiveInfinity, -1f, 2f })
                Assertions.Throws<ArgumentOutOfRangeException>(() => TeleportContextLayoutPolicy.MaximumRowsHeight(600, value, 160, 30), "Invalid native viewport coordinate.");
            Assertions.Throws<InvalidOperationException>(() => TeleportContextLayoutPolicy.MaximumRowsHeight(200, 0.5f, 160, 30), "No space for a source row.");
        }
    }
}
