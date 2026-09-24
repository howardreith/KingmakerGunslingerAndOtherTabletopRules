using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using KingmakerGunslinger.AidAnotherCompatibility;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>One owned leaf and the host selection that receives it.</summary>
    internal sealed class FavoredClassPublicationSurface
    {
        internal FavoredClassPublicationSurface(FavoredClassLeafPair pair,
            BlueprintFeatureSelection selection, BlueprintFeature leaf)
        {
            Pair = pair;
            Selection = selection;
            Leaf = leaf;
        }

        internal FavoredClassLeafPair Pair { get; private set; }
        internal BlueprintFeatureSelection Selection { get; private set; }
        internal BlueprintFeature Leaf { get; private set; }

        internal string Key
        {
            get { return "fcb:" + Selection.AssetGuid + ":" + Leaf.AssetGuid; }
        }
    }

    /// <summary>
    /// Atomic, idempotent publication of KMG-owned leaves into the host's
    /// per-class bonus selections. Every foreign entry and its order is
    /// preserved; leaves are appended by stable identity; a failure restores
    /// only this transaction's own additions while keeping later foreign
    /// appends. Nothing is published for a class the host did not scan, for
    /// a missing class family, or for an effect whose every route is in a
    /// disabled profile.
    /// </summary>
    internal sealed class FavoredClassPublication
    {
        private readonly HelpfulPublicationTransaction _transaction;
        private readonly Dictionary<string, BlueprintFeature[]> _before;

        private FavoredClassPublication(IList<FavoredClassPublicationSurface> surfaces,
            IList<string> skipped, HelpfulPublicationTransaction transaction,
            Dictionary<string, BlueprintFeature[]> before)
        {
            Surfaces = surfaces;
            Skipped = skipped;
            _transaction = transaction;
            _before = before;
        }

        internal IList<FavoredClassPublicationSurface> Surfaces { get; private set; }

        /// <summary>Explicit capability statuses: "effect:reason".</summary>
        internal IList<string> Skipped { get; private set; }

        internal IReadOnlyList<string> Evidence { get { return _transaction.Evidence; } }

        internal bool IsCommitted { get { return _transaction.IsCommitted; } }

        /// <summary>
        /// Plans the publication. <paramref name="injectFailureAt"/> is a
        /// runtime-test fault position (null in production): a surface that
        /// throws on write is placed before that index, so the committed
        /// prefix must roll back exactly.
        /// </summary>
        internal static FavoredClassPublication Plan(FavoredClassBlueprintSet set,
            FavoredClassHostHandles host, FavoredClassProfileState profile,
            bool gunslingerModuleActive, int? injectFailureAt)
        {
            if (set == null) throw new ArgumentNullException("set");
            if (host == null) throw new ArgumentNullException("host");
            if (profile == null) throw new ArgumentNullException("profile");
            if (!host.Decision.IsReady)
                throw new InvalidOperationException("Publication requires a ready host: " +
                    host.Decision);
            var surfaces = new List<FavoredClassPublicationSurface>();
            var skipped = new List<string>();
            foreach (FavoredClassLeafPair pair in set.Pairs)
            {
                string reason = SkipReason(pair, host, profile, gunslingerModuleActive);
                if (reason != null)
                {
                    skipped.Add(pair.Effect.Id + ":" + reason);
                    continue;
                }
                BlueprintFeatureSelection selection = host.BonusSelectionFor(pair.HostClassGuid);
                foreach (BlueprintFeature leaf in pair.Leaves)
                    surfaces.Add(new FavoredClassPublicationSurface(pair, selection, leaf));
            }
            var before = new Dictionary<string, BlueprintFeature[]>(StringComparer.Ordinal);
            foreach (FavoredClassPublicationSurface surface in surfaces)
                if (!before.ContainsKey(surface.Selection.AssetGuid))
                    before[surface.Selection.AssetGuid] = surface.Selection.AllFeatures;
            var transaction = new HelpfulPublicationTransaction();
            for (int index = 0; index < surfaces.Count; index++)
            {
                if (injectFailureAt.HasValue && injectFailureAt.Value == index)
                    AppendFault(transaction, surfaces[index]);
                FavoredClassPublicationSurface surface = surfaces[index];
                BlueprintFeatureSelection selection = surface.Selection;
                transaction.Append(surface.Key, () => selection.AllFeatures,
                    value => selection.AllFeatures = value, surface.Leaf,
                    feature => feature.AssetGuid, true);
            }
            if (injectFailureAt.HasValue && injectFailureAt.Value >= surfaces.Count && surfaces.Count > 0)
                AppendFault(transaction, surfaces[surfaces.Count - 1]);
            return new FavoredClassPublication(surfaces.AsReadOnly(), skipped.AsReadOnly(),
                transaction, before);
        }

        internal void Commit()
        {
            _transaction.Commit();
            Validate();
        }

        internal void Rollback()
        {
            _transaction.Rollback();
        }

        /// <summary>
        /// Exact post-commit validation by reference and identity: every
        /// published leaf appears exactly once, and each foreign prefix is the
        /// original array, entry for entry, in order.
        /// </summary>
        internal void Validate()
        {
            foreach (IGrouping<string, FavoredClassPublicationSurface> group in
                Surfaces.GroupBy(surface => surface.Selection.AssetGuid))
            {
                BlueprintFeatureSelection selection = group.First().Selection;
                BlueprintFeature[] current = selection.AllFeatures ?? new BlueprintFeature[0];
                BlueprintFeature[] original = _before[group.Key] ?? new BlueprintFeature[0];
                if (current.Length < original.Length)
                    throw new InvalidOperationException("Host selection lost entries: " + selection.name);
                for (int index = 0; index < original.Length; index++)
                    if (!ReferenceEquals(current[index], original[index]) &&
                        !OwnedLeaf(group, original[index]))
                        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                            "Host selection {0} foreign entry {1} changed.", selection.name, index));
                foreach (FavoredClassPublicationSurface surface in group)
                {
                    int references = current.Count(value => ReferenceEquals(value, surface.Leaf));
                    int identities = current.Count(value => value != null &&
                        string.Equals(value.AssetGuid, surface.Leaf.AssetGuid, StringComparison.Ordinal));
                    if (references != 1 || identities != 1)
                        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                            "Leaf {0} appears {1} times ({2} identities) in {3}.", surface.Leaf.name,
                            references, identities, selection.name));
                }
            }
        }

        private static bool OwnedLeaf(IEnumerable<FavoredClassPublicationSurface> group,
            BlueprintFeature value)
        {
            return group.Any(surface => ReferenceEquals(surface.Leaf, value));
        }

        private static string SkipReason(FavoredClassLeafPair pair, FavoredClassHostHandles host,
            FavoredClassProfileState profile, bool gunslingerModuleActive)
        {
            if (!profile.IntegrationEnabled)
                return "integration-disabled";
            bool anyRoute = pair.Effect.Rows.Select(FavoredClassCatalog.Row)
                .Any(row => row.IsScheduled && profile.Offers(row.Profile));
            if (!anyRoute)
                return "profile-disabled";
            if (pair.Effect.ClassFamily == FavoredClassCatalog.Gunslinger)
            {
                if (!gunslingerModuleActive)
                    return "gunslinger-module-off";
                if (!host.GunslingerDecision.IsReady)
                    return "gunslinger-not-ready:" + host.GunslingerDecision.Reason;
            }
            if (pair.Effect.Id == FavoredClassCatalog.EffectPerformanceRange && pair.TargetKey != null &&
                !FavoredClassPerformanceManifest.For(pair.TargetKey).Published)
                return "excluded-target:" + pair.TargetKey;
            if (host.BonusSelectionFor(pair.HostClassGuid) == null)
                return "class-not-scanned";
            if (FavoredClassRuntime.IsEffectUnavailable(pair.Effect.Id))
                return "native-contract-unavailable";
            if (FavoredClassRuntime.IsTargetUnavailable(pair.Effect.Id, pair.TargetKey))
                return "native-contract-unavailable:" + pair.TargetKey;
            return null;
        }

        private static void AppendFault(HelpfulPublicationTransaction transaction,
            FavoredClassPublicationSurface surface)
        {
            BlueprintFeatureSelection selection = surface.Selection;
            transaction.Append("fault-injection:" + surface.Key, () => selection.AllFeatures,
                value => { throw new InvalidOperationException("Injected publication fault."); },
                surface.Leaf, feature => feature.AssetGuid, true);
        }
    }
}
