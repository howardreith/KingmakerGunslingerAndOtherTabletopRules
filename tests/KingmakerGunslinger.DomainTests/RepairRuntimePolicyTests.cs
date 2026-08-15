using System;
using System.IO;

namespace KingmakerGunslinger.DomainTests
{
    internal static class RepairRuntimePolicyTests
    {
        internal static void ImmutableArtifactReuseIsFailClosed()
        {
            string common = Read("scripts", "RuntimeHarness.Common.ps1");
            string invoke = Read("scripts", "Invoke-KingmakerRuntimeTest.ps1");
            string build = Read("scripts", "Build-Local.ps1");
            string deploy = Read("scripts", "Deploy-Local.ps1");
            foreach (string identity in new[] { "commit", "version",
                "packageSha256", "dllSha256", "dllMvid",
                "deployedDllSha256", "firearmBundleSha256",
                "featureModuleSettingsSha256" })
                Assertions.True(deploy.Contains(identity),
                    "Deployment manifest omits repair-policy identity " + identity + ".");
            Assertions.True(build.Contains("dllMvid = Get-KmgDllMvid") &&
                common.Contains("function Assert-KmgReusableDeployment") &&
                common.Contains("Reusable runtime execution requires an exactly clean Git state") &&
                common.Contains("Get-KmgDllMvid -Path $dll") &&
                common.Contains("Get-KmgSha256 -Path $bundle") &&
                common.Contains("SettingsSha256 = $settingsHash"),
                "Reusable artifact validation does not fail closed on the exact immutable identity.");
            Assertions.True(invoke.Contains("[switch]$ReuseInstalledArtifact") &&
                invoke.Contains("Assert-KmgReusableDeployment") &&
                invoke.Contains("if ($ReuseInstalledArtifact) {") &&
                invoke.Contains("} else {") &&
                invoke.Contains("Build-Local.ps1") &&
                invoke.Contains("Deploy-Local.ps1"),
                "Runtime launcher lacks an explicit verified reuse/build boundary.");
        }

        internal static void BoundaryMatrixIsExactlyFourteenStates()
        {
            string matrix = Read("scripts", "Invoke-FeatureModuleRuntimeMatrix.ps1");
            Assertions.True(matrix.Contains("[switch]$Boundary14") &&
                matrix.Contains("$enabled -eq 0 -or $enabled -eq 1 -or $enabled -eq 5 -or") &&
                matrix.Contains("$enabled -eq 6") &&
                matrix.Contains("$boundary.Count -ne 14") &&
                matrix.Contains("$invokeArguments.ReuseInstalledArtifact = $true") &&
                matrix.Contains("$invokeArguments.DeploymentManifestPath") &&
                matrix.Contains("$invokeArguments.PackagePath"),
                "The repair matrix is not the exact all-on/all-off/one-on/one-off 14-state boundary with artifact reuse.");
            string compatibility = Read("scripts", "compatibility",
                "Invoke-KingmakerCompatibilityProfile.ps1");
            Assertions.True(compatibility.Contains("ReuseInstalledArtifact") &&
                compatibility.Contains("DeploymentManifestPath") &&
                compatibility.Contains("PackagePath"),
                "Optional-mod profiles cannot reuse the exact installed artifact.");
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
