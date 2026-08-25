using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.CraftMagicItemsCompatibility;
using UnityEngine;
using UnityModManagerNet;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Guarded, save-free qualification of the exact live Craft Magic Items
    /// assembly and the KMG-owned optional registration bridge.
    /// </summary>
    internal static class CraftMagicItemsCompatibilityObserver
    {
        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (request == null) throw new ArgumentNullException("request");
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var warnings = new List<string>();

            UnityModManager.ModEntry entry =
                CraftMagicItemsOptionalExtensionCoordinator.Entry;
            CraftMagicItemsContract contract =
                CraftMagicItemsOptionalExtensionCoordinator.Contract;
            Assembly cmiAssembly = entry == null ? null : entry.Assembly;
            bool exactEntry = entry != null && entry.Info != null &&
                string.Equals(entry.Info.Id,
                    CraftMagicItemsContractProbe.ModId,
                    StringComparison.Ordinal) && entry.Loaded && entry.Active &&
                entry.HasAssembly && !entry.ErrorOnLoading &&
                cmiAssembly != null && contract != null &&
                ReferenceEquals(contract.Assembly, cmiAssembly) &&
                string.Equals(contract.MainType.FullName,
                    CraftMagicItemsContractProbe.MainTypeName,
                    StringComparison.Ordinal);
            Add(assertions, "exact-live-cmi-entry",
                "one active CraftMagicItems UMM entry with CraftMagicItems.Main contract",
                DescribeEntry(entry), exactEntry,
                "live UMM ModEntry plus capability-probed adapter contract");

            string cmiHash = cmiAssembly == null ? "missing" :
                HashFile(cmiAssembly.Location);
            string cmiMvid = cmiAssembly == null ? "missing" :
                cmiAssembly.ManifestModule.ModuleVersionId.ToString();
            string cmiFileVersion = cmiAssembly == null ||
                string.IsNullOrWhiteSpace(cmiAssembly.Location) ? "missing" :
                FileVersionInfo.GetVersionInfo(cmiAssembly.Location)
                    .FileVersion;
            diagnostics.Add("cmiAssembly=" + (cmiAssembly == null ?
                "missing" : cmiAssembly.FullName) + ";fileVersion=" +
                cmiFileVersion + ";mvid=" + cmiMvid + ";sha256=" + cmiHash);

            CraftMagicItemsQualificationResult qualification =
                CraftMagicItemsReflectionBridge.RunGuardedQualification();
            foreach (CraftMagicItemsQualificationCheck check in
                qualification.Checks)
                assertions.Add(new RuntimeTestAssertion
                {
                    Name = check.Name,
                    Expected = check.Expected,
                    Observed = check.Observed,
                    Status = check.Passed ? RuntimeTestStatuses.Pass :
                        RuntimeTestStatuses.Fail,
                    Evidence = check.Evidence
                });
            diagnostics.AddRange(qualification.Diagnostics);
            CraftMagicItemsTooltipInspectionResult tooltipInspection =
                CraftMagicItemsTooltipInspection.Capture(true);
            Add(assertions, "internal-tooltip-markers-hidden",
                "exact state/origin markers remain mechanical while native tooltip text has no <null> block and retains real qualities",
                "passed=" + tooltipInspection.Passed,
                tooltipInspection.Passed,
                "real CMI clone, item-owned KMG markers, and native UIUtilityItem tooltip builder");
            diagnostics.AddRange(tooltipInspection.Diagnostics);
            Add(assertions, "save-free-disposable-boundary",
                "no save, inventory, party, input, or campaign mutation",
                "request-local item entities and CMI custom blueprints removed by the qualified full rebuild",
                qualification.RebuiltGeneration >
                    qualification.InitialGeneration,
                "observer creates no inventory entries and invokes no save/input API");
            warnings.Add("CMI-crafted custom items use CMI's ordinary custom-blueprint GUID persistence and require both mods to remain installed.");
            warnings.Add("Visual placement and end-to-end CMI UMM interaction remain human acceptance items; this observer supplies mechanical graph evidence only.");

            bool pass = exactEntry && qualification.Passed &&
                assertions.All(value => value.Status ==
                    RuntimeTestStatuses.Pass);
            RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                context.Assembly, context.ModEntry.Info.Version);
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = context.Assembly.FullName + ";mvid=" +
                    context.Assembly.ManifestModule.ModuleVersionId +
                    ";sha256=" + HashFile(context.Assembly.Location) +
                    ";pid=" + Process.GetCurrentProcess().Id,
                GitCommit = identity.GitCommit,
                GameVersion = Application.version ?? string.Empty,
                StartUtc = started.ToString("o"),
                EndUtc = string.Empty,
                Assertions = assertions,
                Diagnostics = diagnostics,
                Warnings = warnings,
                ExceptionSummary = qualification.Diagnostics.FirstOrDefault(
                    value => value.StartsWith("qualificationException=",
                        StringComparison.Ordinal)) ?? string.Empty,
                EvidenceFiles = new List<string>(),
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static string DescribeEntry(UnityModManager.ModEntry entry)
        {
            if (entry == null) return "missing";
            Assembly assembly = entry.Assembly;
            return "id=" + (entry.Info == null ? "missing" : entry.Info.Id) +
                ";version=" + (entry.Info == null ? "missing" :
                    entry.Info.Version) + ";loaded=" + entry.Loaded +
                ";active=" + entry.Active + ";error=" +
                entry.ErrorOnLoading + ";assembly=" + (assembly == null ?
                    "missing" : assembly.FullName);
        }

        private static void Add(ICollection<RuntimeTestAssertion> assertions,
            string name, string expected, string observed, bool pass,
            string evidence)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = name,
                Expected = expected,
                Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                Evidence = evidence
            });
        }

        private static string HashFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return "missing";
            using (SHA256 hash = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(hash.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }
    }
}
