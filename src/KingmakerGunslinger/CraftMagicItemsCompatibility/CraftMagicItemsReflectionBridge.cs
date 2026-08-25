using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Items;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Gunsmithing;

namespace KingmakerGunslinger.CraftMagicItemsCompatibility
{
    internal sealed class CraftMagicItemsGraphSnapshot
    {
        internal CraftMagicItemsGraphSnapshot(int generation, int itemTypes,
            int firearmCreationBases, int firearmRecognitionIdentities,
            int martialBases, int exoticBases,
            int customFamilyMagicItemTypes,
            int customFamilyRecognitionIdentities,
            int ammunitionRecipes, int reliableRecipes,
            int ordinaryWeaponRecipes, string[] firearmCreationBaseGuids,
            string[] firearmRecognitionGuids, string[] martialBaseGuids,
            string[] exoticBaseGuids,
            string[] customFamilyRecognitionGuids,
            string[] namedUpgradeOnlyGuids)
        {
            Generation = generation;
            ItemTypes = itemTypes;
            FirearmCreationBases = firearmCreationBases;
            FirearmRecognitionIdentities = firearmRecognitionIdentities;
            MartialBases = martialBases;
            ExoticBases = exoticBases;
            CustomFamilyMagicItemTypes = customFamilyMagicItemTypes;
            CustomFamilyRecognitionIdentities =
                customFamilyRecognitionIdentities;
            AmmunitionRecipes = ammunitionRecipes;
            ReliableRecipes = reliableRecipes;
            OrdinaryWeaponRecipes = ordinaryWeaponRecipes;
            FirearmCreationBaseGuids = firearmCreationBaseGuids ??
                new string[0];
            FirearmRecognitionGuids = firearmRecognitionGuids ??
                new string[0];
            MartialBaseGuids = martialBaseGuids ?? new string[0];
            ExoticBaseGuids = exoticBaseGuids ?? new string[0];
            CustomFamilyRecognitionGuids =
                customFamilyRecognitionGuids ?? new string[0];
            NamedUpgradeOnlyGuids = namedUpgradeOnlyGuids ?? new string[0];
        }

        internal int Generation { get; private set; }
        internal int ItemTypes { get; private set; }
        internal int FirearmCreationBases { get; private set; }
        internal int FirearmRecognitionIdentities { get; private set; }
        internal int MartialBases { get; private set; }
        internal int ExoticBases { get; private set; }
        internal int CustomFamilyMagicItemTypes { get; private set; }
        internal int CustomFamilyRecognitionIdentities { get; private set; }
        internal int AmmunitionRecipes { get; private set; }
        internal int ReliableRecipes { get; private set; }
        internal int OrdinaryWeaponRecipes { get; private set; }
        internal string[] FirearmCreationBaseGuids { get; private set; }
        internal string[] FirearmRecognitionGuids { get; private set; }
        internal string[] MartialBaseGuids { get; private set; }
        internal string[] ExoticBaseGuids { get; private set; }
        internal string[] CustomFamilyRecognitionGuids { get; private set; }
        internal string[] NamedUpgradeOnlyGuids { get; private set; }
    }

    internal sealed class CraftMagicItemsAmmunitionUiSnapshot
    {
        internal CraftMagicItemsAmmunitionUiSnapshot(string patchSeam,
            int lowerPanelRenderCount, int ordinaryRouteCount,
            int ordinaryBodyBypassCount, int graphRollbackCount,
            int uiFailureCount, bool deferredFailurePending,
            string[] eventTypes, string[] selectedRecipeGuids,
            string[] routeObservations)
        {
            PatchSeam = patchSeam ?? string.Empty;
            LowerPanelRenderCount = lowerPanelRenderCount;
            OrdinaryRouteCount = ordinaryRouteCount;
            OrdinaryBodyBypassCount = ordinaryBodyBypassCount;
            GraphRollbackCount = graphRollbackCount;
            UiFailureCount = uiFailureCount;
            DeferredFailurePending = deferredFailurePending;
            EventTypes = eventTypes ?? new string[0];
            SelectedRecipeGuids = selectedRecipeGuids ?? new string[0];
            RouteObservations = routeObservations ?? new string[0];
        }

        internal string OuterSelectorOwner { get { return "CraftMagicItems"; } }
        internal string PatchSeam { get; private set; }
        internal int LowerPanelRenderCount { get; private set; }
        internal int OrdinaryRouteCount { get; private set; }
        internal int OrdinaryBodyBypassCount { get; private set; }
        internal int GraphRollbackCount { get; private set; }
        internal int UiFailureCount { get; private set; }
        internal bool DeferredFailurePending { get; private set; }
        internal string[] EventTypes { get; private set; }
        internal string[] SelectedRecipeGuids { get; private set; }
        internal string[] RouteObservations { get; private set; }
        internal string SelectedCategoryIdentity
        {
            get
            {
                return RouteObservations.Length == 0 ? string.Empty :
                    RouteObservations[RouteObservations.Length - 1]
                        .Split(':')[0];
            }
        }
    }

    /// <summary>
    /// The only runtime boundary that reads or writes Craft Magic Items objects.
    /// All inputs and outputs on the KMG side remain normal project/game types.
    /// </summary>
    internal static class CraftMagicItemsReflectionBridge
    {
        internal const string MagicFirearmsIdentity = "KMGMagicFirearms";
        internal const string MundaneFirearmsIdentity =
            "CraftMundaneKMGFirearms";
        internal const string AmmunitionIdentity = "KMGFirearmAmmunition";
        internal const string ReliableRecipeIdentity = "KMGReliable";
        private const string ArmsAndArmorIdentity = "ArmsAndArmor";
        private const string MartialIdentity = "CraftMundaneMartialWeapons";
        private const string ExoticIdentity = "CraftMundaneExoticWeapons";
        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly object Gate = new object();
        private static ModContext _context;
        private static CraftMagicItemsContract _contract;
        private static CraftMagicItemsRegistrationCatalog _catalog;
        private static Array _currentGraph;
        private static Array _rawGraph;
        private static object _magicFirearms;
        private static object _mundaneFirearms;
        private static object _ammunition;
        private static object _reliableRecipe;
        private static AmmunitionRenderPlan _ammunitionUiPlan;
        private static string _magicFeatGuid;
        private static object _martialData;
        private static object _exoticData;
        private static NewItemBaseState _martialState;
        private static NewItemBaseState _exoticState;
        private static bool _finalized;
        private static bool _failed;
        private static int _generation;
        private static bool _boundaryWarningLogged;
        private static bool _bridgeFailureLogged;
        private static DeferredUiFailure _deferredUiFailure;
        private static bool _uiFailureLogged;
        private static int _uiFailureCount;
        private static bool _uiFaulted;
        private static bool _projectPolicyFailureLogged;
        private static int _newProjectNormalizationCount;
        private static int _legacyProjectMigrationCount;
        private static int _lowerPanelRenderCount;
        private static int _ordinaryRouteCount;
        private static int _ordinaryBodyBypassCount;
        private static int _graphRollbackCount;
        private static readonly HashSet<string> UiEventTypes =
            new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> SelectedAmmunitionGuids =
            new HashSet<string>(StringComparer.Ordinal);
        private static readonly List<string> UiRouteObservations =
            new List<string>();
        private static PropertyInfo _eventCurrentProperty;
        private static PropertyInfo _eventTypeProperty;
        private static CraftMagicItemsGraphSnapshot _snapshot =
            EmptySnapshot(0);

        [ThreadStatic] private static CategoryScope _categoryScope;

        internal static CraftMagicItemsGraphSnapshot Snapshot
        { get { lock (Gate) return _snapshot; } }

        internal static CraftMagicItemsRegistrationCatalog Catalog
        { get { lock (Gate) return _catalog; } }

        internal static bool IsFinalized
        { get { lock (Gate) return _finalized && !_failed; } }

        internal static bool IsFailed
        { get { lock (Gate) return _failed; } }

        internal static bool HasDeferredUiFailure
        { get { lock (Gate) return _deferredUiFailure != null; } }

        internal static bool IsUiFaulted
        { get { lock (Gate) return _uiFaulted; } }

        internal static int NewProjectNormalizationCount
        { get { lock (Gate) return _newProjectNormalizationCount; } }

        internal static int LegacyProjectMigrationCount
        { get { lock (Gate) return _legacyProjectMigrationCount; } }

        internal static CraftMagicItemsAmmunitionUiSnapshot AmmunitionUiSnapshot
        {
            get
            {
                lock (Gate)
                    return new CraftMagicItemsAmmunitionUiSnapshot(
                        CraftMagicItemsMundaneUiTranspiler.AppliedSeam,
                        _lowerPanelRenderCount, _ordinaryRouteCount,
                        _ordinaryBodyBypassCount, _graphRollbackCount,
                        _uiFailureCount, _deferredUiFailure != null,
                        UiEventTypes.OrderBy(value => value).ToArray(),
                        SelectedAmmunitionGuids.OrderBy(value => value)
                            .ToArray(), UiRouteObservations.ToArray());
            }
        }

        internal static void ExternalDisabled()
        {
            lock (Gate)
            {
                _currentGraph = null;
                _rawGraph = null;
                _magicFirearms = null;
                _mundaneFirearms = null;
                _ammunition = null;
                _reliableRecipe = null;
                _ammunitionUiPlan = null;
                _magicFeatGuid = null;
                _martialData = null;
                _exoticData = null;
                _martialState = null;
                _exoticState = null;
                _finalized = false;
                _deferredUiFailure = null;
                _categoryScope = CategoryScope.None;
                _snapshot = EmptySnapshot(_generation);
            }
        }

        internal static void ReportBoundaryFailure(string phase,
            Exception exception)
        { Fail(phase, exception); }

        internal static void Configure(ModContext context,
            CraftMagicItemsContract contract,
            CraftMagicItemsRegistrationCatalog catalog)
        {
            if (context == null || contract == null || catalog == null)
                throw new ArgumentNullException(
                    "CMI reflection bridge inputs are incomplete.");
            InitializeUiEventObservation();
            lock (Gate)
            {
                if (_contract != null && !ReferenceEquals(_contract.Assembly,
                        contract.Assembly))
                    throw new InvalidOperationException(
                        "A second Craft Magic Items assembly cannot replace the active contract.");
                _context = context;
                _contract = contract;
                _catalog = catalog;
            }
        }

