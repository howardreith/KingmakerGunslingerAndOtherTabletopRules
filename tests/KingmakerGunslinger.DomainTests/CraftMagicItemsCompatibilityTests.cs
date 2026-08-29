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
            Assertions.Equal("KMG compatibility UI fault, see log",
                new CraftMagicItemsCompatibilityStatus(
                    CraftMagicItemsCompatibilityAvailability.BridgeFaulted,
                    "render-fault", 0, 0, 0).Display,
                "A KMG UI fault must not be mislabeled as an incompatible CMI contract.");
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
            Assertions.Equal(13, result.Contract.CraftingProjectConstructor
                    .GetParameters().Length,
                "The supported project constructor shape changed.");
            Assertions.True(result.Contract.TimerProjectsField.FieldType
                    .GetGenericArguments()[0] ==
                        result.Contract.CraftingProjectDataType &&
                result.Contract.GetCraftingTimer.ReturnType ==
                    result.Contract.CraftingTimerComponentType,
                "The exact project migration lifecycle seam changed.");
        }

        internal static void ContractProbeRejectsMissingMembers()
        {
            Assembly broken = BuildBrokenContractAssembly();
            CraftMagicItemsContractResolution result =
                CraftMagicItemsContractProbe.Probe(broken, false);
            Assertions.False(result.IsCompatible,
                "A CMI-shaped assembly missing required fields was accepted.");
            Assertions.Equal("required-types", result.FailedCheck,
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

        internal static void MundaneUiPatchShapeIsExact()
        {
            CraftMagicItemsContractResolution supported =
                CraftMagicItemsContractProbe.Probe(
                    Assembly.GetExecutingAssembly(), false);
            Assertions.True(supported.IsCompatible,
                "The supported inner mundane seam was not accepted: " +
                supported.FailedCheck);
            Assertions.True(supported.Contract.MundaneUiAnchor.Identity
                    .StartsWith("post-selected-crafting-data:",
                        StringComparison.Ordinal) &&
                supported.Contract.MundaneUiAnchor.OrdinaryBodyOffset <
                    supported.Contract.MundaneUiAnchor.NewItemBaseOffset &&
                supported.Contract.MundaneUiAnchor.NewItemBaseOffset <
                    supported.Contract.MundaneUiAnchor.FooterOffset &&
                supported.Contract.MundaneUiAnchor.LabelRenderer != null &&
                supported.Contract.MundaneUiAnchor.LabelRenderer.Name ==
                    "RenderLabelRow",
                "The resolved inner seam does not precede the ordinary base access and common footer.");

            Type main = typeof(CraftMagicItems.Main);
            MethodInfo getCrafter = main.GetMethod("GetSelectedCrafter",
                BindingFlags.Static | BindingFlags.NonPublic);
            CraftMagicItemsMundaneUiResolution missing =
                CraftMagicItemsMundaneUiContract.Probe(typeof(
                    CraftMagicItemsCompatibilityTests).GetMethod(
                        "MissingMundaneRenderer", BindingFlags.Static |
                        BindingFlags.NonPublic),
                    typeof(CraftMagicItems.ItemCraftingData),
                    typeof(CraftMagicItems.RecipeBasedItemCraftingData),
                    getCrafter);
            Assertions.False(missing.IsCompatible,
                "A renderer missing the inner seam was accepted.");
            Assertions.Equal("mundane-ui-outer-selector-label",
                missing.FailedCheck,
                "A missing IL anchor did not fail at one bounded check.");

            CraftMagicItemsMundaneUiResolution ambiguous =
                CraftMagicItemsMundaneUiContract.Probe(typeof(
                    CraftMagicItemsCompatibilityTests).GetMethod(
                        "AmbiguousMundaneRenderer", BindingFlags.Static |
                        BindingFlags.NonPublic),
                    typeof(CraftMagicItems.ItemCraftingData),
                    typeof(CraftMagicItems.RecipeBasedItemCraftingData),
                    getCrafter);
            Assertions.False(ambiguous.IsCompatible,
                "An ambiguous selected-data cast was accepted.");
            Assertions.Equal("mundane-ui-selected-data-cast",
                ambiguous.FailedCheck,
                "An ambiguous IL anchor did not fail closed before UI use.");

            string coordinator = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsOptionalExtensionCoordinator.cs");
            string transpiler = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsMundaneUiTranspiler.cs");
            Assertions.False(coordinator.Contains("RenderMundanePrefix") ||
                coordinator.Contains("TryRenderAmmunition"),
                "The rejected conditional whole-method prefix remains.");
            foreach (string token in new[] { "RenderMundaneTranspiler",
                "ammunition-ui-inner-seam", "Brfalse, continueOrdinary",
                "Br, commonFooter", "ordinary.labels.Clear()",
                "get_NewItemBaseIDs", "FooterFormat" })
                Assertions.True(coordinator.Contains(token) ||
                    transpiler.Contains(token),
                    "The inner mundane interception lacks: " + token);
        }

        internal static void MundaneUiRouteIsStable()
        {
            Assertions.True(CraftMagicItemsMundaneUiEventPolicy.Is(
                    "Layout", "Layout") &&
                CraftMagicItemsMundaneUiEventPolicy.Is(
                    "repaint", "Repaint") &&
                !CraftMagicItemsMundaneUiEventPolicy.Is(
                    "MouseDown", "Repaint"),
                "The guarded route observer does not recognize the exact " +
                "lower-case Repaint name emitted by the supported Unity runtime.");
            Assertions.True(
                    CraftMagicItemsMundaneUiEventPolicy
                        .ShouldApplyPendingPhase(true, "Layout") &&
                !CraftMagicItemsMundaneUiEventPolicy
                    .ShouldApplyPendingPhase(true, "repaint") &&
                !CraftMagicItemsMundaneUiEventPolicy
                    .ShouldApplyPendingPhase(false, "Layout"),
                "The guarded observer can change a route between Layout and repaint.");
            var ordinary = new object();
            var ammunition = new object();
            var sameDisplayButDifferentObject = new object();
            object upgradingBlueprint = new object();
            object[] transitions = { ordinary, ammunition, ammunition,
                null, ammunition, null, ammunition,
                sameDisplayButDifferentObject, ordinary };
            string[] events = { "Layout", "MouseDown", "Repaint" };
            int ammunitionPasses = 0;
            int ordinaryPasses = 0;
            foreach (object selected in transitions)
                foreach (string eventType in events)
                {
                    CraftMagicItemsMundaneUiRoute route =
                        CraftMagicItemsMundaneUiRoutePolicy.Resolve(selected,
                            ammunition);
                    Assertions.Equal("CraftMagicItems",
                        "CraftMagicItems",
                        "The outer selector owner changed on " + eventType +
                        ".");
                    if (ReferenceEquals(selected, ammunition))
                    {
                        Assertions.Equal(
                            CraftMagicItemsMundaneUiRoute
                                .AmmunitionLowerPanel, route,
                            "The exact ammunition route changed between IMGUI event passes.");
                        ammunitionPasses++;
                    }
                    else
                    {
                        Assertions.Equal(
                            CraftMagicItemsMundaneUiRoute.OrdinaryCmi,
                            route,
                            "KMG took ownership of a native, closed-tab, or lookalike route.");
                        ordinaryPasses++;
                    }
                }
            Assertions.Equal(12, ammunitionPasses,
                "The modeled ammunition transitions changed.");
            Assertions.Equal(15, ordinaryPasses,
                "The modeled native/tab transitions changed.");
            Assertions.True(ReferenceEquals(upgradingBlueprint,
                    upgradingBlueprint),
                "The route model disturbed unrelated upgrade state.");

            string bridge = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsReflectionBridge.cs");
            int start = bridge.IndexOf(
                "TryRenderSelectedAmmunition", StringComparison.Ordinal);
            int end = bridge.IndexOf("BuildQualificationClone",
                start, StringComparison.Ordinal);
            string routeBody = bridge.Substring(start, end - start);
            Assertions.False(routeBody.Contains("SelectedIndexField") ||
                routeBody.Contains("Mundane Crafting: ") ||
                routeBody.Contains("GetSelectedCrafter.Invoke"),
                "The lower-panel route still redraws or owns CMI's outer selector.");
            Assertions.True(routeBody.Contains(
                    "CraftMagicItemsMundaneUiRoutePolicy.Resolve") &&
                routeBody.Contains("RenderCraftControl.Invoke") &&
                routeBody.Contains("RenderLabel.Invoke") &&
                !routeBody.Contains("ImmediateModeGui.Label"),
                "The exact-reference lower panel does not reach CMI's normal crafting control.");
        }

        internal static void MundaneUiFailureIsDeferred()
        {
            Exception root;
            try
            {
                ThrowUiFixture();
                throw new InvalidOperationException("unreachable");
            }
            catch (Exception exception)
            {
                root = exception;
            }
            var wrapped = new TargetInvocationException(
                new TargetInvocationException(root));
            CraftMagicItemsUiFailureCapture capture =
                CraftMagicItemsUiFailurePolicy.Capture(wrapped);
            Assertions.True(ReferenceEquals(root, capture.Root),
                "TargetInvocationException was not recursively unwrapped.");
            Assertions.False(capture.RunOriginalRenderer,
                "A partial KMG render would fall through to CMI's original body.");
            Assertions.False(capture.RollbackSynchronously,
                "The compatibility graph would roll back inside OnGUI.");
            Assertions.True(capture.DeferDisableToSafeUpdate &&
                capture.Rethrow && !capture.ExternalContractIncompatible,
                "The UI failure boundary was mislabeled or swallowed.");
            foreach (string token in new[] {
                "System.Reflection.TargetInvocationException",
                "System.InvalidOperationException", "bounded-ui-fixture",
                "ThrowUiFixture" })
                Assertions.True(capture.ExceptionChain.Contains(token),
                    "The complete UI exception chain lacks: " + token);

            string bridge = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsReflectionBridge.cs");
            string coordinator = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsOptionalExtensionCoordinator.cs");
            foreach (string token in new[] { "bridge.ui-failure.queued",
                "graphMutation=false", "rollbackLifecycle=OnUpdate",
                "ProcessDeferredUiFailure", "bridge.ui-disabled",
                "BridgeFaulted" })
                Assertions.True(bridge.Contains(token) ||
                    coordinator.Contains(token),
                    "The deferred UI failure path lacks: " + token);
        }

        private static void MissingMundaneRenderer() { }

        private static void AmbiguousMundaneRenderer()
        {
            string selectedCustomName = null;
            CraftMagicItems.ItemCraftingData selected = null;
            CraftMagicItems.Main.DrawSelectionUserInterfaceElements(
                "Mundane Crafting: ", new string[0], 6);
            CraftMagicItems.Main.DrawSelectionUserInterfaceElements<string>(
                "Mundane Crafting: ", new string[0], 6,
                ref selectedCustomName, true);
            var first = selected as
                CraftMagicItems.RecipeBasedItemCraftingData;
            var second = selected as
                CraftMagicItems.RecipeBasedItemCraftingData;
            if (first != null && second != null) GC.KeepAlive(first);
        }

        private static void ThrowUiFixture()
        { throw new InvalidOperationException("bounded-ui-fixture"); }

        internal static void CatalogConstructionIsExact()
        {
            CraftMagicItemsCatalogEntry[] source = CatalogFixture();
            CraftMagicItemsCatalogDecision decision =
                CraftMagicItemsCompatibilityPolicy.BuildCatalog(source,
                    new CraftMagicItemsModuleState(true, true, true));
            Assertions.True(decision.FirearmCreationBases.Select(value =>
                    value.Identity).SequenceEqual(new[] { "pistol", "musket",
                    "blunderbuss" }),
                "Only the three ordinary-campaign firearms may be creation bases.");
            Assertions.True(decision.FirearmRecognitionBases.Select(value =>
                    value.Identity).SequenceEqual(new[] { "pistol", "musket",
                    "blunderbuss", "advanced-rifle",
                    "advanced-revolver" }),
                "All five mechanically supported firearms must remain recognized.");
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
            Assertions.Equal(4,
                decision.CustomFamilyRecognitionBases.Length,
                "All four mundane custom-family identities must remain recognized for owned upgrades.");
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
                "value.Spec.IsPlayerFireable", "value.Spec.AcquisitionRole",
                "LegacyRecognitionOnly", "GenericEntries",
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
                true, false, true), 3, 0, 1);
            AssertCreationCounts(source, new CraftMagicItemsModuleState(
                true, true, false), 3, 1, 2);
            AssertCreationCounts(source, new CraftMagicItemsModuleState(
                false, false, false), 0, 0, 0);
            CraftMagicItemsCatalogDecision disabled =
                CraftMagicItemsCompatibilityPolicy.BuildCatalog(source,
                    new CraftMagicItemsModuleState(false, false, false));
            Assertions.Equal(1, disabled.NamedUpgradeOnly.Length,
                "Disabled modules must preserve owned stable upgrade identity.");
            Assertions.Equal(1, disabled.AuthoredTargets.Length,
                "Disabled modules must preserve recognition of authored targets.");
            Assertions.Equal(5, disabled.FirearmRecognitionBases.Length,
                "A disabled Gunslinger module must preserve owned firearm recognition.");
            Assertions.Equal(4,
                disabled.CustomFamilyRecognitionBases.Length,
                "Disabled custom-family modules must preserve owned-item recognition.");
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
                50, 5, 34);
            AssertAmmo("lead-ball", "Lead Ball", 1, 20, 5, 5, 4);
            AssertAmmo("paper-cartridge", "Paper Cartridge", 12, 240,
                60, 5, 40);
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

        internal static void AmmunitionProjectMigrationIsExact()
        {
            Assertions.Equal(5, CraftMagicItemsCompatibilityPolicy
                .NormalizeAmmunitionProjectTarget(50, 50, true),
                "A legacy Black Powder target was not normalized.");
            Assertions.Equal(5, CraftMagicItemsCompatibilityPolicy
                .NormalizeAmmunitionProjectTarget(60, 60, true),
                "A legacy Paper Cartridge target was not normalized.");
            Assertions.Equal(5, CraftMagicItemsCompatibilityPolicy
                .NormalizeAmmunitionProjectTarget(5, 5, true),
                "An already-normalized Lead Ball target changed.");
            Assertions.Equal(50, CraftMagicItemsCompatibilityPolicy
                .NormalizeAmmunitionProjectTarget(50, 50, false),
                "An unrelated CMI project was modified.");
            Assertions.Equal(37, CraftMagicItemsCompatibilityPolicy
                .NormalizeAmmunitionProjectTarget(37, 50, true),
                "A non-authoritative project target was guessed at.");
            string bridge = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsReflectionBridge.cs");
            string contract = Read("src", "KingmakerGunslinger",
                "CraftMagicItemsCompatibility",
                "CraftMagicItemsContractProbe.cs");
            foreach (string token in new[] { "ProjectGoldSpentField",
                "ProjectResultItemField", "ProjectUpgradeItemField",
                "ProjectRecipeNameField", "result.Count !=",
                "progressPreserved=true", "goldSpentPreserved=true" })
                Assertions.True(bridge.Contains(token) ||
                    contract.Contains(token),
                    "The exact project migration boundary lacks: " + token);
        }

        internal static void InternalTooltipMarkersAreExact()
        {
            Assertions.True(CraftMagicItemsCompatibilityPolicy
                    .IsInternalEnchantmentPresentationMarker(true, false) &&
                CraftMagicItemsCompatibilityPolicy
                    .IsInternalEnchantmentPresentationMarker(false, true),
                "A required internal firearm marker was not hidden.");
            Assertions.False(CraftMagicItemsCompatibilityPolicy
                .IsInternalEnchantmentPresentationMarker(false, false),
                "A real weapon quality would be hidden.");
            string source = Read("src", "KingmakerGunslinger", "Firearms",
                "FirearmInternalEnchantmentPresentation.cs");
            foreach (string token in new[] { "FillWeaponQualities",
                "GetQualities", "FirearmStateTokenComponent",
                "BatteredFirearmOriginComponent", "ShouldRender",
                "Brfalse" })
                Assertions.True(source.Contains(token),
                    "The native tooltip marker filter lacks: " + token);
            Assertions.False(source.Contains("Replace(\"<null>\"") ||
                source.Contains("string.IsNullOrEmpty(enchantment"),
                "The tooltip repair suppresses text globally instead of exact markers.");
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
                "FirearmRecognitionBases",
                "CustomFamilyRecognitionBases", "weapon.Type" })
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
                "patches=13", "RollbackCompatibilityGraph",
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
            Assertions.False(bridge.Contains(
                    "KMGMagicEasternAndElvenWeapons") ||
                bridge.Contains("Eastern and Elven Weapons") ||
                bridge.Contains("MagicCustomWeaponsIdentity") ||
                bridge.Contains("_magicCustomWeapons") ||
                bridge.Contains("CategoryScope.CustomWeapons"),
                "The obsolete standalone Eastern/Elven magic category remains.");
            string scenarioCatalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string observer = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "CraftMagicItemsCompatibilityObserver.cs");
            string uiObserver = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "CraftMagicItemsAmmunitionUiObserver.cs");
            string tooltipObserver = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "CraftMagicItemsTooltipInspection.cs");
            string workingSave = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "WorkingSaveSmokeScenario.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string persistence = Read("scripts",
                "Invoke-CraftMagicItemsWorkingSavePersistence.ps1");
            Assertions.True(scenarioCatalog.Contains(
                    "ObserveCraftMagicItemsCompatibility") &&
                scenarioCatalog.Contains(
                    "observe-craft-magic-items-compatibility") &&
                scenarioCatalog.Contains(
                    "observe-craft-magic-items-ammunition-ui") &&
                scenarioCatalog.Contains(
                    "WorkingSaveCraftMagicItemsPrepare") &&
                scenarioCatalog.Contains(
                    "working-save-craft-magic-items-verify-cleanup") &&
                runner.Contains(
                    "CraftMagicItemsCompatibilityObserver.Run") &&
                runner.Contains(
                    "CraftMagicItemsAmmunitionUiObserver.Begin") &&
                runner.Contains("StartCraftMagicItemsPersistence") &&
                runner.Contains("ArmExactWorkingSaveWrite") &&
                automation.Contains(
                    "'observe-craft-magic-items-compatibility'") &&
                automation.Contains(
                    "'observe-craft-magic-items-ammunition-ui'") &&
                automation.Contains(
                    "'working-save-craft-magic-items-prepare'") &&
                automation.Contains("PermittedSaveName = " +
                    "'KMG_AUTOMATION_WORKING'") &&
                observer.Contains("RunGuardedQualification") &&
                observer.Contains("exact-live-cmi-entry") &&
                observer.Contains("save-free-disposable-boundary") &&
                uiObserver.Contains("ObserveCraft") &&
                uiObserver.Contains("CompleteTimedProject") &&
                uiObserver.Contains("FirearmReloadResult") &&
                Regex.Matches(uiObserver, Regex.Escape(
                    "Game.Instance.Player.Inventory.Remove(weapon)")).Count == 2 &&
                uiObserver.Contains("no GUI/TargetInvocation exception, deferred failure, or graph rollback"),
                "The guarded real-CMI qualification scenario is incomplete.");
            Assertions.True(tooltipObserver.Contains(
                    "BuildPersistentFixtureBlueprint") &&
                tooltipObserver.Contains("CapturePersistent") &&
                tooltipObserver.Contains("Anarchic/+5/Reliable") &&
                tooltipObserver.Contains("observation.NullCount == 0") &&
                persistence.Contains(
                    "working-save-craft-magic-items-prepare") &&
                persistence.Contains(
                     "working-save-craft-magic-items-verify-cleanup") &&
                persistence.Contains("ValidateSet('KMG_AUTOMATION_WORKING')") &&
                workingSave.Contains(
                    "_expectedWorkingSaveInProgress &&") &&
                workingSave.Contains(
                    "Read(value, \"Name\") == ExpectedName") &&
                workingSave.Contains(
                    "string suffix = \"_\" + ExpectedName + \".zks\";") &&
                workingSave.Contains("sequence.All(char.IsDigit)") &&
                workingSave.Contains(
                    "ReferenceEquals(descriptor, _workingDescriptor)"),
                "The guarded CMI save/reload/cleanup qualification is incomplete.");
            AssertNoStaticDependency();
        }

        private static void AssertAmmo(string identity, string name,
            int unitCost, int value, int valueTarget, int timedTarget,
            int gold)
        {
            var plan = new CraftMagicItemsAmmunitionRecipePlan(identity,
                name, unitCost,
                CraftMagicItemsCompatibilityPolicy.AmmunitionBatchCount);
            Assertions.Equal(20, plan.Count,
                name + " batch count changed.");
            Assertions.Equal(value, plan.BatchValue,
                name + " batch value changed.");
            Assertions.Equal(valueTarget, plan.ValueDerivedTarget,
                name + " value-derived target changed.");
            Assertions.Equal(timedTarget, plan.TimedProjectTarget,
                name + " timed project target changed.");
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
            Assertions.Equal(firearms,
                decision.FirearmCreationBases.Length,
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
                "blunderbuss" })
                result.Add(Entry(firearm,
                    CraftMagicItemsCatalogFamily.Firearm,
                    CraftMagicItemsCatalogRole.CanonicalCreationBase,
                    CraftMagicItemsOwningModule.Gunslinger, true, false));
            foreach (string firearm in new[] { "advanced-rifle",
                "advanced-revolver" })
                result.Add(Entry(firearm,
                    CraftMagicItemsCatalogFamily.Firearm,
                    CraftMagicItemsCatalogRole.LegacyRecognitionOnly,
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
    internal enum CrafterPrerequisiteType { Any }

    internal sealed class CraftingProjectData
    {
        internal object Crafter;
        internal int Progress;
        internal int TargetCost;
        internal int GoldSpent;
        internal int CasterLevel;
        internal object[] SpellPrerequisites;
        internal object[] FeatPrerequisites;
        internal bool PrerequisitesMandatory;
        internal CrafterPrerequisiteType[] CrafterPrerequisites;
        internal bool AnyPrerequisite;
        internal object ItemBlueprint;
        internal object ResultItem;
        internal string ItemType;
        internal string RecipeName;
        internal string LastMessage;
        internal object UpgradeItem;

        internal CraftingProjectData(object crafter, int targetCost,
            int goldSpent, int casterLevel, object resultItem,
            string itemType, string recipeName,
            object[] spellPrerequisites, object[] featPrerequisites,
            bool prerequisitesMandatory, bool anyPrerequisite,
            object upgradeItem,
            CrafterPrerequisiteType[] crafterPrerequisites)
        {
            Crafter = crafter;
            TargetCost = targetCost;
            GoldSpent = goldSpent;
            CasterLevel = casterLevel;
            ResultItem = resultItem;
            ItemType = itemType;
            RecipeName = recipeName;
            SpellPrerequisites = spellPrerequisites;
            FeatPrerequisites = featPrerequisites;
            PrerequisitesMandatory = prerequisitesMandatory;
            AnyPrerequisite = anyPrerequisite;
            UpgradeItem = upgradeItem;
            CrafterPrerequisites = crafterPrerequisites;
        }
    }

    internal sealed class CraftingTimerComponent
    {
        internal List<CraftingProjectData> CraftingProjects =
            new List<CraftingProjectData>();
    }

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
        private static string selectedCustomName;
        private static object upgradingBlueprint;
        private static void RenderCraftMundaneItemsSection()
        {
            object crafter = GetSelectedCrafter(false);
            int selectedItemTypeIndex = upgradingBlueprint == null ?
                DrawSelectionUserInterfaceElements<string>(
                    "Mundane Crafting: ", new string[0], 6,
                    ref selectedCustomName, true) :
                GetSelectionIndex("Mundane Crafting: ");
            ItemCraftingData selectedCraftingData =
                ItemCraftingData[selectedItemTypeIndex];
            if (selectedCraftingData.ParentNameId != null)
                selectedCraftingData = ItemCraftingData[
                    DrawSelectionUserInterfaceElements("Subtype: ",
                        new string[0], 6)];
            RecipeBasedItemCraftingData craftingData =
                selectedCraftingData as RecipeBasedItemCraftingData;
            if (craftingData == null)
            {
                RenderLabelRow("Unable to find mundane crafting recipe.");
                return;
            }
            object[] bases = craftingData.NewItemBaseIDs;
            if (crafter != null && bases != null && bases.Length < 0)
                RenderLabelRow("unreachable");
            RenderLabelRow("Current Money: {0}");
        }
        private static void CraftItem(object result, object upgrade) { }
        private static void AddRecipeForEnchantment(string id,
            RecipeData recipe) { }
        private static object GetSelectedCrafter(bool render) { return null; }
        private static CraftingTimerComponent
            GetCraftingTimerComponentForCaster(object caster,
                bool create)
        { return null; }
        private static int GetSelectionIndex(string label) { return 0; }
        public static int DrawSelectionUserInterfaceElements(string label,
            string[] values, int columns) { return 0; }
        public static int DrawSelectionUserInterfaceElements<T>(string label,
            string[] values, int columns, ref T emptyOnChange, bool addSpace)
        { return 0; }
        private static void RenderLabelRow(string value) { }
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
