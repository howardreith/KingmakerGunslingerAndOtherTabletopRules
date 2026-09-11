using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.ElementalRaces.Visuals;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalVisualResourceRecoveryPolicyTests
    {
        private static ElementalVisualResourceStateSnapshot Snapshot(
            bool objectAlive = true, bool cacheContains = true,
            bool cacheReferencesRegistered = true, bool innerAssetsIntact = true)
        {
            return new ElementalVisualResourceStateSnapshot
            {
                AssetId = "d3adb33f-0000-0000-0000-000000000001",
                Symbol = "Body.Male",
                ObjectAlive = objectAlive,
                CacheContains = cacheContains,
                CacheReferencesRegistered = cacheReferencesRegistered,
                InnerAssetsIntact = innerAssetsIntact
            };
        }

        internal static void OwnedClassificationIsSeverityOrdered()
        {
            Assertions.Equal(ElementalVisualResourceRecoveryPolicy.OwnedResourceDestroyed,
                ElementalVisualResourceRecoveryPolicy.ClassifyOwnedResource(
                    Snapshot(objectAlive: false, cacheContains: false)),
                "A destroyed object outranks eviction in owned classification.");
            Assertions.Equal(ElementalVisualResourceRecoveryPolicy.OwnedResourceDestroyed,
                ElementalVisualResourceRecoveryPolicy.ClassifyOwnedResource(
                    Snapshot(objectAlive: false)),
                "A destroyed owned proxy is classified even while its cache entry survives.");
            Assertions.Equal(ElementalVisualResourceRecoveryPolicy.OwnedResourceEvicted,
                ElementalVisualResourceRecoveryPolicy.ClassifyOwnedResource(
                    Snapshot(cacheContains: false)),
                "A live owned proxy missing from the cache is eviction damage.");
            Assertions.Equal(ElementalVisualResourceRecoveryPolicy.OwnedResourceReplaced,
                ElementalVisualResourceRecoveryPolicy.ClassifyOwnedResource(
                    Snapshot(cacheReferencesRegistered: false)),
                "A foreign object under the owned GUID is replacement damage.");
            Assertions.Equal(ElementalVisualResourceRecoveryPolicy.OwnedInnerAssetsDestroyed,
                ElementalVisualResourceRecoveryPolicy.ClassifyOwnedResource(
                    Snapshot(innerAssetsIntact: false)),
                "Destroyed shared inner assets are reported for a cached live proxy.");
            Assertions.Equal((string)null,
                ElementalVisualResourceRecoveryPolicy.ClassifyOwnedResource(
                    Snapshot()),
                "A healthy owned proxy classifies as null.");
            Assertions.Equal((string)null,
                ElementalVisualResourceRecoveryPolicy.ClassifyOwnedResource(
                    new ElementalVisualResourceStateSnapshot()),
                "A snapshot without identity is never damage.");
        }

        internal static void NativeDependencyClassificationIsSeverityOrdered()
        {
            Assertions.Equal(ElementalVisualResourceRecoveryPolicy.NativeDependencyEvicted,
                ElementalVisualResourceRecoveryPolicy.ClassifyNativeDependency(
                    Snapshot(objectAlive: false, cacheContains: false)),
                "A missing cache entry outranks object death for native dependencies.");
            Assertions.Equal(ElementalVisualResourceRecoveryPolicy.NativeDependencyDestroyed,
                ElementalVisualResourceRecoveryPolicy.ClassifyNativeDependency(
                    Snapshot(objectAlive: false)),
                "A dead object under a surviving cache entry is reloadable destruction.");
            Assertions.Equal(ElementalVisualResourceRecoveryPolicy.NativeDependencyReplaced,
                ElementalVisualResourceRecoveryPolicy.ClassifyNativeDependency(
                    Snapshot(cacheReferencesRegistered: false)),
                "A foreign object under the donor GUID is replacement damage.");
            Assertions.Equal(ElementalVisualResourceRecoveryPolicy.NativeInnerAssetsDestroyed,
                ElementalVisualResourceRecoveryPolicy.ClassifyNativeDependency(
                    Snapshot(innerAssetsIntact: false)),
                "Destroyed donor inner assets are reported for a cached live donor.");
            Assertions.Equal(null,
                ElementalVisualResourceRecoveryPolicy.ClassifyNativeDependency(
                    Snapshot()),
                "A healthy native dependency classifies as null.");
            Assertions.Equal(null,
                ElementalVisualResourceRecoveryPolicy.ClassifyNativeDependency(
                    new ElementalVisualResourceStateSnapshot()),
                "A snapshot without identity is never damage.");
        }

        internal static void ForeignReplacementIsTheOnlyUnrecoverableDamage()
        {
            string[] recoverable =
            {
                ElementalVisualResourceRecoveryPolicy.OwnedResourceDestroyed,
                ElementalVisualResourceRecoveryPolicy.OwnedResourceEvicted,
                ElementalVisualResourceRecoveryPolicy.OwnedResourceReplaced,
                ElementalVisualResourceRecoveryPolicy.OwnedInnerAssetsDestroyed,
                ElementalVisualResourceRecoveryPolicy.NativeDependencyDestroyed,
                ElementalVisualResourceRecoveryPolicy.NativeDependencyEvicted,
                ElementalVisualResourceRecoveryPolicy.NativeInnerAssetsDestroyed
            };
            foreach (string kind in recoverable)
                Assertions.True(
                    ElementalVisualResourceRecoveryPolicy.IsRecoverable(kind),
                    "Catalog-provenance reconstruction must cover " + kind + ".");
            Assertions.False(ElementalVisualResourceRecoveryPolicy.IsRecoverable(
                    ElementalVisualResourceRecoveryPolicy.NativeDependencyReplaced),
                "A foreign object legitimately owning a donor GUID must not be displaced.");
            Assertions.False(ElementalVisualResourceRecoveryPolicy.IsRecoverable(
                    "unknown-damage-kind"),
                "An unclassified damage kind fails closed.");
        }

        internal static void DamagedDependencyEntriesAreEvictedBeforeReload()
        {
            Assertions.True(ElementalVisualResourceRecoveryPolicy
                    .ShouldEvictNativeDependencyForReload(
                        ElementalVisualResourceRecoveryPolicy.NativeDependencyDestroyed),
                "A corpse under a surviving cache entry must be evicted or the loader returns it as a hit.");
            Assertions.True(ElementalVisualResourceRecoveryPolicy
                    .ShouldEvictNativeDependencyForReload(
                        ElementalVisualResourceRecoveryPolicy.NativeInnerAssetsDestroyed),
                "A live-but-gutted registered instance must be evicted so the reload is fresh.");
            Assertions.False(ElementalVisualResourceRecoveryPolicy
                    .ShouldEvictNativeDependencyForReload(
                        ElementalVisualResourceRecoveryPolicy.NativeDependencyReplaced),
                "A foreign replacement is never displaced for reload.");
            Assertions.False(ElementalVisualResourceRecoveryPolicy
                    .ShouldEvictNativeDependencyForReload(
                        ElementalVisualResourceRecoveryPolicy.NativeDependencyEvicted),
                "An evicted dependency has no cache entry to remove.");
            Assertions.False(ElementalVisualResourceRecoveryPolicy
                    .ShouldEvictNativeDependencyForReload(null),
                "A healthy dependency needs no eviction.");
            Assertions.True(ElementalVisualResourceRecoveryPolicy.IsNativeDependencyKind(
                    ElementalVisualResourceRecoveryPolicy.NativeDependencyEvicted) &&
                ElementalVisualResourceRecoveryPolicy.IsNativeDependencyKind(
                    ElementalVisualResourceRecoveryPolicy.NativeDependencyReplaced),
                "Every native dependency kind is recognized for heal ordering.");
            Assertions.False(ElementalVisualResourceRecoveryPolicy.IsNativeDependencyKind(
                    ElementalVisualResourceRecoveryPolicy.OwnedInnerAssetsDestroyed),
                "Owned proxy damage never sorts into the dependency-first heal pass.");
        }

        internal static void RetentionExtendsOnlyAfterFullRecovery()
        {
            ElementalVisualResourceDamage remaining =
                new ElementalVisualResourceDamage(
                    "d3adb33f-0000-0000-0000-000000000001", "Body.Male",
                    ElementalVisualResourceRecoveryPolicy.OwnedInnerAssetsDestroyed);
            Assertions.True(ElementalVisualResourceRecoveryPolicy
                    .ShouldExtendRetentionAfterRecovery(null),
                "Retention extends when no damage was assessed.");
            Assertions.True(ElementalVisualResourceRecoveryPolicy
                    .ShouldExtendRetentionAfterRecovery(
                        Enumerable.Empty<ElementalVisualResourceDamage>()),
                "Retention extends when recovery removed every damaged entry.");
            Assertions.False(ElementalVisualResourceRecoveryPolicy
                    .ShouldExtendRetentionAfterRecovery(
                        new[] { remaining }),
                "Retention stays all-or-nothing while any damage remains.");
        }

        internal static void RecoveryAndReportsAreRateGated()
        {
            DateTime now = new DateTime(2026, 9, 11, 12, 0, 0, DateTimeKind.Utc);
            Assertions.False(ElementalVisualResourceRecoveryPolicy
                    .RecoveryRequired(null),
                "An absent damage list requires no recovery.");
            Assertions.False(ElementalVisualResourceRecoveryPolicy
                    .RecoveryRequired(new ElementalVisualResourceDamage[0]),
                "An empty damage list requires no recovery.");
            Assertions.True(ElementalVisualResourceRecoveryPolicy
                    .RecoveryRequired(new[]
                    {
                        new ElementalVisualResourceDamage("a", "a",
                            ElementalVisualResourceRecoveryPolicy.OwnedResourceEvicted)
                    }),
                "Any damaged entry requires recovery.");
            Assertions.True(ElementalVisualResourceRecoveryPolicy.ReportAllowed(
                    now, DateTime.MinValue),
                "The first report after startup is always allowed.");
            Assertions.True(ElementalVisualResourceRecoveryPolicy.ReportAllowed(
                    now, now.AddSeconds(
                        -ElementalVisualResourceRecoveryPolicy.MinimumReportIntervalSeconds)),
                "A report is allowed at exactly the minimum interval.");
            Assertions.False(ElementalVisualResourceRecoveryPolicy.ReportAllowed(
                    now, now.AddSeconds(
                        -(ElementalVisualResourceRecoveryPolicy
                            .MinimumReportIntervalSeconds - 1))),
                "Reports closer than the minimum interval are suppressed.");
        }
    }
}
