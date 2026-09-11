using System;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal static class TeleportContextLayoutPolicy
    {
        // Native point panels try pivots around their point anchor. Reserve enough
        // room for the original body and its spacing above or below that anchor;
        // longer source lists scroll within the remaining measured space.
        internal static float MaximumRowsHeight(float canvasHeight, float anchorY, float nativeHeight, float rowHeight)
        {
            if (!Finite(canvasHeight) || canvasHeight <= 0 || !Finite(anchorY) || anchorY < 0 || anchorY > 1 ||
                !Finite(nativeHeight) || nativeHeight < 0 || !Finite(rowHeight) || rowHeight <= 0)
                throw new ArgumentOutOfRangeException("canvasHeight", "Native destination geometry is not finite or on screen.");
            float available = canvasHeight * Math.Max(anchorY, 1 - anchorY) - nativeHeight - rowHeight * 2;
            if (available < rowHeight) throw new InvalidOperationException("No proven room for a native contextual spell row.");
            return available;
        }
        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        // Rows are sized from the settled native action button, not from the
        // dialog canvas group: the group can be wider than the visible
        // parchment art, which made computed-from-padding rows overflow. The
        // padded dialog bound still caps the choice from above.
        internal static float ActionRowsWidth(float settledNativeActionWidth, float paddedDialogInnerWidth)
        {
            if (!Finite(settledNativeActionWidth) || settledNativeActionWidth <= 0 || !Finite(paddedDialogInnerWidth) || paddedDialogInnerWidth <= 0)
                throw new InvalidOperationException("Native destination content width is unproven.");
            return Math.Min(settledNativeActionWidth, paddedDialogInnerWidth);
        }
        // A rendered row is contained when its world-space horizontal extent
        // stays inside the settled native action extent. Layout may settle with
        // a small sub-pixel drift, never a visible overhang.
        internal static bool RowInsideNativeExtent(float rowMinX, float rowMaxX, float nativeMinX, float nativeMaxX)
        {
            const float tolerance = 0.5f;
            if (!Finite(rowMinX) || !Finite(rowMaxX) || !Finite(nativeMinX) || !Finite(nativeMaxX) || nativeMaxX <= nativeMinX)
                return false;
            return rowMinX >= nativeMinX - tolerance && rowMaxX <= nativeMaxX + tolerance;
        }
        // The viewport wants exactly the laid-out content — every row, group
        // separator, spacing and padding — clamped to the measured maximum.
        // Sizing from the row count alone would clip the separators' height and
        // force unnecessary scrolling.
        internal static float ViewportHeight(float preferredContentHeight, float maximumHeight)
        {
            if (!Finite(preferredContentHeight) || preferredContentHeight <= 0 ||
                !Finite(maximumHeight) || maximumHeight <= 0)
                throw new InvalidOperationException("Appended rows content height is unproven.");
            return Math.Min(preferredContentHeight, maximumHeight);
        }
    }
}
