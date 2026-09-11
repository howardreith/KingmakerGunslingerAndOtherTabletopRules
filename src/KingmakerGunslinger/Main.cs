using System;
using System.Reflection;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Development;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Compatibility;
using KingmakerGunslinger.FeatureModules;
using KingmakerGunslinger.Spells.ShieldOther;
using KingmakerGunslinger.Summoning;
using KingmakerGunslinger.BrownFur;
using KingmakerGunslinger.AidAnotherCompatibility;
using KingmakerGunslinger.EasternWeapons;
using KingmakerGunslinger.CraftMagicItemsCompatibility;
using UnityModManagerNet;

namespace KingmakerGunslinger
{
    /// <summary>
    /// Unity Mod Manager composition root. The unified maintenance design keeps the
    /// accepted firearm vertical slice and exposes one full-round same-item Repair
    /// Firearm action that restores a Broken or Wrecked firearm directly to Normal
    /// with a reusable Gunsmith's Kit, plus a deterministic process-local
    /// qualification fixture for the complete maintenance loop.
    /// </summary>
    public static class Main
    {
        private static readonly object LoadGate = new object();
        private static LoaderState _state = LoaderState.NotStarted;

        /// <summary>
        /// Unity Mod Manager entry point declared by Info.json.
        /// </summary>
        /// <param name="modEntry">Unity Mod Manager metadata and logger.</param>
        /// <returns>True only when the bootstrap completed successfully.</returns>
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            LoaderState observedState;
            lock (LoadGate)
            {
                observedState = _state;
                if (_state == LoaderState.NotStarted)
                {
                    _state = LoaderState.Loading;
                }
            }

            if (observedState == LoaderState.Loaded)
            {
                LogDuplicateLoad();
                return true;
            }

            if (observedState == LoaderState.Loading)
            {
                TryRawLog(modEntry, "[KMG][bootstrap][load.rejected] A bootstrap load is already in progress.");
                return false;
            }

            if (observedState == LoaderState.Failed)
            {
                TryRawLog(modEntry, "[KMG][bootstrap][load.rejected] A previous bootstrap attempt failed; this process will not retry it.");
                return false;
            }

            ModLogger logger = null;
            ModContext context = null;

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                logger = ModLogger.Create(modEntry, assembly);
                logger.Info("bootstrap", "load.start", "Unity Mod Manager invoked the Kingmaker Gunslinger entry point.");

                context = ModContext.Create(modEntry, assembly, logger);
                ModContext.Publish(context);
                // Commit guarded binary identity before request parsing,
                // patches, blueprint work, UI attachment, or asset loading.
                RuntimeTestRunner.RecordEarlyIdentity(context);
                ElementalNereidQualificationControl.TryActivateEarly(context);
                CompatibilityAttributionRuntimeControl.TryActivateEarly(context);
                if (CompatibilityAttributionRuntimeControl.IsAssetFamilyEnabled(
                    Compatibility.CompatibilityAssetFamily.Firearms))
                    Assets.FirearmAssetRuntime.Configure(context);
                else
                    logger.Info("compatibility-attribution",
                        "asset-family.suppressed",
                        "family=firearms;nativeFallback=true;saveState=false");
                if (CompatibilityAttributionRuntimeControl.IsAssetFamilyEnabled(
                    Compatibility.CompatibilityAssetFamily.ElvenBranchedSpears))
                    Assets.ElvenBranchedSpearAssetRuntime.Configure(context);
                else
                    logger.Info("compatibility-attribution",
                        "asset-family.suppressed",
                        "family=elven-branched-spears;nativeFallback=true;saveState=false");
                if (CompatibilityAttributionRuntimeControl.IsAssetFamilyEnabled(
                    Compatibility.CompatibilityAssetFamily.EasternWeapons))
                    Assets.EasternWeaponAssetRuntime.Configure(context);
                else
                    logger.Info("compatibility-attribution",
                        "asset-family.suppressed",
                        "family=eastern-weapons;nativeFallback=true;saveState=false");
                // Native firearm audio is an optional, fail-soft capability.
                // A missing/invalid bank must never disable firearm mechanics.
                Audio.FirearmSoundRuntime.Configure(context);
                context.InstallPatches();
                Spells.Teleportation.TeleportFamiliarityPatches.Install(context);
                Spells.Teleportation.TeleportExplorationGuardPatches.Install(context);
                Spells.Teleportation.TeleportSpecialistSpellCachePatches.Install(context);
                Spells.Teleportation.TeleportationScrollVendorMigration.Install(context);
                Spells.Teleportation.WorldMapPointSpellActionPatches.Install(context);
                Spells.Teleportation.WorldMapPointConsoleSpellActionPatches.Install(context);
                EasternWeaponArmsArmorCompatibility.Install(context.Harmony);
                BrownFurOptionalExtensionCoordinator.Install(context);
                AidAnotherOptionalExtensionCoordinator.Install(context);

