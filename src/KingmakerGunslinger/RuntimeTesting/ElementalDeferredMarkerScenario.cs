using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Read-only native hydration observation for one exact guarded save case.
    // Serialized legacy component data is inspected before native PostLoad;
    // neither its fields nor the engine's result are changed.
    internal static class ElementalDeferredMarkerObservation
    {
        private static readonly Dictionary<Fact, JObject> Rows = new Dictionary<Fact, JObject>();
        internal static bool Active { get; private set; }
        internal static void Activate() { Active = true; }
        internal static JArray Snapshot() { return new JArray(Rows.Values.Select(value => value.DeepClone())); }
        internal static void Observe(Fact fact)
        {
            if (!Active || !(fact is Feature) || fact.Blueprint == null || Rows.ContainsKey(fact)) return;
            string guid = fact.Blueprint.AssetGuid;
            if (guid != "e117e1e0a17a4acec001000000000031" && guid != "e117e1e0a17a4acec001000000000040") return;
            var row = new JObject { ["marker"] = guid, ["ownerId"] = ((Feature)fact).Owner?.Unit?.UniqueId };
            Rows.Add(fact, row);
            try {
                var saved = typeof(Fact).GetField("m_ComponentsData", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(fact) as IEnumerable;
                var entries = saved?.Cast<object>().ToArray() ?? new object[0];
                var data = new JArray();
                foreach (var entry in entries) {
                    var payload = entry.GetType().GetField("Data").GetValue(entry);
                    data.Add(new JObject { ["name"] = (string)entry.GetType().GetField("ComponentName")?.GetValue(entry),
                        ["dataMissing"] = payload == null, ["dataCount"] = (payload as IDictionary)?.Count ?? 0 });
                }
                row["components"] = data;
                row["exactLegacyPayload"] = data.Count == 1 && (string)data[0]["name"] == string.Empty &&
                    data[0].Value<bool>("dataMissing");
            } catch (Exception exception) {
                row["exactLegacyPayload"] = false; row["observationException"] = exception.ToString();
            }
        }
    }

    internal static partial class GunslingerOutfitRenderScenario
    {
        internal static RuntimeTestResult VerifyElementalDeferredMarkers(ModContext context, RuntimeTestRequest request,
            WorkingSaveSmokeEvidence loaded)
        {
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var rows = new JArray();
            var files = new List<string>();
            string failure = string.Empty;
            string fixtureCase = (string)request.Parameters?["fixtureCase"];
            bool authored = fixtureCase == "deferred117";
            try {
                if (request.Scenario != RuntimeTestScenarioCatalog.WorkingSaveElementalDeferredMarkers ||
                    (fixtureCase != "public117" && !authored) || !request.ExitAfterCompletion ||
                    !ElementalDeferredMarkerObservation.Active || !ElementalAlternateTraitPolicy.NereidQualificationActive)
                    throw new InvalidOperationException("The exact guarded public117/deferred117 marker case is required.");
                bool loadExact = loaded != null && loaded.CompletionCallbackObserved && loaded.DescriptorReferenceCorrelated &&
                    !string.IsNullOrEmpty(loaded.StableFingerprint) && !loaded.SaveWritingApiObserved && loaded.HooksRemoved;
                Add(assertions, "deferred-marker-correlated-read-only-load", "exact guarded working-save load and zero save writes",
                    "exact=" + loadExact, loadExact, "ordinary native load before this synchronous read-only probe");
                if (!loadExact) throw new InvalidOperationException("The native working-save boundary was not exact.");
                var set = BlueprintBootstrap.ElementalRaces;
                var races = set.OrderedBlueprints().ToArray();
                var all = Game.Instance.State.Units.All.ToArray();
                var party = Game.Instance.Player.Party.ToArray();
                bool fixtureIdentities = ElementalPersistenceFixtureIds.All(id =>
                    all.Count(unit => unit.UniqueId == id) == 1 && party.Count(unit => unit.UniqueId == id) == 1);
                bool catalog = races.Length == 4 && races.Sum(race => race.Heritages.Choices().Count()) == 12 &&
                    BlueprintBootstrap.ElementalFeats.AllFeats().Length == 11 && fixtureIdentities && party.Length == 27;
                Add(assertions, "deferred-marker-native-fixture-identities", "24 exact native saved actors; four races, twelve heritages and eleven feats",
                    "exact=" + catalog, catalog, "stable qualified persistence IDs and actual Player.Party");
                if (!catalog) throw new InvalidOperationException("Public117 fixture identities are missing or duplicated.");
                for (int heritageIndex = 0; heritageIndex < 3; heritageIndex++)
                for (int raceIndex = 0; raceIndex < races.Length; raceIndex++)
                for (int sex = 0; sex < 2; sex++) {
                    var race = races[raceIndex];
                    if (race.AlternateTraits.Race != ElementalHeritageRace.Oread && race.AlternateTraits.Race != ElementalHeritageRace.Undine) continue;
                    int index = heritageIndex * 8 + raceIndex * 2 + sex;
                    var unit = all.Single(value => value.UniqueId == ElementalPersistenceFixtureIds[index]);
                    var owner = unit.Descriptor;
                    var heritage = race.Heritages.Choices().ElementAt(heritageIndex);
                    string label = race.Definition.Kind.ToString().ToLowerInvariant() + "-" + (sex == 0 ? "male" : "female");
                    if (!heritage.Definition.IsGeneral) label += "-" + heritage.Definition.Id.ToString().ToLowerInvariant();
                    var desired = ElementalVisibleTraitPersistencePolicy.Traits(race.AlternateTraits.Race, sex, heritageIndex)
                        .Select(race.AlternateTraits.Require).ToArray();
                    var replaced = desired.Aggregate(ElementalRacialTraitSlot.None, (mask, trait) => mask | trait.Definition.ReplacedSlots);
                    var facts = new JArray(); var resources = new JArray(); var abilities = new JArray();
                    bool exact = owner.CustomName == ElementalPersistenceFixtureNamePrefix + label.Replace('-', '_').ToUpperInvariant() &&
                        ReferenceEquals(owner.Progression.Race, race.Race) && owner.Progression.CharacterLevel == 1 &&
                        owner.Gender == (sex == 0 ? Gender.Male : Gender.Female);
                    Action<Kingmaker.Blueprints.Classes.BlueprintFeature, int> fact = (blueprint, expected) => {
                        int count = owner.Progression.Features.Enumerable.Count(value => ReferenceEquals(value.Blueprint, blueprint));
                        int rank = owner.Progression.Features.GetRank(blueprint);
                        exact &= count == expected && rank == expected;
                        facts.Add(new JObject { ["guid"] = blueprint.AssetGuid, ["expected"] = expected, ["count"] = count, ["rank"] = rank });
                    };
                    Action<BlueprintAbilityResource, int> resource = (blueprint, expected) => {
                        int count = owner.Resources.PersistantResources.Count(value => ReferenceEquals(value.Blueprint, blueprint));
                        int amount = owner.Resources.GetResourceAmount(blueprint);
                        exact &= count == expected && amount == 0;
                        resources.Add(new JObject { ["guid"] = blueprint.AssetGuid, ["expectedCount"] = expected, ["count"] = count, ["amount"] = amount });
                    };
                    Action<BlueprintAbility, int> ability = (blueprint, expected) => {
                        int count = owner.Abilities.Enumerable.Count(value => ReferenceEquals(value.Blueprint, blueprint));
                        exact &= count == expected;
                        abilities.Add(new JObject { ["guid"] = blueprint.AssetGuid, ["expected"] = expected, ["count"] = count });
                    };
                    fact(race.Resistance, (replaced & ElementalRacialTraitSlot.EnergyResistance) == 0 ? 1 : 0);
                    foreach (var choice in race.Heritages.Choices()) {
                        bool current = ReferenceEquals(choice, heritage);
                        fact(choice.Marker, current ? 1 : 0);
                        fact(choice.Affinity, current && (replaced & ElementalRacialTraitSlot.ElementalAffinity) == 0 ? 1 : 0);
                        int sla = current && (replaced & ElementalRacialTraitSlot.RacialSpellLikeAbility) == 0 ? 1 : 0;
                        fact(choice.SlaFeature, sla); resource(choice.SlaResource, sla); ability(choice.SlaAbility, sla);
                    }
                    foreach (var trait in race.AlternateTraits.Traits()) {
                        int active = desired.Contains(trait) ? 1 : 0;
                        fact(trait.Marker, active); fact(trait.Provider, active);
                        if (trait.Definition.Replaces(ElementalRacialTraitSlot.RacialSpellLikeAbility)) {
                            foreach (var item in trait.Mechanics().OfType<BlueprintAbilityResource>()) resource(item, active);
                            foreach (var item in trait.Mechanics().OfType<BlueprintAbility>()) ability(item,
                                item.Parent == null && item.AssetGuid != ElementalNereidFactory.ShakeFreeGuid ? active : 0);
                        }

                    }
                    foreach (var selection in race.AlternateTraits.Selections())
                        fact(selection.RetainMarker, desired.Any(trait => trait.Definition.PrimarySlot == selection.Definition.Slot) ? 0 : 1);
                    var row = new JObject { ["fixture"] = label, ["id"] = unit.UniqueId, ["race"] = race.Definition.Kind.ToString(),
                        ["heritage"] = heritage.Definition.Id.ToString(), ["sex"] = owner.Gender.ToString(),
                        ["retainedPublishedTraits"] = new JArray(desired.Select(value => value.Definition.Id.ToString())),
                        ["facts"] = facts, ["resources"] = resources, ["abilities"] = abilities, ["exact"] = exact };
                    rows.Add(row);
                    Add(assertions, "deferred-marker-preserved-" + label, "original published graph and spent SLA preserved; no new provider/resource",
                        "exact=" + exact, exact, "actual native saved actor after automatic reconstruction; no direct reconciliation");
                }
                var observed = ElementalDeferredMarkerObservation.Snapshot().OfType<JObject>().ToArray();
                bool payloads = authored ? observed.Length == 12 && observed.All(row => row.Value<bool>("exactLegacyPayload") &&
                    rows.Any(actor => actor.Value<string>("id") == row.Value<string>("ownerId"))) &&
                    observed.Count(row => row.Value<string>("marker") == "e117e1e0a17a4acec001000000000040") == 6 &&
                    observed.Count(row => row.Value<string>("marker") == "e117e1e0a17a4acec001000000000031") == 6
                    : observed.Length == 0;
                Add(assertions, "deferred-marker-native-legacy-payload", authored ? "twelve native empty-name component records without revision data" : "ordinary public117 save has no deferred markers",
                    "observed=" + observed.Length + ";exact=" + payloads, payloads, "read-only Fact.PostLoad prefix; source archive provenance is pinned by the driver");
                bool noEffects = !Game.Instance.State.AreaEffects.All.Any(value => value.Blueprint.AssetGuid.StartsWith("e118e1e0a17a4acec001", StringComparison.Ordinal)) &&
                    all.All(unit => !unit.Buffs.Enumerable.Any(value => value.Blueprint.AssetGuid.StartsWith("e118e1e0a17a4acec001", StringComparison.Ordinal)));
                Add(assertions, "deferred-marker-no-new-effects", "no completion-trait effects created during legacy reconstruction",
                    "exact=" + noEffects, noEffects, "native area and buff collections");
            } catch (Exception exception) { failure = exception.ToString(); }
            string path = Path.Combine(request.EvidenceDirectory, "elemental-deferred-marker-transition.json");
            File.WriteAllText(path, new JObject { ["fixtureCase"] = fixtureCase, ["authoredMarkerCopy"] = authored,
                ["qualificationBoundary"] = "native public117 fixture graph preservation and legacy inert-marker transition; no save write or new character selection",
                ["observedLegacyComponents"] = ElementalDeferredMarkerObservation.Snapshot(), ["actors"] = rows,
                ["exception"] = failure }.ToString(Formatting.Indented));
            files.Add(path);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult { SchemaVersion = 1, RunId = request.RunId, Scenario = request.Scenario,
                Status = failure.Length == 0 && assertions.Count == 16 && assertions.All(value => value.Status == RuntimeTestStatuses.Pass)
                    ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" + assembly.ManifestModule.ModuleVersionId + ";pid=" + Process.GetCurrentProcess().Id,
                GitCommit = assembly.GetCustomAttributes(typeof(AssemblyMetadataAttribute), false).OfType<AssemblyMetadataAttribute>()
                    .Single(value => value.Key == "GitCommit").Value,
                GameVersion = UnityEngine.Application.version, StartUtc = started.ToString("o"), EndUtc = string.Empty,
                DurationMilliseconds = (long)(DateTime.UtcNow - started).TotalMilliseconds,
                Assertions = assertions, Diagnostics = new List<string>(), Warnings = new List<string>(), ExceptionSummary = failure,
                WorkingSaveSmoke = loaded, EvidenceFiles = files, EvidenceDirectory = request.EvidenceDirectory, AutomaticExitRequested = request.ExitAfterCompletion };
        }
    }
}
