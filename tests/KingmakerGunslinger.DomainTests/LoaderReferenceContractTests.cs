using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace KingmakerGunslinger.DomainTests
{
    // A verified production compiler output (test-domain.ps1 binds the DLL that build.ps1 or the focused gate has just
    // compiled, or compiles one itself, by path and SHA-256 in KMG_PRODUCTION_DLL / KMG_PRODUCTION_DLL_SHA256) must
    // reference exactly the pinned loader identities that the pin reader provides in KMG_EXPECTED_LOADER_REFERENCES, and
    // no other UnityModManager or Harmony assembly. Without that binding the check refuses.
    internal static class LoaderReferenceContractTests
    {
        // The case runs for real wherever a production compiler output is bound (test-domain.ps1 always binds one when the
        // case is selected and not deferred: the DLL build.ps1, the focused gate or Build-Local just produced, or one it
        // compiles itself). An unfiltered runner started without a binding (the portable net8 runner, a direct runner
        // call) has no artifact to inspect: the case reports NOT APPLICABLE there. A focused selection without a binding
        // fails, and a Build-Local deferral reports NOT APPLICABLE until Build-Local runs it on its exact output.
        internal static void ProductionDllReferencesExactlyThePinnedLoader()
        {
            string dll = Environment.GetEnvironmentVariable("KMG_PRODUCTION_DLL");
            string expectedSha = Environment.GetEnvironmentVariable("KMG_PRODUCTION_DLL_SHA256");
            bool bound = !string.IsNullOrEmpty(dll) && !string.IsNullOrEmpty(expectedSha);
            bool deferred = string.Equals(Environment.GetEnvironmentVariable("KMG_LOADER_CASE_DEFERRED"), "1", StringComparison.Ordinal);
            if (!bound)
            {
                if (deferred) throw new NotApplicableException("deferred: Build-Local runs this case on its exact compiler output.");
                // Any focused selection (KMG_TEST_FILTER substring or --test-prefix) that chose this case requires the artifact.
                Assertions.True(!RunSelection.Focused,
                    "This check inspects a verified production compiler output and was selected without one (test-domain.ps1 sets KMG_PRODUCTION_DLL and KMG_PRODUCTION_DLL_SHA256).");
                throw new NotApplicableException("no production compiler output is bound to this unfiltered run (test-domain.ps1 binds one).");
            }
#if NET5_0_OR_GREATER
            Assertions.True(false, "The production DLL assertion runs on the .NET Framework domain runner (scripts/test-domain.ps1).");
#else
            string expectedList = Environment.GetEnvironmentVariable("KMG_EXPECTED_LOADER_REFERENCES");
            Assertions.True(!string.IsNullOrEmpty(expectedList), "The expected loader identities were not provided (KMG_EXPECTED_LOADER_REFERENCES).");
            Assertions.True(File.Exists(dll), "The gate's production DLL is missing: " + dll);
            byte[] bytes = File.ReadAllBytes(dll);
            string actualSha;
            using (SHA256 sha = SHA256.Create()) actualSha = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
            Assertions.True(string.Equals(actualSha, expectedSha, StringComparison.Ordinal), "The production DLL is not the bound compiler output: " + actualSha + " vs " + expectedSha);
            Assembly assembly = Assembly.ReflectionOnlyLoad(bytes);
            string[] loaderReferences = assembly.GetReferencedAssemblies()
                .Where(r => Regex.IsMatch(r.Name, "^(unitymodmanager|0?harmony)", RegexOptions.IgnoreCase))
                .Select(r => r.FullName).OrderBy(n => n, StringComparer.Ordinal).ToArray();
            string[] expected = expectedList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).OrderBy(n => n, StringComparer.Ordinal).ToArray();
            Assertions.True(loaderReferences.SequenceEqual(expected),
                "The production DLL's loader references are [" + string.Join("; ", loaderReferences) + "], not the pinned [" + string.Join("; ", expected) + "].");
#endif
        }
    }
}
