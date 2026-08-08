using System;
using System.Globalization;
using System.IO;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Compatibility;
using KingmakerGunslinger.Gunsmithing;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Reloading;

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
        internal const int ExpectedRegisteredBlueprintCount = 247;

        private static readonly object Gate = new object();
        private static LibraryScriptableObject _pendingLibrary;
        private static LibraryScriptableObject _library;
        private static BlueprintFeature _diagnosticFeature;
        private static BlueprintFeature _firearmProficiency;
        private static FirearmScopedProficiencyBlueprintSet _scopedFirearmProficiencies;
        private static FirearmTrainingBlueprintSet _firearmTraining;
        private static BlueprintAbility _reloadTestMusketAbility;
        private static BlueprintAbility _overhaulTestMusketAbility;
        private static BlueprintAbility _repairTestMusketAbility;
        private static BlueprintItem _firearmRepairKit;
        private static GunsmithingSupplyBlueprintSet _gunsmithingSupplies;
        private static GunsmithingCraftingBlueprintSet _gunsmithingCrafting;
        private static BlueprintWeaponType _testMusketWeaponType;
        private static BlueprintItemWeapon _testMusketItem;
        private static BlueprintWeaponType _nativeHeavyCrossbowWeaponType;
        private static FirearmStateTokenBlueprintSet _firearmStateTokens;
        private static BlueprintWeaponEnchantment _batteredOrigin;
        private static BasicAmmunitionBlueprintSet _basicAmmunition;
        private static PaperCartridgeModeBlueprintSet _paperCartridgeMode;
        private static ProductionFirearmBlueprintCatalog _productionFirearms;
        private static MagicFirearmBlueprintCatalog _magicFirearms;
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

        internal static PaperCartridgeModeBlueprintSet PaperCartridgeMode
        {
            get
            {
                lock (Gate)
                {
                    return _paperCartridgeMode;
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

        internal static FirearmScopedProficiencyBlueprintSet ScopedFirearmProficiencies
        {
            get { lock (Gate) { return _scopedFirearmProficiencies; } }
        }

        internal static FirearmTrainingBlueprintSet FirearmTraining
        {
            get { lock (Gate) { return _firearmTraining; } }
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

        internal static GunsmithingSupplyBlueprintSet GunsmithingSupplies
        {
            get { lock (Gate) return _gunsmithingSupplies; }
        }
        internal static GunsmithingCraftingBlueprintSet GunsmithingCrafting
        {
            get { lock (Gate) return _gunsmithingCrafting; }
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

        internal static BlueprintWeaponEnchantment BatteredOrigin
        {
            get { lock (Gate) { return _batteredOrigin; } }
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

        internal static MagicFirearmBlueprintCatalog MagicFirearms
        {
            get { lock (Gate) { return _magicFirearms; } }
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

                    if (_scopedFirearmProficiencies != null)
                    {
                        count += _scopedFirearmProficiencies.Count;
                    }
                    if (_firearmTraining != null) count += _firearmTraining.Count;

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
                ProjectAssetIcons.Load(context);
                BlueprintInitializationResult result = InitializeCore(context, library);

                lock (Gate)
                {
                    _library = library;
                    _diagnosticFeature = result.DiagnosticFeature;
                    _firearmProficiency = result.FirearmProficiency;
                    _scopedFirearmProficiencies = result.ScopedFirearmProficiencies;
                    _firearmTraining = result.FirearmTraining;
                    _reloadTestMusketAbility = result.ReloadTestMusketAbility;
                    _overhaulTestMusketAbility = result.OverhaulTestMusketAbility;
                    _repairTestMusketAbility = result.RepairTestMusketAbility;
                    _firearmRepairKit = result.FirearmRepairKit;
                    _gunsmithingSupplies = result.GunsmithingSupplies;
                    _gunsmithingCrafting = result.GunsmithingCrafting;
                    _testMusketWeaponType = result.TestMusket.WeaponType;
                    _testMusketItem = result.TestMusket.Item;
                    _nativeHeavyCrossbowWeaponType =
                        result.TestMusket.NativeWeaponType;
                    _firearmStateTokens = result.FirearmStateTokens;
                    _batteredOrigin = result.BatteredOrigin;
                    _basicAmmunition = result.BasicAmmunition;
                    _paperCartridgeMode = result.PaperCartridgeMode;
                    _productionFirearms = result.ProductionFirearms;
                    _magicFirearms = result.MagicFirearms;
                    _gunslingerClassBlueprints = result.GunslingerClassBlueprints;
                    _initializationCount++;
                    _state = BootstrapState.Initialized;
                }

                context.Logger.Info(
                    "blueprints",
                    "initialize.complete",
                    "Blueprint lifecycle initialization completed exactly once; all active custom blueprints were registered transactionally, the production firearm catalog and item-token state carrier were configured, basic ammunition and Firearm Repair Kit items were published, Firearm Proficiency granted Reload, and Gunsmithing granted Overhaul and Repair.");
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
            CapitalVendorPublication capitalVendorPublication = null;
            BeneathStolenLandsVendorPublication btslVendorPublication = null;
            RareFirearmCampaignLootPublication rareFirearmLootPublication = null;
            FirearmFeatCatalogPublication featPublication = null;
            try
            {
                BlueprintFeature diagnosticFeature = DiagnosticBlueprints.Register(registry);
                DiagnosticBlueprints.Validate(diagnosticFeature);

                BlueprintFeature firearmProficiency =
                    FirearmProficiencyBlueprints.Register(registry);
                FirearmProficiencyBlueprints.ValidateBase(firearmProficiency);
                FirearmScopedProficiencyBlueprintSet scopedFirearmProficiencies =
                    FirearmScopedProficiencyBlueprints.Register(registry);
                FirearmTrainingBlueprintSet firearmTraining =
                    FirearmTrainingBlueprints.Register(registry);

                FirearmFeatBlueprintSet firearmFeats =
                    FirearmFeatBlueprints.Register(library, registry,
                        firearmProficiency, scopedFirearmProficiencies);
                featPublication = FirearmFeatBlueprints.Publish(library, firearmFeats);

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
                        firearmProficiency,
                        scopedFirearmProficiencies);

                FirearmStateTokenBlueprintSet firearmStateTokens =
                    FirearmStateTokenBlueprints.Register(registry, context.Logger);
                BlueprintWeaponEnchantment batteredOrigin =
                    BatteredFirearmOriginBlueprints.Register(registry);
                BlueprintWeaponEnchantment seeking =
                    Enchantments.SeekingBlueprints.Register(registry);
                Enchantments.SeekingBlueprints.Validate(seeking);
                BlueprintWeaponEnchantment reliable =
                    Enchantments.ReliableBlueprints.Register(registry);
                Enchantments.ReliableBlueprints.Validate(reliable);
                MagicFirearmBlueprintCatalog magicFirearms =
                    MagicFirearmBlueprints.Register(library, registry,
                        productionFirearms, reliable, seeking, context.Logger);

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

                GunsmithingSupplyBlueprintSet gunsmithingSupplies =
                    GunsmithingSupplyBlueprints.Register(library, registry);

                BlueprintAbility reloadTestMusketAbility =
                    ReloadTestMusketAbilityBlueprints.Register(
                        registry,
                        context.Logger,
                        testMusket.Item,
                        basicAmmunition.BlackPowder,
                        basicAmmunition.LeadBall);

                PaperCartridgeModeBlueprintSet paperCartridgeMode =
                    PaperCartridgeModeBlueprints.Register(registry, basicAmmunition);

                BlueprintAbility overhaulTestMusketAbility =
                    OverhaulTestMusketAbilityBlueprints.Register(
                        registry,
                        context.Logger,
                        testMusket.Item,
                        gunsmithingSupplies.OverhaulKit);

                BlueprintAbility repairTestMusketAbility =
                    RepairTestMusketAbilityBlueprints.Register(
                        registry,
                        context.Logger,
                        testMusket.Item,
                        firearmRepairKit);

                BlueprintAbility scatterShotAbility =
                    ScatterShotBlueprints.Register(library, registry);

                GunsmithingCraftingBlueprintSet gunsmithingCrafting =
                    GunsmithingCraftingBlueprints.Register(registry,
                        basicAmmunition, gunsmithingSupplies.GunsmithKit);

                FirearmProficiencyBlueprints.AttachReload(
                    firearmProficiency,
                    reloadTestMusketAbility,
                    scatterShotAbility,
                    paperCartridgeMode.Ability);
                FirearmScopedProficiencyBlueprints.AttachActions(
                    scopedFirearmProficiencies,
                    reloadTestMusketAbility,
                    scatterShotAbility,
                    paperCartridgeMode.Ability);

                BlueprintFeature gunsmithing = GunsmithingBlueprints.Register(
                    registry, overhaulTestMusketAbility, repairTestMusketAbility,
                    gunsmithingCrafting.Ability);

                GunslingerClassBlueprintSet gunslingerClassBlueprints =
                    GunslingerClassBlueprints.Register(
                        library, registry, firearmProficiency,
                        scopedFirearmProficiencies, gunsmithing,
                        productionFirearms.Pistol.Item,
                        basicAmmunition.BlackPowder,
                        basicAmmunition.LeadBall,
                        gunsmithingSupplies.GunsmithKit);
                gunslingerClassBlueprints.Pistolero =
                    PistoleroBlueprints.Register(registry,
                        gunslingerClassBlueprints, firearmTraining,
                        gunslingerClassBlueprints.Grit.Resource);
                gunslingerClassBlueprints.MusketMaster =
                    MusketMasterBlueprints.Register(registry,
                        gunslingerClassBlueprints, firearmTraining,
                        firearmFeats.RapidReloadChoices[1],
                        gunslingerClassBlueprints.Grit.Resource,
                        productionFirearms.Musket.Item,
                        basicAmmunition.BlackPowder,
                        basicAmmunition.LeadBall,
                        gunsmithingSupplies.GunsmithKit);
                GunslingerStartingFirearmResolver.Configure(
                    gunslingerClassBlueprints.CharacterClass,
                    productionFirearms.Pistol.Item,
                    productionFirearms.Musket.Item,
                    gunslingerClassBlueprints.Pistolero.Archetype,
                    gunslingerClassBlueprints.MusketMaster.Archetype);
                TrueGritBlueprints.ConfigureOwnership(
                    gunslingerClassBlueprints.TrueGrit,
                    gunslingerClassBlueprints.Deadeye.Feature,
                    gunslingerClassBlueprints.Dodge.Feature,
                    gunslingerClassBlueprints.QuickClear.Feature,
                    gunslingerClassBlueprints.Initiative,
                    gunslingerClassBlueprints.PistolWhip.Feature,
                    gunslingerClassBlueprints.UtilityShot.Feature,
                    gunslingerClassBlueprints.DeadShot.Feature,
                    gunslingerClassBlueprints.StartlingShot.Feature,
                    gunslingerClassBlueprints.TargetingArms.Feature,
                    gunslingerClassBlueprints.TargetingHead.Feature,
                    gunslingerClassBlueprints.TargetingTorso.Feature,
                    gunslingerClassBlueprints.TargetingLegs.Feature,
                    gunslingerClassBlueprints.BleedingWound.Feature,
                    gunslingerClassBlueprints.ExpertLoading.Feature,
                    gunslingerClassBlueprints.LightningReload.Feature,
                    gunslingerClassBlueprints.Evasive.Feature,
                    gunslingerClassBlueprints.MenacingShot.Feature,
                    gunslingerClassBlueprints.CheatDeath,
                    gunslingerClassBlueprints.DeathsShot.Feature,
                    gunslingerClassBlueprints.StunningShot.Feature,
                    gunslingerClassBlueprints.MysteriousStranger.FocusedAim,
                    gunslingerClassBlueprints.Pistolero.TwinShotKnockdown,
                    gunslingerClassBlueprints.MusketMaster.SteadyAim,
                    gunslingerClassBlueprints.MusketMaster.FastMusket);
                FastMusketRuntime.Configure(
                    gunslingerClassBlueprints.MusketMaster.FastMusket,
                    gunslingerClassBlueprints.TrueGrit.ChoiceFor(
                        TrueGritDeed.FastMusket));
                ClassCatalogDiagnostics.Capture("after-registration", library,
                    gunslingerClassBlueprints.CharacterClass);
                int gritUiAbilities = Grit.GritAbilityUiIntegration.Apply(
                    library, gunslingerClassBlueprints.Grit.Resource,
                    gunslingerClassBlueprints.Dodge.ProneAbility);
                context.Logger.Info("grit", "ui.shared-resource-ready",
                    "Bound the native shared-grit indicator to " +
                    gritUiAbilities + " deed abilities.");
                ProjectAssetIcons.Apply(gunslingerClassBlueprints, firearmFeats,
                    productionFirearms, magicFirearms, basicAmmunition, firearmRepairKit,
                    gunsmithingSupplies,
                    paperCartridgeMode,
                    reloadTestMusketAbility, repairTestMusketAbility,
                    overhaulTestMusketAbility);
                PlayerFacingPresentation.ApplyArchetypes(
                    gunslingerClassBlueprints.CharacterClass,
                    gunslingerClassBlueprints.CharacterClass.Icon);
                ClassCatalogDiagnostics.Capture("before-publish", library,
                    gunslingerClassBlueprints.CharacterClass);
                classPublication = GunslingerClassBlueprints.Publish(
                    gunslingerClassBlueprints.CharacterClass);
                ClassCatalogDiagnostics.Capture("after-publish", library,
                    gunslingerClassBlueprints.CharacterClass);

                capitalVendorPublication = CapitalVendorBlueprints.Publish(
                    library, productionFirearms, magicFirearms, basicAmmunition,
                    firearmRepairKit, gunsmithingSupplies, context.Logger);
                btslVendorPublication = BeneathStolenLandsVendorBlueprints.Publish(
                    library, productionFirearms, magicFirearms, basicAmmunition,
                    firearmRepairKit, gunsmithingSupplies, context.Logger);
                rareFirearmLootPublication = RareFirearmCampaignLootBlueprints.Publish(
                    library, magicFirearms, context.Logger);
                ProjectAssetIcons.ValidateSupplyPublication(registry,
                    basicAmmunition, firearmRepairKit, gunsmithingSupplies,
                    gunsmithingCrafting, capitalVendorPublication,
                    btslVendorPublication, context.Logger);

                if (registry.RegisteredCount != ExpectedRegisteredBlueprintCount)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The current milestone expected exactly {0} custom registrations, but observed {1}.",
                            ExpectedRegisteredBlueprintCount,
                            registry.RegisteredCount));
                }

                Enchantments.SeekingExactItemResolver.Configure(seeking);
                Enchantments.FirearmMisfireReductionResolver.Configure(reliable);

                FirearmRuntimeState.Configure(firearmStateTokens);
                context.Logger.Info(
                    "firearms",
                    "persistence.repository-ready",
                    "Configured the item-owned firearm-state token repository. Sprint 19 save/restart evidence passed; broader merchant and compatibility qualification remains ongoing.");

                return new BlueprintInitializationResult(
                    diagnosticFeature,
                    firearmProficiency,
                    scopedFirearmProficiencies,
                    firearmTraining,
                    reloadTestMusketAbility,
                    overhaulTestMusketAbility,
                    repairTestMusketAbility,
                    firearmRepairKit,
                    gunsmithingSupplies,
                    gunsmithingCrafting,
                    testMusket,
                    productionFirearms,
                    magicFirearms,
                    firearmStateTokens,
                    batteredOrigin,
                    basicAmmunition,
                    paperCartridgeMode,
                    gunslingerClassBlueprints);
            }
            catch (Exception initializationException)
            {
                context.Logger.Failure(
                    "blueprints",
                    "initialize.root-cause",
                    "Blueprint initialization reached a failing owned operation before rollback.",
                    initializationException);
                try
                {
                    GunslingerStartingFirearmResolver.Rollback();
                    Reloading.FastMusketRuntime.Rollback();
                    Feats.NativeFirearmFeatIntegration.Rollback();
                }
                catch (Exception nativeFeatRollbackException)
                {
                    context.Logger.Failure(
                        "blueprints", "native-firearm-feats.rollback-failed",
                        "Blueprint initialization failed and native firearm feat integration rollback was refused.",
                        nativeFeatRollbackException);
                }
                if (btslVendorPublication != null)
                {
                    try { btslVendorPublication.Rollback(); }
                    catch (Exception vendorRollbackException)
                    {
                        context.Logger.Failure("blueprints",
                            "btsl-vendors.rollback-failed",
                            "Blueprint initialization failed and BTSL vendor rollback was refused.",
                            vendorRollbackException);
                    }
                }
                if (rareFirearmLootPublication != null)
                {
                    try { rareFirearmLootPublication.Rollback(); }
                    catch (Exception lootRollbackException)
                    {
                        context.Logger.Failure("blueprints",
                            "rare-firearm-loot.rollback-failed",
                            "Blueprint initialization failed and rare-firearm fixed-loot rollback was refused.",
                            lootRollbackException);
                    }
                }
                if (capitalVendorPublication != null)
                {
                    try
                    {
                        capitalVendorPublication.Rollback();
                    }
                    catch (Exception vendorRollbackException)
                    {
                        context.Logger.Failure(
                            "blueprints", "capital-vendor.rollback-failed",
                            "Blueprint initialization failed and capital vendor rollback was refused.",
                            vendorRollbackException);
                    }
                }
                if (featPublication != null)
                {
                    try { featPublication.Rollback(); }
                    catch (Exception featRollbackException)
                    {
                        context.Logger.Failure("blueprints", "firearm-feats.rollback-failed",
                            "Blueprint initialization failed and firearm feat catalog rollback was refused.",
                            featRollbackException);
                    }
                }
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
                FirearmScopedProficiencyBlueprintSet scopedFirearmProficiencies,
                FirearmTrainingBlueprintSet firearmTraining,
                BlueprintAbility reloadTestMusketAbility,
                BlueprintAbility overhaulTestMusketAbility,
                BlueprintAbility repairTestMusketAbility,
                BlueprintItem firearmRepairKit,
                GunsmithingSupplyBlueprintSet gunsmithingSupplies,
                GunsmithingCraftingBlueprintSet gunsmithingCrafting,
                TestMusketBlueprintSet testMusket,
                ProductionFirearmBlueprintCatalog productionFirearms,
                MagicFirearmBlueprintCatalog magicFirearms,
                FirearmStateTokenBlueprintSet firearmStateTokens,
                BlueprintWeaponEnchantment batteredOrigin,
                BasicAmmunitionBlueprintSet basicAmmunition,
                PaperCartridgeModeBlueprintSet paperCartridgeMode,
                GunslingerClassBlueprintSet gunslingerClassBlueprints)
            {
                DiagnosticFeature = diagnosticFeature ?? throw new ArgumentNullException("diagnosticFeature");
                FirearmProficiency = firearmProficiency ?? throw new ArgumentNullException("firearmProficiency");
                ScopedFirearmProficiencies = scopedFirearmProficiencies ??
                    throw new ArgumentNullException("scopedFirearmProficiencies");
                FirearmTraining = firearmTraining ??
                    throw new ArgumentNullException("firearmTraining");
                ReloadTestMusketAbility = reloadTestMusketAbility ?? throw new ArgumentNullException("reloadTestMusketAbility");
                OverhaulTestMusketAbility = overhaulTestMusketAbility ?? throw new ArgumentNullException("overhaulTestMusketAbility");
                RepairTestMusketAbility = repairTestMusketAbility ?? throw new ArgumentNullException("repairTestMusketAbility");
                FirearmRepairKit = firearmRepairKit ?? throw new ArgumentNullException("firearmRepairKit");
                GunsmithingSupplies = gunsmithingSupplies ?? throw new ArgumentNullException("gunsmithingSupplies");
                GunsmithingCrafting = gunsmithingCrafting ?? throw new ArgumentNullException("gunsmithingCrafting");
                TestMusket = testMusket ?? throw new ArgumentNullException("testMusket");
                ProductionFirearms = productionFirearms ?? throw new ArgumentNullException("productionFirearms");
                MagicFirearms = magicFirearms ?? throw new ArgumentNullException("magicFirearms");
                FirearmStateTokens = firearmStateTokens ?? throw new ArgumentNullException("firearmStateTokens");
                BatteredOrigin = batteredOrigin ?? throw new ArgumentNullException("batteredOrigin");
                BasicAmmunition = basicAmmunition ?? throw new ArgumentNullException("basicAmmunition");
                PaperCartridgeMode = paperCartridgeMode ?? throw new ArgumentNullException("paperCartridgeMode");
                GunslingerClassBlueprints = gunslingerClassBlueprints ??
                    throw new ArgumentNullException("gunslingerClassBlueprints");
            }

            internal BlueprintFeature DiagnosticFeature { get; private set; }

            internal BlueprintFeature FirearmProficiency { get; private set; }

            internal FirearmScopedProficiencyBlueprintSet ScopedFirearmProficiencies
            { get; private set; }

            internal FirearmTrainingBlueprintSet FirearmTraining
            { get; private set; }

            internal BlueprintAbility ReloadTestMusketAbility { get; private set; }

            internal BlueprintAbility OverhaulTestMusketAbility { get; private set; }

            internal BlueprintAbility RepairTestMusketAbility { get; private set; }

            internal BlueprintItem FirearmRepairKit { get; private set; }
            internal GunsmithingSupplyBlueprintSet GunsmithingSupplies { get; private set; }
            internal GunsmithingCraftingBlueprintSet GunsmithingCrafting { get; private set; }

            internal TestMusketBlueprintSet TestMusket { get; private set; }

            internal ProductionFirearmBlueprintCatalog ProductionFirearms { get; private set; }

            internal MagicFirearmBlueprintCatalog MagicFirearms { get; private set; }

            internal FirearmStateTokenBlueprintSet FirearmStateTokens { get; private set; }

            internal BlueprintWeaponEnchantment BatteredOrigin { get; private set; }

            internal BasicAmmunitionBlueprintSet BasicAmmunition { get; private set; }

            internal PaperCartridgeModeBlueprintSet PaperCartridgeMode { get; private set; }

            internal GunslingerClassBlueprintSet GunslingerClassBlueprints { get; private set; }
        }
    }
}
