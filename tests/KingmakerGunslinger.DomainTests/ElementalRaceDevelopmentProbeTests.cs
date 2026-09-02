using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalRaceDevelopmentProbeTests
    {
        private const string Symbol =
            "KMG.ElementalRaces.Diagnostics.ProbeRace";
        private const string Guid =
            "57005fca40ab4775ae2fea5613214054";
        private const string Scenario =
            "observe-elemental-race-blueprints";
        private const string VisualScenario =
            "elemental-race-visual-audit";

        internal static void ReservedIdentityIsExactAndUnique()
        {
            JObject manifest = JObject.Parse(Read("blueprints",
                "blueprints.json"));
            JToken[] entries = manifest["entries"].ToArray();
            JToken[] matches = entries.Where(value => string.Equals(
                (string)value["symbol"], Symbol,
                StringComparison.Ordinal)).ToArray();
            Assertions.Equal(1, matches.Length,
                "Development race probe identity is missing or duplicated.");
            Assertions.Equal(Guid, (string)matches[0]["guid"],
                "Development race probe GUID changed.");
            Assertions.Equal("BlueprintRace",
                (string)matches[0]["plannedType"],
                "Development race probe manifest type changed.");
            Assertions.Equal("reserved", (string)matches[0]["status"],
                "Development race probe must never be ordinary bootstrap content.");
            Assertions.Equal(entries.Length, entries.Select(value =>
                (string)value["guid"]).Distinct(StringComparer.Ordinal).Count(),
                "The diagnostic GUID collides inside the project ledger.");

            string catalog = Read("src", "KingmakerGunslinger",
                "ElementalRaces",
                "ElementalRaceDiagnosticIdentityCatalog.cs");
            Assertions.True(catalog.Contains(Symbol) &&
                catalog.Contains(Guid) &&
                catalog.Contains("never published"),
                "Diagnostic identity catalog does not preserve its development-only contract.");

            string legacyValidator = Read("tools",
                "validate_playtest66.py");
            string summoningValidator = Read("tools",
                "validate_summoning78.py");
            Assertions.True(legacyValidator.Contains(
                    "KMG.ElementalRaces.") &&
                legacyValidator.Contains("elemental_races_active_count") &&
                summoningValidator.Contains("elemental_races_reserved") &&
                summoningValidator.Contains("elemental_races_entries"),
                "Version-chain manifest validators do not count Elemental Races identities by status.");
        }

        internal static void GuardedScenarioWiringIsSaveFree()
        {
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            Assertions.True(catalog.Contains(
                    "internal const string ObserveElementalRaceBlueprints") &&
                catalog.Contains(Scenario) &&
                runner.Contains("ResourcesLibrary.Preloading") &&
                runner.Contains(
                    "ElementalRaceDevelopmentProbeScenario.Begin(") &&
                runner.Contains(
                    "_elementalRaceDevelopmentProbe.Poll()") &&
                catalog.Contains(
                    "internal const string ElementalRaceVisualAudit") &&
                catalog.Contains(VisualScenario) &&
                runner.Contains(
                    "ElementalRaceVisualAuditScenario.Begin(") &&
                runner.Contains("_elementalRaceVisualAudit.Poll()") &&
                automation.Contains("'" + Scenario +
                    "' = [pscustomobject]") &&
                automation.Contains("'" + VisualScenario +
                    "' = [pscustomobject]") &&
                preflight.Contains("'" + Scenario + "'") &&
                preflight.Contains("'" + VisualScenario + "'") &&
                project.Contains(
                    "ElementalRaceDevelopmentProbeScenario.cs") &&
                project.Contains("ElementalRaceVisualAuditScenario.cs") &&
                project.Contains(
                    "ElementalRaceDiagnosticIdentityCatalog.cs"),
                "Development race probe is not wired through every guarded runtime surface.");
            foreach (string scenario in new[] { Scenario, VisualScenario })
            {
                int offset = automation.IndexOf("'" + scenario +
                    "' = [pscustomobject]", StringComparison.Ordinal);
                Assertions.True(offset >= 0,
                    scenario + " metadata is absent.");
                string metadata = automation.Substring(offset,
                    Math.Min(500, automation.Length - offset));
                Assertions.True(metadata.Contains(
                    "RequiresSaveName = $false") &&
                    metadata.Contains(
                        "RequiresManualInteraction = $false") &&
                    metadata.Contains("ReadinessBehavior = 'mod-load'") &&
                    metadata.Contains("UsesCatalogTimeout = $false") &&
                    metadata.Contains("UsesSelectionTimeouts = $false"),
                    scenario +
                    " must remain autonomous, save-free, and selector-free.");
            }
        }

        internal static void ProbeIsAtomicNativeAndOutfitSafe()
        {
            string source = Read("src", "KingmakerGunslinger",
                "RuntimeTesting",
                "ElementalRaceDevelopmentProbeScenario.cs");
            foreach (string token in new[]
            {
                "0a5d473ead98b0646b94495af250fdc4",
                "b7f02ba92b363064fb873963bec275ee",
                "5c4e42124dc2b4647af6e36cf2590500",
                "25a5878d125338244896ebd3238226c8",
                "c4faf439f0e70bd40b5e36ee80d06be7",
                "b3646842ffbd01643ab4dac7479b20b0",
                "1dc20e195581a804890ddc74218bfd8e",
                "ef35a22c9a27da345a4528f0d5889157",
                "9c747d24f6321f744aa1bb4bd343880d",
                "786588ad1694e61498e77321d4b07157",
                "9054d3988d491d944ac144e27b6bc318",
                "4783c3709a74a794dbe7c8e7e0b1b038",
                "85067a04a97416949b5d1dbf986d93f3",
                "f3c0b267dd17a2a45a40805e31fe3cd1",
                "c7104f7526c4c524f91474614054547e",
                "c60969e7f264e6d4b84a1499fdcf9039",
                "4e0e9aba6447d514f88eff1464cc4763",
                "AuditMechanicDonors(_library, native, _evidence",
                "AuditVisualDonors(native, _evidence, _assertions)",
                "mechanicDonors", "visualDonors", "visualDonorTotals",
                "EquipmentEntityLink.Load(false)",
                "preset.Skin.GetLinks(Gender.Male, race.RaceId)",
                "entity.PrimaryRamps", "entity.SecondaryRamps",
                "texture.width", "texture.height",
                "texture.format.ToString()", "texture.isReadable",
                "texture.GetPixel(x, y)",
                "entity.BodyParts", "entity.OutfitParts",
                "outsiderPrecedent",
                "aasimar.AllFeatures", "tiefling.AllFeatures",
                "BlueprintCloneService.Clone(human, ProbeName)",
                "SetMember(_probe, ", "SelectableRaceStat",
                "new ProbeRegistration(_library, _probe",
                "library.BlueprintsByAssetId.Add(guid, blueprint)",
                "_all.Add(blueprint)",
                "_library.BlueprintsByAssetId.Remove(_guid)",
                "_all.Remove(_blueprint)",
                "ReferenceEquals(_root.Progression.CharacterRaces,",
                "new DollState()", "state.CreateData()",
                "data.CreateUnitView(false)",
                "MaximumViewSettleUpdates", "ViewReady(_maleView)",
                "RendererHasCompleteMaterial",
                "_maleState.CharacterClass",
                "_femaleState.CharacterClass",
                "_levelController.SelectRace(_probe)",
                "GetRank(_diagnostic)", "DefaultJsonSettings",
                "_levelController.Cancel()",
                "GunslingerClassAppearanceCatalog.MaleAssetIds()",
                "GunslingerClassAppearanceCatalog.FemaleAssetIds()",
                "UnityEngine.Object.DestroyImmediate(_probe)",
                "saveStateTouched", "publishedToCharacterRaces"
            })
                Assertions.True(source.Contains(token),
                    "Development race probe lacks safety/native token: " +
                    token);

            foreach (string forbidden in new[]
            {
                "Guid.NewGuid", "SaveGame",
                "QuickSave", "PlayerPrefs", "Input.", "Mouse.",
                "ScriptableObject.CreateInstance<BlueprintRace>"
            })
                Assertions.False(source.Contains(forbidden),
                    "Development race probe contains forbidden persistent/UI mutation: " +
                    forbidden);
            Assertions.False(Regex.IsMatch(source,
                    @"\.CharacterRaces\s*=(?!=)"),
                "Development race probe assigns the shared CharacterRaces array.");

            string visual = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "ElementalRaceVisualAuditScenario.cs");
            foreach (string token in new[]
            {
                "MinimumCasesPerRaceAndSex = 7",
                "ElementalRaceVisualCatalog.ResourceIdentityCount",
                "ElementalRaceVisualCatalog.SkinRampCount",
                "BuildCases(races[index], visuals[index])",
                "state.SetRace(renderCase.Race)",
                "state.SetRacePreset(renderCase.Preset)",
                "state.SetClass(_gunslinger)",
                "state.SetHead(renderCase.Head)",
                "state.SetHair(renderCase.Hair)",
                "EyebrowsProperty.SetValue",
                "state.SetBeard(renderCase.Beard)",
                "state.SetHorn(renderCase.Horn)",
                "state.SetSkinColor(renderCase.SkinIndex)",
                "state.SetHairColor(renderCase.HairColorIndex)",
                "state.CreateData()",
                "ElementalRaceDevelopmentProbeScenario.CreateView",
                "ElementalRaceDevelopmentProbeScenario.ViewReady",
                ".DescribeView(renderCase.Label",
                "ElementalRaceDevelopmentProbeScenario.DestroyView",
                "nullMaterials", "nullShaders", "RequiredEntityIds",
                "ReferenceEquals(_root.Progression.CharacterRaces,",
                "saveStateTouched=false;selectorStateTouched=false"
            })
                Assertions.True(visual.Contains(token),
                    "Production visual audit lacks exact token: " + token);
            foreach (string forbidden in new[]
            {
                "Guid.NewGuid", "SaveGame", "QuickSave", "PlayerPrefs",
                "Input.", "Mouse."
            })
                Assertions.False(visual.Contains(forbidden),
                    "Production visual audit contains forbidden mutation/UI token: " +
                    forbidden);
            Assertions.False(Regex.IsMatch(visual,
                    @"\.CharacterRaces\s*=(?!=)"),
                "Production visual audit assigns the shared CharacterRaces array.");

            string appearance = Read("src", "KingmakerGunslinger",
                "Presentation", "GunslingerClassAppearanceCatalog.cs");
            foreach (string assetId in new[]
            {
                "6df8f61725a84294c8661bb9585eca97",
                "4c59d2b9740930145a27a4c693217d22",
                "beba0e0c7dcd5c64d97d767be3e72995",
                "a93ead19aae8afc4794c54f5bcf73168"
            })
                Assertions.True(appearance.Contains(assetId),
                    "Accepted Gunslinger outfit identity changed: " +
                    assetId);
            Assertions.True(appearance.Contains(
                    "DefaultPrimaryColor = 2") &&
                appearance.Contains("DefaultSecondaryColor = 22"),
                "Accepted Gunslinger outfit colors changed.");
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
