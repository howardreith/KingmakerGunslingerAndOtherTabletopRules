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
        private static FavoredClassAuraPublication _aura;

        internal static FavoredClassPublication Publication
        {
            get { lock (Gate) return _publication; }
        }

        internal static FavoredClassHostHandles Host
        {
            get { lock (Gate) return _host; }
        }

        /// <summary>O06's native aura read point, when committed.</summary>
        internal static FavoredClassAuraPublication Aura
        {
            get { lock (Gate) return _aura; }
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
            CompleteMostlyHumanIcons(context);
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
                // Only a committed publication of the exact host activates the
                // owned numerical effects; every other outcome leaves them off.
                FavoredClassRuntime.DeactivateHost("resolving");
                FavoredClassBlueprintSet set = BlueprintBootstrap.FavoredClassLeaves;
                if (set == null)
                {
                    FavoredClassRuntime.DeactivateHost("registration-failed");
                    Report(context, new FavoredClassIntegrationStatus(
                        FavoredClassIntegrationAvailability.RegistrationFailed,
                        "Owned favored-class leaves are not registered; no choices are offered.", 0, null),
                        checkpoint);
                    return true;
                }
                FavoredClassProfileState profile = FavoredClassRuntime.Profile;
                FavoredClassHostHandles host = FavoredClassHostAdapter.Resolve(context.ModEntry,
                    set.GunslingerClassGuid);
                lock (Gate) _host = host;
                if (!host.Decision.IsReady)
                {
                    FavoredClassRuntime.DeactivateHost("host-" + host.Decision.State);
                    bool pending = host.Decision.State == FavoredClassHostState.IncompleteInitialization &&
                        host.Decision.Reason == "library-unassigned";
                    Report(context, new FavoredClassIntegrationStatus(
                        profile.IntegrationEnabled
                            ? FavoredClassIntegrationStatus.FromHostState(host.Decision.State)
                            : FavoredClassIntegrationAvailability.IntegrationDisabled,
                        profile.IntegrationEnabled ? host.Decision.ToString() :
                            "The favored-class integration is disabled; owned identities stay registered for saves.",
                        0, null), checkpoint);
                    return !pending;
                }
                // The Mostly Human racial trait's human access is part of the
                // trait, not of the favored-class integration: its scope is
                // prepared whenever the exact host is ready.
                PrepareAncestryBridge(context, host);
                if (!profile.IntegrationEnabled)
                {
                    FavoredClassRuntime.DeactivateHost("integration-disabled");
                    Report(context, new FavoredClassIntegrationStatus(
                        FavoredClassIntegrationAvailability.IntegrationDisabled,
                        "The favored-class integration is disabled; owned identities stay registered for saves.",
                        0, null), checkpoint);
                    return true;
                }
                // O06's native read point is validated before planning: a
                // drifted aura contract withholds only that counter.
                string auraProblem = null;
                if (set.Pair(FavoredClassCatalog.EffectPaladinAuras, null) != null)
                    try { FavoredClassAuraPublication.Check(BlueprintBootstrap.Library); }
                    catch (Exception auraException) { auraProblem = auraException.Message; }
                var unavailable = new System.Collections.Generic.List<string>();
                if (auraProblem != null)
                {
                    unavailable.Add(FavoredClassCatalog.EffectPaladinAuras);
                    context.Logger.Warning(Phase, "native-contract.unavailable",
                        "effect=" + FavoredClassCatalog.EffectPaladinAuras + ";" + auraProblem);
                }
                // I06/S04 read points are scoped from the live provider graph;
                // a revelation without any found read point is withheld.
                if (set.Pairs.Any(pair => pair.Effect.Id == FavoredClassCatalog.EffectSelectedRevelation))
                {
                    try
                    {
                        System.Collections.Generic.IList<string> withheld =
                            FavoredClassRevelationScopes.Build(BlueprintBootstrap.Library, set);
                        unavailable.AddRange(withheld.Select(key => FavoredClassRuntime.TargetKey(
                            FavoredClassCatalog.EffectSelectedRevelation, key)));
                        context.Logger.Info(Phase, "revelation-read-points.scoped", string.Format(
                            CultureInfo.InvariantCulture, "targets={0};scoped={1};withheld={2}",
                            FavoredClassRevelationManifest.All.Count,
                            FavoredClassRevelationScopes.All.Count(scope => scope.HasReadPoints),
                            string.Join(",", withheld.ToArray())));
                    }
                    catch (Exception revelationException)
                    {
                        FavoredClassRevelationScopes.Clear();
                        unavailable.Add(FavoredClassCatalog.EffectSelectedRevelation);
                        context.Logger.Warning(Phase, "native-contract.unavailable",
                            "effect=" + FavoredClassCatalog.EffectSelectedRevelation + ";" +
                            revelationException.Message);
                    }
                }
                // Every published choice has a real icon: provider donors are
                // resolved now; a counter still without one is withheld.
                var iconEvidence = new System.Collections.Generic.List<string>();
                foreach (string withheldIcon in FavoredClassLeafIcons.Complete(BlueprintBootstrap.Library, set,
                    iconEvidence))
                    unavailable.Add(withheldIcon);
                context.Logger.Info(Phase, "leaf-icons.completed", string.Format(CultureInfo.InvariantCulture,
                    "assigned={0};withheld={1}", iconEvidence.Count(value => value.StartsWith("icon:",
                        StringComparison.Ordinal)), string.Join(",", iconEvidence.Where(value =>
                        value.StartsWith("icon-missing:", StringComparison.Ordinal)).ToArray())));
                FavoredClassRuntime.SetUnavailableEffects(unavailable);
                publication = FavoredClassPublication.Plan(set, host, profile,
                    context.FeatureModules.Active.Gunslinger, null);
                publication.Commit();
                FavoredClassAuraPublication aura = null;
                if (auraProblem == null && set.AuraStepsProperty != null)
                {
                    aura = FavoredClassAuraPublication.Apply(BlueprintBootstrap.Library, set.AuraStepsProperty);
                    lock (Gate) _aura = aura;
                    context.Logger.Info(Phase, "aura-read-point.committed",
                        string.Join("|", aura.Evidence.ToArray()));
                }
                lock (Gate) _publication = publication;
                FavoredClassRuntime.ActivateHost("published");
                Report(context, new FavoredClassIntegrationStatus(
                    FavoredClassIntegrationAvailability.Published,
                    "host=" + host.Decision + ";gunslinger=" + host.GunslingerDecision +
                    ";evidence=" + string.Join("|", publication.Evidence.ToArray()),
                    publication.Surfaces.Count, publication.Skipped), checkpoint);
                return true;
            }
            catch (Exception exception)
            {
                FavoredClassRuntime.DeactivateHost("publication-failed");
                FavoredClassRevelationScopes.Clear();
                FavoredClassAuraPublication committedAura;
                lock (Gate)
                {
                    committedAura = _aura;
                    _aura = null;
                }
                if (committedAura != null)
                    try { committedAura.Rollback(); }
                    catch (Exception auraRollbackException)
                    {
                        exception = new AggregateException(exception, auraRollbackException);
                    }
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

        private static bool _mostlyHumanIconsChecked;

        /// <summary>
        /// The Mostly Human racial trait's icons are completed after the
        /// project icon stage, with or without the host; if a visible choice
        /// would stay blank, its race-feature publication is rolled back (the
        /// identities stay registered, so saved choices resolve).
        /// </summary>
        private static void CompleteMostlyHumanIcons(ModContext context)
        {
            KingmakerGunslinger.ElementalRaces.ElementalMostlyHumanBlueprintSet set = BlueprintBootstrap.MostlyHuman;
            lock (Gate)
            {
                if (_mostlyHumanIconsChecked || set == null || BlueprintBootstrap.Library == null)
                    return;
                _mostlyHumanIconsChecked = true;
            }
            var evidence = new System.Collections.Generic.List<string>();
            try
            {
                bool complete = KingmakerGunslinger.ElementalRaces.ElementalMostlyHumanIcons.Complete(set,
                    BlueprintBootstrap.Library, evidence);
                if (!complete && set.Publication != null)
                {
                    set.Publication.Rollback();
                    set.Publication = null;
                    context.Logger.Warning("elemental-races", "mostly-human.icons-incomplete",
                        "A Mostly Human choice has no icon; the trait is not offered. " +
                        string.Join("|", evidence.ToArray()));
                    return;
                }
                context.Logger.Info("elemental-races", "mostly-human.icons", string.Join("|", evidence.ToArray()));
            }
            catch (Exception exception)
            {
                if (set.Publication != null)
                    try
                    {
                        set.Publication.Rollback();
                        set.Publication = null;
                    }
                    catch (Exception) { }
                context.Logger.Failure("elemental-races", "mostly-human.icons-failed",
                    "Mostly Human icons could not be completed; the trait is not offered.", exception);
            }
        }

        /// <summary>
        /// Scopes the Mostly Human bridge to every exact host human race
        /// prerequisite (the host's human favored-class leaves and human race
        /// traits). A failure clears only the bridge, so geniekin keep their
        /// native access and nothing is widened.
        /// </summary>
        private static void PrepareAncestryBridge(ModContext context, FavoredClassHostHandles host)
        {
            try
            {
                BlueprintRace human = BlueprintLibraryLookup.RequireExact<BlueprintRace>(
                    BlueprintBootstrap.Library, FavoredClassRaceIdentities.ForAncestry(
                        FavoredClassAncestry.Human).RaceGuid, "native Human race");
                Hooks.FavoredClassBridgeScope scope = Hooks.FavoredClassHostRaceBridge.Prepare(context.Harmony,
                    host, human, BlueprintBootstrap.Library);
                context.Logger.Info(Phase, "ancestry-bridge.scoped", string.Format(CultureInfo.InvariantCulture,
                    "favoredClassLeafPrerequisites={0};otherHumanPrerequisites={1}:{2};permission=mostly-human-geniekin-only",
                    scope.FavoredClassLeafPrerequisites, scope.OtherHumanPrerequisites.Count,
                    string.Join(",", scope.OtherHumanPrerequisites.ToArray())));
            }
            catch (Exception exception)
            {
                Hooks.FavoredClassHostRaceBridge.Clear();
                context.Logger.Warning(Phase, "ancestry-bridge.unavailable",
                    exception.GetType().Name + ": " + exception.Message);
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
            FavoredClassRuntime.ActivateHost("republished");
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
