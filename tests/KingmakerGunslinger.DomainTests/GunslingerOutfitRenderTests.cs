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
        }

        private static string RenderSource()
        {
            return Read("src", "KingmakerGunslinger", "RuntimeTesting",
                "GunslingerOutfitRenderScenario.cs");
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
