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
        internal static void RowWidthFollowsTheSettledNativeActionExtent()
        {
            // The dialog canvas group is WIDER than the visible parchment action
            // region in the reported 1920x1200 geometry; the donor button's
            // settled extent must win, not the padded group.
            Assertions.Equal(360f, TeleportContextLayoutPolicy.ActionRowsWidth(360f, 520f), "Settled native action width wins.");
            Assertions.Equal(300f, TeleportContextLayoutPolicy.ActionRowsWidth(480f, 300f), "Padded dialog bound still caps the rows.");
            Assertions.Equal(360f, TeleportContextLayoutPolicy.ActionRowsWidth(360f, 360f), "Exact agreement is accepted.");
            foreach (float value in new[] { 0f, -5f, float.NaN, float.PositiveInfinity })
            {
                Assertions.Throws<InvalidOperationException>(() => TeleportContextLayoutPolicy.ActionRowsWidth(value, 400f), "Unproven native action width fails closed.");
                Assertions.Throws<InvalidOperationException>(() => TeleportContextLayoutPolicy.ActionRowsWidth(360f, value), "Unproven dialog width fails closed.");
            }
        }
        internal static void RenderedRowsMustStayInsideTheNativeExtent()
        {
            Assertions.True(TeleportContextLayoutPolicy.RowInsideNativeExtent(100f, 460f, 100f, 460f), "Exact containment passes.");
            Assertions.True(TeleportContextLayoutPolicy.RowInsideNativeExtent(100.2f, 460.3f, 100f, 460f), "Sub-pixel drift stays accepted.");
            Assertions.False(TeleportContextLayoutPolicy.RowInsideNativeExtent(99f, 460f, 100f, 460f), "Left overhang is rejected.");
            Assertions.False(TeleportContextLayoutPolicy.RowInsideNativeExtent(100f, 462f, 100f, 460f), "Right overhang is rejected.");
            Assertions.False(TeleportContextLayoutPolicy.RowInsideNativeExtent(float.NaN, 460f, 100f, 460f), "Unproven geometry fails closed.");
            Assertions.False(TeleportContextLayoutPolicy.RowInsideNativeExtent(100f, 460f, 460f, 100f), "Inverted native extent fails closed.");
        }
    }
}
