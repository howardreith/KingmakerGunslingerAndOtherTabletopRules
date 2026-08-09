using System;
using System.Reflection;
using Harmony12;
using KingmakerGunslinger.FeatureModules;
using UnityModManagerNet;

namespace KingmakerGunslinger.Bootstrap
{
    /// <summary>
    /// Process-lifetime services and bootstrap state shared by Harmony patches.
    /// </summary>
    internal sealed class ModContext
    {
        private static readonly object CurrentGate = new object();
        private static ModContext _current;

        private readonly object _stateGate = new object();
        private ContextState _state;
        private Exception _failure;
        private HarmonyInstance _harmony;

        private ModContext(
            UnityModManager.ModEntry modEntry,
            Assembly assembly,
            ModLogger logger)
        {
            ModEntry = modEntry;
            Assembly = assembly;
            Logger = logger;
            ModId = modEntry.Info.Id;
            _state = ContextState.Created;
        }

        internal UnityModManager.ModEntry ModEntry { get; private set; }

        internal Assembly Assembly { get; private set; }

        internal ModLogger Logger { get; private set; }

        internal string ModId { get; private set; }

        internal FeatureModuleSettingsState FeatureModules { get; private set; }

        internal HarmonyInstance Harmony
        {
            get
            {
                lock (_stateGate)
                {
                    return _harmony;
                }
            }
        }

        internal bool IsReady
        {
            get
            {
                lock (_stateGate)
                {
                    return _state == ContextState.PatchesInstalled;
                }
            }
        }

        internal bool IsFailed
        {
            get
            {
                lock (_stateGate)
                {
                    return _state == ContextState.Failed;
                }
            }
        }

        internal Exception Failure
        {
            get
            {
                lock (_stateGate)
                {
                    return _failure;
                }
            }
        }

        internal static ModContext Create(
            UnityModManager.ModEntry modEntry,
            Assembly assembly,
            ModLogger logger)
        {
            if (modEntry == null)
            {
                throw new ArgumentNullException("modEntry");
            }

            if (modEntry.Info == null || string.IsNullOrWhiteSpace(modEntry.Info.Id))
            {
                throw new InvalidOperationException("Unity Mod Manager did not provide a valid mod ID.");
            }

            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            ModContext context = new ModContext(modEntry, assembly, logger);
            context.FeatureModules = FeatureModuleSettingsStore.Load(modEntry.Path,
                message => logger.Warning("settings", "load.recovered", message));
            logger.Info("settings", "load.complete", string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "schema={0};path={1};source={2};active={3};recovered={4};restartPending={5}",
                FeatureModuleSettingsStore.CurrentSchemaVersion,
                context.FeatureModules.Path, context.FeatureModules.Source,
                context.FeatureModules.Active, context.FeatureModules.Recovered,
                context.FeatureModules.RestartRequired));
            return context;
        }

        internal static void Publish(ModContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            lock (CurrentGate)
            {
                if (_current != null)
                {
                    throw new InvalidOperationException("The process-lifetime mod context has already been published.");
                }

                _current = context;
            }
        }

        internal static bool TryGet(out ModContext context)
        {
            lock (CurrentGate)
            {
                context = _current;
                return context != null;
            }
        }

        internal void InstallPatches()
        {
            lock (_stateGate)
            {
                if (_state != ContextState.Created)
                {
                    throw new InvalidOperationException("Harmony patch installation can only begin from the Created state.");
                }

                _state = ContextState.InstallingPatches;
            }

            Logger.Info("harmony", "patch.start", "Creating the Harmony 1.2 instance and patching the executing assembly.");

            try
            {
                HarmonyInstance harmony = HarmonyInstance.Create(ModId);
                harmony.PatchAll(Assembly);
                Firing.EmptyFirearmAttackCommandPatch.Install(harmony);
                Firing.FreeActionFullAttackReloadPatch.Install(harmony);

                lock (_stateGate)
                {
                    _harmony = harmony;
                    _state = ContextState.PatchesInstalled;
                }

                Logger.Info("harmony", "patch.complete", "Harmony patch installation completed exactly once for this process.");
            }
            catch (Exception exception)
            {
                lock (_stateGate)
                {
                    _failure = exception;
                    _state = ContextState.Failed;
                }

                Logger.Failure("harmony", "patch.failed", "Harmony patch installation failed.", exception);
                throw;
            }
        }

        internal void MarkFailed(Exception exception)
        {
            lock (_stateGate)
            {
                if (_state == ContextState.Failed)
                {
                    return;
                }

                _failure = exception;
                _state = ContextState.Failed;
            }
        }

        private enum ContextState
        {
            Created = 0,
            InstallingPatches = 1,
            PatchesInstalled = 2,
            Failed = 3
        }
    }
}
