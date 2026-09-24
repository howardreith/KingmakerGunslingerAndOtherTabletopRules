using System;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints.Classes;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityModManagerNet;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// First-update owner of the optional Favored Class integration. The host
    /// initializes inside the LoadDictionary postfix chain; this coordinator
    /// runs on the first UMM update afterwards, resolves the exact host
    /// contract, then publishes the complete owned graph in one transaction.
    /// It never calls host code, never repeats the host's class scan and
    /// never mutates menus again once a character-build session could be
    /// open: profile changes are restart-required.
    /// </summary>
    internal static class FavoredClassIntegrationCoordinator
    {
        internal const string Phase = "favored-class";
        internal const int MaximumPendingUpdateRetries = 2;

        private static readonly object Gate = new object();
        private static ModContext _context;
        private static bool _attached;
        private static int _pendingRetries;
        private static FavoredClassPublication _publication;
        private static FavoredClassHostHandles _host;
        private static FavoredClassSettingsResult _settings;

        internal static FavoredClassPublication Publication
        {
            get { lock (Gate) return _publication; }
        }

        internal static FavoredClassHostHandles Host
        {
            get { lock (Gate) return _host; }
        }

        /// <summary>The settings resolution the effective profile came from.</summary>
        internal static FavoredClassSettingsResult Settings
        {
            get { lock (Gate) return _settings; }
        }

        /// <summary>
        /// Restart-required: the optional FavoredClassIntegration.json is read
        /// once per process, by whichever of blueprint registration (the
        /// Mostly Human publication) or first-update attachment asks first,
        /// and never written.
        /// </summary>
        internal static FavoredClassSettingsResult ResolveSettings(string modDirectory)
        {
            lock (Gate)
            {
                if (_settings == null)
                {
                    _settings = FavoredClassSettings.Load(modDirectory);
                    FavoredClassRuntime.ConfigureProfile(_settings.Profile);
                }
                return _settings;
            }
        }

        internal static void AttachFirstUpdate(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            lock (Gate)
            {
                _context = context;
                if (_attached || _publication != null) return;
                _attached = true;
            }
            FavoredClassSettingsResult settings = ResolveSettings(context.ModEntry.Path);
            if (settings.Source == FavoredClassSettingsSource.Invalid)
                context.Logger.Warning(Phase, "settings.invalid", settings.ToString() +
                    ";using=charter-defaults");
            else
                context.Logger.Info(Phase, "settings.loaded", settings.ToString());
            context.ModEntry.OnUpdate += FirstUpdate;
            context.Logger.Info(Phase, "late-publication.attached",
                "phase=first-umm-update-after-LoadDictionary-postfix-chain");
        }

        private static void FirstUpdate(UnityModManager.ModEntry entry, float delta)
        {
            ModContext context;
            lock (Gate) context = _context;
            bool retry = !TryResolveAndPublish("first-update-after-load-dictionary");
            lock (Gate)
            {
                if (retry && _pendingRetries < MaximumPendingUpdateRetries)
                {
                    _pendingRetries++;
                    return;
                }
                _attached = false;
            }
            if (context != null) context.ModEntry.OnUpdate -= FirstUpdate;
        }

        /// <summary>
        /// Resolves and publishes. Returns false only for a pending state
        /// that a later update may resolve (library not yet assigned).
        /// Repeated calls are idempotent.
        /// </summary>
        internal static bool TryResolveAndPublish(string checkpoint)
        {
            ModContext context;
            FavoredClassPublication existing;
            lock (Gate)
            {
                context = _context;
                existing = _publication;
            }
            if (context == null)
                return true;
            FavoredClassPublication publication = null;
            try
            {
                if (existing != null)
                {
                    existing.Validate();
                    context.Logger.Info(Phase, "late-publication.idempotent",
                        "checkpoint=" + checkpoint + ";leaves=" + existing.Surfaces.Count);
                    return true;
                }
                FavoredClassBlueprintSet set = BlueprintBootstrap.FavoredClassLeaves;
                if (set == null)
                {
                    Report(context, new FavoredClassIntegrationStatus(
                        FavoredClassIntegrationAvailability.RegistrationFailed,
                        "Owned favored-class leaves are not registered; no choices are offered.", 0, null),
                        checkpoint);
                    return true;
                }
                FavoredClassProfileState profile = FavoredClassRuntime.Profile;
                if (!profile.IntegrationEnabled)
                {
                    Report(context, new FavoredClassIntegrationStatus(
                        FavoredClassIntegrationAvailability.IntegrationDisabled,
                        "The favored-class integration is disabled; owned identities stay registered for saves.",
                        0, null), checkpoint);
                    return true;
                }
                FavoredClassHostHandles host = FavoredClassHostAdapter.Resolve(context.ModEntry,
                    set.GunslingerClassGuid);
                lock (Gate) _host = host;
                if (!host.Decision.IsReady)
                {
                    bool pending = host.Decision.State == FavoredClassHostState.IncompleteInitialization &&
                        host.Decision.Reason == "library-unassigned";
                    Report(context, new FavoredClassIntegrationStatus(
                        FavoredClassIntegrationStatus.FromHostState(host.Decision.State),
                        host.Decision.ToString(), 0, null), checkpoint);
                    return !pending;
                }
                publication = FavoredClassPublication.Plan(set, host, profile,
                    context.FeatureModules.Active.Gunslinger, null);
                // Ancestry scopes are registered before the publication
                // commits: the exact host human prerequisites of the host's
                // own leaves, for the verified Mostly Human permission only.
                BlueprintRace human = BlueprintLibraryLookup.RequireExact<BlueprintRace>(
                    BlueprintBootstrap.Library, FavoredClassRaceIdentities.ForAncestry(
                        FavoredClassAncestry.Human).RaceGuid, "native Human race");
                int bridged = Hooks.FavoredClassHostRaceBridge.Prepare(context.Harmony, host, human);
                context.Logger.Info(Phase, "ancestry-bridge.scoped",
                    "trackedHumanPrerequisites=" + bridged + ";permission=mostly-human-geniekin-only");
                publication.Commit();
                lock (Gate) _publication = publication;
                Report(context, new FavoredClassIntegrationStatus(
                    FavoredClassIntegrationAvailability.Published,
                    "host=" + host.Decision + ";gunslinger=" + host.GunslingerDecision +
                    ";evidence=" + string.Join("|", publication.Evidence.ToArray()),
                    publication.Surfaces.Count, publication.Skipped), checkpoint);
                return true;
            }
            catch (Exception exception)
            {
                Hooks.FavoredClassHostRaceBridge.Clear();
                if (publication != null && publication.IsCommitted)
                    try { publication.Rollback(); }
                    catch (Exception rollbackException)
                    {
                        exception = new AggregateException(exception, rollbackException);
                    }
                lock (Gate)
                    if (ReferenceEquals(_publication, publication))
                        _publication = null;
                FavoredClassIntegrationStatusRegistry.Update(new FavoredClassIntegrationStatus(
                    FavoredClassIntegrationAvailability.PublicationFailed,
                    exception.GetType().Name + ": " + exception.Message, 0, null));
                context.Logger.Failure(Phase, "late-publication.blocked",
                    "checkpoint=" + checkpoint +
                    ";the favored-class integration failed closed and rolled back; unrelated KMG modules remain active.",
                    exception);
                return true;
            }
        }

        /// <summary>
        /// Guarded runtime qualification only (main menu, no build session):
        /// after the qualification scenario deliberately rolled back the
        /// committed publication and proved a fault-injected transaction
        /// restores the exact foreign graph, it re-publishes with a fresh,
        /// validated transaction and hands it back here so later idempotent
        /// checks validate the live graph.
        /// </summary>
        internal static void AdoptQualificationRepublication(FavoredClassPublication publication)
        {
            if (publication == null)
                throw new ArgumentNullException("publication");
            if (!publication.IsCommitted)
                throw new InvalidOperationException("Only a committed publication can be adopted.");
            publication.Validate();
            lock (Gate) _publication = publication;
        }

        private static void Report(ModContext context, FavoredClassIntegrationStatus status,
            string checkpoint)
        {
            if (!FavoredClassIntegrationStatusRegistry.Update(status))
                return;
            string message = "checkpoint=" + checkpoint + ";" + status;
            string eventName = "integration." + status.Availability.ToString().ToLowerInvariant();
            if (status.Availability == FavoredClassIntegrationAvailability.UnsupportedBinary ||
                status.Availability == FavoredClassIntegrationAvailability.HostIncomplete)
                context.Logger.Warning(Phase, eventName, message);
            else
                context.Logger.Info(Phase, eventName, message);
            if (status.Skipped.Any(value => value.Contains("gunslinger-not-ready")))
                context.Logger.Warning(Phase, "gunslinger.not-scanned",
                    "The Favored Class host initialized without a Gunslinger favored-class entry. " +
                    "KMG never repeats the host's class scan; Gunslinger favored-class rewards stay " +
                    "unavailable until KMG's class is registered before the host scan and the game restarts. " +
                    string.Format(CultureInfo.InvariantCulture, "skipped={0}",
                        string.Join(",", status.Skipped.ToArray())));
        }
    }
}
