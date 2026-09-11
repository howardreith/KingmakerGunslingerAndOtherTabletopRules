using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Visual.CharacterSystem;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.ElementalRaces.Visuals
{
    /// <summary>
    /// Reconstructs the exact elemental visual resources that must survive the
    /// proven native destruction boundaries: creator doll-update removal passes
    /// (foreign bundle unloads destroy shared materials) and counter-based
    /// loaded-cache cleanups (untouched entries are evicted and destroyed).
    ///
    /// Reconstruction reuses catalog provenance only: donors and palette
    /// sources are reloaded from their native bundles and validated by object
    /// name, and proxies are re-cloned from the validated donors under their
    /// original stable GUIDs. Ordinary native and foreign resources are never
    /// unloaded, pinned, or displaced by this recovery.
    /// </summary>
    internal static class ElementalVisualResourceRecovery
    {
        private static DateTime _lastReportUtc = DateTime.MinValue;
        private static DateTime _lastAttemptUtc = DateTime.MinValue;

        internal static ElementalVisualResourceRecoveryReport Heal(
            ElementalRaceVisualSet set, ModLogger logger, string boundary)
        {
            if (set == null) throw new ArgumentNullException("set");
            if (logger == null) throw new ArgumentNullException("logger");
            ElementalRaceVisualResourceRegistry registry = set.Registry;
            List<ElementalVisualResourceDamage> damage = registry.AssessDamage();
            var report = new ElementalVisualResourceRecoveryReport
            {
                Boundary = boundary,
                Damaged = damage.Count,
                RecoveredProxies = 0,
                ReboundDependencies = 0,
                RemainingDamage = damage.Count
            };
            if (!ElementalVisualResourceRecoveryPolicy.RecoveryRequired(damage))
                return report;

            // Bounded execution: at most one recovery attempt per interval even
            // when a boundary fires repeatedly (LateUpdate retries, loading).
            DateTime now = DateTime.UtcNow;
            if (!ElementalVisualResourceRecoveryPolicy.ReportAllowed(now, _lastAttemptUtc))
                return report;
            _lastAttemptUtc = now;

            bool reportAllowed = ElementalVisualResourceRecoveryPolicy
                .ReportAllowed(now, _lastReportUtc);

            try
            {
                // Dependencies heal first: a reloaded donor restores the shared
                // bundle assets that proxy reclones must not inherit as corpses.
                foreach (ElementalVisualResourceDamage entry in damage.Where(value =>
                    ElementalVisualResourceRecoveryPolicy.IsNativeDependencyKind(value.Kind)))
                {
                    if (!ElementalVisualResourceRecoveryPolicy.IsRecoverable(entry.Kind))
                        continue;
                    if (HealNativeDependency(registry, entry)) report.ReboundDependencies++;
                }
                foreach (ElementalVisualResourceDamage entry in damage.Where(value =>
                    !ElementalVisualResourceRecoveryPolicy.IsNativeDependencyKind(value.Kind)))
                {
                    if (!ElementalVisualResourceRecoveryPolicy.IsRecoverable(entry.Kind))
                        continue;
                    if (HealOwnedProxy(set, registry, entry)) report.RecoveredProxies++;
                }
            }
            catch (Exception exception)
            {
                if (reportAllowed)
                {
                    _lastReportUtc = now;
                    logger.Failure("elemental-races", "visual-resource.recovery-error",
                        string.Format(CultureInfo.InvariantCulture,
                            "Visual resource recovery faulted at boundary {0}; damage={1}; proxies={2}; dependencies={3}.",
                            boundary, report.Damaged, report.RecoveredProxies,
                            report.ReboundDependencies), exception);
                }
            }

            report.RemainingDamage = registry.AssessDamage().Count;
            if (report.RecoveredProxies != 0 || report.ReboundDependencies != 0 ||
                report.RemainingDamage != 0)
            {
                if (reportAllowed)
                {
                    _lastReportUtc = now;
                    logger.Warning("elemental-races", "visual-resource.recovered",
                        string.Format(CultureInfo.InvariantCulture,
                            "Recovered elemental visual resources at boundary {0}; damaged={1}; proxiesRecloned={2}; dependenciesRebound={3}; remaining={4}.",
                            boundary, report.Damaged, report.RecoveredProxies,
                            report.ReboundDependencies, report.RemainingDamage));
                }
            }
            return report;
        }

        private static bool HealNativeDependency(
            ElementalRaceVisualResourceRegistry registry,
            ElementalVisualResourceDamage entry)
        {
            // A corpse or live-but-gutted registered instance under a surviving
            // cache entry is returned as a cache hit by the native loader, so
            // the damaged entry must be evicted before the reload.
            registry.EvictReloadableNativeDependency(entry.AssetId);
            EquipmentEntity fresh = ResourcesLibrary.TryGetResource<
                EquipmentEntity>(entry.AssetId, true);
            return registry.TryRebindNativeDependency(entry.AssetId, fresh);
        }

        private static bool HealOwnedProxy(ElementalRaceVisualSet set,
            ElementalRaceVisualResourceRegistry registry,
            ElementalVisualResourceDamage entry)
        {
            ElementalRaceVisualResourceRegistration registration = set.Ordered()
                .SelectMany(value => value.Resources)
                .SingleOrDefault(value => string.Equals(value.AssetId,
                    entry.AssetId, StringComparison.Ordinal));
            if (registration == null) return false;
            ElementalRaceVisualProxySpec spec = registration.Spec;
            // Prefer the exact donor family used at construction so a recovered
            // appearance is identical; the alternate asset is the last resort.
            EquipmentEntity donor = ResolveValidated(registration.UsedFallback ?
                spec.Fallback : spec.Donor) ?? ResolveValidated(
                    registration.UsedFallback ? spec.Donor : spec.Fallback);
            if (donor == null) return false;
            List<UnityEngine.Texture2D> palette = null;
            if (spec.UsesSkinPalette)
            {
                ElementalRaceVisualBlueprints owner = set.Ordered().SingleOrDefault(
                    value => value.Resources.Any(resource =>
                        string.Equals(resource.AssetId, entry.AssetId,
                            StringComparison.Ordinal)));
                palette = ElementalRaceVisualFactory.RecreatePalette(
                    owner == null ? null : owner.Definition.SkinPalette);
                if (palette == null) return false;
            }
            EquipmentEntity proxy = ElementalRaceVisualFactory.RecreateProxy(
                spec, donor, palette);
            // A clone inherited from a still-gutted donor is itself damaged;
            // never register a reconstruction that would repeat the defect.
            if (proxy == null || proxy.GetInnerAssets().Any(value => value == null))
                return false;
            registry.ReplaceOwnedRegistration(registration, proxy);
            return true;
        }

        private static EquipmentEntity ResolveValidated(
            ElementalRaceNativeVisualAsset asset)
        {
            if (asset == null) return null;
            EquipmentEntity resource = ResourcesLibrary.TryGetResource<
                EquipmentEntity>(asset.AssetId, true);
            if (resource == null ||
                !string.Equals(resource.name, asset.ExpectedName,
                    StringComparison.Ordinal))
                return null;
            return resource;
        }

        internal static bool ReportSuppressed(DateTime utcNow)
        {
            return !ElementalVisualResourceRecoveryPolicy.ReportAllowed(
                utcNow, _lastReportUtc);
        }

        /// <summary>
        /// Rate-gated isolation report for a creator prefix failure: the native
        /// doll update must still run when the elemental retention state is
        /// unrecoverable, so the damage is reported and swallowed.
        /// </summary>
        internal static void ReportIsolatedCreatorFailure(
            ModLogger logger, Exception exception)
        {
            if (logger == null || exception == null) return;
            DateTime now = DateTime.UtcNow;
            if (ReportSuppressed(now)) return;
            _lastReportUtc = now;
            logger.Warning("elemental-races",
                "visual-resource.creator-isolated",
                "Character creator visual retention was isolated; native creation continues: " +
                exception.Message);
        }
    }

    internal sealed class ElementalVisualResourceRecoveryReport
    {
        internal string Boundary;
        internal int Damaged;
        internal int RecoveredProxies;
        internal int ReboundDependencies;
        internal int RemainingDamage;
    }
}
