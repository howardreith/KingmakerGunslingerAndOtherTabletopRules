using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace KingmakerGunslinger.DomainTests
{
    // The production DLL the focused gate has just compiled (Invoke-KmgGate passes its path and SHA-256 in
    // KMG_PRODUCTION_DLL / KMG_PRODUCTION_DLL_SHA256) must reference exactly the pinned loader assemblies of
    // scripts\loader-reference-pins.json with their full binding identity, and no other UnityModManager or Harmony
    // assembly. Without that binding the check refuses: a DLL left by an earlier build is never inspected.
    // (Build-Local checks the packaged DLL's references itself, after its exact build, in validate-package.ps1.)
    internal static class LoaderReferenceContractTests
    {
        internal static void ProductionDllReferencesExactlyThePinnedLoader()
        {
            string root = RepositoryRoot();
            string dll = Environment.GetEnvironmentVariable("KMG_PRODUCTION_DLL");
            string expectedSha = Environment.GetEnvironmentVariable("KMG_PRODUCTION_DLL_SHA256");
            Assertions.True(!string.IsNullOrEmpty(dll) && !string.IsNullOrEmpty(expectedSha),
                "This check inspects the production DLL compiled by the focused gate (Invoke-KmgGate.ps1 sets KMG_PRODUCTION_DLL and KMG_PRODUCTION_DLL_SHA256).");
            Assertions.True(File.Exists(dll), "The gate's production DLL is missing: " + dll);
            byte[] bytes = File.ReadAllBytes(dll);
            string actualSha;
            using (SHA256 sha = SHA256.Create()) actualSha = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
            Assertions.True(string.Equals(actualSha, expectedSha, StringComparison.Ordinal), "The production DLL is not the one the gate compiled: " + actualSha + " vs " + expectedSha);
            string pins = File.ReadAllText(Path.Combine(root, "scripts", "loader-reference-pins.json"));
            string expectedUmm = "UnityModManager, Version=" + PinVersion(pins, "UnityModManager/UnityModManager.dll") + ", Culture=neutral, PublicKeyToken=null";
            string expectedHarmony = "0Harmony12, Version=" + PinVersion(pins, "UnityModManager/0Harmony12.dll") + ", Culture=neutral, PublicKeyToken=null";
            Assembly assembly = Assembly.ReflectionOnlyLoad(bytes);
            string[] loaderReferences = assembly.GetReferencedAssemblies()
                .Where(r => Regex.IsMatch(r.Name, "^(unitymodmanager|0?harmony)", RegexOptions.IgnoreCase))
                .Select(r => r.FullName).OrderBy(n => n, StringComparer.Ordinal).ToArray();
            string[] expected = new[] { expectedHarmony, expectedUmm }.OrderBy(n => n, StringComparer.Ordinal).ToArray();
            Assertions.True(loaderReferences.SequenceEqual(expected),
                "The production DLL's loader references are [" + string.Join("; ", loaderReferences) + "], not the pinned [" + string.Join("; ", expected) + "].");
        }

        private static string PinVersion(string pins, string relativePath)
        {
            Match match = Regex.Match(pins, "\"relativePath\":\\s*\"" + Regex.Escape(relativePath) + "\"[^}]*?\"assemblyVersion\":\\s*\"([0-9.]+)\"");
            Assertions.True(match.Success, "The pins do not define " + relativePath + ".");
            return match.Groups[1].Value;
        }

        private static string RepositoryRoot()
        {
            DirectoryInfo cursor = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (cursor != null)
            {
                if (File.Exists(Path.Combine(cursor.FullName, "KingmakerGunslinger.sln")) &&
                    File.Exists(Path.Combine(cursor.FullName, "scripts", "loader-reference-pins.json")))
                    return cursor.FullName;
                cursor = cursor.Parent;
            }
            throw new InvalidOperationException("Repository root was not found.");
        }
    }
}
