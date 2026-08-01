using System;
using System.Globalization;
using System.IO;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Bootstrap
{
    /// <summary>
    /// One-time blueprint lifecycle coordinator. The current milestone registers the hidden
    /// diagnostic feature, Firearm Proficiency, the clone-derived Test Musket, four
    /// item-owned state-token blueprints, and two stackable ammunition items as one
    /// fail-closed transaction.
    /// </summary>
    internal static class BlueprintBootstrap
    {
        internal const int ExpectedRegisteredBlueprintCount = 33;

        private static readonly object Gate = new object();
        private static LibraryScriptableObject _pendingLibrary;
        private static LibraryScriptableObject _library;
        private static BlueprintFeature _diagnosticFeature;
        private static BlueprintFeature _firearmProficiency;
        private static BlueprintAbility _reloadTestMusketAbility;
        private static BlueprintAbility _overhaulTestMusketAbility;
        private static BlueprintAbility _repairTestMusketAbility;
        private static BlueprintItem _firearmRepairKit;
        private static BlueprintWeaponType _testMusketWeaponType;
        private static BlueprintItemWeapon _testMusketItem;
        private static BlueprintWeaponType _nativeHeavyCrossbowWeaponType;
        private static FirearmStateTokenBlueprintSet _firearmStateTokens;
        private static BasicAmmunitionBlueprintSet _basicAmmunition;
        private static ProductionFirearmBlueprintCatalog _productionFirearms;
        private static GunslingerClassBlueprintSet _gunslingerClassBlueprints;
        private static BootstrapState _state = BootstrapState.WaitingForLibrary;
        private static int _observationCount;
        private static int _initializationCount;

        internal static LibraryScriptableObject Library
        {
            get
            {
                lock (Gate)
                {
                    return _library;
                }
            }
        }

        internal static BlueprintFeature DiagnosticFeature
        {
            get
            {
                lock (Gate)
                {
                    return _diagnosticFeature;
                }
            }
        }

        internal static BlueprintFeature FirearmProficiency
        {
            get
            {
                lock (Gate)
                {
                    return _firearmProficiency;
                }
            }
        }

        internal static BlueprintAbility ReloadTestMusketAbility
        {
            get
            {
                lock (Gate)
                {
                    return _reloadTestMusketAbility;
                }
            }
        }

        internal static BlueprintAbility OverhaulTestMusketAbility
        {
            get
            {
                lock (Gate)
                {
                    return _overhaulTestMusketAbility;
                }
            }
        }

        internal static BlueprintAbility RepairTestMusketAbility
        {
            get
            {
                lock (Gate)
                {
                    return _repairTestMusketAbility;
                }
            }
        }

        internal static BlueprintItem FirearmRepairKit
        {
            get
            {
                lock (Gate)
                {
                    return _firearmRepairKit;
                }
            }
        }

        internal static bool IsInitialized
        {
            get
            {
                lock (Gate)
                {
                    return _state == BootstrapState.Initialized;
                }
            }
        }

        internal static BlueprintWeaponType TestMusketWeaponType
        {
            get
            {
                lock (Gate)
                {
                    return _testMusketWeaponType;
                }
            }
        }

        internal static BlueprintItemWeapon TestMusketItem
        {
            get
            {
                lock (Gate)
                {
                    return _testMusketItem;
                }
            }
        }

        internal static BlueprintWeaponType NativeHeavyCrossbowWeaponType
        {
            get
            {
                lock (Gate)
                {
                    return _nativeHeavyCrossbowWeaponType;
                }
            }
        }

        internal static FirearmStateTokenBlueprintSet FirearmStateTokens
        {
            get
            {
                lock (Gate)
                {
                    return _firearmStateTokens;
                }
            }
        }

        internal static BasicAmmunitionBlueprintSet BasicAmmunition
        {
            get
            {
                lock (Gate)
                {
                    return _basicAmmunition;
                }
            }
        }

        internal static ProductionFirearmBlueprintCatalog ProductionFirearms
        {
            get
            {
                lock (Gate)
                {
                    return _productionFirearms;
                }
            }
        }

        internal static GunslingerClassBlueprintSet GunslingerClass
        {
            get { lock (Gate) { return _gunslingerClassBlueprints; } }
        }

        internal static DeadeyeBlueprintSet Deadeye
        {
            get
            {
                lock (Gate)
                {
                    return _gunslingerClassBlueprints == null
                        ? null : _gunslingerClassBlueprints.Deadeye;
                }
            }
        }

        internal static int ObservationCount
        {
            get
            {
                lock (Gate)
                {
                    return _observationCount;
                }
            }
        }

        internal static int InitializationCount
        {
            get
            {
                lock (Gate)
                {
                    return _initializationCount;
                }
            }
        }

        internal static int RegisteredBlueprintCount
        {
            get
            {
                lock (Gate)
                {
                    int count = 0;
                    if (_diagnosticFeature != null)
                    {
                        count++;
                    }

                    if (_firearmProficiency != null)
                    {
                        count++;
                    }

                    if (_reloadTestMusketAbility != null)
                    {
                        count++;
                    }

                    if (_overhaulTestMusketAbility != null)
                    {
                        count++;
                    }

                    if (_repairTestMusketAbility != null)
                    {
                        count++;
                    }

                    if (_firearmRepairKit != null)
                    {
                        count++;
                    }

                    if (_testMusketWeaponType != null)
                    {
                        count++;
                    }

                    if (_testMusketItem != null)
                    {
                        count++;
                    }

                    if (_firearmStateTokens != null)
                    {
                        count += _firearmStateTokens.Count;
                    }

                    if (_basicAmmunition != null)
                    {
                        count += _basicAmmunition.Count;
                    }

                    if (_productionFirearms != null)
                    {
                        count += _productionFirearms.Count;
                    }

                    if (_gunslingerClassBlueprints != null)
                    {
                        count += _gunslingerClassBlueprints.Count;
                    }

                    return count;
                }
            }
        }

        internal static void Observe(LibraryScriptableObject library)
        {
            if (library == null)
            {
                FailWithoutThrowing(new ArgumentNullException("library"));
                return;
            }

            int observationNumber;
            bool duplicateAfterInitialization;
            bool differentLibraryObserved;

            lock (Gate)
            {
                _observationCount++;
                observationNumber = _observationCount;
                duplicateAfterInitialization = _state == BootstrapState.Initialized;
                differentLibraryObserved = _pendingLibrary != null && !ReferenceEquals(_pendingLibrary, library);

                if (_pendingLibrary == null)
                {
                    _pendingLibrary = library;
                    if (_state == BootstrapState.WaitingForLibrary)
                    {
                        _state = BootstrapState.WaitingForContext;
                    }
                }
            }

            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Info(
                    "blueprints",
                    "lifecycle.observed",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "LibraryScriptableObject.LoadDictionary postfix observed; count={0}.",
                        observationNumber));

                if (differentLibraryObserved)
                {
                    context.Logger.Warning(
                        "blueprints",
                        "lifecycle.library-mismatch",
                        "A later LoadDictionary call supplied a different library instance; the first observed instance remains authoritative.");
                }

                if (duplicateAfterInitialization)
                {
                    context.Logger.Warning(
                        "blueprints",
                        "lifecycle.duplicate",
                        "A later LoadDictionary observation was ignored because blueprint initialization already completed.");
                }
            }

            TryInitializePending();
        }

        internal static bool TryInitializePending()
        {
            ModContext context;
            if (!ModContext.TryGet(out context))
            {
                return false;
            }

            LibraryScriptableObject library;
            lock (Gate)
            {
                if (_state == BootstrapState.Initialized)
                {
                    return true;
                }

                if (_state == BootstrapState.Initializing || _state == BootstrapState.Failed)
                {
                    return false;
                }

                if (_pendingLibrary == null || !context.IsReady)
                {
                    return false;
                }

                _state = BootstrapState.Initializing;
                library = _pendingLibrary;
            }

            context.Logger.Info("blueprints", "initialize.start", "Beginning one-time blueprint lifecycle initialization.");

            try
            {
                BlueprintInitializationResult result = InitializeCore(context, library);

                lock (Gate)
                {
                    _library = library;
                    _diagnosticFeature = result.DiagnosticFeature;
                    _firearmProficiency = result.FirearmProficiency;
                    _reloadTestMusketAbility = result.ReloadTestMusketAbility;
                    _overhaulTestMusketAbility = result.OverhaulTestMusketAbility;
                    _repairTestMusketAbility = result.RepairTestMusketAbility;
                    _firearmRepairKit = result.FirearmRepairKit;
                    _testMusketWeaponType = result.TestMusket.WeaponType;
                    _testMusketItem = result.TestMusket.Item;
                    _nativeHeavyCrossbowWeaponType =
                        result.TestMusket.NativeWeaponType;
                    _firearmStateTokens = result.FirearmStateTokens;
                    _basicAmmunition = result.BasicAmmunition;
                    _productionFirearms = result.ProductionFirearms;
                    _gunslingerClassBlueprints = result.GunslingerClassBlueprints;
                    _initializationCount++;
                    _state = BootstrapState.Initialized;
                }

                context.Logger.Info(
                    "blueprints",
                    "initialize.complete",
                    "Blueprint lifecycle initialization completed exactly once; the firearm domain probe passed, twenty custom blueprints were registered transactionally, the production early-firearm catalog and item-token state carrier were configured, basic ammunition and Firearm Repair Kit items were published, and Reload, Overhaul, and Repair were attached to Firearm Proficiency.");
                return true;
            }
            catch (Exception exception)
            {
                lock (Gate)
                {
                    _state = BootstrapState.Failed;
                }

                context.MarkFailed(exception);
                context.Logger.Failure(
                    "blueprints",
                    "initialize.failed",
                    "Blueprint lifecycle initialization failed and will not be retried in this process.",
                    exception);
                return false;
            }
        }

        private static BlueprintInitializationResult InitializeCore(
            ModContext context,
            LibraryScriptableObject library)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (library == null)
            {
                throw new ArgumentNullException("library");
            }

            string assemblyLocation = context.Assembly.Location;
            if (string.IsNullOrWhiteSpace(assemblyLocation))
            {
                throw new InvalidOperationException("The executing mod assembly has no filesystem location.");
            }

            string modDirectory = Path.GetDirectoryName(assemblyLocation);
            if (string.IsNullOrWhiteSpace(modDirectory))
            {
                throw new InvalidOperationException("The installed mod directory could not be resolved from the assembly location.");
            }

            BlueprintManifest manifest = BlueprintManifest.Load(modDirectory);
            context.Logger.Info(
                "blueprints",
                "manifest.loaded",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Loaded {0} validated blueprint entries from {1}.",
                    manifest.Count,
                    BlueprintManifest.RelativeManifestPath));

            FirearmDefinition probeDefinition = FirearmDomainProbe.VerifyMarkerRoundTrip();
            context.Logger.Info(
                "firearms",
                "domain.ready",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Verified immutable firearm definition and BlueprintComponent round-trip: {0}.",
                    probeDefinition));

            BlueprintRegistry registry = new BlueprintRegistry(library, manifest, context.Logger);
            GunslingerClassCatalogPublication classPublication = null;
            try
            {
                BlueprintFeature diagnosticFeature = DiagnosticBlueprints.Register(registry);
                DiagnosticBlueprints.Validate(diagnosticFeature);

                BlueprintFeature firearmProficiency =
                    FirearmProficiencyBlueprints.Register(registry);
                FirearmProficiencyBlueprints.ValidateBase(firearmProficiency);

                TestMusketBlueprintSet testMusket = TestMusketBlueprints.Register(
                    library,
                    registry,
                    context.Logger,
                    firearmProficiency);

                ProductionFirearmBlueprintCatalog productionFirearms =
                    ProductionFirearmBlueprints.Register(
                        library,
                        registry,
                        context.Logger,
                        firearmProficiency);

                FirearmStateTokenBlueprintSet firearmStateTokens =
                    FirearmStateTokenBlueprints.Register(registry, context.Logger);

                BasicAmmunitionBlueprintSet basicAmmunition =
                    BasicAmmunitionBlueprints.Register(
                        library,
                        registry,
                        context.Logger);

                BlueprintItem firearmRepairKit =
                    FirearmRepairKitBlueprints.Register(
                        library,
                        registry,
                        context.Logger);

                BlueprintAbility reloadTestMusketAbility =
                    ReloadTestMusketAbilityBlueprints.Register(
                        registry,
                        context.Logger,
                        testMusket.Item,
                        basicAmmunition.BlackPowder,
                        basicAmmunition.LeadBall);

                BlueprintAbility overhaulTestMusketAbility =
                    OverhaulTestMusketAbilityBlueprints.Register(
                        registry,
                        context.Logger,
                        testMusket.Item,
                        firearmRepairKit);

                BlueprintAbility repairTestMusketAbility =
                    RepairTestMusketAbilityBlueprints.Register(
                        registry,
                        context.Logger,
                        testMusket.Item,
                        firearmRepairKit);

                FirearmProficiencyBlueprints.AttachAbilities(
                    firearmProficiency,
                    reloadTestMusketAbility,
                    overhaulTestMusketAbility,
                    repairTestMusketAbility);

                GunslingerClassBlueprintSet gunslingerClassBlueprints =
                    GunslingerClassBlueprints.Register(
                        library, registry, firearmProficiency,
                        productionFirearms.Pistol.Item,
                        basicAmmunition.BlackPowder,
                        basicAmmunition.LeadBall);
                classPublication = GunslingerClassBlueprints.Publish(
                    gunslingerClassBlueprints.CharacterClass);

                if (registry.RegisteredCount != ExpectedRegisteredBlueprintCount)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The current milestone expected exactly {0} custom registrations, but observed {1}.",
                            ExpectedRegisteredBlueprintCount,
                            registry.RegisteredCount));
                }

                FirearmRuntimeState.Configure(firearmStateTokens);
                context.Logger.Info(
                    "firearms",
                    "persistence.repository-ready",
                    "Configured the item-owned firearm-state token repository. Sprint 19 save/restart evidence passed; broader merchant and compatibility qualification remains ongoing.");

                return new BlueprintInitializationResult(
                    diagnosticFeature,
                    firearmProficiency,
                    reloadTestMusketAbility,
                    overhaulTestMusketAbility,
                    repairTestMusketAbility,
                    firearmRepairKit,
                    testMusket,
                    productionFirearms,
                    firearmStateTokens,
                    basicAmmunition,
                    gunslingerClassBlueprints);
            }
            catch (Exception initializationException)
            {
                if (classPublication != null)
                {
                    try
                    {
                        classPublication.Rollback();
                    }
                    catch (Exception publicationRollbackException)
                    {
                        context.Logger.Failure(
                            "blueprints",
                            "class-catalog.rollback-failed",
                            "Blueprint initialization failed and Gunslinger class catalog rollback was refused.",
                            publicationRollbackException);
                    }
                }
                try
                {
                    registry.RollbackAll();
                }
                catch (Exception rollbackException)
                {
                    context.Logger.Failure(
                        "blueprints",
                        "registry.rollback-failed",
                        "Blueprint initialization failed and the best-effort rollback also encountered an error.",
                        rollbackException);
                }

                throw new InvalidOperationException(
                    "Blueprint initialization failed; any owned registrations were rolled back where possible.",
                    initializationException);
            }
        }

        private static void FailWithoutThrowing(Exception exception)
        {
            lock (Gate)
            {
                _state = BootstrapState.Failed;
            }

            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.MarkFailed(exception);
                context.Logger.Failure(
                    "blueprints",
                    "lifecycle.invalid",
                    "The blueprint lifecycle patch received an invalid library instance.",
                    exception);
            }
        }

        private enum BootstrapState
        {
            WaitingForLibrary = 0,
            WaitingForContext = 1,
            Initializing = 2,
            Initialized = 3,
            Failed = 4
        }

        private sealed class BlueprintInitializationResult
        {
            internal BlueprintInitializationResult(
                BlueprintFeature diagnosticFeature,
                BlueprintFeature firearmProficiency,
                BlueprintAbility reloadTestMusketAbility,
                BlueprintAbility overhaulTestMusketAbility,
                BlueprintAbility repairTestMusketAbility,
                BlueprintItem firearmRepairKit,
                TestMusketBlueprintSet testMusket,
                ProductionFirearmBlueprintCatalog productionFirearms,
                FirearmStateTokenBlueprintSet firearmStateTokens,
                BasicAmmunitionBlueprintSet basicAmmunition,
                GunslingerClassBlueprintSet gunslingerClassBlueprints)
            {
                DiagnosticFeature = diagnosticFeature ?? throw new ArgumentNullException("diagnosticFeature");
                FirearmProficiency = firearmProficiency ?? throw new ArgumentNullException("firearmProficiency");
                ReloadTestMusketAbility = reloadTestMusketAbility ?? throw new ArgumentNullException("reloadTestMusketAbility");
                OverhaulTestMusketAbility = overhaulTestMusketAbility ?? throw new ArgumentNullException("overhaulTestMusketAbility");
                RepairTestMusketAbility = repairTestMusketAbility ?? throw new ArgumentNullException("repairTestMusketAbility");
                FirearmRepairKit = firearmRepairKit ?? throw new ArgumentNullException("firearmRepairKit");
                TestMusket = testMusket ?? throw new ArgumentNullException("testMusket");
                ProductionFirearms = productionFirearms ?? throw new ArgumentNullException("productionFirearms");
                FirearmStateTokens = firearmStateTokens ?? throw new ArgumentNullException("firearmStateTokens");
                BasicAmmunition = basicAmmunition ?? throw new ArgumentNullException("basicAmmunition");
                GunslingerClassBlueprints = gunslingerClassBlueprints ??
                    throw new ArgumentNullException("gunslingerClassBlueprints");
            }

            internal BlueprintFeature DiagnosticFeature { get; private set; }

            internal BlueprintFeature FirearmProficiency { get; private set; }

            internal BlueprintAbility ReloadTestMusketAbility { get; private set; }

            internal BlueprintAbility OverhaulTestMusketAbility { get; private set; }

            internal BlueprintAbility RepairTestMusketAbility { get; private set; }

            internal BlueprintItem FirearmRepairKit { get; private set; }

            internal TestMusketBlueprintSet TestMusket { get; private set; }

            internal ProductionFirearmBlueprintCatalog ProductionFirearms { get; private set; }

            internal FirearmStateTokenBlueprintSet FirearmStateTokens { get; private set; }

            internal BasicAmmunitionBlueprintSet BasicAmmunition { get; private set; }

            internal GunslingerClassBlueprintSet GunslingerClassBlueprints { get; private set; }
        }
    }
}
