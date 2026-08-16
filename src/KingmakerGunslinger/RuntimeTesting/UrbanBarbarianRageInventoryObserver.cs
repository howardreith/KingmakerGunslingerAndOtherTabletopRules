using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.BrownFur;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class UrbanBarbarianRageInventoryObserver
    {
        internal const string EvidenceFileName =
            "urban-barbarian-rage-inventory.json";
        private const string BarbarianClassGuid =
            "f7d7eb166b3dd594fb330d085df41853";

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            List<BlueprintScriptableObject> all = library == null ?
                new List<BlueprintScriptableObject>() : library.GetAllBlueprints()
                    .OfType<BlueprintScriptableObject>().Where(value => value != null)
                    .ToList();
            BlueprintCharacterClass[] barbarianMatches = all
                .OfType<BlueprintCharacterClass>()
                .Where(value => string.Equals(value.AssetGuid, BarbarianClassGuid,
                    StringComparison.Ordinal))
                .Distinct().ToArray();
            BlueprintCharacterClass barbarian = barbarianMatches.Length == 1 ?
                barbarianMatches[0] : null;
            BlueprintProgression progression = barbarian == null ? null :
                barbarian.Progression;

            var selected = new Dictionary<string, BlueprintScriptableObject>(
                StringComparer.Ordinal);
            Add(selected, barbarian);
            Add(selected, progression);
            if (progression != null && progression.LevelEntries != null)
            {
                foreach (LevelEntry entry in progression.LevelEntries)
                    foreach (BlueprintFeatureBase feature in entry.Features ??
                        new List<BlueprintFeatureBase>())
                        Add(selected, feature);
            }
            foreach (BlueprintScriptableObject blueprint in all)
                if (NameLooksRelevant(blueprint) ||
                    ComponentsLookRelevant(blueprint)) Add(selected, blueprint);

            ExpandForward(selected, 2);
            ExpandReverse(all, selected, 1);
            ExpandForward(selected, 1);

            List<UrbanRageBlueprintRecord> records = selected.Values
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal)
                .Select(Record).ToList();
            List<UrbanRageProgressionEntryRecord> levels = ProgressionRecords(
                progression);
            Assembly cotw = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
                value => string.Equals(value.GetName().Name, "CallOfTheWild",
                    StringComparison.Ordinal));
            CotwArcanistResolution resolution =
                BrownFurOptionalExtensionCoordinator.Current;
            CotwCompatibilityFingerprint fingerprint = resolution == null ||
                resolution.Contract == null ? null :
                resolution.Contract.Fingerprint;
            var evidence = new UrbanRageInventoryEvidence
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Profile = cotw == null ? "cotw-absent" : "cotw-present",
                GameVersion = Application.version ?? string.Empty,
                BarbarianClassGuid = barbarian == null ? string.Empty :
                    barbarian.AssetGuid,
                BarbarianProgressionGuid = progression == null ? string.Empty :
                    progression.AssetGuid,
                BarbarianClassSkills = barbarian == null ||
                    barbarian.ClassSkills == null ? new List<string>() :
                    barbarian.ClassSkills.Select(value => value.ToString()).ToList(),
                BarbarianArchetypes = barbarian == null ||
                    barbarian.Archetypes == null ? new List<string>() :
                    barbarian.Archetypes.Select(BlueprintIdentity).ToList(),
                ProgressionEntries = levels,
                RelatedBlueprints = records,
                CotwAssembly = AssemblyIdentity(cotw),
                CotwModVersion = fingerprint == null ? string.Empty :
                    fingerprint.ModVersion,
                CotwSettingsSha256 = fingerprint == null ? string.Empty :
                    fingerprint.SettingsSha256,
                CotwFingerprint = fingerprint == null ? string.Empty :
                    fingerprint.ToString()
            };
            string evidencePath = Path.Combine(request.EvidenceDirectory,
                EvidenceFileName);
            RuntimeTestResultWriter.WriteAtomic(evidencePath,
                JsonConvert.SerializeObject(evidence, Formatting.Indented) +
                Environment.NewLine);

            AddAssertion(assertions, "native-barbarian-class",
                "exactly one f7d7eb166b3dd594fb330d085df41853",
                barbarianMatches.Length + ":" +
                    (barbarian == null ? "missing" : BlueprintIdentity(barbarian)),
                barbarianMatches.Length == 1 && barbarian != null &&
                    string.Equals(barbarian.AssetGuid, BarbarianClassGuid,
                        StringComparison.Ordinal),
                "live finalized BlueprintLibrary class graph");
            AddAssertion(assertions, "native-barbarian-progression",
                "one nonempty progression", progression == null ? "missing" :
                    BlueprintIdentity(progression) + ";levels=" + levels.Count,
                progression != null && levels.Count > 0,
                "native Barbarian.Progression and final LevelEntries");
            int rageNamed = records.Count(value =>
                Contains(value.Name, "rage") || Contains(value.DisplayName, "rage"));
            AddAssertion(assertions, "rage-candidate-inventory", ">=8",
                "records=" + records.Count + ";rageNamed=" + rageNamed,
                records.Count >= 8 && rageNamed >= 8,
                "bounded forward and typed reverse final-graph traversal plus semantic candidate scan");
            AddAssertion(assertions, "profile-identity",
                cotw == null ? "CotW absent" : "CotW assembly and fingerprint",
                evidence.Profile + ";" + evidence.CotwAssembly + ";" +
                    evidence.CotwFingerprint,
                cotw == null || (!string.IsNullOrWhiteSpace(evidence.CotwAssembly) &&
                    fingerprint != null),
                "current AppDomain plus isolated Brown-Fur structural fingerprint");
            AddAssertion(assertions, "inventory-artifact", "atomic JSON evidence",
                evidencePath, File.Exists(evidencePath),
                "project-owned atomic writer beneath guarded evidence directory");
            AddAssertion(assertions, "save-free-observer",
                "no save, load, input, or blueprint mutation",
                "read-only final BlueprintLibrary and assembly inspection", true,
                "observer performs no publication, registration, selection, or save action");

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
                RuntimeIdentity = AssemblyIdentity(assembly) + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = Metadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = DateTime.UtcNow.ToString("o"),
                EndUtc = string.Empty,
                Assertions = assertions,
                Diagnostics = new List<string>
                {
                    "profile=" + evidence.Profile,
                    "barbarian=" + evidence.BarbarianClassGuid,
                    "progression=" + evidence.BarbarianProgressionGuid,
                    "records=" + records.Count,
                    "artifact=" + evidencePath
                },
                Warnings = new List<string>(),
                ExceptionSummary = string.Empty,
                EvidenceFiles = new List<string> { evidencePath },
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static void ExpandForward(
            Dictionary<string, BlueprintScriptableObject> selected, int passes)
        {
            for (int pass = 0; pass < passes; pass++)
            {
                BlueprintScriptableObject[] snapshot = selected.Values.ToArray();
                int before = selected.Count;
                foreach (BlueprintScriptableObject blueprint in snapshot)
                    foreach (BlueprintScriptableObject reference in
                        BlueprintReferences(blueprint)) Add(selected, reference);
                if (selected.Count == before) break;
            }
        }

        private static void ExpandReverse(List<BlueprintScriptableObject> all,
            Dictionary<string, BlueprintScriptableObject> selected, int passes)
        {
            for (int pass = 0; pass < passes; pass++)
            {
                var targetGuids = new HashSet<string>(selected.Keys,
                    StringComparer.Ordinal);
                int before = selected.Count;
                foreach (BlueprintScriptableObject blueprint in all)
                {
                    if (selected.ContainsKey(blueprint.AssetGuid) ||
                        !CanContainRageContract(blueprint)) continue;
                    if (BlueprintReferences(blueprint).Any(value => value != null &&
                        targetGuids.Contains(value.AssetGuid))) Add(selected, blueprint);
                }
                if (selected.Count == before) break;
            }
        }

        private static IEnumerable<BlueprintScriptableObject> BlueprintReferences(
            BlueprintScriptableObject blueprint)
        {
            if (blueprint == null) yield break;
            foreach (FieldInfo field in Fields(blueprint.GetType()))
            {
                object value;
                try { value = field.GetValue(blueprint); }
                catch { continue; }
                foreach (BlueprintScriptableObject found in ExtractBlueprints(value))
                    yield return found;
            }
            foreach (BlueprintComponent component in blueprint.ComponentsArray ??
                Array.Empty<BlueprintComponent>())
            {
                if (component == null) continue;
                foreach (FieldInfo field in Fields(component.GetType()))
                {
                    object value;
                    try { value = field.GetValue(component); }
                    catch { continue; }
                    foreach (BlueprintScriptableObject found in ExtractBlueprints(value))
                        yield return found;
                }
            }
        }

        private static IEnumerable<BlueprintScriptableObject> ExtractBlueprints(
            object value)
        {
            BlueprintScriptableObject blueprint = value as BlueprintScriptableObject;
            if (blueprint != null)
            {
                yield return blueprint;
                yield break;
            }
            if (value == null || value is string) yield break;
            IEnumerable sequence = value as IEnumerable;
            if (sequence == null) yield break;
            int count = 0;
            foreach (object item in sequence)
            {
                if (++count > 256) yield break;
                BlueprintScriptableObject itemBlueprint =
                    item as BlueprintScriptableObject;
                if (itemBlueprint != null) yield return itemBlueprint;
            }
        }

        private static UrbanRageBlueprintRecord Record(
            BlueprintScriptableObject blueprint)
        {
            return new UrbanRageBlueprintRecord
            {
                Guid = blueprint.AssetGuid,
                Name = blueprint.name ?? string.Empty,
                DisplayName = DisplayName(blueprint),
                Type = blueprint.GetType().FullName,
                Components = (blueprint.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).Select((component, index) =>
                        ComponentRecord(component, index)).ToList(),
                DirectReferences = BlueprintReferences(blueprint)
                    .Where(value => value != null)
                    .Select(BlueprintIdentity).Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal).ToList()
            };
        }

        private static UrbanRageComponentRecord ComponentRecord(
            BlueprintComponent component, int index)
        {
            var fields = new SortedDictionary<string, string>(
                StringComparer.Ordinal);
            if (component != null)
            {
                foreach (FieldInfo field in Fields(component.GetType()))
                {
                    try { fields[field.DeclaringType.FullName + "." + field.Name] =
                        Render(field.GetValue(component)); }
                    catch (Exception exception) { fields[field.Name] =
                        "<unreadable:" + exception.GetType().Name + ">"; }
                }
            }
            return new UrbanRageComponentRecord
            {
                Index = index,
                Type = component == null ? "<null>" :
                    component.GetType().FullName,
                Assembly = component == null ? string.Empty :
                    component.GetType().Assembly.GetName().Name,
                Fields = fields
            };
        }

        private static List<UrbanRageProgressionEntryRecord> ProgressionRecords(
            BlueprintProgression progression)
        {
            var result = new List<UrbanRageProgressionEntryRecord>();
            if (progression == null || progression.LevelEntries == null) return result;
            foreach (LevelEntry entry in progression.LevelEntries)
                result.Add(new UrbanRageProgressionEntryRecord
                {
                    Level = entry.Level,
                    Features = (entry.Features ?? new List<BlueprintFeatureBase>())
                        .Where(value => value != null).Select(BlueprintIdentity)
                        .ToList()
                });
            return result;
        }

        private static IEnumerable<FieldInfo> Fields(Type type)
        {
            var result = new List<FieldInfo>();
            for (Type current = type; current != null && current != typeof(object);
                current = current.BaseType)
                result.AddRange(current.GetFields(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly).Where(field => !field.IsStatic));
            return result.OrderBy(field => field.DeclaringType.FullName,
                StringComparer.Ordinal).ThenBy(field => field.Name,
                StringComparer.Ordinal);
        }

        private static string Render(object value)
        {
            if (value == null) return "<null>";
            BlueprintScriptableObject blueprint = value as BlueprintScriptableObject;
            if (blueprint != null) return BlueprintIdentity(blueprint);
            if (value is string || value.GetType().IsPrimitive ||
                value.GetType().IsEnum || value is decimal)
                return Convert.ToString(value,
                    System.Globalization.CultureInfo.InvariantCulture);
            IEnumerable sequence = value as IEnumerable;
            if (sequence != null)
            {
                var items = new List<string>();
                foreach (object item in sequence)
                {
                    if (items.Count == 128) { items.Add("<truncated>"); break; }
                    items.Add(item is BlueprintScriptableObject ?
                        BlueprintIdentity((BlueprintScriptableObject)item) :
                        (item == null ? "<null>" : item.ToString()));
                }
                return "[" + string.Join("|", items.ToArray()) + "]";
            }
            return value.ToString();
        }

        private static bool NameLooksRelevant(BlueprintScriptableObject blueprint)
        {
            string text = (blueprint.name ?? string.Empty) + " " +
                DisplayName(blueprint) + " " + blueprint.GetType().Name;
            return Contains(text, "barbarian") || Contains(text, "rage") ||
                Contains(text, "fast movement") || Contains(text, "fastmovement") ||
                Contains(text, "tireless") || Contains(text, "mighty rage") ||
                Contains(text, "greater rage");
        }

        private static bool ComponentsLookRelevant(BlueprintScriptableObject blueprint)
        {
            return (blueprint.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Any(component => component != null &&
                    (Contains(component.GetType().FullName, "rage") ||
                     Contains(component.GetType().FullName, "barbarian")));
        }

        private static bool CanContainRageContract(
            BlueprintScriptableObject blueprint)
        {
            string type = blueprint.GetType().FullName ?? string.Empty;
            return Contains(type, "BlueprintFeature") ||
                Contains(type, "BlueprintBuff") ||
                Contains(type, "BlueprintAbility") ||
                Contains(type, "BlueprintActivatableAbility") ||
                Contains(type, "BlueprintItem") ||
                Contains(type, "BlueprintProgression") ||
                Contains(type, "BlueprintCharacterClass") ||
                Contains(type, "BlueprintArchetype");
        }

        private static string DisplayName(BlueprintScriptableObject blueprint)
        {
            if (blueprint == null) return string.Empty;
            PropertyInfo property = blueprint.GetType().GetProperty("Name",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null || property.GetIndexParameters().Length != 0)
                return string.Empty;
            try { return Convert.ToString(property.GetValue(blueprint, null)) ??
                string.Empty; }
            catch { return string.Empty; }
        }

        private static string BlueprintIdentity(BlueprintScriptableObject value)
        {
            return value == null ? "<null>" : value.GetType().FullName + ":" +
                (value.name ?? string.Empty) + ":" + value.AssetGuid + ":" +
                DisplayName(value);
        }

        private static string AssemblyIdentity(Assembly assembly)
        {
            if (assembly == null) return string.Empty;
            string hash = string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(assembly.Location) &&
                    File.Exists(assembly.Location)) hash = Hash(assembly.Location);
            }
            catch { hash = "<unreadable>"; }
            return assembly.FullName + ";mvid=" +
                assembly.ManifestModule.ModuleVersionId + ";sha256=" + hash;
        }

        private static void Add(
            Dictionary<string, BlueprintScriptableObject> selected,
            BlueprintScriptableObject blueprint)
        {
            if (blueprint != null && !string.IsNullOrWhiteSpace(blueprint.AssetGuid))
                selected[blueprint.AssetGuid] = blueprint;
        }

        private static bool ExactText(string value, string expected)
        { return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase); }

        private static bool Contains(string value, string token)
        { return value != null && value.IndexOf(token,
            StringComparison.OrdinalIgnoreCase) >= 0; }

        private static void AddAssertion(List<RuntimeTestAssertion> assertions,
            string name, string expected, string observed, bool pass,
            string evidence)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = name,
                Expected = expected,
                Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = evidence
            });
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

    internal sealed class UrbanRageInventoryEvidence
    {
        [JsonProperty("schemaVersion", Order = 1)] public int SchemaVersion { get; set; }
        [JsonProperty("runId", Order = 2)] public string RunId { get; set; }
        [JsonProperty("profile", Order = 3)] public string Profile { get; set; }
        [JsonProperty("gameVersion", Order = 4)] public string GameVersion { get; set; }
        [JsonProperty("barbarianClassGuid", Order = 5)] public string BarbarianClassGuid { get; set; }
        [JsonProperty("barbarianProgressionGuid", Order = 6)] public string BarbarianProgressionGuid { get; set; }
        [JsonProperty("barbarianClassSkills", Order = 7)] public List<string> BarbarianClassSkills { get; set; }
        [JsonProperty("barbarianArchetypes", Order = 8)] public List<string> BarbarianArchetypes { get; set; }
        [JsonProperty("progressionEntries", Order = 9)] public List<UrbanRageProgressionEntryRecord> ProgressionEntries { get; set; }
        [JsonProperty("relatedBlueprints", Order = 10)] public List<UrbanRageBlueprintRecord> RelatedBlueprints { get; set; }
        [JsonProperty("cotwAssembly", Order = 11)] public string CotwAssembly { get; set; }
        [JsonProperty("cotwModVersion", Order = 12)] public string CotwModVersion { get; set; }
        [JsonProperty("cotwSettingsSha256", Order = 13)] public string CotwSettingsSha256 { get; set; }
        [JsonProperty("cotwFingerprint", Order = 14)] public string CotwFingerprint { get; set; }
    }

    internal sealed class UrbanRageProgressionEntryRecord
    {
        [JsonProperty("level", Order = 1)] public int Level { get; set; }
        [JsonProperty("features", Order = 2)] public List<string> Features { get; set; }
    }

    internal sealed class UrbanRageBlueprintRecord
    {
        [JsonProperty("guid", Order = 1)] public string Guid { get; set; }
        [JsonProperty("name", Order = 2)] public string Name { get; set; }
        [JsonProperty("displayName", Order = 3)] public string DisplayName { get; set; }
        [JsonProperty("type", Order = 4)] public string Type { get; set; }
        [JsonProperty("components", Order = 5)] public List<UrbanRageComponentRecord> Components { get; set; }
        [JsonProperty("directReferences", Order = 6)] public List<string> DirectReferences { get; set; }
    }

    internal sealed class UrbanRageComponentRecord
    {
        [JsonProperty("index", Order = 1)] public int Index { get; set; }
        [JsonProperty("type", Order = 2)] public string Type { get; set; }
        [JsonProperty("assembly", Order = 3)] public string Assembly { get; set; }
        [JsonProperty("fields", Order = 4)] public SortedDictionary<string, string> Fields { get; set; }
    }
}
