using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace KingmakerGunslinger.DomainTests
{
    internal static class GunslingerOutfitRenderTests
    {
        private static readonly string[] ExactCandidateIds =
        {
            "94d11df1d859b6d4f90424213eec0392",
            "431d16d2153d1854280b97470223eea6",
            "e5ff950ef29119943bdcf3bfedd47887",
            "9aa7feeafa6f05f45a9fbae3b87bfc02",
            "49641981096de8b43b198e95c7193b65",
            "e9ce35008c62b334383e73e244becc36",
            "3709387ae978dae4d8ab60700a1e25e2",
            "db2f0f4384784974ba2428c96b21aa4e",
            "7667972f03e25494cb6b39ba7e82126f",
            "eb257cbf25c5363408073e2b11559a19",
            "2abb4698b7fcce24d9bdab0ffbd852f3",
            "6b8410318571dd949bd758e9f1275182",
            "6df8f61725a84294c8661bb9585eca97",
            "4c59d2b9740930145a27a4c693217d22",
            "beba0e0c7dcd5c64d97d767be3e72995",
            "a93ead19aae8afc4794c54f5bcf73168",
            "e249678d823d00f4cb30d4d5c8ca1219",
            "0809ab3735b54874b965a09311f0c898",
            "ca71ad9178ecf6a4d942ce55d0c7857b",
            "e09cf61a567f2a84ea9a3b505f390a32",
            "b6bca728c4ced324da7e8d0d01ad34bb",
            "bc6fb7e5c91de08418b81a397b20bb18",
            "b1c62eff2287d9a4fbbf76c345d58840",
            "d019e95d4a8a8474aa4e03489449d6ee",
            "345af8eabd450524ab364e7a7c6f1044",
            "c6757746d62b78f46a92020110dfe088",
            "096463cb26b8c3343874d2a2a1a752f6",
            "bf0f3ba364295e14eb5f2b285cea16b0",
            "9e98bd43dc04964409db62644ace4b15",
            "24230460eaff3fe49b0e186873c38218",
            "5eeabb19544a9ae41a8b26075933ef8d",
            "50b6ed92792f308479a07f8d9052c6d5"
        };

        internal static void GuardedWorkingSaveBoundaryIsExact()
        {
            string source = RenderSource();
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string request = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRequest.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string orchestrator = Read("scripts",
                "Invoke-KingmakerRuntimeTest.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            const string scenario = "gunslinger-outfit-candidate-render";
            Assertions.True(catalog.Contains(
                    "internal const string GunslingerOutfitCandidateRender") &&
                catalog.Contains(scenario) &&
                runner.Contains(
                    "GunslingerOutfitRenderScenario.Begin(") &&
                runner.Contains(
                    "_gunslingerOutfitCandidateRender.Poll()") &&
                WorkingSavePredicate(request).Contains(
                    "GunslingerOutfitCandidateRender") &&
                automation.Contains("'" + scenario +
                    "' = [pscustomobject]") &&
                preflight.Contains(
                    scenario + "-only-permits-working-save") &&
                project.Contains(
                    @"RuntimeTesting\GunslingerOutfitRenderScenario.cs") &&
                source.Contains(
                    "KMG_AUTOMATION_WORKING; no save API"),
                "Outfit renderer is not wired through every exact guarded working-save surface.");
            string metadata = automation.Substring(
                automation.IndexOf("'" + scenario +
                    "' = [pscustomobject]", StringComparison.Ordinal), 500);
            Assertions.True(metadata.Contains(
                    "RequiresSaveName = $true") &&
                metadata.Contains(
                    "PermittedSaveName = 'KMG_AUTOMATION_WORKING'") &&
                metadata.Contains(
                    "RequiresManualInteraction = $false") &&
                metadata.Contains(
                    "ReadinessBehavior = 'autonomous-working-save'"),
                "Outfit renderer metadata must fail closed to the disposable working save.");
            string collectorMarker = "elseif ($Scenario -eq '" +
                scenario + "')";
            int collectorStart = orchestrator.IndexOf(collectorMarker,
                StringComparison.Ordinal);
            Assertions.True(collectorStart >= 0,
                "Outfit renderer must own an explicit bounded result collector window.");
            string collector = orchestrator.Substring(collectorStart,
                Math.Min(500, orchestrator.Length - collectorStart));
            Assertions.True(collector.Contains(
                    "[Math]::Max($TimeoutSeconds, 600) + 15") &&
                collector.IndexOf("elseif ($Scenario",
                    collectorMarker.Length, StringComparison.Ordinal) > 0,
                "Outfit renderer collector must preserve the exact 600-second scenario-only ceiling.");
        }

        internal static void CandidateCatalogIsExactAndBounded()
        {
            string source = RenderSource();
            int start = source.IndexOf(
                "CandidateSpec[] Candidates", StringComparison.Ordinal);
            int end = source.IndexOf(
                "RenderCase[] Cases", start, StringComparison.Ordinal);
            Assertions.True(start >= 0 && end > start,
                "Outfit candidate catalog boundaries are absent.");
            string block = source.Substring(start, end - start);
            string[] ids = Regex.Matches(block, "[0-9a-f]{32}")
                .Cast<Match>().Select(value => value.Value).ToArray();
            Assertions.True(ExactCandidateIds.SequenceEqual(ids),
                "Outfit candidate IDs or native link order changed.");
            Assertions.Equal(6,
                Regex.Matches(block, "new CandidateSpec\\(").Count,
                "The first render batch must contain exactly six candidates.");
            foreach (string excluded in new[]
            {
                "d4aa53711899045459117dc7cf6f1246",
                "e65aa06e07fd13c4bb551b3371221bff",
                "16d5c17e1577f914084022f56fbdec75",
                "2624c609a899640409eeede202ec7f3d",
                "6233ee6ede86a7147ba705d98aab05e9",
                "9e61836c6078ba54e8fcc445b0b1e646",
                "fb0037ec1d96c8d418bc08d3e0bbf063",
                "52a0a0c7183957a4ea02301ce40b3e83",
                "bba6c03b44e5a1c4dbfacf7eec6123dd",
                "b7613075291c79947a0cde8c7aec5926"
            })
                Assertions.False(block.Contains(excluded),
                    "A structurally excluded cap or cape entered the serious candidate batch: " +
                    excluded);
        }

        internal static void RendererRestoresAndCapturesExactMatrix()
        {
            string source = RenderSource();
            foreach (string token in new[]
            {
                "ResourcesLibrary.TryGetResource<EquipmentEntity>",
                "GetEquipmentClass()", "FighterClassGuid",
                "gunslinger-outfit-render-fighter-donor-class",
                "exact-fighter-fallback", "classEntityPresentCount",
                "originalEntities", "donorClassEntities", "LoadClothes(",
                "RemoveEquipmentEntities(_classEntities, false)",
                "AddEquipmentEntities(_candidateEntities, false)",
                "SetRampIndices(entity, primary, secondary,",
                "RemoveAllEquipmentEntities(false)",
                "RebuildOutfit()", "AvatarMatchesSnapshot()",
                "SavedLinks(_avatar)", "male-human", "female-human",
                "native-default", "audit-alternate", "no-weapon",
                "pistol", "musket", "-preview.png", "-isometric.png",
                "CaptureContactSheet(", "CaptureIsometric(",
                "expectedRecords = 48", "expectedImages = 96",
                "expectedRestorations = 12",
                "productionBlueprintMutated", "saveApiCalled"
            })
                Assertions.True(source.Contains(token),
                    "Outfit renderer lacks exact evidence/restoration token: " +
                    token);
            foreach (string forbidden in new[]
            {
                "SaveGame", "QuickSave", "ScreenCapture",
                "Input.", "Mouse.", "PlayerPrefs",
                "Game.Instance.Player.Inventory"
            })
                Assertions.False(source.Contains(forbidden),
                    "Outfit renderer contains forbidden save/UI/global-inventory token: " +
                    forbidden);
            Assertions.False(source.Contains(
                    "has no native equipment class"),
                "An optional live EquipmentClass must not be required when the exact audited Fighter donor is available.");
            FinalistRaceMatrixIsExactAndReversible();
        }

        internal static void ProductionCompatibilityIsGuardedAndExact()
        {
            string source = Read("src", "KingmakerGunslinger",
                "RuntimeTesting",
                "GunslingerOutfitProductionCompatibilityScenario.cs");
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string request = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRequest.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string orchestrator = Read("scripts",
                "Invoke-KingmakerRuntimeTest.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            const string scenario =
                "gunslinger-outfit-production-compatibility";
            Assertions.True(catalog.Contains(
                    "GunslingerOutfitProductionCompatibility") &&
                catalog.Contains(scenario) &&
                runner.Contains(
                    "BeginProductionCompatibility(") &&
                runner.Contains(
                    "_gunslingerOutfitProductionCompatibility.Poll()") &&
                WorkingSavePredicate(request).Contains(
                    "GunslingerOutfitProductionCompatibility") &&
                automation.Contains("'" + scenario +
                    "' = [pscustomobject]") &&
                preflight.Contains(
                    scenario + "-only-permits-working-save") &&
                project.Contains(@"RuntimeTesting\GunslingerOutfitProductionCompatibilityScenario.cs"),
                "Production outfit compatibility is not wired through every guarded working-save surface.");
            string metadata = automation.Substring(
                automation.IndexOf("'" + scenario +
                    "' = [pscustomobject]", StringComparison.Ordinal), 500);
            Assertions.True(metadata.Contains(
                    "RequiresSaveName = $true") &&
                metadata.Contains(
                    "PermittedSaveName = 'KMG_AUTOMATION_WORKING'") &&
                metadata.Contains(
                    "RequiresManualInteraction = $false"),
                "Production outfit compatibility must fail closed to the disposable working save.");
            int collectorStart = orchestrator.IndexOf(
                "elseif ($Scenario -eq '" + scenario + "')",
                StringComparison.Ordinal);
            Assertions.True(collectorStart >= 0 &&
                orchestrator.Substring(collectorStart,
                    Math.Min(500, orchestrator.Length - collectorStart))
                    .Contains(
                        "[Math]::Max($TimeoutSeconds, 1200) + 15"),
                "Production outfit compatibility needs its exact bounded collector window.");

            Assertions.Equal(28, Regex.Matches(source,
                "new ProductionCompatibilityCase\\(").Count,
                "The production compatibility matrix must contain exactly twenty-eight states.");
            Assertions.True(source.Contains(
                    "ProductionCompatibilityCases.Length != 28") &&
                source.Contains(
                    "ProductionCompatibilityCases.Length)") &&
                source.Contains(
                    "must contain twenty-eight unique states"),
                "The production runtime guard must enforce the same twenty-eight unique states.");
            foreach (string token in new[]
            {
                "abca4797366d4df0831a418eee39069a",
                "afbe88d27a0eb544583e00fa78ffb2c7",
                "559b0b6f194656c428c403a000ceee78",
                "f33dadeeb51cdba45b23bb40a40e5fb3",
                "04dff7841c5f499478c91487d9bbdcef",
                "431d16d2153d1854280b97470223eea6",
                "49641981096de8b43b198e95c7193b65",
                "Progression.CharacterRaces",
                "_gunslingerClass.LoadClothes(",
                "orderedPairExact", "new DollState()",
                "SetClass(_gunslingerClass)", "GetHairEntities()",
                "PollProductionDollCreationReadiness",
                "ResourcesLibrary.Preloading",
                "resource preloading did not finish before native ",
                "dollCreationResourceGatePassed",
                "resourcePreloadingAtDollCreation",
                "dollResourceWaitUpdates",
                "SpawnEntityWithView(dollView,",
                "CreateProductionNeutralBody", "Body.AllSlots",
                "native doll did not settle exactly before",
                "bool nativeDollExact = ReferenceEquals(",
                "_actor.Descriptor.Doll, _dollData)",
                "_dollEntities.All(expected =>",
                "_settleUpdates < MinimumSettleUpdates ||",
                "ProductionFirearms.Pistol.Item",
                "ProductionFirearms.Musket.Item",
                "ProductionFirearms.Blunderbuss.Item",
                "WeaponPresentationEvidenceScenario",
                ".ResolveActivePresentation(_actor, visual,",
                "_expectStoredWeapon ?",
                "weaponPresentationRole",
                "weaponModelRenderable",
                "!_expectHeldWeapon && !_expectStoredWeapon",
                "Body.Armor.InsertItem", "Body.Head.InsertItem",
                "Body.Shoulders.InsertItem",
                "OutfitPartSpecialType.Backpack",
                "UpdateBackpackVisibility(true)",
                "SetProductionPalette(true)",
                "RestoreProductionSnapshot", "RebuildOutfit()",
                "CaptureContactSheet(", "CaptureIsometric(",
                "ProductionBlueprintUnchanged()",
                "productionBlueprintMutated", "saveApiCalled"
            })
                Assertions.True(source.Contains(token),
                    "Production compatibility lacks exact evidence token: " +
                    token);
            int nativeDollGate = source.IndexOf(
                "bool nativeDollExact", StringComparison.Ordinal);
            int productionSnapshot = source.IndexOf(
                "_avatarBefore = TakeProductionSnapshot(_avatar)",
                StringComparison.Ordinal);
            Assertions.True(nativeDollGate >= 0 &&
                productionSnapshot > nativeDollGate,
                "Production compatibility must settle the complete native " +
                "DollData avatar before taking the production mutation snapshot.");
            int creationReadinessCall = source.IndexOf(
                "if (!PollProductionDollCreationReadiness()) return;",
                StringComparison.Ordinal);
            int spawnAfterReadiness = source.IndexOf("SpawnFixture();",
                creationReadinessCall, StringComparison.Ordinal);
            int createDollAfterReadiness = source.IndexOf(
                "_dollData.CreateUnitView(false)", spawnAfterReadiness,
                StringComparison.Ordinal);
            Assertions.True(creationReadinessCall >= 0 &&
                    spawnAfterReadiness > creationReadinessCall &&
                    createDollAfterReadiness > spawnAfterReadiness,
                "Production compatibility must wait for native resource preloading to finish before it creates any DollData view.");
            foreach (string forbidden in new[]
            {
                "SaveGame", "QuickSave", "ScreenCapture", "Input.",
                "Mouse.", "PlayerPrefs", "Game.Instance.Player.Inventory"
            })
                Assertions.False(source.Contains(forbidden),
                    "Production compatibility contains a forbidden save/UI/global-inventory token: " +
                    forbidden);
        }

        internal static void ElementalClassEquipmentReusesProductionTransaction()
        {
            string source = Read("src", "KingmakerGunslinger",
                "RuntimeTesting",
                "GunslingerOutfitProductionCompatibilityScenario.cs");
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string request = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRequest.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string orchestrator = Read("scripts",
                "Invoke-KingmakerRuntimeTest.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");
            const string scenario = "elemental-race-class-equipment";

            Assertions.True(catalog.Contains(
                    "ElementalRaceClassEquipment") &&
                catalog.Contains(scenario) &&
                runner.Contains(".ElementalRaceClassEquipment") &&
                runner.Contains("BeginProductionCompatibility(") &&
                WorkingSavePredicate(request).Contains(
                    "ElementalRaceClassEquipment") &&
                automation.Contains("'" + scenario +
                    "' = [pscustomobject]") &&
                preflight.Contains(
                    scenario + "-only-permits-working-save"),
                "Elemental class/equipment qualification is not wired through every guarded working-save surface.");
            string metadata = automation.Substring(
                automation.IndexOf("'" + scenario +
                    "' = [pscustomobject]", StringComparison.Ordinal), 500);
            Assertions.True(metadata.Contains(
                    "RequiresSaveName = $true") &&
                metadata.Contains(
                    "PermittedSaveName = 'KMG_AUTOMATION_WORKING'") &&
                metadata.Contains(
                    "RequiresManualInteraction = $false"),
                "Elemental class/equipment qualification must fail closed to the disposable working save.");
            int collectorStart = orchestrator.IndexOf(
                "elseif ($Scenario -eq '" + scenario + "')",
                StringComparison.Ordinal);
            Assertions.True(collectorStart >= 0 &&
                orchestrator.Substring(collectorStart,
                    Math.Min(550, orchestrator.Length - collectorStart))
                    .Contains(
                        "[Math]::Max($TimeoutSeconds, 1800) + 15"),
                "Eight elemental fixtures need their exact bounded collector window.");

            foreach (string token in new[]
            {
                "IsElementalRaceClassEquipment",
                "RequireElementalRaces()",
                "BuildElementalFixtures()",
                "ElementalRaceCatalog.RaceCount *",
                ".ElementalRaces.OrderedBlueprints()",
                "_supportedRaces.All(race => fixtureRecords.Count(",
                (char)34 + "raceGuid" + (char)34,
                "race.AssetGuid",
                "elemental-race-class-equipment-index.json",
                "elemental-race-class-equipment-progress.json",
                "elemental-race-class-equipment-fixtures",
                (char)34 + "equipment-matrix" + (char)34,
                "ElementalRaceClassEquipment"
            })
                Assertions.True(source.Contains(token),
                    "Shared production transaction lacks elemental contract token: " +
                    token);
            Assertions.Equal(28, Regex.Matches(source,
                "new ProductionCompatibilityCase\\(").Count,
                "Elemental mode must reuse the expanded twenty-eight-state matrix without duplicating it.");
            foreach (string token in new[]
            {
                "OutfitMediumArmorItemGuid",
                "OutfitRobeItemGuid",
                "OutfitBootsItemGuid",
                "OutfitGlovesItemGuid",
                "OutfitBracersItemGuid",
                "ArmorProficiencyGroup.Medium",
                "_actor.Body.Feet",
                "_actor.Body.Gloves",
                "_actor.Body.Wrist",
                "_actor.Body.Belt",
                "BlueprintBootstrap.CordOfStubbornResolve"
            })
                Assertions.True(source.Contains(token),
                    "Expanded elemental equipment matrix lacks exact token: " +
                    token);
            Assertions.True(source.Contains(
                    "GunslingerOutfitProductionCompatibility") &&
                source.Contains("BuildHumanFixtures()") &&
                source.Contains("expectedFixtures == 2") &&
                source.Contains((char)34 + "Human" + (char)34),
                "The accepted two-Human compatibility path must remain explicitly preserved.");
        }

        internal static void ProductionMotionIsGuardedAndExact()
        {
            string source = Read("src", "KingmakerGunslinger",
                "RuntimeTesting",
                "GunslingerOutfitProductionMotionScenario.cs");
            string shared = Read("src", "KingmakerGunslinger",
                "RuntimeTesting",
                "GunslingerOutfitProductionCompatibilityScenario.cs");
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string request = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRequest.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string orchestrator = Read("scripts",
                "Invoke-KingmakerRuntimeTest.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            const string scenario =
                "gunslinger-outfit-production-motion";
            Assertions.True(catalog.Contains(
                    "GunslingerOutfitProductionMotion") &&
                catalog.Contains(scenario) &&
                runner.Contains("BeginProductionMotion(") &&
                runner.Contains(
                    "_gunslingerOutfitProductionMotion.Poll()") &&
                WorkingSavePredicate(request).Contains(
                    "GunslingerOutfitProductionMotion") &&
                automation.Contains("'" + scenario +
                    "' = [pscustomobject]") &&
                preflight.Contains(
                    scenario + "-only-permits-working-save") &&
                project.Contains(@"RuntimeTesting\GunslingerOutfitProductionMotionScenario.cs"),
                "Production outfit motion is not wired through every guarded working-save surface.");
            string metadata = automation.Substring(
                automation.IndexOf("'" + scenario +
                    "' = [pscustomobject]", StringComparison.Ordinal), 500);
            Assertions.True(metadata.Contains(
                    "RequiresSaveName = $true") &&
                metadata.Contains(
                    "PermittedSaveName = 'KMG_AUTOMATION_WORKING'") &&
                metadata.Contains(
                    "RequiresManualInteraction = $false"),
                "Production outfit motion must fail closed to the disposable working save.");
            int collectorStart = orchestrator.IndexOf(
                "elseif ($Scenario -eq '" + scenario + "')",
                StringComparison.Ordinal);
            Assertions.True(collectorStart >= 0 &&
                orchestrator.Substring(collectorStart,
                    Math.Min(500, orchestrator.Length - collectorStart))
                    .Contains(
                        "[Math]::Max($TimeoutSeconds, 1800) + 15"),
                "Production outfit motion needs its exact bounded collector window.");

            Assertions.Equal(8, Regex.Matches(source,
                "new ProductionMotionSpec\\(").Count,
                "The production motion matrix must contain exactly eight actions.");
            foreach (string token in new[]
            {
                "unarmed-idle", "musket-slow-walk",
                "musket-normal-run", "musket-turn-right",
                "pistol-native-attack", "musket-native-attack",
                "musket-production-reload", "shortsword-native-melee",
                "57c8994d1f1becf49ac4f642e5d8ca9d",
                "ProductionFirearms.Pistol", "ProductionFirearms.Musket",
                "UnitMoveTo", "Pathfinding.ForcedPath",
                "MaxSpeedOverride", "WalkSpeedType.Slow",
                "WalkSpeedType.Normal", "ForceLookAt",
                "locomotionActionPresent",
                "MotionAnimationActionPresent",
                "UnitAttack.CreateAttackCommand", "IsSingleAttack = true",
                "_motionAttackCommand.Init(_actor)",
                "attackReadinessProbeDetached",
                "readinessProbeDetached",
                "_motionAttackCommand.IsInterruptible",
                "retirementReady", "runningCommandTypes",
                "ProductionMotionRunningCommandTypes().Length == 0",
                "RemoveFinishedAndUpdateQueue()", "slotEvicted",
                "residentCommandTypesAfterRetirement",
                "queuedCommandTypesAfterRetirement",
                "ProductionMotionResidentCommandTypes().Length == 0",
                "ProductionMotionQueuedCommandTypes().Length == 0",
                "_actor.Commands.Empty",
                "new AbilityData(", "new UnitUseAbility(",
                "ReloadTestMusketRuntime.Evaluate", "ExecutionProcess.Tick()",
                "ProductionMotionAttackUpdates",
                "ProductionMotionReloadUpdates", "1, 12, 36",
                "1, 12, 36, 96, 160, 240",
                "CaptureContactSheet(", "ProductionMotionOutfitExact()",
                "ProductionEntitiesPresent()", "ProductionRampEvidence()",
                "RestoreProductionSnapshot(_avatarBefore",
                "RestoreProductionMotionInventory",
                "RetireProductionMotionTarget",
                "RetireProductionMotionFactions",
                "PrepareProductionMotionActorBlueprint",
                "blueprint.Brain = null",
                "ConfigureProductionMotionFaction",
                "CreateProductionMotionTarget",
                "if (!PollProductionDollCreationReadiness()) return;",
                "_motionHostileBlueprint.Brain = null",
                "actorAutonomousBrainDisabled",
                "targetAutonomousBrainDisabled",
                "_motionAutonomousCommandsExcluded",
                "acquired an autonomous command before the ",
                "harness-owned UnitAttack; resident=",
                "commandHasAiAction",
                "_motionAttackCommand.AiAction != null",
                "activeCommandAiActions",
                "ProductionMotionResidentCommandAiActions",
                "ProductionMotionHoldingState",
                "Game.Instance.State.LoadedAreaState",
                "_motionLoadedSceneState",
                "new SceneEntitiesState(",
                "SkipSerialize = true",
                "_motionPlayer.CrossSceneState",
                "RefreshProductionMotionPlayerLists",
                "InvalidateCharacterLists",
                "ProductionMotionPlayerListsExact",
                "AttackFactions = new[] { enemy }",
                "IsDirectlyControllable = false",
                "_actor.Group.Memory.Remove(retiredTarget)",
                "ProductionMotionPlayerBoundaryExact",
                "actorTargetBilateralEnemy",
                "actorSharesPlayerGroup",
                "targetSharesPlayerGroup",
                "actorHoldingStateIsRequestLocalLoadedScene",
                "actorInControllableCharacters",
                "targetHoldingStateIsRequestLocalLoadedScene",
                "targetInControllableCharacters",
                "requestLocalSceneMatchesLoadedScene",
                "playerCharacterListsExact",
                "playerHostility=False",
                "ReconcileProductionMotionCombatBoundary",
                "_motionTarget.LeaveCombat()",
                "_actor.LeaveCombat()",
                "_motionTurnBasedController.Tick()",
                "GetController<UnitCombatLeaveController>(true)",
                "GetController<UnitCombatJoinController>(true)",
                "_motionCombatLeaveController.Tick()",
                "_motionCombatJoinController.Tick()",
                "UnitCombatLeaveController.Tick",
                "UnitCombatJoinController.Tick",
                "turnBasedHasEnemyAfterTurnTick",
                "turnBasedUnitsAfterReconcile",
                "playerInCombatAfterReconcile",
                "partyCombatantsAfterReconcile",
                "turnBasedCombatAfterReconcile",
                "gunslinger-outfit-production-motion-combat-boundary",
                "DisposeProductionMotionEntity",
                "RetireProductionMotionScene",
                "_motionSceneState.RemoveEntityData(entity)",
                "_motionSceneDisposed",
                "productionBlueprintMutated", "saveApiCalled"
            })
                Assertions.True(source.Contains(token),
                    "Production motion lacks exact evidence token: " +
                    token);
            Assertions.True(source.Contains(
                    @"(bool)value[""locomotionActionPresent""]") &&
                !source.Contains(
                    @"(int)value[""locomotionClipCount""] > 0"),
                "Production motion must require the native locomotion action and leave its zero-clip implementation detail informational; live movement outcomes prove locomotion behavior.");
            int turnBasedTick = source.IndexOf(
                "_motionTurnBasedController.Tick()",
                StringComparison.Ordinal);
            int leaveTick = source.IndexOf(
                "_motionCombatLeaveController.Tick()",
                turnBasedTick, StringComparison.Ordinal);
            int joinTick = source.IndexOf(
                "_motionCombatJoinController.Tick()", leaveTick,
                StringComparison.Ordinal);
            Assertions.True(turnBasedTick >= 0 && leaveTick > turnBasedTick &&
                    joinTick > leaveTick,
                "Production motion must run native turn-based cache, group leave, and player recomputation in order.");
            Assertions.False(source.Contains(
                    "_motionTarget.CombatState.LeaveCombat()") ||
                source.Contains("_actor.CombatState.LeaveCombat()"),
                "Production motion must not bypass UnitEntityData combat-leave events.");
            Assertions.False(source.Contains("SpawnHostileTarget(") ||
                source.Contains("Game.Instance.Player.Party.Add(") ||
                source.Contains(
                    "Quaternion.identity, _anchor.HoldingState") ||
                source.Contains(
                    "_motionSceneState = _motionAreaState.MainState"),
                "Production motion must use its request-local faction pair and never enlist the working-save party.");
            int probeStart = source.IndexOf(
                "private void PrepareProductionMotionAttackTarget()",
                StringComparison.Ordinal);
            int liveAttackStart = probeStart < 0 ? -1 : source.IndexOf(
                "private void StartProductionMotionAttack()", probeStart,
                StringComparison.Ordinal);
            Assertions.True(probeStart >= 0 && liveAttackStart > probeStart,
                "Production motion lacks distinct readiness-probe and live-attack methods.");
            string probeBody = source.Substring(probeStart,
                liveAttackStart - probeStart);
            Assertions.True(
                    probeBody.Contains("_motionAttackCommand.Init(_actor)") &&
                    !probeBody.Contains(
                        "_actor.Commands.Run(_motionAttackCommand)") &&
                    !probeBody.Contains("_actor.Commands.InterruptAll(true)"),
                "Attack readiness must use a detached native UnitAttack probe; only the separately constructed evidence command may run.");
            int actorBrainDisabled = source.IndexOf(
                "blueprint.Brain = null", StringComparison.Ordinal);
            int targetBrainDisabled = source.IndexOf(
                "_motionHostileBlueprint.Brain = null",
                actorBrainDisabled, StringComparison.Ordinal);
            int combatJoin = source.IndexOf(
                "_actor.CombatState.JoinCombat()",
                targetBrainDisabled, StringComparison.Ordinal);
            int autonomousCommandGate = source.IndexOf(
                "_motionAutonomousCommandsExcluded =",
                combatJoin, StringComparison.Ordinal);
            int readyCapture = source.IndexOf(
                "CaptureProductionMotionRecord(_motionSpec.Label +",
                autonomousCommandGate, StringComparison.Ordinal);
            Assertions.True(actorBrainDisabled >= 0 &&
                    targetBrainDisabled > actorBrainDisabled &&
                    combatJoin > targetBrainDisabled &&
                    autonomousCommandGate > combatJoin &&
                    readyCapture > autonomousCommandGate,
                "Production motion must disable AI only on both request-local clones before combat and reject autonomous commands before accepting an attack evidence frame.");
            int removalStart = source.IndexOf(
                "private void BeginProductionMotionRemoval()",
                StringComparison.Ordinal);
            int removeWeaponStart = removalStart < 0 ? -1 : source.IndexOf(
                "private void RemoveProductionMotionWeapon()", removalStart,
                StringComparison.Ordinal);
            Assertions.True(removalStart >= 0 &&
                    removeWeaponStart > removalStart,
                "Production motion lacks a bounded teardown method.");
            string removalBody = source.Substring(removalStart,
                removeWeaponStart - removalStart);
            int interruptCommands = removalBody.IndexOf(
                "_actor.Commands.InterruptAll(true)",
                StringComparison.Ordinal);
            int evictFinishedCommand = removalBody.IndexOf(
                "_actor.Commands.RemoveFinishedAndUpdateQueue()",
                StringComparison.Ordinal);
            int emptySlotGate = removalBody.IndexOf(
                "if (!slotEvicted || !_actor.Commands.Empty",
                StringComparison.Ordinal);
            int removeWeapon = removalBody.IndexOf(
                "RemoveProductionMotionWeapon();",
                StringComparison.Ordinal);
            Assertions.True(interruptCommands >= 0 &&
                    evictFinishedCommand > interruptCommands &&
                    emptySlotGate > evictFinishedCommand &&
                    removeWeapon > emptySlotGate,
                "Production motion must interrupt, evict finished native command slots, prove an empty command container, and only then remove the weapon.");
            foreach (string token in new[]
            {
                "IsProductionMotion", "PollProductionMotion()",
                "PrepareProductionMotionActorBlueprint(_actorBlueprint)",
                "SceneEntitiesState holdingState = IsProductionMotion",
                "ProductionMotionHoldingState()",
                "productionMotionDollBeforeAttach",
                "productionMotionDollAfterSpawnBeforeTick",
                "productionMotionDollAfterAttach",
                "productionMotionDollAtSettleTimeout",
                "productionMotionDollCreationReadiness",
                "PollProductionDollCreationReadiness",
                "dollCreationResourceGatePassed",
                "resourcePreloadingAtDollCreation",
                "dollResourceWaitUpdates",
                "DescribeProductionDollLifecycle",
                "ResourcesLibrary.Preloading",
                "_dollTemplateAvatar, _avatar",
                "PrepareProductionMotionCleanup()",
                "RetireProductionMotionFactions()",
                "RestoreProductionMotionInventory()",
                "FinishProductionMotion(cleaned)", "SameReferences("
            })
                Assertions.True(shared.Contains(token),
                    "Shared production fixture lacks motion hook token: " +
                    token);
            int dollBeforeAttach = shared.IndexOf(
                "productionMotionDollBeforeAttach",
                StringComparison.Ordinal);
            int dollAfterSpawn = shared.IndexOf(
                "productionMotionDollAfterSpawnBeforeTick",
                StringComparison.Ordinal);
            int dollAfterAttach = shared.IndexOf(
                "productionMotionDollAfterAttach",
                StringComparison.Ordinal);
            int dollSettleTimeout = shared.IndexOf(
                "productionMotionDollAtSettleTimeout",
                StringComparison.Ordinal);
            Assertions.True(dollBeforeAttach >= 0 &&
                    dollAfterSpawn > dollBeforeAttach &&
                    dollAfterAttach > dollAfterSpawn &&
                    dollSettleTimeout > dollAfterAttach,
                "Production motion must record bounded native DollData state before spawn, after spawn, after attachment, and at settle timeout.");
            foreach (string forbidden in new[]
            {
                "SaveGame", "QuickSave", "ScreenCapture", "Input.",
                "Mouse.", "PlayerPrefs"
            })
                Assertions.False(source.Contains(forbidden),
                    "Production motion contains a forbidden save/UI token: " +
                    forbidden);
        }

        internal static void ElementalRaceMotionReusesProductionTransaction()
        {
            string source = Read("src", "KingmakerGunslinger",
                "RuntimeTesting",
                "GunslingerOutfitProductionMotionScenario.cs");
            string transitions = Read("src", "KingmakerGunslinger",
                "RuntimeTesting",
                "ElementalRaceTransitionScenario.cs");
            string shared = Read("src", "KingmakerGunslinger",
                "RuntimeTesting",
                "GunslingerOutfitProductionCompatibilityScenario.cs");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string request = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRequest.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string orchestrator = Read("scripts",
                "Invoke-KingmakerRuntimeTest.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");
            const string scenario = "elemental-race-motion";

            Assertions.True(catalog.Contains("ElementalRaceMotion") &&
                catalog.Contains(scenario) &&
                runner.Contains(".ElementalRaceMotion") &&
                runner.Contains("BeginProductionMotion(") &&
                WorkingSavePredicate(request).Contains(
                    "ElementalRaceMotion") &&
                automation.Contains("'" + scenario +
                    "' = [pscustomobject]") &&
                preflight.Contains(
                    scenario + "-only-permits-working-save"),
                "Elemental race motion is not wired through every guarded working-save surface.");
            int metadataStart = automation.IndexOf("'" + scenario +
                "' = [pscustomobject]", StringComparison.Ordinal);
            string metadata = automation.Substring(metadataStart,
                Math.Min(500, automation.Length - metadataStart));
            Assertions.True(metadata.Contains(
                    "RequiresSaveName = $true") &&
                metadata.Contains(
                    "PermittedSaveName = 'KMG_AUTOMATION_WORKING'") &&
                metadata.Contains(
                    "RequiresManualInteraction = $false"),
                "Elemental race motion must fail closed to the disposable working save.");
            int collectorStart = orchestrator.IndexOf(
                "elseif ($Scenario -eq '" + scenario + "')",
                StringComparison.Ordinal);
            Assertions.True(collectorStart >= 0 &&
                orchestrator.Substring(collectorStart,
                    Math.Min(500, orchestrator.Length - collectorStart))
                    .Contains(
                        "[Math]::Max($TimeoutSeconds, 7200) + 15"),
                "Eight elemental motion fixtures need their exact bounded collector window.");

            foreach (string token in new[]
            {
                "IsElementalRaceMotion",
                "RuntimeTestScenarioCatalog.ElementalRaceMotion",
                "ElementalRaceCatalog.RaceCount * 2",
                "expectedRecords = expectedFixtures * 27",
                "expectedFixtures * 5",
                "expectedFixtures * 8",
                "expectedFixtures * 2",
                "expectedFixtures * 3",
                "records.Count(value => string.Equals(",
                "(string)value[" + (char)34 + "raceGuid" +
                    (char)34 + "], race.AssetGuid",
                "== 54",
                "elemental-race-motion-index.json",
                "elemental-race-motion-guard",
                "elemental-race-motion-fixtures",
                "elemental-race-motion-captures",
                "elemental-race-native-locomotion",
                "elemental-race-native-turn",
                "elemental-race-native-attacks",
                "elemental-race-native-reload",
                "elemental-race-motion-restoration",
                "elemental-race-motion-combat-boundary",
                "elemental-race-motion-blueprint-immutability",
                "elemental-race-motion-cleanup"
            })
                Assertions.True(source.Contains(token),
                    "Elemental motion lacks exact reuse token: " + token);
            foreach (string token in new[]
            {
                "expectedTransitionRecords",
                "ElementalTransitionActions.Length * 2",
                "transitionRecords.Count(value => string.Equals(",
                "== 16",
                "elemental-race-native-sla-casting",
                "elemental-race-native-prone-recovery",
                "elemental-race-native-death-resurrection",
                "elemental-race-native-polymorph-return"
            })
                Assertions.True(source.Contains(token),
                    "Elemental motion lacks transition assertion token: " +
                    token);
            Assertions.True(project.Contains(
                    "RuntimeTesting\\ElementalRaceTransitionScenario.cs"),
                "The elemental transition partial is absent from the explicit production compile list.");
            foreach (string token in new[]
            {
                "UnitUseAbility", "Animation.IsActed",
                "ExecutionProcess.Tick()", "resourceBefore",
                "resourceAfter", "UnitCondition.Prone",
                "PersistantResources", ".Resources.Add(",
                "elementalSlaReadiness=",
                "IAbilityAvailabilityProvider",
                "availabilitySettleUpdates",
                "racial SLA availability (",
                "targetDeferred=True",
                "hostile-2m-forward-navmesh",
                "if (_motionTarget == null)",
                "hostilePlacementCount",
                "target was not placed exactly once",
                "SetProductionMotionUnitPosition(_motionTarget",
                "IsUnitEnoughClose", "ApproachRadius",
                "readiness.Init(_actor)",
                "readinessProbeDetached",
                (char)34 + "targetInState" + (char)34,
                (char)34 + "commandCloseEnough" + (char)34,
                "RuleDealDamage", "ResurrectAndFullRestore",
                "CreateProductionMotionTarget()",
                "_actor.Blueprint.IsCheater = false",
                "request-local-hostile",
                "_elementalDamageAfterLethal",
                "_elementalConstitutionAtLethal",
                "Stats.Constitution.ModifiedValue",
                "BeastShapeTwoSpellGuid",
                "5d4028eb28a106d4691ed1b92bbb1915",
                "8dc6510d31614345a8c718208fbac1f8",
                "Body.IsPolymorphed", "ActiveRenderers(_actor)",
                "sharedMaterials", "material.shader",
                "racial-sla-native-cast-acted",
                "native-prone-restored", "native-resurrected",
                "beast-shape-ii-restored",
                "ElementalTransitionMaximumUpdates",
                "CleanupElementalRaceTransitions",
                (char)34 + "saveApiCalled" + (char)34
            })
                Assertions.True(transitions.Contains(token),
                    "Elemental transition partial lacks exact native token: " +
                    token);
            int availabilityGate = transitions.IndexOf("if (!available)",
                StringComparison.Ordinal);
            int targetCreation = transitions.IndexOf(
                "TargetWrapper target = ElementalSpellTarget();",
                StringComparison.Ordinal);
            Assertions.True(availabilityGate >= 0 &&
                targetCreation > availabilityGate,
                "Elemental transition target construction must remain behind " +
                "the native ability-availability settle gate.");
            foreach (string token in new[]
            {
                "UsesElementalRaceFixtures",
                "IsElementalRaceClassEquipment ||",
                "IsElementalRaceMotion",
                "_supportedRaces = UsesElementalRaceFixtures",
                "_fixtures = UsesElementalRaceFixtures",
                "BuildElementalFixtures()",
                "elemental-race-motion-exception",
                "elemental-race-motion-progress.json",
                "elemental-race-motion-cleanup"
            })
                Assertions.True(shared.Contains(token),
                    "Shared production transaction lacks elemental motion token: " +
                    token);
            Assertions.True(source.Contains(": 2;") &&
                source.Contains(
                    "gunslinger-outfit-production-motion-index.json") &&
                source.Contains(
                    "gunslinger-outfit-production-motion-guard") &&
                source.Contains(
                    "one exact male and female Human production DollData fixture"),
                "The accepted two-Human motion mode was not preserved.");
            foreach (string forbidden in new[]
            {
                "SaveGame", "QuickSave", "PlayerPrefs", "Input.",
                "Mouse."
            })
                Assertions.False(source.Contains(forbidden) ||
                    transitions.Contains(forbidden),
                    "Elemental motion contains forbidden save/UI token: " +
                    forbidden);
        }

        internal static void ProductionPersistenceIsGuardedAndExact()
        {
            string source = Read("src", "KingmakerGunslinger",
                "RuntimeTesting",
                "GunslingerOutfitProductionPersistenceScenario.cs");
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string workingSave = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "WorkingSaveSmokeScenario.cs");
            string request = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRequest.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string orchestrator = Read("scripts",
                "Invoke-KingmakerRuntimeTest.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            string[] scenarios =
            {
                "gunslinger-outfit-production-persistence-prepare",
                "gunslinger-outfit-production-persistence",
                "gunslinger-outfit-production-persistence-verify-absent"
            };
            Assertions.True(catalog.Contains(
                    "GunslingerOutfitProductionPersistence") &&
                scenarios.All(catalog.Contains) &&
                catalog.Contains(
                    "IsGunslingerOutfitProductionPersistenceScenario") &&
                runner.Contains("BeginProductionPersistence(") &&
                runner.Contains(
                    "_gunslingerOutfitProductionPersistence.Poll()") &&
                WorkingSavePredicate(request).Contains(
                    "GunslingerOutfitProductionPersistence") &&
                scenarios.All(value => automation.Contains("'" + value +
                    "' = [pscustomobject]")) &&
                scenarios.All(preflight.Contains) &&
                project.Contains(@"RuntimeTesting\GunslingerOutfitProductionPersistenceScenario.cs"),
                "Production outfit persistence is not wired through every guarded working-save surface.");
            Assertions.True(Regex.IsMatch(workingSave,
                    @"AutomationWorkingWithOutfitFixture\s*=\s*new WorkingSaveSmokeIdentity\(.*?JamandisMansion"", 4\);",
                    RegexOptions.Singleline) &&
                runner.Contains(
                    ".AutomationWorkingWithOutfitFixture"),
                "Marker-bearing verification must require the exact four-member working-save identity before cleanup.");
            foreach (string scenario in scenarios)
            {
                int metadataStart = automation.IndexOf("'" + scenario +
                    "' = [pscustomobject]", StringComparison.Ordinal);
                Assertions.True(metadataStart >= 0,
                    "Missing persistence metadata for " + scenario + ".");
                string metadata = automation.Substring(metadataStart,
                    Math.Min(500, automation.Length - metadataStart));
                Assertions.True(metadata.Contains(
                        "RequiresSaveName = $true") &&
                    metadata.Contains(
                        "PermittedSaveName = 'KMG_AUTOMATION_WORKING'") &&
                    metadata.Contains(
                        "RequiresManualInteraction = $false"),
                    "Persistence phase must fail closed to the disposable working save: " +
                    scenario);
            }
            int collectorStart = orchestrator.IndexOf(
                "gunslinger-outfit-production-persistence-prepare",
                StringComparison.Ordinal);
            Assertions.True(collectorStart >= 0 &&
                orchestrator.Substring(collectorStart,
                    Math.Min(700, orchestrator.Length - collectorStart))
                    .Contains(
                        "[Math]::Max($TimeoutSeconds, 1200) + 15"),
                "Production outfit persistence needs its exact bounded collector window.");
            Assertions.Equal(2, Regex.Matches(source,
                @"new PersistenceFixture\(").Count,
                "Persistence qualification must contain exact male and female Human native-respec fixtures.");
            foreach (string token in new[]
            {
                "PersistedOutfitFixtureUniqueId",
                "PersistedOutfitFixtureName",
                "GunslingerOutfitProductionPersistencePrepare",
                "GunslingerOutfitProductionPersistenceVerifyAbsent",
                "Progression.GetClassLevel(", "Descriptor.Doll",
                "m_EquipmentClass", "UpdateClassEquipment()",
                "RebuildOutfit()", "SerializedClassClothesAbsent",
                "persisted-native-class-reconstruction",
                "RestorePersistedAvatar", "TakeAvatarSnapshot",
                "SavedEquipmentEntities",
                "StartWithoutAssigningStaticInstance",
                "LevelUpState.CharBuildMode.CharGen",
                "LevelUpState.CharBuildMode.Respec", "SelectClass",
                "ApplyClassMechanics", "ApplyLevelup", "Commit",
                "sourceFighterExact", "previewGunslingerLevel",
                "committedGunslingerLevel", "defaultPaletteExact",
                "replacementLevelBeforeRespec",
                "distinctSourceAndReplacement", "respecMode",
                "_respecSourceActor", "RetireRespecSource",
                "RollbackStarterGrants",
                "DefaultPrimaryColor", "DefaultSecondaryColor",
                "CaptureContactSheet(", "CaptureIsometric(",
                "SameValues(expectedRemote", "PartyCharacters",
                "ArmExactWorkingSaveWrite", "WorkingDescriptor",
                "ExpectedWorkingSaveRoutineCount", "RemoveEntityData",
                "productionBlueprintMutated", "saveApiCalledAtCapture",
                "GroupBy(value => value.RaceId)",
                "OrderBy(value => value.AssetGuid",
                "loadedFixtureMembership", "markedCrossUnits",
                "cleanupCandidates", "expectedCross",
                "ForcceUseClassEquipment", "RespecRecordExact",
                "SetValue(_actor.View, null)",
                "postRefreshEquipmentClassExact",
                "BeginPersistedViewActivation",
                "preActivationAppearanceExact",
                "UpdateViewActive()", "SetVisible(true, true)",
                "persistedViewActivationExact",
                "refused to promote or save"
            })
                Assertions.True(source.Contains(token),
                    "Production persistence lacks exact evidence token: " +
                    token);
            foreach (string id in new[]
            {
                "9875b1f3cf3b8bf42a5fb99907e5a794",
                "551682302c6f9b146b7657c52b5cabac",
                "67b5adfbb99269b43bb3ca00438626c8",
                "04e8446d4666d6a46b28a98c55ec9f6c",
                "d771acb96d986484dbd006a78a65cdba",
                "8061ab0f406f7f84e8d36eada05f97a7"
            })
                Assertions.True(source.Contains(id),
                    "Persistence qualification lacks audited historical Fighter entity: " +
                    id);
            foreach (string forbidden in new[]
            {
                "QuickSave", "ScreenCapture",
                "Input.", "Mouse.", "PlayerPrefs",
                "gameObject.SetActive"
            })
                Assertions.False(source.Contains(forbidden),
                    "Production persistence contains a forbidden save/UI token: " +
                    forbidden);
            /* Replaced below with a quote-agnostic source regex.
            Assertions.Equal(1, Regex.Matches(source,
                "value.Name == \\SaveGame\\").Count,
                "Persistence must expose one exact native SaveGame reflection boundary."); */
            Assertions.Equal(1, Regex.Matches(source,
                "value.Name == .SaveGame.").Count,
                "Persistence must expose one exact native SaveGame reflection boundary.");
            int snapshot = source.IndexOf(
                "_persistedAvatarBefore = TakeAvatarSnapshot(",
                StringComparison.Ordinal);
            int force = source.IndexOf(
                "_equipmentClassField.SetValue(_persistedUnit.View, null)",
                StringComparison.Ordinal);
            int restore = source.IndexOf(
                "_persistedRestored = RestorePersistedAvatar()",
                StringComparison.Ordinal);
            Assertions.True(snapshot >= 0 && force > snapshot &&
                    restore > force,
                "The real loaded avatar must be snapshotted before forced native reconstruction and restored afterward.");
        }

        internal static void ElementalRacePersistenceIsGuardedAndExact()
        {
            string source = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "ElementalRacePersistenceScenario.cs");
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string workingSave = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "WorkingSaveSmokeScenario.cs");
            string request = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRequest.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string launcher = Read("scripts",
                "Invoke-KingmakerRuntimeTest.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");
            string sequence = Read("scripts",
                "Invoke-ElementalRacePersistenceQualification.ps1");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            string[] scenarios =
            {
                "elemental-race-persistence-prepare",
                "elemental-race-module-disabled-persistence",
                "elemental-race-persistence-verify-absent"
            };

            Assertions.True(catalog.Contains(
                    "ElementalRacePersistencePrepare") &&
                catalog.Contains(
                    "ElementalRaceModuleDisabledPersistence") &&
                catalog.Contains(
                    "ElementalRacePersistenceVerifyAbsent") &&
                scenarios.All(catalog.Contains) &&
                catalog.Contains(
                    "IsElementalRacePersistenceScenario") &&
                runner.Contains("BeginElementalRacePersistence(") &&
                runner.Contains("_elementalRacePersistence.Poll()") &&
                WorkingSavePredicate(request).Contains(
                    "IsElementalRacePersistenceScenario") &&
                scenarios.All(value => automation.Contains("'" + value +
                    "' = [pscustomobject]")) &&
                scenarios.All(preflight.Contains) &&
                project.Contains(
                    @"RuntimeTesting\ElementalRacePersistenceScenario.cs"),
                "Elemental persistence is not wired through every guarded working-save surface.");
            Assertions.True(Regex.IsMatch(workingSave,
                    @"AutomationWorkingWithElementalFixtures\s*=\s*new WorkingSaveSmokeIdentity\(.*?JamandisMansion"", 11\);",
                    RegexOptions.Singleline) &&
                runner.Contains(
                    ".AutomationWorkingWithElementalFixtures"),
                "Module-disabled verification must require the exact eleven-member marker-bearing working-save identity.");
            foreach (string scenario in scenarios)
            {
                int metadataStart = automation.IndexOf("'" + scenario +
                    "' = [pscustomobject]", StringComparison.Ordinal);
                Assertions.True(metadataStart >= 0,
                    "Missing elemental persistence metadata for " +
                    scenario + ".");
                string metadata = automation.Substring(metadataStart,
                    Math.Min(500, automation.Length - metadataStart));
                Assertions.True(metadata.Contains(
                        "RequiresSaveName = $true") &&
                    metadata.Contains(
                        "PermittedSaveName = 'KMG_AUTOMATION_WORKING'") &&
                    metadata.Contains(
                        "RequiresManualInteraction = $false") &&
                    metadata.Contains(
                        "ReadinessBehavior = 'autonomous-working-save'"),
                    "Elemental persistence phase must fail closed to the disposable working save: " +
                    scenario);
            }
            int collectorStart = launcher.IndexOf(
                "elemental-race-persistence-prepare",
                StringComparison.Ordinal);
            Assertions.True(collectorStart >= 0 &&
                launcher.Substring(collectorStart,
                    Math.Min(750, launcher.Length - collectorStart))
                    .Contains(
                        "[Math]::Max($TimeoutSeconds, 1800) + 15"),
                "Elemental persistence needs its exact bounded collector window.");

            foreach (string id in new[]
            {
                "a9be3b86-9d80-472a-93e6-71fcfb3a827a",
                "2fc7d5a4-5dab-4bb9-bee1-da1fdfa2a337",
                "f4933068-5824-46fa-a330-25b78764503e",
                "27a98188-4106-419d-8897-64ccd6f63305",
                "d532ec12-a328-4afb-8cbf-7f3ddf41f072",
                "08e1cd1d-4512-4c52-a9fa-6dd8d815499a",
                "043d4fc2-c26c-4e72-9d11-219d0ff74b43",
                "91472289-c1d7-4558-b7ed-a5e8c06345fb"
            })
                Assertions.Equal(1, Regex.Matches(source,
                        Regex.Escape(id)).Count,
                    "Elemental persistence needs one stable fixture identity: " +
                    id);
            foreach (string token in new[]
            {
                "ElementalRaceCatalog.RaceCount * 2",
                "exactly eight race/sex fixtures",
                "ElementalRacePersistencePrepare",
                "ElementalRaceModuleDisabledPersistence",
                "ElementalRacePersistenceVerifyAbsent",
                "_context.FeatureModules.Active.ElementalRaces",
                "BlueprintRoot.Instance",
                ".Progression.CharacterRaces",
                "ResourcesLibrary.TryGetBlueprint<",
                "Progression.Race", "race.Features.All",
                "_currentBlueprint.IsCheater = false",
                "!unit.Blueprint.IsCheater",
                "Descriptor.CustomGender = fixture.Gender",
                "owner.CustomGender.HasValue",
                "owner.Stats.GetStat(value.Stat)",
                "StatType.SkillPerception",
                "owner.Stats.Speed.ModifiedValue",
                "owner.Resources.GetResourceAmount(",
                "SlaResource.GetMaxAmount(owner)",
                "AbilityType.SpellLike", "ability.Spellbook == null",
                "!ability.IsAffectedByArcaneSpellFailure",
                "PerformNativeElementalRespec",
                "LevelUpState.CharBuildMode.Respec",
                "SeedFixedElementalRespecRace(_currentBlueprint, fixture)",
                "SeedFixedElementalRespecFacts(",
                "EnsureElementalRespecFact(owner, feature)",
                "fixedRaceBeforeRespec",
                "fixedRaceFactsBeforeRespec",
                "seededSlaResourceBeforeRespec",
                "seededSlaAvailableBeforeRespec",
                "fixedRaceInInitialPreview",
                "fixedRaceFactsInInitialPreview",
                "\"racePreserved\", previewRaceExact",
                "controller.SelectClass(_gunslingerClass, false)",
                "ConfigureExpectedDollState(controller.Doll, fixture)",
                "controller.Commit()",
                "distinctSourceAndReplacement",
                "replacementLevelBeforeRespec",
                "NativeElementalRespecRecordExact",
                "RetireElementalRespecSource",
                "elemental-race-persistence-native-respec",
                "nativeRespecRecords",
                "InvokeAbilitySpend(", "AbilityResourceLogic",
                "costs[0].Spend(ability)",
                "RestController.ApplyRest(",
                "LevelUpState.CharBuildMode.CharGen",
                "ApplyLevelup", "Progression.CharacterLevel",
                "CreateExecutionContext(", "Params.CasterLevel",
                "Descriptor.Doll", "CreateExpectedDollData(",
                "SelectPairedOption(", "presetExact=",
                "PersistenceDollSnapshot.Capture(",
                "GunslingerClassAppearanceCatalog.MaleAssetIds()",
                "GunslingerClassAppearanceCatalog.FemaleAssetIds()",
                "SerializedElementalClassClothesAbsent",
                "HasExactHumanoidRig(", "materialsExact",
                "CaptureContactSheet(", "CaptureIsometric(",
                "ElementalPersistenceFixtureCount * 5",
                "PartyCharacters", "CrossSceneState.AllEntityData",
                "ArmExactWorkingSaveWrite", "RemoveEntityData",
                "string.IsNullOrWhiteSpace(_exceptionSummary)",
                "\"exceptionSummary\", _exceptionSummary",
                "ExpectedWorkingSaveRoutineCount",
                "elemental-race-persistence-module-off",
                "elemental-race-persistence-rest-and-level-up",
                "elemental-race-persistence-absence",
                "protected baseline excluded"
            })
                Assertions.True(source.Contains(token),
                    "Elemental persistence lacks exact native guard/evidence token: " +
                    token);
            foreach (string forbidden in new[]
            {
                "QuickSave", "ScreenCapture", "Input.", "Mouse.",
                "PlayerPrefs", "gameObject.SetActive"
            })
                Assertions.False(source.Contains(forbidden),
                    "Elemental persistence contains a forbidden save/UI token: " +
                    forbidden);
            Assertions.Equal(1, Regex.Matches(source,
                "value.Name == .SaveGame.").Count,
                "Elemental persistence must expose one exact native SaveGame reflection boundary.");
            int nativeCreation = source.IndexOf(
                "ApplyNativeCharacterCreation(fixture, data)",
                StringComparison.Ordinal);
            int visualRaceAssignment = source.IndexOf(
                "_currentBlueprint.Race = fixture.Blueprints.Race",
                StringComparison.Ordinal);
            Assertions.True(nativeCreation >= 0 &&
                    visualRaceAssignment > nativeCreation &&
                    source.Contains(
                        "if (!controller.SelectRace(fixture.Blueprints.Race))"),
                "The fixture must execute native race selection before assigning the clone's visual race.");

            foreach (string token in new[]
            {
                "[ValidateSet('KMG_AUTOMATION_WORKING')]",
                "ReadAllBytes($settings)",
                "WriteAllBytes($temporary, $originalBytes)",
                "[Convert]::ToBase64String($restored)",
                "schemaVersion = 10",
                "'elemental-races' = $enabled",
                "Set-ElementalRacesEnabled $true",
                "Set-ElementalRacesEnabled $false",
                "Restore-OriginalFeatureState",
                "-ReuseInstalledArtifact",
                "Wait-ForGuardedKingmakerExit"
            })
                Assertions.True(sequence.Contains(token),
                    "Three-launch persistence orchestration lacks exact restoration token: " +
                    token);
            Assertions.False(sequence.Contains("KMG_AUTOMATION_BASELINE"),
                "The protected baseline must never be eligible for the persistence sequence.");
            int enable = sequence.IndexOf(
                "Set-ElementalRacesEnabled $true",
                StringComparison.Ordinal);
            int prepare = sequence.IndexOf(
                "& $invoke -Scenario 'elemental-race-persistence-prepare'",
                StringComparison.Ordinal);
            int disable = sequence.IndexOf(
                "Set-ElementalRacesEnabled $false",
                StringComparison.Ordinal);
            int verify = sequence.IndexOf(
                "& $invoke -Scenario 'elemental-race-module-disabled-persistence'",
                StringComparison.Ordinal);
            int finalizer = sequence.IndexOf(
                "finally {", verify, StringComparison.Ordinal);
            int restore = sequence.IndexOf(
                "    Restore-OriginalFeatureState", finalizer,
                StringComparison.Ordinal);
            Assertions.True(enable >= 0 && prepare > enable &&
                    disable > prepare && verify > disable &&
                    finalizer > verify && restore > finalizer,
                "The exact enabled-prepare, disabled-verify-cleanup, and finally-restored launch order changed.");
            Assertions.False(sequence.Contains(
                    "-Scenario 'elemental-race-persistence-verify-absent'"),
                "Fresh-load absence must run only after this transaction has returned and restored settings.");
            Assertions.True(sequence.Contains(
                    "run elemental-race-persistence-verify-absent next"),
                "The two-launch transaction must explicitly hand off the restored-settings fresh-load phase.");
        }

        internal static void FinalistRaceMatrixIsExactAndReversible()
        {
            string source = QualificationSource();
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string request = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRequest.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string orchestrator = Read("scripts",
                "Invoke-KingmakerRuntimeTest.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            const string scenario =
                "gunslinger-outfit-finalist-race-matrix";
            Assertions.True(catalog.Contains(
                    "internal const string GunslingerOutfitFinalistRaceMatrix") &&
                catalog.Contains(scenario) &&
                runner.Contains(
                    "BeginFinalistRaceMatrix(_context, _request)") &&
                runner.Contains(
                    "_gunslingerOutfitFinalistRaceMatrix.Poll()") &&
                WorkingSavePredicate(request).Contains(
                    "GunslingerOutfitFinalistRaceMatrix") &&
                automation.Contains("'" + scenario +
                    "' = [pscustomobject]") &&
                preflight.Contains(
                    scenario + "-only-permits-working-save") &&
                project.Contains(
                    @"RuntimeTesting\GunslingerOutfitQualificationScenario.cs"),
                "Finalist race matrix is not wired through every guarded working-save surface.");

            string metadata = automation.Substring(
                automation.IndexOf("'" + scenario +
                    "' = [pscustomobject]", StringComparison.Ordinal), 500);
            Assertions.True(metadata.Contains(
                    "RequiresSaveName = $true") &&
                metadata.Contains(
                    "PermittedSaveName = 'KMG_AUTOMATION_WORKING'") &&
                metadata.Contains(
                    "RequiresManualInteraction = $false"),
                "Finalist race matrix metadata must fail closed to the disposable working save.");
            int collectorStart = orchestrator.IndexOf(
                "elseif ($Scenario -eq '" + scenario + "')",
                StringComparison.Ordinal);
            Assertions.True(collectorStart >= 0,
                "Finalist race matrix needs a bounded result collector.");
            string collector = orchestrator.Substring(collectorStart,
                Math.Min(500, orchestrator.Length - collectorStart));
            Assertions.True(collector.Contains(
                    "[Math]::Max($TimeoutSeconds, 1200) + 15"),
                "Finalist race matrix collector must preserve its exact scenario-only ceiling.");

            foreach (string token in new[]
            {
                "BlueprintRoot.Instance",
                "Progression.CharacterRaces",
                "GroupBy(value => value.RaceId)",
                "Gender.Male, Gender.Female",
                "GetBlueprints<BlueprintUnit>()",
                "value.Race.RaceId == race.RaceId",
                "ExpectedPlayerRaceSize",
                "race == Race.Gnome || race == Race.Halfling",
                "value.Size == expectedSize",
                "TryAdvanceDonor",
                "RejectCurrentDonor",
                "avatar-roundtrip-restoration-not-exact",
                "donorRejections",
                "donorAttemptIndex",
                "initialRoundTripRestored",
                "originalEmpty",
                "gunslinger-outfit-finalist-donor-selection",
                "BlueprintRaceVisualPreset",
                "race.Presets",
                "race.Presets[0]",
                "new DollState()",
                "SetGender(fixture.Gender)",
                "SetRace(fixture.Race)",
                "SetRacePreset(fixture.Preset)",
                "SetClass(_magusClass)",
                "CreateData()",
                "CreateUnitView(false)",
                "dollView.GetComponent<Character>()",
                "dollView.Blueprint = _actorBlueprint",
                "dollView.UniqueId = Guid.NewGuid().ToString()",
                "SpawnEntityWithView(dollView,",
                "ReferenceEquals(_actor.View, dollView)",
                "dollView = null;",
                "CreateNeutralQualificationBody(fixture.Source)",
                "EmptyHandWeapon = source.Body.EmptyHandWeapon",
                "neutral-body-created-items",
                "requestLocalNeutralBody",
                "Body.AllSlots",
                "slot.RemoveItem(false)",
                "character-creation-doll-not-neutral",
                "unexpectedDollEntityCount",
                "fixture.Preset.RaceId",
                "racePresetVisualRaceId",
                "gunslinger-outfit-finalist-character-creation-dolls",
                "CleanupSnapshotDiagnostic",
                "DescribeRuntimeReference",
                "cleanupSnapshot=",
                "missingUnits",
                "unexpectedUnits",
                "CaptureActorOwnedDependents",
                "_actor.Descriptor.Pet",
                "_unitsBefore.Any(value => ReferenceEquals(value,",
                "RetireActorOwnedDependents",
                "Game.Instance.Player.Party.Remove(dependent)",
                "ownedDependentUnits",
                "ownedDependentsCleared",
                "MagusClassGuid",
                "45a4607686d96a1498891b3286121780",
                "_magusClass.LoadClothes(",
                "fixture.Gender, fixture.Race)",
                "orderedPairExact",
                "_magusClass.PrimaryColor != _finalist.Primary",
                "expectedFixtures = _races.Length * 2",
                "expectedRecords = expectedFixtures * 2",
                "ApplyQualificationPalette",
                "CaptureContactSheet(",
                "CaptureIsometric(",
                "QualificationFeatureNodes",
                "RemoveEquipmentEntities(_classEntities, false)",
                "AddEquipmentEntities(_candidateEntities, false)",
                "RemoveAllEquipmentEntities(false)",
                "QualificationSavedLinks",
                "productionBlueprintMutated",
                "saveApiCalled",
                "KMG_AUTOMATION_WORKING; no save API"
            })
                Assertions.True(source.Contains(token),
                    "Finalist race matrix lacks exact guard/evidence token: " +
                    token);
            Assertions.True(source.Contains(
                    "_avatarBefore.Length == 0") &&
                source.Contains(
                    "character-creation-doll-not-neutral"),
                "Finalist qualification must reject an empty or prefab-contaminated avatar instead of treating it as a neutral player doll.");
            Assertions.False(source.Contains(
                    "value.RaceId == race.RaceId"),
                "A progression race must not be equated with the native preset visual RaceId; Aasimar and other shared visual bodies use the preset's own identity.");
            Assertions.False(source.Contains(
                    "dollView.CharacterAvatar"),
                "CharacterAvatar is initialized by UnitEntityView.OnDataAttached and must not be required on the unbound DollData view template.");
            Assertions.False(source.Contains(
                    "_actorBlueprint, dollView,"),
                "The configured DollData view must be registered directly; SpawnUnit would clone it and lose Character runtime equipment state.");
            int donorClone = source.IndexOf(
                "_actorBlueprint = UnityEngine.Object.Instantiate(",
                StringComparison.Ordinal);
            int neutralBody = source.IndexOf(
                "CreateNeutralQualificationBody(fixture.Source)",
                StringComparison.Ordinal);
            string zeroClearedItems = "(int)value[" + (char)34 +
                "clearedSlotItemCount" + (char)34 + "] == 0";
            Assertions.True(donorClone >= 0 && neutralBody > donorClone &&
                source.Contains("_clearedSlotItems != 0") &&
                source.Contains(zeroClearedItems),
                "The disposable clone must receive an empty request-local body before spawn, and any later-created slot item must reject the donor.");
            Assertions.False(source.Contains("fixture.Source.Body =") ||
                source.Contains("AmiriLevel20_Companion") ||
                source.Contains("ca08eabf5f6a33e4ba366e889e4fecdc"),
                "Neutral fixture construction must not mutate a native source blueprint or hardcode the visually contaminated donor.");
            foreach (string id in new[]
            {
                "6df8f61725a84294c8661bb9585eca97",
                "4c59d2b9740930145a27a4c693217d22",
                "beba0e0c7dcd5c64d97d767be3e72995",
                "a93ead19aae8afc4794c54f5bcf73168"
            })
                Assertions.True(RenderSource().Contains(id),
                    "The exact audited finalist asset is absent: " + id);
            foreach (string forbidden in new[]
            {
                "SaveGame", "QuickSave", "ScreenCapture",
                "Input.", "Mouse.", "PlayerPrefs",
                "Game.Instance.Player.Inventory"
            })
                Assertions.False(source.Contains(forbidden),
                    "Finalist race matrix contains a forbidden save/UI/global-inventory token: " +
                    forbidden);
        }

        private static string RenderSource()
        {
            return Read("src", "KingmakerGunslinger", "RuntimeTesting",
                "GunslingerOutfitRenderScenario.cs");
        }

        private static string QualificationSource()
        {
            return Read("src", "KingmakerGunslinger", "RuntimeTesting",
                "GunslingerOutfitQualificationScenario.cs");
        }

        private static string WorkingSavePredicate(string request)
        {
            int start = request.IndexOf("bool workingSmoke",
                StringComparison.Ordinal);
            int end = request.IndexOf("bool workingEntryObservation",
                start, StringComparison.Ordinal);
            Assertions.True(start >= 0 && end > start,
                "Working-save request predicate boundaries are absent.");
            return request.Substring(start, end - start);
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
