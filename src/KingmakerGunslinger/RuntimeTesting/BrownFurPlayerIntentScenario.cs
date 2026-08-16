using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker;
using Kingmaker.Blueprints;
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
            [JsonProperty("scoreActivatableCount", Order = 2)] public int ScoreActivatableCount { get; set; }
            [JsonProperty("legacySelectorGranted", Order = 3)] public bool LegacySelectorGranted { get; set; }
            [JsonProperty("shareActivatableGranted", Order = 4)] public bool ShareActivatableGranted { get; set; }
            [JsonProperty("scoreIconsDistinct", Order = 5)] public bool ScoreIconsDistinct { get; set; }
            [JsonProperty("shareIconDistinct", Order = 6)] public bool ShareIconDistinct { get; set; }
            [JsonProperty("allResourceBindingsExact", Order = 7)] public bool AllResourceBindingsExact { get; set; }
            [JsonProperty("allScoreGroupsExact", Order = 8)] public bool AllScoreGroupsExact { get; set; }
            [JsonProperty("reservoirBeforeToggle", Order = 9)] public int ReservoirBeforeToggle { get; set; }
            [JsonProperty("reservoirAfterToggle", Order = 10)] public int ReservoirAfterToggle { get; set; }
            [JsonProperty("strengthOn", Order = 11)] public bool StrengthOn { get; set; }
            [JsonProperty("strengthMarkerOn", Order = 12)] public bool StrengthMarkerOn { get; set; }
            [JsonProperty("strengthObserved", Order = 13)] public string StrengthObserved { get; set; }
            [JsonProperty("strengthOff", Order = 14)] public bool StrengthOff { get; set; }
            [JsonProperty("strengthMarkerOff", Order = 15)] public bool StrengthMarkerOff { get; set; }
            [JsonProperty("switchStrengthOff", Order = 16)] public bool SwitchStrengthOff { get; set; }
            [JsonProperty("switchDexterityOn", Order = 17)] public bool SwitchDexterityOn { get; set; }
            [JsonProperty("switchMarkerCount", Order = 18)] public int SwitchMarkerCount { get; set; }
            [JsonProperty("switchObserved", Order = 19)] public string SwitchObserved { get; set; }
            [JsonProperty("combinedValid", Order = 20)] public bool CombinedValid { get; set; }
            [JsonProperty("combinedScore", Order = 21)] public string CombinedScore { get; set; }
            [JsonProperty("combinedShare", Order = 22)] public bool CombinedShare { get; set; }
            [JsonProperty("combinedSupremacy", Order = 23)] public bool CombinedSupremacy { get; set; }
            [JsonProperty("consumeScoreOff", Order = 24)] public bool ConsumeScoreOff { get; set; }
            [JsonProperty("consumeShareOff", Order = 25)] public bool ConsumeShareOff { get; set; }
            [JsonProperty("consumeMarkersGone", Order = 26)] public bool ConsumeMarkersGone { get; set; }
            [JsonProperty("reservoirAfterConsume", Order = 27)] public int ReservoirAfterConsume { get; set; }
            [JsonProperty("orphanResetValid", Order = 28)] public bool OrphanResetValid { get; set; }
            [JsonProperty("orphanMarkerRemoved", Order = 29)] public bool OrphanMarkerRemoved { get; set; }
            [JsonProperty("orphanRequested", Order = 30)] public bool OrphanRequested { get; set; }
            [JsonProperty("selectorReferencesBefore", Order = 31)] public int SelectorReferencesBefore { get; set; }
            [JsonProperty("selectorReferencesAfter", Order = 32)] public int SelectorReferencesAfter { get; set; }
            [JsonProperty("transientsRemoved", Order = 33)] public bool TransientsRemoved { get; set; }
            [JsonProperty("featuresRemoved", Order = 34)] public bool FeaturesRemoved { get; set; }
            [JsonProperty("unitRemoved", Order = 35)] public bool UnitRemoved { get; set; }
            [JsonProperty("allLiveResourceCountersExact", Order = 36)] public bool AllLiveResourceCountersExact { get; set; }
            [JsonProperty("liveResourceCounters", Order = 37)] public string LiveResourceCounters { get; set; }
            [JsonProperty("actionBarNativeStateAndCounter", Order = 38)] public bool ActionBarNativeStateAndCounter { get; set; }
            [JsonProperty("actionBarActivatableIl", Order = 39)] public string ActionBarActivatableIl { get; set; }
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
            ActivatableAbility[] scores = null;
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
                caster.Descriptor.Resources.Add(contract.Reservoir, true);
                scores = blueprints.ScoreActivatables.Select(value =>
                    BrownFurPlayerIntentRuntime.Find(caster.Descriptor, value))
                    .ToArray();
                share = BrownFurPlayerIntentRuntime.Find(caster.Descriptor,
                    blueprints.ShareTransmutationAbility);
                evidence.ScoreActivatableCount = scores.Count(value => value != null);
                evidence.LegacySelectorGranted = caster.Descriptor.Abilities
                    .GetAbility(blueprints.PowerfulChangeSelection) != null;
                evidence.ShareActivatableGranted = share != null && !share.IsOn;
                Sprite[] scoreIcons = blueprints.ScoreActivatables.Select(
                    value => value.Icon).ToArray();
                evidence.ScoreIconsDistinct = scoreIcons.All(value => value != null) &&
                    scoreIcons.Distinct().Count() == scoreIcons.Length;
                evidence.ShareIconDistinct = blueprints.ShareTransmutationAbility.Icon !=
                    null && !scoreIcons.Contains(
                        blueprints.ShareTransmutationAbility.Icon);
                BlueprintActivatableAbility[] consuming = blueprints
                    .ScoreActivatables.Concat(new[] {
                        blueprints.ShareTransmutationAbility }).ToArray();
                evidence.AllResourceBindingsExact = consuming.All(value =>
                    value.ComponentsArray.OfType<
                        ActivatableAbilityResourceLogic>().Count(component =>
                            ReferenceEquals(component.RequiredResource,
                                contract.Reservoir) && component.SpendType ==
                                ActivatableAbilityResourceLogic.ResourceSpendType
                                    .Never) == 1);
                evidence.AllScoreGroupsExact = blueprints.ScoreActivatables.All(
                    value => value.Group ==
                        BrownFurActivatableGroupRuntime.PowerfulChangeGroup &&
                        value.WeightInGroup == 1);
                evidence.ReservoirBeforeToggle = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                int?[] counters = scores.Concat(new[] { share }).Select(
                    value => value == null ? (int?)null : value.ResourceCount)
                    .ToArray();
                evidence.LiveResourceCounters = string.Join(",",
                    counters.Select(value => value.HasValue ?
                        value.Value.ToString() : "none").ToArray());
                evidence.AllLiveResourceCountersExact = counters.Length == 7 &&
                    counters.All(value => value.HasValue && value.Value ==
                        evidence.ReservoirBeforeToggle);
                Type slotType = typeof(Kingmaker.UI.UnitSettings
                    .MechanicActionBarSlotActivableAbility);
                string slotIl = string.Join("|", new[] { "GetResource",
                    "UpdateSlotInternal" }.SelectMany(method =>
                        BrownFurIlDisassembler.Describe(slotType.GetMethod(
                            method, BindingFlags.Instance |
                            BindingFlags.Public | BindingFlags.NonPublic)))
                    .ToArray());
                evidence.ActionBarActivatableIl = slotIl;
                evidence.ActionBarNativeStateAndCounter =
                    slotIl.Contains("ActivatableAbility.get_ResourceCount()") &&
                    slotIl.Contains("ActivatableAbility.get_IsOn()") &&
                    slotIl.Contains("ActionBarSlot.RunningColor");

                stage = "strength-on";
                scores[0].IsOn = true;
                BrownFurPlayerIntentDecision strength =
                    BrownFurPlayerIntentRuntime.Observe(caster.Descriptor,
                        blueprints);
                evidence.StrengthOn = scores[0].IsOn;
                evidence.StrengthMarkerOn = caster.Descriptor.HasFact(
                    blueprints.ScoreBuffs[0]);
                evidence.StrengthObserved = strength.SelectedAbilityScore.ToString();

                stage = "strength-off";
                scores[0].IsOn = false;
                evidence.StrengthOff = !scores[0].IsOn;
                evidence.StrengthMarkerOff = !caster.Descriptor.HasFact(
                    blueprints.ScoreBuffs[0]);

                stage = "exclusive-switch";
                scores[0].IsOn = true;
                scores[1].IsOn = true;
                BrownFurPlayerIntentDecision switched =
                    BrownFurPlayerIntentRuntime.Observe(caster.Descriptor,
                        blueprints);
                evidence.SwitchStrengthOff = !scores[0].IsOn;
                evidence.SwitchDexterityOn = scores[1].IsOn;
                evidence.SwitchMarkerCount = blueprints.ScoreBuffs.Count(value =>
                    caster.Descriptor.HasFact(value));
                evidence.SwitchObserved = switched.SelectedAbilityScore.ToString();
                evidence.ReservoirAfterToggle = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);

                stage = "combined";
                scores[4].IsOn = true;
                share.IsOn = true;
                BrownFurPlayerIntentDecision combined =
                    BrownFurPlayerIntentRuntime.Observe(caster.Descriptor,
                        blueprints);
                evidence.CombinedValid = combined.Valid &&
                    combined.PowerfulChangeRequested;
                evidence.CombinedScore = combined.SelectedAbilityScore.ToString();
                evidence.CombinedShare = combined.ShareTransmutationRequested;
                evidence.CombinedSupremacy = combined.HasTransmutationSupremacy;

                stage = "consume";
                BrownFurPlayerIntentRuntime.Consume(caster.Descriptor, blueprints,
                    new BrownFurCastDecision(true, string.Empty, 2, true, true,
                        false, 2, BrownFurShareDelivery.Touch,
                        BrownFurAbilityScore.Wisdom));
                evidence.ConsumeScoreOff = !scores[4].IsOn;
                evidence.ConsumeShareOff = !share.IsOn;
                evidence.ConsumeMarkersGone =
                    !blueprints.ScoreBuffs.Any(value =>
                        caster.Descriptor.HasFact(value)) &&
                    !caster.Descriptor.HasFact(
                        blueprints.ShareTransmutationBuff);
                evidence.ReservoirAfterConsume = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);

                stage = "orphan-reset";
                RemoveFeatures(caster, blueprints);
                if (caster.Descriptor.AddFact(blueprints.ScoreBuffs[0]) == null)
                    throw new InvalidOperationException(
                        "The orphan-marker fixture could not be established.");
                BrownFurPlayerIntentDecision orphan =
                    BrownFurPlayerIntentRuntime.Observe(caster.Descriptor,
                        blueprints);
                evidence.OrphanResetValid = orphan.Valid;
                evidence.OrphanMarkerRemoved = !caster.Descriptor.HasFact(
                    blueprints.ScoreBuffs[0]);
                evidence.OrphanRequested = orphan.PowerfulChangeRequested;
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
                "all 25 registered identities and exactly one selector publication",
                "identities=" + evidence.IdentityCount + ";selectorBefore=" +
                    evidence.SelectorReferencesBefore + ";selectorAfter=" +
                    evidence.SelectorReferencesAfter,
                evidence.IdentityCount == BrownFurIdentityCatalog.IdentityCount &&
                    evidence.SelectorReferencesBefore == 1 &&
                    evidence.SelectorReferencesAfter == 1,
                "registered optional-extension blueprint set");
            Add(assertions, "intent-feature-grants",
                "six native score activatables and Share; legacy selector not granted",
                "scores=" + evidence.ScoreActivatableCount + ";legacy=" +
                    evidence.LegacySelectorGranted + ";share=" +
                    evidence.ShareActivatableGranted,
                evidence.ScoreActivatableCount == 6 &&
                    !evidence.LegacySelectorGranted &&
                    evidence.ShareActivatableGranted,
                "native AddFacts feature activation");
            Add(assertions, "intent-icons-resource-group",
                "distinct icons, exact shared reservoir binding, one score group",
                "scoreIcons=" + evidence.ScoreIconsDistinct + ";shareIcon=" +
                    evidence.ShareIconDistinct + ";resource=" +
                    evidence.AllResourceBindingsExact + ";group=" +
                    evidence.AllScoreGroupsExact,
                evidence.ScoreIconsDistinct && evidence.ShareIconDistinct &&
                evidence.AllResourceBindingsExact &&
                    evidence.AllLiveResourceCountersExact &&
                    evidence.ActionBarNativeStateAndCounter &&
                    evidence.AllScoreGroupsExact,
                "registered activatable blueprints, native donor sprites, and ActivatableAbility.ResourceCount=" +
                    evidence.LiveResourceCounters);
            Add(assertions, "intent-native-on-off",
                "Strength uses synchronized native IsOn and hidden marker state",
                "on=" + evidence.StrengthOn + ";markerOn=" +
                    evidence.StrengthMarkerOn + ";observed=" +
                    evidence.StrengthObserved + ";off=" + evidence.StrengthOff +
                    ";markerOff=" + evidence.StrengthMarkerOff,
                evidence.StrengthOn && evidence.StrengthMarkerOn &&
                    evidence.StrengthObserved == "Strength" &&
                    evidence.StrengthOff && evidence.StrengthMarkerOff,
                "native activatable lifecycle; no custom action state");
            Add(assertions, "intent-exclusive-switch-no-cost",
                "Dexterity replaces Strength, one marker remains, reservoir unchanged",
                "strengthOff=" + evidence.SwitchStrengthOff + ";dexterityOn=" +
                    evidence.SwitchDexterityOn + ";markers=" +
                    evidence.SwitchMarkerCount + ";observed=" +
                    evidence.SwitchObserved + ";reservoir=" +
                    evidence.ReservoirBeforeToggle + "/" +
                    evidence.ReservoirAfterToggle,
                evidence.SwitchStrengthOff && evidence.SwitchDexterityOn &&
                    evidence.SwitchMarkerCount == 1 &&
                    evidence.SwitchObserved == "Dexterity" &&
                    evidence.ReservoirBeforeToggle ==
                        evidence.ReservoirAfterToggle,
                "Brown-Fur-specific one-slot activatable group");
            Add(assertions, "intent-combined-request",
                "Wisdom and Share can be armed together under Supremacy",
                "valid=" + evidence.CombinedValid + ";score=" +
                    evidence.CombinedScore + ";share=" +
                    evidence.CombinedShare + ";supremacy=" +
                    evidence.CombinedSupremacy,
                evidence.CombinedValid && evidence.CombinedScore == "Wisdom" &&
                    evidence.CombinedShare && evidence.CombinedSupremacy,
                "independent Powerful and Share native activatables");
            Add(assertions, "intent-success-consumption",
                "participating toggles and markers clear without a UI-layer debit",
                "scoreOff=" + evidence.ConsumeScoreOff + ";shareOff=" +
                    evidence.ConsumeShareOff + ";markers=" +
                    evidence.ConsumeMarkersGone + ";reservoir=" +
                    evidence.ReservoirAfterToggle + "/" +
                    evidence.ReservoirAfterConsume,
                evidence.ConsumeScoreOff && evidence.ConsumeShareOff &&
                    evidence.ConsumeMarkersGone &&
                    evidence.ReservoirAfterToggle == evidence.ReservoirAfterConsume,
                "cast commit remains the sole reservoir authority");
            Add(assertions, "intent-orphan-state-reset",
                "legacy marker without activatable state resets to one coherent OFF state",
                "valid=" + evidence.OrphanResetValid + ";removed=" +
                    evidence.OrphanMarkerRemoved + ";requested=" +
                    evidence.OrphanRequested,
                evidence.OrphanResetValid && evidence.OrphanMarkerRemoved &&
                    !evidence.OrphanRequested,
                "save-compatibility reconciliation never exposes contradictory state");
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
