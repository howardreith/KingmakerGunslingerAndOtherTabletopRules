using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using KingmakerGunslinger.CraftMagicItemsCompatibility;

#pragma warning disable 0169, 0649 // Reflection-only external contract fixture.

namespace KingmakerGunslinger.DomainTests
{
    internal static class CraftMagicItemsCompatibilityTests
    {
        internal static void AbsentDependencyIsInert()
        {
            CraftMagicItemsContractResolution absent =
                CraftMagicItemsContractProbe.Probe(null, true);
            Assertions.False(absent.IsCompatible,
                "A missing CMI assembly must remain unavailable.");
            Assertions.Equal("assembly-null", absent.FailedCheck,
                "The absent-contract diagnostic changed.");
            var status = new CraftMagicItemsCompatibilityStatus(
                CraftMagicItemsCompatibilityAvailability.NotInstalled,
                "absent", 0, 0, 0);
            Assertions.Equal("not installed", status.Display,
                "The read-only UMM status must distinguish absence.");
            Assertions.Equal("installed but disabled",
                new CraftMagicItemsCompatibilityStatus(
                    CraftMagicItemsCompatibilityAvailability
                        .InstalledDisabled, "disabled", 0, 0, 0).Display,
                "The read-only UMM status must distinguish a disabled CMI entry.");
            Assertions.Equal("incompatible, see log",
                new CraftMagicItemsCompatibilityStatus(
                    CraftMagicItemsCompatibilityAvailability.Incompatible,
                    "broken", 0, 0, 0).Display,
                "The read-only UMM status must distinguish an incompatible contract.");
            AssertNoStaticDependency();
        }

        internal static void ContractProbeAcceptsExactShape()
        {
            CraftMagicItemsContractResolution result =
                CraftMagicItemsContractProbe.Probe(
                    Assembly.GetExecutingAssembly(), false);
            Assertions.True(result.IsCompatible,
                "The exact bounded CMI 2.1.0 fixture was rejected: " +
                result.FailedCheck);
            Assertions.Equal("CraftMagicItems.Main",
                result.Contract.MainType.FullName,
                "The probe resolved the wrong entry type.");
            Assertions.Equal("HarmonyLib.Harmony",
                result.Contract.HarmonyInstanceField.FieldType.FullName,
                "The exact external Harmony generation was not resolved.");
        }

        internal static void ContractProbeRejectsMissingMembers()
        {
            Assembly broken = BuildBrokenContractAssembly();
            CraftMagicItemsContractResolution result =
                CraftMagicItemsContractProbe.Probe(broken, false);
            Assertions.False(result.IsCompatible,
                "A CMI-shaped assembly missing required fields was accepted.");
            Assertions.Equal("main-static-fields", result.FailedCheck,
                "The contract failure was not one bounded capability check.");
            string coordinator = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsOptionalExtensionCoordinator.cs");
            Assertions.True(coordinator.Contains("_incompatibleLogged") &&
                coordinator.Contains("if (!log || context == null) return") &&
                coordinator.Contains("ExceptionSummary(exception)") &&
                coordinator.Contains("depth < 5"),
                "Incompatible-contract logging is not bounded to one diagnostic.");
        }

