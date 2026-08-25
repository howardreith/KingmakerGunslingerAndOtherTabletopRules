using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.CraftMagicItemsCompatibility
{
    internal sealed class CraftMagicItemsAmmunitionCraftObservation
    {
        internal string ItemGuid { get; set; }
        internal int ExpectedCount { get; set; }
        internal int ExpectedProgress { get; set; }
        internal int ExpectedGold { get; set; }
        internal int InventoryBefore { get; set; }
        internal int InventoryAfter { get; set; }
        internal long MoneyBefore { get; set; }
        internal long MoneyAfter { get; set; }
        internal bool ButtonTriggered { get; set; }
        internal bool Timed { get; set; }
        internal bool ProjectCreated { get; set; }
        internal int ProjectTarget { get; set; }
        internal int ProjectGold { get; set; }
        internal int ProjectResultCount { get; set; }
        internal string ProjectResultGuid { get; set; }
        internal bool ProjectCompleted { get; set; }
    }

    /// <summary>
    /// Request-local adapter for the guarded real-assembly UI observer. It
    /// mutates only CMI selection state, restores that state exactly, and
    /// installs one temporary crafter-result patch under a unique owner.
    /// </summary>
    internal sealed class CraftMagicItemsAmmunitionUiRuntimeAdapter :
        IDisposable
    {
        private const BindingFlags Static = BindingFlags.Static |
            BindingFlags.Public | BindingFlags.NonPublic;
        private const string SelectionLabel = "Mundane Crafting: ";
        private const string ItemLabel = "Item: ";
        private const string SentinelName =
            "KMG_AMMUNITION_UI_UNRELATED_STATE_SENTINEL";
        private static readonly object Gate = new object();
        private static UnitEntityData _runtimeCrafter;
        private static bool _active;
        private static string _expectedCraftItemName;
        private static bool _craftClickArmed;
        private static bool _craftClickTriggered;
        private static bool _forcePlayerInCapital;
        private static PropertyInfo _eventCurrentProperty;
        private static PropertyInfo _eventTypeProperty;

        private readonly CraftMagicItemsContract _contract;
        private readonly IDictionary _selections;
        private readonly Dictionary<object, object> _selectionSnapshot;
        private readonly object _upgradingSnapshot;
        private readonly object _customNameSnapshot;
        private readonly object _ammunition;
        private readonly int _ammunitionIndex;
        private readonly int _ordinaryIndex;
        private readonly BlueprintItemEquipment _sentinelBlueprint;
        private readonly object _harmony;
        private readonly MethodInfo _unpatchAll;
        private readonly string _owner;
        private readonly object _settings;
        private readonly FieldInfo _takesNoTimeField;
        private readonly FieldInfo _costsNoGoldField;
        private readonly FieldInfo _priceScaleField;
        private readonly object _takesNoTimeSnapshot;
        private readonly object _costsNoGoldSnapshot;
        private readonly object _priceScaleSnapshot;
        private readonly FieldInfo _currentSectionField;
        private readonly object _currentSectionSnapshot;
        private readonly IList _creationProjects;
        private readonly object[] _creationProjectsSnapshot;
        private readonly MethodInfo _cancelProject;
        private readonly MethodInfo _getTimer;
        private readonly MethodInfo _workOnProjects;
        private bool _sentinelActive;
        private bool _disposed;

        private CraftMagicItemsAmmunitionUiRuntimeAdapter(
            CraftMagicItemsContract contract, UnitEntityData crafter,
            string owner)
        {
            _contract = contract;
            _owner = owner;
            _selections = contract.SelectedIndexField.GetValue(null) as
                IDictionary;
            if (_selections == null) throw new InvalidOperationException(
                "CMI SelectedIndex is unavailable to the guarded UI observer.");
            _selectionSnapshot = new Dictionary<object, object>();
            foreach (DictionaryEntry entry in _selections)
                _selectionSnapshot.Add(entry.Key, entry.Value);
            _upgradingSnapshot = contract.UpgradingBlueprintField.GetValue(null);
            _customNameSnapshot = contract.SelectedCustomNameField.GetValue(null);

            Array graph = contract.ItemDataField.GetValue(null) as Array;
            if (graph == null) throw new InvalidOperationException(
                "CMI item data is unavailable to the guarded UI observer.");
            object[] ammunitionMatches = graph.Cast<object>().Where(value =>
                string.Equals(ReadString(value, "Name"),
                    CraftMagicItemsReflectionBridge.AmmunitionIdentity,
                    StringComparison.Ordinal)).ToArray();
            if (ammunitionMatches.Length != 1)
                throw new InvalidOperationException(
                    "The exact KMG ammunition data object was missing or ambiguous.");
            _ammunition = ammunitionMatches[0];
            object[] topLevel = TopLevelMundane(graph, contract).ToArray();
            int[] ammunitionIndexes = topLevel.Select((value, index) => new {
                    value, index }).Where(value => ReferenceEquals(value.value,
                        _ammunition)).Select(value => value.index).ToArray();
            int[] ordinaryIndexes = topLevel.Select((value, index) => new {
                    value, index }).Where(value => !ReferenceEquals(value.value,
                        _ammunition) && contract.RecipeBasedType
                        .IsInstanceOfType(value.value))
                .Select(value => value.index).ToArray();
            if (ammunitionIndexes.Length != 1 || ordinaryIndexes.Length == 0)
                throw new InvalidOperationException(
                    "CMI top-level mundane routes were missing or ambiguous.");
            _ammunitionIndex = ammunitionIndexes[0];
            _ordinaryIndex = ordinaryIndexes[0];
            CraftMagicItemsRegistrationCatalog catalog =
                CraftMagicItemsReflectionBridge.Catalog;
            _sentinelBlueprint = catalog == null ? null : catalog
                .FirearmCreationBases.FirstOrDefault();
            if (_sentinelBlueprint == null)
                throw new InvalidOperationException(
                    "No exact KMG equipment blueprint can guard unrelated CMI UI state.");

            FieldInfo settingsField = RequireField(contract.MainType,
                "ModSettings");
            _settings = settingsField.GetValue(null);
            if (_settings == null) throw new InvalidOperationException(
                "CMI settings are unavailable to the guarded UI observer.");
            _takesNoTimeField = RequireField(_settings.GetType(),
                "CraftingTakesNoTime");
            _costsNoGoldField = RequireField(_settings.GetType(),
                "CraftingCostsNoGold");
            _priceScaleField = RequireField(_settings.GetType(),
                "CraftingPriceScale");
            if (_takesNoTimeField.FieldType != typeof(bool) ||
                _costsNoGoldField.FieldType != typeof(bool) ||
                _priceScaleField.FieldType != typeof(float))
                throw new InvalidOperationException(
                    "CMI crafting settings shape changed.");
            _takesNoTimeSnapshot = _takesNoTimeField.GetValue(_settings);
            _costsNoGoldSnapshot = _costsNoGoldField.GetValue(_settings);
            _priceScaleSnapshot = _priceScaleField.GetValue(_settings);
            _currentSectionField = RequireField(contract.MainType,
                "currentSection");
            _currentSectionSnapshot = _currentSectionField.GetValue(null);
            FieldInfo creationProjectsField = RequireField(contract.MainType,
                "ItemCreationProjects");
            _creationProjects = creationProjectsField.GetValue(null) as IList;
            if (_creationProjects == null)
                throw new InvalidOperationException(
                    "CMI creation-project index is unavailable.");
            _creationProjectsSnapshot = _creationProjects.Cast<object>()
                .ToArray();
            _cancelProject = RequireMethod(contract.MainType,
                "CancelCraftingProject", 1, typeof(void));
            _getTimer = RequireMethod(contract.MainType,
                "GetCraftingTimerComponentForCaster", 2, null);
            _workOnProjects = RequireMethod(contract.MainType,
                "WorkOnProjects", 2, typeof(void));

            SetRuntimeCrafter(crafter);
            InstallRuntimePatches(contract, owner, out _harmony,
                out _unpatchAll);
        }

        internal static CraftMagicItemsAmmunitionUiRuntimeAdapter Begin(
            UnitEntityData crafter, string runId)
        {
            if (crafter == null || crafter.Descriptor == null ||
                string.IsNullOrWhiteSpace(runId))
                throw new ArgumentNullException("guarded CMI UI observer input");
            CraftMagicItemsContract contract =
                CraftMagicItemsOptionalExtensionCoordinator.Contract;
            if (contract == null ||
                !CraftMagicItemsReflectionBridge.IsFinalized)
                throw new InvalidOperationException(
                    "The finalized real CMI bridge is unavailable.");
            lock (Gate)
            {
                if (_active) throw new InvalidOperationException(
                    "A guarded CMI ammunition UI observer is already active.");
                _active = true;
                _expectedCraftItemName = null;
                _craftClickArmed = false;
                _craftClickTriggered = false;
                _forcePlayerInCapital = false;
            }
            try
            {
                return new CraftMagicItemsAmmunitionUiRuntimeAdapter(contract,
                    crafter, "KingmakerGunslinger.cmi-ammunition-ui-observer." +
                    runId);
            }
            catch
            {
                lock (Gate)
                {
                    _runtimeCrafter = null;
                    _active = false;
                }
                throw;
            }
        }

        internal void SelectOrdinary()
        {
            RequireActive();
            _contract.UpgradingBlueprintField.SetValue(null, null);
            _sentinelActive = false;
            _selections[SelectionLabel] = _ordinaryIndex;
        }

        internal void SelectAmmunition(int recipeIndex,
            bool preserveUnrelatedState)
        {
            RequireActive();
            if (recipeIndex < 0 || recipeIndex >= 3)
                throw new ArgumentOutOfRangeException("recipeIndex");
            _selections[SelectionLabel] = _ammunitionIndex;
            _selections[ItemLabel] = recipeIndex;
            if (preserveUnrelatedState)
            {
                _contract.UpgradingBlueprintField.SetValue(null,
                    _sentinelBlueprint);
                _contract.SelectedCustomNameField.SetValue(null,
                    SentinelName);
                _sentinelActive = true;
            }
            else
            {
                _contract.UpgradingBlueprintField.SetValue(null, null);
                _sentinelActive = false;
            }
        }

        internal void SetCrafter(UnitEntityData crafter)
        {
            RequireActive();
            if (crafter == null || crafter.Descriptor == null)
                throw new ArgumentNullException("crafter");
            SetRuntimeCrafter(crafter);
        }

        internal void SetNoCrafter()
        {
            RequireActive();
            SetRuntimeCrafter(null);
        }

        internal void ArmCraft(int recipeIndex, bool takesNoTime)
        {
            RequireActive();
            if (recipeIndex < 0 || recipeIndex >= 3)
                throw new ArgumentOutOfRangeException("recipeIndex");
            SelectAmmunition(recipeIndex, false);
            _takesNoTimeField.SetValue(_settings, takesNoTime);
            _costsNoGoldField.SetValue(_settings, false);
            _priceScaleField.SetValue(_settings, 1f);
            CraftMagicItemsRegistrationCatalog catalog =
                CraftMagicItemsReflectionBridge.Catalog;
            string itemName = catalog == null ? null :
                catalog.Ammunition[recipeIndex].Item.Name;
            if (string.IsNullOrWhiteSpace(itemName))
                throw new InvalidOperationException(
                    "The exact CMI ammunition craft label is unavailable.");
            lock (Gate)
            {
                _expectedCraftItemName = itemName;
                _craftClickTriggered = false;
                _craftClickArmed = true;
            }
        }

        internal bool CraftClickTriggered
        { get { lock (Gate) return _craftClickTriggered; } }

        internal CraftMagicItemsAmmunitionCraftObservation ObserveCraft(
            int recipeIndex, int inventoryBefore, long moneyBefore,
            bool timed)
        {
            RequireActive();
            CraftMagicItemsAmmunitionRegistration registration =
                CraftMagicItemsReflectionBridge.Catalog
                    .Ammunition[recipeIndex];
            object[] projects = NewCreationProjects();
            object project = projects.Length == 1 ? projects[0] : null;
            ItemEntity result = project == null ? null :
                ReadField(project, "ResultItem") as ItemEntity;
            return new CraftMagicItemsAmmunitionCraftObservation
            {
                ItemGuid = registration.Item.AssetGuid,
                ExpectedCount = registration.Plan.Count,
                ExpectedProgress = registration.Plan.RequiredProgress,
                ExpectedGold = registration.Plan.GoldCost(1f),
                InventoryBefore = inventoryBefore,
                InventoryAfter = Game.Instance.Player.Inventory.Count(
                    registration.Item),
                MoneyBefore = moneyBefore,
                MoneyAfter = Game.Instance.Player.Money,
                ButtonTriggered = CraftClickTriggered,
                Timed = timed,
                ProjectCreated = project != null,
                ProjectTarget = project == null ? -1 :
                    Convert.ToInt32(ReadField(project, "TargetCost")),
                ProjectGold = project == null ? -1 :
                    Convert.ToInt32(ReadField(project, "GoldSpent")),
                ProjectResultCount = result == null ? -1 : result.Count,
                ProjectResultGuid = result == null || result.Blueprint == null ?
                    string.Empty : result.Blueprint.AssetGuid
            };
        }

        internal void CompleteTimedProject(UnitEntityData crafter,
            CraftMagicItemsAmmunitionCraftObservation observation)
        {
            RequireActive();
            if (crafter == null || crafter.Descriptor == null ||
                observation == null || !observation.Timed)
                throw new ArgumentNullException("timed project input");
            object[] projects = NewCreationProjects();
            if (projects.Length != 1)
                throw new InvalidOperationException(
                    "The guarded timed craft did not create exactly one CMI project.");
            object timer = _getTimer.Invoke(null, new object[] {
                crafter.Descriptor, false });
            if (timer == null) throw new InvalidOperationException(
                "CMI did not attach its ordinary crafting timer.");
            FieldInfo lastUpdated = RequireField(timer.GetType(),
                "LastUpdated");
            FieldInfo projectsField = RequireField(timer.GetType(),
                "CraftingProjects");
            IList timerProjects = projectsField.GetValue(timer) as IList;
            if (lastUpdated.FieldType != typeof(TimeSpan) ||
                timerProjects == null || timerProjects.Cast<object>().Count(
                    value => ReferenceEquals(value, projects[0])) != 1)
                throw new InvalidOperationException(
                    "CMI timed-project timer shape changed.");
            ModifiableValue skill = crafter.Stats.GetStat(
                StatType.SkillKnowledgeWorld);
            int skillBefore = skill.BaseValue;
            try
            {
                skill.BaseValue = 100;
                lastUpdated.SetValue(timer, Game.Instance.Player.GameTime -
                    TimeSpan.FromDays(365));
                lock (Gate) _forcePlayerInCapital = true;
                _workOnProjects.Invoke(null, new object[] {
                    crafter.Descriptor, false });
            }
            finally
            {
                lock (Gate) _forcePlayerInCapital = false;
                skill.BaseValue = skillBefore;
            }
            observation.InventoryAfter = Game.Instance.Player.Inventory.Count(
                CraftMagicItemsReflectionBridge.Catalog.Ammunition.Single(
                    value => value.Item.AssetGuid == observation.ItemGuid).Item);
            observation.MoneyAfter = Game.Instance.Player.Money;
            observation.ProjectCompleted = !NewCreationProjects().Any() &&
                !timerProjects.Cast<object>().Any(value => ReferenceEquals(
                    value, projects[0]));
        }

        internal bool UnrelatedStatePreserved
        {
            get
            {
                return !_sentinelActive ||
                    (ReferenceEquals(_contract.UpgradingBlueprintField
                        .GetValue(null), _sentinelBlueprint) &&
                    string.Equals(_contract.SelectedCustomNameField
                        .GetValue(null) as string, SentinelName,
                        StringComparison.Ordinal));
            }
        }

        internal void InvokeRenderer()
        {
            RequireActive();
            _contract.RenderMundane.Invoke(null, null);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Exception failure = null;
            try
            {
                _unpatchAll.Invoke(_harmony, new object[] { _owner });
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            try
            {
                lock (Gate)
                {
                    _craftClickArmed = false;
                    _expectedCraftItemName = null;
                    _forcePlayerInCapital = false;
                }
                foreach (object project in NewCreationProjects())
                    _cancelProject.Invoke(null, new[] { project });
                _takesNoTimeField.SetValue(_settings, _takesNoTimeSnapshot);
                _costsNoGoldField.SetValue(_settings, _costsNoGoldSnapshot);
                _priceScaleField.SetValue(_settings, _priceScaleSnapshot);
                _currentSectionField.SetValue(null, _currentSectionSnapshot);
                _selections.Clear();
                foreach (KeyValuePair<object, object> entry in
                    _selectionSnapshot)
                    _selections.Add(entry.Key, entry.Value);
                _contract.UpgradingBlueprintField.SetValue(null,
                    _upgradingSnapshot);
                _contract.SelectedCustomNameField.SetValue(null,
                    _customNameSnapshot);
            }
            catch (Exception exception)
            {
                if (failure == null) failure = exception;
                else failure = new AggregateException(failure, exception);
            }
            finally
            {
                lock (Gate)
                {
                    _runtimeCrafter = null;
                    _active = false;
                }
            }
            if (failure != null) throw new InvalidOperationException(
                "Guarded CMI ammunition UI observer cleanup failed.",
                failure);
        }

        private void RequireActive()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
        }

        private static void SetRuntimeCrafter(UnitEntityData crafter)
        { lock (Gate) _runtimeCrafter = crafter; }

        private static bool RuntimeCrafterPrefix(ref UnitEntityData __result)
        {
            lock (Gate) __result = _runtimeCrafter;
            return false;
        }

        private static void InstallRuntimePatches(CraftMagicItemsContract contract,
            string owner, out object harmony, out MethodInfo unpatchAll)
        {
            Type harmonyType = contract.HarmonyInstanceField.FieldType;
            Type harmonyMethodType = harmonyType.Assembly.GetType(
                "HarmonyLib.HarmonyMethod", true, false);
            ConstructorInfo harmonyConstructor = harmonyType.GetConstructor(
                new[] { typeof(string) });
            ConstructorInfo methodConstructor = harmonyMethodType
                .GetConstructor(new[] { typeof(MethodInfo) });
            MethodInfo patch = harmonyType.GetMethods(BindingFlags.Instance |
                    BindingFlags.Public).Single(value => value.Name == "Patch" &&
                    value.GetParameters().Length == 5);
            unpatchAll = harmonyType.GetMethods(BindingFlags.Instance |
                    BindingFlags.Public).Single(value => value.Name ==
                    "UnpatchAll" && value.GetParameters().Length == 1 &&
                    value.GetParameters()[0].ParameterType == typeof(string));
            if (harmonyConstructor == null || methodConstructor == null)
                throw new MissingMemberException(
                    "Harmony 2 runtime observer constructors changed.");
            harmony = harmonyConstructor.Invoke(new object[] { owner });
            try
            {
                object prefix = methodConstructor.Invoke(new object[] {
                    typeof(CraftMagicItemsAmmunitionUiRuntimeAdapter).GetMethod(
                        "RuntimeCrafterPrefix", Static) });
                patch.Invoke(harmony, new[] { contract.GetSelectedCrafter,
                    prefix, null, null, null });

                Type eventType = AppDomain.CurrentDomain.GetAssemblies().Where(
                    value => string.Equals(value.GetName().Name,
                        "UnityEngine.IMGUIModule", StringComparison.Ordinal))
                .Select(value => value.GetType("UnityEngine.Event", false,
                    false)).SingleOrDefault(value => value != null);
                Type layoutType = eventType == null ? null :
                    eventType.Assembly.GetType("UnityEngine.GUILayout", false,
                        false);
                Type optionType = eventType == null ? null :
                    eventType.Assembly.GetType("UnityEngine.GUILayoutOption",
                        false, false);
                MethodInfo button = layoutType == null || optionType == null ?
                    null : layoutType.GetMethod("Button", BindingFlags.Static |
                        BindingFlags.Public, null, new[] { typeof(string),
                        optionType.MakeArrayType() }, null);
                _eventCurrentProperty = eventType == null ? null :
                    eventType.GetProperty("current", BindingFlags.Static |
                        BindingFlags.Public);
                _eventTypeProperty = eventType == null ? null :
                    eventType.GetProperty("type", BindingFlags.Instance |
                        BindingFlags.Public);
                MethodInfo playerInCapital = RequireMethod(contract.MainType,
                    "IsPlayerInCapital", 0, typeof(bool));
                if (button == null || _eventCurrentProperty == null ||
                    _eventTypeProperty == null)
                    throw new MissingMemberException(
                        "Unity IMGUI crafting-button observer seam changed.");
                object buttonPostfix = methodConstructor.Invoke(new object[] {
                    typeof(CraftMagicItemsAmmunitionUiRuntimeAdapter).GetMethod(
                        "CraftButtonPostfix", Static) });
                patch.Invoke(harmony, new[] { button, null, buttonPostfix,
                    null, null });
                object capitalPostfix = methodConstructor.Invoke(new object[] {
                    typeof(CraftMagicItemsAmmunitionUiRuntimeAdapter).GetMethod(
                        "PlayerInCapitalPostfix", Static) });
                patch.Invoke(harmony, new[] { playerInCapital, null,
                    capitalPostfix, null, null });
            }
            catch
            {
                try { unpatchAll.Invoke(harmony, new object[] { owner }); }
                catch { }
                throw;
            }
        }

        private static void CraftButtonPostfix(object[] __args,
            ref bool __result)
        {
            lock (Gate)
            {
                if (!_craftClickArmed || __result || __args == null ||
                    __args.Length == 0 ||
                    !CraftMagicItemsMundaneUiEventPolicy.Is(
                        CurrentEventType(), "Repaint")) return;
                string label = __args[0] as string;
                if (string.IsNullOrWhiteSpace(label) ||
                    string.IsNullOrWhiteSpace(_expectedCraftItemName) ||
                    label.IndexOf(_expectedCraftItemName,
                        StringComparison.Ordinal) < 0) return;
                __result = true;
                _craftClickTriggered = true;
                _craftClickArmed = false;
            }
        }

        private static void PlayerInCapitalPostfix(ref bool __result)
        { lock (Gate) if (_forcePlayerInCapital) __result = true; }

        internal static string CurrentEventType()
        {
            object current = _eventCurrentProperty == null ? null :
                _eventCurrentProperty.GetValue(null, null);
            object type = current == null || _eventTypeProperty == null ? null :
                _eventTypeProperty.GetValue(current, null);
            return type == null ? string.Empty : type.ToString();
        }

        private object[] NewCreationProjects()
        {
            return _creationProjects.Cast<object>().Where(value =>
                !_creationProjectsSnapshot.Any(original => ReferenceEquals(
                    original, value))).ToArray();
        }

        private static FieldInfo RequireField(Type type, string name)
        {
            FieldInfo result = type == null ? null : type.GetField(name,
                BindingFlags.Static | BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic);
            if (result == null) throw new MissingFieldException(type == null ?
                "<null>" : type.FullName, name);
            return result;
        }

        private static object ReadField(object target, string name)
        { return RequireField(target == null ? null : target.GetType(), name)
            .GetValue(target); }

        private static MethodInfo RequireMethod(Type type, string name,
            int parameterCount, Type returnType)
        {
            MethodInfo[] matches = type == null ? new MethodInfo[0] :
                type.GetMethods(BindingFlags.Static | BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic).Where(value =>
                    value.Name == name && value.GetParameters().Length ==
                        parameterCount && (returnType == null ||
                        value.ReturnType == returnType)).ToArray();
            if (matches.Length != 1) throw new MissingMethodException(
                type == null ? "<null>" : type.FullName, name);
            return matches[0];
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

        private static string ReadString(object target, string name)
        {
            FieldInfo field = target == null ? null : target.GetType().GetField(
                name, BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);
            if (field == null || field.FieldType != typeof(string))
                throw new MissingFieldException(target == null ? "<null>" :
                    target.GetType().FullName, name);
            return field.GetValue(target) as string;
        }
    }
}
