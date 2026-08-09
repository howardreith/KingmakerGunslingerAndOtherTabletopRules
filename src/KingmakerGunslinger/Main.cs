using System;
using System.Reflection;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Development;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Compatibility;
using KingmakerGunslinger.FeatureModules;
using UnityModManagerNet;

namespace KingmakerGunslinger
{
    /// <summary>
    /// Unity Mod Manager composition root. Sprint 29 retains the accepted firearm
    /// vertical slice and player-facing Wrecked-to-Broken Overhaul, adds a separate
    /// full-round same-item Broken-to-Normal Repair action, and exposes a deterministic
    /// process-local qualification fixture for the complete maintenance loop.
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
                Assets.FirearmAssetRuntime.Configure(context);
                // Native firearm audio is an optional, fail-soft capability.
                // A missing/invalid bank must never disable firearm mechanics.
                Audio.FirearmSoundRuntime.Configure(context);
                // Commit guarded binary identity before patches, blueprint work,
                // UI attachment, or runtime-request parsing.
                RuntimeTestRunner.RecordEarlyIdentity(context);
                context.InstallPatches();

                // A LoadDictionary call observed during PatchAll is retained and processed
                // only after the context reports that patch installation completed.
                BlueprintBootstrap.TryInitializePending();
                if (context.IsFailed)
                {
                    throw new InvalidOperationException(
                        "Blueprint lifecycle initialization failed during bootstrap.",
                        context.Failure);
                }

                logger.Info(
                    "firearms",
                    "state-carrier.configured",
                    "Sprint 19 runtime evidence proved the core item-owned BlueprintWeaponEnchantment state-token carrier across save, exit, restart, and reload. Extended merchant and compatibility qualification remains pending.");

                FeatureModuleUi.Attach(modEntry, context.FeatureModules);
                logger.Info(
                    "development",
                    "ui.attached",
                    "Attached Sprint 29 controls for proficiency, item-token persistence, ammunition, condition-preserving full-round reload, player-facing full-round Wrecked-to-Broken Overhaul and Broken-to-Normal Repair with Firearm Repair Kits, the accelerated maintenance qualification fixture and PASS/FAIL matrix, loaded-round attack enforcement, natural-roll misfire and native burst diagnostics, two-step destructive cleanup confirmation, weapon-only token reconciliation, and disabled-by-default firearm combat tracing.");

                lock (LoadGate)
                {
                    _state = LoaderState.Loaded;
                }

                ClassCatalogDiagnostics.AttachFirstUpdate(context);
                RuntimeTestRunner.TryAttach(context);
                logger.Info("bootstrap", "load.complete", "Lifecycle bootstrap completed; Sprint 29 registered the full-round Repair Test Musket ability and completed the staged same-item maintenance loop while retaining the accepted firearm attack, reload, persistence, natural-d20 misfire, condition, native 5-foot burst, and Overhaul paths. Overhaul changes exactly one equipped empty/Wrecked Test Musket to empty/Broken and Repair changes exactly one equipped empty/Broken Test Musket to empty/Normal; each consumes exactly one Firearm Repair Kit only during completed delivery, preserves the exact runtime item and item-owned token identity, and advances state exactly once. The process-local qualification fixture prepares a second independent Test Musket and required resources, then reports concise identity, resource, counter, fault, duplicate, and second-item PASS/FAIL evidence through Overhaul, Repair, and Reload. Cancellation before delivery, missing kits, invalid states, ambiguous equipped targets, native Heavy Crossbows, and unrelated firearms remain fail-closed. Generic definition-driven maintenance, Quick Clear, scatter triple damage, class progression, and production firearm content remain deferred.");
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
