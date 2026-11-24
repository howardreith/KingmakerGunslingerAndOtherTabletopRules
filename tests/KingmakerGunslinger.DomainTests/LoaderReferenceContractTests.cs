using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace KingmakerGunslinger.DomainTests
{
    // The production DLL compiled by this checkout (scripts\build.ps1 / Invoke-KmgGate production compilation) must
    // reference exactly the pinned loader assemblies of scripts\loader-reference-pins.json with their full binding
    // identity, and no other UnityModManager or Harmony assembly.
    internal static class LoaderReferenceContractTests
    {
        internal static void ProductionDllReferencesExactlyThePinnedLoader()
        {
            string root = RepositoryRoot();
            string dll = Path.Combine(root, "artifacts", "bin", "Release", "KingmakerGunslinger", "KingmakerGunslinger.dll");
            Assertions.True(File.Exists(dll), "The Release production DLL was not compiled before the domain tests: " + dll);
            string pins = File.ReadAllText(Path.Combine(root, "scripts", "loader-reference-pins.json"));
            string expectedUmm = "UnityModManager, Version=" + PinVersion(pins, "UnityModManager/UnityModManager.dll") + ", Culture=neutral, PublicKeyToken=null";
            string expectedHarmony = "0Harmony12, Version=" + PinVersion(pins, "UnityModManager/0Harmony12.dll") + ", Culture=neutral, PublicKeyToken=null";
            Assembly assembly = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(dll));
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
