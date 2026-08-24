using System;
using System.IO;
using KingmakerGunslinger.Compatibility;

namespace KingmakerGunslinger.DomainTests
{
    internal static class CompatibilityAttributionTests
    {
        internal static void AssetPlansAreExact()
        {
            AssertPlan(CompatibilityAssetAttributionPlan.AllSuppressed,
                false, false, false);
            AssertPlan(CompatibilityAssetAttributionPlan.FirearmsOnly,
                true, false, false);
            AssertPlan(CompatibilityAssetAttributionPlan.SpearsOnly,
                false, true, false);
            AssertPlan(CompatibilityAssetAttributionPlan.EasternOnly,
                false, false, true);
            AssertPlan(CompatibilityAssetAttributionPlan.AllEnabled,
                true, true, true);
        }

        internal static void AssetPlansFailClosed()
        {
            CompatibilityAssetAttributionPlan plan;
            foreach (string value in new[] { null, string.Empty, "ALL-ENABLED",
                "firearms-and-spears", "all-enabled " })
                Assertions.False(CompatibilityAssetAttributionPlan.TryResolve(
                    value, out plan) || plan != null,
                    "Unknown asset configuration did not fail closed: " +
                    (value ?? "<null>"));
            AssertPlan(CompatibilityAssetAttributionPlan.AllSuppressed,
                false, false, false);
        }

        internal static void GuardedRuntimeBoundaryIsExact()
        {
            string main = Read("src", "KingmakerGunslinger", "Main.cs");
            string control = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "CompatibilityAttributionRuntimeControl.cs");
            string request = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRequest.cs");
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string automation = Read("scripts", "RuntimeAutomation.Common.ps1");
            string harness = Read("scripts", "RuntimeHarness.Common.ps1");
            string build = Read("scripts", "Build-Local.ps1");
            const string scenario =
                "observe-kmg-compatibility-asset-attribution";
            int identity = main.IndexOf(
                "RuntimeTestRunner.RecordEarlyIdentity(context)",
                StringComparison.Ordinal);
            int activation = main.IndexOf(
                "CompatibilityAttributionRuntimeControl.TryActivateEarly(context)",
                StringComparison.Ordinal);
            int firearmLoad = main.IndexOf(
                "Assets.FirearmAssetRuntime.Configure(context)",
                StringComparison.Ordinal);
            Assertions.True(identity >= 0 && activation > identity &&
                firearmLoad > activation,
                "Guarded control must activate after identity evidence and before any KMG bundle load.");
            foreach (string token in new[] {
                "RuntimeTestRequestParser", ".TryActivate(",
                "decision.Accepted", "decision.Request.Scenario",
                "ObserveKmgCompatibilityAssetAttribution",
                "processLocal=true;saveState=false" })
                Assertions.True(control.Contains(token),
                    "Early attribution control lacks gate token: " + token);
            Assertions.True(request.Contains("request.Parameters.Count != 1") &&
                request.Contains("assetConfiguration") &&
                request.Contains("asset-configuration-not-allowed") &&
                catalog.Contains(scenario) &&
                automation.Contains("'" + scenario + "' = [pscustomobject]"),
                "C# and PowerShell request boundaries do not share the exact guarded scenario contract.");
            Assertions.True(harness.Contains(
                    "function Get-KmgSourceStateFingerprint") &&
                harness.Contains("$build.sourceStateSha256 -cne $sourceStateSha256") &&
                harness.Contains("$git.Status.Count -ne 0 -and -not $AllowDirtyGit") &&
                build.Contains("sourceStateSha256 = $sourceStateSha256"),
                "Dirty artifact reuse must be explicit and bound to the exact attested source state.");
            Assertions.False(control.Contains("SaveGame") ||
                control.Contains("QuickSave") || control.Contains("PlayerPrefs") ||
                control.Contains("FeatureModules.json") ||
                control.Contains("Harmony"),
                "Attribution control may not write save/settings state or patch behavior.");
        }

        internal static void AssetInventoryAndLogCollectionAreBounded()
        {
            string scenario = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "CompatibilityAssetAttributionScenario.cs");
            string collector = Read("scripts", "compatibility",
                "Collect-KmgCompatibilityAttributionLog.ps1");
            foreach (string token in new[] {
                "GetAllAssetNames()", "bundle.LoadAsset(assetPath)",
                "UnityEngine.Object.Instantiate(prefab)",
                "UnityEngine.Object.DestroyImmediate(instance)",
                "\"UnityEngine.ParticleSystem\"",
                "\"UnityEngine.ParticleSystemRenderer\"",
                "GetComponentsInChildren<Camera>",
                "GetComponentsInChildren<Light>",
                "material.HasProperty(\"_MainTex\")",
                "shader.isSupported", "mesh.isReadable",
                "renderer.lightmapIndex", "MissingSerializedComponents",
                "no-save-owned-state" })
                Assertions.True(scenario.Contains(token),
                    "Asset inventory lacks bounded ownership token: " + token);
            Assertions.False(scenario.Contains("Application.logMessageReceived") ||
                scenario.Contains("Harmony") || scenario.Contains("SaveGame") ||
                scenario.Contains("QuickSave") ||
                scenario.Contains("UnitFxVisibilityManager"),
                "Asset inventory may not install broad log/view patches or use save APIs.");
            foreach (string token in new[] {
                "unsupportedShaderAllPassesRemoved",
                "invalidParticleMeshReadWrite", "missingSerializedScript",
                "lightmapModeMismatch", "zeroSurfaceArea",
                "missingMainTexProperty",
                "favoredClassComponentAppliedOnceOnLevelUp",
                "polymorphTransition", "polymorphTryReplaceView",
                "polymorphRestoreView", "unitFxVisibilityManagerUpdate",
                "FeatureModules.json", "installedMods", "kmgBundles" })
                Assertions.True(collector.Contains(token),
                    "Log collector lacks required fingerprint: " + token);
            Assertions.True(collector.Contains(
                    "Kingmaker must have exited before the attribution log is collected") &&
                collector.Contains("runtime-evidence") &&
                collector.Contains("[IO.File]::Copy") &&
                collector.Contains("Get-FileHash"),
                "Log collector must retain an exact, hashed, post-exit source log under runtime evidence.");
        }

        private static void AssertPlan(string configuration, bool firearms,
            bool spears, bool eastern)
        {
            CompatibilityAssetAttributionPlan plan;
            Assertions.True(CompatibilityAssetAttributionPlan.TryResolve(
                    configuration, out plan) && plan != null,
                "Exact asset plan did not resolve: " + configuration);
            Assertions.True(plan.Configuration == configuration &&
                plan.IsEnabled(CompatibilityAssetFamily.Firearms) == firearms &&
                plan.IsEnabled(CompatibilityAssetFamily.ElvenBranchedSpears) ==
                    spears &&
                plan.IsEnabled(CompatibilityAssetFamily.EasternWeapons) == eastern,
                "Asset plan state changed: " + configuration);
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
