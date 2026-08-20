using System;
using System.IO;

namespace KingmakerGunslinger.DomainTests
{
    internal static class WeaponPresentationMissionTests
    {
        internal static void EvidenceScenarioIsGuardedAndStateLabelled()
        {
            string scenario = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "WeaponPresentationEvidenceScenario.cs");
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string request = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRequest.cs");
            string automation = Read("scripts", "RuntimeAutomation.Common.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");

            const string identity = "weapon-presentation-evidence";
            int workingSaveCompletion = runner.IndexOf(
                "if (_workingSaveSmoke.Complete)", StringComparison.Ordinal);
            int evidenceExecution = runner.IndexOf(
                "WeaponPresentationEvidenceScenario.Begin(",
                StringComparison.Ordinal);
            Assertions.True(catalog.Contains(identity) &&
                runner.Contains("WeaponPresentationEvidenceScenario.Begin(") &&
                runner.Contains("_weaponPresentationEvidence.Poll()") &&
                runner.Contains("if (_weaponPresentationEvidence.Complete)") &&
                workingSaveCompletion >= 0 &&
                evidenceExecution > workingSaveCompletion &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WeaponPresentationEvidence ||") &&
                automation.Contains("'" + identity + "' = [pscustomobject]") &&
                preflight.Contains("'" + identity + "'") &&
                automation.Contains(
                    "PermittedSaveName = 'KMG_AUTOMATION_WORKING'") &&
                automation.Contains(
                    "ReadinessBehavior = 'autonomous-working-save'"),
                "Weapon presentation evidence must be an allowlisted autonomous working-save scenario.");

            foreach (string token in new[] {
                "PistolService", "PistolDuelist", "PistolLastWord",
                "RevolverService", "MusketService", "BlunderbussService",
                "RifleService", "SpearClassic", "SpearThorn", "SpearCrown",
                "WakizashiClassic", "WakizashiPetal", "WakizashiMoon",
                "WakizashiCapstone", "KatanaClassic", "KatanaReed",
                "KatanaRegal", "KatanaCapstone", "NodachiClassic",
                "NodachiCleaver", "NodachiTitan", "NodachiCapstone" })
                Assertions.True(scenario.Contains(token),
                    "Evidence catalog omitted production variant " + token + ".");

            foreach (string token in new[] { "LightCrossbow",
                "HeavyCrossbow", "Longspear", "Scimitar", "BastardSword",
                "Greatsword" })
                Assertions.True(scenario.Contains(
                    "new NativeControlSpec(\"" + token + "\""),
                    "Evidence catalog omitted native control " + token + ".");

            Assertions.True(scenario.Contains("_cases.Length != 28") &&
                scenario.Contains("production.Length != 22") &&
                scenario.Contains("controls.Length != 6") &&
                scenario.Contains("SequenceEqual(ProductionVariants)") &&
                scenario.Contains("BuildNativeControl") &&
                scenario.Contains("ReferenceEquals(item.VisualParameters.Model,") &&
                scenario.Contains("type.VisualParameters.Model") &&
                scenario.Contains("HandsEquipment.UpdateAll()") &&
                scenario.Contains("HandsEquipment.ForceSwitch(false)") &&
                scenario.Contains("HandsEquipment.ForceSwitch(true)") &&
                scenario.Contains("GetWeaponModel(false)") &&
                scenario.Contains("HandsEquipment.InCombat") &&
                scenario.Contains("_fixtureBodyRenderers") &&
                scenario.Contains("empty-handed disposable humanoid") &&
                scenario.Contains("Game.Instance.State.Units.All") &&
                scenario.Contains("Game.Instance.Player.Party") &&
                scenario.Contains("MaximumSettleUpdates") &&
                scenario.Contains("PollMaterialization()") &&
                scenario.Contains("PollRemoval()") &&
                scenario.Contains("_presentationState + \"-default-medium-\"") &&
                scenario.Contains("front-right-three-quarter") &&
                scenario.Contains("aabbOverlapVolume") &&
                scenario.Contains("modelLocalRendererBoundsSize") &&
                scenario.Contains("modelLocalRendererBoundsCenterComponents") &&
                scenario.Contains("modelLocalRendererBoundsSizeComponents") &&
                scenario.Contains("modelLocalMajorAxis") &&
                scenario.Contains("modelLocalMinorAxis") &&
                scenario.Contains("modelLocalBoundsSourceCount") &&
                scenario.Contains("filter.sharedMesh.bounds") &&
                scenario.Contains("skinned.localBounds") &&
                scenario.Contains("weapon-presentation-native-local-geometry-invariant") &&
                scenario.Contains("SameComponents") &&
                scenario.Contains("0.00001") &&
                scenario.Contains("semanticLocators") &&
                scenario.Contains("presentationRole") &&
                scenario.Contains("weapon-presentation-native-donor-controls") &&
                scenario.Contains("no attack, reload, or movement claim") &&
                scenario.Contains("body-centered-capped") &&
                scenario.Contains("SameReferences(_unitsBefore") &&
                scenario.Contains("SameReferences(_partyBefore") &&
                scenario.Contains("File.WriteAllBytes(pngPath, png)"),
                "Evidence must settle exact native stored and held models across game updates, capture four labelled views with outlier-safe framing, retain clipping diagnostics and honest claim limits, and prove exact cleanup.");

            Assertions.False(scenario.Contains("SaveGame") ||
                scenario.Contains("QuickSave") || scenario.Contains("LoadGame") ||
                scenario.Contains("KMG_AUTOMATION_BASELINE") ||
                scenario.Contains("Camera.main.transform.rotation =") ||
                scenario.Contains("actor.View.transform.position ="),
                "The evidence fixture may not save, load, target the protected baseline, or camera-relative-correct a weapon/actor.");
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