        internal static void CatalogConstructionIsExact()
        {
            CraftMagicItemsCatalogEntry[] source = CatalogFixture();
            CraftMagicItemsCatalogDecision decision =
                CraftMagicItemsCompatibilityPolicy.BuildCatalog(source,
                    new CraftMagicItemsModuleState(true, true, true));
            Assertions.Equal(5, decision.FirearmBases.Length,
                "Every authorized production firearm must be a base.");
            Assertions.Equal("nodachi", decision.MartialBases.Single().Identity,
                "Nodachi must use CMI Martial Weapons.");
            Assertions.True(decision.ExoticBases.Select(value =>
                    value.Identity).SequenceEqual(new[] { "wakizashi",
                    "katana", "spear" }),
                "Wakizashi, Katana, and Elven Branched Spear must be Exotic bases.");
            Assertions.Equal(1, decision.AuthoredTargets.Length,
                "Authored generic variants must remain target-only.");
            Assertions.Equal(1, decision.NamedUpgradeOnly.Length,
                "Named campaign items must remain upgrade-only.");
            Assertions.False(decision.AllCreationBases.Any(value =>
                    value.Role != CraftMagicItemsCatalogRole
                        .CanonicalCreationBase || value.Unavailable ||
                    !value.PlayerAuthorized || value.Family ==
                        CraftMagicItemsCatalogFamily.Diagnostic),
                "A diagnostic, unavailable, or noncanonical item entered creation bases.");

            string runtime = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsRegistrationCatalog.cs");
            foreach (string token in new[] {
                "BlueprintBootstrap.ProductionFirearms",
                "value.Spec.IsPlayerFireable", "GenericEntries",
                "NamedEntries", "BlueprintBootstrap.EasternWeapons",
                "BlueprintBootstrap.ElvenBranchedSpears",
                "BlueprintBootstrap.BasicAmmunition",
                "magic.Reliable", "UnavailableProductionFirearmRestriction" })
                Assertions.True(runtime.Contains(token),
                    "The runtime catalog does not derive from authority: " + token);
            Assertions.False(Regex.IsMatch(runtime,
                    "\\\"[0-9a-fA-F]{32}\\\""),
                "The compatibility catalog introduced a loose GUID list.");
            Assertions.True(runtime.Contains("value.Identity +") &&
                runtime.Contains("#CraftMagicItems"),
                "CMI clones of named campaign items are not kept upgrade-only.");
        }

        internal static void RegistrationPolicyIsIdempotent()
        {
            Identity first = new Identity("first");
            Identity second = new Identity("second");
            Identity[] once = CraftMagicItemsCompatibilityPolicy
                .MergeExactlyOnce(new[] { first }, new[] { first, second },
                    value => value.Id);
            Identity[] twice = CraftMagicItemsCompatibilityPolicy
                .MergeExactlyOnce(once, new[] { first, second },
                    value => value.Id);
            Assertions.True(once.SequenceEqual(twice) && twice.Length == 2 &&
                ReferenceEquals(twice[0], first) &&
                ReferenceEquals(twice[1], second),
                "Repeated registration changed identity, order, or count.");
            string bridge = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsReflectionBridge.cs");
            Assertions.True(bridge.Contains("ReferenceEquals(raw, _currentGraph)") &&
                bridge.Contains("AddRecipeForEnchantment") &&
                bridge.Contains("CountAddedItemTypes") &&
                bridge.Contains("CaptureNewItemBaseState") &&
                bridge.Contains("TryRestoreNewItemBaseState"),
                "The runtime graph lacks exact repeated-boundary guards.");
        }

        internal static void FeatureModuleMatrixIsExact()
        {
            CraftMagicItemsCatalogEntry[] source = CatalogFixture();
            AssertCreationCounts(source, new CraftMagicItemsModuleState(
                false, true, true), 0, 1, 3);
            AssertCreationCounts(source, new CraftMagicItemsModuleState(
                true, false, true), 5, 0, 1);
            AssertCreationCounts(source, new CraftMagicItemsModuleState(
                true, true, false), 5, 1, 2);
            AssertCreationCounts(source, new CraftMagicItemsModuleState(
                false, false, false), 0, 0, 0);
            CraftMagicItemsCatalogDecision disabled =
                CraftMagicItemsCompatibilityPolicy.BuildCatalog(source,
                    new CraftMagicItemsModuleState(false, false, false));
            Assertions.Equal(1, disabled.NamedUpgradeOnly.Length,
                "Disabled modules must preserve owned stable upgrade identity.");
            Assertions.Equal(1, disabled.AuthoredTargets.Length,
                "Disabled modules must preserve recognition of authored targets.");
        }

