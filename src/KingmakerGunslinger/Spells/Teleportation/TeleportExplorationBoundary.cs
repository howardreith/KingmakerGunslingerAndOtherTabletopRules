using System;
using System.Globalization;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Saved on the campaign owner. It records a completed magical arrival, not
    // familiarity or exploration. Ordinary walking releases the restriction.
    internal sealed class TeleportExplorationBoundary
    {
        internal readonly string MapId, PointId;
        internal readonly float Miles;
        internal TeleportExplorationBoundary(string mapId, string pointId, float miles)
        {
            if (!TeleportDestinationPolicy.IsStableId(mapId) || !TeleportDestinationPolicy.IsStableId(pointId) ||
                float.IsNaN(miles) || float.IsInfinity(miles) || miles < 0)
                throw new ArgumentException("A stable map/point and finite native mileage are required.");
            MapId = mapId; PointId = pointId; Miles = miles;
        }
        internal bool Suppress(string mapId, string pointId, float miles, bool ordinaryWalking)
        { return !ordinaryWalking && MapId == mapId && PointId == pointId && Miles == miles; }
        internal static bool SuppressSaved(string payload, string mapId, string pointId, float miles, bool ordinaryWalking)
        {
            // A real travel command releases even malformed legacy data. It must
            // never turn this spell boundary into a permanent exploration block.
            if (ordinaryWalking) return false;
            var boundary = Parse(payload);
            return boundary != null && boundary.Suppress(mapId, pointId, miles, false);
        }
        internal string Serialize()
        { return "1|" + MapId + "|" + PointId + "|" + Miles.ToString("R", CultureInfo.InvariantCulture); }
        internal static TeleportExplorationBoundary Parse(string payload)
        {
            if (payload == null) return null;
            string[] parts = payload.Split('|'); float miles;
            if (parts.Length != 4 || parts[0] != "1" ||
                !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out miles))
                throw new FormatException("Unknown magical-arrival exploration boundary.");
            var result = new TeleportExplorationBoundary(parts[1], parts[2], miles);
            if (result.Serialize() != payload) throw new FormatException("Noncanonical magical-arrival exploration boundary.");
            return result;
        }
    }
}