        // Called at CMI's first equipment-index prefix. At that seam its item
        // data and ordinary recipes are assigned, while equipment indexes have
        // not started. The bridge replaces the public array transactionally.
        internal static Array AugmentDataReadResult(Array raw)
        {
            CraftMagicItemsContract contract;
            CraftMagicItemsRegistrationCatalog catalog;
            ModContext context;
            object martial = null;
            object exotic = null;
            NewItemBaseState martialState = null;
            NewItemBaseState exoticState = null;
            lock (Gate)
            {
                contract = _contract;
                catalog = _catalog;
                context = _context;
            }
            if (contract == null || catalog == null || context == null)
                return raw;
            lock (Gate) if (_failed) return raw;
            try
            {
                if (raw == null || raw.GetType() !=
                        contract.ItemDataType.MakeArrayType())
                    throw new InvalidOperationException(
                        "CMI assigned no exact ItemCraftingData array.");
                lock (Gate)
                {
                    if (ReferenceEquals(raw, _currentGraph)) return raw;
                }

                RegisterLocalization();
                martial = RequireItemData(raw, MartialIdentity);
                exotic = RequireItemData(raw, ExoticIdentity);
                object arms = RequireItemData(raw, ArmsAndArmorIdentity);
                string weaponParent = ReadString(martial, "ParentNameId");
                string magicFeatGuid = ReadString(arms, "FeatGuid");
                if (string.IsNullOrWhiteSpace(weaponParent) ||
                    string.IsNullOrWhiteSpace(magicFeatGuid) ||
                    !string.Equals(weaponParent,
                        ReadString(exotic, "ParentNameId"),
                        StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        "CMI mundane weapon parent classification changed.");

                martialState = CaptureNewItemBaseState(martial);
                exoticState = CaptureNewItemBaseState(exotic);
                AppendNewItemBases(martial, catalog.MartialCreationBases);
                AppendNewItemBases(exotic, catalog.ExoticCreationBases);

                object magicFirearms = CreateRecipeBasedItemData(
                    MagicFirearmsIdentity,
                    "KMG.CraftMagicItems.Firearms.Name", null, null,
                    ReadInt(arms, "MinimumCasterLevel"),
                    ReadBool(arms, "PrerequisitesMandatory"),
                    catalog.Modules.Gunslinger ? catalog.FirearmCreationBases :
                        null, 0, 0, false, "Weapon");
                object mundaneFirearms = catalog.Modules.Gunslinger ?
                    CreateRecipeBasedItemData(MundaneFirearmsIdentity,
                        "KMG.CraftMagicItems.Firearms.Name", weaponParent,
                        null, 0, false, catalog.FirearmCreationBases, 0,
                        CraftMagicItemsCompatibilityPolicy.FirearmMundaneBaseDc,
                        true, "Weapon") : null;
                object ammunition = catalog.Ammunition.Length == 0 ? null :
                    CreateRecipeBasedItemData(AmmunitionIdentity,
                        "KMG.CraftMagicItems.Ammunition.Name", null, null, 0,
                        false, new BlueprintItemEquipment[0],
                        CraftMagicItemsCompatibilityPolicy
                            .AmmunitionBatchCount,
                        CraftMagicItemsCompatibilityPolicy
                            .AmmunitionMundaneBaseDc, false, "Usable");

                var additions = new List<object> { magicFirearms };
                if (mundaneFirearms != null) additions.Add(mundaneFirearms);
                if (ammunition != null) additions.Add(ammunition);
                Array augmented = AppendItemData(raw, additions.ToArray());
                lock (Gate)
                {
                    _rawGraph = raw;
                    _currentGraph = augmented;
                    _magicFirearms = magicFirearms;
                    _mundaneFirearms = mundaneFirearms;
                    _ammunition = ammunition;
                    _reliableRecipe = null;
                    _ammunitionUiPlan = null;
                    _magicFeatGuid = magicFeatGuid;
                    _martialData = martial;
                    _exoticData = exotic;
                    _martialState = martialState;
                    _exoticState = exoticState;
                    _finalized = false;
                    _failed = false;
                    _generation++;
                }
                context.Logger.Info("craft-magic-items",
                    "graph.augmented", string.Format(
                        CultureInfo.InvariantCulture,
                        "generation={0};itemTypesAdded={1};firearmBases={2};martialBases={3};exoticBases={4};ammunitionTypes={5}",
                        _generation, additions.Count,
                        catalog.FirearmCreationBases.Length,
                        catalog.MartialCreationBases.Length,
                        catalog.ExoticCreationBases.Length,
                        catalog.Ammunition.Length));
                return augmented;
            }
            catch (Exception exception)
            {
                TryRestoreNewItemBaseState(martial, martialState);
                TryRestoreNewItemBaseState(exotic, exoticState);
                Fail("graph-augmentation", exception);
                return raw;
            }
        }

        // Fallback used only when a compatible graph already exists before
        // this bridge is attached. A complete CMI rebuild is preferred.
        internal static void AfterDataRead()
        {
            CraftMagicItemsContract contract;
            lock (Gate) contract = _contract;
            if (contract == null) return;
            Array raw = contract.ItemDataField.GetValue(null) as Array;
            Array augmented = AugmentDataReadResult(raw);
            if (augmented != null && !ReferenceEquals(raw, augmented))
                contract.ItemDataField.SetValue(null, augmented);
        }

        // Called from the first equipment-index prefix after CMI has initialized
        // ordinary recipes, but before its equipment/enchantment index scan.
        internal static void BeforeEquipmentIndexes()
        {
            CraftMagicItemsContract contract;
            CraftMagicItemsRegistrationCatalog catalog;
            ModContext context;
            object magicFirearms;
            object mundaneFirearms;
            object ammunition;
            Array graph;
            lock (Gate)
            {
                if (_finalized || _failed) return;
                contract = _contract;
                catalog = _catalog;
                context = _context;
                magicFirearms = _magicFirearms;
                mundaneFirearms = _mundaneFirearms;
                ammunition = _ammunition;
                graph = _currentGraph;
            }
            if (contract == null || catalog == null || context == null ||
                graph == null) return;
            try
            {
                if (!ReferenceEquals(contract.ItemDataField.GetValue(null),
                        graph))
                    throw new InvalidOperationException(
                        "CMI replaced the augmented data graph before index finalization.");
                object arms = RequireItemData(graph, ArmsAndArmorIdentity);
                object exotic = RequireItemData(graph, ExoticIdentity);
                object[] ordinaryWeaponRecipes = ReadRecipes(arms)
                    .Where(RecipeSupportsWeapon).ToArray();
                if (ordinaryWeaponRecipes.Length == 0)
                    throw new InvalidOperationException(
                        "CMI initialized no ordinary weapon recipes.");

                object reliable = CreateReliableRecipe(catalog.Reliable);
                SetRecipes(magicFirearms,
                    ordinaryWeaponRecipes.Concat(new[] { reliable }).ToArray());
                if (mundaneFirearms != null)
                    SetRecipes(mundaneFirearms, ReadRecipes(exotic));
                object[] ammunitionRecipes = catalog.Ammunition.Select(value =>
                    CreateAmmunitionRecipe(value.Item)).ToArray();
                if (ammunition != null) SetRecipes(ammunition,
                    ammunitionRecipes);
                AmmunitionRenderPlan ammunitionUiPlan = ammunition == null ?
                    null : CreateAmmunitionRenderPlan(contract, ammunition,
                        ammunitionRecipes, catalog);

                contract.AddRecipeForEnchantment.Invoke(null, new object[] {
                    catalog.Reliable.AssetGuid, reliable });
                SynchronizeMundaneIndexes(contract, catalog,
                    mundaneFirearms);
                lock (Gate)
                {
                    _reliableRecipe = reliable;
                    _ammunitionUiPlan = ammunitionUiPlan;
                    _finalized = true;
                    _snapshot = new CraftMagicItemsGraphSnapshot(_generation,
                        CountAddedItemTypes(),
                        catalog.FirearmCreationBases.Length,
                        catalog.FirearmRecognitionBases.Length,
                        catalog.MartialCreationBases.Length,
                        catalog.ExoticCreationBases.Length, 0,
                        catalog.CustomFamilyRecognitionBases.Length,
                        ammunitionRecipes.Length, 1,
                        ordinaryWeaponRecipes.Length,
                        catalog.FirearmCreationBases.Select(value =>
                            value.AssetGuid).ToArray(),
                        catalog.FirearmRecognitionBases.Select(value =>
                            value.AssetGuid).ToArray(),
                        catalog.MartialCreationBases.Select(value =>
                            value.AssetGuid).ToArray(),
                        catalog.ExoticCreationBases.Select(value =>
                            value.AssetGuid).ToArray(),
                        catalog.CustomFamilyRecognitionBases.Select(value =>
                            value.AssetGuid).ToArray(),
                        catalog.NamedUpgradeOnly.Select(value =>
                            value.AssetGuid).ToArray());
                }
                ValidateFinalizedGraph();
                CraftMagicItemsCompatibilityStatusRegistry.Update(
                    new CraftMagicItemsCompatibilityStatus(
                        CraftMagicItemsCompatibilityAvailability.Active,
                        "CMI graph generation " + _generation +
                        " finalized with synchronized public data and indexes.",
                        _snapshot.ItemTypes,
                        _snapshot.FirearmCreationBases +
                            _snapshot.MartialBases +
                            _snapshot.ExoticBases,
                        _snapshot.OrdinaryWeaponRecipes +
                            _snapshot.ReliableRecipes +
                            _snapshot.AmmunitionRecipes));
                context.Logger.Info("craft-magic-items",
                    "graph.finalized", string.Format(
                        CultureInfo.InvariantCulture,
                        "generation={0};itemTypes={1};firearmCreationBases={2};firearmRecognitionIdentities={3};martialBases={4};exoticBases={5};customFamilyMagicItemTypes=0;customFamilyRecognitionIdentities={6};ordinaryWeaponRecipes={7};reliableRecipes={8};ammunitionRecipes={9};namedCreationBases=0",
                        _snapshot.Generation, _snapshot.ItemTypes,
                        _snapshot.FirearmCreationBases,
                        _snapshot.FirearmRecognitionIdentities,
                        _snapshot.MartialBases, _snapshot.ExoticBases,
                        _snapshot.CustomFamilyRecognitionIdentities,
                        _snapshot.OrdinaryWeaponRecipes,
                        _snapshot.ReliableRecipes,
                        _snapshot.AmmunitionRecipes));
            }
            catch (Exception exception)
            {
                Fail("graph-finalization", exception);
            }
        }

        // CMI publishes every non-null FeatGuid into every relevant feat
        // selection. This category intentionally stays feat-less while
        // that publication runs, then acquires the exact existing Arms and
        // Armor feat for CMI's normal UI/crafting checks.
        internal static void ActivateMagicFeatCategories()
        {
            object firearms;
            string featGuid;
            lock (Gate)
            {
                if (_failed || !_finalized) return;
                firearms = _magicFirearms;
                featGuid = _magicFeatGuid;
            }
            if (firearms == null || string.IsNullOrWhiteSpace(featGuid))
                throw new InvalidOperationException(
                    "CMI magic category feat activation is incomplete.");
            SetField(firearms, "FeatGuid", featGuid);
            if (!string.Equals(ReadString(firearms, "FeatGuid"), featGuid,
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "CMI magic Firearms did not acquire the exact Arms and Armor feat.");
        }

        internal static void EnterRecipeCategory(object craftingData,
            out CategoryScope state)
        {
            state = CategoryScope.None;
            lock (Gate)
            {
                if (ReferenceEquals(craftingData, _magicFirearms))
                    state = CategoryScope.Firearms;
            }
            _categoryScope = state;
        }

        internal static void ExitRecipeCategory(CategoryScope state)
        {
            if (state != CategoryScope.None) _categoryScope =
                CategoryScope.None;
        }

        internal static bool IsCandidateAllowed(BlueprintItemEquipment blueprint)
        {
            BlueprintItemWeapon weapon = blueprint as BlueprintItemWeapon;
            if (_categoryScope == CategoryScope.Firearms)
                return IsFirearm(weapon);
            return true;
        }

        internal static bool ShouldAdmitOwnedWeapon(ItemEntity item)
        {
            BlueprintItemWeapon weapon = item == null ? null :
                item.Blueprint as BlueprintItemWeapon;
            if (_categoryScope == CategoryScope.Firearms)
                return IsFirearm(weapon);
            return _categoryScope == CategoryScope.None &&
                IsRecognizedCustomFamily(weapon);
        }

        internal static bool ShouldRejectMatchingItem(
            BlueprintItemEquipment blueprint, BlueprintItemEquipment upgrade)
        {
            BlueprintItemWeapon weapon = blueprint as BlueprintItemWeapon;
            CraftMagicItemsRegistrationCatalog catalog;
            lock (Gate) catalog = _catalog;
            if (catalog == null || weapon == null) return false;
            if (_categoryScope != CategoryScope.None && upgrade == null &&
                catalog.IsNamedUpgradeOnly(weapon))
                return true;
            return !IsCandidateAllowed(blueprint);
        }

        internal static bool IsReliableRecipe(object recipe)
        { lock (Gate) return ReferenceEquals(recipe, _reliableRecipe); }

        internal static bool ReliableAppliesTo(BlueprintItem blueprint)
        {
            return blueprint == null
                ? _categoryScope == CategoryScope.Firearms
                : IsFirearm(blueprint as BlueprintItemWeapon);
        }

        internal static bool GuardCustomRecipeGuid(string originalGuid,
            IEnumerable<string> enchantments, out string blockedResult)
        {
            blockedResult = null;
            CraftMagicItemsRegistrationCatalog catalog;
            ModContext context;
            lock (Gate)
            {
                catalog = _catalog;
                context = _context;
            }
            if (catalog == null || enchantments == null ||
                !enchantments.Contains(catalog.Reliable.AssetGuid,
                    StringComparer.Ordinal)) return true;
            BlueprintItemWeapon weapon = ResolveWeapon(originalGuid);
            if (IsFirearm(weapon)) return true;
            lock (Gate)
            {
                if (!_boundaryWarningLogged && context != null)
                {
                    _boundaryWarningLogged = true;
                    context.Logger.Warning("craft-magic-items",
                        "reliable.boundary-rejected",
                        "A CMI custom-blueprint request attempted to apply Reliable to a non-firearm base and was rejected.");
                }
            }
            return false;
        }

        internal static void TransferOwnedFirearmState(ItemEntity resultItem,
            ItemEntity upgradeItem)
        {
            ItemEntityWeapon source = upgradeItem as ItemEntityWeapon;
            ItemEntityWeapon target = resultItem as ItemEntityWeapon;
            if (source == null || target == null ||
                !IsFirearm(source.Blueprint) || !IsFirearm(target.Blueprint))
                return;
            IReadOnlyList<string> sourceTokens =
                FirearmRuntimeState.ReadStateTokenIds(source);
            IReadOnlyList<string> targetTokens =
                FirearmRuntimeState.ReadStateTokenIds(target);
            if (sourceTokens.Count > 1 || targetTokens.Count != 0)
                throw new InvalidOperationException(
                    "CMI firearm upgrade state transfer found an ambiguous token carrier.");
            if (sourceTokens.Count == 1)
                FirearmRuntimeState.RestoreMissingStateToken(target,
                    sourceTokens[0]);
            UnitEntityData owner;
            if (BatteredFirearmOriginRuntime.TryGetOwner(source, out owner))
                BatteredFirearmOriginRuntime.Bind(target, owner);
            if (!FirearmRuntimeState.ReadStateTokenIds(source)
                    .SequenceEqual(sourceTokens) ||
                !FirearmRuntimeState.ReadStateTokenIds(target)
                    .SequenceEqual(sourceTokens))
                throw new InvalidOperationException(
                    "CMI firearm upgrade did not preserve exact item-owned state.");
        }

        internal static void NormalizeNewAmmunitionProject(object project)
        {
            try
            {
                CraftMagicItemsAmmunitionRegistration registration;
                CraftMagicItemsContract contract;
                if (!TryResolveAmmunitionProject(project, out registration,
                        out contract))
                    return;
                int current = (int)contract.ProjectTargetCostField.GetValue(
                    project);
                int normalized = CraftMagicItemsCompatibilityPolicy
                    .NormalizeAmmunitionProjectTarget(current,
                        registration.Plan.ValueDerivedTarget, true);
                if (normalized == current) return;
                contract.ProjectTargetCostField.SetValue(project, normalized);
                lock (Gate) _newProjectNormalizationCount++;
            }
            catch (Exception exception)
            {
                ReportProjectPolicyFailure("new-project-target", exception);
            }
        }

        internal static void MigrateExistingAmmunitionProjects(object timer)
        {
            if (timer == null) return;
            try
            {
                CraftMagicItemsContract contract;
                ModContext context;
                lock (Gate)
                {
                    contract = _contract;
                    context = _context;
                }
                if (contract == null || !contract.CraftingTimerComponentType
                        .IsInstanceOfType(timer))
                    return;
                IList projects = contract.TimerProjectsField.GetValue(timer)
                    as IList;
                if (projects == null) return;
                int migrated = 0;
                foreach (object project in projects)
                {
                    CraftMagicItemsAmmunitionRegistration registration;
                    CraftMagicItemsContract resolved;
                    if (!TryResolveAmmunitionProject(project,
                            out registration, out resolved))
                        continue;
                    int current = (int)resolved.ProjectTargetCostField
                        .GetValue(project);
                    int normalized = CraftMagicItemsCompatibilityPolicy
                        .NormalizeAmmunitionProjectTarget(current,
                            registration.Plan.ValueDerivedTarget, true);
                    if (normalized == current) continue;
                    resolved.ProjectTargetCostField.SetValue(project,
                        normalized);
                    migrated++;
                }
                if (migrated == 0) return;
                int total;
                lock (Gate)
                {
                    _legacyProjectMigrationCount += migrated;
                    total = _legacyProjectMigrationCount;
                }
                if (context != null) context.Logger.Info(
                    "craft-magic-items", "ammunition-projects.migrated",
                    "count=" + migrated + ";total=" + total +
                    ";target=5;goldSpentPreserved=true;progressPreserved=true");
            }
            catch (Exception exception)
            {
                ReportProjectPolicyFailure("legacy-project-migration",
                    exception);
            }
        }

        private static bool TryResolveAmmunitionProject(object project,
            out CraftMagicItemsAmmunitionRegistration registration,
            out CraftMagicItemsContract contract)
        {
            registration = null;
            CraftMagicItemsRegistrationCatalog catalog;
            lock (Gate)
            {
                contract = _contract;
                catalog = _catalog;
            }
            if (project == null || contract == null || catalog == null ||
                !contract.CraftingProjectDataType.IsInstanceOfType(project) ||
                !string.Equals(contract.ProjectItemTypeField.GetValue(project)
                    as string, AmmunitionIdentity, StringComparison.Ordinal) ||
                contract.ProjectUpgradeItemField.GetValue(project) != null)
                return false;
            ItemEntity result = contract.ProjectResultItemField.GetValue(
                project) as ItemEntity;
            if (result == null || result.Blueprint == null || result.Count !=
                    CraftMagicItemsCompatibilityPolicy.AmmunitionBatchCount)
                return false;
            registration = catalog.Ammunition.SingleOrDefault(value =>
                ReferenceEquals(value.Item, result.Blueprint));
            if (registration == null || !string.Equals(
                    contract.ProjectRecipeNameField.GetValue(project) as
                        string,
                    "KMGAmmunition." + registration.Item.AssetGuid,
                    StringComparison.Ordinal))
            {
                registration = null;
                return false;
            }
            return true;
        }

        private static void ReportProjectPolicyFailure(string phase,
            Exception exception)
        {
            ModContext context;
            bool log;
            lock (Gate)
            {
                context = _context;
                log = !_projectPolicyFailureLogged;
                _projectPolicyFailureLogged = true;
            }
            if (log && context != null) context.Logger.Failure(
                "craft-magic-items", "ammunition-project-policy.failed",
                "phase=" + phase +
                ";graphMutation=false;originalProjectPreserved=true",
                exception);
        }

        // Injected only after CMI has rendered and finalized its own outer and
        // parent/subtype selectors. Exact object identity controls this inner
        // route; SelectedIndex never controls renderer ownership.
        internal static bool TryRenderSelectedAmmunition(
            UnitEntityData crafter, object selectedCraftingData)
        {
            object ammunition;
            AmmunitionRenderPlan plan;
            bool available;
            lock (Gate)
            {
                ammunition = _ammunition;
                if (CraftMagicItemsMundaneUiRoutePolicy.Resolve(
                        selectedCraftingData, ammunition) !=
                    CraftMagicItemsMundaneUiRoute.AmmunitionLowerPanel)
                {
                    _ordinaryRouteCount++;
                    ObserveUiEventNoLock("ordinary-cmi");
                    return false;
                }
                plan = _ammunitionUiPlan;
                available = _finalized && !_failed;
                _ordinaryBodyBypassCount++;
                ObserveUiEventNoLock(AmmunitionIdentity);
            }

            try
            {
                if (plan == null || !available)
                    throw new InvalidOperationException(
                        "The selected KMG ammunition data has no finalized immutable UI plan.");
                plan.Preflight(crafter, selectedCraftingData);
                lock (Gate) _lowerPanelRenderCount++;
                if (crafter == null || crafter.Descriptor == null)
                {
                    plan.RenderLabel.Invoke(null, new object[] {
                        "No valid crafter is available for firearm ammunition." });
                    return true;
                }

                int selected = (int)plan.DrawSelection.Invoke(null,
                    new object[] { "Item: ", plan.Names, 5 });
                if (selected < 0 || selected >= plan.Recipes.Length)
                    selected = 0;
                object recipe = plan.Recipes[selected];
                BlueprintItem item = plan.Results[selected];
                plan.RenderLabel.Invoke(null, new object[] {
                    plan.Descriptions[selected] });
                plan.RenderCraftingSkill.Invoke(null, new object[] {
                    crafter, StatType.SkillKnowledgeWorld,
                    CraftMagicItemsCompatibilityPolicy.AmmunitionMundaneBaseDc,
                    0, null, null, false, null, true });
                plan.RenderCraftControl.Invoke(null, new object[] {
                    crafter, plan.CraftingData, recipe, 0, item, null });
                lock (Gate) SelectedAmmunitionGuids.Add(item.AssetGuid);
                return true;
            }
            catch (Exception exception)
            {
                ReportUiBoundaryFailure("ammunition-ui", exception);
                CraftMagicItemsUiFailureCapture failure =
                    CraftMagicItemsUiFailurePolicy.Capture(exception);
                ExceptionDispatchInfo.Capture(failure.Root).Throw();
                throw;
            }
        }

        internal static BlueprintItemWeapon BuildQualificationClone(
            BlueprintItemWeapon baseWeapon,
            BlueprintWeaponEnchantment enchantment)
        {
            return BuildQualificationClone(baseWeapon,
                new[] { enchantment });
        }

        internal static BlueprintItemWeapon BuildQualificationClone(
            BlueprintItemWeapon baseWeapon,
            IEnumerable<BlueprintWeaponEnchantment> enchantments)
        {
            BlueprintWeaponEnchantment[] values = enchantments == null ?
                null : enchantments.ToArray();
            if (baseWeapon == null || values == null || values.Length == 0 ||
                values.Any(value => value == null))
                throw new ArgumentNullException("qualification clone input");
            CraftMagicItemsContract contract;
            lock (Gate) contract = _contract;
            object patcher = contract == null ? null :
                contract.BlueprintPatcherField.GetValue(null);
            if (patcher == null) throw new InvalidOperationException(
                "CMI custom blueprint patcher is unavailable.");
            ParameterInfo[] parameters = contract.BuildCustomRecipeGuid
                .GetParameters();
            object[] arguments = parameters.Select(value =>
                value.HasDefaultValue ? value.DefaultValue :
                Default(value.ParameterType)).ToArray();
            arguments[0] = baseWeapon.AssetGuid;
            arguments[1] = values.Select(value => value.AssetGuid).ToArray();
            string guid = contract.BuildCustomRecipeGuid.Invoke(patcher,
                arguments) as string;
            return string.IsNullOrWhiteSpace(guid) ? null :
                ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>(guid);
        }

        internal static void RepeatFinalizationBoundaryForQualification()
        {
            AfterDataRead();
            BeforeEquipmentIndexes();
            ActivateMagicFeatCategories();
            ValidateFinalizedGraph();
        }

        internal static CraftMagicItemsQualificationResult
            RunGuardedQualification()
        {
            var checks = new List<CraftMagicItemsQualificationCheck>();
            var diagnostics = new List<string>();
            var customGuids = new List<string>();
            int initialGeneration = 0;
            int rebuiltGeneration = 0;
            try
            {
                CraftMagicItemsContract contract;
                CraftMagicItemsRegistrationCatalog catalog;
                Array graph;
                object firearms;
                object ammunition;
                object reliableRecipe;
                lock (Gate)
                {
                    contract = _contract;
                    catalog = _catalog;
                    graph = _currentGraph;
                    firearms = _magicFirearms;
                    ammunition = _ammunition;
                    reliableRecipe = _reliableRecipe;
                    initialGeneration = _snapshot.Generation;
                }
                if (contract == null || catalog == null || graph == null ||
                    firearms == null || reliableRecipe == null || !IsFinalized)
                    throw new InvalidOperationException(
                        "CMI qualification requires one finalized compatibility graph.");

                ValidateFinalizedGraph();
                AddQualificationCheck(checks, "contract-and-graph-active",
                    "exact reflected contract and one finalized graph",
                    "status=" + CraftMagicItemsCompatibilityStatusRegistry
                        .Current.Display + ";generation=" + initialGeneration,
                    CraftMagicItemsCompatibilityStatusRegistry.Current
                        .Availability ==
                            CraftMagicItemsCompatibilityAvailability.Active &&
                    initialGeneration > 0,
                    "live CMI contract fields and finalized KMG graph state");

                AddGraphQualificationChecks(checks, contract, catalog, graph,
                    firearms, ammunition, reliableRecipe);
                AddAmmunitionProjectQualificationCheck(checks, contract,
                    catalog);

                object arms = RequireItemData(graph, ArmsAndArmorIdentity);
                object ordinaryRecipe = ReadRecipes(arms).FirstOrDefault(
                    IsOrdinaryPlusOneRecipe);
                BlueprintWeaponEnchantment ordinaryPlusOne = ordinaryRecipe ==
                    null ? null : ReadRecipeEnchantments(ordinaryRecipe)
                        .OfType<BlueprintWeaponEnchantment>().FirstOrDefault();
                AddQualificationCheck(checks, "ordinary-weapon-recipes-reused",
                    "one initialized CMI +1 weapon recipe reused by KMG magic Firearms while ordinary Arms and Armor remains authoritative",
                    ordinaryRecipe == null || ordinaryPlusOne == null ?
                        "missing" :
                        "recipe=" + ReadString(ordinaryRecipe, "Name") +
                        ";enchantment=" + ordinaryPlusOne.AssetGuid,
                    ordinaryRecipe != null && ordinaryPlusOne != null &&
                    ReadRecipes(firearms).Count(value => ReferenceEquals(
                        value, ordinaryRecipe)) == 1,
                    "reference equality against CMI's initialized ArmsAndArmor recipe graph");
                if (ordinaryPlusOne == null)
                    throw new InvalidOperationException(
                        "CMI exposed no ordinary +1 weapon recipe for clone qualification.");

                BlueprintItemWeapon pistol = RequireWeapon(catalog,
                    CraftMagicItemsCatalogFamily.Firearm, "Pistol");
                BlueprintItemWeapon katana = FindWeapon(catalog,
                    CraftMagicItemsCatalogFamily.Katana);
                BlueprintItemWeapon spear = FindWeapon(catalog,
                    CraftMagicItemsCatalogFamily.ElvenBranchedSpear);
                CraftMagicItemsBlueprintIntegritySnapshot pistolBefore =
                    CaptureIntegrity(pistol);
                CraftMagicItemsBlueprintIntegritySnapshot katanaBefore =
                    katana == null ? null : CaptureIntegrity(katana);
                CraftMagicItemsBlueprintIntegritySnapshot spearBefore =
                    spear == null ? null : CaptureIntegrity(spear);

                BlueprintItemWeapon ordinaryPistol = BuildQualificationClone(
                    pistol, ordinaryPlusOne);
                AddCloneQualificationCheck(checks, "ordinary-pistol-clone",
                    pistolBefore, pistol, ordinaryPistol, true);
                BlueprintItemWeapon reliablePistol = BuildQualificationClone(
                    pistol, new[] { ordinaryPlusOne, catalog.Reliable });
                BlueprintItemWeapon enchantedKatana = katana == null ? null :
                    BuildQualificationClone(katana, ordinaryPlusOne);
                BlueprintItemWeapon enchantedSpear = spear == null ? null :
                    BuildQualificationClone(spear, ordinaryPlusOne);
                foreach (BlueprintItemWeapon value in new[] { ordinaryPistol,
                    reliablePistol, enchantedKatana, enchantedSpear })
                    if (value != null) customGuids.Add(value.AssetGuid);

                AddCloneQualificationCheck(checks, "reliable-pistol-clone",
                    pistolBefore, pistol, reliablePistol, true);
                if (katana != null)
                    AddCloneQualificationCheck(checks, "katana-clone",
                        katanaBefore, katana, enchantedKatana, false);
                if (spear != null)
                    AddCloneQualificationCheck(checks,
                        "elven-branched-spear-clone", spearBefore, spear,
                        enchantedSpear, false);

                AddReliableQualificationChecks(checks, contract, catalog,
                    reliablePistol);
                AddOwnedStateQualificationCheck(checks, pistol,
                    reliablePistol);
                AddOwnedCustomFamilyUpgradeCheck(checks, contract, catalog,
                    ordinaryRecipe, ordinaryPlusOne, customGuids);

                CraftMagicItemsGraphSnapshot before = Snapshot;
                CraftMagicItemsOptionalExtensionCoordinator
                    .RebuildCompleteGraphForQualification();
                CraftMagicItemsGraphSnapshot after = Snapshot;
                rebuiltGeneration = after.Generation;
                ValidateFinalizedGraph();
                bool sameGraph = SameGraphShape(before, after) &&
                    rebuiltGeneration == initialGeneration + 1;
                AddQualificationCheck(checks, "complete-rebuild-idempotence",
                    "one new generation with the same exact graph and no duplicates",
                    "before=" + DescribeSnapshot(before) + ";after=" +
                        DescribeSnapshot(after), sameGraph,
                    "real CMI OnToggle(false)/OnToggle(true) full index rebuild");
                diagnostics.Add("initialGraph=" + DescribeSnapshot(before));
                diagnostics.Add("rebuiltGraph=" + DescribeSnapshot(after));
                diagnostics.Add("customBlueprints=" + string.Join(",",
                    customGuids.ToArray()));
            }
            catch (Exception exception)
            {
                diagnostics.Add("qualificationException=" +
                    exception.GetType().FullName + ":" + exception.Message);
                AddQualificationCheck(checks, "qualification-completed",
                    "no exception", exception.GetType().FullName, false,
                    "bounded compatibility qualification exception");
            }
            return new CraftMagicItemsQualificationResult(checks, diagnostics,
                initialGeneration, rebuiltGeneration, customGuids);
        }

        private static void ValidateFinalizedGraph()
        {
            CraftMagicItemsContract contract;
            CraftMagicItemsRegistrationCatalog catalog;
            object reliable;
            object magicFirearms;
            object ammunition;
            lock (Gate)
            {
                contract = _contract;
                catalog = _catalog;
                reliable = _reliableRecipe;
                magicFirearms = _magicFirearms;
                ammunition = _ammunition;
            }
            if (contract == null || catalog == null || reliable == null ||
                ReadRecipes(magicFirearms).Count(value =>
                    ReferenceEquals(value, reliable)) != 1 ||
                ReadRecipeEnchantments(reliable).Length != 1 ||
                !ReferenceEquals(ReadRecipeEnchantments(reliable)[0],
                    catalog.Reliable) ||
                ReadNewItemBases(magicFirearms).Any(value =>
                    !IsFirearm(value as BlueprintItemWeapon)) ||
                (ammunition != null && ReadRecipes(ammunition).Length !=
                    catalog.Ammunition.Length))
                throw new InvalidOperationException(
                    "The finalized CMI compatibility graph failed validation.");
            IDictionary index = contract.EnchantmentToRecipeField.GetValue(null)
                as IDictionary;
            IList reliableIndex = index == null ? null :
                index[catalog.Reliable.AssetGuid] as IList;
            if (reliableIndex == null || reliableIndex.Cast<object>().Count(
                    value => ReferenceEquals(value, reliable)) != 1)
                throw new InvalidOperationException(
                    "Reliable is not indexed exactly once by CMI.");
        }

        private static void AddGraphQualificationChecks(
            ICollection<CraftMagicItemsQualificationCheck> checks,
            CraftMagicItemsContract contract,
            CraftMagicItemsRegistrationCatalog catalog, Array graph,
            object firearms, object ammunition, object reliableRecipe)
        {
            CraftMagicItemsGraphSnapshot snapshot = Snapshot;
            int expectedItemTypes = 1 +
                (catalog.Modules.Gunslinger ? 1 : 0) +
                (catalog.Ammunition.Length == 0 ? 0 : 1);
            AddQualificationCheck(checks, "registration-counts",
                "exact item types, bases, ordinary recipes, Reliable, and ammunition",
                DescribeSnapshot(snapshot), snapshot.ItemTypes ==
                    expectedItemTypes && snapshot.FirearmCreationBases ==
                    catalog.FirearmCreationBases.Length &&
                    snapshot.FirearmRecognitionIdentities ==
                    catalog.FirearmRecognitionBases.Length &&
                    snapshot.MartialBases ==
                    catalog.MartialCreationBases.Length &&
                    snapshot.ExoticBases ==
                    catalog.ExoticCreationBases.Length &&
                    snapshot.CustomFamilyMagicItemTypes == 0 &&
                    snapshot.CustomFamilyRecognitionIdentities ==
                    catalog.CustomFamilyRecognitionBases.Length &&
                    snapshot.ReliableRecipes == 1 &&
                    snapshot.AmmunitionRecipes == catalog.Ammunition.Length,
                "project-owned graph snapshot populated at CMI's pre-index boundary");

            string[] stableIdentities = { MagicFirearmsIdentity,
                MundaneFirearmsIdentity,
                AmmunitionIdentity };
            bool identitiesExact = stableIdentities.All(identity =>
                graph.Cast<object>().Count(value => string.Equals(
                    ReadString(value, "Name"), identity,
                    StringComparison.Ordinal)) ==
                (identity == MundaneFirearmsIdentity
                    ? (catalog.Modules.Gunslinger ? 1 : 0)
                    : identity == AmmunitionIdentity
                        ? (catalog.Ammunition.Length == 0 ? 0 : 1)
                        : 1));
            AddQualificationCheck(checks, "stable-item-type-identities",
                "each enabled KMG item type exactly once",
                string.Join(",", stableIdentities.Select(identity =>
                    identity + "=" + graph.Cast<object>().Count(value =>
                        string.Equals(ReadString(value, "Name"), identity,
                            StringComparison.Ordinal))).ToArray()),
                identitiesExact,
                "live ItemCraftingData[] stable registration identities");

            string[] firearmExpected = catalog.FirearmCreationBases.Select(
                value => value.AssetGuid).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();
            string[] firearmActual = ReadNewItemBases(firearms).Select(value =>
                value.AssetGuid).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();
            AddQualificationCheck(checks, "creation-base-inventory",
                "only exact ordinary-campaign firearm bases; no KMG custom-family magic creation category",
                "firearms=" + string.Join(",", firearmActual) +
                    ";customMagicTypes=" +
                    snapshot.CustomFamilyMagicItemTypes,
                firearmExpected.SequenceEqual(firearmActual,
                    StringComparer.Ordinal) &&
                    firearmActual.Distinct(StringComparer.Ordinal).Count() ==
                        firearmActual.Length &&
                    snapshot.CustomFamilyMagicItemTypes == 0,
                "live CMI NewItemBaseIDs compared to the finalized KMG catalogs");

            BlueprintItemWeapon[] recognitionOnly = catalog
                .FirearmRecognitionBases.Except(catalog.FirearmCreationBases)
                .ToArray();
            BlueprintItemEquipment[] graphBases = graph.Cast<object>()
                .SelectMany(ReadNewItemBases).ToArray();
            IDictionary recognitionIndex = contract.TypeToItemField
                .GetValue(null) as IDictionary;
            bool recognitionOnlyExact = recognitionOnly.Length == 2 &&
                recognitionOnly.Select(value => value.Name).OrderBy(value =>
                    value, StringComparer.Ordinal).SequenceEqual(new[] {
                        "Advanced Revolver", "Advanced Rifle" },
                    StringComparer.Ordinal) && recognitionOnly.All(value =>
                    graphBases.All(candidate => !ReferenceEquals(candidate,
                        value)) && value.Type != null && recognitionIndex !=
                        null && recognitionIndex.Contains(
                            value.Type.AssetGuid) && ReferenceEquals(
                                recognitionIndex[value.Type.AssetGuid],
                                value));
            AddQualificationCheck(checks,
                "advanced-firearms-recognition-only",
                "Advanced Rifle and Advanced Revolver absent from every creation base and present in owned-item type recognition",
                "recognitionOnly=" + string.Join(",", recognitionOnly
                    .Select(value => value.AssetGuid).ToArray()),
                recognitionOnlyExact,
                "all live NewItemBaseIDs plus exact CMI TypeToItem references");

            BlueprintItemEquipment[] everyBase = graphBases;
            string[] namedInBases = everyBase.Where(value =>
                catalog.NamedUpgradeOnly.Any(named =>
                    IsSameOrCustomIdentity(value.AssetGuid,
                        named.AssetGuid))).Select(value => value.AssetGuid)
                .ToArray();
            AddQualificationCheck(checks, "named-uniques-upgrade-only",
                "zero named campaign unique GUIDs in creation bases",
                namedInBases.Length == 0 ? "none" : string.Join(",",
                    namedInBases), namedInBases.Length == 0,
                "all live CMI NewItemBaseIDs scanned by exact/custom identity");

            object martial = RequireItemData(graph, MartialIdentity);
            object exotic = RequireItemData(graph, ExoticIdentity);
            bool mundanePlacement = catalog.MartialCreationBases.All(value =>
                    ReadNewItemBases(martial).Count(candidate => ReferenceEquals(
                        candidate, value)) == 1) &&
                catalog.ExoticCreationBases.All(value =>
                    ReadNewItemBases(exotic).Count(candidate => ReferenceEquals(
                        candidate, value)) == 1);
            AddQualificationCheck(checks, "mundane-category-placement",
                "Nodachi in Martial; Wakizashi, Katana, and Elven Branched Spear in Exotic",
                "martialKmg=" + catalog.MartialCreationBases.Length +
                    ";exoticKmg=" + catalog.ExoticCreationBases.Length,
                mundanePlacement,
                "exact references in CMI's initialized mundane base arrays");

            IDictionary typeIndex = contract.TypeToItemField.GetValue(null) as
                IDictionary;
            bool typeIndexExact = typeIndex != null &&
                catalog.FirearmRecognitionBases.Concat(catalog
                    .CustomFamilyRecognitionBases).All(value =>
                    value.Type != null &&
                    typeIndex.Contains(value.Type.AssetGuid) &&
                    ReferenceEquals(typeIndex[value.Type.AssetGuid], value));
            AddQualificationCheck(checks, "owned-weapon-recognition-index",
                "all five firearm and four custom-family identities are exact CMI type-index values",
                "firearmsExpected=" +
                    catalog.FirearmRecognitionBases.Length +
                    ";customFamiliesExpected=" +
                    catalog.CustomFamilyRecognitionBases.Length +
                    ";indexed=" + (typeIndex == null ? 0 :
                    catalog.FirearmRecognitionBases.Concat(catalog
                        .CustomFamilyRecognitionBases).Count(value =>
                        value.Type != null && typeIndex.Contains(
                            value.Type.AssetGuid))),
                typeIndexExact,
                "CMI TypeToItem after its complete mundane index build");

            AddReliableGraphQualificationChecks(checks, contract, catalog,
                graph, firearms, reliableRecipe);
            AddAmmunitionQualificationChecks(checks, catalog, ammunition);
        }

        private static void AddReliableGraphQualificationChecks(
            ICollection<CraftMagicItemsQualificationCheck> checks,
            CraftMagicItemsContract contract,
            CraftMagicItemsRegistrationCatalog catalog, Array graph,
            object firearms, object reliableRecipe)
        {
            BlueprintItemEnchantment[] enchantments =
                ReadRecipeEnchantments(reliableRecipe);
            Array prerequisiteSpells = RequireField(reliableRecipe.GetType(),
                "PrerequisiteSpells").GetValue(reliableRecipe) as Array;
            string costType = RequireField(reliableRecipe.GetType(),
                "CostType").GetValue(reliableRecipe).ToString();
            bool recipeExact = enchantments.Length == 1 &&
                ReferenceEquals(enchantments[0], catalog.Reliable) &&
                ReadInt(reliableRecipe, "CasterLevelStart") ==
                    CraftMagicItemsCompatibilityPolicy.ReliableCasterLevel &&
                ReadInt(reliableRecipe, "CostFactor") ==
                    CraftMagicItemsCompatibilityPolicy
                        .ReliableEquivalentBonus &&
                string.Equals(costType, "EnhancementLevelSquared",
                    StringComparison.Ordinal) &&
                prerequisiteSpells != null && prerequisiteSpells.Length == 0 &&
                string.Equals(ReadString(reliableRecipe, "NameId"),
                    "Reliable", StringComparison.Ordinal);
            AddQualificationCheck(checks, "reliable-recipe-authority",
                "exact KMG Reliable; +1 equivalent; CL 8; no invented Mending blueprint",
                "guid=" + catalog.Reliable.AssetGuid + ";plus=" +
                    ReadInt(reliableRecipe, "CostFactor") + ";cl=" +
                    ReadInt(reliableRecipe, "CasterLevelStart") +
                    ";prerequisiteSpells=" + (prerequisiteSpells == null ?
                        -1 : prerequisiteSpells.Length), recipeExact,
                "live RecipeData fields and exact BlueprintWeaponEnchantment reference");

            object arms = RequireItemData(graph, ArmsAndArmorIdentity);
            bool categoryExact = string.Equals(ReadString(firearms, "FeatGuid"),
                    ReadString(arms, "FeatGuid"), StringComparison.Ordinal) &&
                string.Equals(ResolveLocalizedText(ReadString(firearms,
                    "NameId")), "Firearms", StringComparison.Ordinal) &&
                ReadRecipes(firearms).Count(value => ReferenceEquals(value,
                    reliableRecipe)) == 1 &&
                graph.Cast<object>().Where(value => contract.RecipeBasedType
                    .IsInstanceOfType(value)).Sum(value => ReadRecipes(value)
                    .Count(recipe => ReferenceEquals(recipe,
                        reliableRecipe))) == 1;
            AddQualificationCheck(checks, "reliable-category-and-feat",
                "Reliable once in magic Firearms using Craft Magic Arms and Armor",
                "name=" + ResolveLocalizedText(ReadString(firearms,
                    "NameId")) + ";feat=" +
                    ReadString(firearms, "FeatGuid"), categoryExact,
                "exact CMI item-type and recipe reference graph");

            IDictionary recipeIndex = contract.EnchantmentToRecipeField
                .GetValue(null) as IDictionary;
            IList indexedRecipes = recipeIndex == null ? null :
                recipeIndex[catalog.Reliable.AssetGuid] as IList;
            IDictionary itemIndex = contract.EnchantmentToItemField
                .GetValue(null) as IDictionary;
            IList indexedItems = itemIndex == null ? null :
                itemIndex[catalog.Reliable.AssetGuid] as IList;
            BlueprintItemWeapon[] reliableAuthored = catalog.Weapons.Select(
                    value => value.Item).Where(value => value.Enchantments !=
                    null && value.Enchantments.Any(enchantment =>
                        ReferenceEquals(enchantment, catalog.Reliable)))
                .ToArray();
            bool itemIndexExact = indexedItems != null &&
                reliableAuthored.All(value => indexedItems.Cast<object>()
                    .Count(candidate => ReferenceEquals(candidate, value)) == 1) &&
                indexedItems.Cast<object>().OfType<BlueprintItemWeapon>()
                    .All(IsFirearm);
            bool indexesExact = indexedRecipes != null &&
                indexedRecipes.Cast<object>().Count(value => ReferenceEquals(
                    value, reliableRecipe)) == 1 && itemIndexExact;
            AddQualificationCheck(checks, "reliable-lookup-indexes",
                "one source recipe and every authored Reliable firearm recognized",
                "recipeCount=" + (indexedRecipes == null ? 0 :
                    indexedRecipes.Count) + ";authoredExpected=" +
                    reliableAuthored.Length + ";itemIndexCount=" +
                    (indexedItems == null ? 0 : indexedItems.Count),
                indexesExact,
                "CMI EnchantmentIdToRecipe and EnchantmentIdToItem final indexes");
        }

        private static void AddAmmunitionQualificationChecks(
            ICollection<CraftMagicItemsQualificationCheck> checks,
            CraftMagicItemsRegistrationCatalog catalog, object ammunition)
        {
            if (catalog.Ammunition.Length == 0)
            {
                AddQualificationCheck(checks, "ammunition-module-gate",
                    "no ammunition category while Gunslinger is disabled",
                    ammunition == null ? "absent" : "present",
                    ammunition == null,
                    "active FeatureModuleConfiguration and live graph");
                return;
            }
            object[] recipes = ReadRecipes(ammunition);
            bool exact = ammunition != null && ReadInt(ammunition, "Count") ==
                    CraftMagicItemsCompatibilityPolicy.AmmunitionBatchCount &&
                recipes.Length == catalog.Ammunition.Length;
            var observations = new List<string>();
            foreach (CraftMagicItemsAmmunitionRegistration registration in
                catalog.Ammunition)
            {
                object[] matches = recipes.Where(value => ReferenceEquals(
                    ReadRecipeResult(value), registration.Item)).ToArray();
                exact &= matches.Length == 1 &&
                    registration.Plan.BatchValue ==
                        registration.Plan.UnitCost * 20 &&
                    registration.Plan.ValueDerivedTarget == Math.Max(1,
                        registration.Plan.BatchValue / 4) &&
                    registration.Plan.TimedProjectTarget == 5;
                observations.Add(registration.Item.Name + ":guid=" +
                    registration.Item.AssetGuid + ":count=20:value=" +
                    registration.Plan.BatchValue + ":valueTarget=" +
                    registration.Plan.ValueDerivedTarget +
                    ":timedTarget=" +
                    registration.Plan.TimedProjectTarget + ":gold=" +
                    registration.Plan.GoldCost(1f));
            }
            AddQualificationCheck(checks, "ammunition-result-recipes",
                "three exact BlueprintItem results; 20 units each; ordinary CMI mundane economics",
                string.Join("|", observations.ToArray()), exact,
                "live RecipeData.ResultItem references plus project-owned unit costs and CMI formula");
        }

        private static void AddAmmunitionProjectQualificationCheck(
            ICollection<CraftMagicItemsQualificationCheck> checks,
            CraftMagicItemsContract contract,
            CraftMagicItemsRegistrationCatalog catalog)
        {
            if (catalog.Ammunition.Length == 0) return;
            var results = new List<ItemEntity>();
            int normalizationBefore = NewProjectNormalizationCount;
            int migrationBefore = LegacyProjectMigrationCount;
            try
            {
                var projects = new List<object>();
                foreach (CraftMagicItemsAmmunitionRegistration registration in
                    catalog.Ammunition)
                {
                    ItemEntity result = registration.Item.CreateEntity();
                    result.SetCount(registration.Plan.Count);
                    results.Add(result);
                    object project = CreateQualificationProject(contract,
                        registration, result);
                    projects.Add(project);
                    if ((int)contract.ProjectTargetCostField.GetValue(project)
                            != registration.Plan.TimedProjectTarget ||
                        (int)contract.ProjectGoldSpentField.GetValue(project)
                            != registration.Plan.GoldCost(1f) ||
                        !ReferenceEquals(contract.ProjectResultItemField
                            .GetValue(project), result))
                        throw new InvalidOperationException(
                            "A request-local ammunition project did not preserve target, gold, or result identity.");
                }

                object legacy = projects[2];
                CraftMagicItemsAmmunitionRegistration legacyRegistration =
                    catalog.Ammunition[2];
                contract.ProjectTargetCostField.SetValue(legacy,
                    legacyRegistration.Plan.ValueDerivedTarget);
                contract.ProjectProgressField.SetValue(legacy, 7);
                int legacyGold = (int)contract.ProjectGoldSpentField
                    .GetValue(legacy);
                object timer = FormatterServices.GetUninitializedObject(
                    contract.CraftingTimerComponentType);
                IList timerProjects = Activator.CreateInstance(
                    contract.TimerProjectsField.FieldType) as IList;
                timerProjects.Add(legacy);
                contract.TimerProjectsField.SetValue(timer, timerProjects);
                MigrateExistingAmmunitionProjects(timer);
                int migrationAfterFirst = LegacyProjectMigrationCount;
                MigrateExistingAmmunitionProjects(timer);
                int migrationAfterSecond = LegacyProjectMigrationCount;

                bool exact = projects.All(project =>
                        (int)contract.ProjectTargetCostField.GetValue(project)
                            == 5) &&
                    (int)contract.ProjectTargetCostField.GetValue(legacy) ==
                        5 &&
                    (int)contract.ProjectProgressField.GetValue(legacy) ==
                        7 &&
                    (int)contract.ProjectGoldSpentField.GetValue(legacy) ==
                        legacyGold &&
                    migrationAfterFirst == migrationBefore + 1 &&
                    migrationAfterSecond == migrationAfterFirst &&
                    NewProjectNormalizationCount == normalizationBefore + 2;
                AddQualificationCheck(checks,
                    "ammunition-project-target-and-migration",
                    "all three new timed targets 5 with gold 34/4/40; one target-60 legacy project migrates once while preserving progress, gold, result, and order",
                    "targets=" + string.Join(",", projects.Select(value =>
                        contract.ProjectTargetCostField.GetValue(value)
                            .ToString()).ToArray()) + ";gold=" +
                    string.Join(",", projects.Select(value => contract
                        .ProjectGoldSpentField.GetValue(value).ToString())
                        .ToArray()) + ";legacyProgress=" + contract
                        .ProjectProgressField.GetValue(legacy) +
                    ";migrationDelta=" + (migrationAfterSecond -
                        migrationBefore), exact,
                    "real patched installed-CMI constructor plus exact timer-project reconciliation boundary");
            }
            finally
            {
                foreach (ItemEntity result in results) result.Dispose();
            }
        }

        private static object CreateQualificationProject(
            CraftMagicItemsContract contract,
            CraftMagicItemsAmmunitionRegistration registration,
            ItemEntity result)
        {
            ParameterInfo[] parameters = contract.CraftingProjectConstructor
                .GetParameters();
            object[] arguments = { null,
                registration.Plan.ValueDerivedTarget,
                registration.Plan.GoldCost(1f), 0, result,
                AmmunitionIdentity,
                "KMGAmmunition." + registration.Item.AssetGuid,
                Array.CreateInstance(parameters[7].ParameterType
                    .GetElementType(), 0),
                Array.CreateInstance(parameters[8].ParameterType
                    .GetElementType(), 0), false, false, null,
                Array.CreateInstance(parameters[12].ParameterType
                    .GetElementType(), 0) };
            try
            {
                return contract.CraftingProjectConstructor.Invoke(arguments);
            }
            catch (TargetInvocationException exception)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException ??
                    exception).Throw();
                throw;
            }
        }