        internal static void ReliableApplicabilityIsMarkerExact()
        {
            Assertions.False(CraftMagicItemsCompatibilityPolicy
                .ReliableApplies(0),
                "Reliable applied without a firearm marker.");
            Assertions.True(CraftMagicItemsCompatibilityPolicy
                .ReliableApplies(1),
                "Reliable rejected an exact firearm marker, including a CMI clone.");
            Assertions.False(CraftMagicItemsCompatibilityPolicy
                .ReliableApplies(2),
                "Reliable accepted an ambiguous duplicated marker.");
            Assertions.Equal(1, CraftMagicItemsCompatibilityPolicy
                .ReliableEquivalentBonus,
                "Reliable's authorized equivalent bonus changed.");
            Assertions.Equal(8, CraftMagicItemsCompatibilityPolicy
                .ReliableCasterLevel,
                "Reliable's authorized caster level changed.");
            string bridge = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsReflectionBridge.cs");
            string coordinator = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsOptionalExtensionCoordinator.cs");
            Assertions.True(bridge.Contains("MarkerCount(weapon) == 1") ||
                Read("src", "KingmakerGunslinger",
                    "CraftMagicItemsCompatibility",
                    "CraftMagicItemsRegistrationCatalog.cs")
                    .Contains("MarkerCount(weapon) == 1"),
                "Reliable does not use the canonical firearm-definition marker.");
            foreach (string token in new[] { "RecipeAppliesPostfix",
                "BuildCustomRecipeGuidPrefix", "GuardCustomRecipeGuid",
                "__result = __result &&" })
                Assertions.True(coordinator.Contains(token) ||
                    bridge.Contains(token),
                    "Reliable lacks a final applicability boundary: " + token);
        }

        internal static void AmmunitionBatchEconomicsAreExact()
        {
            AssertAmmo("black-powder", "Black Powder Charge", 10, 200,
                50, 34);
            AssertAmmo("lead-ball", "Lead Ball", 1, 20, 5, 4);
            AssertAmmo("paper-cartridge", "Paper Cartridge", 12, 240,
                60, 40);
            string bridge = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsReflectionBridge.cs");
            Assertions.True(bridge.Contains("SetRecipeResult(recipe, item)") &&
                bridge.Contains("AmmunitionBatchCount") &&
                bridge.Contains("RenderCraftControl.Invoke"),
                "Ammunition does not use exact result items and CMI's mundane control.");
            Assertions.False(bridge.Contains("NewItemBaseIDs = ammunition"),
                "Plain ammunition was forced into CMI equipment base arrays.");
        }

        internal static void CustomBlueprintIntegrityBoundaryIsExact()
        {
            var firearm = Snapshot("pistol", "pistol-type", 1, 1,
                "pistol-presentation", "firearm", "reload", "capacity");
            var firearmClone = Snapshot("pistol#CraftMagicItems",
                "pistol-type", 1, 1, "pistol-presentation", "firearm",
                "reload", "capacity");
            CraftMagicItemsBlueprintIntegrityDecision firearmDecision =
                CraftMagicItemsCompatibilityPolicy.ValidateCustomClone(
                    firearm, firearm, firearmClone, true);
            Assertions.True(firearmDecision.Valid,
                "A faithful CMI firearm clone was rejected: " +
                firearmDecision.FailedCheck);
            var eastern = Snapshot("katana", "katana-type", 0, 1,
                "katana-presentation", "katana", "grip", "finesse");
            var easternClone = Snapshot("katana#CraftMagicItems",
                "katana-type", 0, 1, "katana-presentation", "katana",
                "grip", "finesse");
            Assertions.True(CraftMagicItemsCompatibilityPolicy
                .ValidateCustomClone(eastern, eastern, easternClone, false)
                .Valid, "A faithful Eastern weapon clone was rejected.");
            var spear = Snapshot("spear", "spear-type", 0, 1,
                "spear-presentation", "elven-branched-spear", "reach",
                "finesse", "zero-cost-policy");
            var spearClone = Snapshot("spear#CraftMagicItems", "spear-type",
                0, 1, "spear-presentation", "elven-branched-spear",
                "reach", "finesse", "zero-cost-policy");
            Assertions.True(CraftMagicItemsCompatibilityPolicy
                .ValidateCustomClone(spear, spear, spearClone, false).Valid,
                "A faithful Elven Branched Spear clone was rejected.");
            Assertions.Equal("base-mutated",
                CraftMagicItemsCompatibilityPolicy.ValidateCustomClone(
                    firearm, Snapshot("pistol", "changed", 1, 1,
                        "pistol-presentation", "firearm", "reload",
                        "capacity"), firearmClone, true).FailedCheck,
                "Mutation of the original base was not rejected.");
            Assertions.Equal("firearm-marker",
                CraftMagicItemsCompatibilityPolicy.ValidateCustomClone(
                    firearm, firearm, Snapshot("clone", "pistol-type", 0,
                        1, "pistol-presentation", "firearm", "reload",
                        "capacity"), true).FailedCheck,
                "A custom firearm clone without the exact marker was accepted.");
            string bridge = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsReflectionBridge.cs");
            foreach (string token in new[] { "BuildQualificationClone",
                "FirearmRuntimeState.ReadStateTokenIds",
                "RestoreMissingStateToken", "BatteredFirearmOriginRuntime",
                "value.Item.Type", "weapon.Type" })
                Assertions.True(bridge.Contains(token),
                    "Custom blueprint integrity contract lacks: " + token);
            Assertions.True(bridge.Contains("BuildCustomRecipeGuid") &&
                !bridge.Contains("ScriptableObject.Instantiate"),
                "KMG must rely on CMI's custom blueprint persistence system.");
        }

