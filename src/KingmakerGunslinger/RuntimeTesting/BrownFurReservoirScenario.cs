using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.BrownFur;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class BrownFurReservoirScenario
    {
        private const string FileName = "brown-fur-reservoir-accounting.json";

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class Evidence
        {
            [JsonProperty("reservoirGuid", Order = 1)] public string ReservoirGuid { get; set; }
            [JsonProperty("ownedBefore", Order = 2)] public bool OwnedBefore { get; set; }
            [JsonProperty("ownedAfterAdd", Order = 3)] public bool OwnedAfterAdd { get; set; }
            [JsonProperty("initialAmount", Order = 4)] public int InitialAmount { get; set; }
            [JsonProperty("exactSuccess", Order = 5)] public bool ExactSuccess { get; set; }
            [JsonProperty("exactFailure", Order = 6)] public string ExactFailure { get; set; }
            [JsonProperty("exactBefore", Order = 7)] public int ExactBefore { get; set; }
            [JsonProperty("exactObservedAfter", Order = 8)] public int ExactObservedAfter { get; set; }
            [JsonProperty("amountAfterExact", Order = 9)] public int AmountAfterExact { get; set; }
            [JsonProperty("amountAfterRestore", Order = 10)] public int AmountAfterRestore { get; set; }
            [JsonProperty("insufficientBefore", Order = 11)] public int InsufficientBefore { get; set; }
            [JsonProperty("insufficientSuccess", Order = 12)] public bool InsufficientSuccess { get; set; }
            [JsonProperty("insufficientFailure", Order = 13)] public string InsufficientFailure { get; set; }
            [JsonProperty("insufficientAfter", Order = 14)] public int InsufficientAfter { get; set; }
            [JsonProperty("ownedAfterRemove", Order = 15)] public bool OwnedAfterRemove { get; set; }
            [JsonProperty("missingOwnerSuccess", Order = 16)] public bool MissingOwnerSuccess { get; set; }
            [JsonProperty("missingOwnerFailure", Order = 17)] public string MissingOwnerFailure { get; set; }
            [JsonProperty("resourceRemoved", Order = 18)] public bool ResourceRemoved { get; set; }
            [JsonProperty("unitRemoved", Order = 19)] public bool UnitRemoved { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence();
            UnitEntityData caster = null;
            bool registered = false;
            CotwArcanistContract contract = null;
            string stage = "contract";
            try
            {
                CotwArcanistResolution resolution =
                    BrownFurOptionalExtensionCoordinator.Current;
                if (resolution == null || !resolution.Decision.IsCompatible ||
                    resolution.Contract == null ||
                    resolution.Contract.Reservoir == null)
                    throw new InvalidOperationException(
                        "Compatible CotW reservoir contract is unavailable.");
                contract = resolution.Contract;
                evidence.ReservoirGuid = contract.Reservoir.AssetGuid;

                stage = "unit";
                caster = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                registered = Game.Instance.State.Units.All.Add(caster);
                if (!registered) throw new InvalidOperationException(
                    "The disposable reservoir caster was not registered.");
                evidence.OwnedBefore = caster.Descriptor.Resources
                    .ContainsResource(contract.Reservoir);
                caster.Descriptor.Resources.Add(contract.Reservoir, true);
                evidence.OwnedAfterAdd = caster.Descriptor.Resources
                    .ContainsResource(contract.Reservoir);
                evidence.InitialAmount = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                if (!evidence.OwnedAfterAdd || evidence.InitialAmount < 2)
                    throw new InvalidOperationException(
                        "The real CotW reservoir did not initialize with two points.");

                stage = "exact-debit";
                BrownFurReservoirDebitResult exact =
                    BrownFurReservoirDebit.TryDebitExact(caster.Descriptor,
                        contract.Reservoir, 2);
                evidence.ExactSuccess = exact.Success;
                evidence.ExactFailure = exact.Failure;
                evidence.ExactBefore = exact.Before;
                evidence.ExactObservedAfter = exact.ObservedAfterSpend;
                evidence.AmountAfterExact = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                caster.Descriptor.Resources.Restore(contract.Reservoir, 2);
                evidence.AmountAfterRestore = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);

                stage = "insufficient";
                caster.Descriptor.Resources.Spend(contract.Reservoir,
                    evidence.InitialAmount - 1);
                evidence.InsufficientBefore = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                BrownFurReservoirDebitResult insufficient =
                    BrownFurReservoirDebit.TryDebitExact(caster.Descriptor,
                        contract.Reservoir, 2);
                evidence.InsufficientSuccess = insufficient.Success;
                evidence.InsufficientFailure = insufficient.Failure;
                evidence.InsufficientAfter = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                caster.Descriptor.Resources.Restore(contract.Reservoir,
                    evidence.InitialAmount - evidence.InsufficientAfter);

                stage = "missing-owner";
                caster.Descriptor.Resources.Remove(contract.Reservoir);
                evidence.OwnedAfterRemove = caster.Descriptor.Resources
                    .ContainsResource(contract.Reservoir);
                BrownFurReservoirDebitResult missing =
                    BrownFurReservoirDebit.TryDebitExact(caster.Descriptor,
                        contract.Reservoir, 1);
                evidence.MissingOwnerSuccess = missing.Success;
                evidence.MissingOwnerFailure = missing.Failure;
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" + exception);
            }
            finally
            {
                if (caster != null && contract != null &&
                    caster.Descriptor.Resources.ContainsResource(
                        contract.Reservoir))
                    caster.Descriptor.Resources.Remove(contract.Reservoir);
                evidence.ResourceRemoved = caster == null || contract == null ||
                    !caster.Descriptor.Resources.ContainsResource(
                        contract.Reservoir);
                if (registered) Game.Instance.State.Units.All.Remove(caster);
                if (caster != null) caster.Dispose();
                evidence.UnitRemoved = caster == null ||
                    !Game.Instance.State.Units.All.Contains(caster);
            }

            Add(assertions, "reservoir-contract-exact",
                "resolved CotW reservoir GUID",
                evidence.ReservoirGuid ?? string.Empty,
                evidence.ReservoirGuid ==
                    "3b775ee982444493b3de8f7bc31bd872",
                "structurally compatible CotW contract");
            Add(assertions, "reservoir-disposable-owner",
                "new disposable unit gains initialized real reservoir",
                "before=" + evidence.OwnedBefore + ";after=" +
                    evidence.OwnedAfterAdd + ";amount=" +
                    evidence.InitialAmount,
                !evidence.OwnedBefore && evidence.OwnedAfterAdd &&
                    evidence.InitialAmount >= 2,
                "native UnitAbilityResourceCollection.Add with restore");
            Add(assertions, "reservoir-combined-debit-exact",
                "combined cost debits exactly two once",
                "success=" + evidence.ExactSuccess + ";failure=" +
                    evidence.ExactFailure + ";before=" + evidence.ExactBefore +
                    ";observed=" + evidence.ExactObservedAfter + ";after=" +
                    evidence.AmountAfterExact,
                evidence.ExactSuccess && evidence.ExactBefore ==
                    evidence.InitialAmount && evidence.ExactObservedAfter ==
                    evidence.InitialAmount - 2 && evidence.AmountAfterExact ==
                    evidence.InitialAmount - 2,
                "BrownFurReservoirDebit over the real CotW resource");
            Add(assertions, "reservoir-restore-exact",
                "qualification restoration returns original amount",
                evidence.AmountAfterRestore.ToString(),
                evidence.AmountAfterRestore == evidence.InitialAmount,
                "native exact restore after observed debit");
            Add(assertions, "reservoir-insufficient-no-debit",
                "one point cannot fund combined cost and remains unchanged",
                "before=" + evidence.InsufficientBefore + ";success=" +
                    evidence.InsufficientSuccess + ";failure=" +
                    evidence.InsufficientFailure + ";after=" +
                    evidence.InsufficientAfter,
                evidence.InsufficientBefore == 1 &&
                    !evidence.InsufficientSuccess &&
                    evidence.InsufficientFailure == "reservoir-insufficient" &&
                    evidence.InsufficientAfter == 1,
                "availability gate runs before native Spend");
            Add(assertions, "reservoir-missing-owner-no-debit",
                "removed resource rejects without mutation",
                "owned=" + evidence.OwnedAfterRemove + ";success=" +
                    evidence.MissingOwnerSuccess + ";failure=" +
                    evidence.MissingOwnerFailure,
                !evidence.OwnedAfterRemove && !evidence.MissingOwnerSuccess &&
                    evidence.MissingOwnerFailure == "reservoir-not-owned",
                "exact resource ownership gate");
            Add(assertions, "reservoir-cleanup",
                "resource and disposable unit removed",
                "resource=" + evidence.ResourceRemoved + ";unit=" +
                    evidence.UnitRemoved,
                evidence.ResourceRemoved && evidence.UnitRemoved,
                "bounded disposable fixture cleanup");

            string path = Path.Combine(request.EvidenceDirectory, FileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            evidenceFiles.Add(path);
            diagnostics.Add("reservoirAccountingSha256=" + Hash(path));
            bool pass = assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult {
                SchemaVersion = 1, RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" +
                    assembly.ManifestModule.ModuleVersionId + ";sha256=" +
                    Hash(assembly.Location) + ";pid=" +
                    Process.GetCurrentProcess().Id,
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