        private static void AddReliableQualificationChecks(
            ICollection<CraftMagicItemsQualificationCheck> checks,
            CraftMagicItemsContract contract,
            CraftMagicItemsRegistrationCatalog catalog,
            BlueprintItemWeapon reliableClone)
        {
            bool firearmsExact = catalog.FirearmRecognitionBases.All(value =>
                InvokeRecipeApplies(contract, _reliableRecipe, value));
            BlueprintItemWeapon katana = FindWeapon(catalog,
                CraftMagicItemsCatalogFamily.Katana);
            BlueprintItemWeapon wakizashi = FindWeapon(catalog,
                CraftMagicItemsCatalogFamily.Wakizashi);
            BlueprintItemWeapon nodachi = FindWeapon(catalog,
                CraftMagicItemsCatalogFamily.Nodachi);
            BlueprintItemWeapon spear = FindWeapon(catalog,
                CraftMagicItemsCatalogFamily.ElvenBranchedSpear);
            BlueprintItemWeapon[] native = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintItemWeapon>().Where(value =>
                    value != null && !IsFirearm(value)).ToArray();
            BlueprintItemWeapon bow = native.FirstOrDefault(value => value
                .Category.ToString().IndexOf("bow",
                    StringComparison.OrdinalIgnoreCase) >= 0 && value.Category
                .ToString().IndexOf("crossbow",
                    StringComparison.OrdinalIgnoreCase) < 0);
            BlueprintItemWeapon crossbow = native.FirstOrDefault(value => value
                .Category.ToString().IndexOf("crossbow",
                    StringComparison.OrdinalIgnoreCase) >= 0);
            BlueprintItemWeapon arbitrary = native.FirstOrDefault(value =>
                !ReferenceEquals(value, bow) && !ReferenceEquals(value,
                    crossbow) && !ReferenceEquals(value, katana) &&
                !ReferenceEquals(value, wakizashi) && !ReferenceEquals(value,
                    nodachi) && !ReferenceEquals(value, spear));
            BlueprintItemWeapon[] rejected = { bow, crossbow, katana,
                wakizashi, nodachi, spear, arbitrary };
            bool rejectedExact = rejected.All(value => value != null &&
                !InvokeRecipeApplies(contract, _reliableRecipe, value));
            bool cloneExact = reliableClone != null &&
                InvokeRecipeApplies(contract, _reliableRecipe, reliableClone);
            AddQualificationCheck(checks, "reliable-applicability-matrix",
                "all five firearms and a CMI clone true; bow, crossbow, Eastern, spear, and arbitrary weapon false",
                "firearms=" + catalog.FirearmRecognitionBases.Length +
                    ";clone=" + cloneExact + ";rejectedResolved=" +
                    rejected.Count(value => value != null),
                firearmsExact && cloneExact && rejectedExact,
                "real patched CMI RecipeAppliesToBlueprint final boundary");

            string blocked;
            bool finalGuard = katana != null &&
                !GuardCustomRecipeGuid(katana.AssetGuid,
                    new[] { catalog.Reliable.AssetGuid }, out blocked) &&
                string.IsNullOrEmpty(blocked);
            AddQualificationCheck(checks, "reliable-custom-guid-boundary",
                "non-firearm Reliable custom blueprint request rejected",
                finalGuard ? "rejected" : "not-rejected", finalGuard,
                "BuildCustomRecipeItemGuid prefix immediately before CMI blueprint generation");

            int plusEquivalent = reliableClone == null ? -1 :
                (int)contract.ItemPlusEquivalent.Invoke(null,
                    new object[] { reliableClone });
            int rulesCost = reliableClone == null ? -1 :
                (int)contract.RulesRecipeItemCost.Invoke(null,
                    new object[] { reliableClone, -1, 0f });
            BlueprintItemWeapon baseWeapon = RequireWeapon(catalog,
                CraftMagicItemsCatalogFamily.Firearm, "Pistol");
            int expectedCost = baseWeapon == null ? -1 :
                checked(baseWeapon.Cost + 300 + 8000);
            bool exactCost = reliableClone != null && plusEquivalent == 2 &&
                rulesCost == expectedCost && reliableClone.Enchantments.Count(
                    value => ReferenceEquals(value, catalog.Reliable)) == 1;
            AddQualificationCheck(checks, "reliable-plus-and-price",
                "+1 enhancement plus +1 Reliable and CMI price = base + 300 masterwork + 8000",
                "plus=" + plusEquivalent + ";rulesCost=" + rulesCost +
                    ";expected=" + expectedCost, exactCost,
                "live CMI ItemPlusEquivalent and RulesRecipeItemCost");
        }