        internal static void LifecycleAndPackagingRemainOptional()
        {
            string coordinator = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsOptionalExtensionCoordinator.cs");
            string bridge = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsReflectionBridge.cs");
            foreach (string token in new[] { "AfterDataRead",
                "AugmentDataReadResult", "AddItemIdForEnchantment",
                "AddAllCraftingFeats", "ActivateMagicFeatCategories",
                "BeforeEquipmentIndexes", "RebuildCompleteGraph",
                "ExternalDisabled", "HarmonyLib.Harmony",
                "first-update-after-umm-load", "late-attachment",
                "patches=11", "RollbackCompatibilityGraph",
                "TryRestoreNewItemBaseState", "object[] __args",
                "BlueprintBootstrap.IsInitialized", "blueprints.pending",
                "SynchronizeMundaneIndexes", "UnpatchAll",
                "harmony.patch-install-rollback", "AggregateException" })
                Assertions.True(coordinator.Contains(token) ||
                    bridge.Contains(token),
                    "The CMI lifecycle contract lacks: " + token);
            Assertions.True(bridge.Contains("MagicFirearmsIdentity") &&
                bridge.Contains("MundaneFirearmsIdentity") &&
                bridge.Contains("AmmunitionIdentity") &&
                bridge.Contains("ReliableRecipeIdentity"),
                "Dedicated stable registration identities are incomplete.");
            string scenarioCatalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string observer = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "CraftMagicItemsCompatibilityObserver.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            Assertions.True(scenarioCatalog.Contains(
                    "ObserveCraftMagicItemsCompatibility") &&
                scenarioCatalog.Contains(
                    "observe-craft-magic-items-compatibility") &&
                runner.Contains(
                    "CraftMagicItemsCompatibilityObserver.Run") &&
                automation.Contains(
                    "'observe-craft-magic-items-compatibility'") &&
                observer.Contains("RunGuardedQualification") &&
                observer.Contains("exact-live-cmi-entry") &&
                observer.Contains("save-free-disposable-boundary"),
                "The guarded real-CMI qualification scenario is incomplete.");
            AssertNoStaticDependency();
        }

        private static void AssertAmmo(string identity, string name,
            int unitCost, int value, int progress, int gold)
        {
            var plan = new CraftMagicItemsAmmunitionRecipePlan(identity,
                name, unitCost,
                CraftMagicItemsCompatibilityPolicy.AmmunitionBatchCount);
            Assertions.Equal(20, plan.Count,
                name + " batch count changed.");
            Assertions.Equal(value, plan.BatchValue,
                name + " batch value changed.");
            Assertions.Equal(progress, plan.RequiredProgress,
                name + " required progress changed.");
            Assertions.Equal(gold, plan.GoldCost(1f),
                name + " ordinary CMI gold cost changed.");
        }

        private static CraftMagicItemsBlueprintIntegritySnapshot Snapshot(
            string identity, string type, int markers, int proficiency,
            string presentation, string category, params string[] mechanics)
        {
            return new CraftMagicItemsBlueprintIntegritySnapshot(identity,
                type, markers, proficiency, presentation, category,
                mechanics);
        }

        private static void AssertCreationCounts(
            CraftMagicItemsCatalogEntry[] source,
            CraftMagicItemsModuleState modules, int firearms, int martial,
            int exotic)
        {
            CraftMagicItemsCatalogDecision decision =
                CraftMagicItemsCompatibilityPolicy.BuildCatalog(source,
                    modules);
            Assertions.Equal(firearms, decision.FirearmBases.Length,
                "Firearm module gate changed.");
            Assertions.Equal(martial, decision.MartialBases.Length,
                "Eastern Martial module gate changed.");
            Assertions.Equal(exotic, decision.ExoticBases.Length,
                "Eastern/Elven Exotic module gates changed.");
        }

        private static CraftMagicItemsCatalogEntry[] CatalogFixture()
        {
            var result = new List<CraftMagicItemsCatalogEntry>();
            foreach (string firearm in new[] { "pistol", "musket",
                "blunderbuss", "advanced-rifle", "advanced-revolver" })
                result.Add(Entry(firearm,
                    CraftMagicItemsCatalogFamily.Firearm,
                    CraftMagicItemsCatalogRole.CanonicalCreationBase,
                    CraftMagicItemsOwningModule.Gunslinger, true, false));
            result.Add(Entry("wakizashi",
                CraftMagicItemsCatalogFamily.Wakizashi,
                CraftMagicItemsCatalogRole.CanonicalCreationBase,
                CraftMagicItemsOwningModule.EasternWeapons, true, false));
            result.Add(Entry("katana", CraftMagicItemsCatalogFamily.Katana,
                CraftMagicItemsCatalogRole.CanonicalCreationBase,
                CraftMagicItemsOwningModule.EasternWeapons, true, false));
            result.Add(Entry("nodachi", CraftMagicItemsCatalogFamily.Nodachi,
                CraftMagicItemsCatalogRole.CanonicalCreationBase,
                CraftMagicItemsOwningModule.EasternWeapons, true, false));
            result.Add(Entry("spear",
                CraftMagicItemsCatalogFamily.ElvenBranchedSpear,
                CraftMagicItemsCatalogRole.CanonicalCreationBase,
                CraftMagicItemsOwningModule.ElvenBranchedSpears, true, false));
            result.Add(Entry("pistol-plus-one",
                CraftMagicItemsCatalogFamily.Firearm,
                CraftMagicItemsCatalogRole.AuthoredGenericTarget,
                CraftMagicItemsOwningModule.Gunslinger, true, false));
            result.Add(Entry("named-katana",
                CraftMagicItemsCatalogFamily.Katana,
                CraftMagicItemsCatalogRole.NamedUpgradeOnly,
                CraftMagicItemsOwningModule.EasternWeapons, true, false));
            result.Add(Entry("test-musket",
                CraftMagicItemsCatalogFamily.Diagnostic,
                CraftMagicItemsCatalogRole.CanonicalCreationBase,
                CraftMagicItemsOwningModule.Gunslinger, true, false));
            result.Add(Entry("unavailable-firearm",
                CraftMagicItemsCatalogFamily.Firearm,
                CraftMagicItemsCatalogRole.CanonicalCreationBase,
                CraftMagicItemsOwningModule.Gunslinger, true, true));
            result.Add(Entry("unauthorized-firearm",
                CraftMagicItemsCatalogFamily.Firearm,
                CraftMagicItemsCatalogRole.CanonicalCreationBase,
                CraftMagicItemsOwningModule.Gunslinger, false, false));
            return result.ToArray();
        }

        private static CraftMagicItemsCatalogEntry Entry(string identity,
            CraftMagicItemsCatalogFamily family,
            CraftMagicItemsCatalogRole role,
            CraftMagicItemsOwningModule module, bool authorized,
            bool unavailable)
        {
            return new CraftMagicItemsCatalogEntry(identity, identity,
                family, role, module, authorized, unavailable);
        }

        private static void AssertNoStaticDependency()
        {
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            Assertions.False(Regex.IsMatch(project,
                    "<Reference\\s+Include=\\\"CraftMagicItems",
                    RegexOptions.IgnoreCase),
                "Production gained a required CraftMagicItems reference.");
            Assertions.False(project.Contains("CraftMagicItems.dll"),
                "Production or package metadata names the external DLL.");
            string[] production = Directory.GetFiles(Path.Combine(Root(),
                "src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility"), "*.cs");
            Assertions.False(production.Any(path => File.ReadAllText(path)
                    .Contains("using CraftMagicItems")),
                "Production has a static CMI namespace reference.");
        }

        private static Assembly BuildBrokenContractAssembly()
        {
            AssemblyName name = new AssemblyName("BrokenCmiFixture");
            AssemblyBuilder assembly = AppDomain.CurrentDomain
                .DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            ModuleBuilder module = assembly.DefineDynamicModule(
                "BrokenCmiFixture.dll");
            Type item = module.DefineType("CraftMagicItems.ItemCraftingData",
                TypeAttributes.Public).CreateType();
            module.DefineType("CraftMagicItems.RecipeData",
                TypeAttributes.Public).CreateType();
            module.DefineType("CraftMagicItems.RecipeBasedItemCraftingData",
                TypeAttributes.Public, item).CreateType();
            module.DefineType(
                "CraftMagicItems.CraftMagicItemsBlueprintPatcher",
                TypeAttributes.Public).CreateType();
            module.DefineType("CraftMagicItems.Main",
                TypeAttributes.Public).CreateType();
            return assembly;
        }

        private static string Read(params string[] parts)
        { return File.ReadAllText(Path.Combine(new[] { Root() }.Concat(parts)
            .ToArray())); }

        private static string Root()
        {
            DirectoryInfo current = new DirectoryInfo(
                AppDomain.CurrentDomain.BaseDirectory);
            while (current != null && !File.Exists(Path.Combine(
                current.FullName, "KingmakerGunslinger.sln")))
                current = current.Parent;
            if (current == null) throw new DirectoryNotFoundException(
                "Repository root not found.");
            return current.FullName;
        }

        private sealed class Identity
        {
            internal Identity(string id) { Id = id; }
            internal string Id { get; private set; }
        }
    }
}

