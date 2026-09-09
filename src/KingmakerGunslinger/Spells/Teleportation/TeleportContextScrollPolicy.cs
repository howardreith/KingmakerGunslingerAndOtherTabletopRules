using System;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Distances are measured down from the content's top, in native canvas units.
    internal static class TeleportContextScrollPolicy
    {
        internal static float Reveal(float contentHeight, float viewportHeight, float offset, float rowTop, float rowBottom)
        {
            foreach (float value in new[] { contentHeight, viewportHeight, offset, rowTop, rowBottom })
                if (float.IsNaN(value) || float.IsInfinity(value)) throw new ArgumentOutOfRangeException("contentHeight");
            if (contentHeight < 0 || viewportHeight <= 0 || rowTop < 0 || rowBottom < rowTop || rowBottom > contentHeight + 0.01f)
                throw new ArgumentOutOfRangeException("rowTop", "Native scroll geometry is invalid.");
            float maximum = Math.Max(0, contentHeight - viewportHeight);
            offset = Math.Max(0, Math.Min(maximum, offset));
            if (rowBottom - rowTop > viewportHeight || rowTop < offset) offset = rowTop;
            else if (rowBottom > offset + viewportHeight) offset = rowBottom - viewportHeight;
            return Math.Max(0, Math.Min(maximum, offset));
        }
    }
}
