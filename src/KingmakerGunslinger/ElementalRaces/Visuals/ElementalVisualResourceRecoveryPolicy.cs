using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.ElementalRaces.Visuals
{
    /// <summary>
    /// Read-only snapshot of one registered visual resource at a recovery
    /// boundary. Pure data so the classification policy stays testable without
    /// Unity or the native resource cache.
    /// </summary>
    internal sealed class ElementalVisualResourceStateSnapshot
    {
        internal string AssetId;
        internal string Symbol;
        internal bool ObjectAlive;
        internal bool CacheContains;
        internal bool CacheReferencesRegistered;
        internal bool InnerAssetsIntact;
    }

    internal sealed class ElementalVisualResourceDamage
    {
        internal ElementalVisualResourceDamage(string assetId, string symbol, string kind)
        {
            AssetId = assetId;
            Symbol = symbol;
            Kind = kind;
        }

        internal string AssetId { get; private set; }
        internal string Symbol { get; private set; }
        internal string Kind { get; private set; }
    }

    /// <summary>
    /// Pure decisions for elemental visual-resource recovery: how a damaged
    /// entry is classified, whether recovery may run at a boundary, and how
    /// often recovery and its diagnostics may repeat.
    /// </summary>
    internal static class ElementalVisualResourceRecoveryPolicy
    {
        internal const string OwnedResourceDestroyed = "owned-resource-destroyed";
        internal const string OwnedResourceEvicted = "owned-resource-evicted";
        internal const string OwnedResourceReplaced = "owned-resource-replaced";
        internal const string OwnedInnerAssetsDestroyed = "owned-inner-assets-destroyed";
        internal const string NativeDependencyDestroyed = "native-dependency-destroyed";
        internal const string NativeDependencyEvicted = "native-dependency-evicted";
        internal const string NativeDependencyReplaced = "native-dependency-replaced";
        internal const string NativeInnerAssetsDestroyed = "native-dependency-inner-assets-destroyed";

        internal const int MinimumReportIntervalSeconds = 2;

        /// <summary>Classifies one owned proxy registration; null when healthy.</summary>
        internal static string ClassifyOwnedResource(
            ElementalVisualResourceStateSnapshot snapshot)
        {
            if (snapshot == null || string.IsNullOrEmpty(snapshot.AssetId)) return null;
            if (!snapshot.ObjectAlive) return OwnedResourceDestroyed;
            if (!snapshot.CacheContains) return OwnedResourceEvicted;
            if (!snapshot.CacheReferencesRegistered) return OwnedResourceReplaced;
            if (!snapshot.InnerAssetsIntact) return OwnedInnerAssetsDestroyed;
            return null;
        }

        /// <summary>Classifies one bound native dependency; null when healthy.</summary>
        internal static string ClassifyNativeDependency(
            ElementalVisualResourceStateSnapshot snapshot)
        {
            if (snapshot == null || string.IsNullOrEmpty(snapshot.AssetId)) return null;
            // A dependency object may be destroyed by the native unused-asset
            // sweep while its cache entry survives with a dead reference.
            if (!snapshot.CacheContains) return NativeDependencyEvicted;
            if (!snapshot.ObjectAlive) return NativeDependencyDestroyed;
            if (!snapshot.CacheReferencesRegistered) return NativeDependencyReplaced;
            if (!snapshot.InnerAssetsIntact) return NativeInnerAssetsDestroyed;
            return null;
        }

        internal static bool RecoveryRequired(
            IEnumerable<ElementalVisualResourceDamage> damage)
        {
            return damage != null && damage.Any();
        }

        /// <summary>
        /// A damage kind is recoverable when the exact resource can be rebuilt
        /// from catalog provenance (reloaded donor, fresh proxy clone) without
        /// displacing a foreign object.
        /// </summary>
        internal static bool IsRecoverable(string kind)
        {
            switch (kind)
            {
                case OwnedResourceDestroyed:
                case OwnedResourceEvicted:
                case OwnedResourceReplaced:
                case OwnedInnerAssetsDestroyed:
                case NativeDependencyDestroyed:
                case NativeDependencyEvicted:
                case NativeInnerAssetsDestroyed:
                    return true;
                case NativeDependencyReplaced:
                    // Another object legitimately owning that GUID means the
                    // installed content changed; rebinding would be ambiguous.
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>True for the damage kinds reported for native dependencies.</summary>
        internal static bool IsNativeDependencyKind(string kind)
        {
            return kind == NativeDependencyDestroyed ||
                kind == NativeDependencyEvicted ||
                kind == NativeDependencyReplaced ||
                kind == NativeInnerAssetsDestroyed;
        }

        /// <summary>
        /// A damaged native dependency's cache entry must be evicted before the
        /// native loader can supply a fresh instance: a Unity-destroyed corpse
        /// or a live-but-gutted registered instance is returned as a cache hit
        /// otherwise. A foreign replacement is never displaced; an evicted
        /// entry has nothing to remove.
        /// </summary>
        internal static bool ShouldEvictNativeDependencyForReload(string kind)
        {
            return kind == NativeDependencyDestroyed ||
                kind == NativeInnerAssetsDestroyed;
        }

        /// <summary>
        /// The retention plan remains all-or-nothing: when any damaged entry is
        /// not recoverable, retention is skipped for that boundary and the
        /// isolation is reported instead of partially extending the native set.
        /// </summary>
        internal static bool ShouldExtendRetentionAfterRecovery(
            IEnumerable<ElementalVisualResourceDamage> remaining)
        {
            return remaining == null || !remaining.Any();
        }

        /// <summary>Rate gate for repeated recovery attempts and diagnostics.</summary>
        internal static bool ReportAllowed(DateTime utcNow, DateTime lastReportUtc)
        {
            return (utcNow - lastReportUtc).TotalSeconds >= MinimumReportIntervalSeconds;
        }
    }
}
