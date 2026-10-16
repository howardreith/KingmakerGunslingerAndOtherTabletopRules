using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityModManagerNet;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Reconciles every alternate summoning caller once per process, on the
    /// first Unity Mod Manager update after the blueprint dictionary has
    /// loaded: by then every mod's <c>LoadDictionary</c> postfix - Call of
    /// the Wild's class construction included - has finished cloning native
    /// summons, so no caller ever copies an adapter name and the result does
    /// not depend on mod load order. Each family is its own transaction: one
    /// that is absent, mismatched or fails keeps its original behavior and
    /// a precise status, and never disturbs another family, the ordinary
    /// spells, or any other module.
    /// </summary>
    internal static class AlternateSummoningCoordinator
    {
        private static readonly object Gate = new object();
        private static readonly Dictionary<string, FamilyState> States =
            new Dictionary<string, FamilyState>(StringComparer.Ordinal);
        private static ModContext _context;
        private static bool _installed;
        private static bool _attached;
        private static bool _reconciling;
        private static int _pendingUpdates;
        private static Stopwatch _waiting;

        /// <summary>Updates on which a family was still waiting for the
        /// package lifecycle; diagnostic only.</summary>
        internal static int PendingUpdates
        { get { lock (Gate) return _pendingUpdates; } }

        internal static void Install(ModContext context)
        {
            if (context == null) return;
            lock (Gate)
            {
                _context = context;
                if (_installed) return;
                _installed = true;
            }
            try
            {
                AlternateSummoningSourceCatalog.Validate();
                foreach (AlternateSummoningFamilySpec family in
                    AlternateSummoningSourceCatalog.All)
                    AlternateSummoningStatusRegistry.Update(Status(family.Key,
                        AlternateSummoningDisposition.Pending,
                        "awaiting-first-update-after-load-dictionary"));
                Attach(context);
            }
            catch (Exception exception)
            {
                context.Logger.Failure("summoning", "alternate-sources.install-failed",
                    "Alternate summoning sources could not be scheduled; every caller keeps its original behavior.",
                    exception);
            }
        }

        private static void Attach(ModContext context)
        {
            lock (Gate)
            {
                if (_attached) return;
                _attached = true;
            }
            context.ModEntry.OnUpdate += OnUpdate;
        }

        private static void Detach(ModContext context)
        {
            lock (Gate)
            {
                if (!_attached) return;
                _attached = false;
            }
            if (context != null) context.ModEntry.OnUpdate -= OnUpdate;
        }

        private static void OnUpdate(UnityModManager.ModEntry entry, float delta)
        {
            ModContext context;
            lock (Gate) context = _context;
            bool settled = TryReconcile("first-update-after-load-dictionary");
            if (settled) { Detach(context); return; }
            lock (Gate)
            {
                _pendingUpdates++;
                if (_waiting == null) _waiting = Stopwatch.StartNew();
                if (_waiting.Elapsed.TotalSeconds <
                    AlternateSummoningLifecyclePolicy.MaximumPendingSeconds)
                    return;
            }
            Detach(context);
            foreach (AlternateSummoningFamilySpec family in
                AlternateSummoningSourceCatalog.All)
            {
                AlternateSummoningFamilyStatus status =
                    AlternateSummoningStatusRegistry.Find(family.Key);
                if (status == null || status.Disposition ==
                    AlternateSummoningDisposition.Pending)
                    Settle(context, family.Key, Status(family.Key,
                        AlternateSummoningDisposition.Blocked,
                        "package-blueprint-lifecycle-not-ready-after-" +
                        AlternateSummoningLifecyclePolicy.MaximumPendingSeconds + "s"));
            }
        }

        /// <summary>Reconciles every unsettled family. Returns true once no
        /// family is waiting. Repeating it never republishes.</summary>
        internal static bool TryReconcile(string checkpoint)
        {
            ModContext context;
            lock (Gate)
            {
                if (_reconciling) return false;
                _reconciling = true;
                context = _context;
            }
            try
            {
                if (context == null) return false;
                LibraryScriptableObject library = BlueprintBootstrap.Library;
                ExpandedSummoningBlueprintSet canonical =
                    ExpandedSummoningBlueprints.Registered;
                bool initialized = BlueprintBootstrap.IsInitialized;
                bool moduleActive = context.FeatureModules.Active.ExpandedSummoning;
                bool canonicalPublished = initialized && library != null &&
                    CanonicalPublished(library);
                DependencyProbe cotw = ProbeCotw(context);
                AlternateSummoningRoster roster = null;
                BlueprintManifest manifest = null;
                bool waiting = false;
                foreach (AlternateSummoningFamilySpec family in
                    AlternateSummoningSourceCatalog.All)
                {
                    lock (Gate)
                        if (States.ContainsKey(family.Key)) continue;
                    AlternateSummoningLifecycleDecision decision =
                        AlternateSummoningLifecyclePolicy.Decide(family.Provider,
                            initialized, BlueprintBootstrap.IsFailed,
                            canonical != null && library != null, cotw.State,
                            moduleActive, canonicalPublished);
                    if (decision.Action == AlternateSummoningLifecycleAction.Wait)
                    {
                        waiting = true;
                        continue;
                    }
                    if (decision.Action == AlternateSummoningLifecycleAction.Absent)
                    {
                        Settle(context, family.Key, Status(family.Key,
                            AlternateSummoningDisposition.AbsentInProfile,
                            decision.Reason));
                        continue;
                    }
                    if (decision.Action == AlternateSummoningLifecycleAction.Block)
                    {
                        Settle(context, family.Key, Status(family.Key,
                            AlternateSummoningDisposition.Blocked, decision.Reason +
                            (family.Provider == AlternateSummoningProvider.CallOfTheWild ?
                                ";" + cotw.Fingerprint : "")));
                        continue;
                    }
                    if (roster == null) roster = AlternateSummoningRoster.Production();
                    if (manifest == null) manifest = LoadManifest(context);
                    Reconcile(context, library, manifest, canonical, roster, family,
                        decision.Publish, checkpoint, family.Provider ==
                            AlternateSummoningProvider.CallOfTheWild ?
                            cotw.Fingerprint : "native");
                }
                return !waiting;
            }
            catch (Exception exception)
            {
                context.Logger.Failure("summoning", "alternate-sources.reconcile-failed",
                    "Alternate summoning reconciliation failed closed at " + checkpoint +
                    "; unsettled callers keep their original behavior.", exception);
                foreach (AlternateSummoningFamilySpec family in
                    AlternateSummoningSourceCatalog.All)
                {
                    bool settled;
                    lock (Gate) settled = States.ContainsKey(family.Key);
                    if (!settled)
                        Settle(context, family.Key, Status(family.Key,
                            AlternateSummoningDisposition.Blocked,
                            "reconcile-exception:" + exception.GetType().Name));
                }
                return true;
            }
            finally
            {
                lock (Gate) _reconciling = false;
            }
        }

        private static void Reconcile(ModContext context,
            LibraryScriptableObject library, BlueprintManifest manifest,
            ExpandedSummoningBlueprintSet canonical, AlternateSummoningRoster roster,
            AlternateSummoningFamilySpec family, bool publish, string checkpoint,
            string fingerprint)
        {
            Stopwatch clock = Stopwatch.StartNew();
            AlternateSummoningFamilyIdentity identity = AlternateSummoningSourceCatalog
                .Identities.Single(value => value.Key == family.Key);
            var registry = new BlueprintRegistry(library, manifest, context.Logger);
            AlternateSummoningFamilyPublication publication = null;
            AlternateSummoningBuiltFamily built = null;
            try
            {
                AlternateSummoningResolvedFamily resolved =
                    AlternateSummoningHostResolver.Resolve(library, family, identity);
                built = AlternateSummoningAdapterBuilder.Build(library, registry,
                    resolved, roster, canonical);
                if (publish)
                    publication = AlternateSummoningFamilyPublication.Publish(
                        resolved, built, roster);
                clock.Stop();
                var status = new AlternateSummoningFamilyStatus(family.Key,
                    publish ? AlternateSummoningDisposition.Published :
                        AlternateSummoningDisposition.RegisteredModuleOff,
                    checkpoint, publication == null ? 0 : publication.RootCount,
                    built.Registered, publication == null ? 0 :
                        publication.ExposedCount,
                    publication == null ? 0 : publication.KeptHost,
                    publication == null ? 0 : publication.RemovedHost,
                    clock.ElapsedMilliseconds, "provider=" + fingerprint +
                        ";hostRoots=" + resolved.Roots.Count);
                lock (Gate) States[family.Key] = new FamilyState {
                    Registry = registry, Built = built, Publication = publication };
                AlternateSummoningStatusRegistry.Update(status);
                context.Logger.Info("summoning", "alternate-sources.reconciled",
                    status.ToString());
            }
            catch (Exception exception)
            {
                string reason = exception is AlternateSummoningResolutionException ?
                    ((AlternateSummoningResolutionException)exception).Check :
                    exception.GetType().Name + ":" + exception.Message;
                try
                {
                    if (publication != null) publication.Rollback();
                    registry.RollbackAll();
                }
                catch (Exception rollbackFailure)
                {
                    reason += ";rollback-refused:" + rollbackFailure.Message;
                    context.Logger.Failure("summoning",
                        "alternate-sources.rollback-refused",
                        "Alternate summoning rollback for " + family.Key +
                        " was refused rather than overwrite another mutation.",
                        rollbackFailure);
                }
                clock.Stop();
                Settle(context, family.Key, new AlternateSummoningFamilyStatus(
                    family.Key, AlternateSummoningDisposition.Blocked, reason, 0, 0,
                    0, 0, 0, clock.ElapsedMilliseconds, "provider=" + fingerprint));
                if (!(exception is AlternateSummoningResolutionException))
                    context.Logger.Failure("summoning", "alternate-sources.family-failed",
                        "Alternate summoning family " + family.Key +
                        " failed closed; its host keeps its original behavior.",
                        exception);
            }
        }

        private static void Settle(ModContext context, string key,
            AlternateSummoningFamilyStatus status)
        {
            lock (Gate) States[key] = new FamilyState();
            AlternateSummoningStatusRegistry.Update(status);
            if (context != null)
                context.Logger.Info("summoning", "alternate-sources.settled",
                    status.ToString());
        }

        private static AlternateSummoningFamilyStatus Status(string key,
            AlternateSummoningDisposition disposition, string reason)
        {
            return new AlternateSummoningFamilyStatus(key, disposition, reason, 0,
                0, 0, 0, 0, 0, null);
        }

        private static bool CanonicalPublished(LibraryScriptableObject library)
        {
            BlueprintScriptableObject found;
            BlueprintAbility probe = library.BlueprintsByAssetId != null &&
                library.BlueprintsByAssetId.TryGetValue(
                    ExpandedSummoningPublisher.CanonicalParentGuids[1], out found) ?
                found as BlueprintAbility : null;
            return ExpandedSummoningPublisher.IsPublishedExpandedParent(probe);
        }

        private static BlueprintManifest LoadManifest(ModContext context)
        {
            string location = context.Assembly == null ? null :
                context.Assembly.Location;
            string directory = string.IsNullOrWhiteSpace(location) ? null :
                Path.GetDirectoryName(location);
            if (string.IsNullOrWhiteSpace(directory))
                throw new InvalidOperationException(
                    "The installed package directory was unavailable for adapter registration.");
            return BlueprintManifest.Load(directory);
        }

        private static DependencyProbe ProbeCotw(ModContext context)
        {
            UnityModManager.ModEntry[] matches = ReadEntries(context.ModEntry)
                .Where(value => value.Info != null && string.Equals(value.Info.Id,
                    AlternateSummoningSourceCatalog.CotwModId,
                    StringComparison.Ordinal)).ToArray();
            if (matches.Length == 0)
                return new DependencyProbe(AlternateSummoningDependencyState.Absent,
                    "cotw=absent");
            if (matches.Length > 1)
                return new DependencyProbe(AlternateSummoningDependencyState.Ambiguous,
                    "cotw=ambiguous:" + matches.Length);
            UnityModManager.ModEntry entry = matches[0];
            Assembly assembly = entry.Assembly;
            string fingerprint = "cotw=" + (entry.Info.Version ?? "?") + ";mvid=" +
                (assembly == null ? "none" : assembly.ManifestModule.ModuleVersionId
                    .ToString("N"));
            bool active = entry.Loaded && entry.Active && entry.HasAssembly &&
                !entry.ErrorOnLoading && assembly != null;
            return new DependencyProbe(active ? AlternateSummoningDependencyState.Active :
                AlternateSummoningDependencyState.Inactive, fingerprint);
        }

        private static UnityModManager.ModEntry[] ReadEntries(
            UnityModManager.ModEntry current)
        {
            Type manager = current == null ? null : current.GetType().DeclaringType;
            FieldInfo field = manager == null ? null : manager.GetField("modEntries",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            IEnumerable values = field == null ? null : field.GetValue(null) as IEnumerable;
            if (values == null) throw new InvalidOperationException(
                "The live UMM modEntries collection was unavailable.");
            return values.Cast<object>().Select(value =>
                value as UnityModManager.ModEntry).Where(value => value != null).ToArray();
        }

        private sealed class DependencyProbe
        {
            internal DependencyProbe(AlternateSummoningDependencyState state,
                string fingerprint)
            { State = state; Fingerprint = fingerprint; }

            internal AlternateSummoningDependencyState State { get; private set; }
            internal string Fingerprint { get; private set; }
        }

        private sealed class FamilyState
        {
            internal BlueprintRegistry Registry;
            internal AlternateSummoningBuiltFamily Built;
            internal AlternateSummoningFamilyPublication Publication;
        }
    }
}
