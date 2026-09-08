using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>Audit the names that native Fact.PreSave/PostLoad actually use.
    /// Reads exact manifest-owned objects; never repairs a live blueprint.</summary>
    internal static class ElementalComponentIdentityScenario
    {
        internal static void Exercise(ModContext context, RuntimeTestRequest request,
            ICollection<RuntimeTestAssertion> assertions, ICollection<string> files)
        {
            BlueprintManifest manifest = BlueprintManifest.Load(context.ModEntry.Path);
            var symbols = new HashSet<string>(ElementalRaceIdentityCatalog.Symbols(),
                StringComparer.Ordinal);
            JObject document = JObject.Parse(File.ReadAllText(manifest.FilePath));
            var owned = new List<BlueprintScriptableObject>();
            foreach (JToken entry in document["entries"].Where(value =>
                symbols.Contains((string)value["symbol"])))
            {
                BlueprintScriptableObject blueprint;
                string guid = (string)entry["guid"];
                if (!BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(guid,
                        out blueprint) || blueprint == null || blueprint.AssetGuid != guid)
                    throw new InvalidOperationException("Missing exact Elemental identity: " + guid);
                manifest.ResolveActive((string)entry["symbol"], blueprint.GetType());
                owned.Add(blueprint);
            }
            if (owned.Count != symbols.Count)
                throw new InvalidOperationException("Incomplete Elemental component identity inventory.");
            var rows = new JArray();
            var foreignComponents = new HashSet<BlueprintComponent>(BlueprintBootstrap.Library
                .BlueprintsByAssetId.Values.Where(value => value != null && !owned.Contains(value))
                .SelectMany(value => value.ComponentsArray ?? new BlueprintComponent[0]));
            foreach (BlueprintScriptableObject blueprint in owned)
            {
                GameLogicComponent[] components = (blueprint.ComponentsArray ??
                    new BlueprintComponent[0]).OfType<GameLogicComponent>().ToArray();
                if (components.Length == 0) continue;
                bool unique = components.Select(value => value.name)
                    .Distinct(StringComparer.Ordinal).Count() == components.Length;
                bool exclusive = components.All(value => !foreignComponents.Contains(value) &&
                    owned.Count(owner => (owner.ComponentsArray ?? new BlueprintComponent[0])
                        .Any(candidate => ReferenceEquals(candidate, value))) == 1);
                var row = new JObject {
                    { "guid", blueprint.AssetGuid }, { "blueprint", blueprint.name },
                    { "uniqueNativeSaveNames", unique }, { "exclusiveOwnership", exclusive },
                    { "components", new JArray(components.Select(value => new JObject {
                        { "type", value.GetType().FullName }, { "name", value.name },
                        { "savedFields", new JArray(SavedFields(value.GetType())) }
                    })) }
                };
                rows.Add(row);
                assertions.Add(new RuntimeTestAssertion {
                    Name = "elemental-component-identities-" + blueprint.AssetGuid,
                    Expected = "one native save-name match; exclusively owned components",
                    Observed = row.ToString(Formatting.None),
                    Status = unique && exclusive ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                    Evidence = "exact manifest identity and live native GameLogicComponent names"
                });
            }
            string path = Path.Combine(request.EvidenceDirectory, "elemental-component-identities.json");
            File.WriteAllText(path, new JObject {
                { "schemaVersion", 1 }, { "saveStateTouched", false },
                { "ownedBlueprints", owned.Count }, { "observations", rows }
            }.ToString(Formatting.Indented));
            files.Add(path);
        }

        private static IEnumerable<string> SavedFields(Type type)
        {
            for (Type current = type; current != null; current = current.BaseType)
                foreach (FieldInfo field in current.GetFields(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    if (field.GetCustomAttributes(typeof(JsonPropertyAttribute), false).Any())
                        yield return current.FullName + "." + field.Name;
        }
    }
}
