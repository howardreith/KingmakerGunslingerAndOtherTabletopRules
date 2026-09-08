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
    }
}