namespace HarmonyLib
{
    internal sealed class HarmonyMethod
    {
        public HarmonyMethod(MethodInfo method) { }
    }

    internal sealed class Harmony
    {
        public Harmony(string owner) { }
        public void Patch(MethodBase original, HarmonyMethod prefix,
            HarmonyMethod postfix, HarmonyMethod transpiler,
            HarmonyMethod finalizer) { }
        public void UnpatchAll(string owner) { }
    }
}

namespace CraftMagicItems
{
    internal enum DataTypeEnum { RecipeBased }
    internal enum Slot { Weapon, Usable }
    internal enum Restriction { Weapon }
    internal enum RecipeCostType { Flat, EnhancementLevelSquared }

    internal sealed class CraftingBlueprint<T>
    {
        internal CraftingBlueprint(T value) { Blueprint = value; }
        internal T Blueprint { get; private set; }
    }

    internal class ItemCraftingData
    {
        internal DataTypeEnum DataType { get; set; }
        internal string Name;
        internal string NameId;
        internal string ParentNameId;
        internal string FeatGuid;
        internal int MinimumCasterLevel;
        internal bool PrerequisitesMandatory;
        private CraftingBlueprint<object>[][] m_NewItemBaseIDs;
        private object[] m_CachedNewItemBaseIDs;
        internal int Count;
        internal object[] NewItemBaseIDs { get { return m_CachedNewItemBaseIDs; } }
    }