                // A LoadDictionary call observed during PatchAll is retained and processed
                // only after the context reports that patch installation completed.
                BlueprintBootstrap.TryInitializePending();
                if (context.IsFailed)
                {
                    throw new InvalidOperationException(
                        "Blueprint lifecycle initialization failed during bootstrap.",
                        context.Failure);
                }
                CraftMagicItemsOptionalExtensionCoordinator.Install(context);

                logger.Info(
                    "firearms",
                    "state-carrier.configured",
                    "Sprint 19 runtime evidence proved the core item-owned BlueprintWeaponEnchantment state-token carrier across save, exit, restart, and reload. Extended merchant and compatibility qualification remains pending.");

                FeatureModuleUi.Attach(modEntry, context.FeatureModules);
                logger.Info(
                    "development",
                    "ui.attached",
                    "Attached controls for proficiency, item-token persistence, ammunition, condition-preserving full-round reload, player-facing unified full-round Broken-or-Wrecked-to-Normal Repair Firearm with a reusable Gunsmith's Kit, the accelerated maintenance qualification fixture and PASS/FAIL matrix, loaded-round attack enforcement, natural-roll misfire and native burst diagnostics, two-step destructive cleanup confirmation, weapon-only token reconciliation, and disabled-by-default firearm combat tracing.");

                lock (LoadGate)
                {
                    _state = LoaderState.Loaded;
                }

                ClassCatalogDiagnostics.AttachFirstUpdate(context);
                ShieldOtherFinalLiveReconciler.AttachFirstUpdate(context);
                Spells.Teleportation.TeleportationFinalLiveReconciler.AttachFirstUpdate(context);
                EasternWeaponLatePublicationCoordinator.AttachFirstUpdate(
                    context);
                ExpandedSummoningAlignmentModeRuntime.Attach(context);
                RuntimeTestRunner.TryAttach(context);
                logger.Info("bootstrap", "load.complete", "Lifecycle bootstrap completed; the unified full-round Repair Firearm ability restores exactly one equipped Broken or Wrecked project firearm straight to Normal while retaining the accepted firearm attack, reload, persistence, natural-d20 misfire, condition, and native 5-foot burst paths. Repair requires one reusable Gunsmith's Kit in the shared inventory, consumes nothing, preserves surviving loaded rounds and the exact runtime item and item-owned token identity, and advances state exactly once. The hidden legacy Overhaul ability identity delegates to this same repair for save compatibility. The process-local qualification fixture prepares a second independent Test Musket and required resources, then reports concise identity, resource, counter, fault, duplicate, and second-item PASS/FAIL evidence through Repair and Reload. Cancellation before delivery, a missing Gunsmith's Kit, invalid states, ambiguous equipped targets, native Heavy Crossbows, and unrelated firearms remain fail-closed.");
                return true;
            }
            catch (Exception exception)
            {
                try
                {
                    if (modEntry != null)
                    {
                        modEntry.OnGUI = null;
                        modEntry.OnSaveGUI = null;
                    }
                }
                catch
                {
                    // Cleanup must not conceal the original bootstrap exception.
                }

                if (context != null)
                {
                    context.MarkFailed(exception);
                }

                if (logger != null)
                {
                    logger.Failure("bootstrap", "load.failed", "Lifecycle bootstrap failed and content initialization remains disabled.", exception);
                }
                else
                {
                    TryRawLog(modEntry, "[KMG][bootstrap][load.failed] Lifecycle bootstrap failed before structured logging was available: " + exception);
                }

                lock (LoadGate)
                {
                    _state = LoaderState.Failed;
                }

                return false;
            }
        }

        private static void LogDuplicateLoad()
        {
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Warning("bootstrap", "load.duplicate", "A duplicate Unity Mod Manager load call was ignored; Harmony patches were not installed again.");
            }
        }

        private static void TryRawLog(UnityModManager.ModEntry modEntry, string message)
        {
            try
            {
                if (modEntry != null && modEntry.Logger != null)
                {
                    modEntry.Logger.Log(message);
                }
            }
            catch
            {
                // Logging must never turn a bootstrap failure into a second failure.
            }
        }

        private enum LoaderState
        {
            NotStarted = 0,
            Loading = 1,
            Loaded = 2,
            Failed = 3
        }
    }
}
