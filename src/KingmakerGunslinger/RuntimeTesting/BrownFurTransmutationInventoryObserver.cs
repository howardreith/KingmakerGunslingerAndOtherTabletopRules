using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.BrownFur;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class BrownFurTransmutationInventoryObserver
    {
        private const string FileName =
            "brown-fur-transmutation-spell-inventory.json";

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            CotwArcanistResolution resolution =
                BrownFurOptionalExtensionCoordinator.Current;
            BrownFurSpellInventoryEvidence inventory = resolution == null ||
                !resolution.Decision.IsCompatible ? null :
                BrownFurTransmutationInventory.Observe(resolution.Contract);
            int expectedRoots = resolution == null || resolution.Contract == null ||
                resolution.Contract.Fingerprint == null ? 0 :
                resolution.Contract.Fingerprint.TransmutationSpellCount;
            Add(assertions, "inventory-contract-compatible", "compatible",
                resolution == null ? "missing" :
                    resolution.Decision.Availability.ToString(),
                resolution != null && resolution.Decision.IsCompatible,
                "isolated CotW runtime contract");
            Add(assertions, "inventory-complete-root-set",
                expectedRoots.ToString(), inventory == null ? "missing" :
                    inventory.RootSpellCount.ToString(),
                inventory != null && expectedRoots > 0 &&
                    inventory.RootSpellCount == expectedRoots,
                "all genuine Transmutation spells in the resolved Arcanist spell list");
            bool singular = inventory != null && inventory.Records != null &&
                inventory.Records.Count == inventory.RecordCountIncludingVariants &&
                inventory.Records.Select(value => value.CanonicalSpellGuid)
                    .Distinct(StringComparer.Ordinal).Count() == inventory.Records.Count;
            Add(assertions, "inventory-variant-identities-singular",
                "one record per root or nested variant GUID",
                inventory == null ? "missing" : "roots=" + inventory.RootSpellCount +
                    ";records=" + inventory.RecordCountIncludingVariants,
                singular, "recursive AbilityVariants graph with ambiguous-parent rejection");
            bool fields = inventory != null && inventory.Records.All(value =>
                !string.IsNullOrWhiteSpace(value.CanonicalSpellGuid) &&
                !string.IsNullOrWhiteSpace(value.SpellbookSourceGuid) &&
                !string.IsNullOrWhiteSpace(value.Range) &&
                value.NestedActionGraph != null && value.AppliedBuffs != null &&
                value.AbilityScoreBonuses != null && value.ModifierDescriptors != null &&
                value.ValuePatterns != null && value.PolymorphAndSizeComponents != null &&
                value.HardCodedToCaster != null &&
                value.QualificationStatus == "Unexplained");
            Add(assertions, "inventory-required-fields", "complete investigation fields",
                fields ? "complete" : "missing-or-preclassified", fields,
                "runtime blueprint/component/action/buff graph reflection");
            Add(assertions, "inventory-publication-gate", "all entries remain Unexplained",
                inventory == null || inventory.QualificationCounts == null ? "missing" :
                    string.Join(",", inventory.QualificationCounts.Select(value =>
                        value.Key + "=" + value.Value).ToArray()),
                inventory != null && inventory.QualificationCounts.Count == 1 &&
                    inventory.QualificationCounts.ContainsKey("Unexplained") &&
                    inventory.QualificationCounts["Unexplained"] ==
                        inventory.RecordCountIncludingVariants,
                "investigation observer cannot authorize player-facing publication");
            Add(assertions, "save-free-observer", "no save or input API invoked",
                "read-only blueprint graph inventory", true,
                "observer does not select, load, mutate, or save a character");

            if (inventory != null)
            {
                string path = Path.Combine(request.EvidenceDirectory, FileName);
                File.WriteAllText(path, JsonConvert.SerializeObject(inventory,
                    Formatting.Indented));
                evidenceFiles.Add(path);
                diagnostics.Add("roots=" + inventory.RootSpellCount +
                    ";records=" + inventory.RecordCountIncludingVariants +
                    ";personal=" + inventory.PersonalSpellCount +
                    ";bonusCandidates=" + inventory.AbilityBonusCandidateCount +
                    ";toCaster=" + inventory.HardCodedToCasterCount +
                    ";sha256=" + Hash(path));
            }
            bool pass = assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult
            {
                SchemaVersion = 1, RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" +
                    assembly.ManifestModule.ModuleVersionId + ";sha256=" +
                    Hash(assembly.Location) + ";pid=" + Process.GetCurrentProcess().Id,
                GitCommit = Metadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = DateTime.UtcNow.ToString("o"), EndUtc = string.Empty,
                Assertions = assertions, Diagnostics = diagnostics,
                Warnings = new List<string>(), ExceptionSummary = string.Empty,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static void Add(List<RuntimeTestAssertion> assertions,
            string name, string expected, string observed, bool pass,
            string evidence)
        {
            assertions.Add(new RuntimeTestAssertion { Name = name,
                Expected = expected, Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = evidence });
        }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }

        private static string Metadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>().FirstOrDefault(item =>
                    item.Key == key);
            return value == null ? string.Empty : value.Value;
        }
    }
}
