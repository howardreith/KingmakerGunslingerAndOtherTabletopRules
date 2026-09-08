using System;
using System.Globalization;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal enum TeleportPointKind { Unknown, Location, HiddenLocation, Landmark, Waypoint, SystemWaypoint }
    [Flags]
    internal enum TeleportDestinationFacts
    {
        None = 0, Persistent = 1, Revealed = 2, Active = 4, Current = 8,
        PlacementSupported = 16, SameGlobalMap = 32, CampaignAllowed = 64,
        Selectable = 128, Transient = 256,
        Required = Persistent | Revealed | Active | Current | PlacementSupported |
            SameGlobalMap | CampaignAllowed | Selectable
    }
    internal enum TeleportDestinationReason
    {
        Eligible, MissingSnapshot, UnstableIdentity, UnknownPointKind, Transient,
        NotPersistent, Unvisited, Unrevealed, Inactive, Removed, MissingAnchor,
        IncompatibleMap, CampaignProhibition, NotSelectable, Forbidden, CurrentPartyPoint,
        MissingOrigin
    }
    internal sealed class TeleportDestinationSnapshot
    {
        internal TeleportDestinationSnapshot(string id, string name, TeleportPointKind kind,
            TeleportDestinationFacts facts, bool nativeVisited, int ordinaryArrivals)
        { Id = id; Name = name; Kind = kind; Facts = facts; NativeVisited = nativeVisited;
            OrdinaryArrivals = ordinaryArrivals; }
        internal string Id { get; private set; }
        internal string Name { get; private set; }
        internal TeleportPointKind Kind { get; private set; }
        internal TeleportDestinationFacts Facts { get; private set; }
        internal bool NativeVisited { get; private set; }
        internal int OrdinaryArrivals { get; private set; }
        internal bool Has(TeleportDestinationFacts fact) { return (Facts & fact) == fact; }
    }
    internal sealed class TeleportDestinationDecision
    {
        internal TeleportDestinationDecision(string id, TeleportDestinationReason reason, string diagnostic)
        { Id = id; Reason = reason; Diagnostic = diagnostic; }
        internal bool Eligible { get { return Reason == TeleportDestinationReason.Eligible; } }
        internal string Id { get; private set; }
        internal TeleportDestinationReason Reason { get; private set; }
        internal string Diagnostic { get; private set; }
    }
    internal static class TeleportDestinationPolicy
    {
        internal static bool IsStableId(string id)
        {
            Guid value;
            return id != null && id.Length == 32 && id == id.ToLowerInvariant() &&
                Guid.TryParseExact(id, "N", out value) && value != Guid.Empty;
        }
        internal static TeleportDestinationDecision Evaluate(TeleportDestinationSnapshot point,
            string originId, TeleportForbiddenDestinationCatalog catalog)
        { return Evaluate(point, originId, catalog, true); }

        // Word of Recall is an exact sanctuary rule. Its caller must also prove
        // the current sanctuary identity; this does not admit Teleport destinations.
        internal static TeleportDestinationDecision EvaluateSafety(TeleportDestinationSnapshot point,
            string originId, TeleportForbiddenDestinationCatalog catalog)
        { return Evaluate(point, originId, catalog, false); }

        private static TeleportDestinationDecision Evaluate(TeleportDestinationSnapshot point,
            string originId, TeleportForbiddenDestinationCatalog catalog, bool requireArrival)
        {
            if (catalog == null) throw new ArgumentNullException("catalog");
            TeleportDestinationReason reason = Reason(point, originId, catalog, requireArrival);
            return new TeleportDestinationDecision(point == null ? null : point.Id, reason,
                "reason=" + reason + ";point=" + (point == null ? "<null>" : point.Id) +
                ";origin=" + originId + ";facts=" + (point == null ? "none" : point.Facts.ToString()) +
                ";nativeVisited=" + (point != null && point.NativeVisited) +
                ";requireArrival=" + requireArrival + ";ordinaryArrivals=" + (point == null ? "0" : point.OrdinaryArrivals.ToString(CultureInfo.InvariantCulture)));
        }
        private static TeleportDestinationReason Reason(TeleportDestinationSnapshot p, string origin,
            TeleportForbiddenDestinationCatalog catalog, bool requireArrival)
        {
            if (p == null) return TeleportDestinationReason.MissingSnapshot;
            if (!IsStableId(p.Id)) return TeleportDestinationReason.UnstableIdentity;
            if (!IsStableId(origin)) return TeleportDestinationReason.MissingOrigin;
            if (!Enum.IsDefined(typeof(TeleportPointKind), p.Kind) || p.Kind == TeleportPointKind.Unknown)
                return TeleportDestinationReason.UnknownPointKind;
            if (p.Has(TeleportDestinationFacts.Transient)) return TeleportDestinationReason.Transient;
            if (!p.Has(TeleportDestinationFacts.Persistent)) return TeleportDestinationReason.NotPersistent;
            // Live reveal/exploration flags are not physical-arrival evidence.
            // One-time legacy inference is persisted before actions are composed.
            if (p.OrdinaryArrivals < 0 || (requireArrival && p.OrdinaryArrivals == 0))
                return TeleportDestinationReason.Unvisited;
            if (!p.Has(TeleportDestinationFacts.Revealed)) return TeleportDestinationReason.Unrevealed;
            if (!p.Has(TeleportDestinationFacts.Active)) return TeleportDestinationReason.Inactive;
            if (!p.Has(TeleportDestinationFacts.Current)) return TeleportDestinationReason.Removed;
            if (!p.Has(TeleportDestinationFacts.PlacementSupported)) return TeleportDestinationReason.MissingAnchor;
            if (!p.Has(TeleportDestinationFacts.SameGlobalMap)) return TeleportDestinationReason.IncompatibleMap;
            if (!p.Has(TeleportDestinationFacts.CampaignAllowed)) return TeleportDestinationReason.CampaignProhibition;
            if (!p.Has(TeleportDestinationFacts.Selectable)) return TeleportDestinationReason.NotSelectable;
            if (catalog.Contains(p.Id)) return TeleportDestinationReason.Forbidden;
            return p.Id == origin ? TeleportDestinationReason.CurrentPartyPoint : TeleportDestinationReason.Eligible;
        }
    }
}