    internal sealed class RecipeBasedItemCraftingData : ItemCraftingData
    {
        internal string[] RecipeFileNames;
        internal Slot[] Slots;
        internal Slot[] SlotRestrictions;
        internal int MundaneBaseDC;
        internal bool MundaneEnhancementsStackable;
        internal RecipeData[] Recipes;
        internal Dictionary<string, List<RecipeData>> SubRecipes;
    }

    internal sealed class RecipeData
    {
        internal string Name;
        internal string NameId;
        internal string ParentNameId;
        private CraftingBlueprint<object>[] m_ResultItem;
        private CraftingBlueprint<object>[][] m_Enchantments;
        internal bool EnchantmentsCumulative;
        internal int CasterLevelStart;
        internal int CasterLevelMultiplier;
        internal object[] PrerequisiteSpells;
        internal RecipeCostType CostType;
        internal int CostFactor;
        internal int CostAdjustment;
        internal Slot[] OnlyForSlots;
        internal Restriction[] Restrictions;
        internal bool CanApplyToMundaneItem;
        internal object ResultItem { get { return null; } }
        internal object[] Enchantments { get { return new object[0]; } }
    }

    internal sealed class CraftMagicItemsBlueprintPatcher
    {
        internal string BuildCustomRecipeItemGuid(string originalGuid,
            IEnumerable<string> enchantments)
        { return originalGuid; }
    }

