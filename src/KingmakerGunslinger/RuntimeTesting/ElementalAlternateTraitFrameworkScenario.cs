using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Root;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free Release C observation of the registered replacement graph.
    /// It reads live blueprints and selectors, then exercises replacement on
    /// request-local native units without touching a campaign save.
    /// </summary>
    internal static class ElementalAlternateTraitFrameworkScenario
    {
        internal const string EvidenceFileName =
            "elemental-alternate-trait-framework.json";

        private sealed class TraitEvidence
        {
            public string Name { get; set; }
            public string MarkerGuid { get; set; }
            public string ProviderGuid { get; set; }
            public int ReplacedSlots { get; set; }
            public int ExclusionCount { get; set; }
            public bool MetadataComplete { get; set; }
        }

        private sealed class SelectionEvidence
        {
            public string Slot { get; set; }
            public string SelectionGuid { get; set; }
            public string RetainGuid { get; set; }
            public int ChoiceCount { get; set; }
            public int RaceFeatureOccurrences { get; set; }
            public bool Exact { get; set; }
        }

        private sealed class RaceEvidence
        {
            public string Race { get; set; }
            public string RaceGuid { get; set; }
            public int TopLevelOccurrences { get; set; }
            public List<SelectionEvidence> Selections { get; set; }
            public List<TraitEvidence> Traits { get; set; }
        }

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool ModuleActive { get; set; }
            public bool SaveStateTouched { get; set; }
            public int FrameworkIdentityCount { get; set; }
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
                        "The live alternate-trait blueprint graph is unavailable.");

                BlueprintRace[] topLevel = root.Progression.CharacterRaces;
                var owned = new List<BlueprintScriptableObject>();
                foreach (ElementalRaceBlueprints race in
                    set.OrderedBlueprints())
                {
                    ElementalAlternateTraitRaceBlueprints graph =
                        race.AlternateTraits;
                    ElementalAlternateTraitBlueprints[] traits = graph
                        .Traits().ToArray();
                    ElementalAlternateTraitSelectionBlueprints[] selections =
                        graph.Selections().ToArray();
                    var row = new RaceEvidence
                    {
                        Race = race.Definition.DisplayName,
                        RaceGuid = race.Race.AssetGuid,
                        TopLevelOccurrences = topLevel.Count(value =>
                            ReferenceEquals(value, race.Race)),
                        Selections = new List<SelectionEvidence>(),
                        Traits = new List<TraitEvidence>()
                    };

                    foreach (ElementalAlternateTraitSelectionBlueprints
                        selection in selections)
                    {
                        owned.Add(selection.Selection);
                        owned.Add(selection.RetainMarker);
                        BlueprintFeature[] expected = new[]
                        {
                            selection.RetainMarker
                        }.Concat(selection.Choices.Select(value =>
                            value.Marker)).ToArray();
                        ElementalAlternateTraitRetainController controller =
                            (selection.RetainMarker.ComponentsArray ??
                                new BlueprintComponent[0]).OfType<
                                    ElementalAlternateTraitRetainController>()
                                .SingleOrDefault();
                        var selectionEvidence = new SelectionEvidence
                        {
                            Slot = selection.Definition.Slot.ToString(),
                            SelectionGuid = selection.Selection.AssetGuid,
                            RetainGuid = selection.RetainMarker.AssetGuid,
                            ChoiceCount = expected.Length,
                            RaceFeatureOccurrences = (race.Race.Features ??
                                new BlueprintFeature[0]).Count(value =>
                                    ReferenceEquals(value,
                                        selection.Selection)),
                            Exact = selection.Selection.Obligatory &&
                                !selection.Selection.IgnorePrerequisites &&
                                selection.Selection.Features != null &&
                                selection.Selection.AllFeatures != null &&
                                selection.Selection.Features.SequenceEqual(
                                    expected) &&
                                selection.Selection.AllFeatures.SequenceEqual(
                                    expected) && controller != null &&
                                controller.Race == (int)graph.Race &&
                                controller.Slot ==
                                    (int)selection.Definition.Slot &&
                                selection.Selection.Icon != null &&
                                selection.RetainMarker.Icon != null &&
                                !string.IsNullOrWhiteSpace(
                                    selection.Selection.Name) &&
                                !string.IsNullOrWhiteSpace(
                                    selection.Selection.Description)
                        };
                        row.Selections.Add(selectionEvidence);
                        Add(assertions, "elemental-trait-selection-" +
                            row.Race.ToLowerInvariant() + "-" +
                            selectionEvidence.Slot.ToLowerInvariant(),
                            "one exact obligatory retain-base selector on parent race",
                            selectionEvidence.SelectionGuid + ";choices=" +
                                selectionEvidence.ChoiceCount +
                                ";raceFeature=" +
                                selectionEvidence.RaceFeatureOccurrences,
                            selectionEvidence.Exact &&
                                selectionEvidence.RaceFeatureOccurrences == 1,
                            "live BlueprintFeatureSelection graph");
                    }

                    foreach (ElementalAlternateTraitBlueprints trait in traits)
                    {
                        owned.Add(trait.Marker);
                        owned.Add(trait.Provider);
                        ElementalAlternateTraitMarkerController marker =
                            (trait.Marker.ComponentsArray ??
                                new BlueprintComponent[0]).OfType<
                                    ElementalAlternateTraitMarkerController>()
                                .SingleOrDefault();
                        ElementalAlternateTraitProviderController provider =
                            (trait.Provider.ComponentsArray ??
                                new BlueprintComponent[0]).OfType<
                                    ElementalAlternateTraitProviderController>()
                                .SingleOrDefault();
                        PrerequisiteNoFeature[] exclusions =
                            (trait.Marker.ComponentsArray ??
                                new BlueprintComponent[0]).OfType<
                                    PrerequisiteNoFeature>().ToArray();
                        BlueprintFeature[] expectedExclusions = traits.Where(
                            value => value.Definition.Id !=
                                trait.Definition.Id &&
                                (value.Definition.ReplacedSlots &
                                    trait.Definition.ReplacedSlots) != 0)
                            .Select(value => value.Marker).ToArray();
                        bool exclusionsExact = exclusions.Length ==
                                expectedExclusions.Length &&
                            exclusions.All(value => value.Group ==
                                Prerequisite.GroupType.All &&
                                expectedExclusions.Contains(value.Feature)) &&
                            exclusions.Select(value => value.Feature)
                                .Distinct().Count() == exclusions.Length;
                        var traitEvidence = new TraitEvidence
                        {
                            Name = trait.Definition.Name,
                            MarkerGuid = trait.Marker.AssetGuid,
                            ProviderGuid = trait.Provider.AssetGuid,
                            ReplacedSlots = (int)trait.Definition.ReplacedSlots,
                            ExclusionCount = exclusions.Length,
                            MetadataComplete = trait.Marker.Icon != null &&
                                trait.Provider.Icon != null &&
                                !trait.Marker.HideInUI &&
                                trait.Provider.HideInUI &&
                                !string.IsNullOrWhiteSpace(trait.Marker.Name) &&
                                !string.IsNullOrWhiteSpace(
                                    trait.Marker.Description)
                        };
                        row.Traits.Add(traitEvidence);
                        Add(assertions, "elemental-trait-provider-" +
                            trait.Definition.Id.ToString().ToLowerInvariant(),
                            "distinct marker/provider with exact overlap exclusions",
                            traitEvidence.MarkerGuid + ";" +
                                traitEvidence.ProviderGuid + ";exclusions=" +
                                traitEvidence.ExclusionCount,
                            !ReferenceEquals(trait.Marker, trait.Provider) &&
                                marker != null && provider != null &&
                                marker.Trait == (int)trait.Definition.Id &&
                                provider.Trait == (int)trait.Definition.Id &&
                                exclusionsExact &&
                                traitEvidence.MetadataComplete,
                            "live marker/provider components and prerequisites");
                    }

                    evidence.Races.Add(row);
                    int expectedSelections = ElementalAlternateTraitPolicy
                        .SelectionsForRace(graph.Race).Count;
                    int expectedTraits = ElementalAlternateTraitPolicy
                        .ForRace(graph.Race).Count;
                    int expectedTopLevel = evidence.ModuleActive ? 1 : 0;
                    Add(assertions, "elemental-trait-race-graph-" +
                        row.Race.ToLowerInvariant(),
                        "exact race-local catalog and module-gated parent publication",
                        "traits=" + row.Traits.Count + ";selections=" +
                            row.Selections.Count + ";top=" +
                            row.TopLevelOccurrences,
                        row.Traits.Count == expectedTraits &&
                            row.Selections.Count == expectedSelections &&
                            row.TopLevelOccurrences == expectedTopLevel,
                        "live parent race and policy inventory");
                }

                evidence.FrameworkIdentityCount = owned.Count;
                evidence.RegisteredIdentityCount = owned.Count(value =>
                {
                    BlueprintScriptableObject registered;
                    return !string.IsNullOrWhiteSpace(value.AssetGuid) &&
                        library.BlueprintsByAssetId.TryGetValue(
                            value.AssetGuid, out registered) &&
                        ReferenceEquals(registered, value);
                });
                bool unique = owned.Select(value => value.AssetGuid).Distinct(
                    StringComparer.Ordinal).Count() == owned.Count;
                Add(assertions, "elemental-trait-stable-registration",
                    "62 unique exact-reference identities in every module state",
                    "owned=" + owned.Count + ";registered=" +
                        evidence.RegisteredIdentityCount,
                    owned.Count == ElementalRaceIdentityCatalog
                        .TraitFrameworkIdentityCount &&
                        evidence.RegisteredIdentityCount == owned.Count &&
                        unique,
                    "live BlueprintsByAssetId exact-reference lookup");
                Add(assertions, "elemental-trait-no-top-level-races",
                    "only the four existing parent race blueprints",
                    string.Join("|", evidence.Races.Select(value =>
                        value.Race + "=" + value.TopLevelOccurrences)
                        .ToArray()),
                    evidence.Races.Count == ElementalRaceCatalog.RaceCount &&
                        evidence.Races.All(value =>
                            value.TopLevelOccurrences ==
                                (evidence.ModuleActive ? 1 : 0)),
                    "live BlueprintRoot.Progression.CharacterRaces");
                var persistenceOwned = new List<BlueprintScriptableObject>();
                foreach (ElementalRaceBlueprints race in set.OrderedBlueprints())
                    GunslingerOutfitRenderScenario.ElementalRacePersistenceSession
                        .AddAlternateTraitIdentities(race, persistenceOwned);
                BlueprintScriptableObject[] expectedPersistence = owned.Concat(
                    set.OrderedBlueprints().SelectMany(race => race
                        .AlternateTraits.Traits()).SelectMany(trait =>
                            trait.Mechanics())).ToArray();
                Add(assertions, "elemental-trait-persistence-identity-coverage",
                    "every framework and mechanic identity exactly once",
                    "collected=" + persistenceOwned.Count + ";expected=" +
                        expectedPersistence.Length,
                    expectedPersistence.Length == ElementalRaceIdentityCatalog
                        .TraitFrameworkIdentityCount + ElementalRaceIdentityCatalog
                            .TraitMechanicIdentityCount &&
                        persistenceOwned.Count == expectedPersistence.Length &&
                        persistenceOwned.Distinct().Count() == persistenceOwned.Count &&
                        expectedPersistence.All(value => persistenceOwned.Count(
                            observed => ReferenceEquals(value, observed)) == 1),
                    "actual persistence collector over the live owned blueprint graph");
                // Ray transport qualification requires a pristine controller.
                // Run it before other scenarios create native projectiles.
                ElementalCrystallineFormNativeAuditScenario.Exercise(request, assertions, evidenceFiles);
                ElementalRemainingTraitNativeAuditScenario.Exercise(request, assertions, evidenceFiles);
                ElementalCrystallineFormScenario.Exercise(request, assertions, evidenceFiles);
                ElementalAlternateTraitReconciliationScenario.Exercise(
                    request, assertions, evidenceFiles);
                ElementalAlternateTraitPassiveScenario.Exercise(
                    request, assertions, evidenceFiles);
                ElementalComponentIdentityScenario.Exercise(context, request,
                    assertions, evidenceFiles);
                ElementalSummonInsightScenario.Exercise(
                    request, assertions, evidenceFiles);
                ElementalBloodScenario.Exercise(request, assertions, evidenceFiles);
                ElementalEfreetiMagicScenario.Exercise(request, assertions, evidenceFiles);
            }
            catch (Exception exception)
            {
                exceptionSummary = exception.ToString();
                diagnostics.Add(exceptionSummary);
            }

            Add(assertions, "elemental-trait-framework-save-free", "false",
                evidence.SaveStateTouched.ToString(),
                !evidence.SaveStateTouched,
                "blueprint observation and request-local native units; no save access");
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
            diagnostics.Add("elementalAlternateTraitFrameworkSha256=" +
                Hash(path));
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
