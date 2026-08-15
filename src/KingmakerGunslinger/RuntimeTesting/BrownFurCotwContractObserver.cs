using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.BrownFur;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.FeatureModules;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class BrownFurCotwContractObserver
    {
        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            CotwArcanistResolution resolution =
                BrownFurOptionalExtensionCoordinator.Current;
            BrownFurBlueprintSet blueprints =
                BrownFurOptionalExtensionCoordinator.Blueprints;
            BrownFurFeatureStatus status = BrownFurFeatureStatusRegistry.Current;
            Add(assertions, "package-bootstrap-isolated", "ready",
                context.IsReady ? "ready" : "not-ready", context.IsReady,
                "ModContext package state remains authoritative and ready");
            Add(assertions, "cotw-contract-resolution", "compatible",
                resolution == null ? "missing" :
                    resolution.Decision.Availability + ":" +
                    resolution.Decision.FailedCheck,
                resolution != null && resolution.Decision.IsCompatible &&
                    resolution.Contract != null,
                "reflection-only CotwArcanistResolver and pure contract policy");

            CotwArcanistContract contract = resolution == null ? null :
                resolution.Contract;
            CotwCompatibilityFingerprint fingerprint = contract == null ? null :
                contract.Fingerprint;
            CotwProgressionDecision progression = contract == null ? null :
                contract.ProgressionDecision;
            bool recognized = progression != null && progression.Compatible &&
                ((progression.Shape == CotwProgressionShape.Normal &&
                    progression.PowerfulChangeReplacementLevel == 3 &&
                    progression.ShareTransmutationReplacementLevel == 9) ||
                 (progression.Shape == CotwProgressionShape.BalanceFixes &&
                    progression.PowerfulChangeReplacementLevel == 4 &&
                    progression.ShareTransmutationReplacementLevel == 10));
            Add(assertions, "cotw-progression-shape",
                "normal=3/9 or balance-fixes=4/10",
                progression == null ? "missing" : progression.Shape + ":" +
                    progression.PowerfulChangeReplacementLevel + "/" +
                    progression.ShareTransmutationReplacementLevel,
                recognized, "resolved exploit-bearing LevelEntry references");
            bool balanceSettingMatches = fingerprint != null && progression != null &&
                ((progression.Shape == CotwProgressionShape.BalanceFixes &&
                    fingerprint.BalanceFixesSetting == bool.TrueString) ||
                 (progression.Shape == CotwProgressionShape.Normal &&
                    fingerprint.BalanceFixesSetting == bool.FalseString));
            Add(assertions, "cotw-balance-setting-agrees-with-progression",
                "live setting and resolved LevelEntries agree",
                fingerprint == null || progression == null ? "missing" :
                    fingerprint.BalanceFixesSetting + "/" + progression.Shape,
                balanceSettingMatches,
                "exact CotW settings.balance_fixes property and resolved progression graph");
            Add(assertions, "cotw-required-identities",
                "exact class/progression/spellbooks/reservoir/Magical Supremacy GUIDs",
                fingerprint == null ? "missing" : fingerprint.ToString(),
                fingerprint != null &&
                    fingerprint.ArcanistClassGuid ==
                        "19c3cf3d51cf4cbf9a136a600c26585a" &&
                    fingerprint.ProgressionGuid ==
                        "2d28526efc2e4a9cb6a84c85267fb344" &&
                    fingerprint.CastingSpellbookGuid ==
                        "0c21cfcab6ce4395bd4df330ab3cf715" &&
                    fingerprint.MemorizationSpellbookGuid ==
                        "ab76417567444a6cb87d9d53e9752955" &&
                    fingerprint.ReservoirGuid ==
                        "3b775ee982444493b3de8f7bc31bd872" &&
                    fingerprint.MagicalSupremacyGuid ==
                        "2d86a417ab1542f98a8444b2b97d4951",
                "live CotW static fields cross-validated against graph references");
            Add(assertions, "cotw-shared-spells-signatures", "exactly two",
                fingerprint == null || fingerprint.SharedSpellsSignatures == null ?
                    "missing" : string.Join("|",
                        fingerprint.SharedSpellsSignatures.ToArray()),
                fingerprint != null && fingerprint.SharedSpellsSignatures != null &&
                    fingerprint.SharedSpellsSignatures.Count == 2 &&
                    fingerprint.SharedSpellsSignatures.All(value =>
                        value.IndexOf("System.Boolean", StringComparison.Ordinal) >= 0),
                "exact reflection-resolved Shared Spells method signatures");
            Add(assertions, "cotw-transmutation-inventory-presence", ">0",
                fingerprint == null ? "missing" :
                    fingerprint.TransmutationSpellCount.ToString(),
                fingerprint != null && fingerprint.TransmutationSpellCount > 0,
                "live Arcanist casting spell list school/type enumeration");
            Add(assertions, "cotw-fingerprint-binary", "hash, MVID, version, settings hash",
                fingerprint == null ? "missing" : fingerprint.ToString(),
                fingerprint != null && IsSha(fingerprint.DllSha256) &&
                    IsSha(fingerprint.SettingsSha256) &&
                    !string.IsNullOrWhiteSpace(fingerprint.DllMvid) &&
                    !string.IsNullOrWhiteSpace(fingerprint.ModVersion),
                "live loaded CotW assembly and exact settings bytes");
            Add(assertions, "brown-fur-effective-status",
                "available, stable identities registered, not published",
                status.DependencyStatus + ";" + status.PublicationStatus +
                    ";detail=" + status.Detail,
                status.Availability == BrownFurDependencyAvailability.Available &&
                    !status.Published,
                "dependency state is distinct from saved intent and publication");
            bool identitiesExact = blueprints != null &&
                BlueprintBootstrap.Library != null &&
                blueprints.Count == BrownFurIdentityCatalog.IdentityCount &&
                BrownFurIdentityCatalog.All.All(spec =>
                    BlueprintBootstrap.Library.BlueprintsByAssetId.ContainsKey(
                        spec.Guid));
            Add(assertions, "brown-fur-stable-identities",
                "19 manifest-backed identities registered",
                blueprints == null ? "missing" : blueprints.Count.ToString(),
                identitiesExact,
                "optional BlueprintRegistry and permanent BrownFurIdentityCatalog GUID ledger");
            int archetypeReferences = contract == null ||
                contract.ArcanistClass == null ||
                contract.ArcanistClass.Archetypes == null ||
                blueprints == null ? 0 : contract.ArcanistClass.Archetypes.Count(
                    value => ReferenceEquals(value, blueprints.Archetype));
            Add(assertions, "brown-fur-publication-gate", "0 selector references",
                archetypeReferences.ToString(), archetypeReferences == 0,
                "stable assets exist for save identity while player-facing publication remains mechanically gated");
            Add(assertions, "brown-fur-reconciliation", ">=1", BrownFurOptionalExtensionCoordinator
                .SuccessfulReconciliations.ToString(),
                BrownFurOptionalExtensionCoordinator.SuccessfulReconciliations >= 1,
                "postfix/immediate/fallback coordinator counter");
            Add(assertions, "save-free-observer", "no save or input API invoked",
                "read-only contract inspection", true,
                "observer does not select, load, mutate, or save a character");
            if (fingerprint != null) diagnostics.Add(fingerprint.ToString());
            bool pass = assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" +
                    assembly.ManifestModule.ModuleVersionId + ";sha256=" +
                    Hash(assembly.Location) + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = Metadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = DateTime.UtcNow.ToString("o"),
                EndUtc = string.Empty,
                Assertions = assertions,
                Diagnostics = diagnostics,
                Warnings = new List<string>(),
                ExceptionSummary = string.Empty,
                EvidenceFiles = new List<string>(),
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

        private static bool IsSha(string value)
        {
            return value != null && value.Length == 64 && value.All(character =>
                (character >= '0' && character <= '9') ||
                (character >= 'A' && character <= 'F'));
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