    internal static class Main
    {
        internal static ItemCraftingData[] ItemCraftingData;
        private static bool modEnabled;
        private static HarmonyLib.Harmony harmonyInstance;
        private static CraftMagicItemsBlueprintPatcher blueprintPatcher;
        private static readonly Dictionary<string, int> SelectedIndex =
            new Dictionary<string, int>();
        private static readonly Dictionary<string, List<ItemCraftingData>>
            SubCraftingData = new Dictionary<string, List<ItemCraftingData>>();
        private static readonly Dictionary<string, object> TypeToItem =
            new Dictionary<string, object>();
        private static readonly Dictionary<string, List<object>>
            EnchantmentIdToItem = new Dictionary<string, List<object>>();
        private static readonly Dictionary<string, List<RecipeData>>
            EnchantmentIdToRecipe =
                new Dictionary<string, List<RecipeData>>();
        private static readonly Dictionary<string, int> EnchantmentIdToCost =
            new Dictionary<string, int>();

        private static bool OnToggle(object entry, bool enabled) { return true; }
        private static bool CanEnchant(object item) { return false; }
        private static bool RecipeAppliesToBlueprint(object recipe,
            object blueprint, bool skipEnchant, bool skipMaterial)
        { return false; }
        private static bool DoesBlueprintMatchSlot(object blueprint,
            object slot) { return false; }
        private static bool DoesItemMatchAllEnchantments(object blueprint,
            string first, string second, object upgrade, bool checkPrice)
        { return false; }
        private static void RenderRecipeBasedCrafting(object unit,
            RecipeBasedItemCraftingData data, object upgrade) { }
        private static void RenderCraftMundaneItemsSection() { }
        private static void CraftItem(object result, object upgrade) { }
        private static void AddRecipeForEnchantment(string id,
            RecipeData recipe) { }
        private static object GetSelectedCrafter(bool render) { return null; }
        public static int DrawSelectionUserInterfaceElements(string label,
            string[] values, int columns) { return 0; }
        private static int RenderCraftingSkillInformation(object crafter,
            object skill, int dc, int level, object spells, object feats,
            bool any, object prerequisites, bool render) { return 0; }
        private static void RenderRecipeBasedCraftItemControl(object crafter,
            object data, object recipe, int level, object item,
            object upgrade) { }
        public static T ReadJsonFile<T>(string path, params object[] converters)
        { return default(T); }
        private static void AddItemIdForEnchantment(object item) { }
        public static int ItemPlusEquivalent(object blueprint) { return 0; }
        public static int RulesRecipeItemCost(object blueprint, int baseCost,
            float weight) { return 0; }

        internal static class MainMenuStartPatch
        {
            private static void InitialiseCraftingData() { }
            private static void AddAllCraftingFeats() { }
        }
    }
}
#pragma warning restore 0169, 0649