        private static void AddOwnedCustomFamilyUpgradeCheck(
            ICollection<CraftMagicItemsQualificationCheck> checks,
            CraftMagicItemsContract contract,
            CraftMagicItemsRegistrationCatalog catalog,
            object ordinaryRecipe,
            BlueprintWeaponEnchantment ordinaryPlusOne,
            ICollection<string> customGuids)
        {
            CraftMagicItemsWeaponRegistration[] owned = catalog.Weapons
                .Where(value => value.Policy.Family !=
                        CraftMagicItemsCatalogFamily.Firearm &&
                    value.Policy.Family !=
                        CraftMagicItemsCatalogFamily.Diagnostic &&
                    (value.Policy.Role == CraftMagicItemsCatalogRole
                        .CanonicalCreationBase || value.Policy.Role ==
                        CraftMagicItemsCatalogRole.AuthoredGenericTarget ||
                     value.Policy.Role == CraftMagicItemsCatalogRole
                        .NamedUpgradeOnly))
                .ToArray();
            BlueprintItemWeapon[] custom = catalog
                .CustomFamilyRecognitionBases.Select(value =>
                    BuildQualificationClone(value, ordinaryPlusOne))
                .ToArray();
            foreach (BlueprintItemWeapon value in custom)
                customGuids.Add(value.AssetGuid);
            BlueprintItemWeapon[] candidates = owned.Select(value =>
                    value.Item).Concat(custom).ToArray();
            bool exact = candidates.Length > 4 && candidates.All(value =>
                CanUpgradeThroughOrdinaryArms(contract, ordinaryRecipe,
                    value));
            string roles = string.Join(",", owned.GroupBy(value =>
                    value.Policy.Role).OrderBy(value => value.Key)
                .Select(value => value.Key + "=" + value.Count()).ToArray());
            AddQualificationCheck(checks,
                "ordinary-arms-owned-custom-family-upgrades",
                "ordinary Arms and Armor accepts canonical, authored, named, and prior CMI custom representatives across all four custom families",
                "owned=" + owned.Length + ";customClones=" +
                    custom.Length + ";roles=" + roles,
                exact && catalog.CustomFamilyRecognitionBases.Length == 4,
                "real patched CMI CanEnchant plus initialized Arms and Armor +1 recipe applicability");
        }

