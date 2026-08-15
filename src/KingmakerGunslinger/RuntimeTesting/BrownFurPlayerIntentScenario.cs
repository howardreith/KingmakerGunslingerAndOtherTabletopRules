using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.BrownFur;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class BrownFurPlayerIntentScenario
    {
        private const string FileName = "brown-fur-player-intent.json";

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class Evidence
        {
            [JsonProperty("identityCount", Order = 1)] public int IdentityCount { get; set; }
            [JsonProperty("selectionGranted", Order = 2)] public bool SelectionGranted { get; set; }
            [JsonProperty("shareActivatableGranted", Order = 3)] public bool ShareActivatableGranted { get; set; }
            [JsonProperty("featuresObserved", Order = 4)] public bool FeaturesObserved { get; set; }
            [JsonProperty("combinedValid", Order = 5)] public bool CombinedValid { get; set; }
            [JsonProperty("combinedPowerful", Order = 6)] public bool CombinedPowerful { get; set; }
            [JsonProperty("combinedScore", Order = 7)] public string CombinedScore { get; set; }
            [JsonProperty("combinedShare", Order = 8)] public bool CombinedShare { get; set; }
            [JsonProperty("combinedSupremacy", Order = 9)] public bool CombinedSupremacy { get; set; }
            [JsonProperty("scoreMarkerAfterClear", Order = 10)] public bool ScoreMarkerAfterClear { get; set; }
            [JsonProperty("shareMarkerAfterClear", Order = 11)] public bool ShareMarkerAfterClear { get; set; }
            [JsonProperty("shareActivatableOnAfterClear", Order = 12)] public bool ShareActivatableOnAfterClear { get; set; }
            [JsonProperty("featuresAfterClear", Order = 13)] public bool FeaturesAfterClear { get; set; }
            [JsonProperty("orphanValid", Order = 14)] public bool OrphanValid { get; set; }
            [JsonProperty("orphanFailure", Order = 15)] public string OrphanFailure { get; set; }
            [JsonProperty("orphanRequested", Order = 16)] public bool OrphanRequested { get; set; }
            [JsonProperty("selectorReferencesBefore", Order = 17)] public int SelectorReferencesBefore { get; set; }
            [JsonProperty("selectorReferencesAfter", Order = 18)] public int SelectorReferencesAfter { get; set; }
            [JsonProperty("transientsRemoved", Order = 19)] public bool TransientsRemoved { get; set; }
            [JsonProperty("featuresRemoved", Order = 20)] public bool FeaturesRemoved { get; set; }
            [JsonProperty("unitRemoved", Order = 21)] public bool UnitRemoved { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            string start = DateTime.UtcNow.ToString("o");
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence();
            UnitEntityData caster = null;
            bool registered = false;
            BrownFurBlueprintSet blueprints = null;
            CotwArcanistContract contract = null;
            ActivatableAbility share = null;
            string stage = "contract";
            try
            {
                CotwArcanistResolution resolution =
                    BrownFurOptionalExtensionCoordinator.Current;
                blueprints = BrownFurOptionalExtensionCoordinator.Blueprints;
                if (resolution == null || !resolution.Decision.IsCompatible ||
                    resolution.Contract == null || blueprints == null)
                    throw new InvalidOperationException(
                        "Compatible registered Brown-Fur blueprints are unavailable.");
                contract = resolution.Contract;
                evidence.IdentityCount = blueprints.Count;
                evidence.SelectorReferencesBefore = SelectorReferences(
                    contract.ArcanistClass, blueprints);

                stage = "unit";
                caster = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                registered = Game.Instance.State.Units.All.Add(caster);
                if (!registered) throw new InvalidOperationException(
                    "The disposable player-intent caster was not registered.");

                stage = "features";
                if (caster.Descriptor.AddFact(blueprints.PowerfulChange) == null ||
                    caster.Descriptor.AddFact(blueprints.ShareTransmutation) == null ||
                    caster.Descriptor.AddFact(
                        blueprints.TransmutationSupremacy) == null)
                    throw new InvalidOperationException(
                        "A real Brown-Fur feature could not be granted.");
                evidence.SelectionGranted = caster.Descriptor.Abilities.GetAbility(
                    blueprints.PowerfulChangeSelection) != null;
                share = caster.Descriptor.ActivatableAbilities.Enumerable
                    .SingleOrDefault(value => value != null && ReferenceEquals(
                        value.Blueprint, blueprints.ShareTransmutationAbility));
                evidence.ShareActivatableGranted = share != null && !share.IsOn;
                BrownFurPlayerIntentDecision features =
                    BrownFurPlayerIntentRuntime.Observe(caster.Descriptor,
                        blueprints);
                evidence.FeaturesObserved = features.Valid &&
                    features.CasterOwnsBrownFur && features.HasPowerfulChange &&
                    features.HasShareTransmutation &&
                    features.HasTransmutationSupremacy &&
                    !features.PowerfulChangeRequested &&
                    !features.ShareTransmutationRequested;

                stage = "combined";
                if (caster.Descriptor.AddFact(blueprints.ScoreBuffs[4]) == null)
                    throw new InvalidOperationException(
                        "The real Wisdom selection marker could not be added.");
                if (share == null) throw new InvalidOperationException(
                    "The real Share Transmutation activatable is absent.");
                share.IsOn = true;
                BrownFurPlayerIntentDecision combined =
                    BrownFurPlayerIntentRuntime.Observe(caster.Descriptor,
                        blueprints);
                evidence.CombinedValid = combined.Valid;
                evidence.CombinedPowerful = combined.PowerfulChangeRequested;
                evidence.CombinedScore = combined.SelectedAbilityScore.ToString();
                evidence.CombinedShare = combined.ShareTransmutationRequested;
                evidence.CombinedSupremacy =
                    combined.HasTransmutationSupremacy;

                stage = "clear";
                BrownFurPlayerIntentRuntime.Clear(caster.Descriptor, blueprints);
                evidence.ScoreMarkerAfterClear = blueprints.ScoreBuffs.Any(
                    value => caster.Descriptor.HasFact(value));
                evidence.ShareMarkerAfterClear = caster.Descriptor.HasFact(
                    blueprints.ShareTransmutationBuff);
                evidence.ShareActivatableOnAfterClear = share.IsOn;
                BrownFurPlayerIntentDecision afterClear =
                    BrownFurPlayerIntentRuntime.Observe(caster.Descriptor,
                        blueprints);
                evidence.FeaturesAfterClear = afterClear.Valid &&
                    afterClear.HasPowerfulChange &&
                    afterClear.HasShareTransmutation &&
                    afterClear.HasTransmutationSupremacy &&
                    !afterClear.PowerfulChangeRequested &&
                    !afterClear.ShareTransmutationRequested;

                stage = "orphan";
                RemoveFeatures(caster, blueprints);
                if (caster.Descriptor.AddFact(blueprints.ScoreBuffs[0]) == null)
                    throw new InvalidOperationException(
                        "The orphan-marker fixture could not be established.");
                BrownFurPlayerIntentDecision orphan =
                    BrownFurPlayerIntentRuntime.Observe(caster.Descriptor,
                        blueprints);
                evidence.OrphanValid = orphan.Valid;
                evidence.OrphanFailure = orphan.Failure;
                evidence.OrphanRequested = orphan.PowerfulChangeRequested;
                BrownFurPlayerIntentRuntime.Clear(caster.Descriptor, blueprints);
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" + exception);
            }
            finally
            {
                if (caster != null && blueprints != null)
                {
                    BrownFurPlayerIntentRuntime.Clear(caster.Descriptor,
                        blueprints);
                    RemoveFeatures(caster, blueprints);
                    evidence.TransientsRemoved =
                        !blueprints.ScoreBuffs.Any(value =>
                            caster.Descriptor.HasFact(value)) &&
                        !caster.Descriptor.HasFact(
                            blueprints.ShareTransmutationBuff);
                    evidence.FeaturesRemoved =
                        !caster.Descriptor.HasFact(blueprints.PowerfulChange) &&
                        !caster.Descriptor.HasFact(
                            blueprints.ShareTransmutation) &&
                        !caster.Descriptor.HasFact(
                            blueprints.TransmutationSupremacy);
                }
                evidence.SelectorReferencesAfter = contract == null ||
                    blueprints == null ? -1 : SelectorReferences(
                        contract.ArcanistClass, blueprints);
                if (registered) Game.Instance.State.Units.All.Remove(caster);
                if (caster != null) caster.Dispose();
                evidence.UnitRemoved = caster == null ||
                    !Game.Instance.State.Units.All.Contains(caster);
            }

            Add(assertions, "intent-stable-blueprints",
                "all 19 registered identities and exactly one selector publication",
                "identities=" + evidence.IdentityCount + ";selectorBefore=" +
                    evidence.SelectorReferencesBefore + ";selectorAfter=" +
                    evidence.SelectorReferencesAfter,
                evidence.IdentityCount == 19 &&
                    evidence.SelectorReferencesBefore == 1 &&
                    evidence.SelectorReferencesAfter == 1,
                "registered optional-extension blueprint set");
            Add(assertions, "intent-feature-grants",
                "real features grant selection ability and off-by-default Share activatable",
                "selection=" + evidence.SelectionGranted + ";share=" +
                    evidence.ShareActivatableGranted,
                evidence.SelectionGranted && evidence.ShareActivatableGranted,
                "native AddFacts feature activation");
            Add(assertions, "intent-feature-ownership",
                "three owned features with no transient requests",
                evidence.FeaturesObserved.ToString(), evidence.FeaturesObserved,
                "BrownFurPlayerIntentRuntime over UnitDescriptor facts");
            Add(assertions, "intent-combined-request",
                "Wisdom Powerful Change plus Share and Supremacy observed together",
                "valid=" + evidence.CombinedValid + ";powerful=" +
                    evidence.CombinedPowerful + ";score=" +
                    evidence.CombinedScore + ";share=" +
                    evidence.CombinedShare + ";supremacy=" +
                    evidence.CombinedSupremacy,
                evidence.CombinedValid && evidence.CombinedPowerful &&
                    evidence.CombinedScore == "Wisdom" &&
                    evidence.CombinedShare && evidence.CombinedSupremacy,
                "one score marker plus native Share activatable buff");
            Add(assertions, "intent-clear-transients",
                "clear removes both requests but retains all three features",
                "score=" + evidence.ScoreMarkerAfterClear + ";shareBuff=" +
                    evidence.ShareMarkerAfterClear + ";shareOn=" +
                    evidence.ShareActivatableOnAfterClear + ";features=" +
                    evidence.FeaturesAfterClear,
                !evidence.ScoreMarkerAfterClear &&
                    !evidence.ShareMarkerAfterClear &&
                    !evidence.ShareActivatableOnAfterClear &&
                    evidence.FeaturesAfterClear,
                "owner-scoped one-shot cleanup");
            Add(assertions, "intent-orphan-marker-rejected",
                "orphan score marker fails closed without arming",
                "valid=" + evidence.OrphanValid + ";failure=" +
                    evidence.OrphanFailure + ";requested=" +
                    evidence.OrphanRequested,
                !evidence.OrphanValid && evidence.OrphanFailure ==
                    "powerful-feature-missing" && !evidence.OrphanRequested,
                "feature ownership is independent from transient marker identity");
            Add(assertions, "intent-disposable-cleanup",
                "all request-local facts and the disposable unit removed",
                "transients=" + evidence.TransientsRemoved + ";features=" +
                    evidence.FeaturesRemoved + ";unit=" + evidence.UnitRemoved,
                evidence.TransientsRemoved && evidence.FeaturesRemoved &&
                    evidence.UnitRemoved,
                "bounded finally cleanup without save mutation");

            string path = Path.Combine(request.EvidenceDirectory, FileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            evidenceFiles.Add(path);
            diagnostics.Add("playerIntentSha256=" + Hash(path));
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
                StartUtc = start, EndUtc = DateTime.UtcNow.ToString("o"),
                Assertions = assertions, Diagnostics = diagnostics,
                Warnings = new List<string>(), ExceptionSummary = string.Empty,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static int SelectorReferences(BlueprintCharacterClass arcanist,
            BrownFurBlueprintSet blueprints)
        {
            return (arcanist.Archetypes ?? new BlueprintArchetype[0]).Count(
                value => ReferenceEquals(value, blueprints.Archetype));
        }

        private static void RemoveFeatures(UnitEntityData caster,
            BrownFurBlueprintSet blueprints)
        {
            if (caster.Descriptor.HasFact(blueprints.PowerfulChange))
                caster.Descriptor.RemoveFact(blueprints.PowerfulChange);
            if (caster.Descriptor.HasFact(blueprints.ShareTransmutation))
                caster.Descriptor.RemoveFact(blueprints.ShareTransmutation);
            if (caster.Descriptor.HasFact(blueprints.TransmutationSupremacy))
                caster.Descriptor.RemoveFact(blueprints.TransmutationSupremacy);
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
