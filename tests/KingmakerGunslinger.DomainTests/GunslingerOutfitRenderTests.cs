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

            Assertions.Equal(16, Regex.Matches(source,
                "new ProductionCompatibilityCase\\(").Count,
                "The production compatibility matrix must contain exactly sixteen states.");
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
                "SpawnEntityWithView(dollView,",
                "CreateProductionNeutralBody", "Body.AllSlots",
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
            foreach (string forbidden in new[]
            {
                "SaveGame", "QuickSave", "ScreenCapture", "Input.",
                "Mouse.", "PlayerPrefs", "Game.Instance.Player.Inventory"
            })
                Assertions.False(source.Contains(forbidden),
                    "Production compatibility contains a forbidden save/UI/global-inventory token: " +
                    forbidden);
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