        private static bool CanUpgradeThroughOrdinaryArms(
            CraftMagicItemsContract contract, object recipe,
            BlueprintItemWeapon blueprint)
        {
            ItemEntity entity = blueprint == null ? null :
                blueprint.CreateEntity();
            if (entity == null) return false;
            try
            {
                entity.Identify();
                bool candidate = (bool)contract.CanEnchant.Invoke(null,
                    new object[] { entity });
                return candidate && InvokeRecipeApplies(contract, recipe,
                    blueprint);
            }
            catch (TargetInvocationException exception)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException ??
                    exception).Throw();
                throw;
            }
            finally
            {
                entity.Dispose();
            }
        }

        private static void AddOwnedStateQualificationCheck(
            ICollection<CraftMagicItemsQualificationCheck> checks,
            BlueprintItemWeapon baseWeapon, BlueprintItemWeapon clone)
        {
            ItemEntityWeapon source = baseWeapon == null ? null :
                baseWeapon.CreateEntity() as ItemEntityWeapon;
            ItemEntityWeapon target = clone == null ? null :
                clone.CreateEntity() as ItemEntityWeapon;
            bool exact = false;
            string observed = "entity-creation-failed";
            try
            {
                if (source != null && target != null)
                {
                    var resolver = new KingmakerFirearmRuntimeItemResolver();
                    ResolvedFirearmItem resolved;
                    string reason;
                    bool cloneResolved = resolver.TryResolve(target,
                        out resolved, out reason);
                    FirearmRuntimeState.SeedLegacyTokenForDebug(source,
                        new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                            FirearmStateTokenCatalog.DiagnosticLeadBall,
                            FirearmCondition.Normal));
                    TransferOwnedFirearmState(target, source);
                    string[] sourceTokens = FirearmRuntimeState
                        .ReadStateTokenIds(source).ToArray();
                    string[] targetTokens = FirearmRuntimeState
                        .ReadStateTokenIds(target).ToArray();
                    exact = cloneResolved && sourceTokens.Length == 1 &&
                        sourceTokens.SequenceEqual(targetTokens,
                            StringComparer.Ordinal);
                    observed = "cloneResolved=" + cloneResolved +
                        ";sourceTokens=" + string.Join(",", sourceTokens) +
                        ";targetTokens=" + string.Join(",", targetTokens) +
                        (reason == null ? string.Empty : ";reason=" + reason);
                }
            }
            finally
            {
                if (source != null) FirearmRuntimeState.Service.Forget(source);
                if (target != null) FirearmRuntimeState.Service.Forget(target);
            }
            AddQualificationCheck(checks, "owned-firearm-state-transfer",
                "CMI clone resolves as a firearm and receives one exact item-owned state token",
                observed, exact,
                "real ItemEntityWeapon resolver and CraftItem compatibility prefix logic");
        }

        private static void AddCloneQualificationCheck(
            ICollection<CraftMagicItemsQualificationCheck> checks, string name,
            CraftMagicItemsBlueprintIntegritySnapshot baseBefore,
            BlueprintItemWeapon baseWeapon, BlueprintItemWeapon clone,
            bool firearm)
        {
            CraftMagicItemsBlueprintIntegritySnapshot baseAfter =
                CaptureIntegrity(baseWeapon);
            CraftMagicItemsBlueprintIntegritySnapshot cloneSnapshot =
                CaptureIntegrity(clone);
            CraftMagicItemsBlueprintIntegrityDecision decision =
                CraftMagicItemsCompatibilityPolicy.ValidateCustomClone(
                    baseBefore, baseAfter, cloneSnapshot, firearm);
            bool exact = decision.Valid && clone != null &&
                ReferenceEquals(baseWeapon.Type, clone.Type) &&
                baseWeapon.ComponentsArray.Length ==
                    clone.ComponentsArray.Length;
            AddQualificationCheck(checks, name,
                "distinct CMI identity with unchanged type, components, proficiency, presentation, category, and inherent mechanics",
                clone == null ? "missing" : "guid=" + clone.AssetGuid +
                    ";type=" + clone.Type.AssetGuid + ";decision=" +
                    decision.FailedCheck, exact,
                "live CMI custom blueprint plus pre/post base snapshots");
        }

        private static CraftMagicItemsBlueprintIntegritySnapshot
            CaptureIntegrity(BlueprintItemWeapon weapon)
        {
            if (weapon == null) return null;
            BlueprintComponent[] itemComponents = weapon.ComponentsArray ??
                new BlueprintComponent[0];
            BlueprintComponent[] typeComponents = weapon.Type == null ?
                new BlueprintComponent[0] : weapon.Type.ComponentsArray ??
                    new BlueprintComponent[0];
            int proficiency = itemComponents.Concat(typeComponents).Count(
                value => value != null && value.GetType().Name.IndexOf(
                    "Proficiency", StringComparison.OrdinalIgnoreCase) >= 0);
            var mechanics = new List<string>();
            mechanics.AddRange(itemComponents.Where(value => value != null)
                .Select(value => "item-component:" + value.GetType().FullName +
                    ":" + value.name));
            mechanics.AddRange(typeComponents.Where(value => value != null)
                .Select(value => "type-component:" + value.GetType().FullName +
                    ":" + value.name));
            mechanics.AddRange((weapon.Enchantments ??
                    new List<BlueprintItemEnchantment>()).Where(value => value !=
                    null && value.EnchantmentCost == 0).Select(value =>
                        "zero-cost-enchantment:" + value.AssetGuid));
            if (weapon.Type != null)
                mechanics.Add("type-flags:ranged=" + weapon.IsRanged +
                    ";twoHanded=" + weapon.Type.IsTwoHanded + ";light=" +
                    weapon.Type.IsLight + ";natural=" +
                    weapon.Type.IsNatural + ";monk=" + weapon.Type.IsMonk +
                    ";range=" + weapon.Type.AttackRange);
            mechanics.Sort(StringComparer.Ordinal);
            return new CraftMagicItemsBlueprintIntegritySnapshot(
                weapon.AssetGuid, weapon.Type == null ? string.Empty :
                    weapon.Type.AssetGuid,
                weapon.Type == null ? 0 : (weapon.Type.ComponentsArray ??
                    new BlueprintComponent[0]).OfType<
                        FirearmDefinitionComponent>().Count(),
                proficiency, DescribePresentation(weapon),
                weapon.Category.ToString(), mechanics);
        }

        private static string DescribePresentation(BlueprintItemWeapon weapon)
        {
            string icon = weapon.Icon == null ? "<null>" : weapon.Icon.name;
            string itemVisual = weapon.VisualParameters == null ? "<null>" :
                weapon.VisualParameters.AnimStyle + "/" +
                ObjectName(weapon.VisualParameters.Model) + "/" +
                ObjectName(weapon.VisualParameters.BeltModel) + "/" +
                ObjectName(weapon.VisualParameters.SheathModel);
            string typeVisual = weapon.Type == null ||
                weapon.Type.VisualParameters == null ? "<null>" :
                weapon.Type.VisualParameters.AnimStyle + "/" +
                ObjectName(weapon.Type.VisualParameters.Model) + "/" +
                ObjectName(weapon.Type.VisualParameters.BeltModel) + "/" +
                ObjectName(weapon.Type.VisualParameters.SheathModel);
            return icon + ";item=" + itemVisual + ";type=" + typeVisual;
        }

        private static string ObjectName(UnityEngine.Object value)
        { return value == null ? "<null>" : value.name; }

        private static bool InvokeRecipeApplies(
            CraftMagicItemsContract contract, object recipe,
            BlueprintItem blueprint)
        {
            return (bool)contract.RecipeApplies.Invoke(null, new object[] {
                recipe, blueprint, true, false });
        }

        private static bool IsOrdinaryPlusOneRecipe(object recipe)
        {
            if (recipe == null || ReferenceEquals(recipe, _reliableRecipe) ||
                !RecipeSupportsWeapon(recipe)) return false;
            object costType = RequireField(recipe.GetType(), "CostType")
                .GetValue(recipe);
            BlueprintItemEnchantment[] values =
                ReadRecipeEnchantments(recipe);
            return costType != null && string.Equals(costType.ToString(),
                    "EnhancementLevelSquared", StringComparison.Ordinal) &&
                values.Length > 0 && values[0] is
                    BlueprintWeaponEnchantment && values[0]
                    .GetComponent<WeaponEnhancementBonus>() != null &&
                values[0].EnchantmentCost == 1;
        }

        private static BlueprintItemWeapon RequireWeapon(
            CraftMagicItemsRegistrationCatalog catalog,
            CraftMagicItemsCatalogFamily family, string displayName)
        {
            BlueprintItemWeapon result = catalog.Weapons.Where(value =>
                    value.Policy.Role == CraftMagicItemsCatalogRole
                        .CanonicalCreationBase &&
                    value.Policy.Family == family && string.Equals(
                        value.Item.Name, displayName,
                        StringComparison.Ordinal)).Select(value => value.Item)
                .SingleOrDefault();
            if (result == null) throw new InvalidOperationException(
                "The qualification catalog lacks canonical " + displayName +
                ".");
            return result;
        }

        private static BlueprintItemWeapon FindWeapon(
            CraftMagicItemsRegistrationCatalog catalog,
            CraftMagicItemsCatalogFamily family)
        {
            return catalog.Weapons.Where(value => value.Policy.Role ==
                    CraftMagicItemsCatalogRole.CanonicalCreationBase &&
                value.Policy.Family == family).Select(value => value.Item)
                .FirstOrDefault();
        }

        private static bool IsSameOrCustomIdentity(string candidate,
            string original)
        {
            return string.Equals(candidate, original, StringComparison.Ordinal) ||
                candidate != null && candidate.StartsWith(original +
                    "#CraftMagicItems", StringComparison.Ordinal);
        }

        private static bool SameGraphShape(CraftMagicItemsGraphSnapshot first,
            CraftMagicItemsGraphSnapshot second)
        {
            return first != null && second != null &&
                first.ItemTypes == second.ItemTypes &&
                first.FirearmCreationBases ==
                    second.FirearmCreationBases &&
                first.FirearmRecognitionIdentities ==
                    second.FirearmRecognitionIdentities &&
                first.MartialBases == second.MartialBases &&
                first.ExoticBases == second.ExoticBases &&
                first.CustomFamilyMagicItemTypes ==
                    second.CustomFamilyMagicItemTypes &&
                first.CustomFamilyRecognitionIdentities ==
                    second.CustomFamilyRecognitionIdentities &&
                first.AmmunitionRecipes == second.AmmunitionRecipes &&
                first.ReliableRecipes == second.ReliableRecipes &&
                first.OrdinaryWeaponRecipes == second.OrdinaryWeaponRecipes &&
                first.FirearmCreationBaseGuids.SequenceEqual(
                    second.FirearmCreationBaseGuids,
                    StringComparer.Ordinal) &&
                first.FirearmRecognitionGuids.SequenceEqual(
                    second.FirearmRecognitionGuids,
                    StringComparer.Ordinal) &&
                first.MartialBaseGuids.SequenceEqual(second.MartialBaseGuids,
                    StringComparer.Ordinal) &&
                first.ExoticBaseGuids.SequenceEqual(second.ExoticBaseGuids,
                    StringComparer.Ordinal) &&
                first.CustomFamilyRecognitionGuids.SequenceEqual(
                    second.CustomFamilyRecognitionGuids,
                    StringComparer.Ordinal) &&
                first.NamedUpgradeOnlyGuids.SequenceEqual(
                    second.NamedUpgradeOnlyGuids, StringComparer.Ordinal);
        }

        private static string DescribeSnapshot(
            CraftMagicItemsGraphSnapshot value)
        {
            return value == null ? "<null>" : string.Format(
                CultureInfo.InvariantCulture,
                "generation={0};itemTypes={1};firearmCreation={2};firearmRecognition={3};martial={4};exotic={5};customMagicTypes={6};customRecognition={7};ordinaryRecipes={8};reliable={9};ammunition={10}",
                value.Generation, value.ItemTypes,
                value.FirearmCreationBases,
                value.FirearmRecognitionIdentities, value.MartialBases,
                value.ExoticBases, value.CustomFamilyMagicItemTypes,
                value.CustomFamilyRecognitionIdentities,
                value.OrdinaryWeaponRecipes, value.ReliableRecipes,
                value.AmmunitionRecipes);
        }

        private static void AddQualificationCheck(
            ICollection<CraftMagicItemsQualificationCheck> checks,
            string name, string expected, string observed, bool passed,
            string evidence)
        {
            checks.Add(new CraftMagicItemsQualificationCheck(name, expected,
                observed, passed, evidence));
        }

        private static object CreateRecipeBasedItemData(string name,
            string nameId, string parentNameId, string featGuid,
            int minimumCasterLevel, bool prerequisitesMandatory,
            IEnumerable<BlueprintItemEquipment> bases, int count,
            int mundaneDc, bool mundaneStackable, string slot)
        {
            CraftMagicItemsContract contract = _contract;
            object value = Activator.CreateInstance(contract.RecipeBasedType);
            SetEnumProperty(value, "DataType", "RecipeBased");
            SetField(value, "Name", name);
            SetField(value, "NameId", nameId);
            SetField(value, "ParentNameId", parentNameId);
            SetField(value, "FeatGuid", featGuid);
            SetField(value, "MinimumCasterLevel", minimumCasterLevel);
            SetField(value, "PrerequisitesMandatory",
                prerequisitesMandatory);
            SetField(value, "Count", count);
            SetField(value, "RecipeFileNames", new string[0]);
            SetEnumArrayField(value, "Slots", slot);
            SetField(value, "SlotRestrictions", null);
            SetField(value, "MundaneBaseDC", mundaneDc);
            SetField(value, "MundaneEnhancementsStackable",
                mundaneStackable);
            SetNewItemBases(value, bases);
            SetRecipes(value, new object[0]);
            return value;
        }

        private static object CreateReliableRecipe(
            BlueprintWeaponEnchantment reliable)
        {
            object recipe = Activator.CreateInstance(_contract.RecipeDataType);
            SetField(recipe, "Name", ReliableRecipeIdentity);
            SetField(recipe, "NameId", "Reliable");
            SetField(recipe, "ParentNameId", null);
            SetRecipeEnchantments(recipe,
                new BlueprintItemEnchantment[] { reliable });
            SetField(recipe, "EnchantmentsCumulative", false);
            SetField(recipe, "CasterLevelStart",
                CraftMagicItemsCompatibilityPolicy.ReliableCasterLevel);
            SetField(recipe, "CasterLevelMultiplier", 0);
            SetEmptyArrayField(recipe, "PrerequisiteSpells");
            SetEnumField(recipe, "CostType", "EnhancementLevelSquared");
            SetField(recipe, "CostFactor",
                CraftMagicItemsCompatibilityPolicy.ReliableEquivalentBonus);
            SetField(recipe, "CostAdjustment", 0);
            SetEnumArrayField(recipe, "OnlyForSlots", "Weapon");
            SetEnumArrayField(recipe, "Restrictions", "Weapon");
            SetField(recipe, "CanApplyToMundaneItem", false);
            return recipe;
        }

        private static object CreateAmmunitionRecipe(BlueprintItem item)
        {
            object recipe = Activator.CreateInstance(_contract.RecipeDataType);
            SetField(recipe, "Name", "KMGAmmunition." + item.AssetGuid);
            SetField(recipe, "NameId", item.Name);
            SetRecipeResult(recipe, item);
            SetEmptyArrayField(recipe, "PrerequisiteSpells");
            SetEnumField(recipe, "CostType", "Flat");
            SetField(recipe, "CostFactor", 0);
            SetField(recipe, "CostAdjustment", 0);
            return recipe;
        }

        private static AmmunitionRenderPlan CreateAmmunitionRenderPlan(
            CraftMagicItemsContract contract, object ammunition,
            object[] recipes, CraftMagicItemsRegistrationCatalog catalog)
        {
            if (contract == null || ammunition == null || recipes == null ||
                catalog == null || !contract.RecipeBasedType.IsInstanceOfType(
                    ammunition) || recipes.Length != catalog.Ammunition.Length ||
                recipes.Length != 3 || ReadInt(ammunition, "Count") !=
                    CraftMagicItemsCompatibilityPolicy.AmmunitionBatchCount)
                throw new InvalidOperationException(
                    "CMI ammunition UI plan inputs changed.");

            var results = new BlueprintItem[recipes.Length];
            var names = new string[recipes.Length];
            var descriptions = new string[recipes.Length];
            for (int index = 0; index < recipes.Length; index++)
            {
                if (!contract.RecipeDataType.IsInstanceOfType(recipes[index]))
                    throw new InvalidOperationException(
                        "CMI ammunition UI recipe type changed.");
                BlueprintItem item = ReadRecipeResult(recipes[index]);
                BlueprintItem expected = catalog.Ammunition[index].Item;
                if (item == null || !ReferenceEquals(item, expected) ||
                    item is BlueprintItemEquipment ||
                    string.IsNullOrWhiteSpace(item.Name))
                    throw new InvalidOperationException(
                        "CMI ammunition UI recipe lost its exact plain BlueprintItem result.");
                results[index] = item;
                names[index] = item.Name;
                descriptions[index] = item.Description ?? string.Empty;
            }
            if (results.Select(value => value.AssetGuid).Distinct(
                    StringComparer.Ordinal).Count() != results.Length)
                throw new InvalidOperationException(
                    "CMI ammunition UI result identities are not unique.");

            ParameterInfo[] skill = contract.RenderCraftingSkill
                .GetParameters();
            ParameterInfo[] control = contract.RenderCraftControl
                .GetParameters();
            if (skill.Length != 9 ||
                !skill[0].ParameterType.IsAssignableFrom(
                    typeof(UnitEntityData)) ||
                skill[1].ParameterType != typeof(StatType) ||
                skill[2].ParameterType != typeof(int) ||
                skill[3].ParameterType != typeof(int) ||
                skill[6].ParameterType != typeof(bool) ||
                skill[8].ParameterType != typeof(bool) ||
                control.Length != 6 ||
                !control[0].ParameterType.IsAssignableFrom(
                    typeof(UnitEntityData)) ||
                !control[1].ParameterType.IsInstanceOfType(ammunition) ||
                !control[2].ParameterType.IsInstanceOfType(recipes[0]) ||
                control[3].ParameterType != typeof(int) ||
                !control[4].ParameterType.IsAssignableFrom(
                    typeof(BlueprintItem)))
                throw new InvalidOperationException(
                    "CMI ammunition lower-panel method arguments changed.");

            return new AmmunitionRenderPlan(ammunition, recipes, results,
                names, descriptions, contract.DrawSelection,
                contract.RenderCraftingSkill, contract.RenderCraftControl,
                contract.MundaneUiAnchor.LabelRenderer,
                contract.RecipeBasedType, contract.RecipeDataType);
        }

        private static Array AppendItemData(Array source, object[] additions)
        {
            var values = source.Cast<object>().ToList();
            foreach (object addition in additions)
            {
                string identity = ReadString(addition, "Name");
                object[] matches = values.Where(value => string.Equals(
                    ReadString(value, "Name"), identity,
                    StringComparison.Ordinal)).ToArray();
                if (matches.Length == 0) values.Add(addition);
                else if (matches.Length != 1 ||
                    !ReferenceEquals(matches[0], addition))
                    throw new InvalidOperationException(
                        "CMI item-type registration identity collision: " +
                        identity);
            }
            Array result = Array.CreateInstance(_contract.ItemDataType,
                values.Count);
            for (int index = 0; index < values.Count; index++)
                result.SetValue(values[index], index);
            return result;
        }

        private static object RequireItemData(Array graph, string name)
        {
            object[] matches = graph.Cast<object>().Where(value =>
                string.Equals(ReadString(value, "Name"), name,
                    StringComparison.Ordinal)).ToArray();
            if (matches.Length != 1 ||
                !_contract.RecipeBasedType.IsInstanceOfType(matches[0]))
                throw new InvalidOperationException(
                    "CMI item data identity is missing or ambiguous: " + name);
            return matches[0];
        }

        private static void AppendNewItemBases(object itemData,
            IEnumerable<BlueprintItemWeapon> additions)
        {
            BlueprintItemEquipment[] existing = ReadNewItemBases(itemData);
            BlueprintItemEquipment[] merged =
                CraftMagicItemsCompatibilityPolicy.MergeExactlyOnce(existing,
                    additions.Cast<BlueprintItemEquipment>(),
                    value => value.AssetGuid);
            if (merged.Length == existing.Length) return;

            FieldInfo field = RequireField(itemData.GetType().BaseType,
                "m_NewItemBaseIDs");
            Array raw = field.GetValue(itemData) as Array;
            Type rowType = field.FieldType.GetElementType();
            var rows = new List<object>();
            if (raw != null) rows.AddRange(raw.Cast<object>());
            HashSet<string> seen = new HashSet<string>(existing.Select(value =>
                value.AssetGuid), StringComparer.Ordinal);
            foreach (BlueprintItemEquipment addition in additions)
                if (seen.Add(addition.AssetGuid))
                    rows.Add(CreateBlueprintRow(rowType, addition));
            Array result = Array.CreateInstance(rowType, rows.Count);
            for (int index = 0; index < rows.Count; index++)
                result.SetValue(rows[index], index);
            field.SetValue(itemData, result);
            ResetNewItemBaseCache(itemData);
        }

        private static void SetNewItemBases(object itemData,
            IEnumerable<BlueprintItemEquipment> bases)
        {
            FieldInfo field = RequireField(itemData.GetType().BaseType,
                "m_NewItemBaseIDs");
            if (bases == null)
            {
                field.SetValue(itemData, null);
                ResetNewItemBaseCache(itemData);
                return;
            }
            BlueprintItemEquipment[] values = bases.ToArray();
            Type rowType = field.FieldType.GetElementType();
            Array result = Array.CreateInstance(rowType, values.Length);
            for (int index = 0; index < values.Length; index++)
                result.SetValue(CreateBlueprintRow(rowType, values[index]),
                    index);
            field.SetValue(itemData, result);
            ResetNewItemBaseCache(itemData);
        }

        private static object CreateBlueprintRow(Type rowType, object blueprint)
        {
            Type wrapperType = rowType.GetElementType();
            Array row = Array.CreateInstance(wrapperType, 1);
            row.SetValue(Activator.CreateInstance(wrapperType,
                new[] { blueprint }), 0);
            return row;
        }

        private static void ResetNewItemBaseCache(object itemData)
        {
            RequireField(itemData.GetType().BaseType,
                "m_CachedNewItemBaseIDs").SetValue(itemData, null);
        }

        private static NewItemBaseState CaptureNewItemBaseState(
            object itemData)
        {
            Type baseType = itemData.GetType().BaseType;
            FieldInfo raw = RequireField(baseType, "m_NewItemBaseIDs");
            FieldInfo cache = RequireField(baseType,
                "m_CachedNewItemBaseIDs");
            return new NewItemBaseState(raw, raw.GetValue(itemData), cache,
                cache.GetValue(itemData));
        }

        private static void TryRestoreNewItemBaseState(object itemData,
            NewItemBaseState state)
        {
            if (itemData == null || state == null) return;
            try
            {
                state.RawField.SetValue(itemData, state.RawValue);
                state.CacheField.SetValue(itemData, state.CacheValue);
            }
            catch
            {
                // The bridge is already failing closed. Avoid masking the
                // bounded contract diagnostic with a secondary rollback error.
            }
        }

        private static BlueprintItemEquipment[] ReadNewItemBases(object data)
        {
            PropertyInfo property = data.GetType().GetProperty(
                "NewItemBaseIDs", Fields) ?? data.GetType().BaseType
                .GetProperty("NewItemBaseIDs", Fields);
            return property == null ? new BlueprintItemEquipment[0] :
                (property.GetValue(data, null) as BlueprintItemEquipment[]) ??
                    new BlueprintItemEquipment[0];
        }

        private static void SetRecipes(object data, object[] recipes)
        {
            FieldInfo field = RequireField(data.GetType(), "Recipes");
            Array array = Array.CreateInstance(_contract.RecipeDataType,
                recipes.Length);
            for (int index = 0; index < recipes.Length; index++)
                array.SetValue(recipes[index], index);
            field.SetValue(data, array);
            FieldInfo subField = RequireField(data.GetType(), "SubRecipes");
            object dictionary = Activator.CreateInstance(subField.FieldType);
            IDictionary asDictionary = dictionary as IDictionary;
            Type listType = subField.FieldType.GetGenericArguments()[1];
            foreach (object recipe in recipes)
            {
                string parent = ReadString(recipe, "ParentNameId");
                if (parent == null) continue;
                IList list = asDictionary[parent] as IList;
                if (list == null)
                {
                    list = Activator.CreateInstance(listType) as IList;
                    asDictionary.Add(parent, list);
                }
                list.Add(recipe);
            }
            subField.SetValue(data, dictionary);
        }

        private static void SynchronizeMundaneIndexes(
            CraftMagicItemsContract contract,
            CraftMagicItemsRegistrationCatalog catalog, object mundane)
        {
            IDictionary sub = contract.SubCraftingDataField.GetValue(null) as
                IDictionary;
            if (sub == null) throw new InvalidOperationException(
                "CMI SubCraftingData was not initialized before its equipment index.");
            if (mundane != null)
            {
                string parent = ReadString(mundane, "ParentNameId");
                if (string.IsNullOrWhiteSpace(parent))
                    throw new InvalidOperationException(
                        "The CMI mundane Firearms parent is unavailable.");
                IList children = sub.Contains(parent) ? sub[parent] as IList :
                    null;
                if (children == null)
                {
                    Type listType = contract.SubCraftingDataField.FieldType
                        .GetGenericArguments()[1];
                    children = Activator.CreateInstance(listType) as IList;
                    if (children == null) throw new InvalidOperationException(
                        "CMI SubCraftingData list shape changed.");
                    sub.Add(parent, children);
                }
                if (!children.Cast<object>().Any(value => ReferenceEquals(
                        value, mundane))) children.Add(mundane);
            }

            IDictionary typeIndex = contract.TypeToItemField.GetValue(null) as
                IDictionary;
            if (typeIndex == null) throw new InvalidOperationException(
                "CMI TypeToItem was not initialized before its equipment index.");
            foreach (BlueprintItemWeapon weapon in catalog
                .FirearmRecognitionBases.Concat(catalog
                    .CustomFamilyRecognitionBases))
            {
                string typeGuid = weapon.Type == null ? null :
                    weapon.Type.AssetGuid;
                if (string.IsNullOrWhiteSpace(typeGuid))
                    throw new InvalidOperationException(
                        "A KMG creation base has no exact weapon-type identity.");
                if (typeIndex.Contains(typeGuid))
                {
                    if (!ReferenceEquals(typeIndex[typeGuid], weapon))
                        throw new InvalidOperationException(
                            "CMI already indexed a different base for KMG weapon type " +
                            typeGuid + ".");
                    continue;
                }
                typeIndex.Add(typeGuid, weapon);
            }
        }

        private static object[] ReadRecipes(object data)
        {
            if (data == null) return new object[0];
            return (RequireField(data.GetType(), "Recipes").GetValue(data) as
                Array)?.Cast<object>().ToArray() ?? new object[0];
        }

        private static bool RecipeSupportsWeapon(object recipe)
        {
            Array slots = RequireField(recipe.GetType(), "OnlyForSlots")
                .GetValue(recipe) as Array;
            return slots == null || slots.Cast<object>().Any(value =>
                string.Equals(value.ToString(), "Weapon",
                    StringComparison.Ordinal));
        }

        private static void SetRecipeEnchantments(object recipe,
            BlueprintItemEnchantment[] enchantments)
        {
            FieldInfo field = RequireField(recipe.GetType(), "m_Enchantments");
            Type rowType = field.FieldType.GetElementType();
            Array result = Array.CreateInstance(rowType, enchantments.Length);
            for (int index = 0; index < enchantments.Length; index++)
                result.SetValue(CreateBlueprintRow(rowType,
                    enchantments[index]), index);
            field.SetValue(recipe, result);
            FieldInfo cache = recipe.GetType().GetField(
                "m_CachedEnchantments", Fields);
            if (cache != null) cache.SetValue(recipe, null);
        }

        private static BlueprintItemEnchantment[] ReadRecipeEnchantments(
            object recipe)
        {
            PropertyInfo property = recipe.GetType().GetProperty(
                "Enchantments", Fields);
            return property == null ? new BlueprintItemEnchantment[0] :
                (property.GetValue(recipe, null) as
                    BlueprintItemEnchantment[]) ??
                    new BlueprintItemEnchantment[0];
        }

        private static void SetRecipeResult(object recipe, BlueprintItem item)
        {
            FieldInfo field = RequireField(recipe.GetType(), "m_ResultItem");
            Type wrapperType = field.FieldType.GetElementType();
            Array result = Array.CreateInstance(wrapperType, 1);
            result.SetValue(Activator.CreateInstance(wrapperType,
                new object[] { item }), 0);
            field.SetValue(recipe, result);
        }

        private static BlueprintItem ReadRecipeResult(object recipe)
        {
            PropertyInfo property = recipe.GetType().GetProperty("ResultItem",
                Fields);
            return property == null ? null : property.GetValue(recipe, null) as
                BlueprintItem;
        }

        private static IEnumerable<object> TopLevelMundane(Array graph,
            CraftMagicItemsContract contract)
        {
            IDictionary sub = contract.SubCraftingDataField.GetValue(null) as
                IDictionary;
            foreach (object value in graph)
            {
                string nameId = ReadString(value, "NameId");
                string feat = ReadString(value, "FeatGuid");
                string parent = ReadString(value, "ParentNameId");
                if (nameId == null || feat != null) continue;
                if (parent == null)
                {
                    yield return value;
                    continue;
                }
                IList children = sub == null ? null : sub[parent] as IList;
                if (children != null && children.Count > 0 &&
                    ReferenceEquals(children[0], value)) yield return value;
            }
        }

        private static string ResolveLocalizedText(string key)
        {
            if (string.IsNullOrEmpty(key)) return key;
            Type l10n = _contract.Assembly.GetType(
                "CraftMagicItems.L10NString", false, false);
            object value = l10n == null ? null : Activator.CreateInstance(l10n,
                new object[] { key });
            return value == null ? key : value.ToString();
        }

        private static BlueprintItemWeapon ResolveWeapon(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid)) return null;
            BlueprintItemWeapon result = ResourcesLibrary
                .TryGetBlueprint<BlueprintItemWeapon>(guid);
            if (result != null) return result;
            int marker = guid.IndexOf("#CraftMagicItems",
                StringComparison.Ordinal);
            return marker <= 0 ? null : ResourcesLibrary
                .TryGetBlueprint<BlueprintItemWeapon>(guid.Substring(0,
                    marker));
        }

        private static bool IsFirearm(BlueprintItemWeapon weapon)
        { return CraftMagicItemsRegistrationCatalog.IsFirearm(weapon); }

        private static bool IsRecognizedCustomFamily(
            BlueprintItemWeapon weapon)
        {
            CraftMagicItemsRegistrationCatalog catalog;
            lock (Gate) catalog = _catalog;
            return weapon != null && weapon.Type != null &&
                catalog != null && catalog.CustomFamilyRecognitionBases.Any(
                    value => value.Type != null && ReferenceEquals(
                        value.Type, weapon.Type));
        }

        private static void RegisterLocalization()
        {
            LocalizationService.Create(
                "KMG.CraftMagicItems.Firearms.Name", "Firearms");
            LocalizationService.Create(
                "KMG.CraftMagicItems.Ammunition.Name",
                "Firearm Ammunition");
        }

        private static int CountAddedItemTypes()
        {
            int result = 1;
            if (_mundaneFirearms != null) result++;
            if (_ammunition != null) result++;
            return result;
        }

        private static CraftMagicItemsGraphSnapshot EmptySnapshot(
            int generation)
        {
            return new CraftMagicItemsGraphSnapshot(generation, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, null, null, null, null, null,
                null);
        }

        private static void SetField(object target, string name, object value)
        {
            FieldInfo field = RequireField(target.GetType(), name);
            if (value != null && !field.FieldType.IsInstanceOfType(value))
                throw new InvalidOperationException("CMI field type changed: " +
                    target.GetType().FullName + "." + name);
            field.SetValue(target, value);
        }

        private static string ReadString(object target, string name)
        { return RequireField(target.GetType(), name).GetValue(target) as string; }

        private static int ReadInt(object target, string name)
        { return (int)RequireField(target.GetType(), name).GetValue(target); }

        private static bool ReadBool(object target, string name)
        { return (bool)RequireField(target.GetType(), name).GetValue(target); }

        private static FieldInfo RequireField(Type type, string name)
        {
            for (Type current = type; current != null;
                current = current.BaseType)
            {
                FieldInfo value = current.GetField(name, Fields |
                    BindingFlags.DeclaredOnly);
                if (value != null) return value;
            }
            throw new MissingFieldException(type.FullName, name);
        }

        private static void SetEnumProperty(object target, string name,
            string value)
        {
            PropertyInfo property = target.GetType().GetProperty(name, Fields)
                ?? target.GetType().BaseType.GetProperty(name, Fields);
            if (property == null || !property.PropertyType.IsEnum)
                throw new MissingMemberException(target.GetType().FullName,
                    name);
            property.SetValue(target, Enum.Parse(property.PropertyType, value),
                null);
        }

        private static void SetEnumField(object target, string name,
            string value)
        {
            FieldInfo field = RequireField(target.GetType(), name);
            if (!field.FieldType.IsEnum) throw new MissingFieldException(
                target.GetType().FullName, name);
            field.SetValue(target, Enum.Parse(field.FieldType, value));
        }

        private static void SetEnumArrayField(object target, string name,
            params string[] values)
        {
            FieldInfo field = RequireField(target.GetType(), name);
            Type element = field.FieldType.GetElementType();
            if (!field.FieldType.IsArray || element == null || !element.IsEnum)
                throw new MissingFieldException(target.GetType().FullName,
                    name);
            Array result = Array.CreateInstance(element, values.Length);
            for (int index = 0; index < values.Length; index++)
                result.SetValue(Enum.Parse(element, values[index]), index);
            field.SetValue(target, result);
        }

        private static void SetEmptyArrayField(object target, string name)
        {
            FieldInfo field = RequireField(target.GetType(), name);
            Type element = field.FieldType.GetElementType();
            if (!field.FieldType.IsArray || element == null)
                throw new MissingFieldException(target.GetType().FullName,
                    name);
            field.SetValue(target, Array.CreateInstance(element, 0));
        }

        private static object Default(Type type)
        { return type.IsValueType ? Activator.CreateInstance(type) : null; }

        internal static bool ProcessDeferredUiFailure()
        {
            DeferredUiFailure failure;
            ModContext context;
            lock (Gate)
            {
                failure = _deferredUiFailure;
                if (failure == null) return false;
                _deferredUiFailure = null;
                _failed = true;
                _finalized = false;
                _uiFaulted = true;
                context = _context;
            }
            RollbackCompatibilityGraph();
            CraftMagicItemsCompatibilityStatusRegistry.Update(
                new CraftMagicItemsCompatibilityStatus(
                    CraftMagicItemsCompatibilityAvailability.BridgeFaulted,
                    failure.Phase + ":" + failure.Root.GetType().FullName,
                    0, 0, 0));
            if (context != null) context.Logger.Warning("craft-magic-items",
                "bridge.ui-disabled", "phase=" + failure.Phase +
                ";lifecycle=OnUpdate;graphRollback=true;cause=" +
                failure.Chain);
            return true;
        }

        private static void ReportUiBoundaryFailure(string phase,
            Exception exception)
        {
            CraftMagicItemsUiFailureCapture failure =
                CraftMagicItemsUiFailurePolicy.Capture(exception);
            Exception root = failure.Root;
            string chain = failure.ExceptionChain;
            ModContext context;
            bool log;
            lock (Gate)
            {
                context = _context;
                _uiFailureCount++;
                if (_deferredUiFailure == null)
                    _deferredUiFailure = new DeferredUiFailure(phase, root,
                        chain);
                log = !_uiFailureLogged;
                _uiFailureLogged = true;
            }
            if (log && context != null) context.Logger.Failure(
                "craft-magic-items", "bridge.ui-failure.queued",
                "phase=" + phase +
                ";graphMutation=false;rollbackLifecycle=OnUpdate;" +
                "exceptionChain=" + chain, root);
        }

        private static void ObserveUiEventNoLock(string route)
        {
            string observed;
            try
            {
                object value = _eventCurrentProperty == null ? null :
                    _eventCurrentProperty.GetValue(null, null);
                object eventType = value == null || _eventTypeProperty == null ?
                    null : _eventTypeProperty.GetValue(value, null);
                observed = eventType == null ? "unavailable" :
                    eventType.ToString();
            }
            catch
            {
                observed = "unavailable";
            }
            UiEventTypes.Add(observed);
            if (UiRouteObservations.Count == 256)
                UiRouteObservations.RemoveAt(0);
            UiRouteObservations.Add((route ?? "unknown") + ":" + observed);
        }

        private static void InitializeUiEventObservation()
        {
            Type eventType = AppDomain.CurrentDomain.GetAssemblies()
                .Where(value => string.Equals(value.GetName().Name,
                    "UnityEngine.IMGUIModule", StringComparison.Ordinal))
                .Select(value => value.GetType("UnityEngine.Event", false,
                    false)).SingleOrDefault(value => value != null);
            PropertyInfo current = eventType == null ? null :
                eventType.GetProperty("current", BindingFlags.Static |
                    BindingFlags.Public);
            PropertyInfo type = eventType == null ? null :
                eventType.GetProperty("type", BindingFlags.Instance |
                    BindingFlags.Public);
            lock (Gate)
            {
                _eventCurrentProperty = current;
                _eventTypeProperty = type;
            }
        }

        private static void Fail(string phase, Exception exception)
        {
            ModContext context;
            bool log;
            lock (Gate)
            {
                _failed = true;
                _finalized = false;
                context = _context;
                log = !_bridgeFailureLogged;
                _bridgeFailureLogged = true;
            }
            RollbackCompatibilityGraph();
            CraftMagicItemsCompatibilityStatusRegistry.Update(
                new CraftMagicItemsCompatibilityStatus(
                    CraftMagicItemsCompatibilityAvailability.Incompatible,
                    phase + ":" + exception.GetType().FullName, 0, 0, 0));
            if (log && context != null) context.Logger.Failure("craft-magic-items",
                "bridge.incompatible", "phase=" + phase +
                ";the optional bridge was disabled without failing KMG.",
                exception);
        }

        private static void RollbackCompatibilityGraph()
        {
            CraftMagicItemsContract contract;
            CraftMagicItemsRegistrationCatalog catalog;
            Array current;
            Array raw;
            object mundane;
            object reliable;
            object martial;
            object exotic;
            NewItemBaseState martialState;
            NewItemBaseState exoticState;
            lock (Gate)
            {
                _graphRollbackCount++;
                contract = _contract;
                catalog = _catalog;
                current = _currentGraph;
                raw = _rawGraph;
                mundane = _mundaneFirearms;
                reliable = _reliableRecipe;
                martial = _martialData;
                exotic = _exoticData;
                martialState = _martialState;
                exoticState = _exoticState;
            }
            if (contract == null) return;
            try
            {
                TryRestoreNewItemBaseState(martial, martialState);
                TryRestoreNewItemBaseState(exotic, exoticState);
                if (current != null && raw != null && ReferenceEquals(
                        contract.ItemDataField.GetValue(null), current))
                    contract.ItemDataField.SetValue(null, raw);

                IDictionary sub = contract.SubCraftingDataField.GetValue(null)
                    as IDictionary;
                if (sub != null && mundane != null)
                {
                    string parent = ReadString(mundane, "ParentNameId");
                    IList children = parent == null ? null : sub[parent] as IList;
                    if (children != null)
                        for (int index = children.Count - 1; index >= 0;
                            index--)
                            if (ReferenceEquals(children[index], mundane))
                                children.RemoveAt(index);
                }

                IDictionary typeIndex = contract.TypeToItemField.GetValue(null)
                    as IDictionary;
                if (typeIndex != null && catalog != null)
                    foreach (BlueprintItemWeapon weapon in catalog
                        .FirearmRecognitionBases.Concat(catalog
                            .CustomFamilyRecognitionBases))
                        if (weapon.Type != null &&
                            typeIndex.Contains(weapon.Type.AssetGuid) &&
                            ReferenceEquals(typeIndex[weapon.Type.AssetGuid],
                                weapon))
                            typeIndex.Remove(weapon.Type.AssetGuid);

                IDictionary recipeIndex = contract.EnchantmentToRecipeField
                    .GetValue(null) as IDictionary;
                if (recipeIndex != null && catalog != null &&
                    recipeIndex.Contains(catalog.Reliable.AssetGuid))
                {
                    IList recipes = recipeIndex[catalog.Reliable.AssetGuid]
                        as IList;
                    if (recipes != null && reliable != null)
                        for (int index = recipes.Count - 1; index >= 0;
                            index--)
                            if (ReferenceEquals(recipes[index], reliable))
                                recipes.RemoveAt(index);
                    if (recipes == null || recipes.Count == 0)
                        recipeIndex.Remove(catalog.Reliable.AssetGuid);
                }
            }
            catch
            {
                // A bridge already marked incompatible must never throw into
                // CMI or KMG. The one bounded primary diagnostic remains the
                // actionable failure record.
            }
            finally
            {
                lock (Gate)
                {
                    _currentGraph = null;
                    _rawGraph = null;
                    _magicFirearms = null;
                    _mundaneFirearms = null;
                    _ammunition = null;
                    _reliableRecipe = null;
                    _ammunitionUiPlan = null;
                    _magicFeatGuid = null;
                    _martialData = null;
                    _exoticData = null;
                    _martialState = null;
                    _exoticState = null;
                    _categoryScope = CategoryScope.None;
                    _snapshot = EmptySnapshot(_generation);
                }
            }
        }

        internal enum CategoryScope
        {
            None = 0,
            Firearms = 1
        }

        private sealed class AmmunitionRenderPlan
        {
            internal AmmunitionRenderPlan(object craftingData,
                object[] recipes, BlueprintItem[] results, string[] names,
                string[] descriptions, MethodInfo drawSelection,
                MethodInfo renderCraftingSkill, MethodInfo renderCraftControl,
                MethodInfo renderLabel, Type craftingDataType,
                Type recipeType)
            {
                CraftingData = craftingData;
                Recipes = recipes;
                Results = results;
                Names = names;
                Descriptions = descriptions;
                DrawSelection = drawSelection;
                RenderCraftingSkill = renderCraftingSkill;
                RenderCraftControl = renderCraftControl;
                RenderLabel = renderLabel;
                CraftingDataType = craftingDataType;
                RecipeType = recipeType;
            }

            internal object CraftingData { get; private set; }
            internal object[] Recipes { get; private set; }
            internal BlueprintItem[] Results { get; private set; }
            internal string[] Names { get; private set; }
            internal string[] Descriptions { get; private set; }
            internal MethodInfo DrawSelection { get; private set; }
            internal MethodInfo RenderCraftingSkill { get; private set; }
            internal MethodInfo RenderCraftControl { get; private set; }
            internal MethodInfo RenderLabel { get; private set; }
            private Type CraftingDataType { get; set; }
            private Type RecipeType { get; set; }

            internal void Preflight(UnitEntityData crafter, object selected)
            {
                if (!ReferenceEquals(selected, CraftingData) ||
                    !CraftingDataType.IsInstanceOfType(CraftingData) ||
                    Recipes == null || Results == null || Names == null ||
                    Descriptions == null || Recipes.Length != 3 ||
                    Results.Length != Recipes.Length || Names.Length !=
                    Recipes.Length || Descriptions.Length != Recipes.Length ||
                    DrawSelection == null || RenderCraftingSkill == null ||
                    RenderCraftControl == null || RenderLabel == null)
                    throw new InvalidOperationException(
                        "The immutable CMI ammunition UI plan is invalid.");
                for (int index = 0; index < Recipes.Length; index++)
                    if (!RecipeType.IsInstanceOfType(Recipes[index]) ||
                        Results[index] == null ||
                        string.IsNullOrWhiteSpace(Names[index]) ||
                        Descriptions[index] == null)
                        throw new InvalidOperationException(
                            "The immutable CMI ammunition UI entry is invalid.");
            }
        }

        private sealed class DeferredUiFailure
        {
            internal DeferredUiFailure(string phase, Exception root,
                string chain)
            {
                Phase = phase ?? "ammunition-ui";
                Root = root ?? new InvalidOperationException(
                    "Unknown CMI ammunition UI failure.");
                Chain = chain ?? string.Empty;
            }

            internal string Phase { get; private set; }
            internal Exception Root { get; private set; }
            internal string Chain { get; private set; }
        }

        private sealed class NewItemBaseState
        {
            internal NewItemBaseState(FieldInfo rawField, object rawValue,
                FieldInfo cacheField, object cacheValue)
            {
                RawField = rawField;
                RawValue = rawValue;
                CacheField = cacheField;
                CacheValue = cacheValue;
            }

            internal FieldInfo RawField { get; private set; }
            internal object RawValue { get; private set; }
            internal FieldInfo CacheField { get; private set; }
            internal object CacheValue { get; private set; }
        }
    }
}
