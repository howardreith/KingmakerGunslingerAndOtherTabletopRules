using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free Release A observation of the production heritage graph.
    /// This scenario reads live registered blueprints and the shared race
    /// selector; it creates no units and touches no save state.
    /// </summary>
    internal static class ElementalHeritageBlueprintScenario
    {
        internal const string EvidenceFileName =
            "elemental-heritage-blueprints.json";

        private sealed class ChoiceEvidence
        {
            public string Name { get; set; }
            public string MarkerGuid { get; set; }
            public string AffinityGuid { get; set; }
            public string SlaFeatureGuid { get; set; }
            public string SlaResourceGuid { get; set; }
            public string SlaAbilityGuid { get; set; }
            public string AbilityType { get; set; }
            public bool General { get; set; }
            public bool MetadataComplete { get; set; }
            public int AuxiliaryCount { get; set; }
        }

        private sealed class RaceEvidence
        {
            public string Race { get; set; }
            public string RaceGuid { get; set; }
            public string SelectionGuid { get; set; }
            public int TopLevelOccurrences { get; set; }
            public int SelectionOccurrences { get; set; }
            public List<ChoiceEvidence> Choices { get; set; }
        }

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool ModuleActive { get; set; }
            public bool SaveStateTouched { get; set; }
            public int HeritageIdentityCount { get; set; }
            public int RegisteredIdentityCount { get; set; }
            public List<RaceEvidence> Races { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence
            {
                SchemaVersion = 1,
                ModuleActive = context.FeatureModules.Active.ElementalRaces,
                SaveStateTouched = false,
                Races = new List<RaceEvidence>()
            };
            string exceptionSummary = string.Empty;
            try
            {
                ElementalRaceBlueprintSet set = BlueprintBootstrap
                    .ElementalRaces;
                LibraryScriptableObject library = BlueprintBootstrap.Library;
                BlueprintRoot root = BlueprintRoot.Instance;
                if (set == null || library == null || root == null ||
                    root.Progression == null ||
                    root.Progression.CharacterRaces == null)
                    throw new InvalidOperationException(
                        "The live heritage blueprint graph is unavailable.");

                BlueprintRace[] topLevel = root.Progression.CharacterRaces;
                var owned = new List<BlueprintScriptableObject>();
                foreach (ElementalRaceBlueprints race in
                    set.OrderedBlueprints())
                {
                    ElementalHeritageRaceBlueprints graph = race.Heritages;
                    ElementalHeritageBlueprints[] choices = graph.Choices()
                        .ToArray();
                    owned.Add(graph.Selection);
                    var row = new RaceEvidence
                    {
                        Race = race.Definition.DisplayName,
                        RaceGuid = race.Race.AssetGuid,
                        SelectionGuid = graph.Selection.AssetGuid,
                        TopLevelOccurrences = topLevel.Count(value =>
                            ReferenceEquals(value, race.Race)),
                        SelectionOccurrences = (race.Race.Features ??
                            new BlueprintFeature[0]).Count(value =>
                                ReferenceEquals(value, graph.Selection)),
                        Choices = new List<ChoiceEvidence>()
                    };
                    foreach (ElementalHeritageBlueprints choice in choices)
                    {
                        owned.Add(choice.Marker);
                        if (!choice.Definition.IsGeneral)
                        {
                            owned.Add(choice.Affinity);
                            owned.Add(choice.SlaFeature);
                            owned.Add(choice.SlaResource);
                            owned.Add(choice.SlaAbility);
                            owned.AddRange(choice.AuxiliaryBlueprints);
                        }
                        row.Choices.Add(new ChoiceEvidence
                        {
                            Name = choice.Definition.Name,
                            MarkerGuid = choice.Marker.AssetGuid,
                            AffinityGuid = choice.Affinity.AssetGuid,
                            SlaFeatureGuid = choice.SlaFeature.AssetGuid,
                            SlaResourceGuid = choice.SlaResource.AssetGuid,
                            SlaAbilityGuid = choice.SlaAbility.AssetGuid,
                            AbilityType = choice.SlaAbility.Type.ToString(),
                            General = choice.Definition.IsGeneral,
                            MetadataComplete = choice.Marker.Icon != null &&
                                !string.IsNullOrWhiteSpace(choice.Marker.Name) &&
                                !string.IsNullOrWhiteSpace(
                                    choice.Marker.Description) &&
                                choice.SlaAbility.Icon != null &&
                                !string.IsNullOrWhiteSpace(
                                    choice.SlaAbility.Name) &&
                                !string.IsNullOrWhiteSpace(
                                    choice.SlaAbility.Description),
                            AuxiliaryCount = choice.AuxiliaryBlueprints.Length
                        });
                    }
                    evidence.Races.Add(row);

                    bool selectionExact = graph.Selection.Obligatory &&
                        !graph.Selection.IgnorePrerequisites &&
                        graph.Selection.AllFeatures != null &&
                        graph.Selection.AllFeatures.SequenceEqual(
                            choices.Select(value => value.Marker));
                    Add(assertions, "elemental-heritage-selection-" +
                        race.Definition.DisplayName.ToLowerInvariant(),
                        "one obligatory ordered three-choice selection",
                        graph.Selection.AssetGuid + ";choices=" +
                            choices.Length + ";top=" +
                            row.TopLevelOccurrences + ";raceFeature=" +
                            row.SelectionOccurrences,
                        selectionExact && choices.Length == 3 &&
                            choices.Count(value =>
                                value.Definition.IsGeneral) == 1 &&
                            row.TopLevelOccurrences == 1 &&
                            row.SelectionOccurrences == 1,
                        "live production BlueprintFeatureSelection and " +
                            "BlueprintRoot.Progression.CharacterRaces");
                    ElementalHeritageBlueprints general = graph.General;
                    Add(assertions, "elemental-heritage-general-reuse-" +
                        race.Definition.DisplayName.ToLowerInvariant(),
                        "exact legacy affinity/SLA feature/resource/ability",
                        general.Affinity.AssetGuid + ";" +
                            general.SlaFeature.AssetGuid + ";" +
                            general.SlaResource.AssetGuid + ";" +
                            general.SlaAbility.AssetGuid,
                        ReferenceEquals(general.Affinity, race.Affinity) &&
                            ReferenceEquals(general.SlaFeature,
                                race.SlaFeature) &&
                            ReferenceEquals(general.SlaResource,
                                race.SlaResource) &&
                            ReferenceEquals(general.SlaAbility,
                                race.SlaAbility),
                        "live reference identity");
                    bool alternatesExact = choices.Where(value =>
                            !value.Definition.IsGeneral).All(value =>
                                !ReferenceEquals(value.Affinity,
                                    race.Affinity) &&
                                !ReferenceEquals(value.SlaFeature,
                                    race.SlaFeature) &&
                                !ReferenceEquals(value.SlaResource,
                                    race.SlaResource) &&
                                !ReferenceEquals(value.SlaAbility,
                                    race.SlaAbility) &&
                                value.SlaAbility.Type ==
                                    AbilityType.SpellLike);
                    Add(assertions, "elemental-heritage-alternate-providers-" +
                        race.Definition.DisplayName.ToLowerInvariant(),
                        "two independent SpellLike provider graphs",
                        string.Join("|", row.Choices.Where(value =>
                            !value.General).Select(value => value.Name + ":" +
                            value.AbilityType).ToArray()),
                        alternatesExact, "live provider reference identity");
                    Add(assertions, "elemental-heritage-metadata-" +
                        race.Definition.DisplayName.ToLowerInvariant(),
                        "non-null icons and complete names/descriptions",
                        string.Join("|", row.Choices.Select(value =>
                            value.Name + "=" + value.MetadataComplete)
                            .ToArray()),
                        row.Choices.All(value => value.MetadataComplete) &&
                            graph.Selection.Icon != null &&
                            !string.IsNullOrWhiteSpace(graph.Selection.Name) &&
                            !string.IsNullOrWhiteSpace(
                                graph.Selection.Description),
                        "live localized blueprint presentation");
                }

                evidence.HeritageIdentityCount = owned.Count;
                evidence.RegisteredIdentityCount = owned.Count(value =>
                {
                    BlueprintScriptableObject registered;
                    return !string.IsNullOrWhiteSpace(value.AssetGuid) &&
                        library.BlueprintsByAssetId.TryGetValue(
                            value.AssetGuid, out registered) &&
                        ReferenceEquals(registered, value);
                });
                bool unique = owned.Select(value => value.AssetGuid)
                    .Distinct(StringComparer.Ordinal).Count() == owned.Count;
                Add(assertions, "elemental-heritage-stable-registration",
                    "53 unique exact-reference live identities", "owned=" +
                        owned.Count + ";registered=" +
                        evidence.RegisteredIdentityCount,
                    owned.Count ==
                        ElementalRaceIdentityCatalog.HeritageIdentityCount &&
                        evidence.RegisteredIdentityCount == owned.Count &&
                        unique,
                    "live BlueprintsByAssetId exact-reference lookup");
                Add(assertions, "elemental-heritage-no-top-level-duplication",
                    "four existing parent races exactly once",
                    string.Join("|", evidence.Races.Select(value =>
                        value.Race + "=" + value.TopLevelOccurrences)
                        .ToArray()),
                    evidence.Races.Count == 4 && evidence.Races.All(
                        value => value.TopLevelOccurrences == 1),
                    "live shared race selector");
            }
            catch (Exception exception)
            {
                exceptionSummary = exception.ToString();
                diagnostics.Add(exceptionSummary);
            }

            Add(assertions, "elemental-heritage-blueprints-save-free",
                "false", evidence.SaveStateTouched.ToString(),
                !evidence.SaveStateTouched,
                "read-only blueprint and selector observation");
            string path = Path.Combine(request.EvidenceDirectory,
                EvidenceFileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver(),
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Error
                }));
            evidenceFiles.Add(path);
            diagnostics.Add("elementalHeritageBlueprintSha256=" + Hash(path));
            bool pass = string.IsNullOrEmpty(exceptionSummary) &&
                assertions.All(value => value.Status ==
                    RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" +
                    assembly.ManifestModule.ModuleVersionId + ";sha256=" +
                    Hash(assembly.Location) + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = Metadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = started.ToString("o"),
                EndUtc = string.Empty,
                DurationMilliseconds = (long)(DateTime.UtcNow - started)
                    .TotalMilliseconds,
                Assertions = assertions,
                Diagnostics = diagnostics,
                Warnings = new List<string>(),
                ExceptionSummary = exceptionSummary,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static string Metadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false)
                .OfType<AssemblyMetadataAttribute>().SingleOrDefault(
                    item => string.Equals(item.Key, key,
                        StringComparison.Ordinal));
            return value == null ? string.Empty : value.Value;
        }

        private static string Hash(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static void Add(ICollection<RuntimeTestAssertion> assertions,
            string name, string expected, string observed, bool passed,
            string source)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = name,
                Expected = expected,
                Observed = observed,
                Status = passed ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                Evidence = source
            });
        }
    }
}
